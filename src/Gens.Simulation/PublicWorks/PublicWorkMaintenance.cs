using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

public sealed record PublicWorkUpkeepAssessedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicWork> PublicWorkId,
    bool Paid,
    Money Cost,
    int PreviousCondition,
    int NewCondition) : IDomainEvent
{
    public string Type => "publicWorks.upkeepAssessed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PublicWorkId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// §6's monthly maintenance tick (Phase 15 item 9) — a static, unwired <c>Tick(state, date)</c> helper
/// matching <see cref="PrivateInfrastructure.InfrastructureUpkeepSystem"/>'s identical shape exactly: for
/// every <see cref="PublicWork"/>, resolves this month's real upkeep (<see
/// cref="PublicWorksCatalog.MonthlyUpkeep"/>) against whoever is actually responsible for paying it — a
/// <see cref="PropertyOwnerKind.PlayerHousehold"/> patron's own Ledger account, or the settlement's own
/// Treasury for a <see cref="PublicWorkFundingSource.StateTaxRevenue"/> work — and either pays it (resets
/// <see cref="PublicWork.ConsecutiveNeglectedMonths"/> to zero) or costs the work <see
/// cref="PublicWorksCatalog.UnpaidUpkeepConditionLoss"/> condition points and advances that neglect
/// streak (§6's "the honest, harder half of euergetism's own real obligation that a single triumphant
/// dedication ceremony doesn't fully discharge"). A work whose patron is a <see
/// cref="PropertyOwnerKind.RivalGens"/> or a Societas has no real tracked balance this codebase can debit
/// (the same honest narrowing <see cref="FundPublicWorkCommands"/> already applies) and is always read as
/// unpaid — its own upkeep genuinely does lapse over time unless the patron's household later funds a
/// real <see cref="FundPublicWorkUpkeepCommand"/> top-up, which any real payer can submit regardless of
/// who originally funded construction.
/// </summary>
public static class PublicWorksMaintenanceSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.PublicWorks.InAscendingOrder().ToArray())
        {
            var work = entry.Value;
            var upkeep = PublicWorksCatalog.MonthlyUpkeep(work.WorkType);
            var account = PayingAccount(work);
            var paid = account is { } realAccount &&
                state.LedgerAccounts.TryGet(realAccount, out var ledgerAccount) && ledgerAccount!.Balance.RawValue >= upkeep.RawValue;

            if (paid)
            {
                events.Add(LedgerService.Post(
                    state, date, LedgerTransactionCategory.Upkeep,
                    new[] { new LedgerPosting(account!.Value, -upkeep), new LedgerPosting(LedgerAccountKey.Mint, upkeep) },
                    reference: $"publicWorks.upkeep:{work.Id.ToTaggedString()}"));
            }

            var newCondition = paid ? work.Condition : Math.Max(0, work.Condition - PublicWorksCatalog.UnpaidUpkeepConditionLoss);
            var newStreak = paid ? 0 : work.ConsecutiveNeglectedMonths + 1;

            PublicWorkResolver.Set(state, work with { Condition = newCondition, ConsecutiveNeglectedMonths = newStreak });
            events.Add(new PublicWorkUpkeepAssessedEvent(state.EventIds.Issue(), date, work.Id, paid, upkeep, work.Condition, newCondition));
        }

        return events;
    }

    /// <summary>The one real Ledger account actually responsible for a work's upkeep — null for a
    /// funding patron this codebase cannot track a real balance for (RivalGens, Societas), which reads as
    /// permanently unpaid until a real household steps in through <see cref="FundPublicWorkUpkeepCommand"/>.</summary>
    private static LedgerAccountKey? PayingAccount(PublicWork work) => work.FundingSource switch
    {
        PublicWorkFundingSource.StateTaxRevenue => LedgerAccountKey.ForSettlementTreasury(work.SettlementId),
        PublicWorkFundingSource.PrivateEuergetism when work.FundingPatronId is { Kind: PropertyOwnerKind.PlayerHousehold } patron =>
            LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(patron.OwnerId!)),
        _ => null,
    };
}

public sealed record PublicWorkUpkeepFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicWork> PublicWorkId,
    int PreviousCondition,
    int NewCondition,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicWorks.upkeepFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PublicWorkId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§6's "recoverable through the same Repair action" — a real, funded restoration of one
/// work's Condition and neglect streak, mirroring <see
/// cref="PrivateInfrastructure.RepairInfrastructureCommand"/>'s identical shape. Any household actually
/// paying (not necessarily the work's own recorded patron) may submit this — a rescue contribution from a
/// third party is a real, plausible euergetism act in its own right, and this item does not gate it to
/// the original funder.</summary>
public sealed record FundPublicWorkUpkeepCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicWork> PublicWorkId,
    RuntimeId<Household> PayingHouseholdId) : ICommand;

