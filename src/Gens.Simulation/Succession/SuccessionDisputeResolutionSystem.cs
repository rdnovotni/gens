using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>Emitted whenever a <see cref="SuccessionDispute"/> resolves (§5.2).</summary>
public sealed record SuccessionDisputeResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SuccessionDispute> DisputeId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character>? WinnerCharacterId,
    SuccessionDisputeStatus Status) : IDomainEvent
{
    public string Type => "succession.disputeResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => WinnerCharacterId is { } winner
        ? new[] { HouseholdId.ToTaggedString(), winner.ToTaggedString() }
        : new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a losing claimant founds an independent splinter Household (§5.3).</summary>
public sealed record SplinterHouseholdFoundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> OriginHouseholdId,
    RuntimeId<Household> SplinterHouseholdId,
    RuntimeId<Character> FounderCharacterId) : IDomainEvent
{
    public string Type => "succession.splinterHouseholdFounded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { OriginHouseholdId.ToTaggedString(), SplinterHouseholdId.ToTaggedString(), FounderCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// Resolves a <see cref="SuccessionDispute"/> once its <see cref="SuccessionDispute.ResolutionDueDate"/>
/// arrives (Phase 11 item 1; §5.2's "the contest" as a process, collapsed to one deterministic
/// resolution roll for this phase's scope rather than a full Scheme-driven progress race). The winner
/// is whichever surviving claimant scores highest on Core Attributes plus a random tiebreak; ties and a
/// claimant dying while the dispute was open are both handled by simply excluding the dead from
/// scoring. A dispute every one of whose claimants has since died resolves with no winner at all — the
/// next <see cref="SuccessionHandoffSystem"/> tick then re-evaluates the (now-empty) pool itself and
/// extinguishes the Household or falls back to a spouse-in-trust, matching that system's own ordinary
/// path exactly rather than duplicating it here.
///
/// The runner-up claimant has <see cref="SuccessionCatalog.SplinterHouseChancePercent"/> odds of
/// founding an independent splinter Household instead of simply losing (§5.3) — a new Household ID,
/// a <see cref="HouseholdHeadship"/> naming the splinter claimant its head, their own <see
/// cref="Character.Household"/> moved to it, and <see cref="SuccessionCatalog.SplinterHouseAssetSharePercent"/>
/// of the origin Household's Denarii balance transferred over via <see cref="LedgerService.Post"/>
/// (§5.3's "a losing claimant can take a share"). Debt obligations deliberately stay with the origin
/// Household rather than being split — an invented scope limit, not a design requirement.
/// </summary>
public sealed class SuccessionDisputeResolutionSystem : IMonthlySystem<WorldState>
{
    public string Id => "succession.disputeResolution";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "successionDisputes", "characters", "householdHeadships", "ledgerAccounts" };

    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "successionDisputes", "householdHeadships", "heirDesignations", "characters",
        "ledgerAccounts", "ledgerTransactions", "eventIds",
    };

    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "succession.handoff" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.SuccessionDisputes (Remove+Add) mid-iteration.
        var disputes = state.SuccessionDisputes.InAscendingOrder().ToArray();

        foreach (var (disputeId, dispute) in disputes)
        {
            if (dispute.Status != SuccessionDisputeStatus.Pending)
                continue;
            if (dispute.ResolutionDueDate.TotalMonths > context.Date.TotalMonths)
                continue;

            var scored = dispute.ClaimantIds
                .Where(id => state.Characters.TryGet(id, out var claimant) && claimant.IsAlive)
                .Select(id => (Id: id, Score: Score(state, id, context)))
                .OrderByDescending(entry => entry.Score)
                .ToArray();

            if (scored.Length == 0)
            {
                state.SuccessionDisputes.Remove(disputeId);
                state.SuccessionDisputes.Add(disputeId, dispute with { Status = SuccessionDisputeStatus.ResolvedByFavor, WinnerCharacterId = null });
                events.Add(new SuccessionDisputeResolvedEvent(
                    state.EventIds.Issue(), context.Date, disputeId, dispute.HouseholdId, null, SuccessionDisputeStatus.ResolvedByFavor));
                continue;
            }

            var winnerId = scored[0].Id;
            state.HouseholdHeadships.Remove(dispute.HouseholdId);
            state.HouseholdHeadships.Add(dispute.HouseholdId, new HouseholdHeadship(dispute.HouseholdId, winnerId, context.Date));

            var status = SuccessionDisputeStatus.ResolvedByFavor;
            RuntimeId<Character>? splinterClaimantId = null;
            RuntimeId<Household>? splinterHouseholdId = null;

            if (scored.Length > 1 && context.RandomStreams.NextUInt(SplinterStreamName, 100) < SuccessionCatalog.SplinterHouseChancePercent)
            {
                var runnerUpId = scored[1].Id;
                var newHouseholdId = FoundSplinterHousehold(state, dispute.HouseholdId, runnerUpId, context.Date, events);
                status = SuccessionDisputeStatus.ResolvedBySplinter;
                splinterClaimantId = runnerUpId;
                splinterHouseholdId = newHouseholdId;
            }

            state.SuccessionDisputes.Remove(disputeId);
            state.SuccessionDisputes.Add(
                disputeId,
                dispute with
                {
                    Status = status,
                    WinnerCharacterId = winnerId,
                    SplinterClaimantId = splinterClaimantId,
                    SplinterHouseholdId = splinterHouseholdId,
                });

            events.Add(new SuccessionDisputeResolvedEvent(state.EventIds.Issue(), context.Date, disputeId, dispute.HouseholdId, winnerId, status));
        }

        return events;
    }

    private static long Score(WorldState state, RuntimeId<Character> characterId, MonthlyTickContext context)
    {
        state.Characters.TryGet(characterId, out var character);
        var attributes = character.GetEffectiveAttributes();
        var sum = attributes.Diplomacy + attributes.Martial + attributes.Stewardship + attributes.Intrigue + attributes.Learning;
        var tiebreak = context.RandomStreams.NextUInt(ScoringStreamName, 1000);
        return (long)sum * 1000 + tiebreak;
    }

    private static RuntimeId<Household> FoundSplinterHousehold(
        WorldState state, RuntimeId<Household> originHouseholdId, RuntimeId<Character> founderId, GameDate date, List<IDomainEvent> events)
    {
        var newHouseholdId = state.HouseholdIds.Issue();
        state.HouseholdHeadships.Add(newHouseholdId, new HouseholdHeadship(newHouseholdId, founderId, date));

        if (state.Characters.TryGet(founderId, out var founder))
        {
            var relocated = founder with { Household = newHouseholdId };
            state.Characters.Remove(founderId);
            state.Characters.Add(founderId, relocated);
        }

        var originAccountKey = LedgerAccountKey.ForHousehold(originHouseholdId);
        if (state.LedgerAccounts.TryGet(originAccountKey, out var originAccount))
        {
            var share = Money.FromMinorUnits(originAccount.Balance.RawValue * SuccessionCatalog.SplinterHouseAssetSharePercent / 100);
            if (share.RawValue > 0)
            {
                LedgerService.Post(
                    state, date, LedgerTransactionCategory.Transfers,
                    new[]
                    {
                        new LedgerPosting(originAccountKey, -share),
                        new LedgerPosting(LedgerAccountKey.ForHousehold(newHouseholdId), share),
                    },
                    reference: $"splinterHousehold:{newHouseholdId.ToTaggedString()}");
            }
        }

        events.Add(new SplinterHouseholdFoundedEvent(state.EventIds.Issue(), date, originHouseholdId, newHouseholdId, founderId));
        return newHouseholdId;
    }

    /// <summary>The named random stream this system draws from for its per-claimant scoring tiebreak
    /// (§5.2), kept distinct from every other stream for rule 8's "adding a draw in one system must not
    /// perturb another".</summary>
    public const string ScoringStreamName = "succession.disputeScoring";

    /// <summary>The named random stream this system draws from for its runner-up splinter-house roll
    /// (§5.3), kept distinct from <see cref="ScoringStreamName"/> for the same rule-8 reason.</summary>
    public const string SplinterStreamName = "succession.disputeSplinter";
}
