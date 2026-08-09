# GENS — System Design: Buildings & Production Chains (FINAL)
*A deep expansion of Estate & Settlement's building system (§6.2)*

---

## Contents

1. Purpose & Inspiration
2. Goods Taxonomy
3. Terrain & Feature Gating
4. Full Building List by Category
5. Showcase Chains
6. Tie-Ins to Existing Systems
7. Data Model
8. Full Building Index
9. Open Questions

---

## 1. Purpose & Inspiration

This document replaces the compact building table in the Estate & Settlement doc with a full production-economy model: raw materials feeding intermediate goods feeding finished and luxury goods, the way *Anno 117: Pax Romana* structures its supply chains — but built entirely from **authentic Roman industries** rather than borrowing that game's specific building roster. Where Estate & Settlement established that Agriculture/Industry chains produce named goods (§3.1 of that doc), this document is what actually fills that out: every raw material, every intermediate, every finished good, and the building that makes each conversion happen.

The organizing idea, same as Anno's: almost nothing is a single building anymore. Bread needs a Wheat Field *and* a Mill *and* a Bakery. A Toga needs a Flax Field, a Weaver, a Murex-hunting operation, a Dye Works, *and* a Tailor.

Not every building produces a good, though. This final pass adds a small number of buildings that generate income or effects purely as **services** — a Tavern, a Brothel, a Slave Market, a Banker's office — and §2.1 below gives that distinction its own name so the whole roster reads as one coherent model rather than a goods-chain system with a few odd exceptions bolted on.

---

## 2. Goods Taxonomy

Five tiers, raw→intermediate→finished→luxury plus a separate imported tier that never has a domestic production chain at all:

| Tier | Examples |
|---|---|
| **Raw Materials** | Grain (Wheat/Barley/Oats), Olives, Grapes, Flax, Lavender, Wool, Pork, Beef, Milk, Honeycomb, Fish, Salt, Murex Snails, Oysters, Clay, Timber, Quartz Sand, Limestone, Raw Marble, Iron Ore, Copper Ore, Tin Ore, Gold Ore, Silver Ore, Resin, Sandarac Wood, Woad/Dye Plants, Herbs, Horses |
| **Intermediate Goods** | Flour, Linen Fiber, Lavender Oil, Tallow, Leather, Wax, Woven Cloth, Tyrian Purple, Common Dye, Iron, Bronze, Refined Gold, Refined Silver, Glass, Concrete, Tile, Worked Marble, Charcoal, Malt |
| **Finished Goods** | Bread, Wine, Beer, Olive Oil, Cheese, Sausages, Tunics, Sandals, Pottery/Amphorae, Tools, Weapons, Armor, Furniture, Parchment, Medicine, Incense, Siege Engines |
| **Luxury Goods** | Perfume, Soap, Fine Glass, Jewelry, Purple-Trimmed Togas, Writing Tablets, Garum, Fine Seafood (Oysters) |
| **Imported Goods** *(never locally producible — see §4.9)* | Silk, Eastern Spices, Baltic Amber |

Luxury goods are deliberately the ones with the longest, most terrain-restricted chains — that scarcity is what makes them worth Dignitas, gifting, and high-value trade rather than just income. Imported goods take that a step further: no chain, no terrain, no amount of investment produces them.

### 2.1 Building Function Types

A lightweight tagging concept, not a rigid classification every building needs formally assigned — useful for reading the roster in §4 as one coherent system:

| Type | What it does | Examples |
|---|---|---|
| **Production** | Converts raw/intermediate goods into other goods | Grain Mill, Ironworks, Weaver's Loom |
| **Service** | Generates income or an effect without a tradeable good | Tavern, Brothel, Slave Market, Bathhouse, Argentaria |
| **Prestige** | Generates Dignitas primarily, no material output | Statue, Nymphaeum, Triumphal Arch |
| **Governance** | Unlocks or modifies political/legal/administrative mechanics | Curia, Basilica, Tabularium, Mint, Praetorium |
| **Housing** | Population capacity | Insulae, Domus |
| **Defense** | Security/risk reduction | Watchtower, City Walls, Lighthouse, Vigiles Post |

Most Civic buildings sit somewhere between Service and Prestige — a Library provides both a functional Education benefit and a Dignitas one, and that overlap is fine; the tagging is a reading aid, not a constraint on design.

