using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Runs one counter-espionage Sweep (<c>gens-espionage-design.md</c> §5) against <see
/// cref="SuspectedActorId"/> — always the investigator's own actor, since that is the only side of a
/// placement a Sweep can meaningfully search (§5: "directly rolls against whatever enemy spy is
/// currently embedded"). Costed and resolved immediately in <see cref="CounterEspionageSweepCommands.Pipeline"/>'s
/// mutate step, rather than as a monthly system: it is an explicit, deliberate player/AI action (§5's
/// "a real Interrogate-adjacent effort, spending time and Influence"), not ambient background drift.</summary>
public sealed record CounterEspionageSweepCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> InvestigatorCharacterId,
    RuntimeId<Actor> SuspectedActorId) : ICommand;

/// <summary>Emitted whenever a <see cref="CounterEspionageSweepCommand"/> is accepted. Private to the
/// investigator only — a caught spy's own sponsor learns of the catch via <see
/// cref="SpyPlacementResolvedEvent"/>, not this event, so a failed or successful Sweep never itself
/// tips off the other side.</summary>
public sealed record CounterEspionageSweepResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> InvestigatorCharacterId,
    RuntimeId<Actor> SuspectedActorId,
    RuntimeId<SpyPlacement>? FoundPlacementId,
    bool SpyIdentified,
    string? CausationId) : IDomainEvent
{
    public string Type => "interactions.counterEspionageSweepResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InvestigatorCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(InvestigatorCharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="CounterEspionageSweepCommand"/> (ADR 0006). Built
/// per <see cref="Random.RandomStreamSet"/>, matching <see cref="Legal.FileLawsuitCommands.CreatePipeline"/>'s
/// identical "the pipeline closes over the RNG stream set a command's own Mutate step needs" shape.</summary>
public static class CounterEspionageSweepCommands
{
    /// <summary>The named random stream this command draws from for its catch roll (Phase 16 item 1),
    /// kept distinct from every other stream for rule 8's "adding a draw in one system must not perturb
    /// another".</summary>
    public const string StreamName = CampaignBootstrapper.CounterEspionageSweepStreamName;

    public static readonly ValidationErrorCode InvestigatorNotFound = new("interactions.counterEspionageSweep.investigatorNotFound");
    public static readonly ValidationErrorCode InvestigatorDeceased = new("interactions.counterEspionageSweep.investigatorDeceased");
    public static readonly ValidationErrorCode SuspectedActorNotFound = new("interactions.counterEspionageSweep.suspectedActorNotFound");
    public static readonly ValidationErrorCode InsufficientFunds = new("interactions.counterEspionageSweep.insufficientFunds");

    public static CommandPipeline<WorldState, CounterEspionageSweepCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, CounterEspionageSweepCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, CounterEspionageSweepCommand command)
    {
        if (!state.Characters.TryGet(command.InvestigatorCharacterId, out var investigator))
            return InvestigatorNotFound;
        if (!state.Actors.TryGet(command.SuspectedActorId, out _))
            return SuspectedActorNotFound;
        if (!investigator.IsAlive)
            return InvestigatorDeceased;

        if (investigator.Household is { } householdId)
        {
            var cost = Money.FromDenarii(SpyPlacementCatalog.SweepCostDenarii);
            var account = LedgerAccountKey.ForHousehold(householdId);
            var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
            if (balance < cost)
                return InsufficientFunds;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CounterEspionageSweepCommand command, RandomStreamSet randomStreams)
    {
        state.Characters.TryGet(command.InvestigatorCharacterId, out var investigator);
        var events = new List<IDomainEvent>();

        if (investigator!.Household is { } householdId)
        {
            var cost = Money.FromDenarii(SpyPlacementCatalog.SweepCostDenarii);
            var account = LedgerAccountKey.ForHousehold(householdId);
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Purchases,
                new[]
                {
                    new LedgerPosting(account, -cost),
                    new LedgerPosting(LedgerAccountKey.Mint, cost),
                },
                reference: $"interactions.counterEspionageSweep:{command.CommandId.ToTaggedString()}"));
        }

        // Deterministic tie-break (ADR 0004): the least-concealed candidate first, then by ID — never
        // an extra RNG draw just to pick which suspected placement the Sweep actually rolls against.
        var candidateEntry = state.SpyPlacements.InAscendingOrder()
            .Where(entry => entry.Value.Status == SpyPlacementStatus.InProgress && entry.Value.TargetActorId == command.SuspectedActorId)
            .OrderBy(entry => entry.Value.ConcealmentQuality)
            .ThenBy(entry => entry.Key.Value)
            .Select(entry => (KeyValuePair<RuntimeId<SpyPlacement>, SpyPlacement>?)entry)
            .FirstOrDefault();

        if (candidateEntry is not { } candidate)
        {
            events.Add(new CounterEspionageSweepResolvedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.InvestigatorCharacterId, command.SuspectedActorId,
                FoundPlacementId: null, SpyIdentified: false, CausationId: command.CommandId.ToTaggedString()));
            return events.ToArray();
        }

        var successChance = Math.Clamp(
            SpyPlacementCatalog.SweepSuccessBaseChancePercent
                - candidate.Value.ConcealmentQuality * SpyPlacementCatalog.SweepSuccessConcealmentWeightPercent / 100,
            0, 100);
        var identified = randomStreams.NextUInt(StreamName, 100) < (uint)successChance;

        if (identified)
        {
            state.SpyPlacements.Remove(candidate.Key);
            state.SpyPlacements.Add(candidate.Key, candidate.Value with { Status = SpyPlacementStatus.DiscoveredAndTraced, LastProgressedDate = command.SubmittedDate });
            events.Add(new SpyPlacementResolvedEvent(
                state.EventIds.Issue(), command.SubmittedDate, candidate.Key, candidate.Value.SpyCharacterId,
                candidate.Value.SponsoringCharacterId, SpyPlacementStatus.DiscoveredAndTraced));
            RivalDossierRefresh.Refresh(
                state, command.SuspectedActorId, command.SubmittedDate,
                "An embedded spy targeting this house was identified and traced back to its sponsor by a counter-espionage Sweep.");
        }

        events.Add(new CounterEspionageSweepResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.InvestigatorCharacterId, command.SuspectedActorId,
            identified ? candidate.Key : null, identified, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
