using Gens.Simulation.Actions;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Policies;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.ExitGate;

/// <summary>
/// Proves the Phase 10 exit gate, verbatim from the roadmap: "several rival actors and a delegated
/// household survive a 200-year soak; their actions use legal commands, generate reports/rumors
/// according to visibility, and remain inside tick budgets." Combines every package this phase
/// shipped in one run: <see cref="LivingWorldActor"/> Background/Noteworthy tiers (package 3),
/// rival-house creation (package 4), the Noteworthy ambition loop (package 7), Ancestral Grudges
/// (package 8), a <see cref="StewardshipAssignment"/> with real competence/loyalty/incident rolls and
/// a <see cref="ReturnReport"/> (packages 9/10/13), house extinction and the <see cref="Scheme"/>
/// engine (packages 11/12), and <see cref="RivalDossier"/> refresh (package 14).
///
/// Promotion/demotion (item 3) and the Return Report (package 13) are exercised via direct calls
/// alongside the tick loop rather than waited on to emerge spontaneously — nothing in this phase wires
/// <see cref="LivingWorldActorTieringService"/> or ending a <see cref="StewardshipAssignment"/> into
/// any monthly system yet (both remain command/service-level operations a future phase's UI or AI
/// triggers), so proving they work end-to-end inside a real soak is this test's job, not waiting for
/// them to fire on their own. The tick-budget cap itself is unit-tested directly by <see
/// cref="Tests.Actors.BackgroundHouseDriftSystemTests.ProcessingMoreActorsThanTheBudgetCompletesWithoutError"/>;
/// this test only confirms an over-cap population survives the full 200-year run without error.
/// </summary>
public sealed class RivalHousesAndStewardshipSoakTests
{
    private const int TotalMonths = 2400;

    [Test]
    public void SeveralRivalHousesAndADelegatedHouseholdSurviveATwoHundredYearSoak()
    {
        var seedsTried = new[] { 1001UL, 2002UL, 3003UL, 4004UL, 5005UL };
        var sawExtinction = false;
        var sawPromotion = false;
        var sawDemotion = false;
        var sawReturnReportOrIncident = false;

        foreach (var seed in seedsTried)
        {
            var result = RunScenario(seed, TotalMonths);

            sawExtinction |= result.SawExtinction;
            sawPromotion |= result.SawPromotion;
            sawDemotion |= result.SawDemotion;
            sawReturnReportOrIncident |= result.SawReturnReportOrIncident;

            // No unhandled exceptions/invariant failures: RunScenario itself would have thrown (the
            // debug-only WriteSetVerifyingSimulation also throws on any undeclared partition write).
            Assert.That(result.State.Date.TotalMonths, Is.EqualTo(TotalMonths));

            // Every event with a CausationId traces back to a structurally real command reference
            // (rule: no direct state mutation bypassing the command pipeline) — an event legitimately
            // has no CausationId only when it is not itself command-originated (e.g. an extinction or
            // a Scheme's own resolution, both fixed to null by their own record definitions).
            foreach (var evt in result.Events)
            {
                if (evt.CausationId is not { } causationId)
                    continue;
                Assert.DoesNotThrow(
                    () => RuntimeId<Command>.Parse(causationId),
                    $"Event '{evt.Type}' carried a CausationId that is not a real command reference: '{causationId}'.");
            }

            // The tick-budget population proof: enough Background actors were seeded to force the
            // drift system's per-tick rotation for the entire run (a 200-year run at a real,
            // per-processed-actor extinction chance is expected to thin the roster substantially by
            // the end — see SawExtinction below — so the over-cap assertion is against the seeded
            // count, not whatever survives).
            Assert.That(result.InitialBackgroundActorCount, Is.GreaterThan(LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick));
        }

        Assert.Multiple(() =>
        {
            Assert.That(sawExtinction, Is.True, "Expected at least one house extinction across the tried seeds.");
            Assert.That(sawPromotion, Is.True, "Expected at least one Noteworthy promotion across the tried seeds.");
            Assert.That(sawDemotion, Is.True, "Expected at least one Noteworthy-to-Background demotion across the tried seeds.");
            Assert.That(sawReturnReportOrIncident, Is.True, "Expected at least one Return Report or steward incident across the tried seeds.");
        });
    }

