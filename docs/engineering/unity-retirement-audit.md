# Unity retirement readiness audit

## Audit identity and decision

| Field | Value |
| --- | --- |
| Audit record commit | `befd55cc328a2def484389fc3b4fcb6bfeddab4c` |
| Audit baseline commit | `f6d4c92f8b2af0c1b065a4418d51a34c8d67c6c6` |
| Audit date | 2026-09-08 |
| Native build version | `Gens.Client.Desktop` 0.1.0, .NET 10 |
| Unity build/version | Unity 6000.3.23f1 (revision `09d2ecc7fb28`) |
| Primary release platform | Windows x64 is the only declared runnable package target |
| Secondary platforms | Linux x64 and macOS arm64 produce CI archives as advisory build evidence, not release-ready packages |
| Decision | **BLOCKED** |

Unity retirement is not approved. The native client is a substantial playable
implementation, but it is not yet a complete production presentation host. Unity,
its project files, adapters, tests, CI workflow, and compatibility code must remain.

The requested prerequisite name `native-runtime-roadmap.md` does not exist; the
actual roadmap is [gens-native-runtime-roadmap.md](gens-native-runtime-roadmap.md).
The requested audio, localization, accessibility, and packaging documents now exist
and are included in this audit.

## Status rules

`PASS` means the gate has concrete source, test, command, or package evidence.
`FAIL` means required capability/evidence is absent or a verification command failed.
`PARTIAL` means some implementation exists but the stated gate has not been proven.
Every mandatory `FAIL` and `PARTIAL` below blocks retirement.

## Evidence index

- **E1 — architecture:** project references in `Gens.slnx` and the `src/*/*.csproj`
  files; [ADR 0013](adr/0013-ui-projection-boundaries.md), [ADR 0014](adr/0014-custom-runtime-and-native-client.md),
  [native runtime architecture](native-runtime-architecture.md), and
  `ArchitectureBoundaryTests`.
- **E2 — native functional tests:** `DesktopApplicationFlowTests` and related
  desktop-client tests (25 total),
  `CampaignSessionTests` (5 tests), and the native controller/application source.
- **E3 — UI/visual tests:** `Gens.UI.Tests` (25 tests), `Gens.Scene2D.Tests` (10
  tests), `Gens.Portraits.Tests` (9 tests), and ten 1280x720 native golden PNGs.
- **E4 — save/replay:** the only permanent `.gens` fixture is
  `tests/Gens.Simulation.Tests/Saves/Fixtures/v1-empty-campaign.gens`;
  `SaveLoadRestoresExactHashAndContinuation`; and the content-tool verification run.
- **E5 — generated art:** `Gens.Art.Tests` (14 tests), including null provider,
  provider failure, cache corruption, and hash-independence cases.
- **E6 — runtime/performance:** `RuntimeHostTests`, `Scene2DTests`,
  [native-runtime-architecture.md](native-runtime-architecture.md), and
  [scene2d.md](scene2d.md).
- **E7 — packaging/platform:** [native-packaging.md](native-packaging.md),
  `publish-native-client.ps1`, `Gens.Platform.Sdl/runtimes`,
  `THIRD-PARTY-NOTICES.md`, package metadata, and the extracted native smoke run.
- **E8 — CI/tooling:** `.github/workflows/standalone.yml`,
  `.github/workflows/unity-smoke.yml`, and `scripts/`.
- **E9 — Unity audit:** all production scripts under `Assets/Scripts`, all Unity
  tests under `Assets/Tests`, `Packages/manifest.json`, and
  `ProjectSettings/ProjectVersion.txt`.
- **E10 — production services:** `DesktopSettings`, `DesktopSettingsStore`,
  `GensDesktopApplication`, `UiSemantics`, `MotionPolicy`, `Gens.Audio`,
  `Gens.Localization`, [audio-system.md](audio-system.md),
  [localization.md](localization.md), [accessibility.md](accessibility.md), and
  [ui-framework.md](ui-framework.md).
