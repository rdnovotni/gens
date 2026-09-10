# Mosaic map language

**Status: design, not yet implemented — [Phase 18](gens-comprehensive-build-roadmap.md).**
This document exists to give the "mosaic map language" item in Phase 18 a
design surface to implement against; no code referenced below exists yet.

## Intent

Gens has no map screen today. This document defines the visual vocabulary a
future map-like presentation (settlement/region/world views) should use,
rather than proposing screens the game doesn't yet call for. "Mosaic" names a
tile-composited look — small geometric/tessellated tile motifs assembling
into a larger map image, distinct from a photographic or free-painted map —
consistent with the manuscript/ink aesthetic already established by
`GensTheme`'s ink/parchment/wax/gold tokens (see
[`ui-framework.md`](ui-framework.md)).

## Architectural placement

Any map view is presentation-only and renders a projection, never
`WorldState` directly (ADR 0013, ADR 0014 — the simulation-owns-truth rule
extends unchanged to the native client). It composes on top of
[`Gens.Scene2D`](scene2d.md): a map is a `Scene2D` with a `Camera2D` for
pan/zoom, hosted in the retained tree through `Gens.UI.SceneView` exactly as
the villa courtyard preview already does. Tile geometry is expected to reuse
`VectorNode2D` (retained graphics paths, no SVG DOM) and `Sprite2D` rather
than introducing a new node type, unless the open questions below determine
otherwise.

Mosaic tiles are decorative/informational presentation, so
`MotionPolicy` classification (see [`accessibility.md`](accessibility.md))
applies to any animated tile transitions the same way it already gates
`SceneView` updates.

## Open questions

- No map screen currently exists in the client — which screen(s) actually
  need this first (estate territory view? a world/region overview?) is
  undecided and should be settled before implementation, not assumed here.
- Tile authoring format: hand-authored vector paths in code (matching
  `VectorNode2D`'s current pattern) versus an imported asset pipeline.
- Whether tessellation/tile-boundary logic needs a new `Gens.Scene2D` node
  type or can compose entirely from `GroupNode2D` + `VectorNode2D`/`Sprite2D`.
- Data source: what projection(s) a map view would read from, and whether
  `IWorldQuery<TProjection>` needs a new query shaped for spatial/tile data.
