namespace Gens.Simulation.Health;

/// <summary>A <see cref="CharacterHealthCondition"/> case's current outcome. <see cref="Active"/> is
/// the only status <see cref="CharacterHealthConditionSystem"/> still progresses each month —
/// <see cref="Recovered"/> and <see cref="Fatal"/> are terminal, and the entry is kept in
/// <c>WorldState.CharacterHealthConditions</c> forever once it reaches either, matching
/// <c>Events.EventInstances</c>'s "resolved or not, kept for the campaign's lifetime"
/// convention.</summary>
public enum CharacterHealthConditionStatus
{
    Active,
    Recovered,
    Fatal,
}