- **E11 — local verification:** see the exact commands and results near the end of
  this document.

## Retirement gate table

| # | Gate | Class | Status | Evidence and finding |
| ---: | --- | --- | --- | --- |
| 4 | Simulation independence | Mandatory | PASS | E1: `Gens.Simulation` references only `System.Text.Json`; no Unity, SDL, Skia, UI, runtime, or client project reference. |
| 5 | Application independence | Mandatory | PASS | E1/E2: `Gens.Application -> Gens.Simulation`; 5 headless `CampaignSessionTests` pass. |
| 6 | Query/command boundary | Mandatory | PASS | E1/E2: native screens use presentation/session APIs. UR-06 removed the indirect escape hatch — `DesktopApplicationController.CurrentCampaign` is now private, exposed to callers only as a `bool HasActiveCampaign`, and `ArchitectureBoundaryTests.DesktopControllerDoesNotExposeCampaignSessionOrWorldState` fails the build if any public property or method return type on the controller is, or embeds as a generic argument, `CampaignSession` or `WorldState`. |
| 7 | Unity-only authoritative logic | Mandatory | PASS | E9: no unique authoritative gameplay rule found; Unity scripts are classified below. Unity-only master-volume persistence is presentation-only and has a native replacement. |
| 8 | Application startup | Mandatory | PARTIAL | E7/E11: the extracted Release package launches and exits cleanly in software smoke, but an interactive main-menu/quit package test is absent. |
| 9 | New Game | Mandatory | PASS | E2: region, difficulty, seed, bootstrap, and deterministic first-month behavior use shared `CampaignStartOptions`/`CampaignSession`. |
| 10 | Load Game fixture matrix | Mandatory | FAIL | E4: current temporary saves load; legacy/Unity-era/migrated permanent fixture coverage requested by this gate is absent. |
| 11 | Main Menu parity | Mandatory | PASS | E2/E3: New Game, Load, Settings, Credits, and Quit are implemented; reference images exist. |
| 12 | Gameplay shell | Mandatory | PASS | E2/E3: persistent Ink Bar, navigation, and date/status projections are implemented. |
| 13 | Household Roster | Mandatory | PARTIAL | E2/E3: people, selection, scroll, portraits, and generic keyboard focus exist; full roster keyboard/focus-restoration workflow is not tested. |
| 14 | Character Detail | Mandatory | PASS | E2/E3: projection-backed detail and back navigation are covered. |
| 15 | Estate/Settlement | Mandatory | PASS | E2/E3: essential projection data/actions and non-authoritative Scene2D preview exist. |
| 16 | Monthly Report | Mandatory | PASS | E2/E3: shared projections and report screen are covered. |
| 17 | Consequential commands | Mandatory | PASS | E2: preview, modal, cancel/hash stability, confirm, result, and refresh are covered. |
| 18 | Month advancement | Mandatory | PASS | E2/E4: deterministic repeated advancement is covered by native and simulation tests. |
| 19 | Return to Main Menu | Mandatory | PARTIAL | E2: new/save/load/menu paths exist, but both required new->menu and load->menu sequences are not independently covered end-to-end. |
| 20 | Repeated session lifecycle | Mandatory | PARTIAL | E2: one replacement cycle is covered; several full New/Menu/Load cycles with managed/native resource observations are absent. |
| 21 | Canonical save compatibility | Mandatory | PASS | E2/E4: native save/load calls shared `CampaignSession`/`SaveReader`/`SaveWriter`; no second format exists. |
| 22 | Unity-era save fixtures | Mandatory | FAIL | E4: no fixture is identified as a supported Unity-era save. |
| 23 | Save migration chain | Mandatory | PASS | E4/E11: the permanent v1 fixture tests and current v1 migrate command pass; there is only one schema version. |
| 24 | Native save verification | Mandatory | PASS | E2/E4/E11: native hash-preserving save/load test and `verify-save` pass. |
| 25 | Future deterministic continuation | Mandatory | PASS | E2: saved session and independently continued session hashes match after the next month. |
| 26 | Generated art independence | Mandatory | PARTIAL | E5: corrupt/missing generated cache falls back safely and save data excludes pixels; exact delete-caches-then-load-client flow is not covered. |
| 27 | Fixed-seed cross-client parity | Mandatory | FAIL | E9/E11: no Unity/native hash transcript exists; Unity EditMode compilation failed before tests, so initial/monthly/final cross-client hashes were not produced. |
| 28 | Presentation independence | Mandatory | PARTIAL | E5/E6: provider variants do not change hashes and Scene2D is separated, but one identical scenario across all four requested rendering modes is absent. |
| 29 | Replay diagnostics | Mandatory | PARTIAL | E4/E11: reload/hash verification passes, but the tool reports that no persisted command log exists and commands are not replayed. |
| 30 | Long soak | Mandatory | PASS | E11: the full 1,677-test simulation suite, including exit-gate soak tests, passes. |
| 31 | Minimum resolution | Mandatory | PASS | E3: all ten primary native reference renders are 1280x720; scroll hosts preserve reachability. |
| 32 | Standard desktop resolutions | Advisory | PARTIAL | E6: 1080p visual benchmarks exist; no complete 1920x1080 and 2560x1440 client workflow audit. |
| 33 | 4K | Advisory | PARTIAL | E6: 4K Scene2D benchmark exists; no full-client quality/usability pass. |
| 34 | UI scaling | Mandatory | PARTIAL | E3/E10: 100/125/150/175/200% choices exist; primary screens are not validated at each required value. |
| 35 | Pseudo-localization | Mandatory | PARTIAL | E7/E10/E11: `qps-ploc` exists and the packaged Settings smoke passes; all primary screens and critical controls have not been exercised under it. |
| 36 | Long-content stress | Advisory | FAIL | No large-roster, long-report, long-name, or long-localized-label fixture covers the native screens. |
| 37 | Reference renders | Mandatory | PARTIAL | E3: generic UI renders compare deterministically, but desktop `ScreenGoldenTests` only verify checked-in files are PNGs, not that current rendering matches them. |
| 38 | Keyboard-only vertical slice | Mandatory | FAIL | E3: generic Tab/modal/button tests exist; the required launch-through-load application workflow is absent. |
| 39 | Focus integrity | Mandatory | PARTIAL | E3: generic Tab/Shift+Tab, modal trap, restore, and disabled-control tests pass; no complete native-screen focus audit. |
| 40 | Reduced Motion | Mandatory | PASS | E10/E11: `MotionPolicy` disables decorative SceneView updates, limits informational transitions, is unit-tested, and reduced-motion packaged smoke passes. |
| 41 | Accessibility semantic audit | Mandatory | PASS | E3/E10: buttons, toggles, headings, dialogs, scroll regions, portraits, and estate preview expose validated roles/names/descriptions; active-modal scope and focus are tested. |
| 42 | Primary OS accessibility bridge | Mandatory | FAIL | E10: Windows emits native focus-change events, but the required full UI Automation fragment provider and a real inspector/screen-reader workflow are incomplete. |
| 43 | Secondary OS accessibility gaps | Advisory | PARTIAL | E10: macOS NSAccessibility and Linux AT-SPI are explicitly documented null bridges; real-host gap validation remains absent. |
| 44 | Scene2D stability | Mandatory | PARTIAL | E3/E6: graph, camera, culling, animation-idle, SceneView, and embedded estate tests exist; actual DPI/resize soak is incomplete. |
| 45 | Scene2D performance | Advisory | PASS | E6: recorded 1080p/4K 100-1,000-node and animation benchmark results are within documented development observations. |
| 46 | Procedural portrait baseline | Mandatory | PASS | E3/E5: deterministic procedural source and fallback remain available with AI disabled. |
| 47 | Portrait determinism | Mandatory | PASS | E3: recipe, seed, hash, byte-output, and cache tests pass at 128/256/512. |
| 48 | AI art optionality | Mandatory | PASS | E5: `NullArtProvider`/disabled configuration leaves procedural portraits active. |
| 49 | AI provider failure | Mandatory | PASS | E5: transient failure, rejection, cancellation, offline cloud, and worker-crash paths preserve usability/fallback. |
| 50 | No audio device | Mandatory | PARTIAL | E10: SDL device/subsystem failure degrades to silence and fake/null backend tests pass, but the required packaged launch with a forced unavailable device was not run. |
| 51 | Volume/mute | Advisory | PASS | E10: schema v2 provides Master, Music, Ambience, Effects, and UI levels plus Master mute; changes update active voices and are tested. |
| 52 | No audio-only gameplay | Mandatory | PASS | No native gameplay signal is audio-only; all current feedback is visual. |
| 53 | Fresh user profile | Mandatory | PASS | E2/E10: controller tests create a new isolated root and required directories/settings work. |
| 54 | Corrupt settings | Mandatory | PASS | E10: corrupt JSON is preserved as `.corrupt-*`, safe defaults load, and the client wires the store warning callback to logging. |
| 55 | Settings migration | Advisory | PASS | E10: schema v1 is upgraded to v2 with defaulted audio settings and fixture coverage. |
| 56 | Unicode paths | Mandatory | PARTIAL | E7/E10/E11: settings/path tests and an extracted-package smoke pass under a Unicode user root; the full save/load/log/cache/screenshot matrix is incomplete. |
| 57 | Read-only/failure paths | Advisory | PARTIAL | Save/load catches useful I/O failures. UR-06 added corrupt-settings recovery, v1-to-v2 migration, atomic-save round-trip, and unwritable-settings-root tests, and made `SettingsService.Save` fail without throwing or losing the prior in-memory value; log/cache/screenshot paths are still not comprehensively tested. |
| 58 | Windows package | Mandatory | PARTIAL | E7/E11: a 46.5 MB self-contained extracted package passes pseudo/reduced/high-contrast/Unicode smoke; the required full player-facing save/relaunch/load/audio feature matrix is absent. |
| 59 | Linux package | Advisory | FAIL | E7: CI produces a 43.2 MB archive, but Linux is not a declared release target and the package lacks repository-owned SDL and real-host validation. |
| 60 | macOS package | Advisory | FAIL | E7: CI produces a 43.2 MB archive, but macOS is not a declared release target and SDL/Skia/HarfBuzz plus real-host validation remain incomplete. |
| 61 | No global development dependencies | Mandatory | PASS | E7/E11: publish is self-contained, verifies app-local dependencies/assets, rejects Unity content, and the extracted Windows executable runs without the SDK or repository lookup. |
| 62 | Smoke test | Mandatory | PASS | E7/E11: the extracted packaged executable exited 0 under software rendering and produced a 1280x720 capture. |
| 63 | Native dependency/license audit | Mandatory | PASS | E7: SDL3, SkiaSharp, HarfBuzzSharp, and Noto Sans payload/license locations are identifiable; SDL and font notices are checked in. |
| 64 | Developer console | Mandatory | PASS | E2: help/state/hash/query/submit/clear plus replay/save/load/advance are implemented; console integration test passes. |
| 65 | Console boundary | Mandatory | PASS | E2: console routes through `DesktopApplicationController` and `CampaignSession`; no console-specific domain mutation exists. |
| 66 | Runtime diagnostics | Advisory | PASS | E6: runtime metrics, UI tree, scene tree, render metrics, art diagnostics, and campaign hash are available. |
| 67 | Headless tooling | Mandatory | PASS | E11: content validate/compile, campaign, save verify/migrate/replay work independently of clients. |
| 68 | Standalone CI | Mandatory | PASS | E8/E11: format/build/all 1,789 tests, content tooling, and deterministic build pass; `global.json` now accepts the installed 10.0.400 feature band. |
| 69 | Native-client CI | Mandatory | PARTIAL | E8: Windows/Linux/macOS package jobs build and test; Windows runs extracted-package smoke. Desktop golden tests still do not compare current pixels with reviewed references. |
| 70 | Platform jobs | Mandatory | PASS | E8: CI builds/tests/publishes Windows x64, Linux x64, and macOS arm64; only the declared primary Windows target is claimed runnable. |
| 71 | No Unity-required merge gate | Mandatory | PARTIAL | Required PR checks are standalone, but the final cross-client parity evidence cannot currently be reproduced because Unity compilation fails after ordinary .NET outputs are present. |
| 72 | Startup | Advisory | PARTIAL | E7: packaged pseudo/reduced/high-contrast smoke takes 3.395 seconds start-to-exit; the distinct main-menu-interactive measurement is absent. |
| 73 | Idle CPU | Mandatory | PASS | E6: event-wait behavior is tested; recorded Windows sandbox observation was 0 ms process CPU over a 3-second idle interval. |
| 74 | Memory | Advisory | PARTIAL | Historical runtime allocations exist; requested main-menu/gameplay/portrait/Scene2D process baselines are absent. |
| 75 | Long client soak | Advisory | FAIL | No extended native navigation/advance resource-count soak exists. |
| 76 | Screen performance | Advisory | PASS | E3/E6: dense 1,000-node UI and visual benchmarks are comfortably responsive in recorded Windows runs. |
| 77 | Native docs | Mandatory | PASS | README/native-client docs name native as the primary presentation-development target and Unity as transitional. |
| 78 | No stale mandatory Unity guidance | Mandatory | PASS | Repository search was classified: active guidance is still relevant because retirement is blocked; ADR/roadmap/audit references are historical/transitional. |
| 79 | Historical accuracy | Mandatory | PASS | ADR 0013/0014 and migration history are preserved unchanged. |

