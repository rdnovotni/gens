# GENS — System Design: Notable Households (§6.26.1, new)
*The missing middle tier between Settlement Demographics' pure aggregate pop-group math and Familia's fully-tracked individual roster — built on the exact pattern Rival Houses already established one rung up: a small, curated sample of named, real households given genuine depth, drawn out of the same aggregate population that keeps running underneath them unchanged. Not every potter's family in the city — a representative handful the player can actually see, name, and watch age, marry, struggle, and occasionally rise.*

---

## Contents

1. Scope & Role
2. The Notable Household Record
3. Sampling — How a Household Gets Named
4. The Household Lifecycle — Formation, Aging & Inheritance
5. Employment, Mobility & Hardship
6. Demotion, Dissolution & the Path Upward
7. Housing, Displacement & Religion
8. Illustrative Example
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

Settlement Demographics already tracks eight pop groups as pure aggregate numbers — size, Employment Ratio, Contentment, Legal Status distribution — and that math keeps working exactly as designed for the overwhelming majority of a settlement's population. Rival Houses already solved the equivalent problem one social tier up: rather than simulate every gens in existence, it tracks a small number of full-depth **Houses of Note** and rolls everyone else as abstracted **Background Houses**, promoting one to the other only when it becomes actually relevant. This document applies that identical pattern one rung down, to ordinary households rather than noble gentes.

A **Notable Household** is a real, named record — a head, a household composition, an occupation, a home — sampled out of the aggregate pop groups Settlement Demographics already tracks. It is never a set of individually-simulated Familia-depth Characters; per direction, "you don't need to simulate every person individually" is the load-bearing design constraint this entire document is built around. A household's two children are a demographic fact on the record, not two additional stat blocks, unless and until one of them is specifically promoted into full Familia depth through the same mechanism Settlement Demographics §11 already defines.

---

## 2. The Notable Household Record

Every field maps directly onto something this project already tracks elsewhere — this document introduces no new underlying stat, only a new, smaller-scale record type that packages existing data around a named household:

| Field | Source |
|---|---|
| **Head's name and household size** | Generated fresh, per this document's own naming conventions |
| **Occupation** | The specific trade behind the household's Settlement Demographics pop group (Opifices → "potter," "smith," "weaver"; Negotiatores → "shopkeeper," "grain merchant") |
| **Composition** (adults, children, elderly, other dependents) | A lightweight demographic breakdown, not individual records — "2 children, 1 elderly dependent" stays exactly that granular |
| **Social status / citizenship** | Familia's existing Legal Status categories (Citizen, Latin Rights, Peregrine, Freedman), read from the parent pop group's own distribution (Settlement Demographics §10) |
| **Wealth** | A light qualitative tier (Meager / Modest / Comfortable / Prosperous) rather than a tracked Net Worth figure — full financial simulation stays Economy & Finance's territory for the player's own household alone |
| **Religion** | The household's own region document's Population & Culture Distribution table, sampled the same way |
| **Place of origin** | Likewise drawn from the region's own population table — a household can be a real outlier per that table's own standing "no distribution is ever exclusive" rule |
| **Housing quality** | Buildings' existing Insula/Domus tier system, tied to a specific Land Ownership & Real Estate Property Record where relevant |
| **Satisfaction** | Settlement Demographics' existing Contentment stat, read at the individual household's own sampled value rather than the pop group's aggregate average |

Nothing here is a parallel simulation — it's a specific, human-scale window onto numbers this project's other systems already produce every month.

---

## 3. Sampling — How a Household Gets Named

A settlement holds a small number of Notable Household slots, scaling with Settlement Demographics' own stage progression (Vicus stage might support a handful; a full City, several dozen spread across all eight pop groups) — deliberately capped, the same restraint Rival Houses applies to its own Houses of Note count, so the settlement reads as populated rather than becoming an unmanageable list.

A slot fills two ways:

