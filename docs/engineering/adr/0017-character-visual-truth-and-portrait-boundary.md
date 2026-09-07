# ADR 0017 — Character Visual Truth, Portrait Recipes, and Generated Artwork Boundary

## Status

Accepted.

## Context

Gens needs recognizable character imagery without making renderer output, a
particular art provider, or presentation randomness part of campaign truth.
The native client also requires a guaranteed offline path while later work may
offer approved generated or user-selected images.

## Decision

- Structured appearance facts in `Gens.Simulation` precede every description,
  recipe, and image. They remain authoritative and engine-independent.
- `Gens.Presentation` projects only visible facts into a versioned
  `CharacterVisualState`, derives an isolated deterministic visual seed, and
  creates descriptions, visual hashes, and renderer-specific recipes.
- Portrait bytes and references are non-authoritative presentation data. They
  never enter `WorldState`, simulation RNG streams, commands, or campaign
  hashes.
- A deterministic procedural renderer is the permanent mandatory fallback.
  External generation remains optional and may not be required to play.
- Recipe, renderer, style, visual-state, and asset provenance versions are
  included in portrait identity. Historical snapshots retain immutable
  references instead of silently changing old event art.
- Source preference is custom, approved generated, procedural, then emergency
  fallback. Ticket 7 implements the latter two and preserves provider-neutral
  reference types for the former two.

## Consequences

The same save and commands produce the same visual projection and procedural
recipe without consuming simulation randomness. Visible appearance changes
invalidate cache identities; unrelated and hidden facts do not. Ticket 8 may
add generation orchestration outside Simulation without replacing the offline
pipeline or changing character truth.

Scene2D remains a separate presentation-only mechanism governed by ADR 0014;
no additional Scene2D ADR is needed for this bounded framework.
