#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
status="$(git -C "$repo_root" status --porcelain --untracked-files=all)"
if [[ -n "$status" ]]; then
  echo "Validation changed tracked or untracked files:" >&2
  echo "$status" >&2
  exit 1
fi
echo "Worktree remains clean."
