using Gens.Simulation.Characters;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class RecruitWandererCommandTests
{
    private const string StreamName = "test-wanderer-generation";
    private static readonly GameDate Now = new(30);

    private static RandomStreamSet Streams(ulong seed = 11)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, seed);
        return streams;
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Settlement> SettlementId) Campaign()
    {
        var state = new WorldState(Now);
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        return (state, state.HouseholdIds.Issue(), settlementId);
    }

    private static RecruitWandererCommand Command(
        WorldState state, RuntimeId<Wanderer> wandererId, RuntimeId<Household> householdId, RuntimeId<Settlement> settlementId) =>
        new(state.CommandIds.Issue(), "player", Now, null, wandererId, householdId, settlementId, StreamName);

    [Test]
    public void RecruitingConvertsTheWandererIntoARealHouseholdCharacterAndEndsTheItinerary()
    {
        var (state, householdId, settlementId) = Campaign();
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.PhilosopherRhetorician, fame: 63);
        var profile = WandererTestFixtures.TypeCatalog.Get(WandererType.PhilosopherRhetorician);
        var pipeline = RecruitWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));

        Assert.That(result.Accepted, Is.True);
        var recruited = result.Events.OfType<WandererRecruitedEvent>().Single();
        state.Characters.TryGet(recruited.CharacterId, out var character);
        state.Wanderers.TryGet(wanderer.Id, out var after);
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account);

        Assert.Multiple(() =>
        {
            // The Character genuinely is the person the player was tracking.
            Assert.That(character!.Praenomen, Is.EqualTo(wanderer.Name.Praenomen));
            Assert.That(character.Nomen, Is.EqualTo(wanderer.Name.Nomen));
            Assert.That(character.Cognomen, Is.EqualTo(wanderer.Name.Cognomen));
            Assert.That(character.Sex, Is.EqualTo(wanderer.Sex));
            Assert.That(character.BirthDate, Is.EqualTo(wanderer.BirthDate));
            Assert.That(character.LegalStatus, Is.EqualTo(wanderer.LegalStatus));
            Assert.That(character.Culture, Is.EqualTo(wanderer.Culture));

            // Familia's real promotion shape.
            Assert.That(character.Household, Is.EqualTo(householdId));
            Assert.That(character.Location, Is.EqualTo(settlementId));
            Assert.That(character.Source, Is.EqualTo(CharacterSource.CourtPosition));
            Assert.That(character.BackfilledHistory, Is.True);
            Assert.That(character.IsAlive, Is.True);

            // §4's universal Fame field is joined at exactly this moment.
            Assert.That(FameResolver.Current(state, recruited.CharacterId), Is.EqualTo(63));

            // §6: the independent Itinerary ends.
            Assert.That(after!.Status, Is.EqualTo(WandererStatus.Recruited));
            Assert.That(after.IsActivelyTracked, Is.False);
            Assert.That(after.Itinerary, Is.Empty);
            Assert.That(after.RecruitedCharacterId, Is.EqualTo(recruited.CharacterId));
            Assert.That(after.CommittedHouseholdId, Is.EqualTo(householdId));

            Assert.That(account!.Balance, Is.EqualTo(Money.Zero - profile.RecruitFee));
        });
    }

    [Test]
    public void RecruitingAPhysicianPlacesThemInTheRealPhysicianDutySlotWhenTheySkillCheckIn()
    {
        // Seed-searched so the recruit's rolled Medicine clears DutySlotCatalog.MinimumCompetence.
        var seed = FindSeedWhereMedicineClearsCompetence();
        var (state, householdId, settlementId) = Campaign();
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Physician);
        var pipeline = RecruitWandererCommands.CreatePipeline(Streams(seed), WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));

        var recruited = result.Events.OfType<WandererRecruitedEvent>().Single();
        state.Characters.TryGet(recruited.CharacterId, out var character);
        var engagement = state.WandererEngagements.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(recruited.DutySlot, Is.EqualTo(DutySlot.Physician));
            Assert.That(character!.Duty, Is.Not.Null);
            Assert.That(character.Duty!.Value.Slot, Is.EqualTo(DutySlot.Physician));
            Assert.That(character.Duty.Value.HouseholdId, Is.EqualTo(householdId));
            Assert.That(engagement.ResultingDutySlot, Is.EqualTo(DutySlot.Physician));
            Assert.That(engagement.EngagementType, Is.EqualTo(WandererEngagementType.Recruit));
        });
    }

    [Test]
    public void ATypeWithNoHonestDutySlotStillJoinsTheHouseholdWithoutOne()
    {
        var (state, householdId, settlementId) = Campaign();
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Entertainer);
        var pipeline = RecruitWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));

        var recruited = result.Events.OfType<WandererRecruitedEvent>().Single();
        state.Characters.TryGet(recruited.CharacterId, out var character);

        Assert.Multiple(() =>
        {
            Assert.That(recruited.DutySlot, Is.Null);
            Assert.That(character!.Duty, Is.Null);
            Assert.That(character.Household, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void AFullPhysicianSlotLeavesTheRecruitInTheHouseholdWithoutTheDuty()
    {
        var seed = FindSeedWhereMedicineClearsCompetence();
        var (state, householdId, settlementId) = Campaign();
        var sittingPhysicianId = state.CharacterIds.Issue();
        state.Characters.Add(sittingPhysicianId, CharacterTestFixtures.Minimal(
            sittingPhysicianId,
            household: householdId,
            duty: new DutyAssignment(householdId, DutySlot.Physician, new GameDate(1))));
        var wanderer = WandererTestFixtures.AddWanderer(state, WandererType.Physician);
        var pipeline = RecruitWandererCommands.CreatePipeline(Streams(seed), WandererTestFixtures.TypeCatalog);

        var result = pipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));

        var recruited = result.Events.OfType<WandererRecruitedEvent>().Single();
        state.Characters.TryGet(recruited.CharacterId, out var character);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True, "a full slot never rejects the recruit itself.");
            Assert.That(recruited.DutySlot, Is.Null);
            Assert.That(character!.Duty, Is.Null);
            Assert.That(character.Household, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void ARecruitedWandererCannotBeRecruitedOrHostedAgain()
    {
        var (state, householdId, settlementId) = Campaign();
        var wanderer = WandererTestFixtures.AddWanderer(state);
        var recruitPipeline = RecruitWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.TypeCatalog);
        recruitPipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));

        var second = recruitPipeline.Execute(state, Command(state, wanderer.Id, householdId, settlementId));
        var host = HostWandererCommands.CreatePipeline(WandererTestFixtures.TypeCatalog).Execute(
            state,
            new HostWandererCommand(state.CommandIds.Issue(), "player", Now, null, wanderer.Id, householdId));

        Assert.Multiple(() =>
        {
            Assert.That(second.Error, Is.EqualTo(RecruitWandererCommands.WandererUnavailable));
            Assert.That(host.Error, Is.EqualTo(HostWandererCommands.WandererUnavailable));
        });
    }

    [Test]
    public void ValidationRejectsAMissingWandererAndAnUnknownSettlement()
    {
        var (state, householdId, _) = Campaign();
        var wanderer = WandererTestFixtures.AddWanderer(state);
        var pipeline = RecruitWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.TypeCatalog);

        Assert.Multiple(() =>
        {
            Assert.That(
                pipeline.Execute(state, Command(
                    state, state.WandererIds.Issue(), householdId, state.SettlementIds.Issue())).Error,
                Is.EqualTo(RecruitWandererCommands.WandererNotFound));
            Assert.That(
                pipeline.Execute(state, Command(state, wanderer.Id, householdId, state.SettlementIds.Issue())).Error,
                Is.EqualTo(RecruitWandererCommands.UnknownSettlement));
        });
    }

    private static ulong FindSeedWhereMedicineClearsCompetence()
    {
        for (ulong seed = 1; seed < 500; seed++)
        {
            var streams = new RandomStreamSet();
            streams.AddDerived(StreamName, seed);
            CharacterVisualProfileGenerator.Generate(streams, StreamName);
            var (_, skills) = CharacterBackfillGenerator.RollAttributesAndSkills(streams, StreamName);
            if (skills.Medicine >= DutySlotCatalog.MinimumCompetence)
                return seed;
        }

        throw new InvalidOperationException("No seed under 500 produced a competent physician recruit.");
    }
}
