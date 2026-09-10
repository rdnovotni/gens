using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Saves;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class SaveIndexServiceTests
{
    private string directory = null!;
    [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "gens-save-index-tests", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    [Test]
    public void MissingIndexDefaultsToASingleUnsavedQuicksaveSlot()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        Assert.Multiple(() =>
        {
            Assert.That(service.Current.Slots, Has.Count.EqualTo(1));
            Assert.That(service.Current.Slots[0].IsQuicksave, Is.True);
            Assert.That(service.Current.Slots[0].LastSavedUtc, Is.Null);
        });
    }

    [Test]
    public void UpsertAndSaveRoundTripsThroughAFreshServiceInstance()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        service.Upsert("save-1", "save-1.gens", "The Aemilii", isQuicksave: false, TimeSpan.FromMinutes(5));
        File.WriteAllBytes(Path.Combine(paths.Saves, "save-1.gens"), new byte[] { 1 });
        var reloaded = new SaveIndexService(paths);
        SaveSlotMetadata slot = reloaded.Current.Slots.Single(s => s.SlotId == "save-1");
        Assert.Multiple(() =>
        {
            Assert.That(slot.DisplayName, Is.EqualTo("The Aemilii"));
            Assert.That(slot.PlaytimeSeconds, Is.EqualTo(300));
            Assert.That(slot.LastSavedUtc, Is.Not.Null);
        });
    }

    [Test]
    public void UpsertOnExistingSlotAddsPlaytimeRatherThanReplacingIt()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        service.Upsert("save-1", "save-1.gens", "The Aemilii", isQuicksave: false, TimeSpan.FromMinutes(2));
        service.Upsert("save-1", "save-1.gens", "The Aemilii", isQuicksave: false, TimeSpan.FromMinutes(3));
        Assert.That(service.Current.Slots.Single(s => s.SlotId == "save-1").PlaytimeSeconds, Is.EqualTo(300));
    }

    [Test]
    public void ReconciliationSynthesizesAnEntryForABarePreExistingQuicksave()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        File.WriteAllBytes(paths.Quicksave, new byte[] { 1, 2, 3 });
        var service = new SaveIndexService(paths);
        SaveSlotMetadata slot = service.Current.Slots.Single(static s => s.IsQuicksave);
        Assert.Multiple(() =>
        {
            Assert.That(slot.DisplayName, Is.EqualTo("Quicksave"));
            Assert.That(slot.LastSavedUtc, Is.Not.Null);
            Assert.That(slot.PlaytimeSeconds, Is.Zero);
        });
    }

    [Test]
    public void ReconciliationPicksUpAForeignGensFileDroppedIntoTheSavesDirectory()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        File.WriteAllBytes(Path.Combine(paths.Saves, "imported-campaign.gens"), new byte[] { 1 });
        var service = new SaveIndexService(paths);
        SaveSlotMetadata slot = service.Current.Slots.Single(static s => !s.IsQuicksave);
        Assert.That(slot.DisplayName, Is.EqualTo("imported-campaign"));
    }

    [Test]
    public void ReconciliationDropsEntriesForFilesThatNoLongerExist()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        service.Upsert("save-1", "save-1.gens", "Gone", isQuicksave: false, TimeSpan.Zero);
        var reloaded = new SaveIndexService(paths);
        Assert.That(reloaded.Current.Slots.Any(s => s.SlotId == "save-1"), Is.False);
    }

    [Test]
    public void RemoveDeletesTheIndexEntry()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        service.Upsert("save-1", "save-1.gens", "The Aemilii", isQuicksave: false, TimeSpan.Zero);
        File.WriteAllBytes(Path.Combine(paths.Saves, "save-1.gens"), new byte[] { 1 });
        service.Remove("save-1");
        Assert.That(service.Current.Slots.Any(s => s.SlotId == "save-1"), Is.False);
    }

    [Test]
    public void CorruptIndexFileIsPreservedNotDeletedAndDefaultsAreRebuilt()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        File.WriteAllText(paths.SaveIndexFile, "{ not valid json");
        string? loggedMessage = null;
        var service = new SaveIndexService(paths, (message, _) => loggedMessage = message);
        Assert.Multiple(() =>
        {
            Assert.That(loggedMessage, Is.Not.Null);
            Assert.That(Directory.GetFiles(paths.Saves, "index.json.corrupt-*"), Has.Length.EqualTo(1));
            Assert.That(service.Current.Slots, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void UnicodeDisplayNameRoundTrips()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        var service = new SaveIndexService(paths);
        service.Upsert("save-1", "save-1.gens", "用户-δοκιμή campaign", isQuicksave: false, TimeSpan.Zero);
        File.WriteAllBytes(Path.Combine(paths.Saves, "save-1.gens"), new byte[] { 1 });
        var reloaded = new SaveIndexService(paths);
        Assert.That(reloaded.Current.Slots.Single(s => s.SlotId == "save-1").DisplayName, Is.EqualTo("用户-δοκιμή campaign"));
    }
}
