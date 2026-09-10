# Gamepad navigation

**Status: design, not yet implemented — [Phase 18](gens-comprehensive-build-roadmap.md).**
This document gives the "keyboard/gamepad navigation" item in Phase 18 a
design surface to implement against; no code referenced below exists yet.
Keyboard navigation (Tab/Shift+Tab, Enter/Space, modal trapping) already
exists and is documented in [`ui-framework.md`](ui-framework.md#input-focus-modals-and-scrolling);
this document covers only the gamepad addition.

## Intent

Let a gamepad drive the same retained UI tree that keyboard/mouse already
drive, without introducing a second navigation model or letting input-backend
types leak into `Gens.UI`.

## Architectural placement

`Gens.UI` already has one `FocusManager` per root that validates membership,
visibility, enabled state, and active modal scope, and traverses focusable
nodes in deterministic tree order for Tab/Shift+Tab. A gamepad adds a second
traversal mode on the same `FocusManager`: d-pad/stick input requests
directional movement (up/down/left/right) instead of linear next/previous,
and face-button activation maps to the same path Enter/Space already use.
Modal trapping and focus restoration behave identically regardless of input
device.

Per the native UI rules in `CLAUDE.md`, gamepad *input* (device enumeration,
raw button/axis state) is a platform/backend concern and must live in
`Gens.Platform`/`Gens.Platform.Sdl` (see [`native-client.md`](native-client.md)),
never referenced directly from `Gens.UI`. The backend translates raw gamepad
state into the same abstract input events `Gens.UI` already consumes from
keyboard/mouse, so `FocusManager` gains a directional-traversal capability
without gaining any knowledge of SDL or a specific controller API.

## Open questions

- Directional-traversal algorithm for non-linear layouts: `Diptych` and
  `Grid`-based screens need spatial (nearest-neighbor-in-direction) focus
  movement, not just tree order — the concrete algorithm is undecided.
- Controller button-glyph/prompt display (e.g. showing an Xbox/PlayStation
  glyph instead of "Enter") is not designed here.
- Whether gamepad support needs a `Gens.Platform` abstraction from day one or
  can start SDL-only (`Gens.Platform.Sdl`) and generalize later.
