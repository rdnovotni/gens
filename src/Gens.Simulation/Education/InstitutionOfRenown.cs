using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Education;

/// <summary>§12's <c>prestigeTier</c> field — a small, closed three-tier scale this implementation
/// invents to rank the five Institutions (§13 leaves the actual figure unsized), read by <see
/// cref="Chronicle.ChronicleProjector"/> to pick Notable vs. Major on a returning credential.</summary>
public enum InstitutionPrestigeTier
{
    Notable,
    Renowned,
    Legendary,
}

/// <summary>
/// One of the five fixed Institutions of Renown (Phase 17 item 2; <c>gens-education-culture-design.md</c>
/// §4's Study Abroad Journey, §12's own <c>InstitutionOfRenown</c> model). Both the record and its own
/// <see cref="DefinitionId{T}"/> phantom marker — mirroring <see
/// cref="Buildings.BuildingDefinition"/>/<see cref="Identity.Building"/>'s split shape would need a
/// second, purely-marker type this five-entry closed roster has no real use for, so this record serves
/// as its own key type directly (a lighter-weight variant already established by, e.g., <see
/// cref="Magistracies.MagistracyRecord"/> keying off its own <see cref="RuntimeId{T}"/>).
///
/// <see cref="DistanceTier"/>/<see cref="BaseRiskLevel"/>/<see cref="JourneyDurationMonths"/>/<see
/// cref="CostPerMonth"/> are this implementation's own invented baseline (§13 leaves every numeric figure
/// in this domain open) — Massilia is deliberately given the nearer, safer <see cref="DistanceTier.Moderate"/>/
/// <see cref="RouteRiskLevel.Guarded"/> pairing the ticket's own plan calls for ("meaningfully shorter/
/// safer... without authoring stub region content"), while Athens/Rhodes/Alexandria/Pergamon share <see
/// cref="DistanceTier.Far"/>/<see cref="RouteRiskLevel.Dangerous"/>.
/// </summary>
public sealed record InstitutionOfRenown
{
    public InstitutionOfRenown(
        DefinitionId<InstitutionOfRenown> id,
        string name,
        DefinitionId<EducationTrack> specialtyTrack,
        DefinitionId<Identity.Culture> primeCulturalAssociation,
        InstitutionPrestigeTier prestigeTier,
        DistanceTier distanceTier,
        RouteRiskLevel baseRiskLevel,
        int journeyDurationMonths,
        Money costPerMonth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An institution requires a non-empty name.", nameof(name));
        if (journeyDurationMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(journeyDurationMonths), journeyDurationMonths, "Journey duration must be positive.");

        Id = id;
        Name = name;
        SpecialtyTrack = specialtyTrack;
        PrimeCulturalAssociation = primeCulturalAssociation;
        PrestigeTier = prestigeTier;
        DistanceTier = distanceTier;
        BaseRiskLevel = baseRiskLevel;
        JourneyDurationMonths = journeyDurationMonths;
        CostPerMonth = costPerMonth;
    }

    public DefinitionId<InstitutionOfRenown> Id { get; }
    public string Name { get; }
    public DefinitionId<EducationTrack> SpecialtyTrack { get; }
    public DefinitionId<Identity.Culture> PrimeCulturalAssociation { get; }
    public InstitutionPrestigeTier PrestigeTier { get; }
    public DistanceTier DistanceTier { get; }
    public RouteRiskLevel BaseRiskLevel { get; }

    /// <summary>How many months of study at the institution complete the Journey (distinct from Travel
    /// time getting there/back) — read by <see cref="StudyAbroadProgressSystem"/>.</summary>
    public int JourneyDurationMonths { get; }

    public Money CostPerMonth { get; }
}

/// <summary>Rejects duplicate institution IDs at construction — mirrors <see
/// cref="EducationTrackCatalog"/>'s identical shape.</summary>
public sealed class InstitutionCatalog
{
    private readonly Dictionary<string, InstitutionOfRenown> _entries;

    public InstitutionCatalog(IEnumerable<InstitutionOfRenown> definitions)
    {
        if (definitions is null)
            throw new ArgumentNullException(nameof(definitions));

        var map = new Dictionary<string, InstitutionOfRenown>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (!map.TryAdd(definition.Id.Value, definition))
                throw new ArgumentException($"Duplicate institution ID '{definition.Id.Value}'.", nameof(definitions));
        }

        _entries = map;
    }

    public int Count => _entries.Count;

    public bool TryGet(DefinitionId<InstitutionOfRenown> id, out InstitutionOfRenown definition) =>
        _entries.TryGetValue(id.Value, out definition!);

    public InstitutionOfRenown Get(DefinitionId<InstitutionOfRenown> id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"No institution is registered for ID '{id.Value}'.");

    public IEnumerable<InstitutionOfRenown> All() => _entries.Values;
}
