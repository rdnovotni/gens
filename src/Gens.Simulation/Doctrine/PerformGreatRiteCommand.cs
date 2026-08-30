using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Religion;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Doctrine;

/// <summary>Emitted whenever a <see cref="PerformGreatRiteCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> <see cref="LedgerService.Post"/> itself produces, matching
/// <see cref="Policies.FundFestivalCommand"/>'s identical "both the ledger receipt and the
/// domain-specific event" convention.</summary>
public sealed record GreatRitePerformedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "doctrine.greatRitePerformed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// Domus Pia's Defining capstone (§3.2: "The Great Rite: a one-time, Edict-scale ceremony granting a
/// major Favor and Dignitas surge"). A real, fixed-cost Ledger spend sized like an Edict rather than an
/// ordinary Funded Action (§3.2's own "Edict-scale" framing), mirroring <see
/// cref="Policies.FundFestivalCommand"/>'s "move the money through the ledger, make the spend real,
/// readable state" shape, into the same named system sink convention that command already established
/// (a new, distinct sink here, not a reuse of that command's own <c>fundedaction:festival</c> account —
/// this is a capstone rite, not another ordinary Festival). Requires the household to have already
/// chosen a Patron Deity (<see cref="HouseholdReligion"/>, Phase 12 item 3) — the Great Rite is a
/// religious ceremony, and a household with no chosen deity has, per that item's own doc comment, "no
/// meaningful Favor to default to zero" for this rite to raise.
/// </summary>
public sealed record PerformGreatRiteCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="PerformGreatRiteCommand"/> (ADR 0006).</summary>
public static class PerformGreatRiteCommands
{
    public static readonly ValidationErrorCode DoctrineNotDefining = new("doctrine.performGreatRite.doctrineNotDefining");
    public static readonly ValidationErrorCode CapstoneAlreadyUsed = new("doctrine.performGreatRite.capstoneAlreadyUsed");
    public static readonly ValidationErrorCode NoPatronDeity = new("doctrine.performGreatRite.noPatronDeity");
    public static readonly ValidationErrorCode InsufficientTreasury = new("doctrine.performGreatRite.insufficientTreasury");

    private static readonly LedgerAccountKey GreatRiteSink = new(LedgerAccountKind.System, "doctrine:greatRite");

    public static readonly CommandPipeline<WorldState, PerformGreatRiteCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, PerformGreatRiteCommand command)
    {
        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.DomusPia);
        if (doctrine.Tier != DoctrineTier.Defining)
            return DoctrineNotDefining;
        if (doctrine.CapstoneUsedThisGeneration)
            return CapstoneAlreadyUsed;
        if (!state.HouseholdReligions.TryGet(command.HouseholdId, out _))
            return NoPatronDeity;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < DoctrineCatalog.GreatRiteCost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, PerformGreatRiteCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -DoctrineCatalog.GreatRiteCost),
                new LedgerPosting(GreatRiteSink, DoctrineCatalog.GreatRiteCost),
            },
            reference: $"doctrine:greatRite:{command.CommandId.ToTaggedString()}");

        var doctrine = HouseholdDoctrineResolver.Current(state, command.HouseholdId, HouseholdDoctrineType.DomusPia);
        HouseholdDoctrineResolver.Set(state, doctrine with { CapstoneUsedThisGeneration = true });

        var events = new List<IDomainEvent> { posted };

        events.AddRange(AdjustFavorCommands.Pipeline.Execute(
            state, new AdjustFavorCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.HouseholdId, DoctrineCatalog.GreatRiteFavorGain, "Domus Pia: The Great Rite")).Events);

        events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
            state, new AdjustDignitasCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.HouseholdId, DoctrineCatalog.GreatRiteDignitasGain, "Domus Pia: The Great Rite")).Events);

        events.Add(new GreatRitePerformedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, posted.TransactionId, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }
}
