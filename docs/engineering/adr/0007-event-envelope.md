# ADR 0007 — Domain Event Envelope

**Status:** Proposed

## Context

Roadmap Phase 2, item 7: "Create a domain-event envelope with event ID, type/version, occurred date, subject IDs, visibility/provenance, causation, and payload." The roadmap names events as the load-bearing mechanism for two other major deliverables: Phase 4's "generic monthly report projection from domain events, with importance, grouping, acknowledgement state, and links to involved entities," and Phase 9's report "generated entirely from domain events and read models."

The code currently has only `interface IDomainEvent;` — an empty marker in `src/Gens.Simulation/Commands/CommandPipeline.cs` — with `CommandResult.Success(params IDomainEvent[] events)` already threading events out of the command pipeline, and `IMonthlySystem<TState>.Tick` already returning `IReadOnlyList<IDomainEvent>` from `src/Gens.Simulation/Time/MonthlySimulation.cs`. Both existing call sites already assume events flow out of every mutation; neither defines what one actually contains.

The design corpus consistently treats events as the thing every downstream system reads rather than raw state: `gens-events-design.md` owns "triggered event delivery, the Weighted Event Pool, chains, and the Monthly Report projection" per the design-authority registry cluster 18; `gens-characters-design.md` §10 states a Scheme's resolution ("succeeded," "failed quietly," "discovered-and-foiled," "discovered-and-escalated") is exactly the kind of thing "the Chronicle and Rival Houses/Events systems can pull on later," implying an event, not a raw field mutation, is what those systems actually consume. Crucially, the corpus already treats visibility as inseparable from an event's identity, not a bolt-on: `gens-labor-slavery-design.md` §6 states "every punishment action logs to the Chronicle," while §8.3's own open question in `gens-characters-design.md` asks explicitly "whether and how the player is informed of purely NPC-on-NPC outcomes (a Chronicle entry only, a rumor via Gossip, or nothing at all until it affects the player directly)" — a question that can only be answered per-event if visibility is a first-class envelope field, which is exactly what this ADR commits to ahead of that document resolving its own open question.

## Decision

`IDomainEvent` gains required members forming the envelope every concrete event type carries in addition to its own payload:

- `EventId` — `RuntimeId<DomainEvent>` (ADR 0001).
- `Type` and `SchemaVersion` — a stable string type tag plus an integer schema version, so a payload shape can evolve (ADR 0011/0012's migration and content-versioning machinery apply to event payloads exactly as they apply to save state and content).
- `OccurredDate` — `GameDate` (ADR 0003), the tick the event was produced in, not the tick it's read in.
- `SubjectIds` — an ordered (ADR 0004) list of `RuntimeId<T>` values naming every entity the event is *about*, not just the one that emitted it (a Betrayal interaction names both initiator and target; a Scheme resolution names initiator, target, and every assisting character per `gens-characters-design.md` §14's `Scheme.assistingCharacterIds`).
- `Visibility` — a provenance descriptor (ADR 0008 defines its shape in full): who, in-fiction, can know this event happened, at what confidence, and via what channel. This is not optional metadata; an event with no defined visibility cannot be constructed.
- `CausationId` — the `CommandId` or parent `EventId` that produced this event, chaining directly onto ADR 0006's `CausationId`.
- `Payload` — the event-type-specific data, itself versioned per `SchemaVersion`.

Events are immutable once emitted and are the *only* channel the Monthly Report (Phase 4/9), the Dynasty Chronicle, and knowledge propagation (ADR 0008) read from — no system reads another system's raw `WorldState` partition to reconstruct "what happened this month"; it reads the event log.

## Consequences

- The Monthly Report's "importance, grouping, acknowledgement state, and links to involved entities" (Phase 4, item 5) are all derivable purely from envelope fields (`Type` for grouping, `SubjectIds` for links) plus a report-specific importance-weighting table read from content — no report-specific shadow data model is needed.
- Visibility being mandatory on every event, not just security-sensitive ones, means Espionage/Scandal/Secrets & Hooks (design-authority registry cluster 13's pipeline) can be built later without retrofitting every existing event type to add a field that should have been there from the start.
- Event payload versioning gives every future system (Rival Houses, Dynasty Chronicle) a stable contract to read even as the systems that emit events evolve.

## Alternatives Considered

- **Events as raw state diffs (a generic "field X changed from A to B" record).** Rejected: loses semantic meaning the Monthly Report and Chronicle both need ("a Scheme was discovered and escalated" is meaningfully different from "field `status` changed"), and would make visibility/provenance impossible to express generically.
- **No mandatory `Visibility` field; treat knowledge/visibility as a separate subscription system layered on top later.** Rejected: retrofitting visibility onto every already-shipped event type is exactly the kind of "structural blocker hidden in an Open Questions section" the roadmap's Phase 1 process is meant to prevent (§1, item 5) — cheaper to require it from event zero.
- **Per-system bespoke event types with no shared envelope**, since `IDomainEvent` today is only a marker interface. Rejected: breaks the "one command path" spirit of rule 2 applied to outputs — every downstream reader (report, Chronicle, knowledge layer) would need per-producer special-casing instead of one shared contract.
