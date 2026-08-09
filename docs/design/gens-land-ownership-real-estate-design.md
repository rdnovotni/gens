# GENS — System Design: Land Ownership, Estates & the Real Estate Market (§6.28, FINAL)
*Final polish pass. A standalone document sitting alongside Estate & Settlement, Economy & Finance, Rival Houses, and Politics & Patronage rather than replacing any of them: this is where a household's wealth stops being one Net Worth number and becomes a real, named portfolio — villas, insulae, workshops, warehouses, farms, ships — each individually ownable, leasable, buyable, and sellable, by a genuinely wide range of owner types this project hasn't tracked before. The organizing tradeoff, per direction: owning everything directly is a real, live choice with real historical downsides (administrative overhead, political exposure, capital tied up rather than working), not a hard wall — leasing out a mature holding to a named Operator is how a growing household actually keeps expanding without collapsing under its own management burden. This pass fixes two cross-references (Economy & Finance's maritime loan sits in §6, not §7; Societas dissolution reuses Succession & Dynasty's inheritance logic, not Familia's), adds a worked example tracing one Insula's own life story from direct management through leasing to a possible freedman buyout, and adds a genuine historical detail — the real lex Claudia de nave senatorum — explaining why elite Romans actually preferred partnership structures over direct trade-ship ownership in the first place.*

---

## Contents

1. Scope & Role
2. Ownership Types — Who Can Actually Own Something
3. The Property Record — Named Assets, Not a Single Number
4. Districts — Urban Value Without Block-by-Block Simulation
5. Acquiring Existing Property — Buying What's Already Built
6. Leasing & Operators — Handing Off Management
   6.1 A Worked Example — One Insula's Own Life Story
7. Societas — Business Partnerships
8. Publicani — Tax Farming as a Contract
9. Property Value & the Market
10. Displacement & Rising Values
11. Portfolio Scale & Oversight
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role

Estate & Settlement already covers buying raw land and building on it. Economy & Finance already covers rent collected from Coloni and Insulae occupants, and rolls a household's total worth into one Net Worth figure. Rival Houses already tracks a rival gens's fortune in the abstract. This document is where all three connect into something more concrete: a household's wealth becomes a real, browsable list of named assets — not just "how much," but "which ones, where, run by whom, and how" — and that list can include property the household doesn't personally operate, doesn't wholly own, or doesn't own at all yet but could plausibly acquire.

This document doesn't re-litigate goods pricing (Resources & Goods), building construction (Estate & Settlement), or Net Worth's own aggregate math (Economy & Finance §8) — it sits on top of all three, giving the player a genuine real-estate-and-business layer rather than redesigning the machinery underneath it.

---

## 2. Ownership Types — Who Can Actually Own Something

Per direction, as wide a real range as this project can support without inventing anything ahistorical:

- **The player's household** — the default and most detailed case, extending Estate & Settlement's own Plot records.
- **A Rival Gens** — Rival Houses' own abstracted fortune, now given real, named individual assets that can specifically change hands.
- **An individual Character** — a freedman shopkeeper who owns his own tabernae outright without being head of a tracked gens, a Companion with personal property, any named person who isn't a full household in their own right.
- **A Temple** — real-historically, temples were often genuinely wealthy landowners and functioned informally as banks; Anatolia's own household of Diodoros (Starting Regions: Anatolia / Asia Minor §11) is already built on exactly this real premise. A temple rarely sells, but can lease or, under real pressure (a large enough favor, bribe, or Influence expenditure), be persuaded to part with a holding.
- **A Collegium** — a trade guild, resolving the open question Rival Houses' own document left unaddressed ("Collegia's actual mechanical depth... left for a dedicated pass"). A collegium can jointly own workshop or warehouse property serving its own trade's shared interest.
- **The Roman state** — *ager publicus*, public land leased rather than sold outright, the real historical root of the Gracchi crisis this project's own Events timeline opens on. Never fully "bought"; only ever leased, with the lease itself a real, live political fact (§5).
- **The settlement itself (Municipal/Civic)** — public buildings (the Forum, the Baths, the Carcer per Crime & Punishment §5) belong to the settlement as a civic body, distinct from the Roman state's own broader public land.
- **A Societas** — a business partnership, a real distinct Roman legal entity in its own right (§7), not simply a relationship between its members.
- **Imperial Patrimonium** — the Emperor's own real, personal property, legally distinct from ordinary state land. Rare and mostly flavor-weighted, relevant chiefly as the source of an exceptional land grant or the destination of a major confiscation.

---

## 3. The Property Record — Named Assets, Not a Single Number

Every significant asset a household (or any other owner type) holds is a real, named **Property Record**: a specific villa, a specific insula block, a named workshop, a warehouse, a farm, a ship. This document doesn't duplicate Estate & Settlement's own Plot schema for the player's directly-built structures — every Plot the player already owns and personally operates continues to live there unchanged. What this document adds is the layer Estate & Settlement never needed on its own: a **management status** flag (Directly Managed or Leased Out, §6) on any Plot the player owns, an ownership pointer that can resolve to any of §2's owner types rather than assuming the player by default, and two genuinely new asset types Estate & Settlement's plot-based model doesn't cover — **Ships** (mobile, not tied to a fixed plot, feeding directly into Resources & Goods' trade routes and carrying real Piracy & Banditry risk) and lightweight **Named Holdings** for a rival gens, temple, or collegium, sized for narrative and negotiation purposes without needing Estate & Settlement's own full building-chain simulation behind them.

---

## 4. Districts — Urban Value Without Block-by-Block Simulation

Per the scope discipline this project has held to everywhere else (Rival Houses' own abstracted Standing rather than a full rival-estate simulation; a region's own curated Gazetteer rather than a literal map), this document resolves "urban geography matters" at the **District** tier rather than the individual-building tier a fully granular model would require. Each settlement at Vicus stage or above (Estate & Settlement's own growth track) is divided into a small number of named Districts — a Vicus might have just one; a full City, four or five (a Forum District, a Riverside or Warehouse District, an Artisans' Quarter, an Elite Quarter, and so on, named to fit the specific settlement).

Each District carries its own **Property Value** trajectory, driven entirely by inputs this project's other systems already generate rather than a new standalone valuation engine: Settlement Demographics' own population and Contentment trends, a Natural Disaster's real damage, a Monument built there (Monuments & Legacy Building), and a region document's own Gazetteer Prominence Tier where the settlement in question is itself a named Gazetteer entry. A District's Property Value scales the rent income and acquisition cost of everything within it, and gives simple ownership real narrative texture — a Domus in the Elite Quarter reads differently than an identical Domus in the Warehouse District, without this document ever needing to simulate the individual buildings between them.

---

## 5. Acquiring Existing Property — Buying What's Already Built

Estate & Settlement's own §7 already covers acquiring raw, undeveloped land (buy outright, land grant, conquest, dowry/inheritance). This document adds the case that document never needed: acquiring property that's already built and already owned by someone else.

- **Voluntary sale** — any owner type from §2 can sell a Property Record outright, at a price scaled by the District's own Property Value (§4) and the asset's own condition and income history (§9). A Declining Rival House (Rival Houses §2.1) liquidating a holding to cover debt is a natural, recurring source of this kind of listing.
- **Forced sale** — an Insolvent household (Economy & Finance §9), a Legal & Court judgment, or Crime & Punishment's own confiscation following an execution or a Proscription (that document's §7–8) can all put a specific, named property up for acquisition rather than simply erasing it from the world.
- **Leasing *ager publicus*** — the state's own public land is never bought outright; it's leased, at a rate the household's own political standing (Politics & Patronage) and the land's own real productivity determine — a direct, concrete mechanical echo of the exact real controversy the Land Redistribution Edict (Policies & Edicts §5.4) and the Events timeline's own Gracchi-era opening are already built around.
- **Persuading a Temple or Collegium** — rarely sells outright, but a sufficiently large favor, bribe, or Politics & Patronage Influence expenditure can move one to part with a specific holding.

---

## 6. Leasing & Operators — Handing Off Management

Per direction, any developed property the household owns can be flagged **Leased Out** rather than **Directly Managed** — the actual mechanical answer to the whole premise this document started from: a growing household doesn't have to keep running everything itself to keep growing.

Leasing assigns a real, named **Operator** — a Character, drawn from the household's own Companions, Clientela, freedmen, or a freshly generated NPC — who runs the property day to day. The Operator's own Core Attributes and Loyalty (the same stats this project already tracks for every Character) determine how the arrangement actually plays out:

- **A steady, loyal Operator** remits a reliable, agreed share of income, freeing the player's own attention entirely — the direct payoff of trading full profit margin for passive income and reduced management overhead (§11).
- **A skimming Operator** quietly under-reports income; detectable through an audit action, at the cost of the player's own time and a relationship-web hit if the Operator turns out to have been honest all along.
- **An ambitious, successful Operator** — particularly plausible for a freedman running an urban Insula or Tabernae, the same upward-mobility story Settlement Demographics' own Negotiatores pop group already models — can eventually accumulate enough of their own wealth to offer to buy the property outright, converting a leased asset into a genuine, independent Individual Character ownership (§2) and, potentially, the seed of that Character's own future gens.

### 6.1 A Worked Example — One Insula's Own Life Story

Concretely: a household builds an Insula in a City's own Forum District, initially Directly Managed. As the portfolio grows and §11's own Administrative Burden starts to bite, the player flags it Leased Out and assigns a trusted freedman as Operator. For several years the arrangement is unremarkable — a steady, smaller income, freed attention spent elsewhere. Two branches from there, both real and both interesting: the freedman proves genuinely capable, the District's own Property Value keeps climbing (§4), and within a decade he has enough saved to offer a real buyout, converting the Insula into his own independent Individual Character holding and giving him a plausible, historically honest path toward founding his own minor gens; or, in the other branch, a routine audit reveals he's been skimming for years, souring the relationship-web bond badly enough that the player replaces him — a small, human-scale story playing out entirely through mechanics this document and its neighbors already define, never needing bespoke event content of its own to feel real.

---

## 7. Societas — Business Partnerships

A real, distinct Roman legal entity, not merely a relationship tag: two or more owners (any combination of the player's household, a Rival Gens, or an Individual Character) jointly hold a Property Record or fund a shared venture, splitting profit and loss by an agreed share. The classic real use case, and the one this document leans on directly, is a shipping venture — a societas is the natural entity two or more investors form specifically to spread the real risk Economy & Finance's own maritime loan (*fenus nauticum*, §6 of that document) already prices in.

A further real, historically concrete motivation worth naming directly, tying back to this whole document's own opening premise about why direct ownership isn't always the obvious choice: the real *lex Claudia de nave senatorum* (218 BC) barred senators from owning large seagoing trade vessels outright, on the theory that a senator's own wealth belonged in land, not commerce. A societas — investing capital through a partnership or a freedman front rather than holding a ship in one's own name — was the real, historically attested way an honestiores-tier household (Crime & Punishment §7) still profited from trade without technically violating the letter of that restriction. This document treats that motivation as a live, era-appropriate reason an NPC or player household might prefer a Societas structure specifically, not just a generic risk-spreading tool.

A partner's own Reactive Traits (Ambition, Greed, per Traits) genuinely affect whether the arrangement holds — a sufficiently ambitious or aggrieved partner can attempt to defraud the venture or exit early, and a societas dissolving, whether by mutual agreement, a partner's death, or a partner's own Insolvency, triggers a real division-of-assets event using the same proportional logic Succession & Dynasty's own inheritance division already applies.

---

## 8. Publicani — Tax Farming as a Contract

A real, historically significant, and historically notorious Roman institution, built here as an extension of Economy & Finance's existing Contracts category (§3.2 of that document) rather than a fully separate bidding-war simulation — a deliberate middle ground between full depth and pure flavor. A household with the right standing can bid for a **Publicanus Contract**: the right to collect a specific province's taxes, paying Rome an agreed sum upfront and keeping the difference between what's actually collected and what's owed.

A single **Collection Intensity** setting — Lenient, Standard, or Aggressive — is this document's own real profit-and-risk dial: Lenient collection yields a modest, safe margin with no real Legal & Court or local-standing exposure; Aggressive collection yields a substantially larger margin but carries a real, live risk of a formal corruption case (the same real pattern Sicily's own document already illustrates via Verres's prosecution, Starting Regions: Sicily §4, §15.3) and genuine damage to local standing in any region running a tapering or localized Reputation Duality mode (Iberian Colony, North African Colony, Syria/The Levant, The Balkans).

---

## 9. Property Value & the Market

Every Property Record carries a tracked **Value**, moving from real, already-established inputs rather than a new standalone economic model: the District's own trend (§4), the property's own income history, Natural Disaster damage, and nearby development (a Monument, a region's own Gazetteer-tier growth). A property can be sold back to the general market — an abstract buyer, resolving at current Value minus a standard friction — or sold directly to a specific, named party from §2's own ownership roster at a negotiated price, the more interesting and more common path for anything valuable enough to matter narratively.

---

## 10. Displacement & Rising Values

Per direction to fit this to whatever the rest of the project's own scope already supports: this document adds no new tracked displacement mechanic. Instead, a District's own sharply rising Property Value (§4) feeds directly into Settlement Demographics' *existing* Contentment and Emigration formula as a new input — higher rent burden depressing Contentment for a District's own lower-tier resident pop groups (Operarii, urban Coloni-adjacent poor) exactly the way overcrowding or low Contentment already does in that document. A gentrifying District is a real, felt consequence without this document inventing a parallel simulation to produce it.

---

## 11. Portfolio Scale & Oversight

No hard cap on how large a household's own property portfolio can grow, consistent with direction's own preference for self-limitation through cost rather than an artificial wall. Instead, a real, scaling **Administrative Burden**: each additional significant Property Record beyond a soft threshold adds to the household's own oversight cost (a genuine Economy & Finance expense line) or, if left unmatched, a real management-quality decay affecting that specific property's own income and condition. This burden is offset the same way every other household-scale management problem in this project already is — hiring additional Overseers or a Procurator (Companions & Court Positions), or delegating through Steward/Council Auto-Management's own existing standing-policy framework — meaning a genuinely large portfolio is entirely achievable, but only by engaging the same delegation tools that already exist rather than by the player personally attending to everything at once. This is the concrete mechanical form of this document's own opening premise: leasing out a mature holding isn't forced, but it's the historically honest and mechanically rewarded way to keep growing past a certain scale.

---

## 12. Cross-System Integration

- **Estate & Settlement:** every player-owned Plot gains a management-status flag (§6) rather than a redesigned schema; §5 adds acquisition methods for already-built property alongside that document's own raw-land methods.
- **Economy & Finance:** §11's Administrative Burden is a new expense line; §7's Societas ties directly to the existing maritime loan (§6 of that document); §8's Publicanus Contract extends the existing Contracts category; property Value (§9) feeds Net Worth (§8 of that doc) as a more granular, itemized version of what was previously one aggregate land/building figure.
- **Rival Houses:** a rival gens's previously abstract fortune now includes real, individually named, individually acquirable Property Records — a Declining house's liquidation (§5) is this document's own concrete mechanism for that document's own standing-trend concept.
- **Politics & Patronage:** *ager publicus* leasing (§5) is a direct, concrete mechanical expression of the Land Redistribution Edict's own real historical root; Influence (§4.4 of that document) is the currency behind persuading a Temple or Collegium to sell (§5).
- **Settlement Demographics:** §10's displacement effect is a new input into that document's own existing Contentment/Emigration formula, not a parallel mechanic.
- **Companions & Court Positions:** an Operator (§6) is a real Character role this document adds to that system's own existing roster logic; a Procurator or additional Overseer is the direct answer to §11's own Administrative Burden.
- **Succession & Dynasty:** a Societas's own dissolution (§7) reuses that document's own proportional inheritance-division logic directly rather than inventing a parallel split formula.
- **Legal & Court, Crime & Punishment:** a forced sale (§5) following a conviction, an execution, or a Proscription is this document's own concrete destination for those systems' own confiscation outcomes; Aggressive tax collection (§8) is a real, live source of future Legal & Court cases; the real *lex Claudia de nave senatorum* (§7) ties directly to that document's own honestiores tier.
- **Steward/Council Auto-Management:** the natural delegation mechanism answering §11's own scale question.
- **Starting Regions (all documents):** a region's own Gazetteer Prominence Tier is a real input into §4's District Property Value wherever a settlement coincides with a named Gazetteer entry; Anatolia's household of Diodoros (§2) is this document's own concrete precedent for Temple-as-landowner.
- **Dynasty Chronicle:** a major property acquisition, an Operator's betrayal or successful buyout, a Societas's dramatic collapse, and a Publicanus scandal are all real, tiered material.

---

## 13. Data Model

```
PropertyRecord {
  propertyId, propertyType,        // "villa" | "insula" | "tabernae" | "workshop" | "warehouse" | "farm" | "ship"
  ownerType,                        // "playerHousehold" | "rivalGens" | "individualCharacter" | "temple" |
                                     // "collegium" | "romanState" | "municipal" | "societas" | "imperialPatrimonium"
  ownerId,
  settlementId, districtId,
  managementStatus,                 // "directlyManaged" | "leasedOut" — §6
  operatorCharacterId,               // nullable — only set when leasedOut
  value, incomeHistory: [ ... ],
  condition,
  linkedEstateSettlementPlotId,      // nullable — set only for player-owned, Estate & Settlement-tracked structures
}

District {
  districtId, settlementId, name,
  propertyValueTrend,                // §4 — derived from Settlement Demographics, Natural Disasters, Monuments, Gazetteer Tier
  linkedGazetteerLocationId,          // nullable — set when this settlement is itself a region's named Gazetteer entry
}

OperatorRecord {                      // §6
  characterId, propertyId,
  incomeShareRemitted,
  isSkimming: bool,
  buyoutAmbition,                     // derived from Ambition/Greed Reactive Traits
}

SocietasRecord {                      // §7
  societasId,
  partners: [ { ownerType, ownerId, shareFraction } ],
  linkedPropertyOrVentureId,
  dissolutionTrigger,                 // null | "mutualAgreement" | "partnerDeath" | "partnerInsolvency" | "fraud"
}

PublicanusContract {                  // §8
  contractId, householdId, provinceOrRegionId,
  upfrontPayment,
  collectionIntensity,                 // "lenient" | "standard" | "aggressive"
  scandalRiskActive: bool,
  localStandingImpact,
}

AdministrativeBurden {                 // §11
  householdId,
  propertyCountAboveThreshold,
  overseerCoverage,                    // from Companions & Court Positions
  netBurdenCost,
}
```

---

## 14. Open Questions

- **All numeric sizing**, per this project's standing convention — District Property Value formulas, Administrative Burden's own threshold and cost curve, Publicanus profit margins per intensity setting, and Societas dissolution-split specifics are all unsized.
- **Named Holding depth for rival gentes.** §3 deliberately keeps a rival's own Named Holdings lightweight rather than fully simulated; the precise boundary between "enough detail to negotiate over" and "not reinventing Estate & Settlement's own building chains" isn't fully drawn.
- **Ager publicus lease duration and renewal.** §5 establishes leasing rather than owning outright but doesn't specify lease terms, renewal conditions, or what happens if Rome ever reclaims the land.
- **Operator succession.** §6 doesn't specify what happens to an existing lease arrangement when either the Operator or the owning household head dies — whether it passes to an heir automatically or requires renegotiation.
- **Collegium's own fuller mechanical identity.** This document resolves Rival Houses' own open question just enough to let a Collegium own property (§2), but a full treatment of collegia as political and economic actors in their own right remains open, consistent with that document's own note that it was "left for a dedicated pass."
- **Cross-region property holding.** Whether a household's portfolio can include Property Records in a region other than its own Home Anchor, and how that interacts with the Starting Regions framework's own Distant Holding mechanic (§7 of that document), isn't fully specified here.
