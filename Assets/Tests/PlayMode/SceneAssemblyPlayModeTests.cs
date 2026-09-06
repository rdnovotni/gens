#nullable enable

using System.Collections;
using System.IO;
using Gens.Presentation.Shell;
using Gens.Presentation.Tests.Support;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Gens.Presentation.Tests.Support.UiTestEvents;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Gens.Presentation.Tests.PlayMode;

public sealed class SceneAssemblyPlayModeTests
{
    private string? _savePath;

    [SetUp]
    public void SetUp()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "quicksave.gens");
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (_savePath is not null && File.Exists(_savePath))
            File.Delete(_savePath);
    }

    [UnityTest]
    public IEnumerator MainScene_NewGame_NavigatesScreens_Saves_And_Reloads()
    {
        // 1. Load Main Scene
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("Assets/Scenes/Main.unity");
        yield return loadOp;
        yield return null;

        var appObj = GameObject.Find("App");
        Assert.That(appObj, Is.Not.Null, "App GameObject should exist in Main scene.");
        var appDoc = appObj.GetComponent<UIDocument>();
        var appRoot = appDoc.rootVisualElement;

        var newGameBtn = appRoot.Q<Button>("main-menu-new-game");
        var loadGameBtn = appRoot.Q<Button>("main-menu-load-game");
        Assert.That(newGameBtn, Is.Not.Null, "New Game button should exist on Main Menu.");
        Assert.That(loadGameBtn, Is.Not.Null, "Load Game button should exist on Main Menu.");
        Assert.That(loadGameBtn.enabledSelf, Is.False, "Load Game should be disabled when no save exists.");

        // 2. Click New Game -> New Game Setup screen
        SimulateClick(newGameBtn);
        yield return null;

        var beginBtn = appRoot.Q<Button>("setup-begin-button");
        Assert.That(beginBtn, Is.Not.Null, "Begin button should exist on New Game Setup screen.");

        // 3. Click Begin -> Gameplay activates
        SimulateClick(beginBtn);
        yield return null;

        var gameplayObj = GameObject.Find("Gameplay");
        Assert.That(gameplayObj, Is.Not.Null, "Gameplay GameObject should exist.");
        Assert.That(gameplayObj.activeSelf, Is.True, "Gameplay should be active after Begin Campaign.");

        var gameplayDoc = gameplayObj.GetComponent<UIDocument>();
        var gameplayRoot = gameplayDoc.rootVisualElement;

        var householdNav = gameplayRoot.Q<Button>("ink-bar-nav-household");
        var estateNav = gameplayRoot.Q<Button>("ink-bar-nav-estate");
        var reportNav = gameplayRoot.Q<Button>("ink-bar-nav-report");
        Assert.That(householdNav, Is.Not.Null, "Household nav button should exist in Ink Bar.");
        Assert.That(estateNav, Is.Not.Null, "Estate nav button should exist in Ink Bar.");
        Assert.That(reportNav, Is.Not.Null, "Report nav button should exist in Ink Bar.");

        Assert.That(gameplayRoot.Q<VisualElement>("household-roster-screen"), Is.Not.Null, "Household roster screen should be visible initially.");
        Assert.That(householdNav.ClassListContains("ink-bar__nav-button--active"), Is.True, "Household nav button should have active class.");

        // 4. Test Navigation to Estate
        SimulateClick(estateNav);
        yield return null;
        Assert.That(gameplayRoot.Q<VisualElement>("estate-settlement-screen"), Is.Not.Null, "Estate settlement screen should be visible.");
        Assert.That(estateNav.ClassListContains("ink-bar__nav-button--active"), Is.True, "Estate nav button should have active class.");
        Assert.That(householdNav.ClassListContains("ink-bar__nav-button--active"), Is.False, "Household nav button should not have active class.");

        // 5. Test Navigation to Report
        SimulateClick(reportNav);
        yield return null;
        Assert.That(gameplayRoot.Q<VisualElement>("monthly-report-screen"), Is.Not.Null, "Monthly report screen should be visible.");
        Assert.That(reportNav.ClassListContains("ink-bar__nav-button--active"), Is.True, "Report nav button should have active class.");

        // 6. Test Navigation back to Household
        SimulateClick(householdNav);
        yield return null;
        Assert.That(gameplayRoot.Q<VisualElement>("household-roster-screen"), Is.Not.Null, "Household roster screen should be visible.");
        Assert.That(householdNav.ClassListContains("ink-bar__nav-button--active"), Is.True, "Household nav button should have active class.");

        // 7. Test Save
        var saveBtn = gameplayRoot.Q<Button>("ink-bar-save");
        SimulateClick(saveBtn);
        yield return null;
        var confirmBtn = gameplayRoot.Q<Button>("confirmation-confirm");
        SimulateClick(confirmBtn);
        yield return null;

        Assert.That(File.Exists(_savePath), Is.True, "Save file should exist after saving.");

        // 8. Reload Scene to test Load Game from Main Menu
        AsyncOperation reloadOp = SceneManager.LoadSceneAsync("Assets/Scenes/Main.unity");
        yield return reloadOp;
        yield return null;

        var reloadedAppObj = GameObject.Find("App");
        var reloadedAppDoc = reloadedAppObj.GetComponent<UIDocument>();
        var reloadedAppRoot = reloadedAppDoc.rootVisualElement;

        var reloadedLoadBtn = reloadedAppRoot.Q<Button>("main-menu-load-game");
        Assert.That(reloadedLoadBtn.enabledSelf, Is.True, "Load Game should be enabled when a save file exists.");

        SimulateClick(reloadedLoadBtn);
        yield return null;

        var reloadedGameplayObj = GameObject.Find("Gameplay");
        Assert.That(reloadedGameplayObj.activeSelf, Is.True, "Gameplay should be active after Load Game.");
        var reloadedGameplayRoot = reloadedGameplayObj.GetComponent<UIDocument>().rootVisualElement;
        Assert.That(reloadedGameplayRoot.Q<VisualElement>("household-roster-screen"), Is.Not.Null, "Household roster should be mounted after Load Game.");
    }
}
