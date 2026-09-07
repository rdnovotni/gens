using Gens.Assets;
using Gens.Graphics;
using Gens.Platform;
using Gens.Presentation.Visuals;
using Gens.Scene2D;

namespace Gens.Portraits;

public sealed record RenderedPortrait(byte[] PngBytes, IGraphicsImage Image, string ContentHash);

public sealed class ProceduralPortraitRenderer(IGraphicsBackend graphics)
{
    public const string Version = "gens.procedural.renderer.v1";
    public RenderedPortrait Render(PortraitRecipe recipe, int pixelSize)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        if (pixelSize is < 32 or > 2048) throw new ArgumentOutOfRangeException(nameof(pixelSize));
        PortraitLayerCatalog.Validate(recipe);
        using IRenderSurface surface = graphics.CreateOffscreenSurface(new PixelSize(pixelSize, pixelSize));
        var scene = new Scene2D.Scene2D(); scene.Camera.Position = new(pixelSize / 2f, pixelSize / 2f);
        int order = 0;
        foreach (PortraitLayer layer in recipe.Layers) scene.Root.AddChild(new PortraitLayerNode(layer, pixelSize) { ZOrder = order++ });
        using (IRenderFrame frame = surface.BeginFrame()) { frame.Canvas.Clear(Color.Transparent); scene.Render(frame.Canvas, new(0, 0, pixelSize, pixelSize)); frame.Present(); }
        byte[] png = graphics.EncodePng(surface.Capture());
        using var stream = new MemoryStream(png, false);
        IGraphicsImage image = graphics.DecodeImage(stream);
        return new(png, image, ContentAddressedCache.HashBytes(png));
    }
}

public static class PortraitLayerCatalog
{
    public const string Provenance = "Original project-created parametric vector primitives; no external artwork.";
    public const string License = "Gens project source license.";
    private static readonly string[] Definitions =
    [
        "background|portrait/background-01", "background|portrait/background-02", "background|portrait/background-03",
        "body|portrait/body/slight", "body|portrait/body/average", "body|portrait/body/muscular", "body|portrait/body/heavyset",
        "clothing|portrait/clothing/citizen-tunic", "clothing|portrait/clothing/freed-tunic", "clothing|portrait/clothing/plain-tunic", "clothing|portrait/clothing/senatorial-toga", "clothing|portrait/clothing/equestrian-toga",
        "head|portrait/head/angular", "head|portrait/head/round", "head|portrait/head/square", "head|portrait/head/oval", "head|portrait/head/gaunt",
        "eyes|portrait/eyes/default",
        "hair|portrait/hair/cropped", "hair|portrait/hair/shoulder-length", "hair|portrait/hair/flowing", "hair|portrait/hair/bound-up",
        "detail|portrait/detail/scar", "detail|portrait/detail/broken-nose", "detail|portrait/detail/freckled", "detail|portrait/detail/birthmark", "detail|portrait/detail/gray-at-temples",
        "office|portrait/office/duty-marker", "foreground|portrait/foreground/memorial", "fallback|portrait/fallback/silhouette",
    ];
    public static AssetManifest Manifest { get; } = new(Definitions.Select(static definition =>
    {
        string[] parts = definition.Split('|');
        return new AssetManifestEntry(new(parts[1]), parts[0], Provenance, License, "procedural://" + parts[1], 1, new[] { "portrait", parts[0] });
    }));
    public static void Validate(PortraitRecipe recipe)
    {
        var used = new HashSet<AssetId>();
        foreach (PortraitLayer layer in recipe.Layers)
        {
            var id = new AssetId(layer.AssetId);
            AssetManifestEntry entry;
            try { entry = Manifest.Resolve(id); }
            catch (KeyNotFoundException error) { throw new InvalidDataException($"Portrait recipe references unknown layer '{layer.AssetId}'.", error); }
            if (!string.Equals(entry.Kind, layer.Slot, StringComparison.Ordinal)) throw new InvalidDataException($"Portrait layer '{layer.AssetId}' is not compatible with slot '{layer.Slot}'.");
            if (!used.Add(id)) throw new InvalidDataException($"Portrait recipe repeats layer '{layer.AssetId}'.");
        }
        if (recipe.Layers.Count == 1 && recipe.Layers[0].Slot == "fallback") return;
        foreach (string required in new[] { "background", "body", "clothing", "head", "eyes", "hair" })
            if (!recipe.Layers.Any(layer => layer.Slot == required)) throw new InvalidDataException($"Portrait recipe is missing required '{required}' layer.");
    }
    public static bool IsKnown(string id) { try { Manifest.Resolve(new(id)); return true; } catch (Exception error) when (error is ArgumentException or KeyNotFoundException) { return false; } }
}

