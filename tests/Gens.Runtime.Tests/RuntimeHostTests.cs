using System.Numerics;
using Gens.Graphics;
using Gens.Platform;
using NUnit.Framework;

namespace Gens.Runtime.Tests;

public sealed class RuntimeHostTests
{
    [Test]
    public void InitialFrameRendersThenIdleWaitsUntilInvalidated()
    {
        var platform = new FakePlatform(); var graphics = new FakeGraphics(); var app = new FakeApplication();
        using var host = new RuntimeHost(platform, graphics, app, new("test"), RendererMode.Software);
        host.Step(TimeSpan.Zero); host.Step(TimeSpan.Zero);
        Assert.Multiple(() => { Assert.That(app.Renders, Is.EqualTo(1)); Assert.That(platform.Waits, Is.EqualTo(1)); Assert.That(app.Updates, Is.Zero); });
        app.Context!.Invalidate(); host.Step(TimeSpan.Zero);
        Assert.That(app.Renders, Is.EqualTo(2));
    }

    [Test]
    public void AnimationUpdatesAndRendersContinuouslyUntilDisabled()
    {
        var app = new FakeApplication { AnimateOnInitialize = true };
        using var host = new RuntimeHost(new FakePlatform(), new FakeGraphics(), app, new("test"), RendererMode.Software);
        host.Step(TimeSpan.Zero); host.Step(TimeSpan.Zero);
        Assert.Multiple(() => { Assert.That(app.Updates, Is.EqualTo(2)); Assert.That(app.Renders, Is.EqualTo(2)); });
        app.Context!.SetAnimating(false); host.Step(TimeSpan.Zero);
        Assert.That(app.Renders, Is.EqualTo(2));
    }

    [Test]
    public void ResizeOccursBeforeEventDispatchAndQuitStopsRendering()
    {
        var platform = new FakePlatform(); var graphics = new FakeGraphics(); var app = new FakeApplication();
        using var host = new RuntimeHost(platform, graphics, app, new("test"), RendererMode.Software);
        host.Step(TimeSpan.Zero);
        platform.Window.Pixels = new(1600, 900); platform.Events.Enqueue(new WindowPixelSizeChangedEvent(2, platform.Window.Id, platform.Window.Pixels));
        host.Step(TimeSpan.Zero);
        Assert.That(graphics.Surface.PixelSize, Is.EqualTo(new PixelSize(1600, 900)));
        platform.Events.Enqueue(new QuitEvent(3)); host.Step(TimeSpan.Zero);
        Assert.Multiple(() => { Assert.That(host.IsRunning, Is.False); Assert.That(app.Renders, Is.EqualTo(2)); });
    }

    [Test]
    public void ArchitectureKeepsBackendsOutOfCoreProjects()
    {
        string root = FindRoot();
        string runtimeProject = File.ReadAllText(Path.Combine(root, "src", "Gens.Runtime", "Gens.Runtime.csproj"));
        string simulationProject = File.ReadAllText(Path.Combine(root, "src", "Gens.Simulation", "Gens.Simulation.csproj"));
        string applicationProject = File.ReadAllText(Path.Combine(root, "src", "Gens.Application", "Gens.Application.csproj"));
        Assert.Multiple(() =>
        {
            Assert.That(runtimeProject, Does.Not.Contain("Gens.Platform.Sdl")); Assert.That(runtimeProject, Does.Not.Contain("Gens.Graphics.Skia"));
            Assert.That(simulationProject, Does.Not.Contain("SDL").And.Not.Contain("Skia")); Assert.That(applicationProject, Does.Not.Contain("SDL").And.Not.Contain("Skia"));
        });
    }

    [Test]
    public void ShutdownAttemptsEveryOwnerWhenApplicationShutdownFails()
    {
        var platform = new FakePlatform(); var graphics = new FakeGraphics(); var app = new FakeApplication { ThrowOnShutdown = true };
        var host = new RuntimeHost(platform, graphics, app, new("test"), RendererMode.Software);
        host.Step(TimeSpan.Zero);
        Assert.That(() => host.Dispose(), Throws.TypeOf<AggregateException>());
        Assert.Multiple(() =>
        {
            Assert.That(platform.Disposed, Is.True);
            Assert.That(graphics.Disposed, Is.True);
            Assert.That(graphics.Surface.Disposed, Is.True);
            Assert.That(platform.Window.Disposed, Is.True);
        });
    }

    private static string FindRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Gens.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}

