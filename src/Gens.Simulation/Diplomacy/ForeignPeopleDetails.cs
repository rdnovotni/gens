using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.State;

namespace Gens.Simulation.Diplomacy;

/// <summary>The Diplomacy-specific data layered on top of one <see cref="LivingWorldActor"/> of <see
/// cref="LivingWorldActorType.ForeignPeople"/> (Phase 16 item 5 slice 1; <c>gens-diplomacy-non-roman-peoples-design.md</c>
/// §2). Name, Tier, Standing Trend, and head Character are all read live off the underlying <see
/// cref="LivingWorldActor"/> rather than duplicated here, matching <see
/// cref="Collegia.CollegiumDetails"/>'s "read live, hold only what genuinely differs" convention — this
/// record only ever holds a fact a <see cref="LivingWorldActor"/> has no field for at all: which culture
/// this people belongs to, needed to resolve their language (§5's Interpreter Problem) and to enforce
/// that only a <see cref="CultureCategory.Frontier"/> culture may seed one in this slice.</summary>
/// <param name="CultureId">One of <see cref="Cultures.KnownWorldCultures"/>'s catalog entries, always
/// <see cref="CultureCategory.Frontier"/> as of the seeding date — Great Power, Contested Buffer, and
/// Trade-Contact-Only peoples are out of this slice's scope (see <see
/// cref="ForeignPeopleCreationService"/>).</param>
public sealed record ForeignPeopleDetails(RuntimeId<Actor> ActorId, DefinitionId<Culture> CultureId);

/// <summary>Read-side helpers over <see cref="WorldState.ForeignPeopleDetails"/>, matching <see
/// cref="Collegia.CollegiumResolver"/>'s shape.</summary>
public static class ForeignPeopleQueries
{
    public static bool TryGet(WorldState state, RuntimeId<Actor> actorId, out ForeignPeopleDetails details) =>
        state.ForeignPeopleDetails.TryGet(actorId, out details);

    /// <summary>Resolves the language this people speaks (§5's negotiation-target language), or
    /// <c>null</c> when either the actor is not a tracked Foreign People or their culture is one of the
    /// three <see cref="CultureLanguageMap"/> deliberately leaves unmapped.</summary>
    public static DefinitionId<LanguageDefinition>? TryResolveLanguage(
        WorldState state, RuntimeId<Actor> actorId, CultureLanguageMap cultureLanguages)
    {
        if (cultureLanguages is null)
            throw new ArgumentNullException(nameof(cultureLanguages));

        return TryGet(state, actorId, out var details) ? cultureLanguages.Resolve(details.CultureId) : null;
    }
}
