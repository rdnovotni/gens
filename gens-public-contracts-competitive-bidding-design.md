# GENS — System Design: Public Contracts & Competitive Bidding (§6.41, new)
*A real gap hiding across three separate documents: Land Ownership & Real Estate built the Publicani tax-farming contract, but the equally real Roman institution of contracted public-works construction (the *redemptor*) and Economy & Finance's own flagged-but-unbuilt military/provincial supply contracts never got the actual competitive process that made all three the same underlying Roman institution — *locatio-conductio publica*, the state letting its own business out to the highest-standing bidder. This document builds that shared process once, resolves Collegia & Guilds' own flagged census-cycle question along the way by introducing a real Censor magistracy, and gives Legal & Court a genuine corruption case type extending the Verres precedent Sicily already established.*

---

## Contents

1. Scope & Role
2. The Censor — A New Magistracy for Contracts and the Census
3. The Lustrum — The Five-Year Cycle, Resolved
4. Three Real Contract Types
5. The Bidding Process
6. Contract Fraud — A Real Historical Precedent
7. Cross-System Integration
8. Data Model
9. Open Questions

---

## 1. Scope & Role

This document doesn't rebuild anything already standing: Land Ownership & Real Estate's Publicanus Contract (§8 of that document) and its Collection Intensity dial stay exactly as designed; Economy & Finance's Military and Provincial Supply Contracts (§3.2) stay as named income categories. What none of them ever got is the actual **competitive process** by which a contract is won in the first place — until now, each simply existed as something a qualifying household could obtain. This document supplies that missing layer once, as a shared mechanic every contract type reuses, rather than three separate bidding systems.

---

## 2. The Censor — A New Magistracy for Contracts and the Census

Politics & Patronage's local ladder (Decurion → Aedile → Quaestor → Duumvir) never named the one Roman office actually responsible for state contracts: the **Censor**. Real historical practice, preserved directly here: Censors were elected in a genuine pair, exactly like the Duumvirate, but held office only for the duration of the census itself rather than a standard annual term — a real, distinct rhythm worth keeping rather than flattening into another yearly seat.

A sitting Censor holds two real, concrete powers this document builds around:

- **Locatio Censoria** — the actual authority to open bidding on, and formally award, every contract type in §4.
- **The Census** — a formal reassessment of every household's Net Worth (Economy & Finance §8), the real mechanism directly gating Merchant Families & the Equestrian Order's own wealth threshold (§2 of that document) and Politics & Patronage's own Senate property-census gate (§6 of that document).

The Censorship sits above Duumvir in real historical prestige, and this document treats it that way — a genuine capstone office, gated on having already held Duumvir at least once, extending Politics & Patronage's own ladder rather than replacing any rung on it.

---

## 3. The Lustrum — The Five-Year Cycle, Resolved

