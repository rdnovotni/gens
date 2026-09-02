using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §6.2's/§8's <c>ContractFraudRecord</c> data model (Phase 15 item 6) — one discovered instance of §6.1's
/// cutting corners. Created only once <see cref="ContractFraudDiscoverySystem"/> actually surfaces it;
/// <see cref="LegalCaseId"/> stays null until <see cref="FileRepetundaeCaseCommand"/> attaches a real
/// prosecution (only reachable when the holder resolves to a real household — see that command's own
/// doc comment), and <see cref="LegalOutcome"/>/<see cref="DisqualifiedFromBidding"/> stay at their
/// "not yet ruled" defaults until <see cref="RepetundaeResolutionHook"/> applies a verdict.
/// </summary>
/// <param name="DisqualifiedUntilDate">§6.2's "permanent or long-term disqualification" — null means
/// disqualified with no known end date (this item never sets that; see <see
/// cref="PublicContractsCatalog.DisqualificationMonths"/>), non-null names the real month it lifts. §9's
/// own "whether a disqualified bidder can ever petition for reinstatement... left open" — this item
/// builds no reinstatement command; the disqualification simply expires on its own once <see
/// cref="ContractBidderResolver.IsDisqualified"/> reads past this date.</param>
public sealed record ContractFraudRecord(
    RuntimeId<ContractFraudRecord> RecordId,
    RuntimeId<PublicContract> ContractId,
    ContractBidderRef Holder,
    GameDate DiscoveredDate,
    RuntimeId<Legal.LegalCase>? LegalCaseId,
    Legal.LegalCaseVerdict? LegalOutcome,
    bool DisqualifiedFromBidding,
    GameDate? DisqualifiedUntilDate);

/// <summary>§6.1's own quiet act — only the contract's current holder may declare it, and only against
/// an already-<see cref="PublicContractStatus.Awarded"/> contract.</summary>
public sealed record DeclareCuttingCornersCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicContract> ContractId) : ICommand;

public sealed record EndCuttingCornersCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicContract> ContractId) : ICommand;

public sealed record CuttingCornersDeclarationChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicContract> ContractId,
    bool IsCuttingCorners,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.cuttingCornersDeclarationChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ContractId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(ContractId.ToTaggedString());
}

public static class CuttingCornersDeclarationCommands
{
    public static readonly ValidationErrorCode ContractNotFound = new("publicContracts.cuttingCorners.contractNotFound");
    public static readonly ValidationErrorCode ContractNotAwarded = new("publicContracts.cuttingCorners.contractNotAwarded");
    public static readonly ValidationErrorCode AlreadyCuttingCorners = new("publicContracts.cuttingCorners.alreadyCuttingCorners");
    public static readonly ValidationErrorCode NotCuttingCorners = new("publicContracts.cuttingCorners.notCuttingCorners");

