using Gens.Simulation.Ledger;

namespace Gens.Simulation.Policies;

/// <summary>The per-tier numeric effects <c>gens-policies-edicts-design.md</c> leaves untuned (its own
/// §10 "all numeric sizing" open question) — this implementation's own invented baseline, matching
/// <see cref="Gens.Simulation.Characters.RegimenCatalog"/>'s identical disclaimer for its own untuned
/// numbers.</summary>
public static class RitesBudgetCatalog
{
    /// <summary>The fallback tier a household with no <see cref="HouseholdPolicyState"/> entry yet
    /// reads as (<see cref="HouseholdPolicyResolver"/>) — a deliberately neutral middle setting,
    /// matching <see cref="Gens.Simulation.Characters.RegimenCatalog.Default"/>'s identical convention.</summary>
    public const RitesBudgetTier Default = RitesBudgetTier.Standard;

    /// <summary>How many months must elapse between two accepted <see
    /// cref="ChangeRitesBudgetCommand"/>s against the same household (§6.12's content-authored
    /// <c>cooldownMonths</c>, matching <c>content/schemas/policies.schema.json</c>'s stub field of the
    /// same name) — the anti-flip-flopping mechanic Phase 9 item 2 names directly.</summary>
    public const int CooldownMonths = 6;

    /// <summary>Monthly Treasury draw for maintaining this tier's Rites Budget (§2.3: "trading
    /// Incense/Treasury draw against Divine Favor stability"). Not yet drawn by any monthly system —
    /// Religion (§6, future) is what will eventually post this — but is a real, ready-to-consume
    /// projection now, matching <see cref="Gens.Simulation.Characters.RegimenCatalog"/>'s own
    /// "projection exists before its consumer does" precedent.</summary>
    public static Money TreasuryDrawPerMonth(RitesBudgetTier tier) => tier switch
    {
        RitesBudgetTier.Frugal => Money.FromDenarii(2),
        RitesBudgetTier.Standard => Money.FromDenarii(5),
        RitesBudgetTier.Lavish => Money.FromDenarii(12),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown Rites Budget tier."),
    };

    /// <summary>This tier's contribution to Divine Favor stability (§2.3) — a signed monthly trend
    /// delta, mirroring <see cref="Gens.Simulation.Characters.RegimenCatalog.HealthTrend"/>'s identical
    /// shape.</summary>
    public static int DivineFavorStabilityModifier(RitesBudgetTier tier) => tier switch
    {
        RitesBudgetTier.Frugal => -2,
        RitesBudgetTier.Standard => 0,
        RitesBudgetTier.Lavish => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown Rites Budget tier."),
    };
}
