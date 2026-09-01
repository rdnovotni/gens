using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>§6/§10's <c>ActioProSocioCase</c> (Phase 15 item 2) — "a formal legal action one partner
/// could bring against another demanding a full, honest accounting of the partnership's own affairs...
/// resolves as a real, new Legal &amp; Court case type... with the same evidence-and-Hearing structure
/// that document already uses... rather than an invented parallel process." Rather than a separate
/// case record duplicating <see cref="LegalCase"/>'s own already-tested Filed/EvidenceGathering/
/// Hearing/Ruled machinery, this is the association: which <see cref="Societas"/> and <see
/// cref="PartnerDisputeType"/> a given <see cref="LegalCase.CaseId"/> is actually about — a new, sparse
/// <see cref="WorldState"/> partition keyed by that already-issued case ID, matching <see
/// cref="RealEstate.PlotPropertyExtension"/>'s own identical "wrap the existing record in a parallel
/// partition rather than edit its schema" convention, applied here to <see cref="LegalCase"/> instead
/// of <see cref="Land.Plot"/>.</summary>
public sealed record ActioProSocioLink(RuntimeId<LegalCase> CaseId, RuntimeId<Societas> SocietasId, PartnerDisputeType DisputeType);

/// <summary>
/// §6's contested-dissolution filing path (Phase 15 item 2): wraps <see
/// cref="FileLawsuitCommands.CreatePipeline"/> with <see cref="LegalCaseType.PartnershipDispute"/> —
/// the "real, new Legal &amp; Court case type" §6 calls for — always at <see
/// cref="LegalCaseDepth.Major"/>, never Quick: an actio pro socio is inherently "a full, honest
/// accounting," not a same-session snap judgment, and forcing Major also means the case is never Ruled
/// before this command can record its own <see cref="ActioProSocioLink"/> (Quick Resolution would
/// otherwise rule the case inline, inside the nested <see cref="FileLawsuitCommand"/> execution, before
/// this command ever reaches the line that adds the link <see cref="ActioProSocioResolutionHook"/>
/// depends on).
///
/// <b>Household-level parties, narrowed further than <see cref="LegalCase"/>'s own existing scope
/// decision:</b> both <see cref="PlaintiffPartnerOwner"/> and <see cref="RespondentPartnerOwner"/> must
/// be <see cref="PropertyOwnerKind.PlayerHousehold"/> partners of <see cref="SocietasId"/> — <see
/// cref="LegalCase"/> already resolves every case at Household granularity (that record's own doc
/// comment), and this item does not add a Character-to-Household reverse lookup just to let an <see
/// cref="PropertyOwnerKind.IndividualCharacter"/> partner file or answer one directly.
/// </summary>
public sealed record FileActioProSocioCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef PlaintiffPartnerOwner,
    PropertyOwnerRef RespondentPartnerOwner,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> FilingCharacterId,
    PartnerDisputeType DisputeType) : ICommand;

public static class FileActioProSocioCommands
{
    public static readonly ValidationErrorCode SocietasNotFound = new("societates.fileActioProSocio.societasNotFound");
    public static readonly ValidationErrorCode SamePartner = new("societates.fileActioProSocio.samePartner");
    public static readonly ValidationErrorCode PlaintiffNotAPartner = new("societates.fileActioProSocio.plaintiffNotAPartner");
    public static readonly ValidationErrorCode RespondentNotAPartner = new("societates.fileActioProSocio.respondentNotAPartner");
    public static readonly ValidationErrorCode PartiesMustBeHouseholds = new("societates.fileActioProSocio.partiesMustBeHouseholds");
    public static readonly ValidationErrorCode SettlementNotFound = new("societates.fileActioProSocio.settlementNotFound");
    public static readonly ValidationErrorCode UnknownFilingCharacter = new("societates.fileActioProSocio.unknownFilingCharacter");
    public static readonly ValidationErrorCode FilingCharacterDeceased = new("societates.fileActioProSocio.filingCharacterDeceased");
    public static readonly ValidationErrorCode FilingCharacterNotInPlaintiffHousehold = new("societates.fileActioProSocio.filingCharacterNotInPlaintiffHousehold");
    public static readonly ValidationErrorCode InsufficientTreasury = new("societates.fileActioProSocio.insufficientTreasury");

    public static CommandPipeline<WorldState, FileActioProSocioCommand> CreatePipeline(RandomStreamSet randomStreams)
    {
        if (randomStreams is null)
            throw new ArgumentNullException(nameof(randomStreams));

        return new CommandPipeline<WorldState, FileActioProSocioCommand>(
            validate: Validate,
            mutate: (state, command) => Mutate(state, command, randomStreams),
            issueSequenceNumber: static state => state.IssueCommandSequenceNumber());
    }

    private static ValidationErrorCode? Validate(WorldState state, FileActioProSocioCommand command)
    {
        if (!state.Societates.TryGet(command.SocietasId, out var societas) || !societas!.IsActive)
            return SocietasNotFound;
        if (command.PlaintiffPartnerOwner == command.RespondentPartnerOwner)
            return SamePartner;
        if (!SocietasResolver.IsPartner(societas, command.PlaintiffPartnerOwner))
            return PlaintiffNotAPartner;
        if (!SocietasResolver.IsPartner(societas, command.RespondentPartnerOwner))
            return RespondentNotAPartner;
        if (command.PlaintiffPartnerOwner.Kind != PropertyOwnerKind.PlayerHousehold ||
            command.RespondentPartnerOwner.Kind != PropertyOwnerKind.PlayerHousehold)
            return PartiesMustBeHouseholds;

        // Mirrors FileLawsuitCommand's own preconditions (Legal.FileLawsuitCommands.Validate) — this
        // command wraps that pipeline for a Major-depth PartnershipDispute filing rather than
        // reimplementing its resolution logic, but must still fail fast on the same preconditions
        // before ever reaching the nested Execute below.
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (!state.Characters.TryGet(command.FilingCharacterId, out var filer))
            return UnknownFilingCharacter;
        if (!filer!.IsAlive)
            return FilingCharacterDeceased;
        var plaintiffHouseholdId = RuntimeId<Household>.Parse(command.PlaintiffPartnerOwner.OwnerId!);
        if (filer.Household != plaintiffHouseholdId)
            return FilingCharacterNotInPlaintiffHousehold;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(plaintiffHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < LegalCatalog.MajorFilingCost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FileActioProSocioCommand command, RandomStreamSet randomStreams)
    {
        var plaintiffHouseholdId = RuntimeId<Household>.Parse(command.PlaintiffPartnerOwner.OwnerId!);
        var respondentHouseholdId = RuntimeId<Household>.Parse(command.RespondentPartnerOwner.OwnerId!);

        var lawsuitResult = FileLawsuitCommands.CreatePipeline(randomStreams).Execute(
            state, new FileLawsuitCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                LegalCaseType.PartnershipDispute, LegalCaseDepth.Major, plaintiffHouseholdId, respondentHouseholdId,
                command.SettlementId, command.FilingCharacterId));

        var events = new List<IDomainEvent>(lawsuitResult.Events);
        var filedEvent = lawsuitResult.Events.OfType<LawsuitFiledEvent>().Single();

        state.ActioProSocioLinks.Add(filedEvent.CaseId, new ActioProSocioLink(filedEvent.CaseId, command.SocietasId, command.DisputeType));

        return events.ToArray();
    }
}
