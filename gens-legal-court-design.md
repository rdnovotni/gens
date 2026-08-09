# GENS — System Design: Legal & Court (§6.16)
*The most cross-referenced undesigned system in this project resolves here — debt disputes, Sumptuary enforcement, Succession challenges, slave-ownership claims, war-captive dispositions, and patria potestas itself all land in one place. This pass adds a real filing cost and a dismissal penalty so "a case that can't win but can still hurt" isn't a consequence-free weapon, lets a presiding magistrate's leanings be scouted before a Hearing the same way Military & Combat's Reconnaissance already works, and wires in Espionage's blackmail material as a real, if uglier, source of leverage.*

---

## Contents

1. Scope & Role
2. Case Types
3. Presiding — Who Actually Judges
4. Quick Resolution — Routine Disputes
5. Major Cases — The Full Process
6. Patria Potestas — Authority and Its Real Limit
7. Bribery, Reputation & Patronage's Thumb on the Scale
8. Testimony & Evidence
9. Verdicts & Consequences
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "formal disputes and lawsuits over land, contracts, debts, or slave ownership; *patria potestas* mechanics played with full historical weight... magistrate rulings shaped by reputation, bribery, and patronage." The core doc's own Open Design Questions flagged this directly: "§6.16 establishes the fiction... but not yet the actual decision tree or how deep a single case goes." This document is that decision tree.

It resolves an unusually large backlog: Economy & Finance's debt-default Legal exposure step, Politics & Patronage's Sumptuary enforcement and magistracy-loss-by-conviction, Succession & Dynasty's Declaration-validity challenges and Disownment reconciliation, Labor & Slavery's ownership/manumission disputes, Military & Combat's war-captive legal disposition, Rival Houses' extinction resolution path, Characters' Litigious/Legal Scholar traits and Request Testimony/Interrogate interactions, Companions & Court Positions' Tabularius (the record-keeping operator already named for exactly this), and Dynasty Chronicle's Faith & Scandal category.

---

## 2. Case Types

Broader than the core doc's own starting list, since everything that's accumulated needs a home:

- **Property & Land** — boundary disputes, contested plots (Estate & Settlement §7).
- **Contract** — a broken Economy & Finance §3.2 Contract.
- **Debt** — Economy & Finance §6.3-6.4's Legal exposure step, in either direction.
- **Slave Ownership** — a Slave Market warranty claim (Resources & Goods), a disputed manumission, a runaway-recapture dispute (Labor & Slavery).
- **Succession** — a Declaration's validity (Succession & Dynasty §2.2), a Disownment's reconciliation (§2.3 of that doc).
- **Criminal** — assault, murder, an escalated Coercive Interaction (Characters §9.4).
- **Political** — Sumptuary Edict violations (Politics & Patronage §8), a magistracy-loss-by-conviction case (that document's own §5.7).
- **Family** — a legitimacy challenge, a contested divorce settlement (Familia §5.1-5.2).
- **Military** — a captive's legal disposition (Military & Combat §5.5's "legalMatter" resolution).

---

## 3. Presiding — Who Actually Judges

Per the decision that both office-holding and conflict of interest matter, differently:

- **Local, routine cases** are presided over by whoever holds the relevant local magistracy (Politics & Patronage's Decurion-through-Duumvir ladder) — a real, concrete perk of office, not just a Dignitas number.
- **Automatic recusal** applies the instant the player's own household is a party — a magistrate cannot judge their own case. Recusal hands the case to a fellow Decurion or a generated NPC magistrate Character instead, exactly the same "generate on demand" principle Characters §11 already uses for any needed figure.
- **Beyond-local-scope cases** — anything touching Rome directly, a provincial matter, the most severe criminal charges — go to a provincial governor (Politics & Patronage §7) or an abstracted higher Roman court, never a local magistrate regardless of who holds that office.

**A presiding magistrate isn't a black box.** The same "informed risk, not a total black box" principle Military & Combat's Reconnaissance already established applies here directly: an Intrigue-driven inquiry or a Legal Scholar's own professional knowledge can reveal a generated NPC magistrate's relevant Axes and Traits before a Hearing ever opens — worth knowing whether the presiding figure reads as Greedy (bribery is the live option), strictly Honorable (arguing the letter of the law matters more than anything else), or Corrupt (patronage and standing outweigh the actual merits). This doesn't guarantee a favorable outcome; it just means a player walks into a major Hearing with real information rather than none.

---

## 4. Quick Resolution — Routine Disputes

Per the decision to keep minor cases light: most disputes resolve in a single weighted check, the same session they're filed. Inputs: the presiding magistrate's relevant Core Attribute (Learning for legal reasoning, Diplomacy for advocacy weight) and Personality Axes (Honor for how strictly they rule on the letter of the case, Greed for bribability, §7); each party's own case strength (Intrigue, a Legal Scholar Trait, any Testimony already in hand, §8); and — per the core doc's own explicit "shaped by reputation" note — each party's existing Dignitas as a real thumb on the scale. Justice reading as reputation-weighted rather than blind is a deliberate feature of this setting, not an oversight.

**Filing isn't free.** A real, if modest, Economy & Finance cost (advocate fees, court costs) attaches to bringing any case, scaling with depth (§5's Major cases cost meaningfully more than a Quick one) — the same kind of small friction that already keeps every other action in this project from being a costless default. **A Dismissed verdict carries a further, sharper cost specifically for the filer:** bringing a case with no real merit and having a magistrate say so outright is its own minor public embarrassment, a small Dignitas cost distinct from — and worse than — honestly losing a well-argued case at a real Hearing. This is the actual brake on §6's "can't win but can still hurt" tactic being spammed consequence-free: it can still hurt the target, but it isn't free for the filer either.

