using Gens.Simulation.Identity;
using Gens.Simulation.Regions;
using Gens.Simulation.Time;

namespace Gens.Simulation.Cultures;

/// <summary>
/// One entry of Cultures of the Known World's real, thirty-six-value roster (§17's <c>culture</c>
/// enum and <c>CultureCategory</c> data model). <see cref="Identity.Culture"/> is the existing phantom
/// type <see cref="Characters.Character.Culture"/> and <see cref="CultureDistributionEntry.CultureRef"/>
/// already reference; this is the first item to actually build the catalog those references resolve
/// against, closing both those files' own explicitly-named forward seams. Mirrors <see
/// cref="RegionProfileDefinition"/>'s "sealed record, constructor validates, content is data" shape.
/// </summary>
public sealed record CultureDefinition
{
    public CultureDefinition(
        DefinitionId<Identity.Culture> id,
        string name,
        DatedRule<CultureCategory> category,
        bool permanentlyUnconquered = false,
        bool isRaidingFrontier = false,
        bool isAuxiliaryServiceCulture = false,
        EncounterRarityTier encounterRarityTier = EncounterRarityTier.NotApplicable,
        bool noveltyDignitasBonus = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A culture definition requires a non-empty name.", nameof(name));
        if (category is null)
            throw new ArgumentNullException(nameof(category));
        if (category.BaseValue != CultureCategory.TradeContactOnly && encounterRarityTier != EncounterRarityTier.NotApplicable)
        {
            throw new ArgumentException(
                "Encounter rarity tier is only meaningful for a Trade Contact Only culture (§17).",
                nameof(encounterRarityTier));
        }
        if (category.BaseValue == CultureCategory.TradeContactOnly && encounterRarityTier == EncounterRarityTier.NotApplicable)
        {
            throw new ArgumentException(
                "A Trade Contact Only culture requires an encounter rarity tier (§17).",
                nameof(encounterRarityTier));
        }

        Id = id;
        Name = name;
        Category = category;
        PermanentlyUnconquered = permanentlyUnconquered;
        IsRaidingFrontier = isRaidingFrontier;
        IsAuxiliaryServiceCulture = isAuxiliaryServiceCulture;
        EncounterRarityTier = encounterRarityTier;
        NoveltyDignitasBonus = noveltyDignitasBonus;
    }

    public DefinitionId<Identity.Culture> Id { get; }
    public string Name { get; }

    /// <summary>§17's <c>category</c> + <c>categoryShiftYear</c> pair, expressed as a single date-aware
    /// <see cref="DatedRule{TValue}"/> instead of two separate fields — the same generalization item 1
    /// built for Reputation Duality's own mid-range shift, applied here to British (Frontier → Provincial,
    /// AD 43), Dacian (AD 106), Nabataean (AD 106), Egyptian (30 BC), and Pannonian (~AD 9).</summary>
    public DatedRule<CultureCategory> Category { get; }

    /// <summary>True only for Hibernian, Caledonian, and Nubian/Kushite (§17's own doc comment).</summary>
    public bool PermanentlyUnconquered { get; }

    /// <summary>True only for Blemmyes — a real raiding relationship rather than a state-to-state one (§7).</summary>
    public bool IsRaidingFrontier { get; }

    /// <summary>True only for Batavian and Cretan — Frontier or Provincial with a real, named
    /// military-recruitment relationship layered on top (§17).</summary>
    public bool IsAuxiliaryServiceCulture { get; }

    /// <summary>Meaningful only for a <see cref="CultureCategory.TradeContactOnly"/> culture (validated
    /// above).</summary>
    public EncounterRarityTier EncounterRarityTier { get; }

    /// <summary>§10.7 — a rare Character of this culture carries real Dignitas curiosity-value (a Slave
    /// Market novelty listing, a Travel Encounter). Set true for the six Trade Contact Only cultures in
    /// <see cref="KnownWorldCultures"/>; left available here for any future culture that might earn the
    /// same treatment without being Trade Contact Only.</summary>
    public bool NoveltyDignitasBonus { get; }

    /// <summary>This culture's category as of <paramref name="date"/> — the worked consumer of <see
    /// cref="Category"/>'s date-aware resolution, mirroring <see
    /// cref="RegionProfileDefinition.ReputationDualityAsOf"/>.</summary>
    public CultureCategory CategoryAsOf(GameDate date) => Category.EffectiveAsOf(date);
}
