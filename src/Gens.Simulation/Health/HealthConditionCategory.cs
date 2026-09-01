namespace Gens.Simulation.Health;

/// <summary>Which of Disease &amp; Public Health's two separately-tracked layers
/// (<c>gens-disease-public-health-design.md</c> §1) a <see cref="HealthConditionDefinition"/> belongs
/// to: <see cref="Chronic"/> for the Endemic Illness layer (§2) — a continuous, low-grade Health drain
/// rather than a discrete event — and <see cref="Acute"/> for the Epidemic layer (§3) — rare,
/// contagious, and the only layer §5's Immunity mechanic ever grants against.</summary>
public enum HealthConditionCategory
{
    Chronic,
    Acute,
}