- **Ambient sampling** — periodically, a household is drawn from a pop group weighted by that group's own relative size, instantiated with the record fields in §2 rolled against the pop group's own current aggregate stats (its Employment Ratio, Contentment, Legal Status distribution) rather than independently invented numbers.
- **Triggered promotion** — a specific household is pulled directly into Notable status because it became mechanically relevant: it's the Operator of a leased property (Land Ownership & Real Estate §6), it's a party to a Legal & Court case, a Disease outbreak or Natural Disaster is centered on it, or a Displacement event (Real Estate §10) needs a real family to actually lose their home rather than an abstract Contentment tick.

---

## 4. The Household Lifecycle — Formation, Aging & Inheritance

A Notable Household ages and changes on the same monthly tick as everything else, resolved through lightweight rolls rather than Familia's own full depth:

- **Aging** — children grow toward adulthood, adults age toward the elderly-dependent category, on the same broad lifecycle timing Familia already uses for its own tracked members, just without the intervening stat growth.
- **Formation and marriage** — a household's own adult children can marry into another Notable Household or back into the general aggregate pop group, occasionally producing a merged or newly-split household record.
- **Death and inheritance** — when a head dies, the household's own eldest adult (or a spouse) inherits the record and continues it; if no heir exists, the household **dissolves**, its remaining members folding back into the aggregate pop group count they came from rather than vanishing unaccounted-for.

This is deliberately the same shape as Familia's own succession logic, scaled down to a record with no individual stat blocks behind it.

---

## 5. Employment, Mobility & Hardship

A Notable Household's own fortunes are a direct, human-scale read of Settlement Demographics' existing aggregate math, not a separate simulation:

- **Unemployment** — when a household's pop group carries an unfavorable Employment Ratio (Settlement Demographics §4.2) at the point the household is sampled or re-evaluated, the household can roll Unemployed, dropping its Wealth tier and Contentment — a real, individual story ("the potter's own trade dried up when the new workshop opened") sitting on top of the aggregate ratio rather than inventing a new one.
- **Social mobility** — a household can shift pop-group category exactly along Settlement Demographics' own existing mobility pathways (§5 of that document — Coloni ⇄ Operarii, Operarii ⇄ Opifices, and so on), reflected as the household's own occupation and status changing rather than a silent aggregate-only shift.
- **Military service** — a Notable Household's own adult can be drawn into active service and return, per Settlement Demographics' existing Veterans loop, giving that pathway a real family it happened to rather than a pure number.

---

## 6. Demotion, Dissolution & the Path Upward

Consistent with Rival Houses' own promotion/demotion logic: a Notable Household that stops being relevant — no active property, no ongoing case, nothing distinguishing it — can quietly demote back into the aggregate pool exactly the way it was sampled out, keeping the total Notable Household count from only ever growing.

**The genuinely interesting direction is upward.** A Notable Household that accumulates enough real wealth — a successful Negotiatores shop, a freedman Operator's own property buyout (Land Ownership & Real Estate §6.1's own worked example is exactly this story) — can cross the threshold into becoming a real, new Rival House of its own, entering that system's own *novus homo* rising-house path (Rival Houses §2.2) directly. This document is, among other things, the actual population this project's Rival Houses document was always describing when it named new houses "rising" without specifying where they rose *from*.

---

## 7. Housing, Displacement & Religion

A Notable Household's own housing tier is never abstract when it matters: where a specific Land Ownership & Real Estate Property Record exists (a specific Insula unit, a specific Domus), the household is tied to it directly, giving that document's own Displacement mechanic (§10 of that document) a real family to actually displace when a District's Property Value spikes, rather than only a Contentment number moving in the aggregate. A household's own Religion and Place of Origin fields are read from its region's own Population & Culture Distribution table (every Starting Region document already has one) — a household can, per those tables' own standing rule, be a real, legitimate outlier rather than only ever matching the majority.

---

## 8. Illustrative Example

*(Directly following the format proposed in review — not a balance target, a texture illustration.)*

> **Gaius Valerius — Notable Household, 5 persons**
> Citizen household · Opifices (potter)
> Adults: Gaius (potter), Fulvia (textile worker)
> Dependents: 2 children, 1 elderly parent
> Wealth: Modest
> Housing: Insula apartment, Riverside District
> Religion: Roman/traditional
> Origin: Local
> Contentment: 71

