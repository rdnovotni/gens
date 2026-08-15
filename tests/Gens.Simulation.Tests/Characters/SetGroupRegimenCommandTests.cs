using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class SetGroupRegimenCommandTests
{
    private static readonly GameDate SubmittedDate = new(300);
    private static readonly RegimenSettings Harsh = new(
        DietTier.Meager, AccommodationTier.Bare, FreedomsTier.Confined, DisciplineTier.Harsh);

    [Test]
    public void SettingAWholeHouseholdDefaultStoresItUnderTheNullSlotKey()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();

        var command = new SetGroupRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, householdId, null, Harsh);
        var result = SetGroupRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Events.Single(), Is.InstanceOf<GroupRegimenSetEvent>());
        Assert.That(state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out var stored), Is.True);
        Assert.That(stored, Is.EqualTo(Harsh));
    }

    [Test]
    public void SettingAPerSlotDefaultDoesNotAffectTheWholeHouseholdDefault()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();

        var command = new SetGroupRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, householdId, DutySlot.FieldHand, Harsh);
        SetGroupRegimenCommands.Pipeline.Execute(state, command);

        Assert.That(state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, DutySlot.FieldHand), out _), Is.True);
        Assert.That(state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out _), Is.False);
    }

    [Test]
    public void SettingTheSameKeyTwiceReplacesTheEarlierValue()
    {
        var state = new WorldState(SubmittedDate);
        var householdId = state.HouseholdIds.Issue();
        var first = new SetGroupRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, householdId, null, Harsh);
        SetGroupRegimenCommands.Pipeline.Execute(state, first);

        var comfortable = new RegimenSettings(
            DietTier.Generous, AccommodationTier.Comfortable, FreedomsTier.FreeMovement, DisciplineTier.Lenient);
        var second = new SetGroupRegimenCommand(
            state.CommandIds.Issue(), "player", SubmittedDate, null, householdId, null, comfortable);
        var result = SetGroupRegimenCommands.Pipeline.Execute(state, second);

        Assert.That(result.Accepted, Is.True);
        state.HouseholdRegimenDefaults.TryGet(new HouseholdRegimenKey(householdId, null), out var stored);
        Assert.That(stored, Is.EqualTo(comfortable));
    }
}
