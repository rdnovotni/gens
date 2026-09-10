# Geometric icon system

**Status: design, not yet implemented — [Phase 18](gens-comprehensive-build-roadmap.md).**
This document gives the "geometric icons" item in Phase 18 a design surface
to implement against; no code referenced below exists yet.

## Intent

A small, consistent set of geometric (line/shape-based, not photographic)
icons for the native UI — status glyphs, action buttons, navigation markers —
that reads at the same visual register as the existing ink/parchment/wax
theme rather than importing an unrelated icon font or raster icon set.

## Architectural placement

This extends the styling model already documented in
[`ui-framework.md`](ui-framework.md#styling-and-gens-controls): `UiTheme`
holds typed `ControlStyle` values, named color tokens, and `TypographyRole`
mappings, with `GensTheme` supplying ink/parchment/wax/gold/muted tokens
without a CSS parser or reflection binding. An icon system should follow the
same shape: callers reference a named `IconRole` (mirroring how
`TypographyRole` decouples a `TextBlock` from font specifics), and `UiTheme`
resolves that role to a concrete rendering for the active theme — including
the high-contrast variant `GensTheme.Create(highContrast: true)` already
builds for `Button`/`Panel`/`TextField`.

Painting stays inside `Gens.UI`/`Gens.Graphics`'s existing surface: an icon
renders through `ICanvas2D`, and geometry is expressed as retained vector
paths — the same choice `VectorNode2D` already made in `Gens.Scene2D` to
avoid introducing an SVG DOM. An `Icon` control would sit alongside the
existing generic controls (`TextBlock`, `Image`, `Border`, `Button`,
`Toggle`) rather than becoming a `Gens Primitive` with campaign knowledge.

## Open questions

- Full icon inventory: which actions/states actually need a distinct icon is
  undecided; this doc does not enumerate one.
- Authoring: hand-coded vector paths (consistent with `VectorNode2D`) versus
  imported vector assets (e.g. compiled SVG paths) — a pipeline choice with
  build/tooling implications not yet made.
- Whether icons need their own theme-token namespace or reuse the existing
  color-token dictionary directly.
