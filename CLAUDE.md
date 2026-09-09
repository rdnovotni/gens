# CLAUDE.md

Guidance for Claude Code (and other AI coding agents) working in this repository.

## What this is

Gens is a deterministic C# simulation game (`src/Gens.Simulation`, targeting
`netstandard2.1`, `.NET 10` tooling per `global.json`) with a purpose-built
native Gens runtime and desktop client (`src/Gens.Client.Desktop` and
friends). The project previously shipped a Unity 6.3 LTS client; Unity has
since been fully retired and removed from the repository. See
[ADR 0014](docs/engineering/adr/0014-custom-runtime-and-native-client.md)
for the target architecture that motivated the migration away from Unity
(kept as historical rationale). The toolchain and architectural boundaries
are recorded in [`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md).
Current gameplay/simulation build status is tracked as a phase checklist in
the [build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md) —
check it rather than assuming from README prose, since it's the document
updated as each phase lands; it links to the native-runtime roadmap for
presentation/platform work.

## Key commands

Run these before considering a change done (full sequence in
[`CONTRIBUTING.md`](CONTRIBUTING.md)):

```sh
dotnet restore Gens.slnx
dotnet format Gens.slnx --no-restore --verify-no-changes
dotnet build Gens.slnx --no-restore --configuration Release
dotnet test Gens.slnx --no-restore --no-build --configuration Release
dotnet run --project tools/Gens.ContentCompiler -- validate content
dotnet run --project tools/Gens.ContentCompiler -- compile content artifacts/content/catalog.json
./scripts/verify-deterministic-build.sh
```

## Repository layout

See the table in [`README.md`](README.md#repository-layout). In short:
`src/Gens.Simulation/` is the engine-free simulation package; `content/source`
+ `content/schemas` is authored content and its validation contract;
`tools/Gens.ContentCompiler` is the CLI that validates/compiles content and
drives headless campaigns (`run-campaign`, `verify-save`, `migrate-save`,
`replay`, plus `--help` for the full command list); `docs/design/` and
`docs/engineering/` hold game design and technical documentation respectively.

## Application boundary

`src/Gens.Application` depends on `Gens.Simulation` and owns campaign-host behavior: creation, queries, commands, month advancement, saves, loads, and replay verification. All clients must use `CampaignSession` for those responsibilities rather than bypassing it. The layer is engine-neutral and contains no platform paths, UI, rendering, audio, SDL, Skia, or AI-provider code.

`src/Gens.Platform` and `src/Gens.Graphics` define the production native abstractions.
SDL calls and unsafe interop stay in `Gens.Platform.Sdl`; SkiaSharp/HarfBuzz types
stay in `Gens.Graphics.Skia`. `Gens.Runtime` references abstractions only and owns
the event-driven host, presentation clock, invalidation, diagnostics, and disposal
order. Use `tools/Gens.EngineSandbox` for native-runtime development; do not import
spike types into production projects.

## Architecture rules

These are load-bearing, not stylistic — see the
[ADR index](docs/engineering/adr/README.md) for the full rationale behind each:

- **Simulation stays presentation- and engine-independent.** `src/Gens.Simulation`
  must never reference presentation or asset APIs (`tech-stack.md`, ADR 0013)
  — and per ADR 0014, this explicitly covers SDL, Skia, native-client/runtime
  code, and external AI art providers. The rule is engine-agnostic: no
  presentation host, past, current, or future, gets a dependency from the
  simulation.
- **Generated artwork stays optional and outside Simulation.** External art-provider
  contracts belong in `Gens.Art`; prompts derive deterministically from structured visual data,
  screens request work through `ArtGenerationQueue`, and procedural portraits remain the immediate
  fallback. Provider failure must never block gameplay, credentials must never be embedded or
  logged, and generated assets/references must never enter authoritative campaign hashes.
- **UI touches simulation only through query projections and commands.**
  UI code reads via `IWorldQuery<TProjection>` implementations under
  `src/Gens.Simulation/Queries/` and writes only by submitting an `ICommand`
  — never by setting a field on a domain object directly (ADR 0013; ADR 0014
  confirms this boundary carries over unchanged to the native client).
  `Gens.Presentation` is the adapter layer that translates between projection
  DTOs and UI view models — do not add a second, competing adapter pattern;
  presentation-model work belongs there. `Gens.Presentation` must not become
  a second place the simulation acquires a dependency on — the dependency
  only ever runs adapter/presentation → simulation, never the reverse.
- **Integers or `Fixed64` only in simulation state.** No `double`/`float` in
  anything that affects campaign outcomes (ADR 0002).
- **Deterministic ordering everywhere.** No raw dictionary/hash-set iteration
  may reach events, hashes, RNG, or saves (ADR 0004).
- **One command path.** Player actions, AI/steward automation, events, and
  migration repairs all go through the same validated command layer (ADR 0006).
- **Truth vs. knowledge stay separate.** Dossiers, rumors, and reports read
  `KnowledgeState`, never the truth partition directly (ADR 0008).

## Documentation conventions

- Put game-design documents in `docs/design/` and engineering/technical
  guidance in `docs/engineering/`.
- When adding, removing, or renaming a file under `docs/design/`, update
  [`docs/design/README.md`](docs/design/README.md) in the same pull request.
- Update documentation in the same pull request when commands, structure, or
  behavior change — this includes this file, README.md, and CONTRIBUTING.md
  when they describe the thing you changed.
- Keep simulation code in `src/Gens.Simulation` independent of any
  presentation/engine platform — per ADR 0014 this covers SDL, Skia,
  native-client code, and external AI providers; add or update tests
  for behavior changes.

## Native runtime migration rules

Before touching presentation, runtime, or platform code, or when a task
description mentions the native client, SDL, Skia, or engine migration:

- Inspect [ADR 0014](docs/engineering/adr/0014-custom-runtime-and-native-client.md)
  before making an architectural call in this area — it is the governing
  contract, not a suggestion.
- Inspect the [native-runtime roadmap](docs/engineering/gens-native-runtime-roadmap.md)
  to see which phase is active and what that phase's exit gate actually
  requires; do not implement ahead of the current phase's scope.
- Do not add SDL, Skia, or equivalent low-level backend dependencies outside
  their designated backend projects (`Gens.Platform.Sdl`, `Gens.Graphics.Skia`,
  and equivalents) — normal application/UI/presentation code must not
  reference those types directly.
- Do not move authoritative campaign state into `Gens.Runtime`,
  `Gens.Client.Desktop`, or any other presentation/runtime layer — the
  simulation-owns-truth rule (ADR 0013) applies to the native client.
- Do not implement speculative engine features (3D, physics, ECS, terrain,
  navmeshes, shader graphs, visual scripting, general-purpose editor
  tooling, or arbitrary user scripting) without new, explicit architectural
  approval — every new engine capability needs a current or explicitly
  planned Gens use case (ADR 0014's scope guardrail).
- Keep PRs small and vertical: one native-runtime roadmap phase (or a
  meaningful slice of one), not a batch of unrelated layers at once.

## Native UI rules

The native desktop client is the primary target for new presentation development.

- Desktop screens query only through `CampaignSession`-backed presenters and submit commands only through `CampaignSession` application operations.
- Presentation DTOs are snapshots: refresh them after relevant commands or lifecycle events; never mutate them as a proxy for simulation state and never query the simulation per frame.
- Generic controls belong in `Gens.UI`; engine-neutral projection mapping belongs in `Gens.Presentation`; client-specific screen composition belongs in `Gens.Client.Desktop`.

- Generic retained controls belong in `src/Gens.UI`; that project must not
  reference Simulation, Application, SDL, or SkiaSharp.
- Use logical units and the shared measure/arrange lifecycle. Invalidate the
  narrowest phase: paint for visual state, arrange for placement, and measure
  only when desired size may change.
- Paint only through `Gens.Graphics`; never use Skia types in UI code.
- Prefer typed theme tokens and typography roles to raw colors/font details.
- Interactive controls must be keyboard focusable and carry appropriate
  `UiSemantics` accessibility information.
- Exercise new generic controls in the EngineSandbox UI gallery and add
  headless tests before campaign screens depend on them.

## Native production-service rules

- Audio is presentation-only and must never gate simulation or be the sole carrier of gameplay information.
- User-facing native strings use stable localization keys; proper names and authoritative content are not localization keys.
- Interactive controls require semantic roles and useful accessibility names. Accessibility regressions require semantic/focus tests.
- Settings are versioned, migrated, atomically written, and separate from campaign saves.
- User-data, save, cache, generated-art, screenshot, log, crash, and mod paths come from `IApplicationPaths`; reject traversal from external filenames.
- Platform-specific implementations stay in platform-specific projects. Generic audio/localization/UI code cannot expose SDL or OS-native types.
- Player packages are self-contained, bundle native dependencies, and must not rely on the SDK, PATH, a NuGet cache, or developer-installed SDL/Skia libraries.

## Scene2D and character portrait rules

- `Gens.Scene2D` is presentation-only. It never owns or queries authoritative
  campaign state; scenes consume projection or presentation models.
- Authoritative structured appearance facts and renderer-specific portrait
  recipes are separate. Recipes, images, caches, and historical portrait
  references never become `WorldState` truth.
- Visual seed derivation and portrait generation must not consume simulation
  RNG. Rendering a portrait must never change campaign state or its hash.
- Every named character must retain a deterministic offline procedural
  portrait and an emergency fallback.
- No AI-provider or vendor-specific image-generation logic belongs in
  Simulation. Do not add such provider logic before Ticket 8, and keep it
  optional when introduced.
