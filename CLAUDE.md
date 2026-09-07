# CLAUDE.md

Guidance for Claude Code (and other AI coding agents) working in this repository.

## What this is

Gens is a deterministic C# simulation game (`src/Gens.Simulation`, targeting
`netstandard2.1`, `.NET 10` tooling per `global.json`) currently migrating
from its existing Unity 6.3 LTS client to a purpose-built native Gens runtime
and desktop client. Unity is the working, supported client today; it is not
the long-term platform. See [ADR 0014](docs/engineering/adr/0014-custom-runtime-and-native-client.md)
for the adopted target architecture and Unity retirement gates, and the
[native-runtime migration roadmap](docs/engineering/gens-native-runtime-roadmap.md)
for the phased plan. The toolchain and architectural boundaries are recorded
in [`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md), which
distinguishes the current transitional stack from the target stack.
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

For Unity-side changes, see CONTRIBUTING.md's `scripts/unity-smoke.sh`
section and the Unity MCP server setup (below).

## Repository layout

See the table in [`README.md`](README.md#repository-layout). In short:
`Assets/`/`Packages/`/`ProjectSettings/` is the Unity project; `src/Gens.Simulation/`
is the Unity-free simulation package; `content/source` + `content/schemas` is
authored content and its validation contract; `tools/Gens.ContentCompiler` is
the CLI that validates/compiles content and drives headless campaigns
(`run-campaign`, `verify-save`, `migrate-save`, `replay`, plus `--help` for
the full command list); `docs/design/` and `docs/engineering/` hold game
design and technical documentation respectively.

## Architecture rules

These are load-bearing, not stylistic — see the
[ADR index](docs/engineering/adr/README.md) for the full rationale behind each:

- **Simulation stays presentation- and engine-independent.** `src/Gens.Simulation`
  must never reference Unity, presentation, or asset APIs (`tech-stack.md`,
  ADR 0013) — and per ADR 0014, this now explicitly also covers SDL, Skia, any
  future native-client/runtime code, and external AI art providers. The rule
  is engine-agnostic, not Unity-specific: no presentation host, past, current,
  or future, gets a dependency from the simulation.
- **UI touches simulation only through query projections and commands.**
  UI code reads via `IWorldQuery<TProjection>` implementations under
  `src/Gens.Simulation/Queries/` and writes only by submitting an `ICommand`
  — never by setting a field on a domain object directly (ADR 0013; ADR 0014
  confirms this boundary carries over unchanged to the future native client).
  `Assets/Scripts/Adapters` is the current, transitional Unity-specific
  adapter layer permitted to translate between projection DTOs and UI Toolkit
  view models (`Assets/README.md`). The future engine-neutral equivalent is
  `Gens.Presentation` (ADR 0014) — do not add a second, competing adapter
  pattern; new presentation-model work belongs in `Gens.Presentation` once it
  exists, not bolted onto `Assets/Scripts/Adapters`. Neither the transitional
  Unity adapters nor the future `Gens.Presentation` layer may become a second
  place the simulation acquires a dependency on — the dependency only ever
  runs adapter/presentation → simulation, never the reverse.
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
  presentation/engine platform — Unity included, and per ADR 0014 also SDL,
  Skia, native-client code, and external AI providers; add or update tests
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
- Do not delete, degrade, or disconnect the Unity client before its
  retirement gates (ADR 0014) pass — not even partially, and not as
  "cleanup" alongside unrelated work.
- Do not add SDL, Skia, or equivalent low-level backend dependencies outside
  their designated backend projects (`Gens.Platform.Sdl`, `Gens.Graphics.Skia`,
  and equivalents) — normal application/UI/presentation code must not
  reference those types directly.
- Do not move authoritative campaign state into `Gens.Runtime`,
  `Gens.Client.Desktop`, or any other presentation/runtime layer — the
  simulation-owns-truth rule (ADR 0013) applies to the native client exactly
  as it applies to Unity today.
- Do not implement speculative engine features (3D, physics, ECS, terrain,
  navmeshes, shader graphs, visual scripting, general-purpose editor
  tooling, or arbitrary user scripting) without new, explicit architectural
  approval — every new engine capability needs a current or explicitly
  planned Gens use case (ADR 0014's scope guardrail).
- Keep PRs small and vertical: one native-runtime roadmap phase (or a
  meaningful slice of one), not a batch of unrelated layers at once.

## Connecting to a live Unity Editor

CONTRIBUTING.md's ["Connecting an AI coding agent to the Editor"](CONTRIBUTING.md#connecting-an-ai-coding-agent-to-the-editor)
section documents wiring the Unity CLI's MCP server (via `.mcp.json`) so an
agent can read the Unity console, compilation results, test runs, and scene/
asset state through a running Editor session. It only works locally.
