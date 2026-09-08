# Gens native desktop client

`src/Gens.Client.Desktop` is the primary target for new presentation development. It is a .NET 10 native executable composed from the production SDL platform backend, Skia graphics backend, runtime host, retained UI, engine-neutral presentation layer, and `Gens.Application`.

Unity remains a supported transitional client until every retirement gate in ADR 0014 passes.

## Architecture

```text
Gens.Client.Desktop
  -> Gens.Runtime / Gens.UI
  -> Gens.Presentation
  -> Gens.Application
  -> Gens.Simulation

composition root only:
  -> Gens.Platform.Sdl / Gens.Graphics.Skia
```

`Program.cs` creates paths, controller, SDL, Skia, the desktop application, and `RuntimeHost`. `DesktopApplicationController` owns navigation and at most one `CampaignSession`. Replacing or abandoning a campaign drops the old presentation gateway and its snapshots. Screens never retain domain objects.

`CampaignPresentation` performs query-to-view-model mapping for the Ink Bar, roster, character detail, estate, monthly report, and action confirmations. Pure mapping lives in `ProjectionMappers`; the transitional Unity adapters delegate to it. Presentation references no Unity, SDL, Skia, or concrete UI control.

Screens refresh only when entered or after a command, month advance, save/load lifecycle event, or settings change. Rendering consumes retained snapshots and never queries the simulation per frame. The gameplay shell retains one Ink Bar while the screen host changes. Runtime dirty rendering returns to event wait after each change.

## Run and test

```sh
dotnet run --project src/Gens.Client.Desktop --configuration Release
dotnet run --project src/Gens.Client.Desktop --configuration Release -- --new-game
dotnet run --project src/Gens.Client.Desktop --configuration Release -- --renderer=software --smoke-test
dotnet test tests/Gens.Client.Desktop.Tests --configuration Release
dotnet publish src/Gens.Client.Desktop -c Release -r win-x64 --self-contained false
```

`--screen=<name>` and `--capture=<path>` are deterministic development-fixture options used by `scripts/capture-native-goldens.ps1`. They use normal campaign bootstrap and navigation; they do not mutate state behind commands.

## Input

- Tab / Shift+Tab: move focus; Enter or Space activates the focused control.
- Escape: cancel a modal, return from Character Detail, or go back to the main menu screens.
- Backquote: toggle the developer console when enabled in Settings.
- F1: layout outlines. F2: dump the retained UI tree.

The console supports `help`, `state`, `hash`, `replay`, `query`, `submit`, `save`, `load`, `advance`, and `clear`. Campaign operations route through `CampaignSession`; it has no privileged mutation path. `replay` writes a diagnostic save into the native cache, reloads it, and compares canonical state hashes.

## User data

Windows uses `%LOCALAPPDATA%/Gens`, macOS uses `~/Library/Application Support/Gens`, and Linux uses `$XDG_DATA_HOME/gens` (or `~/.local/share/gens`). Startup establishes centralized save, settings, logs, cache, generated-art, screenshots, crash-report, and mod paths; traversal derived from external input is rejected. `settings/settings.json` is separate, versioned, migrated, atomically replaced, and preserved with a `.corrupt-*` suffix when invalid. The current save frontend uses `saves/quicksave.gens` and the shared `.gens` reader/writer.

Implemented settings are UI scale (100–200%), Master/Music/Ambience/Effects/UI audio levels and mute, reduced motion, high contrast, runtime locale selection, generated-art policy, and explicit developer-console enablement. The engine-neutral mixer uses an isolated SDL3 PCM16 push-stream backend and falls back silently when the subsystem or default device is unavailable; WAV/OGG asset decoding remains outstanding.

## Native parity checklist

- [x] Main Menu
- [x] New Game (Latium, Campania, Cisalpina; Standard, Hard, Relaxed)
- [x] Settings
- [x] Credits
- [x] Campaign bootstrap
- [x] Persistent Ink Bar
- [x] Household Roster
- [x] Character Detail and back navigation
- [x] Estate/Settlement
- [x] Monthly Report
- [x] Ordinary confirmation
- [x] Wax-seal confirmation
- [x] Advance Month
- [x] Save and load
- [x] Return to Main Menu with session clearing
- [x] Developer console and hash/replay diagnostics

Procedural raster portraits, Scene2D, production service foundations, and self-contained package automation are present. A real audio device/codec backend, full Windows UI Automation provider, macOS/Linux accessibility bridges, packaged universal font fallback, cross-platform SDL payload validation, signing, and Unity retirement remain later roadmap work.
