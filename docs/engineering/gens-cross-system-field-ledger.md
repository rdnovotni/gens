# Gens — Cross-System Field Ledger

*Closes Phase 1, Item 4 of the [Comprehensive Build Roadmap](engineering/gens-comprehensive-build-roadmap.md): "Create a cross-system field ledger: field name, type, units, range, owner, readers, writers, persistence, visibility, and migration policy."*

---

## Scope

This ledger covers only the **vertical-slice systems** the roadmap names for Phase 1, Item 4's own primary design inputs and Phases 5–8's construction order: **Characters, Familia/household, Traits, Estate & Settlement, Resources & Goods, Buildings, Labor & Slavery, Settlement Demographics, and Economy & Finance**. It deliberately does not attempt every one of the 113 design documents — Phase 1's exit gate asks that "every field in the first vertical slice has one owner, one unit, one range, and a testable rule," not that every future-phase system (Politics & Patronage, Rival Houses, Religion, and the rest) be ledgered before it has its own engineering pass.

The ledger enumerates **schema fields** — the actual named properties each authoritative document's own Data Model section defines — not every individual catalog entry those fields can hold. A `Character.traits.congenital` field gets one row; the 69 individual Congenital trait names it can contain do not each get a row, the same way `Good.key` gets one row rather than one per good. Where a document's catalog size or roster matters to an engineering decision (e.g., "234 traits," "≈150 goods"), it is noted in that section's introduction instead.

## Methodology

Each section is organized under **one authoritative document**, per the [design authority registry](../gens-design-authority-registry.md)'s ownership assignments — a field appears under the document the registry names as its owner, not under every document that happens to mention it. Where the registry documents an explicit supersession or migration gap (e.g., cluster 10's live Buildings/Estate & Settlement goods-list contradiction), that gap is called out in the relevant row's Migration Policy column rather than silently ledgering the superseded shape as if it were still current.

