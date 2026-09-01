namespace Gens.Simulation.Hazards;

/// <summary>Pure, RNG-free math turning an <see cref="HazardExposureCalculator"/> Exposure score into
/// §5.1's own two-step roll: whether a Disaster Event fires at all this month, and — given that it
/// does — which of the four <see cref="DisasterSeverity"/> tiers it resolves at. Every figure is this
/// implementation's own invented number (§9's "All numeric sizing" open question, same citation <see
/// cref="HazardExposureCalculator"/>'s own doc comment uses), chosen only so that Exposure 0 never
/// ignites, Exposure 100 ignites often and skews heavily toward the top two tiers, and every point in
/// between reads as a smooth, monotonic increase in both — §5.1's own "a low-Exposure household is both
/// less likely to suffer an Event at all and weighted away from the worst outcomes."</summary>
public static class DisasterSeverityCalculator
{
    /// <summary>Precision <see cref="NaturalDisasterSystem"/>'s RNG draws are compared against,
    /// matching <c>Health.CharacterHealthConditionSystem</c>'s identical <c>RollPrecision</c>
    /// convention.</summary>
    public const uint RollPrecision = 1_000_000;

    private const double MaxMonthlyIgnitionProbability = 0.12;

    /// <summary>The monthly probability a settlement's standing Exposure (0-100) actually produces a
    /// Disaster Event this month. Deliberately capped well below certainty even at Exposure 100 — a
    /// disaster is meant to read as a genuine, occasional occurrence (matching
    /// <c>Health.EpidemicSpreadCalculator.MonthlyIgnitionProbability</c>'s own "deliberately rare"
    /// framing), not a monthly inevitability for even the most exposed settlement.</summary>
    public static double MonthlyIgnitionProbability(int exposureScore) =>
        MaxMonthlyIgnitionProbability * Math.Clamp(exposureScore, 0, 100) / 100.0;

    /// <summary>Rolls which of the four severity tiers a fired Event resolves at, from a second,
    /// independent RNG draw in <c>[0, <see cref="RollPrecision"/>)</c> (the caller's own second roll,
    /// not re-drawn here — this function stays pure). Higher Exposure shifts real probability mass from
    /// <see cref="DisasterSeverity.Minor"/> toward <see cref="DisasterSeverity.Catastrophic"/>: at
    /// Exposure 0 a fired Event is overwhelmingly Minor; at Exposure 100 Catastrophic and Severe together
    /// account for over half the distribution.</summary>
    public static DisasterSeverity RollSeverity(int exposureScore, uint severityRoll)
    {
        var e = Math.Clamp(exposureScore, 0, 100) / 100.0;

        // Catastrophic and Severe shares grow with Exposure; Minor's share shrinks correspondingly.
        // Moderate holds a roughly steady middle share throughout. All four always sum to 1.0.
        var catastrophicShare = 0.03 + 0.22 * e;
        var severeShare = 0.07 + 0.28 * e;
        var moderateShare = 0.25;
        // Minor's own share is never computed explicitly: it is whatever remains below
        // moderateThreshold, the same "everything not covered by the other three thresholds" shape the
        // three explicit thresholds below already guarantee sums to 1.0 across all four tiers.
        var catastrophicThreshold = (uint)Math.Clamp(catastrophicShare * RollPrecision, 0, RollPrecision);
        var severeThreshold = (uint)Math.Clamp((catastrophicShare + severeShare) * RollPrecision, 0, RollPrecision);
        var moderateThreshold = (uint)Math.Clamp((catastrophicShare + severeShare + moderateShare) * RollPrecision, 0, RollPrecision);

        if (severityRoll < catastrophicThreshold)
            return DisasterSeverity.Catastrophic;
        if (severityRoll < severeThreshold)
            return DisasterSeverity.Severe;
        if (severityRoll < moderateThreshold)
            return DisasterSeverity.Moderate;
        return DisasterSeverity.Minor;
    }
}
