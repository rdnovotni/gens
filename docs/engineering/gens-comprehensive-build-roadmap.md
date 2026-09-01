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
- [x] **Phase 12** — Build institutions, reputation, law, religion, and public life
- [x] **Phase 13** — Add geography, travel, correspondence, culture, and history
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

### Phase 12 — Build institutions, reputation, law, religion, and public life — ✅ COMPLETE

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

**Item 5 progress:** Crime, Detention &amp; Punishment lands as a new domain, `src/Gens.Simulation/Crime/`
(`gens-crime-punishment-imprisonment-design.md`). `PunishableOffense` (§3) is a new kept-forever
`WorldState` partition, Character-scoped rather than household-scoped — the one deliberate contrast with
`LegalCase`'s own household-level party model, since §4's Imprison action targets a specific Character (a
dependent, a Client), not their whole household. `RecordPunishableOffenseCommand` is the one command path
every real or future source routes through, mirroring `AdjustDignitasCommand`'s own "the one command path
every future mover routes through" precedent: `LegalCaseRuling.Apply` (Phase 12 item 4) now calls it
directly on every `Convicted` verdict — a real, immediately reachable source this item wires on sight,
minting the offense against the defendant household's own recorded head (the same "lands on the
household's recorded head" simplification that item's own Patria Potestas Scandal-Marked trait already
accepts) — while `Fabricated` is a generic, source-agnostic flag any future caller can set directly
through the same command, since no Scheme type exists yet to originate one (`Interactions.SchemeType`
still has exactly one real value, `Coercive`, from Phase 10 item 6). `DiscoveredScheme`/`DiscoveredAffair`/
`MilitaryCapture`/`PiracyCapture` are kept in the enum for schema completeness but never rolled by any
caller, matching `LegalCase.CaseType`'s own "every real category represented, only some reachable"
precedent — Romance &amp; Sexuality &amp; Lineage's affair-discovery mechanic and Military &amp; Combat/
Piracy &amp; Banditry (both Phase 16) don't exist anywhere in this codebase yet.

