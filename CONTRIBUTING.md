# Contributing to Gens

## Before you start

Open an issue for substantial behavior or design changes so scope and dependencies
can be discussed before implementation. Keep pull requests focused and avoid mixing
unrelated refactors with functional changes.

## Native runtime development

Gens ships a purpose-built native Gens runtime/client, adopted per
[ADR 0014](docs/engineering/adr/0014-custom-runtime-and-native-client.md).
The project previously shipped a Unity 6.3 LTS client during the migration
to this native runtime; Unity has since been fully retired and removed from
the repository. See [`native-runtime-architecture.md`](docs/engineering/native-runtime-architecture.md)
for dependency and lifetime rules. Windows x64 SDL is app-local; no system SDL
installation or PATH entry is required.

## Local setup

Install Git LFS and the exact .NET SDK identified by `global.json`. Then
validate the toolchain and repository:

```sh
./scripts/check-sdk.sh
git lfs install
git lfs pull
dotnet restore Gens.slnx
dotnet format Gens.slnx --no-restore --verify-no-changes
dotnet build Gens.slnx --no-restore --configuration Release
dotnet test Gens.slnx --no-restore --no-build --configuration Release
dotnet run --project tools/Gens.ContentCompiler -- validate content
dotnet run --project tools/Gens.ContentCompiler -- compile content artifacts/content/catalog.json
dotnet run --project benchmarks/Gens.Simulation.Benchmarks -- --job Dry
./scripts/verify-deterministic-build.sh
```

Exercise the native foundation separately:

```sh
dotnet run --project tools/Gens.EngineSandbox --configuration Release
dotnet run --project tools/Gens.EngineSandbox --configuration Release -- --renderer=software
dotnet run --project src/Gens.Client.Desktop --configuration Release
dotnet test tests/Gens.Client.Desktop.Tests --configuration Release
./scripts/publish-engine-sandbox.ps1
```

The content compiler also exposes `inspect`, `diff`, and the save/campaign
commands (`run-campaign`, `verify-save`, `migrate-save`, `replay`, and the
Phase 4 headless shell commands `new-campaign`, `advance`, `submit-command`,
`report`, `save`, `load`, `compare-hashes`, `inspect-state`). Run
`dotnet run --project tools/Gens.ContentCompiler -- --help` for the full list.

Do not commit generated directories such as `bin`, `obj`, or `artifacts`.

## Repository conventions

- Keep simulation code in `src/Gens.Simulation` independent of any
  presentation/engine platform.
- Add or update tests for behavior changes.
- Treat `content/source` as authored input and `content/schemas` as its contract.
- Put game-design documents in `docs/design` and engineering guidance in
  `docs/engineering`.
- When adding, removing, or renaming a file under `docs/design`, update
  `docs/design/README.md` in the same pull request.
- Update documentation in the same pull request when commands, structure, or
  behavior change.
- Use concise, imperative commit subjects.

## CI workflows

- **`standalone.yml`** runs on every pull request and on pushes to `main`. It
  has two independent jobs so a content failure is never hidden behind a test
  failure or vice versa:
  - **`standalone`** — `dotnet restore`/`format --verify-no-changes`/`build`/
    `test` in Release, a dry-run of the benchmark suite, and
    `scripts/verify-deterministic-build.sh`.
  - **`content`** — validates every content family (schema, duplicate IDs,
    references, localization), compiles the golden content pack, then runs
    the exit-gate smoke test: bootstrap a campaign, save, verify, migrate,
    and replay it, comparing state hashes throughout.

## Pull requests

Complete the pull-request template, call out save/content compatibility concerns,
and report the exact validation commands run. Protect `main` by requiring the
**Standalone validation / standalone** and **Standalone validation / content**
checks on pull requests; the independent jobs expose both test and content failures.
