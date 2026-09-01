using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WandererSystemTests
{
    private const string StreamName = "test-wanderer-itinerary";

    private static WandererSystem BuildSystem() =>
        new(StreamName, WandererTestFixtures.TypeCatalog, WandererTestFixtures.BuildRegionCatalog());

    private static MonthlyTickContext Context(GameDate date, ulong seed = 3)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, seed);
        return new MonthlyTickContext(date, streams);
    }

    [Test]
    public void AnEmptyCampaignProducesNoEvents()
    {
        var state = new WorldState(new GameDate(1));

        var events = BuildSystem().Tick(state, Context(new GameDate(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void TheSystemRunsInTheLivingWorldPhaseWithNoPrerequisites()
    {
        var system = BuildSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Prerequisites, Is.Empty);
            Assert.That(system.Writes, Does.Contain("wanderers"));
        });
    }

    [Test]
    public void AWandererStaysPutUntilTheDwellPeriodElapsesThenMovesOn()
    {
        var state = new WorldState(new GameDate(0));
        var wanderer = WandererTestFixtures.AddWanderer(state, arrivalDate: new GameDate(0));
        var system = BuildSystem();

        for (var month = 1; month < WandererItineraryCalculator.MonthsPerStop; month++)
        {
            var quiet = system.Tick(state, Context(new GameDate(month)));
            Assert.That(quiet.OfType<WandererMovedEvent>(), Is.Empty, $"moved early at month {month}.");
        }

        var events = system.Tick(state, Context(new GameDate(WandererItineraryCalculator.MonthsPerStop)));

        state.Wanderers.TryGet(wanderer.Id, out var moved);
        var movedEvent = events.OfType<WandererMovedEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(movedEvent.FromLocationId, Is.EqualTo(WandererTestFixtures.Seat));
            Assert.That(movedEvent.ToLocationId, Is.Not.EqualTo(WandererTestFixtures.Seat));
            Assert.That(moved!.CurrentLocationId, Is.EqualTo(movedEvent.ToLocationId));
            Assert.That(moved.Itinerary, Has.Count.EqualTo(2));
            Assert.That(moved.Itinerary[^1].ArrivalMonth, Is.EqualTo(WandererItineraryCalculator.MonthsPerStop));
        });
    }

    [Test]
    public void TheSameSeedReproducesTheSameTourExactly()
    {
        DefinitionIdTour Run()
        {
            var state = new WorldState(new GameDate(0));
            var wanderer = WandererTestFixtures.AddWanderer(state, arrivalDate: new GameDate(0));
            var system = BuildSystem();
            var streams = new RandomStreamSet();
            streams.AddDerived(StreamName, 3);

            for (var month = 1; month <= 12; month++)
                system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

            state.Wanderers.TryGet(wanderer.Id, out var final);
            return new DefinitionIdTour(final!.Itinerary.Select(stop => stop.LocationId.Value).ToArray());
        }

        Assert.That(Run().Stops, Is.EqualTo(Run().Stops));
    }

    [Test]
    public void FameHoldsThroughTheGracePeriodThenFadesThroughSustainedObscurity()
    {
        var state = new WorldState(new GameDate(0));
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 50, arrivalDate: new GameDate(0));
        var system = BuildSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 3);

        for (var month = 1; month <= WandererFameCalculator.ObscurityGracePeriodMonths - 1; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        state.Wanderers.TryGet(wanderer.Id, out var stillFamous);
        Assert.Multiple(() =>
        {
            Assert.That(stillFamous!.Fame, Is.EqualTo(50));
            Assert.That(stillFamous.FameTrend, Is.EqualTo(WandererFameTrend.Established));
        });

        system.Tick(state, new MonthlyTickContext(new GameDate(WandererFameCalculator.ObscurityGracePeriodMonths), streams));

        state.Wanderers.TryGet(wanderer.Id, out var fading);
        Assert.Multiple(() =>
        {
            Assert.That(fading!.Fame, Is.EqualTo(50 - WandererFameCalculator.ObscurityDecayPerMonth));
            Assert.That(fading.FameTrend, Is.EqualTo(WandererFameTrend.Declining));
            Assert.That(fading.MonthsSinceLastEngagement, Is.EqualTo(WandererFameCalculator.ObscurityGracePeriodMonths));
        });
    }

    [Test]
    public void FameNeverFallsBelowZero()
    {
        var state = new WorldState(new GameDate(0));
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 1, arrivalDate: new GameDate(0));
        var system = BuildSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 3);

        for (var month = 1; month <= 40; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        state.Wanderers.TryGet(wanderer.Id, out var forgotten);
        Assert.That(forgotten!.Fame, Is.Zero);
    }

    [Test]
    public void ARecruitedWandererIsLeftEntirelyAlone()
    {
        var state = new WorldState(new GameDate(0));
        var wanderer = WandererTestFixtures.AddWanderer(state, fame: 60, arrivalDate: new GameDate(0));
        state.Wanderers.Remove(wanderer.Id);
        state.Wanderers.Add(wanderer.Id, wanderer with
        {
            Status = WandererStatus.Recruited,
            IsActivelyTracked = false,
        });
        var system = BuildSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 3);

        for (var month = 1; month <= 24; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        state.Wanderers.TryGet(wanderer.Id, out var untouched);
        Assert.Multiple(() =>
        {
            Assert.That(untouched!.Fame, Is.EqualTo(60));
            Assert.That(untouched.CurrentLocationId, Is.EqualTo(WandererTestFixtures.Seat));
            Assert.That(untouched.MonthsSinceLastEngagement, Is.Zero);
        });
    }

    [Test]
    public void TheItineraryNeverGrowsPastItsCapOverALongTour()
    {
        var state = new WorldState(new GameDate(0));
        var wanderer = WandererTestFixtures.AddWanderer(state, arrivalDate: new GameDate(0));
        var system = BuildSystem();
        var streams = new RandomStreamSet();
        streams.AddDerived(StreamName, 3);

        for (var month = 1; month <= 120; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        state.Wanderers.TryGet(wanderer.Id, out var veteran);
        Assert.That(veteran!.Itinerary, Has.Count.EqualTo(WandererItineraryCalculator.MaxItineraryLength));
    }

    private sealed record DefinitionIdTour(IReadOnlyList<string> Stops);
}
