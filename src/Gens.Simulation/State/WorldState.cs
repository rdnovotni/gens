using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
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
        OrderedRegistry<RuntimeId<Region>, Region> regions,
        OrderedRegistry<RuntimeId<Settlement>, Settlement> settlements,
        OrderedRegistry<RuntimeId<Plot>, Plot> plots,
        OrderedRegistry<RuntimeId<Holding>, Holding> holdings,
        OrderedRegistry<RuntimeId<Character>, Character> characters,
        OrderedRegistry<RelationshipKey, Relationship> relationships,
        OrderedRegistry<ScheduledActionKey, ScheduledActionEntry> scheduledActions,
        OrderedRegistry<PopGroupKey, PopGroup> popGroups,
        OrderedRegistry<HouseholdRegimenKey, RegimenSettings> householdRegimenDefaults,
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
        Regions = regions;
        Settlements = settlements;
        Plots = plots;
        Holdings = holdings;
        Characters = characters;
        Relationships = relationships;
        ScheduledActions = scheduledActions;
        PopGroups = popGroups;
        HouseholdRegimenDefaults = householdRegimenDefaults;
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
        ["regions"] = Regions.Version,
        ["settlements"] = Settlements.Version,
        ["plots"] = Plots.Version,
        ["holdings"] = Holdings.Version,
        ["characters"] = Characters.Version,
        ["relationships"] = Relationships.Version,
        ["scheduledActions"] = ScheduledActions.Version,
        ["popGroups"] = PopGroups.Version,
        ["householdRegimenDefaults"] = HouseholdRegimenDefaults.Version,
        ["knowledge"] = Knowledge.Version,
        ["commandSequence"] = NextCommandSequenceNumber,
        ["date"] = Date.TotalMonths,
    };
}
