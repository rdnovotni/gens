#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project="$repo_root/src/Gens.Simulation/Gens.Simulation.csproj"
scratch="$(mktemp -d)"
trap 'rm -rf "$scratch"' EXIT
build_and_hash() {
  local name="$1"
  dotnet build "$project" --configuration Release --no-restore \
    --output "$scratch/$name" -p:ContinuousIntegrationBuild=true >/dev/null
  sha256sum "$scratch/$name/Gens.Simulation.dll" | cut -d' ' -f1
}
first="$(build_and_hash first)"
second="$(build_and_hash second)"
if [[ "$first" != "$second" ]]; then
  echo "Deterministic build check failed: $first != $second" >&2
  exit 1
fi
echo "Deterministic assembly hash: $first"
