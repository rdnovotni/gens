using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Ledger;

namespace Gens.Simulation.Education;

/// <summary>Versioned constants for Phase 17 item 2's Cultural Patronage mechanics
/// (<c>gens-education-culture-design.md</c> §7), invented as this implementation's own unsized baseline
/// — §13's Open Questions leaves every numeric figure in this domain open, exactly as §11's Open
/// Questions does for Religion — matching <see cref="Religion.ReligionCatalog"/>'s and <see
/// cref="Travel.DistanceTierCatalog"/>'s identical "unsized against real playtest data, but named in one
/// place" disclaimer convention.</summary>
public static class CulturalPrestigeCatalog
{
    /// <summary>§7's Literary Patronage monthly commitment — a standing sponsorship of poets/scholars,
    /// costed lighter than a full Symposium circuit (see <see cref="RecurringSymposiumMonthlyCost"/>)
    /// since it is a quieter, less socially visible form of patronage.</summary>
    public static readonly Money LiteraryPatronMonthlyCost = Money.FromDenarii(15);

    /// <summary>§7's Symposium — a recurring hosted gathering, costed heavier than plain Literary
    /// Patronage since it is the more socially visible, guest-hosting commitment.</summary>
    public static readonly Money RecurringSymposiumMonthlyCost = Money.FromDenarii(25);

    /// <summary>Monthly Cultural Prestige accrual for an active Literary Patronage.</summary>
    public const int LiteraryPatronMonthlyPrestigeGain = 1;

    /// <summary>Monthly Cultural Prestige accrual for an active Symposium — higher than plain Literary
    /// Patronage, matching the higher cost above.</summary>
    public const int RecurringSymposiumMonthlyPrestigeGain = 2;

    /// <summary>§7's "reads as a more attractive marriage partner" gate threshold — see <see
    /// cref="CulturalPrestigeResolver.ClearsMarriageMarketThreshold"/>.</summary>
    public const int MarriageMarketPrestigeThreshold = 10;
}
