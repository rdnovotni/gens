using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>run-campaign --seed &lt;n&gt; --months &lt;n&gt; --out &lt;path&gt; [--content-hash &lt;hash&gt;]</c>:
/// a minimal, content-free clock-advance smoke test — no <see cref="Gens.Simulation.Campaign.CampaignConfig"/>,
/// no bootstrap, no monthly systems (not even <see cref="Gens.Simulation.Campaign.ScheduledActionSystem"/>).
/// Constructs a bare <see cref="WorldState"/>, advances its clock for the requested number of months,
/// saves the result, and prints the final <see cref="StateHasher"/> hash so a second run with the same
/// seed can be checked for reproducibility by hand or by <c>verify-save</c>. For an actual bootstrapped
/// campaign, use <c>new-campaign</c> and <c>advance</c> instead (Phase 4).
/// </summary>
public static class RunCampaignCommand
{
    public static int Run(ulong seed, int months, string outPath, string contentHash)
    {
        if (months < 0)
        {
            Console.Error.WriteLine("--months must be non-negative.");
            return 1;
        }

        var state = new WorldState(new GameDate(0));
        var streams = new RandomStreamSet();
        streams.AddDerived("campaign", seed);

        for (var i = 0; i < months; i++)
            state.AdvanceMonth();

        SaveWriter.Write(outPath, state, streams, GameVersionInfo.Current, contentHash);

        var hash = StateHasher.Hash(state);
        Console.WriteLine($"Advanced {months} month(s) from seed {seed}. Final date: {state.Date.ToDisplayYearLabel()}. " +
            $"State hash: {hash:x16}. Saved to '{outPath}'.");
        return 0;
    }
}
