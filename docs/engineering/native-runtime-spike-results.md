# Native runtime spike results

**Date:** 2026-09-07  
**Scope:** isolated NR2 experiment; no production runtime code  
**Evidence status:** implementation and headless evidence harness complete; execution on this
agent host was blocked because the .NET SDK, SDL3, a display server, and a GPU device are not
installed. Tables below distinguish inspected/design evidence from measurements still requiring
the desktop validation run. **NR2 must not be marked complete until those rows are populated.**

## Executive decision

The source-level spike supports a **conditional go** for SDL3 and SkiaSharp, but the repository
does not yet contain sufficient measured evidence for the unconditional production decision the
NR2 exit gate requires. Ticket 4 must not promote the experiment before a workstation runs the
published checklist and records results here.

The candidate architecture to validate is:

```text
Gens platform abstraction → small project-owned SDL3 C ABI surface → SDL3
Gens graphics abstraction → GPU-backed Skia (OpenGL first; Metal on macOS) → window
                         └→ software Skia → deterministic tests/screenshots/fallback
HarfBuzzSharp → cached shaped runs → Skia glyph drawing
SVG: compiled static icons + constrained runtime SVG for composed portraits
```

## Environment

| Item | Agent environment |
|---|---|
| OS/kernel | Linux x86-64, kernel 6.18.35 |
| CPU | 3 vCPU, Intel Xeon Platinum 8272CL at 2.60 GHz |
| GPU/display | None exposed (`lspci` reports none; no display server) |
| RAM | 17 GiB, no swap |
| .NET SDK/runtime | Not installed (`dotnet: command not found`) |

This is not representative desktop benchmark hardware. It can review source but cannot supply
window, DPI, input, GPU, publish-launch, or Skia timing evidence.

## Dependencies evaluated

The version set is intentionally stable rather than preview/EAP. Version and license metadata
must be reconfirmed from upstream/NuGet during the workstation restore; outbound package and
GitHub access was blocked by the agent environment.

