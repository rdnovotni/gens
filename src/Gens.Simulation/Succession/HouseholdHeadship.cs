using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>
/// Which Character currently holds a Household (Phase 11 item 1; <c>gens-succession-dynasty-design.md</c>
/// §6's "control passes on death or formal retirement"). No real <c>Household</c> record exists yet
/// (only the <see cref="Identity.Household"/> phantom <see cref="RuntimeId{T}"/> tag) — this sparse,
/// per-household partition is this phase's own concrete "who is head" concept, established explicitly
/// via <see cref="EstablishHouseholdHeadCommand"/> rather than inferred from <see
/// cref="Character.Household"/> membership (a household can have many members but only one head).
/// </summary>
/// <param name="RegentCharacterId">Non-null when <see cref="HeadCharacterId"/> is a minor (§6.2
/// Regency) and a surviving spouse is holding the estate in trust on their behalf — the spouse-in-trust
/// half of §3's "surviving spouse... can hold the estate in trust when no adult heir exists". A senior
/// appointee (Rationalis/Procurator) Regent, per §6.2's fallback, would reuse <see
/// cref="Stewardship.StewardshipAssignment"/> rather than this field; that fuller Regency integration is
/// out of this item's scope (see <see cref="SuccessionHandoffSystem"/>'s own doc comment).</param>
public sealed record HouseholdHeadship(
    RuntimeId<Household> HouseholdId,
    RuntimeId<Character> HeadCharacterId,
    GameDate SinceDate,
    RuntimeId<Character>? RegentCharacterId = null);
