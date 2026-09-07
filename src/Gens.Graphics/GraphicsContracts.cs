using System.Numerics;
using Gens.Platform;

namespace Gens.Graphics;

/// <summary>Unpremultiplied RGBA components in the inclusive 0..255 range.</summary>
public readonly record struct Color(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color White = new(255, 255, 255);
}
public readonly record struct Point2(float X, float Y);
public readonly record struct Size2(float Width, float Height);
public readonly record struct Rect(float X, float Y, float Width, float Height);
public readonly record struct FillStyle(Color Color);
public readonly record struct StrokeStyle(Color Color, float Width = 1, LineCap Cap = LineCap.Butt, LineJoin Join = LineJoin.Miter);
public enum LineCap { Butt, Round, Square }
public enum LineJoin { Miter, Round, Bevel }
public enum RendererMode { Default, Gpu, Software }

public enum PixelFormat { Bgra8888Premultiplied }
public readonly record struct ImageData(int Width, int Height, int RowBytes, PixelFormat Format, ReadOnlyMemory<byte> Pixels);
public readonly record struct FontMetrics(float Ascent, float Descent, float Leading);
public readonly record struct Glyph(uint Id, Point2 Position, float Advance, uint Cluster);
public sealed record GlyphRun(IFontFace Font, float FontSize, IReadOnlyList<Glyph> Glyphs);

public interface IGraphicsResource : IDisposable { }
public interface IGraphicsImage : IGraphicsResource { int Width { get; } int Height { get; } }
public interface IFontFace : IGraphicsResource { string FamilyName { get; } FontMetrics GetMetrics(float size); }
public interface IGraphicsPath : IGraphicsResource
{
    void MoveTo(Point2 point); void LineTo(Point2 point); void QuadTo(Point2 control, Point2 destination);
    void CubicTo(Point2 control1, Point2 control2, Point2 destination); void Close();
}

public interface ICanvasState : IDisposable { }
public interface ICanvas2D
{
    ICanvasState Save();
    void Translate(float x, float y); void Rotate(float degrees); void Scale(float x, float y); void Concat(Matrix3x2 transform);
    void ClipRect(Rect rect); void ClipPath(IGraphicsPath path);
    void Clear(Color color); void DrawRect(Rect rect, FillStyle fill); void DrawRoundRect(Rect rect, float radiusX, float radiusY, FillStyle fill);
    void DrawLine(Point2 start, Point2 destination, StrokeStyle stroke); void DrawPath(IGraphicsPath path, FillStyle? fill, StrokeStyle? stroke = null);
    void DrawImage(IGraphicsImage image, Rect destination, float opacity = 1);
    void DrawImage(IGraphicsImage image, Rect source, Rect destination, float opacity = 1);
    void DrawImage(IGraphicsImage image, Rect destination, Color tint, float opacity = 1);
    void DrawImage(IGraphicsImage image, Rect source, Rect destination, Color tint, float opacity = 1);
    void DrawGlyphRun(GlyphRun run, Point2 origin, Color color, float opacity = 1);
}

public interface IRenderFrame : IDisposable
{
    ICanvas2D Canvas { get; }
    void Present();
}

public interface IRenderSurface : IGraphicsResource
{
    PixelSize PixelSize { get; }
    RendererMode Mode { get; }
    IRenderFrame BeginFrame();
    void Resize(PixelSize pixelSize);
    ImageData Capture();
}

public interface IGraphicsBackend : IDisposable
{
    string Name { get; }
    IRenderSurface CreateWindowSurface(IWindow window, RendererMode mode = RendererMode.Default, bool vsync = true);
    IRenderSurface CreateOffscreenSurface(PixelSize size);
    IGraphicsPath CreatePath();
    IGraphicsImage DecodeImage(Stream encodedImage);
    IFontFace LoadFont(Stream fontData);
    GlyphRun ShapeText(IFontFace font, string text, float fontSize);
    byte[] EncodePng(ImageData image);
}
