// SDL specifies UTF-8 char*, never Windows ANSI or UTF-16.
#pragma warning disable CA2101
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gens.Platform.Sdl.Interop;

internal static class SdlNative
{
    private const string Library = "SDL3";
    internal const uint InitVideo = 0x20;
    internal const ulong WindowOpenGl = 0x2, WindowResizable = 0x20, WindowHighPixelDensity = 0x2000;
    internal const uint PixelFormatArgb8888 = 0x16362004;
    internal const int TextureAccessStreaming = 1;
    internal const uint EventQuit = 0x100, EventWindowFirst = 0x200, EventWindowLast = 0x2ff;
    internal const uint EventWindowResized = 0x206, EventWindowPixelSizeChanged = 0x207, EventWindowMinimized = 0x209;
    internal const uint EventWindowMaximized = 0x20a, EventWindowRestored = 0x20b, EventWindowFocusGained = 0x20e;
    internal const uint EventWindowFocusLost = 0x20f, EventWindowCloseRequested = 0x210, EventWindowDisplayScaleChanged = 0x214;
    internal const uint EventWindowEnterFullscreen = 0x217, EventWindowLeaveFullscreen = 0x218;
    internal const uint EventKeyDown = 0x300, EventKeyUp = 0x301, EventTextEditing = 0x302, EventTextInput = 0x303;
    internal const uint EventMouseMotion = 0x400, EventMouseButtonDown = 0x401, EventMouseButtonUp = 0x402, EventMouseWheel = 0x403;

    static SdlNative() => NativeLibrary.SetDllImportResolver(typeof(SdlNative).Assembly, Resolve);

    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? path)
    {
        if (name != Library) return IntPtr.Zero;
        string file = OperatingSystem.IsWindows() ? "SDL3.dll" : OperatingSystem.IsMacOS() ? "libSDL3.0.dylib" : "libSDL3.so.0";
        string rid = RuntimeInformation.RuntimeIdentifier;
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, file),
            Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", file),
        ];
        foreach (string candidate in candidates)
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out IntPtr handle)) return handle;
        throw new DllNotFoundException($"SDL3 native library '{file}' was not found beside the executable or in runtimes/{rid}/native. The runtime never searches PATH.");
    }

    [StructLayout(LayoutKind.Explicit, Size = 128)]
    internal struct Event
    {
        [FieldOffset(0)] internal uint Type;
        [FieldOffset(0)] internal WindowEvent Window;
        [FieldOffset(0)] internal KeyboardEvent Key;
        [FieldOffset(0)] internal TextInputEvent Text;
        [FieldOffset(0)] internal TextEditingEvent Edit;
        [FieldOffset(0)] internal MouseMotionEvent Motion;
        [FieldOffset(0)] internal MouseButtonEvent Button;
        [FieldOffset(0)] internal MouseWheelEvent Wheel;
    }
    [StructLayout(LayoutKind.Sequential)] internal struct WindowEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal int Data1; internal int Data2; }
    [StructLayout(LayoutKind.Sequential)] internal struct KeyboardEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal uint Which; internal uint Scancode; internal uint Key; internal ushort Mod; internal ushort Raw; internal byte Down; internal byte Repeat; }
    [StructLayout(LayoutKind.Sequential)] internal struct TextInputEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal IntPtr Text; }
    [StructLayout(LayoutKind.Sequential)] internal struct TextEditingEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal IntPtr Text; internal int Start; internal int Length; }
    [StructLayout(LayoutKind.Sequential)] internal struct MouseMotionEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal uint Which; internal uint State; internal float X; internal float Y; internal float XRel; internal float YRel; }
    [StructLayout(LayoutKind.Sequential)] internal struct MouseButtonEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal uint Which; internal byte Button; internal byte Down; internal byte Clicks; internal byte Padding; internal float X; internal float Y; }
    [StructLayout(LayoutKind.Sequential)] internal struct MouseWheelEvent { internal uint Type; internal uint Reserved; internal ulong Timestamp; internal uint WindowId; internal uint Which; internal float X; internal float Y; internal uint Direction; internal float MouseX; internal float MouseY; internal int IntegerX; internal int IntegerY; }

    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_Init(uint flags);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_Quit();
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GetError();
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_GetVersion();
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_CreateWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, int width, int height, ulong flags);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_DestroyWindow(IntPtr window);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern uint SDL_GetWindowID(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GetWindowSize(IntPtr window, out int width, out int height);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GetWindowSizeInPixels(IntPtr window, out int width, out int height);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern float SDL_GetWindowDisplayScale(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_SetWindowMinimumSize(IntPtr window, int width, int height);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_SetWindowFullscreen(IntPtr window, [MarshalAs(UnmanagedType.I1)] bool fullscreen);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_StartTextInput(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_StopTextInput(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_PollEvent(out Event @event);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_WaitEventTimeout(out Event @event, int milliseconds);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GetClipboardText();
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_SetClipboardText([MarshalAs(UnmanagedType.LPUTF8Str)] string text);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_free(IntPtr memory);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_CreateRenderer(IntPtr window, [MarshalAs(UnmanagedType.LPUTF8Str)] string? name);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_DestroyRenderer(IntPtr renderer);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_CreateTexture(IntPtr renderer, uint format, int access, int width, int height);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_DestroyTexture(IntPtr texture);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_RenderTexture(IntPtr renderer, IntPtr texture, IntPtr source, IntPtr destination);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_RenderPresent(IntPtr renderer);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GL_SetAttribute(int attribute, int value);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GL_CreateContext(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GL_DestroyContext(IntPtr context);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GL_SwapWindow(IntPtr window);
    [return: MarshalAs(UnmanagedType.I1)][DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern bool SDL_GL_SetSwapInterval(int interval);
    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GL_GetProcAddress([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    internal static string Error => Marshal.PtrToStringUTF8(SDL_GetError()) ?? "unknown SDL error";
}
