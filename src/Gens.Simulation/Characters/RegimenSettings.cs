namespace Gens.Simulation.Characters;

/// <summary>Diet axis tier (<c>gens-labor-slavery-design.md</c> §5), primarily affecting Health trend
/// and upkeep cost.</summary>
public enum DietTier
{
    Meager,
    Adequate,
    Generous,
}

/// <summary>Accommodation axis tier (§5), primarily affecting Health trend, Loyalty trend, and upkeep
/// cost.</summary>
public enum AccommodationTier
{
    Bare,
    Basic,
    Comfortable,
}

/// <summary>Permitted Freedoms axis tier (§5), primarily affecting flight opportunity, Loyalty trend,
/// and access to errands/travel-adjacent tasks.</summary>
public enum FreedomsTier
{
    Confined,
    Restricted,
    FreeMovement,
}

/// <summary>Discipline Strictness axis tier (§5), primarily affecting the Fatigue/output ceiling,
/// Loyalty trend, and flight motive.</summary>
public enum DisciplineTier
{
    Lenient,
    Firm,
    Harsh,
}

/// <summary>The standing, four-axis treatment setting an enslaved individual lives under
/// (<c>gens-labor-slavery-design.md</c> §5). Set at the group level (<see
/// cref="State.WorldState.HouseholdRegimenDefaults"/>) or as a per-individual override (<see
/// cref="Character.Regimen"/>) — see <see cref="RegimenResolver"/> for how the two combine, and <see
/// cref="RegimenCatalog"/> for what each tier actually does.</summary>
public readonly record struct RegimenSettings(
    DietTier Diet, AccommodationTier Accommodation, FreedomsTier Freedoms, DisciplineTier Discipline);
