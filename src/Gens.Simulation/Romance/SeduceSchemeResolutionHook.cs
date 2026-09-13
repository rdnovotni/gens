using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Interactions;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Romance;

/// <summary>
/// The additive hook <see cref="SchemeProgressSystem"/>'s own <c>Resolve</c> calls right after resolving
/// any <see cref="Scheme"/> — a plain static helper, not an <see cref="Time.IMonthlySystem{TState}"/>
/// itself, mirroring the Legal case-type resolution hooks' identical "one additional call from an
/// already-existing resolution point" precedent rather than adding a second tick loop over the same
/// <see cref="State.WorldState.Schemes"/> partition. A no-op for every <see cref="Scheme"/> that is not
/// both <see cref="SchemeType.Seduce"/> and freshly <see cref="SchemeStatus.Succeeded"/>.
///
/// On success (<c>gens-romance-sexuality-lineage-design.md</c> §7):
/// <list type="bullet">
/// <item>Grants <see cref="BondTag.BlackmailLeverage"/> from initiator toward target, directed, via <see
/// cref="RecordInteractionCommand"/> — confirmed by search to be the first real granter of this tag
/// anywhere in the codebase; every other reference is only its enum declaration and its enumeration in
/// <c>WorldStateMapper</c>'s save mapping.</item>
/// <item>Advances the pair's <see cref="RomanticBond"/> by a small positive Affection/Attraction delta via
/// <see cref="RecordRomanticInteractionCommand"/> (§7's successful seduction deepening the bond, not just
/// resolving the Scheme).</item>
/// </list>
///
/// §7 also describes a successful Seduce as potentially *unlocking an option* for a Rival-House
/// Alliance, Recruitment, or an Information payoff — this hook deliberately stops at the two bullets
/// above and does not implement any of those three. They are described in the design document as
/// preconditions a later Espionage/Rival-Houses feature consumes, not actions this item auto-executes —
/// matching how the Companions item left its own forward hooks for a later phase to pick up rather than
/// guessing at that later phase's own shape. This hook is deliberately minimal: no Rival House lookup, no
/// <c>AdjustHouseStandingCommand</c> call, nothing beyond the BlackmailLeverage grant and the bond
/// advance.
/// </summary>
public static class SeduceSchemeResolutionHook
{
    public static IReadOnlyList<IDomainEvent> Apply(WorldState state, Scheme scheme, GameDate date)
    {
        if (scheme.Type != SchemeType.Seduce || scheme.Status != SchemeStatus.Succeeded)
            return Array.Empty<IDomainEvent>();

        var events = new List<IDomainEvent>();

        events.AddRange(RecordInteractionCommands.Pipeline.Execute(
            state,
            new RecordInteractionCommand(
                state.CommandIds.Issue(), "system", date, scheme.SchemeId.ToTaggedString(),
                scheme.InitiatorCharacterId, scheme.TargetCharacterId,
                OpinionDelta: 0, BondsGranted: BondTag.BlackmailLeverage, BondsRevoked: BondTag.None,
                Origin: RelationshipOrigin.Encounter)).Events);

        events.AddRange(RecordRomanticInteractionCommands.Pipeline.Execute(
            state,
            new RecordRomanticInteractionCommand(
                state.CommandIds.Issue(), "system", date, scheme.SchemeId.ToTaggedString(),
                scheme.InitiatorCharacterId, scheme.TargetCharacterId,
                RomanceCatalog.SeduceSuccessAffectionDelta, RomanceCatalog.SeduceSuccessAttractionDelta,
                BondTypeOverride: null)).Events);

        return events;
    }
}
