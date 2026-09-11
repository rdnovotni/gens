using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>Proposes one of the three in-scope Frontier treaties (§6) to a Foreign People. The first
/// real caller of <see cref="DiplomacyLanguageGateEvaluator"/> (Phase 13 item 4's own "no actual
/// Diplomacy negotiation flow to call it from yet" — this is that flow): §5's design doc describes a
/// soft "meaningful penalty" for no qualified speaker, but the already-built gate is hard, and this
/// command honors the code that actually exists — see <see
/// cref="ProposeFrontierTreatyCommands.LanguageGateNotCleared"/>.</summary>
public sealed record ProposeFrontierTreatyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> NegotiatorCharacterId,
    RuntimeId<Actor> ForeignPeopleActorId,
    FrontierTreatyType Type,
    TributeDirection TributeDirection,
    Money MonthlyTribute,
    int TermMonths) : ICommand;

/// <summary>Emitted when a <see cref="ProposeFrontierTreatyCommand"/> succeeds.</summary>
public sealed record FrontierTreatyConcludedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FrontierTreaty> TreatyId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    FrontierTreatyType TreatyType,
    TributeDirection TributeDirection,
    GameDate ExpiresDate,
    FrontierNegotiationQualitySource QualitySource,
    RuntimeId<Character>? InterpresCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "diplomacy.frontierTreatyConcluded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ForeignPeopleActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>Emitted when a <see cref="ProposeFrontierTreatyCommand"/> is accepted but the negotiation
