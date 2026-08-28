using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Funerary;

/// <summary>Records that a household has visibly broken its own <see cref="MourningPeriod"/> early
/// (§4.1's "dancing on the grave" example — attending a rival's banquet, opening a betrothal
/// negotiation, while still within <see cref="MourningPeriod.EndDate"/>). This command only sets <see
/// cref="MourningPeriod.BrokenEarly"/>; the real consequence the design doc names — a Scandal (Scandal
/// §4, Phase 12, not yet built) — has nothing to fire into yet, so this is deliberately the entire
/// mechanical effect for now, matching <see cref="Characters.PunishCommand"/>'s own precedent for
/// deferring a still-missing downstream consequence rather than inventing a stand-in for it.</summary>
public sealed record BreakMourningEarlyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>Emitted whenever a <see cref="BreakMourningEarlyCommand"/> is accepted.</summary>
public sealed record MourningBrokenEarlyEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> TriggeringDeathCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "funerary.mourningBrokenEarly";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), TriggeringDeathCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="BreakMourningEarlyCommand"/> (ADR 0006).</summary>
public static class BreakMourningEarlyCommands
{
    public static readonly ValidationErrorCode NoActiveMourningPeriod = new("funerary.breakMourningEarly.noActiveMourningPeriod");
    public static readonly ValidationErrorCode AlreadyBroken = new("funerary.breakMourningEarly.alreadyBroken");

    public static readonly CommandPipeline<WorldState, BreakMourningEarlyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, BreakMourningEarlyCommand command)
    {
        if (!state.MourningPeriods.TryGet(command.HouseholdId, out var period) || !period!.IsActiveOn(command.SubmittedDate))
            return NoActiveMourningPeriod;
        if (period.BrokenEarly)
            return AlreadyBroken;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, BreakMourningEarlyCommand command)
    {
        state.MourningPeriods.TryGet(command.HouseholdId, out var period);
        state.MourningPeriods.Remove(command.HouseholdId);
        state.MourningPeriods.Add(command.HouseholdId, period! with { BrokenEarly = true });

        return new IDomainEvent[]
        {
            new MourningBrokenEarlyEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, period.TriggeringDeathCharacterId,
                command.CommandId.ToTaggedString()),
        };
    }
}
