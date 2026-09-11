using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Crime;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 16 item 2 save round-trip coverage, mirroring <see
/// cref="SpyPlacementSaveRoundTripTests"/>'s identical pattern.</summary>
public sealed class RaidThreatSaveRoundTripTests
{
    [Test]
    public void RaidThreatRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var confederationActorId = state.ActorIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        var raidId = state.RaidThreatIds.Issue();

        var raid = RaidThreat.Create(
            raidId, confederationActorId, householdId, RaidTargetType.Settlement,
            defenderSecurityLevel: 35, RaidOutcome.RaidSucceeded, Money.FromDenarii(275), new GameDate(5));
        state.RaidThreats.Add(raidId, raid);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.RaidThreats.TryGet(raidId, out var restoredRaid), Is.True);
            Assert.That(restoredRaid, Is.EqualTo(raid));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void EstateSecurityInvestmentRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var householdId = state.HouseholdIds.Issue();

        var investment = EstateSecurityInvestment.CreateUnguarded(householdId, new GameDate(1)).WithLevel(60, new GameDate(4));
        state.EstateSecurityInvestments.Add(householdId, investment);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.EstateSecurityInvestments.TryGet(householdId, out var restoredInvestment), Is.True);
            Assert.That(restoredInvestment, Is.EqualTo(investment));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    /// <summary>Phase 16 item 6 coverage: a raid-captured raider (<see cref="CharacterSource.RaidCaptured"/>,
    /// a new enum value) and the Crime <see cref="DetentionRecord"/> opened for them both round-trip.</summary>
    [Test]
    public void RaidCapturedCharacterAndDetentionRecordRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var settlementId = state.SettlementIds.Issue();
        var streams = new RandomStreamSet();
        streams.Add(CampaignBootstrapper.RaidThreatStreamName, seed: 7, stream: 1);

        var captive = RaidCaptiveGenerator.GenerateCaptive(state, streams, CampaignBootstrapper.RaidThreatStreamName, settlementId, new GameDate(5));
        var detentionId = state.DetentionRecordIds.Issue();
        var detention = new DetentionRecord(detentionId, captive.Id, DetentionLocationType.PrivateErgastulum, new GameDate(5), Justified: true);
        state.DetentionRecords.Add(detentionId, detention);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Characters.TryGet(captive.Id, out var restoredCaptive), Is.True);
            Assert.That(restoredCaptive!.Source, Is.EqualTo(CharacterSource.RaidCaptured));
            Assert.That(restored.DetentionRecords.TryGet(detentionId, out var restoredDetention), Is.True);
            Assert.That(restoredDetention, Is.EqualTo(detention));
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(state)));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }
}
