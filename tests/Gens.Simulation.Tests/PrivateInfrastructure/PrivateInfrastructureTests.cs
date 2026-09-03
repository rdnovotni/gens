using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.PrivateInfrastructure;
using Gens.Simulation.Random;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.PrivateInfrastructure;

/// <summary>Phase 15 item 7 coverage: tiered Private Roads and the §2.1 trade-proximity bonus, Road
/// Clusters and the §4/§4.1 Connected Estate/Unified Estate mechanics, Irrigation Canals and Wells/
/// Cisterns (including the real §3 Drought/Famine Exposure reduction), Land Reclamation's §5.1 Partial/
/// Full resolution, private Bridges, Boundary Wall/Fence and its Regimen confinement backing, §8
/// upkeep/condition decay and disaster vulnerability, and a save/load round trip
/// (<c>gens-private-infrastructure-design.md</c>).</summary>
public sealed class PrivateInfrastructureTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId) OneSettlement(SettlementStage stage = SettlementStage.Vicus)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, stage));
        return (state, settlementId);
    }

    private static RuntimeId<Plot> OwnedPlot(
        WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId,
        TerrainType terrain = TerrainType.FertilePlain, TerrainFeature features = TerrainFeature.None)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(
            plotId, settlementId, terrain, features, ownerId: PropertyOwnerRef.ForPlayerHousehold(householdId).ToTaggedOwnerId()));
        return plotId;
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static Money BalanceOf(WorldState state, RuntimeId<Household> householdId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;

    // ---- §2 Private Roads / trade proximity ---------------------------------------------------

    [Test]
    public void BuildPavedRoadConnectionChargesCostAndSeedsPristineCondition()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(1000));

        var result = BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.Multiple(() =>
        {
            Assert.That(BalanceOf(state, householdId), Is.EqualTo(Money.FromDenarii(1000) - PrivateInfrastructureCatalog.PavedRoadConstructionCost));
            Assert.That(state.PavedRoadConnections.Count, Is.EqualTo(1));
            var connection = state.PavedRoadConnections.InAscendingOrder().Single().Value;
            Assert.That(InfrastructureConditionResolver.Current(state, connection.ConditionKey).Condition, Is.EqualTo(PrivateInfrastructureCatalog.PristineCondition));
        });
    }

    [Test]
    public void BuildPavedRoadConnectionRejectsAnUnownedOrCrossHouseholdPair()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, otherHouseholdId);
        Fund(state, householdId, Money.FromDenarii(1000));

        var result = BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BuildPavedRoadConnectionCommands.NotOwnedByHousehold));
    }

    [Test]
    public void BuildPavedRoadConnectionRejectsInsufficientFunds()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);

        var result = BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BuildPavedRoadConnectionCommands.InsufficientFunds));
    }

    [Test]
    public void TradeProximityBonusAppliesToARiverAdjacentPlotAndItsPavedRoadConnectedNeighbor()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var riverPlot = OwnedPlot(state, settlementId, householdId, TerrainType.River);
        var inlandPlot = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(1000));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, riverPlot, inlandPlot));

        var balanceBefore = BalanceOf(state, householdId);
        PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(1));

        // Both the River plot itself and the inland Plot it is paved-road-connected to qualify.
        var expected = PrivateInfrastructureCatalog.TradeProximityMonthlyBonus.Scale(Fixed64.FromInt(2));
        Assert.That(BalanceOf(state, householdId), Is.EqualTo(balanceBefore + expected));
    }

    [Test]
    public void AnIsolatedInlandPlotEarnsNoTradeProximityBonus()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        OwnedPlot(state, settlementId, householdId);
        var balanceBefore = BalanceOf(state, householdId);

        PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(1));

        Assert.That(BalanceOf(state, householdId), Is.EqualTo(balanceBefore));
    }

    // ---- §4 Road Clusters / Connected Estate / Unified Estate ---------------------------------

    [Test]
    public void ThreePlotsConnectedByPavedRoadFormAClusterWithTheConnectedEstateBonusActive()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        var plotC = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(2000));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotB, plotC));

        var clusters = RoadClusterQuery.ComputeClusters(state, householdId);

        Assert.That(clusters, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(clusters[0].PlotIds, Has.Count.EqualTo(3));
            Assert.That(clusters[0].ConnectedEstateBonusActive, Is.True);
        });
    }

    [Test]
    public void TwoConnectedPlotsDoNotYetClearTheConnectedEstateThreshold()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(1000));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        var clusters = RoadClusterQuery.ComputeClusters(state, householdId);

        Assert.That(clusters[0].ConnectedEstateBonusActive, Is.False);
    }

    [Test]
    public void ConnectedEstateBonusPaysOncePerQualifyingClusterScaledByBuildingCount()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        var plotC = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(2000));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotB, plotC));

        var definition = new BuildingDefinition(new DefinitionId<Building>("field"), BuildingTier.Tier1, 1, 1);
        state.Buildings.Add(state.BuildingIds.Issue(), new BuildingInstance(state.BuildingIds.Issue(), plotA, definition));
        state.Buildings.Add(state.BuildingIds.Issue(), new BuildingInstance(state.BuildingIds.Issue(), plotB, definition));

        var balanceBefore = BalanceOf(state, householdId);
        var events = PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(1));

        var expected = PrivateInfrastructureCatalog.ConnectedEstateBonusPerBuilding.Scale(Fixed64.FromInt(2));
        Assert.Multiple(() =>
        {
            Assert.That(BalanceOf(state, householdId), Is.GreaterThanOrEqualTo(balanceBefore + expected));
            Assert.That(events.OfType<ConnectedEstateBonusAppliedEvent>().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void UnifiedEstateMilestoneFiresExactlyOnceWhenTheClusterCoversEveryOwnedPlot()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        var plotC = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(2000));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotB, plotC));

        var firstTick = PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(1));
        var secondTick = PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(2));

        Assert.Multiple(() =>
        {
            Assert.That(firstTick.OfType<UnifiedEstateAchievedEvent>().Count(), Is.EqualTo(1));
            Assert.That(secondTick.OfType<UnifiedEstateAchievedEvent>(), Is.Empty);
            Assert.That(state.UnifiedEstateMilestones.TryGet(householdId, out _), Is.True);
        });
    }

    [Test]
    public void UnifiedEstateAchievedProjectsIntoTheDynastyChronicle()
    {
        var evt = new UnifiedEstateAchievedEvent(new RuntimeIdCounter<DomainEventEntity>().Issue(), new GameDate(1), new RuntimeIdCounter<Household>().Issue(), 3);
        var state = new WorldState(new GameDate(1));

        var drafts = ChronicleProjector.Project(state, new IDomainEvent[] { evt });

        Assert.That(drafts, Has.Count.EqualTo(1));
        Assert.That(drafts[0].Category, Is.EqualTo(ChronicleCategory.WealthAndBuilding));
    }

    // ---- §3 Irrigation Canal / §3.1 Well & Cistern ---------------------------------------------

    [Test]
    public void BuildIrrigationCanalSucceedsOnARiverAdjacentPlot()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId, TerrainType.FertilePlain, TerrainFeature.RiverAdjacent);
        Fund(state, householdId, Money.FromDenarii(500));

        var result = BuildIrrigationCanalCommands.Pipeline.Execute(
            state, new BuildIrrigationCanalCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(state.IrrigationCanals.TryGet(plotId, out var canal), Is.True);
        Assert.That(canal!.SourceType, Is.EqualTo(IrrigationSourceType.RiverAdjacent));
    }

    [Test]
    public void BuildIrrigationCanalOnAnInlandVicusPlotIsRejectedButSucceedsOnceTheSettlementIsACity()
    {
        var (state, settlementId) = OneSettlement(SettlementStage.Vicus);
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(500));

        var rejected = BuildIrrigationCanalCommands.Pipeline.Execute(
            state, new BuildIrrigationCanalCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId));
        Assert.That(rejected.Accepted, Is.False);
        Assert.That(rejected.Error, Is.EqualTo(BuildIrrigationCanalCommands.NoEligibleSource));

        state.Settlements.Remove(settlementId);
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, state.Regions.InAscendingOrder().Single().Key, SettlementStage.City));
        var accepted = BuildIrrigationCanalCommands.Pipeline.Execute(
            state, new BuildIrrigationCanalCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, plotId));

        Assert.That(accepted.Accepted, Is.True, $"Rejected: {accepted.Error}");
        Assert.That(state.IrrigationCanals.TryGet(plotId, out var canal), Is.True);
        Assert.That(canal!.SourceType, Is.EqualTo(IrrigationSourceType.PrivateAqueductBranch));
    }

    [Test]
    public void BuildWellOrCisternNeedsNoRiverOrAqueduct()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(200));

        var result = BuildWellOrCisternCommands.Pipeline.Execute(
            state, new BuildWellOrCisternCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, WellOrCisternType.Cistern));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(state.WellOrCisterns.TryGet(plotId, out var well), Is.True);
        Assert.That(well!.Type, Is.EqualTo(WellOrCisternType.Cistern));
    }

    [Test]
    public void IrrigatedPlotsRealLyReduceDroughtFamineExposure()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(200));
        BuildWellOrCisternCommands.Pipeline.Execute(
            state, new BuildWellOrCisternCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, WellOrCisternType.Well));

        var withoutIrrigation = HazardExposureCalculator.DroughtFamineExposure(drySeasonMonth: false);
        var profile = HazardExposureProfile.Compute(state, settlementId, new GameDate(1));
        var withIrrigation = profile.ExposureFor(HazardType.DroughtFamine);

        Assert.That(withIrrigation, Is.LessThan(withoutIrrigation));
    }

    // ---- §5 Land Reclamation --------------------------------------------------------------------

    [Test]
    public void StartLandReclamationRequiresMarshTerrain()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);

        var result = StartLandReclamationCommands.Pipeline.Execute(
            state, new StartLandReclamationCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, plotId));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(StartLandReclamationCommands.NotMarshTerrain));
    }

    [Test]
    public void LandReclamationResolvesToAPartialOrFullOutcomeAfterItsFullDuration()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId, TerrainType.Marsh);
        Fund(state, householdId, Money.FromDenarii(10_000));
        StartLandReclamationCommands.Pipeline.Execute(
            state, new StartLandReclamationCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, plotId));

        var streams = new RandomStreamSet();
        streams.AddDerived(LandReclamationResolutionSystem.StreamName, rootSeed: 1);

        IReadOnlyList<IDomainEvent> lastMonthEvents = Array.Empty<IDomainEvent>();
        for (var month = 1; month <= PrivateInfrastructureCatalog.LandReclamationDurationMonths; month++)
            lastMonthEvents = LandReclamationResolutionSystem.Tick(state, new GameDate(month), streams);

        state.Plots.TryGet(plotId, out var finalPlot);
        state.LandReclamationProjects.TryGet(plotId, out var project);

        Assert.Multiple(() =>
        {
            Assert.That(project!.Status, Is.Not.EqualTo(LandReclamationStatus.InProgress));
            Assert.That(lastMonthEvents.OfType<LandReclamationCompletedEvent>().Count(), Is.EqualTo(1));
            Assert.That(finalPlot!.Terrain, project!.Status == LandReclamationStatus.CompletedFull ? Is.EqualTo(TerrainType.FertilePlain) : Is.EqualTo(TerrainType.Marsh));
        });
    }

    [Test]
    public void LandReclamationCanResolveBothPartialAndFullAcrossABoundedSeedRange()
    {
        var sawPartial = false;
        var sawFull = false;

        for (var seed = 1UL; seed <= 40UL && !(sawPartial && sawFull); seed++)
        {
            var (state, settlementId) = OneSettlement();
            var householdId = state.HouseholdIds.Issue();
            var plotId = OwnedPlot(state, settlementId, householdId, TerrainType.Marsh);
            Fund(state, householdId, Money.FromDenarii(10_000));
            StartLandReclamationCommands.Pipeline.Execute(
                state, new StartLandReclamationCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, plotId));

            var streams = new RandomStreamSet();
            streams.AddDerived(LandReclamationResolutionSystem.StreamName, seed);
            for (var month = 1; month <= PrivateInfrastructureCatalog.LandReclamationDurationMonths; month++)
                LandReclamationResolutionSystem.Tick(state, new GameDate(month), streams);

            state.LandReclamationProjects.TryGet(plotId, out var project);
            if (project!.Status == LandReclamationStatus.CompletedFull)
                sawFull = true;
            else if (project.Status == LandReclamationStatus.CompletedPartial)
                sawPartial = true;
        }

        Assert.Multiple(() =>
        {
            Assert.That(sawPartial, Is.True, "Expected at least one seed to resolve Partial.");
            Assert.That(sawFull, Is.True, "Expected at least one seed to resolve Full.");
        });
    }

    [Test]
    public void FullLandReclamationAwardsDignitasAndChroniclesTheAchievement()
    {
        RuntimeId<Household>? householdWithFull = null;
        WorldState? finalState = null;
        RuntimeId<Plot>? finalPlot = null;

        for (var seed = 1UL; seed <= 40UL && householdWithFull is null; seed++)
        {
            var (state, settlementId) = OneSettlement();
            var householdId = state.HouseholdIds.Issue();
            var plotId = OwnedPlot(state, settlementId, householdId, TerrainType.Marsh);
            Fund(state, householdId, Money.FromDenarii(10_000));
            StartLandReclamationCommands.Pipeline.Execute(
                state, new StartLandReclamationCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, plotId));

            var streams = new RandomStreamSet();
            streams.AddDerived(LandReclamationResolutionSystem.StreamName, seed);
            IReadOnlyList<IDomainEvent> events = Array.Empty<IDomainEvent>();
            for (var month = 1; month <= PrivateInfrastructureCatalog.LandReclamationDurationMonths; month++)
                events = LandReclamationResolutionSystem.Tick(state, new GameDate(month), streams);

            if (events.OfType<LandReclamationCompletedEvent>().Any(e => e.Outcome == LandReclamationOutcome.FullReclamation))
            {
                householdWithFull = householdId;
                finalState = state;
                finalPlot = plotId;
            }
        }

        Assert.That(householdWithFull, Is.Not.Null, "No seed in range resolved Full — widen the range.");
        var dignitas = DignitasResolver.Current(finalState!, householdWithFull!.Value);
        Assert.That(dignitas, Is.GreaterThanOrEqualTo(PrivateInfrastructureCatalog.FullReclamationDignitasAward));

        var completedEvent = new LandReclamationCompletedEvent(
            new RuntimeIdCounter<DomainEventEntity>().Issue(), new GameDate(1), finalPlot!.Value, LandReclamationOutcome.FullReclamation, null);
        var drafts = ChronicleProjector.Project(finalState!, new IDomainEvent[] { completedEvent });
        Assert.That(drafts, Has.Count.EqualTo(1));
    }

    // ---- §6 Private Bridges ---------------------------------------------------------------------

    [Test]
    public void BuildPrivateBridgeRequiresARiverCrossing()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(500));

        var result = BuildPrivateBridgeCommands.Pipeline.Execute(
            state, new BuildPrivateBridgeCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(BuildPrivateBridgeCommands.NoRiverCrossing));
    }

    [Test]
    public void BuildPrivateBridgeSucceedsWhenOnePlotIsRiverAdjacent()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId, TerrainType.River);
        var plotB = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(500));

        var result = BuildPrivateBridgeCommands.Pipeline.Execute(
            state, new BuildPrivateBridgeCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(state.PrivateBridges.Count, Is.EqualTo(1));
    }

    // ---- §7 Boundary & Security Infrastructure ---------------------------------------------------

    [Test]
    public void BoundaryInfrastructureCarriesConfinementBackingWhenTheHouseholdRunsAConfinedRegimen()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        state.HouseholdRegimenDefaults.Add(
            new HouseholdRegimenKey(householdId, null), new RegimenSettings(DietTier.Adequate, AccommodationTier.Basic, FreedomsTier.Confined, DisciplineTier.Firm));
        Fund(state, householdId, Money.FromDenarii(500));

        var result = BuildBoundaryInfrastructureCommands.Pipeline.Execute(
            state, new BuildBoundaryInfrastructureCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, BoundaryInfrastructureType.Wall));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(state.BoundaryInfrastructures.TryGet(plotId, out var boundary), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(boundary!.ConfinementBacking, Is.True);
            Assert.That(boundary.RustlingRiskReduction, Is.EqualTo(PrivateInfrastructureCatalog.WallRustlingRiskReduction));
            Assert.That(boundary.PairedWithFortifyPosture, Is.False, "Frontier Security Posture is unbuilt — always false.");
        });
    }

    [Test]
    public void BoundaryInfrastructureHasNoConfinementBackingUnderFreeMovement()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(200));

        var result = BuildBoundaryInfrastructureCommands.Pipeline.Execute(
            state, new BuildBoundaryInfrastructureCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, BoundaryInfrastructureType.Fence));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        state.BoundaryInfrastructures.TryGet(plotId, out var boundary);
        Assert.That(boundary!.ConfinementBacking, Is.False);
    }

    // ---- §8 Maintenance & Disaster Vulnerability --------------------------------------------------

    [Test]
    public void UnpaidUpkeepDegradesConditionWhilePaidUpkeepPostsARealLedgerExpense()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId);
        var plotB = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, PrivateInfrastructureCatalog.PavedRoadConstructionCost);
        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        var connectionKey = state.PavedRoadConnections.InAscendingOrder().Single().Value.ConditionKey;

        // No funds left for upkeep this month.
        InfrastructureUpkeepSystem.Tick(state, new GameDate(1));

        Assert.That(InfrastructureConditionResolver.Current(state, connectionKey).Condition,
            Is.EqualTo(PrivateInfrastructureCatalog.PristineCondition - PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss));

        Fund(state, householdId, Money.FromDenarii(1000));
        var balanceBeforePaidUpkeep = BalanceOf(state, householdId);
        InfrastructureUpkeepSystem.Tick(state, new GameDate(2));

        Assert.Multiple(() =>
        {
            Assert.That(InfrastructureConditionResolver.Current(state, connectionKey).Condition,
                Is.EqualTo(PrivateInfrastructureCatalog.PristineCondition - PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss),
                "A paid month should not lose further condition.");
            Assert.That(BalanceOf(state, householdId), Is.EqualTo(balanceBeforePaidUpkeep - PrivateInfrastructureCatalog.PavedRoadMonthlyUpkeep));
        });
    }

    [Test]
    public void RepairInfrastructureCommandRestoresConditionForARealCost()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(500));
        BuildWellOrCisternCommands.Pipeline.Execute(
            state, new BuildWellOrCisternCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, WellOrCisternType.Well));
        var key = state.WellOrCisterns.InAscendingOrder().Single().Value.ConditionKey;
        InfrastructureConditionResolver.Set(state, InfrastructureCondition.Pristine(key) with { Condition = 40 });

        var result = RepairInfrastructureCommands.Pipeline.Execute(
            state, new RepairInfrastructureCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, key, householdId));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(InfrastructureConditionResolver.Current(state, key).Condition, Is.EqualTo(40 + PrivateInfrastructureCatalog.RepairConditionRestored));
    }

    [Test]
    public void ADisasterEventEligibleForAStructureTypeCanDamageItsCondition()
    {
        var hit = false;
        for (var seed = 1UL; seed <= 30UL && !hit; seed++)
        {
            var (state, settlementId) = OneSettlement();
            var householdId = state.HouseholdIds.Issue();
            var plotA = OwnedPlot(state, settlementId, householdId);
            var plotB = OwnedPlot(state, settlementId, householdId);
            Fund(state, householdId, PrivateInfrastructureCatalog.PavedRoadConstructionCost);
            BuildPavedRoadConnectionCommands.Pipeline.Execute(
                state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
            var key = state.PavedRoadConnections.InAscendingOrder().Single().Value.ConditionKey;

            var disaster = new DisasterEventOccurredEvent(
                state.EventIds.Issue(), new GameDate(1), settlementId, state.DisasterEventIds.Issue(),
                HazardType.Flood, DisasterSeverity.Catastrophic, TriggeredByCompounding: false);

            var streams = new RandomStreamSet();
            streams.AddDerived(InfrastructureDisasterVulnerabilitySystem.StreamName, seed);
            InfrastructureDisasterVulnerabilitySystem.Tick(state, new GameDate(1), new[] { disaster }, streams);

            if (InfrastructureConditionResolver.Current(state, key).Condition < PrivateInfrastructureCatalog.PristineCondition)
                hit = true;
        }

        Assert.That(hit, Is.True, "Expected a Catastrophic Flood to eventually damage a Paved Road within the seed range.");
    }

    [Test]
    public void DroughtFamineNeverDamagesABoundaryWallSinceItIsNotAnEligibleHazard()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        Fund(state, householdId, Money.FromDenarii(500));
        BuildBoundaryInfrastructureCommands.Pipeline.Execute(
            state, new BuildBoundaryInfrastructureCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotId, BoundaryInfrastructureType.Wall));
        var key = state.BoundaryInfrastructures.InAscendingOrder().Single().Value.ConditionKey;

        var disaster = new DisasterEventOccurredEvent(
            state.EventIds.Issue(), new GameDate(1), settlementId, state.DisasterEventIds.Issue(),
            HazardType.DroughtFamine, DisasterSeverity.Catastrophic, TriggeredByCompounding: false);
        var streams = new RandomStreamSet();
        streams.AddDerived(InfrastructureDisasterVulnerabilitySystem.StreamName, rootSeed: 1);

        var events = InfrastructureDisasterVulnerabilitySystem.Tick(state, new GameDate(1), new[] { disaster }, streams);

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(InfrastructureConditionResolver.Current(state, key).Condition, Is.EqualTo(PrivateInfrastructureCatalog.PristineCondition));
        });
    }

    // ---- Save/load round trip and deterministic hash stability -----------------------------------

    [Test]
    public void PrivateInfrastructureStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId) = OneSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotA = OwnedPlot(state, settlementId, householdId, TerrainType.River);
        var plotB = OwnedPlot(state, settlementId, householdId);
        var marshPlot = OwnedPlot(state, settlementId, householdId, TerrainType.Marsh);
        Fund(state, householdId, Money.FromDenarii(5000));

        BuildPavedRoadConnectionCommands.Pipeline.Execute(
            state, new BuildPavedRoadConnectionCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        BuildIrrigationCanalCommands.Pipeline.Execute(
            state, new BuildIrrigationCanalCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA));
        BuildWellOrCisternCommands.Pipeline.Execute(
            state, new BuildWellOrCisternCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotB, WellOrCisternType.Cistern));
        StartLandReclamationCommands.Pipeline.Execute(
            state, new StartLandReclamationCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, marshPlot));
        BuildPrivateBridgeCommands.Pipeline.Execute(
            state, new BuildPrivateBridgeCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotA, plotB));
        BuildBoundaryInfrastructureCommands.Pipeline.Execute(
            state, new BuildBoundaryInfrastructureCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, plotB, BoundaryInfrastructureType.Wall));
        InfrastructureUpkeepSystem.Tick(state, new GameDate(1));
        PrivateInfrastructureBenefitsSystem.Tick(state, new GameDate(1));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.PavedRoadConnections.Count, Is.EqualTo(1));
            Assert.That(restored.IrrigationCanals.Count, Is.EqualTo(1));
            Assert.That(restored.WellOrCisterns.Count, Is.EqualTo(1));
            Assert.That(restored.LandReclamationProjects.Count, Is.EqualTo(1));
            Assert.That(restored.PrivateBridges.Count, Is.EqualTo(1));
            Assert.That(restored.BoundaryInfrastructures.Count, Is.EqualTo(1));
            Assert.That(restored.InfrastructureConditions.Count, Is.EqualTo(state.InfrastructureConditions.Count));
        });
    }
}
