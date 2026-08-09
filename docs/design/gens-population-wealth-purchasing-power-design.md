# GENS — System Design: Population Wealth & Purchasing Power (§6.39, new)
*The missing demand side of an economic system this project has otherwise built almost entirely from the supply side — Resources & Goods produces, Economy & Finance prices, Notable Businesses sells. Nothing until now has asked how much the actual population can afford to buy, and the real historical answer matters more than it might first appear: the ancient Roman economy was not a broad consumer economy the way a modern one is. It was a steep pyramid, with the overwhelming majority of the population living at or near real subsistence and almost all discretionary spending concentrated in a genuinely small elite tier. This document builds that pyramid directly into the existing pop-group system rather than assuming a modern demand curve.*

---

## Contents

1. Scope & Role — Ancient Demand Was Not Modern Demand
2. The Wealth Pyramid — Three Real Tiers
3. Aggregate Purchasing Power
4. Subsistence Goods and Political Sensitivity
5. Elite Discretionary Demand and the Luxury Trade
6. Regional and District Variation
7. Business Viability — Matching Output to Demand
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — Ancient Demand Was Not Modern Demand

This document adds no new production, no new pricing engine, and no new tax mechanic — Resources & Goods, Economy & Finance's Market Dynamics, and every tax mechanism already built stay exactly as designed. What this document adds is the layer that determines how much of what's produced actually gets *bought*, and it insists on a real, historically accurate starting premise before building anything: most of the ancient Roman population had essentially no discretionary spending at all. A bad harvest wasn't a modest inconvenience for most households — it was a genuine subsistence crisis, which is exactly why the real Cura Annonae anxiety already built into this project's own Latium document and Settlement Demographics' Grain Dole existed in the first place. This document treats that reality as the actual shape of demand, not an occasional crisis interrupting an otherwise-normal consumer economy.

---

## 2. The Wealth Pyramid — Three Real Tiers

Mapped directly onto Settlement Demographics' own existing eight pop groups rather than inventing a parallel population model:

- **Subsistence** — the real majority. Coloni and Operarii by default, and any pop group currently reading an unfavorable Employment Ratio (Settlement Demographics §4.2). Spending is almost entirely food and bare shelter; a price spike in a basic good is a real, immediate crisis, not an inconvenience.
- **Modest Surplus** — Opifices and Negotiatores in ordinary standing, and Notable Households at Modest or Comfortable Wealth tier (Notable Households §2). Real, if limited, discretionary spending exists here — a better cut of meat, a slightly finer cloth, an occasional modest luxury — but it's genuinely thin and the first thing cut when times turn hard.
- **Elite Discretionary** — Curiales, Aeditui, and any Prosperous-tier Notable Household or full Character of real standing. This narrow tier is where the overwhelming majority of this project's own luxury-goods demand, imported-goods consumption, and genuinely elastic spending actually lives — a real, historically accurate concentration, not an oversight.

---

## 3. Aggregate Purchasing Power

A settlement's or District's own total demand is a direct, derived reading of how many people sit in each of §2's own tiers, weighted heavily toward the top — a real, honest reflection of just how lopsided ancient demand actually was, rather than a linear population-times-average-wealth calculation that would understate how much economic weight the narrow Elite Discretionary tier actually carries. This reading feeds directly into Business Competition's own Market Capacity concept (§6 of that document), Land Ownership & Real Estate's own District Property Value (§4 of that document), and Resources & Goods' own regional demand for anything beyond basic subsistence goods.

---

## 4. Subsistence Goods and Political Sensitivity

Basic goods — grain and bread above all, but also ordinary cloth, basic pottery, and other Subsistence-tier necessities — carry real political weight no luxury good ever does, because the tier that depends on them has no buffer at all. A price spike here isn't merely an economic event; it's a direct Contentment crisis (Settlement Demographics), a real trigger for Business Competition's own grain-hoarding anxiety (§5 of that document) and its associated Crime & Punishment exposure, and a genuine Scandal risk (Scandal §4) if a Notable Business is seen profiting from it. This document treats subsistence-good pricing as a fundamentally different, higher-stakes category from every other good in Resources & Goods' own registry — the one place this project's economy is never allowed to be politically inert.

---

## 5. Elite Discretionary Demand and the Luxury Trade

The inverse case: imported goods, fine wine, luxury textiles, and every other genuinely discretionary category in Resources & Goods' registry are functionally bought by, and priced for, the narrow Elite Discretionary tier alone — a real, historically accurate concentration that gives Merchant Families & the Equestrian Order's own wholesale-trade identity (§3 of that document) its actual customer base, and gives a region's own real reputation for wealth (Sicily's ancient prosperity, Latium's dense elite concentration) genuine demand-side teeth rather than only a supply-side flavor note. A luxury-goods business's own real viability depends entirely on a large enough Elite Discretionary population actually existing nearby to sustain it — a real, concrete constraint §7 builds on directly.

