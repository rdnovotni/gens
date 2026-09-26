using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.State;

namespace Gens.Simulation.Romance;

/// <summary>
/// Mirrors <see cref="Characters.RelationshipReferentialIntegrityCheck"/> exactly, for the Romance
/// module's own registry: every <see cref="RomanticBond"/> key's <see
/// cref="RomanticBondKey.CharacterAId"/>/<see cref="RomanticBondKey.CharacterBId"/> must reference
/// two distinct, actually-registered Characters. Unlike its directed cousin, this check does not need
/// to say anything about symmetry — <see cref="RomanticBondKey"/> is undirected by construction (see
/// its own doc comment), so there is no second-direction record whose absence or divergence could ever
/// be a defect to flag.
/// </summary>
public sealed class RomanticBondReferentialIntegrityCheck : IInvariantCheck
{
    public string Id => "romance.romanticBonds.referentialIntegrity";
    public bool IsFatal => true;

    public IEnumerable<InvariantViolation> Check(WorldState state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        foreach (var entry in state.RomanticBonds.InAscendingOrder())
        {
            var key = entry.Key;

            if (key.CharacterAId == key.CharacterBId)
            {
                yield return new InvariantViolation(
                    Id,
                    $"Romantic bond '{key.CharacterAId.ToTaggedString()}' <-> '{key.CharacterBId.ToTaggedString()}' is self-paired.",
                    new[] { key.CharacterAId.ToTaggedString() });
                continue;
            }

            if (!state.Characters.TryGet(key.CharacterAId, out _))
                yield return new InvariantViolation(
                    Id,
                    $"Romantic bond references missing Character '{key.CharacterAId.ToTaggedString()}'.",
                    new[] { key.CharacterAId.ToTaggedString(), key.CharacterBId.ToTaggedString() });

            if (!state.Characters.TryGet(key.CharacterBId, out _))
                yield return new InvariantViolation(
                    Id,
                    $"Romantic bond references missing Character '{key.CharacterBId.ToTaggedString()}'.",
                    new[] { key.CharacterAId.ToTaggedString(), key.CharacterBId.ToTaggedString() });
        }
    }
}
