using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Correspondence;

/// <summary>The resolved shape of one letter's journey: Distance Tier and transit time (§3's "reuses
/// Travel's own distance model"), the base interception risk that model implies (§9), and whether §7's
/// Oral Tradition Problem touches this particular (culture, action) pairing at all. Deliberately not
/// itself a <see cref="Letter"/> field — like <see cref="TravelRoute"/>, this is a resolved value the
/// caller consults once at send time, not campaign state of its own.</summary>
public sealed record LetterRoute
{
    private LetterRoute(
        DistanceTier distanceTier, int transitTimeMonths, RouteRiskLevel interceptionRisk,
        bool oralTraditionPenaltyApplied, bool blocked)
    {
        DistanceTier = distanceTier;
        TransitTimeMonths = transitTimeMonths;
        InterceptionRisk = interceptionRisk;
        OralTraditionPenaltyApplied = oralTraditionPenaltyApplied;
        Blocked = blocked;
    }

    public DistanceTier DistanceTier { get; }
    public int TransitTimeMonths { get; }

    /// <summary>Bumped up one <see cref="RouteRiskLevel"/> step (capped at <see
    /// cref="RouteRiskLevel.Dangerous"/>) when <see cref="OralTraditionPenaltyApplied"/> — this item's
    /// own concrete, disclosed proxy for §7's "meaningfully reduced Correspondence effectiveness," since
    /// §12 leaves the exact numeric reduction an explicit open question.</summary>
    public RouteRiskLevel InterceptionRisk { get; }

    public bool OralTraditionPenaltyApplied { get; }

    /// <summary>True only when the counterparty's own <see cref="CorrespondenceReachability"/> is <see
    /// cref="CorrespondenceReachability.OralTraditionBlocked"/> and <paramref name="action"/> (see
    /// <see cref="Resolve"/>) is <see cref="LetterActions.IsSubstantive"/> — §7's extreme case, "some
    /// content simply cannot be transmitted this way at all." <see cref="SendLetterCommands"/> and <see
    /// cref="OriginateInboundLetterCommands"/> both reject a command whose resolved route comes back
    /// blocked.</summary>
    public bool Blocked { get; }

    /// <summary><paramref name="foreignCultureId"/> is whichever party in this exchange is not
    /// necessarily Roman/literate-by-default — the recipient's culture for an outbound letter, the
    /// sender's for an inbound one (§7 only ever penalizes the non-literate side of an exchange, never
    /// a literate Roman correspondent writing to another literate Roman).</summary>
    public static LetterRoute Resolve(
        DefinitionId<RegionProfileDefinition> senderRegionId,
        DefinitionId<RegionProfileDefinition> recipientRegionId,
        DefinitionId<Culture>? foreignCultureId,
        LetterAction action,
        CourierType courierType,
        DistanceTierCatalog distanceTiers,
        CorrespondenceReachabilityCatalog reachability)
    {
        if (distanceTiers is null)
            throw new ArgumentNullException(nameof(distanceTiers));
        if (reachability is null)
            throw new ArgumentNullException(nameof(reachability));

        var tier = distanceTiers.Resolve(senderRegionId, recipientRegionId);
        var transitTimeMonths = CourierCatalog.ResolveTransitTimeMonths(courierType, tier);
        var baseRisk = ResolveBaseRisk(tier);

        var level = reachability.Resolve(foreignCultureId);
        var substantive = LetterActions.IsSubstantive(action);
        var penaltyApplied = level != CorrespondenceReachability.FullyLiterate && substantive;
        var blocked = level == CorrespondenceReachability.OralTraditionBlocked && substantive;

        var risk = penaltyApplied ? BumpUpOneStep(baseRisk) : baseRisk;

        return new LetterRoute(tier, transitTimeMonths, risk, penaltyApplied, blocked);
    }

    private static RouteRiskLevel ResolveBaseRisk(DistanceTier tier) => tier switch
    {
        DistanceTier.Near => RouteRiskLevel.Secure,
        DistanceTier.Moderate => RouteRiskLevel.Guarded,
        DistanceTier.Far => RouteRiskLevel.Dangerous,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown distance tier."),
    };

    private static RouteRiskLevel BumpUpOneStep(RouteRiskLevel risk) => risk switch
    {
        RouteRiskLevel.Secure => RouteRiskLevel.Guarded,
        RouteRiskLevel.Guarded => RouteRiskLevel.Dangerous,
        RouteRiskLevel.Dangerous => RouteRiskLevel.Dangerous,
        _ => throw new ArgumentOutOfRangeException(nameof(risk), risk, "Unknown route risk level."),
    };
}
