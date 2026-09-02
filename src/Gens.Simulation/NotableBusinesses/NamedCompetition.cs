using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§5's Named Competition (Phase 15 item 4): sets or clears a Notable Business's own <see
/// cref="NotableBusiness.MainCompetitorBusinessId"/>. Deliberately unidirectional — §5's worked example
/// (two bakeries, each with the other as its own Main Competitor) is realized by submitting this
/// command once per side, exactly like ordinary <see cref="Characters.Relationship"/> bonds are each
/// recorded from one side's own perspective — a caller wanting a mutual rivalry submits it
/// twice.</summary>
public sealed record SetMainCompetitorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<NotableBusiness>? CompetitorBusinessId) : ICommand;

public sealed record MainCompetitorSetEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    RuntimeId<NotableBusiness>? CompetitorBusinessId,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.mainCompetitorSet";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SetMainCompetitorCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.setMainCompetitor.businessNotFound");
    public static readonly ValidationErrorCode CompetitorNotFound = new("notableBusinesses.setMainCompetitor.competitorNotFound");
    public static readonly ValidationErrorCode SelfTargeted = new("notableBusinesses.setMainCompetitor.selfTargeted");

    public static readonly CommandPipeline<WorldState, SetMainCompetitorCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SetMainCompetitorCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out _))
            return BusinessNotFound;
        if (command.CompetitorBusinessId == command.BusinessId)
            return SelfTargeted;
        if (command.CompetitorBusinessId is { } competitorId && !state.NotableBusinesses.TryGet(competitorId, out _))
            return CompetitorNotFound;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SetMainCompetitorCommand command)
    {
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);
        state.NotableBusinesses.Remove(command.BusinessId);
        state.NotableBusinesses.Add(command.BusinessId, business! with { MainCompetitorBusinessId = command.CompetitorBusinessId });

        return new IDomainEvent[]
        {
            new MainCompetitorSetEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, command.CompetitorBusinessId,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>§5's own four named escalating competitive actions.</summary>
public enum BusinessRivalryActionType
{
    /// <summary>§5.1's worked example — "Gaius undercuts his own bread prices for a season," reading
    /// against Economy &amp; Finance's own Market Dynamics. This item does not itself move <see
    /// cref="Markets.SettlementMarket"/> prices — that mechanism belongs to this phase's own item 5
    /// ("Business competition, price wars... lawful/unlawful responses"), per the roadmap's own
    /// construction order. This item records the act and its real Reputation consequence on the
    /// targeted rival, matching §5's own "his own Reputation and income both take a real, felt
    /// hit."</summary>
    PriceUndercut,

    /// <summary>§5's "poaching a skilled Opifex worker away from the rival." §11's own open question
    /// deliberately keeps employee headcount a derived Settlement Demographics figure rather than
    /// individually tracked — this item does not move any Opifices pop-group count; it records the act
    /// and its Reputation consequence on the target only.</summary>
    WorkerPoach,

    /// <summary>§5's sharper end — "an actual Coercive Interaction (Characters §9.4 — Sabotage...)
    /// deployed specifically against a business rival." See <see
    /// cref="RecordBusinessRivalryActionCommands"/> for how this reuses <see
    /// cref="InitiateSchemeCommand"/>.</summary>
    Sabotage,

    /// <summary>§5's "Spread a Damaging Rumor... questioning the cleanliness of Gaius's own ovens." No
    /// concrete "Spread a Damaging Rumor" Interaction exists anywhere in <c>Gens.Simulation.Interactions</c>
    /// (confirmed by direct search — <see cref="Scandal.ScandalSourceType.DeliberateRumor"/>'s own doc
    /// comment already names this same gap: only a named row in <c>gens-characters-design.md</c> §9.4's
    /// own table). This item reuses the one real Coercive Interaction mechanism that does exist, the
    /// generic <see cref="SchemeType.Coercive"/> wrapper, exactly like <see cref="Sabotage"/> — see <see
    /// cref="RecordBusinessRivalryActionCommands"/>.</summary>
    DamagingRumor,
}

/// <summary>One §5/§10 <c>BusinessRivalryEvent</c> entry (Phase 15 item 4).</summary>
public sealed record BusinessRivalryLogEntry(
    RuntimeId<NotableBusiness> TargetBusinessId, BusinessRivalryActionType ActionType, int ReputationEffect, GameDate Date);

/// <summary>One initiating business's own append-only rivalry-action history, matching <see
/// cref="MerchantFamilies.SenateEntryInvestmentLog"/>'s identical "present only once touched, append-only
/// log" shape rather than each entry carrying its own <see cref="RuntimeId{T}"/> — §10's own sketch names
/// an <c>eventId</c>, but no caller needs to address one entry individually (only ever append and read
/// the whole history), the same "an ID nothing addresses individually" omission that shape already
/// established.</summary>
public sealed record NotableBusinessRivalryLog(RuntimeId<NotableBusiness> InitiatingBusinessId, IReadOnlyList<BusinessRivalryLogEntry> Entries);

/// <summary>§5's Named Competition in action (Phase 15 item 4) — one business's real competitive move
/// against its own already-named <see cref="NotableBusiness.MainCompetitorBusinessId"/> rival (checked
/// both directions: §5.1's rivalry is a mutual, felt relationship, not a one-sided label). Applies the
/// real Reputation hit §5's own worked example names (<see
/// cref="NotableBusinessesCatalog.RivalryActionReputationEffectFor"/>, routed through <see
/// cref="AdjustBusinessReputationCommand"/>) to the targeted business, and — for <see
/// cref="BusinessRivalryActionType.Sabotage"/>/<see cref="BusinessRivalryActionType.DamagingRumor"/>
/// only — additionally starts a real <see cref="InitiateSchemeCommand"/> (<see
/// cref="SchemeType.Coercive"/>) between the two rivals' own owners, wherever both resolve to a real,
/// living Character (<see cref="NotableBusinessOwnerResolver.TryResolveCharacter"/>) and no such Scheme
/// is already in progress between that exact pair — a best-effort secondary integration: this command
/// never fails or rejects because that Scheme-initiation step could not fire (an unresolvable owner, or
/// one already running), since the real, guaranteed-to-land consequence is always the Reputation
/// hit.</summary>
public sealed record RecordBusinessRivalryActionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> InitiatingBusinessId,
    RuntimeId<NotableBusiness> TargetBusinessId,
    BusinessRivalryActionType ActionType) : ICommand;

public sealed record BusinessRivalryActionRecordedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> InitiatingBusinessId,
    RuntimeId<NotableBusiness> TargetBusinessId,
    BusinessRivalryActionType ActionType,
    int ReputationEffect,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.rivalryActionRecorded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { InitiatingBusinessId.ToTaggedString(), TargetBusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class RecordBusinessRivalryActionCommands
{
    public static readonly ValidationErrorCode InitiatingBusinessNotFound = new("notableBusinesses.recordRivalryAction.initiatingBusinessNotFound");
    public static readonly ValidationErrorCode TargetBusinessNotFound = new("notableBusinesses.recordRivalryAction.targetBusinessNotFound");
    public static readonly ValidationErrorCode NotMainCompetitors = new("notableBusinesses.recordRivalryAction.notMainCompetitors");

    public static readonly CommandPipeline<WorldState, RecordBusinessRivalryActionCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordBusinessRivalryActionCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.InitiatingBusinessId, out var initiating))
            return InitiatingBusinessNotFound;
        if (!state.NotableBusinesses.TryGet(command.TargetBusinessId, out var target))
            return TargetBusinessNotFound;

        // §5.1: "the same underlying... named rivalry" — a felt, mutual relationship, not a one-sided
        // label, so this command only fires between two businesses that have each already named the
        // other as their own Main Competitor.
        if (initiating!.MainCompetitorBusinessId != command.TargetBusinessId || target!.MainCompetitorBusinessId != command.InitiatingBusinessId)
            return NotMainCompetitors;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RecordBusinessRivalryActionCommand command)
    {
        var events = new List<IDomainEvent>();
        var effect = NotableBusinessesCatalog.RivalryActionReputationEffectFor(command.ActionType);

        events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.TargetBusinessId, effect, BusinessReputationChangeReason.CompetitiveRivalry)).Events);

        if (command.ActionType is BusinessRivalryActionType.Sabotage or BusinessRivalryActionType.DamagingRumor)
            events.AddRange(TryInitiateCoerciveScheme(state, command));

        state.NotableBusinesses.TryGet(command.InitiatingBusinessId, out var initiating);
        var log = state.NotableBusinessRivalryLogs.TryGet(command.InitiatingBusinessId, out var existing)
            ? existing!
            : new NotableBusinessRivalryLog(command.InitiatingBusinessId, Array.Empty<BusinessRivalryLogEntry>());
        var updatedLog = log with
        {
            Entries = log.Entries.Append(new BusinessRivalryLogEntry(command.TargetBusinessId, command.ActionType, effect, command.SubmittedDate)).ToArray(),
        };
        if (state.NotableBusinessRivalryLogs.TryGet(command.InitiatingBusinessId, out _))
            state.NotableBusinessRivalryLogs.Remove(command.InitiatingBusinessId);
        state.NotableBusinessRivalryLogs.Add(command.InitiatingBusinessId, updatedLog);

        events.Add(new BusinessRivalryActionRecordedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.InitiatingBusinessId, command.TargetBusinessId,
            command.ActionType, effect, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }

    private static IEnumerable<IDomainEvent> TryInitiateCoerciveScheme(WorldState state, RecordBusinessRivalryActionCommand command)
    {
        state.NotableBusinesses.TryGet(command.InitiatingBusinessId, out var initiating);
        state.NotableBusinesses.TryGet(command.TargetBusinessId, out var target);

        if (!NotableBusinessOwnerResolver.TryResolveCharacter(state, initiating!.Owner, out var initiatorCharacterId))
            return Array.Empty<IDomainEvent>();
        if (!NotableBusinessOwnerResolver.TryResolveCharacter(state, target!.Owner, out var targetCharacterId))
            return Array.Empty<IDomainEvent>();
        if (initiatorCharacterId == targetCharacterId)
            return Array.Empty<IDomainEvent>();

        var result = InitiateSchemeCommands.Pipeline.Execute(
            state, new InitiateSchemeCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                initiatorCharacterId, targetCharacterId, SchemeType.Coercive));

        // Best-effort: an already-in-progress Scheme between this exact pair (or any other validation
        // failure) is not this command's own failure — the Reputation hit above is the one guaranteed
        // consequence, per this type's own doc comment.
        return result.Accepted ? result.Events : Array.Empty<IDomainEvent>();
    }
}
