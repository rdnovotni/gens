using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Buildings;

public sealed record ConstructionProject(
    long Sequence,
    RuntimeId<Plot> PlotId,
    BuildingDefinition Definition,
    int CompletedMonths)
{
    public int RemainingMonths => Definition.ConstructionMonths - CompletedMonths;
}

/// <summary>FIFO construction with deterministic sequence numbers and validation at enqueue time.</summary>
public sealed class ConstructionQueue
{
    private readonly List<ConstructionProject> _projects = new();
    private long _nextSequence;

    public IReadOnlyList<ConstructionProject> Projects => _projects;

    public ConstructionProject Enqueue(
        Plot plot,
        BuildingDefinition definition,
        IEnumerable<BuildingInstance> completedBuildings)
    {
        if (plot is null) throw new ArgumentNullException(nameof(plot));
        if (definition is null) throw new ArgumentNullException(nameof(definition));
        if (completedBuildings is null) throw new ArgumentNullException(nameof(completedBuildings));
        if (plot.IsContested)
            throw new InvalidOperationException("Construction cannot begin on contested land.");
        if (!definition.Allows(plot))
            throw new InvalidOperationException("The plot does not satisfy the building's terrain gates.");

        var completed = completedBuildings.ToArray();
        var availableDefinitions = completed.Select(value => value.Definition.Id).ToHashSet();
        var missing = definition.Prerequisites.Where(required => !availableDefinitions.Contains(required)).ToArray();
        if (missing.Length != 0)
            throw new InvalidOperationException($"Missing building prerequisites: {string.Join(", ", missing)}.");

        var used = completed.Where(value => value.PlotId == plot.Id).Sum(value => value.Definition.PlotCapacity) +
            _projects.Where(value => value.PlotId == plot.Id).Sum(value => value.Definition.PlotCapacity);
        if (used + definition.PlotCapacity > plot.Capacity)
            throw new InvalidOperationException("The plot does not have enough unused building capacity.");

        var project = new ConstructionProject(_nextSequence++, plot.Id, definition, 0);
        _projects.Add(project);
        return project;
    }

    /// <summary>Advances only the head project; returns it when construction completes.</summary>
    public ConstructionProject? AdvanceMonth(bool laborAvailable = true)
    {
        if (_projects.Count == 0 || !laborAvailable)
            return null;
        var advanced = _projects[0] with { CompletedMonths = _projects[0].CompletedMonths + 1 };
        if (advanced.RemainingMonths > 0)
        {
            _projects[0] = advanced;
            return null;
        }
        _projects.RemoveAt(0);
        return advanced;
    }

    public bool Cancel(long sequence)
    {
        var index = _projects.FindIndex(project => project.Sequence == sequence);
        if (index < 0) return false;
        _projects.RemoveAt(index);
        return true;
    }
}
