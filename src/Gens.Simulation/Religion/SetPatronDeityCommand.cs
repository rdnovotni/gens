using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// The founding Patron Deity pick (Phase 12 item 3; <c>gens-religion-design.md</c> §2.1: "at founding
/// ... the household selects a Patron Deity"). The one-time entry point into this domain's <see
/// cref="HouseholdReligion"/> partition — every other command in this domain requires one to already
/// exist (see <see cref="AdjustFavorCommand"/>, <see cref="RaiseOmenCommand"/>, and so on). Changing an
/// already-chosen Patron Deity is deliberately a different command, <see cref="ReconsecrateCommand"/>,
/// not a second call to this one: §2.1 frames the two as narratively distinct events (a first choice at
/// founding versus a rare, story-gated later change), and splitting them lets each command carry its
/// own real validation (this one rejects a second call outright; Reconsecration gates on a genuine
/// headship change and spends a real ceremony cost) rather than one command branching internally on
/// "is this the first time."
/// </summary>
public sealed record SetPatronDeityCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    PatronDeity Deity,
    RuntimeId<Character> HeadCharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="SetPatronDeityCommand"/> is accepted. <see cref="Visibility"/>
/// is <see cref="Commands.Visibility.Public"/>, matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s own <c>DignitasChangedEvent</c> reasoning — §9 names
/// "a rival gens carries its own Patron Deity and Favor standing exactly as the player's household
/// does, available as a point of contrast" as real cross-house knowledge, so a household's chosen deity
/// is not a private fact.</summary>
public sealed record PatronDeitySetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    PatronDeity Deity,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.patronDeitySet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SetPatronDeityCommand"/> (ADR 0006).</summary>
public static class SetPatronDeityCommands
{
    public static readonly ValidationErrorCode AlreadyChosen = new("religion.setPatronDeity.alreadyChosen");
    public static readonly ValidationErrorCode HeadCharacterNotFound = new("religion.setPatronDeity.headCharacterNotFound");
    public static readonly ValidationErrorCode HeadCharacterDeceased = new("religion.setPatronDeity.headCharacterDeceased");

    public static readonly CommandPipeline<WorldState, SetPatronDeityCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetPatronDeityCommand command)
    {
        if (HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return AlreadyChosen;
        if (!state.Characters.TryGet(command.HeadCharacterId, out var head))
            return HeadCharacterNotFound;
        if (!head!.IsAlive)
            return HeadCharacterDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetPatronDeityCommand command)
    {
        state.HouseholdReligions.Add(
            command.HouseholdId,
            new HouseholdReligion(command.HouseholdId, command.Deity, Favor: 0, command.HeadCharacterId));

        return new IDomainEvent[]
        {
            new PatronDeitySetEvent(state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.Deity, command.CommandId.ToTaggedString()),
        };
    }
}
