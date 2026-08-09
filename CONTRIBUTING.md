# Contributing to Gens

## Before you start

Open an issue for substantial behavior or design changes so scope and dependencies
can be discussed before implementation. Keep pull requests focused and avoid mixing
unrelated refactors with functional changes.

## Local setup

Install the versions identified by `global.json` and
`ProjectSettings/ProjectVersion.txt`, as well as Git LFS. Then restore and validate
the standalone solution:

```sh
dotnet restore Gens.slnx
dotnet test Gens.slnx --configuration Release
dotnet run --project tools/Gens.ContentCompiler -- \
  content/schemas/definitions.schema.json \
  content/source/catalog.json \
  artifacts/content/catalog.json
```

Open the repository root in Unity Hub when a change touches Unity assets or project
settings. Do not commit generated Unity directories such as `Library`, `Temp`, or
`Logs`.

## Repository conventions

- Keep simulation code in `src/Gens.Simulation` independent of Unity APIs.
- Add or update tests for behavior changes.
- Treat `content/source` as authored input and `content/schemas` as its contract.
- Put game-design documents in `docs/design` and engineering guidance in
  `docs/engineering`.
- Update documentation in the same pull request when commands, structure, or
  behavior change.
- Use concise, imperative commit subjects.

## Pull requests

Complete the pull-request template, call out save/content compatibility concerns,
and report the exact validation commands run. CI must pass before merge.
