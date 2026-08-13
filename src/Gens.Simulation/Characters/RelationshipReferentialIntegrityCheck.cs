using Gens.Simulation.State;

namespace Gens.Simulation.Characters;

/// <summary>
/// A Phase 5 item 8 property (referential integrity, applied to the relationship web specifically):
/// every <see cref="Relationship"/> must reference two distinct, actually-registered Characters. This
/// deliberately does not check for symmetry — a directed relationship existing in only one direction,
/// or with a different opinion in each direction, is the documented model (Phase 5 item 5's "sparse
/// directed records"), not a defect this check should ever flag.
/// </summary>
public sealed class RelationshipReferentialIntegrityCheck : IInvariantCheck
{
    public string Id => "characters.relationships.referentialIntegrity";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.Relationships.InAscendingOrder())
        {
            var key = entry.Key;

            if (key.From == key.To)
            {
                yield return new InvariantViolation(
                    Id,
                    $"Relationship '{key.From.ToTaggedString()}' -> '{key.To.ToTaggedString()}' is self-directed.",
                    new[] { key.From.ToTaggedString() });
                continue;
            }

            if (!state.Characters.TryGet(key.From, out _))
                yield return new InvariantViolation(
                    Id,
                    $"Relationship references missing Character '{key.From.ToTaggedString()}' as its source.",
                    new[] { key.From.ToTaggedString(), key.To.ToTaggedString() });

            if (!state.Characters.TryGet(key.To, out _))
                yield return new InvariantViolation(
                    Id,
                    $"Relationship references missing Character '{key.To.ToTaggedString()}' as its target.",
                    new[] { key.From.ToTaggedString(), key.To.ToTaggedString() });
        }
    }
}
