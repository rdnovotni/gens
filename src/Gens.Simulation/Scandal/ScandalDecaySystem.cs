using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Scandal;

/// <summary>Emitted whenever <see cref="ScandalDecaySystem"/> steps a still-active <see
/// cref="ScandalRecord"/>'s severity down, or finally deactivates it. Private within the scandalized
/// household's own knowledge — unlike <see cref="ScandalRecordedEvent"/>'s own public first airing, a
/// Scandal quietly fading from memory is not itself a new fact the wider settlement needs told to it a
/// second time, matching <see cref="Reputation.FavorExpiredEvent"/>'s own "time itself resolving a
/// record" precedent, though scoped to the one household this record actually names since (unlike a
/// favor) a Scandal has no second party to share visibility with.</summary>
public sealed record ScandalFadedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ScandalRecord> ScandalId,
    RuntimeId<Household> PrimaryHouseholdId,
    ScandalSeverity NewSeverity,
    bool Deactivated,
    string? CausationId) : IDomainEvent
{
    public string Type => "scandal.faded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PrimaryHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(PrimaryHouseholdId.ToTaggedString());
}

/// <summary>
/// §9's Scandal Lifecycle: "an ordinary Scandal's own felt severity fades over time if not actively
/// refreshed by a further incident, eventually settling into background Dynasty Chronicle memory rather
/// than an active, ongoing penalty." Matches <see cref="Reputation.FavorExpirationSystem"/>'s identical
/// age-gated-lapse shape directly: at <see cref="ScandalCatalog.SeverityFadeAfterMonths"/>, a still-active
/// record's <see cref="ScandalRecord.Severity"/> steps down one rung (<see
/// cref="ScandalSeverity.NotaCensoriaEligible"/> → <see cref="ScandalSeverity.PublicDisgrace"/> → <see
/// cref="ScandalSeverity.MinorEmbarrassment"/>); at the further <see
/// cref="ScandalCatalog.DeactivateAfterMonths"/> gate, it is set <see cref="ScandalRecord.IsActive"/>
/// false outright. Runs in <see cref="TickPhase.RelationshipsActors"/>, the same phase <see
/// cref="Reputation.FavorExpirationSystem"/> and every other actor-standing/relationship system in this
/// codebase runs in.
/// </summary>
public sealed class ScandalDecaySystem : IMonthlySystem<WorldState>
{
    public string Id => "scandal.decay";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "scandalRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "scandalRecords", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var updates = new List<(RuntimeId<ScandalRecord> Id, ScandalRecord Record)>();

        foreach (var entry in state.ScandalRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (!record.IsActive)
                continue;

            var ageInMonths = context.Date.TotalMonths - record.RecordedDate.TotalMonths;

            if (ageInMonths >= ScandalCatalog.DeactivateAfterMonths)
            {
                updates.Add((entry.Key, record with { IsActive = false }));
                events.Add(new ScandalFadedEvent(
                    state.EventIds.Issue(), context.Date, entry.Key, record.PrimaryHouseholdId,
                    record.Severity, Deactivated: true, CausationId: null));
            }
            else if (ageInMonths >= ScandalCatalog.SeverityFadeAfterMonths && record.Severity != ScandalSeverity.MinorEmbarrassment)
            {
                var fadedSeverity = record.Severity == ScandalSeverity.NotaCensoriaEligible
                    ? ScandalSeverity.PublicDisgrace
                    : ScandalSeverity.MinorEmbarrassment;
                updates.Add((entry.Key, record with { Severity = fadedSeverity }));
                events.Add(new ScandalFadedEvent(
                    state.EventIds.Issue(), context.Date, entry.Key, record.PrimaryHouseholdId,
                    fadedSeverity, Deactivated: false, CausationId: null));
            }
        }

        foreach (var (id, record) in updates)
        {
            state.ScandalRecords.Remove(id);
            state.ScandalRecords.Add(id, record);
        }

        return events;
    }
}
