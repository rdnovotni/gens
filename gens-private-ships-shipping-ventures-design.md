# GENS — System Design: Private Ships & Shipping Ventures (§6.43, new)
*Comprehensive expansion and polish pass. A real gap sitting in plain sight across three finished documents: Military & Combat built a Fleet of warships, Merchant Families & the Equestrian Order named "Shipping Magnates" as a real archetype, Land Ownership & Real Estate built the Societas specifically around a shipping venture's real risk, and Economy & Finance priced that risk with the fenus nauticum — but no individual merchant vessel has ever actually existed as a persistent, named, ownable thing. This document treats a Ship the way Land Ownership treats a Property Record or Notable Businesses treats a business: a real asset a household (or a Societas, or a fronting arrangement) actually owns, names, commissions, ages, insures, loses, and sometimes mourns. This pass adds real Custom Commissioning (choosing a hull, a build quality, a consecration), a genuine Flagship concept with its own elevated stakes, and seven further real vessel types spanning fast dispatch craft, horse transports, river ferries, and a household's own personal pleasure barge.*

---

## Contents

1. Scope & Role — Ships as a Real Owned Asset Class
2. The Ship Registry — Real Vessel Types Across Cultures
3. Custom Commissioning — Building a Ship to Order
4. The Flagship — A Household's Premier Vessel
5. Ownership — Sole, Societas, and Fronting
6. Voyage Resolution — Aggregate by Default, Discrete Where It Matters
7. Ship Condition, Aging & the Shipyard
8. Loss, Capture & Recovery
9. Ship Reputation — Blessed Launches and the "Lucky Ship"
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role — Ships as a Real Owned Asset Class

This document doesn't touch what already works: Military & Combat's Fleet (§4.1 of that document) remains the warship structure, built at the Shipyard/Navalia and crewed for combat; Economy & Finance's Trade Routes (§7) and fenus nauticum (§7.1) remain the trade-flow and financing layer; Resources & Goods' full Market Dynamics simulation stays exactly as designed. What's never existed is the actual **vessel** — a specific, named ship a household points at and says "that one's mine." This document is that asset class: a **MerchantShip** is a real, persistent record, ownable the same way a Property Record or a Notable Business is, distinct from and never confused with a warship in a military Fleet.

A household's collection of owned Ships forms its own **Merchant Marine** — a portfolio, following the exact pattern Land Ownership & Real Estate already established for a property portfolio (that document's §11): no hard cap, self-limiting through cost and oversight rather than an artificial wall.

---

## 2. The Ship Registry — Real Vessel Types Across Cultures

Per direction, this isn't one generic "Merchant Vessel" — real Mediterranean and adjacent seafaring traditions built genuinely different ships, and giving that variety its own registry (in the same spirit as Cultures of the Known World's own roster) gives a shipping household real texture to build an identity around. Seven further classes join the original roster this pass, rounding it out from general cargo-hauling alone into real specialized roles.

### 2.1 General Cargo Classes *(original roster, unchanged)*

| Vessel Class | Real Grounding | Capacity Tier | Typical Role |
|---|---|---|---|
| **Navis Caudicaria** | A real, attested Roman river-and-coastal barge type — flat-bottomed, modest, built for calm water rather than open sea | Low | The accessible entry point: cheap, low-risk, ideal for a Tiber-style river or short coastal hop rather than a genuine sea voyage |
| **Corbita** | The real, sturdy, round-hulled workhorse of Roman Mediterranean trade — sail-powered, broad-beamed, built for cargo capacity over speed | Standard | The default mid-tier seagoing merchantman — most of a shipping household's own Marine is this class |
| **Grain Carrier (Alexandrian type)** | Real ancient writers describe the great Alexandria-to-Rome grain ships as remarkably, famously enormous for their era — the single largest, most prestigious general-cargo vessel class this registry offers | High | The prestige capstone: expensive, high-capacity, and the natural vessel behind a household holding a Provincial Supply Contract's own Annona obligation (Public Contracts & Competitive Bidding §4.3) |
| **Punic Trading Vessel** | Real, historically excellent Phoenician/Punic seafaring tradition — the Punic culture's own real reputation for skilled, far-ranging maritime trade, predating and outlasting Carthage's own political fall | Standard | The North African Colony region's own natural flavor vessel, mechanically identical to a Corbita but carrying real cultural texture for a Punic-culture household |
| **Aegean Merchantman** | The Greek East's own real, long-standing maritime trade tradition | Standard | That region's own natural flavor equivalent, functionally a Corbita with Greek East cultural dressing |
| **Gallic/Britannic Coaster** | A real, historically distinct Atlantic-facing shipbuilding tradition — heavier, more robustly built than a Mediterranean vessel, suited to rougher northern coastal waters and Britannia's own cross-Channel trade | Standard, with elevated Storm resistance | The frontier's own natural vessel, trading some cargo efficiency for real, mechanical hardiness against the harsher water it actually operates in |
| **Red Sea/Nabataean Trader** | Ties directly to Arabia Felix and Egypt's own real Indian Ocean trade network | Standard to High | The natural vessel for the Egypt/Arabia Felix incense-and-spice trade route already established in those regions' own documents |

