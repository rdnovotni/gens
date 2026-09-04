using Gens.Simulation.Actors;
using Gens.Simulation.Buildings;
using Gens.Simulation.BusinessCompetition;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Clientela;
using Gens.Simulation.Collegia;
using Gens.Simulation.Correspondence;
using Gens.Simulation.Crime;
using Gens.Simulation.Doctrine;
using Gens.Simulation.Economy;
using Gens.Simulation.Edicts;
using Gens.Simulation.Epithets;
using Gens.Simulation.Events;
using Gens.Simulation.Fame;
using Gens.Simulation.Funerary;
using Gens.Simulation.Goods;
using Gens.Simulation.Hazards;
using Gens.Simulation.Health;
using Gens.Simulation.History;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.Land;
using Gens.Simulation.Languages;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.Markets;
using Gens.Simulation.MerchantFamilies;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Policies;
using Gens.Simulation.PrivateInfrastructure;
using Gens.Simulation.PublicContracts;
using Gens.Simulation.PublicWorks;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.Shipping;
using Gens.Simulation.Societates;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Succession;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;
using Gens.Simulation.Wanderers;

namespace Gens.Simulation.State;

/// <summary>
/// The sole authoritative campaign truth container (ADR 0004). Holds one <see
/// cref="RuntimeIdCounter{T}"/> per runtime-instantiated entity kind (ADR 0001) — itself campaign
/// state, saved and restored like any other field — plus the ordered-index partitions those IDs
/// key into, the knowledge/visibility partition (ADR 0008), and the campaign clock. The <c>Characters</c>
/// partition holds the real <see cref="Character"/> record (Phase 5 item 1); every other entity
/// record partition remains a typed placeholder until its own real record lands in the phase that
/// designs it.
/// </summary>
public sealed class WorldState
{
    public WorldState(GameDate date)
    {
        Date = date;
    }

