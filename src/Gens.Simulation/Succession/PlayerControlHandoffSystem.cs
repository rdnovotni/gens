using Gens.Simulation.Commands;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Succession;

/// <summary>
/// The monthly system that keeps <see cref="PlayerControlState"/> in sync with the household/world
/// distinction the roadmap item names (Phase 11 item 2; §6). Runs last among the four Succession
/// systems (after <see cref="SuccessionHandoffSystem"/>, <see cref="SuccessionDisputeResolutionSystem"/>,
/// and <see cref="RegencySystem"/>, same month) so it always observes the fully-settled post-handoff
/// headship state before deciding who — if anyone — the player controls this month.
///
/// No-ops entirely when <see cref="WorldState.PlayerControls"/> has no entry yet (no player household
/// established, the common case in unit tests that never call <see
/// cref="EstablishPlayerControlCommand"/>). Otherwise recomputes the target <see
/// cref="PlayerControlState"/> via <see cref="PlayerControlResolver"/> — the same branching logic <see
/// cref="PlayerControlCommands"/> uses at establishment — and, only when the computed target actually
/// differs from what is stored, replaces the entry and emits <see cref="PlayerControlChangedEvent"/>.
/// An unchanged target produces no write and no event at all, matching <see
/// cref="Stewardship.StewardshipCommands"/>'s <c>AutonomyLevelUnchanged</c> guard's identical
/// "only act on actual change" discipline.
/// </summary>
public sealed class PlayerControlHandoffSystem : IMonthlySystem<WorldState>
{
    public string Id => "succession.playerControlHandoff";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "playerControls", "householdHeadships", "stewardshipAssignments" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "playerControls", "eventIds" };

    public IReadOnlyCollection<string> Prerequisites { get; } =
        new[] { "succession.handoff", "succession.disputeResolution", "succession.regency" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // At most one entry exists today (one player household per campaign) — see
        // PlayerControlState's own doc comment for why this is still a registry, not a singleton field.
        foreach (var (householdId, current) in state.PlayerControls.InAscendingOrder().ToArray())
        {
            var target = PlayerControlResolver.Resolve(state, householdId);
            if (target.ControlledCharacterId == current.ControlledCharacterId && target.Mode == current.Mode)
                continue;

            state.PlayerControls.Remove(householdId);
            state.PlayerControls.Add(householdId, target);

            events.Add(new PlayerControlChangedEvent(
                state.EventIds.Issue(), context.Date, householdId,
                current.ControlledCharacterId, target.ControlledCharacterId, current.Mode, target.Mode));
        }

        return events;
    }
}
