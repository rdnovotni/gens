namespace Gens.Simulation.Collegia;

/// <summary>The four real, historically distinct collegium categories (Phase 12 item 6;
/// <c>gens-collegia-guilds-design.md</c> §2), all coexisting — a single Character or household can
/// plausibly belong to more than one at once (§2's own "layered, overlapping affiliation").</summary>
public enum CollegiumType
{
    /// <summary>Collegia Opificum — Trade Guilds, drawn from Settlement Demographics' own Opifices and
    /// Negotiatores pop groups (§2).</summary>
    Opificum,

    /// <summary>Collegia Funeraticia — Burial Societies (§2, §8).</summary>
    Funeraticia,

    /// <summary>Organized around a specific deity's worship (§2). §2 also names "a foreign cult" as an
    /// alternative organizing principle, but Religions of the Known World (foreign-cult content) does
    /// not exist anywhere in this codebase yet — <see cref="CollegiumDetails.LinkedPatronDeity"/> only
    /// ever names one of the twelve real, already-shipped <see cref="Religion.PatronDeity"/> values,
    /// matching <see cref="Religion.HouseholdReligion"/>'s own "a closed, code-defined enum" precedent;
    /// a foreign-cult-linked collegium is a real, named gap left for whenever that content lands.</summary>
    CultSpecific,

    /// <summary>Collegia Compitalicia — Neighborhood Associations organized around a local crossroads
    /// shrine (§2).</summary>
    Compitalicia,
}

/// <summary>§7's real, genuinely live regulatory tension — not a settled background fact.</summary>
public enum CollegiumLegalStatus
{
    /// <summary>Authorized, legally recognized; operates openly with no risk to its own patron (§7).</summary>
    Licitum,

    /// <summary>Unauthorized, or a formerly Licitum collegium caught using §6's darker political tool
    /// once too often — operates under real, standing legal exposure (§7).</summary>
    Illicit,
}
