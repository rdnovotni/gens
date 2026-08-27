using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>Why a <see cref="HouseholdHeadship"/> changed hands.</summary>
public enum HandoffTrigger
{
    /// <summary>The chosen heir inherited outright (§6.1).</summary>
    OrdinaryInheritance,

    /// <summary>A minor heir's estate passed to a surviving spouse in trust rather than the minor
    /// directly (§3, §6.2).</summary>
    RegencyInTrust,
}

/// <summary>Emitted whenever a Household's headship passes to a new Character (Phase 11 item 1; §6).</summary>
public sealed record HouseholdHeadTransferredEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> FromCharacterId,
    RuntimeId<Character> ToCharacterId,
    HandoffTrigger Trigger) : IDomainEvent
{
    public string Type => "succession.householdHeadTransferred";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), FromCharacterId.ToTaggedString(), ToCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a Household's line runs out entirely (Phase 11 item 1; §7.1's "Realistic"
/// default: "if no eligible heir/adoption candidate exists and everyone's dead, the line genuinely
/// ends"). §7.2's Safety Net/Extinction Off accessibility toggles are Open Questions this
/// implementation does not build — every campaign runs under the Realistic default.</summary>
public sealed record HouseholdExtinguishedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> LastHeadCharacterId) : IDomainEvent
{
    public string Type => "succession.householdExtinguished";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), LastHeadCharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>Emitted whenever a dead head's estate opens a <see cref="SuccessionDispute"/> instead of
/// an immediate handoff (§5.1-§5.2).</summary>
public sealed record SuccessionDisputeOpenedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SuccessionDispute> DisputeId,
    RuntimeId<Household> HouseholdId,
    IReadOnlyList<RuntimeId<Character>> ClaimantIds) : IDomainEvent
{
    public string Type => "succession.disputeOpened";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() }.Concat(ClaimantIds.Select(id => id.ToTaggedString())).ToArray();
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}

/// <summary>
/// The monthly check that hands a dead head's Household on to their heir (Phase 11 item 1;
/// <c>gens-succession-dynasty-design.md</c> §6-§7). For every tracked <see cref="HouseholdHeadship"/>
/// whose current head has died and has no <see cref="SuccessionDispute"/> already pending:
///
/// <list type="number">
/// <item>Resolve the eligible-heir pool (<see cref="HeirEligibilityService"/>) and, from it, the
/// designated heir — a still-eligible Formal Declaration first, then a still-eligible preference,
/// then §2.4's default agnatic-line order.</item>
/// <item>More than one eligible heir and no Formal Declaration to settle it opens a <see
/// cref="SuccessionDispute"/> instead (§5.1-§5.2) — resolved later by <see
/// cref="SuccessionDisputeResolutionSystem"/>. Headship is left pointing at the dead head until then;
/// <see cref="HasPendingDispute"/> is this system's own re-entrancy guard against reopening it every
/// tick.</item>
/// <item>A minor chosen heir with a living spouse-in-trust hands the estate to that spouse instead
/// (§3, §6.2) — a coarse stand-in for §6.2's fuller Regency machinery (a Rationalis/Procurator
/// appointee via <see cref="Stewardship.StewardshipAssignment"/>), out of this item's scope.</item>
/// <item>No eligible heir and no spouse-in-trust extinguishes the Household outright (§7.1).</item>
/// </list>
///
/// Asset and obligation transfer (the roadmap item's own phrase) needs no separate mechanism here:
/// <see cref="Ledger.LedgerAccountKey.ForHousehold"/> and <see cref="Economy.DebtRecord.DebtorHouseholdId"/>
/// are already keyed to the Household, not the head Character, so a plain headship change already
/// carries every Denarii balance and standing obligation forward to the new head untouched — this is
/// this codebase's existing ledger model doing §6's "asset and obligation transfer" for free. Only §5.3's
/// splinter-house founding actually moves money between accounts; see <see
/// cref="SuccessionDisputeResolutionSystem"/> for that.
/// </summary>
public sealed class SuccessionHandoffSystem : IMonthlySystem<WorldState>
{
    public string Id => "succession.handoff";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "householdHeadships", "heirDesignations", "characters", "successionDisputes" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "householdHeadships", "successionDisputes", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body mutates state.HouseholdHeadships (Remove+Add) mid-iteration.
        var headships = state.HouseholdHeadships.InAscendingOrder().ToArray();

