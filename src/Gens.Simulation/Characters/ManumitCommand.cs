using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Immediately manumits a living enslaved Character via Vindicta or Censu
/// (<c>gens-labor-slavery-design.md</c> §8) — Testamento is not immediate and goes through <see
/// cref="PlanTestamentoManumissionCommand"/> instead. Census-window gating (§8: "only available during
/// an active census Event") is deferred — no Events system exists yet to gate against, so <see
/// cref="ManumissionType.Censu"/> behaves identically to <see cref="ManumissionType.Vindicta"/> here
/// beyond the recorded method.</summary>
public sealed record ManumitCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> PatronusId,
    ManumissionType Method) : ICommand;

/// <summary>Emitted whenever a <see cref="ManumitCommand"/> is accepted.</summary>
public sealed record CharacterManumittedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> PatronusId,
    ManumissionType Method,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.manumitted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), PatronusId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ManumitCommand"/> (ADR 0006).</summary>
public static class ManumitCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.manumit.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.manumit.characterDeceased");
    public static readonly ValidationErrorCode NotEnslaved = new("characters.manumit.notEnslaved");
    public static readonly ValidationErrorCode PatronusNotFound = new("characters.manumit.patronusNotFound");
    public static readonly ValidationErrorCode PatronusDeceased = new("characters.manumit.patronusDeceased");
    public static readonly ValidationErrorCode InvalidMethod = new("characters.manumit.invalidMethod");

    public static readonly CommandPipeline<WorldState, ManumitCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ManumitCommand command)
    {
        if (command.Method == ManumissionType.Testamento)
            return InvalidMethod;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.LegalStatus != LegalStatus.Enslaved)
            return NotEnslaved;
        if (!state.Characters.TryGet(command.PatronusId, out var patronus))
            return PatronusNotFound;
        if (!patronus.IsAlive)
            return PatronusDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ManumitCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.TryGet(command.PatronusId, out var patronus);

        // Labor continuity (§8): Duty is deliberately left untouched — manumission doesn't end
        // employment by default, only legal status.
        var freed = character with
        {
            LegalStatus = LegalStatus.Freedman,
            Nomen = patronus.Nomen,
            Regimen = null,
            Flight = null,
            Pursuit = null,
            ManumissionPlan = null,
        };

        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(command.CharacterId, freed);

        return new IDomainEvent[]
        {
            new CharacterManumittedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.PatronusId,
                command.Method, command.CommandId.ToTaggedString()),
        };
    }
}
