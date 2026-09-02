using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Magistracies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §2's Locatio Censoria in its ad hoc form (§3: "a contract can still be issued or renewed outside the
/// Lustrum for a genuine, urgent need — a sudden campaign's supply crisis, an unplanned public work").
/// The Lustrum's own mandatory re-bid path is <see cref="LustrumSystem"/>'s job (it reopens an already-
/// awarded contract for bidding directly); this command is the other half — opening a brand-new contract
/// for the very first time, whether at a Lustrum or ad hoc, gated on §2's own real named power: the
/// executing Character must be a sitting <see cref="MagistracyOffice.Censor"/> at this settlement.
///
/// <b>Scope note:</b> §9's own "ad hoc contract frequency" and "how commonly Military &amp; Combat or a
/// sudden Natural Disaster should actually trigger one" are both left exactly as open as §9 states them —
/// this command is the real mechanism either trigger would call into once built; no Military &amp;
/// Combat campaign hook or Natural Disaster hook calls it automatically today (Military &amp; Combat is
/// Phase 16, confirmed unbuilt by direct search).
/// </summary>
public sealed record OpenPublicContractCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    PublicContractType Type,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Character> ExecutingCensorId) : ICommand;

public sealed record PublicContractOpenedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicContract> ContractId,
    PublicContractType ContractType,
    RuntimeId<Settlement> SettlementId,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.contractOpened";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ContractId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class OpenPublicContractCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("publicContracts.openContract.settlementNotFound");
    public static readonly ValidationErrorCode NoActiveCensorAtSettlement = new("publicContracts.openContract.noActiveCensorAtSettlement");

    public static readonly CommandPipeline<WorldState, OpenPublicContractCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, OpenPublicContractCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (MagistracyResolver.ActiveRecord(state, command.SettlementId, MagistracyOffice.Censor, command.ExecutingCensorId) is null)
            return NoActiveCensorAtSettlement;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, OpenPublicContractCommand command)
    {
        var contractId = state.PublicContractIds.Issue();
        var contract = new PublicContract(
            contractId, command.Type, command.SettlementId, PublicContractStatus.OpenForBidding,
            CurrentHolder: null, ContractValue: Money.Zero, OpenedDate: command.SubmittedDate, AwardedDate: null,
            AwardedViaLustrum: false, IsCuttingCorners: false, FraudDiscovered: false, FraudDiscoveryRisk: 0);
        state.PublicContracts.Add(contractId, contract);

        return new IDomainEvent[]
        {
            new PublicContractOpenedEvent(state.EventIds.Issue(), command.SubmittedDate, contractId, command.Type, command.SettlementId, command.CommandId.ToTaggedString()),
        };
    }
}
