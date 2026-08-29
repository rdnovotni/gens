using System.Text.Json.Serialization;
using Gens.Simulation.Characters;
using Gens.Simulation.Funerary;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;

namespace Gens.Simulation.Saves;

/// <summary>The <c>world.json</c> entry's canonical shape (ADR 0010): every <see
/// cref="State.WorldState"/> counter, ordered partition, and the command sequence number, in an
/// explicit property order. Collections are pre-sorted by <see cref="WorldStateMapper"/> before
/// reaching this DTO — <see cref="CanonicalJson"/> never sorts on a DTO's behalf.</summary>
public sealed record WorldSaveDocument
{
    [JsonPropertyOrder(0)]
    public required int DateTotalMonths { get; init; }

    [JsonPropertyOrder(1)]
    public required long NextCommandSequenceNumber { get; init; }

    [JsonPropertyOrder(2)]
    public required CounterSetDto Counters { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> CharacterIds { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<KnowledgeEntryDto> Knowledge { get; init; }

    /// <summary>The calendar queue (Phase 4 item 4): future-dated work not yet due. Already in
    /// ascending (due date, action ID) order (ADR 0004) via <see
    /// cref="Identity.OrderedRegistry{TId,TEntity}.InAscendingOrder"/>. Not <c>required</c>, and
    /// defaults to empty: ADR 0011's "additive only until v1 ships" policy means the permanent
    /// <c>v1-empty-campaign.gens</c> fixture — written before this field existed — must still load at
    /// <see cref="SaveFormat.CurrentVersion"/> without a migration.</summary>
    [JsonPropertyOrder(5)]
    public IReadOnlyList<ScheduledActionEntryDto> ScheduledActions { get; init; } = Array.Empty<ScheduledActionEntryDto>();

    /// <summary>Every Character's full record (Phase 5 item 1), already in ascending-<see
    /// cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="ScheduledActions"/> above: <see cref="CharacterIds"/> alone
    /// still fully describes a pre-Phase-5 save (its Characters partition was always empty), so no
    /// migration is needed for this field to appear. <see cref="CharacterIds"/> itself is kept,
    /// redundantly derivable from this list's IDs, purely for that same backward compatibility.</summary>
    [JsonPropertyOrder(6)]
    public IReadOnlyList<CharacterDto> Characters { get; init; } = Array.Empty<CharacterDto>();

    /// <summary>Every directed <see cref="Characters.Relationship"/> (Phase 5 item 5), already in
    /// ascending (From, To) order. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="Characters"/> above: a pre-Phase-5-item-5 save's Characters
    /// never had a relationship web populated.</summary>
    [JsonPropertyOrder(7)]
    public IReadOnlyList<RelationshipDto> Relationships { get; init; } = Array.Empty<RelationshipDto>();

    /// <summary>Every background <see cref="Characters.PopGroup"/> (Phase 5 item 7; ADR 0009),
    /// already in ascending (settlement, group type) order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Characters"/> above: a pre-Phase-5-item-7
    /// save never had any background population groups recorded.</summary>
    [JsonPropertyOrder(8)]
    public IReadOnlyList<PopGroupDto> PopGroups { get; init; } = Array.Empty<PopGroupDto>();

    /// <summary>Every <see cref="Land.Region"/> boundary record (Phase 6 item 1), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty: a pre-Phase-6 save has no regions, matching ADR 0011's additive-only policy.</summary>
    [JsonPropertyOrder(9)]
    public IReadOnlyList<RegionDto> Regions { get; init; } = Array.Empty<RegionDto>();

    /// <summary>Every <see cref="Land.Settlement"/> boundary record (Phase 6 item 1), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Regions"/>.</summary>
    [JsonPropertyOrder(10)]
    public IReadOnlyList<SettlementDto> Settlements { get; init; } = Array.Empty<SettlementDto>();

    /// <summary>Every <see cref="Land.Plot"/> boundary record (Phase 6 item 1), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Regions"/>.</summary>
    [JsonPropertyOrder(11)]
    public IReadOnlyList<PlotDto> Plots { get; init; } = Array.Empty<PlotDto>();

    /// <summary>Every <see cref="Land.Holding"/> boundary record (Phase 6 item 1), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Regions"/>.</summary>
    [JsonPropertyOrder(12)]
    public IReadOnlyList<HoldingDto> Holdings { get; init; } = Array.Empty<HoldingDto>();

    /// <summary>Every household-level Regimen default (Phase 6 item 6; <c>gens-labor-slavery-design.md</c>
    /// §5). Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="Regions"/>: a pre-Phase-6-item-6 save never had any group Regimen defaults recorded.</summary>
    [JsonPropertyOrder(13)]
    public IReadOnlyList<HouseholdRegimenDefaultDto> HouseholdRegimenDefaults { get; init; } = Array.Empty<HouseholdRegimenDefaultDto>();

    /// <summary>Every completed <see cref="Buildings.BuildingInstance"/> (Phase 6 item 7), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Regions"/>: a pre-Phase-6-item-7 save
    /// never had any completed buildings recorded.</summary>
    [JsonPropertyOrder(14)]
    public IReadOnlyList<BuildingInstanceDto> Buildings { get; init; } = Array.Empty<BuildingInstanceDto>();

    /// <summary>Every Holding's <see cref="Goods.Stockpile"/> (Phase 6 items 3/7), keyed by Holding.
    /// Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="Regions"/>: a pre-Phase-6-item-7 save never had any stockpiles provisioned.</summary>
    [JsonPropertyOrder(15)]
    public IReadOnlyList<StockpileDto> Stockpiles { get; init; } = Array.Empty<StockpileDto>();

    /// <summary>Every Holding's <see cref="Buildings.ConstructionSchedule"/> (Phase 6 item 7), keyed by
    /// Holding. Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="Regions"/>: a pre-Phase-6-item-7 save never had any construction queues provisioned.</summary>
    [JsonPropertyOrder(16)]
    public IReadOnlyList<ConstructionScheduleDto> ConstructionSchedules { get; init; } = Array.Empty<ConstructionScheduleDto>();

    /// <summary>Every <see cref="Ledger.LedgerAccount"/> (Phase 8 items 1-2), already in ascending
    /// <see cref="Ledger.LedgerAccountKey"/> order. Not <c>required</c>, and defaults to empty — no
    /// pre-Phase-8 save ever had a ledger, matching ADR 0011's additive-only policy.</summary>
    [JsonPropertyOrder(17)]
    public IReadOnlyList<LedgerAccountDto> LedgerAccounts { get; init; } = Array.Empty<LedgerAccountDto>();

    /// <summary>Every posted <see cref="Ledger.LedgerTransaction"/> (Phase 8 item 1), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="LedgerAccounts"/>.</summary>
    [JsonPropertyOrder(18)]
    public IReadOnlyList<LedgerTransactionDto> LedgerTransactions { get; init; } = Array.Empty<LedgerTransactionDto>();

    /// <summary>Every cleared <see cref="Markets.SettlementMarket"/> (Phase 8 items 3-4), already in
    /// ascending <see cref="Markets.MarketGoodKey"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="LedgerAccounts"/>.</summary>
    [JsonPropertyOrder(19)]
    public IReadOnlyList<SettlementMarketDto> MarketPrices { get; init; } = Array.Empty<SettlementMarketDto>();

    /// <summary>Every household's latest <see cref="Economy.HouseholdMonthlyStatement"/> (Phase 8 item
    /// 5), already keyed by household. Not <c>required</c>, and defaults to empty — no pre-Phase-8-item-5
    /// save ever had a statement, matching ADR 0011's additive-only policy.</summary>
    [JsonPropertyOrder(20)]
    public IReadOnlyList<HouseholdMonthlyStatementDto> HouseholdStatements { get; init; } = Array.Empty<HouseholdMonthlyStatementDto>();

    /// <summary>Every <see cref="Economy.DebtRecord"/> (Phase 8 item 6), already in ascending-<see
    /// cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="HouseholdStatements"/>.</summary>
    [JsonPropertyOrder(21)]
    public IReadOnlyList<DebtRecordDto> DebtRecords { get; init; } = Array.Empty<DebtRecordDto>();

    /// <summary>Every household's latest <see cref="Economy.NetWorth"/> assessment (Phase 8 item 6; §8),
    /// already keyed by household. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="HouseholdStatements"/>.</summary>
    [JsonPropertyOrder(22)]
    public IReadOnlyList<NetWorthDto> NetWorthAssessments { get; init; } = Array.Empty<NetWorthDto>();

    /// <summary>Every household's <see cref="Economy.InsolvencyState"/> (Phase 8 item 6; §9), already
    /// keyed by household. Not <c>required</c>, and defaults to empty, for the same additive-only reason
    /// as <see cref="HouseholdStatements"/>.</summary>
    [JsonPropertyOrder(23)]
    public IReadOnlyList<InsolvencyStateDto> InsolvencyStates { get; init; } = Array.Empty<InsolvencyStateDto>();

    /// <summary>Every <see cref="Economy.StandingContract"/> (Phase 8 item 7), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty,
    /// for the same additive-only reason as <see cref="HouseholdStatements"/>.</summary>
    [JsonPropertyOrder(24)]
    public IReadOnlyList<StandingContractDto> StandingContracts { get; init; } = Array.Empty<StandingContractDto>();

    /// <summary>Every household's current Standing Policy configuration (Phase 9 item 2), already
    /// keyed by household. Not <c>required</c>, and defaults to empty, for the same additive-only
    /// reason as <see cref="StandingContracts"/>: a pre-Phase-9-item-2 save never had any Standing
    /// Policy state recorded.</summary>
    [JsonPropertyOrder(25)]
    public IReadOnlyList<HouseholdPolicyStateDto> HouseholdPolicies { get; init; } = Array.Empty<HouseholdPolicyStateDto>();

    /// <summary>Every fired <see cref="Events.EventInstance"/> (Phase 9 item 3), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="HouseholdPolicies"/> above: a
    /// pre-Phase-9-item-3 save never had any event instances recorded.</summary>
    [JsonPropertyOrder(26)]
    public IReadOnlyList<EventInstanceDto> EventInstances { get; init; } = Array.Empty<EventInstanceDto>();

    /// <summary>Every <see cref="Actors.LivingWorldActor"/> (Phase 10 item 3), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="EventInstances"/> above: a pre-Phase-10
    /// save never had any living-world actors recorded.</summary>
    [JsonPropertyOrder(27)]
    public IReadOnlyList<LivingWorldActorDto> Actors { get; init; } = Array.Empty<LivingWorldActorDto>();

    /// <summary>Every tracked house-pair's <see cref="Actors.HouseStanding"/> (Phase 10 item 5), already
    /// in ascending <see cref="Actors.HouseStandingKey"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="Actors"/> above.</summary>
    [JsonPropertyOrder(28)]
    public IReadOnlyList<HouseStandingDto> HouseStandings { get; init; } = Array.Empty<HouseStandingDto>();

    /// <summary>Every actor's <see cref="Actors.RivalDossier"/> (Phase 10 item 5), already keyed by
    /// actor. Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="Actors"/> above.</summary>
    [JsonPropertyOrder(29)]
    public IReadOnlyList<RivalDossierDto> RivalDossiers { get; init; } = Array.Empty<RivalDossierDto>();

    /// <summary>Every actor's <see cref="Actors.RegionalFamiliesEntry"/> (Phase 10 item 5), already
    /// keyed by actor. Not <c>required</c>, and defaults to empty, for the same additive-only reason as
    /// <see cref="Actors"/> above.</summary>
    [JsonPropertyOrder(30)]
    public IReadOnlyList<RegionalFamiliesEntryDto> RegionalFamiliesEntries { get; init; } = Array.Empty<RegionalFamiliesEntryDto>();

    /// <summary>Every <see cref="Stewardship.StewardshipAssignment"/> (Phase 10 item 2), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty,
    /// for the same additive-only reason as <see cref="Actors"/> above.</summary>
    [JsonPropertyOrder(31)]
    public IReadOnlyList<StewardshipAssignmentDto> StewardshipAssignments { get; init; } = Array.Empty<StewardshipAssignmentDto>();

    /// <summary>Every <see cref="Stewardship.AutonomousDecisionLog"/> ever recorded (Phase 10 item 10),
    /// already in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults
    /// to empty, for the same additive-only reason as <see cref="StewardshipAssignments"/> above.</summary>
    [JsonPropertyOrder(32)]
    public IReadOnlyList<AutonomousDecisionLogDto> AutonomousDecisionLogs { get; init; } = Array.Empty<AutonomousDecisionLogDto>();

    /// <summary>Every <see cref="Interactions.Scheme"/>, in-progress or resolved (Phase 10 item 6),
    /// already in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and
    /// defaults to empty, for the same additive-only reason as <see cref="AutonomousDecisionLogs"/>
    /// above.</summary>
    [JsonPropertyOrder(33)]
    public IReadOnlyList<SchemeDto> Schemes { get; init; } = Array.Empty<SchemeDto>();

    /// <summary>Every <see cref="Stewardship.ReturnReport"/> ever produced (Phase 10 package 13),
    /// already in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and
    /// defaults to empty, for the same additive-only reason as <see cref="Schemes"/> above.</summary>
    [JsonPropertyOrder(34)]
    public IReadOnlyList<ReturnReportDto> ReturnReports { get; init; } = Array.Empty<ReturnReportDto>();

    /// <summary>Every <see cref="Succession.HouseholdHeadship"/> (Phase 11 item 1), already keyed by
    /// household. Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="ReturnReports"/> above.</summary>
    [JsonPropertyOrder(35)]
    public IReadOnlyList<HouseholdHeadshipDto> HouseholdHeadships { get; init; } = Array.Empty<HouseholdHeadshipDto>();

    /// <summary>Every <see cref="Succession.HeirDesignation"/> (Phase 11 item 1), already keyed by
    /// household. Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="HouseholdHeadships"/> above.</summary>
    [JsonPropertyOrder(36)]
    public IReadOnlyList<HeirDesignationDto> HeirDesignations { get; init; } = Array.Empty<HeirDesignationDto>();

    /// <summary>Every <see cref="Succession.SuccessionDispute"/>, pending or resolved (Phase 11 item 1),
    /// already in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults
    /// to empty, for the same additive-only reason as <see cref="HeirDesignations"/> above.</summary>
    [JsonPropertyOrder(37)]
    public IReadOnlyList<SuccessionDisputeDto> SuccessionDisputes { get; init; } = Array.Empty<SuccessionDisputeDto>();

    /// <summary>Every <see cref="Succession.PlayerControlState"/> (Phase 11 item 2), already keyed by
    /// household. Not <c>required</c>, and defaults to empty, for the same additive-only reason as <see
    /// cref="SuccessionDisputes"/> above.</summary>
    [JsonPropertyOrder(38)]
    public IReadOnlyList<PlayerControlDto> PlayerControls { get; init; } = Array.Empty<PlayerControlDto>();

    /// <summary>Every <see cref="Chronicle.ChronicleEntry"/> ever recorded (Phase 11 item 3), already
    /// in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="PlayerControls"/> above.</summary>
    [JsonPropertyOrder(39)]
    public IReadOnlyList<ChronicleEntryDto> ChronicleEntries { get; init; } = Array.Empty<ChronicleEntryDto>();

    /// <summary>Every <see cref="Chronicle.GenerationalChapter"/>, past and present (Phase 11 item 3),
    /// already in ascending <see cref="Chronicle.GenerationalChapterKey"/> order. Not <c>required</c>,
    /// and defaults to empty, for the same additive-only reason as <see cref="ChronicleEntries"/>
    /// above.</summary>
    [JsonPropertyOrder(40)]
    public IReadOnlyList<GenerationalChapterDto> GenerationalChapters { get; init; } = Array.Empty<GenerationalChapterDto>();

    /// <summary>Every <see cref="Funerary.FuneralRecord"/>, pending or held (Phase 11 item 4), already
    /// in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to
    /// empty, for the same additive-only reason as <see cref="GenerationalChapters"/> above.</summary>
    [JsonPropertyOrder(41)]
    public IReadOnlyList<FuneralRecordDto> FuneralRecords { get; init; } = Array.Empty<FuneralRecordDto>();

    /// <summary>Every household's current <see cref="Funerary.MourningPeriod"/> (Phase 11 item 4),
    /// already keyed by household. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="FuneralRecords"/> above.</summary>
    [JsonPropertyOrder(42)]
    public IReadOnlyList<MourningPeriodDto> MourningPeriods { get; init; } = Array.Empty<MourningPeriodDto>();

    /// <summary>Every household's running <see cref="Funerary.MemoriaState"/> (Phase 11 item 4), already
    /// keyed by household. Not <c>required</c>, and defaults to empty, for the same additive-only
    /// reason as <see cref="FuneralRecords"/> above.</summary>
    [JsonPropertyOrder(43)]
    public IReadOnlyList<MemoriaStateDto> MemoriaStates { get; init; } = Array.Empty<MemoriaStateDto>();

    /// <summary>Every <see cref="Epithets.Agnomen"/> ever granted (Phase 11 item 5), already in
    /// ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults to empty,
    /// for the same additive-only reason as <see cref="MemoriaStates"/> above.</summary>
    [JsonPropertyOrder(44)]
    public IReadOnlyList<AgnomenDto> Agnomens { get; init; } = Array.Empty<AgnomenDto>();

    /// <summary>Every <see cref="Epithets.InheritedCognomenDecision"/> ever recorded (Phase 11 item 5),
    /// already in ascending-<see cref="Identity.RuntimeId{T}"/> order. Not <c>required</c>, and defaults
    /// to empty, for the same additive-only reason as <see cref="Agnomens"/> above.</summary>
    [JsonPropertyOrder(45)]
    public IReadOnlyList<InheritedCognomenDecisionDto> InheritedCognomenDecisions { get; init; } = Array.Empty<InheritedCognomenDecisionDto>();

    /// <summary>Every household's current <see cref="Epithets.DynasticEpithet"/> (Phase 11 item 5),
    /// already keyed by household. Not <c>required</c>, and defaults to empty, for the same
    /// additive-only reason as <see cref="InheritedCognomenDecisions"/> above.</summary>
    [JsonPropertyOrder(46)]
    public IReadOnlyList<DynasticEpithetDto> DynasticEpithets { get; init; } = Array.Empty<DynasticEpithetDto>();
}

/// <summary>The next-value of every per-entity-kind <see cref="Identity.RuntimeIdCounter{T}"/> (ADR
/// 0001) — itself campaign state, persisted like any other field.</summary>
public sealed record CounterSetDto
{
    [JsonPropertyOrder(0)]
    public required long RegionIds { get; init; }

    [JsonPropertyOrder(1)]
    public required long SettlementIds { get; init; }

    [JsonPropertyOrder(2)]
    public required long PlotIds { get; init; }

    [JsonPropertyOrder(3)]
    public required long HouseholdIds { get; init; }

    [JsonPropertyOrder(4)]
    public required long ActorIds { get; init; }

    [JsonPropertyOrder(5)]
    public required long CharacterIds { get; init; }

    [JsonPropertyOrder(6)]
    public required long BuildingIds { get; init; }

    [JsonPropertyOrder(7)]
    public required long ContractIds { get; init; }

    [JsonPropertyOrder(8)]
    public required long ActivityIds { get; init; }

    [JsonPropertyOrder(9)]
    public required long CommandIds { get; init; }

    [JsonPropertyOrder(10)]
    public required long EventIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: see <see cref="WorldSaveDocument.ScheduledActions"/>'s
    /// doc comment for why this field must tolerate a pre-Phase-4 save with no scheduled actions.</summary>
    [JsonPropertyOrder(11)]
    public long ScheduledActionIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-6 save has no holdings. Additive-only
    /// per ADR 0011's policy — the fixture at <see cref="SaveFormat.CurrentVersion"/> must still load
    /// without this field.</summary>
    [JsonPropertyOrder(12)]
    public long HoldingIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-8 save has no ledger transactions.
    /// Additive-only per ADR 0011's policy, matching <see cref="HoldingIds"/>'s identical reasoning.</summary>
    [JsonPropertyOrder(13)]
    public long LedgerTransactionIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-8-item-6 save has no debt records.
    /// Additive-only per ADR 0011's policy, matching <see cref="LedgerTransactionIds"/>'s identical
    /// reasoning.</summary>
    [JsonPropertyOrder(14)]
    public long DebtRecordIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-8-item-7 save has no standing
    /// contracts. Additive-only per ADR 0011's policy, matching <see cref="LedgerTransactionIds"/>'s
    /// identical reasoning.</summary>
    [JsonPropertyOrder(15)]
    public long StandingContractIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-9-item-3 save has no event
    /// instances. Additive-only per ADR 0011's policy, matching <see cref="StandingContractIds"/>'s
    /// identical reasoning.</summary>
    [JsonPropertyOrder(16)]
    public long EventInstanceIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-10 save has no stewardship
    /// assignments. Additive-only per ADR 0011's policy, matching <see cref="EventInstanceIds"/>'
    /// identical reasoning.</summary>
    [JsonPropertyOrder(17)]
    public long StewardshipAssignmentIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-10 save has no autonomous decision
    /// log entries. Additive-only per ADR 0011's policy, matching <see cref="StewardshipAssignmentIds"/>'
    /// identical reasoning.</summary>
    [JsonPropertyOrder(18)]
    public long AutonomousDecisionLogIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-10-item-6 save has no Schemes.
    /// Additive-only per ADR 0011's policy, matching <see cref="AutonomousDecisionLogIds"/>'s identical
    /// reasoning.</summary>
    [JsonPropertyOrder(19)]
    public long SchemeIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-10-package-13 save has no Return
    /// Reports. Additive-only per ADR 0011's policy, matching <see cref="SchemeIds"/>'s identical
    /// reasoning.</summary>
    [JsonPropertyOrder(20)]
    public long ReturnReportIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-11-item-1 save has no succession
    /// disputes. Additive-only per ADR 0011's policy, matching <see cref="ReturnReportIds"/>'s identical
    /// reasoning.</summary>
    [JsonPropertyOrder(21)]
    public long SuccessionDisputeIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-11-item-3 save has no Chronicle
    /// entries. Additive-only per ADR 0011's policy, matching <see cref="SuccessionDisputeIds"/>'s
    /// identical reasoning.</summary>
    [JsonPropertyOrder(22)]
    public long ChronicleEntryIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-11-item-4 save has no Funeral
    /// Records. Additive-only per ADR 0011's policy, matching <see cref="ChronicleEntryIds"/>'s
    /// identical reasoning.</summary>
    [JsonPropertyOrder(23)]
    public long FuneralRecordIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-11-item-5 save has no Agnomina.
    /// Additive-only per ADR 0011's policy, matching <see cref="FuneralRecordIds"/>'s identical
    /// reasoning.</summary>
    [JsonPropertyOrder(24)]
    public long AgnomenIds { get; init; }

    /// <summary>Not <c>required</c>, and defaults to 0: a pre-Phase-11-item-5 save has no Inherited
    /// Cognomen Decisions. Additive-only per ADR 0011's policy, matching <see cref="AgnomenIds"/>'s
    /// identical reasoning.</summary>
    [JsonPropertyOrder(25)]
    public long InheritedCognomenDecisionIds { get; init; }
}

/// <summary>One <see cref="State.KnowledgeState"/> entry. <see cref="ValueJson"/> holds the fact's
/// value as a pre-serialized JSON string: <see cref="State.KnowledgeEntry.Value"/> is <c>object</c>
/// (Phase 2's storage-only placeholder, per that type's own doc comment — no system has emitted a real
/// payload shape yet), so this DTO round-trips whatever JSON shape a future writer produces without
/// needing to know its type ahead of time.</summary>
public sealed record KnowledgeEntryDto
{
    [JsonPropertyOrder(0)]
    public required string ObserverId { get; init; }

    [JsonPropertyOrder(1)]
    public required string SubjectId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Topic { get; init; }

    [JsonPropertyOrder(3)]
    public required string ValueJson { get; init; }

    [JsonPropertyOrder(4)]
    public required string Confidence { get; init; }

    [JsonPropertyOrder(5)]
    public required int AsOfDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public string? ProvenanceEventId { get; init; }
}

/// <summary>One <see cref="State.ScheduledActionEntry"/>, in the same tagged-string-ID convention as
/// every other cross-referenced runtime ID (ADR 0001).</summary>
public sealed record ScheduledActionEntryDto
{
    [JsonPropertyOrder(0)]
    public required string ActionId { get; init; }

    [JsonPropertyOrder(1)]
    public required int DueDateTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(3)]
    public required string ActionType { get; init; }

    [JsonPropertyOrder(4)]
    public required string PayloadJson { get; init; }

    [JsonPropertyOrder(5)]
    public string? CausationId { get; init; }
}

/// <summary>One <see cref="Characters.Character"/>'s full record (Phase 5 items 1-2), in the same
/// declared-field order the record itself uses.</summary>
public sealed record CharacterDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Praenomen { get; init; }

