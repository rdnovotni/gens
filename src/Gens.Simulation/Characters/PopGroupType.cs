namespace Gens.Simulation.Characters;

/// <summary>The eight background population groups <c>gens-settlement-demographics-design.md</c> §3
/// names, each a real Latin term. Fixed and code-defined, matching that document's own "eight groups"
/// framing rather than anything content-authored.</summary>
public enum PopGroupType
{
    /// <summary>Tenant Farmers — Coloni.</summary>
    Coloni,

    /// <summary>Urban Laborers — Operarii.</summary>
    Operarii,

    /// <summary>Artisans/Craftsmen — Opifices.</summary>
    Opifices,

    /// <summary>Shopkeepers/Merchants — Negotiatores.</summary>
    Negotiatores,

    /// <summary>Temple Staff — Aeditui.</summary>
    Aeditui,

    /// <summary>Local Notables — Curiales; the pool marriage candidates, political allies, and Court
    /// Position recruits are drawn from (§3).</summary>
    Curiales,

    /// <summary>Discharged soldiers settled on granted land — Veterani (§3.1).</summary>
    Veterans,

    /// <summary>The Non-Household Enslaved cohort (§9).</summary>
    NonHouseholdEnslaved,
}
