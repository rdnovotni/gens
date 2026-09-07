using Gens.Graphics;
using Gens.Platform;

namespace Gens.Runtime;

public interface IRuntimeApplication
{
    void Initialize(RuntimeContext context);
    void HandleEvent(PlatformEvent platformEvent);
    void Update(PresentationFrame frame);
    void Render(RenderContext context);
    void Shutdown();
}

public interface IRuntimeLogger
{
    void Log(RuntimeLogLevel level, string message, Exception? exception = null);
}
public enum RuntimeLogLevel { Debug, Information, Warning, Error }
public sealed class ConsoleRuntimeLogger : IRuntimeLogger
{
    public void Log(RuntimeLogLevel level, string message, Exception? exception = null) =>
        (level >= RuntimeLogLevel.Warning ? Console.Error : Console.Out).WriteLine($"[{level}] {message}{(exception is null ? string.Empty : Environment.NewLine + exception)}");
}

public sealed class RuntimeContext
{
    private readonly Action invalidate;
    private readonly Action<bool> setAnimating;
    private readonly Action requestQuit;
    internal RuntimeContext(IWindow window, Action invalidate, Action<bool> setAnimating, Action requestQuit) { Window = window; this.invalidate = invalidate; this.setAnimating = setAnimating; this.requestQuit = requestQuit; }
    public IWindow Window { get; }
    public void Invalidate() => invalidate();
    public void SetAnimating(bool enabled) => setAnimating(enabled);
    public void RequestQuit() => requestQuit();
}

public readonly record struct PresentationFrame(TimeSpan Elapsed, TimeSpan Delta, ulong FrameIndex);
public readonly record struct RenderContext(ICanvas2D Canvas, PresentationFrame Frame, LogicalSize LogicalSize, PixelSize PixelSize, DisplayScale DisplayScale);

public sealed class PresentationClock
{
    private readonly IPlatformClock clock;
    private long started, previous;
    public PresentationClock(IPlatformClock clock) { this.clock = clock; started = previous = clock.GetTimestamp(); }
    public PresentationFrame NextFrame(ulong frameIndex)
    {
        long now = clock.GetTimestamp(); long delta = now - previous; previous = now;
        return new(TimeSpan.FromSeconds((now - started) / (double)clock.Frequency), TimeSpan.FromSeconds(Math.Max(0, delta) / (double)clock.Frequency), frameIndex);
    }
}

public sealed class RuntimeDiagnostics
{
    public ulong FramesPresented { get; internal set; }
    public ulong EventsDispatched { get; internal set; }
    public ulong WaitCount { get; internal set; }
    public TimeSpan LastFrameDuration { get; internal set; }
    public bool IsAnimating { get; internal set; }
    public bool IsDirty { get; internal set; }
    public string PlatformName { get; internal init; } = string.Empty;
    public string GraphicsBackendName { get; internal init; } = string.Empty;
    public RendererMode RendererMode { get; internal init; }
}

/// <summary>
/// Owns platform, window, graphics surface and application lifetime on one thread.
/// Presentation time is monotonic and must never influence authoritative simulation outcomes.
/// </summary>
public sealed class RuntimeHost : IDisposable, IPlatformEventSink
{
    private readonly IPlatform platform;
    private readonly IGraphicsBackend graphics;
    private readonly IRuntimeApplication application;
    private readonly IRuntimeLogger logger;
    private readonly IWindow window;
    private readonly IRenderSurface surface;
    private readonly PresentationClock clock;
    private readonly Queue<PlatformEvent> pendingEvents = [];
    private bool dirty = true, animating, running = true, initialized, disposed;

    public RuntimeHost(IPlatform platform, IGraphicsBackend graphics, IRuntimeApplication application, WindowOptions windowOptions, RendererMode rendererMode = RendererMode.Default, bool vsync = true, IRuntimeLogger? logger = null)
    {
        this.platform = platform ?? throw new ArgumentNullException(nameof(platform));
        this.graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        this.application = application ?? throw new ArgumentNullException(nameof(application));
        this.logger = logger ?? new ConsoleRuntimeLogger();
        try
        {
            window = platform.CreateWindow(windowOptions);
            surface = graphics.CreateWindowSurface(window, rendererMode, vsync);
            clock = new PresentationClock(platform.Clock);
            Diagnostics = new() { PlatformName = platform.PlatformName, GraphicsBackendName = graphics.Name, RendererMode = surface.Mode, IsDirty = true };
        }
        catch { graphics.Dispose(); platform.Dispose(); throw; }
    }

