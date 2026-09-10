using System.Linq;
using Gens.Simulation.Actors;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Succession;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Interactions;

/// <summary>Phase 16 item 2 coverage for <see cref="RaidThreatSystem"/> (§2, §3, §9). Every scenario
/// here is either fully deterministic regardless of the RNG seed (an early-return guard, or a regional
/// mismatch that always fails to find a target), or branches on whichever outcome actually rolled
/// (mirroring <see cref="CounterEspionageSweepCommandTests"/>'s identical "assert consistency for
/// whichever branch happened" pattern) — this suite never asserts a single interception/success roll
/// outcome outright, since <see cref="RaidThreatCatalog"/>'s figures deliberately never saturate a
/// chance to a guaranteed 0% or 100% the way <see cref="SpyPlacementCatalog"/>'s do.</summary>
public sealed class RaidThreatSystemTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RaidThreatStreamName, seed, 1);
        return streams;
    }

    private static (RuntimeId<Actor> ConfederationActorId, RuntimeId<Region> RegionId) AddConfederation(
        WorldState state, LivingWorldActorStandingTrend trend, MilitaryStrengthBand band, RuntimeId<Region>? regionId = null)
    {
        var confederationRegionId = regionId ?? state.RegionIds.Issue();
        var confederationSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(confederationSettlementId, Settlement.Create(confederationSettlementId, confederationRegionId));

        var confederation = BanditConfederationCreationService.CreateAncientSeed(
            state, "Latrones", trend, new LivingWorldActorMilitaryStrength(band), confederationRegionId, confederationSettlementId);
        return (confederation.ActorId, confederationRegionId);
    }

    private static RuntimeId<Household> AddHousehold(WorldState state, RuntimeId<Region> regionId, long fundingDenarii = 100_000)
    {
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, location: settlementId, household: householdId));
        state.HouseholdHeadships.Add(householdId, new HouseholdHeadship(householdId, headId, new GameDate(0)));
        state.LedgerAccounts.Add(
            LedgerAccountKey.ForHousehold(householdId),
            new LedgerAccount(LedgerAccountKey.ForHousehold(householdId), Money.FromDenarii(fundingDenarii)));

        return householdId;
    }

    [Test]
    public void DeclaresTheHazardsPhaseAndReadWriteSet()
    {
        var system = new RaidThreatSystem();

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.Hazards));
            Assert.That(system.Reads, Is.EquivalentTo(new[] { "actors", "householdHeadships", "characters", "settlements", "estateSecurityInvestments" }));
            Assert.That(system.Writes, Is.EquivalentTo(new[] { "raidThreats", "raidThreatIds", "eventIds", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "actors" }));
        });
    }

    [Test]
    public void ATickWithNoBanditConfederationsReturnsNoEventsAndDoesNotThrow()
    {
        var state = new WorldState(new GameDate(0));
        AddHousehold(state, state.RegionIds.Issue());

        var events = new RaidThreatSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty);
            Assert.That(state.RaidThreats.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void AConfederationWithNoHouseholdsAnywhereReturnsNoEvents()
    {
        var state = new WorldState(new GameDate(0));
        AddConfederation(state, LivingWorldActorStandingTrend.Rising, MilitaryStrengthBand.Formidable);

        var events = new RaidThreatSystem().Tick(state, new MonthlyTickContext(new GameDate(1), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void ARegionMismatchedHouseholdIsNeverTargetedNoMatterHowManyMonthsPass()
    {
        var state = new WorldState(new GameDate(0));
        AddConfederation(state, LivingWorldActorStandingTrend.Rising, MilitaryStrengthBand.Formidable);
        AddHousehold(state, state.RegionIds.Issue());

        var system = new RaidThreatSystem();
        var streams = Streams(1);
        for (var month = 1; month <= 500; month++)
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));

        Assert.That(state.RaidThreats.Count, Is.EqualTo(0));
    }

    [Test]
    public void ASameRegionHouseholdEventuallyGetsRaidedWithConsistentOutcomeBookkeeping()
    {
        var state = new WorldState(new GameDate(0));
        var (confederationActorId, regionId) = AddConfederation(state, LivingWorldActorStandingTrend.Rising, MilitaryStrengthBand.Formidable);
        var householdId = AddHousehold(state, regionId);
        var account = LedgerAccountKey.ForHousehold(householdId);

        var system = new RaidThreatSystem();
        var streams = Streams(1);
        for (var month = 1; month <= 2000 && state.RaidThreats.Count == 0; month++)
        {
            state.LedgerAccounts.TryGet(account, out var before);
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            state.LedgerAccounts.TryGet(account, out var after);

            if (state.RaidThreats.Count == 0)
                continue;

            var raid = state.RaidThreats.InAscendingOrder().Single().Value;

            Assert.Multiple(() =>
            {
                Assert.That(raid.ConfederationActorId, Is.EqualTo(confederationActorId));
                Assert.That(raid.TargetHouseholdId, Is.EqualTo(householdId));
                Assert.That(raid.DefenderSecurityLevel, Is.EqualTo(EstateSecurityInvestment.MinValue));

                if (raid.Outcome == RaidOutcome.RaidSucceeded)
                {
                    Assert.That(raid.SpoilsLost, Is.GreaterThan(Money.Zero));
                    Assert.That(after!.Balance, Is.EqualTo(before!.Balance - raid.SpoilsLost));
                }
                else
                {
                    Assert.That(raid.SpoilsLost, Is.EqualTo(Money.Zero));
                    Assert.That(after!.Balance, Is.EqualTo(before!.Balance));
                }
            });
        }

        Assert.That(state.RaidThreats.Count, Is.GreaterThan(0), "No raid occurred within the test's month budget.");
    }
}
