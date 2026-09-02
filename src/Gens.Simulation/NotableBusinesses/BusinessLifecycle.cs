using Gens.Simulation.Actors;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Goods;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>
/// §8's ten named Business Lifecycle behaviors (Phase 15 item 4). Six are "already real, reused
/// directly," per §8's own text, and this item builds no code for them at all — <b>Expand</b> is Land
/// Ownership &amp; Real Estate's own property-development mechanics; <b>Go bankrupt</b> is <see
/// cref="Economy.InsolvencySystem"/>; <b>Change owners</b> and <b>Form partnerships</b> are <see
/// cref="RealEstate.TransferPropertyCommand"/>/<see cref="Societates.FormSocietasCommand"/>; <b>Inherit</b>
/// is Notable Households' own inheritance logic per §8's own text — genuinely unbuilt, since no <c>Notable
/// Households</c> domain exists anywhere in this codebase (confirmed by direct search), a real, named
/// gap this item does not paper over with a substitute; <b>Raise/lower prices</b> is Economy &amp;
/// Finance's own Market Dynamics; <b>Compete for contracts</b> is an ordinary <see
/// cref="GrantGovernmentContractCommand"/> call once a caller has already resolved which of several
/// bidders wins (this item builds no bidding/auction resolution of its own — Public Contracts,
/// Competitive Bidding is this phase's own later item). This type only names <see
/// cref="BusinessLifecycleEventType"/>'s four genuinely new values, since only those four actually need
/// new mechanism — the six reused ones have no dedicated enum value here at all (each is already fully
/// legible through its own real command's own event stream, and repeating them here would just relabel
/// an existing event rather than add anything).
/// </summary>
public enum BusinessLifecycleEventType
{
    Merge,
    Specialize,
    Move,
    LobbyGovernment,
}

/// <summary>§8's Merge — "two Notable Businesses combining into one... a real, new resolution path
/// distinct from an ordinary forced sale." <paramref name="AbsorbedBusinessId"/> is demoted (its own
/// record is kept, matching <see cref="NotableBusinessStatus.Demoted"/>'s own "nothing... is deleted"
/// framing) rather than removed outright, so a later reader (Chronicle, audit) can still resolve what it
/// once was; this command does not sweep every other business's own <see
/// cref="NotableBusiness.MainCompetitorBusinessId"/> for a now-stale pointer at the absorbed business —
/// an honest, minor limitation rather than an exhaustive graph-consistency pass this item does not build.</summary>
public sealed record MergeNotableBusinessesCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> SurvivingBusinessId,
    RuntimeId<NotableBusiness> AbsorbedBusinessId) : ICommand;

