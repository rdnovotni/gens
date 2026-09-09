using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace Gens.Art.Cache;

public sealed record GeneratedAssetReference(string AssetHash, string RequestFingerprint, string VisualStateHash);

public sealed record GeneratedArtRecord(
    string AssetHash,
    string RequestFingerprint,
    string ProviderId,
    string? ProviderVersion,
    string? ProviderModelId,
    string? ProviderModelVersion,
    ArtPurpose Purpose,
    string SubjectId,
    string SubjectVisualHash,
    string PromptCompilerVersion,
    string RecipeVersion,
    string StyleId,
    ulong? Seed,
    int RequestedWidth,
    int RequestedHeight,
    int ActualWidth,
    int ActualHeight,
    DateTimeOffset GeneratedAt,
    string MimeType,
    bool Pinned = false);

public sealed record CachedArtAsset(GeneratedArtRecord Record, string ObjectPath);

public interface IGeneratedArtCache
{
    ValueTask<CachedArtAsset?> FindAsync(string requestFingerprint, CancellationToken cancellationToken = default);
    ValueTask<CachedArtAsset> StoreAsync(ArtGenerationRequest request, IArtProvider provider, ArtProviderOutput output, CancellationToken cancellationToken = default);
    ValueTask<bool> ExistsAsync(string assetHash, CancellationToken cancellationToken = default);
    ValueTask PinAsync(string assetHash, CancellationToken cancellationToken = default);
    ValueTask ClearUnpinnedAsync(CancellationToken cancellationToken = default);
    ArtCacheStatistics GetStatistics();
}

public readonly record struct ArtCacheStatistics(long ObjectBytes, int Records, long Hits, long Misses, long Corruptions);

public sealed class GeneratedArtCache : IGeneratedArtCache, IDisposable
{
    public const int MaximumEncodedBytes = 20 * 1024 * 1024;
    public const int MaximumDimension = 4096;
    public const long MaximumPixels = 16_777_216;
    private readonly string root;
    private readonly Action<string, Exception?> log;
    private readonly SemaphoreSlim gate = new(1, 1);
    private long hits, misses, corruptions;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public GeneratedArtCache(string root, Action<string, Exception?>? log = null) { ArgumentException.ThrowIfNullOrWhiteSpace(root); this.root = Path.GetFullPath(root); this.log = log ?? ((_, _) => { }); }

