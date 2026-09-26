using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Romance;

/// <summary>Phase 17 item 3 slice 6 coverage for the monthly conception roll (<see
/// cref="ConceptionSystem"/>) and childbirth resolution (<see cref="ChildbirthResolutionSystem"/>) —
/// <c>gens-romance-sexuality-lineage-design.md</c> §9 — plus slice 7's Legitimacy acknowledgment cost
/// (<see cref="AcknowledgeIllegitimateChildCommand"/>; §10).</summary>
public sealed class PregnancyAndLegitimacyTests
{
    private static readonly GameDate Epoch = new(0);

    private static WorldState NewState() => new(Epoch);

    private static RandomStreamSet ConceptionStreams(ulong seed = 1)
    {
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RomanceConceptionChanceStreamName, seed, 1);
        return streams;
    }

    private static RandomStreamSet ChildbirthStreams(ulong seed = 1)
    {
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RomanceChildbirthMaternalRiskStreamName, seed, 1);
        streams.Add(CampaignBootstrapper.RomanceChildbirthInfantRiskStreamName, seed, 1);
        streams.Add(CampaignBootstrapper.CharacterGenerationStreamName, seed, 1);
        return streams;
    }

    private static RuntimeId<Character> AddAdult(
        WorldState state, Sex sex, int fertility = 50, int health = 80,
        MarriageRecord[]? maritalHistory = null, RuntimeId<Household>? household = null)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, sex: sex, birthDate: new GameDate(-30 * 12), household: household,
                condition: new Condition(health, 0, 50, 20, fertility), maritalHistory: maritalHistory));
        return id;
    }

    private static void SeedBond(WorldState state, RuntimeId<Character> a, RuntimeId<Character> b, RomanticBondType bondType)
    {
        var key = RomanticBondKey.Create(a, b);
        var bond = new RomanticBond(bondType, 50, 50, false, 0, Epoch, Epoch, null);
        state.RomanticBonds.Add(key, bond);
    }

    private static bool HasAnyPregnancy(WorldState state, RuntimeId<Character> motherId) =>
        state.PregnancyRecords.InAscendingOrder().Any(entry => entry.Value.MotherId == motherId);

    // ---- ConceptionSystem: which bond types conceive ------------------------------------------------

    [TestCase(RomanticBondType.Marriage)]
    [TestCase(RomanticBondType.Concubinage)]
    [TestCase(RomanticBondType.Affair)]
    public void AnOppositeSexBondOfAConceivingTypeEventuallyProducesAPregnancyRecordMatchingItsBondType(
        RomanticBondType bondType)
    {
        var state = NewState();
        var mother = AddAdult(state, Sex.Female, fertility: 100);
        var father = AddAdult(state, Sex.Male, fertility: 100);
        SeedBond(state, mother, father, bondType);

        var system = new ConceptionSystem();
        var streams = ConceptionStreams();
        PregnancyRecord? record = null;
        for (var month = 1; month <= 5 && record is null; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            record = state.PregnancyRecords.InAscendingOrder()
                .Select(entry => entry.Value)
                .FirstOrDefault(p => p.MotherId == mother);
        }

        Assert.That(record, Is.Not.Null, "Expected a pregnancy to be conceived within the test's month budget.");
        Assert.That(record!.ConceivedViaBondType, Is.EqualTo(bondType));
    }

    [TestCase(RomanticBondType.Courtship)]
    [TestCase(RomanticBondType.PastRelationship)]
    public void AnOppositeSexBondOfANonConceivingTypeNeverProducesAPregnancyRecord(RomanticBondType bondType)
    {
        var state = NewState();
        var mother = AddAdult(state, Sex.Female, fertility: 100);
        var father = AddAdult(state, Sex.Male, fertility: 100);
        SeedBond(state, mother, father, bondType);

        var system = new ConceptionSystem();
        var streams = ConceptionStreams();
        for (var month = 1; month <= 5; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        Assert.That(HasAnyPregnancy(state, mother), Is.False);
    }

    [Test]
    public void ASameSexMarriageBondNeverProducesAPregnancyRecord()
    {
        var state = NewState();
        var a = AddAdult(state, Sex.Female, fertility: 100);
        var b = AddAdult(state, Sex.Female, fertility: 100);
        SeedBond(state, a, b, RomanticBondType.Marriage);

        var system = new ConceptionSystem();
        var streams = ConceptionStreams();
        for (var month = 1; month <= 5; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        Assert.That(HasAnyPregnancy(state, a), Is.False);
        Assert.That(HasAnyPregnancy(state, b), Is.False);
    }

    [Test]
    public void AMotherWithAnAlreadyUnresolvedPregnancyNeverConceivesASecondOne()
    {
        var state = NewState();
        var mother = AddAdult(state, Sex.Female, fertility: 100);
        var father = AddAdult(state, Sex.Male, fertility: 100);
        SeedBond(state, mother, father, RomanticBondType.Marriage);

        var existingPregnancyId = state.PregnancyRecordIds.Issue();
        state.PregnancyRecords.Add(
            existingPregnancyId,
            PregnancyRecord.Create(existingPregnancyId, mother, father, RomanticBondType.Marriage, Epoch));

        var system = new ConceptionSystem();
        var streams = ConceptionStreams();
        for (var month = 1; month <= 5; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        var pregnancyCount = state.PregnancyRecords.InAscendingOrder().Count(entry => entry.Value.MotherId == mother);
        Assert.That(pregnancyCount, Is.EqualTo(1));
    }

    // ---- ChildbirthResolutionSystem: legitimacy and resolution -------------------------------------

    [Test]
    public void ALegitimateBirthIsProducedWhenTheFatherMatchesTheMothersCurrentSpouse()
    {
        var state = NewState();
        var father = AddAdult(state, Sex.Male, fertility: 100, health: 100);
        var mother = AddAdult(
            state, Sex.Female, fertility: 100, health: 100,
            maritalHistory: new[] { new MarriageRecord(father, new GameDate(-9), null, null) });
        var abstractedState = WithFertilityRiskAbstracted(state);
        abstractedState.Characters.TryGet(mother, out var abstractedMother);
        abstractedState.Characters.TryGet(father, out var abstractedFather);

        var pregnancyId = abstractedState.PregnancyRecordIds.Issue();
        abstractedState.PregnancyRecords.Add(
            pregnancyId,
            PregnancyRecord.Create(pregnancyId, abstractedMother.Id, abstractedFather.Id, RomanticBondType.Marriage, Epoch));

        var events = new ChildbirthResolutionSystem().Tick(
            abstractedState, new MonthlyTickContext(new GameDate(RomanceCatalog.PregnancyTermMonths), ChildbirthStreams()));

        var bornEvent = events.OfType<CharacterBornEvent>().SingleOrDefault();
        Assert.That(bornEvent, Is.Not.Null);
        Assert.That(bornEvent!.Legitimacy, Is.EqualTo(Legitimacy.Legitimate));

        abstractedState.PregnancyRecords.TryGet(pregnancyId, out var resolved);
        Assert.Multiple(() =>
        {
            Assert.That(resolved.Resolved, Is.True);
            Assert.That(resolved.BornChildId, Is.EqualTo(bornEvent.CharacterId));
        });
    }

    [Test]
    public void AnIllegitimateBirthIsProducedWhenTheFatherDoesNotMatchTheMothersCurrentSpouse()
    {
        var state = NewState();
        var father = AddAdult(state, Sex.Male, fertility: 100, health: 100);
        var unrelatedSpouse = AddAdult(state, Sex.Male, fertility: 100, health: 100);
        var mother = AddAdult(
            state, Sex.Female, fertility: 100, health: 100,
            maritalHistory: new[] { new MarriageRecord(unrelatedSpouse, new GameDate(-9), null, null) });
        var abstractedState = WithFertilityRiskAbstracted(state);
        abstractedState.Characters.TryGet(mother, out var abstractedMother);
        abstractedState.Characters.TryGet(father, out var abstractedFather);

        var pregnancyId = abstractedState.PregnancyRecordIds.Issue();
        abstractedState.PregnancyRecords.Add(
            pregnancyId,
            PregnancyRecord.Create(pregnancyId, abstractedMother.Id, abstractedFather.Id, RomanticBondType.Affair, Epoch));

        var events = new ChildbirthResolutionSystem().Tick(
            abstractedState, new MonthlyTickContext(new GameDate(RomanceCatalog.PregnancyTermMonths), ChildbirthStreams()));

        var bornEvent = events.OfType<CharacterBornEvent>().SingleOrDefault();
        Assert.That(bornEvent, Is.Not.Null);
        Assert.That(bornEvent!.Legitimacy, Is.EqualTo(Legitimacy.Illegitimate));

        abstractedState.PregnancyRecords.TryGet(pregnancyId, out var resolved);
        Assert.That(resolved.Resolved, Is.True);
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndReadWriteSet()
    {
        var system = new ChildbirthResolutionSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "pregnancyRecords", "characters" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "pregnancyRecords", "characters", "eventIds" }));
        });
    }

    // ---- RomanceContentSettings.FertilityRiskAbstracted: zero-risk mode -----------------------------

    [Test]
    public void AbstractedFertilityRiskNeverKillsAMotherEvenWhenMaximallyUnhealthy()
    {
        const int trialCount = 200;
        var state = NewState();
        var mothers = new RuntimeId<Character>[trialCount];
        for (var i = 0; i < trialCount; i++)
        {
            var mother = AddAdult(state, Sex.Female, fertility: 100, health: 0);
            var father = AddAdult(state, Sex.Male, fertility: 100, health: 0);
            mothers[i] = mother;

            var pregnancyId = state.PregnancyRecordIds.Issue();
            state.PregnancyRecords.Add(pregnancyId, PregnancyRecord.Create(pregnancyId, mother, father, RomanticBondType.Marriage, Epoch));
        }

        var abstractedState = WithFertilityRiskAbstracted(state);

        new ChildbirthResolutionSystem().Tick(
            abstractedState, new MonthlyTickContext(new GameDate(RomanceCatalog.PregnancyTermMonths), ChildbirthStreams()));

        foreach (var motherId in mothers)
        {
            abstractedState.Characters.TryGet(motherId, out var mother);
            Assert.That(mother.IsAlive, Is.True);
        }

        var resolvedWithChild = abstractedState.PregnancyRecords.InAscendingOrder()
            .Count(entry => entry.Value.Resolved && entry.Value.BornChildId is not null);
        Assert.That(resolvedWithChild, Is.EqualTo(trialCount));
    }

    [Test]
    public void FullRiskModelingProducesAtLeastOneMaternalDeathAcrossMaximallyUnhealthyMothers()
    {
        const int trialCount = 200;
        var state = NewState();
        var mothers = new RuntimeId<Character>[trialCount];
        for (var i = 0; i < trialCount; i++)
        {
            var mother = AddAdult(state, Sex.Female, fertility: 100, health: 0);
            var father = AddAdult(state, Sex.Male, fertility: 100, health: 0);
            mothers[i] = mother;

            var pregnancyId = state.PregnancyRecordIds.Issue();
            state.PregnancyRecords.Add(pregnancyId, PregnancyRecord.Create(pregnancyId, mother, father, RomanticBondType.Marriage, Epoch));
        }

        // No round trip here: RomanceContentSettings.FertilityRiskAbstracted defaults to false, so the
        // full §9 risk model applies.
        new ChildbirthResolutionSystem().Tick(
            state, new MonthlyTickContext(new GameDate(RomanceCatalog.PregnancyTermMonths), ChildbirthStreams()));

        var deathCount = mothers.Count(motherId =>
        {
            state.Characters.TryGet(motherId, out var mother);
            return !mother.IsAlive;
        });

        Assert.That(deathCount, Is.GreaterThan(0),
            "Expected at least one maternal death across 200 maximally unhealthy mothers under full risk modeling.");
    }

    /// <summary>Round-trips <paramref name="state"/> through <see cref="WorldStateMapper"/> with <see
    /// cref="Saves.WorldSaveDocument.RomanceFertilityRiskAbstracted"/> forced to <c>true</c> on the DTO.
    /// <see cref="WorldState.RomanceContentSettings"/> has a private setter by design (mirroring <see
    /// cref="WorldState.Date"/>'s own construction-only-assignment shape) with no production settings
    /// surface yet to flip it — this save/load round trip is the only non-invasive way a test can
    /// actually exercise the <c>true</c> branch without adding a test-only setter to production
    /// code.</summary>
    private static WorldState WithFertilityRiskAbstracted(WorldState state)
    {
        var dto = WorldStateMapper.ToDto(state);
        var abstractedDto = dto with { RomanceFertilityRiskAbstracted = true };
        return WorldStateMapper.ToWorldState(abstractedDto);
    }

    // ---- Slice 7: AcknowledgeIllegitimateChildCommand's real cost (§10) ------------------------------

    private static RuntimeId<Household> EstablishHousehold(WorldState state, RuntimeId<Character> headId)
    {
        var householdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, Epoch));
        return householdId;
    }

    [Test]
    public void AcknowledgingAnIllegitimateChildAppliesARealHouseholdDignitasPenalty()
    {
        var state = NewState();
        var father = AddAdult(state, Sex.Male);
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(
            childId,
            CharacterTestFixtures.Minimal(childId, fatherId: father, legitimacy: Legitimacy.Illegitimate, birthDate: new GameDate(-240)));
        var householdId = EstablishHousehold(state, father);

        Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(0));

        var command = new AcknowledgeIllegitimateChildCommand(
            state.CommandIds.Issue(), father.ToTaggedString(), Epoch, null, householdId, father, childId);
        var result = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(
            DignitasResolver.Current(state, householdId),
            Is.EqualTo(-RomanceCatalog.IllegitimateChildAcknowledgmentDignitasPenalty));
    }

    [Test]
    public void AcknowledgingAnIllegitimateChildWhileMarriedToSomeoneElseSwingsTheBetrayedSpousesOpinionNegative()
    {
        var state = NewState();
        var spouse = AddAdult(state, Sex.Female);
        var father = AddAdult(
            state, Sex.Male,
            maritalHistory: new[] { new MarriageRecord(spouse, new GameDate(-24), null, null) });
        var motherId = state.CharacterIds.Issue();
        state.Characters.Add(motherId, CharacterTestFixtures.Minimal(motherId, sex: Sex.Female));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(
            childId,
            CharacterTestFixtures.Minimal(
                childId, motherId: motherId, fatherId: father, legitimacy: Legitimacy.Illegitimate,
                birthDate: new GameDate(-240)));
        var householdId = EstablishHousehold(state, father);

        var command = new AcknowledgeIllegitimateChildCommand(
            state.CommandIds.Issue(), father.ToTaggedString(), Epoch, null, householdId, father, childId);
        var result = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var found = state.Relationships.TryGet(new RelationshipKey(spouse, father), out var relationship);
        Assert.That(found, Is.True);
        Assert.That(
            relationship.Opinion,
            Is.EqualTo(RomanceCatalog.IllegitimateChildAcknowledgmentBetrayedSpouseOpinionDelta));
    }

    [Test]
    public void AcknowledgingAnIllegitimateChildWhileUnmarriedRecordsNoSpouseOpinionSwing()
    {
        var state = NewState();
        var father = AddAdult(state, Sex.Male);
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(
            childId,
            CharacterTestFixtures.Minimal(childId, fatherId: father, legitimacy: Legitimacy.Illegitimate, birthDate: new GameDate(-240)));
        var householdId = EstablishHousehold(state, father);

        var command = new AcknowledgeIllegitimateChildCommand(
            state.CommandIds.Issue(), father.ToTaggedString(), Epoch, null, householdId, father, childId);
        var result = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(state.Relationships.InAscendingOrder(), Is.Empty);
    }

    [Test]
    public void AcknowledgingAnIllegitimateChildWhileMarriedToTheChildsOwnOtherParentRecordsNoSpouseOpinionSwing()
    {
        var state = NewState();
        var motherId = state.CharacterIds.Issue();
        var father = state.CharacterIds.Issue();
        state.Characters.Add(
            father,
            CharacterTestFixtures.Minimal(
                father, sex: Sex.Male,
                maritalHistory: new[] { new MarriageRecord(motherId, new GameDate(-24), null, null) }));
        state.Characters.Add(
            motherId,
            CharacterTestFixtures.Minimal(
                motherId, sex: Sex.Female,
                maritalHistory: new[] { new MarriageRecord(father, new GameDate(-24), null, null) }));
        var childId = state.CharacterIds.Issue();
        state.Characters.Add(
            childId,
            CharacterTestFixtures.Minimal(
                childId, motherId: motherId, fatherId: father, legitimacy: Legitimacy.Illegitimate,
                birthDate: new GameDate(-240)));
        var householdId = EstablishHousehold(state, father);

        var command = new AcknowledgeIllegitimateChildCommand(
            state.CommandIds.Issue(), father.ToTaggedString(), Epoch, null, householdId, father, childId);
        var result = AcknowledgeIllegitimateChildCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(state.Relationships.InAscendingOrder(), Is.Empty);
    }
}
