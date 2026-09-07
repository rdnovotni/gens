using System.Collections.ObjectModel;
using Gens.Presentation.Visuals;

namespace Gens.Art;

public enum ArtPurpose { CharacterPortrait, EventIllustration, ChronicleIllustration, EstateIllustration, EnvironmentIllustration }
public enum ArtPriority { ManualRegeneration, VisiblePortrait, CurrentEventIllustration, VisibleHistoricalIllustration, BackgroundPortrait, Precache }
public enum ArtFailureKind { Unavailable, Timeout, RateLimited, Rejected, Unsupported, Authentication, Network, ProviderError, WorkerCrashed, Cancelled, InvalidOutput, InvalidConfiguration }
public enum ArtSafetyProfile { StandardPortrait, MinorPortrait, ViolenceRestricted, HistoricalScene }
public enum ArtJobStatus { Queued, Generating, RetryScheduled, Completed, Failed, Cancelled }

public abstract record ArtSubject(string SubjectId, string VisualStateHash);
public sealed record EventIllustrationSubject(string SubjectId, string VisualStateHash, string Location, IReadOnlyList<string> Participants, string Action, string Weather, string TimeOfDay, string Mood) : ArtSubject(SubjectId, VisualStateHash);
public sealed record CharacterPortraitSubject(CharacterVisualState VisualState, AppearanceDescription Appearance, string HistoricalContext = "Roman Republic") : ArtSubject(VisualState.CharacterId, VisualState.VisualStateHash)
{
    public bool IsMinor => VisualState.AgeBand is VisualAgeBand.Infant or VisualAgeBand.Child or VisualAgeBand.Adolescent;
}

public sealed record ArtSettings
{
    public bool AiGenerationEnabled { get; init; }
    public string Provider { get; init; } = "none";
    public PortraitGenerationPolicy PortraitPolicy { get; init; } = PortraitGenerationPolicy.OnDemand;
    public bool AutoRegenerateMajorChanges { get; init; }
    public bool ExternalGenerationConsent { get; init; }
    public int MaximumConcurrentRequests { get; init; } = 2;
    public int MaximumAutomaticRetries { get; init; } = 2;
}

public enum PortraitGenerationPolicy { Never, OnDemand, ImportantCharacters, AllNamedCharacters }

public readonly record struct ArtRequestId(string Value)
{
    public static ArtRequestId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct ArtStyleId(string Value)
{
    public override string ToString() => Value;
}

public sealed record CompiledArtPrompt(
    string SubjectDescription,
    string HistoricalContext,
    string ClothingAndStatus,
    string Composition,
    string Lighting,
    string VisualStyle,
    IReadOnlyList<string> QualityConstraints,
    IReadOnlyList<string> NegativeConstraints)
{
    public string PositiveText => string.Join(" ", new[]
    {
        SubjectDescription, HistoricalContext, ClothingAndStatus, Composition, Lighting, VisualStyle,
        string.Join(", ", QualityConstraints),
    }.Where(static part => !string.IsNullOrWhiteSpace(part)));
    public string NegativeText => string.Join(", ", NegativeConstraints);
}

public sealed record ArtGenerationProfile(
    string ProfileId,
    ArtSafetyProfile SafetyProfile,
    IReadOnlyDictionary<string, string>? Parameters = null);

public sealed record ArtGenerationRequest(
    ArtRequestId RequestInstanceId,
    ArtPurpose Purpose,
    string SubjectId,
    string SubjectVisualHash,
    ArtStyleId StyleId,
    CompiledArtPrompt Prompt,
    int Width,
    int Height,
    ulong? Seed,
    string PromptCompilerVersion,
    string RecipeVersion,
    ArtGenerationProfile GenerationProfile,
    IReadOnlyDictionary<string, string> Metadata)
{
    public string RequestFingerprint => ArtRequestFingerprint.Compute(this);
}

public sealed record ArtProviderCapabilities(
    IReadOnlySet<ArtPurpose> SupportedPurposes,
    int MaximumWidth,
    int MaximumHeight,
    bool SupportsSeed,
    bool SupportsNegativePrompt,
    bool SupportsReferenceImage,
    bool IsLocal,
    bool SupportsProgress,
    int MaximumConcurrentRequests)
{
    public bool Supports(ArtGenerationRequest request) =>
        SupportedPurposes.Contains(request.Purpose) && request.Width > 0 && request.Height > 0 &&
        request.Width <= MaximumWidth && request.Height <= MaximumHeight && (request.Seed is null || SupportsSeed);

    public static ArtProviderCapabilities Unavailable { get; } = new(
        new ReadOnlySet<ArtPurpose>(new HashSet<ArtPurpose>()), 0, 0, false, false, false, false, false, 1);
}

public sealed record ArtProviderOutput(
    byte[] EncodedImage,
    string MimeType,
    string? ProviderModelId = null,
    string? ProviderModelVersion = null,
    int? ActualWidth = null,
    int? ActualHeight = null);

public sealed record ArtGenerationResult(ArtProviderOutput? Output, ArtFailureKind? FailureKind, string? Diagnostic)
{
    public bool Succeeded => Output is not null && FailureKind is null;
    public static ArtGenerationResult Success(ArtProviderOutput output) => new(output, null, null);
    public static ArtGenerationResult Failure(ArtFailureKind kind, string diagnostic) => new(null, kind, diagnostic);
}

public interface IArtProvider
{
    string ProviderId { get; }
    string? ProviderVersion { get; }
    ArtProviderCapabilities Capabilities { get; }
    Task<ArtGenerationResult> GenerateAsync(ArtGenerationRequest request, CancellationToken cancellationToken);
}

public interface IArtPromptCompiler<in TSubject>
{
    string Version { get; }
    CompiledArtPrompt Compile(TSubject subject, ArtStyleId style, ArtGenerationProfile profile);
}
