using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>Why a <see cref="Characters.Legitimacy"/>-affecting birth might later need a distinct
/// acknowledgement path (<c>gens-romance-sexuality-lineage-design.md</c> §9, §10). Kept here purely as
/// a doc-comment cross-reference point — the enum's own values live on <see
/// cref="Characters.Legitimacy"/>, this record does not duplicate them.</summary>
public enum PregnancyLegitimacyStatus
{
    Legitimate,
    Illegitimate,
    Acknowledged,
}

/// <summary>
/// One conception-to-resolution pregnancy (<c>gens-romance-sexuality-lineage-design.md</c> §9):
/// created by <see cref="ConceptionSystem"/>, resolved nine months later by <see
/// cref="ChildbirthResolutionSystem"/>. <see cref="PregnancyLegitimacyStatus"/> is not stored here —
/// the newborn's own <see cref="Characters.Legitimacy"/> (computed by the existing <see
/// cref="Characters.BirthCharacterCommands"/> pipeline from <see
/// cref="Characters.Character.CurrentSpouseId"/>, unchanged by this item) is the single source of
/// truth for that question once the child actually exists; this record only tracks the pregnancy
/// itself, before and at the moment of resolution.
/// </summary>
/// <param name="ConceivedViaBondType">The <see cref="RomanticBondType"/> the conceiving <see
/// cref="RomanticBond"/> held at conception (Marriage, Concubinage, or Affair — <see
/// cref="ConceptionSystem"/> never rolls conception for any other type) — kept for provenance even
/// though the newborn's real Legitimacy is derived independently at birth.</param>
/// <param name="Resolved">Whether <see cref="ChildbirthResolutionSystem"/> has already processed this
/// pregnancy's due date (success, maternal death, infant loss, or the mother having died beforehand of
/// an unrelated cause) — the pregnancy is inert once <c>true</c>.</param>
/// <param name="MaternalRiskResolved">Whether the maternal-death risk roll (or its <see
/// cref="RomanceContentSettings.FertilityRiskAbstracted"/> skip) has already been applied.</param>
/// <param name="InfantRiskResolved">Whether the infant-death risk roll (or its abstracted skip) has
/// already been applied.</param>
/// <param name="BornChildId">The newborn's ID once a live child was actually delivered; <c>null</c> if
/// the pregnancy resolved without a surviving child (both mother and infant lost, or the mother died
/// of an unrelated cause before term).</param>
public sealed record PregnancyRecord(
    RuntimeId<PregnancyRecord> PregnancyId,
    RuntimeId<Character> MotherId,
    RuntimeId<Character> FatherId,
    RomanticBondType ConceivedViaBondType,
    GameDate ConceivedDate,
    GameDate DueDate,
    bool Resolved,
    bool MaternalRiskResolved,
    bool InfantRiskResolved,
    RuntimeId<Character>? BornChildId)
{
    /// <summary>The only supported construction path for a freshly conceived pregnancy — stamps <see
    /// cref="DueDate"/> as <see cref="RomanceCatalog.PregnancyTermMonths"/> months after <paramref
    /// name="conceivedDate"/> (§9's gestation length) and leaves every resolution flag false.</summary>
    public static PregnancyRecord Create(
        RuntimeId<PregnancyRecord> id,
        RuntimeId<Character> motherId,
        RuntimeId<Character> fatherId,
        RomanticBondType conceivedVia,
        GameDate conceivedDate) =>
        new(
            id, motherId, fatherId, conceivedVia, conceivedDate,
            new GameDate(checked(conceivedDate.TotalMonths + RomanceCatalog.PregnancyTermMonths)),
            Resolved: false, MaternalRiskResolved: false, InfantRiskResolved: false, BornChildId: null);
}
