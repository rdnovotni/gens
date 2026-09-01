using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.RealEstate;

/// <summary>§6's audit action (Phase 15 item 1): "detectable through an audit action, at the cost of
/// the player's own time and a relationship-web hit if the Operator turns out to have been honest all
/// along." <see cref="PropertyRecord.OperatorIsSkimming"/>/<see
/// cref="PlotPropertyExtension.OperatorIsSkimming"/> is already the ground truth — set monthly by <see
/// cref="OperatorLifecycleSystem"/> from the Operator's own Core Attributes/Loyalty — so this command
/// does not roll anything; it reveals that already-resolved truth and applies the one real
/// consequence §6 actually names for the honest-Operator branch.</summary>
public sealed record AuditPropertyOperatorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PropertySubjectRef Subject) : ICommand;

public sealed record PropertyOperatorAuditedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    PropertySubjectRef Subject,
    RuntimeId<Character> OperatorCharacterId,
    bool WasSkimming,
    string? CausationId) : IDomainEvent
{
    public string Type => "realEstate.propertyOperatorAudited";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { Subject.SubjectId, OperatorCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class AuditPropertyOperatorCommands
{
    public static readonly ValidationErrorCode SubjectNotFound = new("realEstate.audit.subjectNotFound");
    public static readonly ValidationErrorCode NoOperatorAssigned = new("realEstate.audit.noOperatorAssigned");

    public static readonly CommandPipeline<WorldState, AuditPropertyOperatorCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AuditPropertyOperatorCommand command)
    {
        if (!PropertyResolver.TryResolve(state, command.Subject, out var view))
            return SubjectNotFound;
        if (view.ManagementStatus != PropertyManagementStatus.LeasedOut || view.OperatorCharacterId is null)
            return NoOperatorAssigned;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AuditPropertyOperatorCommand command)
    {
        PropertyResolver.TryResolve(state, command.Subject, out var view);
        var operatorId = view.OperatorCharacterId!.Value;

        // §6's own honest-Operator consequence: an audit that turns up nothing still costs the
        // relationship — the accused-and-cleared Operator's own Loyalty takes the hit named directly.
        // A genuinely skimming Operator (revealed, not punished by this command itself — a player
        // response, e.g. SetPropertyManagementCommand replacing them, is the real consequence) keeps
        // their Loyalty untouched here.
        if (!view.OperatorIsSkimming && state.Characters.TryGet(operatorId, out var character))
        {
            state.Characters.Remove(operatorId);
            var condition = character!.Condition;
            var loyalty = Math.Max(0, condition.Loyalty - RealEstateCatalog.FalseAuditAccusationLoyaltyPenalty);
            state.Characters.Add(operatorId, character with
            {
                Condition = new Condition(condition.Health, condition.Fatigue, loyalty, condition.Ambition, condition.Fertility),
            });
        }

        return new IDomainEvent[]
        {
            new PropertyOperatorAuditedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.Subject, operatorId, view.OperatorIsSkimming,
                command.CommandId.ToTaggedString()),
        };
    }
}
