# GENS — System Design: Natural Disasters & Environment (§6.17)
*Nine hazard types now, not five — Fire, Flood, Earthquake, Drought/Famine, Storm, Landslide, Blight & Infestation, and Frost as the standing eight, plus a rare, special-cased Volcanic Eruption — each with real historical grounding and its own exposure logic. This pass adds three new hazards, a fourth (Volcanic Eruption) that resolves what the first draft could only flag as an open question, a full Compounding & Seasonal Hazards layer modeling how Mediterranean dry-season fire risk and storm-season flooding actually relate to each other, and a third environmental track (Slope Stability) alongside Soil Fertility and Forest Cover — extending the land-use loop from Agriculture and Timber into Mining as well.*

---

## Contents

1. Scope & Role
2. The Hazard Types
3. Disaster Exposure — A Standing, Emergent Reading
4. Environmental Health — Soil, Forest, Slope & the Land-Use Loop
5. Disaster Events — Effect, Damage & Repair
6. Recovery & Relief — Omens, Patronage, and Debt
7. Cross-System Integration
8. Data Model
9. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "fires, floods, earthquakes, and drought as periodic crisis events, distinct from but able to compound with Disease, that test infrastructure choices made in Estate & Settlement and can ripple into Politics (disaster relief as patronage) and Religion (a disaster as an omen)." This pass treats that as a real, fuller system — the same "mid-weight, standing-meter" treatment Religion's Favor and Politics & Patronage's Faction already received — and takes the "& Environment" half of the title seriously: not five disaster types and a countermeasure checklist, but a genuinely wider hazard roster sitting on top of a real, multi-track land-use loop.

Two halves, working together:

- **Disasters** (§2–3, §5) — eight standing hazard types plus one rare, specially-treated ninth, each with a real, player-influenceable **Exposure** reading — an emergent number read off terrain, buildings, and land-use choices, not a slider the player sets directly — that determines how often and how severely that hazard's periodic Events actually land. §3.1 adds real interaction between hazards: a dry season doesn't just raise Drought risk, it raises Fire risk alongside it, the same way an actual Mediterranean summer does.
- **Environment** (§4) — three tracked values now, not two: **Soil Fertility**, **Forest Cover**, and **Slope Stability**, each depleted by a real, named intensity choice at the relevant building type, with real long-game consequences rather than a cosmetic "environmentalism" layer. This is the same kind of real depletion mechanic Resources & Goods already established with Silphium — a resource that actually runs down through player choice.

Neither half locks the player into a single correct playstyle. A household can farm, log, and mine aggressively and simply accept higher disaster exposure as the cost of higher output — a real, legible tradeoff, not a trap.

---

## 2. The Hazard Types

