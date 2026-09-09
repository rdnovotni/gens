using System.Security.Cryptography;
using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Runtime;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class ScreenGoldenTests
{
    private static readonly string[] Names = { "main-menu", "new-game", "settings", "credits", "roster", "character", "estate", "report", "confirmation", "wax-seal" };

    [TestCaseSource(nameof(Names))]
    public void ScreenFixtureMatchesReviewedGolden(string name)
    {
        string root = Path.Combine(Path.GetTempPath(), "gens-desktop-golden-tests", Guid.NewGuid().ToString("N"));
        string capturePath = Path.Combine(root, "capture", name + ".png");
        Directory.CreateDirectory(root);
        try
        {
            using var graphics = new SkiaGraphicsBackend();
            using var platform = new DeterministicPlatform();
            var controller = new DesktopApplicationController(new DesktopApplicationPaths(root));
            ConfigureFixture(controller, name);
            using var app = new GensDesktopApplication(graphics, controller, smokeTest: true, smokeCapturePath: capturePath);
            using var host = new RuntimeHost(platform, graphics, app, new WindowOptions("Golden Test", 1280, 720), RendererMode.Software, vsync: false);
            app.CapturePng = host.CapturePng;
            while (host.IsRunning) host.Step(TimeSpan.FromMilliseconds(16));

            byte[] expected = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Goldens", name + ".png"));
            byte[] actual = File.ReadAllBytes(capturePath);
            Assert.That(Convert.ToHexString(SHA256.HashData(actual)), Is.EqualTo(Convert.ToHexString(SHA256.HashData(expected))), $"Screen fixture '{name}' no longer matches the reviewed golden.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void ConfigureFixture(DesktopApplicationController controller, string name)
    {
        if (name == "main-menu") return;
        if (name is "new-game" or "settings" or "credits")
        {
            controller.Navigate(name switch { "new-game" => ScreenId.NewGameSetup, "settings" => ScreenId.Settings, _ => ScreenId.Credits });
            return;
        }

        controller.StartNew("latium", "standard");
        switch (name)
        {
            case "roster":
                break;
            case "character":
                controller.OpenCharacter(controller.Roster().Rows[0].CharacterId);
                break;
            case "estate":
                controller.Navigate(ScreenId.EstateSettlement);
                break;
            case "report":
                controller.AdvanceMonth();
                break;
            case "confirmation":
                controller.RequestMainMenu();
                break;
            case "wax-seal":
                controller.RequestAction(Gens.Application.Campaign.CampaignHouseholdAction.FundFestival);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown screen fixture.");
        }
    }
}

internal sealed class DeterministicPlatform : IPlatform
{
    public DeterministicWindow Window { get; } = new();
    public string PlatformName => "deterministic";
    public IClipboard Clipboard { get; } = new DeterministicClipboard();
    public IPlatformClock Clock { get; } = new DeterministicClock();
    public IWindow CreateWindow(WindowOptions options) => Window;
    public void PumpEvents(IPlatformEventSink sink) { }
    public bool WaitForEvents(TimeSpan timeout, IPlatformEventSink sink) { ((DeterministicClock)Clock).Advance(); return false; }
    public void Dispose() { }

    private sealed class DeterministicClipboard : IClipboard
    {
        private string? text;
        public string? GetText() => text;
        public void SetText(string value) => text = value;
    }

    private sealed class DeterministicClock : IPlatformClock
    {
        private long ticks;
        public long Frequency => 1_000_000;
        public long GetTimestamp() { ticks += 250_000; return ticks; }
        public void Advance() => ticks += 250_000;
    }
}

internal sealed class DeterministicWindow : IWindow
{
    public WindowId Id => new(1);
    public LogicalSize LogicalSize => new(1280, 720);
    public PixelSize PixelSize => new(1280, 720);
    public DisplayScale DisplayScale => DisplayScale.Identity;
    public WindowState State => WindowState.Normal;
    public void StartTextInput() { }
    public void StopTextInput() { }
    public void SetFullscreen(bool fullscreen) { }
    public void Dispose() { }
}
