# Unity retirement blocker follow-up tickets

These tickets are required by the blocked decision in
[unity-retirement-audit.md](unity-retirement-audit.md). They are ordered so that
the evidence-producing foundations land before a retirement audit is repeated.

## UR-01 — Establish Unity/native deterministic and save compatibility evidence

**Severity:** Critical

**Problem:** The repository has no permanent Unity-era save fixture matrix and no
fixed-seed Unity/native hash transcript. Unity compilation also fails when normal
.NET `bin/obj` outputs are visible through local packages.

**Evidence:** Audit gates 10, 22, 27, 28, 29, and 71.

**Required outcome:** A clean shared scenario runs through both clients using the
same seed/region/difficulty/commands/month count. Supported pre-native and migrated
saves load natively and continue deterministically.

**Acceptance criteria:**

- [x] Add named permanent current-native, legacy, and migrated `.gens` fixtures.
      **Not done, and cannot be done honestly:** a Unity-era fixture — see
      [ADR 0019](adr/0019-replay-diagnostics-and-save-fixture-contract.md), which
      documents that Unity has never had a working save/load path to have produced
      one.
- [x] Prevent Unity package import from consuming generated `.NET bin/obj` files.
      `Directory.Build.props` redirects build output for the three Unity-imported
      packages to `artifacts/dotnet-build/`; `scripts/check-no-package-build-output.sh`
      enforces it in CI. **Still open:** a human with Unity installed must confirm
      this actually resolves the EditMode CS1704 failure — not verifiable without
      a Unity Editor.
- [x] Record initial, post-command, every-month, and final hashes — done for the
      native client (`ur01-hash-transcript-native.json`, produced by the new
      `run-shared-scenario` command against the fixed scenario in
      `unity-retirement-shared-scenario.md`). **Not done:** the matching Unity-side
      transcript — see ADR 0019's manual follow-up checklist.
- [ ] Prove the hash series matches exactly — blocked on the Unity-side transcript
      above; cannot be claimed until that manual step runs.
- [x] Prove save -> load -> advance continuation matches for every supported
      fixture — `Ur01SharedScenarioFixtureTests` covers all three native fixtures.
