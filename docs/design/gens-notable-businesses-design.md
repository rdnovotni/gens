# GENS — System Design: Notable Businesses (§6.35, FINAL)
*Final polish pass. A fourth and final application of the sampling-and-promotion pattern this project has now used at every population tier — Rival Houses for gentes, Notable Households for ordinary families, Wandering Populations for itinerant specialists, and now this document for named commercial enterprises. Per direct dissection: most of what a "Bakery of Marcus Livius" record actually needs already exists, scattered across Notable Households, Land Ownership & Real Estate, and Economy & Finance. This document fuses those pieces into one legible business-level record and builds the genuinely new mechanics none of them cover — a business's own Reputation distinct from its owner's personal standing, named competition, named suppliers, and the real behaviors (Merge, Specialize, Move, Lobby) that had no home anywhere yet. This pass corrected an overclaim — the document had implied Scandal already tracked a business-specific source when it doesn't, now honestly framed as a new addition this document makes to that system — and added a worked example grounding the named-competition mechanic in a concrete two-bakery rivalry.*

---

## Contents

1. Scope & Role — What's Reused, What's New
2. The Notable Business Record
3. Sampling & Promotion
4. Business Reputation — Distinct From the Owner's Own Standing
5. Named Competition
   5.1 A Worked Example — Two Bakeries, One District
6. Named Suppliers
7. Government Contracts
8. Business Lifecycle Events
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role — What's Reused, What's New

Consistent with the direct dissection this document opens with: **capital, employees, ownership, debt, and government contracts are not new concepts.** A business's own capital and physical premises are Land Ownership & Real Estate's own Property Record (that document's §3); its owner is a Notable Household's own head (Notable Households §2) or a full Character where relevant; its debt runs through Economy & Finance's existing DebtRecord and Insolvency mechanics (that document's §9); a formal government supply relationship extends Land Ownership's own Publicanus Contract concept (§8 of that document) at a lighter, municipal scale (§7 below); a business partnership is a Societas (Land Ownership §7). This document doesn't rebuild any of that.

What this document actually adds: a business's own **Reputation**, genuinely distinct from its owner's personal Fame or Dignitas (§4); real, **named competition** with a specific rival business rather than abstract market pressure (§5); real, **named supplier relationships** giving Resources & Goods' trade flow an actual face (§6); and four real behaviors — **Merge, Specialize, Move, and Lobby** — that don't exist anywhere else in this project yet (§8).

---

## 2. The Notable Business Record

Directly modeled on the format proposed in review, with each field's own actual source made explicit:

