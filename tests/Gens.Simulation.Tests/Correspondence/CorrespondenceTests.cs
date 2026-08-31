using Gens.Simulation.Characters;
using Gens.Simulation.Correspondence;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.Regions;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Correspondence;

public sealed class CorrespondenceTests
{
    private static readonly GameDate StartDate = new(0);

    private static RandomStreamSet Streams(ulong seed = 1)
    {
        var streams = new RandomStreamSet();
        streams.Add(CorrespondenceTransitSystem.RiskStreamName, seed, 1);
        return streams;
    }

    // ---- LetterActions.IsSubstantive -------------------------------------------------------------

    [Test]
    public void SubstantiveActionsMatchTheFiveNegotiationAndDirectiveActions()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LetterActions.IsSubstantive(LetterAction.PetitionPatron), Is.True);
            Assert.That(LetterActions.IsSubstantive(LetterAction.RemoteNegotiation), Is.True);
            Assert.That(LetterActions.IsSubstantive(LetterAction.DirectPlacedSpy), Is.True);
            Assert.That(LetterActions.IsSubstantive(LetterAction.FormalComplaintOrProvocation), Is.True);
            Assert.That(LetterActions.IsSubstantive(LetterAction.WrittenInstructionsToDistantAppointee), Is.True);
            Assert.That(LetterActions.IsSubstantive(LetterAction.MaintainDistantRelationship), Is.False);
            Assert.That(LetterActions.IsSubstantive(LetterAction.EarlyCourtship), Is.False);
            Assert.That(LetterActions.IsSubstantive(LetterAction.NewsAndGossip), Is.False);
            Assert.That(LetterActions.IsSubstantive(LetterAction.CondolenceOrCongratulation), Is.False);
        });
    }

    // ---- CourierCatalog ---------------------------------------------------------------------------

    [Test]
    public void TabellariusMatchesTravelsOwnBaselineTransitTimes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CourierCatalog.ResolveTransitTimeMonths(CourierType.Tabellarius, DistanceTier.Near), Is.EqualTo(1));
            Assert.That(CourierCatalog.ResolveTransitTimeMonths(CourierType.Tabellarius, DistanceTier.Moderate), Is.EqualTo(3));
            Assert.That(CourierCatalog.ResolveTransitTimeMonths(CourierType.Tabellarius, DistanceTier.Far), Is.EqualTo(6));
        });
    }

    [Test]
    public void PigeonIsNeverSlowerThanTabellariusAndCarriesTheHighestRisk()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                CourierCatalog.ResolveTransitTimeMonths(CourierType.Pigeon, DistanceTier.Far),
                Is.LessThanOrEqualTo(CourierCatalog.ResolveTransitTimeMonths(CourierType.Tabellarius, DistanceTier.Far)));
            Assert.That(
                CourierCatalog.Resolve(CourierType.Pigeon).InterceptionRiskModifierPercent,
                Is.GreaterThan(CourierCatalog.Resolve(CourierType.HiredCarrier).InterceptionRiskModifierPercent));
            Assert.That(
                CourierCatalog.Resolve(CourierType.HiredCarrier).InterceptionRiskModifierPercent,
                Is.GreaterThan(CourierCatalog.Resolve(CourierType.Tabellarius).InterceptionRiskModifierPercent));
        });
    }

    // ---- CorrespondenceReachabilityCatalog ---------------------------------------------------------

    [Test]
    public void ResolveDefaultsToFullyLiterateForAnUnlistedCulture()
    {
        var catalog = CorrespondenceTestFixtures.BuildReachabilityCatalog();
        Assert.That(catalog.Resolve(CorrespondenceTestFixtures.LiterateCultureId), Is.EqualTo(CorrespondenceReachability.FullyLiterate));
    }

    [Test]
    public void ResolveDefaultsToFullyLiterateForANullCulture()
    {
        var catalog = CorrespondenceTestFixtures.BuildReachabilityCatalog();
        Assert.That(catalog.Resolve(null), Is.EqualTo(CorrespondenceReachability.FullyLiterate));
    }

    [Test]
    public void ResolveReadsAnAuthoredEntry()
    {
        var catalog = CorrespondenceTestFixtures.BuildReachabilityCatalog();
        Assert.Multiple(() =>
        {
            Assert.That(
                catalog.Resolve(CorrespondenceTestFixtures.OralTraditionPartialCultureId),
                Is.EqualTo(CorrespondenceReachability.OralTraditionPartial));
            Assert.That(
                catalog.Resolve(CorrespondenceTestFixtures.OralTraditionBlockedCultureId),
                Is.EqualTo(CorrespondenceReachability.OralTraditionBlocked));
        });
    }

    [Test]
    public void ConstructorRejectsADuplicateCultureEntry()
    {
        Assert.Throws<ArgumentException>(() => new CorrespondenceReachabilityCatalog(new[]
        {
            new CultureReachabilityEntry(CorrespondenceTestFixtures.OralTraditionPartialCultureId, CorrespondenceReachability.OralTraditionPartial),
            new CultureReachabilityEntry(CorrespondenceTestFixtures.OralTraditionPartialCultureId, CorrespondenceReachability.OralTraditionBlocked),
        }));
    }

    // ---- LetterRoute --------------------------------------------------------------------------------

    [Test]
    public void ResolveWithNoForeignCultureNeverAppliesAPenalty()
    {
        var route = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId, foreignCultureId: null,
            LetterAction.RemoteNegotiation, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceTier, Is.EqualTo(DistanceTier.Far));
            Assert.That(route.TransitTimeMonths, Is.EqualTo(6));
            Assert.That(route.InterceptionRisk, Is.EqualTo(RouteRiskLevel.Dangerous));
            Assert.That(route.OralTraditionPenaltyApplied, Is.False);
            Assert.That(route.Blocked, Is.False);
        });
    }

    [Test]
    public void ResolveAppliesNoPenaltyForARoutineActionEvenUnderAnOralTraditionBlockedCulture()
    {
        var route = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId,
            CorrespondenceTestFixtures.OralTraditionBlockedCultureId,
            LetterAction.NewsAndGossip, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.OralTraditionPenaltyApplied, Is.False);
            Assert.That(route.Blocked, Is.False);
            Assert.That(route.InterceptionRisk, Is.EqualTo(RouteRiskLevel.Secure));
        });
    }

    [Test]
    public void ResolveBumpsRiskUpOneStepForASubstantiveActionUnderPartialReachability()
    {
        var route = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId,
            CorrespondenceTestFixtures.OralTraditionPartialCultureId,
            LetterAction.RemoteNegotiation, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        Assert.Multiple(() =>
        {
            Assert.That(route.OralTraditionPenaltyApplied, Is.True);
            Assert.That(route.Blocked, Is.False);
            // Near tier's own base risk (Secure) bumped up one step to Guarded.
            Assert.That(route.InterceptionRisk, Is.EqualTo(RouteRiskLevel.Guarded));
        });
    }

    [Test]
    public void ResolveBlocksASubstantiveActionUnderOralTraditionBlockedReachability()
    {
        var route = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId,
            CorrespondenceTestFixtures.OralTraditionBlockedCultureId,
            LetterAction.PetitionPatron, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        Assert.That(route.Blocked, Is.True);
    }

    [Test]
    public void ResolveNeverBumpsRiskBeyondDangerous()
    {
        var route = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId,
            CorrespondenceTestFixtures.OralTraditionPartialCultureId,
            LetterAction.RemoteNegotiation, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        Assert.That(route.InterceptionRisk, Is.EqualTo(RouteRiskLevel.Dangerous));
    }

    // ---- Letter.Begin -------------------------------------------------------------------------------

    [Test]
    public void BeginRejectsABlockedRoute()
    {
        var blockedRoute = LetterRoute.Resolve(
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId,
            CorrespondenceTestFixtures.OralTraditionBlockedCultureId,
            LetterAction.PetitionPatron, CourierType.Tabellarius,
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        var counter = new RuntimeIdCounter<Character>();
        var draftedBy = counter.Issue();

        Assert.Throws<ArgumentException>(() => Letter.Begin(
            new RuntimeIdCounter<Letter>().Issue(), LetterDirection.Outbound, LetterAction.PetitionPatron,
            draftedBy.ToTaggedString(), "actor_0000001", draftedBy, blockedRoute, CourierType.Tabellarius,
            null, StartDate, requiresResponse: false));
    }

    [Test]
    public void BeginForcesRequiresResponseFalseForOutbound()
    {
        var route = OpenRoute();
        var counter = new RuntimeIdCounter<Character>();
        var draftedBy = counter.Issue();

        var letter = Letter.Begin(
            new RuntimeIdCounter<Letter>().Issue(), LetterDirection.Outbound, LetterAction.NewsAndGossip,
            draftedBy.ToTaggedString(), "actor_0000001", draftedBy, route, CourierType.Tabellarius,
            null, StartDate, requiresResponse: true);

        Assert.That(letter.RequiresResponse, Is.False);
    }

    [Test]
    public void BeginHonorsRequiresResponseForInbound()
    {
        var route = OpenRoute();

        var letter = Letter.Begin(
            new RuntimeIdCounter<Letter>().Issue(), LetterDirection.Inbound, LetterAction.PetitionPatron,
            "actor_0000001", "char_0000001", null, route, CourierType.Tabellarius,
            null, StartDate, requiresResponse: true);

        Assert.That(letter.RequiresResponse, Is.True);
    }

    private static LetterRoute OpenRoute() => LetterRoute.Resolve(
        CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId, foreignCultureId: null,
        LetterAction.NewsAndGossip, CourierType.Tabellarius,
        CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

    // ---- SendLetterCommand ----------------------------------------------------------------------------

    [Test]
    public void SendLetterCreatesALetterInTransit()
    {
        var (state, drafterId) = OneCharacterHousehold();
        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        var command = new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, "actor_0000042",
            LetterAction.PetitionPatron, CourierType.Tabellarius, null,
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId, null);
        var result = pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.Single(), Is.InstanceOf<LetterSentEvent>());
            Assert.That(state.Letters.Count, Is.EqualTo(1));
        });

        var letter = state.Letters.InAscendingOrder().Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(letter.Direction, Is.EqualTo(LetterDirection.Outbound));
            Assert.That(letter.Status, Is.EqualTo(LetterStatus.InTransit));
            Assert.That(letter.TransitTimeMonths, Is.EqualTo(6));
            Assert.That(letter.RequiresResponse, Is.False);
            Assert.That(letter.SenderCharacterOrActorId, Is.EqualTo(drafterId.ToTaggedString()));
            Assert.That(letter.RecipientCharacterOrActorId, Is.EqualTo("actor_0000042"));
        });
    }

    [Test]
    public void SendLetterRejectsADeceasedDrafter()
    {
        var (state, drafterId) = OneCharacterHousehold();
        state.Characters.TryGet(drafterId, out var drafter);
        state.Characters.Remove(drafterId);
        state.Characters.Add(drafterId, drafter with { DeathRecord = new DeathRecord(StartDate, DeathCause.OldAge, 40) });

        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        var command = new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, "actor_0000042",
            LetterAction.NewsAndGossip, CourierType.Tabellarius, null,
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId, null);
        var result = pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(SendLetterCommands.DrafterDeceased));
    }

    [Test]
    public void SendLetterRejectsAnEmptyRecipient()
    {
        var (state, drafterId) = OneCharacterHousehold();
        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        var command = new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, " ",
            LetterAction.NewsAndGossip, CourierType.Tabellarius, null,
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId, null);
        var result = pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(SendLetterCommands.RecipientRequired));
    }

    [Test]
    public void SendLetterRejectsASubstantiveActionBlockedByOralTradition()
    {
        var (state, drafterId) = OneCharacterHousehold();
        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        var command = new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, "actor_0000042",
            LetterAction.RemoteNegotiation, CourierType.Tabellarius, null,
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.FarRegionId,
            CorrespondenceTestFixtures.OralTraditionBlockedCultureId);
        var result = pipeline.Execute(state, command);

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(SendLetterCommands.OralTraditionBlocksThisAction));
            Assert.That(state.Letters.Count, Is.EqualTo(0));
        });
    }

    // ---- OriginateInboundLetterCommand ------------------------------------------------------------------

    [Test]
    public void OriginateInboundLetterCreatesAnInboundLetterRequiringAResponse()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var pipeline = OriginateInboundLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        var command = new OriginateInboundLetterCommand(
            state.CommandIds.Issue(), "system", StartDate, null, "actor_0000007", recipientId,
            LetterAction.PetitionPatron, CourierType.Tabellarius,
            CorrespondenceTestFixtures.FarRegionId, CorrespondenceTestFixtures.HomeRegionId, null, true);
        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var letter = state.Letters.InAscendingOrder().Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(letter.Direction, Is.EqualTo(LetterDirection.Inbound));
            Assert.That(letter.RequiresResponse, Is.True);
            Assert.That(letter.RecipientCharacterOrActorId, Is.EqualTo(recipientId.ToTaggedString()));
            Assert.That(letter.SenderCharacterOrActorId, Is.EqualTo("actor_0000007"));
            Assert.That(letter.DraftedByCharacterId, Is.Null);
        });
    }

    [Test]
    public void OriginateInboundLetterRejectsAMissingRecipient()
    {
        var state = new WorldState(StartDate);
        var missingId = state.CharacterIds.Issue();
        var pipeline = OriginateInboundLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());

        var command = new OriginateInboundLetterCommand(
            state.CommandIds.Issue(), "system", StartDate, null, "actor_0000007", missingId,
            LetterAction.NewsAndGossip, CourierType.Tabellarius,
            CorrespondenceTestFixtures.FarRegionId, CorrespondenceTestFixtures.HomeRegionId, null, false);
        var result = pipeline.Execute(state, command);

        Assert.That(result.Error, Is.EqualTo(OriginateInboundLetterCommands.RecipientNotFound));
    }

    // ---- RespondToLetterCommand -----------------------------------------------------------------------

    [Test]
    public void RespondToLetterMarksItAnsweredAndRecordsTheResponseAction()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var letterId = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: true);

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.CondolenceOrCongratulation));

        Assert.That(result.Accepted, Is.True);
        state.Letters.TryGet(letterId, out var letter);
        Assert.Multiple(() =>
        {
            Assert.That(letter!.Status, Is.EqualTo(LetterStatus.Answered));
            Assert.That(letter.Responded, Is.True);
            Assert.That(letter.ResponseAction, Is.EqualTo(LetterAction.CondolenceOrCongratulation));
        });
    }

    [Test]
    public void RespondToLetterRejectsAnOutboundLetter()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var route = OpenRoute();
        var letterId = state.LetterIds.Issue();
        var letter = Letter.Begin(
            letterId, LetterDirection.Outbound, LetterAction.NewsAndGossip, recipientId.ToTaggedString(),
            "actor_0000001", recipientId, route, CourierType.Tabellarius, null, StartDate, false);
        state.Letters.Add(letterId, letter);

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        Assert.That(result.Error, Is.EqualTo(RespondToLetterCommands.NotInbound));
    }

    [Test]
    public void RespondToLetterRejectsOneStillInTransit()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var pipeline = OriginateInboundLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        pipeline.Execute(state, new OriginateInboundLetterCommand(
            state.CommandIds.Issue(), "system", StartDate, null, "actor_0000007", recipientId,
            LetterAction.PetitionPatron, CourierType.Tabellarius,
            CorrespondenceTestFixtures.FarRegionId, CorrespondenceTestFixtures.HomeRegionId, null, true));
        var letterId = state.Letters.InAscendingOrder().Single().Key;

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.PetitionPatron));

        Assert.That(result.Error, Is.EqualTo(RespondToLetterCommands.NotYetDelivered));
    }

    [Test]
    public void RespondToLetterRejectsOneThatDoesNotRequireAResponse()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var letterId = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: false);

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        Assert.That(result.Error, Is.EqualTo(RespondToLetterCommands.DoesNotRequireResponse));
    }

    [Test]
    public void RespondToLetterRejectsAnAlreadyAnsweredLetter()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var letterId = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: true);
        RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        Assert.That(result.Error, Is.EqualTo(RespondToLetterCommands.AlreadyResponded));
    }

    [Test]
    public void RespondToLetterRejectsAnInterceptedLetter()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var letterId = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.Intercepted, requiresResponse: true);

        var result = RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        Assert.That(result.Error, Is.EqualTo(RespondToLetterCommands.LetterNeverArrived));
    }

    // ---- CorrespondenceTransitSystem ------------------------------------------------------------------

    [Test]
    public void TransitSystemAdvancesMonthsElapsedWithoutDeliveringEarly()
    {
        var (state, drafterId) = OneCharacterHousehold();
        SendOutbound(state, drafterId, CorrespondenceTestFixtures.FarRegionId, CourierType.Tabellarius); // transit = 6 months

        var system = new CorrespondenceTransitSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(1), Streams()));

        var letter = state.Letters.InAscendingOrder().Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(letter.Status, Is.EqualTo(LetterStatus.InTransit));
            Assert.That(letter.MonthsElapsed, Is.EqualTo(1));
        });
    }

    [Test]
    public void TransitSystemDeliversOnceTransitTimeElapses()
    {
        var (state, drafterId) = OneCharacterHousehold();
        SendOutbound(state, drafterId, CorrespondenceTestFixtures.NearRegionId, CourierType.Tabellarius); // transit = 1 month

        var system = new CorrespondenceTransitSystem();
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(1), Streams()));

        var letter = state.Letters.InAscendingOrder().Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(events.Single(), Is.InstanceOf<LetterDeliveredEvent>());
            Assert.That(letter.Status, Is.EqualTo(LetterStatus.Delivered));
            Assert.That(letter.ArrivalDate, Is.EqualTo(new GameDate(1)));
            Assert.That(letter.Outcome, Is.EqualTo(LetterOutcome.DeliveredIntact).Or.EqualTo(LetterOutcome.Intercepted).Or.EqualTo(LetterOutcome.Forged));
        });
    }

    [Test]
    public void TransitSystemRedirectsOnceWhenTheRecipientIsAwayFromHomeAtArrival()
    {
        var (state, drafterId, recipientId) = OneHouseholdWithASecondCharacter();
        state.Characters.TryGet(recipientId, out var recipient);
        state.Characters.Remove(recipientId);
        state.Characters.Add(recipientId, recipient with
        {
            CurrentTravelLocation = TravelLocation.ProvincialCapital(
                CorrespondenceTestFixtures.NearRegionId, new RuntimeIdCounter<Settlement>().Issue()),
        });

        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        pipeline.Execute(state, new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, recipientId.ToTaggedString(),
            LetterAction.NewsAndGossip, CourierType.Tabellarius, null,
            CorrespondenceTestFixtures.HomeRegionId, CorrespondenceTestFixtures.NearRegionId, null)); // transit = 1 month

        var system = new CorrespondenceTransitSystem();
        var firstTick = system.Tick(state, new MonthlyTickContext(new GameDate(1), Streams()));
        var afterRedirectCheck = state.Letters.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(firstTick, Is.Empty);
            Assert.That(afterRedirectCheck.Status, Is.EqualTo(LetterStatus.InTransit));
            Assert.That(afterRedirectCheck.Redirected, Is.True);
            Assert.That(afterRedirectCheck.RedirectionDelayMonths, Is.EqualTo(CorrespondenceTransitSystem.RedirectionDelayMonths));
        });

        var secondTick = system.Tick(state, new MonthlyTickContext(new GameDate(2), Streams()));
        var delivered = state.Letters.InAscendingOrder().Single().Value;

        Assert.Multiple(() =>
        {
            Assert.That(secondTick.Single(), Is.InstanceOf<LetterDeliveredEvent>());
            Assert.That(delivered.Status, Is.EqualTo(LetterStatus.Delivered));
            Assert.That(delivered.Redirected, Is.True);
        });
    }

    [Test]
    public void TransitSystemLeavesAlreadyDeliveredLettersUntouched()
    {
        var (state, recipientId) = OneCharacterHousehold();
        AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: false);

        var events = new CorrespondenceTransitSystem().Tick(state, new MonthlyTickContext(new GameDate(5), Streams()));

        Assert.That(events, Is.Empty);
    }

    // ---- LetterQueries.PendingInbox ---------------------------------------------------------------------

    [Test]
    public void PendingInboxOnlyReturnsDeliveredUnansweredLettersRequiringAResponse()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var needsResponse = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: true);
        AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: false);
        AddDeliveredInboundLetter(state, recipientId, LetterOutcome.Intercepted, requiresResponse: true);

        var pending = LetterQueries.PendingInbox(state, recipientId.ToTaggedString()).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(pending, Has.Length.EqualTo(1));
            Assert.That(pending[0].Key, Is.EqualTo(needsResponse));
        });
    }

    [Test]
    public void PendingInboxExcludesAnAlreadyAnsweredLetter()
    {
        var (state, recipientId) = OneCharacterHousehold();
        var letterId = AddDeliveredInboundLetter(state, recipientId, LetterOutcome.DeliveredIntact, requiresResponse: true);
        RespondToLetterCommands.Pipeline.Execute(
            state, new RespondToLetterCommand(state.CommandIds.Issue(), "player", StartDate, null, letterId, LetterAction.NewsAndGossip));

        var pending = LetterQueries.PendingInbox(state, recipientId.ToTaggedString()).ToArray();

        Assert.That(pending, Is.Empty);
    }

    // ---- Save/load round trip -----------------------------------------------------------------------------

    [Test]
    public void CorrespondenceStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, drafterId) = OneCharacterHousehold();
        SendOutbound(state, drafterId, CorrespondenceTestFixtures.FarRegionId, CourierType.HiredCarrier);
        AddDeliveredInboundLetter(state, drafterId, LetterOutcome.Forged, requiresResponse: true);
        new CorrespondenceTransitSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams()));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Letters.Count, Is.EqualTo(state.Letters.Count));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    // ---- Shared fixtures ------------------------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Character> DrafterId) OneCharacterHousehold()
    {
        var state = new WorldState(StartDate);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();

        var drafterId = state.CharacterIds.Issue();
        state.Characters.Add(drafterId, CharacterTestFixtures.Minimal(drafterId, nomen: "Drafter", household: householdId, location: settlementId));

        return (state, drafterId);
    }

    private static (WorldState State, RuntimeId<Character> DrafterId, RuntimeId<Character> RecipientId) OneHouseholdWithASecondCharacter()
    {
        var (state, drafterId) = OneCharacterHousehold();
        state.Characters.TryGet(drafterId, out var drafter);
        var recipientId = state.CharacterIds.Issue();
        state.Characters.Add(recipientId, CharacterTestFixtures.Minimal(recipientId, nomen: "Recipient", household: drafter.Household, location: drafter.Location));
        return (state, drafterId, recipientId);
    }

    private static void SendOutbound(
        WorldState state, RuntimeId<Character> drafterId, DefinitionId<RegionProfileDefinition> recipientRegionId, CourierType courierType)
    {
        var pipeline = SendLetterCommands.BuildPipeline(
            CorrespondenceTestFixtures.BuildDistanceTierCatalog(), CorrespondenceTestFixtures.BuildReachabilityCatalog());
        pipeline.Execute(state, new SendLetterCommand(
            state.CommandIds.Issue(), "player", StartDate, null, drafterId, "actor_0000042",
            LetterAction.NewsAndGossip, courierType, null,
            CorrespondenceTestFixtures.HomeRegionId, recipientRegionId, null));
    }

    /// <summary>Constructs an already-delivered inbound letter directly via <see cref="Letter.Restore"/>
    /// so response-guard and Inbox-query tests don't depend on random transit outcomes.</summary>
    private static RuntimeId<Letter> AddDeliveredInboundLetter(
        WorldState state, RuntimeId<Character> recipientId, LetterOutcome outcome, bool requiresResponse)
    {
        var letterId = state.LetterIds.Issue();
        var letter = Letter.Restore(
            letterId, LetterDirection.Inbound, LetterAction.PetitionPatron, "actor_0000009",
            recipientId.ToTaggedString(), draftedByCharacterId: null, StartDate, transitTimeMonths: 1,
            monthsElapsed: 1, redirectionDelayMonths: 0, arrivalDate: new GameDate(1), CourierType.Tabellarius,
            courierCharacterId: null, RouteRiskLevel.Secure, intercepted: outcome == LetterOutcome.Intercepted || outcome == LetterOutcome.Forged,
            forged: outcome == LetterOutcome.Forged, redirected: false, oralTraditionPenaltyApplied: false,
            requiresResponse: requiresResponse, responded: false, responseAction: null,
            LetterStatus.Delivered, outcome);
        state.Letters.Add(letterId, letter);
        return letterId;
    }
}
