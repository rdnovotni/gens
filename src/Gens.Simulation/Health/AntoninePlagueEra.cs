using Gens.Simulation.History;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Health;

/// <summary>§9's Antonine Plague — "a real, dated, Empire-wide Tier 2 Imperial Event... historically AD
/// 165-180, modeled as a multi-year Event Chain elevating Pestilence Exposure everywhere regardless of
/// individual household preparation." Phase 14 item 3's own progress note left this as item 5's own
/// closed gap; item 1/2's <see cref="EpidemicOutbreak.ImperialScale"/> flag was built specifically, and
/// disclosed as callerless, for exactly this trigger.
///
/// <para><b>Modeled as a deterministic historical-era read, not a full interactive multi-stage <see
/// cref="Events.EventDefinition"/>.</b> §9 itself frames the date as foreknown ("player foreknowledge of
/// its real date remains a deliberate feature," Events §6.3) rather than a player choice — there is no
/// option for the player to pick, nothing to author eligibility/AI-scoring/resolve delegates against, so
/// building the full <see cref="Events.EventStageDefinition"/> option-menu machinery <see
/// cref="History.KnownWorldHistoricalTimeline"/>'s own doc comment already disclaims authoring in full
/// for its roughly ninety entries would add UI-facing option content this item doesn't own, not a
/// realer Event Chain. The Chain instead lives here, in code, as the same "two dated points, a start and
/// an end, driving a real cross-system effect between them" shape a multi-stage <see
/// cref="Events.EventDefinition"/> would model with two stages — <see cref="IsActive"/> is the live read
/// every system below consults, and <see cref="EpidemicContagionSystem"/> emits a real, once-only <see
/// cref="AntoninePlagueOnsetEvent"/>/<see cref="AntoninePlagueWaningEvent"/> pair at the two boundary
/// months, the honest realization of "a multi-year Event Chain" for a scripted historical certainty
/// rather than a branching player decision. The existing <c>antonine-plague</c> <see
/// cref="HistoricalTimelineEntryDefinition"/> (<see cref="KnownWorldHistoricalTimeline"/>) is left
/// unlinked, unchanged from item 3/Phase 13 — its own lightweight digest line still fires from <see
/// cref="HistoricalTimelineScheduler"/> the same month <see cref="Start"/> begins, alongside this file's
/// own real mechanical onset.</para>
///
/// <para><b>Deliberately not modeled</b>, matching this namespace's own disclosure discipline: §9's "a
/// returning campaign or Roman Service Character flagged as the real historical introduction vector" —
/// no Military &amp; Combat campaign-return or Roman Service concept exists anywhere in this codebase for
/// this item to read; the onset below fires purely off <see cref="WorldState.Date"/>, honestly narrower
/// than a per-Character introduction vector.</para></summary>
public static class AntoninePlagueEra
{
    /// <summary>AD 165, January — §9's own historical start year, converted the same way every other
    /// real date in this codebase is (<see cref="HistoricalYear.ToGameDate"/>).</summary>
    public static readonly GameDate Start = HistoricalYear.ToGameDate(165, isBce: false);

    /// <summary>AD 180, December — §9's own historical end year's last month; <see cref="IsActive"/>
    /// treats the whole of AD 180 as still within the era, matching how <see
    /// cref="History.HistoricalTimelineEntryDefinition.Date"/> entries default to a year's own January
    /// without asserting month-level precision beyond it.</summary>
    public static readonly GameDate End = HistoricalYear.ToGameDate(180, isBce: false, monthOfYear: 12);

    /// <summary>True for every month from <see cref="Start"/> through <see cref="End"/> inclusive — the
    /// live read every cross-system effect below consults instead of a stored flag, matching
    /// <c>Hazards.HazardExposureProfile</c>'s own "computed on demand, never a snapshot that could drift"
    /// precedent: this needs no <c>WorldState</c> partition or migration at all, since <see
    /// cref="WorldState.Date"/> alone already answers it deterministically.</summary>
    public static bool IsActive(GameDate date) => date.TotalMonths >= Start.TotalMonths && date.TotalMonths <= End.TotalMonths;
}
