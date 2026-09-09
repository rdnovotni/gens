using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Diagnostics;
using Gens.Client.Desktop.Platform;
using Gens.Audio;
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
        DesktopApplicationPaths? paths = null;
        StructuredFileLogger? logger = null;
        DesktopApplicationController? controller = null;
        try
        {
            RendererMode renderer = ParseRenderer(args);
            paths = new DesktopApplicationPaths(Option(args, "--user-data="));
            paths.EnsureRequiredDirectories();
            logger = new(paths);
            logger.Log(AppLogCategory.Client, RuntimeLogLevel.Information, $"Starting Gens {ReleaseMetadata.Current.Display}; OS={System.Runtime.InteropServices.RuntimeInformation.OSDescription}; arch={System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}.");
            using var platform = new SdlPlatform();
            controller = new DesktopApplicationController(paths, new AudioEngine(new SdlAudioBackend()), settingsLog: (message, exception) => logger.Log(AppLogCategory.Application, RuntimeLogLevel.Warning, message, exception));
            if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
            {
                var silence = new byte[19_200];
                controller.Audio.Play(new AudioClip("smoke.silence", TimeSpan.FromMilliseconds(100), 2, 48_000, AudioStorage.Buffered, () => new MemoryStream(silence, writable: false)), AudioBus.UI);
            }
            if (args.Contains("--developer", StringComparer.OrdinalIgnoreCase)) controller.SetConsoleEnabled(true);
            if (Option(args, "--locale=") is { } locale) controller.SetLocale(locale);
            if (args.Contains("--reduced-motion", StringComparer.OrdinalIgnoreCase)) controller.SetReducedMotion(true);
            if (args.Contains("--high-contrast", StringComparer.OrdinalIgnoreCase)) controller.SetHighContrast(true);
            if (args.Contains("--new-game", StringComparer.OrdinalIgnoreCase)) controller.StartNew("latium", "standard");
            if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase) && Option(args, "--screen=") is null && !controller.HasActiveCampaign) controller.StartNew("latium", "standard");
            ConfigureScreenFixture(controller, args);
            using var graphics = new SkiaGraphicsBackend();
            var application = new GensDesktopApplication(graphics, controller, args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase), Option(args, "--capture="));
            using var runtime = new RuntimeHost(platform, graphics, application,
                new WindowOptions("Gens", 1280, 720, Resizable: true, HighDpi: true, MinWidth: 960, MinHeight: 640), renderer, logger: logger);
            application.CapturePng = runtime.CapturePng;
            runtime.Run();
            controller.Audio.Dispose();
            return 0;
        }
        catch (Exception ex)
        {
            string? report = paths is not null && logger is not null ? new CrashReporter(paths, logger).Capture(ex, controller?.CurrentScreen.ToString() ?? "startup", "SDL3", "SkiaSharp", controller?.Audio.BackendName ?? "not initialized", Option(args, "--renderer=") ?? "default") : null;
            Console.Error.WriteLine($"Gens failed to start: {ex.Message}{Environment.NewLine}{ex}{(report is null ? string.Empty : Environment.NewLine + "Crash report: " + report)}"); return 1;
        }
        finally { controller?.Audio.Dispose(); logger?.Dispose(); }
    }

    private static void ConfigureScreenFixture(DesktopApplicationController controller, string[] args)
    {
        string? screen = Option(args, "--screen=")?.ToLowerInvariant(); if (screen is null) return;
        if (screen == "main-menu") return;
        if (screen is "new-game" or "settings" or "credits") { controller.Navigate(screen switch { "new-game" => ScreenId.NewGameSetup, "settings" => ScreenId.Settings, _ => ScreenId.Credits }); return; }
        if (!controller.HasActiveCampaign) controller.StartNew("latium", "standard");
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
