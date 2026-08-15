using Gens.Simulation.Buildings;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Buildings;

[TestFixture]
public sealed class BuildingSystemTests
{
    private static readonly DefinitionId<Building> MineId = new("iron-mine");
    private static readonly DefinitionId<Building> SmithyId = new("smithy");
    private static readonly DefinitionId<Good> IronId = new("iron");
    private static readonly DefinitionId<Good> ToolsId = new("tools");

    [Test]
    public void DefinitionCapturesTierUpkeepStaffingAndDeterministicRecipe()
    {
        var definition = Smithy();

        Assert.Multiple(() =>
        {
            Assert.That(definition.Tier, Is.EqualTo(BuildingTier.Tier2));
            Assert.That(definition.Prerequisites, Is.EqualTo(new[] { MineId }));
            Assert.That(definition.Upkeep.Single().GoodId, Is.EqualTo(IronId));
            Assert.That(definition.StaffingSlots.Single().Capacity, Is.EqualTo(2));
            Assert.That(definition.Recipe!.Inputs.Single().GoodId, Is.EqualTo(IronId));
            Assert.That(definition.Recipe.Outputs.Single().GoodId, Is.EqualTo(ToolsId));
        });
    }

    [Test]
    public void QueueEnforcesTerrainFeaturesPrerequisitesAndCapacity()
    {
        var plot = Plot.Create(RuntimeId<Plot>.Parse("plot_0000001"), RuntimeId<Settlement>.Parse("settlement_0000001"),
            TerrainType.Hills, TerrainFeature.MineralDeposit, capacity: 2);
        var mine = new BuildingDefinition(MineId, BuildingTier.Tier1, 2, 1,
            allowedTerrain: new[] { TerrainType.Hills }, requiredFeatures: TerrainFeature.MineralDeposit);
        var queue = new ConstructionQueue();

        queue.Enqueue(plot, mine, Array.Empty<BuildingInstance>());
        Assert.That(() => queue.Enqueue(plot, Smithy(), Array.Empty<BuildingInstance>()),
            Throws.InvalidOperationException.With.Message.Contains("prerequisites"));

        var completedMine = new BuildingInstance(RuntimeId<Building>.Parse("building_0000001"), plot.Id, mine);
        queue.Cancel(0);
        queue.Enqueue(plot, Smithy(), new[] { completedMine });
        Assert.That(() => queue.Enqueue(plot, mine, new[] { completedMine }),
            Throws.InvalidOperationException.With.Message.Contains("capacity"));
    }

    [Test]
    public void QueueIsFifoAndPausesWithoutLabor()
    {
        var plot = Plot.Create(RuntimeId<Plot>.Parse("plot_0000001"), RuntimeId<Settlement>.Parse("settlement_0000001"), capacity: 2);
        var first = new BuildingDefinition(new DefinitionId<Building>("field"), BuildingTier.Tier1, 2, 1);
        var second = new BuildingDefinition(new DefinitionId<Building>("shed"), BuildingTier.Tier1, 1, 1);
        var queue = new ConstructionQueue();
        queue.Enqueue(plot, first, Array.Empty<BuildingInstance>());
        queue.Enqueue(plot, second, Array.Empty<BuildingInstance>());

        Assert.That(queue.AdvanceMonth(laborAvailable: false), Is.Null);
        Assert.That(queue.Projects[0].CompletedMonths, Is.Zero);
        Assert.That(queue.AdvanceMonth(), Is.Null);
        Assert.That(queue.AdvanceMonth()!.Definition.Id, Is.EqualTo(first.Id));
        Assert.That(queue.AdvanceMonth()!.Definition.Id, Is.EqualTo(second.Id));
    }

    [Test]
    public void StaffingAndUpkeepControlOperationalCondition()
    {
        var building = new BuildingInstance(RuntimeId<Building>.Parse("building_0000001"),
            RuntimeId<Plot>.Parse("plot_0000001"), Smithy());

        Assert.That(building.IsOperational, Is.False);
        building.AssignStaff("smith", "char_0000001");
        building.AssignStaff("smith", "char_0000002");
        Assert.That(building.IsOperational, Is.True);
        Assert.That(() => building.AssignStaff("smith", "char_0000003"), Throws.InvalidOperationException);

        building.ApplyUpkeep(paid: false);
        Assert.That(building.Condition, Is.EqualTo(BuildingCondition.Sound));
        building.ApplyUpkeep(paid: false);
        building.ApplyUpkeep(paid: false);
        building.ApplyUpkeep(paid: false);
        Assert.That(building.IsOperational, Is.False);
        building.Repair();
        Assert.That(building.IsOperational, Is.True);
    }

    [Test]
    public void TerrainGateRequiresPrimaryTerrainAndEveryFeatureFlag()
    {
        var definition = new BuildingDefinition(MineId, BuildingTier.Tier1, 1, 1,
            allowedTerrain: new[] { TerrainType.Hills },
            requiredFeatures: TerrainFeature.MineralDeposit | TerrainFeature.RiverAdjacent);
        var settlement = RuntimeId<Settlement>.Parse("settlement_0000001");

        Assert.That(definition.Allows(Plot.Create(RuntimeId<Plot>.Parse("plot_0000001"), settlement,
            TerrainType.Hills, TerrainFeature.MineralDeposit)), Is.False);
        Assert.That(definition.Allows(Plot.Create(RuntimeId<Plot>.Parse("plot_0000002"), settlement,
            TerrainType.Hills, TerrainFeature.MineralDeposit | TerrainFeature.RiverAdjacent)), Is.True);
    }

    private static BuildingDefinition Smithy() => new(
        SmithyId,
        BuildingTier.Tier2,
        constructionMonths: 3,
        plotCapacity: 1,
        prerequisites: new[] { MineId },
        upkeep: new[] { new RecipeLine(IronId, 1) },
        staffingSlots: new[] { new StaffingSlot("smith", 2) },
        recipe: new ProductionRecipe(
            new[] { new RecipeLine(IronId, 2) },
            new[] { new RecipeLine(ToolsId, 1) }));
}
