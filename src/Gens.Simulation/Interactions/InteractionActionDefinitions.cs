using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Interactions;

/// <summary>Registers two Character-to-Character Interaction Catalog entries into the action-definition
/// layer (Phase 10 item 6; <c>gens-characters-design.md</c> §9/§10): <see cref="Befriend"/>, a Quick
/// interaction wrapping the existing <see cref="Characters.RecordInteractionCommand"/> engine, and <see
/// cref="InitiateScheme"/>, a Multi-stage interaction wrapping <see cref="InitiateSchemeCommand"/>. This
/// is deliberately a worked pair, not the full §9 catalog (dozens of named interactions across seven
/// categories) — every other entry belongs to whichever future system actually consumes it (Politics
/// &amp; Patronage, Espionage, Romance &amp; Seduction), matching <see cref="Actors.RivalHouseActionDefinitions"/>'s
/// identical "worked example wiring a real command into <see cref="ActionCatalog"/>" scope. Any caller —
/// a player-facing UI or an NPC decision loop — ranks this catalog through the same <see
/// cref="ActionSelector"/> <see cref="Actors.RivalAmbitionSystem"/> already uses at the house-standing
/// level, satisfying item 6's "for both player and NPC actions" without a second selection mechanism.</summary>
public static class InteractionActionDefinitions
{
    public static readonly DefinitionId<ActionDefinition> Befriend = new("befriend");
    public static readonly DefinitionId<ActionDefinition> InitiateScheme = new("initiate-scheme");

    /// <summary>An invented baseline (matching this codebase's own untuned-numbers convention, e.g.
    /// <see cref="Actors.RivalHouseActionDefinitions"/>'s identical 0.6/0.4 split) — a friendly gesture
    /// costs nothing and carries no risk, so it is scored as the more attractive default of the two.</summary>
    public const int BefriendOpinionDelta = 10;

    public static ActionCatalog BuildCatalog() => new(new[]
    {
        new ActionDefinition(
            id: Befriend,
            nameKey: "actions.befriend.name",
            descriptionKey: "actions.befriend.description",
            targetKind: ActionTargetKind.Character,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.Ordinary,
            eligibility: BefriendEligibility,
            scoreForAi: (state, invocation) => BefriendEligibility(state, invocation) is null ? 0.6 : 0.0,
            projectResult: (state, invocation) => ActionResultProjection.Of($"Extending friendship toward {invocation.TargetId}.")),
        new ActionDefinition(
            id: InitiateScheme,
            nameKey: "actions.initiate-scheme.name",
            descriptionKey: "actions.initiate-scheme.description",
            targetKind: ActionTargetKind.Character,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.WaxSeal,
            eligibility: InitiateSchemeEligibility,
            scoreForAi: (state, invocation) => InitiateSchemeEligibility(state, invocation) is null ? 0.3 : 0.0,
            projectResult: (state, invocation) => ActionResultProjection.Of($"Initiating a scheme against {invocation.TargetId}.")),
    });

    /// <summary>Maps a selected <see cref="Befriend"/> invocation to the concrete <see
    /// cref="RecordInteractionCommand"/> a caller actually submits — the generic action layer carries no
    /// field of its own for the opinion delta/bond change, matching <see
    /// cref="Actors.RivalHouseActionDefinitions.ToDirection"/>'s identical reason for existing.</summary>
    public static RecordInteractionCommand ToRecordInteractionCommand(RuntimeId<Command> commandId, ActionInvocation invocation) =>
        new(
            commandId, invocation.ActorId, invocation.Date, CausationId: null,
            RuntimeId<Character>.Parse(invocation.ActorId), RuntimeId<Character>.Parse(invocation.TargetId!),
            BefriendOpinionDelta, BondTag.Friend, BondTag.None, RelationshipOrigin.Encounter);

    /// <summary>Maps a selected <see cref="InitiateScheme"/> invocation to the concrete <see
    /// cref="InitiateSchemeCommand"/> a caller actually submits.</summary>
    public static InitiateSchemeCommand ToInitiateSchemeCommand(RuntimeId<Command> commandId, ActionInvocation invocation) =>
        new(
            commandId, invocation.ActorId, invocation.Date, CausationId: null,
            RuntimeId<Character>.Parse(invocation.ActorId), RuntimeId<Character>.Parse(invocation.TargetId!),
            SchemeType.Coercive);

    private static ValidationErrorCode? BefriendEligibility(WorldState state, ActionInvocation invocation)
    {
        if (invocation.TargetId is null)
            return RecordInteractionCommands.TargetNotFound;

        var initiatorId = RuntimeId<Character>.Parse(invocation.ActorId);
        var targetId = RuntimeId<Character>.Parse(invocation.TargetId);
        if (initiatorId == targetId)
            return RecordInteractionCommands.SelfInteraction;
        if (!state.Characters.TryGet(initiatorId, out var initiator))
            return RecordInteractionCommands.CharacterNotFound;
        if (!state.Characters.TryGet(targetId, out var target))
            return RecordInteractionCommands.TargetNotFound;
        if (!initiator.IsAlive)
            return RecordInteractionCommands.CharacterDeceased;
        if (!target.IsAlive)
            return RecordInteractionCommands.TargetDeceased;

        return null;
    }

    private static ValidationErrorCode? InitiateSchemeEligibility(WorldState state, ActionInvocation invocation)
    {
        if (invocation.TargetId is null)
            return InitiateSchemeCommands.TargetNotFound;

        var initiatorId = RuntimeId<Character>.Parse(invocation.ActorId);
        var targetId = RuntimeId<Character>.Parse(invocation.TargetId);
        if (initiatorId == targetId)
            return InitiateSchemeCommands.SelfTargeted;
        if (!state.Characters.TryGet(initiatorId, out var initiator))
            return InitiateSchemeCommands.InitiatorNotFound;
        if (!state.Characters.TryGet(targetId, out var target))
            return InitiateSchemeCommands.TargetNotFound;
        if (!initiator.IsAlive)
            return InitiateSchemeCommands.InitiatorDeceased;
        if (!target.IsAlive)
            return InitiateSchemeCommands.TargetDeceased;

        var alreadyInProgress = state.Schemes.InAscendingOrder().Any(entry =>
            entry.Value.Status == SchemeStatus.InProgress &&
            entry.Value.InitiatorCharacterId == initiatorId &&
            entry.Value.TargetCharacterId == targetId &&
            entry.Value.Type == SchemeType.Coercive);
        if (alreadyInProgress)
            return InitiateSchemeCommands.AlreadyInProgress;

        return null;
    }
}
