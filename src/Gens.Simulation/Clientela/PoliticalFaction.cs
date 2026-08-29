using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>The light Faction leaning <c>gens-politics-patronage-design.md</c> §3.1 gives every
/// Character relevant to local politics — roughly the historical Optimates (Traditionalist) and
/// Populares (Popularist) tendencies. §3.1 frames this as "the one field this document adds on top of
/// the base Character record"; it is deliberately kept out of <see cref="Character"/> itself (a sparse
/// partition instead, see <see cref="CharacterFactionAlignment"/>) rather than adding a required field
/// to that record's already-large <see cref="Character.Create"/> constructor and every one of its
/// existing call sites (<see cref="Characters.BirthCharacterCommand"/>, <see
/// cref="Characters.PromoteToNamedCommand"/>, <see cref="Actors.LivingWorldActorHeadGenerator"/>, and
/// more) — matching how Phase 11 items kept their own additions (Memoria, Dignitas) as sparse
/// partitions rather than retrofitting the base record.</summary>
public enum PoliticalFaction
{
    Traditionalist,
    Popularist,
}

/// <summary>One Character's <see cref="PoliticalFaction"/> (Phase 12 item 2; §3.1). Sparse: a
/// Character §3.1 never actually needs a Faction for — nearly everyone outside the political cast —
/// simply has no entry, matching <see cref="Reputation.HouseholdReputation"/>'s identical "no entry
/// means the default" convention (here, no Faction at all rather than a numeric zero). §3.1's own
/// household-level Faction ("a slow-moving reflection of accumulated choices... enforcing a Sumptuary
/// Edict pulls Traditionalist, funding popular games pulls Popularist") is deliberately not built here:
/// it depends on Sumptuary Edicts (§8, roadmap item 9's "full edicts") and a drift mechanic this item
/// has no accumulated-choices ledger to drive yet — only the per-Character tag this section's own last
/// line calls out as "the one field this document adds."</summary>
public sealed record CharacterFactionAlignment(RuntimeId<Character> CharacterId, PoliticalFaction Faction);

/// <summary>Resolves a Character's current <see cref="PoliticalFaction"/>, defaulting to <c>null</c>
/// ("unaligned" — not relevant to local politics yet) for a Character with no <see
/// cref="CharacterFactionAlignment"/> entry, matching <see cref="Reputation.DignitasResolver"/>'s
/// identical "no entry means the default" shape.</summary>
public static class CharacterFactionResolver
{
    public static PoliticalFaction? Current(WorldState state, RuntimeId<Character> characterId) =>
        state.CharacterFactionAlignments.TryGet(characterId, out var entry) ? entry!.Faction : null;

    /// <summary>Sets or overwrites a Character's Faction, creating the entry if none exists yet.
    /// Replaces the entry (remove then re-add) rather than mutating in place, matching <see
    /// cref="Reputation.DignitasResolver.Apply"/>'s identical immutable-record-partition convention.</summary>
    public static void Set(WorldState state, RuntimeId<Character> characterId, PoliticalFaction faction)
    {
        if (state.CharacterFactionAlignments.TryGet(characterId, out _))
            state.CharacterFactionAlignments.Remove(characterId);
        state.CharacterFactionAlignments.Add(characterId, new CharacterFactionAlignment(characterId, faction));
    }
}

/// <summary>Assigns (or reassigns) a Character's <see cref="PoliticalFaction"/> (Phase 12 item 2;
/// §3.1). Actor-agnostic like every other command in this codebase (rule 2) — content-driven
/// generation (a promoted Curiales pick a leaning on generation), a player choice for their own
/// household's political cast, and a future Sumptuary-Edict-driven drift would all submit this same
/// command rather than each poking <see cref="CharacterFactionAlignment"/> directly.</summary>
public sealed record SetCharacterFactionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    PoliticalFaction Faction) : ICommand;

/// <summary>Emitted whenever a <see cref="SetCharacterFactionCommand"/> is accepted. <see
/// cref="Visibility"/> is <see cref="Commands.Visibility.Public"/>: a Character's political leaning is,
/// per §3.1, exactly the kind of legible-to-the-political-class fact <see
/// cref="Reputation.DignitasChangedEvent"/>'s own doc comment already argues Dignitas is — nobody hides
/// which side of the Curia they sit on.</summary>
public sealed record CharacterFactionSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    PoliticalFaction Faction,
    string? CausationId) : IDomainEvent
{
    public string Type => "clientela.characterFactionSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SetCharacterFactionCommand"/> (ADR 0006).</summary>
public static class SetCharacterFactionCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("clientela.setFaction.characterNotFound");

    public static readonly CommandPipeline<WorldState, SetCharacterFactionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetCharacterFactionCommand command) =>
        !state.Characters.TryGet(command.CharacterId, out _) ? CharacterNotFound : null;

    private static IDomainEvent[] Mutate(WorldState state, SetCharacterFactionCommand command)
    {
        CharacterFactionResolver.Set(state, command.CharacterId, command.Faction);
        return new IDomainEvent[]
        {
            new CharacterFactionSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.Faction,
                command.CommandId.ToTaggedString()),
        };
    }
}
