namespace Gens.Simulation.Land;

/// <summary>Additional map features which may overlap a plot's primary terrain.</summary>
[Flags]
public enum TerrainFeature
{
    None = 0,
    RiverAdjacent = 1 << 0,
    Coastline = 1 << 1,
    Wooded = 1 << 2,
    MineralDeposit = 1 << 3,
    Irrigated = 1 << 4,
}
