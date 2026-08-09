# GENS — System Design: Correspondence & Letters (§6.27)
*The remote, deliberately lower-stakes counterpart to Travel — now a genuine two-way system rather than purely outbound: an Inbox where patrons, rivals, and distant family can write to the player requiring a real response choice, three further correspondence actions (news, written instructions to a distant appointee, condolence/congratulation), real period-accurate letter conventions as diegetic flavor, and — the addition I think matters most — a real, historically honest limit: several Frontier cultures' own genuine oral-tradition resistance to writing (Caesar himself records the druids specifically refusing to commit their teachings to writing) means Correspondence simply doesn't work well, or at all, with some peoples, forcing Travel as the only real diplomatic channel.*

---

## Contents

1. Scope & Role
2. Literacy — Who Can Actually Correspond
3. Sending a Letter — Distance, Transit Time & Real Delay
4. The Diegetic Letter — Real Roman Form
5. Correspondence Actions — What a Letter Can Actually Do
6. Receiving Correspondence — The Inbox
7. The Oral Tradition Problem — Where Writing Doesn't Reach
8. Courier Choice — Tabellarius, Hired Carrier, or Pigeon
9. Interception, Forgery, Redirection & the Message's Own Risk Profile
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

Unchanged from the first pass: the remote, lower-ceiling, real-delay, differently-risked counterpart to Travel, living entirely on the far side of the boundary that document's own §8 already drew. Three real distinctions from Travel remain the core of this document's own identity — lower ceiling, real delay, a risk aimed at the message rather than the traveler — joined this pass by a fourth: **not every recipient can actually be reached this way at all** (§7).

---

## 2. Literacy — Who Can Actually Correspond *(unchanged)*

Well-Read/Illiterate remains this system's foundational gate; an Illiterate paterfamilias has no real privacy from whoever reads his mail for him.

---

## 3. Sending a Letter — Distance, Transit Time & Real Delay *(unchanged)*

Reuses Travel's own distance model; real negotiation by letter typically requires several full round trips, each carrying genuine transit time both ways.

---

## 4. The Diegetic Letter — Real Roman Form

New this pass, and a direct, natural extension of this project's own Visual Identity philosophy ("the game's UI and its own diegetic record-keeping are the same object"): a letter the player sends or receives should read as an actual period letter, not a menu describing one. Real Roman correspondence followed a real, consistent opening convention — sender and recipient named, followed by a standard greeting formula ("*Marcus Aemilius Lucio suo salutem dicit*" — "Marcus Aemilius to his own Lucius, greetings") — and this document adopts that real form directly for how every letter is actually presented on-screen, the same wax-tablet-and-inscription visual language the rest of the project's interface already commits to. This is presentation only, not a new mechanic — but it's the difference between a letter feeling like a real object the household produced and a dialogue box that happens to be about mail.

---

## 5. Correspondence Actions — What a Letter Can Actually Do

The original six actions, plus three real additions this pass:

- **Petition a Patron, Maintain a Distant Relationship, Remote Negotiation, Direct an Already-Placed Spy, a Formal Complaint or Provocation, Early Courtship** — unchanged from the first pass.
- **News & Gossip** *(new)* — a real, low-mechanical-stakes action that's mostly informational: a distant Character's own life update (a married daughter's pregnancy, a son's campaign news) arriving as genuine, readable content rather than a silent background tick. The natural, named channel Events' own cross-reference already anticipated ("the natural channel for a distant Imperial or Rival House development to reach the player when they're not physically present").
- **Written Instructions to a Distant Appointee** *(new)* — a real, direct tie to a Procurator managing a Second Settlement (Companions & Court Positions §5.3) or any Overseer currently away on their own errand: updating standing instructions remotely rather than waiting for a Travel visit to intervene in person. A natural, concrete forward hook for Steward/Council Auto-Management's own eventual automation layer.
- **Condolence or Congratulation** *(new)* — a real, historically ubiquitous genre of actual Roman correspondence: a modest, low-cost relationship and Dignitas gesture in direct response to a distant Character's own real life event (a birth, a death, a marriage) recorded in Familia or Dynasty Chronicle. Small, genuine, and exactly the kind of unglamorous social maintenance real correspondence spent most of its actual volume on.

---

## 6. Receiving Correspondence — The Inbox

The single biggest structural addition this pass: the first draft was entirely outbound, and a real correspondence system has to run both directions. Other Living World Actors — a patron, a rival house, a distant client, a foreign people under an active treaty — can send a letter **to** the player, arriving as a real, readable piece of correspondence requiring an actual response choice rather than a passive notification.

**A real, felt consequence for every response, including no response at all:** answering a patron's own written request promptly and generously reads as real, felt Clientela investment; a slow or dismissive reply costs real standing. Ignoring a rival's own written provocation entirely is a genuine, legitimate choice — it can read as calm de-escalation, or as weakness inviting further pressure, depending on that rival's own Faction and Standing, exactly the kind of ambiguous, real social judgment call this project's other systems already ask the player to make rather than offering an obviously correct button.

