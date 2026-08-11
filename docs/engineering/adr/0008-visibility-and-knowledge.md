# ADR 0008 — Visibility and Knowledge Separation

**Status:** Proposed

## Context

Non-negotiable rule 6: "Knowledge is not omniscience. The simulation stores truth separately from what the player, a character, or an actor knows. Dossiers, rumors, letters, espionage, and reports read a knowledge/provenance layer." Roadmap Phase 1's own primary design inputs list confirms this is expected to be resolvable now, from the corpus already in hand.

The design corpus already draws this line in several places without ever building the shared mechanism rule 6 asks for, which is exactly what makes it ready to formalize rather than invent from nothing:

- `gens-labor-slavery-design.md` §3 states a slave's "full stat block is never fully visible at the point of acquisition," splitting fields explicitly into "visible at a glance" (approximate age, a health *range*, origin) versus "hidden or uncertain until owned" (exact Core Attributes/Labor Skills shown as "a rough band," most personality traits, unobvious permanent injury) — a field-level visibility split the field ledger (this Phase 1 artifact's companion document) must encode per field, not just per record.
- `gens-characters-design.md` §9.7's Informational interaction category — "Share/Withhold Information," "Reveal a Secret," "Gather Intelligence" — and §10's Scheme "Discovery risk" mechanic both presuppose that what a target *knows* about an active scheme is a distinct, trackable quantity from what is objectively true and in progress.
- The design-authority registry's cluster 13 documents an entire pipeline already staged around exactly this truth/knowledge split: Espionage names secret content "purely descriptively," `gens-secrets-hooks-design.md` "owns the actual Secret record," and `gens-scandal-design.md` "owns the public-exposure aftermath once something goes public" — three separate documents already assume a knowledge layer distinct from the underlying Secret record exists.
- `gens-characters-design.md` §15's own open question asks "whether and how the player is informed of purely NPC-on-NPC outcomes... a Chronicle entry only, a rumor via Gossip, or nothing at all" — a question this ADR's mechanism is what will eventually answer, event by event, once §15 is revisited.

## Decision

`WorldState` is split into two kinds of partition, never merged: **truth partitions** (the actual Character, Plot, DebtRecord, Scheme records — what this project's field ledger calls "Owner" state) and a separate **`KnowledgeState`** partition, keyed by `(ObserverId, SubjectId, Topic)`, where `ObserverId` is any Character or the reserved "player" observer, `SubjectId` is the entity or fact the knowledge is about, and `Topic` names which field or event class it concerns (e.g. `"health.range"`, `"activeScheme.exists"`, `"debtRecord.status"`).

Each `KnowledgeState` entry holds: the known value (which may be a coarser/noisier projection of the truth value, not the truth value itself — e.g. a health *range* rather than the exact integer, matching `gens-labor-slavery-design.md` §3 directly), a `Confidence` tier (`certain` / `believed` / `rumored`, covering deception per that same section), an `AsOfDate` (`GameDate`, staleness — a letter's information ages the moment it's sent, per Correspondence & Letters' design-authority note that it's "the remote, deliberately lower-stakes counterpart to Travel"), and a `Provenance` (which event or interaction the observer learned this from, chaining to ADR 0007's `CausationId`).

Every `IDomainEvent`'s mandatory `Visibility` field (ADR 0007) is what drives `KnowledgeState` writes: an event's visibility descriptor names which observers' `KnowledgeState` update, at what confidence, immediately (a witnessed Interaction) or on a delay (a letter in transit, an intelligence report). A field that a design document marks as visibility-restricted at the *record* level (Labor & Slavery §3's acquisition-time slave record) is authored in content/code as a set of per-field `Topic`s with a narrower default `Visibility` than the record's other fields — visibility is a field-level property flowing from the field ledger, not a record-level toggle.

Systems that render a "dossier," a report, a rumor, or a scheme's discovery state (rule 6's own examples) read only `KnowledgeState` for the relevant observer — never the truth partition directly — the same discipline ADR 0013 applies to the UI boundary, applied here to any in-fiction observer, including NPCs reasoning about each other per `gens-characters-design.md` §8.3.

## Consequences

- Deception (`gens-labor-slavery-design.md` §3's seller misrepresentation) is representable directly: the seller's asserted `KnowledgeState` entry for the buyer's observer can simply be wrong, distinct from the truth partition, with no special-case mechanism needed.
- NPC-on-NPC schemes (`gens-characters-design.md` §8.3) can run, resolve, and even leak into rumor without ever touching the player's own `KnowledgeState` unless an event's `Visibility` says the player observer should learn of it — directly answering §15's open question at the mechanism level, even though the exact propagation rules per event type remain future content/design work.
- This is real, ongoing state, not a derived cache: `KnowledgeState` is saved and migrated (ADR 0010/0011) exactly like any other partition, since a player's incomplete or wrong knowledge of the world is itself part of what "continuing a campaign" means.

## Alternatives Considered

- **Compute knowledge on demand from the event log at read time**, rather than maintaining a persisted `KnowledgeState` partition. Rejected: correct in principle but expensive at scale (replaying every relevant historical event per query) and awkward for staleness (a letter's `AsOfDate` needs to reflect when it was *sent*, not be recomputed from "now"); a materialized, incrementally-updated partition is the standard tradeoff and matches how the corpus already treats a Dossier or Rumor as a standing record, not a live query.
- **Record-level visibility flags only** (a whole Character record is "hidden" or "visible" to an observer) instead of field-level `Topic`s. Rejected: directly contradicts `gens-labor-slavery-design.md` §3's own worked example, where some fields (age range, origin) are visible while others (exact skill numbers, most traits) are not, on the *same* record, to the *same* observer, simultaneously.
- **No separate KnowledgeState; let each consuming system (Dossier, Rumor, Report) maintain its own private visibility bookkeeping.** Rejected: this is precisely the "each future system inventing its own" pattern rule 6 and the broader roadmap explicitly warn against (mirroring rule 2's "one command path" reasoning applied to reads instead of writes).
