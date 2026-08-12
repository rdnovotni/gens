namespace Gens.Simulation.Characters;

/// <summary>The five universal Condition stats (<c>gens-familia-design.md</c> §2.3), numeric 0–100.
/// Permanent injuries (§3.1) and lifecycle transitions (Phase 5 item 3) mutate these over time; this
/// item only establishes the validated shape.</summary>
public readonly record struct Condition
{
    public Condition(int health, int fatigue, int loyalty, int ambition, int fertility)
    {
        Health = StatRange.Validate(health, nameof(health));
        Fatigue = StatRange.Validate(fatigue, nameof(fatigue));
        Loyalty = StatRange.Validate(loyalty, nameof(loyalty));
        Ambition = StatRange.Validate(ambition, nameof(ambition));
        Fertility = StatRange.Validate(fertility, nameof(fertility));
    }

    public int Health { get; }
    public int Fatigue { get; }
    public int Loyalty { get; }
    public int Ambition { get; }
    public int Fertility { get; }
}