    public static readonly CommandPipeline<WorldState, DeclareCuttingCornersCommand> DeclarePipeline = new(
        validate: ValidateDeclare, mutate: MutateDeclare, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    public static readonly CommandPipeline<WorldState, EndCuttingCornersCommand> EndPipeline = new(
        validate: ValidateEnd, mutate: MutateEnd, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? ValidateDeclare(WorldState state, DeclareCuttingCornersCommand command)
    {
        if (!state.PublicContracts.TryGet(command.ContractId, out var contract))
            return ContractNotFound;
        if (contract!.Status != PublicContractStatus.Awarded)
            return ContractNotAwarded;
        if (contract.IsCuttingCorners)
            return AlreadyCuttingCorners;

        return null;
    }

    private static IDomainEvent[] MutateDeclare(WorldState state, DeclareCuttingCornersCommand command)
    {
        state.PublicContracts.TryGet(command.ContractId, out var contract);
        state.PublicContracts.Remove(command.ContractId);
        state.PublicContracts.Add(command.ContractId, contract! with { IsCuttingCorners = true, FraudDiscoveryRisk = 0 });

        return new IDomainEvent[]
        {
            new CuttingCornersDeclarationChangedEvent(state.EventIds.Issue(), command.SubmittedDate, command.ContractId, true, command.CommandId.ToTaggedString()),
        };
    }

    private static ValidationErrorCode? ValidateEnd(WorldState state, EndCuttingCornersCommand command)
    {
        if (!state.PublicContracts.TryGet(command.ContractId, out var contract))
            return ContractNotFound;
        if (!contract!.IsCuttingCorners)
            return NotCuttingCorners;

        return null;
    }

    private static IDomainEvent[] MutateEnd(WorldState state, EndCuttingCornersCommand command)
    {
        state.PublicContracts.TryGet(command.ContractId, out var contract);
        state.PublicContracts.Remove(command.ContractId);
        state.PublicContracts.Add(command.ContractId, contract! with { IsCuttingCorners = false });

        return new IDomainEvent[]
        {
            new CuttingCornersDeclarationChangedEvent(state.EventIds.Issue(), command.SubmittedDate, command.ContractId, false, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// §6.1's own real severity resolution (Phase 15 item 6), matching <see
/// cref="BusinessCompetition.GrainHoardingResolutionSystem"/>'s established static <c>Tick(state,
/// date)</c> convention. §6.1's own "resolves exactly like any other concealed action in this project's
/// shared Scheme engine (Characters §10)" is honored in spirit, not literally: <see
/// cref="Interactions.Scheme"/> is a Character-vs-Character wrapper (<see
/// cref="Interactions.Scheme.InitiatorCharacterId"/>/<see cref="Interactions.Scheme.TargetCharacterId"/>
/// are both required, non-optional Characters), and a contract holder can be a household, a Notable
/// Business, or a Societas — none of which is a Character, and "the state" is not a Character either, so
/// no real initiator/target pair exists for this act to construct a <see cref="Interactions.Scheme"/>
/// from. This system instead mirrors that engine's own progress/discovery-race shape directly (a real,
/// quiet margin gain each month, a Discovery risk that climbs 0-100 and resolves once it crosses a
/// threshold) using this domain's own record rather than a mismatched reuse of a Character-scoped type —
/// the same "reuse the shape, not force an ill-fitting type" judgment call this codebase's own precedent
/// already makes elsewhere (e.g. <see cref="BusinessCompetition.GrainHoardingRecord"/> itself does not
/// reuse <see cref="Interactions.Scheme"/> either).
/// </summary>
public static class ContractFraudDiscoverySystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.PublicContracts.InAscendingOrder().ToArray())
        {
            var contract = entry.Value;
            if (!contract.IsCuttingCorners || contract.FraudDiscovered || contract.Status != PublicContractStatus.Awarded)
                continue;

            if (contract.CurrentHolder is { } holder && TryResolveLedgerAccount(state, holder, out var holderAccount) &&
                contract.ContractValue > Money.Zero)
            {
                var gain = contract.ContractValue.Scale(PublicContractsCatalog.CuttingCornersMonthlyMarginGainFraction);
                if (gain > Money.Zero)
                {
                    events.Add(LedgerService.Post(
                        state, date, LedgerTransactionCategory.Contracts,
                        new[]
                        {
                            new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(contract.SettlementId), -gain),
                            new LedgerPosting(holderAccount, gain),
                        },
                        reference: $"publicContracts:cuttingCorners:{entry.Key.ToTaggedString()}"));
                }
            }

            var risk = Math.Min(contract.FraudDiscoveryRisk + PublicContractsCatalog.FraudDiscoveryRiskGainPerMonth, PublicContractsCatalog.FraudDiscoveryRiskThreshold);
            var discovered = risk >= PublicContractsCatalog.FraudDiscoveryRiskThreshold;

            state.PublicContracts.Remove(entry.Key);
            state.PublicContracts.Add(entry.Key, contract with { FraudDiscoveryRisk = risk, FraudDiscovered = discovered });

            if (!discovered)
                continue;

            var recordId = state.ContractFraudRecordIds.Issue();
            state.PublicContractFraudRecords.Add(
                recordId,
                new ContractFraudRecord(recordId, entry.Key, contract.CurrentHolder!.Value, date, LegalCaseId: null, LegalOutcome: null, DisqualifiedFromBidding: false, DisqualifiedUntilDate: null));

            events.Add(new ContractFraudDiscoveredEvent(state.EventIds.Issue(), date, recordId, entry.Key, CausationId: null));
        }

        return events;
    }

    private static bool TryResolveLedgerAccount(WorldState state, ContractBidderRef bidder, out LedgerAccountKey account)
    {
        if (ContractBidderResolver.TryResolveHousehold(state, bidder, out var householdId))
        {
            account = LedgerAccountKey.ForHousehold(householdId);
            return true;
        }

        if (bidder.Kind == ContractBidderKind.RivalHouse)
        {
            account = LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(bidder.BidderId));
            return true;
        }

        account = default;
        return false;
    }
}

public sealed record ContractFraudDiscoveredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ContractFraudRecord> RecordId,
    RuntimeId<PublicContract> ContractId,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.contractFraudDiscovered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { RecordId.ToTaggedString(), ContractId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
