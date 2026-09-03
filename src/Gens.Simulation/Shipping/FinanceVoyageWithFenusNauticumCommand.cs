using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

public sealed record ShipFinancedWithFenusNauticumEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MerchantShip> ShipId,
    RuntimeId<DebtRecord> DebtRecordId,
    Money Principal,
    string? CausationId) : IDomainEvent
{
    public string Type => "shipping.financedWithFenusNauticum";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §6.2/§7.1's real production path onto <see cref="MerchantShip.FenusNauticumRecordId"/> (Phase 15 item
/// 8) — until this command, no live path in this codebase ever populated that field outside save
/// restoration and test setup, leaving <see cref="ShipVoyageRiskSystem"/>'s own financed-voyage/
/// debt-forgiveness branch honestly dead in real play. This is the minimal real trigger the design doc's
/// own recap names: "the voyage is financed by a fenus nauticum... real money is riding on a binary
/// outcome" (§6.2) — a bottomry loan taken out against one specific, already-owned, seaworthy Ship,
/// opened through <see cref="DebtService.IssueLoan"/> with <c>isFenusNauticum: true</c> exactly as that
/// service's own doc comment already expects a real command to call it, then attached to the Ship record
/// itself so <see cref="ShipVoyageRiskSystem"/>'s <see cref="VoyageTriggerReason.FenusNauticumFinanced"/>
/// qualification and its own real "a Ship lost while financed this way simply forgives the associated
/// debt" mutation both have a real Ship to reach. A Ship may carry at most one live fenus nauticum loan
/// at a time (<see cref="ShippingCommands.AlreadyFinanced"/>) — the design doc never describes stacking
/// bottomry loans on the same hull.
/// </summary>
public sealed record FinanceVoyageWithFenusNauticumCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<MerchantShip> ShipId,
    Money Principal,
    Fixed64? InterestRate = null) : ICommand;

public static class ShippingCommands
{
    public static readonly ValidationErrorCode ShipNotFound = new("shipping.financeVoyage.shipNotFound");
    public static readonly ValidationErrorCode NotOwned = new("shipping.financeVoyage.notOwned");
    public static readonly ValidationErrorCode ShipNotActive = new("shipping.financeVoyage.shipNotActive");
    public static readonly ValidationErrorCode AlreadyFinanced = new("shipping.financeVoyage.alreadyFinanced");
    public static readonly ValidationErrorCode InvalidPrincipal = new("shipping.financeVoyage.invalidPrincipal");

    public static readonly CommandPipeline<WorldState, FinanceVoyageWithFenusNauticumCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FinanceVoyageWithFenusNauticumCommand command)
    {
        if (!MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship))
            return ShipNotFound;
        if (ship.ActualOwnerHouseholdId != command.HouseholdId)
            return NotOwned;
        if (ship.Status != ShipStatus.Active)
            return ShipNotActive;
        if (ship.FenusNauticumRecordId is not null)
            return AlreadyFinanced;
        if (command.Principal.RawValue <= 0)
            return InvalidPrincipal;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FinanceVoyageWithFenusNauticumCommand command)
    {
        MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship);

        var debt = DebtService.IssueLoan(
            state, command.SubmittedDate, ship.HomeSettlementId, command.HouseholdId, command.Principal,
            command.InterestRate, DebtOrigin.Loan, isFenusNauticum: true);

        MerchantShipResolver.Set(state, ship with { FenusNauticumRecordId = debt.Id });

        return new IDomainEvent[]
        {
            new ShipFinancedWithFenusNauticumEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ShipId, debt.Id, command.Principal,
                command.CommandId.ToTaggedString()),
        };
    }
}
