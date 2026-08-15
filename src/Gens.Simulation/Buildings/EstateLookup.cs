using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;

namespace Gens.Simulation.Buildings;

/// <summary>
/// Small shared helpers for Phase 6 item 7's monthly systems (<see cref="ConstructionSystem"/>, <see
/// cref="MaintenanceSystem"/>, <see cref="ProductionSystem"/>): resolving a Building's owning Holding
/// through its Plot, and reading that Holding's <see cref="Stockpile"/> — the "one storage constraint"
/// this item wires production and maintenance through (<c>gens-estate-settlement-design.md</c>'s
/// storage accumulating at the estate level, not per building).
/// </summary>
internal static class EstateLookup
{
    /// <summary>Resolves the Holding currently occupying the Plot a Building stands on. False if the
    /// Plot record is missing (should not happen for a Building already on record) or unoccupied.</summary>
    public static bool TryResolveHolding(WorldState state, RuntimeId<Plot> plotId, out RuntimeId<Holding> holdingId)
    {
        if (state.Plots.TryGet(plotId, out var plot) && plot.OccupyingHoldingId is { } occupying)
        {
            holdingId = occupying;
            return true;
        }

        holdingId = default;
        return false;
    }

    /// <summary>Resolves both the owning Holding and its <see cref="Stockpile"/>. False if either the
    /// Holding cannot be resolved (see <see cref="TryResolveHolding"/>) or that Holding has no
    /// Stockpile provisioned yet — a Holding with no storage simply cannot produce or pay upkeep,
    /// rather than silently bypassing the storage constraint.</summary>
    public static bool TryResolveStockpile(
        WorldState state, RuntimeId<Plot> plotId, out RuntimeId<Holding> holdingId, out Stockpile stockpile)
    {
        if (TryResolveHolding(state, plotId, out holdingId) && state.Stockpiles.TryGet(holdingId, out var found))
        {
            stockpile = found;
            return true;
        }

        stockpile = null!;
        return false;
    }

    /// <summary>Whether a Holding has any living resident to draw construction labor from. No
    /// dedicated construction-labor Duty slot exists yet (Phase 6 item 6 defines only FieldHand,
    /// DomesticServant, Craftsman, Cook, and Physician) — until one is added, this is a deliberately
    /// simple proxy: any living Character belonging to the Holding's occupant household counts. A
    /// Holding with no household on record (<see cref="Land.Holding.OccupantId"/> unset or not a
    /// household tag) is treated as having outside/contracted labor available, so construction is not
    /// silently blocked for holdings that never modeled a household in the first place.</summary>
    public static bool HasLaborAvailable(WorldState state, RuntimeId<Holding> holdingId)
    {
        if (!state.Holdings.TryGet(holdingId, out var holding) || holding.OccupantId is null)
            return true;
        if (!TryParseHouseholdTag(holding.OccupantId, out var householdId))
            return true;

        foreach (var entry in state.Characters.InAscendingOrder())
        {
            if (entry.Value.IsAlive && entry.Value.Household == householdId)
                return true;
        }

        return false;
    }

    private static bool TryParseHouseholdTag(string tagged, out RuntimeId<Household> householdId)
    {
        try
        {
            householdId = RuntimeId<Household>.Parse(tagged);
            return true;
        }
        catch (FormatException)
        {
            householdId = default;
            return false;
        }
    }
}
