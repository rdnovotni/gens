using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;

namespace Gens.Simulation.Companions;

/// <summary>The <see cref="CoreAttributes"/> axis a <see cref="CompanionsCatalog"/> lookup resolves
/// against — <see cref="CoreAttributes"/> itself is an unlabeled five-<c>int</c> struct (matching <see
/// cref="LaborSkills"/>'s own shape), so a role/title table needs a small enum to name which axis it
/// reads, the same role this file's own <see cref="ValueOf"/> plays that <see
/// cref="DutySlotCatalog.RelevantSkillValue"/> already plays for <see cref="LaborSkills"/>.</summary>
public enum CoreAttributeAxis
{
    Diplomacy,
    Martial,
    Stewardship,
    Intrigue,
    Learning,
}

/// <summary>
/// Untuned baseline constants and lookup tables for Phase 17 item 1's Companions &amp; Court Positions
/// tiers (<c>gens-companions-court-positions-design.md</c> §4-§7, whose own §8 "Open Questions" leaves
/// every numeric threshold below unsized) — this implementation's own invented figures, matching <see
/// cref="Magistracies.MagistracyCatalog"/>'s and <see
/// cref="Stewardship.StewardIncidentCatalog"/>'s identical "unsized against real playtest data, but
/// named in one place" disclaimer.
///
/// <b>Building category:</b> §4's "the building's own category must match the role" gate is checked
/// here against <see cref="BuildingSector"/> — no dedicated <c>BuildingCategory</c> type exists
/// anywhere in <see cref="Gens.Simulation.Buildings"/> (only <see cref="BuildingSector"/>, <see
/// cref="BuildingTier"/>, and content-authored <c>DefinitionId&lt;Building&gt;</c> identity), so <see
/// cref="CategoryOf"/> resolves each <see cref="OverseerRole"/> to the closest matching <see
/// cref="BuildingSector"/> instead of a type this codebase does not have. This is a deliberate wiring
/// deviation from the design agent's assumed shape, not a scope cut — the mismatch gate itself is
/// still fully checked, just against the real category concept this codebase already has.
/// </summary>
public static class CompanionsCatalog
{
    /// <summary>The <see cref="CoreAttributes"/> floor <see cref="AppointOverseerCommand"/> requires
    /// (§6: "a qualitative bar... roughly modest").</summary>
    public const int OverseerAttributeThreshold = 40;

    /// <summary>The <see cref="Condition.Loyalty"/> floor <see cref="AppointOverseerCommand"/>
    /// requires (§6).</summary>
    public const int OverseerLoyaltyThreshold = 40;

    /// <summary>The <see cref="CoreAttributes"/> floor <see cref="AppointSeniorPositionCommand"/>
    /// requires — higher than <see cref="OverseerAttributeThreshold"/>, per §6's "broader standing...
    /// track record... push" framing for the higher tier.</summary>
    public const int SeniorPositionAttributeThreshold = 55;

    /// <summary>The <see cref="Condition.Loyalty"/> floor <see cref="AppointSeniorPositionCommand"/>
    /// requires — higher than <see cref="OverseerLoyaltyThreshold"/>, same §6 reasoning.</summary>
    public const int SeniorPositionLoyaltyThreshold = 55;

    /// <summary>The monthly Dignitas trickle <see cref="RationalisBonusSystem"/> applies while a
    /// household's Treasurer, Argentarius (Overseer tier), Institor Maximus, and Cellarer are all
    /// concurrently active and present (§5.3's Rationalis cluster bonus).</summary>
    public const int RationalisClusterBonusDignitas = 3;

    /// <summary>The largest retinue <see cref="Travel.BeginTravelCommand"/> accepts — §7's own open
    /// "capacity-limited" question, resolved here with an invented flat cap rather than left unbounded.</summary>
    public const int TravelRetinueCapacity = 4;

