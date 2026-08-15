using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Plans a Testamento manumission grant (<c>gens-labor-slavery-design.md</c> §8: "granted by
/// will ... doesn't take effect until the pater/materfamilias dies"). <see cref="LaborFlightSystem"/>
/// watches every planned grant and resolves it into a full manumission once <see cref="GrantorId"/>'s
/// <see cref="DeathRecord"/> becomes non-null.</summary>
public sealed record PlanTestamentoManumissionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> GrantorId) : ICommand;

/// <summary>Emitted whenever a <see cref="PlanTestamentoManumissionCommand"/> is accepted.</summary>
public sealed record TestamentoManumissionPlannedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> GrantorId,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.testamentoManumissionPlanned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), GrantorId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PlanTestamentoManumissionCommand"/> (ADR 0006).</summary>
public static class PlanTestamentoManumissionCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.planTestamento.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.planTestamento.characterDeceased");
    public static readonly ValidationErrorCode NotEnslaved = new("characters.planTestamento.notEnslaved");
    public static readonly ValidationErrorCode AlreadyPlanned = new("characters.planTestamento.alreadyPlanned");
    public static readonly ValidationErrorCode GrantorNotFound = new("characters.planTestamento.grantorNotFound");
    public static readonly ValidationErrorCode GrantorDeceased = new("characters.planTestamento.grantorDeceased");

    public static readonly CommandPipeline<WorldState, PlanTestamentoManumissionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PlanTestamentoManumissionCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.LegalStatus != LegalStatus.Enslaved)
            return NotEnslaved;
        if (character.ManumissionPlan is not null)
            return AlreadyPlanned;
        if (!state.Characters.TryGet(command.GrantorId, out var grantor))
            return GrantorNotFound;
        if (!grantor.IsAlive)
            return GrantorDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PlanTestamentoManumissionCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(
            command.CharacterId,
            character with { ManumissionPlan = new ManumissionPlan(command.GrantorId, ManumissionType.Testamento) });

        return new IDomainEvent[]
        {
            new TestamentoManumissionPlannedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.GrantorId,
                command.CommandId.ToTaggedString()),
        };
    }
}
