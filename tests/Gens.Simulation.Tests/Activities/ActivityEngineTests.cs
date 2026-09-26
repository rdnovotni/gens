using Gens.Simulation.Activities;
using Gens.Simulation.Actors;
using Gens.Simulation.Buildings;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Companions;
using Gens.Simulation.Cultures;
using Gens.Simulation.Economy;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using Gens.Simulation.Villas;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Activities;

/// <summary>Phase 17 item 4 coverage: the generic Activity Engine — planning validation, Scale and
/// Quality, invitations/RSVP, §4.2 exclusion snubs, Phases and in-Phase Interactions, Quick and
/// Extended durations, interruption, the Witness Pool (and its Scheme discovery consumer), resolution
/// and the Activity Record, NPC hosting, Chronicle wiring, save round-trip, and state-hash stability.</summary>
public sealed class ActivityEngineTests
{
    private static readonly GameDate Date0 = new(0);
    private const ulong Seed = 17_4UL;

    // ---- Fixtures --------------------------------------------------------------------------------

    private sealed record World(
        WorldState State,
        RuntimeId<Household> HouseholdId,
        RuntimeId<Settlement> SettlementId,
        RuntimeId<Holding> HoldingId,
        RuntimeId<Character> HostId);

    private static World NewWorld(int tricliniumTier = 1)
    {
        var state = new WorldState(Date0);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();

        var villa = new Villa(VillaStage.Domus);
        villa.AddRoom(new VillaRoomInstance(new VillaRoomDefinition("triclinium", VillaStage.Rustica, maximumTier: 3), tier: tricliniumTier));
        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId, villa: villa));

        var hostId = Adult(state, householdId, settlementId, "Host");
        return new World(state, householdId, settlementId, holdingId, hostId);
    }

    private static RuntimeId<Character> Adult(
        WorldState state, RuntimeId<Household>? householdId, RuntimeId<Settlement> settlementId, string nomen,
        string culture = "roman", Sex sex = Sex.Male)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, nomen: nomen, household: householdId, location: settlementId, sex: sex,
                birthDate: new GameDate(-30 * 12)) with
            { Culture = new DefinitionId<Culture>(culture) });
        return id;
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

    private static PlanActivityCommand PlanCommand(
        World world,
        IReadOnlyList<RuntimeId<Character>> guests,
        string typeKey = "gathering",
        ActivityVenueKind venueKind = ActivityVenueKind.VillaRoom,
        string venueKey = "triclinium",
        GameDate? startDate = null,
        int durationMonths = 1,
        int quality = 60,
        Money? budget = null) =>
        new(
            world.State.CommandIds.Issue(), "player", Date0, null, world.HostId, world.HouseholdId, null, typeKey,
            venueKind, venueKey, world.SettlementId, venueKind == ActivityVenueKind.VillaRoom ? world.HoldingId : null,
            startDate ?? new GameDate(1), durationMonths, guests, Inputs(quality), budget ?? Money.Zero);

    private static RuntimeId<Activity> Plan(World world, IReadOnlyList<RuntimeId<Character>> guests, int quality = 60,
        ActivityVenueKind venueKind = ActivityVenueKind.VillaRoom, string venueKey = "triclinium", string typeKey = "gathering",
        int durationMonths = 1, Money? budget = null)
    {
        var result = PlanActivityCommands.Pipeline.Execute(
            world.State, PlanCommand(world, guests, typeKey, venueKind, venueKey, durationMonths: durationMonths, quality: quality, budget: budget));
        Assert.That(result.Accepted, Is.True, result.Error?.Code);
        return world.State.Activities.InAscendingOrder().Last().Key;
    }

    private static MonthlyTickContext Context(int month, RandomStreamSet? streams = null)
    {
        if (streams is null)
        {
            streams = new RandomStreamSet();
            streams.AddDerived(CampaignBootstrapper.ActivityPhaseIncidentStreamName, Seed);
            streams.AddDerived(CampaignBootstrapper.ActivityNpcHostingStreamName, Seed);
        }

        return new MonthlyTickContext(new GameDate(month), streams);
    }

    private static IReadOnlyList<IDomainEvent> Tick(WorldState state, int month) =>
        new ActivityProgressSystem().Tick(state, Context(month));

    private static HostedActivity Get(World world, RuntimeId<Activity> id)
    {
        world.State.Activities.TryGet(id, out var activity);
        return activity!;
    }

    private static ActivityInvitation Invitation(World world, RuntimeId<Activity> id, RuntimeId<Character> inviteeId) =>
        ActivityInvitationResolver.Get(world.State, id, inviteeId)!;

    private static RuntimeId<Character>[] Guests(World world, int count, string prefix = "Guest")
    {
        var guests = new RuntimeId<Character>[count];
        for (var i = 0; i < count; i++)
            guests[i] = Adult(world.State, null, world.SettlementId, $"{prefix}{i}");
        return guests;
    }

    // ---- Planning validation ---------------------------------------------------------------------

    [Test]
    public void PlanningRejectsEveryInvalidShape()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var dead = Adult(world.State, null, world.SettlementId, "Dead");
        world.State.Characters.TryGet(dead, out var deadCharacter);
        world.State.Characters.Remove(dead);
        world.State.Characters.Add(dead, deadCharacter! with { DeathRecord = new DeathRecord(Date0, DeathCause.Disease, 30) });

        ValidationErrorCode? Error(PlanActivityCommand command) => PlanActivityCommands.Pipeline.Execute(world.State, command).Error;

        Assert.Multiple(() =>
        {
            Assert.That(Error(PlanCommand(world, new[] { guest }, typeKey: "nonesuch")), Is.EqualTo(PlanActivityCommands.UnknownType));
            Assert.That(Error(PlanCommand(world, new[] { guest }) with { HostActorId = world.State.ActorIds.Issue() }),
                Is.EqualTo(PlanActivityCommands.HostOwnerAmbiguous));
            Assert.That(Error(PlanCommand(world, new[] { guest }) with { HostHouseholdId = world.State.HouseholdIds.Issue() }),
                Is.EqualTo(PlanActivityCommands.HostNotHouseholdMember));
            Assert.That(Error(PlanCommand(world, Array.Empty<RuntimeId<Character>>())), Is.EqualTo(PlanActivityCommands.NoGuests));
            Assert.That(Error(PlanCommand(world, new[] { world.HostId })), Is.EqualTo(PlanActivityCommands.HostOnGuestList));
            Assert.That(Error(PlanCommand(world, new[] { guest, guest })), Is.EqualTo(PlanActivityCommands.DuplicateGuest));
            Assert.That(Error(PlanCommand(world, new[] { dead })), Is.EqualTo(PlanActivityCommands.GuestDeceased));
            Assert.That(Error(PlanCommand(world, new[] { guest }, venueKey: "andron")), Is.EqualTo(PlanActivityCommands.VillaRoomNotFound));
            Assert.That(Error(PlanCommand(world, new[] { guest }) with { HoldingId = null }), Is.EqualTo(PlanActivityCommands.HoldingRequired));
            Assert.That(Error(PlanCommand(world, new[] { guest }, durationMonths: 2)), Is.EqualTo(PlanActivityCommands.InvalidDuration));
            Assert.That(Error(PlanCommand(world, new[] { guest }, typeKey: "extendedGathering", durationMonths: 1)),
                Is.EqualTo(PlanActivityCommands.InvalidDuration));
            Assert.That(Error(PlanCommand(world, new[] { guest }, startDate: new GameDate(-1))), Is.EqualTo(PlanActivityCommands.StartDateInPast));
            Assert.That(Error(PlanCommand(world, new[] { guest }) with { QualityInputs = Inputs(50).Take(2).ToArray() }),
                Is.EqualTo(PlanActivityCommands.QualityInputsMismatch));
            Assert.That(Error(PlanCommand(world, new[] { guest }, quality: 101)), Is.EqualTo(PlanActivityCommands.QualityInputOutOfRange));
            Assert.That(Error(PlanCommand(world, new[] { guest }, budget: Money.FromDenarii(-1))), Is.EqualTo(PlanActivityCommands.InvalidBudget));
        });

        Plan(world, new[] { guest });
        Assert.That(Error(PlanCommand(world, new[] { guest })), Is.EqualTo(PlanActivityCommands.HostAlreadyHosting));
    }

    [Test]
    public void ScaleIsDerivedFromGuestCountAndVenueNeverRolled()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ActivityCatalog.DeriveScale(5, ActivityVenueKind.VillaRoom, "peristylium"), Is.EqualTo(ActivityScaleTier.Intimate));
            Assert.That(ActivityCatalog.DeriveScale(6, ActivityVenueKind.VillaRoom, "triclinium"), Is.EqualTo(ActivityScaleTier.Modest));
            Assert.That(ActivityCatalog.DeriveScale(13, ActivityVenueKind.Outdoor, "saltus"), Is.EqualTo(ActivityScaleTier.Grand));
            Assert.That(ActivityCatalog.DeriveScale(31, ActivityVenueKind.VillaRoom, "triclinium"), Is.EqualTo(ActivityScaleTier.Lavish));
            Assert.That(ActivityCatalog.DeriveScale(2, ActivityVenueKind.CivicSpace, "forum"), Is.EqualTo(ActivityScaleTier.Grand));
            Assert.That(ActivityCatalog.DeriveScale(2, ActivityVenueKind.CivicSpace, "circus"), Is.EqualTo(ActivityScaleTier.Lavish));
        });
    }

    [Test]
    public void PlanningRecordsVenueTierPhaseScheduleAndPendingInvitations()
    {
        var world = NewWorld(tricliniumTier: 3);
        var guests = Guests(world, 2);
        var id = Plan(world, guests);
        var activity = Get(world, id);

        Assert.Multiple(() =>
        {
            Assert.That(activity.Venue.Tier, Is.EqualTo(3));
            Assert.That(activity.Scale, Is.EqualTo(ActivityScaleTier.Intimate));
            Assert.That(activity.Status, Is.EqualTo(ActivityStatus.Planned));
            Assert.That(activity.Phases.Select(p => p.PhaseKey), Is.EqualTo(ActivityPhaseKeys.Default));
            Assert.That(activity.Phases.All(p => p.ScheduledDate == new GameDate(1)), Is.True);
            Assert.That(ActivityInvitationResolver.GuestList(world.State, id).Select(i => i.RsvpStatus),
                Is.All.EqualTo(ActivityRsvpStatus.Pending));
        });
    }

    // ---- RSVP (§4.1) ----------------------------------------------------------------------------

    [Test]
    public void UnansweredInvitationsResolveFromOpinionBondsCultureAndAvailability()
    {
        var world = NewWorld();
        var friend = Adult(world.State, null, world.SettlementId, "Friend");
        var rival = Adult(world.State, null, world.SettlementId, "Rival");
        var nemesis = Adult(world.State, null, world.SettlementId, "Nemesis");
        var stranger = Adult(world.State, null, world.SettlementId, "Stranger");
        var hellene = Adult(world.State, null, world.SettlementId, "Hellene", culture: "greek");
        var traveller = Adult(world.State, null, world.SettlementId, "Traveller");
        StartTrip(world.State, traveller, TravelTripStatus.Traveling);

        Tie(world.State, friend, world.HostId, -10, BondTag.Friend);
        Tie(world.State, rival, world.HostId, 5, BondTag.Rival);
        Tie(world.State, nemesis, world.HostId, -90, BondTag.Nemesis);
        Tie(world.State, hellene, world.HostId, 5);

        var id = Plan(world, new[] { friend, rival, nemesis, stranger, hellene, traveller });
        Tick(world.State, 1);

        Assert.Multiple(() =>
        {
            Assert.That(Invitation(world, id, friend).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Accepted));
            Assert.That(Invitation(world, id, rival).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
            Assert.That(Invitation(world, id, nemesis).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Accepted));
            Assert.That(Invitation(world, id, nemesis).AttendsToCauseTrouble, Is.True);
            Assert.That(Invitation(world, id, stranger).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Accepted));
            Assert.That(Invitation(world, id, hellene).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
            Assert.That(Invitation(world, id, traveller).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
            Assert.That(Invitation(world, id, friend).RespondedExplicitly, Is.False);
        });
    }

    /// <summary>Puts <paramref name="characterId"/> on an active trip. <see cref="BeginTravelCommand"/>
    /// leaves <see cref="Character.CurrentTravelLocation"/> null while a leg is underway, so only an
    /// Arrived trip carries a location — set here to <paramref name="arrivedAt"/>.</summary>
    private static void StartTrip(
        WorldState state, RuntimeId<Character> characterId, TravelTripStatus status, RuntimeId<Settlement>? arrivedAt = null)
    {
        var tripId = state.TravelTripIds.Issue();
        state.TravelTrips.Add(tripId, TravelTrip.Restore(
            tripId, TravelParty.Create(characterId), TravelLocation.Rome(), TravelLocation.Rome(), DistanceTier.Near,
            RouteRiskLevel.Secure, 1, 0, Date0, status, false));
        if (arrivedAt is { } settlementId)
        {
            state.Characters.TryGet(characterId, out var character);
            state.Characters.Remove(characterId);
            state.Characters.Add(characterId, character! with { CurrentTravelLocation = TravelLocation.Home(settlementId) });
        }
    }

    [Test]
    public void OnlyATravellerWhoHasArrivedAtTheVenuesSettlementCanAttend()
    {
        var world = NewWorld();
        var inTransit = Adult(world.State, null, world.SettlementId, "InTransit");
        var arrivedHere = Adult(world.State, null, world.SettlementId, "ArrivedHere");
        var arrivedElsewhere = Adult(world.State, null, world.SettlementId, "ArrivedElsewhere");
        var otherSettlementId = world.State.SettlementIds.Issue();
        world.State.Settlements.Add(otherSettlementId, Settlement.Create(otherSettlementId, world.State.Regions.InAscendingOrder().First().Key));
        StartTrip(world.State, inTransit, TravelTripStatus.Traveling);
        StartTrip(world.State, arrivedHere, TravelTripStatus.Arrived, world.SettlementId);
        StartTrip(world.State, arrivedElsewhere, TravelTripStatus.Arrived, otherSettlementId);

        var id = Plan(world, new[] { inTransit, arrivedHere, arrivedElsewhere });
        Tick(world.State, 1);

        Assert.Multiple(() =>
        {
            Assert.That(Invitation(world, id, inTransit).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
            Assert.That(Invitation(world, id, arrivedHere).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Accepted));
            Assert.That(Invitation(world, id, arrivedElsewhere).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
        });
    }

    [Test]
    public void AGuestWhoLeavesOnATripDropsOutOfTheWitnessPool()
    {
        var world = NewWorld();
        var guests = Guests(world, 2);
        var id = Plan(world, guests, typeKey: "extendedGathering", durationMonths: 3);
        Tick(world.State, 1);
        Assert.That(ActivityWitnessPool.Of(world.State, Get(world, id)), Does.Contain(guests[0]));

        StartTrip(world.State, guests[0], TravelTripStatus.Traveling);
        Assert.That(ActivityWitnessPool.Of(world.State, Get(world, id)), Does.Not.Contain(guests[0]));
    }

    [Test]
    public void AnExplicitResponseIsHonouredAndAnExplicitDeclineCostsTheHostsOpinion()
    {
        var world = NewWorld();
        var eager = Adult(world.State, null, world.SettlementId, "Eager");
        var refuser = Adult(world.State, null, world.SettlementId, "Refuser");
        Tie(world.State, eager, world.HostId, -60);
        var id = Plan(world, new[] { eager, refuser });

        Respond(world, id, eager, accept: true);
        Respond(world, id, refuser, accept: false);
        Assert.That(
            RespondToActivityInvitationCommands.Pipeline.Execute(world.State,
                new RespondToActivityInvitationCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, refuser, true)).Error,
            Is.EqualTo(RespondToActivityInvitationCommands.AlreadyAnswered));
        Assert.That(
            RespondToActivityInvitationCommands.Pipeline.Execute(world.State,
                new RespondToActivityInvitationCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, world.HostId, true)).Error,
            Is.EqualTo(RespondToActivityInvitationCommands.NotInvited));

        Tick(world.State, 1);

        Assert.Multiple(() =>
        {
            Assert.That(Invitation(world, id, eager).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Accepted));
            Assert.That(Invitation(world, id, eager).RespondedExplicitly, Is.True);
            Assert.That(Invitation(world, id, refuser).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Declined));
            Assert.That(Opinion(world.State, world.HostId, refuser), Is.EqualTo(RespondToActivityInvitationCommands.ExplicitDeclineOpinionDelta));
        });
    }

    private static void Respond(World world, RuntimeId<Activity> id, RuntimeId<Character> invitee, bool accept) =>
        Assert.That(RespondToActivityInvitationCommands.Pipeline.Execute(world.State,
            new RespondToActivityInvitationCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, invitee, accept)).Accepted);

    // ---- Exclusion (§4.2) -------------------------------------------------------------------------

    [Test]
    public void LeavingAnExpectedGuestOffAModestGatheringIsAnInsultEquivalentSnub()
    {
        var world = NewWorld();
        var sibling = Adult(world.State, world.HouseholdId, world.SettlementId, "Sibling");
        var patron = Adult(world.State, null, world.SettlementId, "Patron");
        var acquaintance = Adult(world.State, null, world.SettlementId, "Acquaintance");
        Tie(world.State, world.HostId, patron, 30, BondTag.Client);
        Tie(world.State, world.HostId, acquaintance, 30);

        var id = Plan(world, Guests(world, 6));
        var events = Tick(world.State, 1);

        var snubs = events.OfType<ActivityExclusionSnubEvent>().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(Get(world, id).Scale, Is.EqualTo(ActivityScaleTier.Modest));
            Assert.That(snubs.Select(e => e.ExcludedCharacterId), Is.EquivalentTo(new[] { sibling, patron }));
            Assert.That(snubs.All(e => e.Visibility != Visibility.Public), Is.True);
            Assert.That(Invitation(world, id, sibling).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.NotInvited));
            Assert.That(Invitation(world, id, sibling).WasExpectedInvite, Is.True);
            Assert.That(Invitation(world, id, sibling).ExclusionInsultApplied, Is.True);
            Assert.That(Opinion(world.State, sibling, world.HostId), Is.EqualTo(-ActivityCatalog.ExclusionOpinionPenalty(ActivityScaleTier.Modest)));
            Assert.That(ActivityInvitationResolver.Get(world.State, id, acquaintance), Is.Null);
        });
    }

    [Test]
    public void AnIntimateGatheringSnubsNoOneAndAGrandOneSnubsPublicly()
    {
        var intimate = NewWorld();
        Adult(intimate.State, intimate.HouseholdId, intimate.SettlementId, "Sibling");
        Plan(intimate, Guests(intimate, 2));
        Assert.That(Tick(intimate.State, 1).OfType<ActivityExclusionSnubEvent>(), Is.Empty);

        var grand = NewWorld();
        Adult(grand.State, grand.HouseholdId, grand.SettlementId, "Sibling");
        Plan(grand, Guests(grand, 2), venueKind: ActivityVenueKind.CivicSpace, venueKey: "forum");
        var snub = Tick(grand.State, 1).OfType<ActivityExclusionSnubEvent>().Single();
        Assert.That(snub.Visibility, Is.EqualTo(Visibility.Public));
    }

    [Test]
    public void CancellingBeforeTheStartMonthCostsNothingAndSnubsNoOne()
    {
        var world = NewWorld();
        var sibling = Adult(world.State, world.HouseholdId, world.SettlementId, "Sibling");
        var id = Plan(world, Guests(world, 6), budget: Money.FromDenarii(100));

        var cancel = CancelActivityCommands.Pipeline.Execute(world.State,
            new CancelActivityCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, "changed my mind"));
        Assert.That(cancel.Accepted, Is.True);
        var events = Tick(world.State, 1);

        Assert.Multiple(() =>
        {
            Assert.That(Get(world, id).Status, Is.EqualTo(ActivityStatus.Cancelled));
            Assert.That(events, Is.Empty);
            Assert.That(Opinion(world.State, sibling, world.HostId), Is.Zero);
            Assert.That(world.State.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(world.HouseholdId), out _), Is.False);
            Assert.That(CancelActivityCommands.Pipeline.Execute(world.State,
                new CancelActivityCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, "again")).Error,
                Is.EqualTo(CancelActivityCommands.ActivityNotPlanned));
        });
    }

    // ---- Quick resolution, Quality, Outcome (§5, §9) ---------------------------------------------

    [Test]
    public void AQuickActivityRunsEveryPhaseAndResolvesInItsOneMonth()
    {
        var world = NewWorld();
        var guests = Guests(world, 3);
        var id = Plan(world, guests, quality: 100, budget: Money.FromDenarii(250));
        var dignitasBefore = DignitasResolver.Current(world.State, world.HouseholdId);

        var events = Tick(world.State, 1);
        var activity = Get(world, id);

        Assert.Multiple(() =>
        {
            Assert.That(activity.Status, Is.EqualTo(ActivityStatus.Concluded));
            Assert.That(activity.Phases.All(p => p.OccurredDate == new GameDate(1)), Is.True);
            Assert.That(events.OfType<ActivityPhaseResolvedEvent>().Count(), Is.EqualTo(3));
            Assert.That(activity.Outcome!.QualityTier, Is.EqualTo(ActivityQualityTier.Legendary));
            Assert.That(activity.Outcome.AttendeeCount, Is.EqualTo(3));
            Assert.That(activity.Outcome.WitnessCount, Is.EqualTo(4));
            Assert.That(activity.Outcome.HostDignitasDelta, Is.EqualTo(ActivityCatalog.BaseHostDignitas(ActivityQualityTier.Legendary)));
            Assert.That(DignitasResolver.Current(world.State, world.HouseholdId) - dignitasBefore, Is.EqualTo(activity.Outcome.HostDignitasDelta));
            Assert.That(activity.Outcome.NarrativeSummary, Does.Contain("3 of 3 invited guests attended"));
            Assert.That(events.OfType<ActivityConcludedEvent>().Single().Quality, Is.EqualTo(ActivityQualityTier.Legendary));
            world.State.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(world.HouseholdId), out var account);
            Assert.That(account!.Balance, Is.EqualTo(-Money.FromDenarii(250)));
        });

        // Every guest's opinion of the host rose by the Legendary delta (plus whatever a Phase
        // incident between them and the host added) — never fell below it for a non-disputant.
        foreach (var guest in guests)
            Assert.That(Opinion(world.State, guest, world.HostId), Is.GreaterThanOrEqualTo(ActivityCatalog.GuestOpinionDelta(ActivityQualityTier.Legendary) + ActivityPhaseRunner.Amplify(ActivityScaleTier.Intimate, ActivityCatalog.DisputeOpinionDelta)));
    }

    [Test]
    public void AModestQualityLavishGatheringCostsTheHostMoreStandingThanAnIntimateOne()
    {
        var lavish = NewWorld();
        var lavishId = Plan(lavish, Guests(lavish, 2), quality: 0, venueKind: ActivityVenueKind.CivicSpace, venueKey: "circus");
        Tick(lavish.State, 1);

        var intimate = NewWorld();
        var intimateId = Plan(intimate, Guests(intimate, 2), quality: 0);
        Tick(intimate.State, 1);

        Assert.Multiple(() =>
        {
            Assert.That(Get(lavish, lavishId).Outcome!.QualityTier, Is.EqualTo(ActivityQualityTier.Modest));
            Assert.That(Get(lavish, lavishId).Outcome!.HostDignitasDelta, Is.EqualTo(-2 * 3));
            Assert.That(Get(intimate, intimateId).Outcome!.HostDignitasDelta, Is.EqualTo(-2));
        });
    }

    [Test]
    public void QualityReadsWeightedInputsVenueTierAndHostOperators()
    {
        var world = NewWorld(tricliniumTier: 3);
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var command = PlanCommand(world, new[] { guest }) with
        {
            QualityInputs = new[]
            {
                new ActivityQualityInput(ActivityTypeCatalog.ProvisioningInput, 60),
                new ActivityQualityInput(ActivityTypeCatalog.HospitalityInput, 40),
                new ActivityQualityInput(ActivityTypeCatalog.EntertainmentInput, 20),
            },
        };
        PlanActivityCommands.Pipeline.Execute(world.State, command);
        var activity = world.State.Activities.InAscendingOrder().Single().Value;

        // (60*2 + 40 + 20) / 4 = 45, + (3 - 1) * 5 venue tier = 55.
        Assert.That(ActivityOutcomeResolver.QualityScore(world.State, activity), Is.EqualTo(55));

        foreach (var title in new[] { SeniorPositionTitle.Symposiarch, SeniorPositionTitle.HeadCook, SeniorPositionTitle.MasterOfHospitality })
        {
            var recordId = world.State.SeniorPositionAssignmentIds.Issue();
            world.State.SeniorPositionAssignments.Add(recordId, new SeniorPositionAssignment(
                recordId, Adult(world.State, world.HouseholdId, world.SettlementId, title.ToString()), title, world.HouseholdId,
                SeniorPositionScope.Villa, null, null, Date0, null, null, null));
        }

        // Three operators, capped at +10.
        Assert.That(ActivityOutcomeResolver.QualityScore(world.State, activity), Is.EqualTo(65));

        var disrupted = activity with
        {
            Phases = activity.Phases.Select((p, i) => i == 1
                ? p with { Moments = new[] { new ActivityMoment(ActivityMomentKind.Disruption, guest, world.HostId, null, -10) } }
                : p).ToArray(),
        };
        Assert.That(ActivityOutcomeResolver.QualityScore(world.State, disrupted), Is.EqualTo(65 - ActivityCatalog.DisruptionQualityPenalty));
    }

    [Test]
    public void ATroublemakerDisruptsTheMainEvent()
    {
        var world = NewWorld();
        var nemesis = Adult(world.State, null, world.SettlementId, "Nemesis");
        Tie(world.State, nemesis, world.HostId, -80, BondTag.Nemesis);
        var id = Plan(world, new[] { nemesis });
        Tick(world.State, 1);

        var activity = Get(world, id);
        var mainEvent = activity.Phases[ActivityPhaseRunner.DisruptionPhaseIndex(activity)];
        var disruption = mainEvent.Moments.Single(m => m.Kind == ActivityMomentKind.Disruption);
        Assert.Multiple(() =>
        {
            Assert.That(mainEvent.PhaseKey, Is.EqualTo(ActivityPhaseKeys.MainEvent));
            Assert.That(disruption.PrimaryCharacterId, Is.EqualTo(nemesis));
            Assert.That(activity.Outcome!.NarrativeSummary, Does.Contain("1 disruption"));
            Assert.That(Opinion(world.State, world.HostId, nemesis), Is.LessThanOrEqualTo(ActivityCatalog.DisruptionOpinionDelta));
        });
    }

    // ---- In-Phase Interactions (§6.1) ------------------------------------------------------------

    [Test]
    public void APlannedInteractionRunsInItsNamedPhaseAmplifiedByScale()
    {
        var world = NewWorld();
        var guests = Guests(world, 2);
        var id = Plan(world, guests, venueKind: ActivityVenueKind.CivicSpace, venueKey: "forum");
        var result = PerformActivityInteractionCommands.Pipeline.Execute(world.State,
            new PerformActivityInteractionCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, world.HostId, guests[0],
                ActivityPhaseKeys.Reception, 10, BondTag.Friend));
        Assert.That(result.Accepted, Is.True);
        Assert.That(Get(world, id).PlannedInteractions, Has.Count.EqualTo(1));

        Tick(world.State, 1);
        var activity = Get(world, id);
        var interaction = activity.Phases[0].Moments.Single(m => m.Kind == ActivityMomentKind.Interaction);

        Assert.Multiple(() =>
        {
            Assert.That(activity.Scale, Is.EqualTo(ActivityScaleTier.Grand));
            Assert.That(interaction.Magnitude, Is.EqualTo(15));
            Assert.That(world.State.Relationships.TryGet(new RelationshipKey(guests[0], world.HostId), out var tie), Is.True);
            Assert.That(tie.HasBond(BondTag.Friend), Is.True);
        });
    }

    [Test]
    public void InPhaseInteractionsRejectNonParticipantsAndUnknownPhases()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var outsider = Adult(world.State, null, world.SettlementId, "Outsider");
        var id = Plan(world, new[] { guest });

        ValidationErrorCode? Error(RuntimeId<Character> initiator, RuntimeId<Character> target, string phase, int delta = 5) =>
            PerformActivityInteractionCommands.Pipeline.Execute(world.State,
                new PerformActivityInteractionCommand(world.State.CommandIds.Issue(), "player", Date0, null, id, initiator, target,
                    phase, delta, BondTag.None)).Error;

        Assert.Multiple(() =>
        {
            Assert.That(Error(outsider, guest, ActivityPhaseKeys.Reception), Is.EqualTo(PerformActivityInteractionCommands.InitiatorNotParticipant));
            Assert.That(Error(world.HostId, outsider, ActivityPhaseKeys.Reception), Is.EqualTo(PerformActivityInteractionCommands.TargetNotParticipant));
            Assert.That(Error(world.HostId, guest, "dessert"), Is.EqualTo(PerformActivityInteractionCommands.UnknownOrPastPhase));
            Assert.That(Error(world.HostId, world.HostId, ActivityPhaseKeys.Reception), Is.EqualTo(PerformActivityInteractionCommands.SelfInteraction));
            Assert.That(Error(world.HostId, guest, ActivityPhaseKeys.Reception, 0), Is.EqualTo(PerformActivityInteractionCommands.NothingToRecord));
        });

        Tick(world.State, 1);
        Assert.That(Error(world.HostId, guest, ActivityPhaseKeys.Reception), Is.EqualTo(PerformActivityInteractionCommands.ActivityNotOpen));
    }

    // ---- Extended activities and interruption (§3) -----------------------------------------------

    [Test]
    public void AnExtendedActivitySpreadsItsPhasesAndAcceptsInteractionsBetweenThem()
    {
        var world = NewWorld();
        var guests = Guests(world, 2);
        var id = Plan(world, guests, typeKey: "extendedGathering", durationMonths: 4);
        var activity = Get(world, id);
        Assert.That(activity.Phases.Select(p => p.ScheduledDate.TotalMonths), Is.EqualTo(new[] { 1, 2, 4 }));
        Assert.That(activity.EndDate, Is.EqualTo(new GameDate(4)));

        Tick(world.State, 1);
        Assert.That(Get(world, id).Status, Is.EqualTo(ActivityStatus.InProgress));
        Assert.That(Get(world, id).Phases.Count(p => p.HasOccurred), Is.EqualTo(1));

        var immediate = PerformActivityInteractionCommands.Pipeline.Execute(world.State,
            new PerformActivityInteractionCommand(world.State.CommandIds.Issue(), "player", new GameDate(1), null, id, guests[0], guests[1],
                "ignored", 10, BondTag.None));
        Assert.That(immediate.Accepted, Is.True);
        Assert.That(immediate.Events.OfType<ActivityInteractionSubmittedEvent>().Single().Applied, Is.True);
        Assert.That(Get(world, id).Phases[0].Moments.Any(m => m.Kind == ActivityMomentKind.Interaction), Is.True);

        Tick(world.State, 2);
        Tick(world.State, 3);
        Assert.That(Get(world, id).Status, Is.EqualTo(ActivityStatus.InProgress));
        Tick(world.State, 4);
        Assert.That(Get(world, id).Status, Is.EqualTo(ActivityStatus.Concluded));
        Assert.That(Get(world, id).Phases.Select(p => p.OccurredDate!.Value.TotalMonths), Is.EqualTo(new[] { 1, 2, 4 }));
    }

    [Test]
    public void ASevereDisasterAtTheVenueInterruptsAnExtendedActivityWithNoOutcome()
    {
        var world = NewWorld();
        var id = Plan(world, Guests(world, 2), typeKey: "extendedGathering", durationMonths: 3);
        Tick(world.State, 1);
        var dignitasBefore = DignitasResolver.Current(world.State, world.HouseholdId);

        var disasterId = world.State.DisasterEventIds.Issue();
        world.State.DisasterEvents.Add(disasterId, new DisasterEvent
        {
            Id = disasterId,
            SettlementId = world.SettlementId,
            OccurredDate = new GameDate(2),
            HazardType = HazardType.Fire,
            Severity = DisasterSeverity.Severe,
        });

        var events = Tick(world.State, 2);
        var activity = Get(world, id);
        Assert.Multiple(() =>
        {
            Assert.That(activity.Status, Is.EqualTo(ActivityStatus.Interrupted));
            Assert.That(activity.Outcome, Is.Null);
            Assert.That(activity.TerminationReason, Does.Contain("Fire"));
            Assert.That(events.OfType<ActivityEndedWithoutConclusionEvent>().Single().Status, Is.EqualTo(ActivityStatus.Interrupted));
            Assert.That(DignitasResolver.Current(world.State, world.HouseholdId), Is.EqualTo(dignitasBefore));
        });
    }

    [Test]
    public void TheHostsDeathCancelsAPlannedActivityAndInterruptsARunningOne()
    {
        var planned = NewWorld();
        var plannedId = Plan(planned, Guests(planned, 1));
        Kill(planned.State, planned.HostId);
        Tick(planned.State, 1);
        Assert.That(Get(planned, plannedId).Status, Is.EqualTo(ActivityStatus.Cancelled));

        var running = NewWorld();
        var runningId = Plan(running, Guests(running, 1), typeKey: "extendedGathering", durationMonths: 2);
        Tick(running.State, 1);
        Kill(running.State, running.HostId);
        Tick(running.State, 2);
        Assert.That(Get(running, runningId).Status, Is.EqualTo(ActivityStatus.Interrupted));
    }

    private static void Kill(WorldState state, RuntimeId<Character> id)
    {
        state.Characters.TryGet(id, out var character);
        state.Characters.Remove(id);
        state.Characters.Add(id, character! with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.Disease, 30) });
    }

    // ---- Witness Pool (§7) ------------------------------------------------------------------------

    [Test]
    public void TheWitnessPoolIsTheAttendingGuestListAndRaisesSchemeDiscoveryRisk()
    {
        var world = NewWorld();
        var guests = Guests(world, 13);
        var decliner = guests[12];
        Tie(world.State, decliner, world.HostId, -50);
        var id = Plan(world, guests, typeKey: "extendedGathering", durationMonths: 3);
        Tick(world.State, 1);

        var activity = Get(world, id);
        var witnesses = ActivityWitnessPool.Of(world.State, activity);
        Assert.That(activity.Scale, Is.EqualTo(ActivityScaleTier.Grand));
        Assert.That(witnesses, Has.Count.EqualTo(13));
        Assert.That(witnesses, Does.Not.Contain(decliner));

        var atGathering = AdvanceScheme(world.State, guests[0], guests[1]);
        var elsewhere = AdvanceScheme(world.State, guests[0], decliner);
        Assert.That(atGathering - elsewhere, Is.EqualTo(ActivityCatalog.SharedActivityDiscoveryRiskBonus(ActivityScaleTier.Grand)));
    }

    [Test]
    public void AQuickActivityConcludedThisMonthStillCountsAsWitnessesForSchemes()
    {
        var world = NewWorld();
        var guests = Guests(world, 2);
        var id = Plan(world, guests);
        Tick(world.State, 1);
        Assert.That(Get(world, id).Status, Is.EqualTo(ActivityStatus.Concluded));

        var atGathering = AdvanceScheme(world.State, guests[0], guests[1]);
        var outsider = Adult(world.State, null, world.SettlementId, "Outsider");
        var elsewhere = AdvanceScheme(world.State, guests[0], outsider);
        Assert.That(atGathering - elsewhere, Is.EqualTo(ActivityCatalog.SharedActivityDiscoveryRiskBonus(ActivityScaleTier.Intimate)));

        // A later month no longer counts.
        Assert.That(AdvanceScheme(world.State, guests[0], guests[1], month: 2), Is.EqualTo(elsewhere));
    }

    [Test]
    public void InOneMonthlyTickAQuickGatheringRaisesTheRiskOfASchemeBetweenItsGuests()
    {
        var world = NewWorld();
        var guests = Guests(world, 2);
        Plan(world, guests);
        var schemeId = world.State.SchemeIds.Issue();
        world.State.Schemes.Add(schemeId, new Scheme(schemeId, guests[0], guests[1], SchemeType.Coercive, SchemeStatus.InProgress, 0, 0, Date0, Date0));

        var simulation = new MonthlySimulation<WorldState>(new IMonthlySystem<WorldState>[] { new SchemeProgressSystem(), new ActivityProgressSystem() });
        Assert.That(simulation.OrderedSystems.Select(s => s.Id), Is.EqualTo(new[] { "activities.progress", "interactions.schemeProgress" }));

        var streams = Context(0).RandomStreams;
        streams.AddDerived(CampaignBootstrapper.SchemeProgressStreamName, Seed);
        simulation.Tick(world.State, new GameDate(1), streams);

        world.State.Schemes.TryGet(schemeId, out var scheme);
        Assert.That(scheme!.DiscoveryRisk, Is.EqualTo(
            SchemeProgressCatalog.BaseDiscoveryRiskPerMonthPercent + ActivityCatalog.SharedActivityDiscoveryRiskBonus(ActivityScaleTier.Intimate)));
    }

    private static int AdvanceScheme(WorldState state, RuntimeId<Character> initiator, RuntimeId<Character> target, int month = 1)
    {
        var schemeId = state.SchemeIds.Issue();
        state.Schemes.Add(schemeId, new Scheme(schemeId, initiator, target, SchemeType.Coercive, SchemeStatus.InProgress, 0, 0, Date0, Date0));
        var streams = new RandomStreamSet();
        streams.AddDerived(CampaignBootstrapper.SchemeProgressStreamName, Seed);
        new SchemeProgressSystem().Tick(state, new MonthlyTickContext(new GameDate(month), streams));
        state.Schemes.TryGet(schemeId, out var scheme);
        state.Schemes.Remove(schemeId);
        return scheme!.DiscoveryRisk;
    }

    // ---- NPC hosting (§8) -------------------------------------------------------------------------

    private static (World World, RuntimeId<Actor> ActorId, RuntimeId<Character> RivalHeadId) WorldWithRivalHouse()
    {
        var world = NewWorld();
        var headId = Adult(world.State, null, world.SettlementId, "RivalHead");
        var actorId = world.State.ActorIds.Issue();
        world.State.Actors.Add(actorId, LivingWorldActor.Create(
            actorId, LivingWorldActorType.Gens, "Gens Valeria", LivingWorldActorTier.Noteworthy,
            LivingWorldActorStandingTrend.Established, LivingWorldActorOrigin.Ancient, null, LivingWorldActorIdentity.None,
            10, new LivingWorldActorNetWorth(HouseholdWealthBand.Wealthy, null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), world.State.Regions.InAscendingOrder().First().Key,
            world.SettlementId, headCharacterId: headId));
        return (world, actorId, headId);
    }

    private static int FirstHostingMonth(WorldState state, RandomStreamSet streams, int maxMonths = 400)
    {
        for (var month = 1; month <= maxMonths; month++)
        {
            new NpcActivityHostingSystem().Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            if (state.Activities.Count > 0)
                return month;
        }

        return -1;
    }

    [Test]
    public void ARivalHouseConvenesItsOwnGatheringAndInvitesThePlayerItLikes()
    {
        var (world, actorId, rivalHeadId) = WorldWithRivalHouse();
        Tie(world.State, rivalHeadId, world.HostId, 40, BondTag.Friend);

        var streams = Context(0).RandomStreams;
        var month = FirstHostingMonth(world.State, streams);
        Assert.That(month, Is.GreaterThan(0), "a Noteworthy house should eventually convene a gathering");

        var (activityId, activity) = world.State.Activities.InAscendingOrder().Single();
        Assert.Multiple(() =>
        {
            Assert.That(activity.HostIsNpc, Is.True);
            Assert.That(activity.HostActorId, Is.EqualTo(actorId));
            Assert.That(activity.HostCharacterId, Is.EqualTo(rivalHeadId));
            Assert.That(activity.StartDate, Is.EqualTo(new GameDate(month + 1)));
            Assert.That(activity.QualityInputs.All(i => i.Score == ActivityCatalog.NpcQualityInputScore(HouseholdWealthBand.Wealthy)), Is.True);
            Assert.That(Invitation(world, activityId, world.HostId).RsvpStatus, Is.EqualTo(ActivityRsvpStatus.Pending));
        });

        // §8.1: the player accepts; the same engine runs the rival's Phases and resolves its Outcome.
        Respond(world, activityId, world.HostId, accept: true);
        var dignitasBefore = world.State.Actors.InAscendingOrder().Single().Value.Dignitas;
        new ActivityProgressSystem().Tick(world.State, new MonthlyTickContext(new GameDate(month + 1), streams));

        var concluded = Get(world, activityId);
        Assert.That(concluded.Status, Is.EqualTo(ActivityStatus.Concluded));
        Assert.That(ActivityWitnessPool.Of(world.State, concluded), Does.Contain(world.HostId));
        Assert.That(world.State.Actors.InAscendingOrder().Single().Value.Dignitas - dignitasBefore,
            Is.EqualTo(concluded.Outcome!.HostDignitasDelta));
    }

    [Test]
    public void ThePlayerTiedToARivalHeadButNotLikedIsPointedlyLeftOff()
    {
        var (world, _, rivalHeadId) = WorldWithRivalHouse();
        var favourites = Guests(world, 6, "Favourite");
        foreach (var favourite in favourites)
            Tie(world.State, rivalHeadId, favourite, 60);
        Tie(world.State, rivalHeadId, world.HostId, 5, BondTag.Client);

        var streams = Context(0).RandomStreams;
        var month = FirstHostingMonth(world.State, streams);
        var (activityId, _) = world.State.Activities.InAscendingOrder().Single();
        Assert.That(ActivityInvitationResolver.Get(world.State, activityId, world.HostId), Is.Null);

        var events = new ActivityProgressSystem().Tick(world.State, new MonthlyTickContext(new GameDate(month + 1), streams));
        Assert.Multiple(() =>
        {
            Assert.That(events.OfType<ActivityExclusionSnubEvent>().Select(e => e.ExcludedCharacterId), Does.Contain(world.HostId));
            Assert.That(Invitation(world, activityId, world.HostId).WasExpectedInvite, Is.True);
            Assert.That(Opinion(world.State, world.HostId, rivalHeadId), Is.LessThan(0));
        });
    }

    [Test]
    public void AFlirtationIncidentIsAmplifiedByScale()
    {
        // Find a seed whose first Phase-incident draws produce a Flirtation at a Lavish gathering between
        // two romance-eligible guests; the search is itself deterministic.
        for (ulong seed = 1; seed < 500; seed++)
        {
            var world = NewWorld();
            var man = Adult(world.State, null, world.SettlementId, "Man");
            var woman = Adult(world.State, null, world.SettlementId, "Woman", sex: Sex.Female);
            var id = Plan(world, new[] { man, woman }, venueKind: ActivityVenueKind.CivicSpace, venueKey: "circus");
            var streams = new RandomStreamSet();
            streams.AddDerived(CampaignBootstrapper.ActivityPhaseIncidentStreamName, seed);
            new ActivityProgressSystem().Tick(world.State, new MonthlyTickContext(new GameDate(1), streams));

            var flirtations = Get(world, id).Phases.SelectMany(p => p.Moments)
                .Where(m => m.IncidentKind == ActivityIncidentKind.Flirtation).ToArray();
            if (flirtations.Length != 1)
                continue;

            var flirtation = flirtations[0];
            var expectedAffection = ActivityPhaseRunner.Amplify(ActivityScaleTier.Lavish, ActivityCatalog.FlirtationAffectionDelta);
            Assert.That(flirtation.Magnitude, Is.EqualTo(expectedAffection));
            Assert.That(expectedAffection, Is.GreaterThan(ActivityCatalog.FlirtationAffectionDelta));
            world.State.RomanticBonds.TryGet(Gens.Simulation.Romance.RomanticBondKey.Create(flirtation.PrimaryCharacterId, flirtation.SecondaryCharacterId!.Value), out var bond);
            Assert.That(bond.Affection, Is.EqualTo(expectedAffection));
            Assert.That(bond.Attraction, Is.EqualTo(ActivityPhaseRunner.Amplify(ActivityScaleTier.Lavish, ActivityCatalog.FlirtationAttractionDelta)));
            return;
        }

        Assert.Fail("no seed produced exactly one Flirtation incident");
    }

    // ---- Chronicle ------------------------------------------------------------------------------

    [Test]
    public void AHouseholdHostedConclusionProjectsToTheChronicleTieredByBothAxes()
    {
        var world = NewWorld();
        Plan(world, Guests(world, 2), quality: 100, venueKind: ActivityVenueKind.CivicSpace, venueKey: "forum");
        var events = Tick(world.State, 1);

        var draft = ChronicleProjector.Project(world.State, events)
            .Single(d => d.SourceSystem == "activities.concluded");
        Assert.Multiple(() =>
        {
            Assert.That(draft.Tier, Is.EqualTo(ChronicleTier.Major));
            Assert.That(draft.HouseholdId, Is.EqualTo(world.HouseholdId));
            Assert.That(draft.Prose, Does.Contain("Grand gathering"));
        });
    }

    // ---- Save round trip / determinism ------------------------------------------------------------

    [Test]
    public void ActivityStateRoundTripsThroughTheDtoAndHashStaysStable()
    {
        var world = NewWorld(tricliniumTier: 2);
        var guests = Guests(world, 6);
        var nemesis = Adult(world.State, null, world.SettlementId, "Nemesis");
        Tie(world.State, nemesis, world.HostId, -80, BondTag.Nemesis);
        Adult(world.State, world.HouseholdId, world.SettlementId, "Sibling");
        var concluded = Plan(world, guests.Append(nemesis).ToArray(), budget: Money.FromDenarii(40));
        PerformActivityInteractionCommands.Pipeline.Execute(world.State,
            new PerformActivityInteractionCommand(world.State.CommandIds.Issue(), "player", Date0, null, concluded, world.HostId, guests[0],
                ActivityPhaseKeys.MainEvent, 7, BondTag.Friend));
        Tick(world.State, 1);

        var extendedHost = Adult(world.State, world.HouseholdId, world.SettlementId, "SecondHost");
        PlanActivityCommands.Pipeline.Execute(world.State, PlanCommand(world, new[] { guests[1] }, typeKey: "extendedGathering",
            startDate: new GameDate(2), durationMonths: 3) with
        { HostCharacterId = extendedHost });
        PerformActivityInteractionCommands.Pipeline.Execute(world.State,
            new PerformActivityInteractionCommand(world.State.CommandIds.Issue(), "player", Date0, null,
                world.State.Activities.InAscendingOrder().Last().Key, extendedHost, guests[1], ActivityPhaseKeys.Aftermath, -4, BondTag.None));

        Assert.That(Get(world, concluded).Outcome, Is.Not.Null);
        var beforeHash = StateHasher.Hash(world.State);
        var restored = WorldStateMapper.ToWorldState(WorldStateMapper.ToDto(world.State));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        Assert.That(restored.Activities.Count, Is.EqualTo(2));
        Assert.That(restored.ActivityInvitations.Count, Is.EqualTo(world.State.ActivityInvitations.Count));
    }

    [Test]
    public void ChangingOneInvitationFieldChangesTheStateHash()
    {
        var world = NewWorld();
        var guest = Adult(world.State, null, world.SettlementId, "Guest");
        var id = Plan(world, new[] { guest });
        var before = StateHasher.Hash(world.State);

        ActivityInvitationResolver.Replace(world.State, Invitation(world, id, guest) with { AttendsToCauseTrouble = true });
        Assert.That(StateHasher.Hash(world.State), Is.Not.EqualTo(before));
    }

    [Test]
    public void TheSameSeedAndCommandsProduceTheSameHash()
    {
        ulong Run()
        {
            var world = NewWorld();
            var guests = Guests(world, 8);
            Tie(world.State, guests[0], world.HostId, -80, BondTag.Nemesis);
            Plan(world, guests, typeKey: "extendedGathering", durationMonths: 3);
            var streams = Context(0).RandomStreams;
            for (var month = 1; month <= 3; month++)
                new ActivityProgressSystem().Tick(world.State, new MonthlyTickContext(new GameDate(month), streams));
            return StateHasher.Hash(world.State);
        }

        Assert.That(Run(), Is.EqualTo(Run()));
    }
}
