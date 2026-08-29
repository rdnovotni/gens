namespace Gens.Simulation.Clientela;

/// <summary>The five client specialties <c>gens-politics-patronage-design.md</c> §4.2 defines, each
/// loosely following whichever Core Attribute a client is strongest in and determining what favor they
/// can actually perform when called on (Legal testimony, a Mercantile trade tip, Martial retinue
/// muscle, Religious festival favor, Administrative Curia support). Fixed and code-defined rather than
/// content-authored, matching <see cref="Characters.LegalStatus"/>'s identical "categorical, fixed
/// values" convention — §4.2's own table is closed, not an open content catalog.</summary>
public enum ClientSpecialty
{
    Legal,
    Mercantile,
    Martial,
    Religious,
    Administrative,
}
