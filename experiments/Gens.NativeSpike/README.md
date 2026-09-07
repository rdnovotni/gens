# Gens.NativeSpike

Disposable NR2 platform/rendering evidence. It has no project reference to
Gens.Application or Gens.Simulation and is not a production runtime.

```sh
dotnet run -c Release --project experiments/Gens.NativeSpike -- --headless --self-test
dotnet run -c Release --project experiments/Gens.NativeSpike -- --benchmark
dotnet run -c Release --project experiments/Gens.NativeSpike
```

Interactive keys: `1` primitives, `2` tablet, `F1` overlay, `F2` dirty/
continuous, `F3` animation, `F11` borderless fullscreen, `F12` PNG capture,
and `Escape` quit. Click the text field to start SDL text input.

SDL is deliberately loaded only from the application directory (or its
RID-specific `runtimes/<rid>/native` child). Copy an official SDL 3.2.22
binary there before launch; no global-library fallback is used. See the spike
report for publish layouts and the rationale for keeping this native payload
separate from the managed experiment.
