using Gens.Graphics;
using Gens.Scene2D;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class SceneViewTests : UiTestFixture
{
    [Test]
    public void SceneViewClipsMapsPointerResizesAndInvalidatesPaint()
    {
        UiRoot root = Root();
        var scene = new Scene2D.Scene2D(); scene.Camera.Position = new(25, 20); scene.Camera.Zoom = 2;
        var view = new SceneView { Scene = scene, Width = 100, Height = 80, HorizontalAlignment = HorizontalAlignment.Start, VerticalAlignment = VerticalAlignment.Start };
        root.AddChild(view); root.Layout(new(200, 160));
        using IRenderSurface surface = Graphics.CreateOffscreenSurface(new(200, 160));
        using (IRenderFrame frame = surface.BeginFrame()) { root.Render(frame.Canvas); frame.Present(); }
        Point2 world = view.PointerToWorld(new(50, 40));
        Assert.Multiple(() =>
        {
            Assert.That(view.ClipToBounds, Is.True);
            Assert.That(scene.Camera.Viewport, Is.EqualTo(new Rect(0, 0, 100, 80)));
            Assert.That(world.X, Is.EqualTo(25).Within(.001));
            Assert.That(world.Y, Is.EqualTo(20).Within(.001));
        });

        view.Width = 120; root.Layout(new(200, 160));
        using (IRenderFrame frame = surface.BeginFrame()) { root.Render(frame.Canvas); frame.Present(); }
        Assert.That(scene.Camera.Viewport.Width, Is.EqualTo(120));
        scene.Root.AddChild(new RectangleNode2D { Rectangle = new(0, 0, 1, 1) });
        Assert.That(view.IsPaintValid, Is.False);
    }
}