---

## 3. Terrain & Feature Gating

| Gate | Restricts |
|---|---|
| **Coast** | Fishing, Salt Pans, Oyster Beds, Murex harvesting, Ports |
| **River** | Clay Pits, Grain Mills (water-powered), Glassworks |
| **Forest** | Timber Camps, Resin/Sandarac gathering |
| **Hills/Mountain + Deposit** | Iron, Copper, Tin, Gold, Silver, Marble, Stone |
| **Fertility** | Olive Groves, Vineyards, Flax Fields, Lavender Fields, Sandarac Groves, Herb Gardens |
| **Meadow** | Apiaries |

**Regional flavor, tied to the four starting regions:** Murex/Tyrian Purple production suits any coastal region but was historically prized from the Iberian and Greek coasts; Gold and Silver deposits fit the Iberian colony especially well; Tin suits the Gallic/frontier region; Lavender, fine olive/wine chains, and the Gymnasium favor the Italian heartland and Greek East; Beer favors the Gallic/frontier region as Wine's cultural counterpart. None of this is an exclusive lock — it's a bonus/availability weighting on top of the hard terrain gates above.

---

## 4. Full Building List by Category

### 4.1 Infrastructure & Building Materials

| Chain | Stages | Gate |
|---|---|---|
| Timber | Timber Camp → Sawmill (Timber) | Forest |
| Charcoal | Charcoal Kiln (Timber → Charcoal) | — *(feeds nearly every metal/glass/pottery chain below)* |
| Brick | Clay Pit → Brickworks (Tile) | River |
| Concrete | Quarry (Quartz Sand) + Limestone Quarry → Concrete Works (Concrete) | — |
| Marble | Marble Quarry → Marble Works (Worked Marble) | Hills, rare |
| Water | Aqueduct Source → Aqueduct → Cistern | Unlocks at Vicus stage; Cistern is the prerequisite for Forum/Baths/Nymphaeum (§4.10) |
| Roads/Bridges | *(unchanged from Estate & Settlement §3)* | — |

### 4.2 Agriculture — Staples

| Chain | Stages |
|---|---|
| Bread | Wheat Field → Grain Mill (Flour) → Bakery (Bread) |
| Beer | Barley Field → Malting House (Malt) → Brewery (Beer) — the frontier/Gallic counterpart to Wine (§4.3) |
| Legumes | Legume Field → *(consumed directly, no processing tier)* |

### 4.3 Agriculture — Cash & Luxury Crops

| Chain | Stages |
|---|---|
| Oil | Olive Grove → Olive Press (Olive Oil) |
| Wine | Vineyard (Grapes) + Apiary (Honeycomb) → Winery (Wine) |
| Linen | Flax Field → Linen Works (Linen Fiber) |
| Perfume oil | Lavender Field → Distillery (Lavender Oil) |
| Fine wood | Sandarac Grove → *(feeds Scriptorium, §4.6)* |
| Common dye | Woad/Dye Plant Field → Dyer's Workshop (Common Dye) — the everyday color ordinary Tunics and Cloth actually use, distinct from the coastal, luxury-tier Tyrian Purple (§4.8) |
| Medicine | Herb Garden → Apothecary (Medicine) — consumed by Disease & Public Health (§6.13) and a Court Physician (§6.20) rather than sold |

### 4.4 Livestock & Apiary

| Chain | Stages |
|---|---|
| Wool goods | Pasture (Sheep) → Fulling Works (Felt caps) *or* → Weaver's Loom (Wool Cloth, §4.6) |
| Pork products | Pig Pasture → Rendering House (Tallow); Pig Pasture + Salt → Tannery (Leather) |
| Beef & dairy | Cattle Pasture → Dairy (Cheese); Cattle Pasture + Salt → Tannery (Leather) |
| Honey | Apiary (Meadow) → Honeycomb *(feeds Wine and Writing Tablets)* |
| Horses | Horse Pasture → Stable (Horses) — feeds Military & Combat (cavalry), Games & Spectacle (chariot teams), and Travel (§6.18) directly |

### 4.5 Extraction & Metalworking

