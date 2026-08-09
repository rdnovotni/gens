namespace Gens.Simulation.Art;

public readonly record struct ArtRequest(string CacheKey, int Width, int Height, ulong Seed);
public readonly record struct ArtResult(bool Succeeded, byte[]? EncodedImage, string? Error);

public interface IArtGenerationProvider
{
    string ProviderId { get; }
    Task<ArtResult> GenerateAsync(ArtRequest request, CancellationToken cancellationToken);
}

public sealed class NullArtGenerationProvider : IArtGenerationProvider
{
    public string ProviderId => "null";
    public Task<ArtResult> GenerateAsync(ArtRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new ArtResult(false, null, "Art generation is disabled."));
}

public sealed class MockArtGenerationProvider : IArtGenerationProvider
{
    // Valid deterministic 1x1 transparent PNG; callers can exercise raster-cache flows offline.
    private static readonly byte[] Placeholder = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAF/gL+3MxZ5wAAAABJRU5ErkJggg==");
    public string ProviderId => "mock-v1";
    public Task<ArtResult> GenerateAsync(ArtRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ArtResult(true, (byte[])Placeholder.Clone(), null));
    }
}
