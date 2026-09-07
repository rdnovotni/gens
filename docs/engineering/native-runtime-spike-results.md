# Native runtime spike results

**Desktop run:** 2026-09-07 UTC (2026-09-06 local).
**Decision:** conditional go only; **NR2 remains incomplete**.
**ADR:** [0015 — proposed](adr/0015-native-backend-selection.md).
**Evidence:** [raw logs and captures](evidence/nr2-2026-09-07/README.md).

The desktop run now proves a Windows SDL3/OpenGL/Skia path, software rendering,
text timing, SVG rendering, allocation measurement, and executable packaging.
It does **not** pass the entire required desktop completion checklist. The user
confirmed this workstation has one monitor and no IME. Linux/macOS publishes
compile, but their SDL payloads are absent. GPU resize working-set growth still
needs lifetime investigation. These gaps are not marked passed or waived.

Ticket 4 explicitly requires NR2 completion before writing production code.
No production runtime projects or engine sandbox have been introduced, ADR 0015
is not accepted, and NR3 remains unchecked. Screenshots here are **spike captures**,
not screenshots of a production sandbox.

## Environment and reproducibility

| Item | Measured/inspected desktop value |
|---|---|
| OS | Windows build 10.0.26200, x64 |
| CPU | AMD Ryzen 5 7520U with Radeon Graphics |
| GPU | AMD Radeon(TM) Graphics; driver 32.0.21039.2003 |
| OpenGL | 3.3.0 Core Profile Context 25.10.39.02.260118 |
| Physical RAM | 16,368,779,264 bytes |
| SDK | 10.0.100, matching global.json; installed locally for this run |
| Measured final executable runtime | .NET 10.0.11, framework-dependent Windows apphost |
| SDL | 3.2.22; SDL_GetVersion returned 3002022 |
| Display | One monitor; observed fullscreen 1920×1080, scale 1.00 |

The initial Linux host's inability to execute the spike is superseded by this
Windows evidence. Early .NET 10.0.0 runs and GPU runs preceding the framebuffer
fix are excluded from the tables. Raw logs selected below identify the accepted
measurement runs. Measurements are single workstation observations, not estimates
or cross-platform performance guarantees. A 4K drawable was created and checked;
this is not evidence of a physical 4K monitor or monitor-to-monitor DPI changes.

## Dependencies and boundary choice

| Dependency | Resolved version | License evidence | Role |
|---|---|---|---|
| SDL | 3.2.22 official VC archive | Bundled LICENSE.txt, zlib | Window/events/OpenGL context |
| SkiaSharp / SkiaSharp.HarfBuzz | 3.119.1 | Restored NuGet metadata, MIT | Raster/GPU graphics and shaping integration |
| HarfBuzzSharp / native assets | 8.3.1.2 | Restored NuGet metadata, MIT | Actual transitive shaping dependency |
| Svg.Skia | 3.2.1 | Restored NuGet metadata, MIT | SVG parser to cached SKPicture |

The project-owned narrow SDL C ABI remains the proposed binding choice. SDL3-CS
and ppy.SDL3-CS were alternatives discussed by the original spike, **not** bindings
benchmarked on this desktop. Keep any eventual production declarations private to
Gens.Platform.Sdl and compare them with pinned headers. Keep Skia/HarfBuzz private
to Gens.Graphics.Skia. Simulation/Application have no SDL/Skia references.

The Windows archive URL and SHA-256 are pinned in
`scripts/publish-native-spike.ps1`. Its hash is
`093821FCD2B0EAFEDC86E93713687136872A6556966DB036FEBA2672F58586ED`.
This is a recorded content pin of the downloaded upstream release, not a claim
that an independent signature was verified.

## Defects discovered and corrected before measurements

- SDL event union members must begin at offset 0, not 16. SDL3 text input holds a
  UTF-8 pointer, not an SDL2-style inline buffer. Keyboard events include `raw`
  before `down`/`repeat`; wheel events include integer deltas in SDL 3.2.22.
- Composition pointer/start/length are now preserved and logged. Header-derived
  64-bit size/offset assertions run in `--headless --self-test`.
- HarfBuzz's unsupported `new Blob(byte[])` was replaced with its stream API.
  OpenType font functions are explicitly installed.
- The procedural gradient now has explicit shader ownership.
- Linux publish lacked libHarfBuzzSharp; explicit matching native assets fix it.
- A solution build failed because Gens.Application lacked the netstandard2.1
  IsExternalInit compatibility marker. Added that marker; no campaign logic changed.
- GPU resize must preserve the SDL window framebuffer captured at context creation.
  Querying the current framebuffer after Skia readback can return an internal FBO
  and produce blank resized frames. Fixed binding/reset and target recreation.
  Captures occur before swap, outside timing, and reject blank scene readbacks.

## Implemented GPU path

SDL creates an OpenGL 3.3 core context. GRGlInterface resolves entry points through
SDL; GRContext wraps that interface; a GRBackendRenderTarget wraps the window FBO;
a GPU SKSurface renders into it. Each resize disposes the old surface/target,
rebinds the window framebuffer, resets Skia's cached GL state, and queries the
actual drawable dimensions and stencil/sample counts. Teardown disposes Skia
objects before destroying the GL context/window and quitting SDL.

