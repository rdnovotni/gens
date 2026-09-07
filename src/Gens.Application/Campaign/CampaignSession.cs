#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Application.Campaign;

/// <summary>
/// The engine-neutral application layer's sole holder of authoritative campaign state (Phase 9 item 5, ADR
/// 0013 rule 1). It wraps the same <see cref="WorldState"/>/<see cref="RandomStreamSet"/> pair the
/// headless console runner (<c>tools/Gens.ContentCompiler</c>'s <c>new-campaign</c>/<c>advance</c>/
/// <c>submit-command</c>) already owns, exposing only the two sanctioned entry points ADR 0013
/// draws the UI boundary around: <see cref="Query{TProjection}"/> and <see cref="Submit{TCommand}"/>.
/// Deliberately free of engine references so any serialized, single-threaded client can own one.
/// A session is not thread-safe; callers must serialize access on their application thread.
/// </summary>
public sealed class CampaignSession
{
    /// <summary>Transitional mutable-state escape hatch required by the current Unity client. New clients must use <see cref="Query{TProjection}"/> and <see cref="Submit{TCommand}"/>.</summary>
    public WorldState State { get; }

    /// <summary>Transitional access to the one authoritative RNG registry; no copy is created.</summary>
    public RandomStreamSet RandomStreams { get; }

    /// <summary>The player's household, issued at bootstrap (<see cref="CampaignBootstrapper"/>) — the
    /// implicit subject every screen's queries scope to until Phase 10's rival houses give the shell
    /// more than one household to ever look at.</summary>
    public RuntimeId<Household> HouseholdId { get; }

    /// <summary>The settlement the player's household starts in, issued alongside <see
    /// cref="HouseholdId"/> at bootstrap.</summary>
    public RuntimeId<Settlement> SettlementId { get; }

    /// <summary>The compiled content pack this campaign is authored against (ADR 0012) — carried
    /// forward from bootstrap (or a loaded save's manifest) purely so <see cref="Save"/> can round-trip
    /// it without the caller having to track it separately.</summary>
    public string ContentPackHash { get; }

    /// <summary>The player household's runtime ID is always the very first one <see
    /// cref="CampaignBootstrapper"/> issues (Phase 9's single-household vertical slice — Phase 10's
    /// rival houses are the first to add a second one), so a freshly <see cref="Load"/>ed save can
    /// reconstruct it without the save format itself needing to persist it separately.</summary>
    private static readonly RuntimeId<Household> RootHouseholdId = RuntimeId<Household>.Parse("household_0000000");

    /// <summary>The player's starting settlement's runtime ID, by the same "first one issued" reasoning
    /// as <see cref="RootHouseholdId"/>.</summary>
    private static readonly RuntimeId<Settlement> RootSettlementId = RuntimeId<Settlement>.Parse("settlement_0000000");

    private CampaignSession(
        WorldState state, RandomStreamSet randomStreams, RuntimeId<Household> householdId,
        RuntimeId<Settlement> settlementId, string contentPackHash)
    {
        State = state;
        RandomStreams = randomStreams;
        HouseholdId = householdId;
        SettlementId = settlementId;
        ContentPackHash = contentPackHash;
    }

    /// <summary>Bootstraps a fresh campaign from <paramref name="config"/>, returning the session that
    /// owns it alongside the initial history the caller should surface before the first tick runs
    /// (mirroring <c>NewCampaignCommand</c>'s own console output).</summary>
    public static CampaignSession CreateNew(CampaignConfig config, out IReadOnlyList<IDomainEvent> initialHistory)
    {
        var campaign = CampaignBootstrapper.Bootstrap(config);
        initialHistory = campaign.InitialHistory;
        return new CampaignSession(
            campaign.State, campaign.RandomStreams, campaign.HouseholdId, campaign.SettlementId, config.ContentPackHash);
    }

    /// <summary>Loads a previously <see cref="Save"/>d campaign from <paramref name="path"/>, mirroring
    /// the console runner's <c>load</c> verb (<see cref="Gens.Simulation.Saves.SaveReader.Read"/>) but
    /// returning a ready-to-use session rather than just printing a summary. <paramref name="manifest"/>
    /// is the loaded save's manifest (format/game/content versions, RNG stream states), surfaced for a
    /// caller that wants to display or log it.</summary>
    public static CampaignSession Load(string path, out SaveManifest manifest)
    {
#if !UNITY_2021_1_OR_NEWER
        var loaded = SaveReader.Read(path);
        manifest = loaded.Manifest;
        return new CampaignSession(loaded.State, loaded.RandomStreams, RootHouseholdId, RootSettlementId, manifest.ContentPackHash);
#else
        throw new NotSupportedException(
            "Saving/loading .gens packages requires System.Text.Json, which this Unity project has no " +
            "assembly reference for (Gens.Simulation.asmdef has no precompiledReferences for it, and " +
            "Unity's asmdef compilation never sees the csproj's NuGet PackageReference) — only the " +
            ".NET Core runtime runner supports it today.");
#endif
    }

    /// <summary>Writes this session's current state to <paramref name="path"/>, mirroring the console
    /// runner's <c>save</c> verb (<see cref="Gens.Simulation.Saves.SaveWriter.Write"/>).</summary>
    public void Save(string path, string gameVersion)
    {
#if !UNITY_2021_1_OR_NEWER
        SaveWriter.Write(path, State, RandomStreams, gameVersion, ContentPackHash);
#else
        throw new NotSupportedException(
            "Saving/loading .gens packages requires System.Text.Json, which this Unity project has no " +
            "assembly reference for — only the .NET Core runtime runner supports it today.");
#endif
    }

