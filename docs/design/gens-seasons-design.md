# GENS — System Design: Seasons
*The mechanical layer sitting alongside the Roman Calendar's own structural one: that document owns real month names, day-counting, and the market week; Religion owns the sacred calendar's actual feast days. Neither ever said what time of year actually *does* to the rest of the game. This document is that missing layer — a real, four-season cycle converting the calendar's twelve months into concrete, recurring mechanical effects across Agriculture, Military & Combat, Travel, Disease & Public Health, Natural Disasters, and Economy & Finance, several of which already gestured at seasonality (the Roman "closed sea," Frost's own perennial-crop damage, Trade Routes' own seasonality flag) without ever actually defining it. This document is that definition.*

---

## Contents

1. Scope & Role
2. The Four Seasons — Real Roman Reckoning
3. Agriculture & the Growing Cycle
4. The Campaigning Season & Winter Quarters
5. Mare Clausum — The Closed Sea
6. Disease Seasonality — The Autumn Fever
7. The Hungry Gap — Winter Scarcity and Stores
8. Harvest Push — A Temporary Labor Reallocation
9. Seasonal Weighting of Natural Disasters
10. Markets, Prices & the Seasonal Cycle
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role

The Roman Calendar document already supplies real month names, real day-counting (Kalends/Nones/Ides), the nundinal market cycle, and the Julian reform's own before/after split. Religion already owns every fixed feast day sitting on that calendar. Both are structural — neither one asks "so what actually changes about the game in February versus August?" This document answers that question directly, converting the twelve real Roman months into four real seasons, each carrying concrete, recurring mechanical weight across systems that have, in several cases, already been gesturing at seasonality without ever formally defining it: Natural Disasters' own Storm entry already invokes "the ancient 'closed sea' season" by name; Natural Disasters' own Frost hazard already assumes a winter window; Economy & Finance's Trade Routes section already inherits "seasonality" from Resources & Goods without either document actually specifying what that means. This document is the missing definition behind all three references, and several others besides.

**What doesn't move here:** month names, day-counting, feast days, and the market week all stay exactly where the Roman Calendar and Religion already put them. This document adds a seasonal layer on top, not a competing calendar.

---

## 2. The Four Seasons — Real Roman Reckoning

Real Latin gives this project four ready-made, authentic season names, mapped to the Roman Calendar's own twelve months at the boundaries Roman agricultural and military practice actually observed rather than a modern, evenly-quartered year:

| Season | Real Name | Roughly | Character |
|---|---|---|---|
| **Spring** | *Ver* | Februarius – Aprilis | The campaigning season opens (§4); planting for the summer grain harvest; the sailing season reopens (§5) |
| **Summer** | *Aestas* | Maius – Sextilis/Augustus | The main grain harvest; peak campaigning season; peak sailing season; the driest, highest-Fire/Drought stretch of the year |
| **Autumn** | *Autumnus* | September – October | The grape vintage and olive harvest begin; the campaigning season closes; disease risk peaks (§6); the sailing season's own danger window begins |
| **Winter** | *Hiems* | November – Ianuarius | Mare Clausum in full effect (§5); armies in winter quarters (§4); the hungry gap builds toward its worst point (§7); Frost risk (Natural Disasters) is live |

These boundaries are deliberately soft rather than hard calendar-day cutoffs — consistent with how the real Roman world actually experienced them, a "late Ver" and an "early Aestas" genuinely overlapped, and this document's own seasonal effects below phase in and out gradually rather than flipping on a single date.

---

## 3. Agriculture & the Growing Cycle

Resources & Goods already established Cultivation Intensity as a standing policy choice and named real crop-specific goods (Grain, Grapes, Olives) without ever tying any of them to an actual point in the year. This document supplies that timing, giving each crop family a real, historically accurate harvest window rather than a flat, undifferentiated monthly trickle:

- **Grain** — planted in spring, harvested in early-to-mid summer. The single most time-critical harvest in the entire calendar, and the one most exposed to a poorly-timed Drought or Blight & Infestation Event (Natural Disasters) landing during the growing window rather than after.
- **Grapes** — harvested in the vintage, early-to-mid autumn — a real, specific, and genuinely festive moment in the Roman agricultural year worth its own flavor beat distinct from Grain's more workmanlike harvest.
- **Olives** — harvested latest of the three, running into early winter — the last major harvest push of the year, and the direct reason Frost (Natural Disasters §5.4) lands with such disproportionate, multi-year-recovery force: an olive grove's harvest and its single most vulnerable frost-exposed window sit right next to each other on the calendar.

