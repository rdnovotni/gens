using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Policies;

/// <summary>One household's current Standing Policy configuration (<c>gens-policies-edicts-design.md</c>
/// §2) — minimal per Phase 9 item 2's own scope, covering only the Rites Budget (§2.3) rather than the
/// design doc's full twelve-policy roster. <see cref="LastChangedDate"/> is the cooldown's own anchor
/// (§6.12's content-authored <c>cooldownMonths</c>, read here through <see
/// cref="RitesBudgetCatalog.CooldownMonths"/>): <see cref="ChangeRitesBudgetCommands"/> compares it
/// against the submitting date rather than decrementing a stored counter each month, matching <see
/// cref="Gens.Simulation.Time.GameDate"/>'s own "derive from stored dates, never store a redundant
/// countdown" idiom.</summary>
public sealed record HouseholdPolicyState(
    RuntimeId<Household> HouseholdId,
    RitesBudgetTier RitesBudget,
    GameDate LastChangedDate);
