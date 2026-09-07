# Gens retained-mode UI framework

Ticket 5 adds `Gens.UI`, a backend-neutral retained presentation layer over
`Gens.Graphics`. It does not reference SDL, SkiaSharp, Unity,
`Gens.Application`, or `Gens.Simulation`; the sandbox composes it with the
runtime and concrete backends.

## Tree and lifecycle

`UiRoot` owns a persistent tree of `UiNode` objects. A node has at most one
parent; `AddChild`, `RemoveChild`, and `ClearChildren` enforce ownership and
reject cycles. Ordinary nodes are managed objects and are not disposable.
Images and fonts remain separately owned graphics resources. Tree mutation is
permitted during input callbacks, but is rejected during measure, arrange, or
paint. Exceptions propagate with scoped canvas state restoration; input
exceptions add event, target, and node-path context.

Nodes expose a developer `Name`, visibility (`Visible`, `Hidden`, or
`Collapsed`), enabled/focus/hit-test state, margin, size constraints,
alignment, opacity, clipping, z-order, bounds, desired size, invalidation
flags, and accessibility semantics. Names are diagnostic identity only and
must never become authoritative campaign identity.

## Layout and invalidation

All dimensions are logical density-independent units. `UiRoot.UiScale`
applies an additional user preference; the renderer continues to own the
logical-to-physical display scale. Pointer positions are divided by the same
UI scale before hit testing.

Layout is two phase:

1. `Measure(availableSize)` computes a cached `DesiredSize` bottom-up.
2. `Arrange(finalRect)` assigns `Bounds` and `ContentBounds` top-down.

`Margin` is outside a node and `Padding` is inside a container/control.
Alignment is Start, Center, End, or Stretch. Min/max and explicit width/height
are applied consistently by `UiNode`. `StackPanel` supports both orientations;
`Grid` supports fixed, auto, star, row/column placement, and spans; `Overlay`
stacks children deterministically. `Border`, `Spacer`, and `ScrollView` cover
the base composition needs.

Measure invalidation propagates through parent desired-size dependencies and
also invalidates arrange and paint. Arrange invalidation also invalidates
paint. Paint-only state (foreground, background, hover, focus) leaves layout
caches intact. Repeating layout with the same constraint and an unchanged tree
does not revisit the subtree. Runtime invalidation means an unchanged UI does
not render at all while idle.

## Rendering, text, and images

Painting traverses children in stable `(ZIndex, insertion order)` order and
uses only `ICanvas2D`. Every node receives a save/restore scope, and clipping
is hierarchical. `TextBlock` resolves a named typography role, shapes through
`IGraphicsBackend.ShapeText`, and reuses a cached layout keyed by text, font,
size, wrapping width, role, trimming, and line limit. The same glyph runs and
font metrics drive measurement and painting. Version 1 wraps at word
boundaries and supports a practical max-lines ellipsis; full Unicode line
breaking is deferred. `Image` draws an externally owned graphics image using
None, Contain, Cover, or Fill behavior.

## Input, focus, modals, and scrolling

Hit testing walks topmost children first, respects visibility, enabled state,
clipping, and z/insertion order, and stays in logical coordinates. Pointer
events target the deepest node and bubble through ancestors. Enter/exit comes
from hit-target changes. A pressed control may capture the pointer and receives
move/release outside its bounds; button activation still requires the final
pointer to be back over the original pressed button, so press-leave-release
cancels while press-leave-return-release activates once.

One `FocusManager` belongs to each root. It validates membership, visibility,
enabled state, and the active modal scope. Tab and Shift+Tab traverse focusable
nodes in deterministic tree order. Enter and Space activate buttons. Removing,
disabling, or collapsing the focused subtree clears focus. A modal becomes the
only hit-test/focus scope, traps Tab, and restores the previous focus when it
closes. Arrow/Page scrolling operates on a focused `ScrollView`; wheel input
bubbles from its content. Offsets clamp after wheel movement, resize, and
content size changes.

## Styling and Gens controls

`UiTheme` contains typed `ControlStyle` values, named color tokens, and
`TypographyRole` mappings. The roles are Body, Caption, SmallCaption, Heading,
Title, Inscription, Ledger, ChronicleHeading, Button, and Tooltip. `GensTheme`
defines ink, parchment, wax, gold, and muted tokens without a CSS parser,
reflection binding, markup language, or global theme singleton.

Generic controls are `TextBlock`, `Image`, `Border`, `Button`, and `Toggle`.
The first Gens primitives are `WaxTablet`, `Diptych`, `WaxSealButton`,
`CharacterMedallion`, and `InkBar`. They are presentation primitives only and
contain no campaign or simulation knowledge.

## Accessibility, diagnostics, and testing

Every node owns `UiSemantics` with role, label, description, value, and checked
state; enabled state comes from the node. This is the engine-neutral semantic
foundation, while an operating-system accessibility bridge remains future
work.

`UiDiagnostics` records node, measure, arrange, paint-node, and hit-test counts
plus layout and paint duration. `UiInspector` returns state/bounds/invalidation
snapshots and a printable hierarchy. The sandbox shows hovered-node details;
F1 toggles layout bounds and F2 prints the tree. `UiTestHost` finds nodes by
name, moves/clicks the logical pointer, presses physical keys, advances
presentation time, lays out, and renders without OS automation.

`Gens.UI.Tests` covers layout rectangles, grid spans, caching/invalidation,
hierarchical hit testing, focus, modal restoration, buttons, capture, scrolling,
text, deep trees, a dense 1,000-node tree, and deterministic repeated software
renders. Reference tests use the pinned Noto Sans fixture and a fixed surface.
Normal tests never rewrite approved images. To update a future checked-in
golden, run the named reference scene explicitly, inspect the PNG, replace the
fixture intentionally, and commit the visual change with its reason.

The Ticket 5 Windows reference run measured a 1,002-node synthetic tree at
0.443 ms for initial layout, 0.003 ms for unchanged layout, and 0.120 ms for a
steady software paint traversal. Unchanged layout performed zero measure and
arrange calls and allocated zero bytes; steady paint allocated 144 bytes total
(the fixed render-frame/root canvas scopes, not per-node garbage). Hover and
foreground/background changes invalidate paint only, while a single label
change invalidates measure through its ancestor chain. These measurements are
diagnostic observations, not rigid cross-machine thresholds.

See [UI virtualization notes](ui-virtualization-notes.md) for the deliberately
deferred large-list control and compatibility contract.

## Sandbox

```powershell
dotnet run --project tools/Gens.EngineSandbox -c Release -- --page=ui
dotnet run --project tools/Gens.EngineSandbox -c Release -- --page=ui --renderer=software
dotnet run --project tools/Gens.EngineSandbox -c Release -- --page=runtime
```

The UI gallery is the default page. It includes responsive tablets, all first
Gens primitives, wrapping text, a scroll viewport, focusable controls, a modal,
the live hover inspector, user scaling, and layout outlines. It uses fake
presentation data and is not a campaign screen.
