using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Fame;

/// <summary>
/// The monthly Fame decay tick (Phase 12 item 8; Games &amp; Spectacle §2's "decays slowly if
/// genuinely inactive," reused directly per §1's own "this document doesn't touch that foundation"
/// framing). Applies a flat monthly decay to every stored <see cref="CharacterFame"/> balance,
/// clamped at the floor of 0 by <see cref="FameResolver.Apply"/> itself, matching <see
/// cref="Clientela.InfluenceCycleSystem"/>'s identical "no per-source last-touched timestamp exists,
/// so decay applies to every stored balance uniformly rather than only a genuinely inactive one"
/// scope note — inventing a "last generated" timestamp duplicates state this item doesn't otherwise
/// need. Runs in <see cref="TickPhase.RelationshipsActors"/>, the same phase <see
/// cref="Clientela.InfluenceCycleSystem"/> and <see cref="Reputation.FavorExpirationSystem"/> run in.
/// </summary>
public sealed class FameDecaySystem : IMonthlySystem<WorldState>
{
    public string Id => "fame.decay";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "characterFames" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "characterFames" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        // Materialize the key set first — FameResolver.Apply mutates state.CharacterFames mid-scan
        // otherwise, matching InfluenceCycleSystem's identical precaution.
        foreach (var characterId in state.CharacterFames.InAscendingOrder().Select(entry => entry.Key).ToArray())
            FameResolver.Apply(state, characterId, -FameCatalog.DecayPerMonth);

        // A quiet resource drift, matching InfluenceCycleSystem's identical "no per-tick event for a
        // number that already reads directly off FameResolver" precedent.
        return Array.Empty<IDomainEvent>();
    }
}
