using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§7's two-tier catalog. Real, historically documented Roman legal categories — this item's
/// own tier a sentenced Character actually belongs to is read directly off <see
/// cref="Character.SocialClass"/>/<see cref="Character.LegalStatus"/> by <see
/// cref="SentenceTierResolver"/>, not chosen freely by the caller.</summary>
public enum SentenceTier
{
    Honestiores,
    Humiliores,
}

/// <summary>§7's full sentence vocabulary — every real design-doc value is represented (matching <see
/// cref="Legal.LegalSentence"/>'s own "schema completeness first" precedent), but only <see
/// cref="Fine"/>, <see cref="Relegatio"/>, <see cref="Deportatio"/>, <see cref="HonorableExit"/>, and
/// <see cref="Crucifixion"/> are ever actually applied by <see cref="ApplySentenceCommand"/> — see that
/// command's own doc comment for exactly which ones and why. <see cref="Ignominia"/>, <see
/// cref="Flogging"/>, <see cref="DamnatioAdMetalla"/>, <see cref="ServusPoenae"/>, and <see
/// cref="DamnatioAdBestias"/> are kept modeled-but-unreached, matching <see
/// cref="Legal.LegalSentence.DebtBondage"/>'s own precedent for a design-doc value a future pass
/// deliberately wires: <see cref="DamnatioAdBestias"/> specifically is named directly by §7/§11 as
/// "resolved [in Games &amp; Spectacle] exactly as that document already specifies rather than
/// redefined here," and Games &amp; Spectacle (Phase 17) doesn't exist in this codebase yet.</summary>
public enum SentenceType
{
    // Honestiores.
    Fine,
    Relegatio,
    Deportatio,
    Ignominia,
    HonorableExit,

    // Humiliores.
    Flogging,
    DamnatioAdMetalla,
    ServusPoenae,
    DamnatioAdBestias,
    Crucifixion,
}

/// <summary>
/// One sentence carried out against a Character (Phase 12 item 5; §7-§8). Kept forever once applied,
/// matching <see cref="Legal.LegalCase"/>'s identical "kept for the campaign's lifetime" convention —
/// §8's own Chronicle-worthiness for "every execution" needs the full record, not just the live
/// consequence.
/// </summary>
public sealed record SentenceRecord(
    RuntimeId<SentenceRecord> SentenceId,
    RuntimeId<Character> CharacterId,
    SentenceTier Tier,
    SentenceType Type,
    bool WasJustified,
    GameDate AppliedDate,
    RuntimeId<LegalCase>? SourceLegalCaseId = null);

/// <summary>§7's honestiores/humiliores split, read directly off a Character's own already-modeled
/// status rather than a second, parallel classification this item would have to keep in sync:
/// senators/equestrians/decurions/soldiers of real established standing are <see
/// cref="SentenceTier.Honestiores"/>, everyone else <see cref="SentenceTier.Humiliores"/>.</summary>
public static class SentenceTierResolver
{
    public static SentenceTier TierFor(Character character) =>
        character.SocialClass is SocialClass.Senatorial or SocialClass.Equestrian
            ? SentenceTier.Honestiores
            : SentenceTier.Humiliores;
}