### 2.2 Specialized Classes *(new this pass)*

| Vessel Class | Real Grounding | Capacity/Speed | Typical Role |
|---|---|---|---|
| **Liburnian** | A real, genuinely fast, light galley type Rome adopted from Illyrian design, historically used for both naval patrol and urgent light dispatch | Low cargo, high speed | The fast-courier vessel: a natural sea-borne extension of Correspondence & Letters for a genuinely urgent message, or a light escort accompanying a more valuable, slower Corbita or Grain Carrier |
| **Actuaria** | A real oared-and-sailed cargo vessel — oars supplementing sail for real, meaningfully higher speed than a pure sailing ship, at the real cost of needing a larger, wage-drawing rowing crew | Standard cargo, elevated speed | The genuine speed-vs-cost tradeoff: a time-sensitive shipment (a perishable Food Culture good, an urgent Public Contracts delivery) worth paying real extra crew wages to move faster |
| **Ponto** | A real, attested flat-bottomed Roman vessel, historically used as a ferry or a floating pontoon crossing | Low, not a trade vessel | Not a cargo-hauler at all — a direct, cheaper alternative to Private Infrastructure's own private Bridge (§6 of that document) on a river Plot too minor to justify a full bridge |
| **Hippago** | A real, specifically attested Roman horse-transport ship, purpose-built with the stalls and ramps a live-animal cargo actually needs | Low cargo capacity, specialized | The concrete logistics vessel behind moving cavalry mounts (Military & Combat) or breeding-stock Horses (Resources & Goods) by sea rather than the abstract assumption that livestock simply arrives |
| **Nile Riverboat** | A real, distinct Egyptian riverine shipbuilding tradition, historically the actual backbone of the Nile's own grain and goods movement, predating and outlasting Rome's own arrival | Standard, river-only | Egypt's own natural default vessel for the region's own Nile-centered internal trade, distinct from a Red Sea/Nabataean Trader's external, Indian-Ocean-facing role |
| **Pontic Grain Trader** | A real, well-documented ancient pattern: the Bosporan Kingdom's own economy substantially depended on real, large-scale grain exports across the Black Sea to the Greek world | Standard to High | The Bosporan Kingdom region's own real counterpart to the Alexandrian Grain Carrier — a second, geographically distinct grain-export identity rather than treating grain shipping as an exclusively Egyptian phenomenon |
| **Personal Pleasure Barge** | A real, historically attested category of ornate, non-cargo vessels wealthy elites commissioned purely for personal leisure travel on a river or sheltered water | None — carries people and Dignitas, not goods | Not a trade asset at all: a genuine prestige and leisure vessel, the maritime counterpart to Villa's own Private Dock/Boathouse room, built purely to be seen and enjoyed rather than to earn a return |

A household is never required to diversify across these — a Corbita-only Marine is a completely legitimate, ordinary shipping operation. The specialized classes exist for the household that wants a real, functional reason to own more than one kind of vessel.

---

## 3. Custom Commissioning — Building a Ship to Order

A Ship is never simply pulled off an implicit market shelf: every vessel in a household's own Marine is **commissioned** at the Shipyard/Navalia (Buildings §4.11, extended per §7 below to cover merchant vessels alongside its existing warship remit), a real, deliberate construction project rather than an instant purchase.

### 3.1 The Commission

