namespace Gens.Simulation.Health;

/// <summary>Pure, RNG-free monthly progression math for a <see cref="CharacterHealthCondition"/> —
/// drain, recovery odds, fatality odds, and untreated severity drift — extending
/// <c>Characters.MortalityCalculator</c>'s own "documented as invented, pending playtesting" precedent
/// (its own doc comment) to health conditions. No numeric exposure/immunity/treatment/recovery curve
/// exists anywhere in the design corpus (<c>gens-disease-public-health-design.md</c> §12's own "All
/// numeric sizing" open question) — every constant below is this implementation's own invented figure,
/// chosen only so that: an <see cref="HealthConditionCategory.Acute"/> case (an epidemic) drains Health
/// and risks death faster, and resolves faster either way, than a <see
/// cref="HealthConditionCategory.Chronic"/> one (an endemic illness, §2's own "continuous, low-grade
/// Health drain... over a long stretch rather than in a single dramatic moment"); higher Severity
/// worsens fatality risk; Physician treatment (§7 — "a skilled Physician adds a real early-diagnosis
/// check") measurably improves recovery odds and measurably reduces both drain and fatality risk; and a
/// condition with no real cure (<see cref="HealthConditionDefinition.HasCure"/> false) is far harder to
/// shake even under treatment, matching §2's Roman Fever/Consumption "no real cure — only managed
/// severity" framing literally rather than just narratively.</summary>
public static class HealthConditionProgressionCalculator
{
    /// <summary>Treatment roughly halves this month's drain.</summary>
    private const double TreatedDrainMultiplier = 0.5;

    /// <summary>Health points lost this month from a case, before <see cref="TreatedDrainMultiplier"/>'s
    /// reduction when treated. Never less than 1: a standing case always costs something.</summary>
    public static int MonthlyHealthDrain(HealthConditionCategory category, int severity, bool treated)
    {
        var baseDrain = category == HealthConditionCategory.Acute
            ? 2 + severity / 10
            : 1 + severity / 20;
        var drain = treated ? (int)Math.Round(baseDrain * TreatedDrainMultiplier) : baseDrain;
        return Math.Max(1, drain);
    }

    /// <summary>This month's probability the case resolves into recovery. Acute cases resolve faster
    /// than Chronic ones (§3.3's worsening/recovery/death arc reads as comparatively short compared to
    /// Endemic Illness's standing background drain); a condition with no real cure recovers far more
    /// slowly even under treatment.</summary>
    public static double MonthlyRecoveryProbability(HealthConditionCategory category, bool hasCure, bool treated)
    {
        var basis = category == HealthConditionCategory.Acute ? 0.15 : 0.04;
        if (!hasCure)
            basis *= 0.35;
        if (treated)
            basis += category == HealthConditionCategory.Acute ? 0.15 : 0.06;

        return Math.Clamp(basis, 0.0, 0.9);
    }

    /// <summary>This month's probability the case kills the Character, from Severity and the
    /// Character's already-drained Health — mirrors <c>Characters.MortalityCalculator</c>'s own "Health
    /// measurably moderates the result" shape. Treatment roughly halves the risk.</summary>
    public static double MonthlyFatalityProbability(HealthConditionCategory category, int severity, int health, bool treated)
    {
        var baseline = category == HealthConditionCategory.Acute ? 0.03 : 0.006;
        var severityFactor = severity / 100.0;
        var healthFactor = Math.Clamp(1.5 - health / 100.0, 0.5, 1.5);
        var probability = baseline * severityFactor * healthFactor;
        if (treated)
            probability *= 0.5;

        return Math.Clamp(probability, 0.0, 0.6);
    }

    /// <summary>Severity drift for a case that neither recovers nor kills this month: an untreated case
    /// creeps worse over time; treatment holds it steady, matching §7's "manages severity" framing
    /// literally.</summary>
    public static int MonthlySeverityDrift(HealthConditionCategory category, bool treated)
    {
        if (treated)
            return 0;
        return category == HealthConditionCategory.Acute ? 3 : 1;
    }
}
