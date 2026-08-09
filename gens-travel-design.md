# GENS — System Design: Travel (§6.18)
*Narrower in scope than most of this project's documents — Companions & Court Positions already built the entire Retinue mechanic, and the core doc already specified the abstracted loop directly. What's actually left to design is the destinations themselves, en-route risk, and a real CK3-style location tracking layer so a household's people can genuinely be in different places at once. This pass adds a Recall mechanic for cutting a trip short, confirms vacant-post consequences apply to any traveling Character rather than only the player's own retinue, gives Second Settlement and Campaign their own Encounter texture, and adds an explicit "home" Location type.*

---

## Contents

1. Scope & Role
2. Destinations
3. Committing to a Trip — The Abstracted Loop
4. En-Route Events — Flavor and Real Stakes
5. Concurrent Travel — Everyone Has a Location
6. The Retinue (Recap, Not Redesign)
7. Arrival — The Encounter
8. Correspondence's Boundary
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own definition: the player "(and optionally other family members) can journey to Rome, provincial capitals, a rival's estate, the frontier, or a campaign. Primarily abstracted (pick a destination, commit travel time, arrive to an encounter) with a lightweight real-time flourish... The primary gateway into Companion recruitment and many Espionage/Politics/Romance opportunities."

Companions & Court Positions §7 already built the entire Travel Retinue mechanic — what it is, what each member contributes, recruitment via travel — in full; this document doesn't redesign any of that, only triggers it. What this document actually needs to build: the destinations themselves as real, persistent places; the en-route event pool; the arrival-encounter framework; and, per direction, a genuine location-tracking layer so any Familia member — not just the player — can be somewhere else entirely at any given time, the same way CK3 tracks a court member's actual location rather than treating everyone as permanently "at home" unless directly controlled.

---

## 2. Destinations

Per the decision to make these named, specific, persistent places rather than generic types:

