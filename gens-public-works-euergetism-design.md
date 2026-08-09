# GENS — System Design: Public Works & Euergetism (§6.40, extends Policies & Edicts §4)
*Policies & Edicts already lists "Public Works" as one of its eight Funded Action categories, in one line: "Dignitas, a boosted civic building output." This document is the full depth that line never had room for, and it's built around a real, specific Roman institution worth naming directly rather than treating public works as generic charity: euergetism, the actual cultural expectation that a wealthy Roman's own social standing came with a real, felt obligation to fund the community's own functional infrastructure — not the singular glory monuments Monuments & Legacy Building already covers, but the aqueducts, roads, sewers, and marketplaces that made a settlement actually work.*

---

## Contents

1. Scope & Role — Infrastructure, Not Glory Monuments
2. Euergetism — A Real Obligation, Not Just Generosity
3. Types of Public Works
4. Inscription and Credit
5. Competitive Euergetism
6. Maintenance and Upkeep
7. Private and State Funding
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — Infrastructure, Not Glory Monuments

Monuments & Legacy Building already covers the singular, self-glorifying structure — a triumphal arch, a dedicated temple, a personal monument meant chiefly to be looked at and remembered. This document covers something genuinely different: functional civic infrastructure that a settlement's own population actually uses every day — aqueducts, roads, bridges, sewers, marketplaces, and harbors — funded, per real Roman practice, substantially by private elite generosity rather than the state alone. A public works project can absolutely carry a real inscription crediting its patron (§4), and the same household can pursue both a Monument and a genuine public work in the same playthrough, but this document's own mechanical focus is on what the infrastructure actually *does* for the settlement, not primarily on what it does for its patron's own name.

---

## 2. Euergetism — A Real Obligation, Not Just Generosity

The real Roman cultural institution this document is built around: wealthy elites weren't merely free to fund public infrastructure if they felt generous — real social expectation held that a household of sufficient standing genuinely *owed* the community some visible material contribution, and a wealthy household that conspicuously failed to provide one, especially during a real local need (a settlement outgrowing its water supply, a genuine infrastructure failure), faced real social consequence for the omission. This document treats that as a live, felt pressure rather than an optional flavor detail: a sufficiently wealthy, sufficiently prominent household (Events §5's own Prominence concept) that never funds a single public work across a long playthrough carries a real, quiet Dignitas cost distinct from and separate from ordinary Politics & Patronage standing — the community's own genuine expectation, unmet.

---

## 3. Types of Public Works

Six real categories, each carrying a genuine, distinct mechanical effect on the settlement rather than a uniform Dignitas-and-nothing-else payoff:

- **Aqueducts** — a real, direct improvement to Disease & Public Health outcomes, reflecting the genuine historical link between clean water supply and reduced disease burden.
- **Roads** — a real, direct improvement to Travel efficiency and Economy & Finance's own Trade Route effectiveness, reducing the felt cost of both.
- **Bridges** — a real, concrete unlock: genuine new access to previously awkward or costly-to-reach land, feeding directly into Land Ownership & Real Estate's own acquisition and District-value calculations for the newly-accessible area.
- **Sewers** — a further real Disease & Public Health improvement, distinct from an aqueduct's own clean-water contribution, and a genuine Settlement Demographics Contentment boost for the District it actually serves.
- **Marketplaces and Basilicas** — a real, direct boost to Economy & Finance's own Market Dynamics and Notable Businesses' own available District-level Purchasing Power (Population Wealth & Purchasing Power §3), giving local commerce genuine new capacity rather than only prestige.
- **Harbors** — a real, substantial improvement to a coastal settlement's own trade capacity, Resources & Goods' own import/export flow, and Land Ownership & Real Estate's own Major Port-type Property Records nearby.

---

## 4. Inscription and Credit

A real, well-documented Roman practice: a public work commonly bore a formal, visible inscription naming its funding patron — the actual, physical mechanism behind a household's own public works becoming genuinely remembered rather than merely funded. This document ties that directly into Epithets, Nicknames & Titles (§4 of that document): a sufficiently significant public work is a real, plausible source of a formal grant-style Agnomen, and a household's own accumulated pattern of public works across generations is exactly the kind of sustained record Dynasty Chronicle and Epithets' own Dynastic Epithet mechanic (§6 of that document) already draw on.

---

## 5. Competitive Euergetism

A real, historically documented social dynamic worth building in directly: rival elite households in the same settlement genuinely competed to fund ever more impressive public works, a real "arms race" of civic generosity distinct from Business Competition's own commercial rivalry but built on the identical underlying escalation logic (that document's §2). A household that funds a modest aqueduct can find a Rival House responding with a grander one nearby, or a more prominent inscription, each round raising the real Dignitas stakes and the real cost of the next contribution — a genuine, live alternative to Rival Houses' own more adversarial Feud mechanics, where the competition itself benefits the whole settlement rather than harming either side directly.

