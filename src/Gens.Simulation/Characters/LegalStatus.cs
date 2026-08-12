namespace Gens.Simulation.Characters;

/// <summary>The five categorical legal statuses <c>gens-familia-design.md</c> §2.5 defines, each with
/// real mechanical differences (property rights, marriage eligibility, office-holding, obligations to
/// a patron). Fixed and code-defined rather than content-authored, matching the design doc's own
/// "categorical, five values" framing.</summary>
public enum LegalStatus
{
    RomanCitizen,
    LatinRights,
    Peregrine,
    Freedman,
    Enslaved,
}
