# GENS — System Design: Estate & Settlement (§6.2)

---

## 1. Scope & Role

Estate & Settlement is the physical growth engine: the land the player holds, what's built on it, and how a single villa can eventually become a *vicus*, a town, or a city. It supplies the labor demand that Familia and Labor & Slavery fill, the income Economy & Finance runs on, the plots Monuments & Legacy Building occupies, the population Settlement Demographics tracks once things get big enough, and the physical target Natural Disasters tests.

---

## 2. Land & Map

The map is a **fuller geographic model** rather than an abstract slot grid: real terrain features (rivers, hills, coastline, forest, plains) determine what can be built where, but it's **presented** the way *Free Cities'* arcology display works — a clean, modular, diagrammatic view rather than a fully naturalistic map. The player reads terrain and plots as legible shapes and icons on the mosaic-tile map established in the visual design doc, not as a simulated 3D landscape.

**Terrain types** and what they favor:

| Terrain | Favors |
|---|---|
| Fertile Plain | Fields, orchards — baseline agriculture |
| Hills | Quarries, mines (region-gated — see §6) |
| Forest | Timber/lumber-dependent Industry buildings |
| Coast | Ports, fishing, trade-route Commerce bonuses |
| River | Mills, irrigation bonus to adjacent Agriculture, a natural trade route |
| Marsh/Poor land | Cheap to acquire, low base yield, elevated Disease risk (§6.13) |

Each plot has a terrain type fixed at map generation; buildings are gated to plots whose terrain supports them (a mine cannot be dug on a plain).

---

## 3. Building Categories & Chains

Six categories. Depth here comes from genuinely long chains where the category calls for it (Agriculture, Industry, Commerce all run three to four tiers) — Civic, Military, and Monuments stay shorter, since those are more naturally singular, settlement-defining structures than production lines. Each production chain now names the actual **good** it produces rather than an abstract "yield" number, so Economy & Finance and Commerce have real commodities to move rather than a single undifferentiated income figure.

| Category | Chain | Tiers | Produces |
|---|---|---|---|
| **Agriculture** | Field → Terraced Field → Latifundium → Imperial Estate | 4 | Grain |
| | Olive Grove → Managed Grove → Oil Works → *Oleum* Exportarium | 4 | Oil |
| | Vineyard → Trellised Vineyard → Winery → Vintner's Estate | 4 | Wine |
| | Pasture → Managed Herd → Stockyard | 3 | Livestock / Wool |
| | Granary → Grand Granary → Imperial Granary | 3 | *(storage capacity, not a good)* |
| **Industry** | Workshop → Artisan Quarter → Kiln Complex → Manufactory | 4 | Craft Goods |
| | Quarry → Deep Quarry → Mine Concession → Deep Mine *(hills-only)* | 4 | Stone / Ore |
| | Fullery → Textile Works → Weaving Hall | 3 | Textiles |
| | Pottery Works → Kiln Yard → Ceramic Works | 3 | Pottery |
| | Timber Camp → Sawmill *(forest-only)* | 2 | Timber |
| **Commerce** | Market Stall → Market → Trading Post → Emporium | 4 | *(converts stored goods to income)* |
| | Storehouse → Warehouse → Warehouse Row | 3 | *(storage/logistics capacity)* |
| | Port → Harbor → Grand Harbor *(coast-only)* | 3 | *(enables sea trade)* |
| **Civic** *(public — see §3.1)* | Shrine → Temple | 2 | — |
| | Bathhouse → Grand Baths | 2 | — |
| | Aqueduct *(unlocks at vicus stage)* | 1 | — |
| | Forum *(unique, required for Town status — see §5)* | 1 | — |
| | Basilica *(unique, ties Legal & Court, required for City status)* | 1 | — |
| | School → Academy | 2 | — |
| | Amphitheater *(ties Games & Spectacle)* | 1 | — |
| **Military** | Watchtower | 1 | — |
| | Barracks → Garrison → Fortress | 3 | — |
| **Monuments** *(§6.23, prestige-only, no yield)* | Statue → Grand Statue | 2 | — |
| | Family Tomb | 1 | — |
| | Dedicatory Temple | 1 | — |
| **Infrastructure** | Road (connects plots, boosts Commerce/Travel efficiency) | 1 | — |
| | Bridge *(river plots only)* | 1 | — |

