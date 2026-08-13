using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>The permanent record of a Character's death (<c>gens-familia-design.md</c> §3: "Familia
/// doesn't gate which systems are allowed to kill someone; it just guarantees every death has
/// somewhere to register"). A Character's <see cref="Character.DeathRecord"/> being non-null is itself
/// the alive/dead flag — see <see cref="Character.IsAlive"/>.</summary>
public readonly record struct DeathRecord
{
    public DeathRecord(GameDate date, DeathCause cause, int ageAtDeath)
    {
        if (ageAtDeath < 0)
            throw new ArgumentOutOfRangeException(nameof(ageAtDeath), ageAtDeath, "Age at death cannot be negative.");

        Date = date;
        Cause = cause;
        AgeAtDeath = ageAtDeath;
    }

    public GameDate Date { get; }
    public DeathCause Cause { get; }
    public int AgeAtDeath { get; }
}
