using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>The three real historical manumission mechanisms <c>gens-labor-slavery-design.md</c> §8
/// simplifies. <see cref="ManumitCommand"/> handles <see cref="Vindicta"/> and <see cref="Censu"/>
/// immediately; <see cref="Testamento"/> is the only one that needs standing planned state (<see
/// cref="ManumissionPlan"/>) since it doesn't take effect until the grantor dies.</summary>
public enum ManumissionType
{
    Vindicta,
    Testamento,
    Censu,
}

/// <summary>A pending Testamento grant (§8) — non-null on <see cref="Character.ManumissionPlan"/> only
/// between <see cref="PlanTestamentoManumissionCommand"/> and the grantor's eventual death, which <see
/// cref="LaborFlightSystem"/> watches for and resolves each tick.</summary>
public readonly record struct ManumissionPlan(RuntimeId<Character> GrantorId, ManumissionType Type);
