using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Gens.Platform.Sdl.Interop;

namespace Gens.Platform.Sdl;

public sealed class SdlPlatform : IPlatform
{
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private readonly Dictionary<uint, SdlWindow> windows = [];
    private bool disposed;

    public SdlPlatform()
    {
        if (!SdlNative.SDL_Init(SdlNative.InitVideo)) throw Error("SDL_Init");
        Clipboard = new SdlClipboard(this);
        Clock = new StopwatchPlatformClock();
    }

    public string PlatformName => $"SDL {SdlNative.SDL_GetVersion()} ({RuntimeInformation.OSDescription})";
    public IClipboard Clipboard { get; }
    public IPlatformClock Clock { get; }

    public IWindow CreateWindow(WindowOptions options)
    {
        VerifyThread();
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Title);
        if (options.InitialWidth <= 0 || options.InitialHeight <= 0) throw new ArgumentOutOfRangeException(nameof(options));
        ulong flags = SdlNative.WindowOpenGl;
        if (options.Resizable) flags |= SdlNative.WindowResizable;
        if (options.HighDpi) flags |= SdlNative.WindowHighPixelDensity;
        IntPtr handle = SdlNative.SDL_CreateWindow(options.Title, options.InitialWidth, options.InitialHeight, flags);
        if (handle == IntPtr.Zero) throw Error("SDL_CreateWindow");
        var window = new SdlWindow(this, handle);
        windows.Add(window.Id.Value, window);
        if (options.MinWidth is not null || options.MinHeight is not null)
            Check(SdlNative.SDL_SetWindowMinimumSize(handle, options.MinWidth ?? 1, options.MinHeight ?? 1), "SDL_SetWindowMinimumSize");
        return window;
    }

    public void PumpEvents(IPlatformEventSink sink)
    {
        VerifyThread();
        ArgumentNullException.ThrowIfNull(sink);
        while (SdlNative.SDL_PollEvent(out SdlNative.Event evt)) Translate(evt, sink);
    }

    public bool WaitForEvents(TimeSpan timeout, IPlatformEventSink sink)
    {
        VerifyThread();
        ArgumentNullException.ThrowIfNull(sink);
        int milliseconds = (int)Math.Clamp(Math.Ceiling(timeout.TotalMilliseconds), 0, int.MaxValue);
        if (!SdlNative.SDL_WaitEventTimeout(out SdlNative.Event evt, milliseconds)) return false;
        Translate(evt, sink);
        PumpEvents(sink);
        return true;
    }

    internal void Remove(SdlWindow window) => windows.Remove(window.Id.Value);
    internal void VerifyThread()
    {
        if (Environment.CurrentManagedThreadId != ownerThread)
            throw new InvalidOperationException("SDL platform resources are owned by the thread that created SdlPlatform.");
    }

    private void Translate(SdlNative.Event evt, IPlatformEventSink sink)
    {
        PlatformEvent? translated = evt.Type switch
        {
            SdlNative.EventQuit => new QuitEvent(evt.Window.Timestamp),
            SdlNative.EventWindowCloseRequested => new WindowCloseRequestedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId)),
            SdlNative.EventWindowResized => new WindowResizedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), new(evt.Window.Data1, evt.Window.Data2)),
            SdlNative.EventWindowPixelSizeChanged => new WindowPixelSizeChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), new(evt.Window.Data1, evt.Window.Data2)),
            SdlNative.EventWindowFocusGained => new WindowFocusChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), true),
            SdlNative.EventWindowFocusLost => new WindowFocusChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), false),
            SdlNative.EventWindowMinimized => new WindowStateChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), WindowState.Minimized),
            SdlNative.EventWindowMaximized => new WindowStateChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), WindowState.Maximized),
            SdlNative.EventWindowRestored or SdlNative.EventWindowLeaveFullscreen => new WindowStateChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), WindowState.Normal),
            SdlNative.EventWindowEnterFullscreen => new WindowStateChangedEvent(evt.Window.Timestamp, Id(evt.Window.WindowId), WindowState.Fullscreen),
            SdlNative.EventWindowDisplayScaleChanged => ScaleChanged(evt.Window),
            SdlNative.EventKeyDown or SdlNative.EventKeyUp => SdlEventTranslator.Keyboard(evt.Key.Timestamp, evt.Key.WindowId, evt.Key.Scancode, evt.Key.Mod, evt.Type == SdlNative.EventKeyDown, evt.Key.Repeat != 0),
            SdlNative.EventTextInput => new TextInputEvent(evt.Text.Timestamp, Id(evt.Text.WindowId), Marshal.PtrToStringUTF8(evt.Text.Text) ?? string.Empty),
            SdlNative.EventTextEditing => new TextCompositionEvent(evt.Edit.Timestamp, Id(evt.Edit.WindowId), Marshal.PtrToStringUTF8(evt.Edit.Text) ?? string.Empty, evt.Edit.Start, evt.Edit.Length),
            SdlNative.EventMouseMotion => SdlEventTranslator.PointerMoved(evt.Motion.Timestamp, evt.Motion.WindowId, evt.Motion.X, evt.Motion.Y, evt.Motion.XRel, evt.Motion.YRel),
            SdlNative.EventMouseButtonDown or SdlNative.EventMouseButtonUp => SdlEventTranslator.PointerButton(evt.Button.Timestamp, evt.Button.WindowId, evt.Button.X, evt.Button.Y, evt.Button.Button, evt.Type == SdlNative.EventMouseButtonDown, evt.Button.Clicks),
            SdlNative.EventMouseWheel => SdlEventTranslator.Wheel(evt.Wheel.Timestamp, evt.Wheel.WindowId, evt.Wheel.MouseX, evt.Wheel.MouseY, evt.Wheel.X, evt.Wheel.Y, evt.Wheel.Direction == 1),
            _ => null,
        };
        if (translated is WindowStateChangedEvent stateEvent && windows.TryGetValue(stateEvent.Id.Value, out SdlWindow? window)) window.UpdateState(stateEvent.State);
        if (translated is not null) sink.OnEvent(translated);
    }

    private DisplayScaleChangedEvent ScaleChanged(SdlNative.WindowEvent evt)
    {
        var id = Id(evt.WindowId);
        float scale = windows.TryGetValue(evt.WindowId, out SdlWindow? window) ? window.DisplayScale.X : 1;
        return new(evt.Timestamp, id, new(scale, scale));
    }
    private static WindowId Id(uint value) => new(value);
    internal static void Check(bool success, string operation) { if (!success) throw Error(operation); }
    internal static InvalidOperationException Error(string operation) => new($"{operation} failed: {SdlNative.Error}");

    public void Dispose()
    {
        if (disposed) return;
        VerifyThread();
        foreach (SdlWindow window in windows.Values.ToArray()) window.Dispose();
        SdlNative.SDL_Quit();
        disposed = true;
    }

    private sealed class StopwatchPlatformClock : IPlatformClock { public long GetTimestamp() => Stopwatch.GetTimestamp(); public long Frequency => Stopwatch.Frequency; }
    private sealed class SdlClipboard(SdlPlatform owner) : IClipboard
    {
        public string? GetText()
        {
            owner.VerifyThread();
            IntPtr text = SdlNative.SDL_GetClipboardText();
            if (text == IntPtr.Zero) return null;
            try { return Marshal.PtrToStringUTF8(text); } finally { SdlNative.SDL_free(text); }
        }
        public void SetText(string text) { owner.VerifyThread(); ArgumentNullException.ThrowIfNull(text); Check(SdlNative.SDL_SetClipboardText(text), "SDL_SetClipboardText"); }
    }
}