public static class FundPublicWorkUpkeepCommands
{
    public static readonly ValidationErrorCode PublicWorkNotFound = new("publicWorks.fundUpkeep.publicWorkNotFound");
    public static readonly ValidationErrorCode AlreadyPristine = new("publicWorks.fundUpkeep.alreadyPristine");
    public static readonly ValidationErrorCode InsufficientFunds = new("publicWorks.fundUpkeep.insufficientFunds");

    public static readonly CommandPipeline<WorldState, FundPublicWorkUpkeepCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundPublicWorkUpkeepCommand command)
    {
        if (!state.PublicWorks.TryGet(command.PublicWorkId, out var work))
            return PublicWorkNotFound;
        if (work!.Condition >= PublicWorksCatalog.PristineCondition)
            return AlreadyPristine;

        var pointsRestored = Math.Min(PublicWorksCatalog.RepairConditionRestored, PublicWorksCatalog.PristineCondition - work.Condition);
        var cost = PublicWorksCatalog.RepairCostPerConditionPoint.Scale(Numerics.Fixed64.FromInt(pointsRestored));
        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.PayingHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance.RawValue < cost.RawValue)
            return InsufficientFunds;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundPublicWorkUpkeepCommand command)
    {
        state.PublicWorks.TryGet(command.PublicWorkId, out var work);
        var pointsRestored = Math.Min(PublicWorksCatalog.RepairConditionRestored, PublicWorksCatalog.PristineCondition - work!.Condition);
        var cost = PublicWorksCatalog.RepairCostPerConditionPoint.Scale(Numerics.Fixed64.FromInt(pointsRestored));
        var newCondition = Math.Min(PublicWorksCatalog.PristineCondition, work.Condition + pointsRestored);

        var events = new List<IDomainEvent>();
        events.Add(LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Upkeep,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.PayingHouseholdId), -cost),
                new LedgerPosting(LedgerAccountKey.Mint, cost),
            },
            reference: $"publicWorks.fundUpkeep:{work.Id.ToTaggedString()}"));

        PublicWorkResolver.Set(state, work with { Condition = newCondition, ConsecutiveNeglectedMonths = 0 });
        events.Add(new PublicWorkUpkeepFundedEvent(
            state.EventIds.Issue(), command.SubmittedDate, work.Id, work.Condition, newCondition, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}

/// <summary>
/// §6's "in a severe case of visible neglect, risks a real Scandal" — reveals an already-true ground
/// state (<see cref="PublicWork.Condition"/> below <see
/// cref="PublicWorksCatalog.SevereNeglectConditionThreshold"/> for at least <see
/// cref="PublicWorksCatalog.SevereNeglectConsecutiveMonths"/> consecutive unpaid months) rather than
/// rolling anything itself, mirroring <see cref="RealEstate.AuditPropertyOperatorCommand"/>'s and <see
/// cref="DiscoverFabricationCommand"/>'s own identical "reveal, don't re-validate" shape. Only reachable
/// for a work whose patron resolves to a real <see cref="PropertyOwnerKind.PlayerHousehold"/> — <see
/// cref="Scandal.RecordScandalCommand"/> is itself household-scoped, so a RivalGens/Societas-patronized or
/// State-funded work's own neglect (there being no individual reputation to burn for the latter, per §7's
/// own "no individual patron's name") is honestly never a reachable Scandal source through this
/// command.
/// </summary>
public sealed record RecordEuergetismNeglectScandalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicWork> PublicWorkId) : ICommand;

public static class RecordEuergetismNeglectScandalCommands
{
    public static readonly ValidationErrorCode PublicWorkNotFound = new("publicWorks.recordNeglectScandal.publicWorkNotFound");
    public static readonly ValidationErrorCode NotSeverelyNeglected = new("publicWorks.recordNeglectScandal.notSeverelyNeglected");
    public static readonly ValidationErrorCode NoResolvableHouseholdPatron = new("publicWorks.recordNeglectScandal.noResolvableHouseholdPatron");

    public static readonly CommandPipeline<WorldState, RecordEuergetismNeglectScandalCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordEuergetismNeglectScandalCommand command)
    {
        if (!state.PublicWorks.TryGet(command.PublicWorkId, out var work))
            return PublicWorkNotFound;
        if (work!.Condition >= PublicWorksCatalog.SevereNeglectConditionThreshold ||
            work.ConsecutiveNeglectedMonths < PublicWorksCatalog.SevereNeglectConsecutiveMonths)
        {
            return NotSeverelyNeglected;
        }

        if (work.FundingPatronId is not { Kind: PropertyOwnerKind.PlayerHousehold })
            return NoResolvableHouseholdPatron;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordEuergetismNeglectScandalCommand command)
    {
        state.PublicWorks.TryGet(command.PublicWorkId, out var work);
        var householdId = RuntimeId<Household>.Parse(work!.FundingPatronId!.Value.OwnerId!);

        return RecordScandalCommands.Pipeline.Execute(
            state, new RecordScandalCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                householdId, ScandalSourceType.PublicWorksNeglect, ScandalSeverity.PublicDisgrace)).Events.ToArray();
    }
}
