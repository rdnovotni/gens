namespace Gens.Simulation.Characters;

/// <summary>How a Character record first came to exist (<c>gens-characters-design.md</c> §14's
/// <c>source</c> enum), read alongside <see cref="Character.InstantiatedAtMonth"/> by later Phase 5
/// items (lazy instantiation/promotion, item 7) — not written to by anything in this item.</summary>
public enum CharacterSource
{
    Familia,
    CourtPosition,
    CuriaSeat,
    RivalGenerated,
    TravelEncounter,
    EventEncounter,
    Guest,
}
