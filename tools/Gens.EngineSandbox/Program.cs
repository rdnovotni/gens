using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Platform.Sdl;
using Gens.Runtime;
using Gens.Portraits;
using Gens.Presentation.Visuals;
using Gens.Scene2D;
using Gens.Simulation.Characters;
using Gens.UI;
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
            bool runtimePage = args.Any(static x => string.Equals(x, "--page=runtime", StringComparison.OrdinalIgnoreCase));
            IRuntimeApplication application = runtimePage
                ? new SandboxApplication(graphics, args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
                : new UiSandboxApplication(graphics, args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase));
            using var host = new RuntimeHost(platform, graphics, application,
                new WindowOptions("Gens Engine Sandbox", 1280, 720, Resizable: true, HighDpi: true, MinWidth: 640, MinHeight: 360), mode);
            if (application is SandboxApplication runtimeApplication) runtimeApplication.CapturePng = host.CapturePng;
            if (application is UiSandboxApplication uiApplication) uiApplication.CapturePng = host.CapturePng;
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

internal sealed class UiSandboxApplication(IGraphicsBackend graphics, bool smokeTest) : IRuntimeApplication, IDisposable
{
    private RuntimeContext context = null!;
    private UiRoot root = null!;
    private IFontFace font = null!;
    private TextBlock inspectorText = null!;
    private bool fullscreen, smokeCaptured;
    private bool treePrinted;
    private bool hasRendered;
    private SceneView sceneView = null!;
    private PortraitService portraits = null!;
    private IGraphicsImage sceneSprite = null!;
    private IGraphicsPath scenePath = null!;
    private bool disposed;
    public Func<byte[]> CapturePng { private get; set; } = null!;