internal sealed class SdlWindow : IGraphicsWindow, IPixelBufferWindow
{
    private readonly SdlPlatform owner;
    private IntPtr handle;
    private IntPtr renderer;
    private IntPtr texture;
    private PixelSize textureSize;
    private WindowState state;

    internal SdlWindow(SdlPlatform owner, IntPtr handle) { this.owner = owner; this.handle = handle; Id = new(SdlNative.SDL_GetWindowID(handle)); }
    public WindowId Id { get; }
    /// <summary>The raw native SDL window handle, for platform-specific glue (e.g. Windows UI Automation hosting) that needs it. Not part of the engine-neutral <c>IGraphicsWindow</c>/<c>IPixelBufferWindow</c> surface.</summary>
    internal IntPtr NativeSdlWindowHandle => handle;
    public LogicalSize LogicalSize { get { Verify(); SdlPlatform.Check(SdlNative.SDL_GetWindowSize(handle, out int w, out int h), "SDL_GetWindowSize"); return new(w, h); } }
    public PixelSize PixelSize { get { Verify(); SdlPlatform.Check(SdlNative.SDL_GetWindowSizeInPixels(handle, out int w, out int h), "SDL_GetWindowSizeInPixels"); return new(w, h); } }
    public DisplayScale DisplayScale { get { Verify(); float scale = SdlNative.SDL_GetWindowDisplayScale(handle); return new(scale, scale); } }
    public WindowState State => state;
    internal void UpdateState(WindowState newState) => state = newState;
    public void StartTextInput() { Verify(); SdlPlatform.Check(SdlNative.SDL_StartTextInput(handle), "SDL_StartTextInput"); }
    public void StopTextInput() { Verify(); SdlPlatform.Check(SdlNative.SDL_StopTextInput(handle), "SDL_StopTextInput"); }
    public void SetFullscreen(bool fullscreen) { Verify(); SdlPlatform.Check(SdlNative.SDL_SetWindowFullscreen(handle, fullscreen), "SDL_SetWindowFullscreen"); state = fullscreen ? WindowState.Fullscreen : WindowState.Normal; }
    public IOpenGlContext CreateOpenGlContext(OpenGlContextOptions options) { Verify(); return new SdlOpenGlContext(owner, handle, options); }

