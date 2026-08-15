using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>An active pursuit of a fled Character (<c>gens-labor-slavery-design.md</c> §7: "a
/// player-initiated response with a limited window ... before the trail goes cold"). Non-null on <see
/// cref="Character.Pursuit"/> only while the countdown is running; <see cref="LaborFlightSystem"/>
/// decrements <see cref="MonthsRemaining"/> each tick and resolves an outcome once it reaches zero.</summary>
public readonly record struct PursuitRecord(int MonthsRemaining, RuntimeId<Character>? PursuerId);
