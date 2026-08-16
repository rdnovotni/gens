namespace Gens.Simulation.Buildings;

/// <summary>The background-economy sector a Building's own investment counts toward
/// (<c>gens-settlement-demographics-design.md</c> §4.1 "Where Jobs Come From"). Distinct from <see
/// cref="ProductionRecipe"/>'s goods-in/goods-out shape: this is purely about which background <see
/// cref="Characters.PopGroupType"/> a completed building's mere existence creates job openings for,
/// not what it produces. <see cref="None"/> is the default for buildings that generate no background
/// job capacity at all (civic buildings, or fixtures authored before this field existed).</summary>
public enum BuildingSector
{
    None,
    Agriculture,
    Industry,
    Commerce,
    Religion,
}
