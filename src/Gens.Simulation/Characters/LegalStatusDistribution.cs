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

    /// <summary>Merges two distributions field-by-field — used by Roadmap Phase 7 item 4's mobility
    /// and migration systems to fold a moved/entering share into a destination group's existing
    /// distribution.</summary>
    public static LegalStatusDistribution operator +(LegalStatusDistribution left, LegalStatusDistribution right) =>
        new(
            left.Citizen + right.Citizen,
            left.LatinRights + right.LatinRights,
            left.Peregrine + right.Peregrine,
            left.Freedman + right.Freedman);

    /// <summary>New, as-yet-unassimilated population (a birth or an ordinary immigrant, §8.1/§10)
    /// joining this distribution — always enters at the lowest free-status tier, matching <see
    /// cref="AllPeregrine"/>'s own reasoning.</summary>
    public LegalStatusDistribution WithNewEntrants(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Cannot add a negative number of entrants.");
        return this with { Peregrine = Peregrine + count };
    }

    /// <summary>Splits <paramref name="count"/> members out of this distribution, apportioned across
    /// <see cref="Citizen"/>/<see cref="LatinRights"/>/<see cref="Peregrine"/>/<see cref="Freedman"/>
    /// proportional to this distribution's own current composition, using the largest-remainder
    /// method (integer division plus a deterministic, fixed Citizen/LatinRights/Peregrine/Freedman
    /// tie-break order for leftover units) so every unit is accounted for without floating-point
    /// rounding (ADR 0002). <paramref name="count"/> is clamped to <see cref="Total"/> — this never
    /// removes more members than the distribution actually carries. Used by Roadmap Phase 7 item 4's
    /// growth/mortality, migration, and social-mobility systems to move population between groups (or
    /// out of the settlement) while keeping <see cref="Size"/> and this distribution's composition
    /// moving together, per <see cref="PopGroup"/>'s own doc comment.</summary>
    public (LegalStatusDistribution Taken, LegalStatusDistribution Remaining) Split(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Cannot split a negative count.");

        var total = Total;
        var actualCount = Math.Min(count, total);
        if (total == 0 || actualCount == 0)
            return (Empty, this);

        var weights = new[] { Citizen, LatinRights, Peregrine, Freedman };
        var shares = new int[4];
        var remainders = new long[4];
        var assigned = 0;
        for (var i = 0; i < 4; i++)
        {
            var product = (long)weights[i] * actualCount;
            shares[i] = (int)(product / total);
            remainders[i] = product % total;
            assigned += shares[i];
        }

        var leftover = actualCount - assigned;
        var order = new[] { 0, 1, 2, 3 };
        // Largest remainder first; the fixed 0..3 (Citizen, LatinRights, Peregrine, Freedman) order
        // is the deterministic tie-break for equal remainders (ADR 0004's ordering discipline applied
        // to a fixed four-slot tuple rather than a collection).
        Array.Sort(order, (a, b) =>
        {
            var byRemainder = remainders[b].CompareTo(remainders[a]);
            return byRemainder != 0 ? byRemainder : a.CompareTo(b);
        });
        for (var k = 0; k < leftover; k++)
            shares[order[k]]++;

        var taken = new LegalStatusDistribution(shares[0], shares[1], shares[2], shares[3]);
        var remaining = new LegalStatusDistribution(
            Citizen - shares[0], LatinRights - shares[1], Peregrine - shares[2], Freedman - shares[3]);
        return (taken, remaining);
    }

    /// <summary>Shorthand for <see cref="Split"/> when only the remaining distribution is needed (a
    /// death or emigration removing <paramref name="count"/> members proportionally, with nowhere for
    /// the removed share to land).</summary>
    public LegalStatusDistribution Shrink(int count) => Split(count).Remaining;
}