---

## 7. The Oral Tradition Problem — Where Writing Doesn't Reach

New this pass, and a real, historically significant limit worth building in directly rather than assuming every culture on Cultures of the Known World's own thirty-six-entry roster is equally reachable by letter. Julius Caesar's own real, surviving account of the druids is specific and direct: they deliberately declined to commit their own teachings to writing, preserving them orally on purpose, even while using writing for other, more mundane matters. This document treats that real historical detail as a genuine mechanical constraint rather than flavor text to ignore:

- **Gallic, British, and Germanic culture** (Cultures §3) carry a real, meaningfully reduced Correspondence effectiveness for anything touching their own religious or druidic-adjacent leadership specifically — an ordinary trade letter to a Gallic merchant works fine, but a substantive treaty negotiation routed through a druidic-influenced leadership structure genuinely doesn't translate to paper the way it would with a literate Hellenic or Parthian counterpart.
- **Genuinely non-literate Frontier peoples more broadly** (per Cultures' own Thin-Record tag — Numidian, Sarmatian, Illyrian/Pannonian) carry the same real constraint by extension, for the more honest reason that this document simply has no basis to assume otherwise.
- **This is not a hard, universal wall** — Diplomacy with Non-Roman Peoples' own Interpreter Problem-equivalent (a culturally-familiar Character, or a real intermediary who can render the message orally through an interpreter before it's ever written down) can partially close the gap — but a genuinely substantive negotiation with these specific peoples remains meaningfully worse by letter than with a literate civilization, and at the extreme, some content simply cannot be transmitted this way at all, making Travel the only real available channel.

**Genuinely literate civilizations** — Parthia, Egypt, the Hellenic world, and any other State-Sponsored, Great Power, or Trade-Contact-Only culture on Cultures' own roster — carry no such penalty; this constraint is specific and honest, not a blanket "foreigners can't read" assumption this document has no interest in making.

---

## 8. Courier Choice — Tabellarius, Hired Carrier, or Pigeon *(unchanged)*

---

## 9. Interception, Forgery, Redirection & the Message's Own Risk Profile

Unchanged core risk shape (interception, forgery), plus one further real, honest complication: **Redirection**. Because Travel's own concurrent-location tracking means any Character can genuinely be somewhere different by the time a letter arrives than they were when it was sent, a letter can arrive to find its recipient already gone — triggering a real, additional delay while it's forwarded on, or, at worst, never quite catching up before circumstances move past whatever it was responding to. A real, small, honest friction point rather than assuming perfect knowledge of a moving target's own whereabouts.

---

## 10. Cross-System Integration

Unchanged prior integrations (Traits, Companions & Court Positions, Villa, Travel, Characters, Politics & Patronage, Espionage, Diplomacy, Romance & Seduction, Rival Houses, Piracy & Banditry, Dynasty Chronicle), plus this pass's own: **Events** gains News & Gossip as the concrete, named realization of its own previously-flagged cross-reference; **Companions & Court Positions'** Procurator gains a direct remote-management channel; **Cultures of the Known World** gains a real, honest mechanical consequence for its own oral-tradition-flagged and Thin-Record cultures; **Steward/Council Auto-Management (§6.28, future)** gains Written Instructions as a natural object its own eventual automation would extend.

---

## 11. Data Model

```
Letter {
  letterId, senderCharacterOrActorId, recipientCharacterOrActorId,
  draftedByCharacterId,
  direction,                       // new — "outbound" | "inbound"
  action,                            // now includes "newsAndGossip" | "writtenInstructions" | "condolenceOrCongratulation"
  sentMonth, transitTimeMonths, arrivalMonth,
  courierType, courierCharacterId,
  interceptionRisk,
  intercepted: bool, forged: bool, redirected: bool,     // redirected new — §9
  oralTraditionPenaltyApplied: bool,                       // new — §7
  requiresResponse: bool, responded: bool, responseAction,   // new — §6, only meaningful for inbound
  outcome,
}
```

---

## 12. Open Questions

- **All numeric sizing carried forward, plus new unsized figures:** the Oral Tradition Problem's exact effectiveness reduction, Redirection's added delay, and Inbox response-timing thresholds.
- **The Oral Tradition Problem's exact culture list completeness.** §7 names Gallic/British/Germanic specifically and extends by inference to Thin-Record cultures; whether every one of Cultures' own thirty-six entries needs an explicit yes/no flag, or whether the current honest default (literate unless specifically flagged otherwise) is sufficient, isn't fully resolved.
- **Inbox volume and pacing.** §6 doesn't specify how frequently other actors should realistically be initiating correspondence, to avoid either an empty, lifeless Inbox or an overwhelming one.
- **Forgery detection mechanics.** Still unresolved from the first pass.