    /// <summary>Reconstructs a <see cref="WorldState"/> from persisted save data (ADR 0010). Every
    /// counter, ordered partition, and the command sequence number are restored exactly as captured —
    /// this constructor performs no validation of cross-entity consistency beyond what each restored
    /// component already enforces (e.g. <see cref="RuntimeIdCounter{T}.Restore"/> rejecting a negative
    /// counter).</summary>
    internal WorldState(
        GameDate date,
        RuntimeIdCounter<Region> regionIds,
        RuntimeIdCounter<Settlement> settlementIds,
        RuntimeIdCounter<Plot> plotIds,
        RuntimeIdCounter<Holding> holdingIds,
        RuntimeIdCounter<Household> householdIds,
        RuntimeIdCounter<Actor> actorIds,
        RuntimeIdCounter<Character> characterIds,
        RuntimeIdCounter<Building> buildingIds,
        RuntimeIdCounter<Contract> contractIds,
        RuntimeIdCounter<Activity> activityIds,
        RuntimeIdCounter<Command> commandIds,
        RuntimeIdCounter<DomainEventEntity> eventIds,
        RuntimeIdCounter<ScheduledAction> scheduledActionIds,
        RuntimeIdCounter<LedgerTransaction> ledgerTransactionIds,
        RuntimeIdCounter<DebtRecord> debtRecordIds,
        RuntimeIdCounter<StandingContract> standingContractIds,
        RuntimeIdCounter<EventInstance> eventInstanceIds,
        RuntimeIdCounter<StewardshipAssignment> stewardshipAssignmentIds,
        RuntimeIdCounter<AutonomousDecisionLog> autonomousDecisionLogIds,
        RuntimeIdCounter<Scheme> schemeIds,
        RuntimeIdCounter<ReturnReport> returnReportIds,
        RuntimeIdCounter<SuccessionDispute> successionDisputeIds,
        RuntimeIdCounter<ChronicleEntry> chronicleEntryIds,
        RuntimeIdCounter<FuneralRecord> funeralRecordIds,
        RuntimeIdCounter<Agnomen> agnomenIds,
        RuntimeIdCounter<InheritedCognomenDecision> inheritedCognomenDecisionIds,
        RuntimeIdCounter<FavorObligation> favorObligationIds,
        RuntimeIdCounter<MagistracyRecord> magistracyRecordIds,
        RuntimeIdCounter<OmenEvent> omenEventIds,
        RuntimeIdCounter<PriesthoodRecord> priesthoodRecordIds,
        RuntimeIdCounter<LegalCase> legalCaseIds,
        RuntimeIdCounter<PunishableOffense> punishableOffenseIds,
        RuntimeIdCounter<DetentionRecord> detentionRecordIds,
        RuntimeIdCounter<SentenceRecord> sentenceRecordIds,
        RuntimeIdCounter<RansomNegotiation> ransomNegotiationIds,
        RuntimeIdCounter<ScandalRecord> scandalRecordIds,
        RuntimeIdCounter<EdictRecord> edictRecordIds,
        RuntimeIdCounter<TravelTrip> travelTripIds,
        RuntimeIdCounter<Letter> letterIds,
        RuntimeIdCounter<LanguageProficiency> languageProficiencyIds,
        RuntimeIdCounter<DivergenceRecord> divergenceRecordIds,
        RuntimeIdCounter<DistantHolding> distantHoldingIds,
        RuntimeIdCounter<CharacterHealthCondition> characterHealthConditionIds,
        RuntimeIdCounter<EpidemicOutbreak> epidemicOutbreakIds,
        RuntimeIdCounter<DisasterEvent> disasterEventIds,
        RuntimeIdCounter<Wanderer> wandererIds,
        RuntimeIdCounter<WandererEngagement> wandererEngagementIds,
        RuntimeIdCounter<District> districtIds,
        RuntimeIdCounter<PropertyRecord> propertyRecordIds,
        RuntimeIdCounter<Societas> societasIds,
        RuntimeIdCounter<NotableBusiness> notableBusinessIds,
        RuntimeIdCounter<CartelAgreement> cartelAgreementIds,
        RuntimeIdCounter<PublicContract> publicContractIds,
        RuntimeIdCounter<ContractBid> contractBidIds,
        RuntimeIdCounter<LustrumEvent> lustrumEventIds,
        RuntimeIdCounter<ContractFraudRecord> contractFraudRecordIds,
        RuntimeIdCounter<PavedRoadConnection> pavedRoadConnectionIds,
        RuntimeIdCounter<PrivateBridge> privateBridgeIds,
        RuntimeIdCounter<MerchantShip> merchantShipIds,
        RuntimeIdCounter<ShipCommissionProject> shipCommissionProjectIds,
        RuntimeIdCounter<VoyageEvent> voyageEventIds,
        RuntimeIdCounter<PublicWork> publicWorkIds,
        RuntimeIdCounter<CompetitiveEuergetismEvent> competitiveEuergetismEventIds,
        OrderedRegistry<RuntimeId<Region>, Region> regions,
        OrderedRegistry<RuntimeId<Settlement>, Settlement> settlements,
        OrderedRegistry<RuntimeId<Plot>, Plot> plots,
        OrderedRegistry<RuntimeId<Holding>, Holding> holdings,
        OrderedRegistry<RuntimeId<Character>, Character> characters,
        OrderedRegistry<RelationshipKey, Relationship> relationships,
        OrderedRegistry<ScheduledActionKey, ScheduledActionEntry> scheduledActions,
        OrderedRegistry<PopGroupKey, PopGroup> popGroups,
        OrderedRegistry<HouseholdRegimenKey, RegimenSettings> householdRegimenDefaults,
        OrderedRegistry<RuntimeId<Building>, BuildingInstance> buildings,
        OrderedRegistry<RuntimeId<Holding>, Stockpile> stockpiles,
        OrderedRegistry<RuntimeId<Holding>, ConstructionSchedule> constructionSchedules,
        OrderedRegistry<LedgerAccountKey, LedgerAccount> ledgerAccounts,
        OrderedRegistry<RuntimeId<LedgerTransaction>, LedgerTransaction> ledgerTransactions,
        OrderedRegistry<MarketGoodKey, SettlementMarket> marketPrices,
        OrderedRegistry<RuntimeId<Household>, HouseholdMonthlyStatement> householdStatements,
        OrderedRegistry<RuntimeId<DebtRecord>, DebtRecord> debtRecords,
        OrderedRegistry<RuntimeId<Household>, NetWorth> netWorthAssessments,
        OrderedRegistry<RuntimeId<Household>, InsolvencyState> insolvencyStates,
        OrderedRegistry<RuntimeId<StandingContract>, StandingContract> standingContracts,
        OrderedRegistry<RuntimeId<Household>, HouseholdPolicyState> householdPolicies,
        OrderedRegistry<RuntimeId<EventInstance>, EventInstance> eventInstances,
        OrderedRegistry<RuntimeId<Actor>, LivingWorldActor> actors,
        OrderedRegistry<HouseStandingKey, HouseStanding> houseStandings,
        OrderedRegistry<RuntimeId<Actor>, RivalDossier> rivalDossiers,
        OrderedRegistry<RuntimeId<Actor>, RegionalFamiliesEntry> regionalFamiliesEntries,
        OrderedRegistry<RuntimeId<StewardshipAssignment>, StewardshipAssignment> stewardshipAssignments,
        OrderedRegistry<RuntimeId<AutonomousDecisionLog>, AutonomousDecisionLog> autonomousDecisionLogs,
        OrderedRegistry<RuntimeId<Scheme>, Scheme> schemes,
        OrderedRegistry<RuntimeId<ReturnReport>, ReturnReport> returnReports,
        OrderedRegistry<RuntimeId<Household>, HouseholdHeadship> householdHeadships,
        OrderedRegistry<RuntimeId<Household>, HeirDesignation> heirDesignations,
        OrderedRegistry<RuntimeId<SuccessionDispute>, SuccessionDispute> successionDisputes,
        OrderedRegistry<RuntimeId<Household>, PlayerControlState> playerControls,
        OrderedRegistry<RuntimeId<ChronicleEntry>, ChronicleEntry> chronicleEntries,
        OrderedRegistry<GenerationalChapterKey, GenerationalChapter> generationalChapters,
        OrderedRegistry<RuntimeId<FuneralRecord>, FuneralRecord> funeralRecords,
        OrderedRegistry<RuntimeId<Household>, MourningPeriod> mourningPeriods,
        OrderedRegistry<RuntimeId<Household>, MemoriaState> memoriaStates,
        OrderedRegistry<RuntimeId<Agnomen>, Agnomen> agnomens,
        OrderedRegistry<RuntimeId<InheritedCognomenDecision>, InheritedCognomenDecision> inheritedCognomenDecisions,
        OrderedRegistry<RuntimeId<Household>, DynasticEpithet> dynasticEpithets,
        OrderedRegistry<RuntimeId<Household>, HouseholdReputation> householdReputations,
        OrderedRegistry<RuntimeId<FavorObligation>, FavorObligation> favorObligations,
        OrderedRegistry<RuntimeId<Character>, ClientelaEntry> clientelaEntries,
        OrderedRegistry<RuntimeId<Household>, HouseholdInfluence> householdInfluences,
        OrderedRegistry<RuntimeId<Character>, CharacterFactionAlignment> characterFactionAlignments,
        OrderedRegistry<RuntimeId<MagistracyRecord>, MagistracyRecord> magistracyRecords,
        OrderedRegistry<RuntimeId<Household>, HouseholdReligion> householdReligions,
        OrderedRegistry<RuntimeId<OmenEvent>, OmenEvent> omenEvents,
        OrderedRegistry<RuntimeId<PriesthoodRecord>, PriesthoodRecord> priesthoodRecords,
        OrderedRegistry<RuntimeId<LegalCase>, LegalCase> legalCases,
        OrderedRegistry<RuntimeId<PunishableOffense>, PunishableOffense> punishableOffenses,
        OrderedRegistry<RuntimeId<DetentionRecord>, DetentionRecord> detentionRecords,
        OrderedRegistry<RuntimeId<SentenceRecord>, SentenceRecord> sentenceRecords,
        OrderedRegistry<RuntimeId<RansomNegotiation>, RansomNegotiation> ransomNegotiations,
        OrderedRegistry<RuntimeId<Actor>, CollegiumDetails> collegia,
        OrderedRegistry<RuntimeId<ScandalRecord>, ScandalRecord> scandalRecords,
        OrderedRegistry<RuntimeId<Character>, CharacterFame> characterFames,
        OrderedRegistry<HouseholdDoctrineKey, HouseholdDoctrineState> householdDoctrines,
        OrderedRegistry<RuntimeId<EdictRecord>, EdictRecord> edictRecords,
        OrderedRegistry<RuntimeId<TravelTrip>, TravelTrip> travelTrips,
        OrderedRegistry<RuntimeId<Letter>, Letter> letters,
        OrderedRegistry<RuntimeId<LanguageProficiency>, LanguageProficiency> languageProficiencies,
        OrderedRegistry<RuntimeId<Character>, LiteracyRecord> literacyRecords,
        OrderedRegistry<RuntimeId<Household>, InterpresAppointment> interpresAppointments,
        OrderedRegistry<RuntimeId<DivergenceRecord>, DivergenceRecord> divergenceRecords,
        OrderedRegistry<RuntimeId<DistantHolding>, DistantHolding> distantHoldings,
        OrderedRegistry<RuntimeId<CharacterHealthCondition>, CharacterHealthCondition> characterHealthConditions,
        OrderedRegistry<RuntimeId<Settlement>, SettlementSanitationInvestment> settlementSanitationInvestments,
        OrderedRegistry<RuntimeId<EpidemicOutbreak>, EpidemicOutbreak> epidemicOutbreaks,
        OrderedRegistry<RuntimeId<DisasterEvent>, DisasterEvent> disasterEvents,
        OrderedRegistry<RuntimeId<Plot>, DormantVolcano> dormantVolcanoes,
        OrderedRegistry<RuntimeId<Wanderer>, Wanderer> wanderers,
        OrderedRegistry<RuntimeId<WandererEngagement>, WandererEngagement> wandererEngagements,
        OrderedRegistry<string, GameDate> firedHistoricalTimelineEntryIds,
        OrderedRegistry<RuntimeId<District>, District> districts,
        OrderedRegistry<RuntimeId<PropertyRecord>, PropertyRecord> propertyRecords,
        OrderedRegistry<RuntimeId<Plot>, PlotPropertyExtension> plotPropertyExtensions,
        OrderedRegistry<RuntimeId<Societas>, Societas> societates,
        OrderedRegistry<RuntimeId<LegalCase>, ActioProSocioLink> actioProSocioLinks,
        OrderedRegistry<string, MerchantHouseArchetype> merchantHouseArchetypes,
        OrderedRegistry<RuntimeId<Household>, SenateEntryInvestmentLog> senateEntryInvestmentLogs,
        OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusiness> notableBusinesses,
        OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusinessRivalryLog> notableBusinessRivalryLogs,
        OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusinessGovernmentContract> notableBusinessGovernmentContracts,
        OrderedRegistry<RuntimeId<NotableBusiness>, CompetitiveEscalation> competitiveEscalations,
        OrderedRegistry<RuntimeId<CartelAgreement>, CartelAgreement> cartelAgreements,
        OrderedRegistry<RuntimeId<NotableBusiness>, GrainHoardingRecord> grainHoardingRecords,
        OrderedRegistry<MarketGoodKey, MarketCapacityReading> marketCapacityReadings,
        OrderedRegistry<RuntimeId<PublicContract>, PublicContract> publicContracts,
        OrderedRegistry<RuntimeId<ContractBid>, ContractBid> contractBids,
        OrderedRegistry<RuntimeId<LustrumEvent>, LustrumEvent> lustrumEvents,
        OrderedRegistry<RuntimeId<ContractFraudRecord>, ContractFraudRecord> publicContractFraudRecords,
        OrderedRegistry<RuntimeId<LegalCase>, ContractFraudLegalLink> contractFraudLegalLinks,
        OrderedRegistry<RuntimeId<PavedRoadConnection>, PavedRoadConnection> pavedRoadConnections,
        OrderedRegistry<RuntimeId<Plot>, IrrigationCanal> irrigationCanals,
        OrderedRegistry<RuntimeId<Plot>, WellOrCistern> wellOrCisterns,
        OrderedRegistry<RuntimeId<Plot>, LandReclamationProject> landReclamationProjects,
        OrderedRegistry<RuntimeId<PrivateBridge>, PrivateBridge> privateBridges,
        OrderedRegistry<RuntimeId<Plot>, BoundaryInfrastructure> boundaryInfrastructures,
        OrderedRegistry<InfrastructureConditionKey, InfrastructureCondition> infrastructureConditions,
        OrderedRegistry<RuntimeId<Household>, GameDate> unifiedEstateMilestones,
        OrderedRegistry<RuntimeId<MerchantShip>, MerchantShip> merchantShips,
        OrderedRegistry<RuntimeId<ShipCommissionProject>, ShipCommissionProject> shipCommissionProjects,
        OrderedRegistry<RuntimeId<MerchantShip>, FrontingArrangement> shipFrontingArrangements,
        OrderedRegistry<RuntimeId<VoyageEvent>, VoyageEvent> voyageEvents,
        OrderedRegistry<RuntimeId<Household>, GameDate> flagshipDesignationAwards,
        OrderedRegistry<RuntimeId<PublicWork>, PublicWork> publicWorks,
        OrderedRegistry<RuntimeId<Household>, EuergetismObligation> euergetismObligations,
        OrderedRegistry<RuntimeId<CompetitiveEuergetismEvent>, CompetitiveEuergetismEvent> competitiveEuergetismEvents,
        KnowledgeState knowledge,
        long nextCommandSequenceNumber)
    {
        Date = date;
        RegionIds = regionIds;
        SettlementIds = settlementIds;
        PlotIds = plotIds;
        HoldingIds = holdingIds;
        HouseholdIds = householdIds;
        ActorIds = actorIds;
        CharacterIds = characterIds;
        BuildingIds = buildingIds;
        ContractIds = contractIds;
        ActivityIds = activityIds;
        CommandIds = commandIds;
        EventIds = eventIds;
        ScheduledActionIds = scheduledActionIds;
        LedgerTransactionIds = ledgerTransactionIds;
        DebtRecordIds = debtRecordIds;
        StandingContractIds = standingContractIds;
        EventInstanceIds = eventInstanceIds;
        StewardshipAssignmentIds = stewardshipAssignmentIds;
        AutonomousDecisionLogIds = autonomousDecisionLogIds;
        SchemeIds = schemeIds;
        ReturnReportIds = returnReportIds;
        SuccessionDisputeIds = successionDisputeIds;
        ChronicleEntryIds = chronicleEntryIds;
        FuneralRecordIds = funeralRecordIds;
        AgnomenIds = agnomenIds;
        InheritedCognomenDecisionIds = inheritedCognomenDecisionIds;
        FavorObligationIds = favorObligationIds;
        MagistracyRecordIds = magistracyRecordIds;
        OmenEventIds = omenEventIds;
        PriesthoodRecordIds = priesthoodRecordIds;
        LegalCaseIds = legalCaseIds;
        PunishableOffenseIds = punishableOffenseIds;
        DetentionRecordIds = detentionRecordIds;
        SentenceRecordIds = sentenceRecordIds;
        RansomNegotiationIds = ransomNegotiationIds;
        ScandalRecordIds = scandalRecordIds;
        EdictRecordIds = edictRecordIds;
        TravelTripIds = travelTripIds;
        LetterIds = letterIds;
        LanguageProficiencyIds = languageProficiencyIds;
        DivergenceRecordIds = divergenceRecordIds;
        DistantHoldingIds = distantHoldingIds;
        CharacterHealthConditionIds = characterHealthConditionIds;
        EpidemicOutbreakIds = epidemicOutbreakIds;
        DisasterEventIds = disasterEventIds;
        WandererIds = wandererIds;
        WandererEngagementIds = wandererEngagementIds;
        DistrictIds = districtIds;
        PropertyRecordIds = propertyRecordIds;
        SocietasIds = societasIds;
        NotableBusinessIds = notableBusinessIds;
        CartelAgreementIds = cartelAgreementIds;
        PublicContractIds = publicContractIds;
        ContractBidIds = contractBidIds;
        LustrumEventIds = lustrumEventIds;
        ContractFraudRecordIds = contractFraudRecordIds;
        PavedRoadConnectionIds = pavedRoadConnectionIds;
        PrivateBridgeIds = privateBridgeIds;
        MerchantShipIds = merchantShipIds;
        ShipCommissionProjectIds = shipCommissionProjectIds;
        VoyageEventIds = voyageEventIds;
        PublicWorkIds = publicWorkIds;
        CompetitiveEuergetismEventIds = competitiveEuergetismEventIds;
        Regions = regions;
        Settlements = settlements;
        Plots = plots;
        Holdings = holdings;
        Characters = characters;
        Relationships = relationships;
        ScheduledActions = scheduledActions;
        PopGroups = popGroups;
        HouseholdRegimenDefaults = householdRegimenDefaults;
        Buildings = buildings;
        Stockpiles = stockpiles;
        ConstructionSchedules = constructionSchedules;
        LedgerAccounts = ledgerAccounts;
        LedgerTransactions = ledgerTransactions;
        MarketPrices = marketPrices;
        HouseholdStatements = householdStatements;
        DebtRecords = debtRecords;
        NetWorthAssessments = netWorthAssessments;
        InsolvencyStates = insolvencyStates;
        StandingContracts = standingContracts;
        HouseholdPolicies = householdPolicies;
        EventInstances = eventInstances;
        Actors = actors;
        HouseStandings = houseStandings;
        RivalDossiers = rivalDossiers;
        RegionalFamiliesEntries = regionalFamiliesEntries;
        StewardshipAssignments = stewardshipAssignments;
        AutonomousDecisionLogs = autonomousDecisionLogs;
        Schemes = schemes;
        ReturnReports = returnReports;
        HouseholdHeadships = householdHeadships;
        HeirDesignations = heirDesignations;
        SuccessionDisputes = successionDisputes;
        PlayerControls = playerControls;
        ChronicleEntries = chronicleEntries;
        GenerationalChapters = generationalChapters;
        FuneralRecords = funeralRecords;
        MourningPeriods = mourningPeriods;
        MemoriaStates = memoriaStates;
        Agnomens = agnomens;
        InheritedCognomenDecisions = inheritedCognomenDecisions;
        DynasticEpithets = dynasticEpithets;
        HouseholdReputations = householdReputations;
        FavorObligations = favorObligations;
        ClientelaEntries = clientelaEntries;
        HouseholdInfluences = householdInfluences;
        CharacterFactionAlignments = characterFactionAlignments;
        MagistracyRecords = magistracyRecords;
        HouseholdReligions = householdReligions;
        OmenEvents = omenEvents;
        PriesthoodRecords = priesthoodRecords;
        LegalCases = legalCases;
        PunishableOffenses = punishableOffenses;
        DetentionRecords = detentionRecords;
        SentenceRecords = sentenceRecords;
        RansomNegotiations = ransomNegotiations;
        Collegia = collegia;
        ScandalRecords = scandalRecords;
        CharacterFames = characterFames;
        HouseholdDoctrines = householdDoctrines;
        EdictRecords = edictRecords;
        TravelTrips = travelTrips;
        Letters = letters;
        LanguageProficiencies = languageProficiencies;
        LiteracyRecords = literacyRecords;
        InterpresAppointments = interpresAppointments;
        DivergenceRecords = divergenceRecords;
        DistantHoldings = distantHoldings;
        CharacterHealthConditions = characterHealthConditions;
        SettlementSanitationInvestments = settlementSanitationInvestments;
        EpidemicOutbreaks = epidemicOutbreaks;
        DisasterEvents = disasterEvents;
        DormantVolcanoes = dormantVolcanoes;
        Wanderers = wanderers;
        WandererEngagements = wandererEngagements;
        FiredHistoricalTimelineEntryIds = firedHistoricalTimelineEntryIds;
        Districts = districts;
        PropertyRecords = propertyRecords;
        PlotPropertyExtensions = plotPropertyExtensions;
        Societates = societates;
        ActioProSocioLinks = actioProSocioLinks;
        MerchantHouseArchetypes = merchantHouseArchetypes;
        SenateEntryInvestmentLogs = senateEntryInvestmentLogs;
        NotableBusinesses = notableBusinesses;
        NotableBusinessRivalryLogs = notableBusinessRivalryLogs;
        NotableBusinessGovernmentContracts = notableBusinessGovernmentContracts;
        CompetitiveEscalations = competitiveEscalations;
        CartelAgreements = cartelAgreements;
        GrainHoardingRecords = grainHoardingRecords;
        MarketCapacityReadings = marketCapacityReadings;
        PublicContracts = publicContracts;
        ContractBids = contractBids;
        LustrumEvents = lustrumEvents;
        PublicContractFraudRecords = publicContractFraudRecords;
        ContractFraudLegalLinks = contractFraudLegalLinks;
        PavedRoadConnections = pavedRoadConnections;
        IrrigationCanals = irrigationCanals;
        WellOrCisterns = wellOrCisterns;
        LandReclamationProjects = landReclamationProjects;
        PrivateBridges = privateBridges;
        BoundaryInfrastructures = boundaryInfrastructures;
        InfrastructureConditions = infrastructureConditions;
        UnifiedEstateMilestones = unifiedEstateMilestones;
        MerchantShips = merchantShips;
        ShipCommissionProjects = shipCommissionProjects;
        ShipFrontingArrangements = shipFrontingArrangements;
        VoyageEvents = voyageEvents;
        FlagshipDesignationAwards = flagshipDesignationAwards;
        PublicWorks = publicWorks;
        EuergetismObligations = euergetismObligations;
        CompetitiveEuergetismEvents = competitiveEuergetismEvents;
        Knowledge = knowledge;
        _nextCommandSequenceNumber = nextCommandSequenceNumber;
    }

