using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Languages;

/// <summary>Appoints (or replaces) a household's standing <see cref="InterpresAppointment"/> (§7).
/// Requires the appointee to hold Conversational-or-better proficiency in every language named — a
/// household cannot appoint someone to formally cover a language they don't actually speak well enough
/// to informally serve in the first place (§7's own "any Character who happens to hold
/// Conversational-or-better proficiency... can serve this function informally" is the floor a formal
/// appointment stands on, not a separate, lower bar).</summary>
public sealed record AppointInterpresCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId,
    IReadOnlyList<DefinitionId<LanguageDefinition>> LanguagesCovered) : ICommand;

/// <summary>Emitted whenever an <see cref="AppointInterpresCommand"/> is accepted.</summary>
public sealed record InterpresAppointedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> CharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "languages.interpresAppointed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(HouseholdId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="AppointInterpresCommand"/> (ADR 0006).</summary>
public static class AppointInterpresCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("languages.appointInterpres.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("languages.appointInterpres.characterDeceased");
    public static readonly ValidationErrorCode CharacterNotOfHousehold = new("languages.appointInterpres.characterNotOfHousehold");
    public static readonly ValidationErrorCode NoLanguagesCovered = new("languages.appointInterpres.noLanguagesCovered");
    public static readonly ValidationErrorCode InsufficientProficiency = new("languages.appointInterpres.insufficientProficiency");

    public static readonly CommandPipeline<WorldState, AppointInterpresCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointInterpresCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.Household != command.HouseholdId)
            return CharacterNotOfHousehold;
        if (command.LanguagesCovered is null || command.LanguagesCovered.Count == 0)
            return NoLanguagesCovered;

        foreach (var languageId in command.LanguagesCovered)
        {
            if (!LanguageProficiencyQueries.HasConversationalOrBetter(state, command.CharacterId, languageId))
                return InsufficientProficiency;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointInterpresCommand command)
    {
        if (state.InterpresAppointments.TryGet(command.HouseholdId, out _))
            state.InterpresAppointments.Remove(command.HouseholdId);
        state.InterpresAppointments.Add(
            command.HouseholdId, new InterpresAppointment(command.HouseholdId, command.CharacterId, command.LanguagesCovered));

        return new IDomainEvent[]
        {
            new InterpresAppointedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.CharacterId,
                command.CommandId.ToTaggedString()),
        };
    }
}
