using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§2.1's default heir preference: free, reversible, always adjustable. Passing <c>null</c>
/// for <see cref="PreferredHeirId"/> clears it back to "no preference set" — <see
/// cref="HeirEligibilityService"/>'s default agnatic-order fallback then decides who inherits.</summary>
public sealed record SetPreferredHeirCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? PreferredHeirId) : ICommand;

/// <summary>Emitted whenever a <see cref="SetPreferredHeirCommand"/> is accepted.</summary>
public sealed record PreferredHeirSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? PreferredHeirId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.preferredHeirSet";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => PreferredHeirId is { } heir
        ? new[] { HouseholdId.ToTaggedString(), heir.ToTaggedString() }
        : new[] { HouseholdId.ToTaggedString() };

    public Visibility Visibility => Visibility.Private(HouseholdId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="SetPreferredHeirCommand"/> (ADR 0006).</summary>
public static class SetPreferredHeirCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.setPreferredHeir.householdHasNoHead");
    public static readonly ValidationErrorCode HeirNotEligible = new("succession.setPreferredHeir.heirNotEligible");

    public static readonly CommandPipeline<WorldState, SetPreferredHeirCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetPreferredHeirCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return HouseholdHasNoHead;

        if (command.PreferredHeirId is { } heirId)
        {
            state.HeirDesignations.TryGet(command.HouseholdId, out var existing);
            var pool = HeirEligibilityService.EligibleHeirs(state, headship.HeadCharacterId, existing);
            if (!pool.Contains(heirId))
                return HeirNotEligible;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetPreferredHeirCommand command)
    {
        var designation = state.HeirDesignations.TryGet(command.HouseholdId, out var existing)
            ? existing
            : HeirDesignation.Empty(command.HouseholdId);

        state.HeirDesignations.Remove(command.HouseholdId);
        state.HeirDesignations.Add(command.HouseholdId, designation with { PreferredHeirId = command.PreferredHeirId });

        return new IDomainEvent[]
        {
            new PreferredHeirSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.PreferredHeirId,
                command.CommandId.ToTaggedString()),
        };
    }
}
