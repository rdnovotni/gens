using Gens.Simulation.Identity;

namespace Gens.Simulation.Health;

/// <summary>Which real-world driver (<c>gens-disease-public-health-design.md</c> §2's table) an
/// endemic disease's <see cref="EndemicIllnessSystem"/> exposure roll reads from. Every value maps to
/// data this codebase can actually compute today (Phase 14 item 2's own scoping discipline, matching
/// item 1's "deliberately scoped narrower than the design doc" precedent): <see cref="MarshTerrain"/>
/// and <see cref="MiningProximity"/> read <see cref="Land.Plot.Terrain"/> composition;
/// <see cref="PoorSanitation"/> reads <see cref="SettlementSanitationInvestment"/> alone (no Aqueduct/
/// Latrines building content exists yet — see that record's own doc comment); <see
/// cref="PopulationDensity"/> reads a settlement's living-Character-plus-<see
/// cref="Characters.PopGroup"/> headcount against its Plot capacity as a crowding proxy (Settlement
/// Demographics' own Overcrowding/Insulae mechanic does not exist yet); <see cref="LavishDiet"/> reads
/// whether any <see cref="Characters.PopGroup"/> at the settlement sits at <see
/// cref="Characters.WealthBand.EliteDiscretionary"/> with <see cref="Characters.DietTier.Generous"/>
/// (the closest real proxy to §2's "Lavish consumption tier," which Settlement Demographics itself has
/// not yet authored past <see cref="Characters.DietTier.Generous"/>); <see cref="LeadWealthOrMining"/>
/// is Saturnism's own two-unrelated-drivers shape (§2), evaluated as <see cref="LavishDiet"/> OR <see
/// cref="MiningProximity"/> by <see cref="EndemicExposureCalculator.SaturnismMonthlyProbability"/>
/// specifically; <see cref="TimeOnly"/> (Leprosy) and <see cref="RegionalFlavorUnmodeled"/>
/// (Ophthalmia) are flat, driver-less baselines — Leprosy per §2's own "not terrain-driven at all...
/// purely a matter of exposure and time," Ophthalmia because no Region in this codebase yet carries a
/// real, gameplay-reachable arid/dust flag (<see cref="Regions.RegionProfileDefinition.TerrainProfileRef"/>
/// is a free-text ref, not a queryable value — see this file's own top-level doc comment for the
/// full disclosure).</summary>
public enum EndemicExposureDriver
{
    MarshTerrain,
    PoorSanitation,
    PopulationDensity,
    LavishDiet,
    MiningProximity,
    LeadWealthOrMining,
    TimeOnly,
    RegionalFlavorUnmodeled,
}

/// <summary>Content-authored metadata for one of §2's seven endemic diseases, layered on top of the
/// generic <see cref="HealthConditionDefinition"/> item 1 built (that record's Id/Name/Category/
/// HasCure only) — kept as a separate, parallel table rather than adding fields to
/// <see cref="HealthConditionDefinition"/> itself, since <see cref="EndemicExposureDriver"/> and
/// <see cref="SocialExclusion"/> are meaningless for the four epidemic diseases sharing that same base
/// shape (see <see cref="EpidemicDiseaseProfile"/> for their own parallel table).</summary>
public sealed record EndemicDiseaseProfile(
    DefinitionId<HealthConditionDefinition> ConditionId,
    EndemicExposureDriver Driver,
    bool SocialExclusion);

/// <summary>§3.2's vector column: whether an epidemic disease spreads through <see
/// cref="PersonToPerson"/> contact (household co-membership — see <see
/// cref="EpidemicContagionSystem"/>'s own doc comment for exactly which contact graph is real here)
/// or, uniquely for Enteric Fever, through the settlement's own water supply (<see
/// cref="Waterborne"/> — §3.2's "doesn't spread through Group Interactions or shared housing... a
/// contaminated Aqueduct or a post-Flood water supply").</summary>
public enum EpidemicVector
{
    PersonToPerson,
    Waterborne,
}

/// <summary>Content-authored metadata for one of §3.2's four epidemic diseases, the same "parallel
/// table next to the generic <see cref="HealthConditionDefinition"/>" shape as <see
/// cref="EndemicDiseaseProfile"/>.</summary>
public sealed record EpidemicDiseaseProfile(DefinitionId<HealthConditionDefinition> ConditionId, EpidemicVector Vector);

