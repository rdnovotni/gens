using Gens.Simulation.Identity;

namespace Gens.Simulation.Economy;

/// <summary>A household's position on §9's Insolvency ladder, distinct from any single <see
/// cref="DebtRecord"/>'s own <see cref="DebtStatus"/> (§9: "distinct from, and independent of, §6.3-6.4's
/// per-debt consequences" — an aggregate Net Worth state, not a per-loan one).</summary>
public enum InsolvencyStage
{
    Solvent,
    AtRisk,
    Insolvent,
    Ruined,
}

/// <summary>One consequence §9's ladder can apply. Only the first two/three are implemented — see
/// <see cref="InsolvencySystem"/>'s own doc comment for why <c>officeOrCensusLoss</c> and
/// <c>chronicleEntry</c> (§9 rungs 4-5) are skipped.</summary>
public enum InsolvencyConsequence
{
    ForcedLiquidation,
    VillaStageDemotion,
}

/// <summary>
/// One household's Insolvency tracking record (Phase 8 item 6; §9's <c>InsolvencyState</c>).
/// <see cref="ConsequencesApplied"/> is cumulative for as long as this record exists — once a
/// consequence category has ever fired, it stays listed, matching how a Chronicle-style record would
/// never "un-happen" an entry (§9's own "A Dynasty Chronicle entry" framing, even though the Chronicle
/// entry itself is out of scope here).
/// </summary>
public sealed record InsolvencyState(
    RuntimeId<Household> HouseholdId,
    int MonthsBelowThreshold,
    InsolvencyStage Stage,
    IReadOnlyList<InsolvencyConsequence> ConsequencesApplied);
