using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§3's "acknowledged Illegitimate children only" — moves an <see
/// cref="Legitimacy.Illegitimate"/> birth child into <paramref name="HouseholdId"/>'s eligible-heir
/// pool. An unacknowledged Illegitimate child never enters it (§3). Does not change <see
/// cref="Character.Legitimacy"/> itself — legitimacy and heir-eligibility are kept as separate facts,
/// matching how <see cref="HeirDesignation.AcknowledgedIllegitimateChildIds"/> is consulted alongside
/// <see cref="Legitimacy"/> rather than folded into it.
///
/// <b>§10's real cost, added by Phase 17 item 3 slice 7:</b> "acknowledging an Illegitimate child is a
/// deliberate, visible choice carrying its own social cost... not a quiet toggle." This command's own
/// <c>Mutate</c> step, on top of the <see cref="HeirDesignation"/> update above, now (1) always applies
/// a real household Dignitas penalty via the one shared <see cref="AdjustDignitasCommand"/> mover (<see
/// cref="Romance.RomanceCatalog.IllegitimateChildAcknowledgmentDignitasPenalty"/>), and (2) — only when
/// the acknowledging parent currently holds an open marriage to someone other than the child's other
/// parent, i.e. there is an actual betrayed spouse to react — records a negative <see
/// cref="RecordInteractionCommand"/> opinion swing from that spouse toward the acknowledging parent
/// (<see cref="Romance.RomanceCatalog.IllegitimateChildAcknowledgmentBetrayedSpouseOpinionDelta"/>).
/// An unmarried acknowledging parent, or one married to the child's own other parent (no betrayal —
/// acknowledging one's own child by one's own spouse wrongs nobody), is a deliberate no-op on this
/// second point: nothing forces an interaction when there is nobody actually betrayed.</summary>
public sealed record AcknowledgeIllegitimateChildCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> AcknowledgingParentId,
    RuntimeId<Character> ChildId) : ICommand;

/// <summary>Emitted whenever an <see cref="AcknowledgeIllegitimateChildCommand"/> is accepted.</summary>
public sealed record IllegitimateChildAcknowledgedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> AcknowledgingParentId,
    RuntimeId<Character> ChildId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.illegitimateChildAcknowledged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AcknowledgingParentId.ToTaggedString(), ChildId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AcknowledgeIllegitimateChildCommand"/> (ADR 0006).</summary>
public static class AcknowledgeIllegitimateChildCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.acknowledgeIllegitimateChild.householdHasNoHead");
    public static readonly ValidationErrorCode NotTheAcknowledgingParentsHead = new(
        "succession.acknowledgeIllegitimateChild.notTheAcknowledgingParentsHead");
    public static readonly ValidationErrorCode ChildNotFound = new("succession.acknowledgeIllegitimateChild.childNotFound");
    public static readonly ValidationErrorCode NotIllegitimateChildOfParent = new(
        "succession.acknowledgeIllegitimateChild.notIllegitimateChildOfParent");
    public static readonly ValidationErrorCode AlreadyAcknowledged = new("succession.acknowledgeIllegitimateChild.alreadyAcknowledged");

    public static readonly CommandPipeline<WorldState, AcknowledgeIllegitimateChildCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AcknowledgeIllegitimateChildCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return HouseholdHasNoHead;
        if (headship.HeadCharacterId != command.AcknowledgingParentId)
            return NotTheAcknowledgingParentsHead;
        if (!state.Characters.TryGet(command.ChildId, out var child))
            return ChildNotFound;
        if (child.Legitimacy != Legitimacy.Illegitimate ||
            (child.MotherId != command.AcknowledgingParentId && child.FatherId != command.AcknowledgingParentId))
            return NotIllegitimateChildOfParent;

        state.HeirDesignations.TryGet(command.HouseholdId, out var existing);
        if (existing?.AcknowledgedIllegitimateChildIds.Contains(command.ChildId) == true)
            return AlreadyAcknowledged;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AcknowledgeIllegitimateChildCommand command)
    {
        var designation = state.HeirDesignations.TryGet(command.HouseholdId, out var existing)
            ? existing
            : HeirDesignation.Empty(command.HouseholdId);

        state.HeirDesignations.Remove(command.HouseholdId);
        state.HeirDesignations.Add(
            command.HouseholdId,
            designation with
            {
                AcknowledgedIllegitimateChildIds = designation.AcknowledgedIllegitimateChildIds.Append(command.ChildId).ToArray(),
            });

        var events = new List<IDomainEvent>();

        // §10: the real, visible household Dignitas cost — always applied, routed through the one
        // shared Dignitas mover rather than poking HouseholdReputation directly (rule 2).
        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state,
            new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.HouseholdId, -RomanceCatalog.IllegitimateChildAcknowledgmentDignitasPenalty,
                "acknowledged an illegitimate child")).Events);

        // §10: a betrayed-spouse opinion swing, but only when one genuinely exists — the acknowledging
        // parent must currently hold an open marriage to someone OTHER than the child's other parent.
        // An unmarried acknowledging parent, or one married to the child's own other parent, betrays
        // nobody by acknowledging their own child, so this stays a deliberate no-op for both of those
        // cases (see this file's own class-level doc comment).
        if (state.Characters.TryGet(command.AcknowledgingParentId, out var parent) &&
            state.Characters.TryGet(command.ChildId, out var child))
        {
            var otherParentId = child!.MotherId == command.AcknowledgingParentId ? child.FatherId : child.MotherId;

            if (parent!.CurrentSpouseId is { } spouseId && spouseId != otherParentId)
            {
                events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                    state,
                    new RecordInteractionCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        spouseId, command.AcknowledgingParentId,
                        RomanceCatalog.IllegitimateChildAcknowledgmentBetrayedSpouseOpinionDelta,
                        BondTag.None, BondTag.None, RelationshipOrigin.Family)).Events);
            }
        }

        events.Add(new IllegitimateChildAcknowledgedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.AcknowledgingParentId,
            command.ChildId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
