using Gens.Simulation.Characters;
using Gens.Simulation.Collegia;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Markets;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.Numerics;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.BusinessCompetition;

/// <summary>§2's full four-rung ladder, extending Notable Businesses' own worked example (that
/// document's §5.1, which only sketched the opening moves).</summary>
public enum CompetitiveEscalationRung
{
    /// <summary>The default state — no <see cref="CompetitiveEscalation"/> record exists at all while
    /// two Main Competitors sit here, matching <see cref="RealEstate.PlotPropertyExtension.Default"/>'s
    /// own identical "untouched = the default rung, no record needed" convention.</summary>
    OrdinaryRivalry,

    PriceWar,
    PredatoryPricing,

    /// <summary>Terminal — reached only through <see cref="CompetitiveEscalationSystem"/>'s own
    /// automatic detection (§2: "the rival actually reaches Insolvency"), never directly by a player
    /// command (mirroring <see cref="Societates.DissolveSocietasCommand"/>'s own <c>Fraud</c> trigger
    /// gated to the <c>"system"</c> actor sentinel). See <see cref="ResolveForcedConsolidationCommand"/>
    /// for §7's own follow-through.</summary>
    ForcedConsolidation,
}

/// <summary>
/// §2's/§9's <c>CompetitiveEscalation</c> data model (Phase 15 item 5) — one aggressor business's own
/// real, running escalation against its already-named <see
/// cref="NotableBusiness.MainCompetitorBusinessId"/> rival. Sparse, keyed by the aggressor's own
/// already-registered <see cref="RuntimeId{NotableBusiness}"/> rather than a fresh <see
/// cref="RuntimeId{T}"/> of its own — §9's own <c>escalationId</c> sketch is unnecessary here since
/// nothing ever needs to address one escalation independently of "the aggressor business's own current
/// ladder position," matching <see cref="NotableBusinesses.NotableBusinessGovernmentContract"/>'s
/// identical "present only once touched, keyed by the already-registered ID" convention. At most one
/// escalation per aggressor at a time (a business running two simultaneous price wars against different
/// rivals is not modeled — the same "the same underlying... named rivalry" singular-Main-Competitor
/// framing <see cref="NotableBusinesses.NamedCompetition"/> already establishes).
/// </summary>
/// <param name="CollegiumDignitasImpact">§3's own separate, cumulative Dignitas cost this exact
/// escalation has charged the aggressor's own household for running it against a fellow Collegium
/// member — 0 whenever <paramref name="IsWithinSameCollegium"/> is false.</param>
public sealed record CompetitiveEscalation(
    RuntimeId<NotableBusiness> BusinessAId,
    RuntimeId<NotableBusiness> BusinessBId,
    CompetitiveEscalationRung CurrentRung,
    bool IsWithinSameCollegium,
    int CollegiumDignitasImpact);

public static class CompetitiveEscalationResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<NotableBusiness> aggressorBusinessId, out CompetitiveEscalation escalation) =>
        state.CompetitiveEscalations.TryGet(aggressorBusinessId, out escalation!);

    /// <summary>§3's own real gate: true whenever both businesses' owners resolve to a real household
    /// that shares membership in at least one common <see cref="CollegiumDetails"/> roster. Only a <see
    /// cref="RealEstate.PropertyOwnerKind.PlayerHousehold"/> owner can ever be checked — <see
    /// cref="CollegiumDetails.MemberHouseholdIds"/> is itself a real, hand-curated household roster
    /// (that record's own doc comment), and no Collegium anywhere in this codebase ever lists a Rival
    /// Gens Actor or a bare Character as a member.</summary>
    public static bool AreWithinSameCollegium(WorldState state, PropertyOwnerRef ownerA, PropertyOwnerRef ownerB)
    {
        if (!NotableBusinessOwnerResolver.TryResolveHousehold(ownerA, out var householdA))
            return false;
        if (!NotableBusinessOwnerResolver.TryResolveHousehold(ownerB, out var householdB))
            return false;

        foreach (var entry in state.Collegia.InAscendingOrder())
        {
            if (entry.Value.MemberHouseholdIds.Contains(householdA) && entry.Value.MemberHouseholdIds.Contains(householdB))
                return true;
        }

        return false;
    }
}

