#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Gens.Simulation, Gens.Application, and Gens.Presentation are imported into the Unity
# project as local file-path packages (Packages/manifest.json points straight at their
# src/ folders). Unity's package resolver scans that exact folder, so any bin/obj output
# left there by dotnet build/test causes duplicate generated assembly attributes and a
# CS1704 duplicate assembly import once Unity also compiles the same sources via its
# asmdef. Directory.Build.props redirects their build output under artifacts/ to prevent
# this; this script fails CI if that redirect ever regresses (see UR-01 in
# docs/engineering/unity-retirement-follow-up-tickets.md).
packages=(Gens.Simulation Gens.Application Gens.Presentation)
found=0
for package in "${packages[@]}"; do
  for leaf in bin obj; do
    path="$repo_root/src/$package/$leaf"
    if [[ -d "$path" ]]; then
      echo "Found '$path' — Unity-imported package folders must never contain build output." >&2
      found=1
    fi
  done
done

if [[ "$found" -ne 0 ]]; then
  exit 1
fi
echo "No build output found under Unity-imported package folders."