    [JsonPropertyOrder(2)]
    public required string Nomen { get; init; }

    [JsonPropertyOrder(3)]
    public string? Cognomen { get; init; }

    [JsonPropertyOrder(4)]
    public required string Sex { get; init; }

    [JsonPropertyOrder(5)]
    public required int BirthDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public required CharacterVisualProfileDto VisualProfile { get; init; }

    [JsonPropertyOrder(7)]
    public required string LegalStatus { get; init; }

    [JsonPropertyOrder(8)]
    public string? SocialClass { get; init; }

    [JsonPropertyOrder(9)]
    public required string Culture { get; init; }

    [JsonPropertyOrder(10)]
    public required string Location { get; init; }

    [JsonPropertyOrder(11)]
    public string? Household { get; init; }

    [JsonPropertyOrder(12)]
    public required CoreAttributesDto Attributes { get; init; }

    [JsonPropertyOrder(13)]
    public required LaborSkillsDto Skills { get; init; }

    [JsonPropertyOrder(14)]
    public required ConditionDto Condition { get; init; }

    [JsonPropertyOrder(15)]
    public required string Source { get; init; }

    [JsonPropertyOrder(16)]
    public required int InstantiatedAtMonth { get; init; }

    /// <summary>Every field below is Phase 5 item 3 (lifecycle transitions, birth, marriage,
    /// legitimacy, death), added after the Phase 5 items 1-2 fields above. Not <c>required</c>, and
    /// each defaults to "no parents / legitimate / no history / alive" — the same additive-only
    /// reasoning as <see cref="WorldSaveDocument.Characters"/>'s own doc comment: a pre-Phase-5-item-3
    /// save's Characters were never born in play and never married or died, so these fields simply
    /// didn't exist for them, and default to the values that describe exactly that.</summary>
    [JsonPropertyOrder(17)]
    public string? MotherId { get; init; }

