using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Collegia;
using Gens.Simulation.Commands;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Collegia;

/// <summary>Phase 12 item 6 coverage: founding a Collegium (§2-§3), membership (§2), officer appointment
/// (§3, §9), the patron sponsorship payoff (§4), Arca funding (§3), the darker organized-disruption
/// political tool and its Justified/Unjust split (§6-§7), and formal dissolution (§7).</summary>
public sealed class CollegiaTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId)
        HouseholdWithHead(string nomen = "Cornelius")
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));

        return (state, settlementId, householdId, headId);
    }

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) AddHouseholdWithHead(WorldState state, string nomen)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: nomen, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        return (householdId, headId);
    }

    private static RuntimeId<Settlement> AddSettlement(WorldState state)
    {
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return settlementId;
    }

    private static RuntimeId<Actor> FoundOpificum(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var result = FoundCollegiumCommands.Pipeline.Execute(
            state,
            new FoundCollegiumCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Fabrorum", settlementId,
                CollegiumType.Opificum, LinkedPopGroupType: PopGroupType.Opifices));
        return ((CollegiumFoundedEvent)result.Events[0]).CollegiumId;
    }

    [Test]
    public void FoundCollegiumCommandCreatesALicitumCollegiumWithNoMagisterYet()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = AddSettlement(state);

        var collegiumId = FoundOpificum(state, settlementId);

        state.Collegia.TryGet(collegiumId, out var details);
        Assert.Multiple(() =>
        {
            Assert.That(details, Is.Not.Null);
            Assert.That(details!.CollegiumType, Is.EqualTo(CollegiumType.Opificum));
            Assert.That(details.LegalStatus, Is.EqualTo(CollegiumLegalStatus.Licitum));
            Assert.That(details.LinkedPopGroupType, Is.EqualTo(PopGroupType.Opifices));
            Assert.That(CollegiumResolver.MagisterCharacterId(state, collegiumId), Is.Null);
            Assert.That(CollegiumResolver.ArcaBalance(state, collegiumId), Is.EqualTo(Money.Zero));
        });
    }

    [Test]
    public void FoundCollegiumCommandRejectsAMismatchedPopGroupOrPatronDeityLink()
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = AddSettlement(state);

        Assert.That(
            FoundCollegiumCommands.Pipeline.Execute(
                state, new FoundCollegiumCommand(
                    state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Fabrorum", settlementId,
                    CollegiumType.Opificum)).Error,
            Is.EqualTo(FoundCollegiumCommands.PopGroupTypeRequired));

        Assert.That(
            FoundCollegiumCommands.Pipeline.Execute(
                state, new FoundCollegiumCommand(
                    state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Fabrorum", settlementId,
                    CollegiumType.Opificum, LinkedPopGroupType: PopGroupType.Curiales)).Error,
            Is.EqualTo(FoundCollegiumCommands.PopGroupTypeNotTradeEligible));

        Assert.That(
            FoundCollegiumCommands.Pipeline.Execute(
                state, new FoundCollegiumCommand(
                    state.CommandIds.Issue(), "player", new GameDate(0), null, "Collegium Compitalicium", settlementId,
                    CollegiumType.Compitalicia, LinkedPatronDeity: PatronDeity.Vesta)).Error,
            Is.EqualTo(FoundCollegiumCommands.PatronDeityMustBeUnset));

        Assert.That(
            FoundCollegiumCommands.Pipeline.Execute(
                state, new FoundCollegiumCommand(
                    state.CommandIds.Issue(), "player", new GameDate(0), null, "Cult of Bacchus", settlementId,
                    CollegiumType.CultSpecific)).Error,
            Is.EqualTo(FoundCollegiumCommands.PatronDeityRequired));
    }

    [Test]
    public void JoinAndLeaveCollegiumCommandsChangeTheRoster()
    {
        var (state, settlementId, householdId, _) = HouseholdWithHead();
        var collegiumId = FoundOpificum(state, settlementId);

        var joined = CollegiumMembershipCommands.JoinPipeline.Execute(
            state, new JoinCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, householdId));
        Assert.Multiple(() =>
        {
            Assert.That(joined.Accepted, Is.True);
            Assert.That(CollegiumResolver.IsMember(state, collegiumId, householdId), Is.True);
        });

        Assert.That(
            CollegiumMembershipCommands.JoinPipeline.Execute(
                state, new JoinCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, householdId)).Error,
            Is.EqualTo(CollegiumMembershipCommands.AlreadyMember));

        var left = CollegiumMembershipCommands.LeavePipeline.Execute(
            state, new LeaveCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, householdId));
        Assert.Multiple(() =>
        {
            Assert.That(left.Accepted, Is.True);
            Assert.That(CollegiumResolver.IsMember(state, collegiumId, householdId), Is.False);
        });
    }

    [Test]
    public void ElectMagisterAndAppointQuinquennalisSeatRealOfficers()
    {
        var (state, settlementId, _, headId) = HouseholdWithHead();
        var collegiumId = FoundOpificum(state, settlementId);

        var elected = CollegiumOfficerCommands.ElectMagisterPipeline.Execute(
            state, new ElectMagisterCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, headId));

        var quinquennalisCandidate = state.CharacterIds.Issue();
        state.Characters.Add(quinquennalisCandidate, CharacterTestFixtures.Minimal(quinquennalisCandidate, nomen: "Quintus"));
        var appointed = CollegiumOfficerCommands.AppointQuinquennalisPipeline.Execute(
            state, new AppointQuinquennalisCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, quinquennalisCandidate));

        state.Collegia.TryGet(collegiumId, out var details);
        Assert.Multiple(() =>
        {
            Assert.That(elected.Accepted, Is.True);
            Assert.That(appointed.Accepted, Is.True);
            Assert.That(CollegiumResolver.MagisterCharacterId(state, collegiumId), Is.EqualTo(headId));
            Assert.That(details!.QuinquennalisCharacterId, Is.EqualTo(quinquennalisCandidate));
        });
    }

    [Test]
    public void SponsorCollegiumCommandGrantsDignitasAndInfluenceAndFormsThePatronBondWithAResolvedMagister()
    {
        var (state, settlementId, patronHouseholdId, patronHeadId) = HouseholdWithHead();
        var collegiumId = FoundOpificum(state, settlementId);

        var magisterId = state.CharacterIds.Issue();
        state.Characters.Add(magisterId, CharacterTestFixtures.Minimal(magisterId, nomen: "Magister"));
        CollegiumOfficerCommands.ElectMagisterPipeline.Execute(
            state, new ElectMagisterCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, magisterId));

        var result = SponsorCollegiumCommands.Pipeline.Execute(
            state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, patronHouseholdId));

        state.Relationships.TryGet(new RelationshipKey(patronHeadId, magisterId), out var patronToMagister);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(CollegiumResolver.IsSponsored(state, collegiumId), Is.True);
            Assert.That(DignitasResolver.Current(state, patronHouseholdId), Is.EqualTo(CollegiumCatalog.SponsorshipDignitasGrant));
            Assert.That(InfluenceResolver.Current(state, patronHouseholdId), Is.EqualTo(CollegiumCatalog.SponsorshipInfluenceGrant));
            Assert.That(patronToMagister.HasBond(BondTag.Client), Is.True);
        });

        Assert.That(
            SponsorCollegiumCommands.Pipeline.Execute(
                state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, collegiumId, patronHouseholdId)).Error,
            Is.EqualTo(SponsorCollegiumCommands.AlreadySponsored));
    }

    [Test]
    public void FundCollegiumArcaCommandMovesMoneyFromTheHouseholdIntoThePerActorLedgerAccount()
    {
        var (state, settlementId, householdId, _) = HouseholdWithHead();
        var collegiumId = FoundOpificum(state, settlementId);
        LedgerService.Post(
            state, new GameDate(0), LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(100)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-100)),
            });

        var result = FundCollegiumArcaCommands.Pipeline.Execute(
            state, new FundCollegiumArcaCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, collegiumId, Money.FromDenarii(30)));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(CollegiumResolver.ArcaBalance(state, collegiumId), Is.EqualTo(Money.FromDenarii(30)));
            state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var householdAccount);
            Assert.That(householdAccount!.Balance, Is.EqualTo(Money.FromDenarii(70)));
        });

        Assert.That(
            FundCollegiumArcaCommands.Pipeline.Execute(
                state, new FundCollegiumArcaCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, collegiumId, Money.FromDenarii(1000))).Error,
            Is.EqualTo(FundCollegiumArcaCommands.InsufficientBalance));
    }

    [Test]
    public void OrganizedDisruptionCommandCostsOnlyOpinionWhenJustifiedButDignitasAndLegalStatusWhenUnjust()
    {
        var (state, settlementId, patronHouseholdId, patronHeadId) = HouseholdWithHead("Patron");
        var collegiumId = FoundOpificum(state, settlementId);
        SponsorCollegiumCommands.Pipeline.Execute(
            state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, patronHouseholdId));

        var (justifiedTargetHouseholdId, justifiedTargetHeadId) = AddHouseholdWithHead(state, "Guilty");
        RecordPunishableOffenseCommands.Pipeline.Execute(
            state, new RecordPunishableOffenseCommand(
                state.CommandIds.Issue(), "system", new GameDate(1), null, justifiedTargetHeadId,
                PunishableOffenseSource.Fabricated, OffenseSeverity.Serious));

        var justified = RecordCollegiumOrganizedDisruptionCommands.Pipeline.Execute(
            state, new RecordCollegiumOrganizedDisruptionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, justifiedTargetHouseholdId));

        state.Collegia.TryGet(collegiumId, out var afterJustified);
        Assert.Multiple(() =>
        {
            Assert.That(justified.Accepted, Is.True);
            Assert.That(justified.Events.OfType<CollegiumOrganizedDisruptionRecordedEvent>().Single().Justified, Is.True);
            Assert.That(DignitasResolver.Current(state, patronHouseholdId), Is.EqualTo(CollegiumCatalog.SponsorshipDignitasGrant));
            Assert.That(afterJustified!.LegalStatus, Is.EqualTo(CollegiumLegalStatus.Licitum));
        });

        var (innocentTargetHouseholdId, _) = AddHouseholdWithHead(state, "Innocent");
        var beforeUnjust = DignitasResolver.Current(state, patronHouseholdId);

        var unjust = RecordCollegiumOrganizedDisruptionCommands.Pipeline.Execute(
            state, new RecordCollegiumOrganizedDisruptionCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, collegiumId, innocentTargetHouseholdId));

        state.Collegia.TryGet(collegiumId, out var afterUnjust);
        Assert.Multiple(() =>
        {
            Assert.That(unjust.Accepted, Is.True);
            Assert.That(unjust.Events.OfType<CollegiumOrganizedDisruptionRecordedEvent>().Single().Justified, Is.False);
            Assert.That(
                beforeUnjust - DignitasResolver.Current(state, patronHouseholdId),
                Is.EqualTo(CollegiumCatalog.UnjustDisruptionDignitasPenalty));
            Assert.That(afterUnjust!.LegalStatus, Is.EqualTo(CollegiumLegalStatus.Illicit));
        });
    }

    [Test]
    public void DissolveCollegiumCommandRequiresIllicitStatusAndARealSittingMagistrate()
    {
        var (state, settlementId, patronHouseholdId, magistrateHeadId) = HouseholdWithHead("Magistrate");
        var collegiumId = FoundOpificum(state, settlementId);

        Assert.That(
            DissolveCollegiumCommands.Pipeline.Execute(
                state, new DissolveCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, magistrateHeadId)).Error,
            Is.EqualTo(DissolveCollegiumCommands.NotIllicit));

        SponsorCollegiumCommands.Pipeline.Execute(
            state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, patronHouseholdId));
        var (targetHouseholdId, _) = AddHouseholdWithHead(state, "Target");
        RecordCollegiumOrganizedDisruptionCommands.Pipeline.Execute(
            state, new RecordCollegiumOrganizedDisruptionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, targetHouseholdId));

        Assert.That(
            DissolveCollegiumCommands.Pipeline.Execute(
                state, new DissolveCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, collegiumId, magistrateHeadId)).Error,
            Is.EqualTo(DissolveCollegiumCommands.NoRealAuthority));

        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(
            recordId, new MagistracyRecord(recordId, magistrateHeadId, MagistracyOffice.Decurion, settlementId, new GameDate(0)));

        var beforePatronDignitas = DignitasResolver.Current(state, patronHouseholdId);
        var dissolved = DissolveCollegiumCommands.Pipeline.Execute(
            state, new DissolveCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(3), null, collegiumId, magistrateHeadId));

        Assert.Multiple(() =>
        {
            Assert.That(dissolved.Accepted, Is.True);
            Assert.That(state.Collegia.TryGet(collegiumId, out _), Is.False);
            Assert.That(state.Actors.TryGet(collegiumId, out _), Is.False);
            Assert.That(
                beforePatronDignitas - DignitasResolver.Current(state, patronHouseholdId),
                Is.EqualTo(CollegiumCatalog.IllicitPatronDignitasPenalty));
        });
    }

    [Test]
    public void CollegiaStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, householdId, headId) = HouseholdWithHead();
        var collegiumId = FoundOpificum(state, settlementId);
        CollegiumMembershipCommands.JoinPipeline.Execute(
            state, new JoinCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, householdId));
        CollegiumOfficerCommands.ElectMagisterPipeline.Execute(
            state, new ElectMagisterCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, collegiumId, headId));
        SponsorCollegiumCommands.Pipeline.Execute(
            state, new SponsorCollegiumCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, collegiumId, householdId));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(CollegiumResolver.IsMember(restored, collegiumId, householdId), Is.True);
            Assert.That(CollegiumResolver.MagisterCharacterId(restored, collegiumId), Is.EqualTo(headId));
            Assert.That(CollegiumResolver.IsSponsored(restored, collegiumId), Is.True);
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
