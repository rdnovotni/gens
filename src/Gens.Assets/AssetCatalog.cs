using System.Security.Cryptography;
using System.Text;

namespace Gens.Assets;

public readonly record struct AssetId
{
    public AssetId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(static c => !(char.IsLower(c) || char.IsDigit(c) || c is '/' or '-' or '.'))) throw new ArgumentException("Asset IDs use lowercase letters, digits, '.', '/', and '-'.", nameof(value));
        Value = value;
    }
    public string Value { get; }
    public override string ToString() => Value;
}

public sealed record AssetManifestEntry(AssetId Id, string Kind, string Provenance, string License, string AssetPath, int Version, IReadOnlyList<string> Tags);

public sealed class AssetManifest
{
    private readonly Dictionary<AssetId, AssetManifestEntry> entries;
    public AssetManifest(IEnumerable<AssetManifestEntry> entries)
    {
        this.entries = new();
        foreach (AssetManifestEntry entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Kind) || string.IsNullOrWhiteSpace(entry.Provenance) || string.IsNullOrWhiteSpace(entry.License) || string.IsNullOrWhiteSpace(entry.AssetPath) || entry.Version < 1)
                throw new InvalidDataException($"Asset manifest entry '{entry.Id}' is incomplete.");
            if (!this.entries.TryAdd(entry.Id, entry)) throw new InvalidDataException($"Duplicate asset ID '{entry.Id}'.");
        }
    }
    public IReadOnlyCollection<AssetManifestEntry> Entries => entries.Values;
    public AssetManifestEntry Resolve(AssetId id) => entries.TryGetValue(id, out AssetManifestEntry? value) ? value : throw new KeyNotFoundException($"Unknown asset ID '{id}'.");
    public void ValidateReferences(IEnumerable<AssetId> references) { foreach (AssetId id in references) Resolve(id); }
}

public sealed class ContentAddressedCache
{
    private readonly string root;
    public ContentAddressedCache(string root) { this.root = Path.GetFullPath(root ?? throw new ArgumentNullException(nameof(root))); }
    public static string Hash(string canonicalRequest)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
    public static string HashBytes(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    public string PathFor(string namespaceName, string hash, string extension)
    {
        if (hash.Length != 64 || hash.Any(static value => !char.IsAsciiHexDigit(value))) throw new ArgumentException("A SHA-256 hex digest is required.", nameof(hash));
        string directory = Path.Combine(root, namespaceName, hash[..2]);
        return Path.Combine(directory, hash + extension);
    }
    public byte[]? Read(string namespaceName, string hash, string extension) { string path = PathFor(namespaceName, hash, extension); return File.Exists(path) ? File.ReadAllBytes(path) : null; }
    public void Write(string namespaceName, string hash, string extension, ReadOnlySpan<byte> bytes)
    {
        string path = PathFor(namespaceName, hash, extension); string directory = Path.GetDirectoryName(path)!; Directory.CreateDirectory(directory);
        string temporary = path + ".tmp-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        File.WriteAllBytes(temporary, bytes.ToArray());
        File.Move(temporary, path, true);
    }
}
