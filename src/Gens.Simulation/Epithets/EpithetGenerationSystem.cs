using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Epithets;

/// <summary>
/// Turns a batch of already-emitted <see cref="IDomainEvent"/>s — a month's own tick events plus the
/// <see cref="ChronicleEntryRecordedEvent"/>s <see cref="ChronicleGenerationSystem"/> just produced from
/// them — into new <see cref="Agnomen"/> grants and <see cref="DynasticEpithet"/> updates (Phase 11 item
/// 5). Deliberately not an <see cref="Time.IMonthlySystem{TState}"/>, for the exact same reason <see
/// cref="ChronicleGenerationSystem"/> isn't one (see that type's own doc comment): an achievement
/// Agnomen and a Dynastic Epithet both key off <em>this same month's own newly-recorded</em> <see
/// cref="ChronicleEntry"/> records, which only exist once <see cref="ChronicleGenerationSystem.Generate"/>
/// has already run — so this is invoked immediately after it, at the same call sites (the
/// content-compiler CLI's <c>AdvanceCommand</c> and the Unity shell's own <c>CampaignShell.Submit</c>/
/// <c>CampaignShell.AdvanceMonth</c>), over the concatenation of the tick's own events and the Chronicle
/// events they produced.
///
/// Only mints <see cref="AgnomenType.VirtueOrAchievement"/> agnomina — <see cref="AgnomenCatalog"/>'s
/// own doc comment names why the other three types have no real source system to award from yet.
/// </summary>
public static class EpithetGenerationSystem
{
    public static IReadOnlyList<IDomainEvent> Generate(WorldState state, IReadOnlyList<IDomainEvent> events)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var produced = new List<IDomainEvent>();

        foreach (var evt in events)
        {
            switch (evt)
            {
                case SuccessionDisputeResolvedEvent { WinnerCharacterId: { } winnerId } resolved:
                    TryAward(
                        state, produced, winnerId, AgnomenCatalog.SuccessionVictoryAgnomenName, resolved.OccurredDate,
                        sourceChronicleEntryIds: Array.Empty<RuntimeId<ChronicleEntry>>(),
                        sourceSuccessionDisputeId: resolved.DisputeId);
                    break;

                case ChronicleEntryRecordedEvent { Tier: ChronicleTier.Major or ChronicleTier.Legendary } recorded:
                    if (!state.ChronicleEntries.TryGet(recorded.EntryId, out var entry))
                        break;

                    foreach (var characterId in entry!.LinkedCharacterIds)
                        TryAwardAchievement(state, produced, characterId, recorded.OccurredDate);

                    if (recorded.HouseholdId is { } householdId)
                        RecomputeDynasticEpithet(state, produced, householdId, recorded.OccurredDate);
                    break;
            }
        }

        return produced;
    }

    private static void TryAwardAchievement(WorldState state, List<IDomainEvent> produced, RuntimeId<Character> characterId, GameDate date)
    {
        var qualifyingEntryIds = new List<RuntimeId<ChronicleEntry>>();
        foreach (var entry in state.ChronicleEntries.InAscendingOrder())
        {
            if (entry.Value.Tier is not (ChronicleTier.Major or ChronicleTier.Legendary))
                continue;
            if (!entry.Value.LinkedCharacterIds.Contains(characterId))
                continue;
            qualifyingEntryIds.Add(entry.Key);
        }

        if (qualifyingEntryIds.Count < AgnomenCatalog.AchievementChronicleEntryThreshold)
            return;

        TryAward(
            state, produced, characterId, AgnomenCatalog.AchievementAgnomenName, date,
            sourceChronicleEntryIds: qualifyingEntryIds, sourceSuccessionDisputeId: null);
    }

    private static void TryAward(
        WorldState state,
        List<IDomainEvent> produced,
        RuntimeId<Character> characterId,
        string name,
        GameDate date,
        IReadOnlyList<RuntimeId<ChronicleEntry>> sourceChronicleEntryIds,
        RuntimeId<SuccessionDispute>? sourceSuccessionDisputeId)
    {
        foreach (var entry in state.Agnomens.InAscendingOrder())
        {
            if (entry.Value.CharacterId == characterId && string.Equals(entry.Value.Name, name, StringComparison.Ordinal))
                return;
        }

        var agnomenId = state.AgnomenIds.Issue();
        var agnomen = new Agnomen(
            agnomenId, characterId, AgnomenType.VirtueOrAchievement, name, AgnomenGrantMethod.OrganicCrowdOrigin, date,
            sourceChronicleEntryIds, sourceSuccessionDisputeId, DignitasEffect: null, FameEffect: null, IsSuppressible: false);
        state.Agnomens.Add(agnomenId, agnomen);

        produced.Add(new AgnomenGrantedEvent(state.EventIds.Issue(), date, agnomenId, characterId, name, CausationId: null));
    }

    private static void RecomputeDynasticEpithet(WorldState state, List<IDomainEvent> produced, RuntimeId<Household> householdId, GameDate date)
    {
        var qualifying = new List<RuntimeId<ChronicleEntry>>();
        var countByCategory = new Dictionary<ChronicleCategory, int>();
        foreach (var entry in state.ChronicleEntries.InAscendingOrder())
        {
            if (entry.Value.HouseholdId != householdId || entry.Value.Tier is not (ChronicleTier.Major or ChronicleTier.Legendary))
                continue;
            qualifying.Add(entry.Key);
            countByCategory[entry.Value.Category] = countByCategory.GetValueOrDefault(entry.Value.Category) + 1;
        }

        if (qualifying.Count < DynasticEpithetCatalog.MinimumMajorOrLegendaryEntries)
            return;

        var dominantCategory = countByCategory
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => (int)pair.Key)
            .First().Key;
        var epithetText = DynasticEpithetCatalog.TemplateFor(dominantCategory);

        if (state.DynasticEpithets.TryGet(householdId, out var existing) &&
            string.Equals(existing!.EpithetText, epithetText, StringComparison.Ordinal))
            return;

        if (existing is not null)
            state.DynasticEpithets.Remove(householdId);
        state.DynasticEpithets.Add(householdId, new DynasticEpithet(householdId, epithetText, qualifying));

        produced.Add(new DynasticEpithetChangedEvent(state.EventIds.Issue(), date, householdId, epithetText, CausationId: null));
    }
}
