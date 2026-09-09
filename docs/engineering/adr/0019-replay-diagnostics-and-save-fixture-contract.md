# ADR 0019 — Replay Diagnostics and Save Fixture Contract

**Status:** Accepted

## Context

[The Unity retirement readiness audit](../unity-retirement-audit.md) (2026-09-08)
blocked retirement and listed "replay diagnostics" and "save fixture matrix" as
mandatory gates (29 and 10, plus the related 22, 27, and 28) with only partial or
failing evidence. [UR-01](../unity-retirement-follow-up-tickets.md#ur-01--establish-unitynative-deterministic-and-save-compatibility-evidence)
requires either upgrading replay diagnostics "to re-run a persisted command log," or
"explicitly revis[ing] the gate through an ADR if replay-from-log is not the
supported contract." This ADR takes the second path, and separately records what a
save fixture matrix can honestly consist of today.

**Replay today.** `ReplayCommand`'s own doc comment already states the actual
contract plainly: "Phase 4 has not yet built a persisted command log or scheduler
to actually re-run commands against, so there is nothing to replay *from* yet. What
this command proves today is the exit gate's weaker but still real claim: loading a
save reproduces the exact same state hash it was saved with, every time"
(`tools/Gens.ContentCompiler/Commands/ReplayCommand.cs`). `CampaignSession.
VerifyDeterministicReplay` (`src/Gens.Application/Campaign/CampaignSession.cs`)
implements exactly this: save the current state, reload it, and compare
`StateHasher` hashes. No `ICommand` implementation has a serialization/DTO layer
(unlike `WorldSaveDto` for world state), and no `CommandLog`/journal concept exists
anywhere in the codebase. Building one now — a serializable command envelope,
a log storage format, replay-time re-execution against `CommandPipeline` in
`SequenceNumber` order, and handling of the RNG draws `_mutate` consumes along the
way — is a multi-week simulation feature with no current consumer (no crash-recovery
or spectator-replay use case exists today), not a fixture-and-tooling debt fix. Rule
2 (one command path) and ADR 0006's atomicity guarantees do not require a persisted
log to hold; they only require that *if* commands are re-run, they produce
identical results, which is a property this ADR's continuation checks (below)
already exercise without needing a log.

**Unity-era saves.** `SaveReader`, `SaveWriter`, and `CampaignSession.Save`/`Load`/
`VerifyDeterministicReplay` are each wrapped in `#if !UNITY_2021_1_OR_NEWER` and
throw `NotSupportedException` under Unity, because `Gens.Simulation.asmdef` has no
`precompiledReferences` entry for `System.Text.Json`, even though
`Gens.Simulation.csproj` pulls it in via a NuGet `PackageReference` that Unity's
asmdef compilation never sees. Unity has therefore never been able to independently
produce a `.gens` file. A "Unity-era save fixture," read literally, cannot exist —
there was never a working Unity save path to have produced one.

## Decision

**Replay diagnostics contract.** The audit's gate 29 ("Replay diagnostics") is
satisfied by two properties, both provable without a persisted command log:

1. **Save/reload hash equality** — the existing `VerifyDeterministicReplay`/
   `ReplayCommand`/`compare-hashes` behavior: reloading a save reproduces the exact
   state hash it was saved with.
2. **Independent continuation parity** — extending audit gate 25's existing pattern
   (already PASS) into the standard replay contract: given a save taken mid-scenario,
   an in-memory session that keeps advancing and a freshly reloaded session that
   advances the same way must produce identical hashes at every subsequent step.
   This is the property that actually matters for "does reloading a save let you
   keep playing identically," and it does not require replaying *how* the state was
   reached, only that continuing from it is deterministic.

A persisted command log remains legitimate future work — `ReplayCommand`'s doc
comment correctly identifies it as the natural next step *if* a real consumer
(crash recovery, spectator replay, desync debugging) creates the need — but it is
out of this ADR's and UR-01's scope, and is not required for gate 29 to pass.

**Save fixture honesty.** Given Unity has never had a working save path, the
fixture matrix UR-01 asks for is interpreted as follows, and no fixture is ever
fabricated to fill a slot it cannot honestly fill:

- **current-native** — a save written by today's native `SaveFormat.CurrentVersion
  = 1` writer via the shared scenario (below). Fully achievable.
- **legacy** — reinterpreted as *legacy-shaped*, not literally pre-existing: a save
  taken from an earlier point in the same scenario's run (before its command
  sequence executes), still a valid, unmigrated v1 save. This is an honest, useful
  fixture — it is not a claim about a prior schema version, since none has ever
  shipped.
- **migrated** — `SaveMigrationRegistry.Empty` means v1 is the only schema version
  that has ever existed; there is no real migration to exercise yet. The honest
  fixture here is the current-native save run through `migrate-save` and asserted
  byte-identical (a v1→v1 identity migration) — proof the migration pipeline is
  exercised correctly today, not evidence of a migration that has not happened.
  True multi-version migration fixture coverage is deferred until a real schema
  change ships; recording that as a known future gap is correct, flipping the gate
  to a false PASS would not be.
- **Unity-era** — not produced, because it cannot be produced honestly. Gate 22
  stays FAIL until either the Unity `System.Text.Json` asmdef gap is closed and a
  real Unity-produced save is captured (out of this ADR's scope — it is a Unity
  build-configuration fix, not a fixture-authoring task), or the audit is amended by
  its maintainer to acknowledge Unity never had this capability and retire the gate.
  This ADR does not unilaterally flip gate 22's status; it only removes the
  ambiguity about why no such fixture exists.
- **Cross-client parity (gate 27)** — reframed as *hash parity at the shared
  scenario's checkpoints*, not "a save Unity wrote." The shared scenario (below)
  is designed so a human can reproduce it manually inside Unity once its
  compilation gap is fixed, and compare the resulting hash series against the
  native transcript this ADR's tooling produces. See the manual follow-up below.

## The shared scenario

A single fixed scenario — seed, region, ruleset, difficulty, an ordered command
sequence, and a month count — defined once at
`tests/Gens.Simulation.Tests/Saves/Fixtures/shared-scenario.json` and described in
prose at [`unity-retirement-shared-scenario.md`](../unity-retirement-shared-scenario.md),
so it can be run identically by native tooling now (`run-shared-scenario` in
`tools/Gens.ContentCompiler`) and by a human in the Unity Editor later. Hashes are
captured at: bootstrap, after each submitted command, at every month boundary, and
at the end. The native-side transcript
(`tests/Gens.Simulation.Tests/Saves/Fixtures/ur01-hash-transcript-native.json`) is
checked in; the Unity-side transcript is not, until a human produces it (see below).

## Manual follow-up: Unity-side hash transcript

This cannot run in an environment without a Unity Editor. A human with Unity
6000.3.23f1 (or later) installed must:

1. Confirm the Unity bin/obj import fix (`Directory.Build.props`) actually resolves
   the audit's observed CS1704 duplicate-assembly failure: run a normal
   `dotnet build Gens.slnx --configuration Release`, then open the project in the
   Unity Editor (or `./scripts/unity-smoke.sh`) and confirm EditMode compiles
   without duplicate generated assembly attributes or a duplicate `Gens.Simulation`
   import.
2. Run `dotnet run --project tools/Gens.ContentCompiler -- run-shared-scenario
   tests/Gens.Simulation.Tests/Saves/Fixtures/shared-scenario.json --content
   artifacts/content/catalog.json --fixtures-out artifacts/ur01-native
   --transcript-out artifacts/ur01-native/hash-transcript.json` if the checked-in
   native transcript needs refreshing.
3. Inside the Editor (via `CampaignShellBehaviour`/the developer console's
   submit/advance/hash commands), reproduce the same seed/region/ruleset/
   difficulty bootstrap and command sequence from
   `docs/engineering/unity-retirement-shared-scenario.md`, capturing the state hash
   at every listed checkpoint.
4. Record the result into `ur01-hash-transcript-unity.json` in the same schema as
   the native transcript, and diff it against
   `ur01-hash-transcript-native.json` checkpoint by checkpoint.
5. If any checkpoint diverges, do not paper over it — file it as its own ticket
   naming the exact divergent checkpoint. This ADR's job is to define and build the
   comparison harness, not to guarantee the two clients already agree.
6. Once matching, commit `ur01-hash-transcript-unity.json` and update the audit's
   gate 27 (and 10/28) from FAIL/PARTIAL only once this comparison has actually run
   and passed — never preemptively.

## Consequences

- Gate 29 (replay diagnostics) is satisfiable today: save/reload hash equality plus
  independent continuation parity, both already proven by existing and
  ADR-extended tests, are the supported contract — not command-log replay.
- `ReplayCommand`'s CLI help text and doc comment should be updated to point at this
  ADR instead of describing the missing command log as an open gap.
- Gate 22 (Unity-era save fixtures) remains honestly FAIL; this ADR documents why,
  rather than manufacturing a fixture with false provenance.
- Gate 10 (fixture matrix) moves from FAIL toward PARTIAL: current-native,
  legacy-shaped, and migrated (identity) fixtures now exist and are tested; Unity-era
  remains out of reach until the Unity asmdef gap is separately fixed.
- Gate 27 (cross-client parity) stays FAIL until a human runs the manual follow-up
  above; the native-side half of that comparison is now ready and checked in.
- [`gens-comprehensive-build-roadmap.md`](../gens-comprehensive-build-roadmap.md)'s
  references to "replay" as a merge gate should be read as this ADR's contract
  (save/reload/continuation hash equality), not command-log replay.
- A persisted command log remains legitimate future work if a real consumer
  (crash recovery, spectator replay, desync debugging) emerges, but is explicitly
  out of scope until then.

## Alternatives Considered

- **Build a persisted command log now.** Rejected: no serialization layer for
  `ICommand` exists, no current feature consumes it, and it is a multi-week
  simulation-surface addition disproportionate to a fixture/tooling debt ticket —
  exactly the kind of scope creep the audit's own closing recommendation warns
  against ("Do not start the post-migration gameplay roadmap... as part of blocker
  work").
- **Leave gate 29 permanently PARTIAL with no documented contract.** Rejected: an
  audit gate should have a stable, provable definition; leaving it ambiguous means
  it can never honestly reach PASS.
- **Fabricate a Unity-era or multi-version-migrated fixture to flip gates 22/10.**
  Rejected outright: a save file's provenance is part of its evidentiary value: a
  native save relabeled as "Unity-era," or a synthetic old schema invented solely to
  be migrated from, would be a false claim the audit exists specifically to prevent.
