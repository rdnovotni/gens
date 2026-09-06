#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gens.Presentation.Shell;

/// <summary>The application's single entry point above the campaign: Main Menu → New Game setup or
/// Load Game → the running campaign (<see cref="CampaignShellBehaviour"/>/<see
/// cref="GensUIController"/>, Phase 9's own shell pair). Owns a menu-only <see cref="UIDocument"/>
/// separate from the gameplay one so the two never fight over the same visual tree; hides its own menu
/// once gameplay starts rather than tearing it down, since a future "quit to menu" control can show it
/// again without re-loading UXML.</summary>
public sealed class GensAppController : MonoBehaviour
{
    [SerializeField]
    private UIDocument menuDocument = null!;

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

    /// <summary>The one region this content pack actually authors today (<c>content/source/regions</c>
    /// carries only the "latium" sample) — a display-name lookup, not a hardcoded gameplay assumption;
    /// grows the moment Phase 13's real region content lands, with no code change needed beyond this
    /// list.</summary>
    private static readonly (string DisplayName, string RegionId)[] AvailableRegions =
    {
        ("Latium", "latium"),
    };

    /// <summary>The only two difficulty strings this codebase has ever written anywhere (<see
    /// cref="CampaignShellBehaviour"/>'s own default and the golden-path test's choice) — no numeric
    /// difficulty system exists yet for this to select between.</summary>
    private static readonly (string DisplayName, string Difficulty)[] AvailableDifficulties =
    {
        ("Normal", "normal"),
        ("Standard", "standard"),
    };

    private void Awake()
    {
        gameplayRoot.SetActive(false);
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        var root = Mount(mainMenuAsset);

        var newGameButton = root.Q<Button>("main-menu-new-game");
        var loadGameButton = root.Q<Button>("main-menu-load-game");
        var quitButton = root.Q<Button>("main-menu-quit");
        var status = root.Q<Label>("main-menu-status");

        if (newGameButton is not null)
            newGameButton.clicked += ShowNewGameSetup;

        var hasSave = GensUIController.HasSaveFile();
        if (loadGameButton is not null)
        {
            loadGameButton.SetEnabled(hasSave);
            loadGameButton.clicked += LoadExistingCampaign;
        }

        if (status is not null)
            status.text = hasSave ? string.Empty : "No saved campaign yet.";

        if (quitButton is not null)
            quitButton.clicked += QuitApplication;
    }

    private void ShowNewGameSetup()
    {
        var root = Mount(newGameSetupAsset);

        var regionField = root.Q<DropdownField>("new-game-region");
        var difficultyField = root.Q<DropdownField>("new-game-difficulty");
        var seedField = root.Q<IntegerField>("new-game-seed");
        var randomSeedButton = root.Q<Button>("new-game-random-seed");
        var backButton = root.Q<Button>("new-game-back");
        var beginButton = root.Q<Button>("new-game-begin");

        if (regionField is not null)
        {
            regionField.choices = new List<string>(Array.ConvertAll(AvailableRegions, region => region.DisplayName));
            regionField.index = 0;
        }

        if (difficultyField is not null)
        {
            difficultyField.choices = new List<string>(Array.ConvertAll(AvailableDifficulties, difficulty => difficulty.DisplayName));
            difficultyField.index = 0;
        }

        if (seedField is not null)
            seedField.value = new System.Random().Next();

        if (randomSeedButton is not null && seedField is not null)
            randomSeedButton.clicked += () => seedField.value = new System.Random().Next();

        if (backButton is not null)
            backButton.clicked += ShowMainMenu;

        if (beginButton is not null)
            beginButton.clicked += () => BeginNewCampaign(regionField, difficultyField, seedField);
    }

    private void BeginNewCampaign(DropdownField? regionField, DropdownField? difficultyField, IntegerField? seedField)
    {
        var regionId = AvailableRegions[Math.Max(regionField?.index ?? 0, 0)].RegionId;
        var difficulty = AvailableDifficulties[Math.Max(difficultyField?.index ?? 0, 0)].Difficulty;
        var seed = (ulong)Math.Max(seedField?.value ?? 1, 1);

        shellBehaviour.Configure(
            seed: seed,
            startMonths: 0,
            rulesetId: "default",
            contentPackHash: string.Empty,
            regionId: regionId,
            startProfileId: null,
            difficulty: difficulty);

        StartGameplay();
    }

    private void LoadExistingCampaign()
    {
        StartGameplay();
        StartCoroutine(LoadAfterGameplayStarts());
    }

    /// <summary>The gameplay root's <see cref="CampaignShellBehaviour"/> always bootstraps a fresh
    /// campaign in <c>Awake</c> (Phase 9 item 5's contract, unchanged so GoldenPathPlayModeTests keeps
    /// working) and <see cref="GensUIController"/> mounts its screens in its own <c>Start</c>, which
    /// Unity does not run until the frame after <see cref="StartGameplay"/> activates the GameObject
    /// (the exact ordering <c>GoldenPathPlayModeTests</c> itself waits one frame for) — so this waits
    /// one frame before replacing that placeholder campaign with the saved one, the same order
    /// <see cref="GensUIController"/>'s own in-game Load control already uses.</summary>
    private IEnumerator LoadAfterGameplayStarts()
    {
        yield return null;
        uiController.LoadSavedCampaign();
    }

    private void StartGameplay()
    {
        menuDocument.rootVisualElement.Clear();
        gameplayRoot.SetActive(true);
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private VisualElement Mount(VisualTreeAsset asset)
    {
        var root = menuDocument.rootVisualElement;
        root.Clear();
        root.style.flexGrow = 1;
        var instance = asset.CloneTree();
        root.Add(instance);
        return root;
    }
}
