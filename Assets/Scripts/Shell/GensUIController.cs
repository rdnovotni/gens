#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Gens.Presentation.Adapters;
using Gens.Simulation.Actions;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Policies;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gens.Presentation.Shell;

/// <summary>The persistent ink bar plus the four first-class screens Phase 9 item 6 names —
/// household roster, estate/settlement, monthly report, and character detail — plus the pause/advance
/// clock, command submission through wax-seal/ordinary confirmation, save/load, and deterministic
/// replay diagnostics Phase 9 items 7-8 add on top. Owns the one <see cref="UIDocument"/> for the
/// running scene: mounts the ink-bar UXML once at the top (it "persists across every screen, diptych
/// or not", <c>gens-core-design.md</c> §7.4) and swaps the screen host's content on navigation. Every
/// field this controller displays comes from a <see cref="CampaignShell.Query{TProjection}"/> call
/// translated through this screen's own adapter (ADR 0013) — it never reads <c>WorldState</c>
/// directly.</summary>
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

    [SerializeField]
    private VisualTreeAsset confirmationDialogAsset = null!;

    private const string PlayerObserverId = "player";
    private const string SaveFileName = "quicksave.gens";

    /// <summary>The month-tick system list <see cref="AdvanceMonthNow"/> runs, mirroring the console
    /// runner's own <c>advance</c> verb (<c>AdvanceCommand</c>), which likewise only runs <see
    /// cref="ScheduledActionSystem"/> today.</summary>
    private static readonly IReadOnlyList<IMonthlySystem<WorldState>> MonthlySystems =
        new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() };

    /// <summary>The one concrete action catalog Phase 9 items 1-2 built (<see
    /// cref="PolicyActionDefinitions"/>) — content is stateless and side-effect-free, so building it
    /// once per controller instance is as cheap as caching it would be.</summary>
    private readonly ActionCatalog _actionCatalog = PolicyActionDefinitions.BuildCatalog();

    private VisualElement _screenHost = null!;
    private VisualElement _confirmationOverlay = null!;
    private IReadOnlyList<IDomainEvent> _lastMonthEvents = Array.Empty<IDomainEvent>();

    /// <summary>The action the confirmation overlay's confirm control (wax-seal press or plain
    /// "Confirm"/"OK" button) invokes when next clicked. Set by <see cref="ShowConfirmation"/>, cleared
    /// by <see cref="HideConfirmation"/> — a single pending pair rather than per-call event
    /// subscriptions, since only one confirmation can ever be on screen at once.</summary>
    private Action? _pendingConfirm;
    private Action? _pendingCancel;

    /// <summary>The campaign clock starts paused — nothing advances the campaign until the player
    /// explicitly resumes or presses the manual Advance control, matching every existing "advance" verb
    /// in this codebase (the console runner's <c>advance</c>) being a deliberate, one-shot step rather
    /// than something that happens on its own.</summary>
    private bool _isPaused = true;

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

        root.Add(confirmationDialogAsset.CloneTree());
        _confirmationOverlay = root.Q<VisualElement>("confirmation-overlay") ?? throw new InvalidOperationException(
            $"{nameof(confirmationDialogAsset)} must define a 'confirmation-overlay' root element.");
        WireConfirmationDialog();

        WireCampaignClockControls();
        WireSaveLoadDiagnosticsControls();

        RefreshInkBar(shell);
        RefreshClockControls();
        ShowHouseholdRoster();
    }

    /// <summary>Re-applies each month's ink-bar figures. Called after every <see
    /// cref="CampaignShell.AdvanceMonth"/> (see <see cref="AdvanceMonthNow"/>) and after a load (see
    /// <see cref="LoadNow"/>) — this controller only owns rendering, not the advance/load action
    /// itself.</summary>
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
    /// cref="CampaignShell.AdvanceMonth"/> call produced.</summary>
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
        EstateSettlementBinding.Apply(screen, viewModel, RequestChangeRitesBudget, RequestFundFestival);
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
        PortraitBinding.Apply(screen, PortraitAdapter.Adapt(viewModel.NameLabel, projection.VisualProfile));
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

    #region Phase 9 item 8 — pause/advance

    private void WireCampaignClockControls()
    {
        var root = document.rootVisualElement;
        var pauseToggle = root.Q<Button>("ink-bar-pause-toggle");
        var advance = root.Q<VisualElement>("ink-bar-advance");

        if (pauseToggle is not null)
            pauseToggle.clicked += () =>
            {
                _isPaused = !_isPaused;
                RefreshClockControls();
            };

        if (advance is not null)
            advance.RegisterCallback<ClickEvent>(_ =>
            {
                if (_isPaused)
                    return;

                PressWaxSeal(advance);
                AdvanceMonthNow();
            });
    }

    private void RefreshClockControls()
    {
        var root = document.rootVisualElement;
        var status = root.Q<Label>("ink-bar-clock-status");
        var pauseToggle = root.Q<Button>("ink-bar-pause-toggle");
        var advance = root.Q<VisualElement>("ink-bar-advance");

        if (status is not null)
            status.text = _isPaused ? "Paused" : "Running";
        if (pauseToggle is not null)
            pauseToggle.text = _isPaused ? "Resume" : "Pause";

        advance?.SetEnabled(!_isPaused);
    }

    private void AdvanceMonthNow()
    {
        var shell = RequireShell();
        var events = shell.AdvanceMonth(MonthlySystems);
        RefreshInkBar(shell);
        ApplyMonthlyEvents(events);
    }

    /// <summary>The wax-seal press animation (<c>gens-core-design.md</c> §7.6/§7.7): briefly scales the
    /// seal down, then relaxes it back, so pressing it "visually 'sets'" rather than acting like a
    /// generic button.</summary>
    private static void PressWaxSeal(VisualElement waxSeal)
    {
        waxSeal.AddToClassList("wax-seal--pressed");
        waxSeal.schedule.Execute(() => waxSeal.RemoveFromClassList("wax-seal--pressed")).ExecuteLater(150);
    }

    #endregion

    #region Phase 9 items 7-8 — command submission through confirmation

    private void RequestChangeRitesBudget()
    {
        var shell = RequireShell();
        RequestActionConfirmation(shell, PolicyActionDefinitions.ChangeRitesBudget, () =>
        {
            var current = HouseholdPolicyResolver.GetEffectiveRitesBudget(shell.State, shell.HouseholdId);
            var command = new ChangeRitesBudgetCommand(
                shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
                shell.HouseholdId, NextRitesBudgetTier(current));
            AfterCommandSubmitted(shell.Submit(ChangeRitesBudgetCommands.Pipeline, command));
        });
    }

    private void RequestFundFestival()
    {
        var shell = RequireShell();
        RequestActionConfirmation(shell, PolicyActionDefinitions.FundFestival, () =>
        {
            var command = new FundFestivalCommand(
                shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
                shell.HouseholdId, shell.SettlementId, PolicyActionDefinitions.DefaultFestivalAmount);
            AfterCommandSubmitted(shell.Submit(FundFestivalCommands.Pipeline, command));
        });
    }

    /// <summary>Looks up <paramref name="actionId"/> in the one concrete <see cref="ActionCatalog"/>
    /// this pass wires up, checks eligibility, and — if eligible — shows the wax-seal or ordinary
    /// confirmation dialog (per its own <see cref="ActionDefinition.Confirmation"/>) with <paramref
    /// name="submit"/> as the action a confirm actually runs. Never submits anything itself; a
    /// screen-specific caller (e.g. <see cref="RequestChangeRitesBudget"/>) still builds and submits
    /// the real command.</summary>
    private void RequestActionConfirmation(CampaignShell shell, DefinitionId<ActionDefinition> actionId, Action submit)
    {
        if (!_actionCatalog.TryGet(actionId, out var definition))
            return;

        var invocation = new ActionInvocation(shell.HouseholdId.ToTaggedString(), null, shell.State.Date);
        if (definition.Eligibility(shell.State, invocation) is { } error)
        {
            ShowMessage("Not Available", error.Code);
            return;
        }

        var projection = definition.ProjectResult(shell.State, invocation);
        var viewModel = ActionConfirmationAdapter.Adapt(definition, projection);
        ShowConfirmation(viewModel.TitleLabel, viewModel.BodyLabel, viewModel.IsWaxSeal, onConfirm: submit, onCancel: () => { });
    }

    private void AfterCommandSubmitted(CommandResult result)
    {
        var outcome = new CommandOutcomeAdapter().Adapt(result);
        RefreshInkBar();
        ShowEstateSettlement();
        ShowMessage(
            outcome.Accepted ? "Command Accepted" : "Command Rejected",
            outcome.Accepted ? "The household's records have been updated." : $"Rejected: {outcome.ErrorCode}");
    }

    private static RitesBudgetTier NextRitesBudgetTier(RitesBudgetTier tier) => tier switch
    {
        RitesBudgetTier.Frugal => RitesBudgetTier.Standard,
        RitesBudgetTier.Standard => RitesBudgetTier.Lavish,
        RitesBudgetTier.Lavish => RitesBudgetTier.Frugal,
        _ => RitesBudgetTier.Standard,
    };

    #endregion

    #region Phase 9 item 8 — save/load, deterministic replay diagnostics

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void WireSaveLoadDiagnosticsControls()
    {
        var root = document.rootVisualElement;
        var saveButton = root.Q<Button>("ink-bar-save");
        var loadButton = root.Q<Button>("ink-bar-load");
        var diagnosticsButton = root.Q<Button>("ink-bar-diagnostics");

        if (saveButton is not null)
            saveButton.clicked += SaveNow;
        if (loadButton is not null)
            loadButton.clicked += LoadNow;
        if (diagnosticsButton is not null)
            diagnosticsButton.clicked += RunReplayDiagnostics;
    }

    private void SaveNow()
    {
        var shell = RequireShell();
        shell.Save(SaveFilePath, Application.version);
        ShowMessage("Campaign Saved", $"Saved to {SaveFilePath}.");
    }

    private void LoadNow()
    {
        if (!File.Exists(SaveFilePath))
        {
            ShowMessage("Load Campaign", "No save file exists yet — save the campaign first.");
            return;
        }

        var loadedShell = CampaignShell.Load(SaveFilePath, out var manifest);
        shellBehaviour.ReplaceShell(loadedShell);
        _lastMonthEvents = Array.Empty<IDomainEvent>();
        RefreshInkBar(loadedShell);
        ShowHouseholdRoster();
        ShowMessage("Campaign Loaded", $"Loaded save format {manifest.SaveFormatVersion}, game version {manifest.GameVersion}.");
    }

    private void RunReplayDiagnostics()
    {
        var shell = RequireShell();
        var diagnosticsPath = Path.Combine(Application.temporaryCachePath, "replay-diagnostics.gens");
        var result = shell.VerifyDeterministicReplay(diagnosticsPath, Application.version);
        ShowMessage(
            result.Matches ? "Replay Diagnostics: OK" : "Replay Diagnostics: MISMATCH",
            result.Matches
                ? $"Save/reload reproduced an identical state hash ({result.HashBeforeSave:x16})."
                : $"Hash before save {result.HashBeforeSave:x16} does not match hash after reload {result.HashAfterReload:x16}.");
    }

    #endregion

    #region Confirmation dialog machinery (wax-seal + ordinary, gens-core-design.md §7.4/§7.6)

    private void WireConfirmationDialog()
    {
        var overlay = _confirmationOverlay;
        var cancelButton = overlay.Q<Button>("confirmation-cancel");
        var confirmButton = overlay.Q<Button>("confirmation-confirm");
        var waxSeal = overlay.Q<VisualElement>("confirmation-wax-seal");

        if (cancelButton is not null)
            cancelButton.clicked += () =>
            {
                var onCancel = _pendingCancel;
                HideConfirmation();
                onCancel?.Invoke();
            };

        if (confirmButton is not null)
            confirmButton.clicked += () =>
            {
                var onConfirm = _pendingConfirm;
                HideConfirmation();
                onConfirm?.Invoke();
            };

        if (waxSeal is not null)
            waxSeal.RegisterCallback<ClickEvent>(_ =>
            {
                PressWaxSeal(waxSeal);
                var onConfirm = _pendingConfirm;
                HideConfirmation();
                onConfirm?.Invoke();
            });
    }

    /// <summary>Shows the confirmation overlay. <paramref name="isWaxSeal"/> selects §7.6's circular
    /// seal-press control for a consequential decision versus a plain rectangular Confirm/Cancel pair
    /// for a reversible one (Phase 9 item 7). <paramref name="onCancel"/> of <c>null</c> collapses this
    /// into a single dismiss-only informational message (used by <see cref="ShowMessage"/>).</summary>
    private void ShowConfirmation(string title, string body, bool isWaxSeal, Action onConfirm, Action? onCancel)
    {
        _pendingConfirm = onConfirm;
        _pendingCancel = onCancel;

        var overlay = _confirmationOverlay;
        var titleLabel = overlay.Q<Label>("confirmation-title");
        var bodyLabel = overlay.Q<Label>("confirmation-body");
        var waxSeal = overlay.Q<VisualElement>("confirmation-wax-seal");
        var confirmButton = overlay.Q<Button>("confirmation-confirm");
        var cancelButton = overlay.Q<Button>("confirmation-cancel");

        if (titleLabel is not null)
            titleLabel.text = title;
        if (bodyLabel is not null)
            bodyLabel.text = body;

        if (waxSeal is not null)
            waxSeal.style.display = isWaxSeal ? DisplayStyle.Flex : DisplayStyle.None;

        if (confirmButton is not null)
        {
            confirmButton.style.display = isWaxSeal ? DisplayStyle.None : DisplayStyle.Flex;
            confirmButton.text = onCancel is null ? "OK" : "Confirm";
        }

        if (cancelButton is not null)
            cancelButton.style.display = onCancel is null ? DisplayStyle.None : DisplayStyle.Flex;

        overlay.style.display = DisplayStyle.Flex;
    }

    private void HideConfirmation()
    {
        _confirmationOverlay.style.display = DisplayStyle.None;
        _pendingConfirm = null;
        _pendingCancel = null;
    }

    private void ShowMessage(string title, string body) =>
        ShowConfirmation(title, body, isWaxSeal: false, onConfirm: () => { }, onCancel: null);

    #endregion
}