`ImprisonCommand` (§4) is broadly available rather than gated behind a court process, real for three
authority bases: `PatriaPotestas` and `ClientelaAuthority` (Familia's household headship and Phase 12 item
2's Clientela roster, both already shipped), plus `MagisterialJurisdiction`, a third real, reachable basis
this item adds on its own initiative since Phase 12 item 2's Local Magistracies already ships a real
`MagistracyRecord` a holder can be checked against — a Military &amp; Combat/Piracy &amp; Banditry captive
basis is omitted from the enum entirely, matching `LegalCase.CaseType`'s own "omitted rather than
included-but-unreachable" precedent for an unbuilt caller. It always resolves into a new active
`DetentionRecord`, Justified or Unjust read directly off `PunishableOffenseResolver.HasActiveOffense`
(§3/§4): Justified applies only a small relationship-web cost through `RecordInteractionCommand`; Unjust
routes a real Dignitas penalty through `AdjustDignitasCommand` and a sharper relationship-web scar,
"everyone is watching how you use power" built from this project's own existing Dignitas and
relationship-web machinery rather than a new imported stat, exactly as §4 calls for.

`DetentionRecord` (§5) is a new tracked status distinct from Enslaved, `locationType` tracked as data
(`PublicCarcer`/`PrivateErgastulum`) even though neither the public Carcer nor the private Ergastulum
exists anywhere in `Gens.Simulation.Buildings` yet — verified directly, matching
`AppointDecurionCommand`'s own "no building exists, so that half of the gate is not checked" precedent.
Escape risk (`DetentionResolver.ComputeRiskScore`) reuses `FlightRiskCalculator`'s own Labor &amp; Slavery
formula directly whenever it is actually reusable — a Detained enslaved Character still carries a real
`Regimen` to read Freedoms/Discipline from — but falls back to a real, simple, Loyalty-only placeholder
for a free Detained Character (a household dependent, a Client), since Freedoms/Discipline belong to the
labor-management system specifically; this is a narrower reuse than §5's own framing, since it borrows the
shared risk-to-probability curve unconditionally but only the full Regimen-driven score formula when a
Regimen actually exists. `AttemptDetentionEscapeCommand` is a single, directly-submitted roll (mirroring
how `RespondToOmenCommand` and `PromoteToNamedCommand` already roll dice inline inside a command) rather
than a recurring monthly opportunity check with its own dispatched-pursuit countdown — Detention's own
duration is "genuinely open-ended" per §5, not a fixed labor Duty a system needs to re-check every month —
and a failed attempt still costs the detainee Loyalty, mirroring `LaborFlightSystem`'s own
`RecaptureLoyaltyPenalty` for "caught trying," though this item does not build a further
dispatched-pursuit/harm-or-loss resolution the way that system's own enslaved-specific engine does.
`ReleaseFromDetentionCommand` is §10's "simple mercy" — always available, a real positive Dignitas and
relationship-web event precisely because it was never required.

`SentenceRecord`/`ApplySentenceCommand` (§7-§8) give Legal &amp; Court's own thin four-item sentence list
"the real historical depth and breadth it never had room for," per this document's own §1: every real
honestiores (`Fine`/`Relegatio`/`Deportatio`/`Ignominia`/`HonorableExit`) and humiliores
(`Flogging`/`DamnatioAdMetalla`/`ServusPoenae`/`DamnatioAdBestias`/`Crucifixion`) catalog value is
represented, matching `LegalSentence`'s own "schema completeness first" precedent, with the tier read
directly off `Character.SocialClass` rather than a second, parallel classification. Per direction to pick
real, reachable resolution paths rather than modeling the whole catalog with nothing behind it, this item
actually carries out `Fine` (a real Ledger charge, priced independently of `LegalCatalog.FineSentenceAmount`
since this command can apply one without a `LegalCase` ever existing), the exile-equivalent pair
`Relegatio`/`Deportatio` (Deportatio adding a real, flat property-confiscation Ledger charge distinct from
Relegatio's own milder "citizenship and most property retained"), `HonorableExit` (a real, dignified death
via `DeathCause.Unspecified`, distinct from an ordinary execution and preserving family Dignitas per §7's
own framing), and `Crucifixion` (this item's one real, humiliores-tier Execution path via
`DeathCause.Violence`, "played straight" per §7's own restraint) — every other value (`Ignominia`,
`Flogging`, `DamnatioAdMetalla`, `ServusPoenae`, `DamnatioAdBestias`) is rejected with a dedicated
`SentenceNotYetWired` validation error rather than silently no-opping, matching
`MagistracyLossReason.LegalConviction`'s own "the field exists, nothing can set it yet" precedent;
`DamnatioAdBestias` specifically is named directly by §7/§11 as belonging to Games &amp; Spectacle (Phase
17, unbuilt), not redefined here. Every sentence still applies §4's own Justified/Unjust Dignitas swing,
and an Unjust sentence scars the relationship with a named sentencing Character when one is actually
supplied — a Legal-conviction-sourced sentence has no single actor Character to scar against (that
verdict's own scar already lands household-to-household via `LegalCaseRuling`), and is, in practice,
always Justified in the first place since the conviction itself minted the offense.

`RansomNegotiation`/`OpenRansomNegotiationCommand`/`ResolveRansomNegotiationCommand` (§10) size an opening
demand off the captive household's own Dignitas (a real, if partial, stand-in for "sufficient standing" —
net worth is left out, since no per-household wealth figure independent of Economy &amp; Finance's own
broader Net Worth assessment machinery is cheaply reachable here) and resolve Paid/BargainedDown (a real
Ledger transfer between the two households, Detention released, Dignitas and relationship-web gains on
both sides) or Refused (a real relationship-web penalty, the captive stays Detained). §10/§11's own Rival
Houses Standing integration is real, not a named cut: `AdjustHouseStandingCommand` (Phase 10 item 5) is
the actual, reusable primitive, and this command submits it directly whenever *both* households resolve
back to a tracked `LivingWorldActor` through their own recorded head — the honest, narrower condition this
integration actually meets in practice, since a player's own Household never itself heads a
`LivingWorldActor` (`HouseholdReputation`'s own precedent). §11's own `mercyReleaseNoRansom` resolution
value is deliberately not a fourth `RansomResolution` member: §10 frames mercy as "always available too,"
not as something that has to flow through an open negotiation first, and `ReleaseFromDetentionCommand`
already is that exact real primitive, usable with or without an open negotiation — duplicating the same
release path a second time here would only fragment one real mechanic into two. `ChronicleProjector` gains
two new cases matching `InsolvencyStageChangedEvent`'s own "only the terminal rung is Chronicle-worthy"
precedent: a sentence that actually ends a Character's life (Legendary tier when Unjust, per §8's own
"lasting mark... regardless" framing), and a resolved (not merely opened) ransom negotiation.

**Explicitly not built, matching this item's own scope, after real investigation rather than assumption:**
§6 Interrogation &amp; Torture — the free-Detained-character half genuinely needs an interrogation
resolution engine (a real/false information split with its own reliability curve, §13's own open
question) this item does not build, and the enslaved-witness-testimony half needs Legal &amp; Court's own
Testimony &amp; Evidence stage (item 4) to actually call into a Torture resolution instead of its ordinary
`SubmitTestimonyCommand` path, which that already-shipped, already-tested item does not do and reopening
it is out of this item's scope; §9 Fabricating Justification as a new Scheme type — the Scheme engine
itself is real and reachable (`Interactions.SchemeType`'s own doc comment already earmarks room for
exactly this), so unlike Torture this is a genuine, buildable-later gap rather than a blocked one, but
wiring §9's own full Initiation → Discovery → retroactive-Unjust-penalty loop is a separate item of work
this pass does not open; the generic `IsFabricated`/`FabricationDiscovered` hook is real and present on
`PunishableOffense` today specifically so that future Scheme type has somewhere to land without a later
migration. The *Senatus Consultum Silanianum* household-wide legal-jeopardy crisis (§7) is not modeled —
it needs a real investigation-to-resolution pipeline this item does not specify, matching §13's own open
question about it directly. Small-household Ergastulum access, the "honorable exit as a real explicit
player choice vs. a plausible AI outcome" question, and every numeric size (Dignitas/relationship-web
deltas, escape-risk thresholds, Ransom pricing) are all left exactly as unsized as §13 states. Covered in
`tests/Gens.Simulation.Tests/Crime/CrimeTests.cs`, including a real Legal &amp; Court conviction minting a
Punishable Offense automatically, a fabricated offense recorded directly, Imprison's Justified/Unjust split
across all three real authority bases (Patria Potestas, Clientela, Magisterial Jurisdiction) with rejection
of an unauthorized actor and of an already-Detained target, Detention's escape-risk formula in both its
Regimen-reused and Loyalty-only-fallback shapes, a real escape attempt resolving both ways across seeds
with its failed-attempt Loyalty penalty, mercy release's Dignitas/opinion gain, every real sentence path
(Fine, Deportatio's confiscation, HonorableExit, Crucifixion) plus the rejection of a not-yet-wired one and
the Unjust-sentence relationship scar, Ransom's Paid/Refused resolutions plus its real Rival Houses
Standing bridge when both sides are tracked Actors, and a save/load round trip with the deterministic
state hash staying stable across every new partition.

**Item 6 progress:** Collegia & Guilds lands as a new domain, `src/Gens.Simulation/Collegia/`
(`gens-collegia-guilds-design.md`). A Collegium is not a new entity kind with its own ID counter: §6's own
`LivingWorldActorType.Collegium` value — reserved since Phase 10 item 3's own doc comment ("the rest exist
here so the framework itself does not need to change shape") explicitly named this as its future use —
is the real, structured organization itself, giving a Collegium a Name, a Dignitas figure, and a head
Character (§3's Magister, `LivingWorldActor.HeadCharacterId`) for free rather than duplicating any of
that. `CollegiumDetails` is a new sparse partition layered on top, keyed by that same Actor id, holding
only what a `LivingWorldActor` has no field for: `CollegiumType` (Opificum/Funeraticia/CultSpecific/
Compitalicia, §2), `CollegiumLegalStatus` (Licitum/Illicit, §7), an optional linked `PopGroupType`
(Opifices or Negotiatores only, §2's own trade-guild source), an optional linked `PatronDeity` (§2's
"organized around a specific deity" half — the foreign-cult half is a named, reasoned cut, since no
Religions of the Known World content exists in this codebase to link against), a patron household, a
Quinquennalis, and a member-household roster. `FoundCollegiumCommand` creates the underlying
`LivingWorldActor` directly (always `Background` tier, `Established` trend, no head yet) plus its
`CollegiumDetails` entry, and validates each type's own linked-field requirement. `BackgroundHouseDriftSystem`
gained a `LivingWorldActorType.Gens`-only filter, a genuine, necessary fix this item makes on its own
initiative: without it, a founded Collegium would get swept into Rival Houses' own fortune-drift and
(via a drifted-to-Declining trend) background extinction rolls neither of which were ever meant to apply
to it — every other system that walks `WorldState.Actors` (`RivalAmbitionSystem`, the extinction system's
own Noteworthy path) already only ever touches Noteworthy actors a Collegium never becomes, so this was
the one real gap.

`JoinCollegiumCommand`/`LeaveCollegiumCommand` (§2) add/remove a household from the roster directly — a
short, hand-curated list of real, already-tracked households (the player's own, or a rival Actor whose
head is already resolved) rather than a derived read off Settlement Demographics' own pop-group
aggregates, since `PopGroup` is keyed by (settlement, group type) and has no individual-household
membership to enumerate, matching `ClientPoachingSystem`'s own "only ever targets an Actor whose head is
already resolved" precedent for the identical gap. `ElectMagisterCommand`/`AppointQuinquennalisCommand`
(§3, §9) seat officers with deliberately no citizenship or Legal Status gate at all — §9's entire point is
that collegium leadership is "a real, genuine, respected achievement" precisely for a Freedman or
Peregrine the Curia and cursus honorum categorically exclude, a sharp, intentional contrast with
`AppointDecurionCommand`'s identical-shaped citizenship check. `SponsorCollegiumCommand` (§4) is the
patron relationship's real payoff: a flat Dignitas grant (`AdjustDignitasCommand`) and Influence grant
(`InfluenceResolver.Apply`, matching `SalutatioSystem`'s own "no shared Influence-moving command exists
yet" precedent), plus — once the collegium actually has a resolved Magister — a real Patron/Client
`BondTag` pair formed directly between the patron's own household head and that Magister, reusing
Clientela's existing bond vocabulary rather than inventing a parallel one. §4's own headline "an entire
bloc of grateful clients acquired in one relationship" is a deliberate, named cut beyond that one bond:
bulk-converting every member household into individual Clientela entries would need resolving each one's
own lazily-generated head on the patron's behalf, the same "no principled way to supply that" gap
`ClientPoachingSystem`'s own doc comment already names for a rival Actor's head.

The Arca (§3's shared treasury) is not a stored field at all: it is a real `LedgerAccount` at
`LedgerAccountKey.ForActor` keyed by the collegium's own Actor id, reusing the ledger's existing per-Actor
account kind directly. `FundCollegiumArcaCommand` posts a real `LedgerService.Post` transfer from a
funding household's own account into it — covering both §3's membership dues and §4's patron funding cost
as the same one real movement, and, unlike `FundFestivalCommand`'s one-way sink, a balance that genuinely
accumulates for a future command to spend back out of. §6's darker political edge —
`RecordCollegiumOrganizedDisruptionCommand` — is built as the one real `CollegiumPoliticalAction` this
item wires: gated on the instigating household actually sponsoring the collegium, it reads
Crime & Punishment's own `PunishableOffenseResolver.HasActiveOffense` against the target household's
recorded head for the same Justified/Unjust split `ImprisonCommand` already established, a small opinion
cost on the Justified path and a real Dignitas penalty, a Nemesis relationship-web scar, and — §7's own
"caught using this darker tool once too often" — an immediate flip to Illicit on the Unjust path (a single
use is treated as sufficient, rather than inventing an unsized repeated-offense counter the design doc
never specifies). §6's other, legitimate action — an election endorsement — is a deliberate, reasoned cut:
`HoldContestedElectionCommand` (Phase 12 item 2) resolves synchronously in one command call with no
persisted "election currently open" state for an endorsement to attach to ahead of resolution, so there is
nothing yet for one to feed into. `DissolveCollegiumCommand` (§7) is real, terminal dissolution authority:
gated on the collegium actually being Illicit and the initiating Character holding an active Magistracy at
the collegium's own settlement (mirroring `ImprisonCommand`'s identical "a sitting magistrate acting
outside a formal Hearing" authority basis; §7's own second basis, a provincial governor, is omitted
entirely — Reputation Duality and the provincial-governor concept it would need do not exist anywhere in
this codebase, matching `ImprisonAuthorityBasis`'s own "omitted rather than included-but-unreachable"
precedent). A dissolved collegium's own `LivingWorldActor` and `CollegiumDetails` entries are both removed
outright — matching `LivingWorldActorExtinctionSystem`'s identical "removed outright, not frozen"
precedent for a genuinely terminal transition — and a sponsoring patron takes §7's own real Dignitas risk
for having been publicly associated with it.

**Explicitly not built, matching this item's own scope:** the Schola (§3's meeting hall, a Land Ownership
& Real Estate Property Record) — no `PropertyRecord` type, or any other code from that document, exists
anywhere in this codebase yet, so `CollegiumDetails.ScholaPropertyId` stays permanently `null`, the
documented hook a future Land Ownership & Real Estate pass wires, matching `AppointDecurionCommand`'s own
"no building exists, so that half of the gate is not checked" precedent; §5's trade-guild collective
bargaining effect on Market Dynamics — no real per-collegium wage/price-stability read path exists in
Economy & Finance's own Market Clearing to attach to; §8's Funerary Guarantee — its own real trigger ("a
Notable Household's own member dying while in good standing") depends on Notable Households, a distinct
entity kind this codebase has never built (confirmed by direct search — no `NotableHousehold` type exists
anywhere), so there is nothing yet for the guarantee to resolve against; and any Quinquennalis census-cycle
automation, per §12's own open question about how a real, much-longer census interval maps onto this
game's monthly tick — appointment stays direct-only, with no term or automatic re-appointment.

**Interest Groups** lands as a narrower, read-mostly slice, `src/Gens.Simulation/InterestGroups/`
(`gens-interest-groups-design.md`). Per §4's own "membership is derived, never separately tracked," this
domain adds no new `WorldState` partition at all — only `InterestGroupResolver`, a read-side query over
data other, already-shipped domains already own. Of §2's five named coalition types, only
`CreditorsVsDebtors` is checkable against real per-household data, and only its Debtor half: a household
is a real member the moment it holds any `DebtRecord` that is neither `Forgiven` nor resolved. The
"Creditors" half is a genuine, investigated gap rather than an assumption: `DebtRecord.LenderIsPlayer`
"always reads false in this implementation" per that record's own doc comment, so every debt's
counterparty is the settlement Treasury, never another household — there is no real opposing household-
level Creditor bloc to organize at all. The other four types (`LandownersVsLandless`, needing Policies &
Edicts' Land Redistribution Edict; `PublicaniEquestrian`, needing a Publicanus Contract; `Veterans`,
needing a household-scoped veteran flag distinct from Settlement Demographics' own settlement-aggregate
pop group; `ProvincialInterest`, needing Reputation Duality) are named in the enum for schema completeness,
matching `LegalCase.CaseType`'s own "every real category represented, only some reachable" precedent, but
`IsMember` throws a named, explicit exception for each rather than a silently-confident `false` — none of
Policies & Edicts, a Publicanus Contract, a household-scoped veteran flag, or Reputation Duality exist
anywhere in this codebase (each verified directly, not assumed). §5's Collective Lobbying is the one real
action this item builds: §5's own stated target — moving a live Edict's own Reception — has nothing to
move, since Policies & Edicts is entirely unbuilt (Phase 12 item 9, not yet started), so
`CollectiveLobbyingCommand` instead pools real `HouseholdInfluence` from every contributing household and
credits the total to one beneficiary household, who can then spend it through the one real Influence-
spending consumer this codebase has, `HoldContestedElectionCommand`. §5's second action (a Curia Faction
Bloc) and §3's Provincial Patronage are both deliberate cuts: the "Found/Join a Curia Faction Bloc"
Interaction this item would extend exists only as a named catalog row in the Characters design document,
with no bloc-voting resolution engine anywhere in this codebase to give it real effect, and Provincial
Patronage depends on Reputation Duality, which — like every Starting Regions mechanic — is design-doc-only
today. Covered in `tests/Gens.Simulation.Tests/Collegia/CollegiaTests.cs` and
`tests/Gens.Simulation.Tests/InterestGroups/InterestGroupsTests.cs`, including founding a Collegium (with
its type-specific linked-field validation), roster join/leave, Magister/Quinquennalis appointment, the
sponsorship Dignitas/Influence grant and its Patron/Client bond formation once a Magister resolves, real
Ledger-backed Arca funding, the organized-disruption Justified/Unjust split (including the Illicit legal-
status flip), a formal dissolution's authority gate and its patron Dignitas penalty, a real Creditors-vs-
Debtors membership read, every unreachable Interest Group type throwing rather than silently returning
false, Collective Lobbying's real Influence pooling and its insufficient-Influence rejection, and a
save/load round trip with the deterministic state hash staying stable across the new Collegia partition.

**Item 7 progress:** Scandal lands as a new domain, `src/Gens.Simulation/Scandal/`
(`gens-scandal-design.md`), which the document's own §1 already frames correctly: "not a new consequence
system, but the shared engine" a handful of already-shipped Phase 12 moments have been quietly waiting
for. `ScandalRecord` (§3, §11's own data-model sketch) is a new kept-forever `WorldState` partition,
following `LegalCase`'s and `PunishableOffense`'s identical "kept for the campaign's lifetime" convention
— `ScandalRehabilitationSystem`'s own "a real, sustained stretch without further incident" gate (§8) needs
a household's full Scandal history, not just its current one. `RecordScandalCommand` is the one command
path (rule 2) every real or future source routes through, mirroring `AdjustDignitasCommand`'s and
`RecordPunishableOffenseCommand`'s own "the one command path every future mover routes through"
precedent: it always creates a real `ScandalRecord`, and three bool flags (`ApplyOrdinaryDignitasPenalty`,
`ApplyTraitGrant`, plus an optional `ScarredAgainstCharacterId`) let an already-shipped, already-tested
call site opt out of whichever part of §7's own "ordinary case" bundle — a Dignitas penalty, a
relationship-web scar, the Scandal-Marked Trait — it has already applied through its own existing
mechanism, so this command never double-applies a consequence another tested command already produced.
`ScandalSourceType`, `ScandalSeverity`, and `ScandalScope` all represent every value §4/§6/§11 name, matching
`LegalCase.CaseType`'s own "every real category represented, only some reachable" precedent; `ScandalScope`
in particular is fixed at `SettlementWide` for every real trigger this item wires, since Provincial/RomeWide
both need §6's own Prominence concept, confirmed by direct search to exist only as doc-comment TODOs
(`EventWeightInputs.cs`, `MourningPeriod.cs`, `MonthlyReportProjector.cs`) rather than a real field anywhere
in this codebase — the same finding Phase 12 item 1 already made for Fame, reconfirmed here.

Household-level throughout, matching `AdjustDignitasCommand`'s own convention, since no Character-level
reputation primitive exists to move instead. The Scandal-Marked Trait grant reuses `LegalCaseRuling`'s own
existing trait-grant plumbing exactly (a Contains-check before appending to `Character.Traits`, remove-then-
readd the `Characters` entry) rather than inventing a second mechanism, and is deliberately idempotent for
exactly that reason — a caller that already granted the Trait through some other path can still route
through this command's own `ApplyTraitGrant: true` and have the resulting `ScandalRecord` honestly stamp
`ScandalMarkedTraitApplied` true without ever double-granting. Faction-dependent reception (§7/§10) reads
`Clientela.CharacterFactionAlignment` directly off the scandalized household's own recorded head, the way
Phase 12 item 2 established that partition (§3.1): a head who carries a real, recorded Faction has that
audience read their own member's disgrace more harshly (`ScandalCatalog.FactionAlignedReadingPenalty`) than
the other one does — a real, if simple, "we expected better of our own" hypocrisy reading — while a head
with no recorded Faction (nearly everyone, per §3.1's own "the political cast" framing) reads identically
to both audiences. `CurrentFameEffect` stays permanently `null`, matching `Agnomen.FameEffect`'s identical
"the field exists, nothing can set it yet" precedent (Fame itself does not exist anywhere in this codebase,
reconfirmed directly rather than assumed); `NotaCensoriaIssued` stays permanently `false`, since §7's own
"sitting Senator" precondition can never be checked — Phase 12 item 2's own doc comment already omitted
`consul`/`praetor`/every Rome-track office from `MagistracyOffice` entirely. `ScandalSeverity.NotaCensoriaEligible`
is still a real, reachable severity tier despite that — severity and the formal Nota Censoria consequence
are two separate facts, and this item reaches the first without ever reaching the second (see below).

The design doc's own §11 data-model sketch also names a `damageControlActionsTaken` list and a
`dynastyChronicleEntryId` back-reference; `ScandalRecord` deliberately omits both rather than including
them unpopulated. Damage Control (§8) is a genuine, reasoned cut in its entirety: Suppression needs a real
spread-to-Scope mechanism to cap (Notable Households' ambient-spread "crowd" and Correspondence &amp;
Letters' distant-spread channel, §5, neither built); Spin needs a scored competition against felt severity
that does not exist; Scapegoating's own moral/mechanical weighting is explicitly left open by §12 itself.
A Chronicle back-reference does not fit this codebase's own real architecture at all — confirmed by direct
search, not assumed: `ChronicleGenerationSystem` mints a fresh `RuntimeId` for a `ChronicleEntry` strictly
after the tick that produced the source event, and no other domain record anywhere in this codebase (not
`LegalCase`, not `SentenceRecord`) is ever written back to with the resulting entry ID, so modeling one
here would invent plumbing no other Phase 12 item actually builds.

Four real, reachable sources are wired, each an additive call into an already-shipped, already-tested
command rather than a reopening of one — every existing test in `LegalTests.cs`/`CrimeTests.cs`/
`CollegiaTests.cs` that asserts an exact Dignitas or relationship-count value for these call sites still
passes unchanged, confirmed by running those suites against this item's own changes. `LegalCaseRuling`'s
Patria Potestas ruling (§4's "a politically-weaponized Legal &amp; Court case," §6) gets a new, additive
`RecordScandalCommand` call alongside its own existing, untouched `ApplyScandalMark` — `ApplyOrdinaryDignitasPenalty`
off, since the harsher `PatriaPotestasCaseDignitasPenalty` already applied above is the only Dignitas
movement this ruling ever produces. `ImprisonCommand`'s Unjust branch and `ApplySentenceCommand`'s Unjust,
execution-resulting branch both cover §4's "an Unjust imprisonment or execution" — each already applies its
own Dignitas penalty and relationship scar (Crime &amp; Punishment's own already-tested behavior), so both
flags are off there too; the genuinely new consequence each adds is the `ScandalRecord` itself and, for the
first time, a real Scandal-Marked Trait grant on the actor who exercised unjust power (`ImprisonCommand`) or
the named sentencing Character (`ApplySentenceCommand` — skipped entirely when no `SentencingCharacterId` is
supplied, matching that command's own identical "only has somewhere real to land when a specific Character
is named" precedent for its relationship scar). `DissolveCollegiumCommand` covers §4's "an Illicit
Collegium's exposure" (§7) — deliberately *not* wired at `RecordCollegiumOrganizedDisruptionCommand`'s own
earlier Unjust-flip-to-Illicit moment, since §4's own source language is specifically "a patron's own public
association with a *dissolved*, disgraced collegium," and dissolution, not the flip itself, is the real
exposure event §7 describes.

`DiscoverFabricationCommand` is this item's one genuinely new command outside the shared primitive itself,
and its own real payoff: Phase 12 item 5's `PunishableOffense.FabricationDiscovered` field has sat
permanently `false` since that item shipped, its own doc comment naming it "a real, present hook...
specifically so that future Scheme type has somewhere to land" while noting "no caller in this codebase
ever sets [it] true." This item is not that future Scheme type (Crime &amp; Punishment §9's own Fabricating-
Justification-as-a-Scheme-type loop stays unbuilt, per that item's own explicit cut) — but §4's "a
discovered Fabrication... retroactively the single worst-case scandal source this project has built" only
needs *something* real to flip that flag, not specifically a Scheme. `DiscoverFabricationCommand` is that
real, narrow, separate primitive: a new command touching `WorldState.PunishableOffenses` directly (not a
reopening of `RecordPunishableOffenseCommand`'s own pipeline), gated on the offense actually being
`IsFabricated` and not yet discovered, which flips `FabricationDiscovered` true and additively records a
`ScandalRecord` at `ScandalSeverity.NotaCensoriaEligible` — this item's one real path to that severity tier,
even though (as above) the formal Nota Censoria consequence itself still never fires.

`ScandalDecaySystem` (§9, "an ordinary Scandal's own felt severity fades over time if not actively
refreshed by a further incident, eventually settling into background Dynasty Chronicle memory rather than
an active, ongoing penalty") matches `FavorExpirationSystem`'s identical age-gated shape directly: at
`ScandalCatalog.SeverityFadeAfterMonths` a still-active record's severity steps down one rung, and at the
further `DeactivateAfterMonths` gate it is set `IsActive` false outright. `ScandalRehabilitationSystem`
(§8) is Rehabilitation's own concrete trigger, matching `FavorExpirationSystem`'s and
`MagistracyTermSystem`'s identical age-gated-check shape: every month, every living Character who still
carries Scandal-Marked but not yet Rehabilitated is checked against their own household's most recent
`ScandalRecord` (a further incident of *any* severity resets the clock), and past
`ScandalCatalog.RehabilitationAfterMonths` is granted the new **Rehabilitated** Reactive Trait — authored
directly into `content/source/traits/legal.json` alongside `scandal-marked` (the same "nothing else in this
codebase had built it yet" reasoning Phase 12 item 4 already used to author that trait and its two
siblings), additively rather than as a replacement, so the earned redemption sits beside the enduring mark
rather than erasing the history that produced it. `ChronicleProjector` gains two new cases, matching
`InsolvencyStageChangedEvent`'s own "only the severe/terminal rung is Chronicle-worthy" precedent: a
`ScandalRecordedEvent` only when it actually carried the Scandal-Marked Trait (Legendary for a
Nota-Censoria-severity case, Major otherwise), and a `CharacterRehabilitatedEvent` unconditionally, both
into the existing `FaithAndScandal` category rather than a new one this item invents.

**Explicitly not built, matching this item's own scope, after real investigation rather than assumption:**
the remaining five §4 sources — an affair's discovery (Romance, Sexuality &amp; Lineage §11, Phase 17,
unbuilt), a Scandalous theatrical performance and a Fame Collapse via disgrace (Games &amp; Spectacle,
Celebrities &amp; Influential Figures, Phase 17 — Fame itself confirmed not to exist anywhere in this
codebase), aggressive Publicanus tax-farming corruption (Land Ownership &amp; Real Estate, Phase 15,
unbuilt), and a deliberately weaponized rumor (Characters' own "Spread a Damaging Rumor" Interaction,
confirmed by direct search to exist only as a named row in `gens-characters-design.md` §9.4's own table,
with nothing in `Gens.Simulation.Interactions` implementing it) — are each named in `ScandalSourceType` for
schema completeness but never rolled by any caller in this codebase, matching `LegalCase.CaseType`'s own
"every real category represented, only some reachable" precedent. The Rumor Mill (§5) in its entirety —
ambient spread (Notable Households, unbuilt), distant spread (Correspondence &amp; Letters, unbuilt), and
deliberate acceleration via a Libellus Famosus or the still-unbuilt Rumor Interaction — has nothing real to
spread through yet, so `ScandalScope` never moves past its own fixed `SettlementWide` default and
`OriginatedViaLibellusFamosus` stays a real, settable-but-never-set flag on `RecordScandalCommand`, matching
`PunishableOffense.IsFabricated`'s identical "the flag is real, nothing yet has a reason to set it true"
precedent. Damage Control (§8) — Suppression, Spin, and Scapegoating alike — is not built, for the reasons
given above. Nota Censoria's own formal consequence (§2, §7) never fires, since no Rome-track magistracy or
Senator concept exists anywhere in this codebase for its "sitting Senator" precondition to ever check
against. Covered in `tests/Gens.Simulation.Tests/Scandal/ScandalTests.cs`, including `RecordScandalCommand`'s
own ordinary-case bundle (Dignitas, relationship scar, idempotent Trait grant) and Faction-dependent
reception reading real `CharacterFactionAlignment` values, all four real wired sources (an Unjust Imprison,
an Unjust execution with a named sentencer, a Patria Potestas ruling, and an Illicit Collegium's
dissolution) each verified not to double an already-tested call site's own existing Dignitas movement,
`DiscoverFabricationCommand`'s flag-flip and its Nota-Censoria-severity Scandal, the lifecycle decay/fade
and Rehabilitation's trigger (including a further incident resetting its own clock) and Trait grant,
Chronicle projection for a severe case and for Rehabilitation, and a save/load round trip with the
deterministic state hash staying stable across every new partition.

**Item 8 progress:** Fame lands as a new domain, `src/Gens.Simulation/Fame/`
(`gens-celebrities-influential-figures-design.md`, extending `gens-games-spectacle-design.md` §2). This
item is the same "build the shared engine now, before the design doc's own claimed owner has shipped"
move Phase 12 item 1 already made for Dignitas — every earlier item's own doc comment (`Agnomen.FameEffect`,
`ScandalRecord.CurrentFameEffect`, item 1's own `HouseholdReputation` note) says some version of "Fame is
a universal 0-100 Character field owned by Games &amp; Spectacle (Phase 17)... neither built"; Games &amp;
Spectacle has still not shipped, but this roadmap's own Phase 12 construction order places "Fame/celebrity
and public endorsement" here, at item 8, ahead of Phase 17 — so this item builds the primitive itself
rather than waiting on a document that was never actually going to land first. `CharacterFame` is a new,
sparse `WorldState` partition, following `HouseholdReputation`'s own conventions with one deliberate
divergence: it is keyed by Character, not Household, matching §1's own explicit "lives on Character schema
itself" — the one Phase 12 reputation-style primitive that is genuinely Character-level rather than
household-level, since Dignitas itself was built household-level in item 1 specifically because no
Character-level primitive existed yet to move instead. Clamped to §1's own explicit 0-100 range, unlike
Dignitas's deliberately unclamped total. `AdjustFameCommand` is the one command path (rule 2) every real
or future source — Oratory, Literary Work, Wanderer Renown, Military Valor, Romance/Scandal, Athletics,
Religious Charisma, Arena/Circus/Theatre (`FameSourceType`, §3's own full vocabulary) — is meant to route
through, matching `AdjustDignitasCommand`'s identical "the primitive ships, the callers don't exist yet"
precedent: direct search confirms every one of those eight sources needs a system this codebase has not
built (Legal &amp; Court's own advocacy machinery is the closest existing candidate for Oratory, but
nothing in this codebase currently generates Fame from a case outcome), so this item is exercised directly
by its own tests standing in for those future callers, same as item 1 was for Dignitas.

`FameDecaySystem` matches `InfluenceCycleSystem`'s identical "no per-source last-touched timestamp exists,
so a flat monthly decay applies to every stored balance uniformly" shape (Games &amp; Spectacle §2's own
"decays slowly if genuinely inactive," reused directly since this item is now that field's real, load-bearing
owner in practice). `FameDivergenceQuery` (§2's own "Fame/Dignitas Divergence... not a new number to track,
simply the descriptive gap between two fields this project already has") is a pure, non-mutating read
computed from `FameResolver.Current` and the Character's own household `DignitasResolver.Current`, never
stored, matching §11's own "descriptive-only for now" framing directly. Its one real, named judgment call:
§2's "famous and disreputable at once" divergence is properly about Infamia (Crime &amp; Punishment §13,
Romance, Sexuality &amp; Lineage §13), and no Infamia status exists anywhere in this codebase (both Phase
17, unbuilt, confirmed by direct search) — so this query reads a Character's own household Dignitas against
a single threshold instead, a real, reasoned proxy this item's own doc comment names directly rather than
silently conflating the two, not a stand-in Infamia flag or an invented three-way band. §5's endorsement
mechanic ("a crowd that loves a famous charioteer is a crowd more receptive to whichever candidate that
charioteer is seen publicly favoring") is wired as a real, additive extension to item 2's own
`HoldContestedElectionCommand`: two new optional parameters, `EndorsingCelebrityForChallenger`/
`EndorsingCelebrityForIncumbent`, both defaulting to `null`, add a flat score bonus when the named
endorsing Character's own Fame clears `FameCatalog.EndorsementFameThreshold` — the direct individual-scale
complement to that command's own existing Faction-alignment bonus. Every already-shipped Phase 12 item 2
test still submits an election the old way and is untouched: this is the same "additive, non-behavior-
changing extension to an already-tested command" precedent item 7 already established for
`LegalCaseRuling`/`ImprisonCommand`/`ApplySentenceCommand`/`DissolveCollegiumCommand`, applied here to a
command's own parameter list rather than its internal call sites.

**Explicitly not built, matching this item's own scope, after real investigation rather than assumption:**
none of §3's eight Fame sources is ever actually rolled by a real caller — each needs a system this codebase
has not built (see `FameSourceType`'s own doc comment for exactly which one each source needs: Wandering
Populations and Education &amp; Culture's Literary Patronage, both Phase 13/unbuilt; Military &amp; Combat,
Phase 16; Romance, Sexuality &amp; Lineage and Games &amp; Spectacle itself, both Phase 17; Starting Regions:
Greek East's athletic-games content and a religious-charisma concept, neither built) — matching
`ScandalSourceType`'s own identical "every real category represented, only some reachable" precedent. §6's
Risk and Reward of Association (Collegia patronage, Wanderer hosting, overt social association) and §7's
sudden Fame Collapse are both real, reasoned cuts: `ScandalSourceType.FameCollapse` already sits in item 7's
own enum waiting for exactly this item to make Fame real, but wiring a real Fame-collapse trigger into
`RecordScandalCommand` means reopening an already-shipped, already-tested Phase 12 item 7 command outside a
mere additive-parameter extension, which is out of this item's own scope the same way item 1 declined to
retrofit Agnomen and the Funerary Grand-tier trade. §8's Household-Grown Celebrities needs a household's own
Ludus-trained gladiator (Games &amp; Spectacle), literary/oratorical gift (Education &amp; Culture), or
battlefield valor (Military &amp; Combat) — none built. Covered in `tests/Gens.Simulation.Tests/Fame/FameTests.cs`,
including `AdjustFameCommand`'s clamping at both ends of the 0-100 range and its validation, `FameDecaySystem`'s
monthly erosion (including the zero floor), `FameDivergenceQuery`'s three real categories plus the untouched
default, the endorsement bonus actually moving an election's winning score, and a save/load round trip with
the deterministic state hash staying stable across the new partition.

**Item 9 progress:** Household Doctrine and real Edicts land as two new domains,
`src/Gens.Simulation/Doctrine/` and `src/Gens.Simulation/Edicts/` (`gens-policies-edicts-design.md` §3, §5),
closing out Phase 12. Per this item's own direction to pick "a real, coherent, testable slice" rather than
the design doc's full twelve-Policy/eight-Edict/seven-Doctrine roster, this item builds exactly three real
Doctrines, three real Edicts, and zero new Standing Policies — a deliberate, investigated choice explained
below, not an oversight. `HouseholdDoctrineType` (§3.2) names all seven Doctrines for schema completeness,
matching `LegalCase.CaseType`'s and `ScandalRecord.SourceType`'s own "every real category represented, only
some reachable" precedent; only `MosMaiorum`, `DomusPia`, and `DomusDura` are ever resolved above
`DoctrineTier.None` by `DoctrineResolutionSystem`, because those three are the only Doctrines whose §3.2
feed conditions already have real, already-shipped state to read — `ResPublicaPopularis` and
`DomusBellatrix` both need Patronage Generosity (§2.8) and Recruitment Doctrine's own Intensity dial
(§2.5), neither of which exists anywhere in this codebase (confirmed by direct search — the only
pre-existing Policies partition before this item was `HouseholdPolicyState.RitesBudget`, Phase 9 item 2);
`DomusMercatoria` needs Trade Openness (§2.7) and a regional Market Dynamics pricing read; `DomusProvincialis`
needs Provincial Administration Posture (§2.10) and real foreign-cult engagement, the latter already
deferred by Phase 12 item 3 pending Religions of the Known World content. This is this item's own answer to
"extend Standing Policies only as needed": the three real Doctrines it builds are fully fed by facts three
already-shipped items already track (Rites Budget and Faction from items 2/3, the household-wide Regimen
default from Phase 6's own `HouseholdRegimenDefaults` at a null duty slot — which already *is* §2.2's
"Household Regimen Posture" Standing Policy in practice, confirmed directly rather than assumed), so this
item adds no new Standing Policy dial at all rather than inventing one just to complete a formula the
chosen Doctrines don't actually need. `HouseholdDoctrineState` is a new sparse partition keyed by
(household, Doctrine type) — the one deliberate structural divergence from every other Phase 12 partition's
single-household key, needed because a household holds an independent Affinity/Tier pair per Doctrine, not
one shared status. `DoctrineResolutionSystem` reads each real Doctrine's own signal monthly (a match point
per condition met, a mismatch point per condition actively contradicted, matching §3.1's "matching choices
raise Affinity; contradicting choices lower it; unfed Affinity decays slowly on its own" precisely) and
moves Affinity through the Emerging/Defining thresholds (§3.1) — `DoctrineTier.Apex` is kept in the enum for
data-model completeness with §9's own `tier` sketch but is never assigned: §3.3's own real precondition
("Defining survives a succession event with continued matching policy") needs a succession-event hook this
item does not build, a genuine, reasoned cut rather than a blocked one, since `Succession.SuccessionDispute`
and `HouseholdHeadship` both already exist and a future pass has a real, named place to attach that check.
Each real Doctrine's own Defining capstone is a real, separately testable command: `InvokeAncestralSanctionCommand`
(Mos Maiorum's "overturn a Legal & Court ruling") reaches into `WorldState.LegalCases` directly — a new
command touching that partition, not a reopening of `LegalCaseRuling`'s own already-shipped, already-tested
Apply pipeline, matching `DiscoverFabricationCommand`'s identical "a new command touching that partition
directly" precedent — and restores a real, partial share of the conviction's own Dignitas penalty through
`AdjustDignitasCommand` rather than fully erasing the case; `PerformGreatRiteCommand` (Domus Pia's "Edict-scale
ceremony") is a real, fixed-cost Ledger spend into its own named sink, gated on the household actually having
chosen a Patron Deity, granting real Favor and Dignitas through `AdjustFavorCommand`/`AdjustDignitasCommand`;
`ActivateIronHandCommand`/`DoctrineLaborModifierQuery` (Domus Dura's "single highest sustained labor-output
multiplier... permanent Unrest/flight-risk baseline increase") is this item's one capstone that is a real,
projected numeric effect rather than a further state mutation, matching `RitesBudgetCatalog`'s own "the
projection exists before its consumer does" precedent: `Characters.LaborOutputSystem` and
`Characters.LaborFlightSystem` are both already-shipped, already-tested systems this item does not reopen to
actually fold the projection into their own live formulas. "Once per generation" (§3.1's own capstone framing)
is honestly narrowed to "once per campaign" for all three capstones: `HouseholdDoctrineState.CapstoneUsedThisGeneration`
never resets, since no succession-event hook exists yet to reset it against — the same gap Apex's own cut
already names.

Edicts (§5) land as `Edicts.EdictType`'s full eight-value roster for schema completeness, matching every
other Phase 12 enum's identical precedent; only `ManumissionEdict`, `CitizenshipGrant`, and `Proscription`
are ever issuable. `EdictRecord` is a new kept-forever partition (household-level issuer, matching
`AdjustDignitasCommand`'s and `LegalCase`'s own convention) with one command per real type (rule 2's "one
command path" applied per mutation kind, the same shape Legal & Court's own several distinct commands
already established), sharing only the two steps every real Edict needs (`EdictIssuance.ChargeCosts`/
`RecordReception`) rather than one generic do-everything command. Every real Edict costs real Influence
(`Clientela.InfluenceResolver`) and Dignitas (`AdjustDignitasCommand`) to issue, per §5.1's own "every Edict
costs real Influence and Dignitas to issue." Reception (§5.1's "a genuine backlash chain... capable of
escalating into a Scheme, Legal & Court case, or Private Feud") is real, not invented from scratch, per this
item's own required direction: every real Edict's backlash routes through Phase 12 item 7's own Scandal
engine via a new, purely additive `ScandalSourceType.EdictBacklash` enum value — exactly the kind of moment
that item's own §1 framing named directly ("not a new consequence system, but the shared engine... a handful
of already-shipped Phase 12 moments have been quietly waiting for"), confirmed not to change any existing
`RecordScandalCommand` behavior since that command's own Dignitas penalty already switches on `Severity`
alone, never `SourceType`. `IssueManumissionEdictCommand` (§5.5) is an additive loop of already-shipped,
already-tested `Characters.ManumitCommand` calls (Vindicta) across every living Enslaved member of the
issuing household, matching Phase 12 item 8's own "additive extension to an already-tested command"
precedent applied to a loop of calls rather than new parameters, plus a real Dignitas gain and a Favor gain
when the household has a chosen Patron Deity. `GrantCitizenshipEdictCommand` (§5.6) is the first command
anywhere in this codebase to write `Character.LegalStatus` to `RomanCitizen` directly, and optionally files a
real `FileLawsuitCommand` (Political, Major) contesting the grant's own validity when a `ChallengerHouseholdId`
actually resolves to a household with a recorded head — optional and defaulting to null, matching Phase 12
item 8's own `EndorsingCelebrityForChallenger`/`ForIncumbent` "both defaulting to null" precedent, since this
item cannot invent an antagonist household to force a challenge with. That challenge is filed at Major depth
specifically because `FileLawsuitCommands.CreatePipeline` needs a `RandomStreamSet` only for a Quick case's
own inline verdict roll (confirmed directly in that command's own `Mutate`) — a Major filing never touches
it, so this command passes a fresh, unregistered stream rather than threading a real named one through a
static pipeline with nowhere to receive it, and the filed case proceeds through `LegalCaseAdvancementSystem`'s
own already-shipped progression like any other Major case. `IssueProscriptionCommand` (§5.7, "the single
darkest Edict available") is gated on the issuing household holding an active Duumvir seat — the top of
Local Magistracies' own four-office ladder (Phase 12 item 2), read as "Duumvir-or-above" since nothing is
above it in `MagistracyOffice`; §5.7's own alternate civil-crisis Event gate is a named, reasoned cut, since
no Events system entry anywhere in this codebase carries a civil-crisis classification a command could check
(the identical finding Phase 12 item 3's own Omens work already made for a different Event-gated mechanic).
Its effect is real across three already-shipped systems: asset seizure is a real `LedgerService.Post`
transfer out of the target Actor's own `LedgerAccountKey.ForActor` account (Phase 12 item 6's own Arca
convention, applied to seizure instead of funding) capped at `EdictCatalog.ProscriptionMaxSeizure`; a
relationship-web scar lands between the issuing household's own recorded head and the target's own resolved
head Character, matching every other Phase 12 household-vs-Actor consequence's "the player's own Household
is never itself a `LivingWorldActor`, so household-to-Actor consequences land on recorded heads instead"
precedent (Phase 12 item 1's own `HouseholdReputation` doc comment); and §5.7's own demonstration effect
("every regional Rival House shifts toward Wary or Hostile") is a real, additive `AdjustHouseStandingCommand`
call per other real, tracked `LivingWorldActorType.Gens` Actor — entirely Actor-to-Actor, and so reachable
even though the issuing household itself has no Actor id to be a party to that command directly. A real,
issued Proscription is also `Doctrine.DoctrineResolutionSystem`'s own heavily-weighted Domus Dura signal
(§3.2's "at least one Proscription issued"), the one place this item's two new domains compose directly
rather than sitting side by side.

**Explicitly not built, matching this item's own narrow scope, after real investigation rather than
assumption:** `TabulaeNovae` and `DebtBondageBan` both need a real write path onto Economy & Finance's own
`DebtRecord`/debt-bondage machinery this item's household-vs-household Edict engine does not reach into — a
real, narrow future integration point, not a blocked one; `GeneralAmnesty` needs a real "pardon a standing
sentence" write path onto Phase 12 item 5's own already-shipped, already-tested `SentenceRecord`/
`DetentionRecord` commands, and reopening either is out of this item's scope, matching Phase 12 item 1's own
"already-shipped, already-tested, out of scope to reopen" precedent for `Agnomen.DignitasEffect`;
`LandRedistribution` needs Land Ownership & Real Estate's own `PropertyRecord` type, confirmed by direct
search not to exist anywhere in this codebase (Phase 12 item 6's own identical finding for the Collegia
Schola); `GrainRequisition` needs a real Coloni harvest/Contentment write path this item does not reach into
either, the same unreached-consumer shape Phase 12 item 3's own Sacred Calendar left for Settlement
Demographics. Funded Actions (§4) are deliberately not re-authored or unified into one generic abstraction
here: `Policies.FundFestivalCommand` and Phase 12 item 3's own `FundFestivalCelebrationCommand` both already
exist, already tested, and that item's own doc comment already named "a future Policies & Edicts pass (§6.12,
roadmap item 9)" as the natural place to unify them — reopening either is out of this item's scope for the
identical "already-shipped, already-tested" reason named throughout this phase; this item's own two Funded
spends (`PerformGreatRiteCommand`'s Great Rite, priced independently) follow that precedent rather than
inventing the generic `FundedAction` abstraction §9's data model sketches. Policy Playbooks (§6) are not
built: no real quality-of-life need exists yet to save/recall a snapshot of a Standing Policy roster this
item leaves at its Phase 9 item 2 size (one real dial, Rites Budget) — a Playbook over one dial does not
demonstrate the mechanic §6 actually describes. Hybrid Doctrine titles (§3.4) are not built: every named
pair needs at least one Doctrine this item does not make reachable (Res Publica Popularis, Domus Mercatoria,
Domus Bellatrix, Domus Provincialis), leaving only Mos Maiorum + Domus Pia ("Keepers of the Rite") and
Domus Bellatrix + Domus Dura pairs real reachable inputs could ever form, and the latter needs Domus
Bellatrix too — a single reachable pair is not enough to demonstrate a "several commonly co-occurring pairs"
system honestly. The remaining nine of twelve Standing Policies (§2.1, §2.5-§2.12 beyond what already
existed) are not built, per this item's own investigated finding that its three chosen real Doctrines need
none of them — a future pass building Res Publica Popularis, Domus Bellatrix, Domus Mercatoria, or Domus
Provincialis for real is what would actually need them, not this one.

**On Phase 12's own exit gate** ("the same underlying action can produce different legal, reputational,
religious, and factional consequences according to actor, status, audience, evidence, and place—without
bespoke shortcuts for each screen"): this item's own three real Edicts are the clearest end-to-end
demonstration built in this phase — the same Edict-issuance shape produces a Ledger-real Dignitas/Influence
cost, a Faction-and-severity-scoped Scandal reception (item 7's own audience-differentiated reading),
optionally a real Legal & Court case (item 4), a real relationship-web scar and Rival House standing shift
(items 1 and Phase 10), and a real Doctrine-Affinity feed (this item), all from one command, without any of
those five consequence systems needing a bespoke code path built just for Edicts. The gate is met by this
phase's cumulative work across all nine items, not by this item alone; this item's own honest limitation is
breadth, not depth — only three of twelve Standing Policies, three of seven Doctrines, and three of eight
Edicts are real, each for a specifically investigated, cited reason above, not a blanket "out of scope."
Covered in `tests/Gens.Simulation.Tests/Doctrine/DoctrineTests.cs` and
`tests/Gens.Simulation.Tests/Edicts/EdictTests.cs`, including all three Doctrines' Affinity rising with
their own real matching signals and reaching Emerging/Defining, unfed decay, a contradicting-signal reversal,
each capstone's own Defining-tier/single-use gates and real effects (Ancestral Sanction's verdict overturn
and partial Dignitas restore, the Great Rite's Ledger spend and Favor/Dignitas grant, Iron Hand's projected
labor modifiers), all three Edicts' validation and happy paths (Manumission freeing every real enslaved
member, Citizenship Grant's status change and its optional real Legal challenge, Proscription's asset
seizure/relationship scar/demonstration effect and its own Domus Dura Doctrine feed), and a save/load round
trip with the deterministic state hash staying stable across every new partition.

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

### Phase 13 — Add geography, travel, correspondence, culture, and history — ✅ COMPLETE

**Outcome:** distance, language, local rules, and historical time change what is possible.

Construction order:

1. [x] Implement the region profile schema and date-aware rule overrides.
2. [x] Implement location, route, distance tier, travel party, reservations, duration, risk exposure, arrival, and concurrent character locations.
3. [x] Implement letters/messages, courier selection, transit, delivery, response, interception, forgery, and information provenance.
4. [x] Implement culture and language definitions, literacy, fluency, interpreters, naming pools, and visibility/interaction gates.
5. [x] Implement the historical timeline scheduler with immutable history, divergence-eligible events, counterfactual flags, and date-aware content validation.
6. [x] Implement one complete region profile and only then expand region content in waves.
7. [x] Add distant holdings and procurator requirements after travel and delegation are stable.

**Exit gate:** a household can act locally, travel, communicate at distance, encounter language/cultural gates, and receive date-appropriate historical events without loading broad region-specific code.

**Primary design inputs:** `gens-starting-regions-design.md`, `gens-travel-design.md`, `gens-correspondence-letters-design.md`, `gens-language-literacy-design.md`, `gens-cultures-of-the-known-world-design.md`, `gens-religions-of-the-known-world-design.md`, `gens-events-historical-timeline-content.md`.

Region data waves should consume the shared schema, not introduce new mechanics ad hoc. The current region corpus is: `gens-starting-regions-italian-heartland-design.md`, `gens-starting-regions-gallic-frontier-design.md`, `gens-starting-regions-iberian-colony-design.md`, `gens-starting-regions-north-african-colony-design.md`, `gens-starting-regions-greek-east-design.md`, `gens-starting-regions-britannia-design.md`, `gens-starting-regions-egypt-design.md`, `gens-starting-regions-syria-levant-design.md`, `gens-starting-regions-anatolia-design.md`, `gens-starting-regions-balkans-design.md`, `gens-starting-regions-sicily-design.md`, `gens-starting-regions-alpine-provinces-design.md`, `gens-starting-regions-armenia-design.md`, `gens-starting-regions-mesopotamia-design.md`, `gens-starting-regions-nubia-design.md`, `gens-starting-regions-arabia-felix-design.md`, and `gens-starting-regions-bosporan-kingdom-design.md`.

**Item 1 progress:** the Region Profile content-definition family lands in a new
`src/Gens.Simulation/Regions/` namespace, following `Gens.Simulation.Events`'s identical
sealed-record-plus-catalog shape (`RegionProfileDefinition`/`RegionProfileCatalog`, mirroring
`EventDefinition`/`EventCatalog`). Every `gens-starting-regions-design.md` §4 subsection is
represented — §4.1-§4.6 as qualitative string refs/tags per §4's own "numbers are deferred
everywhere" convention (the numeric packages stay Start Modes' territory), §4.7's culture
distribution as a weighted `CultureDistributionEntry` list requiring exactly one outlier-residual
row, §4.9's gazetteer as `GazetteerLocationDefinition` entries each carrying its own `RegionId`
back-reference plus Role/Prominence Tier/grounding note/optional rival-seat tag, and §4.10/§6's
Reputation Duality as the five-value `ReputationDualityMode` enum (`None`/`Full`/`Tapering`/
`PermanentStructural`/`Localized`). The date-aware rule override mechanism §6's tapering case calls
for (Iberian Colony, North African Colony modulating by start year around the Cantabrian Wars'
29-19 BC close) is a general, reusable `DatedRule<TValue>`/`DatedOverride<TValue>` pair — a base
value plus non-overlapping, `GameDate`-bounded override windows resolving to "effective value as of
date X" — not a one-off if/else; it is generic over any §4 field's type, and this pass wires it to
`RegionProfileDefinition.ReputationDuality` as its one concrete consumer. Validation matches
existing content-family conventions (constructor-time `ArgumentException`s, catalog-time duplicate-ID
rejection): every gazetteer entry's `RegionId` must match its owning region, the Home Anchor must be
a real gazetteer entry of that region, culture weights must be positive with exactly one
outlier-residual row, override windows must not overlap, and (a catalog-wide check) at most one
gazetteer entry across every registered region may carry the Rome-only `Capital` role (§8.3). One
fixture region (`SampleRegionProfileDefinitions`, deliberately `sample-*`-named per
`SampleEventDefinitions`'s own precedent) exercises every field including a Full-before/Tapering-at-
and-after dated override on Reputation Duality; item 6's actual authored region content is explicitly
out of this item's scope. Covered by 28 new tests across
`tests/Gens.Simulation.Tests/Regions/{DatedRuleTests,RegionProfileDefinitionTests,RegionProfileCatalogTests,SampleRegionProfileDefinitionsTests}.cs`.
The pre-existing `src/Gens.Simulation/Land/Region.cs` (a `RuntimeId`-keyed settlement-ownership
boundary, Phase 6) is untouched — it answers "which region does this settlement belong to," a
different question from this content-definition schema's "what is a region's authored shape,"
and nothing here changes its callers.

**Item 2 progress:** the Travel data/state model lands in a new `src/Gens.Simulation/Travel/`
namespace, closing every construction-order sub-item against `gens-travel-design.md` §10's own
`Location{}`/`TravelTrip{}` sketch. `TravelLocation` is a value type — not its own `RuntimeId`-keyed
partition — since every kind's identity already resolves through an entity that exists elsewhere
(a `Settlement`, a `RegionProfileDefinition`, a Rival House's `Actor`); its `RegionId` field is this
item's own addition atop §10's literal shape, since nothing yet links a runtime `Settlement`/`Region`
back to the content `RegionProfileDefinition` (Region Profiles, Phase 13 item 1) a Distance Tier
lookup needs, and Rome specifically carries no region of its own, instead resolving via whichever
region's Gazetteer seats it `Capital` (`gens-starting-regions-design.md` §8.3) — a real point of reuse
between the two items. `DistanceTierCatalog` builds the general Near/Moderate/Far lookup mechanism
§7.1 asks for, deliberately leaving the actual region-pair contents unauthored per §13's own open
question; `TravelRoute.Resolve` combines a Distance Tier with §4's route-danger classification
(`RouteRiskLevel`) and this item's own invented, disclosed Travel Time baseline (1/3/6 months), mirroring
`DutySlotCatalog`'s "invented numbers, openly labeled" precedent. `TravelTrip` is a real
`WorldState` partition (`TravelTrips`, `TravelTripIds`), fully wired through `WorldSaveDto`/
`WorldStateMapper`/`StateHasher`/`EntityKinds`, since a trip's own leg progress is genuine campaign
state, unlike Region Profiles' pure content. `BeginTravelCommand` resolves the route and reserves
every `TravelParty` member (§5) — a Character already on a non-`Completed` trip cannot be booked onto
a second one, via `TravelTripQueries.IsReserved`; `TravelProgressSystem` advances every trip's current
leg one month at a time and is what actually sets/clears a Character's new `CurrentTravelLocation`
field (§10's Characters-schema addition, defaulting `null`/"at home") on Arrival and on Completion;
`BeginReturnCommand` starts the return leg deliberately once Arrived; `RecallTravelCommand` implements
§5's Recall, always forcing `EncounterCompleted` false. Concurrent trips are a natural consequence of
`TravelTrips` being an ordinary keyed partition `TravelProgressSystem` iterates in full every tick,
not a queue — matching §5's "resolve fully concurrently" directly. The Retinue mechanic itself
(Companions & Court Positions §7, Encounter menus (§7), en-route event content (§4), and Correspondence
(§8) all stay explicitly out of this item's scope, per their own not-yet-built or future-phase status;
`TravelParty` only carries retinue member IDs, and `LocationKind.Campaign` is a reserved enum value
with no constructor yet, since Military & Combat (Phase 16) doesn't exist. Covered by tests in
`tests/Gens.Simulation.Tests/Travel/TravelTests.cs`, including a save/load round trip with a stable
deterministic state hash.

**Item 3 progress:** Correspondence & Letters lands in a new `src/Gens.Simulation/Correspondence/`
namespace, closing `gens-correspondence-letters-design.md` §11's own `Letter{}` sketch against real
`WorldState` partitions (`Letters`, `LetterIds`), fully wired through `WorldSaveDto`/`WorldStateMapper`/
`StateHasher`/`EntityKinds` exactly like `TravelTrip` — a letter's own transit progress is genuine
campaign state, not content. §3's "reuses Travel's own distance model" is taken literally: `LetterRoute`
calls the same `DistanceTierCatalog` Travel item 2 built and reproduces `TravelRoute`'s own 1/3/6-month
Tabellarius baseline exactly, rather than inventing a parallel distance system. §8's Courier Choice
(Tabellarius/Hired Carrier/Pigeon) carries no mechanical detail in the design corpus beyond its own
heading — unchanged from the superseded first-pass doc — so `CourierCatalog` invents and openly
discloses a speed/coin-cost/interception-risk tradeoff table per courier, mirroring `TravelRoute`'s own
"invented numbers, openly labeled" precedent for its Travel Time baseline; the coin cost is informational
only, not yet posted to the Ledger, matching how item 2 left the Retinue mechanic itself out of scope.
§7's Oral Tradition Problem is built as the general, reusable mechanism the task called for — a
`CorrespondenceReachability` three-value scale (`FullyLiterate`/`OralTraditionPartial`/
`OralTraditionBlocked`) plus a `CorrespondenceReachabilityCatalog` keyed by the existing
`DefinitionId<Culture>` phantom type, defaulting unlisted cultures to `FullyLiterate` (the honest
default per §7's own "not a blanket 'foreigners can't read' assumption" close, unlike
`DistanceTierCatalog`'s unrelated least-committal-middle-tier default) — exactly like item 2 built
`DistanceTierCatalog`'s general lookup without authoring real region-pair content, this item authors no
real culture list at all; which of Cultures' own thirty-six entries are Gallic/British/Germanic or
Thin-Record, and any Interpreter-equivalent mitigation, stays explicitly Phase 13 item 4's job (§12's
own open question). A `LetterActions.IsSubstantive` classification (this item's own invented reading,
since §7 never enumerates which of the nine `LetterAction` values count as "substantive") decides
whether a foreign culture's reachability level does anything at all — a `Blocked` route rejects the
command outright, a `Partial` route bumps the route's `RouteRiskLevel` up one step (this item's own
disclosed proxy for §7's unsized "meaningfully reduced effectiveness"), and every routine action
(News & Gossip, Condolence or Congratulation, Maintain a Distant Relationship, Early Courtship) always
gets through regardless. Every one of §5's nine correspondence actions (the six carried over plus News &
Gossip, Written Instructions to a Distant Appointee, and Condolence or Congratulation) is a complete,
correctly-shaped `LetterAction` value; three of them name a target system this codebase has not built
yet (Direct an Already-Placed Spy — Espionage; Early Courtship — Romance & Seduction; Written
Instructions to a Distant Appointee — Companions & Court Positions' Procurator), so `SendLetterCommand`
and `OriginateInboundLetterCommand` both transit those letters exactly like any other and leave the
actual game-logic payload as that future system's own job, not a fabricated integration. `SendLetterCommand`
begins an outbound letter (always `RequiresResponse = false`, since a response is something the
player's own correspondent might send back, not something this engine tracks against outgoing mail);
`OriginateInboundLetterCommand` is the symmetric entry point for §6's Inbox — "other Living World Actors
... can send a letter to the player" — that whatever future NPC-decision system (or a debug/test tool)
calls to start an inbound letter in transit, since deciding *when* an NPC should write is explicitly out
of scope (§12's own open "Inbox volume and pacing" question); `RespondToLetterCommand` is the real
command a response is, per this item's own task brief, rather than a flag flip, and `LetterQueries.
PendingInbox` is the read model surfacing which delivered, unanswered, actually-arrived letters still
need one — §6's "including no response at all" is a legitimate standing choice this engine never forces
out of. `CorrespondenceTransitSystem` advances every in-transit letter one month at a time exactly like
`TravelProgressSystem` advances trips, resolving two real, disclosed-invented risk mechanics from §9
once transit would otherwise finish: **interception/forgery** (one random draw against the route's own
risk tier plus the courier's own modifier, with a second draw deciding lost-outright versus forged-and-
passed-on) and **redirection** (checked at most once per letter, reusing Travel's own
`Character.CurrentTravelLocation` concurrent-location tracking from item 2 directly — a recipient away
from home adds one further invented month of delay before delivery actually resolves). Forgery detection
mechanics stay explicitly unresolved, matching §12's own open question — this item only records that a
forgery happened, not whether anyone in-fiction ever notices. Covered by 34 new tests in
`tests/Gens.Simulation.Tests/Correspondence/{CorrespondenceTests,CorrespondenceTestFixtures}.cs`,
including a save/load round trip with a stable deterministic state hash.

**Item 4 progress:** Culture and Language land in two new namespaces, `src/Gens.Simulation/Cultures/`
and `src/Gens.Simulation/Languages/`, finally backing the `Gens.Simulation.Identity.Culture` phantom
type and `CultureDistributionEntry.CultureRef` that items before this one only ever referenced as loose
strings. `CultureDefinition`/`CultureCatalog` cover Cultures of the Known World's real §17 data model
(category, `permanentlyUnconquered`, `isRaidingFrontier`, `isAuxiliaryServiceCulture`,
`encounterRarityTier`, `noveltyDignitasBonus`), with `KnownWorldCultures` authoring every real culture
§17's own enum literally lists — 37 values, not the doc's own "thirty-six" intro count, because §12's
own quick-reference table calls Roman "— (the default)" rather than one of the thirty-six added
entries; `KnownWorldCultures`'s own doc comment discloses that reconciliation rather than silently
matching the prose number. `CultureCategory`'s mid-range shifts (British AD 43, Dacian and Nabataean
both AD 106, Egyptian 30 BC, Pannonian ~AD 9) reuse item 1's own `DatedRule`/`DatedOverride` mechanism
exactly as the roadmap background suggested, closing the same "one general mechanism, many consumers"
loop `RegionProfileDefinition.ReputationDuality` opened. §11's Legendary Places and §3.2/§5.1/§6.1's
flavor-only minor sub-groups are deliberately not tracked values, per §17's own data model.
`Languages/` builds `LanguageDefinition`/`LanguageFamily`/`LanguageCatalog` and `FluencyTier`
(None/Basic/Conversational/FluentNative) from §2's real linguistic geography, with `KnownWorldLanguages`
authoring every language §2 actually catalogues — deliberately excluding §2.11's two ritual-only extinct
languages (Etruscan, Sicel/Sicani stay Religion's flavor content) and the thin Italic/Anatolian remnants
§2 itself never gives more than a footnote. One disclosed gap-fill: §2 has no dedicated "Germanic"
language entry despite §6's own hard-gate example naming a Germanic negotiation, so this item adds one,
named honestly in `KnownWorldLanguages`'s own doc comment rather than silently patched, in the same
spirit as this design pass's own stated Oscan/Noric corrections. `CultureLanguageMap` reads §5's native-
acquisition mapping directly off §2's prose for every culture it actually names a language for, and
returns `null` — not a fabricated answer — for the three cultures (Blemmyes, Garamantian, Taprobane) §2
never names one for. `LanguageCatalog.SharesNonIsolateFamily` builds §5's family-relationship discount
as a pure yes/no capability check without sizing the actual discount magnitude, which stays unsized per
§11's own open question. `LanguageProficiency` (own `RuntimeId`, since one Character legitimately holds
several at once per §8's "no artificial ceiling") and `LiteracyRecord` (keyed by `RuntimeId<Character>`
alone, mirroring `ClientelaEntry`'s "the owning entity is already a unique key" shape) are real
`WorldState` partitions, fully wired through `WorldSaveDto`/`WorldStateMapper`/`StateHasher`/
`EntityKinds` exactly like `TravelTrip` and `Letter` before them — a Character's own tracked language
and literacy facts are genuine campaign state, unlike the pure-content Culture/Language catalogs above.
`AcquireLanguageCommand` and `SetLiteracyCommand` record acquisition-method/derivation facts (§5's
`nativeOrigin`/`formalEducation`/`sustainedExposure`/`wandererInstruction`, §3's
`legalStatusAndWealth`/`learningAttribute`) without simulating the systems that would drive them —
Education & Culture's own Learning math, Distant Holding/Travel exposure accrual, and a Wanderer's own
teaching mechanic (Phase 14 item 4) stay unbuilt, per this item's own scope discipline, mirroring how
item 2 modeled `TravelParty` retinue IDs without building Companions itself.
`InterpresAppointment` (§7, keyed by `RuntimeId<Household>`, also real `WorldState`) is the small,
clearly-scoped slice of the Interpres Companion role buildable without Companions & Court Positions
(Phase 16 item 1, not yet built) — a household's standing designation of an already-proficient Character,
with no salary, slot-conflict, or recruitment flow invented on top. `DiplomacyLanguageGateEvaluator`
builds §6/§10's `DiplomacyLanguageGate` hard-gate check as a real, callable, fully tested mechanism
(negotiator fluency, then a formal Interpres, then §7's own "any Conversational-or-better Character can
serve informally") with no Diplomacy negotiation flow yet to call it from — Diplomacy with Non-Roman
Peoples is Phase 16, named as the future caller the same way item 3 named Espionage/Romance/Procurator
as future `LetterAction` callers it fully modeled anyway. `InteractionLanguageBarrier.Severity` builds
§6's soft-penalty half as a standalone, tested severity lookup rather than wiring it into
`RecordInteractionCommand`: every existing Interaction (`Befriend`, `InitiateScheme`) already takes its
opinion delta as a plain caller-supplied constant with no per-invocation attenuation mechanism to hook
into, and §6 itself never sizes the actual penalty magnitude, so deciding which future Interaction
applies this, and by how much, stays a real decision for whichever future pass builds that catalog out.
`CultureNamingPoolCatalog` (§13) turns "real patterns, not exhaustive lists" into real, drawable
`NamePool` content for the existing `CharacterNameGenerator` — the seven cultures §13 itself names a
convention for (Gallic/British/Hibernian's `-rix` suffix, Germanic compounds, Egyptian theophoric forms,
Judaean patronymic/theophoric Hebrew forms, Parthian/Armenian Persian-derived elements, Etruscan's own
non-Indo-European structure), deliberately not the full thirty-seven-culture roster, matching §13's own
title. Finally, `Correspondence/KnownWorldCorrespondenceReachability.cs` closes item 3's own
explicitly-left-open seam: real (culture, reachability) entries built against this item's own real
Culture/Language catalogs — `OralTraditionPartial` for §7's three named cultures (Gallic, British,
Germanic) plus its own "by extension" reading (Hibernian, Caledonian, and Batavian's shared
druidic-adjacent tradition; Thracian, Dacian, Illyrian/Pannonian, and Cappadocian/Anatolian's
thinly-attested language families), and a single `OralTraditionBlocked` entry for Nubian/Kushite on
Meroitic's own real, still-undeciphered script — every other culture stays at the catalog's own honest
`FullyLiterate` default. Covered by 53 new tests across
`tests/Gens.Simulation.Tests/{Cultures/CulturesTests,Languages/LanguagesTests,Correspondence/KnownWorldCorrespondenceReachabilityTests}.cs`,
including a save/load round trip with a stable deterministic state hash for every new `WorldState`
partition. Deliberately out of scope, named for later phases: Companions & Court Positions' own full
Interpres title/slot mechanics and Diplomacy with Non-Roman Peoples' own negotiation flow (both Phase
16), Education & Culture's Learning-investment acquisition math and Wandering Populations' teacher
mechanic (Phase 14), and any actual Interaction-catalog wiring for the language-barrier soft penalty.

**Item 5 progress:** the Historical Timeline lands in a new `src/Gens.Simulation/History/` namespace,
closing `gens-events-design.md` §10's own `HistoricalTimelineEntry{}`/`NamedHistoricalFigure{}`/
`DivergenceRecord{}` sketch against this codebase's real idioms rather than the doc's loose pseudocode —
`Date : GameDate` replaces §10's separate `realYear`/`realMonth` pair outright, since `GameDate` is this
codebase's one canonical time representation (`Time/GameDate.cs`) and a parallel year/month pair would
just be a second, driftable copy of the same fact. Converting the source docs' own "133 BC"/"AD 79"
display-year style into `GameDate` at roughly ninety authored call sites is exactly the kind of place an
off-by-one silently corrupts everything, so `HistoricalYear.ToGameDate(displayYear, isBce, monthOfYear =
1)` is a small, directly tested helper (`HistoricalYearTests.cs`, 9 tests covering the 44 BC/AD 79
round trips against `GameDate.ToDisplayYearLabel()`, the no-year-zero BCE/CE boundary, and both
out-of-range guards) rather than hand-computed inline; every authored entry defaults to January of its
real year, since none of the source content carries month-level granularity. `HistoricalTimelineRange`
fixes the supported 133 BC – AD 235 span as `GameDate` constants — `Start` inclusive, `End` exclusive
(January AD 236) so all of AD 235 stays in range — used both by `HistoricalTimelineEntryDefinition`'s own
constructor-time range validation and by the runtime Divergence-state computation below.

The content layer (`HistoricalTimelineEntryDefinition`/`HistoricalTimelineCatalog` and
`NamedHistoricalFigureDefinition`/`NamedHistoricalFigureCatalog`) mirrors `RegionProfileDefinition`/
`RegionProfileCatalog` and `EventDefinition`/`EventCatalog`'s identical "sealed record, constructor
validates, content is data" shape exactly. `HistoricalTimelineCatalog`'s constructor is this item's own
date-aware, cross-referencing content validation: every `InvolvedFigureIds` entry must resolve against a
supplied `NamedHistoricalFigureCatalog`, and — when a caller also supplies the real `EventCatalog` — every
non-null `LinkedEventDefinitionRef` must resolve against it too, matching `RegionProfileCatalog`'s own
Home Anchor/capital-uniqueness cross-reference convention; the `EventCatalog` parameter is optional
because most campaigns haven't necessarily loaded a full Events catalog by the time the Timeline itself
is built. `LinkedEventDefinitionRef` stays null across every one of the roughly ninety real authored
entries — authoring a full, multi-stage interactive `EventDefinition` for each one is real future content
work, explicitly out of this item's own scope; an unlinked entry still fires as a lightweight digest
event, matching §6.4/§7's own "always at minimum an Auto-Resolved digest line." `DivergenceEligible` is a
mechanical, disclosed rule applied to every authored entry rather than a per-entry editorial judgment
call: `true` for `ImperialSuccession`/`WarOrRevolt`/`PoliticalTrial` (§6.7's own examples — a claimant's
win, a war's resolution, a figure's scheduled death — are all exactly these three shapes), `false`
otherwise, since a real eruption or festival date isn't the kind of thing a household's political action
branches. `Chronological()` sorts by `Date` (then ID as a stable tiebreak) precisely because `All()` alone
doesn't guarantee order — a content author's own declaration order is never assumed sorted.

`KnownWorldHistoricalFigures`/`KnownWorldHistoricalTimeline` author every real figure and entry
`gens-events-historical-timeline-content.md` §2-§6 actually lists, following item 4's own
`KnownWorldCultures`/`KnownWorldLanguages` "real content, not a fixture" precedent — all 43 named figures
(the ten Republic-era names, all 24 Emperors in succession order, the eight other notable figures, and
Jesus of Nazareth per Religions' own careful-treatment note) and, across §2-§5's four era tables, all 84
of the source doc's own dated rows except 146 BC's Carthage entry — 85 authored `HistoricalTimelineEntryDefinition`s
once the two multi-`eventType` rows (AD 64, AD 79) each split into two, per this pass's own several
disclosed authoring calls applied consistently rather than ad hoc: 146 BC's Carthage row is not registered
at all, since the source doc's own §2 heading frames it explicitly as predating this game's own range, not
a real dated entry within it; a row spanning years uses its own start year as `Date` per this item's own
construction-order instruction, except the Numantine War (143-133 BC) alone, whose literal start year
falls before the campaign range's own 133 BC floor — that one entry uses its real ending year (133 BC,
itself the range's own opening year) instead, the sole deliberate exception, disclosed in
`KnownWorldHistoricalTimeline`'s own doc comment rather than silently applied; a row naming more than one
`eventType` (AD 64's Great Fire/persecution/Domus Aurea, AD 79's succession/eruption) splits into two
entries sharing a date and name prefix, each carrying its own single best-fit type, rather than folding
multiple types into one entry or picking only one; and `InvolvedFigureIds` only names a figure §6's own
roster actually registers — several event-table rows name a historical actor (Cicero, Cato, Spartacus,
the Philippi triumvirs) §6 itself never promotes to a `NamedHistoricalFigureDefinition`, and those rows
carry an empty figure list rather than a fabricated reference. A handful of well-documented Republic-era
death years (Marius, Sulla, Pompey, Crassus, Vercingetorix, Lucius Verus) that §6's own tables never state
outright are filled in as standard, uncontroversial Roman history rather than left null, disclosed the
same way item 4 disclosed its own Germanic-language and Oscan/Noric gap-fills. `gens-events-historical-
timeline-late-antiquity-content.md`'s own AD 235 – AD 565 extension is explicitly out of this item's
scope, per the roadmap's own Phase 13 "Primary design inputs" line naming only the 133 BC – AD 235 doc —
mirroring how item 1 named the actual region-content waves out of its own scope and item 6 as the future
item that expands it. `SampleHistoricalTimelineDefinitions` keeps the real content and the test fixture
cleanly separate, `sample-*`-ID per `SampleEventDefinitions`'s own precedent, exercising every field
including a `DivergenceEligible` entry linked to a real sample `EventDefinition`, a multi-figure entry,
and entries sitting at both range boundaries.

The runtime layer mirrors `Letter`/`CorrespondenceTransitSystem` and `TravelTrip`/`TravelProgressSystem`'s
own "one real advancing `WorldState` registry plus a derived-not-stored status" split precisely.
`DivergenceRecord` (a genuine `RuntimeId`-keyed `WorldState` partition, `TriggeringHouseholdId` typed as
the real `RuntimeId<Household>` rather than a loose string) is fully wired through `WorldSaveDto`/
`WorldStateMapper`/`StateHasher`/`EntityKinds.cs` exactly like `Letter`/`TravelTrip` before it — a
recorded Divergence is genuine campaign history, not content. It deliberately omits §10's own
`chronicleEntryTier` field: §6.7 fixes it at "always maximum tier" with nothing left to actually store, so
`ChronicleProjector` (Phase 11 item 3) gains a real new case emitting a `ChronicleTier.Legendary` entry
directly off `DivergenceRecordedEvent`, closing the loop into the real Dynasty Chronicle rather than
leaving §6.7's "every Divergence is an automatic maximum-tier Chronicle entry" commitment unwired.
`TriggeringAction` stays a plain, caller-supplied human-readable string rather than something computed
from a real severity-threshold system — this item builds no such system, matching §11's own open
"Divergence's exact severity threshold" question and item 3's own precedent for leaving forgery-detection
mechanics unresolved rather than fabricated. `RecordDivergenceCommand`'s `CommandPipeline` enforces
"immutable history" from the branching direction: every affected entry must resolve in the catalog, must
be `DivergenceEligible`, must have a real date at or after `WorldState.Date` (an already-passed real date
can never be retroactively branched — five new `ValidationErrorCode`s cover each rejection shape plus the
"already covered by an earlier Divergence" case), following `SendLetterCommand`/`BeginTravelCommand`'s
identical pipeline shape. `HistoricalTimelineQueries.DivergenceStateOf`/`NamedHistoricalFigureQueries.
CurrentStatusOf` are the counterfactual-flag half: pure queries deriving all four/three states against
the one real `DivergenceRecords` list, never stored, matching how `Character.CurrentTravelLocation` favors
deriving over storing wherever the source fact is already tracked elsewhere.

`HistoricalTimelineScheduler` (`IMonthlySystem<WorldState>`, registered in `TickPhase.Events` alongside
`EventPoolSystem`) is the item's own namesake mechanism: each tick, fires every catalog entry whose real
`Date` matches `WorldState.Date` exactly and whose derived state is `OnTrack`, reusing
`FireEventCommands.BuildPipeline` for a linked entry exactly the way `EventPoolSystem` itself fires a
definition, or emitting a new lightweight `HistoricalTimelineEntryOccurredEvent` digest when unlinked. An
already-`Diverged` entry never fires — "immutable history" from the other direction, since once Diverged a
thread genuinely stops drawing on the real historical roster (§6.7). This codebase's `GameCalendar` (§10's
own per-household starting-year/current-year/era sketch) stays explicitly out of this item's scope, same
as the architecture directive requires — that's Start Mode/Core's own job, and this single-household
simulation already treats `WorldState.Date` as *the* one campaign clock everywhere else, so the scheduler
does too rather than inventing an unused parallel calendar; `RivalHouseHistoricalFlavor` (§10's own
`rivalHouseId`/`seededGensName` pair) is likewise untouched — pure flavor-naming for Rival Houses (§6.6),
unrelated to the scheduler/divergence/validation mechanism this item actually builds. Which entries have
already fired is real state (`WorldState.FiredHistoricalTimelineEntryIds`), keyed by the content entry's
own string ID rather than a `RuntimeId` — `DefinitionId<T>` itself implements no `IComparable<T>` for
`OrderedRegistry`'s own ordering guarantee to sort on — so a save/load round trip never re-fires an
already-resolved entry. Covered by 55 new tests across
`tests/Gens.Simulation.Tests/History/{HistoricalYearTests,HistoricalTimelineDefinitionTests,
HistoricalTimelineCatalogTests,HistoricalTimelineQueriesTests,RecordDivergenceCommandTests,
HistoricalTimelineSchedulerTests}.cs`, including the real authored catalog building cleanly with every
cross-reference resolving, the scheduler firing an on-track entry exactly once and never re-firing either
an already-fired or an already-diverged one (both directly and through a save/load round trip with a
stable deterministic state hash), firing a linked entry through the real Events pipeline, every
`RecordDivergenceCommand` validation failure shape and its success path, and both derived-state queries
across all their real states.

**Item 6 progress:** the first real, authored region-content wave lands in `KnownWorldRegions.cs`,
alongside item 1's fixture `SampleRegionProfileDefinitions` in the same `src/Gens.Simulation/Regions/`
namespace — authoring **Latium** in full against `gens-starting-regions-italian-heartland-design.md`
§3, the item this roadmap's own item 1 explicitly deferred ("item 6's actual authored region content is
explicitly out of this item's scope"). Latium is this wave's own deliberate starting point: the launch
roster's most central region per that document's §5 (Rome's immediate political backyard), and the
simpler of the Italian Heartland's own split pair — no Reputation Duality and no dated rule override at
any date (§2's "Shared Italian Identity"), which keeps this first wave a clean, uncomplicated proof that
the schema holds up for real authored content and not just a fixture. Campania and the rest of the launch
roster (§5.1) are a deliberate future wave, matching this same construction-order item's own "and only
then expand region content in waves" framing — this item does not attempt them.

Every §3 subsection maps directly onto `RegionProfileDefinition`'s existing fields: §3.1's terrain
(river-plain fertility, no mineral deposits, the Via Salaria salt-pan tradition), §3.2's economic
character (most expensive land on the roster, thin room to expand, grain-import dependency), §3.3's
political/legal texture (maximum Curia contest, fastest cursus honorum access, an almost entirely
citizen legal-status mix), §3.4's diplomatic/military exposure (no Frontier neighbor, patronage-based
officer recruitment, urban-cohort security), §3.5's religious/cultural default (Mos Maiorum, residual
Etruscan haruspicy influence), and §3.6's regional goods (wine, olive oil, salt, and *peperino* building
stone) each become one qualitative ref/tag string, exactly as item 1's own schema intends — no numeric
sizing invented here either, matching every prior item's own standing convention. §3.7's Population &
Culture Distribution becomes a two-row `CultureDistributionTable` resolving against the real
`KnownWorldCultures` catalog item 4 built rather than loose strings — Roman/Latin dominant (weight 95)
and the one required outlier-residual row standing in for §3.7's own "rare, individual-level outliers
only" close (weight 5) — the first region content to actually consume that catalog by reference instead
of a fixture's own invented placeholder tags. Etruscan presence deliberately gets no row here, per an
automated review finding this pass accepted: §3.7 itself frames it as "residual, religious-influence
only" and "cultural rather than demographic," which a weighted demographic-generation table would
contradict — that residue stays exactly where §3.5 already puts it, in `religiousCulturalDefaultRef`.

§3.8's Gazetteer authors all eight of that section's real locations (Ostia, Tusculum, Praeneste, Tibur,
Antium, Alba Longa, Lavinium, Gabii) with their real Roles, Prominence Tiers, and grounding notes taken
directly from the design document's own table, plus each location's real §3.9 Rival Seeding house
carried as its own `RivalSeatHouseId` free-form tag (Gens Fabricia at Rome, Gens Octavinia at Tusculum,
Gens Sergiana at Praeneste, Gens Considia at Gabii) — §3.9's own house identities are real content, even
though no typed Rival House schema exists yet to own them structurally, matching
`GazetteerLocationDefinition.RivalSeatHouseId`'s own "item 6/9 territory, not this schema's" doc comment.
Rome itself (§5) is this item's own one disclosed authoring call: the design document frames Rome as
belonging to neither Latium nor Campania exclusively, but the schema requires every Gazetteer entry to
declare one owning region, and only Latium exists yet in this wave — so Rome is seated as a Latium
Gazetteer entry, carrying the catalog-unique `GazetteerRole.Capital` role `RegionProfileCatalog`'s own
constructor-time check enforces, on the historically accurate reading that Rome sits geographically
within Latium proper. A future Campania wave does not re-seat Rome; it only gains the shorter Distance
Tier relationship §6 of that document describes — Distance Tiers themselves stay this item's own
deferred territory, since `DistanceTierCatalog` (Travel, item 2) already owns that lookup mechanism and
authoring its real region-pair contents was explicitly left open by that item too. §3.10's Home Anchor
(Tusculum) is the `homeAnchorLocationId` the schema's own constructor-time check validates resolves to a
real Gazetteer entry. §3.11's Templated Background flavor, §6's Distance Tiers, and §7's Historical
Timeline Hooks all stay out of this item's own scope, same as no typed schema field exists yet for any
of the three — this item authors only what item 1's schema actually has fields for. Covered by 8 new
tests in `tests/Gens.Simulation.Tests/Regions/KnownWorldRegionsTests.cs`, proving the real catalog builds
cleanly, the Capital role resolves uniquely to Rome, Reputation Duality reads `None` at every date, the
Home Anchor resolves to Tusculum, the full Gazetteer roster is present, and the culture distribution
table's Roman entry outweighs every other row while carrying exactly one outlier-residual entry.

**Item 7 progress:** Distant Holdings land in `src/Gens.Simulation/Land/` (`DistantHolding.cs`,
`DistantHoldingCommands.cs`, `DistantHoldingMismanagementRiskSystem.cs`), closing §7.2/§12's own
`DistantHolding{}` sketch against a real `WorldState` partition, fully wired through `WorldSaveDto`/
`WorldStateMapper`/`StateHasher`/`EntityKinds` exactly like `TravelTrip`/`Letter` before it — a holding's
own Procurator staffing and mismanagement-risk state is genuine campaign state, not content. No new
Distance Tier mechanism is built: `AcquireDistantHoldingCommand` reuses `DistanceTierCatalog` (Travel,
item 2) exactly as item 2 itself left it — the general lookup mechanism, still with no real region-pair
contents authored, per §13's own still-open "Distance Tier lookup table contents" question. §5.3's
"evaluated exactly like any other Senior Position" is taken literally: `AppointProcuratorCommand` doesn't
invent a parallel appointment path, it drives the existing `StewardshipContext.SecondSettlementProcurator`
`StewardshipAssignment` (reserved but unused since Phase 10 item 2) through `StewardshipCommands.
AppointPipeline` directly, then folds that assignment's outcome back onto the `DistantHolding` record —
so a Procurator appointment competes for a household's one-active-assignment slot exactly the way
`Succession.RegencySystem`'s own supersede logic already assumed it would. §7.2's actual mismanagement
rule — "a Far holding without a competent, high-loyalty Procurator... a real, ongoing risk" — is built as
a deterministic flag, not a random incident roll: `DistantHoldingMismanagementRiskSystem` runs monthly,
reusing `StewardIncidentCatalog.LoyaltyRiskThreshold` (Phase 10 item 2's own Loyalty-risk figure) rather
than inventing a second one, and reverts a holding to unstaffed whenever its cached Procurator's backing
assignment lapses (death, or a graver Regency superseding it) instead of keeping a stale pointer. What
actually happens while the risk flag is active — skimming, drift, an eventual disloyal-Procurator
incident — stays deliberately unbuilt: §11's own open questions ("Disloyal Procurator/Senior Position
consequences," "Procurator autonomy boundary") are unresolved in the design corpus, so this item only
surfaces the risk state honestly rather than fabricating an incident mechanic ahead of its own sizing,
mirroring how Correspondence (item 3) left forgery detection and Regency (Phase 11 item 2) left "no
eligible candidate" as named, not hidden, gaps. Land acquisition cost premiums and Travel time/risk
scaling by Distance Tier (§7.2's other two cost vectors) are out of this item's scope for the same reason
every prior item leaves numeric sizing to Start Modes/a future balancing pass (§13's own "all numeric
sizing" open question) — this item builds the administrative-overhead vector §7.2 actually specifies a
concrete rule for, not the other two's still-unsized multipliers. Covered by 13 new tests in
`tests/Gens.Simulation.Tests/Land/DistantHoldingTests.cs`, including a save/load round trip with a stable
deterministic state hash.

### Phase 14 — Add health, disease, disasters, and mobile populations — 🔶 IN PROGRESS (item 1 of 5)

**Outcome:** environmental and biological pressure matters without becoming arbitrary save destruction.

Construction order:

1. [x] Extend health with conditions, exposure, resistance/immunity, treatment, recovery, mortality attribution, and care capacity.
2. Implement sanitation, food/water quality, crowding, livestock disease, endemic pressure, outbreaks, and quarantine.
3. Implement environmental hazard profiles, forecast/knowledge, disaster instances, damage, displacement, recovery, and region/date modifiers.
4. Implement wandering population cohorts, routes, needs, fame/visibility, settlement interaction, recruitment, and promotion to named characters.
5. Integrate hazards with goods, buildings, populations, markets, travel, events, institutions, and reports through shared events and effects.

**Exit gate:** hazards have visible causes, warnings where appropriate, bounded losses, recovery paths, and deterministic fixtures; they do not bypass ownership, ledger, health, or event rules.

**Primary design inputs:** `gens-disease-public-health-design.md`, `gens-natural-disasters-design.md`, `gens-wandering-populations-design.md`.

**Item 1 progress:** the generic, disease-agnostic Health substrate lands in a new
`src/Gens.Simulation/Health/` namespace, deliberately scoped narrower than
`gens-disease-public-health-design.md` as a whole — §2's seven named endemic diseases, §3's four named
epidemics, their terrain/sanitation/crowding-driven Exposure drivers, contagion spread, quarantine
(§4), Sanitation Investment (§6), and livestock/zoonotic crossover (§8) are all explicitly item 2's
"sanitation... endemic pressure, outbreaks, quarantine" territory, matching the design doc's own
"representative rather than exhaustive" framing and this roadmap's own item-by-item split. This item
instead builds the reusable machinery every one of those future named diseases will plug into:
`HealthConditionDefinition`/`HealthConditionCatalog` (content, empty until item 2 authors real
content) mirror `Cultures.CultureDefinition`/`CultureCatalog`'s identical "sealed record,
constructor-validates, duplicate-ID-rejecting catalog" shape exactly. `CharacterHealthCondition` is
the new `RuntimeId`-keyed `WorldState` partition (`CharacterHealthConditions`, wired through
`StateHasher`/`WorldSaveDto`/`WorldStateMapper` the same five-file way every prior runtime partition
has been) — one entry per standing case, snapshotting its `HealthConditionDefinition`'s Category and
HasCure at onset so no system needs catalog access at tick time, the same reasoning `PermanentInjury`
already established for never re-resolving content. Deliberately not named `HealthCondition`: that
identifier is already `Characters.Condition`'s own five-stat Health/Fatigue/Loyalty/Ambition/Fertility
block (`gens-familia-design.md` §2.3), a wholly different concept. §5's Immunity is not a separate
record: a case that resolves `Recovered` with `GrantedImmunity` set (true only for an
`Acute`/epidemic-layer case, never a `Chronic`/endemic one, per §5's own "survives an Epidemic"
framing) is simply kept in the registry forever, matching `EventInstances`'s "resolved or not, kept
for the campaign's lifetime" convention — `HealthQueries.IsImmune`/`HasActiveCondition`/
`ActiveConditionsFor` are linear scans over it, the same shape `Languages.LanguageProficiencyQueries`
already established for an equivalent "a Character legitimately holds several entries at once"
collection. `AfflictCharacterCommand` is the entry point future callers (item 2's contagion rolls,
item 3's disaster-aftermath flares, Natural Disasters' Flood/Famine triggers) will use once they
exist — an explicit "hook" needing no caller wired up in this item, the same discipline
`ApplyPermanentInjuryCommand` already used; it rejects a duplicate active case of the same condition
and, mechanically realizing §5's whole payoff, rejects afflicting an already-immune Character outright.

Treatment, recovery, mortality attribution, and care capacity are real, tested mechanisms, not just
scaffolding. `HealthConditionProgressionCalculator` (drain/recovery/fatality/severity-drift) and
`CareCapacityCalculator` (a Physician's bounded monthly caseload from `LaborSkills.Medicine` via the
existing `DutySlot.Physician` duty slot) are pure, RNG-free functions extending
`Characters.MortalityCalculator`'s own "documented as invented, pending playtesting" precedent — no
numeric exposure/immunity/treatment/recovery curve exists anywhere in the design corpus
(§12's own "All numeric sizing" open question), so every constant is this implementation's own
invented figure, disclosed in each calculator's doc comment, chosen only so that an Acute
(epidemic-layer) case drains Health and risks death faster than a Chronic (endemic-layer) one, both
resolve faster than they fester, Physician treatment measurably improves every one of those odds, and
an incurable condition (`HasCure` false) resists treatment far more than a curable one — §2's Roman
Fever/Consumption "no real cure — only managed severity" framing made literal. `CharacterHealthConditionSystem`
(`TickPhase.Hazards` — this same roadmap wave's own phase) is the monthly tick: it resolves each
afflicted Household's bounded care-capacity allocation (earliest-onset cases treated first when
capacity falls short), applies drain, rolls recovery or fatality, and on a fatal roll kills the
Character through Familia's existing unrestricted death mechanism (§10) — closing marriages the exact
way `CharacterLifecycleSystem` already does for an old-age death. Mortality attribution is this item's
own closed gap: `DeathRecord` gains an additive, nullable `ConditionId` field (no migration needed,
ADR 0011's pre-v1 additive policy) so a Disease death can finally name *which* condition caused it,
where previously `DeathCause.Disease` existed only as an unattributed enum value populated by
`CharacterLifecycleSystem`'s own coarse Infant-stage heuristic — that older heuristic is untouched;
this item only adds the richer attribution path future disease content will actually use. Covered by
39 new tests across `tests/Gens.Simulation.Tests/Health/{HealthConditionCatalogTests,
HealthConditionProgressionCalculatorTests,CareCapacityCalculatorTests,AfflictCharacterCommandTests,
HealthQueriesTests,CharacterHealthConditionSystemTests,HealthSaveRoundTripTests}.cs`, including a
save/load round trip with a stable deterministic state hash for the new `CharacterHealthConditions`
`WorldState` partition and the `DeathRecord.ConditionId` addition together. Deliberately out of scope,
named for item 2: the seven endemic/four epidemic named diseases themselves, their terrain/sanitation/
crowding Exposure drivers, contagion spread, quarantine, Sanitation Investment, and livestock disease;
named for items 3/5: Natural Disasters' hazard profiles and the Antonine Plague's own Event Chain and
cross-system wiring into goods/buildings/markets/travel/events/reports.

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

