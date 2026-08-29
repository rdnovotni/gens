using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// The funded Sacred Calendar observance tier (Phase 12 item 3; §5: "a genuine Funded Action... buying
/// a real Favor and Dignitas payoff sized to the spend"). §5 itself frames this as the same Funded
/// Action category <c>gens-economy-finance-design.md</c> §4.3 already flagged and Policies &amp; Edicts'
/// own Phase 9 item 2 already realized as <see cref="Policies.FundFestivalCommand"/> — that command
/// already exists, moves the money, and posts the ledger receipt, but its own doc comment says plainly
/// "Religion (§6, future) is what will eventually turn the spend into an actual Divine Favor/Dignitas
/// payoff." This item deliberately does <b>not</b> retrofit that command to add the payoff: <see
/// cref="Policies.FundFestivalCommand"/> is an already-shipped, already-tested Phase 9 item (its own
/// <c>FundFestivalCommandTests</c> asserts an exact two-event result), and reopening it to change
/// already-tested behavior is exactly the precedent Phase 12 item 1 set for <c>Agnomen.DignitasEffect</c>
/// and the Funerary Grand-tier trade — both left alone rather than retrofitted. Instead, this domain
/// builds its own self-contained command per the task's own explicit fallback for this exact situation:
/// "scope funded festivals down to a direct Favor/Dignitas payoff command rather than inventing a new
/// generic Funded Action system." A future Policies &amp; Edicts pass (§6.12, roadmap item 9, "full
/// edicts, funded actions") is the natural place to unify the two commands under one real, generic
/// Funded Action abstraction — not invented here, matching this item's own narrow scope.
///
/// <b>Scope note:</b> §5's own "plus a Settlement Demographics Contentment boost through the... bread-
/// and-circuses channel" is not wired, for the same reason <see
/// cref="Magistracies.FundAedileWorksCommand"/>'s own Contentment half was left out of Phase 12 item 2 —
/// Settlement Demographics has no household/settlement-scoped Contentment write path this item's scope
/// reaches into. Games &amp; Spectacle's own venue-resolution half of a Ludi-linked feast day (§5's "a
/// Ludi-associated feast day... routes its actual event resolution to that system") is likewise not
/// built — Games &amp; Spectacle (Phase 17) does not exist yet.
/// </summary>
public sealed record FundFestivalCelebrationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    string FeastDay,
    Money Amount) : ICommand;

/// <summary>Emitted whenever a <see cref="FundFestivalCelebrationCommand"/> is accepted, alongside the
/// <see cref="LedgerTransactionPostedEvent"/> the spend produces and the <see
/// cref="Reputation.DignitasChangedEvent"/> its Dignitas half routes through <see
/// cref="AdjustDignitasCommand"/> for. Public, matching <see cref="Policies.FestivalFundedEvent"/>'s own
/// visibility.</summary>
public sealed record FestivalCelebrationFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    string FeastDay,
    Money Amount,
    int FavorGain,
    int DignitasGain,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.festivalCelebrationFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundFestivalCelebrationCommand"/> (ADR 0006).</summary>
public static class FundFestivalCelebrationCommands
{
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.fundFestivalCelebration.noPatronDeityYet");
    public static readonly ValidationErrorCode EmptyFeastDay = new("religion.fundFestivalCelebration.emptyFeastDay");
    public static readonly ValidationErrorCode AmountMustBePositive = new("religion.fundFestivalCelebration.amountMustBePositive");
    public static readonly ValidationErrorCode InsufficientTreasury = new("religion.fundFestivalCelebration.insufficientTreasury");

    private static readonly LedgerAccountKey FestivalSink = new(LedgerAccountKind.System, "religion:festival");

    public static readonly CommandPipeline<WorldState, FundFestivalCelebrationCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundFestivalCelebrationCommand command)
    {
        if (!HouseholdReligionResolver.HasChosenPatron(state, command.HouseholdId))
            return NoPatronDeityYet;
        if (string.IsNullOrWhiteSpace(command.FeastDay))
            return EmptyFeastDay;
        if (command.Amount <= Money.Zero)
            return AmountMustBePositive;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundFestivalCelebrationCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -command.Amount),
                new LedgerPosting(FestivalSink, command.Amount),
            },
            reference: $"religion:festival:{command.CommandId.ToTaggedString()}");

        var denarii = command.Amount.RawValue / Money.ScaleFactor;
        var favorGain = (int)Math.Max(1, denarii / ReligionCatalog.FestivalFavorPerDenarii);
        var dignitasGain = (int)Math.Max(1, denarii / ReligionCatalog.FestivalDignitasPerDenarii);

        HouseholdReligionResolver.ApplyFavorDelta(state, command.HouseholdId, favorGain);

        var dignitasCommand = new AdjustDignitasCommand(
            state.CommandIds.Issue(), "system", command.SubmittedDate, command.CommandId.ToTaggedString(), command.HouseholdId,
            dignitasGain, $"funded the {command.FeastDay} celebration");
        var dignitasEvents = AdjustDignitasCommands.Pipeline.Execute(state, dignitasCommand).Events;

        var events = new List<IDomainEvent> { posted };
        events.AddRange(dignitasEvents);
        events.Add(new FestivalCelebrationFundedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.SettlementId, command.FeastDay,
            command.Amount, favorGain, dignitasGain, posted.TransactionId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
