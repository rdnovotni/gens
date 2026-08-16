# Gens

Gens is a Unity 6.3 LTS project backed by an engine-independent, deterministic
C# simulation. The repository's supported toolchain and architectural boundaries
are recorded in [`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md).

The project is in early development. The consolidated [game-design index](docs/design/README.md)
describes the intended systems and setting; implemented behavior is represented by
the source, content schemas, and tests.

## Prerequisites

- Unity Hub with the editor version in `ProjectSettings/ProjectVersion.txt`
- .NET 10 SDK (the expected feature band is in `global.json`)
- Git LFS

Open the repository root as the Unity project. For standalone work, run:

```sh
dotnet restore Gens.slnx
dotnet test Gens.slnx
```

## Repository layout

| Path | Purpose |
| --- | --- |
| `Assets/`, `Packages/`, `ProjectSettings/` | Unity project and package configuration |
| `src/Gens.Simulation/` | Engine-independent deterministic simulation package |
| `tests/` | Standalone automated tests |
| `benchmarks/` | Simulation performance benchmarks |
| `content/source/` | Authored content inputs — typed definition families (goods, buildings, traits, policies, events, regions, cultures, religions, names, presentation) validated against `content/schemas/` |
| `content/schemas/` | Content validation contracts (JSON Schema per definition family, plus cross-file reference/duplicate-ID checks in the content compiler) |
| `tools/` | Standalone development and content tooling, including the `Gens.ContentCompiler` CLI (`validate`, `compile`, `run-campaign`, `verify-save`, `migrate-save`, `replay`) |
| `docs/design/` | Game design, setting references, and content plans |
| `docs/engineering/` | Technical architecture, implementation policy, and the [build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md) |

The simulation package implements a partitioned deterministic `WorldState`,
phased monthly ticks with declared read/write sets, a command/event envelope
with atomic application, named and persistable PCG32 random streams, canonical
`.gens` save serialization with a migration registry, a headless campaign
bootstrap and console runner, and asynchronous artwork-provider boundaries.
On top of that foundation, characters and Familia households (lifecycle,
traits, relationships, roles), and land/goods/buildings/villas/labor with a
production network (three compact chains, storage, construction, maintenance,
and ledger-ready event emission) are implemented and covered by headless
exit-gate soak tests. See the [build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md)
for what is and is not built yet — population groups, the ledger/market, and
the player-facing Unity loop are not started.

## Documentation and contributions

Start with the [documentation map](docs/README.md). Contributions should follow
[`CONTRIBUTING.md`](CONTRIBUTING.md); pull requests are validated by the standalone
.NET test and content-compilation workflow.
