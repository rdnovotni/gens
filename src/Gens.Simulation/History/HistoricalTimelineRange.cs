using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>
/// The supported campaign range for the Historical Timeline (§6.2/§6.4): 133 BC – AD 235. <see
/// cref="Start"/> is inclusive (133 BC's Tiberius Gracchus opening is a real, dated entry a campaign
/// starting exactly then can watch fire); <see cref="End"/> is exclusive, set to January AD 236 so
/// every real month of AD 235 (the Severan dynasty's own closing year) remains in range — a
/// entry-date validator or a Divergence check against this range should compare with <c>&gt;= Start</c>
/// and <c>&lt; End</c>.
/// </summary>
public static class HistoricalTimelineRange
{
    public static readonly GameDate Start = HistoricalYear.ToGameDate(133, isBce: true);
    public static readonly GameDate End = HistoricalYear.ToGameDate(236, isBce: false);

    public static bool Contains(GameDate date) => date.TotalMonths >= Start.TotalMonths && date.TotalMonths < End.TotalMonths;
}
