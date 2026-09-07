#nullable enable

using System;
using System.Collections.Generic;
using Gens.Application.Campaign;
using Gens.Simulation.Campaign;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Queries;
using Gens.Simulation.Random;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Presentation.Shell;

/// <summary>
/// Transitional Unity compatibility facade over the engine-neutral <see cref="CampaignSession"/>.
/// It owns no campaign state or random streams of its own; all orchestration delegates to one session.
/// </summary>
public sealed class CampaignShell
{
    private readonly CampaignSession _session;

    private CampaignShell(CampaignSession session) =>
        _session = session ?? throw new ArgumentNullException(nameof(session));

    /// <summary>Exposes the delegated session for lightweight parity checks and gradual migration.</summary>
    public CampaignSession Session => _session;

    /// <summary>Transitional raw-state access retained for existing Unity command construction and tests.</summary>
    public WorldState State => _session.State;

    /// <summary>Transitional RNG access; returns the session's single authoritative stream set.</summary>
    public RandomStreamSet RandomStreams => _session.RandomStreams;

    public RuntimeId<Household> HouseholdId => _session.HouseholdId;

    public RuntimeId<Settlement> SettlementId => _session.SettlementId;

    public string ContentPackHash => _session.ContentPackHash;

    public static CampaignShell Bootstrap(CampaignConfig config, out IReadOnlyList<IDomainEvent> initialHistory) =>
        new(CampaignSession.CreateNew(config, out initialHistory));

    /// <summary>Creates the same seeded, playable vertical slice used by the native client.</summary>
    public static CampaignShell BootstrapPlayable(CampaignStartOptions options, out IReadOnlyList<IDomainEvent> initialHistory) =>
        new(CampaignSession.CreateNew(options, out initialHistory));

    public static CampaignShell Load(string path, out SaveManifest manifest) =>
        new(CampaignSession.Load(path, out manifest));

    public void Save(string path, string gameVersion) => _session.Save(path, gameVersion);

    public ReplayDiagnosticsResult VerifyDeterministicReplay(string diagnosticsPath, string gameVersion) =>
        _session.VerifyDeterministicReplay(diagnosticsPath, gameVersion);

    public TProjection Query<TProjection>(IWorldQuery<TProjection> query, string observerId) =>
        _session.Query(query, observerId);

    public CommandResult Submit<TCommand>(CommandPipeline<WorldState, TCommand> pipeline, TCommand command)
        where TCommand : ICommand => _session.Submit(pipeline, command);

    public IReadOnlyList<IDomainEvent> AdvanceMonth(IEnumerable<IMonthlySystem<WorldState>> systems) =>
        _session.AdvanceMonth(systems);
}
