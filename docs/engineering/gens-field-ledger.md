# GENS — Cross-System Field Ledger

*This document closes Phase 1, Item 4 of the Comprehensive Build Roadmap: "create a cross-system field ledger: field name, type, units, range, owner, readers, writers, persistence, visibility, and migration policy." It complements, and does not duplicate, two sibling artifacts: `docs/gens-design-authority-registry.md` resolves which **design document** owns a concept in prose; `docs/gens-canonical-registry-design.md` enumerates named **content entities** (every Culture, every Good, every Trait). This ledger is narrower and more operational than either — it lists the actual **runtime state fields** that more than one system reads or writes, grounded in the code that exists today (Phases 2–6), so a system author can check "who else touches this field, and under what save-compatibility contract" before changing it.*

*Scope: fields owned by `WorldState` and its partitions that cross a system boundary — i.e., a field that at least one system other than the one that writes it also reads. Fields that are purely local to one system's internal bookkeeping are not listed. As Phase 7 onward add population groups, ledger accounts, and further systems, extend this table in the same pull request that introduces the field, per `CONTRIBUTING.md`'s "update documentation in the same pull request" rule.*

---

## How to read this table

- **Owner** — the system/file whose command handlers are the sole writer under normal operation (ADR 0006's one-command-path rule: even AI/steward/migration writers go through the same handler).
- **Readers** — other systems or projections that consume the field without mutating it.
- **Persistence** — whether the field is captured in `.gens` save archives (ADR 0010) and, if so, the save-schema section it lives in.
- **Visibility** — whether the field is player-omniscient truth or gated through `KnowledgeState` (ADR 0008).
- **Migration policy** — what a future save-breaking change to this field's shape requires, per ADR 0011.

---

## 1. Time and identity

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `WorldState.Date.TotalMonths` | `int` | Months since epoch (ADR 0003 epoch: 753 BCE = month 0); no upper bound short of `int` overflow (checked arithmetic in `GameDate.NextMonth()`) | `MonthlySimulation` tick driver | Every system; all report/UI projections | Yes — save header | Omniscient | Save-breaking; version bump + migration if the epoch or arithmetic ever changes |
| `RuntimeId<T>` counters (`RegionIds`, `SettlementIds`, `PlotIds`, `HoldingIds`, `HouseholdIds`, `ActorIds`, `CharacterIds`, `BuildingIds`, `ContractIds`, `ActivityIds`, `CommandIds`, `EventIds`, `ScheduledActionIds`) | `long`, monotonic per kind | ≥ 0, one counter per entity kind (ADR 0001) | `WorldState` via `RuntimeIdCounter<T>.Restore` | Every system that allocates or references an entity of that kind | Yes — one counter per kind in the save manifest | Omniscient (an ID's existence is not secret; what it *names* may be) | Additive-only; a new entity kind adds a new counter, never repurposes an existing one |
| `Command.SequenceNumber` (`WorldState.NextCommandSequenceNumber`) | `long`, monotonic | ≥ 0 | `CommandPipeline` | Deterministic-replay diagnostics, event causation chains | Yes | Omniscient | Save-breaking if the sequencing rule changes; must stay stable for replay hash equality |

## 2. Command and event envelopes

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `Command.ActorId` | string/ID | — | `CommandPipeline` (submission) | Validation handlers, event causation, AI/steward attribution | Yes, in command log where persisted | Omniscient | Additive-only |
| `Command.SubmittedDate` | `GameDate` | Months (§1) | `CommandPipeline` | Scheduling, `ScheduledActionSystem` | Yes | Omniscient | Save-breaking if `GameDate` representation changes |
| `Command` validation error code | enum/string | Fixed vocabulary (ADR 0006) | Each system's validation handler | UI confirmation flow, tests asserting rejection behavior | Not persisted (rejected commands leave no state trace) | Omniscient | Additive-only; existing codes must not be renumbered/removed once shipped |
| `IDomainEvent.EventId` | `RuntimeId<DomainEventEntity>` | — | `CommandPipeline` mutation phase | `MonthlyReportProjector`, Chronicle (future), knowledge propagation | Yes | Per-event `Visibility`/provenance field | Additive-only |
| `IDomainEvent.OccurredDate` | `GameDate` | Months (§1) | Same as above | Same as above | Yes | Omniscient | Save-breaking if `GameDate` changes |
| `IDomainEvent.CausationId` | `RuntimeId<Command>?` | — | Same as above | Replay diagnostics, report drill-down | Yes | Omniscient | Additive-only |

## 3. Random streams

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `RandomStreamSet` named stream state (per-stream PCG32 state + increment) | `ulong` × 2 per named stream | Full `ulong` range | `RandomStreamSet` | Every system that draws from a named stream (lifecycle, traits, production variance, event resolution) | Yes — one entry per named stream | Omniscient (state itself is meaningless without the algorithm; not player-facing) | Save-breaking if the PCG32 algorithm or a stream's *name* changes; `SeedDerivation` version is itself a persisted field |
| `SeedDerivation` algorithm version | `int`/string tag | — | `RandomStreamSet` | Save/load, migration runner | Yes | Omniscient | Additive-only; a new derivation version must not silently reinterpret old seeds |

## 4. Character and Familia (Phase 5)

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `Character.Id` | `RuntimeId<Character>` | — | Character lifecycle system | Household, labor, relationships, events, reports, UI | Yes | Omniscient (existence); traits/condition may be knowledge-gated | Additive-only |
| `Character.BirthDate` | `GameDate` | Months (§1) | Character lifecycle system | Aging, mortality, marriage eligibility, education gates | Yes | Omniscient | Save-breaking if `GameDate` changes |
| `Character.LegalStatus` | enum | `Free`/`Freed`/`Enslaved` (per `gens-labor-slavery-design.md`) | Character lifecycle system, manumission/flight handlers | Labor eligibility, marriage, legal-case gates (future) | Yes | Omniscient | Additive-only for new statuses; existing values must not be renumbered |
| `Character.SocialClass` | enum, nullable | — | Character lifecycle system | Wage bands, office eligibility (future), report grouping | Yes | Omniscient | Additive-only |
| `Character.Household` | `RuntimeId<Household>?` | — | Household role assignment | Labor assignment, duty rosters, relationship scoping, reports | Yes | Omniscient | Additive-only |
| `Character.Attributes` (`CoreAttributes`) | int fields | Design-corpus-defined ranges (see `gens-characters-design.md`) | Character generation | Duty competence checks, labor output, event resolution | Yes | Omniscient to owning household; knowledge-gated to outside actors | Additive-only field-by-field |
| `Character.Skills` (`LaborSkills`) | int fields | Design-corpus-defined ranges | Character generation, labor system | `ProductionSystem`, duty assignment | Yes | Same as Attributes | Additive-only |
| `Character.Condition` | record (health/fatigue) | — | Health/lifecycle system, labor fatigue/injury handlers | Labor output calculator, mortality, event eligibility | Yes | Omniscient to household | Additive-only |
| `Character.Traits` | `IReadOnlyList<DefinitionId<Trait>>` | Opposed-pair exclusivity enforced at generation (Phase 5 item 4) | Character generation | Event/interaction resolution, report flavor | Yes | Knowledge-gated to outsiders per ADR 0008 | Additive-only; a trait's definition ID is content, not save-schema |
| `Character.MotherId` / `FatherId` | `RuntimeId<Character>?` | — | Birth/lifecycle system | Genealogy, succession (future), Chronicle (future) | Yes | Omniscient (parentage) vs. knowledge-gated (legitimacy detail) | Additive-only |
| `Character.Legitimacy` | enum | — | Birth/lifecycle system | Succession eligibility (future), social-standing modifiers (future) | Yes | Knowledge-gated | Additive-only |
| `Character.MaritalHistory` | `IReadOnlyList<MarriageRecord>` | — | Marriage/lifecycle system | Household composition, succession, relationships | Yes | Knowledge-gated in part (private grounds for divorce, etc.) | Additive-only; existing records immutable once written |
| `Character.Duty` (`DutyAssignment`) | record, nullable | — | Household role assignment | Labor availability, location-conflict checks | Yes | Omniscient to household | Additive-only |
| `Character.Regimen` (`RegimenSettings`) | record, nullable | Diet tier per `gens-resources-goods-design.md` §13.2 | Labor/needs system | `ProductionSystem` consumption, health | Yes | Omniscient to household | Additive-only |
| `Character.Flight` / `Pursuit` / `ManumissionPlan` | records, nullable | — | Labor flight/manumission handlers (Phase 6 item 6) | Labor availability, legal-status transitions | Yes | Knowledge-gated (pursuit intelligence quality) | Additive-only |
| `Relationship` (opinion/bond/provenance/decay/last-interaction) | keyed by `RelationshipKey` (directed pair) | Opinion range per `gens-characters-design.md`; asymmetric by design (see `RelationshipAsymmetryPropertyTests`) | Relationship system | Event/interaction resolution, marriage eligibility, reports | Yes | Knowledge-gated | Additive-only |

## 5. Land, buildings, and villa (Phase 6)

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `Plot.Terrain` / `Features` | enum / flags | — | Region/settlement bootstrap (content-authored) | Building placement gates, production yield modifiers | Yes | Omniscient | Additive-only for new terrain/feature values |
| `Plot.Condition` (`LandCondition`) | enum | — | Land condition system | Production yield, construction eligibility | Yes | Omniscient | Additive-only |
| `Plot.Capacity` | `int` | Building slots, ≥ 0 | Region/settlement bootstrap | Construction validation | Yes | Omniscient | Additive-only |
| `Plot.OwnerId` / `Holding.OwnerId` | `string?` | — | Ownership/acquisition handlers (Phase 6 item 2) | Command validation (who may act on this plot/holding), ledger (future) | Yes | Omniscient (ownership is public record) | Additive-only |
| `Plot.OccupyingHoldingId` / `Holding.OccupantId` | `RuntimeId<Holding>?` / `string?` | — | Occupancy handlers | Household residency, capacity checks | Yes | Omniscient | Additive-only |
| `Plot.Acquisition` (`LandAcquisition`) | record, nullable | — | Acquisition handlers | Reports, provenance for exceptional-object hooks (future) | Yes | Omniscient | Additive-only |
| `Holding.ResidentCapacity` | `int` | Persons, ≥ 0 | Holding definition/construction | Household placement, migration (future) | Yes | Omniscient | Additive-only |
| `Stockpile.Capacity` | `long` | Good units, ≥ 0 | Building/holding definition | `ProductionSystem`, market (future) | Yes | Omniscient to owning household | Additive-only |
| `Stockpile` lot quantities and spoilage state | `long` quantity, expiry date | Good-specific (content-authored) | `ProductionSystem` | Consumption, spoilage events, reports | Yes | Omniscient to owning household | Additive-only; a lot's shape must stay migratable since save fixtures pin exact stockpile state |
| `BuildingInstance.Condition` (`BuildingCondition`) | enum | — | Maintenance/upkeep handler | Production output, construction/repair eligibility | Yes | Omniscient | Additive-only |
| `ConstructionSchedule.CompletedMonths` / `RemainingMonths` | `int` | Months, `RemainingMonths = ConstructionMonths - CompletedMonths` (derived, not stored) | Construction system | Reports, UI construction queue | `CompletedMonths` yes, `RemainingMonths` derived | Omniscient | Additive-only |
| `Villa.Stage` (`VillaStage`) | enum | — | Villa upgrade system | Room-slot gating, production/labor eligibility, report thresholds | Yes | Omniscient | Additive-only for new stages; stage ordering must not be renumbered (ordinal comparisons exist) |
| `VillaRoomInstance.Condition` / `AssignedTo` | enum / `string?` | — | Villa room system | Duty assignment, upkeep | Yes | Omniscient to household | Additive-only |
| Ledger-ready production/consumption/construction events (Phase 6 item 8) | `IDomainEvent` payloads | Good ID, signed quantity, actor/holding IDs | `ProductionSystem`, construction handlers | Phase 8's ledger/market (not yet built — this is the contract they will consume) | Yes (as events) | Omniscient to owning household | Additive-only; Phase 8 must read these payloads as-is rather than requiring a shape change to already-shipped events |

## 6. State hashing and invariants

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `StateHasher` output | fixed-width hash (hex string in CLI output) | — | `StateHasher` (reads all partitions in canonical order) | Deterministic-replay tests, `compare-hashes` CLI command, CI exit-gate smoke test | Not persisted itself (recomputed on demand); referenced by save-verification tooling | Omniscient (a debug/ops value, not gameplay-visible) | Any change to hashed field order or included partitions changes hash output for existing saves — acceptable (hashes aren't compared cross-version) but must not silently change *within* a save-schema version |

---

## 7. Economy, ledger, and market (Phase 8 items 1-4)

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `Money.RawValue` (every `LedgerAccount.Balance`, `LedgerPosting.Amount`, `SettlementMarket.Price`/`PreviousPrice`) | `long` | Denarii minor units (100 = 1 Denarius, ADR 0002); may be negative for a balance | `LedgerService.Post` (accounts); `MarketClearingSystem` (market prices) | Every future wages/rents/taxes/debt system (Phase 8 item 6); Monthly Report (Phase 9); invariant checks | Yes — `WorldSaveDocument.LedgerAccounts`/`LedgerTransactions`/`MarketPrices` | Omniscient to the owning household/actor; settlement-treasury and system accounts are campaign-wide | Save-breaking if the minor-unit scale (`Money.ScaleFactor`) ever changes; additive-only otherwise |
| `WorldState.LedgerAccounts` (`LedgerAccountKey` → `LedgerAccount`) | keyed registry | One entry per (kind, owner) pair, sparse | `LedgerService.Post` | `LedgerAccountBalanceConsistencyInvariantCheck`, future purchasing/wages/taxes/debt systems (Phase 8 items 5-6) | Yes | Same as `Money.RawValue` above | Additive-only; a new `LedgerAccountKind` value never renumbers an existing one |
| `WorldState.LedgerTransactions` (`RuntimeId<LedgerTransaction>` → `LedgerTransaction`) | append-only ordered log | One entry per posted transaction; `Postings` sum to zero (enforced at write time by `LedgerService.Post` and re-verified by `LedgerTransactionBalanceInvariantCheck`) | `LedgerService.Post` | `LedgerAccountBalanceConsistencyInvariantCheck`, Monthly Report (Phase 9), Dynasty Chronicle (future) | Yes | Same as `Money.RawValue` above | Additive-only; a transaction, once posted, is never mutated or removed |
| `LedgerTransaction.Category` (`LedgerTransactionCategory`) | enum | Treasury/Wages/Sales/Purchases/Taxes/Upkeep/Construction/Debt/Gifts/Contracts/Transfers (roadmap Phase 8 item 2) | `LedgerService.Post` callers | Monthly Report grouping (Phase 9) | Yes | Same as `Money.RawValue` above | Additive-only; existing values must not be renumbered once shipped |
| `WorldState.MarketPrices` (`MarketGoodKey` → `SettlementMarket`) | keyed registry | One entry per (settlement, good) pair, sparse | `MarketClearingSystem` | `MarketBoundedPriceChangeInvariantCheck`, `MarketNoNegativeStockOrPriceInvariantCheck`, future household purchasing (Phase 8 item 5) | Yes — `WorldSaveDocument.MarketPrices` | Omniscient (a settlement market price is public information) | Additive-only |
| `SettlementMarket.Supply` / `Demand` / `ClearedQuantity` / `UnsatisfiedDemand` | `long` | Good units, ≥ 0; `ClearedQuantity = min(Supply, Demand)`, `UnsatisfiedDemand = Demand - ClearedQuantity` (derived, enforced by `SettlementMarket`'s own constructor) | `MarketClearingSystem` | Monthly Report (Phase 9), future household purchasing (Phase 8 item 5) | Yes | Omniscient | Additive-only |
| `MarketOrder` (`SettlementId`, `GoodId`, `Side`, `SourceId`, `Quantity`) | ephemeral, not `WorldState` | Regenerated every tick from `Stockpiles`/`PopGroups`, never persisted or hashed | `MarketClearingSystem` (internal) | None outside `MarketClearingSystem` itself in this phase | No — deliberately ephemeral | N/A | N/A until Phase 8 item 5 gives orders a persisted, player-submitted form |

## 8. Actions, policies, and events (Phase 9)

| Field | Type | Unit / range | Owner | Readers | Persistence | Visibility | Migration policy |
|---|---|---|---|---|---|---|---|
| `WorldState.HouseholdPolicies` (`RuntimeId<Household>` → `HouseholdPolicyState`) | keyed registry | One entry per household with a non-default Standing Policy configuration, sparse | `ChangeRitesBudgetCommand` | `HouseholdPolicyResolver`, `HouseholdPolicyModifiersQuery`, `PolicyActionDefinitions` | Yes — `WorldSaveDocument.HouseholdPolicies` | Omniscient | Additive-only |
| `WorldState.EventInstances` (`RuntimeId<EventInstance>` → `EventInstance`) | keyed registry | One entry per fired event, kept for the campaign's lifetime regardless of terminal status | `FireEventCommand`, `ResolveEventOptionCommand`, `EventPoolSystem` (stage advance/expiry) | `EventInstanceDetailQuery`, `MonthlyReportProjector` (via `IEventInstanceLinkedEvent`) | Yes — `WorldSaveDocument.EventInstances` | Per-event `Visibility` (`EventVisibilityRules`: Personal scope is witness-only, everything else Public) | Additive-only |
| `EventInstance.Status` (`EventInstanceStatus`) | enum | Pending / AwaitingNextStage / Resolved / Expired | `FireEventCommand` (Pending), `ResolveEventOptionCommand` (Resolved/AwaitingNextStage), `EventPoolSystem` (AwaitingNextStage→Pending, Pending→Expired) | Monthly Report drill-down, `EventPoolSystem`'s own next-tick passes | Yes, as part of `EventInstance` | Same as `WorldState.EventInstances` above | Additive-only; existing values must not be renumbered |
| `EventInstance.ExpiresDate` | `GameDate` | Months (§1); dual meaning per `Status` — see that record's own doc comment | `FireEventCommand`, `ResolveEventOptionCommand`, `EventPoolSystem` | `EventPoolSystem`'s own expiry/stage-advance passes | Yes | Same as `WorldState.EventInstances` above | Save-breaking if `GameDate` changes |
| `IDomainEvent.Scope` (`EventScope`, on every Events-namespace event via `IScopedEvent`) | enum | Personal/Household/Settlement/Imperial (`gens-events-design.md` §2) | `EventPoolSystem`, `FireEventCommand`, `ResolveEventOptionCommand` | `MonthlyReportProjector` grouping/importance/acknowledgement | Not separately persisted — derivable from the owning `EventInstance.Scope` already captured above | Same as the owning event's own `Visibility` | Additive-only |

## Maintenance rule

A new cross-system field (read by a system other than its writer) is added to this ledger in the same pull request that introduces it — per `CONTRIBUTING.md`. A field that stops being cross-system (its only external reader is removed) may be deleted from this ledger in the same pull request that removes that reader.
