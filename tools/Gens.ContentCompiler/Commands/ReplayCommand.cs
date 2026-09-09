using Gens.Simulation.Saves;
using Gens.Simulation.State;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>replay &lt;save&gt;</c>: reloads a save and recomputes its state hash. Per
/// <see href="../../docs/engineering/adr/0019-replay-diagnostics-and-save-fixture-contract.md">ADR
/// 0019</see>, this — save/reload hash equality, plus the independent continuation parity
/// <c>Ur01SharedScenarioFixtureTests</c> exercises — is the supported "replay diagnostics" contract,
/// not re-running a persisted command log: no command log exists, and none is required for this gate.
/// A real command log remains legitimate future work if a concrete consumer (crash recovery,
/// spectator replay) ever needs one; this command is the natural place to extend into full command
/// replay if that happens.
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