    [JsonPropertyOrder(18)]
    public string? FatherId { get; init; }

    [JsonPropertyOrder(19)]
    public string Legitimacy { get; init; } = nameof(Characters.Legitimacy.Legitimate);

    [JsonPropertyOrder(20)]
    public IReadOnlyList<MarriageRecordDto> MaritalHistory { get; init; } = Array.Empty<MarriageRecordDto>();

    [JsonPropertyOrder(21)]
    public IReadOnlyList<PermanentInjuryDto> PermanentInjuries { get; init; } = Array.Empty<PermanentInjuryDto>();

    [JsonPropertyOrder(22)]
    public DeathRecordDto? DeathRecord { get; init; }

    /// <summary>Phase 5 item 4 — Trait <c>DefinitionId</c> references only (rule 10: "content is data,
    /// rules are code"). Not <c>required</c>, defaulting to empty, for the same additive-only reason
    /// as every other post-items-1-2 field above: a pre-Phase-5-item-4 save's Characters never had
    /// traits assigned.</summary>
    [JsonPropertyOrder(23)]
    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();

    /// <summary>Phase 5 item 6 (household roles &amp; duty assignments). Not <c>required</c>, defaulting
    /// to <c>null</c> ("no duty held"), for the same additive-only reason as every other post-items-1-2
    /// field above: a pre-Phase-5-item-6 save's Characters never had a duty slot assigned.</summary>
    [JsonPropertyOrder(24)]
    public DutyAssignmentDto? Duty { get; init; }

