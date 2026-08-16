using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly social-mobility tick (Roadmap Phase 7 item 4; <c>gens-settlement-demographics-design.md</c>
/// §5's promotion/demotion pathway graph), excluding the military-gated Veterans transitions:
///
/// <list type="bullet">
/// <item><description>Coloni ⇄ Operarii, Operarii ⇄ Opifices, Opifices ⇄ Negotiatores — pulled by
/// relative Employment Ratio (<see cref="SocialMobilityCalculator.EmploymentPullCount"/>).</description></item>
/// <item><description>Negotiatores/Opifices → Curiales — wealth accumulation, gated on <see
/// cref="WealthBand.EliteDiscretionary"/> and Curiales having open capacity (<see
/// cref="SocialMobilityCalculator.WealthCeilingCount"/>).</description></item>
/// <item><description>Curiales → Operarii/Coloni — rare attrition, split evenly across both landing
/// groups (<see cref="SocialMobilityCalculator.CurialesAttritionCount"/>).</description></item>
/// <item><description>Non-Household Enslaved → Operarii/Opifices — ambient manumission, entering the
/// destination's <see cref="LegalStatusDistribution.Freedman"/> bucket specifically rather than a
/// proportional split (a manumitted person's new legal status is Freedman by definition, not inherited
/// from a source distribution that is always <see cref="LegalStatusDistribution.Empty"/> for this
/// group type, per <see cref="PopGroup"/>'s own NonHouseholdEnslaved carve-out).</description></item>
/// <item><description>Veterans → Coloni — the ordinary drift "absent a recruitment drive" (§5's own
/// phrase; <see cref="SocialMobilityCalculator.VeteranDriftCount"/>).</description></item>
/// </list>
///
/// <b>Deliberately not implemented — a documented hook, not a stub with dead code:</b> §5's Coloni/
/// Operarii → Veterans direction only happens "via Military &amp; Combat service, then discharge — not
/// a direct shift," and Veterans → Active Military is "a temporary call-up... handled by Military &amp;
/// Combat directly." Neither has a driving system yet (Roadmap Phase 7 doesn't include Military &amp;
/// Combat), so this system never moves population into or out of Veterans along that path — Veterans
/// only ever drifts back out to Coloni here. When Military &amp; Combat exists, its own recruitment/
/// discharge commands are the natural place to move Coloni/Operarii Size into Veterans (mirroring how
/// <see cref="PromoteToNamedCommand"/> is the one sanctioned path that moves a PopGroup's Size into a
/// named Character) — not a change to this system.
///
/// Every move uses a per-settlement, in-memory working copy of the settlement's existing <see
/// cref="PopGroup"/> entries (never creating a group type that doesn't already exist there, matching
/// every other Phase 7 system's convention) so a chain of pathways sharing an intermediate group
/// (Coloni → Operarii → Opifices) can't double-count the same population within one tick: each pathway
/// reads the previous pathway's already-applied result, not the tick's original snapshot.
/// </summary>
public sealed class SocialMobilitySystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.socialMobility";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.migration" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            var working = state.PopGroups.InAscendingOrder()
                .Where(entry => entry.Key.SettlementId == settlementId)
                .ToDictionary(entry => entry.Key.GroupType, entry => entry.Value);

            ApplyEmploymentPull(state, working, PopGroupType.Coloni, PopGroupType.Operarii, settlementId, context, events);
            ApplyEmploymentPull(state, working, PopGroupType.Operarii, PopGroupType.Opifices, settlementId, context, events);
            ApplyEmploymentPull(state, working, PopGroupType.Opifices, PopGroupType.Negotiatores, settlementId, context, events);

            ApplyWealthCeiling(state, working, PopGroupType.Negotiatores, settlementId, context, events);
            ApplyWealthCeiling(state, working, PopGroupType.Opifices, settlementId, context, events);

            ApplyCurialesAttrition(state, working, settlementId, context, events);
            ApplyManumission(state, working, settlementId, context, events);
            ApplyVeteranDrift(state, working, settlementId, context, events);

            foreach (var (groupType, group) in working)
            {
                var key = new PopGroupKey(settlementId, groupType);
                state.PopGroups.TryGet(key, out var original);
                if (ReferenceEquals(original, group))
                    continue;
                state.PopGroups.Remove(key);
                state.PopGroups.Add(key, group);
            }
        }

        return events;
    }

    private static void ApplyEmploymentPull(
        WorldState state, Dictionary<PopGroupType, PopGroup> working, PopGroupType a, PopGroupType b,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!working.TryGetValue(a, out var groupA) || !working.TryGetValue(b, out var groupB))
            return;

        var aToB = SocialMobilityCalculator.EmploymentPullCount(groupA.Size, groupA.EmploymentRatio, groupB.EmploymentRatio);
        if (aToB > 0)
        {
            Move(state, working, a, b, aToB, settlementId, context, events);
            return;
        }

        var bToA = SocialMobilityCalculator.EmploymentPullCount(groupB.Size, groupB.EmploymentRatio, groupA.EmploymentRatio);
        if (bToA > 0)
            Move(state, working, b, a, bToA, settlementId, context, events);
    }

    private static void ApplyWealthCeiling(
        WorldState state, Dictionary<PopGroupType, PopGroup> working, PopGroupType source,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!working.TryGetValue(source, out var sourceGroup) || !working.TryGetValue(PopGroupType.Curiales, out var curiales))
            return;

        var count = SocialMobilityCalculator.WealthCeilingCount(sourceGroup.Size, sourceGroup.WealthBand, curiales.EmploymentRatio);
        if (count > 0)
            Move(state, working, source, PopGroupType.Curiales, count, settlementId, context, events);
    }

    private static void ApplyCurialesAttrition(
        WorldState state, Dictionary<PopGroupType, PopGroup> working,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!working.TryGetValue(PopGroupType.Curiales, out var curiales))
            return;

        var total = SocialMobilityCalculator.CurialesAttritionCount(curiales.Size);
        SplitMove(state, working, PopGroupType.Curiales, PopGroupType.Operarii, PopGroupType.Coloni, total, settlementId, context, events);
    }

    private static void ApplyManumission(
        WorldState state, Dictionary<PopGroupType, PopGroup> working,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!working.TryGetValue(PopGroupType.NonHouseholdEnslaved, out var enslaved))
            return;

        var total = SocialMobilityCalculator.ManumissionCount(enslaved.Size);
        SplitMove(
            state, working, PopGroupType.NonHouseholdEnslaved, PopGroupType.Operarii, PopGroupType.Opifices, total, settlementId, context, events,
            asFreedman: true);
    }

    private static void ApplyVeteranDrift(
        WorldState state, Dictionary<PopGroupType, PopGroup> working,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        if (!working.TryGetValue(PopGroupType.Veterans, out var veterans))
            return;

        var count = SocialMobilityCalculator.VeteranDriftCount(veterans.Size);
        if (count > 0)
            Move(state, working, PopGroupType.Veterans, PopGroupType.Coloni, count, settlementId, context, events);
    }

    /// <summary>Splits <paramref name="total"/> evenly across two landing groups (favoring <paramref
    /// name="firstDest"/> on an odd remainder, a fixed, deterministic tie-break), skipping either
    /// landing group that doesn't already exist in this settlement.</summary>
    private static void SplitMove(
        WorldState state, Dictionary<PopGroupType, PopGroup> working, PopGroupType source, PopGroupType firstDest, PopGroupType secondDest,
        int total, RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events, bool asFreedman = false)
    {
        if (total <= 0)
            return;

        var firstShare = (total + 1) / 2;
        var secondShare = total - firstShare;

        if (working.ContainsKey(firstDest) && firstShare > 0)
            Move(state, working, source, firstDest, firstShare, settlementId, context, events, asFreedman);
        if (working.ContainsKey(secondDest) && secondShare > 0)
            Move(state, working, source, secondDest, secondShare, settlementId, context, events, asFreedman);
    }

    private static void Move(
        WorldState state, Dictionary<PopGroupType, PopGroup> working, PopGroupType source, PopGroupType dest, int count,
        RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events, bool asFreedman = false)
    {
        if (count <= 0 || !working.TryGetValue(source, out var sourceGroup) || !working.TryGetValue(dest, out var destGroup))
            return;

        var actualCount = Math.Min(count, sourceGroup.Size);
        if (actualCount <= 0)
            return;

        LegalStatusDistribution movedDistribution;
        LegalStatusDistribution remainingSourceDistribution;
        if (asFreedman)
        {
            movedDistribution = new LegalStatusDistribution(0, 0, 0, actualCount);
            remainingSourceDistribution = sourceGroup.LegalStatusDistribution;
        }
        else
        {
            (movedDistribution, remainingSourceDistribution) = sourceGroup.LegalStatusDistribution.Split(actualCount);
        }

        working[source] = sourceGroup with { Size = sourceGroup.Size - actualCount, LegalStatusDistribution = remainingSourceDistribution };
        working[dest] = destGroup with { Size = destGroup.Size + actualCount, LegalStatusDistribution = destGroup.LegalStatusDistribution + movedDistribution };

        events.Add(new SocialMobilityAppliedEvent(state.EventIds.Issue(), context.Date, settlementId, source, dest, actualCount));
    }
}

/// <summary>Emitted only when <see cref="SocialMobilitySystem"/> actually moves population between two
/// groups this month — a zero-flow pathway has no mobility cause to record, matching <see
/// cref="PopGroupGrowthMortalityAppliedEvent"/>'s identical "only emitted on real change" shape.</summary>
public sealed record SocialMobilityAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    PopGroupType Source,
    PopGroupType Destination,
    int Count) : IDomainEvent
{
    public string Type => "characters.socialMobilityApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
