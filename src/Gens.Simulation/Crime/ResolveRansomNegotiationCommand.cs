using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>
/// §10: resolves an open <see cref="RansomNegotiation"/>. <see cref="RansomResolution.Paid"/> moves the
/// full <see cref="RansomNegotiation.AmountOffered"/> from the target household's Ledger to the
/// capturing household's and releases the captive from Detention outright; <see
/// cref="RansomResolution.BargainedDown"/> does the same at <paramref name="AmountCountered"/> instead
/// (required for that resolution only); <see cref="RansomResolution.Refused"/> moves no money and
/// leaves the captive Detained. §10/§11's own Rival Houses Standing integration is real, not a named
/// cut: <see cref="AdjustHouseStandingCommand"/> is the actual, reusable primitive (found directly in
/// <c>Gens.Simulation.Actors</c>), so this command submits it whenever *both* households resolve to a
/// tracked <see cref="LivingWorldActor"/> via <see
/// cref="RansomNegotiationResolver.TryFindActorForHousehold"/> — which, per that resolver's own doc
/// comment, is the honest, narrower condition this integration actually meets in practice, since a
/// player's own Household never itself heads a <see cref="LivingWorldActor"/>.
/// </summary>
public sealed record ResolveRansomNegotiationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<RansomNegotiation> NegotiationId,
    RansomResolution Resolution,
    Money? AmountCountered = null) : ICommand;

/// <summary>Emitted whenever a <see cref="ResolveRansomNegotiationCommand"/> is accepted.</summary>
public sealed record RansomNegotiationResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<RansomNegotiation> NegotiationId,
    RuntimeId<Character> CaptiveCharacterId,
    RansomResolution Resolution,
    Money AmountPaid,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.ransomNegotiationResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CaptiveCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ResolveRansomNegotiationCommand"/> (ADR 0006).</summary>
public static class ResolveRansomNegotiationCommands
{
    public static readonly ValidationErrorCode NegotiationNotFound = new("crime.resolveRansomNegotiation.negotiationNotFound");
    public static readonly ValidationErrorCode AlreadyResolved = new("crime.resolveRansomNegotiation.alreadyResolved");
    public static readonly ValidationErrorCode CounterAmountRequired = new("crime.resolveRansomNegotiation.counterAmountRequired");
    public static readonly ValidationErrorCode InsufficientTreasury = new("crime.resolveRansomNegotiation.insufficientTreasury");

