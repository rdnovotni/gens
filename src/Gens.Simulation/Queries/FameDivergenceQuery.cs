using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.Romance;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>§2's divergence categories (<c>gens-celebrities-influential-figures-design.md</c>) —
/// matching that section's own four-way vocabulary exactly.</summary>
public enum FameDivergenceCategory
{
    /// <summary>§2's "the gladiator, the actor, the charioteer" — Famous, and the Character carries a
    /// real <see cref="Romance.InfamiaStatus"/> (Phase 17 item 3 slice 9's own real primitive, replacing
    /// this query's former Dignitas-threshold proxy — see <see cref="FameDivergenceQuery"/>'s own doc
    /// comment).</summary>
    FamousAndDisreputable,

    /// <summary>§2's "the quiet, respected senator or magistrate" — the household reads as respected,
    /// but the Character is not yet Famous.</summary>
    RespectedAndObscure,

    /// <summary>§2's "genuinely rare" combination — Famous, and the household reads as respected.</summary>
    FamousAndRespected,

    /// <summary>Neither threshold is cleared yet — the overwhelming default for a Character nothing has
    /// touched.</summary>
    NeitherYet,
}

/// <summary>§2's descriptive-only Fame/Dignitas Divergence reading — "not a new number to track, simply
/// the descriptive gap between two fields this project already has." Computed directly from <see
/// cref="Fame.FameResolver.Current"/> and the Character's own household's <see
/// cref="Reputation.DignitasResolver.Current"/>, never stored, matching §11's own "whether Divergence
/// should ever surface as an explicit... element... this document treats it as descriptive-only for
/// now."</summary>
public readonly record struct FameDivergenceReading(
    string CharacterId,
    int Fame,
    int Dignitas,
    FameDivergenceCategory DivergenceCategory);

/// <summary>Projects a single, caller-specified Character's <see cref="FameDivergenceReading"/> (Phase
/// 12 item 8; §2), matching <see cref="CharacterLifecycleQuery"/>'s own "caller-specified subject"
/// shape. No <see cref="KnowledgeState"/> filtering, matching <see cref="InkBarQuery"/>'s identical
/// precedent — both Fame and Dignitas are already unconditionally public per each field's own
/// <c>*ChangedEvent</c> <see cref="Commands.Visibility"/>.
///
/// <b>Scope note:</b> §2's own "famous and disreputable at once" divergence is really about Infamia
/// (Crime &amp; Punishment §13, Romance, Sexuality &amp; Lineage §13). Phase 17 item 3 slice 9 built that
/// real primitive (<see cref="Romance.InfamiaStatus"/>), so this query now reads <see
/// cref="WorldState.InfamiaStatuses"/> directly for <see cref="FameDivergenceCategory.FamousAndDisreputable"/>
/// instead of the Dignitas-threshold proxy this query used before that status existed — a Famous
/// Character carrying any real Infamia mark reads disreputable regardless of Dignitas (an
/// infamia-marked citizen can still hold real, even high, Dignitas in this codebase's own model; the
/// mark is orthogonal, not merely "low Dignitas restated"). Every other combination still reads
/// Dignitas against <see cref="FameCatalog.RespectedDignitasThreshold"/> exactly as before: <see
/// cref="FameDivergenceCategory.FamousAndRespected"/> and <see
/// cref="FameDivergenceCategory.RespectedAndObscure"/> are unchanged, and a Famous Character with
/// neither a real Infamia mark nor Respected-tier Dignitas now reads <see
/// cref="FameDivergenceCategory.NeitherYet"/> — this implementation's own decisive call: an ordinary,
/// unmarked Famous Character is no longer conflated with a genuinely disreputable one now that a real
/// primitive exists to tell the two apart, even though that leaves such a Character sharing a category
/// label with someone Fame has not touched at all. A Character with no <see cref="Character.Household"/>
/// reads at Dignitas 0, matching every other household-Dignitas read site's identical "no household
/// means no standing to draw on" default.</summary>
public sealed class FameDivergenceQuery : IWorldQuery<FameDivergenceReading>
{
    private readonly RuntimeId<Character> _characterId;

    public FameDivergenceQuery(RuntimeId<Character> characterId) => _characterId = characterId;

    public FameDivergenceReading Execute(WorldState state, string observerId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var fame = FameResolver.Current(state, _characterId);
        var dignitas = 0;
        if (state.Characters.TryGet(_characterId, out var character) && character!.Household is { } householdId)
            dignitas = DignitasResolver.Current(state, householdId);

        var isFamous = fame >= FameCatalog.FamousFameThreshold;
        var isRespected = dignitas >= FameCatalog.RespectedDignitasThreshold;
        var hasInfamia = state.InfamiaStatuses.TryGet(_characterId, out _);

        var category = (isFamous, isRespected, hasInfamia) switch
        {
            (true, _, true) => FameDivergenceCategory.FamousAndDisreputable,
            (true, true, false) => FameDivergenceCategory.FamousAndRespected,
            (false, true, _) => FameDivergenceCategory.RespectedAndObscure,
            _ => FameDivergenceCategory.NeitherYet,
        };

        return new FameDivergenceReading(_characterId.ToTaggedString(), fame, dignitas, category);
    }
}
