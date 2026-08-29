using Gens.Simulation.Campaign;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>advance &lt;savePath&gt; [--months &lt;n&gt;] --out &lt;path&gt; [--report-out &lt;path&gt;]</c>:
/// loads a save, ticks it forward the requested number of months — draining the calendar queue each
/// month via <see cref="ScheduledActionSystem"/> (Phase 4 item 4) — saves the result, and dumps the
/// monthly report for everything that happened across the advance (Phase 4 item 5). <c>--months</c>
/// defaults to 1: "advance one month" and "advance N months" are the same verb with a bigger number.
/// </summary>
public static class AdvanceCommand
{
    public static int Run(string savePath, int months, string outPath, string? reportOutPath)
    {
        if (months < 0)
        {
            Console.Error.WriteLine("--months must be non-negative.");
            return 1;
        }

        var loaded = SaveReader.Read(savePath);
        var state = loaded.State;
        var streams = loaded.RandomStreams;
        var allEvents = new List<IDomainEvent>();

        for (var i = 0; i < months; i++)
        {
            var simulation = new WriteSetVerifyingSimulation(new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() });
            var monthEvents = simulation.Tick(state, state.Date, streams);
            allEvents.AddRange(monthEvents);
            var chronicleEvents = ChronicleGenerationSystem.Generate(state, monthEvents);
            allEvents.AddRange(chronicleEvents);
            var combined = new List<IDomainEvent>(monthEvents);
            combined.AddRange(chronicleEvents);
            allEvents.AddRange(EpithetGenerationSystem.Generate(state, combined));
            state.AdvanceMonth();
        }

        SaveWriter.Write(outPath, state, streams, GameVersionInfo.Current, loaded.Manifest.ContentPackHash);

        var report = MonthlyReportProjector.Project(state.Date, allEvents);
        if (reportOutPath is not null)
            File.WriteAllBytes(reportOutPath, CanonicalJson.SerializeToCanonicalBytes(report));

        var hash = StateHasher.Hash(state);
        Console.WriteLine($"Advanced {months} month(s). Date: {state.Date.ToDisplayYearLabel()}. State hash: {hash:x16}.");
        Console.WriteLine($"Report: {report.Entries.Count} entr{(report.Entries.Count == 1 ? "y" : "ies")}.");
        foreach (var entry in report.Entries)
            Console.WriteLine($"  [{entry.Importance}/{entry.Acknowledgement}] ({entry.Group}) {entry.EventType}: {string.Join(", ", entry.SubjectIds)}");
        Console.WriteLine($"Saved to '{outPath}'.");
        return 0;
    }
}
