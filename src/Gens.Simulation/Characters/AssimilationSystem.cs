using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly assimilation tick (Roadmap Phase 7 item 4, the last hook in this item's chain;
/// <c>gens-settlement-demographics-design.md</c> §10). For every <see cref="PopGroup"/>, shifts its
/// <see cref="PopGroup.LegalStatusDistribution"/> toward higher status via <see
/// cref="AssimilationCalculator.Apply"/>. Never touches <see cref="PopGroup.Size"/> — this is purely a
/// composition shift within the group's existing free-status population, not a population change, so
/// Roadmap item 6's "every population change has a cause" doesn't apply here the way it does to
/// growth/mortality, migration, or social mobility. <see cref="PopGroupType.NonHouseholdEnslaved"/>
/// needs no special-case skip: its distribution is always <see cref="LegalStatusDistribution.Empty"/>
/// by construction (<see cref="PopGroup"/>'s own carve-out), so <see
/// cref="AssimilationCalculator.Apply"/> is naturally a no-op on it. The military-service fast
/// multiplier (§10) is a documented hook on <see cref="AssimilationCalculator"/> itself, not code here
/// — see that class's own doc comment.
/// </summary>
public sealed class AssimilationSystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.assimilation";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.socialMobility" };

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

            var newDistribution = AssimilationCalculator.Apply(group.LegalStatusDistribution);
            if (newDistribution == group.LegalStatusDistribution)
                continue;

            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group with { LegalStatusDistribution = newDistribution });
            events.Add(new AssimilationAppliedEvent(state.EventIds.Issue(), context.Date, key.SettlementId, key.GroupType, newDistribution));
        }

        return events;
    }
}

/// <summary>Emitted only when <see cref="AssimilationSystem"/> actually shifts a group's <see
/// cref="LegalStatusDistribution"/> this month — matching <see
/// cref="PopGroupGrowthMortalityAppliedEvent"/>'s identical "only emitted on real change" shape.</summary>
public sealed record AssimilationAppliedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    PopGroupType GroupType,
    LegalStatusDistribution NewDistribution) : IDomainEvent
{
    public string Type => "characters.assimilationApplied";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
