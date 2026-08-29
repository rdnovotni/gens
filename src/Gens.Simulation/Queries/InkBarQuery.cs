using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Queries;

/// <summary>The persistent ink-bar spine's projection (Phase 9 item 6; <c>gens-core-design.md</c>
/// §7.4's "INK BAR — gens name · date/season · treasury · dignitas", present on every screen). No
/// <see cref="KnowledgeState"/> filtering, matching <see cref="HouseholdPolicyModifiersQuery"/>'s
/// precedent — the ink bar shows only the observing household's own figures.</summary>
public readonly record struct InkBarProjection(
    string HouseholdId,
    string GensName,
    int DisplayYear,
    string Era,
    int MonthOfYear,
    Money Treasury,
    int Dignitas);

/// <summary>Projects the top bar for the caller-specified household, the same "caller-specified
/// subject" shape <see cref="CharacterLifecycleQuery"/> establishes. <see
/// cref="InkBarProjection.Dignitas"/> now reads <see cref="DignitasResolver.Current"/> (Phase 12 item
/// 1) — this query's own doc comment previously reserved the slot at a fixed 0 while "no
/// reputation/standing system exists yet"; that gap is closed, defaulting to 0 for a household nothing
/// has touched yet (<see cref="Reputation.HouseholdReputation"/>'s own "no entry means zero"
/// convention), though no in-game system yet actually calls <see cref="AdjustDignitasCommand"/> to move
/// it. <see
/// cref="InkBarProjection.GensName"/> reads the oldest living household member's Nomen (Roman gens
/// members share a Nomen) as a stand-in for a real head-of-household designation, which Phase 11's
/// succession work (<c>gens-succession-dynasty-design.md</c>) has not landed yet.</summary>
public sealed class InkBarQuery : IWorldQuery<InkBarProjection>
{
    private readonly RuntimeId<Household> _householdId;

    public InkBarQuery(RuntimeId<Household> householdId) => _householdId = householdId;

    public InkBarProjection Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var (astronomicalYear, monthOfYear) = state.Date.ToCalendar();
        var era = astronomicalYear <= 0 ? "BCE" : "CE";

        Character? eldest = null;
        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var character = entry.Value;
            if (!character.IsAlive || character.Household != _householdId)
                continue;
            if (eldest is null || character.BirthDate.TotalMonths < eldest.BirthDate.TotalMonths)
                eldest = character;
        }

        state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(_householdId), out var account);

        return new InkBarProjection(
            HouseholdId: _householdId.ToTaggedString(),
            GensName: eldest?.Nomen ?? "Unknown Gens",
            DisplayYear: GameDate.ToDisplayYear(astronomicalYear),
            Era: era,
            MonthOfYear: monthOfYear,
            Treasury: account?.Balance ?? Money.Zero,
            Dignitas: DignitasResolver.Current(state, _householdId));
    }
}
