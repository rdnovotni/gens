using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.State;

/// <summary>
/// The sole authoritative campaign truth container (ADR 0004). Holds one <see
/// cref="RuntimeIdCounter{T}"/> per runtime-instantiated entity kind (ADR 0001) — itself campaign
/// state, saved and restored like any other field — plus the ordered-index partitions those IDs
/// key into, the knowledge/visibility partition (ADR 0008), and the campaign clock. Entity record
/// partitions are typed placeholders until each entity kind's real record lands in the phase that
/// designs it (Phase 5 onward); Phase 2 only establishes the container shape.
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
        RuntimeIdCounter<Household> householdIds,
        RuntimeIdCounter<Actor> actorIds,
        RuntimeIdCounter<Character> characterIds,
        RuntimeIdCounter<Building> buildingIds,
        RuntimeIdCounter<Contract> contractIds,
        RuntimeIdCounter<Activity> activityIds,
        RuntimeIdCounter<Command> commandIds,
        RuntimeIdCounter<DomainEventEntity> eventIds,
        OrderedRegistry<RuntimeId<Character>, object> characters,
        KnowledgeState knowledge,
        long nextCommandSequenceNumber)
    {
        Date = date;
        RegionIds = regionIds;
        SettlementIds = settlementIds;
        PlotIds = plotIds;
        HouseholdIds = householdIds;
        ActorIds = actorIds;
        CharacterIds = characterIds;
        BuildingIds = buildingIds;
        ContractIds = contractIds;
        ActivityIds = activityIds;
        CommandIds = commandIds;
        EventIds = eventIds;
        Characters = characters;
        Knowledge = knowledge;
        _nextCommandSequenceNumber = nextCommandSequenceNumber;
    }

    public RuntimeIdCounter<Region> RegionIds { get; } = new();
    public RuntimeIdCounter<Settlement> SettlementIds { get; } = new();
    public RuntimeIdCounter<Plot> PlotIds { get; } = new();
    public RuntimeIdCounter<Household> HouseholdIds { get; } = new();
    public RuntimeIdCounter<Actor> ActorIds { get; } = new();
    public RuntimeIdCounter<Character> CharacterIds { get; } = new();
    public RuntimeIdCounter<Building> BuildingIds { get; } = new();
    public RuntimeIdCounter<Contract> ContractIds { get; } = new();
    public RuntimeIdCounter<Activity> ActivityIds { get; } = new();
    public RuntimeIdCounter<Command> CommandIds { get; } = new();
    public RuntimeIdCounter<DomainEventEntity> EventIds { get; } = new();

    /// <summary>Worked-example ordered-index partition (ADR 0004) proving the pattern; replaced with a
    /// typed <c>OrderedRegistry&lt;RuntimeId&lt;Character&gt;, Character&gt;</c> once Phase 5+ defines
    /// the real Character record.</summary>
    public OrderedRegistry<RuntimeId<Character>, object> Characters { get; } = new();

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
        ["householdIds"] = HouseholdIds.Peek,
        ["actorIds"] = ActorIds.Peek,
        ["characterIds"] = CharacterIds.Peek,
        ["buildingIds"] = BuildingIds.Peek,
        ["contractIds"] = ContractIds.Peek,
        ["activityIds"] = ActivityIds.Peek,
        ["commandIds"] = CommandIds.Peek,
        ["eventIds"] = EventIds.Peek,
        ["characters"] = Characters.Version,
        ["knowledge"] = Knowledge.Version,
        ["commandSequence"] = NextCommandSequenceNumber,
        ["date"] = Date.TotalMonths,
    };
}
