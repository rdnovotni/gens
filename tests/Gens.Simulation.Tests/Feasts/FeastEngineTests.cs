using Gens.Simulation.Activities;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Cultures;
using Gens.Simulation.Feasts;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Villas;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Feasts;

/// <summary>Phase 17 item 5 coverage: the Feast Activity Type wired onto the generic Activity Engine —
/// planning (Purpose, Arbiter Bibendi, phase sequence, reused Quality-input validation), seating
/// assignment, seating-judgment resolution and its Insult/Honor/envy consequences, the Scandal call
/// site, the Meal-phase disruption-targeting fix, Comissatio-phase Interactions, Chronicle wiring, save
/// round-trip, and state-hash stability.</summary>
public sealed class FeastEngineTests
{
    private static readonly GameDate Date0 = new(0);
    private const ulong Seed = 17_5UL;

    // ---- Fixtures --------------------------------------------------------------------------------

    private sealed record World(
        WorldState State,
        RuntimeId<Household> HouseholdId,
        RuntimeId<Settlement> SettlementId,
        RuntimeId<Holding> HoldingId,
        RuntimeId<Character> HostId);

    private static World NewWorld()
    {
        var state = new WorldState(Date0);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();

        var villa = new Villa(VillaStage.Domus);
        villa.AddRoom(new VillaRoomInstance(new VillaRoomDefinition(FeastCatalog.DefaultVenueKey, VillaStage.Rustica, maximumTier: 3), tier: 1));
        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId, villa: villa));

        var hostId = Adult(state, householdId, settlementId, "Host");
        return new World(state, householdId, settlementId, holdingId, hostId);
    }

    private static RuntimeId<Character> Adult(
        WorldState state, RuntimeId<Household>? householdId, RuntimeId<Settlement> settlementId, string nomen, Sex sex = Sex.Male)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, nomen: nomen, household: householdId, location: settlementId, sex: sex,
                birthDate: new GameDate(-30 * 12)) with
            { Culture = new DefinitionId<Culture>("roman") });
        return id;
    }

    /// <summary>A guest with their own tracked household and a given starting Dignitas, so <see
    /// cref="FeastSeatingResolutionSystem"/>'s "expected standing" reads differ between guests.</summary>
    private static RuntimeId<Character> GuestWithStanding(World world, string nomen, int dignitas)
    {
        var householdId = world.State.HouseholdIds.Issue();
        var guestId = Adult(world.State, householdId, world.SettlementId, nomen);
        if (dignitas != 0)
            DignitasResolver.Apply(world.State, householdId, dignitas);
        return guestId;
    }

    private static void Tie(WorldState state, RuntimeId<Character> from, RuntimeId<Character> to, int opinion, BondTag bonds = BondTag.None)
    {
        var key = new RelationshipKey(from, to);
        state.Relationships.Remove(key);
        state.Relationships.Add(key, new Relationship(opinion, bonds, RelationshipOrigin.Encounter, Date0, Date0, null));
    }

    private static int Opinion(WorldState state, RuntimeId<Character> from, RuntimeId<Character> to) =>
        state.Relationships.TryGet(new RelationshipKey(from, to), out var tie) ? tie.Opinion : 0;

    private static ActivityQualityInput[] Inputs(int score) => new[]
    {
        new ActivityQualityInput(ActivityTypeCatalog.ProvisioningInput, score),
        new ActivityQualityInput(ActivityTypeCatalog.HospitalityInput, score),
        new ActivityQualityInput(ActivityTypeCatalog.EntertainmentInput, score),
    };

    private static PlanFeastCommand PlanCommand(
        World world,
        IReadOnlyList<RuntimeId<Character>> guests,
        FeastPurpose purpose = FeastPurpose.OrdinarySocial,
        RuntimeId<Character>? arbiterBibendiId = null,
        ActivityVenueKind venueKind = ActivityVenueKind.VillaRoom,
        string venueKey = FeastCatalog.DefaultVenueKey,
        int quality = 60) =>
        new(
            world.State.CommandIds.Issue(), "player", Date0, null, world.HostId, world.HouseholdId, null,
            venueKind, venueKey, world.SettlementId, venueKind == ActivityVenueKind.VillaRoom ? world.HoldingId : null,
            new GameDate(1), guests, Inputs(quality), Money.Zero, purpose, arbiterBibendiId, null);

    private static RuntimeId<Activity> Plan(
        World world, IReadOnlyList<RuntimeId<Character>> guests, FeastPurpose purpose = FeastPurpose.OrdinarySocial,
        RuntimeId<Character>? arbiterBibendiId = null, ActivityVenueKind venueKind = ActivityVenueKind.VillaRoom,
        string venueKey = FeastCatalog.DefaultVenueKey, int quality = 60)
    {
        var result = PlanFeastCommands.Pipeline.Execute(
            world.State, PlanCommand(world, guests, purpose, arbiterBibendiId, venueKind, venueKey, quality));
        Assert.That(result.Accepted, Is.True, result.Error?.Code);
        return world.State.Activities.InAscendingOrder().Last().Key;
    }

    private static CommandResult Assign(World world, RuntimeId<Activity> activityId, RuntimeId<Character> guestId, FeastCouch couch, FeastCouchPosition position) =>
        AssignFeastSeatingCommands.Pipeline.Execute(
            world.State,
            new AssignFeastSeatingCommand(world.State.CommandIds.Issue(), "player", Date0, null, activityId, guestId, couch, position));

    private static MonthlyTickContext Context(int month)
    {
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.ActivityPhaseIncidentStreamName, Seed);
        streams.AddDerived(CampaignBootstrapper.ActivityNpcHostingStreamName, Seed);
        return new MonthlyTickContext(new GameDate(month), streams);
    }

    private static IReadOnlyList<IDomainEvent> Tick(WorldState state, int month)
    {
        var context = Context(month);
        var events = new List<IDomainEvent>(new ActivityProgressSystem().Tick(state, context));
        events.AddRange(new FeastSeatingResolutionSystem().Tick(state, context));
        return events;
    }

    private static HostedActivity Get(World world, RuntimeId<Activity> id)
    {
        world.State.Activities.TryGet(id, out var activity);
        return activity!;
    }

    private static RuntimeId<Character>[] Guests(World world, int count, string prefix = "Guest")
    {
        var guests = new RuntimeId<Character>[count];
        for (var i = 0; i < count; i++)
            guests[i] = Adult(world.State, null, world.SettlementId, $"{prefix}{i}");
        return guests;
    }

    // ---- Planning ----------------------------------------------------------------------------------

    [Test]
    public void PlanningStoresPurposeAndTheFeastsOwnPhaseSequence()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var activityId = Plan(world, new[] { guest }, purpose: FeastPurpose.PatronageDinner);

        var activity = Get(world, activityId);
        var feast = FeastRecordResolver.Get(world.State, activityId);

        Assert.Multiple(() =>
        {
            Assert.That(activity.TypeKey, Is.EqualTo(FeastCatalog.FeastType.Key));
            Assert.That(activity.Phases.Select(p => p.PhaseKey), Is.EqualTo(FeastCatalog.FeastPhaseKeys.All));
            Assert.That(activity.Phases[1].PhaseKey, Is.EqualTo(ActivityPhaseKeys.MainEvent));
            Assert.That(feast, Is.Not.Null);
            Assert.That(feast!.Purpose, Is.EqualTo(FeastPurpose.PatronageDinner));
            Assert.That(feast.ArbiterBibendiId, Is.Null);
            Assert.That(feast.EntertainmentDescription, Is.Null);
        });
    }

    [Test]
    public void ArbiterBibendiMustBeTheHostOrAGuest()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var outsider = Adult(world.State, null, world.SettlementId, "Outsider");

        var rejected = PlanFeastCommands.Pipeline.Execute(world.State, PlanCommand(world, new[] { guest }, arbiterBibendiId: outsider));
        Assert.That(rejected.Error, Is.EqualTo(PlanFeastCommands.ArbiterBibendiNotPresent));

        var accepted = PlanFeastCommands.Pipeline.Execute(world.State, PlanCommand(world, new[] { guest }, arbiterBibendiId: guest));
        Assert.That(accepted.Accepted, Is.True, accepted.Error?.Code);
    }

    [Test]
    public void PlanningReusesTheGenericEnginesOwnValidationForEveryOrdinaryCheck()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");

        var mismatchedInputs = new PlanFeastCommand(
            world.State.CommandIds.Issue(), "player", Date0, null, world.HostId, world.HouseholdId, null,
            ActivityVenueKind.VillaRoom, FeastCatalog.DefaultVenueKey, world.SettlementId, world.HoldingId,
            new GameDate(1), new[] { guest }, Array.Empty<ActivityQualityInput>(), Money.Zero, FeastPurpose.OrdinarySocial, null, null);
        var mismatchResult = PlanFeastCommands.Pipeline.Execute(world.State, mismatchedInputs);
        Assert.That(mismatchResult.Error, Is.EqualTo(PlanActivityCommands.QualityInputsMismatch));

        var noGuests = PlanCommand(world, Array.Empty<RuntimeId<Character>>());
        Assert.That(PlanFeastCommands.Pipeline.Execute(world.State, noGuests).Error, Is.EqualTo(PlanActivityCommands.NoGuests));
    }

    // ---- Seating assignment -------------------------------------------------------------------------

    [Test]
    public void SeatingValidatesInvitationDuplicationAndSeatUniqueness()
    {
        var world = NewWorld();
        var guestA = Adult(world.State, null, world.SettlementId, "GuestA");
        var guestB = Adult(world.State, null, world.SettlementId, "GuestB");
        var outsider = Adult(world.State, null, world.SettlementId, "Outsider");
        var activityId = Plan(world, new[] { guestA, guestB });

        Assert.That(Assign(world, activityId, outsider, FeastCouch.Medius, FeastCouchPosition.Highest).Error,
            Is.EqualTo(AssignFeastSeatingCommands.GuestNotInvited));

        var first = Assign(world, activityId, guestA, FeastCouch.Medius, FeastCouchPosition.Highest);
        Assert.That(first.Accepted, Is.True, first.Error?.Code);
        Assert.That(FeastSeatingResolver.Get(world.State, activityId, guestA)!.IsLocusConsularis, Is.True);

        Assert.That(Assign(world, activityId, guestA, FeastCouch.Summus, FeastCouchPosition.Highest).Error,
            Is.EqualTo(AssignFeastSeatingCommands.DuplicateSeatForGuest));
        Assert.That(Assign(world, activityId, guestB, FeastCouch.Medius, FeastCouchPosition.Highest).Error,
            Is.EqualTo(AssignFeastSeatingCommands.SeatAlreadyTaken));
    }

    [Test]
    public void SeatingIsRejectedForAGenericGatheringOrOnceTheFeastLeavesPlanned()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");

        var gatheringResult = PlanActivityCommands.Pipeline.Execute(
            world.State,
            new PlanActivityCommand(
                world.State.CommandIds.Issue(), "player", Date0, null, world.HostId, world.HouseholdId, null,
                ActivityTypeCatalog.Gathering.Key, ActivityVenueKind.VillaRoom, FeastCatalog.DefaultVenueKey, world.SettlementId,
                world.HoldingId, new GameDate(1), 1, new[] { guest }, Inputs(60), Money.Zero));
        Assert.That(gatheringResult.Accepted, Is.True, gatheringResult.Error?.Code);
        var gatheringId = world.State.Activities.InAscendingOrder().Last().Key;
        Assert.That(Assign(world, gatheringId, guest, FeastCouch.Medius, FeastCouchPosition.Highest).Error,
            Is.EqualTo(AssignFeastSeatingCommands.NotAFeast));

        // A second host, since world.HostId is still busy hosting the still-Planned gathering above.
        var secondHost = Adult(world.State, world.HouseholdId, world.SettlementId, "SecondHost");
        var feastResult = PlanFeastCommands.Pipeline.Execute(
            world.State, PlanCommand(world, new[] { guest }) with { HostCharacterId = secondHost });
        Assert.That(feastResult.Accepted, Is.True, feastResult.Error?.Code);
        var feastId = world.State.Activities.InAscendingOrder().Last().Key;

        Tick(world.State, 1);
        Assert.That(Assign(world, feastId, guest, FeastCouch.Medius, FeastCouchPosition.Highest).Error,
            Is.EqualTo(AssignFeastSeatingCommands.FeastNotPlanned));
    }

    // ---- Seating judgment resolution (§4) -----------------------------------------------------------

    [Test]
    public void SeatingJudgmentAppliesInsultHonorAndEnvyConsequencesIndependently()
    {
        var world = NewWorld();
        var guestHigh = GuestWithStanding(world, "High", 100);
        var guestMid = GuestWithStanding(world, "Mid", 50);
        var guestLow = GuestWithStanding(world, "Low", 0);
        var activityId = Plan(world, new[] { guestHigh, guestMid, guestLow });

        // Deliberately misseated: the highest-standing guest gets the worst seat (a severe Insult), the
        // middle guest gets exactly their own expected seat (no consequence), and the lowest-standing
        // guest gets the seat of honor (a deliberate Honor, at the expense of whoever it displaced).
        Assign(world, activityId, guestHigh, FeastCouch.Imus, FeastCouchPosition.Lowest);
        Assign(world, activityId, guestMid, FeastCouch.Medius, FeastCouchPosition.Middle);
        Assign(world, activityId, guestLow, FeastCouch.Medius, FeastCouchPosition.Highest);

        var context = Context(1);
        new ActivityProgressSystem().Tick(world.State, context);
        var events = new FeastSeatingResolutionSystem().Tick(world.State, context);

        Assert.Multiple(() =>
        {
            Assert.That(FeastSeatingResolver.Get(world.State, activityId, guestHigh)!.Judgment, Is.EqualTo(FeastSeatingJudgment.UnderSeated));
            Assert.That(FeastSeatingResolver.Get(world.State, activityId, guestMid)!.Judgment, Is.EqualTo(FeastSeatingJudgment.AppropriatelySeated));
            Assert.That(FeastSeatingResolver.Get(world.State, activityId, guestLow)!.Judgment, Is.EqualTo(FeastSeatingJudgment.OverSeated));

            var underSeatedOpinion = events.OfType<RelationshipInteractionRecordedEvent>()
                .Single(e => e.CharacterId == guestHigh && e.TargetId == world.HostId);
            Assert.That(underSeatedOpinion.OpinionAfter - underSeatedOpinion.OpinionBefore, Is.EqualTo(-FeastCatalog.MaxSeatingOpinionMagnitude));

            var overSeatedOpinion = events.OfType<RelationshipInteractionRecordedEvent>()
                .Single(e => e.CharacterId == guestLow && e.TargetId == world.HostId);
            Assert.That(overSeatedOpinion.OpinionAfter - overSeatedOpinion.OpinionBefore, Is.EqualTo(FeastCatalog.OverSeatingOpinionBonusPerRank));

            var envyOpinion = events.OfType<RelationshipInteractionRecordedEvent>()
                .Single(e => e.CharacterId == guestHigh && e.TargetId == guestLow);
            Assert.That(envyOpinion.OpinionAfter - envyOpinion.OpinionBefore, Is.EqualTo(-FeastCatalog.OverSeatingEnvyOpinionPenaltyPerRank));

            var underSeatedDignitas = events.OfType<DignitasChangedEvent>()
                .Single(e => e.HouseholdId == world.HouseholdId && e.Reason.Contains("below"));
            Assert.That(underSeatedDignitas.NewDignitas - underSeatedDignitas.PreviousDignitas, Is.EqualTo(-FeastCatalog.MaxSeatingDignitasMagnitude));

            var overSeatedDignitas = events.OfType<DignitasChangedEvent>()
                .Single(e => e.HouseholdId == world.HouseholdId && e.Reason.Contains("above"));
            Assert.That(overSeatedDignitas.NewDignitas - overSeatedDignitas.PreviousDignitas, Is.EqualTo(FeastCatalog.OverSeatingDignitasBonusPerRank));

            // The severe Insult (top-tier expected guest, seated far below it) escalates into a real Scandal.
            var scandal = events.OfType<ScandalRecordedEvent>().Single();
            Assert.That(scandal.SourceType, Is.EqualTo(ScandalSourceType.FeastSeatingInsult));
            Assert.That(scandal.PrimaryHouseholdId, Is.EqualTo(world.HouseholdId));
        });
    }

    [Test]
    public void ResolvingSeatingTwiceInTheSameMonthAppliesConsequencesOnlyOnce()
    {
        var world = NewWorld();
        var guest = GuestWithStanding(world, "Guest", 10);
        var activityId = Plan(world, new[] { guest });
        Assign(world, activityId, guest, FeastCouch.Imus, FeastCouchPosition.Lowest);

        var context = Context(1);
        new ActivityProgressSystem().Tick(world.State, context);
        var first = new FeastSeatingResolutionSystem().Tick(world.State, context);
        var second = new FeastSeatingResolutionSystem().Tick(world.State, context);

        Assert.That(first.OfType<RelationshipInteractionRecordedEvent>(), Is.Not.Empty);
        Assert.That(second, Is.Empty);
    }

    // ---- Disruption targeting (regression for the mainEvent key reuse) ------------------------------

    [Test]
    public void ATroublemakersDisruptionLandsOnTheMealPhaseNotTheFallbackMidpoint()
    {
        var world = NewWorld();
        var nemesis = Adult(world.State, null, world.SettlementId, "Nemesis");
        Tie(world.State, nemesis, world.HostId, -80, BondTag.Nemesis);
        var activityId = Plan(world, new[] { nemesis });
        Tick(world.State, 1);

        var activity = Get(world, activityId);
        var mealPhase = activity.Phases.Single(p => p.PhaseKey == FeastCatalog.FeastPhaseKeys.Meal);
        var comissatioPhase = activity.Phases.Single(p => p.PhaseKey == FeastCatalog.FeastPhaseKeys.Comissatio);

        Assert.Multiple(() =>
        {
            Assert.That(mealPhase.Moments.Any(m => m.Kind == ActivityMomentKind.Disruption), Is.True);
            Assert.That(comissatioPhase.Moments.Any(m => m.Kind == ActivityMomentKind.Disruption), Is.False);
        });
    }

    // ---- Comissatio-phase interactions (§3, §6.1 — zero new code needed) ----------------------------

    [Test]
    public void PerformActivityInteractionAlreadyWorksAgainstTheFeastsOwnComissatioPhase()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var activityId = Plan(world, new[] { guest });

        var result = PerformActivityInteractionCommands.Pipeline.Execute(
            world.State,
            new PerformActivityInteractionCommand(
                world.State.CommandIds.Issue(), "player", Date0, null, activityId, world.HostId, guest,
                FeastCatalog.FeastPhaseKeys.Comissatio, 10, BondTag.Friend));
        Assert.That(result.Accepted, Is.True, result.Error?.Code);
    }

    // ---- Chronicle (zero projector changes needed) --------------------------------------------------

    [Test]
    public void AConcludedFeastProjectsToTheChronicleThroughTheGenericActivityPath()
    {
        var world = NewWorld();
        Plan(world, Guests(world, 2), quality: 100, venueKind: ActivityVenueKind.Outdoor, venueKey: "viridarium");
        var events = Tick(world.State, 1);

        var draft = ChronicleProjector.Project(world.State, events).Single(d => d.SourceSystem == "activities.concluded");
        Assert.That(draft.HouseholdId, Is.EqualTo(world.HouseholdId));
    }

    // ---- Save round trip / determinism ---------------------------------------------------------------

    [Test]
    public void FeastStateRoundTripsThroughTheDtoAndHashStaysStable()
    {
        var world = NewWorld();
        var guestA = GuestWithStanding(world, "GuestA", 50);
        var guestB = GuestWithStanding(world, "GuestB", 0);
        var activityId = Plan(world, new[] { guestA, guestB }, purpose: FeastPurpose.PatronageDinner, arbiterBibendiId: guestA);
        Assign(world, activityId, guestA, FeastCouch.Medius, FeastCouchPosition.Highest);
        Assign(world, activityId, guestB, FeastCouch.Imus, FeastCouchPosition.Lowest);
        Tick(world.State, 1);

        var beforeHash = StateHasher.Hash(world.State);
        var restored = WorldStateMapper.ToWorldState(WorldStateMapper.ToDto(world.State));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        Assert.That(restored.FeastRecords.Count, Is.EqualTo(world.State.FeastRecords.Count));
        Assert.That(restored.FeastSeatingAssignments.Count, Is.EqualTo(world.State.FeastSeatingAssignments.Count));

        var restoredFeast = FeastRecordResolver.Get(restored, activityId);
        Assert.Multiple(() =>
        {
            Assert.That(restoredFeast, Is.Not.Null);
            Assert.That(restoredFeast!.Purpose, Is.EqualTo(FeastPurpose.PatronageDinner));
            Assert.That(restoredFeast.ArbiterBibendiId, Is.EqualTo(guestA));
            Assert.That(FeastSeatingResolver.Get(restored, activityId, guestA)!.Judgment, Is.Not.Null);
        });
    }

    [Test]
    public void ChangingASeatingJudgmentChangesTheStateHash()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var activityId = Plan(world, new[] { guest });
        Assign(world, activityId, guest, FeastCouch.Medius, FeastCouchPosition.Highest);

        var before = StateHasher.Hash(world.State);
        FeastSeatingResolver.Replace(
            world.State, FeastSeatingResolver.Get(world.State, activityId, guest)! with { Judgment = FeastSeatingJudgment.OverSeated });
        Assert.That(StateHasher.Hash(world.State), Is.Not.EqualTo(before));
    }

    [Test]
    public void TheSameSeedAndCommandsProduceTheSameHash()
    {
        ulong Run()
        {
            var world = NewWorld();
            var guestA = GuestWithStanding(world, "GuestA", 80);
            var guestB = GuestWithStanding(world, "GuestB", 10);
            var nemesis = Adult(world.State, null, world.SettlementId, "Nemesis");
            Tie(world.State, nemesis, world.HostId, -80, BondTag.Nemesis);
            var activityId = Plan(world, new[] { guestA, guestB, nemesis });

            Assign(world, activityId, guestA, FeastCouch.Imus, FeastCouchPosition.Lowest);
            Assign(world, activityId, guestB, FeastCouch.Medius, FeastCouchPosition.Highest);

            Tick(world.State, 1);
            return StateHasher.Hash(world.State);
        }

        Assert.That(Run(), Is.EqualTo(Run()));
    }
}
