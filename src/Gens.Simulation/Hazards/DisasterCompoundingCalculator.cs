using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>Pure, RNG-free date/seasonal and compounding math for §3.1 — the part of Exposure that
/// isn't a settlement's own standing terrain/building composition but this same month's calendar
/// position or another hazard's own roll. Every figure is this implementation's own invented number
/// (§9's "All numeric sizing" open question), chosen only so a dry season measurably — not
/// marginally — raises Fire risk alongside Drought, and a severe Storm has a real, felt chance of
/// dragging a Flood in behind it, without turning either into a guaranteed chain (§3.1's own "not an
/// inevitability" framing).</summary>
public static class DisasterCompoundingCalculator
{
    /// <summary>§1/§3.1's Mediterranean dry season: June-August (months 6-8 of the calendar year),
    /// deliberately literal rather than region-varied — no Region carries real seasonal-offset data
    /// (<see cref="HazardExposureCalculator.EarthquakeExposure"/>'s identical regional-data
    /// disclosure).</summary>
    public static bool IsDrySeasonMonth(GameDate date)
    {
        var (_, monthOfYear) = date.ToCalendar();
        return monthOfYear is >= 6 and <= 8;
    }

    /// <summary>§1/§3.1's Mediterranean storm season: October-December — the "closed sea" months.</summary>
    public static bool IsStormSeasonMonth(GameDate date)
    {
        var (_, monthOfYear) = date.ToCalendar();
        return monthOfYear is >= 10 and <= 12;
    }

    /// <summary>§3.1's "Fire Exposure temporarily rises alongside" an active Drought — added directly to
    /// <see cref="HazardExposureCalculator.FireExposure"/>'s own density-driven score by the caller.
    /// Only ever non-zero during the dry season itself (<see cref="IsDrySeasonMonth"/>): this codebase
    /// has no separate "a drought is already in progress" standing flag, so the dry season's own
    /// presence is taken as the real trigger, per §2's own "Drought/Famine... region... worsened by low
    /// Soil Fertility" framing treating the season itself as the proximate cause.</summary>
    public static int DrySeasonFireExposureBonus(bool drySeasonMonth) => drySeasonMonth ? 15 : 0;

    /// <summary>Also raises Storm Exposure during storm season — §1's "the ancient 'closed sea' season
    /// existed for exactly this reason," added directly to <see cref="HazardExposureCalculator.StormExposure"/>'s
    /// own coastal-driven score by the caller.</summary>
    public static int StormSeasonExposureBonus(bool stormSeasonMonth) => stormSeasonMonth ? 20 : 0;

    /// <summary>§3.1's Storm-into-Flood chaining: the probability a Storm Event that resolved at
    /// <paramref name="stormSeverity"/> directly triggers a companion Flood Event this same month on any
    /// River-adjacent plot at the same settlement. Zero below <see cref="DisasterSeverity.Severe"/>, per
    /// §3.1's own "a Storm Event resolving at Severe or Catastrophic severity."</summary>
    public static double StormToFloodChainProbability(DisasterSeverity stormSeverity) => stormSeverity switch
    {
        DisasterSeverity.Severe => 0.35,
        DisasterSeverity.Catastrophic => 0.6,
        _ => 0.0,
    };

    /// <summary>The chained Flood's own severity — one tier below the triggering Storm's, floored at
    /// <see cref="DisasterSeverity.Moderate"/> (never a mere Minor chained flood — §3.1's "a storm surge
    /// or sustained heavy rain overwhelming a river" is never a trivial event once it has actually
    /// chained).</summary>
    public static DisasterSeverity ChainedFloodSeverity(DisasterSeverity stormSeverity) =>
        stormSeverity == DisasterSeverity.Catastrophic ? DisasterSeverity.Severe : DisasterSeverity.Moderate;
}