    public async ValueTask<CachedArtAsset?> FindAsync(string requestFingerprint, CancellationToken cancellationToken = default)
    {
        ValidateHash(requestFingerprint, nameof(requestFingerprint));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string index = IndexPath(requestFingerprint);
            if (!File.Exists(index)) { Interlocked.Increment(ref misses); return null; }
            try
            {
                GeneratedArtRecord? record = JsonSerializer.Deserialize<GeneratedArtRecord>(await File.ReadAllTextAsync(index, cancellationToken).ConfigureAwait(false), JsonOptions);
                if (record is null || record.RequestFingerprint != requestFingerprint) throw new InvalidDataException("Cache index identity mismatch.");
                string objectPath = ObjectPath(record.AssetHash, record.MimeType);
                if (!File.Exists(objectPath)) { Interlocked.Increment(ref misses); return null; }
                byte[] bytes = await File.ReadAllBytesAsync(objectPath, cancellationToken).ConfigureAwait(false);
                ValidatedImage image = ImageValidator.Validate(bytes, record.MimeType);
                string hash = Hash(bytes);
                if (hash != record.AssetHash || image.Width != record.ActualWidth || image.Height != record.ActualHeight) throw new InvalidDataException("Cached generated image failed integrity validation.");
                Interlocked.Increment(ref hits); return new(record, objectPath);
            }
            catch (Exception exception) when (exception is JsonException or IOException or InvalidDataException or UnauthorizedAccessException)
            {
                Interlocked.Increment(ref corruptions); Interlocked.Increment(ref misses);
                Quarantine(index); return null;
            }
        }
        finally { gate.Release(); }
    }

    public async ValueTask<CachedArtAsset> StoreAsync(ArtGenerationRequest request, IArtProvider provider, ArtProviderOutput output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request); ArgumentNullException.ThrowIfNull(provider); ArgumentNullException.ThrowIfNull(output);
        ValidatedImage image = ImageValidator.Validate(output.EncodedImage, output.MimeType);
        string assetHash = Hash(output.EncodedImage); string objectPath = ObjectPath(assetHash, output.MimeType);
        var record = new GeneratedArtRecord(assetHash, request.RequestFingerprint, provider.ProviderId, provider.ProviderVersion,
            output.ProviderModelId, output.ProviderModelVersion, request.Purpose, request.SubjectId, request.SubjectVisualHash,
            request.PromptCompilerVersion, request.RecipeVersion, request.StyleId.Value, request.Seed, request.Width, request.Height,
            image.Width, image.Height, DateTimeOffset.UtcNow, output.MimeType);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(objectPath)!); Directory.CreateDirectory(Path.GetDirectoryName(IndexPath(request.RequestFingerprint))!);
            if (!File.Exists(objectPath)) await AtomicWriteAsync(objectPath, output.EncodedImage, cancellationToken).ConfigureAwait(false);
            byte[] metadata = JsonSerializer.SerializeToUtf8Bytes(record, JsonOptions);
            await AtomicWriteAsync(IndexPath(request.RequestFingerprint), metadata, cancellationToken).ConfigureAwait(false);
            return new(record, objectPath);
        }
        finally { gate.Release(); }
    }

    public ValueTask<bool> ExistsAsync(string assetHash, CancellationToken cancellationToken = default)
    {
        ValidateHash(assetHash, nameof(assetHash)); cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(File.Exists(ObjectPath(assetHash, "image/png")) || File.Exists(ObjectPath(assetHash, "image/jpeg")));
    }

    public async ValueTask PinAsync(string assetHash, CancellationToken cancellationToken = default)
    {
        ValidateHash(assetHash, nameof(assetHash));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (string index in EnumerateIndexes())
            {
                GeneratedArtRecord? record = JsonSerializer.Deserialize<GeneratedArtRecord>(await File.ReadAllTextAsync(index, cancellationToken).ConfigureAwait(false), JsonOptions);
                if (record?.AssetHash == assetHash) await AtomicWriteAsync(index, JsonSerializer.SerializeToUtf8Bytes(record with { Pinned = true }, JsonOptions), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            log($"Generated-art asset {assetHash} could not be pinned; cache retention may reclaim it later.", exception);
        }
        finally { gate.Release(); }
    }

    public async ValueTask ClearUnpinnedAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var keep = new HashSet<string>(StringComparer.Ordinal);
            foreach (string index in EnumerateIndexes())
            {
                GeneratedArtRecord? record = JsonSerializer.Deserialize<GeneratedArtRecord>(await File.ReadAllTextAsync(index, cancellationToken).ConfigureAwait(false), JsonOptions);
                if (record?.Pinned == true) keep.Add(record.AssetHash); else File.Delete(index);
            }
            string objects = Path.Combine(root, "objects");
            if (Directory.Exists(objects)) foreach (string file in Directory.EnumerateFiles(objects, "*", SearchOption.AllDirectories)) if (!keep.Contains(Path.GetFileNameWithoutExtension(file))) File.Delete(file);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            log("Generated-art cache retention pass could not complete; unpinned assets may remain on disk.", exception);
        }
        finally { gate.Release(); }
    }

    public ArtCacheStatistics GetStatistics()
    {
        string objects = Path.Combine(root, "objects"); long bytes = Directory.Exists(objects) ? Directory.EnumerateFiles(objects, "*", SearchOption.AllDirectories).Sum(static path => new FileInfo(path).Length) : 0;
        return new(bytes, EnumerateIndexes().Count(), Volatile.Read(ref hits), Volatile.Read(ref misses), Volatile.Read(ref corruptions));
    }

    private string IndexPath(string fingerprint) => Path.Combine(root, "index", fingerprint[..2], fingerprint + ".json");
    private string ObjectPath(string hash, string mimeType) => Path.Combine(root, "objects", hash[..2], hash + (mimeType == "image/png" ? ".png" : ".jpg"));
    private IEnumerable<string> EnumerateIndexes() { string index = Path.Combine(root, "index"); return Directory.Exists(index) ? Directory.EnumerateFiles(index, "*.json", SearchOption.AllDirectories) : []; }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static void ValidateHash(string value, string name) { if (value.Length != 64 || value.Any(static c => !char.IsAsciiHexDigit(c))) throw new ArgumentException("Expected a lowercase or uppercase SHA-256 hexadecimal value.", name); }
    private static async Task AtomicWriteAsync(string destination, byte[] bytes, CancellationToken cancellationToken)
    {
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { await File.WriteAllBytesAsync(temporary, bytes, cancellationToken).ConfigureAwait(false); File.Move(temporary, destination, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private static void Quarantine(string index) { try { if (File.Exists(index)) File.Move(index, index + ".corrupt-" + Guid.NewGuid().ToString("N")); } catch (IOException) { } }
    public void Dispose() => gate.Dispose();
}

public readonly record struct ValidatedImage(int Width, int Height, string MimeType);

public static class ImageValidator
{
    public static ValidatedImage Validate(byte[] bytes, string mimeType)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0 || bytes.Length > GeneratedArtCache.MaximumEncodedBytes) throw new InvalidDataException("Generated image size is invalid.");
        (int width, int height) = mimeType switch
        {
            "image/png" => ReadPng(bytes),
            "image/jpeg" => ReadJpeg(bytes),
            _ => throw new InvalidDataException("Generated image format is unsupported."),
        };
        if (width is < 1 or > GeneratedArtCache.MaximumDimension || height is < 1 or > GeneratedArtCache.MaximumDimension || (long)width * height > GeneratedArtCache.MaximumPixels)
            throw new InvalidDataException("Generated image dimensions exceed safety limits.");
        return new(width, height, mimeType);
    }

    private static (int Width, int Height) ReadPng(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (bytes.Length < 33 || !bytes[..8].SequenceEqual(signature) || !bytes.Slice(12, 4).SequenceEqual("IHDR"u8) || !bytes[^8..^4].SequenceEqual("IEND"u8)) throw new InvalidDataException("PNG structure is invalid.");
        return (BinaryPrimitives.ReadInt32BigEndian(bytes[16..20]), BinaryPrimitives.ReadInt32BigEndian(bytes[20..24]));
    }

    private static (int Width, int Height) ReadJpeg(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[^2] != 0xff || bytes[^1] != 0xd9) throw new InvalidDataException("JPEG structure is invalid.");
        int offset = 2;
        while (offset + 4 < bytes.Length)
        {
            if (bytes[offset++] != 0xff) continue; byte marker = bytes[offset++]; if (marker is 0xd8 or 0xd9) continue;
            int length = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset, 2));
            if (length < 2 || offset + length > bytes.Length) throw new InvalidDataException("JPEG segment is invalid.");
            if (marker is >= 0xc0 and <= 0xc3) return (BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 5, 2)), BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset + 3, 2)));
            offset += length;
        }
        throw new InvalidDataException("JPEG dimensions are missing.");
    }
}
