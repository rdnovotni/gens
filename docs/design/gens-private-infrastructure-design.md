# GENS — System Design: Private Infrastructure (§6.42, new)
*Polish pass. Estate & Settlement's own Infrastructure category has sat as a two-item stub (Road, Bridge) since that document's first pass, and its own Open Questions explicitly named three real mechanics and set every one of them aside: "a formalized irrigation bonus, formalized road/river/coast trade-proximity bonuses, [and] building-to-building adjacency bonuses... were all considered during this polish pass and set aside — not currently part of the scope." This document is where all three finally get built — the household's own physical capital improvements to its land, distinct from Villa's personal living space and Public Works & Euergetism's civic infrastructure for the whole settlement. This pass adds real upkeep and disaster vulnerability for every infrastructure type, a lighter Well/Cistern alternative to a full Irrigation Canal, the honest double-edged reading of a well-connected estate (a Rival House or raider moves through it just as easily as a merchant does), a direct tie to Policies & Edicts' Frontier Security Posture, and a Chronicle-worthy milestone for a genuinely unified estate.*

---

## Contents

1. Scope & Role — The Household's Own Land, Not the Settlement's
2. Private Roads — Tiered, and Finally Doing Something
3. Irrigation — The Formalized Bonus
4. Adjacency — A Real, Lightweight Bonus
5. Land Reclamation — Improving Poor Terrain
6. Private Bridges & River Crossings
7. Boundary & Security Infrastructure
8. Maintenance & Disaster Vulnerability
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role — The Household's Own Land, Not the Settlement's

Three existing documents already own the neighboring territory, and this one deliberately stays out of all three: Villa owns the household's own interior living space; Public Works & Euergetism owns civic infrastructure funded for the whole settlement's population and built through Public Contracts & Competitive Bidding's own Redemptores; Estate & Settlement's Buildings taxonomy owns the production chains themselves. What's never had a real home is the physical connective and improvement layer sitting *underneath and between* a household's own Plots — the roads linking one field to the next, the water actually reaching them, the poor terrain slowly turned into something better, and the boundaries securing all of it. Per Estate & Settlement's own §3.1 public/private split, Infrastructure was always classified private; this document is simply the first real depth pass on what that classification was always waiting for.

---

## 2. Private Roads — Tiered, and Finally Doing Something

Estate & Settlement's own Infrastructure entry named a single-tier Road ("connects plots, boosts Commerce/Travel efficiency") without ever specifying the actual boost. This document gives it real tiers and a real formula:

