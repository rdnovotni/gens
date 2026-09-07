using System.Security.Cryptography;
using Gens.Graphics;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class ReferenceRenderingTests : UiTestFixture
{
    [TestCase("basic")]
    [TestCase("grid")]
    [TestCase("showcase")]
    [TestCase("modal")]
    [TestCase("scroll")]
    public void ReferenceScenesRenderDeterministicallyOnSoftwareBackend(string scene)
    {
        byte[] first = Render(scene); byte[] second = Render(scene); Assert.That(Convert.ToHexString(SHA256.HashData(second)), Is.EqualTo(Convert.ToHexString(SHA256.HashData(first))));
    }

    [Test]
    public void DenseTreeCachesSteadyLayout()
    {
        UiRoot root = Root(); var stack = new StackPanel(); for (int i = 0; i < 1000; i++) stack.AddChild(new FixedNode(20 + i % 7, 1)); root.AddChild(stack); long allocated = GC.GetAllocatedBytesForCurrentThread(); root.Layout(new(800, 600)); long initialAllocation = GC.GetAllocatedBytesForCurrentThread() - allocated; Assert.That(root.Diagnostics.NodeCount, Is.EqualTo(1002)); TimeSpan initial = root.Diagnostics.LayoutDuration; allocated = GC.GetAllocatedBytesForCurrentThread(); root.Layout(new(800, 600)); long steadyAllocation = GC.GetAllocatedBytesForCurrentThread() - allocated; TestContext.Progress.WriteLine($"dense_nodes=1002 initial_layout_ms={initial.TotalMilliseconds:F3} steady_layout_ms={root.Diagnostics.LayoutDuration.TotalMilliseconds:F3} initial_allocated_bytes={initialAllocation} steady_allocated_bytes={steadyAllocation} measures={root.Diagnostics.MeasureCount} arranges={root.Diagnostics.ArrangeCount}"); Assert.Multiple(() => { Assert.That(root.Diagnostics.MeasureCount, Is.Zero); Assert.That(root.Diagnostics.ArrangeCount, Is.Zero); });
    }

    [Test]
    public void DenseSteadyPaintAvoidsPerNodeGarbage()
    {
        UiRoot root = Root(); var stack = new StackPanel(); for (int i = 0; i < 1000; i++) stack.AddChild(new FixedNode(20, 1)); root.AddChild(stack); root.Layout(new(800, 600)); using IRenderSurface surface = Graphics.CreateOffscreenSurface(new(800, 600)); using (IRenderFrame warm = surface.BeginFrame()) { root.Render(warm.Canvas); warm.Present(); }
        long allocated = GC.GetAllocatedBytesForCurrentThread(); using (IRenderFrame frame = surface.BeginFrame()) { root.Render(frame.Canvas); frame.Present(); }
        long bytes = GC.GetAllocatedBytesForCurrentThread() - allocated; TestContext.Progress.WriteLine($"dense_nodes=1002 steady_paint_ms={root.Diagnostics.PaintDuration.TotalMilliseconds:F3} steady_paint_allocated_bytes={bytes}"); Assert.That(bytes, Is.LessThan(4096));
    }

    private byte[] Render(string scene)
    {
        UiRoot root = Root(); root.AddChild(scene switch { "basic" => Basic(), "grid" => GridScene(), "modal" => ModalScene(), "scroll" => ScrollScene(), _ => Showcase() }); root.Layout(new(480, 300)); using IRenderSurface surface = Graphics.CreateOffscreenSurface(new(480, 300)); using IRenderFrame frame = surface.BeginFrame(); frame.Canvas.Clear(new(25, 20, 17)); root.Render(frame.Canvas); frame.Present(); return Graphics.EncodePng(surface.Capture());
    }

    private static Border Basic() => new() { Margin = new(16), Padding = new(12), Background = new(226, 205, 164), Child = new Button { Content = new TextBlock { Text = "Basic controls", Foreground = Color.White } } };
    private static Grid GridScene() { var grid = new Grid { Margin = new(12) }; grid.Columns.Add(new(UiLength.Fixed(100))); grid.Columns.Add(new(UiLength.Star())); grid.Rows.Add(new(UiLength.Auto)); grid.Rows.Add(new(UiLength.Star())); for (int i = 0; i < 4; i++) { var cell = new Border { Margin = new(2), Background = new Color((byte)(120 + i * 20), 80, 55), Child = new TextBlock { Text = $"Cell {i + 1}", Foreground = Color.White } }; grid.AddChild(cell); grid.SetPlacement(cell, i / 2, i % 2); } return grid; }
    private static WaxTablet Showcase() { var tablet = new WaxTablet { Margin = new(16), Child = new StackPanel { Spacing = 8 } }; var content = (StackPanel)tablet.Child; content.AddChild(new TextBlock { Text = "Gens", TypographyRole = TypographyRole.Title }); content.AddChild(new TextBlock { Text = "A retained chronicle of household and estate.", Wrapping = TextWrapping.Wrap }); content.AddChild(new WaxSealButton { Content = new TextBlock { Text = "Begin", TypographyRole = TypographyRole.Button, Foreground = Color.White } }); return tablet; }
    private static Overlay ModalScene() { var overlay = new Overlay(); overlay.AddChild(new Border { Background = new(35, 28, 24) }); overlay.AddChild(new Border { Width = 260, Height = 140, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Padding = new(16), Background = new(245, 229, 195), BorderBrush = new(133, 48, 39), BorderThickness = 3, Child = new TextBlock { Text = "Modal reference scene", TypographyRole = TypographyRole.Heading, Wrapping = TextWrapping.Wrap } }); return overlay; }
    private static ScrollView ScrollScene() { var content = new Column(); for (int i = 0; i < 20; i++) content.AddChild(new TextBlock { Text = $"Ledger row {i:00}" }); return new ScrollView { Margin = new(20), Content = content, VerticalOffset = 80 }; }
}
