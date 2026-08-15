using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Shared builder for a minimally valid <see cref="Character"/>, used by every test across
/// the suite that only needs "a" Character rather than a specific one (mirrors how other partitions'
/// tests previously used a bare placeholder value).</summary>
public static class CharacterTestFixtures
{
    public static readonly CharacterVisualProfile MinimalVisualProfile = new()
    {
        Height = Height.Average,
        Build = Build.Average,
        FacialStructure = FacialStructure.Oval,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Brown,
        NotableFeatures = Array.Empty<NotableFeature>(),
        Portrait = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, Array.Empty<NotableFeature>()),
    };

    public static Character Minimal(
        RuntimeId<Character> id,
        string praenomen = "Marcus",
        string nomen = "Aurelius",
        CharacterVisualProfile? visualProfile = null,
        GameDate? birthDate = null,
        RuntimeId<Character>? motherId = null,
        RuntimeId<Character>? fatherId = null,
        Legitimacy legitimacy = Legitimacy.Legitimate,
        IReadOnlyList<MarriageRecord>? maritalHistory = null,
        IReadOnlyList<PermanentInjury>? permanentInjuries = null,
        IReadOnlyList<DefinitionId<Trait>>? traits = null,
        DeathRecord? deathRecord = null,
        Condition? condition = null,
        RuntimeId<Settlement>? location = null,
        RuntimeId<Household>? household = null,
        LaborSkills? skills = null,
        DutyAssignment? duty = null,
        bool backfilledHistory = false,
        LegalStatus status = LegalStatus.RomanCitizen,
        RegimenSettings? regimen = null,
        FledRecord? flight = null,
        PursuitRecord? pursuit = null,
        ManumissionPlan? manumissionPlan = null) =>
        Character.Create(
            id: id,
            praenomen: praenomen,
            nomen: nomen,
            cognomen: null,
            sex: Sex.Male,
            birthDate: birthDate ?? new GameDate(0),
            visualProfile: visualProfile ?? MinimalVisualProfile,
            status: status,
            socialClass: status == LegalStatus.RomanCitizen ? SocialClass.Plebeian : null,
            culture: new DefinitionId<Culture>("roman"),
            location: location ?? default(RuntimeId<Settlement>),
            household: household,
            attributes: new CoreAttributes(10, 10, 10, 10, 10),
            skills: skills ?? new LaborSkills(10, 10, 10, 10, 10),
            condition: condition ?? new Condition(80, 0, 50, 20, 50),
            source: CharacterSource.Familia,
            instantiatedAtMonth: 0,
            motherId: motherId,
            fatherId: fatherId,
            legitimacy: legitimacy,
            maritalHistory: maritalHistory,
            permanentInjuries: permanentInjuries,
            traits: traits,
            deathRecord: deathRecord,
            duty: duty,
            backfilledHistory: backfilledHistory,
            regimen: regimen,
            flight: flight,
            pursuit: pursuit,
            manumissionPlan: manumissionPlan);
}
