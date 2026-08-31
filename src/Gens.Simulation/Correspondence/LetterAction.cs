namespace Gens.Simulation.Correspondence;

/// <summary>§5's nine correspondence actions — the six carried over from the first pass plus the
/// three real additions this pass (News &amp; Gossip, Written Instructions to a Distant Appointee,
/// Condolence or Congratulation). This item builds every value's data-model shape completely; the
/// actual game-logic payload for whichever action names a not-yet-built target system (<see
/// cref="DirectPlacedSpy"/> — Espionage; <see cref="EarlyCourtship"/> — Romance &amp; Seduction; <see
/// cref="WrittenInstructionsToDistantAppointee"/> — Companions &amp; Court Positions' Procurator) is a
/// minimal, honestly-labeled stub rather than a fabricated integration — see <see
/// cref="SendLetterCommands"/>'s own doc comment.</summary>
public enum LetterAction
{
    PetitionPatron,
    MaintainDistantRelationship,
    RemoteNegotiation,
    DirectPlacedSpy,
    FormalComplaintOrProvocation,
    EarlyCourtship,

    /// <summary>New this pass (§5) — a distant Character's own life update, the concrete realization
    /// of Events' own previously-flagged cross-reference.</summary>
    NewsAndGossip,

    /// <summary>New this pass (§5) — updating a Procurator's or Overseer's standing instructions
    /// remotely.</summary>
    WrittenInstructionsToDistantAppointee,

    /// <summary>New this pass (§5) — a modest, low-cost relationship/Dignitas gesture responding to a
    /// distant Character's own recorded life event.</summary>
    CondolenceOrCongratulation,
}

/// <summary>Classifies each <see cref="LetterAction"/> for §7's Oral Tradition Problem: whether it
/// carries genuinely substantive content a non-literate leadership structure can fail to translate to
/// paper at all (§7: "a substantive treaty negotiation... genuinely doesn't translate"), versus
/// routine social correspondence that always gets through regardless of the recipient's own
/// Correspondence Reachability (§7: "an ordinary trade letter... works fine"). This item's own
/// invented classification — §7 names the phenomenon but never enumerates which of the nine actions
/// count as "substantive"; the split below is this item's own defensible reading, openly disclosed as
/// invented like every other unsized figure this pass introduces.</summary>
public static class LetterActions
{
    public static bool IsSubstantive(LetterAction action) => action switch
    {
        LetterAction.PetitionPatron => true,
        LetterAction.RemoteNegotiation => true,
        LetterAction.DirectPlacedSpy => true,
        LetterAction.FormalComplaintOrProvocation => true,
        LetterAction.WrittenInstructionsToDistantAppointee => true,
        LetterAction.MaintainDistantRelationship => false,
        LetterAction.EarlyCourtship => false,
        LetterAction.NewsAndGossip => false,
        LetterAction.CondolenceOrCongratulation => false,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown letter action."),
    };
}
