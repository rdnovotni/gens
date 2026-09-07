# Gens — Native Runtime Migration Roadmap

**Scope:** tracks the engine/client migration adopted by
[ADR 0014](adr/0014-custom-runtime-and-native-client.md) — moving Gens's
presentation/runtime platform from Unity to a purpose-built native Gens
runtime and desktop client. `NR` stands for *Native Runtime* and is numbered
independently of the [comprehensive build roadmap](gens-comprehensive-build-roadmap.md)'s
gameplay phases (0–15+) so the two checklists never collide.

This document is deliberately short. It does not re-derive game-system
design; the comprehensive roadmap and `docs/design/` remain authoritative for
gameplay. This document only tracks presentation/runtime platform migration.

Nothing in this roadmap changes `Gens.Simulation` behavior, save format,
content schemas, or any already-shipped gameplay system. Unity remains the
working, supported client until the ADR 0014 retirement gates pass — see
that ADR for the full 17-gate list before treating any later phase here as
"Unity can be removed."

## Phase checklist

- [x] **Phase NR0** — Architecture contract
- [ ] **Phase NR1** — Extract `Gens.Application` / `CampaignSession`
- [ ] **Phase NR2** — SDL3 + Skia rendering/platform spike
- [ ] **Phase NR3** — Runtime and custom UI foundation
- [ ] **Phase NR4** — Native-client vertical-slice parity
- [ ] **Phase NR5** — Scene2D and procedural portrait pipeline
- [ ] **Phase NR6** — AI art, audio, accessibility, packaging
- [ ] **Phase NR7** — Cross-platform hardening and Unity retirement

---

### Phase NR0 — Architecture contract — ✅ COMPLETE

**Outcome:** the target architecture, dependency boundaries, backend
direction, and Unity retirement gates are recorded as an accepted ADR before
any implementation begins, so later phases have a fixed contract to build
against instead of re-litigating platform choice per ticket.

**Deliverables:**

- [ADR 0014](adr/0014-custom-runtime-and-native-client.md), covering target
  layering, dependency direction, forbidden dependencies, initial backend
  direction (SDL3 + SkiaSharp), 2D support scope, the AI/generated-art
  authority boundary, dual-client migration policy, and the Unity retirement
  gates.
- This roadmap document.
- Updated [ADR index](adr/README.md), [`tech-stack.md`](tech-stack.md),
  [`gens-comprehensive-build-roadmap.md`](gens-comprehensive-build-roadmap.md),
  `CLAUDE.md`, and `README.md` cross-links.

**Prerequisites:** none.

**Exit gate:** ADR 0014 is Accepted and indexed; no code, project, or
dependency changes exist yet; the standalone build/test/content/replay
verification suite is unaffected and green.

**Non-goals:** no `Gens.Application`, `Gens.Runtime`, `Gens.Graphics`,
`Gens.UI`, `Gens.Scene2D`, or `Gens.Client.Desktop` project; no SDL, Skia, or
HarfBuzz dependency; no change to `Gens.Simulation`, saves, or content.

---

### Phase NR1 — Extract `Gens.Application` / `CampaignSession`

**Outcome:** an engine-neutral application/orchestration layer exists between
`Gens.Simulation` and any presentation host, so the eventual native client and
the existing Unity client can both consume the same campaign lifecycle,
query gateway, and command gateway without either owning it directly.

**Major deliverables:**

- `Gens.Application` project (`CampaignSession`, campaign bootstrap, query
  gateway, command gateway, month advancement, save/load orchestration,
  replay verification, session lifecycle) per ADR 0014's layering.
- The existing Unity shell/adapters repointed to call through
  `Gens.Application` instead of duplicating orchestration logic, without
  changing observable Unity behavior.

**Prerequisites:** Phase NR0.

**Exit gate:** the Unity client builds and plays identically through
`Gens.Application`; headless campaign/replay tooling either moves onto the
same layer or is confirmed compatible with it; standalone verification stays
green.

**Non-goals:** no `Gens.Runtime`, `Gens.UI`, or native client yet; no SDL/Skia
dependency; no change to `Gens.Simulation`'s public contract beyond what
`Gens.Application` needs to wrap it.

---

### Phase NR2 — SDL3 + Skia rendering/platform spike

**Outcome:** a throwaway-quality technical spike validates (or disqualifies)
SDL3 and SkiaSharp as the initial platform/rendering backends before any
production commitment, per ADR 0014's "subject to a dedicated technical
spike" qualifier.

**Major deliverables:**

- A minimal, non-production `Gens.Platform` + `Gens.Platform.Sdl` and
  `Gens.Graphics` + `Gens.Graphics.Skia` proof of concept: open a window,
  draw text and shapes, take input, on at least the project's primary
  development OS.
- A written go/no-go findings note (performance, packaging, licensing,
  platform coverage) that either confirms SDL3 + SkiaSharp or names a
  replacement backend pair, without changing ADR 0014's architectural
  commitments (platform/renderer abstraction stays; only the backend behind
  it is at stake).

**Prerequisites:** Phase NR0. (Independent of NR1; may run in parallel.)

**Exit gate:** a documented go/no-go decision exists and is recorded (as an
ADR amendment or a new ADR, per the project's migration-discipline
convention) before NR3 begins production implementation against the chosen
backend.

