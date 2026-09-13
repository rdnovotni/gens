using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Romance;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 5 coverage for the household-proximity "spontaneous romance" monthly
/// tick (<c>gens-romance-sexuality-lineage-design.md</c> §8, §8.1).</summary>
public sealed class AutonomousRomanceTests
{
    // Generous relative to the flat 3%-per-pair-per-month roll (RomanceCatalog.AutonomousRomanceMonthlyChancePercent):
    // over 300 months the expected number of hits is ~9, making a false "never triggers" failure
    // astronomically unlikely for any seed, matching SchemeProgressSystemTests' own budgeted-loop idiom.
    private const int GenerousMonthBudget = 300;

    private static RandomStreamSet Streams(ulong seed = 1)
    {
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RomanceAutonomousInitiationStreamName, seed, 1);
        return streams;
    }

    private static WorldState NewState() => new(new GameDate(0));

    private static RuntimeId<Character> AddHouseholdMember(
        WorldState state, RuntimeId<Household> householdId, Sex sex, GameDate birthDate,
        LegalStatus status = LegalStatus.RomanCitizen, MarriageRecord[]? maritalHistory = null)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, sex: sex, birthDate: birthDate, household: householdId, status: status,
                maritalHistory: maritalHistory));
        return id;
    }

    private static void TickMany(WorldState state, RandomStreamSet streams, int months)
    {
        var system = new AutonomousRomanceSystem();
        for (var month = 1; month <= months; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new AutonomousRomanceSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "characters", "romanticBonds" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "romanticBonds", "eventIds" }));
        });
    }

    [Test]
    public void AnEligibleAdultPairSharingAHouseholdEventuallyFormsARomanticBond()
    {
        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var adultBirthDate = new GameDate(-30 * 12);
        var a = AddHouseholdMember(state, householdId, Sex.Male, adultBirthDate);
        var b = AddHouseholdMember(state, householdId, Sex.Female, adultBirthDate);

        TickMany(state, Streams(), GenerousMonthBudget);

        var hasBond = state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.That(hasBond, Is.True, "Expected an eligible household pair to form a bond within the test's month budget.");
        Assert.Multiple(() =>
        {
            Assert.That(bond.Affection, Is.GreaterThan(0));
            Assert.That(bond.Attraction, Is.GreaterThan(0));
        });
    }

    [Test]
    public void AnAdolescentPairedWithAnAdultInTheSameHouseholdNeverFormsABond()
    {
        // A short budget, not GenerousMonthBudget: a 15-year-old ages into Adult (18+) after 36
        // months, which would make the pair genuinely eligible mid-run and defeat the point of this
        // test — eligibility rejection itself is deterministic regardless of month count, so a short
        // budget demonstrates "never triggers" just as well while keeping the pair Adolescent
        // throughout.
        const int shortMonthBudget = 24;

        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var adult = AddHouseholdMember(state, householdId, Sex.Male, new GameDate(-30 * 12));
        var adolescent = AddHouseholdMember(state, householdId, Sex.Female, new GameDate(-15 * 12));

        TickMany(state, Streams(), shortMonthBudget);

        Assert.That(state.RomanticBonds.TryGet(RomanticBondKey.Create(adult, adolescent), out _), Is.False);
    }

    [Test]
    public void AnEnslavedCharacterPairedWithAnAdultInTheSameHouseholdNeverFormsABond()
    {
        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var adult = AddHouseholdMember(state, householdId, Sex.Male, new GameDate(-30 * 12));
        var enslaved = AddHouseholdMember(
            state, householdId, Sex.Female, new GameDate(-30 * 12), status: LegalStatus.Enslaved);

        TickMany(state, Streams(), GenerousMonthBudget);

        Assert.That(state.RomanticBonds.TryGet(RomanticBondKey.Create(adult, enslaved), out _), Is.False);
    }

    [Test]
    public void AnAlreadyMaturePairWithNoOutsideMarriagePromotesToCourtshipOnTheNextSuccessfulRoll()
    {
        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var a = AddHouseholdMember(state, householdId, Sex.Male, new GameDate(-30 * 12));
        var b = AddHouseholdMember(state, householdId, Sex.Female, new GameDate(-30 * 12));
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, a, b,
                RomanceCatalog.AutonomousRomanceMinimumScore, RomanceCatalog.AutonomousRomanceMinimumScore, null));

        TickMany(state, Streams(), GenerousMonthBudget);

        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.That(bond.BondType, Is.EqualTo(RomanticBondType.Courtship));
    }

    [Test]
    public void AnAlreadyMaturePairWhereOnePartyIsMarriedElsewherePromotesToAffairOnTheNextSuccessfulRoll()
    {
        var state = NewState();
        var householdId = state.HouseholdIds.Issue();
        var outsideSpouse = AddHouseholdMember(state, state.HouseholdIds.Issue(), Sex.Female, new GameDate(-30 * 12));
        var a = AddHouseholdMember(
            state, householdId, Sex.Male, new GameDate(-30 * 12),
            maritalHistory: new[] { new MarriageRecord(outsideSpouse, new GameDate(-10), null, null) });
        var b = AddHouseholdMember(state, householdId, Sex.Female, new GameDate(-30 * 12));
        RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "system", new GameDate(0), null, a, b,
                RomanceCatalog.AutonomousRomanceMinimumScore, RomanceCatalog.AutonomousRomanceMinimumScore, null));

        TickMany(state, Streams(), GenerousMonthBudget);

        state.RomanticBonds.TryGet(RomanticBondKey.Create(a, b), out var bond);
        Assert.That(bond.BondType, Is.EqualTo(RomanticBondType.Affair));
    }
}
