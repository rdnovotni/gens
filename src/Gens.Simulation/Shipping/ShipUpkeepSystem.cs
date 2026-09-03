using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

public sealed record ShipUpkeepAssessedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MerchantShip> ShipId,
    bool Paid,
    Money Cost,
    int PreviousCondition,
    int NewCondition) : IDomainEvent
{
    public string Type => "shipping.upkeepAssessed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// §7's "a Ship ages and accumulates wear the same way an Estate &amp; Settlement building does" (Phase
/// 15 item 8), matching <see cref="PrivateInfrastructure.InfrastructureUpkeepSystem"/>'s identical
/// shape: every Ship still in active service (<see cref="ShipStatus.Active"/> or <see
/// cref="ShipStatus.Damaged"/> — a Lost, Captured, Retired, or Sold Ship costs its former owner nothing
/// further) posts a real monthly Ledger expense from the actual owning household's account (<see
/// cref="MerchantShip.ActualOwnerHouseholdId"/>, per §11's "always tracked, regardless of ownerType" —
/// upkeep is a real, felt cost regardless of whether the Ship is Sole, Societas, or Fronted); an unpaid
/// month costs the Ship condition points instead, at §3.1's own Build-Quality-scaled rate (<see
/// cref="ShippingCatalog.UnpaidUpkeepConditionLoss"/>) rather than <see
/// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.UnpaidUpkeepConditionLoss"/>'s single flat
/// figure.
/// </summary>
public static class ShipUpkeepSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.MerchantShips.InAscendingOrder().ToArray())
        {
            var ship = entry.Value;
            if (ship.Status is not (ShipStatus.Active or ShipStatus.Damaged))
                continue;

            var upkeep = ShippingCatalog.MonthlyUpkeep(ShippingCatalog.CapacityTierFor(ship.VesselClass));
            var account = LedgerAccountKey.ForHousehold(ship.ActualOwnerHouseholdId);
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
                    reference: $"shipping.upkeep:{ship.Id.ToTaggedString()}"));
            }

            var loss = ShippingCatalog.UnpaidUpkeepConditionLoss(ship.BuildQuality);
            var newConditionValue = paid ? ship.Condition.Value : Math.Max(0, ship.Condition.Value - loss);
            var newCondition = new LandCondition(newConditionValue);

            events.Add(new ShipUpkeepAssessedEvent(state.EventIds.Issue(), date, ship.Id, paid, upkeep, ship.Condition.Value, newConditionValue));
            MerchantShipResolver.Set(state, ship with { Condition = newCondition });
        }

        return events;
    }
}
