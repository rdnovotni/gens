# GENS — System Design: Resources & Goods (Finalized)
*The complete, authoritative registry. Supersedes the Buildings doc's §2 goods taxonomy and Estate & Settlement's §8 simplified storage list in full. This closing pass adds a livestock tracked-stock system and 28 further goods spanning four new buildings — the largest single expansion this document has seen, and explicitly its last planned one before implementation.*

---

## Contents

1. Scope & Role
2. The Automation Principle
3. Livestock as Tracked Stock
4. New Buildings This Pass
5. Regional Goods & Historical Specialties
6. Every Addition Beyond the Original Buildings Doc Taxonomy
7. The Unified Goods Registry
8. Storage & Capacity
9. Spoilage & Preservation
10. Quality Tiers
11. Currency & Pricing
12. Market Dynamics & Trade
13. Consumption
14. Gifting & Diplomatic Value
15. Cross-System Integration
16. Data Model
17. Open Questions

---

## 1. Scope & Role

The Buildings doc's §2 defines a full 5-tier goods taxonomy. Estate & Settlement's §8 data model lists only eight simplified goods — an inconsistency this document resolves by being the **single authoritative registry**: every good, its storage category, its perishability, its Quality eligibility, and now its livestock status, all in one place. Estate & Settlement's own `goods` object should be read as pointing here.

This pass is the largest yet: a proper livestock stock system, four new buildings (more than every previous pass combined), and 28 additional goods. It's also intended to be the last major expansion before this document is treated as implementation-ready — future passes should be corrections and numeric balancing, not new categories.

**One deliberate exclusion, unchanged:** enslaved people are never modeled as a good here. A person has a full Familia stat block; a good has a price and a storage tier. The two systems don't merge.

---

## 2. The Automation Principle

Every good's full lifecycle — production, storage, spoilage or aging, consumption, overflow sale, and market pricing — runs automatically every month-tick, off standing decisions the player has already made: building/land investment (Estate & Settlement), Overseer and Senior Position appointments (Companions & Court Positions), Regimen and Policy settings (Labor & Slavery, Policies & Edicts), and storage/Commerce investment. The player's real point of contact is the Monthly Report, plus occasional deliberate intervention. This pass extends the same philosophy to livestock (§3): a herd breeds, is culled, and is lost to disaster or raiding automatically, off a standing Policy the player sets once, not a per-animal decision.

### 2.1 A Forward-Looking Note: Population & Workforce

Goods and livestock automation are still only part of "the population runs itself." Workforce allocation across sectors, migration, and *Victoria II/III*-style class-stratified population dynamics belong to **Settlement Demographics (§6.26)**, not yet designed. This document's consumption hooks (§13.1) and livestock model (§3) are foundations that system should build on, not replace.

---

## 3. Livestock as Tracked Stock

Every Pasture-type building has, until now, simply emitted a flat per-tick amount of Wool, Milk, or Meat with no actual herd behind it — no population of animals, the way there's about to be a population of people. This section gives livestock the same lightweight treatment Exotic Beasts already got (§6): a real headcount that breeds, is culled, and can be lost, without demanding Familia-level depth for individual animals.

### 3.1 The Eight Tracked Types

