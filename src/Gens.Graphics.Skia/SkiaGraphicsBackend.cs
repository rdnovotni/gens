using System.Numerics;
using System.Runtime.InteropServices;
using Gens.Platform;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace Gens.Graphics.Skia;

public sealed class SkiaGraphicsBackend : IGraphicsBackend
{
    private bool disposed;
    public string Name => $"SkiaSharp {typeof(SKCanvas).Assembly.GetName().Version}";

    public IRenderSurface CreateWindowSurface(IWindow window, RendererMode mode = RendererMode.Default, bool vsync = true)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(window);
        RendererMode resolved = mode == RendererMode.Default ? RendererMode.Gpu : mode;
        return resolved switch
        {
            RendererMode.Gpu when window is IGraphicsWindow graphicsWindow => new GpuSurface(graphicsWindow, vsync),
            RendererMode.Software when window is IPixelBufferWindow pixelWindow => new SoftwareSurface(window.PixelSize, pixelWindow),
            RendererMode.Gpu => throw new NotSupportedException("The platform window does not provide an OpenGL context."),
            _ => throw new NotSupportedException("The platform window does not provide CPU pixel presentation."),
        };
    }

    public IRenderSurface CreateOffscreenSurface(PixelSize size) { ObjectDisposedException.ThrowIf(disposed, this); return new SoftwareSurface(size, null); }
    public IGraphicsPath CreatePath() { ObjectDisposedException.ThrowIf(disposed, this); return new SkiaPath(); }

    public IGraphicsImage DecodeImage(Stream encodedImage)
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(encodedImage);
        SKBitmap bitmap = SKBitmap.Decode(encodedImage) ?? throw new InvalidDataException("Skia could not decode the image. PNG and JPEG are supported.");
        return new SkiaImage(bitmap);
    }

    public IFontFace LoadFont(Stream fontData)
    {
        ObjectDisposedException.ThrowIf(disposed, this); ArgumentNullException.ThrowIfNull(fontData);
        using var memory = new MemoryStream(); fontData.CopyTo(memory);
        byte[] bytes = memory.ToArray();
        SKTypeface typeface = SKTypeface.FromStream(new MemoryStream(bytes)) ?? throw new InvalidDataException("Skia could not decode the font.");
        return new SkiaFontFace(typeface);
    }

    public GlyphRun ShapeText(IFontFace font, string text, float fontSize)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (font is not SkiaFontFace face) throw new ArgumentException("The font was not created by this backend.", nameof(font));
        ArgumentNullException.ThrowIfNull(text);
        if (!(fontSize > 0)) throw new ArgumentOutOfRangeException(nameof(fontSize));
        using var skiaFont = new SKFont(face.Native, fontSize);
        using var shaper = new SKShaper(face.Native);
        SKShaper.Result result = shaper.Shape(text, skiaFont);
        var glyphs = new Glyph[result.Codepoints.Length];
        for (int i = 0; i < glyphs.Length; i++)
        {
            float advance = i + 1 < glyphs.Length ? result.Points[i + 1].X - result.Points[i].X : result.Width - result.Points[i].X;
            glyphs[i] = new(result.Codepoints[i], new(result.Points[i].X, result.Points[i].Y), advance, result.Clusters[i]);
        }
        return new GlyphRun(font, fontSize, glyphs);
    }

    public byte[] EncodePng(ImageData image)
    {
        if (image.Format != PixelFormat.Bgra8888Premultiplied) throw new NotSupportedException($"Unsupported pixel format {image.Format}.");
        byte[] pixels = image.Pixels.ToArray();
        unsafe
        {
            fixed (byte* pointer = pixels)
            {
                using var bitmap = new SKBitmap();
                if (!bitmap.InstallPixels(new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul), (IntPtr)pointer, image.RowBytes))
                    throw new InvalidOperationException("Unable to wrap screenshot pixels.");
                using SKImage snapshot = SKImage.FromBitmap(bitmap);
                using SKData encoded = snapshot.Encode(SKEncodedImageFormat.Png, 100);
                return encoded.ToArray();
            }
        }
    }

    public void Dispose() => disposed = true;
}

