using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>Starts a new <see cref="SpyPlacement"/> (<c>gens-espionage-design.md</c> §2's Quick Op or
/// Persistent Network). Actor-agnostic like every other command in this codebase (rule 2) — a
/// player-submitted command and an NPC's automated choice both submit this exact command through the
/// same <see cref="CommandPipeline{TState,TCommand}"/>.</summary>
public sealed record PlaceSpyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> SpyCharacterId,
    RuntimeId<Character> SponsoringCharacterId,
    RuntimeId<Actor> TargetActorId,
    SpyPlacementType Type) : ICommand;

/// <summary>Emitted whenever a <see cref="PlaceSpyCommand"/> is accepted. Private to sponsor and spy —
/// a placement is not something its sponsor broadcasts, mirroring <see
/// cref="SchemeInitiatedEvent"/>'s identical visibility reasoning. Discovery (§6), not this event, is
/// what eventually surfaces a placement to anyone beyond its two participants.</summary>
public sealed record SpyPlacedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SpyPlacement> PlacementId,
    RuntimeId<Character> SpyCharacterId,
    RuntimeId<Character> SponsoringCharacterId,
    RuntimeId<Actor> TargetActorId,
    SpyPlacementType PlacementType,
    string? CausationId) : IDomainEvent
{
    public string Type => "interactions.spyPlaced";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SponsoringCharacterId.ToTaggedString(), SpyCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(SponsoringCharacterId.ToTaggedString(), SpyCharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="PlaceSpyCommand"/> (ADR 0006).</summary>
public static class PlaceSpyCommands
{
    public static readonly ValidationErrorCode SpyNotFound = new("interactions.placeSpy.spyNotFound");
    public static readonly ValidationErrorCode SponsorNotFound = new("interactions.placeSpy.sponsorNotFound");
    public static readonly ValidationErrorCode TargetActorNotFound = new("interactions.placeSpy.targetActorNotFound");
    public static readonly ValidationErrorCode SpyDeceased = new("interactions.placeSpy.spyDeceased");
    public static readonly ValidationErrorCode SponsorDeceased = new("interactions.placeSpy.sponsorDeceased");

    /// <summary>One spy holds one posting at a time — nothing in §2 forbids a sponsor running several
    /// unrelated placements against different targets, only the same spy being placed twice at once.</summary>
    public static readonly ValidationErrorCode SpyAlreadyPlaced = new("interactions.placeSpy.spyAlreadyPlaced");

    /// <summary>§2.2's Spymaster capacity cap (<see cref="SpyPlacementCatalog.SpymasterCapacityCap"/>):
    /// a sponsor cannot run more concurrent in-progress placements than their (as-yet-unwired)
    /// Spymaster can support.</summary>
    public static readonly ValidationErrorCode SpymasterCapacityExceeded = new("interactions.placeSpy.spymasterCapacityExceeded");

    public static readonly CommandPipeline<WorldState, PlaceSpyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PlaceSpyCommand command)
    {
        if (!state.Characters.TryGet(command.SpyCharacterId, out var spy))
            return SpyNotFound;
        if (!state.Characters.TryGet(command.SponsoringCharacterId, out var sponsor))
            return SponsorNotFound;
        if (!state.Actors.TryGet(command.TargetActorId, out _))
            return TargetActorNotFound;
        if (!spy.IsAlive)
            return SpyDeceased;
        if (!sponsor.IsAlive)
            return SponsorDeceased;

        var alreadyPlaced = state.SpyPlacements.InAscendingOrder().Any(entry =>
            entry.Value.Status == SpyPlacementStatus.InProgress &&
            entry.Value.SpyCharacterId == command.SpyCharacterId);
        if (alreadyPlaced)
            return SpyAlreadyPlaced;

        var activeCount = state.SpyPlacements.InAscendingOrder().Count(entry =>
            entry.Value.Status == SpyPlacementStatus.InProgress &&
            entry.Value.SponsoringCharacterId == command.SponsoringCharacterId);
        if (activeCount >= SpyPlacementCatalog.SpymasterCapacityCap)
            return SpymasterCapacityExceeded;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PlaceSpyCommand command)
    {
        state.Characters.TryGet(command.SpyCharacterId, out var spy);
        var concealmentQuality = spy!.Attributes.Intrigue;

        var placementId = state.SpyPlacementIds.Issue();
        var placement = SpyPlacement.Create(
            placementId, command.SpyCharacterId, command.SponsoringCharacterId, command.TargetActorId,
            command.Type, concealmentQuality, command.SubmittedDate);
        state.SpyPlacements.Add(placementId, placement);

        return new IDomainEvent[]
        {
            new SpyPlacedEvent(
                state.EventIds.Issue(), command.SubmittedDate, placementId, command.SpyCharacterId,
                command.SponsoringCharacterId, command.TargetActorId, command.Type, command.CommandId.ToTaggedString()),
        };
    }
}
