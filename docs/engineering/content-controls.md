# Content controls

**Status: implemented — [Phase 18](gens-comprehensive-build-roadmap.md).**
This document records the "content controls" item in Phase 18: an exposed
text-scale setting and colorblind-safe presentation, on top of the high
contrast and reduced motion controls documented in
[`accessibility.md`](accessibility.md).

## Intent

Give players a settings surface for presentation adjustments, alongside the
high-contrast toggle that already ships.

## Architectural placement

`UiRoot.UiScale` (see [`ui-framework.md`](ui-framework.md#layout-and-invalidation))
applies a user-preference scale multiplier during layout. It is surfaced as a
discrete step control (100%/125%/150%/175%/200%) in the Settings screen
(`GensDesktopApplication.BuildSettings`), backed by `DisplaySettings.UiScale`
in `DesktopSettings` — no additional scaling machinery was needed.

Colorblind-safe presentation extends the theme model the same way high
contrast does: `GensTheme.Create` takes a `colorblindSafe` flag and builds a
distinct `ControlStyle`/color-token set for it, selected via a
`ColorblindSafe` toggle in the Settings screen backed by
`AccessibilitySettings.ColorblindSafe`. When `highContrast` and
`colorblindSafe` are both set, `highContrast` wins — its black/white/yellow/
blue extremes are already colorblind-safe by construction, so no combined
third variant exists. Status information keeps encoding state in text/shape
as `accessibility.md` already requires ("statuses retain text, not color
alone"); no status color depends on this palette choice.

The colorblind palette itself uses the Okabe–Ito blue/orange pair (`#0072B2`
blue for `Wax`/focus rings, `#E69F00` orange for `Gold`) in place of the
default red-brown wax/gold pairing, which is a weak distinction under
red-green color-vision deficiency (the most common form). Blue/orange remains
distinguishable under protanopia, deuteranopia, and (being rarer) most
tritanopia cases too. `Ink`/`Parchment`/`MutedInk` are unchanged — they were
already neutral browns, not hue-coded.

Settings for these controls live in `DesktopSettings`/`AccessibilitySettings`
(`src/Gens.Client.Desktop/Settings/`), which per the native production-service
rules in `CLAUDE.md` must be versioned, migrated, and atomically written,
and kept separate from campaign saves — the same contract `HighContrast`
already satisfies there. `ColorblindSafe` is a new `bool` defaulting to
`false`, so existing settings files deserialize it safely without a version
bump.