internal abstract class SkiaSurface : IRenderSurface
{
    private bool disposed, frameActive;
    protected SKSurface Surface { get; set; } = null!;
    protected SkiaCanvas Canvas { get; set; } = null!;
    public abstract PixelSize PixelSize { get; }
    public abstract RendererMode Mode { get; }
    public IRenderFrame BeginFrame()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (frameActive) throw new InvalidOperationException("A render frame is already active.");
        PrepareFrame();
        frameActive = true; return new Frame(this);
    }
    public abstract void Resize(PixelSize pixelSize);
    protected abstract void PrepareFrame();
    protected abstract void PresentCore();
    protected virtual void DisposeCore() { Canvas.Dispose(); Surface.Dispose(); }
    public ImageData Capture()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        using SKImage image = Surface.Snapshot();
        var info = new SKImageInfo(PixelSize.Width, PixelSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        int rowBytes = info.RowBytes; byte[] pixels = new byte[checked(rowBytes * info.Height)];
        unsafe { fixed (byte* pointer = pixels) if (!image.ReadPixels(info, (IntPtr)pointer, rowBytes, 0, 0)) throw new InvalidOperationException("Skia screenshot readback failed."); }
        return new(info.Width, info.Height, rowBytes, PixelFormat.Bgra8888Premultiplied, pixels);
    }
    public void Dispose() { if (disposed) return; if (frameActive) throw new InvalidOperationException("Dispose the active render frame before its surface."); DisposeCore(); disposed = true; }
    private sealed class Frame(SkiaSurface owner) : IRenderFrame
    {
        private bool completed;
        public ICanvas2D Canvas => owner.Canvas;
        public void Present() { if (completed) throw new InvalidOperationException("The frame has already completed."); owner.PresentCore(); completed = true; owner.frameActive = false; }
        public void Dispose() { if (!completed) { completed = true; owner.frameActive = false; } }
    }
}

internal sealed class SoftwareSurface : SkiaSurface
{
    private SKBitmap bitmap = null!;
    private readonly IPixelBufferWindow? window;
    private PixelSize size;
    internal SoftwareSurface(PixelSize size, IPixelBufferWindow? window) { this.window = window; Create(size); }
    public override PixelSize PixelSize => size;
    public override RendererMode Mode => RendererMode.Software;
    public override void Resize(PixelSize pixelSize) { if (pixelSize == size) return; Canvas.Dispose(); Surface.Dispose(); bitmap.Dispose(); Create(pixelSize); }
    protected override void PrepareFrame()
    {
        LogicalSize logical = window?.LogicalSize ?? new(size.Width, size.Height);
        Canvas.Reset(size.Width / (float)Math.Max(1, logical.Width), size.Height / (float)Math.Max(1, logical.Height));
    }
    protected override unsafe void PresentCore()
    {
        Surface.Canvas.Flush();
        if (window is not null) window.PresentPixels(new ReadOnlySpan<byte>(bitmap.GetPixels().ToPointer(), checked(bitmap.RowBytes * bitmap.Height)), size, bitmap.RowBytes);
    }
    protected override void DisposeCore() { base.DisposeCore(); bitmap.Dispose(); }
    private void Create(PixelSize requested)
    {
        size = new(Math.Max(1, requested.Width), Math.Max(1, requested.Height));
        bitmap = new SKBitmap(new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        Surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes) ?? throw new InvalidOperationException("Unable to create software Skia surface.");
        Canvas = new SkiaCanvas(Surface.Canvas);
    }
}

internal sealed class GpuSurface : SkiaSurface
{
    private readonly IGraphicsWindow window;
    private readonly IOpenGlContext nativeContext;
    private GRGlInterface gl = null!;
    private GRContext context = null!;
    private GRBackendRenderTarget target = null!;
    private readonly GlBindFramebuffer bind;
    private readonly GlGetInteger getInteger;
    private PixelSize size;