Collegia & Guilds flagged this directly: the real census interval (the **lustrum**, historically five years) never had a stated relationship to this project's own monthly tick. This document resolves it concretely, since it's a real historical fact rather than a number needing a future balancing pass: **every 60 months, a Lustrum fires** — a Censor election (if the office is vacant or contested), a full Net Worth reassessment across every tracked household, and a mandatory re-bidding of every standing contract in §4, whether or not its current holder is performing well. A contract can still be issued or renewed outside the Lustrum for a genuine, urgent need (a sudden campaign's supply crisis, an unplanned public work), but the Lustrum is the real, recurring moment when the whole system resets at once — and, not incidentally, the same beat Collegia's own Quinquennalis finally has a real trigger to point at.

---

## 4. Three Real Contract Types

### 4.1 Publicani — Tax Farming *(recap, unchanged)*

Land Ownership & Real Estate §8 stands as designed — the Collection Intensity dial, the corruption-case exposure at Aggressive settings, the Merchant Families & Equestrian Order eligibility (§2 of that document). This document supplies the actual bidding process (§5) that determines who holds it.

### 4.2 Redemptores — Public Works Construction

The genuinely new type: Public Works & Euergetism (§3 of that document) already established *who pays* for an aqueduct, road, or basilica — a private patron's euergetism, or the state's own tax revenue. This document is *who builds it*: a **Redemptor**, a real, historically attested contractor who bids for and executes the actual construction, distinct from the funding patron whose name goes on the inscription. A state-funded Public Work (Public Works & Euergetism §7) now routes through this document's own bidding process by default; a privately-funded one can either use a hired Redemptor the same way, or, for a household with its own construction capacity, be built directly — the bidding process is the state's own procurement method, not a requirement placed on private patrons.

### 4.3 Military & Provincial Supply Contracts

Economy & Finance §3.2 named these — Grain, Weapons, Horses, and Siege Engines supplied to the legions at a premium, or bulk grain/timber supplied to a province's own administration — without ever giving them real competition. This document is that competition: a genuine bidding war among households and businesses capable of actually delivering at scale, particularly sharp during an active Military & Combat campaign, when Rome's own need is urgent and a contractor's failure to deliver carries real, immediate consequence beyond an ordinary missed sale.

---

## 5. The Bidding Process

Per direction, a genuine multi-actor process rather than a player-vs-abstract-market check. When a contract opens (at a Lustrum, or ad hoc per §3), every eligible party — the player's own household, a Notable Business (via its own Government Contracts hook, Notable Businesses §7), a Societas formed specifically to pool capital for a large bid, a Merchant Family/Equestrian household, or a generated Rival House competitor — can submit a bid, evaluated on three real inputs:

1. **Price** — the baseline: a lower bid (for Publicani, the upfront sum paid to Rome; for a Redemptor or supply contract, the price Rome pays the contractor) is the obvious lever, and the one most bidders lean on hardest.
2. **Reliability** — a bidder's own track record: a Notable Business's Reputation (§4 of that document), a household's own accumulated Dignitas, or simple past-performance history (a prior contract completed cleanly versus one that ran into trouble) all weight this directly — the Censor, and Rome behind them, has a real, historically accurate reason to prefer a known, trustworthy hand over the cheapest possible stranger.
3. **Influence** — political weight spent directly on securing the award: Politics & Patronage's Influence resource (§4.4 of that document), or, at real risk, an outright bribe (Economy & Finance's existing Bribes category, §4.2) aimed at the sitting Censor personally.

The award isn't purely mechanical arithmetic dressed as a formula — a Censor sympathetic to a bidder's Faction (Traditionalist/Popularist, Politics & Patronage §3.1), already personally connected via Clientela, or simply corrupt, can and does weight the decision in ways a purely price-and-reliability calculation wouldn't predict, giving a contested award real political texture rather than reading as a spreadsheet outcome.

### 5.1 Losing a Bid

A rejected bidder isn't merely out of luck — a bidder who believes the process was corrupted (an obviously worse bid winning on suspiciously thin grounds) has a real, standing motive to investigate, and, per §6, a real legal avenue if they find something.

---

## 6. Contract Fraud — A Real Historical Precedent

A genuinely vivid piece of real Roman history, well worth building around directly: in 212 BC, during the Second Punic War, state contractors responsible for shipping supplies to the army in Spain were caught deliberately overloading old, barely-seaworthy vessels with cheap goods, then staging the ships' loss at sea to collect the state's own insurance guarantee on cargo that was, in truth, worth far less than declared — a real, documented state-contract fraud scandal, not an invented mechanic.

### 6.1 The Mechanic

A contract holder can choose, quietly, to **cut corners** — a Redemptor using inferior materials while billing for the specified grade, a supply contractor shorting an actual shipment while declaring the full amount, a Publicanus skimming beyond even an Aggressive Collection Intensity's already-elevated take. This resolves exactly like any other concealed action in this project's shared Scheme engine (Characters §10): a real, quiet margin gain, a Discovery risk that rises the longer it continues and the more people are in a position to notice, and genuine counter-play once suspicion crosses a threshold.

### 6.2 Discovery and Prosecution

Per direction, this carries real Legal & Court weight rather than staying Scandal-only, extending the same **repetundae** (extortion/corruption) case type Sicily's own Verres precedent (Starting Regions: Sicily §4, §15.3) already established for a different office — a contract holder caught defrauding the state faces a real, formal prosecution, not merely a Dignitas markdown. A conviction carries the expected weight: restitution, a real Dignitas and Legal Status consequence, and, concretely, **permanent or long-term disqualification from future contract bidding** — the state's own genuine, lasting response to being defrauded once, distinct from and sharper than an ordinary business's own Reputation recovering the normal way after a bad outcome.

---

## 7. Cross-System Integration

