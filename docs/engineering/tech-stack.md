# Technical baseline

## Native stack

Per [ADR 0014](adr/0014-custom-runtime-and-native-client.md), Gens migrated
its presentation/runtime platform from Unity to a purpose-built native Gens
runtime and desktop client. That migration is complete: Unity has been fully
retired and removed from the repository. This document describes the current
native stack.

```text
.NET 10
Custom Gens runtime/client (Gens.Runtime, Gens.UI, Gens.Scene2D, Gens.Client.Desktop, ...)
Gens.Application netstandard2.1 campaign-session host
Gens.Simulation netstandard2.1
SDL3 backend (Gens.Platform.Sdl)
SkiaSharp backend (Gens.Graphics.Skia)
custom retained-mode Gens UI
custom lightweight Scene2D
.NET 10 standalone tooling
```

The production-shaped foundation is documented in
[native-runtime-architecture.md](native-runtime-architecture.md). The isolated
NR2 evidence harness and its still-open desktop validation items are documented
in [native-runtime-spike-results.md](native-runtime-spike-results.md). SDL/Skia
remain a conditional backend decision until those measured exit-gate items are
completed. Ticket 4 implementation proceeded under an explicit override; the
experiment remains separate and is not a production dependency.

The retained-mode UI foundation is now implemented in `Gens.UI` and documented
in [ui-framework.md](ui-framework.md). It consumes only the engine-neutral
graphics contracts; the desktop client and real campaign screens remain future
work.

The lightweight scene framework and deterministic offline portrait pipeline are
implemented and documented in [scene2d.md](scene2d.md) and
[character-visual-pipeline.md](character-visual-pipeline.md). Procedural portrait
layers are original parametric vector primitives; no external AI provider is part
of NR5.

## Native runtime versions and boundaries

- Runtime/tool target: .NET 10.
- SDL: 3.2.22 Windows x64 app-local payload, project-owned narrow C ABI.
- SkiaSharp: 3.119.1; OpenGL 3.3 core candidate plus software reference surfaces.
- HarfBuzzSharp: 8.3.1.2, isolated behind backend-neutral shaped glyph runs.
- Font fixture: bundled Noto Sans under the SIL Open Font License.

Only backend projects reference these native packages or types. `Gens.Runtime`,
`Gens.Application`, and `Gens.Simulation` do not.

The remainder of this document describes the current native baseline. See
ADR 0014 for the full target-stack layering and dependency rules, and the
[native-runtime migration roadmap](gens-native-runtime-roadmap.md) for the
history of how the migration off Unity proceeded.

## Application layer

`Gens.Application` is the engine-neutral, `netstandard2.1` campaign host used by the native client. `CampaignSession` owns bootstrap, query/command dispatch, monthly advancement, save/load, and replay verification above `Gens.Simulation`. Clients choose platform paths and keep pause, settings, audio, and presentation concerns outside this layer. Mutable state and RNG access remain public only as a documented transitional escape hatch retained from the legacy Unity client's call sites.

## Boundaries

- `Gens.Simulation` is a `netstandard2.1` library with no engine references.
  It must not reference presentation or asset APIs — generalized by ADR 0014
  to: no platform, rendering, UI, external AI, or networking dependency of
  any kind. Retargeting its target framework is a separate future decision.
- Simulation outcomes use integer values and named, persisted PCG32 streams.
  Commands are validated before mutation and produce domain events. Monthly ticks
  are deterministic and target 250 ms normally and one second at maximum scale.
- UI is the custom retained-mode `Gens.UI` tree, painted through `Gens.Graphics`
  (backed by SkiaSharp) with SDL3 windowing/input. `Gens.Scene2D` provides the
  lightweight scene framework for cutaways and visualization.
- Begin with ordinary managed collections. Native collections and low-level
  performance work require profiling evidence.

## Data, saves, and assets

Authored JSON and CSV are inputs to the .NET content compiler. JSON Schema,
stable string IDs, uniqueness, and references are validated before normalized
runtime JSON is emitted.

Save files use the `.gens` extension and are atomic ZIP containers with
`manifest.json`, `world.json`, optional `history.json`, generated-asset references,
an explicit version, and all deterministic RNG states. Breaking changes require a
migration and a permanent fixture. Artwork recipes and metadata are persisted,
but generated images live once in a SHA-256-addressed cache under the platform's
application data path (see `IApplicationPaths`) with separate thumbnails and a
versioned manifest.

SVG icons have a 64x64 viewBox, stable semantic IDs, supported vector
primitives, outlined text, semantic palette tokens, and deterministic
placeholders. Procedural layered SVG portraits are the required baseline and
are reproducible from `CharacterVisualProfile`, `PortraitRecipe`, seed, and
renderer version.

AI art is optional, asynchronous, and never blocks a campaign. Client code uses
`IArtGenerationProvider`, initially with null and mock providers. Production cloud
providers belong behind a controlled backend; local models run out of process.

## Developer tooling

A Settings-gated in-app dev/debug console (backquote to toggle) lets a
developer or tester query and submit commands against a running campaign
without leaving the build — see the Input section of
[`docs/engineering/native-client.md`](native-client.md). It reaches the
campaign exclusively through the same `CampaignShell.Query`/`Submit` boundary
as the rest of the UI (ADR 0013).

## Verification

NUnit and FsCheck cover the standalone simulation, including golden seeds,
invariants, save round trips, and migrations. BenchmarkDotNet tracks monthly
ticks. CI validates content, runs the full test suite and migrations, and
builds the native client across platforms before merge.
