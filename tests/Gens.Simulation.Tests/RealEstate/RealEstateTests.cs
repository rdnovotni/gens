using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.RealEstate;

/// <summary>Phase 15 item 1 coverage — the land/property market, Districts, leasing/Operators,
/// valuations, portfolio oversight (Administrative Burden), and the Settlement Demographics
/// displacement integration (<c>gens-land-ownership-real-estate-design.md</c> §2-6, §9-11).</summary>
public sealed class RealEstateTests
{
    private static MonthlyTickContext Tick(int month) => new(new GameDate(month), new RandomStreamSet());

    private static (WorldState State, RuntimeId<Settlement> SettlementId) OneVicusSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Test Region"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));
        return (state, settlementId);
    }

    private static RuntimeId<Plot> OwnedPlot(WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId)
    {
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, ownerId: PropertyOwnerRef.ForPlayerHousehold(householdId).ToTaggedOwnerId()));
        return plotId;
    }

    private static RuntimeId<Character> AliveCharacter(WorldState state, RuntimeId<Household> householdId, int loyalty = 80, int ambition = 30, int stewardship = 50)
    {
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, household: householdId,
            condition: new Condition(80, 20, loyalty, ambition, 50),
            attributes: new CoreAttributes(50, 50, stewardship, 50, 50)));
        return characterId;
    }

    // ---- PropertyOwnerRef -------------------------------------------------------------------------

    [Test]
    public void PropertyOwnerRefRoundTripsThroughItsTaggedForm()
    {
        var householdId = new RuntimeIdCounter<Household>().Issue();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);

        var parsed = PropertyOwnerRef.Parse(owner.ToTaggedOwnerId());

        Assert.That(parsed, Is.EqualTo(owner));
    }

    [Test]
    public void PropertyOwnerRefParsesALegacyBareHouseholdTagAsPlayerHousehold()
    {
        var householdId = new RuntimeIdCounter<Household>().Issue();

        var parsed = PropertyOwnerRef.Parse(householdId.ToTaggedString());

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Kind, Is.EqualTo(PropertyOwnerKind.PlayerHousehold));
            Assert.That(parsed.OwnerId, Is.EqualTo(householdId.ToTaggedString()));
        });
    }

    [Test]
    public void PropertyOwnerRefParsesLegacyBareCharacterSettlementAndActorTags()
    {
        var characterId = new RuntimeIdCounter<Character>().Issue();
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var actorId = new RuntimeIdCounter<Actor>().Issue();

        Assert.Multiple(() =>
        {
            Assert.That(PropertyOwnerRef.Parse(characterId.ToTaggedString()).Kind, Is.EqualTo(PropertyOwnerKind.IndividualCharacter));
            Assert.That(PropertyOwnerRef.Parse(settlementId.ToTaggedString()).Kind, Is.EqualTo(PropertyOwnerKind.Municipal));
            Assert.That(PropertyOwnerRef.Parse(actorId.ToTaggedString()).Kind, Is.EqualTo(PropertyOwnerKind.RivalGens));
        });
    }

    [Test]
    public void PropertyOwnerRefTryParseFailsGracefullyForAnUnrecognizedLegacyTag()
    {
        var parsed = PropertyOwnerRef.TryParse("Temple of Diana Nemorensis", out _);

        Assert.That(parsed, Is.False);
    }

    [Test]
    public void TryResolveSkipsAPlotWithAnUnparseableLegacyOwnerTagInsteadOfThrowing()
    {
        var (state, settlementId) = OneVicusSettlement();
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, ownerId: "Temple of Diana Nemorensis"));

        var resolved = PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out _);

        Assert.That(resolved, Is.False);
    }

    [Test]
    public void PropertyOwnerRefCoversEveryOwnershipTypeSection2Names()
    {
        var actorId = new RuntimeIdCounter<Actor>().Issue();
        var characterId = new RuntimeIdCounter<Character>().Issue();
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();

        Assert.Multiple(() =>
        {
            Assert.That(PropertyOwnerRef.ForRivalGens(actorId).Kind, Is.EqualTo(PropertyOwnerKind.RivalGens));
            Assert.That(PropertyOwnerRef.ForIndividualCharacter(characterId).Kind, Is.EqualTo(PropertyOwnerKind.IndividualCharacter));
            Assert.That(PropertyOwnerRef.ForTemple("Temple of Diana").Kind, Is.EqualTo(PropertyOwnerKind.Temple));
            Assert.That(PropertyOwnerRef.ForCollegium(actorId).Kind, Is.EqualTo(PropertyOwnerKind.Collegium));
            Assert.That(PropertyOwnerRef.RomanState.Kind, Is.EqualTo(PropertyOwnerKind.RomanState));
            Assert.That(PropertyOwnerRef.ForMunicipal(settlementId).Kind, Is.EqualTo(PropertyOwnerKind.Municipal));
            Assert.That(PropertyOwnerRef.ForSocietasPlaceholder("Societas Navalis").Kind, Is.EqualTo(PropertyOwnerKind.Societas));
            Assert.That(PropertyOwnerRef.ImperialPatrimonium.Kind, Is.EqualTo(PropertyOwnerKind.ImperialPatrimonium));
        });
    }

    // ---- EstablishDistrictCommand (§4) ------------------------------------------------------------

    [Test]
    public void EstablishDistrictSucceedsForAVicusSettlement()
    {
        var (state, settlementId) = OneVicusSettlement();

        var result = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, "Forum District"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.Districts.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void EstablishDistrictRejectsASettlementBelowVicusStage()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Test Region"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Villa));

        var result = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, "Too Early"));

        Assert.That(result.Error, Is.EqualTo(EstablishDistrictCommands.SettlementTooSmall));
    }

    [Test]
    public void EstablishDistrictRejectsOnceTheStagesSoftCapIsReached()
    {
        var (state, settlementId) = OneVicusSettlement();
        EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, "Only District"));

        var result = EstablishDistrictCommands.Pipeline.Execute(
            state, new EstablishDistrictCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, "Second District"));

        Assert.That(result.Error, Is.EqualTo(EstablishDistrictCommands.DistrictCapReached));
    }

    // ---- DistrictPropertyValueSystem (§4) ---------------------------------------------------------

    [Test]
    public void DistrictPropertyValueRisesWithHighContentmentAndFallsWithDisasterDamage()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District"));

        var groupKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(groupKey, PopGroup.Create(
            settlementId, PopGroupType.Operarii, size: 100, contentment: Fixed64.FromRaw(900_000)));

        var system = new DistrictPropertyValueSystem();
        system.Tick(state, Tick(1));
        state.Districts.TryGet(districtId, out var afterContentment);

        Assert.That(afterContentment!.PropertyValue, Is.GreaterThan(RealEstateCatalog.BaselinePropertyValue));

        // A recorded disaster now depresses the target relative to the untouched baseline.
        var disasterId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(disasterId, DisasterEvent.Create(
            disasterId, settlementId, new GameDate(1), HazardType.Fire, DisasterSeverity.Severe, buildingsDamaged: 20));

        // Reset Contentment to neutral so the disaster term is the only thing moving the target this tick.
        state.PopGroups.Remove(groupKey);
        state.PopGroups.Add(groupKey, PopGroup.Create(settlementId, PopGroupType.Operarii, size: 100, contentment: Fixed64.FromRaw(500_000)));

        for (var i = 0; i < 20; i++)
            system.Tick(state, Tick(2 + i));

        state.Districts.TryGet(districtId, out var afterDisaster);
        Assert.That(afterDisaster!.PropertyValue, Is.LessThan(afterContentment.PropertyValue));
    }

    [Test]
    public void DistrictPropertyValueNeverDropsBelowTheMinimumFloor()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Warehouse District"));

        var disasterId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(disasterId, DisasterEvent.Create(
            disasterId, settlementId, new GameDate(0), HazardType.Fire, DisasterSeverity.Catastrophic, buildingsDamaged: 500));

        var system = new DistrictPropertyValueSystem();
        for (var i = 0; i < 50; i++)
            system.Tick(state, Tick(i));

        state.Districts.TryGet(districtId, out var district);
        Assert.That(district!.PropertyValue, Is.GreaterThanOrEqualTo(RealEstateCatalog.MinimumPropertyValue));
    }

    [Test]
    public void DistrictPropertyValueChangesRepriceLinkedPlotsAndPropertyRecords()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District"));

        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetDistrict(state, PropertySubjectRef.ForPlot(plotId), districtId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(10000));

        var recordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(recordId, PropertyRecord.Create(
            recordId, PropertyAssetType.NamedHolding, "Warehouse of the Grain Guild",
            PropertyOwnerRef.ForTemple("Temple of Ceres"), Money.FromDenarii(2000), settlementId, districtId));

        var groupKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(groupKey, PopGroup.Create(
            settlementId, PopGroupType.Operarii, size: 100, contentment: Fixed64.FromRaw(900_000)));

        var system = new DistrictPropertyValueSystem();
        for (var month = 1; month <= 5; month++)
            system.Tick(state, Tick(month));

        state.Districts.TryGet(districtId, out var district);
        Assert.That(district!.PropertyValue, Is.GreaterThan(RealEstateCatalog.BaselinePropertyValue));

        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var plotView);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPropertyRecord(recordId), out var recordView);
        Assert.Multiple(() =>
        {
            Assert.That(plotView.Value, Is.GreaterThan(Money.FromDenarii(10000)),
                "A gentrifying District's rising Property Value must reprice the Plots linked to it.");
            Assert.That(recordView.Value, Is.GreaterThan(Money.FromDenarii(2000)),
                "The same repricing must reach a linked PropertyRecord (Ship/Named Holding), not just Plots.");
        });
    }

    // ---- TransferPropertyCommand (§5, §9) ---------------------------------------------------------

    [Test]
    public void VoluntarySaleTransfersAPlotAndPostsTheLedger()
    {
        var (state, settlementId) = OneVicusSettlement();
        var sellerHouseholdId = state.HouseholdIds.Issue();
        var buyerHouseholdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, sellerHouseholdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(1000));

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyTransferMethod.VoluntarySale, PropertyOwnerRef.ForPlayerHousehold(buyerHouseholdId)));

        Assert.That(result.Accepted, Is.True);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.That(view.Owner, Is.EqualTo(PropertyOwnerRef.ForPlayerHousehold(buyerHouseholdId)));

        var buyerBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(buyerHouseholdId), out var buyerAccount)
            ? buyerAccount.Balance : Money.Zero;
        Assert.That(buyerBalance, Is.EqualTo(Money.FromDenarii(-1000)));
    }

    [Test]
    public void AgerPublicusLeaseSetsALesseeWithoutChangingOwnership()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, ownerId: PropertyOwnerRef.RomanState.ToTaggedOwnerId()));

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyTransferMethod.AgerPublicusLease, PropertyOwnerRef.ForPlayerHousehold(householdId)));

        Assert.That(result.Accepted, Is.True);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.Multiple(() =>
        {
            Assert.That(view.Owner.Kind, Is.EqualTo(PropertyOwnerKind.RomanState));
            Assert.That(view.LesseeId, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void AgerPublicusLeaseRejectsAPropertyThatIsNotRomanState()
    {
        var (state, settlementId) = OneVicusSettlement();
        var sellerHouseholdId = state.HouseholdIds.Issue();
        var lesseeHouseholdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, sellerHouseholdId);

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyTransferMethod.AgerPublicusLease, PropertyOwnerRef.ForPlayerHousehold(lesseeHouseholdId)));

        Assert.That(result.Error, Is.EqualTo(TransferPropertyCommands.NotAgerPublicus));
    }

    [Test]
    public void PersuasionSpendsInfluenceAndTransfersATemplesNamedHolding()
    {
        var (state, settlementId) = OneVicusSettlement();
        var recordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(recordId, PropertyRecord.Create(
            recordId, PropertyAssetType.NamedHolding, "Temple Warehouse", PropertyOwnerRef.ForTemple("Temple of Diana"),
            Money.FromDenarii(500), settlementId));

        var buyerHouseholdId = state.HouseholdIds.Issue();
        InfluenceResolver.Apply(state, buyerHouseholdId, 50);

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPropertyRecord(recordId),
                PropertyTransferMethod.Persuasion, PropertyOwnerRef.ForPlayerHousehold(buyerHouseholdId), PersuasionInfluenceCost: 30));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(InfluenceResolver.Current(state, buyerHouseholdId), Is.EqualTo(20));
        });
        state.PropertyRecords.TryGet(recordId, out var record);
        Assert.That(record!.Owner, Is.EqualTo(PropertyOwnerRef.ForPlayerHousehold(buyerHouseholdId)));
    }

    [Test]
    public void PersuasionRejectsWhenTheHouseholdLacksEnoughInfluence()
    {
        var (state, settlementId) = OneVicusSettlement();
        var recordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(recordId, PropertyRecord.Create(
            recordId, PropertyAssetType.NamedHolding, "Collegium Workshop", PropertyOwnerRef.ForCollegium(state.ActorIds.Issue()),
            Money.FromDenarii(500), settlementId));
        var buyerHouseholdId = state.HouseholdIds.Issue();

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPropertyRecord(recordId),
                PropertyTransferMethod.Persuasion, PropertyOwnerRef.ForPlayerHousehold(buyerHouseholdId), PersuasionInfluenceCost: 30));

        Assert.That(result.Error, Is.EqualTo(TransferPropertyCommands.InsufficientInfluence));
    }

    [Test]
    public void MarketSaleReturnsAPlotToUnownedStateReadyForAcquirePlotCommand()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(1000));

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyTransferMethod.MarketSale, BuyerId: null));

        Assert.That(result.Accepted, Is.True);
        state.Plots.TryGet(plotId, out var plot);
        Assert.That(plot!.OwnerId, Is.Null);

        var acquireResult = AcquirePlotCommands.Pipeline.Execute(
            state, new AcquirePlotCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, plotId, "someone-else", AcquisitionMethod.Purchase));
        Assert.That(acquireResult.Accepted, Is.True);
    }

    [Test]
    public void MarketSaleRemovesAPropertyRecordFromCirculation()
    {
        var (state, settlementId) = OneVicusSettlement();
        var recordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(recordId, PropertyRecord.Create(
            recordId, PropertyAssetType.Ship, "The Swift Gull", PropertyOwnerRef.ForPlayerHousehold(state.HouseholdIds.Issue()),
            Money.FromDenarii(2000)));

        var result = TransferPropertyCommands.Pipeline.Execute(
            state,
            new TransferPropertyCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPropertyRecord(recordId),
                PropertyTransferMethod.MarketSale, BuyerId: null));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.PropertyRecords.TryGet(recordId, out _), Is.False);
        });
    }

    // ---- SetPropertyManagementCommand (§6) --------------------------------------------------------

    [Test]
    public void SetManagementToLeasedOutRequiresALivingOperator()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);

        var result = SetPropertyManagementCommands.Pipeline.Execute(
            state,
            new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, OperatorCharacterId: null));

        Assert.That(result.Error, Is.EqualTo(SetPropertyManagementCommands.OperatorRequiredForLeasedOut));
    }

    [Test]
    public void SetManagementToLeasedOutAssignsTheOperator()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        var operatorId = AliveCharacter(state, householdId);

        var result = SetPropertyManagementCommands.Pipeline.Execute(
            state,
            new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, operatorId));

        Assert.That(result.Accepted, Is.True);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.Multiple(() =>
        {
            Assert.That(view.ManagementStatus, Is.EqualTo(PropertyManagementStatus.LeasedOut));
            Assert.That(view.OperatorCharacterId, Is.EqualTo(operatorId));
        });
    }

    // ---- §6.1 worked example: steady, skimming+audit, ambitious buyout -----------------------------

    [Test]
    public void ASteadyLoyalOperatorRemitsIncomeAndNeverSkims()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(10000));
        var operatorId = AliveCharacter(state, householdId, loyalty: 90);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, operatorId));

        var system = new OperatorLifecycleSystem();
        system.Tick(state, Tick(2));

        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.Multiple(() =>
        {
            Assert.That(view.OperatorIsSkimming, Is.False);
            Assert.That(view.OperatorTenureMonths, Is.EqualTo(1));
        });
        var householdBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
            ? account.Balance : Money.Zero;
        Assert.That(householdBalance, Is.GreaterThan(Money.Zero));
    }

    [Test]
    public void ALowLoyaltyOperatorSkimsAndAnAuditOnAnHonestOperatorCostsLoyalty()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(10000));

        // A skimming Operator (Loyalty below the shared risk threshold).
        var skimmerId = AliveCharacter(state, householdId, loyalty: 10);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, skimmerId));
        new OperatorLifecycleSystem().Tick(state, Tick(2));
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var skimmingView);
        Assert.That(skimmingView.OperatorIsSkimming, Is.True);

        // Auditing the actual skimmer reveals the truth without touching Loyalty.
        var auditSkimmerResult = AuditPropertyOperatorCommands.Pipeline.Execute(
            state, new AuditPropertyOperatorCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, PropertySubjectRef.ForPlot(plotId)));
        state.Characters.TryGet(skimmerId, out var skimmerAfterAudit);
        Assert.Multiple(() =>
        {
            Assert.That(auditSkimmerResult.Accepted, Is.True);
            Assert.That(((PropertyOperatorAuditedEvent)auditSkimmerResult.Events[0]).WasSkimming, Is.True);
            Assert.That(skimmerAfterAudit!.Condition.Loyalty, Is.EqualTo(10));
        });

        // Replacing with an honest, loyal Operator, then auditing anyway, costs a real Loyalty penalty.
        var honestId = AliveCharacter(state, householdId, loyalty: 80);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(4), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, honestId));
        new OperatorLifecycleSystem().Tick(state, Tick(5));

        var auditHonestResult = AuditPropertyOperatorCommands.Pipeline.Execute(
            state, new AuditPropertyOperatorCommand(state.CommandIds.Issue(), "player", new GameDate(6), null, PropertySubjectRef.ForPlot(plotId)));
        state.Characters.TryGet(honestId, out var honestAfterAudit);
        Assert.Multiple(() =>
        {
            Assert.That(auditHonestResult.Accepted, Is.True);
            Assert.That(((PropertyOperatorAuditedEvent)auditHonestResult.Events[0]).WasSkimming, Is.False);
            Assert.That(honestAfterAudit!.Condition.Loyalty, Is.EqualTo(80 - RealEstateCatalog.FalseAuditAccusationLoyaltyPenalty));
        });
    }

    [Test]
    public void AnAmbitiousLongTenuredOperatorOnAGentrifyingDistrictEventuallyOffersABuyout()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(
            districtId, settlementId, "Forum District", propertyValue: RealEstateCatalog.BuyoutDistrictPropertyValueThreshold + Fixed64.FromInt(1)));

        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(5000));
        PropertyResolver.SetDistrict(state, PropertySubjectRef.ForPlot(plotId), districtId);

        var freedmanId = AliveCharacter(
            state, householdId, loyalty: 90, ambition: RealEstateCatalog.BuyoutAmbitionThreshold + 5,
            stewardship: RealEstateCatalog.BuyoutStewardshipThreshold + 5);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, freedmanId));

        var system = new OperatorLifecycleSystem();
        for (var month = 2; month <= RealEstateCatalog.BuyoutMinimumTenureMonths + 1; month++)
            system.Tick(state, Tick(month));

        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.That(view.OperatorBuyoutOffered, Is.True);

        // §6.1's "converting the Insula into his own independent Individual Character holding."
        var resolveResult = ResolveOperatorBuyoutCommands.Pipeline.Execute(
            state, new ResolveOperatorBuyoutCommand(
                state.CommandIds.Issue(), "player", new GameDate(200), null, PropertySubjectRef.ForPlot(plotId), Accept: true));

        Assert.That(resolveResult.Accepted, Is.True);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var afterBuyout);
        Assert.Multiple(() =>
        {
            Assert.That(afterBuyout.Owner, Is.EqualTo(PropertyOwnerRef.ForIndividualCharacter(freedmanId)));
            Assert.That(afterBuyout.ManagementStatus, Is.EqualTo(PropertyManagementStatus.DirectlyManaged));
            Assert.That(afterBuyout.OperatorCharacterId, Is.Null);
        });
    }

    [Test]
    public void AnOperatorWhoOnceSkimmedNeverQualifiesForABuyoutEvenAfterLoyaltyRecovers()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(
            districtId, settlementId, "Forum District", propertyValue: RealEstateCatalog.BuyoutDistrictPropertyValueThreshold + Fixed64.FromInt(1)));

        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(5000));
        PropertyResolver.SetDistrict(state, PropertySubjectRef.ForPlot(plotId), districtId);

        // Starts unqualified (low Loyalty, actively skimming) — §6.1's buyout precondition is "has
        // never skimmed," not merely "isn't skimming this month."
        var operatorId = AliveCharacter(
            state, householdId, loyalty: 10, ambition: RealEstateCatalog.BuyoutAmbitionThreshold + 5,
            stewardship: RealEstateCatalog.BuyoutStewardshipThreshold + 5);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, operatorId));

        var system = new OperatorLifecycleSystem();
        system.Tick(state, Tick(2));
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var afterSkimming);
        Assert.That(afterSkimming.OperatorHasEverSkimmed, Is.True);

        // Loyalty recovers well above the skimming threshold for the rest of the tenure.
        state.Characters.TryGet(operatorId, out var operatorCharacter);
        var condition = operatorCharacter!.Condition;
        state.Characters.Remove(operatorId);
        state.Characters.Add(operatorId, operatorCharacter with
        {
            Condition = new Condition(condition.Health, condition.Fatigue, 90, condition.Ambition, condition.Fertility),
        });

        for (var month = 3; month <= RealEstateCatalog.BuyoutMinimumTenureMonths + 1; month++)
            system.Tick(state, Tick(month));

        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.Multiple(() =>
        {
            Assert.That(view.OperatorIsSkimming, Is.False, "Loyalty has recovered, so this month's reading is honest.");
            Assert.That(view.OperatorHasEverSkimmed, Is.True, "The historical skim is never erased for the same assignment.");
            Assert.That(view.OperatorBuyoutOffered, Is.False, "A once-skimming Operator never qualifies for a buyout offer.");
        });
    }

    [Test]
    public void DecliningABuyoutOfferClearsTheFlagAndKeepsTheLeaseRunning()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        var operatorId = AliveCharacter(state, householdId);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, operatorId));
        PropertyResolver.SetOperatorState(
            state, PropertySubjectRef.ForPlot(plotId), isSkimming: false, hasEverSkimmed: false, tenureMonths: 130, buyoutOffered: true);

        var result = ResolveOperatorBuyoutCommands.Pipeline.Execute(
            state, new ResolveOperatorBuyoutCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, PropertySubjectRef.ForPlot(plotId), Accept: false));

        Assert.That(result.Accepted, Is.True);
        PropertyResolver.TryResolve(state, PropertySubjectRef.ForPlot(plotId), out var view);
        Assert.Multiple(() =>
        {
            Assert.That(view.OperatorBuyoutOffered, Is.False);
            Assert.That(view.ManagementStatus, Is.EqualTo(PropertyManagementStatus.LeasedOut));
            Assert.That(view.OperatorCharacterId, Is.EqualTo(operatorId));
        });
    }

    // ---- AdministrativeBurdenSystem (§11) ---------------------------------------------------------

    [Test]
    public void NoAdministrativeBurdenBelowTheFreeThreshold()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        for (var i = 0; i < RealEstateCatalog.AdministrativeBurdenFreeThreshold; i++)
            OwnedPlot(state, settlementId, householdId);

        var events = new AdministrativeBurdenSystem().Tick(state, Tick(1));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void DirectlyManagedPropertiesPastTheThresholdCostARealLedgerExpense()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        for (var i = 0; i < RealEstateCatalog.AdministrativeBurdenFreeThreshold + 2; i++)
            OwnedPlot(state, settlementId, householdId);

        var events = new AdministrativeBurdenSystem().Tick(state, Tick(1));

        Assert.That(events, Is.Not.Empty);
        var householdBalance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
            ? account.Balance : Money.Zero;
        Assert.That(householdBalance, Is.EqualTo(-RealEstateCatalog.AdministrativeBurdenCostPerProperty.Scale(Fixed64.FromInt(2))));
    }

    [Test]
    public void ALeasedOutPropertyDoesNotCountAgainstAdministrativeBurden()
    {
        var (state, settlementId) = OneVicusSettlement();
        var householdId = state.HouseholdIds.Issue();
        RuntimeId<Plot>? leasedPlotId = null;
        for (var i = 0; i < RealEstateCatalog.AdministrativeBurdenFreeThreshold + 2; i++)
        {
            var plotId = OwnedPlot(state, settlementId, householdId);
            leasedPlotId ??= plotId;
        }

        var operatorId = AliveCharacter(state, householdId);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(leasedPlotId!.Value),
                PropertyManagementStatus.LeasedOut, operatorId));

        var events = new AdministrativeBurdenSystem().Tick(state, Tick(2));

        // Only 1 property now over the threshold (leasing the extra one out delegated it away).
        var assessed = (AdministrativeBurdenAssessedEvent)events.First(e => e is AdministrativeBurdenAssessedEvent);
        Assert.That(assessed.PropertiesOverThreshold, Is.EqualTo(1));
    }

    // ---- §10 Displacement: District Property Value feeding Contentment/Emigration -----------------

    [Test]
    public void ADistrictsRisingPropertyValueDepressesContentmentForRentExposedGroupsOnly()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(
            districtId, settlementId, "Gentrifying District",
            propertyValue: RealEstateCatalog.RentBurdenPropertyValueThreshold + Fixed64.FromInt(1)));

        var operariiKey = new PopGroupKey(settlementId, PopGroupType.Operarii);
        state.PopGroups.Add(operariiKey, PopGroup.Create(
            settlementId, PopGroupType.Operarii, size: 100,
            employmentRatio: Fixed64.One, housingSatisfaction: Fixed64.One, contentment: Fixed64.One));

        var elitePopKey = new PopGroupKey(settlementId, PopGroupType.Curiales);
        state.PopGroups.Add(elitePopKey, PopGroup.Create(
            settlementId, PopGroupType.Curiales, size: 20,
            employmentRatio: Fixed64.One, housingSatisfaction: Fixed64.One, contentment: Fixed64.One));

        var system = new ContentmentSystem();
        system.Tick(state, Tick(1));

        state.PopGroups.TryGet(operariiKey, out var operarii);
        state.PopGroups.TryGet(elitePopKey, out var elite);
        var baselineContentment = ContentmentCalculator.ComputeContentment(
            Fixed64.One, Fixed64.One, NeedsConsumptionCalculator.SatisfactionFor(DietTier.Meager));

        Assert.Multiple(() =>
        {
            Assert.That(operarii.Contentment, Is.LessThan(baselineContentment));
            Assert.That(elite.Contentment, Is.EqualTo(baselineContentment));
        });
    }

    [Test]
    public void ADepressedContentmentFromRentBurdenIncreasesEmigrationThroughTheExistingFormula()
    {
        var lowContentment = Fixed64.FromRaw(200_000); // Below MigrationCalculator's emigration threshold.
        var rate = MigrationCalculator.EmigrationRate(lowContentment);
        Assert.That(rate, Is.GreaterThan(Fixed64.Zero));
    }

    // ---- Save/load round trip and deterministic hash stability -----------------------------------

    [Test]
    public void RealEstateStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId) = OneVicusSettlement();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District", propertyValue: Fixed64.FromRaw(1_200_000)));

        var householdId = state.HouseholdIds.Issue();
        var plotId = OwnedPlot(state, settlementId, householdId);
        PropertyResolver.SetValue(state, PropertySubjectRef.ForPlot(plotId), Money.FromDenarii(3000));
        PropertyResolver.SetDistrict(state, PropertySubjectRef.ForPlot(plotId), districtId);
        var operatorId = AliveCharacter(state, householdId);
        SetPropertyManagementCommands.Pipeline.Execute(
            state, new SetPropertyManagementCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertySubjectRef.ForPlot(plotId),
                PropertyManagementStatus.LeasedOut, operatorId));

        var recordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(recordId, PropertyRecord.Create(
            recordId, PropertyAssetType.Ship, "The Swift Gull", PropertyOwnerRef.ForPlayerHousehold(householdId), Money.FromDenarii(7000)));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.Districts.Count, Is.EqualTo(1));
            Assert.That(restored.PropertyRecords.Count, Is.EqualTo(1));
        });

        PropertyResolver.TryResolve(restored, PropertySubjectRef.ForPlot(plotId), out var restoredView);
        Assert.Multiple(() =>
        {
            Assert.That(restoredView.ManagementStatus, Is.EqualTo(PropertyManagementStatus.LeasedOut));
            Assert.That(restoredView.OperatorCharacterId, Is.EqualTo(operatorId));
            Assert.That(restoredView.DistrictId, Is.EqualTo(districtId));
            Assert.That(restoredView.Value, Is.EqualTo(Money.FromDenarii(3000)));
        });
    }
}
