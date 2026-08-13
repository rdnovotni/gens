namespace Gens.Simulation.Characters;

/// <summary>A generated name, shaped per <c>gens-familia-design.md</c> §2.8's naming convention for
/// the <see cref="LegalStatus"/>/<see cref="Sex"/> combination it was drawn for. <see
/// cref="Praenomen"/> and <see cref="Nomen"/> are always non-empty, matching <see
/// cref="Character.Create"/>'s own invariant.</summary>
public sealed record CharacterName(string Praenomen, string Nomen, string? Cognomen);
