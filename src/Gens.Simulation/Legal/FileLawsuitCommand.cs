using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Random;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>
/// Opens a new <see cref="LegalCase"/> (Phase 12 item 4; §5.1's Filing stage). Household-level parties
/// throughout — see <see cref="LegalCase"/>'s own doc comment for that scope decision. <see
/// cref="FilingCharacterId"/> is the household member actually bringing the suit: a Litigious Character
/// (§10's own Traits cross-reference) files with real, if modest, extra weight behind the accusation from
/// the start (a direct <see cref="LegalCatalog.LitigiousTraitId"/> membership check, matching <see
/// cref="Religion.RespondToOmenCommand"/>'s own precedent for reading personality through a known content
/// trait id rather than an axis score this domain has no access path to).
///
/// §4's Quick Resolution is not a separate command — it resolves inline, in this same submission, the
/// moment <see cref="Depth"/> is <see cref="LegalCaseDepth.Quick"/>: "most disputes resolve in a single
/// weighted check, the same session they're filed." A Major case instead opens at <see
/// cref="LegalCaseStage.EvidenceGathering"/> and is carried forward by <see
/// cref="LegalCaseAdvancementSystem"/>. Either way this command needs a real dice roll (Quick's own
/// resolution, via <see cref="LegalCaseResolver.RollVerdict"/>), captured through a named <see
/// cref="RandomStreamSet"/> stream the same way <see cref="Religion.RespondToOmenCommand"/>'s own
/// <c>CreatePipeline</c> already captures one for an RNG-using command (rule 8).
/// </summary>
public sealed record FileLawsuitCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    LegalCaseType CaseType,
    LegalCaseDepth Depth,
    RuntimeId<Household> PlaintiffId,
    RuntimeId<Household> DefendantId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> FilingCharacterId,
    bool IsPatriaPotestasCase = false) : ICommand;

/// <summary>Emitted whenever a <see cref="FileLawsuitCommand"/> is accepted. Public — a filed suit is a
/// formal, on-the-record civic act (§2's own "formal disputes and lawsuits"), not a private fact between
/// two Characters, matching <see cref="Reputation.AdjustDignitasCommand"/>'s own Dignitas-is-public
/// precedent for a comparably public standing-affecting act.</summary>
public sealed record LawsuitFiledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    LegalCaseType CaseType,
    LegalCaseDepth Depth,
    RuntimeId<Household> PlaintiffId,
    RuntimeId<Household> DefendantId,
    RuntimeId<Character>? PresidingCharacterId,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.lawsuitFiled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PlaintiffId.ToTaggedString(), DefendantId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FileLawsuitCommand"/> (ADR 0006).</summary>
public static class FileLawsuitCommands
{
    /// <summary>The named random stream (rule 8) reserved for Quick Resolution's own verdict roll —
    /// registered in <see cref="Campaign.CampaignBootstrapper"/>. <see cref="LegalCaseAdvancementSystem"/>
    /// uses its own, separate stream for the Major-case Hearing roll.</summary>
    public const string QuickResolutionStreamName = "legal.quickResolutionOutcome";

    public static readonly ValidationErrorCode SameHousehold = new("legal.fileLawsuit.sameHousehold");
    public static readonly ValidationErrorCode UnknownSettlement = new("legal.fileLawsuit.unknownSettlement");
    public static readonly ValidationErrorCode UnknownFilingCharacter = new("legal.fileLawsuit.unknownFilingCharacter");
    public static readonly ValidationErrorCode FilingCharacterDeceased = new("legal.fileLawsuit.filingCharacterDeceased");

    private static readonly LedgerAccountKey FilingFeeSink = new(LedgerAccountKind.System, "legal:filingFees");

    public static CommandPipeline<WorldState, FileLawsuitCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, FileLawsuitCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, FileLawsuitCommand command)
    {
        if (command.PlaintiffId == command.DefendantId)
            return SameHousehold;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return UnknownSettlement;
        if (!state.Characters.TryGet(command.FilingCharacterId, out var filer))
            return UnknownFilingCharacter;
        if (!filer!.IsAlive)
            return FilingCharacterDeceased;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FileLawsuitCommand command, RandomStreamSet randomStreams)
    {
        var events = new List<IDomainEvent>();

        var presidingId = LegalCaseResolver.SelectPresidingMagistrate(
            state, command.SettlementId, command.PlaintiffId, command.DefendantId);

        var filingCost = command.Depth == LegalCaseDepth.Quick ? LegalCatalog.QuickFilingCost : LegalCatalog.MajorFilingCost;
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.PlaintiffId), -filingCost),
                new LedgerPosting(FilingFeeSink, filingCost),
            },
            reference: $"legal:filingFee:{command.CommandId.ToTaggedString()}");
        events.Add(posted);

        state.Characters.TryGet(command.FilingCharacterId, out var filer);
        var litigiousBonus = filer!.Traits.Contains(LegalCatalog.LitigiousTraitId) ? LegalCatalog.TestimonyCaseStrengthGain : 0;

        var caseId = state.LegalCaseIds.Issue();
        var legalCase = new LegalCase(
            caseId, command.CaseType, command.PlaintiffId, command.DefendantId, command.SettlementId,
            command.Depth, LegalCaseStage.Filed, command.SubmittedDate, presidingId,
            PlaintiffCaseStrength: litigiousBonus,
            IsPatriaPotestasCase: command.IsPatriaPotestasCase);

        state.LegalCases.Add(caseId, legalCase);

        events.Add(new LawsuitFiledEvent(
            state.EventIds.Issue(), command.SubmittedDate, caseId, command.CaseType, command.Depth,
            command.PlaintiffId, command.DefendantId, presidingId, command.CommandId.ToTaggedString()));

        if (command.Depth == LegalCaseDepth.Quick)
        {
            var (verdict, sentence) = LegalCaseResolver.RollVerdict(state, legalCase, randomStreams, QuickResolutionStreamName);
            events.AddRange(LegalCaseRuling.Apply(state, legalCase, verdict, sentence, command.SubmittedDate, command.CommandId.ToTaggedString()));
        }
        else
        {
            state.LegalCases.Remove(caseId);
            state.LegalCases.Add(caseId, legalCase with { Stage = LegalCaseStage.EvidenceGathering });
        }

        return events.ToArray();
    }
}