public sealed record BusinessesMergedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> SurvivingBusinessId,
    RuntimeId<NotableBusiness> AbsorbedBusinessId,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.businessesMerged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SurvivingBusinessId.ToTaggedString(), AbsorbedBusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class MergeNotableBusinessesCommands
{
    public static readonly ValidationErrorCode SurvivingBusinessNotFound = new("notableBusinesses.merge.survivingBusinessNotFound");
    public static readonly ValidationErrorCode AbsorbedBusinessNotFound = new("notableBusinesses.merge.absorbedBusinessNotFound");
    public static readonly ValidationErrorCode SelfMerge = new("notableBusinesses.merge.selfMerge");

    public static readonly CommandPipeline<WorldState, MergeNotableBusinessesCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, MergeNotableBusinessesCommand command)
    {
        if (command.SurvivingBusinessId == command.AbsorbedBusinessId)
            return SelfMerge;
        if (!state.NotableBusinesses.TryGet(command.SurvivingBusinessId, out _))
            return SurvivingBusinessNotFound;
        if (!state.NotableBusinesses.TryGet(command.AbsorbedBusinessId, out _))
            return AbsorbedBusinessNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, MergeNotableBusinessesCommand command)
    {
        state.NotableBusinesses.TryGet(command.SurvivingBusinessId, out var surviving);
        state.NotableBusinesses.TryGet(command.AbsorbedBusinessId, out var absorbed);

        // This item's own reasoned sizing for the merged Reputation: an unweighted average of the two
        // — §11's own "all numeric sizing... unsized" leaves the exact blend open, and a straight
        // average is the simplest reading that credits neither business's own standing over the
        // other's.
        var mergedReputation = (surviving!.Reputation + absorbed!.Reputation) / 2;
        var updatedSurviving = surviving with
        {
            Reputation = mergedReputation,
            MainCompetitorBusinessId = surviving.MainCompetitorBusinessId == command.AbsorbedBusinessId ? null : surviving.MainCompetitorBusinessId,
        };
        state.NotableBusinesses.Remove(command.SurvivingBusinessId);
        state.NotableBusinesses.Add(command.SurvivingBusinessId, updatedSurviving);

        var updatedAbsorbed = absorbed with { Status = NotableBusinessStatus.Demoted, MainCompetitorBusinessId = null };
        state.NotableBusinesses.Remove(command.AbsorbedBusinessId);
        state.NotableBusinesses.Add(command.AbsorbedBusinessId, updatedAbsorbed);

        return new IDomainEvent[]
        {
            new BusinessesMergedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.SurvivingBusinessId, command.AbsorbedBusinessId,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>§8's Specialize — "narrowing its own Output to a single high-quality good... trading
/// Reputation-building potential and a real Quality premium for reduced resilience." Narrows <see
/// cref="NotableBusiness.OutputGoodId"/> to match, per that same sentence ("narrowing its own Output"),
/// and applies the one real, immediate half of that trade this item can size — <see
/// cref="NotableBusinessesCatalog.SpecializeReputationBonus"/> — via <see
/// cref="AdjustBusinessReputationCommand"/>. The "reduced resilience" half is not a separate mechanic
/// this item adds: it falls out naturally of <see cref="SupplierDisruptionSystem"/> already reading
/// this narrower single-good <see cref="NotableBusiness.MainSupplier"/> dependency, exactly as it did
/// before specializing — a specialized business simply has nowhere else for its Output to come from if
/// that one supplier fails.</summary>
public sealed record SpecializeNotableBusinessCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    DefinitionId<Good> SpecializedGoodId) : ICommand;

public sealed record BusinessSpecializedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    DefinitionId<Good> SpecializedGoodId,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.businessSpecialized";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SpecializeNotableBusinessCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.specialize.businessNotFound");
    public static readonly ValidationErrorCode AlreadySpecialized = new("notableBusinesses.specialize.alreadySpecialized");

    public static readonly CommandPipeline<WorldState, SpecializeNotableBusinessCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SpecializeNotableBusinessCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business))
            return BusinessNotFound;
        if (business!.IsSpecialized)
            return AlreadySpecialized;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SpecializeNotableBusinessCommand command)
    {
        var events = new List<IDomainEvent>();
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);
        state.NotableBusinesses.Remove(command.BusinessId);
        state.NotableBusinesses.Add(
            command.BusinessId,
            business! with { IsSpecialized = true, SpecializedGoodId = command.SpecializedGoodId, OutputGoodId = command.SpecializedGoodId });

        events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.BusinessId, NotableBusinessesCatalog.SpecializeReputationBonus, BusinessReputationChangeReason.QualityOutput)).Events);

        events.Add(new BusinessSpecializedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.SpecializedGoodId, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}

/// <summary>§8's Move — "relocating to a different District... trading a real, one-time cost against a
/// different District's own Property Value trend and customer base." This item builds the real
/// relocation and its real cost; reading the destination District's own Property Value trend to decide
/// <i>whether</i> to move is left to whichever caller (player or NPC decision layer) invokes this
/// command — the same "this command reveals/records, it does not decide for the caller" scoping <see
/// cref="RealEstate.TransferPropertyCommand"/> already gives its own upstream trigger.</summary>
public sealed record MoveNotableBusinessCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<District> NewDistrictId) : ICommand;

public sealed record BusinessMovedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<District>? PreviousDistrictId,
    RuntimeId<District> NewDistrictId,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.businessMoved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString(), NewDistrictId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class MoveNotableBusinessCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.move.businessNotFound");
    public static readonly ValidationErrorCode DistrictNotFound = new("notableBusinesses.move.districtNotFound");
    public static readonly ValidationErrorCode AlreadyInDistrict = new("notableBusinesses.move.alreadyInDistrict");

    public static readonly CommandPipeline<WorldState, MoveNotableBusinessCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, MoveNotableBusinessCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business))
            return BusinessNotFound;
        if (!state.Districts.TryGet(command.NewDistrictId, out _))
            return DistrictNotFound;
        if (business!.DistrictId == command.NewDistrictId)
            return AlreadyInDistrict;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, MoveNotableBusinessCommand command)
    {
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);
        var previousDistrictId = business!.DistrictId;

        state.NotableBusinesses.Remove(command.BusinessId);
        state.NotableBusinesses.Add(command.BusinessId, business with { DistrictId = command.NewDistrictId });

        // §8's "a real, one-time cost" — posted only when the owner resolves to a real, trackable
        // Ledger account (matching GovernmentContractPaymentSystem's own identical narrowing); an
        // IndividualCharacter owner still relocates, it simply carries no real Ledger cost this item
        // can post against.
        if (TryResolveOwnerAccount(business.Owner, out var ownerAccount))
        {
            LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Transfers,
                new[]
                {
                    new LedgerPosting(ownerAccount, -NotableBusinessesCatalog.MoveRelocationCost),
                    new LedgerPosting(LedgerAccountKey.Mint, NotableBusinessesCatalog.MoveRelocationCost),
                },
                reference: $"notableBusinessMove:{command.BusinessId.ToTaggedString()}");
        }

        return new IDomainEvent[]
        {
            new BusinessMovedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, previousDistrictId, command.NewDistrictId,
                command.CommandId.ToTaggedString()),
        };
    }

    private static bool TryResolveOwnerAccount(PropertyOwnerRef owner, out LedgerAccountKey account)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                account = LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.RivalGens:
                account = LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(owner.OwnerId!));
                return true;
            default:
                account = default;
                return false;
        }
    }
}

