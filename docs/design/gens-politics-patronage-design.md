# GENS — System Design: Politics & Patronage (§6.5)
*The power-and-standing pillar every other document has been drawing lines around — the Curia's magistracies, the cursus honorum, the Mint and Praetorium's political gates, Economy & Finance's inbound tax authority and Publicanus contract, Companions & Court Positions' explicit line against public office. This is where all of it actually resolves. This pass adds Faction, a Curia that's a real body of people, term limits and loss of office, a co-Duumvir relationship, and the property-census gate tying the cursus honorum to Economy & Finance's Net Worth. A later cleanup retires this document's own "Notable" terminology in favor of the Characters system's universal schema (§3) — the mechanics are unchanged throughout; only the record depth behind each name grew.*

---

## Contents

1. Scope & Role
2. Dignitas & Reputation Duality
3. Characters — The Political Cast
4. Clientela — Patrons & Clients
5. Local Magistracies — The Achievable Ladder
6. The Cursus Honorum — A Distant, Rare Goal
7. Provincial Administration
8. Sumptuary Laws
9. Scheming & Interpersonal Politics
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc named this system's shape directly: "local/regional politics as the primary day-to-day pillar... with cursus honorum/Senate seats as a distant, rare-but-reachable goal." Nearly every other document has been pointing at this one without designing it — the Curia "unlocks holding and contesting local magistracies," the Mint and Praetorium are gated behind political milestones, Economy & Finance's inbound Tax Policy and Publicanus contract both wait on "whatever magistracy Politics & Patronage eventually names," and Companions & Court Positions §5.5 drew an explicit line: private household appointments (a Procurator, a Rationalis) are *not* public office, and actual magistracies remain "entirely Politics & Patronage's domain." This document is where that domain finally gets built.

Per the core doc's own structural note, **Reputation Duality (§6.21) folds into this document** rather than standing alone — it's designed here, in §2, as this system's frontier-specific extension rather than a separate pass.

**What this document leans into, per direction:** patron-client relationships get real mechanical teeth (a genuine Clientela roster, not just a relationship-web tag); local office-holding is achievable and mostly light-touch, with a couple of offices carrying an occasional real duty; the cursus honorum stays the distant, mostly-abstracted goal the core doc always intended; and interpersonal politics — scheming, poaching a rival's client, undermining a competitor — gets a real hook now, sized to what's actually buildable before Rival Houses (§6.10) exists, but built to scale into that system directly rather than needing to be re-architected later.

---

## 2. Dignitas & Reputation Duality

