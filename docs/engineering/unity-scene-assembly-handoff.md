# Unity scene assembly — handoff to a live Editor session

*Written from a sandbox with no Unity Editor or `dotnet` available, immediately after confirming the
C# langversion compiler fixes (PRs #103–#106) are green on `main`. This project has never been opened
in the Unity Editor: there are zero `.unity` scene files anywhere in the repo, zero `.meta` files in
git history, and no `ProjectSettings/EditorSettings.asset`/`EditorBuildSettings.asset` — all three are
auto-created the first time a project opens. Everything under `Assets/Scripts` and `Assets/UI` has
only ever been exercised through EditMode/PlayMode tests instantiating objects directly, never through
an actual playable scene. The steps below need a real Editor session (GUID assignment, Inspector
drag-and-drop) that this handoff cannot do from a text-only sandbox.*

## 1. Commit `.meta` files (blocking, do this first)

1. Open the project in the Unity Editor.
2. `Edit → Project Settings → Editor → Version Control → Mode` → **Visible Meta Files** (if not
   already the default in this Unity version).
3. Let the Editor finish importing/generating `.meta` files for every asset.
4. Commit `ProjectSettings/*` (now including `EditorSettings.asset`/`EditorBuildSettings.asset`) and
   every generated `*.meta` file. Without this, every fresh clone or CI checkout invents new GUIDs, and
   any scene referencing today's GUIDs breaks for everyone else.

## 2. Assemble the scene

One scene (e.g. `Assets/Scenes/Main.unity`) is enough — there's no reason for a separate menu scene
given `GensAppController` (added in this branch, `Assets/Scripts/Shell/GensAppController.cs`) already
switches between menu and gameplay UI inside one running scene.

Hierarchy:

- **App** — `GensAppController`
  - `menuDocument`: a `UIDocument` component on this same GameObject (needs its own `PanelSettings`
    asset — create one via `Create → UI Toolkit → Panel Settings Asset` if none exists).
  - `mainMenuAsset`: `Assets/UI/MainMenuScreen.uxml`
  - `newGameSetupAsset`: `Assets/UI/NewGameSetupScreen.uxml`
  - `gameplayRoot`: the **Gameplay** GameObject below
  - `shellBehaviour` / `uiController`: the components on **Gameplay** below
- **Gameplay** (starts inactive in the scene — `GensAppController.Awake` also forces this at runtime,
  but leaving it unchecked in the Inspector avoids a one-frame flash of the gameplay UI on load)
  - `CampaignShellBehaviour` — leave seed/region/etc. fields at their inspector defaults;
    `GensAppController` overwrites them via `Configure(...)` before activating this GameObject for a
    New Game, and leaves them alone for Load Game (see the class doc comments for why both are safe).
  - `UIDocument` (its own `PanelSettings`, separate from the App GameObject's)
  - `GensUIController`, wired exactly as `GoldenPathPlayModeTests` wires it in code:
    `shellBehaviour`, `document`, `inkBarAsset` (`InkBar.uxml`), `householdRosterAsset`
    (`HouseholdRosterScreen.uxml`), `estateSettlementAsset` (`EstateSettlementScreen.uxml`),
    `monthlyReportAsset` (`MonthlyReportScreen.uxml`), `characterDetailAsset`
    (`CharacterDetailScreen.uxml`), `confirmationDialogAsset` (`ConfirmationDialog.uxml`).

Add the scene to `File → Build Settings → Scenes In Build` (index 0).

## 3. Verify

- Enter Play Mode: Main Menu should show (New Game / Load Game — disabled until a save exists / Quit).
- New Game → pick Latium/a difficulty → Begin → the ink bar and Household Roster screen should appear,
  matching `GoldenPathPlayModeTests`' own golden path.
- Use the new ink-bar Household/Estate/Report buttons to confirm navigation and the active-button
  highlight work.
- Save, return to the Main Menu conceptually is not wired yet (no "quit to menu" control exists —
  intentionally out of scope here), but relaunching Play Mode after a Save should make Load Game
  enabled and functional.
- Run `scripts/unity-smoke.sh` (or trigger the `Unity smoke` GitHub Actions workflow manually if a
  `[self-hosted, unity]` runner is ever configured) to get real CI coverage of the Unity compile step —
  today's `Standalone validation` workflow only builds the plain `Gens.slnx` .NET solution and would not
  have caught the CS8773 langversion regression on its own.