| Chain | Stages | Gate |
|---|---|---|
| Iron goods | Iron Mine + Charcoal → Ironworks (Iron) → Armory (Weapons/Armor) or Smithy (Tools) | Hills, Deposit |
| Bronze goods | Copper Mine + Tin Mine + Charcoal → Bronzeworks (Bronze) → Foundry (fine bronze goods, statuary) | Hills, Deposit |
| Precious metal | Gold Mine *or* Silver Mine + Charcoal → Smeltery (Refined Gold/Silver) → Goldsmith's Studio (Jewelry) | Hills, Deposit |
| Building stone | Stone Quarry → *(feeds Concrete and general construction)* | Hills |

### 4.6 Artisan & Luxury Manufacturing

| Chain | Stages |
|---|---|
| Textiles & garments | Weaver's Loom (Wool/Linen Cloth) → Tailoring House (Tunics); Murex harvest → Dye Works (Tyrian Purple) → Tailoring House (Purple-Trimmed Togas) |
| Leather goods | Tannery (Leather) → Cobbler's Workshop (Sandals) *or* → Leatherworks (armor padding, feeds Military) |
| Pottery | Clay Pit + Resin/Charcoal → Potter's Works (Pottery/Amphorae) |
| Glassware | Quartz Sand + Minerals → Glassworks (Glass) → Glassblower's Studio (Fine Glass) *(River)* |
| Perfume & soap | Distillery (Lavender Oil) + Rendering House (Tallow) → Soap Works (Soap); Lavender Oil + fine Olive Oil → Perfumery (Perfume) |
| Writing materials | Sandarac Grove + Apiary (Wax) → Scriptorium (Writing Tablets); *or*, without a Sandarac Grove, Tannery (Leather) → Parchment Works (Parchment) |
| Ritual goods | Resin → Incense Workshop (Incense) — consumed by Religion (§6.6) as ongoing upkeep for Shrines/Temples |
| Furniture | Sawmill (fine Timber) → Carpentry Workshop (Furniture) |

*Historical note: soap in this period was more a Gallic/Germanic import used as hair pomade than a bathing product — its in-fiction framing should lean into that "imported curiosity" angle; Romans bathed with oil and a strigil, which the Bathhouse already represents.*

### 4.7 Food, Provisioning & Sea Harvest

| Building | Produces | Gate |
|---|---|---|
| Bakery | Bread | — |
| Winery | Wine | — |
| Dairy | Cheese | — |
| Cured Meats Works | Sausages | — |
| Fishing Wharf | Fish | Coast |
| Salt Pans | Salt | Coast |
| Oyster Beds | Oysters | Coast |
| Garum Works *(Fishing Wharf + Salt Pans)* | Garum | Coast |

### 4.8 Commerce & Trade Services

The category grew this pass to include buildings that trade in people and services alongside goods — all still under the same Commerce umbrella, played with the same frankness Labor & Slavery established.

