namespace Gens.Simulation.Wanderers;

/// <summary>The six real, historically grounded categories <c>gens-wandering-populations-design.md</c>
/// §2 names — "extensible, not exhaustive" in that section's own words, so this enum is deliberately
/// open to later additions rather than presented as a closed roster. Ordering matches §2's own list
/// top-to-bottom, the same convention <c>Hazards.HazardType</c> already established for §2 of its own
/// design input.</summary>
public enum WandererType
{
    /// <summary>§2: the itinerant teaching and lecturing circuit real history calls the Second
    /// Sophistic. Gravitates toward Education &amp; Culture's own Institutions of Renown — a concept
    /// this codebase has not built, so <see cref="WandererTypeCatalog"/> substitutes the closest real,
    /// queryable proxy (high-<see cref="Regions.ProminenceTier"/> Gazetteer entries); see that
    /// catalog's own doc comment for the full disclosure.</summary>
    PhilosopherRhetorician,

    /// <summary>§2: skilled specialists brought in from elsewhere for a major commission, gravitating
    /// toward wherever construction is actively underway.</summary>
    ArchitectEngineer,

    /// <summary>§2: an individually named trader distinct from Resources &amp; Goods' own abstract
    /// trade-route flow, gravitating toward the best margins.</summary>
    MerchantPeddler,

    /// <summary>§2: traveling troupes of actors, musicians, and acrobats, hired settlement to
    /// settlement.</summary>
    Entertainer,

    /// <summary>§2: itinerant medical specialists, gravitating toward an outbreak or a settlement
    /// without its own Court Physician.</summary>
    Physician,

    /// <summary>§2: itinerant religious and mystic figures, gravitating toward foreign-cult encounter
    /// opportunities.</summary>
    HolyManAstrologer,
}
