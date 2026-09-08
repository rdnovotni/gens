using Gens.Graphics;

namespace Gens.UI;

public class Button : UiNode
{
    private UiNode? content;
    public Button() { IsFocusable = true; Semantics.Role = AccessibilityRole.Button; }
    public UiNode? Content { get => content; set { if (content == value) return; ClearChildren(); content = value; if (value is not null) AddChild(value); } }
    public Action? Clicked { get; set; }
    public ControlStyle? Style { get; set; }
    protected override Size2 MeasureOverride(Size2 availableSize) { ControlStyle style = ResolvedStyle; Content?.Measure(availableSize.Deflate(style.Padding)); return (Content?.DesiredSize ?? default).Inflate(style.Padding); }
    protected override void ArrangeOverride(Rect finalRect) { ContentBounds = finalRect.Deflate(ResolvedStyle.Padding); Content?.Arrange(ContentBounds); }
    protected override void PaintOverride(ICanvas2D canvas)
    {
        ControlStyle style = ResolvedStyle; Color background = !IsEnabled ? style.Disabled : IsPressed ? style.Pressed : IsHovered ? style.Hover : style.Background;
        canvas.DrawRoundRect(Bounds, style.CornerRadius, style.CornerRadius, new(background));
        DrawBorder(canvas, style.Border, style.BorderWidth);
        if (IsFocused) DrawBorder(canvas, style.Focus, Math.Max(2, style.BorderWidth));
    }
    protected override void OnPointerEvent(UiPointerEvent evt)
    {
        if (!IsEnabled) return;
        if (evt.Type == UiPointerEventType.Pressed) evt.CapturePointer();
        else if (evt.Type == UiPointerEventType.Released) evt.ReleasePointerCapture();
        else if (evt.Type == UiPointerEventType.Clicked) Activate();
    }
    protected override void OnKeyEvent(UiKeyEvent evt) { if (IsEnabled && evt.Key.ScanCode is 40 or 44) { Activate(); evt.Handled = true; } }
    protected virtual void Activate() => Clicked?.Invoke();
    private ControlStyle ResolvedStyle => Style ?? Root?.Theme.Button ?? throw new InvalidOperationException("Button must be attached to a UiRoot before layout.");
    private void DrawBorder(ICanvas2D canvas, Color color, float thickness)
    {
        if (thickness <= 0 || color.A == 0) return; canvas.DrawLine(new(Bounds.X, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y), new(color, thickness)); canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(color, thickness)); canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y + Bounds.Height), new(color, thickness)); canvas.DrawLine(new(Bounds.X, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y), new(color, thickness));
    }
}

public sealed class Toggle : Button
{
    public Toggle() { Semantics.Role = AccessibilityRole.Toggle; }
    public bool IsChecked { get; private set; }
    public Action<bool>? Changed { get; set; }
    public void SetChecked(bool value, bool notify = false) { if (IsChecked == value) return; IsChecked = value; Semantics.IsChecked = value; InvalidatePaint(); if (notify) Changed?.Invoke(value); }
    protected override void Activate() { SetChecked(!IsChecked, notify: true); base.Activate(); }
}
