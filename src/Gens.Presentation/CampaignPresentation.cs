#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Gens.Application.Campaign;
using Gens.Presentation.Models;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;

namespace Gens.Presentation;

/// <summary>Engine-neutral, snapshot-only presentation gateway for one active campaign.</summary>
public sealed class CampaignPresentation
{
    private const string Observer = "player";
    private readonly CampaignSession session;

    public CampaignPresentation(CampaignSession session) =>
        this.session = session ?? throw new ArgumentNullException(nameof(session));

    public InkBarModel InkBar()
    {
        InkBarProjection p = session.Query(new InkBarQuery(session.HouseholdId), Observer);
        return ProjectionMappers.InkBar(p);
    }

    public HouseholdRosterModel HouseholdRoster()
    {
        HouseholdRosterProjection p = session.Query(new HouseholdRosterQuery(session.HouseholdId), Observer);
        return ProjectionMappers.HouseholdRoster(p);
    }

    public CharacterDetailModel CharacterDetail(string characterId)
    {
        CharacterDetailProjection p = session.Query(new CharacterDetailQuery(RuntimeId<Character>.Parse(characterId)), Observer);
        return ProjectionMappers.CharacterDetail(p);
    }

    public EstateSettlementModel EstateSettlement()
    {
        EstateSettlementProjection p = session.Query(new EstateSettlementQuery(session.SettlementId, session.HouseholdId), Observer);
        return ProjectionMappers.EstateSettlement(p);
    }

    public MonthlyReportModel MonthlyReport(IReadOnlyList<IDomainEvent> events)
    {
        HouseholdFinancialsProjection f = session.Query(new HouseholdFinancialsQuery(session.HouseholdId), Observer);
        MonthlyReportProjection report = session.ProjectMonthlyReport(events);
        return ProjectionMappers.MonthlyReport(f, report);
    }

    public ConfirmationModel Preview(CampaignHouseholdAction action)
    {
        CampaignActionPreview p = session.PreviewAction(action);
        return ProjectionMappers.Confirmation(p);
    }

}
