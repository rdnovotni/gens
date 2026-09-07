using Gens.Art.Cache;
using Gens.Presentation;

namespace Gens.Art.Queue;

public sealed record ArtQueueOptions(int MaximumConcurrency = 2, int MaximumAutomaticRetries = 2, TimeSpan? InitialRetryDelay = null)
{
    public TimeSpan RetryDelay => InitialRetryDelay ?? TimeSpan.FromSeconds(1);
}

public sealed record ArtJobSnapshot(string Fingerprint, string SubjectId, ArtJobStatus Status, ArtPriority Priority, int AttemptCount, string? AssetHash, ArtFailureKind? FailureKind, string? Diagnostic);
public readonly record struct ArtQueueStatistics(int Queued, int Running, long Completed, long Failed, long Cancelled, long CacheHits, long CacheMisses);

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Ticket contract and domain language require ArtGenerationQueue.")]
public sealed class ArtGenerationQueue : IAsyncDisposable
{
    private readonly IArtProvider provider;
    private readonly IGeneratedArtCache cache;
    private readonly ArtQueueOptions options;
    private readonly object sync = new();
    private readonly SortedSet<Job> pending = new(JobComparer.Instance);
    private readonly Dictionary<string, Job> active = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ArtJobSnapshot> history = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim signal = new(0);
    private readonly CancellationTokenSource shutdown = new();
    private readonly Task[] workers;
    private long sequence, completed, failed, cancelled, cacheHits, cacheMisses;
    private int running;

