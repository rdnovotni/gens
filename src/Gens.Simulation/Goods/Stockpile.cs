using Gens.Simulation.Identity;

namespace Gens.Simulation.Goods;

/// <summary>
/// Capacity-bounded storage for fungible goods. Batches retain the dimensions that prevent unsafe
/// merging (grade, condition, age, and provenance), while reservations protect quantities for
/// construction and production without pretending that the goods have left storage.
/// </summary>
public sealed class Stockpile
{
    private readonly List<StockLot> _lots = new();
    private readonly SortedDictionary<string, StockReservation> _reservations = new(StringComparer.Ordinal);

    public Stockpile(long capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Stockpile capacity must be positive.");
        Capacity = capacity;
    }

    public long Capacity { get; }
    public long Quantity => _lots.Sum(lot => lot.Quantity);
    public long RemainingCapacity => Capacity - Quantity;
    public IReadOnlyList<StockLot> Lots => _lots;
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.Values;

    public void Add(
        GoodDefinition good,
        long quantity,
        GoodQuality? quality = null,
        StockCondition? condition = null,
        StockProvenance? provenance = null,
        int ageInTicks = 0)
    {
        if (good is null)
            throw new ArgumentNullException(nameof(good));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Added quantity must be positive.");
        if (quantity > RemainingCapacity)
            throw new InvalidOperationException("The stockpile does not have enough remaining capacity.");
        if (ageInTicks < 0)
            throw new ArgumentOutOfRangeException(nameof(ageInTicks), ageInTicks, "Stock age cannot be negative.");
        if (good.QualityEligible != (quality is not null))
            throw new ArgumentException(good.QualityEligible
                ? "A quality-eligible good requires a quality."
                : "This good does not track displayed quality.", nameof(quality));
        if (good.ConditionTracked != (condition is not null))
            throw new ArgumentException(good.ConditionTracked
                ? "A condition-tracked good requires a condition."
                : "This good does not track condition.", nameof(condition));

        _lots.Add(new StockLot(good, quantity, quality, condition, ageInTicks, provenance));
    }

    public long QuantityOf(DefinitionId<Good> goodId, GoodQuality? quality = null) =>
        _lots.Where(lot => lot.Good.Id == goodId && (quality is null || lot.Quality == quality)).Sum(lot => lot.Quantity);

    public long ReservedQuantityOf(DefinitionId<Good> goodId, GoodQuality? quality = null) =>
        _reservations.Values.Where(r => r.GoodId == goodId && (quality is null || r.Quality == quality)).Sum(r => r.Quantity);

    public long AvailableQuantityOf(DefinitionId<Good> goodId, GoodQuality? quality = null) =>
        QuantityOf(goodId, quality) - ReservedQuantityOf(goodId, quality);

    public void Reserve(string reservationId, DefinitionId<Good> goodId, long quantity, GoodQuality? quality = null)
    {
        ValidateReservationId(reservationId);
        RequireQualitySelectorWhenNeeded(goodId, quality);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Reserved quantity must be positive.");
        if (_reservations.ContainsKey(reservationId))
            throw new ArgumentException($"Reservation '{reservationId}' already exists.", nameof(reservationId));
        if (quantity > AvailableQuantityOf(goodId, quality))
            throw new InvalidOperationException("There is not enough unreserved stock.");

        _reservations.Add(reservationId, new StockReservation(reservationId, goodId, quality, quantity));
    }

    public bool Release(string reservationId)
    {
        ValidateReservationId(reservationId);
        return _reservations.Remove(reservationId);
    }

    public IReadOnlyList<StockMovement> ConsumeReserved(string reservationId)
    {
        ValidateReservationId(reservationId);
        if (!_reservations.TryGetValue(reservationId, out var reservation))
            throw new KeyNotFoundException($"Reservation '{reservationId}' does not exist.");

        var removed = Remove(reservation.GoodId, reservation.Quantity, reservation.Quality);
        _reservations.Remove(reservationId);
        return removed;
    }

