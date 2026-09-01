using Gens.Simulation.Buildings;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Hazards;

/// <summary>Pure, RNG-free math for §5.2/§5.3/§5.4's damage magnitude — how many <see
/// cref="BuildingCondition"/> steps a struck building loses, and what fraction of a settlement's
/// affected background population a Catastrophic structural Event costs. Every figure is this
/// implementation's own invented number (§9's own "All numeric sizing" open question, same citation
/// every other calculator in this namespace uses). <see cref="Buildings.BuildingInstance.Repair"/>
/// (already built, Phase 6 item 7) is the entire recovery mechanism these steps feed — §5.2's own "this
/// document supplies the trigger and severity, not a parallel repair system," taken literally: nothing
/// in this file writes a building's condition itself, only how many steps <see
/// cref="NaturalDisasterSystem"/> should apply via <see
/// cref="Buildings.BuildingInstance.ApplyDisasterDamage"/>.</summary>
public static class DisasterDamageCalculator
{
    /// <summary>§5.2's condition drop "scaled to severity." A Catastrophic result always costs enough
    /// steps to push even a <see cref="BuildingCondition.Pristine"/> building to <see
    /// cref="BuildingCondition.Ruined"/> in one blow (§5.2's own "a Catastrophic result can push
    /// condition all the way to Destroyed"), since <see cref="BuildingCondition"/> only spans four
    /// non-Ruined steps.</summary>
    public static int BuildingConditionStepsLost(DisasterSeverity severity) => severity switch
    {
        DisasterSeverity.Minor => 1,
        DisasterSeverity.Moderate => 1,
        DisasterSeverity.Severe => 2,
        DisasterSeverity.Catastrophic => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unhandled disaster severity."),
    };

    /// <summary>§5.4's Frost-specific perennial-crop recovery tail: "sets that building's own output
    /// back to something closer to its earliest production tier rather than merely docking a season's
    /// yield." No Olive Grove/Vineyard building or output-tier concept exists anywhere in this codebase
    /// yet (<see cref="HazardExposureCalculator.FrostExposure"/>'s own disclosure) — the closest real,
    /// mechanically distinct proxy this item can build is applying <see
    /// cref="BuildingConditionStepsLost"/>'s own harshest (Catastrophic-equivalent) condition drop the
    /// moment Frost resolves at <see cref="DisasterSeverity.Severe"/> or above, rather than that
    /// severity's own ordinarily milder step count — a genuinely longer, more painful setback than any
    /// other hazard's own Severe-tier damage, matching §5.4's "longer, more painful recovery tail" in
    /// substance if not in its own dedicated multi-year mechanic.</summary>
    public static int FrostBuildingConditionStepsLost(DisasterSeverity severity) =>
        severity >= DisasterSeverity.Severe
            ? BuildingConditionStepsLost(DisasterSeverity.Catastrophic)
            : BuildingConditionStepsLost(severity);

    /// <summary>§5.3's "a Catastrophic Fire, Flood, Landslide, or eruption in a dense district can cause
    /// genuine population loss" — extended uniformly to every Catastrophic structural hazard this item
    /// models (Fire, Flood, Earthquake, Storm, Landslide, Volcanic Eruption), not only the four §5.3
    /// names as illustrative examples. Zero below Catastrophic: Minor/Moderate/Severe read as real but
    /// survivable, matching §5.1's own severity framing. A flat fraction of the affected <see
    /// cref="Characters.PopGroup.Size"/>, applied by the caller the same way <see
    /// cref="Characters.GrowthMortalitySystem"/> already applies its own monthly rate — real population
    /// loss with a real cause, not a cosmetic Contentment-only hit.</summary>
    public static double CatastrophicPopulationLossFraction(DisasterSeverity severity) =>
        severity == DisasterSeverity.Catastrophic ? 0.05 : 0.0;

    /// <summary>The independent per-<see cref="Buildings.BuildingInstance"/> chance that <see
    /// cref="NaturalDisasterSystem"/> rolls it individually struck once a structural Event has fired on
    /// its plot — §5.2/§5.3's own "buildings" plural read as "not automatically every building on the
    /// affected terrain," the same "a real Event, not a uniform area wipe" framing every other roll in
    /// this namespace already carries. This implementation's own invented number (§9's "All numeric
    /// sizing" open question).</summary>
    public static double BuildingHitProbability(DisasterSeverity severity) => severity switch
    {
        DisasterSeverity.Minor => 0.15,
        DisasterSeverity.Moderate => 0.30,
        DisasterSeverity.Severe => 0.55,
        DisasterSeverity.Catastrophic => 0.90,
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unhandled disaster severity."),
    };

    /// <summary>§5.3's "Settlement Demographics' Contentment takes a direct hit proportional to
    /// severity" — a same-month <see cref="Characters.PopGroup.Contentment"/> shock <see
    /// cref="NaturalDisasterSystem"/> writes directly, deliberately not folded into <see
    /// cref="Characters.ContentmentCalculator.ComputeContentment"/>'s own three-input formula (that
    /// calculator's own doc comment already names "Natural Disasters... inputs" as a future adjuster it
    /// doesn't yet integrate) — <c>Characters.ContentmentSystem</c> runs earlier in the same monthly tick
    /// (<see cref="Time.TickPhase.EmploymentNeeds"/> precedes <see cref="Time.TickPhase.Hazards"/>) and
    /// will recompute Contentment from scratch next month, so this hit reads as a real, felt shock the
    /// month it happens rather than a silently persistent debuff — an honest, disclosed limitation
    /// rather than a fabricated fourth Contentment input. Every hazard type applies this, including the
    /// three (Drought/Famine, Blight &amp; Infestation, and any Frost below Severe) that never touch a
    /// building at all, per §5.3's "act on yield and Contentment rather than physical structures"
    /// framing.</summary>
    public static Fixed64 ContentmentImpact(DisasterSeverity severity) => severity switch
    {
        DisasterSeverity.Minor => Fixed64.FromRaw(-20_000), // -0.02.
        DisasterSeverity.Moderate => Fixed64.FromRaw(-50_000), // -0.05.
        DisasterSeverity.Severe => Fixed64.FromRaw(-120_000), // -0.12.
        DisasterSeverity.Catastrophic => Fixed64.FromRaw(-250_000), // -0.25.
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unhandled disaster severity."),
    };
}
