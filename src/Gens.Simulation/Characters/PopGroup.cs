using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>
/// One background population group at one settlement — the <see cref="FidelityTier.Background"/> tier
/// of ADR 0009, and the source side of <see cref="PromoteToNamedCommand"/>. Deliberately narrower than
/// <c>gens-settlement-demographics-design.md</c> §15's full <c>PopGroup</c> data model: that document's
/// <c>legalStatusDistribution</c>, <c>employmentRatio</c>, and <c>contentment</c> fields belong to the
/// Settlement Demographics system's own future construction-order item (Roadmap Phase 6+), which
/// populates and updates this group's <see cref="Size"/> over time. This record carries only what
/// promotion itself needs — a count to conserve — so Phase 5 item 7 does not have to build that whole
/// system first; a real Settlement Demographics pass can grow this record additively later, matching
/// ADR 0011's "additive only" save convention.
/// </summary>
public sealed record PopGroup
{
    private PopGroup()
    {
    }

    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required PopGroupType GroupType { get; init; }

    /// <summary>How many unnamed individuals remain in this group. Conserved by <see
    /// cref="PromoteToNamedCommand"/>: promoting one person decrements this by exactly one, never
    /// resets or recomputed from elsewhere (<c>gens-settlement-demographics-design.md</c> §5's
    /// "population changes have causes and no group silently duplicates or disappears").</summary>
    public required int Size { get; init; }

    public static PopGroup Create(RuntimeId<Settlement> settlementId, PopGroupType groupType, int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), size, "A PopGroup's size cannot be negative.");

        return new PopGroup
        {
            SettlementId = settlementId,
            GroupType = groupType,
            Size = size,
        };
    }
}
