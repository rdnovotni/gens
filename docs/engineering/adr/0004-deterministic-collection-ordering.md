# ADR 0004 — Deterministic Collection Ordering

**Status:** Accepted

## Context

Non-negotiable rule 3 states this in the flattest possible terms: "Deterministic ordering is explicit. Never rely on dictionary iteration, Unity frame order, reflection discovery order, or incidental registration order." The roadmap's skeleton assessment flags the current monthly loop as exactly this failure mode: "Registration order is insufficient as a permanent dependency model." Phase 2's exit gate is the sharpest test of this ADR: "the same seed plus the same ordered commands produces identical event logs and state hashes across repeated headless runs" — any hidden iteration-order dependency (a `Dictionary<TKey,TValue>.Values` walk whose order is an implementation detail, not a contract) breaks that gate the moment .NET's internal hash bucket layout changes between versions or platforms, even with no code change at all.

The codebase already contains the pattern this ADR should generalize: `src/Gens.Simulation/Random/RandomStreamSet.cs` explicitly sorts before emitting anything observable — `Names` returns `_streams.Keys.OrderBy(name => name, StringComparer.Ordinal)`, and `CaptureStates()` likewise does `.OrderBy(pair => pair.Key, StringComparer.Ordinal)` before building the persisted dictionary. This is the one place in the current skeleton that already treats ordering as a first-class concern rather than an accident — this ADR promotes that pattern to project-wide policy rather than leaving it as one file's local discipline.

Every future `WorldState` partition (characters, plots, contracts, pop groups per ADR 0001's registries) is exactly the kind of "many entities of one kind" collection where this failure mode recurs constantly — group interaction resolution (`gens-characters-design.md` §9.8, "a Group Interaction is simply a normal Interaction resolved simultaneously against every present Character") explicitly requires resolving multiple targets "individually" in some fixed order for the outcome list itself to be reproducible, even though the document states the aggregate result doesn't depend on order — the per-target *event sequence* still must.

## Decision

- **No bare `Dictionary<TKey,TValue>` (or `HashSet<T>`) is iterated anywhere a result feeds an event, a hash, an RNG draw sequence, or a save.** Any code that needs to walk "every entity of kind X" does so through an ordered index.
- **The standard `WorldState` collection shape is an ordered index**: keyed by `RuntimeId<T>` (ADR 0001), backed by a data structure that guarantees `RuntimeId` ascending iteration order (a sorted structure, or a plain array/list plus a lookup dictionary kept in sync — implementation detail, but the *contract* is "iterates in ascending RuntimeId order," always). Because `RuntimeId<T>` is issued by a strictly increasing per-kind counter (ADR 0001), ascending-ID order is equivalent to creation order, which is itself deterministic and requires no extra bookkeeping.
- **Where iteration order must reflect something other than creation order** (e.g., Politics & Patronage's Curia vote, "run as a Group Interaction against every seated Decurion" per `gens-characters-design.md` §9.8) the explicit secondary sort key is named in that system's own design (seat number, appointment date) and applied as a stable sort *on top of* the ascending-`RuntimeId` default, never a replacement for having a defined order at all.
- **String-keyed collections** (content `DefinitionId<T>` lookups, random-stream names) sort by `StringComparer.Ordinal`, matching the existing `RandomStreamSet` precedent exactly — culture-sensitive comparers are banned in simulation code since their collation rules can change between .NET/ICU versions.

## Consequences

- Every `IMonthlySystem<TState>` (ADR 0005) that enumerates a WorldState partition gets deterministic iteration for free from the ordered-index contract, rather than needing to remember to sort locally.
- Code review and an eventual Roslyn analyzer (tracked as Phase 0/2 tooling work, not designed here) can mechanically flag a raw `foreach` over `Dictionary<,>.Values`/`Keys` inside `Gens.Simulation` as a policy violation.
- Deterministic hashing (Phase 2's state-hash exit gate) becomes a straightforward fold over each ordered index in a fixed, named partition order — no separate "canonicalize before hashing" step is needed if the live state is already canonically ordered.

## Alternatives Considered

- **Sort only at serialization time, keep runtime storage as ordinary dictionaries.** Rejected: event emission order and RNG draw order happen *during* the tick, before serialization — a save-time sort does nothing to fix a mid-tick nondeterminism that already produced a divergent event log or RNG state.
- **GUID- or hash-ordered iteration** (sort by a content hash of the entity) instead of `RuntimeId` order. Rejected: adds a hashing cost to every iteration for no benefit over the free ordering `RuntimeId`'s counter already provides, and obscures creation order, which several design-doc mechanics (seniority, "first pair a Character matches wins" in `gens-characters-design.md` §6) implicitly want to reason about.
- **A project-wide `readonly` immutable-collection convention instead of ordering policy.** Considered as a complementary discipline (immutability helps prevent accidental mutation during iteration) but doesn't by itself solve ordering — an immutable `Dictionary` is exactly as order-unstable as a mutable one; kept as a separate, non-ADR-level style guideline instead.