    /// <summary>The <see cref="BuildingSector"/> each <see cref="OverseerRole"/>'s own workplace must
    /// belong to — see this class's own doc comment for why <see cref="BuildingSector"/> stands in for
    /// the design doc's unbuilt <c>BuildingCategory</c> concept. Every civic/household role with no
    /// obviously matching sector (e.g. <see cref="OverseerRole.Rhetor"/>, a tutor) resolves to <see
    /// cref="BuildingSector.None"/>, the sector most ordinary household/civic buildings already carry.</summary>
    public static BuildingSector CategoryOf(OverseerRole role) => role switch
    {
        OverseerRole.Vilicus => BuildingSector.Agriculture,
        OverseerRole.MagisterOfficinae => BuildingSector.Industry,
        OverseerRole.Metallarius => BuildingSector.Industry,
        OverseerRole.Institor => BuildingSector.Commerce,
        OverseerRole.Portitor => BuildingSector.Commerce,
        OverseerRole.Aquarius => BuildingSector.Industry,
        OverseerRole.Horrearius => BuildingSector.Agriculture,
        OverseerRole.SacerdosPublicus => BuildingSector.Religion,
        OverseerRole.Rhetor => BuildingSector.None,
        OverseerRole.Lanista => BuildingSector.None,
        OverseerRole.Editor => BuildingSector.None,
        OverseerRole.Valetudinarius => BuildingSector.None,
        OverseerRole.Venalicius => BuildingSector.Commerce,
        OverseerRole.LenoLena => BuildingSector.Commerce,
        OverseerRole.Argentarius => BuildingSector.Commerce,
        OverseerRole.Vigil => BuildingSector.None,
        OverseerRole.Alimentarius => BuildingSector.Agriculture,
        OverseerRole.Libitinarius => BuildingSector.None,
        OverseerRole.Navarchus => BuildingSector.Commerce,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown Overseer role."),
    };

    /// <summary>The primary <see cref="CoreAttributeAxis"/> <see cref="AppointOverseerCommand"/> checks
    /// against <see cref="CompanionsCatalog.OverseerAttributeThreshold"/> for each role — §6 never
    /// names one per role, so this implementation picks the attribute most obviously mapping to each
    /// role's day-to-day competence, matching <see cref="Magistracies.MagistracyCatalog"/>'s own
    /// "picking a concrete default where the design doc leaves an open slot" precedent.</summary>
    public static CoreAttributeAxis AttributeOf(OverseerRole role) => role switch
    {
        OverseerRole.Vilicus => CoreAttributeAxis.Stewardship,
        OverseerRole.MagisterOfficinae => CoreAttributeAxis.Stewardship,
        OverseerRole.Metallarius => CoreAttributeAxis.Stewardship,
        OverseerRole.Institor => CoreAttributeAxis.Diplomacy,
        OverseerRole.Portitor => CoreAttributeAxis.Diplomacy,
        OverseerRole.Aquarius => CoreAttributeAxis.Stewardship,
        OverseerRole.Horrearius => CoreAttributeAxis.Stewardship,
        OverseerRole.SacerdosPublicus => CoreAttributeAxis.Learning,
        OverseerRole.Rhetor => CoreAttributeAxis.Learning,
        OverseerRole.Lanista => CoreAttributeAxis.Martial,
        OverseerRole.Editor => CoreAttributeAxis.Diplomacy,
        OverseerRole.Valetudinarius => CoreAttributeAxis.Learning,
        OverseerRole.Venalicius => CoreAttributeAxis.Diplomacy,
        OverseerRole.LenoLena => CoreAttributeAxis.Intrigue,
        OverseerRole.Argentarius => CoreAttributeAxis.Stewardship,
        OverseerRole.Vigil => CoreAttributeAxis.Martial,
        OverseerRole.Alimentarius => CoreAttributeAxis.Stewardship,
        OverseerRole.Libitinarius => CoreAttributeAxis.Stewardship,
        OverseerRole.Navarchus => CoreAttributeAxis.Martial,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown Overseer role."),
    };

    /// <summary>A secondary <see cref="CoreAttributeAxis"/> a handful of roles also lean on — informational
    /// only (not currently gated by <see cref="AppointOverseerCommand"/>'s own validation, which checks
    /// <see cref="AttributeOf"/> alone); <c>null</c> for every role with no obvious second axis.</summary>
    public static CoreAttributeAxis? SecondaryAttributeOf(OverseerRole role) => role switch
    {
        OverseerRole.Argentarius => CoreAttributeAxis.Diplomacy,
        OverseerRole.LenoLena => CoreAttributeAxis.Diplomacy,
        OverseerRole.Navarchus => CoreAttributeAxis.Stewardship,
        OverseerRole.Institor => CoreAttributeAxis.Stewardship,
        _ => null,
    };

