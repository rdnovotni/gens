using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Actors;

/// <summary>
/// Lazy generation of a <see cref="LivingWorldActor"/>'s head Character (Phase 10 item 4;
/// <c>gens-characters-design.md</c> §11's lazy instantiation, applied "one level up" per
/// <c>gens-rival-houses-design.md</c> §3.2: "Members are Characters... generated the moment the
/// player household actually interacts with them"). Reuses the exact Phase 5 backfill primitives
/// <see cref="Characters.PromoteToNamedCommand"/> already uses for promoting a background pop-group
/// member to a named Character — name/visual profile first, then attributes/skills, then condition,
/// in that fixed draw order — rather than a new generator, and tags the result <see
/// cref="CharacterSource.RivalGenerated"/>, the enum value already reserved for exactly this case.
/// </summary>
public static class LivingWorldActorHeadGenerator
{
    /// <summary>Generates <paramref name="actorId"/>'s head Character and stamps it onto the actor's
    /// <see cref="LivingWorldActor.HeadCharacterId"/>. Passes <see cref="LivingWorldActor.Name"/>
    /// straight through as <see cref="CharacterIdentityGenerator.Generate"/>'s <c>fixedNomen</c>, so
    /// <see cref="RivalHouseCreationService"/>'s creation paths must store the masculine "-us" nomen
    /// form there (matching <see cref="NamePool.Nomina"/>'s own convention, e.g. "Valerius") rather
    /// than an already-feminized or display-composed name — <see cref="CharacterNameGenerator.Feminize"/>
    /// derives the feminine form for a female head automatically, and double-feminizes an already
    /// feminine input otherwise. Throws if the actor already has a head — generation is meant to run
    /// exactly once per actor; call sites that only need to know a head exists should check <see
    /// cref="LivingWorldActor.HeadCharacterId"/> themselves first.</summary>
    public static (LivingWorldActor Actor, Character Head) GenerateHead(
        WorldState state,
        RandomStreamSet streams,
        string streamName,
        RuntimeId<Actor> actorId,
        GameDate asOf,
        LegalStatus status,
        SocialClass? socialClass,
        DefinitionId<Culture> culture,
        NamePool namePool)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        if (!state.Actors.TryGet(actorId, out var actor))
            throw new ArgumentException($"No LivingWorldActor with ID '{actorId}' is registered.", nameof(actorId));

        if (actor!.HeadCharacterId is not null)
            throw new InvalidOperationException($"LivingWorldActor '{actorId}' already has a head Character.");

        var sex = streams.NextUInt(streamName, 2) == 0 ? Sex.Male : Sex.Female;
        var birthDate = CharacterBackfillGenerator.RollAdultBirthDate(streams, streamName, asOf);
        var identity = CharacterIdentityGenerator.Generate(streams, streamName, sex, status, namePool, fixedNomen: actor.Name);
        var (attributes, skills) = CharacterBackfillGenerator.RollAttributesAndSkills(streams, streamName);
        var condition = CharacterBackfillGenerator.RollCondition(streams, streamName);

        var headId = state.CharacterIds.Issue();
        var head = Character.Create(
            id: headId,
            praenomen: identity.Name.Praenomen,
            nomen: identity.Name.Nomen,
            cognomen: identity.Name.Cognomen,
            sex: sex,
            birthDate: birthDate,
            visualProfile: identity.Visual,
            status: status,
            socialClass: socialClass,
            culture: culture,
            location: actor.HomeSettlementId,
            household: null,
            attributes: attributes,
            skills: skills,
            condition: condition,
            source: CharacterSource.RivalGenerated,
            instantiatedAtMonth: asOf.TotalMonths,
            backfilledHistory: true);
        state.Characters.Add(headId, head);

        var updatedActor = actor with { HeadCharacterId = headId };
        state.Actors.Remove(actorId);
        state.Actors.Add(actorId, updatedActor);

        return (updatedActor, head);
    }
}