    internal GpuSurface(IGraphicsWindow window, bool vsync)
    {
        this.window = window;
        nativeContext = window.CreateOpenGlContext(new()); nativeContext.SetVSync(vsync);
        bind = Marshal.GetDelegateForFunctionPointer<GlBindFramebuffer>(nativeContext.GetProcAddress("glBindFramebuffer"));
        getInteger = Marshal.GetDelegateForFunctionPointer<GlGetInteger>(nativeContext.GetProcAddress("glGetIntegerv"));
        gl = GRGlInterface.Create(nativeContext.GetProcAddress) ?? throw new InvalidOperationException("Skia could not create an OpenGL interface.");
        context = GRContext.CreateGl(gl) ?? throw new InvalidOperationException("Skia could not create an OpenGL rendering context.");
        CreateTarget(window.PixelSize);
    }
    public override PixelSize PixelSize => size;
    public override RendererMode Mode => RendererMode.Gpu;
    public override void Resize(PixelSize pixelSize) { if (pixelSize == size) return; Canvas.Dispose(); Surface.Dispose(); target.Dispose(); CreateTarget(pixelSize); }
    protected override void PrepareFrame()
    {
        // Readback may leave a Skia-owned FBO bound. Restore the window target
        // before every frame, not only after resize.
        bind(0x8d40, nativeContext.DefaultFramebuffer);
        context.ResetContext();
        LogicalSize logical = window.LogicalSize;
        Canvas.Reset(size.Width / (float)Math.Max(1, logical.Width), size.Height / (float)Math.Max(1, logical.Height));
    }
    protected override void PresentCore() { Surface.Canvas.Flush(); context.Flush(); context.Submit(); nativeContext.SwapBuffers(); }
    protected override void DisposeCore() { base.DisposeCore(); target.Dispose(); context.Dispose(); gl.Dispose(); nativeContext.Dispose(); }
    private void CreateTarget(PixelSize requested)
    {
        size = new(Math.Max(1, requested.Width), Math.Max(1, requested.Height));
        bind(0x8d40, nativeContext.DefaultFramebuffer); context.ResetContext();
        getInteger(0x0d57, out int stencil); getInteger(0x80a9, out int samples);
        target = new GRBackendRenderTarget(size.Width, size.Height, samples, stencil, new GRGlFramebufferInfo(nativeContext.DefaultFramebuffer, 0x8058));
        Surface = SKSurface.Create(context, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888) ?? throw new InvalidOperationException("Skia could not create a GPU surface.");
        Canvas = new SkiaCanvas(Surface.Canvas);
    }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlBindFramebuffer(uint target, uint framebuffer);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlGetInteger(uint name, out int value);
}