**Yield concentration, not redistribution.** This document doesn't change how much a given Agriculture building produces across a year — Resources & Goods' own totals stand — it changes *when* that output actually lands, concentrating each crop's yield into its own real harvest month rather than spreading it evenly across twelve. This is what makes §10's own market-glut mechanic and §8's own labor-surge mechanic possible at all: neither makes sense against a flat, yield-every-month baseline.

---

## 4. The Campaigning Season & Winter Quarters

Military & Combat's own Estate Force and Roman Service tracks have always assumed a Force can simply deploy whenever the player chooses. Real Roman practice didn't work that way, and this document gives that document's own deployment mechanics the real seasonal shape they were always missing: armies campaigned from spring through autumn and, with rare and notable exceptions, withdrew into **winter quarters** (*hiberna*) for the winter months rather than continuing active operations.

- **An Offense/Campaign deployment launched during Ver through Autumnus** resolves under Military & Combat's own ordinary rules, unchanged.
- **A deployment launched or still active deep into Hiems** carries a real, additional Morale and Supply-drain penalty on top of that document's own existing mechanics — not a hard block (real Roman commanders occasionally did campaign through winter, and this document doesn't want to remove a genuine, if costly, player choice), but a real, felt cost for defying the season rather than a free option.
- **Winter Quarters itself is a legitimate, deliberate posture**, not merely "not campaigning" — a Force deliberately encamped for the winter draws reduced Supply and suffers no Readiness penalty for standing still, the honest, historically accurate alternative to either an expensive winter campaign or fully disbanding a Force between campaigning seasons.

This gives the Muster mechanic (Military & Combat §2.5) a real, natural rhythm: raised in spring, active through the campaigning season, stood down into Winter Quarters or discharged back to the Veterans pool as autumn closes — exactly the shape a real Roman military year actually had.

---

## 5. Mare Clausum — The Closed Sea

Natural Disasters' own Storm entry already names this directly ("the ancient 'closed sea' season existed for exactly this reason") without ever formally defining it — this document is that definition. Real Roman and later Roman-era sources (Vegetius among them) describe a genuine, named seasonal closure of Mediterranean sea lanes to ordinary shipping, roughly spanning Hiems, driven by real, sharply elevated storm danger rather than superstition.

- **Travel's own sea-route options** (§4 of that document) carry a real, substantially elevated real-stakes-event weighting during Mare Clausum — not a hard lock (a determined or desperate traveler can still sail), but the honest, felt cost of defying it, mirroring §4's own winter-campaign treatment above.
- **Trade Routes** (Economy & Finance §7, Resources & Goods §12) see reduced volume and, per §10 below, real price effects during the same window — this is the actual mechanism behind Resources & Goods' own previously-unspecified "seasonality" flag.
- **Piracy & Banditry's own raiders** are subject to the identical real danger a merchant convoy faces — raiding activity itself drops during Mare Clausum, a real, mutual seasonal lull rather than a one-sided restriction only the player's own shipping feels.
- **The Fenus Nauticum** (Economy & Finance §7.1) reads Mare Clausum directly: a maritime loan financing a voyage that sails during the closed season carries a real, appropriately steeper premium, reflecting the genuinely higher real risk being underwritten.

---

## 6. Disease Seasonality — The Autumn Fever

A real, well-documented ancient Mediterranean health pattern worth giving Disease & Public Health a concrete seasonal hook: what ancient sources describe as a recurring "autumn fever" — almost certainly malaria, though the ancient world didn't have that word for it — was a genuine, seasonally concentrated hazard, worst in late summer into autumn and specifically worse in Marsh/Poor-land terrain (Estate & Settlement §2's own existing elevated-Disease-risk terrain type) — the real, historical reason the Pontine Marshes near Rome itself carried such a lasting, well-earned reputation for sickness.

This document ties Disease & Public Health's own outbreak-risk math directly to this seasonal window: Marsh-adjacent settlements see real, elevated illness risk concentrated in late Aestas through Autumnus specifically, rather than a flat year-round baseline — giving that terrain's already-established risk a real, seasonal shape rather than a constant, undifferentiated hum.

---

## 7. The Hungry Gap — Winter Scarcity and Stores

A real, honest agrarian-society phenomenon worth naming directly: the lean period before a new harvest, when the previous year's stores are running low, is a real, recurring seasonal pressure point distinct from an acute Famine Disaster Event (Natural Disasters §2). This document ties it to late Hiems specifically — the stretch after autumn's own olive harvest has been fully consumed or sold and before spring planting has produced anything at all.