| Hazard | Real grounding | Primary exposure driver | Existing/new countermeasure |
|---|---|---|---|
| **Fire** | The real danger of Rome's own densely-packed *insulae* districts (the historical Great Fire of 64 AD looms large for good reason) | Insulae density and overall urban building density; rises during Drought conditions (§3.1) | **Vigiles Post** (Buildings §4.10) |
| **Flood** | Seasonal river flooding, a routine hazard for any settlement built near water | River-adjacent plots, worsened by low regional Forest Cover (§4.2); rises during Storm season (§3.1) | **Levee** *(new, §2 of the first pass — River/Coast-gated)* |
| **Earthquake** | The Mediterranean's real seismic activity — the Aegean and Italian peninsula both sat on genuinely active ground | Region (Greek East and parts of the Italian heartland run higher baseline exposure) | **None — genuinely unpreventable, see §2.1** |
| **Drought / Famine** | The recurring Mediterranean dry-season threat to a Grain-dependent economy | Region (Iberian colony runs hot), worsened by low Soil Fertility (§4.1) | **Aqueduct/Cistern** (Aquarius) and **Horreum** (Horrearius) |
| **Storm** | Mediterranean sailing's real seasonal danger — the ancient "closed sea" season existed for exactly this reason | Coastal/Port reliance and sea-trade volume | **Lighthouse** (Buildings §4.10) |
| **Landslide** *(new)* | A real, direct consequence of hillside deforestation and quarrying — ancient writers themselves connected slope clearance to slope failure | Hills/Mountain terrain, driven jointly by low regional Forest Cover **and** low Slope Stability (§4.3) | **Terraced Field** (Estate & Settlement's own Agriculture tier) doubles as slope reinforcement on adjacent Hills plots |
| **Blight & Infestation** *(new)* | Crop disease and locust/pest swarms — both real, recurring, and vividly documented ancient agricultural hazards | Low crop diversity — an Intensive-Monoculture estate is mechanically more vulnerable, the same real ecological principle actual monocultures suffer from | **Crop diversification** (Estate & Settlement §6's own specialization-vs-diversification choice) — no new building; diversifying is itself the countermeasure |
| **Frost** *(new)* | Perennial crops — olives and grapevines specifically — are genuinely frost-vulnerable, a real seasonal risk ancient agricultural writers worried over directly | Region (Gallic frontier runs cold) and, more distinctively, **how concentrated the estate's own output is in Olive/Vineyard chains specifically** | **Diversification** away from Olive/Vineyard concentration — again Estate & Settlement's own existing lever, not a new building |
| **Volcanic Eruption** *(new, rare/special)* | Real, and impossible not to name given the setting: an Italian-heartland, Campania-adjacent estate sits in the shadow of a real and eventually catastrophic mountain | A flagged **Dormant Volcano** terrain feature, present only on specific Italian-heartland plots | **None — see §2.2** |

Fire, Flood, Storm, and Landslide all produce genuine building/cargo damage requiring Repair (§5); Earthquake and Volcanic Eruption can strike any building type; Drought/Famine, Blight & Infestation, and Frost act on yield and Contentment rather than physical structures — Frost specifically hitting perennial crops with the distinct multi-year recovery tail described in §5.4.

### 2.1 Earthquake — Still the One Ordinary Hazard With No Countermeasure

Unchanged from the first pass, and worth restating plainly: no building, masonry investment, or engineering choice reduces Earthquake's likelihood or base severity. What the player can influence is entirely the aftermath — Treasury reserve, patronage standing, and Repair speed — never the event itself.

### 2.2 Volcanic Eruption — Rare by Design, Not by Exposure Math

Deliberately treated outside the ordinary Exposure system (§3) rather than folded into it: a real volcanic eruption on the scale this document has in mind happened perhaps once in the entire span of history relevant to this game, and giving it a standing monthly-roll Exposure score the way Fire or Storm gets one would wildly overstate its real frequency. Instead, a **Dormant Volcano** is a rare, fixed terrain feature placed on specific Italian-heartland plots at map generation (echoing the real Vesuvius/Campania geography directly), and an eruption is a single, exceptionally low-frequency Catastrophic-only Event gated to households holding or neighboring that plot — closer in spirit to Silphium's own rarity treatment than to this document's other eight hazards.

**The genuinely double-edged part, played straight rather than softened:** an eruption is a real, immediate catastrophe for anything near the epicenter — Destroyed buildings, real population loss, a struck Monument as likely as not — but the ash fallout that follows is a real, historically accurate **long-term Soil Fertility boost** across the surrounding region, the same reason Campania's own volcanic soil made it some of the most fertile farmland in the ancient Mediterranean. This document doesn't pretend the eruption was secretly good — the immediate cost is real and severe — but the land it leaves behind is, in a very real sense, better for it, a genuinely uncomfortable historical truth worth keeping rather than smoothing away.

---

## 3. Disaster Exposure — A Standing, Emergent Reading

Each of the eight standing hazards (Volcanic Eruption excepted, per §2.2) carries a hidden **Exposure** score, read continuously off the household's own choices — matching the same "accumulated pattern, not a menu pick" logic Politics & Patronage's Faction and Policies & Edicts' Household Doctrine already use. A genuinely low-Exposure household has earned that safety through real investment; a high-Exposure one has usually traded that safety away for something else.

Exposure feeds a genuinely periodic Event roll — this stays a real, discrete narrative *Event* (Events §6.8, future), not a constant background drain — but the odds and likely severity of that roll are entirely a function of the Exposure the player's own choices have produced.

### 3.1 Compounding & Seasonal Hazards

New this pass: hazards aren't fully independent of each other, and treating them that way would have flattened out a real, well-documented Mediterranean pattern — a dry season is both fire season and drought season at once, and a storm's heavy rain doesn't stop being a flood risk just because it arrived as wind and waves first.

- **Dry-season overlap:** while a settlement's Drought Exposure is actively elevated (a real drought either in progress or building), Fire Exposure temporarily rises alongside it — dry tinder is dry tinder, whether the immediate concern is a failed harvest or a spark in an Insulae district.
- **Storm-into-Flood chaining:** a Storm Event resolving at Severe or Catastrophic severity has a real chance of directly triggering a Flood Event on any River-adjacent plot in the same settlement the same month, rather than the two hazards rolling entirely independently — a storm surge or sustained heavy rain overwhelming a river is a single real weather event wearing two of this document's hazard labels, not a coincidence.
- **Deforestation's double reach:** low Forest Cover (§4.2) already raises Flood Exposure; this pass extends the same regional value to Landslide Exposure as well (§2), since the real hydrological and slope-stability effects of losing tree cover genuinely overlap rather than sitting in separate categories.

None of this turns any single hazard into an inevitability — it's a real, felt correlation a player can read and plan around (a household that's just weathered a bad drought knows to watch for fire too), not a guaranteed chain.

---

## 4. Environmental Health — Soil, Forest, Slope & the Land-Use Loop

Three tracked values now, all real and depletable, all tied directly into existing Estate & Settlement and Resources & Goods mechanics.

### 4.1 Soil Fertility

Unchanged from the first pass: every Fertile Plain plot carries a Soil Fertility value, read against a standing **Cultivation Intensity** setting (Fallow Rotation / Standard / Intensive Monoculture, mirroring Resources & Goods' own Herd Strategy shape) at the Agriculture-building level. Fallow Rotation recovers Fertility over time (faster with an adjacent Legume Field); Intensive Monoculture drains it. Depleted Fertility lowers base yield and raises Drought/Famine severity specifically on that land — and, new this pass, raises Blight & Infestation vulnerability on the same plot, since exhausted, monoculture-heavy soil is exactly the real ecological condition both crop disease and pest swarms exploit most easily.

### 4.2 Forest Cover

Unchanged from the first pass: a regional value read against a standing **Harvest Intensity** setting (Sustainable Yield / Standard / Clear-Cutting) at the Timber Camp level. Depleted Forest Cover raises Flood Exposure and — new this pass, per §3.1 — Landslide Exposure as well, and sustained long enough caps and eventually reduces the Timber Camp's own maximum output.

### 4.3 Slope Stability *(new)*

The third track this pass adds, closing the gap Landslide's own introduction opened: every Hills/Mountain plot carries a **Slope Stability** value, read against a standing **Excavation Intensity** setting at the Quarry/Mine-building level, the same three-tier shape as its two siblings:

| Tier | Effect |
|---|---|
| **Conservative Extraction** | Reduced current Stone/Ore output, but Slope Stability slowly recovers |
| **Standard** | Slope Stability holds roughly steady |
| **Aggressive Extraction** | The highest available current output, at a real, compounding Slope Stability drain |

Low Slope Stability, combined with low regional Forest Cover per §3.1, is what actually drives Landslide Exposure on a given Hills plot — a household running Clear-Cutting *and* Aggressive Extraction in the same hilly region is deliberately stacking two real risk factors into one hazard, and should feel that stacking rather than have it hidden.

### 4.4 A Real Tradeoff, Not a Morality Meter

None of Cultivation Intensity, Harvest Intensity, or Excavation Intensity is framed as a "good" or "bad" choice this document judges — each is a real, legitimate bet that short-term output matters more than long-term resilience, matching Design Pillar #1's "no dominant setting, only tradeoffs." Worth noting only in passing: this bet sits naturally alongside Policies & Edicts' own Household Doctrine (§6.12 §3) — a household drifting toward Domus Dura's exploitative logic finds all three Intensive/Clear-Cutting/Aggressive settings a comfortable extension of the same worldview, while Domus Pia or Domus Provincialis's more harmony-oriented instincts pull the other way.

---

## 5. Disaster Events — Effect, Damage & Repair

### 5.1 Severity Tiers

Every Disaster Event resolves at one of four severity tiers: **Minor**, **Moderate**, **Severe**, or **Catastrophic** (Volcanic Eruption, per §2.2, resolves at Catastrophic only). Severity is rolled against the hazard's current Exposure (§3) — a low-Exposure household is both less likely to suffer an Event at all and weighted away from the worst outcomes when one does land.

### 5.2 Building Damage

Estate & Settlement's own Plot data model already carries a building `condition` field, established specifically for this purpose. A Disaster Event writes a real condition drop (scaled to severity) directly into it, and the existing Repair action is the entire recovery mechanism — this document supplies the trigger and severity, not a parallel repair system. A Catastrophic result can push condition all the way to **Destroyed**, at which point Estate & Settlement's own Demolition-and-rebuild path applies.

### 5.3 Beyond Buildings

- **Monuments:** a struck Monument loses Dignitas standing on top of the usual condition/Repair treatment.
- **Livestock:** Flood, Storm, and Landslide can all reduce a Pasture's headcount, per Resources & Goods §3.2's own existing flag.
- **Pop Groups:** Settlement Demographics' Contentment takes a direct hit proportional to severity; a Catastrophic Fire, Flood, Landslide, or eruption in a dense district can cause genuine population loss.
- **Cargo & Ships:** Storm-specific — an in-transit shipment or vessel can be damaged or lost.

### 5.4 Perennial Crops — A Distinct Recovery Shape

New this pass, specific to Frost (and, incidentally, to a nearby Volcanic Eruption's own ash-and-fire damage to standing groves): unlike Grain, which recovers on an ordinary one-season Repair-equivalent timeline, a killed Olive Grove or Vineyard doesn't simply come back next season. Mature olive trees and grapevines take real years to reach productive age, so Frost damage to a perennial-crop building sets that building's own output back to something closer to its *earliest* production tier rather than merely docking a season's yield — a genuinely longer, more painful recovery tail than any of this document's other crop-affecting hazards impose, and the mechanical reason Frost Exposure concentrated in Olive/Vineyard-heavy estates is worth actually worrying about rather than shrugging off as one bad year.

---

## 6. Recovery & Relief — Omens, Patronage, and Debt

### 6.1 Religion — Disaster as Omen

Unchanged in substance from the first pass: a household in Divine Displeasure (Religion §2.3) has its passive Omen Events skew toward foreshadowing whichever hazard its current Exposure profile makes most likely, now drawing from the full nine-hazard roster rather than the original five — a Frost-prone, Olive-heavy estate's Omens read differently from a Fire-prone urban one's.

### 6.2 Politics & Patronage — Relief as Patronage

Unchanged: **Disaster Relief** remains a natural ninth Policies & Edicts Funded Action, triggered by physical disaster damage. Worth adding explicitly this pass: a Minor or Moderate Event rarely justifies the political theater of a funded relief response — Disaster Relief's real patronage value is concentrated at Severe and Catastrophic severity, where the whole settlement has actually noticed something happened and a visible response actually reads as an act of patronage rather than an oddly generous non-event.

### 6.3 Economy & Finance — Recovery as a Reason to Borrow

Unchanged: a Severe or Catastrophic Event's Repair cost — now including Frost's own longer perennial-crop recovery tail (§5.4) as a real, extended drain rather than a single expense — is exactly the kind of sudden, large, non-optional cost that pushes a Treasury into genuine debt.

---

## 7. Cross-System Integration

- **Estate & Settlement:** the Plot `condition` field and Repair/Demolition mechanics are this document's entire building-damage layer; terrain (River, Coast, Hills, Marsh/Poor land) drives most hazards' Exposure directly; the specialization-vs-diversification choice (§6 of that doc) is the actual countermeasure for both Blight & Infestation and Frost.
- **Buildings:** Vigiles Post, Lighthouse, Aqueduct/Cistern, Horreum, and the Levee are named countermeasures; Terraced Field does double duty as Landslide mitigation.
- **Companions & Court Positions:** the Aquarius and Horrearius remain Drought/Famine's named operators.
- **Resources & Goods:** Cultivation Intensity, Harvest Intensity, and the new Excavation Intensity all mirror that document's own Herd Strategy pattern; livestock mortality from disaster is a named, existing hook; Forest Cover's long-run Timber cap and Volcanic Eruption's fertility-boost aftermath both echo Silphium's own precedent for real, played-straight resource consequences.
- **Settlement Demographics:** Contentment and, at Catastrophic severity, real population loss are direct outputs; Overcrowding is itself a Fire-Exposure contributor.
- **Religion:** Divine Displeasure skews Omens toward the household's current highest-Exposure hazard across the full nine-type roster.
- **Policies & Edicts:** Disaster Relief remains a named forward addition to that system's Funded Action roster; Frontier Security Posture's disaster-exposure language reads this document's Storm/Flood/Landslide Exposure.
- **Economy & Finance:** disaster recovery, including Frost's extended perennial-crop drain, as a borrowing trigger.
- **Military & Combat:** Storm damage to vessels remains a distinct, maritime-specific outcome.
- **Disease & Public Health (§6.13, future):** unchanged — a Flood, Famine, or Blight's own crowding/scarcity fallout is that system's natural trigger condition; this document still deliberately draws the line at not simulating epidemic disease itself.
- **Dynasty Chronicle:** a Catastrophic Event of any kind, a struck Monument, a Volcanic Eruption specifically, or a well-handled relief response are all real, guaranteed-or-near-guaranteed entries.
- **Rival Houses:** a disaster striking a rival's holdings, or a visibly better/worse relief response than a rival's, remains a natural comparison point.

---

## 8. Data Model

```
HazardExposure {
  settlementId,
  hazardType,                    // "fire" | "flood" | "earthquake" | "droughtFamine" | "storm" |
                                  // "landslide" | "blightInfestation" | "frost"
                                  // (volcanicEruption tracked separately — see DormantVolcano below)
  exposureScore,
  contributingFactors: [ ... ],
  temporarilyElevatedBy: [ ... ],   // §3.1 — e.g. "activeDroughtConditions" raising fire's own score
}

DormantVolcano {                  // §2.2 — a rare, fixed terrain feature, not an ordinary HazardExposure record
  plotId, settlementId,
  hasErupted: bool,
  postEruptionFertilityBoostActive: bool,
  postEruptionFertilityBoostRegionRadius,
}

EnvironmentalHealth {
  settlementId,
  soilFertilityByPlot: [ { plotId, fertility, cultivationIntensity } ],
  forestCoverRegional, harvestIntensity,
  slopeStabilityByPlot: [ { plotId, stability, excavationIntensity } ],   // new — §4.3
}

DisasterEvent {
  eventId, settlementId, month,
  hazardType,                     // includes "volcanicEruption" as a possible value here, distinct from the
                                   // ordinary HazardExposure-tracked list above
  severity,
  precededByOmen: bool,
  triggeredByCompounding: bool,    // §3.1 — true if this Event fired as a direct chain from another that month
  affectedPlotIds: [ ... ],
  monumentStruck: bool,
  livestockLoss: [ { buildingId, headcountLost } ],
  perennialCropSetback: bool,       // §5.4 — true if a struck Olive Grove/Vineyard resets to an early production tier
  popGroupContentmentImpact: [ { popGroupId, contentmentDelta, populationLoss } ],
  cargoOrVesselLoss: [ ... ],
  reliefFundedActionRef,
}
```

---

## 9. Open Questions

- **All numeric sizing.** Exposure-to-Event-odds curves, severity distribution, the three Environmental Health tracks' drain/recovery rates, compounding-hazard trigger probability, and Volcanic Eruption's own vanishingly-low frequency are all unsized, per this project's convention.
- **Regional Earthquake and Frost baselines.** Both are flagged as region-weighted (§2) without the actual weighting specified.
- **Dormant Volcano plot placement.** §2.2 establishes it as an Italian-heartland/Campania-flavored fixed feature; how many such plots exist, and whether a player can ever end up owning more than one, isn't decided.
- **Blight & Infestation's regional/seasonal flavor.** §2 treats crop disease and pest swarms as one combined hazard; whether they should eventually split into two distinct sub-flavors with different regional weighting isn't resolved.
- **Slope Stability's regional granularity**, mirroring Forest Cover's own carried-forward open question about per-plot vs. settlement-wide tracking.
- **Multi-settlement Exposure interaction.** Unchanged — whether a second settlement's Exposure and Environmental Health track fully independently or share regional factors isn't addressed.
- **Disaster Relief's exact Funded Action shape.** Still deferred to a future Policies & Edicts revisit rather than this document.
