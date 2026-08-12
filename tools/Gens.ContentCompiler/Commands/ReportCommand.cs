using Gens.Simulation.Campaign;
using Gens.Simulation.Saves;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>report &lt;reportPath&gt;</c>: pretty-prints a monthly report previously dumped by <c>advance
/// --report-out &lt;path&gt;</c> — the headless runner's "dump report" verb (Phase 4 item 5), applied
/// to an already-produced report file. Nothing is recomputed from a save: no event history is
/// persisted to a save yet, so a report only exists for the advance call that produced it.
/// </summary>
public static class ReportCommand
{
    public static int Run(string reportPath)
    {
        var bytes = File.ReadAllBytes(reportPath);
        var report = CanonicalJson.Deserialize<MonthlyReportProjection>(bytes);

        Console.WriteLine($"Monthly report for {report.Date.ToDisplayYearLabel()}: " +
            $"{report.Entries.Count} entr{(report.Entries.Count == 1 ? "y" : "ies")}.");
        foreach (var entry in report.Entries)
            Console.WriteLine($"  [{entry.Importance}/{entry.Acknowledgement}] ({entry.Group}) {entry.EventType}: {string.Join(", ", entry.SubjectIds)}");
        return 0;
    }
}