    public RuntimeIdCounter<Region> RegionIds { get; } = new();
    public RuntimeIdCounter<Settlement> SettlementIds { get; } = new();
    public RuntimeIdCounter<Plot> PlotIds { get; } = new();
    public RuntimeIdCounter<Holding> HoldingIds { get; } = new();
    public RuntimeIdCounter<Household> HouseholdIds { get; } = new();
    public RuntimeIdCounter<Actor> ActorIds { get; } = new();
    public RuntimeIdCounter<Character> CharacterIds { get; } = new();
    public RuntimeIdCounter<Building> BuildingIds { get; } = new();
    public RuntimeIdCounter<Contract> ContractIds { get; } = new();
    public RuntimeIdCounter<Activity> ActivityIds { get; } = new();
    public RuntimeIdCounter<Command> CommandIds { get; } = new();
    public RuntimeIdCounter<DomainEventEntity> EventIds { get; } = new();
    public RuntimeIdCounter<ScheduledAction> ScheduledActionIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Ledger.LedgerTransaction"/> (Phase 8 item 1).</summary>
    public RuntimeIdCounter<LedgerTransaction> LedgerTransactionIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Economy.DebtRecord"/> (Phase 8 item 6).</summary>
    public RuntimeIdCounter<DebtRecord> DebtRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Economy.StandingContract"/> (Phase 8 item 7).</summary>
    public RuntimeIdCounter<StandingContract> StandingContractIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Events.EventInstance"/> (Phase 9 item 3).</summary>
    public RuntimeIdCounter<EventInstance> EventInstanceIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Stewardship.StewardshipAssignment"/> (Phase 10 item 2).</summary>
    public RuntimeIdCounter<StewardshipAssignment> StewardshipAssignmentIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Stewardship.AutonomousDecisionLog"/> (Phase 10 item 10).</summary>
    public RuntimeIdCounter<AutonomousDecisionLog> AutonomousDecisionLogIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Interactions.Scheme"/> (Phase 10 item 6).</summary>
    public RuntimeIdCounter<Scheme> SchemeIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Stewardship.ReturnReport"/> (Phase 10 package 13).</summary>
    public RuntimeIdCounter<ReturnReport> ReturnReportIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Succession.SuccessionDispute"/> (Phase 11 item 1).</summary>
    public RuntimeIdCounter<SuccessionDispute> SuccessionDisputeIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Chronicle.ChronicleEntry"/> (Phase 11 item 3).</summary>
    public RuntimeIdCounter<ChronicleEntry> ChronicleEntryIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Funerary.FuneralRecord"/> (Phase 11 item 4).</summary>
    public RuntimeIdCounter<FuneralRecord> FuneralRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Epithets.Agnomen"/> (Phase 11 item 5).</summary>
    public RuntimeIdCounter<Agnomen> AgnomenIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Epithets.InheritedCognomenDecision"/> (Phase 11 item 5).</summary>
    public RuntimeIdCounter<InheritedCognomenDecision> InheritedCognomenDecisionIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Reputation.FavorObligation"/> (Phase 12 item 1).</summary>
    public RuntimeIdCounter<FavorObligation> FavorObligationIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Magistracies.MagistracyRecord"/> (Phase 12 item 2).</summary>
    public RuntimeIdCounter<MagistracyRecord> MagistracyRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Religion.OmenEvent"/> (Phase 12 item 3).</summary>
    public RuntimeIdCounter<OmenEvent> OmenEventIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Religion.PriesthoodRecord"/> (Phase 12 item 3).</summary>
    public RuntimeIdCounter<PriesthoodRecord> PriesthoodRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Legal.LegalCase"/> (Phase 12 item 4).</summary>
    public RuntimeIdCounter<LegalCase> LegalCaseIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Crime.PunishableOffense"/> (Phase 12 item 5).</summary>
    public RuntimeIdCounter<PunishableOffense> PunishableOffenseIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Crime.DetentionRecord"/> (Phase 12 item 5).</summary>
    public RuntimeIdCounter<DetentionRecord> DetentionRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Crime.SentenceRecord"/> (Phase 12 item 5).</summary>
    public RuntimeIdCounter<SentenceRecord> SentenceRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Crime.RansomNegotiation"/> (Phase 12 item 5).</summary>
    public RuntimeIdCounter<RansomNegotiation> RansomNegotiationIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Scandal.ScandalRecord"/> (Phase 12 item 7).</summary>
    public RuntimeIdCounter<ScandalRecord> ScandalRecordIds { get; } = new();
    public RuntimeIdCounter<EdictRecord> EdictRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Travel.TravelTrip"/> (Phase 13 item 2).</summary>
    public RuntimeIdCounter<TravelTrip> TravelTripIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Correspondence.Letter"/> (Phase 13 item 3).</summary>
    public RuntimeIdCounter<Letter> LetterIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Languages.LanguageProficiency"/> (Phase 13 item 4).</summary>
    public RuntimeIdCounter<LanguageProficiency> LanguageProficiencyIds { get; } = new();

    /// <summary>Issues IDs for <see cref="History.DivergenceRecord"/> (Phase 13 item 5).</summary>
    public RuntimeIdCounter<DivergenceRecord> DivergenceRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Land.DistantHolding"/> (Phase 13 item 7).</summary>
    public RuntimeIdCounter<DistantHolding> DistantHoldingIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Health.CharacterHealthCondition"/> (Phase 14 item 1).</summary>
    public RuntimeIdCounter<CharacterHealthCondition> CharacterHealthConditionIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Health.EpidemicOutbreak"/> (Phase 14 item 2).</summary>
    public RuntimeIdCounter<EpidemicOutbreak> EpidemicOutbreakIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Hazards.DisasterEvent"/> (Phase 14 item 3).</summary>
    public RuntimeIdCounter<DisasterEvent> DisasterEventIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.Wanderers.Wanderer"/> (Phase 14 item 4).</summary>
    public RuntimeIdCounter<Wanderer> WandererIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.Wanderers.WandererEngagement"/> (Phase 14 item 4).</summary>
    public RuntimeIdCounter<WandererEngagement> WandererEngagementIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.RealEstate.District"/> (Phase 15 item 1).</summary>
    public RuntimeIdCounter<District> DistrictIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.RealEstate.PropertyRecord"/> (Phase 15 item 1).</summary>
    public RuntimeIdCounter<PropertyRecord> PropertyRecordIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.Societates.Societas"/> (Phase 15 item 2).</summary>
    public RuntimeIdCounter<Societas> SocietasIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.NotableBusinesses.NotableBusiness"/> (Phase 15
    /// item 4).</summary>
    public RuntimeIdCounter<NotableBusiness> NotableBusinessIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.BusinessCompetition.CartelAgreement"/> (Phase 15
    /// item 5) — the one new entity kind this item needs a fresh counter for; see that record's own doc
    /// comment for why <see cref="CompetitiveEscalation"/> and <see cref="GrainHoardingRecord"/> need
    /// none.</summary>
    public RuntimeIdCounter<CartelAgreement> CartelAgreementIds { get; } = new();

    /// <summary>Phase 15 item 6's four new runtime-entity kinds — <see
    /// cref="PublicContract"/>, <see cref="ContractBid"/>, <see cref="LustrumEvent"/>, and <see
    /// cref="ContractFraudRecord"/> — each needing a real identity of its own (matching <see
    /// cref="CartelAgreementIds"/>'s identical "genuinely its own entity" precedent).</summary>
    public RuntimeIdCounter<PublicContract> PublicContractIds { get; } = new();
    public RuntimeIdCounter<ContractBid> ContractBidIds { get; } = new();
    public RuntimeIdCounter<LustrumEvent> LustrumEventIds { get; } = new();
    public RuntimeIdCounter<ContractFraudRecord> ContractFraudRecordIds { get; } = new();

    /// <summary>Phase 15 item 7's two new runtime-entity kinds needing a real identity of their own —
    /// <see cref="PavedRoadConnection"/> and <see cref="PrivateBridge"/> — matching <see
    /// cref="PublicContractIds"/>'s identical "genuinely its own entity" precedent. Every other new
    /// structure this item adds (<see cref="IrrigationCanal"/>, <see cref="WellOrCistern"/>, <see
    /// cref="LandReclamationProject"/>, <see cref="BoundaryInfrastructure"/>) is instead keyed by the
    /// already-issued <see cref="RuntimeId{Plot}"/> it was built on, needing no counter of its own.</summary>
    public RuntimeIdCounter<PavedRoadConnection> PavedRoadConnectionIds { get; } = new();
    public RuntimeIdCounter<PrivateBridge> PrivateBridgeIds { get; } = new();
    public RuntimeIdCounter<MerchantShip> MerchantShipIds { get; } = new();
    public RuntimeIdCounter<ShipCommissionProject> ShipCommissionProjectIds { get; } = new();
    public RuntimeIdCounter<VoyageEvent> VoyageEventIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.PublicWorks.PublicWork"/> (Phase 15 item 9).</summary>
    public RuntimeIdCounter<PublicWork> PublicWorkIds { get; } = new();

