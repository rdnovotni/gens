using System.Security.Cryptography;
using Gens.Graphics.Skia;
using Gens.Presentation.Visuals;
using Gens.Simulation.Characters;
using NUnit.Framework;

namespace Gens.Portraits.Tests;

public sealed class PortraitPipelineTests
{
    private string cache = null!;
    [SetUp] public void SetUp() { cache = Path.Combine(TestContext.CurrentContext.WorkDirectory, "portrait-cache", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(cache)) Directory.Delete(cache, true); }

    [Test]
    public void VisualSeedDescriptionHashAndRecipeAreDeterministic()
    {
        CharacterVisualState first = State(), second = State();
        Assert.Multiple(() =>
        {
            Assert.That(first.VisualSeed, Is.EqualTo(second.VisualSeed));
            Assert.That(first.VisualStateHash, Is.EqualTo(second.VisualStateHash));
            Assert.That(AppearanceDescriptionBuilder.Build(first), Is.EqualTo(AppearanceDescriptionBuilder.Build(second)));
            Assert.That(PortraitRecipeBuilder.Build(first).CanonicalIdentity, Is.EqualTo(PortraitRecipeBuilder.Build(second).CanonicalIdentity));
        });
    }

    [TestCase(128)]
    [TestCase(256)]
    [TestCase(512)]
    public void SameRecipeAndRendererVersionProduceSamePixels(int size)
    {
        using var graphics = new SkiaGraphicsBackend(); var renderer = new ProceduralPortraitRenderer(graphics); var recipe = PortraitRecipeBuilder.Build(State());
        RenderedPortrait firstResult = renderer.Render(recipe, size), secondResult = renderer.Render(recipe, size);
        using var first = firstResult.Image; using var second = secondResult.Image; byte[] firstBytes = firstResult.PngBytes, secondBytes = secondResult.PngBytes;
        Assert.That(SHA256.HashData(firstBytes), Is.EqualTo(SHA256.HashData(secondBytes)));
    }

    [Test]
    public void ServiceUsesContentAddressedMemoryAndDiskCache()
    {
        using var graphics = new SkiaGraphicsBackend(); using var firstService = new PortraitService(graphics, cache);
        ResolvedPortrait first = firstService.Resolve(State(), 128), memory = firstService.Resolve(State(), 128);
        Assert.Multiple(() => { Assert.That(first.CacheHit, Is.False); Assert.That(memory.CacheHit, Is.True); Assert.That(memory.Reference.ContentId, Is.EqualTo(first.Reference.ContentId)); });
    }

    [Test]
    public void AppearanceChangesInvalidateWhileUnrelatedCallsDoNotMutateState()
    {
        CharacterVisualState first = State(); CharacterVisualState changed = first with { AgeBand = VisualAgeBand.Elderly };
        using var graphics = new SkiaGraphicsBackend(); using var service = new PortraitService(graphics, cache);
        string originalHash = first.VisualStateHash; ResolvedPortrait before = service.Resolve(first, 128), after = service.Resolve(changed, 128);
        Assert.Multiple(() => { Assert.That(first.VisualStateHash, Is.EqualTo(originalHash)); Assert.That(after.Reference.ContentId, Is.Not.EqualTo(before.Reference.ContentId)); Assert.That(VisualHash.Classify(first, changed), Is.EqualTo(VisualChangeKind.Major)); });
    }

    [Test]
    public void SnapshotRetainsImmutablePortraitReference()
    {
        using var graphics = new SkiaGraphicsBackend(); using var service = new PortraitService(graphics, cache); ResolvedPortrait portrait = service.Resolve(State(), 128);
        PortraitSnapshot snapshot = service.CaptureSnapshot("character:1", "03/100 CE", portrait);
        Assert.Multiple(() => { Assert.That(snapshot.SubjectId, Is.EqualTo("character:1")); Assert.That(snapshot.Portrait, Is.EqualTo(portrait.Reference)); Assert.That(service.Snapshots, Has.Count.EqualTo(1)); Assert.That(service.FindSnapshot("character:1", "03/100 CE"), Is.EqualTo(snapshot)); });
    }

    [Test]
    public void LayerManifestRejectsUnknownDuplicateAndIncompatibleReferences()
    {
        Gens.Presentation.Visuals.PortraitRecipe recipe = PortraitRecipeBuilder.Build(State());
        Assert.Multiple(() =>
        {
            Assert.That(() => PortraitLayerCatalog.Validate(recipe with { Layers = recipe.Layers.Append(new("detail", "portrait/detail/missing", "detail")).ToArray() }), Throws.TypeOf<InvalidDataException>());
            Assert.That(() => PortraitLayerCatalog.Validate(recipe with { Layers = recipe.Layers.Append(recipe.Layers[0]).ToArray() }), Throws.TypeOf<InvalidDataException>());
            Assert.That(() => PortraitLayerCatalog.Validate(recipe with { Layers = recipe.Layers.Select((layer, index) => index == 0 ? layer with { Slot = "hair" } : layer).ToArray() }), Throws.TypeOf<InvalidDataException>());
        });
    }

    [Test]
    public void SourcePriorityPrefersCustomThenGeneratedThenProcedural()
    {
        PortraitReference procedural = new(PortraitSourceKind.Procedural, "p", PortraitStyleId.ProceduralDefault, "hash", 1, "renderer", 128);
        PortraitReference generated = procedural with { SourceKind = PortraitSourceKind.Generated, ContentId = "g" };
        PortraitReference custom = procedural with { SourceKind = PortraitSourceKind.Custom, ContentId = "c" };
        Assert.That(PortraitSourcePriority.Select(new[] { procedural, generated, custom }), Is.EqualTo(custom));
    }

    private static CharacterVisualState State()
    {
        var profile = new CharacterVisualProfile
        {
            Height = Height.Tall,
            Build = Build.Average,
            FacialStructure = FacialStructure.Angular,
            Complexion = Complexion.Olive,
            HairColor = HairColor.Brown,
            HairStyle = HairStyle.Cropped,
            EyeColor = EyeColor.Hazel,
            NotableFeatures = new[] { NotableFeature.Scar },
            Portrait = PortraitRecipeGenerator.Generate(Height.Tall, Build.Average, FacialStructure.Angular, Complexion.Olive, HairColor.Brown, HairStyle.Cropped, EyeColor.Hazel, new[] { NotableFeature.Scar }),
        };
        return CharacterVisualStateProjector.Project("character:1", Sex.Male, 42, profile, LegalStatus.RomanCitizen, SocialClass.Senatorial, DutySlot.Craftsman);
    }
}
