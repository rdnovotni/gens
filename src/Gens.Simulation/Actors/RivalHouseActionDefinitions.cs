using Gens.Simulation.Actions;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Actors;

/// <summary>Registers <see cref="AdjustHouseStandingCommand"/>'s two directions into the action-
/// definition layer, matching <see cref="Policies.PolicyActionDefinitions"/>'s identical "worked
/// example wiring a real command into <see cref="ActionCatalog"/>" shape (Phase 10 item 5/6). <see
/// cref="RivalAmbitionSystem"/> consumes this catalog through the same <see cref="Actions.ActionSelector"/>
/// a player-facing UI would.</summary>
public static class RivalHouseActionDefinitions
{
    public static readonly DefinitionId<ActionDefinition> SeekAlliance = new("seek-alliance");
    public static readonly DefinitionId<ActionDefinition> DeclareRivalry = new("declare-rivalry");

    public static ActionCatalog BuildCatalog() => new(new[]
    {
        new ActionDefinition(
            id: SeekAlliance,
            nameKey: "actions.seek-alliance.name",
            descriptionKey: "actions.seek-alliance.description",
            targetKind: ActionTargetKind.Actor,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.Ordinary,
            eligibility: (state, invocation) => Eligibility(state, invocation, HouseStandingAdjustmentDirection.TowardAlliance),
            scoreForAi: (state, invocation) => Score(state, invocation, HouseStandingAdjustmentDirection.TowardAlliance),
            projectResult: (state, invocation) => Project(state, invocation, HouseStandingAdjustmentDirection.TowardAlliance)),
        new ActionDefinition(
            id: DeclareRivalry,
            nameKey: "actions.declare-rivalry.name",
            descriptionKey: "actions.declare-rivalry.description",
            targetKind: ActionTargetKind.Actor,
            cost: ActionCost.None,
            duration: ActionDuration.Instant,
            confirmation: ActionConfirmationSeverity.WaxSeal,
            eligibility: (state, invocation) => Eligibility(state, invocation, HouseStandingAdjustmentDirection.TowardRivalry),
            scoreForAi: (state, invocation) => Score(state, invocation, HouseStandingAdjustmentDirection.TowardRivalry),
            projectResult: (state, invocation) => Project(state, invocation, HouseStandingAdjustmentDirection.TowardRivalry)),
    });

    /// <summary>Maps a selected <see cref="ActionDefinition.Id"/> back to the concrete <see
    /// cref="HouseStandingAdjustmentDirection"/> a caller needs to actually submit an <see
    /// cref="AdjustHouseStandingCommand"/> — the generic layer's <see cref="ActionInvocation"/> carries
    /// no field of its own for this, matching <see cref="Policies.PolicyActionDefinitions"/>'s own
    /// household-ID-parsing helper for the same reason.</summary>
    public static HouseStandingAdjustmentDirection ToDirection(DefinitionId<ActionDefinition> id) =>
        id == SeekAlliance ? HouseStandingAdjustmentDirection.TowardAlliance
        : id == DeclareRivalry ? HouseStandingAdjustmentDirection.TowardRivalry
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Not a RivalHouseActionDefinitions entry.");

    private static ValidationErrorCode? Eligibility(WorldState state, ActionInvocation invocation, HouseStandingAdjustmentDirection direction)
    {
        if (invocation.TargetId is null)
            return AdjustHouseStandingCommands.UnknownActor;

        var initiatorId = RuntimeId<Actor>.Parse(invocation.ActorId);
        var targetId = RuntimeId<Actor>.Parse(invocation.TargetId);
        if (initiatorId == targetId)
            return AdjustHouseStandingCommands.SameActor;
        if (!state.Actors.TryGet(initiatorId, out _) || !state.Actors.TryGet(targetId, out _))
            return AdjustHouseStandingCommands.UnknownActor;

        var current = HouseStandingResolver.GetEffectiveStanding(state, initiatorId, targetId);
        if (direction == HouseStandingAdjustmentDirection.TowardAlliance && current == HouseStandingLevel.Allied)
            return AdjustHouseStandingCommands.AlreadyAtExtreme;
        if (direction == HouseStandingAdjustmentDirection.TowardRivalry && current == HouseStandingLevel.Feuding)
            return AdjustHouseStandingCommands.AlreadyAtExtreme;

        if (direction == HouseStandingAdjustmentDirection.TowardAlliance)
        {
            var key = HouseStandingKey.Between(initiatorId, targetId);
            if (state.HouseStandings.TryGet(key, out var existing) && existing!.Grudge is { } grudge &&
                AncestralGrudgeCatalog.IsActive(invocation.Date, grudge))
                return AdjustHouseStandingCommands.BlockedByAncestralGrudge;
        }

        return null;
    }

    /// <summary>An invented baseline (matching this codebase's own "§10 untuned-numbers" convention):
    /// slightly prefers alliance-seeking over rivalry by default, since nothing here yet reads the
    /// initiator's personality — <see cref="RivalAmbitionSystem"/> is what layers Ambition/Boldness on
    /// top of this base score before deciding whether, and which candidate target, to actually act
    /// on.</summary>
    private static double Score(WorldState state, ActionInvocation invocation, HouseStandingAdjustmentDirection direction) =>
        Eligibility(state, invocation, direction) is null
            ? direction == HouseStandingAdjustmentDirection.TowardAlliance ? 0.6 : 0.4
            : 0.0;

    private static ActionResultProjection Project(WorldState state, ActionInvocation invocation, HouseStandingAdjustmentDirection direction)
    {
        var verb = direction == HouseStandingAdjustmentDirection.TowardAlliance ? "Seeking alliance with" : "Declaring rivalry against";
        return ActionResultProjection.Of($"{verb} {invocation.TargetId}.");
    }
}
