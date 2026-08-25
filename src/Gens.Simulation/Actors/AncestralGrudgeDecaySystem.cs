using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>Emitted when an <see cref="AncestralGrudge"/> finally decays past <see
/// cref="AncestralGrudgeCatalog.DecayMonths"/> — chronicle-worthy in its own right (§5.2's "for
/// generations" framing means this is a rare, notable occurrence), so kept public like <see
/// cref="HouseStandingChangedEvent"/>.</summary>
public sealed record AncestralGrudgeDecayedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Actor> ActorAId,
    RuntimeId<Actor> ActorBId) : IDomainEvent
{
    public string Type => "actors.ancestralGrudgeDecayed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ActorAId.ToTaggedString(), ActorBId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly cleanup pass clearing an <see cref="AncestralGrudge"/> once it has gone <see
/// cref="AncestralGrudgeCatalog.DecayMonths"/> without being renewed (Phase 10 item 5). This is a
/// proactive sweep rather than the only place grudge expiry is checked — <see
/// cref="AdjustHouseStandingCommands.Validate"/> and <see cref="RivalHouseActionDefinitions"/>'s own
/// eligibility check both independently compute <see cref="AncestralGrudgeCatalog.IsActive"/> from the
/// stored <see cref="AncestralGrudge.OriginDate"/> rather than relying on this system having already
/// run in the same month, so correctness never depends on tick ordering between the two. Clearing the
/// grudge only removes the modifier itself — the underlying <see cref="HouseStanding.Standing"/> level
/// is untouched; two houses can still simply be Feuding on its own separate merits.
/// </summary>
public sealed class AncestralGrudgeDecaySystem : IMonthlySystem<WorldState>
{
    public string Id => "actors.ancestralGrudgeDecay";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "houseStandings" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "houseStandings" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: the loop body replaces entries in state.HouseStandings mid-iteration,
        // matching RelationshipDecaySystem's identical "snapshot before mutating" guard.
        var standings = state.HouseStandings.InAscendingOrder().ToArray();

        foreach (var entry in standings)
        {
            if (entry.Value.Grudge is not { } grudge || AncestralGrudgeCatalog.IsActive(context.Date, grudge))
                continue;

            state.HouseStandings.Remove(entry.Key);
            state.HouseStandings.Add(entry.Key, entry.Value with { Grudge = null });
            events.Add(new AncestralGrudgeDecayedEvent(state.EventIds.Issue(), context.Date, entry.Key.ActorAId, entry.Key.ActorBId));
        }

        return events;
    }
}
