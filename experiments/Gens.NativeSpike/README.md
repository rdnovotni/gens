# Gens.NativeSpike

Disposable NR2 evidence harness, not a production runtime or engine sandbox.
It has no project reference to Gens.Application or Gens.Simulation.

Use the SDK pinned by global.json. For a reproducible Windows native publish:

```powershell
./scripts/publish-native-spike.ps1
./artifacts/native-spike/win-x64/Gens.NativeSpike.exe
```

The script downloads SDL 3.2.22 from the official release, checks its SHA-256,
publishes, copies the x64 DLL/license, and checks Skia/HarfBuzz payload presence.
SDL resolution is app-local/RID-local only; no system-wide SDL is required.
The publish is framework-dependent and requires .NET 10 installed.

Evidence modes (the first matching mode wins; run them separately):

```powershell
$spike = './artifacts/native-spike/win-x64/Gens.NativeSpike.exe'
& $spike --headless --self-test
& $spike --benchmark
& $spike --gpu-benchmark
& $spike --text-benchmark
& $spike --svg
& $spike --allocations
& $spike --vsync
& $spike --soak
& $spike --soak --gpu
```

`--headless --self-test` requires Skia/HarfBuzz but no SDL/display. Its ABI assertions
cover desktop 64-bit SDL 3.2.22 layouts. The software benchmark excludes SDL upload.
The GPU benchmark requires a display/GL 3.3 context and includes glFinish and swap;
it does not use GPU timer queries. Run benchmarks serially without other benchmark
processes. Both scene modes warm up 10 frames and time 120 frames per scene/size.
GPU captures are made outside timing and rejected if blank. Allocation mode measures
1,000 cached resource draws and 120 actual PNG/JPEG bitmap decodes. Text mode emits
its exact synthetic 3,000-word corpus. Soaks log process working set/private bytes,
not isolated native live-resource counts; inspect trends rather than assuming no leak.

Screenshots and corpus go to `captures/` under the working directory. Interactive
keys: `1` primitives, `2` tablet, `3` visual stress, `F1` overlay, `F2` dirty/continuous,
`F3` animation, `F11` fullscreen, `F12` capture, `Escape` exit. Click below logical
Y=620 to start the rudimentary text input; text appears in the overlay. There is no
clipboard paste or complete text control. Composition events are logged separately.

Publish/inspect all RIDs:

```powershell
dotnet publish experiments/Gens.NativeSpike -c Release -r linux-x64 --self-contained false -o artifacts/native-spike/linux-x64
dotnet publish experiments/Gens.NativeSpike -c Release -r osx-arm64 --self-contained false -o artifacts/native-spike/osx-arm64
./scripts/test-native-spike-packaging.ps1
```

The all-RID payload check currently **fails** until Linux/macOS SDL binaries are
built/bundled. Do not mistake publish success for completed native packaging.
See [measured results](../../docs/engineering/native-runtime-spike-results.md) and
[raw evidence](../../docs/engineering/evidence/nr2-2026-09-07/README.md).
NR2 is incomplete and Ticket 4 remains gated.
