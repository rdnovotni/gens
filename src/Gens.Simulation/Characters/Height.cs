namespace Gens.Simulation.Characters;

/// <summary>One of the fixed-genetics "height and build" attributes <c>gens-familia-design.md</c>
/// §2.4 / Core §7.11 describe — a raw descriptive attribute, distinct from Traits' own Physique tier
/// (Frail/Average/Strong/Herculean), which is later Phase 5 work (item 4) layered on top of this.</summary>
public enum Height
{
    // Not "Short": CA1720 flags an identifier that collides with the `short` (Int16) keyword alias.
    Diminutive,
    Average,
    Tall,
}
