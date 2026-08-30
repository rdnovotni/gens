using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Scandal;

/// <summary>Versioned constants for Phase 12 item 7's Scandal mechanics
/// (<c>gens-scandal-design.md</c>), matching <see cref="Crime.CrimeCatalog"/>'s identical "unsized
/// against real playtest data, but named in one place" convention — §12's Open Questions explicitly
/// leaves "all numeric sizing... Scandal decay rates, the ambient-spread-to-Scope threshold, and
/// Suppression/Spin's own effectiveness curves" unsized.</summary>
public static class ScandalCatalog
{
    /// <summary>§7's ordinary-case Dignitas penalty, "scaled to severity and scope" — scope itself is
    /// never anything but <see cref="ScandalScope.SettlementWide"/> in this item (see <see
    /// cref="ScandalRecord"/>'s own doc comment), so only <see cref="ScandalSeverity"/> actually drives
    /// this scale here.</summary>
    public const int MinorEmbarrassmentDignitasPenalty = 8;
    public const int PublicDisgraceDignitasPenalty = 25;
    public const int NotaCensoriaEligibleDignitasPenalty = 45;

    /// <summary>§7's "a relationship-web scar across everyone connected to the matter" — this item's
    /// own narrower, real slice of that: a scar between the scandalized household's own recorded head
    /// and a specific, named other party, when <see cref="RecordScandalCommand.ScarredAgainstCharacterId"/>
    /// actually names one (matching <see cref="Crime.ApplySentenceCommand"/>'s identical "only has
    /// somewhere real to land when a specific... Character is named" precedent). Already negative,
    /// matching <see cref="Legal.LegalCatalog.RelationshipScarOpinionDelta"/>'s identical sign
    /// convention — passed directly, never negated a second time.</summary>
    public const int RelationshipScarOpinionDelta = -20;

    /// <summary>§7's Faction-dependent reception (§10's own reuse of Politics &amp; Patronage §3.1's
    /// <see cref="Clientela.CharacterFactionAlignment"/>): when the scandalized household's own recorded
    /// head carries a real, recorded Faction, that audience reads its own member's disgrace more
    /// harshly than the other audience does — a real, if simple, "we expected better of our own"
    /// hypocrisy reading, rather than the uniform severity-only reading a head with no recorded Faction
    /// (nearly everyone — §3.1's own "the political cast" is a narrow slice) still gets from both
    /// audiences alike.</summary>
    public const int FactionAlignedReadingPenalty = 10;

    /// <summary>§7: the Scandal-Marked Trait is reserved for "a sufficiently severe or public case" —
    /// <see cref="ScandalSeverity.MinorEmbarrassment"/> alone never grants it, matching this document's
    /// own severity ladder framing ("a minor embarrassment... through a genuine public disgrace").</summary>
    public static readonly DefinitionId<Trait> ScandalMarkedTraitId = Legal.LegalCatalog.ScandalMarkedTraitId;

    /// <summary>§8's Rehabilitation payoff: the Reactive Trait counterpart to <see
    /// cref="ScandalMarkedTraitId"/>, authored directly into <c>content/source/traits/legal.json</c>
    /// alongside it (this item's own concrete trigger for §8's "a real, sustained stretch without
    /// further incident" — see <see cref="ScandalRehabilitationSystem"/>).</summary>
    public static readonly DefinitionId<Trait> RehabilitatedTraitId = new("rehabilitated");

    /// <summary>§8/§9: the age gate <see cref="ScandalRehabilitationSystem"/> checks against the
    /// scandalized household's own most recent <see cref="ScandalRecord"/> — a "further incident"
    /// resets this clock entirely, matching <see cref="Reputation.FavorExpirationSystem"/>'s identical
    /// age-gated-lapse shape.</summary>
    public const int RehabilitationAfterMonths = 36;

    /// <summary>§9's Scandal Lifecycle: "an ordinary Scandal's own felt severity fades over time if not
    /// actively refreshed by a further incident" — <see cref="ScandalDecaySystem"/>'s own age gate for
    /// stepping a still-active record's <see cref="ScandalSeverity"/> down one rung, matching <see
    /// cref="Reputation.FavorExpirationSystem"/>'s identical age-gated shape.</summary>
    public const int SeverityFadeAfterMonths = 18;

    /// <summary>§9: "eventually settling into background Dynasty Chronicle memory rather than an
    /// active, ongoing penalty" — the further age gate, past <see cref="SeverityFadeAfterMonths"/>, at
    /// which <see cref="ScandalDecaySystem"/> finally sets <see cref="ScandalRecord.IsActive"/> false.</summary>
    public const int DeactivateAfterMonths = 42;
}
