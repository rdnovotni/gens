using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Platform.Sdl;
using Gens.Runtime;
using System.Diagnostics;

namespace Gens.EngineSandbox;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--reference-benchmark", StringComparer.OrdinalIgnoreCase)) return SandboxBenchmark.Run();
            RendererMode mode = ParseRenderer(args);
            using var platform = new SdlPlatform();
            using var graphics = new SkiaGraphicsBackend();
            var application = new SandboxApplication(graphics, args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase));
            using var host = new RuntimeHost(platform, graphics, application,
                new WindowOptions("Gens Engine Sandbox", 1280, 720, Resizable: true, HighDpi: true, MinWidth: 640, MinHeight: 360), mode);
            application.CapturePng = host.CapturePng;
            host.Run();
            RuntimeDiagnostics d = host.Diagnostics;
            Console.WriteLine($"Stopped cleanly. frames={d.FramesPresented}; events={d.EventsDispatched}; waits={d.WaitCount}; last_frame_ms={d.LastFrameDuration.TotalMilliseconds:F3}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Engine sandbox failed: {ex.Message}{Environment.NewLine}{ex}");
            return 1;
        }
    }

    private static RendererMode ParseRenderer(string[] args)
    {
        string? value = args.FirstOrDefault(static x => x.StartsWith("--renderer=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];
        return value?.ToLowerInvariant() switch { null or "default" => RendererMode.Default, "gpu" => RendererMode.Gpu, "software" => RendererMode.Software, _ => throw new ArgumentException("Renderer must be default, gpu, or software.") };
    }
}

internal static class SandboxBenchmark
{
    internal static int Run()
    {
        const int frames = 300;
        using var graphics = new SkiaGraphicsBackend();
        using IRenderSurface surface = graphics.CreateOffscreenSurface(new(1920, 1080));
        using IGraphicsPath path = graphics.CreatePath();
        path.MoveTo(new(40, 400)); path.CubicTo(new(420, 50), new(760, 780), new(1100, 240)); path.LineTo(new(1300, 700)); path.Close();
        for (int i = 0; i < 20; i++) Draw(i);
        var samples = new double[frames];
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < frames; i++)
        {
            long started = Stopwatch.GetTimestamp(); Draw(i); samples[i] = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        }
        long bytesPerFrame = (GC.GetAllocatedBytesForCurrentThread() - allocated) / frames;
        Array.Sort(samples);
        Console.WriteLine($"reference_1920x1080 frames={frames} average_ms={samples.Average():F3} p95_ms={samples[(int)(frames * .95) - 1]:F3} p99_ms={samples[(int)(frames * .99) - 1]:F3} allocated_bytes_per_frame={bytesPerFrame}");
        return 0;

        void Draw(int frameIndex)
        {
            using IRenderFrame frame = surface.BeginFrame();
            ICanvas2D canvas = frame.Canvas; canvas.Clear(new(26, 21, 18));
            canvas.DrawRoundRect(new(24, 24, 1872, 1032), 18, 18, new(new Color(226, 202, 157)));
            using (canvas.Save()) { canvas.ClipRect(new(60, 60, 1800, 940)); canvas.Translate(frameIndex % 3, 0); canvas.DrawPath(path, new(new Color(133, 48, 39)), new(new Color(55, 31, 25), 3)); }
            for (int i = 0; i < 24; i++) canvas.DrawRect(new(80 + i * 70, 820, 48, 120 - (i % 5) * 12), new(new Color((byte)(100 + i * 5), 75, 45)));
            frame.Present();
        }
    }
}

internal sealed class SandboxApplication(IGraphicsBackend graphics, bool smokeTest) : IRuntimeApplication
{
    private RuntimeContext context = null!;
    private IGraphicsPath path = null!;
    private IGraphicsImage image = null!;
    private IFontFace font = null!, arabicFont = null!;
    private GlyphRun title = null!, sample = null!, arabicSample = null!;
    private GlyphRun? inputRun, metricsRun;
    private string? shapedInput, shapedMetrics;
    private PointerPosition pointer;
    private string input = "Click and type: ";
    private bool animating = true, fullscreen;
    private double animationSeconds;
    private bool smokeCaptured;
    public Func<byte[]> CapturePng { private get; set; } = null!;

