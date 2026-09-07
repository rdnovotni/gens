using System.Numerics;
using System.Security.Cryptography;
using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using NUnit.Framework;

namespace Gens.Scene2D.Tests;

public sealed class Scene2DTests
{
    [Test]
    public void TransformSupportsTranslationRotationScaleAndPivot()
    {
        var transform = new Transform2D(new(10, 20), 90, new(2, 3), new(1, 1));
        Point2 result = transform.TransformPoint(new(1, 2));
        Assert.Multiple(() => { Assert.That(result.X, Is.EqualTo(7).Within(.001)); Assert.That(result.Y, Is.EqualTo(20).Within(.001)); });
    }

    [Test]
    public void DeepHierarchyRoundTripsLocalAndWorldIncludingNegativeScale()
    {
        var root = new GroupNode2D { Transform = new(new(20, 10), 12, new(-1, 2), default) };
        var middle = new GroupNode2D { Transform = new(new(5, 8), -30, new(.5f, 1.2f), new(2, 3)) };
        var leaf = new GroupNode2D { Transform = new(new(-4, 6), 70, new(2, .75f), default) };
        root.AddChild(middle); middle.AddChild(leaf);
        Point2 source = new(3, 9), world = leaf.LocalToWorld(source), roundTrip = leaf.WorldToLocal(world);
        Assert.Multiple(() => { Assert.That(roundTrip.X, Is.EqualTo(source.X).Within(.001)); Assert.That(roundTrip.Y, Is.EqualTo(source.Y).Within(.001)); });
    }

    [Test]
    public void NodeRejectsCyclesAndSecondParent()
    {
        var first = new GroupNode2D(); var second = new GroupNode2D(); var child = new GroupNode2D();
        first.AddChild(child);
        Assert.Multiple(() => { Assert.That(() => second.AddChild(child), Throws.InvalidOperationException); Assert.That(() => child.AddChild(first), Throws.InvalidOperationException); });
    }

    [Test]
    public void CameraCoordinatesRoundTrip()
    {
        var camera = new Camera2D { Position = new(30, -20), Zoom = 2.5f, Viewport = new(100, 50, 800, 600) };
        Point2 world = new(90, 44), screen = camera.WorldToScreen(world), result = camera.ScreenToWorld(screen);
        Assert.Multiple(() => { Assert.That(result.X, Is.EqualTo(world.X).Within(.001)); Assert.That(result.Y, Is.EqualTo(world.Y).Within(.001)); });
    }

    [Test]
    public void RenderingOrderIsZThenStableInsertion()
    {
        var log = new List<string>(); var scene = new Scene2D(); scene.Camera.Viewport = new(0, 0, 100, 100);
        scene.Root.AddChild(new LoggingNode("middle-1", log) { ZOrder = 2 });
        scene.Root.AddChild(new LoggingNode("back", log) { ZOrder = -1 });
        scene.Root.AddChild(new LoggingNode("middle-2", log) { ZOrder = 2 });
        scene.Render(new RecordingCanvas(), new(0, 0, 100, 100));
        Assert.That(log, Is.EqualTo(new[] { "back", "middle-1", "middle-2" }));
    }

    [Test]
    public void InspectorReportsTreeAndViewportCulling()
    {
        var scene = new Scene2D(); scene.Camera.Position = new(50, 50);
        Layer2D layer = scene.AddLayer("Estate", 10);
        layer.AddChild(new RectangleNode2D { Name = "Villa", Rectangle = new(10, 10, 20, 20) });
        layer.AddChild(new Sprite2D { Name = "OffscreenProp", Size = new(10, 10), Transform = Transform2D.Identity with { Position = new(500, 500) } });
        scene.Render(new RecordingCanvas(), new(0, 0, 100, 100));
        Assert.Multiple(() =>
        {
            Assert.That(scene.LastRenderMetrics.NodeCount, Is.EqualTo(4));
            Assert.That(scene.LastRenderMetrics.CulledNodeCount, Is.EqualTo(1));
            Assert.That(scene.LastRenderMetrics.SpriteCount, Is.EqualTo(1));
            Assert.That(SceneInspector.Tree(scene), Does.Contain("Estate").And.Contain("Villa").And.Contain("OffscreenProp"));
            Assert.That(SceneInspector.Describe(scene), Does.Contain("culled=1").And.Contain("zoom=1.00"));
        });
    }

    [Test]
    public void SpritePassesTintToGraphicsBackend()
    {
        var canvas = new RecordingCanvas();
        var scene = new Scene2D(); scene.Camera.Position = new(50, 50);
        scene.Root.AddChild(new Sprite2D { Image = new FakeImage(), Size = new(10, 10), Tint = new(120, 80, 40, 200), Transform = Transform2D.Identity with { Position = new(50, 50) } });
        scene.Render(canvas, new(0, 0, 100, 100));
        Assert.That(canvas.LastTint, Is.EqualTo(new Color(120, 80, 40, 200)));
    }

