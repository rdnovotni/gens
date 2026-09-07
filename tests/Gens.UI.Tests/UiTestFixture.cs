using Gens.Graphics;
using Gens.Graphics.Skia;
using NUnit.Framework;

namespace Gens.UI.Tests;

public abstract class UiTestFixture
{
    protected SkiaGraphicsBackend Graphics { get; private set; } = null!;
    protected IFontFace Font { get; private set; } = null!;
    [SetUp] public void SetUpGraphics() { Graphics = new(); using FileStream stream = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "NotoSans-Regular.ttf")); Font = Graphics.LoadFont(stream); }
    [TearDown] public void TearDownGraphics() { Font.Dispose(); Graphics.Dispose(); }
    protected UiRoot Root() => new(GensTheme.Create(Graphics, Font));
}

internal sealed class FixedNode(float width, float height) : UiNode
{
    protected override Size2 MeasureOverride(Size2 availableSize) => new(Math.Min(width, availableSize.Width), Math.Min(height, availableSize.Height));
}

internal sealed class ThrowingPaintNode : UiNode
{
    protected override Size2 MeasureOverride(Size2 availableSize) => new(10, 10);
    protected override void PaintOverride(ICanvas2D canvas) => throw new InvalidOperationException("paint failed");
}
