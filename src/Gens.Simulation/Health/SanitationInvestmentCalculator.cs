using Gens.Simulation.Ledger;

namespace Gens.Simulation.Health;

/// <summary>Pure math for §6's Sanitation Investment: an ongoing Treasury cost per tier, and the
/// exposure/spread multiplier it buys. No cost/benefit curve exists in the design corpus (§12's own
/// "Sanitation Investment's own cost/benefit curve" open item) — every figure here is this
/// implementation's own invented number, chosen only so that a higher tier costs strictly more and
/// reduces exposure/spread strictly further, and so <see cref="SanitationInvestmentTier.Comprehensive"/>
/// meaningfully compounds with, rather than replaces, whatever infrastructure a future Buildings pass
/// eventually adds (§6: "a genuine multiplier on top of whatever infrastructure already exists" — this
/// multiplier applies even with zero infrastructure contribution today, since no Aqueduct/Latrines/
/// Bathhouse building exists yet to multiply against).</summary>
public static class SanitationInvestmentCalculator
{
    /// <summary>The multiplier applied to every Endemic Exposure probability (<see
    /// cref="EndemicExposureCalculator"/>) and every Epidemic ignition/spread probability (<see
    /// cref="EpidemicSpreadCalculator"/>) at this settlement — §6's single lever over the whole
    /// system at once.</summary>
    public static double ExposureMultiplier(SanitationInvestmentTier tier) => tier switch
    {
        SanitationInvestmentTier.Minimal => 1.0,
        SanitationInvestmentTier.Standard => 0.75,
        SanitationInvestmentTier.Comprehensive => 0.5,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown sanitation investment tier."),
    };

    /// <summary>The recurring monthly draw from the settlement's own Treasury (<see
    /// cref="LedgerAccountKey.ForSettlementTreasury"/>) — <see cref="SanitationInvestmentTier.Minimal"/>
    /// costs nothing (it is the "doing nothing extra" baseline), matching Religion's Rites Budget's own
    /// "even its cheapest tier still has a real number" shape but starting from a true zero here since
    /// Minimal is this policy's explicit default rather than an authored floor tier.</summary>
    public static Money MonthlyTreasuryCost(SanitationInvestmentTier tier) => tier switch
    {
        SanitationInvestmentTier.Minimal => Money.Zero,
        SanitationInvestmentTier.Standard => Money.FromDenarii(20),
        SanitationInvestmentTier.Comprehensive => Money.FromDenarii(60),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown sanitation investment tier."),
    };
}
