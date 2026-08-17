using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 8 items 5-8 save round-trip coverage, mirroring <see
/// cref="LedgerAndMarketSaveRoundTripTests"/>'s identical pattern.</summary>
public sealed class EconomySaveRoundTripTests
{
    [Test]
    public void EveryNewPhase8Items5Through7PartitionRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();

        var statement = new HouseholdMonthlyStatement(householdId, new GameDate(5), Money.FromDenarii(10), Money.FromDenarii(4), Money.FromDenarii(6));
        state.HouseholdStatements.Add(householdId, statement);

        var debt = DebtService.IssueLoan(state, new GameDate(5), settlementId, householdId, Money.FromDenarii(200));

        var netWorth = new NetWorth(householdId, new GameDate(5), Money.FromDenarii(200), Money.FromDenarii(50), Money.FromDenarii(200), Money.FromDenarii(50));
        state.NetWorthAssessments.Add(householdId, netWorth);

        var insolvency = new InsolvencyState(householdId, 2, InsolvencyStage.AtRisk, new[] { InsolvencyConsequence.ForcedLiquidation });
        state.InsolvencyStates.Add(householdId, insolvency);

        var tradeRoute = StandingContractService.CommitToTradeRoute(
            state, new GameDate(5), settlementId, householdId, "Test Route", Money.FromDenarii(75));

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseholdStatements.TryGet(householdId, out var restoredStatement), Is.True);
            Assert.That(restoredStatement, Is.EqualTo(statement));

            Assert.That(restored.DebtRecords.TryGet(debt.Id, out var restoredDebt), Is.True);
            Assert.That(restoredDebt, Is.EqualTo(debt));
            Assert.That(restored.DebtRecordIds.Peek, Is.EqualTo(state.DebtRecordIds.Peek));

            Assert.That(restored.NetWorthAssessments.TryGet(householdId, out var restoredNetWorth), Is.True);
            Assert.That(restoredNetWorth, Is.EqualTo(netWorth));

            Assert.That(restored.InsolvencyStates.TryGet(householdId, out var restoredInsolvency), Is.True);
            Assert.That(restoredInsolvency!.HouseholdId, Is.EqualTo(insolvency.HouseholdId));
            Assert.That(restoredInsolvency.MonthsBelowThreshold, Is.EqualTo(insolvency.MonthsBelowThreshold));
            Assert.That(restoredInsolvency.Stage, Is.EqualTo(insolvency.Stage));
            Assert.That(restoredInsolvency.ConsequencesApplied, Is.EqualTo(insolvency.ConsequencesApplied));

            Assert.That(restored.StandingContracts.TryGet(tradeRoute.Id, out var restoredContract), Is.True);
            Assert.That(restoredContract, Is.EqualTo(tradeRoute));
            Assert.That(restored.StandingContractIds.Peek, Is.EqualTo(state.StandingContractIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyPhase8Items5Through7Data()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.State.HouseholdStatements.Count, Is.EqualTo(0));
            Assert.That(loaded.State.DebtRecords.Count, Is.EqualTo(0));
            Assert.That(loaded.State.NetWorthAssessments.Count, Is.EqualTo(0));
            Assert.That(loaded.State.InsolvencyStates.Count, Is.EqualTo(0));
            Assert.That(loaded.State.StandingContracts.Count, Is.EqualTo(0));
            Assert.That(loaded.State.DebtRecordIds.Peek, Is.EqualTo(0));
            Assert.That(loaded.State.StandingContractIds.Peek, Is.EqualTo(0));
        });
    }
}
