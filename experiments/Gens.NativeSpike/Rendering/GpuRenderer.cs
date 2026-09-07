using System.Runtime.InteropServices;
using Gens.NativeSpike.Platform;
using SkiaSharp;

namespace Gens.NativeSpike.Rendering;

// Disposable experiment: one SDL/GL owner, all Skia resources die before the native context.
internal sealed class GpuRenderer : IDisposable
{
    private IntPtr window, nativeContext;
    private GRGlInterface? gl;
    private GRContext? context;
    private GRBackendRenderTarget? target;
    private SKSurface? surface;
    private GlFinish? finish;
    private uint windowFramebuffer;
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlBindFramebuffer(uint target, uint framebuffer);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlFinish();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void GlGetInteger(uint name, out int value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate IntPtr GlGetString(uint name);
    internal int Width { get; private set; }
    internal int Height { get; private set; }
    internal SKCanvas Canvas => surface!.Canvas;
    internal void SetVsync(bool enabled)
    {
        ObjectDisposedException.ThrowIf(nativeContext == IntPtr.Zero, this);
        Check(SdlNative.SDL_GL_SetSwapInterval(enabled ? 1 : 0));
    }
    internal GpuRenderer(int width, int height)
    {
        try
        {
            Check(SdlNative.SDL_Init(SdlNative.InitVideo));
            Check(SdlNative.SDL_GL_SetAttribute(17, 3));
            Check(SdlNative.SDL_GL_SetAttribute(18, 3));
            Check(SdlNative.SDL_GL_SetAttribute(20, 1));
            Check(SdlNative.SDL_GL_SetAttribute(5, 1));
            Check(SdlNative.SDL_GL_SetAttribute(7, 8));
            window = SdlNative.SDL_CreateWindow("Gens NR2 GPU validation", width, height, 2 | SdlNative.WindowResizable | SdlNative.WindowHighPixelDensity);
            if (window == IntPtr.Zero) throw new InvalidOperationException(SdlNative.Error);
            nativeContext = SdlNative.SDL_GL_CreateContext(window);
            if (nativeContext == IntPtr.Zero) throw new InvalidOperationException(SdlNative.Error);
            var getInteger = Marshal.GetDelegateForFunctionPointer<GlGetInteger>(SdlNative.SDL_GL_GetProcAddress("glGetIntegerv"));
            getInteger(0x8ca6, out int initialFramebuffer);
            windowFramebuffer = (uint)initialFramebuffer;
            gl = GRGlInterface.Create(SdlNative.SDL_GL_GetProcAddress) ?? throw new InvalidOperationException("GL interface unavailable");
            context = GRContext.CreateGl(gl) ?? throw new InvalidOperationException("Skia GL context unavailable");
            finish = Marshal.GetDelegateForFunctionPointer<GlFinish>(SdlNative.SDL_GL_GetProcAddress("glFinish"));
            var getString = Marshal.GetDelegateForFunctionPointer<GlGetString>(SdlNative.SDL_GL_GetProcAddress("glGetString"));
            Console.WriteLine($"GL renderer={Marshal.PtrToStringUTF8(getString(0x1f01))}; version={Marshal.PtrToStringUTF8(getString(0x1f02))}");
            Console.WriteLine($"swap_interval_zero={SdlNative.SDL_GL_SetSwapInterval(0)}");
            Resize(width, height);
        }
        catch { Dispose(); throw; }
    }
    internal void Resize(int width, int height)
    {
        surface?.Dispose(); surface = null;
        target?.Dispose(); target = null;
        Check(SdlNative.SDL_SetWindowSize(window, width, height));
        while (SdlNative.SDL_PollEvent(out _)) { }
        Check(SdlNative.SDL_GetWindowSizeInPixels(window, out int pw, out int ph));
        Width = pw; Height = ph;
        var getInteger = Marshal.GetDelegateForFunctionPointer<GlGetInteger>(SdlNative.SDL_GL_GetProcAddress("glGetIntegerv"));
        // Snapshot/readback can leave a Skia-owned FBO bound. The SDL window target
        // is the framebuffer captured immediately after native context creation.
        var bind = Marshal.GetDelegateForFunctionPointer<GlBindFramebuffer>(SdlNative.SDL_GL_GetProcAddress("glBindFramebuffer"));
        bind(0x8d40, windowFramebuffer);
        context!.ResetContext();
        getInteger(0x0d57, out int stencil);
        getInteger(0x80a9, out int samples);
        target = new GRBackendRenderTarget(pw, ph, samples, stencil, new GRGlFramebufferInfo(windowFramebuffer, 0x8058));
        surface = SKSurface.Create(context, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
            ?? throw new InvalidOperationException("GPU surface unavailable");
    }
    internal void Present()
    {
        context!.Flush(); context.Submit(); finish!();
        Check(SdlNative.SDL_GL_SwapWindow(window));
        while (SdlNative.SDL_PollEvent(out _)) { }
    }
    internal void SavePng(string path)
    {
        using var snapshot = surface!.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        using var bitmap = SKBitmap.Decode(data);
        var colors = new HashSet<SKColor>();
        for (int y = 0; y < Height; y += Math.Max(1, Height / 20))
            for (int x = 0; x < Width; x += Math.Max(1, Width / 20)) colors.Add(bitmap.GetPixel(x, y));
        if (colors.Count < 3) throw new InvalidOperationException("GPU scene capture is blank; benchmark evidence rejected.");
        using var output = File.Create(path); data.SaveTo(output);
    }
    private static void Check(bool result) { if (!result) throw new InvalidOperationException(SdlNative.Error); }
    public void Dispose()
    {
        surface?.Dispose(); target?.Dispose(); context?.Dispose(); gl?.Dispose();
        surface = null; target = null; context = null; gl = null;
        if (nativeContext != IntPtr.Zero) { SdlNative.SDL_GL_DestroyContext(nativeContext); nativeContext = IntPtr.Zero; }
        if (window != IntPtr.Zero) { SdlNative.SDL_DestroyWindow(window); window = IntPtr.Zero; }
        SdlNative.SDL_Quit();
    }
}