/// <summary>§2's own escalation move — steps the aggressor's own ladder position up by exactly one rung
/// (Ordinary Rivalry -&gt; Price War -&gt; Predatory Pricing only; §2's own rung 4, Forced Consolidation,
/// is reached only automatically — see <see cref="CompetitiveEscalationRung.ForcedConsolidation"/>'s own
/// doc comment), gated on the identical mutual-Main-Competitor check <see
/// cref="NotableBusinesses.RecordBusinessRivalryActionCommands"/> already establishes for §5. Folds in
/// §3's own Breaking Ranks consequence directly: whenever this exact step is run against a fellow
/// Collegium member (<see cref="CompetitiveEscalationResolver.AreWithinSameCollegium"/>), it also applies
/// a real Dignitas penalty to the aggressor's own household (<see
/// cref="BusinessCompetitionCatalog.BreakingRanksDignitasPenalty"/>, via <see
/// cref="AdjustDignitasCommand"/>) and a real Opinion penalty, tagged <see cref="BondTag.Rival"/>,
/// between the two rivals' own resolved owner Characters (via <see cref="RecordInteractionCommand"/>) —
/// §3's own two real, always-reachable consequences. §3's own further, severer "organized collective
/// response mirroring the collegium's own darker political-disruption capacity" is a real, verified scope
/// cut: <see cref="RecordCollegiumOrganizedDisruptionCommand"/> is the one real mechanism that shape of
/// consequence could reuse, but that command's own validation requires the acting household to already
/// sponsor the collegium in question (§4's patron relationship) — nothing guarantees the collegium a
/// breaking-ranks offender belongs to is sponsored by anyone able to retaliate this way, so fabricating
/// that precondition just to force the reuse is rejected in favor of naming the gap directly, matching
/// this phase's own established "name what's left honestly" convention.</summary>
public sealed record EscalateCompetitiveRungCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> AggressorBusinessId,
    RuntimeId<NotableBusiness> TargetBusinessId) : ICommand;

