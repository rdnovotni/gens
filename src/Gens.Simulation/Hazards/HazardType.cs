namespace Gens.Simulation.Hazards;

/// <summary>The nine named hazards <c>gens-natural-disasters-design.md</c> §2 catalogs — the eight
/// standing hazards this item gives a real, monthly-rolled <see cref="HazardExposureCalculator"/>
/// score and <see cref="NaturalDisasterSystem"/> Event roll, plus <see cref="VolcanicEruption"/>, which
/// §2.2 deliberately treats outside that ordinary Exposure system entirely (no standing Exposure score
/// exists for it — see <see cref="DormantVolcano"/>'s own doc comment for how this item scopes it
/// instead). Ordering matches §2's own table top-to-bottom.</summary>
public enum HazardType
{
    /// <summary>§2: insulae/urban building density; rises during an active Drought (§3.1's dry-season
    /// overlap).</summary>
    Fire,

    /// <summary>§2: River-adjacent plots, worsened by low regional Forest Cover (§4.2/§3.1); can be
    /// chained into directly by a Severe/Catastrophic Storm (§3.1).</summary>
    Flood,

    /// <summary>§2/§2.1: region-weighted, genuinely unpreventable — no countermeasure exists.</summary>
    Earthquake,

    /// <summary>§2: region-weighted, worsened by low Soil Fertility (§4.1).</summary>
    DroughtFamine,

    /// <summary>§2: coastal/port reliance and sea-trade volume; a Severe/Catastrophic result can chain
    /// into <see cref="Flood"/> on River-adjacent plots (§3.1).</summary>
    Storm,

    /// <summary>§2 (new): Hills/Mountain terrain, driven jointly by low regional Forest Cover and low
    /// Slope Stability (§4.3/§3.1).</summary>
    Landslide,

    /// <summary>§2 (new): low crop diversity — an Intensive-Monoculture estate runs measurably higher.</summary>
    BlightInfestation,

    /// <summary>§2 (new): region-weighted (the Gallic frontier runs cold), driven by how concentrated an
    /// estate's own output is in Olive/Vineyard chains specifically; §5.4's own distinct multi-year
    /// perennial-crop recovery tail.</summary>
    Frost,

    /// <summary>§2.2 (new): rare and specially-cased rather than Exposure-driven — see <see
    /// cref="DormantVolcano"/>.</summary>
    VolcanicEruption,
}
