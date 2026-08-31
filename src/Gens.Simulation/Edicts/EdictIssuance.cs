using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Edicts;

/// <summary>Shared plumbing every real Edict command in this item uses: §5.1's "every Edict costs real
/// Influence and Dignitas to issue" charge, and its own Reception routed through Phase 12 item 7's
/// Scandal engine (see <see cref="ScandalSourceType.EdictBacklash"/>'s own doc comment). Not itself a
/// <see cref="CommandPipeline{TState,TCommand}"/> — each Edict type's own command is rule 2's "one
/// command path" for its own kind of mutation; this only factors out the two steps every one of them
/// shares, matching <see cref="Legal.LegalCaseRuling"/>'s own "shared verdict-consequence logic, called
/// from more than one command" precedent.</summary>
internal static class EdictIssuance
{
    public static readonly ValidationErrorCode InsufficientInfluence = new("edicts.issue.insufficientInfluence");

    public static ValidationErrorCode? ValidateAffordability(WorldState state, RuntimeId<Household> householdId, int influenceCost) =>
        InfluenceResolver.Current(state, householdId) < influenceCost ? InsufficientInfluence : null;

    /// <summary>Charges §5.1's Influence and Dignitas issuance costs, routing the Dignitas half through
    /// <see cref="AdjustDignitasCommand"/> (the one command path every Dignitas-moving system uses) and
    /// applying the Influence half directly, matching <see cref="Magistracies.SalutatioSystem"/>'s own
    /// "no shared Influence-moving command exists yet" precedent.</summary>
    public static List<IDomainEvent> ChargeCosts(
        WorldState state, RuntimeId<Command> commandId, string actorId, GameDate date,
        RuntimeId<Household> householdId, int influenceCost, int dignitasCost, string reason)
    {
        InfluenceResolver.Apply(state, householdId, -influenceCost);

        var events = new List<IDomainEvent>();
        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), actorId, date, commandId.ToTaggedString(), householdId, -dignitasCost, reason)).Events);
        return events;
    }

    /// <summary>Records §5.1's Reception as a real <see cref="ScandalRecord"/> — "a genuine backlash
    /// chain reading Faction... and severity," reusing <see cref="RecordScandalCommand"/>'s own
    /// already-built Faction-dependent reception exactly rather than a second, parallel model.</summary>
    public static (RuntimeId<ScandalRecord> ScandalId, List<IDomainEvent> Events) RecordReception(
        WorldState state, RuntimeId<Command> commandId, string actorId, GameDate date,
        RuntimeId<Household> householdId, ScandalSeverity severity)
    {
        var result = RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), actorId, date, commandId.ToTaggedString(), householdId,
                ScandalSourceType.EdictBacklash, severity));

        var scandalId = result.Events.OfType<ScandalRecordedEvent>().Single().ScandalId;
        return (scandalId, result.Events.ToList());
    }
}
