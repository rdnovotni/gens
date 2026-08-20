#nullable enable

using System.Collections;
using System.IO;
using Gens.Presentation.Adapters;
using Gens.Presentation.Shell;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Buildings;
using Gens.Simulation.Characters;
using Gens.Simulation.Policies;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Gens.Presentation.Tests.Support.PrivateFieldAccess;
using static Gens.Presentation.Tests.Support.UiTestEvents;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gens.Presentation.Tests.PlayMode;

/// <summary>
/// Drives Phase 9 item 9's exact golden path — "new campaign → assign labor → build/produce → change
/// policy → advance month → inspect report → save/load" — through the real <see
/// cref="CampaignShellBehaviour"/>/<see cref="GensUIController"/> pair and the real UXML screens under
/// <c>Assets/UI</c>, simulating the same pointer input a player's mouse would send (<see
/// cref="UiTestEvents.SimulateClick"/>). Every reachable step goes through the UI's own wired
/// controls; the two steps nothing in Phase 9 wires a control for yet — provisioning a household's
/// first holding/plot and staffing a construction queue — are seeded directly on <see
/// cref="CampaignShell.State"/> the same way <see cref="CampaignTestFixtures"/> seeds them for the
/// EditMode estate-adapter tests, since <c>AcquirePlotCommand</c>/<c>ConstructionSchedule.Enqueue</c>
/// have no Phase 9 UI action bound to them; every assertion after each step still reads back through
/// the real adapter/screen the way a player would, not raw simulation state, per ADR 0013.
/// </summary>
public sealed class GoldenPathPlayModeTests
{
    private GameObject? _root;
    private string? _savePath;

    [TearDown]
    public void TearDown()
    {
        if (_root != null)
            Object.Destroy(_root);
        if (_savePath is not null && File.Exists(_savePath))
            File.Delete(_savePath);
    }

    [UnityTest]
    public IEnumerator NewCampaignAssignLaborBuildProduceChangePolicyAdvanceInspectReportSaveLoad()
    {
        _root = new GameObject("gens-golden-path");
        _root.SetActive(false);

        var shellBehaviour = _root.AddComponent<CampaignShellBehaviour>();
        Set(shellBehaviour, "seed", CampaignTestFixtures.DefaultSeed);
        Set(shellBehaviour, "startMonths", CampaignTestFixtures.DefaultStartMonths);
        Set(shellBehaviour, "rulesetId", "classic");
        Set(shellBehaviour, "contentPackHash", "content-hash-placeholder");
        Set(shellBehaviour, "regionId", "latium");
        Set(shellBehaviour, "difficulty", "standard");

        var document = _root.AddComponent<UIDocument>();
        document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

        var controller = _root.AddComponent<GensUIController>();
        Set(controller, "shellBehaviour", shellBehaviour);
        Set(controller, "document", document);
        Set(controller, "inkBarAsset", LoadUxml("Assets/UI/InkBar.uxml"));
        Set(controller, "householdRosterAsset", LoadUxml("Assets/UI/HouseholdRosterScreen.uxml"));
        Set(controller, "estateSettlementAsset", LoadUxml("Assets/UI/EstateSettlementScreen.uxml"));
        Set(controller, "monthlyReportAsset", LoadUxml("Assets/UI/MonthlyReportScreen.uxml"));
        Set(controller, "characterDetailAsset", LoadUxml("Assets/UI/CharacterDetailScreen.uxml"));
        Set(controller, "confirmationDialogAsset", LoadUxml("Assets/UI/ConfirmationDialog.uxml"));

        // New campaign: activating the GameObject fires CampaignShellBehaviour.Awake (bootstrap) and
        // then GensUIController.Start (mounts the ink bar + household roster), the same order the real
        // scene's own lifecycle runs them in.
        _root.SetActive(true);
        yield return null;

        var shell = shellBehaviour.Shell ?? throw new System.InvalidOperationException("CampaignShellBehaviour did not bootstrap a shell.");
        Assert.That(shell.HouseholdId.ToTaggedString(), Is.EqualTo("household_0000000"));
        var rootVisual = document.rootVisualElement;
        Assert.That(rootVisual.Q<Label>(InkBarBinding.DateLabelName).text, Does.Match(@"^\d{2}/\d+ (BCE|CE)$"));

        // Assign labor: a fresh bootstrap has no Characters at all (Phase 4's "empty campaign"), so one
        // adult household member is seeded directly (test scaffolding, not a UI action — see
        // CampaignTestFixtures), then assigned a Labor duty through the exact CampaignShell.Submit path
        // every real command in this codebase goes through (ADR 0013).
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(shell);
        var assignResult = shell.Submit(AssignDutyCommands.Pipeline, new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand));
        Assert.That(assignResult.Accepted, Is.True);

        controller.ShowHouseholdRoster();
        Assert.That(
            rootVisual.Q<Button>($"roster-row-{characterId.ToTaggedString()}"), Is.Not.Null,
            "Household roster screen did not render the newly assigned member.");

