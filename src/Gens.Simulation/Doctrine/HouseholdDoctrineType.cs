namespace Gens.Simulation.Doctrine;

/// <summary>§3.2's seven Household Doctrines (<c>gens-policies-edicts-design.md</c>) — every named
/// societal path is represented, matching <see cref="Legal.LegalCase.CaseType"/>'s own "every real
/// category represented, only some reachable" precedent. Only <see cref="MosMaiorum"/>, <see
/// cref="DomusPia"/>, and <see cref="DomusDura"/> are ever actually resolved above <see
/// cref="DoctrineTier.None"/> by <see cref="DoctrineResolutionSystem"/> — see that system's own doc
/// comment for exactly which real, already-shipped Standing Policy/Edict signals feed each one, and
/// for why the other four stay permanently at <see cref="DoctrineTier.None"/> in this item: <see
/// cref="ResPublicaPopularis"/> and <see cref="DomusBellatrix"/> both need Standing Policies this item
/// does not build (Patronage Generosity §2.8, Recruitment Doctrine's own Intensity dial §2.5 — see
/// this item's own roadmap write-up for why); <see cref="DomusMercatoria"/> needs Trade Openness (§2.7)
/// and a regional Market Dynamics pricing read, neither built; <see cref="DomusProvincialis"/> needs
/// Provincial Administration Posture (§2.10, frontier-only, unbuilt) and real foreign-cult engagement
/// (Religion §7, itself deferred pending Religions of the Known World content per that item's own
/// scope note).</summary>
public enum HouseholdDoctrineType
{
    MosMaiorum,
    ResPublicaPopularis,
    DomusMercatoria,
    DomusBellatrix,
    DomusPia,
    DomusProvincialis,
    DomusDura,
}

/// <summary>§3.1's three solidification thresholds, plus §3.3's Apex. <see cref="Apex"/> is kept for
/// data-model completeness with §9's own <c>tier</c> sketch ("none" | "emerging" | "defining" | "apex")
/// but is never actually assigned by <see cref="DoctrineResolutionSystem"/> in this item — Apex's own
/// real precondition (§3.3: "Defining survives a succession event with continued matching policy") is a
/// genuine, generation-spanning mechanic this item deliberately does not build; see this item's own
/// roadmap write-up for the reasoning.</summary>
public enum DoctrineTier
{
    None,
    Emerging,
    Defining,
    Apex,
}
