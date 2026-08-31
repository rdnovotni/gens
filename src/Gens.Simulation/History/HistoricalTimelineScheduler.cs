using Gens.Simulation.Commands;
using Gens.Simulation.Events;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>Emitted for an unlinked (<see
/// cref="HistoricalTimelineEntryDefinition.LinkedEventDefinitionRef"/> null) entry once it fires — the
/// lightweight digest §6.4/§7 call for: "a dated Historical Timeline entry is always at minimum an
/// Auto-Resolved digest line." Mirrors <see cref="Gens.Simulation.Correspondence.LetterDeliveredEvent"/>'s
/// shape.</summary>
public sealed record HistoricalTimelineEntryOccurredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    DefinitionId<HistoricalTimelineEntryDefinition> EntryId,
    string RealWorldName,
    string? CausationId) : IDomainEvent
{
    public string Type => "history.timelineEntryOccurred";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => Array.Empty<string>();
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The historical timeline scheduler itself (Phase 13 item 5's own name): each tick, fires every
/// catalog entry whose real <see cref="HistoricalTimelineEntryDefinition.Date"/> matches <see
/// cref="WorldState.Date"/> exactly (GameDate is month-granular) and whose derived <see
/// cref="HistoricalDivergenceState"/> is <see cref="HistoricalDivergenceState.OnTrack"/> — an already-
/// <see cref="HistoricalDivergenceState.Diverged"/> entry never fires, "immutable history" from the
/// other direction (once Diverged, the real event genuinely doesn't happen for that campaign). Mirrors
/// <see cref="Correspondence.CorrespondenceTransitSystem"/>/<see
/// cref="Travel.TravelProgressSystem"/>'s <see cref="IMonthlySystem{TState}"/> shape.
///
/// Uses <see cref="WorldState.Date"/> directly as the campaign's own single clock for both the "has this
/// entry's real date arrived" check and (via <paramref name="campaignStartingDate"/>) the <see
/// cref="HistoricalDivergenceState.PredatesStart"/> check — this codebase's <c>GameCalendar</c> (§10's
/// own per-household starting-year/current-year/era sketch) is explicitly out of this item's scope
/// (that's Start Mode/Core's own job); this single-household simulation already treats <see
/// cref="WorldState.Date"/> as *the* campaign clock everywhere else, so this scheduler does too rather
/// than inventing a parallel, unused multi-household calendar concept.
/// </summary>
public sealed class HistoricalTimelineScheduler : IMonthlySystem<WorldState>
{
    private readonly HistoricalTimelineCatalog _catalog;
    private readonly GameDate _campaignStartingDate;
    private readonly EventCatalog? _eventCatalog;
    private readonly CommandPipeline<WorldState, FireEventCommand>? _firePipeline;

    /// <param name="catalog">Every registered <see cref="HistoricalTimelineEntryDefinition"/>.</param>
    /// <param name="campaignStartingDate">This campaign's own Starting Year (§6.2).</param>
    /// <param name="eventCatalog">When supplied, an entry with a non-null <see
    /// cref="HistoricalTimelineEntryDefinition.LinkedEventDefinitionRef"/> fires through the existing
    /// Events pipeline (<see cref="FireEventCommands"/>) instead of emitting its own digest event — the
    /// same reuse <see cref="EventPoolSystem"/> itself makes, resolving subjects off the linked
    /// definition's own declared <see cref="EventDefinition.Scope"/> (<see cref="ResolveSubjects"/>)
    /// exactly like <see cref="EventPoolSystem"/>'s own per-scope candidate resolution — never the fixed
    /// Imperial sentinel regardless of scope, which would fire e.g. a Personal-scope linked definition as
    /// a private instance against a subject that doesn't exist. <c>null</c> when no caller has one to
    /// supply yet (every authored real entry currently leaves this link null anyway; see <see
    /// cref="KnownWorldHistoricalTimeline"/>'s own doc comment).</param>
    public HistoricalTimelineScheduler(HistoricalTimelineCatalog catalog, GameDate campaignStartingDate, EventCatalog? eventCatalog = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _campaignStartingDate = campaignStartingDate;
        _eventCatalog = eventCatalog;
        _firePipeline = eventCatalog is null ? null : FireEventCommands.BuildPipeline(eventCatalog);
    }

    /// <summary>Every real subject ID a linked <see cref="EventDefinition"/> of <paramref name="scope"/>
    /// should fire against this tick — the same per-scope candidate sets <see
    /// cref="EventSubjects"/>/<see cref="EventPoolSystem"/> already use for the ordinary weighted-pool
    /// and Scripted paths, so a Historical Timeline entry linking a non-Imperial definition fires exactly
    /// as that definition's own scope intends rather than always against the Imperial sentinel.</summary>
    private static IEnumerable<string> ResolveSubjects(WorldState state, EventScope scope) => scope switch
    {
        EventScope.Personal => EventSubjects.Characters(state).Select(id => id.ToTaggedString()),
        EventScope.Household => EventSubjects.Households(state).Select(id => id.ToTaggedString()),
        EventScope.Settlement => EventSubjects.Settlements(state).Select(id => id.ToTaggedString()),
        EventScope.Imperial => new[] { EventSubjects.ImperialSubjectId },
        _ => Array.Empty<string>(),
    };

    public string Id => "history.timelineScheduler";
    public TickPhase Phase => TickPhase.Events;

    public IReadOnlyCollection<string> Reads { get; } = new[] { "divergenceRecords" };

    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "firedHistoricalTimelineEntryIds", "commandIds", "eventIds", "commandSequence", "eventInstances", "eventInstanceIds" };

    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        var due = _catalog.Chronological().Where(entry => entry.Date.TotalMonths == context.Date.TotalMonths);
        foreach (var entry in due)
        {
            if (state.FiredHistoricalTimelineEntryIds.TryGet(entry.Id.Value, out _))
                continue;

            var divergenceState = HistoricalTimelineQueries.DivergenceStateOf(state, _catalog, entry, _campaignStartingDate);
            if (divergenceState != HistoricalDivergenceState.OnTrack)
                continue;

            if (entry.LinkedEventDefinitionRef is { } linkedId && _firePipeline is not null && _eventCatalog is not null &&
                _eventCatalog.TryGet(linkedId, out var linkedDefinition))
            {
                var anyAccepted = false;
                foreach (var subjectId in ResolveSubjects(state, linkedDefinition.Scope))
                {
                    var command = new FireEventCommand(
                        state.CommandIds.Issue(), "system", context.Date, CausationId: null,
                        linkedId, new[] { subjectId }, subjectId);
                    var result = _firePipeline.Execute(state, command);
                    // A rejected fire (e.g. an already-active instance of the same definition/subject)
                    // must not permanently suppress this entry: only mark it fired once at least one
                    // subject's firing actually succeeds, so a later tick can retry rather than silently
                    // losing it.
                    if (result.Accepted)
                    {
                        events.AddRange(result.Events);
                        anyAccepted = true;
                    }
                }

                if (anyAccepted)
                    state.FiredHistoricalTimelineEntryIds.Add(entry.Id.Value, context.Date);
                continue;
            }

            state.FiredHistoricalTimelineEntryIds.Add(entry.Id.Value, context.Date);
            events.Add(new HistoricalTimelineEntryOccurredEvent(state.EventIds.Issue(), context.Date, entry.Id, entry.RealWorldName, CausationId: null));
        }

        return events;
    }
}
