using Gens.Simulation.Commands;
using Gens.Simulation.Epithets;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// Births a new Character to a recorded mother (<c>gens-familia-design.md</c> §6). <see
/// cref="Sex"/> is optional — leave <c>null</c> to have it rolled 50/50 on <see
/// cref="RandomStreamName"/>, the same stream identity/condition generation draws from. The newborn
/// inherits its mother's <see cref="Character.Culture"/>, <see cref="Character.Location"/>, and gens
/// name (<see cref="Character.Nomen"/>) — a deliberate implementation choice standing in for a
/// not-yet-designed inheritance rule, since nothing in the design corpus specifies how these should be
/// determined for a newborn.
/// </summary>
public sealed record BirthCharacterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> MotherId,
    RuntimeId<Character>? FatherId,
    Sex? Sex,
    LegalStatus Status,
    SocialClass? SocialClass,
    NamePool NamePool,
    string RandomStreamName,
    RuntimeId<Household>? Household = null) : ICommand;

/// <summary>Emitted whenever a <see cref="BirthCharacterCommand"/> is accepted. <see
/// cref="SubjectIds"/> is the newborn, the mother, and — when recorded — the father.</summary>
public sealed record CharacterBornEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> MotherId,
    RuntimeId<Character>? FatherId,
    Legitimacy Legitimacy,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.born";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => FatherId is { } father
        ? new[] { CharacterId.ToTaggedString(), MotherId.ToTaggedString(), father.ToTaggedString() }
        : new[] { CharacterId.ToTaggedString(), MotherId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="BirthCharacterCommand"/> (ADR 0006). Exposed
/// via a factory rather than a pre-built static pipeline: unlike every other Character command here,
/// birth draws from a <see cref="RandomStreamSet"/> during <c>mutate</c> (name, appearance, and
/// baseline Condition generation) — <see cref="CommandPipeline{TState,TCommand}"/>'s <c>mutate</c>
/// delegate only receives <c>WorldState</c>, so the stream set is captured by this closure instead.</summary>
public static class BirthCharacterCommands
{
    public static readonly ValidationErrorCode MotherNotFound = new("characters.birth.motherNotFound");
    public static readonly ValidationErrorCode MotherDeceased = new("characters.birth.motherDeceased");
    public static readonly ValidationErrorCode MotherNotOfAge = new("characters.birth.motherNotOfAge");
    public static readonly ValidationErrorCode FatherNotFound = new("characters.birth.fatherNotFound");
    public static readonly ValidationErrorCode FatherDeceased = new("characters.birth.fatherDeceased");
    public static readonly ValidationErrorCode MotherIsFather = new("characters.birth.motherIsFather");

    public static CommandPipeline<WorldState, BirthCharacterCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, BirthCharacterCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, BirthCharacterCommand command)
    {
        if (!state.Characters.TryGet(command.MotherId, out var mother))
            return MotherNotFound;
        if (!mother.IsAlive)
            return MotherDeceased;

        var motherStage = mother.GetLifecycleStage(command.SubmittedDate);
        if (motherStage is LifecycleStage.Infant or LifecycleStage.Child)
            return MotherNotOfAge;

        if (command.FatherId is { } fatherId)
        {
            if (fatherId == command.MotherId)
                return MotherIsFather;
            if (!state.Characters.TryGet(fatherId, out var father))
                return FatherNotFound;
            if (!father.IsAlive)
                return FatherDeceased;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BirthCharacterCommand command, RandomStreamSet randomStreams)
    {
        state.Characters.TryGet(command.MotherId, out var mother);

        var sex = command.Sex ?? (randomStreams.NextUInt(command.RandomStreamName, 2) == 0 ? Sex.Male : Sex.Female);
        var identity = CharacterIdentityGenerator.Generate(
            randomStreams, command.RandomStreamName, sex, command.Status, command.NamePool, fixedNomen: mother.Nomen);
        var condition = CharacterConditionGenerator.Generate(randomStreams, command.RandomStreamName);

        // Legitimate exactly when the mother's currently-open marriage is to the recorded father
        // (gens-familia-design.md §5.2); both null (no father recorded, no open marriage) also
        // compares equal and reads as legitimate by the same "no marriage question" convention as a
        // parentless founder (see Character.Legitimacy's own doc comment).
        var legitimacy = mother.CurrentSpouseId == command.FatherId ? Legitimacy.Legitimate : Legitimacy.Illegitimate;
        var resolvedHousehold = command.Household ?? mother.Household;

        // §5's own "changes how every subsequent generation is actually named": a household that
        // formally adopted an earned Agnomen as its standing cognomen (AdoptAgnomenAsCognomenCommand)
        // overrides this newborn's freshly generated cognomen with that adopted name, rather than every
        // birth rolling an unrelated one regardless of the family's own real decision.
        var cognomen = resolvedHousehold is { } householdForCognomen
            ? InheritedCognomenResolver.CurrentCognomen(state, householdForCognomen) ?? identity.Name.Cognomen
            : identity.Name.Cognomen;

        var childId = state.CharacterIds.Issue();
        var child = Character.Create(
            id: childId,
            praenomen: identity.Name.Praenomen,
            nomen: identity.Name.Nomen,
            cognomen: cognomen,
            sex: sex,
            birthDate: command.SubmittedDate,
            visualProfile: identity.Visual,
            status: command.Status,
            socialClass: command.SocialClass,
            culture: mother.Culture,
            location: mother.Location,
            household: resolvedHousehold,
            // An infant has "no stats beyond Health" (gens-familia-design.md §3's Infant row); Core
            // Attributes and Labor Skills start at zero as a placeholder until a real generation
            // system for them exists — not itself specified anywhere in the design corpus.
            attributes: new CoreAttributes(0, 0, 0, 0, 0),
            skills: new LaborSkills(0, 0, 0, 0, 0),
            condition: condition,
            source: CharacterSource.Familia,
            instantiatedAtMonth: command.SubmittedDate.TotalMonths,
            motherId: command.MotherId,
            fatherId: command.FatherId,
            legitimacy: legitimacy);

        state.Characters.Add(childId, child);

        return new IDomainEvent[]
        {
            new CharacterBornEvent(
                state.EventIds.Issue(), command.SubmittedDate, childId, command.MotherId, command.FatherId,
                legitimacy, command.CommandId.ToTaggedString()),
        };
    }
}
