using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>§3's two new asset types coming into existence (Phase 15 item 1) — a Ship freshly built or
/// bought new, or a Named Holding a Temple/Collegium/Rival Gens is recognized as owning. This is the
/// creation path; §5's <see cref="TransferPropertyCommand"/> is the separate "it already exists and
/// already has an owner" acquisition path, matching <see cref="AcquirePlotCommand"/>'s and this
/// namespace's own identical "raw creation" vs. "acquiring what's already built" split.</summary>
public sealed record RegisterPropertyRecordCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertyAssetType AssetType,
    string Name,
    PropertyOwnerRef Owner,
    Money Value,
    RuntimeId<Settlement>? SettlementId = null,
    RuntimeId<District>? DistrictId = null) : ICommand;

public sealed record PropertyRecordRegisteredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PropertyRecord> PropertyRecordId,
    PropertyAssetType AssetType,
    PropertyOwnerRef Owner,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.propertyRecordRegistered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PropertyRecordId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class RegisterPropertyRecordCommands
{
    public static readonly ValidationErrorCode EmptyName = new("realEstate.registerProperty.emptyName");
    public static readonly ValidationErrorCode NegativeValue = new("realEstate.registerProperty.negativeValue");
    public static readonly ValidationErrorCode SettlementNotFound = new("realEstate.registerProperty.settlementNotFound");
    public static readonly ValidationErrorCode ShipCannotHaveSettlement = new("realEstate.registerProperty.shipCannotHaveSettlement");
    public static readonly ValidationErrorCode NamedHoldingRequiresSettlement = new("realEstate.registerProperty.namedHoldingRequiresSettlement");
    public static readonly ValidationErrorCode DistrictNotFound = new("realEstate.registerProperty.districtNotFound");
    public static readonly ValidationErrorCode DistrictWrongSettlement = new("realEstate.registerProperty.districtWrongSettlement");

    public static readonly CommandPipeline<WorldState, RegisterPropertyRecordCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RegisterPropertyRecordCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return EmptyName;
        if (command.Value.IsNegative)
            return NegativeValue;
        if (command.AssetType == PropertyAssetType.Ship && command.SettlementId is not null)
            return ShipCannotHaveSettlement;
        if (command.AssetType == PropertyAssetType.NamedHolding && command.SettlementId is null)
            return NamedHoldingRequiresSettlement;
        if (command.SettlementId is { } settlementId && !state.Settlements.TryGet(settlementId, out _))
            return SettlementNotFound;
        if (command.DistrictId is { } districtId)
        {
            if (!state.Districts.TryGet(districtId, out var district))
                return DistrictNotFound;
            if (command.SettlementId is null || district!.SettlementId != command.SettlementId)
                return DistrictWrongSettlement;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RegisterPropertyRecordCommand command)
    {
        var id = state.PropertyRecordIds.Issue();
        var record = PropertyRecord.Create(
            id, command.AssetType, command.Name, command.Owner, command.Value, command.SettlementId, command.DistrictId);
        state.PropertyRecords.Add(id, record);

        return new IDomainEvent[]
        {
            new PropertyRecordRegisteredEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.AssetType, command.Owner,
                command.CommandId.ToTaggedString()),
        };
    }
}
