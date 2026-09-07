using Gens.Graphics;

namespace Gens.UI;

public sealed class ScrollView : UiNode
{
    private UiNode? content;
    private float horizontalOffset, verticalOffset;
    public ScrollView() { ClipToBounds = true; Semantics.Role = AccessibilityRole.ScrollView; }
    public UiNode? Content { get => content; set { if (content == value) return; ClearChildren(); content = value; if (value is not null) AddChild(value); } }
    public float HorizontalOffset { get => horizontalOffset; set { float next = Math.Clamp(value, 0, MaxHorizontalOffset); if (horizontalOffset == next) return; horizontalOffset = next; InvalidateArrange(); } }
    public float VerticalOffset { get => verticalOffset; set { float next = Math.Clamp(value, 0, MaxVerticalOffset); if (verticalOffset == next) return; verticalOffset = next; InvalidateArrange(); } }
    public float MaxHorizontalOffset => Math.Max(0, Extent.Width - Viewport.Width);
    public float MaxVerticalOffset => Math.Max(0, Extent.Height - Viewport.Height);
    public Size2 Extent { get; private set; }
    public Size2 Viewport { get; private set; }
    public float WheelStep { get; set; } = 42;
    protected override Size2 MeasureOverride(Size2 availableSize) { Content?.Measure(new(float.PositiveInfinity, float.PositiveInfinity)); Extent = Content?.DesiredSize ?? default; return new(Math.Min(availableSize.Width, Extent.Width), Math.Min(availableSize.Height, Extent.Height)); }
    protected override void ArrangeOverride(Rect finalRect) { Viewport = new(finalRect.Width, finalRect.Height); HorizontalOffset = horizontalOffset; VerticalOffset = verticalOffset; ContentBounds = finalRect; Content?.Arrange(new(finalRect.X - HorizontalOffset, finalRect.Y - VerticalOffset, Math.Max(Extent.Width, finalRect.Width), Math.Max(Extent.Height, finalRect.Height))); }
    protected override void OnPointerEvent(UiPointerEvent evt) { if (evt.Type == UiPointerEventType.Wheel && IsEnabled) { VerticalOffset -= evt.Delta.Y * WheelStep; evt.Handled = true; } }
    protected override void OnKeyEvent(UiKeyEvent evt) { float before = VerticalOffset; if (evt.Key.ScanCode == 81) VerticalOffset += WheelStep; else if (evt.Key.ScanCode == 82) VerticalOffset -= WheelStep; else if (evt.Key.ScanCode == 78) VerticalOffset += Viewport.Height; else if (evt.Key.ScanCode == 75) VerticalOffset -= Viewport.Height; evt.Handled = before != VerticalOffset; }
}
