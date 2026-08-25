using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Stewardship;

/// <summary>Phase 10 item 2 data-model coverage for <see cref="StewardshipAssignment"/>.</summary>
public sealed class StewardshipAssignmentTests
{
    [Test]
    public void CreateBuildsAValidSingleStewardAssignment()
    {
        var state = new WorldState(new GameDate(0));
        var appointeeId = state.CharacterIds.Issue();

        var assignment = StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Travel,
            StewardshipMode.SingleSteward, appointeeId, null, null, StewardshipAssignment.DefaultAutonomyLevel,
            new GameDate(0));

        Assert.Multiple(() =>
        {
            Assert.That(assignment.AppointeeCharacterId, Is.EqualTo(appointeeId));
            Assert.That(assignment.CouncilMembers, Is.Empty);
            Assert.That(assignment.IsActive, Is.True);
        });
    }

    [Test]
    public void CreateRejectsASingleStewardAssignmentWithNoAppointee()
    {
        var state = new WorldState(new GameDate(0));

        Assert.Throws<ArgumentException>(() => StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Travel,
            StewardshipMode.SingleSteward, null, null, null, StewardshipAssignment.DefaultAutonomyLevel, new GameDate(0)));
    }

    [Test]
    public void CreateRejectsASingleStewardAssignmentWithCouncilSeats()
    {
        var state = new WorldState(new GameDate(0));
        var appointeeId = state.CharacterIds.Issue();
        var seats = new[] { new CouncilMember(CouncilDomain.Finance, state.CharacterIds.Issue()) };

        Assert.Throws<ArgumentException>(() => StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Travel,
            StewardshipMode.SingleSteward, appointeeId, seats, null, StewardshipAssignment.DefaultAutonomyLevel, new GameDate(0)));
    }

    [Test]
    public void CreateBuildsAValidCouncilAssignment()
    {
        var state = new WorldState(new GameDate(0));
        var rationalisId = state.CharacterIds.Issue();
        var seats = new[] { new CouncilMember(CouncilDomain.Finance, rationalisId) };

        var assignment = StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Regency,
            StewardshipMode.Council, null, seats, rationalisId, StewardshipAssignment.DefaultAutonomyLevel, new GameDate(0));

        Assert.That(assignment.CouncilHeadCharacterId, Is.EqualTo(rationalisId));
    }

    [Test]
    public void CreateRejectsACouncilAssignmentWithNoSeats()
    {
        var state = new WorldState(new GameDate(0));

        Assert.Throws<ArgumentException>(() => StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Regency,
            StewardshipMode.Council, null, null, null, StewardshipAssignment.DefaultAutonomyLevel, new GameDate(0)));
    }

    [Test]
    public void CreateRejectsACouncilHeadThatIsNotASeatMember()
    {
        var state = new WorldState(new GameDate(0));
        var seats = new[] { new CouncilMember(CouncilDomain.Finance, state.CharacterIds.Issue()) };
        var outsiderId = state.CharacterIds.Issue();

        Assert.Throws<ArgumentException>(() => StewardshipAssignment.Create(
            state.StewardshipAssignmentIds.Issue(), state.HouseholdIds.Issue(), StewardshipContext.Regency,
            StewardshipMode.Council, null, seats, outsiderId, StewardshipAssignment.DefaultAutonomyLevel, new GameDate(0)));
    }
}