- **A household with adequate Horreum (granary) capacity and genuine stored surplus** (Buildings §4.x, already a named Drought/Famine countermeasure in Natural Disasters) simply weathers this window without incident — the concrete, seasonal payoff for having built and stocked that capacity in better months, rather than a countermeasure that only ever matters during a rare Disaster Event roll.
- **A household without adequate stores** faces a real, recurring, and entirely predictable annual dip — a modest Labor & Slavery Regimen Diet strain and a small Settlement Demographics Contentment cost — every single Hiems, distinct from and additive to any acute Famine Event that might separately land in a given year. This is a genuine, plannable seasonal cost rather than a random one, rewarding a player who actually manages stores across the year rather than only reacting to Disaster rolls.

---

## 8. Harvest Push — A Temporary Labor Reallocation

New this pass, and a direct payoff of §3's own concentrated-yield timing: each crop's real harvest window (Grain in summer, Grapes and Olives in autumn) creates a genuine, temporary labor demand spike Labor & Slavery's own ordinary Duty Slot assignments don't otherwise account for. **Harvest Push** is a standing, player-toggleable policy — reusing the same "player sets a standing decision, the simulation handles the rest" shape this project already uses for Regimen and Herd Strategy — temporarily reassigning labor from other, less time-critical Duty Slots (Household staff, non-essential Craft production) into the relevant Agriculture buildings for the real duration of that crop's own harvest window.

- **Accepting the Push** measurably boosts that harvest's actual yield, at the honest cost of whatever those reassigned workers would otherwise have been doing during that same window — a genuine, felt tradeoff rather than a free bonus.
- **Declining it** leaves the harvest at its ordinary baseline yield, with every other Duty Slot continuing uninterrupted — the safer, steadier choice for a household that can't afford the temporary disruption elsewhere.

This is the same real seasonal rhythm that made hiring extra hands at harvest time a genuine, recurring feature of real ancient agricultural life, given a small, concrete, optional mechanical shape rather than left as unmodeled flavor.

---

## 9. Seasonal Weighting of Natural Disasters

Natural Disasters already gestures at seasonal hazards without a unified table tying its full nine-hazard roster to an actual point in the year — this document supplies that table directly, formalizing what several entries already implied:

| Hazard | Peak Season | Why |
|---|---|---|
| **Fire** | Aestas | Dry-season tinder, already tied to Drought overlap (Natural Disasters §3.1) |
| **Flood** | Ver | Spring snowmelt and rain, distinct from Storm-driven flooding |
| **Drought/Famine** | Aestas | The dry-season peak |
| **Storm** | Autumnus–Hiems | The Mare Clausum window (§5) at its most dangerous |
| **Landslide** | Ver, Hiems | Wet-season slope saturation, compounding with Flood |
| **Blight & Infestation** | Ver–Aestas | The growing season itself, when a crop is actually vulnerable |
| **Frost** | Hiems, early Ver | Already established directly in Natural Disasters §5.4 |
| **Earthquake, Volcanic Eruption** | No seasonal weighting | Both remain genuinely unpredictable per that document's own §2.1–2.2, exactly as designed — this document doesn't retrofit a seasonal pattern onto hazards that were deliberately built without one |

This is a real weighting on top of that document's own existing Exposure math, not a replacement for it — a low-Exposure household's Aestas is still safer than a high-Exposure household's, the season simply shifts *when* the dice are more loaded rather than *whether* they are at all.

---

## 10. Markets, Prices & the Seasonal Cycle

The direct resolution of a gap two other documents already flagged without filling: Resources & Goods' own Market Dynamics (§12 of that document) and Economy & Finance's own Trade Routes section (§7) both already assume "seasonality" affects price and volume without either one actually defining it. This document is that definition:

- **Harvest-month glut.** In the specific month(s) a crop actually lands (§3), that good's own local supply spikes and its price dips accordingly — the concrete mechanism behind why a Nundinal market day (Roman Calendar §5) in the middle of the grape vintage looks nothing like one in late winter.
- **Hungry-gap scarcity.** The same good's price rises through Hiems as stores draw down toward §7's own lean window, peaking right before the next harvest — a real, predictable, and tradeable price cycle a player can plan around, buying low at harvest and knowing prices firm up before spring.
- **Mare Clausum's own price effect.** Imported goods reliant on sea trade (Resources & Goods' own Imported Goods category) see reduced availability and firmer prices during §5's own closed season, independent of any local harvest cycle.

---

## 11. Cross-System Integration

