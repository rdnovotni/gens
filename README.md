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
| `content/source/` | Authored content inputs |
| `content/schemas/` | Content validation contracts |
| `tools/` | Standalone development and content tooling |
| `docs/design/` | Game design, setting references, and content plans |
| `docs/engineering/` | Technical architecture and implementation policy |

The initial simulation framework provides ordered monthly systems, named and
persistable random streams, validated command handling, save-format contracts,
and asynchronous artwork-provider boundaries. These are intentionally small
building blocks for feature systems rather than gameplay implementations.

## Documentation and contributions

Start with the [documentation map](docs/README.md). Contributions should follow
[`CONTRIBUTING.md`](CONTRIBUTING.md); pull requests are validated by the standalone
.NET test and content-compilation workflow.
