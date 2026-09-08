using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Gens.Art.Providers;

public sealed class GensBackendArtProvider : HttpArtProviderBase
{
    public GensBackendArtProvider(HttpClient client, Uri endpoint, Func<CancellationToken, ValueTask<string?>> accessToken) : base(client, endpoint, accessToken, false) { }
    public override string ProviderId => "gens-backend";
}

public sealed class LocalWorkerArtProvider : HttpArtProviderBase
{
    public const string ProtocolVersion = "gens-art-worker-v1";
    public LocalWorkerArtProvider(HttpClient client, Uri endpoint) : base(client, endpoint, null, true)
    {
        if (!endpoint.IsLoopback) throw new ArgumentException("The local worker endpoint must be loopback.", nameof(endpoint));
    }
    public override string ProviderId => "local-worker";
    protected override ArtFailureKind ConnectionFailureKind => ArtFailureKind.WorkerCrashed;
}

public abstract class HttpArtProviderBase : IArtProvider
{
    private const int MaximumResponseBytes = 20 * 1024 * 1024;
    private readonly HttpClient client;
    private readonly Uri endpoint;
    private readonly Func<CancellationToken, ValueTask<string?>>? accessToken;

    protected HttpArtProviderBase(HttpClient client, Uri endpoint, Func<CancellationToken, ValueTask<string?>>? accessToken, bool allowLoopbackHttp)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client)); this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint)); this.accessToken = accessToken;
        if (endpoint.Scheme != Uri.UriSchemeHttps && !(allowLoopbackHttp && endpoint.IsLoopback))
            throw new ArgumentException("Art services require HTTPS; plain HTTP is allowed only for a loopback local worker.", nameof(endpoint));
        Capabilities = new(new HashSet<ArtPurpose> { ArtPurpose.CharacterPortrait, ArtPurpose.EventIllustration }, 2048, 2048, true, true, false, allowLoopbackHttp, false, 2);
    }

    public abstract string ProviderId { get; }
    protected virtual ArtFailureKind ConnectionFailureKind => ArtFailureKind.Network;
    public string? ProviderVersion => "protocol-v1";
    public ArtProviderCapabilities Capabilities { get; }

    public async Task<ArtGenerationResult> GenerateAsync(ArtGenerationRequest request, CancellationToken cancellationToken)
    {
        if (!Capabilities.Supports(request)) return ArtGenerationResult.Failure(ArtFailureKind.Unsupported, "Request exceeds provider capabilities.");
        using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(endpoint, "v1/generate")) { Content = JsonContent.Create(new WireRequest(request)) };
        if (accessToken is not null)
        {
            string? token = await accessToken(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(token)) return ArtGenerationResult.Failure(ArtFailureKind.Authentication, "No backend credential is available.");
            message.Headers.Authorization = new("Bearer", token);
        }
        try
        {
            using HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.TooManyRequests) return ArtGenerationResult.Failure(ArtFailureKind.RateLimited, "Backend rate limited the request.");
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) return ArtGenerationResult.Failure(ArtFailureKind.Authentication, "Backend authentication failed.");
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity) return ArtGenerationResult.Failure(ArtFailureKind.Rejected, "Backend rejected the request.");
            if (!response.IsSuccessStatusCode) return ArtGenerationResult.Failure((int)response.StatusCode >= 500 ? ArtFailureKind.ProviderError : ArtFailureKind.InvalidConfiguration, $"Backend returned HTTP {(int)response.StatusCode}.");
            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType is not ("image/png" or "image/jpeg")) return ArtGenerationResult.Failure(ArtFailureKind.InvalidOutput, "Backend returned an unsupported content type.");
            long? length = response.Content.Headers.ContentLength;
            if (length > MaximumResponseBytes) return ArtGenerationResult.Failure(ArtFailureKind.InvalidOutput, "Backend image exceeds the response-size limit.");
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var destination = new MemoryStream(); byte[] buffer = new byte[81920]; int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0) { if (destination.Length + read > MaximumResponseBytes) return ArtGenerationResult.Failure(ArtFailureKind.InvalidOutput, "Backend image exceeds the response-size limit."); destination.Write(buffer, 0, read); }
            return ArtGenerationResult.Success(new(destination.ToArray(), mediaType));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return ArtGenerationResult.Failure(ArtFailureKind.Timeout, "Art service timed out."); }
        catch (HttpRequestException exception) { return ArtGenerationResult.Failure(ConnectionFailureKind, exception.Message); }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try { using HttpResponseMessage response = await client.GetAsync(new Uri(endpoint, "v1/health"), cancellationToken).ConfigureAwait(false); return response.IsSuccessStatusCode; }
        catch (HttpRequestException) { return false; }
    }

    private sealed record WireRequest(
        [property: JsonPropertyName("requestId")] string RequestId,
        [property: JsonPropertyName("fingerprint")] string Fingerprint,
        [property: JsonPropertyName("purpose")] string Purpose,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("negativePrompt")] string NegativePrompt,
        [property: JsonPropertyName("width")] int Width,
        [property: JsonPropertyName("height")] int Height,
        [property: JsonPropertyName("seed")] ulong? Seed)
    {
        public WireRequest(ArtGenerationRequest request) : this(request.RequestInstanceId.Value, request.RequestFingerprint, request.Purpose.ToString(), request.Prompt.PositiveText, request.Prompt.NegativeText, request.Width, request.Height, request.Seed) { }
    }
}

public static class ArtWorkerProtocol
{
    public const string Version = "gens-art-worker-v1";
    public const string HealthPath = "v1/health";
    public const string VersionPath = "v1/version";
    public const string CapabilitiesPath = "v1/capabilities";
    public const string GeneratePath = "v1/generate";
    public const string CancelPath = "v1/cancel/{requestId}";
}
