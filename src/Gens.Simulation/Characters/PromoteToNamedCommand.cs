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
    public static readonly ValidationErrorCode SourceCohortMismatch = new("characters.promote.sourceCohortMismatch");

    /// <summary>The specific source-to-cohort ties <c>gens-settlement-demographics-design.md</c> §11
    /// names explicitly — a marriage proposal only ever targets a named Curiales individual, and a
    /// Slave Market purchase only ever draws from the Non-Household Enslaved cohort. Every other
    /// <see cref="CharacterSource"/> promotion trigger (a Labor Duty Slot/Overseer hire, a Court
    /// Position, a Curia seat, a Travel/Events encounter, a Guest, a rival-generated Character) isn't
    /// tied to one specific <see cref="PopGroupType"/> anywhere in the design corpus, so those are left
    /// unrestricted rather than guessing a cohort the documents never specify.</summary>
    private static readonly Dictionary<CharacterSource, PopGroupType> RequiredCohortBySource =
        new()
        {
            [CharacterSource.MarriageProposal] = PopGroupType.Curiales,
            [CharacterSource.SlaveMarketPurchase] = PopGroupType.NonHouseholdEnslaved,
        };

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

        // A marriage proposal always draws from Curiales and a Slave Market purchase always draws
        // from the Non-Household Enslaved cohort (§11) — any other cohort for these two sources would
        // decrement the wrong pop group and record misleading provenance on the resulting Character.
        if (RequiredCohortBySource.TryGetValue(command.Source, out var requiredCohort) && command.GroupType != requiredCohort)
            return SourceCohortMismatch;

        var key = new PopGroupKey(command.SettlementId, command.GroupType);
        if (!state.PopGroups.TryGet(key, out var popGroup))
            return PopGroupNotFound;
        if (popGroup.Size <= 0)
            return PopGroupEmpty;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PromoteToNamedCommand command, RandomStreamSet randomStreams)
    {
        // Every fallible step — generation and Character.Create's own cross-field validation — runs
        // before this method touches PopGroups or Characters. CommandPipeline has no rollback: if
        // Character.Create threw after the PopGroup had already been decremented, the campaign would
        // lose one unit of population with no corresponding Character or event to show for it,
        // breaking the exact conservation invariant this command exists to guarantee (ADR 0009).
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
            // §11: a lazily-instantiated adult's history is generated, not lived through the
            // simulation. Trait backfill itself isn't implemented yet — no system in this codebase
            // rolls traits onto a newly-generated Character yet (BirthCharacterCommand leaves them
            // empty too); Traits defaults to empty here for that same current-scope reason.
            backfilledHistory: true);

        // Only now that construction has fully succeeded do we commit the conserved population change.
        var key = new PopGroupKey(command.SettlementId, command.GroupType);
        state.PopGroups.TryGet(key, out var popGroup);
        state.PopGroups.Remove(key);
        state.PopGroups.Add(key, popGroup with { Size = popGroup.Size - 1 });

        state.Characters.Add(characterId, character);

        return new IDomainEvent[]
        {
            new CharacterPromotedEvent(
                state.EventIds.Issue(), command.SubmittedDate, characterId, command.SettlementId,
                command.GroupType, command.Source, command.CommandId.ToTaggedString()),
        };
    }
}
