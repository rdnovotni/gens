using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>§4 Adoption: brings <see cref="ChildId"/> into <see cref="HouseholdId"/>'s eligible-heir
/// pool with "identical standing" to a birth child (§3). Moves the adoptee's <see
/// cref="Character.Household"/> membership to the adopting Household. A direct command rather than a
/// weighted-comparison "Propose Adoption" interaction: §4 names Characters §9.1's Propose Adoption as
/// the intended resolution path, but no such weighted-comparison mechanism exists anywhere in this
/// codebase yet — <see cref="RecordMarriageCommand"/> is a bare direct record with no candidate-scoring of its
/// own either — building that shared interaction is out
/// of this item's scope, so this command establishes the concrete outcome directly, the same way this
/// codebase's other phases invent a scoped baseline where a forward-referenced mechanism doesn't exist
/// yet.</summary>
public sealed record AdoptChildCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> AdoptingParentId,
    RuntimeId<Character> ChildId) : ICommand;

/// <summary>Emitted whenever an <see cref="AdoptChildCommand"/> is accepted.</summary>
public sealed record ChildAdoptedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> AdoptingParentId,
    RuntimeId<Character> ChildId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.childAdopted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AdoptingParentId.ToTaggedString(), ChildId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdoptChildCommand"/> (ADR 0006).</summary>
public static class AdoptChildCommands
{
    public static readonly ValidationErrorCode HouseholdHasNoHead = new("succession.adoptChild.householdHasNoHead");
    public static readonly ValidationErrorCode NotTheAdoptingParentsHead = new("succession.adoptChild.notTheAdoptingParentsHead");
    public static readonly ValidationErrorCode ChildNotFound = new("succession.adoptChild.childNotFound");
    public static readonly ValidationErrorCode ChildDeceased = new("succession.adoptChild.childDeceased");
    public static readonly ValidationErrorCode CannotAdoptOwnBirthChild = new("succession.adoptChild.cannotAdoptOwnBirthChild");
    public static readonly ValidationErrorCode AlreadyAdopted = new("succession.adoptChild.alreadyAdopted");

    public static readonly CommandPipeline<WorldState, AdoptChildCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdoptChildCommand command)
    {
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return HouseholdHasNoHead;
        if (headship.HeadCharacterId != command.AdoptingParentId)
            return NotTheAdoptingParentsHead;
        if (!state.Characters.TryGet(command.ChildId, out var child))
            return ChildNotFound;
        if (!child.IsAlive)
            return ChildDeceased;
        if (child.MotherId == command.AdoptingParentId || child.FatherId == command.AdoptingParentId)
            return CannotAdoptOwnBirthChild;

        state.HeirDesignations.TryGet(command.HouseholdId, out var existing);
        if (existing?.AdoptedChildIds.Contains(command.ChildId) == true)
            return AlreadyAdopted;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdoptChildCommand command)
    {
        var designation = state.HeirDesignations.TryGet(command.HouseholdId, out var existing)
            ? existing
            : HeirDesignation.Empty(command.HouseholdId);

        state.HeirDesignations.Remove(command.HouseholdId);
        state.HeirDesignations.Add(
            command.HouseholdId,
            designation with { AdoptedChildIds = designation.AdoptedChildIds.Append(command.ChildId).ToArray() });

        state.Characters.TryGet(command.ChildId, out var child);
        var relocated = child with { Household = command.HouseholdId };
        state.Characters.Remove(command.ChildId);
        state.Characters.Add(command.ChildId, relocated);

        return new IDomainEvent[]
        {
            new ChildAdoptedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.AdoptingParentId,
                command.ChildId, command.CommandId.ToTaggedString()),
        };
    }
}
