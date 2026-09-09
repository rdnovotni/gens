using System.Runtime.InteropServices;
using Gens.Platform.Sdl.Interop;

namespace Gens.Platform.Sdl;

/// <summary>
/// Windows-only helper exposing a window's native HWND and its Win32 message stream, via SDL's own sanctioned
/// interception point (<c>SDL_SetWindowsMessageHook</c>, SDL 3.2+) rather than subclassing SDL's WndProc.
/// Deliberately HWND/message-only in its public surface — no UI Automation or other high-level Windows API
/// types belong in <c>Gens.Platform.Sdl</c>; callers (<c>Gens.Accessibility.Windows</c>) interpret messages
/// they care about (e.g. <c>WM_GETOBJECT</c>) themselves.
/// </summary>
public static class SdlWindowsMessageHook
{
    public delegate bool MessageCallback(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

    private static MessageCallback? subscriber;
    private static SdlNative.WindowsMessageHookCallback? nativeCallback;
    private static bool installed;

    /// <summary>Returns the native HWND for an SDL window, or <see cref="IntPtr.Zero"/> if unavailable (non-Windows, or no HWND property).</summary>
    public static IntPtr GetHwnd(IntPtr sdlWindow)
    {
        if (!OperatingSystem.IsWindows() || sdlWindow == IntPtr.Zero) return IntPtr.Zero;
        uint props = SdlNative.SDL_GetWindowProperties(sdlWindow);
        return props == 0 ? IntPtr.Zero : SdlNative.SDL_GetPointerProperty(props, SdlNative.PropertyWindowWin32Hwnd, IntPtr.Zero);
    }

    /// <summary>
    /// Installs (once) or replaces the single global Windows-message subscriber. Must be called on SDL's main
    /// thread, matching <c>SDL_SetWindowsMessageHook</c>'s own threading requirement. Pass null to stop
    /// forwarding messages (the native hook itself is left installed, forwarding no-ops).
    /// </summary>
    public static void SetSubscriber(MessageCallback? callback)
    {
        if (!OperatingSystem.IsWindows()) return;
        subscriber = callback;
        if (installed) return;
        nativeCallback = OnMessage;
        SdlNative.SDL_SetWindowsMessageHook(nativeCallback, IntPtr.Zero);
        installed = true;
    }

    private static bool OnMessage(IntPtr userdata, IntPtr msgPtr)
    {
        Msg msg = Marshal.PtrToStructure<Msg>(msgPtr);
        return subscriber?.Invoke(msg.Hwnd, msg.Message, msg.WParam, msg.LParam) ?? true;
    }

    // Matches the layout of the Win32 tagMSG struct.
    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr Hwnd;
        public uint Message;
        public IntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public int PtX;
        public int PtY;
    }
}
