using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>One settlement's standing record of a fired Disaster Event — §8's <c>DisasterEvent</c> data
/// model, scoped to the fields this codebase can actually maintain, the same "real record, honestly
/// narrowed" discipline <c>Health.EpidemicOutbreak</c>'s own doc comment already established. §8's own
/// <c>affectedPlotIds</c>/<c>livestockLoss</c>/<c>cargoOrVesselLoss</c> fields are not carried here: no
/// Pasture/livestock or vessel/cargo concept exists anywhere in this codebase yet (this namespace's own
/// top-level disclosures). <c>reliefFundedActionRef</c> is item 5's own <see cref="ReliefFunded"/>
/// below, honestly narrowed to a bool rather than a stored reference — see
/// cref="BuildingsDamaged"/> and <see cref="PopulationLost"/> are this item's own real, aggregate
/// substitute for "which specific plots/buildings," kept as counts rather than ID lists since <see
/// cref="NaturalDisasterSystem"/> already applies the actual per-building/per-PopGroup mutation
/// directly and this record's own job is a queryable history entry, not a second copy of that
/// state. Kept in <c>WorldState</c> forever once fired, matching <c>Health.EpidemicOutbreak</c>'s
/// identical "resolved or not, kept for the campaign's lifetime" convention (<c>Events.EventInstances</c>'s
/// own precedent).</summary>
public sealed record DisasterEvent
{
    public required RuntimeId<DisasterEvent> Id { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required GameDate OccurredDate { get; init; }
    public required HazardType HazardType { get; init; }
    public required DisasterSeverity Severity { get; init; }

    /// <summary>§3.1/§8's <c>triggeredByCompounding</c> — true only for a Flood fired directly by <see
    /// cref="DisasterCompoundingCalculator.StormToFloodChainProbability"/>'s own chain roll, never for
    /// an ordinarily-rolled Event of any hazard.</summary>
    public bool TriggeredByCompounding { get; init; }

    /// <summary>How many <see cref="Buildings.BuildingInstance"/>s this Event applied <see
    /// cref="Buildings.BuildingInstance.ApplyDisasterDamage"/> to. Zero for the three hazards §5.3's own
    /// framing keeps off physical structures entirely (Drought/Famine, Blight &amp; Infestation) — see
    /// <see cref="NaturalDisasterSystem"/>'s own doc comment for exactly which hazards touch buildings.</summary>
    public int BuildingsDamaged { get; init; }

    /// <summary>§5.3's Catastrophic-only population loss (<see
    /// cref="DisasterDamageCalculator.CatastrophicPopulationLossFraction"/>), applied directly to the
    /// affected <see cref="Characters.PopGroup.Size"/> the same tick this record is created.</summary>
    public int PopulationLost { get; init; }

    /// <summary>§5.4's Frost-specific perennial-crop recovery-tail flag — true only when <see
    /// cref="HazardType"/> is <see cref="Hazards.HazardType.Frost"/> and <see cref="Severity"/> reached
    /// <see cref="DisasterSeverity.Severe"/> or above, mirroring exactly when <see
    /// cref="DisasterDamageCalculator.FrostBuildingConditionStepsLost"/> applies its own harsher
    /// condition drop.</summary>
    public bool PerennialCropSetback { get; init; }

    /// <summary>§8's <c>reliefFundedActionRef</c>, honestly narrowed the same way <see
    /// cref="PerennialCropSetback"/> already is: a bool rather than a stored reference, since <see
    /// cref="Policies.FundDisasterReliefCommand"/>'s own <see
    /// cref="Policies.DisasterReliefFundedEvent.TransactionId"/> is already the queryable receipt of
    /// *which* funded action paid for relief — this field only answers "has this Event already had one."
    /// Additive (ADR 0011): defaults false for every save predating item 5.</summary>
    public bool ReliefFunded { get; init; }

    public static DisasterEvent Create(
        RuntimeId<DisasterEvent> id,
        RuntimeId<Settlement> settlementId,
        GameDate occurredDate,
        HazardType hazardType,
        DisasterSeverity severity,
        bool triggeredByCompounding = false,
        int buildingsDamaged = 0,
        int populationLost = 0,
        bool perennialCropSetback = false,
        bool reliefFunded = false)
    {
        if (buildingsDamaged < 0)
            throw new ArgumentOutOfRangeException(nameof(buildingsDamaged), buildingsDamaged, "Buildings damaged cannot be negative.");
        if (populationLost < 0)
            throw new ArgumentOutOfRangeException(nameof(populationLost), populationLost, "Population lost cannot be negative.");

        return new DisasterEvent
        {
            Id = id,
            SettlementId = settlementId,
            OccurredDate = occurredDate,
            HazardType = hazardType,
            Severity = severity,
            TriggeredByCompounding = triggeredByCompounding,
            BuildingsDamaged = buildingsDamaged,
            PopulationLost = populationLost,
            PerennialCropSetback = perennialCropSetback,
            ReliefFunded = reliefFunded,
        };
    }
}