## Decision table

| Measure | Result |
| --- | --- |
| Mandatory gates | **39/61 PASS** |
| Mandatory partial | **17** |
| Mandatory fail | **5** |
| Advisory gates | **5/15 PASS** |
| Advisory partial | **6** |
| Advisory fail | **4** |
| Retirement decision | **BLOCKED** |

Mandatory failed gates: 10, 22, 27, 38, and 42. Mandatory partial gates:
8, 13, 19, 20, 26, 28, 29, 34, 35, 37, 39, 44, 50, 56, 58, 69,
and 71. (Gate 6 moved from partial to pass — see UR-06.)

## Blockers

The implementation work is split into actionable tickets in
[unity-retirement-follow-up-tickets.md](unity-retirement-follow-up-tickets.md).
The blocking themes are:

1. **Critical — compatibility evidence:** no Unity-era fixture matrix and no
   fixed-seed Unity/native parity transcript.
2. **Critical — accessible input/release quality:** no keyboard-only vertical slice,
   all-screen pseudo-localization proof, or complete Windows UIA bridge.
3. **Critical — player-package exit test:** the extracted Windows smoke passes, but
   full save/relaunch/load, offline, AI-disabled, forced-no-audio, and lifecycle/soak
   workflows have not run against the player package.
4. **Major — release audio completion:** the resilient audio foundation exists, but
   packaged assets/decoding, device recovery, and forced-no-device launch remain.
