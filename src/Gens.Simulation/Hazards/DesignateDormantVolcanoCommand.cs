using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Hazards;

/// <summary>Marks one Plot as carrying a §2.2 <see cref="DormantVolcano"/>. The explicit, callerless
/// "hook" a future map-generation pass will call once it exists — see <see
/// cref="DormantVolcano"/>'s own doc comment for the full disclosure of what this command does and, just
/// as importantly, does not yet wire up.</summary>
public sealed record DesignateDormantVolcanoCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Plot> PlotId) : ICommand;

/// <summary>Emitted whenever a <see cref="DesignateDormantVolcanoCommand"/> is accepted.</summary>
public sealed record DormantVolcanoDesignatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Plot> PlotId,
    RuntimeId<Settlement> SettlementId,
    string? CausationId) : IDomainEvent
{
    public string Type => "hazards.dormantVolcanoDesignated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlotId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="DesignateDormantVolcanoCommand"/> (ADR 0006).</summary>
public static class DesignateDormantVolcanoCommands
{
    public static readonly ValidationErrorCode PlotNotFound = new("hazards.dormantVolcano.plotNotFound");
    public static readonly ValidationErrorCode AlreadyDesignated = new("hazards.dormantVolcano.alreadyDesignated");

    public static readonly CommandPipeline<WorldState, DesignateDormantVolcanoCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DesignateDormantVolcanoCommand command)
    {
        if (!state.Plots.TryGet(command.PlotId, out _))
            return PlotNotFound;
        if (state.DormantVolcanoes.TryGet(command.PlotId, out _))
            return AlreadyDesignated;
        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DesignateDormantVolcanoCommand command)
    {
        state.Plots.TryGet(command.PlotId, out var plot);
        state.DormantVolcanoes.Add(command.PlotId, DormantVolcano.Create(command.PlotId, plot.SettlementId));

        return new IDomainEvent[]
        {
            new DormantVolcanoDesignatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.PlotId, plot.SettlementId,
                command.CommandId.ToTaggedString()),
        };
    }
}
