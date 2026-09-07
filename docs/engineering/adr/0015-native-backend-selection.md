# ADR 0015 — Native backend selection

## Status

**Proposed. Not accepted.** NR2 desktop validation is incomplete. See the
[measured results and outstanding checklist](../native-runtime-spike-results.md).
This proposal does not authorize Ticket 4 production implementation.

## Context

ADR 0014 requires a dedicated backend spike. Windows now has measured SDL3 3.2.22
and SkiaSharp 3.119.1 software/OpenGL evidence, including corrected native event
layouts and framebuffer ownership. Cross-platform packaging, actual fractional
DPI/IME validation and GPU lifetime evidence remain incomplete.

## Proposed decision

Use a narrow, pinned project-owned SDL3 ABI behind Gens.Platform.Sdl. Use
SkiaSharp behind Gens.Graphics.Skia, with Windows OpenGL 3.3 core as the measured
candidate and software Skia retained for reference/headless rendering. Use
HarfBuzz shaping and cached glyph runs. Restrict SVG parsing to a validated asset
boundary; the experiment's fixture is not a production SVG security policy.

Do not infer a macOS Metal or Linux graphics implementation from Windows results.
No alternative binding/GPU backend was performance-compared on this workstation.

## Acceptance conditions

Complete the outstanding NR2 checklist with real evidence, investigate GPU resize
memory retention, and record a supported native-package baseline. Then evaluate
and explicitly accept or revise this proposal before beginning Ticket 4.
Preserve ADR 0014's abstraction boundaries and ADR 0013's simulation boundary.
