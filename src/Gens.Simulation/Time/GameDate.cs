namespace Gens.Simulation.Time;

/// <summary>
/// The sole authoritative, persisted representation of simulation time (ADR 0003): a compact,
/// month-granular <see cref="int"/>. <c>TotalMonths = 0</c> is January of astronomical year
/// <see cref="EpochAstronomicalYear"/>. Calendar and BCE/CE display are pure, stateless conversions
/// derived from <see cref="TotalMonths"/>, never stored redundantly.
/// </summary>
public readonly record struct GameDate(int TotalMonths)
{
    /// <summary>Astronomical year of <c>TotalMonths = 0</c>: 754 BCE proleptic Julian, one year
    /// before Rome's traditional founding date.</summary>
    public const int EpochAstronomicalYear = -753;

    public GameDate NextMonth() => new(checked(TotalMonths + 1));

    /// <summary>Converts <see cref="TotalMonths"/> to an astronomical year and a 1-based month of year.</summary>
    public (int AstronomicalYear, int MonthOfYear) ToCalendar()
    {
        var monthIndex = checked(TotalMonths);
        var year = EpochAstronomicalYear + monthIndex / 12;
        var monthOfYear = monthIndex % 12;
        if (monthOfYear < 0)
        {
            monthOfYear += 12;
            year = checked(year - 1);
        }

        return (year, monthOfYear + 1);
    }

    /// <summary>Maps an astronomical year to its BCE/CE display magnitude. There is no year 0 in
    /// BCE/CE display, so the mapping subtracts one crossing that boundary: astronomical year 0 is
    /// 1 BCE, astronomical year -1 is 2 BCE, astronomical year 1 is 1 CE.</summary>
    public static int ToDisplayYear(int astronomicalYear) =>
        astronomicalYear <= 0 ? checked(-astronomicalYear + 1) : astronomicalYear;

    /// <summary>The full human-readable year label for this date, e.g. <c>"753 BCE"</c> or <c>"14 CE"</c>.</summary>
    public string ToDisplayYearLabel()
    {
        var (year, _) = ToCalendar();
        return year <= 0 ? $"{ToDisplayYear(year)} BCE" : $"{year} CE";
    }
}

