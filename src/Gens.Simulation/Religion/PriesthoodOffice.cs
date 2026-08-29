namespace Gens.Simulation.Religion;

/// <summary>
/// The state Priesthood track (Phase 12 item 3; §6.2's "a lightweight parallel track... running
/// alongside [the local magistracy] ladder rather than folding into it"). §6.2 also names <c>
/// sacerdosPublicus</c> in its own data-model sketch (§10) as the baseline public temple-keeper role
/// this track sits above — but that role belongs to Companions &amp; Court Positions (§5.2 of that
/// document), which has no code anywhere in this repository yet (no <c>CourtPosition</c>/<c>Companion</c>
/// staffing system exists at all, unlike Local Magistracies' own Curia/Mint building gate, which at
/// least had a building type to name and skip). <c>SacerdosPublicus</c> is therefore omitted from this
/// enum entirely, matching <see cref="Magistracies.MagistracyOffice"/>'s own "omitted rather than
/// included-but-unreachable" precedent for the Rome-track offices that document's own item 2 declined
/// to build — nothing in this item can construct one, and adding a value with no code path to reach it
/// would misrepresent what's actually built.
/// </summary>
public enum PriesthoodOffice
{
    /// <summary>§4.2/§6.2 — reads Auspices at superior reliability (see <see
    /// cref="CommissionAuspicesCommand"/>); also engageable for a wider settlement's own major
    /// decisions per §6.2, though this item does not build that settlement-wide engagement path (no
    /// settlement-scale decision system reads an Auspices skew anywhere in this codebase yet — see
    /// <see cref="CommissionAuspicesCommand"/>'s own scope note).</summary>
    Augur,

    /// <summary>§6.2 — "a priest dedicated specifically to the household's own Patron Deity... the
    /// single strongest available multiplier on that deity's own domain bonus." The domain-bonus half
    /// of that sentence is not built (see <see cref="PatronDeity"/>'s own doc comment for why); the
    /// office itself, its Patron-Deity-matching gate, and its own monthly Favor/Dignitas trickle are.</summary>
    Flamen,

    /// <summary>§6.2's capstone — "the rare, prestige endpoint of this track," standing to Augur/Flamen
    /// "roughly as a Duumvir stands to a plain Decurion." Requires already holding an active Augur or
    /// Flamen seat, mirroring <see cref="Magistracies.MagistracyOffice.Duumvir"/>'s own "must already
    /// hold [a lower office] first" gate shape.</summary>
    Pontifex,
}
