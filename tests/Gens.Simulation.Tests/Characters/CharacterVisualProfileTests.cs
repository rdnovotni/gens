using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class CharacterVisualProfileTests
{
    [Test]
    public void TwoProfilesWithEqualButDistinctNotableFeatureListsAreEqual()
    {
        var first = BuildProfile(new List<NotableFeature> { NotableFeature.Scar });
        var second = BuildProfile(new[] { NotableFeature.Scar });

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        });
    }

    [Test]
    public void ProfilesWithDifferentNotableFeaturesAreNotEqual()
    {
        var first = BuildProfile(Array.Empty<NotableFeature>());
        var second = BuildProfile(new[] { NotableFeature.Scar });

        Assert.That(first, Is.Not.EqualTo(second));
    }

    private static CharacterVisualProfile BuildProfile(IReadOnlyList<NotableFeature> notableFeatures) => new()
    {
        Height = Height.Average,
        Build = Build.Average,
        FacialStructure = FacialStructure.Oval,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Brown,
        NotableFeatures = notableFeatures,
        Portrait = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, notableFeatures),
    };
}
