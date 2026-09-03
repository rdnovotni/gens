using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>§2's Paved Road, "connecting two or more Plots" (Phase 15 item 7) — modeled as a single
/// paved edge between two of a household's own Plots rather than a per-Plot flag, since §4's Road
/// Cluster is exactly the connected-component graph these edges form. This item's own honest scope
/// narrowing: <c>Land.Plot</c> carries no spatial coordinate or adjacency graph anywhere in this
/// codebase (confirmed by direct search — no "adjacent Plots" concept exists for any purpose today), so
/// this command does not validate §2's own "connecting two adjacent Plots" geometrically; it validates
/// the one real, checkable fact this codebase can check instead — both Plots belong to the same
/// settlement and the same household — matching <see cref="Magistracies.HoldContestedElectionCommand"/>'s
/// own established "the caller supplies an already-resolved [fact this item cannot itself generate]"
/// precedent, applied here to spatial adjacency instead of a rival candidate.</summary>
public sealed record PavedRoadConnection
{
    public required RuntimeId<PavedRoadConnection> ConnectionId { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required RuntimeId<Household> HouseholdId { get; init; }
    public required RuntimeId<Plot> PlotAId { get; init; }
    public required RuntimeId<Plot> PlotBId { get; init; }
    public required GameDate BuiltDate { get; init; }

    public static PavedRoadConnection Create(
        RuntimeId<PavedRoadConnection> connectionId, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId,
        RuntimeId<Plot> plotAId, RuntimeId<Plot> plotBId, GameDate builtDate) => new()
        {
            ConnectionId = connectionId,
            SettlementId = settlementId,
            HouseholdId = householdId,
            PlotAId = plotAId,
            PlotBId = plotBId,
            BuiltDate = builtDate,
        };

    public InfrastructureConditionKey ConditionKey => new(InfrastructureStructureType.PavedRoad, ConnectionId.ToTaggedString());
}

public sealed record PavedRoadConnectionBuiltEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PavedRoadConnection> ConnectionId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotAId,
    RuntimeId<Plot> PlotBId,
    string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.pavedRoadConnectionBuilt";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), PlotAId.ToTaggedString(), PlotBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§2's Paved Road construction (Phase 15 item 7). §5's real, formalized cost (<see
/// cref="PrivateInfrastructureCatalog.PavedRoadConstructionCost"/>) is paid immediately from the
/// building household's own Ledger account.</summary>
public sealed record BuildPavedRoadConnectionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotAId,
    RuntimeId<Plot> PlotBId) : ICommand;

public static class BuildPavedRoadConnectionCommands
{
    public static readonly ValidationErrorCode SamePlot = new("privateInfrastructure.buildPavedRoad.samePlot");
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.buildPavedRoad.plotNotFound");
    public static readonly ValidationErrorCode DifferentSettlements = new("privateInfrastructure.buildPavedRoad.differentSettlements");
    public static readonly ValidationErrorCode NotOwnedByHousehold = new("privateInfrastructure.buildPavedRoad.notOwnedByHousehold");
    public static readonly ValidationErrorCode AlreadyConnected = new("privateInfrastructure.buildPavedRoad.alreadyConnected");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.buildPavedRoad.insufficientFunds");

    public static readonly CommandPipeline<WorldState, BuildPavedRoadConnectionCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BuildPavedRoadConnectionCommand command)
    {
        if (command.PlotAId == command.PlotBId)
            return SamePlot;
        if (!state.Plots.TryGet(command.PlotAId, out var plotA) || !state.Plots.TryGet(command.PlotBId, out var plotB))
            return PlotNotFound;
        if (plotA!.SettlementId != plotB!.SettlementId)
            return DifferentSettlements;
        if (!PrivateInfrastructureOwnership.OwnedByHousehold(plotA!, command.HouseholdId) ||
            !PrivateInfrastructureOwnership.OwnedByHousehold(plotB!, command.HouseholdId))
            return NotOwnedByHousehold;
        if (AlreadyConnectedPair(state, command.PlotAId, command.PlotBId))
            return AlreadyConnected;

        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < PrivateInfrastructureCatalog.PavedRoadConstructionCost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BuildPavedRoadConnectionCommand command)
    {
        state.Plots.TryGet(command.PlotAId, out var plotA);

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -PrivateInfrastructureCatalog.PavedRoadConstructionCost),
                new LedgerPosting(LedgerAccountKey.Mint, PrivateInfrastructureCatalog.PavedRoadConstructionCost),
            },
            reference: $"privateInfrastructure.buildPavedRoad:{command.PlotAId.ToTaggedString()}:{command.PlotBId.ToTaggedString()}"));

        var connectionId = state.PavedRoadConnectionIds.Issue();
        var connection = PavedRoadConnection.Create(
            connectionId, plotA!.SettlementId, command.HouseholdId, command.PlotAId, command.PlotBId, command.SubmittedDate);
        state.PavedRoadConnections.Add(connectionId, connection);
        InfrastructureConditionResolver.Seed(state, connection.ConditionKey);

        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotAId);
        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotBId);

        events.Add(new PavedRoadConnectionBuiltEvent(
            state.EventIds.Issue(), command.SubmittedDate, connectionId, command.HouseholdId, command.PlotAId, command.PlotBId,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static bool AlreadyConnectedPair(WorldState state, RuntimeId<Plot> plotAId, RuntimeId<Plot> plotBId) =>
        state.PavedRoadConnections.InAscendingOrder().Any(entry =>
            (entry.Value.PlotAId == plotAId && entry.Value.PlotBId == plotBId) ||
            (entry.Value.PlotAId == plotBId && entry.Value.PlotBId == plotAId));
}
