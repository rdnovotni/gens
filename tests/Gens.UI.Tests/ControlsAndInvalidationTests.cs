using Gens.Graphics;
using Gens.Platform;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class ControlsAndInvalidationTests : UiTestFixture
{
    [Test]
    public void BackgroundAndHoverInvalidatePaintOnlyWhileWidthAndTextInvalidateMeasure()
    {
        UiRoot root = Root(); var border = new Border { Child = new TextBlock { Text = "old" } }; root.AddChild(border); root.Layout(new(200, 80)); using IRenderSurface surface = Graphics.CreateOffscreenSurface(new(200, 80)); using (IRenderFrame frame = surface.BeginFrame()) { root.Render(frame.Canvas); frame.Present(); }
        border.Background = new(1, 2, 3); Assert.Multiple(() => { Assert.That(border.IsMeasureValid, Is.True); Assert.That(border.IsArrangeValid, Is.True); Assert.That(border.IsPaintValid, Is.False); });
        border.Width = 100; Assert.That(border.IsMeasureValid, Is.False); root.Layout(new(200, 80)); ((TextBlock)border.Child!).Text = "new text"; Assert.That(border.IsMeasureValid, Is.False);
    }

    [Test]
    public void ScrollViewClampsWheelResizeAndContentChanges()
    {
        UiRoot root = Root(); var scroll = new ScrollView { Width = 100, Height = 80, IsFocusable = true, HorizontalAlignment = HorizontalAlignment.Start, VerticalAlignment = VerticalAlignment.Start, Content = new FixedNode(100, 300) }; root.AddChild(scroll); root.Layout(new(200, 200)); scroll.VerticalOffset = 999; Assert.That(scroll.VerticalOffset, Is.EqualTo(220)); scroll.Content = new FixedNode(100, 40); root.Layout(new(200, 200)); Assert.That(scroll.VerticalOffset, Is.Zero); scroll.Content = new FixedNode(100, 300); root.Layout(new(200, 200)); root.MovePointer(new(10, 10)); root.HandleEvent(new WheelEvent(0, new(1), new(10, 10), new(0, -1))); Assert.That(scroll.VerticalOffset, Is.EqualTo(42)); root.Focus.RequestFocus(scroll); root.HandleKey(new(new(81), Gens.Platform.KeyModifiers.None, false)); Assert.That(scroll.VerticalOffset, Is.EqualTo(84));
    }

    [Test]
    public void TextWrappingCacheAndStyleInvalidationAreConsistent()
    {
        UiRoot root = Root(); var text = new TextBlock { Text = "alpha beta gamma delta", Wrapping = TextWrapping.Wrap, Width = 70, MaxLines = 2, Trimming = TextTrimming.Ellipsis }; root.AddChild(text); root.Layout(new(100, 200)); Size2 first = text.DesiredSize; root.Layout(new(100, 200)); Assert.That(root.Diagnostics.MeasureCount, Is.Zero); text.TypographyRole = TypographyRole.Heading; Assert.That(text.IsMeasureValid, Is.False); root.Layout(new(100, 200)); Assert.That(text.DesiredSize.Height, Is.GreaterThan(first.Height)); text.Foreground = new(1, 2, 3); Assert.That(text.IsMeasureValid, Is.True);
    }

    [Test]
    public void DeepPlausibleTreeLaysOutWithoutRebuildingUnchangedSubtree()
    {
        UiRoot root = Root(); UiNode current = root; for (int i = 0; i < 250; i++) { var next = new Panel(); current.AddChild(next); current = next; }
        current.AddChild(new FixedNode(1, 1)); root.Layout(new(10, 10)); Assert.That(root.Diagnostics.NodeCount, Is.EqualTo(252)); root.Layout(new(10, 10)); Assert.That(root.Diagnostics.MeasureCount, Is.Zero);
    }
}
