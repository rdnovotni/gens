using System.Diagnostics;
using System.Runtime.InteropServices;
using Gens.NativeSpike.Assets;
using Gens.NativeSpike.Rendering;
using Gens.NativeSpike.Scenes;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Svg.Skia;

namespace Gens.NativeSpike;

internal static class ValidationModes
{
    internal static int Allocations()
    {
        using var renderer = new SoftwareRenderer(1920, 1080);
        using var paint = new SKPaint { Color = SKColors.Gold, IsAntialias = true };
        using var path = new SKPath(); path.MoveTo(10, 10); path.CubicTo(50, 100, 150, 100, 200, 10);
        void Draw() { renderer.Canvas.Clear(SKColors.Black); renderer.Canvas.DrawPath(path, paint); renderer.Canvas.DrawRect(300, 100, 200, 200, paint); renderer.Canvas.Flush(); }
        Draw();
        Console.WriteLine("scene,pixels,avg_ms,p95_ms,p99_ms,allocated_bytes_frame,working_set_mib");
        Measure("cached_resources,1920x1080", Draw, 1000);
        using var assets = new ProceduralAssets();
        foreach (var format in new[] { SKEncodedImageFormat.Png, SKEncodedImageFormat.Jpeg })
        {
            using var encoded = assets.Illustration.Encode(format, 90);
            // Decode into pixels here; FromEncodedData alone may defer codec work.
            Measure($"decode_{format},1024x768", () => { using var decoded = SKBitmap.Decode(encoded); if (decoded is null) throw new InvalidOperationException("Decode failed"); }, 120);
        }
        return 0;
    }
    internal static int Vsync()
    {
        using var gpu = new GpuRenderer(1280, 720);
        Console.WriteLine("scene,pixels,avg_ms,p95_ms,p99_ms,allocated_bytes_frame,working_set_mib");
        foreach (bool enabled in new[] { false, true })
        {
            gpu.SetVsync(enabled);
            void Draw() { gpu.Canvas.Clear(SKColors.DarkRed); gpu.Present(); }
            for (int i = 0; i < 10; i++) Draw();
            Measure($"vsync_{enabled},1280x720", Draw, 120);
        }
        return 0;
    }
    internal static void CheckAbiAndDpi()
    {
        if (Marshal.SizeOf<Platform.SdlNative.Event>() != 128 ||
            Marshal.OffsetOf<Platform.SdlNative.Event>("Key").ToInt32() != 0 ||
            Marshal.OffsetOf<Platform.SdlNative.KeyboardEvent>("Down").ToInt32() != 36 ||
            Marshal.OffsetOf<Platform.SdlNative.KeyboardEvent>("Repeat").ToInt32() != 37 ||
            Marshal.OffsetOf<Platform.SdlNative.TextInputEvent>("Text").ToInt32() != 24 ||
            Marshal.OffsetOf<Platform.SdlNative.TextEditingEvent>("Start").ToInt32() != 32 ||
            Marshal.SizeOf<Platform.SdlNative.MouseWheelEvent>() != 56)
            throw new InvalidOperationException("SDL 3.2.22 desktop 64-bit ABI mismatch");
        foreach (float scale in new[] { 1f, 1.25f, 1.5f, 2f })
        {
            var dpi = new Platform.DpiMetrics(1280, 720, (int)(1280 * scale), (int)(720 * scale));
            var (x, y) = dpi.LogicalToPixel(100, 80); var (lx, ly) = dpi.PixelToLogical(x, y);
            if (lx != 100 || ly != 80 || dpi.ScaleX != scale) throw new InvalidOperationException("DPI round-trip failed");
        }
        Console.WriteLine("SDL 3.2.22 x64 event ABI and synthetic 100/125/150/200% DPI checks passed (not monitor/IME validation)");
    }
    internal static int BenchmarkGpu()
    {
        using var assets = new ProceduralAssets();
        using var gpu = new GpuRenderer(1280, 720);
        Console.WriteLine("scene,pixels,avg_ms,p95_ms,p99_ms,allocated_bytes_frame,working_set_mib");
        foreach (var (w, h) in new[] { (1280, 720), (1920, 1080), (2560, 1440), (3840, 2160) })
        {
            gpu.Resize(w, h);
            if (gpu.Width != w || gpu.Height != h) throw new InvalidOperationException($"Requested {w}x{h}, actual {gpu.Width}x{gpu.Height}; benchmark cannot mislabel resolution.");
            for (int scene = 0; scene < 3; scene++)
            {
                int frame = 0;
                bool capture = false;
                void Draw()
                {
                    gpu.Canvas.Clear(SKColors.Black);
                    if (scene == 0) SpikeScenes.DrawBasicBenchmark(gpu.Canvas, w, h, frame / 60d, assets);
                    else { gpu.Canvas.Save(); gpu.Canvas.Scale(w / 1280f, h / 720f); if (scene == 1) SpikeScenes.DrawTablet(gpu.Canvas, assets, frame / 60d); else SpikeScenes.DrawStress(gpu.Canvas, assets, frame / 60d); gpu.Canvas.Restore(); }
                    if (capture) gpu.SavePng($"captures/gpu-{w}-{scene}.png");
                    gpu.Present();
                    frame++;
                }
                for (int i = 0; i < 10; i++) Draw();
                frame = 0;
                Measure($"{scene},{w}x{h}", Draw, 120);
                Directory.CreateDirectory("captures");
                capture = true;
                frame = 0;
                Draw(); // Read the rendered back buffer before swap, outside the timed interval.
            }
        }
        return 0;
    }
    internal static int Text()
    {
        // 150 copies of a checked-in 20-word sentence, exactly 3,000 whitespace-separated words.
        const string sentence = "The household records its harvest and remembers every promise while distant families exchange letters about land grain duty and inheritance.";
        string corpus = string.Join(' ', Enumerable.Repeat(sentence, 150));
        Directory.CreateDirectory("captures"); File.WriteAllText("captures/chronicle-3000.txt", corpus);
        string[] words = corpus.Split(' ');
        if (words.Length != 3000) throw new InvalidOperationException($"Corpus has {words.Length} words");
        using var face = SKTypeface.FromFamilyName("Segoe UI");
        using var font = new SKFont(face, 18);
        using var shaper = new SKShaper(face);
        long start = Stopwatch.GetTimestamp();
        var shaped = words.Select(w => shaper.Shape(w + " ", font)).ToArray();
        Console.WriteLine($"words={words.Length},shape_ms={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F3},font={face.FamilyName}");
        start = Stopwatch.GetTimestamp();
        using var builder = new SKTextBlobBuilder(); float x = 20, y = 28;
        foreach (var run in shaped)
        {
            if (x + run.Width > 1180) { x = 20; y += 25; }
            ushort[] ids = run.Codepoints.Select(g => checked((ushort)g)).ToArray();
            SKPoint[] points = run.Points.Select(p => new SKPoint(p.X + x, p.Y + y)).ToArray();
            builder.AddPositionedRun(ids, font, points); x += run.Width;
        }
        using var blob = builder.Build();
        Console.WriteLine($"layout_and_blob_ms={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F3},height={y + 20}");
        using var renderer = new SoftwareRenderer(1200, (int)y + 20);
        using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        void Draw() { renderer.Canvas.Clear(SKColors.White); renderer.Canvas.DrawText(blob, 0, 0, paint); renderer.Canvas.Flush(); }
        Draw(); Measure("cached_text,1200x" + renderer.Height, Draw, 120);
        renderer.SavePng("captures/chronicle.png"); return 0;
    }
    internal static int Svg()
    {
        using var svg = new SKSvg();
        var picture = svg.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", "features.svg")) ?? throw new InvalidOperationException("SVG parse failed");
        using var renderer = new SoftwareRenderer(640, 360);
        renderer.Canvas.Clear(SKColors.Transparent); renderer.Canvas.DrawPicture(picture);
        renderer.SavePng("captures/svg-features.png");
        Console.WriteLine($"SVG bounds={picture.CullRect}; hash={renderer.PixelHash()}"); return 0;
    }
    internal static int Soak(bool gpuMode)
    {
        using var gpu = gpuMode ? new GpuRenderer(640, 360) : null;
        using var sw = gpuMode ? null : new SoftwareRenderer(640, 360);
        using var paint = new SKPaint { Color = SKColors.Gold };
        Console.WriteLine("mode,iteration,working_set_bytes,managed_bytes,private_bytes");
        for (int i = 0; i < 1000; i++)
        {
            int w = 640 + i % 2 * 160, h = 360 + i % 2 * 90;
            if (gpu is not null) { gpu.Resize(w, h); gpu.Canvas.Clear(SKColors.Black); gpu.Canvas.DrawRect(1, 1, 120, 80, paint); gpu.Present(); }
            else { sw!.Resize(w, h); sw.Canvas.Clear(SKColors.Black); sw.Canvas.DrawRect(1, 1, 120, 80, paint); }
            if (i % 100 == 0) Sample("resize", i);
        }
        Sample("resize", 1000);
        for (int i = 0; i < 10000; i++)
        {
            using var surface = SKSurface.Create(new SKImageInfo(128, 128));
            using var path = new SKPath(); path.MoveTo(0, 0); path.LineTo(100, 100);
            surface.Canvas.DrawPath(path, paint);
            using var image = surface.Snapshot();
            (gpu?.Canvas ?? sw!.Canvas).DrawImage(image, 0, 0);
            if (gpu is not null && i % 100 == 0) gpu.Present();
            if (i % 1000 == 0) Sample("create_draw_dispose", i);
        }
        Sample("create_draw_dispose", 10000); return 0;
    }
    private static void Sample(string mode, int i)
    {
        using var process = Process.GetCurrentProcess();
        Console.WriteLine($"{mode},{i},{process.WorkingSet64},{GC.GetTotalMemory(false)},{process.PrivateMemorySize64}");
    }
    private static void Measure(string name, Action action, int count)
    {
        double[] samples = new double[count]; long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < count; i++) { long start = Stopwatch.GetTimestamp(); action(); samples[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds; }
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated; Array.Sort(samples);
        Console.WriteLine($"{name},{samples.Average():F3},{samples[(int)(count * .95) - 1]:F3},{samples[(int)(count * .99) - 1]:F3},{allocated / count},{Environment.WorkingSet / 1048576d:F1}");
    }
}
