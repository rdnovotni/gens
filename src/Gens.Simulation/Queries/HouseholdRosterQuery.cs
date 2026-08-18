using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>One living household member's roster row (Phase 9 item 6's household roster screen).
/// <see cref="DutySlot"/> is null when the member holds no current Labor duty (<see
/// cref="Characters.DutyAssignment"/>).</summary>
public readonly record struct HouseholdRosterRow(
    string CharacterId,
    string FullName,
    Sex Sex,
    int AgeInYears,
    LifecycleStage Stage,
    LegalStatus LegalStatus,
    SocialClass? SocialClass,
    DutySlot? DutySlot);

/// <summary>The household roster screen's projection (Phase 9 item 6): every living member of one
/// household, in ascending Character-ID (creation) order — <see cref="Identity.OrderedRegistry{TId,TEntity}"/>'s
/// standard iteration guarantee, so the roster's row order is stable across queries without a
/// separate sort key. Dead members are excluded; the Dynasty Chronicle (Phase 11), not the roster, is
/// where the household's departed are read back.</summary>
public readonly record struct HouseholdRosterProjection(
    string HouseholdId,
    IReadOnlyList<HouseholdRosterRow> Members);

/// <summary>Projects the caller-specified household's living roster, the same "caller-specified
/// subject" shape <see cref="CharacterLifecycleQuery"/> establishes. No <see cref="KnowledgeState"/>
/// filtering yet, matching that query's own precedent — a player always knows their own household's
/// full membership.</summary>
public sealed class HouseholdRosterQuery : IWorldQuery<HouseholdRosterProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public HouseholdRosterQuery(RuntimeId<Household> householdId) => _householdId = householdId;

    public HouseholdRosterProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var rows = new List<HouseholdRosterRow>();
        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var character = entry.Value;
            if (!character.IsAlive || character.Household != _householdId)
                continue;

            rows.Add(new HouseholdRosterRow(
                CharacterId: character.Id.ToTaggedString(),
                FullName: FormatFullName(character),
                Sex: character.Sex,
                AgeInYears: character.AgeInYears(state.Date),
                Stage: character.GetLifecycleStage(state.Date),
                LegalStatus: character.LegalStatus,
                SocialClass: character.SocialClass,
                DutySlot: character.Duty?.Slot));
        }

        return new HouseholdRosterProjection(_householdId.ToTaggedString(), rows);
    }

    private static string FormatFullName(Character character) =>
        character.Cognomen is { Length: > 0 } cognomen
            ? $"{character.Praenomen} {character.Nomen} {cognomen}"
            : $"{character.Praenomen} {character.Nomen}";
}
