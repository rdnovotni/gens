# GENS — System Design: Starting Regions (§4, extended)
*The framework document. This pass doesn't design any single region's full mechanical texture — that's deliberately deferred to a dedicated design document per region, the same way this project treats every other major system. What this document does is define what a "region" *is*, what every region document must specify, which regions exist (launch roster plus an extensible slate), and the shared mechanics — selection, customization, distance, a real historical Gazetteer, and rival seeding — that every region-specific document will plug into rather than reinvent.*

---

## Contents

1. Scope & Role
2. The Region Concept
3. Region Selection & Customization — The Free Cities Model
4. The Region Profile Schema
5. The Regional Roster
6. Reputation Duality Applicability
7. Distance & Expansion — Holdings Beyond the Home Region
8. The Regional Gazetteer — Real Places, Abstract Estate
9. Regional Rival Seeding
10. Customization & Accessibility Options
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role

The core doc's own Setting & Start Selection section sketches four regions in a paragraph each. That was always a placeholder — a gesture at the idea of region-driven variety, not a system. This document promotes "starting region" to what it always implied it would become: a real, structured choice with mechanical teeth, on par with Culture, Start Mode, and starting Familia composition as a foundational character-creation decision.

This document is explicitly the **skeleton, not the flesh**. Per direction, it establishes:

- What a region is and what it's responsible for determining.
- A shared **Region Profile schema** — the checklist every region-specific document must fill out, so five (or more) separate documents read as one coherent system rather than five inconsistent ones.
- The **regional roster** — which regions exist at launch, and which sit in an extensible slate for future passes.
- The mechanics that cut across all regions at once: selection and customization, distance/relocation, rival seeding, and accessibility toggles.

Each region's actual terrain mix, starting economic package, political texture, diplomatic neighbors, religious defaults, and named flavor content is the subject of its own future document — **Starting Regions: Italian Heartland**, **Starting Regions: Gallic Frontier**, and so on — each of which fills in this document's schema rather than inventing its own structure.

---

## 2. The Region Concept

A **region** is the answer to "where does this household's estate actually sit in the Roman world" — the single character-creation choice with the widest downstream reach, because nearly every other system reads it at least once: Buildings' terrain gating, Resources & Goods' regional specialties, Politics & Patronage's access to Rome, Diplomacy with Non-Roman Peoples' neighbor roster, Religion's local cult exposure, Education & Culture's Cultural Drift baseline, Piracy & Banditry's threat profile, and Rival Houses' local competitive field all take a region as an input.

Two things a region is deliberately **not**:

- **Not a literal map tile.** Per §8, a region is an abstract flavor zone with a coherent identity, not a hex on a world map the player physically maneuvers across. The player is building a villa, not conquering a board.
- **Not a culture assignment.** Per §3, the region a household settles in and the culture that household identifies with are related but independently chosen — a Roman citizen family can found an estate in Gallic frontier territory, or a Gallic freedman family can hold land in the Italian heartland, and the game should represent both without friction.

---

## 3. Region Selection & Customization — The Free Cities Model

Per direction, this system takes its cue directly from *Free Cities*' own highly customizable start: broad, sensible defaults the player can accept as-is, with every layer underneath open to override for a player who wants to hand-build a very specific situation.

### 3.1 The Two Independent Choices

Region and Culture are selected **separately**, not as a single bundled pick:

- **Region** determines the *place* — terrain, local economy, local politics, local threats, and (per §3.2) the *background population's* likely culture mix.
- **Culture** (Education & Culture §2) determines the *starting household's* own cultural identity, independent of where they happen to live.

A player can pick Gallic Frontier as their region and Roman as their starting Familia's culture (a colonial administrator household), or Gallic Frontier as the region *and* Gallic as the starting culture (an assimilated or newly-enfranchised local family building a Roman-style estate on native ground) — both are real, intended playstyles, not an edge case either has to fight the character creator to reach.

### 3.2 Distribution, Not Assignment — NPCs Skew, They Don't Lock

Every region carries a **weighted culture distribution** for procedurally generated Characters (locals hired as labor, met in Politics & Patronage, encountered in Diplomacy, rolled as Rival Houses) — this is the actual mechanical payoff of the region choice for the *living world* around the player, distinct from the player's own household. A Greek East start's local population skews heavily Hellenic; a Gallic Frontier start's skews Gallic with a real, growing Roman colonial minority; and so on. Cultures of the Known World's own regional tagging (§12 of that document) is the source table this distribution reads from.

Critically — and this is worth stating as a hard design rule rather than an implementation detail — **every distribution always has room for outliers**. A Syrian merchant's household turning up in the Gallic Frontier, a Hellenic tutor working in the Italian heartland, an Egyptian trader passing through the Iberian colony — all real, all historically plausible given how connected the Roman trade world actually was, and all deliberately left possible by keeping the distribution weighted rather than exclusive. A region that could *only* ever generate its own "native" culture would read as flatter and less alive than the actual ancient Mediterranean ever was.

### 3.3 What the Player Can Override

Consistent with Full Custom's own existing scope (Core §5.1), a player using Full Custom start mode can override:

- The starting Familia's culture, independent of region.
- The starting Familia's Legal Status mix (a Full Custom Iberian start with an already-citizen family, for instance, rather than the region's more typical Peregrine-leaning default).
- Whether Regional Rival Seeding (§9) uses this region's pre-filled rival gentes, generates them procedurally instead, or is turned off outright.
- The specific terrain/feature roll for the starting plot, within whatever range that region's own document defines as plausible (see §4.1).

Templated Backgrounds, Randomized, and Scenario starts (Core §5.2–5.4) instead pull from each region's own sensible defaults, exactly the way *Free Cities*' quick-start options do — full depth available, but never mandatory to engage with.

---

## 4. The Region Profile Schema

This is the actual deliverable of this document: the fixed checklist every region-specific design document fills out, so "Starting Regions: Iberian Colony" and "Starting Regions: Greek East" read as two instances of one system rather than two unrelated documents. Numbers are deferred everywhere, per this project's standing convention — a region document specifies *shape and direction*, not magnitudes.

### 4.1 Terrain & Feature Profile
Which of Buildings' terrain gates (Coast, River, Forest, Hills/Mountain + Deposit, Fertility, Meadow) this region's starting plots plausibly roll, and in what typical mix. Reads directly into Buildings §3's existing gate table and its "regional flavor" weighting note — this document doesn't replace that table, it's the region document's job to say which parts of it apply.

### 4.2 Economic Package
Land cost, starting Treasury range, and market depth/liquidity — flagged here as **primarily a Start Modes concern** (each Start Mode already defines its own resource envelope) rather than something this document dictates directly. A region document's role is narrower: describing the *qualitative* economic character (expensive-but-liquid vs. cheap-but-thin) that Start Modes' own numeric packages should read as flavor guidance when they're tuned.

### 4.3 Political & Legal Texture
Access to Politics & Patronage's cursus honorum track, typical local Faction exposure, and the region's baseline Legal Status mix (citizen/Latin/Peregrine) among both the starting household's options and the surrounding population, per Legal & Court and Familia's existing status mechanics.

### 4.4 Diplomatic & Military Exposure
For frontier-adjacent regions: a named default neighboring people or peoples for Diplomacy with Non-Roman Peoples to hook into immediately, plus baseline Piracy & Banditry raid-exposure weighting (land-based vs. sea-based, per §6/§7 of this document) and any regionally-flavored recruitment pool for Military & Combat and Companions & Court Positions.

