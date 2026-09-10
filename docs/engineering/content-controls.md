# Content controls

**Status: design, not yet implemented — [Phase 18](gens-comprehensive-build-roadmap.md).**
This document gives the "content controls" item in Phase 18 a design surface
to implement against. High contrast and reduced motion already exist and are
documented in [`accessibility.md`](accessibility.md); this document covers
only the additional player-facing controls Phase 18 still calls for: an
exposed text-scale setting and colorblind-safe presentation.

## Intent

Give players a settings surface for presentation adjustments that today
either exist only as internal groundwork (`UiRoot.UiScale`) or don't exist at
all (colorblind-safe palette), alongside the high-contrast toggle that
already ships.

## Architectural placement

`UiRoot.UiScale` (see [`ui-framework.md`](ui-framework.md#layout-and-invalidation))
already applies a user-preference scale multiplier during layout, and the
EngineSandbox UI gallery already exposes "user scaling" as a proof surface —
finishing this item means surfacing that same control in an actual settings
screen, not building new scaling machinery.

Colorblind-safe presentation extends the theme model the same way high
contrast already does: `GensTheme.Create` builds a distinct
`ControlStyle`/color-token set for `highContrast: true` today; a colorblind
mode would add another named theme variant selected the same way, keeping
status information encoded in text/shape as `accessibility.md` already
requires ("statuses retain text, not color alone") rather than inventing a
new status-communication rule.

Settings for these controls live in `DesktopSettings`/`AccessibilitySettings`
(`src/Gens.Client.Desktop/Settings/`), which per the native production-service
rules in `CLAUDE.md` must be versioned, migrated, and atomically written,
and kept separate from campaign saves — the same contract `HighContrast`
already satisfies there.

## Open questions

- No settings screen currently exposes any of `HighContrast`, `UiScale`, or a
  future colorblind mode to players in the native client; that screen's
  design is not covered here.
- Colorblind palette source of truth (which deficiency types to target, and
  where the palette values come from) is undecided.
- Whether text scale needs a discrete step control (e.g. Small/Normal/Large)
  or a continuous slider bound to `UiRoot.UiScale`.
