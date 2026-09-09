# Gens

Gens is built around an engine-independent, deterministic C# simulation. It
has a playable, purpose-built native Gens runtime and desktop client
([ADR 0014](docs/engineering/adr/0014-custom-runtime-and-native-client.md)
records the architecture decision). The project previously shipped a Unity
6.3 LTS client; Unity has since been fully retired (see the
[native-runtime migration roadmap](docs/engineering/gens-native-runtime-roadmap.md)
for how that migration proceeded). The deterministic simulation itself is
unaffected by this history — it remains engine-independent regardless of
which client presents it. The repository's supported toolchain and
architectural boundaries are recorded in
[`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md).

The project is in early development. The consolidated [game-design index](docs/design/README.md)
describes the intended systems and setting; implemented behavior is represented by
the source, content schemas, and tests. For a player-facing read of what the game
actually is and how it plays, see the [player manual](docs/manual/README.md).

## Prerequisites

The native desktop client is the primary and only target for presentation
development. See the [native client guide](docs/engineering/native-client.md).

- .NET 10 SDK (the expected feature band is in `global.json`)
- Git LFS

```sh
dotnet restore Gens.slnx
dotnet test Gens.slnx
dotnet run --project src/Gens.Client.Desktop --configuration Release
```

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/Gens.Simulation/` | Engine-independent deterministic simulation package |
| `src/Gens.Application/` | Engine-neutral campaign lifecycle, query/command, save/load, and replay host |
| `src/Gens.Platform/`, `src/Gens.Platform.Sdl/` | Engine-neutral desktop contracts and the isolated SDL3 backend |
| `src/Gens.Graphics/`, `src/Gens.Graphics.Skia/` | Backend-neutral 2D graphics contracts and Skia software/OpenGL implementation |
| `src/Gens.Scene2D/` | Lightweight retained 2D scenes, transforms, cameras, layers, sprites, vectors, text, culling, and animation |
| `src/Gens.Assets/`, `src/Gens.Portraits/` | Stable/content-addressed assets and deterministic offline procedural portrait rendering |
| `src/Gens.Runtime/` | Native application loop, presentation clock, invalidation scheduling, lifecycle, and diagnostics |
| `src/Gens.UI/` | Backend-neutral retained UI tree, layout, controls, input/focus, themes, accessibility semantics, and Gens design primitives |
| `src/Gens.Presentation/` | Engine-neutral snapshot mapping shared by native clients |
| `src/Gens.Client.Desktop/` | Playable native desktop client and SDL/Skia composition root |
| `tests/Gens.Client.Desktop.Tests/` | Native application-flow, lifecycle, save/load, determinism, and architecture tests |
| `tests/Gens.Application.Tests/` | Standalone integration tests for the shared campaign session |
| `tests/` | Standalone automated tests |
| `benchmarks/` | Simulation and native visual performance benchmarks |
| `content/source/` | Authored content inputs — typed definition families (goods, buildings, traits, policies, events, regions, cultures, religions, names, presentation) validated against `content/schemas/` |
| `content/schemas/` | Content validation contracts (JSON Schema per definition family, plus cross-file reference/duplicate-ID checks in the content compiler) |
| `tools/` | Standalone tooling, including `Gens.ContentCompiler` and the production-abstraction `Gens.EngineSandbox` native executable |
| `docs/design/` | Game design, setting references, and content plans |
| `docs/engineering/` | Technical architecture, implementation policy, and the [build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md) |

The simulation package implements a partitioned deterministic `WorldState`,
phased monthly ticks with declared read/write sets, a command/event envelope
with atomic application, named and persistable PCG32 random streams, canonical
`.gens` save serialization with a migration registry, a headless campaign
bootstrap and console runner, and asynchronous artwork-provider boundaries.
On top of that foundation, characters and Familia households; land, goods,
buildings, villas, and labor with a production network; background
population groups and employment; the household ledger, market, and debt/
contract system; the action/standing-policy layer and event/report
projections; an application shell and adapters exposing a playable
loop (household roster, estate/settlement, monthly report, and character
detail screens, with confirmations, pause/advance, save/load, and
deterministic-replay diagnostics); autonomous rival houses and steward/
Council delegation; and dynasty continuity (succession, Regency, the
Chronicle) are all implemented and covered by headless exit-gate soak
tests. See the [build roadmap](docs/engineering/gens-comprehensive-build-roadmap.md)
for the authoritative, continually-updated phase checklist of what is and
is not built yet.

## Documentation and contributions

Start with the [documentation map](docs/README.md). Contributions should follow
[`CONTRIBUTING.md`](CONTRIBUTING.md); pull requests are validated by the standalone
.NET test and content-compilation workflow.