- **Rome** — the capital, a single always-available named location. The highest-tier venue for Politics & Patronage (sponsor-seeking for the cursus honorum, Senate-adjacent activity), Espionage's local-administration target at its most concentrated, and the natural hub for major trade and Correspondence.
- **Provincial Capitals** — one or more per starting region (Estate & Settlement's own region selection), each the seat of a Provincial Governor (Politics & Patronage §7) and the venue for that document's own higher-scope Legal & Court cases.
- **A Rival House's Estate** — literally the home settlement already tracked on that house's own LivingWorldActor record (Rival Houses §3.1). Visiting in person unlocks the full Characters Interaction Catalog against whoever's actually there, at real-world effectiveness a letter can't match — a Duel challenge, a marriage negotiation, a Romance & Seduction courtship, an Espionage placement.
- **The Frontier** — a region rather than a single settlement, the natural venue for Diplomacy with Non-Roman Peoples (§6.25, future) and carrying real elevated Piracy & Banditry and Natural Disaster exposure (§4).
- **A Campaign** — a genuinely mobile destination that moves with wherever Military & Combat's active deployment currently is, letting the player personally join a Force they've sent out or take up a Roman Service commission in person.
- **A Second Settlement** — where the player holds one via the Procurator mechanic (Companions & Court Positions §5.3), a real destination for checking in personally rather than relying on that appointee's own reports.

---

## 3. Committing to a Trip — The Abstracted Loop

Exactly the loop the core doc specifies: pick a destination, the game computes real **Travel Time** scaling with actual distance (Rome from an Italian heartland start is short; the same trip from a frontier province is long; a Rival House's estate's distance depends on where that house actually sits), commit, and the trip resolves as one committed block of time — a visible route indicator on the regional map (the "lightweight real-time flourish," cosmetic only) with periodic en-route Events (§4) rolling during the block, ending in Arrival (§7).

---

## 4. En-Route Events — Flavor and Real Stakes

Per the decision to keep both kinds in the pool, with real stakes weighted by actual route danger:

- **Flavor-only events** — always in the pool regardless of route: a chance meeting, a waystation market, a piece of local color, no mechanical stake attached. These exist purely to make the trip feel lived-in.
- **Real-stakes events** — weighted up sharply on genuinely dangerous routes (Frontier, Campaign, sea lanes) and down on secure ones (an established road between Italian settlements): a Piracy & Banditry ambush roll, a Disease exposure check, Natural Disaster interference (a washed-out road, a storm at sea).

**The Retinue is the actual mitigation**, exactly as Companions & Court Positions §7.2 already specified — a Bodyguard or Marshal reduces ambush odds, a Court Physician or Valetudinarius reduces Disease exposure. This document's own job is simply to define the event pool and the trigger conditions those existing retinue bonuses apply against, not to invent a second layer of risk-reduction.

---

## 5. Concurrent Travel — Everyone Has a Location

Per direction, given directly: household members' locations should be tracked the way CK3 tracks a court member's actual whereabouts, not assumed to default to "at home" — a son can genuinely be in Rome while the player is somewhere else entirely, or at home, at the same time. Every Character carries a **current location** (§10), and multiple trips resolve **fully concurrently**: the player can send any number of other Familia members on their own separate journeys at the same time as their own, each with its own destination, retinue, and timeline, resolving independently rather than queued one at a time.

**Worth restating now that this applies well beyond the player's own retinue:** Companions & Court Positions §7.2 already established that a traveling Overseer or Senior Position holder leaves their post effectively unstaffed for the duration — this applies uniformly to *any* traveling Character, not only someone in the player's own retinue. Sending a talented but currently-essential Vilicus off on an independent errand carries the same real cost as bringing them along personally; concurrent travel multiplies the *number* of decisions like this a player can make at once, not the underlying tradeoff itself.

**A trip can be cut short.** Committing to a destination and a travel time isn't an unbreakable contract — a genuine crisis at home (a Succession dispute, an attack, an urgent Legal matter) can trigger a **Recall**: an early, deliberate end to a trip, resolving the return leg immediately rather than waiting out the original commitment. A Recall isn't free — the traveler forfeits whatever Encounter (§7) they hadn't yet completed, and an already-committed multi-stage engagement (a Scheme, a Siege, a Hearing) they were personally present for doesn't simply pause and wait for them to leave.

**A genuine emergent-world consequence worth naming:** because destinations are persistent, real places (§2), two Characters who happen to be in the same place at the same time — a player-sent son and a rival's own traveling relative, both in Rome on unrelated business — can encounter each other there, entirely outside anything the player planned. This is the same "living world" texture Rival Houses' rival-vs-rival dynamics already rely on, now given a physical dimension.

---

## 6. The Retinue (Recap, Not Redesign)

Unchanged, cross-referenced only: Companions & Court Positions §7 remains the complete, authoritative treatment of who can accompany the player, what each retinue member contributes, and how recruitment via Travel actually works. This document doesn't repeat or revise any of it.

---

## 7. Arrival — The Encounter

Consistent with the core doc's own singular "arrive to an encounter" framing: each destination type offers a curated menu of what's actually available there, drawn from whichever systems are relevant to that place, rather than one universal action list. Rome's menu leans toward seeking a cursus honorum sponsor or a major Correspondence/trade opportunity; a Rival House's estate opens the full Interaction Catalog against whoever's actually present; the Frontier opens Diplomacy with Non-Roman Peoples negotiation; a Second Settlement's menu centers on reviewing the Procurator's own record in person and directly overriding whatever standing instructions no longer fit; arriving at a Campaign drops the traveler straight into whatever Military & Combat context is actually unfolding there — reviewing the Force in person, taking direct command of an unfolding engagement, or simply being present for a Roman Service commission's own duties. A destination the player has visited before carries forward whatever familiarity and Standing already exist there rather than resetting — a second visit to the same rival's estate is a continuation, not a fresh encounter.

---

## 8. Correspondence's Boundary

The core doc draws this line directly: Correspondence & Letters (§6.27, future) is "the remote counterpart to Travel... lower-risk and lower-reward than the equivalent handled in person." This document holds that boundary rather than encroaching on it: anything requiring real physical presence — a Duel, a Seduce Scheme's later stages, an in-person Legal Hearing, an Espionage placement that needs a body actually on-site — stays Travel's domain; the lower-stakes remote equivalents (a petition, a routine negotiation, staying in touch with a married-off daughter) are left entirely to that document's own future pass.

---

## 9. Cross-System Integration

- **Companions & Court Positions:** §7 is fully reused for the Retinue; this document is purely the trip and destination layer around it.
- **Characters:** every destination's Encounter (§7) opens the same universal Interaction Catalog; §5's location tracking is a genuine addition to that document's own schema (§10).
- **Rival Houses:** a specific house's home settlement (§3.1 of that doc) is the concrete destination behind "a rival's estate"; encountering a traveling rival Character away from home (§5) is a direct application of that document's rival-vs-rival living-world texture.
- **Politics & Patronage:** Rome and Provincial Capitals are the physical venues for the cursus honorum sponsor mechanic and provincial governance.
- **Espionage:** Rome specifically is the highest-concentration venue for infiltrating "the local Roman administration"; a Persistent Network placement's initial recruitment often happens here.
- **Romance & Seduction:** in-person courtship and Seduce Scheme stages needing real presence are explicitly Travel's domain, not Correspondence's.
- **Military & Combat:** the Campaign destination lets the player personally join a deployed Force or take up a Roman Service commission in person.
- **Piracy & Banditry (§6.24, future) / Natural Disasters (§6.17, future) / Disease & Public Health (§6.13, future):** all three supply §4's real-stakes en-route event types.
- **Diplomacy with Non-Roman Peoples (§6.25, future):** the Frontier destination is that system's natural venue.
- **Steward/Council Auto-Management (§6.28, future):** exactly the QoL layer the core doc already names as covering the household while the player is away.
- **Correspondence & Letters (§6.27, future):** §8 draws the explicit boundary that document's own eventual pass should respect.

---

## 10. Data Model

```
TravelTrip {
  tripId,
  travelerCharacterId,
  retinueCharacterIds: [...],   // Companions & Court Positions §7 — unchanged mechanic
  destinationId,          // a Location (below)
  travelTimeMonths,
  monthsElapsed,
  routeDangerLevel,        // drives §4's real-stakes event weighting
  enRouteEvents: [...],
  status,                // "traveling" | "arrived" | "returning" | "recalled"
  encounterCompleted: bool,   // false if a "recalled" trip forfeited an unfinished Encounter (§7)
}

Location {
  locationId,
  type,                // "home" | "rome" | "provincialCapital" | "rivalEstate" | "frontierRegion" |
                         // "campaign" | "secondSettlement"
  linkedActorId,          // set for "rivalEstate" — the specific Rival Houses LivingWorldActor
  linkedSettlementId,       // set for "secondSettlement" or "home"
}

// Addition to Characters' own Character{} schema (Characters §14):
// currentLocationId — every Character, not just the player, carries one; defaults to a "home" Location
```

---

## 11. Open Questions

- **All numeric sizing.** Consistent with this project's convention: travel time's actual distance formula, en-route event roll frequency, and route danger-level weighting are all unsized.
- **Concurrent trip capacity.** §5 establishes travel is fully concurrent without capping how many simultaneous trips a household can sensibly run at once — whether there's a practical limit (Wages/upkeep cost per traveling retinue, perhaps) isn't addressed.
- **Campaign-following's exact risk delta.** §2 lets a player join an active campaign in person; whether doing so adds risk beyond Military & Combat's own ordinary engagement stakes isn't specified.
- **Return trips and unplanned extension.** Whether a trip's return leg is automatic and identical in risk to the outbound one, or whether a player can extend a stay at a destination indefinitely rather than committing to a round-trip upfront, isn't decided.
- **Recall's exact cost and eligibility.** §5 establishes that a trip can be cut short for a genuine crisis without specifying what qualifies as one, or whether a Recall carries any cost beyond the forfeited Encounter — a Dignitas or relationship cost for abandoning an in-progress negotiation, say.
