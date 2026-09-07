using Gens.Assets;
using Gens.Graphics;
using Gens.Presentation.Visuals;

namespace Gens.Portraits;

public sealed record ResolvedPortrait(PortraitReference Reference, IGraphicsImage Image, AppearanceDescription Appearance, bool CacheHit, GeneratedPortraitStaleness? Staleness = null);

public sealed class PortraitService : IDisposable
{
    private readonly IGraphicsBackend graphics;
    private readonly ProceduralPortraitRenderer renderer;
    private readonly ContentAddressedCache cache;
    private readonly IGeneratedPortraitSource? generated;
    private readonly Dictionary<string, IGraphicsImage> memory = new(StringComparer.Ordinal);
    private readonly List<PortraitSnapshot> snapshots = [];
    private bool disposed;

    public PortraitService(IGraphicsBackend graphics, string cacheRoot, IGeneratedPortraitSource? generated = null)
    {
        this.graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        renderer = new(graphics);
        cache = new(cacheRoot);
        this.generated = generated;
    }

    public IReadOnlyList<PortraitSnapshot> Snapshots => snapshots;

    public ResolvedPortrait Resolve(CharacterVisualState visual, int pixelSize)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        GeneratedPortraitAsset? selected = generated?.GetCurrent(visual);
        if (selected is not null && selected.Staleness != GeneratedPortraitStaleness.StaleMajor)
        {
            string assetHash = selected.AssetHash;
            var generatedReference = new PortraitReference(PortraitSourceKind.Generated, assetHash, selected.StyleId, selected.VisualStateHash,
                PortraitRecipeBuilder.CurrentRecipeVersion, selected.ProviderVersion, selected.PixelSize);
            if (memory.TryGetValue(assetHash, out IGraphicsImage? generatedImage)) return new(generatedReference, generatedImage, AppearanceDescriptionBuilder.Build(visual), true, selected.Staleness);
            try
            {
                using FileStream stream = File.OpenRead(selected.ObjectPath); generatedImage = graphics.DecodeImage(stream); memory.Add(assetHash, generatedImage);
                return new(generatedReference, generatedImage, AppearanceDescriptionBuilder.Build(visual), false, selected.Staleness);
            }
            catch (Exception error) when (error is IOException or InvalidDataException or ArgumentException) { }
        }
        PortraitRecipe recipe = PortraitRecipeBuilder.Build(visual);
        AppearanceDescription description = AppearanceDescriptionBuilder.Build(visual);
        string request = recipe.CanonicalIdentity + "\n" + ProceduralPortraitRenderer.Version + "\n" + pixelSize;
        string key = ContentAddressedCache.Hash(request);
        PortraitReference reference = new(PortraitSourceKind.Procedural, key, recipe.StyleId, recipe.VisualStateHash, recipe.RecipeVersion, ProceduralPortraitRenderer.Version, pixelSize);
        if (memory.TryGetValue(key, out IGraphicsImage? existing)) return new(reference, existing, description, true);
        byte[]? png = cache.Read("portraits", key, ".png");
        if (png is not null)
        {
            using var stream = new MemoryStream(png, false); IGraphicsImage cachedImage = graphics.DecodeImage(stream); memory.Add(key, cachedImage);
            return new(reference, cachedImage, description, true);
        }
        try
        {
            RenderedPortrait rendered = renderer.Render(recipe, pixelSize); cache.Write("portraits", key, ".png", rendered.PngBytes); memory.Add(key, rendered.Image);
            return new(reference, rendered.Image, description, false);
        }
        catch (Exception error) when (error is InvalidDataException or ArgumentException or InvalidOperationException)
        {
            PortraitRecipe fallback = new(1, visual.VisualSeed, PortraitStyleId.ProceduralDefault, visual.VisualStateHash, new[] { new PortraitLayer("fallback", "portrait/fallback/silhouette", "fallback") });
            RenderedPortrait rendered = renderer.Render(fallback, pixelSize); string fallbackKey = ContentAddressedCache.Hash(fallback.CanonicalIdentity + "\n" + ProceduralPortraitRenderer.Version + "\n" + pixelSize);
            memory[fallbackKey] = rendered.Image;
            return new(new(PortraitSourceKind.Fallback, fallbackKey, fallback.StyleId, visual.VisualStateHash, fallback.RecipeVersion, ProceduralPortraitRenderer.Version, pixelSize), rendered.Image, description, false);
        }
    }

    public PortraitSnapshot CaptureSnapshot(string subjectId, string campaignDate, ResolvedPortrait portrait)
    {
        var snapshot = new PortraitSnapshot(subjectId, campaignDate, portrait.Reference.VisualStateHash, portrait.Reference, portrait.Reference.RecipeVersion);
        snapshots.Add(snapshot); return snapshot;
    }

    public IReadOnlyList<PortraitSnapshot> FindSnapshots(string subjectId) => snapshots.Where(snapshot => string.Equals(snapshot.SubjectId, subjectId, StringComparison.Ordinal)).ToArray();
    public PortraitSnapshot? FindSnapshot(string subjectId, string campaignDate) => snapshots.LastOrDefault(snapshot => string.Equals(snapshot.SubjectId, subjectId, StringComparison.Ordinal) && string.Equals(snapshot.CampaignDate, campaignDate, StringComparison.Ordinal));

    public void Dispose()
    {
        if (disposed) return;
        foreach (IGraphicsImage image in memory.Values.Distinct()) image.Dispose();
        memory.Clear(); disposed = true;
    }
}
