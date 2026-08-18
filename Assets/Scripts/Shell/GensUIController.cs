#nullable enable

using System;
using System.Collections.Generic;
using Gens.Presentation.Adapters;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Queries;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gens.Presentation.Shell;

/// <summary>The persistent ink bar plus the four first-class screens Phase 9 item 6 names —
/// household roster, estate/settlement, monthly report, and character detail. Owns the one
/// <see cref="UIDocument"/> for the running scene: mounts the ink-bar UXML once at the top (it
/// "persists across every screen, diptych or not", <c>gens-core-design.md</c> §7.4) and swaps the
/// screen host's content on navigation. Every field this controller displays comes from a <see
/// cref="CampaignShell.Query{TProjection}"/> call translated through this screen's own adapter
/// (ADR 0013) — it never reads <c>WorldState</c> directly.</summary>
public sealed class GensUIController : MonoBehaviour
{
    [SerializeField]
    private CampaignShellBehaviour shellBehaviour = null!;

    [SerializeField]
    private UIDocument document = null!;

    [SerializeField]
    private VisualTreeAsset inkBarAsset = null!;

    [SerializeField]
    private VisualTreeAsset householdRosterAsset = null!;

    [SerializeField]
    private VisualTreeAsset estateSettlementAsset = null!;

    [SerializeField]
    private VisualTreeAsset monthlyReportAsset = null!;

    [SerializeField]
    private VisualTreeAsset characterDetailAsset = null!;

    private const string PlayerObserverId = "player";

    private VisualElement _screenHost = null!;
    private IReadOnlyList<IDomainEvent> _lastMonthEvents = Array.Empty<IDomainEvent>();

    private void Start()
    {
        var shell = shellBehaviour.Shell ?? throw new InvalidOperationException(
            $"{nameof(GensUIController)} requires {nameof(CampaignShellBehaviour)} to have bootstrapped its shell first.");
        _lastMonthEvents = shellBehaviour.InitialHistory;

        var root = document.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;

        var inkBar = inkBarAsset.CloneTree();
        root.Add(inkBar);

        _screenHost = new VisualElement { name = "screen-host" };
        _screenHost.style.flexGrow = 1;
        root.Add(_screenHost);

        RefreshInkBar(shell);
        ShowHouseholdRoster();
    }

    /// <summary>Re-applies each month's ink-bar figures. Phase 9 item 8's pause/advance UI is
    /// expected to call this (and <see cref="ApplyMonthlyEvents"/>) after every
    /// <see cref="CampaignShell.AdvanceMonth"/> — this controller only owns rendering, not the
    /// advance action itself.</summary>
    public void RefreshInkBar()
    {
        if (shellBehaviour.Shell is { } shell)
            RefreshInkBar(shell);
    }

    private void RefreshInkBar(CampaignShell shell)
    {
        var projection = shell.Query(new InkBarQuery(shell.HouseholdId), PlayerObserverId);
        var viewModel = new InkBarAdapter().Adapt(projection);
        InkBarBinding.Apply(document.rootVisualElement, viewModel);
    }

    /// <summary>Feeds the Monthly Report screen the events one <see
    /// cref="CampaignShell.AdvanceMonth"/> call produced. Kept as a public seam rather than called
    /// internally, since this controller (Phase 9 item 6) does not itself own advancing the
    /// campaign (Phase 9 item 8).</summary>
    public void ApplyMonthlyEvents(IReadOnlyList<IDomainEvent> events)
    {
        _lastMonthEvents = events ?? throw new ArgumentNullException(nameof(events));
        if (document.rootVisualElement.Q<VisualElement>("monthly-report-screen") is not null)
            ShowMonthlyReport();
    }

    public void ShowHouseholdRoster()
    {
        var shell = RequireShell();
        var screen = MountScreen(householdRosterAsset);

        var projection = shell.Query(new HouseholdRosterQuery(shell.HouseholdId), PlayerObserverId);
        var viewModel = new HouseholdRosterAdapter().Adapt(projection);
        HouseholdRosterBinding.Apply(screen, viewModel, characterId => ShowCharacterDetail(RuntimeId<Character>.Parse(characterId)));
    }

    public void ShowEstateSettlement()
    {
        var shell = RequireShell();
        var screen = MountScreen(estateSettlementAsset);

        var projection = shell.Query(new EstateSettlementQuery(shell.SettlementId, shell.HouseholdId), PlayerObserverId);
        var viewModel = new EstateSettlementAdapter().Adapt(projection);
        EstateSettlementBinding.Apply(screen, viewModel);
    }

    public void ShowMonthlyReport()
    {
        var shell = RequireShell();
        var screen = MountScreen(monthlyReportAsset);

        var financials = shell.Query(new HouseholdFinancialsQuery(shell.HouseholdId), PlayerObserverId);
        var report = MonthlyReportProjector.Project(shell.State.Date, _lastMonthEvents);
        var viewModel = MonthlyReportAdapter.Adapt(financials, report);
        MonthlyReportBinding.Apply(screen, viewModel);
    }

    public void ShowCharacterDetail(RuntimeId<Character> characterId)
    {
        var shell = RequireShell();
        var screen = MountScreen(characterDetailAsset);

        var projection = shell.Query(new CharacterDetailQuery(characterId), PlayerObserverId);
        var viewModel = new CharacterDetailAdapter().Adapt(projection);
        CharacterDetailBinding.Apply(screen, viewModel);
    }

    private CampaignShell RequireShell() =>
        shellBehaviour.Shell ?? throw new InvalidOperationException(
            $"{nameof(GensUIController)} requires {nameof(CampaignShellBehaviour)} to have bootstrapped its shell first.");

    private VisualElement MountScreen(VisualTreeAsset asset)
    {
        _screenHost.Clear();
        var instance = asset.CloneTree();
        _screenHost.Add(instance);
        return instance;
    }
}
