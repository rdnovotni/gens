# GENS — System Design: Wandering Populations (§6.30, FINAL)
*Final polish pass. The missing itinerant layer between Travel's one-off encounters and Education & Culture's fixed Institutions of Renown. That document already covers a household sending its own son *to* Athens; this document covers the reverse and the persistent version — a real, named philosopher, architect, merchant, or entertainer who exists in the world independent of the player's own actions, moving between real places on their own logic, growing or losing renown, and, critically, available to a Rival House exactly as much as to the player. A Wanderer the player doesn't act on doesn't wait patiently at home — they move on, or someone else hires them first. This pass verified every cross-reference against its source document (all checked out, including a citation to Correspondence & Letters this pass double-checked and confirmed correct) and added a worked example grounding the competition mechanic in a concrete "I should have left sooner" scenario.*

---

## Contents

1. Scope & Role
2. Wanderer Types
3. The Itinerary — How a Wanderer Actually Moves
4. Fame — Growth, Decay, and What It's Worth
5. Encountering a Wanderer
6. Engagement — Host or Recruit
7. Competition — Losing a Wanderer to Someone Else
   7.1 A Worked Example — Racing a Rival for the Same Rhetorician
8. Sampling, Promotion & the Ambient Pool
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

Travel's own existing encounter machinery already lets a one-off "promising client's son" or "stranded freedman" join the household on the spot (Companions & Court Positions §7.3). That's the right mechanic for a chance meeting, but it doesn't cover something real Roman society genuinely had: a class of people who moved *professionally*, on their own circuit, whose reputation traveled ahead of them and who multiple households might be actively competing to host or hire at any given moment. A traveling sophist touring the Greek East's own real Institutions of Renown, a peddler working a trade circuit between provincial towns, a troupe of actors booked settlement to settlement — none of these people are waiting around for the player specifically. This document gives them a real, persistent existence: a name, a specialty, a current location, a moving itinerary, and a Fame score that rises and falls whether or not the player is watching.

---

## 2. Wanderer Types

Six real, historically grounded categories — extensible, not exhaustive:

- **Philosophers, Rhetoricians & Sophists** — the itinerant teaching and lecturing circuit real history calls the Second Sophistic, especially prominent in the Greek East. A Wanderer of this type gravitates toward Education & Culture's own Institutions of Renown (Athens, Rhodes, Alexandria, Pergamon, Massilia) — the exact flip side of that document's own Study Abroad Journey, where the student travels to a fixed place; here, the teacher travels between them.
- **Architects & Engineers** — real, skilled specialists brought in from elsewhere for a major commission, gravitating toward wherever Buildings or Monuments & Legacy Building construction is actively underway.
- **Merchants & Peddlers** — an individually named trader distinct from Resources & Goods' own abstract trade-route flow, carrying real, sometimes rare or region-specific goods, gravitating toward wherever Economy & Finance's own Market Dynamics suggest the best margins.
- **Entertainers** — real traveling troupes of actors, musicians, and acrobats, hired settlement to settlement, gravitating toward Games & Spectacle's own funded events and a Villa's own Symposium hosting demand.
- **Physicians** — itinerant medical specialists, gravitating toward a Disease & Public Health outbreak or a settlement without its own Court Physician.
- **Holy Men, Astrologers & Mystics** — real, itinerant religious and mystic figures, gravitating toward Religion's own foreign-cult encounter opportunities and a Character's own Astrologer/Theologian Traits.

---

## 3. The Itinerary — How a Wanderer Actually Moves

Each Wanderer carries a real, moving current **Location** — a settlement, or, where relevant, a specific region document's own named Gazetteer entry — and an **Itinerary** that advances it over time, weighted by that Wanderer's own type-specific logic rather than pure randomness: a philosopher's itinerary skews toward Institutions of Renown and other high-Prominence Gazetteer locations; a merchant's skews toward wherever Market Dynamics currently favor; an architect's skews toward active construction; an entertainer's skews toward funded Games and Symposium demand. This gives Rival Houses' own established "a living world, whether or not the player is watching" principle a genuine population of people, not just houses, moving through it.

---

## 4. Fame — Growth, Decay, and What It's Worth

