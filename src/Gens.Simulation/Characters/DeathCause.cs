namespace Gens.Simulation.Characters;

/// <summary>Why a Character died (<c>gens-familia-design.md</c> §3: "Death is deliberately
/// unrestricted in cause... old age (elderly-stage probability), disease (§6.13), childbirth (§6),
/// violence (Military §6.7, Legal §6.16 executions, Espionage §6.15 assassination), and disaster
/// (§6.17) can all end a life"). Familia doesn't gate which systems may kill a Character; this only
/// names the categories those systems attribute a death to. <see cref="Unspecified"/> covers any
/// cause not yet modeled by a concrete system.</summary>
public enum DeathCause
{
    OldAge,
    Disease,
    Childbirth,
    Violence,
    Disaster,
    Unspecified,
}