internal sealed class SkiaCanvas(SKCanvas canvas) : ICanvas2D, IDisposable
{
    private readonly Dictionary<(Color, bool, float, LineCap, LineJoin), SKPaint> paints = [];
    private readonly Dictionary<GlyphRun, SKTextBlob> textBlobs = [];
    internal void Reset(float scaleX, float scaleY)
    {
        canvas.RestoreToCount(1);
        canvas.Save();
        canvas.ResetMatrix();
        canvas.Scale(scaleX, scaleY);
    }
    public ICanvasState Save() { canvas.Save(); return new CanvasState(canvas); }
    public void Translate(float x, float y) => canvas.Translate(x, y);
    public void Rotate(float degrees) => canvas.RotateDegrees(degrees);
    public void Scale(float x, float y) => canvas.Scale(x, y);
    public void Concat(Matrix3x2 m) { var matrix = new SKMatrix { ScaleX = m.M11, SkewX = m.M21, TransX = m.M31, SkewY = m.M12, ScaleY = m.M22, TransY = m.M32, Persp0 = 0, Persp1 = 0, Persp2 = 1 }; canvas.Concat(matrix); }
    public void ClipRect(Rect rect) => canvas.ClipRect(ToSk(rect), SKClipOperation.Intersect, true);
    public void ClipPath(IGraphicsPath path) => canvas.ClipPath(AsPath(path).Native, SKClipOperation.Intersect, true);
    public void Clear(Color color) => canvas.Clear(ToSk(color));
    public void DrawRect(Rect rect, FillStyle fill) => canvas.DrawRect(ToSk(rect), Paint(fill.Color));
    public void DrawRoundRect(Rect rect, float radiusX, float radiusY, FillStyle fill) => canvas.DrawRoundRect(ToSk(rect), radiusX, radiusY, Paint(fill.Color));
    public void DrawLine(Point2 start, Point2 end, StrokeStyle stroke) => canvas.DrawLine(start.X, start.Y, end.X, end.Y, Paint(stroke));
    public void DrawPath(IGraphicsPath path, FillStyle? fill, StrokeStyle? stroke = null) { if (fill is FillStyle f) canvas.DrawPath(AsPath(path).Native, Paint(f.Color)); if (stroke is StrokeStyle s) canvas.DrawPath(AsPath(path).Native, Paint(s)); }
    public void DrawImage(IGraphicsImage image, Rect destination, float opacity = 1)
    {
        if (image is not SkiaImage skia) throw new ArgumentException("Image belongs to a different graphics backend.", nameof(image));
        canvas.DrawBitmap(skia.Native, ToSk(destination), Paint(new Color(255, 255, 255, Alpha(opacity))));
    }
    public void DrawGlyphRun(GlyphRun run, Point2 origin, Color color, float opacity = 1)
    {
        if (run.Font is not SkiaFontFace face) throw new ArgumentException("Font belongs to a different graphics backend.", nameof(run));
        if (!textBlobs.TryGetValue(run, out SKTextBlob? blob))
        {
            using var font = new SKFont(face.Native, run.FontSize);
            using var builder = new SKTextBlobBuilder();
            SKPositionedRunBuffer buffer = builder.AllocatePositionedRun(font, run.Glyphs.Count, null);
            buffer.SetGlyphs(run.Glyphs.Select(static x => checked((ushort)x.Id)).ToArray());
            buffer.SetPositions(run.Glyphs.Select(x => new SKPoint(x.Position.X, x.Position.Y)).ToArray());
            blob = builder.Build() ?? throw new InvalidOperationException("Skia could not build the glyph run.");
            textBlobs.Add(run, blob);
        }
        byte alpha = (byte)((color.A * Alpha(opacity)) / 255);
        canvas.DrawText(blob, origin.X, origin.Y, Paint(new Color(color.R, color.G, color.B, alpha)));
    }
    private SKPaint Paint(Color color) => Get((color, false, 0, LineCap.Butt, LineJoin.Miter));
    private SKPaint Paint(StrokeStyle stroke) => Get((stroke.Color, true, stroke.Width, stroke.Cap, stroke.Join));
    private SKPaint Get((Color color, bool stroke, float width, LineCap cap, LineJoin join) key)
    {
        if (paints.TryGetValue(key, out SKPaint? paint)) return paint;
        paint = new SKPaint
        {
            Color = ToSk(key.color),
            IsAntialias = true,
            Style = key.stroke ? SKPaintStyle.Stroke : SKPaintStyle.Fill,
            StrokeWidth = key.width,
            StrokeCap = key.cap switch { LineCap.Round => SKStrokeCap.Round, LineCap.Square => SKStrokeCap.Square, _ => SKStrokeCap.Butt },
            StrokeJoin = key.join switch { LineJoin.Round => SKStrokeJoin.Round, LineJoin.Bevel => SKStrokeJoin.Bevel, _ => SKStrokeJoin.Miter },
        };
        paints.Add(key, paint); return paint;
    }
    private static SkiaPath AsPath(IGraphicsPath path) => path as SkiaPath ?? throw new ArgumentException("Path belongs to a different graphics backend.", nameof(path));
    private static SKRect ToSk(Rect r) => new(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
    private static SKColor ToSk(Color c) => new(c.R, c.G, c.B, c.A);
    private static byte Alpha(float opacity) => (byte)Math.Clamp(MathF.Round(opacity * 255), 0, 255);
    public void Dispose()
    {
        foreach (SKTextBlob blob in textBlobs.Values) blob.Dispose();
        foreach (SKPaint paint in paints.Values) paint.Dispose();
        textBlobs.Clear(); paints.Clear();
    }
    private sealed class CanvasState(SKCanvas canvas) : ICanvasState { private bool disposed; public void Dispose() { if (!disposed) { canvas.Restore(); disposed = true; } } }
}

internal sealed class SkiaPath : IGraphicsPath
{
    internal SKPath Native { get; } = new();
    public void MoveTo(Point2 p) => Native.MoveTo(p.X, p.Y); public void LineTo(Point2 p) => Native.LineTo(p.X, p.Y);
    public void QuadTo(Point2 c, Point2 e) => Native.QuadTo(c.X, c.Y, e.X, e.Y);
    public void CubicTo(Point2 c1, Point2 c2, Point2 e) => Native.CubicTo(c1.X, c1.Y, c2.X, c2.Y, e.X, e.Y);
    public void Close() => Native.Close(); public void Dispose() => Native.Dispose();
}
internal sealed class SkiaImage(SKBitmap bitmap) : IGraphicsImage { internal SKBitmap Native => bitmap; public int Width => bitmap.Width; public int Height => bitmap.Height; public void Dispose() => bitmap.Dispose(); }
internal sealed class SkiaFontFace(SKTypeface typeface) : IFontFace
{
    internal SKTypeface Native => typeface; public string FamilyName => typeface.FamilyName;
    public FontMetrics GetMetrics(float size) { using var font = new SKFont(typeface, size); font.GetFontMetrics(out SKFontMetrics m); return new(-m.Ascent, m.Descent, m.Leading); }
    public void Dispose() => typeface.Dispose();
}
