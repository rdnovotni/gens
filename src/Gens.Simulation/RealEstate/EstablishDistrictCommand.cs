using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Regions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>§4's District creation: names a new subdivision of a settlement that has reached <see
/// cref="RealEstateCatalog.MinimumStageForDistricts"/>, up to <see
/// cref="RealEstateCatalog.MaxDistrictsForStage"/>'s own soft cap for that settlement's current <see
/// cref="SettlementStage"/> (Phase 15 item 1).</summary>
public sealed record EstablishDistrictCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    string Name,
    DefinitionId<GazetteerLocationDefinition>? LinkedGazetteerLocationId = null) : ICommand;

public sealed record DistrictEstablishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<District> DistrictId,
    RuntimeId<Settlement> SettlementId,
    string Name,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.districtEstablished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { DistrictId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class EstablishDistrictCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("realEstate.establishDistrict.settlementNotFound");
    public static readonly ValidationErrorCode SettlementTooSmall = new("realEstate.establishDistrict.settlementTooSmall");
    public static readonly ValidationErrorCode DistrictCapReached = new("realEstate.establishDistrict.districtCapReached");
    public static readonly ValidationErrorCode EmptyName = new("realEstate.establishDistrict.emptyName");
    public static readonly ValidationErrorCode DuplicateGazetteerLink = new("realEstate.establishDistrict.duplicateGazetteerLink");

    public static readonly CommandPipeline<WorldState, EstablishDistrictCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EstablishDistrictCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out var settlement))
            return SettlementNotFound;
        if (settlement!.Stage < RealEstateCatalog.MinimumStageForDistricts)
            return SettlementTooSmall;
        if (string.IsNullOrWhiteSpace(command.Name))
            return EmptyName;

        var existing = state.Districts.InAscendingOrder()
            .Where(entry => entry.Value.SettlementId == command.SettlementId)
            .Select(entry => entry.Value)
            .ToArray();
        if (existing.Length >= RealEstateCatalog.MaxDistrictsForStage(settlement.Stage))
            return DistrictCapReached;
        if (command.LinkedGazetteerLocationId is { } gazetteerId &&
            existing.Any(district => district.LinkedGazetteerLocationId == gazetteerId))
            return DuplicateGazetteerLink;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EstablishDistrictCommand command)
    {
        var id = state.DistrictIds.Issue();
        var district = District.Create(id, command.SettlementId, command.Name, command.LinkedGazetteerLocationId);
        state.Districts.Add(id, district);

        return new IDomainEvent[]
        {
            new DistrictEstablishedEvent(
                state.EventIds.Issue(), command.SubmittedDate, id, command.SettlementId, command.Name,
                command.CommandId.ToTaggedString()),
        };
    }
}