---

## 5. Major Cases — The Full Process

Per the decision to give real stakes (a contested Declaration, a capital charge, a large debt default) the same multi-stage treatment Characters' Scheme engine and Military & Combat's Sieges already established, rather than a single roll:

1. **Filing** — the dispute, the parties, and the actual stakes are named.
2. **Evidence & Testimony** — runs over real time, not an instant. Either side can gather Testimony (Characters §9.7's Request Testimony, a Legal Scholar's argument-building, an Intrigue-driven investigation) — and the other side has real counter-play: discredit a witness, bribe one off, or run a Scheme (Characters §10) to suppress evidence entirely. This is genuine back-and-forth, exactly the shape a Siege's prolonged state already uses.
3. **The Hearing** — a real, singular event, not a silent tick: both sides present what they've built, and the presiding party (§3) weighs it.
4. **The Ruling** (§9) — a genuine range of outcomes, never a flat binary.

---

## 6. Patria Potestas — Authority and Its Real Limit

Per the decision to keep this near-absolute, matching historical reality directly: *patria potestas* grants a household head real, formally unchallengeable authority over their own dependents — marriage approval and veto, formal Disownment (Succession & Dynasty §2.3, confirmed here as legally valid regardless of how it's perceived), and, in the harshest historical cases, life-and-death authority, played with the same frankness Labor & Slavery's punishment ladder already extends to enslaved workers — now applicable, distinctly and far more rarely, to a household's own free dependents. **No court can formally override an exercise of this authority against the household's own dependents.** It stands as legally valid no matter how brutal, exactly matching the real historical institution rather than softening it into something a magistrate can reverse.

**The real check is entirely social, not legal.** A severe exercise of *patria potestas* costs Dignitas at a severity scaled to the act itself, leaves a lasting relationship-web scar across every surviving family member and anyone who heard of it, and reads very differently depending on audience Faction (Politics & Patronage §3.1) — a Traditionalist audience may genuinely respect strict enforcement of household order; a Popularist one recoils. **One deliberate exception worth naming directly:** a case can still be *brought* around an exercise of this authority — by a rival, an enemy, anyone looking for a political weapon — even with essentially no chance of actually succeeding. A case that can't win but can still hurt is a real, usable political attack in its own right: a public airing, a Scandal-Marked Trait (§6.6 of that doc) for the household head regardless of the (inevitable) formal outcome, and a Dynasty Chronicle Faith & Scandal entry, all generated by a case that was always going to be legally dismissed.

---

## 7. Bribery, Reputation & Patronage's Thumb on the Scale

The core doc's own three named influences, each with a real, already-existing mechanism to plug into rather than a new one invented here:

- **Bribery** — Economy & Finance's Bribes (§4.2) apply directly to a presiding magistrate's Greed axis, the same mechanism that already governs bribability everywhere else in this project.
- **Reputation** — Dignitas is read directly into both Quick Resolution (§4) and a Hearing's weighting (§5) — the higher a party's standing, the more favorably a close case tips before any evidence is weighed at all.
- **Patronage** — a Legal-Specialty Clientela favor (Politics & Patronage §4.2) is direct representation in a case; a presiding magistrate who's the player's own Client, or whose house holds an Allied Standing (Rival Houses §5.2), starts predisposed favorably before the Hearing even opens.

---

## 8. Testimony & Evidence

Characters' Request Testimony (§9.7) and Interrogate (same section) are the concrete tools — no new interaction invented. A witness's own Honor axis determines whether they hold up under pressure, a bribe, or a threat; a Legal Scholar Trait (Traits §5.3) gives real argument-construction weight beyond raw Learning; and physical evidence traces back to whatever underlying record actually generated the dispute — a DebtRecord (Economy & Finance), a Slave Market warranty claim (Resources & Goods), or a punishment record (Labor & Slavery). **A further, sharper-edged source worth naming:** blackmail material generated through Espionage (§6.15, future) is a natural, if ethically uglier, fit here too — leverage that either strengthens a case directly or, more often, quietly convinces the opposing party to drop one before it ever reaches a Hearing at all, an off-the-books resolution this document doesn't need to formally model beyond acknowledging it's a real, available option once that system exists.

---

## 9. Verdicts & Consequences

Real outcome diversity, matching the same philosophy Characters' Schemes, Military & Combat's engagements, and Sieges all already established — never a flat binary:

- **Dismissed** — insufficient case either way.
- **Ruled for Plaintiff / Ruled for Defendant** — a clean resolution.
- **Split or Compromise Ruling** — a real middle outcome, common for property and contract disputes specifically.
- **For criminal/capital cases:** Acquitted, or Convicted with an actual sentence — a fine, exile (feeding the Exiled Reactive Trait, Traits §6.10), debt bondage (Economy & Finance §6.4's own mechanism), or, at the extreme, execution.

Every real verdict ripples outward rather than resolving in isolation: a Dignitas shift for both parties, a relationship-web scar, an asset transfer (Economy & Finance), a Legal Status change (Labor & Slavery), and — per Dynasty Chronicle §6 — a real entry, tiered by the case's own severity.

---

## 10. Cross-System Integration

- **Economy & Finance:** debt disputes (§6.3-6.4) and their Legal exposure step finally have an actual resolution mechanism rather than a named-but-unbuilt one.
- **Politics & Patronage:** Sumptuary enforcement (§8) and magistracy-loss-by-conviction (§5.7) both resolve here directly; local presiding (§3) is a real, added incentive to hold office.
- **Succession & Dynasty:** a Declaration's validity and a Disownment's reconciliation (§2.2-2.3 of that doc) both get their actual legal mechanism.
- **Labor & Slavery:** ownership disputes, warranty claims, and manumission's formal validity all resolve through this document.
- **Military & Combat:** a captured commander's "legalMatter" resolution (§5.5) is fully realized here.
- **Rival Houses:** an extinct house's holdings disposed of by legal ruling (§5.3) is one of that document's own three case-by-case paths, now designed.
- **Characters:** Request Testimony, Interrogate, and the Scheme engine are all reused directly for §5.2's evidence stage rather than inventing parallel mechanics.
- **Traits:** Litigious/Conflict-Averse and Legal Scholar (§6.6, §5.3 of that doc) both find their concrete mechanical home; Scandal-Marked/Rehabilitated is this document's direct output for a severe or politically-weaponized case.
- **Espionage (§6.15, future):** blackmail material is a natural, if uglier, source of leverage here — either strengthening a case directly or convincing the other side to drop one before a Hearing.
- **Companions & Court Positions:** the Tabularius (that document's own §5.2) is this document's named record-keeping operator, feeding case filings and the Dynasty Chronicle alike.
- **Dynasty Chronicle:** every real verdict, and especially a capital case or a politically-weaponized *patria potestas* case (§6), is real material for that document's Faith & Scandal and Politics & Office categories.
- **Familia:** *patria potestas*'s marriage-approval and Disownment authority are confirmed here as legally absolute, closing that document's own forward reference.

---

## 11. Data Model

```
LegalCase {
  caseId,
  caseType,          // "propertyLand" | "contract" | "debt" | "slaveOwnership" | "succession" |
                       // "criminal" | "political" | "family" | "military"
  plaintiffId, defendantId,
  presidingCharacterId,    // null until assigned; recused automatically if either party overlaps the presider's own household
  presidingCharacterScouted: bool,   // §3 — whether the presider's Axes/Traits were revealed before the Hearing
  depth,              // "quick" | "major"
  stage,              // "filed" | "evidenceGathering" | "hearing" | "ruled"  (quick cases skip straight to "ruled")
  filingCostPaid,       // §4 — scales with depth
  evidenceGathered: [...], testimonyGiven: [...],
  bribesOffered: [...],
  monthsRunning,        // relevant only for "major"
  verdict,             // "dismissed" | "plaintiff" | "defendant" | "splitCompromise" |
                        // "acquitted" | "convicted"
  dismissalDignitasPenalty,   // §4 — applied to the filer specifically, only when verdict is "dismissed"
  sentence,            // set only if "convicted": "fine" | "exile" | "debtBondage" | "execution"
  isPatriaPotestasCase: bool,   // §6 — flags the "can't win but can still hurt" category
}
```

---

## 12. Open Questions

- **All numeric sizing.** Consistent with this project's convention: the Quick Resolution weighting formula, a major case's typical duration, verdict-probability curves, the filing-cost scale, and the Dismissed-verdict Dignitas penalty are all unsized.
- **Small-settlement recusal chain.** §3's automatic recusal assumes another local magistrate is available; what happens in a settlement small enough that every Decurion has some conflict of interest isn't specified.
- **Appeal process.** Whether a Ruling can be appealed to a higher/provincial court, and what that actually costs, isn't addressed — a plausible future extension rather than a gap this pass resolves.
- **Patria potestas severity thresholds.** §6 establishes a scaled Dignitas cost without specifying the actual curve, or precisely which acts count as severe enough to trigger the political-weaponization exception.
- **Multi-party cases.** §5's process describes a clean plaintiff/defendant shape; a dispute with more than two real parties (three heirs contesting the same estate, say) isn't explicitly handled.
