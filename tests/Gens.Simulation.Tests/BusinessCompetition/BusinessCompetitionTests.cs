using Gens.Simulation.Actors;
using Gens.Simulation.BusinessCompetition;
using Gens.Simulation.Characters;
using Gens.Simulation.Collegia;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.BusinessCompetition;

/// <summary>Phase 15 item 5 coverage — §2's full four-rung escalation ladder (including the real
/// Insolvency-driven auto-detection of Forced Consolidation and §7's spoils-of-victory resolution), §3's
/// Breaking Ranks collegium Dignitas/Opinion penalty, §4's cartel formation/defection/discovery, §5's
/// grain-hoarding severity path (mob violence and Crime &amp; Punishment exposure), §6's market saturation
/// reading, and a save/load round trip (<c>gens-business-competition-design.md</c>).</summary>
public sealed class BusinessCompetitionTests
{
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

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead(
        WorldState state, string nomen, int ambition = 30, bool greedy = false)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        var traits = greedy ? new[] { BusinessCompetitionCatalog.GreedyTraitId } : Array.Empty<DefinitionId<Trait>>();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(
            headId, nomen: nomen, household: householdId, condition: new Condition(80, 20, 60, ambition, 50), traits: traits));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
    }

    private static RuntimeId<NotableBusiness> PromotedBusiness(
        WorldState state, string name, PropertyOwnerRef owner, RuntimeId<District>? districtId = null, DefinitionId<Good>? outputGoodId = null)
    {
        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, name, owner,
                NotableBusinessTrigger.DirectPlayerTransaction, outputGoodId, LinkedPropertyRecordId: null, districtId));
        Assert.That(result.Accepted, Is.True, $"Promotion of '{name}' was rejected: {result.Error}");
        return ((NotableBusinessPromotedEvent)result.Events[0]).BusinessId;
    }

    private static void MakeMainCompetitors(WorldState state, RuntimeId<NotableBusiness> a, RuntimeId<NotableBusiness> b)
    {
        SetMainCompetitorCommands.Pipeline.Execute(state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, a, b));
        SetMainCompetitorCommands.Pipeline.Execute(state, new SetMainCompetitorCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, b, a));
    }

    private static void Fund(WorldState state, LedgerAccountKey account, Money amount)
    {
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(account, amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });
    }

    // ---- §2 The Competitive Escalation Ladder --------------------------------------------------

    [Test]
    public void EscalateCompetitiveRungCommandStepsFromOrdinaryRivalryToPriceWarThenPredatoryPricing()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);

        var first = EscalateCompetitiveRungCommands.Pipeline.Execute(
            state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));
        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var afterFirst);

        var second = EscalateCompetitiveRungCommands.Pipeline.Execute(
            state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, a, b));
        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var afterSecond);

        var third = EscalateCompetitiveRungCommands.Pipeline.Execute(
            state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(5), null, a, b));

        Assert.Multiple(() =>
        {
            Assert.That(first.Accepted, Is.True);
            Assert.That(afterFirst.CurrentRung, Is.EqualTo(CompetitiveEscalationRung.PriceWar));
            Assert.That(second.Accepted, Is.True);
            Assert.That(afterSecond.CurrentRung, Is.EqualTo(CompetitiveEscalationRung.PredatoryPricing));
            Assert.That(third.Accepted, Is.False);
            Assert.That(third.Error, Is.EqualTo(EscalateCompetitiveRungCommands.AlreadyAtCeiling));
        });
    }

    [Test]
    public void EscalateCompetitiveRungCommandRejectsBusinessesThatAreNotMutualMainCompetitors()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId);

        var result = EscalateCompetitiveRungCommands.Pipeline.Execute(
            state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(EscalateCompetitiveRungCommands.NotMainCompetitors));
    }

    [Test]
    public void DeescalateCompetitiveRungCommandStepsBackDownAndRemovesRecordAtOrdinaryRivalry()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, a, b));

        var stepDown = DeescalateCompetitiveRungCommands.Pipeline.Execute(
            state, new DeescalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(5), null, a));
        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var afterOneStep);

        var stepDownAgain = DeescalateCompetitiveRungCommands.Pipeline.Execute(
            state, new DeescalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(6), null, a));

        Assert.Multiple(() =>
        {
            Assert.That(stepDown.Accepted, Is.True);
            Assert.That(afterOneStep.CurrentRung, Is.EqualTo(CompetitiveEscalationRung.PriceWar));
            Assert.That(stepDownAgain.Accepted, Is.True);
            Assert.That(CompetitiveEscalationResolver.TryGetCurrent(state, a, out _), Is.False);
        });
    }

    [Test]
    public void CompetitiveEscalationSystemNudgesTheAggressorsOwnMarketPriceDownwardDuringAPriceWar()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        var key = new MarketGoodKey(settlementId, breadId);
        state.MarketPrices.Add(key, new SettlementMarket(settlementId, breadId, Money.FromDenarii(100), Money.FromDenarii(100), 50, 50, 50, 0));
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));

        CompetitiveEscalationSystem.Tick(state, new GameDate(4));

        state.MarketPrices.TryGet(key, out var market);
        Assert.That(market!.Price, Is.LessThan(Money.FromDenarii(100)));
    }

    [Test]
    public void CompetitiveEscalationSystemAutoAdvancesToForcedConsolidationOnceTheTargetIsInsolvent()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, headB) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, a, b));
        state.InsolvencyStates.Add(ownerB, new InsolvencyState(ownerB, 12, InsolvencyStage.Ruined, Array.Empty<InsolvencyConsequence>()));

        CompetitiveEscalationSystem.Tick(state, new GameDate(5));

        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var escalation);
        Assert.That(escalation.CurrentRung, Is.EqualTo(CompetitiveEscalationRung.ForcedConsolidation));
        _ = headB;
    }

    [Test]
    public void ResolveForcedConsolidationCommandGrantsWinnerReputationAndDemotesTheLoser()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, a, b));
        state.InsolvencyStates.Add(ownerB, new InsolvencyState(ownerB, 12, InsolvencyStage.Ruined, Array.Empty<InsolvencyConsequence>()));
        CompetitiveEscalationSystem.Tick(state, new GameDate(5));
        NotableBusinessResolver.TryGetCurrent(state, a, out var winnerBefore);
        _ = settlementId;

        var result = ResolveForcedConsolidationCommands.Pipeline.Execute(
            state, new ResolveForcedConsolidationCommand(state.CommandIds.Issue(), "player", new GameDate(6), null, a));

        NotableBusinessResolver.TryGetCurrent(state, a, out var winnerAfter);
        NotableBusinessResolver.TryGetCurrent(state, b, out var loserAfter);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(winnerAfter.Reputation, Is.EqualTo(winnerBefore.Reputation + ResolveForcedConsolidationCommands.WinnerReputationGain));
            Assert.That(loserAfter.Status, Is.EqualTo(NotableBusinessStatus.Demoted));
            Assert.That(CompetitiveEscalationResolver.TryGetCurrent(state, a, out _), Is.False);
        });
    }

    [Test]
    public void ResolveForcedConsolidationCommandTransfersTheLosersLinkedPropertyRecordToTheWinner()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var propertyRecordId = state.PropertyRecordIds.Issue();
        state.PropertyRecords.Add(
            propertyRecordId,
            PropertyRecord.Create(
                propertyRecordId, PropertyAssetType.NamedHolding, "Gaius' Bakery Holding",
                PropertyOwnerRef.ForPlayerHousehold(ownerB), Money.FromDenarii(500), settlementId, districtId));
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var result = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, "Bakery of Gaius",
                PropertyOwnerRef.ForPlayerHousehold(ownerB), NotableBusinessTrigger.DirectPlayerTransaction,
                OutputGoodId: breadId, LinkedPropertyRecordId: propertyRecordId, DistrictId: districtId));
        var b = ((NotableBusinessPromotedEvent)result.Events[0]).BusinessId;
        MakeMainCompetitors(state, a, b);
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));
        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, a, b));
        state.InsolvencyStates.Add(ownerB, new InsolvencyState(ownerB, 12, InsolvencyStage.Ruined, Array.Empty<InsolvencyConsequence>()));
        CompetitiveEscalationSystem.Tick(state, new GameDate(5));

        var resolveResult = ResolveForcedConsolidationCommands.Pipeline.Execute(
            state, new ResolveForcedConsolidationCommand(state.CommandIds.Issue(), "player", new GameDate(6), null, a));

        state.PropertyRecords.TryGet(propertyRecordId, out var transferredProperty);
        Assert.Multiple(() =>
        {
            Assert.That(resolveResult.Accepted, Is.True);
            Assert.That(transferredProperty!.Owner, Is.EqualTo(PropertyOwnerRef.ForPlayerHousehold(ownerA)));
        });
    }

    // ---- §3 Breaking Ranks ----------------------------------------------------------------------

    [Test]
    public void EscalatingAgainstAFellowCollegiumMemberAppliesTheBreakingRanksDignitasAndOpinionPenalty()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, headA) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, headB) = HouseholdWithHead(state, "Target");
        var collegiumResult = FoundCollegiumCommands.Pipeline.Execute(
            state, new FoundCollegiumCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Pistorum", settlementId,
                CollegiumType.Opificum, LinkedPopGroupType: PopGroupType.Opifices));
        var collegiumId = ((CollegiumFoundedEvent)collegiumResult.Events[0]).CollegiumId;
        CollegiumMembershipCommands.JoinPipeline.Execute(state, new JoinCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, collegiumId, ownerA));
        CollegiumMembershipCommands.JoinPipeline.Execute(state, new JoinCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, collegiumId, ownerB));

        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        var dignitasBefore = DignitasResolver.Current(state, ownerA);

        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));

        var dignitasAfter = DignitasResolver.Current(state, ownerA);
        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var escalation);
        state.Relationships.TryGet(new RelationshipKey(headA, headB), out var relationship);

        Assert.Multiple(() =>
        {
            Assert.That(escalation.IsWithinSameCollegium, Is.True);
            Assert.That(escalation.CollegiumDignitasImpact, Is.EqualTo(BusinessCompetitionCatalog.BreakingRanksDignitasPenalty));
            Assert.That(dignitasAfter, Is.EqualTo(dignitasBefore - BusinessCompetitionCatalog.BreakingRanksDignitasPenalty));
            Assert.That(relationship.Bonds.HasFlag(BondTag.Rival), Is.True);
        });
    }

    [Test]
    public void EscalatingAgainstAnOutsideRivalAppliesNoBreakingRanksPenalty()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Aggressor");
        var (ownerB, _) = HouseholdWithHead(state, "Target");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of Marcus", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of Gaius", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        var dignitasBefore = DignitasResolver.Current(state, ownerA);

        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));

        var dignitasAfter = DignitasResolver.Current(state, ownerA);
        CompetitiveEscalationResolver.TryGetCurrent(state, a, out var escalation);
        Assert.Multiple(() =>
        {
            Assert.That(escalation.IsWithinSameCollegium, Is.False);
            Assert.That(escalation.CollegiumDignitasImpact, Is.EqualTo(0));
            Assert.That(dignitasAfter, Is.EqualTo(dignitasBefore));
        });
    }

    // ---- §4 Cartels and Market-Sharing Agreements ------------------------------------------------

    [Test]
    public void FormCartelCommandRejectsFewerThanTwoParticipantsAndDuplicates()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "A");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId);

        var tooFew = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a }, CartelAgreementType.PriceFixing));
        var duplicate = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, a }, CartelAgreementType.PriceFixing));

        Assert.Multiple(() =>
        {
            Assert.That(tooFew.Accepted, Is.False);
            Assert.That(tooFew.Error, Is.EqualTo(FormCartelCommands.TooFewParticipants));
            Assert.That(duplicate.Accepted, Is.False);
            Assert.That(duplicate.Error, Is.EqualTo(FormCartelCommands.DuplicateParticipant));
        });
    }

    [Test]
    public void FormCartelCommandCreatesAnUndiscoveredCartelAcrossItsParticipants()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "A");
        var (ownerB, _) = HouseholdWithHead(state, "B");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of B", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);

        var result = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, b }, CartelAgreementType.PriceFixing));
        var cartelId = ((CartelFormedEvent)result.Events[0]).CartelId;
        CartelAgreementResolver.TryGetCurrent(state, cartelId, out var cartel);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(cartel.IsDiscovered, Is.False);
            Assert.That(cartel.ParticipantBusinessIds, Is.EquivalentTo(new[] { a, b }));
        });
    }

    [Test]
    public void CartelDefectionRiskSystemDetectsAGreedyParticipantAndFiresAPriceUndercutAgainstTheOtherParticipant()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Greedy", greedy: true);
        var (ownerB, _) = HouseholdWithHead(state, "Honest");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of B", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        MakeMainCompetitors(state, a, b);
        var formResult = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, b }, CartelAgreementType.PriceFixing));
        var cartelId = ((CartelFormedEvent)formResult.Events[0]).CartelId;
        NotableBusinessResolver.TryGetCurrent(state, b, out var targetBefore);

        CartelDefectionRiskSystem.Tick(state, new GameDate(3));

        CartelAgreementResolver.TryGetCurrent(state, cartelId, out var cartelAfter);
        NotableBusinessResolver.TryGetCurrent(state, b, out var targetAfter);
        Assert.Multiple(() =>
        {
            Assert.That(cartelAfter.BreakingParticipantId, Is.EqualTo(a));
            Assert.That(targetAfter.Reputation, Is.LessThan(targetBefore.Reputation));
        });
    }

    [Test]
    public void CartelDefectionRiskSystemLeavesAnUntemptedCartelUnbroken()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "Honest1", ambition: 10);
        var (ownerB, _) = HouseholdWithHead(state, "Honest2", ambition: 10);
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of B", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        var formResult = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, b }, CartelAgreementType.PriceFixing));
        var cartelId = ((CartelFormedEvent)formResult.Events[0]).CartelId;

        CartelDefectionRiskSystem.Tick(state, new GameDate(3));

        CartelAgreementResolver.TryGetCurrent(state, cartelId, out var cartelAfter);
        Assert.That(cartelAfter.BreakingParticipantId, Is.Null);
    }

    [Test]
    public void DiscoverCartelCommandPenalizesEveryParticipantsReputationAndRecordsAPersonalScandal()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, headA) = HouseholdWithHead(state, "A");
        var (ownerB, _) = HouseholdWithHead(state, "B");
        var breadId = new DefinitionId<Good>("bread");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of B", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        var formResult = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, b }, CartelAgreementType.PriceFixing));
        var cartelId = ((CartelFormedEvent)formResult.Events[0]).CartelId;
        NotableBusinessResolver.TryGetCurrent(state, a, out var beforeA);
        var dignitasBefore = DignitasResolver.Current(state, ownerA);
        _ = headA;

        var result = DiscoverCartelCommands.Pipeline.Execute(
            state, new DiscoverCartelCommand(state.CommandIds.Issue(), "player", new GameDate(4), null, cartelId));

        NotableBusinessResolver.TryGetCurrent(state, a, out var afterA);
        var dignitasAfter = DignitasResolver.Current(state, ownerA);
        var scandalFound = false;
        foreach (var entry in state.ScandalRecords.InAscendingOrder())
        {
            if (entry.Value.PrimaryHouseholdId == ownerA && entry.Value.SourceType == ScandalSourceType.CartelDiscovery)
                scandalFound = true;
        }

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(afterA.Reputation, Is.EqualTo(beforeA.Reputation - BusinessCompetitionCatalog.CartelDiscoveryReputationLoss));
            Assert.That(dignitasAfter, Is.LessThan(dignitasBefore));
            Assert.That(scandalFound, Is.True);
        });
    }

    // ---- §5 Grain Hoarding ----------------------------------------------------------------------

    [Test]
    public void DeclareGrainHoardingCommandRejectsANonGrainTradingBusiness()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "A");
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, new DefinitionId<Good>("bread"));

        var result = GrainHoardingDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, a));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(GrainHoardingDeclarationCommands.NotGrainTrading));
    }

    [Test]
    public void GrainHoardingResolutionSystemTriggersMobViolenceAndAPunishableOffenseDuringARealShortage()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, headA) = HouseholdWithHead(state, "GrainTrader");
        var grainId = NeedsConsumptionCalculator.ConsumptionGood;
        var a = PromotedBusiness(state, "Grain House of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, grainId);
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, grainId), new SettlementMarket(settlementId, grainId, Money.FromDenarii(50), Money.FromDenarii(50), 10, 20, 10, 10));
        Fund(state, LedgerAccountKey.ForHousehold(ownerA), Money.FromDenarii(1000));
        GrainHoardingDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, a));
        var offensesBefore = 0;
        foreach (var entry in state.PunishableOffenses.InAscendingOrder())
            if (entry.Value.CharacterId == headA)
                offensesBefore++;

        GrainHoardingResolutionSystem.Tick(state, new GameDate(3));

        GrainHoardingResolver.TryGetCurrent(state, a, out var record);
        var offensesAfter = 0;
        foreach (var entry in state.PunishableOffenses.InAscendingOrder())
            if (entry.Value.CharacterId == headA && entry.Value.Source == PunishableOffenseSource.GrainHoarding)
                offensesAfter++;

        Assert.Multiple(() =>
        {
            Assert.That(record.MobViolenceTriggered, Is.True);
            Assert.That(record.DuringActiveShortage, Is.True);
            Assert.That(record.PunishableOffenseGenerated, Is.True);
            Assert.That(offensesAfter, Is.EqualTo(offensesBefore + 1));
        });
    }

    [Test]
    public void GrainHoardingResolutionSystemDoesNothingWithoutARealShortage()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "GrainTrader");
        var grainId = NeedsConsumptionCalculator.ConsumptionGood;
        var a = PromotedBusiness(state, "Grain House of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, grainId);
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, grainId), new SettlementMarket(settlementId, grainId, Money.FromDenarii(50), Money.FromDenarii(50), 20, 10, 10, 0));
        GrainHoardingDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, a));

        GrainHoardingResolutionSystem.Tick(state, new GameDate(3));

        GrainHoardingResolver.TryGetCurrent(state, a, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(record.MobViolenceTriggered, Is.False);
            Assert.That(record.DuringActiveShortage, Is.False);
        });
    }

    [Test]
    public void EndGrainHoardingCommandClearsTheActiveFlag()
    {
        var (state, _, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "GrainTrader");
        var grainId = NeedsConsumptionCalculator.ConsumptionGood;
        var a = PromotedBusiness(state, "Grain House of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, grainId);
        GrainHoardingDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, a));

        var result = GrainHoardingDeclarationCommands.EndPipeline.Execute(
            state, new EndGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a));

        GrainHoardingResolver.TryGetCurrent(state, a, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(record.IsActivelyHoarding, Is.False);
        });
    }

    // ---- §6 Market Entry and Saturation -----------------------------------------------------------

    [Test]
    public void MarketSaturationSystemReadsUndersaturatedForOneBusinessAndSaturatedForFourWithHealthyEmployment()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var breadId = new DefinitionId<Good>("bread");
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 100, employmentRatio: Fixed64.One));
        var (owner1, _) = HouseholdWithHead(state, "One");
        PromotedBusiness(state, "Bakery 1", PropertyOwnerRef.ForPlayerHousehold(owner1), districtId, breadId);

        MarketSaturationSystem.Tick(state);
        MarketCapacityResolver.TryGetCurrent(state, settlementId, breadId, out var oneBusinessReading);

        for (var i = 2; i <= 4; i++)
        {
            var (ownerN, _) = HouseholdWithHead(state, $"Extra{i}");
            PromotedBusiness(state, $"Bakery {i}", PropertyOwnerRef.ForPlayerHousehold(ownerN), districtId, breadId);
        }

        MarketSaturationSystem.Tick(state);
        MarketCapacityResolver.TryGetCurrent(state, settlementId, breadId, out var fourBusinessReading);

        Assert.Multiple(() =>
        {
            Assert.That(oneBusinessReading.SaturationLevel, Is.EqualTo(MarketSaturationLevel.Undersaturated));
            Assert.That(oneBusinessReading.CurrentBusinessCount, Is.EqualTo(1));
            Assert.That(fourBusinessReading.SaturationLevel, Is.EqualTo(MarketSaturationLevel.Saturated));
            Assert.That(fourBusinessReading.CurrentBusinessCount, Is.EqualTo(4));
        });
    }

    [Test]
    public void MarketSaturationSystemReadsSaturatedWheneverEmploymentRatioHasFallenBelowOneRegardlessOfCount()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var breadId = new DefinitionId<Good>("bread");
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Opifices),
            PopGroup.Create(settlementId, PopGroupType.Opifices, 100, employmentRatio: Fixed64.FromRaw(500_000)));
        var (owner1, _) = HouseholdWithHead(state, "One");
        PromotedBusiness(state, "Bakery 1", PropertyOwnerRef.ForPlayerHousehold(owner1), districtId, breadId);

        MarketSaturationSystem.Tick(state);

        MarketCapacityResolver.TryGetCurrent(state, settlementId, breadId, out var reading);
        Assert.That(reading.SaturationLevel, Is.EqualTo(MarketSaturationLevel.Saturated));
    }

    // ---- Save/load round trip and deterministic hash stability -------------------------------------

    [Test]
    public void BusinessCompetitionStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, districtId) = OneSettlementWithDistrict();
        var (ownerA, _) = HouseholdWithHead(state, "A");
        var (ownerB, _) = HouseholdWithHead(state, "B", greedy: true);
        var breadId = new DefinitionId<Good>("bread");
        var grainId = NeedsConsumptionCalculator.ConsumptionGood;
        var a = PromotedBusiness(state, "Bakery of A", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, breadId);
        var b = PromotedBusiness(state, "Bakery of B", PropertyOwnerRef.ForPlayerHousehold(ownerB), districtId, breadId);
        var grainBusiness = PromotedBusiness(state, "Grain House", PropertyOwnerRef.ForPlayerHousehold(ownerA), districtId, grainId);
        MakeMainCompetitors(state, a, b);

        EscalateCompetitiveRungCommands.Pipeline.Execute(state, new EscalateCompetitiveRungCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, a, b));

        var cartelResult = FormCartelCommands.Pipeline.Execute(
            state, new FormCartelCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, new[] { a, b }, CartelAgreementType.MarketSharingByDistrict));
        var cartelId = ((CartelFormedEvent)cartelResult.Events[0]).CartelId;
        CartelDefectionRiskSystem.Tick(state, new GameDate(4));

        GrainHoardingDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareGrainHoardingCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, grainBusiness));
        state.MarketPrices.Add(
            new MarketGoodKey(settlementId, grainId), new SettlementMarket(settlementId, grainId, Money.FromDenarii(50), Money.FromDenarii(50), 10, 20, 10, 10));
        Fund(state, LedgerAccountKey.ForHousehold(ownerA), Money.FromDenarii(1000));
        GrainHoardingResolutionSystem.Tick(state, new GameDate(5));

        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Opifices), PopGroup.Create(settlementId, PopGroupType.Opifices, 100, employmentRatio: Fixed64.One));
        MarketSaturationSystem.Tick(state);

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.CompetitiveEscalations.Count, Is.EqualTo(1));
            Assert.That(restored.CartelAgreements.Count, Is.EqualTo(1));
            Assert.That(restored.GrainHoardingRecords.Count, Is.EqualTo(1));
            Assert.That(restored.MarketCapacityReadings.Count, Is.GreaterThanOrEqualTo(1));

            CompetitiveEscalationResolver.TryGetCurrent(restored, a, out var restoredEscalation);
            Assert.That(restoredEscalation.CurrentRung, Is.EqualTo(CompetitiveEscalationRung.PriceWar));

            CartelAgreementResolver.TryGetCurrent(restored, cartelId, out var restoredCartel);
            Assert.That(restoredCartel.ParticipantBusinessIds, Is.EquivalentTo(new[] { a, b }));

            GrainHoardingResolver.TryGetCurrent(restored, grainBusiness, out var restoredGrainRecord);
            Assert.That(restoredGrainRecord.MobViolenceTriggered, Is.True);
        });
    }
}
