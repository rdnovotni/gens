using System.Collections.Generic;
using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
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
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "romanticBonds", "characters", "affairRecords" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "romanticBonds", "affairRecords", "eventIds" }));
        });
    }
}
