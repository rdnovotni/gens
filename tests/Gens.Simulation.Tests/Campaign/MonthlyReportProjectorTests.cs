using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Campaign;

public sealed class MonthlyReportProjectorTests
{
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
    }
}
