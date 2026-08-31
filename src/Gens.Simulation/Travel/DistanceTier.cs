namespace Gens.Simulation.Travel;

/// <summary><c>gens-starting-regions-design.md</c> §7.1's abstract Distance Tier: every region pair
/// carries one of these three relative to a Character's home region, rather than a computed geographic
/// distance — "a simple, hand-assigned lookup table per region pair, not a formula."</summary>
public enum DistanceTier
{
    Near,
    Moderate,
    Far,
}
