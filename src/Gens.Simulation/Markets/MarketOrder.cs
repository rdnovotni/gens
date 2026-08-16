using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Markets;

/// <summary>
/// One supply or demand line <see cref="MarketClearingSystem"/> derives for a settlement's good, every
/// month (Phase 8 item 3's "orders"). Deliberately ephemeral — generated fresh from <see
/// cref="State.WorldState.Stockpiles"/>/<see cref="State.WorldState.PopGroups"/> each tick and never
/// itself persisted or hashed, since no player- or household-submitted order exists yet (that is
/// Phase 8 item 5's "household purchasing," explicitly out of this phase's scope). <see
/// cref="SourceId"/> is the tagged ID of the Holding (a supply order) or the group-type name (a demand
/// order) this line came from, kept only so clearing is traceable/testable — it does not itself
/// participate in the clearing math, which aggregates by (<see cref="SettlementId"/>, <see
/// cref="GoodId"/>, <see cref="Side"/>) rather than matching individual orders against each other
/// (per-order matching is exactly the granularity Phase 8 item 5's reservation contracts will need,
/// and this type's shape is deliberately ready for that without implementing it now).
/// </summary>
public sealed record MarketOrder(
    RuntimeId<Settlement> SettlementId,
    DefinitionId<Good> GoodId,
    MarketOrderSide Side,
    string SourceId,
    long Quantity);
