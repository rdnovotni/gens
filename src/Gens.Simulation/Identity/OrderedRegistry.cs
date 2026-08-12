using System.Diagnostics.CodeAnalysis;

namespace Gens.Simulation.Identity;

/// <summary>
/// The standard <c>WorldState</c> collection shape (ADR 0004): guarantees ascending-<typeparamref
/// name="TId"/> iteration regardless of insertion order. Because runtime IDs are issued by a
/// strictly increasing counter (ADR 0001), ascending-ID order is equivalent to creation order — a
/// free, deterministic default ordering every system gets without extra bookkeeping. The backing
/// data structure is an implementation detail; the contract is the ordering guarantee.
/// </summary>
public sealed class OrderedRegistry<TId, TEntity>
    where TId : notnull, IComparable<TId>
{
    private readonly SortedList<TId, TEntity> _entries = new();

    public int Count => _entries.Count;

    /// <summary>Bumped on every structural mutation (add/remove). Used by debug-only write-set
    /// verification (ADR 0005) to detect an undeclared mutation; not a save-relevant field.</summary>
    public long Version { get; private set; }

    public void Add(TId id, TEntity entity)
    {
        if (_entries.ContainsKey(id))
            throw new ArgumentException($"An entity with ID '{id}' is already registered.", nameof(id));
        _entries.Add(id, entity);
        Version++;
    }

    public bool TryGet(TId id, out TEntity entity)
    {
        if (_entries.TryGetValue(id, out var found))
        {
            entity = found;
            return true;
        }

        entity = default!;
        return false;
    }

    public bool Remove(TId id)
    {
        var removed = _entries.Remove(id);
        if (removed)
            Version++;
        return removed;
    }

    /// <summary>Iterates every entry in ascending <typeparamref name="TId"/> order.</summary>
    public IEnumerable<KeyValuePair<TId, TEntity>> InAscendingOrder() => _entries;

    /// <summary>Rebuilds a registry from persisted save data (ADR 0010). <paramref name="entries"/> may
    /// arrive in any order — the backing <see cref="SortedList{TKey,TValue}"/> re-establishes ascending
    /// <typeparamref name="TId"/> order regardless, matching this type's own ordering contract.</summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "TId/TEntity are fixed by the caller's context (a save reader restoring a " +
        "specific WorldState partition); an instance-based factory would need an unused instance to " +
        "call it on, matching RuntimeIdCounter<T>.Restore's identical justification.")]
    public static OrderedRegistry<TId, TEntity> Restore(IEnumerable<KeyValuePair<TId, TEntity>> entries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        var registry = new OrderedRegistry<TId, TEntity>();
        foreach (var entry in entries)
            registry.Add(entry.Key, entry.Value);

        // Restoring is not a structural "mutation" in the write-set-verification sense (ADR 0005) —
        // it is establishing the starting state a tick then mutates from — so reset Version to 0
        // rather than leaving it at entries.Count from the Add calls above.
        registry.Version = 0;
        return registry;
    }
}
