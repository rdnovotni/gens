using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Epithets;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One earned <see cref="Agnomen"/>, rendered for display (Phase 11 item 5).</summary>
public readonly record struct AgnomenRow(
    string AgnomenId,
    string CharacterId,
    string CharacterName,
    AgnomenType AgnomenType,
    string Name,
    AgnomenGrantMethod GrantMethod,
    int GrantedDateTotalMonths);

/// <summary>One household's read of its own earned names (Phase 11 item 5): every <see
/// cref="Agnomen"/> held by a currently-member Character, and the household's own <see
/// cref="DynasticEpithet"/> text if it has earned one yet.</summary>
public readonly record struct EpithetProjection(
    string HouseholdId,
    string? DynasticEpithetText,
    IReadOnlyList<AgnomenRow> Agnomens);

/// <summary>
/// Projects one household's Agnomina and Dynastic Epithet (Phase 11 item 5) — the read-model
/// counterpart to <see cref="ChronicleQuery"/>, since without it nothing <see
/// cref="Epithets.EpithetGenerationSystem"/> awards would ever reach a caller through the query
/// boundary (ADR 0013 rule 1: <c>WorldState</c> itself is never exposed directly).
/// </summary>
public sealed class EpithetQuery : IWorldQuery<EpithetProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public EpithetQuery(RuntimeId<Household> householdId)
    {
        _householdId = householdId;
    }

    public EpithetProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var agnomens = new List<AgnomenRow>();
        foreach (var entry in state.Agnomens.InAscendingOrder())
        {
            var agnomen = entry.Value;
            if (!state.Characters.TryGet(agnomen.CharacterId, out var character) || character!.Household != _householdId)
                continue;

            agnomens.Add(new AgnomenRow(
                agnomen.AgnomenId.ToTaggedString(),
                agnomen.CharacterId.ToTaggedString(),
                ChronicleProjector.Name(state, agnomen.CharacterId),
                agnomen.AgnomenType,
                agnomen.Name,
                agnomen.GrantMethod,
                agnomen.GrantedDate.TotalMonths));
        }

        var dynasticEpithetText = state.DynasticEpithets.TryGet(_householdId, out var epithet) ? epithet!.EpithetText : null;

        return new EpithetProjection(_householdId.ToTaggedString(), dynasticEpithetText, agnomens);
    }
}
