# GENS — System Design: Merchant Families & the Equestrian Order (§6.33, new)
*Resolving a real tension this project has named twice without ever building it out: Policies & Edicts' own Hybrid Doctrine table names "Merchant Princes of the People" and flags "the equestrian tension, made into a single house's whole identity" directly — but nothing before this document actually specifies what that tension is, who lives inside it, or what a merchant family's own distinct path through Roman society actually looks like. This document is the character study Domus Mercatoria's own practitioners never got: the real, formal Equestrian Order, its real privileges, its real and permanent friction with the old senatorial aristocracy, and the wealth-first path a family actually walks from a first successful cargo voyage to a seat in the Senate.*

---

## Contents

1. Scope & Role — The Equestrian Tension, Finally Built
2. The Equestrian Order — A Real, Formal Middle Tier
3. Wholesale and Retail — The Cicero Distinction
4. Merchant Archetypes
5. New Money and Old — The Senatorial Tension
6. The Merchant Path to the Senate
7. Merchant Families as Rival Houses
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — The Equestrian Tension, Finally Built

This document doesn't touch trade mechanics themselves — Economy & Finance's Market Dynamics, Resources & Goods' entire registry, and Land Ownership & Real Estate's own Societas partnerships and Publicanus contracts all stay exactly as built. What this document adds is the social and political layer sitting above all of that: a formal treatment of the **Ordo Equester**, the real historical class merchant wealth actually lived in, and the real, permanent friction between that class and the inherited prestige of the senatorial aristocracy — a tension this project's own Policies & Edicts document has already named directly without ever specifying its shape.

---

## 2. The Equestrian Order — A Real, Formal Middle Tier

A real, formally recognized Roman social and legal class, sitting between ordinary citizens and the Senate — not an informal wealth bracket, but a genuine census-qualified order with its own real privileges:

- **A real wealth threshold**, distinct from and lower than the Senate's own property census (Politics & Patronage §6) — this document reads that threshold directly off Economy & Finance's existing Net Worth figure rather than inventing a parallel wealth check.
- **The angusticlavus** — a real, narrow purple stripe on the toga, the equestrian order's own visible marker, distinct from a senator's own broader *laticlavus* stripe — a genuine, concrete, at-a-glance status distinction available to Villa's own household display and Dynasty Chronicle flavor text.
- **Reserved public seating** — a real historical practice: equestrians held their own designated seating at Games & Spectacle's own theatrical and arena events, distinct from both the Senate's own front rows and the ordinary crowd behind them — a genuine, visible social marker every time a Fame-generating public event actually happens.
- **Equestrian-exclusive offices** — most strikingly, Egypt's own real Prefecture (Starting Regions: Egypt §1, §4): senators were formally barred from governing Egypt at all, making that province's own administration an equestrian-exclusive path to real power a senatorial family, however wealthy or well-connected, simply couldn't access directly.
- **Publicani eligibility** — the equestrian order's own real, historical core business: tax-farming contracts (Land Ownership & Real Estate §8) were overwhelmingly an equestrian activity, precisely because the *lex Claudia de nave senatorum* (that document's own §7) barred senators from the large-scale commercial activity equestrians specialized in.

---

## 3. Wholesale and Retail — The Cicero Distinction

A real, genuinely specific piece of Roman class snobbery worth building directly into this document's own Wealth-tier logic: Cicero himself real-historically distinguished between petty retail trade — buying cheap and reselling at a markup, conducted at small scale — which he judged beneath a respectable man's dignity, and large-scale wholesale or import commerce, which he judged not merely acceptable but *nearly* honorable if conducted on a sufficiently grand scale. This document reads that real distinction directly into Notable Households' own Wealth tier (that document's §2): a Modest-tier shopkeeper's own trade carries real, if mild, social condescension from above; a Prosperous-tier import merchant or shipping investor's own trade carries genuine, if still second-tier, respectability — the same underlying activity, judged by scale rather than by kind.

---

## 4. Merchant Archetypes