`--gpu-benchmark` draws all three scenes at all four requested physical sizes.
The program rejects a requested/actual dimension mismatch. All twelve readbacks
passed the nonblank check; the dense 1080p image was visually inspected after the
resize fix. No macOS Metal or Linux display execution is claimed.

## Software and GPU performance

Both paths use 10 warm-up frames followed by 120 timed frames per scene. The
software timings measure CPU paint/flush only. GPU timings include paint, flush,
submit, glFinish, swap at interval zero, and event pumping. GPU values are thus
**not GPU timer-query measurements**, and CPU numbers exclude SDL upload/present.
Scene time advances by frame index in both paths. Screenshots are outside timing.
Managed allocation is current-thread bytes/frame; native allocations are separate.
The dense scene is the existing tablet fixture, not a production UI workload.

Average milliseconds/frame:

| Path / scene | 1280×720 | 1920×1080 | 2560×1440 | 3840×2160 |
|---|---:|---:|---:|---:|
| Software basic | 1.042 | 1.739 | 2.267 | 4.345 |
| Software dense | 0.856 | 2.114 | 2.988 | 6.356 |
| Software visual | 1.897 | 3.389 | 5.260 | 10.366 |
| GPU basic | 2.190 | 2.991 | 2.417 | 3.315 |
| GPU dense | 1.264 | 2.351 | 3.546 | 4.740 |
| GPU visual | 1.746 | 2.986 | 3.971 | 5.959 |

[Software raw data](evidence/nr2-2026-09-07/software-final.txt) contains average,
p95/p99, allocation, GC counts and working set.
[GPU raw data](evidence/nr2-2026-09-07/gpu-verified.txt) contains average, p95/p99,
allocation and working set. Earlier GPU results are rejected because the resized
framebuffer/readback bug had not yet been corrected.

These results support continuing the OpenGL experiment on Windows; they do not
justify declaring all desktop/platform/lifetime gates passed.

## Text, SVG, allocation and decode

The text corpus is exactly 3,000 whitespace-separated words: 150 repetitions of
a checked-in 20-word sentence. The generated input is retained with evidence.
This is a repeatable synthetic Chronicle workload, not 3,000 unique words.
Segoe UI at 18 px is a host font; it is not redistributed and these measurements
must not be used as portable golden text tests.

| Measurement | Actual result |
|---|---:|
| Cold word shaping (font/shaper already initialized) | 13.001 ms |
| Greedy line layout + positioned SKTextBlob construction | 12.537 ms |
| Cached paint, entire 1200×3773 paragraph surface | 5.340 ms average; 6.415 p95; 7.853 p99 |
| Cached paragraph paint managed allocation | 0 bytes/frame |
| Cached primitive/path resource paint, 1080p, 1,000 frames | 1.089 ms average; 0 bytes/frame |
| PNG actual bitmap decode, 1024×768 | 4.692 ms average; 163 managed bytes/decode |
| JPEG actual bitmap decode, 1024×768 | 5.274 ms average; 161 managed bytes/decode |

`--text-benchmark` shapes through SKShaper/HarfBuzz, lays out positioned glyphs,
and repeatedly paints the cached blob. This is a minimal word-wrap experiment,
not bidi segmentation, fallback, grapheme-aware editing, or a paragraph engine.
The separate probe reports Latin/Greek and Arabic glyphs and RTL direction; the
interactive primitive scene still uses raw DrawText. Do not promote it as a
production multilingual rendering implementation.

`--svg` loads the checked-in `Fixtures/features.svg` through Svg.Skia. Its fixture
covers paths, even-odd fill, stroke, linear gradient, transform and viewBox.
The 640×360 output was visually inspected. Pixel hash:
`BA6AC19312D00FA45CFBFC36172CEC9206F97EC2E83B72E000238246FC09272A`.
The hybrid compiled-static/constrained-runtime SVG recommendation remains:
validate trusted SVG at an asset boundary; this spike is not that validator.

## Idle and pacing

With F3 disabling animation, five seconds of no input in dirty mode consumed
**0.000 ms process CPU over 5041.624 ms wall time** at the OS counter's resolution.
F2 continuous redraw consumed **875.000 ms over 5034.001 ms**. Do not interpret the
zero sample as proof of zero power consumption or zero CPU indefinitely.

At 1280×720, a separate clear/present probe measured:

| Swap interval | Average | p95 | p99 | Managed bytes/frame |
|---|---:|---:|---:|---:|
| 0 | 1.288 ms | 3.989 ms | 6.070 ms | 0 |
| 1 | 16.671 ms | 17.812 ms | 17.922 ms | 0 |

Vsync selection succeeded. Keep idle event waits and explicit frame deadlines;
do not combine a full post-present 16.7 ms sleep with an already blocking vsync
present in production. The old software interactive loop observed roughly 50 FPS
because it sleeps after drawing; Ticket 4 must replace that pacing implementation.

## Resource soak

