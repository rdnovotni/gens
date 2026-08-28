using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>Emitted whenever a household's annual <c>Parentalia</c> offering is credited (§5.1).</summary>
public sealed record ParentaliaObservedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int MemoriaGained) : IDomainEvent
{
    public string Type => "funerary.parentaliaObserved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a household's <c>Parentalia</c> offering goes unobserved for want of
/// funds (§5.1, §6.3).</summary>
public sealed record ParentaliaSkippedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int MemoriaLost) : IDomainEvent
{
    public string Type => "funerary.parentaliaSkipped";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// The ongoing Manes cult / <c>Parentalia</c> observance (Phase 11 item 4; §5.1) — the steady Memoria
/// trickle source the roadmap task calls out directly, "structurally parallel to Religion's own
/// household-worship Favor mechanic" per §5.1's own text and mirroring <see
/// cref="Policies.FundFestivalCommand"/>'s Rites-Budget-adjacent shape the same task names as this
/// system's own precedent. Runs every month but only acts in <see
/// cref="FuneraryCatalog.ParentaliaMonthOfYear"/> (February) — the real nine-day festival window (§5.1)
/// collapsed to a single once-a-year credit, per <see cref="FuneraryCatalog"/>'s own doc comment.
///
/// Fully automatic, deliberately: the roadmap task allows a manual "record Parentalia observance"
/// command only "if [observance] isn't fully automatic". §5.1's own named reasons for a skip — neglect,
/// Travel absence, genuine financial crisis — reduce in this codebase's current state to exactly one
/// mechanically real gate (Travel does not exist yet to model an absence), so this system checks only
/// affordability: for every tracked <see cref="Succession.HouseholdHeadship"/>, it tries to post <see
/// cref="FuneraryCatalog.ParentaliaOfferingCost"/> from that household's Treasury; success credits <see
/// cref="FuneraryCatalog.ParentaliaBaseMemoriaGain"/> plus the household's own Chronicle-entry trickle
/// (§6.1, <see cref="FuneraryCatalog.ChronicleTrickle"/>) and records <see
/// cref="MemoriaState.LastParentaliaObservedDate"/>; insufficient funds skips the draw entirely and
/// applies <see cref="FuneraryCatalog.ParentaliaSkippedMemoriaLoss"/> instead — no explicit command
/// needed for either outcome. <c>Lemuria</c> (§5.3) is deliberately not implemented: it wants an
/// Omens &amp; Auspices-style Event instance (Religion §4.1, not yet built), and this pass does not
/// attempt to stand in for that missing Event-content machinery.
/// </summary>
public sealed class ManesObservanceSystem : IMonthlySystem<WorldState>
{
    public string Id => "funerary.manesObservance";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "householdHeadships", "memoriaStates", "chronicleEntries", "ledgerAccounts" };

    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "memoriaStates", "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };

    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    /// <summary>The named system sink a Parentalia offering's cost drains into, matching <see
    /// cref="Policies.FundFestivalCommands"/>'s identical "one named account, not an untracked leak"
    /// discipline.</summary>
    private static readonly LedgerAccountKey ParentaliaSink = new(LedgerAccountKind.System, "funerary:parentalia");

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var (_, monthOfYear) = context.Date.ToCalendar();
        if (monthOfYear != FuneraryCatalog.ParentaliaMonthOfYear)
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();

        foreach (var entry in state.HouseholdHeadships.InAscendingOrder())
        {
            var householdId = entry.Key;
            var cost = FuneraryCatalog.ParentaliaOfferingCost;
            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
                ? account!.Balance
                : Money.Zero;

            if (balance < cost)
            {
                MemoriaResolver.Apply(state, householdId, -FuneraryCatalog.ParentaliaSkippedMemoriaLoss);
                events.Add(new ParentaliaSkippedEvent(
                    state.EventIds.Issue(), context.Date, householdId, FuneraryCatalog.ParentaliaSkippedMemoriaLoss));
                continue;
            }

            var posted = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -cost),
                    new LedgerPosting(ParentaliaSink, cost),
                },
                reference: $"funerary:parentalia:{householdId.ToTaggedString()}:{context.Date.TotalMonths}");
            events.Add(posted);

            var gain = FuneraryCatalog.ParentaliaBaseMemoriaGain + FuneraryCatalog.ChronicleTrickle(state, householdId);
            MemoriaResolver.Apply(state, householdId, gain, context.Date);
            events.Add(new ParentaliaObservedEvent(state.EventIds.Issue(), context.Date, householdId, gain));
        }

        return events;
    }
}
