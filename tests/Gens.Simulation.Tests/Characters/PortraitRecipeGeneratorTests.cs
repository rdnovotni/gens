using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class PortraitRecipeGeneratorTests
{
    [Test]
    public void ProducesOneLayerPerFixedAttributeWithNoNotableFeatures()
    {
        var recipe = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, Array.Empty<NotableFeature>());

        Assert.That(recipe.Layers, Is.EqualTo(new[]
        {
            "body/height/average",
            "body/build/average",
            "face/structure/oval",
            "face/complexion/olive",
            "eyes/color/brown",
            "hair/color/brown",
            "hair/style/cropped",
        }));
    }

    [Test]
    public void KebabCasesAMultiWordEnumValue()
    {
        var recipe = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.ShoulderLength, EyeColor.Brown, Array.Empty<NotableFeature>());

        Assert.That(recipe.Layers, Does.Contain("hair/style/shoulder-length"));
    }

    [Test]
    public void NotableFeatureLayersFollowTheFixedCatalogOrderRegardlessOfInputOrder()
    {
        var recipe = PortraitRecipeGenerator.Generate(
            Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown,
            new[] { NotableFeature.GrayAtTemples, NotableFeature.Scar });

        var featureLayers = recipe.Layers.Where(layer => layer.StartsWith("feature/", StringComparison.Ordinal)).ToArray();

        Assert.That(featureLayers, Is.EqualTo(new[] { "feature/scar", "feature/gray-at-temples" }));
    }

    [Test]
    public void RejectsNullNotableFeatures() =>
        Assert.That(
            () => PortraitRecipeGenerator.Generate(
                Height.Average, Build.Average, FacialStructure.Oval, Complexion.Olive,
                HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, null!),
            Throws.ArgumentNullException);

    [Test]
    public void TwoRecipesWithTheSameLayersInDifferentInstancesAreEqual()
    {
        var first = new PortraitRecipe { Layers = new[] { "a", "b" } };
        var second = new PortraitRecipe { Layers = new List<string> { "a", "b" } };

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        });
    }
}
