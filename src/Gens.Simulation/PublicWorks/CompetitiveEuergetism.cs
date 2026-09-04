using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicWorks;

/// <summary>
/// §5/§9's <c>CompetitiveEuergetismEvent</c> data model (Phase 15 item 9) — reusing Business
/// Competition's own escalation-ladder logic (§5's own text: "built on the identical underlying
/// escalation logic," that document's §2), applied to civic generosity rather than commercial rivalry.
/// Unlike <see cref="BusinessCompetition.CompetitiveEscalation"/> (naturally keyed by its own aggressor's
/// already-registered <see cref="RuntimeId{NotableBusiness}"/>), a Public Works rivalry has no single
/// already-registered entity both households share, so this item needs a real, fresh <see
/// cref="RuntimeId{T}"/> of its own, matching <see cref="BusinessCompetition.CartelAgreement"/>'s
/// identical "genuinely its own entity" precedent for the same reason (an N/two-ary relationship, not one
/// party's own attribute).
/// </summary>
public sealed record CompetitiveEuergetismEvent
{
    private CompetitiveEuergetismEvent()
    {
    }

    public required RuntimeId<CompetitiveEuergetismEvent> Id { get; init; }
    public required RuntimeId<Settlement> SettlementId { get; init; }

    /// <summary>Restricted to <see cref="PropertyOwnerKind.PlayerHousehold"/>/<see
    /// cref="PropertyOwnerKind.RivalGens"/> — the two real household-like entities this codebase tracks,
    /// matching every other Phase 15 item's identical narrowing (<see
    /// cref="MerchantFamilies.EquestrianStatusQuery"/>'s own doc comment).</summary>
    public required PropertyOwnerRef InitiatingHouseholdId { get; init; }

    public required PropertyOwnerRef RespondingHouseholdId { get; init; }
    public required int EscalationRound { get; init; }

    public static CompetitiveEuergetismEvent Create(
        RuntimeId<CompetitiveEuergetismEvent> id, RuntimeId<Settlement> settlementId, PropertyOwnerRef initiatingHouseholdId,
        PropertyOwnerRef respondingHouseholdId) => new()
        {
            Id = id,
            SettlementId = settlementId,
            InitiatingHouseholdId = initiatingHouseholdId,
            RespondingHouseholdId = respondingHouseholdId,
            EscalationRound = 1,
        };

    /// <summary>Reconstructs a <see cref="CompetitiveEuergetismEvent"/> from persisted save data (ADR
    /// 0010).</summary>
    public static CompetitiveEuergetismEvent Restore(
        RuntimeId<CompetitiveEuergetismEvent> id, RuntimeId<Settlement> settlementId, PropertyOwnerRef initiatingHouseholdId,
        PropertyOwnerRef respondingHouseholdId, int escalationRound) => new()
        {
            Id = id,
            SettlementId = settlementId,
            InitiatingHouseholdId = initiatingHouseholdId,
            RespondingHouseholdId = respondingHouseholdId,
            EscalationRound = escalationRound,
        };
}

public sealed record CompetitiveEuergetismInitiatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<CompetitiveEuergetismEvent> CompetitiveEuergetismEventId,
    RuntimeId<Settlement> SettlementId,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicWorks.competitiveEuergetismInitiated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CompetitiveEuergetismEventId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§5's opening move — a household funds a Public Work and a Rival House responds, "raising the
/// real Dignitas stakes." Validated against two distinct real household-like <see cref="PropertyOwnerRef"/>
/// kinds; does not itself re-validate that <paramref name="InitiatingPublicWorkId"/> or a responding work
/// actually exist yet, mirroring <see cref="RealEstate.TransferPropertyCommand"/>'s own "reveal/record,
/// don't re-validate the upstream trigger" scoping — the initiating Public Work is this item's own real,
/// separate <see cref="FundPublicWorkCommand"/>.</summary>
public sealed record InitiateCompetitiveEuergetismCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Settlement> SettlementId,
    PropertyOwnerRef InitiatingHouseholdId,
    PropertyOwnerRef RespondingHouseholdId) : ICommand;

public static class InitiateCompetitiveEuergetismCommands
{
    public static readonly ValidationErrorCode SettlementNotFound = new("publicWorks.initiateCompetitiveEuergetism.settlementNotFound");
    public static readonly ValidationErrorCode UnsupportedHouseholdKind = new("publicWorks.initiateCompetitiveEuergetism.unsupportedHouseholdKind");
    public static readonly ValidationErrorCode SameHousehold = new("publicWorks.initiateCompetitiveEuergetism.sameHousehold");

    public static readonly CommandPipeline<WorldState, InitiateCompetitiveEuergetismCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, InitiateCompetitiveEuergetismCommand command)
    {
        if (!state.Settlements.TryGet(command.SettlementId, out _))
            return SettlementNotFound;
        if (!IsSupportedHouseholdKind(command.InitiatingHouseholdId) || !IsSupportedHouseholdKind(command.RespondingHouseholdId))
            return UnsupportedHouseholdKind;
        if (command.InitiatingHouseholdId == command.RespondingHouseholdId)
            return SameHousehold;

        return null;
    }

    private static bool IsSupportedHouseholdKind(PropertyOwnerRef owner) =>
        owner.Kind is PropertyOwnerKind.PlayerHousehold or PropertyOwnerKind.RivalGens;

    private static IDomainEvent[] Mutate(WorldState state, InitiateCompetitiveEuergetismCommand command)
    {
        var id = state.CompetitiveEuergetismEventIds.Issue();
        var record = CompetitiveEuergetismEvent.Create(id, command.SettlementId, command.InitiatingHouseholdId, command.RespondingHouseholdId);
        state.CompetitiveEuergetismEvents.Add(id, record);

        return new IDomainEvent[]
        {
            new CompetitiveEuergetismInitiatedEvent(state.EventIds.Issue(), command.SubmittedDate, id, command.SettlementId, command.CommandId.ToTaggedString()),
        };
    }
}

public sealed record CompetitiveEuergetismEscalatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<CompetitiveEuergetismEvent> CompetitiveEuergetismEventId,
    int EscalationRound,
    Money Cost,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicWorks.competitiveEuergetismEscalated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CompetitiveEuergetismEventId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>§5's escalation round — the responding household funds "an ever more impressive" work of its
/// own, this item's own real, scaled cost (<see
/// cref="PublicWorksCatalog.EscalationCostMultiplierPerRound"/>) and Dignitas award (<see
/// cref="PublicWorksCatalog.EscalationDignitasPerRound"/>), capped at <see
/// cref="PublicWorksCatalog.MaxEscalationRounds"/> per §10's own "a real escalation needs a real top."
/// The escalating household alternates with each call (the round's own current <see
/// cref="CompetitiveEuergetismEvent.RespondingHouseholdId"/> becomes the next round's initiator) — §5's
/// own real back-and-forth "arms race." Only a <see cref="PropertyOwnerKind.PlayerHousehold"/> side is
/// actually charged and awarded Dignitas, the same honest narrowing <see
/// cref="FundPublicWorkCommands"/> already applies: a RivalGens side has no real tracked Ledger balance
/// this codebase can debit, and no command anywhere adjusts a Rival House's own Dignitas field directly
/// (matching <see cref="MerchantFamilies.RecordDignitasInvestmentActionCommand"/>'s own identical
/// finding).</summary>
public sealed record EscalateCompetitiveEuergetismCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<CompetitiveEuergetismEvent> CompetitiveEuergetismEventId) : ICommand;

public static class EscalateCompetitiveEuergetismCommands
{
    public static readonly ValidationErrorCode EventNotFound = new("publicWorks.escalateCompetitiveEuergetism.eventNotFound");
    public static readonly ValidationErrorCode AtCeiling = new("publicWorks.escalateCompetitiveEuergetism.atCeiling");
    public static readonly ValidationErrorCode InsufficientFunds = new("publicWorks.escalateCompetitiveEuergetism.insufficientFunds");

    public static readonly CommandPipeline<WorldState, EscalateCompetitiveEuergetismCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EscalateCompetitiveEuergetismCommand command)
    {
        if (!state.CompetitiveEuergetismEvents.TryGet(command.CompetitiveEuergetismEventId, out var record))
            return EventNotFound;
        if (record!.EscalationRound >= PublicWorksCatalog.MaxEscalationRounds)
            return AtCeiling;

        if (record.RespondingHouseholdId.Kind == PropertyOwnerKind.PlayerHousehold)
        {
            var householdId = RuntimeId<Household>.Parse(record.RespondingHouseholdId.OwnerId!);
            var cost = EscalationCost(record.EscalationRound + 1);
            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account)
                ? account!.Balance
                : Money.Zero;
            if (balance.RawValue < cost.RawValue)
                return InsufficientFunds;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EscalateCompetitiveEuergetismCommand command)
    {
        state.CompetitiveEuergetismEvents.TryGet(command.CompetitiveEuergetismEventId, out var record);
        var nextRound = record!.EscalationRound + 1;
        var cost = EscalationCost(nextRound);
        var events = new List<IDomainEvent>();

        if (record.RespondingHouseholdId.Kind == PropertyOwnerKind.PlayerHousehold)
        {
            var householdId = RuntimeId<Household>.Parse(record.RespondingHouseholdId.OwnerId!);
            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Treasury,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -cost),
                    new LedgerPosting(LedgerAccountKey.Mint, cost),
                },
                reference: $"publicWorks.competitiveEuergetism.escalate:{command.CompetitiveEuergetismEventId.ToTaggedString()}"));

            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    householdId, PublicWorksCatalog.EscalationDignitasPerRound * nextRound, "public works: competitive euergetism escalation")).Events);
            EuergetismObligationResolver.RecordFunded(state, householdId);
        }

        var updated = record with
        {
            EscalationRound = nextRound,
            InitiatingHouseholdId = record.RespondingHouseholdId,
            RespondingHouseholdId = record.InitiatingHouseholdId,
        };
        state.CompetitiveEuergetismEvents.Remove(command.CompetitiveEuergetismEventId);
        state.CompetitiveEuergetismEvents.Add(command.CompetitiveEuergetismEventId, updated);

        events.Add(new CompetitiveEuergetismEscalatedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.CompetitiveEuergetismEventId, nextRound, cost, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static Money EscalationCost(int round)
    {
        var basis = PublicWorksCatalog.ConstructionCost(PublicWorkType.MarketplaceOrBasilica);
        var multiplier = Numerics.Fixed64.One + Numerics.Fixed64.Multiply(PublicWorksCatalog.EscalationCostMultiplierPerRound, Numerics.Fixed64.FromInt(round));
        return basis.Scale(multiplier);
    }
}