public sealed record CompetitiveEscalationChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> AggressorBusinessId,
    RuntimeId<NotableBusiness> TargetBusinessId,
    CompetitiveEscalationRung PreviousRung,
    CompetitiveEscalationRung NewRung,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.escalationChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { AggressorBusinessId.ToTaggedString(), TargetBusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class EscalateCompetitiveRungCommands
{
    public static readonly ValidationErrorCode AggressorNotFound = new("businessCompetition.escalate.aggressorNotFound");
    public static readonly ValidationErrorCode TargetNotFound = new("businessCompetition.escalate.targetNotFound");
    public static readonly ValidationErrorCode NotMainCompetitors = new("businessCompetition.escalate.notMainCompetitors");
    public static readonly ValidationErrorCode NotSameTrade = new("businessCompetition.escalate.notSameTrade");
    public static readonly ValidationErrorCode AlreadyAtCeiling = new("businessCompetition.escalate.alreadyAtCeiling");
    public static readonly ValidationErrorCode WrongAggressor = new("businessCompetition.escalate.wrongAggressor");

    public static readonly CommandPipeline<WorldState, EscalateCompetitiveRungCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EscalateCompetitiveRungCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.AggressorBusinessId, out var aggressor))
            return AggressorNotFound;
        if (!state.NotableBusinesses.TryGet(command.TargetBusinessId, out var target))
            return TargetNotFound;
        if (aggressor!.MainCompetitorBusinessId != command.TargetBusinessId || target!.MainCompetitorBusinessId != command.AggressorBusinessId)
            return NotMainCompetitors;
        // §2's own "cuts prices... to draw customers away from a named competitor" only makes economic
        // sense between two businesses trading the same good — a mismatched or untracked OutputGoodId
        // pair would let the price nudge (which moves only the aggressor's own good market) and the
        // eventual Forced Consolidation trigger fire against an economically unrelated rival.
        if (aggressor.OutputGoodId is null || aggressor.OutputGoodId != target!.OutputGoodId)
            return NotSameTrade;

        if (state.CompetitiveEscalations.TryGet(command.AggressorBusinessId, out var existing))
        {
            if (existing!.BusinessBId != command.TargetBusinessId)
                return WrongAggressor;
            if (existing.CurrentRung >= CompetitiveEscalationRung.PredatoryPricing)
                return AlreadyAtCeiling;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EscalateCompetitiveRungCommand command)
    {
        var events = new List<IDomainEvent>();
        state.NotableBusinesses.TryGet(command.AggressorBusinessId, out var aggressor);
        state.NotableBusinesses.TryGet(command.TargetBusinessId, out var target);

        var hasExisting = state.CompetitiveEscalations.TryGet(command.AggressorBusinessId, out var existing);
        var previousRung = hasExisting ? existing!.CurrentRung : CompetitiveEscalationRung.OrdinaryRivalry;
        var nextRung = previousRung == CompetitiveEscalationRung.OrdinaryRivalry
            ? CompetitiveEscalationRung.PriceWar
            : CompetitiveEscalationRung.PredatoryPricing;

        var isWithinSameCollegium = CompetitiveEscalationResolver.AreWithinSameCollegium(state, aggressor!.Owner, target!.Owner);
        var dignitasImpact = hasExisting ? existing!.CollegiumDignitasImpact : 0;

        if (isWithinSameCollegium)
        {
            dignitasImpact += BusinessCompetitionCatalog.BreakingRanksDignitasPenalty;
            if (NotableBusinessOwnerResolver.TryResolveHousehold(aggressor.Owner, out var aggressorHouseholdId))
            {
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                    state, new AdjustDignitasCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        aggressorHouseholdId, -BusinessCompetitionCatalog.BreakingRanksDignitasPenalty,
                        $"breaking ranks against fellow Collegium member's business {command.TargetBusinessId.ToTaggedString()}")).Events);
            }

            if (NotableBusinessOwnerResolver.TryResolveCharacter(state, aggressor.Owner, out var aggressorCharacterId)
                && NotableBusinessOwnerResolver.TryResolveCharacter(state, target.Owner, out var targetCharacterId)
                && aggressorCharacterId != targetCharacterId)
            {
                events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                    state, new RecordInteractionCommand(
                        state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                        aggressorCharacterId, targetCharacterId, BusinessCompetitionCatalog.BreakingRanksOpinionPenalty,
                        BondTag.Rival, BondTag.None, RelationshipOrigin.Commercial)).Events);
            }
        }

        if (hasExisting)
            state.CompetitiveEscalations.Remove(command.AggressorBusinessId);
        state.CompetitiveEscalations.Add(
            command.AggressorBusinessId,
            new CompetitiveEscalation(command.AggressorBusinessId, command.TargetBusinessId, nextRung, isWithinSameCollegium, dignitasImpact));

        events.Add(new CompetitiveEscalationChangedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.AggressorBusinessId, command.TargetBusinessId,
            previousRung, nextRung, command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}

/// <summary>§2's own "genuinely reversible — either side can simply stop": steps the aggressor's own
/// ladder position back down by one rung, or removes the record outright once it reaches Ordinary
/// Rivalry. Never reachable from <see cref="CompetitiveEscalationRung.ForcedConsolidation"/> — that rung
/// is terminal (§2: "the rival actually reaches Insolvency... aggressor is now the... natural
/// buyer" — there is no walking that back).</summary>
public sealed record DeescalateCompetitiveRungCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> AggressorBusinessId) : ICommand;

public static class DeescalateCompetitiveRungCommands
{
    public static readonly ValidationErrorCode EscalationNotFound = new("businessCompetition.deescalate.escalationNotFound");
    public static readonly ValidationErrorCode CannotDeescalateForcedConsolidation = new("businessCompetition.deescalate.cannotDeescalateForcedConsolidation");

