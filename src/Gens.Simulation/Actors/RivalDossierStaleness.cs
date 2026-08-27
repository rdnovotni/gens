using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>A read-only "as of" view of a <see cref="RivalDossier"/>'s own age (Phase 10 item 5/package
/// 14; <c>gens-rival-houses-design.md</c> §7: "Dossier isn't omnisciently live"). A UI/report layer
/// reads this instead of treating <see cref="RivalDossier.Summary"/> as a live figure — the concrete
/// mechanism behind "information staleness," without inventing a numeric decay mechanic the design doc
/// never sizes.</summary>
public static class RivalDossierStaleness
{
    /// <summary>Whole months elapsed between <paramref name="dossier"/>'s own <see
    /// cref="RivalDossier.LastUpdatedDate"/> and <paramref name="currentDate"/> — never negative, since
    /// <see cref="RivalDossierRefresh"/> guarantees <see cref="RivalDossier.LastUpdatedDate"/> never
    /// exceeds the date of whatever event refreshed it.</summary>
    public static int MonthsSinceUpdate(RivalDossier dossier, GameDate currentDate) =>
        Math.Max(0, currentDate.TotalMonths - dossier.LastUpdatedDate.TotalMonths);

    /// <summary>A short, human-readable "as of" label (e.g. <c>"as of this month"</c>, <c>"as of 7
    /// months ago"</c>) for direct display — the sole sanctioned path to dossier-age text, matching
    /// <see cref="Ledger.Money.ToDisplayString"/>'s identical "one sanctioned presentation path"
    /// convention.</summary>
    public static string Describe(RivalDossier dossier, GameDate currentDate)
    {
        if (dossier is null)
            throw new ArgumentNullException(nameof(dossier));

        var months = MonthsSinceUpdate(dossier, currentDate);
        return months switch
        {
            0 => "as of this month",
            1 => "as of 1 month ago",
            _ => $"as of {months} months ago",
        };
    }
}