*(Corrected: this Fame score is not a Wanderer-specific mechanic. It is Games & Spectacle's own universal Fame field, Characters §13–14 — the same 0–100 score any Character carries, generated by many systems — applied here to a Wanderer's own itinerant reputation as one source among the many that field already supports. See Celebrities & Influential Figures §6.22.1 for that field's fuller treatment.)*

A Wanderer's own Fame rises after a successful engagement (a well-received lecture, a well-executed building, a profitable trade, a healed patient, a well-attended performance) and fades through sustained obscurity — the same Rising/Established/Declining shape Rival Houses already applies to a gens's own standing-trend, here applied to an individual. A high-Fame Wanderer is a genuinely more valuable Host or Recruit target (§6) and a genuinely more visible object of §7's own competition; a Wanderer whose Fame has quietly faded is easy to engage and easy to lose interest in, a real, honest reflection of how itinerant reputation actually worked.

---

## 5. Encountering a Wanderer

Three real ways a Wanderer actually becomes visible to the player, layered rather than exclusive:

- **Direct Travel encounter** — the player's own Travel destination (Travel §2) happens to currently host the Wanderer, surfaced through that document's own existing Arrival-Encounter framework (§7 of that document) as a genuine, available option alongside whatever else that destination already offers.
- **Ambient rumor** — the Monthly Report or Correspondence & Letters carries word of a Wanderer's own current renown and rough location before the player ever travels there — "a celebrated rhetorician is said to be touring the province" — giving the player a real reason to plan a Travel destination around reaching them.
- **A direct approach** — for a sufficiently Prominent household (Events §5), a famous enough Wanderer can seek the player out directly, the same "renown attracts renown" logic Education & Culture's own §5.4 already applies to a household's own Institution-tier Academy or Library.

---

## 6. Engagement — Host or Recruit

Two genuinely different, non-exclusive ways to actually use a Wanderer once encountered:

- **Host** — a one-time, lower-commitment engagement: a funded lecture, a building consultation, a hired performance, a course of treatment, a single consultation. This delivers a real, immediate benefit (a Cultural Prestige boost, a construction discount, a rare goods purchase, a Health recovery, a Favor gain) without recruiting the Wanderer into the household — they remain independent, and move on afterward per their own Itinerary (§3).
- **Recruit** — a genuine, permanent offer to join the household outright, converting the Wanderer into a full Familia record the instant it succeeds, per Familia §7's own existing promotion rule — from that point on, an ordinary Companion or Court Position holder (a hosted physician becomes the household's own Court Physician; a hosted architect becomes available for every future Buildings or Monuments commission without needing to be found again). A successful Recruit ends that Wanderer's own independent Itinerary entirely.

---

## 7. Competition — Losing a Wanderer to Someone Else

The genuinely load-bearing mechanic that separates a Wanderer from an ordinary Travel encounter: a sufficiently high-Fame Wanderer is a real, visible object of interest to more than just the player. A Rival House can independently Host or Recruit the same Wanderer, resolved the instant either side actually commits rather than held open indefinitely — a player who hears the rumor (§5) and delays risks arriving to find the philosopher already left for a rival's own Symposium, or, in the sharper case, already recruited into a rival's own household entirely. This is a direct, individual-scale application of Design Pillar #6's own "a living world" principle, and gives Correspondence & Letters' own early-warning function (§5 of that document) genuine urgency: hearing the rumor early is what actually gives the player a real chance to win the race.

### 7.1 A Worked Example — Racing a Rival for the Same Rhetorician

Concretely: a letter arrives via Correspondence's own News & Gossip action reporting that a rising rhetorician is touring the Greek East, currently at Rhodes and expected to move on within a season. The player can commit a Travel destination toward Rhodes immediately, arriving to a real, available Host or Recruit option — or delay, perhaps to finish a harvest or a Curia election first, and risk exactly what §7 describes: arriving to find the rhetorician already gone, or, worse, to learn through a later Report that a rival house hosted him first and is now enjoying the Cultural Prestige and Clientela-adjacent goodwill that could have been the player's own. Nothing about this sequence required a bespoke event chain — it's §3's Itinerary, §4's Fame, §5's rumor, and §7's competition check, running exactly as designed, producing a genuine "I should have left sooner" story entirely on their own.

---

## 8. Sampling, Promotion & the Ambient Pool

Consistent with the sampling-and-promotion pattern this project has now applied at every population tier (Rival Houses' Background/Notable split; Notable Households' own ambient-sampling-and-triggered-promotion pattern), the overwhelming majority of Wanderers who could plausibly exist in the wider world stay purely ambient and unnamed — background texture, never individually tracked. A specific Wanderer is only actually instantiated with a real name, Fame score, and moving Itinerary when a player's own Travel destination, Correspondence rumor, or Prominence-driven direct approach (§5) makes them genuinely relevant — the same restraint keeping Rival Houses and Notable Households from needing to track every gens or every family in the world, applied here to keep the game from needing to track every conceivable itinerant specialist across the entire map at all times.

