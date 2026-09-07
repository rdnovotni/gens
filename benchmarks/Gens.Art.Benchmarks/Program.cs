using System.Diagnostics;
using Gens.Art;
using Gens.Art.Cache;
using Gens.Art.Providers;
using Gens.Art.Queue;

string cacheRoot = Path.Combine(Path.GetTempPath(), "gens-art-benchmark", Guid.NewGuid().ToString("N"));
try
{
    foreach (int count in new[] { 100, 1_000 })
    {
        string runRoot = Path.Combine(cacheRoot, count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        using var cache = new GeneratedArtCache(runRoot);
        var provider = new MockArtProvider(new(TimeSpan.Zero, MaximumConcurrentRequests: 8));
        await using var queue = new ArtGenerationQueue(provider, cache, new(MaximumConcurrency: 8));
        var watch = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0, count).Select(i => queue.EnqueueAsync(Request(i), ArtPriority.Precache)));
        watch.Stop();
        ArtCacheStatistics statistics = cache.GetStatistics();
        Console.WriteLine($"queue {count}: {watch.Elapsed.TotalMilliseconds:F1} ms; provider calls {provider.CallCount}; cache bytes {statistics.ObjectBytes}");
        ArtGenerationRequest duplicate = Request(0); watch.Restart();
        for (int i = 0; i < count; i++) await queue.EnqueueAsync(duplicate, ArtPriority.VisiblePortrait);
        watch.Stop();
        Console.WriteLine($"cache lookup {count}: {watch.Elapsed.TotalMilliseconds:F1} ms; provider calls still {provider.CallCount}");
    }
}
finally { if (Directory.Exists(cacheRoot)) Directory.Delete(cacheRoot, true); }

static ArtGenerationRequest Request(int id)
{
    var prompt = new CompiledArtPrompt($"benchmark subject {id}", "Roman Republic", "period clothing", "portrait", "natural light", "painted realism", ["coherent"], ["text"]);
    return new(ArtRequestId.New(), ArtPurpose.CharacterPortrait, $"subject-{id}", $"visual-{id}", new("painted-realism"), prompt, 8, 8, (ulong)id,
        CharacterPortraitPromptCompiler.CompilerVersion, "benchmark-v1", new("benchmark", ArtSafetyProfile.StandardPortrait), new Dictionary<string, string>());
}
