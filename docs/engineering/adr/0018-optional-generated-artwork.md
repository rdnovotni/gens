# ADR 0018 — Optional Generated Artwork, Provider Isolation, and Provenance

## Status

Accepted.

## Decision

AI-generated artwork is optional presentation data and defaults off. The deterministic procedural
portrait remains permanent and immediately available. Structured visible facts stay in
`Gens.Presentation`; versioned deterministic prompt compilation, providers, queues, and generated
asset storage live in `Gens.Art`. Simulation contains no provider contracts, HTTP, credentials,
queues, or encoded generated images.

Screens use the generated-portrait coordinator and `ArtGenerationQueue`, never providers directly.
Request fingerprints hash canonical semantic inputs; separately, asset hashes identify accepted
bytes. Provider output is untrusted and validated before atomic content-addressed storage. Saves may
retain lightweight references but never image bytes, and missing or corrupt assets fall back safely.

Cloud traffic uses a controlled Gens HTTPS backend with injected authentication; vendor credentials
never ship in the client. Local generation uses an isolated loopback worker boundary. Failure,
cancellation, nondeterminism, cache state, and generated references cannot affect campaign state,
replay, hashes, commands, month advancement, saves, or loading.

## Consequences

Prompt rule changes require a compiler-version bump. Historical assets may be pinned rather than
overwritten. Provider operations remain asynchronous, cancellable, capability-driven, and bounded.
