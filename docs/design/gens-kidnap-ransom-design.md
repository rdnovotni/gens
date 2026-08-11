# GENS — System Design: Kidnap & Ransom (§6.61)

*Built on CK3's own kidnap-and-ransom model, per direction, and unusually compositional even by this project's own recent standard: three separate documents already reference this exact content without any of them owning it in full. Characters names "Kidnap / Imprison," "Ransom," and "Defend / Protect / Rescue" as one-line Interaction stubs with no real Scheme behind any of them. Crime, Punishment & Imprisonment built the actual Detention tracked-status and a working Ransom negotiation machinery — but wrote it for a captured Rival House member or battlefield prisoner, not a targeted kidnapping. Piracy & Banditry built two real sourcing paths — a paid Targeted Contract against a named enemy, and opportunistic capture of a traveling Familia member — plus the defensive Estate Security investment that counters both. This document is where all three finally connect, and where the pieces none of them built — a real Kidnap Scheme, ransom demands beyond gold, a standing-hostage alternative to cashing out, and an actual Rescue operation — get written for the first time.*

---

## Contents

1. Scope & Role — Consolidating Three Existing Threads
2. The Kidnap Scheme — Taking a Captive
3. Detention — Reused Directly
4. What Can Be Demanded — Beyond Gold
5. Negotiation — Offer, Counter, Impasse
6. Keeping a Hostage — The Standing Threat Option
7. Escape
8. Rescue — A Real Counter-Operation
9. Life During Captivity — Relationship and Trait Effects
10. Consequences for the Kidnapper
11. Special Case — An Heir Held Hostage
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role — Consolidating Three Existing Threads

Nothing here is invented from a blank page. Characters' own Interaction Catalog already lists "Kidnap / Imprison" (§9.4, Coercive/Intrigue), "Ransom" (§9.5, Economic — "post-kidnap resolution"), and "Defend / Protect / Rescue" (§9.6, Violent) as real, named entries with no Scheme or resolution logic actually written behind any of the three. Crime, Punishment & Imprisonment built the real machinery those stubs were always going to need — a tracked Detention status (§5) and a working offer/counter-offer Ransom negotiation (§10) — but built it for a battlefield captive or a captured Rival House member, never for a kidnapping specifically targeted and planned. Piracy & Banditry then built the two real ways a kidnapping actually happens in this project — a paid Targeted Contract against a named enemy (§7.1) and opportunistic capture of a traveling Familia member (§8) — plus the Estate Security investment that defends against both (§9). This document's job is almost entirely connective, exactly the role Espionage played for the scattered spy content a few documents ago: it builds the real Scheme these three threads all assumed existed, and adds the genuinely new CK3-inspired content none of them got to — non-gold demands, a standing-hostage option, and a real Rescue.

---

## 2. The Kidnap Scheme — Taking a Captive

The actual Multi-stage Scheme (Characters §10) behind the Kidnap / Imprison stub, reading the initiator's Martial and Intrigue against the target's own real security — Companions & Court Positions' Bodyguard/Retinue investment at home, or Piracy & Banditry §9's full Estate Security tally while the target is traveling. Three real sourcing paths all converge into this same Scheme resolution rather than each needing its own parallel mechanic:

- **A direct Scheme** — a rival's own operative attempts it personally, carrying Characters' own Scheme-discovery risk throughout.
- **A contracted Targeted Contract** (Piracy & Banditry §7.1) — paying a Confederation to do it instead, deniable up front but subject to that document's own Traceability model if the arrangement is ever exposed.
- **Opportunistic travel capture** (Piracy & Banditry §8) — no Scheme initiated by anyone specific at all, simply a real, standing environmental risk that Estate Security investment directly reduces.

On success, regardless of which path produced it, the target enters Detention (§3) under the kidnapper's control.

---

## 3. Detention — Reused Directly

