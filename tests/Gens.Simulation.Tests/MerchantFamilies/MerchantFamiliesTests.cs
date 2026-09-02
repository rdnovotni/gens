using Gens.Simulation.Actors;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.MerchantFamilies;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.MerchantFamilies;

/// <summary>Phase 15 item 3 coverage — the Equestrian Order's computed wealth threshold (§2), the
/// Merchant House archetype extending Rival Houses' own Background/Notable framework (§4, §7), the
/// merchant-specific Senate entry path's Net Worth and Dignitas gates and its three named
/// Dignitas-investment actions (§6), and a save/load round trip
/// (<c>gens-merchant-families-design.md</c>).</summary>
public sealed class MerchantFamiliesTests
{
    private static (RuntimeId<Household> HouseholdId, RuntimeId<Region> RegionId, RuntimeId<Settlement> SettlementId) OneHousehold(WorldState state)
    {
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        var householdId = state.HouseholdIds.Issue();
        return (householdId, regionId, settlementId);
    }

    private static void SetNetWorth(WorldState state, RuntimeId<Household> householdId, Money total)
    {
        state.NetWorthAssessments.Add(householdId, new NetWorth(householdId, state.Date, total, Money.Zero, Money.Zero, total));
    }

    private static RuntimeId<Actor> RivalActor(WorldState state, RuntimeId<Region> regionId, RuntimeId<Settlement> settlementId, Money? netWorthFigure)
    {
        var actorId = state.ActorIds.Issue();
        var actor = LivingWorldActor.Create(
            actorId, LivingWorldActorType.Gens, "Valeria", LivingWorldActorTier.Noteworthy, LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Wealthy, netWorthFigure),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), regionId, settlementId);
        state.Actors.Add(actorId, actor);
        return actorId;
    }

    // ---- Equestrian Status (§2, computed) --------------------------------------------------------

    [Test]
    public void EquestrianStatusQueryQualifiesAPlayerHouseholdOnceNetWorthClearsTheThreshold()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);
        SetNetWorth(state, householdId, MerchantFamiliesCatalog.EquestrianNetWorthThreshold);

        var status = EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForPlayerHousehold(householdId));

        Assert.Multiple(() =>
        {
            Assert.That(status.QualifiesByNetWorth, Is.True);
            Assert.That(status.HoldsAngusticlavus, Is.True);
            Assert.That(status.EligibleForEquestrianOffices, Is.True);
            Assert.That(status.PublicaniEligible, Is.True);
        });
    }

    [Test]
    public void EquestrianStatusQueryDoesNotQualifyAHouseholdBelowTheThreshold()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);
        SetNetWorth(state, householdId, MerchantFamiliesCatalog.EquestrianNetWorthThreshold - Money.FromDenarii(1));

        var status = EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForPlayerHousehold(householdId));

        Assert.That(status, Is.EqualTo(EquestrianStatus.NotQualified));
    }

    [Test]
    public void EquestrianStatusQueryDoesNotQualifyAHouseholdWithNoNetWorthAssessmentYet()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);

        var status = EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForPlayerHousehold(householdId));

        Assert.That(status, Is.EqualTo(EquestrianStatus.NotQualified));
    }

    [Test]
    public void EquestrianStatusQueryReadsANoteworthyRivalGensOwnTrackedNetWorthFigure()
    {
        var state = new WorldState(new GameDate(0));
        var (_, regionId, settlementId) = OneHousehold(state);
        var richActorId = RivalActor(state, regionId, settlementId, MerchantFamiliesCatalog.EquestrianNetWorthThreshold);
        var poorActorId = RivalActor(state, regionId, settlementId, Money.FromDenarii(1));

        Assert.Multiple(() =>
        {
            Assert.That(EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForRivalGens(richActorId)).QualifiesByNetWorth, Is.True);
            Assert.That(EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForRivalGens(poorActorId)).QualifiesByNetWorth, Is.False);
        });
    }

    [Test]
    public void EquestrianStatusQueryDoesNotQualifyABackgroundTierRivalGensWithNoExactFigure()
    {
        var state = new WorldState(new GameDate(0));
        var (_, regionId, settlementId) = OneHousehold(state);
        var actorId = state.ActorIds.Issue();
        var actor = LivingWorldActor.Create(
            actorId, LivingWorldActorType.Gens, "Cornelia", LivingWorldActorTier.Background, LivingWorldActorStandingTrend.Established,
            LivingWorldActorOrigin.Ancient, parentActorId: null, LivingWorldActorIdentity.None,
            dignitas: 0, new LivingWorldActorNetWorth(HouseholdWealthBand.Wealthy, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest), regionId, settlementId);
        state.Actors.Add(actorId, actor);

        Assert.That(EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForRivalGens(actorId)).QualifiesByNetWorth, Is.False);
    }

    [Test]
    public void EquestrianStatusQueryDoesNotQualifyAnOwnerKindWithNoTrackedNetWorth()
    {
        var state = new WorldState(new GameDate(0));

        Assert.That(EquestrianStatusQuery.Current(state, PropertyOwnerRef.ForTemple("Temple of Diana")).QualifiesByNetWorth, Is.False);
    }

    // ---- Merchant House archetype (§4, §7) -------------------------------------------------------

    [Test]
    public void DesignateMerchantHouseCommandCreatesAnArchetypeForAPlayerHousehold()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);

        var result = DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, owner,
                MerchantHouseType.ShippingMagnate, TradeScaleTier.WholesaleOrImport));

        MerchantHouseArchetypeResolver.TryGetCurrent(state, owner, out var archetype);
        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(archetype.MerchantType, Is.EqualTo(MerchantHouseType.ShippingMagnate));
            Assert.That(archetype.WholesaleOrRetailTier, Is.EqualTo(TradeScaleTier.WholesaleOrImport));
        });
    }

    [Test]
    public void DesignateMerchantHouseCommandCreatesAnArchetypeForANoteworthyRivalGens()
    {
        var state = new WorldState(new GameDate(0));
        var (_, regionId, settlementId) = OneHousehold(state);
        var actorId = RivalActor(state, regionId, settlementId, netWorthFigure: null);
        var owner = PropertyOwnerRef.ForRivalGens(actorId);

        var result = DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "system", new GameDate(1), null, owner,
                MerchantHouseType.TaxFarmer, TradeScaleTier.WholesaleOrImport));

        Assert.That(result.Accepted, Is.True);
    }

    [Test]
    public void DesignateMerchantHouseCommandRejectsAnUnresolvableRivalGens()
    {
        var state = new WorldState(new GameDate(0));
        var unknownActorId = state.ActorIds.Issue();

        var result = DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertyOwnerRef.ForRivalGens(unknownActorId),
                MerchantHouseType.Negotiator, TradeScaleTier.Retail));

        Assert.That(result.Error, Is.EqualTo(DesignateMerchantHouseCommands.OwnerNotFound));
    }

    [Test]
    public void DesignateMerchantHouseCommandRejectsAnOwnerKindOutsideThePlayerHouseholdOrRivalGensRoster()
    {
        var state = new WorldState(new GameDate(0));

        var result = DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertyOwnerRef.ForTemple("Temple of Diana"),
                MerchantHouseType.Mercator, TradeScaleTier.Retail));

        Assert.That(result.Error, Is.EqualTo(DesignateMerchantHouseCommands.InvalidOwnerKind));
    }

    [Test]
    public void DesignateMerchantHouseCommandReplacesAnAlreadyDesignatedArchetype()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);
        var owner = PropertyOwnerRef.ForPlayerHousehold(householdId);

        DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, owner, MerchantHouseType.Mercator, TradeScaleTier.Retail));
        DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, owner, MerchantHouseType.FreedmanDynasty, TradeScaleTier.WholesaleOrImport));

        MerchantHouseArchetypeResolver.TryGetCurrent(state, owner, out var archetype);
        Assert.That(archetype.MerchantType, Is.EqualTo(MerchantHouseType.FreedmanDynasty));
    }

    // ---- Senate entry progress (§6) ---------------------------------------------------------------

    [Test]
    public void SenateEntryProgressQueryReadsBothGatesLiveOffNetWorthAndDignitas()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);

        var beforeAnything = SenateEntryProgressQuery.Current(state, householdId);

        SetNetWorth(state, householdId, MerchantFamiliesCatalog.SenateNetWorthThreshold);
        var afterNetWorthOnly = SenateEntryProgressQuery.Current(state, householdId);

        DignitasResolver.Apply(state, householdId, MerchantFamiliesCatalog.SenateDignitasThreshold);
        var afterBothGates = SenateEntryProgressQuery.Current(state, householdId);

        Assert.Multiple(() =>
        {
            Assert.That(beforeAnything.NetWorthGateCleared, Is.False);
            Assert.That(beforeAnything.DignitasGateCleared, Is.False);
            Assert.That(afterNetWorthOnly.NetWorthGateCleared, Is.True);
            Assert.That(afterNetWorthOnly.DignitasGateCleared, Is.False);
            Assert.That(afterBothGates.NetWorthGateCleared, Is.True);
            Assert.That(afterBothGates.DignitasGateCleared, Is.True);
        });
    }

    [Test]
    public void RecordDignitasInvestmentActionCommandAppliesTheRealDignitasAwardAndAppendsTheLog()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);

        var funded = RecordDignitasInvestmentActionCommands.Pipeline.Execute(
            state, new RecordDignitasInvestmentActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, DignitasInvestmentActionType.FundedGamesOrPublicWorks));
        var married = RecordDignitasInvestmentActionCommands.Pipeline.Execute(
            state, new RecordDignitasInvestmentActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, DignitasInvestmentActionType.StrategicMarriage));

        var progress = SenateEntryProgressQuery.Current(state, householdId);
        var expectedDignitas =
            MerchantFamiliesCatalog.DignitasEffectFor(DignitasInvestmentActionType.FundedGamesOrPublicWorks) +
            MerchantFamiliesCatalog.DignitasEffectFor(DignitasInvestmentActionType.StrategicMarriage);

        Assert.Multiple(() =>
        {
            Assert.That(funded.Accepted, Is.True);
            Assert.That(married.Accepted, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(expectedDignitas));
            Assert.That(progress.DignitasInvestmentActions, Has.Count.EqualTo(2));
            Assert.That(progress.DignitasInvestmentActions[0].ActionType, Is.EqualTo(DignitasInvestmentActionType.FundedGamesOrPublicWorks));
            Assert.That(progress.DignitasInvestmentActions[1].ActionType, Is.EqualTo(DignitasInvestmentActionType.StrategicMarriage));
        });
    }

    [Test]
    public void RecordDignitasInvestmentActionCommandRejectsAnUnrecognizedActionType()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, _, _) = OneHousehold(state);

        var result = RecordDignitasInvestmentActionCommands.Pipeline.Execute(
            state, new RecordDignitasInvestmentActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, (DignitasInvestmentActionType)99));

        Assert.That(result.Error, Is.EqualTo(RecordDignitasInvestmentActionCommands.UnrecognizedActionType));
    }

    // ---- Save/load round trip and deterministic hash stability -----------------------------------

    [Test]
    public void MerchantFamiliesStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(0));
        var (householdId, regionId, settlementId) = OneHousehold(state);
        var actorId = RivalActor(state, regionId, settlementId, MerchantFamiliesCatalog.EquestrianNetWorthThreshold);

        SetNetWorth(state, householdId, MerchantFamiliesCatalog.SenateNetWorthThreshold);

        DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, PropertyOwnerRef.ForPlayerHousehold(householdId),
                MerchantHouseType.ShippingMagnate, TradeScaleTier.WholesaleOrImport));
        DesignateMerchantHouseCommands.Pipeline.Execute(
            state, new DesignateMerchantHouseCommand(
                state.CommandIds.Issue(), "system", new GameDate(1), null, PropertyOwnerRef.ForRivalGens(actorId),
                MerchantHouseType.TaxFarmer, TradeScaleTier.Retail));
        RecordDignitasInvestmentActionCommands.Pipeline.Execute(
            state, new RecordDignitasInvestmentActionCommand(
                state.CommandIds.Issue(), "player", new GameDate(2), null, householdId, DignitasInvestmentActionType.LocalMagistracy));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.MerchantHouseArchetypes.Count, Is.EqualTo(2));
            Assert.That(restored.SenateEntryInvestmentLogs.Count, Is.EqualTo(1));

            MerchantHouseArchetypeResolver.TryGetCurrent(restored, PropertyOwnerRef.ForPlayerHousehold(householdId), out var restoredArchetype);
            Assert.That(restoredArchetype.MerchantType, Is.EqualTo(MerchantHouseType.ShippingMagnate));

            var restoredProgress = SenateEntryProgressQuery.Current(restored, householdId);
            Assert.That(restoredProgress.DignitasInvestmentActions, Has.Count.EqualTo(1));
            Assert.That(restoredProgress.NetWorthGateCleared, Is.True);
        });
    }
}
