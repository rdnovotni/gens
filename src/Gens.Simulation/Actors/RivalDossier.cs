using Gens.Simulation.Identity;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>A tracked <see cref="LivingWorldActor"/>'s legibility record for the player
/// (<c>gens-rival-houses-design.md</c> §7; Phase 10 item 5): Name/Identity/Dignitas/Net Worth/
/// Military Strength/Standing are read live from the <see cref="LivingWorldActor"/> itself when the
/// dossier is displayed — this record holds only what genuinely lags behind live state: the narrative
/// <see cref="Summary"/> and <see cref="RecentChronicleEntries"/>, refreshed only when new information
/// actually reaches the player (contact, correspondence, a shared event), per that section's
/// "Dossier isn't omnisciently live" staleness rule. <see cref="LastUpdatedDate"/> is what a future
/// staleness display reads to show "as of" rather than a live figure — no decay/refresh system lands
/// in this package; see the Phase 10 plan's package 5/13 for what populates and ages this.</summary>
/// <param name="ActorId">Sparse: an actor the player has never had contact with has no entry in <see
/// cref="Gens.Simulation.State.WorldState.RivalDossiers"/> at all.</param>
/// <param name="HeadComboTitle">The head Character's Combo Title (Traits §7) as dossier headline
/// flavor — a plain string rather than a typed reference, since no Combo Title record exists in this
/// codebase yet.</param>
/// <param name="RecentChronicleEntries">Plain string references: no Dynasty Chronicle record exists
/// yet (Phase 11), matching <see cref="LivingWorldActorMilitaryStrength.ResolvedForceId"/>'s identical
/// "reference an entity kind that does not exist yet as a plain string" convention.</param>
public sealed record RivalDossier(
    RuntimeId<Actor> ActorId,
    string Summary,
    string? HeadComboTitle,
    GameDate LastUpdatedDate,
    IReadOnlyList<string> RecentChronicleEntries);
