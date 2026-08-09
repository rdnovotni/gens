# GENS — System Design: Societates & Business Partnerships (§6.36, new)
*The deep-dive Land Ownership & Real Estate's own brief Societas paragraph (§7 of that document) never had room to be. Sale, Acquisition, and Merger already have real homes — Land Ownership & Real Estate §5 and Notable Businesses §8 — and this document doesn't rebuild either. What it does build is the partnership structure itself: the real, distinct Roman partnership types, the governance choices a partnership actually has to negotiate, and the single most dramatic real fact about a societas this project hadn't surfaced yet — a partner's liability wasn't limited to what they invested. It was unlimited, reaching their entire personal fortune.*

---

## Contents

1. Scope & Role — What's Already Covered, What Isn't
2. Real Partnership Types
3. Unlimited Liability — The Central Stake
4. Governance Models
5. Formation — The Lex Societatis
6. Dissolution and the Actio Pro Socio
7. Partner Disputes
8. Societates and Notable Businesses
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role — What's Already Covered, What Isn't

Land Ownership & Real Estate's own Societas section already establishes the basic concept — a real, distinct Roman legal entity, multiple owners, profit split by agreed share, the real *lex Claudia de nave senatorum* motivation behind why senators used them at all. Notable Businesses' own Merge and Form Partnerships behaviors (§8 of that document) already give a Societas its concrete business-level application. Neither document goes further than that, and this document doesn't re-litigate either — it builds the actual partnership *structure* underneath both: what kind of societas a household is actually forming, who really controls it, what happens when it goes wrong, and, most importantly, exactly how much a partner actually has on the line by joining one.

---

## 2. Real Partnership Types

Roman law recognized genuinely different scopes of partnership, not one universal template — this document names the three that matter most for this project's own purposes:

- ***Societas Unius Rei*** — "partnership for one thing." A real, narrow partnership formed for a single specific venture — one shipping voyage, one trade expedition — dissolving automatically the instant that purpose is complete. This is this project's own default, lightest-commitment societas, matching Land Ownership & Real Estate's own maritime-loan use case directly.
- ***Societas Omnium Bonorum*** — "partnership of all goods." A real, far more serious commitment: partners pool essentially their entire property together, not just capital earmarked for one venture. A genuinely rare, high-trust arrangement in this project's own terms — typically between close family or a household's own long-standing, deeply loyal allies — and correspondingly the highest-stakes application of §3's own unlimited liability.
- **Publicani Societates** — real, historically dominant tax-farming syndicates, organized specifically as societates because a Publicanus Contract's own real scale (Land Ownership & Real Estate §8) was rarely something one investor shouldered alone. This document ties this type directly to that existing contract mechanic rather than duplicating it.

---

## 3. Unlimited Liability — The Central Stake

The real, dramatic fact this project hadn't built in anywhere: a Roman societas carried no equivalent of a modern limited-liability shield. If the partnership failed, defaulted, or was successfully sued, **each partner's own personal fortune — not merely their invested capital — was exposed**, in principle without limit. This document treats that as the actual, central tension of forming any Societas: a household entering a Societas Unius Rei for one voyage risks a real, contained loss if that voyage fails; a household entering a Societas Omnium Bonorum with an unreliable or dishonest partner risks genuine, complete ruin — the same severity Economy & Finance's own Insolvency (§9 of that document) already models, now reachable through a partner's own failure rather than the household's own direct mismanagement. A household considering any Societas should read this document's own real question as: not "how much am I investing," but "how much of everything I own is now actually exposed."

---

## 4. Governance Models

Roman partnership law didn't impose one universal decision-making structure — the actual terms lived in each partnership's own negotiated agreement (§5). This document names three real, recurring patterns worth formalizing as concrete choices:

- **Equal Partners** — genuine shared decision-making among socii of comparable standing, each with real say over the venture's own direction.
- **Dominant Partner** — one partner, typically the wealthiest or most socially prominent, effectively directs the partnership while the others function closer to passive investors — the real, common shape a senator-and-equestrian-front arrangement actually took, given the *lex Claudia*'s own restriction (Land Ownership & Real Estate §7): the senator's own capital and prestige backing the venture, the equestrian partner actually running it.
- **Silent Partner** — a partner contributing capital only, explicitly excluded from management by the partnership's own terms — the cleanest, most legally cautious version of the same senator-avoiding-direct-involvement pattern, and the governance model that most directly protects a socially prominent partner's own public Dignitas at the cost of any real say in how their money is actually used.

---

## 5. Formation — The Lex Societatis

Every Societas is formed around a real, specific negotiated agreement — its own *lex societatis* — rather than a default template applying automatically. Formation is a genuine negotiation between the prospective partners, resolving: which of §2's own partnership types is being formed, which of §4's own governance models applies, the actual profit-and-loss split, and the venture's own real duration or defining purpose (open-ended for a Societas Omnium Bonorum, bounded to a single voyage or contract for a Societas Unius Rei). This document treats formation itself as a real, meaningful Interaction rather than an instant checkbox — the terms genuinely matter, and a badly negotiated *lex societatis* (an unequal profit split relative to actual risk exposure, an unclear governance model) is a real, live source of §7's own future disputes.

---

## 6. Dissolution and the Actio Pro Socio

