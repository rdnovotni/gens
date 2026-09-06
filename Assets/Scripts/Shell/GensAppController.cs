#nullable enable

using System;
using System.IO;
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
    private GameObject gameplayRoot = null!;

    [SerializeField]
    private CampaignShellBehaviour shellBehaviour = null!;

    [SerializeField]
    private GensUIController uiController = null!;

    private const string SaveFileName = "quicksave.gens";
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private string _selectedRegion = "latium";
    private string _selectedDifficulty = "standard";

    private void Start()
    {
        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

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

        var newGameBtn = root.Q<Button>("main-menu-new-game");
        var loadGameBtn = root.Q<Button>("main-menu-load-game");
        var quitBtn = root.Q<Button>("main-menu-quit");

        bool hasSave = File.Exists(SaveFilePath);
        if (loadGameBtn != null)
        {
            loadGameBtn.SetEnabled(hasSave);
            loadGameBtn.clicked += LoadGame;
        }

        if (newGameBtn != null)
            newGameBtn.clicked += ShowNewGameSetup;

        if (quitBtn != null)
            quitBtn.clicked += QuitApp;
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
            shellBehaviour.Configure(seed: 1, regionId: region, difficulty: difficulty);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);
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
        uiController.RefreshInkBar();
        uiController.ShowHouseholdRoster();
    }

    private static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
