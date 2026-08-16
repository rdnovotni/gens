using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Buildings;

/// <summary>The outcome <see cref="ProductionResolvedEvent"/> records for one Building's production
/// attempt this month.</summary>
public enum ProductionOutcome
{
    /// <summary>Every input was available and every output fit; inputs were consumed and outputs
    /// added to the Holding's <see cref="Stockpile"/>.</summary>
    Produced,

    /// <summary>At least one recipe input was short; no partial production — nothing was consumed or
    /// produced this month.</summary>
    InputShortage,

    /// <summary>Every input was available, but the Holding's <see cref="Stockpile"/> does not have
    /// enough remaining capacity for the full output even after the inputs it would free up; no
    /// partial production — nothing was consumed or produced this month. This is the "storage
    /// constraint" Phase 6 item 7 asks production to respect: an estate cannot silently overflow or
    /// discard goods it has nowhere to put.</summary>
    StorageFull,
}

/// <summary>
/// The monthly production tick (Phase 6 item 7): for every completed, <see
/// cref="BuildingInstance.IsOperational"/> Building with a <see cref="ProductionRecipe"/>, resolves
/// this month's conversion against its Holding's <see cref="Stockpile"/> — consuming inputs and adding
/// outputs only when every input is available <em>and</em> the full output fits in remaining capacity
/// (no partial production either way, matching <see cref="MaintenanceSystem"/>'s identical
/// all-or-nothing rule) — and always emits a <see cref="ProductionResolvedEvent"/> recording the
/// outcome, per Phase 6 item 8's "emit complete ledger-ready production... events even before the
/// economy consumes them." <see cref="LaborOutputComputedEvent"/>'s own doc comment names this system
/// as its eventual aggregator; this first pass does not yet scale output by labor quality (that
/// requires a documented conversion this implementation does not yet specify) — a recipe either runs
/// at its full authored rate or does not run at all this month.
/// </summary>
public sealed class ProductionSystem : IMonthlySystem<WorldState>
{
    private readonly Dictionary<DefinitionId<Good>, GoodDefinition> _goodCatalog;

    /// <param name="goodCatalog">Resolves a recipe output's <see cref="DefinitionId{T}"/> to the full
    /// content-authored <see cref="GoodDefinition"/> a new <see cref="Stockpile"/> lot needs. No
    /// content-loading catalog exists yet at runtime (every <see cref="BuildingInstance"/> already
    /// carries its full <see cref="BuildingDefinition"/> by reference rather than by ID, for the same
    /// reason) — the caller supplies this lookup directly, matching how <see
    /// cref="Characters.CharacterLifecycleSystem"/> takes its random stream name rather than this
    /// project inventing a service locator.</param>
    public ProductionSystem(IEnumerable<GoodDefinition> goodCatalog)
    {
        if (goodCatalog is null)
            throw new ArgumentNullException(nameof(goodCatalog));
        _goodCatalog = goodCatalog.ToDictionary(static good => good.Id);
    }

    public string Id => "buildings.production";
    public TickPhase Phase => TickPhase.Production;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characters", "buildings", "plots", "stockpiles" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "stockpiles", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.laborOutput", "buildings.maintenance" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.Buildings.InAscendingOrder().ToArray())
        {
            var building = entry.Value;
            if (building.Definition.Recipe is not { } recipe)
                continue;
            if (!building.IsOperational)
                continue;
            if (!EstateLookup.TryResolveStockpile(state, building.PlotId, out var holdingId, out var stockpile))
                continue;

            var hasEveryInput = recipe.Inputs.All(line => stockpile.AvailableQuantityOf(line.GoodId) >= line.Quantity);
            if (!hasEveryInput)
            {
                events.Add(new ProductionResolvedEvent(
                    state.EventIds.Issue(), context.Date, building.Id, holdingId,
                    ProductionOutcome.InputShortage, recipe.Inputs, recipe.Outputs));
                continue;
            }

            // Consuming the inputs first frees exactly their quantity back into the stockpile, so the
            // real capacity test is against remaining capacity plus what this recipe would free up.
            var totalInputQuantity = recipe.Inputs.Sum(line => line.Quantity);
            var totalOutputQuantity = recipe.Outputs.Sum(line => line.Quantity);
            if (totalOutputQuantity > stockpile.RemainingCapacity + totalInputQuantity)
            {
                events.Add(new ProductionResolvedEvent(
                    state.EventIds.Issue(), context.Date, building.Id, holdingId,
                    ProductionOutcome.StorageFull, recipe.Inputs, recipe.Outputs));
                continue;
            }

            foreach (var line in recipe.Inputs)
                stockpile.ConsumeAvailable(line.GoodId, line.Quantity);
            foreach (var line in recipe.Outputs)
                stockpile.Add(ResolveGood(line.GoodId), line.Quantity);

            events.Add(new ProductionResolvedEvent(
                state.EventIds.Issue(), context.Date, building.Id, holdingId,
                ProductionOutcome.Produced, recipe.Inputs, recipe.Outputs));
        }

        return events;
    }

    private GoodDefinition ResolveGood(DefinitionId<Good> goodId) =>
        _goodCatalog.TryGetValue(goodId, out var good)
            ? good
            : throw new InvalidOperationException($"No good catalog entry for '{goodId.Value}'.");
}

/// <summary>Emitted for every operational Building with a recipe, every month, whether or not
/// production actually ran (Phase 6 item 8's "emit complete ledger-ready production... events even
/// before the economy consumes them") — ledger-ready groundwork for a future Markets & Ledger system,
/// matching <see cref="Characters.LaborOutputComputedEvent"/>'s identical "always emitted" convention.
/// <see cref="InputLines"/> and <see cref="OutputLines"/> always carry the recipe's full authored lines
/// regardless of <see cref="Outcome"/> — production is all-or-nothing, so these are exactly the
/// quantities that moved on <see cref="ProductionOutcome.Produced"/> and exactly the quantities that
/// were needed (but did not move) on <see cref="ProductionOutcome.InputShortage"/> or <see
/// cref="ProductionOutcome.StorageFull"/>. A ledger consumer needs the attempted flow in every case, not
/// only the successful one.</summary>
public sealed record ProductionResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Building> BuildingId,
    RuntimeId<Holding> HoldingId,
    ProductionOutcome Outcome,
    IReadOnlyList<RecipeLine> InputLines,
    IReadOnlyList<RecipeLine> OutputLines) : IDomainEvent
{
    public string Type => "buildings.productionResolved";
    public int SchemaVersion => 2;
    public IReadOnlyList<string> SubjectIds => new[] { BuildingId.ToTaggedString(), HoldingId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