    /// <summary>Phase 5 item 7 (lazy instantiation/promotion). Not <c>required</c>, defaulting to
    /// <c>false</c> ("raised inside the simulation, not backfilled"), for the same additive-only
    /// reason as every other post-items-1-2 field above: a pre-Phase-5-item-7 save's Characters were
    /// only ever born in play.</summary>
    [JsonPropertyOrder(25)]
    public bool BackfilledHistory { get; init; }

    /// <summary>Every field below is Phase 6 item 6 (Labor &amp; Slavery: Regimen, flight, manumission).
    /// Not <c>required</c>, and each defaults to "no override / never fled / no plan" for the same
    /// additive-only reason as every other post-items-1-2 field above: a pre-Phase-6-item-6 save's
    /// Characters never had any of these set.</summary>
    [JsonPropertyOrder(26)]
    public RegimenSettingsDto? Regimen { get; init; }

    [JsonPropertyOrder(27)]
    public FledRecordDto? Flight { get; init; }

    [JsonPropertyOrder(28)]
    public PursuitRecordDto? Pursuit { get; init; }

    [JsonPropertyOrder(29)]
    public ManumissionPlanDto? ManumissionPlan { get; init; }
}

/// <summary>One <see cref="Characters.RegimenSettings"/> (<c>gens-labor-slavery-design.md</c> §5).</summary>
public sealed record RegimenSettingsDto
{
    [JsonPropertyOrder(0)]
    public required string Diet { get; init; }

    [JsonPropertyOrder(1)]
    public required string Accommodation { get; init; }

    [JsonPropertyOrder(2)]
    public required string Freedoms { get; init; }

    [JsonPropertyOrder(3)]
    public required string Discipline { get; init; }
}

/// <summary>One <see cref="Characters.FledRecord"/> (<c>gens-labor-slavery-design.md</c> §7).</summary>
public sealed record FledRecordDto
{
    [JsonPropertyOrder(0)]
    public required int FledDateTotalMonths { get; init; }

    [JsonPropertyOrder(1)]
    public required string FormerHousehold { get; init; }

    [JsonPropertyOrder(2)]
    public string? LastKnownLocation { get; init; }
}

/// <summary>One <see cref="Characters.PursuitRecord"/> (<c>gens-labor-slavery-design.md</c> §7).</summary>
public sealed record PursuitRecordDto
{
    [JsonPropertyOrder(0)]
    public required int MonthsRemaining { get; init; }

    [JsonPropertyOrder(1)]
    public string? PursuerId { get; init; }
}

/// <summary>One <see cref="Characters.ManumissionPlan"/> (<c>gens-labor-slavery-design.md</c> §8).</summary>
public sealed record ManumissionPlanDto
{
    [JsonPropertyOrder(0)]
    public required string GrantorId { get; init; }

    [JsonPropertyOrder(1)]
    public required string Type { get; init; }
}

/// <summary>One household-level Regimen default (Phase 6 item 6), keyed by its <see
/// cref="Characters.HouseholdRegimenKey"/>'s (household, duty slot or <c>null</c>) pair.</summary>
public sealed record HouseholdRegimenDefaultDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public string? Slot { get; init; }

    [JsonPropertyOrder(2)]
    public required RegimenSettingsDto Regimen { get; init; }
}

/// <summary>One <see cref="Characters.PopGroup"/> (Phase 5 item 7; ADR 0009), keyed by its <see
/// cref="Characters.PopGroupKey"/>'s (settlement, group type) pair.</summary>
public sealed record PopGroupDto
{
    [JsonPropertyOrder(0)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(1)]
    public required string GroupType { get; init; }

    [JsonPropertyOrder(2)]
    public required int Size { get; init; }

    [JsonPropertyOrder(3)]
    public required int CitizenCount { get; init; }

    [JsonPropertyOrder(4)]
    public required int LatinRightsCount { get; init; }

    [JsonPropertyOrder(5)]
    public required int PeregrineCount { get; init; }

    [JsonPropertyOrder(6)]
    public required int FreedmanCount { get; init; }

    [JsonPropertyOrder(7)]
    public required string Culture { get; init; }

    [JsonPropertyOrder(8)]
    public required string WealthBand { get; init; }

    [JsonPropertyOrder(9)]
    public required string NeedsProfile { get; init; }

    [JsonPropertyOrder(10)]
    public required Fixed64 EmploymentRatio { get; init; }

    [JsonPropertyOrder(11)]
    public required Fixed64 HousingSatisfaction { get; init; }

    [JsonPropertyOrder(12)]
    public required Fixed64 Contentment { get; init; }

    [JsonPropertyOrder(13)]
    public required Fixed64 HealthExposure { get; init; }
}

/// <summary>The DTO shape of a <see cref="Characters.DutyAssignment"/>.</summary>
public sealed record DutyAssignmentDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string Slot { get; init; }

    [JsonPropertyOrder(2)]
    public required int AssignedDateTotalMonths { get; init; }
}

/// <summary>One directed <see cref="Characters.Relationship"/> (Phase 5 item 5;
/// <c>gens-characters-design.md</c> §7), keyed by its <see cref="Characters.RelationshipKey"/>'s
/// (From, To) pair.</summary>
public sealed record RelationshipDto
{
    [JsonPropertyOrder(0)]
    public required string From { get; init; }

    [JsonPropertyOrder(1)]
    public required string To { get; init; }

    [JsonPropertyOrder(2)]
    public required int Opinion { get; init; }

    /// <summary>Every held <see cref="Characters.BondTag"/> flag, by name.</summary>
    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> Bonds { get; init; }

    [JsonPropertyOrder(4)]
    public required string Origin { get; init; }

    [JsonPropertyOrder(5)]
    public required int FormedDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public required int LastMeaningfulInteractionDateTotalMonths { get; init; }

    [JsonPropertyOrder(7)]
    public string? ProvenanceEventId { get; init; }
}

