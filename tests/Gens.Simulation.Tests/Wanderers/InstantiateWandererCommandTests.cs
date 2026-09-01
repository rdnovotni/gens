using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class InstantiateWandererCommandTests
{
    private const string StreamName = "test-wanderer-generation";

    private static RandomStreamSet Streams(ulong seed = 7)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, seed);
        return streams;
    }

    private static InstantiateWandererCommand Command(
        WorldState state,
        WandererType type = WandererType.PhilosopherRhetorician,
        DefinitionId<GazetteerLocationDefinition>? location = null,
        Sex? sex = Sex.Male) =>
        new(
            state.CommandIds.Issue(), "player", new GameDate(12), null, type,
            location ?? WandererTestFixtures.Seat, WandererTestFixtures.Culture, LegalStatus.Peregrine,
            NamePoolTestFixtures.Roman, StreamName, WandererInstantiationTrigger.TravelArrival, sex);

    [Test]
    public void PromotionCreatesANamedActivelyTrackedWandererAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(12));
        var pipeline = InstantiateWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.BuildRegionCatalog());

        var result = pipeline.Execute(state, Command(state));

        Assert.That(result.Accepted, Is.True);
        var instantiated = (WandererInstantiatedEvent)result.Events.Single();
        var wanderer = state.Wanderers.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(wanderer.IsActivelyTracked, Is.True);
            Assert.That(wanderer.Status, Is.EqualTo(WandererStatus.Wandering));
            Assert.That(wanderer.Type, Is.EqualTo(WandererType.PhilosopherRhetorician));
            Assert.That(wanderer.CurrentLocationId, Is.EqualTo(WandererTestFixtures.Seat));
            Assert.That(wanderer.Name.Praenomen, Is.Not.Empty);
            Assert.That(wanderer.Name.Nomen, Is.Not.Empty);
            Assert.That(wanderer.Itinerary, Has.Count.EqualTo(1));
            Assert.That(wanderer.Itinerary[0].ArrivalMonth, Is.EqualTo(12));
            Assert.That(wanderer.Fame, Is.InRange(
                WandererFameCalculator.MinimumStartingFame, WandererFameCalculator.MaximumStartingFame));
            Assert.That(wanderer.FameTrend, Is.EqualTo(WandererFameTrend.Established));
            Assert.That(wanderer.CommittedHouseholdId, Is.Null);
            Assert.That(wanderer.InterestedHouseholdIds, Is.Empty);
            Assert.That(instantiated.Fame, Is.EqualTo(wanderer.Fame));
            Assert.That(instantiated.Trigger, Is.EqualTo(WandererInstantiationTrigger.TravelArrival));
        });
    }

    [Test]
    public void TheSameSeedReproducesTheSameWandererExactly()
    {
        var catalog = WandererTestFixtures.BuildRegionCatalog();

        var firstState = new WorldState(new GameDate(12));
        InstantiateWandererCommands.CreatePipeline(Streams(), catalog).Execute(firstState, Command(firstState, sex: null));

        var secondState = new WorldState(new GameDate(12));
        InstantiateWandererCommands.CreatePipeline(Streams(), catalog).Execute(secondState, Command(secondState, sex: null));

        var first = firstState.Wanderers.InAscendingOrder().Single().Value;
        var second = secondState.Wanderers.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(second.Name, Is.EqualTo(first.Name));
            Assert.That(second.Sex, Is.EqualTo(first.Sex));
            Assert.That(second.BirthDate, Is.EqualTo(first.BirthDate));
            Assert.That(second.Fame, Is.EqualTo(first.Fame));
        });
    }

    [Test]
    public void ValidationRejectsALocationThatIsNotInAnyRegionGazetteer()
    {
        var state = new WorldState(new GameDate(12));
        var pipeline = InstantiateWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.BuildRegionCatalog());

        var result = pipeline.Execute(state, Command(
            state, location: new DefinitionId<GazetteerLocationDefinition>("nowhere")));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(InstantiateWandererCommands.UnknownLocation));
        });
    }

    [Test]
    public void SamplingRestraintRefusesASecondWandererOfTheSameTypeInTheSamePlace()
    {
        var state = new WorldState(new GameDate(12));
        var pipeline = InstantiateWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.BuildRegionCatalog());

        Assert.That(pipeline.Execute(state, Command(state)).Accepted, Is.True);
        var second = pipeline.Execute(state, Command(state));

        Assert.Multiple(() =>
        {
            Assert.That(second.Accepted, Is.False);
            Assert.That(second.Error, Is.EqualTo(InstantiateWandererCommands.AlreadyTrackedHere));
            Assert.That(state.Wanderers.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void ADifferentTypeOrADifferentPlaceIsStillAllowed()
    {
        var state = new WorldState(new GameDate(12));
        var pipeline = InstantiateWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.BuildRegionCatalog());

        Assert.That(pipeline.Execute(state, Command(state)).Accepted, Is.True);
        Assert.That(pipeline.Execute(state, Command(state, type: WandererType.Physician)).Accepted, Is.True);
        Assert.That(
            pipeline.Execute(state, Command(state, location: WandererTestFixtures.Port)).Accepted,
            Is.True);

        Assert.That(state.Wanderers.Count, Is.EqualTo(3));
    }

    [Test]
    public void ARecruitedWandererNoLongerBlocksASuccessorInTheSamePlace()
    {
        // IsActivelyTracked false means the person has left the itinerant world entirely — the record
        // persists as history but must not keep the sampling slot occupied forever.
        var state = new WorldState(new GameDate(12));
        var wanderer = WandererTestFixtures.AddWanderer(state);
        state.Wanderers.Remove(wanderer.Id);
        state.Wanderers.Add(wanderer.Id, wanderer with
        {
            IsActivelyTracked = false,
            Status = WandererStatus.Recruited,
        });

        var pipeline = InstantiateWandererCommands.CreatePipeline(Streams(), WandererTestFixtures.BuildRegionCatalog());

        Assert.That(pipeline.Execute(state, Command(state)).Accepted, Is.True);
    }
}
