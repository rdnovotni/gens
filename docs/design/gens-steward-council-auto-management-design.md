# GENS — System Design: Steward/Council Auto-Management (§6.28)
*The quality-of-life system nearly every other document has already been quietly leaning on: Companions & Court Positions' Procurator, Travel's away-from-home household, and Succession & Dynasty's own Regency for a minor heir have all explicitly named this system as the principle they're built on without it ever being designed. One shared framework now covers all three — a real, player-set Autonomy Level dial, a genuine competence-and-Loyalty stake in who's actually left in charge, and, per direction, real embezzlement risk rather than a purely cosmetic quality scale. Scope stays exactly where the core doc put it: QoL, not new simulation depth — sensible default handling of routine business, with anything consequential still held for the player's return.*

---

## Contents

1. Scope & Role
2. The Steward — Who Actually Runs Things
3. Autonomy Level — A Real, Player-Set Dial
4. Always-Held Decisions — The Floor Beneath Every Autonomy Level
5. Competence — Stewardship Quality in Practice
6. Loyalty & Betrayal — The Real Risk of Unsupervised Trust
7. Three Contexts, One Framework
8. The Return Report
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "a trusted appointee can run routine estate business while the player character is away via Travel, so leaving home doesn't mean the household simply freezes for the duration. Scope is QoL rather than simulation depth: sensible default handling of day-to-day decisions, with anything consequential still held for the player's return." Per direction, this document does three things at once: it names who actually fills this role (§2), gives the player real, configurable control over how much autonomy that person has (§3), and — the one place this document deliberately goes further than "pure QoL" — makes who you trust with that autonomy a real, felt decision with real consequences (§5–6), because a system about delegating authority isn't honest if delegating it is always safe.

---

## 2. The Steward — Who Actually Runs Things

No new role. This document formalizes how three already-named appointees actually function once they're the one holding the reins:

