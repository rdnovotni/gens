using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 2 command coverage, mirroring <see cref="Policies.ChangeRitesBudgetCommandTests"/>'s
/// accept/reject shape.</summary>
public sealed class StewardshipCommandsTests
{
    private static AppointStewardshipCommand MakeAppointCommand(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> appointeeId) =>
        new(state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, householdId,
            StewardshipContext.Travel, StewardshipMode.SingleSteward, appointeeId, null, null,
            StewardshipAssignment.DefaultAutonomyLevel);

    [Test]
    public void AppointCreatesAnActiveAssignmentAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var appointeeId = state.CharacterIds.Issue();

        var result = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, appointeeId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Count.EqualTo(1));
            Assert.That(result.Events[0], Is.InstanceOf<StewardshipAssignedEvent>());
        });
    }

    [Test]
    public void AppointRejectsASecondActiveAssignmentForTheSameHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));

        var result = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(StewardshipCommands.AlreadyHasActiveAssignment));
        });
    }

    [Test]
    public void AppointAllowsANewAssignmentOnceThePriorOneHasEnded()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var firstResult = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));
        var firstAssignmentId = ((StewardshipAssignedEvent)firstResult.Events[0]).AssignmentId;
        StewardshipCommands.EndPipeline.Execute(
            state, new EndStewardshipAssignmentCommand(state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, firstAssignmentId));

        var result = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void ChangeAutonomyUpdatesTheLevelAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));
        var assignmentId = ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;

        var result = StewardshipCommands.ChangeAutonomyPipeline.Execute(
            state, new ChangeStewardshipAutonomyCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, assignmentId, StewardAutonomyLevel.FullAutonomy));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.StewardshipAssignments.TryGet(assignmentId, out var stored), Is.True);
            Assert.That(stored!.AutonomyLevel, Is.EqualTo(StewardAutonomyLevel.FullAutonomy));
        });
    }

    [Test]
    public void ChangeAutonomyRejectsANoOpChange()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));
        var assignmentId = ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;

        var result = StewardshipCommands.ChangeAutonomyPipeline.Execute(
            state, new ChangeStewardshipAutonomyCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, assignmentId,
                StewardshipAssignment.DefaultAutonomyLevel));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(StewardshipCommands.AutonomyLevelUnchanged));
        });
    }

    [Test]
    public void ChangeAutonomyRejectsAnEndedAssignment()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));
        var assignmentId = ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;
        StewardshipCommands.EndPipeline.Execute(
            state, new EndStewardshipAssignmentCommand(state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, assignmentId));

        var result = StewardshipCommands.ChangeAutonomyPipeline.Execute(
            state, new ChangeStewardshipAutonomyCommand(
                state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, assignmentId, StewardAutonomyLevel.FullAutonomy));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(StewardshipCommands.AssignmentNotActive));
        });
    }

    [Test]
    public void EndSetsTheEndDateAndMakesTheAssignmentInactive()
    {
        var state = new WorldState(new GameDate(3));
        var householdId = state.HouseholdIds.Issue();
        var appointResult = StewardshipCommands.AppointPipeline.Execute(state, MakeAppointCommand(state, householdId, state.CharacterIds.Issue()));
        var assignmentId = ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;

        var result = StewardshipCommands.EndPipeline.Execute(
            state, new EndStewardshipAssignmentCommand(state.CommandIds.Issue(), householdId.ToTaggedString(), new GameDate(9), null, assignmentId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.StewardshipAssignments.TryGet(assignmentId, out var stored), Is.True);
            Assert.That(stored!.IsActive, Is.False);
            Assert.That(stored.EndDate, Is.EqualTo(new GameDate(9)));
        });
    }
}
