using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Sets a household's group-level Regimen default (<c>gens-labor-slavery-design.md</c> §5:
/// "all Field Hands," "all Household Slaves"), either for one <see cref="DutySlot"/> or, when <see
/// cref="Slot"/> is <c>null</c>, for the whole household. No household-existence check — matching <see
/// cref="AssignDutyCommand"/>'s own precedent of never validating <c>RuntimeId&lt;Household&gt;</c> as
/// its own entity, since no <c>Household</c> aggregate record exists yet.</summary>
public sealed record SetGroupRegimenCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    DutySlot? Slot,
    RegimenSettings Regimen) : ICommand;

/// <summary>Emitted whenever a <see cref="SetGroupRegimenCommand"/> is accepted.</summary>
public sealed record GroupRegimenSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    DutySlot? Slot,
    RegimenSettings Regimen,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.groupRegimenSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SetGroupRegimenCommand"/> (ADR 0006). Always
/// accepted — there is nothing on the household or slot to validate against.</summary>
public static class SetGroupRegimenCommands
{
    public static readonly CommandPipeline<WorldState, SetGroupRegimenCommand> Pipeline = new(
        validate: static (_, _) => null,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static IDomainEvent[] Mutate(WorldState state, SetGroupRegimenCommand command)
    {
        var key = new HouseholdRegimenKey(command.HouseholdId, command.Slot);
        state.HouseholdRegimenDefaults.Remove(key);
        state.HouseholdRegimenDefaults.Add(key, command.Regimen);

        return new IDomainEvent[]
        {
            new GroupRegimenSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.Slot, command.Regimen,
                command.CommandId.ToTaggedString()),
        };
    }
}