- **The Steward (Dispensator)** — Companions & Court Positions' own Villa-scale Senior Position, the default appointee for an ordinary Travel absence.
- **The Procurator** — that same document's §5.3 Second-Settlement appointee, already permanent and ongoing rather than tied to any single trip.
- **The Regent** — Succession & Dynasty's own §6.2 appointee for a minor heir's regency: the surviving spouse where one exists and is willing (in which case, worth restating plainly, the *player controls the Regent directly* — this document's own automation applies to the alternative case, a non-family appointee such as a Rationalis or Procurator standing in instead).

### 2.1 The Council — When One Steward Isn't Enough

Worth naming directly, since this system's own title promises it and the first pass only designed half: a single generalist Dispensator handling literally everything — finances, religious observance, legal filings, military readiness — makes real sense for a modest household, but stops being realistic once an estate has grown large enough to have actually filled Companions & Court Positions' own specialized Senior Positions (a Rationalis for finance, a Praefectus for military readiness, a Tabularius for legal and record matters, a Procurator for a second settlement). For a household at that stage, autonomous management during an absence runs as a genuine **Council** instead: each domain-specific Senior Position holder makes autonomous decisions within their own portfolio, at whatever Autonomy Level the player has set, rather than one person attempting to competently judge every domain at once.

Where a Rationalis is filled, that position — already described elsewhere as a real aggregate, cross-domain financial-oversight capstone — serves as the Council's own natural first-among-equals, the point of contact for anything genuinely cross-domain or for breaking a tie between two Council members who'd otherwise handle a borderline situation differently. Where no Rationalis exists, the ordinary Steward remains the single point of contact, and the household simply hasn't grown into the Council model yet.

**The real tradeoff this creates:** a single Steward is a single point of failure — one disloyal or incompetent appointee and the whole estate's autonomous management suffers at once — while a genuine Council spreads that same risk (and that same competence) across several people, at the real cost of now needing several trustworthy appointees instead of just one. Neither is strictly better; it's the natural, legible consequence of how developed a household's own administrative structure actually is.

---

## 3. Autonomy Level — A Real, Player-Set Dial

Per direction, mirroring Events' own Manual Mode (§7 of that document) rather than a fixed, non-adjustable boundary:

| Level | Behavior |
|---|---|
| **Conservative** | The Steward handles only the most routine upkeep — ordinary wage payment, routine Repair, maintaining Standing Policies exactly as already set. Nearly everything else queues and waits. The safest setting, at a real cost: opportunities that could have been seized promptly simply sit unaddressed until the player's return. |
| **Standard** *(default)* | The core doc's own described behavior exactly: sensible day-to-day handling, with anything genuinely consequential still held. |
| **Full Autonomy** | The Steward is empowered to make real, independent calls within the Always-Held floor (§4) — adjusting a Standing Policy tier if circumstances shift, resolving a Flagged-for-Choice Event using their own judgment, funding a modest Funded Action. The fastest, most hands-off setting, and the one where §5–6's own stakes actually matter — the decisions made reflect the Steward's own competence and character, not the player's. |

The level can be set per-assignment (§7) and, per Correspondence & Letters' own already-named Written Instructions action, adjusted remotely mid-absence without the player needing to physically return.

---

## 4. Always-Held Decisions — The Floor Beneath Every Autonomy Level

Even at Full Autonomy, a real, fixed set of decisions never auto-resolve — no Steward, however trusted, acts on these without the player: issuing an Edict, any Household Doctrine-defining choice, arranging or approving a marriage, initiating a military campaign, any Legal & Court action of real severity, Diplomacy with Non-Roman Peoples' own Alliance Against Rome (obviously), a specific Manumission decision for a named individual, and anything touching Succession itself. This is a deliberate, sensible design floor — Full Autonomy means broad discretion over ordinary running of the estate, never a blank check over the household's own defining choices.

---

## 5. Competence — Stewardship Quality in Practice

The Steward's own Stewardship Core Attribute (and, for more complex situations, Learning) genuinely determines how well autonomous decisions actually land, especially at Standard and Full Autonomy. A highly capable Steward left in charge makes real, sound calls; a mediocre one risks real, felt mistakes — a missed Repair that lets Natural Disaster damage compound, a Flagged Event resolved poorly, a Funded Action spent on the wrong target. This is the direct, concrete reason *who* the player appoints matters beyond simple availability — the same "labor continuity, demonstrated performance" logic Companions & Court Positions already applies to the Overseer ladder, now mattering for the single most consequential appointment a household can make.

---

## 6. Loyalty & Betrayal — The Real Risk of Unsupervised Trust

Per direction, a genuine risk rather than a second competence dial. A Steward's own Loyalty (the same Condition Stat this project already tracks everywhere else) determines a real, standing background risk across any extended period of unsupervised authority, especially with real Treasury access:

- **Skimming** — a low-Loyalty Steward has a real, ongoing chance of quietly diverting a modest sum for themselves across a long absence, discovered only on the player's return or through an active Tabularius audit.
- **Embezzlement** — rarer, and far more severe: a genuinely disloyal Steward at Full Autonomy over a long stretch can inflict a real, substantial Treasury loss, and — once discovered — a real Legal & Court case opportunity against them.
- **Active sabotage** — the rarest and darkest possibility, reserved for a Steward whose Loyalty has collapsed toward outright hostility, or one a rival house has specifically suborned (a direct, real Espionage and Rival Houses tie): deliberately undermining a Standing Policy or an Overseer's own position on purpose, not merely skimming for personal gain.

**The real, felt tradeoff this creates:** a highly capable but Resentful or low-Loyalty Steward is a genuine risk precisely because of the authority Full Autonomy grants them; a fiercely loyal but less capable one is safer at a real cost to how well things actually run. Choosing who to leave in charge is never a purely mechanical "pick the highest Stewardship number" decision — it's the same character-judgment call this whole project asks of every other appointment, now carrying real stakes for the single role with the least direct oversight.

**A rival's own opportunity, worth naming directly:** a known, extended player absence — a long Travel trip, an obvious Regency — is a real, visible window a rival house can specifically try to exploit, whether through outright bribery of an existing Steward or by working to have their own preferred candidate installed in the role in the first place.

---

## 7. Three Contexts, One Framework

Per direction, one shared system rather than three disconnected assumptions, distinguished only by duration and stakes:

- **Ordinary Travel Absence** — the shortest and lowest-stakes context; the Villa-stage Steward operates for the trip's own duration, Autonomy Level set for that single assignment.
- **The Second-Settlement Procurator** — permanent and ongoing rather than tied to any single trip; **defaults to Standard Autonomy** rather than Full, even though real necessity often pushes a player to raise it — Full Autonomy's own real risk (§6) shouldn't be the unexamined starting point even for a role this project already trusts with real, ongoing independence. Travel's own §7 "Arrival" encounter remains the mechanism for a personal, in-the-flesh override whenever the player does visit.
- **Regency for a Minor Heir** — the longest and highest-stakes context by a wide margin, potentially running for real years rather than months. This is where §5–6's own competence and Loyalty stakes matter most: a multi-year regency run at Full Autonomy under a mediocre or disloyal Regent is a genuine, dynasty-level risk to the very estate the heir eventually inherits — a real, dramatic stake in its own right, and a direct, mechanical expression of Design Pillar #7's "Memory has weight" at its most exposed. **A light, resolved note on the heir's own voice:** an Adolescent heir approaching majority carries no formal authority over their own Regent's Autonomy Level or appointment, but their own relationship-web opinion of the Regent (Characters' existing schema, unchanged) is real and readable — a ward who's come to resent a controlling Regent, or one who's grown to trust and admire them, is exactly the kind of texture the eventual handoff (Succession & Dynasty §6.1) should carry forward into the new head's own first real decisions.

---

## 8. The Return Report

Mirroring Events' own Monthly Report pattern rather than inventing a separate digest format: upon Travel's return, or at a Regency's natural end when the heir comes of age, the player receives a real, readable compiled account of everything the Steward or Regent actually did — not a raw stat delta, a genuine narrative summary. Any Skimming or Embezzlement discovered lands here as a real, dramatic reveal specifically, and a sufficiently eventful absence or regency — a genuinely well-run one, or a genuinely disastrous one — is real Dynasty Chronicle material in its own right.

**A light, real texture worth adding for a genuine Council (§2.1):** where more than one Senior Position holder shared authority during the absence, the Return Report can carry a real note of internal disagreement — a Rationalis pressing for economy where a Praefectus wanted readiness funded instead, resolved one way or the other before the player ever returns. This is flavor, not a new mechanic requiring player adjudication after the fact, but it's exactly the kind of texture that makes a Council read as several real people governing together rather than one abstracted management value.

---

## 9. Cross-System Integration

- **Companions & Court Positions:** the Steward, Procurator, and Rationalis are the named appointee roles this document actually operationalizes; nothing about their own staffing mechanics changes.
- **Policies & Edicts:** a household's own current Standing Policies and any saved Playbook are literally what a Steward "follows" during autonomous operation — the concrete instruction set this document's own automation reads rather than reinventing.
- **Events:** Autonomy Level is a direct, named extension of that document's own Manual Mode dial; Auto-Resolved versus Flagged-for-Choice categorization is exactly what a Steward executes differently at each Autonomy tier.
- **Travel:** this document is the QoL layer that document's own §9 already named as covering an absent household.
- **Succession & Dynasty:** Regency's own automation is now fully specified here rather than only gestured at.
- **Correspondence & Letters:** Written Instructions to a Distant Appointee is the concrete, existing mechanism for adjusting Autonomy Level or standing instructions mid-absence.
- **Economy & Finance:** Treasury access is the real, direct stake behind Skimming and Embezzlement.
- **Legal & Court:** a discovered severe Embezzlement is a real, named case trigger.
- **Espionage / Rival Houses:** a suborned Steward, and a rival's own opportunistic targeting of a known absence, are both real, direct ties.
- **Dynasty Chronicle:** a notably well- or badly-run absence or regency is guaranteed-or-near-guaranteed material.

---

## 10. Data Model

```
StewardshipAssignment {
  assignmentId, householdId,
  context,                       // "travelAbsence" | "secondSettlementProcurator" | "regency"
  mode,                            // new — "singleSteward" | "council" — §2.1
  appointeeCharacterId,             // set for mode "singleSteward"
  councilMembers: [                  // new — set for mode "council"
    { positionType, characterId, domain }   // e.g. "rationalis" handling "finance", "praefectus" handling "military"
  ],
  councilHeadCharacterId,             // new — the Rationalis if filled, else null
  autonomyLevel,                    // "conservative" | "standard" | "fullAutonomy"
  startMonth, endMonth,               // endMonth null while ongoing (Procurator, active regency)
  activePlaybookRef,                    // Policies & Edicts' own PolicyPlaybook, if one is in use
}

AutonomousDecisionLog {
  logId, assignmentId, month,
  decisionType,
  outcome,
  competenceRollFactor,               // reads the appointee's own Stewardship/Learning
  loyaltyRiskRollFactor,                // reads the appointee's own Loyalty
  incidentType,                          // null | "skimming" | "embezzlement" | "activeSabotage"
}

ReturnReport {
  reportId, assignmentId,
  summaryEntries: [ ... ],
  totalTreasuryImpact,
  incidentsDiscovered: [ AutonomousDecisionLog, ... ],
  chronicleWorthy: bool,
}
```

---

## 11. Open Questions

- **All numeric sizing.** Consistent with this project's convention: exact Autonomy-tier boundaries, competence/Loyalty roll formulas, and incident-severity distributions are all unsized.
- **Always-Held list completeness.** §4 names the clearest, highest-stakes examples; whether every other document's own most-consequential single action needs an explicit entry on this list isn't fully enumerated.
- **Council formation's exact threshold.** §2.1 establishes that filling specialized Senior Positions is what unlocks the Council model over a single generalist Steward, without specifying exactly how many, or which combination, of those positions need to be filled before the household is treated as having "grown into" it.
- **Council tie-breaking without a Rationalis.** §2.1 names the Rationalis as the natural first-among-equals; how a genuine cross-domain disagreement resolves for a household running a real multi-position Council but without that specific role filled isn't specified.
- **Embezzlement's exact discovery mechanics** beyond "the Return Report or an active Tabularius audit" — how proactive an audit actually needs to be, and its own real cost, isn't specified.
