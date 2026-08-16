using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Land;

/// <summary>Links an existing holding to one parcel without conflating physical occupancy with title.</summary>
public sealed record OccupyPlotCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Plot> PlotId,
    RuntimeId<Holding> HoldingId) : ICommand;

public sealed record PlotOccupiedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Plot> PlotId,
    RuntimeId<Holding> HoldingId,
    string? CausationId) : IDomainEvent
{
    public string Type => "land.plotOccupied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString(), HoldingId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class OccupyPlotCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("land.occupy.plotNotFound");
    public static readonly ValidationErrorCode HoldingNotFound = new("land.occupy.holdingNotFound");
    public static readonly ValidationErrorCode SettlementMismatch = new("land.occupy.settlementMismatch");
    public static readonly ValidationErrorCode AlreadyOccupied = new("land.occupy.alreadyOccupied");
    public static readonly ValidationErrorCode OwnerMismatch = new("land.occupy.ownerMismatch");

    public static readonly CommandPipeline<WorldState, OccupyPlotCommand> Pipeline = new(
        Validate,
        Mutate,
        static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, OccupyPlotCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (!state.Holdings.TryGet(command.HoldingId, out var holding))
            return HoldingNotFound;
        if (plot.SettlementId != holding.SettlementId)
            return SettlementMismatch;
        if (plot.OccupyingHoldingId is not null)
            return AlreadyOccupied;
        if (plot.OwnerId is not null && holding.OwnerId != plot.OwnerId)
            return OwnerMismatch;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, OccupyPlotCommand command)
    {
        state.Plots.TryGet(command.PlotId, out var plot);
        state.Plots.Remove(command.PlotId);
        state.Plots.Add(command.PlotId, plot with { OccupyingHoldingId = command.HoldingId });
        return new IDomainEvent[]
        {
            new PlotOccupiedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.PlotId, command.HoldingId,
                command.CommandId.ToTaggedString()),
        };
    }
}
