using System.Linq;
#nullable enable
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Romance;

/// <summary>What real-world activity or conviction marked a Character with Infamia
/// (<c>gens-romance-sexuality-lineage-design.md</c> §13). <see cref="ConvictedAdultery"/> is the only
/// source this item's own <see cref="AdulteryResolutionHook"/> actually grants — <see
/// cref="Prostitution"/>/<see cref="Acting"/>/<see cref="Gladiatorial"/> are modeled for schema
/// completeness (§13 names all four as real historical Infamia triggers) but left genuinely unreached
/// this pass: no existing Brothel/theatrical/gladiatorial staff-assignment command in this codebase was
/// confirmed, within this item's own time budget, to be an easy, low-risk caller to wire — matching <see
/// cref="Legal.LegalSentence.DebtBondage"/>'s own "schema complete, caller later" precedent for exactly
/// this situation.</summary>
public enum InfamiaSource
{
    Prostitution,
    Acting,
    Gladiatorial,
    ConvictedAdultery,
}

/// <summary>
/// A real, persistent Infamia mark (<c>gens-romance-sexuality-lineage-design.md</c> §13) — an orthogonal
/// status a <see cref="Character"/> carries without ceasing to hold their own <see
/// cref="LegalStatus"/>; deliberately not a sixth value of that enum, since a Roman citizen convicted of
/// adultery remains a <see cref="LegalStatus.RomanCitizen"/> in every other respect while carrying this
/// separate, additional mark. New, sparse <see cref="State.WorldState.InfamiaStatuses"/> partition keyed
/// by Character — at most one <see cref="InfamiaStatus"/> per Character (<see
/// cref="GrantInfamiaCommand"/> rejects granting a second one; see that command's own doc comment for why
/// "reject" rather than "overwrite/upgrade" is this item's own simplest defensible call).
///
/// This item's own real, reachable caller is <see cref="AdulteryResolutionHook"/>, granting <see
/// cref="InfamiaSource.ConvictedAdultery"/> on a §12 Adultery conviction. <see
/// cref="Queries.FameDivergenceQuery"/> is the one existing read site this item updates to consume this
/// real primitive instead of its own previous Dignitas-as-proxy reading (that query's own doc comment,
/// prior to this item, named this exact type as the intended eventual replacement).
/// </summary>
/// <param name="LegalProtectionsLost">A small, concrete, but explicitly untuned placeholder list of
/// what this mark costs a Character legally/socially (§17's own "every numeric/textual size is an
/// untuned placeholder" disclaimer, applied here to prose rather than a number) — no mechanical system
/// in this codebase currently reads this list to actually withhold a specific legal protection; it
/// exists so a future Legal &amp; Court or Politics &amp; Patronage pass has a concrete, honestly-labeled
/// starting point rather than inventing one from scratch.</param>
public sealed record InfamiaStatus(
    RuntimeId<Character> CharacterId,
    InfamiaSource Source,
    IReadOnlyList<string> LegalProtectionsLost);
