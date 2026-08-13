using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// Roadmap Phase 5 item 7 / ADR 0009's promotion command: converts one unit of an aggregate <see
/// cref="PopGroup"/> entry into a full <see cref="Character"/> record. This is the sole path from
/// <see cref="FidelityTier.Background"/> to <see cref="FidelityTier.Named"/> — nothing else in the
/// codebase mutates a <see cref="PopGroup"/>'s <see cref="PopGroup.Size"/>, matching ADR 0006's "one
/// command path" rule and keeping population conservation (§5) a mechanical property of this single
/// command rather than something every future caller has to remember separately.
/// </summary>
public sealed record PromoteToNamedCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    PopGroupType GroupType,
    CharacterSource Source,
    Sex? Sex,
    LegalStatus Status,
    SocialClass? SocialClass,
    DefinitionId<Culture> Culture,
    NamePool NamePool,
    string RandomStreamName,
    RuntimeId<Household>? Household = null) : ICommand;

/// <summary>Emitted whenever a <see cref="PromoteToNamedCommand"/> is accepted. <see
/// cref="SubjectIds"/> is the newly-named Character and the settlement its source <see
/// cref="PopGroup"/> was drawn from.</summary>
public sealed record CharacterPromotedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Settlement> SettlementId,
    PopGroupType GroupType,
    CharacterSource Source,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.promoted";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), SettlementId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="PromoteToNamedCommand"/> (ADR 0006). Exposed
/// via a factory rather than a pre-built static pipeline for the same reason as <see
/// cref="BirthCharacterCommands.CreatePipeline"/>: <c>mutate</c> draws from a <see
/// cref="RandomStreamSet"/> (age, identity, and attribute/skill/condition backfill), and <see
/// cref="CommandPipeline{TState,TCommand}"/>'s <c>mutate</c> delegate only receives
/// <c>WorldState</c>.</summary>
public static class PromoteToNamedCommands
{
    public static readonly ValidationErrorCode PopGroupNotFound = new("characters.promote.popGroupNotFound");
    public static readonly ValidationErrorCode PopGroupEmpty = new("characters.promote.popGroupEmpty");
    public static readonly ValidationErrorCode InvalidSource = new("characters.promote.invalidSource");

    public static CommandPipeline<WorldState, PromoteToNamedCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, PromoteToNamedCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, PromoteToNamedCommand command)
    {
        // Familia means "born inside the simulation" (CharacterSource's own doc comment) — never a
        // valid promotion trigger, since promotion always has a source PopGroup, not a mother.
        if (command.Source == CharacterSource.Familia)
            return InvalidSource;

        var key = new PopGroupKey(command.SettlementId, command.GroupType);
        if (!state.PopGroups.TryGet(key, out var popGroup))
            return PopGroupNotFound;
        if (popGroup.Size <= 0)
            return PopGroupEmpty;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PromoteToNamedCommand command, RandomStreamSet randomStreams)
    {
        var key = new PopGroupKey(command.SettlementId, command.GroupType);
        state.PopGroups.TryGet(key, out var popGroup);
        state.PopGroups.Remove(key);
        state.PopGroups.Add(key, popGroup with { Size = popGroup.Size - 1 });

        var sex = command.Sex ?? (randomStreams.NextUInt(command.RandomStreamName, 2) == 0 ? Sex.Male : Sex.Female);
        var birthDate = CharacterBackfillGenerator.RollAdultBirthDate(randomStreams, command.RandomStreamName, command.SubmittedDate);
        var identity = CharacterIdentityGenerator.Generate(
            randomStreams, command.RandomStreamName, sex, command.Status, command.NamePool);
        var (attributes, skills) = CharacterBackfillGenerator.RollAttributesAndSkills(randomStreams, command.RandomStreamName);
        var condition = CharacterBackfillGenerator.RollCondition(randomStreams, command.RandomStreamName);

        var characterId = state.CharacterIds.Issue();
        var character = Character.Create(
            id: characterId,
            praenomen: identity.Name.Praenomen,
            nomen: identity.Name.Nomen,
            cognomen: identity.Name.Cognomen,
            sex: sex,
            birthDate: birthDate,
            visualProfile: identity.Visual,
            status: command.Status,
            socialClass: command.SocialClass,
            culture: command.Culture,
            location: command.SettlementId,
            household: command.Household,
            attributes: attributes,
            skills: skills,
            condition: condition,
            source: command.Source,
            instantiatedAtMonth: command.SubmittedDate.TotalMonths,
            // §11: a lazily-instantiated adult "obviously can't start with zero Reactive traits" —
            // this record's history was generated, not lived through the simulation.
            backfilledHistory: true);

        state.Characters.Add(characterId, character);

        return new IDomainEvent[]
        {
            new CharacterPromotedEvent(
                state.EventIds.Issue(), command.SubmittedDate, characterId, command.SettlementId,
                command.GroupType, command.Source, command.CommandId.ToTaggedString()),
        };
    }
}
