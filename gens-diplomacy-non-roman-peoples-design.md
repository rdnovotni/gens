# GENS — System Design: Diplomacy with Non-Roman Peoples (§6.25)
*Two real, equally-deep diplomatic theaters — ordinary tribal Frontier treaty-making and a full Parthian state-to-state layer with real hostage-exchange practice — now with the real friction this system was missing: a genuine Interpreter/Cultural-Familiarity mechanic reading directly against Education & Culture's own Cultural Drift, asymmetric hostage-taking from Frontier peoples distinct from Parthia's own mutual exchange, a full Raiding & Retaliation cycle for when diplomacy fails, and real competitive texture from Rival Houses courting the same neighboring peoples. Alliance Against Rome now resolves in real gradations — a negotiated new autonomy short of total independence, or, at the furthest and most dangerous extreme, direct Parthian backing for open rebellion.*

---

## Contents

1. Scope & Role
2. The Foreign Leader & People as a Living World Actor
3. Frontier Relations Posture — A Standing Policy
4. Per-People Standing — Reputation Duality, Made Plural
5. The Interpreter Problem — Cultural Familiarity as a Real Negotiation Input
6. Frontier Diplomatic Actions
7. Diplomatic Failure — Raiding & Retaliation
8. Great Power Diplomacy — Parthia
9. Armenia — Resolving the Contested Buffer
10. Alliance Against Rome — The Point of No Return
11. A Crowded Frontier — Rival Houses & Competing Diplomacy
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "the neighboring tribes aren't just an unrest modifier but actors in their own right, reachable for treaties, tribute arrangements, and alliances — including, potentially, an alliance against Rome itself." Per direction, Great Power diplomacy gets the same real depth as tribal treaty-making, and this pass adds the friction, failure states, and competitive texture the first draft's all-positive treaty menu was missing.

---

## 2. The Foreign Leader & People as a Living World Actor *(unchanged)*

Reuses Rival Houses' own Background/Note tiering wholesale — a Frontier people the player hasn't meaningfully engaged is a name and a general disposition; sustained contact promotes them to a real, lazily-instantiated leader Character.

---

## 3. Frontier Relations Posture — A Standing Policy *(unchanged)*

Guarded Distance, Active Engagement, or Subjugation Pressure — the last capable of accelerating one specific local relationship's real Category shift ahead of the wider Empire's own historical schedule, a bounded, household-scale echo of Events' own Divergence mechanic.

---

## 4. Per-People Standing — Reputation Duality, Made Plural *(unchanged)*

A household can hold genuinely different Standing with different neighboring peoples simultaneously, using Rival Houses' own tiered scale.

---

## 5. The Interpreter Problem — Cultural Familiarity as a Real Negotiation Input

New this pass, and a real, honest gap the first draft's own treaty menu quietly ignored: Cultures of the Known World now names three dozen real, distinct languages and cultures, and no negotiation with any of them should proceed as smoothly as one conducted entirely in Latin among fellow Romans. Every Frontier or Parthian negotiation now reads a real **Interpreter Quality** input, resolved one of three ways:

- **No qualified Character present** — the negotiation proceeds at a real, meaningful penalty, reflecting genuine miscommunication risk.
- **A culturally-familiar Character conducts it** — a Character whose own `culture` (Education & Culture §2) either matches the target people's culture natively, or has drifted meaningfully toward it through sustained Cultural Drift, negotiates at a real, full-strength baseline with no penalty at all — the direct, concrete payoff for every Foreign Tutor, Institution of Renown, or blended-marriage choice this project's own Culture mechanic already lets a player pursue.
- **A hired Interpreter** — a real, purchasable partial fix: closes most but not all of the gap a native or drifted Character would close for free, a genuine, lesser alternative for a household that hasn't cultivated real cultural familiarity with a given people yet.