    /// <summary>Issues IDs for <see cref="Gens.Simulation.PublicWorks.CompetitiveEuergetismEvent"/> (Phase
    /// 15 item 9) — the one new entity kind this item needs a fresh counter for, matching <see
    /// cref="BusinessCompetition.CartelAgreement"/>'s own identical "genuinely its own entity" precedent;
    /// <see cref="Gens.Simulation.PublicWorks.EuergetismObligation"/> needs none, keyed by the
    /// already-registered <see cref="RuntimeId{Household}"/> it describes instead.</summary>
    public RuntimeIdCounter<CompetitiveEuergetismEvent> CompetitiveEuergetismEventIds { get; } = new();

    /// <summary>Every Region (Phase 6 item 1), in ascending-<see cref="RuntimeId{T}"/> order
    /// (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<Region>, Region> Regions { get; } = new();

    /// <summary>Every Settlement (Phase 6 item 1), in ascending-<see cref="RuntimeId{T}"/> order
    /// (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<Settlement>, Settlement> Settlements { get; } = new();

    /// <summary>Every Plot (Phase 6 item 1), in ascending-<see cref="RuntimeId{T}"/> order
    /// (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<Plot>, Plot> Plots { get; } = new();

    /// <summary>Every Holding (Phase 6 item 1), in ascending-<see cref="RuntimeId{T}"/> order
    /// (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<Holding>, Holding> Holdings { get; } = new();

    /// <summary>Every named Character (Phase 5 item 1), in ascending-<see cref="RuntimeId{T}"/> order
    /// (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<Character>, Character> Characters { get; } = new();

    /// <summary>Every directed dyadic tie in the relationship web (Phase 5 item 5;
    /// <c>gens-characters-design.md</c> §7), in ascending <see cref="RelationshipKey"/> order (ADR
    /// 0004) — source Character first, target second. Sparse by construction: a pair with no recorded
    /// interaction simply has no entry here at all, rather than every possible pair pre-allocating a
    /// zero-opinion slot.</summary>
    public OrderedRegistry<RelationshipKey, Relationship> Relationships { get; } = new();

    /// <summary>The calendar queue (Phase 4 item 4): future-dated work not yet due. Ordered by
    /// (due date, action ID) so draining it is a deterministic ascending scan (ADR 0004). Systems and
    /// commands add/remove entries directly through this partition's own API, matching how
    /// <see cref="Characters"/> and <see cref="Knowledge"/> are mutated.</summary>
    public OrderedRegistry<ScheduledActionKey, ScheduledActionEntry> ScheduledActions { get; } = new();

    /// <summary>Every background population group — ADR 0009's <c>Background</c> fidelity tier (Phase
    /// 5 item 7) — in ascending (settlement, group type) order (ADR 0004).
    /// <c>PromoteToNamedCommand</c> is the only thing that mutates an entry here: it decrements a
    /// group's size by exactly one and adds the corresponding entry to <see cref="Characters"/>.</summary>
    public OrderedRegistry<PopGroupKey, PopGroup> PopGroups { get; } = new();

    /// <summary>Every household-level Regimen default (Phase 6 item 6; <c>gens-labor-slavery-design.md</c>
    /// §5), keyed by (household, duty slot or <c>null</c> for the whole-household fallback). Sparse: a
    /// household with no group default set simply has no entry — <see cref="RegimenResolver"/> falls
    /// back to <see cref="RegimenCatalog.Default"/>.</summary>
    public OrderedRegistry<HouseholdRegimenKey, RegimenSettings> HouseholdRegimenDefaults { get; } = new();

    /// <summary>Every completed Building (Phase 6 item 4/7), in ascending-<see cref="RuntimeId{T}"/>
    /// order (ADR 0004), regardless of which Plot or Holding it stands on. <see
    /// cref="BuildingInstance"/> is a mutable class (not an immutable record like <see
    /// cref="Character"/>) — systems mutate an entry in place (staffing, condition) rather than
    /// removing and re-adding it; only construction completion structurally adds a new entry here.</summary>
    public OrderedRegistry<RuntimeId<Building>, BuildingInstance> Buildings { get; } = new();

    /// <summary>One capacity-bounded <see cref="Stockpile"/> per Holding (Phase 6 items 3/7) — "one
    /// estate transforms inputs... into deterministic outputs" (Phase 6 exit gate) reads as one
    /// storage pool per estate, matching <c>gens-resources-goods-design.md</c> §8's storage
    /// accumulating at the estate/settlement level rather than per individual building. Sparse: a
    /// Holding with no stockpile provisioned yet simply has no entry, and buildings on its plots
    /// cannot produce or consume until one exists.</summary>
    public OrderedRegistry<RuntimeId<Holding>, Stockpile> Stockpiles { get; } = new();

    /// <summary>One <see cref="ConstructionSchedule"/> per Holding (Phase 6 item 7's "one construction
    /// queue") — a single FIFO per estate that <see cref="ConstructionSchedule.Enqueue"/> can target any
    /// of that Holding's plots, matching <see cref="ConstructionSchedule"/>'s own per-call <c>Plot</c>
    /// parameter. <see cref="ConstructionSchedule"/> is a mutable class, mutated in place like <see
    /// cref="Buildings"/> above.</summary>
    public OrderedRegistry<RuntimeId<Holding>, ConstructionSchedule> ConstructionSchedules { get; } = new();

    /// <summary>Every household, actor, settlement-treasury, or system <see cref="LedgerAccount"/>
    /// balance (Phase 8 items 1-2), in ascending <see cref="LedgerAccountKey"/> order (ADR 0004).
    /// Sparse: an account with no posting yet simply has no entry — <see cref="LedgerService.Post"/>
    /// creates one at a zero balance on first use, matching <see cref="Stockpiles"/>'s identical
    /// sparse-provisioning convention.</summary>
    public OrderedRegistry<LedgerAccountKey, LedgerAccount> LedgerAccounts { get; } = new();

    /// <summary>Every posted <see cref="LedgerTransaction"/> (Phase 8 item 1), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004) — the append-only double-entry-style audit log <see
    /// cref="LedgerAccounts"/>' balances are folded from.</summary>
    public OrderedRegistry<RuntimeId<LedgerTransaction>, LedgerTransaction> LedgerTransactions { get; } = new();

    /// <summary>One cleared <see cref="SettlementMarket"/> per (settlement, good) (Phase 8 items 3-4),
    /// in ascending <see cref="MarketGoodKey"/> order (ADR 0004). Sparse: a (settlement, good) pair
    /// that has never cleared simply has no entry yet.</summary>
    public OrderedRegistry<MarketGoodKey, SettlementMarket> MarketPrices { get; } = new();

    /// <summary>Each household's latest monthly income/expense/net summary (Phase 8 item 5), keyed by
    /// household, ascending <see cref="RuntimeId{T}"/> order (ADR 0004). Sparse and overwritten each
    /// month — see <see cref="Economy.HouseholdMonthlyStatement"/>'s own doc comment for why this is a
    /// latest-snapshot read model, not an accumulating history.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdMonthlyStatement> HouseholdStatements { get; } = new();

    /// <summary>Every standing loan (Phase 8 item 6; <c>gens-economy-finance-design.md</c> §6.1), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — never removed once opened, even once
    /// resolved, matching <see cref="LedgerTransactions"/>' append-only-audit-log convention.</summary>
    public OrderedRegistry<RuntimeId<DebtRecord>, DebtRecord> DebtRecords { get; } = new();

    /// <summary>Each household's latest Net Worth assessment (Phase 8 item 6; §8), keyed by household.
    /// Sparse and overwritten each month, matching <see cref="HouseholdStatements"/>' identical
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, NetWorth> NetWorthAssessments { get; } = new();

    /// <summary>Each household's Insolvency ladder position (Phase 8 item 6; §9), keyed by household.
    /// Sparse and overwritten each month; unlike <see cref="NetWorthAssessments"/>, its own <see
    /// cref="Economy.InsolvencyState.ConsequencesApplied"/> field is itself cumulative across months —
    /// see that record's own doc comment.</summary>
    public OrderedRegistry<RuntimeId<Household>, InsolvencyState> InsolvencyStates { get; } = new();

    /// <summary>Every standing market contract and trade-route commitment (Phase 8 item 7), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<StandingContract>, StandingContract> StandingContracts { get; } = new();

    /// <summary>Each household's current Standing Policy configuration (Phase 9 item 2;
    /// <c>gens-policies-edicts-design.md</c> §2), keyed by household. Sparse and overwritten on each
    /// accepted <see cref="Policies.ChangeRitesBudgetCommand"/>, matching <see
    /// cref="HouseholdRegimenDefaults"/>'s identical "no entry means the catalog default" convention —
    /// see <see cref="Policies.HouseholdPolicyResolver"/>.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdPolicyState> HouseholdPolicies { get; } = new();

    /// <summary>Every fired <see cref="Events.EventInstance"/> (Phase 9 item 3), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004). <see cref="Events.EventInstance"/> is an immutable
    /// record, so a stage advancement, resolution, or expiry replaces the entry (remove then re-add
    /// under the same <see cref="Events.EventInstance.InstanceId"/>) rather than mutating it in place —
    /// matching <see cref="HouseholdPolicies"/>' identical convention. Entries are kept, resolved or
    /// not, for the campaign's lifetime: a resolved instance's own <see
    /// cref="Events.EventInstance.ResolvedOptionId"/>/<see cref="Events.EventInstance.ResolvingEventId"/>
    /// fields are exactly what the Monthly Report's drill-down (Phase 9 item 4) reads back.</summary>
    public OrderedRegistry<RuntimeId<EventInstance>, EventInstance> EventInstances { get; } = new();

    /// <summary>Every <see cref="LivingWorldActor"/> — rival houses and, later, the other actor kinds
    /// <c>gens-rival-houses-design.md</c> §6 generalizes to (Phase 10 item 3) — in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004). Immutable record entries: a system replaces an entry
    /// rather than mutating one in place, matching <see cref="EventInstances"/>' identical convention.</summary>
    public OrderedRegistry<RuntimeId<Actor>, LivingWorldActor> Actors { get; } = new();

    /// <summary>Every tracked house-pair's <see cref="Actors.HouseStanding"/> (Phase 10 item 5), in
    /// ascending <see cref="HouseStandingKey"/> order (ADR 0004). Sparse: an untracked pair has no
    /// entry — see <see cref="Actors.HouseStandingResolver"/> for the default that applies then.</summary>
    public OrderedRegistry<HouseStandingKey, HouseStanding> HouseStandings { get; } = new();

    /// <summary>Every actor the player has an actual <see cref="Actors.RivalDossier"/> for (Phase 10
    /// item 5), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Sparse: an actor never
    /// contacted has no entry.</summary>
    public OrderedRegistry<RuntimeId<Actor>, RivalDossier> RivalDossiers { get; } = new();

    /// <summary>Every actor with lighter, pre-contact regional visibility (Phase 10 item 5;
    /// <c>gens-rival-houses-design.md</c> §7's "Notable Families of the Region"), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004). Sparse, and distinct from <see cref="RivalDossiers"/>:
    /// an actor can appear here without ever having a full dossier.</summary>
    public OrderedRegistry<RuntimeId<Actor>, RegionalFamiliesEntry> RegionalFamiliesEntries { get; } = new();

    /// <summary>Every household's delegated-management assignment, past and present (Phase 10 item 2),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept even once ended (<see
    /// cref="StewardshipAssignment.EndDate"/> set) rather than removed, matching <see
    /// cref="EventInstances"/>' identical "resolved or not, kept for the campaign's lifetime"
    /// convention — a later Return Report still needs to read the ended assignment back.</summary>
    public OrderedRegistry<RuntimeId<StewardshipAssignment>, StewardshipAssignment> StewardshipAssignments { get; } = new();