/// <summary>One <see cref="Characters.MarriageRecord"/> (<c>gens-familia-design.md</c> §5, §5.1).</summary>
public sealed record MarriageRecordDto
{
    [JsonPropertyOrder(0)]
    public required string SpouseId { get; init; }

    [JsonPropertyOrder(1)]
    public required int StartDateTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public int? EndDateTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public string? EndReason { get; init; }
}

/// <summary>One <see cref="Characters.PermanentInjury"/> (<c>gens-familia-design.md</c> §3.1).</summary>
public sealed record PermanentInjuryDto
{
    [JsonPropertyOrder(0)]
    public required string Target { get; init; }

    [JsonPropertyOrder(1)]
    public required int Magnitude { get; init; }

    [JsonPropertyOrder(2)]
    public required string Cause { get; init; }

    [JsonPropertyOrder(3)]
    public required int InflictedDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Characters.DeathRecord"/> (<c>gens-familia-design.md</c> §3).</summary>
public sealed record DeathRecordDto
{
    [JsonPropertyOrder(0)]
    public required int DateTotalMonths { get; init; }

    [JsonPropertyOrder(1)]
    public required string Cause { get; init; }

    [JsonPropertyOrder(2)]
    public required int AgeAtDeath { get; init; }
}

/// <summary>One <see cref="Characters.CharacterVisualProfile"/> (<c>gens-familia-design.md</c> §2.4 /
/// Core §7.11; Phase 5 item 2).</summary>
public sealed record CharacterVisualProfileDto
{
    [JsonPropertyOrder(0)]
    public required string Height { get; init; }

    [JsonPropertyOrder(1)]
    public required string Build { get; init; }

    [JsonPropertyOrder(2)]
    public required string FacialStructure { get; init; }

    [JsonPropertyOrder(3)]
    public required string Complexion { get; init; }

    [JsonPropertyOrder(4)]
    public required string HairColor { get; init; }

    [JsonPropertyOrder(5)]
    public required string HairStyle { get; init; }

    [JsonPropertyOrder(6)]
    public required string EyeColor { get; init; }

    /// <summary>Already in the generator's own fixed canonical order (<see
    /// cref="Characters.PortraitRecipeGenerator"/>'s <c>FeatureLayerOrder</c>) — never re-sorted here.</summary>
    [JsonPropertyOrder(7)]
    public required IReadOnlyList<string> NotableFeatures { get; init; }

    [JsonPropertyOrder(8)]
    public required PortraitRecipeDto Portrait { get; init; }
}

/// <summary>One <see cref="Characters.PortraitRecipe"/>.</summary>
public sealed record PortraitRecipeDto
{
    [JsonPropertyOrder(0)]
    public required IReadOnlyList<string> Layers { get; init; }
}

/// <summary>One <see cref="Characters.CoreAttributes"/> (<c>gens-familia-design.md</c> §2.1).</summary>
public sealed record CoreAttributesDto
{
    [JsonPropertyOrder(0)]
    public required int Diplomacy { get; init; }

    [JsonPropertyOrder(1)]
    public required int Martial { get; init; }

    [JsonPropertyOrder(2)]
    public required int Stewardship { get; init; }

    [JsonPropertyOrder(3)]
    public required int Intrigue { get; init; }

    [JsonPropertyOrder(4)]
    public required int Learning { get; init; }
}

/// <summary>One <see cref="Characters.LaborSkills"/> (<c>gens-familia-design.md</c> §2.2).</summary>
public sealed record LaborSkillsDto
{
    [JsonPropertyOrder(0)]
    public required int Fieldwork { get; init; }

    [JsonPropertyOrder(1)]
    public required int DomesticService { get; init; }

    [JsonPropertyOrder(2)]
    public required int Craft { get; init; }

    [JsonPropertyOrder(3)]
    public required int Culinary { get; init; }

    [JsonPropertyOrder(4)]
    public required int Medicine { get; init; }
}

/// <summary>One <see cref="Characters.Condition"/> (<c>gens-familia-design.md</c> §2.3).</summary>
public sealed record ConditionDto
{
    [JsonPropertyOrder(0)]
    public required int Health { get; init; }

    [JsonPropertyOrder(1)]
    public required int Fatigue { get; init; }

    [JsonPropertyOrder(2)]
    public required int Loyalty { get; init; }

    [JsonPropertyOrder(3)]
    public required int Ambition { get; init; }

    [JsonPropertyOrder(4)]
    public required int Fertility { get; init; }
}

/// <summary>One <see cref="Land.Region"/> boundary record (Phase 6 item 1).</summary>
public sealed record RegionDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Name { get; init; }
}

/// <summary>One <see cref="Land.Settlement"/> boundary record (Phase 6 item 1).</summary>
public sealed record SettlementDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string RegionId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Stage { get; init; }
}

/// <summary>One <see cref="Land.Plot"/> boundary record (Phase 6 item 1).</summary>
public sealed record PlotDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(2)]
    public string Terrain { get; init; } = "FertilePlain";

    [JsonPropertyOrder(3)]
    public int Features { get; init; }

    [JsonPropertyOrder(4)]
    public int Condition { get; init; } = 100;

    [JsonPropertyOrder(5)]
    public int Capacity { get; init; } = 1;

    [JsonPropertyOrder(6)]
    public string? OwnerId { get; init; }

    [JsonPropertyOrder(7)]
    public string? OccupyingHoldingId { get; init; }

    [JsonPropertyOrder(8)]
    public bool IsContested { get; init; }

    [JsonPropertyOrder(9)]
    public string? AcquisitionMethod { get; init; }

    [JsonPropertyOrder(10)]
    public int? AcquiredDateTotalMonths { get; init; }

    [JsonPropertyOrder(11)]
    public string? AcquisitionSourceId { get; init; }
}

/// <summary>One <see cref="Land.Holding"/> boundary record (Phase 6 item 1).</summary>
public sealed record HoldingDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(2)]
    public string? OwnerId { get; init; }

    [JsonPropertyOrder(3)]
    public string? OccupantId { get; init; }

    [JsonPropertyOrder(4)]
    public int ResidentCapacity { get; init; } = 1;

    [JsonPropertyOrder(5)]
    public VillaDto? Villa { get; init; }
}

public sealed record VillaDto
{
    [JsonPropertyOrder(0)]
    public required string Stage { get; init; }

    [JsonPropertyOrder(1)]
    public bool IsOutpost { get; init; }

    [JsonPropertyOrder(2)]
    public IReadOnlyList<VillaRoomDto> Rooms { get; init; } = Array.Empty<VillaRoomDto>();
}

public sealed record VillaRoomDto
{
    [JsonPropertyOrder(0)]
    public required string Key { get; init; }

    [JsonPropertyOrder(1)]
    public required string DefinitionKey { get; init; }

    [JsonPropertyOrder(2)]
    public required string MinimumStage { get; init; }

    [JsonPropertyOrder(3)]
    public int MaximumTier { get; init; }

    [JsonPropertyOrder(4)]
    public bool UsesRoomSlot { get; init; }

    [JsonPropertyOrder(5)]
    public int Tier { get; init; }

    [JsonPropertyOrder(6)]
    public required string CapacityTier { get; init; }

    [JsonPropertyOrder(7)]
    public required string Condition { get; init; }

    [JsonPropertyOrder(8)]
    public string? AssignedTo { get; init; }
}

/// <summary>One <see cref="Buildings.RecipeLine"/> — a positive quantity of a good.</summary>
public sealed record RecipeLineDto
{
    [JsonPropertyOrder(0)]
    public required string GoodId { get; init; }

    [JsonPropertyOrder(1)]
    public required long Quantity { get; init; }
}

/// <summary>One <see cref="Buildings.StaffingSlot"/>.</summary>
public sealed record StaffingSlotDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required int Capacity { get; init; }

    [JsonPropertyOrder(2)]
    public bool RequiredForProduction { get; init; } = true;
}

/// <summary>One <see cref="Buildings.ProductionRecipe"/>.</summary>
public sealed record ProductionRecipeDto
{
    [JsonPropertyOrder(0)]
    public required IReadOnlyList<RecipeLineDto> Inputs { get; init; }

    [JsonPropertyOrder(1)]
    public required IReadOnlyList<RecipeLineDto> Outputs { get; init; }
}

