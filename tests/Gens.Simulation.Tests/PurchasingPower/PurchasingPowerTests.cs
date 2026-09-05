using Gens.Simulation.BusinessCompetition;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.PurchasingPower;
using Gens.Simulation.Random;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using System.Linq;

namespace Gens.Simulation.Tests.PurchasingPower;

/// <summary>Phase 15 item 10 coverage: §2's Wealth Pyramid classification onto every real <see
/// cref="PopGroupType"/> (including the unfavorable-Employment-Ratio override), §3's weighted Aggregate
/// Purchasing Power reading, §4's subsistence-good Contentment crisis and its Scandal exposure for a
/// visibly-thriving grain-trading business, §6's District Property Value and Market Capacity/saturation
/// integration, §7's Business Viability check (every tier, including its real recurring Reputation
/// drain), and a save/load round trip with deterministic hash stability
/// (<c>gens-population-wealth-purchasing-power-design.md</c>).</summary>
public sealed class PurchasingPowerTests
{
    private static readonly DefinitionId<Good> GrainId = NeedsConsumptionCalculator.ConsumptionGood;
    private static readonly DefinitionId<Good> ToolsId = new("tools");

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<District> DistrictId) OneSettlementWithDistrict()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District"));
        return (state, settlementId, districtId);
    }

    private static void AddPopGroup(WorldState state, RuntimeId<Settlement> settlementId, PopGroupType groupType, int size, WealthBand band)
    {
        var key = new PopGroupKey(settlementId, groupType);
        state.PopGroups.Add(key, PopGroup.Create(settlementId, groupType, size, wealthBand: band));
    }

    private static RuntimeId<NotableBusiness> PromotedBusiness(
        WorldState state, string name, RuntimeId<District> districtId, DefinitionId<Good> outputGoodId, PropertyOwnerRef? owner = null)
    {
        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, name,
                owner ?? PropertyOwnerRef.ForPlayerHousehold(state.HouseholdIds.Issue()),
                NotableBusinessTrigger.DirectPlayerTransaction, outputGoodId, LinkedPropertyRecordId: null, districtId));
        Assert.That(result.Accepted, Is.True, $"Promotion of '{name}' was rejected: {result.Error}");
        return ((NotableBusinessPromotedEvent)result.Events[0]).BusinessId;
    }

    private static void SetShortage(WorldState state, RuntimeId<Settlement> settlementId, bool shortage)
    {
        var key = new MarketGoodKey(settlementId, GrainId);
        if (state.MarketPrices.TryGet(key, out _))
            state.MarketPrices.Remove(key);
        state.MarketPrices.Add(
            key, shortage
                ? new SettlementMarket(settlementId, GrainId, Money.FromDenarii(50), Money.FromDenarii(50), 10, 20, 10, 10)
                : new SettlementMarket(settlementId, GrainId, Money.FromDenarii(50), Money.FromDenarii(50), 20, 10, 10, 0));
    }

    // ---- §2 The Wealth Pyramid classification -----------------------------------------------------

    [Test]
    public void ClassifyPopGroupMapsEveryGroupTypeOntoItsOwnTierAtFavorableEmployment()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Coloni, Fixed64.One), Is.EqualTo(WealthBand.Subsistence));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Operarii, Fixed64.One), Is.EqualTo(WealthBand.Subsistence));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Veterans, Fixed64.One), Is.EqualTo(WealthBand.Subsistence));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.NonHouseholdEnslaved, Fixed64.One), Is.EqualTo(WealthBand.Subsistence));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Opifices, Fixed64.One), Is.EqualTo(WealthBand.ModestSurplus));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Negotiatores, Fixed64.One), Is.EqualTo(WealthBand.ModestSurplus));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Aeditui, Fixed64.One), Is.EqualTo(WealthBand.EliteDiscretionary));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Curiales, Fixed64.One), Is.EqualTo(WealthBand.EliteDiscretionary));
        });
    }

    [Test]
    public void ClassifyPopGroupForcesSubsistenceForAnyGroupWithAnUnfavorableEmploymentRatio()
    {
        var unfavorable = Fixed64.FromRaw(500_000); // 0.5.
        Assert.Multiple(() =>
        {
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Curiales, unfavorable), Is.EqualTo(WealthBand.Subsistence));
            Assert.That(PurchasingPowerCalculator.ClassifyPopGroup(PopGroupType.Negotiatores, unfavorable), Is.EqualTo(WealthBand.Subsistence));
        });
    }

    // ---- §3 Aggregate Purchasing Power -------------------------------------------------------------

    [Test]
    public void AggregatePurchasingPowerSystemWeighsEliteDiscretionaryPopulationFarHeavierThanSubsistence()
    {
        var (state, settlementId, _) = OneSettlementWithDistrict();
        AddPopGroup(state, settlementId, PopGroupType.Coloni, 1000, WealthBand.Subsistence);
        AddPopGroup(state, settlementId, PopGroupType.Curiales, 20, WealthBand.EliteDiscretionary);

        AggregatePurchasingPowerSystem.Tick(state);

        Assert.That(AggregateDemandResolver.TryGetCurrent(state, settlementId, out var reading), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(reading.SubsistencePopulation, Is.EqualTo(1000));
            Assert.That(reading.EliteDiscretionaryPopulation, Is.EqualTo(20));
            // A tiny elite population still contributes a meaningful share of total weighted demand,
            // per §3's own "weighted heavily toward the top."
            Assert.That(reading.EliteDiscretionaryWeight, Is.GreaterThan(Fixed64.Zero));
            Assert.That(reading.TotalDemandIndex, Is.GreaterThan(PurchasingPowerCatalog.SubsistenceDemandWeight));
        });
    }

    [Test]
    public void AggregatePurchasingPowerSystemRecordsNoReadingForASettlementWithNoPopulation()
    {
        var (state, settlementId, _) = OneSettlementWithDistrict();
        AggregatePurchasingPowerSystem.Tick(state);
        Assert.That(AggregateDemandResolver.TryGetCurrent(state, settlementId, out _), Is.False);
    }

    // ---- §4 Subsistence Goods and Political Sensitivity --------------------------------------------

    [Test]
    public void SubsistenceGoodSensitivityDetectsAGenuineShortageOnlyFromRealUnsatisfiedDemand()
    {
        var (state, settlementId, _) = OneSettlementWithDistrict();
        SetShortage(state, settlementId, shortage: false);
        Assert.That(SubsistenceGoodSensitivityQuery.IsGenuineShortage(state, settlementId), Is.False);

        SetShortage(state, settlementId, shortage: true);
        Assert.That(SubsistenceGoodSensitivityQuery.IsGenuineShortage(state, settlementId), Is.True);
    }

    [Test]
    public void ContentmentPenaltyAppliesOnlyToSubsistenceTierDuringAGenuineShortage()
    {
        var (state, settlementId, _) = OneSettlementWithDistrict();
        SetShortage(state, settlementId, shortage: true);

        Assert.Multiple(() =>
        {
            Assert.That(SubsistenceGoodSensitivityQuery.ContentmentPenalty(state, settlementId, WealthBand.Subsistence),
                Is.EqualTo(PurchasingPowerCatalog.SubsistenceShortageContentmentPenalty));
            Assert.That(SubsistenceGoodSensitivityQuery.ContentmentPenalty(state, settlementId, WealthBand.ModestSurplus), Is.EqualTo(Fixed64.Zero));
            Assert.That(SubsistenceGoodSensitivityQuery.ContentmentPenalty(state, settlementId, WealthBand.EliteDiscretionary), Is.EqualTo(Fixed64.Zero));
        });
    }

    [Test]
    public void ContentmentSystemMeasurablyDepressesSubsistenceTierContentmentDuringAGenuineShortage()
    {
        var (state, settlementId, _) = OneSettlementWithDistrict();
        SetShortage(state, settlementId, shortage: true);

        var subsistenceKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(subsistenceKey, PopGroup.Create(
            settlementId, PopGroupType.Operarii, size: 100, wealthBand: WealthBand.Subsistence,
            employmentRatio: Fixed64.One, housingSatisfaction: Fixed64.One, contentment: Fixed64.One));

        var eliteKey = new PopGroupKey(settlementId, PopGroupType.Curiales);
        state.PopGroups.Add(eliteKey, PopGroup.Create(
            settlementId, PopGroupType.Curiales, size: 20, wealthBand: WealthBand.EliteDiscretionary,
            employmentRatio: Fixed64.One, housingSatisfaction: Fixed64.One, contentment: Fixed64.One));

        new ContentmentSystem().Tick(state, Tick(1));

        state.PopGroups.TryGet(subsistenceKey, out var subsistence);
        state.PopGroups.TryGet(eliteKey, out var elite);
        var baseline = ContentmentCalculator.ComputeContentment(Fixed64.One, Fixed64.One, NeedsConsumptionCalculator.SatisfactionFor(DietTier.Meager));

        Assert.Multiple(() =>
        {
            Assert.That(subsistence.Contentment, Is.LessThan(baseline));
            Assert.That(elite.Contentment, Is.EqualTo(baseline));
        });
    }

    [Test]
    public void RecordSubsistencePriceGougingScandalRequiresAVisiblyThrivingGrainTradingBusinessDuringAGenuineShortage()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var businessId = PromotedBusiness(state, "Grain House", districtId, GrainId);

        SetShortage(state, settlementId, shortage: false);
        var rejectedNoShortage = RecordSubsistencePriceGougingScandalCommands.Pipeline.Execute(
            state, new RecordSubsistencePriceGougingScandalCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId));
        Assert.That(rejectedNoShortage.Error, Is.EqualTo(RecordSubsistencePriceGougingScandalCommands.NoGenuineShortage));

        SetShortage(state, settlementId, shortage: true);
        var rejectedNotThriving = RecordSubsistencePriceGougingScandalCommands.Pipeline.Execute(
            state, new RecordSubsistencePriceGougingScandalCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId));
        Assert.That(rejectedNotThriving.Error, Is.EqualTo(RecordSubsistencePriceGougingScandalCommands.NotVisiblyThriving));

        var boosted = AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, 40, BusinessReputationChangeReason.QualityOutput));
        Assert.That(boosted.Accepted, Is.True);

        var toolsBusinessId = PromotedBusiness(state, "Tool Shop", districtId, ToolsId);
        var wrongGood = RecordSubsistencePriceGougingScandalCommands.Pipeline.Execute(
            state, new RecordSubsistencePriceGougingScandalCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, toolsBusinessId));
        Assert.That(wrongGood.Error, Is.EqualTo(RecordSubsistencePriceGougingScandalCommands.NotGrainTrading));

        var accepted = RecordSubsistencePriceGougingScandalCommands.Pipeline.Execute(
            state, new RecordSubsistencePriceGougingScandalCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId));
        Assert.That(accepted.Accepted, Is.True);

        state.NotableBusinesses.TryGet(businessId, out var business);
        Assert.That(business!.Reputation, Is.LessThan(NotableBusinessesCatalog.DefaultReputation + 40));
    }

    [Test]
    public void RecordSubsistencePriceGougingScandalRejectsAnUnknownBusiness()
    {
        var (state, _, _) = OneSettlementWithDistrict();
        var result = RecordSubsistencePriceGougingScandalCommands.Pipeline.Execute(
            state, new RecordSubsistencePriceGougingScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, state.NotableBusinessIds.Issue()));
        Assert.That(result.Error, Is.EqualTo(RecordSubsistencePriceGougingScandalCommands.BusinessNotFound));
    }

    // ---- §6 District Property Value / Market Capacity integration ----------------------------------

    [Test]
    public void DistrictPropertyValueReadsAggregatePurchasingPowerAsAFurtherRealInput()
    {
        var (richState, settlementId, _) = OneSettlementWithDistrict();
        var districtId = richState.DistrictIds.Issue();
        richState.Districts.Add(districtId, District.Create(districtId, settlementId, "Second"));
        AddPopGroup(richState, settlementId, PopGroupType.Curiales, 500, WealthBand.EliteDiscretionary);
        AggregatePurchasingPowerSystem.Tick(richState);

        var (poorState, poorSettlementId, poorDistrictId) = OneSettlementWithDistrict();
        AddPopGroup(poorState, poorSettlementId, PopGroupType.Coloni, 500, WealthBand.Subsistence);
        AggregatePurchasingPowerSystem.Tick(poorState);

        var system = new DistrictPropertyValueSystem();
        system.Tick(richState, Tick(1));
        system.Tick(poorState, Tick(1));

        richState.Districts.TryGet(districtId, out var richForum);
        // richState's first District (the original "Forum District") shares the same settlement's
        // reading as the second District created above.
        richState.Districts.TryGet(richState.Districts.InAscendingOrder().First().Key, out var richFirst);
        poorState.Districts.TryGet(poorDistrictId, out var poorDistrict);

        Assert.That(richFirst!.PropertyValue, Is.GreaterThan(poorDistrict!.PropertyValue));
    }

    [Test]
    public void MarketSaturationReadsThinPurchasingPowerAsSaturatingAnOtherwiseBalancedMarket()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        AddPopGroup(state, settlementId, PopGroupType.Coloni, 1000, WealthBand.Subsistence);
        AggregatePurchasingPowerSystem.Tick(state);

        var opifKey = new PopGroupKey(settlementId, PopGroupType.Opifices);
        state.PopGroups.Add(opifKey, PopGroup.Create(settlementId, PopGroupType.Opifices, 50, employmentRatio: Fixed64.One));

        for (var i = 0; i < BusinessCompetitionCatalog.UndersaturatedBusinessCountCeiling + 1; i++)
            PromotedBusiness(state, $"Shop {i}", districtId, ToolsId);

        MarketSaturationSystem.Tick(state);

        Assert.That(MarketCapacityResolver.TryGetCurrent(state, settlementId, ToolsId, out var reading), Is.True);
        Assert.That(reading.SaturationLevel, Is.EqualTo(MarketSaturationLevel.Saturated));
    }

    // ---- §7 Business Viability ----------------------------------------------------------------------

    [Test]
    public void EvaluateBusinessViabilityAlwaysMatchesASubsistenceOutputRegardlessOfPopulationMix()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var reading = PurchasingPowerCalculator.ComputeAggregateDemand(settlementId, 1000, 0, 0);
        var check = PurchasingPowerCalculator.EvaluateBusinessViability(
            state.NotableBusinessIds.Issue(), districtId, WealthBand.Subsistence, reading);
        Assert.Multiple(() =>
        {
            Assert.That(check.LocalDemandMatch, Is.True);
            Assert.That(check.RecommendedAction, Is.Null);
        });
    }

    [Test]
    public void EvaluateBusinessViabilityRecommendsSpecializeForAModestSurplusOutputWithTooThinANonSubsistencePopulation()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var reading = PurchasingPowerCalculator.ComputeAggregateDemand(settlementId, 1000, 5, 0);
        var check = PurchasingPowerCalculator.EvaluateBusinessViability(
            state.NotableBusinessIds.Issue(), districtId, WealthBand.ModestSurplus, reading);
        Assert.Multiple(() =>
        {
            Assert.That(check.LocalDemandMatch, Is.False);
            Assert.That(check.RecommendedAction, Is.EqualTo("specialize"));
        });
    }

    [Test]
    public void EvaluateBusinessViabilityRecommendsMoveForAnEliteDiscretionaryOutputWithNoRealEliteBase()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var reading = PurchasingPowerCalculator.ComputeAggregateDemand(settlementId, 1000, 200, 2);
        var check = PurchasingPowerCalculator.EvaluateBusinessViability(
            state.NotableBusinessIds.Issue(), districtId, WealthBand.EliteDiscretionary, reading);
        Assert.Multiple(() =>
        {
            Assert.That(check.LocalDemandMatch, Is.False);
            Assert.That(check.RecommendedAction, Is.EqualTo("move"));
        });
    }

    [Test]
    public void EvaluateBusinessViabilityMatchesAnEliteDiscretionaryOutputWithARealLocalEliteBase()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var reading = PurchasingPowerCalculator.ComputeAggregateDemand(settlementId, 1000, 200, 20);
        var check = PurchasingPowerCalculator.EvaluateBusinessViability(
            state.NotableBusinessIds.Issue(), districtId, WealthBand.EliteDiscretionary, reading);
        Assert.Multiple(() =>
        {
            Assert.That(check.LocalDemandMatch, Is.True);
            Assert.That(check.RecommendedAction, Is.Null);
        });
    }

    [Test]
    public void BusinessViabilitySystemAppliesARealRecurringReputationDrainWhileAMismatchPersists()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        AddPopGroup(state, settlementId, PopGroupType.Coloni, 1000, WealthBand.Subsistence);
        AggregatePurchasingPowerSystem.Tick(state);

        var businessId = PromotedBusiness(state, "Perfumer", districtId, ToolsId);
        // Force this business's Output to read as Elite Discretionary-tier for this test's own purposes
        // is not possible without a real luxury good in content (confirmed unbuilt — see
        // PurchasingPowerCalculator.GetGoodTier's own doc comment), so this test instead exercises the
        // real ModestSurplus mismatch path directly reachable with today's content.
        BusinessViabilitySystem.Tick(state, new GameDate(1));
        state.NotableBusinesses.TryGet(businessId, out var afterFirstTick);
        var firstReputation = afterFirstTick!.Reputation;

        BusinessViabilitySystem.Tick(state, new GameDate(2));
        state.NotableBusinesses.TryGet(businessId, out var afterSecondTick);

        Assert.Multiple(() =>
        {
            Assert.That(BusinessViabilityResolver.TryGetCurrent(state, businessId, out var check), Is.True);
            Assert.That(firstReputation, Is.LessThan(NotableBusinessesCatalog.DefaultReputation));
            Assert.That(afterSecondTick!.Reputation, Is.LessThanOrEqualTo(firstReputation));
        });
    }

    [Test]
    public void BusinessViabilitySystemSkipsABusinessWhoseSettlementHasNoReadingYet()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var businessId = PromotedBusiness(state, "Shop", districtId, ToolsId);

        BusinessViabilitySystem.Tick(state, new GameDate(1));

        Assert.That(BusinessViabilityResolver.TryGetCurrent(state, businessId, out _), Is.False);
    }

    // ---- Save/load round trip and deterministic hash stability -------------------------------------

    [Test]
    public void PurchasingPowerStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        AddPopGroup(state, settlementId, PopGroupType.Coloni, 1000, WealthBand.Subsistence);
        AddPopGroup(state, settlementId, PopGroupType.Curiales, 5, WealthBand.EliteDiscretionary);
        AggregatePurchasingPowerSystem.Tick(state);

        var businessId = PromotedBusiness(state, "Tool Shop", districtId, ToolsId);
        BusinessViabilitySystem.Tick(state, new GameDate(1));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.AggregateDemandReadings.Count, Is.EqualTo(1));
            Assert.That(restored.BusinessViabilityChecks.Count, Is.EqualTo(1));
        });

        AggregateDemandResolver.TryGetCurrent(restored, settlementId, out var restoredReading);
        BusinessViabilityResolver.TryGetCurrent(restored, businessId, out var restoredCheck);
        Assert.Multiple(() =>
        {
            Assert.That(restoredReading.SubsistencePopulation, Is.EqualTo(1000));
            Assert.That(restoredReading.EliteDiscretionaryPopulation, Is.EqualTo(5));
            Assert.That(restoredCheck.OutputGoodTier, Is.EqualTo(WealthBand.ModestSurplus));
        });
    }

    private static MonthlyTickContext Tick(int month) => new(new GameDate(month), new RandomStreamSet());
}
