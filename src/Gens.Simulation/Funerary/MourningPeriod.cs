using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>
/// A household's <c>luctus</c> — the socially binding mourning period following a death (Phase 11
/// item 4; §4.1, §9's <c>MourningPeriod</c> data model). Sparse and keyed by household, like <see
/// cref="Succession.HouseholdHeadship"/>: at most one active period per household at a time, and a
/// second death during an already-active period extends rather than stacks (see <see
/// cref="FuneralOpeningSystem"/>). Deliberately covers only §4.1's household-wide <c>luctus</c> — the
/// widow's <c>tempus lugendi</c> (§4.2) and the settlement-scale <c>iustitium</c> (§4.3) are both named
/// directly by the roadmap task as "more speculative extensions... only build if cleanly reusing
/// existing state", and neither does: §4.2 needs Romance, Sexuality &amp; Lineage's remarriage-timing
/// machinery (not built yet) to actually gate anything, and §4.3 needs Politics &amp; Patronage's
/// Prominence gate (also not built yet) to decide who may even declare one — both are left as
/// documented gaps in this namespace's own roadmap progress note rather than half-built here.
/// </summary>
/// <param name="BrokenEarly">Set by <see cref="BreakMourningEarlyCommand"/> when a household visibly
/// breaks its own mourning (§4.1's "dancing on the grave" example) before <see cref="EndDate"/>. The
/// real consequence — a Scandal (Scandal §4, Phase 12, not yet built) — cannot fire yet; this flag is
/// the documented hook a future Scandal integration reads directly, matching how this same
/// design doc names the same gap for itself (§8's "an early-broken mourning period... is a real, new
/// Scandal source").</param>
public sealed record MourningPeriod(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> TriggeringDeathCharacterId,
    GameDate StartDate,
    GameDate EndDate,
    bool BrokenEarly = false)
{
    /// <summary>Whether <paramref name="date"/> falls within this period's own window, inclusive —
    /// the read <see cref="ManesObservanceSystem"/> and any future consumer use rather than each
    /// re-deriving the same month comparison.</summary>
    public bool IsActiveOn(GameDate date) => date.TotalMonths >= StartDate.TotalMonths && date.TotalMonths <= EndDate.TotalMonths;
}