    /// <summary>Villa-scope (§5.1) vs. estate/settlement-scope (§5.2), per <see
    /// cref="SeniorPositionTitle"/>'s own doc comment.</summary>
    public static SeniorPositionScope ScopeOf(SeniorPositionTitle title) => title switch
    {
        SeniorPositionTitle.Steward or SeniorPositionTitle.Bodyguard or SeniorPositionTitle.HouseholdPriest or
            SeniorPositionTitle.Secretary or SeniorPositionTitle.Cellarer or SeniorPositionTitle.HeadCook or
            SeniorPositionTitle.Tutor or SeniorPositionTitle.Nurse or SeniorPositionTitle.WeavingMistress or
            SeniorPositionTitle.MasterOfHospitality or SeniorPositionTitle.Balneator or SeniorPositionTitle.Chamberlain or
            SeniorPositionTitle.HouseholdSpymaster or SeniorPositionTitle.GuardCaptain or SeniorPositionTitle.CourtPhysician or
            SeniorPositionTitle.Treasurer or SeniorPositionTitle.MasterBeekeeper or SeniorPositionTitle.FurnaceMaster or
            SeniorPositionTitle.MenagerieKeeper or SeniorPositionTitle.HarborSteward or SeniorPositionTitle.DovecoteKeeper or
            SeniorPositionTitle.BakerInChief or SeniorPositionTitle.GranaryKeeper or SeniorPositionTitle.Curator or
            SeniorPositionTitle.Ergastularius or SeniorPositionTitle.Symposiarch => SeniorPositionScope.Villa,
        SeniorPositionTitle.Actor or SeniorPositionTitle.InstitorMaximus or SeniorPositionTitle.PraefectusMetallorum or
            SeniorPositionTitle.PraefectusVigilum or SeniorPositionTitle.NavarchusPrincipes or SeniorPositionTitle.EditorMuneris or
            SeniorPositionTitle.Tabularius or SeniorPositionTitle.Procurator or
            SeniorPositionTitle.Rationalis => SeniorPositionScope.EstateOrSettlement,
        _ => throw new ArgumentOutOfRangeException(nameof(title), title, "Unknown Senior Position title."),
    };

    /// <summary>Whether <paramref name="title"/> is tied to one specific room/building within the
    /// villa (§5.1's "oversees-style cluster relevant only to a few titles") rather than the household
    /// as a whole. Always false for every <see cref="SeniorPositionScope.EstateOrSettlement"/> title —
    /// none of §5.2's roles name a single tied building.</summary>
    public static bool IsRoomTied(SeniorPositionTitle title) => title switch
    {
        SeniorPositionTitle.Cellarer => true,
        SeniorPositionTitle.HeadCook => true,
        SeniorPositionTitle.WeavingMistress => true,
        SeniorPositionTitle.Balneator => true,
        SeniorPositionTitle.MasterBeekeeper => true,
        SeniorPositionTitle.FurnaceMaster => true,
        SeniorPositionTitle.MenagerieKeeper => true,
        SeniorPositionTitle.HarborSteward => true,
        SeniorPositionTitle.DovecoteKeeper => true,
        SeniorPositionTitle.BakerInChief => true,
        SeniorPositionTitle.GranaryKeeper => true,
        SeniorPositionTitle.Ergastularius => true,
        SeniorPositionTitle.Steward => false,
        SeniorPositionTitle.Bodyguard => false,
        SeniorPositionTitle.HouseholdPriest => false,
        SeniorPositionTitle.Secretary => false,
        SeniorPositionTitle.Tutor => false,
        SeniorPositionTitle.Nurse => false,
        SeniorPositionTitle.MasterOfHospitality => false,
        SeniorPositionTitle.Chamberlain => false,
        SeniorPositionTitle.HouseholdSpymaster => false,
        SeniorPositionTitle.GuardCaptain => false,
        SeniorPositionTitle.CourtPhysician => false,
        SeniorPositionTitle.Treasurer => false,
        SeniorPositionTitle.Curator => false,
        SeniorPositionTitle.Symposiarch => false,
        SeniorPositionTitle.Actor => false,
        SeniorPositionTitle.InstitorMaximus => false,
        SeniorPositionTitle.PraefectusMetallorum => false,
        SeniorPositionTitle.PraefectusVigilum => false,
        SeniorPositionTitle.NavarchusPrincipes => false,
        SeniorPositionTitle.EditorMuneris => false,
        SeniorPositionTitle.Tabularius => false,
        SeniorPositionTitle.Procurator => false,
        SeniorPositionTitle.Rationalis => false,
        _ => throw new ArgumentOutOfRangeException(nameof(title), title, "Unknown Senior Position title."),
    };