    public IReadOnlyList<StockMovement> ConsumeAvailable(
        DefinitionId<Good> goodId,
        long quantity,
        GoodQuality? quality = null)
    {
        RequireQualitySelectorWhenNeeded(goodId, quality);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Consumed quantity must be positive.");
        if (quantity > AvailableQuantityOf(goodId, quality))
            throw new InvalidOperationException("There is not enough unreserved stock.");
        return Remove(goodId, quantity, quality);
    }

    /// <summary>
    /// Advances every batch by one monthly tick and removes batches reaching their authored shelf
    /// life. Oldest batches are reported first. The hook is deliberately policy-free: preservation,
    /// conversion, loss reporting, and ledger events can subscribe without coupling storage to a
    /// future production system.
    /// </summary>
    public IReadOnlyList<SpoilageRecord> AdvanceSpoilage(SpoilageHook? hook = null)
    {
        for (var i = 0; i < _lots.Count; i++)
            _lots[i] = _lots[i] with { AgeInTicks = checked(_lots[i].AgeInTicks + 1) };

        var expired = _lots
            .Where(lot => lot.Good.ShelfLifeTicks is int shelfLife && lot.AgeInTicks >= shelfLife)
            .OrderByDescending(lot => lot.AgeInTicks)
            .ToArray();
        var records = new List<SpoilageRecord>(expired.Length);
        foreach (var lot in expired)
        {
            _lots.Remove(lot);
            var released = ReconcileReservationsAfterLoss(lot.Good.Id, lot.Quality);
            var record = new SpoilageRecord(ToMovement(lot, lot.Quantity), released);
            records.Add(record);
            hook?.Invoke(record);
        }
        return records;
    }

    private IReadOnlyList<StockMovement> Remove(DefinitionId<Good> goodId, long quantity, GoodQuality? quality)
    {
        var remaining = quantity;
        var removed = new List<StockMovement>();
        // FEFO/FIFO: consume the oldest compatible stock first; insertion order breaks equal-age ties.
        foreach (var lot in _lots.Where(l => l.Good.Id == goodId && (quality is null || l.Quality == quality))
                     .OrderByDescending(l => l.AgeInTicks).ToArray())
        {
            if (remaining == 0)
                break;
            var taken = Math.Min(remaining, lot.Quantity);
            removed.Add(ToMovement(lot, taken));
            var index = _lots.IndexOf(lot);
            if (taken == lot.Quantity)
                _lots.RemoveAt(index);
            else
                _lots[index] = lot with { Quantity = lot.Quantity - taken };
            remaining -= taken;
        }
        return removed;
    }

    private long ReconcileReservationsAfterLoss(DefinitionId<Good> goodId, GoodQuality? quality)
    {
        var compatible = _reservations.Where(pair => pair.Value.GoodId == goodId &&
            pair.Value.Quality == quality).ToArray();
        var reserved = compatible.Sum(pair => pair.Value.Quantity);
        var physical = QuantityOf(goodId, quality);
        var excessReservation = Math.Max(0, reserved - physical);
        var released = 0L;
        // Reservation IDs provide a stable priority: later ordinal IDs lose their claim first.
        foreach (var pair in compatible.Reverse())
        {
            var reduction = Math.Min(pair.Value.Quantity, excessReservation);
            if (reduction == 0)
                continue;
            if (reduction == pair.Value.Quantity)
                _reservations.Remove(pair.Key);
            else
                _reservations[pair.Key] = pair.Value with { Quantity = pair.Value.Quantity - reduction };
            excessReservation -= reduction;
            released += reduction;
            if (excessReservation == 0)
                break;
        }
        return released;
    }

    private static StockMovement ToMovement(StockLot lot, long quantity) =>
        new(lot.Good, quantity, lot.Quality, lot.Condition, lot.AgeInTicks, lot.Provenance);

    private static void ValidateReservationId(string reservationId)
    {
        if (string.IsNullOrWhiteSpace(reservationId))
            throw new ArgumentException("A reservation ID cannot be empty.", nameof(reservationId));
    }

    private void RequireQualitySelectorWhenNeeded(DefinitionId<Good> goodId, GoodQuality? quality)
    {
        if (quality is null && _lots.Any(lot => lot.Good.Id == goodId && lot.Good.QualityEligible))
            throw new ArgumentException("A quality must be selected for a quality-eligible good.", nameof(quality));
    }
}