    public ArtGenerationQueue(IArtProvider provider, IGeneratedArtCache cache, ArtQueueOptions? options = null)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider)); this.cache = cache ?? throw new ArgumentNullException(nameof(cache)); this.options = options ?? new();
        int concurrency = Math.Max(1, Math.Min(this.options.MaximumConcurrency, Math.Max(1, provider.Capabilities.MaximumConcurrentRequests)));
        workers = Enumerable.Range(0, concurrency).Select(_ => Task.Run(WorkerAsync)).ToArray();
    }

    public event EventHandler<ArtJobSnapshot>? JobChanged;

    public async Task<CachedArtAsset?> EnqueueAsync(ArtGenerationRequest request, ArtPriority priority, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request); cancellationToken.ThrowIfCancellationRequested();
        CachedArtAsset? cached = await cache.FindAsync(request.RequestFingerprint, cancellationToken).ConfigureAwait(false);
        if (cached is not null) { Interlocked.Increment(ref cacheHits); return cached; }
        Interlocked.Increment(ref cacheMisses);
        Job job;
        lock (sync)
        {
            if (active.TryGetValue(request.RequestFingerprint, out job!) && job.Completion.Task.IsCompletedSuccessfully)
            {
                CachedArtAsset? completedAsset = job.Completion.Task.Result;
                if (completedAsset is not null && File.Exists(completedAsset.ObjectPath)) return completedAsset;
                active.Remove(request.RequestFingerprint);
            }
            if (!active.TryGetValue(request.RequestFingerprint, out job!))
            {
                job = new(request, priority, Interlocked.Increment(ref sequence)); active.Add(request.RequestFingerprint, job); pending.Add(job);
                PublishLocked(job, ArtJobStatus.Queued); signal.Release();
            }
        }
        using CancellationTokenRegistration registration = cancellationToken.Register(static state => ((Job)state!).Cancellation.Cancel(), job);
        return await job.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public bool Cancel(string fingerprint)
    {
        lock (sync) { if (!active.TryGetValue(fingerprint, out Job? job)) return false; job.Cancellation.Cancel(); signal.Release(); return true; }
    }

    public IReadOnlyList<ArtJobSnapshot> Inspect() { lock (sync) return history.Values.OrderBy(static item => item.SubjectId, StringComparer.Ordinal).ThenBy(static item => item.Fingerprint, StringComparer.Ordinal).ToArray(); }
    public ArtQueueStatistics GetStatistics() { lock (sync) return new(pending.Count, running, completed, failed, cancelled, cacheHits, cacheMisses); }

    private async Task WorkerAsync()
    {
        while (!shutdown.IsCancellationRequested)
        {
            try { await signal.WaitAsync(shutdown.Token).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
            Job? job = null;
            lock (sync)
            {
                if (pending.Count > 0) { job = pending.Min!; pending.Remove(job); running++; PublishLocked(job, ArtJobStatus.Generating); }
            }
            if (job is null) continue;
            await ExecuteAsync(job).ConfigureAwait(false);
            lock (sync) running--;
        }
    }

    private async Task ExecuteAsync(Job job)
    {
        try
        {
            if (!provider.Capabilities.Supports(job.Request)) { CompleteFailure(job, ArtFailureKind.Unsupported, "Selected provider does not support this request."); return; }
            ArtGenerationResult result;
            while (true)
            {
                job.Attempts++;
                result = await provider.GenerateAsync(job.Request, job.Cancellation.Token).ConfigureAwait(false);
                if (result.Succeeded) break;
                if (!IsTransient(result.FailureKind) || job.Attempts > options.MaximumAutomaticRetries) { CompleteFailure(job, result.FailureKind ?? ArtFailureKind.ProviderError, result.Diagnostic); return; }
                lock (sync) PublishLocked(job, ArtJobStatus.RetryScheduled, result.FailureKind, result.Diagnostic);
                TimeSpan delay = TimeSpan.FromMilliseconds(Math.Min(options.RetryDelay.TotalMilliseconds * Math.Pow(2, job.Attempts - 1), 30_000));
                await Task.Delay(delay, job.Cancellation.Token).ConfigureAwait(false);
                lock (sync) PublishLocked(job, ArtJobStatus.Generating);
            }
            try
            {
                CachedArtAsset asset = await cache.StoreAsync(job.Request, provider, result.Output!, job.Cancellation.Token).ConfigureAwait(false);
                lock (sync) { Interlocked.Increment(ref completed); PublishLocked(job, ArtJobStatus.Completed, assetHash: asset.Record.AssetHash); }
                job.Completion.TrySetResult(asset);
            }
            catch (InvalidDataException exception) { CompleteFailure(job, ArtFailureKind.InvalidOutput, exception.Message); }
        }
        catch (OperationCanceledException)
        {
            lock (sync) { Interlocked.Increment(ref cancelled); PublishLocked(job, ArtJobStatus.Cancelled, ArtFailureKind.Cancelled, "Generation cancelled."); FinishLocked(job); }
            job.Completion.TrySetCanceled();
        }
        catch (Exception exception) when (exception is IOException or HttpRequestException)
        {
            CompleteFailure(job, ArtFailureKind.ProviderError, exception.Message);
        }
    }

    private void CompleteFailure(Job job, ArtFailureKind kind, string? diagnostic)
    {
        lock (sync) { Interlocked.Increment(ref failed); PublishLocked(job, ArtJobStatus.Failed, kind, diagnostic); FinishLocked(job); }
        job.Completion.TrySetResult(null);
    }

    private void FinishLocked(Job job) { active.Remove(job.Request.RequestFingerprint); job.Cancellation.Dispose(); }
    private void PublishLocked(Job job, ArtJobStatus status, ArtFailureKind? kind = null, string? diagnostic = null, string? assetHash = null)
    {
        var snapshot = new ArtJobSnapshot(job.Request.RequestFingerprint, job.Request.SubjectId, status, job.Priority, job.Attempts, assetHash, kind, diagnostic);
        history[job.Request.RequestFingerprint] = snapshot; JobChanged?.Invoke(this, snapshot);
    }
    private static bool IsTransient(ArtFailureKind? kind) => kind is ArtFailureKind.Timeout or ArtFailureKind.RateLimited or ArtFailureKind.Network or ArtFailureKind.ProviderError or ArtFailureKind.WorkerCrashed or ArtFailureKind.Unavailable;

    public async ValueTask DisposeAsync()
    {
        shutdown.Cancel(); lock (sync) foreach (Job job in active.Values) job.Cancellation.Cancel();
        try { await Task.WhenAll(workers).ConfigureAwait(false); } catch (OperationCanceledException) { }
        shutdown.Dispose(); signal.Dispose();
    }

    private sealed class Job(ArtGenerationRequest request, ArtPriority priority, long sequence)
    {
        public ArtGenerationRequest Request { get; } = request; public ArtPriority Priority { get; } = priority; public long Sequence { get; } = sequence;
        public CancellationTokenSource Cancellation { get; } = new(); public TaskCompletionSource<CachedArtAsset?> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously); public int Attempts { get; set; }
    }
    private sealed class JobComparer : IComparer<Job>
    {
        public static JobComparer Instance { get; } = new();
        public int Compare(Job? x, Job? y) { if (ReferenceEquals(x, y)) return 0; if (x is null) return -1; if (y is null) return 1; int priority = x.Priority.CompareTo(y.Priority); return priority != 0 ? priority : x.Sequence.CompareTo(y.Sequence); }
    }
}
