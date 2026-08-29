using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// The Aedile's "occasional real duty" (Phase 12 item 2; §5.2): "periodically, holding the Aedileship
/// prompts the player to actually fund a specific game or public work, with a real choice... and a real
/// consequence." This command is that choice, submittable whenever the game/UI layer decides the
/// prompt has come up (this item does not itself invent a cadence for how often the prompt fires — §5.2
/// names the duty as occasional, not scheduled, and no numeric cadence is given anywhere in the design
/// corpus).
///
/// <b>Scope note:</b> §5.2 pairs the consequence with "a Dignitas/Contentment boost if funded well" —
/// only the Dignitas half is wired here. Moving Settlement Demographics' Contentment needs that
/// system's own household/settlement-scoped write path, which this item's scope (§4/§5 of the Politics
/// &amp; Patronage doc only) doesn't reach into; §12's own numeric sizing is an Open Question regardless,
/// so the amounts in <see cref="MagistracyCatalog"/> are this implementation's own placeholder choice.
/// </summary>
public sealed record FundAedileWorksCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> AedileCharacterId,
    RuntimeId<Settlement> SettlementId,
    AedileFundingChoice Choice) : ICommand;

/// <summary>Emitted whenever a <see cref="FundAedileWorksCommand"/> is accepted. Public — the games or
/// works an Aedile funds (or skips) are, by the office's own civic nature, a public act.</summary>
public sealed record AedileWorksFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> AedileCharacterId,
    RuntimeId<Settlement> SettlementId,
    AedileFundingChoice Choice,
    int DignitasDelta,
    string? CausationId) : IDomainEvent
{
    public string Type => "magistracies.aedileWorksFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AedileCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundAedileWorksCommand"/> (ADR 0006).</summary>
public static class FundAedileWorksCommands
{
    public static readonly ValidationErrorCode NotAnActiveAedile = new("magistracies.fundAedileWorks.notAnActiveAedile");
    public static readonly ValidationErrorCode NoHousehold = new("magistracies.fundAedileWorks.noHousehold");

    public static readonly CommandPipeline<WorldState, FundAedileWorksCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundAedileWorksCommand command)
    {
        if (MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Aedile, command.AedileCharacterId) is null)
            return NotAnActiveAedile;
        if (!state.Characters.TryGet(command.AedileCharacterId, out var aedile) || aedile!.Household is null)
            return NoHousehold;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundAedileWorksCommand command)
    {
        state.Characters.TryGet(command.AedileCharacterId, out var aedile);
        var householdId = aedile!.Household!.Value;

        var dignitasDelta = command.Choice switch
        {
            AedileFundingChoice.FundGenerously => MagistracyCatalog.AedileFundGenerouslyDignitasGain,
            AedileFundingChoice.FundMinimally => MagistracyCatalog.AedileFundMinimallyDignitasGain,
            _ => -MagistracyCatalog.AedileLetItPassDignitasCost,
        };

        var reason = command.Choice switch
        {
            AedileFundingChoice.FundGenerously => "funded the games/works generously as Aedile",
            AedileFundingChoice.FundMinimally => "funded the games/works minimally as Aedile",
            _ => "let the Aedile's funding moment pass",
        };

        var dignitasCommand = new AdjustDignitasCommand(
            state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(), householdId,
            dignitasDelta, reason);
        var dignitasResult = AdjustDignitasCommands.Pipeline.Execute(state, dignitasCommand);

        var events = new List<IDomainEvent>(dignitasResult.Events)
        {
            new AedileWorksFundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.AedileCharacterId, command.SettlementId, command.Choice,
                dignitasDelta, command.CommandId.ToTaggedString()),
        };
        return events.ToArray();
    }
}