- **Land Ownership & Real Estate:** Publicani (§8 of that document) is recapped, not rebuilt; this document supplies its actual award mechanism.
- **Public Works & Euergetism:** the Redemptor (§4.2) is the concrete answer to *who builds* what that document's own §3 establishes patrons *fund*.
- **Economy & Finance:** Military and Provincial Supply Contracts (§3.2) finally get real competition; the existing Bribes category (§4.2) is directly reused for §5's Influence input.
- **Politics & Patronage:** the Censor (§2) extends that document's own ladder above Duumvir; Influence (§4.4) and Faction (§3.1) are both direct inputs to §5's award weighting.
- **Merchant Families & the Equestrian Order:** the Lustrum's Net Worth reassessment (§3) is the concrete, periodic mechanism behind that document's own equestrian wealth-threshold check; Publicani eligibility (§2 of that document) remains that document's own territory.
- **Collegia & Guilds:** the Lustrum (§3) directly resolves that document's own flagged "Quinquennalis's own actual census-cycle trigger" open question.
- **Notable Businesses:** Government Contracts (§7 of that document) now route through a real competitive award rather than existing as an unconditional standing relationship.
- **Societates & Business Partnerships:** a large Redemptor or supply bid is a natural, concrete reason to form a Societas Unius Rei specifically to pool the needed capital or capacity.
- **Interest Groups:** the Publicani and Equestrian Trade Interests bloc (§2 of that document) has a direct, real stake in every Lustrum's outcome.
- **Rival Houses:** a generated Rival House competitor is a real, named bidding rival, not merely an abstract market participant.
- **Military & Combat:** an active campaign's own supply need is §4.3's most urgent, highest-stakes trigger for an ad hoc contract outside the normal Lustrum cycle.
- **Legal & Court:** §6.2's repetundae-style prosecution is a genuine new case type, directly extending the Verres precedent to an ordinary contract holder rather than only a provincial governor.
- **Scandal:** a discovered fraud that stops short of formal prosecution, or the public fallout following a conviction, is a real, new Scandal source.
- **Starting Regions: Sicily:** Verres's own real historical prosecution (§4, §15.3 of that document) is this document's direct precedent and narrative anchor for §6.

---

## 8. Data Model

```
CensorTerm {
  censorPairIds: [ characterId, characterId ],   // a real, paired office per §2
  settlementId,
  startMonth, endMonth,               // duration = the census itself, not a fixed annual term
}

LustrumEvent {                       // §3
  month,                              // fires every 60 months
  netWorthReassessments: [ { householdId, newNetWorth } ],
  contractsReopenedForBid: [ contractId, ... ],
}

PublicContract {
  contractId, settlementOrProvinceId,
  type,                    // "publicani" | "redemptor" | "militarySupply" | "provincialSupply"
  currentHolderId,           // householdId, notableBusinessId, or societasId
  awardedMonth, termEndMonth,
  awardedViaLustrum: bool,    // false if ad hoc, per §3
  isCuttingCorners: bool,      // §6.1 — hidden until discovered
  fraudDiscoveryRisk,
}

ContractBid {                 // §5
  bidId, contractId, bidderId, bidderType,   // "player" | "notableBusiness" | "societas" | "merchantFamily" | "rivalHouse"
  priceOffered,
  reliabilityScore,             // read from Reputation/Dignitas/past performance
  influenceSpent, bribeAttempted: bool,
  outcome,                    // "won" | "lost" | "lostContestedLegally"
}

ContractFraudRecord {            // §6.2
  recordId, contractId, holderId,
  discoveredMonth,
  legalOutcome,                // "acquitted" | "convicted"
  disqualifiedFromBidding: bool,
}
```

---

## 9. Open Questions

- **All numeric sizing** beyond the Lustrum's own real 60-month historical interval — bid-weighting formulas, reliability scoring, and disqualification duration are all unsized, per convention.
- **Whether the Censorship should carry any function beyond §2's two named powers.** The real historical office also carried moral oversight (the *regimen morum*) with real teeth (expelling a Senator for misconduct); this document deliberately doesn't build that half, leaving it as a possible future tie-in to Sumptuary enforcement or Scandal if ever wanted.
- **Ad hoc contract frequency outside the Lustrum.** §3 allows urgent contracts between Lustra but doesn't specify how commonly Military & Combat or a sudden Natural Disaster should actually trigger one.
- **Rival House bid AI depth.** §5 treats a Rival House bidder as a real competitor but doesn't specify how sophisticated its own bidding logic should be, consistent with Rival Houses' own still-open AI-depth question.
- **Whether a disqualified bidder (§6.2) can ever petition for reinstatement**, and under what real political circumstance — left open, in the same spirit as Monuments & Legacy Building's own rare Damnatio Memoriae reversal.
