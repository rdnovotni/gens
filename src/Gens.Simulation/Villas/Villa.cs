using Gens.Simulation.Buildings;

namespace Gens.Simulation.Villas;

/// <summary>The independent growth track of a holding's principal residence.</summary>
public enum VillaStage
{
    Rustica = 0,
    Urbana = 1,
    Domus = 2,
}

/// <summary>Capacity applied only to rooms which provide bulk household or guest housing.</summary>
public enum VillaRoomCapacityTier
{
    None = 0,
    Modest = 1,
    Ample = 2,
    Grand = 3,
    Vast = 4,
}

/// <summary>
/// A content-light room definition. Room effects and decoration content deliberately remain outside
/// the campaign model; this layer records only the construction gates shared with buildings.
/// </summary>
public sealed record VillaRoomDefinition
{
    public VillaRoomDefinition(string key, VillaStage minimumStage, int maximumTier = 1, bool usesRoomSlot = true)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A room key is required.", nameof(key));
        if (!Enum.IsDefined(typeof(VillaStage), minimumStage))
            throw new ArgumentOutOfRangeException(nameof(minimumStage));
        if (maximumTier <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumTier), maximumTier, "Maximum tier must be positive.");

        Key = key;
        MinimumStage = minimumStage;
        MaximumTier = maximumTier;
        UsesRoomSlot = usesRoomSlot;
    }

    public string Key { get; }
    public VillaStage MinimumStage { get; }
    public int MaximumTier { get; }
    public bool UsesRoomSlot { get; }
}

/// <summary>Campaign state for one constructed villa room.</summary>
public sealed record VillaRoomInstance
{
    public VillaRoomInstance(
        VillaRoomDefinition definition,
        int tier = 1,
        VillaRoomCapacityTier capacityTier = VillaRoomCapacityTier.None,
        BuildingCondition condition = BuildingCondition.Pristine,
        string? assignedTo = null)
        : this(definition?.Key ?? throw new ArgumentNullException(nameof(definition)), definition, tier, capacityTier, condition, assignedTo)
    {
    }

    public VillaRoomInstance(
        string key,
        VillaRoomDefinition definition,
        int tier = 1,
        VillaRoomCapacityTier capacityTier = VillaRoomCapacityTier.None,
        BuildingCondition condition = BuildingCondition.Pristine,
        string? assignedTo = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A room instance key is required.", nameof(key));
        if (tier <= 0 || tier > definition.MaximumTier)
            throw new ArgumentOutOfRangeException(nameof(tier));
        if (!Enum.IsDefined(typeof(VillaRoomCapacityTier), capacityTier))
            throw new ArgumentOutOfRangeException(nameof(capacityTier));
        if (!Enum.IsDefined(typeof(BuildingCondition), condition))
            throw new ArgumentOutOfRangeException(nameof(condition));
        if (assignedTo is not null && string.IsNullOrWhiteSpace(assignedTo))
            throw new ArgumentException("An assignee ID cannot be empty.", nameof(assignedTo));

        Key = key;
        Tier = tier;
        CapacityTier = capacityTier;
        Condition = condition;
        AssignedTo = assignedTo;
    }

    public VillaRoomDefinition Definition { get; }
    public string Key { get; }
    public int Tier { get; }
    public VillaRoomCapacityTier CapacityTier { get; }
    public BuildingCondition Condition { get; }
    public string? AssignedTo { get; }
}

/// <summary>
/// The specialized building layer attached to a holding. It owns room instances and its stage is
/// intentionally independent of the surrounding settlement's growth stage.
/// </summary>
public sealed class Villa
{
    private readonly SortedDictionary<string, VillaRoomInstance> _rooms = new(StringComparer.Ordinal);

    public Villa(VillaStage stage = VillaStage.Rustica, bool isOutpost = false)
    {
        if (!Enum.IsDefined(typeof(VillaStage), stage))
            throw new ArgumentOutOfRangeException(nameof(stage));
        if (isOutpost && stage != VillaStage.Rustica)
            throw new ArgumentException("An outpost residence is permanently capped at Rustica.", nameof(stage));
        Stage = stage;
        IsOutpost = isOutpost;
    }

    public VillaStage Stage { get; private set; }
    public bool IsOutpost { get; }
    public IReadOnlyCollection<VillaRoomInstance> Rooms => _rooms.Values.ToArray();
    public int UsedRoomSlots => _rooms.Values.Count(room => room.Definition.UsesRoomSlot);

    public void AddRoom(VillaRoomInstance room)
    {
        if (room is null) throw new ArgumentNullException(nameof(room));
        if (room.Definition.MinimumStage > Stage)
            throw new InvalidOperationException($"Room '{room.Definition.Key}' requires villa stage {room.Definition.MinimumStage}.");
        if (!_rooms.TryAdd(room.Key, room))
            throw new InvalidOperationException($"Room key '{room.Key}' already exists in this villa.");
    }

    public bool TryGetRoom(string key, out VillaRoomInstance room) => _rooms.TryGetValue(key, out room!);

    public void AdvanceStage()
    {
        if (IsOutpost)
            throw new InvalidOperationException("An outpost residence cannot advance beyond Rustica.");
        if (Stage == VillaStage.Domus)
            throw new InvalidOperationException("The villa is already at its maximum stage.");
        Stage++;
    }

    /// <summary>
    /// Forces the villa's stage backward one step (Phase 8 item 6; <c>gens-economy-finance-design.md</c>
    /// §9's Insolvency ladder rung 3: "where upkeep genuinely can't be sustained, the Villa's own stage...
    /// can be forced backward"). Unlike <see cref="AdvanceStage"/>, this is never a player action — only
    /// <see cref="Economy.InsolvencySystem"/> calls it, and only for a household at the <c>Ruined</c>
    /// stage. §13 leaves reversibility on recovery an open question; this implementation does not
    /// automatically restore a demoted stage, matching that open question's own "isn't decided" framing —
    /// a future pass can add restoration once the question is resolved.
    /// </summary>
    public void DemoteStage()
    {
        if (IsOutpost)
            throw new InvalidOperationException("An outpost residence has no stage to demote.");
        if (Stage == VillaStage.Rustica)
            throw new InvalidOperationException("The villa is already at its minimum stage.");
        Stage--;
    }
}