5. **Major — CI/tooling:** native package/platform/smoke jobs exist, but reviewed
   desktop pixel comparison is absent and Unity local-package compilation is polluted
   by normal .NET `bin/obj` outputs.
6. **Major — hardening:** mutable session exposure, lifecycle/resource soak,
   complete Unicode/failure paths, and desktop golden comparison remain.

## Unity script classification

No authoritative game rule exists only in Unity. The production scripts classify
as follows:

| Unity area | Classification | Finding |
| --- | --- | --- |
| `Adapters/*` (clock, roster, detail, estate, report, ink bar, confirmation, command outcome) | compatibility wrapper / presentation-only | Shared model mapping is in `Gens.Presentation`; UI Toolkit binding remains Unity-only. |
| `PortraitAdapter` | presentation-only / obsolete after retirement | Unity monogram+tint placeholder; native deterministic raster portraits are richer. |
| `CampaignShell` | compatibility wrapper | Thin facade over `CampaignSession`; exposes transitional state/RNG access. |
| `CampaignShellBehaviour` | compatibility wrapper | Unity lifecycle/bootstrap host. |
| `GensAppController` | presentation-only, contains unique non-authoritative behavior | Unity menu/new/load/settings/quit host; Unity-only `PlayerPrefs` master-volume storage is superseded by native schema-v2 audio settings. |
| `GensUIController` | presentation-only / compatibility wrapper | UI Toolkit composition; it directly reads state to build/preview commands but submits through shared pipelines. Native uses `CampaignClientActions`. |
| `DevConsole/*` | compatibility wrapper / presentation-only | Unity console UI and command wrappers; native console covers the essential command set. |
| `Compatibility/IsExternalInit.cs` | compatibility shim | Unity compiler support only. |

