using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>An immutable snapshot of every <see cref="WorldState"/> partition's size/next-ID, for the
/// headless console runner's debug inspectors (Phase 4 item 6). Deliberately does not include RNG
/// stream state or the content-pack hash — those live in <see cref="Saves.SaveManifest"/>, outside
/// <see cref="WorldState"/>, and are the console runner's job to report alongside this snapshot, not
/// this query's.</summary>
public sealed record CampaignDebugSnapshot(
    int DateTotalMonths,
    string DisplayYearLabel,
    long RegionIds,
    long SettlementIds,
    long PlotIds,
    long HouseholdIds,
    long ActorIds,
    long CharacterIds,
    long BuildingIds,
    long ContractIds,
    long ActivityIds,
    long CommandIds,
    long EventIds,
    long ScheduledActionIds,
    int CharacterCount,
    int ScheduledActionCount,
    int PopGroupCount,
    int KnowledgeEntryCount,
    long NextCommandSequenceNumber,
    ulong StateHash);

/// <summary>Worked debug-inspector example (ADR 0013's read-boundary discipline, applied even in a
/// console/CLI context rather than Unity UI): every field is read from <see cref="WorldState"/>'s
/// existing public partitions, never a bespoke traversal.</summary>
public sealed class CampaignDebugQuery : IWorldQuery<CampaignDebugSnapshot>
{
    public CampaignDebugSnapshot Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return new CampaignDebugSnapshot(
            DateTotalMonths: state.Date.TotalMonths,
            DisplayYearLabel: state.Date.ToDisplayYearLabel(),
            RegionIds: state.RegionIds.Peek,
            SettlementIds: state.SettlementIds.Peek,
            PlotIds: state.PlotIds.Peek,
            HouseholdIds: state.HouseholdIds.Peek,
            ActorIds: state.ActorIds.Peek,
            CharacterIds: state.CharacterIds.Peek,
            BuildingIds: state.BuildingIds.Peek,
            ContractIds: state.ContractIds.Peek,
            ActivityIds: state.ActivityIds.Peek,
            CommandIds: state.CommandIds.Peek,
            EventIds: state.EventIds.Peek,
            ScheduledActionIds: state.ScheduledActionIds.Peek,
            CharacterCount: state.Characters.Count,
            ScheduledActionCount: state.ScheduledActions.Count,
            PopGroupCount: state.PopGroups.Count,
            KnowledgeEntryCount: state.Knowledge.All().Count(),
            NextCommandSequenceNumber: state.NextCommandSequenceNumber,
            StateHash: StateHasher.Hash(state));
    }
}
