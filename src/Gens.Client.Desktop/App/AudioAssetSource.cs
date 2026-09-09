using Gens.Audio;
using Gens.Audio.Decoding;

namespace Gens.Client.Desktop.App;

/// <summary>
/// Resolves bundled `.wav`/`.ogg` files under the client's Assets/Audio directory and decodes them into
/// playable <see cref="AudioClip"/>s via <see cref="AudioAssetLoader"/>. Mirrors the traversal-safe path
/// resolution used for user-data paths (<c>IApplicationPaths.ResolveSafePath</c>) so a malformed or hostile
/// relative path can never escape the audio asset directory.
/// </summary>
public sealed class AudioAssetSource
{
    private readonly string root;
    private readonly AudioAssetLoader loader;

    public AudioAssetSource(string? root = null, AudioAssetLoader? loader = null)
    {
        this.root = Path.GetFullPath(root ?? Path.Combine(AppContext.BaseDirectory, "Assets", "Audio"));
        this.loader = loader ?? new AudioAssetLoader();
    }

    public AudioClip Load(string id, string relativePath, AudioStorage storage = AudioStorage.Buffered)
    {
        string resolvedPath = ResolveSafePath(relativePath);
        return loader.LoadClip(id, () => File.OpenRead(resolvedPath), storage);
    }

    private string ResolveSafePath(string untrustedRelativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(untrustedRelativePath);
        string fullRoot = root + Path.DirectorySeparatorChar;
        string candidate = Path.GetFullPath(Path.Combine(root, untrustedRelativePath));
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!candidate.StartsWith(fullRoot, comparison)) throw new InvalidOperationException("The requested audio asset path escapes the audio asset directory.");
        return candidate;
    }
}