| Name | Purpose/version evaluated | License/source | Native dependencies and platforms | Maintenance/need/replacement |
|---|---|---|---|---|
| SDL | Window, events, display/DPI, presentation; **3.2.22** | zlib; [libsdl-org/SDL](https://github.com/libsdl-org/SDL) | Native Windows x64 DLL, Linux x64 SO, macOS arm64 dylib; SDL supports all three | Mature, active upstream. Essential behind a narrow boundary; medium replacement cost. |
| SDL3-CS | Binding candidate A; repository state inspected conceptually against SDL 3.2 API | zlib; [flibitijibibo/SDL3-CS](https://github.com/flibitijibibo/SDL3-CS) | Requires separately supplied SDL; broad generated binding | Thin and debuggable, but consumes a much larger API and release coupling than this spike needs. Low/medium replacement cost behind an abstraction. |
| ppy.SDL3-CS | Binding candidate B | MIT; [ppy/SDL3-CS](https://github.com/ppy/SDL3-CS) | Managed wrapper/native packages target desktop RIDs | Credible and exercised by osu!, but its fork/package cadence and broad surface are another supply-chain/version coupling. Medium replacement cost. |
| Project-owned interop | Binding candidate C and implemented choice; SDL 3.2.22 ABI subset | Project code under repository license; declarations derived from [SDL headers](https://github.com/libsdl-org/SDL/tree/release-3.2.x/include/SDL3) | Exactly one official SDL binary per RID; unsafe only for fixed event text buffer | About 35 imports cover the spike. Easy native debugging and deterministic loading. Medium upkeep, low replacement cost. |
| SkiaSharp | Raster/vector/image implementation; **3.119.1** | MIT; [mono/SkiaSharp](https://github.com/mono/SkiaSharp) | `libSkiaSharp` native assets; Windows/Linux/macOS | Active .NET binding to Skia. High replacement cost after graphics contracts gain breadth, hence isolation is essential. |
| HarfBuzzSharp / SkiaSharp.HarfBuzz | Unicode shaping; **3.119.1** | MIT; [mono/SkiaSharp](https://github.com/mono/SkiaSharp) wrapping HarfBuzz (MIT) | Native HarfBuzz bundled through package graph on the three desktop targets | Necessary: raw `DrawText` is not shaping architecture. Medium replacement cost. |
| Svg.Skia | SVG parser-to-`SKPicture` candidate; **3.2.1** | MIT; [wieslawsoltes/Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) | SkiaSharp plus managed SVG parsing; desktop targets follow SkiaSharp | Maintained and convenient, but SVG feature/security/compatibility surface is large. Use only behind an asset boundary. Medium replacement cost. |

No SDL/Skia package or project reference was added to `Gens.Simulation` or `Gens.Application`.

## SDL binding decision

### Evaluation

| Criterion | SDL3-CS | ppy.SDL3-CS | Project-owned narrow ABI |
|---|---|---|---|
| Coverage | Broad/generated | Broad/fork-oriented | Only APIs Gens proves it needs |
| SDL lag | Tracks upstream tags, still a second release | Tracks ppy needs | Header update is explicit work |
| API quality | C-like names; low abstraction | C-like/generated | C-like declarations hidden in one file |
| Unsafe | Binding implementation | Binding implementation | One fixed UTF-8 event buffer |
| Loading | Generally default native resolution | Package conventions | Explicit app-local/RID-only resolver |
| Debugging | Straightforward | Straightforward, more package layers | Most direct stack and ABI mapping |
| Burden observed | Dependency plus native packaging | Dependency/package family | 35 declarations, event union, resolver |

**Recommendation:** own the narrow ABI, generated from pinned SDL headers once the production
surface grows. The implemented manual surface proves the initial burden is modest. Add an ABI
layout test (event size/offsets) and header-diff review in Ticket 4; do not let these declarations
escape `Gens.Platform.Sdl`. Switch to SDL3-CS if the production API expands enough that maintaining
the subset ceases to be small.

## Implemented evidence harness

`experiments/Gens.NativeSpike` provides:

- app-local-only SDL resolution with actionable missing-library errors;
- resizable/high-pixel-density window creation and logical/pixel metric capture;
- quit, window, physical keyboard, modifiers/repeat, mouse motion/buttons/wheel, and SDL text input;
- software BGRA Skia surface uploaded to an SDL streaming texture;
- primitive and dense Gens-tablet scenes, procedural PNG/JPEG decode, 512/1024/2048 imagery,
  transforms, nested clipping, gradients, opacity, animation, and 100-sprite stress scene;
- dirty wait-with-timeout and active 60 Hz modes;
- overlay, fullscreen toggle, screenshot capture, deterministic pixel hash self-test;
- direct HarfBuzz shaping probes for Latin/ligatures/accented Latin/Greek and Arabic;
- CSV benchmarks for three scenes at 720p, 1080p, 1440p, and 4K.

The test field is deliberately rudimentary and composition events are captured by the SDL event
surface rather than turned into a textbox framework.

## Renderer candidates

### Software Skia reference

Architecture: persistent `SKBitmap`/`SKCanvas` in physical pixel dimensions → draw with logical
scene scale → `SDL_UpdateTexture` copy → `SDL_RenderTexture`/present. Resizes dispose and recreate
bitmap, canvas, and SDL texture. PNG capture and hash operate before SDL presentation, so window
driver differences cannot affect reference pixels.

This should remain the permanent CI/reference backend if workstation results confirm exact hashes.
It is simple, inspectable, and works headlessly. Its unavoidable full-frame CPU raster plus CPU/GPU
upload makes it an unlikely production choice at 4K.

### Accelerated Skia

The viable candidate is SDL-created OpenGL 3.3+ core context → `GRGlInterface`/`GRContext` →
`GRBackendRenderTarget` wrapping the current framebuffer → GPU `SKSurface`; recreate the backend
target after drawable resize, flush/submit before `SDL_GL_SwapWindow`, and dispose Skia objects
before the GL context/window. macOS should use Metal rather than making deprecated OpenGL the
shipping contract; Windows can initially use OpenGL while a Direct3D/ANGLE decision is evaluated.

This path was **investigated but not implemented or benchmarked** here: the host exposes no GPU or
display, and adding untestable context code would create false evidence. The production v1 choice
therefore remains conditional. A 100–200 line accelerated branch on representative hardware is the
last mandatory NR2 activity; SDL_GPU is not needed to answer it.

## Text findings

The shaping probe feeds UTF-16 to HarfBuzz, calls `GuessSegmentProperties`, shapes with the actual
Skia typeface bytes, and records glyph IDs, advances, and direction. That demonstrates the required
raw capability; production rendering should segment text by script/bidi/font fallback, shape each
run once, and draw glyph IDs/positions through Skia. Cache immutable shaped runs or `SKTextBlob`s;
cache `SKPicture`s for larger static document sections. Cached surfaces are appropriate only after
profiling because they consume resolution-dependent memory.

Fallback recommendation: build rune coverage runs using the primary `SKTypeface`, query the
platform/bundled fallback family for missing code points, preserve grapheme/script clusters, then
shape each resulting run. Ship an OFL-licensed primary family plus explicit fallback families;
do not rely on copied OS fonts. The spike commits no font.

Chronicle shape/layout/paint separation exists as a benchmark requirement but has **not been timed**
on this host. Ticket 4 should use 3,000 words, record cold shaping separately from cached painting,
and retain the corpus/input alongside results.

## SVG findings

SkiaSharp is not an SVG document engine. `Svg.Skia` is the evaluated runtime parser and can turn
supported SVG documents into an `SKPicture`, but the checked-in demo intentionally uses equivalent
Skia paths until package/API execution can be verified. Recommendation: **hybrid pipeline**.

1. Validate and compile static icons at build time to a versioned, constrained vector/display-list
   representation (no scripts, external resources, remote URLs, embedded fonts, or filters).
2. Permit runtime parsing only for trusted, generated layered portraits through the same validator,
   then cache the resulting picture by content hash and renderer version.
3. Rasterize complex event art when SVG features exceed the supported subset.

This meets procedural composition needs without parsing every static asset at startup or accepting
arbitrary active SVG content.

## DPI and input findings

SDL logical window size and pixel size are queried independently; `pixel/logical` is the effective
scale per axis. Skia targets physical pixels and receives a logical-to-pixel canvas scale. SDL mouse
coordinates are logical window coordinates, so hit regions remain logical and do not receive an
extra scale. The pure conversion helper covers 1.0, 1.25, 1.5, and 2.0; a desktop must still verify
monitor moves and fractional scale changes.

Physical key/scancode and modifiers/repeat are recorded separately from `SDL_EVENT_TEXT_INPUT`.
Text is never derived from a scan code. SDL start/stop text input follows field focus. The event
union reserves SDL's documented 128-byte size; production must generate this ABI definition and
test composition/editing offsets against the pinned header.

## Performance and idle results

Run:

```sh
dotnet run -c Release --project experiments/Gens.NativeSpike -- --benchmark
```

The harness records 120 frames after warm-up, average/p95/p99 CPU paint time, approximate unpaced
FPS, current-thread allocation/frame, GC0/1/2, and process working set.

| Path | Scene | 1280×720 | 1920×1080 | 2560×1440 | 3840×2160 |
|---|---|---:|---:|---:|---:|
| Software | A basic | not run | not run | not run | not run |
| Software | B dense tablet | not run | not run | not run | not run |
| Software | C visual future | not run | not run | not run | not run |
| GPU OpenGL | A/B/C | not run | not run | not run | not run |

Idle CPU, present/vsync behavior, resource-lifetime soak, decode/upload timing, 3,000-word paragraph
timing, and managed allocations are likewise **not measured**, not guessed. Dirty mode blocks in
`SDL_WaitEventTimeout(250)` when inactive; active animation uses short waits plus a 60 Hz deadline.
This is the recommended loop topology, but the required “drastically lower CPU” observation remains
a desktop validation item.

## Screenshot determinism

`--headless --self-test` renders the identical tablet twice into separate BGRA surfaces and requires
exact SHA-256 equality, then encodes a PNG and validates shaping/DPI invariants. It was not executed
on this host. Exact hashes are expected only for a pinned OS/RID, Skia native version, font asset,
pixel format, color space, dimensions, and renderer configuration. Production goldens should bundle
the font and compare software pixel bytes; tolerances must be justified by a diagnosed difference.

## Native packaging and security

The SDL resolver considers only:

```text
<publish>/SDL3.dll                         (Windows)
<publish>/libSDL3.so.0                     (Linux)
<publish>/libSDL3.0.dylib                  (macOS)
<publish>/runtimes/<rid>/native/<same file>
```

It never mutates `PATH`, `LD_LIBRARY_PATH`, or searches the working directory/system installation.
Publish automation should download the pinned official artifact, verify a checked-in SHA-256, copy
only the correct RID binary, and retain SDL's license. Windows needs SDL3.dll; Linux needs a distro-
compatible `libSDL3.so.0` plus normal system graphics/window libraries; macOS should bundle/sign the
universal/arm64 dylib under `Contents/Frameworks` and use an app-relative install name.

Skia/HarfBuzz native assets are NuGet runtime assets. Inspect each publish RID and fail packaging if
any expected native binary is absent. A controlled missing-SDL launch should produce the resolver's
actionable `DllNotFoundException`, not an access violation.

Framework-dependent publish commands to validate are:

```sh
dotnet publish experiments/Gens.NativeSpike -c Release -r linux-x64 --self-contained false -o artifacts/native-spike/linux-x64
dotnet publish experiments/Gens.NativeSpike -c Release -r win-x64 --self-contained false -o artifacts/native-spike/win-x64
dotnet publish experiments/Gens.NativeSpike -c Release -r osx-arm64 --self-contained false -o artifacts/native-spike/osx-arm64
```

No publish could be produced here. Copy the primary RID output to a fresh directory, launch from a
different working directory, run the headless self-test, then run the window checklist. Cross-RID
compile and native packaging status remain unverified.

## Resource lifecycle

Explicitly disposable objects include `SKBitmap`, `SKCanvas`, `SKSurface`, `SKImage`, encoded data,
paint/font/typeface, shaders/paths, HarfBuzz blob/face/font/buffer, SDL texture/renderer/window, GL
surface/context, and SVG pictures. Long-lived scenes should reuse immutable fonts, paints, images,
and shaped blobs; resize should dispose framebuffer wrappers in dependency order. Ticket 4 needs a
1,000-resize and 10,000 create/draw/dispose soak with working-set/native-memory sampling.

## Risks and blockers

1. No executable or manual evidence was obtainable on this environment; this is the blocking risk.
2. Handwritten event ABI drift can corrupt memory; generate/check layouts from pinned headers.
3. OpenGL portability, macOS Metal divergence, color management, and GPU context loss need proof.
4. Font fallback/bidi/line breaking is substantially more than calling HarfBuzz once.
5. Svg.Skia supports a broad SVG surface; restrict/validate it and pin behavior with fixtures.
6. Software 4K copy bandwidth is likely too costly for continuous production rendering.
7. Linux SDL binaries need an explicit glibc/window-system compatibility baseline.
8. Current benchmark drawing creates temporary text resources; it reveals allocation pressure but
   is not the recommended cached production pattern.

## Final recommendation (conditional pending measurements)

| Question | Answer |
|---|---|
| Proceed with SDL3? | **Conditional yes**; API surface fits, desktop execution/package proof pending. |
| Proceed with SkiaSharp? | **Conditional yes**; software surface fits, GPU proof pending. |
| SDL strategy? | Small project-owned generated ABI behind `Gens.Platform.Sdl`. |
| Production v1 renderer? | GPU-backed Skia, OpenGL initially on Windows/Linux and Metal on macOS, only after benchmark proof. |
| Permanent software backend? | **Yes**, for headless CI, screenshots, debugging, and fallback. |
| Shaped text? | Bundled licensed fonts → fallback/script/bidi runs → HarfBuzz → cached glyph runs/text blobs → Skia. |
| SVG? | Hybrid compiled-static/constrained-runtime pipeline; Svg.Skia behind validation. |

## Required desktop completion checklist

- Run restore/build/tests, headless self-test, all benchmark modes, paragraph timing, allocation and
  resource soak; paste raw summary values into this document.
- Exercise close/resize/minimize/restore/maximize/F11 and monitor moves at 100/125/150/200%.
- Verify pointer hit alignment, buttons/wheel, scan codes/modifiers/repeat, UTF-8 text, Arabic/Greek,
  and IME composition on an IME-equipped system.
- Implement and benchmark the GPU-backed Skia branch at all four resolutions.
- Render an Svg.Skia fixture covering path/fill/stroke/gradient/transform/viewBox.
- Publish all three RIDs; copy and launch the primary publish in a fresh directory with no global SDL.
- Run a missing-SDL failure and confirm the exact diagnostic.
- Only then accept ADR 0015 and mark NR2 complete.

## Ticket 4 recommendation

Do not start NR3 yet. Finish the checklist above, accept a narrow ADR 0015, then create production
abstractions from the measured contracts—not by copying this disposable program. Promote the
software renderer concept and tests intentionally; rewrite the SDL ABI generation, lifetime,
input, accelerated surface, and text pipeline as production code.
