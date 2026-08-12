using Gens.Simulation.Saves;
using Gens.Simulation.State;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>replay &lt;save&gt;</c>: reloads a save and recomputes its state hash. This is a deliberately
/// narrow "replay" — Phase 4 has not yet built a persisted command log or scheduler to actually re-run
/// commands against, so there is nothing to replay *from* yet. What this command proves today is the
/// exit gate's weaker but still real claim: loading a save reproduces the exact same state hash it was
/// saved with, every time. Once Phase 4 adds a command log, this command is the natural place to
/// extend into full command replay.
/// </summary>
public static class ReplayCommand
{
    public static int Run(string savePath)
    {
        var loaded = SaveReader.Read(savePath);
        var hash = StateHasher.Hash(loaded.State);

        Console.WriteLine($"Reloaded '{savePath}': date={loaded.State.Date.ToDisplayYearLabel()}, state hash={hash:x16}.");
        Console.WriteLine("Note: no persisted command log exists yet (Phase 4) — this reproduces the saved state, it does not re-run commands.");
        return 0;
    }
}
