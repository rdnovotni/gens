using System.Numerics;

namespace Gens.Platform;

public readonly record struct WindowId(uint Value);
public readonly record struct LogicalSize(int Width, int Height);
public readonly record struct PixelSize(int Width, int Height);
public readonly record struct DisplayScale(float X, float Y)
{
    public static readonly DisplayScale Identity = new(1, 1);
}
public readonly record struct PointerPosition(float X, float Y);

public sealed record WindowOptions(
    string Title,
    int InitialWidth = 1280,
    int InitialHeight = 720,
    bool Resizable = true,
    bool HighDpi = true,
    int? MinWidth = null,
    int? MinHeight = null);

public enum WindowState { Normal, Minimized, Maximized, Fullscreen }
public enum PointerButton { Unknown, Primary, Middle, Secondary, X1, X2 }

[Flags]
public enum KeyModifiers
{
    None = 0, LeftShift = 1 << 0, RightShift = 1 << 1, LeftControl = 1 << 2,
    RightControl = 1 << 3, LeftAlt = 1 << 4, RightAlt = 1 << 5,
    LeftGui = 1 << 6, RightGui = 1 << 7, NumLock = 1 << 8,
    CapsLock = 1 << 9, AltGr = 1 << 10,
}

/// <summary>A hardware-position key identity. Text is delivered separately by <see cref="TextInputEvent"/>.</summary>
public readonly record struct PhysicalKey(uint ScanCode)
{
    public static readonly PhysicalKey Unknown = new(0);
}

public abstract record PlatformEvent(ulong Timestamp, WindowId? WindowId);
public sealed record QuitEvent(ulong Timestamp) : PlatformEvent(Timestamp, null);
public sealed record WindowCloseRequestedEvent(ulong Timestamp, WindowId Id) : PlatformEvent(Timestamp, Id);
public sealed record WindowResizedEvent(ulong Timestamp, WindowId Id, LogicalSize Size) : PlatformEvent(Timestamp, Id);
public sealed record WindowPixelSizeChangedEvent(ulong Timestamp, WindowId Id, PixelSize Size) : PlatformEvent(Timestamp, Id);
public sealed record WindowFocusChangedEvent(ulong Timestamp, WindowId Id, bool HasFocus) : PlatformEvent(Timestamp, Id);
public sealed record WindowStateChangedEvent(ulong Timestamp, WindowId Id, WindowState State) : PlatformEvent(Timestamp, Id);
public sealed record DisplayScaleChangedEvent(ulong Timestamp, WindowId Id, DisplayScale Scale) : PlatformEvent(Timestamp, Id);
public sealed record KeyboardEvent(ulong Timestamp, WindowId Id, PhysicalKey Key, KeyModifiers Modifiers, bool IsDown, bool IsRepeat) : PlatformEvent(Timestamp, Id);
public sealed record TextInputEvent(ulong Timestamp, WindowId Id, string Text) : PlatformEvent(Timestamp, Id);
public sealed record TextCompositionEvent(ulong Timestamp, WindowId Id, string Text, int SelectionStart, int SelectionLength) : PlatformEvent(Timestamp, Id);
public sealed record PointerMovedEvent(ulong Timestamp, WindowId Id, PointerPosition Position, Vector2 Delta) : PlatformEvent(Timestamp, Id);
public sealed record PointerButtonEvent(ulong Timestamp, WindowId Id, PointerPosition Position, PointerButton Button, bool IsDown, byte ClickCount) : PlatformEvent(Timestamp, Id);
public sealed record WheelEvent(ulong Timestamp, WindowId Id, PointerPosition Position, Vector2 Delta) : PlatformEvent(Timestamp, Id);

public interface IPlatformEventSink { void OnEvent(PlatformEvent platformEvent); }

public interface IClipboard
{
    string? GetText();
    void SetText(string text);
}

public interface IPlatformClock { long GetTimestamp(); long Frequency { get; } }

public interface IEventSource
{
    void PumpEvents(IPlatformEventSink sink);
    bool WaitForEvents(TimeSpan timeout, IPlatformEventSink sink);
}

public interface IPlatform : IEventSource, IDisposable
{
    string PlatformName { get; }
    IClipboard Clipboard { get; }
    IPlatformClock Clock { get; }
    IWindow CreateWindow(WindowOptions options);
}

public interface IWindow : IDisposable
{
    WindowId Id { get; }
    LogicalSize LogicalSize { get; }
    PixelSize PixelSize { get; }
    DisplayScale DisplayScale { get; }
    WindowState State { get; }
    void StartTextInput();
    void StopTextInput();
    void SetFullscreen(bool fullscreen);
}

public sealed record OpenGlContextOptions(int MajorVersion = 3, int MinorVersion = 3, bool CoreProfile = true, int DepthBits = 0, int StencilBits = 8);

/// <summary>Engine-neutral graphics context supplied by a platform window.</summary>
public interface IOpenGlContext : IDisposable
{
    IntPtr GetProcAddress(string name);
    void SetVSync(bool enabled);
    void SwapBuffers();
    uint DefaultFramebuffer { get; }
}

public interface IGraphicsWindow : IWindow
{
    IOpenGlContext CreateOpenGlContext(OpenGlContextOptions options);
}

/// <summary>Optional CPU pixel presentation capability for the reference renderer.</summary>
public interface IPixelBufferWindow : IWindow
{
    void PresentPixels(ReadOnlySpan<byte> pixels, PixelSize size, int rowBytes);
}
