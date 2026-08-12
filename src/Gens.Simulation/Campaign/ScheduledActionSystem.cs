using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Campaign;

/// <summary>
/// Drains the calendar queue (Phase 4 item 4) every tick: every <see cref="ScheduledActionEntry"/>
/// whose due date has arrived is dispatched, in deterministic (due date, action ID) order, as a
/// <see cref="GenericCommand"/> through <see cref="CommandPipeline{TState,TCommand}"/> — the same one
/// command path (rule 2) a player action or steward decision would use. Registered in
/// <see cref="TickPhase.ScheduledCommands"/>, the first phase of every tick (ADR 0005), so anything a
/// scheduled action produces is visible to every later-phase system in the same month.
/// </summary>
public sealed class ScheduledActionSystem : IMonthlySystem<WorldState>
{
    public string Id => "scheduled-actions.drain";
    public TickPhase Phase => TickPhase.ScheduledCommands;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "scheduledActions" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "scheduledActions", "commandIds", "eventIds", "commandSequence" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // Already ascending (due date, action ID) order (ADR 0004) via OrderedRegistry — no separate
        // sort needed, and materializing to an array first means removing entries below does not
        // disturb the enumeration we're removing from.
        var due = state.ScheduledActions.InAscendingOrder()
            .Where(entry => entry.Key.DueDate.TotalMonths <= context.Date.TotalMonths)
            .Select(entry => entry.Value)
            .ToArray();

        if (due.Length == 0)
            return Array.Empty<IDomainEvent>();

        var pipeline = new CommandPipeline<WorldState, GenericCommand>(
            validate: static (_, _) => null,
            mutate: (worldState, command) => new IDomainEvent[]
            {
                new GenericCommandExecutedEvent(
                    worldState.EventIds.Issue(),
                    context.Date,
                    command.ActorId,
                    command.CommandType,
                    command.PayloadJson,
                    command.CommandId.ToTaggedString()),
            },
            issueSequenceNumber: static worldState => worldState.IssueCommandSequenceNumber());

        var events = new List<IDomainEvent>(due.Length);
        foreach (var action in due)
        {
            state.ScheduledActions.Remove(new ScheduledActionKey(action.DueDate, action.ActionId));

            var command = new GenericCommand(
                state.CommandIds.Issue(),
                action.ActorId,
                context.Date,
                action.CausationId,
                action.ActionType,
                action.PayloadJson);

            var result = pipeline.Execute(state, command);
            if (result.Accepted)
                events.AddRange(result.Events);
        }

        return events;
    }
}
