# ADR 0014 — Custom Runtime and Native Client

## Status

Accepted.

This decision reopens and supersedes the platform-selection line of the
[comprehensive build roadmap](../gens-comprehensive-build-roadmap.md)'s original
audit table ("Stack selection | Settled | Keep Unity 6.3 LTS + pure C#
simulation. Do not reopen the engine decision."). It does not follow this
project's usual convention of marking an ADR Accepted only once code and tests
exist behind it (see the [ADR index](README.md)); it is recorded Accepted
immediately, ahead of implementation, because it replaces the currently active
platform direction and must govern all subsequent presentation/runtime work
starting now, not retroactively once a native client happens to exist. It does
not supersede [ADR 0013](0013-ui-projection-boundaries.md) — see "Relationship
to ADR 0013" below.

## Context

Gens's original engine choice (Unity 6.3 LTS) was reasonable at the time it was
made and is not being reopened because it was wrong. It is being reopened
because the shape of the game that has since been built, and the shape of the
game still to come, is not the shape Unity is built for:

- Gens is fundamentally simulation-heavy and presentation-heavy, not
  physics-heavy or continuously-rendered-3D-scene-heavy. The 15 completed
  phases of the build roadmap are household records, character lifecycles,
  estates, production networks, events, reports, policy, delegation, dynasty
  succession, and the Chronicle — not a tactical battle map, a continuously
  simulated 3D world, or real-time physics.
- There is no tactical battle map and no continuously controlled 3D world at
  the heart of the design. The player-facing loop (household roster, estate/
  settlement, monthly report, character detail, confirmations) is
  fundamentally a data-and-document interface over a deterministic monthly
  simulation, not a spatial 3D game.
- `Gens.Simulation` is already engine-independent (`netstandard2.1`, no Unity
  reference, verified by `noEngineReferences`). The hard part of an engine
  migration — decoupling authoritative game logic from the presentation host —
  is already done.
- [ADR 0013](0013-ui-projection-boundaries.md) already isolates presentation
  behind `IWorldQuery<TProjection>` reads and `ICommand` writes. Unity/UI
  Toolkit code holds no mutable domain reference today. This existing
  separation is exactly the boundary a new presentation host needs to sit
  behind, which makes replacing the presentation host substantially lower risk
  than a conventional "rewrite the game in a new engine" migration would be
  for a spatial/physics-heavy game.
- Future Gens presentation still requires substantial 2D capability: layered
  scenes, sprites, cameras, transforms, animation, procedural and AI-generated
  portraits, illustrated events and estates, and richer Chronicle
  visualization are all named in the design corpus and are explicitly not
  ruled out by "no 3D, no physics."
- Given that requirement profile — rich 2D and document-style presentation,
  no 3D, no physics, an already-isolated simulation core — replacing Unity
  with another general-purpose game engine (Godot or otherwise) buys less
  than it costs: a general-purpose engine's value is concentrated in the
  capabilities Gens does not need (3D rendering pipelines, physics, ECS,
  terrain, navmeshes), while its cost (a large, general-purpose dependency
  the project must build around rather than for) still applies. A
  Gens-specific runtime, scoped to exactly the 2D/document/UI capability the
  design actually calls for, is more attractive than adopting a second
  general-purpose engine.

## Decision

**Gens will develop a purpose-built custom runtime and native desktop client,
and will retire Unity once the retirement gates defined below are satisfied.**

This does not mean implementing every low-level computing subsystem from
scratch. The project draws a firm line between what Gens owns and what it
consumes from mature libraries.

### What Gens intends to own

- Application lifecycle above the platform abstraction
- Campaign host/application services
- Navigation
- Presentation models
- UI framework
- Gens-specific controls
- 2D scene framework
- Animation
- Asset identity/resolution
- Generated-art orchestration
- Audio abstraction/mixing layer
- Settings
- Localization
- Accessibility semantics
- Diagnostics
- Developer tooling
- Save frontend
- Input-action abstraction

### What Gens does not intend to implement from scratch

- Operating-system windowing
- DirectX/Vulkan/Metal drivers
- Font rasterization
- Unicode shaping
- Common image codecs
- Basic OS controller APIs
- Low-level audio-device drivers

Mature libraries provide those low-level capabilities behind Gens-owned
abstractions; Gens code does not reimplement them.

### Initial low-level technology direction

The intended starting technologies are:

```text
.NET / C#
SDL3
SkiaSharp
HarfBuzz-compatible text shaping
```

These are **initial backend choices**, distinct from **architectural
commitments**:

| Architectural commitments (durable) | Initial backend choices (replaceable) |
| --- | --- |
| Platform abstraction (`Gens.Platform`) | SDL3 |
| Renderer abstraction (`Gens.Graphics`) | SkiaSharp |
| Retained-mode custom UI (`Gens.UI`) | — |
| Custom Scene2D (`Gens.Scene2D`) | — |
| Engine-neutral presentation models (`Gens.Presentation`) | — |

SDL3 and SkiaSharp are subject to the dedicated technical spike (native-runtime
roadmap phase NR2) before any production commitment to backend details, and
are expected to remain swappable without disturbing normal application/UI
code. **Gens code outside backend implementation projects (`Gens.Platform.Sdl`,
`Gens.Graphics.Skia`, and equivalent future backend projects) must not
directly depend on SDL or Skia types.**

## Runtime scope

The future custom runtime is a **Gens-specific application/game runtime, not a
general-purpose game engine.**

Acceptable to support, eventually:

- 2D sprites, layered scenes, cameras, transforms, animation
- Vector graphics, raster images, procedural imagery, AI-generated imagery
- Particles where justified
- Audio
- Mouse/keyboard/controller input
- Custom UI
- Text-heavy screens

Not initially intended to support:

- 3D
- Physics
- ECS
- Terrain
- Navmeshes
- Multiplayer replication
- Shader graphs
- Visual scripting
- General-purpose editor tooling
- Arbitrary user scripting

Any of these require separate future architectural approval if a real Gens
requirement emerges for them — see "Custom-engine scope guardrail" below.

## Relationship to ADR 0013

**ADR 0013 remains valid and is not superseded.** ADR 0013's rule — UI touches
the simulation only through read-only query projections and command submission,
and no mutable domain object is ever exposed to presentation code — is
generalized here from "Unity/UI Toolkit" to "any presentation host, including
the future native client," but the rule itself is unchanged. The native client
must obey the exact same query/command boundary Unity obeys today. Nothing in
this ADR grants the native client, `Gens.Runtime`, `Gens.UI`, or any other
presentation-layer project any access to simulation state that Unity does not
already have.

## Target layering

```text
Gens.Simulation
        ↓
Gens.Application
        ↓
Gens.Presentation
        ↓
Gens.Runtime
   ├── Gens.UI
   ├── Gens.Scene2D
   ├── Gens.Graphics
   ├── Gens.Assets
   ├── Gens.Audio
   └── Gens.Art
        ↓
Gens.Client.Desktop
        ↓
Platform + rendering implementations
        ↓
SDL3 + SkiaSharp + operating system
```

### `Gens.Simulation`

Authoritative deterministic game mechanics (already implemented). Must remain
independent of platform, rendering, UI, external AI, networking, and audio.
Unchanged by this ADR.

### `Gens.Application`

Engine-neutral application/campaign orchestration. Expected future
responsibilities: `CampaignSession`, campaign bootstrap, query gateway, command
gateway, month advancement, save/load orchestration, replay verification,
session lifecycle. **This layer will be implemented in a later ticket; it is
not created by this ADR.**

### `Gens.Presentation`

Engine-neutral presentation models:

```text
Simulation projection
        ↓
Presentation model
        ↓
Concrete UI
```

Prevents UI code from needing to understand simulation internals, and prevents
display models from becoming tied to SDL/Skia types.

### `Gens.Runtime`

Application infrastructure: application loop, navigation, modal lifecycle,
async jobs, settings, service lifetime, presentation clock, diagnostics.

### `Gens.UI`

Custom retained-mode user interface: layout, text, focus, input, scrolling,
styling, accessibility semantics, Gens-native widgets.

### `Gens.Graphics`

Renderer-independent graphics contracts: canvas, images/textures, paths,
transforms, clipping, render targets.

### `Gens.Graphics.Skia`

Initial Skia implementation. A backend, not an architectural dependency of
normal gameplay/UI code.

### `Gens.Platform`

Platform contracts: window, event loop, input, clipboard, file dialogs,
display/DPI, lifecycle, low-level audio device access.

### `Gens.Platform.Sdl`

Initial SDL3 backend.

### `Gens.Scene2D`

Future lightweight 2D scene framework: `Scene2D`, `SceneNode2D`, `Transform2D`,
`Sprite2D`, `Camera2D`, layers, animation. **`Scene2D` is not authoritative
simulation state** — it is a presentation-side representation driven by
projections, exactly like every other presentation surface under ADR 0013.

### `Gens.Assets`

Stable asset identities, manifests, loading, caching, and asset-source
resolution.

### `Gens.Art`

Procedural and AI-assisted visual generation orchestration. External AI
providers must eventually live outside `Gens.Simulation` (they already do, per
the existing `IArtGenerationProvider` seam).

### `Gens.Audio`

Gens-owned audio abstraction/mixing behavior over a low-level backend.

### `Gens.Client.Desktop`

Composition root and the actual desktop executable. Minimal business logic.

## Dependency direction

```text
Gens.Client.Desktop
        │
        ▼
Gens.Runtime
   ├──────────► Gens.UI
   ├──────────► Gens.Scene2D
   ├──────────► Gens.Audio
   └──────────► Gens.Assets
                    │
                    ▼
              Gens.Graphics

Gens.UI ───────────► Gens.Graphics
Gens.Scene2D ──────► Gens.Graphics

Gens.Runtime
        │
        ▼
Gens.Presentation
        │
        ▼
Gens.Application
        │
        ▼
Gens.Simulation
```

Backend implementations sit at the outside, selected only by the composition
root:

```text
Gens.Graphics.Skia → Gens.Graphics
Gens.Platform.Sdl  → Gens.Platform
```

### Explicit forbidden dependency directions

```text
Gens.Simulation → Gens.Application
Gens.Simulation → Gens.Runtime
Gens.Simulation → Gens.UI
Gens.Simulation → Gens.Graphics
Gens.Simulation → SDL
Gens.Simulation → Skia
Gens.Simulation → external AI provider
```

Normal UI/presentation code (`Gens.UI`, `Gens.Scene2D`, `Gens.Runtime`,
`Gens.Presentation`, `Gens.Application`, and any future equivalent) must not
directly reference SDL or Skia implementation types. Only `Gens.Platform.Sdl`
and `Gens.Graphics.Skia` (and equivalent future backend projects) may do so.

## Simulation target-framework migration

`Gens.Simulation` currently targets `netstandard2.1`. This is kept unchanged
during the dual-client migration because the existing Unity client consumes
the package at that target. **This ADR does not retarget `Gens.Simulation`.**
After Unity retirement, moving it to the current .NET target is a distinct,
separately-approved future change — not bundled with the presentation
migration, since the two are independent concerns (framework target vs.
presentation host).

## Unity migration policy

### State A — Current

```text
Unity = primary playable client
Native client = not yet implemented
```

### State B — Parallel migration

```text
Unity client
      │
      ├──── same Simulation/Application layer
      │
Native client
```

Both clients coexist, consuming the same `Gens.Simulation`/`Gens.Application`
layer through the same query/command boundary. The native client should
increasingly become the primary development target as it gains parity, but
Unity is not degraded, disconnected, or feature-frozen merely because the
native client exists — it stops receiving new investment only once the
retirement gates below pass.

### State C — Retirement

Unity is removed only after all of the following gates pass:

1. Native client launches independently of Unity.
2. Existing campaign creation works.
3. Existing `.gens` saves load.
4. Save/load produces deterministic-equivalent campaign state.
5. Query/command architecture remains intact.
6. Current playable vertical slice is available.
7. Main menu/new-game/settings flow is available.
8. Household Roster works.
9. Character Detail works.
10. Estate/Settlement works.
11. Monthly Report works.
12. Confirmations work.
13. Month advancement works.
14. Dev/debug tooling is ported.
15. Automated native-client tests exist.
16. A packaged native build can run outside a development environment.
17. No required production feature still depends on Unity.

Only after all 17 gates pass should a dedicated Unity-removal change delete
`Assets/`, `Packages/`, `ProjectSettings/`, Unity-specific CI/tooling, and
Unity-only adapters. This ADR does not perform that removal and does not
schedule it to any specific future ticket; it only defines the gates.

## Art/AI architecture rule

**Simulation defines appearance facts and deterministic visual recipes. Art
systems depict those facts. Generated art never determines game state.**

Target flow:

```text
Character state
      ↓
CharacterVisualProfile
      ↓
Appearance description / PortraitRecipe
      ↓
Procedural renderer OR AI art provider
      ↓
Generated asset
```

Never:

```text
generated image
      ↓
infer character mechanics
```

AI generation must be optional, asynchronous, cancelable, cacheable,
replaceable, and non-blocking, and must never be authoritative. Missing
generated art must never prevent campaign load or simulation advancement.
This generalizes the existing `IArtGenerationProvider` seam and tech-stack.md
policy to the native runtime; it does not change the seam's current behavior.

## 2D support rule

The native client is not intended to remain a static, application-style UI
forever. The native runtime should deliberately support future 2D
presentation, including procedural portraits, AI portraits, illustrated
events, estate visualization, environmental layers, sprites, sprite animation,
cameras, transforms, transitions, animated overlays, weather/atmospheric
visual effects, and Chronicle illustration. This capability must remain
presentation-only unless an explicit simulation command/query crosses the
existing ADR 0013 boundary — 2D richness is never a backdoor into authoritative
state.

## Application-loop principle

**Simulation time** continues to be controlled by deterministic game
advancement (`GameDate.TotalMonths`, phased monthly ticks) exactly as today.

**Presentation time** may use real elapsed time for animation, cursor
movement, audio fades, page turns, wax-seal effects, and particles.

Presentation time must never determine authoritative simulation outcomes. A
frame's elapsed-time delta is a `Gens.Runtime`/`Gens.Scene2D` concept; it never
reaches `Gens.Simulation`.

## Custom-engine scope guardrail

**Every new engine capability must be justified by a current or explicitly
planned Gens use case.** Do not build generalized systems simply because
Unity, Godot, or Unreal contain them. This mirrors the roadmap's existing
"we build what the corpus requires, not what a generic engine happens to
offer" discipline, applied now to the runtime itself. This rule is repeated in
`CLAUDE.md` for coding agents.

## Consequences

- No new dependencies (SDL, SkiaSharp, HarfBuzz) are added by this ADR. No new
  projects (`Gens.Application`, `Gens.Runtime`, `Gens.Graphics`, `Gens.UI`,
  `Gens.Scene2D`, `Gens.Client.Desktop`) are created by this ADR. This is a
  documentation-only architectural decision; implementation begins in later
  tickets tracked by the
  [native-runtime migration roadmap](../gens-native-runtime-roadmap.md).
- Unity remains the only playable client until State B produces a native
  client with real parity, and remains supported until all 17 retirement
  gates in State C pass.
- `Gens.Simulation` remains `netstandard2.1` until a separate, later decision
  retargets it after Unity retirement.
- Future tickets that touch presentation should check this ADR and the
  native-runtime roadmap before introducing new engine-shaped abstractions.
