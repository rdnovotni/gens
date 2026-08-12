using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Campaign;

/// <summary>
/// A minimally-typed command for the headless campaign shell's "submit a command" surface (Phase 4
/// item 3) and for dispatching a due <see cref="State.ScheduledActionEntry"/> (Phase 4 item 4)
/// through the same one command path every other actor uses (rule 2). It always validates and always
/// emits a <see cref="GenericCommandExecutedEvent"/> — there is no real gameplay command type yet
/// (those land per-system starting Phase 5), so this exists to prove and exercise the pipeline shape,
/// not to model any particular player action.
/// </summary>
public sealed record GenericCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    string CommandType,
    string PayloadJson) : ICommand;

/// <summary>Emitted whenever a <see cref="GenericCommand"/> is accepted. <see cref="Type"/> is always
/// <c>"command.executed"</c>, not <see cref="CommandType"/> itself — the latter is carried as payload
/// data (<see cref="CommandType"/> property below) so a report/knowledge reader can group on the
/// stable envelope type while still recovering which named command actually ran.</summary>
public sealed record GenericCommandExecutedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    string ActorId,
    string CommandType,
    string PayloadJson,
    string? CausationId) : IDomainEvent
{
    public string Type => "command.executed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActorId };
    public Visibility Visibility => Visibility.Public;
}
