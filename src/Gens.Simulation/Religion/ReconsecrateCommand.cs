using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Religion;

/// <summary>
/// Changes an already-chosen Patron Deity (Phase 12 item 3; §2.1's "Reconsecration"). §2.1 names two
/// real triggers — "whenever a new paterfamilias or materfamilias assumes headship" or "following a
/// major Chronicle-worthy event plausibly attributed to divine intervention" — and this command builds
/// only the first: <see cref="Succession.HouseholdHeadship"/> (Phase 11 item 1) is a real, checkable
/// hook (its own <c>HeadCharacterId</c> is exactly "who currently heads the household," compared here
/// against <see cref="HouseholdReligion.ConsecratedUnderHeadCharacterId"/>), matching this project's own
/// "build against a real, checkable hook, defer what has no clean one" discipline. The Chronicle-event
/// trigger has no such hook: nothing in <c>Gens.Simulation.Chronicle</c> classifies an entry as
/// "plausibly attributed to divine intervention" (that would need a per-entry, content-or-rules
/// judgment this item does not have a principled way to make), so that second trigger is not built —
/// matching <see cref="Magistracies.MagistracyOffice"/>'s own "omitted rather than included-but-
/// unreachable" precedent for a design-doc trigger with no real dependency to check yet.
///
/// Per §2.1, Reconsecration "is a Funded Action — a real ceremony, not a menu toggle... and resets
/// accumulated Favor toward a neutral middle rather than preserving it outright." This command debits
/// <see cref="ReligionCatalog.ReconsecrationCeremonyCost"/> from the household's ledger (mirroring <see
/// cref="Policies.FundFestivalCommand"/>'s own ledger-spend shape, into this domain's own named sink)
/// and resets <see cref="HouseholdReligion.Favor"/> to zero — the same "neutral middle" every other
/// standing meter in this codebase treats zero as (<see cref="Reputation.HouseholdReputation"/>'s own
/// "no entry means zero" default).
///
/// <b>Scope note:</b> §2.1's own <c>reconsecrationHistory</c> data-model field (a list of past
/// from/to/trigger/month entries) is not built — this command overwrites <see
/// cref="HouseholdReligion.ConsecratedUnderHeadCharacterId"/> and <see cref="HouseholdReligion.PatronDeity"/>
/// in place rather than appending to a kept history, matching <see cref="Economy.NetWorthAssessments"/>'s
/// own "sparse and overwritten each month" convention for a latest-snapshot read model rather than an
/// accumulating log; the emitted <see cref="ReconsecrationEvent"/> below is itself the auditable record
/// of each change, readable back the same way any other domain event already is, without this item
/// needing to duplicate that history into a second, bespoke list.
/// </summary>
public sealed record ReconsecrateCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    PatronDeity NewDeity) : ICommand;

/// <summary>Emitted whenever a <see cref="ReconsecrateCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> the ceremony's own spend produces. Public, matching <see
/// cref="PatronDeitySetEvent"/>.</summary>
public sealed record ReconsecrationEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    PatronDeity PreviousDeity,
    PatronDeity NewDeity,
    RuntimeId<Characters.Character> NewHeadCharacterId,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "religion.reconsecrated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ReconsecrateCommand"/> (ADR 0006).</summary>
public static class ReconsecrateCommands
{
    public static readonly ValidationErrorCode NoPatronDeityYet = new("religion.reconsecrate.noPatronDeityYet");
    public static readonly ValidationErrorCode SameDeity = new("religion.reconsecrate.sameDeity");
    public static readonly ValidationErrorCode NoHeadshipRecorded = new("religion.reconsecrate.noHeadshipRecorded");
    public static readonly ValidationErrorCode NotANewHeadship = new("religion.reconsecrate.notANewHeadship");
    public static readonly ValidationErrorCode InsufficientTreasury = new("religion.reconsecrate.insufficientTreasury");

    private static readonly LedgerAccountKey ReconsecrationSink = new(LedgerAccountKind.System, "religion:reconsecration");

    public static readonly CommandPipeline<WorldState, ReconsecrateCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ReconsecrateCommand command)
    {
        if (!state.HouseholdReligions.TryGet(command.HouseholdId, out var religion))
            return NoPatronDeityYet;
        if (religion!.PatronDeity == command.NewDeity)
            return SameDeity;
        if (!state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship))
            return NoHeadshipRecorded;
        if (headship!.HeadCharacterId == religion.ConsecratedUnderHeadCharacterId)
            return NotANewHeadship;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < ReligionCatalog.ReconsecrationCeremonyCost)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ReconsecrateCommand command)
    {
        state.HouseholdReligions.TryGet(command.HouseholdId, out var religion);
        state.HouseholdHeadships.TryGet(command.HouseholdId, out var headship);
        var newHeadId = headship!.HeadCharacterId;

        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -ReligionCatalog.ReconsecrationCeremonyCost),
                new LedgerPosting(ReconsecrationSink, ReligionCatalog.ReconsecrationCeremonyCost),
            },
            reference: $"religion:reconsecration:{command.CommandId.ToTaggedString()}");

        state.HouseholdReligions.Remove(command.HouseholdId);
        state.HouseholdReligions.Add(
            command.HouseholdId,
            new HouseholdReligion(command.HouseholdId, command.NewDeity, Favor: 0, newHeadId));

        return new IDomainEvent[]
        {
            posted,
            new ReconsecrationEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, religion!.PatronDeity, command.NewDeity,
                newHeadId, posted.TransactionId, command.CommandId.ToTaggedString()),
        };
    }
}
