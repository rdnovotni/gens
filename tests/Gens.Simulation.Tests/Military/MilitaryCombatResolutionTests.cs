using System.Linq;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Combat;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Military;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Military;

/// <summary>Proves the shared Combat Resolution Engine (<see cref="Gens.Simulation.Combat.CombatResolutionEngine"/>;
/// build roadmap Phase 16 item 4) actually resolves a real <see cref="MilitaryDeployment"/> end to end,
/// and that its output satisfies <see cref="MilitaryCommands.ApplyAftermath"/>'s own existing invariants
/// (proving the two commands' shapes actually line up) rather than merely unit-testing the kernel in
/// isolation.</summary>
public sealed class MilitaryCombatResolutionTests
{
    private static readonly CombatSituation Neutral = new(CombatTerrain.Open, Ambush: false, Fortified: false);

    [Test]
    public void ResolvingAHeavilyFavoredDeploymentIsADecisiveVictoryThatConservesManpowerAndRoundTrips()
    {
        var state = BuildState(out var settlementId, out var squadId, out var deploymentId);
        var randomStreams = new RandomStreamSet();
        randomStreams.Add(CampaignBootstrapper.MilitaryCombatResolutionStreamName, seed: 1, stream: 1);
        var pipeline = MilitaryCommands.CreateResolveDeploymentPipeline(randomStreams);

        var weakOpposition = new[] { new CombatantGroup(CombatantType.Irregular, Manpower: 10, EquipmentTier: 0, Readiness: 10, Morale: 10) };
        var result = pipeline.Execute(state, new ResolveMilitaryDeploymentCommand(
            state.CommandIds.Issue(), "system", state.Date, null, deploymentId,
            weakOpposition, null, Neutral, Neutral, "A rout in the field."));

        state.Squads.TryGet(squadId, out var squad);
        state.MilitaryDeployments.TryGet(deploymentId, out var deployment);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(deployment!.Status, Is.EqualTo(MilitaryDeploymentStatus.Resolved));
            Assert.That(deployment.Outcome, Is.EqualTo(MilitaryOutcome.DecisiveVictory));
            Assert.That(squad!.Manpower, Is.LessThan(100).And.GreaterThan(90), "a Decisive Victory costs minimal, not zero, losses");
            Assert.That(squad.Status, Is.EqualTo(SquadStatus.Ready));
            Assert.That(deployment.Losses.Single().Casualties, Is.EqualTo(100 - squad.Manpower));
        });

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(state)));
    }

    [Test]
    public void ResolvingWithNoOpposingGroupsIsRejectedAndLeavesTheDeploymentActive()
    {
        var state = BuildState(out _, out _, out var deploymentId);
        var randomStreams = new RandomStreamSet();
        randomStreams.Add(CampaignBootstrapper.MilitaryCombatResolutionStreamName, seed: 1, stream: 1);
        var pipeline = MilitaryCommands.CreateResolveDeploymentPipeline(randomStreams);

        var result = pipeline.Execute(state, new ResolveMilitaryDeploymentCommand(
            state.CommandIds.Issue(), "system", state.Date, null, deploymentId,
            System.Array.Empty<CombatantGroup>(), null, Neutral, Neutral, "No enemy in sight."));

        state.MilitaryDeployments.TryGet(deploymentId, out var deployment);
        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(MilitaryCommands.OpposingForceInvalid));
            Assert.That(deployment!.Status, Is.EqualTo(MilitaryDeploymentStatus.Active));
        });
    }

    private static WorldState BuildState(out RuntimeId<Settlement> settlementId, out RuntimeId<Squad> squadId,
        out RuntimeId<MilitaryDeployment> deploymentId)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Italia"));
        settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId, location: settlementId, household: householdId));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(householdId), new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(100_000)));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 200));

        var established = MilitaryCommands.EstablishForce.Execute(state, new EstablishEstateForceCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId, settlementId, ForceInfrastructureTier.Barracks));
        var raised = MilitaryCommands.RaiseSquad.Execute(state, new RaiseSquadCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, settlementId, sponsorId,
            "First Italic", SquadType.Infantry, SquadRecruitmentSource.Citizens, PopGroupType.Operarii, 100));
        squadId = state.Squads.InAscendingOrder().Single().Key;

        var deployed = MilitaryCommands.BeginDeployment.Execute(state, new BeginMilitaryDeploymentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, sponsorId, settlementId,
            MilitaryDeploymentType.OffenseCampaign, TravelLocation.Rome(), new[] { squadId }));
        deploymentId = state.MilitaryDeployments.InAscendingOrder().Single().Key;

        Assert.That(established.Accepted && raised.Accepted && deployed.Accepted, Is.True, "test fixture setup must itself succeed");
        return state;
    }
}