No new mechanic: Crime, Punishment & Imprisonment §5's `DetentionRecord` — location type, escape risk, torture-for-testimony flag — is the captive's own tracked state throughout, entirely unmodified. This document's one real addition is a scope clarification: an ordinary Imprison action under that document's own Justice Spectrum (§2 of that document) requires a real Punishable Offense to read as legitimate. A kidnapping has no such requirement and makes no such claim — it's extralegal from its own first moment by definition, so that document's Justified/Unjust math never applies to the *captive*. It applies in full, undiluted force to the *kidnapper* instead (§10).

---

## 4. What Can Be Demanded — Beyond Gold

Per direction to look at CK3's own model, which lets a captor demand a claim or a title rather than only coin: this project's own equivalent menu, each option reading an existing system directly rather than this document inventing a parallel currency for every case.

| Demand Type | What Actually Happens |
|---|---|
| Coin/Goods | The existing default — Crime, Punishment & Imprisonment §10's own Ransom negotiation, unmodified |
| Hook Handover | Secrets & Hooks' own Hook is transferred to the kidnapper as payment in kind — a real, direct tie between the two documents |
| Debt Forgiveness | An Economy & Finance `DebtRecord` the kidnapper owes the paying house is canceled as ransom |
| Masterwork/Heirloom Handover | A specific, named Masterworks & Unique Crafted Objects item changes hands |
| Forced Betrothal Break | Familia's own marriage market absorbs a coerced rather than chosen dissolution |
| Land or Client Transfer | A parcel (Land Ownership & Real Estate) or a Clientela relationship (Politics & Patronage) is ceded under duress |
| Public Apology/Submission | No property changes hands at all — a direct Dignitas transfer from the paying house to the demanding one, the bloodless version of the demand |

Whatever the actual payment type, §5's Negotiation resolves identically — this document doesn't need seven parallel negotiation systems, only one that can accept any of the above as its "amount."

---

## 5. Negotiation — Offer, Counter, Impasse

