using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>Which <see cref="ContractFraudRecord"/> a given <see cref="LegalCase.CaseId"/> is actually
/// about — a new, sparse <see cref="WorldState"/> partition keyed by that already-issued case ID,
/// matching <see cref="Societates.ActioProSocioLink"/>'s and <see
/// cref="RealEstate.PlotPropertyExtension"/>'s identical "wrap the existing record in a parallel
/// partition rather than edit its schema" convention.</summary>
public sealed record ContractFraudLegalLink(RuntimeId<LegalCase> CaseId, RuntimeId<ContractFraudRecord> FraudRecordId);

/// <summary>
/// §6.2's real repetundae prosecution filing (Phase 15 item 6): wraps <see
/// cref="FileLawsuitCommands.CreatePipeline"/> with <see cref="LegalCaseType.Repetundae"/>, always at
/// <see cref="LegalCaseDepth.Major"/> — matching <see cref="Societates.FileActioProSocioCommand"/>'s
/// own identical "force Major so the case is never Ruled before this command can record its own link"
/// reasoning. §5.1's "a rejected bidder... has a real, standing motive to investigate" names the natural
/// filer this command expects (<paramref name="AccusingHouseholdId"/>), though nothing here requires the
/// accuser to have actually been a losing bidder on this exact contract — that motive is narrative, not a
/// mechanical precondition this item enforces.
///
/// <b>Only a household-resolving holder is prosecutable:</b> <see cref="LegalCase"/> resolves every case
/// at Household granularity (that record's own doc comment); a <see
/// cref="ContractFraudRecord.Holder"/> that is a <see cref="ContractBidderKind.RivalHouse"/> has no
/// <see cref="RuntimeId{Household}"/> this codebase can name as a defendant (matching <see
/// cref="ContractBidderResolver.TryResolveHousehold"/>'s own identical narrowing) — this command
/// honestly fails validation for that case rather than fabricating a household to sue. A discovered
/// fraud against such a holder still carries §6.1's own real financial exposure risk that discovery
/// itself already created; only the formal §6.2 prosecution path is unreachable for it.
/// </summary>
public sealed record FileRepetundaeCaseCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<ContractFraudRecord> FraudRecordId,
    RuntimeId<Household> AccusingHouseholdId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> FilingCharacterId) : ICommand;

public static class FileRepetundaeCaseCommands
{
    public static readonly ValidationErrorCode FraudRecordNotFound = new("publicContracts.fileRepetundae.fraudRecordNotFound");
    public static readonly ValidationErrorCode AlreadyLinkedToCase = new("publicContracts.fileRepetundae.alreadyLinkedToCase");
    public static readonly ValidationErrorCode HolderNotProsecutable = new("publicContracts.fileRepetundae.holderNotProsecutable");
    public static readonly ValidationErrorCode SameHousehold = new("publicContracts.fileRepetundae.sameHousehold");
    public static readonly ValidationErrorCode SettlementNotFound = new("publicContracts.fileRepetundae.settlementNotFound");
    public static readonly ValidationErrorCode UnknownFilingCharacter = new("publicContracts.fileRepetundae.unknownFilingCharacter");
    public static readonly ValidationErrorCode FilingCharacterDeceased = new("publicContracts.fileRepetundae.filingCharacterDeceased");
    public static readonly ValidationErrorCode FilingCharacterNotInAccusingHousehold = new("publicContracts.fileRepetundae.filingCharacterNotInAccusingHousehold");
    public static readonly ValidationErrorCode InsufficientTreasury = new("publicContracts.fileRepetundae.insufficientTreasury");

    public static CommandPipeline<WorldState, FileRepetundaeCaseCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, FileRepetundaeCaseCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, FileRepetundaeCaseCommand command)
    {
        if (!state.PublicContractFraudRecords.TryGet(command.FraudRecordId, out var record))
            return FraudRecordNotFound;
        if (record!.LegalCaseId is not null)
            return AlreadyLinkedToCase;
        if (!ContractBidderResolver.TryResolveHousehold(state, record.Holder, out var defendantHouseholdId))
            return HolderNotProsecutable;
        if (defendantHouseholdId == command.AccusingHouseholdId)
            return SameHousehold;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (!state.Characters.TryGet(command.FilingCharacterId, out var filer))
            return UnknownFilingCharacter;
        if (!filer!.IsAlive)
            return FilingCharacterDeceased;
        if (filer.Household != command.AccusingHouseholdId)
            return FilingCharacterNotInAccusingHousehold;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.AccusingHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < LegalCatalog.MajorFilingCost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FileRepetundaeCaseCommand command, RandomStreamSet randomStreams)
    {
        state.PublicContractFraudRecords.TryGet(command.FraudRecordId, out var record);
        ContractBidderResolver.TryResolveHousehold(state, record!.Holder, out var defendantHouseholdId);

        var lawsuitResult = FileLawsuitCommands.CreatePipeline(randomStreams).Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                LegalCaseType.Repetundae, LegalCaseDepth.Major, command.AccusingHouseholdId, defendantHouseholdId,
                command.SettlementId, command.FilingCharacterId));

        var events = new List<IDomainEvent>(lawsuitResult.Events);
        var filedEvent = lawsuitResult.Events.OfType<LawsuitFiledEvent>().Single();

        state.ContractFraudLegalLinks.Add(filedEvent.CaseId, new ContractFraudLegalLink(filedEvent.CaseId, command.FraudRecordId));

        state.PublicContractFraudRecords.Remove(command.FraudRecordId);
        state.PublicContractFraudRecords.Add(command.FraudRecordId, record with { LegalCaseId = filedEvent.CaseId });

        return events.ToArray();
    }
}
