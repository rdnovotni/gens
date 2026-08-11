# ADR 0009 — Fidelity Tiers

**Status:** Proposed

## Context

Non-negotiable rule 7: "Fidelity tiers are first-class. Named characters and noteworthy actors receive detailed simulation; background households, populations, and distant actors use bounded aggregates until promoted." Roadmap Phase 10, item 3 later names the general mechanism directly: "Implement the `LivingWorldActor` framework and background/noteworthy fidelity tiers" — but the vertical slice (Phases 5–7) already needs the two extremes of this spectrum working correctly, so the tier boundary itself belongs in Phase 1, not deferred to Phase 10.

The design corpus already draws this exact line, repeatedly and consistently, which is what makes it ready to formalize rather than design from scratch:

- `gens-familia-design.md` §7 states the rule in its cleanest form: every member of the player's own Familia "always keeps a full stat block, however large that group grows," while "unnamed background population... does not automatically get full stat blocks — that population is Settlement Demographics' responsibility, tracked in aggregate."
- `gens-characters-design.md` §11 names the promotion mechanism: "lazy instantiation, the same moment-of-first-contact principle... a Character record is created the instant a person is specifically named," with "age-appropriate backfill" generating "a plausible trait set for their apparent age and source" rather than a blank adult — so promotion is not merely "copy the aggregate record," it is a generation step in its own right.
- `gens-settlement-demographics-design.md` §3 implements the aggregate side concretely: eight `PopGroup`s (Coloni, Operarii, Opifices, Negotiatores, Aeditui, Curiales, Veterans, Non-Household Enslaved), each tracked only as `size`, `legalStatusDistribution`, `employmentRatio`, `contentment` — no individual records at all, by design.
- The design-authority registry's cluster 6 documents that this is not one bespoke pattern but a repeated one across the corpus: `gens-rival-houses-design.md`'s Living World Actor, Notable Households, Wandering Populations, and Notable Businesses are each named as "the sampling-and-promotion pattern this project has now used at every population tier."

Notably, the corpus's own two-tier language ("full stat block" vs. "tracked in aggregate") does not yet name a middle tier, but the registry's cluster 6 "sampling" language (a tribal leader or petty king "is simply a Character... functioning as a Living World Actor at whatever tier the player's actual contact warrants") implies one is needed for Phase 10's rival/actor work — this ADR reserves that slot now so Phase 5–7 code is not built assuming only two tiers ever exist.

## Decision

Three fidelity tiers, declared as a fixed enum every relevant entity carries:

1. **`Background`** — aggregate only, per `gens-settlement-demographics-design.md`'s `PopGroup` shape. No `RuntimeId<Character>` exists for any individual in this tier; the entity is a count inside a group record.
2. **`Noteworthy`** — reserved for Phase 10's `LivingWorldActor` bounded-aggregate tier (a rival house's unnamed members, a distant actor sampled only when the player's "actual contact warrants" it, per registry cluster 6). Not populated by any vertical-slice system (Phases 5–7 use only `Background` and `Named`), but declared now so the enum, the promotion event shape, and any Phase 5–7 code that switches on fidelity tier does not need to be revisited when Phase 10 adds real `Noteworthy` entities.
3. **`Named`** — the full Character record per `gens-characters-design.md` §14: complete Core Attributes, Labor Skills, Condition, traits, Personality Axes, relationships, everything the field ledger's Characters section enumerates. Once a character reaches `Named`, per `gens-familia-design.md` §7, it never demotes back to `Background` — promotion is one-directional for individuals (a settlement's aggregate *population count* can shrink through emigration or death without that implying any specific `Named` character was ever demoted).

**Promotion** is a command (ADR 0006), not a silent state write: a `PromoteToNamedCommand` consumes one unit from the source `PopGroup.size` (keeping population conserved — directly serving `gens-settlement-demographics-design.md` §5's "conservation tests so population changes have causes and no group silently duplicates or disappears"), creates a new `RuntimeId<Character>`, and runs `gens-characters-design.md` §11's age-appropriate backfill generation to populate the full record, setting `Character.source` and `Character.instantiatedAtMonth` exactly as that document's data model already specifies. The triggers are exactly those `gens-settlement-demographics-design.md` §11 names: "a deliberate hire into a Labor Duty Slot, Overseer post, or Court Position; a marriage proposal targeting a named Curiales individual; a Travel or Events encounter singling someone out; or a direct Slave Market purchase."

## Consequences

- Every system built in Phases 5–7 declares which fidelity tier(s) it reads/writes as part of its `IMonthlySystem<TState>` read/write set (ADR 0005), making it mechanically visible if a system accidentally assumes every character in a settlement has a full `Named` record.
- Reserving the `Noteworthy` tier now, even unused, avoids a breaking enum change and a promotion-pipeline rework when Phase 10 needs it — the field ledger and every ADR referencing fidelity tiers stays stable across that phase boundary.
- Population conservation tests (`gens-settlement-demographics-design.md` §5) become a direct, mechanical consequence of routing every promotion through one command rather than an aspiration to test for separately.

## Alternatives Considered

- **Two tiers only (Background/Named), adding Noteworthy in Phase 10 when actually needed.** Rejected: Phase 10's `LivingWorldActor` is explicitly built on top of Characters "without modification" per `gens-characters-design.md` §13 — if the fidelity enum and promotion-event shape aren't stable before Phase 5–7 code exists, that code will need revisiting exactly when Phase 10 lands, which is the situation this ADR is meant to prevent.
- **A continuous fidelity "detail budget" number instead of a fixed enum.** Rejected: nothing in the corpus asks for graduated partial detail (a character is either a full record or an aggregate count, per `gens-familia-design.md` §7's own binary framing); a continuous scale would be unimplementable against systems built to read either a `PopGroup` or a `Character`, not something in between.
- **Automatic tier assignment purely from gameplay proximity (distance/screen presence) rather than explicit promotion commands.** Rejected: contradicts rule 2 (one command path) and the corpus's own explicit trigger list in `gens-settlement-demographics-design.md` §11 — promotion is deliberate and event-sourced, not an implicit side effect of rendering.
