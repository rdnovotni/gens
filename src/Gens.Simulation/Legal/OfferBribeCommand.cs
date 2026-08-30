using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Legal;

/// <summary>
/// Offers the presiding magistrate a bribe on behalf of one party to a Major <see cref="LegalCase"/>
/// (Phase 12 item 4; §7: "Bribery — Economy &amp; Finance's Bribes (§4.2) apply directly to a presiding
/// magistrate's Greed axis, the same mechanism that already governs bribability everywhere else in this
/// project"). No <c>OfferBribeCommand</c> exists anywhere else in this codebase to reuse (only <see
/// cref="LedgerTransactionCategory.Gifts"/>'s own doc comment names bribes as belonging to that category)
/// — this item builds the first one, using that category directly rather than inventing a new one. The
/// magistrate's own Greed axis is not read here: no per-Character axis score is reachable from this
/// domain (see <see cref="LegalCatalog"/>'s own doc comment on that same limitation), so a bribe's Denarii
/// amount converts directly to case-score weight instead — a flat, if cruder, stand-in for "how bribable
/// is this particular presider," matching every other Phase 12 item's own "the design doc's real gate
/// isn't reachable, so a simpler real one substitutes" precedent (e.g. Auspices pricing its reliability
/// fee in plain Money rather than an unbacked Incense Good).
/// </summary>
public sealed record OfferBribeCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Household> BribingHouseholdId,
    Money Amount) : ICommand;

/// <summary>Emitted whenever an <see cref="OfferBribeCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> the spend itself produces. Deliberately <see
/// cref="Commands.Visibility.Private"/> to the briber and the presiding magistrate only — a bribe is,
/// definitionally, concealed, the sharp contrast to every other event this domain emits (<see
/// cref="LawsuitFiledEvent"/>, <see cref="TestimonySubmittedEvent"/>, <see
/// cref="Legal.LegalCaseRuledEvent"/>) all being public, on-the-record civic facts.</summary>
public sealed record BribeOfferedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<LegalCase> CaseId,
    RuntimeId<Household> BribingHouseholdId,
    RuntimeId<Characters.Character> PresidingCharacterId,
    Money Amount,
    int BriberyWeightGain,
    string? CausationId) : IDomainEvent
{
    public string Type => "legal.bribeOffered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BribingHouseholdId.ToTaggedString(), PresidingCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(BribingHouseholdId.ToTaggedString(), PresidingCharacterId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="OfferBribeCommand"/> (ADR 0006).</summary>
public static class OfferBribeCommands
{
    public static readonly ValidationErrorCode UnknownCase = new("legal.offerBribe.unknownCase");
    public static readonly ValidationErrorCode NotAMajorCase = new("legal.offerBribe.notAMajorCase");
    public static readonly ValidationErrorCode CaseAlreadyRuled = new("legal.offerBribe.caseAlreadyRuled");
    public static readonly ValidationErrorCode NoPresiderAssigned = new("legal.offerBribe.noPresiderAssigned");
    public static readonly ValidationErrorCode NotAParty = new("legal.offerBribe.notAParty");
    public static readonly ValidationErrorCode NonPositiveAmount = new("legal.offerBribe.nonPositiveAmount");
    public static readonly ValidationErrorCode InsufficientTreasury = new("legal.offerBribe.insufficientTreasury");

    private static readonly LedgerAccountKey BriberySink = new(LedgerAccountKind.System, "legal:bribery");

    public static readonly CommandPipeline<WorldState, OfferBribeCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, OfferBribeCommand command)
    {
        if (!state.LegalCases.TryGet(command.CaseId, out var legalCase))
            return UnknownCase;
        if (legalCase!.Depth != LegalCaseDepth.Major)
            return NotAMajorCase;
        if (legalCase.Stage == LegalCaseStage.Ruled)
            return CaseAlreadyRuled;
        if (legalCase.PresidingCharacterId is null)
            return NoPresiderAssigned;
        if (command.BribingHouseholdId != legalCase.PlaintiffId && command.BribingHouseholdId != legalCase.DefendantId)
            return NotAParty;
        if (command.Amount <= Money.Zero)
            return NonPositiveAmount;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.BribingHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, OfferBribeCommand command)
    {
        state.LegalCases.TryGet(command.CaseId, out var legalCase);

        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.BribingHouseholdId), -command.Amount),
                new LedgerPosting(BriberySink, command.Amount),
            },
            reference: $"legal:bribe:{command.CommandId.ToTaggedString()}");

        var weightGain = (int)Math.Min(
            command.Amount.RawValue / Money.ScaleFactor / 10 * LegalCatalog.BriberyWeightPerTenDenarii,
            LegalCatalog.MaxBriberyWeight);

        var isPlaintiff = command.BribingHouseholdId == legalCase!.PlaintiffId;
        var updated = isPlaintiff
            ? legalCase with { PlaintiffBriberyWeight = Math.Min(legalCase.PlaintiffBriberyWeight + weightGain, LegalCatalog.MaxBriberyWeight) }
            : legalCase with { DefendantBriberyWeight = Math.Min(legalCase.DefendantBriberyWeight + weightGain, LegalCatalog.MaxBriberyWeight) };

        state.LegalCases.Remove(command.CaseId);
        state.LegalCases.Add(command.CaseId, updated);

        return new IDomainEvent[]
        {
            posted,
            new BribeOfferedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CaseId, command.BribingHouseholdId,
                legalCase.PresidingCharacterId!.Value, command.Amount, weightGain, command.CommandId.ToTaggedString()),
        };
    }
}
