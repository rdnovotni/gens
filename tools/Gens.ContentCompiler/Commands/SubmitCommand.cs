using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>submit-command &lt;savePath&gt; --actor &lt;id&gt; --type &lt;name&gt; [--payload &lt;json&gt;]
/// [--due-in-months &lt;n&gt;] --out &lt;path&gt;</c>: the headless runner's "submit a command" verb
/// (Phase 4 item 3). With no <c>--due-in-months</c> (or <c>0</c>), the <see cref="GenericCommand"/>
/// executes immediately through <see cref="CommandPipeline{TState,TCommand}"/>. With a positive
/// <c>--due-in-months</c>, it is instead placed on the calendar queue (Phase 4 item 4) for <see
/// cref="ScheduledActionSystem"/> to dispatch on a future <c>advance</c>.
/// </summary>
public static class SubmitCommand
{
    public static int Run(string savePath, string actorId, string commandType, string payloadJson, int dueInMonths, string outPath)
    {
        if (dueInMonths < 0)
        {
            Console.Error.WriteLine("--due-in-months must be non-negative.");
            return 1;
        }

        var loaded = SaveReader.Read(savePath);
        var state = loaded.State;

        if (dueInMonths == 0)
        {
            var pipeline = new CommandPipeline<WorldState, GenericCommand>(
                validate: static (_, _) => null,
                mutate: (worldState, command) => new IDomainEvent[]
                {
                    new GenericCommandExecutedEvent(
                        worldState.EventIds.Issue(), worldState.Date, command.ActorId, command.CommandType, command.PayloadJson, command.CausationId),
                },
                issueSequenceNumber: static worldState => worldState.IssueCommandSequenceNumber());

            var command = new GenericCommand(state.CommandIds.Issue(), actorId, state.Date, null, commandType, payloadJson);
            var result = pipeline.Execute(state, command);
            Console.WriteLine(result.Accepted
                ? $"Executed '{commandType}' immediately. {result.Events.Count} event(s) emitted."
                : $"Rejected: {result.Error}.");
        }
        else
        {
            var dueDate = new GameDate(checked(state.Date.TotalMonths + dueInMonths));
            var actionId = state.ScheduledActionIds.Issue();
            state.ScheduledActions.Add(
                new ScheduledActionKey(dueDate, actionId),
                new ScheduledActionEntry(actionId, dueDate, actorId, commandType, payloadJson, null));
            Console.WriteLine($"Scheduled '{commandType}' for {dueDate.ToDisplayYearLabel()} (action {actionId.ToTaggedString()}).");
        }

        SaveWriter.Write(outPath, state, loaded.RandomStreams, GameVersionInfo.Current, loaded.Manifest.ContentPackHash);
        Console.WriteLine($"Saved to '{outPath}'.");
        return 0;
    }
}
