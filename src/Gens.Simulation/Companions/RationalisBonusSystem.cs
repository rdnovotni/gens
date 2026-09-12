using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>Emitted the first month a household's Rationalis cluster becomes fully staffed and present
/// (edge-triggered — see <see cref="RationalisBonusSystem"/>'s own doc comment), or the first month it
/// stops being so.</summary>
public sealed record RationalisClusterStatusChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    bool Active,
    string? CausationId) : IDomainEvent
{
    public string Type => "companions.rationalisClusterStatusChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// The monthly Rationalis cluster bonus tick (Phase 17 item 1; §5.3): while a household's <see
/// cref="SeniorPositionTitle.Treasurer"/>, <see cref="OverseerRole.Argentarius"/> (the Overseer tier,
/// not a Senior Position), <see cref="SeniorPositionTitle.InstitorMaximus"/>, and <see
/// cref="SeniorPositionTitle.Cellarer"/> are all concurrently active and present (not on-leave) for a
/// household with an active <see cref="SeniorPositionTitle.Rationalis"/>, applies a monthly Dignitas
/// trickle via the same <see cref="AdjustDignitasCommand"/>/<see
/// cref="AdjustDignitasCommands.Pipeline"/> pattern <see cref="Magistracies.MagistracyTermSystem"/>
/// already uses for its own office trickle — reuse, not a new ledger line. Re-derives "is the cluster
/// filled" from <see cref="OverseerAssignment"/>/<see cref="SeniorPositionAssignment"/> fresh every
/// tick (no cached boolean drives the trickle itself, matching <see
/// cref="Magistracies.MagistracyTermSystem"/>'s own style).
///
/// <b>Edge-triggering:</b> the trickle applies every month the cluster holds, same as an ordinary
/// office trickle — but the separate <see cref="RationalisClusterStatusChangedEvent"/> should fire only
/// on the tick the cluster's completeness actually flips, not every month it holds. Detecting that
/// transition purely by re-deriving current state each tick is not possible for every edge (returning
/// from a retinue trip clears <see cref="OverseerAssignment.OnLeaveSince"/>/<see
/// cref="SeniorPositionAssignment.OnLeaveSince"/> back to <c>null</c>, which erases the one timestamp
/// that would otherwise mark "just became active again"), so this system keeps one small piece of its
/// own bookkeeping — <see cref="WorldState.RationalisClusterActiveHouseholds"/>, a sparse "currently
/// active" marker this system alone reads and writes — to know which households were active as of last
/// month. This is a deliberate, narrow deviation from the original plan's "no cached boolean, exactly
/// two new registries" wiring assumption: true edge-triggering for a toggling condition needs one bit of
/// memory somewhere, and every other registry available here already means something else.
/// </summary>
public sealed class RationalisBonusSystem : IMonthlySystem<WorldState>
{
    public string Id => "companions.rationalisBonus";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } =
        new[] { "overseerAssignments", "seniorPositionAssignments", "characters", "rationalisClusterActiveHouseholds" };
    public IReadOnlyCollection<string> Writes { get; } =
        new[] { "householdReputations", "rationalisClusterActiveHouseholds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "companions.positionVacancy" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var processed = new HashSet<RuntimeId<Household>>();

        foreach (var entry in state.SeniorPositionAssignments.InAscendingOrder())
        {
            var record = entry.Value;
            if (record.Title != SeniorPositionTitle.Rationalis || !SeniorPositionResolver.IsActive(record))
                continue;
            var householdId = record.HouseholdId;
            if (!processed.Add(householdId))
                continue;

            var filled = IsClusterFilled(state, householdId);
            var wasActive = state.RationalisClusterActiveHouseholds.TryGet(householdId, out _);

            if (filled)
            {
                var trickle = new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", context.Date, null, householdId,
                    CompanionsCatalog.RationalisClusterBonusDignitas,
                    "the household's Rationalis cluster (Treasurer, Argentarius, Institor Maximus, Cellarer) is fully staffed and present");
                events.AddRange(AdjustDignitasCommands.Pipeline.Execute(state, trickle).Events);

                if (!wasActive)
                {
                    state.RationalisClusterActiveHouseholds.Add(householdId, context.Date);
                    events.Add(new RationalisClusterStatusChangedEvent(state.EventIds.Issue(), context.Date, householdId, true, CausationId: null));
                }
            }
            else if (wasActive)
            {
                state.RationalisClusterActiveHouseholds.Remove(householdId);
                events.Add(new RationalisClusterStatusChangedEvent(state.EventIds.Issue(), context.Date, householdId, false, CausationId: null));
            }
        }

        // A household that lost its Rationalis entirely (no active record left at all) never reaches
        // the loop above but may still be marked active from a prior month — clear it here.
        foreach (var entry in state.RationalisClusterActiveHouseholds.InAscendingOrder().ToArray())
        {
            if (processed.Contains(entry.Key))
                continue;

            state.RationalisClusterActiveHouseholds.Remove(entry.Key);
            events.Add(new RationalisClusterStatusChangedEvent(state.EventIds.Issue(), context.Date, entry.Key, false, CausationId: null));
        }

        return events;
    }

    private static bool IsClusterFilled(WorldState state, RuntimeId<Household> householdId) =>
        OverseerResolver.IsCurrentlyFilled(state, householdId, OverseerRole.Argentarius) &&
        SeniorPositionResolver.IsCurrentlyFilled(state, householdId, SeniorPositionTitle.Treasurer) &&
        SeniorPositionResolver.IsCurrentlyFilled(state, householdId, SeniorPositionTitle.InstitorMaximus) &&
        SeniorPositionResolver.IsCurrentlyFilled(state, householdId, SeniorPositionTitle.Cellarer);
}
