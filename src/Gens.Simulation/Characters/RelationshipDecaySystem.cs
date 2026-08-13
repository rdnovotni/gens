using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly relationship decay tick (Phase 5 item 5): every tick a <see cref="Relationship"/> goes
/// untouched, its <see cref="Relationship.Opinion"/> drifts one point toward neutral (0) — passing
/// sentiment fades without upkeep, the same "use it or lose it" logic <see
/// cref="CharacterLifecycleSystem"/>'s Fatigue recovery already applies to a different stat. A record
/// that decays all the way to <see cref="Relationship.IsEmpty"/> is pruned outright rather than kept
/// as a permanent zero-opinion, no-bond entry, keeping the store sparse (Phase 5 item 5's own name)
/// even across a long campaign full of one-off encounters. Nothing in the design corpus specifies a
/// numeric decay rate or which bonds should resist it — both are this implementation's own invented
/// baseline, matching <see cref="CharacterLifecycleSystem"/>'s identical disclaimer for its own
/// invented Fatigue/Health rates.
/// </summary>
public sealed class RelationshipDecaySystem : IMonthlySystem<WorldState>
{
    /// <summary>Opinion points moved toward zero per month a relationship goes without a meaningful
    /// interaction.</summary>
    private const int MonthlyOpinionDecay = 1;

    /// <summary>Bonds that represent a structural or sustained-commitment tie rather than passing
    /// sentiment — family relations (§2.7's "structural, not opinion-based") and Contubernium. A
    /// relationship carrying any of these is exempt from opinion decay entirely: a parent's opinion of
    /// a distant child doesn't quietly erode to neutral just because they haven't spoken this month.</summary>
    private const BondTag StickyBonds =
        BondTag.Parent | BondTag.Child | BondTag.Sibling | BondTag.Spouse | BondTag.Contubernium;

    public string Id => "characters.relationshipDecay";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "relationships" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "relationships" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // Materialize first: the loop body mutates state.Relationships (Remove, and possibly Add)
        // mid-iteration, matching CharacterLifecycleSystem's identical "snapshot before mutating"
        // guard against invalidating the enumerator.
        var relationships = state.Relationships.InAscendingOrder().ToArray();

        foreach (var entry in relationships)
        {
            var key = entry.Key;
            var relationship = entry.Value;

            // Just interacted this same tick, or the relationship was formed this same tick — nothing
            // to decay yet.
            if (relationship.LastMeaningfulInteractionDate.TotalMonths >= context.Date.TotalMonths)
                continue;

            if ((relationship.Bonds & StickyBonds) != BondTag.None)
                continue;

            if (relationship.Opinion == 0)
            {
                if (relationship.Bonds == BondTag.None)
                    state.Relationships.Remove(key);
                continue;
            }

            var decayedOpinion = relationship.Opinion > 0
                ? Math.Max(0, relationship.Opinion - MonthlyOpinionDecay)
                : Math.Min(0, relationship.Opinion + MonthlyOpinionDecay);

            var decayed = new Relationship(
                decayedOpinion, relationship.Bonds, relationship.Origin, relationship.FormedDate,
                relationship.LastMeaningfulInteractionDate, relationship.ProvenanceEventId);
            state.Relationships.Remove(key);
            if (!decayed.IsEmpty)
                state.Relationships.Add(key, decayed);
        }

        // Decay is a silent background drift, not an event-worthy occurrence — matching how
        // CharacterLifecycleSystem's own monthly Fatigue recovery emits nothing either; only the
        // occasional stage change or death does.
        return Array.Empty<IDomainEvent>();
    }
}
