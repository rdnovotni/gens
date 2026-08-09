# GENS — System Design: Activities & the Activity Engine (§6.47, new)
*The real third tier this project has been missing between two existing shapes: Characters built the single-tick, aggregate Group Interaction (§9.8) for routine group moments, and the adversarial, multi-stage Scheme engine (§10) for covert action against a target. Neither fits what CK3 calls an Activity — a deliberately convened, named, guest-listed gathering with real internal structure, real stakes, and real drama that emerges specifically because a defined set of people are gathered together for a real reason. This document is that missing tier: a shared, reusable Activity Engine every future specific Activity Type — Feasts, Hunts, Weddings, and whatever else follows — will plug into, rather than each inventing its own gathering logic from scratch. Per direction, this pass builds the engine only; the specific Activity Types are deliberately left to their own future design passes.*

---

## Contents

1. Scope & Role — A Real Third Tier
2. Anatomy of an Activity
3. Duration — Quick and Extended Activities
4. The Guest List & Invitation — Who's There, and Who Isn't
5. Scale and Quality — Two Real, Independent Axes
6. Phases — What Actually Happens
7. The Witness Pool — Activities as a Real Discovery/Scandal Surface
8. NPC-Hosted Activities — A Living World That Gathers Without the Player
9. Resolution & Outcome
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role — A Real Third Tier

Three real resolution shapes now exist side by side, each suited to a genuinely different kind of social moment:

- **A Group Interaction** (Characters §9.8) is quick, routine, and aggregate — an ordinary Salutatio, a plain dinner, resolved as the sum of individual per-target reactions with no real internal structure.
- **A Scheme** (Characters §10) is covert, adversarial, and built around one initiator working against one target's own awareness.
- **An Activity** (this document) is neither: it's a real, deliberately convened, *named* gathering — a specific host, a specific guest list, a specific venue, and real internal Phases (§6) during which the ordinary Interaction Catalog and Scheme engine both operate at elevated stakes, precisely because a defined set of people are genuinely gathered together.

This document builds the shared skeleton only. Every future Activity Type — a Feast, a Hunt, a Wedding, a Funeral (Ancestor Veneration & Funerary Customs' own §2 funeral sequence is, in retrospect, an early, unlabeled instance of exactly this pattern), a Symposium — is simply a specific configuration of the six real slots §2 defines, not a parallel system.

---

## 2. Anatomy of an Activity

Every Activity, regardless of eventual type, is built from the same six real components:

1. **Host** — the convening household or Character; §8 covers a non-player Host.
2. **Type** — a pluggable slot. This document doesn't define any specific Type; each future Activity design fills this in.
3. **Venue** — a real, specific location: an existing Villa room (Triclinium, Oecus, Peristylium, Andron, Diaeta, Xenodochium), a settlement civic space (the Forum, the Circus, the Basilica), or an outdoor location for something like a Hunt. The Venue's own existing tier (Villa's Grandeur-contributing room tiers) feeds directly into Quality (§5).
4. **Guest List** — §4.
5. **Duration** — §3.
6. **Phases** — §6.

---

## 3. Duration — Quick and Extended Activities

Per direction, both real modes exist, and which one applies is a property of the specific Activity Type, not a fixed rule this document imposes uniformly:

- **Quick Activities** resolve entirely within a single month, with Phases running as sub-steps inside that same tick — the natural shape for an ordinary Feast or a Symposium.
- **Extended Activities** span multiple real months, their Phases spread across that real duration rather than compressed into one tick — the natural shape for a Wedding's own real lead-up-and-ceremony arc, or a genuine multi-day hunting expedition. An Extended Activity is a real, standing state for its duration, and can be genuinely interrupted by an unrelated event — a Natural Disaster, a Scheme reaching resolution, a Piracy raid — the same way any other ongoing state in this project already can be, giving a long Activity real, felt stakes beyond a single evening's outcome.

---

## 4. The Guest List & Invitation — Who's There, and Who Isn't

A Guest List is a real, named set of Characters — drawn from the host's own Familia, Clientela roster, Companions, or any Living World Actor (a Rival House's own Head, a foreign dignitary under an active Diplomacy treaty) the host chooses to invite.

### 4.1 Invitation and RSVP

Sending an Invitation is a real, meaningful act, not a formality: each invitee's own RSVP reads their existing opinion, Faction, and standing — a Traditionalist Decurion accepts a properly Roman gathering more readily than a heavily Hellenized one; a household mid-Feud with the host almost certainly declines, or, in the sharper case, accepts specifically to cause trouble once there.

### 4.2 Exclusion as a Real Signal

The deliberate CK3-style read this document exists to support: leaving a Character off the Guest List for an Activity they'd reasonably expect an invitation to — a prominent local Decurion snubbed from a major Feast, a family member excluded from a Wedding — is a real, direct, Insult-equivalent event in its own right, even though nothing was said or done. This reuses the existing opinion/relationship-web machinery rather than inventing a parallel snub mechanic; it simply names "who wasn't invited" as a real, felt input worth checking, not just "who attended."

---

## 5. Scale and Quality — Two Real, Independent Axes