| Building | Notes |
|---|---|
| Market Stall → Market → Trading Post → Emporium | Standard goods trade |
| Storehouse → Warehouse → Warehouse Row | Storage/logistics |
| Port → Harbor → Grand Harbor | Enables sea trade and §4.9's Imported Goods |
| **Slave Market (Venalicium)** *(new, Vicus stage+)* | Gives Labor & Slavery's "Slave markets" acquisition method (that doc's §2) a physical, ownable building rather than an abstract off-map source — a rotating local stock the player can browse, subject to the same partial-information/deception rules (Labor & Slavery §3), and a venue to sell from directly rather than always requiring Travel. Generates Commerce income from transaction volume. Consistent with Labor & Slavery's stated tone, the institution itself carries no inherent Dignitas cost in this setting — but a market known for cruelty or deceptive dealing can become its own liability, the same way any business's reputation can |
| **Brothel (Lupanar)** *(new)* | A licensed commercial venue generating Service-type Commerce income. Consistent with §9's content note: nothing about this building is depicted or narrated beyond a name, an income figure, and its mechanical tradeoffs — it functions exactly like a Tavern in terms of what the player actually sees. Workers are typically enslaved or of low free status; Roman law marked such occupations with *infamia*, a social/legal stigma layered on top of (not replacing) the citizen/freedman/enslaved status already in Familia §2.5 — flagged in §9 as a cross-doc addition still to make. Generates a modest ongoing Disease risk (§6.13) and a small Dignitas cost for the owning gens *unless* operated at arm's length through a freedman or client manager (Companions & Court Positions, §6.20) rather than directly |
| **Argentaria (Banker's Office)** *(new)* | Formal lending and deposit services — the concrete building behind Labor & Slavery's Debt Bondage acquisition path (that doc's §2) and Economy & Finance's debt mechanics generally, and a natural venue for a Legal & Court dispute over a defaulted loan |

### 4.9 Imported Goods (Trade-Only)

A category with no production chain at all, deliberately. **Silk**, **Eastern Spices**, and **Baltic Amber** are never built or grown — they're purchased, and only through a built Port/Harbor or a sufficiently developed Emporium (§4.8), reflecting how these actually reached Roman elites: trade networks reaching far beyond anything a single estate could produce. Availability and price fluctuate with the state of those Commerce buildings and the wider trade situation (Piracy & Banditry, §6.24, is the natural threat to a shipment arriving) rather than anything the player can build around.

### 4.10 Civic & Public

Five sub-groups: governance, commerce/administration, entertainment/culture, population/infrastructure, and health/welfare.

**Governance & Legacy**

| Building | Notes |
|---|---|
| Shrine → Temple | Religion; Temple consumes Incense (§4.6) as ongoing upkeep |
| Forum *(unique)* | Requires Cistern; required for Town status |
| Basilica *(unique)* | Ties Legal & Court; required for City status |
| Curia *(unique)* | Unlocks holding and contesting local magistracies in Politics & Patronage (§6.5) |
| Tabularium *(unique)* | The Dynasty Chronicle's (§6.11) physical archive; where Legal & Court disputes, including Labor & Slavery's warranty claims, are formally filed |
| Nymphaeum | Requires Cistern — an ornamental water monument converting the Aqueduct investment into direct Dignitas |
| Mint/Moneta *(unique, City-stage, requires a Politics & Patronage milestone)* | Ongoing income/Dignitas bonus as a mark of political weight |
| Praetorium *(unique, City-stage, requires holding a provincial office)* | Seat of governorship; a strong Dignitas/political-capacity building |

**Commerce & Administration**

| Building | Notes |
|---|---|
| Macellum | Covered market for perishables/delicacies — gives Food & Provisioning's finer output (Fish, Cheese, Oysters, Sausages) a premium venue |
| Customs House/Portorium *(requires a Port)* | Tolls trade volume — the building behind future tariff decisions in Policies & Edicts (§6.12) |

**Entertainment & Culture**

| Building | Notes |
|---|---|
| School → Academy | Education & Culture |
| Amphitheater *(unique)* | Games & Spectacle — gladiatorial contests |
| Theatre *(unique)* | Drama/oratory, Education & Culture-leaning Dignitas |
| Circus *(unique, requires a Stable)* | Chariot racing, distinct from the Amphitheater's blood-sport focus |
| Library/Bibliotheca | Consumes Writing Tablets or Parchment as ongoing upkeep |
| Gymnasium/Palaestra | Education & Culture and Military & Combat dual benefit; regionally weighted to the Greek East |
| Odeon | Cheaper, earlier-accessible alternative to the Theatre |

**Population & Infrastructure**

| Building | Notes |
|---|---|
| Bathhouse → Grand Baths | Requires Cistern |
| Aqueduct Source → Aqueduct → Cistern | §4.1 |
| Insulae → Domus *(2-tier)* | The housing mechanism behind Settlement Demographics (§6.26): Insulae raise population capacity cheaply and densely; Domus house fewer people but raise average social standing and the Dignitas ceiling |
| Horreum | Civic-scale public granary — a famine reserve blunting Natural Disasters' worst harvest-failure outcomes for the whole settlement |
| Public Latrines/Fountains | The cheapest buildings in the document — small Health boosts meant to be built repeatedly as a settlement grows |
| Lighthouse *(coastal)* | Reduces Piracy & Banditry risk to sea trade; improves Port efficiency and sea Travel safety |
| Vigiles Post | Organized fire watch — the dedicated countermeasure to fire among Natural Disasters' hazard types |
| **Caupona/Taberna (Tavern)** *(new)* | The Commerce-facing consumption point for Bread, Wine, Beer, Cheese, and Sausages — where those finished goods actually get used up by an ordinary population rather than only ever being sold wholesale or gifted for Dignitas. Also Travel's natural lodging stop and a minor Settlement Demographics happiness contributor |

**Health & Welfare**

| Building | Notes |
|---|---|
| Valetudinarium | Treats the sick/injured, consuming the Apothecary's Medicine as ongoing input |
| Alimenta/Orphanage | Charitable support for children without means; gives orphaned or unsupported children a concrete place in the simulation |
| Necropolis | Public cemetery, distinct from the private Family Tomb (§4.11) |

### 4.11 Military

| Building | Notes |
|---|---|
| Watchtower | Basic estate security |
| Barracks → Garrison → Fortress | Standard military progression |
| Armory | Converts Iron/Bronze (§4.5) into the Weapons/Armor that equip recruited soldiers |
| Siege Workshop | Timber + Iron → Siege Engines, reserved for offensive campaigns |
| City Walls & Gates *(unique)* | A fortification tier, and a companion requirement alongside Basilica and the Aqueduct Cistern for true City status |
| Shipyard/Navalia *(coastal)* | Builds/repairs warships — an active naval countermeasure to Piracy & Banditry, complementing the Lighthouse's passive one |
| **Ludus (Gladiator School)** *(new)* | The training-and-housing facility for gladiators, mirroring the Barracks' role but for the combat-entertainer labor subtype Games & Spectacle (§6.22) and Labor & Slavery (§6.3) both reference — a slave, a condemned criminal, or (rarely, at real Dignitas cost) a free volunteer is turned into a gladiator here, feeding the Amphitheater's and Circus's need for participants the way the Stable feeds the Circus with horses |

### 4.12 Monuments

Statue → Grand Statue; Family Tomb; Dedicatory Temple; Triumphal Arch *(military-victory-specific, giving Military & Combat its own Monuments payoff)*.

---

## 5. Showcase Chains

**Purple-Trimmed Toga** *(4 stages, 2 converging sub-chains):* Flax Field → Linen Works → Weaver's Loom (Cloth) ⟶ *combines with* ⟶ Murex harvest → Dye Works (Tyrian Purple) ⟶ **Tailoring House → Toga**.

**Perfume** *(3 stages):* Lavender Field → Distillery (Lavender Oil) → **Perfumery → Perfume**.

**Jewelry** *(3 stages):* Gold Mine → Smeltery (Refined Gold) → **Goldsmith's Studio → Jewelry**.

**Garum** *(2 stages, 2 converging inputs):* Fishing Wharf (Fish) + Salt Pans (Salt) → **Garum Works → Garum**.

**Fine Glass** *(4 stages):* Quarry (Quartz Sand) + Mineral Quarry → Glassworks (Glass) → **Glassblower's Studio → Fine Glass**.

**Weapons & Armor** *(3 stages):* Iron Mine + Charcoal Kiln → Ironworks (Iron) → **Armory → Weapons/Armor**.

**Writing Tablets** *(3 stages, 2 converging inputs):* Sandarac Grove + Apiary (Wax) → **Scriptorium → Writing Tablets**. *(Parchment, via Tannery → Parchment Works, is the 2-stage alternative.)*

**Cavalry Mount** *(2 stages):* Horse Pasture → **Stable → Horses**, feeding Military, Games & Spectacle, and Travel simultaneously — the shortest production chain in the document.

**Medicine** *(2 stages):* Herb Garden → **Apothecary → Medicine**, consumed by Disease & Public Health rather than sold.

**Service buildings have no chain at all**, by design — a Slave Market, Brothel, Tavern, or Argentaria is a single building generating income or an effect directly, the Service-type counterpart to a production chain's depth.

---

## 6. Tie-Ins to Existing Systems

- **Economic Identity (Estate & Settlement §6):** Agrarian leans §4.2–4.4; Industrial leans §4.5–4.6; Mercantile leans §4.8/§4.9 plus whichever luxury chain a region supports; Martial leans §4.5's metal chains feeding §4.11's Armory, Siege Workshop, and Ludus.
- **Games & Spectacle (§6.22):** the Amphitheater and Circus are the venues; the Stable and Ludus are what actually supply them with horses and gladiators respectively.
- **Education & Culture (§6.14) and Legal & Court (§6.16):** both draw on Writing Tablets or Parchment; the Tabularium is where Legal records physically live.
- **Disease & Public Health (§6.13):** the Apothecary produces Medicine, the Valetudinarium consumes it to actually treat people, and the Brothel is a modest ongoing risk source — a full loop rather than a single good.
- **Religion (§6.6):** the Incense Workshop ties Shrines/Temples to the wider economy.
- **Military & Combat (§6.7):** Horses, Siege Engines, City Walls, the Shipyard, and the Ludus give a Martial-identity estate a genuinely broad set of buildings to work toward.
- **Labor & Slavery (§6.3):** the Slave Market gives that system's market-acquisition method a physical building; the Argentaria does the same for Debt Bondage; the Brothel and Ludus both supply the specific labor subtypes those systems reference.
- **Familia (§6.1):** the Brothel's *infamia* note is flagged in §9 as a needed cross-doc addition to the Legal Status system.
- **Dignitas & gifting:** Luxury and Imported goods remain the natural currency for patronage gifts and marriage-negotiation sweeteners (Politics & Patronage §6.5, Familia's marriage market).
- **Politics & Patronage (§6.5):** the Curia unlocks local office-holding; the Mint and Praetorium both require a political milestone to build at all.
- **Settlement Demographics (§6.26):** Insulae/Domus are the housing mechanism; the Tavern is the consumption point for everyday finished goods; Horreum is the famine-resilience building.
- **Piracy & Banditry (§6.24) and Travel (§6.18):** Lighthouse (passive) and Shipyard (active) cover sea risk; the Tavern is Travel's lodging stop.
- **Natural Disasters (§6.17):** Vigiles Post (fire), Lighthouse (sea), Watchtower (land) — every major hazard type has a dedicated countermeasure building.
- **Succession & Dynasty (§6.9):** the Alimenta/Orphanage gives orphaned children a concrete place in the simulation.

---

## 7. Data Model

```
goods: {
  raw: { grain, olives, grapes, flax, lavender, wool, pork, beef, milk, honeycomb,
         fish, salt, murex, oysters, clay, timber, quartzSand, limestone,
         rawMarble, ironOre, copperOre, tinOre, goldOre, silverOre, resin,
         sandaracWood, woad, herbs, horses },
  intermediate: { flour, linenFiber, lavenderOil, tallow, leather, wax, cloth,
                  tyrianPurple, commonDye, iron, bronze, refinedGold, refinedSilver,
                  glass, concrete, tile, workedMarble, charcoal, malt },
  finished: { bread, wine, beer, oliveOil, cheese, sausages, tunics, sandals, pottery,
              tools, weapons, armor, furniture, parchment, medicine, incense, siegeEngines },
  luxury: { perfume, soap, fineGlass, jewelry, togas, writingTablets, garum, fineSeafood },
  imported: { silk, spices, amber }   // §4.9 — no production chain
}

// Service buildings (§4.8, §4.10) don't add to `goods` — they generate income/effects directly:
serviceBuilding: {
  key, type,              // "slaveMarket" | "brothel" | "tavern" | "argentaria" | "bathhouse" | ...
  monthlyIncome,
  effects: { dignitas, disease, happiness, ... }   // whichever apply to that building
}
```

---

## 8. Full Building Index

*(Alphabetical, each pointing to its home section. Chains list only the final/named building; see §4 for full stage sequences.)*

Academy §4.10 · Alimenta/Orphanage §4.10 · Amphitheater §4.10 · Apothecary §4.3 · Aqueduct §4.1 · Argentaria §4.8 · Armory §4.11 · Bakery §4.7 · Barracks §4.11 · Basilica §4.10 · Bathhouse §4.10 · Brewery §4.2 · Brickworks §4.1 · Bridge §4.1 · Brothel §4.8 · Carpentry Workshop §4.6 · Charcoal Kiln §4.1 · Circus §4.10 · Cistern §4.1 · City Walls & Gates §4.11 · Cobbler's Workshop §4.6 · Concrete Works §4.1 · Curia §4.10 · Customs House/Portorium §4.10 · Dairy §4.7 · Distillery §4.3 · Dye Works §4.6 · Dyer's Workshop §4.3 · Emporium §4.8 · Fishing Wharf §4.7 · Fortress §4.11 · Foundry §4.5 · Fulling Works §4.4 · Garrison §4.11 · Garum Works §4.7 · Glassblower's Studio §4.6 · Glassworks §4.6 · Goldsmith's Studio §4.5 · Grain Mill §4.2 · Grand Baths §4.10 · Grand Statue §4.12 · Gymnasium/Palaestra §4.10 · Harbor §4.8 · Herb Garden §4.3 · Horreum §4.10 · Incense Workshop §4.6 · Insulae/Domus §4.10 · Ironworks §4.5 · Leatherworks §4.6 · Library/Bibliotheca §4.10 · Lighthouse §4.10 · Linen Works §4.3 · Ludus (Gladiator School) §4.11 · Macellum §4.10 · Malting House §4.2 · Marble Works §4.1 · Market Stall/Market §4.8 · Mint/Moneta §4.10 · Necropolis §4.10 · Nymphaeum §4.10 · Odeon §4.10 · Olive Press §4.3 · Oyster Beds §4.7 · Parchment Works §4.6 · Perfumery §4.6 · Port §4.8 · Potter's Works §4.6 · Praetorium §4.10 · Public Latrines/Fountains §4.10 · Rendering House §4.4 · Sawmill §4.1 · School §4.10 · Scriptorium §4.6 · Shipyard/Navalia §4.11 · Shrine/Temple §4.10 · Siege Workshop §4.11 · Slave Market (Venalicium) §4.8 · Smeltery §4.5 · Smithy §4.5 · Soap Works §4.6 · Stable §4.4 · Storehouse/Warehouse §4.8 · Tabularium §4.10 · Tailoring House §4.6 · Tannery §4.4 · Tavern/Caupona §4.10 · Theatre §4.10 · Timber Camp §4.1 · Trading Post §4.8 · Triumphal Arch §4.12 · Valetudinarium §4.10 · Vigiles Post §4.10 · Watchtower §4.11 · Weaver's Loom §4.6 · Winery §4.3/§4.7

---

## 9. Open Questions

- **Consumption/demand layer.** This document covers production and now some consumption (Tavern) — whether a fuller demand system exists at the population level is still tied to Settlement Demographics (§6.26).
- **Chain balancing.** Processing-time/ratio balancing isn't attempted here — this document establishes *what* the chains are, not their throughput math.
- **Regional exclusivity vs. weighting.** Treated as soft weighting throughout; whether any chain should be a harder regional exclusive is still open.
- **New buildings' population/Dignitas/income numbers.** Most buildings added across this document's several passes are scoped narratively but not numerically costed or valued yet — a dedicated balancing pass will be needed before implementation.
- **Imported goods pricing/availability model.** Depends on Port/Emporium development and the wider trade situation, but no formula yet.
- **Medicine's and Incense's consumption rates.** Both established as ongoing consumables; neither has an actual monthly rate yet.
- **Wine vs. Beer regional preference strength.** Cultural split established; whether it carries an actual Dignitas/happiness modifier isn't specified.
- **Mint's and Praetorium's political-grant triggers.** Both require "a Politics & Patronage milestone" in principle; neither milestone is named yet.
- **Insulae/Domus population numbers.** Depend on Settlement Demographics being designed.
- **Curia's relationship to existing Politics mechanics.** Politics & Patronage (§6.5) predates this requirement — should be revisited to confirm local office-holding is genuinely gated on this building.
- **City status prerequisite list.** Basilica + Aqueduct/Cistern + City Walls have accumulated as City requirements across separate passes — worth confirming in Estate & Settlement §5 that all three together is the intended bar.
- **Valetudinarium treatment capacity.** Not yet sized relative to settlement population.
- **Brothel's *infamia* status.** Flagged directly: Familia's Legal Status system (§6.1 §2.5) should get a social-stigma flag layered on top of citizen/freedman/enslaved status to properly represent this and other historically *infamis* occupations (performers, gladiators) — not yet added to that document.
- **Slave Market and Brothel Dignitas tuning.** Both note a modest Dignitas effect (Slave Market only if poorly run; Brothel as a baseline unless managed at arm's length) — neither has an actual number yet.

*An inland Salt Mine was considered during an earlier pass and set aside — coastal Salt Pans remain the only Salt source for now.*
