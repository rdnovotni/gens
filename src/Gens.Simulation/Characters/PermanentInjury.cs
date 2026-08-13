using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>A lasting, non-healing modifier (<c>gens-familia-design.md</c> §3.1) — "a lamed leg from
/// a battlefield wound, a maiming from a workplace or punishment accident, blindness from disease, a
/// difficult birth's lasting toll on the mother." <see cref="Magnitude"/> is the standing penalty
/// applied to <see cref="Target"/> and read back by <see cref="Character.GetEffectiveAttributes"/>,
/// <see cref="Character.GetEffectiveSkills"/>, and <see cref="Character.GetEffectiveFertility"/> — the
/// "hook" other future systems (Military & Combat, Labor & Slavery punishment, Natural Disasters,
/// difficult childbirth) attach to once they exist.</summary>
public readonly record struct PermanentInjury
{
    public PermanentInjury(PermanentInjuryTarget target, int magnitude, string cause, GameDate inflictedDate)
    {
        if (magnitude is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(magnitude), magnitude, "'magnitude' must be between 1 and 100.");
        if (string.IsNullOrWhiteSpace(cause))
            throw new ArgumentException("A permanent injury requires a non-empty cause label.", nameof(cause));

        Target = target;
        Magnitude = magnitude;
        Cause = cause;
        InflictedDate = inflictedDate;
    }

    public PermanentInjuryTarget Target { get; }
    public int Magnitude { get; }
    public string Cause { get; }
    public GameDate InflictedDate { get; }
}
