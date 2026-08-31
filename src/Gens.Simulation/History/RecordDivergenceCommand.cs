using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.History;

/// <summary>Records a Divergence (§6.7), mirroring <see
/// cref="Gens.Simulation.Correspondence.SendLetterCommand"/>/<see
/// cref="Gens.Simulation.Travel.BeginTravelCommand"/>'s <see cref="CommandPipeline{TState,TCommand}"/>
/// shape (ADR 0006).</summary>
public sealed record RecordDivergenceCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> TriggeringHouseholdId,
    string TriggeringAction,
    IReadOnlyList<DefinitionId<HistoricalTimelineEntryDefinition>> AffectedTimelineEntryIds) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordDivergenceCommand"/> is accepted — always <see
/// cref="Visibility.Public"/> and always maximum-tier Dynasty Chronicle material (§6.7); see <see
/// cref="Chronicle.ChronicleProjector"/>'s own handling of this event type.</summary>
public sealed record DivergenceRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<DivergenceRecord> DivergenceId,
    RuntimeId<Household> TriggeringHouseholdId,
    string TriggeringAction,
    IReadOnlyList<DefinitionId<HistoricalTimelineEntryDefinition>> AffectedTimelineEntryIds,
    string? CausationId) : IDomainEvent
{
    public string Type => "history.divergenceRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { TriggeringHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The validate/mutate pipeline for <see cref="RecordDivergenceCommand"/> (ADR 0006). Enforces
/// "immutable history" from the branching direction: an entry whose real date has already passed in
/// this campaign (<see cref="HistoricalTimelineEntryDefinition.Date"/> earlier than <see
/// cref="WorldState.Date"/>) can never be diverged, and an entry already covered by an earlier <see
/// cref="DivergenceRecord"/> can never be diverged a second time.
/// </summary>
public static class RecordDivergenceCommands
{
    public static readonly ValidationErrorCode NoAffectedEntries = new("history.recordDivergence.noAffectedEntries");
    public static readonly ValidationErrorCode UnknownTimelineEntry = new("history.recordDivergence.unknownTimelineEntry");
    public static readonly ValidationErrorCode NotDivergenceEligible = new("history.recordDivergence.notDivergenceEligible");
    public static readonly ValidationErrorCode EntryAlreadyPast = new("history.recordDivergence.entryAlreadyPast");
    public static readonly ValidationErrorCode EntryAlreadyDiverged = new("history.recordDivergence.entryAlreadyDiverged");

    public static CommandPipeline<WorldState, RecordDivergenceCommand> BuildPipeline(HistoricalTimelineCatalog catalog)
    {
        if (catalog is null)
            throw new ArgumentNullException(nameof(catalog));

        return new CommandPipeline<WorldState, RecordDivergenceCommand>(
            validate: (state, command) => Validate(state, command, catalog),
            mutate: (state, command) => Mutate(state, command),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, RecordDivergenceCommand command, HistoricalTimelineCatalog catalog)
    {
        if (command.AffectedTimelineEntryIds is null || command.AffectedTimelineEntryIds.Count == 0)
            return NoAffectedEntries;

        var alreadyDivergedIds = state.DivergenceRecords.InAscendingOrder()
            .SelectMany(entry => entry.Value.AffectedTimelineEntryIds)
            .ToHashSet();

        foreach (var entryId in command.AffectedTimelineEntryIds)
        {
            if (!catalog.TryGet(entryId, out var entry))
                return UnknownTimelineEntry;
            if (!entry.DivergenceEligible)
                return NotDivergenceEligible;
            // Strict "<" alone would still accept an entry dated exactly this month once it has
            // already fired (HistoricalTimelineScheduler fires same-month entries before this command
            // could reasonably be submitted against them) — the FiredHistoricalTimelineEntryIds check
            // below closes that gap so a real, already-emitted historical fact can never retroactively
            // become "diverged."
            if (entry.Date.TotalMonths < state.Date.TotalMonths)
                return EntryAlreadyPast;
            if (state.FiredHistoricalTimelineEntryIds.TryGet(entryId.Value, out _))
                return EntryAlreadyPast;
            if (alreadyDivergedIds.Contains(entryId))
                return EntryAlreadyDiverged;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordDivergenceCommand command)
    {
        var divergenceId = state.DivergenceRecordIds.Issue();
        var record = new DivergenceRecord(
            divergenceId, command.SubmittedDate, command.TriggeringHouseholdId, command.TriggeringAction,
            command.AffectedTimelineEntryIds, NewAlternateHistoryBranchActive: true);
        state.DivergenceRecords.Add(divergenceId, record);

        return new IDomainEvent[]
        {
            new DivergenceRecordedEvent(
                state.EventIds.Issue(), command.SubmittedDate, divergenceId, command.TriggeringHouseholdId,
                command.TriggeringAction, command.AffectedTimelineEntryIds, command.CommandId.ToTaggedString()),
        };
    }
}
