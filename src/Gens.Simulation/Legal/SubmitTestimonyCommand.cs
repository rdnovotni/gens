using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>
/// Records one witness's testimony for a side of a Major <see cref="LegalCase"/> (Phase 12 item 4; §5.2,
/// §8: "Characters' Request Testimony (§9.7)... no new interaction invented" for the underlying social
/// act). This command is this domain's own consumer of that testimony once given — the actual "ask a
/// Character to testify" social interaction is Characters §9.7's own territory (not built anywhere in
/// this codebase per that section's own survey), so this command takes an already-willing <see
/// cref="WitnessCharacterId"/> the same way <see cref="Magistracies.HoldContestedElectionCommand"/> takes
/// an already-resolved challenger rather than generating one inline. A Legal Scholar witness (§8: "a Legal
/// Scholar Trait gives real argument-construction weight beyond raw Learning") adds real extra weight —
/// see <see cref="LegalCatalog.LegalScholarCaseStrengthBonus"/>.
/// </summary>
public sealed record SubmitTestimonyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Character> WitnessCharacterId,
    LegalCaseSide SupportingSide) : ICommand;

/// <summary>Emitted whenever a <see cref="SubmitTestimonyCommand"/> is accepted. Public — testimony given
/// toward a formal case is part of that case's own public record, unlike a bribe's deliberately concealed
/// nature (see <see cref="BribeOfferedEvent"/>'s contrasting <see cref="Visibility"/>).</summary>
public sealed record TestimonySubmittedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Character> WitnessCharacterId,
    LegalCaseSide SupportingSide,
    int CaseStrengthGain,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.testimonySubmitted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WitnessCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="SubmitTestimonyCommand"/> (ADR 0006).</summary>
public static class SubmitTestimonyCommands
{
    public static readonly ValidationErrorCode UnknownCase = new("legal.submitTestimony.unknownCase");
    public static readonly ValidationErrorCode NotAMajorCase = new("legal.submitTestimony.notAMajorCase");
    public static readonly ValidationErrorCode NotGatheringEvidence = new("legal.submitTestimony.notGatheringEvidence");
    public static readonly ValidationErrorCode UnknownWitness = new("legal.submitTestimony.unknownWitness");
    public static readonly ValidationErrorCode WitnessDeceased = new("legal.submitTestimony.witnessDeceased");

    public static readonly CommandPipeline<WorldState, SubmitTestimonyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SubmitTestimonyCommand command)
    {
        if (!state.LegalCases.TryGet(command.CaseId, out var legalCase))
            return UnknownCase;
        if (legalCase!.Depth != LegalCaseDepth.Major)
            return NotAMajorCase;
        if (legalCase.Stage != LegalCaseStage.EvidenceGathering)
            return NotGatheringEvidence;
        if (!state.Characters.TryGet(command.WitnessCharacterId, out var witness))
            return UnknownWitness;
        if (!witness!.IsAlive)
            return WitnessDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SubmitTestimonyCommand command)
    {
        state.LegalCases.TryGet(command.CaseId, out var legalCase);
        state.Characters.TryGet(command.WitnessCharacterId, out var witness);

        var gain = LegalCatalog.TestimonyCaseStrengthGain
            + (witness!.Traits.Contains(LegalCatalog.LegalScholarTraitId) ? LegalCatalog.LegalScholarCaseStrengthBonus : 0);

        var updated = command.SupportingSide == LegalCaseSide.Plaintiff
            ? legalCase! with { PlaintiffCaseStrength = legalCase.PlaintiffCaseStrength + gain }
            : legalCase! with { DefendantCaseStrength = legalCase.DefendantCaseStrength + gain };

        state.LegalCases.Remove(command.CaseId);
        state.LegalCases.Add(command.CaseId, updated);

        return new IDomainEvent[]
        {
            new TestimonySubmittedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CaseId, command.WitnessCharacterId,
                command.SupportingSide, gain, command.CommandId.ToTaggedString()),
        };
    }
}
