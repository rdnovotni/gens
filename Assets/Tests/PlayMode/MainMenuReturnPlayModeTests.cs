#nullable enable

using System.Collections;
using System.IO;
using Gens.Presentation.Tests.Support;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using static Gens.Presentation.Tests.Support.UiTestEvents;

namespace Gens.Presentation.Tests.PlayMode;

/// <summary>Covers the Main Menu flesh-out gap named in <c>docs/engineering/unity-scene-assembly-handoff.md</c>
/// ("return to the Main Menu conceptually is not wired yet") plus the new Settings/Credits screens,
/// through the real <c>Assets/Scenes/Main.unity</c> scene, mirroring <see cref="SceneAssemblyPlayModeTests"/>.
/// </summary>
public sealed class MainMenuReturnPlayModeTests
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
    public IEnumerator MainScene_SettingsAndCredits_ReturnToMainMenu_FromMainMenu()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("Assets/Scenes/Main.unity");
        yield return loadOp;
        yield return null;

        var appObj = GameObject.Find("App");
        var appRoot = appObj.GetComponent<UIDocument>().rootVisualElement;

        // Settings: reachable and returns to the Main Menu.
        var settingsBtn = appRoot.Q<Button>("main-menu-settings");
        Assert.That(settingsBtn, Is.Not.Null, "Settings button should exist on Main Menu.");
        SimulateClick(settingsBtn);
        yield return null;

        var volumeSlider = appRoot.Q<Slider>("settings-master-volume");
        Assert.That(volumeSlider, Is.Not.Null, "Master volume slider should exist on Settings screen.");
        var settingsBackBtn = appRoot.Q<Button>("settings-back-button");
        SimulateClick(settingsBackBtn);
        yield return null;
        Assert.That(appRoot.Q<Button>("main-menu-new-game"), Is.Not.Null, "Settings Back should return to Main Menu.");

        // Credits: reachable and returns to the Main Menu.
        var creditsBtn = appRoot.Q<Button>("main-menu-credits");
        Assert.That(creditsBtn, Is.Not.Null, "Credits button should exist on Main Menu.");
        SimulateClick(creditsBtn);
        yield return null;

        var creditsBackBtn = appRoot.Q<Button>("credits-back-button");
        Assert.That(creditsBackBtn, Is.Not.Null, "Back button should exist on Credits screen.");
        SimulateClick(creditsBackBtn);
        yield return null;
        Assert.That(appRoot.Q<Button>("main-menu-new-game"), Is.Not.Null, "Credits Back should return to Main Menu.");
    }

    [UnityTest]
    public IEnumerator MainScene_ReturnToMainMenu_FromGameplay_AllowsSecondCampaign()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync("Assets/Scenes/Main.unity");
        yield return loadOp;
        yield return null;

        var appObj = GameObject.Find("App");
        var appRoot = appObj.GetComponent<UIDocument>().rootVisualElement;

        // First campaign.
        SimulateClick(appRoot.Q<Button>("main-menu-new-game"));
        yield return null;
        SimulateClick(appRoot.Q<Button>("setup-begin-button"));
        yield return null;

        var gameplayObj = GameObject.Find("Gameplay");
        Assert.That(gameplayObj.activeSelf, Is.True, "Gameplay should be active after Begin Campaign.");
        var gameplayRoot = gameplayObj.GetComponent<UIDocument>().rootVisualElement;

        // Return to Main Menu: click the ink bar control, confirm the dialog.
        var mainMenuBtn = gameplayRoot.Q<Button>("ink-bar-main-menu");
        Assert.That(mainMenuBtn, Is.Not.Null, "Main Menu button should exist in the ink bar.");
        SimulateClick(mainMenuBtn);
        yield return null;

        var confirmBtn = gameplayRoot.Q<Button>("confirmation-confirm");
        Assert.That(confirmBtn, Is.Not.Null, "Return-to-menu confirmation should be showing.");
        SimulateClick(confirmBtn);
        yield return null;

        Assert.That(gameplayObj.activeSelf, Is.False, "Gameplay should be deactivated after returning to the Main Menu.");
        Assert.That(appRoot.Q<Button>("main-menu-new-game"), Is.Not.Null, "Main Menu should be showing again.");

        // Second campaign in the same session must re-bootstrap and re-mount cleanly (Awake/Start only
        // ever fire once per object; GensAppController must drive the second entry explicitly).
        SimulateClick(appRoot.Q<Button>("main-menu-new-game"));
        yield return null;
        SimulateClick(appRoot.Q<Button>("setup-begin-button"));
        yield return null;

        Assert.That(gameplayObj.activeSelf, Is.True, "Gameplay should be active again after a second Begin Campaign.");
        var reenteredGameplayRoot = gameplayObj.GetComponent<UIDocument>().rootVisualElement;
        Assert.That(
            reenteredGameplayRoot.Q<VisualElement>("household-roster-screen"), Is.Not.Null,
            "Household roster screen should be mounted again after a second campaign entry.");
    }
}