Real Roman law provided a specific legal remedy for exactly this moment: the ***actio pro socio*** — a formal legal action one partner could bring against another demanding a full, honest accounting of the partnership's own affairs before final dissolution. This document gives that real mechanism a genuine Legal & Court home: a contested Societas dissolution — one partner alleging the other has hidden profits, misrepresented losses, or breached the *lex societatis* itself — resolves as a real, new Legal & Court case type, distinct from an ordinary civil dispute, with the same evidence-and-Hearing structure that document already uses (Legal & Court §5) rather than an invented parallel process. A dissolution reached without dispute — mutual agreement, a natural Societas Unius Rei completion, or an amicable Societas Omnium Bonorum wind-down — simply resolves the way Land Ownership & Real Estate's own SocietasRecord already specifies, no case required.

---

## 7. Partner Disputes

Beyond outright dissolution, a live Societas carries several real, distinct dispute types worth naming directly, each escalating differently:

- **Suspected skimming or fraud** — a partner quietly diverting more than their agreed share, the direct partner-to-partner parallel to Land Ownership & Real Estate's own Operator-skimming risk (§6 of that document), but between equals rather than an owner and a hired manager — detectable the same way, through a real audit action, and, if confirmed, a natural trigger for §6's own *actio pro socio*.
- **A partner wanting early exit** — particularly relevant for a Societas Omnium Bonorum, where unwinding one partner's own stake from a genuinely comprehensive pooled arrangement is real, complicated work rather than a clean, instant withdrawal.
- **Profit distribution disagreement** — a dispute over interpretation of the *lex societatis* itself rather than any actual dishonesty, real and human, and the single most common real source of an *actio pro socio* case that doesn't involve genuine fraud at all.

---

## 8. Societates and Notable Businesses

A direct, concrete connection to Notable Businesses' own Merge mechanic (§8 of that document): a Merge is often, mechanically, exactly a new Societas being formed between two previously separate business owners — the same negotiation this document's own §5 already describes, simply resulting in one combined Notable Business record afterward rather than two competing ones. Similarly, a joint Acquisition of a third business — two households pooling capital specifically to buy out a struggling competitor rather than one household doing it alone — is a genuine Societas Unius Rei formed for exactly that single purpose, dissolving once the acquired business is successfully absorbed or resold.

---

## 9. Cross-System Integration

- **Land Ownership & Real Estate:** this document is the direct deep-dive extension of that document's own §7 Societas section — the *lex Claudia* motivation, the maritime-loan use case, and the existing SocietasRecord dissolution triggers are all inherited rather than redefined.
- **Notable Businesses:** §8 ties this document's own formation and dissolution mechanics directly to that document's own Merge behavior (§8 of that document) and joint-acquisition scenarios.
- **Economy & Finance:** §3's unlimited liability is this document's own direct new pathway into that document's existing Insolvency mechanic (§9 of that document) — a household can now go Insolvent because of a partner's own failure, not only its own.
- **Legal & Court:** §6's *actio pro socio* is a real, new case type built on that document's own existing evidence-and-Hearing structure (§5 of that document) rather than an invented parallel process.
- **Traits:** Ambition and Greed (already read by Land Ownership & Real Estate's own SocietasRecord) directly determine a partner's own likelihood of triggering §7's own dispute types.
- **Merchant Families & the Equestrian Order:** §4's Dominant Partner and Silent Partner governance models are this document's own concrete mechanical expression of that document's own equestrian-front pattern (§4, §7 of that document).
- **Dynasty Chronicle:** a dramatic *actio pro socio* case, a catastrophic unlimited-liability collapse, or a successful long-running Societas Omnium Bonorum are all real, tiered material.

---

## 10. Data Model

```
Societas {                              // extends Land Ownership & Real Estate's own SocietasRecord
  societasId,
  partnershipType,                       // "unusRei" | "omniumBonorum" | "publicani" — §2
  governanceModel,                        // "equalPartners" | "dominantPartner" | "silentPartner" — §4
  lexSocietatis: {                        // §5 — the negotiated formation terms
    profitSplit, durationOrPurpose, dominantPartnerId,
  },
  partners: [ { ownerType, ownerId, shareFraction, liabilityExposure: "unlimited" } ],   // §3
  linkedPropertyOrVentureId,
  linkedPublicanusContractId,               // nullable — set only for publicani-type societates
}

ActioProSocioCase {                        // §6 — new Legal & Court case type
  caseId, societasId,
  filingPartnerId, respondentPartnerId,
  disputeType,                              // "suspectedFraud" | "profitDistributionDisagreement" | "earlyExitDispute"
  resolution,                                 // reuses Legal & Court's own Ruling range
}

UnlimitedLiabilityEvent {                    // §3 — the dramatic consequence this document introduces
  householdId, societasId,
  triggeringPartnerFailure: bool,
  personalNetWorthExposed: bool,               // true — the defining fact distinguishing this from ordinary business loss
  resultingInsolvency: bool,
}
```

---

## 11. Open Questions

- **All numeric sizing**, per this project's standing convention — profit-split defaults, unlimited-liability exposure curves, and dispute-resolution probabilities are all unsized.
- **Whether a partnership agreement can ever internally cap liability between partners**, even though it couldn't shield either from a third-party claim under real Roman law — this document treats unlimited liability as absolute per §3, consistent with the real historical fact, but doesn't address whether an internal indemnification side-agreement between partners has any mechanical standing.
- **Multiple simultaneous societates.** Whether a household can hold membership in more than one Societas at once, and how liability exposure stacks if it does, isn't addressed.
- **Silent Partner detection risk.** §4 notes this governance model protects a prominent partner's own public Dignitas, but doesn't specify whether that protection can fail — a Scandal (Scandal §6) or an *actio pro socio* case (§6) publicly revealing a supposedly silent partner's real involvement.
- **Societas Omnium Bonorum's own real-world rarity.** §2 flags this as genuinely rare in this project's own terms; the precise conditions under which an NPC household would ever actually offer or accept one aren't specified.
