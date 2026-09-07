# Native runtime architecture

Ticket 4 introduces the first production-shaped native runtime foundation. It was
implemented under an explicit prerequisite override while NR2 and ADR 0015 remain
open; this document describes the code that exists, not acceptance of the remaining
cross-platform, IME, DPI-monitor, or GPU-lifetime gates.

## Dependency boundaries

```text
Gens.Platform.Sdl  -> Gens.Platform
Gens.Graphics.Skia -> Gens.Graphics -> Gens.Platform
Gens.Runtime       -> Gens.Platform + Gens.Graphics
Gens.EngineSandbox -> Gens.Runtime + both backend projects
```

`Gens.Runtime` cannot see either concrete backend. `Gens.Simulation` and
`Gens.Application` gained no platform or graphics dependency. SDL declarations are
private to `Gens.Platform.Sdl`; SkiaSharp and HarfBuzzSharp types are private to
`Gens.Graphics.Skia`.

## Platform model

`IPlatform` owns platform initialization, event pumping/waiting, clipboard access,
the monotonic clock, and window creation. `IWindow` explicitly separates
`LogicalSize`, `PixelSize`, and `DisplayScale`; pointer events use SDL's logical
window coordinates. Keyboard events carry a physical scan code and modifiers, while
committed text and IME composition are independent events.
Composition selection indices currently preserve SDL's UTF-8 byte offsets; converting
them to a higher-level text model is deferred to the retained-mode text-control work.

The SDL backend uses the narrow project-owned SDL 3.2.22 C ABI validated by NR2.
The only unsafe code is the backend's pixel-upload/interop boundary. A resolver
loads SDL only from the application directory or its RID-specific `runtimes`
directory and reports a controlled error otherwise; it never searches PATH. The
checked-in Windows x64 payload and zlib license are copied to build and publish
outputs. Linux/macOS SDL payloads remain an explicit NR2 limitation.

## Graphics model

`IGraphicsBackend` creates window/offscreen surfaces, paths, decoded images, fonts,
and shaped glyph runs. `ICanvas2D` supports scoped save/restore, transforms,
clipping, fills/strokes, paths, raster images, and glyph runs without exposing Skia
types. API colors are unpremultiplied byte RGBA; the backend owns conversion to
Skia's premultiplied storage.

The default/production surface is the measured OpenGL 3.3 core route:

```text
SDL window/context -> GRGlInterface -> GRContext -> window framebuffer SKSurface
```

Resize destroys the old Skia surface/target, rebinds the framebuffer captured at
context creation, resets Skia GL state, and recreates the target at the new pixel
size. The software surface is first-class for window presentation, offscreen tests,
screenshots, CI, and fallback selected explicitly with `--renderer=software`.
HarfBuzz shapes bundled-font text into backend-neutral positioned glyphs. PNG/JPEG
decoding is intentionally backend-local; the final asset registry is out of scope.

## Runtime scheduling and ownership

`RuntimeHost` is the root owner. It initializes an `IRuntimeApplication`, orders
event dispatch, updates the monotonic `PresentationClock`, resizes before delivering
resize-related events, and renders only when invalidated or animating. Idle hosts
wait for events rather than spinning. `RuntimeContext.Invalidate()` requests one
frame; `SetAnimating(true)` schedules presentation updates/frames until disabled.
Presentation time is never authoritative simulation time.

Ownership is strictly nested:

```text
RuntimeHost -> application -> render frame
            -> render surface -> Skia objects -> GL context
            -> window -> SDL platform lifetime
```

Frames and canvas state use `IDisposable` scopes. Shutdown calls the application,
then disposes surface, window, graphics backend, and platform, even when application
shutdown reports an error. Platform/window/GL resources assert main-thread use.

`RuntimeDiagnostics` reports backend identity, selected renderer, frame/event/wait
counts, dirty/animation state, and last frame duration. F12 in the sandbox performs
backend-independent surface readback and PNG encoding.

## Sandbox and commands

The sandbox uses only production contracts and demonstrates drawing, clipping,
transforms, generated raster-image decode/draw, bundled-font shaped text, input,
resize/DPI metrics, a finite animation that returns to idle waiting, fullscreen,
and screenshot capture.

Its default `--page=ui` gallery exercises the retained UI tree, responsive
Diptych/WaxTablet layout, Gens controls, wrapping text, scrolling, Tab focus,
pointer capture, a focus-trapping modal, the live hovered-node inspector, UI
scaling, and layout outlines using fake presentation data. Use `--page=runtime`
for the original low-level canvas/backend page.

```powershell
dotnet run --project tools/Gens.EngineSandbox -c Release
dotnet run --project tools/Gens.EngineSandbox -c Release -- --page=ui
dotnet run --project tools/Gens.EngineSandbox -c Release -- --page=runtime
dotnet run --project tools/Gens.EngineSandbox -c Release -- --renderer=software
dotnet run --project tools/Gens.EngineSandbox -c Release -- --renderer=gpu
dotnet run --project tools/Gens.EngineSandbox -c Release -- --renderer=software --smoke-test
dotnet run --project tools/Gens.EngineSandbox -c Release -- --reference-benchmark
./scripts/publish-engine-sandbox.ps1
```

Controls: F3 restarts/toggles animation, F11 toggles fullscreen, F12 writes a PNG,
and Escape exits. The bundled Noto Sans asset uses the SIL Open Font License.

## Local validation snapshot

The 2026-09-07 UTC Windows validation ran both renderer smoke modes, verified the
published payload, and completed 30 consecutive published GPU launch/shape/capture/
shutdown runs after moving shaping onto `SkiaSharp.HarfBuzz`. Once the sandbox's
four-second animation settled, measured process CPU time was 0 ms over a three-second
idle interval and the window closed cleanly through its normal close event.

The production-abstraction reference workload at 1920×1080 measured 2.523 ms average,
3.338 ms p95, 3.855 ms p99, and 64 managed bytes/frame on the NR2 workstation. It is
a new workload, so it is not a like-for-like benchmark; it falls between NR2's dense
(2.114 ms) and visual (3.389 ms) software fixtures at that size. Eliminating the
remaining render-frame lease allocation is a later optimization, not a correctness
dependency.
