# Generated Art System

## Boundary and flow

```text
simulation visual facts -> presentation appearance/subject -> versioned prompt
  -> optional queue/provider -> validated content-addressed asset -> PortraitReference
```

Generated pixels never flow back into simulation. `Gens.Simulation` retains genetics, visual facts,
and procedural recipe tokens. `Gens.Presentation` turns them into an `AppearanceDescription`, a
privacy-minimized `CharacterPortraitSubject`, and deterministic visual hashes. `Gens.Art` owns all
external generation infrastructure. Procedural portraits remain available before, during, and after
every generated-art request.

## Prompt and request identity

`CharacterPortraitPromptCompiler` emits separately structured subject, period, clothing, composition,
lighting, style, quality, and exclusion fields. It uses no model or network and declares version
`portrait-prompt-v1`. Minor portraits add age-appropriate exclusions. Providers that lack native
negative prompts translate or ignore exclusions within their adapter.

`ArtRequestFingerprint` canonically joins the purpose, visual hash, style, compiler/recipe versions,
dimensions, seed, generation profile, structured prompt fields, and ordinally sorted metadata, then
computes lowercase SHA-256. `RequestInstanceId` is independent and may be random. Accepted bytes get
their own SHA-256 `AssetHash`; a cloud provider is not assumed reproducible from prompt and seed.

## Providers and security

`IArtProvider` exposes identity, optional version, capability data, and one cancellable asynchronous
operation. `NullArtProvider` is a valid production configuration. `MockArtProvider` provides
deterministic full-size PNGs, delay, failure, retry, cancellation, concurrency and nondeterminism
fixtures without a live dependency.

`LocalWorkerArtProvider` uses a loopback HTTP process boundary. The v1 contract reserves health,
version, capabilities, generate, and cancel operations; a missing/crashed worker is a request failure,
not application failure. This ticket supplies the reference client/protocol boundary, not a model or
installer. `GensBackendArtProvider` accepts HTTPS only and talks to a controlled Gens endpoint. It
caps responses at 20 MiB, accepts PNG/JPEG only, maps HTTP failures structurally, and receives tokens
from an injected callback. No vendor key or permanent client secret is stored in code or configuration.

## Queue, retries, and cancellation

`ArtGenerationQueue` is an in-memory semantic-priority queue. Identical fingerprints share one
provider operation. Effective concurrency is the lower of provider and user limits. Cancellation
flows through `CancellationToken`; pending work is intentionally not campaign state. Timeout,
network, rate-limit, unavailable, worker-crash and temporary provider failures use bounded exponential
backoff (one second base, 30 seconds maximum by default) up to the configured retry count. Rejection,
authentication, unsupported, invalid configuration and invalid output do not auto-retry.

## Cache and provenance

```text
generated/
  objects/aa/<asset-sha256>.png
  index/ff/<request-fingerprint>.json
  thumbnails/                         (reserved)
```

Writes use a sibling temporary file followed by atomic move. Metadata records request and asset
identities, provider/model provenance when known, subject and visual hash, compiler/recipe/style,
seed, requested/actual dimensions, UTC generation time, MIME type and pin state. Validation limits
are 20 MiB encoded, 4096 pixels per side and 16,777,216 decoded pixels. Cache lookup revalidates
structure, dimensions, object hash and metadata. A corrupt/missing object is a miss and never blocks
campaign load. Manual clear preserves pinned historical assets; UI must warn that unpinned historical
visuals can otherwise disappear. Thumbnails and automatic eviction are reserved for later work.

## Portrait lifecycle, consent, and staleness

Resolution order is custom, selected current generated, procedural, then fallback. `GetCurrent`
returns procedural data synchronously; completion emits `PortraitUpdated(subjectId)` so only affected
medallions/detail views need invalidate. Users explicitly enable generation, select a functioning
provider, and consent before external generation. On-demand is the default policy. Reverting selects
procedural output without deleting generated history.

Exact visual hash matches are current. A changed exact hash with unchanged major hash is `StaleMinor`;
a changed major age band or durable visual attribute is `StaleMajor`. Historical snapshots are
`Historical`, pin generated assets, and remain independent of later regeneration.

## Diagnostics and tests

`ArtDiagnostics` exposes opt-in/provider availability, queue depth/running/completed/failed/cancelled,
cache hits/misses/bytes/corruptions, stale count, and per-request status/attempt/asset/failure details.
It contains no credentials. Tests cover prompt and fingerprint determinism, deduplication, bounded
retry, permanent failure, cancellation, cache hit/corruption/missing objects, concurrency, fallback,
staleness, historical pinning, and the Simulation ownership boundary. CI uses only Mock/Null providers.
