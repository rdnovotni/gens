using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Records one Romance interaction's effect on the undirected <see
/// cref="RomanticBond"/> between <see cref="CharacterAId"/> and <see cref="CharacterBId"/>
/// (<c>gens-romance-sexuality-lineage-design.md</c> §3, §16) — an Affection swing, an Attraction
/// swing, or a bond-type change, or any combination. This is the one generic mutator every later
/// Romance command/system (Courtship interactions, Seduce Scheme success, autonomous romance) submits
/// instead of touching <see cref="State.WorldState.RomanticBonds"/> directly, matching <see
/// cref="Characters.RecordInteractionCommand"/>'s identical "one shared mutator, reused everywhere"
/// role for the ordinary relationship web. Creates the <see cref="RomanticBond"/> record lazily, on
/// first contact, exactly like <see cref="Characters.RecordInteractionCommand"/> creates a <see
/// cref="Characters.Relationship"/> — nothing is pre-allocated for a pair until something actually
/// needs recording.</summary>
public sealed record RecordRomanticInteractionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId,
    int AffectionDelta,
    int AttractionDelta,
    RomanticBondType? BondTypeOverride) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordRomanticInteractionCommand"/> is accepted. Private,
/// matching <see cref="Characters.RelationshipInteractionRecordedEvent"/>'s identical reasoning — a
/// romantic swing is specifically the kind of interior state real people don't broadcast, so only the
/// two participants are named observers.</summary>
public sealed record RomanticInteractionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterAId,
    RuntimeId<Character> CharacterBId,
    RomanticBondType BondType,
    int AffectionBefore,
    int AffectionAfter,
    int AttractionBefore,
    int AttractionAfter,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.romanticInteractionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString() };

    public Visibility Visibility => Visibility.Private(CharacterAId.ToTaggedString(), CharacterBId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="RecordRomanticInteractionCommand"/> (ADR 0006).</summary>
public static class RecordRomanticInteractionCommands
{
    public static readonly ValidationErrorCode NothingToRecord = new("romance.recordInteraction.nothingToRecord");

    public static readonly CommandPipeline<WorldState, RecordRomanticInteractionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordRomanticInteractionCommand command)
    {
        // RomanceEligibility.CheckPair already covers existence and both-alive, so there is nothing
        // left for this pipeline to check on that front.
        var eligibilityError = RomanceEligibility.CheckPair(
            state, command.CharacterAId, command.CharacterBId, command.SubmittedDate);
        if (eligibilityError is { } code)
            return code;

        if (command.AffectionDelta == 0 && command.AttractionDelta == 0 && command.BondTypeOverride is null)
            return NothingToRecord;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordRomanticInteractionCommand command)
    {
        var key = RomanticBondKey.Create(command.CharacterAId, command.CharacterBId);
        var exists = state.RomanticBonds.TryGet(key, out var existing);

        var affectionBefore = exists ? existing.Affection : 0;
        var attractionBefore = exists ? existing.Attraction : 0;
        var affectionAfter = Math.Clamp(affectionBefore + command.AffectionDelta, RomanticBond.MinScore, RomanticBond.MaxScore);
        var attractionAfter = Math.Clamp(attractionBefore + command.AttractionDelta, RomanticBond.MinScore, RomanticBond.MaxScore);
        var bondType = command.BondTypeOverride ?? (exists ? existing.BondType : RomanticBondType.Courtship);
        var isKnownPublicly = exists && existing.IsKnownPublicly;
        var discoveryRisk = exists ? existing.DiscoveryRisk : 0;
        var formedDate = exists ? existing.FormedDate : command.SubmittedDate;

        var eventId = state.EventIds.Issue();
        var bond = new RomanticBond(
            bondType, affectionAfter, attractionAfter, isKnownPublicly, discoveryRisk,
            formedDate, command.SubmittedDate, eventId.ToTaggedString());

        if (exists)
            state.RomanticBonds.Remove(key);
        state.RomanticBonds.Add(key, bond);

        return new IDomainEvent[]
        {
            new RomanticInteractionRecordedEvent(
                eventId, command.SubmittedDate, command.CharacterAId, command.CharacterBId, bondType,
                affectionBefore, affectionAfter, attractionBefore, attractionAfter,
                command.CommandId.ToTaggedString()),
        };
    }
}
