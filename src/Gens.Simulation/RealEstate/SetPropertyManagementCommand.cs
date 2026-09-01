using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>§6's leasing flag itself (Phase 15 item 1): "any developed property the household owns can
/// be flagged Leased Out rather than Directly Managed," with a real, named Operator Character assigned
/// in the same command whenever the new status is <see cref="PropertyManagementStatus.LeasedOut"/>.
/// Flagging back to <see cref="PropertyManagementStatus.DirectlyManaged"/> clears the Operator (§6.1's
/// "the player replaces him" case reduces to setting a fresh Operator via a second call to this same
/// command).</summary>
public sealed record SetPropertyManagementCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertySubjectRef Subject,
    PropertyManagementStatus Status,
    RuntimeId<Character>? OperatorCharacterId = null) : ICommand;

public sealed record PropertyManagementChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertySubjectRef Subject,
    PropertyManagementStatus Status,
    RuntimeId<Character>? OperatorCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.propertyManagementChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => OperatorCharacterId is { } operatorId
        ? new[] { Subject.SubjectId, operatorId.ToTaggedString() }
        : new[] { Subject.SubjectId };
    public Visibility Visibility => Visibility.Public;
}

public static class SetPropertyManagementCommands
{
    public static readonly ValidationErrorCode SubjectNotFound = new("realEstate.setManagement.subjectNotFound");
    public static readonly ValidationErrorCode OperatorRequiredForLeasedOut = new("realEstate.setManagement.operatorRequiredForLeasedOut");
    public static readonly ValidationErrorCode OperatorNotFound = new("realEstate.setManagement.operatorNotFound");
    public static readonly ValidationErrorCode OperatorDeceased = new("realEstate.setManagement.operatorDeceased");

    public static readonly CommandPipeline<WorldState, SetPropertyManagementCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetPropertyManagementCommand command)
    {
        if (!PropertyResolver.TryResolve(state, command.Subject, out _))
            return SubjectNotFound;

        if (command.Status == PropertyManagementStatus.LeasedOut)
        {
            if (command.OperatorCharacterId is not { } operatorId)
                return OperatorRequiredForLeasedOut;
            if (!state.Characters.TryGet(operatorId, out var character))
                return OperatorNotFound;
            if (!character!.IsAlive)
                return OperatorDeceased;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetPropertyManagementCommand command)
    {
        var operatorId = command.Status == PropertyManagementStatus.LeasedOut ? command.OperatorCharacterId : null;
        PropertyResolver.SetManagement(state, command.Subject, command.Status, operatorId);

        return new IDomainEvent[]
        {
            new PropertyManagementChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.Subject, command.Status, operatorId,
                command.CommandId.ToTaggedString()),
        };
    }
}
