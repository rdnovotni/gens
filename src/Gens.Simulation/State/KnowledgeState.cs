using Gens.Simulation.Time;

namespace Gens.Simulation.State;

/// <summary>Identifies one observer's knowledge of one subject's one topic, e.g. (player, char_0000042, "location").</summary>
public readonly record struct KnowledgeKey(string ObserverId, string SubjectId, string Topic);

/// <summary>One fact an observer knows, at some confidence, as of some date, traced to its source event.</summary>
public readonly record struct KnowledgeEntry(object Value, KnowledgeConfidence Confidence, GameDate AsOfDate, string? ProvenanceEventId);

/// <summary>
/// Truth-separate-from-knowledge partition (ADR 0008). Phase 2 provides storage only — a place for
/// domain events' <c>Visibility</c> field to eventually write into and for read-model queries
/// (ADR 0013) to filter through. Propagation from events, confidence decay, and staleness are out of
/// scope until a later phase emits events with real payloads to propagate.
/// </summary>
public sealed class KnowledgeState
{
    private readonly SortedDictionary<KnowledgeKey, KnowledgeEntry> _entries = new(KnowledgeKeyComparer.Instance);

    public int Count => _entries.Count;

    /// <summary>Bumped on every write. Used by debug-only write-set verification (ADR 0005); not a
    /// save-relevant field.</summary>
    public long Version { get; private set; }

    public void Set(KnowledgeKey key, KnowledgeEntry entry)
    {
        _entries[key] = entry;
        Version++;
    }

    public bool TryGet(KnowledgeKey key, out KnowledgeEntry entry) => _entries.TryGetValue(key, out entry);

    /// <summary>Every fact this observer knows, in deterministic (ADR 0004) key order.</summary>
    public IEnumerable<KeyValuePair<KnowledgeKey, KnowledgeEntry>> ForObserver(string observerId) =>
        _entries.Where(entry => string.Equals(entry.Key.ObserverId, observerId, StringComparison.Ordinal));

    /// <summary>Every entry, in deterministic (ADR 0004) key order.</summary>
    public IEnumerable<KeyValuePair<KnowledgeKey, KnowledgeEntry>> All() => _entries;

    /// <summary>Rebuilds knowledge state from persisted save data (ADR 0010). <paramref name="entries"/>
    /// may arrive in any order — the backing <see cref="SortedDictionary{TKey,TValue}"/> re-establishes
    /// deterministic key order regardless.</summary>
    public static KnowledgeState Restore(IEnumerable<KeyValuePair<KnowledgeKey, KnowledgeEntry>> entries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        var state = new KnowledgeState();
        foreach (var entry in entries)
            state.Set(entry.Key, entry.Value);

        state.Version = 0;
        return state;
    }

    private sealed class KnowledgeKeyComparer : IComparer<KnowledgeKey>
    {
        public static readonly KnowledgeKeyComparer Instance = new();

        public int Compare(KnowledgeKey x, KnowledgeKey y)
        {
            var observer = string.CompareOrdinal(x.ObserverId, y.ObserverId);
            if (observer != 0)
                return observer;

            var subject = string.CompareOrdinal(x.SubjectId, y.SubjectId);
            return subject != 0 ? subject : string.CompareOrdinal(x.Topic, y.Topic);
        }
    }
}