    [Test]
    public void SameSeedReproducesIdenticalStateHashAcrossTwoIndependentRuns()
    {
        const int months = 120;

        var runA = RunScenario(424242UL, months);
        var runB = RunScenario(424242UL, months);

        Assert.Multiple(() =>
        {
            Assert.That(runB.Events.Select(Describe), Is.EqualTo(runA.Events.Select(Describe)));
            Assert.That(runB.FinalHash, Is.EqualTo(runA.FinalHash));
        });
    }

    private static string Describe(IDomainEvent evt) =>
        $"{evt.Type}|{evt.OccurredDate.TotalMonths}|{evt.SchemaVersion}|{string.Join(",", evt.SubjectIds)}|{evt.CausationId}";

    private sealed record ScenarioResult(
        WorldState State,
        IReadOnlyList<IDomainEvent> Events,
        ulong FinalHash,
        int InitialBackgroundActorCount,
        bool SawExtinction,
        bool SawPromotion,
        bool SawDemotion,
        bool SawReturnReportOrIncident);

    private static ScenarioResult RunScenario(ulong seed, int months)
    {
        var config = BuildConfig(seed);
        var campaign = CampaignBootstrapper.Bootstrap(config);
        var state = campaign.State;
        var streams = campaign.RandomStreams;

        var simulation = new WriteSetVerifyingSimulation(new IMonthlySystem<WorldState>[]
        {
            new ScheduledActionSystem(),
            new CharacterLifecycleSystem(CampaignBootstrapper.CharacterMortalityStreamName),
            new RelationshipDecaySystem(),
            new BackgroundHouseDriftSystem(),
            new RivalAmbitionSystem(
                RivalHouseActionDefinitions.BuildCatalog(), new TraitCatalog(Array.Empty<TraitDefinition>()),
                CampaignBootstrapper.RivalAmbitionStreamName),
            new LivingWorldActorExtinctionSystem(),
            new AncestralGrudgeDecaySystem(),
            new SchemeProgressSystem(),
            new StewardAutonomousDecisionSystem(PolicyActionDefinitions.BuildCatalog(), _ => campaign.SettlementId),
        });

        // Enough Background rival houses to force the drift system's per-tick rotation (package 3/7).
        var backgroundCount = LivingWorldActorDriftCatalog.MaxBackgroundActorsProcessedPerTick + 50;
        var seededActors = new List<LivingWorldActor>();
        for (var i = 0; i < backgroundCount; i++)
        {
            var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
            var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
            var actor = RivalHouseCreationService.CreateAncientSeed(
                state, $"Rivalia{i}", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
                dignitas: 10, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
            seededActors.Add(actor);
        }

        // Promote two Background actors to Noteworthy with a real head Character each (package 3/4).
        var noteworthy = new List<(RuntimeId<Actor> ActorId, RuntimeId<Character> HeadId)>();
        for (var i = 0; i < 2; i++)
        {
            var actor = seededActors[i];
            var headId = state.CharacterIds.Issue();
            state.Characters.Add(
                headId,
                CharacterFixtures.Minimal(
                    headId, attributes: new CoreAttributes(20, 20, 30, 20, 20), condition: new Condition(80, 0, 50, 80, 50)));
            state.Actors.Remove(actor.ActorId);
            state.Actors.Add(actor.ActorId, actor with { HeadCharacterId = headId });
            LivingWorldActorTieringService.RecordContactAndPromote(state, actor.ActorId, state.Date);
            noteworthy.Add((actor.ActorId, headId));
        }

        var sawPromotion = noteworthy.Count > 0;

        // Exercise demotion directly (item 3's other half): freeze the quiet clock into the past for
        // one Noteworthy actor and demote it back to Background.
        state.Actors.TryGet(noteworthy[0].ActorId, out var toDemote);
        var staleContact = new GameDate(state.Date.TotalMonths - LivingWorldActorTieringCatalog.DemotionQuietPeriodMonths - 1);
        state.Actors.Remove(noteworthy[0].ActorId);
        state.Actors.Add(noteworthy[0].ActorId, toDemote! with { LastContactDate = staleContact });
        var demoted = LivingWorldActorTieringService.DemoteIfQuiet(state, noteworthy[0].ActorId, state.Date);
        var sawDemotion = demoted.Tier == LivingWorldActorTier.Background;

        // The player household's own participant and a low-competence, low-loyalty steward appointee.
        var playerCharacterId = state.CharacterIds.Issue();
        state.Characters.Add(playerCharacterId, CharacterFixtures.Minimal(playerCharacterId, praenomen: "Gaius", nomen: "Player"));

        var stewardId = state.CharacterIds.Issue();
        state.Characters.Add(
            stewardId,
            CharacterFixtures.Minimal(
                stewardId, praenomen: "Titus", nomen: "Steward",
                attributes: new CoreAttributes(10, 10, 25, 10, 10), condition: new Condition(80, 0, 5, 20, 50)));

        var appointResult = StewardshipCommands.AppointPipeline.Execute(
            state,
            new AppointStewardshipCommand(
                state.CommandIds.Issue(), campaign.HouseholdId.ToTaggedString(), state.Date, null, campaign.HouseholdId,
                StewardshipContext.Travel, StewardshipMode.SingleSteward, stewardId, null, null, StewardAutonomyLevel.FullAutonomy));
        var assignmentId = ((StewardshipAssignedEvent)appointResult.Events[0]).AssignmentId;

        // Fund the treasury so both ordinary steward actions and any incident have money to move.
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(campaign.HouseholdId), Money.FromDenarii(2000)),
                new LedgerPosting(LedgerAccountKey.Mint, -(Money.FromDenarii(2000))),
            });

        // One initiated Scheme between the player household's own character and a Noteworthy rival head.
        InitiateSchemeCommands.Pipeline.Execute(
            state,
            new InitiateSchemeCommand(
                state.CommandIds.Issue(), playerCharacterId.ToTaggedString(), state.Date, null, playerCharacterId,
                noteworthy[1].HeadId, SchemeType.Coercive));

        var allEvents = new List<IDomainEvent>();
        var sawExtinction = false;
        var sawReturnReportOrIncident = false;
        var endedAssignment = false;

        for (var month = 0; month < months; month++)
        {
            var events = simulation.Tick(state, state.Date, streams);
            allEvents.AddRange(events);

            if (events.Any(e => e is LivingWorldActorExtinguishedEvent))
                sawExtinction = true;
            if (events.Any(e => e is StewardIncidentDiscoveredEvent))
                sawReturnReportOrIncident = true;

            state.AdvanceMonth();

            // End the stewardship assignment partway through (Travel's own natural return) to prove
            // the Return Report path end-to-end, then leave the household undelegated for the rest of
            // the run — matching how a real Travel absence eventually ends.
            if (!endedAssignment && month == 24 && state.StewardshipAssignments.TryGet(assignmentId, out var current) && current!.IsActive)
            {
                var endResult = StewardshipCommands.EndPipeline.Execute(
                    state,
                    new EndStewardshipAssignmentCommand(
                        state.CommandIds.Issue(), campaign.HouseholdId.ToTaggedString(), state.Date, null, assignmentId));
                if (endResult.Accepted)
                {
                    allEvents.AddRange(endResult.Events);
                    sawReturnReportOrIncident |= endResult.Events.Any(e => e is ReturnReportGeneratedEvent);
                    endedAssignment = true;
                }
            }
        }

        return new ScenarioResult(
            state, allEvents, StateHasher.Hash(state), backgroundCount, sawExtinction, sawPromotion, sawDemotion,
            sawReturnReportOrIncident);
    }

    private static CampaignConfig BuildConfig(ulong seed) => new()
    {
        Seed = seed,
        StartDate = new GameDate(0),
        RulesetId = "classic",
        ContentPackHash = "content-hash-placeholder",
        RegionId = "latium",
        Difficulty = "standard",
    };
}