Unity tests contain deliberate direct fixture mutation for characters/land/building
setup. That is test scaffolding, not production gameplay logic, but it reinforces
that cross-client parity needs a new shared, non-mutating scenario harness.

## Save and deterministic results

- Unity-era save compatibility: **FAIL** — no fixture is designated as a supported
  Unity-era save, and Unity-compiled save/load is conditionally unsupported in
  `CampaignSession` because Unity does not receive the csproj `System.Text.Json`
  reference.
- Native save/load future continuation: **PASS** —
  `SaveLoadRestoresExactHashAndContinuation` and
  `SaveLoadPreservesHashRandomStreamsAndFutureDeterminism` pass.
- Native-created save verification: **PASS** — 12-month seed-1 save hash
  `ecf7b8462f18ac09` passed checksum verification, migration, and reload.
- Replay: **PARTIAL** — saved-state hash validation succeeds, but the tool explicitly
  reports that commands are not replayed because no command log is persisted.
- Unity/native fixed-seed parity: **FAIL** — no comparable hash series was produced.

## Native package, input, accessibility, and platform results

| Area | Result |
| --- | --- |
| Native packaged vertical slice | PARTIAL — extracted-package smoke only; no player-package vertical slice |
| Offline | PARTIAL — core play is network-independent and Null AI passes, but packaged offline E2E was not run |
| AI disabled | PASS at service/test level; packaged E2E not run |
| No audio device | PARTIAL — null/failure backend path passes; forced packaged launch not run |
| Keyboard-only | FAIL — generic controls tested, full vertical slice absent |
| Windows accessibility | FAIL — semantic snapshots/focus events exist; full UIA provider and real inspection absent |
| Linux accessibility | PARTIAL/advisory — explicit null bridge; not a declared retirement target |
| macOS accessibility | PARTIAL/advisory — explicit null bridge; not a declared retirement target |
| Pseudo-localization | PARTIAL — packaged Settings smoke passes; full screen suite absent |
| Windows package | PARTIAL — self-contained smoke passes; full exit test absent |
| Linux package | FAIL/advisory — archive builds, runtime payload/host validation incomplete |
| macOS package | FAIL/advisory — archive builds, runtime payload/host validation incomplete |

