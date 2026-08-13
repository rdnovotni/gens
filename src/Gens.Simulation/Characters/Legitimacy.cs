namespace Gens.Simulation.Characters;

/// <summary>Whether a child's birth was within a recognized marriage (<c>gens-familia-design.md</c>
/// §5.2: "children born within a recognized marriage are legitimate by default; children resulting
/// from an affair... are not, unless the paterfamilias explicitly acknowledges and legitimizes
/// them"). A Character with no recorded parents (e.g. a campaign-start founder instantiated directly
/// rather than born in play) is <see cref="Legitimate"/> by convention — there is no marriage question
/// to have been outside of.</summary>
public enum Legitimacy
{
    Legitimate,
    Illegitimate,
}