Commissioning a Ship is a genuine choice across three real axes, kept simple rather than a full shipwright minigame:

- **Hull Class** — any entry from §2's registry, gated by the settlement's own region/culture the way Estate & Settlement already gates buildings by terrain.
- **Build Quality** — Common, Fine, or Exceptional, reusing Resources & Goods' own existing three-grade Quality system (§10 of that document) rather than inventing a fourth scale: a higher Build Quality raises the new Ship's own starting Condition ceiling and its long-run resistance to ordinary wear, at a real, proportionally higher commissioning cost.
- **Decoration** — a carved figurehead, a painted eye or protective emblem on the bow (a real, widely-attested ancient maritime practice), or a dedication to a specific protective deity — purely a Dignitas-and-flavor layer, mechanically light, but a real opportunity for Villa-style personalization on an asset that otherwise risks reading as a spreadsheet line.

### 3.2 The Launch — A Real Consecration

A real, historically well-attested ancient practice worth building in directly rather than skipping past: the **Isidis Navigium**, a real Roman-Egyptian religious festival marking the reopening of the sailing season with an actual ceremonial ship procession, reflects a genuine, felt ancient anxiety about entrusting a vessel and its crew to the sea. A newly commissioned Ship can be given a formal **Consecrated Launch** — a real Religion (§6.6) Funded Action dedicating the vessel to a protective deity (the household's own Patron Deity, or a specifically maritime-associated one such as Isis in her sailors'-protector aspect) — granting the new Ship a small, permanent **Blessed Launch** flag: a modest, standing reduction in future Voyage Event severity, distinct from and stacking with whatever "lucky ship" reputation (§9) it might go on to earn through its own actual service record. A Ship launched without this ceremony suffers no penalty — it's a real, optional, devotional choice, not a mandatory tax on ownership.

---

## 4. The Flagship — A Household's Premier Vessel

A household with more than one Ship can formally designate one — almost always its largest, finest, or most elaborately commissioned — as its **Flagship**: the acknowledged premier vessel of its Marine, carrying real weight beyond an ordinary Ship's own Reputation.

- **Elevated Dignitas.** A Flagship is real, standing Dignitas material simply by existing prominently in a household's own portfolio — a named, notable vessel reads the way a particularly grand Villa room does, a visible, legible statement of the household's own maritime standing.
- **The natural vessel for Travel by sea.** A household's own paterfamilias or a family member undertaking Travel (§6.18) by sea naturally embarks on the Flagship where one exists, reading meaningfully better in reception terms than chartering ordinary passage or sailing aboard a plain Corbita — the concrete payoff for commissioning something genuinely fine rather than merely functional.
- **A real diplomatic and ceremonial venue.** A Flagship is a plausible, real setting for hosting an important arrival, a treaty negotiation (Diplomacy with Non-Roman Peoples), or a Politics & Patronage patronage event held aboard rather than in the Triclinium — a genuine, distinctive alternative venue for a household wanting to make a specific kind of impression.
- **Heavier stakes if lost.** Precisely because a Flagship carries this much accumulated prestige, its loss (§8) is treated with real, elevated weight — a guaranteed Dynasty Chronicle entry and a real, sharper Dignitas hit than an ordinary Ship's own loss, the honest cost of putting a household's own best vessel forward rather than keeping its finest asset safely in port.

A household can re-designate a new Flagship at any time — retiring the old title to whichever vessel currently best earns it — but only ever holds one at a time.

---

## 5. Ownership — Sole, Societas, and Fronting

Three real, distinct ownership shapes, matching how a household or investor group actually holds a Ship:

- **Sole Ownership** — the straightforward default: the household's own Treasury bought it, the household's own name is on it.
- **Societas Ownership** — Land Ownership & Real Estate's own Societas Unius Rei (§7 of that document), recapped rather than rebuilt: multiple investors pool capital specifically to buy and operate one or several Ships, splitting profit and loss by agreed share, carrying that document's own real unlimited-liability stake if the venture goes badly.
- **Fronting** — the *lex Claudia de nave senatorum*'s own real historical teeth, already named as a motivation in Land Ownership §7 and given a light but real mechanic here: a senatorial household's Ship is registered to a freedman Operator or a Societas the senator quietly controls, rather than to the senator's own name directly. This works cleanly under ordinary play — but if the arrangement is ever publicly exposed (an Espionage discovery, a Legal & Court proceeding, an unrelated Scandal pulling on the same thread), the real senator behind it faces a genuine, additional Scandal/Dignitas consequence layered on top of whatever the exposure event itself already causes, reusing Scandal's own existing engine rather than building a parallel discovery-and-counterplay system just for this.