Forum and Basilica remain singular, settlement-defining buildings rather than repeatable chains — building one is itself a milestone action tied to §5, not a yield choice like a fourth field would be.

### 3.1 Goods, Trade & Public vs. Private Buildings

Agriculture and Industry chains each produce a **named commodity** (Grain, Oil, Wine, Wool, Craft Goods, Stone/Ore, Textiles, Pottery, Timber) rather than generic yield. Those goods accumulate in Granary/Warehouse storage and are converted to actual denarii income by Commerce buildings (a Market or Emporium sells stored goods; a Port/Harbor enables selling into wider sea trade) — this is the concrete link into Economy & Finance's income line, and it's also what makes Economic Identity (§6) legible: an Agrarian estate is one whose storage is full of Grain, Oil, and Wine; an Industrial one is full of Stone, Textiles, and Craft Goods.

**Public vs. private** is a clean split along category lines: **Civic** buildings (Shrine/Temple, Bathhouse, Aqueduct, Forum, Basilica, School, Amphitheater) are public — once Settlement Demographics (§6.26) is tracking a real population beyond the household, their benefits (Disease reduction, Dignitas, Education access, religious favor, games capacity) extend to that whole population, not just Familia's roster. Every other category — Agriculture, Industry, Commerce, Military, Monuments — is private: its output and upkeep belong to the player's own household economy, regardless of how many outside laborers a large operation might employ.

---

## 4. Construction Mechanics

Cost and time scale with the building, consistent with your steer:

- **Small/cheap buildings** (a Field, a Shrine, a Market Stall) are effectively instant to place, but carry a short **activation delay** (a partial month) before producing at full output — nothing comes fully online the instant coin changes hands.
- **Larger buildings** (a Temple, an Aqueduct, a Basilica, most Tier-2 upgrades) take **one to several months**, and construction isn't a single upfront payment: it draws ongoing money and **assigned labor** (diverting workers from their regular duties, per Familia §4 and the *vilicus* concept from Labor & Slavery §4) for the duration. A large project left under-resourced simply takes longer rather than failing outright.
- **Upkeep & decay:** every completed building carries a small ongoing upkeep cost (folded into Economy & Finance's monthly expense total). Neglect — or unrepaired disaster damage from §6.17 — degrades a building's output over time until a deliberate **Repair** action restores it; buildings don't spontaneously collapse absent an actual disaster event, but a long-neglected one becomes a liability rather than staying inert.

**Demolition & repurposing.** Any building can be deliberately demolished to reclaim its plot for something else — a partial cost (labor-time and a modest fee, well below the original construction cost) rather than free, and no refund of the resources already spent building it. **Monuments are the one exception worth flagging explicitly**: demolishing a Statue, Family Tomb, or Dedicatory Temple carries a real Dignitas penalty on top of the usual cost, since tearing down something built for legacy is a visible, remarked-upon act rather than routine estate management.

---

## 5. Settlement Growth Stages

Four stages — **Villa → Vicus → Town → City** — each requiring **both** a population threshold (read from Settlement Demographics, §6.26, once that system is active) **and** specific civic construction:

| Stage | Population (approx.) | Required construction |
|---|---|---|
| Villa | Starting state | — |
| Vicus | A modest threshold | At least one Market Stall/Trading Post and one Shrine/Temple |
| Town | A larger threshold | A completed **Forum** |
| City | A substantial threshold | A completed **Basilica**, plus an Aqueduct |

Reaching both thresholds together doesn't auto-advance the stage silently — it *unlocks* the stage transition as a deliberate action (consistent with wanting growth to feel like an achievement, not a background counter ticking over), which is also a natural Chronicle entry and a Dignitas moment.

---

## 6. Economic Identity & Region

**Specialization is real but never mandatory.** Committing heavily to one identity — Agrarian, Mercantile, Industrial, or Martial — grants compounding synergy bonuses (e.g., a majority-agricultural estate gets a growing yield bonus the more Agriculture buildings it holds relative to other categories). Diversifying forgoes that compounding bonus but carries no penalty of its own — it's simply the steadier, lower-ceiling path, never a trap.

**Region** (from the original setting choice — Italian heartland, Gallic frontier, Iberian colony, Greek East) affects Estate & Settlement in two ways at once, per your call:
- **Fixed bonuses/penalties** — e.g., frontier land is cheaper to acquire but starts with a higher Disaster/Piracy exposure; the Greek East gets an Education & Culture-adjacent bonus to School buildings.
- **Gated availability** — some chains simply don't exist without the right region/terrain combination (Mine Concession needs Hills terrain *and* isn't available at all in a region without mineral wealth; a Port-flavored Commerce building needs a Coast region).