internal sealed class PortraitLayerNode(PortraitLayer layer, int size) : SceneNode2D
{
    protected override Rect? LocalBounds => new Rect(0, 0, size, size);
    protected override void RenderSelf(ICanvas2D canvas, float opacity)
    {
        float s = size / 256f;
        Color color = Palette(layer.ColorToken);
        if (layer.Slot == "background") { canvas.DrawRect(new(0, 0, size, size), new(color)); canvas.DrawRoundRect(new(18 * s, 18 * s, 220 * s, 220 * s), 110 * s, 110 * s, new(new Color(232, 214, 176))); return; }
        if (layer.Slot == "body") { canvas.DrawRoundRect(new(48 * s, 174 * s, 160 * s, 104 * s), 50 * s, 50 * s, new(color)); return; }
        if (layer.Slot == "clothing") { canvas.DrawRoundRect(new(34 * s, 187 * s, 188 * s, 86 * s), 30 * s, 30 * s, new(color)); canvas.DrawLine(new(128 * s, 188 * s), new(128 * s, 254 * s), new(new Color(214, 186, 125), 3 * s)); return; }
        if (layer.Slot == "head") { DrawHead(canvas, color, s); return; }
        if (layer.Slot == "eyes") { canvas.DrawRoundRect(new(88 * s, 112 * s, 18 * s, 10 * s), 5 * s, 5 * s, new(color)); canvas.DrawRoundRect(new(150 * s, 112 * s, 18 * s, 10 * s), 5 * s, 5 * s, new(color)); return; }
        if (layer.Slot == "hair") { canvas.DrawRoundRect(new(74 * s, 57 * s, 108 * s, HairHeight(layer.AssetId) * s), 32 * s, 32 * s, new(color)); return; }
        if (layer.Slot == "detail") { DrawDetail(canvas, layer.AssetId, s); return; }
        if (layer.Slot == "office") { canvas.DrawRoundRect(new(187 * s, 198 * s, 22 * s, 22 * s), 11 * s, 11 * s, new(color)); return; }
        if (layer.Slot == "foreground") { canvas.DrawLine(new(30 * s, 228 * s), new(226 * s, 228 * s), new(color, 8 * s)); return; }
        canvas.DrawRoundRect(new(64 * s, 48 * s, 128 * s, 180 * s), 64 * s, 64 * s, new(new Color(92, 82, 72)));
    }

    private void DrawHead(ICanvas2D canvas, Color color, float s)
    {
        float width = layer.AssetId.EndsWith("/round", StringComparison.Ordinal) ? 126 : layer.AssetId.EndsWith("/gaunt", StringComparison.Ordinal) ? 92 : 108;
        canvas.DrawRoundRect(new((256 - width) / 2 * s, 63 * s, width * s, 139 * s), width / 2 * s, 58 * s, new(color));
        canvas.DrawRoundRect(new(112 * s, 176 * s, 32 * s, 38 * s), 12 * s, 12 * s, new(color));
    }
    private static float HairHeight(string id) => id.EndsWith("/cropped", StringComparison.Ordinal) ? 35 : id.EndsWith("/bound-up", StringComparison.Ordinal) ? 54 : 66;
    private static void DrawDetail(ICanvas2D canvas, string id, float s)
    {
        if (id.EndsWith("/scar", StringComparison.Ordinal)) canvas.DrawLine(new(155 * s, 124 * s), new(143 * s, 158 * s), new(new Color(112, 55, 44), 3 * s));
        else if (id.EndsWith("/broken-nose", StringComparison.Ordinal)) canvas.DrawLine(new(128 * s, 124 * s), new(135 * s, 144 * s), new(new Color(105, 67, 52), 3 * s));
        else if (id.EndsWith("/birthmark", StringComparison.Ordinal)) canvas.DrawRoundRect(new(91 * s, 145 * s, 13 * s, 9 * s), 5 * s, 5 * s, new(new Color(123, 72, 57)));
        else if (id.EndsWith("/freckled", StringComparison.Ordinal)) for (int i = 0; i < 5; i++) canvas.DrawRoundRect(new((102 + i * 12) * s, (142 + i % 2 * 4) * s, 3 * s, 3 * s), 1.5f * s, 1.5f * s, new(new Color(116, 73, 50)));
        else canvas.DrawLine(new(82 * s, 81 * s), new(174 * s, 81 * s), new(new Color(175, 172, 165), 4 * s));
    }
    private static Color Palette(string token) => token switch
    {
        "background" => new(79, 47, 39),
        "cloth-muted" => new(87, 66, 53),
        "cloth-status" => new(139, 48, 43),
        "skin-fair" => new(231, 190, 157),
        "skin-olive" => new(192, 142, 100),
        "skin-bronzed" => new(157, 104, 67),
        "skin-dark" => new(103, 67, 49),
        "eye-blue" => new(65, 104, 130),
        "eye-green" => new(74, 102, 73),
        "eye-gray" => new(101, 108, 108),
        "eye-hazel" => new(112, 91, 52),
        "eye-brown" => new(71, 48, 37),
        "hair-black" => new(30, 25, 22),
        "hair-brown" => new(78, 48, 32),
        "hair-auburn" => new(116, 52, 32),
        "hair-blond" => new(195, 159, 92),
        "hair-gray" => new(126, 123, 116),
        "hair-white" => new(210, 205, 191),
        "status-gold" => new(196, 151, 56),
        "memorial" => new(62, 55, 51),
        _ => new(92, 55, 43),
    };
}
