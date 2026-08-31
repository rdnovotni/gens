namespace Gens.Simulation.Regions;

/// <summary>Whether a region is selectable at campaign start or belongs to the roster's own future
/// expansion slate (<c>gens-starting-regions-design.md</c> §5, §12).</summary>
public enum RegionStatus
{
    /// <summary>One of the launch roster's six regions (§5.1): selectable at campaign start today.</summary>
    Launch,

    /// <summary>A named future candidate on the extensible slate (§5.2), not yet assumed complete —
    /// "none of them is assumed complete until it receives one [document]."</summary>
    ExtensibleSlate,
}