/// <summary>One <see cref="Buildings.BuildingDefinition"/>, embedded inline rather than referenced by
/// ID — no content catalog resolves a <see cref="Identity.DefinitionId{T}"/> back to a
/// <c>BuildingDefinition</c> at runtime yet (Phase 6 item 7's scope is the production network, not a
/// content-loading pipeline), so every runtime <see cref="Buildings.BuildingInstance"/> and <see
/// cref="Buildings.ConstructionProject"/> already carries its full definition by reference in memory;
/// this DTO round-trips that same shape rather than inventing a lookup this project doesn't have yet.</summary>
public sealed record BuildingDefinitionDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Tier { get; init; }

    [JsonPropertyOrder(2)]
    public required int ConstructionMonths { get; init; }

    [JsonPropertyOrder(3)]
    public required int PlotCapacity { get; init; }

    [JsonPropertyOrder(4)]
    public IReadOnlyList<string> Prerequisites { get; init; } = Array.Empty<string>();

    [JsonPropertyOrder(5)]
    public IReadOnlyList<string> AllowedTerrain { get; init; } = Array.Empty<string>();

    [JsonPropertyOrder(6)]
    public int RequiredFeatures { get; init; }

    [JsonPropertyOrder(7)]
    public IReadOnlyList<RecipeLineDto> Upkeep { get; init; } = Array.Empty<RecipeLineDto>();

    [JsonPropertyOrder(8)]
    public IReadOnlyList<StaffingSlotDto> StaffingSlots { get; init; } = Array.Empty<StaffingSlotDto>();

    [JsonPropertyOrder(9)]
    public ProductionRecipeDto? Recipe { get; init; }
}

/// <summary>One Building's staffing slot assignment (<see cref="Buildings.BuildingInstance.Staff"/>).</summary>
public sealed record StaffSlotAssignmentDto
{
    [JsonPropertyOrder(0)]
    public required string SlotId { get; init; }

    [JsonPropertyOrder(1)]
    public required IReadOnlyList<string> WorkerIds { get; init; }
}

/// <summary>One <see cref="Buildings.BuildingInstance"/> (Phase 6 item 7).</summary>
public sealed record BuildingInstanceDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string PlotId { get; init; }

    [JsonPropertyOrder(2)]
    public required BuildingDefinitionDto Definition { get; init; }

    [JsonPropertyOrder(3)]
    public required string Condition { get; init; }

    [JsonPropertyOrder(4)]
    public IReadOnlyList<StaffSlotAssignmentDto> Staff { get; init; } = Array.Empty<StaffSlotAssignmentDto>();
}

/// <summary>One <see cref="Buildings.ConstructionProject"/>.</summary>
public sealed record ConstructionProjectDto
{
    [JsonPropertyOrder(0)]
    public required long Sequence { get; init; }

    [JsonPropertyOrder(1)]
    public required string PlotId { get; init; }

    [JsonPropertyOrder(2)]
    public required BuildingDefinitionDto Definition { get; init; }

    [JsonPropertyOrder(3)]
    public required int CompletedMonths { get; init; }
}

/// <summary>One Holding's <see cref="Buildings.ConstructionSchedule"/> (Phase 6 item 7's "one
/// construction queue").</summary>
public sealed record ConstructionScheduleDto
{
    [JsonPropertyOrder(0)]
    public required string HoldingId { get; init; }

    [JsonPropertyOrder(1)]
    public required long NextSequence { get; init; }

    [JsonPropertyOrder(2)]
    public IReadOnlyList<ConstructionProjectDto> Projects { get; init; } = Array.Empty<ConstructionProjectDto>();
}

/// <summary>One <see cref="Goods.GoodDefinition"/>, embedded inline for the same reason as <see
/// cref="BuildingDefinitionDto"/>: no content catalog resolves a Good <see
/// cref="Identity.DefinitionId{T}"/> back to its storage-relevant properties at runtime yet.</summary>
public sealed record GoodDefinitionDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Perishability { get; init; }

    [JsonPropertyOrder(2)]
    public bool QualityEligible { get; init; }

    [JsonPropertyOrder(3)]
    public bool ConditionTracked { get; init; }

    [JsonPropertyOrder(4)]
    public int? ShelfLifeTicks { get; init; }
}

/// <summary>One optional <see cref="Goods.StockProvenance"/> on a batch.</summary>
public sealed record StockProvenanceDto
{
    [JsonPropertyOrder(0)]
    public required string SourceId { get; init; }

    [JsonPropertyOrder(1)]
    public string? EventId { get; init; }

    [JsonPropertyOrder(2)]
    public string? ExceptionalObjectId { get; init; }
}

/// <summary>One <see cref="Goods.StockLot"/> batch, in the Stockpile's original list order — restore
/// re-adds lots in this exact order because <see cref="Goods.Stockpile"/>'s FEFO/FIFO removal breaks
/// equal-age ties by insertion order (that type's own doc comment).</summary>
public sealed record StockLotDto
{
    [JsonPropertyOrder(0)]
    public required GoodDefinitionDto Good { get; init; }

    [JsonPropertyOrder(1)]
    public required long Quantity { get; init; }

    [JsonPropertyOrder(2)]
    public string? Quality { get; init; }

    [JsonPropertyOrder(3)]
    public int? Condition { get; init; }

    [JsonPropertyOrder(4)]
    public required int AgeInTicks { get; init; }

    [JsonPropertyOrder(5)]
    public StockProvenanceDto? Provenance { get; init; }
}

/// <summary>One <see cref="Goods.StockReservation"/>, in ordinal <c>ReservationId</c> order.</summary>
public sealed record StockReservationDto
{
    [JsonPropertyOrder(0)]
    public required string ReservationId { get; init; }

    [JsonPropertyOrder(1)]
    public required string GoodId { get; init; }

    [JsonPropertyOrder(2)]
    public string? Quality { get; init; }

    [JsonPropertyOrder(3)]
    public required long Quantity { get; init; }
}

/// <summary>One Holding's <see cref="Goods.Stockpile"/> (Phase 6 items 3/7's "one storage
/// constraint").</summary>
public sealed record StockpileDto
{
    [JsonPropertyOrder(0)]
    public required string HoldingId { get; init; }

    [JsonPropertyOrder(1)]
    public required long Capacity { get; init; }

    [JsonPropertyOrder(2)]
    public IReadOnlyList<StockLotDto> Lots { get; init; } = Array.Empty<StockLotDto>();

    [JsonPropertyOrder(3)]
    public IReadOnlyList<StockReservationDto> Reservations { get; init; } = Array.Empty<StockReservationDto>();
}

/// <summary>One <see cref="Ledger.LedgerAccount"/> (Phase 8 items 1-2).</summary>
public sealed record LedgerAccountDto
{
    [JsonPropertyOrder(0)]
    public required string Kind { get; init; }

    [JsonPropertyOrder(1)]
    public required string OwnerId { get; init; }

    [JsonPropertyOrder(2)]
    public required Money Balance { get; init; }
}

/// <summary>One <see cref="Ledger.LedgerPosting"/>, part of <see cref="LedgerTransactionDto"/>.</summary>
public sealed record LedgerPostingDto
{
    [JsonPropertyOrder(0)]
    public required string Kind { get; init; }

    [JsonPropertyOrder(1)]
    public required string OwnerId { get; init; }

    [JsonPropertyOrder(2)]
    public required Money Amount { get; init; }
}

/// <summary>One posted <see cref="Ledger.LedgerTransaction"/> (Phase 8 item 1).</summary>
public sealed record LedgerTransactionDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required int OccurredDateTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required string Category { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<LedgerPostingDto> Postings { get; init; }

    [JsonPropertyOrder(4)]
    public string? Reference { get; init; }
}

/// <summary>One cleared <see cref="Markets.SettlementMarket"/> (Phase 8 items 3-4).</summary>
public sealed record SettlementMarketDto
{
    [JsonPropertyOrder(0)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(1)]
    public required string GoodId { get; init; }

    [JsonPropertyOrder(2)]
    public required Money Price { get; init; }

    [JsonPropertyOrder(3)]
    public required Money PreviousPrice { get; init; }

    [JsonPropertyOrder(4)]
    public required long Supply { get; init; }

    [JsonPropertyOrder(5)]
    public required long Demand { get; init; }

    [JsonPropertyOrder(6)]
    public required long ClearedQuantity { get; init; }

    [JsonPropertyOrder(7)]
    public required long UnsatisfiedDemand { get; init; }
}

/// <summary>One household's <see cref="Economy.HouseholdMonthlyStatement"/> (Phase 8 item 5).</summary>
public sealed record HouseholdMonthlyStatementDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required int MonthTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required Money Income { get; init; }

    [JsonPropertyOrder(3)]
    public required Money Expenses { get; init; }

    [JsonPropertyOrder(4)]
    public required Money Net { get; init; }
}

