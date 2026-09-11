using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Companions;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Tests.Characters;
using Gens.Simulation.Tests.Travel;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Companions;

/// <summary>Phase 17 item 1 coverage: Overseer/Senior Position appointment and vacancy, Procurator's
/// linked Stewardship assignment, the Rationalis cluster bonus, travel retinue effects and on-leave
/// bookkeeping, Chronicle wiring, save round-trip, and state-hash stability.</summary>
public sealed class CompanionsTests
{
    private static readonly GameDate Date0 = new(0);

    // ---- Fixtures --------------------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Household> HouseholdId, RuntimeId<Settlement> SettlementId) HouseholdWithSettlement()
    {
        var state = new WorldState(Date0);
        var regionId = state.RegionIds.Issue();
        state.Regions.Add(regionId, Region.Create(regionId, "Latium"));
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var householdId = state.HouseholdIds.Issue();
        return (state, householdId, settlementId);
    }

    private static RuntimeId<Character> AdultCharacter(
        WorldState state, RuntimeId<Household>? householdId, RuntimeId<Settlement> settlementId, string nomen,
        int attributeValue = 60, int loyalty = 60, LegalStatus status = LegalStatus.RomanCitizen)
    {
        var id = state.CharacterIds.Issue();
        state.Characters.Add(
            id,
            CharacterTestFixtures.Minimal(
                id, nomen: nomen, household: householdId, location: settlementId, status: status,
                birthDate: new GameDate(-30 * 12),
                attributes: new CoreAttributes(attributeValue, attributeValue, attributeValue, attributeValue, attributeValue),
                condition: new Condition(80, 0, loyalty, 20, 50)));
        return id;
    }

    private static RuntimeId<Building> BuildBuilding(WorldState state, BuildingSector sector, string tag)
    {
        var buildingId = state.BuildingIds.Issue();
        var plotId = state.PlotIds.Issue();
        var definition = new BuildingDefinition(new DefinitionId<Building>($"test-{tag}"), BuildingTier.Tier1, 1, 1, sector: sector);
        state.Buildings.Add(buildingId, new BuildingInstance(buildingId, plotId, definition));
        return buildingId;
    }

    private static RuntimeId<OverseerAssignment> AppointOverseer(
        WorldState state, RuntimeId<Character> characterId, RuntimeId<Household> householdId, RuntimeId<Building> buildingId,
        OverseerRole role, GameDate? date = null)
    {
        var result = AppointOverseerCommands.Pipeline.Execute(
            state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", date ?? Date0, null, characterId, householdId, buildingId, role));
        Assert.That(result.Accepted, Is.True, result.Error?.ToString());
        return ((OverseerAssignedEvent)result.Events[0]).RecordId;
    }

    private static RuntimeId<SeniorPositionAssignment> AppointSeniorPosition(
        WorldState state, RuntimeId<Character> characterId, RuntimeId<Household> householdId, SeniorPositionTitle title,
        RuntimeId<Building>? tiedBuildingId = null, RuntimeId<OverseerAssignment>? promotedFromOverseerRecordId = null, GameDate? date = null)
    {
        var result = AppointSeniorPositionCommands.Pipeline.Execute(
            state,
            new AppointSeniorPositionCommand(
                state.CommandIds.Issue(), "player", date ?? Date0, null, characterId, householdId, title, tiedBuildingId,
                promotedFromOverseerRecordId));
        Assert.That(result.Accepted, Is.True, result.Error?.ToString());
        return ((SeniorPositionAssignedEvent)result.Events[0]).RecordId;
    }

    // ---- AppointOverseerCommand --------------------------------------------------------------------

    [Test]
    public void AppointOverseerCommandSeatsAnEligibleCharacter()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, settlementId, "Vilicus");
        var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");

