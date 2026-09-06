#nullable enable

using Gens.Simulation.Queries;

namespace Gens.Presentation.Shell.DevConsole.Commands;

/// <summary>`hash` — the state hash alone, pulled from the same <see cref="CampaignDebugQuery"/>
/// `state` uses, for a quick before/after divergence check without the rest of `state`'s output.
/// </summary>
public sealed class HashCommand : IDevConsoleCommand
{
    private const string ObserverId = DevConsoleContext.DevConsoleActorId;

    public string Name => "hash";

    public string Description => "Prints the current deterministic state hash.";

    public string Execute(DevConsoleContext context, string[] args)
    {
        var shell = context.Shell;
        if (shell is null)
            return "No active campaign.";

        return shell.Query(new CampaignDebugQuery(), ObserverId).StateHash.ToString();
    }
}
