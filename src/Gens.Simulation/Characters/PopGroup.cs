using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Characters;

/// <summary>
/// One background population group at one settlement — the <see cref="FidelityTier.Background"/> tier
/// of ADR 0009, and the source side of <see cref="PromoteToNamedCommand"/>. Carries
/// <c>gens-settlement-demographics-design.md</c> §15's full <c>PopGroup</c> data model (Roadmap Phase
/// 7 item 1): settlement and occupation/class are the key (<see cref="SettlementId"/>, <see
/// cref="GroupType"/>); <see cref="Size"/>, <see cref="LegalStatusDistribution"/>, <see
/// cref="Culture"/>, <see cref="WealthBand"/>, <see cref="NeedsProfile"/>, <see
/// cref="EmploymentRatio"/>, <see cref="HousingSatisfaction"/>, <see cref="Contentment"/>, and <see
/// cref="HealthExposure"/> are the rest. <see cref="PromoteToNamedCommand"/> is still the only thing
/// that conserves <see cref="Size"/> (decrementing it by exactly one per promotion); the other fields
/// are composition/condition data that later Phase 7 items (job capacity, needs/migration dynamics,
/// social mobility) will read and update, and are left untouched by promotion for now.
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
    /// reset or recomputed from elsewhere (<c>gens-settlement-demographics-design.md</c> §5's
    /// "population changes have causes and no group silently duplicates or disappears").</summary>
    public required int Size { get; init; }

    /// <summary>The group's legal-status composition (§10, §15). Tracks composition among the
    /// group's free-status population rather than strictly partitioning every member — a member with
    /// a legal status outside the four tracked categories (e.g. an individual enslaved within an
    /// otherwise-free group) is possible and simply isn't counted here, and <see
    /// cref="PromoteToNamedCommand"/> decrements <see cref="Size"/> without updating this
    /// distribution — so <see cref="LegalStatusDistribution.Total"/> may differ from <see
    /// cref="Size"/> in either direction and is not validated against it.</summary>
    public required LegalStatusDistribution LegalStatusDistribution { get; init; }

    /// <summary>The group's culture (content-authored; same reference type as <see
    /// cref="Character.Culture"/>).</summary>
    public required DefinitionId<Culture> Culture { get; init; }

    /// <summary>The group's wealth tier (<c>gens-population-wealth-purchasing-power-design.md</c>
    /// §2).</summary>
    public required WealthBand WealthBand { get; init; }

    /// <summary>The group's default consumption tier (§6.1's "Meager/Adequate/Generous consumption"),
    /// reusing <see cref="RegimenSettings"/>'s <see cref="DietTier"/> rather than a duplicate
    /// enum.</summary>
    public required DietTier NeedsProfile { get; init; }

    /// <summary>Available background job slots in this group's sector divided by the group's own
    /// size (§4.2) — a <see cref="Fixed64"/> ratio, not yet computed by any system.</summary>
    public required Fixed64 EmploymentRatio { get; init; }

    /// <summary>How well the group's housing/land capacity need is currently met (§7) — a <see
    /// cref="Fixed64"/> ratio, not yet computed by any system.</summary>
    public required Fixed64 HousingSatisfaction { get; init; }

    /// <summary>The group's contentment (§6.2) — a <see cref="Fixed64"/> value, not yet computed by
    /// any system.</summary>
    public required Fixed64 Contentment { get; init; }

    /// <summary>The group's elevated exposure to disease/public-health risk (§7.3's overcrowding
    /// cross-reference) — a <see cref="Fixed64"/> value, not yet computed by any system.</summary>
    public required Fixed64 HealthExposure { get; init; }

    /// <summary>The one culture the sample content currently defines
    /// (<c>content/source/cultures/sample.json</c>) — the default when no culture is specified,
    /// matching the literal default used across this codebase's own <c>PopGroup</c>/promotion
    /// tests.</summary>
    private static readonly DefinitionId<Culture> DefaultCulture = new("roman");

    public static PopGroup Create(
        RuntimeId<Settlement> settlementId,
        PopGroupType groupType,
        int size,
        LegalStatusDistribution? legalStatusDistribution = null,
        DefinitionId<Culture>? culture = null,
        WealthBand wealthBand = WealthBand.Subsistence,
        DietTier needsProfile = DietTier.Meager,
        Fixed64? employmentRatio = null,
        Fixed64? housingSatisfaction = null,
        Fixed64? contentment = null,
        Fixed64? healthExposure = null)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), size, "A PopGroup's size cannot be negative.");

        var distribution = legalStatusDistribution ?? (groupType == PopGroupType.NonHouseholdEnslaved
            ? LegalStatusDistribution.Empty
            : LegalStatusDistribution.AllPeregrine(size));

        if (distribution.Citizen < 0 || distribution.LatinRights < 0 || distribution.Peregrine < 0 || distribution.Freedman < 0)
            throw new ArgumentOutOfRangeException(
                nameof(legalStatusDistribution), distribution, "A LegalStatusDistribution's counts cannot be negative.");
        // Deliberately not validated against `size`: PromoteToNamedCommand decrements Size without
        // touching the distribution (PopGroup.cs's own doc comment), so a group that has had members
        // promoted legitimately carries a distribution whose Total exceeds its current Size until a
        // later mobility/assimilation pass (Phase 7 items 4-6) starts maintaining both together.
        if (groupType == PopGroupType.NonHouseholdEnslaved && distribution != LegalStatusDistribution.Empty)
            throw new ArgumentException(
                $"A {PopGroupType.NonHouseholdEnslaved} group's whole population is enslaved by group type; " +
                $"it cannot also carry a non-empty {nameof(LegalStatusDistribution)}.",
                nameof(legalStatusDistribution));

        var resolvedEmploymentRatio = employmentRatio ?? Fixed64.One;
        var resolvedHousingSatisfaction = housingSatisfaction ?? Fixed64.One;
        var resolvedContentment = contentment ?? Fixed64.One;
        var resolvedHealthExposure = healthExposure ?? Fixed64.Zero;

        if (resolvedEmploymentRatio < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(employmentRatio), resolvedEmploymentRatio, "EmploymentRatio cannot be negative.");
        if (resolvedHousingSatisfaction < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(housingSatisfaction), resolvedHousingSatisfaction, "HousingSatisfaction cannot be negative.");
        if (resolvedContentment < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(contentment), resolvedContentment, "Contentment cannot be negative.");
        if (resolvedHealthExposure < Fixed64.Zero)
            throw new ArgumentOutOfRangeException(nameof(healthExposure), resolvedHealthExposure, "HealthExposure cannot be negative.");

        return new PopGroup
        {
            SettlementId = settlementId,
            GroupType = groupType,
            Size = size,
            LegalStatusDistribution = distribution,
            Culture = culture ?? DefaultCulture,
            WealthBand = wealthBand,
            NeedsProfile = needsProfile,
            EmploymentRatio = resolvedEmploymentRatio,
            HousingSatisfaction = resolvedHousingSatisfaction,
            Contentment = resolvedContentment,
            HealthExposure = resolvedHealthExposure,
        };
    }
}
