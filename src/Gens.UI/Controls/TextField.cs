using Gens.Graphics;

namespace Gens.UI;

/// <summary>A focusable single-line text-entry control. There is no other generic text-entry
/// surface in <c>Gens.UI</c> today — this is the control every screen that needs player-typed text
/// (e.g. naming a save) must use, routed through <see cref="UiRoot.HandleTextInput"/> rather than
/// any screen hand-appending platform text events itself.</summary>
public sealed class TextField : UiNode
{
    private string text = string.Empty;
    public TextField() { IsFocusable = true; Semantics.Role = AccessibilityRole.TextInput; }
    public string Text { get => text; set { value = Clamp(value ?? string.Empty); if (text == value) return; text = value; Semantics.Value = text; InvalidatePaint(); } }
    public int MaxLength { get; set; } = 64;
    public string? Placeholder { get; set; }
    public ControlStyle? Style { get; set; }
    public Action? Submitted { get; set; }
    public Action<string>? Changed { get; set; }

    protected override Size2 MeasureOverride(Size2 availableSize)
    {
        ControlStyle style = ResolvedStyle;
        TypographyStyle typography = Root!.Theme.Resolve(TypographyRole.Body);
        FontMetrics metrics = typography.Font.GetMetrics(typography.Size);
        float naturalHeight = metrics.Ascent + metrics.Descent + metrics.Leading;
        float height = typography.LineHeight > 0 ? typography.LineHeight : naturalHeight;
        float width = Math.Min(availableSize.Width, Math.Max(120, availableSize.Width));
        return new Size2(width, height).Inflate(style.Padding);
    }

    protected override void ArrangeOverride(Rect finalRect) => ContentBounds = finalRect.Deflate(ResolvedStyle.Padding);

    protected override void PaintOverride(ICanvas2D canvas)
    {
        ControlStyle style = ResolvedStyle;
        Color background = !IsEnabled ? style.Disabled : style.Background;
        canvas.DrawRoundRect(Bounds, style.CornerRadius, style.CornerRadius, new(background));
        DrawBorder(canvas, IsFocused ? style.Focus : style.Border, IsFocused ? Math.Max(2, style.BorderWidth) : style.BorderWidth);

        TypographyStyle typography = Root!.Theme.Resolve(TypographyRole.Body);
        bool showingPlaceholder = text.Length == 0 && !string.IsNullOrEmpty(Placeholder);
        string shown = showingPlaceholder ? Placeholder! : text;
        if (shown.Length > 0)
        {
            GlyphRun run = Root.Theme.Graphics.ShapeText(typography.Font, shown, typography.Size);
            FontMetrics metrics = typography.Font.GetMetrics(typography.Size);
            float y = ContentBounds.Y + (ContentBounds.Height - (metrics.Ascent + metrics.Descent)) / 2 + metrics.Ascent;
            Color color = showingPlaceholder ? style.Border : style.Foreground;
            canvas.DrawGlyphRun(run, new(ContentBounds.X, y), color, showingPlaceholder ? .8f : 1f);
        }
        if (IsFocused)
        {
            FontMetrics metrics = typography.Font.GetMetrics(typography.Size);
            float caretWidth = text.Length == 0 ? 0 : WidthOf(Root.Theme.Graphics.ShapeText(typography.Font, text, typography.Size));
            float caretX = ContentBounds.X + caretWidth;
            float caretTop = ContentBounds.Y + (ContentBounds.Height - (metrics.Ascent + metrics.Descent)) / 2;
            canvas.DrawLine(new(caretX, caretTop), new(caretX, caretTop + metrics.Ascent + metrics.Descent), new(style.Foreground, 1));
        }
    }

    protected override void OnPointerEvent(UiPointerEvent evt) { if (IsEnabled && evt.Type == UiPointerEventType.Pressed) evt.CapturePointer(); }

    protected override void OnKeyEvent(UiKeyEvent evt)
    {
        if (!IsEnabled) return;
        if (evt.Key.ScanCode == 42 && text.Length > 0) { Text = text[..^1]; Changed?.Invoke(text); evt.Handled = true; }
        else if (evt.Key.ScanCode == 40) { Submitted?.Invoke(); evt.Handled = true; }
    }

    protected override void OnTextInput(string text)
    {
        if (!IsEnabled || string.IsNullOrEmpty(text)) return;
        string filtered = new(text.Where(static c => !char.IsControl(c)).ToArray());
        if (filtered.Length == 0) return;
        Text = this.text + filtered;
        Changed?.Invoke(this.text);
    }

    private string Clamp(string value) => value.Length > MaxLength ? value[..MaxLength] : value;
    private ControlStyle ResolvedStyle => Style ?? Root?.Theme.TextField ?? throw new InvalidOperationException("TextField must be attached to a UiRoot before layout.");
    private static float WidthOf(GlyphRun run) => run.Glyphs.Count == 0 ? 0 : run.Glyphs.Max(static g => g.Position.X + g.Advance);
    private void DrawBorder(ICanvas2D canvas, Color color, float thickness)
    {
        if (thickness <= 0 || color.A == 0) return;
        canvas.DrawLine(new(Bounds.X, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y), new(color, thickness));
        canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y), new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(color, thickness));
        canvas.DrawLine(new(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y + Bounds.Height), new(color, thickness));
        canvas.DrawLine(new(Bounds.X, Bounds.Y + Bounds.Height), new(Bounds.X, Bounds.Y), new(color, thickness));
    }
}