    /// <summary>Every autonomous decision any steward/Council has ever logged (Phase 10 item 10), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — an append-only audit log, matching <see
    /// cref="Ledger.LedgerTransactions"/>' identical convention. A future Return Report (package 11)
    /// reads the entries for one <see cref="Stewardship.StewardshipAssignment"/> back out of here.</summary>
    public OrderedRegistry<RuntimeId<AutonomousDecisionLog>, AutonomousDecisionLog> AutonomousDecisionLogs { get; } = new();

    /// <summary>Every <see cref="Interactions.Scheme"/>, in-progress or resolved (Phase 10 item 6), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept once resolved rather than removed,
    /// matching <see cref="EventInstances"/>' identical "resolved or not, kept for the campaign's
    /// lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<Scheme>, Scheme> Schemes { get; } = new();

    /// <summary>Every <see cref="Stewardship.ReturnReport"/> ever produced when a <see
    /// cref="StewardshipAssignment"/> ended (Phase 10 package 13; design doc §10), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004) — one per ended assignment, kept for the campaign's
    /// lifetime like <see cref="AutonomousDecisionLogs"/>.</summary>
    public OrderedRegistry<RuntimeId<ReturnReport>, ReturnReport> ReturnReports { get; } = new();

    /// <summary>Which Character currently heads each tracked Household (Phase 11 item 1), keyed by
    /// household. Sparse: a Household with no explicitly established head has no entry — see <see
    /// cref="Succession.EstablishHouseholdHeadCommand"/>. Immutable record entries: <see
    /// cref="Succession.SuccessionHandoffSystem"/> replaces an entry (remove then re-add) rather than
    /// mutating one in place, matching <see cref="EventInstances"/>' identical convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdHeadship> HouseholdHeadships { get; } = new();

    /// <summary>Each Household's succession bookkeeping — preference, Formal Declaration, disownments,
    /// adoptions, and acknowledged Illegitimate children (Phase 11 item 1; §2-§4) — keyed by household.
    /// Sparse: a Household with none of these ever recorded has no entry, matching <see
    /// cref="HouseholdPolicies"/>' identical "no entry means the default" convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, HeirDesignation> HeirDesignations { get; } = new();

    /// <summary>Every contested succession, pending or resolved (Phase 11 item 1; §5.2), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept once resolved rather than removed,
    /// matching <see cref="Schemes"/>' identical "resolved or not, kept for the campaign's lifetime"
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<SuccessionDispute>, SuccessionDispute> SuccessionDisputes { get; } = new();

    /// <summary>Which Character the player currently controls, and how (Phase 11 item 2; §6.2), keyed
    /// by household. Sparse: at most one entry across a whole campaign today (one player household),
    /// with no entry until <see cref="Succession.EstablishPlayerControlCommand"/> establishes one — see
    /// <see cref="Succession.PlayerControlState"/>'s own doc comment for why this is still a registry
    /// rather than a bespoke singleton field. Immutable record entries: <see
    /// cref="Succession.PlayerControlHandoffSystem"/> replaces an entry (remove then re-add) rather
    /// than mutating one in place, matching <see cref="HouseholdHeadships"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, PlayerControlState> PlayerControls { get; } = new();

    /// <summary>Every <see cref="Chronicle.ChronicleEntry"/> ever recorded, across every household
    /// (Phase 11 item 3), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — <see
    /// cref="Queries.ChronicleQuery"/> is what narrows this to one household's own read. Kept once
    /// recorded rather than removed, matching <see cref="EventInstances"/>' identical "kept for the
    /// campaign's lifetime" convention; a pin or annotation replaces an entry (remove then re-add
    /// under the same <see cref="Chronicle.ChronicleEntry.EntryId"/>) rather than mutating one in
    /// place.</summary>
    public OrderedRegistry<RuntimeId<ChronicleEntry>, ChronicleEntry> ChronicleEntries { get; } = new();

    /// <summary>Every generational chapter, past and present, across every household (Phase 11 item
    /// 3; §4), in ascending <see cref="GenerationalChapterKey"/> (household, then start month) order
    /// (ADR 0004). Kept once closed rather than removed, matching <see cref="ChronicleEntries"/>'
    /// identical convention.</summary>
    public OrderedRegistry<GenerationalChapterKey, GenerationalChapter> GenerationalChapters { get; } = new();

    /// <summary>Every <see cref="Funerary.FuneralRecord"/>, pending or held (Phase 11 item 4), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept once held rather than removed,
    /// matching <see cref="SuccessionDisputes"/>' identical "resolved or not, kept for the campaign's
    /// lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<FuneralRecord>, FuneralRecord> FuneralRecords { get; } = new();

    /// <summary>Each household's current <see cref="Funerary.MourningPeriod"/> (Phase 11 item 4;
    /// §4.1), keyed by household. Sparse: a household never touched by a death has no entry, matching
    /// <see cref="HouseholdPolicies"/>' identical convention. Immutable record entries: <see
    /// cref="Funerary.FuneralOpeningSystem"/> and <see cref="Funerary.BreakMourningEarlyCommand"/>
    /// replace an entry (remove then re-add) rather than mutating one in place, matching <see
    /// cref="HouseholdHeadships"/>' identical convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, MourningPeriod> MourningPeriods { get; } = new();

    /// <summary>Each household's running Memoria total (Phase 11 item 4; §6's "third axis"), keyed by
    /// household. Sparse: a household this item never touches has no entry, matching <see
    /// cref="HouseholdPolicies"/>' identical convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, MemoriaState> MemoriaStates { get; } = new();

    /// <summary>Every <see cref="Epithets.Agnomen"/> ever granted (Phase 11 item 5), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once granted, matching <see
    /// cref="SuccessionDisputes"/>' identical "resolved or not, kept for the campaign's lifetime"
    /// convention — this item never revokes one.</summary>
    public OrderedRegistry<RuntimeId<Agnomen>, Agnomen> Agnomens { get; } = new();

    /// <summary>Every <see cref="Epithets.InheritedCognomenDecision"/> ever recorded (Phase 11 item 5;
    /// §5), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once recorded,
    /// matching <see cref="Agnomens"/>' identical convention.</summary>
    public OrderedRegistry<RuntimeId<InheritedCognomenDecision>, InheritedCognomenDecision> InheritedCognomenDecisions { get; } = new();

    /// <summary>Each household's current <see cref="Epithets.DynasticEpithet"/> (Phase 11 item 5; §6),
    /// keyed by household. Sparse: a household that hasn't crossed <see
    /// cref="Epithets.DynasticEpithetCatalog.MinimumMajorOrLegendaryEntries"/> yet has no entry,
    /// matching <see cref="HouseholdPolicies"/>' identical "no entry means the default" convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, DynasticEpithet> DynasticEpithets { get; } = new();

    /// <summary>Each household's running <see cref="Reputation.HouseholdReputation"/> Dignitas total
    /// (Phase 12 item 1; <c>gens-politics-patronage-design.md</c> §2), keyed by household. Sparse: a
    /// household this item never touches has no entry, matching <see cref="MemoriaStates"/>'s identical
    /// "no entry means the default" convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdReputation> HouseholdReputations { get; } = new();

    /// <summary>Every <see cref="Reputation.FavorObligation"/> ever granted, resolved or not (Phase 12
    /// item 1), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept once resolved rather than
    /// removed, matching <see cref="SuccessionDisputes"/>'s identical "resolved or not, kept for the
    /// campaign's lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<FavorObligation>, FavorObligation> FavorObligations { get; } = new();

    /// <summary>Every Character's Clientela roster membership (Phase 12 item 2; §4.1), keyed by client.
    /// Sparse: a Character who is nobody's client has no entry, matching <see
    /// cref="HouseholdReputations"/>'s identical "no entry means the default" convention. Immutable
    /// record entries: <see cref="Clientela.CallInClientFavorCommand"/> and <see
    /// cref="Clientela.ClientPoachingSystem"/> replace an entry (remove then re-add) rather than
    /// mutating one in place, matching <see cref="HouseholdHeadships"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<Character>, ClientelaEntry> ClientelaEntries { get; } = new();

    /// <summary>Each household's running Influence total (Phase 12 item 2; §4.4), keyed by household.
    /// Sparse: a household this item never touches has no entry, matching <see
    /// cref="HouseholdReputations"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdInfluence> HouseholdInfluences { get; } = new();

    /// <summary>Each Character's <see cref="Clientela.PoliticalFaction"/> (Phase 12 item 2; §3.1), keyed
    /// by Character. Sparse: a Character not relevant to local politics has no entry.</summary>
    public OrderedRegistry<RuntimeId<Character>, CharacterFactionAlignment> CharacterFactionAlignments { get; } = new();

    /// <summary>Every <see cref="Magistracies.MagistracyRecord"/> ever created, active or ended (Phase
    /// 12 item 2; §5), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once
    /// created rather than removed, matching <see cref="SuccessionDisputes"/>'s identical "resolved or
    /// not, kept for the campaign's lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<MagistracyRecord>, MagistracyRecord> MagistracyRecords { get; } = new();

    /// <summary>Each household's chosen Patron Deity and running Favor total (Phase 12 item 3; <see
    /// cref="Religion.HouseholdReligion"/>), keyed by household. Sparse: a household that has never
    /// issued <see cref="Religion.SetPatronDeityCommand"/> has no entry, matching <see
    /// cref="HouseholdReputations"/>'s identical "no entry means the default" convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, HouseholdReligion> HouseholdReligions { get; } = new();

    /// <summary>Every <see cref="Religion.OmenEvent"/> ever raised, resolved or not (Phase 12 item 3),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept once resolved rather than removed,
    /// matching <see cref="FavorObligations"/>'s identical "resolved or not, kept for the campaign's
    /// lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<OmenEvent>, OmenEvent> OmenEvents { get; } = new();

    /// <summary>Every <see cref="Religion.PriesthoodRecord"/> ever created, active or ended (Phase 12
    /// item 3), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once created,
    /// matching <see cref="MagistracyRecords"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<PriesthoodRecord>, PriesthoodRecord> PriesthoodRecords { get; } = new();

    /// <summary>Every <see cref="Legal.LegalCase"/> ever filed, ruled or not (Phase 12 item 4; §11's own
    /// data model), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once filed,
    /// matching <see cref="MagistracyRecords"/>'s identical "kept for the campaign's lifetime"
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<LegalCase>, LegalCase> LegalCases { get; } = new();

    /// <summary>Every <see cref="Crime.PunishableOffense"/> ever recorded (Phase 12 item 5; §3), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once recorded, matching <see
    /// cref="LegalCases"/>'s identical "kept for the campaign's lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<PunishableOffense>, PunishableOffense> PunishableOffenses { get; } = new();

    /// <summary>Every <see cref="Crime.DetentionRecord"/> ever opened, active or ended (Phase 12 item
    /// 5; §5), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once opened,
    /// matching <see cref="Magistracies.MagistracyRecord"/>'s identical "ended records are never
    /// removed, only replaced" convention.</summary>
    public OrderedRegistry<RuntimeId<DetentionRecord>, DetentionRecord> DetentionRecords { get; } = new();

    /// <summary>Every <see cref="Crime.SentenceRecord"/> ever applied (Phase 12 item 5; §7-§8), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once applied, matching <see
    /// cref="PunishableOffenses"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<SentenceRecord>, SentenceRecord> SentenceRecords { get; } = new();

    /// <summary>Every <see cref="Crime.RansomNegotiation"/> ever opened, resolved or not (Phase 12 item
    /// 5; §10), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once opened,
    /// matching <see cref="PunishableOffenses"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<RansomNegotiation>, RansomNegotiation> RansomNegotiations { get; } = new();

