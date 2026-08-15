using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Recorded once a Character has fled (<c>gens-labor-slavery-design.md</c> §7) — non-null on
/// <see cref="Character.Flight"/> for as long as they remain at large, whether or not a <see
/// cref="PursuitRecord"/> is currently active. <see cref="FormerHousehold"/> is what a successful
/// recapture restores <see cref="Character.Household"/> to.</summary>
public readonly record struct FledRecord(GameDate FledDate, RuntimeId<Household> FormerHousehold, RuntimeId<Settlement>? LastKnownLocation);
