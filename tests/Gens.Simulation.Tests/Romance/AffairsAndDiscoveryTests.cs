using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 8 coverage for <see cref="AffairDiscoverySystem"/>
/// (<c>gens-romance-sexuality-lineage-design.md</c> §11) — including the long-reserved <see
/// cref="ScandalSourceType.AffairDiscovery"/> wiring point finally getting a real caller. Every
/// resolution-roll test below sizes each party's <see cref="CoreAttributes.Intrigue"/> to push <see
/// cref="RomanceCatalog.AffairDiscoveryBaseEscalateChancePercent"/>'s own Intrigue-difference term all
/// the way to a clamped 0 or 100, making the Foiled-vs-Escalated roll itself deterministic — only how
/// many months <see cref="RomanticBond.DiscoveryRisk"/> takes to cross threshold is budgeted, matching
/// <see cref="Interactions.SchemeProgressSystemTests"/>'s own budgeted-tick-loop idiom for a
/// probabilistic monthly system.</summary>
public sealed class AffairsAndDiscoveryTests
{
    private static readonly GameDate Epoch = new(0);

    private static WorldState NewState() => new(Epoch);

    private static RandomStreamSet Streams(ulong seed = 1)
    {
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RomanceAffairDiscoveryStreamName, seed, 1);
        return streams;
    }

    private static RuntimeId<Character> AddAdult(
        WorldState state, int intrigue, Sex sex = Sex.Male, RuntimeId<Household>? household = null,
        MarriageRecord[]? maritalHistory = null)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, sex: sex, birthDate: new GameDate(-30 * 12), household: household,
                attributes: new CoreAttributes(10, 10, 10, intrigue, 10), maritalHistory: maritalHistory));
        return id;
    }

    private static void SeedAffairBond(WorldState state, RuntimeId<Character> a, RuntimeId<Character> b, int discoveryRisk = 0)
    {
        var key = RomanticBondKey.Create(a, b);
        var bond = new RomanticBond(RomanticBondType.Affair, 50, 50, false, discoveryRisk, Epoch, Epoch, null);
        state.RomanticBonds.Add(key, bond);
    }

    private static RomanticBond GetBond(WorldState state, RuntimeId<Character> a, RuntimeId<Character> b)
    {
        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        return bond;
    }

    /// <summary>Directly seeds an unresolved, high-stakes <see cref="AffairRecord"/> — bypassing <see
    /// cref="AffairDiscoverySystem"/>'s own probabilistic escalation entirely, since <see
    /// cref="ResolveAffairCommand"/>/<see cref="ExerciseExtremeLegalRemedyCommand"/> only ever consume an
    /// already-created record and have no need to re-exercise that system's own already-covered discovery
    /// roll.</summary>
    private static (RuntimeId<AffairRecord> AffairId, AffairRecord Record) SeedHighStakesAffair(
        WorldState state, RuntimeId<Character> offender, RuntimeId<Character> thirdParty, RuntimeId<Character> wrongedSpouse)
    {
        var affairId = state.AffairRecordIds.Issue();
        var record = new AffairRecord(
            affairId, offender, thirdParty, wrongedSpouse, AffairStakesLevel.HighStakes,
            InvolvesRivalHouse: false, LegitimacyContested: false, ThreatensPoliticalMarriage: false,
            Resolution: null, StatusRoleDignitasModifier: 0, DiscoveredDate: Epoch);
        state.AffairRecords.Add(affairId, record);
        return (affairId, record);
    }

    /// <summary>Establishes a real, bidirectional open marriage between two already-created Characters —
    /// <see cref="AddAdult"/>'s own one-directional <c>maritalHistory</c> stub (only the offender's own
    /// record points back at the wronged spouse) is enough for <see cref="AffairDiscoverySystem"/>'s own
    /// tests, but <see cref="Characters.EndMarriageCommand"/>/<see
    /// cref="ExerciseExtremeLegalRemedyCommand"/>'s own marriage-closing path reads <see
    /// cref="Character.CurrentSpouseId"/> off both sides, so a real Divorced/extreme-remedy test needs
    /// both records to agree.</summary>
    private static void MarryEachOther(WorldState state, RuntimeId<Character> a, RuntimeId<Character> b, GameDate startDate)
    {
        state.Characters.TryGet(a, out var characterA);
        state.Characters.Remove(a);
        state.Characters.Add(a, characterA! with { MaritalHistory = new[] { new MarriageRecord(b, startDate, null, null) } });

        state.Characters.TryGet(b, out var characterB);
        state.Characters.Remove(b);
        state.Characters.Add(b, characterB! with { MaritalHistory = new[] { new MarriageRecord(a, startDate, null, null) } });
    }

    [Test]
    public void AnUndiscoveredAffairsDiscoveryRiskClimbsMonthly()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(
            state, intrigue: 10,
            maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        SeedAffairBond(state, offender, thirdParty);

        var system = new AffairDiscoverySystem();
        system.Tick(state, new MonthlyTickContext(new GameDate(1), Streams()));

        var bond = GetBond(state, offender, thirdParty);
        Assert.Multiple(() =>
        {
            Assert.That(bond.DiscoveryRisk, Is.GreaterThan(0));
            Assert.That(bond.DiscoveryRisk, Is.LessThan(RomanceCatalog.AffairDiscoveryThresholdPercent));
            Assert.That(bond.IsKnownPublicly, Is.False);
            Assert.That(state.AffairRecords.InAscendingOrder(), Is.Empty);
        });
    }

    [Test]
    public void AMinorStakesEscalationAutoResolvesQuietlyAndWiresTheLongReservedScandalSourceType()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 100, sex: Sex.Female);
        var householdId = state.HouseholdIds.Issue();
        var offender = AddAdult(
            state, intrigue: 0, household: householdId,
            maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 0, sex: Sex.Female);
        SeedAffairBond(state, offender, thirdParty);

        var system = new AffairDiscoverySystem();
        var streams = Streams();
        AffairRecord? record = null;
        for (var month = 1; month <= 20 && record is null; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            record = state.AffairRecords.InAscendingOrder().Select(entry => entry.Value).FirstOrDefault();
        }

        Assert.That(record, Is.Not.Null, "Expected the affair to escalate within the test's month budget.");
        Assert.Multiple(() =>
        {
            Assert.That(record!.StakesLevel, Is.EqualTo(AffairStakesLevel.Minor));
            Assert.That(record.Resolution, Is.EqualTo(AffairResolution.QuietlyResolved));
            Assert.That(record.OffenderCharacterId, Is.EqualTo(offender));
            Assert.That(record.ThirdPartyCharacterId, Is.EqualTo(thirdParty));
            Assert.That(record.WrongedSpouseId, Is.EqualTo(wrongedSpouse));

            var bond = GetBond(state, offender, thirdParty);
            Assert.That(bond.IsKnownPublicly, Is.True);

            var scandal = state.ScandalRecords.InAscendingOrder()
                .Select(entry => entry.Value)
                .FirstOrDefault(s => s.SourceType == ScandalSourceType.AffairDiscovery);
            Assert.That(scandal, Is.Not.Null, "Expected a real ScandalRecord with SourceType.AffairDiscovery.");
            Assert.That(scandal!.PrimaryHouseholdId, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void AHighStakesEscalationViaAContestedLegitimacyPregnancyCreatesAnUnresolvedRecordAndEmitsAPublicEvent()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 100, sex: Sex.Female);
        var householdId = state.HouseholdIds.Issue();
        var offender = AddAdult(
            state, intrigue: 0, household: householdId,
            maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 0, sex: Sex.Female);
        SeedAffairBond(state, offender, thirdParty);

        var pregnancyId = state.PregnancyRecordIds.Issue();
        state.PregnancyRecords.Add(
            pregnancyId, PregnancyRecord.Create(pregnancyId, thirdParty, offender, RomanticBondType.Affair, Epoch));

        var system = new AffairDiscoverySystem();
        var streams = Streams();
        var allEvents = new List<IDomainEvent>();
        AffairRecord? record = null;
        for (var month = 1; month <= 20 && record is null; month++)
        {
            allEvents.AddRange(system.Tick(state, new MonthlyTickContext(new GameDate(month), streams)));
            record = state.AffairRecords.InAscendingOrder().Select(entry => entry.Value).FirstOrDefault();
        }

        Assert.That(record, Is.Not.Null, "Expected the affair to escalate within the test's month budget.");
        Assert.Multiple(() =>
        {
            Assert.That(record!.StakesLevel, Is.EqualTo(AffairStakesLevel.HighStakes));
            Assert.That(record.LegitimacyContested, Is.True);
            Assert.That(record.Resolution, Is.Null);

            var bond = GetBond(state, offender, thirdParty);
            Assert.That(bond.IsKnownPublicly, Is.True);

            var escalatedEvent = allEvents.OfType<AffairEscalatedEvent>().SingleOrDefault();
            Assert.That(escalatedEvent, Is.Not.Null);
            Assert.That(escalatedEvent!.Visibility, Is.EqualTo(Visibility.Public));
        });
    }

    [Test]
    public void AFoiledOutcomeLeavesTheBondStillPrivateWithReducedRisk()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 0, sex: Sex.Female);
        var offender = AddAdult(
            state, intrigue: 100,
            maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 100, sex: Sex.Female);
        SeedAffairBond(state, offender, thirdParty);

        var system = new AffairDiscoverySystem();
        var streams = Streams();
        for (var month = 1; month <= 30; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        var bond = GetBond(state, offender, thirdParty);
        Assert.Multiple(() =>
        {
            Assert.That(bond.IsKnownPublicly, Is.False);
            Assert.That(bond.DiscoveryRisk, Is.LessThan(RomanceCatalog.AffairDiscoveryThresholdPercent));
            Assert.That(state.AffairRecords.InAscendingOrder(), Is.Empty);
        });
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new AffairDiscoverySystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "romanticBonds", "characters", "affairRecords", "householdReputations" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "romanticBonds", "affairRecords", "householdReputations", "eventIds" }));
        });
    }

    // ---- ResolveAffairCommand (§11) --------------------------------------------------------------

    [Test]
    public void ResolveAffairCommandForgivenGrantsTheRehabilitatedTraitAndAPositiveBondSwing()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.Forgiven));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.AffairRecords.TryGet(affairId, out var record);
        state.Characters.TryGet(wrongedSpouse, out var wrongedCharacter);
        var bond = GetBond(state, wrongedSpouse, offender);

        Assert.Multiple(() =>
        {
            Assert.That(record!.Resolution, Is.EqualTo(AffairResolution.Forgiven));
            Assert.That(wrongedCharacter!.Traits, Does.Contain(RomanceCatalog.RehabilitatedTraitId));
            Assert.That(bond.Affection, Is.EqualTo(RomanceCatalog.AffairForgivenessAffectionDelta));
            Assert.That(result.Events.OfType<AffairResolvedEvent>().Single().Resolution, Is.EqualTo(AffairResolution.Forgiven));
        });
    }

    [Test]
    public void ResolveAffairCommandDivorcedEndsTheMarriage()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.Divorced));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.Characters.TryGet(wrongedSpouse, out var wrongedCharacter);
        state.Characters.TryGet(offender, out var offenderCharacter);
        state.AffairRecords.TryGet(affairId, out var record);

        Assert.Multiple(() =>
        {
            Assert.That(wrongedCharacter!.CurrentSpouseId, Is.Null);
            Assert.That(offenderCharacter!.CurrentSpouseId, Is.Null);
            Assert.That(record!.Resolution, Is.EqualTo(AffairResolution.Divorced));
        });
    }

    [Test]
    public void ResolveAffairCommandChallengedIsANoOpBeyondRecordingTheResolution()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.Challenged));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.Characters.TryGet(wrongedSpouse, out var wrongedCharacter);
        state.Characters.TryGet(offender, out var offenderCharacter);
        state.AffairRecords.TryGet(affairId, out var record);

        Assert.Multiple(() =>
        {
            Assert.That(record!.Resolution, Is.EqualTo(AffairResolution.Challenged));
            Assert.That(wrongedCharacter!.IsAlive, Is.True);
            Assert.That(offenderCharacter!.IsAlive, Is.True);
            Assert.That(wrongedCharacter.CurrentSpouseId, Is.EqualTo(offender));
        });
    }

    [Test]
    public void ResolveAffairCommandRejectsExtremeLegalRemedyResolutionValue()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(
            state, intrigue: 10, maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.ExtremeLegalRemedyExercised));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveAffairCommands.UseExerciseExtremeLegalRemedyCommand));
    }

    [Test]
    public void ResolveAffairCommandRejectsAMinorStakesRecord()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(
            state, intrigue: 10, maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var affairId = state.AffairRecordIds.Issue();
        state.AffairRecords.Add(
            affairId,
            new AffairRecord(
                affairId, offender, thirdParty, wrongedSpouse, AffairStakesLevel.Minor,
                false, false, false, AffairResolution.QuietlyResolved, 0, Epoch));

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.Forgiven));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveAffairCommands.NotHighStakes));
    }

    [Test]
    public void ResolveAffairCommandRejectsAnAlreadyResolvedHighStakesRecord()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(
            state, intrigue: 10, maritalHistory: new[] { new MarriageRecord(wrongedSpouse, new GameDate(-24), null, null) });
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var (affairId, record) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);
        state.AffairRecords.Remove(affairId);
        state.AffairRecords.Add(affairId, record with { Resolution = AffairResolution.Challenged });

        var result = ResolveAffairCommands.CreatePipeline(new RandomStreamSet()).Execute(
            state, new ResolveAffairCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, affairId, AffairResolution.Forgiven));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ResolveAffairCommands.AlreadyResolved));
    }

    // ---- ExerciseExtremeLegalRemedyCommand (§12, §17) --------------------------------------------

    [Test]
    public void ExerciseExtremeLegalRemedyCommandKillsTheThirdPartyAndAppliesTheSevereDignitasHitRegardless()
    {
        var state = NewState();
        var actorHouseholdId = state.HouseholdIds.Issue();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female, household: actorHouseholdId);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ExerciseExtremeLegalRemedyCommands.Pipeline.Execute(
            state, new ExerciseExtremeLegalRemedyCommand(
                state.CommandIds.Issue(), wrongedSpouse.ToTaggedString(), new GameDate(1), null, affairId, AlsoOffender: false));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.Characters.TryGet(thirdParty, out var deadThirdParty);
        state.Characters.TryGet(offender, out var stillLivingOffender);
        state.AffairRecords.TryGet(affairId, out var record);

        Assert.Multiple(() =>
        {
            Assert.That(deadThirdParty!.IsAlive, Is.False);
            Assert.That(stillLivingOffender!.IsAlive, Is.True);
            Assert.That(
                DignitasResolver.Current(state, actorHouseholdId),
                Is.EqualTo(-RomanceCatalog.ExtremeLegalRemedyActorHouseholdDignitasPenalty));
            Assert.That(record!.Resolution, Is.EqualTo(AffairResolution.ExtremeLegalRemedyExercised));
        });
    }

    [Test]
    public void ExerciseExtremeLegalRemedyCommandAlsoKillsTheOffenderOnlyWhenRequested()
    {
        var state = NewState();
        var actorHouseholdId = state.HouseholdIds.Issue();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female, household: actorHouseholdId);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ExerciseExtremeLegalRemedyCommands.Pipeline.Execute(
            state, new ExerciseExtremeLegalRemedyCommand(
                state.CommandIds.Issue(), wrongedSpouse.ToTaggedString(), new GameDate(1), null, affairId, AlsoOffender: true));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");

        state.Characters.TryGet(offender, out var deadOffender);
        state.Characters.TryGet(wrongedSpouse, out var actorAfter);

        Assert.Multiple(() =>
        {
            Assert.That(deadOffender!.IsAlive, Is.False);
            Assert.That(actorAfter!.CurrentSpouseId, Is.Null);
            Assert.That(
                DignitasResolver.Current(state, actorHouseholdId),
                Is.EqualTo(-RomanceCatalog.ExtremeLegalRemedyActorHouseholdDignitasPenalty));
        });
    }

    [Test]
    public void ExerciseExtremeLegalRemedyCommandRejectsANonWrongedSpouseActor()
    {
        var state = NewState();
        var wrongedSpouse = AddAdult(state, intrigue: 10, sex: Sex.Female);
        var offender = AddAdult(state, intrigue: 10);
        var thirdParty = AddAdult(state, intrigue: 10, sex: Sex.Female);
        MarryEachOther(state, wrongedSpouse, offender, new GameDate(-24));
        var (affairId, _) = SeedHighStakesAffair(state, offender, thirdParty, wrongedSpouse);

        var result = ExerciseExtremeLegalRemedyCommands.Pipeline.Execute(
            state, new ExerciseExtremeLegalRemedyCommand(
                state.CommandIds.Issue(), offender.ToTaggedString(), new GameDate(1), null, affairId, AlsoOffender: false));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ExerciseExtremeLegalRemedyCommands.ActorNotWrongedSpouse));
    }
}
