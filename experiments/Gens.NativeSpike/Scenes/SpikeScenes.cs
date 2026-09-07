using Gens.NativeSpike.Assets;
using SkiaSharp;

namespace Gens.NativeSpike.Scenes;

internal static class SpikeScenes
{
    private static readonly SKColor Parchment = new(232, 217, 180);
    private static readonly SKColor Ink = new(48, 37, 28);
    private static readonly SKColor Red = new(116, 37, 35);

    internal static void Draw(SKCanvas c, int width, int height, double time, int scene, ProceduralAssets assets)
    {
        c.ResetMatrix(); c.Clear(Ink);
        float sx = width / 1280f, sy = height / 720f;
        c.Scale(sx, sy);
        if (scene == 0) DrawPrimitives(c, assets, time); else if (scene == 1) DrawTablet(c, assets, time); else DrawStress(c, assets, time);
    }

    internal static void DrawPrimitives(SKCanvas c, ProceduralAssets assets, double time)
    {
        using var fill = new SKPaint { IsAntialias = true, Color = Parchment };
        using var stroke = new SKPaint { IsAntialias = true, Color = Red, Style = SKPaintStyle.Stroke, StrokeWidth = 4 };
        c.DrawRect(25, 25, 1230, 670, fill);
        c.DrawRoundRect(new SKRect(55, 70, 360, 190), 18, 18, stroke);
        fill.Shader = SKShader.CreateLinearGradient(new SKPoint(390, 70), new SKPoint(700, 190), [Red, new SKColor(218, 161, 72)], null, SKShaderTileMode.Clamp);
        c.DrawRoundRect(new SKRect(390, 70, 700, 190), 18, 18, fill); fill.Shader?.Dispose(); fill.Shader = null;
        c.DrawLine(740, 70, 1000, 190, stroke); c.DrawCircle(1110, 130, 72, stroke);
        using var path = new SKPath(); path.MoveTo(70, 260); path.CubicTo(240, 170, 230, 380, 390, 270); c.DrawPath(path, stroke);
        c.Save(); c.ClipRect(new SKRect(430, 230, 780, 430)); c.ClipRoundRect(new SKRoundRect(new SKRect(470, 250, 740, 410), 25)); c.RotateDegrees((float)Math.Sin(time) * 12, 605, 330); c.DrawImage(assets.PngDecoded, new SKRect(420, 180, 790, 490), fill); c.Restore();
        c.Save(); c.Translate(880, 300); c.RotateDegrees(18); c.Scale(1.2f); fill.Color = Red.WithAlpha(150); c.DrawRect(-100, -70, 200, 140, fill); c.Restore();
        DrawText(c, "Primitives · clipping · nested transforms · opacity", 54, 540, 28, Ink);
        DrawText(c, "Latin ligature: office affinity · Ελληνικά · العربية", 54, 590, 25, Ink);
        c.DrawImage(assets.JpegDecoded, new SKRect(850, 470, 1210, 660), fill);
    }

    internal static void DrawTablet(SKCanvas c, ProceduralAssets assets, double time)
    {
        using var p = new SKPaint { IsAntialias = true, Color = Parchment };
        c.DrawRoundRect(new SKRect(45, 30, 1235, 690), 12, 12, p); p.Color = Ink; p.Style = SKPaintStyle.Stroke; p.StrokeWidth = 3;
        c.DrawRoundRect(new SKRect(45, 30, 1235, 690), 12, 12, p); c.DrawLine(45, 112, 1235, 112, p); c.DrawLine(570, 112, 570, 690, p);
        DrawText(c, "FAMILIA CORNELIA", 640, 82, 35, Red, SKTextAlign.Center, true);
        DrawText(c, "CHARACTER", 85, 158, 18, Red, bold: true); DrawText(c, "HOUSEHOLD", 620, 158, 18, Red, bold: true);
        c.DrawImage(assets.Portrait, new SKRect(85, 185, 405, 505));
        DrawText(c, "Lucius Cornelius", 85, 548, 29, Ink, bold: true); DrawText(c, "Age 43 · Consular rank", 85, 580, 20, Ink);
        DrawText(c, "Wealth", 620, 215, 20, Ink); DrawText(c, "2,840", 1130, 215, 23, Ink, SKTextAlign.Right, true);
        DrawText(c, "Prestige", 620, 253, 20, Ink); DrawText(c, "112", 1130, 253, 23, Ink, SKTextAlign.Right, true);
        DrawText(c, "Current matters", 620, 325, 25, Red, bold: true);
        string[] matters = ["• Estate harvest requires judgment", "• Senate vote on the grain levy", "• Julia requests a household audience", "• A long clipped dispatch from Ostia will…"];
        for (int i = 0; i < matters.Length; i++) DrawText(c, matters[i], 640, 372 + i * 39, 19, Ink);
        float seal = .75f + .25f * (float)((Math.Sin(time * 3) + 1) / 2); c.Save(); c.Translate(1080, 585); c.Scale(seal); p.Style = SKPaintStyle.Fill; p.Color = Red.WithAlpha((byte)(150 + seal * 100)); c.DrawCircle(0, 0, 55, p); DrawText(c, "SPQR", 0, 8, 18, Parchment, SKTextAlign.Center, true); c.Restore();
    }

    internal static void DrawStress(SKCanvas c, ProceduralAssets assets, double time)
    {
        using var p = new SKPaint { IsAntialias = true, Color = SKColors.White };
        c.DrawImage(assets.LargeBackground, new SKRect(0, 0, 1280, 720), p);
        for (int i = 0; i < 100; i++) { float x = (i * 83) % 1250; float y = (i * 47) % 680; p.Color = Parchment.WithAlpha(150); c.DrawRoundRect(new SKRect(x, y, x + 45, y + 34), 5, 5, p); }
        DrawText(c, "THE CHRONICLE OF ROME", 640, 80, 40, SKColors.White, SKTextAlign.Center, true);
    }

    internal static void DrawBasicBenchmark(SKCanvas c, int width, int height, double time, ProceduralAssets assets)
    {
        c.Clear(Ink); using var p = new SKPaint { IsAntialias = true };
        for (int i = 0; i < 100; i++) { p.Color = i % 2 == 0 ? Parchment : Red; c.DrawRect((i * 71) % width, (i * 41) % height, 80, 32, p); DrawText(c, $"Label {i}", (i * 71) % width, (i * 41) % height + 22, 13, Ink); }
        for (int i = 0; i < 10; i++) c.DrawImage(assets.Portrait, new SKRect(i * width / 10f, height - 80, i * width / 10f + 64, height - 16), p);
    }

    private static void DrawText(SKCanvas c, string text, float x, float y, float size, SKColor color, SKTextAlign align = SKTextAlign.Left, bool bold = false)
    {
        using SKTypeface face = SKTypeface.FromFamilyName(null, bold ? SKFontStyle.Bold : SKFontStyle.Normal);
        using var font = new SKFont(face, size) { Edging = SKFontEdging.SubpixelAntialias, Subpixel = true };
        using var paint = new SKPaint { IsAntialias = true, Color = color };
        float offset = align switch { SKTextAlign.Center => font.MeasureText(text) / 2, SKTextAlign.Right => font.MeasureText(text), _ => 0 };
        c.DrawText(text, x - offset, y, font, paint);
    }
}