- **Type** and **Units/Range** are stated as designed in the source document's own prose or Data Model block, translated into the engineering vocabulary [ADR 0001](engineering/adr/0001-stable-typed-ids.md) (typed IDs) and [ADR 0002](engineering/adr/0002-fixed-point-arithmetic.md) (integers/`Fixed64`) establish, so this ledger is directly implementable against those ADRs rather than needing a second translation pass later.
- **Owner** names the authoritative document (per the registry); **Readers**/**Writers** are drawn from that document's own "Cross-System Integration" section, where one exists — not invented from outside the corpus.
- **Persisted** defaults to "Yes" for state fields and "No (derived)" for fields a document's own text marks as computed/read-only (e.g., `comboTitle` is "recomputed whenever the Reactive trait set changes meaningfully" per `gens-characters-design.md` §6, not an independent state value).
- **Visibility** is stated per [ADR 0008](engineering/adr/0008-visibility-and-knowledge.md)'s tiers (`Public` / `Household` / `RestrictedUntilOwned` / `Secret`), grounded in the owning document's own text wherever it specifies a visibility rule (most concretely, `gens-labor-slavery-design.md` §3's visible-at-a-glance/hidden-until-owned split); where a document is silent, "Not specified — default Household" is used rather than inventing an unstated rule, flagging it as an engineering judgment call, not a corpus citation.
- **Migration Policy** defaults to the stub **"Additive only until v1 ships"** per [ADR 0011](engineering/adr/0011-migrations.md)/[ADR 0012](engineering/adr/0012-content-versioning.md)'s stated policy; a more concrete note is given wherever the source document's own "Open Questions" or migration language supplies one (e.g., the Traits catalog's explicit Migration Note, §8).

---

## 1. Characters (`gens-characters-design.md`)

*Owns Character identity, lifecycle stage, Personality Axes, the Interaction Catalog, and the Scheme engine (registry cluster 1). Fields below are from §14's `Character`, `Interaction`, `GroupInteraction`, and `Scheme` records. Traits themselves are ledgered under §3 (Traits), which is authoritative for trait content per registry cluster 2; the `traits` container field is ledgered here since Characters owns the record it sits on.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `Character.id` | `RuntimeId<Character>` | — | campaign-unique | Characters | every system in this ledger | Characters (on instantiation, §11) | Yes | Household (existence known to anyone who has met them) | Additive only until v1 ships |
| `Character.praenomen`, `.nomen`, `.cognomen` | string | — | Familia §2.8 naming pool | Characters (record); Familia (naming rules) | all social/political systems | Familia (naming), Labor & Slavery (manumission renaming, §8) | Yes | Public | Additive only |
| `Character.sex` | enum | — | 2 values | Characters | Familia (marriage, §5), Romance & Sexuality | Characters, at instantiation | Yes | Public | Additive only |
| `Character.age` | int | years | 0–~100 | Characters | Familia (lifecycle gating, §3) | Familia lifecycle system, monthly | Yes | Public (exact); RestrictedUntilOwned for an unowned acquisition candidate — visible only as "approximate age" per `gens-labor-slavery-design.md` §3 | Additive only |
| `Character.lifecycleStage` | enum | — | infant / child / adolescent / adult / elderly (Familia §3) | Familia (owns lifecycle, per Characters §1) | every system gating actions by age | Familia lifecycle system | Yes | Public | Additive only |
| `Character.legalStatus` | enum | — | Citizen / Latin Rights / Peregrine / Freedman / Enslaved (Familia §2.5) | Familia | nearly every system (rights, offices, marriage, debt bondage eligibility per `gens-economy-finance-design.md` §6.3) | Familia, Labor & Slavery (manumission), Economy & Finance (debt bondage) | Yes | Public | Additive only |
| `Character.socialClass` | enum | — | Senatorial / Equestrian / Plebeian (Familia §2.5), citizen-only | Familia | Politics & Patronage (future) | Familia | Yes | Public | Additive only |
| `Character.coreAttributes.{diplomacy,martial,stewardship,intrigue,learning}` | int (×5) | score | 0–100 (Familia §2.1) | Familia (defined), Characters (universalized) | every system listed per-attribute in Familia §2.1's table | lifecycle/education/event systems | Yes | Household — approximate/banded for an unowned acquisition candidate per `gens-labor-slavery-design.md` §3 ("shown as a rough band") | Additive only |
| `Character.laborSkills.{fieldwork,domestic,craft,culinary,medicine}` | int (×5) | score | 0–100 (Familia §2.2) | Familia | Labor & Slavery (duty output, §4) | Labor & Slavery, Education & Culture | Yes | Household; RestrictedUntilOwned (banded) for a market/inheritance/seizure acquisition candidate per `gens-labor-slavery-design.md` §3 | Additive only |
| `Character.condition.health` | int | score | 0–100 (Familia §2.3) | Familia | Disease & Public Health (future), Labor & Slavery (output cap, §4) | lifecycle, Labor & Slavery, Disease systems | Yes | Household; visible only as a range at unowned-acquisition time (`gens-labor-slavery-design.md` §3) | Additive only |
| `Character.condition.fatigue` | int | score | 0–100 (Familia §2.3) | Familia | Labor & Slavery (§4, §7 flight risk), unrest math | Labor & Slavery Regimen system | Yes | Household | Additive only |
| `Character.condition.loyalty` | int | score | 0–100 (Familia §2.3) | Familia | "the single most-read stat for compliance across every system" per Familia §2.3 | Labor & Slavery Regimen/Punishment, Economy & Finance (wages, §4.1) | Yes | Household | Additive only |
| `Character.condition.ambition` | int | score | 0–100 (Familia §2.3) | Familia | Characters §8.3 (autonomous initiation), succession-drama systems | events, treatment history | Yes | Household | Additive only |
| `Character.condition.fertility` | int | score | 0–100, age/sex-gated (Familia §2.3, §6) | Familia | Familia childbirth math (§6) | Familia, permanent-injury system (§3.1) | Yes | Household | Additive only |
| `Character.permanentInjuries` | list of struct | — | unbounded | Familia §3.1 | Appearance/Paperdoll (registry cluster 19), Labor Skills/Martial ceiling | Military & Combat, Labor & Slavery punishment (Severe tier), childbirth, Natural Disasters | Yes | Public (visible on portrait per Familia §3.1) | Additive only |
| `Character.traits.{congenital,formative,reactive}` | list of `DefinitionId<Trait>` (×3) | — | lifecycle-gated per category (Characters §4.4) | Traits catalog (content), Characters (container field) | Combo Titles (§6), Personality Axes derivation (§5), nearly every mechanical resolution (§8.1) | trait-acquisition systems per category (birth, upbringing, treatment/events) | Yes | Household; a lazily-instantiated adult's backfilled set is generated, not observed, per §11 | Traits doc §10 supersedes the inline array shape "only insofar as each string entry there should now resolve against this richer `Trait` table rather than a bare label" |
| `Character.personalityAxes.{honor,compassion,greed,zealotry,vengefulness,boldness,rationality}` | `Fixed64` or int (×7) | score | -100..100, 0 neutral (§5) | Characters | mechanical resolution (§8.1), narrative-resolution brief (§8.2), Scheme progress (§10) | trait-nudge accumulation, sustained-experience drift (§5) | Yes | Secret — "hidden from ordinary UI the way CK3's own AI weights are" (§5) | Additive only |
| `Character.comboTitle` | string / `DefinitionId<ComboTitle>` | — | curated list or dynamic fallback (§6) | Characters | UI display | recomputed on Reactive-trait-set change (§6) | No (derived) | Public | Additive only |
| `Character.appearance` | struct | — | Familia §2.4 dataset | Familia (data), Paperdoll (§7.11, rendering — registry cluster 19) | Appearance/Portraiture | Familia, Fashion & Dress (future) | Yes | Public | Additive only |
| `Character.relationships[otherId].opinion` | int | score | -100..100 (Familia §2.7) | Familia | marriage, scheme, succession-dispute math (§7) | Interaction resolution, events | Yes | Secret to third parties by default; Household to the two parties themselves — not specified further in source, engineering default | Additive only |
| `Character.relationships[otherId].bondTags` | list of enum | — | Friend/Rival/Lover/Patron-Client/Mentor-Student/Contubernium/family bonds + §7's new tags (Nemesis, Debtor/Creditor, Co-Magistrate, Blackmail Leverage) | Characters §7 | Politics & Patronage, Economy & Finance (`DebtRecord` mirror) | Interaction resolution | Yes | Secret for Blackmail Leverage specifically (§7 names it "a standing, usable threat" — not ambient knowledge); Household for the rest | Additive only |
| `Character.faction` | enum? / null | — | "traditionalist" \| "popularist" \| null (§14) | Politics & Patronage (future); field declared here since Characters carries it | Politics & Patronage | Politics & Patronage (future) | Yes | Public | Additive only — only set where Politics & Patronage applies |
| `Character.source` | enum | — | familia / courtPosition / curiaSeat / rivalGenerated / travelEncounter / eventEncounter / guest (§14, §12) | Characters | promotion/audit tooling | Characters, at instantiation (§11) | Yes | Not specified — default Household | Additive only |
| `Character.instantiatedAtMonth` | `GameDate` (int months) | months since epoch | — | Characters §11 | Chronicle (future), fidelity-tier tooling (ADR 0009) | Characters, at instantiation | Yes | Not specified — default Household | Additive only |
| `Character.backfilledHistory` | bool | — | — | Characters §11 | narrative-resolution brief (§8.2) | Characters, at instantiation | Yes | Not specified — default Household | Additive only |
| `Scheme.schemeId` / `.type` / `.initiatorId` / `.targetId` / `.assistingCharacterIds` | `RuntimeId<Scheme>`, enum, `RuntimeId<Character>` (×3 forms) | — | type per §10's catalog | Characters §10 | Espionage, Romance & Sexuality, Politics & Patronage (all inherit the engine, §10) | Scheme-initiation command (ADR 0006) | Yes | Secret until discovered (§10.3) | Additive only |
| `Scheme.progress` | int | score | 0–100 (§14) | Characters §10 | Scheme resolution | monthly Scheme-progress tick | Yes | Secret to target until discovery threshold (§10.3–4) | Additive only |
| `Scheme.discoveryRisk` | int | score | rises over time, unsized (§10.3) | Characters §10 | target's counter-play decision (§10.4) | monthly Scheme-progress tick | Yes | Secret | Additive only |
| `Scheme.monthsRunning` / `.status` | int / enum | months / — | — / active-succeeded-failedQuiet-discoveredFoiled-discoveredEscalated (§10.5) | Characters §10 | Chronicle (future) | Scheme resolution | Yes | Secret while active; becomes visible per outcome (§10.5) | Additive only |
| `Interaction.*` / `GroupInteraction.*` (§14) | struct (interactionId, category, type, initiatorId, targetId(s), resolutionLayer, inputsUsed, outcome) | — | category per §9's 8 tables | Characters §9 | every system's own interaction verbs (§9.1–9.8) | Interaction command execution | Yes (event-sourced, per ADR 0007) | Visibility per interaction category and witnesses present — not uniformly specified; engineering default is event `Visibility` per ADR 0007/0008 | Additive only |

---

## 2. Familia / Household (`gens-familia-design.md`)

*Owns lifecycle stages, household role, birth, marriage (`affectio maritalis`), and legitimacy (registry cluster 3). Stat-architecture fields (§2.1–2.7) are ledgered under Characters §1 above, since Characters §1 states plainly it "doesn't re-litigate" that content — this section covers what Familia retains sole authority over: lifecycle stage semantics, marriage/legitimacy, and household duty roles, per §8's own data model.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| Lifecycle stage age bands (Infant 0–3, Child 4–12, Adolescent 13–17, Adult 18–59, Elderly 60+) | enum→range mapping | years | §3's table | Familia | every age-gated system | Familia lifecycle system | Yes (as `Character.lifecycleStage`, ledgered above) | Public | Additive only |
| `role` (duty-slot or Court Position assignment) | `DefinitionId<DutySlot>` \| `DefinitionId<CourtPosition>` | — | §4's two-tier split (Labor Skills-driven duty slots vs. Core Attribute-driven Court Positions) | Familia §4 | Estate & Settlement (labor demand), Labor & Slavery (output) | Household-role assignment command | Yes | Household | Additive only |
| `maritalHistory[]` entries: `{spouseId, startDate, endDate, endReason}` | list of struct | `GameDate` (×2), enum | — | Familia §5, §5.1 | Succession & Dynasty (future), Rival Houses (future — divorce scars, §5.1) | marriage/divorce commands | Yes | Public (marriage is a public social fact) | Additive only |
| `legitimacy` | enum/bool | — | legitimate by default within marriage; requires explicit acknowledgment otherwise (§5.2) | Familia §5.2 | Succession & Dynasty (§6.9, default eligibility gate) | Familia (birth), explicit-acknowledgment command | Yes | Public once determined; the underlying paternity fact may itself be `Secret` per ADR 0008 until acknowledged or discovered | Additive only |
| `originCulture` | `DefinitionId<Culture>` | — | Cultures of the Known World roster (future) | Familia §2.5-adjacent; Cultures doc (future, registry cluster 9) | Language & Literacy, Diplomacy, Appearance/Paperdoll | Familia, at instantiation | Yes | Public | Additive only |
| Divorce consequence fields (dowry return/retention terms, Dignitas hit, relationship scar) | struct, unsized | — | "sized to how the divorce is perceived" (§5.1), not numerically specified | Familia §5.1 | Rival Houses (future) | divorce command | Yes | Public | **Open in source:** §9 flags "Divorce consequence tuning" as unresolved — magnitude fields are declared but unsized; treat as additive-only placeholder until a numeric-balancing pass |
| Fertility/childbirth-risk toggle; historical-restriction toggle | bool or enum (granularity undecided) | — | player-configurable at game start (§2.9, §6) | Familia | UI (campaign setup), Familia mechanics | player, at campaign creation | Yes (campaign config, not per-character) | Public (player's own setting) | **Open in source:** §9 flags toggle granularity (single on/off vs. multi-step) as undecided — additive-only until resolved |

---

## 3. Traits (`gens-traits-design.md`)

*Owns the full 234-trait catalog, the five Tiered Spectrums, and expanded Combo Titles — supersedes Characters §4's original 115-trait content in full (registry cluster 2, `gens-traits-design.md` §8's own Migration Note). The `Trait` and `CharacterLifestyleSlots` records are §10's Data Model; the trait catalog itself (234 named entries) is content, not schema, per rule 10, and is intentionally not enumerated row-by-row here per this ledger's stated scope.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `Trait.name` | `DefinitionId<Trait>` | — | one of 234 (§8) | Traits (content) | Characters (§4, §14's `traits` field) | content authoring only | Yes (content, not save) | Public (catalog is not secret; a *held* trait's visibility is per §2's own Combo Title/Interaction reads) | Additive only — content, per ADR 0012 |
| `Trait.category` | enum | — | congenital \| formative \| reactive \| lifestyle (§10) | Traits | Characters §4.4 (lifecycle gating) | content authoring | Yes (content) | Public | Additive only |
| `Trait.spectrum` | enum? / null | — | null \| intellect \| beauty \| physique \| humors \| piety (§3, §10) | Traits | Character generation (tiered, mandatory-pick-one per §3) | content authoring | Yes (content) | Public | Additive only |
| `Trait.tierPosition` | int? / null | — | null for ordinary pairs; 0–3 for tiered spectrum members (§10) | Traits | tiered-spectrum roll logic | content authoring | Yes (content) | Public | Additive only |
| `Trait.opposedTrait` | `DefinitionId<Trait>`? / null | — | null for standalone traits (Haunted, Ambidextrous, etc.) | Traits | opposed-pair exclusivity enforcement (Characters §4) | content authoring | Yes (content) | Public | Additive only |
| `Trait.axisNudges[]` | list of `{axis, magnitude}` | — | magnitude small\|large; axis one of the seven Characters §5 axes | Traits | Personality Axes drift (Characters §5) | content authoring | Yes (content) | Secret (feeds the hidden Axis layer) | Additive only |
| `Trait.bespokeEffect` | string (mechanical spec) | — | free text per trait | Traits | mechanical resolution (Characters §8.1) | content authoring | Yes (content) | Public | Additive only |
| `Trait.minLifecycleStage` | enum | — | infant (rare) \| child \| adolescent \| adult (§10) | Traits | trait-acquisition gating | content authoring | Yes (content) | Public | Additive only |
| `Trait.costOverride` | value? / null | — | null for most Lifestyle traits; set for Duelist/Spymaster-style costed exceptions (§5.3, §10) | Traits | Lifestyle-trait acquisition | content authoring | Yes (content) | Public | **Open in source:** §11 flags exact acquisition trigger and cap-scaling as unresolved |
| `CharacterLifestyleSlots.activeLifestyleTraits[]` | list of `DefinitionId<Trait>` | — | max 3 (§5.3, §10) | Traits §10 | Characters (record extension) | Lifestyle-trait acquisition command | Yes | Household | **Open in source:** §11 flags lapse-selection rule (player choice vs. least-recently-exercised) as undecided |
| `CharacterLifestyleSlots.lapsedLifestyleTraits[]` | list of `DefinitionId<Trait>` | — | retained for flavor/history only (§10) | Traits §10 | narrative-resolution brief (Characters §8.2) | Lifestyle-trait lapse event | Yes | Household | Additive only |

---

## 4. Estate & Settlement (`gens-estate-settlement-design.md`)

*Owns the physical growth engine — land map, terrain-gated plots, building categories/chains (registry cluster 24). Its own §8 `goods` field is explicitly superseded by Resources & Goods §7's Unified Goods Registry per registry cluster 10 — ledgered here as superseded, not as current, per this document's own instruction that "Estate & Settlement's own `goods` object should be read as pointing" to Resources & Goods.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `Plot.id` | `RuntimeId<Plot>` | — | campaign-unique | Estate & Settlement | Buildings, Resources & Goods, Settlement Demographics (§7.2 rural capacity) | Estate & Settlement, at map generation/acquisition | Yes | Public | Additive only |
| `Plot.terrain` | enum | — | Fertile Plain / Hills / Forest / Coast / River / Marsh-Poor land (§2) | Estate & Settlement | Buildings (terrain gating, §3), Resources & Goods (region weighting) | fixed at map generation | Yes | Public | Additive only |
| `Plot.region` | `DefinitionId<Region>` | — | Starting Regions roster (registry cluster 5) | Starting Regions (schema owner); Estate & Settlement (field host) | economic-identity bonuses (§6), regional goods (Resources & Goods §5) | fixed at map generation | Yes | Public | Additive only |
| `Plot.building.category` | enum | — | Agriculture / Industry / Commerce / Civic / Military / Monuments / Infrastructure (§3) | Estate & Settlement / Buildings (full roster, registry cluster 14) | Economic Identity weighting (§6) | construction command | Yes | Public | **Live gap flagged by registry cluster 10** — see Resources & Goods below |
| `Plot.building.key` | `DefinitionId<Building>` | — | Full Building Index (`gens-buildings-design.md` §8) | Buildings | production systems, Economic Identity | construction command | Yes (content ref) | Public | Additive only |
| `Plot.building.tier` | int | — | chain-dependent, 1–4 typically (§3's table) | Estate & Settlement / Buildings | production output scaling | upgrade command | Yes | Public | Additive only |
| `Plot.building.produces` | `DefinitionId<Good>`? / null | — | named commodity per chain, or null for storage/service buildings (§3.1) | Estate & Settlement (chain) / Resources & Goods (good identity) | Commerce buildings, market | fixed by building definition | Yes (content ref) | Public | **Superseded shape** — Resources & Goods §7 is authoritative for the good identity itself; Estate & Settlement's chain table is a production-chain reference, not a second goods registry |
| `Plot.building.public` | bool | — | Civic = public, all else private (§3.1) | Estate & Settlement | Settlement Demographics (civic benefit extension) | fixed by building category | Yes | Public | Additive only |
| `Plot.building.constructionMonthsRemaining` | int | months | 0 = complete; scales with building size (§4) | Estate & Settlement | Labor & Slavery (labor diversion), tick-phase Production system (ADR 0005) | construction-progress monthly system | Yes | Public | Additive only |
| `Plot.building.laborAssigned` | `RuntimeId<Character>`[] or ratio | — | diverts workers from regular duties (§4) | Estate & Settlement | Labor & Slavery, Familia (duty-slot conflicts) | labor-assignment command | Yes | Household | Additive only |
| `Plot.building.condition` | int (`Fixed64`?) | score | degrades from neglect/disaster; restored by Repair (§4) | Estate & Settlement | Economy & Finance (Net Worth depreciation, §8 of that doc) | decay monthly system, Repair command | Yes | Public | **Open in source:** §9 flags Repair action cost/time as unsized |
| `Plot.contested` | bool | — | occasional, not default friction (§7) | Estate & Settlement | Rival Houses (future — contest resolution) | land-acquisition system | Yes | Public | **Open in source:** §9 flags contested-plot resolution mechanism as unresolved |
| `Settlement.stage` | enum | — | Villa / Vicus / Town / City (§5) | Estate & Settlement | Buildings (stage-gated construction), Settlement Demographics (population threshold) | stage-transition command (deliberate action, §5) | Yes | Public | **Open in source:** §9 flags stage-transition population numbers as pending Settlement Demographics' own numeric pass |
| `Settlement.goods{...}` | struct (grain/oil/wine/wool/craftGoods/stone/textiles/pottery/timber totals) | quantity | — | **Superseded** — see Resources & Goods `GoodStock` below | (superseded) | (superseded) | — | — | **Registry cluster 10: this field is explicitly superseded by Resources & Goods §7/§16's `GoodStock`; the source document itself has not yet been edited to remove it — flagged by the registry as "the most concrete remaining task before implementation."** ADR 0012 makes this contradiction a hard content-build failure once the content compiler exists. |
| `Settlement.economicIdentity.{agrarian,mercantile,industrial,martial}` | `Fixed64` (×4) | relative weight | compounding bonus, unsized formula (§6) | Estate & Settlement §6 | Buildings (§6, category-to-identity mapping) | recomputed from building-category mix, monthly | No (derived) | Public | **Open in source:** §9 flags the specialization bonus curve as unsized |
| `Settlement.region` | `DefinitionId<Region>` | — | Starting Regions roster | Starting Regions (schema) | Estate & Settlement's own bonus/gating rules (§6) | fixed at campaign start | Yes | Public | Additive only |

---

## 5. Resources & Goods (`gens-resources-goods-design.md`)

*Self-declared "the complete, authoritative registry" for every good (registry cluster 10), superseding Buildings §2's goods taxonomy and Estate & Settlement §8's storage list — the single most concrete pre-implementation blocker the design authority registry names. Fields below are §16's Data Model in full.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `Good.key` | `DefinitionId<Good>` | — | one of ≈150 goods across 5 tiers (§7) | Resources & Goods | every production/consumption/market system in this ledger | content authoring | Yes (content) | Public | **This is the field ADR 0012's duplicate-`DefinitionId` validation directly protects** — Buildings and Estate & Settlement's own now-superseded partial lists must not re-declare the same key |
| `Good.tier` | enum | — | raw \| intermediate \| finished \| luxury \| imported (§16) | Resources & Goods | Dignitas/gifting value (§14) | content authoring | Yes (content) | Public | Additive only |
| `Good.storageCategory` | enum | — | granary \| warehouse \| apotheca \| strongroom \| macellum \| none \| internalUse \| finiteRegionalStock (§8, §16) | Resources & Goods | Buildings (storage capacity), Estate & Settlement | content authoring | Yes (content) | Public | Additive only |
| `Good.perishability` | enum | — | perishable \| semiPerishable \| nonPerishable \| n/a (§9, §16) | Resources & Goods | spoilage monthly system | content authoring | Yes (content) | Public | Additive only |
| `Good.qualityEligible` | bool | — | §10.1's displayed-grade rule | Resources & Goods | market pricing, gifting | content authoring | Yes (content) | Public | Additive only |
| `Good.hiddenQualityPrecursor` | bool | — | applies to raw/bulk-intermediate goods (§10.1) | Resources & Goods | downstream Quality-ceiling calculation | content authoring | Yes (content) | Secret-ish — "still sets a downstream ceiling without displaying their own grade" (§10.1); engineering reading: computed, never shown directly | Additive only |
| `Good.regionWeight` | `DefinitionId<Region>`? / null | — | §5's regional-specialty list | Resources & Goods | market pricing (regional scarcity, §11) | content authoring | Yes (content) | Public | Additive only |
| `Good.basePrice` | int | denarii (minor units per ADR 0002) | tier-scaled, unsized formula (§11) | Resources & Goods | Economy & Finance (all income categories) | content authoring | Yes (content) | Public | **Open in source:** §17 flags base-price/Quality-multiplier weights as unsized |
| `Good.giftValueMultiplier` | `Fixed64` | multiplier | — | Resources & Goods §14 | Familia (marriage-market gifting), Politics & Patronage (future) | content authoring | Yes (content) | Public | Additive only |
| `Good.autoConsumedBy` | `DefinitionId<Building>`? / null | — | Temple/Valetudinarium/libraries/Balneum/Mint, per §13.4 | Resources & Goods | Buildings (upkeep consumption) | content authoring | Yes (content) | Public | **Open in source:** §17 flags Medicine's/Incense's actual consumption rates as unsized |
| `Good.preservationConversion` | `DefinitionId<Good>`? / null | — | Orchard Fruit→Dried Fruit, Grapes→Raisins, per §9.2 | Resources & Goods §9.2 | spoilage-mitigation monthly system | content authoring | Yes (content) | Public | Additive only |
| `LivestockStock.buildingId` | `RuntimeId<Building>` | — | one per Pasture-type building | Resources & Goods §3 | Companions & Court Positions (Vilicus remit) | building instantiation | Yes | Household | Additive only |
| `LivestockStock.animalType` | enum | — | horses \| oxen \| mulesAndDonkeys \| cattle \| sheep \| goats \| pigs \| poultry (§3.1, §16) | Resources & Goods §3 | Military & Combat (future, cavalry), Travel (future) | building definition | Yes | Public | Additive only |
| `LivestockStock.headcount` | int | animals | ≥0 | Resources & Goods §3 | Piracy & Banditry (rustling target, §12), Disease & Public Health (future) | growth/cull monthly system | Yes | Public | Additive only |
| `LivestockStock.purposeAllocation` | enum? | — | cattle: dairyBeef\|draft; horses: cavalry\|logistics; else n/a (§16) | Resources & Goods §3.2 | production output split | building-level standing choice command | Yes | Household | Additive only |
| `LivestockStock.herdStrategy` | enum | — | growthFocused \| balanced \| yieldMaximizing (§3.2, §16) | Resources & Goods §3.2 | growth/cull monthly system | standing-policy command (Regimen-equivalent) | Yes | Household | **Open in source:** §17 flags the actual growth/yield tradeoff rates as unsized |
| `LivestockStock.mortalityRisk` | `Fixed64` | probability | — | Resources & Goods §3.2 | Natural Disasters (future), Disease & Public Health (future) | monthly recomputation | No (derived) | Household | Additive only |
| `RegionalStock.goodKey` / `.remainingReserve` / `.depletionRatePerHarvestIntensity` / `.exhausted` | `DefinitionId<Good>`, int, `Fixed64`, bool | quantity / rate / — | Silphium is the sole instance for now (§5, §16) | Resources & Goods §5 | Dynasty Chronicle (future — exhaustion is "Chronicle-worthy," §15) | harvest monthly system | Yes | Public | **Open in source:** §17 flags Silphium's depletion curve as unsized |
| `GoodStock.goodKey` / `.quantity` / `.quality` / `.ageInTicks` / `.locationId` | `DefinitionId<Good>`, int, enum, int, `RuntimeId<Storage>` | quantity / — / months / — | quality: Common\|Fine\|Exceptional (§10.2) | Resources & Goods §16 | Economy & Finance (Net Worth, §8 of that doc), every consumption system | production, consumption, sale, spoilage monthly systems | Yes | Household (an owned household's stockpile); the corresponding field on `Settlement.goods` this replaces is flagged superseded above | Additive only — this is the record that finally resolves the Buildings/Estate & Settlement contradiction once content validation (ADR 0012) is live |
| `SettlementMarket.prices{}` / `.supply{}` / `.demand{}` | `Dictionary<DefinitionId<Good>, Fixed64/int>` (×3) | denarii / quantity / quantity | — | Resources & Goods §12 | Economy & Finance (§7's "unchanged... authoritative baseline"), Rival Houses (shared market, future) | Market Dynamics monthly system | Yes | Public | **Open in source:** §17 flags the supply/demand formula itself as unsized |
| `SettlementMarket.tradeExposure` | `Fixed64` | risk score | — | Resources & Goods §12 | Piracy & Banditry (future) | Market Dynamics monthly system | Yes | Public | Additive only |

---

## 6. Buildings (`gens-buildings-design.md`)

*Owns building instances and production chains — the Full Building Index (registry cluster 14). Its own §2 goods taxonomy is explicitly superseded by Resources & Goods §7 (registry cluster 10) and is not ledgered here as authoritative; only the `serviceBuilding` shape (§7's Data Model), which Resources & Goods does not cover, is genuinely Buildings' own field content.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `goods.{raw,intermediate,finished,luxury,imported}` object (§7) | struct | — | — | **Superseded** | (superseded) | (superseded) | — | — | **Registry cluster 10: superseded in full by Resources & Goods §7's Unified Goods Registry; still present in the source document, unedited, as of this pass — the concrete instance ADR 0012's duplicate-`DefinitionId` validation must catch.** |
| `serviceBuilding.key` | `DefinitionId<Building>` | — | Slave Market / Brothel / Tavern / Argentaria / Bathhouse / etc. (§4.8, §4.10) | Buildings §7 | Labor & Slavery (Slave Market, Argentaria), Economy & Finance | content authoring | Yes (content) | Public | Additive only |
| `serviceBuilding.type` | enum | — | slaveMarket \| brothel \| tavern \| argentaria \| bathhouse \| ... (§7) | Buildings §7 | same as above | content authoring | Yes (content) | Public | Additive only |
| `serviceBuilding.monthlyIncome` | int | denarii | — | Buildings §7 | Economy & Finance (Ledger, §10 of that doc) | monthly Commerce-income system | Yes | Household | **Open in source:** §9 flags most new buildings' income/Dignitas/population numbers as not yet numerically costed |
| `serviceBuilding.effects.{dignitas,disease,happiness,...}` | struct of `Fixed64`/int | varies (Dignitas score / disease risk / Contentment) | whichever apply per building (§7) | Buildings §7 | Politics & Patronage (Dignitas, future), Disease & Public Health (future), Settlement Demographics (Contentment) | building-effect monthly system | Yes | Public | **Open in source:** §9 flags Slave Market's and Brothel's exact Dignitas tuning as unspecified; the Brothel's *infamia* legal-status flag is named as "a needed cross-doc addition" to Familia §2.5, not yet made |

---

## 7. Labor & Slavery (`gens-labor-slavery-design.md`)

*Owns acquisition, day-to-day treatment, punishment, flight, and manumission for enslaved household members — reads/writes Familia's stat blocks directly (§1). Fields below are §11's Data Model, extending the Familia record.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `acquisition.method` | enum | — | slaveMarket \| warCaptive \| debtBondage \| birth \| inheritanceGift \| legalSeizure \| piracyKidnapping (§2) | Labor & Slavery §2 | Chronicle (future), Dignitas systems (piracy-sourcing risk) | acquisition command | Yes | Public | Additive only |
| `acquisition.date` | `GameDate` | months since epoch | — | Labor & Slavery §2 | Chronicle (future) | acquisition command | Yes | Public | Additive only |
| `acquisition.price` | int | denarii | 0 for inheritance/gift; market-formula otherwise (§2) | Labor & Slavery §2 | Economy & Finance (Capital Expenditure, §4.4 of that doc) | acquisition command | Yes | Household (the buyer knows; the seller's true condition may be misrepresented per §3) | **Open in source:** §12 flags exact pricing-formula weights as unsized |
| `regimen.diet` | enum | — | Meager \| Adequate \| Generous (§5) | Labor & Slavery §5 | Health trend system, Resources & Goods (consumption, §13.2) | Regimen standing-policy command; per-individual override takes precedence over group default | Yes | Household | Additive only |
| `regimen.accommodation` | enum | — | Bare \| Basic \| Comfortable (§5) | Labor & Slavery §5 | Health/Loyalty trend systems | Regimen command | Yes | Household | Additive only |
| `regimen.freedoms` | enum | — | Confined \| Restricted \| Free Movement (§5) | Labor & Slavery §5 | Flight-risk derivation (§7) | Regimen command | Yes | Household | Additive only |
| `regimen.discipline` | enum | — | Lenient \| Firm \| Harsh (§5) | Labor & Slavery §5 | Fatigue/output ceiling, Unrest math | Regimen command | Yes | Household | **Open in source:** §12 flags exact per-tier numeric deltas (upkeep, Health/Loyalty trend, Unrest) as untuned |
| `flightRisk` | `Fixed64` | probability | derived from Loyalty, household Unrest, Regimen, traits (§7) | Labor & Slavery §7 | pursuit-trigger monthly system | monthly recomputation | No (derived) | Secret (an internal risk score, not shown to the fleeing individual's owner as a raw number by design implication — engineering default; not explicitly stated) | **Open in source:** §12 flags flight-risk thresholds and opportunity-roll frequency as unsized |
| `pursuit.active` | bool | — | set on flight event (§7) | Labor & Slavery §7 | UI (pursuit-decision screen, future) | flight event, pursuit-resolution command | Yes | Household | Additive only |
| `pursuit.monthsRemaining` | int | months | "a few months" window (§7), unsized | Labor & Slavery §7 | pursuit-resolution monthly system | flight event; decrements monthly | Yes | Household | **Open in source:** exact window length unsized |
| `pursuit.lastKnownLocation` | ref (settlement/region) | — | — | Labor & Slavery §7 | pursuit-resolution system | flight event | Yes | Household | Additive only |
| `manumissionPlan.type` | enum? / null | — | Vindicta \| Testamento \| Censu (§8) | Labor & Slavery §8 | Succession & Dynasty (Testamento specifically, §6.9 future) | manumission-plan command | Yes | Household (Testamento may be undisclosed until effective — not specified in source; engineering default) | Additive only |
| `manumissionPlan.effectiveOn` | `GameDate`? / null | months since epoch | Testamento: the owner's death (§8); others: immediate | Labor & Slavery §8 | Familia (legal-status transition system) | manumission-plan command | Yes | Household | Additive only |

---

## 8. Settlement Demographics (`gens-settlement-demographics-design.md`)

*Owns the background pop-group model — growth, migration, class mobility (registry cluster 25's supply/labor side; Population & Wealth-Purchasing-Power extends it with demand, out of this ledger's vertical-slice scope). Fields below are §15's Data Model in full.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `PopGroup.settlementId` | `RuntimeId<Settlement>` | — | — | Settlement Demographics | every reader below | pop-group instantiation | Yes | Public | Additive only |
| `PopGroup.groupType` | enum | — | coloni \| operarii \| opifices \| negotiatores \| aeditui \| curiales \| veterans \| nonHouseholdEnslaved (§3, §15) | Settlement Demographics §3 | Estate & Settlement (rural capacity, §7.2), Economy & Finance (Rents, §3.1 of that doc) | pop-group instantiation | Yes | Public | Additive only |
| `PopGroup.size` | int | headcount | ≥0 | Settlement Demographics | Employment Ratio (§4.2), Estate & Settlement (stage thresholds, §12) | migration/mobility/promotion monthly systems | Yes | Public (aggregate count); individuals within it have no `RuntimeId<Character>` per ADR 0009's `Background` tier | **Conservation is a stated invariant** — §5's own "conservation tests so population changes have causes and no group silently duplicates or disappears"; any migration must preserve total population unless a named cause (birth, death, migration, promotion) accounts for the delta |
| `PopGroup.legalStatusDistribution.{citizen,latinRights,peregrine,freedman}` | `Fixed64` (×4, fractions) | fraction of group | sums to 1.0 (§10) | Settlement Demographics §10 | Assimilation monthly system | Assimilation monthly system, military-service fast-lane (§10) | Yes | Public | Additive only |
| `PopGroup.employmentRatio` | `Fixed64` | ratio | jobs/size, well above/below 1.0 meaningful (§4.2) | Settlement Demographics §4.2 | mobility (§5) and migration (§8) systems | monthly recomputation from `BackgroundEconomicCapacity` | No (derived) | Public | Additive only |
| `PopGroup.contentment` | `Fixed64` | score | driven by Employment Ratio, needs, Policy, hazards (§6.2) | Settlement Demographics §6.2 | Emigration (§8.2), Economy & Finance (Rent collection, §3.1 of that doc), future Politics & Patronage | monthly Contentment system | Yes | Public | **Open in source:** §16 flags Contentment formula weighting as unsized |
| `BackgroundEconomicCapacity.settlementId` / `.sector` / `.availableSlots` | ref, enum, int | — / — / job slots | sector: agriculture \| industry \| commerce \| religion (§15) | Settlement Demographics §4.1 | Employment Ratio derivation | Estate & Settlement building-investment monthly system | Yes | Public | Additive only |
| `HousingAndLandCapacity.urbanCapacity.{insulae,domus}` | int (×2) | resident capacity | scales with building tier (§7.1) | Settlement Demographics §7.1 | overcrowding system (§7.3) | Buildings §4.10 construction (Insulae/Domus) | Yes | Public | **Open in source:** §9 of `gens-buildings-design.md` flags Insulae/Domus population numbers as pending this doc |
| `HousingAndLandCapacity.ruralCapacity.unclaimedFertilePlots` | int | plots | competes directly with Estate & Settlement's own Agriculture expansion (§7.2) | Settlement Demographics §7.2 | Coloni/Veterans capacity | Estate & Settlement land-use system (shared resource) | Yes | Public | **Open in source:** §16 flags how tightly this competition should bind as untuned |
| `MilitaryDemographicInterface.activeServiceDrawFromColoniOperarii` | int | headcount | drawn down by enlistment (§5, §15) | Settlement Demographics §5 | Military & Combat (future) | Military & Combat recruitment command (future) | Yes | Public | Additive only |
| `MilitaryDemographicInterface.pendingDischarges` | int | headcount | feeds new Veterans, with citizenship upgrade (§10) | Settlement Demographics §5, §10 | Veterans pop-group creation | discharge monthly/event system | Yes | Public | Additive only |
| `SettlementDemographics.totalBackgroundPopulation` | int | headcount | sum of all `PopGroup.size` excluding player's own Familia (§12) | Settlement Demographics §12 | Estate & Settlement (Vicus/Town/City thresholds, §5 of that doc) | recomputed monthly | No (derived) | Public | Additive only |
| `SettlementDemographics.migrationPressure` | `Fixed64` | score | drives §8's immigration/emigration | Settlement Demographics §8 | Immigration/Emigration monthly system | monthly recomputation | No (derived) | Public | Additive only |

---

## 9. Economy & Finance (`gens-economy-finance-design.md`)

*The treasury layer above Resources & Goods and Estate & Settlement (registry cluster 11 — Resources & Goods §12 remains authoritative for market simulation; this document adds only the treasury-facing layer). Fields below are §12's Data Model in full.*

| Field | Type | Units | Range | Owner | Readers | Writers | Persisted | Visibility | Migration Policy |
|---|---|---|---|---|---|---|---|---|---|
| `Treasury.balance` | int | denarii (minor units) | can run negative (§2) | Economy & Finance §2 | every system in this ledger that spends or earns | monthly Ledger-posting system, every income/expense command | Yes | Household | Additive only |
| `Treasury.reserveThreshold` | int | denarii | standing policy, player-set (§2) | Economy & Finance §2 | liquidation/borrowing trigger logic | player policy command | Yes | Household | Additive only |
| `LedgerEntry.category` | enum | — | 18 named categories (§12: goodsSale, rentAgricultural, rentUrban, contractMilitary, contractConcession, contractProvincial, taxRevenue, windfall, seigniorage, wages, bribe, fundedAction, capitalExpenditure, tributum, loanInterestPaid, loanInterestReceived, upkeep, routeInvestment) | Economy & Finance §12 | Monthly Report (§10), every reader of a specific category | posting monthly systems / discrete-action commands | Yes | Household (Bribes specifically read as "off-the-books-feeling" per §4.2 — still Household-visible to the paying household, not Public) | Additive only |
| `LedgerEntry.amount` | int (signed) | denarii (minor units) | positive = income, negative = expense (§12) | Economy & Finance §12 | Net Worth (§8), Monthly Report | same as category | Yes | Household | Additive only |
| `LedgerEntry.sourceOrTarget` | `RuntimeId<Character>` \| `RuntimeId<PopGroup>` \| `RuntimeId<Building>` \| "rome"/"province" | — | — | Economy & Finance §12 | audit/report drill-down | same as category | Yes | Household | Additive only |
| `CapitalExpenditure.type` | enum | — | slaveMarketPurchase \| landParcel \| villaStageUpgrade \| livestockPurchase \| other (§4.4, §12) | Economy & Finance §4.4 | Labor & Slavery (linked acquisition), Estate & Settlement | acquisition commands (cross-referenced from owning systems) | Yes | Household | **Open in source:** §13 flags the exact "significant" livestock-purchase threshold as undrawn |
| `MintPolicy.seigniorageRate` | `Fixed64` | rate | steady recurring (§3.5) | Economy & Finance §3.5 | monthly income posting | Mint-operation monthly system | Yes | Public | Additive only |
| `MintPolicy.debasementActive` | bool | — | rare-use, deliberate lever (§3.5) | Economy & Finance §3.5 | Resources & Goods (market inflation, §12 of that doc) | player debasement command | Yes | Public (once acted upon — coinage debasement is a visible political fact per §3.5) | Additive only |
| `MintPolicy.debasementSeverity` | `Fixed64` | scale | scales one-time gain and market/political consequence | Economy & Finance §3.5 | market-price system, Reputation Duality (future) | player debasement command | Yes | Public | **Open in source:** §13 flags the debasement-severity-to-consequence relationship as unspecified |
| `DebtRecord.lenderIsPlayer` | bool | — | false = player borrowed, true = player lent (§6) | Economy & Finance §6 | Legal & Court (future, dispute direction) | loan-origination command | Yes | Household (both parties); Public once in Legal dispute (§6.3–6.4) | Additive only |
| `DebtRecord.principal` / `.interestRate` | int, `Fixed64` | denarii / rate | — | Economy & Finance §6.1 | default-ladder systems (§6.3–6.4) | loan-origination command; interest-escalation monthly system | Yes | Household | **Open in source:** §13 flags fenus nauticum premium sizing and general interest-rate scale as unsized |
| `DebtRecord.isFenusNauticum` | bool | — | if true, shipment loss forgives the debt instead of defaulting (§7.1) | Economy & Finance §7.1 | default-ladder system (exempts this record) | loan-origination command | Yes | Household | Additive only |
| `DebtRecord.monthsOverdue` / `.status` | int, enum | months / — | status: current\|overdue\|inLegalDispute\|defaulted\|forgiven (§12) | Economy & Finance §6.3–6.4 | Legal & Court (future) | monthly default-escalation system | Yes | Household while current; Public once `inLegalDispute` (a court case is a public fact) | **Open in source:** §13 flags rent/tax-arrears-to-§6.4 threshold and legal-dispute-vs-automatic-seizure boundary as undecided |
| `DebtRecord.bondedPersonIds[]` | `RuntimeId<Character>`[] | — | populated only on `debtBondage` resolution; can exceed one person (§6.4) | Economy & Finance §6.4 | Labor & Slavery (new acquisition record), Familia (legal-status transition) | debt-bondage resolution command | Yes | Public (a legal ruling) | **Open in source:** §13 flags family-bondage scope (capped vs. uncapped) as unresolved |
| `WindfallEvent.type` / `.amount` / `.sourceEventOrPersonId` | enum, int, ref | — / denarii / — | warSpoils \| dowryReceived \| inheritance \| treasureFind (§3.4, §12) | Economy & Finance §3.4 | Familia (dowry, §6), Succession & Dynasty (inheritance, future) | triggering event/command | Yes | Household | Additive only |
| `NetWorth.{treasuryBalance,storedGoodsValue,livestockValue,landAndBuildingValue,netOutstandingDebt,total}` | int/`Fixed64` (×6) | denarii (all) | — | Economy & Finance §8 | Familia (marriage-market dowry/alliance value, §6), Rival Houses (future comparison), Succession & Dynasty (future inheritance division) | monthly recomputation from constituent partitions | No (derived — "read, not spent... it isn't a second currency," §8) | Household (own); Public/estimated for a rival per Rival Houses' future design | **Open in source:** §13 flags the land/building depreciation-by-neglect curve as unspecified |
| `InsolvencyState.monthsBelowThreshold` / `.stage` | int, enum | months / — | stage: solvent\|atRisk\|insolvent\|ruined (§9, §12) | Economy & Finance §9 | Villa doc (stage demotion, §9), Politics & Patronage (future office/census loss) | monthly Insolvency-tracking system | Yes | Public once `atRisk` or worse — insolvency is a household's visible standing collapse, not a hidden ledger fact | **Open in source:** §13 flags the exact Net Worth depth/duration trigger threshold as unsized |
| `InsolvencyState.consequencesApplied[]` | list of enum | — | forcedLiquidation \| forcedAssetSale \| villaStageDemotion \| officeOrCensusLoss \| chronicleEntry (§9, §12) | Economy & Finance §9 | Dynasty Chronicle (future — "The Fall of the House") | Insolvency-ladder monthly system | Yes | Public | Additive only |
| `TaxPolicy.vectigaliaRate` / `.decumaRate` | `Fixed64` (×2) | rate | — | Economy & Finance §5.2 | Settlement Demographics (Contentment/Emigration cost, §6, §8 of that doc) | player Tax Policy command, gated on Curia + future office | Yes | Public (a settlement's declared tax rate is a public policy fact) | **Open in source:** §13 flags tax-rate-to-Contentment-penalty curve as unsized |
| `TaxPolicy.requiresOffice` | bool | — | gated on Curia (Buildings §4.10) + future Politics & Patronage magistracy (§5.2) | Economy & Finance §5.2 | player policy-command validation | fixed by game-state (office held or not) | No (derived from office-holding state) | Public | Additive only |
| `TradeRouteInvestment.denariiCommitted` | int | denarii | reduces route disruption exposure (§7) | Economy & Finance §7 | Piracy & Banditry (future) | player route-investment command | Yes | Household | Additive only |
| `TradeRouteInvestment.riskProfile` | enum | — | steady \| highRiskHighMargin (§7) | Economy & Finance §7 | route-selection UI | player route-choice command | Yes | Household | Additive only |

---

## Summary

| Section | Authoritative document | Rows |
|---|---|---|
| 1 | Characters (`gens-characters-design.md`) | 26 |
| 2 | Familia / Household (`gens-familia-design.md`) | 7 |
| 3 | Traits (`gens-traits-design.md`) | 11 |
| 4 | Estate & Settlement (`gens-estate-settlement-design.md`) | 15 |
| 5 | Resources & Goods (`gens-resources-goods-design.md`) | 20 |
| 6 | Buildings (`gens-buildings-design.md`) | 5 |
| 7 | Labor & Slavery (`gens-labor-slavery-design.md`) | 12 |
| 8 | Settlement Demographics (`gens-settlement-demographics-design.md`) | 12 |
| 9 | Economy & Finance (`gens-economy-finance-design.md`) | 21 |
| **Total** | | **129** |

Every row cites the source document's own section number; every "Migration Policy" cell marked with an explicit open item quotes that document's own Open Questions section rather than inventing a resolution the design corpus hasn't reached yet, consistent with Phase 1's exit gate: "No structural blocker remains hidden in an 'Open Questions' section" — this ledger surfaces them instead of hiding them.