Two separately tracked values, deliberately orthogonal rather than one blended score:

### 5.1 Scale

How big, how public, how visible — **Intimate / Modest / Grand / Lavish**, set by the host at planning time from Guest List size and Venue choice, not rolled. A five-guest Peristylium gathering is Intimate regardless of how much was spent per guest; a Circus-wide public spectacle is inherently Lavish.

### 5.2 Quality

How well the Activity is actually executed, reusing Food Culture's own three-input Banquet Quality formula as the direct template rather than reinventing it: an Activity's Quality reads whatever inputs its specific Type actually calls for (catering for a Feast, game stock and ground quality for a Hunt, officiant skill and ritual observance for a Wedding), resolving into the same four-tier **Modest / Respectable / Refined / Legendary** output Food Culture already established.

### 5.3 Why Keep Them Separate

A small, Intimate gathering can be Legendary in Quality — an intensely well-executed private dinner. A huge, Lavish public spectacle can just as easily land at Modest Quality despite the expense — a badly-run games day, a rained-out hunt. Consistent with Design Pillar #1, bigger is never simply better; Scale buys reach and stakes, Quality buys actual execution, and a host has to genuinely earn both rather than one substituting for the other.

---

## 6. Phases — What Actually Happens

The real connective tissue between an Activity's own container and everything this project already built. Every Activity resolves through one or more named **Phases** — a generic default sequence (Reception → Main Event → Aftermath) any future Activity Type can use as-is or override with its own.

### 6.1 Interactions Inside a Phase

During a Phase, the host and any attending Character can initiate ordinary Interactions (Characters §9.1–9.7) against any other attendee at elevated availability or effectiveness. Several existing Interactions are specifically more natural inside an Activity's own Phase than in the ordinary daily flow: a formal Duel challenge, a betrothal proposal, a Scheme's own Confront With Evidence or Reveal a Secret moment (§10 of that document), a Politics & Patronage favor request. This document doesn't grant any of these a new effect — it simply gives them a real, appropriate stage to happen on.

### 6.2 Events Inside a Phase

A Phase can also trigger a genuine, contextual Personal-scope Event (Events §2–3). The Weighted Event Pool gains a real, felt anchor point this way: an Event firing specifically "during the Reception phase of a hosted Feast" reads richer than the same content firing in a narrative vacuum, and several existing Personal-scope events — a Romance flirtation, a dispute, an Omen — are natural candidates to specifically prefer firing inside an appropriate Activity Phase rather than at any random moment.

---

## 7. The Witness Pool — Activities as a Real Discovery/Scandal Surface

A genuinely useful consolidation: several existing documents already lean on an informal "how many people are aware" input without ever naming a shared population to check it against — a Scheme's own discovery risk (Characters §10.3), Food Culture's Ostentatious-banquet publicity mechanic (§4.2 of that document), a Fronting arrangement's own exposure risk (Private Ships §3). This document names that shared population directly: **an Activity's own Guest List *is* its Witness Pool.**

A larger, more Lavish Activity has a larger Witness Pool, and that cuts both ways — real Dignitas upside from a well-executed, well-attended gathering, and real, elevated discovery risk for anything happening at or because of it: a Scheme confronted mid-Feast, a Duel fought in front of witnesses, a Forgery (Art & Art Commissions §10) or a Fronting arrangement (Private Ships §3) exposed at exactly the wrong social moment. This document doesn't rebuild any of those systems' own discovery formulas — it simply gives all of them the same real, shared population to check against whenever the triggering moment happens to land during a hosted Activity.

---

## 8. NPC-Hosted Activities — A Living World That Gathers Without the Player

Per direction, built into this core pass directly. Any Living World Actor with real standing — a Rival House, a Notable Household, a foreign dignitary — can independently convene their own Activity, entirely outside the player's own initiative, per the same "the player is never a protected special case" principle Piracy & Banditry (§7) already established.

### 8.1 Receiving an Invitation

The player can receive a real Invitation to a Rival House's own wedding, feast, or Symposium. Accepting is a real opportunity — Clientela-building, a Romance opening, direct social intelligence on a rival household's own composition and mood. Declining is a real, legible social choice of its own, reading exactly the way declining any other social overture already does elsewhere in this project.

### 8.2 Not Being Invited

The sharper, quieter case: the player can simply not appear on a rival's Guest List at all (§4.2's exclusion logic applied against the player rather than by them) — a real, felt signal about current standing, discoverable through Correspondence & Letters' own gossip channels (§5 of that document) even without being personally present to notice the snub directly.

---

## 9. Resolution & Outcome

At an Activity's conclusion — after one tick for a Quick Activity, or after its full real duration for an Extended one — the engine resolves one aggregate **Outcome**:

- **Quality's own base payoff** — read the same way Food Culture's Banquet Quality already feeds a Politics & Patronage dinner's own Dignitas and relationship effect.
- **Scale's Witness-Pool amplification** (§7) of whatever specific Interaction or Event outcomes actually occurred during its Phases.
- **A real, readable Activity Record** — a lightweight generated summary, mirroring Military & Combat's own Battle Report (§5.6 of that document) rather than a wall of individual per-guest logs: one legible readout of "how did the wedding actually go," highlighting real key moments (a proposal accepted, a Duel fought, a rival pointedly snubbed) rather than forcing the player to reconstruct the evening from a dozen separate interaction results.

