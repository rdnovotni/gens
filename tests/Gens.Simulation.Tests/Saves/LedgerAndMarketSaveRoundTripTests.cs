using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 8 items 1-4 save round-trip coverage, mirroring <see cref="WorldStateMapperTests"/>'
/// "every field populated" pattern.</summary>
public sealed class LedgerAndMarketSaveRoundTripTests
{
    [Test]
    public void LedgerAccountsAndTransactionsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(10));
        var householdId = state.HouseholdIds.Issue();
        var household = LedgerAccountKey.ForHousehold(householdId);

        LedgerService.Post(
            state, new GameDate(10), LedgerTransactionCategory.Sales,
            new[]
            {
                new LedgerPosting(household, Money.FromDenarii(42)),
                new LedgerPosting(LedgerAccountKey.Mint, Money.FromDenarii(-42)),
            },
            reference: "harvest-sale");

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.LedgerAccounts.TryGet(household, out var account), Is.True);
            Assert.That(account!.Balance, Is.EqualTo(Money.FromDenarii(42)));
            Assert.That(restored.LedgerTransactions.Count, Is.EqualTo(1));
            Assert.That(restored.LedgerTransactionIds.Peek, Is.EqualTo(state.LedgerTransactionIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void SettlementMarketsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var goodId = new DefinitionId<Good>("grain");
        var market = new SettlementMarket(
            settlementId, goodId, Money.FromDenarii(11), Money.FromDenarii(10), supply: 5, demand: 8, clearedQuantity: 5, unsatisfiedDemand: 3);
        state.MarketPrices.Add(new MarketGoodKey(settlementId, goodId), market);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.That(restored.MarketPrices.TryGet(new MarketGoodKey(settlementId, goodId), out var restoredMarket), Is.True);
        Assert.That(restoredMarket, Is.EqualTo(market));

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyLedgerOrMarketData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.LedgerAccounts.Count, Is.EqualTo(0));
        Assert.That(loaded.State.LedgerTransactions.Count, Is.EqualTo(0));
        Assert.That(loaded.State.MarketPrices.Count, Is.EqualTo(0));
        Assert.That(loaded.State.LedgerTransactionIds.Peek, Is.EqualTo(0));
    }
}
