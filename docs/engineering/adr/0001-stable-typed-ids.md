# ADR 0001 — Stable Typed IDs

**Status:** Proposed

## Context

Non-negotiable rule 5 requires that "campaign, region, settlement, household, character, institution, contract, activity, and historical-event state need explicit ownership boundaries and stable IDs." Rule 3 requires that ordering never depend on "dictionary iteration... or incidental registration order." The roadmap's Phase 2, item 1 names the concrete scope: "stable typed IDs and registries for campaigns, definitions, regions, settlements, plots, households, actors, characters, goods, buildings, contracts, events, and activities."

The design corpus already assumes IDs exist without specifying their shape. `gens-characters-design.md` §14 opens `Character { id, praenomen, ... }`; `gens-estate-settlement-design.md` §8 opens `Plot { id, terrain, region, ... }`; `gens-resources-goods-design.md` §16 keys `Good { key, tier, ... }` and `LivestockStock { buildingId, ... }`; `gens-economy-finance-design.md` §12 references `personId`, `plotId`, `rivalHouseId`, `settlementId` as foreign keys throughout `DebtRecord`, `CapitalExpenditure`, and `WindfallEvent`. None of these documents specify whether an ID is a string, a GUID, or an integer, or whether "goods" IDs (content-authored, e.g. `"grain"`) and "character" IDs (runtime-generated, e.g. the 4,000th character born in a campaign) should share a scheme. They clearly do not: `tech-stack.md` already commits definitions to "stable string IDs, uniqueness, and references... validated before normalized runtime JSON is emitted," which only makes sense for content, not runtime instances.

The existing `Gens.Simulation` code (`SaveManifest`, `RandomStreamSet`) has no ID type yet at all; this is greenfield.

## Decision

Two separate, non-interchangeable ID families:

1. **`DefinitionId<T>`** — a validated, non-empty ASCII string (kebab-case, e.g. `"grain"`, `"iron-ore"`, `"strong-weak"`), authored in content and never runtime-generated. Owned and validated by the content compiler per `tech-stack.md`. Stable across content-pack versions; a retired definition is tombstoned (ADR 0012), never deleted or reassigned to a new meaning.
2. **`RuntimeId<T>`** — a `readonly record struct` wrapping a campaign-scoped, monotonically increasing 64-bit integer plus a compact type tag (e.g. `char_0000042`), issued by a per-entity-kind counter stored in `WorldState`. Runtime IDs are **not** GUIDs and are **not** derived from wall-clock time: both would break bit-for-bit deterministic replay, since two runs of the same seed and command log must issue identical IDs in identical order. The issuing counter is itself campaign state, saved and restored like any other field.

`T` is a phantom type per entity kind (`RuntimeId<Character>`, `RuntimeId<Plot>`, `RuntimeId<Contract>`...) so a `Character` ID can never be silently passed where a `Plot` ID is expected — a compile-time guard, not just a runtime one.

Because runtime IDs are assigned by a strictly increasing counter, sorting entities by ID is equivalent to sorting by creation order, which gives every system a free, deterministic default ordering (feeding ADR 0004) without needing a separate timestamp.

## Consequences

- Every new entity kind (Character, Plot, Household, Contract, Activity, HistoricalEvent, Institution, Actor) gets its counter and its `RuntimeId<T>` wrapper before any system that creates one is implemented.
- Cross-references in save files serialize as the tagged string form (`char_0000042`), keeping saves human-diffable per `tech-stack.md`'s canonical-JSON goal, while runtime code holds the compact struct.
- Content authors never see or assign a `RuntimeId`; simulation code never treats a `DefinitionId` as orderable campaign state.
- Registries (per-kind indexes from `RuntimeId<T>` to the entity) become the standard WorldState building block, feeding ADR 0004's ordered-collection policy directly.

## Alternatives Considered

- **GUIDs for runtime entities.** Rejected: not deterministic across replay unless seeded from the RNG stream, which would then perturb draws in unrelated systems — a direct violation of rule 8 ("adding a draw in one system must not perturb another").
- **A single untyped `string Id` for everything**, as most design-doc sketches informally use. Rejected: collapses the definition/runtime distinction the corpus already needs (`gens-resources-goods-design.md` keys goods by string `key` while `gens-characters-design.md` needs a counter-like `id`), and loses the compile-time type safety a large cross-system field ledger like this project's depends on.
- **Composite natural keys** (e.g., `(settlementId, plotIndex)`). Rejected as the general case: works for `Plot` but not for `Character`, `Contract`, or `HistoricalEvent`, which have no natural composite key; a single scheme is simpler to reason about project-wide.
