using Gens.Art.Cache;
using Gens.Art.Portraits;
using Gens.Art.Queue;
using Gens.Presentation.Visuals;

namespace Gens.Art.Diagnostics;

public sealed record ArtDiagnosticsSnapshot(bool AiEnabled, string SelectedProvider, bool ProviderAvailable, ArtQueueStatistics Queue, ArtCacheStatistics Cache, int StalePortraits, IReadOnlyList<ArtJobSnapshot> Requests);

public sealed class ArtDiagnostics(ArtGenerationQueue queue, IGeneratedArtCache cache, IArtProvider provider)
{
    public ArtDiagnosticsSnapshot Capture(ArtSettings settings, IEnumerable<GeneratedPortraitSelection>? portraits = null) => new(
        settings.AiGenerationEnabled, settings.Provider, provider.Capabilities.SupportedPurposes.Count > 0,
        queue.GetStatistics(), cache.GetStatistics(), portraits?.Count(static portrait => portrait.Staleness is GeneratedPortraitStaleness.StaleMajor or GeneratedPortraitStaleness.StaleMinor) ?? 0, queue.Inspect());
}
