#nullable enable

using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.State;

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`submit &lt;type&gt; &lt;payloadJson&gt; [actorId]` — the console's generic escape hatch,
/// mirroring `tools/Gens.ContentCompiler`'s `submit-command` verb: wraps the input in a
/// <see cref="GenericCommand"/> and runs it through the same inline
/// <see cref="CommandPipeline{TState,TCommand}"/> shape that CLI verb uses, via the shell's sanctioned
/// <see cref="CampaignShell.Submit{TCommand}"/> write path. Never bypasses validation — a
/// <see cref="GenericCommand"/> always validates today because no real gameplay command type has its
/// own dev-console wrapper yet (v1 scope).</summary>
public sealed class SubmitVerbCommand : IDevConsoleCommand
{
    public string Name => "submit";

    public string Description =>
        "submit <type> <payloadJson> [actorId] - submits a GenericCommand (payload JSON must not contain spaces).";

    public string Execute(DevConsoleContext context, string[] args)
    {
        var shell = context.Shell;
        if (shell is null)
            return "No active campaign.";

        if (args.Length < 2)
            return "Usage: " + Description;

        var commandType = args[0];
        var payloadJson = args[1];
        var actorId = args.Length > 2 ? args[2] : DevConsoleContext.DevConsoleActorId;

        var pipeline = new CommandPipeline<WorldState, GenericCommand>(
            validate: static (_, _) => null,
            mutate: (worldState, command) => new IDomainEvent[]
            {
                new GenericCommandExecutedEvent(
                    worldState.EventIds.Issue(), worldState.Date, command.ActorId, command.CommandType,
                    command.PayloadJson, command.CausationId),
            },
            issueSequenceNumber: static worldState => worldState.IssueCommandSequenceNumber());

        var command = new GenericCommand(
            shell.State.CommandIds.Issue(), actorId, shell.State.Date, null, commandType, payloadJson);
        var result = shell.Submit(pipeline, command);
        return result.Accepted
            ? $"Executed '{commandType}' as {actorId}. {result.Events.Count} event(s) emitted."
            : $"Rejected: {result.Error}.";
    }
}
