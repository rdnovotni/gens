namespace Gens.Simulation.Health;

/// <summary>An <see cref="EpidemicOutbreak"/>'s lifecycle state. <see cref="Active"/> is the only
/// status <see cref="EpidemicContagionSystem"/> still spreads from; <see cref="Ended"/> is terminal
/// once a settlement's active case count for that disease returns to zero, and — matching <see
/// cref="CharacterHealthConditionStatus"/>'s own convention — the entry is kept in <c>WorldState</c>
/// forever rather than removed, so a settlement's outbreak history stays queryable.</summary>
public enum EpidemicOutbreakStatus
{
    Active,
    Ended,
}