    /// <summary>Every <see cref="Collegia.CollegiumDetails"/> layered on top of a <see
    /// cref="LivingWorldActor"/> of <see cref="LivingWorldActorType.Collegium"/> (Phase 12 item 6),
    /// keyed by that same Actor — no separate ID counter, per that record's own doc comment. Sparse: an
    /// Actor of any other <see cref="LivingWorldActorType"/> has no entry here at all. Kept forever once
    /// founded and removed only alongside its own <see cref="Actors"/> entry on a real, terminal <see
    /// cref="Collegia.DissolveCollegiumCommand"/>, matching <see
    /// cref="Actors.LivingWorldActorExtinctionSystem"/>'s identical "removed outright, not frozen"
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<Actor>, CollegiumDetails> Collegia { get; } = new();

    /// <summary>Every <see cref="Scandal.ScandalRecord"/> ever recorded (Phase 12 item 7), in ascending
    /// <see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once recorded, matching <see
    /// cref="PunishableOffenses"/>'s and <see cref="LegalCases"/>'s identical "kept for the campaign's
    /// lifetime" convention — see <see cref="Scandal.ScandalRecord"/>'s own doc comment for why.</summary>
    public OrderedRegistry<RuntimeId<ScandalRecord>, ScandalRecord> ScandalRecords { get; } = new();

    /// <summary>Each Character's running <see cref="Fame.CharacterFame"/> score (Phase 12 item 8;
    /// <c>gens-celebrities-influential-figures-design.md</c> §1), keyed by Character. Sparse: a
    /// Character this item never touches has no entry, matching <see cref="HouseholdReputations"/>'s
    /// identical "no entry means the default" convention — the one deliberate divergence being that
    /// this partition is keyed by Character, not Household (see <see cref="Fame.CharacterFame"/>'s own
    /// doc comment for why).</summary>
    public OrderedRegistry<RuntimeId<Character>, CharacterFame> CharacterFames { get; } = new();

    /// <summary>Every household's standing against every <see cref="HouseholdDoctrineType"/> it has an
    /// entry for (Phase 12 item 9), keyed by (household, Doctrine type). Sparse: a (household, Doctrine)
    /// pair <see cref="Doctrine.DoctrineResolutionSystem"/> has never touched has no entry, matching
    /// <see cref="HouseholdReputations"/>'s identical "no entry means the default" convention.</summary>
    public OrderedRegistry<HouseholdDoctrineKey, HouseholdDoctrineState> HouseholdDoctrines { get; } = new();

    /// <summary>Every Edict ever issued (Phase 12 item 9), in ascending <see cref="RuntimeId{T}"/> order
    /// (ADR 0004). Kept forever once issued, matching <see cref="ScandalRecords"/>'s and <see
    /// cref="LegalCases"/>'s identical "kept for the campaign's lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<EdictRecord>, EdictRecord> EdictRecords { get; } = new();

    /// <summary>Every <see cref="Travel.TravelTrip"/> ever begun (Phase 13 item 2), in ascending <see
    /// cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once begun, matching <see
    /// cref="EdictRecords"/>'s identical "kept for the campaign's lifetime" convention — a completed
    /// trip is a real part of a Character's own travel history, not scratch state to discard.</summary>
    public OrderedRegistry<RuntimeId<TravelTrip>, TravelTrip> TravelTrips { get; } = new();

    /// <summary>Every <see cref="Correspondence.Letter"/> ever sent or originated (Phase 13 item 3), in
    /// ascending <see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once begun, matching <see
    /// cref="TravelTrips"/>'s identical "kept for the campaign's lifetime" convention — a resolved
    /// letter is a real part of a household's own correspondence history, not scratch state to discard.</summary>
    public OrderedRegistry<RuntimeId<Letter>, Letter> Letters { get; } = new();

    /// <summary>Every named Character's <see cref="Languages.LanguageProficiency"/> (Phase 13 item 4),
    /// in ascending <see cref="RuntimeId{T}"/> order (ADR 0004). A Character legitimately owns several
    /// entries at once (§8's own "no artificial ceiling").</summary>
    public OrderedRegistry<RuntimeId<LanguageProficiency>, LanguageProficiency> LanguageProficiencies { get; } = new();

    /// <summary>Every named Character's tracked <see cref="Languages.LiteracyRecord"/> (Phase 13 item
    /// 4), keyed by the Character it describes — one per Character, matching <see
    /// cref="ClientelaEntries"/>'s identical "the owning entity is already a unique key" shape.</summary>
    public OrderedRegistry<RuntimeId<Character>, LiteracyRecord> LiteracyRecords { get; } = new();

    /// <summary>Every household's standing <see cref="Languages.InterpresAppointment"/> (Phase 13 item
    /// 4), keyed by the appointing household — at most one active appointment per household.</summary>
    public OrderedRegistry<RuntimeId<Household>, InterpresAppointment> InterpresAppointments { get; } = new();

    /// <summary>Every recorded <see cref="History.DivergenceRecord"/> (Phase 13 item 5; §6.7), in
    /// ascending <see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once recorded, matching <see
    /// cref="EdictRecords"/>'s identical "kept for the campaign's lifetime" convention — every Divergence
    /// is an automatic maximum-tier Dynasty Chronicle entry, not scratch state to discard.</summary>
    public OrderedRegistry<RuntimeId<DivergenceRecord>, DivergenceRecord> DivergenceRecords { get; } = new();

    /// <summary>Every <see cref="Land.DistantHolding"/> a household currently holds (Phase 13 item 7),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — kept for the campaign's lifetime,
    /// matching <see cref="TravelTrips"/>'s and <see cref="Letters"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<DistantHolding>, DistantHolding> DistantHoldings { get; } = new();

    /// <summary>Every standing <see cref="Health.CharacterHealthCondition"/> case, active or resolved
    /// (Phase 14 item 1), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once
    /// opened rather than removed, matching <see cref="DistantHoldings"/>'s identical "kept for the
    /// campaign's lifetime" convention — a resolved case (Recovered, with or without granted Immunity,
    /// or Fatal) is real campaign history <see cref="Health.HealthQueries"/> reads back, not scratch
    /// state to discard.</summary>
    public OrderedRegistry<RuntimeId<CharacterHealthCondition>, CharacterHealthCondition> CharacterHealthConditions { get; } = new();

    /// <summary>One settlement's standing §6 Sanitation Investment tier (Phase 14 item 2), keyed
    /// directly by <see cref="RuntimeId{Settlement}"/> — a settlement missing an entry here reads as
    /// <see cref="Health.SanitationInvestmentTier.Minimal"/> (<see cref="Health.SanitationQueries.EffectiveTier"/>).</summary>
    public OrderedRegistry<RuntimeId<Settlement>, SettlementSanitationInvestment> SettlementSanitationInvestments { get; } = new();

    /// <summary>Every standing <see cref="Health.EpidemicOutbreak"/>, active or ended (Phase 14 item
    /// 2), kept forever once opened — matching <see cref="CharacterHealthConditions"/>'s identical
    /// "resolved or not, kept for the campaign's lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<EpidemicOutbreak>, EpidemicOutbreak> EpidemicOutbreaks { get; } = new();

    /// <summary>Every fired §5 <see cref="Hazards.DisasterEvent"/> (Phase 14 item 3), in ascending-<see
    /// cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once fired, matching <see
    /// cref="EpidemicOutbreaks"/>'s identical "resolved or not, kept for the campaign's lifetime"
    /// convention — a Disaster Event is real campaign history, not scratch state to discard.</summary>
    public OrderedRegistry<RuntimeId<DisasterEvent>, DisasterEvent> DisasterEvents { get; } = new();

    /// <summary>Every §2.2 <see cref="Hazards.DormantVolcano"/> designation (Phase 14 item 3), keyed
    /// directly by the <see cref="RuntimeId{Plot}"/> it was designated on — a plot missing an entry here
    /// simply carries no Dormant Volcano, the same "absence reads as the default" convention <see
    /// cref="SettlementSanitationInvestments"/> already established for its own owner-keyed
    /// partition.</summary>
    public OrderedRegistry<RuntimeId<Plot>, DormantVolcano> DormantVolcanoes { get; } = new();

    /// <summary>Every individually-tracked <see cref="Gens.Simulation.Wanderers.Wanderer"/> (Phase 14 item 4), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004). Kept forever once instantiated — even a
    /// Recruited Wanderer stays, as the campaign's own record of who that person was before they
    /// joined — matching <see cref="DisasterEvents"/>'s identical "real campaign history, not scratch
    /// state to discard" convention. Purely ambient Wanderers are never stored at all (see <see
    /// cref="Gens.Simulation.Wanderers.Wanderer"/>'s own doc comment on §8's sampling pattern).</summary>
    public OrderedRegistry<RuntimeId<Wanderer>, Wanderer> Wanderers { get; } = new();

    /// <summary>Every completed §6 <see cref="Gens.Simulation.Wanderers.WandererEngagement"/> (Phase 14 item 4), in
    /// ascending-<see cref="RuntimeId{T}"/> (chronological) order, kept forever once written for the
    /// same reason as <see cref="Wanderers"/> itself.</summary>
    public OrderedRegistry<RuntimeId<WandererEngagement>, WandererEngagement> WandererEngagements { get; } = new();

    /// <summary>Which <see cref="History.HistoricalTimelineEntryDefinition"/>s (by <see
    /// cref="DefinitionId{T}.Value"/>) <see cref="History.HistoricalTimelineScheduler"/> has already
    /// fired this campaign, mapped to the date each fired — real state, not derivable, so a save/load
    /// round trip never re-fires an already-resolved entry. Keyed by the definition's own string ID
    /// rather than <see cref="OrderedRegistry{TId,TEntity}"/>'s usual <see cref="RuntimeId{T}"/>/key-
    /// struct key, since <see cref="DefinitionId{T}"/> itself implements no <see cref="IComparable{T}"/>
    /// this registry's ordering guarantee could sort on.</summary>
    public OrderedRegistry<string, GameDate> FiredHistoricalTimelineEntryIds { get; } = new();

    /// <summary>Every §4 <see cref="Gens.Simulation.RealEstate.District"/> (Phase 15 item 1), in
    /// ascending-<see cref="RuntimeId{T}"/> order.</summary>
    public OrderedRegistry<RuntimeId<District>, District> Districts { get; } = new();

    /// <summary>Every §3 <see cref="Gens.Simulation.RealEstate.PropertyRecord"/> — a Ship or Named
    /// Holding, the two asset kinds with no Plot of their own (Phase 15 item 1).</summary>
    public OrderedRegistry<RuntimeId<PropertyRecord>, PropertyRecord> PropertyRecords { get; } = new();

    /// <summary>§3's management-status flag and ownership extension for a player-owned <see
    /// cref="Plot"/> (Phase 15 item 1) — sparse, matching <see cref="Fame.CharacterFame"/>'s identical
    /// "present only once touched" convention (see <see
    /// cref="Gens.Simulation.RealEstate.PlotPropertyResolver.Current"/> for the untouched default).</summary>
    public OrderedRegistry<RuntimeId<Plot>, PlotPropertyExtension> PlotPropertyExtensions { get; } = new();

    /// <summary>Every §10 <see cref="Gens.Simulation.Societates.Societas"/> (Phase 15 item 2), in
    /// ascending-<see cref="RuntimeId{T}"/> order. Kept forever once formed, dissolved or not, matching
    /// <see cref="LegalCases"/>'s and <see cref="ScandalRecords"/>'s identical "kept for the campaign's
    /// lifetime" convention.</summary>
    public OrderedRegistry<RuntimeId<Societas>, Societas> Societates { get; } = new();

    /// <summary>§6's <see cref="Gens.Simulation.Societates.ActioProSocioLink"/> — which Societas and
    /// dispute type a <see cref="LegalCase"/> is actually about (Phase 15 item 2), sparse and keyed by
    /// that already-issued case's own <see cref="RuntimeId{T}"/>, matching <see
    /// cref="PlotPropertyExtensions"/>'s identical "wrap the existing record in a parallel partition"
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<LegalCase>, ActioProSocioLink> ActioProSocioLinks { get; } = new();

