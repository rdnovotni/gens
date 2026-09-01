using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Hazards;
using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Campaign;

public sealed class MonthlyReportProjectorTests
{
    [Test]
    public void ASevereOrCatastrophicDisasterEventReportsAsHighButAMinorOneReadsAsOrdinary()
    {
        var date = new GameDate(3);
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var disasterEventIds = new RuntimeIdCounter<DisasterEvent>();
        var eventIds = new RuntimeIdCounter<DomainEventEntity>();

        var catastrophic = new DisasterEventOccurredEvent(
            eventIds.Issue(), date, settlementId, disasterEventIds.Issue(), HazardType.Fire, DisasterSeverity.Catastrophic, false);
        var minor = new DisasterEventOccurredEvent(
            eventIds.Issue(), date, settlementId, disasterEventIds.Issue(), HazardType.Fire, DisasterSeverity.Minor, false);

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { catastrophic, minor });

        Assert.That(report.Entries[0].Importance, Is.EqualTo(ReportImportance.High));
        Assert.That(report.Entries[1].Importance, Is.EqualTo(ReportImportance.Medium));
    }

    [Test]
    public void AnEpidemicOutbreakIgnitionReportsAsHigh()
    {
        var date = new GameDate(3);
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        var ignited = new EpidemicOutbreakIgnitedEvent(
            new RuntimeIdCounter<DomainEventEntity>().Issue(), date, settlementId,
            new RuntimeIdCounter<EpidemicOutbreak>().Issue(), DiseaseCatalog.Pestilence);

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { ignited });

        Assert.That(report.Entries.Single().Importance, Is.EqualTo(ReportImportance.High));
    }

    [Test]
    public void ProjectsEventEnvelopeFieldsIntoReportEntries()
    {
        var date = new GameDate(3);
        var bootstrapEvent = new CampaignBootstrappedEvent(
            new RuntimeIdCounter<DomainEventEntity>().Issue(),
            date,
            new RuntimeIdCounter<Region>().Issue(),
            new RuntimeIdCounter<Settlement>().Issue(),
            new RuntimeIdCounter<Household>().Issue());

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { bootstrapEvent });

        Assert.That(report.Date, Is.EqualTo(date));
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        var entry = report.Entries[0];
        Assert.That(entry.EventType, Is.EqualTo("campaign.bootstrapped"));
        Assert.That(entry.Group, Is.EqualTo("campaign"));
        Assert.That(entry.Importance, Is.EqualTo(ReportImportance.High));
        Assert.That(entry.Acknowledgement, Is.EqualTo(ReportAcknowledgementState.AutoResolved));
        Assert.That(entry.SubjectIds, Is.EqualTo(bootstrapEvent.SubjectIds));
    }

    [Test]
    public void PreservesEmissionOrderRatherThanResorting()
    {
        var date = new GameDate(1);
        var laterId = RuntimeId<DomainEventEntity>.Parse("event_0000005");
        var earlierId = RuntimeId<DomainEventEntity>.Parse("event_0000002");
        var eventA = new GenericCommandExecutedEvent(laterId, date, "system", "z.action", "{}", null);
        var eventB = new GenericCommandExecutedEvent(earlierId, date, "system", "a.action", "{}", null);

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { eventA, eventB });

        Assert.That(report.Entries.Select(e => e.EventId), Is.EqualTo(new[] { "event_0000005", "event_0000002" }));
    }

    [Test]
    public void EmptyEventListProducesAnEmptyReport()
    {
        var report = MonthlyReportProjector.Project(new GameDate(0), Array.Empty<IDomainEvent>());

        Assert.That(report.Entries, Is.Empty);
        Assert.That(report.AutomationSummaries, Is.Empty);
    }

    [Test]
    public void GroupsAScopedEventByTheRealTaxonomyRatherThanTheNamespacePrefix()
    {
        var date = new GameDate(4);
        var instanceId = new RuntimeIdCounter<EventInstance>().Issue();
        var fired = new EventFiredEvent(
            new RuntimeIdCounter<DomainEventEntity>().Issue(), date, instanceId,
            new DefinitionId<EventDefinition>("some-def"), EventScope.Household, new[] { "household_0000001" }, null);

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { fired });

        var entry = report.Entries.Single();
        Assert.That(entry.Group, Is.EqualTo("household"));
        Assert.That(entry.Importance, Is.EqualTo(ReportImportance.High));
        Assert.That(entry.Acknowledgement, Is.EqualTo(ReportAcknowledgementState.Flagged));
        Assert.That(entry.SourceInstanceId, Is.EqualTo(instanceId.ToTaggedString()));
    }

    [Test]
    public void APersonalFiredEventIsAutoResolvedAndMediumImportance()
    {
        var date = new GameDate(4);
        var fired = new EventFiredEvent(
            new RuntimeIdCounter<DomainEventEntity>().Issue(), date, new RuntimeIdCounter<EventInstance>().Issue(),
            new DefinitionId<EventDefinition>("some-def"), EventScope.Personal, new[] { "char_0000001" }, null);

        var report = MonthlyReportProjector.Project(date, new IDomainEvent[] { fired });

        var entry = report.Entries.Single();
        Assert.That(entry.Group, Is.EqualTo("personal"));
        Assert.That(entry.Importance, Is.EqualTo(ReportImportance.Medium));
        Assert.That(entry.Acknowledgement, Is.EqualTo(ReportAcknowledgementState.AutoResolved));
    }

    [Test]
    public void ManySimilarAutoResolvedEventsCollapseIntoOneAutomationSummary()
    {
        var date = new GameDate(4);
        var events = Enumerable.Range(0, MonthlyReportProjector.AutomationSummaryThreshold)
            .Select(i => (IDomainEvent)new EventOptionResolvedEvent(
                RuntimeId<DomainEventEntity>.Parse($"event_{i:D7}"), date, new RuntimeIdCounter<EventInstance>().Issue(),
                new DefinitionId<EventDefinition>("some-def"), EventScope.Personal,
                new DefinitionId<EventOptionDefinition>("some-option"), new[] { "char_0000001" }, null))
            .ToArray();

        var report = MonthlyReportProjector.Project(date, events);

        Assert.That(report.Entries, Is.Empty);
        var summary = report.AutomationSummaries.Single();
        Assert.That(summary.Count, Is.EqualTo(MonthlyReportProjector.AutomationSummaryThreshold));
        Assert.That(summary.Group, Is.EqualTo("personal"));
        Assert.That(summary.EventType, Is.EqualTo("events.optionResolved"));
    }

    [Test]
    public void FewerThanTheThresholdSimilarEventsStayItemized()
    {
        var date = new GameDate(4);
        var events = Enumerable.Range(0, MonthlyReportProjector.AutomationSummaryThreshold - 1)
            .Select(i => (IDomainEvent)new EventOptionResolvedEvent(
                RuntimeId<DomainEventEntity>.Parse($"event_{i:D7}"), date, new RuntimeIdCounter<EventInstance>().Issue(),
                new DefinitionId<EventDefinition>("some-def"), EventScope.Personal,
                new DefinitionId<EventOptionDefinition>("some-option"), new[] { "char_0000001" }, null))
            .ToArray();

        var report = MonthlyReportProjector.Project(date, events);

        Assert.That(report.Entries, Has.Count.EqualTo(MonthlyReportProjector.AutomationSummaryThreshold - 1));
        Assert.That(report.AutomationSummaries, Is.Empty);
    }
}