Both `--soak` and `--soak --gpu` completed 1,000 alternating 640×360/800×450
resizes and 10,000 create/draw/dispose iterations without an observed native crash.
Each create iteration constructs a raster surface, path and snapshot; GPU mode
also draws that snapshot through the GPU and presents every 100 iterations.
This is not 10,000 complete GPU context creations.

| Sample | Working-set bytes | Private bytes |
|---|---:|---:|
| Software first resize | 29,409,280 | 8,908,800 |
| Software resize 1,000 | 31,080,448 | 10,399,744 |
| Software create/dispose 10,000 | 42,553,344 | 21,000,192 |
| GPU first resize | 100,298,752 | 97,665,024 |
| GPU resize 1,000 | 695,865,344 | 728,604,672 |
| GPU create/dispose 10,000 | 472,920,064 | 453,042,176 |

GPU memory rises and falls substantially; the sampled resize private-memory peak
was 752,660,480 bytes. **Leak-free steady state is not established.** Working set
and private bytes include driver caches/allocator retention and do not isolate
live native resources. A longer plateau/disposal investigation with native GPU
resource accounting remains an exit concern. The modes expose real observations
instead of equating “no crash” with “no leak.”

## Screenshot determinism

The final Windows framework-dependent self-test renders the tablet twice into
independent software bitmaps and checks exact equality:

`C4C40A8F756881132440730E25A40CF39FA74030A84BFAC8148DC9F19D0BE889`.

This passes paired rendering on this configuration. Earlier .NET 10.0.0 execution
produced a different paired hash; no cross-runtime golden guarantee is made.
Production reference tests must pin runtime/native versions, font files, dimensions,
color/pixel format and scale. No host-font-dependent global golden is introduced.

## Packaging and controlled failure

All three framework-dependent publish commands completed. File inspection is
separate from publish success:

| RID | Skia | HarfBuzz | SDL | Launch |
|---|---|---|---|---|
| win-x64 | present | present | present + license, pinned script | passed |
| linux-x64 | present | present after fix | **missing** | not run |
| osx-arm64 | present | present | **missing** | not run |

`scripts/test-native-spike-packaging.ps1` correctly fails for the two missing SDL
payloads. Cross-RID compile success is not a distributable package claim. SDL Linux
build baseline and macOS dylib bundling/signing need target-platform work.

Windows publish was copied to a fresh directory and run with the repository as a
**different working directory**. Headless self-test and native window launch worked;
the resolver only considers app-local/RID-local SDL, not PATH or the working directory.
A separate copied publish excluding SDL failed with exit code **1** and exactly:

```text
Native spike failed: SDL3 native library 'SDL3.dll' was not found beside the executable or in runtimes/win-x64/native. See experiments/Gens.NativeSpike/README.md.
```

No access violation was observed. The OS does have .NET installed: this is
framework-dependent deployment evidence, not a clean machine with no runtime.

## Required desktop completion checklist

- [x] Restore and Release build; standalone tests pass (1,690 total).
- [x] Headless self-test; header-derived ABI and synthetic 100/125/150/200% DPI checks.
- [x] Software and GPU scene benchmarks at all four drawable sizes.
- [x] Paragraph shaping/layout/cached paint, allocation, decode and vsync modes.
- [x] Svg.Skia path/fill/stroke/gradient/transform/viewBox fixture and inspected capture.
- [x] Execute software/GPU resource soaks and retain samples.
- [ ] Establish acceptable GPU resize resource plateau/lifetime behavior.
- [x] Window launch, maximize/restore, minimize/restore, F11 fullscreen/back, F12 capture,
  Escape exit; observed native dimensions changed coherently at scale 1.00.
- [x] Pointer motion, click-to-focus, wheel, physical keys, modifier state and separate
  Latin text event observed through the real SDL window.
- [ ] Held-key repeat, native Arabic/Greek text entry, IME composition.
  The automation's Unicode typing used Ctrl+V; the spike has no clipboard handler.
- [ ] Monitor moves and actual 100/125/150/200% scale/input alignment. One monitor,
  no IME available, confirmed by the user. Synthetic conversions do not replace these.
- [x] Publish all three RIDs and inspect native payloads.
- [x] Fresh-directory primary Windows launch/self-test and controlled missing-SDL failure.
- [ ] Complete native SDL payloads/launch validation on Linux and macOS.
- [x] Full solution format verification with the editor LF policy aligned to .gitattributes.
- [ ] Accept ADR 0015 and mark NR2 complete **only after outstanding gate evidence**.

## Repository verification and remaining work

Commands and raw results are retained in the evidence directory. Restore/build/test,
content validate/compile, deterministic assembly verification and spike-only format
verification pass. Full solution `dotnet format --verify-no-changes` fails on existing
source formatting. The simulation benchmark entry point ignores CLI arguments;
`--job Dry` started its normal benchmark and was interrupted, not counted as a dry pass.

Ticket 4 remains blocked by its prerequisite. Do not add production projects or mark
NR3/NR3A complete from these results. Once the gate passes, implement its narrow
platform/graphics/runtime contracts and permanent sandbox, preserve software as a
first-class reference path, bundle a licensed font, and test event/lifetime/scheduling
contracts with fake backends. NR3 is not split here because no portion is complete.
