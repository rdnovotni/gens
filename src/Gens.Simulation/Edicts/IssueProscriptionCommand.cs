using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Edicts;

/// <summary>Emitted whenever an <see cref="IssueProscriptionCommand"/> is accepted — §5.1's Declaration.
/// Public, matching every other real Edict's identical reasoning.</summary>
public sealed record ProscriptionIssuedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<EdictRecord> EdictId,
    RuntimeId<Household> IssuingHouseholdId,
    RuntimeId<Actor> TargetActorId,
    Money AssetsSeized,
    bool DemonstrationEffectTriggered,
    string? CausationId) : IDomainEvent
{
    public string Type => "edicts.proscriptionIssued";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { IssuingHouseholdId.ToTaggedString(), TargetActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §5.7's Proscription — "the single darkest Edict available... declares a named Rival House an outlaw,
/// stripping legal protection and seizing assets in one stroke." Gated on the issuing household holding
/// an active <see cref="MagistracyOffice.Duumvir"/> seat — the top of Local Magistracies' own four-office
/// ladder (Phase 12 item 2), read here as "Duumvir-or-above" since no higher office exists anywhere in
/// this codebase's own <see cref="MagistracyOffice"/> enum to be "above" it. §5.7's own alternate
/// "active civil-crisis Event" gate is a named, reasoned cut: no Events system entry anywhere in this
/// codebase carries a civil-crisis classification a command could check (Phase 12 item 3's own Omens
/// work made the identical finding for a different Event-gated mechanic), so only the Magistracy half of
/// §5.7's gate is checked, matching <see cref="Magistracies.AppointDecurionCommand"/>'s own "no building
/// exists, so that half of the gate is not checked" precedent applied to an Event instead of a building.
///
/// <b>Effect</b> reaches three real, already-shipped systems rather than inventing a new one: asset
/// seizure is a real <see cref="LedgerService.Post"/> transfer out of the target's own <see
/// cref="LedgerAccountKey.ForActor"/> account (Phase 12 item 6's own Arca convention, applied to seizure
/// instead of funding), capped at <see cref="EdictCatalog.ProscriptionMaxSeizure"/> and never more than
/// the target actually holds; a relationship-web scar between the issuing household's own recorded head
/// and the target Actor's own resolved head Character (when both resolve) via <see
/// cref="RecordInteractionCommand"/>, matching every other Phase 12 household-vs-Actor consequence's
/// identical "the player's own Household is never itself a <see cref="LivingWorldActor"/>, so household-
/// to-Actor consequences land on recorded heads instead" precedent (Phase 12 item 1's own <see
/// cref="Reputation.HouseholdReputation"/> doc comment); and §5.7's own demonstration effect — "every
/// regional Rival House shifts toward Wary or Hostile" — as one real <see
/// cref="AdjustHouseStandingCommand"/> (<see cref="HouseStandingAdjustmentDirection.TowardRivalry"/>)
/// call per other real, tracked <see cref="LivingWorldActorType.Gens"/> Actor, entirely Actor-to-Actor
/// and so reachable even though the issuing household itself has no Actor id to be a party to that
/// command directly. Reception (§5.7's own "the most severe available") is a real Scandal at <see
/// cref="EdictCatalog.ProscriptionReceptionSeverity"/>.
/// </summary>
public sealed record IssueProscriptionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> IssuingHouseholdId,
    RuntimeId<Actor> TargetActorId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="IssueProscriptionCommand"/> (ADR 0006).</summary>
public static class IssueProscriptionCommands
{
    public static readonly ValidationErrorCode InsufficientInfluence = EdictIssuance.InsufficientInfluence;
    public static readonly ValidationErrorCode TargetNotFound = new("edicts.issueProscription.targetNotFound");
    public static readonly ValidationErrorCode TargetNotAGens = new("edicts.issueProscription.targetNotAGens");
    public static readonly ValidationErrorCode IssuerLacksAuthority = new("edicts.issueProscription.issuerLacksAuthority");

    public static readonly CommandPipeline<WorldState, IssueProscriptionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, IssueProscriptionCommand command)
    {
        if (!state.Actors.TryGet(command.TargetActorId, out var target))
            return TargetNotFound;
        if (target!.ActorType != LivingWorldActorType.Gens)
            return TargetNotAGens;
        if (!HoldsActiveDuumvirSeat(state, command.IssuingHouseholdId))
            return IssuerLacksAuthority;

        return EdictIssuance.ValidateAffordability(state, command.IssuingHouseholdId, EdictCatalog.ProscriptionInfluenceCost);
    }

    private static IDomainEvent[] Mutate(WorldState state, IssueProscriptionCommand command)
    {
        var events = EdictIssuance.ChargeCosts(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.IssuingHouseholdId,
            EdictCatalog.ProscriptionInfluenceCost, EdictCatalog.ProscriptionDignitasCost,
            "Edict issued: Proscription");

        var seized = SeizeAssets(state, command);
        events.AddRange(seized.Events);

        state.Actors.TryGet(command.TargetActorId, out var target);

        if (state.HouseholdHeadships.TryGet(command.IssuingHouseholdId, out var issuerHeadship) &&
            target!.HeadCharacterId is { } targetHeadId &&
            state.Characters.TryGet(issuerHeadship!.HeadCharacterId, out var issuerHead) && issuerHead!.IsAlive &&
            state.Characters.TryGet(targetHeadId, out var targetHead) && targetHead!.IsAlive)
        {
            events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                state, new RecordInteractionCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    issuerHeadship.HeadCharacterId, targetHeadId, -30, BondTag.Nemesis, BondTag.None,
                    RelationshipOrigin.Political)).Events);
        }

        var demonstrationTriggered = false;
        foreach (var entry in state.Actors.InAscendingOrder())
        {
            // §5.7's demonstration effect is explicitly regional ("every regional Rival House"), not
            // campaign-wide — LivingWorldActor already carries its own RegionId, so this filters to the
            // target's own region rather than shifting every tracked Gens actor everywhere.
            if (entry.Key == command.TargetActorId || entry.Value.ActorType != LivingWorldActorType.Gens ||
                entry.Value.RegionId != target!.RegionId)
                continue;

            var result = AdjustHouseStandingCommands.Pipeline.Execute(
                state, new AdjustHouseStandingCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.TargetActorId, entry.Key, HouseStandingAdjustmentDirection.TowardRivalry));
            if (result.Accepted)
            {
                events.AddRange(result.Events);
                demonstrationTriggered = true;
            }
        }

        var (scandalId, receptionEvents) = EdictIssuance.RecordReception(
            state, command.CommandId, command.ActorId, command.SubmittedDate, command.IssuingHouseholdId,
            EdictCatalog.ProscriptionReceptionSeverity);
        events.AddRange(receptionEvents);

        var edictId = state.EdictRecordIds.Issue();
        state.EdictRecords.Add(edictId, new EdictRecord(
            edictId, command.IssuingHouseholdId, EdictType.Proscription, command.SubmittedDate,
            EdictCatalog.ProscriptionInfluenceCost, EdictCatalog.ProscriptionDignitasCost, scandalId,
            DemonstrationEffectTriggered: demonstrationTriggered));

        events.Add(new ProscriptionIssuedEvent(
            state.EventIds.Issue(), command.SubmittedDate, edictId, command.IssuingHouseholdId, command.TargetActorId,
            seized.Amount, demonstrationTriggered, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    /// <summary>Sizes and moves the seizure off the target's own <see
    /// cref="LivingWorldActorNetWorth.Band"/> — the only wealth figure a Gens actor actually carries
    /// (<see cref="EdictCatalog.ProscriptionMaxSeizure"/>'s own doc comment: <see
    /// cref="LedgerAccountKey.ForActor"/> is never funded for a Gens actor, only for a Collegium, so
    /// reading it here always seized zero from a real rival house). The seized amount enters the
    /// issuing household's Treasury from the <see cref="LedgerAccountKey.Mint"/> system account — the
    /// same "a household-external source of tracked money" convention <see
    /// cref="Stewardship.StewardAutonomousDecisionSystem"/> already uses — since a Background/Noteworthy
    /// actor's wealth Band has no ledger account of its own for a balanced two-sided posting to draw
    /// from. The target's own Band steps down one tier to make the seizure real on the actor's own
    /// stored state, not just a number materializing on the issuer's side.</summary>
    private static (Money Amount, List<IDomainEvent> Events) SeizeAssets(WorldState state, IssueProscriptionCommand command)
    {
        state.Actors.TryGet(command.TargetActorId, out var target);
        var band = target!.NetWorth.Band;
        var seizeAmount = EdictCatalog.ProscriptionSeizureByBand[band];

        if (seizeAmount <= Money.Zero)
            return (Money.Zero, new List<IDomainEvent>());

        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Transfers,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.Mint, -seizeAmount),
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.IssuingHouseholdId), seizeAmount),
            },
            reference: $"edicts:proscription:{command.CommandId.ToTaggedString()}");

        var downgradedBand = (HouseholdWealthBand)Math.Clamp(
            (int)band - 1, (int)HouseholdWealthBand.Ruined, (int)HouseholdWealthBand.Wealthy);
        state.Actors.Remove(command.TargetActorId);
        state.Actors.Add(command.TargetActorId, target with { NetWorth = target.NetWorth with { Band = downgradedBand } });

        return (seizeAmount, new List<IDomainEvent> { posted });
    }

    private static bool HoldsActiveDuumvirSeat(WorldState state, RuntimeId<Household> householdId)
    {
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            if (!MagistracyResolver.IsActive(entry.Value) || entry.Value.Office != MagistracyOffice.Duumvir)
                continue;
            if (state.Characters.TryGet(entry.Value.HolderId, out var holder) && holder!.Household == householdId)
                return true;
        }

        return false;
    }
}
