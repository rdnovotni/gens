using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 2 save round-trip coverage, mirroring <see cref="ActorsSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class StewardshipSaveRoundTripTests
{
    [Test]
    public void StewardshipAssignmentsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var householdId = state.HouseholdIds.Issue();
        var rationalisId = state.CharacterIds.Issue();

        var singleStewardId = state.StewardshipAssignmentIds.Issue();
        var singleSteward = StewardshipAssignment.Create(
            singleStewardId, householdId, StewardshipContext.Travel, StewardshipMode.SingleSteward,
            state.CharacterIds.Issue(), null, null, StewardAutonomyLevel.Standard, new GameDate(1));
        state.StewardshipAssignments.Add(singleStewardId, singleSteward);

        var councilId = state.StewardshipAssignmentIds.Issue();
        var council = StewardshipAssignment.Create(
            councilId, state.HouseholdIds.Issue(), StewardshipContext.Regency, StewardshipMode.Council,
            null, new[] { new CouncilMember(CouncilDomain.Finance, rationalisId) }, rationalisId,
            StewardAutonomyLevel.FullAutonomy, new GameDate(2)) with
        { EndDate = new GameDate(10) };
        state.StewardshipAssignments.Add(councilId, council);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.StewardshipAssignments.TryGet(singleStewardId, out var restoredSingle), Is.True);
            Assert.That(restoredSingle, Is.EqualTo(singleSteward));

            // CouncilMembers is an IReadOnlyList<CouncilMember> (array-backed): default record equality
            // compares array references, not contents, matching why EventInstance's own save round-trip
            // test asserts field-by-field rather than whole-record equality wherever a list field is
            // involved.
            Assert.That(restored.StewardshipAssignments.TryGet(councilId, out var restoredCouncil), Is.True);
            Assert.That(restoredCouncil!.AssignmentId, Is.EqualTo(council.AssignmentId));
            Assert.That(restoredCouncil.HouseholdId, Is.EqualTo(council.HouseholdId));
            Assert.That(restoredCouncil.Context, Is.EqualTo(council.Context));
            Assert.That(restoredCouncil.Mode, Is.EqualTo(council.Mode));
            Assert.That(restoredCouncil.AppointeeCharacterId, Is.EqualTo(council.AppointeeCharacterId));
            Assert.That(restoredCouncil.CouncilMembers, Is.EqualTo(council.CouncilMembers));
            Assert.That(restoredCouncil.CouncilHeadCharacterId, Is.EqualTo(council.CouncilHeadCharacterId));
            Assert.That(restoredCouncil.AutonomyLevel, Is.EqualTo(council.AutonomyLevel));
            Assert.That(restoredCouncil.StartDate, Is.EqualTo(council.StartDate));
            Assert.That(restoredCouncil.EndDate, Is.EqualTo(council.EndDate));

            Assert.That(restored.StewardshipAssignmentIds.Peek, Is.EqualTo(state.StewardshipAssignmentIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyStewardshipData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.StewardshipAssignments.Count, Is.EqualTo(0));
        Assert.That(loaded.State.StewardshipAssignmentIds.Peek, Is.EqualTo(0));
    }
}
