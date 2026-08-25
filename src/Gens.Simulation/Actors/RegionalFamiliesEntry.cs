using Gens.Simulation.Identity;

namespace Gens.Simulation.Actors;

/// <summary>The lighter, pre-contact "Notable Families of the Region" visibility an actor can have
/// before it is ever fully dossier-tracked (<c>gens-rival-houses-design.md</c> §7; Phase 10 item 5):
/// name, standing tier, and a single Identity tag only — ambient/discoverable via Travel, Events, or
/// Correspondence, deliberately shallower than a full <see cref="RivalDossier"/>. Sparse, keyed by
/// actor: an actor with no regional visibility yet simply has no entry.</summary>
public sealed record RegionalFamiliesEntry(
    RuntimeId<Actor> ActorId,
    string Name,
    LivingWorldActorStandingTrend StandingTrend,
    EconomicIdentityTag? IdentityEconomic);
