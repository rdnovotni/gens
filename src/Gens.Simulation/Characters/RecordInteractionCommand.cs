using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Records one interaction's effect on the directed relationship from <see
/// cref="CharacterId"/> toward <see cref="TargetId"/> (<c>gens-characters-design.md</c> §9's
/// Interaction catalog): an opinion swing, bond tags gained or lost, or both. Creates the <see
/// cref="Relationship"/> record on first contact (lazy, matching the rest of Phase 5's "nothing exists
/// until the moment it's needed" convention) and updates <see
/// cref="Relationship.LastMeaningfulInteractionDate"/> on every acceptance — this command is, by
/// definition, always itself a "meaningful" interaction. This only ever touches the <see
/// cref="CharacterId"/>-to-<see cref="TargetId"/> direction; a caller that wants the interaction
/// reflected both ways (a mutual Friend bond, a shared grudge) submits a second, independent command
/// for the reverse direction — directed asymmetry (Phase 5 item 8) is the model, not an oversight.</summary>
public sealed record RecordInteractionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> TargetId,
    int OpinionDelta,
    BondTag BondsGranted,
    BondTag BondsRevoked,
    RelationshipOrigin Origin) : ICommand;

/// <summary>Emitted whenever a <see cref="RecordInteractionCommand"/> is accepted. Private rather than
/// <see cref="Visibility.Public"/> (unlike most other Character events) — an opinion swing is
/// specifically the kind of interior state real people don't broadcast, so only the two participants
/// are named observers (<c>gens-familia-design.md</c>'s "witnessed" channel).</summary>
public sealed record RelationshipInteractionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character> TargetId,
    int OpinionBefore,
    int OpinionAfter,
    BondTag BondsGranted,
    BondTag BondsRevoked,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.relationshipInteractionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), TargetId.ToTaggedString() };

    public Visibility Visibility => Visibility.Private(CharacterId.ToTaggedString(), TargetId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="RecordInteractionCommand"/> (ADR 0006).</summary>
public static class RecordInteractionCommands
{
    public static readonly ValidationErrorCode SelfInteraction = new("characters.relationship.selfInteraction");
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.relationship.characterNotFound");
    public static readonly ValidationErrorCode TargetNotFound = new("characters.relationship.targetNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.relationship.characterDeceased");
    public static readonly ValidationErrorCode TargetDeceased = new("characters.relationship.targetDeceased");
    public static readonly ValidationErrorCode NothingToRecord = new("characters.relationship.nothingToRecord");
    public static readonly ValidationErrorCode ConflictingBondChange = new("characters.relationship.conflictingBondChange");

    public static readonly CommandPipeline<WorldState, RecordInteractionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordInteractionCommand command)
    {
        if (command.CharacterId == command.TargetId)
            return SelfInteraction;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!state.Characters.TryGet(command.TargetId, out var target))
            return TargetNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (!target.IsAlive)
            return TargetDeceased;
        if (command.OpinionDelta == 0 && command.BondsGranted == BondTag.None && command.BondsRevoked == BondTag.None)
            return NothingToRecord;
        if ((command.BondsGranted & command.BondsRevoked) != BondTag.None)
            return ConflictingBondChange;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordInteractionCommand command)
    {
        var key = new RelationshipKey(command.CharacterId, command.TargetId);
        var exists = state.Relationships.TryGet(key, out var existing);

        var opinionBefore = exists ? existing.Opinion : 0;
        var opinionAfter = Math.Clamp(opinionBefore + command.OpinionDelta, Relationship.MinOpinion, Relationship.MaxOpinion);
        var bonds = ((exists ? existing.Bonds : BondTag.None) | command.BondsGranted) & ~command.BondsRevoked;
        var origin = exists ? existing.Origin : command.Origin;
        var formedDate = exists ? existing.FormedDate : command.SubmittedDate;

        var eventId = state.EventIds.Issue();
        var relationship = new Relationship(
            opinionAfter, bonds, origin, formedDate, command.SubmittedDate, eventId.ToTaggedString());

        if (exists)
            state.Relationships.Remove(key);
        state.Relationships.Add(key, relationship);

        return new IDomainEvent[]
        {
            new RelationshipInteractionRecordedEvent(
                eventId, command.SubmittedDate, command.CharacterId, command.TargetId,
                opinionBefore, opinionAfter, command.BondsGranted, command.BondsRevoked,
                command.CommandId.ToTaggedString()),
        };
    }
}
