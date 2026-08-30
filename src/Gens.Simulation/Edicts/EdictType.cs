namespace Gens.Simulation.Edicts;

/// <summary>§5's eight Edict types (<c>gens-policies-edicts-design.md</c>) — every named type is
/// represented, matching <see cref="Legal.LegalCase.CaseType"/>'s and <see
/// cref="Scandal.ScandalRecord.SourceType"/>'s own "every real category represented, only some
/// reachable" precedent. Only <see cref="ManumissionEdict"/>, <see cref="CitizenshipGrant"/>, and <see
/// cref="Proscription"/> are ever actually issuable by this item — see <see
/// cref="IssueManumissionEdictCommand"/>, <see cref="GrantCitizenshipEdictCommand"/>, and <see
/// cref="IssueProscriptionCommand"/> for why each of the other five stays unreachable: <see
/// cref="TabulaeNovae"/> and <see cref="DebtBondageBan"/> both need Economy &amp; Finance's own
/// <c>DebtRecord</c>/debt-bondage machinery in a way this item's own household-vs-household Edict
/// engine does not reach into (a real, if narrow, future integration point, not a blocked one); <see
/// cref="GeneralAmnesty"/> needs a real "pardon a standing sentence" write path onto Phase 12 item 5's
/// own already-shipped, already-tested <c>SentenceRecord</c>/<c>DetentionRecord</c> commands, which
/// reopening is out of this item's scope (matching Phase 12 item 1's own "already-shipped,
/// already-tested, out of scope to reopen" precedent for <c>Agnomen.DignitasEffect</c>); <see
/// cref="LandRedistribution"/> needs Land Ownership &amp; Real Estate's own <c>PropertyRecord</c> type,
/// confirmed by direct search not to exist anywhere in this codebase (Phase 12 item 6's own identical
/// finding for the Collegia Schola); <see cref="GrainRequisition"/> needs a real Coloni harvest/
/// Contentment write path this item does not reach into either, the same unreached-consumer shape
/// Phase 12 item 3's own Sacred Calendar left for Settlement Demographics.</summary>
public enum EdictType
{
    TabulaeNovae,
    GeneralAmnesty,
    LandRedistribution,
    ManumissionEdict,
    CitizenshipGrant,
    Proscription,
    DebtBondageBan,
    GrainRequisition,
}