    /// <summary>Deterministic replay diagnostics (Phase 9 item 8): saves the current state to
    /// <paramref name="diagnosticsPath"/>, reloads it, and compares <see cref="StateHasher"/> hashes
    /// before and after — the same "load reproduces the exact same state hash it was saved with" check
    /// the console runner's <c>replay</c>/<c>compare-hashes</c> verbs make, surfaced here so the Unity
    /// shell can run it on demand without shelling out.</summary>
    public ReplayDiagnosticsResult VerifyDeterministicReplay(string diagnosticsPath, string gameVersion)
    {
#if !UNITY_2021_1_OR_NEWER
        var hashBeforeSave = StateHasher.Hash(State);
        Save(diagnosticsPath, gameVersion);
        var reloaded = SaveReader.Read(diagnosticsPath);
        var hashAfterReload = StateHasher.Hash(reloaded.State);
        return new ReplayDiagnosticsResult(hashBeforeSave, hashAfterReload, hashBeforeSave == hashAfterReload);
#else
        throw new NotSupportedException(
            "Deterministic replay diagnostics require System.Text.Json, which this Unity project has no " +
            "assembly reference for — only the .NET Core runtime runner supports it today.");
#endif
    }

    /// <summary>The sole read path (ADR 0013): executes <paramref name="query"/> against the session's
    /// own <see cref="WorldState"/> for <paramref name="observerId"/> and returns the resulting
    /// projection DTO. Presentation clients should use this path rather than the transitional <see cref="State"/> escape hatch.</summary>
    public TProjection Query<TProjection>(IWorldQuery<TProjection> query, string observerId)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        return query.Execute(State, observerId);
    }

    /// <summary>The sole write path (ADR 0013): runs <paramref name="command"/> through its own
    /// <paramref name="pipeline"/> against the session's <see cref="WorldState"/>. Adapters and screens
    /// never set a field on a domain object directly, under any circumstance. Also runs <see
    /// cref="ChronicleGenerationSystem.Generate"/> over the command's own events (Phase 11 item 3) —
    /// many Chronicle-worthy facts (a marriage, an adoption, a heir declaration) are player commands
    /// rather than monthly-system output, so <see cref="AdvanceMonth"/> alone would never chronicle
    /// them; the merged result includes any resulting <c>chronicle.*</c> events alongside the
    /// command's own. Also runs <see cref="EpithetGenerationSystem.Generate"/> over the combined batch
    /// (Phase 11 item 5) — an Agnomen or Dynastic Epithet can key off either the command's own events or
    /// the Chronicle entries they just produced, matching that system's own doc comment.</summary>
    public CommandResult Submit<TCommand>(CommandPipeline<WorldState, TCommand> pipeline, TCommand command)
        where TCommand : ICommand
    {
        if (pipeline is null)
            throw new ArgumentNullException(nameof(pipeline));

        var result = pipeline.Execute(State, command);
        if (!result.Accepted)
            return result;

        var chronicleEvents = ChronicleGenerationSystem.Generate(State, result.Events);
        var combined = chronicleEvents.Count == 0 ? result.Events : result.Events.Concat(chronicleEvents).ToArray();
        var epithetEvents = EpithetGenerationSystem.Generate(State, combined);
        return epithetEvents.Count == 0 ? result with { Events = combined } : result with { Events = combined.Concat(epithetEvents).ToArray() };
    }

    /// <summary>Advances the campaign one month, mirroring <c>AdvanceCommand</c>'s pairing of a
    /// <see cref="WriteSetVerifyingSimulation"/> tick with <see cref="WorldState.AdvanceMonth"/> —
    /// including that same command's <see cref="ChronicleGenerationSystem.Generate"/> call (Phase 11
    /// item 3), so the Dynasty Chronicle populates during ordinary play, not only from the
    /// content-compiler CLI. The pause/advance UI (Phase 9 item 8, <c>GensUIController</c>) owns when
    /// to call this; this method only owns the state transition itself. Also runs <see
    /// cref="EpithetGenerationSystem.Generate"/> over the same combined batch (Phase 11 item 5), matching
    /// <see cref="Submit{TCommand}"/>'s identical pairing.</summary>
    public IReadOnlyList<IDomainEvent> AdvanceMonth(IEnumerable<IMonthlySystem<WorldState>> systems)
    {
        if (systems is null)
            throw new ArgumentNullException(nameof(systems));

        var simulation = new WriteSetVerifyingSimulation(systems);
        var tickEvents = simulation.Tick(State, State.Date, RandomStreams);
        var chronicleEvents = ChronicleGenerationSystem.Generate(State, tickEvents);
        var combined = tickEvents.Concat(chronicleEvents).ToArray();
        var epithetEvents = EpithetGenerationSystem.Generate(State, combined);
        State.AdvanceMonth();
        return combined.Concat(epithetEvents).ToArray();
    }
}

/// <summary>The result of <see cref="CampaignSession.VerifyDeterministicReplay"/>: the state hash
/// immediately before saving, the hash after reloading that same save, and whether they matched.
/// </summary>
public readonly record struct ReplayDiagnosticsResult(ulong HashBeforeSave, ulong HashAfterReload, bool Matches);
