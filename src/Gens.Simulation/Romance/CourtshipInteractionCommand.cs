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

/// <summary>The four cooperative Courtship interactions (<c>gens-romance-sexuality-lineage-design.md</c>
/// §4). Deliberately not routed through the adversarial <see cref="Interactions.Scheme"/> engine — unlike
/// the Seduce Scheme (§7), Affection/Attraction is already its own progress signal, so there is nothing a
/// Multi-stage progress/discovery race would add here.</summary>
public enum CourtshipInteractionKind
{
    /// <summary>A light, low-stakes overture (§4).</summary>
    Flirt,

    /// <summary>A more deliberate, higher-investment pursuit (§4).</summary>
    CourtWoo,

    /// <summary>A direct declaration, the highest-investment/highest-payoff of the four (§4).</summary>
    ConfessFeelings,

    /// <summary>A rejection — the one Courtship interaction that moves both scores down rather than up
    /// (§4).</summary>
    Rebuke,
}

/// <summary>Records one Courtship interaction between two eligible Characters
/// (<c>gens-romance-sexuality-lineage-design.md</c> §4). A thin wrapper: all it decides is which
/// <see cref="RomanceCatalog"/> deltas apply for <see cref="Kind"/>, then submits <see
/// cref="RecordRomanticInteractionCommand"/> to do the actual bond mutation, matching <see
/// cref="Scandal.RecordScandalCommand"/>'s own "one command's Mutate composes another's Pipeline inline"
/// idiom (see that file for the precedent this follows).</summary>
public sealed record RecordCourtshipInteractionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> InitiatorId,
    RuntimeId<Character> TargetId,
    CourtshipInteractionKind Kind) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordCourtshipInteractionCommand"/> is accepted, in addition
/// to whatever <see cref="RecordRomanticInteractionCommand"/> itself emits (this command's Mutate merges
/// the sub-command's events into its own result and then adds this one on top, matching <see
/// cref="Scandal.RecordScandalCommand"/>'s identical "merge sub-events, then add my own" idiom) — this is
/// the event a caller interested specifically in "a Courtship interaction of this Kind happened," rather
/// than the more generic Affection/Attraction swing, should read. Private to both parties, like every
/// other Romance event — a courtship overture is not broadcast (§4).</summary>
public sealed record CourtshipInteractionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> InitiatorId,
    RuntimeId<Character> TargetId,
    CourtshipInteractionKind Kind,
    string? CausationId) : IDomainEvent
{
    public string Type => "romance.courtshipInteractionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatorId.ToTaggedString(), TargetId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(InitiatorId.ToTaggedString(), TargetId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="RecordCourtshipInteractionCommand"/> (ADR 0006).</summary>
public static class RecordCourtshipInteractionCommands
{
    public static readonly CommandPipeline<WorldState, RecordCourtshipInteractionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordCourtshipInteractionCommand command) =>
        RomanceEligibility.CheckPair(state, command.InitiatorId, command.TargetId, command.SubmittedDate);

    private static IDomainEvent[] Mutate(WorldState state, RecordCourtshipInteractionCommand command)
    {
        var (affectionDelta, attractionDelta) = command.Kind switch
        {
            CourtshipInteractionKind.Flirt => (RomanceCatalog.FlirtAffectionDelta, RomanceCatalog.FlirtAttractionDelta),
            CourtshipInteractionKind.CourtWoo => (RomanceCatalog.CourtWooAffectionDelta, RomanceCatalog.CourtWooAttractionDelta),
            CourtshipInteractionKind.ConfessFeelings =>
                (RomanceCatalog.ConfessFeelingsAffectionDelta, RomanceCatalog.ConfessFeelingsAttractionDelta),
            CourtshipInteractionKind.Rebuke => (RomanceCatalog.RebukeAffectionDelta, RomanceCatalog.RebukeAttractionDelta),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind, "Unknown courtship interaction kind."),
        };

        var events = new List<IDomainEvent>(RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.InitiatorId, command.TargetId, affectionDelta, attractionDelta, BondTypeOverride: null)).Events);

        events.Add(new CourtshipInteractionRecordedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.InitiatorId, command.TargetId, command.Kind,
            command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
