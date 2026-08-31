namespace Gens.Simulation.Correspondence;

/// <summary>§8's three courier choices — unchanged by name from the superseded first-pass doc, whose
/// §8 itself carries no mechanical detail beyond the heading. Every speed/cost/reliability tradeoff
/// below is this item's own invented mechanic, openly disclosed, matching <see
/// cref="Travel.TravelRoute"/>'s own "invented numbers, openly labeled" precedent for its Travel Time
/// baseline — see <see cref="CourierCatalog"/>'s own doc comment for the actual figures.</summary>
public enum CourierType
{
    /// <summary>The household's own trained messenger — the reliable baseline: no extra coin cost, no
    /// added interception risk beyond the route's own <see cref="Travel.RouteRiskLevel"/>, and transit
    /// time equal to Travel's own Distance-Tier baseline (§3: "reuses Travel's own distance model").</summary>
    Tabellarius,

    /// <summary>A paid third-party carrier: faster than a Tabellarius on <see
    /// cref="Travel.DistanceTier.Moderate"/>/<see cref="Travel.DistanceTier.Far"/> routes, at a real
    /// coin cost and a real trust cost — a hired stranger is more likely to be bribed, robbed, or
    /// simply careless than a household's own man.</summary>
    HiredCarrier,

    /// <summary>The fastest option and the cheapest in coin, at the highest interception risk — a bird
    /// can be shot, lost, or simply fail to arrive, and carries no capacity for anything beyond a short
    /// message (a constraint this item does not yet enforce mechanically, since no letter carries a
    /// length/content-size field).</summary>
    Pigeon,
}
