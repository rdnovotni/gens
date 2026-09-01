using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>§4.1 Personal Quarantine — toggles <see cref="CharacterHealthCondition.Quarantined"/> on
/// one standing case. Only meaningful on an <see cref="CharacterHealthConditionStatus.Active"/> case:
/// a resolved case has nothing left to quarantine.</summary>
public sealed record SetPersonalQuarantineCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<CharacterHealthCondition> CaseId,
    bool Quarantined) : ICommand;

/// <summary>Emitted whenever a <see cref="SetPersonalQuarantineCommand"/> is accepted.</summary>
public sealed record CharacterQuarantineChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Characters.Character> CharacterId,
    RuntimeId<CharacterHealthCondition> CaseId,
    bool Quarantined,
    string? CausationId) : IDomainEvent
{
    public string Type => "health.characterQuarantineChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SetPersonalQuarantineCommands
{
    public static readonly ValidationErrorCode CaseNotFound = new("health.quarantine.caseNotFound");
    public static readonly ValidationErrorCode CaseNotActive = new("health.quarantine.caseNotActive");

    public static readonly CommandPipeline<WorldState, SetPersonalQuarantineCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetPersonalQuarantineCommand command)
    {
        if (!state.CharacterHealthConditions.TryGet(command.CaseId, out var existing))
            return CaseNotFound;
        if (existing.Status != CharacterHealthConditionStatus.Active)
            return CaseNotActive;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetPersonalQuarantineCommand command)
    {
        state.CharacterHealthConditions.TryGet(command.CaseId, out var existing);
        state.CharacterHealthConditions.Remove(command.CaseId);
        state.CharacterHealthConditions.Add(command.CaseId, existing with { Quarantined = command.Quarantined });

        return new IDomainEvent[]
        {
            new CharacterQuarantineChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, existing.CharacterId, command.CaseId,
                command.Quarantined, command.CommandId.ToTaggedString()),
        };
    }
}