- [x] Upgrade replay diagnostics to re-run a persisted command log or explicitly
      revise the gate through an ADR if replay-from-log is not the supported
      contract. **Done via the ADR path:**
      [ADR 0019](adr/0019-replay-diagnostics-and-save-fixture-contract.md) revises
      the gate to save/reload hash equality plus independent continuation parity;
      no command log was built (see that ADR's Alternatives Considered for why).

**Status: partially closed.** Everything achievable without a live Unity Editor is
done. The sole remaining item is the manual Unity-side hash transcript run — see
ADR 0019's "Manual follow-up: Unity-side hash transcript" section for the exact
checklist. Do not re-close this ticket, and do not remove the Unity CI merge gate
(UR-05), until that run completes and its transcript matches the checked-in native
one.

## UR-02 — Complete accessible keyboard-first native UI hardening

**Severity:** Critical

**Problem:** Generic focus primitives, semantic snapshots, pseudo-localization, and
Reduced Motion now exist, but the full keyboard-only workflow, all-screen stress
proof, and complete Windows UI Automation bridge do not.

**Evidence:** Audit gates 13, 34-44.

**Required outcome:** The complete playable slice is usable without a pointer and is
inspectable through the primary OS accessibility stack at supported scaling/content
lengths.

**Acceptance criteria:**

- Automate the required launch -> new -> roster -> character -> estate -> command ->
  advance -> report -> save -> menu -> load keyboard-only workflow.
- Implement and test the Windows UI Automation bridge with one real inspector or
  screen-reader workflow.
- Retain Reduced Motion coverage across every major animated surface.
- Exercise the existing pseudo-locale and add long-content fixtures across all
  primary screens.
- Verify 100%, 125%, 150%, and 200% scaling plus focus visibility/trapping/restore.
- Compare current screen renders against reviewed golden pixels, not only PNG headers.

## UR-03 — Produce and verify the Windows player package

**Severity:** Critical

**Problem:** The self-contained extracted Windows package passes a Unicode,
pseudo-localized, reduced-motion, high-contrast software smoke, but has not completed
the player-facing vertical slice, offline, AI-disabled, forced-no-audio, or fresh-data
exit tests.

**Evidence:** Audit gates 8, 19, 20, 26, 58, 61, 62, and 72-75.

**Required outcome:** A reproducible Windows x64 package runs without an SDK, Unity,
repository checkout, NuGet cache, or system SDL/Skia install.

**Acceptance criteria:**

- Keep the documented packaging script and third-party notices payload verified.
- Test an extracted package in a fresh environment with no generated art/settings/saves.
- Complete the full packaged exit test, save/relaunch/load/advance included.
- Repeat core gameplay offline and with AI disabled.
- Run several New/Menu/Load cycles and a long client soak while recording startup,
  idle CPU, memory, native resources, caches, audio voices, and UI/scene node counts.
- Verify Unicode and unwritable data roots produce correct behavior and useful errors.

## UR-04 — Complete production audio assets and device resilience

**Severity:** Major

**Problem:** `Gens.Audio`, SDL PCM playback, null/failure fallback, and schema-v2 mix
settings exist, but WAV/OGG asset loading, packaged audio fixtures, default-device
recovery, and a forced-no-device package launch remain release gaps.

**Evidence:** Audit gates 50-52.

**Required outcome:** Shipped audio works from the player package, remains optional,
and never blocks launch/gameplay; all relevant feedback remains visual.

**Acceptance criteria:**

- Add WAV/OGG decoding and packaged music/ambience/effect/UI fixtures.
- Recover safely when the default device changes.
- Launch the packaged application with no/fake device and initialization failure;
  verify normal shutdown and retained mix settings.
- Verify all gameplay-relevant notifications retain equivalent visual information.

## UR-05 — Add native package, reference, smoke, and platform CI

**Severity:** Major

**Problem:** CI now builds/tests/publishes Windows, Linux, and macOS archives and runs
Windows packaged smoke, but desktop screen tests still validate only PNG headers and
do not compare current pixels with reviewed goldens.

**Evidence:** Audit gates 68-71.

**Required outcome:** Every mandatory retirement verification is reproducible in CI
without a Unity license after the one-time cross-client transcript is recorded.

**Acceptance criteria:**

- Keep the existing native package matrix and Windows packaged smoke required.
- Add reviewed desktop reference-render comparisons as required checks.
- Keep content, migration, deterministic build, and long simulation checks intact.
- Declare Linux/macOS release support explicitly; add jobs only for declared targets.
- Remove the Unity merge dependency only after UR-01 passes and its evidence is retained.

## UR-06 — Close session, settings, and application-data escape hatches

**Severity:** Major

**Problem:** Native presentation publicly exposes `CampaignSession`. Corrupt settings
recovery, warnings, and v1-to-v2 migration now exist, but the complete Unicode and
failure-path matrix is incomplete.

**Evidence:** Audit gates 6, 20, 54-57.

**Required outcome:** Presentation cannot reach mutable authoritative state, and user
data failures are explicit, recoverable, and tested.

**Acceptance criteria:**

- [x] Remove or hide the native controller's public path to `CampaignSession.State` and
      strengthen architecture tests against indirect exposure.
      `DesktopApplicationController.CurrentCampaign` is now private (exposed only as
      `bool HasActiveCampaign`), and `ArchitectureBoundaryTests` fails the build if any
      public member of the controller is, or embeds as a generic argument,
      `CampaignSession` or `WorldState` (gate 6, now PASS).
- [x] Retain corrupt-settings warning/default and v1-to-v2 migration coverage.
      Covered by new `SettingsAndPathsTests` (`CorruptSettingsFileFallsBackToDefaultsAndIsPreserved`,
      `V1SettingsMigrateReducedMotionIntoMotionModeAndVersion`); the underlying logic in
      `SettingsService`/`DesktopSettings.Migrate` was already present but untested.
- [x] Test save/load/settings/log/cache/screenshot behavior under non-ASCII paths.
      Settings already had Unicode-root coverage (`DesktopApplicationFlowTests.UnicodeApplicationPathsRoundTripAndTraversalIsRejected`).
      New `UnicodeApplicationPathsSaveLoadAndLogRoundTrip` extends this to save/load
      (`DesktopApplicationController.Save`/`Load` against `paths.Quicksave` under a
      `用户-δοκιμή` root, including hash-preserving reload), `StructuredFileLogger`
      (log file created and written under the Unicode `Logs` root), and `CrashReporter`
      (`.json` report written under the Unicode `CrashReports` root). Generated-art
      cache under non-ASCII roots is exercised indirectly by `Gens.Art.Tests`'s
      existing hash-keyed (ASCII-safe) path scheme and is not path-encoding-sensitive
      by construction, so a dedicated cache case was not added.
- [x] Test unwritable roots and atomic-write failures without silent crashes (for settings,
      log, and crash-report paths).
      `SettingsService.Save` had no exception handling at all — an unwritable settings
      directory would throw out of any `Set*` call on `DesktopApplicationController`,
      including `RequestQuit`. Both call sites now catch `IOException`/`UnauthorizedAccessException`,
      log, and roll back to the last-known-good in-memory settings; new
      `SaveTwiceRoundTripsAndLeavesNoTemporaryFile` and
      `UnwritableSettingsRootFailsSaveWithoutThrowingOrLosingPriorState` cover this.
      Campaign save/load already had equivalent handling.
      **New this pass:** `StructuredFileLogger`'s constructor and `Log()`/`Rotate()` had
      no exception handling — an unwritable `Logs` root, or an `IOException` mid-session
      (disk full, permission revoked), threw uncaught. It now degrades to an in-memory-only
      `Recent` buffer instead of throwing, covered by
      `DiagnosticsTests.UnwritableLogsRootDoesNotThrowOnConstructOrLog`. More seriously,
      `CrashReporter.Capture` — the one writer invoked specifically during failure
      handling in `Program.Main`'s outer `catch` — had zero exception handling and no
      guard at its call site either: an unwritable `CrashReports` root would throw a
      *second*, fully unhandled exception out of the exception handler itself, turning a
      graceful "Gens failed to start" message into a raw crash. `Capture` now returns
      `string?` and catches `IOException`/`UnauthorizedAccessException` internally,
      covered by `DiagnosticsTests.UnwritableCrashReportsRootDoesNotThrowAndReturnsNull`.
      The `--capture=` smoke-test screenshot write in `GensDesktopApplication.Render`
      was also unguarded and now catches the same exception set, logging to stderr
      instead of throwing. Generated-art cache writes (`GeneratedArtCache.StoreAsync`)
      remain unguarded at that layer, but its only caller, `ArtGenerationQueue`, already
      catches `IOException`/`HttpRequestException` around the call and reports a
      structured `ArtFailureKind.InvalidOutput`/provider failure instead of propagating,
      so gameplay is not blocked; `GeneratedArtCache.PinAsync`/`ClearUnpinnedAsync` (used
      for cache-retention housekeeping, not the gameplay-blocking store path) remain
      unguarded and are noted as a smaller residual gap.

**Status: partially closed.** Gate 6 (mandatory) is closed. The settings-specific slice of
gates 56/57 (advisory) has real test coverage and a real fix (settings save no longer
throws on an unwritable root). This pass closes the non-ASCII path matrix for
save/load/settings/log/crash-report writers and the unwritable-root/atomic-write gap for
log and crash-report writers, including fixing a real crash-in-crash-handler bug in
`CrashReporter.Capture`. Remaining open items: gate 20's repeated-lifecycle resource-count
observation, non-ASCII coverage for the generated-art cache specifically, and unwritable-root
guards for `GeneratedArtCache.PinAsync`/`ClearUnpinnedAsync`.

## UR-07 — Finish declared cross-platform backend validation

**Severity:** Major

**Problem:** NR2 remains incomplete and ADR 0015 remains Proposed. Linux/macOS SDL
payload, IME/monitor-DPI, and GPU lifetime evidence is incomplete.

**Evidence:** Audit gates 32, 33, 43-45, 59, and 60, plus the native runtime roadmap.

**Required outcome:** Backend support and non-support are declared honestly before
NR7 is reconsidered.

**Acceptance criteria:**

- Finish NR2's actual monitor, IME, GPU lifetime, and cross-RID payload checks.
- Accept/amend/reject ADR 0015 based on measured evidence.
- Declare the retirement release-platform matrix.
- For each declared platform, test build, package, launch, rendering, input, data paths,
  and accessibility gaps; keep undeclared platforms advisory.
