using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Gens.NativeSpike.Assets;
using Gens.NativeSpike.Platform;
using Gens.NativeSpike.Rendering;
using Gens.NativeSpike.Scenes;
using Gens.NativeSpike.Text;
using SkiaSharp;

namespace Gens.NativeSpike;

internal static unsafe class Program
{
    private const uint KeyEscape = 0x1b, KeyBackspace = 0x08, KeyF1 = 0x4000003a, KeyF2 = 0x4000003b, KeyF3 = 0x4000003c, KeyF11 = 0x40000044, KeyF12 = 0x40000045;

    public static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--allocations", StringComparer.Ordinal)) return ValidationModes.Allocations();
            if (args.Contains("--vsync", StringComparer.Ordinal)) return ValidationModes.Vsync();
            if (args.Contains("--gpu-benchmark", StringComparer.Ordinal)) return ValidationModes.BenchmarkGpu();
            if (args.Contains("--text-benchmark", StringComparer.Ordinal)) return ValidationModes.Text();
            if (args.Contains("--svg", StringComparer.Ordinal)) return ValidationModes.Svg();
            if (args.Contains("--soak", StringComparer.Ordinal)) return ValidationModes.Soak(args.Contains("--gpu", StringComparer.Ordinal));
            if (args.Contains("--benchmark", StringComparer.Ordinal)) return RunBenchmarks();
            if (args.Contains("--headless", StringComparer.Ordinal)) return RunHeadless(args.Contains("--self-test", StringComparer.Ordinal));
            return RunWindow();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Native spike failed: {ex.Message}\n{ex}");
            return 1;
        }
    }

    private static int RunHeadless(bool selfTest)
    {
        ValidationModes.CheckAbiAndDpi();
        LogEnvironment("software/headless", null);
        using var assets = new ProceduralAssets();
        using var a = new SoftwareRenderer(1280, 720); using var b = new SoftwareRenderer(1280, 720);
        SpikeScenes.Draw(a.Canvas, 1280, 720, 0, 1, assets); SpikeScenes.Draw(b.Canvas, 1280, 720, 0, 1, assets);
        string hashA = a.PixelHash(), hashB = b.PixelHash();
        string path = a.SavePng(Path.Combine("captures", "reference-tablet.png"));
        ShapingResult latin = ShapingProbe.Shape("office affinity café Ελληνικά");
        ShapingResult arabic = ShapingProbe.Shape("العربية");
        var dpi = new DpiMetrics(1280, 720, 1920, 1080);
        (float px, float py) = dpi.LogicalToPixel(100, 80); (float lx, float ly) = dpi.PixelToLogical(px, py);
        Console.WriteLine($"reference={hashA}\nrepeat={hashB}\nPNG={path}\nshaping: Latin glyphs={latin.Glyphs.Length}, Arabic glyphs={arabic.Glyphs.Length}, direction={arabic.Direction}\nDPI round-trip=({lx},{ly}) scale={dpi.ScaleX}");
        if (selfTest && (hashA != hashB || File.ReadAllBytes(path).Length < 100 || latin.Glyphs.Length == 0 || arabic.Direction is not ("RightToLeft" or "Rtl") || Math.Abs(lx - 100) > .001))
            throw new InvalidOperationException("One or more headless evidence checks failed.");
        return 0;
    }

    private static int RunBenchmarks()
    {
        LogEnvironment("software/headless", null);
        using var assets = new ProceduralAssets();
        Console.WriteLine("scene,resolution,avg_ms,p95_ms,p99_ms,approx_fps,allocated_bytes_frame,gc0,gc1,gc2,working_set_mib");
        foreach ((int width, int height) in new[] { (1280, 720), (1920, 1080), (2560, 1440), (3840, 2160) })
            foreach ((string name, Action<SKCanvas, int, int, double> draw) in new (string, Action<SKCanvas, int, int, double>)[]
            {
            ("basic", (c,w,h,t) => SpikeScenes.DrawBasicBenchmark(c,w,h,t,assets)),
            ("dense", (c,w,h,t) => { c.Save(); c.Scale(w / 1280f, h / 720f); SpikeScenes.DrawTablet(c,assets,t); c.Restore(); }),
            ("visual", (c,w,h,t) => { c.Save(); c.Scale(w / 1280f, h / 720f); SpikeScenes.DrawStress(c,assets,t); c.Restore(); }),
            })
            {
                BenchmarkSample x = SoftwareRenderer.Measure(width, height, draw, 120);
                Console.WriteLine($"{name},{width}x{height},{x.AverageMs:F3},{x.P95Ms:F3},{x.P99Ms:F3},{1000 / x.AverageMs:F1},{x.AllocatedBytesPerFrame},{x.Collections[0]},{x.Collections[1]},{x.Collections[2]},{x.WorkingSetBytes / 1048576d:F1}");
            }
        return 0;
    }

    private static int RunWindow()
    {
        if (!SdlNative.SDL_Init(SdlNative.InitVideo)) throw new InvalidOperationException($"SDL_Init: {SdlNative.Error}");
        IntPtr window = IntPtr.Zero, renderer = IntPtr.Zero, texture = IntPtr.Zero;
        try
        {
            window = SdlNative.SDL_CreateWindow("Gens Native Rendering Spike", 1280, 720, SdlNative.WindowResizable | SdlNative.WindowHighPixelDensity);
            if (window == IntPtr.Zero) throw new InvalidOperationException($"SDL_CreateWindow: {SdlNative.Error}");
            renderer = SdlNative.SDL_CreateRenderer(window, null);
            if (renderer == IntPtr.Zero) throw new InvalidOperationException($"SDL_CreateRenderer: {SdlNative.Error}");
            using var assets = new ProceduralAssets();
            GetMetrics(window, out DpiMetrics metrics); using var software = new SoftwareRenderer(metrics.PixelWidth, metrics.PixelHeight);
            LogEnvironment("Skia CPU → SDL streaming texture", metrics);
            bool running = true, dirty = true, continuous = false, animate = true, overlay = true, fullscreen = false, editing = false;
            int scene = 1; float mouseX = 0, mouseY = 0, wheel = 0; uint lastKey = 0; ushort mods = 0; bool repeat = false; string input = "Type Unicode here: ";
            var clock = Stopwatch.StartNew(); var frames = new Queue<double>(); long lastFrame = Stopwatch.GetTimestamp();
            while (running)
            {
                int timeout = continuous || animate ? 1 : 250;
                if (SdlNative.SDL_WaitEventTimeout(out SdlNative.Event first, timeout)) Handle(first);
                while (SdlNative.SDL_PollEvent(out SdlNative.Event e)) Handle(e);
                if (!dirty && !continuous && !animate) continue;
                GetMetrics(window, out DpiMetrics now);
                if (now.PixelWidth != software.Width || now.PixelHeight != software.Height) { software.Resize(now.PixelWidth, now.PixelHeight); if (texture != IntPtr.Zero) SdlNative.SDL_DestroyTexture(texture); texture = IntPtr.Zero; metrics = now; }
                if (texture == IntPtr.Zero) texture = SdlNative.SDL_CreateTexture(renderer, SdlNative.PixelFormatArgb8888, SdlNative.TextureAccessStreaming, software.Width, software.Height);
                if (texture == IntPtr.Zero) throw new InvalidOperationException($"SDL_CreateTexture: {SdlNative.Error}");
                SpikeScenes.Draw(software.Canvas, software.Width, software.Height, clock.Elapsed.TotalSeconds, scene, assets);
                if (overlay) DrawOverlay(software.Canvas, metrics, mouseX, mouseY, wheel, lastKey, mods, repeat, continuous, input, frames);
                software.Canvas.Flush();
                if (!SdlNative.SDL_UpdateTexture(texture, IntPtr.Zero, software.Pixels, software.RowBytes) || !SdlNative.SDL_RenderTexture(renderer, texture, IntPtr.Zero, IntPtr.Zero) || !SdlNative.SDL_RenderPresent(renderer))
                    throw new InvalidOperationException($"SDL present: {SdlNative.Error}");
                long nowTicks = Stopwatch.GetTimestamp(); frames.Enqueue(Stopwatch.GetElapsedTime(lastFrame, nowTicks).TotalMilliseconds); lastFrame = nowTicks; while (frames.Count > 120) frames.Dequeue();
                dirty = false;
                if (continuous || animate) { double remainder = 1d / 60 - Stopwatch.GetElapsedTime(nowTicks).TotalSeconds; if (remainder > 0) Thread.Sleep(TimeSpan.FromSeconds(remainder)); }
            }
            return 0;

            void Handle(SdlNative.Event e)
            {
                dirty = true;
                if (e.Type == SdlNative.EventQuit) { running = false; return; }
                if (e.Type >= SdlNative.EventWindowFirst && e.Type <= SdlNative.EventWindowLast) return;
                if (e.Type is SdlNative.EventKeyDown or SdlNative.EventKeyUp)
                {
                    lastKey = e.Key.Scancode; mods = e.Key.Mod; repeat = e.Key.Repeat != 0; Console.WriteLine($"key={lastKey} modifiers={mods} repeat={repeat}");
                    if (e.Type == SdlNative.EventKeyUp) return;
                    switch (e.Key.Key) { case KeyEscape: running = false; break; case (uint)'1': scene = 0; break; case (uint)'2': scene = 1; break; case (uint)'3': scene = 2; break; case KeyF1: overlay = !overlay; break; case KeyF2: continuous = !continuous; break; case KeyF3: animate = !animate; break; case KeyF11: fullscreen = !fullscreen; _ = SdlNative.SDL_SetWindowFullscreen(window, fullscreen); break; case KeyF12: Console.WriteLine($"Captured {software.SavePng(Path.Combine("captures", $"frame-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"))}"); break; case KeyBackspace when editing && input.Length > 0: input = input[..^1]; break; }
                }
                else if (e.Type == SdlNative.EventMouseMotion) { mouseX = e.Motion.X; mouseY = e.Motion.Y; }
                else if (e.Type == SdlNative.EventMouseWheel) wheel = e.Wheel.Y;
                else if (e.Type == SdlNative.EventMouseButtonDown) { mouseX = e.Button.X; mouseY = e.Button.Y; editing = mouseY > 620; _ = editing ? SdlNative.SDL_StartTextInput(window) : SdlNative.SDL_StopTextInput(window); }
                else if (e.Type == SdlNative.EventTextEditing) { Console.WriteLine($"composition={Marshal.PtrToStringUTF8(e.Edit.Text)} start={e.Edit.Start} length={e.Edit.Length}"); }
                else if (e.Type == SdlNative.EventTextInput) { string text = Marshal.PtrToStringUTF8(e.Text.Text) ?? string.Empty; input += text; Console.WriteLine($"text={text}"); }
            }
        }
        finally { if (texture != IntPtr.Zero) SdlNative.SDL_DestroyTexture(texture); if (renderer != IntPtr.Zero) SdlNative.SDL_DestroyRenderer(renderer); if (window != IntPtr.Zero) SdlNative.SDL_DestroyWindow(window); SdlNative.SDL_Quit(); }
    }

    private static void GetMetrics(IntPtr window, out DpiMetrics metrics) { if (!SdlNative.SDL_GetWindowSize(window, out int lw, out int lh) || !SdlNative.SDL_GetWindowSizeInPixels(window, out int pw, out int ph)) throw new InvalidOperationException(SdlNative.Error); metrics = new(lw, lh, pw, ph); }

    private static void DrawOverlay(SKCanvas c, DpiMetrics d, float mx, float my, float wheel, uint key, ushort mods, bool repeat, bool continuous, string input, Queue<double> frames)
    {
        using var bg = new SKPaint { Color = new SKColor(0, 0, 0, 190) }; c.DrawRect(12, 12, 480, 150, bg);
        using var font = new SKFont(SKTypeface.Default, 15); using var fg = new SKPaint { Color = SKColors.White, IsAntialias = true };
        double avg = frames.Count == 0 ? 0 : frames.Average();
        string[] lines = [$"{(continuous ? "continuous" : "dirty")} software | {1000 / Math.Max(avg, .001):F0} FPS {avg:F2} ms", $"logical {d.LogicalWidth}×{d.LogicalHeight} | pixels {d.PixelWidth}×{d.PixelHeight} | scale {d.ScaleX:F2}", $"mouse logical {mx:F1},{my:F1} wheel {wheel:F1}", $"scancode {key} modifiers 0x{mods:x} repeat {repeat}", input, $"managed memory {GC.GetTotalMemory(false) / 1048576d:F1} MiB"];
        for (int i = 0; i < lines.Length; i++) c.DrawText(lines[i], 22, 34 + i * 21, font, fg);
    }

    private static void LogEnvironment(string path, DpiMetrics? dpi)
    {
        AssemblyName skia = typeof(SKCanvas).Assembly.GetName();
        Console.WriteLine($".NET {Environment.Version}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}; SkiaSharp {skia.Version}; renderer={path}");
        if (dpi is { } d) Console.WriteLine($"SDL {SdlNative.SDL_GetVersion()}; logical={d.LogicalWidth}x{d.LogicalHeight}; pixels={d.PixelWidth}x{d.PixelHeight}; effective-scale={d.ScaleX:F2}");
    }
}