---

## 10. Cross-System Integration

- **Characters:** the Interaction Catalog (§9.1–9.7) and the Scheme engine (§10) are both reused directly inside Phases (§6.1) rather than duplicated; Group Interaction (§9.8) remains the lighter, routine tier this document's own Activities sit above.
- **Events:** the Weighted Event Pool's own Personal-scope content (§2–3 of that document) gains a real, contextual anchor point inside a Phase (§6.2).
- **Food Culture:** the Banquet Quality formula (§2 of that document) is this document's own direct template for Quality (§5.2); the Ostentatious/publicity mechanic (§4) is a concrete precedent for the Witness Pool (§7).
- **Villa:** every named Venue (§2) is an existing room, requiring no new construction; a room's own tier feeds Quality directly.
- **Politics & Patronage:** the Salutatio and a patron-client dinner are both natural, retroactive instances of this document's own Activity shape.
- **Education & Culture:** the Symposium (§7.1 of that document) is a natural Quick Activity Type.
- **Ancestor Veneration & Funerary Customs:** the funeral sequence (§2 of that document) is, in retrospect, an early, unlabeled Extended Activity — future passes may wish to formally reframe it as one.
- **Romance, Sexuality & Lineage:** a courtship dinner or a formal betrothal proposal is a natural in-Phase Interaction (§6.1).
- **Games & Spectacle:** a hosted spectacle is a natural large-scale, inherently Lavish Activity Type.
- **Scandal:** the Witness Pool (§7) is this document's own direct, concrete contribution to that system's aftermath engine — a scandal's own "how many people saw this" input finally has a real, named source.
- **Rival Houses / Correspondence & Letters:** NPC-hosted Activities (§8) are a direct, concrete application of the living-world principle; §8.2's un-invited signal is discoverable through Correspondence's own gossip channel.
- **Companions & Court Positions:** the Xenodochus, Symposiarch, Archimagirus, and Editor are all natural, existing Phase-quality operators for whichever Activity Type eventually calls on them.
- **Wandering Populations:** a Hosted engagement (§6 of that document) is a light, single-Phase instance of this same underlying pattern.
- **Travel:** a Journey's own destination can plausibly include arriving to attend an Activity already underway or about to begin.

---

## 11. Data Model

```
Activity {
  activityId,
  type,                        // pluggable — defined by future Activity Type design passes
  hostId, hostIsNPC: bool,
  venueRef,
  durationMode,                 // "quick" | "extended" — §3
  startMonth, endMonth,
  guestList: [ characterId, ... ],
  scaleTier,                    // "intimate" | "modest" | "grand" | "lavish" — §5.1
  qualityTier,                  // "modest" | "respectable" | "refined" | "legendary" — §5.2
  qualityInputs: { ... },         // Type-specific, mirrors Food Culture's own BanquetRecord inputs
  phases: [ ActivityPhase, ... ],
  witnessPoolCharacterIds: [ ... ],  // derived directly from guestList — §7
  outcomeSummary,
}

ActivityInvitation {              // §4
  activityId, inviteeId,
  rsvpStatus,                    // "accepted" | "declined" | "notInvited"
  wasExpectedInvite: bool,          // §4.2 — true if the exclusion itself is meaningful
  exclusionInsultApplied: bool,
}

ActivityPhase {                   // §6
  phaseId, activityId, phaseType,
  monthOccurred,
  interactionsInitiated: [ interactionRef, ... ],
  eventRollResults: [ eventRef, ... ],
}

ActivityRecord {                   // §9
  activityId,
  narrativeSummary,
  keyMoments: [ ... ],
}
```

---

## 12. Open Questions

- **All numeric sizing**, per convention — Scale tier guest-count thresholds, Quality's own input weighting, and Witness-Pool discovery-risk scaling are all unsized.
- **Player control granularity.** This document doesn't specify whether the player actively directs every Phase of a Quick Activity in real time, or can set an overall intent (per Steward/Council Auto-Management's own existing "sensible defaults, consequential decisions held" principle, §6.28) and let routine Phases resolve automatically — left for either a future UX pass or the specific Activity Type docs to decide case by case.
- **NPC Phase depth.** §8 establishes that an NPC-hosted Activity is real and can generate real invitations and outcomes, but doesn't specify how much internal Phase detail is actually simulated for one the player isn't attending versus a lighter, summarized resolution.
- **Specific Activity Types.** Per direction, Feasts, Hunts, Weddings, Symposiums (as a formal Type rather than Education & Culture's own existing mechanic), and any further types are explicitly deferred to their own future design passes — this document supplies only the shared slots (§2) they'll each fill in.
- **Cross-Activity scheduling conflicts.** Whether two Activities (the player's own and a rival's) can be scheduled in real conflict, forcing a genuine choice of which to attend, isn't addressed here.
