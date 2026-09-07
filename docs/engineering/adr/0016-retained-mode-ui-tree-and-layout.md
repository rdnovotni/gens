# ADR 0016 — Retained-Mode UI Tree and Layout

## Status

Accepted.

## Context

The native runtime needs a document- and data-oriented UI that stays isolated
from backend libraries and authoritative simulation state. Immediate-mode
drawing in the engine sandbox cannot provide persistent focus, accessibility,
incremental layout, or inspectable control identity.

## Decision

Gens owns a retained `UiNode` tree with a cached measure/arrange layout model.
Layout uses logical units; render backends map them to physical pixels.
`Gens.UI` depends on engine-neutral `Gens.Graphics`, not SDL, SkiaSharp, Unity,
Application, or Simulation. Styling uses typed theme/control/typography values,
not CSS, reflection binding, or markup. Accessibility semantics exist on base
nodes before an OS bridge exists.

The tree supports measure, arrange, and paint invalidation; hierarchical
clipping/hit testing; bubbled pointer events and capture; one root focus manager;
Tab navigation; scroll viewports; and modal focus/input scoping. UI callbacks
may mutate the tree during event dispatch, while mutation during measure,
arrange, or paint fails explicitly.

## Consequences

Application screens can compose durable, inspectable controls without backend
types or simulation references. The project owns layout/input correctness and
must maintain headless rectangle, interaction, and reference-render tests.
Complex Unicode line breaking, native accessibility adapters, virtualization,
directional navigation, and campaign screens remain separate work.

