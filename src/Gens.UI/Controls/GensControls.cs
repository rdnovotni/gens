using Gens.Graphics;

namespace Gens.UI;

public sealed class WaxTablet : Border
{
    public WaxTablet() { Name = "WaxTablet"; Padding = new(18); BorderThickness = 2; CornerRadius = new(8); }
    protected override void PaintOverride(ICanvas2D canvas)
    {
        if (Root is not null) { Background = Root.Theme.Color("Parchment"); BorderBrush = Root.Theme.Color("Ink"); }
        base.PaintOverride(canvas);
        canvas.DrawLine(new(Bounds.X + 7, Bounds.Y + 10), new(Bounds.X + 7, Bounds.Y + Bounds.Height - 10), new(Root?.Theme.Color("Gold") ?? Color.White, 2));
    }
}

public sealed class Diptych : Grid
{
    public Diptych()
    {
        Name = "Diptych"; Columns.Add(new(UiLength.Star())); Columns.Add(new(UiLength.Star()));
    }
    public void SetLeft(UiNode node) { AddChild(node); SetPlacement(node, 0, 0); }
    public void SetRight(UiNode node) { AddChild(node); SetPlacement(node, 0, 1); }
}

public sealed class WaxSealButton : Button
{
    public WaxSealButton() { Name = "WaxSealButton"; Width = 74; Height = 74; }
    protected override void PaintOverride(ICanvas2D canvas)
    {
        if (Root is not null) Style = Root.Theme.Button with { Background = Root.Theme.Color("Wax"), Hover = Root.Theme.Color("WaxHover"), CornerRadius = 37, BorderWidth = 3, Padding = new(10) };
        base.PaintOverride(canvas);
    }
}

public sealed class CharacterMedallion : Border
{
    private readonly Image image;
    public CharacterMedallion(IGraphicsImage? portrait = null)
    {
        Name = "CharacterMedallion"; Width = Height = 112; CornerRadius = new(56); BorderThickness = 4; Padding = new(5);
        image = new Image { Source = portrait, Stretch = ImageStretch.Cover, Semantics = { Label = "Character portrait" } }; Child = image;
    }
    public IGraphicsImage? Portrait { get => image.Source; set => image.Source = value; }
    protected override void PaintOverride(ICanvas2D canvas) { if (Root is not null) { Background = Root.Theme.Color("MutedInk"); BorderBrush = Root.Theme.Color("Gold"); } base.PaintOverride(canvas); }
}

public sealed class InkBar : Border
{
    public InkBar() { Name = "InkBar"; Height = 58; Padding = new(16, 8); BorderThickness = 0; Semantics.Role = AccessibilityRole.Group; }
    protected override void PaintOverride(ICanvas2D canvas) { if (Root is not null) Background = Root.Theme.Color("Ink"); base.PaintOverride(canvas); }
}

/// <summary>Compact label/value row for dense ledgers and character statistics.</summary>
public sealed class StatRow : Grid
{
    private readonly TextBlock label = new() { TypographyRole = TypographyRole.Ledger };
    private readonly TextBlock value = new() { TypographyRole = TypographyRole.Ledger, HorizontalAlignment = HorizontalAlignment.End };
    public StatRow()
    {
        Name = "StatRow"; Columns.Add(new(UiLength.Star(2))); Columns.Add(new(UiLength.Star()));
        AddChild(label); SetPlacement(label, 0, 0); AddChild(value); SetPlacement(value, 0, 1);
    }
    public string Label { get => label.Text; set => label.Text = value; }
    public string Value { get => this.value.Text; set => this.value.Text = value; }
}
