namespace Gens.Simulation.Fame;

/// <summary>§3/§10's source vocabulary (<c>gens-celebrities-influential-figures-design.md</c>) —
/// every source that document names is represented, matching <see
/// cref="Legal.LegalCase.CaseType"/>'s and <see cref="Scandal.ScandalSourceType"/>'s own identical
/// "every real category represented, only some reachable" precedent. None of them is ever actually
/// rolled by a real caller in this codebase: Oratory would route through Legal &amp; Court's own
/// prosecution/defense machinery, <see cref="LiteraryWork"/> and <see cref="ReligiousCharisma"/> need
/// Education &amp; Culture's Literary Patronage and a charisma concept neither built, <see
/// cref="WandererRenown"/> needs Wandering Populations (Phase 13, unbuilt), <see
/// cref="MilitaryValor"/> needs Military &amp; Combat (Phase 16, unbuilt), <see
/// cref="RomanceOrScandal"/> needs Romance, Sexuality &amp; Lineage's Infamia status (Phase 17,
/// unbuilt), <see cref="Athletics"/> needs Starting Regions: Greek East's own athletic-games content
/// (unbuilt), and <see cref="ArenaOrCircusOrTheatre"/> needs Games &amp; Spectacle (Phase 17, unbuilt)
/// — this item builds the shared Fame primitive those future sources are all meant to route through
/// (see <see cref="AdjustFameCommand"/>'s own doc comment), exercised directly by this item's own
/// tests standing in for those future callers, matching Phase 12 item 1's identical "the primitive
/// ships, the callers don't exist yet" precedent for Dignitas.</summary>
public enum FameSourceType
{
    Oratory,
    LiteraryWork,
    WandererRenown,
    MilitaryValor,
    RomanceOrScandal,
    Athletics,
    ReligiousCharisma,
    ArenaOrCircusOrTheatre,
}
