using Gens.Simulation.Actors;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Land;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§7's Government Contracts (Phase 15 item 4) — "a lighter-weight, municipal-scale extension
/// of Land Ownership &amp; Real Estate's own Publicanus Contract concept... rather than a parallel
/// system." §7's own two concrete destinations named for this — Settlement Demographics' Grain Dole and
/// Policies &amp; Edicts' Grain Dole Funded Action — are both confirmed unbuilt anywhere in this codebase
/// by direct search (no "grain dole" concept exists in <c>Gens.Simulation.Characters</c> or <c>Gens.
/// Simulation.Policies</c>), so this item does not literally tie a contract to either; it builds the
/// real, standing municipal supply relationship itself — steady income, a real civic obligation — as a
/// building block those two still-unbuilt systems (or a future item) can point at once they exist. At
/// most one active contract per business, per §10's own nullable <c>activeGovernmentContractId</c>;
/// this item omits that redundant ID field entirely and reads presence in <see
/// cref="WorldState.NotableBusinessGovernmentContracts"/> instead, matching <see
/// cref="RealEstate.PlotPropertyExtension"/>'s own "wrap the pointer in a parallel sparse partition"
/// convention.</summary>
public sealed record NotableBusinessGovernmentContract(
    RuntimeId<NotableBusiness> BusinessId, RuntimeId<Settlement> SettlementId, Money MonthlyStipend, GameDate StartDate);

/// <summary>§7's grant — also, per §3's own "holds a real government contract" trigger, re-promotes a
/// currently-<see cref="NotableBusinessStatus.Demoted"/> business back to <see
/// cref="NotableBusinessStatus.Tracked"/> in the same step.</summary>
public sealed record GrantGovernmentContractCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<Settlement> SettlementId,
    Money? MonthlyStipend = null) : ICommand;

public sealed record GovernmentContractGrantedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<Settlement> SettlementId,
    Money MonthlyStipend,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.governmentContractGranted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class GrantGovernmentContractCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.grantContract.businessNotFound");
    public static readonly ValidationErrorCode SettlementNotFound = new("notableBusinesses.grantContract.settlementNotFound");
    public static readonly ValidationErrorCode AlreadyHasContract = new("notableBusinesses.grantContract.alreadyHasContract");

    public static readonly CommandPipeline<WorldState, GrantGovernmentContractCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, GrantGovernmentContractCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out _))
            return BusinessNotFound;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (state.NotableBusinessGovernmentContracts.TryGet(command.BusinessId, out _))
            return AlreadyHasContract;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, GrantGovernmentContractCommand command)
    {
        NotableBusinessTieringService.RecordContactAndPromote(state, command.BusinessId, command.SubmittedDate);

        var stipend = command.MonthlyStipend ?? NotableBusinessesCatalog.GovernmentContractDefaultMonthlyStipend;
        state.NotableBusinessGovernmentContracts.Add(
            command.BusinessId, new NotableBusinessGovernmentContract(command.BusinessId, command.SettlementId, stipend, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new GovernmentContractGrantedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.SettlementId, stipend,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>§7's own ending half — a clean expiry/non-renewal versus <see
/// cref="FailedToDeliver"/>'s own real consequence: "a genuine obligation the business can't simply walk
/// away from without real Reputation... consequences if it fails to deliver."</summary>
public sealed record EndGovernmentContractCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    bool FailedToDeliver) : ICommand;

public sealed record GovernmentContractEndedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    bool FailedToDeliver,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.governmentContractEnded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class EndGovernmentContractCommands
{
    public static readonly ValidationErrorCode NoActiveContract = new("notableBusinesses.endContract.noActiveContract");

    public static readonly CommandPipeline<WorldState, EndGovernmentContractCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EndGovernmentContractCommand command) =>
        state.NotableBusinessGovernmentContracts.TryGet(command.BusinessId, out _) ? null : NoActiveContract;

    private static IDomainEvent[] Mutate(WorldState state, EndGovernmentContractCommand command)
    {
        var events = new List<IDomainEvent>();
        state.NotableBusinessGovernmentContracts.Remove(command.BusinessId);

        if (command.FailedToDeliver)
        {
            events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
                state, new AdjustBusinessReputationCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.BusinessId, -NotableBusinessesCatalog.ContractFailureReputationLoss, BusinessReputationChangeReason.SupplyFailure)).Events);
        }

        events.Add(new GovernmentContractEndedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.FailedToDeliver, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}

/// <summary>§7's "carrying real, steady income" — the monthly posting half, mirroring <see
/// cref="Economy.RentAndTaxSystem"/>'s own identical "walk every active record, post through the
/// Ledger" shape. Posts Settlement Treasury → the business owner's own resolvable Ledger account: a
/// <see cref="PropertyOwnerKind.PlayerHousehold"/> or <see cref="PropertyOwnerKind.RivalGens"/> owner
/// resolves to a real <see cref="LedgerAccountKey"/>; a <see
/// cref="PropertyOwnerKind.IndividualCharacter"/> owner has no dedicated Ledger account kind anywhere in
/// this codebase (only <see cref="LedgerAccountKind.Actor"/>, itself a Companion/staff concept, not an
/// arbitrary Character), so it routes through <see cref="LedgerAccountKey.Mint"/> instead, matching <see
/// cref="RealEstate.TransferPropertyCommand"/>'s own identical "route an owner kind this item cannot yet
/// track a real balance for through the Mint" precedent. A <see
/// cref="NotableBusinessStatus.Demoted"/> business's contract still pays (§3's own "no longer given
/// extra simulation fidelity" applies to Reputation drift and disruption checks, not to an already-real,
/// standing financial obligation).</summary>
public static class GovernmentContractPaymentSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.NotableBusinessGovernmentContracts.InAscendingOrder().ToArray())
        {
            if (!state.NotableBusinesses.TryGet(entry.Key, out var business))
                continue;

            var ownerAccount = ResolveOwnerAccount(business!.Owner);
            var posted = LedgerService.Post(
                state, date, LedgerTransactionCategory.Contracts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(entry.Value.SettlementId), -entry.Value.MonthlyStipend),
                    new LedgerPosting(ownerAccount, entry.Value.MonthlyStipend),
                },
                reference: $"governmentContract:{entry.Key.ToTaggedString()}");
            events.Add(posted);
        }

        return events;
    }

    private static LedgerAccountKey ResolveOwnerAccount(PropertyOwnerRef owner) => owner.Kind switch
    {
        PropertyOwnerKind.PlayerHousehold => LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(owner.OwnerId!)),
        PropertyOwnerKind.RivalGens => LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(owner.OwnerId!)),
        _ => LedgerAccountKey.Mint,
    };
}
