using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

public sealed record ShipAssignedToTradeRouteEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MerchantShip> ShipId,
    RuntimeId<StandingContract> TradeRouteId,
    string? CausationId) : IDomainEvent
{
    public string Type => "shipping.assignedToTradeRoute";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §6.1's aggregate default (Phase 15 item 8): "a Ship assigned to an ordinary, already-established
/// Trade Route... simply contributes its own Capacity Tier and Condition as a direct multiplier on that
/// route's existing aggregate monthly output." This command sets <see
/// cref="MerchantShip.AssignedTradeRouteId"/>, the real, persisted assignment §6.1 describes — but the
/// multiplier itself is honestly not wired: <see cref="Economy.StandingContract"/>'s own doc comment
/// already discloses (Phase 15 item 7's identical finding, confirmed by direct search still holding) that
/// <see cref="StandingContractKind.TradeRouteInvestment"/> is "only a persisted record of a one-off
/// commitment"— no live, recurring "Trade Route effectiveness" figure exists anywhere in this codebase
/// for a Ship's Capacity Tier and Condition to actually multiply. <see
/// cref="ShipVoyageRiskSystem"/> is this item's own real consumer of the assignment instead, for the
/// narrower §6.2 discrete-Voyage-Event case.
/// </summary>
public sealed record AssignShipToTradeRouteCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<MerchantShip> ShipId,
    RuntimeId<StandingContract> TradeRouteId) : ICommand;

public static class AssignShipToTradeRouteCommands
{
    public static readonly ValidationErrorCode ShipNotFound = new("shipping.assignShipToTradeRoute.shipNotFound");
    public static readonly ValidationErrorCode NotOwned = new("shipping.assignShipToTradeRoute.notOwned");
    public static readonly ValidationErrorCode ShipNotActive = new("shipping.assignShipToTradeRoute.shipNotActive");
    public static readonly ValidationErrorCode NotATradeVessel = new("shipping.assignShipToTradeRoute.notATradeVessel");
    public static readonly ValidationErrorCode TradeRouteNotFound = new("shipping.assignShipToTradeRoute.tradeRouteNotFound");
    public static readonly ValidationErrorCode NotATradeRouteContract = new("shipping.assignShipToTradeRoute.notATradeRouteContract");
    public static readonly ValidationErrorCode TradeRouteNotOwned = new("shipping.assignShipToTradeRoute.tradeRouteNotOwned");
    public static readonly ValidationErrorCode TradeRouteNotActive = new("shipping.assignShipToTradeRoute.tradeRouteNotActive");

    public static readonly CommandPipeline<WorldState, AssignShipToTradeRouteCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AssignShipToTradeRouteCommand command)
    {
        if (!MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship))
            return ShipNotFound;
        if (ship.ActualOwnerHouseholdId != command.HouseholdId)
            return NotOwned;
        if (ship.Status != ShipStatus.Active)
            return ShipNotActive;
        if (!ShippingCatalog.IsTradeVessel(ship.VesselClass))
            return NotATradeVessel;
        if (!state.StandingContracts.TryGet(command.TradeRouteId, out var route))
            return TradeRouteNotFound;
        if (route!.Kind != StandingContractKind.TradeRouteInvestment)
            return NotATradeRouteContract;
        if (route.HouseholdId != command.HouseholdId)
            return TradeRouteNotOwned;
        if (route.Status != StandingContractStatus.Active)
            return TradeRouteNotActive;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AssignShipToTradeRouteCommand command)
    {
        MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship);
        MerchantShipResolver.Set(state, ship with { AssignedTradeRouteId = command.TradeRouteId });

        return new IDomainEvent[]
        {
            new ShipAssignedToTradeRouteEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ShipId, command.TradeRouteId, command.CommandId.ToTaggedString()),
        };
    }
}
