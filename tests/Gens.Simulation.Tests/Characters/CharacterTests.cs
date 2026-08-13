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

    [Test]
    public void CreateRejectsACharacterAsItsOwnMother()
    {
        var id = NextId();

        Assert.That(() => CharacterTestFixtures.Minimal(id, motherId: id), Throws.ArgumentException);
    }

    [Test]
    public void CreateRejectsACharacterAsItsOwnFather()
    {
        var id = NextId();

        Assert.That(() => CharacterTestFixtures.Minimal(id, fatherId: id), Throws.ArgumentException);
    }

    [Test]
    public void IsAliveIsTrueWithNoDeathRecord()
    {
        var character = CharacterTestFixtures.Minimal(NextId());

        Assert.That(character.IsAlive, Is.True);
    }

    [Test]
    public void IsAliveIsFalseWithADeathRecord()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            deathRecord: new DeathRecord(new GameDate(10), DeathCause.OldAge, 60));

        Assert.That(character.IsAlive, Is.False);
    }

    [Test]
    public void CurrentSpouseIdIsNullWithNoMaritalHistory()
    {
        var character = CharacterTestFixtures.Minimal(NextId());

        Assert.That(character.CurrentSpouseId, Is.Null);
    }

    [Test]
    public void CurrentSpouseIdReturnsTheOpenMarriageEntry()
    {
        var spouseId = NextId();
        var character = CharacterTestFixtures.Minimal(NextId(),
            maritalHistory: new[] { new MarriageRecord(spouseId, new GameDate(0), null, null) });

        Assert.That(character.CurrentSpouseId, Is.EqualTo(spouseId));
    }

    [Test]
    public void CurrentSpouseIdIsNullWhenTheOnlyMarriageIsClosed()
    {
        var spouseId = NextId();
        var character = CharacterTestFixtures.Minimal(NextId(),
            maritalHistory: new[]
            {
                new MarriageRecord(spouseId, new GameDate(0), new GameDate(12), MarriageEndReason.Divorce),
            });

        Assert.That(character.CurrentSpouseId, Is.Null);
    }

    [Test]
    public void GetEffectiveAttributesSubtractsMatchingInjuryMagnitude()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            permanentInjuries: new[] { new PermanentInjury(PermanentInjuryTarget.Martial, 4, "battlefield wound", new GameDate(0)) });

        var effective = character.GetEffectiveAttributes();

        Assert.That(effective.Martial, Is.EqualTo(6));
        Assert.That(effective.Diplomacy, Is.EqualTo(character.Attributes.Diplomacy));
    }

    [Test]
    public void GetEffectiveAttributesSumsMultipleInjuriesOnTheSameTargetAndClampsToZero()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            permanentInjuries: new[]
            {
                new PermanentInjury(PermanentInjuryTarget.Martial, 60, "wound one", new GameDate(0)),
                new PermanentInjury(PermanentInjuryTarget.Martial, 60, "wound two", new GameDate(1)),
            });

        Assert.That(character.GetEffectiveAttributes().Martial, Is.EqualTo(0));
    }

    [Test]
    public void GetEffectiveSkillsSubtractsMatchingInjuryMagnitude()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            permanentInjuries: new[] { new PermanentInjury(PermanentInjuryTarget.Fieldwork, 3, "lamed leg", new GameDate(0)) });

        Assert.That(character.GetEffectiveSkills().Fieldwork, Is.EqualTo(7));
    }

    [Test]
    public void GetEffectiveFertilitySubtractsMatchingInjuryMagnitude()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            condition: new Condition(80, 0, 50, 20, 50),
            permanentInjuries: new[] { new PermanentInjury(PermanentInjuryTarget.Fertility, 20, "difficult birth", new GameDate(0)) });

        Assert.That(character.GetEffectiveFertility(), Is.EqualTo(30));
    }

    [Test]
    public void GetEffectiveFertilityIgnoresInjuriesOnOtherTargets()
    {
        var character = CharacterTestFixtures.Minimal(NextId(),
            condition: new Condition(80, 0, 50, 20, 50),
            permanentInjuries: new[] { new PermanentInjury(PermanentInjuryTarget.Martial, 20, "wound", new GameDate(0)) });

        Assert.That(character.GetEffectiveFertility(), Is.EqualTo(50));
    }

    private static RuntimeId<Character> NextId() => new RuntimeIdCounter<Character>().Issue();
}
