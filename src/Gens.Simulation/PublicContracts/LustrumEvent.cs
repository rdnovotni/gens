using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §3's/§8's <c>LustrumEvent</c> data model (Phase 15 item 6) — one fired instance of "every 60 months,
/// a Lustrum fires... a full Net Worth reassessment across every tracked household, and a mandatory
/// re-bidding of every standing contract." <see cref="HouseholdsReassessed"/> is a real, checkable
/// snapshot of Economy &amp; Finance's own already-monthly <see cref="Economy.NetWorth"/> computation
/// (<see cref="Economy.InsolvencySystem"/> already recomputes every tracked household's Net Worth every
/// month, not just at a Lustrum — see <see cref="LustrumSystem"/>'s own doc comment for why this item
/// snapshots that existing figure rather than recomputing it a second way) at the moment the Lustrum
/// fires, not a second, independent reassessment.
/// </summary>
public sealed record LustrumEvent(
    RuntimeId<LustrumEvent> LustrumId,
    GameDate Month,
    IReadOnlyList<RuntimeId<Household>> HouseholdsReassessed,
    IReadOnlyList<RuntimeId<PublicContract>> ContractsReopenedForBid);
