using Gens.Graphics;

namespace Gens.UI;

public class Border : UiNode
{
    private Thickness padding;
    private Color background = Color.Transparent, borderBrush = Color.Transparent;
    private float borderThickness;
    private CornerRadius cornerRadius;
    public Color Background { get => background; set { if (background == value) return; background = value; InvalidatePaint(); } }
    public Color BorderBrush { get => borderBrush; set { if (borderBrush == value) return; borderBrush = value; InvalidatePaint(); } }
    public float BorderThickness { get => borderThickness; set { if (borderThickness == value) return; borderThickness = value; InvalidateMeasure(); } }
    public CornerRadius CornerRadius { get => cornerRadius; set { if (cornerRadius == value) return; cornerRadius = value; InvalidatePaint(); } }
    public Thickness Padding { get => padding; set { if (padding == value) return; padding = value; InvalidateMeasure(); } }
    public UiNode? Child { get => Children.Count == 0 ? null : Children[0]; set { if (Child == value) return; ClearChildren(); if (value is not null) AddChild(value); } }
    protected override Size2 MeasureOverride(Size2 availableSize) { Thickness inset = new(Padding.Left + BorderThickness, Padding.Top + BorderThickness, Padding.Right + BorderThickness, Padding.Bottom + BorderThickness); Child?.Measure(availableSize.Deflate(inset)); return (Child?.DesiredSize ?? default).Inflate(inset); }
    protected override void ArrangeOverride(Rect finalRect) { Thickness inset = new(Padding.Left + BorderThickness, Padding.Top + BorderThickness, Padding.Right + BorderThickness, Padding.Bottom + BorderThickness); ContentBounds = finalRect.Deflate(inset); Child?.Arrange(ContentBounds); }
    protected override void PaintOverride(ICanvas2D canvas)
    {
        float radius = CornerRadius.TopLeft;
        if (Background.A > 0) canvas.DrawRoundRect(Bounds, radius, radius, new(Background));
        if (BorderThickness > 0 && BorderBrush.A > 0)
        {
            canvas.DrawLine(new(Bounds.X, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y), new(BorderBrush, BorderThickness));
            canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(BorderBrush, BorderThickness));
            canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y + Bounds.Height), new(BorderBrush, BorderThickness));
            canvas.DrawLine(new(Bounds.X, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y), new(BorderBrush, BorderThickness));
        }
    }
}
