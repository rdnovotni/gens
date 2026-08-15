using Gens.Simulation.Characters;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class LaborOutputSystemTests
{
    [Test]
    public void ACharacterWithNoDutyIsUnaffected()
    {
        var state = new WorldState(new GameDate(50));
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(id, condition: new Condition(80, 20, 50, 20, 50)));
        var system = new LaborOutputSystem();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), new RandomStreamSet()));

        Assert.That(events, Is.Empty);
        state.Characters.TryGet(id, out var unchanged);
        Assert.That(unchanged.Condition.Fatigue, Is.EqualTo(20));
    }

    [Test]
    public void ACharacterOnDutyAccruesFatigueAndEmitsAnOutputEvent()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, condition: new Condition(80, 20, 50, 20, 50),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40))));
        var system = new LaborOutputSystem();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), new RandomStreamSet()));

        var outputEvent = events.OfType<LaborOutputComputedEvent>().Single();
        Assert.That(outputEvent.CharacterId, Is.EqualTo(id));
        Assert.That(outputEvent.Slot, Is.EqualTo(DutySlot.Cook));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Fatigue, Is.EqualTo(20 + LaborOutputCalculator.MonthlyDutyFatigueAccrual));
    }

    [Test]
    public void AnEnslavedCharacterOnDutyGetsTheRegimenHealthAndLoyaltyTrend()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        var harsh = new RegimenSettings(DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh);
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Enslaved, condition: new Condition(80, 20, 50, 20, 50),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40)), regimen: harsh));
        var system = new LaborOutputSystem();

        system.Tick(state, new MonthlyTickContext(new GameDate(50), new RandomStreamSet()));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Health, Is.EqualTo(80 + RegimenCatalog.HealthTrend(harsh)));
        Assert.That(updated.Condition.Loyalty, Is.EqualTo(50 + RegimenCatalog.LoyaltyTrend(harsh)));
    }

    [Test]
    public void AFreedmanContinuingTheSameDutyDoesNotGetARegimenTrend()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, status: LegalStatus.Freedman, condition: new Condition(80, 20, 50, 20, 50),
            duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40))));
        var system = new LaborOutputSystem();

        system.Tick(state, new MonthlyTickContext(new GameDate(50), new RandomStreamSet()));

        state.Characters.TryGet(id, out var updated);
        Assert.That(updated.Condition.Health, Is.EqualTo(80));
        Assert.That(updated.Condition.Loyalty, Is.EqualTo(50));
    }

    [Test]
    public void ADeceasedCharacterOnDutyIsSkipped()
    {
        var state = new WorldState(new GameDate(50));
        var householdId = state.HouseholdIds.Issue();
        var id = state.CharacterIds.Issue();
        state.Characters.Add(id, CharacterTestFixtures.Minimal(
            id, duty: new DutyAssignment(householdId, DutySlot.Cook, new GameDate(40)),
            deathRecord: new DeathRecord(new GameDate(45), DeathCause.Disease, 30)));
        var system = new LaborOutputSystem();

        var events = system.Tick(state, new MonthlyTickContext(new GameDate(50), new RandomStreamSet()));

        Assert.That(events, Is.Empty);
    }
}