/// <summary>One <see cref="Economy.DebtRecord"/> (Phase 8 item 6).</summary>
public sealed record DebtRecordDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(2)]
    public required string DebtorHouseholdId { get; init; }

    [JsonPropertyOrder(3)]
    public required Money Principal { get; init; }

    [JsonPropertyOrder(4)]
    public required Fixed64 InterestRate { get; init; }

    [JsonPropertyOrder(5)]
    public required string Origin { get; init; }

    [JsonPropertyOrder(6)]
    public required bool IsFenusNauticum { get; init; }

    [JsonPropertyOrder(7)]
    public required int MonthsOverdue { get; init; }

    [JsonPropertyOrder(8)]
    public required string Status { get; init; }

    [JsonPropertyOrder(9)]
    public required string Resolution { get; init; }
}

/// <summary>One household's <see cref="Economy.NetWorth"/> assessment (Phase 8 item 6; §8).</summary>
public sealed record NetWorthDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required int MonthTotalMonths { get; init; }

    [JsonPropertyOrder(2)]
    public required Money TreasuryBalance { get; init; }

    [JsonPropertyOrder(3)]
    public required Money StoredGoodsValue { get; init; }

    [JsonPropertyOrder(4)]
    public required Money OutstandingDebt { get; init; }

    [JsonPropertyOrder(5)]
    public required Money Total { get; init; }
}

/// <summary>One household's <see cref="Economy.InsolvencyState"/> (Phase 8 item 6; §9).</summary>
public sealed record InsolvencyStateDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required int MonthsBelowThreshold { get; init; }

    [JsonPropertyOrder(2)]
    public required string Stage { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> ConsequencesApplied { get; init; }
}

/// <summary>One <see cref="Economy.StandingContract"/> (Phase 8 item 7).</summary>
public sealed record StandingContractDto
{
    [JsonPropertyOrder(0)]
    public required string Id { get; init; }

    [JsonPropertyOrder(1)]
    public required string Kind { get; init; }

    [JsonPropertyOrder(2)]
    public required string SettlementId { get; init; }

    [JsonPropertyOrder(3)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(4)]
    public required string Status { get; init; }

    [JsonPropertyOrder(5)]
    public string? HoldingId { get; init; }

    [JsonPropertyOrder(6)]
    public string? GoodId { get; init; }

    [JsonPropertyOrder(7)]
    public required long QuantityPerMonth { get; init; }

    [JsonPropertyOrder(8)]
    public required Fixed64 PriceOverMarketFraction { get; init; }

    [JsonPropertyOrder(9)]
    public required Money DenariiCommitted { get; init; }

