using Gens.Graphics;

namespace Gens.UI;

public enum TypographyRole { Body, Caption, SmallCaption, Heading, Title, Inscription, Ledger, ChronicleHeading, Button, Tooltip }

public sealed record TypographyStyle(IFontFace Font, float Size, float LineHeight = 0);

public sealed record ControlStyle(Color Background, Color Foreground, Color Border, Color Hover, Color Pressed, Color Disabled, Color Focus, float BorderWidth = 1, float CornerRadius = 4, Thickness Padding = default);

public sealed class UiTheme(IGraphicsBackend graphics)
{
    private readonly Dictionary<TypographyRole, TypographyStyle> typography = [];
    private readonly Dictionary<string, Color> colors = new(StringComparer.Ordinal);
    public IGraphicsBackend Graphics { get; } = graphics ?? throw new ArgumentNullException(nameof(graphics));
    public ControlStyle Button { get; set; } = new(new(92, 60, 43), new(248, 232, 194), new(53, 34, 26), new(116, 74, 49), new(76, 42, 34), new(95, 88, 78), new(222, 171, 77), 1, 5, new(12, 7));
    public ControlStyle Panel { get; set; } = new(new(226, 205, 164), new(46, 35, 28), new(95, 67, 44), new(226, 205, 164), new(226, 205, 164), new(150, 140, 125), new(222, 171, 77), 1, 6, new(12));
    public ControlStyle TextField { get; set; } = new(new(20, 18, 16), new(245, 229, 195), new(95, 67, 44), new(30, 27, 24), new(30, 27, 24), new(60, 55, 48), new(222, 171, 77), 1, 4, new(10, 6));
    public void SetTypography(TypographyRole role, TypographyStyle style) => typography[role] = style;
    public TypographyStyle Resolve(TypographyRole role) => typography.TryGetValue(role, out TypographyStyle? style) ? style : throw new InvalidOperationException($"Typography role {role} is not configured.");
    public void SetColor(string token, Color value) => colors[token] = value;
    public Color Color(string token) => colors.TryGetValue(token, out Color value) ? value : throw new KeyNotFoundException($"Theme color token '{token}' is not configured.");
}

public static class GensTheme
{
    public static UiTheme Create(IGraphicsBackend graphics, IFontFace bodyFont, IFontFace? displayFont = null, bool highContrast = false)
    {
        displayFont ??= bodyFont;
        var theme = new UiTheme(graphics);
        if (highContrast)
        {
            theme.Button = new(new(0, 0, 0), new(255, 255, 255), new(255, 255, 255), new(35, 35, 35), new(70, 70, 70), new(90, 90, 90), new(255, 221, 0), 2, 3, new(12, 7));
            theme.Panel = new(new(255, 255, 255), new(0, 0, 0), new(0, 0, 0), new(255, 255, 255), new(255, 255, 255), new(110, 110, 110), new(0, 70, 255), 2, 3, new(12));
            theme.TextField = new(new(0, 0, 0), new(255, 255, 255), new(255, 255, 255), new(35, 35, 35), new(35, 35, 35), new(90, 90, 90), new(255, 221, 0), 2, 3, new(10, 6));
        }
        theme.SetColor("Ink", new(46, 35, 28)); theme.SetColor("Parchment", new(226, 205, 164)); theme.SetColor("ParchmentLight", new(245, 229, 195));
        theme.SetColor("Wax", new(133, 48, 39)); theme.SetColor("WaxHover", new(160, 59, 46)); theme.SetColor("Gold", new(190, 142, 54)); theme.SetColor("MutedInk", new(105, 91, 74));
        theme.SetTypography(TypographyRole.Body, new(bodyFont, 17, 23));
        theme.SetTypography(TypographyRole.Caption, new(bodyFont, 14, 19));
        theme.SetTypography(TypographyRole.SmallCaption, new(bodyFont, 12, 16));
        theme.SetTypography(TypographyRole.Heading, new(displayFont, 24, 31));
        theme.SetTypography(TypographyRole.Title, new(displayFont, 34, 42));
        theme.SetTypography(TypographyRole.Inscription, new(displayFont, 18, 24));
        theme.SetTypography(TypographyRole.Ledger, new(bodyFont, 16, 22));
        theme.SetTypography(TypographyRole.ChronicleHeading, new(displayFont, 28, 36));
        theme.SetTypography(TypographyRole.Button, new(displayFont, 16, 22));
        theme.SetTypography(TypographyRole.Tooltip, new(bodyFont, 13, 18));
        return theme;
    }
}
