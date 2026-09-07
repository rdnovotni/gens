using System.Security.Cryptography;
using Gens.Graphics.Skia;
using Gens.Platform;
using NUnit.Framework;

namespace Gens.Graphics.Tests;

public sealed class ReferenceRenderingTests
{
    [Test]
    public void SoftwareRenderingIsDeterministicAndPngEncodes()
    {
        using var backend = new SkiaGraphicsBackend();
        using IRenderSurface first = backend.CreateOffscreenSurface(new PixelSize(128, 96));
        using IRenderSurface second = backend.CreateOffscreenSurface(new PixelSize(128, 96));
        Draw(first, backend); Draw(second, backend);
        ImageData a = first.Capture(), b = second.Capture();
        Assert.Multiple(() =>
        {
            Assert.That(SHA256.HashData(a.Pixels.Span), Is.EqualTo(SHA256.HashData(b.Pixels.Span)));
            Assert.That(backend.EncodePng(a), Has.Length.GreaterThan(100));
            Assert.That(first.Mode, Is.EqualTo(RendererMode.Software));
        });
    }

    [Test]
    public void WindowSurfaceMapsLogicalCanvasUnitsToPixels()
    {
        using var backend = new SkiaGraphicsBackend();
        using var window = new FakePixelWindow();
        using IRenderSurface surface = backend.CreateWindowSurface(window, RendererMode.Software);
        using (IRenderFrame frame = surface.BeginFrame())
        {
            frame.Canvas.Clear(Color.Black);
            frame.Canvas.DrawRect(new(0, 0, 100, 100), new(new Color(255, 0, 0)));
            frame.Present();
        }
        ImageData capture = surface.Capture();
        int last = ((capture.Height - 1) * capture.RowBytes) + ((capture.Width - 1) * 4);
        Assert.That(capture.Pixels.Span.Slice(last, 4).ToArray(), Is.EqualTo(new byte[] { 0, 0, 255, 255 }));
    }

    private static void Draw(IRenderSurface surface, SkiaGraphicsBackend backend)
    {
        using IGraphicsPath path = backend.CreatePath(); path.MoveTo(new(10, 80)); path.LineTo(new(64, 10)); path.LineTo(new(118, 80)); path.Close();
        using IRenderFrame frame = surface.BeginFrame();
        frame.Canvas.Clear(new(20, 30, 40)); frame.Canvas.DrawRect(new(4, 4, 120, 88), new(new Color(210, 180, 120)));
        using (frame.Canvas.Save()) { frame.Canvas.ClipRect(new(8, 8, 112, 80)); frame.Canvas.Translate(2, 1); frame.Canvas.DrawPath(path, new(new Color(120, 30, 25)), new(Color.White, 2)); }
        frame.Present();
    }
}

internal sealed class FakePixelWindow : IPixelBufferWindow
{
    public WindowId Id => new(1);
    public LogicalSize LogicalSize => new(100, 100);
    public PixelSize PixelSize => new(200, 200);
    public DisplayScale DisplayScale => new(2, 2);
    public WindowState State => WindowState.Normal;
    public void PresentPixels(ReadOnlySpan<byte> pixels, PixelSize size, int rowBytes) { }
    public void StartTextInput() { }
    public void StopTextInput() { }
    public void SetFullscreen(bool fullscreen) { }
    public void Dispose() { }
}