    /// <summary>The primary <see cref="CoreAttributeAxis"/> <see cref="AppointSeniorPositionCommand"/>
    /// checks against <see cref="SeniorPositionAttributeThreshold"/> for each title — same "pick a
    /// concrete default" reasoning as <see cref="AttributeOf(OverseerRole)"/>.</summary>
    public static CoreAttributeAxis AttributeOf(SeniorPositionTitle title) => title switch
    {
        SeniorPositionTitle.Steward => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Bodyguard => CoreAttributeAxis.Martial,
        SeniorPositionTitle.HouseholdPriest => CoreAttributeAxis.Learning,
        SeniorPositionTitle.Secretary => CoreAttributeAxis.Learning,
        SeniorPositionTitle.Cellarer => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.HeadCook => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Tutor => CoreAttributeAxis.Learning,
        SeniorPositionTitle.Nurse => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.WeavingMistress => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.MasterOfHospitality => CoreAttributeAxis.Diplomacy,
        SeniorPositionTitle.Balneator => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Chamberlain => CoreAttributeAxis.Diplomacy,
        SeniorPositionTitle.HouseholdSpymaster => CoreAttributeAxis.Intrigue,
        SeniorPositionTitle.GuardCaptain => CoreAttributeAxis.Martial,
        SeniorPositionTitle.CourtPhysician => CoreAttributeAxis.Learning,
        SeniorPositionTitle.Treasurer => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.MasterBeekeeper => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.FurnaceMaster => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.MenagerieKeeper => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.HarborSteward => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.DovecoteKeeper => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.BakerInChief => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.GranaryKeeper => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Curator => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Ergastularius => CoreAttributeAxis.Martial,
        SeniorPositionTitle.Symposiarch => CoreAttributeAxis.Diplomacy,
        SeniorPositionTitle.Actor => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.InstitorMaximus => CoreAttributeAxis.Diplomacy,
        SeniorPositionTitle.PraefectusMetallorum => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.PraefectusVigilum => CoreAttributeAxis.Martial,
        SeniorPositionTitle.NavarchusPrincipes => CoreAttributeAxis.Martial,
        SeniorPositionTitle.EditorMuneris => CoreAttributeAxis.Diplomacy,
        SeniorPositionTitle.Tabularius => CoreAttributeAxis.Learning,
        SeniorPositionTitle.Procurator => CoreAttributeAxis.Stewardship,
        SeniorPositionTitle.Rationalis => CoreAttributeAxis.Stewardship,
        _ => throw new ArgumentOutOfRangeException(nameof(title), title, "Unknown Senior Position title."),
    };

    /// <summary>Reads the <see cref="CoreAttributes"/> value <paramref name="attribute"/> names —
    /// matching <see cref="DutySlotCatalog.RelevantSkillValue"/>'s identical "name the axis, then read
    /// it" pattern for <see cref="LaborSkills"/>.</summary>
    public static int ValueOf(CoreAttributes attributes, CoreAttributeAxis attribute) => attribute switch
    {
        CoreAttributeAxis.Diplomacy => attributes.Diplomacy,
        CoreAttributeAxis.Martial => attributes.Martial,
        CoreAttributeAxis.Stewardship => attributes.Stewardship,
        CoreAttributeAxis.Intrigue => attributes.Intrigue,
        CoreAttributeAxis.Learning => attributes.Learning,
        _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown Core Attribute."),
    };
}
