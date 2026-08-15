namespace Gens.Simulation.Characters;

/// <summary>Pure labor-output/fatigue/condition-trend math (<c>gens-labor-slavery-design.md</c> §4:
/// "output from a given duty slot is a function of the assigned person's relevant Labor Skill,
/// moderated by current Health and Fatigue ... and by their active Regimen settings"). Nothing here is
/// persisted on <see cref="Character"/> — <see cref="LaborOutputSystem"/> calls this each tick and
/// exposes the result via <see cref="LaborOutputComputedEvent"/> for the later production-network item
/// (Phase 6 item 7) to consume once buildings/goods are wired up.</summary>
public static class LaborOutputCalculator
{
    /// <summary>Invented baseline (§12 leaves fatigue accrual untuned): Fatigue gained per month spent
    /// on Duty, layered on top of <see cref="CharacterLifecycleSystem"/>'s flat monthly recovery —
    /// exactly the extension point that system's own doc comment anticipates.</summary>
    public const int MonthlyDutyFatigueAccrual = 15;

    /// <summary>Above this Fatigue, output is capped regardless of skill (§4: "heavy fatigue caps
    /// effective output regardless of raw skill").</summary>
    public const int HeavyFatigueThreshold = 70;

    /// <summary>Computes a 0-100 output score from effective skill, Health, Fatigue, and the resolved
    /// Regimen's Discipline-driven output ceiling.</summary>
    public static int ComputeOutput(int effectiveSkill, int health, int fatigue, RegimenSettings regimen)
    {
        var ceilingModifier = RegimenCatalog.OutputCeilingModifier(regimen.Discipline);
        var baseOutput = Math.Clamp(effectiveSkill + ceilingModifier, 0, 100);

        var healthFactor = health / 100.0;
        var fatiguePenalty = fatigue > HeavyFatigueThreshold ? (fatigue - HeavyFatigueThreshold) * 2 : 0;

        var output = baseOutput * healthFactor - fatiguePenalty;
        return Math.Clamp((int)Math.Round(output), 0, 100);
    }

    public static int ApplyMonthlyFatigueAccrual(int currentFatigue) =>
        Math.Clamp(currentFatigue + MonthlyDutyFatigueAccrual, 0, 100);

    public static int ApplyMonthlyHealthTrend(int currentHealth, RegimenSettings regimen) =>
        Math.Clamp(currentHealth + RegimenCatalog.HealthTrend(regimen), 0, 100);

    public static int ApplyMonthlyLoyaltyTrend(int currentLoyalty, RegimenSettings regimen) =>
        Math.Clamp(currentLoyalty + RegimenCatalog.LoyaltyTrend(regimen), 0, 100);
}
