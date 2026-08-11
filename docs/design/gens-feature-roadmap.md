# Gens — Comprehensive Feature Roadmap

**Companion to:** [About Gens](gens-about.md) (the pitch), the [design index](README.md) (the specifications), the [Canonical Object & Data Registry](../gens-canonical-registry-design.md) (the content inventory), the [Design Authority Registry](../gens-design-authority-registry.md) (ownership per shared concept), and the [Comprehensive Build Roadmap](../engineering/gens-comprehensive-build-roadmap.md) (the engineering construction order).

## Purpose and how to read this against the build roadmap

The engineering [Comprehensive Build Roadmap](../engineering/gens-comprehensive-build-roadmap.md) answers *how Gens gets built* — kernel, saves, content contracts, then systems, phase by phase, bottom-up, in dependency order. This document answers a different question: ***what player-facing feature does each stage of that construction actually deliver, and in what order should the 27+ core systems and their extensions come online for someone playing the game rather than building it.*** It is a feature/content sequencing plan, not a new engineering plan — every wave below maps onto one or more phases of the build roadmap, and every system named cites its authoritative document per the Design Authority Registry.

The order follows one rule throughout: **a feature ships only after every system it depends on for its own numbers, actors, or record-keeping already exists.** This keeps the sequencing honest — for example, Politics & Patronage cannot ship before Familia and Dignitas exist to run on, and Games & Spectacle cannot ship before Fame (owned by Games & Spectacle itself, per Design Authority §7) and Labor & Slavery (for gladiators) both exist.

---

## Roadmap at a glance

| Wave | Theme | Player experience unlocked | Build-roadmap phases |
|---|---|---|---|
| 0 | Foundation | Nothing playable yet — the world can boot, save, and advance time | Phases 0–4 |
| 1 | The Household | A living family: birth, aging, marriage, traits, appearance | Phase 5 |
| 2 | The Estate | Land, a villa, goods, buildings, and labor that actually produce | Phase 6 |
| 3 | The Settlement | A population beyond the named household | Phase 7 |
| 4 | The Economy | Money, prices, wages, and scarcity | Phase 8 |
| 5 | The First Playable Loop | Actions, policy, events, a monthly report, and a Unity screen | Phase 9 |
| 6 | The Living World | Delegation and rival houses that act without waiting on the player | Phase 10 |
| 7 | Dynasty & Memory | Succession, inheritance, and a readable Chronicle | Phase 11 |
| 8 | Public Life | Reputation, patronage, religion, law, crime, and factions | Phase 12 |
| 9 | Geography & Reach | Regions, travel, correspondence, culture, and history | Phase 13 |
| 10 | Hazards & Mobile Populations | Disease, disaster, and itinerant groups | Phase 14 |
| 11 | Advanced Commerce | Property markets, partnerships, contracts, and infrastructure | Phase 15 |
| 12 | Conflict & Danger | Espionage, banditry, the military, and diplomacy | Phase 16 |
| 13 | Personal & Cultural Depth | Romance, activities, spectacle, art, and legacy objects | Phase 17 |
| 14 | Content Breadth & Release | Full catalogs, presentation, performance, and release ops | Phase 18 |

---

## Wave 0 — Foundation

**Player experience:** none yet. This wave produces the substrate every later feature stands on: a green, deterministic, saveable simulation shell that can advance months and validate content, but contains no gameplay system.

**Delivers:** a trustworthy CI/build pipeline; a design-authority registry and cross-system field ledger reconciling the 110-document corpus (this pass — see the [Design Authority Registry](../gens-design-authority-registry.md)); typed IDs, `WorldState`, tick phases, command/event envelopes, and RNG stream governance; canonical save serialization and a headless campaign runner.

**Why first:** every later feature writes through these contracts. Shipping a system before this wave exists means re-deriving IDs, save format, or event plumbing ad hoc per system — exactly the fragmentation the Design Authority Registry exists to prevent.