---

## 6. Regional and District Variation

Purchasing Power is never uniform across this project's own map — it varies by region, by settlement, and, per Land Ownership & Real Estate's own Districts (§4 of that document), by neighborhood within a single settlement. Latium's own dense elite concentration (Starting Regions: Italian Heartland §3.3) reads as an unusually large Elite Discretionary tier relative to its own population; Britannia's or Gallic Frontier's own raw, newly-settled economy reads as almost entirely Subsistence and Modest Surplus, with genuinely little elite demand to sustain a luxury trade at all; an Elite Quarter District within any City-stage settlement reads its own Purchasing Power far higher than a Warehouse District in the same city. This gives every region document's own already-established economic character (§3 of each) a real, concrete demand-side consequence rather than only a supply-side one.

---

## 7. Business Viability — Matching Output to Demand

The direct, practical payoff for Notable Businesses: a business's own long-term viability depends on whether its Output actually matches the Purchasing Power tier genuinely available in its own District. A bakery serving Subsistence-tier bread can thrive almost anywhere a real population exists at all — the demand is thin per customer but universal. A perfumer or a fine-jewelry workshop attempting to operate in a District with no real Elite Discretionary population is a business genuinely starved of customers regardless of the quality of its own Output — a real, honest constraint this document adds directly to Notable Businesses' own Reputation and income mechanics (§4 of that document), and a real, concrete reason a Notable Business might choose to Move (that document's §8) toward a wealthier District rather than simply improving its own product.

---

## 8. Cross-System Integration

- **Settlement Demographics:** §2's entire Wealth Pyramid is read directly from that document's existing eight pop groups and Employment Ratio — no new population data invented.
- **Notable Households:** Wealth tiers (§2 of that document) map directly onto §2's own Modest Surplus and Elite Discretionary categories.
- **Business Competition:** §3's Aggregate Purchasing Power feeds that document's own Market Capacity reading (§6 of that document) directly.
- **Land Ownership & Real Estate:** §6's regional/District variation is a direct new input into that document's own District Property Value (§4 of that document).
- **Resources & Goods:** §4 and §5 give that document's own subsistence-versus-luxury goods split real, concrete demand-side stakes for the first time.
- **Merchant Families & the Equestrian Order:** §5's Elite Discretionary tier is the real, concrete customer base behind that document's own wholesale-trade identity (§3 of that document).
- **Crime & Punishment, Business Competition, Scandal:** §4's subsistence-good political sensitivity is the direct real-world stakes behind grain hoarding (Business Competition §5) and its associated consequence chains.
- **Starting Regions (all documents):** §6 gives every region's own already-established economic character a concrete demand-side reading rather than leaving it purely supply-side flavor.
- **Notable Businesses:** §7 is a direct, new viability constraint on that document's own Reputation and income mechanics (§4 of that document), and a concrete new motivation for the Move behavior (§8 of that document).

---

## 9. Data Model

```
PurchasingPowerTier {                    // §2 — mapped onto Settlement Demographics' existing pop groups
  popGroupOrHouseholdId,
  tier,                                   // "subsistence" | "modestSurplus" | "eliteDiscretionary"
}

AggregateDemandReading {                  // §3
  settlementOrDistrictId,
  subsistenceWeight, modestSurplusWeight, eliteDiscretionaryWeight,
  totalDemandIndex,                        // weighted heavily toward the elite tier, per §3
}

SubsistenceGoodSensitivity {               // §4
  goodType,
  isSubsistenceCategory: bool,
  priceSpikeContentmentImpact, hoardingRiskMultiplier,
}

BusinessViabilityCheck {                   // §7
  businessId, districtId,
  outputGoodTier,                           // "subsistence" | "modestSurplus" | "eliteDiscretionary"
  localDemandMatch: bool,
  recommendedAction,                          // null | "specialize" | "move" (Notable Businesses §8)
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — the exact weighting curve favoring Elite Discretionary demand, and every tier's own precise boundary conditions, are unsized.
- **Whether Modest Surplus should itself be split further** — this document treats it as one tier, but a genuinely prosperous Negotiatores household and a barely-surviving Opifices one might warrant distinct sub-tiers in a future pass.
- **Cross-region luxury demand.** §5 assumes a luxury business's customers are local; whether a sufficiently famous luxury business (tying to Celebrities & Influential Figures) can draw genuine Elite Discretionary demand from beyond its own District or settlement isn't addressed.
- **Interaction with Natural Disasters and Disease.** This document doesn't specify how a population shock (a plague, a famine) should temporarily reshape the Wealth Pyramid itself beyond the existing Contentment and mortality mechanics those systems already model.
- **Whether Aggregate Purchasing Power should feed Legal & Court's own case volume** — a genuinely poor settlement might plausibly generate fewer debt-dispute cases than a wealthy one simply for lack of anything worth suing over, but this document doesn't formalize that connection.
