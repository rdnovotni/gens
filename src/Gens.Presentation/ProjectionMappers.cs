#nullable enable

using System;
using System.Linq;
using Gens.Application.Campaign;
using Gens.Presentation.Models;
using Gens.Simulation.Actions;
using Gens.Simulation.Campaign;
using Gens.Simulation.Queries;

namespace Gens.Presentation;

/// <summary>Pure display mapping shared by native and transitional Unity clients.</summary>
public static class ProjectionMappers
{
    private static readonly char[] WordSeparators = { ' ' };
    public static InkBarModel InkBar(InkBarProjection p) => new(p.GensName, $"{p.MonthOfYear:D2}/{p.DisplayYear} {p.Era}", $"{p.Treasury.ToDisplayString()} denarii", $"{p.Dignitas} dignitas");
    public static HouseholdRosterModel HouseholdRoster(HouseholdRosterProjection p) => new(p.Members.Select(row => new RosterRowModel(row.CharacterId, row.FullName, row.DutySlot is { } duty ? $"{row.AgeInYears} · {row.LegalStatus} · {duty}" : $"{row.AgeInYears} · {row.LegalStatus}", Monogram(row.FullName))).ToArray());
    public static CharacterDetailModel CharacterDetail(CharacterDetailProjection p)
    {
        string status = p.IsAlive ? p.Stage.ToString() : "Deceased"; string subtitle = p.DutySlot is { } duty ? $"{p.AgeInYears} · {status} · {p.LegalStatus} · {duty}" : $"{p.AgeInYears} · {status} · {p.LegalStatus}";
        return new(p.CharacterId, p.FullName, subtitle, Monogram(p.FullName),
            new[] { Stat("Diplomacy", p.Attributes.Diplomacy), Stat("Martial", p.Attributes.Martial), Stat("Stewardship", p.Attributes.Stewardship), Stat("Intrigue", p.Attributes.Intrigue), Stat("Learning", p.Attributes.Learning) },
            new[] { Stat("Fieldwork", p.Skills.Fieldwork), Stat("Domestic", p.Skills.DomesticService), Stat("Craft", p.Skills.Craft), Stat("Culinary", p.Skills.Culinary), Stat("Medicine", p.Skills.Medicine) },
            new[] { Stat("Health", p.Condition.Health), Stat("Fatigue", p.Condition.Fatigue), Stat("Loyalty", p.Condition.Loyalty) });
    }
    public static EstateSettlementModel EstateSettlement(EstateSettlementProjection p) => new(p.SettlementStage.ToString(), p.Holdings.Select(h => new HoldingModel(h.HoldingId, h.VillaStage is { } stage ? $"Villa ({stage}) · {h.ResidentCapacity} residents" : $"Holding · {h.ResidentCapacity} residents", h.Plots.SelectMany(plot => plot.Buildings).Select(b => new BuildingModel(b.BuildingId, $"{b.DefinitionKey} ({b.Tier})", b.Condition.ToString())).ToArray())).ToArray());
    public static MonthlyReportModel MonthlyReport(HouseholdFinancialsProjection f, MonthlyReportProjection report)
    {
        var calendar = report.Date.ToCalendar(); string era = calendar.AstronomicalYear <= 0 ? "BCE" : "CE";
        return new($"{calendar.MonthOfYear:D2}/{Gens.Simulation.Time.GameDate.ToDisplayYear(calendar.AstronomicalYear)} {era}", $"{f.Income.ToDisplayString()} denarii", $"{f.Expenses.ToDisplayString()} denarii", $"{f.Net.ToDisplayString()} denarii", report.Entries.Select(e => new ReportHeadlineModel(e.EventId, $"[{e.Group}] {e.EventType} — {e.Importance}", e.SourceInstanceId)).ToArray(), report.AutomationSummaries.Select(s => $"[{s.Group}] {s.EventType} × {s.Count}").ToArray());
    }
    public static ConfirmationModel Confirmation(CampaignActionPreview p) => new(p.Action, Humanize(p.NameKey), p.Summary, p.RequiresWaxSeal, p.IsAvailable, p.ErrorCode);
    public static string Humanize(string key) { string[] segments = key.Split('.'); string slug = segments.Length >= 2 ? segments[segments.Length - 2] : segments[segments.Length - 1]; return string.Join(" ", slug.Split('-').Where(static x => x.Length > 0).Select(static x => char.ToUpperInvariant(x[0]) + x.Substring(1))); }
    private static StatModel Stat(string label, int value) => new(label, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    private static string Monogram(string name) => string.Concat(name.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries).Take(2).Select(static part => char.ToUpperInvariant(part[0])));
}
