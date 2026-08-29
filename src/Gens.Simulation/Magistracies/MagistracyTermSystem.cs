using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>Emitted whenever <see cref="MagistracyTermSystem"/> ends an active <see
/// cref="MagistracyRecord"/> early — an Insolvency strip (§5.7) or a holder's death mid-term (this
/// system's own addition; see its doc comment). A losing re-election is <see
/// cref="MagistracyAssumedEvent"/>/<see cref="ElectionResolvedEvent"/>'s own territory instead, since
/// that ending only ever happens through <see cref="HoldContestedElectionCommand"/>, not this system.</summary>
public sealed record MagistracyLostEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MagistracyRecord> RecordId,
    RuntimeId<Character> HolderId,
    MagistracyOffice Office,
    MagistracyLossReason? LossReason,
    string? CausationId) : IDomainEvent
{
    public string Type => "magistracies.magistracyLost";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly office-holding tick (Phase 12 item 2): §5.1/§5.4's passive Dignitas trickle for every
/// active office, and §5.7's term limits and mid-term loss of office. Runs in <see
/// cref="TickPhase.RelationshipsActors"/>, the same phase every other Politics &amp; Patronage system
/// in this item runs in.
///
/// <b>Term renewal (§5.7):</b> "re-election at each term's end is its own contested election (§5.5) if
/// challenged, or a simple renewal if not." This system only ever performs the "simple renewal" half —
/// on the exact month a term reaches <see cref="MagistracyCatalog.TermLengthMonths"/>, an active record
/// still standing (nobody submitted <see cref="HoldContestedElectionCommand"/> against it) has its term
/// start date advanced in place rather than being ended and re-created, since nothing about who holds
/// the seat actually changed. A challenged renewal is <see cref="HoldContestedElectionCommand"/>'s own
/// job and never reaches this branch at all, because by the time this system's monthly scan runs, that
/// command has already replaced the record.
///
/// <b>Loss of office (§5.7):</b> Insolvency is checked every month, not just at the term boundary — "an
/// office can also be lost mid-term, not just fail to renew." This reads <see
/// cref="InsolvencyState.Stage"/> directly, since <see cref="InsolvencySystem"/> itself doesn't yet
/// apply its own flagged <c>officeOrCensusLoss</c> consequence (see that system's doc comment) — see
/// <see cref="MagistracyLossReason.Insolvency"/>'s own doc comment for why this item reads the ladder
/// directly instead of waiting on that gap to close. A Legal &amp; Court conviction (§5.7's other
/// route) is not checked — see <see cref="MagistracyLossReason.LegalConviction"/>.
///
/// <b>Holder death (this system's own addition, not named by §5.7):</b> a dead Character can't
/// meaningfully continue holding a seat, so this system ends their record on death too (no <see
/// cref="MagistracyLossReason"/> — a vacancy, not a loss) rather than leaving a corpse listed as
/// Decurion forever. Re-seating the vacancy is left to <see cref="AppointDecurionCommand"/>/<see
/// cref="HoldContestedElectionCommand"/>, not automated here.
/// </summary>
public sealed class MagistracyTermSystem : IMonthlySystem<WorldState>
{
    public string Id => "magistracies.term";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "magistracyRecords", "characters", "insolvencyStates" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "magistracyRecords", "householdReputations", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.MagistracyRecords.InAscendingOrder().Where(e => MagistracyResolver.IsActive(e.Value)).ToArray())
        {
            var record = entry.Value;
            state.Characters.TryGet(record.HolderId, out var holder);

            if (holder is null || !holder.IsAlive)
            {
                EndRecord(state, record, context.Date, lossReason: null);
                events.Add(new MagistracyLostEvent(
                    state.EventIds.Issue(), context.Date, record.RecordId, record.HolderId, record.Office, null, CausationId: null));
                continue;
            }

            if (holder.Household is { } householdId &&
                state.InsolvencyStates.TryGet(householdId, out var insolvency) &&
                insolvency!.Stage is InsolvencyStage.Insolvent or InsolvencyStage.Ruined)
            {
                EndRecord(state, record, context.Date, MagistracyLossReason.Insolvency);
                var penalty = new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", context.Date, null, householdId,
                    -MagistracyCatalog.EarlyLossDignitasPenalty, $"lost the {record.Office} seat to Insolvency");
                var penaltyResult = AdjustDignitasCommands.Pipeline.Execute(state, penalty);
                events.AddRange(penaltyResult.Events);
                events.Add(new MagistracyLostEvent(
                    state.EventIds.Issue(), context.Date, record.RecordId, record.HolderId, record.Office,
                    MagistracyLossReason.Insolvency, CausationId: null));
                continue;
            }

            if (holder.Household is { } activeHouseholdId)
            {
                var trickle = record.Office switch
                {
                    MagistracyOffice.Decurion => MagistracyCatalog.DecurionMonthlyDignitas,
                    MagistracyOffice.Aedile => MagistracyCatalog.AedileMonthlyDignitas,
                    MagistracyOffice.QuaestorLocal => MagistracyCatalog.QuaestorLocalMonthlyDignitas,
                    MagistracyOffice.Duumvir => MagistracyCatalog.DuumvirMonthlyDignitas,
                    _ => 0,
                };
                var trickleCommand = new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", context.Date, null, activeHouseholdId, trickle,
                    $"held the {record.Office} seat");
                var trickleResult = AdjustDignitasCommands.Pipeline.Execute(state, trickleCommand);
                events.AddRange(trickleResult.Events);
            }

            var monthsHeld = context.Date.TotalMonths - record.TermStartDate.TotalMonths;
            if (monthsHeld > 0 && monthsHeld % MagistracyCatalog.TermLengthMonths == 0)
            {
                state.MagistracyRecords.Remove(record.RecordId);
                state.MagistracyRecords.Add(record.RecordId, record with { TermStartDate = context.Date });
            }
        }

        return events;
    }

    private static void EndRecord(WorldState state, MagistracyRecord record, GameDate date, MagistracyLossReason? lossReason)
    {
        state.MagistracyRecords.Remove(record.RecordId);
        state.MagistracyRecords.Add(record.RecordId, record with { TermEndDate = date, LossReason = lossReason });
    }
}
