using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

public sealed record InfrastructureUpkeepAssessedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    InfrastructureConditionKey Key,
    bool Paid,
    Money Cost,
    int PreviousCondition,
    int NewCondition) : IDomainEvent
{
    public string Type => "privateInfrastructure.upkeepAssessed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Key.StructureTag };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// §8's "modest, real, recurring upkeep cost, folded into Economy &amp; Finance's expense total exactly
/// like any Estate &amp; Settlement building's own upkeep" (Phase 15 item 7), matching <see
/// cref="RealEstate.AdministrativeBurdenSystem"/>'s identical shape: for every built structure this
/// namespace tracks, posts a real monthly Ledger expense from the owning household's account when the
/// balance covers it; an unpaid month costs the structure <see
/// cref="PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss"/> condition points instead (§8's
/// "neglect degrades the improvement's own effect over time"), floored at zero and recoverable only
/// through a real <see cref="RepairInfrastructureCommand"/>, never by upkeep alone. A Road Cluster's
/// own Connected Estate/Trade-Proximity benefits (<see cref="PrivateInfrastructureBenefitsSystem"/>)
/// already read a structure's own condition through <see
/// cref="InfrastructureConditionResolver.IsOperational"/>, so a chronically unpaid structure's real
/// effect lapses on its own without this system needing to separately toggle anything off.
/// </summary>
public static class InfrastructureUpkeepSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.PavedRoadConnections.InAscendingOrder())
            Assess(state, date, entry.Value.ConditionKey, entry.Value.HouseholdId, PrivateInfrastructureCatalog.PavedRoadMonthlyUpkeep, events);

        foreach (var entry in state.IrrigationCanals.InAscendingOrder())
            AssessForPlot(state, date, entry.Value.ConditionKey, entry.Key, PrivateInfrastructureCatalog.IrrigationCanalMonthlyUpkeep, events);

        foreach (var entry in state.WellOrCisterns.InAscendingOrder())
        {
            var upkeep = entry.Value.Type == WellOrCisternType.Cistern
                ? PrivateInfrastructureCatalog.CisternMonthlyUpkeep
                : PrivateInfrastructureCatalog.WellMonthlyUpkeep;
            AssessForPlot(state, date, entry.Value.ConditionKey, entry.Key, upkeep, events);
        }

        foreach (var entry in state.PrivateBridges.InAscendingOrder())
            Assess(state, date, entry.Value.ConditionKey, entry.Value.HouseholdId, PrivateInfrastructureCatalog.PrivateBridgeMonthlyUpkeep, events);

        foreach (var entry in state.BoundaryInfrastructures.InAscendingOrder())
        {
            var upkeep = entry.Value.Type == BoundaryInfrastructureType.Wall
                ? PrivateInfrastructureCatalog.WallMonthlyUpkeep
                : PrivateInfrastructureCatalog.FenceMonthlyUpkeep;
            AssessForPlot(state, date, entry.Value.ConditionKey, entry.Key, upkeep, events);
        }

        return events;
    }

    private static void AssessForPlot(
        WorldState state, GameDate date, InfrastructureConditionKey key, RuntimeId<Land.Plot> plotId, Money upkeep, List<IDomainEvent> events)
    {
        if (!state.Plots.TryGet(plotId, out var plot) || plot!.OwnerId is null)
            return;
        RealEstate.PropertyOwnerRef owner;
        try
        {
            owner = RealEstate.PropertyOwnerRef.Parse(plot.OwnerId);
        }
        catch (FormatException)
        {
            return;
        }
        if (owner.Kind != RealEstate.PropertyOwnerKind.PlayerHousehold || owner.OwnerId is not { } ownerId)
            return;

        Assess(state, date, key, RuntimeId<Household>.Parse(ownerId), upkeep, events);
    }

    private static void Assess(
        WorldState state, GameDate date, InfrastructureConditionKey key, RuntimeId<Household> householdId, Money upkeep,
        List<IDomainEvent> events)
    {
        var current = InfrastructureConditionResolver.Current(state, key);
        var account = LedgerAccountKey.ForHousehold(householdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        var paid = balance.RawValue >= upkeep.RawValue;

        if (paid)
        {
            events.Add(LedgerService.Post(
                state, date, LedgerTransactionCategory.Upkeep,
                new[]
                {
                    new LedgerPosting(account, -upkeep),
                    new LedgerPosting(LedgerAccountKey.Mint, upkeep),
                },
                reference: $"privateInfrastructure.upkeep:{key.StructureType}:{key.StructureTag}"));
        }

        var newCondition = paid
            ? current.Condition
            : Math.Max(0, current.Condition - PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss);

        InfrastructureConditionResolver.Set(state, current with { Condition = newCondition });
        events.Add(new InfrastructureUpkeepAssessedEvent(state.EventIds.Issue(), date, key, paid, upkeep, current.Condition, newCondition));
    }
}
