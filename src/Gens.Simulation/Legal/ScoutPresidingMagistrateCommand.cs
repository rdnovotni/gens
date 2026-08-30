using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>
/// Marks a case's presiding magistrate as scouted (Phase 12 item 4; §3: "an Intrigue-driven inquiry or a
/// Legal Scholar's own professional knowledge can reveal a generated NPC magistrate's relevant Axes and
/// Traits before a Hearing ever opens"). This command is deliberately only the flag: the presider's real
/// Axes/Traits are already directly readable off the live <see cref="Characters.Character"/> record the
/// moment <see cref="LegalCase.PresidingCharacterId"/> is known, matching <see cref="Epithets.Agnomen"/>'s
/// own "the flag is the documented hook a future [consumer] reads directly" precedent for a consequence
/// this pass names but leaves the actual reveal-vs-hide presentation gating to whatever future query
/// layer reads it.
/// </summary>
public sealed record ScoutPresidingMagistrateCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<LegalCase> CaseId) : ICommand;

/// <summary>Emitted whenever a <see cref="ScoutPresidingMagistrateCommand"/> is accepted. Public — §3's
/// own "worth knowing" framing is about giving the player real information, not about concealing that an
/// inquiry happened at all.</summary>
public sealed record PresidingMagistrateScoutedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Characters.Character> PresidingCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.presidingMagistrateScouted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PresidingCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ScoutPresidingMagistrateCommand"/> (ADR 0006).</summary>
public static class ScoutPresidingMagistrateCommands
{
    public static readonly ValidationErrorCode UnknownCase = new("legal.scoutPresidingMagistrate.unknownCase");
    public static readonly ValidationErrorCode NoPresiderAssigned = new("legal.scoutPresidingMagistrate.noPresiderAssigned");
    public static readonly ValidationErrorCode AlreadyScouted = new("legal.scoutPresidingMagistrate.alreadyScouted");
    public static readonly ValidationErrorCode CaseAlreadyRuled = new("legal.scoutPresidingMagistrate.caseAlreadyRuled");

    public static readonly CommandPipeline<WorldState, ScoutPresidingMagistrateCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ScoutPresidingMagistrateCommand command)
    {
        if (!state.LegalCases.TryGet(command.CaseId, out var legalCase))
            return UnknownCase;
        if (legalCase!.Stage == LegalCaseStage.Ruled)
            return CaseAlreadyRuled;
        if (legalCase.PresidingCharacterId is null)
            return NoPresiderAssigned;
        if (legalCase.PresidingCharacterScouted)
            return AlreadyScouted;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ScoutPresidingMagistrateCommand command)
    {
        state.LegalCases.TryGet(command.CaseId, out var legalCase);
        state.LegalCases.Remove(command.CaseId);
        state.LegalCases.Add(command.CaseId, legalCase! with { PresidingCharacterScouted = true });

        return new IDomainEvent[]
        {
            new PresidingMagistrateScoutedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CaseId, legalCase.PresidingCharacterId!.Value,
                command.CommandId.ToTaggedString()),
        };
    }
}