| Type | Home Building | Role | Renewable Yield (doesn't reduce headcount) | Culled Yield (does) |
|---|---|---|---|---|
| **Horses** | Horse Pasture → Stable | Status/cavalry — Military & Combat, Games & Spectacle chariots, Travel, personal status | — | Rarely culled; sold or gifted live, not slaughtered for goods |
| **Oxen** *(new)* | Cattle Pasture, draft-allocated | Heavy plowing/hauling — an Estate & Settlement construction-speed and Agriculture-efficiency bonus, not a tradeable good in itself | Labor capacity | Rarely culled |
| **Mules/Donkeys** *(new)* | Horse Pasture → Stable, logistics-allocated | Trade caravan capacity; Travel and Correspondence & Letters courier reliability | Hauling capacity | Rarely culled |
| **Cattle** | Cattle Pasture, dairy/beef-allocated | Beef, Milk, Raw Hides, Tallow | Milk | Beef, Raw Hides, Tallow |
| **Sheep** | Pasture | Wool, Mutton | Wool | Mutton, Raw Hides |
| **Goats** *(new)* | Goat Pasture, Marsh/Poor-land-gated | Goat Meat, Milk, Raw Hides — the hardy option for marginal terrain | Milk | Goat Meat, Raw Hides |
| **Pigs** | Pig Pasture | Pork, Lard, Raw Hides | — | Pork, Lard, Raw Hides |
| **Poultry** | Poultry Yard | Eggs, Feathers, Poultry meat | Eggs, Feathers | Poultry |

**Mutton is specifically Sheep's meat; Goats produce their own separate Goat Meat.** These are commonly conflated in casual usage, so it's worth being precise in the registry even though flavor text can be looser.

### 3.2 Mechanics

Each Pasture-type building carries a **headcount**, not a flat production rate:

- **Growth** is driven by the assigned Vilicus's Stewardship (the Vilicus already covers "Fields, Groves, Vineyards, Pastures, Latifundium-tier estates" per Companions & Court Positions §4.2 — no new Overseer position is needed) plus a standing **Herd Strategy** Policy setting: *Growth-Focused* (slower current yield, faster headcount recovery), *Balanced*, or *Yield-Maximizing* (faster current output, slower recovery, higher vulnerability to a bad season). This is the livestock equivalent of the Regimen system — a standing choice, not a per-tick decision.
- **Culling** automatically draws down headcount to produce the culled-yield goods in §3.1, balanced against growth, per the active Herd Strategy.
- **Purpose allocation** (Cattle → dairy/beef vs. draft/Oxen; Horse Pasture/Stable → cavalry/status vs. logistics/Mules) is a standing choice at the building level, not re-decided each tick.
- **Vulnerability:** livestock disease is a new touchpoint for Disease & Public Health (§6.13) distinct from human illness; Natural Disasters can wipe out a herd the same way they damage a building; and Piracy & Banditry (§6.24) gains a new raid target — livestock rustling, distinct from raiding goods or people.

### 3.3 What This Isn't

Livestock stock does **not** use the Quality (Common/Fine/Exceptional) system — an aggregate herd doesn't have individual craftsmanship the way a bolt of Cloth does. It's a wholly separate tracking dimension (headcount, growth, mortality) layered under the existing goods the herd produces, which keep their own normal Quality eligibility where applicable (Wool remains Quality-eligible; the herd itself never does).

---

## 4. New Buildings This Pass

Every previous pass folded new goods into existing buildings. This one couldn't, entirely — four new buildings were genuinely necessary given the scope, and are flagged here as a deliberate, one-time exception to that pattern rather than a change in approach going forward.

| Building | Gate | Produces | Why It's Necessary |
|---|---|---|---|
| **Orchard** | Fertility | Orchard Fruit + Nuts (dual output) | No fruit or nut category existed at all beyond Grapes and Olives — a genuine gap, not a refinement. |
| **Garden Plot** | Fertility, cheap | Garden Produce | A basic subsistence-tier building alongside the existing Legume Field; onions, garlic, and leeks are bundled into one generic good rather than tracked separately. |
| **Reed Bed** | Marsh | Reeds | Gives Marsh terrain its *first* dedicated building — previously that terrain type produced nothing at all. |
| **Goat Pasture** | Marsh/Poor land | Goats | The second Marsh/Poor-land building. Between this and Reed Bed, a terrain type that was previously just "cheap and bad" now has two genuine specialties nowhere else can easily match. |

Everything else this pass — all 24 remaining new goods — folds into a building that already existed, consistent with every prior pass.

---

## 5. Regional Goods & Historical Specialties

*(Unchanged from the previous finalized pass — Italian heartland's Pozzolana and Truffles, Gallic Furs/Pelts and Butter, Iberian Esparto Grass and Numidian marble, Greek East/border Silphium and Saffron, and Egypt's forward-looking Natron/Faience/Alabaster. See §6 and §7 for how this pass's new goods layer additional regional weighting on top — Preserved Meat joins Beer and Butter as a third Gallic food identity, and Reed Bed/Goat Pasture give Marsh/Poor-land terrain its own identity independent of any single region.)*

---

## 6. Every Addition Beyond the Original Buildings Doc Taxonomy

The complete list, all passes consolidated. New this pass are marked; everything else was previously finalized and appears here only for completeness of the registry's provenance.

### 6.1 Raw Materials

| Good | Integrates Into | Reasoning |
|---|---|---|
| Lead Ore → Lead | Ironworks | Real metal missing from the extraction chain. |
| Hemp → Rope/Cordage | Linen Works | Shipyard had no listed inputs. |
| Sea Sponges | Balneum/Bathhouse | Bathing chain had no goods dependency. |
| Cinnabar → Pigments | Dye Works | Gives Fresco Styles material backing. |
| Manure | Consumed on-site by Agriculture | Livestock/Agriculture synergy. |
| Sinew | Siege Workshop | Authentic torsion-engine material. |
| Pozzolana | Concrete Works | Real basis of Roman concrete's durability. |
| Esparto Grass | Linen Works, Cobbler's Workshop | Real Iberian fiber source. |
| Silphium | Special Herb Garden variant | The design's one permanently depletable good. |
| Saffron | Perfumery, Apothecary, Dye Works | Prized ancient luxury touching three buildings. |
| **Orchard Fruit** *(new)* | Orchard (new building) | Closes the "no fruit category" gap. |
| **Nuts** *(new)* | Orchard (second output) | Free once Orchard exists. |
| **Garden Produce** *(new)* | Garden Plot (new building) | Real everyday staple, bundled as one good. |
| **Raw Hides/Skins** *(new)* | Byproduct of any culled livestock, feeds Tannery | Makes the existing Pork/Beef→Tannery chain explicit rather than implicit. |
| **Reeds** *(new)* | Reed Bed (new building) | Gives Marsh terrain its first good. |
| **Building Stone** *(new)* | Named output of the existing generic Stone Quarry | Bulk construction stone distinct from decorative Marble — a naming fix as much as an addition. |
| **Gypsum** *(new)* | Alternate output of Limestone Quarry | Feeds Mortar/Plaster. |
| **Coral** *(new)* | Rare secondary yield of Oyster Beds | Real Mediterranean harvest good with a genuine Familia flavor hook (protective amulets for children). |

### 6.2 Livestock (New Tracking Category — §3)

Horses, Oxen *(new)*, Mules/Donkeys *(new)*, Cattle, Sheep, Goats *(new)*, Pigs, Poultry.

### 6.3 Intermediate Goods

| Good | Integrates Into | Reasoning |
|---|---|---|
| Pitch (Pix) | Charcoal Kiln | Second missing Shipyard input. |
| Pigments | Dye Works | See above. |
| **Honey** *(new)* | Split from Honeycomb at the Apiary | Honeycomb previously stood in for both Wax and edible sweetener; splitting them unlocks Mulsum. |
| **Felt** *(new)* | Fulling Works | The Buildings doc already names this output; it was simply missing from the registry — a bug fix. |
| **Quicklime** *(new)* | Alternate output of Limestone Quarry | Feeds Mortar/Plaster alongside Gypsum. |
| **Mortar/Plaster** *(new)* | Second output at Concrete Works | Completes the Fresco chain — Pigments alone never had a substrate to be applied to. |
| **Cut Building Stone** *(new)* | Marble Works (alternate input alongside Raw Marble) | Mirrors the existing Marble Quarry → Worked Marble pattern exactly. |
| **Glue** *(new)* | Tannery (second output alongside Leather) | From boiled Raw Hides; feeds Carpentry Workshop and pairs with Sinew at Siege Workshop. |
| **Orichalcum** *(new)* | Bronzeworks (alternate output alongside Bronze) | The real coinage alloy — finally gives the Mint/Moneta building a material input it always lacked. |

### 6.4 Finished Goods

| Good | Integrates Into | Reasoning |
|---|---|---|
| Vinegar | Winery byproduct | Real, low complexity. |
| Butter | Dairy | Real Roman/Gallic cultural fault line vs. Olive Oil. |
| **Lard** *(new)* | Rendering House (Pig-fat output, corrected split from the previous generic "Tallow") | Historically Lard is pig fat, Tallow is beef/mutton fat — this pass fixes that conflation. |
| **Specialty Wines — Mulsum, Passum** *(new)* | Winery (alternate recipes) | Real, famous, and gives Winery an actual choice instead of one flat output; Mulsum needs the new Honey, Passum needs Raisins below. |
| **Dried Fruit/Raisins** *(new)* | A new Spoilage-mitigation mechanism (§9.2), not a building | Preserves Orchard Fruit/Grapes instead of losing them to spoilage; feeds Passum. |
| **Preserved Meat** *(new)* | Cured Meats Works (alternate, Gallic-flavored recipe) | Smoking rather than Sausages' grinding-and-spicing — gives Gaul a third food identity alongside Beer and Butter. |

### 6.5 Luxury Goods

| Good | Integrates Into | Reasoning |
|---|---|---|
| Feathers | Poultry Yard byproduct | Rounds out food output. |
| Furs/Pelts | Used directly | Gallic comfort good. |
| Truffles | Consumed at hosting events | Italian prized food. |

### 6.6 Imported Goods

| Good | Integrates Into | Reasoning |
|---|---|---|
| Papyrus, Ivory, Frankincense → Fine Incense, Exotic Beasts, Natron, Faience, Alabaster | Various — see prior passes | Already finalized. |
| **Indigo** *(new)* | Dye Works, premium alternative to domestic Woad | Follows the exact Frankincense/Incense pattern. |
| **Aromatic Woods** *(new)* | Carpentry Workshop, alongside Ivory | Raises Furniture's quality ceiling. |
| **Pearl** *(new)* | Goldsmith's Studio, or displayed/gifted directly | One of Roman elite society's single most status-charged luxuries — a real sumptuary-law hook for Policies & Edicts. |
| **Pepper** *(new)* | Broken out of generic Eastern Spices | Historically dominant enough (Rome once ransomed itself partly in pepper) to earn individual treatment. |
| **Myrrh** *(new)* | Apothecary, and the Libitinarius/Necropolis | Distinct from Frankincense — real embalming and medicinal use, not a duplicate incense input. |
| **Gemstones** *(new)* | Goldsmith's Studio | Rounds out Jewelry's input roster alongside Gold/Silver/Ivory/Pearl/Coral. |
| **Cotton** *(new, flagged minor)* | Weaver's Loom, alternate to Wool/Linen | Real but genuinely marginal for this era/setting — kept as an exotic curiosity, not a textile pillar. |

---

## 7. The Unified Goods Registry

Every good, five tiers, six properties: **Storage** (§8), **Perishability** (§9), **Quality-Eligible** (§10), **Region** (§5), and **Livestock** status for the eight tracked animal types (§3).

### 7.1 Raw Materials

| Good | Storage | Perishability | Quality | Region |
|---|---|---|---|---|
| Grain (Wheat/Barley/Oats) | Granary | Semi-Perishable | — | Egypt (future) |
| Legumes | Granary | Semi-Perishable | — | — |
| Olives | Warehouse | Perishable | — | — |
| Grapes | Warehouse | Perishable | — | Italian/Greek East |
| Flax | Warehouse | Semi-Perishable | — | — |
| Lavender | Warehouse | Perishable | — | Italian/Greek East |
| Wool | Warehouse | Semi-Perishable | — | — |
| Milk | None — same-tick only | Perishable | — | — |
| Fish | None / Macellum | Perishable | — | — |
| Salt | Warehouse | Non-Perishable | — | — |
| Murex Snails | None — immediate | Perishable | — | Iberian/Greek East |
| Oysters | Macellum / None | Perishable | — | — |
| Clay | Warehouse | Non-Perishable | — | — |
| Timber | Warehouse | Non-Perishable | — | — |
| Quartz Sand | Warehouse | Non-Perishable | — | — |
| Limestone | Warehouse | Non-Perishable | — | — |
| Raw Marble | Warehouse | Non-Perishable | — | Greek East/Iberian |
| Iron Ore | Warehouse | Non-Perishable | — | — |
| Copper Ore | Warehouse | Non-Perishable | — | — |
| Tin Ore | Warehouse | Non-Perishable | — | Gallic/frontier |
| Gold Ore | Warehouse | Non-Perishable | — | Iberian |
| Silver Ore | Warehouse | Non-Perishable | — | Iberian |
| Lead Ore | Warehouse | Non-Perishable | — | — |
| Resin | Warehouse | Semi-Perishable | — | — |
| Sandarac Wood | Warehouse | Non-Perishable | — | — |
| Woad/Dye Plants | Warehouse | Semi-Perishable | — | Gallic |
| Herbs | Warehouse | Perishable | — | — |
| Hemp | Warehouse | Semi-Perishable | — | — |
| Sea Sponges | Warehouse | Non-Perishable | — | Greek East |
| Cinnabar | Warehouse | Non-Perishable | — | Iberian |
| Manure | None — consumed on-site | Perishable | — | — |
| Sinew | Warehouse | Non-Perishable | — | — |
| Pozzolana | Warehouse | Non-Perishable | — | Italian heartland |
| Esparto Grass | Warehouse | Non-Perishable | — | Iberian |
| Silphium | None — finite regional stock | Non-Perishable | — | Greek East/Iberian-NA border |
| Saffron | Warehouse | Semi-Perishable | — | Greek East |
| **Orchard Fruit** *(new)* | Warehouse | Perishable | — | — |
| **Nuts** *(new)* | Warehouse | Non-Perishable | — | — |
| **Garden Produce** *(new)* | None / Macellum | Perishable | — | — |
| **Raw Hides/Skins** *(new)* | Warehouse | Semi-Perishable | — | — |
| **Reeds** *(new)* | Warehouse | Non-Perishable | — | — |
| **Building Stone** *(new)* | Warehouse | Non-Perishable | — | — |
| **Gypsum** *(new)* | Warehouse | Non-Perishable | — | — |
| **Coral** *(new)* | Strongroom / Warehouse | Non-Perishable | **Yes** | — |
| **Poultry, Eggs** | None — immediate use | Perishable | — | — |

*(Beef, Pork, Mutton, Goat Meat, and Feathers moved to §7.3–7.4 as livestock-derived goods; Poultry, Eggs remain listed above as they were before livestock formalization. Cattle, Sheep, Goats, Pigs, Horses, Oxen, and Mules/Donkeys are tracked as headcount per §3, not as warehouse-stored raw materials.)*

### 7.2 Intermediate Goods

| Good | Storage | Perishability | Quality | Region |
|---|---|---|---|---|
| Flour | Granary-adjacent | Semi-Perishable | — | — |
| Linen Fiber | Warehouse | Semi-Perishable | — | — |
| Lavender Oil | Warehouse | Semi-Perishable | — | — |
| Tallow | Warehouse | Semi-Perishable | — | — |
| Leather | Warehouse | Non-Perishable | — | — |
| Wax | Warehouse | Non-Perishable | — | — |
| Woven Cloth | Warehouse | Non-Perishable | **Yes** | — |
| Tyrian Purple | Warehouse | Non-Perishable | **Yes** | Iberian/Greek East |
| Common Dye | Warehouse | Non-Perishable | — | — |
| Iron | Warehouse | Non-Perishable | — | — |
| Bronze | Warehouse | Non-Perishable | — | — |
| Refined Gold | Strongroom | Non-Perishable | — | Iberian |
| Refined Silver | Strongroom | Non-Perishable | — | Iberian |
| Lead | Warehouse | Non-Perishable | — | — |
| Glass | Warehouse | Non-Perishable | — | — |
| Concrete | Warehouse | Non-Perishable | **Yes** | Italian heartland for Exceptional |
| Tile | Warehouse | Non-Perishable | — | — |
| Worked Marble | Warehouse | Non-Perishable | **Yes** *(fixed this pass — was inconsistently unmarked despite regional Exceptional-grade text)* | Greek East/Iberian for Exceptional |
| Charcoal | Warehouse | Non-Perishable | — | — |
| Malt | Granary-adjacent | Semi-Perishable | — | Gallic/frontier |
| Rope/Cordage | Warehouse | Non-Perishable | — | — |
| Pitch | Warehouse | Non-Perishable | — | — |
| Pigments | Warehouse | Non-Perishable | **Yes** | Iberian |
| **Honey** *(new)* | Warehouse | Semi-Perishable | **Yes** | Greek East (Hymettus) |
| **Felt** *(new)* | Warehouse | Non-Perishable | — | — |
| **Quicklime** *(new)* | Warehouse | Non-Perishable | — | — |
| **Mortar/Plaster** *(new)* | Warehouse | Non-Perishable | **Yes** | — |
| **Cut Building Stone** *(new)* | Warehouse | Non-Perishable | — | — |
| **Glue** *(new)* | Warehouse | Non-Perishable | — | — |
| **Orichalcum** *(new)* | Strongroom | Non-Perishable | — | — |

### 7.3 Finished Goods

| Good | Storage | Perishability | Quality | Region |
|---|---|---|---|---|
| Bread | None / Tavern immediate | Perishable | — | — |
| Wine | Apotheca / Warehouse | Semi-Perishable *(ages)* | **Yes** | Italian/Greek East |
| Beer | Warehouse | Perishable | — | Gallic/frontier |
| Olive Oil | Apotheca / Warehouse | Semi-Perishable | **Yes** | Italian/Greek East |
| Cheese | Warehouse | Semi-Perishable | **Yes** | — |
| Sausages | Warehouse | Semi-Perishable | — | — |
| Tunics | Warehouse | Non-Perishable | **Yes** | — |
| Sandals | Warehouse | Non-Perishable | — | Iberian (Esparto variant) |
| Pottery/Amphorae | Warehouse | Non-Perishable | **Yes** | Greek East |
| Tools | Warehouse | Non-Perishable | — | — |
| Weapons | Warehouse / Armory | Non-Perishable | **Yes** | — |
| Armor | Warehouse / Armory | Non-Perishable | **Yes** | — |
| Furniture | Warehouse | Non-Perishable | **Yes** | — |
| Parchment | Warehouse | Non-Perishable | — | — |
| Medicine | Warehouse | Semi-Perishable | **Yes** | — |
| Incense | Warehouse | Semi-Perishable | — | — |
| Siege Engines | Warehouse / Military | Non-Perishable | **Yes** | — |
| Vinegar | Warehouse | Non-Perishable | — | — |
| Butter | Warehouse | Perishable | — | Gallic/frontier |
| **Beef** *(culled livestock yield)* | Warehouse (short-term) | Perishable | — | — |
| **Pork** *(culled livestock yield)* | Warehouse (short-term) | Perishable | — | — |
| **Mutton** *(new, culled Sheep yield)* | Warehouse (short-term) | Perishable | — | — |
| **Goat Meat** *(new, culled Goat yield)* | Warehouse (short-term) | Perishable | — | — |
| **Lard** *(new)* | Warehouse | Semi-Perishable | — | — |
| **Mulsum, Passum (Specialty Wines)** *(new)* | Apotheca / Warehouse | Semi-Perishable *(ages like Wine)* | **Yes** | — |
| **Dried Fruit/Raisins** *(new)* | Warehouse | Non-Perishable | — | — |
| **Preserved Meat** *(new)* | Warehouse | Semi-Perishable | — | Gallic/frontier |

### 7.4 Luxury Goods

| Good | Storage | Perishability | Quality | Region |
|---|---|---|---|---|
| Perfume | Warehouse | Semi-Perishable | **Yes** | — |
| Soap | Warehouse | Non-Perishable | — | — |
| Fine Glass | Warehouse | Non-Perishable | **Yes** | — |
| Jewelry | Strongroom | Non-Perishable | **Yes** | — |
| Purple-Trimmed Togas | Warehouse | Non-Perishable | **Yes** | — |
| Writing Tablets | Warehouse | Non-Perishable | — | — |
| Garum | Warehouse | Non-Perishable | **Yes** | Iberian |
| Fine Seafood (Oysters) | Macellum / immediate | Perishable | **Yes** | — |
| Fine Incense | Warehouse | Semi-Perishable | **Yes** | — |
| **Feathers** | Warehouse | Non-Perishable | — | — |
| Furs/Pelts | Warehouse | Non-Perishable | **Yes** | Gallic/frontier |
| Truffles | None / Macellum-adjacent | Perishable | **Yes** | Italian heartland |

### 7.5 Imported Goods *(no domestic production chain)*

| Good | Storage | Perishability | Quality | Notes |
|---|---|---|---|---|
| Silk | Warehouse | Non-Perishable | **Yes** | — |
| Eastern Spices | Warehouse | Semi-Perishable | **Yes** | Now excludes Pepper, tracked separately below |
| Baltic Amber | Strongroom | Non-Perishable | **Yes** | Gallic trade advantage |
| Papyrus | Warehouse | Semi-Perishable | — | Egypt-flavored |
| Ivory | Strongroom / Warehouse | Non-Perishable | **Yes** | — |
| Frankincense | Warehouse | Non-Perishable | — | — |
| Exotic Beasts (Venatio Stock) | Menagerie/Amphitheater headcount | N/A | **Yes** *(rarity)* | Iberian/North African source |
| Natron | Warehouse | Non-Perishable | — | Egypt-flavored |
| Faience | Warehouse | Non-Perishable | **Yes** | Egypt-flavored |
| Alabaster | Warehouse | Non-Perishable | — | Egypt-flavored |
| **Indigo** *(new)* | Warehouse | Non-Perishable | — | Premium alternative to domestic Woad |
| **Aromatic Woods** *(new)* | Warehouse | Non-Perishable | **Yes** | — |
| **Pearl** *(new)* | Strongroom | Non-Perishable | **Yes** | Real sumptuary-law hook |
| **Pepper** *(new)* | Warehouse | Semi-Perishable | **Yes** | Broken out of Eastern Spices |
| **Myrrh** *(new)* | Warehouse | Non-Perishable | — | Distinct from Frankincense |
| **Gemstones** *(new)* | Strongroom | Non-Perishable | **Yes** | — |
| **Cotton** *(new, minor)* | Warehouse | Non-Perishable | **Yes** | Kept a minor curiosity, not a textile pillar |

### 7.6 Livestock (Tracked as Headcount, Not Warehouse Goods — §3)

Horses, Oxen, Mules/Donkeys, Cattle, Sheep, Goats, Pigs, Poultry. See §3.1 for home building, role, and yield split.

---

## 8. Storage & Capacity

| Storage Category | Building(s) | Holds |
|---|---|---|
| **Granary** | Granary → Grand Granary → Imperial Granary; Horreum | Grain, Legumes, Malt, Flour |
| **Apotheca** | Apotheca → Grand Cellar | Wine, Olive Oil, Mulsum, Passum — the only category that *improves* its contents (§9.3) |
| **Warehouse** | Storehouse → Warehouse → Warehouse Row | The general-purpose default |
| **Strongroom** | Private Strongroom/Treasury; the Argentaria | Refined Gold/Silver, Jewelry, Baltic Amber, Ivory, Pearl, Gemstones, Coral, Orichalcum |
| **Macellum / fast-turnover** | Macellum | Fish, Oysters, Fine Seafood, Truffles, Garden Produce |
| **None / immediate use** | — | Milk, Murex Snails, Poultry, Eggs, Bread |
| **None / consumed on-site** | — | Manure |
| **None / finite regional stock** | — | Silphium alone |
| **Livestock headcount** *(generalized this pass)* | Pasture-type buildings; Menagerie/Amphitheater for Exotic Beasts | All eight tracked animal types (§3), plus Exotic Beasts — none of these are warehouse-stored goods |

Capacity scales with each storage building's own tier. Overflow defaults to an automatic discounted sale if a Market/Emporium is connected; otherwise it's lost.

---

## 9. Spoilage & Preservation

### 9.1 The Three Tiers

- **Perishable:** Milk, Fish, Herbs, Oysters, Poultry, Eggs, Murex Snails, Lavender, Bread, Manure, Truffles, Butter, Orchard Fruit, Garden Produce, and freshly-culled Beef/Pork/Mutton/Goat Meat. A short shelf timer; lost if unused.
- **Semi-Perishable:** Grain, Malt, Flour, Wool, Resin, Wine/Olive Oil without active aging, Cheese, Sausages, Medicine, Incense, Fine Incense, Papyrus, Eastern Spices, Pepper, Saffron, Honey, Raw Hides/Skins, Lard, Preserved Meat. A generous buffer, then gradual value decay.
- **Non-Perishable:** every metal, Stone, Marble, Glass, Concrete, Mortar/Plaster, Tile, Leather, Wax, Rope, Pitch, Glue, Pottery, Tools, Weapons, Armor, Furniture, Jewelry, Nuts, Reeds, Building Stone, Gypsum, Coral, Felt, Quicklime, Cut Building Stone, Orichalcum, Dried Fruit/Raisins, and the rest of §7's unmarked goods. Never spoils.

### 9.2 What Mitigates It — Including a New Mechanism

Granary tier extends Grain's buffer; Salt-based preservation moves a Perishable raw good into a preserved form; the Macellum's fast-turnover sidesteps the timer entirely.

**New this pass:** a standing **Preservation Policy** lets Orchard Fruit and Grapes convert automatically into their Non-Perishable **Dried Fruit/Raisins** form instead of spoiling, the same automatic, no-per-tick-decision way Salt-curing already works for meat and fish. This is the concrete mechanism behind Dried Fruit/Raisins existing at all, and it also feeds Passum (§6.4).

### 9.3 The Wine Exception

Wine, Cheese, Mulsum, and Passum, stored in an Apotheca or Grand Cellar, improve rather than decay. Stored anywhere else, they're ordinary Semi-Perishable goods.

---

## 10. Quality Tiers

### 10.1 Displayed Grade vs. Hidden Precursor

Unchanged rule from the previous pass: **displayed** Quality (Common/Fine/Exceptional, shown to the player) applies to goods that are sold, gifted, displayed, or served directly, or where a specific design point is being made about an input's effect. **Hidden quality-precursor** applies to raw agricultural/mineral materials and bulk industrial intermediates — their production circumstances still set a downstream ceiling without displaying their own grade.

This pass's fix: **Worked Marble** was inconsistently unmarked despite already having regional Exceptional-grade flavor text (Greek Pentelic/Parian, Iberian Numidian) — now correctly marked Quality-eligible. **Mortar/Plaster** and **Honey** are new and Quality-eligible from the start, for the same reason Concrete and Pigments already are: they're the concrete mechanism behind a specific craftsmanship point (Fresco quality; a fine varietal honey).

### 10.2 The Three Grades

Common / Fine / Exceptional, with Exotic Beasts' rarity-scale reuse unchanged.

### 10.3 What Sets It

Input quality (Pozzolana, Natron, Alabaster, Saffron, better Grapes) sets a ceiling; processing quality (a skilled Overseer, a dedicated upgrade like the Grand Cellar) closes the gap.

---

## 11. Currency & Pricing

Unchanged. Denarii is the single tracked currency; real denominations appear only in narrative flavor text. Base price by tier, modified by Quality, regional scarcity, and Market Dynamics.

---

## 12. Market Dynamics & Trade

Unchanged: full dynamic simulation — supply/demand, seasonality, disruption from Disasters/Piracy/War, trade exposure scaling with Commerce development, and a shared regional market Rival Houses also participate in. This pass adds livestock rustling (§3.2) as a new Piracy & Banditry disruption vector, distinct from goods or people.

---

## 13. Consumption

### 13.1 Population-Level Consumption

The Tavern/Caupona draws down Bread, Wine/Beer, Cheese, Sausages, and now Preserved Meat and Garden Produce, at a rate scaling with population — see §2.1 for where population tracking is actually headed.

### 13.2 Household-Level Consumption — the Regimen's Diet Axis

| Diet Tier | Consumes |
|---|---|
| **Meager** | Grain/Legumes only |
| **Adequate** | Bread, plus a modest Wine or Beer allotment |
| **Generous** | Bread, Wine/Beer, Cheese or Sausages, and occasional Fish or Garum |

### 13.3 Internal/On-Estate Consumption

Manure and Sinew are auto-applied to an adjacent building the same tick they're produced.

### 13.4 Building Upkeep Consumption (Consolidated)

Temple/Shrine consumes Incense or Fine Incense; Valetudinarium/Iatreion consumes Medicine (and now, optionally, Myrrh for a quality boost); libraries consume Writing Tablets, Parchment, or Papyrus; Balneum/Bathhouse consume Sea Sponges; the Mint/Moneta now consumes Orichalcum, closing a gap that building always had.

---

## 14. Gifting & Diplomatic Value

Unchanged. Every good carries a Gift Value beyond its sale price; Quality and scarcity both compound it. Pearl in particular is worth flagging as a marquee gift good given its real historical status weight — a strong future hook for a specific Politics & Patronage or Legal & Court sumptuary-law event.

---

## 15. Cross-System Integration

- **Buildings doc:** four new buildings this pass (Orchard, Garden Plot, Reed Bed, Goat Pasture); every other new good folds into an existing building, now including Rendering House, Fulling Works, Concrete Works, Marble Works, Tannery, Bronzeworks, Winery, and Cured Meats Works as newly-touched buildings alongside the previous passes' list.
- **Estate & Settlement:** Marsh/Poor land terrain finally has two dedicated buildings (Reed Bed, Goat Pasture) instead of being purely a liability.
- **Companions & Court Positions:** the Vilicus's existing Pasture remit (§4.2 there) is exactly what governs livestock Herd Strategy — no new Overseer position was needed.
- **Settlement Demographics (§6.26):** §2.1 hands that future system this document's consumption *and* livestock models as a foundation.
- **Labor & Slavery:** Regimen's Diet axis now includes Preserved Meat and Garden Produce as real options.
- **Disease & Public Health:** livestock disease (§3.2) is a genuinely new touchpoint for that system.
- **Piracy & Banditry:** livestock rustling (§3.2, §12) is a new raid category distinct from goods or people.
- **Games & Spectacle:** Oxen/Mules' logistics role plausibly feeds chariot-adjacent Circus flavor alongside Horses.
- **Military & Combat:** Glue (with Sinew) completes authentic composite-construction material for Siege Engines.
- **Religion:** Myrrh gives embalming/funerary practice (the Libitinarius, Necropolis) a real material tie for the first time.
- **Politics & Patronage:** Pearl's sumptuary-law history is a flagged future hook.
- **Familia:** Coral's protective-amulet association ties directly to the Infant/Child lifecycle stakes.
- **Dynasty Chronicle:** Silphium's eventual exhaustion remains designed as a Chronicle-worthy, irreversible event; unchanged this pass.

---

## 16. Data Model

```
Good {
  key,
  tier,                   // "raw" | "intermediate" | "finished" | "luxury" | "imported"
  storageCategory,        // "granary" | "warehouse" | "apotheca" | "strongroom" | "macellum" |
                           // "none" | "internalUse" | "finiteRegionalStock"
  perishability,          // "perishable" | "semiPerishable" | "nonPerishable" | "n/a"
  qualityEligible: bool,
  hiddenQualityPrecursor: bool,
  regionWeight: regionId | null,
  basePrice,
  giftValueMultiplier,
  autoConsumedBy: buildingCategory | null,
  preservationConversion: goodKey | null   // NEW — e.g. Orchard Fruit -> Dried Fruit, §9.2
}

LivestockStock {                // NEW — §3
  buildingId,
  animalType,              // "horses" | "oxen" | "mulesAndDonkeys" | "cattle" | "sheep" |
                            // "goats" | "pigs" | "poultry"
  headcount,
  purposeAllocation,       // relevant only for cattle ("dairyBeef"|"draft") and horses ("cavalry"|"logistics")
  herdStrategy,            // "growthFocused" | "balanced" | "yieldMaximizing"
  mortalityRisk
}

RegionalStock {                 // §5 — unique to Silphium for now
  goodKey,
  remainingReserve,
  depletionRatePerHarvestIntensity,
  exhausted: bool
}

GoodStock {
  goodKey,
  quantity,
  quality,
  ageInTicks,
  locationId
}

SettlementMarket {
  settlementId,
  prices: { [goodKey]: currentPrice },
  supply: { [goodKey]: currentSupply },
  demand: { [goodKey]: currentDemand },
  tradeExposure
}
```

---

## 17. Open Questions

- **All previously-carried numeric questions** (spoilage timers, Quality multipliers, supply/demand formula, overflow discount, regional variance magnitude, Silphium's depletion curve) remain open, per this project's established numbers-later convention.
- **Herd Strategy's actual growth/yield tradeoff numbers.** §3.2 establishes three tiers exist; the actual rates aren't specified.
- **Livestock disease mechanics.** §3.2 flags this as a new Disease & Public Health touchpoint; the actual design (a parallel outbreak system, or a modifier on existing mechanics) belongs to that system's own eventual pass.
- **Livestock rustling as a Piracy & Banditry event type.** Flagged as a new raid category; not designed beyond the flag.
- **Oxen and Mules/Donkeys' actual efficiency bonus size.** §3.1 establishes the role (construction/Agriculture speed for Oxen, caravan/courier capacity for Mules); neither is numerically sized.
- **Whether Cotton's "minor, flagged" status should have any mechanical expression** (a price ceiling, an availability cap) beyond narrative flagging.
- **Back-porting into the Buildings and Estate & Settlement docs.** Both still contain their own now-superseded partial lists; this pass makes that gap larger, not smaller, and it's the most concrete remaining task before implementation.
- **This document's own completeness.** Given the scale of this pass, a final "no more categories" commitment is being made explicitly (§1) — any future goods work should be numeric balancing or bug fixes like this pass's Worked Marble and Tallow/Lard corrections, not new categories.