    public unsafe void PresentPixels(ReadOnlySpan<byte> pixels, PixelSize size, int rowBytes)
    {
        Verify();
        if (renderer == IntPtr.Zero) { renderer = SdlNative.SDL_CreateRenderer(handle, null); if (renderer == IntPtr.Zero) throw SdlPlatform.Error("SDL_CreateRenderer"); }
        if (texture == IntPtr.Zero || textureSize != size)
        {
            if (texture != IntPtr.Zero) SdlNative.SDL_DestroyTexture(texture);
            texture = SdlNative.SDL_CreateTexture(renderer, SdlNative.PixelFormatArgb8888, SdlNative.TextureAccessStreaming, size.Width, size.Height);
            if (texture == IntPtr.Zero) throw SdlPlatform.Error("SDL_CreateTexture");
            textureSize = size;
        }
        fixed (byte* pointer = pixels)
            SdlPlatform.Check(SdlNative.SDL_UpdateTexture(texture, IntPtr.Zero, (IntPtr)pointer, rowBytes), "SDL_UpdateTexture");
        SdlPlatform.Check(SdlNative.SDL_RenderTexture(renderer, texture, IntPtr.Zero, IntPtr.Zero), "SDL_RenderTexture");
        SdlPlatform.Check(SdlNative.SDL_RenderPresent(renderer), "SDL_RenderPresent");
    }

    private void Verify() { owner.VerifyThread(); ObjectDisposedException.ThrowIf(handle == IntPtr.Zero, this); }
    public void Dispose()
    {
        if (handle == IntPtr.Zero) return;
        Verify();
        if (texture != IntPtr.Zero) SdlNative.SDL_DestroyTexture(texture);
        if (renderer != IntPtr.Zero) SdlNative.SDL_DestroyRenderer(renderer);
        SdlNative.SDL_DestroyWindow(handle); texture = renderer = handle = IntPtr.Zero; owner.Remove(this);
    }
}

internal sealed class SdlOpenGlContext : IOpenGlContext
{
    private readonly SdlPlatform owner;
    private readonly IntPtr window;
    private IntPtr context;
    internal SdlOpenGlContext(SdlPlatform owner, IntPtr window, OpenGlContextOptions options)
    {
        this.owner = owner; this.window = window;
        SdlPlatform.Check(SdlNative.SDL_GL_SetAttribute(17, options.MajorVersion), "SDL_GL_SetAttribute major");
        SdlPlatform.Check(SdlNative.SDL_GL_SetAttribute(18, options.MinorVersion), "SDL_GL_SetAttribute minor");
        SdlPlatform.Check(SdlNative.SDL_GL_SetAttribute(20, options.CoreProfile ? 1 : 0), "SDL_GL_SetAttribute profile");
        SdlPlatform.Check(SdlNative.SDL_GL_SetAttribute(6, options.DepthBits), "SDL_GL_SetAttribute depth");
        SdlPlatform.Check(SdlNative.SDL_GL_SetAttribute(7, options.StencilBits), "SDL_GL_SetAttribute stencil");
        context = SdlNative.SDL_GL_CreateContext(window);
        if (context == IntPtr.Zero) throw SdlPlatform.Error("SDL_GL_CreateContext");
        DefaultFramebuffer = QueryDefaultFramebuffer();
    }
    public uint DefaultFramebuffer { get; }
    public IntPtr GetProcAddress(string name) { Verify(); return SdlNative.SDL_GL_GetProcAddress(name); }
    public void SetVSync(bool enabled) { Verify(); SdlPlatform.Check(SdlNative.SDL_GL_SetSwapInterval(enabled ? 1 : 0), "SDL_GL_SetSwapInterval"); }
    public void SwapBuffers() { Verify(); SdlPlatform.Check(SdlNative.SDL_GL_SwapWindow(window), "SDL_GL_SwapWindow"); }
    private static uint QueryDefaultFramebuffer() { var get = Marshal.GetDelegateForFunctionPointer<GlGetInteger>(SdlNative.SDL_GL_GetProcAddress("glGetIntegerv")); get(0x8ca6, out int value); return (uint)value; }
    private void Verify() { owner.VerifyThread(); ObjectDisposedException.ThrowIf(context == IntPtr.Zero, this); }
    public void Dispose() { if (context == IntPtr.Zero) return; Verify(); SdlNative.SDL_GL_DestroyContext(context); context = IntPtr.Zero; }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlGetInteger(uint name, out int value);
}
