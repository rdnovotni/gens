using Gens.Graphics;

namespace Gens.UI;

public sealed class Image : UiNode
{
    private IGraphicsImage? source;
    public Image() { IsHitTestVisible = false; Semantics.Role = AccessibilityRole.Image; }
    public IGraphicsImage? Source { get => source; set { if (source == value) return; source = value; InvalidateMeasure(); } }
    public ImageStretch Stretch { get; set; } = ImageStretch.Contain;
    public HorizontalAlignment ImageHorizontalAlignment { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment ImageVerticalAlignment { get; set; } = VerticalAlignment.Center;
    protected override Size2 MeasureOverride(Size2 availableSize) => Source is null ? default : new(Math.Min(availableSize.Width, Source.Width), Math.Min(availableSize.Height, Source.Height));
    protected override void PaintOverride(ICanvas2D canvas)
    {
        if (Source is null) return; float imageWidth = Source.Width, imageHeight = Source.Height;
        if (Stretch == ImageStretch.Fill) { imageWidth = Bounds.Width; imageHeight = Bounds.Height; }
        else if (Stretch is ImageStretch.Contain or ImageStretch.Cover) { float factor = Stretch == ImageStretch.Contain ? Math.Min(Bounds.Width / imageWidth, Bounds.Height / imageHeight) : Math.Max(Bounds.Width / imageWidth, Bounds.Height / imageHeight); imageWidth *= factor; imageHeight *= factor; }
        float x = ImageHorizontalAlignment switch { HorizontalAlignment.Start => Bounds.X, HorizontalAlignment.End => Bounds.X + Bounds.Width - imageWidth, _ => Bounds.X + (Bounds.Width - imageWidth) / 2 };
        float y = ImageVerticalAlignment switch { VerticalAlignment.Start => Bounds.Y, VerticalAlignment.End => Bounds.Y + Bounds.Height - imageHeight, _ => Bounds.Y + (Bounds.Height - imageHeight) / 2 };
        canvas.DrawImage(Source, new(x, y, imageWidth, imageHeight), Opacity);
    }
}
