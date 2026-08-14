using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Buildings;

public enum BuildingTier
{
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3,
}

/// <summary>A positive quantity of a content-authored good.</summary>
public readonly record struct RecipeLine
{
    public RecipeLine(DefinitionId<Good> goodId, long quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "A recipe quantity must be positive.");
        GoodId = goodId;
        Quantity = quantity;
    }

    public DefinitionId<Good> GoodId { get; }
    public long Quantity { get; }
}

/// <summary>A deterministic monthly conversion performed by a staffed, maintained building.</summary>
public sealed record ProductionRecipe
{
    public ProductionRecipe(IEnumerable<RecipeLine> inputs, IEnumerable<RecipeLine> outputs)
    {
        Inputs = Normalize(inputs, nameof(inputs), allowEmpty: true);
        Outputs = Normalize(outputs, nameof(outputs), allowEmpty: false);
    }

    public IReadOnlyList<RecipeLine> Inputs { get; }
    public IReadOnlyList<RecipeLine> Outputs { get; }

    private static IReadOnlyList<RecipeLine> Normalize(IEnumerable<RecipeLine> lines, string name, bool allowEmpty)
    {
        if (lines is null)
            throw new ArgumentNullException(name);
        var result = lines.OrderBy(line => line.GoodId.Value, StringComparer.Ordinal).ToArray();
        if (!allowEmpty && result.Length == 0)
            throw new ArgumentException("A production recipe needs at least one output.", name);
        if (result.Select(line => line.GoodId).Distinct().Count() != result.Length)
            throw new ArgumentException("A good may occur only once in each side of a recipe.", name);
        return result;
    }
}

/// <summary>A named worker role and the number of assignments it accepts.</summary>
public sealed record StaffingSlot
{
    public StaffingSlot(string id, int capacity, bool requiredForProduction = true)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A staffing slot ID is required.", nameof(id));
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Staffing capacity must be positive.");
        Id = id;
        Capacity = capacity;
        RequiredForProduction = requiredForProduction;
    }

    public string Id { get; }
    public int Capacity { get; }
    public bool RequiredForProduction { get; }
}

/// <summary>Content-authored rules shared by every runtime instance of a building.</summary>
public sealed record BuildingDefinition
{
    public BuildingDefinition(
        DefinitionId<Building> id,
        BuildingTier tier,
        int constructionMonths,
        int plotCapacity,
        IEnumerable<DefinitionId<Building>>? prerequisites = null,
        IEnumerable<TerrainType>? allowedTerrain = null,
        TerrainFeature requiredFeatures = TerrainFeature.None,
        IEnumerable<RecipeLine>? upkeep = null,
        IEnumerable<StaffingSlot>? staffingSlots = null,
        ProductionRecipe? recipe = null)
    {
        if (!Enum.IsDefined(typeof(BuildingTier), tier))
            throw new ArgumentOutOfRangeException(nameof(tier));
        if (constructionMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(constructionMonths), constructionMonths, "Construction time must be positive.");
        if (plotCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(plotCapacity), plotCapacity, "Plot capacity use must be positive.");

        Id = id;
        Tier = tier;
        ConstructionMonths = constructionMonths;
        PlotCapacity = plotCapacity;
        Prerequisites = DistinctSorted(prerequisites ?? Array.Empty<DefinitionId<Building>>());
        if (Prerequisites.Contains(id))
            throw new ArgumentException("A building cannot require itself.", nameof(prerequisites));
        AllowedTerrain = (allowedTerrain ?? Array.Empty<TerrainType>()).Distinct().OrderBy(value => value).ToArray();
        RequiredFeatures = requiredFeatures;
        Upkeep = NormalizeLines(upkeep ?? Array.Empty<RecipeLine>(), nameof(upkeep));
        StaffingSlots = (staffingSlots ?? Array.Empty<StaffingSlot>()).OrderBy(slot => slot.Id, StringComparer.Ordinal).ToArray();
        if (StaffingSlots.Select(slot => slot.Id).Distinct(StringComparer.Ordinal).Count() != StaffingSlots.Count)
            throw new ArgumentException("Staffing slot IDs must be unique.", nameof(staffingSlots));
        Recipe = recipe;
    }

    public DefinitionId<Building> Id { get; }
    public BuildingTier Tier { get; }
    public int ConstructionMonths { get; }
    public int PlotCapacity { get; }
    public IReadOnlyList<DefinitionId<Building>> Prerequisites { get; }
    /// <summary>An empty list permits every primary terrain type.</summary>
    public IReadOnlyList<TerrainType> AllowedTerrain { get; }
    public TerrainFeature RequiredFeatures { get; }
    public IReadOnlyList<RecipeLine> Upkeep { get; }
    public IReadOnlyList<StaffingSlot> StaffingSlots { get; }
    public ProductionRecipe? Recipe { get; }

    public bool Allows(Plot plot) =>
        plot is not null &&
        (AllowedTerrain.Count == 0 || AllowedTerrain.Contains(plot.Terrain)) &&
        (plot.Features & RequiredFeatures) == RequiredFeatures;

    private static IReadOnlyList<DefinitionId<Building>> DistinctSorted(IEnumerable<DefinitionId<Building>> values) =>
        values.Distinct().OrderBy(value => value.Value, StringComparer.Ordinal).ToArray();

    private static IReadOnlyList<RecipeLine> NormalizeLines(IEnumerable<RecipeLine> values, string name)
    {
        var lines = values.OrderBy(value => value.GoodId.Value, StringComparer.Ordinal).ToArray();
        if (lines.Select(line => line.GoodId).Distinct().Count() != lines.Length)
            throw new ArgumentException("A good may occur only once.", name);
        return lines;
    }
}
