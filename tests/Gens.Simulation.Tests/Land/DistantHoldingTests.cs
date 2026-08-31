using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Regions;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Tests.Travel;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Land;

/// <summary>Phase 13 item 7 coverage — <see cref="DistantHolding"/>, its two commands, and <see
/// cref="DistantHoldingMismanagementRiskSystem"/>. Reuses <see cref="TravelTestFixtures"/>'s own Home/
/// Near/Far region ids and distance-tier catalog rather than authoring a second, redundant lookup
/// table, matching Correspondence item 3's own "reuses Travel's own distance model" precedent.</summary>
public sealed class DistantHoldingTests
{
    // ---- DistantHolding.Begin ------------------------------------------------------------------------

    [Test]
    public void BeginFlagsMismanagementRiskImmediatelyForAFarUnstaffedAcquisition()
    {
        var state = new WorldState(new GameDate(0));
        var holding = DistantHolding.Begin(
            state.DistantHoldingIds.Issue(), state.HouseholdIds.Issue(), TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.FarRegionId, state.HoldingIds.Issue(), DistanceTier.Far);

        Assert.Multiple(() =>
        {
            Assert.That(holding.ProcuratorCharacterId, Is.Null);
            Assert.That(holding.MismanagementRiskActive, Is.True);
        });
    }

    [Test]
    public void BeginNeverFlagsMismanagementRiskForANearAcquisition()
    {
        var state = new WorldState(new GameDate(0));
        var holding = DistantHolding.Begin(
            state.DistantHoldingIds.Issue(), state.HouseholdIds.Issue(), TravelTestFixtures.HomeRegionId,
            TravelTestFixtures.NearRegionId, state.HoldingIds.Issue(), DistanceTier.Near);

        Assert.That(holding.MismanagementRiskActive, Is.False);
    }

    // ---- AcquireDistantHoldingCommand -----------------------------------------------------------------

    [Test]
    public void AcquireRegistersADistantHoldingWithTheResolvedDistanceTier()
    {
        var (state, householdId, holdingId) = OneHouseholdWithAHolding();
        var pipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());

