using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Buildings;

public enum BuildingCondition
{
    Ruined = 0,
    Poor = 1,
    Worn = 2,
    Sound = 3,
    Pristine = 4,
}

/// <summary>Campaign state for one completed building.</summary>
public sealed class BuildingInstance
{
    private readonly SortedDictionary<string, SortedSet<string>> _staff = new(StringComparer.Ordinal);

    public BuildingInstance(RuntimeId<Building> id, RuntimeId<Plot> plotId, BuildingDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Id = id;
        PlotId = plotId;
        Condition = BuildingCondition.Pristine;
        foreach (var slot in definition.StaffingSlots)
            _staff.Add(slot.Id, new SortedSet<string>(StringComparer.Ordinal));
    }

    public RuntimeId<Building> Id { get; }
    public RuntimeId<Plot> PlotId { get; }
    public BuildingDefinition Definition { get; }
    public BuildingCondition Condition { get; private set; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Staff =>
        _staff.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<string>)pair.Value.ToArray(),
            StringComparer.Ordinal);
    public bool IsOperational => Condition != BuildingCondition.Ruined &&
        Definition.StaffingSlots.Where(slot => slot.RequiredForProduction)
            .All(slot => _staff[slot.Id].Count == slot.Capacity);

    public void AssignStaff(string slotId, string workerId)
    {
        if (!_staff.TryGetValue(slotId, out var workers))
            throw new KeyNotFoundException($"Staffing slot '{slotId}' does not exist.");
        if (string.IsNullOrWhiteSpace(workerId))
            throw new ArgumentException("A worker ID is required.", nameof(workerId));
        if (_staff.Values.Any(set => set.Contains(workerId)))
            throw new InvalidOperationException($"Worker '{workerId}' is already assigned to this building.");
        var slot = Definition.StaffingSlots.Single(value => value.Id == slotId);
        if (workers.Count >= slot.Capacity)
            throw new InvalidOperationException($"Staffing slot '{slotId}' is full.");
        workers.Add(workerId);
    }

    public bool ReleaseStaff(string workerId)
    {
        if (string.IsNullOrWhiteSpace(workerId))
            throw new ArgumentException("A worker ID is required.", nameof(workerId));
        return _staff.Values.Any(workers => workers.Remove(workerId));
    }

    /// <summary>Records whether this month's authored upkeep was fully paid.</summary>
    public void ApplyUpkeep(bool paid)
    {
        if (paid)
            return;
        if (Condition > BuildingCondition.Ruined)
            Condition--;
    }

    public void Repair(BuildingCondition condition = BuildingCondition.Pristine)
    {
        if (!Enum.IsDefined(typeof(BuildingCondition), condition) || condition == BuildingCondition.Ruined)
            throw new ArgumentOutOfRangeException(nameof(condition), "A repair must restore a usable condition.");
        if (condition <= Condition)
            throw new InvalidOperationException("A repair must improve the building's condition.");
        Condition = condition;
    }

    /// <summary>Rebuilds a building instance from persisted save data (ADR 0010), bypassing the
    /// constructor's "always starts Pristine and unstaffed" default and <see cref="ApplyUpkeep"/>/<see
    /// cref="Repair"/>'s step-at-a-time validation — the condition and staffing were already valid when
    /// saved (matching <see cref="Identity.OrderedRegistry{TId,TEntity}.Restore"/>'s identical
    /// reasoning).</summary>
    public static BuildingInstance Restore(
        RuntimeId<Building> id,
        RuntimeId<Plot> plotId,
        BuildingDefinition definition,
        BuildingCondition condition,
        IEnumerable<KeyValuePair<string, IReadOnlyCollection<string>>> staff)
    {
        if (!Enum.IsDefined(typeof(BuildingCondition), condition))
            throw new ArgumentOutOfRangeException(nameof(condition));
        if (staff is null)
            throw new ArgumentNullException(nameof(staff));

        var instance = new BuildingInstance(id, plotId, definition) { Condition = condition };
        foreach (var (slotId, workers) in staff)
        {
            if (!instance._staff.TryGetValue(slotId, out var set))
                throw new KeyNotFoundException($"Staffing slot '{slotId}' does not exist.");
            foreach (var workerId in workers)
                set.Add(workerId);
        }

        return instance;
    }
}
