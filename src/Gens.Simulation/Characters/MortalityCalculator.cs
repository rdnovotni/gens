namespace Gens.Simulation.Characters;

/// <summary>
/// A pure, RNG-free monthly death-probability curve (Phase 5 item 3). No numeric mortality formula
/// exists anywhere in the design corpus — <c>gens-familia-design.md</c> §3 only describes the shape
/// qualitatively ("Infant... primary gameplay surface is Disease/mortality risk"; "Elderly... death
/// risk... rise[s]"). Every constant below is this implementation's own invented curve, chosen only so
/// that: Infant and Elderly carry visibly elevated risk relative to Adult/Adolescent/Child, Elderly
/// risk keeps climbing with age, and Health measurably moderates the result in both directions —
/// matching that qualitative description rather than transcribing a spec value that doesn't exist.
/// </summary>
public static class MortalityCalculator
{
    /// <summary>The Health value at which <see cref="HealthFactor"/> is neutral (multiplier 1.0);
    /// below it, risk rises, above it, risk falls.</summary>
    private const int NeutralHealth = 50;

    /// <summary>Computes the probability this Character dies during the current month, from their
    /// lifecycle stage, age, and (already injury/decline-adjusted) Health.</summary>
    public static double MonthlyProbability(LifecycleStage stage, int ageInYears, int health)
    {
        var annualRate = AnnualRate(stage, ageInYears);
        var annualProbability = Math.Clamp(annualRate * HealthFactor(health), 0.0, 0.9);

        // Converts an annual probability to its equivalent monthly probability assuming independent
        // monthly draws: (1 - monthly)^12 = 1 - annual.
        return 1.0 - Math.Pow(1.0 - annualProbability, 1.0 / 12.0);
    }

    private static double AnnualRate(LifecycleStage stage, int ageInYears) => stage switch
    {
        LifecycleStage.Infant => 0.02,
        LifecycleStage.Child => 0.005,
        LifecycleStage.Adolescent => 0.004,
        LifecycleStage.Adult => 0.004,
        LifecycleStage.Elderly => Math.Min(0.35, 0.03 + 0.01 * (ageInYears - 60) / 10.0),
        _ => 0.004,
    };

    private static double HealthFactor(int health) =>
        Math.Clamp(1.0 + (NeutralHealth - health) / 100.0, 0.4, 2.0);
}
