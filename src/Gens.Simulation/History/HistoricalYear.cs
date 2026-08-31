using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// Converts a real, display-facing BCE/CE year (e.g. "44 BC", "AD 79", as the content source docs
/// write them) into this codebase's one canonical <see cref="GameDate"/> — rather than hand-computing
/// <see cref="GameDate.TotalMonths"/> inline at each of the roughly ninety authored call sites in <see
/// cref="KnownWorldHistoricalTimeline"/>/<see cref="KnownWorldHistoricalFigures"/>, where an off-by-one
/// here would silently corrupt every one of them. Per <see cref="GameDate.ToDisplayYear"/>'s own
/// documented inverse: a BCE display year <c>Y</c> is astronomical year <c>1 - Y</c>; a CE display year
/// <c>Y</c> is astronomical year <c>Y</c> directly (there is no year 0 in BCE/CE display).
/// </summary>
public static class HistoricalYear
{
    /// <param name="displayYear">The BCE/CE magnitude as written (e.g. <c>44</c> for "44 BC" or "AD 44"
    /// — always positive; <paramref name="isBce"/> supplies the sign).</param>
    /// <param name="isBce">Whether <paramref name="displayYear"/> is a BCE (before Christ) or CE (Anno
    /// Domini) year.</param>
    /// <param name="monthOfYear">1-based month of that year (January = 1). Defaults to 1 — none of this
    /// item's own source content docs carry month-level granularity for a real historical date, so every
    /// authored entry defaults to January of its real year rather than fabricating a specific month.</param>
    public static GameDate ToGameDate(int displayYear, bool isBce, int monthOfYear = 1)
    {
        if (displayYear < 1)
            throw new ArgumentOutOfRangeException(nameof(displayYear), displayYear, "A display year must be at least 1 (there is no year 0).");
        if (monthOfYear is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(monthOfYear), monthOfYear, "Month of year must be between 1 and 12.");

        var astronomicalYear = isBce ? 1 - displayYear : displayYear;
        var totalMonths = checked((astronomicalYear - GameDate.EpochAstronomicalYear) * 12 + (monthOfYear - 1));
        return new GameDate(totalMonths);
    }
}
