namespace Gens.Simulation.Characters;

/// <summary>A distinctive, permanent physical detail (<c>gens-familia-design.md</c> §2.4 / Core
/// §7.11's "a scar, a broken nose, gray at the temples with age"). A <see cref="CharacterVisualProfile"/>
/// carries zero or more of these; later Phase 5 work (permanent injuries, aging) can add to the set a
/// Character already has, but this item only generates the starting roll.</summary>
public enum NotableFeature
{
    Scar,
    BrokenNose,
    Freckled,
    Birthmark,
    GrayAtTemples,
}
