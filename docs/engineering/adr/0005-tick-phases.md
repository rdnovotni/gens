# ADR 0005 — Monthly Tick Phases

**Status:** Accepted

## Context

Non-negotiable rule 9: "Every monthly system declares reads, writes, prerequisites, and phase. Debug builds verify the declared access set and invariants." Roadmap Phase 2, item 4 supplies a concrete candidate ordering: "Define tick phases, for example: scheduled commands → lifecycle → production → employment/needs → markets/ledger → relationships/actors → hazards → events → reports → invariant checks." Item 5 in the same phase requires: "Make systems declare ID, phase, dependencies, read set, and write set. Topologically sort once and fail on missing or cyclic dependencies."

The current code is precisely the "prototype" the roadmap's skeleton-assessment table describes: `src/Gens.Simulation/Time/MonthlySimulation.cs`'s `IMonthlySystem<TState>` interface declares only `Id` and a `Tick` method; `MonthlySimulation<TState>` runs systems "in their explicit registration order" (per its own doc comment) with no phase, dependency, read-set, or write-set concept at all — exactly the gap this ADR closes.

The design corpus's cross-system dependency chains make the roadmap's example ordering concrete rather than arbitrary: `gens-resources-goods-design.md` §2 states every good's "production, storage, spoilage or aging, consumption, overflow sale, and market pricing... runs automatically every month-tick" off standing decisions — production must resolve before the market that prices its output. `gens-settlement-demographics-design.md` §4.2's Employment Ratio depends on `BackgroundEconomicCapacity`, which in turn depends on completed buildings (`gens-estate-settlement-design.md` §4's construction-months-remaining countdown) — lifecycle/construction before employment. `gens-economy-finance-design.md` §3.1's Rents depend on Settlement Demographics' Contentment and pop-group presence, which itself depends on employment and needs — ledger after employment/needs, exactly as the roadmap's example lists it. `gens-characters-design.md` §8.3's NPC-initiated interactions and §10's Scheme progress are explicitly relationship/actor-driven, naturally sitting after markets/ledger settle for the month. Every automated system's monthly report (`gens-economy-finance-design.md` §10, `gens-resources-goods-design.md` §2) reads *this* month's completed events, so Reports must be strictly the second-to-last phase, with Invariant Checks last per Phase 2's own exit-gate language ("invariant hooks and deterministic state hashing after commands and ticks").

## Decision

Ten fixed, named phases, run in this order every tick, matching the roadmap's example exactly since the corpus's own dependency chains independently confirm it:

`ScheduledCommands → Lifecycle → Production → EmploymentNeeds → MarketsLedger → RelationshipsActors → Hazards → Events → Reports → InvariantChecks`

`IMonthlySystem<TState>` (currently `{ Id, Tick }`) grows three required members: `Phase` (one of the ten, fixed enum, not extensible per-system), `Reads`/`Writes` (sets of `WorldState` partition tags, e.g. `"characters"`, `"settlementMarket:{settlementId}"`), and `Prerequisites` (system `Id`s that must run first, for ordering *within* a phase — e.g. Estate & Settlement's construction-completion system must run before Resources & Goods' production system, both inside `Production`). `MonthlySimulation<TState>` topologically sorts once at construction (phase order first, then prerequisite order within a phase, then `Id` lexical order as the final deterministic tiebreak per ADR 0004) and throws on a missing or cyclic prerequisite rather than silently falling back to registration order. A debug-only verification pass wraps each system's `Tick` call to assert its actual state mutations stayed inside its declared `Writes` set — the concrete mechanism behind rule 9's "debug builds verify the declared access set."

## Consequences

- Adding a new monthly system is a declaration, not a registration-order guess: a new Labor & Slavery flight-risk check declares `Phase = RelationshipsActors`, `Reads = {characters, regimen}`, `Writes = {flightRisk}`, and the scheduler places it correctly without the author needing to know every other system's position.
- The topological sort failing loudly on a cycle turns an entire category of latent bugs (system A needs B's output, B was accidentally registered after A) into a startup-time error instead of a silently wrong tick.
- Phase 2's exit gate (identical hashes across repeated runs) becomes directly testable per-phase, not just end-to-end, once each phase's write set is declared.

## Alternatives Considered

- **A fully general dependency graph with no fixed phase buckets**, letting any system declare prerequisites on any other system directly. Rejected: harder to reason about at a glance, and rule 9 explicitly asks for "phase" as a first-class declared property, not an emergent property of an arbitrary graph — fixed phases also bound how deep a single tick's dependency chain can get, which matters for the 250 ms/1 s performance budget (rule 12).
- **Per-system configurable phase ordering (a numeric priority instead of ten named phases).** Rejected: numeric priorities invite silent collisions and don't communicate *why* one system runs before another the way named phases (`Production` before `MarketsLedger`) do; named phases are self-documenting against the corpus's own dependency language ("supplies," "feeds," "runs on top of").
- **Running all systems in parallel within a phase for performance**, deferred rather than rejected: Phase 2's determinism exit gate must be proven with strictly sequential execution first; parallelizing within a phase (where `Writes` sets are proven disjoint) is a Phase 18 profiling-driven optimization per rule 12, not a day-one decision.
