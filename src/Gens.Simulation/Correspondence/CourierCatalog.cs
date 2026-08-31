using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>One <see cref="CourierType"/>'s invented tradeoff profile — see <see
/// cref="CourierType"/>'s own doc comment for why these figures exist at all. <see
/// cref="InterceptionRiskModifierPercent"/> is added to the route's own base interception chance (<see
/// cref="LetterRoute"/>); <see cref="SilverCostPerLetter"/> is informational only — nothing here posts
/// it to the Ledger (Phase 8), matching Travel's own "the Retinue mechanic itself stays Companions
/// &amp; Court Positions' job" boundary for a cost this item names but does not yet actually
/// charge.</summary>
public sealed record CourierProfile(int InterceptionRiskModifierPercent, int SilverCostPerLetter);

/// <summary>The general courier lookup mechanism §8 asks for. Every figure here is this item's own
/// invented baseline, openly disclosed — not sized by the design corpus.</summary>
public static class CourierCatalog
{
    public static CourierProfile Resolve(CourierType type) => type switch
    {
        CourierType.Tabellarius => new CourierProfile(InterceptionRiskModifierPercent: 0, SilverCostPerLetter: 0),
        CourierType.HiredCarrier => new CourierProfile(InterceptionRiskModifierPercent: 10, SilverCostPerLetter: 25),
        CourierType.Pigeon => new CourierProfile(InterceptionRiskModifierPercent: 20, SilverCostPerLetter: 5),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown courier type."),
    };

    /// <summary>Transit time per (courier, Distance Tier) pair. <see cref="CourierType.Tabellarius"/>
    /// always matches <see cref="TravelRoute"/>'s own 1/3/6-month baseline exactly (§3's "reuses
    /// Travel's own distance model"); <see cref="CourierType.HiredCarrier"/> and <see
    /// cref="CourierType.Pigeon"/> trade the Tabellarius's reliability for real speed, per <see
    /// cref="CourierType"/>'s own doc comment.</summary>
    public static int ResolveTransitTimeMonths(CourierType type, DistanceTier tier) => (type, tier) switch
    {
        (CourierType.Tabellarius, DistanceTier.Near) => 1,
        (CourierType.Tabellarius, DistanceTier.Moderate) => 3,
        (CourierType.Tabellarius, DistanceTier.Far) => 6,
        (CourierType.HiredCarrier, DistanceTier.Near) => 1,
        (CourierType.HiredCarrier, DistanceTier.Moderate) => 2,
        (CourierType.HiredCarrier, DistanceTier.Far) => 4,
        (CourierType.Pigeon, DistanceTier.Near) => 1,
        (CourierType.Pigeon, DistanceTier.Moderate) => 1,
        (CourierType.Pigeon, DistanceTier.Far) => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown distance tier."),
    };
}
