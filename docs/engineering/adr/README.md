# Gens — Architecture Decision Records

This index closes Phase 1, Item 3 of the [Comprehensive Build Roadmap](../gens-comprehensive-build-roadmap.md): "Create architecture decision records for IDs, fixed-point arithmetic, time/epoch, tick phases, event envelopes, command atomicity, deterministic collection ordering, visibility/knowledge, fidelity tiers, save serialization, migrations, content versioning, and UI projection boundaries." All 13 named topics are covered, one ADR each. A 14th ADR was added later to adopt the custom runtime/native-client migration; see below.

## Numbering and status convention

- ADRs are numbered sequentially (`0001`, `0002`, ...) in **dependency order**, not roadmap-list order: primitives (IDs, numbers, time, ordering) come first, then the simulation-loop contracts built on top of them (tick phases, commands, events), then the systems that read those contracts (visibility/knowledge, fidelity tiers), then persistence (saves, migrations, content), and finally the presentation boundary that depends on everything above it.
- An ADR moves to **Accepted** once the corresponding engineering contract has real code and tests behind it, and to **Superseded** (with a pointer to its replacement) if a later decision reopens it — never edited in place once Accepted, per this project's own migration discipline (ADR 0011). ADRs 0001–0007 became **Accepted** as of Phase 2 ("Harden the deterministic simulation kernel"), which built `Fixed64`, typed IDs and registries, `WorldState`, the epoch-aware `GameDate`, tick-phase scheduling, the command envelope, and the domain-event envelope against them. ADRs 0010–0012 became **Accepted** as of Phase 3 ("Build saves, content contracts, and developer tooling"), which built the migration registry, `SaveReader`/`SaveWriter`, and the content compiler's validator/manifest. ADR 0013 became **Accepted** as of Phase 9 ("Build the player loop"), which built the `IWorldQuery` projection interfaces and the Unity shell/adapters that consume them. ADRs 0008 and 0009 became **Accepted** as of Phase 10 ("Add delegation, autonomous action, and rival houses"), which built `KnowledgeState`/`KnowledgeConfidence` and the fidelity-tier framework (`LivingWorldActor`) respectively. All 13 ADRs are now **Accepted**; see the [build roadmap](../gens-comprehensive-build-roadmap.md) for what is still unbuilt (Phases 16–18).
- Each ADR cites the specific non-negotiable construction rule(s) (numbered 1–14 in the roadmap) and specific design-document sections that drove the decision, distinguishing corpus-grounded requirements from engineering judgment calls the design corpus is silent on — the same evidentiary discipline the [design authority registry](../../gens-design-authority-registry.md) established for design-ownership claims.
- Where an ADR extends existing code (`src/Gens.Simulation/...` from the merged framework PR), it says so explicitly and treats that code as the starting point to extend, not to replace wholesale.

## The 13 ADRs

