namespace Gens.Art.Providers;

public sealed record MockArtProviderOptions(
    TimeSpan Delay,
    int TransientFailuresBeforeSuccess = 0,
    ArtFailureKind? PermanentFailure = null,
    string OutputVariant = "default",
    int MaximumConcurrentRequests = 2);

public sealed class MockArtProvider : IArtProvider
{
    private readonly MockArtProviderOptions options;
    private int calls;
    private int active;
    private int maximumObservedConcurrency;

    public MockArtProvider(MockArtProviderOptions? options = null) => this.options = options ?? new(TimeSpan.Zero);
    public string ProviderId => "mock";
    public string? ProviderVersion => "mock-v1";
    public int CallCount => Volatile.Read(ref calls);
    public int MaximumObservedConcurrency => Volatile.Read(ref maximumObservedConcurrency);
    public bool ObservedCancellation { get; private set; }
    public ArtProviderCapabilities Capabilities { get; } = new(
        new HashSet<ArtPurpose>(Enum.GetValues<ArtPurpose>()), 2048, 2048, true, true, false, true, false, 8);

    public async Task<ArtGenerationResult> GenerateAsync(ArtGenerationRequest request, CancellationToken cancellationToken)
    {
        int call = Interlocked.Increment(ref calls);
        int now = Interlocked.Increment(ref active);
        UpdateMaximum(now);
        try
        {
            try { await Task.Delay(options.Delay, cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { ObservedCancellation = true; throw; }
            if (options.PermanentFailure is { } permanent)
                return ArtGenerationResult.Failure(permanent, "Configured deterministic Mock provider failure.");
            if (call <= options.TransientFailuresBeforeSuccess)
                return ArtGenerationResult.Failure(ArtFailureKind.Network, $"Configured transient Mock failure {call}.");
            byte[] png = DeterministicPng.Create(request.Width, request.Height, request.RequestFingerprint + options.OutputVariant);
            return ArtGenerationResult.Success(new(png, "image/png", "mock-raster", "1", request.Width, request.Height));
        }
        finally { Interlocked.Decrement(ref active); }
    }

    private void UpdateMaximum(int value)
    {
        int observed;
        do { observed = Volatile.Read(ref maximumObservedConcurrency); if (value <= observed) return; }
        while (Interlocked.CompareExchange(ref maximumObservedConcurrency, value, observed) != observed);
    }
}