internal sealed class FakeApplication : IRuntimeApplication
{
    public RuntimeContext? Context { get; private set; }
    public int Renders { get; private set; }
    public int Updates { get; private set; }
    public bool AnimateOnInitialize { get; init; }
    public bool ThrowOnShutdown { get; init; }
    public void Initialize(RuntimeContext context)
    {
        Context = context;
        if (AnimateOnInitialize) context.SetAnimating(true);
    }
    public void HandleEvent(PlatformEvent platformEvent) { }
    public void Update(PresentationFrame frame) => Updates++;
    public void Render(RenderContext context) => Renders++;
    public void Shutdown() { if (ThrowOnShutdown) throw new InvalidOperationException("expected test failure"); }
}

internal sealed class FakePlatform : IPlatform
{
    public FakeWindow Window { get; } = new();
    public Queue<PlatformEvent> Events { get; } = [];
    public int Waits { get; private set; }
    public bool Disposed { get; private set; }
    public string PlatformName => "fake";
    public IClipboard Clipboard { get; } = new FakeClipboard();
    public IPlatformClock Clock { get; } = new FakeClock();
    public IWindow CreateWindow(WindowOptions options) => Window;
    public void PumpEvents(IPlatformEventSink sink)
    {
        while (Events.TryDequeue(out PlatformEvent? evt)) sink.OnEvent(evt);
    }
    public bool WaitForEvents(TimeSpan timeout, IPlatformEventSink sink)
    {
        Waits++;
        bool result = Events.Count > 0;
        PumpEvents(sink);
        ((FakeClock)Clock).Ticks += 16;
        return result;
    }
    public void Dispose() => Disposed = true;
    private sealed class FakeClipboard : IClipboard
    {
        private string? text;
        public string? GetText() => text;
        public void SetText(string value) => text = value;
    }
    private sealed class FakeClock : IPlatformClock
    {
        public long Ticks;
        public long GetTimestamp() => ++Ticks;
        public long Frequency => 1000;
    }
}
internal sealed class FakeWindow : IWindow
{
    public WindowId Id => new(1);
    public PixelSize Pixels { get; set; } = new(800, 600);
    public LogicalSize LogicalSize => new(800, 600);
    public PixelSize PixelSize => Pixels;
    public DisplayScale DisplayScale => DisplayScale.Identity;
    public WindowState State => WindowState.Normal;
    public bool Disposed { get; private set; }
    public void StartTextInput() { }
    public void StopTextInput() { }
    public void SetFullscreen(bool fullscreen) { }
    public void Dispose() => Disposed = true;
}
internal sealed class FakeGraphics : IGraphicsBackend
{
    public FakeSurface Surface { get; } = new();
    public string Name => "fake";
    public bool Disposed { get; private set; }
    public IRenderSurface CreateWindowSurface(IWindow window, RendererMode mode = RendererMode.Default, bool vsync = true)
    {
        Surface.Resize(window.PixelSize);
        return Surface;
    }
    public IRenderSurface CreateOffscreenSurface(PixelSize size) => throw new NotSupportedException();
    public IGraphicsPath CreatePath() => throw new NotSupportedException();
    public IGraphicsImage DecodeImage(Stream encodedImage) => throw new NotSupportedException();
    public IFontFace LoadFont(Stream fontData) => throw new NotSupportedException();
    public GlyphRun ShapeText(IFontFace font, string text, float fontSize) => throw new NotSupportedException();
    public byte[] EncodePng(ImageData image) => image.Pixels.ToArray();
    public void Dispose() => Disposed = true;
}
internal sealed class FakeSurface : IRenderSurface
{
    public PixelSize PixelSize { get; private set; }
    public RendererMode Mode => RendererMode.Software;
    public bool Disposed { get; private set; }
    public IRenderFrame BeginFrame() => new FakeFrame();
    public void Resize(PixelSize size) => PixelSize = size;
    public ImageData Capture() => new(PixelSize.Width, PixelSize.Height, PixelSize.Width * 4, PixelFormat.Bgra8888Premultiplied, new byte[PixelSize.Width * PixelSize.Height * 4]);
    public void Dispose() => Disposed = true;
    private sealed class FakeFrame : IRenderFrame
    {
        public ICanvas2D Canvas { get; } = new FakeCanvas();
        public void Present() { }
        public void Dispose() { }
    }
}
internal sealed class FakeCanvas : ICanvas2D
{
    public ICanvasState Save() => new State();
    public void Translate(float x, float y) { }
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
    public void DrawImage(IGraphicsImage image, Rect destination, Color tint, float opacity = 1) { }
    public void DrawImage(IGraphicsImage image, Rect source, Rect destination, Color tint, float opacity = 1) { }
    public void DrawGlyphRun(GlyphRun run, Point2 origin, Color color, float opacity = 1) { }
    private sealed class State : ICanvasState { public void Dispose() { } }
}
