# Technical baseline

## Transitional stack vs. target stack

Per [ADR 0014](adr/0014-custom-runtime-and-native-client.md), Gens is
migrating its presentation/runtime platform from Unity to a purpose-built
native Gens runtime and desktop client. This document describes both states;
do not read the presence of Unity below as meaning it is the permanent
platform, and do not assume Unity has already been removed — it has not.

**Transitional stack (current, in active use):**

```text
Unity 6.3 LTS existing client
Gens.Application netstandard2.1 campaign-session host
Gens.Simulation netstandard2.1
.NET 10 standalone tooling
```

**Target stack (adopted direction, not yet implemented):**

```text
.NET 10
Custom Gens runtime/client (Gens.Runtime, Gens.UI, Gens.Scene2D, Gens.Client.Desktop, ...)
SDL3 backend (Gens.Platform.Sdl)
SkiaSharp backend (Gens.Graphics.Skia)
custom retained-mode Gens UI
custom lightweight Scene2D
```

The isolated NR2 evidence harness and its still-open desktop validation items
are documented in [native-runtime-spike-results.md](native-runtime-spike-results.md).
SDL/Skia remain conditional backend candidates until those measured exit-gate
items are completed; the experiment is not a production dependency.

The remainder of this document, unless a section says otherwise, describes
the **current, transitional** baseline — the Unity client and the standalone
tooling that exist and are exercised by CI today. See ADR 0014 for the full
target-stack layering, dependency rules, and Unity retirement gates, and the
[native-runtime migration roadmap](gens-native-runtime-roadmap.md) for the
phased plan to get from here to there. Unity remains fully supported and is
not degraded, disconnected, or deprioritized by this migration until the
retirement gates in ADR 0014 pass.

## Version policy (transitional Unity client)

The project is pinned to Unity 6.3 LTS by `ProjectVersion.txt`; Unity Hub must
install that exact editor. Editor changes are made only in a dedicated upgrade
pull request, which must also commit the regenerated `packages-lock.json`.
Standalone tools and tests target .NET 10 LTS. Unity uses the .NET Standard 2.1
API compatibility level, Mono for editor iteration, and IL2CPP for verified
production release builds.

## Application layer

`Gens.Application` is the engine-neutral, `netstandard2.1` campaign host shared by the transitional Unity client and future .NET client. `CampaignSession` owns bootstrap, query/command dispatch, monthly advancement, save/load, and replay verification above `Gens.Simulation`. The compatible target is required because Unity consumes both projects as local source packages; clients choose platform paths and keep pause, settings, audio, and presentation concerns outside this layer. Mutable state and RNG access remain public only as a documented transitional escape hatch for existing Unity call sites.

## Boundaries (transitional)

- `Gens.Simulation` is a `netstandard2.1` library and a local Unity package with
  `noEngineReferences`. It must not reference Unity, presentation, or asset APIs
  — generalized by ADR 0014 to: no platform, rendering, UI, external AI, or
  networking dependency of any kind, Unity included. `Gens.Simulation` stays
  `netstandard2.1` through the migration; retargeting it is a distinct future
  decision made only after Unity retirement (ADR 0014).
- Simulation outcomes use integer values and named, persisted PCG32 streams.
  Commands are validated before mutation and produce domain events. Monthly ticks
  are deterministic and target 250 ms normally and one second at maximum scale.
- UI is UI Toolkit (UXML/USS, VectorImage and Painter2D). Scene-like cutaways use
  SpriteRenderer and the URP 2D Renderer. uGUI requires a documented exception.
- Begin with ordinary managed collections. Jobs, Burst, Mathematics, and native
  collections require profiling evidence; Entities, ECS, NetCode, Havok, and a
  full DOTS architecture are excluded from the baseline.

## Data, saves, and assets

Authored JSON and CSV are inputs to the .NET content compiler. JSON Schema,
stable string IDs, uniqueness, and references are validated before normalized
runtime JSON is emitted. ScriptableObjects are presentation configuration only.

Save files use the `.gens` extension and are atomic ZIP containers with
`manifest.json`, `world.json`, optional `history.json`, generated-asset references,
an explicit version, and all deterministic RNG states. Breaking changes require a
migration and a permanent fixture. Artwork recipes and metadata are persisted,
but generated images live once in a SHA-256-addressed cache under
`Application.persistentDataPath` with separate thumbnails and a versioned manifest.

Shipped artwork and optional packs use Addressables. SVG icons have a 64x64
viewBox, stable semantic IDs, supported vector primitives, outlined text, semantic
palette tokens, and deterministic placeholders. Procedural layered SVG portraits
are the required baseline and are reproducible from `CharacterVisualProfile`,
`PortraitRecipe`, seed, and renderer version.

AI art is optional, asynchronous, and never blocks a campaign. Client code uses
`IArtGenerationProvider`, initially with null and mock providers. Production cloud
providers belong behind a controlled backend; local models run out of process.

## Developer tooling

A Settings-gated in-app dev/debug console (backquote to toggle) lets a
developer or tester query and submit commands against a running campaign
without leaving the Editor/build — see
[`docs/engineering/dev-console.md`](dev-console.md). It reaches the campaign
exclusively through the same `CampaignShell.Query`/`Submit` boundary as the
rest of the UI (ADR 0013).

## Verification

NUnit and FsCheck cover the standalone simulation, including golden seeds,
invariants, save round trips, and migrations. Unity Test Framework covers EditMode
and PlayMode, while UI Test Framework covers critical workflows. BenchmarkDotNet
tracks monthly ticks. CI validates content, runs both test suites and migrations,
and verifies a Unity build before merge.
