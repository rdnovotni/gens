using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

public sealed record ShipRepairedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MerchantShip> ShipId,
    int PreviousCondition,
    int NewCondition,
    string? CausationId) : IDomainEvent
{
    public string Type => "shipping.shipRepaired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§7's "recoverable through the same Repair action" (Phase 15 item 8), mirroring <see
/// cref="PrivateInfrastructure.RepairInfrastructureCommand"/>'s own shape directly, reading and writing
/// <see cref="MerchantShip.Condition"/> on the Ship record itself rather than a separate keyed
/// partition (<see cref="MerchantShip"/>'s own doc comment explains why).</summary>
public sealed record RepairShipCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<MerchantShip> ShipId,
    RuntimeId<Household> PayingHouseholdId) : ICommand;

public static class RepairShipCommands
{
    public static readonly ValidationErrorCode ShipNotFound = new("shipping.repairShip.shipNotFound");
    public static readonly ValidationErrorCode ShipNotRepairable = new("shipping.repairShip.shipNotRepairable");
    public static readonly ValidationErrorCode AlreadyPristine = new("shipping.repairShip.alreadyPristine");
    public static readonly ValidationErrorCode InsufficientFunds = new("shipping.repairShip.insufficientFunds");

    public static readonly CommandPipeline<WorldState, RepairShipCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RepairShipCommand command)
    {
        if (!MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship))
            return ShipNotFound;
        if (ship.Status is not (ShipStatus.Active or ShipStatus.Damaged))
            return ShipNotRepairable;
        if (ship.Condition.Value >= LandCondition.Pristine.Value)
            return AlreadyPristine;

        var cost = RepairCost(ship.Condition.Value);
        var balance = BalanceOf(state, command.PayingHouseholdId);
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RepairShipCommand command)
    {
        MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship);
        var cost = RepairCost(ship.Condition.Value);
        var pointsRestored = Math.Min(ShippingCatalog.RepairConditionRestored, 100 - ship.Condition.Value);
        var newCondition = new LandCondition(Math.Min(100, ship.Condition.Value + pointsRestored));

        var events = new List<IDomainEvent>();
        var account = LedgerAccountKey.ForHousehold(command.PayingHouseholdId);
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Upkeep,
            new[]
            {
                new LedgerPosting(account, -cost),
                new LedgerPosting(LedgerAccountKey.Mint, cost),
            },
            reference: $"shipping.repair:{command.ShipId.ToTaggedString()}"));

        var previousCondition = ship.Condition.Value;
        MerchantShipResolver.Set(state, ship with { Condition = newCondition, Status = ShipStatus.Active });

        events.Add(new ShipRepairedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.ShipId, previousCondition, newCondition.Value,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static Money RepairCost(int currentCondition)
    {
        var pointsRestored = Math.Min(ShippingCatalog.RepairConditionRestored, 100 - currentCondition);
        return ShippingCatalog.RepairCostPerConditionPoint.Scale(Numerics.Fixed64.FromInt(pointsRestored));
    }

    internal static Money BalanceOf(WorldState state, RuntimeId<Household> householdId) =>
        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;
}
