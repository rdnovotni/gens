using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>Establishes (or transfers, outside the death/handoff path — e.g. a scripted campaign
/// start) which Character currently heads a Household (Phase 11 item 1). Fails if the Household
/// already has a recorded head; <see cref="SuccessionHandoffSystem"/> is the only path that replaces
/// an existing <see cref="HouseholdHeadship"/> once one exists.</summary>
public sealed record EstablishHouseholdHeadCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeadCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="HouseholdHeadship"/> is first established.</summary>
public sealed record HouseholdHeadEstablishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeadCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "succession.householdHeadEstablished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), HeadCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="EstablishHouseholdHeadCommand"/> (ADR 0006).</summary>
public static class EstablishHouseholdHeadCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("succession.establishHead.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("succession.establishHead.characterDeceased");
    public static readonly ValidationErrorCode AlreadyHasHead = new("succession.establishHead.alreadyHasHead");

    public static readonly CommandPipeline<WorldState, EstablishHouseholdHeadCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EstablishHouseholdHeadCommand command)
    {
        if (!state.Characters.TryGet(command.HeadCharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (state.HouseholdHeadships.TryGet(command.HouseholdId, out _))
            return AlreadyHasHead;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EstablishHouseholdHeadCommand command)
    {
        state.HouseholdHeadships.Add(
            command.HouseholdId, new HouseholdHeadship(command.HouseholdId, command.HeadCharacterId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new HouseholdHeadEstablishedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.HeadCharacterId,
                command.CommandId.ToTaggedString()),
        };
    }
}
