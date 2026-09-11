using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Languages;
using Gens.Simulation.State;

namespace Gens.Simulation.Diplomacy;

/// <summary>§5's <c>InterpreterQuality</c> source, resolved fresh at negotiation time and never
/// persisted (§13's own data-model note: "resolved at negotiation time, not persistently stored").</summary>
public enum FrontierNegotiationQualitySource
{
    /// <summary>No qualified speaker at all — the design doc's own §5 frames this as "a real, meaningful
    /// penalty," but the already-built <see cref="DiplomacyLanguageGateEvaluator"/> this evaluator wraps
    /// implements a hard gate rather than a soft one; a caller proposing a formal treaty on this result
    /// must reject rather than merely penalize (see <see cref="ProposeFrontierTreatyCommands"/>'s own
    /// <c>languageGateNotCleared</c> error). Diplomatic Gifts remain gate-exempt and only use this
    /// source to skip the goodwill bonus a fluent envoy would otherwise earn.</summary>
    None,

    /// <summary>A hired Interpres (formal <see cref="InterpresAppointment"/> or an informally qualified
    /// household member) closes most, but not all, of the gap (§5's "a real, purchasable partial fix").</summary>
    InterpresPresent,

    /// <summary>The negotiator's own Conversational-or-better fluency in the target people's language —
    /// a full-strength baseline (§5).</summary>
    NegotiatorFluency,

    /// <summary>The negotiator's own origin culture exactly matches the target people's culture — §5's
    /// top tier, "a real, full-strength baseline with no penalty at all... the direct, concrete payoff
    /// for every Foreign Tutor, Institution of Renown, or blended-marriage choice." Cultural Drift (a
    /// Character's culture shifting away from their origin through sustained exposure) does not exist
    /// yet anywhere in this codebase — Education &amp; Culture is a later phase — so this slice reads
    /// only an exact <see cref="Character.Culture"/> match, an honest, disclosed gap rather than a
    /// fabricated drift mechanic.</summary>
    CulturalFamiliarity,
}

/// <summary>The ephemeral, never-persisted result of evaluating §5's Interpreter Problem for one
/// negotiation attempt.</summary>
public readonly record struct FrontierNegotiationQuality(
    FrontierNegotiationQualitySource Source, RuntimeId<Character>? InterpresCharacterId, int QualityModifierPercent)
{
    public bool GateCleared => Source != FrontierNegotiationQualitySource.None;
}

/// <summary>
/// Evaluates §5's Interpreter Problem for a negotiator attempting to deal with a Foreign People —
/// the first real caller of <see cref="DiplomacyLanguageGateEvaluator"/>, which was built and tested in
/// Phase 13 item 4 with "no actual Diplomacy negotiation flow to call it from yet... named as the future
/// caller." Layers §5's three-way quality read (Cultural Familiarity > Negotiator Fluency > Interpres)
/// on top of that gate's own cleared/uncleared result: a negotiator whose culture matches the target
/// people's but who does not actually speak their language still needs an Interpres to clear the gate at
/// all — cultural familiarity upgrades the *quality* of an already-cleared negotiation, it does not
/// itself substitute for the language.
/// </summary>
public static class FrontierNegotiationQualityEvaluator
{
    public static FrontierNegotiationQuality Evaluate(
        WorldState state,
        RuntimeId<Character> negotiatorId,
        RuntimeId<Household> negotiatingHouseholdId,
        RuntimeId<Actor> foreignPeopleActorId,
        CultureLanguageMap cultureLanguages)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (cultureLanguages is null)
            throw new ArgumentNullException(nameof(cultureLanguages));

        if (!ForeignPeopleQueries.TryGet(state, foreignPeopleActorId, out var details))
            return new FrontierNegotiationQuality(FrontierNegotiationQualitySource.None, null, 0);

        var requiredLanguage = cultureLanguages.Resolve(details.CultureId);
        if (requiredLanguage is null)
            return new FrontierNegotiationQuality(FrontierNegotiationQualitySource.None, null, 0);

        var gate = DiplomacyLanguageGateEvaluator.Evaluate(state, negotiatorId, requiredLanguage.Value, negotiatingHouseholdId);
        if (!gate.Cleared)
            return new FrontierNegotiationQuality(FrontierNegotiationQualitySource.None, null, 0);

        if (gate.GateClearedBy == LanguageGateClearedBy.NegotiatorFluency &&
            state.Characters.TryGet(negotiatorId, out var negotiator) && negotiator!.Culture == details.CultureId)
        {
            return new FrontierNegotiationQuality(
                FrontierNegotiationQualitySource.CulturalFamiliarity, null, FrontierDiplomacyCatalog.CulturalFamiliarityBonusPercent);
        }

        return gate.GateClearedBy == LanguageGateClearedBy.NegotiatorFluency
            ? new FrontierNegotiationQuality(FrontierNegotiationQualitySource.NegotiatorFluency, null, FrontierDiplomacyCatalog.NegotiatorFluencyBonusPercent)
            : new FrontierNegotiationQuality(
                FrontierNegotiationQualitySource.InterpresPresent, gate.InterpresCharacterId, -FrontierDiplomacyCatalog.InterpresPenaltyPercent);
    }
}
