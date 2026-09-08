namespace Gens.Art.Providers;

public sealed class NullArtProvider : IArtProvider
{
    public string ProviderId => "none";
    public string? ProviderVersion => null;
    public ArtProviderCapabilities Capabilities => ArtProviderCapabilities.Unavailable;
    public Task<ArtGenerationResult> GenerateAsync(ArtGenerationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ArtGenerationResult.Failure(ArtFailureKind.Unavailable, "Optional artwork generation is disabled or unavailable."));
    }
}
