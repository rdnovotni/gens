using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Fame;

/// <summary>
/// One Character's running Fame score (Phase 12 item 8; <c>gens-celebrities-influential-figures-design.md</c>
/// §1, extending <c>gens-games-spectacle-design.md</c> §2). This is the field a long chain of earlier
/// items' own doc comments named as missing outright — <see cref="Epithets.Agnomen.FameEffect"/>, <see
/// cref="Scandal.ScandalRecord.CurrentFameEffect"/>, and Phase 12 item 1's own <see
/// cref="Reputation.HouseholdReputation"/> doc comment all say some version of "Fame is a universal
/// 0-100 Character field owned by Games &amp; Spectacle (Phase 17) and widened by Celebrities &amp;
/// Influential Figures, neither built." Games &amp; Spectacle has still not shipped — but this
/// roadmap's own Phase 12 construction order (item 8, "Fame/celebrity and public endorsement") places
/// building the shared primitive itself here, before Phase 17, the same "build the shared engine now,
/// let the design doc's own claimed owner catch up later" move Phase 12 item 1 already made for
/// Dignitas. This record is that primitive.
///
/// <b>Character-level, not household-level</b> — the one deliberate divergence from <see
/// cref="Reputation.HouseholdReputation"/>'s own convention, and a direct one: §1's own data model is
/// explicit that Fame "lives on Character schema itself," unlike Dignitas, which Phase 12 item 1 built
/// household-level specifically because "no Character-level reputation primitive exists... to move
/// instead." Fame is that Character-level primitive, now that it exists.
///
/// Sparse, matching <see cref="Reputation.HouseholdReputation"/>'s identical "present only once
/// something has actually touched it" convention (see <see cref="FameResolver.Current"/> for the
/// "no entry means zero" default) — and, unlike Dignitas, clamped to §1's own explicit 0-100 range
/// rather than left open-ended, since the design doc states that range directly rather than leaving it
/// unsized the way Dignitas's own scale is.
/// </summary>
/// <param name="CharacterId">The Character this Fame score belongs to.</param>
/// <param name="Fame">The current score, always in [0, 100].</param>
public sealed record CharacterFame(RuntimeId<Character> CharacterId, int Fame);

/// <summary>Resolves a Character's current Fame, defaulting a Character with no <see
/// cref="CharacterFame"/> entry yet to zero — matching <see cref="Reputation.DignitasResolver"/>'s
/// identical "no entry means the default" convention.</summary>
public static class FameResolver
{
    public static int Current(WorldState state, RuntimeId<Character> characterId) =>
        state.CharacterFames.TryGet(characterId, out var entry) ? entry!.Fame : 0;

    /// <summary>Applies a signed Fame delta, clamped to [0, 100] (§1), creating the Character's first
    /// <see cref="CharacterFame"/> entry if none exists yet. Replaces the entry (remove then re-add)
    /// rather than mutating in place, matching <see cref="Reputation.DignitasResolver.Apply"/>'s
    /// identical convention.</summary>
    public static void Apply(WorldState state, RuntimeId<Character> characterId, int delta)
    {
        var next = Math.Clamp(Current(state, characterId) + delta, 0, 100);
        if (state.CharacterFames.TryGet(characterId, out _))
            state.CharacterFames.Remove(characterId);
        state.CharacterFames.Add(characterId, new CharacterFame(characterId, next));
    }
}
