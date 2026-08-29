using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>The reliability tier an Auspices reading resolves at (Phase 12 item 3; §4.2/§10's own
/// <c>reliabilityTier</c> field, "Sacerdos Domesticus (base) &lt; hired Haruspex (mid) &lt; Augur
/// officeholder (superior)"). Only the two ends of that three-tier list are built: <see
/// cref="HouseholdDefault"/> stands in for the household's own reading (§4.2: "a household's own
/// Sacerdos Domesticus can take basic Auspices at a modest reliability") without a real Sacerdos
/// Domesticus role to check, since Companions &amp; Court Positions has no code anywhere in this
/// repository (see <see cref="PriesthoodOffice"/>'s own doc comment for the identical dependency gap);
/// <see cref="AugurSuperior"/> reads a real, active <see cref="PriesthoodOffice.Augur"/> record. The
/// middle "hired Haruspex" tier §6.2 separately describes is not built — it needs both a paid,
/// per-reading hire mechanism (Companions &amp; Court Positions' own "wage-earning free labor" pattern,
/// again unbuilt) and a priced <c>incense</c> Good that does not exist in content yet (see <see
/// cref="ReligionCatalog.AuspicesFee"/>'s own doc comment) — two real, named dependency gaps, not one
/// judgment call, matching this item's own "note the gap, don't invent a stand-in" discipline.</summary>
public enum AuspicesReliabilityTier
{
    HouseholdDefault,
    AugurSuperior,
}

/// <summary>
/// The active, commissionable Auspices action (Phase 12 item 3; §4.2: "taking the Auspices before a
/// major decision... consumes Incense... and returns an 'informed risk' preview"). <paramref
/// name="PrecedingDecisionType"/> is a plain, free-form string (§10's own <c>precedingDecisionType</c>
/// field lists "militaryCampaign | travel | settlementFounding | marriage | other") rather than a closed
/// enum, matching <see cref="Reputation.AdjustDignitasCommand"/>'s identical <c>Reason</c> convention —
/// no single catalog of "decisions an Auspices reading can precede" exists across the unbuilt systems
/// §4.2 names (Military &amp; Combat, Travel, and settlement founding are all themselves unbuilt).
///
/// <b>Scope note:</b> §4.2's own payoff — "a real skew toward or away from the decision... a one-time
/// reroll/insurance against the single worst possible outcome of the action it precedes" — is not wired
/// into any preceding decision's own resolution, because none of those decisions (a Military campaign
/// launch, a Travel journey, a settlement founding) has a resolution system in this codebase yet to
/// skew. This command's own payoff is scoped to Religion's own Favor axis directly (§2.2's "a
/// well-executed Auspices reading" Favor-gain bullet) — the reliability tier still matters, and still
/// pays out differently, but the downstream decision-skew consumer is a future integration point,
/// matching <see cref="Reputation.AdjustDignitasCommand"/>'s own "no such caller exists yet" precedent
/// applied to a consumer instead of a caller.
/// </summary>
public sealed record CommissionAuspicesCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character>? PerformedByCharacterId,
    string PrecedingDecisionType) : ICommand;

/// <summary>Emitted whenever a <see cref="CommissionAuspicesCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> the fee produces. Public — commissioning Auspices is a visible
/// household act, matching every other Favor-moving event in this domain.</summary>
public sealed record AuspicesCommissionedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character>? PerformedByCharacterId,
    AuspicesReliabilityTier ReliabilityTier,
    string PrecedingDecisionType,
    int FavorGain,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.auspicesCommissioned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="CommissionAuspicesCommand"/> (ADR 0006).</summary>
public static class CommissionAuspicesCommands
{
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.commissionAuspices.noPatronDeityYet");
    public static readonly ValidationErrorCode EmptyDecisionType = new("religion.commissionAuspices.emptyDecisionType");
    public static readonly ValidationErrorCode PerformerNotFound = new("religion.commissionAuspices.performerNotFound");
    public static readonly ValidationErrorCode PerformerDeceased = new("religion.commissionAuspices.performerDeceased");
    public static readonly ValidationErrorCode InsufficientTreasury = new("religion.commissionAuspices.insufficientTreasury");

    private static readonly LedgerAccountKey AuspicesSink = new(LedgerAccountKind.System, "religion:auspices");

    public static readonly CommandPipeline<WorldState, CommissionAuspicesCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, CommissionAuspicesCommand command)
    {
        if (!HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityYet;
        if (string.IsNullOrWhiteSpace(command.PrecedingDecisionType))
            return EmptyDecisionType;

        if (command.PerformedByCharacterId is { } performerId)
        {
            if (!state.Characters.TryGet(performerId, out var performer))
                return PerformerNotFound;
            if (!performer!.IsAlive)
                return PerformerDeceased;
        }

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < ReligionCatalog.AuspicesFee)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CommissionAuspicesCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -ReligionCatalog.AuspicesFee),
                new LedgerPosting(AuspicesSink, ReligionCatalog.AuspicesFee),
            },
            reference: $"religion:auspices:{command.CommandId.ToTaggedString()}");

        var isAugur = command.PerformedByCharacterId is { } performerId &&
            PriesthoodResolver.ActiveRecord(state, command.SettlementId, PriesthoodOffice.Augur, performerId) is not null;
        var tier = isAugur ? AuspicesReliabilityTier.AugurSuperior : AuspicesReliabilityTier.HouseholdDefault;
        var favorGain = tier == AuspicesReliabilityTier.AugurSuperior
            ? ReligionCatalog.AuspicesAugurFavorGain
            : ReligionCatalog.AuspicesDefaultFavorGain;

        HouseholdReligionResolver.ApplyFavorDelta(state, command.HouseholdId, favorGain);

        return new IDomainEvent[]
        {
            posted,
            new AuspicesCommissionedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.SettlementId, command.PerformedByCharacterId,
                tier, command.PrecedingDecisionType, favorGain, posted.TransactionId, command.CommandId.ToTaggedString()),
        };
    }
}
