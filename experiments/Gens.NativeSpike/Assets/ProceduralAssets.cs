using SkiaSharp;

namespace Gens.NativeSpike.Assets;

internal sealed class ProceduralAssets : IDisposable
{
    internal ProceduralAssets()
    {
        Portrait = Make(512, 512, true);
        Illustration = Make(1024, 768, false);
        LargeBackground = Make(2048, 2048, false);
        // Exercise both codecs without committing large generated assets.
        using SKData png = Portrait.Encode(SKEncodedImageFormat.Png, 90);
        using SKData jpeg = Illustration.Encode(SKEncodedImageFormat.Jpeg, 85);
        PngDecoded = SKImage.FromEncodedData(png) ?? throw new InvalidDataException("PNG decode failed.");
        JpegDecoded = SKImage.FromEncodedData(jpeg) ?? throw new InvalidDataException("JPEG decode failed.");
    }
    internal SKImage Portrait { get; }
    internal SKImage Illustration { get; }
    internal SKImage LargeBackground { get; }
    internal SKImage PngDecoded { get; }
    internal SKImage JpegDecoded { get; }

    private static SKImage Make(int width, int height, bool portrait)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        SKCanvas c = surface.Canvas;
        c.Clear(new SKColor(44, 34, 27));
        using var paint = new SKPaint { IsAntialias = true };
        using var gradient = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(width, height),
            [new SKColor(157, 118, 62), new SKColor(44, 66, 64), new SKColor(65, 36, 40)], null, SKShaderTileMode.Clamp);
        paint.Shader = gradient;
        c.DrawRect(0, 0, width, height, paint);
        paint.Shader = null; paint.Color = new SKColor(239, 218, 174, 210);
        int count = portrait ? 24 : 80;
        for (int i = 0; i < count; i++) c.DrawCircle((i * 97) % width, (i * 53) % height, 8 + i % 37, paint);
        if (portrait) { paint.Color = new SKColor(45, 29, 24); c.DrawCircle(width / 2, height * .38f, width * .18f, paint); c.DrawOval(new SKRect(width * .25f, height * .55f, width * .75f, height), paint); }
        return surface.Snapshot();
    }
    public void Dispose() { Portrait.Dispose(); Illustration.Dispose(); LargeBackground.Dispose(); PngDecoded.Dispose(); JpegDecoded.Dispose(); }
}
