using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly immigration/emigration tick (Roadmap Phase 7 item 4; <c>gens-settlement-demographics-
/// design.md</c> §8). Two independent passes per Settlement:
///
/// <list type="bullet">
/// <item><description><b>Emigration</b> (§8.2): every existing <see cref="PopGroup"/> whose <see
/// cref="PopGroup.Contentment"/> is below <see cref="MigrationCalculator.EmigrationRate"/>'s threshold
/// loses population, resolved via <see cref="StochasticRounding.RoundedDelta"/> and removed
/// proportionally from <see cref="PopGroup.LegalStatusDistribution"/> (<see
/// cref="LegalStatusDistribution.Shrink"/>) — they leave the simulated settlement entirely (not routed
/// to another Settlement; no cross-settlement travel/logistics model exists yet), a documented, real
/// population loss with this event as its cause (Roadmap item 6's "every population change has a
/// cause").</description></item>
/// <item><description><b>Immigration</b> (§8.1): only when the settlement already carries at least one
/// of its default entry-point groups (Coloni or Operarii, §8.1/§5's "new population... enters
/// overwhelmingly as Coloni or Operarii") does this system compute an ambient pull off that entry
/// point's own Employment Ratio/Housing Satisfaction and the settlement's size-weighted average
/// Contentment, split the resulting new arrivals evenly across whichever of Coloni/Operarii exist
/// (favoring Coloni on an odd remainder — an arbitrary but fixed, deterministic tie-break), and enters
/// them as unassimilated (<see cref="LegalStatusDistribution.WithNewEntrants"/>, §10). A settlement
/// with neither group registered yet cannot receive ordinary immigration through this system — event-
/// driven refugee waves and veteran settlement waves (§8.1's other two entry flavors) are Events-system
/// scope, not this ambient tick's.</description></item>
/// </list>
/// </summary>
public sealed class MigrationSystem : IMonthlySystem<WorldState>
{
    private static readonly PopGroupType[] DefaultEntryGroups = { PopGroupType.Coloni, PopGroupType.Operarii };

    private readonly string _emigrationStreamName;
    private readonly string _immigrationStreamName;

    /// <param name="emigrationStreamName">The named stream this system draws its emigration rounding
    /// roll from.</param>
    /// <param name="immigrationStreamName">The named stream this system draws its immigration
    /// rounding roll from — kept distinct from <paramref name="emigrationStreamName"/> so a change to
    /// one draw sequence never perturbs the other (rule 8), matching <see
    /// cref="LaborFlightSystem"/>'s identical two-stream shape.</param>
    public MigrationSystem(string emigrationStreamName, string immigrationStreamName)
    {
        if (string.IsNullOrWhiteSpace(emigrationStreamName))
            throw new ArgumentException("An emigration random stream name is required.", nameof(emigrationStreamName));
        if (string.IsNullOrWhiteSpace(immigrationStreamName))
            throw new ArgumentException("An immigration random stream name is required.", nameof(immigrationStreamName));
        _emigrationStreamName = emigrationStreamName;
        _immigrationStreamName = immigrationStreamName;
    }

    public string Id => "characters.migration";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.growthMortality" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            ApplyEmigration(state, settlementId, context, events);
            ApplyImmigration(state, settlementId, context, events);
        }

        return events;
    }

    private void ApplyEmigration(
        WorldState state, RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var groupKeys = state.PopGroups.InAscendingOrder()
            .Where(entry => entry.Key.SettlementId == settlementId)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var key in groupKeys)
        {
            if (!state.PopGroups.TryGet(key, out var group))
                continue;

            // MigrationCalculator.EmigrationRate returns a positive magnitude (how much the group
            // loses); negate it here so RoundedDelta produces an outflow, not a phantom gain.
            var rate = -MigrationCalculator.EmigrationRate(group.Contentment);
            var delta = StochasticRounding.RoundedDelta(group.Size, rate, context.RandomStreams, _emigrationStreamName);
            if (delta >= 0)
                continue;

            var newSize = Math.Max(0, group.Size + delta);
            var actualLeft = group.Size - newSize;
            if (actualLeft == 0)
                continue;

            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group with { Size = newSize, LegalStatusDistribution = group.LegalStatusDistribution.Shrink(actualLeft) });
            events.Add(new MigrationAppliedEvent(state.EventIds.Issue(), context.Date, settlementId, key.GroupType, MigrationDirection.Emigration, actualLeft, newSize));
        }
    }

    private void ApplyImmigration(
        WorldState state, RuntimeId<Settlement> settlementId, MonthlyTickContext context, List<IDomainEvent> events)
    {
        var entryGroups = DefaultEntryGroups
            .Select(type => (Type: type, Key: new PopGroupKey(settlementId, type)))
            .Where(entry => state.PopGroups.TryGet(entry.Key, out _))
            .ToArray();
        if (entryGroups.Length == 0)
            return;

        var settlementContentment = SizeWeightedAverageContentment(state, settlementId);
        var (entryEmploymentRatio, entryHousingSatisfaction) = AverageEntryOpportunity(state, entryGroups.Select(e => e.Key));
        var rate = MigrationCalculator.ImmigrationRate(settlementContentment, entryEmploymentRatio, entryHousingSatisfaction);

        var totalSettlementPopulation = state.PopGroups.InAscendingOrder()
            .Where(entry => entry.Key.SettlementId == settlementId)
            .Sum(entry => entry.Value.Size);

        var arrivals = StochasticRounding.RoundedDelta(totalSettlementPopulation, rate, context.RandomStreams, _immigrationStreamName);
        if (arrivals <= 0)
            return;

        // Split evenly across whichever default entry groups exist; an odd remainder favors Coloni —
        // an arbitrary but fixed, deterministic tie-break (DefaultEntryGroups' own declared order).
        var shares = new int[entryGroups.Length];
        var baseShare = arrivals / entryGroups.Length;
        var remainder = arrivals - baseShare * entryGroups.Length;
        for (var i = 0; i < entryGroups.Length; i++)
            shares[i] = baseShare + (i < remainder ? 1 : 0);

        for (var i = 0; i < entryGroups.Length; i++)
        {
            if (shares[i] == 0)
                continue;
            var key = entryGroups[i].Key;
            state.PopGroups.TryGet(key, out var group);
            var newSize = group.Size + shares[i];
            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group with { Size = newSize, LegalStatusDistribution = group.LegalStatusDistribution.WithNewEntrants(shares[i]) });
            events.Add(new MigrationAppliedEvent(state.EventIds.Issue(), context.Date, settlementId, key.GroupType, MigrationDirection.Immigration, shares[i], newSize));
        }
    }

    private static Fixed64 SizeWeightedAverageContentment(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var totalSize = 0L;
        var weightedSum = Fixed64.Zero;
        foreach (var entry in state.PopGroups.InAscendingOrder())
        {
            if (entry.Key.SettlementId != settlementId || entry.Value.Size == 0)
                continue;
            weightedSum += Fixed64.Multiply(entry.Value.Contentment, Fixed64.FromInt(entry.Value.Size));
            totalSize += entry.Value.Size;
        }

        return totalSize > 0 ? Fixed64.Divide(weightedSum, Fixed64.FromInt((int)totalSize)) : Fixed64.Zero;
    }

    private static (Fixed64 EmploymentRatio, Fixed64 HousingSatisfaction) AverageEntryOpportunity(
        WorldState state, IEnumerable<PopGroupKey> entryKeys)
    {
        var keys = entryKeys.ToArray();
        var employmentSum = Fixed64.Zero;
        var housingSum = Fixed64.Zero;
        foreach (var key in keys)
        {
            state.PopGroups.TryGet(key, out var group);
            employmentSum += group.EmploymentRatio;
            housingSum += group.HousingSatisfaction;
        }

        var count = Fixed64.FromInt(keys.Length);
        return (Fixed64.Divide(employmentSum, count), Fixed64.Divide(housingSum, count));
    }
}

/// <summary>Which way population moved in a <see cref="MigrationAppliedEvent"/>.</summary>
public enum MigrationDirection
{
    Immigration,
    Emigration,
}

/// <summary>Emitted only when <see cref="MigrationSystem"/> actually moves population for one group
/// this month — a zero-flow month has no migration cause to record, matching <see
/// cref="PopGroupGrowthMortalityAppliedEvent"/>'s identical "only emitted on real change" shape.</summary>
public sealed record MigrationAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    PopGroupType GroupType,
    MigrationDirection Direction,
    int Count,
    int NewSize) : IDomainEvent
{
    public string Type => "characters.migrationApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
