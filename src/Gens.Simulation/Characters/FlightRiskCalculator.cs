namespace Gens.Simulation.Characters;

/// <summary>Pure flight-risk math (<c>gens-labor-slavery-design.md</c> §7: "flight risk is derived from
/// Loyalty (low), household Unrest, the individual's Regimen ..., and personal traits"). Deliberately
/// omits a household-Unrest term — that stat doesn't exist anywhere in the codebase yet (no system
/// models it) — and a traits term — none of the specific Reactive traits §7 names (<c>Resentful</c>,
/// <c>Ambitious</c>, <c>Content</c>, <c>Grateful</c>) exist in <c>content/source/traits/</c> yet. Both
/// gaps are the two inputs this formula is missing until their systems ship. <see cref="LaborFlightSystem"/>
/// is the only caller; nothing persists a risk score on <see cref="Character"/> (<c>gens-labor-slavery-design.md</c>
/// §11 lists it as a stored field, but this codebase's convention — see <see
/// cref="Character.GetLifecycleStage"/> — is to never store what's derivable from current state).</summary>
public static class FlightRiskCalculator
{
    /// <summary>The precision <see cref="MonthlyProbabilityThreshold"/>'s result and <see
    /// cref="LaborFlightSystem"/>'s opportunity roll share, matching <see
    /// cref="CharacterLifecycleSystem"/>'s identical mortality-roll precision.</summary>
    public const uint RollPrecision = 1_000_000;

    /// <summary>Invented baseline (§12 leaves flight-risk thresholds untuned): the monthly probability,
    /// out of <see cref="RollPrecision"/>, that a Character at the maximum possible risk score (200)
    /// flees given this month's opportunity roll.</summary>
    private const uint MaxMonthlyProbability = 50_000; // 5%.

    private const int MaxRiskScore = 200;

    /// <summary>A 0-200 risk score: motive from low Loyalty, plus Regimen-derived opportunity
    /// (Freedoms) and motive (Discipline) terms.</summary>
    public static int ComputeRiskScore(int loyalty, RegimenSettings regimen)
    {
        var loyaltyMotive = 100 - loyalty;
        var opportunity = RegimenCatalog.FlightOpportunityWeight(regimen.Freedoms);
        var disciplineMotive = RegimenCatalog.FlightMotiveWeight(regimen.Discipline);
        return Math.Clamp(loyaltyMotive + opportunity + disciplineMotive, 0, MaxRiskScore);
    }

    /// <summary>The monthly opportunity-roll threshold (out of <see cref="RollPrecision"/>) a given
    /// risk score maps to — a roll strictly below this threshold means the Character flees this
    /// month.</summary>
    public static uint MonthlyProbabilityThreshold(int riskScore)
    {
        var scaled = (long)riskScore * MaxMonthlyProbability / MaxRiskScore;
        return (uint)Math.Clamp(scaled, 0L, (long)MaxMonthlyProbability);
    }
}
