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

**This has since changed substantially.** Phases 0–10 (see "Detailed roadmap" below, and the checklist immediately following this paragraph) have since been implemented and merged: `WorldState`, typed IDs, epoch-aware `GameDate`, phased ticks, command/event envelopes, RNG stream registry, canonical save serialization with migrations, typed content-definition families (goods, buildings, traits, policies, events, regions, cultures, religions, names, presentation), a headless campaign bootstrap and console runner, `Character`/Familia lifecycle, region/settlement/plot/holding, stockpiles, buildings, villas, labor, and a production network with ledger-ready event emission, background population groups and employment, household ledgers/markets/debt/contracts, the action/standing-policy layer, the weighted event pool and monthly report projection, the Unity application shell and adapters, the persistent ink bar and four first-class screens, wax-seal/ordinary confirmations, pause/advance/save/load/replay diagnostics, placeholder portraits, the Phase 9 EditMode/PlayMode presentation-layer test suites (including the 24-month exit-gate soak test), and Phase 10's `LivingWorldActor` Background/Noteworthy tiers, rival-house lifecycle, Ancestral Grudges, the shared `ActionSelector`, steward/Council autonomy with real competence/loyalty rolls and Return Reports, the Scheme engine, `RivalDossier` refresh/staleness, and a combined 200-year rival-house/stewardship soak test. **The vertical-slice acceptance test's engineering scaffolding is now in place end to end, and the world can act without waiting on the player; Phase 11 onward (dynasty continuity, institutions, geography/travel, and beyond) remains unbuilt.** Treat the assessment table below as the state at the original audit point, not the current state — see "Detailed roadmap" for what has been completed since.

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
- [ ] **Phase 11** — Guarantee dynasty continuity and historical memory ← **next up**
- [ ] **Phase 12** — Build institutions, reputation, law, religion, and public life
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

### Phase 11 — Guarantee dynasty continuity and historical memory — ⬜ NOT STARTED

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
including a save/load round trip and the deterministic state hash. Items 3–6 (Dynasty Chronicle,
funerals/mourning/memoria, epithets/titles, succession fixtures) remain.

Construction order:

1. Implement heirs, eligibility, designation, adoption, wills/inheritance rules, disputed succession, asset and obligation transfer, and household extinction.
2. Implement the player-character handoff while preserving the household/world distinction.
3. Implement Chronicle entries from domain events, significance tiers, chapters, filters, pins, annotations, and rival entries.
4. Implement funerals, mourning, memoria, ancestor records, and memorial/legacy hooks.
5. Implement epithet/nickname/title awards from rules and provenance rather than free text.
6. Add succession fixtures for ordinary inheritance, contested inheritance, adoption, debt inheritance, absent heirs, and extinction.

**Exit gate:** the vertical-slice campaign can survive at least three successions, including a contested case, while ledgers, property, relationships, history, and saves remain consistent.

**Primary design inputs:** `gens-succession-dynasty-design.md`, `gens-dynasty-chronicle-design.md`, `gens-ancestor-veneration-funerary-customs-design.md`, `gens-epithets-nicknames-titles-design.md`.

### Phase 12 — Build institutions, reputation, law, religion, and public life — ⬜ NOT STARTED

**Outcome:** household choices operate inside a social and political order.

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

Background population, market clearing, the Unity vertical slice, the rest of Phases 7–9, and Phase 10's delegation/autonomous-action/rival-houses work have also since been implemented (see the phase checklist above). **The next unimplemented work is Phase 11** — dynasty continuity and historical memory.

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

**The next milestone is Phase 11 — dynasty continuity and historical memory.** Death, succession, and the Dynasty Chronicle can now be constructed as extensions of the same shared contracts (commands, events, ledgers, read models, knowledge/visibility) every prior milestone has used, including the actor/genealogy machinery Phase 10 just added. That is the safest route to the unusually deep game described by the design corpus without sacrificing determinism, historical breadth, or future AI-assisted presentation.

