using Gens.Simulation.Queries;
using Gens.Simulation.Saves;
using Gens.Simulation.State;

namespace Gens.ContentCompiler.Commands;

/// <summary>
/// <c>inspect-state &lt;savePath&gt;</c>: the headless runner's debug inspector (Phase 4 item 6) —
/// dumps every <see cref="WorldState"/> partition's size/next-ID, the RNG stream registry, the save's
/// declared content-pack identity, and the result of running every registered <see
/// cref="IInvariantCheck"/> (none exist yet outside test scaffolding, so this reports "0 checks
/// registered" rather than a false pass/fail).
/// </summary>
public static class InspectStateCommand
{
    public static int Run(string savePath)
    {
        var loaded = SaveReader.Read(savePath);
        var snapshot = new CampaignDebugQuery().Execute(loaded.State, "system");

        Console.WriteLine($"Partitions: region={snapshot.RegionIds}, settlement={snapshot.SettlementIds}, plot={snapshot.PlotIds}, " +
            $"household={snapshot.HouseholdIds}, actor={snapshot.ActorIds}, character={snapshot.CharacterIds} (registered {snapshot.CharacterCount}), " +
            $"building={snapshot.BuildingIds}, contract={snapshot.ContractIds}, activity={snapshot.ActivityIds}, command={snapshot.CommandIds}, " +
            $"event={snapshot.EventIds}, scheduledAction={snapshot.ScheduledActionIds} (queued {snapshot.ScheduledActionCount}).");
        Console.WriteLine($"Knowledge entries: {snapshot.KnowledgeEntryCount}. Next command sequence: {snapshot.NextCommandSequenceNumber}.");
        Console.WriteLine($"Date: {snapshot.DisplayYearLabel}. State hash: {snapshot.StateHash:x16}.");
        Console.WriteLine($"RNG streams: {string.Join(", ", loaded.RandomStreams.Names)}.");
        Console.WriteLine($"Content pack hash: {loaded.Manifest.ContentPackHash}. Save format version: {loaded.Manifest.SaveFormatVersion}.");

        var runner = new InvariantCheckRunner(Array.Empty<IInvariantCheck>());
        var violations = runner.RunAll(loaded.State);
        Console.WriteLine(violations.Count == 0
            ? "Invariants: OK (0 checks registered)."
            : $"Invariants: {violations.Count} violation(s) recorded.");

        return 0;
    }
}
