using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.NotableBusinesses;

/// <summary>Phase 15 item 4 coverage — §3's sampling and promotion door (including quiet-period
/// demotion and re-promotion), §4's business Reputation and its Scandal cross-integration, §5's Named
/// Competition (including the Coercive Scheme reuse for Sabotage/Damaging Rumor), §6's Named Suppliers
/// and disruption, §7's Government Contracts (grant/end/monthly payment), §8's four new Business
/// Lifecycle behaviors (Merge/Specialize/Move/Lobby), and a save/load round trip
/// (<c>gens-notable-businesses-design.md</c>).</summary>
public sealed class NotableBusinessesTests
{
    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Region> RegionId, RuntimeId<Settlement> SettlementId) OneHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Test Region"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));
        var householdId = state.HouseholdIds.Issue();
        return (state, householdId, regionId, settlementId);
    }

    private static RuntimeId<Character> AliveCharacter(WorldState state)
    {
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        return characterId;
    }

    private static RuntimeId<NotableBusiness> PromotedBusiness(
        WorldState state, string name, PropertyOwnerRef owner, GameDate? date = null,
        NotableBusinessTrigger trigger = NotableBusinessTrigger.DirectPlayerTransaction)
    {
        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", date ?? new GameDate(1), null, name, owner, trigger));
        Assert.That(result.Accepted, Is.True, $"Promotion of '{name}' was rejected: {result.Error}");
        var eventPayload = (NotableBusinessPromotedEvent)result.Events[0];
        return eventPayload.BusinessId;
    }

    // ---- §3 Sampling & Promotion --------------------------------------------------------------

    [Test]
    public void PromoteNotableBusinessCommandCreatesATrackedBusinessForAPlayerHousehold()
    {
        var (state, householdId, _, _) = OneHousehold();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);

        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", owner, trigger: NotableBusinessTrigger.AmbientSample);

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        Assert.Multiple(() =>
        {
            Assert.That(business.Name, Is.EqualTo("Bakery of Marcus Livius"));
            Assert.That(business.Owner, Is.EqualTo(owner));
            Assert.That(business.Status, Is.EqualTo(NotableBusinessStatus.Tracked));
            Assert.That(business.Reputation, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation));
            Assert.That(business.SampledOrTriggeredBy, Is.EqualTo(NotableBusinessTrigger.AmbientSample));
        });
    }

    [Test]
    public void PromoteNotableBusinessCommandRejectsAnOwnerKindOutsideItsRoster()
    {
        var state = new WorldState(new GameDate(0));

        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, "Temple Bakery",
                PropertyOwnerRef.ForTemple("Temple of Diana"), NotableBusinessTrigger.AmbientSample));

        Assert.That(result.Error, Is.EqualTo(PromoteNotableBusinessCommands.InvalidOwnerKind));
    }

    [Test]
    public void PromoteNotableBusinessCommandRejectsAnEmptyName()
    {
        var (state, householdId, _, _) = OneHousehold();

        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, "  ",
                PropertyOwnerRef.ForPlayerHousehold(householdId), NotableBusinessTrigger.AmbientSample));

        Assert.That(result.Error, Is.EqualTo(PromoteNotableBusinessCommands.EmptyName));
    }

    [Test]
    public void NotableBusinessTieringServiceDemotesAQuietBusinessAndRecordContactRePromotesIt()
    {
        var (state, householdId, _, _) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId), new GameDate(0));

        var stillQuiet = NotableBusinessTieringService.DemoteIfQuiet(state, businessId, new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths - 1));
        var demoted = NotableBusinessTieringService.DemoteIfQuiet(state, businessId, new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths));
        var rePromoted = NotableBusinessTieringService.RecordContactAndPromote(state, businessId, new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths + 1));

        Assert.Multiple(() =>
        {
            Assert.That(stillQuiet.Status, Is.EqualTo(NotableBusinessStatus.Tracked));
            Assert.That(demoted.Status, Is.EqualTo(NotableBusinessStatus.Demoted));
            Assert.That(rePromoted.Status, Is.EqualTo(NotableBusinessStatus.Tracked));
            Assert.That(rePromoted.LastRelevantContactDate, Is.EqualTo(new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths + 1)));
        });
    }

    // ---- §4 Business Reputation -------------------------------------------------------------------

    [Test]
    public void AdjustBusinessReputationCommandClampsToTheZeroToHundredScale()
    {
        var (state, householdId, _, _) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));

        AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, 1000, BusinessReputationChangeReason.QualityOutput));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        Assert.That(business.Reputation, Is.EqualTo(NotableBusinessesCatalog.MaxReputation));
    }

    [Test]
    public void AdjustBusinessReputationCommandRejectsADemotedBusiness()
    {
        var (state, householdId, _, _) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId), new GameDate(0));
        NotableBusinessTieringService.DemoteIfQuiet(state, businessId, new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths));

        var result = AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, 5, BusinessReputationChangeReason.QualityOutput));

        Assert.That(result.Error, Is.EqualTo(AdjustBusinessReputationCommands.BusinessNotTracked));
    }

    [Test]
    public void RecordBusinessScandalCommandPenalizesReputationAndRecordsAScandalWithNoPersonalDignitasPenalty()
    {
        var (state, householdId, _, _) = OneHousehold();
        var headId = AliveCharacter(state);
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));

        var result = RecordBusinessScandalCommands.Pipeline.Execute(
            state, new RecordBusinessScandalCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, ScandalSeverity.PublicDisgrace));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        var scandalEvent = (ScandalRecordedEvent)result.Events.Single(e => e is ScandalRecordedEvent);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(business.Reputation, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation - NotableBusinessesCatalog.BusinessScandalReputationLoss));
            Assert.That(scandalEvent.SourceType, Is.EqualTo(ScandalSourceType.BusinessMisconduct));
            Assert.That(DignitasResolver_Current(state, householdId), Is.EqualTo(0), "the ordinary personal Dignitas penalty must be suppressed");
        });
    }

    private static int DignitasResolver_Current(WorldState state, RuntimeId<Household> householdId) =>
        Gens.Simulation.Reputation.DignitasResolver.Current(state, householdId);

    // ---- §5 Named Competition ----------------------------------------------------------------------

    [Test]
    public void RecordBusinessRivalryActionCommandRequiresAMutualMainCompetitorLink()
    {
        var (state, householdId, _, _) = OneHousehold();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);
        var bakeryA = PromotedBusiness(state, "Bakery of Marcus Livius", owner);
        var bakeryB = PromotedBusiness(state, "Bakery of Gaius", owner);

        var result = RecordBusinessRivalryActionCommands.Pipeline.Execute(
            state, new RecordBusinessRivalryActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryA, bakeryB, BusinessRivalryActionType.PriceUndercut));

        Assert.That(result.Error, Is.EqualTo(RecordBusinessRivalryActionCommands.NotMainCompetitors));
    }

    [Test]
    public void RecordBusinessRivalryActionCommandAppliesTheReputationHitAndAppendsTheLog()
    {
        var (state, householdId, _, _) = OneHousehold();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);
        var bakeryA = PromotedBusiness(state, "Bakery of Marcus Livius", owner);
        var bakeryB = PromotedBusiness(state, "Bakery of Gaius", owner);
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryA, bakeryB));
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryB, bakeryA));

        var result = RecordBusinessRivalryActionCommands.Pipeline.Execute(
            state, new RecordBusinessRivalryActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, bakeryB, bakeryA, BusinessRivalryActionType.PriceUndercut));

        NotableBusinessResolver.TryGetCurrent(state, bakeryA, out var targetBusiness);
        var expectedEffect = NotableBusinessesCatalog.RivalryActionReputationEffectFor(BusinessRivalryActionType.PriceUndercut);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(targetBusiness.Reputation, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation + expectedEffect));
        });
    }

    [Test]
    public void RecordBusinessRivalryActionCommandInitiatesACoerciveSchemeForSabotage()
    {
        var (state, householdId, _, _) = OneHousehold();
        var ownerA = AliveCharacter(state);
        var ownerB = AliveCharacter(state);
        var bakeryA = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForIndividualCharacter(ownerA));
        var bakeryB = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForIndividualCharacter(ownerB));
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryA, bakeryB));
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryB, bakeryA));

        RecordBusinessRivalryActionCommands.Pipeline.Execute(
            state, new RecordBusinessRivalryActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, bakeryA, bakeryB, BusinessRivalryActionType.Sabotage));

        var scheme = state.Schemes.InAscendingOrder().Select(e => e.Value).SingleOrDefault(
            s => s.InitiatorCharacterId == ownerA && s.TargetCharacterId == ownerB && s.Type == SchemeType.Coercive);
        Assert.That(scheme, Is.Not.Null);
    }

    // ---- §6 Named Suppliers -------------------------------------------------------------------------

    [Test]
    public void SetMainSupplierCommandRejectsAnUnresolvableCharacterSupplier()
    {
        var (state, householdId, _, _) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        var unknownCharacterId = state.CharacterIds.Issue();

        var result = SetMainSupplierCommands.Pipeline.Execute(
            state, new SetMainSupplierCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId,
                NotableBusinessSupplierRef.ForCharacter(unknownCharacterId)));

        Assert.That(result.Error, Is.EqualTo(SetMainSupplierCommands.SupplierNotFound));
    }

    [Test]
    public void SupplierDisruptionSystemPenalizesOnceWhenTheHouseholdSupplierGoesInsolvent()
    {
        var (state, householdId, _, _) = OneHousehold();
        var supplierHouseholdId = state.HouseholdIds.Issue();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        SetMainSupplierCommands.Pipeline.Execute(
            state, new SetMainSupplierCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, businessId,
                NotableBusinessSupplierRef.ForHousehold(supplierHouseholdId)));
        state.InsolvencyStates.Add(supplierHouseholdId, new InsolvencyState(supplierHouseholdId, 6, InsolvencyStage.Insolvent, Array.Empty<InsolvencyConsequence>()));

        SupplierDisruptionSystem.Tick(state, new GameDate(3));
        var afterFirstTick = GetReputation(state, businessId);
        SupplierDisruptionSystem.Tick(state, new GameDate(4));
        var afterSecondTick = GetReputation(state, businessId);

        Assert.Multiple(() =>
        {
            Assert.That(afterFirstTick, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation - NotableBusinessesCatalog.SupplierDisruptionReputationLoss));
            Assert.That(afterSecondTick, Is.EqualTo(afterFirstTick), "a second monthly tick must not re-penalize the same disruption bout");
        });
    }

    private static int GetReputation(WorldState state, RuntimeId<NotableBusiness> businessId)
    {
        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        return business.Reputation;
    }

    // ---- §7 Government Contracts --------------------------------------------------------------------

    [Test]
    public void GrantGovernmentContractCommandRePromotesADemotedBusiness()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId), new GameDate(0));
        NotableBusinessTieringService.DemoteIfQuiet(state, businessId, new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths));

        var result = GrantGovernmentContractCommands.Pipeline.Execute(
            state, new GrantGovernmentContractCommand(
                state.CommandIds.Issue(), "player", new GameDate(NotableBusinessesCatalog.DemotionQuietPeriodMonths + 1), null, businessId, settlementId));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(business.Status, Is.EqualTo(NotableBusinessStatus.Tracked));
            Assert.That(state.NotableBusinessGovernmentContracts.TryGet(businessId, out _), Is.True);
        });
    }

    [Test]
    public void GovernmentContractPaymentSystemPostsTheMonthlyStipendFromTheSettlementTreasury()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        GrantGovernmentContractCommands.Pipeline.Execute(
            state, new GrantGovernmentContractCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, settlementId));

        GovernmentContractPaymentSystem.Tick(state, new GameDate(3));

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var householdAccount);
        Assert.That(householdAccount.Balance, Is.EqualTo(NotableBusinessesCatalog.GovernmentContractDefaultMonthlyStipend));
    }

    [Test]
    public void EndGovernmentContractCommandPenalizesReputationOnlyWhenItFailedToDeliver()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        GrantGovernmentContractCommands.Pipeline.Execute(
            state, new GrantGovernmentContractCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, settlementId));

        EndGovernmentContractCommands.Pipeline.Execute(
            state, new EndGovernmentContractCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, businessId, FailedToDeliver: true));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        Assert.Multiple(() =>
        {
            Assert.That(state.NotableBusinessGovernmentContracts.TryGet(businessId, out _), Is.False);
            Assert.That(business.Reputation, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation - NotableBusinessesCatalog.ContractFailureReputationLoss));
        });
    }

    // ---- §8 Business Lifecycle: Merge, Specialize, Move, Lobby --------------------------------------

    [Test]
    public void MergeNotableBusinessesCommandAveragesReputationAndDemotesTheAbsorbedBusiness()
    {
        var (state, householdId, _, _) = OneHousehold();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);
        var survivor = PromotedBusiness(state, "Bakery of Marcus Livius", owner);
        var absorbed = PromotedBusiness(state, "Bakery of Gaius", owner);
        AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, survivor, 20, BusinessReputationChangeReason.QualityOutput));

        var result = MergeNotableBusinessesCommands.Pipeline.Execute(
            state, new MergeNotableBusinessesCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, survivor, absorbed));

        NotableBusinessResolver.TryGetCurrent(state, survivor, out var survivingBusiness);
        NotableBusinessResolver.TryGetCurrent(state, absorbed, out var absorbedBusiness);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(survivingBusiness.Reputation, Is.EqualTo((NotableBusinessesCatalog.DefaultReputation + 20 + NotableBusinessesCatalog.DefaultReputation) / 2));
            Assert.That(absorbedBusiness.Status, Is.EqualTo(NotableBusinessStatus.Demoted));
        });
    }

    [Test]
    public void SpecializeNotableBusinessCommandNarrowsOutputAndGrantsAReputationBonus()
    {
        var (state, householdId, _, _) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        var breadId = new DefinitionId<Good>("bread");

        var result = SpecializeNotableBusinessCommands.Pipeline.Execute(
            state, new SpecializeNotableBusinessCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, breadId));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(business.IsSpecialized, Is.True);
            Assert.That(business.SpecializedGoodId, Is.EqualTo(breadId));
            Assert.That(business.OutputGoodId, Is.EqualTo(breadId));
            Assert.That(business.Reputation, Is.EqualTo(NotableBusinessesCatalog.DefaultReputation + NotableBusinessesCatalog.SpecializeReputationBonus));
        });
    }

    [Test]
    public void MoveNotableBusinessCommandUpdatesDistrictAndPostsTheRelocationCost()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District"));
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(1000)));

        var result = MoveNotableBusinessCommands.Pipeline.Execute(
            state, new MoveNotableBusinessCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, districtId));

        NotableBusinessResolver.TryGetCurrent(state, businessId, out var business);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(business.DistrictId, Is.EqualTo(districtId));
            Assert.That(account.Balance, Is.EqualTo(Money.FromDenarii(1000) - NotableBusinessesCatalog.MoveRelocationCost));
        });
    }

    [Test]
    public void LobbyGovernmentCommandWithDirectPaymentGrantsAGovernmentContract()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var businessId = PromotedBusiness(state, "Bakery of Marcus Livius", PropertyOwnerRef.ForPlayerHousehold(householdId));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(1000)));

        var result = LobbyGovernmentCommands.Pipeline.Execute(
            state, new LobbyGovernmentCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, businessId, settlementId, SpendInfluence: false));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.NotableBusinessGovernmentContracts.TryGet(businessId, out _), Is.True);
        });
    }

    // ---- Save/load round trip and deterministic hash stability -------------------------------------

    [Test]
    public void NotableBusinessesStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, _, settlementId) = OneHousehold();
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);
        var districtId = state.DistrictIds.Issue();
        state.Districts.Add(districtId, District.Create(districtId, settlementId, "Forum District"));
        var supplierHouseholdId = state.HouseholdIds.Issue();

        var bakeryA = PromotedBusiness(state, "Bakery of Marcus Livius", owner);
        var bakeryB = PromotedBusiness(state, "Bakery of Gaius", owner);
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryA, bakeryB));
        SetMainCompetitorCommands.Pipeline.Execute(
            state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, bakeryB, bakeryA));
        RecordBusinessRivalryActionCommands.Pipeline.Execute(
            state, new RecordBusinessRivalryActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, bakeryB, bakeryA, BusinessRivalryActionType.PriceUndercut));
        SetMainSupplierCommands.Pipeline.Execute(
            state, new SetMainSupplierCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, bakeryA, NotableBusinessSupplierRef.ForHousehold(supplierHouseholdId)));
        MoveNotableBusinessCommands.Pipeline.Execute(
            state, new MoveNotableBusinessCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, bakeryA, districtId));
        GrantGovernmentContractCommands.Pipeline.Execute(
            state, new GrantGovernmentContractCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, bakeryA, settlementId));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.NotableBusinesses.Count, Is.EqualTo(2));
            Assert.That(restored.NotableBusinessRivalryLogs.Count, Is.EqualTo(1));
            Assert.That(restored.NotableBusinessGovernmentContracts.Count, Is.EqualTo(1));

            NotableBusinessResolver.TryGetCurrent(restored, bakeryA, out var restoredBakeryA);
            Assert.That(restoredBakeryA.DistrictId, Is.EqualTo(districtId));
            Assert.That(restoredBakeryA.MainSupplier, Is.EqualTo(NotableBusinessSupplierRef.ForHousehold(supplierHouseholdId)));
            Assert.That(restoredBakeryA.MainCompetitorBusinessId, Is.EqualTo(bakeryB));
            Assert.That(
                restoredBakeryA.Reputation,
                Is.EqualTo(NotableBusinessesCatalog.DefaultReputation + NotableBusinessesCatalog.RivalryActionReputationEffectFor(BusinessRivalryActionType.PriceUndercut)));
        });
    }
}
