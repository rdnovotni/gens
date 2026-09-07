#nullable enable

using System;
using System.IO;
using Gens.Presentation.Shell.DevConsole;
using Gens.Simulation.Campaign;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gens.Presentation.Shell;

/// <summary>
/// Root controller for the application flow: Main Menu -> New Game setup -> Gameplay.
/// Owns the top-level App UIDocument and controls activation of the gameplay hierarchy.
/// </summary>
public sealed class GensAppController : MonoBehaviour
{
    [SerializeField]
    private UIDocument document = null!;

    [SerializeField]
    private VisualTreeAsset mainMenuAsset = null!;

    [SerializeField]
    private VisualTreeAsset newGameSetupAsset = null!;

    [SerializeField]
    private VisualTreeAsset settingsAsset = null!;

    [SerializeField]
    private VisualTreeAsset creditsAsset = null!;

    [SerializeField]
    private GameObject gameplayRoot = null!;

    [SerializeField]
    private CampaignShellBehaviour shellBehaviour = null!;

    [SerializeField]
    private GensUIController uiController = null!;

    private const string SaveFileName = "quicksave.gens";
    private const string MasterVolumePrefKey = "MasterVolume";
    private static string SaveFilePath => Path.Combine(UnityEngine.Application.persistentDataPath, SaveFileName);

    private string _selectedRegion = "latium";
    private string _selectedDifficulty = "standard";

    /// <summary>True once gameplay has been entered (New Campaign or Load Campaign) at least once this
    /// session. Unity only ever calls <see cref="CampaignShellBehaviour.Awake"/>/<see
    /// cref="GensUIController.Start"/> on the very first activation, so every entry after the first
    /// (reachable via <see cref="ReturnToMainMenu"/>) must re-bootstrap/re-mount explicitly instead.</summary>
    private bool _gameplayEnteredOnce;

    private void Start()
    {
        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        if (uiController != null)
            uiController.OnReturnToMainMenuConfirmed = ReturnToMainMenu;

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        var root = document.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;

        var menuInstance = mainMenuAsset.CloneTree();
        menuInstance.style.flexGrow = 1;
        root.Add(menuInstance);

        // The card starts at opacity:0/scale:0.96 in MainMenuScreen.uss so this class add (next frame,
        // after the initial style has actually applied) triggers the USS transition into view rather
        // than the card just appearing instantly.
        var menuCard = root.Q<VisualElement>(className: "main-menu-card");
        menuCard?.schedule.Execute(() => menuCard.AddToClassList("main-menu-card--visible"));

        var newGameBtn = root.Q<Button>("main-menu-new-game");
        var loadGameBtn = root.Q<Button>("main-menu-load-game");
        var settingsBtn = root.Q<Button>("main-menu-settings");
        var creditsBtn = root.Q<Button>("main-menu-credits");
        var quitBtn = root.Q<Button>("main-menu-quit");

        bool hasSave = File.Exists(SaveFilePath);
        if (loadGameBtn != null)
        {
            loadGameBtn.SetEnabled(hasSave);
            loadGameBtn.clicked += LoadGame;
        }

        if (newGameBtn != null)
            newGameBtn.clicked += ShowNewGameSetup;

        if (settingsBtn != null)
            settingsBtn.clicked += ShowSettings;

        if (creditsBtn != null)
            creditsBtn.clicked += ShowCredits;

        if (quitBtn != null)
            quitBtn.clicked += QuitApp;
    }