    [Test]
    public void AnimationStopsRequestingFramesOnCompletion()
    {
        float value = -1; var player = new AnimationPlayer(); var scene = new Scene2D(); scene.AddAnimation(player);
        player.Play(new("fade", new IAnimationTrack[] { AnimationPlayer.Scalar(new[] { new Keyframe<float>(TimeSpan.Zero, 0), new Keyframe<float>(TimeSpan.FromSeconds(1), 1) }, x => value = x) }));
        scene.Update(TimeSpan.FromSeconds(.5)); Assert.That(value, Is.EqualTo(.5).Within(.001)); Assert.That(scene.HasActiveAnimations, Is.True);
        scene.Update(TimeSpan.FromSeconds(.5)); Assert.That(value, Is.EqualTo(1)); Assert.That(scene.HasActiveAnimations, Is.False);
    }

    [Test]
    public void AnimationLoopPingPongAndStopUseSuppliedPresentationDelta()
    {
        float value = -1; var player = new AnimationPlayer { LoopMode = LoopMode.Loop };
        var clip = new AnimationClip("motion", new IAnimationTrack[] { AnimationPlayer.Scalar(new[] { new Keyframe<float>(TimeSpan.Zero, 0), new Keyframe<float>(TimeSpan.FromSeconds(1), 10) }, x => value = x) });
        player.Play(clip); player.Update(TimeSpan.FromSeconds(1.25));
        Assert.That(value, Is.EqualTo(2.5).Within(.001));
        player.LoopMode = LoopMode.PingPong; player.Play(clip); player.Update(TimeSpan.FromSeconds(1.25));
        Assert.That(value, Is.EqualTo(7.5).Within(.001));
        player.Stop(); player.Update(TimeSpan.FromSeconds(.25));
        Assert.Multiple(() => { Assert.That(value, Is.EqualTo(7.5).Within(.001)); Assert.That(player.IsPlaying, Is.False); });
    }

    [Test]
    public void SoftwareSceneRenderingIsByteDeterministic()
    {
        using var graphics = new SkiaGraphicsBackend();
        byte[] first = Render(), second = Render();
        Assert.That(SHA256.HashData(first), Is.EqualTo(SHA256.HashData(second)));
        byte[] Render()
        {
            using IRenderSurface surface = graphics.CreateOffscreenSurface(new PixelSize(128, 128));
            var scene = new Scene2D(); scene.Camera.Position = new(64, 64);
            scene.Root.AddChild(new RectangleNode2D { Rectangle = new(12, 20, 90, 70), Color = new(160, 80, 40), CornerRadius = 8 });
            using (IRenderFrame frame = surface.BeginFrame()) { frame.Canvas.Clear(Color.Transparent); scene.Render(frame.Canvas, new(0, 0, 128, 128)); frame.Present(); }
            return surface.Capture().Pixels.ToArray();
        }
    }
}

internal sealed class LoggingNode(string name, List<string> log) : SceneNode2D { protected override void RenderSelf(ICanvas2D canvas, float opacity) => log.Add(name); }
internal sealed class RecordingCanvas : ICanvas2D
{
    public Color? LastTint { get; private set; }
    public ICanvasState Save() => new State(); public void Translate(float x, float y) { }
    public void Rotate(float degrees) { }
    public void Scale(float x, float y) { }
    public void Concat(Matrix3x2 transform) { }
    public void ClipRect(Rect rect) { }
    public void ClipPath(IGraphicsPath path) { }
    public void Clear(Color color) { }
    public void DrawRect(Rect rect, FillStyle fill) { }
    public void DrawRoundRect(Rect rect, float radiusX, float radiusY, FillStyle fill) { }
    public void DrawLine(Point2 start, Point2 destination, StrokeStyle stroke) { }
    public void DrawPath(IGraphicsPath path, FillStyle? fill, StrokeStyle? stroke = null) { }
    public void DrawImage(IGraphicsImage image, Rect destination, float opacity = 1) { }
    public void DrawImage(IGraphicsImage image, Rect source, Rect destination, float opacity = 1) { }
    public void DrawImage(IGraphicsImage image, Rect destination, Color tint, float opacity = 1) { LastTint = tint; }
    public void DrawImage(IGraphicsImage image, Rect source, Rect destination, Color tint, float opacity = 1) { LastTint = tint; }
    public void DrawGlyphRun(GlyphRun run, Point2 origin, Color color, float opacity = 1) { }
    private sealed class State : ICanvasState { public void Dispose() { } }
}

internal sealed class FakeImage : IGraphicsImage { public int Width => 10; public int Height => 10; public void Dispose() { } }
