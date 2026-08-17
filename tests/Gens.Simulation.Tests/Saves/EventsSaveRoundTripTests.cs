using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 9 item 3 save round-trip coverage, mirroring <see cref="PoliciesSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class EventsSaveRoundTripTests
{
    [Test]
    public void EventInstancesRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var instanceId = state.EventInstanceIds.Issue();
        var instance = new EventInstance(
            instanceId,
            new DefinitionId<EventDefinition>("some-def"),
            EventScope.Household,
            new[] { "household_0000001" },
            "household_0000001",
            CurrentStageIndex: 1,
            FiredDate: new GameDate(2),
            ExpiresDate: new GameDate(8),
            Status: EventInstanceStatus.AwaitingNextStage,
            ResolvedOptionId: new DefinitionId<EventOptionDefinition>("some-option"),
            ResolvedDate: new GameDate(4),
            ResolvingEventId: "event_0000003");
        state.EventInstances.Add(instanceId, instance);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.EventInstances.TryGet(instanceId, out var restoredInstance), Is.True);
            Assert.That(restoredInstance!.InstanceId, Is.EqualTo(instance.InstanceId));
            Assert.That(restoredInstance.DefinitionId, Is.EqualTo(instance.DefinitionId));
            Assert.That(restoredInstance.Scope, Is.EqualTo(instance.Scope));
            Assert.That(restoredInstance.SubjectIds, Is.EqualTo(instance.SubjectIds));
            Assert.That(restoredInstance.ActorId, Is.EqualTo(instance.ActorId));
            Assert.That(restoredInstance.CurrentStageIndex, Is.EqualTo(instance.CurrentStageIndex));
            Assert.That(restoredInstance.FiredDate, Is.EqualTo(instance.FiredDate));
            Assert.That(restoredInstance.ExpiresDate, Is.EqualTo(instance.ExpiresDate));
            Assert.That(restoredInstance.Status, Is.EqualTo(instance.Status));
            Assert.That(restoredInstance.ResolvedOptionId, Is.EqualTo(instance.ResolvedOptionId));
            Assert.That(restoredInstance.ResolvedDate, Is.EqualTo(instance.ResolvedDate));
            Assert.That(restoredInstance.ResolvingEventId, Is.EqualTo(instance.ResolvingEventId));
            Assert.That(restored.EventInstanceIds.Peek, Is.EqualTo(state.EventInstanceIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyPhase9Item3Data()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.EventInstances.Count, Is.EqualTo(0));
        Assert.That(loaded.State.EventInstanceIds.Peek, Is.EqualTo(0));
    }
}