    /// <summary>Every §7/§9 <see cref="Gens.Simulation.MerchantFamilies.MerchantHouseArchetype"/> (Phase
    /// 15 item 3), sparse and keyed by its owner's own <see
    /// cref="Gens.Simulation.RealEstate.PropertyOwnerRef.ToTaggedOwnerId"/> string — a plain <see
    /// cref="string"/> key rather than a <see cref="RuntimeId{T}"/>, matching <see
    /// cref="FiredHistoricalTimelineEntryIds"/>'s identical "key by a tag this <see
    /// cref="OrderedRegistry{TId,TEntity}"/> can order on" convention, since <see
    /// cref="Gens.Simulation.RealEstate.PropertyOwnerRef"/> itself implements no <see
    /// cref="IComparable{T}"/> this registry's ordering guarantee could sort on (the same reason <see
    /// cref="FiredHistoricalTimelineEntryIds"/> keys by string rather than by <see
    /// cref="Gens.Simulation.History.HistoricalTimelineEntryDefinition"/>'s own <see
    /// cref="DefinitionId{T}"/>).</summary>
    public OrderedRegistry<string, MerchantHouseArchetype> MerchantHouseArchetypes { get; } = new();

    /// <summary>Every player household's §6/§9 <see
    /// cref="Gens.Simulation.MerchantFamilies.SenateEntryInvestmentLog"/> (Phase 15 item 3), sparse and
    /// keyed by <see cref="RuntimeId{Household}"/>, matching <see
    /// cref="HouseholdReputations"/>'s identical "present only once touched" convention — see <see
    /// cref="Gens.Simulation.MerchantFamilies.SenateEntryInvestmentLog"/>'s own doc comment for why this
    /// partition is player-household-only.</summary>
    public OrderedRegistry<RuntimeId<Household>, SenateEntryInvestmentLog> SenateEntryInvestmentLogs { get; } = new();

    /// <summary>Every §2/§10 <see cref="Gens.Simulation.NotableBusinesses.NotableBusiness"/> (Phase 15
    /// item 4), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusiness> NotableBusinesses { get; } = new();

    /// <summary>Every initiating business's own §5/§10 <see
    /// cref="Gens.Simulation.NotableBusinesses.NotableBusinessRivalryLog"/> (Phase 15 item 4), sparse and
    /// keyed by the already-registered <see cref="RuntimeId{NotableBusiness}"/>, matching <see
    /// cref="SenateEntryInvestmentLogs"/>'s identical "present only once touched" convention.</summary>
    public OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusinessRivalryLog> NotableBusinessRivalryLogs { get; } = new();

    /// <summary>Every business's own §7 <see
    /// cref="Gens.Simulation.NotableBusinesses.NotableBusinessGovernmentContract"/> (Phase 15 item 4),
    /// sparse and keyed by the already-registered <see cref="RuntimeId{NotableBusiness}"/> — at most one
    /// active contract per business, per that record's own doc comment on why this item omits a
    /// redundant <c>activeGovernmentContractId</c> field.</summary>
    public OrderedRegistry<RuntimeId<NotableBusiness>, NotableBusinessGovernmentContract> NotableBusinessGovernmentContracts { get; } = new();

    /// <summary>Every aggressor business's own real, running §2 <see
    /// cref="Gens.Simulation.BusinessCompetition.CompetitiveEscalation"/> (Phase 15 item 5), sparse and
    /// keyed by the already-registered <see cref="RuntimeId{NotableBusiness}"/>, matching <see
    /// cref="NotableBusinessGovernmentContracts"/>'s identical "present only once touched" convention.</summary>
    public OrderedRegistry<RuntimeId<NotableBusiness>, CompetitiveEscalation> CompetitiveEscalations { get; } = new();

    /// <summary>Every §4/§10 <see cref="Gens.Simulation.BusinessCompetition.CartelAgreement"/> (Phase 15
    /// item 5), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<CartelAgreement>, CartelAgreement> CartelAgreements { get; } = new();

    /// <summary>Every grain-trading business's own §5/§10 <see
    /// cref="Gens.Simulation.BusinessCompetition.GrainHoardingRecord"/> (Phase 15 item 5), sparse and
    /// keyed by the already-registered <see cref="RuntimeId{NotableBusiness}"/>, matching <see
    /// cref="CompetitiveEscalations"/>'s identical "present only once touched" convention.</summary>
    public OrderedRegistry<RuntimeId<NotableBusiness>, GrainHoardingRecord> GrainHoardingRecords { get; } = new();

    /// <summary>Every §6/§9 <see cref="Gens.Simulation.BusinessCompetition.MarketCapacityReading"/> (Phase
    /// 15 item 5), keyed by <see cref="MarketGoodKey"/> — reused directly from <see
    /// cref="SettlementMarket"/>'s own identical (settlement, good) key shape rather than a second key
    /// type for the same pairing.</summary>
    public OrderedRegistry<MarketGoodKey, MarketCapacityReading> MarketCapacityReadings { get; } = new();

    /// <summary>Every §8 <see cref="Gens.Simulation.PublicContracts.PublicContract"/> (Phase 15 item
    /// 6), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<PublicContract>, PublicContract> PublicContracts { get; } = new();

    /// <summary>Every §5/§8 <see cref="Gens.Simulation.PublicContracts.ContractBid"/> (Phase 15 item 6),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<ContractBid>, ContractBid> ContractBids { get; } = new();

    /// <summary>Every fired §3/§8 <see cref="Gens.Simulation.PublicContracts.LustrumEvent"/> (Phase 15
    /// item 6), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — kept forever, matching <see
    /// cref="Magistracies.MagistracyRecord"/>'s identical "a full campaign log, not just live
    /// state" convention.</summary>
    public OrderedRegistry<RuntimeId<LustrumEvent>, LustrumEvent> LustrumEvents { get; } = new();

    /// <summary>Every discovered §6.2/§8 <see cref="Gens.Simulation.PublicContracts.ContractFraudRecord"/>
    /// (Phase 15 item 6), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<ContractFraudRecord>, ContractFraudRecord> PublicContractFraudRecords { get; } = new();

    /// <summary>Which <see cref="Gens.Simulation.PublicContracts.ContractFraudRecord"/> a given §6.2
    /// repetundae <see cref="LegalCase.CaseId"/> is actually about (Phase 15 item 6), sparse and keyed by
    /// that already-issued case ID, matching <see cref="ActioProSocioLinks"/>'s identical convention.</summary>
    public OrderedRegistry<RuntimeId<LegalCase>, ContractFraudLegalLink> ContractFraudLegalLinks { get; } = new();

    /// <summary>Every §2/§4 <see cref="Gens.Simulation.PrivateInfrastructure.PavedRoadConnection"/>
    /// (Phase 15 item 7), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<PavedRoadConnection>, PavedRoadConnection> PavedRoadConnections { get; } = new();

    /// <summary>Every §3 <see cref="Gens.Simulation.PrivateInfrastructure.IrrigationCanal"/> (Phase 15
    /// item 7), sparse and keyed by the Plot it was built on, matching <see
    /// cref="RealEstate.PlotPropertyExtension"/>'s own "wrap the existing record in a parallel partition"
    /// convention.</summary>
    public OrderedRegistry<RuntimeId<Plot>, IrrigationCanal> IrrigationCanals { get; } = new();

    /// <summary>Every §3.1 <see cref="Gens.Simulation.PrivateInfrastructure.WellOrCistern"/> (Phase 15
    /// item 7), sparse and keyed by Plot, same convention as <see cref="IrrigationCanals"/>.</summary>
    public OrderedRegistry<RuntimeId<Plot>, WellOrCistern> WellOrCisterns { get; } = new();

    /// <summary>Every §5 <see cref="Gens.Simulation.PrivateInfrastructure.LandReclamationProject"/>
    /// (Phase 15 item 7), sparse and keyed by Plot — at most one project (in progress or resolved) per
    /// Plot.</summary>
    public OrderedRegistry<RuntimeId<Plot>, LandReclamationProject> LandReclamationProjects { get; } = new();

    /// <summary>Every §6 <see cref="Gens.Simulation.PrivateInfrastructure.PrivateBridge"/> (Phase 15
    /// item 7), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<PrivateBridge>, PrivateBridge> PrivateBridges { get; } = new();

    /// <summary>Every §7 <see cref="Gens.Simulation.PrivateInfrastructure.BoundaryInfrastructure"/>
    /// (Phase 15 item 7), sparse and keyed by Plot, same convention as <see cref="IrrigationCanals"/>.</summary>
    public OrderedRegistry<RuntimeId<Plot>, BoundaryInfrastructure> BoundaryInfrastructures { get; } = new();

    /// <summary>Every §8 <see cref="Gens.Simulation.PrivateInfrastructure.InfrastructureCondition"/>
    /// (Phase 15 item 7), one entry per built structure above, keyed by that structure's own <see
    /// cref="Gens.Simulation.PrivateInfrastructure.InfrastructureConditionKey"/>.</summary>
    public OrderedRegistry<InfrastructureConditionKey, InfrastructureCondition> InfrastructureConditions { get; } = new();

    /// <summary>§4.1's Unified Estate milestone — the <see cref="GameDate"/> a household's own <see
    /// cref="RoadClusterQuery"/> cluster first covered every Plot it holds, sparse and keyed by
    /// household; present means already achieved, matching <see
    /// cref="PrivateInfrastructureBenefitsSystem"/>'s own "fire exactly once" guard.</summary>
    public OrderedRegistry<RuntimeId<Household>, GameDate> UnifiedEstateMilestones { get; } = new();

    /// <summary>Every §11 <see cref="Gens.Simulation.Shipping.MerchantShip"/> (Phase 15 item 8), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<MerchantShip>, MerchantShip> MerchantShips { get; } = new();

    /// <summary>Every §3 <see cref="Gens.Simulation.Shipping.ShipCommissionProject"/> (Phase 15 item 8),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004) — kept forever, matching <see
    /// cref="PrivateInfrastructure.LandReclamationProject"/>'s own "in progress or resolved, a real
    /// record either way" convention.</summary>
    public OrderedRegistry<RuntimeId<ShipCommissionProject>, ShipCommissionProject> ShipCommissionProjects { get; } = new();

    /// <summary>Every §5 <see cref="Gens.Simulation.Shipping.FrontingArrangement"/> (Phase 15 item 8),
    /// sparse and keyed by the already-registered <see cref="RuntimeId{MerchantShip}"/> it describes,
    /// matching <see cref="RealEstate.PlotPropertyExtension"/>'s own "wrap the existing record in a
    /// parallel partition" convention.</summary>
    public OrderedRegistry<RuntimeId<MerchantShip>, FrontingArrangement> ShipFrontingArrangements { get; } = new();

    /// <summary>Every resolved §6.2 <see cref="Gens.Simulation.Shipping.VoyageEvent"/> (Phase 15 item 8),
    /// in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<VoyageEvent>, VoyageEvent> VoyageEvents { get; } = new();

    /// <summary>§4's Flagship-designation Dignitas-award dates (Phase 15 item 8), sparse and keyed by
    /// household; present means that household has already received <see
    /// cref="Gens.Simulation.Shipping.ShippingCatalog.FlagshipDesignationDignitasAward"/> once, matching
    /// <see cref="UnifiedEstateMilestones"/>'s own identical "sparse, present-means-achieved, fire
    /// exactly once" guard.</summary>
    public OrderedRegistry<RuntimeId<Household>, GameDate> FlagshipDesignationAwards { get; } = new();

    /// <summary>Every §9 <see cref="Gens.Simulation.PublicWorks.PublicWork"/> (Phase 15 item 9), in
    /// ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<PublicWork>, PublicWork> PublicWorks { get; } = new();

