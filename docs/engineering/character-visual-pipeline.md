# Deterministic character visual pipeline

The guaranteed portrait path is offline, deterministic, lazy, and downstream of simulation truth:

```text
CharacterVisualProfile (Simulation)
  -> CharacterVisualState (player-visible projection)
  -> AppearanceDescription + versioned native PortraitRecipe (Presentation)
  -> ProceduralPortraitRenderer (Scene2D/Graphics)
  -> PortraitReference + content-addressed cache (PortraitService)
```

No generated image is read back into simulation state. No external AI provider, network call, wall clock, GUID, or simulation random stream participates in profile projection, recipe building, or rendering.

## Authoritative facts and projection

The existing `Gens.Simulation.Characters.CharacterVisualProfile` remains the canonical fixed appearance model: height, build, facial structure, complexion, hair color/style, eye color, notable visible features, and the legacy persisted semantic recipe. Age, sex, legal/social standing, duty, injuries, and death remain their existing authoritative character fields. Ticket 7 does not create competing character truth.

`CharacterVisualState` is a presentation snapshot with `VisualProfileVersion = 1`. It contains only player-visible fields needed by art. Its `VisualSeed` is the first 64 bits of SHA-256 over the versioned namespace and stable tagged character ID. It is therefore reproducible after save/load, isolated from PCG32 streams, and unaffected by adding layers. Age changes only at meaningful bands (infant, child, adolescent, young adult, adult, mature, elderly).

`VisualStateHash` is SHA-256 over the visual-profile version, tagged ID, sex, age band, the fixed profile fields and ordered features, legal/social status, visible duty marker, and living state. It deliberately excludes attributes, skills, money, hidden traits, relationship state, and the rest of the character record. `VisualHash.Classify` distinguishes unchanged, minor status/office changes, and major profile/age/death changes.

## Description and recipe

`AppearanceDescriptionBuilder` produces separate short, detailed, portrait-oriented, and accessibility descriptions. The builder composes discrete structured terms rather than treating English prose as canonical, leaving a clean replacement point for localization.

The native `PortraitRecipe` is renderer data rather than simulation truth. Version 1 records the visual seed, `gens.procedural.default` style, visual-state hash, and stable ordered layers (background, body, clothing, head, eyes, hair, visible details, office, foreground). Its canonical serialization is culture-invariant. A renderer-version change or recipe-version change creates a new cache identity.

## Rendering, assets, and cache

`ProceduralPortraitRenderer` version `gens.procedural.renderer.v1` validates every layer through `PortraitLayerCatalog`, constructs a Scene2D composition, renders on an offscreen surface through `Gens.Graphics`, and encodes PNG. Initial content consists entirely of original parametric vector primitives committed as code. No downloaded artwork is present; provenance is declared by the catalog.

`PortraitService` resolves on demand and caches decoded images in memory plus PNGs beneath the native application cache path. The SHA-256 request identity includes canonical recipe, visual hash, style, recipe version, renderer version, and size. A 128, 256, or 512 image therefore cannot collide, and visible changes produce new immutable entries. Invalid layer data falls back to a deterministic silhouette. `PortraitReference` is provider-neutral (`Procedural`, `Generated`, `Custom`, `Fallback`) so Ticket 8 can add approved generated/custom candidates without changing screens. `PortraitSnapshot` captures an immutable dated reference explicitly for Chronicle, succession, marriage, office, or memorial events; no monthly snapshot loop exists.

The Household Roster resolves 128 px portraits and Character Detail resolves 256 px portraits through the service. Accessibility descriptions label each medallion. Generation occurs once per request identity; subsequent navigation uses memory or disk cache and paint calls only draw the decoded image. The EngineSandbox includes a four-character deterministic gallery and an inspector readout showing seed, age band, profile-driven description, style, resolution, visual hash, recipe version, and renderer version.

On the 2026-09-07 Windows software-reference run, 128/256/512 cold render calls measured 65.856/4.766/15.041 ms (the first includes one-time warm-up effects), warm memory hits measured 0.297/0.189/0.342 ms, and disk hits measured 9.000/6.129/7.533 ms. Allocations were approximately 170 KB/547 KB/2.13 MB. Deterministic batches of 100 and 1,000 recipes averaged 16.510 and 15.523 µs per recipe. Use `benchmarks/Gens.Visual.Benchmarks` to remeasure. Automated tests lock seed/hash/description/recipe determinism, byte-identical PNG output at all three sizes, cache identity, invalidation, snapshots, transforms, render order, and animation-idle behavior.

## Ticket 8 boundary

Ticket 8 should add generated-art orchestration outside Simulation and feed approved results into the existing source-priority boundary: custom, approved generated, procedural, fallback. It should retain `CharacterVisualState`, descriptions, visual hashes, snapshots, and procedural portraits as the permanent offline guarantee. The governing decision is [ADR 0017](adr/0017-character-visual-truth-and-portrait-boundary.md).
