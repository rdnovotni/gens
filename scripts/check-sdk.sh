#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
required="$(sed -n 's/.*"version": "\([^"]*\)".*/\1/p' "$repo_root/global.json")"
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet was not found. Install SDK $required (see CONTRIBUTING.md)." >&2
  exit 1
fi
actual="$(cd "$repo_root" && dotnet --version)"
if [[ "$actual" != "$required" ]]; then
  echo "Expected .NET SDK $required, but dotnet selected $actual." >&2
  exit 1
fi
echo ".NET SDK $actual is ready."
