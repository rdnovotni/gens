using Gens.Art.Cache;
using Gens.Art.Portraits;
using Gens.Art.Providers;
using Gens.Art.Queue;
using Gens.Application.Campaign;
using Gens.Graphics.Skia;
using Gens.Portraits;
using Gens.Presentation.Visuals;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Art.Tests;

public sealed class ArtPipelineTests
{
    private string cacheRoot = null!;
    private static readonly CharacterVisualProfile Profile = new()
    {
        Height = Height.Tall,
        Build = Build.Slight,
        FacialStructure = FacialStructure.Angular,
        Complexion = Complexion.Olive,
        HairColor = HairColor.Brown,
        HairStyle = HairStyle.Cropped,
        EyeColor = EyeColor.Hazel,
        NotableFeatures = [NotableFeature.Scar],
        Portrait = PortraitRecipeGenerator.Generate(Height.Tall, Build.Slight, FacialStructure.Angular, Complexion.Olive, HairColor.Brown, HairStyle.Cropped, EyeColor.Hazel, [NotableFeature.Scar]),
    };

    [SetUp] public void SetUp() { cacheRoot = Path.Combine(Path.GetTempPath(), "gens-art-tests", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(cacheRoot)) Directory.Delete(cacheRoot, true); }

    [Test]
    public void PromptAndFingerprintAreDeterministicAndStructured()
    {
        CharacterPortraitSubject subject = Subject(); var compiler = new CharacterPortraitPromptCompiler();
        var profile = new ArtGenerationProfile("standard", ArtSafetyProfile.StandardPortrait, new Dictionary<string, string> { ["b"] = "2", ["a"] = "1" });
        CompiledArtPrompt first = compiler.Compile(subject, new("painted-realism"), profile);
        CompiledArtPrompt second = compiler.Compile(subject, new("painted-realism"), profile);
        ArtGenerationRequest a = Request(first, profile, new Dictionary<string, string> { ["z"] = "9", ["a"] = "1" });
        ArtGenerationRequest b = Request(second, profile, new Dictionary<string, string> { ["a"] = "1", ["z"] = "9" });
        Assert.Multiple(() => { Assert.That(first.PositiveText, Is.EqualTo(second.PositiveText)); Assert.That(first.NegativeText, Is.EqualTo(second.NegativeText)); Assert.That(first.NegativeConstraints, Does.Contain("watermarks")); Assert.That(a.RequestFingerprint, Is.EqualTo(b.RequestFingerprint)); Assert.That(a.RequestFingerprint, Has.Length.EqualTo(64)); });
    }

