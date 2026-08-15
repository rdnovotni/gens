using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Land;

/// <summary>Common acquisition hook used by purchases, grants, conquest, inheritance, and legal
/// seizure. Pricing and political/legal resolution happen upstream; this command owns the atomic
/// transfer into land state.</summary>
public sealed record AcquirePlotCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Plot> PlotId,
    string NewOwnerId,
    AcquisitionMethod Method,
    string? SourceId = null) : ICommand;

public sealed record PlotAcquiredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Plot> PlotId,
    string OwnerId,
    AcquisitionMethod Method,
    string? SourceId,
    string? CausationId) : IDomainEvent
{
    public string Type => "land.plotAcquired";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString(), OwnerId };
    public Visibility Visibility => Visibility.Public;
}

public static class AcquirePlotCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("land.acquire.plotNotFound");
    public static readonly ValidationErrorCode InvalidOwner = new("land.acquire.invalidOwner");
    public static readonly ValidationErrorCode InvalidMethod = new("land.acquire.invalidMethod");
    public static readonly ValidationErrorCode AlreadyOwned = new("land.acquire.alreadyOwned");
    public static readonly ValidationErrorCode PlotContested = new("land.acquire.plotContested");

    public static readonly CommandPipeline<WorldState, AcquirePlotCommand> Pipeline = new(
        Validate,
        Mutate,
        static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AcquirePlotCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out var plot))
            return PlotNotFound;
        if (string.IsNullOrWhiteSpace(command.NewOwnerId))
            return InvalidOwner;
        if (!Enum.IsDefined(typeof(AcquisitionMethod), command.Method))
            return InvalidMethod;
        if (plot.OwnerId is not null)
            return AlreadyOwned;
        if (plot.IsContested)
            return PlotContested;
        return null;
    }

    private static IReadOnlyList<IDomainEvent> Mutate(WorldState state, AcquirePlotCommand command)
    {
        state.Plots.TryGet(command.PlotId, out var plot);
        state.Plots.Remove(command.PlotId);
        state.Plots.Add(command.PlotId, plot with
        {
            OwnerId = command.NewOwnerId,
            Acquisition = new LandAcquisition(command.Method, command.SubmittedDate, command.SourceId),
        });

        return new IDomainEvent[]
        {
            new PlotAcquiredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.PlotId, command.NewOwnerId,
                command.Method, command.SourceId, command.CommandId.ToTaggedString()),
        };
    }
}