---

## 6. Voyage Resolution — Aggregate by Default, Discrete Where It Matters

Per direction: mostly aggregate, with real discrete stakes reserved for when they're actually earned.

### 6.1 The Default — Aggregate

A Ship assigned to an ordinary, already-established Trade Route (Economy & Finance §7) simply contributes its own Capacity Tier and Condition as a direct multiplier on that route's existing aggregate monthly output. No per-voyage roll, no monthly "did it arrive" check for routine trade — consistent with that document's own explicit "primarily passive" steer, now with a real asset behind the number instead of an abstract flow.

### 6.2 The Exception — Discrete Voyage Events

A Ship undertaking a genuinely high-stakes voyage resolves instead as a real, discrete **Voyage Event** — Arrived Safely, Damaged, Lost to Storm, Lost to Piracy, or Captured. This applies specifically when:

- The voyage runs Economy & Finance's own named higher-risk luxury route (§7 of that document), rather than an ordinary steady one.
- The voyage is financed by a fenus nauticum (§7.1 of that document) — real money is riding on a binary outcome, and that deserves an actual roll rather than folding into an aggregate average.
- The voyage carries a specific, named, one-off cargo of real narrative weight — a dowry shipment, a Provincial Supply Contract's own Annona delivery, a particularly valuable single consignment.
- The vessel is the household's own Flagship (§4), regardless of cargo — its elevated stakes mean any voyage it undertakes is worth resolving discretely rather than folding into the background average.

This keeps the game from rolling dice on every unremarkable grain shipment while still giving the genuinely dramatic voyages real, felt stakes.

---

## 7. Ship Condition, Aging & the Shipyard

A Ship ages and accumulates wear the same way an Estate & Settlement building does, reusing that document's own condition/decay/Repair pattern rather than inventing a parallel one — repaired at the Shipyard/Navalia, which this document extends to also build and repair merchant vessels alongside its existing warship remit. A poorly-maintained, aging Ship carries a real, elevated Voyage Event risk on top of whatever the route itself already exposes it to — the same honest logic behind Public Contracts & Competitive Bidding's own contract-fraud precedent, where a corner-cutting contractor's real risk was sending an unseaworthy vessel out regardless. A Ship no longer worth repairing can be sold — to another household, to a Notable Business, or simply scrapped for whatever residual material value remains — following Land Ownership & Real Estate's own existing sale mechanics rather than a bespoke one.

---

## 8. Loss, Capture & Recovery

- **Storm loss** resolves through Natural Disasters' existing Storm hazard (§5.3 of that document), now landing on a real, named Ship record instead of an abstract "Cargo & Ships" note.
- **Piracy loss or capture** resolves through Piracy & Banditry's existing raid machinery. A **Captured** outcome is genuinely distinct from a total loss: the vessel, its cargo, and any crew or passengers aboard become that document's own kidnap-and-ransom object (§8 of that document) — the Ship itself can potentially be ransomed back, sold at a slave/prize market by its captors, or absorbed into a Confederation's own raiding fleet outright.
- **Fenus nauticum resolution** — recapped from Economy & Finance §7.1: a Ship lost while financed this way simply forgives the associated debt; it never triggers that document's own default ladder.
- **Presumed Lost** — a real, evocative, and historically honest resolution in its own right: a Ship that simply never returns from a voyage, with no confirmed Storm or Piracy cause ever established, is a legitimate and genuinely atmospheric outcome distinct from either — the ancient anxiety of a ship that simply never came home, and no one ever learned why.
- **A Flagship's loss** (§4) carries the elevated stakes named there regardless of which of the above outcomes actually claims it.

---

## 9. Ship Reputation — Blessed Launches and the "Lucky Ship"

Two real, distinct sources feed a Ship's own standing reputation, worth keeping separate rather than merging into one generic score:

- **A Blessed Launch** (§3.2) is granted once, at commissioning, through deliberate religious investment — a real, chosen act, not something earned through service.
- **A "lucky ship" reputation** accrues gradually, the way Notable Businesses' own Reputation (§4 of that document) sits distinct from its owner's personal standing: a Ship surviving a long run of successful voyages earns this real, if imprecise, sailors' superstition organically, carrying a small, earned reduction in its own future Voyage Event risk and a modest Dignitas value to its owner simply from operating a long-proven vessel. A Ship can hold both a Blessed Launch and an earned lucky reputation at once — the pious beginning and the proven track record reinforcing each other rather than competing.
- Conversely, a Ship that's suffered repeated bad voyages can just as easily earn the opposite reputation — a genuinely harder ship to crew, with a Sailor-trait Character more reluctant to sign aboard a vessel with a bad name.
- A sufficiently long-lived, storied Ship, or the captain who commands one, is real Celebrities & Influential Figures material (§2 of that document) — Fame accruing to the vessel or its Navarchus, the maritime counterpart to Food Culture's own Named Cook. A Flagship carrying a strong lucky reputation is a particularly natural candidate.

---

## 10. Cross-System Integration

- **Military & Combat:** the Fleet (§4.1 of that document) is explicitly and permanently distinguished from this document's MerchantShip — warships versus commercial vessels, never conflated; the Hippago (§2.2) is this document's own concrete link to that system's cavalry-mount logistics.
- **Economy & Finance:** Trade Routes (§7) and the fenus nauticum (§7.1) both now attach to a real, persistent Ship record rather than an abstract shipment.
- **Land Ownership & Real Estate:** Societas Unius Rei (§7 of that document) is this document's own multi-investor ownership vehicle, recapped rather than rebuilt; the *lex Claudia* (also §7 of that document) is given real, if light, mechanical teeth via §5's Fronting mechanic.
- **Merchant Families & the Equestrian Order:** the Shipping Magnate archetype (§4 of that document) finally has a concrete asset class to actually operate.
- **Buildings / Estate & Settlement:** the Shipyard/Navalia is extended to build and repair merchant vessels alongside its existing warship remit; §3's Custom Commissioning is this document's own construction-project analogue to that document's own building system.
- **Private Infrastructure:** the Ponto (§2.2) is a real, cheaper alternative to that document's own private Bridge (§6 of that document) on a minor river crossing.
- **Piracy & Banditry:** a Ship is now a real, named, capturable target, giving that document's raiding and Contracted Raids mechanics (§7 of that document) a concrete object rather than an abstract "shipment"; a captured Ship's cargo and crew are subject to that document's existing kidnap/ransom/enslave machinery (§8).
- **Natural Disasters:** Storm's existing "Cargo & Ships" note (§5.3 of that document) now resolves against a real Ship record.
- **Public Contracts & Competitive Bidding:** the Grain Carrier and Pontic Grain Trader classes are the natural vessels behind fulfilling a Provincial Supply Contract's own Annona obligation, from Egypt and the Bosporan Kingdom respectively.
- **Notable Businesses:** a shipping-focused Notable Business's own named suppliers and competitors can be, concretely, other households' MerchantShips.
- **Religion:** §3.2's Consecrated Launch is a real, named Funded Action extension, tying directly to the Cult of Isis's own real maritime-protector association where that cult is active.
- **Correspondence & Letters / Travel:** the Liburnian (§2.2) is this document's own fast sea-borne dispatch option; the Personal Pleasure Barge (§2.2) and the Flagship (§4) both give sea Travel a real, prestige-bearing alternative to chartered or ordinary passage.
- **Villa:** the Personal Pleasure Barge is the natural maritime counterpart to the Private Dock/Boathouse room (Villa §4.7).
- **Scandal:** an exposed Fronting arrangement (§5) is a real, new Scandal source.
- **Celebrities & Influential Figures:** a storied Ship or its Navarchus is real Fame material (§9).
- **Companions & Court Positions:** the Navarchus remains this document's own named per-ship (or per-Marine) command role.
- **Cultures of the Known World:** §2's registry ties specific vessel classes to Punic, Greek East, Gallic/Britannic, Egyptian, Bosporan, and Egypt/Arabia Felix cultural identity directly.
- **Diplomacy with Non-Roman Peoples / Politics & Patronage:** §4's Flagship is a real, distinctive ceremonial and diplomatic venue for either system's own hosting needs.
- **Dynasty Chronicle:** a Ship's dramatic loss, a Presumed Lost mystery, a long-lived "lucky ship" finally retired in honor, and any Flagship's own loss or launch are all real, natural Chronicle entries.