Reused directly from Crime, Punishment & Imprisonment §10's existing machinery: an offer/counter-offer process running through Economy & Finance, with the same three named resolutions (paid, refused, bargained down) plus mercy release without ransom, each already a real, direct Rival Houses Standing event exactly as that document specifies. This document's one addition is **Impasse** — a negotiation that simply stalls (an insultingly low offer, a target too proud to pay what's reasonable) doesn't force a resolution on any fixed clock. It leaves the captive in ongoing Detention, and the real next move becomes §6, §7, or §8 rather than an artificial timeout.

---

## 6. Keeping a Hostage — The Standing Threat Option

The genuinely new option this project never built, and the clearest single CK3 import in this document: rather than negotiating a one-time cash-out, a kidnapper can choose to simply hold the captive indefinitely as leverage. Mechanically, this generates a **Captive Leverage** bond — a direct structural sibling to Secrets & Hooks' own Hook, usable to Compel the paying house into an ongoing concession rather than a single lump sum. This is a real, live tradeoff against §5's Negotiation: a quick ransom is certain and final; a held hostage is a renewable, escalating threat that risks §7's Escape or §8's Rescue taking the leverage away entirely before it's ever cashed in.

---

## 7. Escape

Distinct from Labor & Slavery's own escape math, which is built specifically around an enslaved Character's flight risk: a kidnapped Character's own escape attempt instead reads their personal Martial and Boldness axis against the `DetentionRecord`'s own location type and the kidnapper's own security investment — the same real inputs Labor & Slavery already uses, read in the opposite direction, since the captive here is proving something rather than fleeing something. A successful escape ends the Detention outright with no payment; a failed one is a real, felt setback, tightening the kidnapper's own security against a repeat attempt.

---

## 8. Rescue — A Real Counter-Operation

Characters §9.6 already names "Defend / Protect / Rescue" as a one-line stub; this is where it actually gets built for the kidnap case. A Rescue is a genuine Multi-stage Scheme in its own right, mounted by the captive's own household against a known or discovered holding location, with a real choice of method:

- **Direct Assault** — Military & Combat's own Combat Resolution Engine resolves the attempt outright, loud and forceful.
- **Covert Extraction** — Espionage's own Persistent Network machinery resolves a quieter attempt instead, at a different real risk profile.

A failed Rescue is a genuine, felt cost, not a free retry: the rescuing party risks becoming a second captive themselves, and a botched loud attempt can push a previously negotiable kidnapper toward §6's harder standing-hostage posture instead of continuing to deal in good faith.

---

## 9. Life During Captivity — Relationship and Trait Effects

A real, quiet layer this document adds directly: an extended Detention isn't inert time. The kidnapper can set a real treatment posture toward the captive, reading Labor & Slavery's own Bare/Confined/Harsh-to-Comfortable spectrum by analogy rather than duplicating its exact enslaved-specific mechanic. A sufficiently long, sufficiently harsh captivity is a real, plausible trigger for an existing Reactive Trait — Broken, or Vengeful — exactly the way Labor & Slavery's own Regimen already produces them. A surprisingly humane captivity, per real historical and psychological plausibility, is the rarer, honest opposite case: a genuine opening for the relationship web between captor and captive to shift somewhere unexpected — never guaranteed, never engineered by this document, simply a real possibility it doesn't foreclose.

---

## 10. Consequences for the Kidnapper

Three distinct exposure paths, deliberately not collapsed into one:

- **A contracted kidnapping** carries Piracy & Banditry's own Traceability model in full (§7.1 of that document) — deniable until traced, and a traced contract is a real, direct Rival Houses Standing hit plus Legal & Court exposure.
- **A direct Scheme** carries Characters' own Scheme-discovery math throughout the attempt itself, and a discovered *completed* kidnapping reads exactly as Crime, Punishment & Imprisonment's own worst case describes an unjustified action: there was never a real Punishable Offense behind it, so exposure lands as straightforwardly Unjust the instant it surfaces.
- **Public knowledge of a ransom paid or refused** is its own Scandal-adjacent consequence regardless of how the kidnapping was discovered — a paying house's own willingness, or pointed refusal, to pay is itself a real, felt Dignitas event exactly as Crime, Punishment & Imprisonment §10 already frames it.

---

## 11. Special Case — An Heir Held Hostage

Worth its own short section given the real stakes involved: Piracy & Banditry's own Targeted Contract already names a **killed** heir as a genuine extinction trigger for a house with no remaining line. A **kidnapped** heir is this document's own softer but still serious equivalent — Succession & Dynasty's own inheritance process, if actively underway, is a real, direct complication for as long as the heir remains in Detention, resolved cleanly the moment a Ransom or Rescue brings them home rather than a hard, permanent block. A held heir is, deliberately, the single highest-leverage use of §6's standing-hostage option anywhere in this system, and correspondingly the single most tempting — and most dangerous to botch — target for a Rescue.

---

## 12. Cross-System Integration

- **Characters:** formalizes the Kidnap/Imprison, Ransom, and Defend/Protect/Rescue Interaction stubs directly; builds the real Scheme and Rescue operation behind all three.
- **Crime, Punishment & Imprisonment:** Detention (§5) and the full Ransom negotiation machinery (§10) are both reused wholesale, entirely unmodified.
- **Piracy & Banditry:** Targeted Contracts (§7.1) and travel-risk kidnapping (§8) are this document's two real sourcing paths into the same Scheme; Estate Security (§9) is the direct defensive counterweight to both.
- **Secrets & Hooks:** §6's Captive Leverage bond is a direct structural sibling to that document's own Hook; §4's demand menu includes a direct Hook handover as a payment type.
- **Espionage:** §8's covert-extraction Rescue option reuses that document's own Persistent Network machinery directly.
- **Military & Combat:** §8's direct-assault Rescue option reuses the Combat Resolution Engine directly.
- **Labor & Slavery:** §7's escape math and §9's captivity-treatment posture both read that document's own concepts by analogy, without duplicating its exact enslaved-specific formulas.
- **Traits:** §9's Broken/Vengeful trigger reuses those existing Reactive traits directly.
- **Succession & Dynasty / Rival Houses:** §11's heir-hostage complication and the deliberate distinction between a killed heir's extinction trigger and a merely kidnapped heir's softer one both read those documents' own mechanics directly.
- **Scandal:** §10's public-ransom-knowledge consequence reads that document's own severity/consequence machinery directly.
- **Economy & Finance / Masterworks & Unique Crafted Objects / Familia / Land Ownership & Real Estate / Politics & Patronage:** §4's full non-gold demand menu reads each of those documents' own existing mechanics directly as a valid ransom payment type.

---

## 13. Data Model

```
KidnapScheme extends Scheme {          // Characters §10's existing Scheme engine
  initiatorActorId,           // a household, or a contracted Confederation actor
  targetCharacterId,
  sourcePath,                 // "directScheme" | "contractedRaid" | "opportunisticTravelCapture"
  outcome,                    // "captured" | "targetEscapedAttempt" | "detectedAndFoiled"
}

// DetentionRecord reused directly from Crime, Punishment & Imprisonment §5/§12 — no new fields required

RansomDemand {
  demandId, detentionRecordId,
  demandType,                 // "coin" | "hookHandover" | "debtForgiveness" | "masterworkHandover" |
                               // "betrothalBreak" | "landOrClientTransfer" | "publicApology"
  demandDetailRef,             // nullable — points to the specific Hook/Masterwork/DebtRecord/etc. being demanded
}

// RansomNegotiation reused directly from Crime, Punishment & Imprisonment §12,
// generalized so amountOffered/amountCountered can reference a RansomDemand instead of only a bare figure

CaptiveLeverageBond {           // §6 — structural sibling to Secrets & Hooks' Hook
  bondId, captorCharacterId, capturingHouseholdId, captiveCharacterId,
  isActive: bool,
  compelUsesRemaining,          // qualitative, unsized
}

CaptivityTreatment {            // §9
  detentionRecordId,
  posture,                    // "harsh" | "confined" | "comfortable" — read by analogy to Labor & Slavery's Regimen
  durationMonths,
  reactiveTraitTriggered,        // nullable — "broken" | "vengeful" | (rare) a positive relationship-web shift
}

RescueOperation {              // §8
  operationId, detentionRecordId, rescuingHouseholdId,
  method,                     // "directAssault" | "covertExtraction"
  outcome,                    // "success" | "failedRescuerCaptured" | "failedNoChange" | "failedEscalatedToHardHostage"
}
```

---

## 14. Open Questions

- **All numeric sizing**, per this project's standing convention — Scheme discovery odds, escape-risk magnitude, ransom pricing curves (already left open in Crime, Punishment & Imprisonment), and Captive Leverage's own compel-uses-remaining figure are all unsized.
- **Whether §9's rare positive relationship-web shift during a humane captivity should ever produce a genuine new bond tag of its own**, or remain a purely qualitative, ungoverned possibility as this pass leaves it.
- **Whether a household can hold more than one Captive Leverage bond simultaneously against the same paying house**, and how their Compel demands interact if so — the same shape of open question Secrets & Hooks already carries for its own Hook stacking.
- **Whether §11's heir-hostage complication needs its own explicit Succession & Dynasty data field**, or can be read implicitly from an active `DetentionRecord` that already references a Character flagged as an heir.
- **Whether a captive with meaningful personal wealth independent of their own household** — if such a distinction is ever built into a future Economy & Finance pass — should be able to negotiate their own ransom directly, bypassing their family's involvement entirely. An interesting, currently unbuilt wrinkle this document doesn't resolve.