/// <summary>The real, named seven-endemic/four-epidemic disease roster §2/§3.2 describe — the content
/// Phase 14 item 1's <see cref="HealthConditionCatalog"/> was deliberately left empty for. Every
/// numeric severity/onset figure a caller needs beyond this static content lives in <see
/// cref="EndemicExposureCalculator"/>/<see cref="EpidemicSpreadCalculator"/> instead, matching item 1's
/// own "content is static, math is a separate pure calculator" split. <b>Deliberately not authored
/// here:</b> Saturnism's two real drivers are both present in the design corpus (elite lead-sweetened
/// wine and cookware vs. Iberian mining proximity) but this codebase has no Domus-stage
/// plumbing/cookware choice and no Region carrying a real, gameplay-reachable "Iberian" flag yet
/// (<see cref="Regions.SampleRegionProfileDefinitions"/> is the only Region content that exists, and it
/// is an explicitly generic fixture) — so Saturnism's mining driver here reads Hills-terrain
/// proximity anywhere, not "Hills terrain specifically in the Iberian colony," an honest widening
/// disclosed in <see cref="EndemicExposureCalculator.SaturnismMonthlyProbability"/>'s own doc
/// comment.</summary>
public static class DiseaseCatalog
{
    public static readonly DefinitionId<HealthConditionDefinition> RomanFever = new("disease-roman-fever");
    public static readonly DefinitionId<HealthConditionDefinition> TheFlux = new("disease-the-flux");
    public static readonly DefinitionId<HealthConditionDefinition> Ophthalmia = new("disease-ophthalmia");
    public static readonly DefinitionId<HealthConditionDefinition> Consumption = new("disease-consumption");
    public static readonly DefinitionId<HealthConditionDefinition> Leprosy = new("disease-leprosy");
    public static readonly DefinitionId<HealthConditionDefinition> Gout = new("disease-gout");
    public static readonly DefinitionId<HealthConditionDefinition> Saturnism = new("disease-saturnism");

    public static readonly DefinitionId<HealthConditionDefinition> Pestilence = new("disease-pestilence");
    public static readonly DefinitionId<HealthConditionDefinition> Pox = new("disease-pox");
    public static readonly DefinitionId<HealthConditionDefinition> CampFever = new("disease-camp-fever");
    public static readonly DefinitionId<HealthConditionDefinition> EntericFever = new("disease-enteric-fever");

    private static readonly EndemicDiseaseProfile[] EndemicProfilesArray =
    {
        new(RomanFever, EndemicExposureDriver.MarshTerrain, SocialExclusion: false),
        new(TheFlux, EndemicExposureDriver.PoorSanitation, SocialExclusion: false),
        new(Ophthalmia, EndemicExposureDriver.RegionalFlavorUnmodeled, SocialExclusion: false),
        new(Consumption, EndemicExposureDriver.PopulationDensity, SocialExclusion: false),
        new(Leprosy, EndemicExposureDriver.TimeOnly, SocialExclusion: true),
        new(Gout, EndemicExposureDriver.LavishDiet, SocialExclusion: false),
        new(Saturnism, EndemicExposureDriver.LeadWealthOrMining, SocialExclusion: false),
    };

    private static readonly EpidemicDiseaseProfile[] EpidemicProfilesArray =
    {
        new(Pestilence, EpidemicVector.PersonToPerson),
        new(Pox, EpidemicVector.PersonToPerson),
        new(CampFever, EpidemicVector.PersonToPerson),
        new(EntericFever, EpidemicVector.Waterborne),
    };

    public static IReadOnlyList<EndemicDiseaseProfile> EndemicProfiles => EndemicProfilesArray;
    public static IReadOnlyList<EpidemicDiseaseProfile> EpidemicProfiles => EpidemicProfilesArray;

    /// <summary>Builds the real content <see cref="HealthConditionCatalog"/> item 1 left empty. §7's
    /// "manages severity without guaranteeing a cure" is taken literally for every endemic illness
    /// (<see cref="HealthConditionDefinition.HasCure"/> false for all seven, per §2's own repeated "no
    /// real cure" framing across Roman Fever, Consumption, Leprosy, Saturnism, and the table's general
    /// tone for the rest). Among the epidemics, Enteric Fever alone is marked curable (<c>hasCure:
    /// true</c>) per §3.2's own "resolves faster than Pestilence when treated" — the one epidemic this
    /// document distinguishes as more tractable than the others, which stay <c>false</c> to preserve
    /// §3.3's real, meaningful mortality risk.</summary>
    public static HealthConditionCatalog BuildConditionCatalog() => new(new[]
    {
        new HealthConditionDefinition(RomanFever, "Roman Fever", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(TheFlux, "The Flux", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Ophthalmia, "Ophthalmia", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Consumption, "Consumption", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Leprosy, "Leprosy", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Gout, "Gout", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Saturnism, "Saturnism", HealthConditionCategory.Chronic, hasCure: false),
        new HealthConditionDefinition(Pestilence, "Pestilence", HealthConditionCategory.Acute, hasCure: false),
        new HealthConditionDefinition(Pox, "Pox", HealthConditionCategory.Acute, hasCure: false),
        new HealthConditionDefinition(CampFever, "Camp Fever", HealthConditionCategory.Acute, hasCure: false),
        new HealthConditionDefinition(EntericFever, "Enteric Fever", HealthConditionCategory.Acute, hasCure: true),
    });

    public static bool TryGetEndemicProfile(DefinitionId<HealthConditionDefinition> conditionId, out EndemicDiseaseProfile profile)
    {
        foreach (var candidate in EndemicProfilesArray)
        {
            if (candidate.ConditionId == conditionId)
            {
                profile = candidate;
                return true;
            }
        }

        profile = null!;
        return false;
    }

    public static bool TryGetEpidemicProfile(DefinitionId<HealthConditionDefinition> conditionId, out EpidemicDiseaseProfile profile)
    {
        foreach (var candidate in EpidemicProfilesArray)
        {
            if (candidate.ConditionId == conditionId)
            {
                profile = candidate;
                return true;
            }
        }

        profile = null!;
        return false;
    }
}
