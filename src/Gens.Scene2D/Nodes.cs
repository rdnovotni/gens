using Gens.Graphics;

namespace Gens.Scene2D;

public enum SpriteScalingMode { NativeSize, Stretch, Contain, Cover }

public sealed class Sprite2D : SceneNode2D
{
    private IGraphicsImage? image;
    private Rect? sourceRect;
    private Size2 size;
    private Point2 anchor = new(0.5f, 0.5f);
    private Color tint = Color.White;
    private bool flipX, flipY;
    private SpriteScalingMode scalingMode = SpriteScalingMode.Stretch;
    public IGraphicsImage? Image { get => image; set { if (image == value) return; image = value; Invalidate(); } }
    public Rect? SourceRect { get => sourceRect; set { if (sourceRect == value) return; sourceRect = value; Invalidate(); } }
    public Size2 Size { get => size; set { if (size == value) return; size = value; Invalidate(); } }
    public Point2 Anchor { get => anchor; set { if (anchor == value) return; anchor = value; Invalidate(); } }
    public Color Tint { get => tint; set { if (tint == value) return; tint = value; Invalidate(); } }
    public bool FlipX { get => flipX; set { if (flipX == value) return; flipX = value; Invalidate(); } }
    public bool FlipY { get => flipY; set { if (flipY == value) return; flipY = value; Invalidate(); } }
    public SpriteScalingMode ScalingMode { get => scalingMode; set { if (scalingMode == value) return; scalingMode = value; Invalidate(); } }
    protected override Rect? LocalBounds => Destination;

    protected override void RenderSelf(ICanvas2D canvas, float opacity)
    {
        if (Image is null) return;
        Rect destination = Destination;
        using ICanvasState state = canvas.Save();
        if (FlipX || FlipY)
        {
            canvas.Translate(destination.X + (FlipX ? destination.Width : 0), destination.Y + (FlipY ? destination.Height : 0));
            canvas.Scale(FlipX ? -1 : 1, FlipY ? -1 : 1);
            destination = new(0, 0, destination.Width, destination.Height);
        }
        if (SourceRect is Rect source) canvas.DrawImage(Image, source, destination, Tint, opacity);
        else canvas.DrawImage(Image, destination, Tint, opacity);
    }

    private Rect Destination
    {
        get
        {
            float nativeWidth = SourceRect?.Width ?? Image?.Width ?? 0, nativeHeight = SourceRect?.Height ?? Image?.Height ?? 0;
            float width = Size.Width > 0 ? Size.Width : nativeWidth, height = Size.Height > 0 ? Size.Height : nativeHeight;
            if (ScalingMode == SpriteScalingMode.NativeSize) { width = nativeWidth; height = nativeHeight; }
            else if (ScalingMode is SpriteScalingMode.Contain or SpriteScalingMode.Cover && nativeWidth > 0 && nativeHeight > 0 && width > 0 && height > 0)
            {
                float factor = ScalingMode == SpriteScalingMode.Contain ? Math.Min(width / nativeWidth, height / nativeHeight) : Math.Max(width / nativeWidth, height / nativeHeight);
                width = nativeWidth * factor; height = nativeHeight * factor;
            }
            return new(-Anchor.X * width, -Anchor.Y * height, width, height);
        }
    }
}

public sealed class VectorNode2D : SceneNode2D
{
    private IGraphicsPath? path;
    private FillStyle? fill;
    private StrokeStyle? stroke;
    private Rect bounds;
    public IGraphicsPath? Path { get => path; set { if (path == value) return; path = value; Invalidate(); } }
    public FillStyle? Fill { get => fill; set { if (fill == value) return; fill = value; Invalidate(); } }
    public StrokeStyle? Stroke { get => stroke; set { if (stroke == value) return; stroke = value; Invalidate(); } }
    public Rect Bounds { get => bounds; set { if (bounds == value) return; bounds = value; Invalidate(); } }
    protected override Rect? LocalBounds => Bounds;
    protected override void RenderSelf(ICanvas2D canvas, float opacity) { if (Path is not null) canvas.DrawPath(Path, Fill is { } fill ? new(new Color(fill.Color.R, fill.Color.G, fill.Color.B, (byte)(fill.Color.A * opacity))) : null, Stroke); }
}

public sealed class RectangleNode2D : SceneNode2D
{
    private Rect rectangle;
    private Color color = Color.White;
    private float cornerRadius;
    public Rect Rectangle { get => rectangle; set { if (rectangle == value) return; rectangle = value; Invalidate(); } }
    public Color Color { get => color; set { if (color == value) return; color = value; Invalidate(); } }
    public float CornerRadius { get => cornerRadius; set { if (cornerRadius == value) return; cornerRadius = value; Invalidate(); } }
    protected override Rect? LocalBounds => Rectangle;
    protected override void RenderSelf(ICanvas2D canvas, float opacity)
    {
        Color color = new(Color.R, Color.G, Color.B, (byte)(Color.A * opacity));
        if (CornerRadius > 0) canvas.DrawRoundRect(Rectangle, CornerRadius, CornerRadius, new(color)); else canvas.DrawRect(Rectangle, new(color));
    }
}

public sealed class TextNode2D : SceneNode2D
{
    private GlyphRun? glyphRun;
    private Point2 origin;
    private Color color = Color.White;
    public GlyphRun? GlyphRun { get => glyphRun; set { if (glyphRun == value) return; glyphRun = value; Invalidate(); } }
    public Point2 Origin { get => origin; set { if (origin == value) return; origin = value; Invalidate(); } }
    public Color Color { get => color; set { if (color == value) return; color = value; Invalidate(); } }
    protected override void RenderSelf(ICanvas2D canvas, float opacity) { if (GlyphRun is not null) canvas.DrawGlyphRun(GlyphRun, Origin, Color, opacity); }
}