/// itself fails its success roll (§7's "diplomatic failure" framing, applied at proposal time).</summary>
public sealed record FrontierTreatyRejectedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    FrontierTreatyType TreatyType,
    FrontierNegotiationQualitySource QualitySource,
    int SuccessChancePercent,
    string? CausationId) : IDomainEvent
{
    public string Type => "diplomacy.frontierTreatyRejected";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ForeignPeopleActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ProposeFrontierTreatyCommand"/> (ADR 0006). Built
/// per <see cref="CultureLanguageMap"/> and <see cref="RandomStreamSet"/>, matching <see
/// cref="Interactions.CounterEspionageSweepCommands.CreatePipeline"/>'s "the pipeline closes over the RNG
/// stream set a command's own Mutate step needs" shape. Resolves immediately in <c>mutate</c> — a
/// deliberate player/AI action, not a multi-month negotiation, matching that same command's reasoning.</summary>
public static class ProposeFrontierTreatyCommands
{
    /// <summary>The named random stream this command draws from for its success roll, kept distinct
    /// from every other stream for rule 8's "adding a draw in one system must not perturb another".</summary>
    public const string StreamName = CampaignBootstrapper.FrontierNegotiationStreamName;

    public static readonly ValidationErrorCode NegotiatorNotFound = new("diplomacy.proposeTreaty.negotiatorNotFound");
    public static readonly ValidationErrorCode NegotiatorDeceased = new("diplomacy.proposeTreaty.negotiatorDeceased");
    public static readonly ValidationErrorCode NegotiatorNotOfHousehold = new("diplomacy.proposeTreaty.negotiatorNotOfHousehold");
    public static readonly ValidationErrorCode UnknownForeignPeople = new("diplomacy.proposeTreaty.unknownForeignPeople");
    public static readonly ValidationErrorCode LanguageGateNotCleared = new("diplomacy.proposeTreaty.languageGateNotCleared");
    public static readonly ValidationErrorCode UnmappedPeopleLanguage = new("diplomacy.proposeTreaty.unmappedPeopleLanguage");
    public static readonly ValidationErrorCode TreatyTypeAlreadyActive = new("diplomacy.proposeTreaty.treatyTypeAlreadyActive");
    public static readonly ValidationErrorCode TributeTermsInvalid = new("diplomacy.proposeTreaty.tributeTermsInvalid");
    public static readonly ValidationErrorCode TermOutOfRange = new("diplomacy.proposeTreaty.termOutOfRange");
    public static readonly ValidationErrorCode StandingTooHostile = new("diplomacy.proposeTreaty.standingTooHostile");

    public static CommandPipeline<WorldState, ProposeFrontierTreatyCommand> CreatePipeline(
        CultureLanguageMap cultureLanguages, RandomStreamSet randomStreams)
    {
        if (cultureLanguages is null)
            throw new ArgumentNullException(nameof(cultureLanguages));
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, ProposeFrontierTreatyCommand>(
            validate: (state, command) => Validate(state, command, cultureLanguages),
            mutate: (state, command) => Mutate(state, command, cultureLanguages, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, ProposeFrontierTreatyCommand command, CultureLanguageMap cultureLanguages)
    {
        if (!state.Characters.TryGet(command.NegotiatorCharacterId, out var negotiator))
            return NegotiatorNotFound;
        if (!negotiator!.IsAlive)
            return NegotiatorDeceased;
        if (negotiator.Household != command.HouseholdId)
            return NegotiatorNotOfHousehold;
        if (!state.Actors.TryGet(command.ForeignPeopleActorId, out var actor) || actor!.ActorType != LivingWorldActorType.ForeignPeople ||
            !ForeignPeopleQueries.TryGet(state, command.ForeignPeopleActorId, out var details))
        {
            return UnknownForeignPeople;
        }

        if (cultureLanguages.Resolve(details.CultureId) is null)
            return UnmappedPeopleLanguage;

        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, command.NegotiatorCharacterId, command.HouseholdId, command.ForeignPeopleActorId, cultureLanguages);
        if (!quality.GateCleared)
            return LanguageGateNotCleared;

        var standing = PerPeopleStandingResolver.GetEffective(state, command.HouseholdId, command.ForeignPeopleActorId).Standing;
        if (standing == HouseStandingLevel.Feuding)
            return StandingTooHostile;

        var tributeFieldsSet = command.TributeDirection != TributeDirection.None || command.MonthlyTribute != Money.Zero;
        if (command.Type == FrontierTreatyType.Tribute && command.TributeDirection == TributeDirection.None)
            return TributeTermsInvalid;
        if (command.Type != FrontierTreatyType.Tribute && tributeFieldsSet)
            return TributeTermsInvalid;

        if (command.TermMonths <= 0)
            return TermOutOfRange;

        var hasActiveOfSameType = state.FrontierTreaties.InAscendingOrder().Any(entry =>
            entry.Value.HouseholdId == command.HouseholdId && entry.Value.ForeignPeopleActorId == command.ForeignPeopleActorId &&
            entry.Value.Type == command.Type && entry.Value.Status == FrontierTreatyStatus.Active);
        if (hasActiveOfSameType)
            return TreatyTypeAlreadyActive;

        return null;
    }

    private static IDomainEvent[] Mutate(
        WorldState state, ProposeFrontierTreatyCommand command, CultureLanguageMap cultureLanguages, RandomStreamSet randomStreams)
    {
        var events = new List<IDomainEvent>();
        var quality = FrontierNegotiationQualityEvaluator.Evaluate(
            state, command.NegotiatorCharacterId, command.HouseholdId, command.ForeignPeopleActorId, cultureLanguages);

        var standing = PerPeopleStandingResolver.GetEffective(state, command.HouseholdId, command.ForeignPeopleActorId).Standing;
        var standingBonus = ((int)HouseStandingLevel.Neutral - (int)standing) * FrontierDiplomacyCatalog.StandingSuccessChancePercentPerTierTowardAllied;

        var successChance = Math.Clamp(
            FrontierDiplomacyCatalog.NegotiationBaseSuccessChancePercent + quality.QualityModifierPercent + standingBonus, 0, 100);
        var succeeded = randomStreams.NextUInt(StreamName, 100) < (uint)successChance;

        var key = new PerPeopleStandingKey(command.HouseholdId, command.ForeignPeopleActorId);

        if (succeeded)
        {
            var treatyId = state.FrontierTreatyIds.Issue();
            var expiresDate = new GameDate(command.SubmittedDate.TotalMonths + command.TermMonths);
            var treaty = FrontierTreaty.Create(
                treatyId, command.HouseholdId, command.ForeignPeopleActorId, command.Type, command.TributeDirection,
                command.MonthlyTribute, command.SubmittedDate, expiresDate);
            state.FrontierTreaties.Add(treatyId, treaty);

            PerPeopleStandingMutator.Apply(state, key, FrontierDiplomacyCatalog.TreatyConcludedGoodwillGain, command.SubmittedDate);

            events.Add(new FrontierTreatyConcludedEvent(
                state.EventIds.Issue(), command.SubmittedDate, treatyId, command.HouseholdId, command.ForeignPeopleActorId,
                treaty.Type, command.TributeDirection, expiresDate, quality.Source, quality.InterpresCharacterId,
                command.CommandId.ToTaggedString()));
            RivalDossierRefresh.Refresh(
                state, command.ForeignPeopleActorId, command.SubmittedDate,
                $"A {command.Type} treaty was concluded with this household.");
        }
        else
        {
            PerPeopleStandingMutator.Apply(state, key, -FrontierDiplomacyCatalog.TreatyRejectedGoodwillLoss, command.SubmittedDate);

            events.Add(new FrontierTreatyRejectedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.ForeignPeopleActorId,
                command.Type, quality.Source, successChance, command.CommandId.ToTaggedString()));
            RivalDossierRefresh.Refresh(
                state, command.ForeignPeopleActorId, command.SubmittedDate,
                $"A {command.Type} treaty proposal from this household was rejected.");
        }

        LivingWorldActorTieringService.RecordContactAndPromote(state, command.ForeignPeopleActorId, command.SubmittedDate);

        return events.ToArray();
    }
}
