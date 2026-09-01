namespace Gens.Simulation.Hazards;

/// <summary>Pure, RNG-free monthly Exposure math for §3's eight standing hazards (<see
/// cref="HazardType.VolcanicEruption"/> excepted — see <see cref="DormantVolcano"/>), one function per
/// hazard, extending <c>Health.EndemicExposureCalculator</c>'s own "documented as invented, pending
/// playtesting" precedent to this design doc's identical §9 "All numeric sizing" open question. Every
/// score is a deterministic 0-100 read off real, already-computable inputs (<see
/// cref="HazardExposureProfile"/> assembles them once per settlement per tick) — never a player-set
/// slider, per §3's own "an emergent number read off terrain, buildings, and land-use choices"
/// framing. Three tracked drivers §4 names outright — Soil Fertility, Forest Cover, and Slope
/// Stability — do not exist as real, settable player mechanics anywhere in this codebase yet (no
/// Cultivation/Harvest/Excavation Intensity lever exists at any building), and are Phase 14 item 4/a
/// future item's own territory per this task's own scoping note; every function below that would read
/// one instead reads the closest real, already-computable terrain proxy this codebase has today,
/// disclosed individually in that function's own doc comment — the same "honest, disclosed proxy
/// instead of a faked track" discipline <c>Health.DiseaseCatalog</c> already established for Saturnism's
/// mining driver and Gout's wealth driver.</summary>
public static class HazardExposureCalculator
{
    private const int MaxExposure = 100;

    /// <summary>§2's insulae/urban building density driver. <paramref name="buildingDensity"/> is a
    /// settlement's completed-building count divided by its total Plot capacity — the real, already-
    /// computable proxy for "how densely packed this settlement's own building stock is," since no
    /// Insulae-specific building type or density track exists yet. <paramref name="drySeasonBonus"/> is
    /// §3.1's dry-season overlap, added by the caller (<see
    /// cref="DisasterCompoundingCalculator.DrySeasonFireExposureBonus"/>) rather than computed here, so
    /// this function stays a pure function of density alone.</summary>
    public static int FireExposure(double buildingDensity, int drySeasonBonus) =>
        Clamp((int)Math.Round(55 * Math.Max(0.0, buildingDensity)) + drySeasonBonus);

    /// <summary>§2's River-adjacent driver, worsened by low regional Forest Cover (§4.2) — extended per
    /// §3.1 to also read the same Forest Cover value <see cref="LandslideExposure"/> reads. <paramref
    /// name="riverAdjacentFraction"/> is the settlement's own River-terrain/River-adjacent <see
    /// cref="Land.Plot"/> share (0-1); <paramref name="forestCoverFraction"/> is this codebase's real
    /// proxy for the not-yet-built regional Forest Cover track — the settlement's own Forest-terrain
    /// <see cref="Land.Plot"/> share, disclosed here and in <see cref="LandslideExposure"/>'s own doc
    /// comment as the honest stand-in for a value no Harvest Intensity lever exists yet to actually
    /// deplete.</summary>
    public static int FloodExposure(double riverAdjacentFraction, double forestCoverFraction)
    {
        var deforestationFactor = 1.0 - Math.Clamp(forestCoverFraction, 0.0, 1.0);
        return Clamp((int)Math.Round(70 * Math.Max(0.0, riverAdjacentFraction) * (0.5 + 0.5 * deforestationFactor)));
    }

    /// <summary>§2/§2.1's region-weighted, genuinely unpreventable driver. No Region in this codebase
    /// carries real terrain-bonus/penalty data yet (<c>Land.Region</c>'s own doc comment: "Terrain
    /// bonuses, penalty tables... are additive fields deferred") — §9's own "Regional Earthquake and
    /// Frost baselines" open question is taken at face value, so this is a flat, region-agnostic
    /// baseline rather than a faked per-region weighting.</summary>
    public static int EarthquakeExposure() => Clamp(12);

    /// <summary>§2's region-weighted driver, worsened by low Soil Fertility (§4.1). No regional
    /// weighting data exists (see <see cref="EarthquakeExposure"/>'s identical disclosure) and no Soil
    /// Fertility track exists either (§4.1 is explicitly out of this item's scope) — a flat,
    /// region-and-fertility-agnostic baseline, elevated during the same dry season <see
    /// cref="DisasterCompoundingCalculator.IsDrySeasonMonth"/> flags for <see cref="FireExposure"/>'s
    /// own compounding bonus, since a Mediterranean dry season is drought season by definition (§1).</summary>
    public static int DroughtFamineExposure(bool drySeasonMonth) => Clamp(drySeasonMonth ? 30 : 14);

    /// <summary>§2's coastal/port-reliance driver. <paramref name="coastalFraction"/> is the
    /// settlement's own Coast-terrain/Coastline-feature <see cref="Land.Plot"/> share (0-1) — the real
    /// proxy for "sea-trade volume," since no separate trade-volume figure exists per settlement.</summary>
    public static int StormExposure(double coastalFraction) =>
        Clamp((int)Math.Round(60 * Math.Max(0.0, coastalFraction)));

    /// <summary>§2's (new) Hills/Mountain terrain driver, jointly driven by low regional Forest Cover
    /// and low Slope Stability (§4.3/§3.1). <paramref name="hillsFraction"/> is the settlement's own
    /// Hills-terrain <see cref="Land.Plot"/> share; <paramref name="forestCoverFraction"/> is the same
    /// Forest-terrain-fraction proxy <see cref="FloodExposure"/> reads (§3.1's own "the same regional
    /// value"). No Slope Stability track exists yet (§4.3 is out of this item's scope, matching Soil
    /// Fertility/Forest Cover's own exemption) — its own "low Slope Stability" half of the driver is
    /// folded into the same Hills-fraction weighting rather than a separate, faked stability score.</summary>
    public static int LandslideExposure(double hillsFraction, double forestCoverFraction)
    {
        var deforestationFactor = 1.0 - Math.Clamp(forestCoverFraction, 0.0, 1.0);
        return Clamp((int)Math.Round(65 * Math.Max(0.0, hillsFraction) * (0.4 + 0.6 * deforestationFactor)));
    }

    /// <summary>§2's (new) low-crop-diversity driver — "an Intensive-Monoculture estate is mechanically
    /// more vulnerable." No Cultivation Intensity/crop-diversification lever exists anywhere in this
    /// codebase yet (§4.1/§6's own specialization-vs-diversification choice is unbuilt), so this is a
    /// flat, diversity-agnostic baseline, honestly disclosed rather than reading a proxy that doesn't
    /// exist even loosely.</summary>
    public static int BlightInfestationExposure() => Clamp(18);

    /// <summary>§2's (new) region-weighted, Olive/Vineyard-concentration driver, and §5.4's own distinct
    /// perennial-crop recovery-tail hazard. No regional weighting data exists (see <see
    /// cref="EarthquakeExposure"/>'s identical disclosure) and no Olive Grove/Vineyard building or
    /// output-concentration figure exists either — a flat, region-and-concentration-agnostic
    /// baseline.</summary>
    public static int FrostExposure() => Clamp(16);

    private static int Clamp(int score) => Math.Clamp(score, 0, MaxExposure);
}