Real, distinct Roman terminology worth preserving rather than flattening into one generic "merchant" label:

- **Negotiatores** — the broader, more prestigious term: financiers, wholesale businessmen, and large-scale commercial investors, the real Latin root behind Settlement Demographics' own existing Negotiatores pop group.
- **Mercatores** — the narrower term for the traders and shippers actually moving goods, closer to the hands-on commercial work a Wandering Populations Merchant/Peddler (that document's §2) embodies.
- **Shipping Magnates** — a household whose wealth runs through Societas partnerships (Land Ownership & Real Estate §7), spreading real maritime risk across investors precisely because the *lex Claudia* barred senators from holding ships directly.
- **Tax Farmers** — a household holding one or more Publicanus Contracts (Land Ownership & Real Estate §8), carrying that system's own real profit-and-scandal-risk dial.
- **Freedman Merchant Dynasties** — the real, historically plausible and already-modeled upward path (Land Ownership & Real Estate §6.1's own worked example; Notable Households §6's own Rising House transition): a freedman Operator's own successful property buyout is, in miniature, exactly how a real merchant dynasty's own founding generation often began.

---

## 5. New Money and Old — The Senatorial Tension

The real, historically significant social dynamic this document exists to formalize: an equestrian merchant family can be, and often was, genuinely **wealthier** than many old senatorial houses, while carrying real, comparatively little inherited Dignitas — the exact mirror of Politics & Patronage's own already-established "strong Dignitas, insufficient Net Worth" case for a declining old house (that document's §6). Where an old, impoverished gens is a real story of prestige outliving fortune, a rising equestrian merchant family is the reverse: fortune arriving well ahead of the prestige that would make it fully respected. Neither position is dominant over the other — a real, direct expression of Design Pillar #1 — and this document treats the friction between the two as a live, ongoing social fact rather than a problem either side can simply resolve by accumulating more of what they're missing.

---

## 6. The Merchant Path to the Senate

This document gives Politics & Patronage's own *novus homo* story (§6 of that document) its specific merchant-family shape, genuinely distinct from a political or military rising house's own path: **wealth accumulates first, respectability is deliberately purchased afterward.** A merchant family clears the Senate's own Net Worth gate comparatively early and comparatively easily — trade wealth grows faster than land-based fortune typically does — but stalls at the Dignitas gate specifically, and has to close that second gap through deliberate, visible investment rather than simply waiting: funding a Games & Spectacle event or a Public Works Funded Action (Policies & Edicts §4) for Dignitas rather than direct profit, pursuing a strategic marriage into an old, prestige-rich but cash-poor house (a direct, natural pairing with §5's own declining-old-house case), or holding a local magistracy (Politics & Patronage §5) as a visible, respectability-building stepping stone before ever attempting the cursus honorum's own higher rungs. This is a genuinely different rhythm from a military *novus homo*'s own prestige-first, wealth-follows path — the same destination, reached from the opposite direction.

---

## 7. Merchant Families as Rival Houses

