#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
editor="${UNITY_EDITOR_PATH:-}"
if [[ -z "$editor" ]]; then
  version="$(sed -n 's/m_EditorVersion: //p' "$repo_root/ProjectSettings/ProjectVersion.txt")"
  echo "Set UNITY_EDITOR_PATH to the Unity $version executable." >&2
  exit 1
fi
"$editor" -batchmode -quit -projectPath "$repo_root" -logFile -