    [Test]
    public async Task QueueDeduplicatesConcurrentRequestsAndCacheHitSkipsProvider()
    {
        var provider = new MockArtProvider(new(TimeSpan.FromMilliseconds(50))); var cache = new GeneratedArtCache(cacheRoot);
        await using var queue = new ArtGenerationQueue(provider, cache); ArtGenerationRequest request = Request();
        CachedArtAsset?[] results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => queue.EnqueueAsync(request, ArtPriority.VisiblePortrait)));
        CachedArtAsset? hit = await queue.EnqueueAsync(request, ArtPriority.VisiblePortrait);
        Assert.Multiple(() => { Assert.That(provider.CallCount, Is.EqualTo(1)); Assert.That(results.Select(static result => result!.Record.AssetHash).Distinct().Count(), Is.EqualTo(1)); Assert.That(hit, Is.Not.Null); Assert.That(queue.GetStatistics().CacheHits, Is.EqualTo(1)); });
    }

    [Test]
    public async Task QueueRetriesTransientFailuresButNotPermanentRejection()
    {
        var cache = new GeneratedArtCache(cacheRoot); var transient = new MockArtProvider(new(TimeSpan.Zero, TransientFailuresBeforeSuccess: 2));
        await using (var queue = new ArtGenerationQueue(transient, cache, new(2, 2, TimeSpan.FromMilliseconds(1)))) Assert.That(await queue.EnqueueAsync(Request(), ArtPriority.VisiblePortrait), Is.Not.Null);
        string secondRoot = cacheRoot + "-rejected"; var rejected = new MockArtProvider(new(TimeSpan.Zero, PermanentFailure: ArtFailureKind.Rejected));
        try { await using var queue = new ArtGenerationQueue(rejected, new GeneratedArtCache(secondRoot), new(2, 5, TimeSpan.FromMilliseconds(1))); Assert.That(await queue.EnqueueAsync(Request(metadata: new Dictionary<string, string> { ["case"] = "rejected" }), ArtPriority.VisiblePortrait), Is.Null); }
        finally { if (Directory.Exists(secondRoot)) Directory.Delete(secondRoot, true); }
        Assert.Multiple(() => { Assert.That(transient.CallCount, Is.EqualTo(3)); Assert.That(rejected.CallCount, Is.EqualTo(1)); });
    }

    [Test]
    public void CancellationReachesProviderAndAcceptsNoAsset()
    {
        Assert.That(async () =>
        {
            var provider = new MockArtProvider(new(TimeSpan.FromSeconds(5))); await using var queue = new ArtGenerationQueue(provider, new GeneratedArtCache(cacheRoot));
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await queue.EnqueueAsync(Request(), ArtPriority.VisiblePortrait, cancellation.Token);
        }, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task QueueHonorsConfiguredConcurrency()
    {
        var provider = new MockArtProvider(new(TimeSpan.FromMilliseconds(40))); await using var queue = new ArtGenerationQueue(provider, new GeneratedArtCache(cacheRoot), new(MaximumConcurrency: 2));
        await Task.WhenAll(Enumerable.Range(0, 10).Select(i => queue.EnqueueAsync(Request(metadata: new Dictionary<string, string> { ["number"] = i.ToString(System.Globalization.CultureInfo.InvariantCulture) }), ArtPriority.BackgroundPortrait)));
        Assert.That(provider.MaximumObservedConcurrency, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task CorruptOrMissingCacheFallsBackToMissWithoutCrashing()
    {
        var provider = new MockArtProvider(); var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(provider, cache); ArtGenerationRequest request = Request();
        CachedArtAsset asset = (await queue.EnqueueAsync(request, ArtPriority.VisiblePortrait))!; File.WriteAllBytes(asset.ObjectPath, [1, 2, 3]);
        Assert.That(await cache.FindAsync(request.RequestFingerprint), Is.Null);
        File.Delete(asset.ObjectPath);
        Assert.That(await cache.FindAsync(request.RequestFingerprint), Is.Null);
        Assert.That(cache.GetStatistics().Corruptions, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task NonAsciiCacheRootRoundTripsStoreFindPinAndClear()
    {
        string unicodeRoot = Path.Combine(cacheRoot, "用户-δοκιμή");
        var cache = new GeneratedArtCache(unicodeRoot); await using var queue = new ArtGenerationQueue(new MockArtProvider(), cache);
        ArtGenerationRequest request = Request();
        CachedArtAsset stored = (await queue.EnqueueAsync(request, ArtPriority.VisiblePortrait))!;
        await cache.PinAsync(stored.Record.AssetHash);
        await cache.ClearUnpinnedAsync();
        CachedArtAsset? found = await cache.FindAsync(request.RequestFingerprint);
        Assert.Multiple(() =>
        {
            Assert.That(found, Is.Not.Null);
            Assert.That(File.Exists(stored.ObjectPath), Is.True);
        });
    }

    // Simulating an unwritable directory via permission bits is unreliable here (this suite may run
    // as root, which bypasses DAC checks, and Windows ACL denial needs elevation to set up). A
    // corrupted index entry is a structural failure that reproduces identically on every platform
    // and exercises the same catch clause an unwritable/locked index file would hit.
    [Test]
    public async Task CorruptIndexEntryDoesNotThrowOnPinOrClearUnpinned()
    {
        var provider = new MockArtProvider(); var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(provider, cache);
        CachedArtAsset stored = (await queue.EnqueueAsync(Request(), ArtPriority.VisiblePortrait))!;
        string indexPath = Path.Combine(cacheRoot, "index", stored.Record.RequestFingerprint[..2], stored.Record.RequestFingerprint + ".json");
        File.WriteAllText(indexPath, "{ not valid json");

        string? loggedMessage = null;
        var guardedCache = new GeneratedArtCache(cacheRoot, (message, _) => loggedMessage = message);
        Assert.DoesNotThrowAsync(() => guardedCache.PinAsync(stored.Record.AssetHash).AsTask());
        Assert.That(loggedMessage, Is.Not.Null);
        loggedMessage = null;
        Assert.DoesNotThrowAsync(() => guardedCache.ClearUnpinnedAsync().AsTask());
        Assert.That(loggedMessage, Is.Not.Null);
    }

    [Test]
    public async Task PortraitAlwaysReturnsProceduralAndPublishesGeneratedReplacement()
    {
        var provider = new MockArtProvider(new(TimeSpan.FromMilliseconds(20))); var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(provider, cache); var service = new GeneratedPortraitCoordinator(queue, cache); CharacterPortraitSubject subject = Subject();
        Assert.That(service.GetCurrent(subject.VisualState), Is.Null, "Null means the permanent procedural service remains selected immediately."); int updates = 0; service.PortraitUpdated += (_, _) => updates++;
        GeneratedPortraitSelection? generated = await service.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "mock" }); service.UseProcedural(subject.SubjectId);
        Assert.Multiple(() => { Assert.That(generated, Is.Not.Null); Assert.That(service.GetCurrent(subject.VisualState), Is.Null); Assert.That(updates, Is.EqualTo(2)); });
    }

    [Test]
    public async Task MissingProviderLeavesProceduralPortraitActive()
    {
        var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(new NullArtProvider(), cache); var service = new GeneratedPortraitCoordinator(queue, cache); CharacterPortraitSubject subject = Subject();
        GeneratedPortraitSelection? result = await service.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "none", ExternalGenerationConsent = true });
        Assert.That(result, Is.Null, "The caller continues using its procedural portrait.");
    }

    [Test]
    public async Task HistoricalPortraitIsPinnedAndCurrentPortraitMayChangeIndependently()
    {
        var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(new MockArtProvider(), cache); var service = new GeneratedPortraitCoordinator(queue, cache); CharacterPortraitSubject subject = Subject();
        GeneratedPortraitSelection original = (await service.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "mock" }))!; GeneratedPortraitSnapshot snapshot = (await service.CaptureHistoricalAsync("event-1", subject.VisualState))!;
        GeneratedPortraitSelection regenerated = (await service.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "mock" }, regenerate: true))!;
        Assert.Multiple(() => { Assert.That(snapshot.Selection.Staleness, Is.EqualTo(GeneratedPortraitStaleness.Historical)); Assert.That(snapshot.Selection.Asset.Record.AssetHash, Is.EqualTo(original.Asset.Record.AssetHash)); Assert.That(regenerated.Asset.Record.RequestFingerprint, Is.Not.EqualTo(original.Asset.Record.RequestFingerprint)); });
    }

    [Test]
    public async Task StalenessDistinguishesMinorAndMajorChanges()
    {
        CharacterPortraitSubject subject = Subject(); var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(new MockArtProvider(), cache); var service = new GeneratedPortraitCoordinator(queue, cache);
        Assert.That(await service.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "mock" }), Is.Not.Null);
        CharacterVisualState minor = Visual(Profile, 43, DutySlot.Cook);
        CharacterVisualProfile changed = Profile with { HairColor = HairColor.Gray };
        CharacterVisualState major = Visual(changed, 63);
        Assert.Multiple(() => { Assert.That(service.GetCurrent(minor)!.Staleness, Is.EqualTo(GeneratedPortraitStaleness.StaleMinor)); Assert.That(service.GetCurrent(major)!.Staleness, Is.EqualTo(GeneratedPortraitStaleness.StaleMajor)); });
    }

    [Test]
    public void SimulationHasNoExternalArtProviderSeamAndArtDoesNotMutateAuthoritativeState()
    {
        string root = FindRoot(); string simulation = Path.Combine(root, "src", "Gens.Simulation");
        Assert.Multiple(() => { Assert.That(Directory.GetFiles(simulation, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText), Has.None.Contains("IArtGenerationProvider")); Assert.That(typeof(GeneratedArtRecord).Assembly, Is.Not.EqualTo(typeof(CharacterVisualProfile).Assembly)); });
    }

    [Test]
    public async Task LocalWorkerCrashAndCloudOutageBecomeStructuredFailures()
    {
        using var client = new HttpClient(new ThrowingHandler()) { Timeout = TimeSpan.FromSeconds(1) };
        var local = new LocalWorkerArtProvider(client, new Uri("http://127.0.0.1:49321/"));
        var cloud = new GensBackendArtProvider(client, new Uri("https://art.gens.invalid/"), _ => ValueTask.FromResult<string?>("test-token"));
        ArtGenerationResult localResult = await local.GenerateAsync(Request(), CancellationToken.None);
        ArtGenerationResult cloudResult = await cloud.GenerateAsync(Request(), CancellationToken.None);
        Assert.Multiple(() => { Assert.That(localResult.FailureKind, Is.EqualTo(ArtFailureKind.WorkerCrashed)); Assert.That(cloudResult.FailureKind, Is.EqualTo(ArtFailureKind.Network)); });
    }

    [Test]
    public async Task ProviderNondeterminismAndGeneratedReferencesDoNotChangeCampaignHash()
    {
        CampaignSession first = Campaign(); CampaignSession second = Campaign();
        ulong before = StateHasher.Hash(first.State);
        using var firstCache = new GeneratedArtCache(cacheRoot + "-variant-a"); using var secondCache = new GeneratedArtCache(cacheRoot + "-variant-b");
        try
        {
            await using var firstQueue = new ArtGenerationQueue(new MockArtProvider(new(TimeSpan.Zero, OutputVariant: "a")), firstCache);
            await using var secondQueue = new ArtGenerationQueue(new MockArtProvider(new(TimeSpan.Zero, OutputVariant: "b")), secondCache);
            CachedArtAsset a = (await firstQueue.EnqueueAsync(Request(), ArtPriority.VisiblePortrait))!;
            CachedArtAsset b = (await secondQueue.EnqueueAsync(Request(), ArtPriority.VisiblePortrait))!;
            first.AdvanceMonth([]); second.AdvanceMonth([]);
            Assert.Multiple(() => { Assert.That(a.Record.AssetHash, Is.Not.EqualTo(b.Record.AssetHash)); Assert.That(StateHasher.Hash(first.State), Is.EqualTo(StateHasher.Hash(second.State))); Assert.That(before, Is.Not.EqualTo(StateHasher.Hash(first.State))); });
        }
        finally { if (Directory.Exists(cacheRoot + "-variant-a")) Directory.Delete(cacheRoot + "-variant-a", true); if (Directory.Exists(cacheRoot + "-variant-b")) Directory.Delete(cacheRoot + "-variant-b", true); }
    }

    [Test]
    public async Task ProductionPortraitServicePrefersGeneratedThenRevertsToProcedural()
    {
        using var cache = new GeneratedArtCache(cacheRoot); await using var queue = new ArtGenerationQueue(new MockArtProvider(), cache); var coordinator = new GeneratedPortraitCoordinator(queue, cache); CharacterPortraitSubject subject = Subject();
        using var graphics = new SkiaGraphicsBackend(); using var portraits = new PortraitService(graphics, cacheRoot + "-procedural", coordinator);
        Assert.That(portraits.Resolve(subject.VisualState, 128).Reference.SourceKind, Is.EqualTo(PortraitSourceKind.Procedural));
        Assert.That(await coordinator.RequestAsync(subject.VisualState, subject.Appearance, new() { AiGenerationEnabled = true, Provider = "mock" }), Is.Not.Null);
        Assert.That(portraits.Resolve(subject.VisualState, 128).Reference.SourceKind, Is.EqualTo(PortraitSourceKind.Generated));
        coordinator.UseProcedural(subject.SubjectId);
        Assert.That(portraits.Resolve(subject.VisualState, 128).Reference.SourceKind, Is.EqualTo(PortraitSourceKind.Procedural));
    }

    private static CharacterPortraitSubject Subject() { CharacterVisualState visual = Visual(Profile, 43); return new(visual, AppearanceDescriptionBuilder.Build(visual)); }
    private static CharacterVisualState Visual(CharacterVisualProfile profile, int age, DutySlot? duty = null) => CharacterVisualStateProjector.Project("character-1", Sex.Male, age, profile, LegalStatus.RomanCitizen, SocialClass.Senatorial, duty);
    private static ArtGenerationRequest Request(CompiledArtPrompt? prompt = null, ArtGenerationProfile? profile = null, IReadOnlyDictionary<string, string>? metadata = null)
    {
        CharacterPortraitSubject subject = Subject(); var compiler = new CharacterPortraitPromptCompiler(); profile ??= new("standard", ArtSafetyProfile.StandardPortrait); prompt ??= compiler.Compile(subject, new("painted-realism"), profile);
        return new(ArtRequestId.New(), ArtPurpose.CharacterPortrait, subject.SubjectId, subject.VisualStateHash, new("painted-realism"), prompt, 64, 64, 7, compiler.Version, "recipe-v1", profile, metadata ?? new Dictionary<string, string>());
    }
    private static string FindRoot() { DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Gens.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
    private static CampaignSession Campaign() => CampaignSession.CreateNew(new CampaignConfig { Seed = 4242, StartDate = new GameDate(0), RulesetId = "default", ContentPackHash = "test-content", RegionId = "latium", Difficulty = "standard" }, out _);
    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromException<HttpResponseMessage>(new HttpRequestException("offline"));
    }
}
