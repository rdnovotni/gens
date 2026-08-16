namespace Gens.Simulation.Characters;

/// <summary>The three-tier wealth pyramid <c>gens-population-wealth-purchasing-power-design.md</c>
/// §2 defines, "mapped onto Settlement Demographics' existing pop groups" per that document's own
/// §9 data model. Fixed and code-defined, like <see cref="LegalStatus"/>, <see cref="SocialClass"/>,
/// and <see cref="DietTier"/>.</summary>
public enum WealthBand
{
    /// <summary>Real subsistence — the tier "the overwhelming majority of the population" sits in
    /// (§2).</summary>
    Subsistence,

    /// <summary>Real, if limited, discretionary spending — ordinary-standing Opifices/Negotiatores
    /// (§2).</summary>
    ModestSurplus,

    /// <summary>The narrow tier where most luxury-goods and imported-goods demand actually lives
    /// (§2).</summary>
    EliteDiscretionary,
}
