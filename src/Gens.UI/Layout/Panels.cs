using Gens.Graphics;

namespace Gens.UI;

public class Panel : UiNode
{
    private Thickness padding;
    public Thickness Padding { get => padding; set { if (padding == value) return; padding = value; InvalidateMeasure(); } }
    protected override Size2 MeasureOverride(Size2 availableSize)
    {
        Size2 inner = availableSize.Deflate(Padding); float width = 0, height = 0;
        foreach (UiNode child in Children) { child.Measure(inner); width = Math.Max(width, child.DesiredSize.Width); height = Math.Max(height, child.DesiredSize.Height); }
        return new Size2(width, height).Inflate(Padding);
    }
    protected override void ArrangeOverride(Rect finalRect) { ContentBounds = finalRect.Deflate(Padding); foreach (UiNode child in Children) child.Arrange(ContentBounds); }
}

public sealed class Overlay : Panel { }

public sealed class Spacer : UiNode
{
    public Spacer(float width = 0, float height = 0) { Width = width; Height = height; IsHitTestVisible = false; }
}

public class StackPanel : Panel
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public float Spacing { get; set; }
    protected override Size2 MeasureOverride(Size2 availableSize)
    {
        Size2 inner = availableSize.Deflate(Padding); float primary = 0, cross = 0; int visible = 0;
        foreach (UiNode child in Children.Where(static c => c.Visibility != UiVisibility.Collapsed))
        {
            child.Measure(Orientation == Orientation.Vertical ? new(inner.Width, float.PositiveInfinity) : new(float.PositiveInfinity, inner.Height));
            primary += Orientation == Orientation.Vertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            cross = Math.Max(cross, Orientation == Orientation.Vertical ? child.DesiredSize.Width : child.DesiredSize.Height); visible++;
        }
        primary += Math.Max(0, visible - 1) * Spacing;
        return (Orientation == Orientation.Vertical ? new Size2(cross, primary) : new Size2(primary, cross)).Inflate(Padding);
    }
    protected override void ArrangeOverride(Rect finalRect)
    {
        ContentBounds = finalRect.Deflate(Padding); float cursor = Orientation == Orientation.Vertical ? ContentBounds.Y : ContentBounds.X;
        foreach (UiNode child in Children.Where(static c => c.Visibility != UiVisibility.Collapsed))
        {
            if (Orientation == Orientation.Vertical) { child.Arrange(new(ContentBounds.X, cursor, ContentBounds.Width, child.DesiredSize.Height)); cursor += child.DesiredSize.Height + Spacing; }
            else { child.Arrange(new(cursor, ContentBounds.Y, child.DesiredSize.Width, ContentBounds.Height)); cursor += child.DesiredSize.Width + Spacing; }
        }
    }
}

public sealed class Row : StackPanel { public Row() { Orientation = Orientation.Horizontal; } }
public sealed class Column : StackPanel { public Column() { Orientation = Orientation.Vertical; } }
