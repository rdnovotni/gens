using Gens.Simulation.Identity;

namespace Gens.Simulation.Goods;

/// <summary>Quantity held for a named future consumer; reservations do not remove physical stock.</summary>
public readonly record struct StockReservation(
    string ReservationId,
    DefinitionId<Good> GoodId,
    GoodQuality? Quality,
    long Quantity);