    public void Initialize(RuntimeContext runtimeContext)
    {
        context = runtimeContext;
        string fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "NotoSans-Regular.ttf");
        using (FileStream stream = File.OpenRead(fontPath)) font = graphics.LoadFont(stream);
        string arabicFontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "NotoSansArabic-Regular.ttf");
        using (FileStream stream = File.OpenRead(arabicFontPath)) arabicFont = graphics.LoadFont(stream);
        title = graphics.ShapeText(font, "Gens Production Runtime", 34);
        sample = graphics.ShapeText(font, "Shaped text: office affinity  Ελληνικά", 22);
        arabicSample = graphics.ShapeText(arabicFont, "العربية", 22);
        path = graphics.CreatePath(); path.MoveTo(new(0, 48)); path.CubicTo(new(65, -20), new(135, 115), new(210, 30)); path.LineTo(new(210, 85)); path.QuadTo(new(90, 125), new(0, 48)); path.Close();
        using IRenderSurface offscreen = graphics.CreateOffscreenSurface(new(96, 96));
        using (IRenderFrame frame = offscreen.BeginFrame())
        {
            frame.Canvas.Clear(new(38, 31, 25));
            for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++) frame.Canvas.DrawRect(new(x * 24, y * 24, 24, 24), new(new Color((byte)(150 + x * 20), (byte)(92 + y * 20), 48)));
            frame.Canvas.DrawLine(new(0, 0), new(96, 96), new(Color.White, 5, LineCap.Round)); frame.Present();
        }
        using var png = new MemoryStream(graphics.EncodePng(offscreen.Capture())); image = graphics.DecodeImage(png);
        context.SetAnimating(true); context.Window.StartTextInput();
    }

    public void HandleEvent(PlatformEvent platformEvent)
    {
        switch (platformEvent)
        {
            case PointerMovedEvent moved: pointer = moved.Position; context.Invalidate(); break;
            case PointerButtonEvent { IsDown: true } clicked: pointer = clicked.Position; context.Invalidate(); break;
            case WheelEvent wheel: animationSeconds = Math.Max(0, animationSeconds + wheel.Delta.Y * .1); context.Invalidate(); break;
            case TextInputEvent text: input += text.Text; context.Invalidate(); break;
            case TextCompositionEvent composition: Console.WriteLine($"IME composition='{composition.Text}' selection={composition.SelectionStart}+{composition.SelectionLength}"); break;
            case KeyboardEvent { IsDown: true, IsRepeat: false } key: HandleKey(key.Key); break;
        }
    }

    public void Update(PresentationFrame frame)
    {
        animationSeconds += Math.Min(frame.Delta.TotalSeconds, .1);
        double duration = smokeTest ? .25 : 4;
        if (animationSeconds >= duration && animating)
        {
            animating = false; context.SetAnimating(false);
            if (smokeTest && !smokeCaptured)
            {
                smokeCaptured = true; string path = Path.Combine(Environment.CurrentDirectory, $"engine-sandbox-smoke-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
                File.WriteAllBytes(path, CapturePng()); Console.WriteLine($"Smoke capture: {path}"); context.RequestQuit();
            }
            else Console.WriteLine("Animation completed; runtime is now dirty-redraw/event-wait driven. Press F3 to restart.");
        }
    }

    public void Render(RenderContext render)
    {
        ICanvas2D c = render.Canvas; float width = render.LogicalSize.Width; float height = render.LogicalSize.Height;
        c.Clear(new(25, 20, 17));
        c.DrawRect(new(0, 0, width, 74), new(new Color(79, 43, 31)));
        c.DrawGlyphRun(title, new(28, 48), new(245, 225, 184));
        c.DrawGlyphRun(sample, new(32, 112), new(232, 215, 185));
        c.DrawGlyphRun(arabicSample, new(500, 112), new(232, 215, 185));
        c.DrawRoundRect(new(28, 142, width - 56, height - 188), 14, 14, new(new Color(224, 199, 151)));
        using (c.Save())
        {
            c.ClipRect(new(46, 160, width - 92, height - 224));
            c.Translate(78, 205); c.Rotate((float)Math.Sin(animationSeconds * 2) * 6);
            c.DrawPath(path, new(new Color(125, 40, 38, 210)), new(new Color(61, 34, 27), 3, LineCap.Round, LineJoin.Round));
        }
        c.DrawImage(image, new(width - 190, 190, 112, 112), .95f);
        float orbX = 90 + (float)((Math.Sin(animationSeconds * 2.2) + 1) * Math.Max(1, width - 240) / 2);
        c.DrawRoundRect(new(orbX, height - 180, 42, 42), 21, 21, new(new Color(190, 65, 43)));
        c.DrawLine(new(pointer.X - 10, pointer.Y), new(pointer.X + 10, pointer.Y), new(new Color(35, 90, 110), 2));
        c.DrawLine(new(pointer.X, pointer.Y - 10), new(pointer.X, pointer.Y + 10), new(new Color(35, 90, 110), 2));
        if (shapedInput != input) { shapedInput = input; inputRun = graphics.ShapeText(font, input, 18); }
        c.DrawGlyphRun(inputRun!, new(48, height - 70), new(45, 35, 30));
        string metricText = $"logical {render.LogicalSize.Width}×{render.LogicalSize.Height} | pixels {render.PixelSize.Width}×{render.PixelSize.Height} | scale {render.DisplayScale.X:F2} | renderer controls: F3 animation, F11 fullscreen, F12 capture, Esc quit";
        if (shapedMetrics != metricText) { shapedMetrics = metricText; metricsRun = graphics.ShapeText(font, metricText, 13); }
        c.DrawGlyphRun(metricsRun!, new(22, height - 18), new(215, 197, 164));
    }

    private void HandleKey(PhysicalKey key)
    {
        switch (key.ScanCode)
        {
            case 41: context.RequestQuit(); break;
            case 60: animating = !animating; if (animating) animationSeconds = 0; context.SetAnimating(animating); break;
            case 68: fullscreen = !fullscreen; context.Window.SetFullscreen(fullscreen); context.Invalidate(); break;
            case 69:
                string directory = Path.Combine(Environment.CurrentDirectory, "captures"); Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, $"engine-sandbox-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"); File.WriteAllBytes(path, CapturePng()); Console.WriteLine($"Captured {path}"); break;
        }
    }

    public void Shutdown() { context.Window.StopTextInput(); path.Dispose(); image.Dispose(); arabicFont.Dispose(); font.Dispose(); }
}
