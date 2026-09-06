#nullable enable

using Gens.Simulation.Queries;

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`state` — runs <see cref="CampaignDebugQuery"/> (built for exactly this: "the headless
/// console runner's debug inspectors") through the shell's sanctioned read path and prints the
/// resulting snapshot record (partition sizes/next-IDs, next command sequence number, state hash).
/// </summary>
public sealed class StateCommand : IDevConsoleCommand
{
    private const string ObserverId = DevConsoleContext.DevConsoleActorId;

    public string Name => "state";

    public string Description => "Prints partition sizes, the next command sequence number, and the state hash.";

    public string Execute(DevConsoleContext context, string[] args)
    {
        var shell = context.Shell;
        if (shell is null)
            return "No active campaign.";

        return shell.Query(new CampaignDebugQuery(), ObserverId).ToString();
    }
}