    [JsonPropertyOrder(10)]
    public string? RouteName { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Policies.HouseholdPolicyState"/> (Phase 9 item 2), keyed by
/// household.</summary>
public sealed record HouseholdPolicyStateDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string RitesBudget { get; init; }

    [JsonPropertyOrder(2)]
    public required int LastChangedDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Events.EventInstance"/> (Phase 9 item 3).</summary>
public sealed record EventInstanceDto
{
    [JsonPropertyOrder(0)]
    public required string InstanceId { get; init; }

    [JsonPropertyOrder(1)]
    public required string DefinitionId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Scope { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> SubjectIds { get; init; }

    [JsonPropertyOrder(4)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(5)]
    public required int CurrentStageIndex { get; init; }

    [JsonPropertyOrder(6)]
    public required int FiredDateTotalMonths { get; init; }

    [JsonPropertyOrder(7)]
    public required int ExpiresDateTotalMonths { get; init; }

    [JsonPropertyOrder(8)]
    public required string Status { get; init; }

    [JsonPropertyOrder(9)]
    public string? ResolvedOptionId { get; init; }

    [JsonPropertyOrder(10)]
    public int? ResolvedDateTotalMonths { get; init; }

    [JsonPropertyOrder(11)]
    public string? ResolvingEventId { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Actors.LivingWorldActor"/> (Phase 10 item 3).</summary>
public sealed record LivingWorldActorDto
{
    [JsonPropertyOrder(0)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(1)]
    public required string ActorType { get; init; }

    [JsonPropertyOrder(2)]
    public required string Name { get; init; }

    [JsonPropertyOrder(3)]
    public required string Tier { get; init; }

    [JsonPropertyOrder(4)]
    public required string StandingTrend { get; init; }

    [JsonPropertyOrder(5)]
    public required string OriginStory { get; init; }

    [JsonPropertyOrder(6)]
    public string? ParentActorId { get; init; }

    [JsonPropertyOrder(7)]
    public string? IdentityEconomic { get; init; }

    [JsonPropertyOrder(8)]
    public string? IdentityFaction { get; init; }

    [JsonPropertyOrder(9)]
    public string? HeadCharacterId { get; init; }

    [JsonPropertyOrder(10)]
    public required int Dignitas { get; init; }

    [JsonPropertyOrder(11)]
    public required string NetWorthBand { get; init; }

    [JsonPropertyOrder(12)]
    public long? NetWorthFigure { get; init; }

    [JsonPropertyOrder(13)]
    public required string MilitaryStrengthBand { get; init; }

    [JsonPropertyOrder(14)]
    public string? MilitaryStrengthResolvedForceId { get; init; }

    [JsonPropertyOrder(15)]
    public required string RegionId { get; init; }

    [JsonPropertyOrder(16)]
    public required string HomeSettlementId { get; init; }

    [JsonPropertyOrder(17)]
    public int? LastContactDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Actors.HouseStanding"/> entry (Phase 10 item 5).</summary>
public sealed record HouseStandingDto
{
    [JsonPropertyOrder(0)]
    public required string ActorAId { get; init; }

    [JsonPropertyOrder(1)]
    public required string ActorBId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Standing { get; init; }

    [JsonPropertyOrder(3)]
    public string? GrudgeOriginEngagementId { get; init; }

    [JsonPropertyOrder(4)]
    public int? GrudgeOriginDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Actors.RivalDossier"/> (Phase 10 item 5).</summary>
public sealed record RivalDossierDto
{
    [JsonPropertyOrder(0)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(1)]
    public required string Summary { get; init; }

    [JsonPropertyOrder(2)]
    public string? HeadComboTitle { get; init; }

    [JsonPropertyOrder(3)]
    public required int LastUpdatedDateTotalMonths { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<string> RecentChronicleEntries { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Actors.RegionalFamiliesEntry"/> (Phase 10 item 5).</summary>
public sealed record RegionalFamiliesEntryDto
{
    [JsonPropertyOrder(0)]
    public required string ActorId { get; init; }

    [JsonPropertyOrder(1)]
    public required string Name { get; init; }

    [JsonPropertyOrder(2)]
    public required string StandingTrend { get; init; }

    [JsonPropertyOrder(3)]
    public string? IdentityEconomic { get; init; }
}

/// <summary>One filled Council seat, part of <see cref="StewardshipAssignmentDto"/>.</summary>
public sealed record CouncilMemberDto
{
    [JsonPropertyOrder(0)]
    public required string Domain { get; init; }

    [JsonPropertyOrder(1)]
    public required string CharacterId { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Stewardship.StewardshipAssignment"/> (Phase 10 item 2).</summary>
public sealed record StewardshipAssignmentDto
{
    [JsonPropertyOrder(0)]
    public required string AssignmentId { get; init; }

    [JsonPropertyOrder(1)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Context { get; init; }

    [JsonPropertyOrder(3)]
    public required string Mode { get; init; }

    [JsonPropertyOrder(4)]
    public string? AppointeeCharacterId { get; init; }

    [JsonPropertyOrder(5)]
    public required IReadOnlyList<CouncilMemberDto> CouncilMembers { get; init; }

    [JsonPropertyOrder(6)]
    public string? CouncilHeadCharacterId { get; init; }

    [JsonPropertyOrder(7)]
    public required string AutonomyLevel { get; init; }

    [JsonPropertyOrder(8)]
    public required int StartDateTotalMonths { get; init; }

    [JsonPropertyOrder(9)]
    public int? EndDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Stewardship.AutonomousDecisionLog"/> (Phase 10 item 10).</summary>
public sealed record AutonomousDecisionLogDto
{
    [JsonPropertyOrder(0)]
    public required string LogId { get; init; }

    [JsonPropertyOrder(1)]
    public required string AssignmentId { get; init; }

    [JsonPropertyOrder(2)]
    public required int MonthTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public required string DecisionType { get; init; }

    [JsonPropertyOrder(4)]
    public required string Outcome { get; init; }

    [JsonPropertyOrder(5)]
    public required int CompetenceRollFactor { get; init; }

    [JsonPropertyOrder(6)]
    public required int LoyaltyRiskRollFactor { get; init; }

    [JsonPropertyOrder(7)]
    public string? IncidentType { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Interactions.Scheme"/> (Phase 10 item 6).</summary>
public sealed record SchemeDto
{
    [JsonPropertyOrder(0)]
    public required string SchemeId { get; init; }

    [JsonPropertyOrder(1)]
    public required string InitiatorCharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required string TargetCharacterId { get; init; }

    [JsonPropertyOrder(3)]
    public required string Type { get; init; }

    [JsonPropertyOrder(4)]
    public required string Status { get; init; }

    [JsonPropertyOrder(5)]
    public required int Progress { get; init; }

    [JsonPropertyOrder(6)]
    public required int DiscoveryRisk { get; init; }

    [JsonPropertyOrder(7)]
    public required int InitiatedDateTotalMonths { get; init; }

    [JsonPropertyOrder(8)]
    public required int LastProgressedDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Stewardship.ReturnReport"/> (Phase 10 package 13).</summary>
public sealed record ReturnReportDto
{
    [JsonPropertyOrder(0)]
    public required string ReportId { get; init; }

    [JsonPropertyOrder(1)]
    public required string AssignmentId { get; init; }

    [JsonPropertyOrder(2)]
    public required IReadOnlyList<string> SummaryEntries { get; init; }

    [JsonPropertyOrder(3)]
    public required long TotalTreasuryImpactRawValue { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<string> IncidentsDiscovered { get; init; }

    [JsonPropertyOrder(5)]
    public required bool ChronicleWorthy { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Succession.HouseholdHeadship"/> (Phase 11 item 1).</summary>
public sealed record HouseholdHeadshipDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string HeadCharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required int SinceDateTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public string? RegentCharacterId { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Succession.HeirDesignation"/> (Phase 11 item 1).</summary>
public sealed record HeirDesignationDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public string? PreferredHeirId { get; init; }

    [JsonPropertyOrder(2)]
    public string? FormallyDeclaredHeirId { get; init; }

    [JsonPropertyOrder(3)]
    public int? DeclaredDateTotalMonths { get; init; }

    [JsonPropertyOrder(4)]
    public required IReadOnlyList<string> DisownedCharacterIds { get; init; }

    [JsonPropertyOrder(5)]
    public required IReadOnlyList<string> AdoptedChildIds { get; init; }

    [JsonPropertyOrder(6)]
    public required IReadOnlyList<string> AcknowledgedIllegitimateChildIds { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Succession.SuccessionDispute"/> (Phase 11 item 1).</summary>
public sealed record SuccessionDisputeDto
{
    [JsonPropertyOrder(0)]
    public required string DisputeId { get; init; }

    [JsonPropertyOrder(1)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(2)]
    public required string DeceasedHeadId { get; init; }

    [JsonPropertyOrder(3)]
    public required IReadOnlyList<string> ClaimantIds { get; init; }

    [JsonPropertyOrder(4)]
    public required int OpenedDateTotalMonths { get; init; }

    [JsonPropertyOrder(5)]
    public required int ResolutionDueDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public required string Status { get; init; }

    [JsonPropertyOrder(7)]
    public string? WinnerCharacterId { get; init; }

    [JsonPropertyOrder(8)]
    public string? SplinterClaimantId { get; init; }

    [JsonPropertyOrder(9)]
    public string? SplinterHouseholdId { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Succession.PlayerControlState"/> (Phase 11 item 2).</summary>
public sealed record PlayerControlDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public string? ControlledCharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required string Mode { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Chronicle.ChronicleEntry"/> (Phase 11 item 3).</summary>
public sealed record ChronicleEntryDto
{
    [JsonPropertyOrder(0)]
    public required string EntryId { get; init; }

    [JsonPropertyOrder(1)]
    public string? HouseholdId { get; init; }

    [JsonPropertyOrder(2)]
    public required int MonthTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public required string Category { get; init; }

    [JsonPropertyOrder(4)]
    public required string Tier { get; init; }

    [JsonPropertyOrder(5)]
    public required string Prose { get; init; }

    [JsonPropertyOrder(6)]
    public required IReadOnlyList<string> LinkedCharacterIds { get; init; }

    [JsonPropertyOrder(7)]
    public required string SourceSystem { get; init; }

    [JsonPropertyOrder(8)]
    public required string Source { get; init; }

    [JsonPropertyOrder(9)]
    public required bool Pinned { get; init; }

    [JsonPropertyOrder(10)]
    public string? PlayerAnnotation { get; init; }

    [JsonPropertyOrder(11)]
    public string? CrossHouseLinkedEntryId { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Chronicle.GenerationalChapter"/> (Phase 11 item 3).</summary>
public sealed record GenerationalChapterDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string HeadCharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required int StartMonthTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public int? EndMonthTotalMonths { get; init; }

    [JsonPropertyOrder(4)]
    public required string ChapterSummary { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Funerary.FuneralRecord"/> (Phase 11 item 4).</summary>
public sealed record FuneralRecordDto
{
    [JsonPropertyOrder(0)]
    public required string FuneralId { get; init; }

    [JsonPropertyOrder(1)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(2)]
    public required string DeceasedCharacterId { get; init; }

    [JsonPropertyOrder(3)]
    public required int DeathDateTotalMonths { get; init; }

    [JsonPropertyOrder(4)]
    public required string Status { get; init; }

    [JsonPropertyOrder(5)]
    public string? Tier { get; init; }

    [JsonPropertyOrder(6)]
    public string? BurialMethod { get; init; }

    [JsonPropertyOrder(7)]
    public string? InterredAt { get; init; }

    [JsonPropertyOrder(8)]
    public int? HeldDateTotalMonths { get; init; }

    [JsonPropertyOrder(9)]
    public long? CostRawValue { get; init; }

    [JsonPropertyOrder(10)]
    public int? MemoriaGained { get; init; }

    [JsonPropertyOrder(11)]
    public required bool ImaginesDisplayed { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Funerary.MourningPeriod"/> (Phase 11 item 4), keyed by
/// household.</summary>
public sealed record MourningPeriodDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string TriggeringDeathCharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required int StartDateTotalMonths { get; init; }

    [JsonPropertyOrder(3)]
    public required int EndDateTotalMonths { get; init; }

    [JsonPropertyOrder(4)]
    public required bool BrokenEarly { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Funerary.MemoriaState"/> (Phase 11 item 4), keyed by
/// household.</summary>
public sealed record MemoriaStateDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required int Memoria { get; init; }

    [JsonPropertyOrder(2)]
    public int? LastParentaliaObservedDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Epithets.Agnomen"/> (Phase 11 item 5).</summary>
public sealed record AgnomenDto
{
    [JsonPropertyOrder(0)]
    public required string AgnomenId { get; init; }

    [JsonPropertyOrder(1)]
    public required string CharacterId { get; init; }

    [JsonPropertyOrder(2)]
    public required string AgnomenType { get; init; }

    [JsonPropertyOrder(3)]
    public required string Name { get; init; }

    [JsonPropertyOrder(4)]
    public required string GrantMethod { get; init; }

    [JsonPropertyOrder(5)]
    public required int GrantedDateTotalMonths { get; init; }

    [JsonPropertyOrder(6)]
    public required IReadOnlyList<string> SourceChronicleEntryIds { get; init; }

    [JsonPropertyOrder(7)]
    public string? SourceSuccessionDisputeId { get; init; }

    [JsonPropertyOrder(8)]
    public int? DignitasEffect { get; init; }

    [JsonPropertyOrder(9)]
    public int? FameEffect { get; init; }

    [JsonPropertyOrder(10)]
    public required bool IsSuppressible { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Epithets.InheritedCognomenDecision"/> (Phase 11 item 5).</summary>
public sealed record InheritedCognomenDecisionDto
{
    [JsonPropertyOrder(0)]
    public required string DecisionId { get; init; }

    [JsonPropertyOrder(1)]
    public required string OriginalAgnomenId { get; init; }

    [JsonPropertyOrder(2)]
    public required string DecidingHouseholdId { get; init; }

    [JsonPropertyOrder(3)]
    public required bool AdoptedAsPermanentCognomen { get; init; }

    [JsonPropertyOrder(4)]
    public required int EffectiveFromDateTotalMonths { get; init; }
}

/// <summary>One <see cref="Gens.Simulation.Epithets.DynasticEpithet"/> (Phase 11 item 5), keyed by
/// household.</summary>
public sealed record DynasticEpithetDto
{
    [JsonPropertyOrder(0)]
    public required string HouseholdId { get; init; }

    [JsonPropertyOrder(1)]
    public required string EpithetText { get; init; }

    [JsonPropertyOrder(2)]
    public required IReadOnlyList<string> DerivedFromChronicleEntryIds { get; init; }
}
