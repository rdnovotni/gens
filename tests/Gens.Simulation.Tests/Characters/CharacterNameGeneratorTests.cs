using Gens.Simulation.Characters;
using Gens.Simulation.Random;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterNameGeneratorTests
{
    [Test]
    public void SameSeedProducesTheSameNameEveryTime()
    {
        var first = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);
        var second = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void MaleCitizenGetsAPraenomenAndNomenFromThePool()
    {
        var name = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman);

        Assert.Multiple(() =>
        {
            Assert.That(NamePoolTestFixtures.Roman.Praenomina, Does.Contain(name.Praenomen));
            Assert.That(NamePoolTestFixtures.Roman.Nomina, Does.Contain(name.Nomen));
        });
    }

    [Test]
    public void MaleCitizenNeverGetsACognomenWhenThePoolHasNone()
    {
        var name = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.NoCognomina);

        Assert.That(name.Cognomen, Is.Null);
    }

    [Test]
    public void MaleCitizenHonorsAFixedNomen()
    {
        var name = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman,
            fixedNomen: "Cornelius");

        Assert.That(name.Nomen, Is.EqualTo("Cornelius"));
    }

    [Test]
    public void FemaleCitizenTakesTheFeminizedNomenAsBothPraenomenAndNomen()
    {
        var name = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Female, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman,
            fixedNomen: "Aurelius");

        Assert.Multiple(() =>
        {
            Assert.That(name.Praenomen, Is.EqualTo("Aurelia"));
            Assert.That(name.Nomen, Is.EqualTo("Aurelia"));
        });
    }

    [Test]
    public void FemaleCitizenCarriesAnOrdinalCognomenWhenDisambiguationIsRequested()
    {
        var name = CharacterNameGenerator.Generate(
            Streams(1), "names", Sex.Female, LegalStatus.RomanCitizen, NamePoolTestFixtures.Roman,
            fixedNomen: "Aurelius", femaleOrdinal: 2);

        Assert.That(name.Cognomen, Is.EqualTo("Secunda"));
    }

    [TestCase(LegalStatus.Enslaved)]
    [TestCase(LegalStatus.Peregrine)]
    public void EnslavedAndPeregrineCharactersGetASingleDuplicatedName(LegalStatus status)
    {
        var name = CharacterNameGenerator.Generate(Streams(1), "names", Sex.Female, status, NamePoolTestFixtures.Roman);

        Assert.Multiple(() =>
        {
            Assert.That(name.Praenomen, Is.EqualTo(name.Nomen));
            Assert.That(NamePoolTestFixtures.Roman.GivenNames, Does.Contain(name.Praenomen));
            Assert.That(name.Cognomen, Is.Null);
        });
    }

    [TestCase("Aurelius", "Aurelia")]
    [TestCase("Fabius", "Fabia")]
    [TestCase("Bato", "Batoa")]
    public void FeminizeAppliesTheExpectedTransform(string nomen, string expected) =>
        Assert.That(CharacterNameGenerator.Feminize(nomen), Is.EqualTo(expected));

    [Test]
    public void FeminizeRejectsAnEmptyNomen() =>
        Assert.That(() => CharacterNameGenerator.Feminize(""), Throws.ArgumentException);

    [Test]
    public void GenerateRejectsAnEmptyPool()
    {
        var emptyPool = new NamePool
        {
            Praenomina = Array.Empty<string>(),
            Nomina = new[] { "Aurelius" },
            Cognomina = Array.Empty<string>(),
            GivenNames = new[] { "Bato" },
        };

        Assert.That(
            () => CharacterNameGenerator.Generate(Streams(1), "names", Sex.Male, LegalStatus.RomanCitizen, emptyPool),
            Throws.ArgumentException);
    }

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("names", seed, 1);
        return streams;
    }
}