| # | Title | One-line decision |
|---|---|---|
| [0001](0001-stable-typed-ids.md) | Stable Typed IDs | Two ID families — content-authored `DefinitionId<T>` strings and campaign-scoped, counter-issued `RuntimeId<T>` structs — never interchangeable, never GUID- or clock-derived. |
| [0002](0002-fixed-point-arithmetic.md) | Fixed-Point and Integer Arithmetic | Plain integers for counts/stats/money-in-minor-units; a shared `Fixed64` (parts-per-million) for every rate/ratio the corpus names; no `double`/`float` anywhere in simulation state. |
| [0003](0003-epoch-and-game-date.md) | Epoch and GameDate | Keep `GameDate.TotalMonths` as the sole authoritative, compact time value; fix its epoch at astronomical year -753; derive BCE/CE and calendar display via pure conversion functions, never stored redundantly. |
| [0004](0004-deterministic-collection-ordering.md) | Deterministic Collection Ordering | No raw dictionary/hash-set iteration reaches events, hashes, RNG, or saves; every `WorldState` collection is an ordered index, ascending `RuntimeId`, matching the existing `RandomStreamSet` sorting precedent. |
| [0005](0005-tick-phases.md) | Monthly Tick Phases | Ten fixed phases (`ScheduledCommands → Lifecycle → Production → EmploymentNeeds → MarketsLedger → RelationshipsActors → Hazards → Events → Reports → InvariantChecks`); every system declares phase, reads, writes, prerequisites; the scheduler topologically sorts once and fails on cycles. |
| [0006](0006-command-atomicity.md) | Command Envelope and Atomicity | Extend the existing `ICommand`/`CommandPipeline` with `CommandId`, `ActorId`, `SubmittedDate`, `CausationId`, a stable `ValidationErrorCode`, and a deterministic sequence number; validation is pure and RNG-free so a rejected command never perturbs state or RNG. |
| [0007](0007-event-envelope.md) | Domain Event Envelope | Extend `IDomainEvent` with `EventId`, `Type`/`SchemaVersion`, `OccurredDate`, `SubjectIds`, a mandatory `Visibility` descriptor, `CausationId`, and a versioned payload; every report/Chronicle reads only events, never raw state. |
| [0008](0008-visibility-and-knowledge.md) | Visibility and Knowledge Separation | Split `WorldState` into truth partitions and a separate `KnowledgeState` keyed by `(ObserverId, SubjectId, Topic)` with confidence, staleness, and provenance; every event's `Visibility` field drives what gets written there. |
| [0009](0009-fidelity-tiers.md) | Fidelity Tiers | Three tiers — `Background` (aggregate `PopGroup` only), `Noteworthy` (reserved for Phase 10's `LivingWorldActor`), `Named` (full Character record) — with promotion as a one-directional, population-conserving command. |
| [0010](0010-save-serialization.md) | Save Serialization | Canonical, deterministically-ordered JSON inside an atomically-written `.gens` ZIP; per-entry SHA-256 checksums in `manifest.json`; RNG states persist via the existing `RandomStreamSet.CaptureStates()`/`Restore()` shape unchanged. |
| [0011](0011-migrations.md) | Save Migrations | A migration registry of pure JSON-DOM functions, chained strictly sequentially by version, each with a permanent golden-save fixture verified in CI as a merge gate; additive-only changes need no migration. |
| [0012](0012-content-versioning.md) | Content Versioning and Validation | One authoring location per content family with hard duplicate-`DefinitionId` failures across it — the concrete, automatable fix for the still-live Buildings/Estate & Settlement goods-list contradiction the design authority registry flags; retirement tombstones definitions, never deletes them. |
| [0013](0013-ui-projection-boundaries.md) | UI Projection Boundaries | UI touches the simulation only through read-only query projections (filtered through ADR 0008's `KnowledgeState`) and command submission (ADR 0006); no mutable domain object is ever exposed to Unity/UI Toolkit code. |

## ADR 0014 — a platform decision, not a boundary change

[ADR 0014](0014-custom-runtime-and-native-client.md) formally adopts a
purpose-built Gens runtime and native desktop client as the long-term
presentation platform, replacing Unity once explicit retirement gates pass.
It is numbered and indexed separately from the 13 ADRs above because it
answers a different kind of question: the 13 above are simulation-kernel and
persistence contracts that became Accepted only once real code and tests
existed behind them, while ADR 0014 is a forward-looking platform decision
recorded Accepted immediately so it can govern implementation from the start
(see its own Status section for why).

**ADR 0014 changes the concrete presentation/runtime platform. It does not
change, reopen, or supersede ADR 0013.** ADR 0013's rule — UI touches the
simulation only through query projections and command submission, never a
mutable domain reference — is generalized in ADR 0014's terminology from
"Unity/UI Toolkit" to "any presentation host," but the rule itself, and its
Accepted status, are unchanged. The future native client is bound by the same
boundary Unity is bound by today.

## Relationship to the other Phase 1 artifacts

- The [design authority registry](../../gens-design-authority-registry.md) is the map these ADRs were built from — each ADR's Context section cites the specific registry cluster or design-document section it grounds a decision in.
- The [cross-system field ledger](../gens-field-ledger.md) is this index's sibling deliverable (Phase 1, Item 4): it applies these ADRs' vocabulary (typed IDs, `Fixed64` rates, fidelity tiers, visibility) to the actual fields the vertical-slice design documents define, system by system.

## ADR 0015 — proposed backend selection

[ADR 0015](0015-native-backend-selection.md) records the measured Windows candidate.
It remains **Proposed** while the NR2 desktop exit checklist is incomplete.
Ticket 4's production-shaped foundation was implemented under an explicit
override; that implementation does not itself satisfy ADR 0015's outstanding
acceptance conditions.

## ADR 0016 — retained-mode UI

[ADR 0016](0016-retained-mode-ui-tree-and-layout.md) records the accepted
retained-tree, measure/arrange, logical-unit, typed-theme, backend-neutral, and
accessibility-semantics foundation implemented by Ticket 5.