**Dignitas** has been the project's standing reputation stat since the core doc — this document is its actual mechanical home. Dignitas is a single tracked number per household, moved by nearly everything: Villa Grandeur, Monuments, Funded Actions, marriage alliances, military victories, a won magistracy, a scandal, a defaulted debt. It's read constantly by other systems (Familia's marriage market, Rival Houses' comparison, Legal & Court's ruling weight) without this document needing to duplicate those read sites — Dignitas is a shared resource other documents already reference; this one just formally owns its accumulation and decay.

### 2.1 Reputation Duality (Frontier Play)

In frontier or newly-annexed settings (the Gallic province start, principally), Dignitas splits into two independently-tracked axes rather than one number:

- **Standing with Rome** — the traditional Dignitas, moved by the things above, plus Tributum payment history (Economy & Finance §5.1) and formal office-holding.
- **Local standing** — favor with the surrounding non-Roman populace, moved by Diplomacy with Non-Roman Peoples (§6.25, future), fair treatment of Peregrine-status residents, and local Contentment (Settlement Demographics).

The two axes can genuinely diverge — a household beloved by Rome and resented locally (or the reverse) is a real, playable position, not a temporary imbalance that resolves itself. A player deep in the Reputation Duality can face a real tension this document surfaces directly: a magistracy win that pleases Rome can simultaneously cost local standing, and vice versa.

---

## 3. Characters — The Political Cast

*(Originally written as "Notables" — a deliberately lightweight tier. The Characters system design (§6.1-adjacent) has since retired that distinction: every named individual, including everyone below, now gets that document's full schema rather than the lighter subset originally specified here. Nothing about the mechanics in this section changes — sourcing, elections, Faction, Clientela all work exactly as designed; only the depth of the record behind each name grew. Section retained under its original numbering for cross-reference stability.)*

A structural gap worth naming before Clientela or elections can work: Familia tracks full stat-blocked household members: Settlement Demographics tracks aggregate background pop groups. Neither tier fits a named client, a rival candidate for the same magistracy, or a fellow Decurion on the Curia — people the player needs to recognize, remember, and interact with individually.

A **Character** (per the Characters system's universal schema) is exactly that: a named individual with Core Attributes, Condition Stats, Legal Status/Social Class, a full Personality Trait set, the hidden Personality Axes, and a position in the relationship web — generated in full the instant they're needed (Characters §11's lazy instantiation), not a stripped-down placeholder.

These political-cast Characters are drawn from three sources:

- **Promoted Curiales.** Settlement Demographics' rarest, highest-tier pop group is explicitly the class Roman municipal government drew its councilors from — this document is where that connection pays off. A player can elevate a Curiales-tier individual out of the aggregate pop group into a tracked Character, using Familia §7's promotion rule directly.
- **Existing Patron/Client bond holders.** A freedman or client already tagged in Familia's relationship web (§2.7) who hasn't been promoted to a full Familia record is a Character by default the moment that tag exists.
- **Rival candidates.** Until Rival Houses (§6.10) exists, a contested election (§5.5) or political scheme (§9) surfaces its opposing figure as a Character — a real, named, rememberable rival with actual attributes, traits, and opinions, not a flavor-text placeholder, and built to convert directly into a Rival House-affiliated character once that system exists rather than needing replacement.

A political-cast Character can, of course, later be promoted into full Familia proper (a client the player marries into the family, a rival who's captured or recruited) — though per the Characters system, that's now a distinction of *role* (household member vs. not) rather than of *record depth*, since both already carry the same schema.

### 3.1 Faction — Traditionalist or Popularist

Every Character relevant to local politics, and the player's own household, carries a light **Faction** leaning — **Traditionalist** (conservative, ancestral-custom-favoring, roughly the historical *Optimates* tendency) or **Popularist** (reform- and common-favor-leaning, the *Populares* tendency). This isn't a hard allegiance system, just a single tag that makes several already-designed moments read consistently instead of each inventing its own audience: it's what actually determines who reacts well to a Sumptuary Edict (§8), who a scheme (§9) lands easier against, and which fellow Decurions (§5.6) a player naturally has friction or common cause with. A Character's Faction is fixed at generation; the player's own household Faction is a slow-moving reflection of accumulated choices (enforcing Sumptuary Edicts pulls Traditionalist, funding popular games pulls Popularist) rather than a one-time pick.

---

## 4. Clientela — Patrons & Clients

Per the decision to build something with real teeth rather than only the existing relationship-web tag: Clientela is a genuine, trackable system layered on top of Familia's Patron/Client bond.

### 4.1 Building a Roster

The player's **Clientela** is a roster of Characters (§3) and, less commonly, full Familia members (a freedman client especially) bound in the Patron/Client relationship. Growing the roster happens through Travel encounters, Events, direct recruitment of a promoted Curiales Character, or a former debtor bonded not into slavery but into clientage (a real, softer historical alternative to Economy & Finance §6.4's harshest debt-bondage outcome, worth flagging as an additional resolution option there).

### 4.2 Client Specialties & Favors

Each client carries a **Specialty** — Legal, Mercantile, Martial, Religious, or Administrative — loosely following whichever Core Attribute they're strongest in, and determines what favor they can actually perform when called on:

| Specialty | Typical Favor |
|---|---|
| Legal | Testimony or representation in a Legal & Court dispute (§6.16, future) |
| Mercantile | A trade tip, a Contract lead, or help moving goods around a Piracy-disrupted route |
| Martial | A retinue addition for Travel, or informal muscle in a dispute |
| Religious | Favor with a specific cult or festival, feeding Religion (§6.6, future) |
| Administrative | Support at the Curia — votes, procedural help, a friendly ear in a magistracy dispute |

A favor drawn on too often without reciprocation costs the relationship-web opinion between patron and client — Clientela is reciprocal, not a free resource tap, consistent with how every other relationship-driven system in this project already works.

### 4.3 The Salutatio

The real, attested morning ritual — clients calling on their patron for greetings, favors, and small business — becomes a recurring monthly event rather than a one-off flavor beat. The Atrium (Villa doc) is its physical home. A well-attended Salutatio (a large, high-opinion Clientela roster) generates a small Dignitas and Influence (§4.4) trickle just from being seen to hold court; a neglected one — too few clients, or a roster with poor average opinion — costs Dignitas instead. This gives the Atrium's existing "hosts a Politics & Patronage patron-client dinner" cross-reference (Villa doc §4.5) its actual mechanical payoff.

### 4.4 Influence

An aggregate resource, distinct from Dignitas and from denarii, generated by Clientela roster size and quality (client count, average opinion, Specialty diversity) plus held office (§5–6). Influence is spent, not merely accumulated — the currency behind requesting a favor at scale, backing a contested election (§5.5), or attempting a scheme (§9). This is the mechanical answer to "how do political actions actually cost something" without overloading Dignitas (a reputation score) or denarii (a household's wealth) to do a resource's job neither is well-suited for. Like Loyalty and every other opinion-driven figure in this project, Influence isn't a bank that sits still — a stockpile left unspent slowly decays, consistent with the real shape of political capital: it needs regular use (a Salutatio held, a favor called in, an election actually contested) to stay worth anything.

### 4.5 Loyalty, Ambition & Poaching

A client's Loyalty and Ambition (the same Familia Core Condition stats, per §3) are live, not static. A high-Ambition client whose favors keep going unrewarded, or who watches a rival house's patron offer better terms, is a real poaching risk — the client relationship-web bond can flip from the player's Patron/Client tag to a rival's. This is deliberately the sharpest edge Clientela has, and the clearest hook into Rival Houses once that system exists: for now, a poaching event resolves against whatever Character is available (§3), with the mechanic already built to point at a real Rival House record the moment that system is designed.

---

## 5. Local Magistracies — The Achievable Ladder

The Curia (Buildings §4.10) is the building that "unlocks holding and contesting local magistracies" — this section is what that unlock actually leads to. Per the decision to keep this ladder mostly light-touch with occasional real duties, four real, historically-grounded municipal offices, achievable within a single playthrough rather than the cursus honorum's distant-goal pacing. **Every office below is scoped to a single settlement's own Curia** — a player running a second settlement through a Procurator (Companions & Court Positions §5.3) holds no automatic claim on that settlement's magistracies; a second Curia seat there is a genuinely separate contest, won or lost independently of the first.

### 5.1 Decurion

The base entry point — a seated seat on the Curia itself. Requires the building, a Dignitas/Core Attribute threshold, and (per Familia §2.5) Roman Citizenship or Latin Rights at minimum; Peregrine and Freedman statuses are excluded from formal office per that document's own legal-status distinctions. Decurion carries no active duty beyond the seat itself — a modest, passive Dignitas trickle and the gate that makes every office below available to contest.

### 5.2 Aedile

Public works and games funding — the office historically closest to Economy & Finance's new Funded Actions category (§4.3 of that doc) and the natural forward hook into Games & Spectacle (§6.22, future). This is the ladder's "occasional real duty" office: periodically, holding the Aedileship prompts the player to actually fund a specific game or public work, with a real choice (fund it generously, fund it minimally, or let the moment pass) and a real consequence (a Dignitas/Contentment boost if funded well, a modest Dignitas cost if skipped) rather than resolving as pure passive income the way Decurion does. **Worth distinguishing from a title that sounds adjacent:** Companions & Court Positions' Editor and Editor Maximus (§4.2, §5.2 of that doc) are private household appointments who handle a venue's actual staging and logistics once a game is happening — the Aedile is the political office deciding *whether and how generously* to fund one in the first place. A single household can hold both at once, one civic and one domestic, without overlap.

### 5.3 Quaestor (Local)

Financial oversight — and the concrete office Economy & Finance §5.2 was waiting on: **holding the local Quaestorship is what actually satisfies that document's "requiresOffice" gate**, unlocking the player's own Tax Policy (Vectigalia/Decuma rates) on their own settlement. This resolves that open placeholder directly rather than leaving it pointing at an undesigned system indefinitely.

### 5.4 Duumvir

The paired chief-magistrate office — historically the top of a municipality's own ladder, held by two colleagues rather than one (a real, attested check-and-balance structure worth keeping rather than simplifying to a single top office). Duumvir is the office that satisfies the Mint/Moneta's "political milestone" gate (Buildings §4.10) and carries the ladder's largest passive Dignitas bonus. Consistent with the mostly-light-touch decision, Duumvir carries no forced recurring duty of its own — its weight is in what it unlocks (the Mint, the ladder's ceiling, eligibility to be noticed for §6's distant track) rather than an ongoing minigame. The co-Duumvir is always a real Character (§3), not an abstraction — a genuine relationship-web entry the player inherits the moment they win the office, and a natural source of either a useful ally or a standing rival for the length of the term, depending how that relationship is actually played.

### 5.5 Contested Elections

Per the decision to keep this abstracted for now while building toward the real thing: an election for any office above Decurion pits the player against an opposing Character (§3) rather than a placeholder threshold alone. Resolution weighs the player's relevant Core Attribute, Dignitas, and spent Influence (§4.4) against the same figures for the rival Character — a real contest with a real, named opponent, but resolved as a weighted comparison rather than a deep campaign minigame. Shared Faction (§3.1) with key Curia members (§5.6) is a soft thumb on the scale in either direction. This is deliberately built to need only the swap of "generated Character" for "an actual Rival House candidate" once that system exists — the resolution math doesn't change, only where the opposing figure's numbers come from.

### 5.6 The Curia as a Body

Decurion (§5.1) has been framed as a single seat so far — worth extending slightly, since a council of one undersells the "day-to-day pillar" the core doc calls for. The Curia holds a fixed-per-settlement-size number of seats, most filled by generated Characters rather than the player alone. These fellow Decurions aren't scenery: each carries their own Faction (§3.1), their own opinion of the player, and collectively can be courted, factionalized, or leaned on ahead of a vote — light Curia business (approving a Sumptuary Edict, endorsing a Funded Action, seating a new Decurion) resolves as a simple majority read off the assembled seats' opinions and Factions rather than a deep legislative minigame, keeping this consistent with the ladder's overall light-touch treatment while still making the council feel like a real body of people rather than a menu.

### 5.7 Term Limits & Loss of Office

Consistent with real Roman practice, every local office (§5.1–5.4) runs on an **annual term** rather than a permanent appointment — re-election at each term's end is its own contested election (§5.5) if challenged, or a simple renewal if not. An office can also be lost mid-term, not just fail to renew: Economy & Finance's Insolvency state (§9 of that doc) can strip a held magistracy directly, and a Legal & Court conviction (§6.16, future) is a second, sharper route to the same outcome — both deliberately more severe than simply losing a re-election, since they carry a real Dignitas penalty on top of the office itself.

---

## 6. The Cursus Honorum — A Distant, Rare Goal

Kept exactly as abstracted as the core doc always intended — the traditional Senate-track sequence (Quaestor → Aedile/Tribune → Praetor → Consul, at Rome rather than the municipality) is a single long-horizon **milestone track**, not a parallel duties system to §5's local ladder. Advancing it requires being *noticed* by Rome in the first place — a sufficiently high Dignitas (the Rome-standing axis specifically, where Reputation Duality applies), a sponsor (often an existing Clientela relationship in the other direction, the player as someone else's client for once), and a rare, high-stakes Event rather than a routine action.

**A second, harder gate: the property census.** Historically, Senate entry carried a literal minimum wealth requirement, not just standing — this document reuses Economy & Finance's Net Worth figure (§8 of that doc) directly as that gate rather than inventing a parallel wealth check. A household with strong Dignitas but insufficient Net Worth is a real, historically-grounded story (an old, respected name that's quietly gone poor) and stays locked out of the cursus honorum until both bars clear — just as a wealthy-but-undistinguished household (strong Net Worth, weak Dignitas) is the mirror case. A player who clears both gates from a modest Social Class starting point (Familia §2.5) is deliberately playing out the *novus homo* ("new man") story — Rome's own celebrated rags-to-riches arc, worth flagging as a natural Dynasty Chronicle highlight in its own right rather than treating class origin as a hard ceiling.

Each rung is a genuine milestone: a real Dynasty Chronicle (§6.11, future) entry, a substantial Dignitas jump, and narrative weight matching how rare it actually is — a household that reaches the Consulship has done something a hundred other gentes across a full campaign never will. This is deliberately the opposite design shape from §5's achievable ladder: local magistracies are the day-to-day pillar every playthrough can engage with; the cursus honorum is the distant peak a small fraction of playthroughs will actually summit.

---

## 7. Provincial Administration

A lateral branch off the cursus honorum rather than a further rung on it: a provincial governorship (proconsul/propraetor-style) is reachable through the same "noticed by Rome" gate §6 describes, without necessarily requiring the full Consular sequence first — historically, provincial commands were often held by ex-magistrates rather than exclusively by the very top of the ladder. Holding one:

- Satisfies the Praetorium's (Buildings §4.10) own "holding a provincial office" gate directly.
- Unlocks Economy & Finance's flagged-but-undesigned **Publicanus** contract (§5.2 of that doc) — a provincial governor is exactly the figure positioned to offer that tax-farming arrangement, resolving that placeholder now that the office exists to grant it.
- Extends the Reputation Duality axis (§2.1) to the whole administered province rather than just the player's own settlement — a real widening of what's actually at stake in that split.

---

## 8. Sumptuary Laws

Flagged directly by Resources & Goods (Pearl's "strong future hook for a specific Politics & Patronage or Legal & Court sumptuary-law event") — this section closes that hook. A **Sumptuary Edict** is a standing policy, in the same family as Economy & Finance's Tax Policy, restricting which Legal Status/Social Class tiers may display specific luxury goods (Pearl, Tyrian Purple togas, certain jewelry) publicly. Available once the player holds any office at Decurion or above (§5.1), reflecting that this was historically a magistrate's power to invoke, not a private household choice:

- Enforcing one is a real, if unusual, Dignitas lever — a magistrate seen enforcing traditional restraint gains standing with a Traditionalist-leaning audience (§3.1), at the cost of resentment from the wealthier Negotiatores/Curiales tier it actually restricts, most sharply among Popularist-leaning Characters (Settlement Demographics' Contentment machinery, the same shape Tax Policy already uses).
- It's also a soft counter-lever against a rival house's own conspicuous wealth display — a Sumptuary Edict timed against a rival's own showy Dignitas play is a legitimate, historically-grounded political weapon, worth keeping in mind once Rival Houses (§6.10) exists to be the target of one.

---

## 9. Scheming & Interpersonal Politics

The CK3-style layer the direction explicitly called for: political actors — a rival Character, a fellow Decurion, a client weighing a better offer elsewhere — are people the player can act *against* and *through*, not just compete with via abstracted thresholds.

- **Undermine a rival candidate.** Ahead of a contested election (§5.5), the player can spend Influence and/or draw on a Legal- or Intrigue-Specialty client's favor (§4.2) to depress a rival Character's standing directly — a real, if currently lightweight, scheme action. This, and every scheme action below, now runs on the Characters system's own Scheme engine (that document's §10) rather than a bespoke resolution — progress, discovery risk, and real counter-play, not a single roll.
- **Poach a rival's client.** The mirror image of §4.5's own poaching risk — courting a rival's discontented client with better terms, resolved against that client's own Loyalty/Ambition.
- **Leverage from other systems.** Romance & Seduction (§6.19, core doc) already names itself as feeding Politics directly via seduction-for-leverage; Espionage (§6.15, future) is named as generating "blackmail material" for exactly this kind of use. Neither is designed by this document, but both have a clear, already-established door into it — and, per the Characters system, both now share this document's same underlying Scheme engine rather than needing their own.

All of this is deliberately sized to what's buildable now — against Characters, using Influence and existing relationship-web mechanics — while sharing its resolution shape directly with what Rival Houses will eventually need, so that system's own pass extends this one rather than replacing it.

---

## 10. Cross-System Integration

- **Buildings (Production Chains) doc:** the Curia (§5's whole ladder), the Mint/Moneta (§5.4), and the Praetorium (§7) all get the political milestones they were built waiting on.
- **Economy & Finance:** the local Quaestorship (§5.3) is the concrete office satisfying that document's Tax Policy gate; a provincial governorship (§7) resolves its flagged Publicanus contract; Insolvency (that doc's §9) is now a direct, named trigger for losing a held magistracy (§5.7), not just a described mirror; Net Worth (§8 of that doc) is this document's second cursus honorum gate alongside Dignitas (§6).
- **Characters:** §3's political cast, Clientela favors, and §9's Scheming all run on that document's universal schema, Personality Axes, Interaction Catalog, and Scheme engine — this document's own lighter treatment of "Notable" generation and ad-hoc scheme resolution should be read as superseded by that pass rather than as a competing design.
- **Companions & Court Positions:** §5.5's explicit line (private appointment vs. public office) is honored directly — nothing in this document lets a Procurator or Rationalis become a magistrate by another name; a Character must contest an actual office per §5; the Aedile/Editor distinction (§5.2) keeps that document's Editor and Editor Maximus cleanly separate from this document's civic office; office-holding is explicitly per-settlement (§5), consistent with the Procurator running a second settlement without inheriting the first one's political standing.
- **Familia:** the marriage market's alliance-value figure is a natural, if not yet formally wired, input to Clientela recruitment.
- **Settlement Demographics:** the Curiales pop group is this document's Character-recruitment source (§3) and the population whose Contentment a Sumptuary Edict or Aedile's games funding actually moves.
- **Villa (interior design doc):** the Atrium is the Salutatio's (§4.3) physical home, giving that room's existing cross-reference a real mechanic.
- **Reputation Duality (§6.21):** folded fully into §2.1 of this document per the core doc's own structural note, rather than existing as a separate pass.
- **Diplomacy with Non-Roman Peoples (§6.25, future):** local standing (§2.1) is this document's shared axis with whatever that system eventually builds.
- **Legal & Court (§6.16, future):** a Legal-Specialty client's courtroom favor (§4.2) and a Sumptuary Edict's enforcement (§8) are both concrete hooks into that system's eventual caseload.
- **Romance & Seduction (§6.19) / Espionage (§6.15, future):** both are named as feeding this document's scheming layer (§9) directly, per the core doc's own cross-references.
- **Rival Houses (§6.10, future):** Characters (§3), the poaching mechanic (§4.5), contested elections (§5.5), and scheming (§9) are all deliberately built to extend directly into that system rather than needing replacement once it exists.
- **Dynasty Chronicle (§6.11, future):** every cursus honorum rung (§6) and a won or lost contested election are exactly the milestone-catalog material that system is meant to record.
- **Games & Spectacle (§6.22, future) / Religion (§6.6, future):** the Aedileship's funding duty (§5.2) is this document's forward hook into both.

---

## 11. Data Model

```
Dignitas {
  settlementId,
  standingWithRome,       // the traditional single Dignitas figure outside Reputation Duality settings
  localStanding,          // §2.1 — only tracked separately in frontier/newly-annexed settings
  householdFaction,       // "traditionalist" | "popularist" — §3.1, slow-moving, driven by accumulated choices
}

CuriaBody {          // §5.6
  settlementId,
  seats: [
    { holderId,           // playerCharacterId or notableId
      isPlayerOrFamilia: bool,
      faction,
      opinionOfPlayer }
  ],
}

// Notable{} retired — every political-cast individual is now a Character per that system's own
// schema (traits, Personality Axes, appearance, the full model), not a separate lighter struct here.
// faction (below) is the one field this document adds on top of the base Character record.

ClientelaRoster {
  patronId,               // the player's household, or in principle any Character/Rival House
  clients: [
    {
      clientId,           // a Character id (per the Characters system) — Familia members are Characters too
      specialty,           // "legal" | "mercantile" | "martial" | "religious" | "administrative"
      lastFavorMonth,
      opinion,
    }
  ],
  influence,               // §4.4 — spent, not just accumulated
}

MagistracyRecord {
  personId,               // the player character, or any Character/rival
  office,                  // "decurion" | "aedile" | "quaestorLocal" | "duumvir" |
                           // "quaestorRoman" | "aedileRoman" | "praetor" | "consul" | "provincialGovernor"
  settlementOrProvinceId,
  termStartMonth,
  termLengthMonths,        // §5.7 — annual for local offices
  isCursusHonorum: bool,    // false for §5's local ladder, true for §6's distant track
  lostEarlyReason,         // null | "insolvency" | "legalConviction" — §5.7
}

Election {
  electionId,
  office,
  incumbentId,
  challengerCharacterId,    // §5.5 — swaps for a real Rival House id once that system exists
  resolutionInputs: { attributeScore, dignitas, influenceSpent },
  outcome,
}

SumptuaryEdict {
  settlementId,
  restrictedGoods: [ "pearl", "tyrianPurpleToga", ... ],
  restrictedToClassesAbove,   // e.g. "curiales" — who's still permitted to display
  active: bool,
}

Scheme {           // §9
  schemeId,
  initiatorId,
  targetCharacterId,
  type,             // "underminesCandidate" | "poachClient" | "leverageFromRomance" | "leverageFromEspionage"
  influenceSpent,
  outcome,
}
```

---

## 12. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Dignitas thresholds per office, Influence generation/cost/decay rates, election resolution weighting, Curia seat counts, and the Net Worth census threshold (§6) are all unsized.
- **Character-to-Rival-House conversion.** §3, §4.5, and §5.5 all deliberately build against a generated Character now, flagged to swap in a real Rival House record later — the actual conversion/migration logic isn't designed, since it depends on that system's own eventual data model. (The Characters system carries this same open question independently — resolving it there resolves it here too.)
- **Sponsor requirement for the cursus honorum.** §6 gestures at "often an existing Clientela relationship in the other direction" as a plausible way the player gets noticed by Rome, but doesn't formalize what qualifies a sponsor or how that reverse-patronage relationship is actually established.
- **Aedile duty frequency.** §5.2 establishes the office's funding prompt as a real, occasional duty; how often it actually triggers isn't specified.
- **Sumptuary Edict enforcement mechanism.** §8 establishes the lever and its Dignitas/Contentment tradeoff, but not how (or whether) actual violations are detected versus the edict just applying a passive modifier.
- **Whether female or non-citizen household members can hold any political role here.** Familia §2.5 already restricts formal office to citizens (male, per that document's own historical-restriction toggle); whether a Character or client role in the Clientela system carries the same restriction, or is more permissive since it's patronage rather than office, isn't explicitly decided.
- **Provincial governorship duration/rotation.** §7 doesn't specify whether a held governorship is permanent, term-limited, or subject to being reassigned/recalled by Rome — a real historical wrinkle left for a future pass.
- **Faction's actual gameplay pull.** §3.1 establishes Traditionalist/Popularist as a real tag read by Sumptuary Laws, Scheming, and the Curia body, but not how strongly it should weight any single roll relative to the other inputs already in play.
- **Curia seat count formula.** §5.6 ties seat count to "settlement size" without specifying the actual scaling curve.
- **What happens to a lost local office's seat.** §5.7 covers the player losing an office; whether a vacated seat immediately opens to a new contested election, sits empty for a term, or is filled by Curia appointment isn't specified.
