using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>The general actor-agnostic standing-nudge path for a household/Foreign-People pair,
/// mirroring <see cref="AdjustHouseStandingCommand"/> exactly — future systems (Raiding &amp; Retaliation,
/// rival competing diplomacy) reuse this single command path (ADR 0006 rule 2) rather than each writing
/// <see cref="WorldState.PerPeopleStandings"/> directly.</summary>
public sealed record AdjustPerPeopleStandingCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    HouseStandingAdjustmentDirection Direction) : ICommand;

/// <summary>Emitted whenever an <see cref="AdjustPerPeopleStandingCommand"/> is accepted.</summary>
public sealed record PerPeopleStandingChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    HouseStandingLevel PreviousStanding,
    HouseStandingLevel NewStanding,
    string? CausationId) : IDomainEvent
{
    public string Type => "diplomacy.perPeopleStandingChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ForeignPeopleActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustPerPeopleStandingCommand"/> (ADR 0006).</summary>
public static class AdjustPerPeopleStandingCommands
{
    public static readonly ValidationErrorCode UnknownHousehold = new("diplomacy.adjustPerPeopleStanding.unknownHousehold");
    public static readonly ValidationErrorCode UnknownForeignPeople = new("diplomacy.adjustPerPeopleStanding.unknownForeignPeople");
    public static readonly ValidationErrorCode AlreadyAtExtreme = new("diplomacy.adjustPerPeopleStanding.alreadyAtExtreme");

    public static readonly CommandPipeline<WorldState, AdjustPerPeopleStandingCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustPerPeopleStandingCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out _))
            return UnknownHousehold;
        if (!state.Actors.TryGet(command.ForeignPeopleActorId, out var actor) || actor!.ActorType != LivingWorldActorType.ForeignPeople)
            return UnknownForeignPeople;

        var current = PerPeopleStandingResolver.GetEffective(state, command.HouseholdId, command.ForeignPeopleActorId).Standing;
        if (command.Direction == HouseStandingAdjustmentDirection.TowardAlliance && current == HouseStandingLevel.Allied)
            return AlreadyAtExtreme;
        if (command.Direction == HouseStandingAdjustmentDirection.TowardRivalry && current == HouseStandingLevel.Feuding)
            return AlreadyAtExtreme;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustPerPeopleStandingCommand command)
    {
        var key = new PerPeopleStandingKey(command.HouseholdId, command.ForeignPeopleActorId);
        var existing = PerPeopleStandingResolver.GetEffective(state, command.HouseholdId, command.ForeignPeopleActorId);

        // A direct one-tier step, mirroring AdjustHouseStandingCommand.Step exactly, rather than routing
        // through PerPeopleStandingMutator's goodwill accumulator — a deliberate, explicit standing
        // adjustment should always move exactly one tier, not be at the mercy of whatever goodwill
        // remainder the accumulator happens to be carrying.
        var delta = command.Direction == HouseStandingAdjustmentDirection.TowardAlliance ? -1 : 1;
        var next = (HouseStandingLevel)Math.Clamp(
            (int)existing.Standing + delta, (int)HouseStandingLevel.Allied, (int)HouseStandingLevel.Feuding);

        var updated = new PerPeopleStanding(command.HouseholdId, command.ForeignPeopleActorId, next, existing.Goodwill, command.SubmittedDate);
        if (state.PerPeopleStandings.TryGet(key, out _))
            state.PerPeopleStandings.Remove(key);
        state.PerPeopleStandings.Add(key, updated);

        RivalDossierRefresh.Refresh(
            state, command.ForeignPeopleActorId, command.SubmittedDate,
            $"Standing with this household shifted from {existing.Standing} to {next}.");

        return new IDomainEvent[]
        {
            new PerPeopleStandingChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.ForeignPeopleActorId,
                existing.Standing, next, command.CommandId.ToTaggedString()),
        };
    }
}
