# Gens

Gens is a Unity 6.3 LTS project backed by an engine-independent, deterministic
C# simulation. The repository's supported toolchain and architectural boundaries
are recorded in [`docs/engineering/tech-stack.md`](docs/engineering/tech-stack.md).

## Prerequisites

- Unity Hub with the editor version in `ProjectSettings/ProjectVersion.txt`
- .NET 10 SDK (the expected feature band is in `global.json`)
- Git LFS

Open the repository root as the Unity project. For standalone work, run:

```sh
dotnet restore Gens.slnx
dotnet test Gens.slnx
```

