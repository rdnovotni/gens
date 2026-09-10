using Gens.Application.Campaign;
using System.Globalization;
using Gens.Accessibility;
using Gens.Accessibility.Windows;
using Gens.Art.Portraits;
using Gens.Art.Diagnostics;
using Gens.Audio;
using System.Collections.Concurrent;
using Gens.Client.Desktop.Settings;
using Gens.Localization;
using Gens.Graphics;
using Gens.Platform;
using Gens.Platform.Sdl;
using Gens.Presentation.Models;
using Gens.Presentation.Visuals;
using Gens.Portraits;
using Gens.Runtime;
using Gens.Scene2D;
using Gens.UI;

namespace Gens.Client.Desktop.App;

/// <summary>Native presentation host. It never stores or reads WorldState.</summary>
public sealed class GensDesktopApplication(IGraphicsBackend graphics, DesktopApplicationController controller, bool smokeTest = false, string? smokeCapturePath = null) : IRuntimeApplication, IDisposable
{
    private RuntimeContext context = null!;
    private UiRoot root = null!;
    private IFontFace font = null!;
    private Border? screenHost;
    private TextBlock? inkName, inkDate, inkTreasury, inkDignitas;
    private bool gameplayMounted, consoleOpen;
    private string consoleInput = string.Empty;
    private bool saveAsPromptOpen;
    private TextField? saveAsField;
    private bool smokeCaptured;
    private bool smokeCapturePending;
    private bool smokeSemanticsValidated;
    private bool disposed;
    private LocalizationService localization = null!;
    private IAccessibilityBridge accessibilityBridge = null!;
    private AccessibilityCoordinator accessibility = null!;
    private readonly DesktopArtServices artServices = new(graphics, controller);
    private readonly Dictionary<string, List<(CharacterMedallion Medallion, CharacterVisualState Visual, int Size)>> portraitBindings = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> generatingPortraits = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<Action> uiActions = new();
    public Func<byte[]>? CapturePng { private get; set; }

    public UiRoot Root => root;
    public DesktopApplicationController Controller => controller;

