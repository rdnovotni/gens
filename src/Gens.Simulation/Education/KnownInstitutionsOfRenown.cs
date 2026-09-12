using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Education;

/// <summary>
/// The real, fixed five-Institution roster (Phase 17 item 2; §12) — mirrors <c>
/// content/source/institutions/institutions.json</c> exactly, matching <see
/// cref="KnownEducationTracks"/>'s and <see cref="KnownWorldCultures"/>'s identical "hand-authored
/// runtime catalog, JSON validated separately" precedent. Rhodes is the one institution whose credential
/// also satisfies <see cref="EducationGateResolver.CanContestMagistracyAboveLowestRung"/> (§3/§12's own
/// cross-reference) — see that method's own doc comment.
/// </summary>
public static class KnownInstitutionsOfRenown
{
    public static readonly DefinitionId<InstitutionOfRenown> Athens = new("athens");
    public static readonly DefinitionId<InstitutionOfRenown> Rhodes = new("rhodes");
    public static readonly DefinitionId<InstitutionOfRenown> Alexandria = new("alexandria");
    public static readonly DefinitionId<InstitutionOfRenown> Pergamon = new("pergamon");
    public static readonly DefinitionId<InstitutionOfRenown> Massilia = new("massilia");

    public static readonly InstitutionCatalog Catalog = new(new[]
    {
        new InstitutionOfRenown(
            Athens, "Athens", KnownEducationTracks.Rhetoric, KnownWorldCultures.Hellenic, InstitutionPrestigeTier.Legendary,
            DistanceTier.Far, RouteRiskLevel.Dangerous, journeyDurationMonths: 24, Money.FromDenarii(25)),
        new InstitutionOfRenown(
            Rhodes, "Rhodes", KnownEducationTracks.Rhetoric, KnownWorldCultures.Hellenic, InstitutionPrestigeTier.Renowned,
            DistanceTier.Far, RouteRiskLevel.Dangerous, journeyDurationMonths: 24, Money.FromDenarii(25)),
        new InstitutionOfRenown(
            Alexandria, "Alexandria", KnownEducationTracks.Philosophy, KnownWorldCultures.AlexandrianGreek, InstitutionPrestigeTier.Legendary,
            DistanceTier.Far, RouteRiskLevel.Dangerous, journeyDurationMonths: 24, Money.FromDenarii(30)),
        new InstitutionOfRenown(
            Pergamon, "Pergamon", KnownEducationTracks.Philosophy, KnownWorldCultures.Hellenic, InstitutionPrestigeTier.Renowned,
            DistanceTier.Far, RouteRiskLevel.Dangerous, journeyDurationMonths: 24, Money.FromDenarii(25)),
        new InstitutionOfRenown(
            Massilia, "Massilia", KnownEducationTracks.Rhetoric, KnownWorldCultures.Hellenic, InstitutionPrestigeTier.Notable,
            DistanceTier.Moderate, RouteRiskLevel.Guarded, journeyDurationMonths: 18, Money.FromDenarii(20)),
    });
}
