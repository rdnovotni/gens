# ADR 0013 — UI Projection Boundaries

**Status:** Proposed

## Context

Non-negotiable rule 1, the first rule listed and the one every other ADR in this set ultimately serves: "The simulation owns truth. Unity displays projections and submits commands. It never owns authoritative campaign state." `tech-stack.md` already enforces half of this at the assembly level: "`Gens.Simulation` is a `netstandard2.1` library and a local Unity package with `noEngineReferences`. It must not reference Unity, presentation, or asset APIs." Roadmap Phase 2, item 10 requires the other half: "Add query/read-model interfaces so UI code cannot mutate domain objects." Phase 9, item 5 places this in the actual build sequence: "Create the Unity application shell and adapters without referencing Unity from the simulation package." The "Work explicitly deferred" section is direct about the risk of skipping this: "Do not create a large Unity UI around mutable domain objects; wait for queries/read models and the command path."

`tech-stack.md`'s UI section is also concrete about what the presentation layer actually is: "UI is UI Toolkit (UXML/USS, VectorImage and Painter2D). Scene-like cutaways use SpriteRenderer and the URP 2D Renderer." None of that changes this ADR's scope — the boundary this ADR draws is the same regardless of which UI framework sits on the far side of it.

## Decision

- **The existing one-way assembly reference is preserved and never weakened.** `Gens.Simulation` has `noEngineReferences` today per `tech-stack.md`; this ADR adds the equivalent constraint in the other direction as an explicit architectural rule, not just a build-system fact: Unity/presentation code may depend on `Gens.Simulation`'s public read-model and command-submission surface, and nothing else in it.
- **Two, and only two, sanctioned entry points from UI into the simulation:**
  1. **Query interfaces** (`IWorldQuery<TProjection>` or similarly-shaped read-only interfaces) that return **immutable projection DTOs** — plain records containing exactly the fields a given screen needs, in display-ready but presentation-agnostic form (e.g., a `HouseholdRosterRow` projection, not a `Character` domain object). Projections are built from `WorldState` truth partitions filtered through ADR 0008's `KnowledgeState` for the "player" observer — a query never returns raw truth the player-observer doesn't actually know, which is what makes this boundary also the concrete enforcement point for rule 6's knowledge/omniscience separation on the read side.
  2. **Command submission** (ADR 0006) — the *only* write path. UI never sets a field on a domain object, ever, under any circumstance, including "obviously safe" cases like a cosmetic UI-only preference; if a UI action needs to change anything the simulation is authoritative over, it constructs and submits an `ICommand` and reacts to the resulting `CommandResult`/`ValidationErrorCode`.
- **Projection DTOs are throwaway, not cached domain references.** A query result is a snapshot as of the tick it was requested; UI code never holds a live reference into `WorldState` across a tick boundary, and never mutates a returned projection expecting it to write back — projections have no setters.
- **Unity adapters are the only code permitted to translate between projection DTOs and UI Toolkit's UXML/USS-bound view models** — this adapter layer is itself simulation-unaware beyond the projection shape, keeping the actual `Gens.Simulation` package genuinely engine-free per `tech-stack.md`'s existing constraint.

## Consequences

- The four first-class screens Phase 9 names (household roster, estate/settlement, monthly report, character detail) are each backed by one or more named query interfaces, each returning a purpose-built projection type — not a generic "give me the Character object" escape hatch that would let a screen quietly start depending on internal domain shape.
- PlayMode/UI tests (Phase 9, item 9: "new campaign → assign labor → build/produce → change policy → advance month → inspect report → save/load") exercise exactly this boundary: every UI action in that flow is a command submission, every displayed result is a query — the test naturally validates the boundary is real, not just declared.
- Because commands are the only write path, ADR 0006's atomicity and RNG-free-validation guarantees extend automatically to every UI-triggered change — there is no separate "UI convenience mutation" path that could bypass them.
- Confirmation UI (Phase 9, item 7: wax-seal confirmation for consequential decisions) is purely a presentation-layer gate on *when* a command is submitted, never a reason to mutate state before or without a command.

## Alternatives Considered

- **Expose mutable domain objects directly to UI Toolkit data bindings for convenience, restrict mutation by code-review discipline alone.** Rejected outright by rule 1 and the roadmap's explicit deferred-work warning against building "a large Unity UI around mutable domain objects" — discipline-only enforcement has already proven insufficient in this codebase's own history (the roadmap's audit describes the pre-framework prototype as allowing "arbitrary mutation... before events are trusted").
- **A single generic `IReadModel` returning `dynamic`/`JsonElement` for flexibility instead of typed projection DTOs per screen.** Rejected: loses compile-time safety for UI code and makes it easy to accidentally leak a field the `KnowledgeState` filter should have excluded; a typed, purpose-built projection is exactly as much as the query author decided a screen should see, and no more.
- **Allow direct UI reads of `WorldState` (bypassing `KnowledgeState`) on the grounds that "the player already sees everything in a single-player game."** Rejected: contradicts rule 6 directly, and would need to be unwound the moment any visibility-restricted mechanic (Espionage, Secrets & Hooks, an NPC-on-NPC scheme per `gens-characters-design.md` §8.3) needs the player's UI to genuinely not show something — cheaper to route every read through the same `KnowledgeState` filter from the start than to retrofit it once a system needs it.
