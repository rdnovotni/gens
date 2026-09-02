using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Societates;

/// <summary>
/// §3's <c>UnlimitedLiabilityEvent</c> (Phase 15 item 2) — "each partner's own personal fortune... in
/// principle without limit" made real: a real Ledger debit against one exposed partner's own household
/// account, sized from the linked venture's own tracked Value where one resolves (<see
/// cref="Societas.LinkedPropertySubject"/>), scaled further by <see
/// cref="SocietatesCatalog.OmniumBonorumLiabilityMultiplier"/> for a Societas Omnium Bonorum
/// specifically (§3's own worked "contained loss... genuine, complete ruin" contrast). Deliberately
/// does <b>not</b> write <see cref="Economy.InsolvencyState"/> directly — that partition has exactly
/// one real writer, <see cref="Economy.InsolvencySystem"/>'s own monthly Net Worth tick — so this
/// command only depresses the exposed household's Treasury balance the same way any other real expense
/// would, letting that already-shipped monthly system organically detect the resulting Net Worth
/// collapse and escalate <see cref="Economy.InsolvencyStage"/> on its own normal schedule (§3's cross-
/// integration: "a household can now go Insolvent because of a partner's own failure, not only its
/// own... reachable through a partner's own failure" — reachable, not force-set). Only meaningful for a
/// partner this item can resolve a real Ledger account for (<see
/// cref="RealEstate.OperatorLifecycleSystem"/>'s own identical <c>TryOwnerLedgerAccount</c> owner-kind
/// roster) — every other partner kind settles the call against <see cref="LedgerAccountKey.Mint"/>
/// instead, matching <see cref="RealEstate.TransferPropertyCommand"/>'s own "the explicit, named
/// conservation boundary" precedent for an owner kind this item cannot yet track a real balance for.
/// </summary>
public sealed record TriggerUnlimitedLiabilityCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef ExposedPartnerOwner,
    bool TriggeringPartnerFailure) : ICommand;

public sealed record UnlimitedLiabilityEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Societas> SocietasId,
    PropertyOwnerRef ExposedPartnerOwner,
    bool TriggeringPartnerFailure,
    Money AmountExposed,
    string? CausationId) : IDomainEvent
{
    public string Type => "societates.unlimitedLiabilityTriggered";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SocietasId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class TriggerUnlimitedLiabilityCommands
{
    public static readonly ValidationErrorCode SocietasNotFound = new("societates.unlimitedLiability.societasNotFound");
    public static readonly ValidationErrorCode PartnerNotFound = new("societates.unlimitedLiability.partnerNotFound");

    public static readonly CommandPipeline<WorldState, TriggerUnlimitedLiabilityCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, TriggerUnlimitedLiabilityCommand command)
    {
        if (!state.Societates.TryGet(command.SocietasId, out var societas))
            return SocietasNotFound;
        if (!SocietasResolver.IsPartner(societas!, command.ExposedPartnerOwner))
            return PartnerNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, TriggerUnlimitedLiabilityCommand command)
    {
        state.Societates.TryGet(command.SocietasId, out var societas);
        var amount = ComputeExposure(state, societas!);

        var events = new List<IDomainEvent>();
        if (amount != Money.Zero)
        {
            var exposedKey = TryOwnerLedgerAccount(command.ExposedPartnerOwner, out var resolved) ? resolved : LedgerAccountKey.Mint;
            if (exposedKey != LedgerAccountKey.Mint)
            {
                events.Add(LedgerService.Post(
                    state, command.SubmittedDate, LedgerTransactionCategory.Debt,
                    new[] { new LedgerPosting(exposedKey, -amount), new LedgerPosting(LedgerAccountKey.Mint, amount) },
                    reference: $"societates.unlimitedLiability:{command.SocietasId.ToTaggedString()}"));
            }
        }

        events.Add(new UnlimitedLiabilityEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.SocietasId, command.ExposedPartnerOwner,
            command.TriggeringPartnerFailure, amount, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    /// <summary>§3's own invented sizing (§11's "unlimited-liability exposure curves... unsized"): the
    /// linked venture's own tracked Value, scaled down to <see
    /// cref="SocietatesCatalog.LinkedAssetLiabilityFraction"/> (and, for a Societas Omnium Bonorum,
    /// scaled back up by <see cref="SocietatesCatalog.OmniumBonorumLiabilityMultiplier"/>) when one
    /// resolves; <see cref="SocietatesCatalog.BaseUnlimitedLiabilityAmount"/> (Omnium Bonorum-scaled
    /// the same way) otherwise.</summary>
    public static Money ComputeExposure(WorldState state, Societas societas)
    {
        var isOmniumBonorum = societas.PartnershipType == PartnershipType.OmniumBonorum;

        if (societas.LinkedPropertySubject is { } subject && PropertyResolver.TryResolve(state, subject, out var view))
        {
            var scaled = view.Value.Scale(SocietatesCatalog.LinkedAssetLiabilityFraction);
            return isOmniumBonorum ? scaled.Scale(SocietatesCatalog.OmniumBonorumLiabilityMultiplier) : scaled;
        }

        return isOmniumBonorum
            ? SocietatesCatalog.BaseUnlimitedLiabilityAmount.Scale(SocietatesCatalog.OmniumBonorumLiabilityMultiplier)
            : SocietatesCatalog.BaseUnlimitedLiabilityAmount;
    }

    private static bool TryOwnerLedgerAccount(PropertyOwnerRef owner, out LedgerAccountKey key)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                key = LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.RivalGens:
            case PropertyOwnerKind.Collegium:
                key = LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.Municipal:
                key = LedgerAccountKey.ForSettlementTreasury(RuntimeId<Settlement>.Parse(owner.OwnerId!));
                return true;
            default:
                key = default;
                return false;
        }
    }
}