        var result = AppointOverseerCommands.Pipeline.Execute(
            state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, buildingId, OverseerRole.Vilicus));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(OverseerResolver.ActiveRecordForCharacter(state, characterId), Is.Not.Null);
            var assigned = (OverseerAssignedEvent)result.Events.Single();
            Assert.That(assigned.Visibility, Is.EqualTo(Visibility.Public));
            Assert.That(assigned.Role, Is.EqualTo(OverseerRole.Vilicus));
        });
    }

    [Test]
    public void AppointOverseerCommandAcceptsAPeregrineFreedmanAndSlaveAlike()
    {
        foreach (var status in new[] { LegalStatus.Peregrine, LegalStatus.Freedman, LegalStatus.Enslaved })
        {
            var (state, householdId, settlementId) = HouseholdWithSettlement();
            var characterId = AdultCharacter(state, householdId, settlementId, "Vilicus", status: status);
            var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");

            var result = AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, buildingId, OverseerRole.Vilicus));

            Assert.That(result.Accepted, Is.True, $"legal status {status} should not gate Overseer eligibility");
        }
    }

    [Test]
    public void AppointOverseerCommandRejectsEachValidationFailure()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");

        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null,
                    RuntimeId<Character>.Parse("char_0009999"), householdId, buildingId, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.CharacterNotFound));

        var deceasedId = AdultCharacter(state, householdId, settlementId, "Deceased");
        state.Characters.TryGet(deceasedId, out var deceased);
        state.Characters.Remove(deceasedId);
        state.Characters.Add(deceasedId, deceased! with { DeathRecord = new DeathRecord(Date0, DeathCause.OldAge, 40) });
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, deceasedId, householdId, buildingId, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.CharacterDeceased));

        var outsiderId = AdultCharacter(state, null, settlementId, "Outsider");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, outsiderId, householdId, buildingId, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.NotHouseholdMember));

        var childId = state.CharacterIds.Issue();
        state.Characters.Add(childId, CharacterTestFixtures.Minimal(childId, nomen: "Child", household: householdId, location: settlementId, birthDate: new GameDate(-2 * 12)));
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, childId, householdId, buildingId, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.IneligibleLifecycleStage));

        var eligibleId = AdultCharacter(state, householdId, settlementId, "Eligible");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null,
                    eligibleId, householdId, RuntimeId<Building>.Parse("building_0009999"), OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.BuildingNotFound));

        var wrongCategoryBuilding = BuildBuilding(state, BuildingSector.Commerce, "market");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, wrongCategoryBuilding, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.BuildingCategoryMismatch));

        AppointOverseer(state, eligibleId, householdId, buildingId, OverseerRole.Vilicus);
        var secondBuilding = BuildBuilding(state, BuildingSector.Agriculture, "villa2");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, secondBuilding, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.AlreadyHoldsOverseerPosition));

        var anotherCharacterId = AdultCharacter(state, householdId, settlementId, "Another");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, anotherCharacterId, householdId, buildingId, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.BuildingAlreadyHasOverseer));

        var weakId = AdultCharacter(state, householdId, settlementId, "Weak", attributeValue: 10, loyalty: 60);
        var weakBuilding = BuildBuilding(state, BuildingSector.Agriculture, "villa3");
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, weakId, householdId, weakBuilding, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.InsufficientAttributeStanding));

        var disloyalId = AdultCharacter(state, householdId, settlementId, "Disloyal", attributeValue: 60, loyalty: 10);
        Assert.That(
            AppointOverseerCommands.Pipeline.Execute(
                state, new AppointOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, disloyalId, householdId, weakBuilding, OverseerRole.Vilicus)).Error,
            Is.EqualTo(AppointOverseerCommands.InsufficientLoyalty));
    }

    // ---- AppointSeniorPositionCommand ---------------------------------------------------------------

    [Test]
    public void AppointSeniorPositionCommandSeatsADirectTierSkippingAppointment()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, settlementId, "Steward");

        var result = AppointSeniorPositionCommands.Pipeline.Execute(
            state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, SeniorPositionTitle.Steward, null, null));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var assigned = (SeniorPositionAssignedEvent)result.Events.Single();
            Assert.That(assigned.PromotedFromOverseerRecordId, Is.Null);
            Assert.That(SeniorPositionResolver.ActiveRecordForCharacter(state, characterId), Is.Not.Null);
        });
    }

    [Test]
    public void AppointSeniorPositionCommandPromotesFromAnActiveOverseerAssignmentAndEndsTheSource()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, settlementId, "RisingStar");
        var buildingId = BuildBuilding(state, BuildingSector.Commerce, "bank");
        var overseerRecordId = AppointOverseer(state, characterId, householdId, buildingId, OverseerRole.Argentarius);

        var result = AppointSeniorPositionCommands.Pipeline.Execute(
            state,
            new AppointSeniorPositionCommand(
                state.CommandIds.Issue(), "player", new GameDate(3), null, characterId, householdId, SeniorPositionTitle.Treasurer, null, overseerRecordId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            var assigned = (SeniorPositionAssignedEvent)result.Events.Single();
            Assert.That(assigned.PromotedFromOverseerRecordId, Is.EqualTo(overseerRecordId));

            state.OverseerAssignments.TryGet(overseerRecordId, out var source);
            Assert.That(OverseerResolver.IsActive(source!), Is.False);
            Assert.That(source!.EndDate, Is.EqualTo(new GameDate(3)));
        });
    }

    [Test]
    public void AppointSeniorPositionCommandRejectsEachValidationFailure()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();

        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null,
                    RuntimeId<Character>.Parse("char_0009999"), householdId, SeniorPositionTitle.Steward, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.CharacterNotFound));

        var eligibleId = AdultCharacter(state, householdId, settlementId, "Eligible");
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, SeniorPositionTitle.Procurator, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.ProcuratorRequiresDedicatedCommand));

        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, SeniorPositionTitle.Cellarer, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.TiedBuildingRequired));

        var buildingId = BuildBuilding(state, BuildingSector.None, "cellar");
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, SeniorPositionTitle.Steward, buildingId, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.TiedBuildingNotApplicable));

        AppointSeniorPosition(state, eligibleId, householdId, SeniorPositionTitle.Steward);
        var secondId = AdultCharacter(state, householdId, settlementId, "Second");
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, eligibleId, householdId, SeniorPositionTitle.Treasurer, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.AlreadyHoldsSeniorPosition));
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, secondId, householdId, SeniorPositionTitle.Steward, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.PositionAlreadyFilled));

        var weakId = AdultCharacter(state, householdId, settlementId, "Weak", attributeValue: 10, loyalty: 60);
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, weakId, householdId, SeniorPositionTitle.Treasurer, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.InsufficientAttributeStanding));

        var disloyalId = AdultCharacter(state, householdId, settlementId, "Disloyal", attributeValue: 60, loyalty: 10);
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state, new AppointSeniorPositionCommand(state.CommandIds.Issue(), "player", Date0, null, disloyalId, householdId, SeniorPositionTitle.Treasurer, null, null)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.InsufficientLoyalty));

        var promoterId = AdultCharacter(state, householdId, settlementId, "Promoter");
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state,
                new AppointSeniorPositionCommand(
                    state.CommandIds.Issue(), "player", Date0, null, promoterId, householdId, SeniorPositionTitle.Treasurer, null,
                    RuntimeId<OverseerAssignment>.Parse("overseer_0009999"))).Error,
            Is.EqualTo(AppointSeniorPositionCommands.PromotionSourceNotFound));

        var overseerBuilding = BuildBuilding(state, BuildingSector.Commerce, "bank2");
        var otherOverseerHolderId = AdultCharacter(state, householdId, settlementId, "OtherOverseer");
        var mismatchedRecordId = AppointOverseer(state, otherOverseerHolderId, householdId, overseerBuilding, OverseerRole.Argentarius);
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state,
                new AppointSeniorPositionCommand(
                    state.CommandIds.Issue(), "player", Date0, null, promoterId, householdId, SeniorPositionTitle.Treasurer, null, mismatchedRecordId)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.PromotionSourceHolderMismatch));

        VacateOverseerCommands.Pipeline.Execute(state, new VacateOverseerCommand(state.CommandIds.Issue(), "player", Date0, null, mismatchedRecordId));
        Assert.That(
            AppointSeniorPositionCommands.Pipeline.Execute(
                state,
                new AppointSeniorPositionCommand(
                    state.CommandIds.Issue(), "player", Date0, null, otherOverseerHolderId, householdId, SeniorPositionTitle.Treasurer, null, mismatchedRecordId)).Error,
            Is.EqualTo(AppointSeniorPositionCommands.PromotionSourceNotActive));
    }

    // ---- AppointSecondSettlementProcuratorCommand -------------------------------------------------------------------

    [Test]
    public void AppointProcuratorCommandCreatesBothLinkedRecords()
    {
        var (state, householdId, homeSettlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, homeSettlementId, "Procurator");
        var farRegionId = state.RegionIds.Issue();
        state.Regions.Add(farRegionId, Region.Create(farRegionId, "Gaul"));
        var targetSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(targetSettlementId, Settlement.Create(targetSettlementId, farRegionId));

        var result = AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
            state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, targetSettlementId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Events.OfType<SeniorPositionAssignedEvent>().Single().Title, Is.EqualTo(SeniorPositionTitle.Procurator));
            Assert.That(result.Events.OfType<StewardshipAssignedEvent>().Single().Context, Is.EqualTo(StewardshipContext.SecondSettlementProcurator));

            var procurator = SeniorPositionResolver.ActiveProcuratorRecord(state, householdId, targetSettlementId);
            Assert.That(procurator, Is.Not.Null);
            Assert.That(procurator!.OversightSettlementId, Is.EqualTo(targetSettlementId));

            var linkedStewardship = ProcuratorLink.ActiveLinkedAssignment(state, procurator);
            Assert.That(linkedStewardship, Is.Not.Null);
            Assert.That(linkedStewardship!.AppointeeCharacterId, Is.EqualTo(characterId));
        });
    }

    [Test]
    public void AppointProcuratorCommandRejectsTheHomeSettlementAndADuplicateTarget()
    {
        var (state, householdId, homeSettlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, homeSettlementId, "Procurator");

        Assert.That(
            AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
                state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, homeSettlementId)).Error,
            Is.EqualTo(AppointSecondSettlementProcuratorCommands.TargetSettlementIsHome));

        var farRegionId = state.RegionIds.Issue();
        state.Regions.Add(farRegionId, Region.Create(farRegionId, "Gaul"));
        var targetSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(targetSettlementId, Settlement.Create(targetSettlementId, farRegionId));
        AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
            state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, targetSettlementId));

        var secondCharacterId = AdultCharacter(state, householdId, homeSettlementId, "SecondProcurator");
        Assert.That(
            AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
                state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, secondCharacterId, householdId, targetSettlementId)).Error,
            Is.EqualTo(AppointSecondSettlementProcuratorCommands.TargetAlreadyHasProcurator));
    }

    [Test]
    public void VacateSeniorPositionCommandOnAProcuratorEndsBothLinkedRecords()
    {
        var (state, householdId, homeSettlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, homeSettlementId, "Procurator");
        var farRegionId = state.RegionIds.Issue();
        state.Regions.Add(farRegionId, Region.Create(farRegionId, "Gaul"));
        var targetSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(targetSettlementId, Settlement.Create(targetSettlementId, farRegionId));
        var appointResult = AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
            state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, targetSettlementId));
        var recordId = appointResult.Events.OfType<SeniorPositionAssignedEvent>().Single().RecordId;
        var assignmentId = appointResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId;

        var vacateResult = VacateSeniorPositionCommands.Pipeline.Execute(
            state, new VacateSeniorPositionCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, recordId));

        Assert.Multiple(() =>
        {
            Assert.That(vacateResult.Accepted, Is.True);
            state.SeniorPositionAssignments.TryGet(recordId, out var record);
            Assert.That(SeniorPositionResolver.IsActive(record!), Is.False);
            state.StewardshipAssignments.TryGet(assignmentId, out var assignment);
            Assert.That(assignment!.IsActive, Is.False);
            Assert.That(assignment.EndDate, Is.EqualTo(new GameDate(2)));
        });
    }

    // ---- RationalisBonusSystem ----------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Household> HouseholdId) HouseholdWithRationalisCluster(bool fillCluster)
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var rationalisId = AdultCharacter(state, householdId, settlementId, "Rationalis");
        AppointSeniorPosition(state, rationalisId, householdId, SeniorPositionTitle.Rationalis);

        if (fillCluster)
        {
            var treasurerId = AdultCharacter(state, householdId, settlementId, "Treasurer");
            AppointSeniorPosition(state, treasurerId, householdId, SeniorPositionTitle.Treasurer);
            var institorMaximusId = AdultCharacter(state, householdId, settlementId, "InstitorMaximus");
            AppointSeniorPosition(state, institorMaximusId, householdId, SeniorPositionTitle.InstitorMaximus);
            var cellarBuilding = BuildBuilding(state, BuildingSector.None, "cellar");
            var cellarerId = AdultCharacter(state, householdId, settlementId, "Cellarer");
            AppointSeniorPosition(state, cellarerId, householdId, SeniorPositionTitle.Cellarer, cellarBuilding);
            var argentariusId = AdultCharacter(state, householdId, settlementId, "Argentarius");
            var bankBuilding = BuildBuilding(state, BuildingSector.Commerce, "bank");
            AppointOverseer(state, argentariusId, householdId, bankBuilding, OverseerRole.Argentarius);
        }

        return (state, householdId);
    }

    [Test]
    public void RationalisBonusSystemAppliesTheDignitasTrickleOnlyWhenTheClusterIsFullyStaffed()
    {
        var (unfilledState, unfilledHouseholdId) = HouseholdWithRationalisCluster(fillCluster: false);
        var beforeUnfilled = DignitasResolver.Current(unfilledState, unfilledHouseholdId);
        new RationalisBonusSystem().Tick(unfilledState, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        Assert.That(DignitasResolver.Current(unfilledState, unfilledHouseholdId), Is.EqualTo(beforeUnfilled), "an incomplete cluster grants no trickle");

        var (state, householdId) = HouseholdWithRationalisCluster(fillCluster: true);
        var before = DignitasResolver.Current(state, householdId);
        var events = new RationalisBonusSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(DignitasResolver.Current(state, householdId) - before, Is.EqualTo(CompanionsCatalog.RationalisClusterBonusDignitas));
            Assert.That(events.OfType<RationalisClusterStatusChangedEvent>().Single().Active, Is.True);
        });
    }

    [Test]
    public void RationalisBonusSystemStopsAndFiresAnInactiveEventOnceAMemberIsVacated()
    {
        var (state, householdId) = HouseholdWithRationalisCluster(fillCluster: true);
        new RationalisBonusSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        var treasurerRecord = SeniorPositionResolver.ActiveRecordForTitle(state, householdId, SeniorPositionTitle.Treasurer)!;
        VacateSeniorPositionCommands.Pipeline.Execute(
            state, new VacateSeniorPositionCommand(state.CommandIds.Issue(), "player", new GameDate(2), null, treasurerRecord.RecordId));

        var before = DignitasResolver.Current(state, householdId);
        var events = new RationalisBonusSystem().Tick(state, new MonthlyTickContext(new GameDate(2), new RandomStreamSet()));

        Assert.Multiple(() =>
        {
            Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(before), "the trickle stops the month a cluster member vacates");
            var statusEvent = events.OfType<RationalisClusterStatusChangedEvent>().Single();
            Assert.That(statusEvent.Active, Is.False);
            Assert.That(statusEvent.HouseholdId, Is.EqualTo(householdId));
        });

        // The event is edge-triggered: a further month with no state change fires no further event.
        var repeatEvents = new RationalisBonusSystem().Tick(state, new MonthlyTickContext(new GameDate(3), new RandomStreamSet()));
        Assert.That(repeatEvents.OfType<RationalisClusterStatusChangedEvent>(), Is.Empty);
    }

    [Test]
    public void RationalisBonusSystemStopsWhileAClusterMemberIsOnLeave()
    {
        var (state, householdId) = HouseholdWithRationalisCluster(fillCluster: true);
        var argentariusRecord = OverseerResolver.ActiveRecordForCharacter(
            state, state.OverseerAssignments.InAscendingOrder().Single().Value.HolderId)!;
        state.OverseerAssignments.Remove(argentariusRecord.RecordId);
        state.OverseerAssignments.Add(argentariusRecord.RecordId, argentariusRecord with { OnLeaveSince = new GameDate(1) });

        Assert.That(OverseerResolver.IsCurrentlyFilled(state, householdId, OverseerRole.Argentarius), Is.False);

        var before = DignitasResolver.Current(state, householdId);
        new RationalisBonusSystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));
        Assert.That(DignitasResolver.Current(state, householdId), Is.EqualTo(before));
    }

    // ---- PositionVacancySystem ----------------------------------------------------------------------

    [Test]
    public void PositionVacancySystemEndsADeadHoldersAssignmentWithNoLossReason()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, settlementId, "Vilicus");
        var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");
        var recordId = AppointOverseer(state, characterId, householdId, buildingId, OverseerRole.Vilicus);

        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.OldAge, 60) });

        var events = new PositionVacancySystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        state.OverseerAssignments.TryGet(recordId, out var record);
        Assert.Multiple(() =>
        {
            Assert.That(OverseerResolver.IsActive(record!), Is.False);
            Assert.That(events.OfType<OverseerVacatedEvent>().Single().RecordId, Is.EqualTo(recordId));
        });
    }

    [Test]
    public void PositionVacancySystemEndsAVacatedProcuratorsLinkedStewardshipAssignment()
    {
        var (state, householdId, homeSettlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, homeSettlementId, "Procurator");
        var farRegionId = state.RegionIds.Issue();
        state.Regions.Add(farRegionId, Region.Create(farRegionId, "Gaul"));
        var targetSettlementId = state.SettlementIds.Issue();
        state.Settlements.Add(targetSettlementId, Settlement.Create(targetSettlementId, farRegionId));
        var appointResult = AppointSecondSettlementProcuratorCommands.Pipeline.Execute(
            state, new AppointSecondSettlementProcuratorCommand(state.CommandIds.Issue(), "player", Date0, null, characterId, householdId, targetSettlementId));
        var assignmentId = appointResult.Events.OfType<StewardshipAssignedEvent>().Single().AssignmentId;

        state.Characters.TryGet(characterId, out var character);
        state.Characters.Remove(characterId);
        state.Characters.Add(characterId, character! with { DeathRecord = new DeathRecord(new GameDate(1), DeathCause.OldAge, 60) });

        new PositionVacancySystem().Tick(state, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        state.StewardshipAssignments.TryGet(assignmentId, out var assignment);
        Assert.That(assignment!.IsActive, Is.False);
    }

    // ---- Travel retinue ------------------------------------------------------------------------------

    private static (WorldState State, RuntimeId<Character> TravelerId, RuntimeId<Character> BodyguardId, RuntimeId<Household> HouseholdId, RuntimeId<Settlement> SettlementId)
        TravelPartyWithABodyguard()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var travelerId = AdultCharacter(state, householdId, settlementId, "Traveler");
        var bodyguardId = AdultCharacter(state, householdId, settlementId, "Bodyguard");
        AppointSeniorPosition(state, bodyguardId, householdId, SeniorPositionTitle.Bodyguard);
        return (state, travelerId, bodyguardId, householdId, settlementId);
    }

    [Test]
    public void TravelRetinueQuerySurfacesBodyguardSecretaryAndPhysicianContributions()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var bodyguardId = AdultCharacter(state, householdId, settlementId, "Bodyguard");
        AppointSeniorPosition(state, bodyguardId, householdId, SeniorPositionTitle.Bodyguard);
        var secretaryId = AdultCharacter(state, householdId, settlementId, "Secretary");
        AppointSeniorPosition(state, secretaryId, householdId, SeniorPositionTitle.Secretary);
        var physicianId = AdultCharacter(state, householdId, settlementId, "Physician");
        AppointSeniorPosition(state, physicianId, householdId, SeniorPositionTitle.CourtPhysician);
        var familyMemberId = AdultCharacter(state, householdId, settlementId, "FamilyMember", attributeValue: 90);

        var party = TravelParty.Create(
            AdultCharacter(state, householdId, settlementId, "Traveler"),
            new[] { bodyguardId, secretaryId, physicianId, familyMemberId });

        var contribution = TravelRetinueQuery.Resolve(state, party);

        Assert.Multiple(() =>
        {
            Assert.That(contribution.HasAmbushMitigation, Is.True);
            Assert.That(contribution.HasCorrespondenceUnlocked, Is.True);
            Assert.That(contribution.HasDiseaseMitigation, Is.True);
            Assert.That(contribution.BestAttributes.Diplomacy, Is.EqualTo(90));
        });
    }

    [Test]
    public void BeginTravelSetsOnLeaveForARetinueMembersPositionAndReturnClearsIt()
    {
        var (state, travelerId, bodyguardId, householdId, settlementId) = TravelPartyWithABodyguard();
        var pipeline = BeginTravelCommands.BuildPipeline(
            TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());

        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", Date0, null, travelerId, new[] { bodyguardId },
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId));
        var beginResult = pipeline.Execute(state, command);
        Assert.That(beginResult.Accepted, Is.True);

        var bodyguardRecord = SeniorPositionResolver.ActiveRecordForCharacter(state, bodyguardId);
        Assert.Multiple(() =>
        {
            Assert.That(bodyguardRecord, Is.Not.Null);
            Assert.That(bodyguardRecord!.OnLeaveSince, Is.EqualTo(Date0));
            Assert.That(SeniorPositionResolver.IsCurrentlyFilled(bodyguardRecord), Is.False, "on-leave is excluded from currently-filled checks");
        });

        var system = new TravelProgressSystem();
        var trip = state.TravelTrips.InAscendingOrder().Single().Value;
        // Advance the outbound leg to Arrived, then explicitly return and advance the return leg to Completed.
        for (var i = 0; i < trip.TravelTimeMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(1 + i), new RandomStreamSet()));

        var tripId = state.TravelTrips.InAscendingOrder().Single().Key;
        BeginReturnCommands.Pipeline.Execute(
            state, new BeginReturnCommand(state.CommandIds.Issue(), "player", new GameDate(trip.TravelTimeMonths), null, tripId, EncounterCompleted: true));

        var refreshedTrip = state.TravelTrips.InAscendingOrder().Single().Value;
        for (var i = 0; i < refreshedTrip.TravelTimeMonths; i++)
            system.Tick(state, new MonthlyTickContext(new GameDate(trip.TravelTimeMonths + 1 + i), new RandomStreamSet()));

        var returnedRecord = SeniorPositionResolver.ActiveRecordForCharacter(state, bodyguardId);
        Assert.That(returnedRecord!.OnLeaveSince, Is.Null);
    }

    [Test]
    public void BeginTravelRejectsARetinueOverCapacity()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var travelerId = AdultCharacter(state, householdId, settlementId, "Traveler");
        var retinueIds = new RuntimeId<Character>[CompanionsCatalog.TravelRetinueCapacity + 1];
        for (var i = 0; i < retinueIds.Length; i++)
            retinueIds[i] = AdultCharacter(state, householdId, settlementId, $"Retinue{i}");

        var pipeline = BeginTravelCommands.BuildPipeline(
            TravelTestFixtures.BuildRegionCatalog(), TravelTestFixtures.BuildDistanceTierCatalog());
        var command = new BeginTravelCommand(
            state.CommandIds.Issue(), "player", Date0, null, travelerId, retinueIds,
            TravelTestFixtures.HomeRegionId, TravelLocation.FrontierRegion(TravelTestFixtures.FarRegionId));

        var result = pipeline.Execute(state, command);
        Assert.That(result.Error, Is.EqualTo(BeginTravelCommands.RetinueOverCapacity));
    }

    // ---- Chronicle -------------------------------------------------------------------------------

    [Test]
    public void ChronicleProjectsAPromotionAsNotableADirectAppointmentAsMinorAndAnOverseerAssignmentAsNothing()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var overseerHolderId = AdultCharacter(state, householdId, settlementId, "Promotee");
        var buildingId = BuildBuilding(state, BuildingSector.Commerce, "bank");
        var overseerRecordId = AppointOverseer(state, overseerHolderId, householdId, buildingId, OverseerRole.Argentarius);

        var overseerEvent = new OverseerAssignedEvent(
            state.EventIds.Issue(), Date0, overseerRecordId, overseerHolderId, OverseerRole.Argentarius, householdId, buildingId, null);
        Assert.That(ChronicleProjector.Project(state, new IDomainEvent[] { overseerEvent }), Is.Empty, "Overseer assignment is not narrative material");

        var promotionEvent = new SeniorPositionAssignedEvent(
            state.EventIds.Issue(), Date0, RuntimeId<SeniorPositionAssignment>.Parse("seniorPosition_0000001"), overseerHolderId,
            SeniorPositionTitle.Treasurer, householdId, overseerRecordId, null);
        var promotionDraft = ChronicleProjector.Project(state, new IDomainEvent[] { promotionEvent }).Single();
        Assert.That(promotionDraft.Tier, Is.EqualTo(ChronicleTier.Notable));

        var directAppointmentId = AdultCharacter(state, householdId, settlementId, "DirectAppointee");
        var directEvent = new SeniorPositionAssignedEvent(
            state.EventIds.Issue(), Date0, RuntimeId<SeniorPositionAssignment>.Parse("seniorPosition_0000002"), directAppointmentId,
            SeniorPositionTitle.Steward, householdId, null, null);
        var directDraft = ChronicleProjector.Project(state, new IDomainEvent[] { directEvent }).Single();
        Assert.That(directDraft.Tier, Is.EqualTo(ChronicleTier.Minor));
    }

    // ---- Save round-trip / hash stability ------------------------------------------------------------

    [Test]
    public void CompanionsStateRoundTripsThroughTheDtoAndDeterministicHashStaysStable()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var overseerCharacterId = AdultCharacter(state, householdId, settlementId, "Overseer");
        var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");
        var overseerRecordId = AppointOverseer(state, overseerCharacterId, householdId, buildingId, OverseerRole.Vilicus);
        AppointSeniorPosition(state, AdultCharacter(state, householdId, settlementId, "Steward"), householdId, SeniorPositionTitle.Steward);
        VacateOverseerCommands.Pipeline.Execute(
            state, new VacateOverseerCommand(state.CommandIds.Issue(), "player", new GameDate(1), null,
                AppointOverseer(state, AdultCharacter(state, householdId, settlementId, "EndedOverseer"), householdId, BuildBuilding(state, BuildingSector.Agriculture, "villa2"), OverseerRole.Vilicus)));

        state.OverseerAssignments.TryGet(overseerRecordId, out var onLeaveSource);
        state.OverseerAssignments.Remove(overseerRecordId);
        state.OverseerAssignments.Add(overseerRecordId, onLeaveSource! with { OnLeaveSince = new GameDate(1) });

        var (rationalisState, rationalisHouseholdId) = HouseholdWithRationalisCluster(fillCluster: true);
        new RationalisBonusSystem().Tick(rationalisState, new MonthlyTickContext(new GameDate(1), new RandomStreamSet()));

        foreach (var target in new (WorldState State, RuntimeId<Household> HouseholdId)[]
                 {
                     (state, householdId),
                     (rationalisState, rationalisHouseholdId),
                 })
        {
            var beforeHash = StateHasher.Hash(target.State);
            var dto = WorldStateMapper.ToDto(target.State);
            var restored = WorldStateMapper.ToWorldState(dto);
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        }
    }

    [Test]
    public void OnLeaveSinceChangeAloneChangesTheStateHash()
    {
        var (state, householdId, settlementId) = HouseholdWithSettlement();
        var characterId = AdultCharacter(state, householdId, settlementId, "Vilicus");
        var buildingId = BuildBuilding(state, BuildingSector.Agriculture, "villa");
        var recordId = AppointOverseer(state, characterId, householdId, buildingId, OverseerRole.Vilicus);

        var beforeHash = StateHasher.Hash(state);

        state.OverseerAssignments.TryGet(recordId, out var record);
        state.OverseerAssignments.Remove(recordId);
        state.OverseerAssignments.Add(recordId, record! with { OnLeaveSince = new GameDate(1) });

        Assert.That(StateHasher.Hash(state), Is.Not.EqualTo(beforeHash));
    }

    [Test]
    public void IdenticalFinalStatesHashIdenticallyRegardlessOfCommandOrdering()
    {
        // Overseer and Senior Position records are issued from independent RuntimeIdCounters, so
        // which of the two commands runs first never changes either one's assigned RecordId — unlike
        // two commands of the *same* kind, where execution order alone would change which RecordId
        // lands on which logical fact and so would legitimately change the hash.
        var (stateA, householdIdA, settlementIdA) = HouseholdWithSettlement();
        var overseerCharacterA = AdultCharacter(stateA, householdIdA, settlementIdA, "Overseer");
        var stewardCharacterA = AdultCharacter(stateA, householdIdA, settlementIdA, "Steward");
        var buildingA = BuildBuilding(stateA, BuildingSector.Agriculture, "villa1");
        AppointOverseer(stateA, overseerCharacterA, householdIdA, buildingA, OverseerRole.Vilicus);
        AppointSeniorPosition(stateA, stewardCharacterA, householdIdA, SeniorPositionTitle.Steward);

        var (stateB, householdIdB, settlementIdB) = HouseholdWithSettlement();
        var overseerCharacterB = AdultCharacter(stateB, householdIdB, settlementIdB, "Overseer");
        var stewardCharacterB = AdultCharacter(stateB, householdIdB, settlementIdB, "Steward");
        var buildingB = BuildBuilding(stateB, BuildingSector.Agriculture, "villa1");
        AppointSeniorPosition(stateB, stewardCharacterB, householdIdB, SeniorPositionTitle.Steward);
        AppointOverseer(stateB, overseerCharacterB, householdIdB, buildingB, OverseerRole.Vilicus);

        Assert.That(StateHasher.Hash(stateA), Is.EqualTo(StateHasher.Hash(stateB)));
    }
}
