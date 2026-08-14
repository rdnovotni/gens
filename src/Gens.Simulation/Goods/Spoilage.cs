namespace Gens.Simulation.Goods;

/// <summary>Information supplied to downstream reporting/ledger hooks when stock expires.</summary>
public readonly record struct SpoilageRecord(StockMovement Stock, long ReservedQuantityReleased);

public delegate void SpoilageHook(SpoilageRecord spoilage);
