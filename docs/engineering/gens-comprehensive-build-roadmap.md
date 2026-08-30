# Gens — Comprehensive Bottom-Up Construction Roadmap

**Repository:** [`rdnovotni/gens`](https://github.com/rdnovotni/gens)  
**Audit point:** commit [`bc8dfd5090c38727d9cecdb00317a5a3db74ead0`](https://github.com/rdnovotni/gens/tree/bc8dfd5090c38727d9cecdb00317a5a3db74ead0), 9 August 2026  
**Purpose:** define the order in which Gens should be constructed from its present technical skeleton through a simulation-complete, content-rich release.

## Executive decision

Gens should be built as a **headless deterministic simulation first, a playable vertical slice second, and a broad historical sandbox third**.

The repository has the correct strategic stack—Unity 6.3 LTS, C#, an engine-independent `netstandard2.1` simulation package, UI Toolkit, URP 2D, integer authoritative state, named PCG32 streams, data-authored content, and optional asynchronous AI artwork—but it does not yet have a gameplay foundation. The current code is a collection of useful seams rather than a working game model.

The correct dependency spine is:

1. make the repository green and enforceable;
2. reconcile design authority and quantify the first slice;
3. harden the deterministic kernel, state model, saves, and content contracts;
4. create a bootable headless campaign;
5. build characters and household continuity;
6. build land, production, labor, population, and economy in that order;
7. expose that loop through actions, policies, events, reports, and the first Unity UI;
8. add autonomous houses, institutions, geography, danger, advanced commerce, conflict, and cultural breadth on top;
9. scale content, presentation, performance, and release operations only after the simulation survives long deterministic runs.

This order preserves the intended depth. It does not cut the large systems; it prevents them from being implemented against incompatible assumptions.

## Current-state audit

### What exists

The [technical baseline](https://github.com/rdnovotni/gens/blob/bc8dfd5090c38727d9cecdb00317a5a3db74ead0/docs/engineering/tech-stack.md) is unusually clear for this stage:

- Unity 6.3 LTS with UI Toolkit and URP 2D;
- a Unity-free C# simulation package;
- deterministic monthly ticks with a normal target below 250 ms and a one-second maximum-scale ceiling;
- integer authoritative values and named, persisted PCG32 random streams;
- JSON/CSV source content compiled and validated before runtime;
- versioned atomic `.gens` saves;
- procedural SVG portraits as the reproducible baseline;
- optional AI art behind an asynchronous provider and never required to continue a campaign;
- ordinary managed C# first, with Burst/Jobs only after profiling and no baseline DOTS/ECS architecture.

The merged [framework pull request](https://github.com/rdnovotni/gens/pull/5) added:

- `Pcg32` and `RandomStreamSet`;
- `GameDate`, `IMonthlySystem<TState>`, and ordered `MonthlySimulation<TState>`;
- a minimal command/validation/mutation/event abstraction;
- save-format constants and a manifest record;
- null and mock art-generation providers;
- a first content compiler and JSON Schema;
- three small test files and a random-number benchmark.

The [design index](../design/README.md) links 110 design documents (111 files under `docs/design/` including the index itself), totaling approximately 384,000 words and 24,800 lines as of this revision (excluding the index file itself). The design is broad and rich; it is not yet an engineering contract.

### What does not exist yet (as of the original audit)

At the audit point there was no authoritative `WorldState`, campaign bootstrap, historical date conversion, entity-ID model, deterministic collection policy, transaction/change-set model, event envelope, query/read-model layer, save writer/reader, migration runner, substantive content schema, headless simulation executable, gameplay system, Unity scene, UI document, or player loop.

The authored content catalog contained only `status.placeholder`. The JSON Schema required only an `id` and permitted every other property. The benchmark measured 10,000 random draws rather than a monthly tick. The save code declared names and a version but did not serialize or migrate a campaign. The art-provider seam was intentionally disconnected from gameplay.

**This has since changed substantially.** Phases 0–11 (see "Detailed roadmap" below, and the checklist immediately following this paragraph) have since been implemented and merged: `WorldState`, typed IDs, epoch-aware `GameDate`, phased ticks, command/event envelopes, RNG stream registry, canonical save serialization with migrations, typed content-definition families (goods, buildings, traits, policies, events, regions, cultures, religions, names, presentation), a headless campaign bootstrap and console runner, `Character`/Familia lifecycle, region/settlement/plot/holding, stockpiles, buildings, villas, labor, and a production network with ledger-ready event emission, background population groups and employment, household ledgers/markets/debt/contracts, the action/standing-policy layer, the weighted event pool and monthly report projection, the Unity application shell and adapters, the persistent ink bar and four first-class screens, wax-seal/ordinary confirmations, pause/advance/save/load/replay diagnostics, placeholder portraits, the Phase 9 EditMode/PlayMode presentation-layer test suites (including the 24-month exit-gate soak test), Phase 10's `LivingWorldActor` Background/Noteworthy tiers, rival-house lifecycle, Ancestral Grudges, the shared `ActionSelector`, steward/Council autonomy with real competence/loyalty rolls and Return Reports, the Scheme engine, `RivalDossier` refresh/staleness, and a combined 200-year rival-house/stewardship soak test, and Phase 11's succession/heirs/disputed-inheritance handling, the player-control handoff and Regency, the Dynasty Chronicle, funerals/mourning/Memoria, rules-and-provenance epithets, and a three-succession (one contested) exit-gate soak. **The vertical-slice acceptance test's engineering scaffolding is now in place end to end, the world can act without waiting on the player, and a dynasty survives its own head's death; Phase 12 onward (institutions, geography/travel, and beyond) remains unbuilt.** Treat the assessment table below as the state at the original audit point, not the current state — see "Detailed roadmap" for what has been completed since.

### Phase completion checklist (as of this revision)

- [x] **Phase 0** — Restore a green foundation
- [x] **Phase 1** — Convert design prose into implementable authority
- [x] **Phase 2** — Harden the deterministic simulation kernel
- [x] **Phase 3** — Build saves, content contracts, and developer tooling
- [x] **Phase 4** — Create the bootable headless campaign shell
- [x] **Phase 5** — Implement Characters and Familia
- [x] **Phase 6** — Implement land, villa, goods, buildings, and labor
- [x] **Phase 7** — Implement settlement demographics and background labor
- [x] **Phase 8** — Implement the economy, ledger, and market
- [x] **Phase 9** — Build the player loop: actions, policies, events, report, and first Unity slice
- [x] **Phase 10** — Add delegation, autonomous action, and rival houses
- [x] **Phase 11** — Guarantee dynasty continuity and historical memory
- [ ] **Phase 12** — Build institutions, reputation, law, religion, and public life (items 1-4 of 9 done — see "Item 4 progress" below) ← **next up: item 5**
- [ ] **Phase 13** — Add geography, travel, correspondence, culture, and history
- [ ] **Phase 14** — Add health, disease, disasters, and mobile populations
- [ ] **Phase 15** — Add advanced commerce, property, and public investment
- [ ] **Phase 16** — Add espionage, banditry, military force, and diplomacy
- [ ] **Phase 17** — Add deep relationships, activities, culture, and legacy objects
- [ ] **Phase 18** — Scale content, presentation, art, performance, and release operations

### CI status

The originally reported ambiguity between NUnit's and FsCheck's `Property` attribute in `Pcg32Tests.cs` is resolved in source: both property tests fully qualify the attribute as `[FsCheck.NUnit.Property]`. Both the `standalone` and `content` jobs are green on `main` as of this revision (latest run on commit `e33deb2`, 16 August 2026): `dotnet format`, `dotnet build`/`dotnet test` in Release, the skeleton benchmark, the deterministic-build check, content validation/compilation, and the run-campaign → verify-save → migrate-save → replay exit-gate smoke test all pass from a clean clone. Re-check current CI status before relying on this snapshot, but as of this revision there is no outstanding red condition.

The workflow still runs only standalone .NET validation. It does not yet perform the Unity EditMode, PlayMode, UI, or Unity-build checks promised by the technical baseline (content-migration, save-fixture, and deterministic-replay checks are now covered by the `content` job's exit-gate smoke test).

### Skeleton assessment (original audit point)

This table reflects the state at the original audit commit, not the current state. See "Detailed roadmap" below and the phase completion checklist above for what Phases 0–9 have since delivered (green CI, `WorldState`/command/event envelopes, epoch-aware time, canonical saves with migrations, typed content families, headless campaign, characters/Familia, land/goods/buildings/labor/production, background population/employment, ledger/market, and the full action/policy/event/report/Unity player loop through PR #49).

| Area | Status at audit | Assessment |
| --- | --- | --- |
| Stack selection | Settled | Keep Unity 6.3 LTS + pure C# simulation. Do not reopen the engine decision. |
| Repository structure | Good early baseline | Source, tests, tools, content, benchmarks, design, and Unity paths are sensibly separated. |
| CI | Red and incomplete | Fix the failing tests first; expand checks incrementally with the roadmap. |
| Randomness | Useful primitive | Preserve PCG32, but formalize stream ownership, naming, derivation, versioning, and save compatibility. |
| Monthly loop | Prototype | Registration order is insufficient as a permanent dependency model. Add explicit phases, dependencies, invariants, and transactional failure behavior. |
| Commands/events | Prototype | Arbitrary mutation can occur before events are trusted. Introduce envelopes, stable error codes, change sets, causation, and atomic application. |
| Time | Placeholder | `TotalMonths` needs a defined epoch, BCE/CE mapping, calendar rules, and display conversion. |
| Saves | Contract stub | Implement archive IO, canonical serialization, checksums, migrations, fixtures, and generated-asset references. |
| Content | Placeholder | Replace the one-field catch-all schema with typed definition families and reference validation. |
| Simulation | Not started | No characters, household, land, population, production, market, actors, or gameplay rules exist. |
| Unity presentation | Not started | No playable scene, UI Toolkit shell, view models, or Unity tests exist. |
| Art generation | Correctly isolated | Keep optional and late. Procedural deterministic portraits remain the required baseline. |

## Non-negotiable construction rules

These rules should be recorded as architecture decisions before feature work begins.

1. **The simulation owns truth.** Unity displays projections and submits commands. It never owns authoritative campaign state.
2. **One command path.** Player actions, AI decisions, steward automation, events, debug tools, and migration repairs use the same validated action/command layer whenever they cause equivalent world changes.
3. **Deterministic ordering is explicit.** Never rely on dictionary iteration, Unity frame order, reflection discovery order, or incidental registration order.
4. **Integers or named fixed-point values are authoritative.** Floating point may be used in presentation but not for campaign outcomes.
5. **State is partitioned by scope.** Campaign, region, settlement, household, character, institution, contract, activity, and historical-event state need explicit ownership boundaries and stable IDs.
6. **Knowledge is not omniscience.** The simulation stores truth separately from what the player, a character, or an actor knows. Dossiers, rumors, letters, espionage, and reports read a knowledge/provenance layer.
7. **Fidelity tiers are first-class.** Named characters and noteworthy actors receive detailed simulation; background households, populations, and distant actors use bounded aggregates until promoted.
8. **Every random stream has an owner.** Stream names, algorithms, and seed derivation are versioned. Adding a draw in one system must not perturb another.
9. **Every monthly system declares reads, writes, prerequisites, and phase.** Debug builds verify the declared access set and invariants.
10. **Content is data, rules are code.** Definitions, localized text, portraits, names, traits, buildings, goods, events, and region profiles are authored data. Core invariant enforcement stays in tested code.
11. **Every save-breaking change ships with a migration and permanent fixture.** Deterministic replay from selected golden saves is a merge gate.
12. **Performance is budgeted by subsystem and fidelity tier.** Optimization follows profiles; DOTS remains excluded unless ordinary C# demonstrably cannot meet the agreed budget.
13. **AI is additive, asynchronous, and replaceable.** AI can assist portraits, flavor, or authoring, but it cannot decide authoritative outcomes, invent resources, block a tick, or make a save dependent on a provider.
14. **The vertical slice is a permanent integration target.** Each phase extends one playable campaign rather than building isolated system islands.

## Dependency spine

| Layer | Must exist before | Why |
| --- | --- | --- |
| Green CI + design authority | All feature systems | Otherwise errors and schema contradictions accumulate invisibly. |
| IDs, world state, time, commands, events, RNG | Saves and gameplay | Every later system writes through these contracts. |
| Content registry + saves + headless runner | Feature iteration | Definitions, reproducibility, and long-run testing must precede content volume. |
| Characters + household | Labor, relationships, politics, succession | Named people are the shared record read by nearly every design document. |
| Land + goods + buildings + labor | Population and economy | Markets need supply, demand, ownership, jobs, and stockpiles. |
| Population groups | Market demand and settlement growth | Consumption, employment, migration, and class mobility drive the background economy. |
| Ledger + market | Policies, rivals, institutions, advanced commerce | Later systems require prices, payments, obligations, valuation, and scarcity. |
| Actions + events + monthly report + UI | Playability | A correct simulation is not yet a game until players can understand and influence it. |
| Living-world actors + knowledge | Politics, diplomacy, espionage, rival commerce | These systems require autonomous targets and incomplete information. |
| Geography + travel + correspondence | Regional breadth and distant action | Distance must matter consistently before wide historical content is authored. |
| Stable integrated simulation | Art scale, AI providers, DOTS investigation, release content | Presentation breadth should not conceal an unstable model. |

## Detailed roadmap

### Phase 0 — Restore a green foundation — ✅ COMPLETE

**Outcome:** `main` is trustworthy enough to build upon.

Construction order:

1. Qualify the FsCheck property attributes so the test project compiles.
2. Run the full standalone solution and content compiler in CI; prevent merges while required checks are red.
3. Add formatting/analyzer enforcement, nullable warnings, deterministic-build verification, JSON/Schema validation, and a clean-worktree check for generated artifacts.
4. Make the content compiler run even when tests fail as a separate job, so one failure does not hide another.
5. Add a minimal smoke test that opens the Unity project and compiles assemblies; add licensed Unity tests/builds when credentials and runner policy are ready.
6. Record the exact local developer setup, including an SDK bootstrap check.

**Exit gate:** standalone restore, build, tests, content compilation, and skeleton benchmark all pass from a clean clone. CI is required on pull requests.

**Primary inputs:** `tech-stack.md`, `CONTRIBUTING.md`, `.github/workflows/standalone.yml`, PR #5.

### Phase 1 — Convert design prose into implementable authority — ✅ COMPLETE

**Outcome:** the project has one canonical contract per shared concept and a sized first slice.

Construction order:

1. Create a **design authority registry**. For every shared concept, name the authoritative document and mark older descriptions as summaries, extensions, or superseded text.
2. Resolve known overlaps first:
   - `gens-characters-design.md` owns the universal named-person record;
   - `gens-traits-design.md` owns the trait catalog and supersedes inline trait lists;
   - `gens-familia-design.md` owns lifecycle, household role, birth, marriage, and legitimacy;
   - `gens-romance-sexuality-lineage-design.md` supersedes `gens-romance-seduction-design.md` for implemented romance rules;
   - `gens-starting-regions-design.md` owns the region schema, while individual region documents provide data;
   - `gens-rival-houses-design.md` owns the shared living-world actor abstraction;
   - `gens-activities-activity-engine-design.md` owns multi-phase hosted activities;
   - `gens-events-design.md` owns triggered event delivery and chains.
3. Create architecture decision records for IDs, fixed-point arithmetic, time/epoch, tick phases, event envelopes, command atomicity, deterministic collection ordering, visibility/knowledge, fidelity tiers, save serialization, migrations, content versioning, and UI projection boundaries.
4. Create a cross-system field ledger: field name, type, units, range, owner, readers, writers, persistence, visibility, and migration policy. See `docs/engineering/gens-field-ledger.md`.
5. Convert open questions into three queues:
   - **structural blockers** required before coding a system;
   - **slice tuning** required before its first playable version;
   - **post-slice balancing** intentionally left configurable.
6. Quantify the vertical slice: initial household size, named/background population bands, production yields, needs, wages, prices, construction times, tick order, event cadence, relationship ranges, and report thresholds.
7. Define simulation scale targets for small, normal, large, and soak-test worlds.

**Exit gate:** every field in the first vertical slice has one owner, one unit, one range, and a testable rule. No structural blocker remains hidden in an “Open Questions” section.

**Primary design inputs:** `gens-core-design.md`, `gens-characters-design.md`, `gens-traits-design.md`, `gens-familia-design.md`, `gens-estate-settlement-design.md`, `gens-resources-goods-design.md`, `gens-buildings-design.md`, `gens-settlement-demographics-design.md`, `gens-economy-finance-design.md`, `gens-policies-edicts-design.md`, `gens-events-design.md`, `gens-rival-houses-design.md`, and `gens-starting-regions-design.md`.

### Phase 2 — Harden the deterministic simulation kernel — ✅ COMPLETE

**Outcome:** all future mechanics have safe, versioned primitives.

Construction order:

1. Add stable typed IDs and registries for campaigns, definitions, regions, settlements, plots, households, actors, characters, goods, buildings, contracts, events, and activities.
2. Introduce `WorldState` with explicit state partitions and deterministic indexes.
3. Replace bare `GameDate(TotalMonths)` semantics with a defined epoch, historical-year conversion, BCE/CE display, month-of-year, and overflow-safe arithmetic. Keep the authoritative representation compact.
4. Define tick phases, for example: scheduled commands → lifecycle → production → employment/needs → markets/ledger → relationships/actors → hazards → events → reports → invariant checks.
5. Make systems declare ID, phase, dependencies, read set, and write set. Topologically sort once and fail on missing or cyclic dependencies.
6. Replace mutation-by-convention with a command envelope and atomic change set. Include command ID, actor, submitted date, causation, validation error code, emitted events, and deterministic sequence number.
7. Create a domain-event envelope with event ID, type/version, occurred date, subject IDs, visibility/provenance, causation, and payload.
8. Formalize random-stream registration and seed derivation. Persist algorithm version and stream states; test that unrelated system changes do not perturb existing streams.
9. Add invariant hooks and deterministic state hashing after commands and ticks.
10. Add query/read-model interfaces so UI code cannot mutate domain objects.

**Exit gate:** the same seed plus the same ordered commands produces identical event logs and state hashes across repeated headless runs. A rejected or failed command leaves state and RNG unchanged.

### Phase 3 — Build saves, content contracts, and developer tooling — ✅ COMPLETE

**Outcome:** campaigns and authored definitions are reproducible, inspectable, and migratable.

Construction order:

1. Implement canonical JSON serialization rules and stable ordering.
2. Implement atomic `.gens` archive writing/reading, manifest checksums, world/history entries, RNG states, content-pack hashes, and generated-asset references.
3. Build a migration registry and permanent fixtures beginning with save version 1.
4. Replace the catch-all content schema with typed definition families: goods, buildings, traits, policies, events, regions, cultures, religions, names, and presentation metadata.
5. Add cross-file reference validation, duplicate-ID detection, enum/range validation, inheritance/patch rules, localization-key checks, and deterministic normalized output.
6. Add a content manifest with schema version, compiler version, source hashes, and dependency order.
7. Build developer commands for validate, compile, inspect definition, diff normalized content, run headless campaign, replay commands, verify save, and migrate save.
8. Add golden content packages and save fixtures to CI.

**Exit gate:** a compiled content pack can bootstrap, save, load, replay, migrate, and reproduce the same state hash. Invalid references and schema violations fail before Unity starts.

### Phase 4 — Create the bootable headless campaign shell — ✅ COMPLETE

**Outcome:** Gens can run months without Unity even though most systems are still empty.

Construction order:

1. Define `CampaignConfig`: seed, start date, ruleset, content manifests, accessibility/content toggles, difficulty, and region/start-profile IDs.
2. Implement bootstrap/factory logic that constructs world, region, settlement, household, RNG streams, scheduler, and initial history.
3. Add a headless console runner with commands for new campaign, advance one month, advance N months, submit a command, dump report, save, load, and compare hashes.
4. Add scheduled actions and a calendar queue for future-dated work.
5. Add a generic monthly report projection from domain events, with importance, grouping, acknowledgement state, and links to involved entities.
6. Add debug inspectors for state partitions, definitions, event log, RNG state, and invariant failures.

**Exit gate:** a seed-defined empty campaign advances for 1,200 months, saves/loads repeatedly, and reproduces identical hashes without unbounded allocations or history growth.

**Primary design inputs:** `gens-roman-calendar-design.md`, the time-scale sections of `gens-core-design.md`, and the delivery/report requirements in `gens-events-design.md`.

### Phase 5 — Implement Characters and Familia — ✅ COMPLETE

**Outcome:** the world contains persistent named people who age, relate, work, and die.

Construction order:

1. Implement the canonical `Character` identity, lifecycle, legal status, social class, culture, location, attributes, skills, condition, household membership, and visibility record.
2. Implement deterministic name/appearance generation and a `CharacterVisualProfile`; generate procedural portrait recipes but no required raster art.
3. Implement lifecycle transitions, aging, baseline health, fatigue, mortality, permanent injury hooks, birth, parentage, legitimacy, marriage history, and death records.
4. Implement traits as definition references, opposed-pair enforcement, personality axes, and a small representative slice of the catalog. Do not hand-author all 234 traits yet.
5. Implement relationships as sparse directed records with opinion, bonds, provenance, decay rules, and last meaningful interaction.
6. Implement household roles and duty assignments, competence checks, capacity, availability, and location conflicts.
7. Implement deterministic lazy instantiation and promotion from aggregate/background people to named characters.
8. Add invariant and property tests for genealogies, lifecycle gates, trait exclusivity, relationship asymmetry, and referential integrity.

**Exit gate:** a 6–10 named-person household can run for multiple generations in headless mode; births, aging, assignments, relationships, deaths, and promotion are deterministic and save-safe.

**Primary design inputs:** `gens-characters-design.md`, `gens-traits-design.md`, `gens-familia-design.md`. Use only the lifecycle/lineage minimum from `gens-romance-sexuality-lineage-design.md` and the position minimum from `gens-companions-court-positions-design.md` at this stage.

### Phase 6 — Implement land, villa, goods, buildings, and labor — ✅ COMPLETE

**Outcome:** the household owns a place, stores resources, assigns labor, and produces goods.

Construction order:

1. Implement region/settlement/plot/holding boundaries without broad region content.
2. Implement ownership, occupancy, terrain/features, land condition, capacity, and acquisition hooks.
3. Implement stockpiles with quantity, quality/condition where required, capacity, reservation, spoilage hooks, and provenance for exceptional objects later.
4. Implement building definitions, instances, construction queues, prerequisites, terrain gates, tiers, condition, upkeep, staffing slots, and production recipes.
5. Implement villa stage and room instances as a specialized holding/building layer; defer full decorative content.
6. Implement labor assignments, availability, skill effects, regimen/policy hooks, output, fatigue, injury, and basic flight/manumission state transitions.
7. Implement a small production network: three compact chains, one storage constraint, one construction queue, and maintenance.
8. Emit complete ledger-ready production, consumption, construction, and labor events even before the economy consumes them.

**Exit gate:** one estate transforms inputs, labor, time, and maintenance into deterministic outputs for 120 months; shortages, invalid assignments, and interrupted construction resolve consistently.

**Primary design inputs:** `gens-estate-settlement-design.md`, `gens-villa-design.md`, `gens-resources-goods-design.md`, `gens-buildings-design.md`, `gens-labor-slavery-design.md`.

### Phase 7 — Implement settlement demographics and background labor — ✅ COMPLETE

**Outcome:** named characters sit atop a living aggregate population rather than an empty map.

Construction order:

1. Implement population groups by settlement, occupation/class, legal status, culture, wealth band, needs profile, employment, housing, contentment, and health exposure.
2. Implement job capacity from buildings and background economic capacity.
3. Implement employment matching, wages offered, underemployment, and named/background labor boundaries.
4. Implement needs demand, consumption, housing capacity, contentment, growth, mortality modifiers, migration, promotion/demotion, and assimilation hooks.
5. Implement promotion from a pop group into a named character only when an interaction, office, exceptional achievement, relationship, or event requires it.
6. Add conservation tests so population changes have causes and no group silently duplicates or disappears.

**Exit gate:** population, employment, housing, needs, and migration reach stable or explainably changing equilibria in seeded 50-, 200-, and 1,000-year headless runs.

**Primary design inputs:** `gens-settlement-demographics-design.md`, `gens-population-wealth-purchasing-power-design.md`, and the aggregate/named boundary in `gens-characters-design.md`.

### Phase 8 — Implement the economy, ledger, and market — ✅ COMPLETE

**Outcome:** production and population interact through money, prices, obligations, and scarcity.

Construction order:

1. Implement double-entry-style household and actor ledgers using integer minor units.
2. Define accounts and transaction types for treasury, wages, sales, purchases, taxes, upkeep, construction, debt, gifts, contracts, and transfers.
3. Implement orders, settlement markets, stock availability, price formation, clearing order, unsatisfied demand, and bounded price changes.
4. Feed production supply and population/household needs into the market.
5. Implement household purchasing, sales, inventory reservation, and monthly statements.
6. Implement wages, rents, taxes, basic debt/interest, default, insolvency, valuation, and net-worth bands.
7. Implement one market contract and one trade-route stub using the same ledger and reservation contracts.
8. Add accounting invariants, conservation tests, anti-overflow tests, and long-run economic telemetry.

**Exit gate:** every quantity and coin movement reconciles; prices react to scarcity without numerical explosion; save/replay preserves exact ledgers; the slice survives boom, shortage, debt, and insolvency scenarios.

**Primary design input:** `gens-economy-finance-design.md`, with demand from `gens-population-wealth-purchasing-power-design.md` and definitions from `gens-resources-goods-design.md`.

### Phase 9 — Build the player loop: actions, policies, events, report, and first Unity slice — ✅ COMPLETE

**Outcome:** the headless simulation becomes a comprehensible game.

Construction order:

1. Build an action-definition layer that declares targets, eligibility, costs, duration, reservations, validation, AI utility hooks, confirmation severity, and result projection.
2. Implement a minimal standing-policy model, policy change command, cooldown, household modifier projection, and one funded action.
3. Implement the weighted event pool, scripted triggers, event instances, options, delayed stages, expiry, AI/NPC resolution, and visibility.
4. Generate the monthly report entirely from domain events and read models; support grouping, priority, automation summaries, and drill-down.
5. Create the Unity application shell and adapters without referencing Unity from the simulation package.
6. Build the persistent ink bar and four first-class screens: household roster, estate/settlement, monthly report, and character detail.
7. Add wax-seal confirmation for consequential decisions and ordinary confirmations for reversible actions.
8. Implement pause/advance, command submission, save/load, deterministic replay diagnostics, and placeholder/procedural portraits.
9. Add EditMode tests for adapters and PlayMode/UI tests for new campaign → assign labor → build/produce → change policy → advance month → inspect report → save/load.

**Vertical-slice contents:** one representative start profile, one estate, 6–10 named household members, background population groups, three compact production chains, one market/contract, a small policy set, one overseer, one rival seed, and three compact event chains. This is an integration target, not the final content ceiling.

**Exit gate:** a player can complete the loop for 24 months without debug tools and can explain every important change from the UI and monthly report.

**Primary design inputs:** `gens-core-design.md`, `gens-policies-edicts-design.md`, `gens-events-design.md`, and the visual/UI sections of the core and villa documents.

### Phase 10 — Add delegation, autonomous action, and rival houses — ✅ COMPLETE

**Outcome:** the world acts without waiting for the player and the household can be governed indirectly. Delivered across 15 packages: the `LivingWorldActor` core/registry and Background/Noteworthy fidelity tiers; `HouseStanding`/`RivalDossier`/`RegionalFamiliesEntry` storage; rival-house creation (ancient seed, *novus homo*, cadet branch) with lazy head-Character generation; the shared `ActionSelector` reused by both the Noteworthy `RivalAmbitionSystem` and the steward autonomous decision loop; the Background-tier abstract drift tick with its per-tick processing cap; Ancestral Grudge formation/blocking/decay; `StewardshipAssignment` with autonomy-level commands; real steward competence (Stewardship attribute) and Loyalty-risk rolls driving Skimming/Embezzlement/Active-Sabotage incidents through the ordinary ledger/policy command paths, folded into a `ReturnReport` on assignment end; house extinction; the actor-agnostic `Scheme` interaction engine; and `RivalDossier` refresh-on-genuine-contact with a staleness display helper. A combined 200-year exit-gate soak test exercises rival houses, an active `StewardshipAssignment`, and a `Scheme` together.

Construction order:

1. Implement reusable AI considerations and action selection against the same action definitions used by the player.
2. Implement steward/council autonomy levels, always-held decisions, budgets, standing orders, competence, loyalty risk, and return reports.
3. Implement the `LivingWorldActor` framework and background/noteworthy fidelity tiers.
4. Implement rival-house creation, identity, holdings, standing trend, wealth/dignitas bands, regional visibility, promotion/demotion, lazy character generation, and retirement/extinction.
5. Implement actor-to-actor standing, alliances, rivalry, feuds, dossiers, information staleness, and knowledge provenance.
6. Implement individual interactions and a first reusable scheme engine for both player and NPC actions.
7. Add simulation budgets so background actors cannot expand work linearly without bounds.

**Exit gate:** several rival actors and a delegated household survive a 200-year soak; their actions use legal commands, generate reports/rumors according to visibility, and remain inside tick budgets.

**Primary design inputs:** `gens-steward-council-auto-management-design.md`, `gens-rival-houses-design.md`, interaction/scheme sections of `gens-characters-design.md`, and `gens-notable-households-design.md`.

### Phase 11 — Guarantee dynasty continuity and historical memory — ✅ COMPLETE

**Outcome:** death changes play rather than ending the simulation arbitrarily.

**Item 1 progress:** heirs, eligibility, designation, adoption, disputed succession, asset/obligation
transfer, and household extinction are implemented (`src/Gens.Simulation/Succession/`) —
`HouseholdHeadship`/`HeirDesignation`/`SuccessionDispute` state, `SetPreferredHeirCommand`/
`DeclareHeirCommand`/`DisownHeirCommand`/`AcknowledgeIllegitimateChildCommand`/`AdoptChildCommand`,
and the `SuccessionHandoffSystem`/`SuccessionDisputeResolutionSystem` monthly pair (dispute →
favor-score resolution → optional splinter household). Asset/obligation transfer reuses the existing
household-keyed ledger/debt model rather than a new mechanism; item 6's fixtures (ordinary
inheritance, contested inheritance, adoption, debt inheritance, absent heirs, extinction) are covered
in `tests/Gens.Simulation.Tests/Succession/`.

**Item 2 progress:** the player-character handoff is implemented (`src/Gens.Simulation/Succession/PlayerControl.cs`,
`RegencySystem.cs`, `PlayerControlHandoffSystem.cs`) — `PlayerControlState` (household, controlled
Character or none, and one of `DirectHead`/`RegentInTrust`/`AutoManaged`/`Extinguished`, §6.2) is now
first-class state distinct from `HouseholdHeadship`, established explicitly via
`EstablishPlayerControlCommand` and kept in sync monthly by `PlayerControlHandoffSystem`, which only
writes and emits `PlayerControlChangedEvent` when the computed target actually differs from what is
stored. Closes a real gap left in item 1: `SuccessionHandoffSystem`'s minor-heir branch previously fell
through to an outright transfer with no Regent at all when there was no surviving spouse, leaving a
minor head ungoverned; `RegencySystem` now appoints a non-family Regent (the household's own
highest-Stewardship living adult, via a `Regency`-context `StewardshipAssignment`, reusing Phase 10's
Steward/Council auto-management wholesale per §6.2) when one is needed, and ends the Regency once the
heir comes of age — the "future Regency-ends-when-the-heir-comes-of-age system" `SuccessionHandoffSystem`'s
own doc comment named as out of item 1's scope. Both new systems run in the `RelationshipsActors` phase
after `succession.handoff` and `succession.disputeResolution` (and, for `PlayerControlHandoffSystem`,
after `succession.regency` too), so a month's headship changes are always fully settled before player
control is recomputed. Covered in `tests/Gens.Simulation.Tests/Succession/PlayerControlTests.cs`,
including a save/load round trip and the deterministic state hash. Items 5–6 (epithets/titles,
succession fixtures) remain.

**Item 3 progress:** the Dynasty Chronicle is implemented (`src/Gens.Simulation/Chronicle/`) —
`ChronicleEntry` (category, significance tier, prose, linked Characters, source system/event, pin,
player annotation) and `GenerationalChapter` (one per head's tenure) are new `WorldState` partitions.
`ChronicleProjector` reads a month's already-emitted domain events (never raw state, per ADR 0007) and
maps the succession-cluster events (headship established/transferred/extinguished, disputes
opened/resolved, splinter households), births/deaths/marriages, a rival house's own extinction, the
Insolvency ladder's terminal "Fall of the House" rung (`gens-economy-finance-design.md` §9 rung 5,
closing the gap `InsolvencySystem`'s own doc comment named), and a discovered-and-escalated Scheme onto
§6's default tier mapping; `ChronicleGenerationSystem.Generate` (deliberately not an `IMonthlySystem` —
see its own doc comment) persists the resulting entries, opens/closes `GenerationalChapter`s on
headship transitions, and cross-posts Major/Legendary entries to a rival's own `RivalDossier` (§9) —
retiring that record's former plain-`string` `RecentChronicleEntries` stopgap in favor of real
`RuntimeId<ChronicleEntry>` references. `SetChronicleEntryPinnedCommand`/`AnnotateChronicleEntryCommand`/
`AddChronicleNoteCommand` cover §7's player pinning/annotation/diary-note tools, and
`Queries/ChronicleQuery` projects one household's filtered, chapter-grouped read (§4), excluding
Minor-tier entries from the default view unless pinned, per §3. The design doc's §5 Milestone-as-
goal-tracker mechanism is deliberately out of this item's scope — the roadmap line above never names
it — and `buildings.constructionCompleted` is the one named Chronicle-worthy event left out for now,
since nothing yet resolves a Holding back to an owning Household. Covered in
`tests/Gens.Simulation.Tests/Chronicle/ChronicleTests.cs`, including a save/load round trip and the
deterministic state hash.

**Item 4 progress:** funerals, mourning, and Memoria are implemented (`src/Gens.Simulation/Funerary/`)
— `MemoriaState` (a household's running Memoria total) and `HouseholdPolicyState`-shaped sparse
per-household partitions, `FuneralRecord` (a `RuntimeId`-keyed entity kind, "funeral" tag, kept once
held like `SuccessionDispute`), and `MourningPeriod` are new `WorldState` partitions, following the
exact sparse-per-household/kept-entity shapes those two named types already use. `FuneralOpeningSystem`
detects a death directly from raw `Character.IsAlive` state (matching `SuccessionHandoffSystem`'s own
convention — both run in the `RelationshipsActors` phase, strictly after `CharacterLifecycleSystem`'s
earlier `Lifecycle` phase, so a month's deaths are already visible) and opens a `Pending` `FuneralRecord`
plus starts (or extends) a household `MourningPeriod` for *any* household member's death, not only a
tracked head's — broader than Succession's own "who inherits" question, per §2's "every death... now
routes through the same real sequence". It declares no `Prerequisites` against `succession.handoff`
(the two never read each other's writes) but is named `funerary.funeralOpening` specifically so it
sorts ordinally before `succession.handoff` in the same phase's deterministic tiebreak — the design
doc's own sequencing intent without touching Succession's own declared `Prerequisites`.
`ChooseFuneralTierCommand` (the funeral-tier choice command the task calls for) and
`FuneralAutoResolutionSystem` (a stale `Pending` funeral resolves at a Modest default after two months,
matching `SuccessionDisputeResolutionSystem`'s own "resolve automatically after N months" shape, so a
background/NPC household is never stuck) share one `FuneralResolution.Hold` helper for the actual
Treasury-cost-against-Memoria-yield trade (§2.2), scaling a Grand funeral's yield by the household's
own existing Major/Legendary Dynasty Chronicle entry count (§2.2, §6.1) — Dignitas is deliberately left
out of that trade entirely, since (per `DeclareHeirCommand`'s own doc comment) no personal or household
Dignitas stat exists yet, only `LivingWorldActor.Dignitas` for rival houses. `ManesObservanceSystem` is
the ongoing Manes-cult/*Parentalia* Memoria trickle the task names directly, mirroring
`FundFestivalCommand`'s Rites-Budget-adjacent shape: every February it tries a small automatic Treasury
draw for every tracked household, crediting Memoria (base gain plus a capped per-Major/Legendary-entry
trickle, realizing item 3's own "a Dynasty Chronicle entry for any ancestor... contributes a small,
permanent Memoria trickle" note) on success or applying a small Memoria loss on insufficient funds — no
separate "record observance" command needed since Travel does not exist yet to model the design's other
named skip reason (absence). `BreakMourningEarlyCommand` sets `MourningPeriod.BrokenEarly` but cannot
fire the real consequence the design doc names (a Scandal, Phase 12, not yet built) — the flag is the
documented hook a future Scandal integration reads directly. `FuneralHeldEvent` and
`MourningBrokenEarlyEvent` both route into `ChronicleProjector`'s existing event-to-entry mapping, so a
held funeral and a broken mourning period actually appear in the Dynasty Chronicle, closing the loop
§8 describes. Three deliberate scope cuts, each named in its own file's doc comment the way item 3's own
`buildings.constructionCompleted` gap was named: (1) burial method is a hardcoded `Cremation` default
rather than the design's culture/faith-tenet-driven soft drift (§3) — Cultures and Religions of the
Known World (Phase 13) do not exist yet to own that tenet system, and this item does not attempt a
stand-in for it; (2) the widow's *tempus lugendi* (§4.2) and the settlement-scale *iustitium* (§4.3) are
both left unbuilt rather than half-built, since the first needs Romance, Sexuality & Lineage's
remarriage-timing machinery and the second needs Politics & Patronage's Prominence gate, neither of
which exists yet; (3) the *laudatio funebris* (§7) is skipped entirely — it needs Rhetoric/orator
mechanics this codebase does not have. Covered in `tests/Gens.Simulation.Tests/Funerary/FuneralTests.cs`,
including a funeral raising Memoria (scaled by ancestral Chronicle achievement at Grand tier), a skipped
*Parentalia* lowering it against a well-funded one raising it, a full funeral/mourning/Memoria state
round trip through save/load, and the deterministic state hash staying stable.

**Item 5 progress:** rules-and-provenance epithet/title awards are implemented
(`src/Gens.Simulation/Epithets/`) — `Agnomen` (`AgnomenType`/`AgnomenGrantMethod`, a Character's real
earned name with `SourceChronicleEntryIds`/`SourceSuccessionDisputeId` provenance rather than free
text), `InheritedCognomenDecision` (§5's real, permanent adopt-as-family-cognomen decision), and
`DynasticEpithet` (§6's whole-house flavor-tier reputation text, derived from accumulated Major/
Legendary Dynasty Chronicle entries) are new `WorldState` partitions, following Chronicle's own kept-
entity/sparse-per-household conventions. `EpithetGenerationSystem.Generate` — deliberately not an
`IMonthlySystem`, for the identical reason `ChronicleGenerationSystem` isn't one (see that type's own
doc comment) — is invoked immediately after it at the same three call sites (`AdvanceCommand`,
`CampaignShell.Submit`/`AdvanceMonth`), reading the same-month's own newly-recorded Chronicle entries to
award the `Magnus` achievement agnomen (a Character's own personally-linked Major/Legendary Chronicle
entries crossing a threshold) and the `Felix` agnomen (prevailing in a resolved
`SuccessionDisputeResolvedEvent`), and to set a household's `DynasticEpithet` off its own accumulated
Major/Legendary record once it crosses a threshold. `AdoptAgnomenAsCognomenCommand` records §5's real
decision and, via `InheritedCognomenResolver`, actually changes how the next generation is named:
`BirthCharacterCommand` now overrides a newborn's freshly generated cognomen with the household's
adopted Agnomen name when one has been adopted, realizing §5's own "changes how every subsequent
generation is actually named" rather than only recording the decision as inert bookkeeping. Three
deliberate scope cuts, each named directly in `Agnomen`'s own doc comment the way item 4's own burial-
method simplification was named: (1) `AgnomenType.Conquest` is modeled but never minted — it needs
Military & Combat and Diplomacy with Non-Roman Peoples (Phase 16) to resolve a real campaign outcome,
neither built yet; (2) `AgnomenType.CrowdGivenNickname` is likewise modeled but never minted — Fame does
not exist anywhere in this codebase yet (Games & Spectacle/Celebrities, Phase 17); (3)
`AgnomenType.MockingNickname` (§7's real "the name sticks anyway" case) is modeled but never minted —
Scandal (Phase 12) does not exist yet to source one from, and `IsSuppressible`/Damage Control accordingly
never applies. `DignitasEffect`/`FameEffect` are likewise always `null`: no personal or household
Dignitas stat exists on `Character` yet (only `LivingWorldActor.Dignitas` tracks a bare int for rival
houses, the same gap `DeclareHeirCommand` already documents), and Fame does not exist at all. Every
Agnomen this pass mints therefore carries `AgnomenGrantMethod.OrganicCrowdOrigin`, never
`FormalSenateOrCuriaGrant` — a real Senate/Curia vote needs Politics & Patronage (Phase 12) to actually
convene one. Covered in `tests/Gens.Simulation.Tests/Epithets/EpithetTests.cs`, including achievement and
succession-victory awards, award idempotency, dynastic epithet threshold crossing, cognomen-adoption
validation and its effect on a subsequent birth, a save/load round trip, and the deterministic state hash
staying stable.

**Item 6 progress:** the per-scenario succession fixtures the construction order calls for (ordinary
inheritance, contested inheritance, adoption, debt inheritance, absent heirs, and extinction) were
already delivered alongside item 1 (`tests/Gens.Simulation.Tests/Succession/SuccessionTests.cs`) — what
remained was proving the phase's own exit gate itself: the whole system stack wired together across a
real multi-generation run rather than one system exercised in isolation per test.
`tests/Gens.Simulation.Tests/ExitGate/SuccessionDynastyExitGateTests.cs` closes that gap, matching how
`RivalHousesAndStewardshipSoakTests` closed out Phase 10: a single household survives three real
successions (a contested Gen0→Gen1 dispute resolved by `SuccessionDisputeResolutionSystem`, then two
ordinary Gen1→Gen2 and Gen2→Gen3 handoffs) with `SuccessionHandoffSystem`, `SuccessionDisputeResolutionSystem`,
`RegencySystem`, `PlayerControlHandoffSystem`, `FuneralOpeningSystem`, `FuneralAutoResolutionSystem`, and
`ManesObservanceSystem` all ticking together, plus `ChronicleGenerationSystem.Generate`/
`EpithetGenerationSystem.Generate` run the same way `CampaignShell.AdvanceMonth` and the content-compiler
CLI's `AdvanceCommand` already pair them. Across the run it asserts: the original loan still rides the
same Household through every handoff untouched (§6's "asset and obligation transfer" free-by-construction
claim); the Dynasty Chronicle records all three headship transitions plus the three funerals; all three
generations' funerals are actually held and Memoria moves off zero; the contested winner is awarded the
Felix agnomen; `PlayerControlHandoffSystem` follows control all the way to the third-generation head;
relationship referential integrity holds after three generations of deaths; and both a mid-run and a
final save/load round trip reproduce the exact same state hash. Head deaths are driven directly (the
same `DeathRecord`-flipping technique the isolated fixtures already use) rather than left to
`CharacterLifecycleSystem`'s own mortality roll, since a real roll cannot be scheduled to land on three
chosen generations inside a bounded run — that system's own soak coverage already lives in Phase 5's
`FamiliaHouseholdSoakTests`.

Construction order:

1. Implement heirs, eligibility, designation, adoption, wills/inheritance rules, disputed succession, asset and obligation transfer, and household extinction.
2. Implement the player-character handoff while preserving the household/world distinction.
3. Implement Chronicle entries from domain events, significance tiers, chapters, filters, pins, annotations, and rival entries.
4. Implement funerals, mourning, memoria, ancestor records, and memorial/legacy hooks.
5. Implement epithet/nickname/title awards from rules and provenance rather than free text.
6. Add succession fixtures for ordinary inheritance, contested inheritance, adoption, debt inheritance, absent heirs, and extinction.

**Exit gate:** the vertical-slice campaign can survive at least three successions, including a contested case, while ledgers, property, relationships, history, and saves remain consistent.

**Primary design inputs:** `gens-succession-dynasty-design.md`, `gens-dynasty-chronicle-design.md`, `gens-ancestor-veneration-funerary-customs-design.md`, `gens-epithets-nicknames-titles-design.md`.

### Phase 12 — Build institutions, reputation, law, religion, and public life — 🔶 IN PROGRESS (item 4 of 9)

**Outcome:** household choices operate inside a social and political order.

**Item 1 progress:** the shared Dignitas/reputation/favor-obligation primitive is implemented
(`src/Gens.Simulation/Reputation/`) — `HouseholdReputation` (a household-level Dignitas total) and
`FavorObligation` (a generic, kind-agnostic favor/obligation ledger between two Characters) are new
`WorldState` partitions, following `MemoriaState`'s sparse-per-household and `FuneralRecord`'s
kept-entity conventions respectively. `HouseholdReputation` is the field a long chain of earlier phases'
own doc comments named as missing outright — `DeclareHeirCommand` (Phase 11 item 1), `Agnomen`'s
`DignitasEffect` and `FuneraryCatalog`'s Grand-tier trade (Phase 11 items 4-5), and `InkBarQuery`'s
reserved ink-bar slot (Phase 9 item 6) all say some version of "no personal or household Dignitas stat
exists yet, only `LivingWorldActor.Dignitas` for rival houses" — because the player's own `Household` is
never itself a `LivingWorldActor` and so never had anywhere to keep one. `AdjustDignitasCommand` is the
one command path (rule 2) every future Dignitas-moving trigger — a Politics & Patronage Salutatio, a won
magistracy, a Legal & Court verdict, a Scandal, a defaulted debt (`gens-politics-patronage-design.md`
§2) — is meant to route through, exercised directly by this item's own tests standing in for those
future callers. `GrantFavorCommand`/`SettleFavorCommand`/`FavorExpirationSystem` give item 2's own
Clientela system (§4.2's "a favor drawn on too often without reciprocation costs the relationship-web
opinion... Clientela is reciprocal, not a free resource tap") the generic ledger shape to build on,
without this item building Clientela itself or deciding how a specific favor should move
`Relationship.Opinion` — that policy judgment is left to whichever system actually knows what kind of
favor it was. Audience-specific visibility is real rather than a separate invented model: a
`DignitasChangedEvent` is always `Visibility.Public` (per `gens-celebrities-influential-figures-design.md`
§4, Dignitas is "legible to Curiales, Rival Houses, and the political class" by definition, not a fact
that has to propagate through contact first), while `FavorGrantedEvent`/`FavorSettledEvent`/
`FavorExpiredEvent` are all `Visibility.Private` to the two Characters involved — the same `Visibility`
mechanism every other system in this codebase already reads knowledge through (ADR 0008), not a parallel
audience model built just for this item. `InkBarQuery`'s Dignitas field now reads
`DignitasResolver.Current` instead of a hardcoded 0 — a safe, non-mutating read wired in immediately;
`Agnomen.DignitasEffect` and the Funerary Grand-tier trade are deliberately **not** retrofitted, since
both are already-shipped, already-tested Phase 11 items built and asserted against a null/absent value,
and reopening them is out of this item's scope. Two further, explicitly named cuts: Fame is not built at
all — per `gens-games-spectacle-design.md` §2 and the Celebrities document's own §1, Fame is a universal
0-100 Character field owned by Games & Spectacle (Phase 17) and widened by Celebrities & Influential
Figures, neither built — so this item does not invent a stand-in field, matching `Agnomen`'s own
precedent for `FameEffect` staying permanently `null` until that system actually exists; and the
richer, audience-differentiated "how famous is this Character to the crowd versus the political class"
question the task description gestures at has no real answer to give yet for exactly that reason — there
is no Fame field for a wider-public audience to read, only Dignitas, which the political-class audience
already reads directly and unconditionally, so building a bespoke "perceived Dignitas per audience"
query on top would be inventing complexity the design docs never ask for. Covered in
`tests/Gens.Simulation.Tests/Reputation/ReputationTests.cs`, including Dignitas accumulation/negative
totals, all three favor-lifecycle commands and their validation, `FavorExpirationSystem`'s age-gated
lapse, the `InkBarQuery` wiring, a save/load round trip, and the deterministic state hash staying stable.
Items 2-9 (Clientela/offices, Religion, Legal, Crime & Punishment, Interest Groups/Collegia, Scandal,
Fame/Celebrity, and full Edicts) remain — this item alone does not close Phase 12's own exit gate, which
needs those systems' own legal/religious/factional consequences to actually exist.

**Item 2 progress:** Clientela and the four local Magistracies land as two new domains,
`src/Gens.Simulation/Clientela/` and `src/Gens.Simulation/Magistracies/`. Clientela (§4): `ClientelaEntry`
is a sparse, per-Character roster-membership partition (`RecruitClientCommand`/`DismissClientCommand`),
layered on top of Familia's existing `BondTag.Patron`/`BondTag.Client` tags via a direct relationship-web
write mirroring `RecordInteractionCommand`'s own shape. `CallInClientFavorCommand` is exactly the
integration item 1's own `FavorObligation` doc comment named as deliberately deferred: it opens and
immediately resolves a favor through that item's own `FavorObligation`/`FavorGrantedEvent`/
`FavorSettledEvent` types rather than inventing new bookkeeping, and adds §4.2's own reciprocity rule — a
call-in inside `ClientelaCatalog.FavorCooldownMonths` of the last one costs the client's opinion of the
patron (`OverdrawnOpinionPenalty`), a spaced-out one costs nothing. `HouseholdInfluence` (§4.4) is a
zero-floored spendable resource, unlike Dignitas's deliberately-unclamped total; `InfluenceCycleSystem`
generates it monthly from roster size/quality/held office and decays it monthly, and
`HoldContestedElectionCommand` (below) is its one spend path — Scheme-spending (§9) is not wired, see
below. `SalutatioSystem` (§4.3) writes its Dignitas half through `AdjustDignitasCommand` — the exact
future caller that command's own item-1 doc comment reserved — and its Influence half directly, since no
shared Influence-moving command exists yet for anything else to route through. `ClientPoachingSystem`
(§4.5) targets a real `LivingWorldActor` rather than a placeholder stranger, since Rival Houses already
shipped in Phase 10; it only poaches into an Actor whose head Character has already been lazily
generated, deliberately not triggering that generation itself (it needs a NamePool/culture/settlement
context this generic system has no principled way to supply on a rival's behalf). Faction (§3.1) is a
new `CharacterFactionAlignment` sparse partition (`SetCharacterFactionCommand`) rather than a field added
to `Character` itself, avoiding a retrofit of that record's already-large constructor and every existing
call site; the household-level Faction drift §3.1 also describes is not built (no accumulated-choices
ledger exists yet to drive it). Local Magistracies (§5): `MagistracyRecord` covers the four achievable
offices (Decurion/Aedile/QuaestorLocal/Duumvir) only — the Rome-track values §11's own sketch lists
alongside them (`quaestorRoman`, `praetor`, `consul`, etc.) are omitted from the enum entirely rather than
included-but-unreachable, since §6/§7 are out of this item's scope. `AppointDecurionCommand` seats the
base entry point directly (citizenship + a Dignitas threshold); `HoldContestedElectionCommand` resolves
every office above it as a weighted comparison (Diplomacy + household Dignitas + spent Influence, plus a
Faction-alignment thumb against the Curia majority) but takes an already-resolved challenger/incumbent
Character rather than generating one inline, matching how `LivingWorldActorHeadGenerator`/
`PromoteToNamedCommand` already generate for their own callers. §5.6's Curia body is deliberately not a
second stored registry — `MagistracyResolver.ActiveCuriaSeats` derives it from active Decurion records
directly, since that already is the seat roster. `PairDuumvirsCommand` links two independently-won
Duumvir seats and writes `BondTag.CoMagistrate`; `FundAedileWorksCommand` gives the Aedile's "occasional
real duty" three named choices with real, if placeholder-sized, Dignitas consequences (the paired
Contentment half is not wired — out of this item's Politics & Patronage-only scope).
`MagistracyTermSystem` applies every active office's monthly passive Dignitas trickle, auto-renews an
unchallenged term at its annual anniversary, and strips a held office on Insolvency (reading
`InsolvencyState.Stage` directly, since `InsolvencySystem` itself doesn't yet apply its own flagged
`officeOrCensusLoss` consequence) with the extra §5.7 Dignitas penalty; it also vacates a seat on the
holder's death, an addition not named by §5.7 itself. **Explicitly not built, matching this item's own
narrow Politics & Patronage-only scope:** the Curia (Buildings §4.10) and Mint (§5.4) building gates —
no such building types exist anywhere in `Gens.Simulation.Buildings` yet, so `AppointDecurionCommand`/
`PairDuumvirsCommand` check only the Dignitas/office half of their gates and note the building check as
future wiring; a Legal & Court conviction loss-of-office route (§5.7) — kept as an unreachable
`MagistracyLossReason.LegalConviction` enum value, matching item 1's own `Agnomen.FameEffect` precedent
for "the field exists, nothing can set it yet"; §9 Scheming's "undermine a rival candidate" hook onto an
election's own score inputs — the Scheme engine now exists (Phase 10 packages 11-12,
`Gens.Simulation.Interactions`), so this is a real, buildable gap for a future pass rather than a
blocked one, just not built here; and rival-candidate/Curiales-promotion generation itself, left to the
existing generators named above rather than duplicated inline. Covered in
`tests/Gens.Simulation.Tests/Clientela/ClientelaTests.cs` and
`tests/Gens.Simulation.Tests/Magistracies/MagistracyTests.cs`, including recruitment/dismissal and their
bond-forming writes, the overdrawn-favor opinion cost, Influence generation/decay, the Salutatio's two
outcomes, a poaching event actually flipping a client's bond to a real rival Actor, Decurion appointment
and Curia-capacity limits, a contested election with a real score-driven outcome, Duumvir pairing, all
three Aedile funding choices, term auto-renewal, Insolvency- and death-driven loss of office, and a
save/load round trip with the deterministic state hash staying stable across every new partition.

**Item 3 progress:** Religion lands as a new domain, `src/Gens.Simulation/Religion/` (§2-§6 of
`gens-religion-design.md`). `HouseholdReligion` (§2, §10's own data-model sketch) is a new sparse,
per-household `WorldState` partition pairing a chosen `PatronDeity` (all twelve of §2.1's "real, viable
picks" — Jupiter through Bacchus, a closed, code-defined enum since §2.1 frames its own list as
exhaustive, not representative) with a running `Favor` total — deliberately unclamped, matching
`HouseholdReputation.Dignitas`'s own "no floor, only gravity" convention exactly, per this item's own
brief that Favor is "explicitly analogous to Dignitas but a second distinct axis." Unlike Dignitas,
Favor cannot exist independent of a chosen deity (§2's own "the Patron Deity... determines what the
single Favor score actually does" makes the pairing structural), so `SetPatronDeityCommand` is the one
founding entry point and `AdjustFavorCommand` — the Favor analog of `AdjustDignitasCommand`, exercised
directly by this item's own tests standing in for the same kind of future callers item 1's own command
named (a future Legal & Court sacrilege case, a future Scandal) — requires an entry to already exist.
Divine Displeasure (§2.3) is a derived read (`HouseholdReligionResolver.IsDivinelyDispleased`) off a
threshold constant rather than a second, storable boolean that could drift out of sync with Favor,
matching `MagistracyResolver.IsActive`'s own "derive it, don't duplicate it" precedent.
`ReconsecrateCommand` (§2.1) builds only the one real, checkable trigger the design doc names —
`HouseholdHeadship.HeadCharacterId` (Phase 11 item 1) differing from the Patron Deity's own recorded
`ConsecratedUnderHeadCharacterId` — and skips §2.1's second trigger ("a major Chronicle-worthy event
plausibly attributed to divine intervention") outright, since nothing in `Gens.Simulation.Chronicle`
classifies an entry that way and inventing that judgment call would be building a mechanic the design
doc itself only gestures at; it is a real Funded Action (a fixed ceremony cost through the Ledger,
mirroring `Policies.FundFestivalCommand`'s own spend shape) that resets Favor to zero rather than
carrying it over, per §2.1's own "resets... toward a neutral middle."

The Rites Budget (§3.1) is **not** re-authored: Phase 9 item 2 already shipped `Policies.RitesBudgetTier`,
`ChangeRitesBudgetCommand`, `HouseholdPolicyResolver`, and — critically — `RitesBudgetCatalog`'s own
`TreasuryDrawPerMonth`/`DivineFavorStabilityModifier` projections, with that catalog's own doc comment
naming Religion directly as the pass that would "actually consume" them "without yet wiring them into a
monthly tick." `FavorCycleSystem` is that wiring: every month, every household with a chosen Patron
Deity pays its tier's Treasury draw into this domain's own Ledger sink and its Favor moves by that
tier's stability modifier, closing the forward reference rather than inventing a second, parallel
standing-policy mechanism. Omens (§4.1) are a new kept-forever `OmenEvent` partition
(`RaiseOmenCommand`/`RespondToOmenCommand`) rather than a self-triggering periodic generator: no system
in this codebase hooks a Religion-specific entry into Phase 9's content-authored weighted Event pool, so
`RaiseOmenCommand` is the commissionable primitive a future pool entry would submit, matching
`AdjustDignitasCommand`'s own "no such caller exists yet" precedent applied to a trigger instead of a
consumer. Heeding always averts (§4.1); ignoring rolls a severity-scaled "did the omen's warning come
true" chance through a newly registered named stream (`religion.omenIgnoredOutcome`, wired into
`CampaignBootstrapper` alongside every other rule-8 stream) — `RespondToOmenCommand.CreatePipeline`
captures the `RandomStreamSet` the same way `PromoteToNamedCommand.CreatePipeline` already does,
establishing that a command, not only a monthly system, can roll dice deterministically. §8's Piety gate
("Impious is immune to the ignore penalty; Zealous pays a real cost even when nothing follows") is read
directly off `Character.Traits` against the actual content-authored trait ids `content/source/traits/
piety.json` defines (`impious`/`devout`/`zealous`) — a real, non-parallel read of the shipped Piety
spectrum, not a stand-in, since (unlike Fame) that content genuinely exists; no compiled `TraitCatalog`
is reachable from a command or `IMonthlySystem`'s own `MonthlyTickContext` (it carries only `GameDate`
and the random stream registry), so this is a direct id-membership check rather than a catalog lookup.

The state Priesthood track (§6.2) is a new `PriesthoodRecord`/`PriesthoodOffice` pair mirroring
`MagistracyRecord`/`MagistracyOffice` by direct instruction, covering only Augur, Flamen, and the
Pontifex capstone — §6.2's own `sacerdosPublicus` baseline role is omitted from the enum entirely
(matching `MagistracyOffice`'s own "omitted rather than included-but-unreachable" precedent for a
Rome-track office that item 2 declined to build) since Companions & Court Positions has no code
anywhere in this repository, not even a building type to gate against the way Local Magistracies' own
Curia/Mint gaps at least had. `AppointPriesthoodCommand` gates on citizenship (Familia §2.5, matching
`AppointDecurionCommand`'s identical check) plus §6.2's own Piety-tier (Devout or Zealous) and Learning
threshold rather than Politics & Patronage's Dignitas floor; Flamen additionally requires the candidate's
own household Patron Deity to match, and Pontifex requires already holding an active Augur or Flamen
seat, mirroring Duumvir's own "must already hold a lower office" gate. `PriesthoodTrickleSystem` applies
a monthly Favor/Dignitas trickle ranked Augur < Flamen < Pontifex per §6.2/§6.3's own relative framing,
and vacates a seat on the holder's death (no term/re-election concept at all, unlike a Magistracy's
annual cycle — historically a priesthood ran for life, per this record's own doc comment). Auspices
(§4.2) is `CommissionAuspicesCommand`, gated on two of the three reliability tiers §10's own sketch names
(household default vs. an active Augur officeholder) — the middle "hired Haruspex" tier is a named,
reasoned cut: it needs both a paid per-reading hire mechanism (Companions & Court Positions, again
unbuilt) and a priced `incense` Good that does not exist anywhere in `content/source/goods/` (only
metals/staples/textiles are authored), so the fee is priced in Money instead, matching
`AppointDecurionCommand`'s own "no building exists, so that half of the gate is not checked" precedent
applied to a resource rather than a building. The reading's own §4.2 payoff — "a real skew... feeding
the preceding decision's own resolution" — is not wired into Military & Combat, Travel, or settlement
founding, none of which have a resolution system in this codebase yet to skew; this command's own payoff
is scoped to Religion's Favor axis directly, leaving the downstream consumer as a future integration
point, matching `AdjustDignitasCommand`'s own "no such caller exists yet" shape applied to a consumer
instead of a caller.

The Sacred Calendar (§5) gets its two observance tiers: `ObserveFeastDayCommand` (passive, a small
automatic Favor tick) and `FundFestivalCelebrationCommand` (funded, a Favor/Dignitas payoff sized to the
spend). The funded command is deliberately **not** a retrofit of `Policies.FundFestivalCommand` — that
command already exists from Phase 9 item 2, already moves the money and posts the Ledger receipt, and
its own doc comment already names Religion as the future pass that would "turn the spend into an actual
Divine Favor/Dignitas payoff" — but reopening it would change already-tested behavior (its own
`FundFestivalCommandTests` asserts an exact two-event result), the same "already-shipped, already-tested,
out of scope to reopen" precedent item 1 set for `Agnomen.DignitasEffect` and the Funerary Grand-tier
trade. Instead this item builds its own self-contained command, per the task's own explicit fallback for
exactly this situation ("a direct Favor/Dignitas payoff command rather than inventing a new generic
Funded Action system") — a future Policies & Edicts pass (§6.12, roadmap item 9) is the natural place to
unify the two under one real Funded Action abstraction. Both feast-day commands take a plain, free-form
feast-day string rather than a closed catalog, since §5's own table is "a representative, non-exhaustive
sample... a natural later-pass task" (§11), matching `AdjustDignitasCommand`'s identical `Reason`
convention for an open-ended vocabulary. §5's own paired Settlement Demographics Contentment boost and
Games & Spectacle venue-resolution hook are both left unwired — the same "no write path/consumer system
exists yet" reasoning `FundAedileWorksCommand`'s own Contentment half already established in item 2.

**Explicitly not built, matching this item's own narrow scope:** the Vestals (§6.3) — a deliberate cut,
since a Chastity-violation case sits at Legal & Court's own capital-case tier (§9 of that doc) and Legal
& Court (Phase 12 item 4, the next item) has no case machinery anywhere in this codebase yet to resolve
one against; foreign cults and religious syncretism (§7) — depends on the Religions of the Known World
catalog content, a separate future document this item does not pre-empt; persecution mechanics (§7's own
"heaviest ceiling") — depends on both the foreign-cult mechanic above and Legal & Court's own case
machinery, neither built. The Genius/Juno household-spirit flavor (§3) is deliberately not a fourth
tracked value, per that section's own "a case where naming the real institution matters more than giving
it its own number." No `InkBarQuery` slot is reserved for Favor — `gens-core-design.md` §7.4's own ink
bar field list ("gens name · date/season · treasury · dignitas") names four fields, Favor among them
not, so this item does not invent UI surface the design doc never asked for, in contrast to item 1's own
Dignitas slot, which *was* reserved and simply unwired. Covered in
`tests/Gens.Simulation.Tests/Religion/ReligionTests.cs`, including Patron Deity founding and its
one-shot guard, Favor adjustment and Divine Displeasure's threshold read, a headship-gated Reconsecration
(rejected against the same head, accepted against a new one) with its ledger spend and Favor reset, the
Rites Budget cycle's Treasury draw and stability modifier, Omen raising/heeding/ignoring (including the
Impious immunity and a real severity-scaled random draw), Priesthood appointment's citizenship/Piety/
Learning/Flamen-deity/Pontifex-capstone gates and its monthly trickle and death-driven vacancy, Auspices'
two reliability tiers, both feast-day observance tiers, and a save/load round trip with the deterministic
state hash staying stable across every new partition.

**Item 4 progress:** Legal & Court lands as a new domain, `src/Gens.Simulation/Legal/`
(`gens-legal-court-design.md`). `LegalCase` (§11's own data-model sketch) is a new kept-forever
`WorldState` partition, following `MagistracyRecord`'s "kept for the campaign's lifetime" convention —
every case type §2 names (`PropertyLand`, `Contract`, `Debt`, `SlaveOwnership`, `Succession`, `Criminal`,
`Political`, `Family`, `Military`) is represented in the enum even though only `Criminal`/`Political`
(the two capital-shaped types) and the generic civil shape have a real resolution path from this item
itself — no Debt/Succession/Slave-Market/Military caller exists yet to file a real case of those types,
matching `AdjustDignitasCommand`'s own "no such caller exists yet" precedent applied to an enum instead of
a command. **Household-level parties throughout** is this item's one deliberate scope decision, named
directly in `LegalCase`'s own doc comment: §11 leaves `plaintiffId`/`defendantId` untyped, and this item
resolves every case at the same `Household` granularity `AdjustDignitasCommand` already moves rather than
at individual `Character` granularity, since no Character-level standing/reputation primitive exists
anywhere in this codebase to move instead. `FileLawsuitCommand` is §5.1's Filing stage: it assigns a
presiding magistrate via `LegalCaseResolver.SelectPresidingMagistrate` (§3 — the first active Decurion at
the settlement whose own household isn't a party; recusal leaves the case presider-less rather than
inventing a generated-NPC-magistrate fallback when none is eligible, a named cut against §12's own
"small-settlement recusal chain isn't specified" open question), debits §4's filing cost (scaled by
depth) through the Ledger, and — for a Quick case — resolves inline in the same submission through
`LegalCaseResolver.RollVerdict`, a single weighted check reading each party's case strength, Dignitas
(§4's own "thumb on the scale"), and any Bribery weight already offered, with a real, non-binary outcome
distribution (Dismissed / Plaintiff / Defendant / SplitCompromise, or Acquitted / Convicted for the two
capital types) per §9. A Major case instead opens at `EvidenceGathering` and is carried forward by
`LegalCaseAdvancementSystem`, a new monthly system that holds the stage for `MajorCaseEvidenceGatheringMonths`,
moves it into `Hearing` for one real tick (§5.3's "a real, singular event, not a silent tick"), then rolls
the same `RollVerdict`/`LegalCaseRuling.Apply` pair `FileLawsuitCommand`'s own Quick Resolution already
uses — one shared resolution path for both depths, not two. `SubmitTestimonyCommand`/`GatherEvidenceCommand`
are §8's Testimony &amp; Evidence stage, each adding case-strength to a side (a witness's Legal Scholar
Trait or a gatherer's Intrigue attribute add real extra weight); `OfferBribeCommand` is §7's Bribery input
— the first `OfferBribeCommand` anywhere in this codebase (only `LedgerTransactionCategory.Gifts`'s own
doc comment previously named bribes as belonging to that category without a real mover), converting a
bribe's Denarii amount directly into case-score weight since no per-Character Greed axis score is
reachable from this domain, matching `ReligionCatalog`'s own "no compiled TraitCatalog reachable here"
precedent for reading personality through content trait ids instead. `ScoutPresidingMagistrateCommand` is
§3's own scouting flag — deliberately only the flag, since the presider's real Axes/Traits are already
directly readable off the live `Character` record the moment they're assigned, matching `Agnomen`'s own
"the flag is the documented hook" precedent.

`LegalCaseRuling.Apply` is §9's shared "verdict lands, consequences ripple outward" logic, the one place
both Quick and Major resolution apply a rolled verdict: a Dignitas shift for both parties through
`AdjustDignitasCommand`, a relationship-web scar between the two household heads through
`RecordInteractionCommand` (`BondTag.Nemesis`, a real opinion cost) on any clean win/loss or capital
verdict, and — for a `Convicted` verdict on a `Political` case — office loss through the new
`EndMagistracyForConvictionCommand` in `Gens.Simulation.Magistracies`, finally minting
`MagistracyLossReason.LegalConviction`, an enum value Phase 12 item 2's own doc comment named as
"genuinely unreachable in this codebase today" until this item existed to wire it — the exact kind of
forward reference item 2 itself flagged as this item's job to close. A `Convicted` verdict also rolls a
`LegalSentence`: only `Fine` (a real Ledger charge) and `Exile` (recorded, but with no `Exiled` Reactive
Trait to attach — that trait belongs to a different, unbuilt document) are ever actually minted;
`DebtBondage` and `Execution` are kept modeled-but-unreached, matching `MagistracyLossReason.LegalConviction`'s
own precedent for a design-doc value a future pass deliberately wires. §6's Patria Potestas case is a real,
if narrow, slice: `LegalCase.IsPatriaPotestasCase` forces `RollVerdict` straight to `Dismissed`
unconditionally ("no court can formally override"), while `LegalCaseRuling` still applies a harsher
Dignitas penalty than an ordinary dismissal and marks the defendant household's own recorded head with a
new `scandal-marked` content trait (`content/source/traits/legal.json`, alongside `litigious` and
`legal-scholar` — the three Traits §6.6/§10 name as needing "a concrete mechanical home," authored here
since nothing else in this codebase had built them yet) — landing on the household's recorded head rather
than the specific dependent Patria Potestas was exercised against, the same household-level-party
limitation the flag already accepts. A `LegalCaseRuledEvent` is projected into the Dynasty Chronicle for
exactly the two scandal-shaped outcomes (a `Convicted` verdict, or any Patria Potestas case), matching
`InsolvencyStageChangedEvent`'s own "only the terminal rung is Chronicle-worthy" precedent — an ordinary
civil win/loss/split is routine bookkeeping, not chronicled.

**Explicitly not built, matching this item's own scope:** Espionage's blackmail-material leverage (§8,
Phase 12 item 6+ or later) and every cross-system case source §10 lists — Economy &amp; Finance's Debt
Legal-exposure step, Succession &amp; Dynasty's Declaration/Disownment challenges, Labor &amp; Slavery's
ownership/manumission disputes, Military &amp; Combat's captive legal disposition, Rival Houses'
extinction disposition, and Politics &amp; Patronage's Sumptuary enforcement — none of those domains
submit `FileLawsuitCommand` themselves yet; this item builds the generic engine those future passes are
meant to file into, exercised directly by its own tests standing in for those future callers, matching
item 1's own "this item only builds the shared primitive itself" precedent. Covered in
`tests/Gens.Simulation.Tests/Legal/LegalTests.cs`, including filing/presiding assignment and recusal, the
Quick filing fee, Testimony/Evidence case-strength gains (with the Legal Scholar bonus), Bribery's
capped weight and real Ledger spend, the Major case's Evidence-Gathering → Hearing → Ruled progression,
a Patria Potestas case's forced Dismissal with its harsher penalty and one-shot Scandal-Marked trait, a
Political conviction stripping office and collecting the fine, and a save/load round trip with the
deterministic state hash staying stable.

Recommended internal order:

1. Dignitas, fame, personal reputation, actor standing, favors, obligations, and audience-specific visibility.
2. Patronage/clientela and office/appointment foundations.
3. Religion, rites, favor, priesthood/institution actors, and culturally scoped rules.
4. Legal cases, evidence, standing, testimony, verdicts, and enforceable consequences.
5. Crime, detention, punishment, ransom, legitimacy, and authority boundaries.
6. Interest groups, collegia/guild membership, notable households, and collective actions.
7. Scandal records, rumors, propagation, discovery, damage control, and lifecycle.
8. Fame/celebrity and public endorsement.
9. Full edicts, funded actions, doctrine, backlash, and political feedback.

**Exit gate:** the same underlying action can produce different legal, reputational, religious, and factional consequences according to actor, status, audience, evidence, and place—without bespoke shortcuts for each screen.

**Primary design inputs:** `gens-politics-patronage-design.md`, `gens-religion-design.md`, `gens-legal-court-design.md`, `gens-crime-punishment-imprisonment-design.md`, `gens-interest-groups-design.md`, `gens-collegia-guilds-design.md`, `gens-notable-households-design.md`, `gens-scandal-design.md`, `gens-celebrities-influential-figures-design.md`, `gens-policies-edicts-design.md`.

### Phase 13 — Add geography, travel, correspondence, culture, and history — ⬜ NOT STARTED

**Outcome:** distance, language, local rules, and historical time change what is possible.

Construction order:

1. Implement the region profile schema and date-aware rule overrides.
2. Implement location, route, distance tier, travel party, reservations, duration, risk exposure, arrival, and concurrent character locations.
3. Implement letters/messages, courier selection, transit, delivery, response, interception, forgery, and information provenance.
4. Implement culture and language definitions, literacy, fluency, interpreters, naming pools, and visibility/interaction gates.
5. Implement the historical timeline scheduler with immutable history, divergence-eligible events, counterfactual flags, and date-aware content validation.
6. Implement one complete region profile and only then expand region content in waves.
7. Add distant holdings and procurator requirements after travel and delegation are stable.

**Exit gate:** a household can act locally, travel, communicate at distance, encounter language/cultural gates, and receive date-appropriate historical events without loading broad region-specific code.

**Primary design inputs:** `gens-starting-regions-design.md`, `gens-travel-design.md`, `gens-correspondence-letters-design.md`, `gens-language-literacy-design.md`, `gens-cultures-of-the-known-world-design.md`, `gens-religions-of-the-known-world-design.md`, `gens-events-historical-timeline-content.md`.

Region data waves should consume the shared schema, not introduce new mechanics ad hoc. The current region corpus is: `gens-starting-regions-italian-heartland-design.md`, `gens-starting-regions-gallic-frontier-design.md`, `gens-starting-regions-iberian-colony-design.md`, `gens-starting-regions-north-african-colony-design.md`, `gens-starting-regions-greek-east-design.md`, `gens-starting-regions-britannia-design.md`, `gens-starting-regions-egypt-design.md`, `gens-starting-regions-syria-levant-design.md`, `gens-starting-regions-anatolia-design.md`, `gens-starting-regions-balkans-design.md`, `gens-starting-regions-sicily-design.md`, `gens-starting-regions-alpine-provinces-design.md`, `gens-starting-regions-armenia-design.md`, `gens-starting-regions-mesopotamia-design.md`, `gens-starting-regions-nubia-design.md`, `gens-starting-regions-arabia-felix-design.md`, and `gens-starting-regions-bosporan-kingdom-design.md`.

### Phase 14 — Add health, disease, disasters, and mobile populations — ⬜ NOT STARTED

**Outcome:** environmental and biological pressure matters without becoming arbitrary save destruction.

Construction order:

1. Extend health with conditions, exposure, resistance/immunity, treatment, recovery, mortality attribution, and care capacity.
2. Implement sanitation, food/water quality, crowding, livestock disease, endemic pressure, outbreaks, and quarantine.
3. Implement environmental hazard profiles, forecast/knowledge, disaster instances, damage, displacement, recovery, and region/date modifiers.
4. Implement wandering population cohorts, routes, needs, fame/visibility, settlement interaction, recruitment, and promotion to named characters.
5. Integrate hazards with goods, buildings, populations, markets, travel, events, institutions, and reports through shared events and effects.

**Exit gate:** hazards have visible causes, warnings where appropriate, bounded losses, recovery paths, and deterministic fixtures; they do not bypass ownership, ledger, health, or event rules.

**Primary design inputs:** `gens-disease-public-health-design.md`, `gens-natural-disasters-design.md`, `gens-wandering-populations-design.md`.

### Phase 15 — Add advanced commerce, property, and public investment — ⬜ NOT STARTED

**Outcome:** economic play expands from one household market loop into institutions, portfolios, partnerships, and infrastructure.

Recommended internal order:

1. Land/property market, districts, leases, operators, valuations, and portfolio oversight.
2. Societates, partner shares, governance, liability, disputes, and dissolution.
3. Merchant families/equestrian status and actor archetypes.
4. Notable businesses and promotion from ordinary market activity.
5. Business competition, price wars, cartels, saturation, and lawful/unlawful responses.
6. Public contracts, bids, bonds, delivery milestones, corruption, and audit.
7. Private infrastructure with capacity, access, upkeep, tolls, and public spillovers.
8. Shipping ventures, vessels, cargo, crew, routes, loss, insurance-like arrangements where historically supported, and distant settlement markets.
9. Public works/euergetism, contribution, prestige, maintenance, and institutional ownership.
10. Deeper wealth-band purchasing power after the underlying market is proven.

**Exit gate:** every enterprise resolves through the common actor, property, contract, ledger, market, knowledge, and event contracts; no feature creates a parallel economy.

**Primary design inputs:** `gens-land-ownership-real-estate-design.md`, `gens-societates-business-partnerships-design.md`, `gens-merchant-families-design.md`, `gens-notable-businesses-design.md`, `gens-business-competition-design.md`, `gens-public-contracts-competitive-bidding-design.md`, `gens-private-infrastructure-design.md`, `gens-private-ships-shipping-ventures-design.md`, `gens-public-works-euergetism-design.md`, `gens-population-wealth-purchasing-power-design.md`.

### Phase 16 — Add espionage, banditry, military force, and diplomacy — ⬜ NOT STARTED

**Outcome:** coercion and external danger use the same world rather than a separate minigame state.

Recommended internal order:

1. Information networks, spy placement, intelligence quality, counterintelligence, discovery, and traceability.
2. Security exposure, bandit/pirate actors, raids, protection, pursuit, and regional risk.
3. Persistent forces, squads, equipment, readiness, command, deployment, losses, captives, and aftermath.
4. A shared combat-resolution kernel usable by military, guards, raids, duels, and spectacle without giving each a separate damage model.
5. Foreign people as living-world actors, per-people standing, interpreters, diplomatic actions, treaties, retaliation, and great-power/buffer logic.
6. Integrate public authority, law, economy, travel, correspondence, reputation, and history before expanding battle content.

**Exit gate:** conflict conserves people, equipment, goods, money, location, injury, captivity, and reputation; intelligence remains uncertain; military outcomes do not bypass the ordinary state model.

**Primary design inputs:** `gens-espionage-design.md`, `gens-piracy-banditry-design.md`, `gens-military-combat-design.md`, `gens-diplomacy-non-roman-peoples-design.md`.

### Phase 17 — Add deep relationships, activities, culture, and legacy objects — ⬜ NOT STARTED

**Outcome:** the mature simulation gains its richest personal and cultural expression after its shared engines are stable.

Recommended internal order:

1. Companions/court positions and travel retinues on top of duties, delegation, and relationships.
2. Education, pedagogy, study, literacy, cultural patronage, and institutions.
3. Full adult romance, sexuality, affection/attraction, courtship, autonomous relationships, pregnancy, legitimacy, affairs, and consequences. Preserve the document's hard Adult lifecycle gate and power-imbalance exclusions.
4. The generic activity engine: invitations, guest lists, phases, quality/scale, witness pools, resolution, and NPC hosting.
5. Feasts as the first complete activity type.
6. Games/spectacle, competitors, fame, hosting, wagering, and political payoff.
7. Books/manuscripts: works vs. copies, authorship, reading, provenance, copying, loss, and libraries.
8. Art/commissions and artists on the same provenance/object framework.
9. Masterworks as exceptional goods with provenance, function, loss, repair, heirloom, and display state.
10. Monuments/legacy building on top of property, public works, chronicle, and dynasty memory.

**Exit gate:** these systems reuse characters, actions, activities, objects, provenance, knowledge, contracts, ledgers, witnesses, events, and chronicle entries. They do not introduce parallel social or item models.

**Primary design inputs:** `gens-companions-court-positions-design.md`, `gens-education-culture-design.md`, `gens-romance-sexuality-lineage-design.md`, `gens-activities-activity-engine-design.md`, `gens-feasts-design.md`, `gens-games-spectacle-design.md`, `gens-books-manuscripts-design.md`, `gens-art-art-commissions-design.md`, `gens-masterworks-unique-crafted-objects-design.md`, `gens-monuments-legacy-building-design.md`. Treat `gens-romance-seduction-design.md` as superseded reference material, not a second implementation source.

### Phase 18 — Scale content, presentation, art, performance, and release operations — ⬜ NOT STARTED

**Outcome:** the integrated sandbox becomes a maintainable product.

Construction order:

1. Finish the canonical data catalogs and migrate provisional definitions.
2. Expand region profiles, traits, events, buildings, goods, policies, actors, historical hooks, activities, and authored text in measured content waves.
3. Complete the visual system: ink bar, diptych layouts, mosaic map language, Chronicle presentation, wax seals, geometric icons, responsive UI, keyboard/gamepad navigation, text scaling, contrast, and content controls.
4. Complete deterministic procedural layered SVG portraits and caches; add optional generated-art providers only after placeholders and procedural art cover every required state.
5. Add artwork moderation, recipes, hashes, cache manifests, provider failure/retry, offline behavior, and save portability.
6. Profile real worlds. Optimize algorithms and allocations first; use Jobs/Burst selectively only with benchmarks. Reconsider Entities/DOTS only if measured scale still misses the agreed budget and the migration cost is justified.
7. Add load tests, fuzz/property tests, 1,000-year soaks, deterministic cross-platform comparisons, save-corruption recovery, migration chains, content-pack compatibility, and generated-asset cache recovery.
8. Add telemetry with privacy controls, crash reporting, diagnostics export, build signing, versioning, release channels, rollback, localization, accessibility review, tutorial/onboarding, and balance instrumentation.
9. Establish release gates for supported hardware, startup/load/save time, monthly tick percentiles, memory growth, zero broken references, zero migration failures, and zero unresolved critical accessibility defects.

**Exit gate:** a release candidate can begin, run, save, migrate, and complete long campaigns across supported configurations; optional services can fail without preventing play; all required content and presentation have non-AI fallbacks.

## The first 24 implementation work packages

These are the recommended first issues or narrowly scoped pull requests, in order. **All 24 are complete** (delivered across PRs #6–#34; see "Detailed roadmap" Phases 0–6 above):

1. [x] Diagnose and fix the current `standalone` and `content` job build failures and restore green CI (see "Immediate red condition" above; the original `Property`-ambiguity report is already resolved, but other build/analyzer errors remain).
2. [x] Split tests and content compilation into independent required CI jobs.
3. [x] Add design-authority registry and supersession markers.
4. [x] Add ADRs for IDs, time, fixed point, tick phases, command atomicity, event envelopes, knowledge, and fidelity tiers.
5. [x] Add first-slice field/unit/range/owner ledger.
6. [x] Introduce typed stable IDs and definition IDs.
7. [x] Introduce partitioned `WorldState` and deterministic indexes.
8. [x] Define epoch-aware `GameDate` and historical display conversion.
9. [x] Replace registration-order-only ticks with declared phases/dependencies.
10. [x] Add command envelope, stable validation errors, change sets, and atomic application.
11. [x] Add versioned domain-event envelope and event registry.
12. [x] Formalize random stream registry, seed derivation, and rollback behavior.
13. [x] Add invariant runner and deterministic world hash.
14. [x] Implement canonical save serialization and `.gens` archive IO.
15. [x] Add migration registry and version-1 golden save fixture.
16. [x] Replace the placeholder content schema with manifest plus typed definition envelope.
17. [x] Add reference/range/localization validation and deterministic compiled output.
18. [x] Add headless campaign bootstrap and console runner.
19. [x] Add scheduled-action queue and empty 1,200-month soak test.
20. [x] Add canonical Character/Familia records and deterministic generation.
21. [x] Add lifecycle, household roles, relationships, and save round trips.
22. [x] Add minimal region/settlement/plot/holding state.
23. [x] Add goods, stockpiles, building instances, and production recipes.
24. [x] Add labor assignment and the first three compact production chains.

Background population, market clearing, the Unity vertical slice, the rest of Phases 7–9, Phase 10's delegation/autonomous-action/rival-houses work, and Phase 11's dynasty-continuity/historical-memory work have also since been implemented (see the phase checklist above), and Phase 12 items 1-4 (Dignitas/reputation/favor-obligation primitive; patronage/clientela and office/appointment foundations; Religion's Favor meter, rites, Omens/Auspices, and the Priesthood track; Legal & Court's case filing, presiding assignment, evidence/testimony/bribery, and verdict consequences) are now done too. **The next unimplemented work is Phase 12 item 5** — crime, detention, punishment, ransom, legitimacy, and authority boundaries.

## Vertical-slice acceptance test

The first playable slice is complete only when all of the following hold:

- A new campaign is entirely defined by versioned content, ruleset, seed, and ordered commands.
- The household includes 6–10 persistent named characters with lifecycle, roles, relationships, health, traits, and portraits/placeholders.
- The settlement includes land, a villa, stockpiles, three production chains, labor, upkeep, and background population groups.
- Production, needs, employment, wages, prices, transactions, tax/upkeep, and one market contract reconcile in the ledger.
- The player can issue actions, assign labor, build, change a policy, delegate to one overseer, and respond to three compact event chains.
- One rival actor operates autonomously and is visible only through appropriate knowledge/report channels.
- Every important monthly change appears in a readable report with cause and drill-down.
- The UI supports new campaign, advance, pause, inspect, command, save, load, and recover from validation errors.
- A 24-month manual playthrough and 200-year headless soak both pass.
- Replaying the same seed and commands produces identical hashes; saving/loading at any monthly boundary does not change the result.
- Normal-scale monthly ticks meet the agreed 250 ms target on the reference machine with subsystem timing reported.

## Work explicitly deferred until its dependency is ready

- Do not author the full trait catalog into runtime data before the trait contract and representative slice are proven.
- Do not implement all region documents before one region validates the shared schema and date-aware rules.
- Do not create a large Unity UI around mutable domain objects; wait for queries/read models and the command path.
- Do not build separate combat, spectacle, duel, and raid resolution engines.
- Do not build separate economies for contracts, ships, public works, or rival houses.
- Do not make AI portraits or generated prose a campaign dependency.
- Do not introduce DOTS because the design is large; require profiling evidence against actual scale fixtures.
- Do not solve balancing exclusively in prose. Put tunable values in versioned content and test invariant envelopes.
- Do not expand the design corpus with another major system until the authority registry, numeric slice, and first vertical loop are in place.

## Roadmap governance

For each phase, maintain four synchronized artifacts:

1. **Design contract:** intended behavior, authority, open decisions, and player-facing consequences.
2. **Engineering contract:** state ownership, commands, events, read models, tick placement, deterministic ordering, save/content versions, and performance budget.
3. **Acceptance fixtures:** unit/property tests, scenario tests, golden seeds, saves, replays, and benchmarks.
4. **Content slice:** the smallest representative data that exercises the system end to end.

An issue is not ready for implementation until its dependencies, authoritative fields, units, invariants, events, visibility rules, save impact, content impact, and acceptance tests are named. A phase is not complete because its classes exist; it is complete when its exit gate passes through the headless runner and, where applicable, the Unity vertical slice.

## Final recommendation

~~The next milestone should be called **Foundation Contract & Headless Campaign**~~ — ✅ **complete.** Phases 0–4 delivered a green, deterministic, saveable, content-validated campaign shell.

~~The milestone after that should be **Household Economy Vertical Slice**, encompassing Phases 5–9.~~ — ✅ **complete.** Its output, the first genuinely playable Gens loop (named household, land/production/labor, background population, ledger/market, and the action/policy/event/report/Unity presentation layer), is now in place, through PR #49.

~~The milestone after that should be **Delegation, Autonomous Action, and Rival Houses**, encompassing Phase 10.~~ — ✅ **complete.** `LivingWorldActor` tiers, rival-house lifecycle, Ancestral Grudges, the shared `ActionSelector`, steward/Council autonomy with real competence/loyalty rolls and Return Reports, the Scheme engine, and the 200-year combined soak are all in place.

~~The milestone after that should be **Dynasty Continuity and Historical Memory**, encompassing Phase 11.~~ — ✅ **complete.** Heirs/succession/disputed inheritance, the player-control handoff and Regency, the Dynasty Chronicle, funerals/mourning/Memoria, and rules-and-provenance epithets are all in place, proven together by a three-succession (one contested) exit-gate soak.

**The next milestone is Phase 12 — institutions, reputation, law, religion, and public life.** Dignitas, fame, patronage, religion, legal cases, crime and punishment, interest groups, scandal, and public life can now be constructed as extensions of the same shared contracts (commands, events, ledgers, read models, knowledge/visibility, and the Dynasty Chronicle Phase 11 just added) every prior milestone has used. That is the safest route to the unusually deep game described by the design corpus without sacrificing determinism, historical breadth, or future AI-assisted presentation. Items 1–4 (the shared Dignitas/reputation/favor-obligation primitive; patronage/clientela and office/appointment foundations; Religion; Legal & Court) are complete; items 5–9 remain.

