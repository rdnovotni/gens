using Gens.Simulation.Characters;
using Gens.Simulation.Random;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterVisualProfileGeneratorTests
{
    [Test]
    public void SameSeedProducesTheSameProfileEveryTime()
    {
        var first = CharacterVisualProfileGenerator.Generate(Streams(7), "appearance");
        var second = CharacterVisualProfileGenerator.Generate(Streams(7), "appearance");

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void DifferentSeedsCanProduceDifferentProfiles()
    {
        var first = CharacterVisualProfileGenerator.Generate(Streams(1), "appearance");
        var second = CharacterVisualProfileGenerator.Generate(Streams(2), "appearance");

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void GeneratedPortraitMatchesThePortraitRecipeGeneratorOutputForTheSameAttributes()
    {
        var profile = CharacterVisualProfileGenerator.Generate(Streams(3), "appearance");

        var expectedPortrait = PortraitRecipeGenerator.Generate(
            profile.Height, profile.Build, profile.FacialStructure, profile.Complexion,
            profile.HairColor, profile.HairStyle, profile.EyeColor, profile.NotableFeatures);

        Assert.That(profile.Portrait, Is.EqualTo(expectedPortrait));
    }

    [Test]
    public void RunningManyDrawsNeverProducesMoreNotableFeaturesThanTheCatalogDefines()
    {
        for (ulong seed = 0; seed < 50; seed++)
        {
            var profile = CharacterVisualProfileGenerator.Generate(Streams(seed), "appearance");
            Assert.That(profile.NotableFeatures.Count, Is.LessThanOrEqualTo(5));
            Assert.That(profile.NotableFeatures.Distinct().Count(), Is.EqualTo(profile.NotableFeatures.Count));
        }
    }

    [Test]
    public void RejectsANullStreamSet() =>
        Assert.That(() => CharacterVisualProfileGenerator.Generate(null!, "appearance"), Throws.ArgumentNullException);

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("appearance", seed, 1);
        return streams;
    }
}
