using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>Emitted whenever <see cref="PriesthoodTrickleSystem"/> ends an active <see
/// cref="PriesthoodRecord"/> because its holder died — a vacancy, not a loss (no re-election or
/// disgrace mechanic exists in this domain), matching <see
/// cref="Magistracies.MagistracyTermSystem"/>'s own "a dead Character can't meaningfully continue
/// holding a seat" addition and its identical "no <c>LossReason</c> — a vacancy" framing.</summary>
public sealed record PriesthoodVacatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PriesthoodRecord> RecordId,
    RuntimeId<Character> HolderId,
    PriesthoodOffice Office,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.priesthoodVacated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly Priesthood tick (Phase 12 item 3): §6.2/§6.3's passive Favor/Dignitas trickle for every
/// active office, and vacating a seat on the holder's death — the same addition <see
/// cref="Magistracies.MagistracyTermSystem"/> makes for its own office ladder, applied here since a
/// Priesthood has no annual term to expire on its own (see <see cref="PriesthoodRecord"/>'s own doc
/// comment for why no term-renewal branch exists at all). Runs in <see
/// cref="TickPhase.RelationshipsActors"/>, alongside every other Politics &amp; Patronage/Religion
/// office-holding system.
/// </summary>
public sealed class PriesthoodTrickleSystem : IMonthlySystem<WorldState>
{
    public string Id => "religion.priesthoodTrickle";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "priesthoodRecords", "characters", "householdReligions" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "priesthoodRecords", "householdReputations", "householdReligions", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.PriesthoodRecords.InAscendingOrder().Where(e => PriesthoodResolver.IsActive(e.Value)).ToArray())
        {
            var record = entry.Value;
            state.Characters.TryGet(record.HolderId, out var holder);

            if (holder is null || !holder.IsAlive)
            {
                state.PriesthoodRecords.Remove(record.RecordId);
                state.PriesthoodRecords.Add(record.RecordId, record with { EndDate = context.Date });
                events.Add(new PriesthoodVacatedEvent(
                    state.EventIds.Issue(), context.Date, record.RecordId, record.HolderId, record.Office, CausationId: null));
                continue;
            }

            if (holder.Household is not { } householdId)
                continue;

            var (favorTrickle, dignitasTrickle) = record.Office switch
            {
                PriesthoodOffice.Augur => (ReligionCatalog.AugurMonthlyFavor, ReligionCatalog.AugurMonthlyDignitas),
                PriesthoodOffice.Flamen => (ReligionCatalog.FlamenMonthlyFavor, ReligionCatalog.FlamenMonthlyDignitas),
                PriesthoodOffice.Pontifex => (ReligionCatalog.PontifexMonthlyFavor, ReligionCatalog.PontifexMonthlyDignitas),
                _ => (0, 0),
            };

            var dignitasCommand = new AdjustDignitasCommand(
                state.CommandIds.Issue(), "system", context.Date, null, householdId, dignitasTrickle, $"held the {record.Office} priesthood");
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(state, dignitasCommand).Events);

            if (HouseholdReligionResolver.HasChosenPatron(state, householdId))
                HouseholdReligionResolver.ApplyFavorDelta(state, householdId, favorTrickle);
        }

        return events;
    }
}
