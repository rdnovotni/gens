using Gens.Simulation.Queries;
using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>load &lt;savePath&gt;</c>: the headless runner's standalone "load" verb (Phase 4 item 3) — loads
/// a save and prints a summary, without saving it back out or advancing the clock.
/// </summary>
public static class LoadCommand
{
    public static int Run(string savePath)
    {
        var loaded = SaveReader.Read(savePath);
        var snapshot = new CampaignDebugQuery().Execute(loaded.State, "system");

        Console.WriteLine($"Loaded '{savePath}': date={snapshot.DisplayYearLabel}, state hash={snapshot.StateHash:x16}, " +
            $"contentPackHash={loaded.Manifest.ContentPackHash}, saveFormatVersion={loaded.Manifest.SaveFormatVersion}.");
        return 0;
    }
}
