using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>
/// Records one round of evidence-gathering for a side of a Major <see cref="LegalCase"/> (Phase 12 item
/// 4; §5.2, §8: "an Intrigue-driven investigation," "physical evidence traces back to whatever underlying
/// record actually generated the dispute — a DebtRecord, a Slave Market warranty claim, or a punishment
/// record"). This item does not resolve that trace-back into any specific source record itself — no
/// Debt/Slave-Market/Labor-punishment caller submits <see cref="FileLawsuitCommand"/> yet (see <see
/// cref="LegalCaseType"/>'s own doc comment), so <see cref="GatheringCharacterId"/>'s Intrigue is the one
/// real, checkable input this command actually has: a sharper investigator builds a materially stronger
/// case, whatever it eventually turns out to document.
/// </summary>
public sealed record GatherEvidenceCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Character> GatheringCharacterId,
    LegalCaseSide SupportingSide) : ICommand;

/// <summary>Emitted whenever a <see cref="GatherEvidenceCommand"/> is accepted. Public, matching <see
/// cref="TestimonySubmittedEvent"/>'s identical "part of the case's own public record" precedent.</summary>
public sealed record EvidenceGatheredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Character> GatheringCharacterId,
    LegalCaseSide SupportingSide,
    int CaseStrengthGain,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.evidenceGathered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { GatheringCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="GatherEvidenceCommand"/> (ADR 0006).</summary>
public static class GatherEvidenceCommands
{
    public static readonly ValidationErrorCode UnknownCase = new("legal.gatherEvidence.unknownCase");
    public static readonly ValidationErrorCode NotAMajorCase = new("legal.gatherEvidence.notAMajorCase");
    public static readonly ValidationErrorCode NotGatheringEvidence = new("legal.gatherEvidence.notGatheringEvidence");
    public static readonly ValidationErrorCode UnknownGatherer = new("legal.gatherEvidence.unknownGatherer");
    public static readonly ValidationErrorCode GathererDeceased = new("legal.gatherEvidence.gathererDeceased");

    public static readonly CommandPipeline<WorldState, GatherEvidenceCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, GatherEvidenceCommand command)
    {
        if (!state.LegalCases.TryGet(command.CaseId, out var legalCase))
            return UnknownCase;
        if (legalCase!.Depth != LegalCaseDepth.Major)
            return NotAMajorCase;
        if (legalCase.Stage != LegalCaseStage.EvidenceGathering)
            return NotGatheringEvidence;
        if (!state.Characters.TryGet(command.GatheringCharacterId, out var gatherer))
            return UnknownGatherer;
        if (!gatherer!.IsAlive)
            return GathererDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, GatherEvidenceCommand command)
    {
        state.LegalCases.TryGet(command.CaseId, out var legalCase);
        state.Characters.TryGet(command.GatheringCharacterId, out var gatherer);

        var gain = LegalCatalog.EvidenceCaseStrengthGain + (gatherer!.Attributes.Intrigue / 10);

        var updated = command.SupportingSide == LegalCaseSide.Plaintiff
            ? legalCase! with { PlaintiffCaseStrength = legalCase.PlaintiffCaseStrength + gain }
            : legalCase! with { DefendantCaseStrength = legalCase.DefendantCaseStrength + gain };

        state.LegalCases.Remove(command.CaseId);
        state.LegalCases.Add(command.CaseId, updated);

        return new IDomainEvent[]
        {
            new EvidenceGatheredEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CaseId, command.GatheringCharacterId,
                command.SupportingSide, gain, command.CommandId.ToTaggedString()),
        };
    }
}