This is deliberately not a new tracked meter — it's a real-time read of Education & Culture's own existing Culture field at the moment of negotiation, giving that document's own mechanics a further, concrete payoff without this document inventing a parallel system.

---

## 6. Frontier Diplomatic Actions

The original roster, plus two real, historically-grounded additions:

- **Treaty of Non-Aggression, Tribute Arrangement, Trade Agreement, Marriage Alliance, Auxiliary Levy Agreement, Foederati Pact** — unchanged from the first pass; see that pass's own description of each.
- **Diplomatic Gifts** *(new)* — a real, lighter-weight, repeatable goodwill action distinct from a formal Treaty: sending luxury goods, wine, or fine craftsmanship (Resources & Goods) to a foreign leader for a modest, real Standing improvement, at Treasury cost rather than negotiation risk. Real, well-documented ancient practice, and the natural first move for a household not yet ready to propose a full Treaty.
- **Frontier Hostage-Taking** *(new)* — distinct in kind from Parthia's own mutual Hostage Exchange (§8.2): a real, historically common Roman practice, well-documented in Caesar's own accounts of Gaul specifically, where hostages are demanded *from* a Frontier people **as a condition of peace** rather than exchanged between equals. One-directional, and reads that way mechanically — a real, felt Standing cost to the people supplying the hostage, and a real, meaningful reduction in that people's own likelihood of breaking a treaty while the hostage remains in the household's custody. The received hostage is a real Character, and reads against the same Cultural Drift and relationship-building potential a Parthian hostage would, per Education & Culture's own mechanics — but the underlying relationship is coercive, not diplomatic between equals, and this document keeps that distinction honest rather than blurring it with §8.2's own real mutuality.

---

## 7. Diplomatic Failure — Raiding & Retaliation

New this pass, and the real "what happens when this doesn't work" the first draft never addressed.

### 7.1 Raiding

A people at Hostile or Feuding Standing (§4), or one whose Treaty of Non-Aggression the household itself broke, generates real raid Events against the household's own frontier holdings — reading Natural Disasters' own Frontier Security Posture (Policies & Edicts §2.12) as the direct mitigating factor, and Military & Combat's own defensive engine as the actual resolution mechanism. This is deliberately the same shape Piracy & Banditry's own land-based Blemmyes-style raiding already uses (Cultures §7) — diplomacy's own failure state and that system's own baseline threat are the same underlying phenomenon read from two different documents.

### 7.2 Retaliatory Campaign

A real, distinct option short of full annexation or Subjugation Pressure's own slow accumulation: a punitive Military & Combat campaign aimed specifically at restoring Standing and deterring further raids, not at conquest. A successful Retaliatory Campaign moves Standing back toward Wary rather than toward client status — a real, proportionate response rather than every military option against a Frontier people defaulting to annexation.

---

## 8. Great Power Diplomacy — Parthia

### 8.1 The Legate *(unchanged)*

### 8.2 Hostage Exchange *(unchanged)*

Real, mutual, historically documented — distinct in kind from §6's own one-directional Frontier Hostage-Taking, exactly the contrast worth keeping visible between negotiating with a tribe and negotiating with a genuine peer power.

### 8.3 Parthian Treaty Actions *(expanded)*

Peace Treaty and a Trade Agreement reaching toward the real Silk Road intersection (Sogdian culture, Cultures §10.6), now joined by a **Luxury Trade Concession** — a specific, high-value negotiated arrangement over silk and other Eastern luxury goods distinct from an ordinary Trade Agreement's own broader scope, and Armenia's own contest (§9).

### 8.4 Envoy Protection — A Real, Old Norm

Worth naming directly rather than leaving implicit: the principle that an envoy or ambassador conducting genuine diplomacy is owed real, if not absolute, protection even in otherwise hostile dealings is a real, ancient norm predating Rome itself. A Legate conducting Parthian negotiation, or any Character conducting a Frontier treaty under an active Treaty of Non-Aggression, carries a real, if imperfect, expectation of safety that a breach of specifically reads as a severe, reputation-damaging escalation for whichever side violates it — a real, historically grounded reason this document doesn't treat diplomatic Travel as carrying the same baseline danger as, say, wandering into contested territory uninvited.