/// <summary>§8's Lobby — "a lighter-weight political action... a Notable Business spending Influence or
/// a direct payment specifically to win or renew a government contract." On acceptance, grants (or, if
/// one is already active, first ends and then re-grants — §8's own "renew") a <see
/// cref="NotableBusinessGovernmentContract"/> for <paramref name="SettlementId"/>. §8's second named
/// use — "petition against a specific Sumptuary or trade regulation" — needs a real, targetable
/// regulation record to petition against; Policies &amp; Edicts' existing <see
/// cref="Edicts.EdictRecord"/> catalog has no Sumptuary/trade-regulation entries this item could point
/// at without inventing new content this item's own scope does not call for, so this command only ever
/// resolves the Government Contract half.</summary>
public sealed record LobbyGovernmentCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<Settlement> SettlementId,
    bool SpendInfluence) : ICommand;

public sealed record BusinessLobbiedGovernmentEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<Settlement> SettlementId,
    bool SpendInfluence,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.businessLobbiedGovernment";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class LobbyGovernmentCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.lobby.businessNotFound");
    public static readonly ValidationErrorCode SettlementNotFound = new("notableBusinesses.lobby.settlementNotFound");
    public static readonly ValidationErrorCode InfluenceSpendRequiresPlayerHousehold = new("notableBusinesses.lobby.influenceSpendRequiresPlayerHousehold");
    public static readonly ValidationErrorCode InsufficientInfluence = new("notableBusinesses.lobby.insufficientInfluence");
    public static readonly ValidationErrorCode DirectPaymentRequiresLedgerAccount = new("notableBusinesses.lobby.directPaymentRequiresLedgerAccount");

    public static readonly CommandPipeline<WorldState, LobbyGovernmentCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, LobbyGovernmentCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business))
            return BusinessNotFound;
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;

        if (command.SpendInfluence)
        {
            if (business!.Owner.Kind != PropertyOwnerKind.PlayerHousehold)
                return InfluenceSpendRequiresPlayerHousehold;
            var householdId = RuntimeId<Household>.Parse(business.Owner.OwnerId!);
            if (InfluenceResolver.Current(state, householdId) < NotableBusinessesCatalog.LobbyInfluenceCost)
                return InsufficientInfluence;
        }
        else if (business!.Owner.Kind is not (PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.RivalGens))
        {
            return DirectPaymentRequiresLedgerAccount;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, LobbyGovernmentCommand command)
    {
        var events = new List<IDomainEvent>();
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);

        if (command.SpendInfluence)
        {
            var householdId = RuntimeId<Household>.Parse(business!.Owner.OwnerId!);
            InfluenceResolver.Apply(state, householdId, -NotableBusinessesCatalog.LobbyInfluenceCost);
        }
        else
        {
            var ownerAccount = business!.Owner.Kind == PropertyOwnerKind.PlayerHousehold
                ? LedgerAccountKey.ForHousehold(RuntimeId<Household>.Parse(business.Owner.OwnerId!))
                : LedgerAccountKey.ForActor(RuntimeId<Actor>.Parse(business.Owner.OwnerId!));
            LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(ownerAccount, -NotableBusinessesCatalog.LobbyDirectPaymentCost),
                    new LedgerPosting(LedgerAccountKey.ForSettlementTreasury(command.SettlementId), NotableBusinessesCatalog.LobbyDirectPaymentCost),
                },
                reference: $"notableBusinessLobby:{command.BusinessId.ToTaggedString()}");
        }

        if (state.NotableBusinessGovernmentContracts.TryGet(command.BusinessId, out _))
        {
            events.AddRange(EndGovernmentContractCommands.Pipeline.Execute(
                state, new EndGovernmentContractCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    command.BusinessId, FailedToDeliver: false)).Events);
        }

        events.AddRange(GrantGovernmentContractCommands.Pipeline.Execute(
            state, new GrantGovernmentContractCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.BusinessId, command.SettlementId)).Events);

        events.Add(new BusinessLobbiedGovernmentEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.SettlementId, command.SpendInfluence,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