    /// <summary>Every §2 <see cref="Gens.Simulation.PublicWorks.EuergetismObligation"/> (Phase 15 item 9),
    /// sparse and keyed by household — present only once a household has either funded a Public Work or
    /// been read as wealthy enough to matter, matching <see cref="RealEstate.PlotPropertyExtension"/>'s
    /// own "wrap the existing record in a parallel partition" convention.</summary>
    public OrderedRegistry<RuntimeId<Household>, EuergetismObligation> EuergetismObligations { get; } = new();

    /// <summary>Every §5 <see cref="Gens.Simulation.PublicWorks.CompetitiveEuergetismEvent"/> (Phase 15
    /// item 9), in ascending-<see cref="RuntimeId{T}"/> order (ADR 0004).</summary>
    public OrderedRegistry<RuntimeId<CompetitiveEuergetismEvent>, CompetitiveEuergetismEvent> CompetitiveEuergetismEvents { get; } = new();

    public KnowledgeState Knowledge { get; } = new();

    public GameDate Date { get; private set; }

    public void AdvanceMonth() => Date = Date.NextMonth();

    private long _nextCommandSequenceNumber;

    /// <summary>Assigns the next deterministic command sequence number, at acceptance time (ADR 0006).</summary>
    public long IssueCommandSequenceNumber() => checked(_nextCommandSequenceNumber++);

    /// <summary>The sequence number that will be issued next. Persist and restore this for save compatibility.</summary>
    public long NextCommandSequenceNumber => _nextCommandSequenceNumber;

    /// <summary>Snapshots a version/counter value per declared partition tag, for debug-only
    /// write-set verification (ADR 0005). Not a save-relevant operation.</summary>
    internal IReadOnlyDictionary<string, long> CapturePartitionVersions() => new Dictionary<string, long>(StringComparer.Ordinal)
    {
        ["regionIds"] = RegionIds.Peek,
        ["settlementIds"] = SettlementIds.Peek,
        ["plotIds"] = PlotIds.Peek,
        ["holdingIds"] = HoldingIds.Peek,
        ["householdIds"] = HouseholdIds.Peek,
        ["actorIds"] = ActorIds.Peek,
        ["characterIds"] = CharacterIds.Peek,
        ["buildingIds"] = BuildingIds.Peek,
        ["contractIds"] = ContractIds.Peek,
        ["activityIds"] = ActivityIds.Peek,
        ["commandIds"] = CommandIds.Peek,
        ["eventIds"] = EventIds.Peek,
        ["scheduledActionIds"] = ScheduledActionIds.Peek,
        ["ledgerTransactionIds"] = LedgerTransactionIds.Peek,
        ["debtRecordIds"] = DebtRecordIds.Peek,
        ["standingContractIds"] = StandingContractIds.Peek,
        ["eventInstanceIds"] = EventInstanceIds.Peek,
        ["stewardshipAssignmentIds"] = StewardshipAssignmentIds.Peek,
        ["autonomousDecisionLogIds"] = AutonomousDecisionLogIds.Peek,
        ["schemeIds"] = SchemeIds.Peek,
        ["returnReportIds"] = ReturnReportIds.Peek,
        ["successionDisputeIds"] = SuccessionDisputeIds.Peek,
        ["chronicleEntryIds"] = ChronicleEntryIds.Peek,
        ["funeralRecordIds"] = FuneralRecordIds.Peek,
        ["agnomenIds"] = AgnomenIds.Peek,
        ["inheritedCognomenDecisionIds"] = InheritedCognomenDecisionIds.Peek,
        ["favorObligationIds"] = FavorObligationIds.Peek,
        ["magistracyRecordIds"] = MagistracyRecordIds.Peek,
        ["omenEventIds"] = OmenEventIds.Peek,
        ["priesthoodRecordIds"] = PriesthoodRecordIds.Peek,
        ["legalCaseIds"] = LegalCaseIds.Peek,
        ["punishableOffenseIds"] = PunishableOffenseIds.Peek,
        ["detentionRecordIds"] = DetentionRecordIds.Peek,
        ["sentenceRecordIds"] = SentenceRecordIds.Peek,
        ["ransomNegotiationIds"] = RansomNegotiationIds.Peek,
        ["scandalRecordIds"] = ScandalRecordIds.Peek,
        ["travelTripIds"] = TravelTripIds.Peek,
        ["letterIds"] = LetterIds.Peek,
        ["languageProficiencyIds"] = LanguageProficiencyIds.Peek,
        ["divergenceRecordIds"] = DivergenceRecordIds.Peek,
        ["distantHoldingIds"] = DistantHoldingIds.Peek,
        ["characterHealthConditionIds"] = CharacterHealthConditionIds.Peek,
        ["epidemicOutbreakIds"] = EpidemicOutbreakIds.Peek,
        ["disasterEventIds"] = DisasterEventIds.Peek,
        ["wandererIds"] = WandererIds.Peek,
        ["wandererEngagementIds"] = WandererEngagementIds.Peek,
        ["districtIds"] = DistrictIds.Peek,
        ["propertyRecordIds"] = PropertyRecordIds.Peek,
        ["societasIds"] = SocietasIds.Peek,
        ["regions"] = Regions.Version,
        ["settlements"] = Settlements.Version,
        ["plots"] = Plots.Version,
        ["holdings"] = Holdings.Version,
        ["characters"] = Characters.Version,
        ["relationships"] = Relationships.Version,
        ["scheduledActions"] = ScheduledActions.Version,
        ["popGroups"] = PopGroups.Version,
        ["householdRegimenDefaults"] = HouseholdRegimenDefaults.Version,
        ["buildings"] = Buildings.Version,
        ["stockpiles"] = Stockpiles.Version,
        ["constructionSchedules"] = ConstructionSchedules.Version,
        ["ledgerAccounts"] = LedgerAccounts.Version,
        ["ledgerTransactions"] = LedgerTransactions.Version,
        ["marketPrices"] = MarketPrices.Version,
        ["householdStatements"] = HouseholdStatements.Version,
        ["debtRecords"] = DebtRecords.Version,
        ["netWorthAssessments"] = NetWorthAssessments.Version,
        ["insolvencyStates"] = InsolvencyStates.Version,
        ["standingContracts"] = StandingContracts.Version,
        ["householdPolicies"] = HouseholdPolicies.Version,
        ["eventInstances"] = EventInstances.Version,
        ["actors"] = Actors.Version,
        ["houseStandings"] = HouseStandings.Version,
        ["rivalDossiers"] = RivalDossiers.Version,
        ["regionalFamiliesEntries"] = RegionalFamiliesEntries.Version,
        ["stewardshipAssignments"] = StewardshipAssignments.Version,
        ["autonomousDecisionLogs"] = AutonomousDecisionLogs.Version,
        ["schemes"] = Schemes.Version,
        ["returnReports"] = ReturnReports.Version,
        ["householdHeadships"] = HouseholdHeadships.Version,
        ["heirDesignations"] = HeirDesignations.Version,
        ["successionDisputes"] = SuccessionDisputes.Version,
        ["playerControls"] = PlayerControls.Version,
        ["chronicleEntries"] = ChronicleEntries.Version,
        ["generationalChapters"] = GenerationalChapters.Version,
        ["funeralRecords"] = FuneralRecords.Version,
        ["mourningPeriods"] = MourningPeriods.Version,
        ["memoriaStates"] = MemoriaStates.Version,
        ["agnomens"] = Agnomens.Version,
        ["inheritedCognomenDecisions"] = InheritedCognomenDecisions.Version,
        ["dynasticEpithets"] = DynasticEpithets.Version,
        ["householdReputations"] = HouseholdReputations.Version,
        ["favorObligations"] = FavorObligations.Version,
        ["clientelaEntries"] = ClientelaEntries.Version,
        ["householdInfluences"] = HouseholdInfluences.Version,
        ["characterFactionAlignments"] = CharacterFactionAlignments.Version,
        ["magistracyRecords"] = MagistracyRecords.Version,
        ["householdReligions"] = HouseholdReligions.Version,
        ["omenEvents"] = OmenEvents.Version,
        ["priesthoodRecords"] = PriesthoodRecords.Version,
        ["legalCases"] = LegalCases.Version,
        ["punishableOffenses"] = PunishableOffenses.Version,
        ["detentionRecords"] = DetentionRecords.Version,
        ["sentenceRecords"] = SentenceRecords.Version,
        ["ransomNegotiations"] = RansomNegotiations.Version,
        ["collegia"] = Collegia.Version,
        ["scandalRecords"] = ScandalRecords.Version,
        ["characterFames"] = CharacterFames.Version,
        ["householdDoctrines"] = HouseholdDoctrines.Version,
        ["travelTrips"] = TravelTrips.Version,
        ["letters"] = Letters.Version,
        ["languageProficiencies"] = LanguageProficiencies.Version,
        ["literacyRecords"] = LiteracyRecords.Version,
        ["interpresAppointments"] = InterpresAppointments.Version,
        ["divergenceRecords"] = DivergenceRecords.Version,
        ["distantHoldings"] = DistantHoldings.Version,
        ["characterHealthConditions"] = CharacterHealthConditions.Version,
        ["settlementSanitationInvestments"] = SettlementSanitationInvestments.Version,
        ["epidemicOutbreaks"] = EpidemicOutbreaks.Version,
        ["disasterEvents"] = DisasterEvents.Version,
        ["dormantVolcanoes"] = DormantVolcanoes.Version,
        ["wanderers"] = Wanderers.Version,
        ["wandererEngagements"] = WandererEngagements.Version,
        ["firedHistoricalTimelineEntryIds"] = FiredHistoricalTimelineEntryIds.Version,
        ["edictRecords"] = EdictRecords.Version,
        ["districts"] = Districts.Version,
        ["propertyRecords"] = PropertyRecords.Version,
        ["plotPropertyExtensions"] = PlotPropertyExtensions.Version,
        ["societates"] = Societates.Version,
        ["actioProSocioLinks"] = ActioProSocioLinks.Version,
        ["merchantHouseArchetypes"] = MerchantHouseArchetypes.Version,
        ["senateEntryInvestmentLogs"] = SenateEntryInvestmentLogs.Version,
        ["notableBusinesses"] = NotableBusinesses.Version,
        ["notableBusinessRivalryLogs"] = NotableBusinessRivalryLogs.Version,
        ["notableBusinessGovernmentContracts"] = NotableBusinessGovernmentContracts.Version,
        ["competitiveEscalations"] = CompetitiveEscalations.Version,
        ["cartelAgreements"] = CartelAgreements.Version,
        ["grainHoardingRecords"] = GrainHoardingRecords.Version,
        ["marketCapacityReadings"] = MarketCapacityReadings.Version,
        ["publicContracts"] = PublicContracts.Version,
        ["contractBids"] = ContractBids.Version,
        ["lustrumEvents"] = LustrumEvents.Version,
        ["publicContractFraudRecords"] = PublicContractFraudRecords.Version,
        ["contractFraudLegalLinks"] = ContractFraudLegalLinks.Version,
        ["pavedRoadConnections"] = PavedRoadConnections.Version,
        ["irrigationCanals"] = IrrigationCanals.Version,
        ["wellOrCisterns"] = WellOrCisterns.Version,
        ["landReclamationProjects"] = LandReclamationProjects.Version,
        ["privateBridges"] = PrivateBridges.Version,
        ["boundaryInfrastructures"] = BoundaryInfrastructures.Version,
        ["infrastructureConditions"] = InfrastructureConditions.Version,
        ["unifiedEstateMilestones"] = UnifiedEstateMilestones.Version,
        ["knowledge"] = Knowledge.Version,
        ["commandSequence"] = NextCommandSequenceNumber,
        ["date"] = Date.TotalMonths,
    };
}