- **Roman Calendar:** this document's entire seasonal layer sits directly on top of that document's own month structure, adding no new date-tracking of its own.
- **Religion:** several existing feast days already cluster seasonally (a sowing-adjacent spring festival, a harvest-adjacent one) — this document doesn't touch that table, only confirms its existing placement already lines up naturally with §2's own season boundaries.
- **Resources & Goods, Estate & Settlement:** §3's harvest timing gives Cultivation Intensity a real calendar to operate against; §10 is the concrete definition behind that document's own previously-unspecified "seasonality" flag on Market Dynamics.
- **Military & Combat:** §4 gives the Estate Force, Muster, and Roman Service tracks a real seasonal rhythm they've always lacked; Winter Quarters is a legitimate, named posture rather than an absence of activity.
- **Travel, Piracy & Banditry:** §5 formally defines Natural Disasters' own already-named "closed sea" reference, and gives both documents a shared, mutual seasonal lull/danger window rather than a one-sided player restriction.
- **Economy & Finance:** the Fenus Nauticum (§7.1 of that doc) reads Mare Clausum directly for its own risk premium; §10's price cycle is real, plannable Trade Route texture.
- **Disease & Public Health:** §6 gives Marsh/Poor-land terrain's already-elevated Disease risk (Estate & Settlement §2) a real seasonal shape.
- **Buildings, Natural Disasters:** the Horreum's own already-established Drought/Famine countermeasure role (Natural Disasters §6.2, Companions & Court Positions' Horrearius) gets a second, entirely predictable annual payoff via §7's Hungry Gap, beyond only mattering during a rare Disaster Event roll.
- **Labor & Slavery:** §7's Diet-strain cost and §8's Harvest Push both reuse that document's own standing-policy pattern (Regimen, Herd Strategy) rather than inventing a new mechanic shape.
- **Natural Disasters:** §9 is a direct, formalizing extension of that document's own hazard table, resolving the gap between several already-seasonal-sounding entries (Storm, Frost) and the rest of the roster.

---

## 12. Data Model

```
SeasonalState {                  // computed off Roman Calendar's own current month, not separately tracked
  currentSeason,                  // "ver" | "aestas" | "autumnus" | "hiems"
  transitionProgress,               // 0.0-1.0 — supports §2's own soft, gradual boundary phase-in/out
}

HarvestWindow {                  // §3
  cropType,                        // "grain" | "grapes" | "olives"
  peakMonth,
  yieldConcentrationFactor,          // how much of the annual total lands in this window vs. spread flat
}

CampaignSeasonModifier {          // §4
  deploymentId,                     // pointer to Military & Combat's own active deployment record
  isWinterQuartersPosture: bool,
  moraleAndSupplyPenaltyActive: bool, // true only for an active campaign deep into Hiems
}

MareClausumState {                // §5
  active: bool,                      // true through Hiems per §2's soft boundary
  seaRouteRiskMultiplier,
  tradeVolumeMultiplier,
  fenusNauticumPremiumModifier,
}

HungryGapState {                  // §7
  settlementId,
  horreumStockSufficient: bool,
  dietStrainActive: bool,
  contentmentPenaltyActive: bool,
}

HarvestPushPolicy {               // §8 — a standing policy, mirroring Regimen's own shape
  settlementId,
  enabled: bool,
  activeForCropType,                 // nullable — only set during that crop's own real harvest window
}

SeasonalDiseaseWeighting {        // §6
  settlementId,
  isMarshAdjacent: bool,
  autumnFeverRiskMultiplier,          // elevated specifically late Aestas through Autumnus
}

SeasonalMarketModifier {          // §10
  goodKey,
  settlementId,
  currentPriceMultiplier,
  cause,                             // "harvestGlut" | "hungryGapScarcity" | "mareClausumImportScarcity" | "none"
}
```

---

## 13. Open Questions

- **All numeric sizing**, per this project's standing convention — every multiplier, penalty, and price effect named above (the winter-campaign Morale/Supply penalty, Mare Clausum's own risk multiplier, the Hungry Gap's Contentment cost, harvest-glut/scarcity price swings) is unsized.
- **Exact season-boundary dates.** §2's table gives rough month ranges deliberately rather than fixing precise Kalends-of-X cutoffs; whether the soft-transition model (§2, §12's `transitionProgress`) needs firmer edges for implementation clarity is a natural follow-up.
- **Whether Harvest Push (§8) should have a hard participation cap** — how many Duty Slots can actually be reassigned before other neglected duties start generating their own separate problems isn't specified.
- **Regional variation.** This document's seasonal calendar is written from an Italian-heartland-centric baseline; whether a genuinely different climate region (Egypt's Nile-flood-driven agricultural calendar, notably, ran on an entirely different real rhythm than the Mediterranean-standard one this document assumes) warrants its own distinct regional Seasonal calendar rather than inheriting this one wholesale is a real, flagged gap rather than a decided non-issue.
- **Whether Mare Clausum ever fully closes sea Travel** versus only ever elevating its risk. §5 deliberately keeps it a real, costly choice rather than a hard block, but a stricter, harder-gated version remains a legitimate alternative design if the softer version proves to not be felt strongly enough in play.
