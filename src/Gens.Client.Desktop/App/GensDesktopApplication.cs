using Gens.Application.Campaign;
using Gens.Client.Desktop.Settings;
using Gens.Graphics;
using Gens.Platform;
using Gens.Presentation.Models;
using Gens.Runtime;
using Gens.UI;

namespace Gens.Client.Desktop.App;

/// <summary>Native presentation host. It never stores or reads WorldState.</summary>
public sealed class GensDesktopApplication(IGraphicsBackend graphics, DesktopApplicationController controller, bool smokeTest = false, string? smokeCapturePath = null) : IRuntimeApplication
{
    private RuntimeContext context = null!;
    private UiRoot root = null!;
    private IFontFace font = null!;
    private Border? screenHost;
    private TextBlock? inkName, inkDate, inkTreasury, inkDignitas;
    private bool gameplayMounted, consoleOpen;
    private string consoleInput = string.Empty;
    private bool smokeCaptured;
    public Func<byte[]>? CapturePng { private get; set; }

    public UiRoot Root => root;
    public DesktopApplicationController Controller => controller;

    public void Initialize(RuntimeContext context)
    {
        this.context = context;
        using FileStream stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "NotoSans-Regular.ttf"));
        font = graphics.LoadFont(stream);
        root = new(GensTheme.Create(graphics, font)) { Name = "DesktopRoot", UiScale = controller.Settings.Display.UiScale };
        root.AttachInvalidation(context.Invalidate);
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
        if (!smokeTest || smokeCaptured || frame.Elapsed < TimeSpan.FromMilliseconds(200)) return;
        smokeCaptured = true;
        string path = string.IsNullOrWhiteSpace(smokeCapturePath) ? Path.Combine(Environment.CurrentDirectory, "native-client-smoke.png") : smokeCapturePath;
        string? directory = Path.GetDirectoryName(path); if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        if (CapturePng is not null) File.WriteAllBytes(path, CapturePng());
        Console.WriteLine($"Native client smoke capture: {path}");
        context.SetAnimating(false); context.RequestQuit();
    }
    public void Render(RenderContext context) { context.Canvas.Clear(new(25, 20, 17)); root.Layout(new(context.LogicalSize.Width, context.LogicalSize.Height)); root.Render(context.Canvas); }
    public void Shutdown() { font.Dispose(); }

    private void Rebuild()
    {
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
        row.AddChild(NavButton("Household", ScreenId.HouseholdRoster)); row.AddChild(NavButton("Estate", ScreenId.EstateSettlement)); row.AddChild(NavButton("Report", ScreenId.MonthlyReport));
        row.AddChild(Button("Advance", () => Run(controller.AdvanceMonth))); row.AddChild(Button("Save", () => Run(controller.Save))); row.AddChild(Button("Menu", () => Run(controller.RequestMainMenu)));
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
        _ => BuildMainMenu(),
    };

    private ScrollView BuildMainMenu()
    {
        var tablet = Tablet("MainMenu", 540, 580); var c = Content(tablet);
        c.AddChild(Text("GENS", TypographyRole.Title)); c.AddChild(Text("A household through the ages", TypographyRole.Inscription)); c.AddChild(new Spacer(height: 24));
        c.AddChild(Button("New Game", () => Navigate(ScreenId.NewGameSetup), "MainMenuNewGame"));
        Button load = Button("Load Game", () => Run(controller.Load), "MainMenuLoad"); load.IsEnabled = controller.HasSave; c.AddChild(load);
        c.AddChild(Button("Settings", () => Navigate(ScreenId.Settings))); c.AddChild(Button("Credits", () => Navigate(ScreenId.Credits))); c.AddChild(Button("Quit", () => Run(controller.RequestQuit)));
        return Center(tablet);
    }

    private ScrollView BuildNewGame()
    {
        string region = "latium", difficulty = "standard";
        var tablet = Tablet("NewGameSetup", 760, 620); var c = Content(tablet);
        c.AddChild(Text("New Campaign", TypographyRole.Title)); c.AddChild(Text("Region", TypographyRole.Heading));
        var regions = new Row { Spacing = 8 }; regions.AddChild(Button("Latium", () => region = "latium")); regions.AddChild(Button("Campania", () => region = "campania")); regions.AddChild(Button("Cisalpina", () => region = "cisalpina")); c.AddChild(regions);
        c.AddChild(Text("Difficulty", TypographyRole.Heading)); var diffs = new Row { Spacing = 8 }; diffs.AddChild(Button("Standard", () => difficulty = "standard")); diffs.AddChild(Button("Hard", () => difficulty = "hard")); diffs.AddChild(Button("Relaxed", () => difficulty = "relaxed")); c.AddChild(diffs);
        c.AddChild(Text("Campaign seed: 1 · deterministic bootstrap", TypographyRole.Caption));
        c.AddChild(Button("Begin Campaign", () => Run(() => controller.StartNew(region, difficulty)), "BeginCampaign")); c.AddChild(Button("Back", () => Navigate(ScreenId.MainMenu)));
        return Center(tablet);
    }

    private ScrollView BuildSettings()
    {
        var tablet = Tablet("Settings", 650, 610); var c = Content(tablet); DesktopSettings s = controller.Settings;
        c.AddChild(Text("Settings", TypographyRole.Title)); c.AddChild(Text($"UI scale: {s.Display.UiScale:P0}", TypographyRole.Heading));
        var scales = new Row { Spacing = 6 }; foreach (float scale in new[] { 1f, 1.25f, 1.5f, 1.75f, 2f }) scales.AddChild(Button($"{scale:P0}", () => { controller.SetUiScale(scale); root.UiScale = scale; Rebuild(); })); c.AddChild(scales);
        c.AddChild(Toggle("Reduced motion", s.Accessibility.ReducedMotion, controller.SetReducedMotion));
        c.AddChild(Toggle("Developer console (backquote)", s.Developer.ConsoleEnabled, controller.SetConsoleEnabled));
        c.AddChild(Text("Audio is not yet implemented; no inert volume control is shown.", TypographyRole.Caption)); c.AddChild(Button("Back", () => Navigate(ScreenId.MainMenu)));
        return Center(tablet);
    }

    private ScrollView BuildCredits()
    {
        var tablet = Tablet("Credits", 620, 500); var c = Content(tablet); c.AddChild(Text("Credits", TypographyRole.Title));
        c.AddChild(Text("Gens\nDesign and development: the Gens contributors\nNative runtime: SDL3, SkiaSharp, HarfBuzz\nFont: Noto Sans (SIL Open Font License)", TypographyRole.Body));
        c.AddChild(Button("Back", () => Navigate(ScreenId.MainMenu))); return Center(tablet);
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
        HouseholdRosterModel vm = controller.Roster(); var tablet = Tablet("HouseholdRoster", null, 610); var c = Content(tablet); c.AddChild(Text("Household Roster", TypographyRole.Title));
        var rows = new Column { Spacing = 5 }; foreach (RosterRowModel member in vm.Rows) rows.AddChild(Button($"{member.Monogram}   {member.Name}\n      {member.Subtitle}", () => Run(() => controller.OpenCharacter(member.CharacterId)), $"Character-{member.CharacterId}"));
        c.AddChild(new ScrollView { Name = "RosterScroll", Height = 500, Content = rows, IsFocusable = true }); return Screen(tablet);
    }

    private ScrollView BuildCharacter()
    {
        CharacterDetailModel vm = controller.Character(); var tablet = Tablet("CharacterDetail", null, 610); var c = Content(tablet); c.AddChild(Button("Back to Household", () => { controller.Back(); Rebuild(); }));
        var header = new Row { Spacing = 18 }; var medal = new CharacterMedallion { Width = 96, Height = 96 }; medal.Child = Text(vm.Monogram, TypographyRole.Title); medal.Semantics.Label = $"Placeholder portrait for {vm.Name}"; header.AddChild(medal);
        var identity = new Column(); identity.AddChild(Text(vm.Name, TypographyRole.Title)); identity.AddChild(Text(vm.Subtitle, TypographyRole.Caption)); header.AddChild(identity); c.AddChild(header);
        c.AddChild(Stats("Attributes", vm.Attributes)); c.AddChild(Stats("Skills", vm.Skills)); c.AddChild(Stats("Condition", vm.Condition)); return Screen(tablet);
    }

    private ScrollView BuildEstate()
    {
        EstateSettlementModel vm = controller.Estate(); var tablet = Tablet("EstateSettlement", null, 610); var c = Content(tablet); c.AddChild(Text("Estate & Settlement", TypographyRole.Title)); c.AddChild(Text($"Settlement stage: {vm.SettlementStage}", TypographyRole.Heading));
        var actions = new Row { Spacing = 10 }; actions.AddChild(Button("Change Rites Budget", () => Run(() => controller.RequestAction(CampaignHouseholdAction.CycleRitesBudget)))); actions.AddChild(new WaxSealButton { Content = Text("Fête", TypographyRole.Button, light: true), Clicked = () => Run(() => controller.RequestAction(CampaignHouseholdAction.FundFestival)) }); c.AddChild(actions);
        var holdings = new Column { Spacing = 8 }; foreach (HoldingModel h in vm.Holdings) { holdings.AddChild(Text(h.Label, TypographyRole.Heading)); foreach (BuildingModel b in h.Buildings) holdings.AddChild(new StatRow { Label = b.Label, Value = b.Condition }); }
        if (vm.Holdings.Count == 0) holdings.AddChild(Text("No household holdings are recorded.", TypographyRole.Body));
        c.AddChild(new ScrollView { Name = "EstateScroll", Height = 430, Content = holdings, IsFocusable = true }); return Screen(tablet);
    }

    private ScrollView BuildReport()
    {
        MonthlyReportModel vm = controller.Report(); var tablet = Tablet("MonthlyReport", null, 610); var c = Content(tablet); c.AddChild(Text($"Monthly Report · {vm.Date}", TypographyRole.Title));
        var summary = new Row { Spacing = 20 }; summary.AddChild(Text($"Income  {vm.Income}", TypographyRole.Ledger)); summary.AddChild(Text($"Expenses  {vm.Expenses}", TypographyRole.Ledger)); summary.AddChild(Text($"Net  {vm.Net}", TypographyRole.Ledger)); c.AddChild(summary);
        var lines = new Column { Spacing = 8 }; foreach (string s in vm.AutomationSummaries) lines.AddChild(Text(s, TypographyRole.Body)); foreach (ReportHeadlineModel h in vm.Headlines) lines.AddChild(Text(h.Label, TypographyRole.Body)); if (vm.Headlines.Count + vm.AutomationSummaries.Count == 0) lines.AddChild(Text("No notable events were recorded this month.", TypographyRole.Body));
        c.AddChild(new ScrollView { Name = "ReportScroll", Height = 470, Content = lines, IsFocusable = true }); return Screen(tablet);
    }

    private void ShowControllerModal()
    {
        ModalState modal = controller.Modal!; var dialog = new Border { Name = modal.Kind == ModalKind.WaxSeal ? "WaxSealConfirmation" : "ConfirmationDialog", Width = 520, Height = 300, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Background = new(245, 229, 195), BorderBrush = new(133, 48, 39), BorderThickness = 4, Padding = new(24), Semantics = { Role = AccessibilityRole.Dialog, Label = modal.Title } };
        var c = new Column { Spacing = 14 }; c.AddChild(Text(modal.Title, TypographyRole.Heading)); c.AddChild(Text(modal.Body, TypographyRole.Body)); var actions = new Row { Spacing = 12 };
        if (modal.Kind == ModalKind.WaxSeal) actions.AddChild(new WaxSealButton { Content = Text("Seal", TypographyRole.Button, light: true), Clicked = () => { controller.ConfirmModal(); Rebuild(); } });
        else actions.AddChild(Button(modal.Kind == ModalKind.Information ? "OK" : "Confirm", () => { controller.ConfirmModal(); Rebuild(); }));
        if (modal.Kind != ModalKind.Information) actions.AddChild(Button("Cancel", () => { controller.CancelModal(); root.CloseModal(); context.Invalidate(); })); c.AddChild(actions); dialog.Child = c; root.ShowModal(dialog);
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
        var c = new Column { Spacing = 8 }; c.AddChild(Text("GENS DEVELOPER CONSOLE · help for commands · ` to close", TypographyRole.Inscription, light: true));
        var output = new Column { Spacing = 3 }; foreach (string line in controller.Logs.TakeLast(18)) output.AddChild(Text(line, TypographyRole.SmallCaption, light: true)); c.AddChild(new ScrollView { Height = 400, Content = output }); c.AddChild(Text($"> {consoleInput}▌", TypographyRole.Ledger, light: true)); panel.Child = c; root.ShowModal(panel); context.Invalidate();
    }

    private void RefreshInkBar() { InkBarModel vm = controller.InkBar(); inkName!.Text = vm.GensName; inkDate!.Text = vm.Date; inkTreasury!.Text = vm.Treasury; inkDignitas!.Text = vm.Dignitas; }
    private void Navigate(ScreenId id) { controller.Navigate(id); Rebuild(); }
    private void Run(Action action) { action(); Rebuild(); }
    private Button NavButton(string label, ScreenId id) => Button(label, () => Navigate(id), $"Nav{id}");
    private static Button Button(string label, Action click, string? name = null) => new() { Name = name ?? label.Replace(" ", string.Empty, StringComparison.Ordinal), Content = Text(label, TypographyRole.Button, light: true), Clicked = click };
    private static Toggle Toggle(string label, bool value, Action<bool> changed) { var t = new Toggle { Content = Text(label, TypographyRole.Button, light: true), Changed = changed }; t.SetChecked(value); return t; }
    private static TextBlock Text(string value, TypographyRole role, bool light = false) => new() { Text = value, TypographyRole = role, Wrapping = TextWrapping.Wrap, Foreground = light ? new Color(245, 229, 195) : null };
    private static WaxTablet Tablet(string name, float? width, float height) => new() { Name = name, Width = width, Height = height, Margin = new(18), HorizontalAlignment = width is null ? HorizontalAlignment.Stretch : HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private static Column Content(WaxTablet tablet) { var c = new Column { Spacing = 12 }; tablet.Child = c; return c; }
    private static ScrollView Center(UiNode child) { var overlay = new Overlay(); overlay.AddChild(child); return new ScrollView { Content = overlay, IsFocusable = true }; }
    private static ScrollView Screen(WaxTablet tablet) => new() { Content = tablet, IsFocusable = true };
    private static Column Stats(string title, IReadOnlyList<StatModel> stats) { var c = new Column { Spacing = 3 }; c.AddChild(Text(title, TypographyRole.Heading)); foreach (StatModel stat in stats) c.AddChild(new StatRow { Label = stat.Label, Value = stat.Value }); return c; }
    private static bool IsGameplay(ScreenId id) => id is ScreenId.HouseholdRoster or ScreenId.CharacterDetail or ScreenId.EstateSettlement or ScreenId.MonthlyReport;
}
