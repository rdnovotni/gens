using Gens.Simulation.Chronicle;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Epithets;

/// <summary>
/// A whole gens's own informal, reputation-based epithet (Phase 11 item 5; <c>gens-epithets-nicknames-
/// titles-design.md</c> §6/§9's <c>DynasticEpithet</c> data model) — distinct from §5's inherited
/// cognomen (a real, formal naming-convention change) and from Policies &amp; Edicts' Hybrid Doctrine
/// naming (a philosophy label): flavor-tier text read directly off a sustained pattern of Dynasty
/// Chronicle entries, never a formal part of anyone's actual name. Sparse, keyed by household, and
/// overwritten (remove then re-add) as the underlying pattern changes — matching <see
/// cref="Funerary.MemoriaState"/>'s identical "no entry means nothing earned yet" convention.
/// </summary>
/// <param name="DerivedFromChronicleEntryIds">Every Major/Legendary <see cref="ChronicleEntry"/> this
/// text was computed from at the time it was last set — §9's own provenance requirement, so the text is
/// never free text with no traceable source.</param>
public sealed record DynasticEpithet(
    RuntimeId<Household> HouseholdId,
    string EpithetText,
    IReadOnlyList<RuntimeId<ChronicleEntry>> DerivedFromChronicleEntryIds);

/// <summary>This item's own invented sizing/mapping for §10's "all numeric sizing... unsized" gap,
/// applied to §6's dynastic epithet — the design doc names no threshold or template set.</summary>
public static class DynasticEpithetCatalog
{
    /// <summary>How many of a household's own accumulated Major/Legendary <see cref="ChronicleEntry"/>
    /// records it takes before that household earns a nameable reputation shorthand at all.</summary>
    public const int MinimumMajorOrLegendaryEntries = 5;

    /// <summary>A deterministic prose template per <see cref="ChronicleCategory"/> — §6's "the house
    /// that held the Rhine" texture, generated from whichever category is most common among the
    /// household's own qualifying entries (ties broken by this enum's own declaration order, matching
    /// every other deterministic-tiebreak convention in this codebase).</summary>
    public static string TemplateFor(ChronicleCategory dominantCategory) => dominantCategory switch
    {
        ChronicleCategory.BirthsAndDeaths => "the ever-enduring house",
        ChronicleCategory.MarriagesAndFamily => "the house of many alliances",
        ChronicleCategory.PoliticsAndOffice => "the house of high office",
        ChronicleCategory.WarAndCombat => "the house that held the line",
        ChronicleCategory.WealthAndBuilding => "the house of stone and gold",
        ChronicleCategory.FaithAndScandal => "the house under watching eyes",
        _ => "a house of note",
    };
}