    public static readonly CommandPipeline<WorldState, ResolveRansomNegotiationCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ResolveRansomNegotiationCommand command)
    {
        if (!state.RansomNegotiations.TryGet(command.NegotiationId, out var negotiation))
            return NegotiationNotFound;
        if (!RansomNegotiationResolver.IsOpen(negotiation!))
            return AlreadyResolved;
        if (command.Resolution == RansomResolution.BargainedDown && command.AmountCountered is null)
            return CounterAmountRequired;

        if (command.Resolution is RansomResolution.Paid or RansomResolution.BargainedDown)
        {
            var amount = command.Resolution == RansomResolution.Paid ? negotiation!.AmountOffered : command.AmountCountered!.Value;
            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(negotiation!.TargetHouseholdId), out var account)
                ? account!.Balance
                : Money.Zero;
            if (balance < amount)
                return InsufficientTreasury;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ResolveRansomNegotiationCommand command)
    {
        state.RansomNegotiations.TryGet(command.NegotiationId, out var negotiation);
        var events = new List<IDomainEvent>();
        var amountPaid = Money.Zero;

        if (command.Resolution is RansomResolution.Paid or RansomResolution.BargainedDown)
        {
            amountPaid = command.Resolution == RansomResolution.Paid ? negotiation!.AmountOffered : command.AmountCountered!.Value;

            events.Add(LedgerService.Post(
                state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(negotiation!.TargetHouseholdId), -amountPaid),
                    new LedgerPosting(LedgerAccountKey.ForHousehold(negotiation.CapturingHouseholdId), amountPaid),
                },
                reference: $"crime:ransom:{command.CommandId.ToTaggedString()}"));

            if (DetentionResolver.ActiveFor(state, negotiation.CaptiveCharacterId) is { } detention)
            {
                state.DetentionRecords.Remove(detention.DetentionId);
                state.DetentionRecords.Add(detention.DetentionId, detention with { EndDate = command.SubmittedDate });
            }

            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state, new AdjustDignitasCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    negotiation.CapturingHouseholdId, CrimeCatalog.RansomPaidOrMercyDignitasGain,
                    $"ransom paid for {negotiation.CaptiveCharacterId.ToTaggedString()}")).Events);

            events.AddRange(ApplyHeadToHeadOpinion(
                state, negotiation.CapturingHouseholdId, negotiation.TargetHouseholdId, command,
                CrimeCatalog.RansomPaidOrMercyOpinionGain));

            events.AddRange(ApplyStandingIfBothAreTrackedActors(state, negotiation, command, HouseStandingAdjustmentDirection.TowardAlliance));
        }
        else
        {
            events.AddRange(ApplyHeadToHeadOpinion(
                state, negotiation!.CapturingHouseholdId, negotiation.TargetHouseholdId, command,
                -CrimeCatalog.RansomRefusedOpinionPenalty));

            events.AddRange(ApplyStandingIfBothAreTrackedActors(state, negotiation, command, HouseStandingAdjustmentDirection.TowardRivalry));
        }

        var resolved = negotiation! with
        {
            Resolution = command.Resolution,
            ResolvedDate = command.SubmittedDate,
            AmountCountered = command.AmountCountered ?? negotiation.AmountCountered,
        };
        state.RansomNegotiations.Remove(command.NegotiationId);
        state.RansomNegotiations.Add(command.NegotiationId, resolved);

        events.Add(new RansomNegotiationResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.NegotiationId, negotiation.CaptiveCharacterId,
            command.Resolution, amountPaid, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static IDomainEvent[] ApplyHeadToHeadOpinion(
        WorldState state, RuntimeId<Household> capturingHouseholdId, RuntimeId<Household> targetHouseholdId,
        ResolveRansomNegotiationCommand command, int opinionDelta)
    {
        if (!state.HouseholdHeadships.TryGet(capturingHouseholdId, out var capturingHeadship) ||
            !state.HouseholdHeadships.TryGet(targetHouseholdId, out var targetHeadship))
            return Array.Empty<IDomainEvent>();

        var capturingHeadId = capturingHeadship!.HeadCharacterId;
        var targetHeadId = targetHeadship!.HeadCharacterId;

        if (!state.Characters.TryGet(capturingHeadId, out var capturingHead) || !capturingHead!.IsAlive ||
            !state.Characters.TryGet(targetHeadId, out var targetHead) || !targetHead!.IsAlive)
            return Array.Empty<IDomainEvent>();

        return RecordInteractionCommands.Pipeline.Execute(
            state, new RecordInteractionCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                capturingHeadId, targetHeadId, opinionDelta, BondTag.None, BondTag.None, RelationshipOrigin.Political)).Events.ToArray();
    }

    private static IDomainEvent[] ApplyStandingIfBothAreTrackedActors(
        WorldState state, RansomNegotiation negotiation, ResolveRansomNegotiationCommand command,
        HouseStandingAdjustmentDirection direction)
    {
        var capturingActorId = RansomNegotiationResolver.TryFindActorForHousehold(state, negotiation.CapturingHouseholdId);
        var targetActorId = RansomNegotiationResolver.TryFindActorForHousehold(state, negotiation.TargetHouseholdId);
        if (capturingActorId is not { } initiator || targetActorId is not { } target || initiator == target)
            return Array.Empty<IDomainEvent>();

        // A validation rejection here (e.g. AlreadyAtExtreme) is a real, harmless no-op — a ransom's
        // own Standing nudge is a bonus on top of the Ledger/Dignitas/opinion consequences already
        // applied above, not a required step this command's own acceptance depends on.
        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, new AdjustHouseStandingCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                initiator, target, direction));
        return result.Events.ToArray();
    }
}