This is the entire record — no hidden individual stat blocks behind Fulvia or the two children, just a real, legible household the player can watch age, prosper, struggle, or eventually rise, exactly per direction's own stated scope.

---

## 9. Cross-System Integration

- **Settlement Demographics:** this document's own entire foundation — every field in §2 reads from that document's existing pop-group math rather than duplicating it; §5's mobility and employment mechanics are that document's own pathways given individual faces.
- **Rival Houses:** shares its exact Background/Notable sampling-and-promotion pattern (§3, §6), one social tier down; §6's upward path is this document's own concrete supplier for that system's *novus homo* rising-house mechanic.
- **Land Ownership & Real Estate:** an Operator (that document's §6) is now naturally a Notable Household's own head rather than a name generated in isolation; §7's Displacement mechanic (that document's §10) gets a real family to affect.
- **Familia:** §4's lifecycle and inheritance logic mirrors that document's own succession pattern at reduced depth; Settlement Demographics §11's promotion trigger is this document's own bridge into full Familia tracking when a household member becomes individually relevant.
- **Starting Regions (all documents):** §7's Religion and Origin fields read directly from each region's own Population & Culture Distribution table.
- **Legal & Court, Disease & Public Health, Natural Disasters & Environment:** each system's own household-facing case or crisis now has a real, named Notable Household to land on rather than an anonymous aggregate tick.
- **Dynasty Chronicle:** a Notable Household's own rise to Rival House status, or a dramatic dissolution, is real, tiered material in its own right.

---

## 10. Data Model

```
NotableHousehold {
  householdId, settlementId, headCharacterName,
  popGroupType,                    // reads from Settlement Demographics' own eight categories
  occupation,                       // the specific trade behind the pop group
  composition: { adults, children, elderly, otherDependents },
  legalStatus,                      // "citizen" | "latinRights" | "peregrine" | "freedman"
  wealthTier,                       // "meager" | "modest" | "comfortable" | "prosperous"
  religion, placeOfOrigin,          // sampled from the region's own Population & Culture Distribution table
  housingTier, linkedPropertyRecordId,   // nullable — set when tied to a specific Land Ownership & Real Estate Property Record
  contentment,                      // read at this household's own sampled value
  isUnemployed: bool,
  sampledOrTriggeredBy,              // "ambientSample" | "operatorRole" | "legalCase" | "diseaseOrDisaster" | "displacement"
}

HouseholdLifecycleEvent {
  householdId, eventType,           // "aging" | "marriage" | "birth" | "death" | "inheritance" | "dissolution"
  resultingHouseholdIds: [ ... ],    // supports merges/splits
}

HouseholdMobilityRecord {
  householdId,
  fromPopGroupType, toPopGroupType,
  triggeredByEmploymentRatio: bool,
}

RisingHouseTransition {              // §6 — the bridge into Rival Houses
  householdId, newRivalHouseId,
  triggerCondition,                  // e.g. "propertyBuyout" | "sustainedProsperousWealthTier"
}
```

---

## 11. Open Questions

- **All numeric sizing**, per this project's standing convention — Notable Household slot counts per settlement stage, sampling frequency, the Unemployed roll's own probability curve, and the Rising House wealth threshold are all unsized.
- **Whether a Notable Household's own composition should ever expose more granularity than "2 children, 1 elderly dependent"** — this document deliberately holds the line at demographic facts rather than named individual dependents, but a future pass could revisit this if playtesting shows the record reads as too thin.
- **Multi-generational Notable Household continuity.** §4 establishes inheritance and dissolution but doesn't specify how many generations a single household record can realistically persist before naturally converting to a Rising House or dissolving — left open rather than artificially bounded.
- **Player interaction depth.** This document assumes a Notable Household is primarily something the player observes and occasionally transacts with (as a tenant, a court party, a disaster victim) rather than something with its own Interaction Catalog access the way a full Character has — whether any light, specific interactions (a rent negotiation, a direct patronage offer) should be available directly against a Notable Household without first promoting it to full Familia/Character depth isn't decided.
- **Cross-district household movement.** Whether a Notable Household displaced from one District (Real Estate §10) relocates to another within the same settlement, emigrates per Settlement Demographics §8.2, or simply dissolves isn't specified.
