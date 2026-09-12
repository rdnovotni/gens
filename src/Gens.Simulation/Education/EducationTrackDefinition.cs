using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Education;

/// <summary>Phantom marker type for <see cref="DefinitionId{T}"/>, matching <see cref="Identity.Culture"/>'s
/// identical "a marker type with no members of its own" convention (Phase 17 item 2).</summary>
public sealed class EducationTrack
{
    private EducationTrack()
    {
    }
}

/// <summary>
/// One authored Educational Track (Phase 17 item 2; <c>gens-education-culture-design.md</c> §3): the
/// three tracks this ticket builds — Rhetoric (Diplomacy), Philosophy (Learning + Cultural Prestige), and
/// Gymnasium (Martial + Learning) — each delivered by a settlement building staffed with its own Rhetor/
/// Magister/Paidotribes (the existing generic <see cref="BuildingInstance.AssignStaff"/> mechanism, per
/// this ticket's own confirmed scope decision — no new staffing concept). <see
/// cref="PrimaryAttribute"/>/<see cref="SecondaryAttribute"/> are drawn from <see
/// cref="PermanentInjuryTarget"/>'s existing Core-Attribute members rather than a new, parallel "which
/// attribute" enum — the same five values that enum already names for Diplomacy/Martial/Stewardship/
/// Intrigue/Learning, reused here instead of duplicated (constructor validates both are drawn from that
/// five-value Core Attribute subset, never a Labor Skill or Fertility target).
///
/// <see cref="CompletionMonths"/>, the Distinguished-tier cost/multiplier, and the flat monthly attribute
/// gain are this implementation's own unsized baseline — §13 leaves every numeric figure in this domain
/// open, matching <see cref="CulturalPrestigeCatalog"/>'s and <see cref="Religion.ReligionCatalog"/>'s
/// identical disclaimer convention. This C# catalog is the one the simulation runtime actually reads;
/// <c>content/source/education/tracks.json</c> mirrors the same three entries for the content
/// compiler's own schema/reference validation, matching <see cref="Cultures.KnownWorldCultures"/>'s
/// identical "hand-authored runtime catalog, JSON validated separately" precedent (<see
/// cref="Cultures.CultureDefinition"/> is likewise never loaded from JSON at runtime).
/// </summary>
public sealed record EducationTrackDefinition
{
    private static readonly IReadOnlyCollection<PermanentInjuryTarget> CoreAttributeTargets = new[]
    {
        PermanentInjuryTarget.Diplomacy, PermanentInjuryTarget.Martial, PermanentInjuryTarget.Stewardship,
        PermanentInjuryTarget.Intrigue, PermanentInjuryTarget.Learning,
    };

    public EducationTrackDefinition(
        DefinitionId<EducationTrack> id,
        string name,
        IEnumerable<DefinitionId<Building>> deliveringBuildingIds,
        PermanentInjuryTarget primaryAttribute,
        int monthlyAttributeGain,
        int completionMonths,
        Money distinguishedTierCostPerMonth,
        int distinguishedTierMultiplier,
        PermanentInjuryTarget? secondaryAttribute = null,
        bool grantsCulturalPrestige = false,
        int monthlyPrestigeGain = 0,
        bool satisfiesMagistracyContestGate = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An education track requires a non-empty name.", nameof(name));
        if (!CoreAttributeTargets.Contains(primaryAttribute))
            throw new ArgumentException("Primary attribute must be one of the five Core Attributes.", nameof(primaryAttribute));
        if (secondaryAttribute is { } secondary && !CoreAttributeTargets.Contains(secondary))
            throw new ArgumentException("Secondary attribute must be one of the five Core Attributes.", nameof(secondaryAttribute));
        if (monthlyAttributeGain <= 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyAttributeGain), monthlyAttributeGain, "Monthly attribute gain must be positive.");
        if (completionMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(completionMonths), completionMonths, "Completion months must be positive.");
        if (distinguishedTierMultiplier <= 1)
            throw new ArgumentOutOfRangeException(
                nameof(distinguishedTierMultiplier), distinguishedTierMultiplier, "The Distinguished tier must multiply the base gain.");
        if (monthlyPrestigeGain < 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyPrestigeGain), monthlyPrestigeGain, "Monthly Prestige gain cannot be negative.");

        var buildings = (deliveringBuildingIds ?? throw new ArgumentNullException(nameof(deliveringBuildingIds)))
            .Distinct().OrderBy(b => b.Value, StringComparer.Ordinal).ToArray();
        if (buildings.Length == 0)
            throw new ArgumentException("An education track needs at least one delivering building.", nameof(deliveringBuildingIds));

        Id = id;
        Name = name;
        DeliveringBuildingIds = buildings;
        PrimaryAttribute = primaryAttribute;
        SecondaryAttribute = secondaryAttribute;
        MonthlyAttributeGain = monthlyAttributeGain;
        CompletionMonths = completionMonths;
        DistinguishedTierCostPerMonth = distinguishedTierCostPerMonth;
        DistinguishedTierMultiplier = distinguishedTierMultiplier;
        GrantsCulturalPrestige = grantsCulturalPrestige;
        MonthlyPrestigeGain = monthlyPrestigeGain;
        SatisfiesMagistracyContestGate = satisfiesMagistracyContestGate;
    }

    public DefinitionId<EducationTrack> Id { get; }
    public string Name { get; }
    public IReadOnlyList<DefinitionId<Building>> DeliveringBuildingIds { get; }
    public PermanentInjuryTarget PrimaryAttribute { get; }
    public PermanentInjuryTarget? SecondaryAttribute { get; }
    public int MonthlyAttributeGain { get; }

    /// <summary>How many months of active enrollment complete this track — see <see
    /// cref="EducationalTrackProgressSystem"/>.</summary>
    public int CompletionMonths { get; }

    public Money DistinguishedTierCostPerMonth { get; }
    public int DistinguishedTierMultiplier { get; }

    /// <summary>Philosophy only, per §3's "philosophy→Learning+Cultural Prestige" mapping.</summary>
    public bool GrantsCulturalPrestige { get; }
    public int MonthlyPrestigeGain { get; }

    /// <summary>Rhetoric only — "the flag that satisfies the magistracy-contest gate" (§3), read by <see
    /// cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/>.</summary>
    public bool SatisfiesMagistracyContestGate { get; }
}
