using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Events;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.Policies;
using Gens.Simulation.Time;

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
        ["knowledge"] = Knowledge.Version,
        ["commandSequence"] = NextCommandSequenceNumber,
        ["date"] = Date.TotalMonths,
    };
}
