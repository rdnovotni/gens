using Gens.Simulation.Characters;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Military;
using Gens.Simulation.Queries;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Military;

public sealed class MilitaryLifecycleTests
{
    [Test]
    public void RaisingEquippingDeployingAndAftermathConserveStateAndRoundTrip()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId);

        var established = MilitaryCommands.EstablishForce.Execute(state, new EstablishEstateForceCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, householdId, sponsorId,
            settlementId, ForceInfrastructureTier.Barracks));
        var raised = MilitaryCommands.RaiseSquad.Execute(state, new RaiseSquadCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, settlementId, sponsorId,
            "First Italic", SquadType.Infantry, SquadRecruitmentSource.MusteredVeterans, PopGroupType.Veterans, 20));
        var squadId = state.Squads.InAscendingOrder().Single().Key;

        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId, householdId.ToTaggedString()));
        var stockpile = new Stockpile(100);
        var weapons = new GoodDefinition(new DefinitionId<Good>("weapons"), Perishability.NonPerishable);
        stockpile.Add(weapons, 30);
        state.Stockpiles.Add(holdingId, stockpile);
        var equipped = MilitaryCommands.CommitEquipment.Execute(state, new CommitSquadEquipmentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, sponsorId, squadId, holdingId,
            EquipmentKind.Weapons, weapons.Id, null, 20));

        var deployed = MilitaryCommands.BeginDeployment.Execute(state, new BeginMilitaryDeploymentCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, sponsorId, settlementId,
            MilitaryDeploymentType.OffenseCampaign, TravelLocation.Rome(), new[] { squadId }));
        var deploymentId = state.MilitaryDeployments.InAscendingOrder().Single().Key;

        var captiveId = state.CharacterIds.Issue();
        state.Characters.Add(captiveId, CharacterTestFixtures.Minimal(captiveId, location: settlementId));
        var aftermath = MilitaryCommands.ApplyAftermath.Execute(state, new ApplyMilitaryAftermathCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, deploymentId,
            MilitaryOutcome.CostlyVictory,
            new[] { new SquadLoss(squadId, 5, 2, 10, 20, new[] { new EquipmentLoss(EquipmentKind.Weapons, 3) }) },
            4, settlementId, PopGroupType.Operarii, settlementId, new[] { captiveId }, householdId,
            "The force prevailed at a real cost."));

        state.Squads.TryGet(squadId, out var squad);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Veterans), out var veterans);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved), out var enslaved);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var captiveSource);
        state.MilitaryDeployments.TryGet(deploymentId, out var resolved);

        Assert.Multiple(() =>
        {
            Assert.That(established.Accepted && raised.Accepted && equipped.Accepted && deployed.Accepted && aftermath.Accepted, Is.True);
            Assert.That(veterans!.Size, Is.EqualTo(32), "twenty mustered, two deserters returned");
            Assert.That(enslaved!.Size, Is.EqualTo(14), "four captives entered the existing labor population");
            Assert.That(captiveSource!.Size, Is.EqualTo(46), "captives left a real source population");
            Assert.That(squad!.Manpower, Is.EqualTo(13));
            Assert.That(squad.Equipment.Single().Available, Is.EqualTo(17));
            Assert.That(squad.Location.Kind, Is.EqualTo(LocationKind.Home));
            Assert.That(resolved!.Status, Is.EqualTo(MilitaryDeploymentStatus.Resolved));
            Assert.That(state.MilitaryCaptivities.TryGet(captiveId, out _), Is.True);
            Assert.That(Gens.Simulation.Crime.DetentionResolver.ActiveFor(state, captiveId), Is.Not.Null,
                "military capture must enter the ordinary detention/ransom state model");
        });

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(state)));
            Assert.That(CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored)),
                Is.EqualTo(CanonicalJson.SerializeToCanonicalBytes(dto)));
        });
    }

    [Test]
    public void MonthlyReadinessRecoversAndUnpaidLowMoraleSquadDesertsIntoItsSourceCohort()
    {
        var state = BuildState(out var householdId, out var settlementId, out _);
        state.EstateForces.Add(settlementId, new EstateForce(settlementId, householdId,
            ForceInfrastructureTier.Barracks, null, state.Date));
        state.LedgerAccounts.Remove(LedgerAccountKey.ForHousehold(householdId));
        var squadId = state.SquadIds.Issue();
        state.Squads.Add(squadId, Squad.Create(squadId, settlementId, "Unpaid", SquadType.Infantry,
            SquadRecruitmentSource.Citizens, PopGroupType.Operarii, 10, readiness: 20, morale: 30));

        var events = new MilitaryReadinessSystem().Tick(state,
            new MonthlyTickContext(new GameDate(1), new Gens.Simulation.Random.RandomStreamSet()));

        state.Squads.TryGet(squadId, out var squad);
        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var operarii);
        Assert.Multiple(() =>
        {
            Assert.That(squad!.Readiness, Is.EqualTo(28));
            Assert.That(squad.Morale, Is.EqualTo(15));
            Assert.That(squad.Manpower, Is.EqualTo(9));
            Assert.That(operarii!.Size, Is.EqualTo(51));
            Assert.That(events.OfType<MilitaryStateChangedEvent>().Single().Change, Is.EqualTo("desertion"));
        });
    }

    [Test]
    public void RejectedRecruitmentDoesNotConsumePopulation()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId);
        state.EstateForces.Add(settlementId, new EstateForce(settlementId, householdId,
            ForceInfrastructureTier.Barracks, null, state.Date));

        var result = MilitaryCommands.RaiseSquad.Execute(state, new RaiseSquadCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, settlementId, sponsorId,
            "Impossible", SquadType.Infantry, SquadRecruitmentSource.MusteredVeterans,
            PopGroupType.Veterans, MilitaryCatalog.MaxSquadManpower));

        state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Veterans), out var veterans);
        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(MilitaryCommands.InsufficientPopulation));
            Assert.That(veterans!.Size, Is.EqualTo(50));
            Assert.That(state.Squads.Count, Is.Zero);
        });
    }

    [Test]
    public void DemobilizingAMusterReturnsEverySurvivorToTheVeteranCohort()
    {
        var state = BuildState(out var householdId, out var settlementId, out var sponsorId);
        state.EstateForces.Add(settlementId, new EstateForce(settlementId, householdId,
            ForceInfrastructureTier.Barracks, null, state.Date));
        var squadId = state.SquadIds.Issue();
        state.Squads.Add(squadId, Squad.Create(squadId, settlementId, "Muster", SquadType.Infantry,
            SquadRecruitmentSource.MusteredVeterans, PopGroupType.Veterans, 18, 70, 60));
        var veteransKey = new PopGroupKey(settlementId, PopGroupType.Veterans);
        state.PopGroups.TryGet(veteransKey, out var veterans);
        state.PopGroups.Remove(veteransKey);
        state.PopGroups.Add(veteransKey, veterans! with { Size = veterans.Size - 18 });

        var result = MilitaryCommands.DemobilizeSquad.Execute(state, new DemobilizeSquadCommand(
            state.CommandIds.Issue(), sponsorId.ToTaggedString(), state.Date, null, sponsorId, squadId));

        state.PopGroups.TryGet(veteransKey, out var returnedVeterans);
        state.Squads.TryGet(squadId, out var squad);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(returnedVeterans!.Size, Is.EqualTo(50));
            Assert.That(squad!.Status, Is.EqualTo(SquadStatus.Demobilized));
            Assert.That(squad.Manpower, Is.Zero);
        });
    }

    [Test]
    public void InvalidAftermathIsAtomicAndLeavesDeploymentActive()
    {
        var state = BuildState(out var householdId, out var settlementId, out _);
        state.EstateForces.Add(settlementId, new EstateForce(settlementId, householdId,
            ForceInfrastructureTier.Barracks, null, state.Date));
        var squadId = state.SquadIds.Issue();
        state.Squads.Add(squadId, Squad.Create(squadId, settlementId, "First", SquadType.Infantry,
            SquadRecruitmentSource.Citizens, PopGroupType.Operarii, 10, 50, 50));
        var deploymentId = state.MilitaryDeploymentIds.Issue();
        state.MilitaryDeployments.Add(deploymentId, MilitaryDeployment.Begin(deploymentId, settlementId,
            MilitaryDeploymentType.Defense, TravelLocation.Home(settlementId), new[] { squadId }, state.Date));
        var deployedSquad = state.Squads.InAscendingOrder().Single().Value with { Status = SquadStatus.Deployed };
        state.Squads.Remove(squadId);
        state.Squads.Add(squadId, deployedSquad);

        var result = MilitaryCommands.ApplyAftermath.Execute(state, new ApplyMilitaryAftermathCommand(
            state.CommandIds.Issue(), "system", state.Date, null, deploymentId, MilitaryOutcome.Defeat,
            new[] { new SquadLoss(squadId, 11, 0, 0, 0, Array.Empty<EquipmentLoss>()) }, 0, null, null, null,
            Array.Empty<RuntimeId<Character>>(), null, "Impossible losses."));

        state.Squads.TryGet(squadId, out var squad);
        state.MilitaryDeployments.TryGet(deploymentId, out var deployment);
        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(MilitaryCommands.AftermathInvalid));
            Assert.That(squad!.Manpower, Is.EqualTo(10));
            Assert.That(deployment!.Status, Is.EqualTo(MilitaryDeploymentStatus.Active));
        });
    }

    [Test]
    public void ForceQueryReturnsOnlyTheRequestedHouseholdsSnapshot()
    {
        var state = BuildState(out var householdId, out var settlementId, out _);
        state.EstateForces.Add(settlementId, new EstateForce(settlementId, householdId,
            ForceInfrastructureTier.Garrison, null, state.Date));
        var squadId = state.SquadIds.Issue();
        state.Squads.Add(squadId, Squad.Create(squadId, settlementId, "First", SquadType.Infantry,
            SquadRecruitmentSource.Citizens, PopGroupType.Operarii, 10, 50, 50));

        var projection = new MilitaryForceQuery().Execute(state, householdId.ToTaggedString());

        Assert.Multiple(() =>
        {
            Assert.That(projection, Is.Not.Null);
            Assert.That(projection!.Value.SquadCap, Is.EqualTo(4));
            Assert.That(projection.Value.Squads.Single().Name, Is.EqualTo("First"));
        });
    }

    private static WorldState BuildState(out RuntimeId<Household> householdId,
        out RuntimeId<Settlement> settlementId, out RuntimeId<Character> sponsorId)
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Italia"));
        settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        householdId = state.HouseholdIds.Issue();
        sponsorId = state.CharacterIds.Issue();
        state.Characters.Add(sponsorId, CharacterTestFixtures.Minimal(sponsorId, location: settlementId, household: householdId));
        state.LedgerAccounts.Add(LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(100_000)));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Veterans), PopGroup.Create(settlementId, PopGroupType.Veterans, 50));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.Operarii), PopGroup.Create(settlementId, PopGroupType.Operarii, 50));
        state.PopGroups.Add(new PopGroupKey(settlementId, PopGroupType.NonHouseholdEnslaved), PopGroup.Create(settlementId, PopGroupType.NonHouseholdEnslaved, 10));
        return state;
    }
}
