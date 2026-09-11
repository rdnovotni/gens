using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Diplomacy;

/// <summary>Ends an active <see cref="FrontierTreaty"/> early, at a goodwill cost. Raiding/retaliation
/// consequences for breaking a treaty are explicitly deferred — see the build roadmap's Phase 16 item 5
/// progress note.</summary>
public sealed record AbrogateFrontierTreatyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<FrontierTreaty> TreatyId,
    RuntimeId<Character> DecidingCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="FrontierTreaty"/> ends, whether by <see
/// cref="AbrogateFrontierTreatyCommand"/> or by <see cref="FrontierTreatySystem"/>'s own monthly expiry
/// check.</summary>
public sealed record FrontierTreatyEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FrontierTreaty> TreatyId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Actor> ForeignPeopleActorId,
    FrontierTreatyStatus EndStatus,
    string? CausationId) : IDomainEvent
{
    public string Type => "diplomacy.frontierTreatyEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), ForeignPeopleActorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AbrogateFrontierTreatyCommand"/> (ADR 0006).</summary>
public static class AbrogateFrontierTreatyCommands
{
    public static readonly ValidationErrorCode TreatyNotFound = new("diplomacy.abrogateTreaty.treatyNotFound");
    public static readonly ValidationErrorCode TreatyNotActive = new("diplomacy.abrogateTreaty.treatyNotActive");
    public static readonly ValidationErrorCode DeciderNotFound = new("diplomacy.abrogateTreaty.deciderNotFound");
    public static readonly ValidationErrorCode DeciderDeceased = new("diplomacy.abrogateTreaty.deciderDeceased");
    public static readonly ValidationErrorCode DeciderNotOfHousehold = new("diplomacy.abrogateTreaty.deciderNotOfHousehold");

    public static readonly CommandPipeline<WorldState, AbrogateFrontierTreatyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AbrogateFrontierTreatyCommand command)
    {
        if (!state.FrontierTreaties.TryGet(command.TreatyId, out var treaty))
            return TreatyNotFound;
        if (treaty!.Status != FrontierTreatyStatus.Active)
            return TreatyNotActive;
        if (!state.Characters.TryGet(command.DecidingCharacterId, out var decider))
            return DeciderNotFound;
        if (!decider!.IsAlive)
            return DeciderDeceased;
        if (decider.Household != treaty.HouseholdId)
            return DeciderNotOfHousehold;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AbrogateFrontierTreatyCommand command)
    {
        state.FrontierTreaties.TryGet(command.TreatyId, out var treaty);
        var updated = treaty! with { Status = FrontierTreatyStatus.Abrogated, EndedDate = command.SubmittedDate };
        state.FrontierTreaties.Remove(command.TreatyId);
        state.FrontierTreaties.Add(command.TreatyId, updated);

        var key = new PerPeopleStandingKey(treaty.HouseholdId, treaty.ForeignPeopleActorId);
        PerPeopleStandingMutator.Apply(state, key, -FrontierDiplomacyCatalog.AbrogationGoodwillPenalty, command.SubmittedDate);
        DignitasResolver.Apply(state, treaty.HouseholdId, -FrontierDiplomacyCatalog.AbrogationDignitasPenalty);

        RivalDossierRefresh.Refresh(
            state, treaty.ForeignPeopleActorId, command.SubmittedDate,
            $"This household abrogated its own {treaty.Type} treaty.");

        return new IDomainEvent[]
        {
            new FrontierTreatyEndedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.TreatyId, treaty.HouseholdId, treaty.ForeignPeopleActorId,
                FrontierTreatyStatus.Abrogated, command.CommandId.ToTaggedString()),
        };
    }
}
