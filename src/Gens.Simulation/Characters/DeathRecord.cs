using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>The permanent record of a Character's death (<c>gens-familia-design.md</c> §3: "Familia
/// doesn't gate which systems are allowed to kill someone; it just guarantees every death has
/// somewhere to register"). A Character's <see cref="Character.DeathRecord"/> being non-null is itself
/// the alive/dead flag — see <see cref="Character.IsAlive"/>.</summary>
public readonly record struct DeathRecord
{
    public DeathRecord(GameDate date, DeathCause cause, int ageAtDeath, DefinitionId<HealthConditionDefinition>? conditionId = null)
    {
        if (ageAtDeath < 0)
            throw new ArgumentOutOfRangeException(nameof(ageAtDeath), ageAtDeath, "Age at death cannot be negative.");

        Date = date;
        Cause = cause;
        AgeAtDeath = ageAtDeath;
        ConditionId = conditionId;
    }

    public GameDate Date { get; }
    public DeathCause Cause { get; }
    public int AgeAtDeath { get; }

    /// <summary>Which specific <see cref="Health.HealthConditionDefinition"/> caused this death, when
    /// <see cref="Cause"/> is <see cref="DeathCause.Disease"/> and <see
    /// cref="Health.CharacterHealthConditionSystem"/> is what attributed it (Phase 14 item 1 — closing
    /// the "which disease" gap <see cref="DeathCause.Disease"/> left open since Phase 5 item 3). Null
    /// for every other <see cref="Cause"/>, and for a Disease death <see
    /// cref="Characters.CharacterLifecycleSystem"/>'s own older, coarser Infant-stage heuristic
    /// attributed instead — that heuristic is untouched by this item.</summary>
    public DefinitionId<HealthConditionDefinition>? ConditionId { get; }
}
