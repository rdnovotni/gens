using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§3's "acknowledged Illegitimate children only" — moves an <see
/// cref="Legitimacy.Illegitimate"/> birth child into <paramref name="HouseholdId"/>'s eligible-heir
/// pool. An unacknowledged Illegitimate child never enters it (§3). Does not change <see
/// cref="Character.Legitimacy"/> itself — legitimacy and heir-eligibility are kept as separate facts,
/// matching how <see cref="HeirDesignation.AcknowledgedIllegitimateChildIds"/> is consulted alongside
/// <see cref="Legitimacy"/> rather than folded into it.</summary>
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

        return new IDomainEvent[]
        {
            new IllegitimateChildAcknowledgedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.AcknowledgingParentId,
                command.ChildId, command.CommandId.ToTaggedString()),
        };
    }
}
