#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Historical: while Unity was still a supported client, Gens.Simulation, Gens.Application,
# and Gens.Presentation were also imported into the Unity project as local file-path
# packages (Packages/manifest.json pointed straight at their src/ folders). Unity's package
# resolver scanned that exact folder, so any bin/obj output left there by dotnet build/test
# caused duplicate generated assembly attributes and a CS1704 duplicate assembly import once
# Unity also compiled the same sources via its asmdef. Directory.Build.props redirects their
# build output under artifacts/ to prevent this; Unity has since been retired, but this
# script is kept as a general build-output hygiene check.
packages=(Gens.Simulation Gens.Application Gens.Presentation)
found=0
for package in "${packages[@]}"; do
  for leaf in bin obj; do
    path="$repo_root/src/$package/$leaf"
    if [[ -d "$path" ]]; then
      echo "Found '$path' — these package folders must never contain build output." >&2
      found=1
    fi
  done
done

if [[ "$found" -ne 0 ]]; then
  exit 1
fi
echo "No build output found under the redirected package folders."
