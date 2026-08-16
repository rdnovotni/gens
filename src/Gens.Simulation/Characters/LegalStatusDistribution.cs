namespace Gens.Simulation.Characters;

/// <summary>
/// A <see cref="PopGroup"/>'s legal-status composition (<c>gens-settlement-demographics-design.md</c>
/// §10, §15): the four free-population categories <see cref="LegalStatus"/> defines, excluding <see
/// cref="LegalStatus.Enslaved"/> — a group's enslaved members are tracked by <see
/// cref="PopGroupType.NonHouseholdEnslaved"/> itself, per §15's own four-field list, not
/// double-counted here.
/// </summary>
public readonly record struct LegalStatusDistribution(int Citizen, int LatinRights, int Peregrine, int Freedman)
{
    public int Total => Citizen + LatinRights + Peregrine + Freedman;

    /// <summary>No tracked free-status members — the only valid distribution for a <see
    /// cref="PopGroupType.NonHouseholdEnslaved"/> group.</summary>
    public static readonly LegalStatusDistribution Empty = new(0, 0, 0, 0);

    /// <summary>The default for a newly created ordinary group: unassimilated population enters at
    /// the lowest free-status tier (§10's Assimilation framing shifts a group's distribution toward
    /// higher status over time; it does not start there).</summary>
    public static LegalStatusDistribution AllPeregrine(int size) => new(0, 0, size, 0);
}
