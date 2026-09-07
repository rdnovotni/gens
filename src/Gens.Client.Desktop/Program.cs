using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
using Gens.Graphics;
using Gens.Graphics.Skia;
using Gens.Platform;
using Gens.Platform.Sdl;
using Gens.Runtime;

namespace Gens.Client.Desktop;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            RendererMode renderer = ParseRenderer(args);
            var paths = new DesktopApplicationPaths();
            var controller = new DesktopApplicationController(paths);
            if (args.Contains("--new-game", StringComparer.OrdinalIgnoreCase)) controller.StartNew("latium", "standard");
            ConfigureScreenFixture(controller, args);
            using var platform = new SdlPlatform();
            using var graphics = new SkiaGraphicsBackend();
            var application = new GensDesktopApplication(graphics, controller, args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase), Option(args, "--capture="));
            using var runtime = new RuntimeHost(platform, graphics, application,
                new WindowOptions("Gens", 1280, 720, Resizable: true, HighDpi: true, MinWidth: 960, MinHeight: 640), renderer);
            application.CapturePng = runtime.CapturePng;
            runtime.Run();
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"Gens failed to start: {ex.Message}{Environment.NewLine}{ex}"); return 1; }
    }

    private static void ConfigureScreenFixture(DesktopApplicationController controller, string[] args)
    {
        string? screen = Option(args, "--screen=")?.ToLowerInvariant(); if (screen is null) return;
        if (screen == "main-menu") return;
        if (screen is "new-game" or "settings" or "credits") { controller.Navigate(screen switch { "new-game" => ScreenId.NewGameSetup, "settings" => ScreenId.Settings, _ => ScreenId.Credits }); return; }
        if (controller.CurrentCampaign is null) controller.StartNew("latium", "standard");
        switch (screen)
        {
            case "roster": break;
            case "character": controller.OpenCharacter(controller.Roster().Rows[0].CharacterId); break;
            case "estate": controller.Navigate(ScreenId.EstateSettlement); break;
            case "report": controller.AdvanceMonth(); break;
            case "confirmation": controller.RequestMainMenu(); break;
            case "wax-seal": controller.RequestAction(Gens.Application.Campaign.CampaignHouseholdAction.FundFestival); break;
        }
    }

    private static string? Option(string[] args, string prefix) => args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))?.Substring(prefix.Length);

    private static RendererMode ParseRenderer(string[] args)
    {
        string? value = args.FirstOrDefault(static a => a.StartsWith("--renderer=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];
        return value?.ToLowerInvariant() switch { null or "default" => RendererMode.Default, "software" => RendererMode.Software, "gpu" => RendererMode.Gpu, _ => throw new ArgumentException("Renderer must be default, software, or gpu.") };
    }
}
