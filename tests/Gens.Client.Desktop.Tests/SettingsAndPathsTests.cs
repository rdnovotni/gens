using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Settings;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class SettingsAndPathsTests
{
    private string directory = null!;
    [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "gens-settings-tests", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    [Test]
    public void CorruptSettingsFileFallsBackToDefaultsAndIsPreserved()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        Directory.CreateDirectory(paths.Settings); File.WriteAllText(paths.SettingsFile, "{ not valid json");
        string? loggedMessage = null;
        var service = new SettingsService(paths, (message, _) => loggedMessage = message);
        Assert.Multiple(() =>
        {
            Assert.That(service.Current, Is.EqualTo(new DesktopSettings()));
            Assert.That(loggedMessage, Is.Not.Null);
            Assert.That(Directory.GetFiles(paths.Settings, "settings.json.corrupt-*"), Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void V1SettingsMigrateReducedMotionIntoMotionModeAndVersion()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories(); Directory.CreateDirectory(paths.Settings);
        File.WriteAllText(paths.SettingsFile, """{"version":1,"accessibility":{"reducedMotion":true}}""");
        var service = new SettingsService(paths);
        Assert.Multiple(() =>
        {
            Assert.That(service.Current.Version, Is.EqualTo(DesktopSettings.CurrentVersion));
            Assert.That(service.Current.Accessibility.Motion, Is.EqualTo(MotionMode.Reduced));
        });
    }

    [Test]
    public void SaveTwiceRoundTripsAndLeavesNoTemporaryFile()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SettingsService(paths);
        service.Update(service.Current with { Display = service.Current.Display with { UiScale = 1.5f } }, "Display");
        service.Update(service.Current with { Language = service.Current.Language with { Locale = "fr" } }, "Language");
        var reloaded = new SettingsService(paths);
        Assert.Multiple(() =>
        {
            Assert.That(reloaded.Current.Display.UiScale, Is.EqualTo(1.5f));
            Assert.That(reloaded.Current.Language.Locale, Is.EqualTo("fr"));
            Assert.That(File.Exists(paths.SettingsFile + ".tmp"), Is.False);
        });
    }

    // A permission-bit ("read-only directory") simulation is unreliable here: this suite may run
    // as root, which bypasses DAC permission checks entirely. Instead this puts a plain file where
    // the settings directory must go, so Directory.CreateDirectory fails structurally regardless of
    // the running user's privileges.
    [Test]
    public void UnwritableSettingsRootFailsSaveWithoutThrowingOrLosingPriorState()
    {
        var paths = new DesktopApplicationPaths(directory);
        Directory.CreateDirectory(paths.UserData);
        File.WriteAllText(paths.Settings, "a file blocking the settings directory");

        string? loggedMessage = null;
        var service = new SettingsService(paths, (message, _) => loggedMessage = message);
        DesktopSettings previous = service.Current;

        Assert.DoesNotThrow(() => service.Update(previous with { Language = previous.Language with { Locale = "de" } }, "Language"));
        Assert.Multiple(() =>
        {
            Assert.That(loggedMessage, Is.Not.Null);
            Assert.That(service.Current, Is.EqualTo(previous));
        });
    }
}