---

## 11. Data Model

```
MerchantShip {
  shipId, name,
  vesselClass,            // "navisCaudicaria" | "corbita" | "grainCarrier" | "punicTrader" |
                            // "aegeanMerchantman" | "gallicBritannicCoaster" | "redSeaNabataeanTrader" |
                            // "liburnian" | "actuaria" | "ponto" | "hippago" | "nileRiverboat" |
                            // "ponticGrainTrader" | "personalPleasureBarge"
  capacityTier,             // "none" | "low" | "standard" | "high" — read from vesselClass
  buildQuality,             // "common" | "fine" | "exceptional" — §3.1
  isFlagship: bool,          // §4 — at most one true per household
  ownerType,                // "sole" | "societas" | "fronted"
  registeredOwnerOfRecordId,  // the freedman/Societas id if fronted, per §5
  actualOwnerHouseholdId,      // the real beneficial owner — always tracked, regardless of ownerType
  condition,                 // reuses Estate & Settlement's own condition scale
  navarchusId,               // the assigned captain, a Character id
  assignedTradeRouteId,
  blessedLaunch: bool,         // §3.2 — set once, at commissioning, never revoked
  reputationTier,             // "none" | "luckyShip" | "badReputation" — §9, distinct from blessedLaunch
  voyagesCompleted,
  status,                    // "active" | "damaged" | "presumedLost" | "lostToStorm" | "lostToPiracy" |
                              // "captured" | "retired" | "sold"
  fenusNauticumRecordId,       // nullable — set only when §6.2's financing applies
}

ShipCommission {              // §3
  commissionId, shipIdOnceComplete,
  hullClass, buildQuality, decorationChoice,
  monthsInProgress, laborAssigned,
  consecratedLaunchRequested: bool,
}

VoyageEvent {                 // §6.2 — only generated for a qualifying high-stakes voyage
  eventId, shipId, month,
  triggerReason,              // "luxuryRoute" | "fenusNauticumFinanced" | "namedSignificantCargo" | "isFlagship"
  outcome,                    // "arrivedSafely" | "damaged" | "lostToStorm" | "lostToPiracy" | "captured" | "presumedLost"
  cargoValue,
}

MerchantMarine {              // §1 — a household's own Ship portfolio, distinct from a military Fleet
  householdId,
  shipIds: [ ... ],
  flagshipId,                  // nullable — §4
}

FrontingArrangement {         // §5
  shipId, realOwnerHouseholdId, frontingPersonOrSocietasId,
  exposed: bool,
  exposureScandalRef,
}
```

---

## 12. Open Questions

- **All numeric sizing**, per convention — vessel class costs/capacity figures, Build Quality's own Condition-ceiling premium, Voyage Event probability weighting, and the Blessed Launch/lucky-ship/bad-reputation risk modifiers are all unsized.
- **Whether a captured Ship can ever be recovered** through a paid ransom or a Military & Combat naval action against its captors, versus being permanently lost to the household's own Marine once captured — §8 leaves this open.
- **Full vessel-class coverage.** §2's registry names the distinctive cases rather than a bespoke hull for all 36 Cultures of the Known World; whether further cultures eventually warrant their own named class isn't resolved.
- **Fronting exposure's actual trigger conditions.** §5 names Espionage, Legal & Court, and Scandal as plausible discovery paths without specifying relative likelihood or a formal detection roll.
- **Flagship re-designation cost.** §4 allows a household to name a new Flagship at any time, but doesn't specify whether doing so carries any real transition cost or ceremony of its own, versus being a free, instant administrative choice.
- **Commissioning time relative to Estate & Settlement's own building-construction timeline.** §3 assumes a Shipyard project scales similarly to a large building per that document's §4, but the actual relative duration isn't specified.
- **Interaction with Public Contracts & Competitive Bidding's own contract-fraud mechanic.** §7 draws a parallel between an aging, corner-cut Ship and that document's own contractor fraud, but doesn't formally merge the two into one shared risk calculation.
