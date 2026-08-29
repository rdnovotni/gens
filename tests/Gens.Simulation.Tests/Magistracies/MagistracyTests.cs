using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Magistracies;

/// <summary>Phase 12 item 2 coverage: Decurion appointment, contested elections (§5.5), Duumvir
/// pairing (§5.4), the Aedile's funding choice (§5.2), and term limits/loss of office (§5.7).</summary>
public sealed class MagistracyTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId)
        EligibleCitizen(int dignitas = MagistracyCatalog.DecurionDignitasThreshold)
    {
        var state = new WorldState(new GameDate(0));
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: "Cato", household: householdId));
        if (dignitas != 0)
            AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId, dignitas, "seed"));

        return (state, settlementId, householdId, characterId);
    }

    private static RuntimeId<MagistracyRecord> MakeDecurion(WorldState state, RuntimeId<Settlement> settlementId, RuntimeId<Character> characterId, GameDate date)
    {
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, characterId, MagistracyOffice.Decurion, settlementId, date));
        return recordId;
    }

    [Test]
    public void AppointDecurionCommandSeatsAnEligibleCitizen()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen();

        var result = AppointDecurionCommands.Pipeline.Execute(
            state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Decurion, characterId), Is.Not.Null);
            var assumed = (MagistracyAssumedEvent)result.Events[0];
            Assert.That(assumed.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void AppointDecurionCommandRejectsIneligibleLegalStatusInsufficientDignitasAndDuplicates()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen(dignitas: 0);

        Assert.That(
            AppointDecurionCommands.Pipeline.Execute(
                state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId)).Error,
            Is.EqualTo(AppointDecurionCommands.InsufficientDignitas));

        state.Characters.TryGet(characterId, out var citizen);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, citizen! with { LegalStatus = LegalStatus.Peregrine, SocialClass = null });
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(1), null, citizen.Household!.Value,
                MagistracyCatalog.DecurionDignitasThreshold, "seed"));

        Assert.That(
            AppointDecurionCommands.Pipeline.Execute(
                state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId)).Error,
            Is.EqualTo(AppointDecurionCommands.IneligibleLegalStatus));

        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, citizen with { LegalStatus = LegalStatus.RomanCitizen });
        AppointDecurionCommands.Pipeline.Execute(
            state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId));
        Assert.That(
            AppointDecurionCommands.Pipeline.Execute(
                state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId)).Error,
            Is.EqualTo(AppointDecurionCommands.AlreadyHoldsSeat));
    }

    [Test]
    public void AppointDecurionCommandRejectsOnceTheCuriaIsFull()
    {
        var (state, settlementId, _, _) = EligibleCitizen();

        for (var i = 0; i < MagistracyCatalog.DecurionCuriaSeatCount; i++)
        {
            var (_, _, householdId, characterId) = EligibleCitizenIn(state, settlementId);
            AppointDecurionCommands.Pipeline.Execute(
                state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId));
        }

        var (_, _, _, overflowCharacterId) = EligibleCitizenIn(state, settlementId);
        Assert.That(
            AppointDecurionCommands.Pipeline.Execute(
                state, new AppointDecurionCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, overflowCharacterId, settlementId)).Error,
            Is.EqualTo(AppointDecurionCommands.CuriaFull));
    }

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, RuntimeId<Character> CharacterId)
        EligibleCitizenIn(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var householdId = state.HouseholdIds.Issue();
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId, nomen: $"Citizen{characterId.Value}", household: householdId));
        AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(state.CommandIds.Issue(), "system", new GameDate(0), null, householdId,
                MagistracyCatalog.DecurionDignitasThreshold, "seed"));
        return (state, settlementId, householdId, characterId);
    }

    [Test]
    public void HoldContestedElectionCommandLetsAChallengerWinAnOpenSeat()
    {
        var (state, settlementId, householdId, characterId) = EligibleCitizen();
        MakeDecurion(state, settlementId, characterId, new GameDate(0));

        var result = HoldContestedElectionCommands.Pipeline.Execute(
            state,
            new HoldContestedElectionCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, MagistracyOffice.Aedile, settlementId,
                IncumbentCharacterId: null, characterId, InfluenceSpentByChallenger: 0, InfluenceSpentByIncumbent: 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Aedile, characterId), Is.Not.Null);
            var resolved = result.Events.OfType<ElectionResolvedEvent>().Single();
            Assert.That(resolved.WinnerId, Is.EqualTo(characterId));
            Assert.That(resolved.LoserId, Is.Null);
        });
    }

    [Test]
    public void HoldContestedElectionCommandLetsAHigherScoringChallengerUnseatTheIncumbent()
    {
        var (state, settlementId, _, incumbentId) = EligibleCitizen();
        MakeDecurion(state, settlementId, incumbentId, new GameDate(0));
        var incumbentAedileRecordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(incumbentAedileRecordId, new MagistracyRecord(incumbentAedileRecordId, incumbentId, MagistracyOffice.Aedile, settlementId, new GameDate(0)));

        var (_, _, _, challengerId) = EligibleCitizenIn(state, settlementId);
        MakeDecurion(state, settlementId, challengerId, new GameDate(0));

        // Equal attributes/Dignitas score a tie, and a tie favors the incumbent by this command's own
        // deterministic default — so give the challenger a real edge via spent Influence.
        state.Characters.TryGet(challengerId, out var challenger);
        InfluenceResolver.Apply(state, challenger!.Household!.Value, 50);

        var decisive = HoldContestedElectionCommands.Pipeline.Execute(
            state,
            new HoldContestedElectionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, MagistracyOffice.Aedile, settlementId,
                incumbentId, challengerId, InfluenceSpentByChallenger: 50, InfluenceSpentByIncumbent: 0));

        Assert.Multiple(() =>
        {
            Assert.That(decisive.Accepted, Is.True);
            var resolved = decisive.Events.OfType<ElectionResolvedEvent>().Single();
            Assert.That(resolved.WinnerId, Is.EqualTo(challengerId));
            Assert.That(resolved.LoserId, Is.EqualTo(incumbentId));
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Aedile, challengerId), Is.Not.Null);
            Assert.That(MagistracyResolver.ActiveRecord(state, settlementId, MagistracyOffice.Aedile, incumbentId), Is.Null);
            Assert.That(InfluenceResolver.Current(state, challenger.Household!.Value), Is.EqualTo(0));
        });
    }

    [Test]
    public void HoldContestedElectionCommandRejectsDecurionItselfAndANonDecurionChallenger()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen();

        Assert.That(
            HoldContestedElectionCommands.Pipeline.Execute(
                state,
                new HoldContestedElectionCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, MagistracyOffice.Decurion, settlementId,
                    null, characterId, 0, 0)).Error,
            Is.EqualTo(HoldContestedElectionCommands.NotAContestableOffice));

        Assert.That(
            HoldContestedElectionCommands.Pipeline.Execute(
                state,
                new HoldContestedElectionCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, MagistracyOffice.Aedile, settlementId,
                    null, characterId, 0, 0)).Error,
            Is.EqualTo(HoldContestedElectionCommands.ChallengerNotADecurion));
    }

    [Test]
    public void PairDuumvirsCommandLinksBothSeatsAndWritesTheCoMagistrateBond()
    {
        var (state, settlementId, _, holderAId) = EligibleCitizen();
        var (_, _, _, holderBId) = EligibleCitizenIn(state, settlementId);
        var recordAId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordAId, new MagistracyRecord(recordAId, holderAId, MagistracyOffice.Duumvir, settlementId, new GameDate(0)));
        var recordBId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordBId, new MagistracyRecord(recordBId, holderBId, MagistracyOffice.Duumvir, settlementId, new GameDate(0)));

        var result = PairDuumvirsCommands.Pipeline.Execute(
            state, new PairDuumvirsCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, settlementId, holderAId, holderBId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            state.MagistracyRecords.TryGet(recordAId, out var recordA);
            Assert.That(recordA!.CoHolderId, Is.EqualTo(holderBId));
            state.Relationships.TryGet(new RelationshipKey(holderAId, holderBId), out var relationship);
            Assert.That(relationship.HasBond(BondTag.CoMagistrate), Is.True);
        });
    }

    [Test]
    public void FundAedileWorksCommandAppliesTheChosenDignitasConsequence()
    {
        var (state, settlementId, householdId, characterId) = EligibleCitizen();
        var recordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(recordId, new MagistracyRecord(recordId, characterId, MagistracyOffice.Aedile, settlementId, new GameDate(0)));
        var before = DignitasResolver.Current(state, householdId);

        var result = FundAedileWorksCommands.Pipeline.Execute(
            state, new FundAedileWorksCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, AedileFundingChoice.FundGenerously));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId) - before, Is.EqualTo(MagistracyCatalog.AedileFundGenerouslyDignitasGain));
        });
    }

    [Test]
    public void FundAedileWorksCommandRejectsANonAedile()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen();
        Assert.That(
            FundAedileWorksCommands.Pipeline.Execute(
                state, new FundAedileWorksCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, characterId, settlementId, AedileFundingChoice.LetItPass)).Error,
            Is.EqualTo(FundAedileWorksCommands.NotAnActiveAedile));
    }

    [Test]
    public void MagistracyTermSystemAppliesThePassiveMonthlyDignitasTrickle()
    {
        var (state, settlementId, householdId, characterId) = EligibleCitizen();
        MakeDecurion(state, settlementId, characterId, new GameDate(1));
        var before = DignitasResolver.Current(state, householdId);

        var events = new MagistracyTermSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<DignitasChangedEvent>().Any(), Is.True);
            Assert.That(DignitasResolver.Current(state, householdId) - before, Is.EqualTo(MagistracyCatalog.DecurionMonthlyDignitas));
        });
    }

    [Test]
    public void MagistracyTermSystemAutoRenewsAnUnchallengedTermAtTheAnniversary()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen();
        var recordId = MakeDecurion(state, settlementId, characterId, new GameDate(0));

        new MagistracyTermSystem().Tick(state, new MonthlyTickContext(new GameDate(MagistracyCatalog.TermLengthMonths), new RandomStreamSet()));

        state.MagistracyRecords.TryGet(recordId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(MagistracyResolver.IsActive(record!), Is.True);
            Assert.That(record!.TermStartDate, Is.EqualTo(new GameDate(MagistracyCatalog.TermLengthMonths)));
        });
    }

    [Test]
    public void MagistracyTermSystemStripsOfficeOnInsolvencyWithAnExtraDignitasPenalty()
    {
        var (state, settlementId, householdId, characterId) = EligibleCitizen();
        var recordId = MakeDecurion(state, settlementId, characterId, new GameDate(0));
        state.InsolvencyStates.Add(householdId, new InsolvencyState(householdId, 3, InsolvencyStage.Insolvent, Array.Empty<InsolvencyConsequence>()));
        var before = DignitasResolver.Current(state, householdId);

        var events = new MagistracyTermSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        state.MagistracyRecords.TryGet(recordId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(MagistracyResolver.IsActive(record!), Is.False);
            Assert.That(record!.LossReason, Is.EqualTo(MagistracyLossReason.Insolvency));
            Assert.That(events.OfType<MagistracyLostEvent>().Single().LossReason, Is.EqualTo(MagistracyLossReason.Insolvency));
            // A month an office is stripped in doesn't also pay out that same month's passive trickle.
            Assert.That(before - MagistracyCatalog.EarlyLossDignitasPenalty, Is.EqualTo(DignitasResolver.Current(state, householdId)));
        });
    }

    [Test]
    public void MagistracyTermSystemVacatesTheSeatOnHolderDeath()
    {
        var (state, settlementId, _, characterId) = EligibleCitizen();
        var recordId = MakeDecurion(state, settlementId, characterId, new GameDate(0));
        state.Characters.TryGet(characterId, out var citizen);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, citizen! with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.OldAge, 60) });

        var events = new MagistracyTermSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        state.MagistracyRecords.TryGet(recordId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(MagistracyResolver.IsActive(record!), Is.False);
            Assert.That(record!.LossReason, Is.Null);
            Assert.That(events.OfType<MagistracyLostEvent>().Single().LossReason, Is.Null);
        });
    }

    [Test]
    public void MagistraciesStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, householdId, characterId) = EligibleCitizen();
        MakeDecurion(state, settlementId, characterId, new GameDate(0));
        var duumvirRecordId = state.MagistracyRecordIds.Issue();
        state.MagistracyRecords.Add(duumvirRecordId, new MagistracyRecord(duumvirRecordId, characterId, MagistracyOffice.Duumvir, settlementId, new GameDate(0), CoHolderId: null));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(MagistracyResolver.ActiveRecord(restored, settlementId, MagistracyOffice.Decurion, characterId), Is.Not.Null);
            Assert.That(MagistracyResolver.ActiveRecord(restored, settlementId, MagistracyOffice.Duumvir, characterId), Is.Not.Null);
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
