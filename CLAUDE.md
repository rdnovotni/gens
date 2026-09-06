# CLAUDE.md

Guidance for Claude Code (and other AI coding agents) working in this repository.

## What this is

Gens is a Unity 6.3 LTS project backed by an engine-independent, deterministic
C# simulation (`src/Gens.Simulation`, targeting `netstandard2.1`, `.NET 10`
tooling per `global.json`). The toolchain and architectural boundaries are
recorded in [`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md).
Current build status is tracked as a phase checklist in the
[build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md) — check
it rather than assuming from README prose, since it's the document updated as
each phase lands.

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

- **Simulation stays Unity-free.** `src/Gens.Simulation` must never reference
  Unity, presentation, or asset APIs (`tech-stack.md`, ADR 0013).
- **UI touches simulation only through query projections and commands.**
  Unity/UI code reads via `IWorldQuery<TProjection>` implementations under
  `src/Gens.Simulation/Queries/` and writes only by submitting an `ICommand`
  — never by setting a field on a domain object directly (ADR 0013).
  `Assets/Scripts/Adapters` is the only code permitted to translate between
  projection DTOs and UI Toolkit view models (`Assets/README.md`).
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
- Keep simulation code in `src/Gens.Simulation` independent of Unity APIs;
  add or update tests for behavior changes.

## Connecting to a live Unity Editor

CONTRIBUTING.md's ["Connecting an AI coding agent to the Editor"](CONTRIBUTING.md#connecting-an-ai-coding-agent-to-the-editor)
section documents wiring the Unity CLI's MCP server (via `.mcp.json`) so an
agent can read the Unity console, compilation results, test runs, and scene/
asset state through a running Editor session. It only works locally.
