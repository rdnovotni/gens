#nullable enable

using System.Collections.Generic;
using Gens.Application.Campaign;
using Gens.Presentation.Visuals;

namespace Gens.Presentation.Models;

public sealed record InkBarModel(string GensName, string Date, string Treasury, string Dignitas);
public sealed record RosterRowModel(string CharacterId, string Name, string Subtitle, string Monogram, CharacterVisualState Visual, AppearanceDescription Appearance);
public sealed record HouseholdRosterModel(IReadOnlyList<RosterRowModel> Rows);
public sealed record StatModel(string Label, string Value);
public sealed record CharacterDetailModel(
    string CharacterId, string Name, string Subtitle, string Monogram, CharacterVisualState Visual, AppearanceDescription Appearance,
    IReadOnlyList<StatModel> Attributes, IReadOnlyList<StatModel> Skills, IReadOnlyList<StatModel> Condition);
public sealed record BuildingModel(string BuildingId, string Label, string Condition);
public sealed record HoldingModel(string HoldingId, string Label, IReadOnlyList<BuildingModel> Buildings);
public sealed record EstateSettlementModel(string SettlementStage, IReadOnlyList<HoldingModel> Holdings);
public sealed record ReportHeadlineModel(string EventId, string Label, string? SourceInstanceId);
public sealed record MonthlyReportModel(
    string Date, string Income, string Expenses, string Net,
    IReadOnlyList<ReportHeadlineModel> Headlines, IReadOnlyList<string> AutomationSummaries);
public sealed record ConfirmationModel(CampaignHouseholdAction Action, string Title, string Body, bool IsWaxSeal, bool IsAvailable, string? ErrorCode);
