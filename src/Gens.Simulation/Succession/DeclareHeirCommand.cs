using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§2.2's Formal Declaration — a Curia announcement naming this heir over any bare <see
/// cref="HeirDesignation.PreferredHeirId"/>, and over the default agnatic-line fallback (§2.4).
/// Replaces any prior Formal Declaration outright — this implementation still does not model §2.2's
/// Dignitas cost for reversing a still-eligible declared heir. That is no longer because no such stat
/// exists: household Dignitas (<see cref="Reputation.AdjustDignitasCommand"/>) and Character-level Fame
/// (<see cref="Fame.AdjustFameCommand"/>) both exist today (Phase 17 item 3 slice 7 closed that gap for
/// <see cref="AcknowledgeIllegitimateChildCommand"/>, its neighboring Succession command). Reversing a
/// declared heir simply remains out of THIS command's own scope — no design-doc section asks for a
/// reversal-specific cost here the way §10 asks for one on acknowledgment, and bolting one on would be
/// inventing a new cost mechanic rather than fixing a stale claim. A future Succession-focused pass
/// could add one using the exact same <see cref="Reputation.AdjustDignitasCommand"/> pattern this
/// command's own neighbor now uses, if a later design pass actually calls for it.</summary>
public sealed record DeclareHeirCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeirId) : ICommand;

/// <summary>Emitted whenever a <see cref="DeclareHeirCommand"/> is accepted.</summary>
public sealed record HeirFormallyDeclaredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeirId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.heirFormallyDeclared";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), HeirId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="DeclareHeirCommand"/> (ADR 0006).</summary>
public static class DeclareHeirCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.declareHeir.householdHasNoHead");
    public static readonly ValidationErrorCode HeirNotEligible = new("succession.declareHeir.heirNotEligible");

    public static readonly CommandPipeline<WorldState, DeclareHeirCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DeclareHeirCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return HouseholdHasNoHead;

        state.HeirDesignations.TryGet(command.HouseholdId, out var existing);
        var pool = HeirEligibilityService.EligibleHeirs(state, headship.HeadCharacterId, existing);
        if (!pool.Contains(command.HeirId))
            return HeirNotEligible;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DeclareHeirCommand command)
    {
        var designation = state.HeirDesignations.TryGet(command.HouseholdId, out var existing)
            ? existing
            : HeirDesignation.Empty(command.HouseholdId);

        state.HeirDesignations.Remove(command.HouseholdId);
        state.HeirDesignations.Add(
            command.HouseholdId,
            designation with { FormallyDeclaredHeirId = command.HeirId, DeclaredDate = command.SubmittedDate });

        return new IDomainEvent[]
        {
            new HeirFormallyDeclaredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.HeirId,
                command.CommandId.ToTaggedString()),
        };
    }
}