---

## 9. Armenia — Resolving the Contested Buffer *(unchanged)*

A covert Scheme or an overt Military & Combat campaign writes directly into Armenia's own `currentAllegianceIfContested` field, never assumed permanent.

---

## 10. Alliance Against Rome — The Point of No Return

### 10.1 Gating *(unchanged)*

Only available from a household already at a real breaking point with Rome.

### 10.2 Three Real Stages *(unchanged)*

Secret Negotiation, Open Declaration, and the War Itself, resolved through Military & Combat's own engine against Rome's real, overwhelming default resources.

### 10.3 Victory, Now in Real Gradations *(new)*

A real, historically honest refinement: most actual ancient revolts that achieved any real success at all ended in a **negotiated new settlement**, not the imperial structure's total collapse. This document now offers both:

- **Negotiated Autonomy** — the more common, more achievable success state: Rome, unable or unwilling to fully re-subjugate the household and its allies, grants a real, formal, semi-independent status — functionally a new, unusually favorable client relationship rather than a full break. A real, meaningful win, and a real Events Divergence in its own right, but a smaller one than the alternative below.
- **A Full, Clean Break** — the rarer, harder, and more dramatic outcome: Rome's own authority in the region is genuinely and permanently ended, and the household becomes the founding dynasty of something new entirely. The larger, rarer Divergence.

### 10.4 Parthian Backing — The Furthest, Most Dangerous Escalation *(new)*

A real, historically attested pattern worth building in directly: Parthia is real-historically documented as having occasionally backed anti-Roman claimants and uprisings as a matter of genuine strategic interest. A household pursuing an Alliance Against Rome can, at the furthest and most dangerous extreme, seek **direct Parthian sponsorship** rather than relying on Frontier allies alone — real, substantial military and material backing, at the cost of an even more severe and immediate Discovery risk during the Secret Negotiation stage (§10.2), since courting a Great Power's own direct involvement in a rebellion against Rome is a categorically larger secret to keep than courting a neighboring tribe. This is the single highest-risk, highest-reward variant this document offers.

---

## 11. A Crowded Frontier — Rival Houses & Competing Diplomacy

New this pass: nothing about this system is exclusive to the player. Per Rival Houses' own living-world principle, another gens can be pursuing its own Frontier Relations Posture, its own Treaties, even its own Alliance Against Rome, entirely independent of the player's own actions. Two real, concrete consequences:

- **Competing for the same relationship** — two houses courting the same Frontier people's favor is a real, live possibility; a rival securing a Foederati Pact or Marriage Alliance the player was also pursuing is a real, felt loss, not a hidden background process.
- **A rival's own secret treason as leverage** — Espionage's own existing machinery (§6.15) is the natural discovery mechanism for a rival house's own Alliance Against Rome negotiations: uncovering it is real, potent blackmail material, or grounds for a Legal & Court case severe enough to remove a rival from play entirely — a real, high-stakes use for that system distinct from its more ordinary applications.

---

## 12. Cross-System Integration