A formal **Merchant House** archetype for Rival Houses' own Background/Notable framework (that document's §2), distinct in its own characteristic volatility from an old landed gens: a merchant house's own Net Worth (Rival Houses §3.4) is real-historically far more volatile than land-based wealth — a bad Societas venture, a lost cargo to Piracy & Banditry, a sudden Insolvency (Economy & Finance §9) can genuinely collapse a merchant fortune in a single bad season in a way an established landed estate's own slower-moving wealth rarely does. This document treats that volatility as the merchant house's own defining texture — a real, live "boom or bust" identity distinct from an old aristocratic house's own comparatively stable, slow-moving Standing trajectory, and a genuine source of real Rival Houses drama (a sudden collapse opening a real acquisition opportunity per Land Ownership & Real Estate §5's own forced-sale mechanism) that an old gens's own document doesn't produce in quite the same way.

---

## 8. Cross-System Integration

- **Policies & Edicts:** this document is the direct character study behind that document's own Domus Mercatoria Household Doctrine and its "Merchant Princes of the People"/"Lords of the Wide Roads" Hybrid Doctrine combos (§3.4 of that document) — the equestrian tension those entries name is fully specified here for the first time.
- **Politics & Patronage:** §6 gives that document's own *novus homo* cursus honorum story (§6 of that document) its specific merchant-family shape; §2's equestrian order is a new, formal social tier that document's own Dignitas and Net Worth gates already implicitly assumed without naming.
- **Land Ownership & Real Estate:** Societas (§7) and Publicanus Contracts (§8) are this document's own concrete merchant activities; the *lex Claudia de nave senatorum* (§7 of that document) is the direct historical reason the equestrian order specializes in exactly the commerce it does.
- **Starting Regions: Egypt:** the Prefecture's own real equestrian-exclusive status (§1, §4 of that document) is this document's own clearest example of an equestrian-only path to genuine power.
- **Rival Houses:** §7 adds a formal Merchant House archetype to that document's own Background/Notable framework, with its own distinct volatility profile.
- **Notable Households:** §3 and §4's Wealth-tier and Rising House mechanics are read directly rather than duplicated; a Freedman Merchant Dynasty's own founding generation is that document's own upward-mobility story given its specific commercial flavor.
- **Games & Spectacle:** reserved equestrian seating (§2) is a real, concrete social marker at any public event that system hosts.
- **Villa:** the angusticlavus (§2) is a genuine, visible household-display detail.
- **Economy & Finance:** the equestrian wealth threshold (§2) and a merchant house's own volatility (§7) both read directly from that document's existing Net Worth and Insolvency mechanics.
- **Dynasty Chronicle:** a family's own formal entry into the equestrian order, a successful Senate crossing via §6's own merchant path, or a sudden merchant-house collapse are all real, tiered material.

---

## 9. Data Model

```
EquestrianStatus {
  characterOrHouseholdId,
  qualifiesByNetWorth: bool,           // §2 — read directly from Economy & Finance's existing figure
  holdsAngusticlavus: bool,
  eligibleForEquestrianOffices: bool,   // e.g. Egypt's Prefecture — §2
  publicaniEligible: bool,
}

MerchantHouseArchetype {                // §7 — extends Rival Houses' own Background/Notable framework
  householdId,
  merchantType,                          // "negotiator" | "mercator" | "shippingMagnate" | "taxFarmer" | "freedmanDynasty"
  wealthVolatilityTier,                  // "high" — the defining trait distinguishing this from an old landed gens
  wholesaleOrRetailTier,                  // §3 — "retail" | "wholesaleOrImport", affecting baseline social respectability
}

SenateEntryProgress {                    // §6 — the merchant-specific novus homo path
  householdId,
  netWorthGateCleared: bool,              // typically cleared early
  dignitasGateCleared: bool,               // typically the actual bottleneck
  dignitasInvestmentActions: [ { actionType, effect } ],   // "fundedGamesOrPublicWorks" | "strategicMarriage" | "localMagistracy"
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — the equestrian Net Worth threshold itself, wholesale-vs-retail respectability weighting, and merchant-house volatility curves are all unsized.
- **Whether Equestrian Status should be a formally tracked, player-visible flag** rather than a derived reading off existing Net Worth — this document treats it as computed rather than separately stored, but a future UI pass may want it surfaced directly.
- **Interaction with Familia's own sex-based restrictions.** Whether a female-headed household's own path through this document's mechanics (particularly §6's Senate path, which assumes eventual male officeholding) needs its own explicit treatment isn't addressed here.
- **Multiple equestrian offices held simultaneously.** §2 names Egypt's Prefecture as the clearest example of an equestrian-exclusive post, but doesn't specify whether other such offices exist or how holding more than one might interact.
- **The exact mechanical trigger for a merchant house's own sudden collapse (§7).** This document names the real volatility pattern and points at existing causes (Piracy, Insolvency, a failed Societas) but doesn't specify a combined probability model across them.