## Performance results

The existing evidence records idle event waiting and 0 ms process CPU over a
three-second idle observation after animation settled. The 1920x1080 reference
workload averaged 2.523 ms (p95 3.338 ms, p99 3.855 ms). Scene2D recorded
1.123/1.998/4.282 ms at 1080p and 4.036/5.245/6.587 ms at 4K for
100/500/1,000 nodes. These support runtime viability, but startup-to-interactive,
full-client memory baselines, and a long client resource soak remain open.

## Dependency graph and framework decision

```text
Gens.Simulation
  <- Gens.Application
    <- Gens.Presentation
      <- Gens.Art / Gens.Portraits / Gens.Client.Desktop

Gens.Platform <- Gens.Platform.Sdl
Gens.Platform <- Gens.Graphics <- Gens.Graphics.Skia
Gens.Graphics <- Gens.Scene2D <- Gens.UI
Gens.Platform + Gens.Graphics <- Gens.Runtime
```

`Gens.Simulation` remains `netstandard2.1`. Retargeting is explicitly deferred
because Unity retirement is blocked and the Unity/Application/Presentation source
packages still consume the compatible target. No retarget or legacy-seam deletion
was performed. The obsolete `Gens.Simulation.Art.IArtGenerationProvider` seam is
already absent; `Gens.Art.IArtProvider` is correctly outside Simulation.

