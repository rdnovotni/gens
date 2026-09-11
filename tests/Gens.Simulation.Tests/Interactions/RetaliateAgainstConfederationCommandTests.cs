using System.Linq;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Combat;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Military;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 6 coverage for <see cref="RetaliateAgainstConfederationCommands"/> — the
/// Combat Resolution Engine wired onto Piracy &amp; Banditry's own deferred Retaliation (§5).</summary>
public sealed class RetaliateAgainstConfederationCommandTests
{
    private static readonly CombatSituation Neutral = new(CombatTerrain.Open, Ambush: false, Fortified: false);

    [Test]
    public void AHeavilyFavoredRetaliationIsADecisiveVictoryThatDriftsTheConfederationTowardDeclining()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId, out var squadId,
            MilitaryStrengthBand.Negligible, LivingWorldActorStandingTrend.Established, out var confederationActorId);

        var randomStreams = new RandomStreamSet();
        randomStreams.Add(RetaliateAgainstConfederationCommands.StreamName, seed: 1, stream: 1);
        var pipeline = RetaliateAgainstConfederationCommands.CreatePipeline(randomStreams);

        var result = pipeline.Execute(state, new RetaliateAgainstConfederationCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId,
            settlementId, confederationActorId, new[] { squadId }, Neutral, "The household strikes back."));

        state.Squads.TryGet(squadId, out var squad);
        state.Actors.TryGet(confederationActorId, out var confederation);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events, Has.Some.InstanceOf<ConfederationRetaliationResolvedEvent>());
            var resolved = result.Events.OfType<ConfederationRetaliationResolvedEvent>().Single();
            Assert.That(resolved.Outcome, Is.EqualTo(CombatOutcome.DecisiveVictory));
            Assert.That(squad!.Manpower, Is.LessThan(100).And.GreaterThan(90), "even a decisive win costs minimal, not zero, losses");
            Assert.That(squad.Status, Is.EqualTo(SquadStatus.Ready), "an abstract retaliation never touches Squad.Location/travel state");
            Assert.That(confederation!.StandingTrend, Is.EqualTo(LivingWorldActorStandingTrend.Declining));
        });

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(state)));
    }

    [Test]
    public void AHeavilyOutmatchedRetaliationCostsRealSquadCasualties()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId, out var squadId,
            MilitaryStrengthBand.Formidable, LivingWorldActorStandingTrend.Established, out var confederationActorId,
            manpower: 5);

        var randomStreams = new RandomStreamSet();
        randomStreams.Add(RetaliateAgainstConfederationCommands.StreamName, seed: 1, stream: 1);
        var pipeline = RetaliateAgainstConfederationCommands.CreatePipeline(randomStreams);

        var result = pipeline.Execute(state, new RetaliateAgainstConfederationCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId,
            settlementId, confederationActorId, new[] { squadId }, Neutral, "An ill-advised sortie."));

        state.Squads.TryGet(squadId, out var squad);
        state.Actors.TryGet(confederationActorId, out var confederation);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var resolved = result.Events.OfType<ConfederationRetaliationResolvedEvent>().Single();
            Assert.That(resolved.Outcome, Is.EqualTo(CombatOutcome.CatastrophicDefeat));
            Assert.That(squad!.Manpower, Is.LessThan(5));
            Assert.That(confederation!.StandingTrend, Is.EqualTo(LivingWorldActorStandingTrend.Established),
                "a loss must never itself drift the Confederation toward Declining");
        });
    }

    [Test]
    public void RejectsRetaliatingAgainstAnActorThatIsNotABanditConfederation()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId, out var squadId,
            MilitaryStrengthBand.Negligible, LivingWorldActorStandingTrend.Established, out _);
        var notAConfederation = state.ActorIds.Issue();

        var randomStreams = new RandomStreamSet();
        randomStreams.Add(RetaliateAgainstConfederationCommands.StreamName, seed: 1, stream: 1);
        var result = RetaliateAgainstConfederationCommands.CreatePipeline(randomStreams).Execute(state, new RetaliateAgainstConfederationCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId,
            settlementId, notAConfederation, new[] { squadId }, Neutral, "Nothing to retaliate against."));

        Assert.That(result.Error, Is.EqualTo(RetaliateAgainstConfederationCommands.ConfederationInvalid));
    }

    [Test]
    public void RejectsADeployedOrDestroyedSquad()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId, out var squadId,
            MilitaryStrengthBand.Negligible, LivingWorldActorStandingTrend.Established, out var confederationActorId);
        state.Squads.TryGet(squadId, out var squad);
        state.Squads.Remove(squadId);
        state.Squads.Add(squadId, squad! with { Status = SquadStatus.Deployed });

        var randomStreams = new RandomStreamSet();
        randomStreams.Add(RetaliateAgainstConfederationCommands.StreamName, seed: 1, stream: 1);
        var result = RetaliateAgainstConfederationCommands.CreatePipeline(randomStreams).Execute(state, new RetaliateAgainstConfederationCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId,
            settlementId, confederationActorId, new[] { squadId }, Neutral, "Cannot commit a Squad already away."));

        Assert.That(result.Error, Is.EqualTo(RetaliateAgainstConfederationCommands.SquadUnavailable));
    }

    private static WorldState BuildState(
        out RuntimeId<Household> householdId, out RuntimeId<Settlement> settlementId, out RuntimeId<Character> sponsorId,
        out RuntimeId<Squad> squadId, MilitaryStrengthBand confederationBand, LivingWorldActorStandingTrend confederationTrend,
        out RuntimeId<Actor> confederationActorId, int manpower = 100)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Italia"));
        settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        householdId = state.HouseholdIds.Issue();
        sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId, location: settlementId, household: householdId));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(100_000)));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 200));

        var established = MilitaryCommands.EstablishForce.Execute(state, new EstablishEstateForceCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId, settlementId, ForceInfrastructureTier.Barracks));
        var raised = MilitaryCommands.RaiseSquad.Execute(state, new RaiseSquadCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, settlementId, sponsorId,
            "First Italic", SquadType.Infantry, SquadRecruitmentSource.Citizens, PopGroupType.Operarii, manpower));
        squadId = state.Squads.InAscendingOrder().Single().Key;
        Assert.That(established.Accepted && raised.Accepted, Is.True, "test fixture setup must itself succeed");

        var confederationSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(confederationSettlementId, Settlement.Create(confederationSettlementId, regionId));
        var confederation = BanditConfederationCreationService.CreateAncientSeed(
            state, "Latrones", confederationTrend, new LivingWorldActorMilitaryStrength(confederationBand), regionId, confederationSettlementId);
        confederationActorId = confederation.ActorId;

        return state;
    }
}
