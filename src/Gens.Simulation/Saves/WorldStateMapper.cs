using System.Text.Json;
using Gens.Simulation.Actors;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Clientela;
using Gens.Simulation.Economy;
using Gens.Simulation.Epithets;
using Gens.Simulation.Events;
using Gens.Simulation.Funerary;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Markets;
using Gens.Simulation.Policies;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;
using Gens.Simulation.Villas;

namespace Gens.Simulation.Saves;

/// <summary>Maps between the live <see cref="WorldState"/> and its canonical <see
/// cref="WorldSaveDocument"/> persisted shape (ADR 0010). Every collection is sorted here, before it
/// ever reaches <see cref="CanonicalJson"/>, so a re-save of unchanged state is byte-identical.</summary>
public static class WorldStateMapper
{
    public static WorldSaveDocument ToDto(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        return new WorldSaveDocument
        {
            DateTotalMonths = state.Date.TotalMonths,
            NextCommandSequenceNumber = state.NextCommandSequenceNumber,
            Counters = new CounterSetDto
            {
                RegionIds = state.RegionIds.Peek,
                SettlementIds = state.SettlementIds.Peek,
                PlotIds = state.PlotIds.Peek,
                HoldingIds = state.HoldingIds.Peek,
                HouseholdIds = state.HouseholdIds.Peek,
                ActorIds = state.ActorIds.Peek,
                CharacterIds = state.CharacterIds.Peek,
                BuildingIds = state.BuildingIds.Peek,
                ContractIds = state.ContractIds.Peek,
                ActivityIds = state.ActivityIds.Peek,
                CommandIds = state.CommandIds.Peek,
                EventIds = state.EventIds.Peek,
                ScheduledActionIds = state.ScheduledActionIds.Peek,
                LedgerTransactionIds = state.LedgerTransactionIds.Peek,
                DebtRecordIds = state.DebtRecordIds.Peek,
                StandingContractIds = state.StandingContractIds.Peek,
                EventInstanceIds = state.EventInstanceIds.Peek,
                StewardshipAssignmentIds = state.StewardshipAssignmentIds.Peek,
                AutonomousDecisionLogIds = state.AutonomousDecisionLogIds.Peek,
                SchemeIds = state.SchemeIds.Peek,
                ReturnReportIds = state.ReturnReportIds.Peek,
                SuccessionDisputeIds = state.SuccessionDisputeIds.Peek,
                ChronicleEntryIds = state.ChronicleEntryIds.Peek,
                FuneralRecordIds = state.FuneralRecordIds.Peek,
                AgnomenIds = state.AgnomenIds.Peek,
                InheritedCognomenDecisionIds = state.InheritedCognomenDecisionIds.Peek,
                FavorObligationIds = state.FavorObligationIds.Peek,
                MagistracyRecordIds = state.MagistracyRecordIds.Peek,
                OmenEventIds = state.OmenEventIds.Peek,
                PriesthoodRecordIds = state.PriesthoodRecordIds.Peek,
                LegalCaseIds = state.LegalCaseIds.Peek,
            },
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            CharacterIds = state.Characters.InAscendingOrder().Select(entry => entry.Key.ToTaggedString()).ToArray(),
            Characters = state.Characters.InAscendingOrder().Select(entry => ToCharacterDto(entry.Value)).ToArray(),
            // Already deterministic key order (ADR 0004) via KnowledgeState.All.
            Knowledge = state.Knowledge.All().Select(ToKnowledgeDto).ToArray(),
            // Already ascending (due date, action ID) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            ScheduledActions = state.ScheduledActions.InAscendingOrder().Select(entry => ToScheduledActionDto(entry.Value)).ToArray(),
            // Already ascending (From, To) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            Relationships = state.Relationships.InAscendingOrder().Select(entry => ToRelationshipDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending (settlement, group type) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            PopGroups = state.PopGroups.InAscendingOrder().Select(entry => ToPopGroupDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Regions = state.Regions.InAscendingOrder().Select(entry => ToRegionDto(entry.Value)).ToArray(),
            Settlements = state.Settlements.InAscendingOrder().Select(entry => ToSettlementDto(entry.Value)).ToArray(),
            Plots = state.Plots.InAscendingOrder().Select(entry => ToPlotDto(entry.Value)).ToArray(),
            Holdings = state.Holdings.InAscendingOrder().Select(entry => ToHoldingDto(entry.Value)).ToArray(),
            // Already ascending (household, slot) order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            HouseholdRegimenDefaults = state.HouseholdRegimenDefaults.InAscendingOrder()
                .Select(entry => ToHouseholdRegimenDefaultDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Buildings = state.Buildings.InAscendingOrder().Select(entry => ToBuildingInstanceDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Stockpiles = state.Stockpiles.InAscendingOrder().Select(entry => ToStockpileDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            ConstructionSchedules = state.ConstructionSchedules.InAscendingOrder()
                .Select(entry => ToConstructionScheduleDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending LedgerAccountKey order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            LedgerAccounts = state.LedgerAccounts.InAscendingOrder().Select(entry => ToLedgerAccountDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            LedgerTransactions = state.LedgerTransactions.InAscendingOrder().Select(entry => ToLedgerTransactionDto(entry.Value)).ToArray(),
            // Already ascending MarketGoodKey order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            MarketPrices = state.MarketPrices.InAscendingOrder().Select(entry => ToSettlementMarketDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HouseholdStatements = state.HouseholdStatements.InAscendingOrder().Select(entry => ToHouseholdMonthlyStatementDto(entry.Value)).ToArray(),
            DebtRecords = state.DebtRecords.InAscendingOrder().Select(entry => ToDebtRecordDto(entry.Value)).ToArray(),
            NetWorthAssessments = state.NetWorthAssessments.InAscendingOrder().Select(entry => ToNetWorthDto(entry.Value)).ToArray(),
            InsolvencyStates = state.InsolvencyStates.InAscendingOrder().Select(entry => ToInsolvencyStateDto(entry.Value)).ToArray(),
            StandingContracts = state.StandingContracts.InAscendingOrder().Select(entry => ToStandingContractDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HouseholdPolicies = state.HouseholdPolicies.InAscendingOrder().Select(entry => ToHouseholdPolicyStateDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            EventInstances = state.EventInstances.InAscendingOrder().Select(entry => ToEventInstanceDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Actors = state.Actors.InAscendingOrder().Select(entry => ToLivingWorldActorDto(entry.Value)).ToArray(),
            // Already ascending HouseStandingKey order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            HouseStandings = state.HouseStandings.InAscendingOrder().Select(entry => ToHouseStandingDto(entry.Key, entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            RivalDossiers = state.RivalDossiers.InAscendingOrder().Select(entry => ToRivalDossierDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            RegionalFamiliesEntries = state.RegionalFamiliesEntries.InAscendingOrder().Select(entry => ToRegionalFamiliesEntryDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            StewardshipAssignments = state.StewardshipAssignments.InAscendingOrder().Select(entry => ToStewardshipAssignmentDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            AutonomousDecisionLogs = state.AutonomousDecisionLogs.InAscendingOrder().Select(entry => ToAutonomousDecisionLogDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Schemes = state.Schemes.InAscendingOrder().Select(entry => ToSchemeDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            ReturnReports = state.ReturnReports.InAscendingOrder().Select(entry => ToReturnReportDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HouseholdHeadships = state.HouseholdHeadships.InAscendingOrder().Select(entry => ToHouseholdHeadshipDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HeirDesignations = state.HeirDesignations.InAscendingOrder().Select(entry => ToHeirDesignationDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            SuccessionDisputes = state.SuccessionDisputes.InAscendingOrder().Select(entry => ToSuccessionDisputeDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            PlayerControls = state.PlayerControls.InAscendingOrder().Select(entry => ToPlayerControlDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            ChronicleEntries = state.ChronicleEntries.InAscendingOrder().Select(entry => ToChronicleEntryDto(entry.Value)).ToArray(),
            // Already ascending GenerationalChapterKey order (ADR 0004) via OrderedRegistry.InAscendingOrder.
            GenerationalChapters = state.GenerationalChapters.InAscendingOrder().Select(entry => ToGenerationalChapterDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            FuneralRecords = state.FuneralRecords.InAscendingOrder().Select(entry => ToFuneralRecordDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            MourningPeriods = state.MourningPeriods.InAscendingOrder().Select(entry => ToMourningPeriodDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            MemoriaStates = state.MemoriaStates.InAscendingOrder().Select(entry => ToMemoriaStateDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            Agnomens = state.Agnomens.InAscendingOrder().Select(entry => ToAgnomenDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            InheritedCognomenDecisions = state.InheritedCognomenDecisions.InAscendingOrder().Select(entry => ToInheritedCognomenDecisionDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            DynasticEpithets = state.DynasticEpithets.InAscendingOrder().Select(entry => ToDynasticEpithetDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HouseholdReputations = state.HouseholdReputations.InAscendingOrder().Select(entry => ToHouseholdReputationDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            FavorObligations = state.FavorObligations.InAscendingOrder().Select(entry => ToFavorObligationDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            ClientelaEntries = state.ClientelaEntries.InAscendingOrder().Select(entry => ToClientelaEntryDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            HouseholdInfluences = state.HouseholdInfluences.InAscendingOrder().Select(entry => ToHouseholdInfluenceDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            CharacterFactionAlignments = state.CharacterFactionAlignments.InAscendingOrder()
                .Select(entry => ToCharacterFactionAlignmentDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            MagistracyRecords = state.MagistracyRecords.InAscendingOrder().Select(entry => ToMagistracyRecordDto(entry.Value)).ToArray(),
            HouseholdReligions = state.HouseholdReligions.InAscendingOrder().Select(entry => ToHouseholdReligionDto(entry.Value)).ToArray(),
            OmenEvents = state.OmenEvents.InAscendingOrder().Select(entry => ToOmenEventDto(entry.Value)).ToArray(),
            PriesthoodRecords = state.PriesthoodRecords.InAscendingOrder().Select(entry => ToPriesthoodRecordDto(entry.Value)).ToArray(),
            // Already ascending-RuntimeId order (ADR 0001/0004) via OrderedRegistry.InAscendingOrder.
            LegalCases = state.LegalCases.InAscendingOrder().Select(entry => ToLegalCaseDto(entry.Value)).ToArray(),
        };
    }

    public static WorldState ToWorldState(WorldSaveDocument dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var characters = OrderedRegistry<RuntimeId<Character>, Character>.Restore(
            dto.Characters.Select(characterDto =>
            {
                var character = FromCharacterDto(characterDto);
                return new KeyValuePair<RuntimeId<Character>, Character>(character.Id, character);
            }));

        var knowledge = KnowledgeState.Restore(dto.Knowledge.Select(FromKnowledgeDto));

        var scheduledActions = OrderedRegistry<ScheduledActionKey, ScheduledActionEntry>.Restore(
            dto.ScheduledActions.Select(FromScheduledActionDto));

        var relationships = OrderedRegistry<RelationshipKey, Relationship>.Restore(
            dto.Relationships.Select(FromRelationshipDto));

        var popGroups = OrderedRegistry<PopGroupKey, PopGroup>.Restore(
            dto.PopGroups.Select(FromPopGroupDto));

        var regions = OrderedRegistry<RuntimeId<Region>, Region>.Restore(
            dto.Regions.Select(r =>
            {
                var region = FromRegionDto(r);
                return new KeyValuePair<RuntimeId<Region>, Region>(region.Id, region);
            }));

        var settlements = OrderedRegistry<RuntimeId<Settlement>, Settlement>.Restore(
            dto.Settlements.Select(s =>
            {
                var settlement = FromSettlementDto(s);
                return new KeyValuePair<RuntimeId<Settlement>, Settlement>(settlement.Id, settlement);
            }));

        var plots = OrderedRegistry<RuntimeId<Plot>, Plot>.Restore(
            dto.Plots.Select(p =>
            {
                var plot = FromPlotDto(p);
                return new KeyValuePair<RuntimeId<Plot>, Plot>(plot.Id, plot);
            }));

        var holdings = OrderedRegistry<RuntimeId<Holding>, Holding>.Restore(
            dto.Holdings.Select(h =>
            {
                var holding = FromHoldingDto(h);
                return new KeyValuePair<RuntimeId<Holding>, Holding>(holding.Id, holding);
            }));

        var householdRegimenDefaults = OrderedRegistry<HouseholdRegimenKey, RegimenSettings>.Restore(
            dto.HouseholdRegimenDefaults.Select(FromHouseholdRegimenDefaultDto));

        var buildings = OrderedRegistry<RuntimeId<Building>, BuildingInstance>.Restore(
            dto.Buildings.Select(b =>
            {
                var building = FromBuildingInstanceDto(b);
                return new KeyValuePair<RuntimeId<Building>, BuildingInstance>(building.Id, building);
            }));

        var stockpiles = OrderedRegistry<RuntimeId<Holding>, Stockpile>.Restore(
            dto.Stockpiles.Select(s => new KeyValuePair<RuntimeId<Holding>, Stockpile>(
                RuntimeId<Holding>.Parse(s.HoldingId), FromStockpileDto(s))));

        var constructionSchedules = OrderedRegistry<RuntimeId<Holding>, ConstructionSchedule>.Restore(
            dto.ConstructionSchedules.Select(q => new KeyValuePair<RuntimeId<Holding>, ConstructionSchedule>(
                RuntimeId<Holding>.Parse(q.HoldingId), FromConstructionScheduleDto(q))));

        var ledgerAccounts = OrderedRegistry<LedgerAccountKey, LedgerAccount>.Restore(
            dto.LedgerAccounts.Select(a =>
            {
                var account = FromLedgerAccountDto(a);
                return new KeyValuePair<LedgerAccountKey, LedgerAccount>(account.Key, account);
            }));

        var ledgerTransactions = OrderedRegistry<RuntimeId<LedgerTransaction>, LedgerTransaction>.Restore(
            dto.LedgerTransactions.Select(t =>
            {
                var transaction = FromLedgerTransactionDto(t);
                return new KeyValuePair<RuntimeId<LedgerTransaction>, LedgerTransaction>(transaction.Id, transaction);
            }));

        var marketPrices = OrderedRegistry<MarketGoodKey, SettlementMarket>.Restore(
            dto.MarketPrices.Select(m =>
            {
                var market = FromSettlementMarketDto(m);
                return new KeyValuePair<MarketGoodKey, SettlementMarket>(new MarketGoodKey(market.SettlementId, market.GoodId), market);
            }));

        var householdStatements = OrderedRegistry<RuntimeId<Household>, HouseholdMonthlyStatement>.Restore(
            dto.HouseholdStatements.Select(s =>
            {
                var statement = FromHouseholdMonthlyStatementDto(s);
                return new KeyValuePair<RuntimeId<Household>, HouseholdMonthlyStatement>(statement.HouseholdId, statement);
            }));

        var debtRecords = OrderedRegistry<RuntimeId<DebtRecord>, DebtRecord>.Restore(
            dto.DebtRecords.Select(d =>
            {
                var debt = FromDebtRecordDto(d);
                return new KeyValuePair<RuntimeId<DebtRecord>, DebtRecord>(debt.Id, debt);
            }));

        var netWorthAssessments = OrderedRegistry<RuntimeId<Household>, NetWorth>.Restore(
            dto.NetWorthAssessments.Select(n =>
            {
                var netWorth = FromNetWorthDto(n);
                return new KeyValuePair<RuntimeId<Household>, NetWorth>(netWorth.HouseholdId, netWorth);
            }));

        var insolvencyStates = OrderedRegistry<RuntimeId<Household>, InsolvencyState>.Restore(
            dto.InsolvencyStates.Select(i =>
            {
                var insolvency = FromInsolvencyStateDto(i);
                return new KeyValuePair<RuntimeId<Household>, InsolvencyState>(insolvency.HouseholdId, insolvency);
            }));

        var standingContracts = OrderedRegistry<RuntimeId<StandingContract>, StandingContract>.Restore(
            dto.StandingContracts.Select(c =>
            {
                var contract = FromStandingContractDto(c);
                return new KeyValuePair<RuntimeId<StandingContract>, StandingContract>(contract.Id, contract);
            }));

        var householdPolicies = OrderedRegistry<RuntimeId<Household>, HouseholdPolicyState>.Restore(
            dto.HouseholdPolicies.Select(p =>
            {
                var policy = FromHouseholdPolicyStateDto(p);
                return new KeyValuePair<RuntimeId<Household>, HouseholdPolicyState>(policy.HouseholdId, policy);
            }));

        var eventInstances = OrderedRegistry<RuntimeId<EventInstance>, EventInstance>.Restore(
            dto.EventInstances.Select(e =>
            {
                var instance = FromEventInstanceDto(e);
                return new KeyValuePair<RuntimeId<EventInstance>, EventInstance>(instance.InstanceId, instance);
            }));

        var actors = OrderedRegistry<RuntimeId<Actor>, LivingWorldActor>.Restore(
            dto.Actors.Select(a =>
            {
                var actor = FromLivingWorldActorDto(a);
                return new KeyValuePair<RuntimeId<Actor>, LivingWorldActor>(actor.ActorId, actor);
            }));

        var houseStandings = OrderedRegistry<HouseStandingKey, HouseStanding>.Restore(
            dto.HouseStandings.Select(FromHouseStandingDto));

        var rivalDossiers = OrderedRegistry<RuntimeId<Actor>, RivalDossier>.Restore(
            dto.RivalDossiers.Select(d =>
            {
                var dossier = FromRivalDossierDto(d);
                return new KeyValuePair<RuntimeId<Actor>, RivalDossier>(dossier.ActorId, dossier);
            }));

        var regionalFamiliesEntries = OrderedRegistry<RuntimeId<Actor>, RegionalFamiliesEntry>.Restore(
            dto.RegionalFamiliesEntries.Select(e =>
            {
                var entry = FromRegionalFamiliesEntryDto(e);
                return new KeyValuePair<RuntimeId<Actor>, RegionalFamiliesEntry>(entry.ActorId, entry);
            }));

        var stewardshipAssignments = OrderedRegistry<RuntimeId<StewardshipAssignment>, StewardshipAssignment>.Restore(
            dto.StewardshipAssignments.Select(a =>
            {
                var assignment = FromStewardshipAssignmentDto(a);
                return new KeyValuePair<RuntimeId<StewardshipAssignment>, StewardshipAssignment>(assignment.AssignmentId, assignment);
            }));

        var autonomousDecisionLogs = OrderedRegistry<RuntimeId<AutonomousDecisionLog>, AutonomousDecisionLog>.Restore(
            dto.AutonomousDecisionLogs.Select(l =>
            {
                var log = FromAutonomousDecisionLogDto(l);
                return new KeyValuePair<RuntimeId<AutonomousDecisionLog>, AutonomousDecisionLog>(log.LogId, log);
            }));

        var schemes = OrderedRegistry<RuntimeId<Scheme>, Scheme>.Restore(
            dto.Schemes.Select(s =>
            {
                var scheme = FromSchemeDto(s);
                return new KeyValuePair<RuntimeId<Scheme>, Scheme>(scheme.SchemeId, scheme);
            }));

        var returnReports = OrderedRegistry<RuntimeId<ReturnReport>, ReturnReport>.Restore(
            dto.ReturnReports.Select(r =>
            {
                var report = FromReturnReportDto(r);
                return new KeyValuePair<RuntimeId<ReturnReport>, ReturnReport>(report.ReportId, report);
            }));

        var householdHeadships = OrderedRegistry<RuntimeId<Household>, HouseholdHeadship>.Restore(
            dto.HouseholdHeadships.Select(h =>
            {
                var headship = FromHouseholdHeadshipDto(h);
                return new KeyValuePair<RuntimeId<Household>, HouseholdHeadship>(headship.HouseholdId, headship);
            }));

        var heirDesignations = OrderedRegistry<RuntimeId<Household>, HeirDesignation>.Restore(
            dto.HeirDesignations.Select(h =>
            {
                var designation = FromHeirDesignationDto(h);
                return new KeyValuePair<RuntimeId<Household>, HeirDesignation>(designation.HouseholdId, designation);
            }));

        var successionDisputes = OrderedRegistry<RuntimeId<SuccessionDispute>, SuccessionDispute>.Restore(
            dto.SuccessionDisputes.Select(d =>
            {
                var dispute = FromSuccessionDisputeDto(d);
                return new KeyValuePair<RuntimeId<SuccessionDispute>, SuccessionDispute>(dispute.DisputeId, dispute);
            }));

        var playerControls = OrderedRegistry<RuntimeId<Household>, PlayerControlState>.Restore(
            dto.PlayerControls.Select(p =>
            {
                var control = FromPlayerControlDto(p);
                return new KeyValuePair<RuntimeId<Household>, PlayerControlState>(control.HouseholdId, control);
            }));

        var chronicleEntries = OrderedRegistry<RuntimeId<ChronicleEntry>, ChronicleEntry>.Restore(
            dto.ChronicleEntries.Select(e =>
            {
                var entry = FromChronicleEntryDto(e);
                return new KeyValuePair<RuntimeId<ChronicleEntry>, ChronicleEntry>(entry.EntryId, entry);
            }));

        var generationalChapters = OrderedRegistry<GenerationalChapterKey, GenerationalChapter>.Restore(
            dto.GenerationalChapters.Select(c =>
            {
                var chapter = FromGenerationalChapterDto(c);
                var key = new GenerationalChapterKey(chapter.HouseholdId, chapter.StartMonth.TotalMonths);
                return new KeyValuePair<GenerationalChapterKey, GenerationalChapter>(key, chapter);
            }));

        var funeralRecords = OrderedRegistry<RuntimeId<FuneralRecord>, FuneralRecord>.Restore(
            dto.FuneralRecords.Select(f =>
            {
                var funeral = FromFuneralRecordDto(f);
                return new KeyValuePair<RuntimeId<FuneralRecord>, FuneralRecord>(funeral.FuneralId, funeral);
            }));

        var mourningPeriods = OrderedRegistry<RuntimeId<Household>, MourningPeriod>.Restore(
            dto.MourningPeriods.Select(m =>
            {
                var period = FromMourningPeriodDto(m);
                return new KeyValuePair<RuntimeId<Household>, MourningPeriod>(period.HouseholdId, period);
            }));

        var memoriaStates = OrderedRegistry<RuntimeId<Household>, MemoriaState>.Restore(
            dto.MemoriaStates.Select(m =>
            {
                var memoria = FromMemoriaStateDto(m);
                return new KeyValuePair<RuntimeId<Household>, MemoriaState>(memoria.HouseholdId, memoria);
            }));

        var agnomens = OrderedRegistry<RuntimeId<Agnomen>, Agnomen>.Restore(
            dto.Agnomens.Select(a =>
            {
                var agnomen = FromAgnomenDto(a);
                return new KeyValuePair<RuntimeId<Agnomen>, Agnomen>(agnomen.AgnomenId, agnomen);
            }));

        var inheritedCognomenDecisions = OrderedRegistry<RuntimeId<InheritedCognomenDecision>, InheritedCognomenDecision>.Restore(
            dto.InheritedCognomenDecisions.Select(d =>
            {
                var decision = FromInheritedCognomenDecisionDto(d);
                return new KeyValuePair<RuntimeId<InheritedCognomenDecision>, InheritedCognomenDecision>(decision.DecisionId, decision);
            }));

        var dynasticEpithets = OrderedRegistry<RuntimeId<Household>, DynasticEpithet>.Restore(
            dto.DynasticEpithets.Select(e =>
            {
                var epithet = FromDynasticEpithetDto(e);
                return new KeyValuePair<RuntimeId<Household>, DynasticEpithet>(epithet.HouseholdId, epithet);
            }));

        var householdReputations = OrderedRegistry<RuntimeId<Household>, HouseholdReputation>.Restore(
            dto.HouseholdReputations.Select(r =>
            {
                var reputation = FromHouseholdReputationDto(r);
                return new KeyValuePair<RuntimeId<Household>, HouseholdReputation>(reputation.HouseholdId, reputation);
            }));

        var favorObligations = OrderedRegistry<RuntimeId<FavorObligation>, FavorObligation>.Restore(
            dto.FavorObligations.Select(f =>
            {
                var favor = FromFavorObligationDto(f);
                return new KeyValuePair<RuntimeId<FavorObligation>, FavorObligation>(favor.FavorId, favor);
            }));

        var clientelaEntries = OrderedRegistry<RuntimeId<Character>, ClientelaEntry>.Restore(
            dto.ClientelaEntries.Select(c =>
            {
                var entry = FromClientelaEntryDto(c);
                return new KeyValuePair<RuntimeId<Character>, ClientelaEntry>(entry.ClientId, entry);
            }));

        var householdInfluences = OrderedRegistry<RuntimeId<Household>, HouseholdInfluence>.Restore(
            dto.HouseholdInfluences.Select(i =>
            {
                var influence = FromHouseholdInfluenceDto(i);
                return new KeyValuePair<RuntimeId<Household>, HouseholdInfluence>(influence.HouseholdId, influence);
            }));

        var characterFactionAlignments = OrderedRegistry<RuntimeId<Character>, CharacterFactionAlignment>.Restore(
            dto.CharacterFactionAlignments.Select(f =>
            {
                var alignment = FromCharacterFactionAlignmentDto(f);
                return new KeyValuePair<RuntimeId<Character>, CharacterFactionAlignment>(alignment.CharacterId, alignment);
            }));

        var magistracyRecords = OrderedRegistry<RuntimeId<MagistracyRecord>, MagistracyRecord>.Restore(
            dto.MagistracyRecords.Select(m =>
            {
                var record = FromMagistracyRecordDto(m);
                return new KeyValuePair<RuntimeId<MagistracyRecord>, MagistracyRecord>(record.RecordId, record);
            }));

        var householdReligions = OrderedRegistry<RuntimeId<Household>, HouseholdReligion>.Restore(
            dto.HouseholdReligions.Select(r =>
            {
                var religion = FromHouseholdReligionDto(r);
                return new KeyValuePair<RuntimeId<Household>, HouseholdReligion>(religion.HouseholdId, religion);
            }));

        var omenEvents = OrderedRegistry<RuntimeId<OmenEvent>, OmenEvent>.Restore(
            dto.OmenEvents.Select(o =>
            {
                var omen = FromOmenEventDto(o);
                return new KeyValuePair<RuntimeId<OmenEvent>, OmenEvent>(omen.OmenId, omen);
            }));

        var priesthoodRecords = OrderedRegistry<RuntimeId<PriesthoodRecord>, PriesthoodRecord>.Restore(
            dto.PriesthoodRecords.Select(p =>
            {
                var record = FromPriesthoodRecordDto(p);
                return new KeyValuePair<RuntimeId<PriesthoodRecord>, PriesthoodRecord>(record.RecordId, record);
            }));

        var legalCases = OrderedRegistry<RuntimeId<LegalCase>, LegalCase>.Restore(
            dto.LegalCases.Select(c =>
            {
                var legalCase = FromLegalCaseDto(c);
                return new KeyValuePair<RuntimeId<LegalCase>, LegalCase>(legalCase.CaseId, legalCase);
            }));

        return new WorldState(
            date: new GameDate(dto.DateTotalMonths),
            regionIds: RuntimeIdCounter<Region>.Restore(dto.Counters.RegionIds),
            settlementIds: RuntimeIdCounter<Settlement>.Restore(dto.Counters.SettlementIds),
            plotIds: RuntimeIdCounter<Plot>.Restore(dto.Counters.PlotIds),
            holdingIds: RuntimeIdCounter<Holding>.Restore(dto.Counters.HoldingIds),
            householdIds: RuntimeIdCounter<Household>.Restore(dto.Counters.HouseholdIds),
            actorIds: RuntimeIdCounter<Actor>.Restore(dto.Counters.ActorIds),
            characterIds: RuntimeIdCounter<Character>.Restore(dto.Counters.CharacterIds),
            buildingIds: RuntimeIdCounter<Building>.Restore(dto.Counters.BuildingIds),
            contractIds: RuntimeIdCounter<Contract>.Restore(dto.Counters.ContractIds),
            activityIds: RuntimeIdCounter<Activity>.Restore(dto.Counters.ActivityIds),
            commandIds: RuntimeIdCounter<Command>.Restore(dto.Counters.CommandIds),
            eventIds: RuntimeIdCounter<DomainEventEntity>.Restore(dto.Counters.EventIds),
            scheduledActionIds: RuntimeIdCounter<ScheduledAction>.Restore(dto.Counters.ScheduledActionIds),
            ledgerTransactionIds: RuntimeIdCounter<LedgerTransaction>.Restore(dto.Counters.LedgerTransactionIds),
            debtRecordIds: RuntimeIdCounter<DebtRecord>.Restore(dto.Counters.DebtRecordIds),
            standingContractIds: RuntimeIdCounter<StandingContract>.Restore(dto.Counters.StandingContractIds),
            eventInstanceIds: RuntimeIdCounter<EventInstance>.Restore(dto.Counters.EventInstanceIds),
            stewardshipAssignmentIds: RuntimeIdCounter<StewardshipAssignment>.Restore(dto.Counters.StewardshipAssignmentIds),
            autonomousDecisionLogIds: RuntimeIdCounter<AutonomousDecisionLog>.Restore(dto.Counters.AutonomousDecisionLogIds),
            schemeIds: RuntimeIdCounter<Scheme>.Restore(dto.Counters.SchemeIds),
            returnReportIds: RuntimeIdCounter<ReturnReport>.Restore(dto.Counters.ReturnReportIds),
            successionDisputeIds: RuntimeIdCounter<SuccessionDispute>.Restore(dto.Counters.SuccessionDisputeIds),
            chronicleEntryIds: RuntimeIdCounter<ChronicleEntry>.Restore(dto.Counters.ChronicleEntryIds),
            funeralRecordIds: RuntimeIdCounter<FuneralRecord>.Restore(dto.Counters.FuneralRecordIds),
            agnomenIds: RuntimeIdCounter<Agnomen>.Restore(dto.Counters.AgnomenIds),
            inheritedCognomenDecisionIds: RuntimeIdCounter<InheritedCognomenDecision>.Restore(dto.Counters.InheritedCognomenDecisionIds),
            favorObligationIds: RuntimeIdCounter<FavorObligation>.Restore(dto.Counters.FavorObligationIds),
            magistracyRecordIds: RuntimeIdCounter<MagistracyRecord>.Restore(dto.Counters.MagistracyRecordIds),
            omenEventIds: RuntimeIdCounter<OmenEvent>.Restore(dto.Counters.OmenEventIds),
            priesthoodRecordIds: RuntimeIdCounter<PriesthoodRecord>.Restore(dto.Counters.PriesthoodRecordIds),
            legalCaseIds: RuntimeIdCounter<LegalCase>.Restore(dto.Counters.LegalCaseIds),
            regions: regions,
            settlements: settlements,
            plots: plots,
            holdings: holdings,
            characters: characters,
            relationships: relationships,
            scheduledActions: scheduledActions,
            popGroups: popGroups,
            householdRegimenDefaults: householdRegimenDefaults,
            buildings: buildings,
            stockpiles: stockpiles,
            constructionSchedules: constructionSchedules,
            ledgerAccounts: ledgerAccounts,
            ledgerTransactions: ledgerTransactions,
            marketPrices: marketPrices,
            householdStatements: householdStatements,
            debtRecords: debtRecords,
            netWorthAssessments: netWorthAssessments,
            insolvencyStates: insolvencyStates,
            standingContracts: standingContracts,
            householdPolicies: householdPolicies,
            eventInstances: eventInstances,
            actors: actors,
            houseStandings: houseStandings,
            rivalDossiers: rivalDossiers,
            regionalFamiliesEntries: regionalFamiliesEntries,
            stewardshipAssignments: stewardshipAssignments,
            autonomousDecisionLogs: autonomousDecisionLogs,
            schemes: schemes,
            returnReports: returnReports,
            householdHeadships: householdHeadships,
            heirDesignations: heirDesignations,
            successionDisputes: successionDisputes,
            playerControls: playerControls,
            chronicleEntries: chronicleEntries,
            generationalChapters: generationalChapters,
            funeralRecords: funeralRecords,
            mourningPeriods: mourningPeriods,
            memoriaStates: memoriaStates,
            agnomens: agnomens,
            inheritedCognomenDecisions: inheritedCognomenDecisions,
            dynasticEpithets: dynasticEpithets,
            householdReputations: householdReputations,
            favorObligations: favorObligations,
            clientelaEntries: clientelaEntries,
            householdInfluences: householdInfluences,
            characterFactionAlignments: characterFactionAlignments,
            magistracyRecords: magistracyRecords,
            householdReligions: householdReligions,
            omenEvents: omenEvents,
            priesthoodRecords: priesthoodRecords,
            legalCases: legalCases,
            knowledge: knowledge,
            nextCommandSequenceNumber: dto.NextCommandSequenceNumber);
    }

    private static KnowledgeEntryDto ToKnowledgeDto(KeyValuePair<KnowledgeKey, KnowledgeEntry> entry) => new()
    {
        ObserverId = entry.Key.ObserverId,
        SubjectId = entry.Key.SubjectId,
        Topic = entry.Key.Topic,
        ValueJson = JsonSerializer.Serialize(entry.Value.Value, entry.Value.Value.GetType(), CanonicalJson.Options),
        Confidence = entry.Value.Confidence.ToString(),
        AsOfDateTotalMonths = entry.Value.AsOfDate.TotalMonths,
        ProvenanceEventId = entry.Value.ProvenanceEventId,
    };

    private static KeyValuePair<KnowledgeKey, KnowledgeEntry> FromKnowledgeDto(KnowledgeEntryDto dto)
    {
        using var document = JsonDocument.Parse(dto.ValueJson);
        var value = document.RootElement.Clone();
        var confidence = Enum.Parse<KnowledgeConfidence>(dto.Confidence);
        var key = new KnowledgeKey(dto.ObserverId, dto.SubjectId, dto.Topic);
        var entry = new KnowledgeEntry(value, confidence, new GameDate(dto.AsOfDateTotalMonths), dto.ProvenanceEventId);
        return new KeyValuePair<KnowledgeKey, KnowledgeEntry>(key, entry);
    }

    private static CharacterDto ToCharacterDto(Character character) => new()
    {
        Id = character.Id.ToTaggedString(),
        Praenomen = character.Praenomen,
        Nomen = character.Nomen,
        Cognomen = character.Cognomen,
        Sex = character.Sex.ToString(),
        BirthDateTotalMonths = character.BirthDate.TotalMonths,
        VisualProfile = ToVisualProfileDto(character.VisualProfile),
        LegalStatus = character.LegalStatus.ToString(),
        SocialClass = character.SocialClass?.ToString(),
        Culture = character.Culture.Value,
        Location = character.Location.ToTaggedString(),
        Household = character.Household?.ToTaggedString(),
        Attributes = new CoreAttributesDto
        {
            Diplomacy = character.Attributes.Diplomacy,
            Martial = character.Attributes.Martial,
            Stewardship = character.Attributes.Stewardship,
            Intrigue = character.Attributes.Intrigue,
            Learning = character.Attributes.Learning,
        },
        Skills = new LaborSkillsDto
        {
            Fieldwork = character.Skills.Fieldwork,
            DomesticService = character.Skills.DomesticService,
            Craft = character.Skills.Craft,
            Culinary = character.Skills.Culinary,
            Medicine = character.Skills.Medicine,
        },
        Condition = new ConditionDto
        {
            Health = character.Condition.Health,
            Fatigue = character.Condition.Fatigue,
            Loyalty = character.Condition.Loyalty,
            Ambition = character.Condition.Ambition,
            Fertility = character.Condition.Fertility,
        },
        Source = character.Source.ToString(),
        InstantiatedAtMonth = character.InstantiatedAtMonth,
        MotherId = character.MotherId?.ToTaggedString(),
        FatherId = character.FatherId?.ToTaggedString(),
        Legitimacy = character.Legitimacy.ToString(),
        MaritalHistory = character.MaritalHistory.Select(ToMarriageRecordDto).ToArray(),
        PermanentInjuries = character.PermanentInjuries.Select(ToPermanentInjuryDto).ToArray(),
        DeathRecord = character.DeathRecord is null ? null : ToDeathRecordDto(character.DeathRecord.Value),
        Traits = character.Traits.Select(static trait => trait.Value).ToArray(),
        Duty = character.Duty is null ? null : ToDutyAssignmentDto(character.Duty.Value),
        BackfilledHistory = character.BackfilledHistory,
        Regimen = character.Regimen is null ? null : ToRegimenSettingsDto(character.Regimen.Value),
        Flight = character.Flight is null ? null : ToFledRecordDto(character.Flight.Value),
        Pursuit = character.Pursuit is null ? null : ToPursuitRecordDto(character.Pursuit.Value),
        ManumissionPlan = character.ManumissionPlan is null ? null : ToManumissionPlanDto(character.ManumissionPlan.Value),
    };

    private static RegimenSettingsDto ToRegimenSettingsDto(RegimenSettings regimen) => new()
    {
        Diet = regimen.Diet.ToString(),
        Accommodation = regimen.Accommodation.ToString(),
        Freedoms = regimen.Freedoms.ToString(),
        Discipline = regimen.Discipline.ToString(),
    };

    private static RegimenSettings FromRegimenSettingsDto(RegimenSettingsDto dto) => new(
        Enum.Parse<DietTier>(dto.Diet), Enum.Parse<AccommodationTier>(dto.Accommodation),
        Enum.Parse<FreedomsTier>(dto.Freedoms), Enum.Parse<DisciplineTier>(dto.Discipline));

    private static FledRecordDto ToFledRecordDto(FledRecord flight) => new()
    {
        FledDateTotalMonths = flight.FledDate.TotalMonths,
        FormerHousehold = flight.FormerHousehold.ToTaggedString(),
        LastKnownLocation = flight.LastKnownLocation?.ToTaggedString(),
    };

    private static FledRecord FromFledRecordDto(FledRecordDto dto) => new(
        new GameDate(dto.FledDateTotalMonths),
        RuntimeId<Household>.Parse(dto.FormerHousehold),
        dto.LastKnownLocation is null ? null : RuntimeId<Settlement>.Parse(dto.LastKnownLocation));

    private static PursuitRecordDto ToPursuitRecordDto(PursuitRecord pursuit) => new()
    {
        MonthsRemaining = pursuit.MonthsRemaining,
        PursuerId = pursuit.PursuerId?.ToTaggedString(),
    };

    private static PursuitRecord FromPursuitRecordDto(PursuitRecordDto dto) => new(
        dto.MonthsRemaining, dto.PursuerId is null ? null : RuntimeId<Character>.Parse(dto.PursuerId));

    private static ManumissionPlanDto ToManumissionPlanDto(ManumissionPlan plan) => new()
    {
        GrantorId = plan.GrantorId.ToTaggedString(),
        Type = plan.Type.ToString(),
    };

    private static ManumissionPlan FromManumissionPlanDto(ManumissionPlanDto dto) => new(
        RuntimeId<Character>.Parse(dto.GrantorId), Enum.Parse<ManumissionType>(dto.Type));

    private static HouseholdRegimenDefaultDto ToHouseholdRegimenDefaultDto(HouseholdRegimenKey key, RegimenSettings regimen) => new()
    {
        HouseholdId = key.HouseholdId.ToTaggedString(),
        Slot = key.Slot?.ToString(),
        Regimen = ToRegimenSettingsDto(regimen),
    };

    private static KeyValuePair<HouseholdRegimenKey, RegimenSettings> FromHouseholdRegimenDefaultDto(HouseholdRegimenDefaultDto dto)
    {
        var key = new HouseholdRegimenKey(
            RuntimeId<Household>.Parse(dto.HouseholdId), dto.Slot is null ? null : Enum.Parse<DutySlot>(dto.Slot));
        return new KeyValuePair<HouseholdRegimenKey, RegimenSettings>(key, FromRegimenSettingsDto(dto.Regimen));
    }

    private static DutyAssignmentDto ToDutyAssignmentDto(DutyAssignment duty) => new()
    {
        HouseholdId = duty.HouseholdId.ToTaggedString(),
        Slot = duty.Slot.ToString(),
        AssignedDateTotalMonths = duty.AssignedDate.TotalMonths,
    };

    private static DutyAssignment FromDutyAssignmentDto(DutyAssignmentDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<DutySlot>(dto.Slot),
        new GameDate(dto.AssignedDateTotalMonths));

    private static Character FromCharacterDto(CharacterDto dto) => Character.Create(
        id: RuntimeId<Character>.Parse(dto.Id),
        praenomen: dto.Praenomen,
        nomen: dto.Nomen,
        cognomen: dto.Cognomen,
        sex: Enum.Parse<Sex>(dto.Sex),
        birthDate: new GameDate(dto.BirthDateTotalMonths),
        visualProfile: FromVisualProfileDto(dto.VisualProfile),
        status: Enum.Parse<LegalStatus>(dto.LegalStatus),
        socialClass: dto.SocialClass is null ? null : Enum.Parse<SocialClass>(dto.SocialClass),
        culture: new DefinitionId<Culture>(dto.Culture),
        location: RuntimeId<Settlement>.Parse(dto.Location),
        household: dto.Household is null ? null : RuntimeId<Household>.Parse(dto.Household),
        attributes: new CoreAttributes(
            dto.Attributes.Diplomacy, dto.Attributes.Martial, dto.Attributes.Stewardship,
            dto.Attributes.Intrigue, dto.Attributes.Learning),
        skills: new LaborSkills(
            dto.Skills.Fieldwork, dto.Skills.DomesticService, dto.Skills.Craft,
            dto.Skills.Culinary, dto.Skills.Medicine),
        condition: new Condition(
            dto.Condition.Health, dto.Condition.Fatigue, dto.Condition.Loyalty,
            dto.Condition.Ambition, dto.Condition.Fertility),
        source: Enum.Parse<CharacterSource>(dto.Source),
        instantiatedAtMonth: dto.InstantiatedAtMonth,
        backfilledHistory: dto.BackfilledHistory,
        motherId: dto.MotherId is null ? null : RuntimeId<Character>.Parse(dto.MotherId),
        fatherId: dto.FatherId is null ? null : RuntimeId<Character>.Parse(dto.FatherId),
        legitimacy: Enum.Parse<Legitimacy>(dto.Legitimacy),
        maritalHistory: dto.MaritalHistory.Select(FromMarriageRecordDto).ToArray(),
        permanentInjuries: dto.PermanentInjuries.Select(FromPermanentInjuryDto).ToArray(),
        traits: dto.Traits.Select(static trait => new DefinitionId<Trait>(trait)).ToArray(),
        deathRecord: dto.DeathRecord is null ? null : FromDeathRecordDto(dto.DeathRecord),
        duty: dto.Duty is null ? null : FromDutyAssignmentDto(dto.Duty),
        regimen: dto.Regimen is null ? null : FromRegimenSettingsDto(dto.Regimen),
        flight: dto.Flight is null ? null : FromFledRecordDto(dto.Flight),
        pursuit: dto.Pursuit is null ? null : FromPursuitRecordDto(dto.Pursuit),
        manumissionPlan: dto.ManumissionPlan is null ? null : FromManumissionPlanDto(dto.ManumissionPlan));

    private static MarriageRecordDto ToMarriageRecordDto(MarriageRecord record) => new()
    {
        SpouseId = record.SpouseId.ToTaggedString(),
        StartDateTotalMonths = record.StartDate.TotalMonths,
        EndDateTotalMonths = record.EndDate?.TotalMonths,
        EndReason = record.EndReason?.ToString(),
    };

    private static MarriageRecord FromMarriageRecordDto(MarriageRecordDto dto) => new(
        RuntimeId<Character>.Parse(dto.SpouseId),
        new GameDate(dto.StartDateTotalMonths),
        dto.EndDateTotalMonths is null ? null : new GameDate(dto.EndDateTotalMonths.Value),
        dto.EndReason is null ? null : Enum.Parse<MarriageEndReason>(dto.EndReason));

    private static PermanentInjuryDto ToPermanentInjuryDto(PermanentInjury injury) => new()
    {
        Target = injury.Target.ToString(),
        Magnitude = injury.Magnitude,
        Cause = injury.Cause,
        InflictedDateTotalMonths = injury.InflictedDate.TotalMonths,
    };

    private static PermanentInjury FromPermanentInjuryDto(PermanentInjuryDto dto) => new(
        Enum.Parse<PermanentInjuryTarget>(dto.Target),
        dto.Magnitude,
        dto.Cause,
        new GameDate(dto.InflictedDateTotalMonths));

    private static DeathRecordDto ToDeathRecordDto(DeathRecord record) => new()
    {
        DateTotalMonths = record.Date.TotalMonths,
        Cause = record.Cause.ToString(),
        AgeAtDeath = record.AgeAtDeath,
    };

    private static DeathRecord FromDeathRecordDto(DeathRecordDto dto) => new(
        new GameDate(dto.DateTotalMonths),
        Enum.Parse<DeathCause>(dto.Cause),
        dto.AgeAtDeath);

    private static CharacterVisualProfileDto ToVisualProfileDto(CharacterVisualProfile profile) => new()
    {
        Height = profile.Height.ToString(),
        Build = profile.Build.ToString(),
        FacialStructure = profile.FacialStructure.ToString(),
        Complexion = profile.Complexion.ToString(),
        HairColor = profile.HairColor.ToString(),
        HairStyle = profile.HairStyle.ToString(),
        EyeColor = profile.EyeColor.ToString(),
        NotableFeatures = profile.NotableFeatures.Select(static feature => feature.ToString()).ToArray(),
        Portrait = new PortraitRecipeDto { Layers = profile.Portrait.Layers.ToArray() },
    };

    private static CharacterVisualProfile FromVisualProfileDto(CharacterVisualProfileDto dto) => new()
    {
        Height = Enum.Parse<Height>(dto.Height),
        Build = Enum.Parse<Build>(dto.Build),
        FacialStructure = Enum.Parse<FacialStructure>(dto.FacialStructure),
        Complexion = Enum.Parse<Complexion>(dto.Complexion),
        HairColor = Enum.Parse<HairColor>(dto.HairColor),
        HairStyle = Enum.Parse<HairStyle>(dto.HairStyle),
        EyeColor = Enum.Parse<EyeColor>(dto.EyeColor),
        NotableFeatures = dto.NotableFeatures.Select(static feature => Enum.Parse<NotableFeature>(feature)).ToArray(),
        Portrait = new PortraitRecipe { Layers = dto.Portrait.Layers.ToArray() },
    };

    private static ScheduledActionEntryDto ToScheduledActionDto(ScheduledActionEntry entry) => new()
    {
        ActionId = entry.ActionId.ToTaggedString(),
        DueDateTotalMonths = entry.DueDate.TotalMonths,
        ActorId = entry.ActorId,
        ActionType = entry.ActionType,
        PayloadJson = entry.PayloadJson,
        CausationId = entry.CausationId,
    };

    private static KeyValuePair<ScheduledActionKey, ScheduledActionEntry> FromScheduledActionDto(ScheduledActionEntryDto dto)
    {
        var actionId = RuntimeId<ScheduledAction>.Parse(dto.ActionId);
        var dueDate = new GameDate(dto.DueDateTotalMonths);
        var entry = new ScheduledActionEntry(actionId, dueDate, dto.ActorId, dto.ActionType, dto.PayloadJson, dto.CausationId);
        return new KeyValuePair<ScheduledActionKey, ScheduledActionEntry>(new ScheduledActionKey(dueDate, actionId), entry);
    }

    /// <summary>Every <see cref="BondTag"/> flag, in a fixed, explicit, hand-listed order rather than
    /// <c>Enum.GetValues</c> — ADR 0004's "never rely on reflection discovery order" applies just as
    /// much to an enum's runtime value order as to any other collection (matching <see
    /// cref="CharacterVisualProfileGenerator"/>'s identical convention).</summary>
    private static readonly BondTag[] AllBondTags =
    {
        BondTag.Parent, BondTag.Child, BondTag.Sibling, BondTag.Spouse,
        BondTag.Friend, BondTag.Rival, BondTag.Lover, BondTag.Mentor, BondTag.Student,
        BondTag.Contubernium, BondTag.Nemesis, BondTag.Patron, BondTag.Client,
        BondTag.Debtor, BondTag.Creditor, BondTag.CoMagistrate, BondTag.BlackmailLeverage,
    };

    /// <summary>Every individual flag set in <paramref name="bonds"/>, by name — stored as a string
    /// list rather than the raw numeric bitmask so a save file stays human-readable and stable across
    /// a future <see cref="BondTag"/> reordering, matching how <see cref="Character.Traits"/> is
    /// stored as definition-ID strings rather than an opaque encoding.</summary>
    private static string[] ToBondTagDto(BondTag bonds) =>
        AllBondTags.Where(tag => bonds.HasFlag(tag)).Select(tag => tag.ToString()).ToArray();

    private static BondTag FromBondTagDto(IReadOnlyList<string> bonds)
    {
        var result = BondTag.None;
        foreach (var bond in bonds)
            result |= Enum.Parse<BondTag>(bond);
        return result;
    }

    private static RelationshipDto ToRelationshipDto(RelationshipKey key, Relationship relationship) => new()
    {
        From = key.From.ToTaggedString(),
        To = key.To.ToTaggedString(),
        Opinion = relationship.Opinion,
        Bonds = ToBondTagDto(relationship.Bonds),
        Origin = relationship.Origin.ToString(),
        FormedDateTotalMonths = relationship.FormedDate.TotalMonths,
        LastMeaningfulInteractionDateTotalMonths = relationship.LastMeaningfulInteractionDate.TotalMonths,
        ProvenanceEventId = relationship.ProvenanceEventId,
    };

    private static KeyValuePair<RelationshipKey, Relationship> FromRelationshipDto(RelationshipDto dto)
    {
        var key = new RelationshipKey(RuntimeId<Character>.Parse(dto.From), RuntimeId<Character>.Parse(dto.To));
        var relationship = new Relationship(
            dto.Opinion,
            FromBondTagDto(dto.Bonds),
            Enum.Parse<RelationshipOrigin>(dto.Origin),
            new GameDate(dto.FormedDateTotalMonths),
            new GameDate(dto.LastMeaningfulInteractionDateTotalMonths),
            dto.ProvenanceEventId);
        return new KeyValuePair<RelationshipKey, Relationship>(key, relationship);
    }

    private static PopGroupDto ToPopGroupDto(PopGroup popGroup) => new()
    {
        SettlementId = popGroup.SettlementId.ToTaggedString(),
        GroupType = popGroup.GroupType.ToString(),
        Size = popGroup.Size,
        CitizenCount = popGroup.LegalStatusDistribution.Citizen,
        LatinRightsCount = popGroup.LegalStatusDistribution.LatinRights,
        PeregrineCount = popGroup.LegalStatusDistribution.Peregrine,
        FreedmanCount = popGroup.LegalStatusDistribution.Freedman,
        Culture = popGroup.Culture.Value,
        WealthBand = popGroup.WealthBand.ToString(),
        NeedsProfile = popGroup.NeedsProfile.ToString(),
        EmploymentRatio = popGroup.EmploymentRatio,
        HousingSatisfaction = popGroup.HousingSatisfaction,
        Contentment = popGroup.Contentment,
        HealthExposure = popGroup.HealthExposure,
    };

    private static KeyValuePair<PopGroupKey, PopGroup> FromPopGroupDto(PopGroupDto dto)
    {
        var settlementId = RuntimeId<Settlement>.Parse(dto.SettlementId);
        var groupType = Enum.Parse<PopGroupType>(dto.GroupType);
        var distribution = new LegalStatusDistribution(dto.CitizenCount, dto.LatinRightsCount, dto.PeregrineCount, dto.FreedmanCount);
        var popGroup = PopGroup.Create(
            settlementId,
            groupType,
            dto.Size,
            legalStatusDistribution: distribution,
            culture: new DefinitionId<Culture>(dto.Culture),
            wealthBand: Enum.Parse<WealthBand>(dto.WealthBand),
            needsProfile: Enum.Parse<DietTier>(dto.NeedsProfile),
            employmentRatio: dto.EmploymentRatio,
            housingSatisfaction: dto.HousingSatisfaction,
            contentment: dto.Contentment,
            healthExposure: dto.HealthExposure);
        return new KeyValuePair<PopGroupKey, PopGroup>(new PopGroupKey(settlementId, groupType), popGroup);
    }

    private static RegionDto ToRegionDto(Region region) => new()
    {
        Id = region.Id.ToTaggedString(),
        Name = region.Name,
    };

    private static Region FromRegionDto(RegionDto dto) =>
        Region.Create(RuntimeId<Region>.Parse(dto.Id), dto.Name);

    private static SettlementDto ToSettlementDto(Settlement settlement) => new()
    {
        Id = settlement.Id.ToTaggedString(),
        RegionId = settlement.RegionId.ToTaggedString(),
        Stage = settlement.Stage.ToString(),
    };

    private static Settlement FromSettlementDto(SettlementDto dto) =>
        Settlement.Create(
            RuntimeId<Settlement>.Parse(dto.Id),
            RuntimeId<Region>.Parse(dto.RegionId),
            Enum.Parse<SettlementStage>(dto.Stage));

    private static PlotDto ToPlotDto(Plot plot) => new()
    {
        Id = plot.Id.ToTaggedString(),
        SettlementId = plot.SettlementId.ToTaggedString(),
        Terrain = plot.Terrain.ToString(),
        Features = (int)plot.Features,
        Condition = plot.Condition.Value,
        Capacity = plot.Capacity,
        OwnerId = plot.OwnerId,
        OccupyingHoldingId = plot.OccupyingHoldingId?.ToTaggedString(),
        IsContested = plot.IsContested,
        AcquisitionMethod = plot.Acquisition?.Method.ToString(),
        AcquiredDateTotalMonths = plot.Acquisition?.AcquiredDate.TotalMonths,
        AcquisitionSourceId = plot.Acquisition?.SourceId,
    };

    private static Plot FromPlotDto(PlotDto dto) =>
        Plot.Create(
            RuntimeId<Plot>.Parse(dto.Id),
            RuntimeId<Settlement>.Parse(dto.SettlementId),
            Enum.Parse<TerrainType>(dto.Terrain),
            (TerrainFeature)dto.Features,
            new LandCondition(dto.Condition),
            dto.Capacity,
            dto.IsContested,
            dto.OwnerId,
            dto.OccupyingHoldingId is null ? null : RuntimeId<Holding>.Parse(dto.OccupyingHoldingId),
            dto.AcquisitionMethod is null || dto.AcquiredDateTotalMonths is null
                ? null
                : new LandAcquisition(
                    Enum.Parse<AcquisitionMethod>(dto.AcquisitionMethod),
                    new GameDate(dto.AcquiredDateTotalMonths.Value),
                    dto.AcquisitionSourceId));

    private static HoldingDto ToHoldingDto(Holding holding) => new()
    {
        Id = holding.Id.ToTaggedString(),
        SettlementId = holding.SettlementId.ToTaggedString(),
        OwnerId = holding.OwnerId,
        OccupantId = holding.OccupantId,
        ResidentCapacity = holding.ResidentCapacity,
        Villa = holding.Villa is null ? null : ToVillaDto(holding.Villa),
    };

    private static Holding FromHoldingDto(HoldingDto dto) =>
        Holding.Create(
            RuntimeId<Holding>.Parse(dto.Id), RuntimeId<Settlement>.Parse(dto.SettlementId),
            dto.OwnerId, dto.OccupantId, dto.ResidentCapacity,
            dto.Villa is null ? null : FromVillaDto(dto.Villa));

    private static VillaDto ToVillaDto(Villa villa) => new()
    {
        Stage = villa.Stage.ToString(),
        IsOutpost = villa.IsOutpost,
        Rooms = villa.Rooms.Select(room => new VillaRoomDto
        {
            Key = room.Key,
            DefinitionKey = room.Definition.Key,
            MinimumStage = room.Definition.MinimumStage.ToString(),
            MaximumTier = room.Definition.MaximumTier,
            UsesRoomSlot = room.Definition.UsesRoomSlot,
            Tier = room.Tier,
            CapacityTier = room.CapacityTier.ToString(),
            Condition = room.Condition.ToString(),
            AssignedTo = room.AssignedTo,
        }).ToArray(),
    };

    private static Villa FromVillaDto(VillaDto dto)
    {
        var villa = new Villa(Enum.Parse<VillaStage>(dto.Stage), dto.IsOutpost);
        foreach (var room in dto.Rooms)
        {
            var definition = new VillaRoomDefinition(
                room.DefinitionKey, Enum.Parse<VillaStage>(room.MinimumStage), room.MaximumTier, room.UsesRoomSlot);
            villa.AddRoom(new VillaRoomInstance(
                room.Key, definition, room.Tier, Enum.Parse<VillaRoomCapacityTier>(room.CapacityTier),
                Enum.Parse<BuildingCondition>(room.Condition), room.AssignedTo));
        }
        return villa;
    }

    private static RecipeLineDto ToRecipeLineDto(RecipeLine line) => new()
    {
        GoodId = line.GoodId.Value,
        Quantity = line.Quantity,
    };

    private static RecipeLine FromRecipeLineDto(RecipeLineDto dto) =>
        new(new DefinitionId<Good>(dto.GoodId), dto.Quantity);

    private static StaffingSlotDto ToStaffingSlotDto(StaffingSlot slot) => new()
    {
        Id = slot.Id,
        Capacity = slot.Capacity,
        RequiredForProduction = slot.RequiredForProduction,
    };

    private static StaffingSlot FromStaffingSlotDto(StaffingSlotDto dto) =>
        new(dto.Id, dto.Capacity, dto.RequiredForProduction);

    private static BuildingDefinitionDto ToBuildingDefinitionDto(BuildingDefinition definition) => new()
    {
        Id = definition.Id.Value,
        Tier = definition.Tier.ToString(),
        ConstructionMonths = definition.ConstructionMonths,
        PlotCapacity = definition.PlotCapacity,
        Prerequisites = definition.Prerequisites.Select(static id => id.Value).ToArray(),
        AllowedTerrain = definition.AllowedTerrain.Select(static terrain => terrain.ToString()).ToArray(),
        RequiredFeatures = (int)definition.RequiredFeatures,
        Upkeep = definition.Upkeep.Select(ToRecipeLineDto).ToArray(),
        StaffingSlots = definition.StaffingSlots.Select(ToStaffingSlotDto).ToArray(),
        Recipe = definition.Recipe is null ? null : new ProductionRecipeDto
        {
            Inputs = definition.Recipe.Inputs.Select(ToRecipeLineDto).ToArray(),
            Outputs = definition.Recipe.Outputs.Select(ToRecipeLineDto).ToArray(),
        },
    };

    private static BuildingDefinition FromBuildingDefinitionDto(BuildingDefinitionDto dto) => new(
        new DefinitionId<Building>(dto.Id),
        Enum.Parse<BuildingTier>(dto.Tier),
        dto.ConstructionMonths,
        dto.PlotCapacity,
        prerequisites: dto.Prerequisites.Select(static id => new DefinitionId<Building>(id)),
        allowedTerrain: dto.AllowedTerrain.Select(static terrain => Enum.Parse<TerrainType>(terrain)),
        requiredFeatures: (TerrainFeature)dto.RequiredFeatures,
        upkeep: dto.Upkeep.Select(FromRecipeLineDto),
        staffingSlots: dto.StaffingSlots.Select(FromStaffingSlotDto),
        recipe: dto.Recipe is null ? null : new ProductionRecipe(
            dto.Recipe.Inputs.Select(FromRecipeLineDto), dto.Recipe.Outputs.Select(FromRecipeLineDto)));

    private static BuildingInstanceDto ToBuildingInstanceDto(BuildingInstance building) => new()
    {
        Id = building.Id.ToTaggedString(),
        PlotId = building.PlotId.ToTaggedString(),
        Definition = ToBuildingDefinitionDto(building.Definition),
        Condition = building.Condition.ToString(),
        // Already ordinal slot-ID order (ADR 0004) via BuildingInstance.Staff's SortedDictionary.
        Staff = building.Staff.Select(pair => new StaffSlotAssignmentDto
        {
            SlotId = pair.Key,
            WorkerIds = pair.Value.ToArray(),
        }).ToArray(),
    };

    private static BuildingInstance FromBuildingInstanceDto(BuildingInstanceDto dto) => BuildingInstance.Restore(
        RuntimeId<Building>.Parse(dto.Id),
        RuntimeId<Plot>.Parse(dto.PlotId),
        FromBuildingDefinitionDto(dto.Definition),
        Enum.Parse<BuildingCondition>(dto.Condition),
        dto.Staff.Select(assignment =>
            new KeyValuePair<string, IReadOnlyCollection<string>>(assignment.SlotId, assignment.WorkerIds)));

    private static ConstructionProjectDto ToConstructionProjectDto(ConstructionProject project) => new()
    {
        Sequence = project.Sequence,
        PlotId = project.PlotId.ToTaggedString(),
        Definition = ToBuildingDefinitionDto(project.Definition),
        CompletedMonths = project.CompletedMonths,
    };

    private static ConstructionProject FromConstructionProjectDto(ConstructionProjectDto dto) => new(
        dto.Sequence, RuntimeId<Plot>.Parse(dto.PlotId), FromBuildingDefinitionDto(dto.Definition), dto.CompletedMonths);

    private static ConstructionScheduleDto ToConstructionScheduleDto(RuntimeId<Holding> holdingId, ConstructionSchedule queue) => new()
    {
        HoldingId = holdingId.ToTaggedString(),
        NextSequence = queue.NextSequence,
        // Already FIFO order (ADR 0004) via ConstructionSchedule.Projects.
        Projects = queue.Projects.Select(ToConstructionProjectDto).ToArray(),
    };

    private static ConstructionSchedule FromConstructionScheduleDto(ConstructionScheduleDto dto) =>
        ConstructionSchedule.Restore(dto.NextSequence, dto.Projects.Select(FromConstructionProjectDto));

    private static GoodDefinitionDto ToGoodDefinitionDto(GoodDefinition good) => new()
    {
        Id = good.Id.Value,
        Perishability = good.Perishability.ToString(),
        QualityEligible = good.QualityEligible,
        ConditionTracked = good.ConditionTracked,
        ShelfLifeTicks = good.ShelfLifeTicks,
    };

    private static GoodDefinition FromGoodDefinitionDto(GoodDefinitionDto dto) => new(
        new DefinitionId<Good>(dto.Id),
        Enum.Parse<Perishability>(dto.Perishability),
        dto.QualityEligible,
        dto.ConditionTracked,
        dto.ShelfLifeTicks);

    private static StockProvenanceDto ToStockProvenanceDto(StockProvenance provenance) => new()
    {
        SourceId = provenance.SourceId,
        EventId = provenance.EventId,
        ExceptionalObjectId = provenance.ExceptionalObjectId,
    };

    private static StockProvenance FromStockProvenanceDto(StockProvenanceDto dto) =>
        new(dto.SourceId, dto.EventId, dto.ExceptionalObjectId);

    private static StockLotDto ToStockLotDto(StockLot lot) => new()
    {
        Good = ToGoodDefinitionDto(lot.Good),
        Quantity = lot.Quantity,
        Quality = lot.Quality?.ToString(),
        Condition = lot.Condition?.Value,
        AgeInTicks = lot.AgeInTicks,
        Provenance = lot.Provenance is null ? null : ToStockProvenanceDto(lot.Provenance.Value),
    };

    private static StockReservationDto ToStockReservationDto(StockReservation reservation) => new()
    {
        ReservationId = reservation.ReservationId,
        GoodId = reservation.GoodId.Value,
        Quality = reservation.Quality?.ToString(),
        Quantity = reservation.Quantity,
    };

    private static StockpileDto ToStockpileDto(RuntimeId<Holding> holdingId, Stockpile stockpile) => new()
    {
        HoldingId = holdingId.ToTaggedString(),
        Capacity = stockpile.Capacity,
        // Original list order — see StockLotDto's doc comment for why this matters.
        Lots = stockpile.Lots.Select(ToStockLotDto).ToArray(),
        // Ordinal ReservationId order (ADR 0004) via Stockpile's SortedDictionary.
        Reservations = stockpile.Reservations.Select(ToStockReservationDto).ToArray(),
    };

    private static LedgerAccountDto ToLedgerAccountDto(LedgerAccount account) => new()
    {
        Kind = account.Key.Kind.ToString(),
        OwnerId = account.Key.OwnerId,
        Balance = account.Balance,
    };

    private static LedgerAccount FromLedgerAccountDto(LedgerAccountDto dto) =>
        new(new LedgerAccountKey(Enum.Parse<LedgerAccountKind>(dto.Kind), dto.OwnerId), dto.Balance);

    private static LedgerPostingDto ToLedgerPostingDto(LedgerPosting posting) => new()
    {
        Kind = posting.Account.Kind.ToString(),
        OwnerId = posting.Account.OwnerId,
        Amount = posting.Amount,
    };

    private static LedgerPosting FromLedgerPostingDto(LedgerPostingDto dto) =>
        new(new LedgerAccountKey(Enum.Parse<LedgerAccountKind>(dto.Kind), dto.OwnerId), dto.Amount);

    private static LedgerTransactionDto ToLedgerTransactionDto(LedgerTransaction transaction) => new()
    {
        Id = transaction.Id.ToTaggedString(),
        OccurredDateTotalMonths = transaction.OccurredDate.TotalMonths,
        Category = transaction.Category.ToString(),
        // Original posting order preserved — a transaction's postings are a fixed, authored sequence,
        // not a re-sortable collection.
        Postings = transaction.Postings.Select(ToLedgerPostingDto).ToArray(),
        Reference = transaction.Reference,
    };

    private static LedgerTransaction FromLedgerTransactionDto(LedgerTransactionDto dto) => new(
        RuntimeId<LedgerTransaction>.Parse(dto.Id),
        new GameDate(dto.OccurredDateTotalMonths),
        Enum.Parse<LedgerTransactionCategory>(dto.Category),
        dto.Postings.Select(FromLedgerPostingDto).ToArray(),
        dto.Reference);

    private static SettlementMarketDto ToSettlementMarketDto(SettlementMarket market) => new()
    {
        SettlementId = market.SettlementId.ToTaggedString(),
        GoodId = market.GoodId.Value,
        Price = market.Price,
        PreviousPrice = market.PreviousPrice,
        Supply = market.Supply,
        Demand = market.Demand,
        ClearedQuantity = market.ClearedQuantity,
        UnsatisfiedDemand = market.UnsatisfiedDemand,
    };

    private static SettlementMarket FromSettlementMarketDto(SettlementMarketDto dto) => new(
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        new DefinitionId<Good>(dto.GoodId),
        dto.Price,
        dto.PreviousPrice,
        dto.Supply,
        dto.Demand,
        dto.ClearedQuantity,
        dto.UnsatisfiedDemand);

    private static Stockpile FromStockpileDto(StockpileDto dto)
    {
        var stockpile = new Stockpile(dto.Capacity);
        foreach (var lot in dto.Lots)
        {
            stockpile.Add(
                FromGoodDefinitionDto(lot.Good),
                lot.Quantity,
                lot.Quality is null ? null : Enum.Parse<GoodQuality>(lot.Quality),
                lot.Condition is null ? null : new StockCondition(lot.Condition.Value),
                lot.Provenance is null ? null : FromStockProvenanceDto(lot.Provenance),
                lot.AgeInTicks);
        }

        foreach (var reservation in dto.Reservations)
        {
            stockpile.Reserve(
                reservation.ReservationId,
                new DefinitionId<Good>(reservation.GoodId),
                reservation.Quantity,
                reservation.Quality is null ? null : Enum.Parse<GoodQuality>(reservation.Quality));
        }

        return stockpile;
    }

    private static HouseholdMonthlyStatementDto ToHouseholdMonthlyStatementDto(HouseholdMonthlyStatement statement) => new()
    {
        HouseholdId = statement.HouseholdId.ToTaggedString(),
        MonthTotalMonths = statement.Month.TotalMonths,
        Income = statement.Income,
        Expenses = statement.Expenses,
        Net = statement.Net,
    };

    private static HouseholdMonthlyStatement FromHouseholdMonthlyStatementDto(HouseholdMonthlyStatementDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        new GameDate(dto.MonthTotalMonths),
        dto.Income,
        dto.Expenses,
        dto.Net);

    private static DebtRecordDto ToDebtRecordDto(DebtRecord debt) => new()
    {
        Id = debt.Id.ToTaggedString(),
        SettlementId = debt.SettlementId.ToTaggedString(),
        DebtorHouseholdId = debt.DebtorHouseholdId.ToTaggedString(),
        Principal = debt.Principal,
        InterestRate = debt.InterestRate,
        Origin = debt.Origin.ToString(),
        IsFenusNauticum = debt.IsFenusNauticum,
        MonthsOverdue = debt.MonthsOverdue,
        Status = debt.Status.ToString(),
        Resolution = debt.Resolution.ToString(),
    };

    private static DebtRecord FromDebtRecordDto(DebtRecordDto dto) => new(
        RuntimeId<DebtRecord>.Parse(dto.Id),
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        RuntimeId<Household>.Parse(dto.DebtorHouseholdId),
        dto.Principal,
        dto.InterestRate,
        Enum.Parse<DebtOrigin>(dto.Origin),
        dto.IsFenusNauticum,
        dto.MonthsOverdue,
        Enum.Parse<DebtStatus>(dto.Status),
        Enum.Parse<DebtResolution>(dto.Resolution));

    private static NetWorthDto ToNetWorthDto(NetWorth netWorth) => new()
    {
        HouseholdId = netWorth.HouseholdId.ToTaggedString(),
        MonthTotalMonths = netWorth.Month.TotalMonths,
        TreasuryBalance = netWorth.TreasuryBalance,
        StoredGoodsValue = netWorth.StoredGoodsValue,
        OutstandingDebt = netWorth.OutstandingDebt,
        Total = netWorth.Total,
    };

    private static NetWorth FromNetWorthDto(NetWorthDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        new GameDate(dto.MonthTotalMonths),
        dto.TreasuryBalance,
        dto.StoredGoodsValue,
        dto.OutstandingDebt,
        dto.Total);

    private static InsolvencyStateDto ToInsolvencyStateDto(InsolvencyState insolvency) => new()
    {
        HouseholdId = insolvency.HouseholdId.ToTaggedString(),
        MonthsBelowThreshold = insolvency.MonthsBelowThreshold,
        Stage = insolvency.Stage.ToString(),
        ConsequencesApplied = insolvency.ConsequencesApplied.Select(static c => c.ToString()).ToArray(),
    };

    private static InsolvencyState FromInsolvencyStateDto(InsolvencyStateDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.MonthsBelowThreshold,
        Enum.Parse<InsolvencyStage>(dto.Stage),
        dto.ConsequencesApplied.Select(static c => Enum.Parse<InsolvencyConsequence>(c)).ToArray());

    private static StandingContractDto ToStandingContractDto(StandingContract contract) => new()
    {
        Id = contract.Id.ToTaggedString(),
        Kind = contract.Kind.ToString(),
        SettlementId = contract.SettlementId.ToTaggedString(),
        HouseholdId = contract.HouseholdId.ToTaggedString(),
        Status = contract.Status.ToString(),
        HoldingId = contract.HoldingId?.ToTaggedString(),
        GoodId = contract.GoodId?.Value,
        QuantityPerMonth = contract.QuantityPerMonth,
        PriceOverMarketFraction = contract.PriceOverMarketFraction,
        DenariiCommitted = contract.DenariiCommitted,
        RouteName = contract.RouteName,
    };

    private static StandingContract FromStandingContractDto(StandingContractDto dto) => new(
        RuntimeId<StandingContract>.Parse(dto.Id),
        Enum.Parse<StandingContractKind>(dto.Kind),
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<StandingContractStatus>(dto.Status),
        dto.HoldingId is null ? null : RuntimeId<Holding>.Parse(dto.HoldingId),
        dto.GoodId is null ? null : new DefinitionId<Good>(dto.GoodId),
        dto.QuantityPerMonth,
        dto.PriceOverMarketFraction,
        dto.DenariiCommitted,
        dto.RouteName);

    private static HouseholdPolicyStateDto ToHouseholdPolicyStateDto(HouseholdPolicyState policy) => new()
    {
        HouseholdId = policy.HouseholdId.ToTaggedString(),
        RitesBudget = policy.RitesBudget.ToString(),
        LastChangedDateTotalMonths = policy.LastChangedDate.TotalMonths,
    };

    private static HouseholdPolicyState FromHouseholdPolicyStateDto(HouseholdPolicyStateDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<RitesBudgetTier>(dto.RitesBudget),
        new GameDate(dto.LastChangedDateTotalMonths));

    private static EventInstanceDto ToEventInstanceDto(EventInstance instance) => new()
    {
        InstanceId = instance.InstanceId.ToTaggedString(),
        DefinitionId = instance.DefinitionId.Value,
        Scope = instance.Scope.ToString(),
        SubjectIds = instance.SubjectIds,
        ActorId = instance.ActorId,
        CurrentStageIndex = instance.CurrentStageIndex,
        FiredDateTotalMonths = instance.FiredDate.TotalMonths,
        ExpiresDateTotalMonths = instance.ExpiresDate.TotalMonths,
        Status = instance.Status.ToString(),
        ResolvedOptionId = instance.ResolvedOptionId?.Value,
        ResolvedDateTotalMonths = instance.ResolvedDate?.TotalMonths,
        ResolvingEventId = instance.ResolvingEventId,
    };

    private static EventInstance FromEventInstanceDto(EventInstanceDto dto) => new(
        RuntimeId<EventInstance>.Parse(dto.InstanceId),
        new DefinitionId<EventDefinition>(dto.DefinitionId),
        Enum.Parse<EventScope>(dto.Scope),
        dto.SubjectIds,
        dto.ActorId,
        dto.CurrentStageIndex,
        new GameDate(dto.FiredDateTotalMonths),
        new GameDate(dto.ExpiresDateTotalMonths),
        Enum.Parse<EventInstanceStatus>(dto.Status),
        dto.ResolvedOptionId is null ? null : new DefinitionId<EventOptionDefinition>(dto.ResolvedOptionId),
        dto.ResolvedDateTotalMonths is null ? null : new GameDate(dto.ResolvedDateTotalMonths.Value),
        dto.ResolvingEventId);

    private static LivingWorldActorDto ToLivingWorldActorDto(LivingWorldActor actor) => new()
    {
        ActorId = actor.ActorId.ToTaggedString(),
        ActorType = actor.ActorType.ToString(),
        Name = actor.Name,
        Tier = actor.Tier.ToString(),
        StandingTrend = actor.StandingTrend.ToString(),
        OriginStory = actor.OriginStory.ToString(),
        ParentActorId = actor.ParentActorId?.ToTaggedString(),
        IdentityEconomic = actor.IdentityTags.Economic?.ToString(),
        IdentityFaction = actor.IdentityTags.Faction?.ToString(),
        HeadCharacterId = actor.HeadCharacterId?.ToTaggedString(),
        Dignitas = actor.Dignitas,
        NetWorthBand = actor.NetWorth.Band.ToString(),
        NetWorthFigure = actor.NetWorth.Figure?.RawValue,
        MilitaryStrengthBand = actor.MilitaryStrength.Band.ToString(),
        MilitaryStrengthResolvedForceId = actor.MilitaryStrength.ResolvedForceId,
        RegionId = actor.RegionId.ToTaggedString(),
        HomeSettlementId = actor.HomeSettlementId.ToTaggedString(),
        LastContactDateTotalMonths = actor.LastContactDate?.TotalMonths,
    };

    private static LivingWorldActor FromLivingWorldActorDto(LivingWorldActorDto dto) => LivingWorldActor.Create(
        RuntimeId<Actor>.Parse(dto.ActorId),
        Enum.Parse<LivingWorldActorType>(dto.ActorType),
        dto.Name,
        Enum.Parse<LivingWorldActorTier>(dto.Tier),
        Enum.Parse<LivingWorldActorStandingTrend>(dto.StandingTrend),
        Enum.Parse<LivingWorldActorOrigin>(dto.OriginStory),
        dto.ParentActorId is null ? null : RuntimeId<Actor>.Parse(dto.ParentActorId),
        new LivingWorldActorIdentity(
            dto.IdentityEconomic is null ? null : Enum.Parse<EconomicIdentityTag>(dto.IdentityEconomic),
            dto.IdentityFaction is null ? null : Enum.Parse<FactionTag>(dto.IdentityFaction)),
        dto.Dignitas,
        new LivingWorldActorNetWorth(
            Enum.Parse<HouseholdWealthBand>(dto.NetWorthBand),
            dto.NetWorthFigure is null ? null : Money.FromMinorUnits(dto.NetWorthFigure.Value)),
        new LivingWorldActorMilitaryStrength(
            Enum.Parse<MilitaryStrengthBand>(dto.MilitaryStrengthBand),
            dto.MilitaryStrengthResolvedForceId),
        RuntimeId<Region>.Parse(dto.RegionId),
        RuntimeId<Settlement>.Parse(dto.HomeSettlementId),
        dto.HeadCharacterId is null ? null : RuntimeId<Character>.Parse(dto.HeadCharacterId),
        dto.LastContactDateTotalMonths is null ? null : new GameDate(dto.LastContactDateTotalMonths.Value));

    private static HouseStandingDto ToHouseStandingDto(HouseStandingKey key, HouseStanding standing) => new()
    {
        ActorAId = key.ActorAId.ToTaggedString(),
        ActorBId = key.ActorBId.ToTaggedString(),
        Standing = standing.Standing.ToString(),
        GrudgeOriginEngagementId = standing.Grudge?.OriginEngagementId,
        GrudgeOriginDateTotalMonths = standing.Grudge?.OriginDate.TotalMonths,
    };

    private static KeyValuePair<HouseStandingKey, HouseStanding> FromHouseStandingDto(HouseStandingDto dto)
    {
        var key = HouseStandingKey.Between(RuntimeId<Actor>.Parse(dto.ActorAId), RuntimeId<Actor>.Parse(dto.ActorBId));
        var grudge = dto.GrudgeOriginEngagementId is null
            ? (AncestralGrudge?)null
            : new AncestralGrudge(dto.GrudgeOriginEngagementId, new GameDate(dto.GrudgeOriginDateTotalMonths!.Value));
        var standing = new HouseStanding(Enum.Parse<HouseStandingLevel>(dto.Standing), grudge);
        return new KeyValuePair<HouseStandingKey, HouseStanding>(key, standing);
    }

    private static RivalDossierDto ToRivalDossierDto(RivalDossier dossier) => new()
    {
        ActorId = dossier.ActorId.ToTaggedString(),
        Summary = dossier.Summary,
        HeadComboTitle = dossier.HeadComboTitle,
        LastUpdatedDateTotalMonths = dossier.LastUpdatedDate.TotalMonths,
        RecentChronicleEntries = dossier.RecentChronicleEntries.Select(id => id.ToTaggedString()).ToArray(),
    };

    private static RivalDossier FromRivalDossierDto(RivalDossierDto dto) => new(
        RuntimeId<Actor>.Parse(dto.ActorId),
        dto.Summary,
        dto.HeadComboTitle,
        new GameDate(dto.LastUpdatedDateTotalMonths),
        ParseChronicleEntryIds(dto.RecentChronicleEntries));

    /// <summary>Skips, rather than throws on, any value that isn't a real <c>chronentry_*</c> reference
    /// — a pre-Phase-11-item-3 save's <see cref="RivalDossierDto.RecentChronicleEntries"/> could carry
    /// this field's former arbitrary-placeholder-string stopgap shape, which would otherwise make the
    /// whole save unloadable. Matches this codebase's pre-v1 "no real campaign saves exist yet"
    /// additive-only save policy (ADR 0011) — silently dropping a handful of unresolvable dossier
    /// references is preferable to refusing the load entirely.</summary>
    private static List<RuntimeId<ChronicleEntry>> ParseChronicleEntryIds(IReadOnlyList<string> raw)
    {
        var parsed = new List<RuntimeId<ChronicleEntry>>(raw.Count);
        foreach (var value in raw)
        {
            if (TryParseChronicleEntryId(value, out var entryId))
                parsed.Add(entryId);
        }

        return parsed;
    }

    private static bool TryParseChronicleEntryId(string tagged, out RuntimeId<ChronicleEntry> entryId)
    {
        try
        {
            entryId = RuntimeId<ChronicleEntry>.Parse(tagged);
            return true;
        }
        catch (FormatException)
        {
            entryId = default;
            return false;
        }
    }

    private static RegionalFamiliesEntryDto ToRegionalFamiliesEntryDto(RegionalFamiliesEntry entry) => new()
    {
        ActorId = entry.ActorId.ToTaggedString(),
        Name = entry.Name,
        StandingTrend = entry.StandingTrend.ToString(),
        IdentityEconomic = entry.IdentityEconomic?.ToString(),
    };

    private static RegionalFamiliesEntry FromRegionalFamiliesEntryDto(RegionalFamiliesEntryDto dto) => new(
        RuntimeId<Actor>.Parse(dto.ActorId),
        dto.Name,
        Enum.Parse<LivingWorldActorStandingTrend>(dto.StandingTrend),
        dto.IdentityEconomic is null ? null : Enum.Parse<EconomicIdentityTag>(dto.IdentityEconomic));

    private static StewardshipAssignmentDto ToStewardshipAssignmentDto(StewardshipAssignment assignment) => new()
    {
        AssignmentId = assignment.AssignmentId.ToTaggedString(),
        HouseholdId = assignment.HouseholdId.ToTaggedString(),
        Context = assignment.Context.ToString(),
        Mode = assignment.Mode.ToString(),
        AppointeeCharacterId = assignment.AppointeeCharacterId?.ToTaggedString(),
        CouncilMembers = assignment.CouncilMembers
            .Select(m => new CouncilMemberDto { Domain = m.Domain.ToString(), CharacterId = m.CharacterId.ToTaggedString() })
            .ToArray(),
        CouncilHeadCharacterId = assignment.CouncilHeadCharacterId?.ToTaggedString(),
        AutonomyLevel = assignment.AutonomyLevel.ToString(),
        StartDateTotalMonths = assignment.StartDate.TotalMonths,
        EndDateTotalMonths = assignment.EndDate?.TotalMonths,
    };

    private static StewardshipAssignment FromStewardshipAssignmentDto(StewardshipAssignmentDto dto) => new(
        RuntimeId<StewardshipAssignment>.Parse(dto.AssignmentId),
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<StewardshipContext>(dto.Context),
        Enum.Parse<StewardshipMode>(dto.Mode),
        dto.AppointeeCharacterId is null ? null : RuntimeId<Character>.Parse(dto.AppointeeCharacterId),
        dto.CouncilMembers
            .Select(m => new CouncilMember(Enum.Parse<CouncilDomain>(m.Domain), RuntimeId<Character>.Parse(m.CharacterId)))
            .ToArray(),
        dto.CouncilHeadCharacterId is null ? null : RuntimeId<Character>.Parse(dto.CouncilHeadCharacterId),
        Enum.Parse<StewardAutonomyLevel>(dto.AutonomyLevel),
        new GameDate(dto.StartDateTotalMonths),
        dto.EndDateTotalMonths is null ? null : new GameDate(dto.EndDateTotalMonths.Value));

    private static AutonomousDecisionLogDto ToAutonomousDecisionLogDto(AutonomousDecisionLog log) => new()
    {
        LogId = log.LogId.ToTaggedString(),
        AssignmentId = log.AssignmentId.ToTaggedString(),
        MonthTotalMonths = log.Month.TotalMonths,
        DecisionType = log.DecisionType,
        Outcome = log.Outcome,
        CompetenceRollFactor = log.CompetenceRollFactor,
        LoyaltyRiskRollFactor = log.LoyaltyRiskRollFactor,
        IncidentType = log.IncidentType?.ToString(),
    };

    private static AutonomousDecisionLog FromAutonomousDecisionLogDto(AutonomousDecisionLogDto dto) => new(
        RuntimeId<AutonomousDecisionLog>.Parse(dto.LogId),
        RuntimeId<StewardshipAssignment>.Parse(dto.AssignmentId),
        new GameDate(dto.MonthTotalMonths),
        dto.DecisionType,
        dto.Outcome,
        dto.CompetenceRollFactor,
        dto.LoyaltyRiskRollFactor,
        dto.IncidentType is null ? null : Enum.Parse<StewardIncidentType>(dto.IncidentType));

    private static SchemeDto ToSchemeDto(Scheme scheme) => new()
    {
        SchemeId = scheme.SchemeId.ToTaggedString(),
        InitiatorCharacterId = scheme.InitiatorCharacterId.ToTaggedString(),
        TargetCharacterId = scheme.TargetCharacterId.ToTaggedString(),
        Type = scheme.Type.ToString(),
        Status = scheme.Status.ToString(),
        Progress = scheme.Progress,
        DiscoveryRisk = scheme.DiscoveryRisk,
        InitiatedDateTotalMonths = scheme.InitiatedDate.TotalMonths,
        LastProgressedDateTotalMonths = scheme.LastProgressedDate.TotalMonths,
    };

    private static Scheme FromSchemeDto(SchemeDto dto) => new(
        RuntimeId<Scheme>.Parse(dto.SchemeId),
        RuntimeId<Character>.Parse(dto.InitiatorCharacterId),
        RuntimeId<Character>.Parse(dto.TargetCharacterId),
        Enum.Parse<SchemeType>(dto.Type),
        Enum.Parse<SchemeStatus>(dto.Status),
        dto.Progress,
        dto.DiscoveryRisk,
        new GameDate(dto.InitiatedDateTotalMonths),
        new GameDate(dto.LastProgressedDateTotalMonths));

    private static ReturnReportDto ToReturnReportDto(ReturnReport report) => new()
    {
        ReportId = report.ReportId.ToTaggedString(),
        AssignmentId = report.AssignmentId.ToTaggedString(),
        SummaryEntries = report.SummaryEntries.ToArray(),
        TotalTreasuryImpactRawValue = report.TotalTreasuryImpact.RawValue,
        IncidentsDiscovered = report.IncidentsDiscovered.Select(id => id.ToTaggedString()).ToArray(),
        ChronicleWorthy = report.ChronicleWorthy,
    };

    private static ReturnReport FromReturnReportDto(ReturnReportDto dto) => new(
        RuntimeId<ReturnReport>.Parse(dto.ReportId),
        RuntimeId<StewardshipAssignment>.Parse(dto.AssignmentId),
        dto.SummaryEntries.ToArray(),
        Money.FromMinorUnits(dto.TotalTreasuryImpactRawValue),
        dto.IncidentsDiscovered.Select(RuntimeId<AutonomousDecisionLog>.Parse).ToArray(),
        dto.ChronicleWorthy);

    private static HouseholdHeadshipDto ToHouseholdHeadshipDto(HouseholdHeadship headship) => new()
    {
        HouseholdId = headship.HouseholdId.ToTaggedString(),
        HeadCharacterId = headship.HeadCharacterId.ToTaggedString(),
        SinceDateTotalMonths = headship.SinceDate.TotalMonths,
        RegentCharacterId = headship.RegentCharacterId?.ToTaggedString(),
    };

    private static HouseholdHeadship FromHouseholdHeadshipDto(HouseholdHeadshipDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        RuntimeId<Character>.Parse(dto.HeadCharacterId),
        new GameDate(dto.SinceDateTotalMonths),
        dto.RegentCharacterId is null ? null : RuntimeId<Character>.Parse(dto.RegentCharacterId));

    private static HeirDesignationDto ToHeirDesignationDto(HeirDesignation designation) => new()
    {
        HouseholdId = designation.HouseholdId.ToTaggedString(),
        PreferredHeirId = designation.PreferredHeirId?.ToTaggedString(),
        FormallyDeclaredHeirId = designation.FormallyDeclaredHeirId?.ToTaggedString(),
        DeclaredDateTotalMonths = designation.DeclaredDate?.TotalMonths,
        DisownedCharacterIds = designation.DisownedCharacterIds.Select(id => id.ToTaggedString()).ToArray(),
        AdoptedChildIds = designation.AdoptedChildIds.Select(id => id.ToTaggedString()).ToArray(),
        AcknowledgedIllegitimateChildIds = designation.AcknowledgedIllegitimateChildIds.Select(id => id.ToTaggedString()).ToArray(),
    };

    private static HeirDesignation FromHeirDesignationDto(HeirDesignationDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.PreferredHeirId is null ? null : RuntimeId<Character>.Parse(dto.PreferredHeirId),
        dto.FormallyDeclaredHeirId is null ? null : RuntimeId<Character>.Parse(dto.FormallyDeclaredHeirId),
        dto.DeclaredDateTotalMonths is { } declared ? new GameDate(declared) : null,
        dto.DisownedCharacterIds.Select(RuntimeId<Character>.Parse).ToArray(),
        dto.AdoptedChildIds.Select(RuntimeId<Character>.Parse).ToArray(),
        dto.AcknowledgedIllegitimateChildIds.Select(RuntimeId<Character>.Parse).ToArray());

    private static SuccessionDisputeDto ToSuccessionDisputeDto(SuccessionDispute dispute) => new()
    {
        DisputeId = dispute.DisputeId.ToTaggedString(),
        HouseholdId = dispute.HouseholdId.ToTaggedString(),
        DeceasedHeadId = dispute.DeceasedHeadId.ToTaggedString(),
        ClaimantIds = dispute.ClaimantIds.Select(id => id.ToTaggedString()).ToArray(),
        OpenedDateTotalMonths = dispute.OpenedDate.TotalMonths,
        ResolutionDueDateTotalMonths = dispute.ResolutionDueDate.TotalMonths,
        Status = dispute.Status.ToString(),
        WinnerCharacterId = dispute.WinnerCharacterId?.ToTaggedString(),
        SplinterClaimantId = dispute.SplinterClaimantId?.ToTaggedString(),
        SplinterHouseholdId = dispute.SplinterHouseholdId?.ToTaggedString(),
    };

    private static SuccessionDispute FromSuccessionDisputeDto(SuccessionDisputeDto dto) => new(
        RuntimeId<SuccessionDispute>.Parse(dto.DisputeId),
        RuntimeId<Household>.Parse(dto.HouseholdId),
        RuntimeId<Character>.Parse(dto.DeceasedHeadId),
        dto.ClaimantIds.Select(RuntimeId<Character>.Parse).ToArray(),
        new GameDate(dto.OpenedDateTotalMonths),
        new GameDate(dto.ResolutionDueDateTotalMonths),
        Enum.Parse<SuccessionDisputeStatus>(dto.Status),
        dto.WinnerCharacterId is null ? null : RuntimeId<Character>.Parse(dto.WinnerCharacterId),
        dto.SplinterClaimantId is null ? null : RuntimeId<Character>.Parse(dto.SplinterClaimantId),
        dto.SplinterHouseholdId is null ? null : RuntimeId<Household>.Parse(dto.SplinterHouseholdId));

    private static PlayerControlDto ToPlayerControlDto(PlayerControlState control) => new()
    {
        HouseholdId = control.HouseholdId.ToTaggedString(),
        ControlledCharacterId = control.ControlledCharacterId?.ToTaggedString(),
        Mode = control.Mode.ToString(),
    };

    private static PlayerControlState FromPlayerControlDto(PlayerControlDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.ControlledCharacterId is null ? null : RuntimeId<Character>.Parse(dto.ControlledCharacterId),
        Enum.Parse<PlayerControlMode>(dto.Mode));

    private static ChronicleEntryDto ToChronicleEntryDto(ChronicleEntry entry) => new()
    {
        EntryId = entry.EntryId.ToTaggedString(),
        HouseholdId = entry.HouseholdId?.ToTaggedString(),
        MonthTotalMonths = entry.Month.TotalMonths,
        Category = entry.Category.ToString(),
        Tier = entry.Tier.ToString(),
        Prose = entry.Prose,
        LinkedCharacterIds = entry.LinkedCharacterIds.Select(id => id.ToTaggedString()).ToArray(),
        SourceSystem = entry.SourceSystem,
        Source = entry.Source.ToString(),
        Pinned = entry.Pinned,
        PlayerAnnotation = entry.PlayerAnnotation,
        CrossHouseLinkedEntryId = entry.CrossHouseLinkedEntryId?.ToTaggedString(),
    };

    private static ChronicleEntry FromChronicleEntryDto(ChronicleEntryDto dto) => new(
        RuntimeId<ChronicleEntry>.Parse(dto.EntryId),
        dto.HouseholdId is null ? null : RuntimeId<Household>.Parse(dto.HouseholdId),
        new GameDate(dto.MonthTotalMonths),
        Enum.Parse<ChronicleCategory>(dto.Category),
        Enum.Parse<ChronicleTier>(dto.Tier),
        dto.Prose,
        dto.LinkedCharacterIds.Select(RuntimeId<Character>.Parse).ToArray(),
        dto.SourceSystem,
        Enum.Parse<ChronicleEntrySource>(dto.Source),
        dto.Pinned,
        dto.PlayerAnnotation,
        dto.CrossHouseLinkedEntryId is null ? null : RuntimeId<ChronicleEntry>.Parse(dto.CrossHouseLinkedEntryId));

    private static GenerationalChapterDto ToGenerationalChapterDto(GenerationalChapter chapter) => new()
    {
        HouseholdId = chapter.HouseholdId.ToTaggedString(),
        HeadCharacterId = chapter.HeadCharacterId.ToTaggedString(),
        StartMonthTotalMonths = chapter.StartMonth.TotalMonths,
        EndMonthTotalMonths = chapter.EndMonth?.TotalMonths,
        ChapterSummary = chapter.ChapterSummary,
    };

    private static GenerationalChapter FromGenerationalChapterDto(GenerationalChapterDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        RuntimeId<Character>.Parse(dto.HeadCharacterId),
        new GameDate(dto.StartMonthTotalMonths),
        dto.EndMonthTotalMonths is { } end ? new GameDate(end) : null,
        dto.ChapterSummary);

    private static FuneralRecordDto ToFuneralRecordDto(FuneralRecord funeral) => new()
    {
        FuneralId = funeral.FuneralId.ToTaggedString(),
        HouseholdId = funeral.HouseholdId.ToTaggedString(),
        DeceasedCharacterId = funeral.DeceasedCharacterId.ToTaggedString(),
        DeathDateTotalMonths = funeral.DeathDate.TotalMonths,
        Status = funeral.Status.ToString(),
        Tier = funeral.Tier?.ToString(),
        BurialMethod = funeral.BurialMethod?.ToString(),
        InterredAt = funeral.InterredAt?.ToString(),
        HeldDateTotalMonths = funeral.HeldDate?.TotalMonths,
        CostRawValue = funeral.Cost?.RawValue,
        MemoriaGained = funeral.MemoriaGained,
        ImaginesDisplayed = funeral.ImaginesDisplayed,
    };

    private static FuneralRecord FromFuneralRecordDto(FuneralRecordDto dto) => new(
        RuntimeId<FuneralRecord>.Parse(dto.FuneralId),
        RuntimeId<Household>.Parse(dto.HouseholdId),
        RuntimeId<Character>.Parse(dto.DeceasedCharacterId),
        new GameDate(dto.DeathDateTotalMonths),
        Enum.Parse<FuneralStatus>(dto.Status),
        dto.Tier is null ? null : Enum.Parse<FuneralTier>(dto.Tier),
        dto.BurialMethod is null ? null : Enum.Parse<BurialMethod>(dto.BurialMethod),
        dto.InterredAt is null ? null : Enum.Parse<IntermentDestination>(dto.InterredAt),
        dto.HeldDateTotalMonths is { } held ? new GameDate(held) : null,
        dto.CostRawValue is { } cost ? Money.FromMinorUnits(cost) : null,
        dto.MemoriaGained,
        dto.ImaginesDisplayed);

    private static MourningPeriodDto ToMourningPeriodDto(MourningPeriod period) => new()
    {
        HouseholdId = period.HouseholdId.ToTaggedString(),
        TriggeringDeathCharacterId = period.TriggeringDeathCharacterId.ToTaggedString(),
        StartDateTotalMonths = period.StartDate.TotalMonths,
        EndDateTotalMonths = period.EndDate.TotalMonths,
        BrokenEarly = period.BrokenEarly,
    };

    private static MourningPeriod FromMourningPeriodDto(MourningPeriodDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        RuntimeId<Character>.Parse(dto.TriggeringDeathCharacterId),
        new GameDate(dto.StartDateTotalMonths),
        new GameDate(dto.EndDateTotalMonths),
        dto.BrokenEarly);

    private static MemoriaStateDto ToMemoriaStateDto(MemoriaState memoria) => new()
    {
        HouseholdId = memoria.HouseholdId.ToTaggedString(),
        Memoria = memoria.Memoria,
        LastParentaliaObservedDateTotalMonths = memoria.LastParentaliaObservedDate?.TotalMonths,
    };

    private static MemoriaState FromMemoriaStateDto(MemoriaStateDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.Memoria,
        dto.LastParentaliaObservedDateTotalMonths is { } observed ? new GameDate(observed) : null);

    private static AgnomenDto ToAgnomenDto(Agnomen agnomen) => new()
    {
        AgnomenId = agnomen.AgnomenId.ToTaggedString(),
        CharacterId = agnomen.CharacterId.ToTaggedString(),
        AgnomenType = agnomen.AgnomenType.ToString(),
        Name = agnomen.Name,
        GrantMethod = agnomen.GrantMethod.ToString(),
        GrantedDateTotalMonths = agnomen.GrantedDate.TotalMonths,
        SourceChronicleEntryIds = agnomen.SourceChronicleEntryIds.Select(id => id.ToTaggedString()).ToArray(),
        SourceSuccessionDisputeId = agnomen.SourceSuccessionDisputeId?.ToTaggedString(),
        DignitasEffect = agnomen.DignitasEffect,
        FameEffect = agnomen.FameEffect,
        IsSuppressible = agnomen.IsSuppressible,
    };

    private static Agnomen FromAgnomenDto(AgnomenDto dto) => new(
        RuntimeId<Agnomen>.Parse(dto.AgnomenId),
        RuntimeId<Character>.Parse(dto.CharacterId),
        Enum.Parse<AgnomenType>(dto.AgnomenType),
        dto.Name,
        Enum.Parse<AgnomenGrantMethod>(dto.GrantMethod),
        new GameDate(dto.GrantedDateTotalMonths),
        dto.SourceChronicleEntryIds.Select(RuntimeId<ChronicleEntry>.Parse).ToArray(),
        dto.SourceSuccessionDisputeId is null ? null : RuntimeId<SuccessionDispute>.Parse(dto.SourceSuccessionDisputeId),
        dto.DignitasEffect,
        dto.FameEffect,
        dto.IsSuppressible);

    private static InheritedCognomenDecisionDto ToInheritedCognomenDecisionDto(InheritedCognomenDecision decision) => new()
    {
        DecisionId = decision.DecisionId.ToTaggedString(),
        OriginalAgnomenId = decision.OriginalAgnomenId.ToTaggedString(),
        DecidingHouseholdId = decision.DecidingHouseholdId.ToTaggedString(),
        AdoptedAsPermanentCognomen = decision.AdoptedAsPermanentCognomen,
        EffectiveFromDateTotalMonths = decision.EffectiveFromDate.TotalMonths,
    };

    private static InheritedCognomenDecision FromInheritedCognomenDecisionDto(InheritedCognomenDecisionDto dto) => new(
        RuntimeId<InheritedCognomenDecision>.Parse(dto.DecisionId),
        RuntimeId<Agnomen>.Parse(dto.OriginalAgnomenId),
        RuntimeId<Household>.Parse(dto.DecidingHouseholdId),
        dto.AdoptedAsPermanentCognomen,
        new GameDate(dto.EffectiveFromDateTotalMonths));

    private static DynasticEpithetDto ToDynasticEpithetDto(DynasticEpithet epithet) => new()
    {
        HouseholdId = epithet.HouseholdId.ToTaggedString(),
        EpithetText = epithet.EpithetText,
        DerivedFromChronicleEntryIds = epithet.DerivedFromChronicleEntryIds.Select(id => id.ToTaggedString()).ToArray(),
    };

    private static DynasticEpithet FromDynasticEpithetDto(DynasticEpithetDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.EpithetText,
        dto.DerivedFromChronicleEntryIds.Select(RuntimeId<ChronicleEntry>.Parse).ToArray());

    private static HouseholdReputationDto ToHouseholdReputationDto(HouseholdReputation reputation) => new()
    {
        HouseholdId = reputation.HouseholdId.ToTaggedString(),
        Dignitas = reputation.Dignitas,
    };

    private static HouseholdReputation FromHouseholdReputationDto(HouseholdReputationDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.Dignitas);

    private static FavorObligationDto ToFavorObligationDto(FavorObligation favor) => new()
    {
        FavorId = favor.FavorId.ToTaggedString(),
        GrantorId = favor.GrantorId.ToTaggedString(),
        BeneficiaryId = favor.BeneficiaryId.ToTaggedString(),
        Kind = favor.Kind,
        GrantedDateTotalMonths = favor.GrantedDate.TotalMonths,
        Status = favor.Status.ToString(),
        ResolvedDateTotalMonths = favor.ResolvedDate?.TotalMonths,
    };

    private static FavorObligation FromFavorObligationDto(FavorObligationDto dto) => new(
        RuntimeId<FavorObligation>.Parse(dto.FavorId),
        RuntimeId<Character>.Parse(dto.GrantorId),
        RuntimeId<Character>.Parse(dto.BeneficiaryId),
        dto.Kind,
        new GameDate(dto.GrantedDateTotalMonths),
        Enum.Parse<FavorStatus>(dto.Status),
        dto.ResolvedDateTotalMonths is { } resolved ? new GameDate(resolved) : null);

    private static ClientelaEntryDto ToClientelaEntryDto(ClientelaEntry entry) => new()
    {
        ClientId = entry.ClientId.ToTaggedString(),
        PatronHouseholdId = entry.PatronHouseholdId.ToTaggedString(),
        Specialty = entry.Specialty.ToString(),
        RecruitedDateTotalMonths = entry.RecruitedDate.TotalMonths,
        LastFavorCalledDateTotalMonths = entry.LastFavorCalledDate?.TotalMonths,
    };

    private static ClientelaEntry FromClientelaEntryDto(ClientelaEntryDto dto) => new(
        RuntimeId<Character>.Parse(dto.ClientId),
        RuntimeId<Household>.Parse(dto.PatronHouseholdId),
        Enum.Parse<ClientSpecialty>(dto.Specialty),
        new GameDate(dto.RecruitedDateTotalMonths),
        dto.LastFavorCalledDateTotalMonths is { } lastFavor ? new GameDate(lastFavor) : null);

    private static HouseholdInfluenceDto ToHouseholdInfluenceDto(HouseholdInfluence influence) => new()
    {
        HouseholdId = influence.HouseholdId.ToTaggedString(),
        Influence = influence.Influence,
    };

    private static HouseholdInfluence FromHouseholdInfluenceDto(HouseholdInfluenceDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        dto.Influence);

    private static CharacterFactionAlignmentDto ToCharacterFactionAlignmentDto(CharacterFactionAlignment alignment) => new()
    {
        CharacterId = alignment.CharacterId.ToTaggedString(),
        Faction = alignment.Faction.ToString(),
    };

    private static CharacterFactionAlignment FromCharacterFactionAlignmentDto(CharacterFactionAlignmentDto dto) => new(
        RuntimeId<Character>.Parse(dto.CharacterId),
        Enum.Parse<PoliticalFaction>(dto.Faction));

    private static MagistracyRecordDto ToMagistracyRecordDto(MagistracyRecord record) => new()
    {
        RecordId = record.RecordId.ToTaggedString(),
        HolderId = record.HolderId.ToTaggedString(),
        Office = record.Office.ToString(),
        SettlementId = record.SettlementId.ToTaggedString(),
        TermStartDateTotalMonths = record.TermStartDate.TotalMonths,
        TermEndDateTotalMonths = record.TermEndDate?.TotalMonths,
        LossReason = record.LossReason?.ToString(),
        CoHolderId = record.CoHolderId?.ToTaggedString(),
    };

    private static MagistracyRecord FromMagistracyRecordDto(MagistracyRecordDto dto) => new(
        RuntimeId<MagistracyRecord>.Parse(dto.RecordId),
        RuntimeId<Character>.Parse(dto.HolderId),
        Enum.Parse<MagistracyOffice>(dto.Office),
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        new GameDate(dto.TermStartDateTotalMonths),
        dto.TermEndDateTotalMonths is { } end ? new GameDate(end) : null,
        dto.LossReason is { } loss ? Enum.Parse<MagistracyLossReason>(loss) : null,
        dto.CoHolderId is { } coHolder ? RuntimeId<Character>.Parse(coHolder) : null);

    private static HouseholdReligionDto ToHouseholdReligionDto(HouseholdReligion religion) => new()
    {
        HouseholdId = religion.HouseholdId.ToTaggedString(),
        PatronDeity = religion.PatronDeity.ToString(),
        Favor = religion.Favor,
        ConsecratedUnderHeadCharacterId = religion.ConsecratedUnderHeadCharacterId.ToTaggedString(),
    };

    private static HouseholdReligion FromHouseholdReligionDto(HouseholdReligionDto dto) => new(
        RuntimeId<Household>.Parse(dto.HouseholdId),
        Enum.Parse<PatronDeity>(dto.PatronDeity),
        dto.Favor,
        RuntimeId<Character>.Parse(dto.ConsecratedUnderHeadCharacterId));

    private static OmenEventDto ToOmenEventDto(OmenEvent omen) => new()
    {
        OmenId = omen.OmenId.ToTaggedString(),
        HouseholdId = omen.HouseholdId.ToTaggedString(),
        RaisedDateTotalMonths = omen.RaisedDate.TotalMonths,
        ThemedDeity = omen.ThemedDeity.ToString(),
        Severity = omen.Severity,
        PlayerChoice = omen.PlayerChoice?.ToString(),
        Outcome = omen.Outcome.ToString(),
    };

    private static OmenEvent FromOmenEventDto(OmenEventDto dto) => new(
        RuntimeId<OmenEvent>.Parse(dto.OmenId),
        RuntimeId<Household>.Parse(dto.HouseholdId),
        new GameDate(dto.RaisedDateTotalMonths),
        Enum.Parse<PatronDeity>(dto.ThemedDeity),
        dto.Severity,
        dto.PlayerChoice is { } choice ? Enum.Parse<OmenChoice>(choice) : null,
        Enum.Parse<OmenOutcome>(dto.Outcome));

    private static PriesthoodRecordDto ToPriesthoodRecordDto(PriesthoodRecord record) => new()
    {
        RecordId = record.RecordId.ToTaggedString(),
        HolderId = record.HolderId.ToTaggedString(),
        Office = record.Office.ToString(),
        SettlementId = record.SettlementId.ToTaggedString(),
        AppointedDateTotalMonths = record.AppointedDate.TotalMonths,
        FlamenDeity = record.FlamenDeity?.ToString(),
        EndDateTotalMonths = record.EndDate?.TotalMonths,
    };

    private static PriesthoodRecord FromPriesthoodRecordDto(PriesthoodRecordDto dto) => new(
        RuntimeId<PriesthoodRecord>.Parse(dto.RecordId),
        RuntimeId<Character>.Parse(dto.HolderId),
        Enum.Parse<PriesthoodOffice>(dto.Office),
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        new GameDate(dto.AppointedDateTotalMonths),
        dto.FlamenDeity is { } deity ? Enum.Parse<PatronDeity>(deity) : null,
        dto.EndDateTotalMonths is { } end ? new GameDate(end) : null);

    private static LegalCaseDto ToLegalCaseDto(LegalCase legalCase) => new()
    {
        CaseId = legalCase.CaseId.ToTaggedString(),
        CaseType = legalCase.CaseType.ToString(),
        PlaintiffId = legalCase.PlaintiffId.ToTaggedString(),
        DefendantId = legalCase.DefendantId.ToTaggedString(),
        SettlementId = legalCase.SettlementId.ToTaggedString(),
        Depth = legalCase.Depth.ToString(),
        Stage = legalCase.Stage.ToString(),
        FiledDateTotalMonths = legalCase.FiledDate.TotalMonths,
        PresidingCharacterId = legalCase.PresidingCharacterId?.ToTaggedString(),
        PresidingCharacterScouted = legalCase.PresidingCharacterScouted,
        PlaintiffCaseStrength = legalCase.PlaintiffCaseStrength,
        DefendantCaseStrength = legalCase.DefendantCaseStrength,
        PlaintiffBriberyWeight = legalCase.PlaintiffBriberyWeight,
        DefendantBriberyWeight = legalCase.DefendantBriberyWeight,
        IsPatriaPotestasCase = legalCase.IsPatriaPotestasCase,
        Verdict = legalCase.Verdict?.ToString(),
        Sentence = legalCase.Sentence?.ToString(),
        RuledDateTotalMonths = legalCase.RuledDate?.TotalMonths,
    };

    private static LegalCase FromLegalCaseDto(LegalCaseDto dto) => new(
        RuntimeId<LegalCase>.Parse(dto.CaseId),
        Enum.Parse<LegalCaseType>(dto.CaseType),
        RuntimeId<Household>.Parse(dto.PlaintiffId),
        RuntimeId<Household>.Parse(dto.DefendantId),
        RuntimeId<Settlement>.Parse(dto.SettlementId),
        Enum.Parse<LegalCaseDepth>(dto.Depth),
        Enum.Parse<LegalCaseStage>(dto.Stage),
        new GameDate(dto.FiledDateTotalMonths),
        dto.PresidingCharacterId is { } presider ? RuntimeId<Character>.Parse(presider) : null,
        dto.PresidingCharacterScouted,
        dto.PlaintiffCaseStrength,
        dto.DefendantCaseStrength,
        dto.PlaintiffBriberyWeight,
        dto.DefendantBriberyWeight,
        dto.IsPatriaPotestasCase,
        dto.Verdict is { } verdict ? Enum.Parse<LegalCaseVerdict>(verdict) : null,
        dto.Sentence is { } sentence ? Enum.Parse<LegalSentence>(sentence) : null,
        dto.RuledDateTotalMonths is { } ruled ? new GameDate(ruled) : null);
}
