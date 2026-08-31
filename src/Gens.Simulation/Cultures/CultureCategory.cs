namespace Gens.Simulation.Cultures;

/// <summary>§2 and §17's four real categories a culture can hold. The data model's own enum is
/// exactly these five values (Contested Buffer split out from Great Power) — "Client" (Numidian/Mauri,
/// Cappadocian, Judaean, Nabataean, Egyptian, Bosporan per §12's own quick-reference column) and
/// "Independent" (Nubian/Kushite) are real relationship labels the source prose uses but neither is its
/// own enum value in §17's own <c>CultureCategory</c> data model — a Client culture resolves as <see
/// cref="Provincial"/> here (already administratively bound for mechanical purposes, per §1's "no new
/// mechanics" framing) and Nubian/Kushite resolves as <see cref="Frontier"/> with <see
/// cref="CultureDefinition.PermanentlyUnconquered"/> set, matching Hibernian and Caledonian's identical
/// "Frontier, permanently" treatment (§17's own doc comment naming exactly these three).</summary>
public enum CultureCategory
{
    /// <summary>Already within Roman administration for most or all of the range, including a culture
    /// already fully absorbed before the range even opens (§2, §3.1).</summary>
    Provincial,

    /// <summary>Genuinely outside Roman control for some or all of the range — Diplomacy with
    /// Non-Roman Peoples' real subject (§2). Category can shift mid-range at a real historical
    /// conquest date via <see cref="CultureDefinition.Category"/>'s <see cref="Regions.DatedRule{TValue}"/>.</summary>
    Frontier,

    /// <summary>Parthia (§2, §9).</summary>
    GreatPower,

    /// <summary>Armenia's own real, distinct position between Rome and Parthia (§2, §9).</summary>
    ContestedBuffer,

    /// <summary>Real commercial/cultural contact only, never political or military contact at
    /// meaningful scale (§2, §10) — the six rare-encounter cultures.</summary>
    TradeContactOnly,
}
