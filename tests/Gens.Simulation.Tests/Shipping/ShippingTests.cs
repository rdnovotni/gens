using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Goods;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.Random;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.Shipping;
using Gens.Simulation.Societates;
using Gens.Simulation.State;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Shipping;

/// <summary>Phase 15 item 8 coverage: Custom Commissioning and its maritime-settlement gate (§3), the
/// §3.2 Consecrated Launch Funded Action, §4 Flagship designation, §5 Sole/Societas/Fronted ownership,
/// §7 upkeep/condition decay and Repair, §6.2/§8's real Storm-driven discrete Voyage Event resolution
/// (including fenus nauticum debt forgiveness and §9's lucky-ship/bad-reputation reputation tiers), and
/// a save/load round trip with deterministic state hash stability
/// (<c>gens-private-ships-shipping-ventures-design.md</c>).</summary>
public sealed class ShippingTests
{
    private static (WorldState State, RuntimeId<Settlement> SettlementId) MaritimeSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Coast, TerrainFeature.None));
        return (state, settlementId);
    }

    private static (WorldState State, RuntimeId<Settlement> SettlementId) InlandSettlement()
    {
        var state = new WorldState(new GameDate(0));
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.FertilePlain, TerrainFeature.None));
        return (state, settlementId);
    }

    private static void Fund(WorldState state, RuntimeId<Household> householdId, Money amount) =>
        LedgerService.Post(
            state, state.Date, LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), amount), new LedgerPosting(LedgerAccountKey.Mint, -amount) });

    private static Money BalanceOf(WorldState state, RuntimeId<Household> householdId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;

    private static (RuntimeId<Household> HouseholdId, RuntimeId<Character> HeadId) HouseholdWithHead(WorldState state)
    {
        var householdId = state.HouseholdIds.Issue();
        var headId = state.CharacterIds.Issue();
        state.Characters.Add(headId, CharacterTestFixtures.Minimal(headId, nomen: "Cornelius", household: householdId));
        return (householdId, headId);
    }

    private static void GivePatron(WorldState state, RuntimeId<Household> householdId, RuntimeId<Character> headId) =>
        SetPatronDeityCommands.Pipeline.Execute(
            state, new SetPatronDeityCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, PatronDeity.Jupiter, headId));

    private static RuntimeId<ShipCommissionProject> StartCommission(
        WorldState state, RuntimeId<Household> householdId, RuntimeId<Settlement> settlementId,
        ShipVesselClass vesselClass = ShipVesselClass.Corbita, GoodQuality quality = GoodQuality.Common,
        bool consecratedLaunch = false, ShipOwnershipMode ownershipMode = ShipOwnershipMode.Sole,
        RuntimeId<Societas>? societasId = null, PropertyOwnerRef? frontingRef = null)
    {
        var result = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna", vesselClass, quality,
                "a painted eye on the bow", consecratedLaunch, ownershipMode, societasId, frontingRef));
        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        return state.ShipCommissionProjects.InAscendingOrder().Last().Key;
    }

    private static MerchantShip RunCommissionToCompletion(
        WorldState state, RuntimeId<ShipCommissionProject> projectId, ShipVesselClass vesselClass, GoodQuality quality)
    {
        var durationMonths = ShippingCatalog.CommissionDurationMonths(ShippingCatalog.CapacityTierFor(vesselClass));
        for (var month = 1; month <= durationMonths; month++)
            ShipCommissionResolutionSystem.Tick(state, new GameDate(month));

        state.ShipCommissionProjects.TryGet(projectId, out var project);
        Assert.That(project!.Status, Is.EqualTo(ShipCommissionStatus.Completed));
        state.MerchantShips.TryGet(project.ResultingShipId!.Value, out var ship);
        return ship!;
    }

    // ---- §3 Custom Commissioning ------------------------------------------------------------

    [Test]
    public void CommissionShipRejectsANonMaritimeSettlement()
    {
        var (state, settlementId) = InlandSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var result = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Sole));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(CommissionShipCommands.SettlementNotMaritime));
    }

    [Test]
    public void CommissionShipAcceptsACoastalSettlement()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var projectId = StartCommission(state, householdId, settlementId);

        Assert.That(state.ShipCommissionProjects.TryGet(projectId, out var project), Is.True);
        Assert.That(project!.Status, Is.EqualTo(ShipCommissionStatus.InProgress));
    }

    [Test]
    public void CommissionShipRequiresAPatronDeityForAConsecratedLaunchRequest()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var result = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", ConsecratedLaunchRequested: true, ShipOwnershipMode.Sole));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(CommissionShipCommands.NoPatronDeityForConsecratedLaunch));
    }

    [Test]
    public void CommissionShipRequiresAValidSocietasWhenOwnershipModeIsSocietas()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var missing = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Societas));
        Assert.That(missing.Error, Is.EqualTo(CommissionShipCommands.SocietasRequired));

        var neverRegisteredSocietasId = state.SocietasIds.Issue();
        var bogus = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Societas, neverRegisteredSocietasId));
        Assert.That(bogus.Error, Is.EqualTo(CommissionShipCommands.SocietasNotFound));
    }

    [Test]
    public void CommissionShipRejectsASocietasTheCommissioningHouseholdIsNotAPartnerOf()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        var (otherHouseholdId, _) = HouseholdWithHead(state);
        var (thirdHouseholdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var formResult = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "A shipping venture between others",
                new[]
                {
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(otherHouseholdId), Fixed64.FromRaw(500_000)),
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(thirdHouseholdId), Fixed64.FromRaw(500_000)),
                }));
        Assert.That(formResult.Accepted, Is.True, $"Rejected: {formResult.Error}");
        var societasId = formResult.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Societas, societasId));

        Assert.That(result.Accepted, Is.False, "Any household could otherwise name an unrelated active Societas as owner.");
        Assert.That(result.Error, Is.EqualTo(CommissionShipCommands.HouseholdNotPartner));
    }

    [Test]
    public void CommissionShipAcceptsASocietasTheCommissioningHouseholdIsAPartnerOf()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        var (otherHouseholdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var formResult = FormSocietasCommands.Pipeline.Execute(
            state, new FormSocietasCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null,
                PartnershipType.UnusRei, SocietasGovernanceModel.EqualPartners, "A shared shipping venture",
                new[]
                {
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(householdId), Fixed64.FromRaw(500_000)),
                    new SocietasPartner(PropertyOwnerRef.ForPlayerHousehold(otherHouseholdId), Fixed64.FromRaw(500_000)),
                }));
        Assert.That(formResult.Accepted, Is.True, $"Rejected: {formResult.Error}");
        var societasId = formResult.Events.OfType<SocietasFormedEvent>().Single().SocietasId;

        var result = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Societas, societasId));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
    }

    [Test]
    public void CommissionShipRejectsAFrontingKindOutsideAFreedmanCharacterOrASocietas()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));

        var romanState = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Fronted,
                FrontingPersonOrSocietasId: PropertyOwnerRef.RomanState));
        Assert.That(romanState.Error, Is.EqualTo(CommissionShipCommands.FrontingKindNotSupported));

        var playerHousehold = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Fronted,
                FrontingPersonOrSocietasId: PropertyOwnerRef.ForPlayerHousehold(householdId)));
        Assert.That(playerHousehold.Error, Is.EqualTo(CommissionShipCommands.FrontingKindNotSupported));

        var nonexistentCharacter = CommissionShipCommands.Pipeline.Execute(
            state, new CommissionShipCommand(
                state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, settlementId, "The Fortuna",
                ShipVesselClass.Corbita, GoodQuality.Common, "plain", false, ShipOwnershipMode.Fronted,
                FrontingPersonOrSocietasId: PropertyOwnerRef.ForIndividualCharacter(state.CharacterIds.Issue())));
        Assert.That(nonexistentCharacter.Error, Is.EqualTo(CommissionShipCommands.FrontingCharacterNotFound));
    }

    [Test]
    public void ShipCommissionResolutionAdvancesOnlyWhenPaidAndCreatesTheShipOnCompletion()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(2000));

        var projectId = StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita, GoodQuality.Fine);
        var durationMonths = ShippingCatalog.CommissionDurationMonths(ShipCapacityTier.Standard);

        // Drain the household to zero right after the first paid month, forcing a stall.
        var remaining = BalanceOf(state, householdId);
        LedgerService.Post(
            state, new GameDate(1), LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -remaining), new LedgerPosting(LedgerAccountKey.Mint, remaining) });
        ShipCommissionResolutionSystem.Tick(state, new GameDate(1)); // Unpaid: stalls.
        state.ShipCommissionProjects.TryGet(projectId, out var stalled);
        Assert.That(stalled!.MonthsInvested, Is.EqualTo(0));

        Fund(state, householdId, Money.FromDenarii(2000));
        var ship = RunCommissionToCompletion(state, projectId, ShipVesselClass.Corbita, GoodQuality.Fine);

        Assert.Multiple(() =>
        {
            Assert.That(ship.Name, Is.EqualTo("The Fortuna"));
            Assert.That(ship.VesselClass, Is.EqualTo(ShipVesselClass.Corbita));
            Assert.That(ship.Status, Is.EqualTo(ShipStatus.Active));
            Assert.That(ship.Condition.Value, Is.EqualTo(ShippingCatalog.StartingCondition(GoodQuality.Fine)));
            Assert.That(ship.BlessedLaunch, Is.False);
            Assert.That(ship.ActualOwnerHouseholdId, Is.EqualTo(householdId));
        });
        Assert.That(durationMonths, Is.GreaterThan(1));
    }

    [Test]
    public void ConsecratedLaunchGrantsBlessedLaunchFavorAndDignitasWhenFunded()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, headId) = HouseholdWithHead(state);
        GivePatron(state, householdId, headId);
        Fund(state, householdId, Money.FromDenarii(5000));

        var projectId = StartCommission(
            state, householdId, settlementId, ShipVesselClass.NavisCaudicaria, GoodQuality.Common, consecratedLaunch: true);
        var dignitasBefore = DignitasResolver.Current(state, householdId);

        var ship = RunCommissionToCompletion(state, projectId, ShipVesselClass.NavisCaudicaria, GoodQuality.Common);

        Assert.Multiple(() =>
        {
            Assert.That(ship.BlessedLaunch, Is.True);
            Assert.That(DignitasResolver.Current(state, householdId), Is.GreaterThan(dignitasBefore));
        });
    }

    [Test]
    public void FrontedOwnershipRecordsARealFrontingArrangementNeverExposed()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        var freedmanId = state.CharacterIds.Issue();
        state.Characters.Add(freedmanId, CharacterTestFixtures.Minimal(freedmanId, nomen: "Libertus"));
        Fund(state, householdId, Money.FromDenarii(5000));

        var projectId = StartCommission(
            state, householdId, settlementId, ShipVesselClass.Corbita, GoodQuality.Common, consecratedLaunch: false,
            ShipOwnershipMode.Fronted, frontingRef: PropertyOwnerRef.ForIndividualCharacter(freedmanId));
        var ship = RunCommissionToCompletion(state, projectId, ShipVesselClass.Corbita, GoodQuality.Common);

        Assert.That(FrontingArrangementResolver.TryGetCurrent(state, ship.Id, out var arrangement), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(ship.OwnershipMode, Is.EqualTo(ShipOwnershipMode.Fronted));
            Assert.That(ship.ActualOwnerHouseholdId, Is.EqualTo(householdId), "The real beneficial owner is always tracked (§11).");
            Assert.That(arrangement.RealOwnerHouseholdId, Is.EqualTo(householdId));
            Assert.That(arrangement.FrontingPersonOrSocietasId, Is.EqualTo(PropertyOwnerRef.ForIndividualCharacter(freedmanId)));
            Assert.That(arrangement.Exposed, Is.False, "No live exposure trigger exists in this item (§5's own honest narrowing).");
        });
    }

    // ---- §4 Flagship --------------------------------------------------------------------------

    [Test]
    public void DesignateFlagshipUnsetsThePreviousFlagshipAndAwardsDignitasOnce()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(10_000));

        var firstShip = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Liburnian), ShipVesselClass.Liburnian, GoodQuality.Common);
        var secondShip = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Liburnian), ShipVesselClass.Liburnian, GoodQuality.Common);

        DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, firstShip.Id));
        var dignitasAfterFirst = DignitasResolver.Current(state, householdId);
        var result = DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, secondShip.Id));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        state.MerchantShips.TryGet(firstShip.Id, out var firstAfter);
        state.MerchantShips.TryGet(secondShip.Id, out var secondAfter);
        Assert.Multiple(() =>
        {
            Assert.That(firstAfter!.IsFlagship, Is.False, "Only ever holds one Flagship at a time (§4).");
            Assert.That(secondAfter!.IsFlagship, Is.True);
            Assert.That(MerchantMarineQuery.FlagshipOf(state, householdId)!.Id, Is.EqualTo(secondShip.Id));
            Assert.That(dignitasAfterFirst, Is.GreaterThan(0));

            // The Dignitas award is a real, one-time household achievement, not a repeatable grant — a
            // household re-designating the title onto a different owned Ship earns no further Dignitas
            // (Codex review finding, PR #97): only the household's own first-ever designation pays out.
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasAfterFirst));
        });
    }

    [Test]
    public void DesignateFlagshipDoesNotReawardDignitasWhenAlternatingBetweenTwoOwnedShips()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(10_000));

        var firstShip = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Liburnian), ShipVesselClass.Liburnian, GoodQuality.Common);
        var secondShip = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Liburnian), ShipVesselClass.Liburnian, GoodQuality.Common);

        DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, firstShip.Id));
        DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, secondShip.Id));
        var dignitasAfterTwoSwaps = DignitasResolver.Current(state, householdId);

        // Alternate the title back onto the first Ship a second time — this is the exact farming pattern
        // the fix guards against.
        var result = DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, householdId, firstShip.Id));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        Assert.That(
            DignitasResolver.Current(state, householdId), Is.EqualTo(dignitasAfterTwoSwaps),
            "Repeatedly alternating the Flagship title between two owned Ships must not re-earn the one-time award.");
    }

    [Test]
    public void DesignateFlagshipRejectsAShipTheHouseholdDoesNotOwn()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (ownerHouseholdId, _) = HouseholdWithHead(state);
        var otherHouseholdId = state.HouseholdIds.Issue();
        Fund(state, ownerHouseholdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, ownerHouseholdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);

        var result = DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(0), null, otherHouseholdId, ship.Id));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(DesignateFlagshipCommands.NotOwned));
    }

    // ---- §7 Upkeep, condition, and Repair -----------------------------------------------------

    [Test]
    public void UnpaidUpkeepCostsConditionScaledByBuildQuality()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita, GoodQuality.Common), ShipVesselClass.Corbita,
            GoodQuality.Common);

        // Drain the household so this month's upkeep goes unpaid.
        var balance = BalanceOf(state, householdId);
        LedgerService.Post(
            state, new GameDate(1), LedgerTransactionCategory.Treasury,
            new[] { new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -balance), new LedgerPosting(LedgerAccountKey.Mint, balance) });

        ShipUpkeepSystem.Tick(state, new GameDate(1));

        state.MerchantShips.TryGet(ship.Id, out var after);
        Assert.That(
            after!.Condition.Value,
            Is.EqualTo(ship.Condition.Value - ShippingCatalog.UnpaidUpkeepConditionLoss(GoodQuality.Common)));
    }

    [Test]
    public void PaidUpkeepChargesTheOwningHouseholdAndLeavesConditionUnchanged()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);
        var balanceBefore = BalanceOf(state, householdId);

        ShipUpkeepSystem.Tick(state, new GameDate(1));

        state.MerchantShips.TryGet(ship.Id, out var after);
        var upkeep = ShippingCatalog.MonthlyUpkeep(ShippingCatalog.CapacityTierFor(ShipVesselClass.Corbita));
        Assert.Multiple(() =>
        {
            Assert.That(BalanceOf(state, householdId), Is.EqualTo(balanceBefore - upkeep));
            Assert.That(after!.Condition.Value, Is.EqualTo(ship.Condition.Value));
        });
    }

    [Test]
    public void RepairShipRestoresConditionAndChargesTheLedger()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita, GoodQuality.Common), ShipVesselClass.Corbita,
            GoodQuality.Common);
        state.MerchantShips.TryGet(ship.Id, out var current);
        state.MerchantShips.Remove(ship.Id);
        state.MerchantShips.Add(ship.Id, current! with { Condition = new LandCondition(30) });

        var result = RepairShipCommands.Pipeline.Execute(
            state, new RepairShipCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, ship.Id, householdId));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        state.MerchantShips.TryGet(ship.Id, out var repaired);
        Assert.That(repaired!.Condition.Value, Is.EqualTo(Math.Min(100, 30 + ShippingCatalog.RepairConditionRestored)));
    }

    // ---- §6.1 Trade Route assignment -----------------------------------------------------------

    [Test]
    public void AssignShipToTradeRouteRejectsAPontoSinceItIsNotATradeVessel()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Ponto), ShipVesselClass.Ponto, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));

        var result = AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignShipToTradeRouteCommands.NotATradeVessel));
    }

    [Test]
    public void AssignShipToTradeRouteSucceedsForAnOrdinaryCargoVessel()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));

        var result = AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        state.MerchantShips.TryGet(ship.Id, out var assigned);
        Assert.That(assigned!.AssignedTradeRouteId, Is.EqualTo(routeId));
    }

    [Test]
    public void AssignShipToTradeRouteRejectsAnEndedRoute()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Ended,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));

        var result = AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(AssignShipToTradeRouteCommands.TradeRouteNotActive));
    }

    [Test]
    public void AnEndedTradeRouteNeverQualifiesAShipForAVoyageEvent()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));
        AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));

        // The Ship is a Flagship (so it would otherwise qualify), but the route it's still pointed at has
        // since ended.
        state.MerchantShips.TryGet(ship.Id, out var current);
        state.MerchantShips.Remove(ship.Id);
        state.MerchantShips.Add(ship.Id, current! with { IsFlagship = true });
        state.StandingContracts.Remove(routeId);
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Ended,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));

        var streams = new RandomStreamSet();
        streams.AddDerived(ShipVoyageRiskSystem.StreamName, rootSeed: 1);
        var events = ShipVoyageRiskSystem.Tick(
            state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Catastrophic, new GameDate(2)) }, streams);

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty, "An ended Trade Route must never keep generating Voyage Events.");
            Assert.That(state.VoyageEvents.Count, Is.EqualTo(0));
        });
    }

    // ---- §6.2/§8 Discrete Voyage Events (Storm) ------------------------------------------------

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, MerchantShip Ship, RuntimeId<DebtRecord> DebtId)
        FlagshipOnAFenusNauticumFinancedRoute()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);

        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Alexandria run"));
        AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));

        var debt = DebtService.IssueLoan(state, new GameDate(1), settlementId, householdId, Money.FromDenarii(500), isFenusNauticum: true);
        state.MerchantShips.TryGet(ship.Id, out var current);
        state.MerchantShips.Remove(ship.Id);
        // Pristine condition, so ShippingCatalog.ConditionRiskMultiplier stays neutral (1.0x) here — this
        // fixture is about the fenus nauticum/Storm mechanics, not §7's condition-based risk, which has
        // its own dedicated coverage below.
        var financed = current! with { FenusNauticumRecordId = debt.Id, Condition = LandCondition.Pristine };
        state.MerchantShips.Add(ship.Id, financed);

        return (state, settlementId, householdId, financed, debt.Id);
    }

    private static DisasterEventOccurredEvent StormEvent(WorldState state, RuntimeId<Settlement> settlementId, DisasterSeverity severity, GameDate date) =>
        new(state.EventIds.Issue(), date, settlementId, state.DisasterEventIds.Issue(), HazardType.Storm, severity, TriggeredByCompounding: false);

    private static (WorldState State, RuntimeId<Settlement> SettlementId, RuntimeId<Household> HouseholdId, MerchantShip Ship)
        ShipAssignedToARoute(ShipVesselClass vesselClass = ShipVesselClass.Corbita)
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ship = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, vesselClass), vesselClass, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Alexandria run"));
        AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, routeId));
        return (state, settlementId, householdId, ship);
    }

    [Test]
    public void FinanceVoyageWithFenusNauticumAttachesARealLoanToTheShip()
    {
        var (state, _, householdId, ship) = ShipAssignedToARoute();

        var result = ShippingCommands.Pipeline.Execute(
            state, new FinanceVoyageWithFenusNauticumCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, Money.FromDenarii(300)));

        Assert.That(result.Accepted, Is.True, $"Rejected: {result.Error}");
        state.MerchantShips.TryGet(ship.Id, out var financed);
        Assert.That(financed!.FenusNauticumRecordId, Is.Not.Null);
        state.DebtRecords.TryGet(financed.FenusNauticumRecordId!.Value, out var debt);
        Assert.Multiple(() =>
        {
            Assert.That(debt, Is.Not.Null);
            Assert.That(debt!.IsFenusNauticum, Is.True);
            Assert.That(debt.Principal, Is.EqualTo(Money.FromDenarii(300)));
            Assert.That(debt.DebtorHouseholdId, Is.EqualTo(householdId));
        });
    }

    [Test]
    public void FinanceVoyageWithFenusNauticumRejectsAShipAlreadyFinanced()
    {
        var (state, settlementId, householdId, ship, _) = FlagshipOnAFenusNauticumFinancedRoute();

        var result = ShippingCommands.Pipeline.Execute(
            state, new FinanceVoyageWithFenusNauticumCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, Money.FromDenarii(100)));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(ShippingCommands.AlreadyFinanced));
        _ = settlementId;
    }

    [Test]
    public void ARealFenusNauticumLoanAttachedThroughTheCommandForgivesOnALostVoyage()
    {
        WorldState? finalState = null;
        RuntimeId<DebtRecord> debtId = default;
        RuntimeId<MerchantShip> shipId = default;

        for (var seed = 1UL; seed <= 80UL && finalState is null; seed++)
        {
            var (state, settlementId, householdId, ship) = ShipAssignedToARoute();
            var financeResult = ShippingCommands.Pipeline.Execute(
                state, new FinanceVoyageWithFenusNauticumCommand(
                    state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id, Money.FromDenarii(300)));
            Assert.That(financeResult.Accepted, Is.True, $"Rejected: {financeResult.Error}");
            state.MerchantShips.TryGet(ship.Id, out var financed);
            state.MerchantShips.Remove(ship.Id);
            state.MerchantShips.Add(ship.Id, financed! with { Condition = LandCondition.Pristine });

            var streams = new RandomStreamSet();
            streams.AddDerived(ShipVoyageRiskSystem.StreamName, seed);
            ShipVoyageRiskSystem.Tick(
                state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Catastrophic, new GameDate(2)) }, streams);

            state.MerchantShips.TryGet(ship.Id, out var after);
            if (after!.Status != ShipStatus.LostToStorm)
                continue;

            finalState = state;
            debtId = financed.FenusNauticumRecordId!.Value;
            shipId = ship.Id;
        }

        Assert.That(finalState, Is.Not.Null, "Expected at least one seed to resolve LostToStorm for the financed Ship.");
        finalState!.DebtRecords.TryGet(debtId, out var debt);
        Assert.That(debt!.Status, Is.EqualTo(DebtStatus.Forgiven), "A Ship lost while fenus-nauticum-financed forgives the debt (§8).");
        _ = shipId;
    }

    [Test]
    public void APoorlyMaintainedShipCarriesElevatedVoyageRiskComparedToAPristineOne()
    {
        int SafeArrivalsAcrossSeeds(int condition)
        {
            var safeCount = 0;
            for (var seed = 1UL; seed <= 200UL; seed++)
            {
                var (state, settlementId, householdId, ship, _) = FlagshipOnAFenusNauticumFinancedRoute();
                state.MerchantShips.TryGet(ship.Id, out var current);
                state.MerchantShips.Remove(ship.Id);
                state.MerchantShips.Add(ship.Id, current! with { IsFlagship = true, Condition = new LandCondition(condition) });

                var streams = new RandomStreamSet();
                streams.AddDerived(ShipVoyageRiskSystem.StreamName, seed);
                ShipVoyageRiskSystem.Tick(
                    state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Severe, new GameDate(2)) }, streams);

                state.MerchantShips.TryGet(ship.Id, out var after);
                if (after!.Status == ShipStatus.Active && after.VoyagesCompleted > 0)
                    safeCount++;
                _ = householdId;
            }

            return safeCount;
        }

        var pristineSafeArrivals = SafeArrivalsAcrossSeeds(condition: 100);
        var wreckedSafeArrivals = SafeArrivalsAcrossSeeds(condition: 10);

        Assert.That(
            wreckedSafeArrivals, Is.LessThan(pristineSafeArrivals),
            "A poorly-maintained Ship must carry a real, elevated Voyage Event risk (§7) compared to a pristine one.");
    }

    [Test]
    public void OnlyAQualifyingShipEverRollsAgainstAStorm()
    {
        var (state, settlementId) = MaritimeSettlement();
        var (householdId, _) = HouseholdWithHead(state);
        Fund(state, householdId, Money.FromDenarii(5000));
        var ordinaryShip = RunCommissionToCompletion(
            state, StartCommission(state, householdId, settlementId, ShipVesselClass.Corbita), ShipVesselClass.Corbita, GoodQuality.Common);
        var routeId = state.StandingContractIds.Issue();
        state.StandingContracts.Add(
            routeId,
            new StandingContract(
                routeId, StandingContractKind.TradeRouteInvestment, settlementId, householdId, StandingContractStatus.Active,
                denariiCommitted: Money.FromDenarii(100), routeName: "Ostia run"));
        AssignShipToTradeRouteCommands.Pipeline.Execute(
            state, new AssignShipToTradeRouteCommand(
                state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ordinaryShip.Id, routeId));

        var streams = new RandomStreamSet();
        streams.AddDerived(ShipVoyageRiskSystem.StreamName, rootSeed: 1);
        var events = ShipVoyageRiskSystem.Tick(
            state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Catastrophic, new GameDate(2)) }, streams);

        Assert.Multiple(() =>
        {
            Assert.That(events, Is.Empty, "An ordinary Ship (no Flagship, no fenus nauticum) stays on the §6.1 aggregate default.");
            Assert.That(state.VoyageEvents.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void AQualifyingShipCanResolveBothArrivedSafelyAndLostToStormAcrossABoundedSeedRange()
    {
        var sawSafe = false;
        var sawLost = false;

        for (var seed = 1UL; seed <= 60UL && !(sawSafe && sawLost); seed++)
        {
            var (state, settlementId, householdId, ship, debtId) = FlagshipOnAFenusNauticumFinancedRoute();
            state.MerchantShips.TryGet(ship.Id, out var current);
            state.MerchantShips.Remove(ship.Id);
            state.MerchantShips.Add(ship.Id, current! with { IsFlagship = true });

            var streams = new RandomStreamSet();
            streams.AddDerived(ShipVoyageRiskSystem.StreamName, seed);
            ShipVoyageRiskSystem.Tick(
                state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Catastrophic, new GameDate(2)) }, streams);

            state.MerchantShips.TryGet(ship.Id, out var after);
            if (after!.Status == ShipStatus.LostToStorm)
            {
                sawLost = true;
                state.DebtRecords.TryGet(debtId, out var debt);
                Assert.That(debt!.Status, Is.EqualTo(DebtStatus.Forgiven), "A Ship lost while fenus-nauticum-financed forgives the debt (§8).");
            }
            else if (after.Status == ShipStatus.Active && after.VoyagesCompleted > 0)
            {
                sawSafe = true;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(sawSafe, Is.True, "Expected at least one seed to resolve ArrivedSafely.");
            Assert.That(sawLost, Is.True, "Expected at least one seed to resolve LostToStorm.");
        });
    }

    [Test]
    public void ALostFlagshipCarriesTheSharperDignitasPenaltyAndAChronicleEntry()
    {
        WorldState? finalState = null;

        for (var seed = 1UL; seed <= 80UL && finalState is null; seed++)
        {
            var (state, settlementId, householdId, ship, _) = FlagshipOnAFenusNauticumFinancedRoute();
            state.MerchantShips.TryGet(ship.Id, out var current);
            state.MerchantShips.Remove(ship.Id);
            state.MerchantShips.Add(ship.Id, current! with { IsFlagship = true });
            var dignitasBefore = DignitasResolver.Current(state, householdId);

            var streams = new RandomStreamSet();
            streams.AddDerived(ShipVoyageRiskSystem.StreamName, seed);
            var events = ShipVoyageRiskSystem.Tick(
                state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Catastrophic, new GameDate(2)) }, streams);

            state.MerchantShips.TryGet(ship.Id, out var after);
            if (after!.Status != ShipStatus.LostToStorm)
                continue;

            Assert.That(
                DignitasResolver.Current(state, householdId),
                Is.EqualTo(dignitasBefore + ShippingCatalog.FlagshipLossDignitasPenalty));

            var drafts = ChronicleProjector.Project(state, events);
            Assert.That(drafts, Has.Some.Matches<ChronicleEntryDraft>(draft => draft.HouseholdId == householdId));

            finalState = state;
        }

        Assert.That(finalState, Is.Not.Null, "Expected at least one seed to resolve LostToStorm for the Flagship.");
    }

    [Test]
    public void RepeatedSafeVoyagesEarnALuckyShipReputation()
    {
        var (state, settlementId, householdId, ship, _) = FlagshipOnAFenusNauticumFinancedRoute();
        var shipId = ship.Id;

        var streams = new RandomStreamSet();
        streams.AddDerived(ShipVoyageRiskSystem.StreamName, rootSeed: 7);

        var safeVoyages = 0;
        for (var month = 2; month < 200 && safeVoyages < ShippingCatalog.LuckyShipVoyageThreshold; month++)
        {
            state.MerchantShips.TryGet(shipId, out var before);
            if (before!.Status != ShipStatus.Active)
                break;

            ShipVoyageRiskSystem.Tick(
                state, new GameDate(month), new[] { StormEvent(state, settlementId, DisasterSeverity.Minor, new GameDate(month)) }, streams);

            state.MerchantShips.TryGet(shipId, out var after);
            if (after!.VoyagesCompleted > before.VoyagesCompleted)
                safeVoyages = after.VoyagesCompleted;
        }

        state.MerchantShips.TryGet(shipId, out var final);
        if (final!.VoyagesCompleted >= ShippingCatalog.LuckyShipVoyageThreshold)
            Assert.That(final.ReputationTier, Is.EqualTo(ShipReputationTier.LuckyShip));
    }

    // ---- Save/load round trip and deterministic hash stability -------------------------------

    [Test]
    public void ShippingStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, settlementId, householdId, ship, debtId) = FlagshipOnAFenusNauticumFinancedRoute();
        DesignateFlagshipCommands.Pipeline.Execute(
            state, new DesignateFlagshipCommand(state.CommandIds.Issue(), "player", new GameDate(1), null, householdId, ship.Id));

        var freedmanId = state.CharacterIds.Issue();
        state.Characters.Add(freedmanId, CharacterTestFixtures.Minimal(freedmanId, nomen: "Libertus"));
        Fund(state, householdId, Money.FromDenarii(2000));
        var frontedProjectId = StartCommission(
            state, householdId, settlementId, ShipVesselClass.Ponto, GoodQuality.Common, consecratedLaunch: false, ShipOwnershipMode.Fronted,
            frontingRef: PropertyOwnerRef.ForIndividualCharacter(freedmanId));
        RunCommissionToCompletion(state, frontedProjectId, ShipVesselClass.Ponto, GoodQuality.Common);

        var streams = new RandomStreamSet();
        streams.AddDerived(ShipVoyageRiskSystem.StreamName, rootSeed: 3);
        ShipVoyageRiskSystem.Tick(
            state, new GameDate(2), new[] { StormEvent(state, settlementId, DisasterSeverity.Minor, new GameDate(2)) }, streams);
        ShipUpkeepSystem.Tick(state, new GameDate(2));

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
            Assert.That(restored.MerchantShips.Count, Is.EqualTo(state.MerchantShips.Count));
            Assert.That(restored.ShipCommissionProjects.Count, Is.EqualTo(state.ShipCommissionProjects.Count));
            Assert.That(restored.ShipFrontingArrangements.Count, Is.EqualTo(1));
            Assert.That(restored.VoyageEvents.Count, Is.EqualTo(state.VoyageEvents.Count));
            Assert.That(MerchantMarineQuery.ShipsOwnedBy(restored, householdId).Count(), Is.EqualTo(2));
        });
    }
}
