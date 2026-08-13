using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterIdentityGeneratorTests
{
    [Test]
    public void SameSeedProducesTheSameNameAndVisualProfile()
    {
        var first = CharacterIdentityGenerator.Generate(
            Streams(5), "identity", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);
        var second = CharacterIdentityGenerator.Generate(
            Streams(5), "identity", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);

        Assert.Multiple(() =>
        {
            Assert.That(second.Name, Is.EqualTo(first.Name));
            Assert.That(second.Visual, Is.EqualTo(first.Visual));
        });
    }

    [Test]
    public void GeneratedIdentityFeedsDirectlyIntoCharacterCreate()
    {
        var identity = CharacterIdentityGenerator.Generate(
            Streams(9), "identity", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);

        var character = Character.Create(
            id: new RuntimeIdCounter<Character>().Issue(),
            praenomen: identity.Name.Praenomen,
            nomen: identity.Name.Nomen,
            cognomen: identity.Name.Cognomen,
            sex: Sex.Male,
            birthDate: new GameDate(0),
            visualProfile: identity.Visual,
            status: LegalStatus.RomanCitizen,
            socialClass: SocialClass.Plebeian,
            culture: new DefinitionId<Culture>("roman"),
            location: default,
            household: null,
            attributes: new CoreAttributes(10, 10, 10, 10, 10),
            skills: new LaborSkills(10, 10, 10, 10, 10),
            condition: new Condition(80, 0, 50, 20, 50),
            source: CharacterSource.Familia,
            instantiatedAtMonth: 0);

        Assert.Multiple(() =>
        {
            Assert.That(character.Praenomen, Is.EqualTo(identity.Name.Praenomen));
            Assert.That(character.VisualProfile, Is.EqualTo(identity.Visual));
        });
    }

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("identity", seed, 1);
        return streams;
    }
}
