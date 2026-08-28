using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;

namespace Gens.Simulation.Chronicle;

/// <summary>
/// Turns one month's already-emitted <see cref="IDomainEvent"/>s into persisted <see
/// cref="ChronicleEntry"/> entries (Phase 11 item 3), mints/updates the household's <see
/// cref="GenerationalChapter"/>s (§4), and cross-posts a Major/Legendary entry to a rival's own <see
/// cref="RivalDossier"/> (§9) — replacing that record's own "no Dynasty Chronicle record exists yet"
/// plain-string stopgap with a real <see cref="RuntimeId{ChronicleEntry}"/> reference.
///
/// Deliberately not an <see cref="Time.IMonthlySystem{TState}"/>: that interface's <c>Tick(state,
/// context)</c> never receives the events other same-month systems just emitted (ADR 0005's phases
/// communicate through mutated <c>WorldState</c>, not a shared event list), and per ADR 0007 "no
/// system reconstructs 'what happened this month' by re-reading another system's raw WorldState
/// partition — it reads the event log." <see cref="Generate"/> is invoked directly by whoever already
/// collects a tick's full event list — the content-compiler CLI's own <c>AdvanceCommand</c> and its
/// <c>allEvents</c> accumulation is exactly that caller — immediately after <see
/// cref="MonthlySimulation{TState}.Tick"/> returns, mirroring how <see
/// cref="Campaign.MonthlyReportProjector.Project"/> is already invoked from that same call site.
/// </summary>
public static class ChronicleGenerationSystem
{
    public static IReadOnlyList<IDomainEvent> Generate(WorldState state, IReadOnlyList<IDomainEvent> monthEvents)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (monthEvents is null)
            throw new ArgumentNullException(nameof(monthEvents));

        var produced = new List<IDomainEvent>();

        foreach (var draft in ChronicleProjector.Project(state, monthEvents))
        {
            var entryId = state.ChronicleEntryIds.Issue();
            var entry = new ChronicleEntry(
                entryId,
                draft.HouseholdId,
                draft.Month,
                draft.Category,
                draft.Tier,
                draft.Prose,
                draft.LinkedCharacterIds,
                draft.SourceSystem,
                ChronicleEntrySource.System);
            state.ChronicleEntries.Add(entryId, entry);

            produced.Add(new ChronicleEntryRecordedEvent(
                state.EventIds.Issue(), draft.Month, entryId, draft.HouseholdId, draft.Tier, draft.SourceEventId));

            if (draft.Tier is ChronicleTier.Major or ChronicleTier.Legendary)
                CrossPostToRivals(state, draft, entryId);
        }

        UpdateChapters(state, monthEvents);

        return produced;
    }

    /// <summary>§9: "a House of Note maintains its own lightweight Chronicle, generally populated at
    /// Major/Legendary tier only." Posts directly to the named actor for an actor-only draft (<see
    /// cref="ChronicleEntryDraft.RivalActorId"/>, e.g. a rival house's own extinction), or to whichever
    /// linked Character resolves to a tracked <see cref="LivingWorldActor"/> head otherwise — <see
    /// cref="RivalDossierRefresh.RefreshForCharacter"/> is already a no-op for every Character that
    /// isn't one, so this can run unconditionally over every linked Character.</summary>
    private static void CrossPostToRivals(WorldState state, ChronicleEntryDraft draft, RuntimeId<ChronicleEntry> entryId)
    {
        if (draft.RivalActorId is { } actorId)
        {
            RivalDossierRefresh.Refresh(state, actorId, draft.Month, draft.Prose, entryId);
            return;
        }

        foreach (var characterId in draft.LinkedCharacterIds)
            RivalDossierRefresh.RefreshForCharacter(state, characterId, draft.Month, draft.Prose, entryId);
    }

    private static void UpdateChapters(WorldState state, IReadOnlyList<IDomainEvent> events)
    {
        foreach (var evt in events)
        {
            switch (evt)
            {
                case HouseholdHeadEstablishedEvent established:
                    OpenChapter(
                        state, established.HouseholdId, established.HeadCharacterId, established.OccurredDate,
                        $"{ChronicleProjector.Name(state, established.HeadCharacterId)} became head of the household.");
                    break;

                case HouseholdHeadTransferredEvent transferred:
                    CloseChapter(state, transferred.HouseholdId, transferred.OccurredDate);
                    OpenChapter(
                        state, transferred.HouseholdId, transferred.ToCharacterId, transferred.OccurredDate,
                        $"{ChronicleProjector.Name(state, transferred.ToCharacterId)} took up the headship of the household, succeeding {ChronicleProjector.Name(state, transferred.FromCharacterId)}.");
                    break;

                case HouseholdExtinguishedEvent extinguished:
                    CloseChapter(state, extinguished.HouseholdId, extinguished.OccurredDate);
                    break;
            }
        }
    }

    private static void OpenChapter(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> headCharacterId, GameDate startMonth, string summary)
    {
        var key = new GenerationalChapterKey(householdId, startMonth.TotalMonths);
        if (state.GenerationalChapters.TryGet(key, out _))
            return;

        state.GenerationalChapters.Add(key, new GenerationalChapter(householdId, headCharacterId, startMonth, null, summary));
    }

    private static void CloseChapter(WorldState state, RuntimeId<Household> householdId, GameDate endMonth)
    {
        GenerationalChapterKey? openKey = null;
        foreach (var entry in state.GenerationalChapters.InAscendingOrder())
        {
            if (entry.Key.HouseholdId == householdId && entry.Value.EndMonth is null)
            {
                openKey = entry.Key;
                break;
            }
        }

        if (openKey is not { } key || !state.GenerationalChapters.TryGet(key, out var chapter))
            return;

        state.GenerationalChapters.Remove(key);
        state.GenerationalChapters.Add(key, chapter with { EndMonth = endMonth });
    }
}