- **Cultures of the Known World:** the Frontier and Great Power categories remain this document's entire roster; Subjugation Pressure's category-shift acceleration and Armenia's allegiance field are both directly read/written here.
- **Education & Culture:** the Interpreter Problem (§5) is a direct, real payoff for Cultural Drift, Foreign Tutors, and Institutions of Renown; cross-cultural marriage is realized as a concrete Frontier action.
- **Natural Disasters / Policies & Edicts:** Frontier Security Posture is the direct mitigating factor against Raiding (§7.1); Frontier Relations Posture is this document's own new, forward-flagged Standing Policy.
- **Piracy & Banditry:** Raiding (§7.1) is the same underlying phenomenon as that system's own land-based raiding, read from this document's own diplomatic-failure angle.
- **Military & Combat:** Retaliatory Campaigns, Auxiliary Levy Agreements, Armenia's overt-campaign option, and the Alliance Against Rome's own final war stage all resolve through that document's existing engine.
- **Rival Houses / Espionage:** §11 is this document's own direct realization of the living-world principle both documents already establish.
- **Events:** both Alliance Against Rome victory gradations (§10.3) formally register as Divergence, at two different weights.
- **Companions & Court Positions:** the Legate remains this document's own named role.
- **Legal & Court:** secret-negotiation discovery, whether the player's own or a rival's uncovered by Espionage, remains a severe case trigger.
- **Dynasty Chronicle:** Open Declaration, either victory gradation, and a rival's own exposed treason are all guaranteed, maximum-weight entries.

---

## 13. Data Model

```
FrontierRelationsPosture {
  householdId, posture,   // "guardedDistance" | "activeEngagement" | "subjugationPressure"
}

PerPeopleStanding {
  householdId, foreignPeopleActorId, standingTier,
}

InterpreterQuality {                    // new — §5, resolved at negotiation time, not persistently stored
  negotiatingCharacterId, targetPeopleActorId,
  source,                                 // "nativeOrDriftedCulture" | "hiredInterpreter" | "none"
  qualityModifier,
}

FrontierTreaty {
  treatyId, householdId, foreignPeopleActorId,
  type,                                    // "nonAggression" | "tribute" | "trade" | "marriageAlliance" |
                                            // "auxiliaryLevy" | "foederatiPact" | "diplomaticGift"
  tributeDirection,
}

FrontierHostageRecord {                    // new — §6, distinct from ParthianRelations' own hostageRecord
  hostageCharacterId, sourcePeopleActorId,
  takenMonth, releasedMonth,
  isCoercive: bool,                          // always true — the honest marker distinguishing this from Parthian exchange
}

RaidingState {                             // new — §7.1
  householdId, sourcePeopleActorId,
  active: bool, severityTier,
  causeTreatyBreach: bool,
}

ParthianRelations {
  householdId, legateCharacterId, standingTier,
  hostageRecord: { direction, hostageCharacterId, startMonth, returnedMonth },
  luxuryTradeConcessionActive: bool,          // new — §8.3
}

ArmenianAllegiance {
  currentAllegiance, lastChangedByHouseholdId, lastChangedMonth, method,
}

AllianceAgainstRome {
  householdId,
  stage,                                       // "secretNegotiation" | "openDeclaration" | "warUnderway" |
                                                // "victoriousAutonomy" | "victoriousCleanBreak" | "crushed"
  alliedForeignPeopleActorIds: [ ... ],
  parthianBackingSought: bool,                  // new — §10.4
  divergenceRef,
}
```

---

## 14. Open Questions

- **All numeric sizing carried forward, plus new unsized figures**: Interpreter Quality's exact modifier values, Diplomatic Gift costs/effects, Frontier Hostage-Taking's treaty-stability bonus, and Parthian Backing's exact additional Discovery-risk increase.
- **The Legate's exact recruitment.** Still unresolved from the first pass.
- **Multiple simultaneous hostage arrangements**, across both Frontier and Parthian forms now. Still unresolved.
- **Negotiated Autonomy's long-term stability.** §10.3 doesn't specify whether Rome can ever attempt to fully reclaim a household that achieved this lesser victory gradation, or whether it's treated as permanently settled the way a Clean Break is.
- **Rival houses' own AllianceAgainstRome visibility.** §11 establishes Espionage as the discovery mechanism; whether the player has any passive, non-Espionage way to ever notice a rival's own treason isn't specified.
- **Subjugation Pressure's interaction with Diplomacy's own Standing.** Still unresolved from the first pass.
