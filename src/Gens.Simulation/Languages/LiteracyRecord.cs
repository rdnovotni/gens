using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Languages;

/// <summary>§3, §10's <c>derivedFrom</c> field — how <see cref="IsLiterate"/> was reached, not a
/// simulated system: <see cref="LegalStatusAndWealth"/> is the ambient default this item deliberately
/// never auto-computes here (§3: "never separately tracked" for the overwhelming majority of the
/// population — a caller deriving that default reads Legal Status/Wealth directly rather than this
/// record), while <see cref="LearningAttribute"/> marks a named Character whose Literacy was actually
/// set because it became mechanically relevant.</summary>
public enum LiteracyDerivation
{
    LegalStatusAndWealth,
    LearningAttribute,
}

/// <summary>§3, §10's <c>LiteracyRecord</c> shape — tracked only where mechanically relevant, per §3's
/// own restraint. A real <see cref="WorldState"/> partition: keyed by <see cref="CharacterId"/> alone
/// (one record per Character), mirroring <see cref="Clientela.ClientelaEntry"/>'s identical "the owning
/// entity is already a unique key" shape rather than needing its own <see cref="RuntimeId{T}"/>.</summary>
public sealed record LiteracyRecord(RuntimeId<Character> CharacterId, bool IsLiterate, LiteracyDerivation DerivedFrom);

/// <summary>Read-side helper over <see cref="WorldState.LiteracyRecords"/>.</summary>
public static class LiteracyQueries
{
    public static bool TryGet(WorldState state, RuntimeId<Character> characterId, out LiteracyRecord record) =>
        state.LiteracyRecords.TryGet(characterId, out record);
}
