using System.Collections.Concurrent;
using Gens.Art.Cache;
using Gens.Art.Queue;
using Gens.Presentation.Visuals;

namespace Gens.Art.Portraits;

public sealed record GeneratedPortraitSelection(
    string SubjectId,
    CachedArtAsset Asset,
    CharacterVisualState VisualState,
    GeneratedPortraitStaleness Staleness = GeneratedPortraitStaleness.Current);

public sealed record GeneratedPortraitSnapshot(string EventId, GeneratedPortraitSelection Selection, DateTimeOffset CapturedAt);

public sealed class GeneratedPortraitCoordinator : IGeneratedPortraitSource
{
    private readonly ArtGenerationQueue queue;
    private readonly IGeneratedArtCache cache;
    private readonly CharacterPortraitPromptCompiler compiler;
    private readonly ConcurrentDictionary<string, GeneratedPortraitSelection> generated = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> pending = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> proceduralOverrides = new(StringComparer.Ordinal);

    public GeneratedPortraitCoordinator(ArtGenerationQueue queue, IGeneratedArtCache cache, CharacterPortraitPromptCompiler? compiler = null)
    {
        this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.compiler = compiler ?? new();
    }

    public event EventHandler<GeneratedPortraitUpdatedEventArgs>? PortraitUpdated;

    public GeneratedPortraitSelection? GetCurrent(CharacterVisualState visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        if (proceduralOverrides.ContainsKey(visual.CharacterId) || !generated.TryGetValue(visual.CharacterId, out GeneratedPortraitSelection? selected)) return null;
        if (selected.VisualState.VisualStateHash == visual.VisualStateHash) return selected with { Staleness = GeneratedPortraitStaleness.Current };
        VisualChangeKind change = VisualHash.Classify(selected.VisualState, visual);
        return selected with { Staleness = change == VisualChangeKind.Minor ? GeneratedPortraitStaleness.StaleMinor : GeneratedPortraitStaleness.StaleMajor };
    }

    GeneratedPortraitAsset? IGeneratedPortraitSource.GetCurrent(CharacterVisualState visual)
    {
        GeneratedPortraitSelection? selection = GetCurrent(visual);
        if (selection is null) return null;
        GeneratedArtRecord record = selection.Asset.Record;
        return new(selection.SubjectId, record.AssetHash, selection.Asset.ObjectPath, new(record.StyleId), record.SubjectVisualHash,
            record.ProviderVersion ?? record.ProviderId, record.ActualWidth, selection.Staleness);
    }

    public async Task<GeneratedPortraitSelection?> RequestAsync(CharacterVisualState visual, AppearanceDescription appearance, ArtSettings settings, ArtStyleId? style = null, bool regenerate = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(settings);
        if (!settings.AiGenerationEnabled || settings.PortraitPolicy == PortraitGenerationPolicy.Never) return null;
        if (settings.Provider is not "mock" and not "local-worker" && !settings.ExternalGenerationConsent)
            throw new InvalidOperationException("External artwork generation requires informed user consent.");
        proceduralOverrides.TryRemove(visual.CharacterId, out _);
        var subject = new CharacterPortraitSubject(visual, appearance);
        var profile = new ArtGenerationProfile("portrait-standard-v1", subject.IsMinor ? ArtSafetyProfile.MinorPortrait : ArtSafetyProfile.StandardPortrait);
        ArtStyleId selectedStyle = style ?? new("painted-realism");
        CompiledArtPrompt prompt = compiler.Compile(subject, selectedStyle, profile);
        var metadata = regenerate ? new Dictionary<string, string> { ["regenerationNonce"] = ArtRequestId.New().Value } : new Dictionary<string, string>();
        var request = new ArtGenerationRequest(ArtRequestId.New(), ArtPurpose.CharacterPortrait, subject.SubjectId, subject.VisualStateHash,
            selectedStyle, prompt, 512, 512, null, compiler.Version, $"character-portrait-recipe-v{PortraitRecipeBuilder.CurrentRecipeVersion}", profile, metadata);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (pending.TryRemove(subject.SubjectId, out CancellationTokenSource? previous)) { previous.Cancel(); previous.Dispose(); }
        pending[subject.SubjectId] = linked;
        try
        {
            CachedArtAsset? asset = await queue.EnqueueAsync(request, regenerate ? ArtPriority.ManualRegeneration : ArtPriority.VisiblePortrait, linked.Token).ConfigureAwait(false);
            if (asset is null) return null;
            var selection = new GeneratedPortraitSelection(subject.SubjectId, asset, visual);
            generated[subject.SubjectId] = selection;
            PortraitUpdated?.Invoke(this, new(subject.SubjectId, selection));
            return selection;
        }
        finally { pending.TryRemove(new KeyValuePair<string, CancellationTokenSource>(subject.SubjectId, linked)); }
    }

    public bool Cancel(string subjectId) { if (!pending.TryGetValue(subjectId, out CancellationTokenSource? cancellation)) return false; cancellation.Cancel(); return true; }
    public void UseProcedural(string subjectId) { proceduralOverrides[subjectId] = 0; PortraitUpdated?.Invoke(this, new(subjectId, null)); }

    public async ValueTask<GeneratedPortraitSnapshot?> CaptureHistoricalAsync(string eventId, CharacterVisualState visual, CancellationToken cancellationToken = default)
    {
        GeneratedPortraitSelection? current = GetCurrent(visual);
        if (current is null) return null;
        await cache.PinAsync(current.Asset.Record.AssetHash, cancellationToken).ConfigureAwait(false);
        return new(eventId, current with { Staleness = GeneratedPortraitStaleness.Historical }, DateTimeOffset.UtcNow);
    }
}

public sealed class GeneratedPortraitUpdatedEventArgs(string subjectId, GeneratedPortraitSelection? selection) : EventArgs
{
    public string SubjectId { get; } = subjectId;
    public GeneratedPortraitSelection? Selection { get; } = selection;
}
