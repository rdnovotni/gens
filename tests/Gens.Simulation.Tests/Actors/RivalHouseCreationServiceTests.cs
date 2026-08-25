using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 4 ("rival-house creation... lazy character generation") coverage for the
/// three creation paths (§2.2).</summary>
public sealed class RivalHouseCreationServiceTests
{
    private static LivingWorldActorNetWorth ModestNetWorth => new(HouseholdWealthBand.Modest, Figure: null);
    private static LivingWorldActorMilitaryStrength ModestMilitary => new(MilitaryStrengthBand.Modest);

    [Test]
    public void CreateAncientSeedRegistersABackgroundActorWithNoParentAndNoHead()
    {
        var state = new WorldState(new GameDate(0));

        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Valeria", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            dignitas: 20, ModestNetWorth, ModestMilitary, state.RegionIds.Issue(), state.SettlementIds.Issue());

        Assert.Multiple(() =>
        {
            Assert.That(state.Actors.TryGet(actor.ActorId, out var stored), Is.True);
            Assert.That(stored, Is.EqualTo(actor));
            Assert.That(actor.OriginStory, Is.EqualTo(LivingWorldActorOrigin.Ancient));
            Assert.That(actor.Tier, Is.EqualTo(LivingWorldActorTier.Background));
            Assert.That(actor.ParentActorId, Is.Null);
            Assert.That(actor.HeadCharacterId, Is.Null);
        });
    }

    [Test]
    public void CreateNovusHomoAlwaysStartsRisingWithNegligibleMilitaryStrength()
    {
        var state = new WorldState(new GameDate(0));

        var actor = RivalHouseCreationService.CreateNovusHomo(
            state, "Aemilia", LivingWorldActorIdentity.None, dignitas: 2,
            ModestNetWorth, state.RegionIds.Issue(), state.SettlementIds.Issue());

        Assert.Multiple(() =>
        {
            Assert.That(actor.OriginStory, Is.EqualTo(LivingWorldActorOrigin.NovusHomo));
            Assert.That(actor.StandingTrend, Is.EqualTo(LivingWorldActorStandingTrend.Rising));
            Assert.That(actor.MilitaryStrength.Band, Is.EqualTo(MilitaryStrengthBand.Negligible));
            Assert.That(actor.ParentActorId, Is.Null);
        });
    }

    [Test]
    public void CreateCadetBranchRecordsTheParentAndInheritsItsRegionAndIdentity()
    {
        var state = new WorldState(new GameDate(0));
        var parent = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established,
            new LivingWorldActorIdentity(EconomicIdentityTag.Martial, FactionTag.Traditionalist),
            dignitas: 50, ModestNetWorth, ModestMilitary, state.RegionIds.Issue(), state.SettlementIds.Issue());

        var cadet = RivalHouseCreationService.CreateCadetBranch(state, parent, "Cornelia Minor");

        Assert.Multiple(() =>
        {
            Assert.That(cadet.OriginStory, Is.EqualTo(LivingWorldActorOrigin.CadetBranch));
            Assert.That(cadet.ParentActorId, Is.EqualTo(parent.ActorId));
            Assert.That(cadet.RegionId, Is.EqualTo(parent.RegionId));
            Assert.That(cadet.HomeSettlementId, Is.EqualTo(parent.HomeSettlementId));
            Assert.That(cadet.IdentityTags, Is.EqualTo(parent.IdentityTags));
        });
    }

    [Test]
    public void CreateCadetBranchStartsAlliedWithItsParentHouse()
    {
        var state = new WorldState(new GameDate(0));
        var parent = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            dignitas: 50, ModestNetWorth, ModestMilitary, state.RegionIds.Issue(), state.SettlementIds.Issue());

        var cadet = RivalHouseCreationService.CreateCadetBranch(state, parent, "Cornelia Minor");

        var standing = HouseStandingResolver.GetEffectiveStanding(state, parent.ActorId, cadet.ActorId);
        Assert.That(standing, Is.EqualTo(HouseStandingLevel.Allied));
    }
}
