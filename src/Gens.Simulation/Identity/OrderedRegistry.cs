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
}