    public static readonly CommandPipeline<WorldState, DeescalateCompetitiveRungCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DeescalateCompetitiveRungCommand command)
    {
        if (!state.CompetitiveEscalations.TryGet(command.AggressorBusinessId, out var existing))
            return EscalationNotFound;
        if (existing!.CurrentRung == CompetitiveEscalationRung.ForcedConsolidation)
            return CannotDeescalateForcedConsolidation;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, DeescalateCompetitiveRungCommand command)
    {
        state.CompetitiveEscalations.TryGet(command.AggressorBusinessId, out var existing);
        var previousRung = existing!.CurrentRung;
        var nextRung = previousRung == CompetitiveEscalationRung.PredatoryPricing
            ? CompetitiveEscalationRung.PriceWar
            : CompetitiveEscalationRung.OrdinaryRivalry;

        state.CompetitiveEscalations.Remove(command.AggressorBusinessId);
        if (nextRung != CompetitiveEscalationRung.OrdinaryRivalry)
            state.CompetitiveEscalations.Add(command.AggressorBusinessId, existing with { CurrentRung = nextRung });

        return new IDomainEvent[]
        {
            new CompetitiveEscalationChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.AggressorBusinessId, existing.BusinessBId,
                previousRung, nextRung, command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>
/// The monthly resolution for every active §2 escalation (Phase 15 item 5). Matches <see
/// cref="NotableBusinesses.SupplierDisruptionSystem"/>'s own established "a static <c>Tick(state, date)</c>
/// helper, exercised directly by its own tests, not wired into the central <see
/// cref="IMonthlySystem{TState}"/> pipeline" convention — the same primitive-before-caller precedent
/// this phase's own item 4 already set for Notable Businesses' own monthly-ish resolution.
///
/// For every escalation at <see cref="CompetitiveEscalationRung.PriceWar"/> or <see
/// cref="CompetitiveEscalationRung.PredatoryPricing"/>: applies §2's own "cuts prices... to draw
/// customers away" as a real, small downward nudge to the aggressor's own settlement-market price for
/// its own <see cref="NotableBusiness.OutputGoodId"/> (only where that resolves to a real settlement via
/// <see cref="NotableBusiness.DistrictId"/> and a real cleared <see cref="SettlementMarket"/> — an
/// unlinked business genuinely has no market this system can move), clamped so the combined move never
/// exceeds <see cref="MarketPriceBoundConfig.Default"/>'s own bound (deliberately not touching <see
/// cref="Markets.MarketClearingSystem"/> itself, which remains the one real monthly authority over <see
/// cref="SettlementMarket"/>'s supply/demand-derived clearing — this system only ever nudges the price
/// that clearing already produced this month, the same "downstream real-money consequence layered on an
/// already-cleared figure" shape <see cref="Economy.InsolvencySystem"/>'s own forced-liquidation-at-a-
/// below-market-rate already establishes). At Predatory Pricing specifically, also posts §2 rung 3's own
/// real monthly Ledger drain against the aggressor.
///
/// Finally, detects §2 rung 4's own real trigger: an escalation at Predatory Pricing whose target
/// resolves to a <see cref="RealEstate.PropertyOwnerKind.PlayerHousehold"/> owner currently at <see
/// cref="InsolvencyStage.Insolvent"/> or worse auto-advances to <see
/// cref="CompetitiveEscalationRung.ForcedConsolidation"/> — only a household owner is ever checked, since
/// <see cref="InsolvencyState"/> is itself household-scoped (no Insolvency tracking exists for a Rival
/// Gens Actor or a bare Character anywhere in this codebase); a Predatory Pricing campaign against a
/// non-household-owned rival can run forever without ever reaching rung 4, a real, honest narrowing named
/// directly here rather than silently assumed to work for every owner kind.
/// </summary>
public static class CompetitiveEscalationSystem
{
    public static IReadOnlyList<IDomainEvent> Tick(WorldState state, GameDate date)
    {
        var events = new List<IDomainEvent>();

        foreach (var entry in state.CompetitiveEscalations.InAscendingOrder().ToArray())
        {
            var aggressorId = entry.Key;
            var escalation = entry.Value;
            if (escalation.CurrentRung is not (CompetitiveEscalationRung.PriceWar or CompetitiveEscalationRung.PredatoryPricing))
                continue;
            if (!state.NotableBusinesses.TryGet(aggressorId, out var aggressor) || aggressor!.Status != NotableBusinessStatus.Tracked)
                continue;

            ApplyPriceNudge(state, aggressor, escalation.CurrentRung);

            if (escalation.CurrentRung == CompetitiveEscalationRung.PredatoryPricing)
            {
                events.AddRange(ApplyPredatoryDrain(state, date, aggressor, aggressorId));
                events.AddRange(TryDetectForcedConsolidation(state, date, aggressorId, escalation));
            }
        }

        return events;
    }

    private static void ApplyPriceNudge(WorldState state, NotableBusiness aggressor, CompetitiveEscalationRung rung)
    {
        if (aggressor.OutputGoodId is not { } goodId)
            return;
        if (aggressor.DistrictId is not { } districtId || !state.Districts.TryGet(districtId, out var district))
            return;
        var key = new MarketGoodKey(district!.SettlementId, goodId);
        if (!state.MarketPrices.TryGet(key, out var market))
            return;

        var fraction = rung == CompetitiveEscalationRung.PredatoryPricing
            ? BusinessCompetitionCatalog.PredatoryPricingPriceNudgeFraction
            : BusinessCompetitionCatalog.PriceWarPriceNudgeFraction;
        var candidate = market!.Price.Scale(Fixed64.One - fraction);

        // Clamp to Markets' own bounded-price-change rule so this nudge, stacked on whatever move
        // MarketClearingSystem already produced this same month, never itself produces an
        // invariant-violating swing (MarketBoundedPriceChangeInvariantCheck's own +/-15% rule).
        var boundConfig = MarketPriceBoundConfig.Default;
        var lowerBound = market.PreviousPrice.RawValue > 0
            ? market.PreviousPrice.Scale(Fixed64.One - boundConfig.MaxPriceMoveFraction)
            : boundConfig.MinimumPrice;
        var floor = lowerBound > boundConfig.MinimumPrice ? lowerBound : boundConfig.MinimumPrice;
        var newPrice = candidate > floor ? candidate : floor;
        if (newPrice == market.Price)
            return;

        state.MarketPrices.Remove(key);
        state.MarketPrices.Add(key, new SettlementMarket(
            market.SettlementId, market.GoodId, newPrice, market.PreviousPrice, market.Supply, market.Demand,
            market.ClearedQuantity, market.UnsatisfiedDemand));
    }

    private static IDomainEvent[] ApplyPredatoryDrain(
        WorldState state, GameDate date, NotableBusiness aggressor, RuntimeId<NotableBusiness> aggressorId)
    {
        if (!TryResolveOwnerAccount(aggressor.Owner, out var ownerAccount))
            return Array.Empty<IDomainEvent>();

        var ledgerEvent = LedgerService.Post(
            state, date, LedgerTransactionCategory.Transfers,
            new[]
            {
                new LedgerPosting(ownerAccount, -BusinessCompetitionCatalog.PredatoryPricingMonthlyDrain),
                new LedgerPosting(LedgerAccountKey.Mint, BusinessCompetitionCatalog.PredatoryPricingMonthlyDrain),
            },
            reference: $"businessCompetition.predatoryPricingDrain:{aggressorId.ToTaggedString()}");
        return new[] { ledgerEvent };
    }

    private static IDomainEvent[] TryDetectForcedConsolidation(
        WorldState state, GameDate date, RuntimeId<NotableBusiness> aggressorId, CompetitiveEscalation escalation)
    {
        if (!state.NotableBusinesses.TryGet(escalation.BusinessBId, out var target))
            return Array.Empty<IDomainEvent>();
        if (!NotableBusinessOwnerResolver.TryResolveHousehold(target!.Owner, out var targetHouseholdId))
            return Array.Empty<IDomainEvent>();
        if (!state.InsolvencyStates.TryGet(targetHouseholdId, out var insolvency))
            return Array.Empty<IDomainEvent>();
        if (insolvency!.Stage is not (InsolvencyStage.Insolvent or InsolvencyStage.Ruined))
            return Array.Empty<IDomainEvent>();

        state.CompetitiveEscalations.Remove(aggressorId);
        state.CompetitiveEscalations.Add(aggressorId, escalation with { CurrentRung = CompetitiveEscalationRung.ForcedConsolidation });

        return new IDomainEvent[]
        {
            new CompetitiveEscalationChangedEvent(
                state.EventIds.Issue(), date, aggressorId, escalation.BusinessBId,
                CompetitiveEscalationRung.PredatoryPricing, CompetitiveEscalationRung.ForcedConsolidation, CausationId: null),
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

/// <summary>
/// §7's "the winner... is this project's own single most natural buyer for the defeated rival's own
/// now-available Property Record... and gains real Reputation." Player-invoked once an escalation has
/// actually reached <see cref="CompetitiveEscalationRung.ForcedConsolidation"/>
/// (<see cref="CompetitiveEscalationSystem"/>'s own real, automatic detection) — mirrors <see
/// cref="RealEstate.ResolveOperatorBuyoutCommand"/>'s own "settles an accepted offer through <see
/// cref="TransferPropertyCommand"/> itself" shape exactly: the actual property transfer (best-effort —
/// only fires when the loser carries a real <see cref="NotableBusiness.LinkedPropertyRecordId"/>, since
/// most ordinary businesses run out of an untracked Plot per that field's own doc comment) is <see
/// cref="PropertyTransferMethod.ForcedSale"/> to the winner's own <see cref="NotableBusiness.Owner"/>;
/// the real, guaranteed-to-land consequences are the winner's own Reputation gain and the loser's own
/// demotion (matching <see cref="NotableBusinesses.MergeNotableBusinessesCommands"/>'s identical
/// "absorbed business is demoted, not deleted" precedent).
/// </summary>
public sealed record ResolveForcedConsolidationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> WinnerBusinessId) : ICommand;

public sealed record ForcedConsolidationResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> WinnerBusinessId,
    RuntimeId<NotableBusiness> LoserBusinessId,
    bool PropertyAcquired,
    string? CausationId) : IDomainEvent
{
    public string Type => "businessCompetition.forcedConsolidationResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { WinnerBusinessId.ToTaggedString(), LoserBusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class ResolveForcedConsolidationCommands
{
    public static readonly ValidationErrorCode EscalationNotFound = new("businessCompetition.resolveForcedConsolidation.escalationNotFound");
    public static readonly ValidationErrorCode NotAtForcedConsolidation = new("businessCompetition.resolveForcedConsolidation.notAtForcedConsolidation");

    /// <summary>§7's own Reputation award for the winner — sized against <see
    /// cref="NotableBusinesses.NotableBusinessesCatalog.SpecializeReputationBonus"/>'s own magnitude for
    /// a comparably significant, one-time business milestone.</summary>
    public const int WinnerReputationGain = NotableBusinessesCatalog.SpecializeReputationBonus;

    public static readonly CommandPipeline<WorldState, ResolveForcedConsolidationCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ResolveForcedConsolidationCommand command)
    {
        if (!state.CompetitiveEscalations.TryGet(command.WinnerBusinessId, out var escalation))
            return EscalationNotFound;
        if (escalation!.CurrentRung != CompetitiveEscalationRung.ForcedConsolidation)
            return NotAtForcedConsolidation;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ResolveForcedConsolidationCommand command)
    {
        var events = new List<IDomainEvent>();
        state.CompetitiveEscalations.TryGet(command.WinnerBusinessId, out var escalation);
        var loserId = escalation!.BusinessBId;

        state.NotableBusinesses.TryGet(command.WinnerBusinessId, out var winner);
        state.NotableBusinesses.TryGet(loserId, out var loser);

        var propertyAcquired = false;
        if (loser!.LinkedPropertyRecordId is { } propertyRecordId)
        {
            var transferResult = TransferPropertyCommands.Pipeline.Execute(
                state, new TransferPropertyCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    PropertySubjectRef.ForPropertyRecord(propertyRecordId), PropertyTransferMethod.ForcedSale, winner!.Owner));
            if (transferResult.Accepted)
            {
                events.AddRange(transferResult.Events);
                propertyAcquired = true;
            }
        }

        events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.WinnerBusinessId, WinnerReputationGain, BusinessReputationChangeReason.QualityOutput)).Events);

        state.NotableBusinesses.TryGet(loserId, out var loserLatest);
        state.NotableBusinesses.Remove(loserId);
        state.NotableBusinesses.Add(loserId, loserLatest! with { Status = NotableBusinessStatus.Demoted, MainCompetitorBusinessId = null });

        // Matches MergeNotableBusinessesCommand's own "clear the survivor's own now-stale pointer at
        // the absorbed business" precedent — the winner's own Main Competitor pointer would otherwise
        // keep referencing the now-demoted loser.
        state.NotableBusinesses.TryGet(command.WinnerBusinessId, out var winnerLatest);
        if (winnerLatest!.MainCompetitorBusinessId == loserId)
        {
            state.NotableBusinesses.Remove(command.WinnerBusinessId);
            state.NotableBusinesses.Add(command.WinnerBusinessId, winnerLatest with { MainCompetitorBusinessId = null });
        }

        state.CompetitiveEscalations.Remove(command.WinnerBusinessId);

        events.Add(new ForcedConsolidationResolvedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.WinnerBusinessId, loserId, propertyAcquired,
            command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