        foreach (var (householdId, headship) in headships)
        {
            if (!state.Characters.TryGet(headship.HeadCharacterId, out var head) || head!.IsAlive)
                continue;
            if (HasPendingDispute(state, householdId))
                continue;

            state.HeirDesignations.TryGet(householdId, out var designation);
            var pool = HeirEligibilityService.EligibleHeirs(state, headship.HeadCharacterId, designation);
            var chosen = ChooseHeir(designation, pool);

            if (chosen is null)
            {
                var spouse = HeirEligibilityService.SurvivingSpouse(state, head);
                if (spouse is { } spouseId)
                {
                    Transfer(state, householdId, headship, spouseId, HandoffTrigger.RegencyInTrust, context.Date, events);
                    continue;
                }

                state.HouseholdHeadships.Remove(householdId);
                events.Add(new HouseholdExtinguishedEvent(state.EventIds.Issue(), context.Date, householdId, headship.HeadCharacterId));
                continue;
            }

            var declaredAndStillEligible = designation?.FormallyDeclaredHeirId is { } declared && pool.Contains(declared);
            if (pool.Count > 1 && !declaredAndStillEligible &&
                context.RandomStreams.NextUInt(DisputeTriggerStreamName, 100) < SuccessionCatalog.DisputeTriggerChancePercent)
            {
                var disputeId = state.SuccessionDisputeIds.Issue();
                var dispute = new SuccessionDispute(
                    disputeId, householdId, headship.HeadCharacterId, pool, context.Date,
                    new GameDate(context.Date.TotalMonths + SuccessionCatalog.DisputeResolutionMonths),
                    SuccessionDisputeStatus.Pending, WinnerCharacterId: null, SplinterClaimantId: null, SplinterHouseholdId: null);
                state.SuccessionDisputes.Add(disputeId, dispute);
                events.Add(new SuccessionDisputeOpenedEvent(state.EventIds.Issue(), context.Date, disputeId, householdId, pool));
                continue;
            }

            var isMinor = HeirEligibilityService.IsMinor(state, chosen.Value, context.Date);
            if (isMinor)
            {
                var spouse = HeirEligibilityService.SurvivingSpouse(state, head);
                if (spouse is { } spouseId)
                {
                    Transfer(state, householdId, headship, spouseId, HandoffTrigger.RegencyInTrust, context.Date, events, futureHeirId: chosen.Value);
                    continue;
                }
            }

            Transfer(state, householdId, headship, chosen.Value, HandoffTrigger.OrdinaryInheritance, context.Date, events);
        }

        return events;
    }

    /// <summary>A still-eligible Formal Declaration first (§2.2), then a still-eligible preference
    /// (§2.1), then §2.4's default agnatic-line order (the pool's own ordering).</summary>
    private static RuntimeId<Character>? ChooseHeir(HeirDesignation? designation, IReadOnlyList<RuntimeId<Character>> pool)
    {
        if (designation?.FormallyDeclaredHeirId is { } declared && pool.Contains(declared))
            return declared;
        if (designation?.PreferredHeirId is { } preferred && pool.Contains(preferred))
            return preferred;
        return pool.Count > 0 ? pool[0] : null;
    }

    private static bool HasPendingDispute(WorldState state, RuntimeId<Household> householdId)
    {
        foreach (var (_, dispute) in state.SuccessionDisputes.InAscendingOrder())
            if (dispute.HouseholdId == householdId && dispute.Status == SuccessionDisputeStatus.Pending)
                return true;
        return false;
    }

    /// <param name="futureHeirId">When set, <paramref name="newHeadId"/> is a Regent holding the
    /// estate in trust for this still-minor blood/adopted heir (§6.2) — recorded only as metadata on
    /// the event; a future Regency-ends-when-the-heir-comes-of-age system is out of this item's scope.</param>
    private static void Transfer(
        WorldState state, RuntimeId<Household> householdId, HouseholdHeadship previous, RuntimeId<Character> newHeadId,
        HandoffTrigger trigger, GameDate date, List<IDomainEvent> events, RuntimeId<Character>? futureHeirId = null)
    {
        state.HouseholdHeadships.Remove(householdId);
        state.HouseholdHeadships.Add(
            householdId,
            new HouseholdHeadship(householdId, futureHeirId ?? newHeadId, date, futureHeirId is not null ? newHeadId : null));

        events.Add(new HouseholdHeadTransferredEvent(
            state.EventIds.Issue(), date, householdId, previous.HeadCharacterId, futureHeirId ?? newHeadId, trigger));
    }

    /// <summary>The named random stream this system draws from for its succession-drama trigger roll
    /// (§5.1), kept distinct from every other stream for rule 8's "adding a draw in one system must not
    /// perturb another".</summary>
    public const string DisputeTriggerStreamName = "succession.disputeTrigger";
}
