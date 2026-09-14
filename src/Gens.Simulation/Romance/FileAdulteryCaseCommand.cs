using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>
/// §12's real adultery prosecution filing (Phase 17 item 3 slice 9): wraps <see
/// cref="FileLawsuitCommands.CreatePipeline"/> with <see cref="LegalCaseType.Adultery"/>, always at <see
/// cref="LegalCaseDepth.Major"/> — matching <see cref="PublicContracts.FileRepetundaeCaseCommand"/>'s
/// and <see cref="Societates.FileActioProSocioCommand"/>'s own identical "force Major so the case is
/// never Ruled before this command can record its own link" reasoning.
///
/// <b>Composition with <see cref="ResolveAffairCommand"/>:</b> this command, not <see
/// cref="ResolveAffairCommand"/>, is the one place that writes <see cref="AffairRecord.Resolution"/> for
/// the <see cref="AffairResolution.ProsecutedAdultery"/> path (alongside <see
/// cref="AffairRecord.LegalCaseId"/>) — <see cref="ResolveAffairCommand"/>'s own <see
/// cref="AffairResolution.ProsecutedAdultery"/> branch derives this command's own two extra parameters
/// (<see cref="SettlementId"/> from the wronged spouse's own <see cref="Character.Location"/>, <see
/// cref="FilingCharacterId"/> as the wronged spouse themselves) and delegates entirely to this pipeline,
/// rather than each command racing to set the same field. This avoids the double-write the approved plan
/// itself flags as a real risk here: exactly one command ever sets <see cref="AffairRecord.Resolution"/>
/// for this path, and it is this one.
///
/// <b>The accusing/defendant households</b> resolve directly from the linked <see
/// cref="AffairRecord.WrongedSpouseId"/>'s and <see cref="AffairRecord.OffenderCharacterId"/>'s own <see
/// cref="Character.Household"/> — this command honestly fails validation when either has no household to
/// name as a party, mirroring <see cref="PublicContracts.FileRepetundaeCaseCommand"/>'s own identical "no
/// household to name as defendant" precedent, rather than fabricating one.
/// </summary>
public sealed record FileAdulteryCaseCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<AffairRecord> AffairId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> FilingCharacterId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="FileAdulteryCaseCommand"/> (ADR 0006).</summary>
public static class FileAdulteryCaseCommands
{
    public static readonly ValidationErrorCode AffairRecordNotFound = new("romance.fileAdulteryCase.affairRecordNotFound");
    public static readonly ValidationErrorCode AffairAlreadyResolved = new("romance.fileAdulteryCase.affairAlreadyResolved");
    public static readonly ValidationErrorCode AlreadyLinkedToCase = new("romance.fileAdulteryCase.alreadyLinkedToCase");
    public static readonly ValidationErrorCode WrongedSpouseHasNoHousehold = new("romance.fileAdulteryCase.wrongedSpouseHasNoHousehold");
    public static readonly ValidationErrorCode OffenderHasNoHousehold = new("romance.fileAdulteryCase.offenderHasNoHousehold");
    public static readonly ValidationErrorCode SameHousehold = new("romance.fileAdulteryCase.sameHousehold");
    public static readonly ValidationErrorCode SettlementNotFound = new("romance.fileAdulteryCase.settlementNotFound");
    public static readonly ValidationErrorCode UnknownFilingCharacter = new("romance.fileAdulteryCase.unknownFilingCharacter");
    public static readonly ValidationErrorCode FilingCharacterDeceased = new("romance.fileAdulteryCase.filingCharacterDeceased");
    public static readonly ValidationErrorCode FilingCharacterNotInAccusingHousehold = new("romance.fileAdulteryCase.filingCharacterNotInAccusingHousehold");
    public static readonly ValidationErrorCode InsufficientTreasury = new("romance.fileAdulteryCase.insufficientTreasury");

    public static CommandPipeline<WorldState, FileAdulteryCaseCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, FileAdulteryCaseCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, FileAdulteryCaseCommand command)
    {
        if (!state.AffairRecords.TryGet(command.AffairId, out var record))
            return AffairRecordNotFound;
        if (record!.Resolution is not null)
            return AffairAlreadyResolved;
        if (record.LegalCaseId is not null)
            return AlreadyLinkedToCase;

        if (!state.Characters.TryGet(record.WrongedSpouseId, out var wrongedSpouse) || wrongedSpouse!.Household is not { } accusingHouseholdId)
            return WrongedSpouseHasNoHousehold;
        if (!state.Characters.TryGet(record.OffenderCharacterId, out var offender) || offender!.Household is not { } defendantHouseholdId)
            return OffenderHasNoHousehold;
        if (accusingHouseholdId == defendantHouseholdId)
            return SameHousehold;

        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (!state.Characters.TryGet(command.FilingCharacterId, out var filer))
            return UnknownFilingCharacter;
        if (!filer!.IsAlive)
            return FilingCharacterDeceased;
        if (filer.Household != accusingHouseholdId)
            return FilingCharacterNotInAccusingHousehold;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(accusingHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < LegalCatalog.MajorFilingCost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FileAdulteryCaseCommand command, RandomStreamSet randomStreams)
    {
        state.AffairRecords.TryGet(command.AffairId, out var record);
        state.Characters.TryGet(record!.WrongedSpouseId, out var wrongedSpouse);
        state.Characters.TryGet(record.OffenderCharacterId, out var offender);
        var accusingHouseholdId = wrongedSpouse!.Household!.Value;
        var defendantHouseholdId = offender!.Household!.Value;

        var lawsuitResult = FileLawsuitCommands.CreatePipeline(randomStreams).Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                LegalCaseType.Adultery, LegalCaseDepth.Major, accusingHouseholdId, defendantHouseholdId,
                command.SettlementId, command.FilingCharacterId));

        var events = new List<IDomainEvent>(lawsuitResult.Events);
        var filedEvent = lawsuitResult.Events.OfType<LawsuitFiledEvent>().Single();

        state.AdulteryCaseLinks.Add(filedEvent.CaseId, new AdulteryCaseLink(filedEvent.CaseId, command.AffairId));

        state.AffairRecords.Remove(command.AffairId);
        state.AffairRecords.Add(
            command.AffairId,
            record with { Resolution = AffairResolution.ProsecutedAdultery, LegalCaseId = filedEvent.CaseId });

        return events.ToArray();
    }
}
