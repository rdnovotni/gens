#nullable enable

using System;
using System.Collections.Generic;
using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Presentation.Shell;

/// <summary>
/// The Unity application shell's sole holder of authoritative campaign state (Phase 9 item 5, ADR
/// 0013 rule 1). It wraps the same <see cref="WorldState"/>/<see cref="RandomStreamSet"/> pair the
/// headless console runner (<c>tools/Gens.ContentCompiler</c>'s <c>new-campaign</c>/<c>advance</c>/
/// <c>submit-command</c>) already owns, exposing only the two sanctioned entry points ADR 0013
/// draws the UI boundary around: <see cref="Query{TProjection}"/> and <see cref="Submit{TCommand}"/>.
/// Deliberately free of any <c>UnityEngine</c> reference, so it can be constructed and exercised in
/// a plain test host; <see cref="CampaignShellBehaviour"/> is the engine entry point that owns one
/// at runtime.
/// </summary>
public sealed class CampaignShell
{
    public WorldState State { get; }

    public RandomStreamSet RandomStreams { get; }

    /// <summary>The player's household, issued at bootstrap (<see cref="CampaignBootstrapper"/>) — the
    /// implicit subject every screen's queries scope to until Phase 10's rival houses give the shell
    /// more than one household to ever look at.</summary>
    public RuntimeId<Household> HouseholdId { get; }

    /// <summary>The settlement the player's household starts in, issued alongside <see
    /// cref="HouseholdId"/> at bootstrap.</summary>
    public RuntimeId<Settlement> SettlementId { get; }

    private CampaignShell(
        WorldState state, RandomStreamSet randomStreams, RuntimeId<Household> householdId, RuntimeId<Settlement> settlementId)
    {
        State = state;
        RandomStreams = randomStreams;
        HouseholdId = householdId;
        SettlementId = settlementId;
    }

    /// <summary>Bootstraps a fresh campaign from <paramref name="config"/>, returning the shell that
    /// owns it alongside the initial history the caller should surface before the first tick runs
    /// (mirroring <c>NewCampaignCommand</c>'s own console output).</summary>
    public static CampaignShell Bootstrap(CampaignConfig config, out IReadOnlyList<IDomainEvent> initialHistory)
    {
        var campaign = CampaignBootstrapper.Bootstrap(config);
        initialHistory = campaign.InitialHistory;
        return new CampaignShell(campaign.State, campaign.RandomStreams, campaign.HouseholdId, campaign.SettlementId);
    }

    /// <summary>The sole read path (ADR 0013): executes <paramref name="query"/> against the shell's
    /// own <see cref="WorldState"/> for <paramref name="observerId"/> and returns the resulting
    /// projection DTO. Never exposes <see cref="WorldState"/> itself to callers.</summary>
    public TProjection Query<TProjection>(IWorldQuery<TProjection> query, string observerId)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        return query.Execute(State, observerId);
    }

    /// <summary>The sole write path (ADR 0013): runs <paramref name="command"/> through its own
    /// <paramref name="pipeline"/> against the shell's <see cref="WorldState"/>. Adapters and screens
    /// never set a field on a domain object directly, under any circumstance.</summary>
    public CommandResult Submit<TCommand>(CommandPipeline<WorldState, TCommand> pipeline, TCommand command)
        where TCommand : ICommand
    {
        if (pipeline is null)
            throw new ArgumentNullException(nameof(pipeline));

        return pipeline.Execute(State, command);
    }

    /// <summary>Advances the campaign one month, mirroring <c>AdvanceCommand</c>'s pairing of a
    /// <see cref="WriteSetVerifyingSimulation"/> tick with <see cref="WorldState.AdvanceMonth"/>. The
    /// full pause/advance UI is Phase 9 item 8's job; this method only owns the state transition.</summary>
    public IReadOnlyList<IDomainEvent> AdvanceMonth(IEnumerable<IMonthlySystem<WorldState>> systems)
    {
        if (systems is null)
            throw new ArgumentNullException(nameof(systems));

        var simulation = new WriteSetVerifyingSimulation(systems);
        var events = simulation.Tick(State, State.Date, RandomStreams);
        State.AdvanceMonth();
        return events;
    }
}
