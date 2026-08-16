using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly background-population growth/mortality tick (Roadmap Phase 7 item 4). For every <see
/// cref="PopGroup"/>, applies <see cref="GrowthMortalityCalculator.MonthlyNetRate"/> (from its
/// already-computed <see cref="PopGroup.Contentment"/>/<see cref="PopGroup.HealthExposure"/>) to <see
/// cref="PopGroup.Size"/>, resolving the fractional remainder via <see
/// cref="StochasticRounding.RoundedDelta"/> so this system's own randomness — unlike named-character
/// mortality's per-individual roll (<see cref="MortalityCalculator"/>/<see
/// cref="CharacterLifecycleSystem"/>) — decides only rounding, not whether the change happens at all;
/// a background aggregate has no individuals to roll for (§9's "background labor deliberately doesn't
/// get its own... mechanic" reasoning, extended here to lifecycle). Every unit gained enters as an
/// unassimilated new member (<see cref="LegalStatusDistribution.WithNewEntrants"/>, same entry rule as
/// migration, §10); every unit lost is removed proportionally across the existing distribution (<see
/// cref="LegalStatusDistribution.Shrink"/>) so <see cref="PopGroup.Size"/> and <see
/// cref="PopGroup.LegalStatusDistribution"/> keep moving together, per <see cref="PopGroup"/>'s own
/// doc comment. <see cref="PopGroup.Size"/> is clamped at zero — a group's own Contentment/
/// HealthExposure can shrink it to nothing but never past it.
/// </summary>
public sealed class GrowthMortalitySystem : IMonthlySystem<WorldState>
{
    private readonly string _roundingStreamName;

    /// <param name="roundingStreamName">The named <see cref="Random.RandomStreamSet"/> stream this
    /// system draws its rounding roll from — supplied by the caller (see <see
    /// cref="Campaign.CampaignBootstrapper.PopGroupGrowthMortalityStreamName"/>) rather than
    /// hardcoded, matching <see cref="CharacterLifecycleSystem"/>'s identical constructor
    /// shape.</param>
    public GrowthMortalitySystem(string roundingStreamName)
    {
        if (string.IsNullOrWhiteSpace(roundingStreamName))
            throw new ArgumentException("A growth/mortality random stream name is required.", nameof(roundingStreamName));
        _roundingStreamName = roundingStreamName;
    }

    public string Id => "characters.growthMortality";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.contentment" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.PopGroups.InAscendingOrder().ToArray())
        {
            var key = entry.Key;
            if (!state.PopGroups.TryGet(key, out var group))
                continue;

            var rate = GrowthMortalityCalculator.MonthlyNetRate(group.Contentment, group.HealthExposure);
            var delta = StochasticRounding.RoundedDelta(group.Size, rate, context.RandomStreams, _roundingStreamName);
            if (delta == 0)
                continue;

            var newSize = Math.Max(0, group.Size + delta);
            var actualDelta = newSize - group.Size;
            if (actualDelta == 0)
                continue;

            var newDistribution = actualDelta > 0
                ? group.LegalStatusDistribution.WithNewEntrants(actualDelta)
                : group.LegalStatusDistribution.Shrink(-actualDelta);

            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group with { Size = newSize, LegalStatusDistribution = newDistribution });

            events.Add(new PopGroupGrowthMortalityAppliedEvent(
                state.EventIds.Issue(), context.Date, key.SettlementId, key.GroupType, actualDelta, newSize));
        }

        return events;
    }
}

/// <summary>Emitted only when <see cref="GrowthMortalitySystem"/> actually changes a <see
/// cref="PopGroup.Size"/> this month — unlike <see cref="BackgroundJobCapacityComputedEvent"/>'s
/// "always emitted" convention, a zero-delta month has no population-change cause to record.</summary>
public sealed record PopGroupGrowthMortalityAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    PopGroupType GroupType,
    int Delta,
    int NewSize) : IDomainEvent
{
    public string Type => "characters.popGroupGrowthMortalityApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
