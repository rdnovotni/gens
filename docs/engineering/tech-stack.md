# Technical baseline

## Version policy

The project is pinned to Unity 6.3 LTS by `ProjectVersion.txt`; Unity Hub must
install that exact editor. Editor changes are made only in a dedicated upgrade
pull request, which must also commit the regenerated `packages-lock.json`.
Standalone tools and tests target .NET 10 LTS. Unity uses the .NET Standard 2.1
API compatibility level, Mono for editor iteration, and IL2CPP for verified
production release builds.

## Boundaries

- `Gens.Simulation` is a `netstandard2.1` library and a local Unity package with
  `noEngineReferences`. It must not reference Unity, presentation, or asset APIs.
- Simulation outcomes use integer values and named, persisted PCG32 streams.
  Commands are validated before mutation and produce domain events. Monthly ticks
  are deterministic and target 250 ms normally and one second at maximum scale.
- UI is UI Toolkit (UXML/USS, VectorImage and Painter2D). Scene-like cutaways use
  SpriteRenderer and the URP 2D Renderer. uGUI requires a documented exception.
- Begin with ordinary managed collections. Jobs, Burst, Mathematics, and native
  collections require profiling evidence; Entities, ECS, NetCode, Havok, and a
  full DOTS architecture are excluded from the baseline.

## Data, saves, and assets

Authored JSON and CSV are inputs to the .NET content compiler. JSON Schema,
stable string IDs, uniqueness, and references are validated before normalized
runtime JSON is emitted. ScriptableObjects are presentation configuration only.

Save files use the `.gens` extension and are atomic ZIP containers with
`manifest.json`, `world.json`, optional `history.json`, generated-asset references,
an explicit version, and all deterministic RNG states. Breaking changes require a
migration and a permanent fixture. Artwork recipes and metadata are persisted,
but generated images live once in a SHA-256-addressed cache under
`Application.persistentDataPath` with separate thumbnails and a versioned manifest.

Shipped artwork and optional packs use Addressables. SVG icons have a 64x64
viewBox, stable semantic IDs, supported vector primitives, outlined text, semantic
palette tokens, and deterministic placeholders. Procedural layered SVG portraits
are the required baseline and are reproducible from `CharacterVisualProfile`,
`PortraitRecipe`, seed, and renderer version.

AI art is optional, asynchronous, and never blocks a campaign. Client code uses
`IArtGenerationProvider`, initially with null and mock providers. Production cloud
providers belong behind a controlled backend; local models run out of process.

## Verification

NUnit and FsCheck cover the standalone simulation, including golden seeds,
invariants, save round trips, and migrations. Unity Test Framework covers EditMode
and PlayMode, while UI Test Framework covers critical workflows. BenchmarkDotNet
tracks monthly ticks. CI validates content, runs both test suites and migrations,
and verifies a Unity build before merge.

