using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>
/// §6's uncontested dissolution path (Phase 15 item 2): "a dissolution reached without dispute —
/// mutual agreement, a natural Societas Unius Rei completion, or an amicable Societas Omnium Bonorum
/// wind-down — simply resolves... no case required." Every <see cref="SocietasDissolutionTrigger"/>
/// except <see cref="SocietasDissolutionTrigger.Fraud"/> is reachable through this command directly;
/// <see cref="SocietasDissolutionTrigger.Fraud"/> is applied only by <see
/// cref="ActioProSocioResolutionHook"/>, once a real Legal &amp; Court verdict actually confirms the
/// fraud a <see cref="PartnerDisputeType.SuspectedFraud"/> case alleged (§6's own contested/uncontested
/// split).
/// </summary>
public sealed record DissolveSocietasCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Societas> SocietasId,
    SocietasDissolutionTrigger Trigger) : ICommand;

public sealed record SocietasDissolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Societas> SocietasId,
    SocietasDissolutionTrigger Trigger,
    string? CausationId) : IDomainEvent
{
    public string Type => "societates.dissolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SocietasId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class DissolveSocietasCommands
{
    public static readonly ValidationErrorCode SocietasNotFound = new("societates.dissolve.societasNotFound");
    public static readonly ValidationErrorCode AlreadyDissolved = new("societates.dissolve.alreadyDissolved");
    public static readonly ValidationErrorCode FraudRequiresActioProSocio = new("societates.dissolve.fraudRequiresActioProSocio");

    public static readonly CommandPipeline<WorldState, DissolveSocietasCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DissolveSocietasCommand command)
    {
        if (!state.Societates.TryGet(command.SocietasId, out var societas))
            return SocietasNotFound;
        if (!societas!.IsActive)
            return AlreadyDissolved;
        if (command.Trigger == SocietasDissolutionTrigger.Fraud && command.ActorId != "system")
            return FraudRequiresActioProSocio;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DissolveSocietasCommand command)
    {
        state.Societates.TryGet(command.SocietasId, out var societas);
        state.Societates.Remove(command.SocietasId);
        state.Societates.Add(command.SocietasId, societas! with
        {
            IsActive = false,
            DissolutionTrigger = command.Trigger,
            DissolvedDate = command.SubmittedDate,
        });

        return new IDomainEvent[]
        {
            new SocietasDissolvedEvent(state.EventIds.Issue(), command.SubmittedDate, command.SocietasId, command.Trigger, command.CommandId.ToTaggedString()),
        };
    }
}
