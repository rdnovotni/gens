using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Education;

/// <summary>
/// The real, authored three-Track roster (Phase 17 item 2; §3) — mirrors <c>
/// content/source/education/tracks.json</c> exactly, matching <see cref="Cultures.KnownWorldCultures"/>'s
/// identical "real content, hand-authored in C# for the runtime, JSON validated separately" precedent.
/// </summary>
public static class KnownEducationTracks
{
    public static readonly DefinitionId<EducationTrack> Rhetoric = new("rhetoric");
    public static readonly DefinitionId<EducationTrack> Philosophy = new("philosophy");
    public static readonly DefinitionId<EducationTrack> Gymnasium = new("gymnasium");

    public static readonly DefinitionId<Building> Schola = new("schola");
    public static readonly DefinitionId<Building> Academia = new("academia");
    public static readonly DefinitionId<Building> GymnasiumBuilding = new("gymnasium");
    public static readonly DefinitionId<Building> Palaestra = new("palaestra");

    public static readonly EducationTrackCatalog Catalog = new(new[]
    {
        new EducationTrackDefinition(
            Rhetoric, "Rhetoric", new[] { Schola },
            PermanentInjuryTarget.Diplomacy, monthlyAttributeGain: 1, completionMonths: 24,
            distinguishedTierCostPerMonth: Money.FromDenarii(20), distinguishedTierMultiplier: 2,
            satisfiesMagistracyContestGate: true),
        new EducationTrackDefinition(
            Philosophy, "Philosophy", new[] { Academia },
            PermanentInjuryTarget.Learning, monthlyAttributeGain: 1, completionMonths: 24,
            distinguishedTierCostPerMonth: Money.FromDenarii(20), distinguishedTierMultiplier: 2,
            grantsCulturalPrestige: true, monthlyPrestigeGain: 1),
        new EducationTrackDefinition(
            Gymnasium, "Gymnasium", new[] { GymnasiumBuilding, Palaestra },
            PermanentInjuryTarget.Martial, monthlyAttributeGain: 1, completionMonths: 24,
            distinguishedTierCostPerMonth: Money.FromDenarii(15), distinguishedTierMultiplier: 2,
            secondaryAttribute: PermanentInjuryTarget.Learning),
    });
}