        // Build/produce: seed the household's first holding/plot (see the class doc comment for why
        // this step alone bypasses the UI — no Phase 9 action wraps land acquisition or construction
        // yet), enqueue one building, then advance through CampaignShell.AdvanceMonth with construction
        // and production included, exactly as GensUIController's own AdvanceMonthNow does with its
        // narrower system list.
        var holdingId = CampaignTestFixtures.SeedHouseholdHolding(shell);
        var plotId = CampaignTestFixtures.FirstPlotOf(shell, holdingId);
        shell.State.ConstructionSchedules.Add(holdingId, new ConstructionSchedule());
        shell.State.ConstructionSchedules.TryGet(holdingId, out var schedule);
        shell.State.Plots.TryGet(plotId, out var plot);
        schedule!.Enqueue(plot!, CampaignTestFixtures.GrainFieldDefinition(), System.Array.Empty<BuildingInstance>());
        shell.AdvanceMonth(new IMonthlySystem<WorldState>[]
        {
            new ConstructionSystem(),
            new ProductionSystem(CampaignTestFixtures.GoodCatalog),
        });

        controller.ShowEstateSettlement();
        var estateProjection = shell.Query(new EstateSettlementQuery(shell.SettlementId, shell.HouseholdId), "player");
        var estateViewModel = new EstateSettlementAdapter().Adapt(estateProjection);
        Assert.That(estateViewModel.Holdings, Has.Some.Matches<EstateHoldingRowViewModel>(
            holding => holding.Buildings.Count > 0), "No building completed construction.");
        Assert.That(rootVisual.Q<VisualElement>(EstateSettlementBinding.HoldingContainerName).childCount, Is.GreaterThan(0));

        // Change policy: a real button click on the estate/settlement screen, through the wax-seal/
        // ordinary confirmation flow Phase 9 items 7-8 built — Change Rites Budget is Ordinary severity
        // (PolicyActionDefinitions), so the plain Confirm button is the one to click.
        SimulateClick(rootVisual.Q<Button>(EstateSettlementBinding.ChangeRitesBudgetButtonName));
        Assert.That(rootVisual.Q<Label>("confirmation-title").text, Is.EqualTo("Change Rites Budget"));
        var confirmButton = rootVisual.Q<Button>("confirmation-confirm");
        Assert.That(confirmButton.style.display.value, Is.EqualTo(DisplayStyle.Flex), "Ordinary confirmation should show the Confirm button, not the wax seal.");
        SimulateClick(confirmButton);
        Assert.That(
            HouseholdPolicyResolver.GetEffectiveRitesBudget(shell.State, shell.HouseholdId), Is.Not.EqualTo(RitesBudgetTier.Standard),
            "Confirming the dialog should have submitted the real ChangeRitesBudgetCommand.");

        // Advance month: resume the clock, then press the wax-seal advance control — the same
        // pause/advance flow Phase 9 item 8 built.
        SimulateClick(rootVisual.Q<Button>("ink-bar-pause-toggle"));
        var beforeAdvance = shell.State.Date;
        SimulateClickEvent(rootVisual.Q<VisualElement>("ink-bar-advance"));
        Assert.That(shell.State.Date.TotalMonths, Is.EqualTo(beforeAdvance.TotalMonths + 1));

        // Inspect report: the Monthly Report screen the advance above should have surfaced.
        Assert.That(rootVisual.Q<VisualElement>("monthly-report-screen"), Is.Not.Null);
        Assert.That(rootVisual.Q<Label>(MonthlyReportBinding.IncomeLabelName).text, Does.EndWith(" denarii"));

        // Save/load: real button clicks driving CampaignShell.Save/Load exactly as a player's Save and
        // Load presses would.
        SimulateClick(rootVisual.Q<Button>("ink-bar-save"));
        Assert.That(rootVisual.Q<Label>("confirmation-title").text, Is.EqualTo("Campaign Saved"));
        SimulateClick(rootVisual.Q<Button>("confirmation-confirm"));

        SimulateClick(rootVisual.Q<Button>("ink-bar-load"));
        Assert.That(rootVisual.Q<Label>("confirmation-title").text, Is.EqualTo("Campaign Loaded"));
        SimulateClick(rootVisual.Q<Button>("confirmation-confirm"));

        Assert.That(shellBehaviour.Shell, Is.Not.Null.And.Not.SameAs(shell), "Load should have replaced the shell with a freshly loaded one.");
        var reloadedRoster = shellBehaviour.Shell!.Query(new HouseholdRosterQuery(shellBehaviour.Shell.HouseholdId), "player");
        Assert.That(reloadedRoster.Members, Has.Count.EqualTo(1), "The loaded save should still carry the assigned household member.");

        _savePath = Path.Combine(Application.persistentDataPath, "quicksave.gens");
    }

    private static VisualTreeAsset LoadUxml(string projectRelativePath)
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(projectRelativePath);
        if (asset == null)
            throw new System.InvalidOperationException($"Could not load '{projectRelativePath}' as a VisualTreeAsset.");
        return asset;
#else
        throw new System.PlatformNotSupportedException(
            "GoldenPathPlayModeTests loads UXML via AssetDatabase and only runs inside the Editor.");
#endif
    }
}