        var result = pipeline.Execute(state, MakeAcquireCommand(state, householdId, TravelTestFixtures.FarRegionId, holdingId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var evt = (DistantHoldingAcquiredEvent)result.Events[0];
            Assert.That(evt.DistanceTier, Is.EqualTo(DistanceTier.Far));
            Assert.That(state.DistantHoldings.TryGet(evt.DistantHoldingId, out var stored), Is.True);
            Assert.That(stored!.MismanagementRiskActive, Is.True);
        });
    }

    [Test]
    public void AcquireRejectsAHoldingNotOwnedByTheHousehold()
    {
        var (state, _, holdingId) = OneHouseholdWithAHolding();
        var otherHouseholdId = state.HouseholdIds.Issue();
        var pipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());

        var result = pipeline.Execute(
            state, MakeAcquireCommand(state, otherHouseholdId, TravelTestFixtures.FarRegionId, holdingId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(DistantHoldingCommands.HoldingNotOwnedByHousehold));
        });
    }

    [Test]
    public void AcquireRejectsAHoldingInTheHouseholdsOwnHomeRegion()
    {
        var (state, householdId, holdingId) = OneHouseholdWithAHolding();
        var pipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());

        var result = pipeline.Execute(
            state, MakeAcquireCommand(state, householdId, TravelTestFixtures.HomeRegionId, holdingId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(DistantHoldingCommands.NotActuallyDistant));
        });
    }

    [Test]
    public void AcquireRejectsARegisteringTheSameHoldingTwice()
    {
        var (state, householdId, holdingId) = OneHouseholdWithAHolding();
        var pipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());
        pipeline.Execute(state, MakeAcquireCommand(state, householdId, TravelTestFixtures.FarRegionId, holdingId));

        var result = pipeline.Execute(state, MakeAcquireCommand(state, householdId, TravelTestFixtures.FarRegionId, holdingId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(DistantHoldingCommands.AlreadyRegistered));
        });
    }

    // ---- AppointProcuratorCommand ----------------------------------------------------------------------

    [Test]
    public void AppointProcuratorStaffsTheHoldingAndCreatesTheBackingAssignment()
    {
        var (state, householdId, distantHoldingId, characterId) = OneDistantHoldingWithACandidate();

        var result = DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, characterId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(state.DistantHoldings.TryGet(distantHoldingId, out var stored), Is.True);
            Assert.That(stored!.ProcuratorCharacterId, Is.EqualTo(characterId));
            // High Loyalty candidate on a Far holding: no longer at risk once staffed.
            Assert.That(stored.MismanagementRiskActive, Is.False);

            var backing = state.StewardshipAssignments.InAscendingOrder()
                .Select(entry => entry.Value)
                .Single(a => a.HouseholdId == householdId && a.Context == StewardshipContext.SecondSettlementProcurator);
            Assert.That(backing.AppointeeCharacterId, Is.EqualTo(characterId));
            Assert.That(backing.IsActive, Is.True);
        });
    }

    [Test]
    public void AppointProcuratorRejectsACandidateFromAnotherHousehold()
    {
        var (state, _, distantHoldingId, _) = OneDistantHoldingWithACandidate();
        var strangerId = state.CharacterIds.Issue();
        state.Characters.Add(strangerId, CharacterTestFixtures.Minimal(strangerId, household: state.HouseholdIds.Issue()));

        var result = DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, strangerId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(DistantHoldingCommands.CandidateNotHouseholdMember));
        });
    }

    [Test]
    public void AppointProcuratorRejectsWhenTheHouseholdAlreadyHasAnActiveAssignment()
    {
        var (state, householdId, distantHoldingId, characterId) = OneDistantHoldingWithACandidate();
        StewardshipCommands.AppointPipeline.Execute(
            state, new AppointStewardshipCommand(
                state.CommandIds.Issue(), "player", state.Date, null, householdId, StewardshipContext.Travel,
                StewardshipMode.SingleSteward, characterId, null, null, StewardshipAssignment.DefaultAutonomyLevel));

        var result = DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, characterId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(DistantHoldingCommands.HouseholdAlreadyHasActiveAssignment));
        });
    }

    // ---- DistantHoldingMismanagementRiskSystem -----------------------------------------------------

    [Test]
    public void TickFlagsRiskWhenTheProcuratorsLoyaltyFallsBelowTheThreshold()
    {
        var (state, _, distantHoldingId, characterId) = OneDistantHoldingWithACandidate();
        DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, characterId));

        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { Condition = character.Condition with { Loyalty = 10 } });

        new DistantHoldingMismanagementRiskSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.DistantHoldings.TryGet(distantHoldingId, out var stored);
        Assert.That(stored!.MismanagementRiskActive, Is.True);
    }

    [Test]
    public void TickVacatesTheProcuratorAndFlagsRiskOnceTheyDie()
    {
        var (state, _, distantHoldingId, characterId) = OneDistantHoldingWithACandidate();
        DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, characterId));

        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { DeathRecord = new DeathRecord(state.Date, DeathCause.Unspecified, ageAtDeath: 40) });

        new DistantHoldingMismanagementRiskSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.DistantHoldings.TryGet(distantHoldingId, out var stored);
        Assert.Multiple(() =>
        {
            Assert.That(stored!.ProcuratorCharacterId, Is.Null);
            Assert.That(stored.MismanagementRiskActive, Is.True);
        });
    }

    [Test]
    public void TickNeverFlagsRiskForAnUnstaffedNearHolding()
    {
        var (state, householdId, holdingId) = OneHouseholdWithAHolding();
        var acquirePipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());
        var acquireResult = acquirePipeline.Execute(
            state, MakeAcquireCommand(state, householdId, TravelTestFixtures.NearRegionId, holdingId));
        var distantHoldingId = ((DistantHoldingAcquiredEvent)acquireResult.Events[0]).DistantHoldingId;

        new DistantHoldingMismanagementRiskSystem().Tick(state, new MonthlyTickContext(state.Date, new RandomStreamSet()));

        state.DistantHoldings.TryGet(distantHoldingId, out var stored);
        Assert.That(stored!.MismanagementRiskActive, Is.False);
    }

    // ---- Save/load round trip ----------------------------------------------------------------------

    [Test]
    public void DistantHoldingStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, _, distantHoldingId, characterId) = OneDistantHoldingWithACandidate();
        DistantHoldingCommands.AppointProcuratorPipeline.Execute(
            state, new AppointProcuratorCommand(state.CommandIds.Issue(), "player", state.Date, null, distantHoldingId, characterId));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.DistantHoldings.Count, Is.EqualTo(state.DistantHoldings.Count));
            restored.DistantHoldings.TryGet(distantHoldingId, out var restoredHolding);
            Assert.That(restoredHolding!.ProcuratorCharacterId, Is.EqualTo(characterId));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }

    // ---- Shared fixtures -------------------------------------------------------------------------------

    private static AcquireDistantHoldingCommand MakeAcquireCommand(
        WorldState state, RuntimeId<Household> householdId, DefinitionId<RegionProfileDefinition> holdingRegionId, RuntimeId<Holding> holdingId) =>
        new(state.CommandIds.Issue(), householdId.ToTaggedString(), state.Date, null, householdId,
            TravelTestFixtures.HomeRegionId, holdingRegionId, holdingId);

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Holding> HoldingId) OneHouseholdWithAHolding()
    {
        var state = new WorldState(new GameDate(0));
        var householdId = state.HouseholdIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var holdingId = state.HoldingIds.Issue();
        state.Holdings.Add(holdingId, Holding.Create(holdingId, settlementId, ownerId: householdId.ToTaggedString()));

        return (state, householdId, holdingId);
    }

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<DistantHolding> DistantHoldingId, RuntimeId<Character> CandidateId)
        OneDistantHoldingWithACandidate()
    {
        var (state, householdId, holdingId) = OneHouseholdWithAHolding();
        var acquirePipeline = DistantHoldingCommands.BuildAcquirePipeline(TravelTestFixtures.BuildDistanceTierCatalog());
        var acquireResult = acquirePipeline.Execute(
            state, MakeAcquireCommand(state, householdId, TravelTestFixtures.FarRegionId, holdingId));
        var distantHoldingId = ((DistantHoldingAcquiredEvent)acquireResult.Events[0]).DistantHoldingId;

        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(
            characterId, household: householdId, condition: new Condition(80, 0, 90, 20, 50)));

        return (state, householdId, distantHoldingId, characterId);
    }
}
