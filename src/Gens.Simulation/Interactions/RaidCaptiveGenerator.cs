using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Cultures;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Interactions;

/// <summary>
/// Lazy generation of a captured raider's Character (Phase 16 item 6; <c>gens-piracy-banditry-design.md</c>
/// §3's own "a captured raider is a real Character... sale into slavery," left as an outcome-only record
/// by <see cref="RaidOutcome.RaidersCaptured"/>'s own doc comment until this item closed the gap).
/// Sibling to <see cref="Actors.LivingWorldActorHeadGenerator"/>'s "lazily instantiate a bare Character
/// the moment one is actually needed" pattern, reusing the exact same Phase 5 backfill primitives
/// (name/visual profile, then attributes/skills, then condition, in that fixed draw order) — but simpler:
/// a captured raider is not a <see cref="Actors.LivingWorldActor"/>'s head, so there is no actor to stamp
/// and no fixed nomen to honor.
///
/// No Confederation-level culture concept exists anywhere in this codebase (<see
/// cref="Actors.BanditConfederationCreationService"/> stores no culture field), so this generator always
/// draws from <see cref="CultureNamingPoolCatalog.Roman"/> under a plain <c>"roman"</c> <see
/// cref="DefinitionId{T}"/> tag — this implementation's own simplification, disclosed rather than
/// fabricating a per-Confederation culture/name-pool resolution this item's scope does not call for.
/// A captured raider starts <see cref="LegalStatus.Peregrine"/> (a free foreigner, not yet enslaved) —
/// matching Military's own captured-Character precedent (<see
/// cref="Military.MilitaryCommands.ApplyMilitaryAftermathCommand"/> relocates a named captive without
/// changing their legal status either), leaving any future enslavement to whatever downstream command
/// (a Legal &amp; Court disposition, a sale) actually decides their fate.
/// </summary>
public static class RaidCaptiveGenerator
{
    private static readonly DefinitionId<Culture> DefaultCulture = new("roman");

    /// <summary>Draws and registers one captured raider's Character, off the caller's own already
    /// registered <paramref name="streamName"/> stream (rule 8: never a second stream just for this).</summary>
    public static Character GenerateCaptive(
        WorldState state,
        RandomStreamSet streams,
        string streamName,
        RuntimeId<Settlement> capturedAtSettlementId,
        GameDate asOf)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (streams is null)
            throw new ArgumentNullException(nameof(streams));

        var sex = streams.NextUInt(streamName, 2) == 0 ? Sex.Male : Sex.Female;
        var birthDate = CharacterBackfillGenerator.RollAdultBirthDate(streams, streamName, asOf);
        var identity = CharacterIdentityGenerator.Generate(streams, streamName, sex, LegalStatus.Peregrine, CultureNamingPoolCatalog.Roman);
        var (attributes, skills) = CharacterBackfillGenerator.RollAttributesAndSkills(streams, streamName);
        var condition = CharacterBackfillGenerator.RollCondition(streams, streamName);

        var id = state.CharacterIds.Issue();
        var captive = Character.Create(
            id: id,
            praenomen: identity.Name.Praenomen,
            nomen: identity.Name.Nomen,
            cognomen: identity.Name.Cognomen,
            sex: sex,
            birthDate: birthDate,
            visualProfile: identity.Visual,
            status: LegalStatus.Peregrine,
            socialClass: null,
            culture: DefaultCulture,
            location: capturedAtSettlementId,
            household: null,
            attributes: attributes,
            skills: skills,
            condition: condition,
            source: CharacterSource.RaidCaptured,
            instantiatedAtMonth: asOf.TotalMonths,
            backfilledHistory: true);
        state.Characters.Add(id, captive);

        return captive;
    }
}
