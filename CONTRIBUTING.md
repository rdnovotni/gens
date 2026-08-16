# Contributing to Gens

## Before you start

Open an issue for substantial behavior or design changes so scope and dependencies
can be discussed before implementation. Keep pull requests focused and avoid mixing
unrelated refactors with functional changes.

## Local setup

Install Git LFS, the exact .NET SDK identified by `global.json`, and the exact
Unity editor identified by `ProjectSettings/ProjectVersion.txt` (including Linux
Build Support when working on Linux). Then validate the toolchain and repository:

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

The content compiler also exposes `inspect`, `diff`, and the save/campaign
commands (`run-campaign`, `verify-save`, `migrate-save`, `replay`, and the
Phase 4 headless shell commands `new-campaign`, `advance`, `submit-command`,
`report`, `save`, `load`, `compare-hashes`, `inspect-state`). Run
`dotnet run --project tools/Gens.ContentCompiler -- --help` for the full list.

For Unity changes, open the root in Unity Hub or run the assembly compilation smoke
check with an already activated editor:

```sh
UNITY_EDITOR_PATH=/absolute/path/to/Unity ./scripts/unity-smoke.sh
```

Licensed Unity tests and builds are deferred until CI credential and runner policy
is approved. Do not commit generated directories such as `Library`, `Temp`, or
`Logs`.

## Repository conventions

- Keep simulation code in `src/Gens.Simulation` independent of Unity APIs.
- Add or update tests for behavior changes.
- Treat `content/source` as authored input and `content/schemas` as its contract.
- Put game-design documents in `docs/design` and engineering guidance in
  `docs/engineering`.
- When adding, removing, or renaming a file under `docs/design`, update
  `docs/design/README.md` in the same pull request.
- Update documentation in the same pull request when commands, structure, or
  behavior change.
- Use concise, imperative commit subjects.

## Pull requests

Complete the pull-request template, call out save/content compatibility concerns,
and report the exact validation commands run. Protect `main` by requiring the
**Standalone validation / standalone** and **Standalone validation / content**
checks on pull requests; the independent jobs expose both test and content failures.
