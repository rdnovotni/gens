namespace Gens.Simulation.Land;

/// <summary>
/// The four growth stages a settlement can advance through (Phase 6 item 1;
/// <c>gens-estate-settlement-design.md</c> §5). Each stage requires both a population threshold and
/// specific civic construction — the thresholds and construction requirements are additive fields
/// deferred to the settlement growth system pass, per ADR 0011's "additive only" policy.
/// </summary>
public enum SettlementStage
{
    /// <summary>The starting state — a single household's villa and its immediate land.</summary>
    Villa,

    /// <summary>A modest hamlet: at least one market and one shrine built, minimum population
    /// reached. (<c>gens-estate-settlement-design.md</c> §5)</summary>
    Vicus,

    /// <summary>A proper town: a Forum constructed, larger population threshold met.</summary>
    Town,

    /// <summary>A full city: a Basilica and an Aqueduct completed, substantial population
    /// established.</summary>
    City,
}