---

## Wave 1 — The Household

**Player experience:** a persistent named family that ages, marries, has children, dies, and carries traits and a face.

**Delivers:**
- **Familia** (owns lifecycle, household role, birth, marriage, legitimacy) and **Characters** (owns the universal person record, personality axes, and the Scheme engine) — both foundational per Design Authority §1 and §3.
- **Traits** (owns the full 219-entry catalog and three tiered spectrums, superseding the inline pairs Characters originally sketched — Design Authority §2), scoped at launch to a representative slice rather than all 219 entries at once.
- A first slice of **The Paper Doll** (appearance/portrait synthesis) and **Companions & Court Positions** (position roster only, not yet the mechanical depth Politics later builds on it).
- The lifecycle/lineage minimum of **Romance, Sexuality & Lineage** (Design Authority §4) — enough for births and legitimate marriage, not yet full adult romance.

**Why now:** named people are the shared record nearly every later document reads. Nothing about land, money, or politics is legible without a household to attach it to.

---

## Wave 2 — The Estate

**Player experience:** the household owns a place, stores goods, and produces something.

**Delivers:**
- **Estate & Settlement** (the physical growth engine — plots, terrain, building categories) and **The Villa** (the household's own interior residence, nested inside it — Design Authority §24).
- **Resources & Goods** (the authoritative 144-item Unified Goods Registry) and **Buildings & Production Chains** (94 building types, reading its terrain-gate table from Starting Regions per Design Authority §5).
- **Labor, Slavery & Punishment** — acquisition, assignment, discipline, overwork/health/revolt-risk tradeoffs, and manumission, with the same character depth as family members.

**Why now:** the estate is the first thing the player actually manages turn to turn; it needs land, storage, and labor as prerequisites before it can support population or an economy on top.

**Known pre-work:** the Buildings §2 and Estate & Settlement §8 goods lists are still self-flagged as superseded-but-not-cleaned-up by Resources & Goods (Design Authority §10) — the single most concrete unresolved contradiction in the corpus, and worth resolving before this wave's content is authored in bulk.

---

## Wave 3 — The Settlement

**Player experience:** the estate sits inside a living population, not an empty map — colonist influx, freedmen setting up shops, growth pressure on housing and food.

**Delivers:** **Settlement Demographics** (population groups by occupation/class/legal status/culture, employment, housing, migration) and **Population, Wealth & Purchasing Power** (the demand-side extension of the same pop-group system, not a competing model — Design Authority §25).

**Why now:** background population is what turns a private estate into a settlement, and it's the demand side the economy wave needs before a market can clear.

---

## Wave 4 — The Economy

**Player experience:** production and population interact through money — prices, wages, taxes, debt.

**Delivers:** **Economy & Finance** (household ledger, taxation, debt — the treasury-facing extension of Resources & Goods' market, not a re-simulation of it, per Design Authority §11) and the dynamic market itself (Resources & Goods §12 — supply/demand, seasonality, disruption).

**Why now:** without prices and wages, buildings and labor produce numbers nobody can spend, and every later system that touches money (policy funding, patronage gifts, dowries, bribes) has nothing real to draw from.

---

## Wave 5 — The First Playable Loop

**Player experience:** the game becomes legible and playable end to end — issue actions, set policy, react to events, read a monthly report, see a first Unity screen.

**Delivers:**
- **Policies & Edicts** (12 standing policies, 9 edicts, 7 household doctrines) as lasting, revisable choices rather than one-off prompts.
- **Events** (triggered delivery, the weighted event pool, chains — the mechanism) as distinct from the Historical Timeline content that later reads through it (Design Authority §18).
- **The City Bulletin** (Monthly Report & Daily Acta) generated entirely from domain events.
- The first Unity vertical slice: household roster, estate/settlement, monthly report, and character-detail screens, with the wax-seal confirmation interaction and the diptych layout described in the [core design](gens-core-design.md).

**Why now:** this is the wave that turns a correct simulation into an actual game — everything before it is invisible to a player; everything after it extends a loop that already works, rather than bootstrapping one from scratch.

**This is the first shippable vertical slice.** Its scope matches the build roadmap's own "Household Economy Vertical Slice" milestone (Phases 5–9): one estate, 6–10 named household members, background population groups, three production chains, one market contract, a small policy set, one overseer, one rival seed, and three event chains.

---

## Wave 6 — The Living World

**Player experience:** the world keeps moving without the player — a steward can run routine business, and rival houses chase the same land, offices, and marriages on their own initiative.

**Delivers:**
- **Steward/Council Auto-Management** — the shared autonomy framework that Companions & Court Positions' Procurator, Travel's away-from-home household, and Succession & Dynasty's Regency all already assume exists (Design Authority §27).
- **Rival Houses & the Living World** — the shared Living World Actor abstraction (Design Authority §6), with **Notable Households**, **Wandering Populations**, and (later, Wave 11) **Notable Businesses** as its sibling instances, each an application of the same sampling-and-promotion pattern.

**Why now:** delegation and rivals are the first features that require background actors with their own agency — they need the full household/estate/economy stack from Waves 1–4 to have anything meaningful to act on.

---

## Wave 7 — Dynasty & Memory

**Player experience:** death changes play instead of ending it; the household's history becomes a first-class, readable record.

**Delivers:** **Succession & Dynasty** (player-chosen succession, adoption as a real political tool, optional contested-inheritance drama), the **Dynasty Chronicle** and **Dynasty Legacy Tree** (the illuminated-scroll historical record), **Ancestor Veneration & Funerary Customs** (the household's relationship with its own dead — distinct from Religion's relationship with the living household's gods, Design Authority §16), and **Epithets, Nicknames & Titles** (naming built directly on top of Scandal's mechanics, Design Authority §28).

**Why now:** this is the pillar the game is named for — "a game about lineage should let you read your lineage" — and it can only pay off once at least one generational transition is possible, which requires the full household and living-world stack already in place.

---

## Wave 8 — Public Life

**Player experience:** household choices operate inside a real social and political order — patronage, religion, law, crime, and factional pressure all respond differently depending on who you are and who's watching.

**Delivers, in the internal order the build roadmap's Phase 12 recommends:**
1. Dignitas, Fame (owned by Games & Spectacle, extended by Celebrities & Influential Figures per Design Authority §7), and audience-specific reputation.
2. **Politics & Patronage** (Faction, patron-client relationships, local magistracies, the distant *cursus honorum*) and **Reputation Duality** for frontier starts.
3. **Religion** (household gods, omens, festivals, priesthoods).
4. **Legal & Court** (trial, evidence, verdict — the sole owner of that process per Design Authority §13) and **Crime, Punishment & Imprisonment** (the punishment catalog and Detention status, leaving trial process untouched).
5. **Interest Groups** and **Collegia & Guilds** — the two non-overlapping organizing structures alongside Faction (Design Authority §22).
6. **Scandal** (exposure, spread, severity, Damage Control) and **Secrets & Hooks** (the Secret record and leverage mechanic that Espionage's content and Crime & Punishment's fabricated records both already assume, Design Authority §13).
7. **Celebrities & Influential Figures** and full public edicts/backlash feedback.

**Why now:** every system in this wave depends on Dignitas, a household, and an economy already existing to have stakes — a scandal or a court case means nothing without prior standing to lose.

---

## Wave 9 — Geography & Reach

**Player experience:** distance, language, and historical time start to matter — the world is bigger than one estate.

**Delivers:** the full **Starting Regions** schema and one complete region profile, then the wider 18-region roster in waves (6 launch regions, 5 promoted regions, 7 further individual region documents — see the [Canonical Object & Data Registry §1](../gens-canonical-registry-design.md#1-starting-regions-18)); **Travel** and **Correspondence & Letters** (the asynchronous counterpart to Travel, Design Authority §27); **Language & Literacy** (two derived stats reading from Education & Culture's Learning investment, Design Authority §29); **Cultures of the Known World** and **Religions of the Known World** (35 cultures, 27 named faiths); the **Roman Calendar** and **Seasons** (the structural and mechanical-effect layers underneath Events' `GameCalendar` and Religion's sacred calendar, Design Authority §15); the historical-timeline scheduler; and **Named Roads & Trade Itineraries**.

**Why now:** region-specific play (frontier Reputation Duality, non-Roman diplomacy, distant holdings) needs travel, correspondence, and a real calendar in place first, or "distance" has no mechanical cost.

---

## Wave 10 — Hazards & Mobile Populations

**Player experience:** environmental and biological pressure matters without arbitrarily destroying a save.

**Delivers:** **Disease & Public Health** (7 endemic + 4 epidemic named diseases) and **Natural Disasters & Environment** (five hazard types), each mirroring the other's multi-hazard design language while staying non-overlapping (Design Authority §30); **Wandering Populations** at full depth (routes, needs, recruitment, promotion to named characters).

**Why now:** hazards need buildings, population, markets, and travel already in place to have something real to damage or displace.

---

## Wave 11 — Advanced Commerce

**Player experience:** economic play expands from one household's market loop into property portfolios, partnerships, competitive contracts, and public investment.

**Delivers, in the internal order the build roadmap's Phase 15 recommends:** **Land Ownership, Estates & the Real Estate Market** (the individually-named property portfolio sitting alongside Estate & Settlement, Design Authority §24); **Societates & Business Partnerships** (structure and liability only — Sale/Acquisition/Merger stay owned elsewhere, Design Authority §12); **Merchant Families & the Equestrian Order**; **Notable Businesses** at full depth and **Business Competition** (the escalation ladder beyond Notable Businesses' own opening sketch); **Public Contracts & Competitive Bidding** and **Public Auctions** (two distinct real Roman institutions — state contracting versus competitive sale, Design Authority §26); **Private Infrastructure** and **Private Ships & Shipping Ventures**; **Public Works & Euergetism** (the functional depth behind Policies & Edicts' one-line category, Design Authority §26).

**Why now:** every feature here resolves through the actor, property, contract, ledger, and market contracts Waves 2–4 already built — none of them should invent a parallel economy, which is only possible once the shared contracts are proven stable.

---

## Wave 12 — Conflict & Danger

**Player experience:** coercion and external threat use the same world model as everything else, rather than a separate minigame.

**Delivers:** **Espionage & Information Network** (spy networks, targeting the full Living World Actor framework, Design Authority §20); **Piracy & Banditry** (the base Confederation actor) and **Bandit Lords & Outlaw Factions** (a rare named-individual tier on top of it, Design Authority §23); **Military & Combat** (a shared combat-resolution kernel reused by military, guards, raids, duels, and spectacle); **Servile Wars, Slave Revolts & Collective Resistance** (the collective-escalation tier above Labor & Slavery's individual mechanics, Design Authority §23); **Kidnap & Ransom** (unifying three previously-scattered fragments — Characters, Crime & Punishment, and Piracy & Banditry — into one owned mechanic, Design Authority §23); **Diplomacy with Non-Roman Peoples** and **Client Kingdoms & Vassal Rulers** (the generalized vassal-relationship engine four region documents had each independently reached for, Design Authority §20).

**Why now:** danger needs a living world, an economy, and reputation to threaten — it is deliberately the second-to-last wave rather than an early hook, so conflict has real stakes to interrupt.

---

## Wave 13 — Personal & Cultural Depth

**Player experience:** the game's richest, most personal expression — the layer most players will remember a playthrough by.

**Delivers, in the internal order the build roadmap's Phase 17 recommends:** full **Companions & Court Positions** depth and travel retinues; **Education & Culture** (pedagogy, patronage, Institutions of Renown); full adult **Romance, Sexuality & Lineage** (courtship, autonomous relationships, affairs and consequences, preserving the hard Adult-lifecycle gate — this supersedes **Romance & Seduction** for every implemented rule, Design Authority §4); the **Activities & the Activity Engine** (the shared six-slot hosted-activity anatomy) with **Feasts**, **Weddings**, **Pilgrimages**, and **Hunts, Beast Taming & Menageries** as its instances, each retaining domain-specific ownership of its own machinery (Design Authority §8); **Games & Spectacle** (gladiators, chariot races, wagering, Fame); **Books & Manuscripts** and **Art & Art Commissions** (authorship, provenance, copying, loss); **Masterworks & Unique Crafted Objects** (exceptional goods on the same provenance framework); **Monuments & Legacy Building** (13-item roster, building on Buildings §4.12 and feeding the Chronicle); **Social Places**, **Graffiti, Dynamic Walls & Rumors**, **Fashion & Dress** (plus the full Garment Roster), and **Hair, Facial Hair & Body Marking** — the descriptive layers the Paper Doll aggregates without owning (Design Authority §19).

**Why now:** every feature here reuses characters, actions, activities, objects, and the Chronicle that all prior waves built — none of it should introduce a parallel social or item model, which is only safe once those shared engines are stable.

---

## Wave 14 — Content Breadth & Release

**Player experience:** the integrated sandbox becomes the full, content-rich release — the complete Known World rather than a slice of it.

**Delivers:** the remaining world-reference registries at full depth — the **Bestiary** (60 non-legendary + 17 legendary creatures), the **Flora & Herbal Registry** (105 plants), the **Discovery Roster** (94 named technologies across six historical eras, riding on the Technology & Discoveries engine), and **Occupations & Trades** (119 named trades, explicitly a naming layer with no mechanics of its own, Design Authority §21); the full 219-entry Trait catalog and full 144-item Goods registry authored in complete rather than representative form; the remaining 14 of 18 Starting Regions; the complete visual system (ink bar, mosaic map, Chronicle presentation, accessibility passes); deterministic procedural portraits at full fidelity, with optional AI-assisted art strictly additive and never save-critical; and the performance, telemetry, localization, and release-gate work the build roadmap's Phase 18 defines in full.

**Why now:** this is deliberately last — every catalog here is safe to scale only once the systems that consume it (traits feeding characters, goods feeding the market, regions feeding travel and diplomacy) are already proven at a representative scale.

---

## Cross-cutting notes

- **This document does not re-litigate build order within a wave** — the [Comprehensive Build Roadmap](../engineering/gens-comprehensive-build-roadmap.md) is authoritative for the actual construction sequence, exit gates, and acceptance tests. This document exists so a feature-level question ("when does Games & Spectacle ship, and what does it need first?") has a direct answer without reading all 18 engineering phases.
- **Every system named above has exactly one authoritative document**, per the [Design Authority Registry](../gens-design-authority-registry.md). Where a wave lists an "extension," that document adds a bounded slice on top of another system's authoritative record rather than competing with it — implementation should always read the authoritative document first.
- **Content volume scales independently of feature order.** A system can ship in an early wave with a representative content slice (e.g., Wave 1 ships Traits with a handful of entries, not all 219) and still have its full catalog authored later, in Wave 14, without changing when the *mechanic* itself became playable.
- **Open items that could reorder this roadmap if left unresolved:** the Goods/Buildings contradiction flagged in Design Authority §10 (Wave 2); Romance & Seduction's exact supersession boundary, Design Authority §4 (Wave 13); and the lack of any sync mechanism for keeping the Design Authority and Canonical Registries current as new design docs are written (all waves, ongoing).
