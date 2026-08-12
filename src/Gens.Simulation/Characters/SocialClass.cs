namespace Gens.Simulation.Characters;

/// <summary>The citizen-only political-ceiling categorical <c>gens-familia-design.md</c> §2.5
/// defines, orthogonal to <see cref="LegalStatus"/>. Only meaningful for a <see
/// cref="LegalStatus.RomanCitizen"/> — <see cref="Characters.Character.SocialClass"/> is <c>null</c>
/// for every other legal status.</summary>
public enum SocialClass
{
    Senatorial,
    Equestrian,
    Plebeian,
}
