using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Activities;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Feasts;

/// <summary>
/// Convenes a Feast (Phase 17 item 5; <c>gens-feasts-design.md</c> §2). Fills the Activity Engine's own
/// six slots exactly like a generic <see cref="PlanActivityCommand"/> (host, Venue, Guest List, start
/// month, Quality inputs, Budget — always <see cref="FeastCatalog.FeastType"/>'s key and always one
/// month, so neither field is repeated here), plus §6's Purpose and §3's optional Arbiter Bibendi and
/// §7's optional Entertainment description.
/// </summary>
public sealed record PlanFeastCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    RuntimeId<Actor>? HostActorId,
    ActivityVenueKind VenueKind,
    string VenueKey,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Holding>? HoldingId,
    GameDate StartDate,
    IReadOnlyList<RuntimeId<Character>> GuestIds,
    IReadOnlyList<ActivityQualityInput> QualityInputs,
    Money Budget,
    FeastPurpose Purpose,
    RuntimeId<Character>? ArbiterBibendiId,
    string? EntertainmentDescription) : ICommand;

/// <summary>Emitted whenever a <see cref="PlanFeastCommand"/> is accepted, alongside the composed <see
/// cref="ActivityPlannedEvent"/> — carrying the Feast-specific facts that generic event doesn't.
/// Public, matching <see cref="ActivityPlannedEvent"/>'s own visibility.</summary>
public sealed record FeastPlannedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    FeastPurpose Purpose,
    RuntimeId<Character>? ArbiterBibendiId,
    string? CausationId) : IDomainEvent
{
    public string Type => "feasts.planned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActivityId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PlanFeastCommand"/> (ADR 0006).</summary>
public static class PlanFeastCommands
{
    public static readonly ValidationErrorCode UnknownPurpose = new("feasts.plan.unknownPurpose");
    public static readonly ValidationErrorCode ArbiterBibendiNotPresent = new("feasts.plan.arbiterBibendiNotPresent");

    public static readonly CommandPipeline<WorldState, PlanFeastCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PlanFeastCommand command)
    {
        // Pre-validated here, against a throwaway inner command (CommandId is never read by
        // PlanActivityCommands.Validate), so this command's own CommandResult.Accepted reflects
        // reality before Mutate ever composes the real PlanActivityCommands.Pipeline call — the same
        // "no un-accept" reasoning ProposeMarriageCommand's own doc comment already established for a
        // composed Pipeline.Execute call, just resolved here by reusing the inner Validate directly
        // (widened to internal for exactly this) rather than duplicating a ~20-branch validation
        // gauntlet by hand.
        var innerError = PlanActivityCommands.Validate(state, BuildInnerCommand(default, command));
        if (innerError is { } code)
            return code;

        if (!Enum.IsDefined(typeof(FeastPurpose), command.Purpose))
            return UnknownPurpose;

        if (command.ArbiterBibendiId is { } arbiterId &&
            arbiterId != command.HostCharacterId &&
            !command.GuestIds.Contains(arbiterId))
            return ArbiterBibendiNotPresent;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PlanFeastCommand command)
    {
        var inner = BuildInnerCommand(state.CommandIds.Issue(), command);
        var events = new List<IDomainEvent>(PlanActivityCommands.Pipeline.Execute(state, inner).Events);
        var activityId = events.OfType<ActivityPlannedEvent>().Single().ActivityId;

        state.FeastRecords.Add(
            activityId,
            new FeastRecord(activityId, command.Purpose, command.ArbiterBibendiId, command.EntertainmentDescription));

        events.Add(new FeastPlannedEvent(
            state.EventIds.Issue(), command.SubmittedDate, activityId, command.Purpose, command.ArbiterBibendiId,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static PlanActivityCommand BuildInnerCommand(RuntimeId<Command> innerCommandId, PlanFeastCommand command) =>
        new(
            innerCommandId, command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
            command.HostCharacterId, command.HostHouseholdId, command.HostActorId,
            FeastCatalog.FeastType.Key, command.VenueKind, command.VenueKey, command.SettlementId, command.HoldingId,
            command.StartDate, 1, command.GuestIds, command.QualityInputs, command.Budget);
}
