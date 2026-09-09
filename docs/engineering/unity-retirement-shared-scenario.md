# UR-01 shared scenario

This document describes, in prose, the exact scenario
[ADR 0019](adr/0019-replay-diagnostics-and-save-fixture-contract.md) defines as the
one both the native tooling and a human running Unity manually must reproduce
identically, so their resulting state-hash series can be compared checkpoint by
checkpoint. The machine-readable spec is
[`shared-scenario.json`](../../tests/Gens.Simulation.Tests/Saves/Fixtures/shared-scenario.json);
this document exists so a person does not need to parse JSON to know what to run.

## Parameters

| Field | Value |
| --- | --- |
| Seed | `1` |
| Region | `latium` |
| Ruleset | `default` |
| Difficulty | `standard` |
| Start profile | none (default) |
| Start date | month `0` (epoch) |
| Month count | `12` |

## Command sequence

Executed immediately, in this order, right after bootstrap and before any month
advances:

1. Actor `system`, type `ur01.sharedScenario.ledgerTouch`, payload
   `{"note":"UR-01 shared scenario — ledger checkpoint"}`
2. Actor `system`, type `ur01.sharedScenario.stewardshipTouch`, payload
   `{"note":"UR-01 shared scenario — stewardship checkpoint"}`

These are deliberately generic commands (`GenericCommand`/`submit-command`'s own
shape) rather than commands tied to a specific gameplay system — the scenario
exists to prove the command pipeline and month-advance loop reach the same state
in both clients, not to exercise any one system's rules.

## Checkpoints

A `StateHasher` hash is captured at each of the following points, in order:

1. `initial` — immediately after bootstrap, before any command runs.
2. `post-command:<type>` — immediately after each command above executes, in
   sequence.
3. `month:<n>` — after each of the 12 month advances, `n` from 1 to 12.
4. `final` — identical to `month:12`, recorded separately for clarity.

## Running it natively

```sh
dotnet run --project tools/Gens.ContentCompiler -- \
  run-shared-scenario tests/Gens.Simulation.Tests/Saves/Fixtures/shared-scenario.json \
  --content artifacts/content/catalog.json \
  --fixtures-out artifacts/ur01-native/ur01 \
  --transcript-out tests/Gens.Simulation.Tests/Saves/Fixtures/ur01-hash-transcript-native.json
```

This also writes `ur01-legacy.gens` (state at the `initial` checkpoint, before any
command runs) and `ur01-current-native.gens` (state at the `final` checkpoint).

## Running it in Unity (manual follow-up)

See [ADR 0019](adr/0019-replay-diagnostics-and-save-fixture-contract.md#manual-follow-up-unity-side-hash-transcript)
for the full checklist. In short: reproduce the same seed/region/ruleset/
difficulty bootstrap and command sequence above inside the Unity Editor (via
`CampaignShellBehaviour` or the developer console's submit/advance/hash commands),
capture the same checkpoints, and record them into
`ur01-hash-transcript-unity.json` in the same schema as the native transcript for
a checkpoint-by-checkpoint comparison.
