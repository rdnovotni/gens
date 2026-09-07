using Gens.Graphics;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class LayoutTests : UiTestFixture
{
    [Test]
    public void StackPanelMeasuresAndArrangesChildrenWithSpacingMarginAndCollapsedItems()
    {
        UiRoot root = Root(); var stack = new StackPanel { Padding = new(5), Spacing = 3, Width = 120, HorizontalAlignment = HorizontalAlignment.Start };
        var first = new FixedNode(40, 10) { Margin = new(2), HorizontalAlignment = HorizontalAlignment.Start }; var collapsed = new FixedNode(999, 999) { Visibility = UiVisibility.Collapsed }; var second = new FixedNode(60, 20);
        stack.AddChild(first); stack.AddChild(collapsed); stack.AddChild(second); root.AddChild(stack); root.Layout(new(300, 200));
        Assert.Multiple(() => { Assert.That(stack.DesiredSize, Is.EqualTo(new Size2(120, 47))); Assert.That(first.Bounds, Is.EqualTo(new Rect(7, 7, 40, 10))); Assert.That(second.Bounds, Is.EqualTo(new Rect(5, 22, 110, 20))); Assert.That(collapsed.Bounds, Is.EqualTo(default(Rect))); });
    }

    [Test]
    public void GridSupportsFixedAutoStarAndSpans()
    {
        UiRoot root = Root(); var grid = new Grid { Width = 300, Height = 120, HorizontalAlignment = HorizontalAlignment.Start, VerticalAlignment = VerticalAlignment.Start };
        grid.Columns.Add(new(UiLength.Fixed(50))); grid.Columns.Add(new(UiLength.Auto)); grid.Columns.Add(new(UiLength.Star())); grid.Rows.Add(new(UiLength.Auto)); grid.Rows.Add(new(UiLength.Star()));
        var auto = new FixedNode(70, 20); var star = new FixedNode(10, 10); var span = new FixedNode(150, 12);
        grid.AddChild(auto); grid.SetPlacement(auto, 0, 1); grid.AddChild(star); grid.SetPlacement(star, 1, 2); grid.AddChild(span); grid.SetPlacement(span, 0, 0, 1, 2); root.AddChild(grid); root.Layout(new(500, 300));
        Assert.Multiple(() => { Assert.That(auto.Bounds, Is.EqualTo(new Rect(50, 0, 100, 20))); Assert.That(star.Bounds, Is.EqualTo(new Rect(150, 20, 150, 100))); Assert.That(span.Bounds, Is.EqualTo(new Rect(0, 0, 150, 20))); });
    }

    [Test]
    public void AlignmentMinMaxPaddingAndNestedLayoutUseExplicitRectangles()
    {
        UiRoot root = Root(); var border = new Border { Padding = new(10), Width = 100, Height = 80, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Child = new FixedNode(200, 2) { MinHeight = 20, MaxWidth = 50, HorizontalAlignment = HorizontalAlignment.End, VerticalAlignment = VerticalAlignment.End } }; root.AddChild(border); root.Layout(new(200, 200));
        Assert.Multiple(() => { Assert.That(border.Bounds, Is.EqualTo(new Rect(50, 60, 100, 80))); Assert.That(border.ContentBounds, Is.EqualTo(new Rect(60, 70, 80, 60))); Assert.That(border.Child!.Bounds, Is.EqualTo(new Rect(90, 110, 50, 20))); });
    }

    [Test]
    public void MeasureAndArrangeAreCachedUntilInvalidated()
    {
        UiRoot root = Root(); var child = new FixedNode(10, 10); root.AddChild(child); root.Layout(new(100, 100)); Assert.That(root.Diagnostics.MeasureCount, Is.GreaterThan(0)); root.Layout(new(100, 100)); Assert.Multiple(() => { Assert.That(root.Diagnostics.MeasureCount, Is.Zero); Assert.That(root.Diagnostics.ArrangeCount, Is.Zero); }); child.Width = 20; root.Layout(new(100, 100)); Assert.That(root.Diagnostics.MeasureCount, Is.GreaterThan(0));
    }

    [Test]
    public void ParentOwnershipAndLayoutMutationAreGuarded()
    {
        var first = new Panel(); var second = new Panel(); var child = new FixedNode(1, 1); first.AddChild(child); Assert.Throws<InvalidOperationException>(() => second.AddChild(child)); Assert.That(first.RemoveChild(child), Is.True); second.AddChild(child); Assert.That(child.Parent, Is.SameAs(second));
    }
}
