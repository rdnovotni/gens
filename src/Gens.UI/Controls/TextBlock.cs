using Gens.Graphics;

namespace Gens.UI;

public sealed class TextBlock : UiNode
{
    private string text = string.Empty;
    private TypographyRole role = TypographyRole.Body;
    private TextWrapping wrapping;
    private TextTrimming trimming;
    private Color? foreground;
    private TextLayout? layout;
    private TextCacheKey cacheKey;
    public string Text { get => text; set { value ??= string.Empty; if (text == value) return; text = value; Semantics.Label = value; InvalidateText(); } }
    public TypographyRole TypographyRole { get => role; set { if (role == value) return; role = value; InvalidateText(); } }
    public TextWrapping Wrapping { get => wrapping; set { if (wrapping == value) return; wrapping = value; InvalidateText(); } }
    public TextTrimming Trimming { get => trimming; set { if (trimming == value) return; trimming = value; InvalidateText(); } }
    public int? MaxLines { get; set; }
    public Color? Foreground { get => foreground; set { if (foreground == value) return; foreground = value; InvalidatePaint(); } }
    public TextBlock() { IsHitTestVisible = false; Semantics.Role = AccessibilityRole.Text; }
    protected override Size2 MeasureOverride(Size2 availableSize) { EnsureLayout(availableSize.Width); return layout!.Size; }
    protected override void PaintOverride(ICanvas2D canvas)
    {
        EnsureLayout(ContentBounds.Width); TypographyStyle style = Root!.Theme.Resolve(TypographyRole); Color color = Foreground ?? Root.Theme.Color("Ink"); float y = Bounds.Y;
        foreach (TextLine line in layout!.Lines) { y += line.Baseline; canvas.DrawGlyphRun(line.Run, new(Bounds.X, y), color, Opacity); y += line.Height - line.Baseline; }
    }
    private void EnsureLayout(float width)
    {
        UiTheme theme = Root?.Theme ?? throw new InvalidOperationException("TextBlock must be attached to a UiRoot before layout."); TypographyStyle style = theme.Resolve(TypographyRole);
        var key = new TextCacheKey(Text, TypographyRole, Wrapping, Trimming, MaxLines, width, style.Font, style.Size);
        if (layout is not null && key == cacheKey) return; cacheKey = key;
        List<string> lines = Wrapping == TextWrapping.Wrap && !float.IsInfinity(width) ? Wrap(theme.Graphics, style, Text, width) : Text.Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n').ToList();
        if (MaxLines is int max && lines.Count > max) { lines = lines.Take(max).ToList(); if (Trimming == TextTrimming.Ellipsis) lines[^1] = TrimToWidth(theme.Graphics, style, lines[^1] + "…", width); }
        var shaped = new List<TextLine>(); float maxWidth = 0, totalHeight = 0; FontMetrics metrics = style.Font.GetMetrics(style.Size); float naturalHeight = metrics.Ascent + metrics.Descent + metrics.Leading; float height = style.LineHeight > 0 ? style.LineHeight : naturalHeight; float baseline = metrics.Ascent + Math.Max(0, height - naturalHeight) / 2;
        foreach (string line in lines) { GlyphRun run = theme.Graphics.ShapeText(style.Font, line, style.Size); float lineWidth = WidthOf(run); shaped.Add(new(run, lineWidth, height, baseline)); maxWidth = Math.Max(maxWidth, lineWidth); totalHeight += height; }
        layout = new(new(maxWidth, totalHeight), shaped);
    }
    private static List<string> Wrap(IGraphicsBackend graphics, TypographyStyle style, string text, float width)
    {
        var result = new List<string>(); foreach (string paragraph in text.Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n')) { string current = string.Empty; foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries)) { string candidate = current.Length == 0 ? word : current + " " + word; if (current.Length > 0 && WidthOf(graphics.ShapeText(style.Font, candidate, style.Size)) > width) { result.Add(current); current = word; } else current = candidate; } result.Add(current); }
        return result;
    }
    private static string TrimToWidth(IGraphicsBackend graphics, TypographyStyle style, string text, float width) { while (text.Length > 1 && WidthOf(graphics.ShapeText(style.Font, text, style.Size)) > width) text = text[..^2] + "…"; return text; }
    private static float WidthOf(GlyphRun run) => run.Glyphs.Count == 0 ? 0 : run.Glyphs.Max(static g => g.Position.X + g.Advance);
    private void InvalidateText() { layout = null; InvalidateMeasure(); }
    private readonly record struct TextCacheKey(string Text, TypographyRole Role, TextWrapping Wrapping, TextTrimming Trimming, int? MaxLines, float Width, IFontFace Font, float Size);
    private sealed record TextLayout(Size2 Size, IReadOnlyList<TextLine> Lines);
    private sealed record TextLine(GlyphRun Run, float Width, float Height, float Baseline);
}
