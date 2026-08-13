using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterTests
{
    [Test]
    public void CreateBuildsAValidCharacterFromValidFields()
    {
        var character = CharacterTestFixtures.Minimal(NextId());

        Assert.That(character.Praenomen, Is.EqualTo("Marcus"));
        Assert.That(character.LegalStatus, Is.EqualTo(LegalStatus.RomanCitizen));
        Assert.That(character.SocialClass, Is.EqualTo(SocialClass.Plebeian));
    }

    [Test]
    public void CreateRejectsAnEmptyPraenomen()
    {
        Assert.That(
            () => Character.Create(
                id: NextId(),
                praenomen: "",
                nomen: "Aurelius",
                cognomen: null,
                sex: Sex.Male,
                birthDate: new GameDate(0),
                visualProfile: CharacterTestFixtures.MinimalVisualProfile,
                status: LegalStatus.RomanCitizen,
                socialClass: null,
                culture: new DefinitionId<Culture>("roman"),
                location: default,
                household: null,
                attributes: new CoreAttributes(10, 10, 10, 10, 10),
                skills: new LaborSkills(10, 10, 10, 10, 10),
                condition: new Condition(50, 0, 50, 20, 50),
                source: CharacterSource.Familia,
                instantiatedAtMonth: 0),
            Throws.ArgumentException);
    }

    [Test]
    public void CreateRejectsAnEmptyNomen()
    {
        Assert.That(
            () => Character.Create(
                id: NextId(),
                praenomen: "Numa",
                nomen: "",
                cognomen: null,
                sex: Sex.Male,
                birthDate: new GameDate(0),
                visualProfile: CharacterTestFixtures.MinimalVisualProfile,
                status: LegalStatus.RomanCitizen,
                socialClass: null,
                culture: new DefinitionId<Culture>("roman"),
                location: default,
                household: null,
                attributes: new CoreAttributes(10, 10, 10, 10, 10),
                skills: new LaborSkills(10, 10, 10, 10, 10),
                condition: new Condition(50, 0, 50, 20, 50),
                source: CharacterSource.Familia,
                instantiatedAtMonth: 0),
            Throws.ArgumentException);
    }

    [Test]
    public void CreateRejectsANullVisualProfile()
    {
        Assert.That(
            () => Character.Create(
                id: NextId(),
                praenomen: "Numa",
                nomen: "Pompilius",
                cognomen: null,
                sex: Sex.Male,
                birthDate: new GameDate(0),
                visualProfile: null!,
                status: LegalStatus.RomanCitizen,
                socialClass: null,
                culture: new DefinitionId<Culture>("roman"),
                location: default,
                household: null,
                attributes: new CoreAttributes(10, 10, 10, 10, 10),
                skills: new LaborSkills(10, 10, 10, 10, 10),
                condition: new Condition(50, 0, 50, 20, 50),
                source: CharacterSource.Familia,
                instantiatedAtMonth: 0),
            Throws.ArgumentNullException);
    }

    [Test]
    public void CreateRejectsASocialClassOnANonCitizen()
    {
        Assert.That(
            () => Character.Create(
                id: NextId(),
                praenomen: "Numa",
                nomen: "Pompilius",
                cognomen: null,
                sex: Sex.Male,
                birthDate: new GameDate(0),
                visualProfile: CharacterTestFixtures.MinimalVisualProfile,
                status: LegalStatus.Peregrine,
                socialClass: SocialClass.Plebeian,
                culture: new DefinitionId<Culture>("roman"),
                location: default,
                household: null,
                attributes: new CoreAttributes(10, 10, 10, 10, 10),
                skills: new LaborSkills(10, 10, 10, 10, 10),
                condition: new Condition(50, 0, 50, 20, 50),
                source: CharacterSource.Familia,
                instantiatedAtMonth: 0),
            Throws.ArgumentException);
    }

    [TestCase(0, LifecycleStage.Infant)]
    [TestCase(3, LifecycleStage.Infant)]
    [TestCase(4, LifecycleStage.Child)]
    [TestCase(12, LifecycleStage.Child)]
    [TestCase(13, LifecycleStage.Adolescent)]
    [TestCase(17, LifecycleStage.Adolescent)]
    [TestCase(18, LifecycleStage.Adult)]
    [TestCase(59, LifecycleStage.Adult)]
    [TestCase(60, LifecycleStage.Elderly)]
    [TestCase(90, LifecycleStage.Elderly)]
    public void GetLifecycleStageReturnsTheBandForTheGivenAge(int ageInYears, LifecycleStage expected)
    {
        var character = CharacterTestFixtures.Minimal(NextId());
        var asOf = new GameDate(character.BirthDate.TotalMonths + ageInYears * 12);

        Assert.That(character.GetLifecycleStage(asOf), Is.EqualTo(expected));
    }

    [Test]
    public void GetLifecycleStageRejectsADateBeforeBirth()
    {
        var character = CharacterTestFixtures.Minimal(NextId());
        var beforeBirth = new GameDate(character.BirthDate.TotalMonths - 1);

        Assert.That(() => character.GetLifecycleStage(beforeBirth), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    private static RuntimeId<Character> NextId() => new RuntimeIdCounter<Character>().Issue();
}