---

## 9. Cross-System Integration

- **Travel:** §5's direct-encounter path is a new, named option within that document's own existing Arrival-Encounter framework (§7 of that document), not a parallel encounter system.
- **Education & Culture:** Philosopher/Rhetorician Wanderers are the direct structural mirror of that document's own Study Abroad Journey (§5) — the teacher moving to the student's world instead of the reverse — and gravitate toward the same named Institutions of Renown.
- **Companions & Court Positions:** a successful Recruit (§6) converts a Wanderer directly into an ordinary Companion or Court Position holder, per Familia §7's existing promotion rule.
- **Rival Houses:** §7's competition mechanic is a direct, individual-scale application of that document's own "living world" rival-vs-rival principle; a Wanderer's own Fame trend uses the identical Rising/Established/Declining shape that document applies to a house's own standing.
- **Games & Spectacle, Celebrities & Influential Figures:** a Wanderer's own Fame (§4) is that shared universal field, not a parallel score — corrected per Celebrities & Influential Figures §1.
- **Correspondence & Letters:** §5's ambient-rumor path is a genuine, concrete use case for that system's own early-warning function, giving a player real lead time in §7's own competitive races.
- **Events, Prominence:** §5's direct-approach path reuses that document's own Prominence-gated "the wider world notices you" logic directly.
- **Buildings, Monuments & Legacy Building:** an Architect/Engineer Wanderer is a real, named alternative to the player's own default construction pipeline, offering a genuine quality or speed advantage at real cost.
- **Games & Spectacle, Villa:** an Entertainer Wanderer is a real, hireable alternative to a household's own permanent Actor Companion, sized for a single funded event or Symposium rather than a standing position.
- **Disease & Public Health:** a Physician Wanderer is a real, temporary alternative or supplement to a household's own standing Court Physician.
- **Religion:** a Holy Man/Astrologer/Mystic Wanderer is a natural, individually-named source for a foreign-cult encounter or an Auspices-adjacent reading, distinct from a household's own standing Sacerdos Domesticus.
- **Dynasty Chronicle:** a successfully recruited high-Fame Wanderer, or a dramatic loss of one to a rival, is real, tiered material in its own right.

---

## 10. Data Model

```
Wanderer {
  wandererId, name, wandererType,      // "philosopher" | "architect" | "merchant" | "entertainer" | "physician" | "holyManOrAstrologer"
  currentLocationId,                    // a settlement or a region document's own named Gazetteer entry
  itinerary: [ { locationId, arrivalMonth } ],
  fame,                                  // Games & Spectacle's own universal 0-100 field (Characters §13-14), not a new score — §4
  fameTrend,                             // "rising" | "established" | "declining" — mirrors Rival Houses' own standing-trend shape
  isActivelyTracked: bool,               // false while purely ambient — §8
}

WandererEngagement {
  engagementId, wandererId, householdId,
  engagementType,                         // "host" | "recruit"
  benefitDelivered,                        // varies by wandererType — Prestige, construction discount, rare goods, Health, Favor
  resultingCompanionOrPositionId,           // set only when engagementType is "recruit"
}

WandererCompetitionEvent {                // §7
  eventId, wandererId,
  competingHouseholdIds: [ ... ],
  resolutionMonth,
  winningHouseholdId,
  wasPlayerAwareViaRumor: bool,            // §5 — did Correspondence's early warning actually reach the player in time
}
```

---

## 11. Open Questions

- **All numeric sizing**, per this project's standing convention — Fame growth/decay rates, itinerary movement frequency, and the Prominence threshold for a direct approach (§5) are all unsized.
- **Wanderer count per region/era.** How many Wanderers should exist as actively-tracked at once across a typical playthrough isn't specified — too few and the system feels empty, too many and it competes for attention with Rival Houses and Notable Households.
- **Multi-type Wanderers.** Whether a single individual could plausibly carry more than one Wanderer type (a physician who also dabbles in astrology, a real, plausible historical combination) isn't addressed.
- **Host engagement repeatability.** §6 doesn't specify whether the same Wanderer can be Hosted multiple times across separate visits to the same settlement, or whether a Host engagement consumes that opportunity for a meaningful stretch.
- **Interaction with the Alternate History Layer or other exceptional regions.** Whether a Wanderer's own itinerary can plausibly cross into Mesopotamia's own temporary window or Armenia's own contested territory isn't addressed — left to a future pass's judgment.
