using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Languages;

/// <summary>§10's <c>DiplomacyLanguageGate.gateClearedBy</c> — how (or whether) §6's hard gate cleared.</summary>
public enum LanguageGateClearedBy
{
    /// <summary>Not cleared — negotiation cannot proceed (§6, §10's own <c>null</c> case, given a real
    /// name here rather than a nullable-enum encoding of "no").</summary>
    None,

    /// <summary>The negotiating Character's own Conversational-or-better proficiency (§6).</summary>
    NegotiatorFluency,

    /// <summary>A qualified Interpres present — either the household's own formal <see
    /// cref="InterpresAppointment"/>, or (§7's own "deliberately flexible rather than mandatory") any
    /// other Character with Conversational-or-better proficiency serving informally.</summary>
    InterpresPresent,
}

/// <summary>§10's <c>DiplomacyLanguageGate</c> shape, as a real, callable check — §6's hard gate for
/// "negotiating directly with a Frontier, Contested Buffer, Independent, or Great Power people." This is
/// the general mechanism, built and fully tested per this item's own scope discipline, with no actual
/// Diplomacy negotiation flow to call it from yet (Diplomacy with Non-Roman Peoples is Phase 16, not
/// built) — the same "mechanism now, caller later" shape <see
/// cref="Correspondence.CorrespondenceReachabilityCatalog"/> and <see
/// cref="Travel.DistanceTierCatalog"/> both already established for their own not-yet-consumed
/// lookups.</summary>
public sealed record DiplomacyLanguageGateResult(LanguageGateClearedBy GateClearedBy, RuntimeId<Character>? InterpresCharacterId)
{
    public bool Cleared => GateClearedBy != LanguageGateClearedBy.None;
}

public static class DiplomacyLanguageGateEvaluator
{
    /// <summary>Evaluates §6's hard gate for <paramref name="negotiatorId"/> attempting to negotiate in
    /// <paramref name="requiredLanguage"/>, checking (in order) the negotiator's own fluency, the
    /// negotiating household's formal <see cref="InterpresAppointment"/>, then §7's "any Character who
    /// happens to hold Conversational-or-better proficiency... can serve this function informally" —
    /// scanning every living member of <paramref name="negotiatingHouseholdId"/> (excluding the
    /// negotiator, already checked) for one qualified enough to stand in.</summary>
    public static DiplomacyLanguageGateResult Evaluate(
        WorldState state, RuntimeId<Character> negotiatorId, DefinitionId<LanguageDefinition> requiredLanguage,
        RuntimeId<Household>? negotiatingHouseholdId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        if (LanguageProficiencyQueries.HasConversationalOrBetter(state, negotiatorId, requiredLanguage))
            return new DiplomacyLanguageGateResult(LanguageGateClearedBy.NegotiatorFluency, null);

        if (negotiatingHouseholdId is null)
            return new DiplomacyLanguageGateResult(LanguageGateClearedBy.None, null);

        var householdId = negotiatingHouseholdId.Value;

        if (InterpresQueries.TryGet(state, householdId, out var appointment) &&
            appointment.LanguagesCovered.Contains(requiredLanguage) &&
            state.Characters.TryGet(appointment.CharacterId, out var appointee) &&
            appointee.IsAlive && appointee.Household == householdId)
        {
            return new DiplomacyLanguageGateResult(LanguageGateClearedBy.InterpresPresent, appointment.CharacterId);
        }

        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var candidate = entry.Value;
            if (candidate.Id == negotiatorId || candidate.Household != householdId || !candidate.IsAlive)
                continue;
            if (LanguageProficiencyQueries.HasConversationalOrBetter(state, candidate.Id, requiredLanguage))
                return new DiplomacyLanguageGateResult(LanguageGateClearedBy.InterpresPresent, candidate.Id);
        }

        return new DiplomacyLanguageGateResult(LanguageGateClearedBy.None, null);
    }
}

/// <summary>§6's soft-penalty half — "reduced effectiveness, described narratively as halting or
/// imprecise, rather than an outright block" for an ordinary Interaction between Characters sharing no
/// language. Built as a standalone, tested severity lookup rather than wired into <see
/// cref="Characters.RecordInteractionCommand"/> or <see cref="Interactions.InteractionActionDefinitions"/>
/// directly: every existing Interaction in this codebase (<c>Befriend</c>, <c>InitiateScheme</c>) already
/// takes its opinion delta as a plain caller-supplied constant with no per-invocation attenuation
/// mechanism to hook into, and §6 itself never sizes the actual penalty magnitude (matching §11's own
/// "all numeric sizing... is unsized" scope) — deciding which of the dozens of interactions §9 of the
/// Language doc's own cross-reference eventually names should apply this, and by how much, is a real
/// design decision for whichever future pass actually builds that catalog out, not this item's to make
/// unilaterally. A future caller applies this by scaling its own opinion delta (or narration) against
/// <see cref="Severity"/>.</summary>
public enum LanguageBarrierSeverity
{
    /// <summary>Shared language at Conversational-or-better on at least one side, or no tracked
    /// Proficiency data at all for either party (the ambient-population default, per §3's own "not
    /// tracked" restraint — this system never penalizes a pair it has no data on).</summary>
    None,

    /// <summary>Some tracked overlap, but only at <see cref="FluencyTier.Basic"/> — "enough for simple
    /// trade," not comfortable daily exchange (§4).</summary>
    Halting,

    /// <summary>No tracked overlap at all between two Characters who otherwise both have at least one
    /// tracked Proficiency — genuinely no shared language (§6's own "share no language" case).</summary>
    NoSharedLanguage,
}

public static class InteractionLanguageBarrier
{
    /// <summary>The higher (worse) tier either party's own best mutual proficiency reaches, across every
    /// language both Characters have any tracked Proficiency in. Symmetric in its two Character
    /// arguments.</summary>
    public static LanguageBarrierSeverity Severity(WorldState state, RuntimeId<Character> firstId, RuntimeId<Character> secondId)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var firstLanguages = LanguageProficiencyQueries.ForCharacter(state, firstId);
        var secondLanguages = LanguageProficiencyQueries.ForCharacter(state, secondId);
        if (firstLanguages.Count == 0 || secondLanguages.Count == 0)
            return LanguageBarrierSeverity.None;

        var best = LanguageBarrierSeverity.NoSharedLanguage;
        foreach (var mine in firstLanguages)
        {
            foreach (var theirs in secondLanguages)
            {
                if (mine.LanguageId != theirs.LanguageId)
                    continue;

                var lowerTier = mine.FluencyTier < theirs.FluencyTier ? mine.FluencyTier : theirs.FluencyTier;
                if (lowerTier is FluencyTier.Conversational or FluencyTier.FluentNative)
                    return LanguageBarrierSeverity.None;
                if (lowerTier == FluencyTier.Basic)
                    best = LanguageBarrierSeverity.Halting;
            }
        }

        return best;
    }
}
