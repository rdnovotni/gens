using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Education;

/// <summary>
/// One Character's in-progress Culture drift toward a target Culture (Phase 17 item 2; §2), keyed by the
/// Character it describes — one drift target in progress per Character, matching <see
/// cref="Languages.LiteracyRecord"/>'s identical "the owning entity is already a unique key" shape.
/// <see cref="ProgressMonths"/> accrues monthly (see <see cref="CulturalDriftSystem"/>) at a rate keyed
/// by lifecycle stage and accelerated by an active Foreign Tutor (§3.3) targeting the same Culture;
/// crossing <see cref="EducationCulturalDriftCatalog.DriftThresholdMonths"/> reassigns <see
/// cref="Character.Culture"/> — the only place that field changes post-creation — and clears this entry.
/// </summary>
public sealed record CulturalDriftState(RuntimeId<Character> CharacterId, DefinitionId<Identity.Culture> TargetCultureId, int ProgressMonths = 0);

/// <summary>Read-side helpers over <see cref="WorldState.CulturalDriftStates"/>.</summary>
public static class CulturalDriftResolver
{
    public static bool TryGet(WorldState state, RuntimeId<Character> characterId, out CulturalDriftState drift) =>
        state.CulturalDriftStates.TryGet(characterId, out drift);

    /// <summary>Starts (or retargets) a Character's drift toward <paramref name="targetCultureId"/>,
    /// resetting <see cref="CulturalDriftState.ProgressMonths"/> to zero — a caller changing the target
    /// culture mid-drift starts that new target's accrual from scratch rather than carrying over progress
    /// made toward the old one.</summary>
    public static void SetTarget(WorldState state, RuntimeId<Character> characterId, DefinitionId<Identity.Culture> targetCultureId)
    {
        if (state.CulturalDriftStates.TryGet(characterId, out _))
            state.CulturalDriftStates.Remove(characterId);
        state.CulturalDriftStates.Add(characterId, new CulturalDriftState(characterId, targetCultureId));
    }

    public static void Clear(WorldState state, RuntimeId<Character> characterId)
    {
        if (state.CulturalDriftStates.TryGet(characterId, out _))
            state.CulturalDriftStates.Remove(characterId);
    }
}

/// <summary>Versioned constants for Phase 17 item 2's Culture drift mechanics (§2) — this
/// implementation's own unsized baseline, matching <see cref="CulturalPrestigeCatalog"/>'s identical
/// disclaimer convention.</summary>
public static class EducationCulturalDriftCatalog
{
    /// <summary>§2's "fast" Childhood/Adolescence drift rate.</summary>
    public const int FastDriftMonthsPerMonth = 3;

    /// <summary>§2's "slow" Adult (and Elderly) drift rate.</summary>
    public const int SlowDriftMonthsPerMonth = 1;

    /// <summary>§3.3's Foreign Tutor acceleration multiplier, applied on top of the stage rate above.</summary>
    public const int ForeignTutorAccelerationMultiplier = 2;

    /// <summary>§4's Study Abroad Journey acceleration multiplier — sharper than a Foreign Tutor's, per
    /// that section's own "sharply accelerated" framing; consumed by Phase 17 item 2's Institutions of
    /// Renown slice once <c>StudyAbroadJourney</c> exists.</summary>
    public const int StudyAbroadAccelerationMultiplier = 4;

    /// <summary>Total accrued drift-months needed to cross over to the target Culture.</summary>
    public const int DriftThresholdMonths = 36;
}
