using Gens.Simulation.Actions;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Schemes;

/// <summary>Registers one <see cref="ActionDefinition"/> per <see cref="SchemeType"/> — exposing the
/// Scheme engine "as an available action for the player's household" (Phase 10 item 13) exactly the
/// way <see cref="Actors.RivalHouseActionDefinitions"/> exposed House Standing adjustment: any caller
/// with this catalog (a player-facing UI, <see cref="Actors.RivalAmbitionSystem"/>, a future steward
/// extension) selects through the same <see cref="Actions.ActionSelector"/> everyone else uses.</summary>
public static class SchemeActionDefinitions
{
    public static readonly DefinitionId<ActionDefinition> FabricateHook = new("scheme-fabricate-hook");
    public static readonly DefinitionId<ActionDefinition> Sabotage = new("scheme-sabotage");
    public static readonly DefinitionId<ActionDefinition> Blackmail = new("scheme-blackmail");
    public static readonly DefinitionId<ActionDefinition> Frame = new("scheme-frame");
    public static readonly DefinitionId<ActionDefinition> Assassinate = new("scheme-assassinate");

    private static readonly Dictionary<DefinitionId<ActionDefinition>, SchemeType> TypeById =
        new()
        {
            [FabricateHook] = SchemeType.FabricateHook,
            [Sabotage] = SchemeType.Sabotage,
            [Blackmail] = SchemeType.Blackmail,
            [Frame] = SchemeType.Frame,
            [Assassinate] = SchemeType.Assassinate,
        };

    public static ActionCatalog BuildCatalog() => new(
        TypeById.Select(entry => new ActionDefinition(
            id: entry.Key,
            nameKey: $"actions.{entry.Key.Value}.name",
            descriptionKey: $"actions.{entry.Key.Value}.description",
            targetKind: ActionTargetKind.Character,
            cost: ActionCost.None,
            duration: ActionDuration.Fixed(1),
            confirmation: ActionConfirmationSeverity.WaxSeal,
            eligibility: (state, invocation) => Eligibility(state, invocation, entry.Value),
            scoreForAi: (state, invocation) => Eligibility(state, invocation, entry.Value) is null ? 0.3 : 0.0,
            projectResult: (state, invocation) => Project(invocation, entry.Value))));

    /// <summary>Maps a selected <see cref="ActionDefinition.Id"/> back to its <see cref="SchemeType"/> —
    /// the generic layer's <see cref="ActionInvocation"/> carries no field of its own for this,
    /// matching <see cref="Actors.RivalHouseActionDefinitions.ToDirection"/>'s identical convention.</summary>
    public static SchemeType ToSchemeType(DefinitionId<ActionDefinition> id) =>
        TypeById.TryGetValue(id, out var type) ? type : throw new ArgumentOutOfRangeException(nameof(id), id, "Not a SchemeActionDefinitions entry.");

    private static ValidationErrorCode? Eligibility(WorldState state, ActionInvocation invocation, SchemeType type)
    {
        if (invocation.TargetId is null)
            return SchemeCommands.UnknownCharacter;

        var initiatorId = RuntimeId<Character>.Parse(invocation.ActorId);
        var targetId = RuntimeId<Character>.Parse(invocation.TargetId);
        if (initiatorId == targetId)
            return SchemeCommands.SameCharacter;
        if (!state.Characters.TryGet(initiatorId, out _) || !state.Characters.TryGet(targetId, out _))
            return SchemeCommands.UnknownCharacter;

        var alreadyScheming = state.Schemes.InAscendingOrder().Any(entry =>
            entry.Value.Stage != SchemeStage.Resolved && entry.Value.Type == type &&
            entry.Value.InitiatorCharacterId == initiatorId && entry.Value.TargetCharacterId == targetId);
        return alreadyScheming ? AlreadySchemingErrorFor(type) : null;
    }

    private static readonly Dictionary<SchemeType, ValidationErrorCode> AlreadySchemingErrors = new();

    private static ValidationErrorCode AlreadySchemingErrorFor(SchemeType type)
    {
        if (!AlreadySchemingErrors.TryGetValue(type, out var code))
        {
            code = new ValidationErrorCode($"schemes.initiate.alreadyScheming.{type.ToString().ToLowerInvariant()}");
            AlreadySchemingErrors[type] = code;
        }

        return code;
    }

    private static ActionResultProjection Project(ActionInvocation invocation, SchemeType type) =>
        ActionResultProjection.Of($"Initiating a {type} scheme against {invocation.TargetId}.");
}
