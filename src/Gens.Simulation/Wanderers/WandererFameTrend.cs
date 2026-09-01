namespace Gens.Simulation.Wanderers;

/// <summary>A Wanderer's own itinerant-reputation trajectory
/// (<c>gens-wandering-populations-design.md</c> §4/§10's <c>fameTrend</c>) — deliberately the identical
/// three-value shape <see cref="Actors.LivingWorldActorStandingTrend"/> already applies to a gens's own
/// standing, per §4's explicit "the same Rising/Established/Declining shape Rival Houses already
/// applies to a gens's own standing-trend, here applied to an individual" and §9's own cross-reference
/// saying the same.
///
/// <para>A distinct enum rather than a direct reuse of <see cref="Actors.LivingWorldActorStandingTrend"/>:
/// that type's own doc comment defines it as a <i>house's</i> fortune trajectory and it is what <see
/// cref="Actors.LivingWorldActorExtinctionSystem"/> gates a house's extinction roll on — a Wanderer is
/// an individual, is never a <see cref="Actors.LivingWorldActor"/>, and must never be swept into those
/// house-level rolls by sharing their type. The <i>shape</i> is mirrored exactly (three values, same
/// order, same meaning) and the trend is computed the same "compare against the previous reading"
/// way; only the identity is separate.</para></summary>
public enum WandererFameTrend
{
    Rising,
    Established,
    Declining,
}
