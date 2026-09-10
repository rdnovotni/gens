using System.Text.Json;
using Gens.Client.Desktop.Platform;

namespace Gens.Client.Desktop.Saves;

/// <summary>
/// Owns <c>saves/index.json</c>: the single, versioned, atomically-written source of save-slot
/// metadata (ADR 0020), mirroring <see cref="Settings.SettingsService"/>'s load/migrate/atomic-save
/// recipe. Unlike settings, this file is reconciled against the actual contents of <see
/// cref="IApplicationPaths.Saves"/> on every load: a bare pre-existing <c>quicksave.gens</c> (from
/// before this feature shipped) or any <c>.gens</c> file dropped in from elsewhere gets a synthesized
/// entry, and entries whose file no longer exists are dropped.
/// </summary>
public sealed class SaveIndexService
{
    public const string QuicksaveSlotId = "quicksave";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IApplicationPaths paths;
    private readonly Action<string, Exception?> log;

    public SaveIndexService(IApplicationPaths paths, Action<string, Exception?>? log = null)
    {
        this.paths = paths;
        this.log = log ?? ((_, _) => { });
        Current = Load();
    }

    public SaveIndex Current { get; private set; }

    public SaveIndex Load()
    {
        SaveIndex loaded;
        try
        {
            if (!File.Exists(paths.SaveIndexFile)) loaded = new();
            else
            {
                string json = File.ReadAllText(paths.SaveIndexFile);
                loaded = JsonSerializer.Deserialize<SaveIndex>(json, JsonOptions) ?? throw new JsonException("Save index root is null.");
                if (loaded.Version != SaveIndex.CurrentVersion)
                    throw new JsonException($"Unsupported save index version {loaded.Version}.");
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            log("Save index could not be loaded; it will be rebuilt from the saves directory.", ex);
            PreserveInvalidFile();
            loaded = new();
        }

        SaveIndex reconciled = Reconcile(loaded);
        try { Save(reconciled); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log($"Save index could not be persisted to {paths.SaveIndexFile}; slot metadata will be rebuilt next launch.", ex);
        }
        return reconciled;
    }

    /// <summary>Scans <see cref="IApplicationPaths.Saves"/> for <c>*.gens</c> files: keeps indexed
    /// metadata for any file that still exists, synthesizes metadata (from the filename and the
    /// file's own last-write time) for any file the index doesn't know about yet, drops entries for
    /// files that no longer exist, and always guarantees exactly one quicksave entry so callers never
    /// need a separate <c>File.Exists</c> check for it.</summary>
    public SaveIndex Reconcile(SaveIndex loaded)
    {
        var byFileName = loaded.Slots.ToDictionary(static s => s.FileName, StringComparer.OrdinalIgnoreCase);
        var result = new List<SaveSlotMetadata>();

        if (Directory.Exists(paths.Saves))
        {
            foreach (string file in Directory.EnumerateFiles(paths.Saves, "*.gens").OrderBy(static f => f, StringComparer.Ordinal))
            {
                string fileName = Path.GetFileName(file);
                if (byFileName.TryGetValue(fileName, out SaveSlotMetadata? existing)) { result.Add(existing); continue; }
                bool isQuicksave = string.Equals(fileName, "quicksave.gens", StringComparison.OrdinalIgnoreCase);
                result.Add(new SaveSlotMetadata
                {
                    SlotId = isQuicksave ? QuicksaveSlotId : $"save-{Guid.NewGuid():N}",
                    FileName = fileName,
                    DisplayName = isQuicksave ? "Quicksave" : Path.GetFileNameWithoutExtension(fileName),
                    IsQuicksave = isQuicksave,
                    LastSavedUtc = SafeLastWriteTimeUtc(file),
                    PlaytimeSeconds = 0,
                });
            }
        }

        if (!result.Exists(static s => s.IsQuicksave))
            result.Insert(0, new SaveSlotMetadata { SlotId = QuicksaveSlotId, FileName = "quicksave.gens", DisplayName = "Quicksave", IsQuicksave = true, LastSavedUtc = null, PlaytimeSeconds = 0 });

        return new SaveIndex { Version = SaveIndex.CurrentVersion, Slots = result };
    }

    public void Save(SaveIndex index)
    {
        Directory.CreateDirectory(paths.Saves);
        string temporary = paths.SaveIndexFile + ".tmp";
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(index, JsonOptions);
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough)) { stream.Write(bytes); stream.Flush(true); }
        if (File.Exists(paths.SaveIndexFile))
        {
            try { File.Replace(temporary, paths.SaveIndexFile, paths.SaveIndexFile + ".bak", true); }
            catch (PlatformNotSupportedException) { File.Move(temporary, paths.SaveIndexFile, true); }
        }
        else File.Move(temporary, paths.SaveIndexFile);
        Current = index;
    }

    /// <summary>Creates or overwrites a slot's metadata: a brand-new slot starts at zero playtime, an
    /// existing one has <paramref name="playtimeDelta"/> added to its running total (the unaccounted
    /// wall-clock time since the last save to any slot). Always sets <see
    /// cref="SaveSlotMetadata.LastSavedUtc"/> to now.</summary>
    public SaveSlotMetadata Upsert(string slotId, string fileName, string displayName, bool isQuicksave, TimeSpan playtimeDelta)
    {
        List<SaveSlotMetadata> slots = Current.Slots.ToList();
        int index = slots.FindIndex(s => string.Equals(s.SlotId, slotId, StringComparison.Ordinal));
        long previousPlaytime = index >= 0 ? slots[index].PlaytimeSeconds : 0;
        var updated = new SaveSlotMetadata
        {
            SlotId = slotId,
            FileName = fileName,
            DisplayName = displayName,
            IsQuicksave = isQuicksave,
            LastSavedUtc = DateTimeOffset.UtcNow,
            PlaytimeSeconds = previousPlaytime + (long)Math.Max(0, playtimeDelta.TotalSeconds),
        };
        if (index >= 0) slots[index] = updated; else slots.Add(updated);
        Save(new SaveIndex { Version = SaveIndex.CurrentVersion, Slots = slots });
        return updated;
    }

    public SaveSlotMetadata Rename(string slotId, string displayName)
    {
        List<SaveSlotMetadata> slots = Current.Slots.ToList();
        int index = slots.FindIndex(s => string.Equals(s.SlotId, slotId, StringComparison.Ordinal));
        if (index < 0) throw new InvalidOperationException($"No save slot '{slotId}' exists.");
        SaveSlotMetadata updated = slots[index] with { DisplayName = displayName };
        slots[index] = updated;
        Save(new SaveIndex { Version = SaveIndex.CurrentVersion, Slots = slots });
        return updated;
    }

    public void Remove(string slotId)
    {
        SaveIndex next = Current with { Slots = Current.Slots.Where(s => !string.Equals(s.SlotId, slotId, StringComparison.Ordinal)).ToArray() };
        Save(next);
    }

    private static DateTimeOffset? SafeLastWriteTimeUtc(string file)
    {
        try { return File.GetLastWriteTimeUtc(file); }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    private void PreserveInvalidFile()
    {
        try { if (File.Exists(paths.SaveIndexFile)) File.Move(paths.SaveIndexFile, paths.SaveIndexFile + $".corrupt-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}", false); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