---

## 6. Maintenance and Upkeep

Unlike a Monument, which this project treats as a largely one-time construction, functional infrastructure carries a real, ongoing upkeep cost — an aqueduct or road that isn't maintained genuinely degrades, reading directly against Buildings' own existing condition-and-decay mechanics. A patron who funds construction but neglects ongoing upkeep sees their own public work's real benefit fade over time, and, in a severe case of visible neglect, risks a real Scandal (Scandal §4) for having let a once-celebrated contribution fall into disrepair — the honest, harder half of euergetism's own real obligation that a single triumphant dedication ceremony doesn't fully discharge.

---

## 7. Private and State Funding

Two real, distinct funding sources, both legitimate: **private funding** (euergetism proper, §2) is this document's own primary focus — a single wealthy patron or a Societas of several (Societates & Business Partnerships §8) directly funding a project for real Dignitas and inscription credit. **State funding** draws instead on the settlement's or province's own tax revenue (Economy & Finance), carrying no individual patron's name and no personal Dignitas payoff, but also no risk of a single household's own fortune being tied up in the project — the honest, impersonal alternative for infrastructure a community needs but no single household currently has the wealth or the motivation to personally fund.

---

## 8. Cross-System Integration

- **Policies & Edicts:** this document is the full depth extension of that document's own brief Public Works Funded Action entry (§4 of that document).
- **Monuments & Legacy Building:** explicitly distinguished as a separate, complementary system — glory monuments versus functional infrastructure — rather than a competing or redundant one.
- **Disease & Public Health:** Aqueducts and Sewers (§3) are direct, concrete mechanical inputs into that document's own disease-burden mechanics.
- **Economy & Finance, Notable Businesses, Population Wealth & Purchasing Power:** Marketplaces and Harbors (§3) feed Market Dynamics, District-level Purchasing Power, and trade capacity directly.
- **Land Ownership & Real Estate:** Bridges (§3) unlock genuine new acquisition territory; Harbors give Major Port-type Property Records real added value.
- **Epithets, Nicknames & Titles, Dynasty Chronicle:** §4's inscription practice is a direct, concrete Agnomen and Dynastic Epithet source.
- **Rival Houses, Business Competition:** §5's competitive euergetism reuses that document's own escalation-ladder logic, applied to civic generosity rather than commercial rivalry.
- **Buildings:** §6's maintenance mechanic reads directly against that document's own existing condition-and-decay system.
- **Scandal:** neglected upkeep (§6) is a real, new Scandal source; failure to meet real euergetism expectations (§2) is a quiet, ongoing Dignitas cost rather than a discrete Scandal event.
- **Societates & Business Partnerships:** a joint public-works funding venture between multiple households is a natural Societas Unius Rei application (§8 of that document).

---

## 9. Data Model

```
PublicWork {
  publicWorkId, settlementId, districtId,
  workType,                              // "aqueduct" | "road" | "bridge" | "sewer" | "marketplaceOrBasilica" | "harbor"
  fundingSource,                          // "privateEuergetism" | "stateTaxRevenue"
  fundingPatronHouseholdOrSocietasId,       // nullable — set only for privateEuergetism
  hasInscription: bool,
  condition,                               // reads Buildings' own existing decay mechanics — §6
  upkeepFundedRecently: bool,
}

EuergetismObligation {                    // §2 — the quiet, ongoing social pressure
  householdId,
  prominenceTier,                          // Events §5
  publicWorksFundedCount,
  perceivedAsNeglectful: bool,               // true if Prominence is high and this count stays at zero too long
}

CompetitiveEuergetismEvent {                // §5
  eventId, initiatingHouseholdId, respondingHouseholdId,
  settlementId,
  escalationRound,
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — the exact Prominence-to-obligation threshold, each work type's own mechanical benefit magnitude, and upkeep cost curves are all unsized.
- **Whether a public work should ever be formally "completed" versus perpetually needing upkeep** — this document assumes ongoing maintenance is simply a permanent fact of owning infrastructure, but a future pass could specify a point past which a well-maintained work requires less frequent attention.
- **Competitive Euergetism's own natural stopping point.** §5 describes an escalating cycle but doesn't specify what causes it to actually end, short of one household simply running out of will or wealth to continue.
- **Cross-settlement public works.** Whether a household can fund infrastructure (particularly a road) connecting two separate settlements, rather than a single work located entirely within one, isn't addressed.
- **Interaction with a region's own Historical Timeline Hooks.** Whether a real historical event (a Natural Disaster, a war) can retroactively damage or destroy an existing Public Work, requiring a real rebuilding decision, isn't specified here.
