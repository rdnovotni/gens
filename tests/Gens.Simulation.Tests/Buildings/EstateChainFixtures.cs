using Gens.Simulation.Buildings;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Tests.Buildings;

/// <summary>
/// The three compact production chains Phase 6 item 7 asks for, authored in code with the exact
/// <c>DefinitionId</c> values <c>content/source/goods/*.json</c> and
/// <c>content/source/buildings/production.json</c> use — no runtime content-loading catalog exists yet
/// (see <see cref="ProductionSystem"/>'s constructor doc comment), so tests construct the same
/// <see cref="BuildingDefinition"/>/<see cref="GoodDefinition"/> shapes directly, matching
/// <c>BuildingSystemTests</c>' identical convention. Each chain is two buildings: a raw producer with
/// an empty-input recipe, and a converter consuming that output — the same "Horse Pasture → Stable →
/// Horses" compactness <c>gens-buildings-design.md</c> §5 calls out as the shortest documented chain.
/// </summary>
internal static class EstateChainFixtures
{
    public static readonly DefinitionId<Good> GrainId = new("grain");
    public static readonly DefinitionId<Good> BreadId = new("bread");
    public static readonly DefinitionId<Good> IronId = new("iron");
    public static readonly DefinitionId<Good> ToolsId = new("tools");
    public static readonly DefinitionId<Good> WoolId = new("wool");
    public static readonly DefinitionId<Good> TextilesId = new("textiles");

    public static readonly DefinitionId<Building> AgerId = new("ager");
    public static readonly DefinitionId<Building> PistrinumId = new("pistrinum");
    public static readonly DefinitionId<Building> FerrariaId = new("ferraria");
    public static readonly DefinitionId<Building> FabricaId = new("fabrica");
    public static readonly DefinitionId<Building> OvileId = new("ovile");
    public static readonly DefinitionId<Building> TextrinumId = new("textrinum");

    /// <summary>Every good referenced by the three chains below, resolvable by <see
    /// cref="ProductionSystem"/>'s constructor.</summary>
    public static IReadOnlyList<GoodDefinition> GoodCatalog { get; } = new[]
    {
        new GoodDefinition(GrainId, Perishability.NonPerishable),
        new GoodDefinition(BreadId, Perishability.NonPerishable),
        new GoodDefinition(IronId, Perishability.NonPerishable),
        new GoodDefinition(ToolsId, Perishability.NonPerishable),
        new GoodDefinition(WoolId, Perishability.NonPerishable),
        new GoodDefinition(TextilesId, Perishability.NonPerishable),
    };

    /// <summary>Chain 1 — Grain: Ager (grain field) → Pistrinum → Bread.</summary>
    public static BuildingDefinition Ager() => new(
        AgerId,
        BuildingTier.Tier1,
        constructionMonths: 1,
        plotCapacity: 1,
        staffingSlots: new[] { new StaffingSlot("farmhand", 2) },
        recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(GrainId, 4) }));

    public static BuildingDefinition Pistrinum() => new(
        PistrinumId,
        BuildingTier.Tier1,
        constructionMonths: 2,
        plotCapacity: 1,
        staffingSlots: new[] { new StaffingSlot("baker", 1) },
        recipe: new ProductionRecipe(new[] { new RecipeLine(GrainId, 1) }, new[] { new RecipeLine(BreadId, 1) }));

    /// <summary>Chain 2 — Iron: Ferraria (iron mine) → Fabrica → Tools.</summary>
    public static BuildingDefinition Ferraria() => new(
        FerrariaId,
        BuildingTier.Tier1,
        constructionMonths: 2,
        plotCapacity: 1,
        allowedTerrain: new[] { TerrainType.Hills },
        requiredFeatures: TerrainFeature.MineralDeposit,
        staffingSlots: new[] { new StaffingSlot("miner", 2) },
        recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(IronId, 2) }));

    public static BuildingDefinition Fabrica() => new(
        FabricaId,
        BuildingTier.Tier2,
        constructionMonths: 3,
        plotCapacity: 1,
        prerequisites: new[] { FerrariaId },
        upkeep: new[] { new RecipeLine(IronId, 1) },
        staffingSlots: new[] { new StaffingSlot("smith", 1) },
        recipe: new ProductionRecipe(new[] { new RecipeLine(IronId, 2) }, new[] { new RecipeLine(ToolsId, 1) }));

    /// <summary>Chain 3 — Wool: Ovile (sheepfold) → Textrinum → Textiles.</summary>
    public static BuildingDefinition Ovile() => new(
        OvileId,
        BuildingTier.Tier1,
        constructionMonths: 1,
        plotCapacity: 1,
        staffingSlots: new[] { new StaffingSlot("shepherd", 1) },
        recipe: new ProductionRecipe(Array.Empty<RecipeLine>(), new[] { new RecipeLine(WoolId, 3) }));

    public static BuildingDefinition Textrinum() => new(
        TextrinumId,
        BuildingTier.Tier1,
        constructionMonths: 2,
        plotCapacity: 1,
        staffingSlots: new[] { new StaffingSlot("weaver", 1) },
        recipe: new ProductionRecipe(new[] { new RecipeLine(WoolId, 2) }, new[] { new RecipeLine(TextilesId, 1) }));
}
