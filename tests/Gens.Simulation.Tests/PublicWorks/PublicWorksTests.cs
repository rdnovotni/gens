using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.PublicWorks;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.PublicWorks;

/// <summary>Phase 15 item 9 coverage: every work type (§3) and both funding sources (§7), the private
/// funder's real Dignitas/inscription credit (§4), the Euergetism Obligation's quiet Dignitas cost for a
/// wealthy household that never funds anything (§2), Competitive Euergetism's escalation ladder (§5),
/// maintenance decay and its severe-neglect Scandal trigger (§6), the real cross-system effects (§3/§8:
/// Health, Contentment, District Property Value, Treasury income, Notable Business income), and a
/// save/load round trip with deterministic hash stability (<c>gens-public-works-euergetism-design.md</c>).</summary>
public sealed class PublicWorksTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId) InlandSettlement(SettlementStage stage = SettlementStage.Villa)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, stage));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, TerrainFeature.None));
        return (state, settlementId);
    }

    private static (WorldState State, RuntimeId<Settlement> SettlementId) CoastalSettlement(SettlementStage stage = SettlementStage.Villa)
    {
        var (state, settlementId) = InlandSettlement(stage);
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Coast, TerrainFeature.None));
        return (state, settlementId);
    }

    private static RuntimeId<Household> HouseholdWithHead(WorldState state, string nomen = "Cornelius")
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        return householdId;
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static void FundTreasury(WorldState state, RuntimeId<Settlement> settlementId, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static Money BalanceOf(WorldState state, RuntimeId<Household> householdId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;

    private static Money TreasuryBalanceOf(WorldState state, RuntimeId<Settlement> settlementId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForSettlementTreasury(settlementId), out var account) ? account!.Balance : Money.Zero;

    private static void SetNetWorth(WorldState state, RuntimeId<Household> householdId, Money total) =>
        state.NetWorthAssessments.Add(householdId, new NetWorth(householdId, state.Date, total, Money.Zero, Money.Zero, total));

    private static RuntimeId<PublicWork> FundWork(
        WorldState state, RuntimeId<Settlement> settlementId, PublicWorkType workType, PublicWorkFundingSource fundingSource,
        RuntimeId<District>? districtId = null, PropertyOwnerRef? patron = null, GameDate? date = null)
    {
        var result = FundPublicWorkCommands.Pipeline.Execute(
            state, new FundPublicWorkCommand(
                state.CommandIds.Issue(), "player", date ?? new GameDate(0), null, settlementId, workType, fundingSource, districtId, patron));
        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        return state.PublicWorks.InAscendingOrder().Last().Key;
    }

    // ---- §3/§7 every work type & funding source ------------------------------------------------

    [Test]
    public void EveryWorkTypeCanBeStateFunded()
    {
        var (state, settlementId) = CoastalSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(10_000));

        foreach (var workType in Enum.GetValues<PublicWorkType>())
        {
            var id = FundWork(state, settlementId, workType, PublicWorkFundingSource.StateTaxRevenue);
            Assert.That(state.PublicWorks.TryGet(id, out var work), Is.True);
            Assert.That(work!.HasInscription, Is.False, $"{workType} state funding should carry no inscription.");
            Assert.That(work.FundingPatronId, Is.Null);
        }
    }

    [Test]
    public void StateFundingDrawsFromTheSettlementTreasuryWithNoPatronCredit()
    {
        var (state, settlementId) = InlandSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(1000));
        var before = TreasuryBalanceOf(state, settlementId);

        FundWork(state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.StateTaxRevenue);

        Assert.That(TreasuryBalanceOf(state, settlementId), Is.EqualTo(before - PublicWorksCatalog.ConstructionCost(PublicWorkType.Road)));
    }

    [Test]
    public void PrivateEuergetismByAPlayerHouseholdDebitsTheirLedgerAndAwardsDignitasAndInscription()
    {
        var (state, settlementId) = InlandSettlement();
        var householdId = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(2000));
        var before = BalanceOf(state, householdId);
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        var workId = FundWork(
            state, settlementId, PublicWorkType.Aqueduct, PublicWorkFundingSource.PrivateEuergetism, patron: PropertyOwnerRef.ForPlayerHousehold(householdId));

        state.PublicWorks.TryGet(workId, out var work);
        Assert.Multiple(() =>
        {
            Assert.That(work!.HasInscription, Is.True);
            Assert.That(BalanceOf(state, householdId), Is.EqualTo(before - PublicWorksCatalog.ConstructionCost(PublicWorkType.Aqueduct)));
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + PublicWorksCatalog.PrivateFundingDignitasAward));
            Assert.That(EuergetismObligationResolver.Current(state, householdId).PublicWorksFundedCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void PrivateEuergetismRejectsAnUnderfundedHousehold()
    {
        var (state, settlementId) = InlandSettlement();
        var householdId = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(1));

        var result = FundPublicWorkCommands.Pipeline.Execute(
            state, new FundPublicWorkCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, PublicWorkType.Aqueduct,
                PublicWorkFundingSource.PrivateEuergetism, FundingPatronId: PropertyOwnerRef.ForPlayerHousehold(householdId)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundPublicWorkCommands.InsufficientFunds));
    }

    [Test]
    public void HarborRequiresACoastalSettlement()
    {
        var (state, settlementId) = InlandSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(2000));

        var result = FundPublicWorkCommands.Pipeline.Execute(
            state, new FundPublicWorkCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, PublicWorkType.Harbor,
                PublicWorkFundingSource.StateTaxRevenue));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FundPublicWorkCommands.HarborRequiresCoastalSettlement));
    }

    [Test]
    public void JointSocietasFundingSharesDignitasAndObligationCreditAcrossPlayerHouseholdPartners()
    {
        var (state, settlementId) = InlandSettlement();
        var partnerA = HouseholdWithHead(state, "Cornelius");
        var partnerB = HouseholdWithHead(state, "Valerius");
        Fund(state, partnerA, Money.FromDenarii(2000));

        var formResult = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners,
                "fund the Marketplace", new[]
                {
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(partnerA), Fixed64.FromRaw(500_000)),
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(partnerB), Fixed64.FromRaw(500_000)),
                }));
        Assert.That(formResult.Accepted, Is.True);
        var societasId = state.Societates.InAscendingOrder().Last().Key;

        var dignitasABefore = DignitasResolver.Current(state, partnerA);
        var dignitasBBefore = DignitasResolver.Current(state, partnerB);

        var result = FundPublicWorkCommands.Pipeline.Execute(
            state, new FundPublicWorkCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, PublicWorkType.Bridge,
                PublicWorkFundingSource.PrivateEuergetism, FundingSocietasId: societasId));
        Assert.That(result.Accepted, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(DignitasResolver.Current(state, partnerA), Is.EqualTo(dignitasABefore + PublicWorksCatalog.PrivateFundingDignitasAward));
            Assert.That(DignitasResolver.Current(state, partnerB), Is.EqualTo(dignitasBBefore + PublicWorksCatalog.PrivateFundingDignitasAward));
            Assert.That(EuergetismObligationResolver.Current(state, partnerA).PublicWorksFundedCount, Is.EqualTo(1));
            Assert.That(EuergetismObligationResolver.Current(state, partnerB).PublicWorksFundedCount, Is.EqualTo(1));
        });
    }

    // ---- §3 cross-system integration -----------------------------------------------------------

    [Test]
    public void OperationalAqueductAndSewerReduceTheSanitationMultiplier()
    {
        var (state, settlementId) = InlandSettlement();
        Assert.That(PublicWorksHealthQuery.SanitationMultiplier(state, settlementId), Is.EqualTo(Fixed64.One));

        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        FundWork(state, settlementId, PublicWorkType.Aqueduct, PublicWorkFundingSource.StateTaxRevenue);
        FundWork(state, settlementId, PublicWorkType.Sewer, PublicWorkFundingSource.StateTaxRevenue);

        var expected = Fixed64.Multiply(PublicWorksCatalog.AqueductSanitationMultiplier, PublicWorksCatalog.SewerSanitationMultiplier);
        Assert.That(PublicWorksHealthQuery.SanitationMultiplier(state, settlementId), Is.EqualTo(expected));
    }

    [Test]
    public void OperationalSewerGrantsARealContentmentBonus()
    {
        var (state, settlementId) = InlandSettlement();
        Assert.That(PublicWorksContentmentQuery.CivicInfrastructureBonus(state, settlementId), Is.EqualTo(Fixed64.Zero));

        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        FundWork(state, settlementId, PublicWorkType.Sewer, PublicWorkFundingSource.StateTaxRevenue);

        Assert.That(PublicWorksContentmentQuery.CivicInfrastructureBonus(state, settlementId), Is.EqualTo(PublicWorksCatalog.SewerContentmentBonus));
    }

    [Test]
    public void BridgeConstructionBumpsEveryPlotsPropertyValueInTheLinkedDistrict()
    {
        var (state, settlementId) = InlandSettlement(SettlementStage.Vicus);
        var districtResult = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, "Forum"));
        Assert.That(districtResult.Accepted, Is.True);
        var districtId = state.Districts.InAscendingOrder().Last().Key;

        var householdId = HouseholdWithHead(state);
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, TerrainFeature.None));
        PlotPropertyResolver.Set(state, PlotPropertyResolver.Current(state, plotId) with { DistrictId = districtId });
        var before = PlotPropertyResolver.Current(state, plotId).Value;

        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        FundWork(state, settlementId, PublicWorkType.Bridge, PublicWorkFundingSource.StateTaxRevenue, districtId: districtId);

        Assert.That(PlotPropertyResolver.Current(state, plotId).Value, Is.EqualTo(before + PublicWorksCatalog.BridgePropertyValueBonusPerPlot));
    }

    [Test]
    public void RoadAndHarborPostARealMonthlyTreasuryIncome()
    {
        var (state, settlementId) = CoastalSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        FundWork(state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.StateTaxRevenue);
        FundWork(state, settlementId, PublicWorkType.Harbor, PublicWorkFundingSource.StateTaxRevenue);

        var before = TreasuryBalanceOf(state, settlementId);
        PublicWorksBenefitsSystem.Tick(state, new GameDate(1));

        var expected = before + PublicWorksCatalog.RoadTreasuryMonthlyBonus + PublicWorksCatalog.HarborTreasuryMonthlyBonus;
        Assert.That(TreasuryBalanceOf(state, settlementId), Is.EqualTo(expected));
    }

    [Test]
    public void MarketplacePostsARealMonthlyIncomeToEveryHouseholdOwnedBusinessInTheDistrict()
    {
        var (state, settlementId) = InlandSettlement(SettlementStage.Vicus);
        var districtResult = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, "Forum"));
        var districtId = state.Districts.InAscendingOrder().Last().Key;

        var householdId = HouseholdWithHead(state);
        var businessId = state.NotableBusinessIds.Issue();
        state.NotableBusinesses.Add(businessId, NotableBusiness.Create(
            businessId, "The Fuller's Shop", PropertyOwnerRef.ForPlayerHousehold(householdId), NotableBusinessTrigger.AmbientSample,
            new GameDate(0), outputGoodId: null, linkedPropertyRecordId: null, districtId: districtId));

        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        FundWork(state, settlementId, PublicWorkType.MarketplaceOrBasilica, PublicWorkFundingSource.StateTaxRevenue, districtId: districtId);

        var before = BalanceOf(state, householdId);
        PublicWorksBenefitsSystem.Tick(state, new GameDate(1));

        Assert.That(BalanceOf(state, householdId), Is.EqualTo(before + PublicWorksCatalog.MarketplaceBusinessMonthlyBonus));
    }

    // ---- §6 Maintenance & neglect ----------------------------------------------------------------

    [Test]
    public void PaidUpkeepLeavesConditionUntouchedAndResetsTheNeglectStreak()
    {
        var (state, settlementId) = InlandSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        var workId = FundWork(state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.StateTaxRevenue);

        PublicWorksMaintenanceSystem.Tick(state, new GameDate(1));

        state.PublicWorks.TryGet(workId, out var work);
        Assert.Multiple(() =>
        {
            Assert.That(work!.Condition, Is.EqualTo(PublicWorksCatalog.PristineCondition));
            Assert.That(work.ConsecutiveNeglectedMonths, Is.EqualTo(0));
        });
    }

    [Test]
    public void UnpaidUpkeepDecaysConditionAndAdvancesTheNeglectStreak()
    {
        var (state, settlementId) = InlandSettlement();
        var householdId = HouseholdWithHead(state);
        Fund(state, householdId, PublicWorksCatalog.ConstructionCost(PublicWorkType.Road));
        var workId = FundWork(
            state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.PrivateEuergetism, patron: PropertyOwnerRef.ForPlayerHousehold(householdId));
        // Household's ledger balance is now zero -- every future month's upkeep goes unpaid.

        PublicWorksMaintenanceSystem.Tick(state, new GameDate(1));

        state.PublicWorks.TryGet(workId, out var work);
        Assert.Multiple(() =>
        {
            Assert.That(work!.Condition, Is.EqualTo(PublicWorksCatalog.PristineCondition - PublicWorksCatalog.UnpaidUpkeepConditionLoss));
            Assert.That(work.ConsecutiveNeglectedMonths, Is.EqualTo(1));
        });
    }

    [Test]
    public void FundingUpkeepRestoresConditionAndClearsTheNeglectStreak()
    {
        var (state, settlementId) = InlandSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(2000));
        var workId = FundWork(state, settlementId, PublicWorkType.Sewer, PublicWorkFundingSource.StateTaxRevenue);
        for (var i = 0; i < 3; i++)
            PublicWorksMaintenanceSystem.Tick(state, new GameDate(i + 1)); // Treasury has plenty of funds -- these all pay.

        // Force a real, unpaid neglect streak by draining the Treasury.
        var remaining = TreasuryBalanceOf(state, settlementId);
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(settlementId), -remaining), new LedgerPosting(LedgerAccountKey.Mint, remaining) });
        PublicWorksMaintenanceSystem.Tick(state, new GameDate(4));
        state.PublicWorks.TryGet(workId, out var neglected);
        Assert.That(neglected!.ConsecutiveNeglectedMonths, Is.EqualTo(1));

        var payingHouseholdId = HouseholdWithHead(state);
        Fund(state, payingHouseholdId, Money.FromDenarii(500));
        var result = FundPublicWorkUpkeepCommands.Pipeline.Execute(
            state, new FundPublicWorkUpkeepCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, workId, payingHouseholdId));
        Assert.That(result.Accepted, Is.True);

        state.PublicWorks.TryGet(workId, out var restored);
        Assert.Multiple(() =>
        {
            Assert.That(restored!.Condition, Is.GreaterThan(neglected.Condition));
            Assert.That(restored.ConsecutiveNeglectedMonths, Is.EqualTo(0));
        });
    }

    [Test]
    public void SevereNeglectRejectsTheScandalCommandUntilBothGatesAreMet()
    {
        var (state, settlementId) = InlandSettlement();
        var householdId = HouseholdWithHead(state);
        Fund(state, householdId, PublicWorksCatalog.ConstructionCost(PublicWorkType.Aqueduct));
        var workId = FundWork(
            state, settlementId, PublicWorkType.Aqueduct, PublicWorkFundingSource.PrivateEuergetism, patron: PropertyOwnerRef.ForPlayerHousehold(householdId));

        // Not yet neglected long enough.
        PublicWorksMaintenanceSystem.Tick(state, new GameDate(1));
        var earlyResult = RecordEuergetismNeglectScandalCommands.Pipeline.Execute(
            state, new RecordEuergetismNeglectScandalCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, workId));
        Assert.That(earlyResult.Error, Is.EqualTo(RecordEuergetismNeglectScandalCommands.NotSeverelyNeglected));

        var monthsToSeverelyNeglect = Math.Max(
            PublicWorksCatalog.SevereNeglectConsecutiveMonths,
            ((PublicWorksCatalog.PristineCondition - PublicWorksCatalog.SevereNeglectConditionThreshold) / PublicWorksCatalog.UnpaidUpkeepConditionLoss) + 1);
        for (var month = 2; month <= monthsToSeverelyNeglect + 1; month++)
            PublicWorksMaintenanceSystem.Tick(state, new GameDate(month));

        state.PublicWorks.TryGet(workId, out var work);
        Assert.That(work!.Condition, Is.LessThan(PublicWorksCatalog.SevereNeglectConditionThreshold));
        Assert.That(work.ConsecutiveNeglectedMonths, Is.GreaterThanOrEqualTo(PublicWorksCatalog.SevereNeglectConsecutiveMonths));

        var dignitasBefore = DignitasResolver.Current(state, householdId);
        var scandalResult = RecordEuergetismNeglectScandalCommands.Pipeline.Execute(
            state, new RecordEuergetismNeglectScandalCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, workId));

        Assert.Multiple(() =>
        {
            Assert.That(scandalResult.Accepted, Is.True);
            Assert.That(state.ScandalRecords.Count, Is.EqualTo(1));
            Assert.That(state.ScandalRecords.InAscendingOrder().Single().Value.SourceType, Is.EqualTo(ScandalSourceType.PublicWorksNeglect));
            Assert.That(DignitasResolver.Current(state, householdId), Is.LessThan(dignitasBefore));
        });
    }

    [Test]
    public void StateFundedWorkNeverReachesTheNeglectScandalCommand()
    {
        var (state, settlementId) = InlandSettlement();
        FundTreasury(state, settlementId, Money.FromDenarii(10));
        var workId = FundWork(state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.StateTaxRevenue);
        var monthsToSeverelyNeglect = Math.Max(
            PublicWorksCatalog.SevereNeglectConsecutiveMonths,
            ((PublicWorksCatalog.PristineCondition - PublicWorksCatalog.SevereNeglectConditionThreshold) / PublicWorksCatalog.UnpaidUpkeepConditionLoss) + 1);
        for (var month = 1; month <= monthsToSeverelyNeglect + 1; month++)
            PublicWorksMaintenanceSystem.Tick(state, new GameDate(month));

        var result = RecordEuergetismNeglectScandalCommands.Pipeline.Execute(
            state, new RecordEuergetismNeglectScandalCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, workId));

        Assert.That(result.Error, Is.EqualTo(RecordEuergetismNeglectScandalCommands.NoResolvableHouseholdPatron));
    }

    // ---- §2 Euergetism Obligation ------------------------------------------------------------------

    [Test]
    public void AWealthyHouseholdThatNeverFundsAnythingAccruesAQuietOngoingDignitasCost()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = HouseholdWithHead(state);
        SetNetWorth(state, householdId, PublicWorksCatalog.ObligationNetWorthThreshold);

        for (var month = 1; month <= PublicWorksCatalog.ObligationGracePeriodMonths; month++)
            EuergetismObligationSystem.Tick(state, new GameDate(month));

        Assert.That(EuergetismObligationResolver.Current(state, householdId).PerceivedAsNeglectful, Is.False, "Grace period has not yet fully elapsed.");

        var dignitasBefore = DignitasResolver.Current(state, householdId);
        EuergetismObligationSystem.Tick(state, new GameDate(PublicWorksCatalog.ObligationGracePeriodMonths + 1));

        Assert.Multiple(() =>
        {
            Assert.That(EuergetismObligationResolver.Current(state, householdId).PerceivedAsNeglectful, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasBefore + PublicWorksCatalog.ObligationMonthlyDignitasPenalty));
        });
    }

    [Test]
    public void AHouseholdBelowTheThresholdAccruesNoObligation()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = HouseholdWithHead(state);
        SetNetWorth(state, householdId, PublicWorksCatalog.ObligationNetWorthThreshold - Money.FromDenarii(1));

        for (var month = 1; month <= PublicWorksCatalog.ObligationGracePeriodMonths + 5; month++)
            EuergetismObligationSystem.Tick(state, new GameDate(month));

        Assert.That(EuergetismObligationResolver.Current(state, householdId).PerceivedAsNeglectful, Is.False);
    }

    [Test]
    public void FundingAPublicWorkClearsAnAlreadyNeglectfulReading()
    {
        var (state, settlementId) = InlandSettlement();
        var householdId = HouseholdWithHead(state);
        SetNetWorth(state, householdId, PublicWorksCatalog.ObligationNetWorthThreshold);
        for (var month = 1; month <= PublicWorksCatalog.ObligationGracePeriodMonths + 1; month++)
            EuergetismObligationSystem.Tick(state, new GameDate(month));
        Assert.That(EuergetismObligationResolver.Current(state, householdId).PerceivedAsNeglectful, Is.True);

        Fund(state, householdId, Money.FromDenarii(2000));
        FundWork(
            state, settlementId, PublicWorkType.Road, PublicWorkFundingSource.PrivateEuergetism, patron: PropertyOwnerRef.ForPlayerHousehold(householdId),
            date: new GameDate(PublicWorksCatalog.ObligationGracePeriodMonths + 1));

        Assert.That(EuergetismObligationResolver.Current(state, householdId).PerceivedAsNeglectful, Is.False);
    }

    // ---- §5 Competitive Euergetism -----------------------------------------------------------------

    [Test]
    public void CompetitiveEuergetismEscalatesRoundsWithScalingCostAndDignitas()
    {
        var (state, settlementId) = InlandSettlement();
        var initiator = HouseholdWithHead(state, "Initiator");
        var responder = HouseholdWithHead(state, "Responder");
        Fund(state, responder, Money.FromDenarii(5000));

        var initiateResult = InitiateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new InitiateCompetitiveEuergetismCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId,
                PropertyOwnerRef.ForPlayerHousehold(initiator), PropertyOwnerRef.ForPlayerHousehold(responder)));
        Assert.That(initiateResult.Accepted, Is.True);
        var eventId = state.CompetitiveEuergetismEvents.InAscendingOrder().Last().Key;

        var balanceBefore = BalanceOf(state, responder);
        var dignitasBefore = DignitasResolver.Current(state, responder);

        var escalateResult = EscalateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new EscalateCompetitiveEuergetismCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, eventId));
        Assert.That(escalateResult.Accepted, Is.True);

        state.CompetitiveEuergetismEvents.TryGet(eventId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(record!.EscalationRound, Is.EqualTo(2));
            Assert.That(record.InitiatingHouseholdId, Is.EqualTo(PropertyOwnerRef.ForPlayerHousehold(responder)), "Rounds alternate who is currently responding.");
            Assert.That(BalanceOf(state, responder), Is.LessThan(balanceBefore));
            Assert.That(DignitasResolver.Current(state, responder), Is.GreaterThan(dignitasBefore));
        });
    }

    [Test]
    public void CompetitiveEuergetismCannotEscalatePastTheCeiling()
    {
        var (state, settlementId) = InlandSettlement();
        var initiator = HouseholdWithHead(state, "Initiator");
        var responder = HouseholdWithHead(state, "Responder");
        Fund(state, initiator, Money.FromDenarii(20_000));
        Fund(state, responder, Money.FromDenarii(20_000));

        InitiateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new InitiateCompetitiveEuergetismCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId,
                PropertyOwnerRef.ForPlayerHousehold(initiator), PropertyOwnerRef.ForPlayerHousehold(responder)));
        var eventId = state.CompetitiveEuergetismEvents.InAscendingOrder().Last().Key;

        for (var round = 1; round < PublicWorksCatalog.MaxEscalationRounds; round++)
        {
            var result = EscalateCompetitiveEuergetismCommands.Pipeline.Execute(
                state, new EscalateCompetitiveEuergetismCommand(state.CommandIds.Issue(), "player", new GameDate(round), null, eventId));
            Assert.That(result.Accepted, Is.True, $"Round {round} unexpectedly rejected: {result.Error}");
        }

        var ceilingResult = EscalateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new EscalateCompetitiveEuergetismCommand(state.CommandIds.Issue(), "player", new GameDate(99), null, eventId));

        Assert.That(ceilingResult.Error, Is.EqualTo(EscalateCompetitiveEuergetismCommands.AtCeiling));
    }

    [Test]
    public void InitiateCompetitiveEuergetismRejectsARivalGensParticipantWithNoLiveActor()
    {
        var (state, settlementId) = InlandSettlement();
        var initiator = HouseholdWithHead(state);

        var result = InitiateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new InitiateCompetitiveEuergetismCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId,
                PropertyOwnerRef.ForPlayerHousehold(initiator), PropertyOwnerRef.ForTemple("Temple of Diana")));

        Assert.That(result.Error, Is.EqualTo(InitiateCompetitiveEuergetismCommands.UnsupportedHouseholdKind));
    }

    // ---- Save/load round trip -----------------------------------------------------------------------

    [Test]
    public void PublicWorksStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId) = CoastalSettlement(SettlementStage.Vicus);
        var districtResult = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, settlementId, "Forum"));
        Assert.That(districtResult.Accepted, Is.True);
        var districtId = state.Districts.InAscendingOrder().Last().Key;

        var householdId = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        SetNetWorth(state, householdId, PublicWorksCatalog.ObligationNetWorthThreshold);
        FundTreasury(state, settlementId, Money.FromDenarii(5000));

        FundWork(state, settlementId, PublicWorkType.Harbor, PublicWorkFundingSource.StateTaxRevenue, districtId: districtId);
        FundWork(
            state, settlementId, PublicWorkType.Aqueduct, PublicWorkFundingSource.PrivateEuergetism, patron: PropertyOwnerRef.ForPlayerHousehold(householdId));

        PublicWorksMaintenanceSystem.Tick(state, new GameDate(1));
        PublicWorksBenefitsSystem.Tick(state, new GameDate(1));
        EuergetismObligationSystem.Tick(state, new GameDate(1));

        var rivalActorId = state.ActorIds.Issue();
        state.Actors.Add(rivalActorId, LivingWorldActor.Create(
            rivalActorId, LivingWorldActorType.Gens, "Valeria", LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Wealthy, Money.FromDenarii(50_000)),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), state.RegionIds.Issue(), settlementId));
        InitiateCompetitiveEuergetismCommands.Pipeline.Execute(
            state, new InitiateCompetitiveEuergetismCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId,
                PropertyOwnerRef.ForPlayerHousehold(householdId), PropertyOwnerRef.ForRivalGens(rivalActorId)));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.PublicWorks.Count, Is.EqualTo(state.PublicWorks.Count));
            Assert.That(restored.EuergetismObligations.Count, Is.EqualTo(state.EuergetismObligations.Count));
            Assert.That(restored.CompetitiveEuergetismEvents.Count, Is.EqualTo(1));
        });
    }
}
