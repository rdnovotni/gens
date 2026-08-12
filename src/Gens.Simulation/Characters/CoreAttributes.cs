namespace Gens.Simulation.Characters;

/// <summary>The five Core Attributes (<c>gens-familia-design.md</c> §2.1), numeric 0–100, applying
/// identically to every Character regardless of legal status or role.</summary>
public readonly record struct CoreAttributes
{
    public CoreAttributes(int diplomacy, int martial, int stewardship, int intrigue, int learning)
    {
        Diplomacy = StatRange.Validate(diplomacy, nameof(diplomacy));
        Martial = StatRange.Validate(martial, nameof(martial));
        Stewardship = StatRange.Validate(stewardship, nameof(stewardship));
        Intrigue = StatRange.Validate(intrigue, nameof(intrigue));
        Learning = StatRange.Validate(learning, nameof(learning));
    }

    public int Diplomacy { get; }
    public int Martial { get; }
    public int Stewardship { get; }
    public int Intrigue { get; }
    public int Learning { get; }
}
