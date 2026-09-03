using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PrivateInfrastructure;

/// <summary>§6's private Bridge (Phase 15 item 7) — a household's own two-Plot river crossing,
/// explicitly distinct from Public Works &amp; Euergetism's civic Bridge (that document's §3), which
/// connects entire Districts at settlement scale. This item's own honest scope narrowing on §2's own
/// adjacency gap applies here too: with no spatial adjacency graph anywhere in this codebase, this
/// command validates the one real, checkable fact instead — at least one of the two Plots is genuinely
/// River terrain or River-adjacent (§6's own "river crossing" framing), and both belong to the same
/// household — rather than a geometric "opposite banks" check nothing in <see cref="Land.Plot"/> could
/// support.</summary>
public sealed record PrivateBridge
{
    public required RuntimeId<PrivateBridge> BridgeId { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }
    public required RuntimeId<Household> HouseholdId { get; init; }
    public required RuntimeId<Plot> PlotAId { get; init; }
    public required RuntimeId<Plot> PlotBId { get; init; }
    public required bool RiverCrossing { get; init; }
    public required GameDate BuiltDate { get; init; }

    public InfrastructureConditionKey ConditionKey => new(InfrastructureStructureType.PrivateBridge, BridgeId.ToTaggedString());

    public static PrivateBridge Create(
        RuntimeId<PrivateBridge> bridgeId, RuntimeId<Settlement> settlementId, RuntimeId<Household> householdId,
        RuntimeId<Plot> plotAId, RuntimeId<Plot> plotBId, bool riverCrossing, GameDate builtDate) => new()
        {
            BridgeId = bridgeId,
            SettlementId = settlementId,
            HouseholdId = householdId,
            PlotAId = plotAId,
            PlotBId = plotBId,
            RiverCrossing = riverCrossing,
            BuiltDate = builtDate,
        };
}

public sealed record PrivateBridgeBuiltEvent(
    RuntimeId<DomainEventEntity> EventId, GameDate OccurredDate, RuntimeId<PrivateBridge> BridgeId,
    RuntimeId<Household> HouseholdId, RuntimeId<Plot> PlotAId, RuntimeId<Plot> PlotBId, string? CausationId) : IDomainEvent
{
    public string Type => "privateInfrastructure.privateBridgeBuilt";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), PlotAId.ToTaggedString(), PlotBId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public sealed record BuildPrivateBridgeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Plot> PlotAId,
    RuntimeId<Plot> PlotBId) : ICommand;

public static class BuildPrivateBridgeCommands
{
    public static readonly ValidationErrorCode SamePlot = new("privateInfrastructure.buildPrivateBridge.samePlot");
    public static readonly ValidationErrorCode PlotNotFound = new("privateInfrastructure.buildPrivateBridge.plotNotFound");
    public static readonly ValidationErrorCode DifferentSettlements = new("privateInfrastructure.buildPrivateBridge.differentSettlements");
    public static readonly ValidationErrorCode NotOwnedByHousehold = new("privateInfrastructure.buildPrivateBridge.notOwnedByHousehold");
    public static readonly ValidationErrorCode NoRiverCrossing = new("privateInfrastructure.buildPrivateBridge.noRiverCrossing");
    public static readonly ValidationErrorCode InsufficientFunds = new("privateInfrastructure.buildPrivateBridge.insufficientFunds");

    public static readonly CommandPipeline<WorldState, BuildPrivateBridgeCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BuildPrivateBridgeCommand command)
    {
        if (command.PlotAId == command.PlotBId)
            return SamePlot;
        if (!state.Plots.TryGet(command.PlotAId, out var plotA) || !state.Plots.TryGet(command.PlotBId, out var plotB))
            return PlotNotFound;
        if (plotA!.SettlementId != plotB!.SettlementId)
            return DifferentSettlements;
        if (!PrivateInfrastructureOwnership.OwnedByHousehold(plotA, command.HouseholdId) ||
            !PrivateInfrastructureOwnership.OwnedByHousehold(plotB, command.HouseholdId))
            return NotOwnedByHousehold;
        if (!IsRiverAdjacent(plotA) && !IsRiverAdjacent(plotB))
            return NoRiverCrossing;

        var account = LedgerAccountKey.ForHousehold(command.HouseholdId);
        var balance = state.LedgerAccounts.TryGet(account, out var ledgerAccount) ? ledgerAccount!.Balance : Money.Zero;
        if (balance.RawValue < PrivateInfrastructureCatalog.PrivateBridgeConstructionCost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BuildPrivateBridgeCommand command)
    {
        state.Plots.TryGet(command.PlotAId, out var plotA);

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Construction,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -PrivateInfrastructureCatalog.PrivateBridgeConstructionCost),
                new LedgerPosting(LedgerAccountKey.Mint, PrivateInfrastructureCatalog.PrivateBridgeConstructionCost),
            },
            reference: $"privateInfrastructure.buildPrivateBridge:{command.PlotAId.ToTaggedString()}:{command.PlotBId.ToTaggedString()}"));

        var bridgeId = state.PrivateBridgeIds.Issue();
        var bridge = PrivateBridge.Create(
            bridgeId, plotA!.SettlementId, command.HouseholdId, command.PlotAId, command.PlotBId, riverCrossing: true, command.SubmittedDate);
        state.PrivateBridges.Add(bridgeId, bridge);
        InfrastructureConditionResolver.Seed(state, bridge.ConditionKey);

        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotAId);
        PrivateInfrastructureOwnership.BumpPropertyValue(state, command.PlotBId);

        events.Add(new PrivateBridgeBuiltEvent(
            state.EventIds.Issue(), command.SubmittedDate, bridgeId, command.HouseholdId, command.PlotAId, command.PlotBId,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static bool IsRiverAdjacent(Plot plot) =>
        plot.Terrain == TerrainType.River || plot.Features.HasFlag(TerrainFeature.RiverAdjacent);
}