- **Dirt Track** — the free, implicit baseline connecting any two adjacent Plots; no construction needed, no bonus beyond simple traversal.
- **Paved Road** — a genuine construction project (Resources & Goods' Pozzolana/Concrete Works materials, real Roman road-engineering flavor), connecting two or more Plots with a real, formalized logistics bonus: goods move between a Paved-Road-connected Field, Granary, and Market with reduced friction, read as a direct efficiency multiplier on Estate & Settlement's own goods-to-income conversion (§3.1 of that document) for any chain spanning connected Plots.

### 2.1 The Trade-Proximity Bonus, Resolved

Estate & Settlement's own terrain table already gestured at River being "a natural trade route" and Coast carrying "trade-route Commerce bonuses" without ever formalizing either. This document closes that gap directly: a Plot that is River-adjacent, Coast-adjacent, or connected via Paved Road to one that is, carries a real, formalized Commerce/logistics bonus feeding Economy & Finance's own Trade Route effectiveness — the same real-world logic that made Roman river and coastal settlements consistently wealthier than inland ones without road access, now an actual mechanical fact rather than flavor text.

### 2.2 The Honest Double Edge

Worth stating plainly rather than leaving as an unexamined pure upside, consistent with Design Pillar #1's "no dominant setting, only tradeoffs": a Paved Road speeds up whoever's using it, not only the household's own goods carts. A well-connected estate is a genuinely easier one for a Rival House's Private Feud (Military & Combat §6) to move through quickly, and Piracy & Banditry's land-based raiders reach a Paved-Road-connected Plot faster than an isolated one at the end of a Dirt Track. This isn't a reason to avoid building roads — the Commerce and logistics upside is real and usually worth it — but a household investing heavily in §2's own network without a matching look at §7's Boundary infrastructure or Policies & Edicts' Frontier Security Posture (§2.12 of that document) is making a real, legible bet, not a free lunch.

### 2.3 A Note on Regional Flavor

No new mechanic here, but worth naming: Latium's own real historical density of major roads (the *Via Appia* chief among them) makes a fully paved, tightly-clustered Road network the natural, thematically appropriate default for a Latium household, while a Gallic Frontier or Iberian Colony estate starting on cheaper, rawer land (Estate & Settlement §6) more plausibly begins on Dirt Tracks alone, with a mature Paved network read as a real, earned sign of the frontier estate's own maturation over time.

---

## 3. Irrigation — The Formalized Bonus

The most directly-named of Estate & Settlement's own deferred items. A private **Irrigation Canal** — built on a River-adjacent Plot, or fed by a private branch off the settlement's own civic Aqueduct where no river is available — is this document's concrete answer:

- **Soil Fertility recovery** (Natural Disasters §4.1) is meaningfully faster on an irrigated Plot regardless of the active Cultivation Intensity setting, a real, direct counterweight to Intensive Monoculture's own drain.
- **Drought/Famine severity** (Natural Disasters §2) is reduced specifically on irrigated Plots — the concrete, felt payoff for the investment, and the real historical reason irrigated land commanded a premium.
- Irrigation doesn't replace Cultivation Intensity's own tradeoff (§4.4 of that document) — it sits alongside it as a real, separate lever, not a way to avoid that choice's consequences entirely.

### 3.1 A Lighter Alternative — Wells & Cisterns

Not every Plot sits near a river or within reach of an Aqueduct branch, and this document doesn't want Irrigation's real benefit locked away from an estate that simply lacks either. A **Well** (cheap, single-Plot, no River/Aqueduct requirement) or a larger **Cistern** (a genuine rainwater-capture structure, a step up in both cost and effect) gives any Plot a real, if more modest, version of §3's own Fertility-recovery and Drought-mitigation benefit — the honest, lower-ceiling option for a household whose land simply isn't positioned for a full Irrigation Canal, rather than that household being locked out of this mechanic entirely.

---

## 4. Adjacency — A Real, Lightweight Bonus

Estate & Settlement's own Open Questions named "building-to-building adjacency bonuses" and set them aside — reasonably, since a full per-pair adjacency grid (the classic city-builder trap) would be exactly the kind of tedious bookkeeping this project's own conventions try to avoid. This document resolves the underlying want without the tedium, by reusing §2's own Road network rather than inventing a parallel grid:

**Road Clusters.** Any set of three or more Plots connected to each other by Paved Road, regardless of shape or arrangement, forms a **Road Cluster**, and the cluster as a whole — not any individual pair — carries a single, modest **Connected Estate** bonus (a small aggregate efficiency lift across every building in the cluster) once it reaches that three-Plot threshold. Growing the cluster further doesn't compound the bonus again; it simply keeps more of the estate inside the one bonus that already exists. This gives the real, intuitive feeling of "a well-connected estate runs better" without asking the player — or the game — to evaluate a single adjacency relationship for every possible pair of buildings.

### 4.1 A Real Milestone — The Unified Estate

New this pass: a Road Cluster that eventually comes to include *every* Plot the household currently holds is a genuine, visible achievement worth more than its own Connected Estate bonus alone — a real, Dynasty Chronicle-eligible milestone ("the whole estate, joined by road") distinct from and additional to the mechanical benefit, the same way Estate & Settlement's own stage transitions are treated as a deliberate, celebrated moment rather than a silent threshold crossing.

---

## 5. Land Reclamation — Improving Poor Terrain

A genuinely new capital project, distinct from any ordinary Agriculture-chain upgrade: **Land Reclamation** is a costly, slow, deliberate infrastructure investment that can improve a Marsh/Poor-land Plot's own underlying terrain classification, rather than merely building a better structure on top of what's already there.

### 5.1 Real Historical Grounding — and Its Real Limits

Worth building honestly rather than as a guaranteed win: Rome's own real, centuries-long ambition to drain the Pontine Marshes near Rome itself is genuinely well documented — repeatedly attempted, only ever partially successful across the actual historical record, and not fully resolved in antiquity at all. This document treats Land Reclamation the same honest way: a real, substantial, multi-month project drawing heavily on Labor and denarii, with a real chance of landing as only a **Partial Reclamation** — a genuine, permanent improvement (reduced Disease Exposure per Natural Disasters, a modest yield floor raised) that stops short of fully converting the Plot to Fertile Plain — rather than a guaranteed full terrain change every time. A **Full Reclamation** remains possible and is a genuine, rare achievement worth real Dignitas and a Chronicle entry when it lands, but this document deliberately doesn't promise it as the default outcome.

---

## 6. Private Bridges & River Crossings

Estate & Settlement's existing single-tier Bridge (river-plots-only) is recapped and given its real scale distinction: this is a **private** crossing connecting two of the household's own Plots across a river, distinct from Public Works & Euergetism's civic Bridge (§3 of that document), which connects entire Districts and unlocks new acquisition territory at the settlement scale. A household can hold both — a private bridge knitting its own estate together, and, separately, support or benefit from a civic one the settlement or a rival patron funds.

---

## 7. Boundary & Security Infrastructure

A real, functional counterpart to the existing Terminus Stone (a Monument — cheap, common, purely prestige-and-flavor per Monuments & Legacy Building §2.3): a **Boundary Wall/Fence** is this document's own working infrastructure investment, distinct from that Monument in being mechanical rather than commemorative. A fenced or walled Plot carries a real, direct reduction in Piracy & Banditry's livestock-rustling risk (that document's own raid category, Resources & Goods §3.2) and gives Labor & Slavery's own Regimen "Permitted Freedoms" axis a real physical backing on plots where confinement is the active policy, rather than that axis operating as a pure administrative setting with no felt physical presence on the land itself.

### 7.1 Frontier Security Posture, Made Physical

A direct tie worth drawing out: Policies & Edicts' own Frontier Security Posture (§2.12 of that document) already governs how the household's Estate Force itself is postured — Fortify, Patrol, or Minimal Garrison — as an abstract standing choice. A household running Fortify has a real, concrete reason to also invest in this document's own Boundary Wall network specifically, the physical estate-scale expression of the same defensive lean, the way the real Roman frontier *limes* system paired standing garrisons with actual, physical boundary works rather than soldiers alone. Neither document requires the other, but a Fortify-postured household that never builds a single Boundary Wall is leaving an obvious, thematically consistent investment on the table.

---

## 8. Maintenance & Disaster Vulnerability

Per this project's own standing convention (Buildings' condition-and-decay system, Public Works & Euergetism's own upkeep obligation), none of this document's infrastructure is a one-time, maintenance-free purchase:

- **Ordinary upkeep.** A Paved Road, Irrigation Canal, Well/Cistern, private Bridge, and Boundary Wall each carry a modest, real, recurring upkeep cost, folded into Economy & Finance's expense total exactly like any Estate & Settlement building's own upkeep. Neglect degrades the improvement's own effect over time, read against the same `condition` field Estate & Settlement's Plot data model already tracks, recoverable through the same Repair action.
- **Disaster vulnerability.** This infrastructure isn't shielded from Natural Disasters' own existing hazard rolls: a Flood can wash out a Paved Road segment or silt up an Irrigation Canal; a Landslide on a Hills-adjacent Plot can damage or destroy a Boundary Wall built there; an Earthquake can drop a private Bridge's own condition sharply, mirroring how that document's Severity Tiers already resolve against ordinary buildings. None of this needs a parallel disaster system — it's the exact same Exposure-and-Severity machinery Natural Disasters already runs, simply extended to cover this document's own new structures rather than only Estate & Settlement's production buildings.

---

## 9. Cross-System Integration

- **Estate & Settlement:** this document is the direct, full depth extension of that document's own Infrastructure category stub, and formally resolves all three items its own Open Questions explicitly deferred (irrigation bonus, road/river/coast trade-proximity bonus, adjacency bonus).
- **Natural Disasters:** Irrigation (§3) directly feeds Soil Fertility recovery (§4.1 of that document) and reduces Drought/Famine severity; Land Reclamation (§5) permanently shifts a Plot's own hazard-exposure profile; §8 extends that document's own Severity/condition machinery to every structure this document adds.
- **Resources & Goods:** Land Reclamation is the one mechanism that can genuinely change a Marsh/Poor-land Plot's own long-term identity, rather than working within what Reed Bed and Goat Pasture already make of it.
- **Public Works & Euergetism / Public Contracts & Competitive Bidding:** explicitly distinguished by scale and funding — civic Roads and Bridges are settlement-wide, state- or patron-funded, and built via a Redemptor's competitive bid; this document's Roads and Bridges are private, single-estate-scale, and built directly by the household with no bidding process involved.
- **Land Ownership & Real Estate:** a Plot's own private infrastructure investment is a real, direct input to that document's own Property Value (§9 of that document), alongside District trend and income history.
- **Economy & Finance:** Paved Road, Irrigation Canal, Well/Cistern, and Land Reclamation construction are all real Capital Expenditure line items (§4.4 of that document); §2.1's trade-proximity bonus is a direct, formalized input to that document's own Trade Route effectiveness.
- **Travel:** a well-developed private Road network (§2) reduces internal friction moving between the household's own Plots and its Villa, distinct from and complementary to Public Works' own settlement-to-settlement Road improvements, which remain that document's territory for external Travel efficiency.
- **Piracy & Banditry / Military & Combat:** §2.2's double-edged Road reading and §7's Boundary Wall are this document's two concrete, opposed levers on raid and Private Feud exposure.
- **Policies & Edicts:** §7.1 draws a direct, concrete line to Frontier Security Posture (§2.12 of that document) — the physical complement to that document's own abstract military-posture dial.
- **Labor & Slavery:** §7's Boundary Wall gives the Regimen's Permitted Freedoms axis a real physical presence on the land itself.
- **Monuments & Legacy Building:** the Terminus Stone (§2.3 of that document) is explicitly distinguished from §7's functional Boundary Wall — commemorative marker versus working infrastructure.
- **Dynasty Chronicle:** a Full Reclamation (§5.1) and a completed Unified Estate (§4.1) are both genuine, rare, Chronicle-worthy achievements.

---

## 10. Data Model

```
RoadCluster {                      // §2, §4
  clusterId, settlementId,
  plotIds: [ ... ],
  isPavedThroughout: bool,
  connectedEstateBonusActive: bool,   // true once plotIds.length >= 3
  isUnifiedEstate: bool,               // §4.1 — true once plotIds covers every Plot the household holds
}

IrrigationCanal {                   // §3
  plotId,
  sourceType,                        // "riverAdjacent" | "privateAqueductBranch"
  fertilityRecoveryBonus, droughtSeverityReduction,
}

WellOrCistern {                     // §3.1
  plotId,
  type,                              // "well" | "cistern"
  fertilityRecoveryBonus, droughtSeverityReduction,   // both lower-magnitude than IrrigationCanal's own
}

LandReclamationProject {              // §5
  plotId, startMonth,
  fromTerrain,                        // "marshPoorLand"
  targetOutcome,                       // "partialReclamation" | "fullReclamation" — resolved, not chosen, per §5.1
  monthsInvested, laborAssigned,
  status,                              // "inProgress" | "completedPartial" | "completedFull"
}

PrivateBridge {                     // §6
  bridgeId, connectedPlotIds: [ plotId, plotId ],
  riverCrossing: bool,
}

BoundaryInfrastructure {              // §7
  plotId,
  type,                               // "fence" | "wall"
  rustlingRiskReduction,
  confinementBacking: bool,             // true if Regimen Permitted Freedoms is set to Confined/Restricted here
  pairedWithFortifyPosture: bool,        // §7.1
}

InfrastructureCondition {              // §8 — one entry per built structure above
  structureId, structureType,           // "pavedRoad" | "irrigationCanal" | "wellOrCistern" | "privateBridge" | "boundaryInfrastructure"
  condition,                            // reads the same scale as Estate & Settlement's Plot condition field
  lastDisasterEventRef,
}
```

---

## 11. Open Questions

- **All numeric sizing**, per convention — Paved Road's own cost/bonus magnitude, the Connected Estate bonus's actual size, Irrigation's and the Well/Cistern's Fertility/Drought reduction figures, and every upkeep cost in §8 are all unsized.
- **Land Reclamation's Partial-vs-Full resolution odds.** §5.1 establishes both outcomes are real and possible, deliberately mirroring the Pontine Marshes' own historical difficulty, but the actual probability weighting (and whether continued investment after a Partial result can eventually push toward Full) isn't specified.
- **Road Cluster threshold tuning.** §4 sets the bonus trigger at three connected Plots as a deliberately simple, round number; whether that's the right threshold for a very large, sprawling estate versus a compact one isn't tested.
- **Irrigation Canal and Well/Cistern capacity.** §3 and §3.1 don't specify whether a single structure can serve multiple Plots or whether each Plot needs its own dedicated construction.
- **Disaster reroll granularity.** §8 extends Natural Disasters' existing machinery directly, but doesn't specify whether this document's own structures should be checked every time their host Plot suffers an Event, or only for hazard types plausibly relevant to that structure type (a Drought, for instance, having no obvious mechanism for damaging a Boundary Wall).
- **Interaction with a second settlement.** Land Ownership & Real Estate's own Administrative Burden mechanic already covers oversight cost at scale; whether Private Infrastructure investment should be visible or comparable across two separately-managed settlements isn't addressed here.
