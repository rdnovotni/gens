using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.Ledger;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 3 save round-trip coverage, mirroring <see cref="EventsSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class ActorsSaveRoundTripTests
{
    [Test]
    public void LivingWorldActorsRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var parentId = state.ActorIds.Issue();
        var parent = LivingWorldActor.Create(
            parentId,
            LivingWorldActorType.Gens,
            "Gens Valeria",
            LivingWorldActorTier.Noteworthy,
            LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient,
            parentActorId: null,
            new LivingWorldActorIdentity(EconomicIdentityTag.Mercantile, FactionTag.Popularist),
            dignitas: 42,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Wealthy, Money.FromDenarii(5_000)),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Notable, "force_placeholder"),
            state.RegionIds.Issue(),
            state.SettlementIds.Issue(),
            headCharacterId: state.CharacterIds.Issue());
        state.Actors.Add(parentId, parent);

        var cadetId = state.ActorIds.Issue();
        var cadet = LivingWorldActor.Create(
            cadetId,
            LivingWorldActorType.Gens,
            "Gens Valeria Minor",
            LivingWorldActorTier.Background,
            LivingWorldActorStandingTrend.Rising,
            LivingWorldActorOrigin.CadetBranch,
            parentActorId: parentId,
            LivingWorldActorIdentity.None,
            dignitas: 0,
            new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Negligible),
            parent.RegionId,
            parent.HomeSettlementId);
        state.Actors.Add(cadetId, cadet);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Actors.TryGet(parentId, out var restoredParent), Is.True);
            Assert.That(restoredParent, Is.EqualTo(parent));
            Assert.That(restored.Actors.TryGet(cadetId, out var restoredCadet), Is.True);
            Assert.That(restoredCadet, Is.EqualTo(cadet));
            Assert.That(restored.ActorIds.Peek, Is.EqualTo(state.ActorIds.Peek));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyPhase10Data()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.That(loaded.State.Actors.Count, Is.EqualTo(0));
        Assert.That(loaded.State.ActorIds.Peek, Is.EqualTo(0));
    }
}
