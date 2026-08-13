using Gens.Simulation.Identity;

namespace Gens.Simulation.Characters;

/// <summary>The runtime-side mirror of one compiled Trait content definition
/// (<c>content/schemas/traits.schema.json</c>; <c>gens-traits-design.md</c> §10's Data Model) — just
/// enough of the authored shape for <see cref="TraitCatalog"/> to enforce opposed-pair and tiered-
/// spectrum exclusivity and to compute Personality Axis scores. A <see cref="Character"/> itself never
/// embeds one of these; it only ever holds a bare <see cref="DefinitionId{T}"/> reference (rule 10,
/// "content is data, rules are code") — this type exists so code that already has the compiled
/// catalog in hand can reason about what a referenced trait actually means.</summary>
public readonly record struct TraitDefinition
{
    public TraitDefinition(
        DefinitionId<Trait> id,
        TraitCategory category,
        PersonalityAxis? axis = null,
        int? axisNudge = null,
        DefinitionId<Trait>? opposes = null,
        string? spectrum = null,
        int? tierPosition = null)
    {
        if (axis.HasValue != axisNudge.HasValue)
            throw new ArgumentException(
                "'axis' and 'axisNudge' must be supplied together, or not at all (traits.schema.json's dependentRequired).",
                nameof(axisNudge));
        if (axisNudge is 0)
            throw new ArgumentOutOfRangeException(nameof(axisNudge), axisNudge, "An 'axisNudge' of zero is not a nudge; omit 'axis' instead.");
        if (axisNudge is < -25 or > 25)
            throw new ArgumentOutOfRangeException(nameof(axisNudge), axisNudge, "'axisNudge' must be between -25 and 25.");
        if (opposes.HasValue && opposes.Value == id)
            throw new ArgumentException("A trait cannot oppose itself.", nameof(opposes));
        if (opposes.HasValue && spectrum is not null)
            throw new ArgumentException(
                "A trait cannot both 'oppose' another trait and belong to a tiered 'spectrum' — these are two distinct exclusivity mechanisms (gens-traits-design.md §3 vs. Characters §4).",
                nameof(spectrum));
        if (spectrum is not null != tierPosition.HasValue)
            throw new ArgumentException(
                "'spectrum' and 'tierPosition' must be supplied together, or not at all.",
                nameof(tierPosition));
        if (tierPosition is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(tierPosition), tierPosition, "'tierPosition' must be between 0 and 3.");

        Id = id;
        Category = category;
        Axis = axis;
        AxisNudge = axisNudge;
        Opposes = opposes;
        Spectrum = spectrum;
        TierPosition = tierPosition;
    }

    public DefinitionId<Trait> Id { get; }
    public TraitCategory Category { get; }

    /// <summary>Which Personality Axis this trait nudges, if any (many traits, e.g. Patient/Impatient,
    /// carry no Axis nudge at all — a purely bespoke-effect trait).</summary>
    public PersonalityAxis? Axis { get; }

    /// <summary>Signed nudge applied to <see cref="Axis"/> when this trait is held — positive toward
    /// that axis's Pole A, negative toward Pole B (<see cref="PersonalityAxis"/>). Always non-null
    /// exactly when <see cref="Axis"/> is.</summary>
    public int? AxisNudge { get; }

    /// <summary>The other trait this one mutually excludes (<c>gens-traits-design.md</c> Characters
    /// §4: "mutually-exclusive opposed pairs"). Null for standalone traits and for tiered-spectrum
    /// members, which use <see cref="Spectrum"/> instead.</summary>
    public DefinitionId<Trait>? Opposes { get; }

    /// <summary>The tiered spectrum this trait is one rung of (<c>gens-traits-design.md</c> §3), e.g.
    /// <c>"piety"</c>. Every trait sharing the same <see cref="Spectrum"/> value mutually excludes
    /// every other — a Character holds at most one tag from the group, not just from a single pair.</summary>
    public string? Spectrum { get; }

    /// <summary>This trait's rung within <see cref="Spectrum"/> (0-3, low to high per <c>
    /// gens-traits-design.md</c> §3's four-tier spectrums). Null unless <see cref="Spectrum"/> is set.</summary>
    public int? TierPosition { get; }
}