## Repository-wide Unity reference classification

Active Unity references remain intentionally present in `Assets`, `Packages`,
`ProjectSettings`, the manual Unity workflow/script, `.mcp.json`, current dual-client
instructions, and compatibility guards in Application/Simulation. Historical
references remain in ADRs and roadmap history. No reference is being labeled stale
and removed while the decision is blocked. Current docs now link this audit so the
dual-client status is explicit.

## Exact verification commands and results

| Command | Result |
| --- | --- |
| `dotnet --version` with repository `global.json` | PASS: 10.0.400 selected through `latestFeature` roll-forward |
| `dotnet restore Gens.slnx` | PASS |
| `dotnet format Gens.slnx --no-restore --verify-no-changes` | PASS |
| `dotnet build Gens.slnx --no-restore -c Release` | PASS, 0 warnings/errors |
| `dotnet test Gens.slnx --no-restore --no-build -c Release` | PASS: 1,789 tests (1,677 Simulation + 112 other), 0 failed/skipped |
| `./scripts/verify-deterministic-build.sh` | PASS: repeated build hash `eb152dcc7f46874eb1861842dff90064e6bf423899487f58a5a717861b5c0465` |
| `Gens.ContentCompiler.exe validate content` | PASS: 59 definitions, 10 families |
| `Gens.ContentCompiler.exe compile content artifacts/content/ticket10-catalog.json` | PASS |
| `Gens.ContentCompiler.exe run-campaign --seed 1 --months 12 ...` | PASS: `ecf7b8462f18ac09` |
| `Gens.ContentCompiler.exe verify-save artifacts/saves/ticket10-smoke.gens` | PASS |
| `Gens.ContentCompiler.exe migrate-save ...` | PASS: v1 to v1 |
| `Gens.ContentCompiler.exe replay artifacts/saves/ticket10-smoke.gens` | PARTIAL: reload hash matches; no command log to re-run |
| `./scripts/publish-native-client.ps1 -Runtime win-x64` | PASS: self-contained 46,548,272-byte ZIP |
| packaged `Gens.Client.Desktop.exe --renderer=software --smoke-test --screen=settings --developer --locale=qps-ploc --reduced-motion --high-contrast --user-data=artifacts/smoke-用户-δοκιμή` | PASS: exit 0 and 1280x720 capture |
| Unity 6000.3.23f1 EditMode test command | FAIL before tests: duplicate generated assembly attributes, then CS1704 duplicate `Gens.Simulation` import; no test-result XML |

Unity-generated untracked `.meta` files were removed; no authored Unity file was
changed.

## Retirement action result

- Unity removed: **No**.
- Directories removed: **None**.
- Required assets migrated: **None required or attempted in a blocked audit**.
- Transitional source removed: **None**.
- Unity CI/tooling removed: **None**.
- Framework retarget performed: **No**.
- Determinism after retarget: **Not applicable**.
- Fresh-clone verification: **Partial**; CI covers clone/restore/build/test/publish
  across three RIDs, but an independently fresh Windows host was not used locally.
- Final packaged smoke/end-to-end: **Smoke PASS; full end-to-end NOT RUN**.

## Remaining technical debt and recommendation

Complete the linked blocker tickets, beginning with deterministic/save evidence and
the Windows package/accessibility/keyboard gates. Re-run this audit only after all
mandatory rows have executable evidence. Do not start the post-migration gameplay
roadmap or retarget Simulation as part of blocker work; first prove Unity is truly
redundant. When the audit eventually passes, the next major phase should return to
the main gameplay/content roadmap while treating the native runtime as ordinary Gens
infrastructure, not a standalone general-purpose engine.

