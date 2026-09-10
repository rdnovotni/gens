# Scene2D

`Gens.Scene2D` is the retained, presentation-only 2D scene layer used by the native client. It owns no campaign state and depends only on `Gens.Graphics`.

## Model and ownership

`Scene2D` owns a `Root`, `Camera`, and registered `AnimationPlayer` instances. A `SceneNode2D` has at most one parent. `AddChild` rejects cycles and reparenting without explicit removal. Children render by ascending `ZOrder`, with insertion order retained for ties; no dictionary iteration participates in output.

`Transform2D` composes pivot translation, scale, clockwise canvas rotation in degrees, and position into `Matrix3x2`. Local matrices compose through parents to form `WorldTransform`. `LocalToWorld` and `WorldToLocal` use that same matrix path. Negative scale is supported when the resulting matrix remains invertible.

The initial public nodes are `GroupNode2D`, `Layer2D`, `Sprite2D`, `AnimatedSprite2D`, `VectorNode2D`, `RectangleNode2D`, and `TextNode2D`. Sprites support source rectangles, native/stretch/contain/cover sizing, anchor, opacity, flips, and backend-neutral tinting. Vector nodes retain a graphics path rather than introducing an SVG DOM. Text nodes consume an already-shaped `GlyphRun` and do not duplicate UI text layout.

## Camera, viewport, and culling

`Camera2D` is orthographic. Its position is the world point centered in `Viewport`; zoom is clamped by `MinZoom` and `MaxZoom`. `WorldToScreen` and `ScreenToWorld` are exact inverses within floating-point tolerance. Rendering clips to the requested rectangle, applies the camera matrix, and skips bounded nodes whose transformed axis-aligned extent is outside the world viewport.

`Gens.UI.SceneView` measures like a normal retained node, clips rendering to its arranged bounds, maps pointer positions through the camera, and translates scene invalidation into the narrow UI paint invalidation path. A static scene requests no future frames. The host advances a scene only while `HasActiveAnimations` is true and mirrors that state to `RuntimeContext.SetAnimating`, allowing the event-driven runtime to return to idle.

## Animation

`AnimationClip` contains typed `AnimationTrack<T>` objects with ordered `Keyframe<T>` values. The supplied factories interpolate scalar, point, and color values with Linear, EaseIn, EaseOut, or EaseInOut curves. `AnimationPlayer` supports Once, Loop, and PingPong plus completion/state events. Timing is supplied only as presentation `TimeSpan` deltas; simulation time and RNG are not referenced. `AnimatedSprite2D` provides independent frame-duration animation.

## Proof surfaces and performance

The EngineSandbox UI gallery contains a clipped villa courtyard SceneView with layers, a vector roof, a raster sprite, shaped scene text, a camera, and a finite animation. Mouse wheel zooms the camera, arrow keys pan it, and pointer movement shows world coordinates. F2 prints both the retained UI tree and Scene2D tree plus node, visibility, sprite, rendered/cull, camera, and animation metrics from `SceneInspector`. The native Estate screen contains a non-authoritative preview built only from `EstateSettlementModel`; it never reads `WorldState` or submits placement commands.

Run the synthetic visual workload with:

```powershell
dotnet run --project benchmarks/Gens.Visual.Benchmarks -c Release
```

On the 2026-09-07 Windows software-reference run, 1080p render averages were 1.123/1.998/4.282 ms for 100/500/1,000 nodes; 4K averages were 4.036/5.245/6.587 ms. Twelve animation-track updates averaged 0.030–0.265 ms. Approximate managed allocation per combined benchmark iteration ranged from 6.6 KB to 57 KB. These are development-machine observations, not release budgets.

Current limitations: no material/shader system, arbitrary clipping nodes, particles, spatial index, or batching layer.

See [mosaic map language](mosaic-map-language.md) for a design spec that
would compose a map presentation on top of this layer; not yet implemented.