> **Bakery of Marcus Livius**
> Owner: Marcus Livius *(a Notable Household head or full Character)*
> Property: workshop-type Property Record, [District] *(Land Ownership & Real Estate §3)*
> Employees: drawn from the settlement's own Opifices pop group *(Settlement Demographics)*
> Output: bread *(Resources & Goods' existing production chain)*
> Reputation: Good *(new — §4)*
> Debt: an active DebtRecord *(Economy & Finance §6)*
> Main Supplier: [named grain-trading household] *(new — §6)*
> Main Competitor: Bakery of Gaius *(new — §5)*
> Government Contracts: 1 *(Land Ownership & Real Estate §8, lighter municipal scale)*

Every field either points directly at something this project already tracks, or is one of this document's own four genuinely new additions — no field on this record requires inventing a parallel simulation.

---

## 3. Sampling & Promotion

Consistent with the identical pattern Rival Houses, Notable Households, and Wandering Populations have each already established: the overwhelming majority of a settlement's own commerce stays pure Settlement Demographics aggregate math, with no individual business ever instantiated. A specific business becomes a tracked **Notable Business** only when it becomes genuinely relevant — its owner is already a Notable Household of note, it holds a real government contract, it becomes the subject of a Legal & Court case or a Scandal, or the player directly transacts with it (buying from it, competing against it, considering acquiring it). A Notable Business that stops being relevant can demote back to the aggregate pool exactly the way its predecessors already do, keeping the tracked set from growing without bound across a long playthrough.

---

## 4. Business Reputation — Distinct From the Owner's Own Standing

A real, new, small tracked value — separate from Fame, separate from Dignitas — representing how the business's own output and service are actually regarded, independent of what anyone thinks of its owner personally. Marcus Livius the man can carry entirely ordinary personal Dignitas while his bakery's own bread is genuinely well-regarded across the neighborhood, or the reverse: a personally respected household head running a business the crowd quietly considers mediocre. Reputation rises through consistent Quality output and falls through supply failures, price gouging, or a genuine business-specific Scandal — a new source this document adds to that system's own existing list (Scandal §4), distinct from an ordinary Scandal implicating the owner's own personal conduct rather than the business itself.

---

## 5. Named Competition

The single most valuable new mechanic this document adds: a Notable Business's own **Main Competitor** is a specific, named rival business, not an abstract market pressure. This gives Settlement Demographics' own aggregate Employment Ratio and Market Dynamics (Economy & Finance) a real, individual face and opens genuine, escalating competitive actions: undercutting prices (reading directly against Economy & Finance's own Market Dynamics), poaching a skilled Opifex worker away from the rival, or, at the sharper end, an actual Coercive Interaction (Characters §9.4 — Sabotage, Spread a Damaging Rumor) deployed specifically against a business rival's own Reputation or supply chain rather than a personal target. A sustained rivalry between two Notable Businesses is real, tracked material in its own right, distinct from and smaller in scale than a full Rival Houses Feud, but built on the same underlying "living world, competing on its own initiative" principle.

### 5.1 A Worked Example — Two Bakeries, One District

Concretely: the Bakery of Marcus Livius and the Bakery of Gaius sit in the same District, both drawing on the settlement's own Opifices pool and reading the same Market Dynamics. When Gaius undercuts his own bread prices for a season, Marcus Livius's own Reputation and income both take a real, felt hit unless he responds — matching the price cut (thinning his own margin), poaching one of Gaius's own skilled bakers to shore up his Quality instead, or, if the rivalry has genuinely soured, funding a Spread a Damaging Rumor Interaction questioning the cleanliness of Gaius's own ovens. None of this requires bespoke event content — it's §4's Reputation, §5's own named rivalry, and Characters' existing Interaction Catalog, running exactly as designed, producing a real, specific commercial rivalry a player can actually follow rather than an abstract number moving in the background.

---

## 6. Named Suppliers

A Notable Business's own real, upstream trade relationship, giving Resources & Goods' existing production-chain inputs a specific face rather than an anonymous market source: a bakery's own grain doesn't just "come from the market," it comes from a specific named household, a Wandering Merchant (Wandering Populations §2), or a Land Ownership & Real Estate Property Record producing that exact good. This relationship carries its own real, if lighter, dependency risk — a supplier's own bad harvest, bankruptcy, or a Piracy & Banditry loss genuinely disrupts the dependent business's own Output, giving Resources & Goods' abstract chains a real, felt point of failure a purely aggregate system never could.

---

## 7. Government Contracts

A lighter-weight, municipal-scale extension of Land Ownership & Real Estate's own Publicanus Contract concept (§8 of that document), rather than a parallel system: a Notable Business can hold a real, standing supply contract with the settlement itself — most naturally, a bakery or grain-trade business contracted to help fulfill Settlement Demographics' own Grain Dole (that document's §6.3) or Policies & Edicts' own Grain Dole Funded Action (§4 of that document) — carrying real, steady income and real, visible civic standing, at the cost of a genuine obligation the business can't simply walk away from without real Reputation and relationship-web consequences if it fails to deliver.

---

## 8. Business Lifecycle Events

Ten behaviors named in review, six of them already real and simply reused, four of them genuinely new:

**Already real, reused directly:**
- **Expand** — Land Ownership & Real Estate's own property-development mechanics.
- **Go bankrupt** — Economy & Finance's existing Insolvency (§9 of that document).
- **Change owners** — a voluntary or forced sale, per Land Ownership & Real Estate §5.
- **Inherit** — Notable Households' own existing inheritance/dissolution logic (§4 of that document).
- **Raise/lower prices** — Economy & Finance's existing Market Dynamics.
- **Form partnerships** — a Societas, per Land Ownership & Real Estate §7.
- **Compete for contracts** — an extension of §7 above, resolved the same way any Publicanus-adjacent competition resolves.

**Genuinely new, this document's own contribution:**
- **Merge** — two Notable Businesses combining into one, typically following a marriage between their own owning households, a buyout, or a struggling business's own owner accepting absorption rather than facing Insolvency outright — a real, new resolution path distinct from an ordinary forced sale.
- **Specialize** — a Notable Business narrowing its own Output to a single high-quality good rather than a broader ordinary range, trading Reputation-building potential and a real Quality premium for reduced resilience if that one good's own supply chain (§6) is disrupted.
- **Move** — relocating to a different District (Land Ownership & Real Estate §4), trading a real, one-time cost against a different District's own Property Value trend and customer base.
- **Lobby government** — a lighter-weight political action than a full Politics & Patronage campaign or a Collegia & Guilds endorsement (that document's §6): a Notable Business spending Influence or a direct payment specifically to win or renew a government contract (§7), or to petition against a specific Sumptuary or trade regulation affecting its own Output directly.

---

## 9. Cross-System Integration

- **Notable Households:** a Notable Business's own owner is that document's own household head; §3's sampling-and-promotion logic is reused directly rather than reinvented.
- **Land Ownership & Real Estate:** the Property Record, Districts, Societas, and Publicanus Contract are this document's own direct foundation; Merge and Move (§8) are new behaviors that document's own property system didn't previously support.
- **Economy & Finance:** Insolvency, Market Dynamics, and DebtRecord are all reused directly rather than duplicated.
- **Resources & Goods:** §6's named suppliers give that document's own abstract production chains a real, individual point of failure.
- **Settlement Demographics:** the aggregate pool every Notable Business is sampled from and can demote back into; the Grain Dole (§7) is a direct, concrete government-contract destination.
- **Characters:** §5's sharper competitive actions reuse the existing Coercive Interaction catalog directly rather than inventing parallel business-specific versions.
- **Scandal:** §4 adds a new business-specific source to that document's own existing sourceType list (Scandal §4) — a genuine addition, not a pre-existing entry — distinct from any Scandal implicating the owner's own personal conduct.
- **Collegia & Guilds:** a Notable Business's own owner is a natural collegium member (that document's §2); collective bargaining there and named individual competition here operate as two distinct but compatible layers.
- **Policies & Edicts:** the Grain Dole Funded Action (§4 of that document) is this document's own concrete government-contract source.
- **Dynasty Chronicle:** a dramatic Merge, a business's own collapse into Insolvency, or a sustained, escalating rivalry with a named competitor are all real, tiered material.

---

## 10. Data Model

```
NotableBusiness {
  businessId, name, ownerCharacterOrHouseholdId,
  linkedPropertyRecordId,               // Land Ownership & Real Estate
  outputGoodType,                        // Resources & Goods
  reputation,                             // 0-100, distinct from owner's own Fame/Dignitas — §4
  mainCompetitorBusinessId,                // nullable — §5
  mainSupplierId,                           // Character, Notable Household, or PropertyRecord — §6
  activeGovernmentContractId,                // nullable, extends PublicanusContract — §7
  isSpecialized: bool, specializedGoodType,   // §8
  sampledOrTriggeredBy,                        // "ambientSample" | "ownerAlreadyNotable" | "governmentContract" |
                                                 // "legalOrScandalCase" | "directPlayerTransaction"
}

BusinessRivalryEvent {                     // §5
  eventId, businessAId, businessBId,
  actionType,                               // "priceUndercut" | "workerPoach" | "sabotage" | "damagingRumor"
  resolvedOutcome,
}

BusinessLifecycleEvent {                    // §8
  businessId, eventType,                      // "merge" | "specialize" | "move" | "lobbyGovernment" |
                                                // (plus the six reused types, logged the same way for Chronicle purposes)
  resultingBusinessId,                          // set for merge
  newDistrictId,                                 // set for move
}
```

---

## 11. Open Questions

- **All numeric sizing**, per this project's standing convention — Reputation growth/decay, the Notable Business count per settlement, and Merge/Move's own real costs are all unsized.
- **Whether a Notable Business's own Reputation should ever directly affect Settlement Demographics' Contentment** the way a Wanderer's Fame does, or stay a purely commercial figure — this document treats it as commercial-only for now.
- **Employee-level detail.** This document deliberately keeps "Employees: 7" as a derived headcount from the settlement's own Opifices pool rather than tracking each worker individually — whether a specific, named employee (distinct from the owner) ever warrants their own promotion path isn't addressed.
- **Cross-settlement competition.** §5 assumes a Main Competitor sits in the same settlement; whether a business can have a named rival in a different settlement entirely (relevant for a Wandering Merchant-adjacent trade network) isn't specified.
- **Lobbying's own success mechanics.** §8 names Lobby Government as a real action but doesn't specify its own resolution formula beyond pointing at Influence and existing Politics & Patronage machinery.
