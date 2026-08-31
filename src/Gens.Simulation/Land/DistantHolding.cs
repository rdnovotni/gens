using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Land;

/// <summary>
/// <c>gens-starting-regions-design.md</c> §12's <c>DistantHolding{}</c> shape (Phase 13 item 7): the
/// record of one household <see cref="Holding"/> that sits outside that household's own home region,
/// closing §7's "a household should be able to acquire a second holding outside its home region"
/// against a real <see cref="State.WorldState"/> partition — a distant holding's own Procurator
/// staffing and mismanagement-risk state is genuine campaign state, not content, matching <see
/// cref="Travel.TravelTrip"/>'s and <see cref="Correspondence.Letter"/>'s identical reasoning.
/// </summary>
public sealed record DistantHolding
{
    private DistantHolding()
    {
    }

    public required RuntimeId<DistantHolding> Id { get; init; }
    public required RuntimeId<Household> HouseholdId { get; init; }
    public required DefinitionId<RegionProfileDefinition> HomeRegionId { get; init; }
    public required DefinitionId<RegionProfileDefinition> HoldingRegionId { get; init; }
    public required RuntimeId<Holding> HoldingId { get; init; }
    public required DistanceTier DistanceTier { get; init; }

    /// <summary>§7.2: "strongly recommended; absence flagged as a risk state" — null while unstaffed.
    /// The Procurator's own <see cref="Stewardship.StewardshipAssignment"/> (<see
    /// cref="Stewardship.StewardshipContext.SecondSettlementProcurator"/>) is the authority on *who*
    /// currently holds the appointment; this field is this holding's own cached pointer to that same
    /// Character, kept in sync by <see cref="DistantHoldingMismanagementRiskSystem"/>.</summary>
    public RuntimeId<Character>? ProcuratorCharacterId { get; init; }

    /// <summary>§7.2/§12: true exactly when this holding is <see cref="Travel.DistanceTier.Far"/> and
    /// either unstaffed or staffed by a Procurator whose Loyalty has fallen below <see
    /// cref="Stewardship.StewardIncidentCatalog.LoyaltyRiskThreshold"/> — the same Loyalty-risk
    /// threshold <see cref="Stewardship.StewardAutonomousDecisionSystem"/> already uses for exactly
    /// this "is the person running things unsupervised a real liability" question, reused rather than
    /// inventing a second one. A Near or Moderate holding never carries this risk (§7.2's "a Near
    /// second holding wouldn't"), whether or not it has a Procurator.</summary>
    public bool MismanagementRiskActive { get; init; }

    /// <summary>Registers a newly-acquired distant holding (<see cref="AcquireDistantHoldingCommand"/>),
    /// always unstaffed at the moment of acquisition — §7.2's mismanagement risk applies immediately
    /// for a <see cref="Travel.DistanceTier.Far"/> acquisition rather than waiting for the caller to
    /// notice no Procurator was ever appointed.</summary>
    public static DistantHolding Begin(
        RuntimeId<DistantHolding> id,
        RuntimeId<Household> householdId,
        DefinitionId<RegionProfileDefinition> homeRegionId,
        DefinitionId<RegionProfileDefinition> holdingRegionId,
        RuntimeId<Holding> holdingId,
        DistanceTier distanceTier) =>
        new()
        {
            Id = id,
            HouseholdId = householdId,
            HomeRegionId = homeRegionId,
            HoldingRegionId = holdingRegionId,
            HoldingId = holdingId,
            DistanceTier = distanceTier,
            ProcuratorCharacterId = null,
            MismanagementRiskActive = distanceTier == DistanceTier.Far,
        };

    /// <summary>Reconstructs a <see cref="DistantHolding"/> from persisted save data (ADR 0010) —
    /// mirrors <see cref="Correspondence.Letter.Restore"/>'s identical "the mapper's own restore path"
    /// shape.</summary>
    public static DistantHolding Restore(
        RuntimeId<DistantHolding> id,
        RuntimeId<Household> householdId,
        DefinitionId<RegionProfileDefinition> homeRegionId,
        DefinitionId<RegionProfileDefinition> holdingRegionId,
        RuntimeId<Holding> holdingId,
        DistanceTier distanceTier,
        RuntimeId<Character>? procuratorCharacterId,
        bool mismanagementRiskActive) =>
        new()
        {
            Id = id,
            HouseholdId = householdId,
            HomeRegionId = homeRegionId,
            HoldingRegionId = holdingRegionId,
            HoldingId = holdingId,
            DistanceTier = distanceTier,
            ProcuratorCharacterId = procuratorCharacterId,
            MismanagementRiskActive = mismanagementRiskActive,
        };
}
