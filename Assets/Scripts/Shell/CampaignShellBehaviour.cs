#nullable enable

using Gens.Simulation.Campaign;
using Gens.Simulation.Time;
using UnityEngine;

namespace Gens.Presentation.Shell;

/// <summary>
/// The application shell's single Unity entry point (Phase 9 item 5): owns the one
/// <see cref="CampaignShell"/> instance for the running scene and is the only component permitted
/// to construct or replace it. Every other presentation-layer component (adapters, screens) reaches
/// campaign state exclusively through this behaviour's <see cref="Shell"/> property, never by
/// holding its own reference to <c>WorldState</c> — the concrete enforcement point for ADR 0013's
/// one-way boundary on the Unity side.
/// </summary>
public sealed class CampaignShellBehaviour : MonoBehaviour
{
    [SerializeField]
    private ulong seed = 1;

    [SerializeField]
    private int startMonths;

    [SerializeField]
    private string rulesetId = "default";

    [SerializeField]
    private string contentPackHash = string.Empty;

    [SerializeField]
    private string regionId = string.Empty;

    [SerializeField]
    private string? startProfileId;

    [SerializeField]
    private string difficulty = "normal";

    /// <summary>The running scene's authoritative campaign state holder. Null until <see cref="Awake"/>
    /// has bootstrapped it.</summary>
    public CampaignShell? Shell { get; private set; }

    private void Awake()
    {
        var config = new CampaignConfig
        {
            Seed = seed,
            StartDate = new GameDate(startMonths),
            RulesetId = rulesetId,
            ContentPackHash = contentPackHash,
            RegionId = regionId,
            StartProfileId = startProfileId,
            Difficulty = difficulty,
        };

        Shell = CampaignShell.Bootstrap(config, out var initialHistory);
        foreach (var domainEvent in initialHistory)
            Debug.Log($"[Gens] {domainEvent.Type}: {string.Join(", ", domainEvent.SubjectIds)}");
    }
}
