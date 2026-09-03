using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Shipping;

/// <summary>Emitted whenever a <see cref="DesignateFlagshipCommand"/> is accepted. Public — §4 frames a
/// Flagship as "a visible, legible statement of the household's own maritime standing," the same
/// publicly-known reasoning <see cref="Reputation.DignitasChangedEvent"/>'s own doc comment already gives
/// for Dignitas generally.</summary>
public sealed record ShipDesignatedFlagshipEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<MerchantShip> ShipId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<MerchantShip>? PreviousFlagshipId,
    string? CausationId) : IDomainEvent
{
    public string Type => "shipping.designatedFlagship";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ShipId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §4's Flagship designation (Phase 15 item 8): "a household can re-designate a new Flagship at any
/// time... but only ever holds one at a time." <see cref="MerchantMarineQuery.FlagshipOf"/> is the single
/// source of truth for "which Ship, if any, currently holds the title" — this command is the one real
/// path (rule 2) that ever flips <see cref="MerchantShip.IsFlagship"/>, unsetting whichever Ship
/// previously held it in the same mutation per that section's own "only ever holds one at a time."
/// §4's own real Dignitas payoff ("real, standing Dignitas material simply by existing prominently") is
/// realized here as a one-time award (<see cref="ShippingCatalog.FlagshipDesignationDignitasAward"/>) at
/// the moment of designation, matching <see
/// cref="PrivateInfrastructure.PrivateInfrastructureCatalog.FullReclamationDignitasAward"/>'s identical
/// "real, invented, one-time achievement award" shape — this item does not build an ongoing monthly
/// Dignitas trickle for merely holding the title, since no other Phase 15 item's own asset grants one
/// either. §4's own re-designation cost/ceremony (§12's own open question) is left exactly that
/// open — a free, instant administrative choice.
/// </summary>
public sealed record DesignateFlagshipCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<MerchantShip> ShipId) : ICommand;

public static class DesignateFlagshipCommands
{
    public static readonly ValidationErrorCode ShipNotFound = new("shipping.designateFlagship.shipNotFound");
    public static readonly ValidationErrorCode NotOwned = new("shipping.designateFlagship.notOwned");
    public static readonly ValidationErrorCode ShipNotActive = new("shipping.designateFlagship.shipNotActive");
    public static readonly ValidationErrorCode AlreadyFlagship = new("shipping.designateFlagship.alreadyFlagship");

    public static readonly CommandPipeline<WorldState, DesignateFlagshipCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DesignateFlagshipCommand command)
    {
        if (!MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship))
            return ShipNotFound;
        if (ship.ActualOwnerHouseholdId != command.HouseholdId)
            return NotOwned;
        if (ship.Status != ShipStatus.Active)
            return ShipNotActive;
        if (ship.IsFlagship)
            return AlreadyFlagship;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DesignateFlagshipCommand command)
    {
        MerchantShipResolver.TryGetCurrent(state, command.ShipId, out var ship);

        var previous = MerchantMarineQuery.FlagshipOf(state, command.HouseholdId);
        if (previous is not null)
            MerchantShipResolver.Set(state, previous with { IsFlagship = false });

        MerchantShipResolver.Set(state, ship with { IsFlagship = true });

        var events = new List<IDomainEvent>
        {
            new ShipDesignatedFlagshipEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.ShipId, command.HouseholdId, previous?.Id,
                command.CommandId.ToTaggedString()),
        };

        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), "system", command.SubmittedDate, command.CommandId.ToTaggedString(), command.HouseholdId,
                ShippingCatalog.FlagshipDesignationDignitasAward, "designated a new Flagship")).Events);

        return events.ToArray();
    }
}