    public void Initialize(RuntimeContext runtimeContext)
    {
        context = runtimeContext;
        using (FileStream stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "NotoSans-Regular.ttf"))) font = graphics.LoadFont(stream);
        root = new(GensTheme.Create(graphics, font)) { Name = "UiGallery" };
        root.AttachInvalidation(context.Invalidate);
        portraits = new(graphics, Path.Combine(Environment.CurrentDirectory, "artifacts", "sandbox-cache"));
        root.AddChild(BuildGallery());
        Console.WriteLine("UI + Scene2D gallery: animated villa, deterministic portrait, F1 bounds, F2 tree, F3 UI scale, F11 fullscreen, F12 capture, Escape quit.");
        context.SetAnimating(true);
    }

    public void HandleEvent(PlatformEvent platformEvent)
    {
        root.HandleEvent(platformEvent);
        if (platformEvent is PointerMovedEvent moved)
        {
            UiNode? node = root.HoveredNode;
            Point2 world = sceneView.PointerToWorld(new(moved.Position.X, moved.Position.Y));
            inspectorText.Text = (node is null ? "Inspector: no node" : Format(UiInspector.Inspect(node))) + $"  |  world {world.X:F1},{world.Y:F1}  |  {SceneInspector.Describe(sceneView.Scene!)}";
        }
        if (platformEvent is WheelEvent wheel) { sceneView.Scene!.Camera.Zoom *= wheel.Delta.Y > 0 ? 1.1f : .9f; UpdateSceneInspector(); }
        if (platformEvent is KeyboardEvent { IsDown: true, IsRepeat: false } key) HandleKey(key.Key);
    }

    public void Update(PresentationFrame frame)
    {
        sceneView.Advance(frame.Delta);
        if (!sceneView.HasActiveAnimations && !smokeTest) context.SetAnimating(false);
        if (!smokeTest || smokeCaptured || !hasRendered || frame.Elapsed < TimeSpan.FromMilliseconds(250)) return;
        smokeCaptured = true; string path = Path.Combine(Environment.CurrentDirectory, $"engine-sandbox-ui-smoke-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"); File.WriteAllBytes(path, CapturePng()); Console.WriteLine($"UI smoke capture: {path}"); context.SetAnimating(false); context.RequestQuit();
    }

    public void Render(RenderContext render)
    {
        render.Canvas.Clear(new(25, 20, 17)); root.Layout(new(render.LogicalSize.Width, render.LogicalSize.Height)); if (smokeTest && !treePrinted) { treePrinted = true; Console.WriteLine(UiInspector.Tree(root)); }
        root.Render(render.Canvas);
        hasRendered = true;
    }

    public void Shutdown() => Dispose();
    public void Dispose() { if (disposed) return; portraits.Dispose(); sceneSprite.Dispose(); scenePath.Dispose(); font.Dispose(); disposed = true; }

    private Column BuildGallery()
    {
        var shell = new Column { Name = "GalleryShell" };
        var bar = new InkBar();
        var barRow = new Row { Spacing = 18 };
        barRow.AddChild(new TextBlock { Text = "GENS · UI FOUNDATION", TypographyRole = TypographyRole.Inscription, Foreground = new Color(245, 229, 195) });
        barRow.AddChild(new TextBlock { Text = "Retained tree  ·  Logical units  ·  Backend neutral", TypographyRole = TypographyRole.Caption, Foreground = new Color(190, 142, 54), VerticalAlignment = VerticalAlignment.Center });
        bar.Child = barRow; shell.AddChild(bar);

        var diptych = new Diptych { Margin = new(18), Height = 570 };
        var left = new WaxTablet { Margin = new(0, 0, 9, 0) };
        var leftContent = new StackPanel { Spacing = 12 };
        leftContent.AddChild(new TextBlock { Text = "The House of Aemilius", TypographyRole = TypographyRole.Heading });
        leftContent.AddChild(new TextBlock { Text = "A design-system study using invented presentation data. Long inscriptions wrap within their tablet without touching authoritative campaign state.", Wrapping = TextWrapping.Wrap });
        var actions = new Row { Spacing = 10 };
        actions.AddChild(ActionButton("Open Modal", OpenModal));
        actions.AddChild(new Toggle { Name = "ShowLineageToggle", Content = new TextBlock { Text = "Lineage", TypographyRole = TypographyRole.Button, Foreground = new Color(245, 229, 195) } });
        actions.AddChild(new WaxSealButton { Name = "AdvanceSeal", Content = new TextBlock { Text = "+", TypographyRole = TypographyRole.Heading, Foreground = Color.White }, Clicked = () => Console.WriteLine("Wax seal activated.") });
        leftContent.AddChild(actions);
        ResolvedPortrait portrait = portraits.Resolve(SampleVisual(), 128);
        var gallery = new Row { Name = "PortraitGallery", Spacing = 8 };
        for (int variant = 0; variant < 4; variant++)
        {
            ResolvedPortrait item = portraits.Resolve(SampleVisual(variant), 96);
            gallery.AddChild(new CharacterMedallion(item.Image) { Name = $"PortraitVariant{variant + 1}", Width = 72, Height = 72, Semantics = { Label = item.Appearance.AccessibilityDescription } });
        }
        leftContent.AddChild(gallery);
        sceneView = new SceneView { Name = "VillaSceneView", Scene = BuildScene(), Height = 220, Semantics = { Label = "Animated non-authoritative Roman villa courtyard demonstration." } };
        leftContent.AddChild(sceneView);
        left.Child = leftContent; diptych.SetLeft(left);

        var right = new WaxTablet { Margin = new(9, 0, 0, 0) };
        var rightContent = new Column { Spacing = 8 };
        rightContent.AddChild(new TextBlock { Text = "Ledger Preview", TypographyRole = TypographyRole.Heading });
        var list = new Column { Spacing = 4 };
        for (int i = 1; i <= 40; i++) list.AddChild(new TextBlock { Name = $"LedgerRow{i}", Text = $"{i:00}  ·  Estate entry {1000 + i} denarii", TypographyRole = TypographyRole.Ledger });
        rightContent.AddChild(new ScrollView { Name = "LedgerScroll", Height = 315, Content = list, IsFocusable = true });
        rightContent.AddChild(new TextBlock { Name = "PortraitInspector", Text = PortraitInspection(portrait), TypographyRole = TypographyRole.SmallCaption, Wrapping = TextWrapping.Wrap });
        right.Child = rightContent; diptych.SetRight(right); shell.AddChild(diptych);
        inspectorText = new TextBlock { Name = "InspectorReadout", Text = "Inspector: move the pointer across the retained tree", TypographyRole = TypographyRole.SmallCaption, Foreground = new Color(245, 229, 195), Margin = new(20, 0) };
        shell.AddChild(inspectorText); return shell;
    }

    private Scene2D.Scene2D BuildScene()
    {
        using IRenderSurface surface = graphics.CreateOffscreenSurface(new(32, 32));
        using (IRenderFrame frame = surface.BeginFrame()) { frame.Canvas.Clear(new(89, 120, 69)); frame.Canvas.DrawRoundRect(new(4, 4, 24, 24), 12, 12, new(new Color(190, 142, 54))); frame.Present(); }
        using (var png = new MemoryStream(graphics.EncodePng(surface.Capture()))) sceneSprite = graphics.DecodeImage(png);
        scenePath = graphics.CreatePath(); scenePath.MoveTo(new(245, 42)); scenePath.LineTo(new(330, 100)); scenePath.LineTo(new(160, 100)); scenePath.Close();
        var scene = new Scene2D.Scene2D(); scene.Camera.Position = new(250, 100);
        Layer2D background = scene.AddLayer("Background", 0); background.AddChild(new RectangleNode2D { Rectangle = new(0, 0, 500, 200), Color = new(208, 185, 133) });
        Layer2D environment = scene.AddLayer("Environment", 10); environment.AddChild(new RectangleNode2D { Rectangle = new(95, 98, 310, 88), Color = new(178, 137, 86) }); environment.AddChild(new VectorNode2D { Path = scenePath, Fill = new(new Color(129, 57, 42)), Bounds = new(160, 42, 170, 58) });
        for (int i = 0; i < 6; i++) environment.AddChild(new RectangleNode2D { Rectangle = new(125 + i * 48, 112, 18, 74), Color = new(226, 207, 169) });
        var sprite = new Sprite2D { Image = sceneSprite, Size = new(32, 32), Transform = Transform2D.Identity with { Position = new(70, 158) } }; environment.AddChild(sprite);
        Layer2D labels = scene.AddLayer("Labels", 20); labels.AddChild(new TextNode2D { GlyphRun = graphics.ShapeText(font, "Villa courtyard", 15), Origin = new(14, 24), Color = new(61, 41, 32) });
        var player = new AnimationPlayer { LoopMode = LoopMode.Once };
        player.Play(new("courtyard-walk", new IAnimationTrack[] { AnimationPlayer.Point(new[] { new Keyframe<Point2>(TimeSpan.Zero, new(70, 158)), new Keyframe<Point2>(TimeSpan.FromSeconds(2), new(430, 158), Easing.EaseInOut) }, value => sprite.Transform = sprite.Transform with { Position = value }) }));
        scene.AddAnimation(player); return scene;
    }

    private static CharacterVisualState SampleVisual(int variant = 0)
    {
        Height height = new[] { Height.Average, Height.Tall, Height.Diminutive, Height.Tall }[variant % 4];
        Build build = new[] { Build.Average, Build.Muscular, Build.Slight, Build.Heavyset }[variant % 4];
        FacialStructure face = new[] { FacialStructure.Oval, FacialStructure.Square, FacialStructure.Angular, FacialStructure.Round }[variant % 4];
        Complexion complexion = new[] { Complexion.Olive, Complexion.Bronzed, Complexion.Fair, Complexion.Dark }[variant % 4];
        HairColor hair = new[] { HairColor.Auburn, HairColor.Black, HairColor.Blond, HairColor.Gray }[variant % 4];
        HairStyle style = new[] { HairStyle.BoundUp, HairStyle.Cropped, HairStyle.Flowing, HairStyle.ShoulderLength }[variant % 4];
        EyeColor eyes = new[] { EyeColor.Brown, EyeColor.Hazel, EyeColor.Blue, EyeColor.Green }[variant % 4];
        NotableFeature[] features = variant switch { 1 => new[] { NotableFeature.Scar }, 2 => new[] { NotableFeature.Freckled, NotableFeature.Birthmark }, 3 => new[] { NotableFeature.GrayAtTemples }, _ => new[] { NotableFeature.Freckled } };
        var profile = new CharacterVisualProfile { Height = height, Build = build, FacialStructure = face, Complexion = complexion, HairColor = hair, HairStyle = style, EyeColor = eyes, NotableFeatures = features, Portrait = PortraitRecipeGenerator.Generate(height, build, face, complexion, hair, style, eyes, features) };
        return CharacterVisualStateProjector.Project($"sandbox:portrait-{variant + 1}", variant % 2 == 0 ? Sex.Female : Sex.Male, 24 + variant * 17, profile, LegalStatus.RomanCitizen, variant == 1 ? SocialClass.Senatorial : SocialClass.Equestrian, variant == 3 ? DutySlot.Craftsman : null);
    }

    private static string PortraitInspection(ResolvedPortrait portrait) => $"PORTRAIT INSPECTOR · source={portrait.Reference.SourceKind} · seed={SampleVisual().VisualSeed} · age={SampleVisual().AgeBand} · style={portrait.Reference.StyleId} · size={portrait.Reference.PixelSize}px · visual={portrait.Reference.VisualStateHash[..12]} · recipe=v{portrait.Reference.RecipeVersion} · renderer={portrait.Reference.RendererVersion}\n{portrait.Appearance.ShortDescription}";

    private static Button ActionButton(string text, Action action) => new() { Name = text.Replace(" ", string.Empty, StringComparison.Ordinal), Content = new TextBlock { Text = text, TypographyRole = TypographyRole.Button, Foreground = new Color(245, 229, 195) }, Clicked = action };

    private void OpenModal()
    {
        var dialog = new Border { Name = "ModalDialog", Width = 430, Height = 230, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Background = new(245, 229, 195), BorderBrush = new(133, 48, 39), BorderThickness = 4, Padding = new(24), Semantics = { Role = AccessibilityRole.Dialog, Label = "Demonstration dialog" } };
        var content = new Column { Spacing = 14 }; content.AddChild(new TextBlock { Text = "Modal Focus", TypographyRole = TypographyRole.Heading }); content.AddChild(new TextBlock { Text = "Background input is blocked. Tab remains in this focus scope; closing restores the previous focus.", Wrapping = TextWrapping.Wrap }); content.AddChild(ActionButton("Close", root.CloseModal)); dialog.Child = content; root.ShowModal(dialog);
    }

    private void HandleKey(PhysicalKey key)
    {
        switch (key.ScanCode)
        {
            case 41: context.RequestQuit(); break;
            case 58: root.DrawLayoutOutlines = !root.DrawLayoutOutlines; context.Invalidate(); break;
            case 59: Console.WriteLine(UiInspector.Tree(root)); Console.WriteLine(SceneInspector.Tree(sceneView.Scene!)); Console.WriteLine(SceneInspector.Describe(sceneView.Scene!)); break;
            case 60: root.UiScale = root.UiScale >= 1.5f ? 1 : root.UiScale + .25f; root.InvalidateMeasure(); Console.WriteLine($"UI scale: {root.UiScale:P0}"); break;
            case 79: PanScene(20, 0); break;
            case 80: PanScene(-20, 0); break;
            case 81: PanScene(0, 20); break;
            case 82: PanScene(0, -20); break;
            case 68: fullscreen = !fullscreen; context.Window.SetFullscreen(fullscreen); context.Invalidate(); break;
            case 69: string directory = Path.Combine(Environment.CurrentDirectory, "captures"); Directory.CreateDirectory(directory); string path = Path.Combine(directory, $"engine-sandbox-ui-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png"); File.WriteAllBytes(path, CapturePng()); Console.WriteLine($"Captured {path}"); break;
        }
    }

    private void PanScene(float x, float y) { Camera2D camera = sceneView.Scene!.Camera; camera.Position = new(camera.Position.X + x, camera.Position.Y + y); UpdateSceneInspector(); }
    private void UpdateSceneInspector() { inspectorText.Text = SceneInspector.Describe(sceneView.Scene!); context.Invalidate(); }

    private static string Format(UiInspection i) => $"{i.Type}{(i.Name is null ? string.Empty : " #" + i.Name)}  bounds {i.Bounds.X:F0},{i.Bounds.Y:F0} {i.Bounds.Width:F0}×{i.Bounds.Height:F0}  desired {i.DesiredSize.Width:F0}×{i.DesiredSize.Height:F0}  {(i.Focused ? "focused " : string.Empty)}{(i.Hovered ? "hovered " : string.Empty)}children {i.ChildrenCount}  invalid M:{!i.MeasureValid} A:{!i.ArrangeValid} P:{!i.PaintValid}";
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
