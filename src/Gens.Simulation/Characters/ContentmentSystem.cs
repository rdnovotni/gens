using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly contentment tick (Roadmap Phase 7 item 4; <c>gens-settlement-demographics-design.md</c>
/// §6.2/§7.3). For every <see cref="PopGroup"/>, combines its already-computed <see
/// cref="PopGroup.EmploymentRatio"/> and <see cref="PopGroup.HousingSatisfaction"/> (both written
/// earlier this same tick phase by <see cref="JobCapacitySystem"/> and <see
/// cref="HousingCapacitySystem"/>) with <see cref="NeedsConsumptionCalculator.SatisfactionFor"/> into
/// <see cref="PopGroup.Contentment"/> (<see cref="ContentmentCalculator.ComputeContentment"/>), and
/// derives <see cref="PopGroup.HealthExposure"/> from housing overcrowding (§7.3, <see
/// cref="ContentmentCalculator.ComputeHealthExposure"/>).
/// </summary>
public sealed class ContentmentSystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.contentment";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.housingCapacity" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            var lines = new List<ContentmentLine>();

            foreach (var entry in state.PopGroups.InAscendingOrder().Where(entry => entry.Key.SettlementId == settlementId).ToArray())
            {
                var key = entry.Key;
                if (!state.PopGroups.TryGet(key, out var group))
                    continue;

                var needsSatisfaction = NeedsConsumptionCalculator.SatisfactionFor(group.NeedsProfile);
                var contentment = ContentmentCalculator.ComputeContentment(group.EmploymentRatio, group.HousingSatisfaction, needsSatisfaction);
                var healthExposure = ContentmentCalculator.ComputeHealthExposure(group.HousingSatisfaction);

                if (contentment != group.Contentment || healthExposure != group.HealthExposure)
                {
                    state.PopGroups.Remove(key);
                    state.PopGroups.Add(key, group with { Contentment = contentment, HealthExposure = healthExposure });
                }

                lines.Add(new ContentmentLine(key.GroupType, contentment, healthExposure));
            }

            events.Add(new ContentmentComputedEvent(state.EventIds.Issue(), context.Date, settlementId, lines));
        }

        return events;
    }
}

/// <summary>One Settlement's computed Contentment/HealthExposure for one pop group this month (§6.2,
/// §7.3), part of <see cref="ContentmentComputedEvent"/>.</summary>
public sealed record ContentmentLine(PopGroupType GroupType, Fixed64 Contentment, Fixed64 HealthExposure);

/// <summary>Emitted for every Settlement, every month, whether or not any group's Contentment changed —
/// ledger-ready groundwork matching <see cref="BackgroundJobCapacityComputedEvent"/>'s identical
/// "always emitted" convention.</summary>
public sealed record ContentmentComputedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    IReadOnlyList<ContentmentLine> Lines) : IDomainEvent
{
    public string Type => "characters.contentmentComputed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