## UR-01 progress update — 2026-09-09

This is an addendum, not a rewrite: the gate table and decision above remain exactly
as recorded on 2026-09-08, since this document is a point-in-time snapshot ("Re-run
this audit only after all mandatory rows have executable evidence" — the audit is
not yet re-run). This addendum records concrete, native-side-only progress against
UR-01, made without access to a Unity Editor, per
[ADR 0019](adr/0019-replay-diagnostics-and-save-fixture-contract.md).

- **Gate 71** (no stale Unity bin/obj CI pollution) — new evidence:
  `Directory.Build.props` now redirects `Gens.Simulation`/`Gens.Application`/
  `Gens.Presentation` build output to `artifacts/dotnet-build/`, outside the folders
  `Packages/manifest.json` imports into Unity as local packages, and
  `scripts/check-no-package-build-output.sh` (wired into the `standalone` CI job)
  fails the build if that ever regresses. Confirmed locally: `dotnet build
  Gens.slnx` no longer leaves any `bin`/`obj` under `src/Gens.Simulation`,
  `src/Gens.Application`, or `src/Gens.Presentation`. **Still open:** a human with
  Unity installed must confirm this actually resolves the audit's observed CS1704
  duplicate-assembly failure in the Editor — that check cannot run without one.
- **Gate 29** (replay diagnostics) — ADR 0019 now defines the gate's contract as
  save/reload hash equality plus independent continuation parity, both of which have
  real test evidence (existing `VerifyDeterministicReplay`/gate 25, plus this
  update's new fixture-continuation tests below). No persisted command log was
  built; ADR 0019 explains why that is not required for this gate.
- **Gate 10** (save fixture matrix) — new permanent fixtures exist under
  `tests/Gens.Simulation.Tests/Saves/Fixtures/`: `ur01-current-native.gens`,
  `ur01-legacy.gens` (state before the shared scenario's commands run), and
  `ur01-migrated.gens` (the current-native fixture run through `migrate-save`,
  confirmed byte-identical — today's only possible migration, since
  `SaveMigrationRegistry.Empty` means v1 is the only schema version that has ever
  shipped). A recorded hash transcript
  (`ur01-hash-transcript-native.json`) covers bootstrap, every submitted command,
  every month boundary, and the final state of a fixed shared scenario (documented
  in `unity-retirement-shared-scenario.md`). `Ur01SharedScenarioFixtureTests`
  proves each fixture's hash matches its recorded checkpoint and that each
  continues deterministically after an independent reload. This remains **PARTIAL**,
  not PASS — no Unity-era fixture exists, per gate 22 below.
- **Gate 22** (Unity-era save fixtures) — remains **FAIL**. ADR 0019 documents why:
  `SaveReader`/`SaveWriter`/`CampaignSession.Save`/`Load` are all
  `#if !UNITY_2021_1_OR_NEWER`-excluded because `Gens.Simulation.asmdef` has no
  `System.Text.Json` reference Unity's asmdef compiler can see, so Unity has never
  been able to independently produce a `.gens` file. No fixture was fabricated to
  flip this gate; it stays honestly FAIL until either that asmdef gap is separately
  closed and a real Unity-produced save is captured, or the audit's maintainer
  amends the gate to acknowledge Unity never had this capability.
- **Gate 27** (cross-client parity) — remains **FAIL**. The native half of the
  comparison this gate needs is now ready and checked in
  (`ur01-hash-transcript-native.json`); ADR 0019's manual follow-up checklist
  defines exactly what a human with Unity installed must run to produce the
  matching Unity-side transcript and complete the comparison. This gate is not
  flipped until that comparison actually runs and passes.

See [UR-01](unity-retirement-follow-up-tickets.md#ur-01--establish-unitynative-deterministic-and-save-compatibility-evidence)
for the itemized acceptance-criteria status.
