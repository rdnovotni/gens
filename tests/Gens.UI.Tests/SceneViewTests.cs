using Gens.Graphics;
using Gens.Scene2D;
using NUnit.Framework;

namespace Gens.UI.Tests;

public sealed class SceneViewTests : UiTestFixture
{
    [Test]
    public void DecorativeSceneAnimationStopsUnderReducedMotion()
    {
        float value = 0; var scene = new Scene2D.Scene2D(); var player = new AnimationPlayer();
        scene.AddAnimation(player); player.Play(new("ambient", [AnimationPlayer.Scalar([new(TimeSpan.Zero, 0), new(TimeSpan.FromSeconds(1), 1)], updated => value = updated)]));
        UiRoot root = Root(); var view = new SceneView { Scene = scene }; root.AddChild(view); root.MotionPolicy = new(MotionMode.Reduced); view.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Multiple(() => { Assert.That(value, Is.Zero); Assert.That(view.HasActiveAnimations, Is.False); });
        root.MotionPolicy = new(MotionMode.Full); view.Advance(TimeSpan.FromMilliseconds(500)); Assert.That(value, Is.EqualTo(.5f).Within(.001));
    }

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