**Non-goals:** no `Gens.UI`, `Gens.Scene2D`, or `Gens.Client.Desktop`; the
spike code is not expected to be production-quality or kept as-is.

---

### Phase NR3 — Runtime and custom UI foundation

**Outcome:** the minimum `Gens.Runtime` + `Gens.UI` foundation needed to
render a real (not spike) screen exists: application loop, navigation,
layout, text, focus, input, and enough styling to build a first real screen.

**Major deliverables:**

- `Gens.Runtime` (application loop, navigation, modal lifecycle, service
  lifetime, presentation clock, diagnostics scaffold).
- `Gens.UI` (layout, text rendering via the chosen backend, focus, input,
  scrolling, basic styling).
- `Gens.Presentation` (engine-neutral presentation models consumed by the
  first UI screen).

**Prerequisites:** Phases NR1 and NR2.

**Exit gate:** one real, non-trivial screen (e.g. main menu, or Household
Roster) renders and responds to input entirely through `Gens.Runtime`/
`Gens.UI`/`Gens.Presentation`, reading only through `Gens.Application`
queries — no direct simulation reference, matching ADR 0013's boundary.

**Non-goals:** no `Gens.Scene2D`; no full vertical-slice parity yet; no AI
art, audio, or packaging.

---

### Phase NR4 — Native-client vertical-slice parity

**Outcome:** the native client reaches feature parity with the existing
Unity vertical slice for the core playable loop.

**Major deliverables:**

- `Gens.Client.Desktop` composition root and executable.
- Native implementations of: main menu/new-game/settings, Household Roster,
  Character Detail, Estate/Settlement, Monthly Report, confirmations, and
  month advancement — mirroring ADR 0014 retirement gates 6–13.
- Save/load through the native client against existing `.gens` saves.

**Prerequisites:** Phase NR3.

**Exit gate:** ADR 0014 retirement gates 1–13 pass for the native client.

**Non-goals:** dev/debug tooling parity (NR6), AI art/audio/accessibility/
packaging (NR6), Scene2D-based visualization (NR5), and Unity removal (NR7)
are explicitly out of scope here.

---

### Phase NR5 — Scene2D and procedural portrait pipeline

**Outcome:** the native runtime gains the 2D scene capability ADR 0014
anticipates, starting with procedural portraits as the first concrete
consumer.

**Major deliverables:**

- `Gens.Scene2D` (`Scene2D`, `SceneNode2D`, `Transform2D`, `Sprite2D`,
  `Camera2D`, layers, animation), explicitly non-authoritative per ADR 0014.
- `Gens.Assets` (stable asset identity, manifests, loading, caching).
- The existing procedural-portrait recipe pipeline rendered through
  `Gens.Scene2D`/`Gens.Graphics` instead of Unity's `SpriteRenderer`/URP 2D
  path, with no change to `CharacterVisualProfile`/`PortraitRecipe` inputs.

**Prerequisites:** Phase NR4.

**Exit gate:** procedural portraits render identically (same recipe, same
seed, same renderer-version contract) through the native client as they do
in Unity today.

**Non-goals:** AI-generated art integration (NR6); illustrated events/estate
visualization beyond portraits; particle/weather effects.

---

### Phase NR6 — AI art, audio, accessibility, packaging

**Outcome:** the native client closes the remaining gaps needed for it to
be a real production release candidate rather than a development-only build.

**Major deliverables:**

- `Gens.Art` orchestration for the existing `IArtGenerationProvider` seam,
  preserving the ADR 0014 art/AI authority boundary (optional, async,
  cancelable, cacheable, non-blocking, non-authoritative).
- `Gens.Audio` abstraction/mixing layer.
- Accessibility semantics in `Gens.UI`.
- Dev/debug console parity (ADR 0014 gate 14).
- Automated native-client tests (gate 15).
- A packaged native build runnable outside a development environment
  (gate 16).

**Prerequisites:** Phase NR4; NR5 for any art-pipeline-dependent work.

**Exit gate:** ADR 0014 retirement gates 14–16 pass.

**Non-goals:** cross-platform coverage beyond the primary target OS (NR7);
Unity removal (NR7).

---

### Phase NR7 — Cross-platform hardening and Unity retirement

**Outcome:** the native client is production-ready across the project's
target platforms and Unity is retired per ADR 0014's gates — the final phase,
not a formality.

**Major deliverables:**

- Cross-platform validation of the SDL/Skia backend choice (or whatever
  backend NR2 settled on) across all shipping target operating systems.
- Confirmation of ADR 0014 gate 17 (no required production feature still
  depends on Unity).
- The dedicated Unity-removal change: delete `Assets/`, `Packages/`,
  `ProjectSettings/`, Unity-specific CI/tooling, and Unity-only adapters —
  performed only after all 17 gates pass, as its own change, not bundled
  with feature work.
- A follow-up decision (tracked separately, per ADR 0014) on retargeting
  `Gens.Simulation` off `netstandard2.1` now that Unity no longer consumes it.

**Prerequisites:** Phases NR1–NR6, and all 17 ADR 0014 retirement gates
passing.

**Exit gate:** Unity is removed from the repository; the native client is the
sole supported client; standalone verification remains green throughout.

**Non-goals:** none — this is the terminal phase of the migration roadmap.
