using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>
/// §11's Portfolio Scale &amp; Oversight (Phase 15 item 1): "each additional significant Property
/// Record beyond a soft threshold adds to the household's own oversight cost... or, if left unmatched,
/// a real management-quality decay." This item does not have Companions &amp; Court Positions'
/// Overseer/Procurator hiring to offset that cost with (confirmed unbuilt, Phase 17 — the same gap this
/// namespace's other doc comments already name) — §11's own closing line is what this system actually
/// implements instead: "leasing out a mature holding isn't forced, but it's the historically honest and
/// mechanically rewarded way to keep growing past a certain scale." A <see
/// cref="PropertyManagementStatus.LeasedOut"/> property has already engaged this item's one real
/// delegation tool (an assigned Operator, §6) and does not count against a household's burden at all;
/// only <see cref="PropertyManagementStatus.DirectlyManaged"/> properties held past <see
/// cref="RealEstateCatalog.AdministrativeBurdenFreeThreshold"/> cost anything, posted monthly as a real
/// Ledger expense (§11's "a genuine Economy &amp; Finance expense line") rather than the alternative
/// condition-decay branch §11 also allows — decay needs a "which property decays" allocation rule §11
/// does not specify, so this item picks the one branch it can implement without inventing that rule,
/// a real, reasoned narrowing rather than an oversight.
/// </summary>
public sealed class AdministrativeBurdenSystem : IMonthlySystem<WorldState>
{
    public string Id => "realEstate.administrativeBurden";
    public TickPhase Phase => TickPhase.MarketsLedger;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "plots", "plotPropertyExtensions", "propertyRecords" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "ledgerAccounts", "ledgerTransactions", "ledgerTransactionIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "realEstate.operatorLifecycle" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var directlyManagedCountByHousehold = new SortedDictionary<RuntimeId<Household>, int>();

        foreach (var plotEntry in state.Plots.InAscendingOrder())
            Count(directlyManagedCountByHousehold, PropertySubjectRef.ForPlot(plotEntry.Key), state);

        foreach (var recordEntry in state.PropertyRecords.InAscendingOrder())
            Count(directlyManagedCountByHousehold, PropertySubjectRef.ForPropertyRecord(recordEntry.Key), state);

        foreach (var (householdId, count) in directlyManagedCountByHousehold)
        {
            var overThreshold = count - RealEstateCatalog.AdministrativeBurdenFreeThreshold;
            if (overThreshold <= 0)
                continue;

            var cost = RealEstateCatalog.AdministrativeBurdenCostPerProperty.Scale(Fixed64.FromInt(overThreshold));
            if (cost == Money.Zero)
                continue;

            var householdAccount = LedgerAccountKey.ForHousehold(householdId);
            var ledgerEvent = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Upkeep,
                new[] { new LedgerPosting(householdAccount, -cost), new LedgerPosting(LedgerAccountKey.Mint, cost) },
                reference: $"realEstate.administrativeBurden:{householdId.ToTaggedString()}");
            events.Add(ledgerEvent);
            events.Add(new AdministrativeBurdenAssessedEvent(state.EventIds.Issue(), context.Date, householdId, count, overThreshold, cost));
        }

        return events;
    }

    private static void Count(SortedDictionary<RuntimeId<Household>, int> counts, PropertySubjectRef subject, WorldState state)
    {
        if (!PropertyResolver.TryResolve(state, subject, out var view))
            return;
        if (view.ManagementStatus != PropertyManagementStatus.DirectlyManaged)
            return;
        if (view.Owner.Kind != PropertyOwnerKind.PlayerHousehold)
            return;

        var householdId = RuntimeId<Household>.Parse(view.Owner.OwnerId!);
        counts[householdId] = counts.TryGetValue(householdId, out var existing) ? existing + 1 : 1;
    }
}

/// <summary>Emitted for every household whose Directly Managed portfolio actually clears <see
/// cref="RealEstateCatalog.AdministrativeBurdenFreeThreshold"/> this month (§11) — silent for a
/// household under the threshold, matching <see cref="Hazards.NaturalDisasterSystem"/>'s own "only
/// emitted when the mechanic actually bites" convention.</summary>
public sealed record AdministrativeBurdenAssessedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int DirectlyManagedPropertyCount,
    int PropertiesOverThreshold,
    Money Cost) : IDomainEvent
{
    public string Type => "realEstate.administrativeBurdenAssessed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
