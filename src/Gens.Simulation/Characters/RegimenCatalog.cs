namespace Gens.Simulation.Characters;

/// <summary>The per-tier numeric effects <c>gens-labor-slavery-design.md</c> §5 leaves untuned (its
/// §12 "Regimen tier deltas" open question) — this implementation's own invented baseline, matching
/// <see cref="DutySlotCatalog"/>'s and <see cref="RelationshipDecaySystem"/>'s identical disclaimer for
/// their own untuned numbers. <see cref="Default"/> also stands in for §6.12's not-yet-built Policies
/// &amp; Edicts household-wide "slave treatment" slider — the roadmap's "policy hook" — until that
/// system exists to set it instead.</summary>
public static class RegimenCatalog
{
    /// <summary>The system-wide fallback used when neither an individual override nor a group default
    /// applies (<see cref="RegimenResolver"/>) — a deliberately neutral middle setting on every axis.</summary>
    public static readonly RegimenSettings Default = new(
        DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Restricted, DisciplineTier.Firm);

    /// <summary>Combined monthly Health trend delta from Diet and Accommodation.</summary>
    public static int HealthTrend(RegimenSettings regimen) =>
        DietHealthTrend(regimen.Diet) + AccommodationHealthTrend(regimen.Accommodation);

    /// <summary>Combined monthly Loyalty trend delta from Accommodation, Freedoms, and Discipline.</summary>
    public static int LoyaltyTrend(RegimenSettings regimen) =>
        AccommodationLoyaltyTrend(regimen.Accommodation) + FreedomsLoyaltyTrend(regimen.Freedoms) +
        DisciplineLoyaltyTrend(regimen.Discipline);

    /// <summary>Discipline's effect on the achievable labor output ceiling (§4: "harsher, cheaper
    /// regimens raise the achievable output ceiling ... at a rising cost").</summary>
    public static int OutputCeilingModifier(DisciplineTier discipline) => discipline switch
    {
        DisciplineTier.Lenient => -5,
        DisciplineTier.Firm => 0,
        DisciplineTier.Harsh => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(discipline), discipline, "Unknown discipline tier."),
    };

    /// <summary>Freedoms' contribution to flight opportunity (§7: "Free Movement raises opportunity").</summary>
    public static int FlightOpportunityWeight(FreedomsTier freedoms) => freedoms switch
    {
        FreedomsTier.Confined => 5,
        FreedomsTier.Restricted => 15,
        FreedomsTier.FreeMovement => 35,
        _ => throw new ArgumentOutOfRangeException(nameof(freedoms), freedoms, "Unknown freedoms tier."),
    };

    /// <summary>Discipline's contribution to flight motive (§7: "Confined/Harsh suppresses opportunity
    /// but raises motive").</summary>
    public static int FlightMotiveWeight(DisciplineTier discipline) => discipline switch
    {
        DisciplineTier.Lenient => 5,
        DisciplineTier.Firm => 15,
        DisciplineTier.Harsh => 30,
        _ => throw new ArgumentOutOfRangeException(nameof(discipline), discipline, "Unknown discipline tier."),
    };

    private static int DietHealthTrend(DietTier diet) => diet switch
    {
        DietTier.Meager => -2,
        DietTier.Adequate => 0,
        DietTier.Generous => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(diet), diet, "Unknown diet tier."),
    };

    private static int AccommodationHealthTrend(AccommodationTier accommodation) => accommodation switch
    {
        AccommodationTier.Bare => -1,
        AccommodationTier.Basic => 0,
        AccommodationTier.Comfortable => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(accommodation), accommodation, "Unknown accommodation tier."),
    };

    private static int AccommodationLoyaltyTrend(AccommodationTier accommodation) => accommodation switch
    {
        AccommodationTier.Bare => -1,
        AccommodationTier.Basic => 0,
        AccommodationTier.Comfortable => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(accommodation), accommodation, "Unknown accommodation tier."),
    };

    private static int FreedomsLoyaltyTrend(FreedomsTier freedoms) => freedoms switch
    {
        FreedomsTier.Confined => -1,
        FreedomsTier.Restricted => 0,
        FreedomsTier.FreeMovement => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(freedoms), freedoms, "Unknown freedoms tier."),
    };

    private static int DisciplineLoyaltyTrend(DisciplineTier discipline) => discipline switch
    {
        DisciplineTier.Lenient => 1,
        DisciplineTier.Firm => 0,
        DisciplineTier.Harsh => -2,
        _ => throw new ArgumentOutOfRangeException(nameof(discipline), discipline, "Unknown discipline tier."),
    };
}