    public void Initialize(RuntimeContext context)
    {
        this.context = context;
        using FileStream stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "NotoSans-Regular.ttf"));
        font = graphics.LoadFont(stream);
        localization = new("en", controller.Settings.Developer.ConsoleEnabled);
        using (FileStream catalog = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "Localization", "en.json"))) localization.AddJson("en", catalog);
        localization.SetLocale(controller.Settings.Language.Locale);
        root = new(GensTheme.Create(graphics, font, highContrast: controller.Settings.Accessibility.HighContrast)) { Name = "DesktopRoot", UiScale = controller.Settings.Display.UiScale, MotionPolicy = new(controller.Settings.Accessibility.Motion) };
        root.AttachInvalidation(context.Invalidate);
        controller.Audio.ActivityChanged += OnAudioActivityChanged;
        if (OperatingSystem.IsWindows())
        {
            var windowsBridge = new WindowsAccessibilityBridge(() => (context.Window as SdlWindow) is { } sdlWindow ? SdlWindowsMessageHook.GetHwnd(sdlWindow.NativeSdlWindowHandle) : IntPtr.Zero);
            accessibilityBridge = windowsBridge;
            SdlWindowsMessageHook.SetSubscriber(windowsBridge.HandleWindowsMessage);
        }
        else accessibilityBridge = new NullAccessibilityBridge();
        accessibility = new(accessibilityBridge);
        artServices.Coordinator.PortraitUpdated += OnPortraitUpdated;
        Rebuild();
        if (smokeTest) context.SetAnimating(true);
    }

    public void HandleEvent(PlatformEvent platformEvent)
    {
        root.HandleEvent(platformEvent);
        if (platformEvent is TextInputEvent text && consoleOpen) { consoleInput += text.Text; RefreshConsole(); return; }
        if (platformEvent is not KeyboardEvent { IsDown: true, IsRepeat: false } key) return;
        switch (key.Key.ScanCode)
        {
            case 40 when consoleOpen: controller.ExecuteConsoleCommand(consoleInput); consoleInput = string.Empty; RefreshConsole(); break;
            case 42 when consoleOpen: if (consoleInput.Length > 0) consoleInput = consoleInput[..^1]; RefreshConsole(); break;
            case 53 when controller.Settings.Developer.ConsoleEnabled: ToggleConsole(); break;
            case 41 when consoleOpen: ToggleConsole(); break;
            case 41: controller.Back(); Rebuild(); break;
            case 58: root.DrawLayoutOutlines = !root.DrawLayoutOutlines; context.Invalidate(); break;
            case 59: Console.WriteLine(UiInspector.Tree(root)); break;
        }
    }

    public void Update(PresentationFrame frame)
    {
        controller.Audio.Update(frame.Delta);
        controller.AccumulatePlaytime(frame.Delta);
        while (uiActions.TryDequeue(out Action? action)) action();
        if (!smokeTest || smokeCaptured || frame.Elapsed < TimeSpan.FromMilliseconds(200)) return;
        smokeCapturePending = true;
    }
    public void Render(RenderContext context)
    {
        context.Canvas.Clear(new(25, 20, 17)); root.Layout(new(context.LogicalSize.Width, context.LogicalSize.Height));
        if (smokeTest && !smokeSemanticsValidated)
        {
            IReadOnlyList<string> semanticErrors = root.ValidateSemantics();
            if (semanticErrors.Count > 0) throw new InvalidOperationException("Accessibility semantic validation failed: " + string.Join("; ", semanticErrors));
            smokeSemanticsValidated = true;
        }
        accessibility.Synchronize(root); root.Render(context.Canvas);
        if (!smokeCapturePending || smokeCaptured) return;
        smokeCaptured = true;
        string path = string.IsNullOrWhiteSpace(smokeCapturePath) ? Path.Combine(Environment.CurrentDirectory, "native-client-smoke.png") : smokeCapturePath;
        try
        {
            string? directory = Path.GetDirectoryName(path); if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (CapturePng is not null) File.WriteAllBytes(path, CapturePng());
            Console.WriteLine($"Native client smoke capture: {path}");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Native client smoke capture failed: {exception.Message}");
        }
        this.context.SetAnimating(false); this.context.RequestQuit();
    }
    public void Shutdown() => Dispose();
    public void Dispose() { if (disposed) return; artServices.Coordinator.PortraitUpdated -= OnPortraitUpdated; controller.Audio.ActivityChanged -= OnAudioActivityChanged; if (OperatingSystem.IsWindows()) SdlWindowsMessageHook.SetSubscriber(null); artServices.Dispose(); accessibilityBridge.Dispose(); font.Dispose(); disposed = true; }

    private void Rebuild()
    {
        portraitBindings.Clear();
        if (root.Modal is not null) root.CloseModal();
        if (IsGameplay(controller.CurrentScreen))
        {
            if (!gameplayMounted) MountGameplayShell();
            RefreshInkBar();
            screenHost!.Child = BuildGameplayScreen();
            root.Focus.Clear(); root.Focus.Move(false);
        }
        else
        {
            gameplayMounted = false; screenHost = null; root.ClearChildren(); root.AddChild(BuildMenuScreen());
            root.Focus.Clear(); root.Focus.Move(false);
        }
        if (controller.Modal is not null) ShowControllerModal();
        if (controller.QuitRequested) context.RequestQuit();
        context.Invalidate();
    }

    private void MountGameplayShell()
    {
        root.ClearChildren();
        var shell = new Column { Name = "GameplayShell" };
        var bar = new InkBar { Name = "PersistentInkBar" };
        var row = new Row { Spacing = 10 };
        inkName = Text("GENS", TypographyRole.Inscription, light: true); inkName.Width = 150;
        inkDate = Text("", TypographyRole.Caption, light: true); inkDate.Width = 105;
        inkTreasury = Text("", TypographyRole.Caption, light: true); inkTreasury.Width = 160;
        inkDignitas = Text("", TypographyRole.Caption, light: true); inkDignitas.Width = 100;
        row.AddChild(inkName); row.AddChild(inkDate); row.AddChild(inkTreasury); row.AddChild(inkDignitas);
        row.AddChild(NavButton(L("nav.household"), ScreenId.HouseholdRoster)); row.AddChild(NavButton(L("nav.estate"), ScreenId.EstateSettlement)); row.AddChild(NavButton(L("nav.report"), ScreenId.MonthlyReport));
        row.AddChild(Button(L("campaign.advance"), () => Run(controller.AdvanceMonth))); row.AddChild(Button(L("common.save"), () => Run(controller.Save))); row.AddChild(Button(L("menu.save_browser"), () => Navigate(ScreenId.SaveBrowser), "GameplaySaveBrowser")); row.AddChild(Button(L("common.menu"), () => Run(controller.RequestMainMenu)));
        bar.Child = row; shell.AddChild(bar);
        screenHost = new Border { Name = "ScreenHost", Margin = new(18), Padding = new(2), Height = Math.Max(250, 620 / root.UiScale - 20), Background = new(41, 31, 25), ClipToBounds = true };
        shell.AddChild(screenHost); root.AddChild(shell); gameplayMounted = true;
    }

    private ScrollView BuildMenuScreen() => controller.CurrentScreen switch
    {
        ScreenId.MainMenu => BuildMainMenu(),
        ScreenId.NewGameSetup => BuildNewGame(),
        ScreenId.Settings => BuildSettings(),
        ScreenId.Credits => BuildCredits(),
        ScreenId.SaveBrowser => BuildSaveBrowser(),
        _ => BuildMainMenu(),
    };

    private ScrollView BuildMainMenu()
    {
        var tablet = Tablet("MainMenu", 540, 580); var c = Content(tablet);
        c.AddChild(Text(L("app.title").ToUpperInvariant(), TypographyRole.Title)); c.AddChild(Text(L("app.tagline"), TypographyRole.Inscription)); c.AddChild(new Spacer(height: 24));
        c.AddChild(Button(L("menu.new_game"), () => Navigate(ScreenId.NewGameSetup), "MainMenuNewGame"));
        Button load = Button(L("menu.load_game"), () => Run(controller.Load), "MainMenuLoad"); load.IsEnabled = controller.HasSave; c.AddChild(load);
        c.AddChild(Button(L("menu.save_browser"), () => Navigate(ScreenId.SaveBrowser), "MainMenuSaveBrowser"));
        c.AddChild(Button(L("menu.settings"), () => Navigate(ScreenId.Settings))); c.AddChild(Button(L("menu.credits"), () => Navigate(ScreenId.Credits))); c.AddChild(Button(L("menu.quit"), () => Run(controller.RequestQuit)));
        return Center(tablet);
    }

    private ScrollView BuildNewGame()
    {
        string region = "latium", difficulty = "standard";
        var tablet = Tablet("NewGameSetup", 760, 620); var c = Content(tablet);
        c.AddChild(Text(L("campaign.new"), TypographyRole.Title)); c.AddChild(Text(L("campaign.region"), TypographyRole.Heading));
        var regions = new Row { Spacing = 8 }; regions.AddChild(Button("Latium", () => region = "latium")); regions.AddChild(Button("Campania", () => region = "campania")); regions.AddChild(Button("Cisalpina", () => region = "cisalpina")); c.AddChild(regions);
        c.AddChild(Text(L("campaign.difficulty"), TypographyRole.Heading)); var diffs = new Row { Spacing = 8 }; diffs.AddChild(Button(L("campaign.difficulty.standard"), () => difficulty = "standard")); diffs.AddChild(Button(L("campaign.difficulty.hard"), () => difficulty = "hard")); diffs.AddChild(Button(L("campaign.difficulty.relaxed"), () => difficulty = "relaxed")); c.AddChild(diffs);
        c.AddChild(Text(L("campaign.seed"), TypographyRole.Caption));
        c.AddChild(Button(L("campaign.begin"), () => Run(() => controller.StartNew(region, difficulty)), "BeginCampaign")); c.AddChild(Button(L("common.back"), () => Navigate(ScreenId.MainMenu)));
        return Center(tablet);
    }

    private ScrollView BuildSettings()
    {
        var tablet = Tablet("Settings", 700, 1050); var c = Content(tablet); DesktopSettings s = controller.Settings;
        c.AddChild(Text(L("settings.title"), TypographyRole.Title)); c.AddChild(Text(L("settings.display"), TypographyRole.Heading)); c.AddChild(Text(L("settings.ui_scale", ("scale", s.Display.UiScale.ToString("P0", CultureInfo.CurrentCulture))), TypographyRole.Body));
        var scales = new Row { Spacing = 6 }; foreach (float scale in new[] { 1f, 1.25f, 1.5f, 1.75f, 2f }) scales.AddChild(Button($"{scale:P0}", () => { controller.SetUiScale(scale); root.UiScale = scale; Rebuild(); })); c.AddChild(scales);
        c.AddChild(Text(L("settings.audio"), TypographyRole.Heading));
        foreach ((AudioBus bus, float value, string key) in new[] { (AudioBus.Master, s.Audio.MasterVolume, "settings.master"), (AudioBus.Music, s.Audio.MusicVolume, "settings.music"), (AudioBus.Ambience, s.Audio.AmbienceVolume, "settings.ambience"), (AudioBus.Effects, s.Audio.EffectsVolume, "settings.effects"), (AudioBus.UI, s.Audio.UiVolume, "settings.ui") })
        { var audioRow = new Row { Spacing = 6 }; string busName = L(key); audioRow.AddChild(Text($"{busName}: {value:P0}", TypographyRole.Body)); Button down = Button("−", () => { controller.SetAudioVolume(bus, value - .1f); Rebuild(); }, $"{bus}Down"); down.Semantics.Label = $"Decrease {busName} volume"; audioRow.AddChild(down); Button up = Button("+", () => { controller.SetAudioVolume(bus, value + .1f); Rebuild(); }, $"{bus}Up"); up.Semantics.Label = $"Increase {busName} volume"; audioRow.AddChild(up); c.AddChild(audioRow); }
        c.AddChild(Toggle(L("settings.mute"), s.Audio.Muted, controller.SetAudioMuted));
        c.AddChild(Text(L("settings.accessibility"), TypographyRole.Heading));
        c.AddChild(Toggle(L("settings.reduced_motion"), s.Accessibility.ReducedMotion, value => { controller.SetReducedMotion(value); root.MotionPolicy = new(controller.Settings.Accessibility.Motion); }));
        c.AddChild(Toggle(L("settings.high_contrast"), s.Accessibility.HighContrast, value => { controller.SetHighContrast(value); root.ApplyTheme(GensTheme.Create(graphics, font, highContrast: value)); Rebuild(); }));
        c.AddChild(Text(L("settings.language"), TypographyRole.Heading));
        c.AddChild(Button(L("settings.english"), () => { controller.SetLocale("en"); localization.SetLocale("en"); Rebuild(); }));
        if (s.Developer.ConsoleEnabled) c.AddChild(Button(L("settings.pseudo"), () => { controller.SetLocale("qps-ploc"); localization.SetLocale("qps-ploc"); Rebuild(); }));
        c.AddChild(Toggle(L("settings.developer_console"), s.Developer.ConsoleEnabled, controller.SetConsoleEnabled));
        c.AddChild(Text(L("settings.privacy"), TypographyRole.Heading));
        c.AddChild(Toggle(L("settings.telemetry"), s.Privacy.UsageTelemetry == ConsentState.Granted, controller.SetUsageTelemetryConsent));
        c.AddChild(Toggle(L("settings.crash_reports"), s.Privacy.CrashReports == ConsentState.Granted, controller.SetCrashReportConsent));
        c.AddChild(Text(L("settings.privacy_notice"), TypographyRole.Caption));
        var diagnosticActions = new Row { Spacing = 8 };
        diagnosticActions.AddChild(Button(L("settings.export_diagnostics"), () => Run(() => { controller.ExportDiagnostics(); })));
        diagnosticActions.AddChild(Button(L("settings.delete_diagnostics"), () => Run(controller.DeleteLocalDiagnostics)));
        c.AddChild(diagnosticActions);
        c.AddChild(Text(L("settings.crash_count", ("count", controller.PendingCrashReportCount.ToString(CultureInfo.CurrentCulture))), TypographyRole.SmallCaption));
        c.AddChild(Text(L("settings.art"), TypographyRole.Heading));
        c.AddChild(Toggle(L("settings.art_enable"), s.Art.AiGenerationEnabled, value => { controller.SetAiArtEnabled(value); Rebuild(); }));
        c.AddChild(Text(L("settings.art_notice"), TypographyRole.Caption));
        if (s.Developer.ConsoleEnabled)
        {
            ArtDiagnosticsSnapshot diagnostics = artServices.Diagnostics.Capture(s.Art);
            c.AddChild(Text($"Art diagnostics · provider={diagnostics.SelectedProvider} available={diagnostics.ProviderAvailable} queued={diagnostics.Queue.Queued} running={diagnostics.Queue.Running} completed={diagnostics.Queue.Completed} failed={diagnostics.Queue.Failed} cache hits={diagnostics.Cache.Hits} misses={diagnostics.Cache.Misses} bytes={diagnostics.Cache.ObjectBytes}", TypographyRole.SmallCaption));
        }
        c.AddChild(Text(L("settings.audio_backend", ("backend", controller.Audio.BackendName + (controller.Audio.IsOutputAvailable ? string.Empty : " (silent fallback)"))), TypographyRole.Caption)); c.AddChild(Button(L("common.back"), () => Navigate(ScreenId.MainMenu)));
        return Center(tablet);
    }

    private ScrollView BuildCredits()
    {
        var tablet = Tablet("Credits", 620, 500); var c = Content(tablet); c.AddChild(Text(L("credits.title"), TypographyRole.Title));
        c.AddChild(Text($"Gens\nVersion: {Gens.Client.Desktop.Diagnostics.ReleaseMetadata.Current.Display}\nDesign and development: the Gens contributors\nNative runtime: SDL3, SkiaSharp, HarfBuzz\nFont: Noto Sans (SIL Open Font License)", TypographyRole.Body));
        c.AddChild(Button(L("common.back"), () => Navigate(ScreenId.MainMenu))); return Center(tablet);
    }

    private ScrollView BuildSaveBrowser()
    {
        SaveBrowserModel vm = controller.SaveBrowser(); var tablet = Tablet("SaveBrowser", null, 610); var c = Content(tablet); c.AddChild(Heading(L("save_browser.title"), TypographyRole.Title));

        var saveAsRow = new Row { Name = "SaveAsRow", Spacing = 8 };
        if (controller.HasActiveCampaign)
        {
            if (saveAsPromptOpen)
            {
                saveAsField = new TextField { Name = "SaveAsField", Width = 260, MaxLength = 64, Placeholder = L("save_browser.name_placeholder"), Submitted = () => Run(() => { controller.SaveAs(saveAsField!.Text); saveAsPromptOpen = false; }), Semantics = { Label = L("save_browser.name_placeholder") } };
                saveAsRow.AddChild(saveAsField);
                saveAsRow.AddChild(Button(L("common.confirm"), () => Run(() => { controller.SaveAs(saveAsField!.Text); saveAsPromptOpen = false; }), "SaveAsConfirm"));
                saveAsRow.AddChild(Button(L("common.cancel"), () => Run(() => saveAsPromptOpen = false), "SaveAsCancel"));
            }
            else saveAsRow.AddChild(Button(L("save_browser.save_as"), () => Run(() => saveAsPromptOpen = true), "SaveAsOpen"));
        }
        c.AddChild(saveAsRow);

        var rows = new Column { Spacing = 8 };
        foreach (SaveSlotRowModel slot in vm.Slots)
        {
            var row = new Row { Name = $"SaveSlot-{slot.SlotId}", Spacing = 10 };
            var labels = new Column(); labels.AddChild(Text(slot.DisplayName, TypographyRole.Button, light: true)); labels.AddChild(Text(slot.LastSavedDisplay, TypographyRole.SmallCaption, light: true)); if (!string.IsNullOrEmpty(slot.PlaytimeDisplay)) labels.AddChild(Text(slot.PlaytimeDisplay, TypographyRole.SmallCaption, light: true)); row.AddChild(labels);
            Button loadButton = Button(L("save_browser.load"), () => Run(() => controller.LoadSlot(slot.SlotId)), $"LoadSlot-{slot.SlotId}"); loadButton.IsEnabled = slot.Exists; row.AddChild(loadButton);
            if (!slot.IsQuicksave) row.AddChild(Button(L("save_browser.delete"), () => Run(() => controller.RequestDeleteSlot(slot.SlotId)), $"DeleteSlot-{slot.SlotId}"));
            rows.AddChild(row);
        }
        c.AddChild(new ScrollView { Name = "SaveBrowserScroll", Height = 380, Content = rows, IsFocusable = true });
        c.AddChild(Button(L("common.back"), () => Run(() => { saveAsPromptOpen = false; controller.Navigate(controller.HasActiveCampaign ? ScreenId.HouseholdRoster : ScreenId.MainMenu); })));
        return Screen(tablet);
    }

    private ScrollView BuildGameplayScreen() => controller.CurrentScreen switch
    {
        ScreenId.HouseholdRoster => BuildRoster(),
        ScreenId.CharacterDetail => BuildCharacter(),
        ScreenId.EstateSettlement => BuildEstate(),
        ScreenId.MonthlyReport => BuildReport(),
        _ => BuildRoster(),
    };

    private ScrollView BuildRoster()
    {
        HouseholdRosterModel vm = controller.Roster(); var tablet = Tablet("HouseholdRoster", null, 610); var c = Content(tablet); c.AddChild(Heading(L("screen.household"), TypographyRole.Title));
        var rows = new Column { Spacing = 5 };
        foreach (RosterRowModel member in vm.Rows)
        {
            ResolvedPortrait portrait = artServices.Portraits.Resolve(member.Visual, 128);
            var medallion = new CharacterMedallion(portrait.Image) { Width = 58, Height = 58, Semantics = { Label = $"Portrait of {member.Name}", Description = portrait.Appearance.AccessibilityDescription } }; BindPortrait(member.CharacterId, medallion, member.Visual, 128);
            var row = new Row { Spacing = 12 }; row.AddChild(medallion);
            var labels = new Column(); labels.AddChild(Text(member.Name, TypographyRole.Button, light: true)); labels.AddChild(Text(member.Subtitle, TypographyRole.SmallCaption, light: true)); row.AddChild(labels);
            rows.AddChild(new Button { Name = $"Character-{member.CharacterId}", Content = row, Clicked = () => Run(() => controller.OpenCharacter(member.CharacterId)), Semantics = { Label = $"Open details for {member.Name}" } });
        }
        c.AddChild(new ScrollView { Name = "RosterScroll", Height = 500, Content = rows, IsFocusable = true }); return Screen(tablet);
    }

    private ScrollView BuildCharacter()
    {
        CharacterDetailModel vm = controller.Character(); var tablet = Tablet("CharacterDetail", null, 610); var c = Content(tablet); c.AddChild(Button(L("screen.character_back"), () => { controller.Back(); Rebuild(); }));
        ResolvedPortrait portrait = artServices.Portraits.Resolve(vm.Visual, 256);
        var header = new Row { Spacing = 18 }; var medal = new CharacterMedallion(portrait.Image) { Width = 128, Height = 128 }; medal.Semantics.Label = $"Portrait of {vm.Name}"; medal.Semantics.Description = portrait.Appearance.AccessibilityDescription; BindPortrait(vm.CharacterId, medal, vm.Visual, 256); header.AddChild(medal);
        var identity = new Column(); identity.AddChild(Text(vm.Name, TypographyRole.Title)); identity.AddChild(Text(vm.Subtitle, TypographyRole.Caption)); header.AddChild(identity); c.AddChild(header);
        c.AddChild(Text(vm.Appearance.DetailedDescription, TypographyRole.Body));
        if (controller.Settings.Art.AiGenerationEnabled)
        {
            var artActions = new Row { Spacing = 8 };
            GeneratedPortraitSelection? selected = artServices.Coordinator.GetCurrent(vm.Visual);
            artActions.AddChild(Button(selected is null ? L("portrait.generate") : L("portrait.regenerate"), () => _ = GeneratePortraitAsync(vm, selected is not null)));
            if (selected is not null) artActions.AddChild(Button(L("portrait.procedural"), () => artServices.Coordinator.UseProcedural(vm.CharacterId)));
            if (generatingPortraits.ContainsKey(vm.CharacterId)) artActions.AddChild(Button(L("common.cancel"), () => artServices.Coordinator.Cancel(vm.CharacterId)));
            c.AddChild(artActions);
        }
        c.AddChild(Stats("Attributes", vm.Attributes)); c.AddChild(Stats("Skills", vm.Skills)); c.AddChild(Stats("Condition", vm.Condition)); return Screen(tablet);
    }

    private ScrollView BuildEstate()
    {
        EstateSettlementModel vm = controller.Estate(); var tablet = Tablet("EstateSettlement", null, 610); var c = Content(tablet); c.AddChild(Heading(L("screen.estate"), TypographyRole.Title)); c.AddChild(Text($"Settlement stage: {vm.SettlementStage}", TypographyRole.Heading));
        c.AddChild(BuildEstateScene(vm));
        var actions = new Row { Spacing = 10 }; actions.AddChild(Button(L("estate.change_rites"), () => Run(() => controller.RequestAction(CampaignHouseholdAction.CycleRitesBudget)))); string festival = L("estate.festival"); actions.AddChild(new WaxSealButton { Content = Text(festival, TypographyRole.Button, light: true), Clicked = () => Run(() => controller.RequestAction(CampaignHouseholdAction.FundFestival)), Semantics = { Label = festival } }); c.AddChild(actions);
        var holdings = new Column { Spacing = 8 }; foreach (HoldingModel h in vm.Holdings) { holdings.AddChild(Text(h.Label, TypographyRole.Heading)); foreach (BuildingModel b in h.Buildings) holdings.AddChild(new StatRow { Label = b.Label, Value = b.Condition }); }
        if (vm.Holdings.Count == 0) holdings.AddChild(Text(L("estate.no_holdings"), TypographyRole.Body));
        c.AddChild(new ScrollView { Name = "EstateScroll", Height = 430, Content = holdings, IsFocusable = true }); return Screen(tablet);
    }

    private static SceneView BuildEstateScene(EstateSettlementModel model)
    {
        var scene = new Scene2D.Scene2D(); scene.Camera.Position = new(300, 100);
        Layer2D background = scene.AddLayer("Background", 0); background.AddChild(new RectangleNode2D { Rectangle = new(0, 0, 600, 200), Color = new(206, 184, 132) });
        Layer2D environment = scene.AddLayer("Environment", 10); environment.AddChild(new RectangleNode2D { Rectangle = new(0, 145, 600, 55), Color = new(111, 119, 61) });
        int holdingIndex = 0;
        foreach (HoldingModel holding in model.Holdings)
        {
            float x = 40 + holdingIndex++ * 170; environment.AddChild(new RectangleNode2D { Rectangle = new(x, 70, 135, 78), Color = new(176, 139, 91), CornerRadius = 4 });
            int buildingIndex = 0; foreach (BuildingModel _ in holding.Buildings.Take(4)) environment.AddChild(new RectangleNode2D { Rectangle = new(x + 12 + buildingIndex++ * 28, 115, 20, 30), Color = new(108, 67, 45) });
        }
        return new SceneView { Name = "EstateScenePreview", Scene = scene, Height = 200, Semantics = { Label = $"Non-authoritative estate preview with {model.Holdings.Count} holdings." } };
    }

    private ScrollView BuildReport()
    {
        MonthlyReportModel vm = controller.Report(); var tablet = Tablet("MonthlyReport", null, 610); var c = Content(tablet); c.AddChild(Heading(L("screen.report", ("date", vm.Date)), TypographyRole.Title));
        var summary = new Row { Spacing = 20 }; summary.AddChild(Text($"Income  {vm.Income}", TypographyRole.Ledger)); summary.AddChild(Text($"Expenses  {vm.Expenses}", TypographyRole.Ledger)); summary.AddChild(Text($"Net  {vm.Net}", TypographyRole.Ledger)); c.AddChild(summary);
        var lines = new Column { Spacing = 8 }; foreach (string s in vm.AutomationSummaries) lines.AddChild(Text(s, TypographyRole.Body)); foreach (ReportHeadlineModel h in vm.Headlines) lines.AddChild(Text(h.Label, TypographyRole.Body)); if (vm.Headlines.Count + vm.AutomationSummaries.Count == 0) lines.AddChild(Text(L("report.no_events"), TypographyRole.Body));
        c.AddChild(new ScrollView { Name = "ReportScroll", Height = 470, Content = lines, IsFocusable = true }); return Screen(tablet);
    }

    private void ShowControllerModal()
    {
        ModalState modal = controller.Modal!; var dialog = new Border { Name = modal.Kind == ModalKind.WaxSeal ? "WaxSealConfirmation" : "ConfirmationDialog", Width = 520, Height = 300, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Background = new(245, 229, 195), BorderBrush = new(133, 48, 39), BorderThickness = 4, Padding = new(24), Semantics = { Role = AccessibilityRole.Dialog, Label = modal.Title } };
        var c = new Column { Spacing = 14 }; c.AddChild(Text(modal.Title, TypographyRole.Heading)); c.AddChild(Text(modal.Body, TypographyRole.Body)); var actions = new Row { Spacing = 12 };
        if (modal.Kind == ModalKind.WaxSeal) { string seal = L("dialog.seal"); actions.AddChild(new WaxSealButton { Content = Text(seal, TypographyRole.Button, light: true), Clicked = () => { controller.ConfirmModal(); Rebuild(); }, Semantics = { Label = seal } }); }
        else if (modal.Kind == ModalKind.PrivacyConsent)
        {
            actions.AddChild(Button(L("privacy.allow"), () => { controller.AcceptPrivacyConsent(); Rebuild(); }));
            actions.AddChild(Button(L("privacy.keep_off"), () => { controller.DeclinePrivacyConsent(); Rebuild(); }));
        }
        else if (modal.Kind == ModalKind.CrashRecovery)
        {
            actions.AddChild(Button(L("privacy.open_settings"), () => { controller.ConfirmModal(); controller.Navigate(ScreenId.Settings); Rebuild(); }));
            actions.AddChild(Button(L("privacy.later"), () => { controller.ConfirmModal(); Rebuild(); }));
        }
        else if (modal.Kind == ModalKind.SaveRecovery)
        {
            string slotId = modal.SlotId ?? string.Empty;
            actions.AddChild(Button(L("save_recovery.retry"), () => Run(() => controller.RetryLoad(slotId))));
            actions.AddChild(Button(L("save_recovery.choose_different"), () => Run(() => { controller.DismissSaveRecovery(); controller.Navigate(ScreenId.SaveBrowser); })));
            actions.AddChild(Button(L("save_recovery.delete"), () => Run(() => controller.DeleteRecoverySlot(slotId))));
        }
        else actions.AddChild(Button(modal.Kind == ModalKind.Information ? "OK" : L("common.confirm"), () => { controller.ConfirmModal(); Rebuild(); }));
        if (modal.Kind is ModalKind.Confirmation or ModalKind.WaxSeal) actions.AddChild(Button(L("common.cancel"), () => { controller.CancelModal(); root.CloseModal(); context.Invalidate(); })); c.AddChild(actions); dialog.Child = c; root.ShowModal(dialog);
    }

    private void ToggleConsole()
    {
        if (consoleOpen) { consoleOpen = false; context.Window.StopTextInput(); root.CloseModal(); context.Invalidate(); }
        else { consoleOpen = true; context.Window.StartTextInput(); RefreshConsole(); }
    }

    private void RefreshConsole()
    {
        if (root.Modal is not null) root.CloseModal();
        var panel = new Border { Name = "DeveloperConsole", Width = 920, Height = 520, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Background = new(20, 18, 16), BorderBrush = new(190, 142, 54), BorderThickness = 2, Padding = new(18), Semantics = { Role = AccessibilityRole.Dialog, Label = "Developer console" } };
        var c = new Column { Spacing = 8 }; c.AddChild(Text(L("developer.console_title"), TypographyRole.Inscription, light: true));
        var output = new Column { Spacing = 3 }; foreach (string line in controller.Logs.TakeLast(18)) output.AddChild(Text(line, TypographyRole.SmallCaption, light: true)); c.AddChild(new ScrollView { Height = 400, Content = output }); c.AddChild(Text($"> {consoleInput}▌", TypographyRole.Ledger, light: true)); panel.Child = c; root.ShowModal(panel); context.Invalidate();
    }

    private void RefreshInkBar() { InkBarModel vm = controller.InkBar(); inkName!.Text = vm.GensName; inkDate!.Text = vm.Date; inkTreasury!.Text = vm.Treasury; inkDignitas!.Text = vm.Dignitas; }
    private void BindPortrait(string subjectId, CharacterMedallion medallion, CharacterVisualState visual, int size)
    {
        if (!portraitBindings.TryGetValue(subjectId, out List<(CharacterMedallion, CharacterVisualState, int)>? bindings)) { bindings = []; portraitBindings.Add(subjectId, bindings); }
        bindings.Add((medallion, visual, size));
    }
    private void OnPortraitUpdated(object? sender, GeneratedPortraitUpdatedEventArgs args)
    {
        uiActions.Enqueue(() =>
        {
            if (portraitBindings.TryGetValue(args.SubjectId, out List<(CharacterMedallion Medallion, CharacterVisualState Visual, int Size)>? bindings))
                foreach ((CharacterMedallion medallion, CharacterVisualState visual, int size) in bindings) medallion.Portrait = artServices.Portraits.Resolve(visual, size).Image;
        });
        context.Invalidate();
    }
    private void OnAudioActivityChanged(bool active) => context.SetAnimating(active || smokeTest || !generatingPortraits.IsEmpty);
    private async Task GeneratePortraitAsync(CharacterDetailModel model, bool regenerate)
    {
        if (!generatingPortraits.TryAdd(model.CharacterId, 0)) return;
        context.SetAnimating(true);
        Rebuild();
        try { await artServices.Coordinator.RequestAsync(model.Visual, model.Appearance, controller.Settings.Art, regenerate: regenerate).ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        finally
        {
            uiActions.Enqueue(() => { generatingPortraits.TryRemove(model.CharacterId, out _); if (generatingPortraits.IsEmpty && !smokeTest) context.SetAnimating(false); });
            context.Invalidate();
        }
    }
    private void Navigate(ScreenId id) { controller.Navigate(id); Rebuild(); }
    private void Run(Action action) { action(); Rebuild(); }
    private Button NavButton(string label, ScreenId id) => Button(label, () => Navigate(id), $"Nav{id}");
    private static Button Button(string label, Action click, string? name = null) => new() { Name = name ?? label.Replace(" ", string.Empty, StringComparison.Ordinal), Content = Text(label, TypographyRole.Button, light: true), Clicked = click, Semantics = { Label = label } };
    private static Toggle Toggle(string label, bool value, Action<bool> changed) { var t = new Toggle { Content = Text(label, TypographyRole.Button, light: true), Changed = changed, Semantics = { Label = label } }; t.SetChecked(value); return t; }
    private static TextBlock Text(string value, TypographyRole role, bool light = false) => new() { Text = value, TypographyRole = role, Wrapping = TextWrapping.Wrap, Foreground = light ? new Color(245, 229, 195) : null };
    private static TextBlock Heading(string value, TypographyRole role) { TextBlock text = Text(value, role); text.Semantics.Role = AccessibilityRole.Heading; text.Semantics.Label = value; return text; }
    private string L(string key, params (string Key, object? Value)[] arguments) => localization.Get(key, arguments.Length == 0 ? null : arguments.ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal));
    private static WaxTablet Tablet(string name, float? width, float height) => new() { Name = name, Width = width, Height = height, Margin = new(18), HorizontalAlignment = width is null ? HorizontalAlignment.Stretch : HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private static Column Content(WaxTablet tablet) { var c = new Column { Spacing = 12 }; tablet.Child = c; return c; }
    private static ScrollView Center(UiNode child) { var overlay = new Overlay(); overlay.AddChild(child); return new ScrollView { Content = overlay, IsFocusable = true }; }
    private static ScrollView Screen(WaxTablet tablet) => new() { Content = tablet, IsFocusable = true };
    private static Column Stats(string title, IReadOnlyList<StatModel> stats) { var c = new Column { Spacing = 3 }; c.AddChild(Text(title, TypographyRole.Heading)); foreach (StatModel stat in stats) c.AddChild(new StatRow { Label = stat.Label, Value = stat.Value }); return c; }
    private static bool IsGameplay(ScreenId id) => id is ScreenId.HouseholdRoster or ScreenId.CharacterDetail or ScreenId.EstateSettlement or ScreenId.MonthlyReport;
}
