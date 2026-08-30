using Gens.Simulation.Characters;
using Gens.Simulation.Fame;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;

namespace Gens.Simulation.Queries;

/// <summary>§2's divergence categories (<c>gens-celebrities-influential-figures-design.md</c>) —
/// matching that section's own four-way vocabulary exactly.</summary>
public enum FameDivergenceCategory
{
    /// <summary>§2's "the gladiator, the actor, the charioteer" — Famous, and the Character's own
    /// household reads as disreputable by this item's Dignitas-proxy reading (see <see
    /// cref="FameDivergenceQuery"/>'s own doc comment for why Dignitas, not a real Infamia flag,
    /// stands in here).</summary>
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
/// (Crime &amp; Punishment §13, Romance, Sexuality &amp; Lineage §13), not Dignitas directly — but no
/// Infamia status exists anywhere in this codebase yet (both are Phase 17, unbuilt, confirmed by direct
/// search). This query reads a Character's own household Dignitas against <see
/// cref="FameCatalog.LowDignitasThreshold"/>/<see cref="FameCatalog.RespectedDignitasThreshold"/>
/// instead — Dignitas and Infamia move in the same real direction for most of §2's own worked examples
/// (a gladiator's household is rarely also a Dignitas powerhouse), so this is a real, reasoned proxy
/// rather than an invented mechanic, and this doc comment says so directly rather than silently
/// conflating the two. A single threshold, not a three-way band, decides "respected" versus not: a
/// Famous Character whose household falls short of it reads <see
/// cref="FameDivergenceCategory.FamousAndDisreputable"/> even at merely-ordinary Dignitas, since §2's
/// own "Dignitas without Fame is... entirely respectable... the default" framing already treats
/// ordinary/default Dignitas as outside the "genuinely respected" tier that makes <see
/// cref="FameDivergenceCategory.FamousAndRespected"/> "genuinely rare." A Character with no <see
/// cref="Character.Household"/> reads at Dignitas 0, matching every other household-Dignitas read
/// site's identical "no household means no standing to draw on" default.</summary>
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

        var category = (isFamous, isRespected) switch
        {
            (true, true) => FameDivergenceCategory.FamousAndRespected,
            (true, false) => FameDivergenceCategory.FamousAndDisreputable,
            (false, true) => FameDivergenceCategory.RespectedAndObscure,
            (false, false) => FameDivergenceCategory.NeitherYet,
        };

        return new FameDivergenceReading(_characterId.ToTaggedString(), fame, dignitas, category);
    }
}
