using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Languages;

/// <summary>Records a fact about a named Character's own Language Proficiency (§5, §10) — grants a new
/// entry, or updates an existing one's tier/method, but never simulates the growth curve getting there;
/// per this item's own scope discipline, whatever caller decides a Character has reached this tier
/// through this method (native-origin assignment at creation, an Education &amp; Culture Learning
/// milestone, a Distant Holding tick, a Wanderer's own instruction) does that deciding — this command
/// only records the resulting fact.</summary>
public sealed record AcquireLanguageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    DefinitionId<LanguageDefinition> LanguageId,
    FluencyTier FluencyTier,
    LanguageAcquisitionMethod AcquisitionMethod) : ICommand;

/// <summary>Emitted whenever an <see cref="AcquireLanguageCommand"/> is accepted.</summary>
public sealed record LanguageProficiencyChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    DefinitionId<LanguageDefinition> LanguageId,
    FluencyTier FluencyTier,
    string? CausationId) : IDomainEvent
{
    public string Type => "languages.proficiencyChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="AcquireLanguageCommand"/> (ADR 0006). Built
/// against a <see cref="LanguageCatalog"/>, matching <see
/// cref="Travel.BeginTravelCommands.BuildPipeline"/>'s identical "caller-loaded content" shape.</summary>
public static class AcquireLanguageCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("languages.acquire.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("languages.acquire.characterDeceased");
    public static readonly ValidationErrorCode UnknownLanguage = new("languages.acquire.unknownLanguage");
    public static readonly ValidationErrorCode NativeOriginRequiresNativeLanguage =
        new("languages.acquire.nativeOriginRequiresNativeLanguage");

    public static CommandPipeline<WorldState, AcquireLanguageCommand> BuildPipeline(
        LanguageCatalog languages, CultureLanguageMap cultureLanguages)
    {
        if (languages is null)
            throw new ArgumentNullException(nameof(languages));
        if (cultureLanguages is null)
            throw new ArgumentNullException(nameof(cultureLanguages));

        return new CommandPipeline<WorldState, AcquireLanguageCommand>(
            validate: (state, command) => Validate(state, command, languages, cultureLanguages),
            mutate: Mutate,
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(
        WorldState state, AcquireLanguageCommand command, LanguageCatalog languages, CultureLanguageMap cultureLanguages)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (!languages.TryGetLanguage(command.LanguageId, out _))
            return UnknownLanguage;

        // §5's "native acquisition" is specifically the origin culture's own mapped language — this
        // guards against silently mislabeling a learned second language as if it were native.
        if (command.AcquisitionMethod == LanguageAcquisitionMethod.NativeOrigin &&
            cultureLanguages.Resolve(character.Culture) != command.LanguageId)
        {
            return NativeOriginRequiresNativeLanguage;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AcquireLanguageCommand command)
    {
        if (LanguageProficiencyQueries.TryGet(state, command.CharacterId, command.LanguageId, out var existing))
            state.LanguageProficiencies.Remove(existing.Id);

        var id = state.LanguageProficiencyIds.Issue();
        state.LanguageProficiencies.Add(
            id, new LanguageProficiency(id, command.CharacterId, command.LanguageId, command.FluencyTier, command.AcquisitionMethod));

        return new IDomainEvent[]
        {
            new LanguageProficiencyChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.LanguageId,
                command.FluencyTier, command.CommandId.ToTaggedString()),
        };
    }
}