---

## 7. Land Acquisition & Expansion

All four methods you approved, plus room left open for more as later systems suggest them:

- **Buy outright** — the default, denarii-for-land baseline method (as in the prototype's "Found New Outpost").
- **Land grants** — awarded through Politics & Patronage (§6.5) as a reward for service, favor, or standing, at little or no direct cost.
- **Conquest/seizure** — an output of Military & Combat (§6.7) campaigns, similar in spirit to how war captives arrive in Labor & Slavery.
- **Marriage dowry/inheritance** — land arriving as part of Familia's marriage or succession mechanics, same principle as an inherited slave in §6.3.
- *(Left open, consistent with "any other methods you can think of": a colonial veteran land grant at game start per the Scenario Start option, and a foreclosure/legal seizure of a defaulting debtor's land via Legal & Court, mirroring §6.3's own legal-seizure acquisition path.)*

**Contested land:** most expansion is simply player-paced, but **certain high-value plots** (a notably fertile field, a coastal or river-adjacent parcel, anything a rival house would also want) can be actively contested — a rival gens can outbid or petition for the same plot, turning that specific acquisition into a race or a negotiation rather than a guaranteed purchase. This is deliberately occasional, not the default friction on every expansion.

**Multiple settlements:** a single, contiguous, growing estate remains the primary and default shape of play. At sufficient scale, however, a **second settlement** becomes possible — most naturally through the takeover of a rival house's holdings (via marriage absorption, a legal ruling, conquest, or a rival's extinction leaving their land unclaimed) rather than simply planting a second outpost from scratch. This stays a late-game possibility rather than a core early loop.

---

## 8. Data Model Sketch

```
Plot {
  id, terrain, region,
  building: { category, key, tier, produces, public: bool, constructionMonthsRemaining, laborAssigned, condition },
  contested: bool
}

Settlement {
  stage,                     // Villa / Vicus / Town / City
  plots: [Plot, ...],
  goods: { grain, oil, wine, wool, craftGoods, stone, textiles, pottery, timber },  // §3.1 storage totals
  economicIdentity: { agrarian, mercantile, industrial, martial },  // relative weights driving §6's bonus
  region
}
```

---

## 9. Open Questions Carried Forward

- **Specialization bonus curve.** §6 establishes that specialization compounds, but not the actual formula relating category concentration to the bonus magnitude.
- **Contested-plot resolution.** §7 establishes that certain plots can be contested, but not the actual mechanism (a bidding war, a Politics & Patronage check, a timed race) for resolving one.
- **Stage-transition population numbers.** §5's thresholds are described qualitatively; actual numbers depend on Settlement Demographics (§6.26) being designed first.
- **Repair action cost/time.** §4 establishes decay and a Repair action exist, but not their cost or duration relative to original construction.
- **Second-settlement management model.** §7 flags a second settlement as a late-game possibility but doesn't yet specify whether it's independently managed, run by an appointed steward (Court Positions, §6.20), or merged into a unified view.
- **Goods price/conversion formula.** §3.1 establishes that Commerce buildings convert stored goods to income, but not the actual per-good price or how a Market vs. an Emporium vs. a Port differ in conversion efficiency.
- **Demolition cost formula.** §4 establishes demolition is possible and partially costly, but not the exact fraction of original construction cost it recovers or requires.

*Soil fertility/depletion, seasonal yield variance, a formalized irrigation bonus, formalized road/river/coast trade-proximity bonuses, building-to-building adjacency bonuses, and ordinary (non-Monument) buildings contributing Dignitas were all considered during this polish pass and set aside — not currently part of the scope.*
