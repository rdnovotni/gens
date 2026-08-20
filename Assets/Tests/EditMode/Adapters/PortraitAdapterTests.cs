#nullable enable

using Gens.Presentation.Adapters;
using Gens.Simulation.Characters;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Gens.Presentation.Tests.EditMode.Adapters;

public sealed class PortraitAdapterTests
{
    [Test]
    public void AdaptIsDeterministicForTheSameVisualProfile()
    {
        var profile = Profile();

        var first = PortraitAdapter.Adapt("Marcus Aurelius", profile);
        var second = PortraitAdapter.Adapt("Marcus Aurelius", profile);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void AdaptAlwaysPicksAColorFromTheMedallionPalette()
    {
        var palette = new[] { "#5C3350", "#9C4B2E", "#6E8272", "#B9922E", "#7A6A9C", "#4E6B57", "#8C5A3C", "#5E5140" };

        for (var seed = 0; seed < 25; seed++)
        {
            var viewModel = PortraitAdapter.Adapt("Marcus Aurelius", Profile(seed));
            Assert.That(palette, Does.Contain(viewModel.BackgroundColorHex));
        }
    }

    [Test]
    public void MonogramCombinesFirstAndLastInitialsForAMultiWordName()
    {
        var viewModel = PortraitAdapter.Adapt("Marcus Aurelius Antoninus", Profile());

        Assert.That(viewModel.MonogramLabel, Is.EqualTo("MA"));
    }

    [Test]
    public void MonogramUsesTheSoleInitialForASingleWordName()
    {
        var viewModel = PortraitAdapter.Adapt("Spartacus", Profile());

        Assert.That(viewModel.MonogramLabel, Is.EqualTo("S"));
    }

    [Test]
    public void MonogramFallsBackToAnEmDashForAnEmptyName()
    {
        var viewModel = PortraitAdapter.Adapt(string.Empty, Profile());

        Assert.That(viewModel.MonogramLabel, Is.EqualTo("—"));
    }

    [Test]
    public void AdaptThrowsOnNullVisualProfile()
    {
        Assert.That(() => PortraitAdapter.Adapt("Marcus", null!), Throws.TypeOf<System.ArgumentNullException>());
    }

    [Test]
    public void BindingSetsTheMonogramLabelAndParsesTheBackgroundColor()
    {
        var root = new VisualElement();
        var frame = new VisualElement { name = PortraitBinding.FrameElementName };
        root.Add(frame);
        root.Add(new Label { name = PortraitBinding.MonogramLabelName });

        PortraitBinding.Apply(root, new PortraitViewModel("MA", "#5C3350"));

        Assert.That(root.Q<Label>(PortraitBinding.MonogramLabelName).text, Is.EqualTo("MA"));
        Assert.That(frame.style.backgroundColor.keyword, Is.EqualTo(StyleKeyword.Undefined));
    }

    private static CharacterVisualProfile Profile(int seed = 0) => new()
    {
        Height = Height.Average,
        Build = Build.Average,
        FacialStructure = FacialStructure.Oval,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Brown,
        NotableFeatures = System.Array.Empty<NotableFeature>(),
        Portrait = PortraitRecipeGenerator.Generate(
            (Height)(seed % 3), Build.Average, FacialStructure.Oval, Complexion.Olive,
            HairColor.Brown, HairStyle.Cropped, EyeColor.Brown, System.Array.Empty<NotableFeature>()),
    };
}