    public void ShowSettings()
    {
        var root = document.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;

        var settingsInstance = settingsAsset.CloneTree();
        settingsInstance.style.flexGrow = 1;
        root.Add(settingsInstance);

        var volumeSlider = root.Q<Slider>("settings-master-volume");
        if (volumeSlider != null)
        {
            var savedVolume = PlayerPrefs.GetFloat(MasterVolumePrefKey, 1f);
            UnityEngine.AudioListener.volume = savedVolume;
            volumeSlider.value = savedVolume;
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                UnityEngine.AudioListener.volume = evt.newValue;
                PlayerPrefs.SetFloat(MasterVolumePrefKey, evt.newValue);
            });
        }

        var devConsoleToggle = root.Q<Toggle>("settings-dev-console-toggle");
        if (devConsoleToggle != null)
        {
            devConsoleToggle.value = PlayerPrefs.GetInt(DevConsoleController.EnabledPrefKey, 0) != 0;
            devConsoleToggle.RegisterValueChangedCallback(evt =>
                PlayerPrefs.SetInt(DevConsoleController.EnabledPrefKey, evt.newValue ? 1 : 0));
        }

        var backBtn = root.Q<Button>("settings-back-button");
        if (backBtn != null)
            backBtn.clicked += ShowMainMenu;
    }

    public void ShowCredits()
    {
        var root = document.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;

        var creditsInstance = creditsAsset.CloneTree();
        creditsInstance.style.flexGrow = 1;
        root.Add(creditsInstance);

        var backBtn = root.Q<Button>("credits-back-button");
        if (backBtn != null)
            backBtn.clicked += ShowMainMenu;
    }

    /// <summary>Deactivates gameplay and returns to the Main Menu, invoked once the ink bar's "Main
    /// Menu" confirmation (<see cref="GensUIController"/>) is accepted. The campaign itself is left
    /// running in memory (a player who instead chooses New Campaign or Load Campaign next re-bootstraps
    /// or replaces it explicitly — see <see cref="BeginCampaign"/>/<see cref="LoadGame"/>).</summary>
    public void ReturnToMainMenu()
    {
        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        document.rootVisualElement.style.display = DisplayStyle.Flex;
        ShowMainMenu();
    }

    public void ShowNewGameSetup()
    {
        var root = document.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;

        var setupInstance = newGameSetupAsset.CloneTree();
        setupInstance.style.flexGrow = 1;
        root.Add(setupInstance);

        _selectedRegion = "latium";
        _selectedDifficulty = "standard";

        var regionLatium = root.Q<Button>("region-latium");
        var regionCampania = root.Q<Button>("region-campania");
        var regionCisalpina = root.Q<Button>("region-cisalpina");

        void UpdateRegionSelection(string region)
        {
            _selectedRegion = region;
            const string selectedClass = "setup-option-button--selected";
            regionLatium?.RemoveFromClassList(selectedClass);
            regionCampania?.RemoveFromClassList(selectedClass);
            regionCisalpina?.RemoveFromClassList(selectedClass);

            if (region == "latium") regionLatium?.AddToClassList(selectedClass);
            else if (region == "campania") regionCampania?.AddToClassList(selectedClass);
            else if (region == "cisalpina") regionCisalpina?.AddToClassList(selectedClass);
        }

        if (regionLatium != null) regionLatium.clicked += () => UpdateRegionSelection("latium");
        if (regionCampania != null) regionCampania.clicked += () => UpdateRegionSelection("campania");
        if (regionCisalpina != null) regionCisalpina.clicked += () => UpdateRegionSelection("cisalpina");

        var diffStandard = root.Q<Button>("difficulty-standard");
        var diffHard = root.Q<Button>("difficulty-hard");
        var diffRelaxed = root.Q<Button>("difficulty-relaxed");

        void UpdateDifficultySelection(string diff)
        {
            _selectedDifficulty = diff;
            const string selectedClass = "setup-option-button--selected";
            diffStandard?.RemoveFromClassList(selectedClass);
            diffHard?.RemoveFromClassList(selectedClass);
            diffRelaxed?.RemoveFromClassList(selectedClass);

            if (diff == "standard") diffStandard?.AddToClassList(selectedClass);
            else if (diff == "hard") diffHard?.AddToClassList(selectedClass);
            else if (diff == "relaxed") diffRelaxed?.AddToClassList(selectedClass);
        }

        if (diffStandard != null) diffStandard.clicked += () => UpdateDifficultySelection("standard");
        if (diffHard != null) diffHard.clicked += () => UpdateDifficultySelection("hard");
        if (diffRelaxed != null) diffRelaxed.clicked += () => UpdateDifficultySelection("relaxed");

        var beginBtn = root.Q<Button>("setup-begin-button");
        var backBtn = root.Q<Button>("setup-back-button");

        if (beginBtn != null)
            beginBtn.clicked += () => BeginCampaign(_selectedRegion, _selectedDifficulty);

        if (backBtn != null)
            backBtn.clicked += ShowMainMenu;
    }

    public void BeginCampaign(string region, string difficulty)
    {
        document.rootVisualElement.Clear();
        document.rootVisualElement.style.display = DisplayStyle.None;

        if (shellBehaviour != null)
        {
            shellBehaviour.Configure(seed: 1, regionId: region, difficulty: difficulty);
            if (_gameplayEnteredOnce)
                shellBehaviour.Bootstrap();
        }

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);

        if (_gameplayEnteredOnce)
            uiController.Initialize();

        _gameplayEnteredOnce = true;
    }

    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
            return;

        document.rootVisualElement.Clear();
        document.rootVisualElement.style.display = DisplayStyle.None;

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);

        var loadedShell = CampaignShell.Load(SaveFilePath, out _);
        shellBehaviour.ReplaceShell(loadedShell);

        // Initialize() (re)mounts the ink bar/screen host synchronously against the just-replaced shell
        // rather than relying on Unity's own GensUIController.Start(), which SetActive(true) above only
        // schedules for next frame — calling ShowHouseholdRoster() before that had run would hit a null
        // screen host on this session's very first Load.
        uiController.Initialize();

        _gameplayEnteredOnce = true;
    }

    private static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }
}
