using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§5's two real physical anchors. Both are new settlement/household-level building families
/// (Estate &amp; Settlement §5 names the public Carcer alongside the Forum and Baths; Labor &amp;
/// Slavery names the private Ergastulum) that do not exist anywhere in <c>Gens.Simulation.Buildings</c>
/// at the time this item was built (verified directly — that namespace holds only <see
/// cref="Buildings.BuildingDefinition"/>/<see cref="Buildings.BuildingInstance"/> plumbing and no
/// content-authored building family at all yet). <see cref="DetentionRecord.LocationType"/> tracks
/// this as data anyway, matching <see cref="Magistracies.AppointDecurionCommand"/>'s own "no building
/// exists, so that half of the gate is not checked" precedent — a future Estate &amp; Settlement/Labor
/// &amp; Slavery pass that actually ships these buildings can gate against a real
/// <c>BuildingInstance</c> without this record's own shape changing at all.</summary>
public enum DetentionLocationType
{
    PublicCarcer,
    PrivateErgastulum,
}

/// <summary>
/// A Character's own tracked Detained status (Phase 12 item 5; §5: "a real tracked status... distinct
/// from Enslaved"). <see cref="EndDate"/> null is the "currently active" flag, matching <see
/// cref="Magistracies.MagistracyRecord.TermEndDate"/>'s identical "ended records are never removed,
/// only replaced" convention — kept forever once opened, a Character's full detention history being
/// exactly the kind of record a future Legal &amp; Court/Chronicle query needs the whole log for.
/// </summary>
/// <param name="LinkedLegalCaseId">§5: "a Detained status can genuinely persist... while a major Legal
/// &amp; Court case runs its own multi-stage course." Nullable — an Imprison exercised purely on
/// household/Clientela/magisterial authority, with no formal case ever filed, leaves this null.</param>
/// <param name="Escaped">True when <see cref="AttemptDetentionEscapeCommand"/> resolved a successful
/// escape — distinct from an ordinary release (<see cref="ReleaseFromDetentionCommand"/>) or a
/// sentence being carried out (<see cref="ApplySentenceCommand"/>), all three of which also set <see
/// cref="EndDate"/> but for genuinely different reasons a future query may care to tell apart.</param>
public sealed record DetentionRecord(
    RuntimeId<DetentionRecord> DetentionId,
    RuntimeId<Character> CharacterId,
    DetentionLocationType LocationType,
    GameDate StartDate,
    bool Justified,
    RuntimeId<LegalCase>? LinkedLegalCaseId = null,
    GameDate? EndDate = null,
    bool Escaped = false);

/// <summary>Read-side helpers over <see cref="WorldState.DetentionRecords"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical linear-scan convention.</summary>
public static class DetentionResolver
{
    public static bool IsActive(DetentionRecord record) => record.EndDate is null;

    /// <summary>The character's own currently active Detention, if any.</summary>
    public static DetentionRecord? ActiveFor(WorldState state, RuntimeId<Character> characterId)
    {
        foreach (var entry in state.DetentionRecords.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.CharacterId == characterId)
                return entry.Value;

        return null;
    }

    /// <summary>§5's escape-risk read, mirroring <see cref="FlightRiskCalculator"/>'s own Labor &amp;
    /// Slavery formula (that document's §7) directly whenever it is actually reusable: a Detained
    /// enslaved Character still carries the same <see cref="Character.Regimen"/> Labor &amp; Slavery
    /// already reads Freedoms/Discipline from, so this simply calls that same calculator. A free
    /// Detained Character (a household dependent, a Client) has no Regimen at all — Freedoms/Discipline
    /// belong to the labor-management system specifically, not to detention in general — so this falls
    /// back to a real, simple, Loyalty-only placeholder instead of inventing a parallel Regimen-shaped
    /// concept for the free case. This is a narrower reuse than §5's own "mirrors that document's own
    /// flight/recapture math directly" framing: it borrows the shared risk-to-probability curve (<see
    /// cref="FlightRiskCalculator.MonthlyProbabilityThreshold"/>) unconditionally, but only borrows the
    /// full Regimen-driven risk-score formula when a Regimen actually exists to read.</summary>
    public static int ComputeRiskScore(WorldState state, RuntimeId<Character> characterId)
    {
        if (!state.Characters.TryGet(characterId, out var character))
            return 0;

        if (character.Regimen is { } regimen)
            return FlightRiskCalculator.ComputeRiskScore(character.Condition.Loyalty, regimen);

        var loyaltyMotive = 100 - character.Condition.Loyalty;
        return Math.Clamp(loyaltyMotive, 0, CrimeCatalog.MaxFreeDetaineeRiskScore);
    }
}