### 4.5 Religious & Cultural Defaults
A default local cult or pantheon the household is exposed to from turn one (Religion's foreign-cult/syncretism mechanics), and a starting Cultural Drift lean (Education & Culture) reflecting how readily a household in that region trends toward the locally dominant culture absent player intervention.

### 4.6 Regional Goods & Trade
Inherits Resources & Goods' existing region-tagged goods table (§5/§7 of that document) as authoritative. A region document's job is to describe the resulting *production identity* this creates (a mining-and-metals economy, a grain-and-oil economy, a luxury-textile economy) rather than re-deriving the goods list from scratch.

### 4.7 Population & Culture Distribution
The weighted culture-distribution table described in §3.2, sourced from Cultures of the Known World's regional tags, plus a short note on the region's realistic outlier range.

### 4.8 Regional Rival Seeding
A small set of named, pre-sketched rival gentes native to the region (per §9), sized to give the region real living-world texture at launch without pre-writing its entire political landscape.

### 4.9 The Regional Gazetteer
A curated list of real, historically-grounded places (per §8) — each with a Role, a Prominence Tier, and a grounding note — plus the region's single designated Home Anchor (§8.1), for Travel, Politics & Patronage, Religion, Diplomacy, and Espionage to actually transact with.

### 4.10 Reputation Duality Applicability
Whether, and how, this region uses the Reputation Duality split (per §6 of this document).

---

## 5. The Regional Roster

### 5.1 Launch Regions (Five, Not Four)

Per direction, Iberian and North African are split into two regions rather than the core doc's original merged "Iberian/North African colony" — Cultures of the Known World already treats Numidian/Mauri and Punic as culturally distinct from Iberian/Celtiberian/Lusitanian, and the two regions' real economic and military histories (Iberia's mining-and-conquest arc via the Punic and Cantabrian Wars; North Africa's grain-basket-and-Punic-legacy arc centered on Carthage's real aftermath) are different enough to earn separate documents rather than one document doing double duty.

| Region | One-line identity | Primary future document |
|---|---|---|
| **Latium** | Rome's own immediate political orbit — highest prestige ceiling, densest political competition, fastest cursus honorum access, real economic fragility (grain-import dependent). | *Starting Regions: Italian Heartland (Latium & Campania)* |
| **Campania** | Bay of Naples wealth and cosmopolitanism — rich volcanic agriculture, major trade ports, real Greek/Oscan cultural layering, and a live catastrophic risk in Vesuvius. | *Starting Regions: Italian Heartland (Latium & Campania)* |
| **Gallic Frontier** | Cheap land, room to grow, real security risk, the primary Reputation Duality setting. | *Starting Regions: Gallic Frontier* |
| **Iberian Colony** | Mining and metals wealth (gold, silver, tin), a real, dateable conquest arc (Cantabrian Wars closing it out), moderate risk. | *Starting Regions: Iberian Colony* |
| **North African Colony** | Grain-basket agriculture, olives, a living Punic cultural legacy in eclipse rather than erased, moderate risk. | *Starting Regions: North African Colony* |
| **Greek East** | Culture/education prestige, deep trade networks, distinct Hellenistic legal and social custom. | *Starting Regions: Greek East* |

*(The launch roster is now six regions, not five: per the Italian Heartland's own region document, "Italian Heartland" split into Latium and Campania rather than remaining one merged entry — the same reasoning that split Iberian from North African applies here, since Rome's own immediate political backyard and the Bay of Naples' trade-and-leisure economy are different enough in texture, risk profile, and identity to earn separate selections rather than one paragraph doing double duty.)*

*(Note for a future Resources & Goods correction pass: that document's existing table sometimes tags a good simply "Iberian" where the intent, post-split, is Iberian specifically rather than the old merged Iberian/North African label — e.g., Esparto Grass and Cinnabar are genuinely Iberian, not North African. Flagged here rather than silently changed, since Resources & Goods is that document's own authoritative territory.)*

### 5.2 The Extensible Slate

Per direction, expanded from the core doc's original three-item wishlist. Each entry below gets a short rationale rather than a full profile — these are genuine future candidates, not yet scheduled for their own document, and none of them is assumed complete until it receives one.

- **Egypt** *(promoted — see* Starting Regions: Egypt*)* — client status until 30 BC, then a uniquely-administered imperial province in its own right, now fully realized: governed by an equestrian Prefect rather than a senatorial governor, with senators themselves formally barred from entering without imperial permission. Nile flood-cycle agriculture inverts this project's own usual Flood/Drought hazard logic, per that document's own §2. Alexandria's own Institution of Renown status (Education & Culture) makes it this roster's genuine administrative and intellectual capital at once, and that document's own §6 establishes a third real Reputation Duality shape — Permanent Structural Duality — distinct from both "full" and "tapering."
- **Syria / The Levant** *(promoted — see* Starting Regions: Syria / The Levant*)* — the richest culturally-contested candidate on this list, now fully realized: Syrian/Levantine, Judaean, Nabataean, and Palmyrene cultures all sit in this one real geographic space, giving it genuine internal texture (desert caravan trade, a client-to-provincial Judaea with real religious-legal friction, Palmyra's quasi-autonomous merchant-city status) that no other region replicates. That document's own §5 clarifies this region's relationship to Greek East's prior Parthia claim: Syria holds the frontier's heavier, more central sector (Zeugma), while Greek East's Cappadocia anchors a real but comparatively northern, Armenia-focused secondary sector of the same overall border — both true, not competing. That document's own §6 also introduces this project's fourth Reputation Duality shape, Localized, for a region where the mechanic's applicability genuinely varies by specific sub-area.
- **Britannia** *(promoted — see* Starting Regions: Britannia*)* — the most dramatically volatile frontier candidate, now fully realized: Frontier shifting to Provincial region-by-region across the real historical range, with Boudicca's real revolt (AD 60–61) as a genuinely live, dangerous mid-game Divergence point under a default start — the first Timeline Hook on this project's roster that isn't closed history by default. That document's own §8 corrects this note's original "overlaps with Iberian Colony" line: the real overlap is with Gallic Frontier's own Tin tag, not Iberia's Gold/Silver, resolved there as a real point-of-origin/trade-route split rather than a contradiction.
- **Anatolia / Asia Minor** *(promoted — see* Starting Regions: Anatolia / Asia Minor*)* — now fully realized, and honestly renegotiated from this note's own original pitch: Cappadocia, Rhodes, and Pergamon had all since been claimed by Greek East's own finalized document by the time this region was actually built, so its real, distinct identity instead runs through Galatia, Cilicia, Bithynia-Pontus, and the western coast beyond Pergamon and Rhodes specifically — Ephesus (resolving Religions of the Known World's own dangling Cult of Artemis reference), Sardis, and a real, concrete Correspondence & Letters case study via the Bithynia-Pontus governor-to-Emperor correspondence. Genuinely calm — that document's own §6 designates it Reputation Duality None, the same honest finding Italian Heartland and Greek East already reached.
- **The Balkans (Illyria/Pannonia/Thrace/Dacia corridor)** *(promoted — see* Starting Regions: The Balkans*)* — now fully realized, and the roster's own clearest structural first: Dacia's real AD 106 closing date is so late that it isn't selectable territory at all for any playthrough starting earlier — a genuine, unique case of a region's own map growing mid-game rather than merely tapering in tension over time. That document's own §6 reuses Syria/The Levant's Localized Reputation Duality shape as its own second application, confirming the pattern as genuinely reusable rather than a one-off.

---

## 6. Reputation Duality Applicability

Per direction to make the call on mechanical/historical grounds: Reputation Duality (Politics & Patronage §2.1) should extend beyond its original "principally Gallic" framing to **any region where Roman authority is recent, contested, or administratively thin** — the actual real-world condition the mechanic is modeling — rather than being hard-locked to one region's flavor text.

| Region | Reputation Duality? | Reasoning |
|---|---|---|
| Italian Heartland | No | Roman authority isn't a live question here; there is no "local, non-Roman" populace to hold a second axis of standing with. |
| Gallic Frontier | **Yes — full** | The mechanic's original, intended home. |
| Iberian Colony | **Yes, tapering** | Iberia's own conquest arc closes late (Cantabrian Wars, 29–19 BC) and unevenly by sub-region — a household starting early in the game's range plays a genuinely dual-standing frontier, while one starting late plays a largely-settled colony where the axis has mostly (but not entirely) converged. The region document should let start year/scenario modulate how "live" the split still is, rather than treating it as binary. |
| North African Colony | **Yes, tapering** | Similar logic — Punic cultural legacy in eclipse rather than erased means local standing remains a real, distinct thing to hold even once formal conquest is old news, just at lower intensity than an active frontier. |
| Greek East | No | Provincial for the entire range, urbane, deeply integrated into the wider Hellenistic-Roman world — a "local standing vs. Rome standing" split doesn't reflect anything real here the way it does on an actual military frontier. |
| Britannia | **Yes — full** | Established in that document's own §6: Britain never gets a clean conquest-closing date within this game's range, and Boudicca's revolt proves even the "settled" south's own loyalty stayed genuinely fragile well past the point Gallic Frontier's own population typically reads as pacified. |
| Egypt | **Yes — a third shape: Permanent Structural** | Established in that document's own §6: Egypt's transition to Roman rule was sudden and total, so there's no extended conquest period to taper away from — but its native culture, religion, and administration were kept genuinely, deliberately separate from ordinary provincial life for the entire range, by real Roman design rather than incomplete conquest. A structural rather than a temporal tension. |
| Syria / The Levant | **Yes — a fourth shape: Localized** | Established in that document's own §6: this region genuinely contains three different local-standing textures at once — Syria proper (none), Judaea (full, and arguably the sharpest on the whole roster, given two separate real revolts within range), and the desert trade cities (a distinct, non-adversarial Cooperative Client relationship). The mechanic's applicability depends on which specific sub-area a household's own life actually touches, not one regional dial. |
| Anatolia / Asia Minor | No | Established in that document's own §6: no live external conquest, no sub-area carrying anything like Judaea's or Britannia's own genuine tension — a genuinely calm region, and this document treats that as an honest finding rather than a gap needing a novel mechanic. |
| The Balkans | **Yes — Localized (second application)** | Established in that document's own §6: reuses Syria/The Levant's Localized shape rather than inventing a sixth mode, applied to its own distinct sub-area breakdown — Illyria/Moesia/Thrace (none), Pannonia (tapering, hinged on Bato's Revolt), and Dacia (full, and only ever available from AD 106 onward per that document's own §1 structural premise). |
| Sicily | No — the deepest "None" on the roster | Established in that document's own §6: over a century of integration before this game's own range even opens, making Sicily's own calm the oldest and most settled on the entire roster, Italian Heartland included. |
| The Alpine Provinces | No | Established in that document's own §6: both Raetia and Noricum settle quickly and durably after their own different real founding moments (conquest and peaceful annexation, respectively) — the third region on the roster, after Anatolia and Sicily, to reach this same honest finding. |
| Armenia | **N/A — replaced by Great Power Allegiance** | Established in that document's own §6: none of this project's four existing Reputation Duality shapes describe a household whose entire outward-facing concern is which of two outside empires currently holds sway, rather than reconciling local standing with a single ruling power. That document introduces a genuinely new, fifth concept instead, built directly on Diplomacy with Non-Roman Peoples' own existing `ArmenianAllegiance` field rather than a sixth Reputation Duality mode. |
| Mesopotamia | **N/A — not yet formed by default; "full" if the rare Sustained Occupation Divergence succeeds** | Established in that document's own §6 and §16: this region only exists as Roman territory for two real years by historical default, nowhere near enough time for local standing, imperial standing, or any tension between them to develop into a stable pattern. That document's own §16 opens one explicit, deliberate exception to this project's "real outcomes aren't rewritten" rule — a household whose own governance is extraordinary enough can plausibly extend the occupation past AD 117, at which point Reputation Duality begins accumulating like any fresh occupation. |
| Nubia | **N/A — no Roman authority ever existed here** | Established in that document's own §6: the simplest possible reason of all five "doesn't apply" findings on this roster — Nubia was never Roman territory at any point across the entire range, so there is no administrative relationship for a household's standing to be dual with in the first place. |
| Arabia Felix | **N/A by default, same reasoning as Nubia; "full" if the optional Alternate History Layer is toggled on** | Established in that document's own §6 and §16: no Roman authority ever existed here in the honest historical record, but that document's own explicit, player-toggled counterfactual (Aelius Gallus's real 26–24 BC expedition, reimagined as succeeding) would generate fresh Reputation Duality from scratch if a player opts into it. |
| The Bosporan Kingdom | No, and deliberately not a new mechanic | Established in that document's own §6: a genuinely low-friction, stable, permanent Client relationship — the same honest "None" finding Anatolia, Sicily, and the Alpine Provinces all reached, though for a fourth distinct reason. That document's own live Sarmatian/Scythian frontier relationship is handled by reusing Diplomacy with Non-Roman Peoples' existing Frontier toolkit directly, rather than inventing a bespoke mechanic — the clearest example yet of this project's own growing preference for reuse over invention. |

A fourth `reputationDualityMode` value — `"localized"` — is added to this document's own data model (§12) to reflect Syria's own determination; the mode enum now reads `"none" | "full" | "tapering" | "permanentStructural" | "localized"`.

This gives the mechanic real texture across the roster without stretching it somewhere it doesn't historically belong, and gives each region document a clear, pre-settled answer rather than re-litigating the question independently.

---

## 7. Distance & Expansion — Holdings Beyond the Home Region

Per direction to weigh in: yes, a household should be able to acquire a second holding outside its home region as the game progresses — the core doc's own vision of dynasties growing "into something much larger" reads naturally as eventually meaning geographic reach, not just a single villa's own footprint — and yes, distance should cost more, but abstractly rather than through literal mileage (consistent with §8's flavor-zone approach).

### 7.1 Distance Tiers, Not Distances

Every region pair carries an abstract **Distance Tier** relative to the household's home region — **Near**, **Moderate**, or **Far** — rather than a computed geographic distance. Italian Heartland to Greek East might sit at Moderate (a real, meaningful sea journey, but a well-traveled and administratively normal one); Italian Heartland to Britannia would sit at Far (the edge of the known-and-governed world). This is a simple, hand-assigned lookup table per region pair, not a formula — consistent with keeping the whole system abstract.

### 7.2 What Distance Actually Costs

A Far acquisition costs more in three real, distinct ways rather than one blunt price multiplier:

- **Land acquisition cost** (Estate & Settlement) — a straightforward premium, reflecting genuine unfamiliarity with a distant market and the practical friction of negotiating land purchase from afar.
- **Administrative overhead** — a distant holding cannot be run by the player's direct hand-on-the-tiller attention the way the home estate can; it functionally *requires* a trusted **Procurator** (Companions & Court Positions' existing role) rather than merely benefiting from one, and a Far holding without a competent, high-loyalty Procurator in place should carry a real, ongoing risk of mismanagement, skimming, or drift that a Near second holding wouldn't.
- **Travel time and risk** (Travel) — visiting a Far holding in person costs real time off the calendar and carries whatever regional Piracy & Banditry/Natural Disaster exposure the intervening journey implies, making "just go check on it yourself" a genuinely costlier choice the farther out the holding sits.

### 7.3 Why This Is a Good Tradeoff, Not a Punishment

This keeps Design Pillar #1's "no dominant setting, only tradeoffs" intact at the expansion layer specifically: a Near second holding is cheap and easy to administer but competes for the same limited land and rivals as the home estate; a Far holding is expensive and administratively risky but opens access to an entirely different regional goods profile, culture, and political theater the home region doesn't offer. Neither is simply better — expanding far is a real strategic identity (the "wide roads" flavor Policies & Edicts' own Hybrid Doctrine list already names), not an inevitable late-game default.

---

## 8. The Regional Gazetteer — Real Places, Abstract Estate

Revised per direction: the player's own estate footprint stays exactly as abstract as §2 and §7 already establish — no hex grid, no forced coordinate, no plot the player physically walks across. But the *world around* that estate shouldn't be abstract in the same way. Per direction, this document now asks every region to carry a real, CK3-style **Gazetteer**: a curated list of actual historical places — real cities, ports, sanctuaries, fortresses, and roads that genuinely existed in that region — each carrying enough identity that the player can travel to it, transact with it, be threatened by it, or see a rival house seated in it, the same way a CK3 county is a real place on the map even though the player never has to simulate its internal economy tile by tile.

The abstraction and the concreteness sit at two different layers on purpose:

- **The player's own estate and its expansion** — still fully abstract, per §2 and §7. The player doesn't place their villa at a specific coordinate; it simply exists "in" the region (and, per §7, an eventual second holding exists "in" whichever other region the player expands into).
- **The region's own historical geography around that estate** — now concrete and real. The player's abstract estate sits conceptually *near* one or more of the Gazetteer's real places, without ever needing to be pinned to an exact spot relative to them.

### 8.1 The Home Anchor

Each region document assigns the player's starting estate a **Home Anchor** — the single Gazetteer entry the estate is described as sitting nearest to (a day's ride from Narbo Martius, in sight of the road to Corduba, and so on). This is a flavor and Travel-cost convenience, not a coordinate: Travel to the Home Anchor is treated as effectively local and low-cost, while Travel to every other Gazetteer entry in the same region carries whatever ordinary in-region Travel cost that system already defines — no new distance math, just a single fixed reference point every region needs exactly one of.

### 8.2 Gazetteer Entry Anatomy

Every Gazetteer entry carries:

- **A real historical name** — Narbo Martius, Tarraco, Gades, Alexandria, Ephesus, Palmyra, Londinium — drawn from the genuine geography of that region and era, consistent with this project's standing "real grounding" convention (the same discipline Cultures of the Known World already applies to every culture it names).
- **A Role** (§8.3) — what kind of place it is, which determines what a player can actually *do* there.
- **A Prominence Tier** — **Provincial Seat**, **Regional Center**, or **Outpost/Waystation** — a light, three-step read of how significant the place is, deliberately not a numeric population or wealth figure (consistent with this project's no-numeric-sizing convention). This is flavor and interaction-depth signaling, not a simulated economy.
- **A one- or two-sentence historical grounding note**, in the same voice Cultures of the Known World already uses to justify why a given entry belongs on the list.
- **An optional Rival Seat flag** (§9) — whether a pre-filled rival gens is seated there at game start.

### 8.3 Location Roles — What a Gazetteer Entry Is For

A Role isn't flavor text alone — it's what actually determines which of a region's own cross-referenced systems can transact with that place:

| Role | What happens there |
|---|---|
| **Capital** *(Rome only)* | Introduced by the Italian Heartland region document — Rome itself isn't a provincial seat (Italy was never organized as an ordinary province under the Principate), it's the single unique seat of the cursus honorum, the Senate, and the widest possible range of Politics & Patronage, Legal & Court, and Games & Spectacle actions in the game. Only one Gazetteer entry in the entire roster ever carries this Role. |
| **Provincial Seat** | The region's own administrative capital — Politics & Patronage's cursus honorum actions, provincial governance interactions, and Legal & Court's higher magistrate rulings anchor here. Usually only one per region. |
| **Major Port** | Trade volume, Resources & Goods' import/export flow, and the embarkation point for any Travel leaving the region entirely (including toward a distant second holding, per §7). |
| **Legionary Base/Camp** | Military & Combat's recruitment, muster, and (for a frontier region) the standing garrison a Frontier Security Posture (Policies & Edicts §2.12) actually musters out of. |
| **Sanctuary/Temple Site** | Religion's pilgrimage, favor-seeking, and Haruspex-consultation actions; a natural Omens/Auspices flavor location. |
| **Market/Trade Hub** | A concrete venue for Resources & Goods' ordinary market dynamics and for Politics & Patronage's lower-stakes local Clientela dealings, distinct from a Major Port's larger-scale trade. |
| **Frontier Outpost** | Diplomacy with Non-Roman Peoples anchors a neighboring people's territory and treaty-negotiation site here rather than to a vague "nearby"; also a natural Espionage or Piracy & Banditry raid-target flavor location. |

A single Gazetteer entry can reasonably carry more than one Role (a Major Port that's also the Provincial Seat is realistic and common); a region document should feel free to double them up rather than forcing every entry into exactly one slot.

### 8.4 What the Gazetteer Is Not

Consistent with keeping this a lightweight texture layer rather than a second settlement-simulation system: a Gazetteer entry is not a place the player builds in, garrisons piece by piece, or owns — that's what the player's own Estate & Settlement growth track is for, and it stays entirely separate. A Gazetteer entry doesn't have its own economy, population count, or building list; it has a Role, a Tier, and enough identity to be a meaningful destination and a meaningful stake. If a future pass wants some Gazetteer entries to be capturable, ownable, or fought over directly, that's a deliberate escalation worth its own design conversation — not something this document quietly implies.

### 8.5 Persistence Across Playthroughs

Because Gazetteer entries are real historical places rather than procedurally generated ones, the same region's Gazetteer looks the same from playthrough to playthrough — Narbo Martius is always Narbo Martius. What varies between playthroughs is what's *happening* there: which rival gens (if any) is seated at a given entry per §9, what Diplomacy relationship currently anchors to a Frontier Outpost, what Events have recently touched the place. The location is fixed and real; the living world layered on top of it isn't.

---

## 9. Regional Rival Seeding

Per direction: a mix of a handful of **pre-filled, named rival gentes** per region, sketched briefly in that region's own document, plus **procedural generation** filling out the rest of the living competitive field — and the player can customize or disable either layer.

### 9.1 The Two Layers

- **Pre-filled rivals** — each region document names perhaps three to five rival houses with a name, a one- or two-line identity, a starting Rival Houses disposition (per that system's own Background/Note tiering), and, now that §8 gives the region real geography, a **Rival Seat** — the specific Gazetteer entry that house is described as based near or holding influence over. Seating a rival at Tarraco or Massilia rather than a generic "somewhere in the region" gives the player's very first read of the local political map the same concrete, CK3-style texture the Gazetteer itself now has, and gives Politics & Patronage and Espionage a real place to point at when they reference that house's home turf.
- **Procedural rivals** — Rival Houses' own existing generation system fills in the remainder of the region's competitive landscape, and continues generating/retiring houses across play exactly as that system already specifies. A procedurally generated house can also be seated at any Gazetteer entry not already claimed by a pre-filled rival, keeping the two layers visually consistent with each other rather than having only the hand-authored rivals feel like they live somewhere real.

### 9.2 Player Control

Consistent with §3.3's Full Custom override scope: a player can accept a region's pre-filled rivals as-is, swap them out for an equivalent number of freshly-procedurally-generated houses instead, adjust how many rival houses the region starts with, or disable Rival Houses generation for that region entirely for a quieter, lower-friction playthrough. None of this changes Rival Houses' own underlying system — it's purely a question of how that system gets seeded at the very start of a given playthrough.

---

## 10. Customization & Accessibility Options

Consistent with the Free Cities-derived philosophy in §3, and gathering the toggles referenced throughout this document into one place for a future settings/character-creation-screen implementation to read from:

- Region and Culture selected independently (§3.1).
- Starting Familia culture, Legal Status mix, and terrain roll overridable under Full Custom (§3.3).
- Regional Rival Seeding accepted, swapped for procedural, resized, or disabled (§9.2).
- Cheat/Easy Start selectable in any region at any time, per direction — it isn't gated behind region or difficulty context, since its entire purpose is offering a lower-friction entry point on demand. Per the core doc's own existing framing, using it disables achievement/milestone tracking for that playthrough, which remains the sole cost of choosing it.
- A future settings pass should consider whether individual regional hazard layers (e.g., a region's Reputation Duality, or its Diplomacy-with-neighbors exposure) can be independently toggled off for players who want a region's economic/cultural flavor without its more demanding political subsystems — flagged here as an open question (§13) rather than decided.

---

## 11. Cross-System Integration

- **Core Design:** supersedes the original four-region sketch in §4 of that document with the five-region launch roster in §5.1 above; the core doc's own Start Modes (§5) remain the actual owner of numeric starting-resource packages, per §4.2.
- **Buildings:** §3's terrain/feature gate table and regional-flavor weighting note are the authoritative source each region document's Terrain & Feature Profile (§4.1) reads from.
- **Resources & Goods:** §5/§7's region-tagged goods table is the authoritative source each region document's Regional Goods & Trade profile (§4.6) reads from; flagged for a future correction pass per §5.1's Iberian/North African split note.
- **Cultures of the Known World:** §12's culture-to-region quick reference is the authoritative source for every region's Population & Culture Distribution (§4.7) and for the Extensible Slate's own rationale (§5.2).
- **Politics & Patronage:** Reputation Duality (§2.1 of that document) is scoped region-by-region in §6 above rather than left as a single "principally Gallic" note.
- **Diplomacy with Non-Roman Peoples:** each frontier-adjacent region's default neighboring people (§4.4) is this document's own hook into that system's Per-People Standing and Frontier Relations Posture mechanics.
- **Education & Culture:** each region's starting Cultural Drift lean (§4.5) reads directly into that system's own existing Cultural Drift mechanic without this document inventing a parallel one.
- **Companions & Court Positions:** the Procurator role (§7.2) is this document's own mechanism for administering a distant second holding.
- **Travel:** the Regional Gazetteer (§8) gives that system real, named, discrete destinations to route journeys toward, each with a Home Anchor as the low-cost local default (§8.1) — this document's own contribution to that system rather than a replacement for its own travel-cost mechanics.
- **Politics & Patronage, Legal & Court, Religion, Military & Combat, Diplomacy with Non-Roman Peoples, Espionage, Piracy & Banditry:** each reads the Gazetteer's Role tags (§8.3) as the concrete venue for its own region-anchored actions — a Provincial Seat for cursus honorum and high rulings, a Sanctuary for Religion's favor-seeking, a Frontier Outpost for treaty negotiation and raid flavor, and so on.
- **Rival Houses:** Regional Rival Seeding (§9) is this document's own start-of-game hook into that system's ongoing generation and retirement mechanics, now with each seeded house pinned to a real Gazetteer entry rather than a generic regional presence.
- **Events:** a region's own historical conquest-arc timing (e.g., Britannia's Boudicca window, Iberia's Cantabrian Wars close) is a natural source of region-specific Divergence-eligible Events for a future pass.

---

## 12. Data Model

```
Region {
  regionId, name,                          // "italianHeartland" | "gallicFrontier" | "iberianColony" |
                                            // "northAfricanColony" | "greekEast" | (extensible slate ids)
  status,                                   // "launch" | "extensibleSlate"
  terrainProfileRef,                        // §4.1 — points into Buildings' gate table
  economicCharacterTag,                     // §4.2 — qualitative only; numeric package owned by Start Modes
  politicalLegalProfileRef,                 // §4.3
  diplomaticMilitaryProfileRef,             // §4.4 — includes defaultNeighboringPeopleId(s)
  religiousCulturalDefaultRef,              // §4.5 — includes startingCulturalDriftLean
  regionalGoodsProfileRef,                  // §4.6 — points into Resources & Goods
  cultureDistributionTable,                 // §4.7 — [{ cultureId, weight }], always includes an "outlier" residual weight
  reputationDualityMode,                    // "none" | "full" | "tapering" | "permanentStructural" | "localized" — §6
  homeAnchorLocationId,                     // §8.1 — points into this region's Gazetteer
  gazetteer: [ GazetteerLocation ],         // §8
}

GazetteerLocation {                         // §8.2 — real, historically-grounded places, not procedural
  locationId, regionId, name,               // e.g. "narboMartius", "gades", "ephesus"
  roles: [ ... ],                           // §8.3 — "provincialSeat" | "majorPort" | "legionaryBase" |
                                             // "sanctuary" | "marketHub" | "frontierOutpost"; can hold more than one
  prominenceTier,                           // "provincialSeat" | "regionalCenter" | "outpost" — light, non-numeric
  groundingNote,                            // short real-historical justification, Cultures-doc style
  rivalSeatHouseId,                         // nullable — §9.1, prefilled or procedural rival based here
}

RegionSelection {                           // per playthrough
  householdId, regionId,
  startModeId,
  startingFamiliaCultureId,                 // independently chosen — §3.1
  terrainRollOverride,                      // Full Custom only — §3.3
  rivalSeedingMode,                         // "prefilled" | "proceduralReplacement" | "disabled" — §9.2
  isCheatStart: bool,                       // disables achievement/milestone tracking
}

DistanceTier {                              // §7.1 — a lookup table, not a formula
  fromRegionId, toRegionId,
  tier,                                      // "near" | "moderate" | "far"
}

DistantHolding {                            // §7.2
  householdId, homeRegionId, holdingRegionId,
  distanceTier,
  procuratorCharacterId,                     // strongly recommended; absence flagged as a risk state
  mismanagementRiskActive: bool,              // true when Far and unstaffed or staffed with low-loyalty Procurator
}
```

---

## 13. Open Questions

- **All numeric sizing**, per this project's standing convention — Distance Tier cost multipliers, cultural-drift-lean starting strength, and every region's specific terrain/economic ranges are left to their own region documents and an eventual balancing pass.
- **Independent hazard-layer toggles.** §10 flags but doesn't resolve whether a player can keep a region's economic/cultural flavor while disabling its more demanding political subsystems (Reputation Duality, Diplomacy exposure) independently of full Cheat Start.
- **Distance Tier lookup table contents.** §7.1 establishes the Near/Moderate/Far concept but doesn't assign actual tiers to actual region pairs — reasonable to resolve once the Extensible Slate regions are formally scheduled, so the table doesn't need revisiting piecemeal.
- **Gazetteer entry count per region.** §8 doesn't fix how many entries a region's Gazetteer should carry — enough for real texture and enough Rival Seats and Roles to matter, but not so many that a region reads as a location-management task. Left for each region document to judge against its own real historical geography.
- **Whether any Gazetteer entry ever becomes ownable.** §8.4 deliberately keeps every entry a fixed, real place rather than a capturable asset; whether a future pass ever wants to let sustained Politics & Patronage or Military & Combat success let a household gain real influence *over* a Gazetteer entry (short of the player's own separate Estate & Settlement track) is an open, deliberately-unresolved escalation.
- **Home Anchor uniqueness under Full Custom.** §8.1 assigns one Home Anchor per region by default; whether a Full Custom start should let the player choose their own Home Anchor from the region's Gazetteer rather than accepting the region document's default isn't yet decided.
- **Iberian/North African goods-tagging cleanup.** §5.1 flags specific Resources & Goods entries (Esparto Grass, Cinnabar) that need re-tagging now that the two regions are split; left to that document's own next pass rather than edited here.
- **Multiple distant holdings simultaneously.** §7 assumes a household can hold one distant second holding; whether a third region, or a second Far holding, introduces compounding administrative risk beyond what one Procurator assignment already models isn't addressed.
- **Scenario Starts' own region defaults.** Core §5.4's Scenario Starts ("a newly settled veteran colony," etc.) likely imply specific regions per scenario; that mapping isn't drawn here and is natural to resolve once Scenario Starts gets its own dedicated pass.
- **Extensible Slate scheduling.** §5.2 originally ordered Egypt, Syria, and Britannia as the strongest candidates and Anatolia and the Balkans as further-out ones. All five have since been promoted to fully realized regions — Britannia, Egypt, Syria/the Levant, Anatolia/Asia Minor, and the Balkans — closing out every candidate this section ever named. Seven further regions have since been added beyond that original list entirely, closing out this project's current region-building series: **Sicily** (Rome's first province, 241 BC), **The Alpine Provinces** (Raetia & Noricum), **Armenia** (a Contested Buffer, never fully Roman territory), **Mesopotamia** (a real Roman province for two years by historical default, with one explicit exception to this project's own "real outcomes aren't rewritten" rule), **Nubia** (an Independent kingdom, never Roman territory at all), **Arabia Felix** (a real independent trade economy plus an explicit, separately-labeled alternate-history layer), and **The Bosporan Kingdom** (see *Starting Regions: The Bosporan Kingdom*) — a genuine fourth Rome-relationship shape, a permanent, unconverted Client kingdom, the only region on the roster to reach its own honest conclusion by reusing an existing Diplomacy mechanic rather than inventing a new one. The extensible slate remains open for future additions beyond these twelve, at whatever point this project returns to it.