    public RuntimeDiagnostics Diagnostics { get; }
    public RuntimeContext Context { get; private set; } = null!;
    public bool IsRunning => running && !disposed;

    public void Run()
    {
        EnsureInitialized();
        logger.Log(RuntimeLogLevel.Information, $"Runtime started: {Diagnostics.PlatformName}; {Diagnostics.GraphicsBackendName}; renderer={Diagnostics.RendererMode}.");
        try { while (running) Step(); }
        catch (Exception ex) { logger.Log(RuntimeLogLevel.Error, "Runtime loop failed.", ex); throw; }
    }

    /// <summary>Processes one scheduling iteration. Public to support deterministic fake-backend tests.</summary>
    public bool Step(TimeSpan? maximumIdleWait = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this); EnsureInitialized();
        TimeSpan wait = maximumIdleWait ?? (animating ? TimeSpan.FromMilliseconds(16) : TimeSpan.FromMilliseconds(250));
        if (!dirty)
        {
            Diagnostics.WaitCount++;
            platform.WaitForEvents(wait, this);
        }
        else platform.PumpEvents(this);
        DispatchEvents();
        if (!running || (!dirty && !animating)) { UpdateDiagnostics(); return running; }
        PresentationFrame presentationFrame = clock.NextFrame(Diagnostics.FramesPresented);
        long started = platform.Clock.GetTimestamp();
        // Clear before callbacks so an invalidation raised during Update/Render is retained.
        dirty = false;
        if (animating) application.Update(presentationFrame);
        PixelSize currentSize = window.PixelSize;
        if (currentSize != surface.PixelSize) surface.Resize(currentSize);
        using (IRenderFrame renderFrame = surface.BeginFrame())
        {
            application.Render(new(renderFrame.Canvas, presentationFrame, window.LogicalSize, currentSize, window.DisplayScale));
            renderFrame.Present();
        }
        Diagnostics.FramesPresented++;
        Diagnostics.LastFrameDuration = TimeSpan.FromSeconds((platform.Clock.GetTimestamp() - started) / (double)platform.Clock.Frequency);
        UpdateDiagnostics();
        return running;
    }

    public byte[] CapturePng()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return graphics.EncodePng(surface.Capture());
    }

    public void RequestQuit() => running = false;
    public void OnEvent(PlatformEvent platformEvent) => pendingEvents.Enqueue(platformEvent);

    private void DispatchEvents()
    {
        while (pendingEvents.TryDequeue(out PlatformEvent? evt))
        {
            switch (evt)
            {
                case QuitEvent or WindowCloseRequestedEvent: running = false; break;
                case WindowResizedEvent or WindowPixelSizeChangedEvent or DisplayScaleChangedEvent or WindowStateChangedEvent:
                    PixelSize pixels = window.PixelSize; if (pixels != surface.PixelSize) surface.Resize(pixels); dirty = true; break;
            }
            application.HandleEvent(evt); Diagnostics.EventsDispatched++;
        }
    }

    private void EnsureInitialized()
    {
        if (initialized) return;
        Context = new(window, () => dirty = true, enabled => { animating = enabled; if (enabled) dirty = true; }, RequestQuit);
        // Mark initialized before entering application code so partial
        // initialization still receives Shutdown during host disposal.
        initialized = true;
        application.Initialize(Context);
    }
    private void UpdateDiagnostics() { Diagnostics.IsAnimating = animating; Diagnostics.IsDirty = dirty; }

    public void Dispose()
    {
        if (disposed) return;
        List<Exception> errors = [];
        DisposeStep(() => { if (initialized) application.Shutdown(); }, "Application shutdown");
        DisposeStep(surface.Dispose, "Render-surface disposal");
        DisposeStep(window.Dispose, "Window disposal");
        DisposeStep(graphics.Dispose, "Graphics-backend disposal");
        DisposeStep(platform.Dispose, "Platform disposal");
        disposed = true; running = false;
        if (errors.Count > 0) throw new AggregateException("Runtime shutdown reported errors after attempting every disposal step.", errors);

        void DisposeStep(Action action, string operation)
        {
            try { action(); }
            catch (Exception ex) { errors.Add(ex); logger.Log(RuntimeLogLevel.Error, $"{operation} failed.", ex); }
        }
    }
}
