using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Runtime;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

// UR-02 / ADR 0014 gate 38: proves the launch -> new -> roster -> character -> estate -> command ->
// advance -> report -> save -> menu -> load slice is reachable using only Tab/Shift+Tab focus
// traversal and Enter activation, routed through the same PlatformEvent path RuntimeHost dispatches
// in production. No pointer event is used anywhere in this test.
[TestFixture]
public sealed class KeyboardOnlyWorkflowTests : IDisposable
{
    private const uint TabScanCode = 43, EnterScanCode = 40, EscapeScanCode = 41;

    private string directory = null!;
    private DesktopApplicationController controller = null!;
    private GensDesktopApplication application = null!;
    private RuntimeHost host = null!;
    private ulong timestamp;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "gens-keyboard-tests", Guid.NewGuid().ToString("N"));
        controller = new(new DesktopApplicationPaths(directory));
        var graphics = new SkiaGraphicsBackend();
        application = new(graphics, controller);
        host = new(new HeadlessPlatform(), graphics, application, new("keyboard-only-workflow"), RendererMode.Software);
        host.Step(TimeSpan.Zero);
    }

    [TearDown]
    public void TearDown()
    {
        Dispose();
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    public void Dispose() => host?.Dispose();

    [Test]
    public void FullCampaignVerticalSliceCompletesKeyboardOnly()
    {
        TabTo("New Game"); Activate();
        Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.NewGameSetup));

        TabTo("Begin Campaign"); Activate();
        Assert.Multiple(() =>
        {
            Assert.That(controller.HasActiveCampaign, Is.True);
            Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster));
        });

        string characterName = controller.Roster().Rows[0].Name;
        TabTo($"Open details for {characterName}"); Activate();
        Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.CharacterDetail));

        PressKey(EscapeScanCode);
        Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster));

        TabTo("Estate"); Activate();
        Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.EstateSettlement));

        ulong hashBeforeCommand = controller.StateHash!.Value;
        TabTo("Fête"); Activate();
        Assert.That(controller.Modal?.Kind, Is.EqualTo(ModalKind.WaxSeal));
        TabTo("Seal"); Activate();
        Assert.Multiple(() =>
        {
            Assert.That(controller.StateHash, Is.Not.EqualTo(hashBeforeCommand));
            Assert.That(controller.Modal?.Kind, Is.EqualTo(ModalKind.Information));
        });
        TabTo("OK"); Activate();
        Assert.That(controller.Modal, Is.Null);

        string dateBeforeAdvance = controller.InkBar().Date;
        TabTo("Advance"); Activate();
        Assert.Multiple(() =>
        {
            Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.MonthlyReport));
            Assert.That(controller.InkBar().Date, Is.Not.EqualTo(dateBeforeAdvance));
        });

        TabTo("Save"); Activate();
        Assert.That(controller.Modal?.Title, Is.EqualTo("Campaign Saved"));
        TabTo("OK"); Activate();

        TabTo("Menu"); Activate();
        Assert.That(controller.Modal?.Title, Is.EqualTo("Return to Main Menu"));
        TabTo("Confirm"); Activate();
        Assert.Multiple(() =>
        {
            Assert.That(controller.HasActiveCampaign, Is.False);
            Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.MainMenu));
        });

        TabTo("Load Game"); Activate();
        Assert.Multiple(() =>
        {
            Assert.That(controller.HasActiveCampaign, Is.True);
            Assert.That(controller.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster));
        });
    }

    private void TabTo(string label)
    {
        UiNode? first = application.Root.Focus.FocusedNode;
        for (int i = 0; i < 100; i++)
        {
            UiNode? node = application.Root.Focus.FocusedNode;
            if (node is not null && string.Equals(node.Semantics.Label, label, StringComparison.Ordinal)) return;
            PressKey(TabScanCode);
            if (i > 0 && ReferenceEquals(application.Root.Focus.FocusedNode, first)) break;
        }
        throw new InvalidOperationException($"No focusable control labeled '{label}' was reachable by Tab from the current screen.");
    }

    private void Activate() => PressKey(EnterScanCode);

    private void PressKey(uint scanCode) =>
        application.HandleEvent(new KeyboardEvent(++timestamp, new(1), new(scanCode), KeyModifiers.None, IsDown: true, IsRepeat: false));

    private sealed class HeadlessPlatform : IPlatform
    {
        public HeadlessWindow Window { get; } = new();
        public string PlatformName => "headless-test";
        public IClipboard Clipboard { get; } = new HeadlessClipboard();
        public IPlatformClock Clock { get; } = new HeadlessClock();
        public IWindow CreateWindow(WindowOptions options) => Window;
        public void PumpEvents(IPlatformEventSink sink) { }
        public bool WaitForEvents(TimeSpan timeout, IPlatformEventSink sink) => false;
        public void Dispose() { }

        private sealed class HeadlessClipboard : IClipboard
        {
            private string? text;
            public string? GetText() => text;
            public void SetText(string value) => text = value;
        }

        private sealed class HeadlessClock : IPlatformClock
        {
            private long ticks;
            public long GetTimestamp() => ++ticks;
            public long Frequency => 1000;
        }
    }

    private sealed class HeadlessWindow : IPixelBufferWindow
    {
        public WindowId Id => new(1);
        public LogicalSize LogicalSize => new(1280, 720);
        public PixelSize PixelSize => new(1280, 720);
        public DisplayScale DisplayScale => DisplayScale.Identity;
        public WindowState State => WindowState.Normal;
        public void StartTextInput() { }
        public void StopTextInput() { }
        public void SetFullscreen(bool fullscreen) { }
        public void PresentPixels(ReadOnlySpan<byte> pixels, PixelSize size, int rowBytes) { }
        public void Dispose() { }
    }
}
