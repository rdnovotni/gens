using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.PublicContracts;
using Gens.Simulation.Random;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.PublicContracts;

/// <summary>Phase 15 item 6 coverage: the Censor magistracy and its eligibility gate (§2), the Lustrum
/// tick (§3), contract opening/bidding/award across §5's Price/Reliability/Influence inputs and its
/// Faction/Clientela/corruption skew, contract fraud's cutting-corners discovery race (§6.1), the
/// repetundae prosecution and its conviction consequences (§6.2), and a save/load round trip
/// (<c>gens-public-contracts-competitive-bidding-design.md</c>).</summary>
public sealed class PublicContractsTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId) OneSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId, SettlementStage.Vicus));
        return (state, settlementId);
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead(
        WorldState state, string nomen, IReadOnlyList<DefinitionId<Trait>>? traits = null)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId, traits: traits));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
    }

    private static void Fund(WorldState state, LedgerAccountKey account, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(account, amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    /// <summary>A Character who has, at some point, held Duumvir (an ended record) — §2's Censor
    /// eligibility gate.</summary>
    private static RuntimeId<Character> FormerDuumvir(WorldState state, RuntimeId<Settlement> settlementId, string nomen)
    {
        var (householdId, headId) = HouseholdWithHead(state, nomen);
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            recordId, new MagistracyRecord(recordId, headId, MagistracyOffice.Duumvir, settlementId, new GameDate(0), new GameDate(6), CoHolderId: null));
        _ = householdId;
        return headId;
    }

    private static RuntimeId<Character> ElectCensor(WorldState state, RuntimeId<Settlement> settlementId, string nomenA = "Censor A", string nomenB = "Censor B")
    {
        var a = FormerDuumvir(state, settlementId, nomenA);
        var b = FormerDuumvir(state, settlementId, nomenB);
        var result = ElectCensorsCommands.Pipeline.Execute(
            state, new ElectCensorsCommand(state.CommandIds.Issue(), "player", new GameDate(7), null, settlementId, a, b));
        Assert.That(result.Accepted, Is.True, $"Censor election was rejected: {result.Error}");
        return a;
    }

    private static RuntimeId<PublicContract> OpenContract(
        WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Character> censorId, PublicContractType type = PublicContractType.Redemptor)
    {
        var result = OpenPublicContractCommands.Pipeline.Execute(
            state, new OpenPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(8), null, type, settlementId, censorId));
        Assert.That(result.Accepted, Is.True, $"Contract open was rejected: {result.Error}");
        return ((PublicContractOpenedEvent)result.Events[0]).ContractId;
    }

    // ---- §2 The Censor -------------------------------------------------------------------------

    [Test]
    public void ElectCensorsCommandRejectsACandidateWhoNeverHeldDuumvir()
    {
        var (state, settlementId) = OneSettlement();
        var a = FormerDuumvir(state, settlementId, "Eligible");
        var (_, b) = HouseholdWithHead(state, "Ineligible");

        var result = ElectCensorsCommands.Pipeline.Execute(
            state, new ElectCensorsCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, a, b));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ElectCensorsCommands.HolderBNeverHeldDuumvir));
    }

    [Test]
    public void ElectCensorsCommandPairsBothSeatsAndWritesTheCoMagistrateBond()
    {
        var (state, settlementId) = OneSettlement();
        var a = FormerDuumvir(state, settlementId, "Censor A");
        var b = FormerDuumvir(state, settlementId, "Censor B");

        var result = ElectCensorsCommands.Pipeline.Execute(
            state, new ElectCensorsCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, a, b));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Censor, a), Is.Not.Null);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Censor, a)!.CoHolderId, Is.EqualTo(b));
            state.Relationships.TryGet(new RelationshipKey(a, b), out var relationship);
            Assert.That(relationship.Bonds.HasFlag(BondTag.CoMagistrate), Is.True);
        });
    }

    [Test]
    public void ElectCensorsCommandRejectsWhenTheCensorshipIsAlreadyFilled()
    {
        var (state, settlementId) = OneSettlement();
        ElectCensor(state, settlementId);
        var c = FormerDuumvir(state, settlementId, "Third");
        var d = FormerDuumvir(state, settlementId, "Fourth");

        var result = ElectCensorsCommands.Pipeline.Execute(
            state, new ElectCensorsCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, settlementId, c, d));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ElectCensorsCommands.CensorshipAlreadyFilled));
    }

    [Test]
    public void MagistracyTermSystemGrantsCensorTrickleButNeverAutoRenewsTheirTerm()
    {
        var (state, settlementId) = OneSettlement();
        var a = ElectCensor(state, settlementId);

        // Past MagistracyCatalog.TermLengthMonths (12) — an ordinary office would auto-renew here; the
        // Censor's own term is untouched by this system (only LustrumSystem ends it).
        new MagistracyTermSystem().Tick(state, new MonthlyTickContext(new GameDate(7 + MagistracyCatalog.TermLengthMonths + 1), new RandomStreamSet()));

        var record = MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Censor, a);
        Assert.Multiple(() =>
        {
            Assert.That(record, Is.Not.Null);
            Assert.That(record!.TermStartDate, Is.EqualTo(new GameDate(7)));
            state.Characters.TryGet(a, out var character);
            Assert.That(DignitasResolver.Current(state, character!.Household!.Value), Is.GreaterThan(0));
        });
    }

    // ---- §5 The Bidding Process -----------------------------------------------------------------

    [Test]
    public void OpenPublicContractCommandRequiresAnActiveCensor()
    {
        var (state, settlementId) = OneSettlement();
        var notACensor = FormerDuumvir(state, settlementId, "Nobody");

        var result = OpenPublicContractCommands.Pipeline.Execute(
            state, new OpenPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, PublicContractType.Redemptor, settlementId, notACensor));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(OpenPublicContractCommands.NoActiveCensorAtSettlement));
    }

    [Test]
    public void SubmitContractBidCommandAcceptsAPlayerHouseholdBidSpendingInfluenceAndOfferingABribe()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var (bidderHousehold, _) = HouseholdWithHead(state, "Bidder");
        InfluenceResolver.Apply(state, bidderHousehold, 20);
        Fund(state, LedgerAccountKey.ForHousehold(bidderHousehold), Money.FromDenarii(500));

        var result = SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(
                state.CommandIds.Issue(), "player", new GameDate(9), null, contractId,
                ContractBidderRef.ForPlayerHousehold(bidderHousehold), Money.FromDenarii(300), InfluenceSpent: 10, BribeAmount: Money.FromDenarii(50)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(InfluenceResolver.Current(state, bidderHousehold), Is.EqualTo(10));
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(bidderHousehold), out var account);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(500) - Money.FromDenarii(50)));
            Assert.That(PublicContractResolver.PendingBids(state, contractId), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void SubmitContractBidCommandRejectsInfluenceOrBribeFromABidderWithNoResolvableHousehold()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var rivalActorId = state.ActorIds.Issue();
        state.Actors.Add(rivalActorId, LivingWorldActor.Create(
            rivalActorId, LivingWorldActorType.Gens, "Gens Rivalis", LivingWorldActorTier.Noteworthy,
            LivingWorldActorStandingTrend.Established, LivingWorldActorOrigin.Ancient, parentActorId: null,
            LivingWorldActorIdentity.None, dignitas: 10, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), state.RegionIds.Issue(), settlementId));

        var result = SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(
                state.CommandIds.Issue(), "player", new GameDate(9), null, contractId,
                ContractBidderRef.ForRivalHouse(rivalActorId), Money.FromDenarii(300), InfluenceSpent: 5));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SubmitContractBidCommands.InfluenceRequiresHousehold));
    }

    [Test]
    public void SubmitContractBidCommandAcceptsANotableBusinessAndASocietasBidder()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);

        var (businessOwnerId, _) = HouseholdWithHead(state, "Business Owner");
        var promoted = PromoteNotableBusinessCommands.Pipeline.Execute(
            state, new PromoteNotableBusinessCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, "Redemptores of Marcus",
                PropertyOwnerRef.ForPlayerHousehold(businessOwnerId), NotableBusinessTrigger.DirectPlayerTransaction, null, null, null));
        var businessId = ((NotableBusinessPromotedEvent)promoted.Events[0]).BusinessId;

        var (partnerId, _) = HouseholdWithHead(state, "Partner");
        var societasId = state.SocietasIds.Issue();
        state.Societates.Add(
            societasId,
            Societas.Create(
                societasId, PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "Pooling for a Redemptor bid",
                new[] { new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(partnerId), Fixed64.One) },
                designatedPartner: null, linkedPropertySubject: null));

        var businessBid = SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForNotableBusiness(businessId), Money.FromDenarii(400)));
        var societasBid = SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForSocietas(societasId), Money.FromDenarii(350)));

        Assert.Multiple(() =>
        {
            Assert.That(businessBid.Accepted, Is.True);
            Assert.That(societasBid.Accepted, Is.True);
            Assert.That(PublicContractResolver.PendingBids(state, contractId), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void SubmitContractBidCommandRejectsADisqualifiedBidder()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var (holderId, _) = HouseholdWithHead(state, "Disqualified");

        var recordId = state.ContractFraudRecordIds.Issue();
        state.PublicContractFraudRecords.Add(
            recordId,
            new ContractFraudRecord(
                recordId, contractId, ContractBidderRef.ForPlayerHousehold(holderId), new GameDate(1), null, null,
                DisqualifiedFromBidding: true, DisqualifiedUntilDate: new GameDate(100)));

        var result = SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(holderId), Money.FromDenarii(300)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SubmitContractBidCommands.BidderDisqualified));
    }

    [Test]
    public void AwardPublicContractCommandAwardsTheHighestTotalScoreAndMarksOthersLost()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);

        var (cheapId, _) = HouseholdWithHead(state, "Cheap Bidder");
        var (reliableId, reliableHeadId) = HouseholdWithHead(state, "Reliable Bidder");
        AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, reliableId, 500, "seed"));

        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(cheapId), Money.FromDenarii(50)));
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(reliableId), Money.FromDenarii(50)));

        var result = AwardPublicContractCommands.Pipeline.Execute(
            state, new AwardPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, contractId, censor));

        PublicContractResolver.TryGetCurrent(state, contractId, out var contract);
        _ = reliableHeadId;
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(contract.Status, Is.EqualTo(PublicContractStatus.Awarded));
            Assert.That(contract.CurrentHolder, Is.EqualTo(ContractBidderRef.ForPlayerHousehold(reliableId)));
        });
    }

    [Test]
    public void AwardPublicContractCommandLetsACorruptCensorFavorTheLargestBriber()
    {
        var (state, settlementId) = OneSettlement();
        var (censorHouseholdA, censorA) = HouseholdWithHead(state, "Censor A", traits: new[] { PublicContractsCatalog.CorruptCensorTraitId });
        var (censorHouseholdB, censorB) = HouseholdWithHead(state, "Censor B");
        _ = censorHouseholdA;
        _ = censorHouseholdB;
        var recordAId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordAId, new MagistracyRecord(recordAId, censorA, MagistracyOffice.Censor, settlementId, new GameDate(0), CoHolderId: censorB));
        var recordBId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordBId, new MagistracyRecord(recordBId, censorB, MagistracyOffice.Censor, settlementId, new GameDate(0), CoHolderId: censorA));
        var contractId = OpenContract(state, settlementId, censorA);

        var (fairId, _) = HouseholdWithHead(state, "Fair Bidder");
        AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, fairId, 20, "seed"));
        var (briberId, _) = HouseholdWithHead(state, "Briber");
        Fund(state, LedgerAccountKey.ForHousehold(briberId), Money.FromDenarii(1_000));

        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(fairId), Money.FromDenarii(50)));
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(
                state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(briberId), Money.FromDenarii(50),
                BribeAmount: Money.FromDenarii(600)));

        AwardPublicContractCommands.Pipeline.Execute(
            state, new AwardPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, contractId, censorA));

        PublicContractResolver.TryGetCurrent(state, contractId, out var contract);
        Assert.That(contract.CurrentHolder, Is.EqualTo(ContractBidderRef.ForPlayerHousehold(briberId)));
    }

    // ---- §3 The Lustrum --------------------------------------------------------------------------

    [Test]
    public void LustrumSystemFiresOnlyOnTheSixtyMonthIntervalAndReopensAwardedContractsAndEndsTheCensorTerm()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var (bidderId, _) = HouseholdWithHead(state, "Bidder");
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(bidderId), Money.FromDenarii(50)));
        AwardPublicContractCommands.Pipeline.Execute(state, new AwardPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, contractId, censor));

        var notYet = LustrumSystem.Tick(state, new GameDate(59));
        var fired = LustrumSystem.Tick(state, new GameDate(60));

        PublicContractResolver.TryGetCurrent(state, contractId, out var reopenedContract);
        Assert.Multiple(() =>
        {
            Assert.That(notYet, Is.Empty);
            Assert.That(fired.OfType<LustrumFiredEvent>().Any(), Is.True);
            Assert.That(reopenedContract.Status, Is.EqualTo(PublicContractStatus.OpenForBidding));
            Assert.That(reopenedContract.CurrentHolder, Is.Null);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Censor, censor), Is.Null);
            Assert.That(state.LustrumEvents.Count, Is.EqualTo(1));
        });
    }

    // ---- §6 Contract Fraud ------------------------------------------------------------------------

    [Test]
    public void CuttingCornersAccumulatesDiscoveryRiskAndPostsAQuietMarginGainUntilDiscovered()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var (holderId, _) = HouseholdWithHead(state, "Holder");
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(holderId), Money.FromDenarii(1_000)));
        AwardPublicContractCommands.Pipeline.Execute(state, new AwardPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, contractId, censor));
        Fund(state, LedgerAccountKey.ForSettlementTreasury(settlementId), Money.FromDenarii(1_000));

        CuttingCornersDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareCuttingCornersCommand(state.CommandIds.Issue(), "player", new GameDate(11), null, contractId));

        for (var month = 0; month < (PublicContractsCatalog.FraudDiscoveryRiskThreshold + PublicContractsCatalog.FraudDiscoveryRiskGainPerMonth - 1) / PublicContractsCatalog.FraudDiscoveryRiskGainPerMonth; month++)
            ContractFraudDiscoverySystem.Tick(state, new GameDate(12 + month));

        PublicContractResolver.TryGetCurrent(state, contractId, out var contract);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(holderId), out var holderAccount);

        Assert.Multiple(() =>
        {
            Assert.That(contract.FraudDiscovered, Is.True);
            Assert.That(holderAccount!.Balance, Is.GreaterThan(Money.Zero));
            Assert.That(state.PublicContractFraudRecords.Count, Is.EqualTo(1));
        });
    }

    // ---- §6.2 Repetundae Prosecution -----------------------------------------------------------

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Character> Censor, RuntimeId<PublicContract> ContractId, RuntimeId<ContractFraudRecord> FraudRecordId, RuntimeId<Household> HolderId)
        DiscoveredFraud()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var (holderId, _) = HouseholdWithHead(state, "Fraudulent Holder");
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(9), null, contractId, ContractBidderRef.ForPlayerHousehold(holderId), Money.FromDenarii(1_000)));
        AwardPublicContractCommands.Pipeline.Execute(state, new AwardPublicContractCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, contractId, censor));
        Fund(state, LedgerAccountKey.ForSettlementTreasury(settlementId), Money.FromDenarii(1_000));
        Fund(state, LedgerAccountKey.ForHousehold(holderId), Money.FromDenarii(1_000));

        CuttingCornersDeclarationCommands.DeclarePipeline.Execute(
            state, new DeclareCuttingCornersCommand(state.CommandIds.Issue(), "player", new GameDate(11), null, contractId));
        for (var month = 0; month < (PublicContractsCatalog.FraudDiscoveryRiskThreshold + PublicContractsCatalog.FraudDiscoveryRiskGainPerMonth - 1) / PublicContractsCatalog.FraudDiscoveryRiskGainPerMonth; month++)
            ContractFraudDiscoverySystem.Tick(state, new GameDate(12 + month));

        var recordId = state.PublicContractFraudRecords.InAscendingOrder().Single().Key;
        return (state, settlementId, censor, contractId, recordId, holderId);
    }

    [Test]
    public void FileRepetundaeCaseCommandRejectsAHolderThatDoesNotResolveToAHousehold()
    {
        var (state, settlementId) = OneSettlement();
        var censor = ElectCensor(state, settlementId);
        var contractId = OpenContract(state, settlementId, censor);
        var rivalActorId = state.ActorIds.Issue();
        state.Actors.Add(rivalActorId, LivingWorldActor.Create(
            rivalActorId, LivingWorldActorType.Gens, "Gens Rivalis", LivingWorldActorTier.Noteworthy,
            LivingWorldActorStandingTrend.Established, LivingWorldActorOrigin.Ancient, parentActorId: null,
            LivingWorldActorIdentity.None, dignitas: 10, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), state.RegionIds.Issue(), settlementId));
        var recordId = state.ContractFraudRecordIds.Issue();
        state.PublicContractFraudRecords.Add(
            recordId,
            new ContractFraudRecord(recordId, contractId, ContractBidderRef.ForRivalHouse(rivalActorId), new GameDate(1), null, null, false, null));
        var (accuserId, accuserHeadId) = HouseholdWithHead(state, "Accuser");
        Fund(state, LedgerAccountKey.ForHousehold(accuserId), Money.FromDenarii(100));

        var result = FileRepetundaeCaseCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new FileRepetundaeCaseCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, recordId, accuserId, settlementId, accuserHeadId));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(FileRepetundaeCaseCommands.HolderNotProsecutable));
    }

    [Test]
    public void ARepetundaeConvictionOrdersRestitutionDisqualifiesTheHolderAndReopensTheContract()
    {
        for (var seed = 1UL; seed <= 80UL; seed++)
        {
            var (state, settlementId, _, contractId, recordId, holderId) = DiscoveredFraud();
            var (accuserId, accuserHeadId) = HouseholdWithHead(state, "Accuser");
            Fund(state, LedgerAccountKey.ForHousehold(accuserId), Money.FromDenarii(1_000));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, accuserId, 500, "seed"));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, holderId, -500, "seed"));

            var streams = new RandomStreamSet();
            streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, seed);
            streams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, seed);

            var filed = FileRepetundaeCaseCommands.CreatePipeline(streams).Execute(
                state, new FileRepetundaeCaseCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, recordId, accuserId, settlementId, accuserHeadId));
            Assert.That(filed.Accepted, Is.True, $"Filing was rejected: {filed.Error}");
            var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(holderId), out var accountBeforeVerdict);
            var balanceBeforeVerdict = accountBeforeVerdict!.Balance;

            var system = new LegalCaseAdvancementSystem();
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths + 1), streams));

            state.LegalCases.TryGet(caseId, out var legalCase);
            if (legalCase!.Verdict != LegalCaseVerdict.Convicted)
                continue;

            state.PublicContractFraudRecords.TryGet(recordId, out var record);
            PublicContractResolver.TryGetCurrent(state, contractId, out var contract);
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(holderId), out var holderAccount);

            Assert.Multiple(() =>
            {
                Assert.That(record!.DisqualifiedFromBidding, Is.True);
                Assert.That(record.LegalOutcome, Is.EqualTo(LegalCaseVerdict.Convicted));
                Assert.That(ContractBidderResolver.IsDisqualified(state, ContractBidderRef.ForPlayerHousehold(holderId), new GameDate(21)), Is.True);
                Assert.That(contract.Status, Is.EqualTo(PublicContractStatus.OpenForBidding));
                Assert.That(holderAccount!.Balance, Is.EqualTo(balanceBeforeVerdict - Money.FromDenarii(500)));
            });
            return;
        }

        Assert.Fail("No seed in the searched range produced a Convicted repetundae verdict.");
    }

    [Test]
    public void AnAcquittedRepetundaeVerdictLeavesTheHolderEligibleToBidAgain()
    {
        for (var seed = 1UL; seed <= 80UL; seed++)
        {
            var (state, settlementId, _, _, recordId, holderId) = DiscoveredFraud();
            var (accuserId, accuserHeadId) = HouseholdWithHead(state, "Accuser");
            Fund(state, LedgerAccountKey.ForHousehold(accuserId), Money.FromDenarii(1_000));
            AdjustDignitasCommands.Pipeline.Execute(state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, holderId, 500, "seed"));

            var streams = new RandomStreamSet();
            streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, seed);
            streams.AddDerived(LegalCaseAdvancementSystem.VerdictOutcomeStreamName, seed);

            var filed = FileRepetundaeCaseCommands.CreatePipeline(streams).Execute(
                state, new FileRepetundaeCaseCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, recordId, accuserId, settlementId, accuserHeadId));
            var caseId = filed.Events.OfType<LawsuitFiledEvent>().Single().CaseId;

            var system = new LegalCaseAdvancementSystem();
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths), streams));
            system.Tick(state, new MonthlyTickContext(new GameDate(20 + LegalCatalog.MajorCaseEvidenceGatheringMonths + 1), streams));

            state.LegalCases.TryGet(caseId, out var legalCase);
            if (legalCase!.Verdict != LegalCaseVerdict.Acquitted)
                continue;

            state.PublicContractFraudRecords.TryGet(recordId, out var record);
            Assert.Multiple(() =>
            {
                Assert.That(record!.DisqualifiedFromBidding, Is.False);
                Assert.That(record.LegalOutcome, Is.EqualTo(LegalCaseVerdict.Acquitted));
                Assert.That(ContractBidderResolver.IsDisqualified(state, ContractBidderRef.ForPlayerHousehold(holderId), new GameDate(21)), Is.False);
            });
            return;
        }

        Assert.Fail("No seed in the searched range produced an Acquitted repetundae verdict.");
    }

    // ---- Save/load round trip and deterministic hash stability -------------------------------------

    [Test]
    public void PublicContractsStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, censor, contractId, recordId, holderId) = DiscoveredFraud();
        var (accuserId, accuserHeadId) = HouseholdWithHead(state, "Accuser");
        Fund(state, LedgerAccountKey.ForHousehold(accuserId), Money.FromDenarii(1_000));

        var streams = new RandomStreamSet();
        streams.AddDerived(FileLawsuitCommands.QuickResolutionStreamName, 1UL);
        FileRepetundaeCaseCommands.CreatePipeline(streams).Execute(
            state, new FileRepetundaeCaseCommand(state.CommandIds.Issue(), "player", new GameDate(20), null, recordId, accuserId, settlementId, accuserHeadId));

        var secondContractId = OpenContract(state, settlementId, censor, PublicContractType.Publicani);
        SubmitContractBidCommands.Pipeline.Execute(
            state, new SubmitContractBidCommand(state.CommandIds.Issue(), "player", new GameDate(21), null, secondContractId, ContractBidderRef.ForPlayerHousehold(holderId), Money.FromDenarii(80)));

        LustrumSystem.Tick(state, new GameDate(60));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.PublicContracts.Count, Is.EqualTo(2));
            Assert.That(restored.ContractBids.Count, Is.EqualTo(2));
            Assert.That(restored.LustrumEvents.Count, Is.EqualTo(1));
            Assert.That(restored.PublicContractFraudRecords.Count, Is.EqualTo(1));
            Assert.That(restored.ContractFraudLegalLinks.Count, Is.EqualTo(1));

            PublicContractResolver.TryGetCurrent(restored, contractId, out var restoredContract);
            Assert.That(restoredContract.Type, Is.EqualTo(PublicContractType.Redemptor));

            restored.PublicContractFraudRecords.TryGet(recordId, out var restoredRecord);
            Assert.That(restoredRecord.LegalCaseId, Is.Not.Null);
        });
    }
}
