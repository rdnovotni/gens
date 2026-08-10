# GENS — System Design: Organic & Autonomous Growth (§6.54, new)
*The shared engine this project has been quietly assuming exists without ever actually building it. Rival Houses describes Background Houses evolving "through periodic abstract rolls — a rough simulation of births, deaths, marriages, and fortune shifts." Notable Households tracks aging, marriage, birth, death, and inheritance. Settlement Demographics tracks a population "growth trend." Cultures of the Known World tracks Cultural Drift. Technology & Discoveries tracks Cultural Head Start acceleration. Every one of these is the same underlying idea — the wider world keeps moving whether or not the player is watching — described independently, five separate times, with no single shared mechanism or resolution order underneath any of it. This document is that mechanism: a real World Tick, a single generic Growth Roll every one of those systems can plug into, and, per direction, honest, CK3-style success-and-failure branching so the living world can genuinely surprise the player rather than only ever trending smoothly in one direction. Per direction, this system is scoped entirely to the wider world — the player's own household continues to run through its own explicit systems, never through this document's own abstracted rolls.*

---

## Contents

1. Scope & Role — The World's Own Background Engine
2. The World Tick — Order of Operations
3. The Growth Roll — One Shared Abstraction
4. Life Events — Real Success and Real Failure
5. The Household Lifecycle Pipeline
6. Settlement-Level Autonomous Growth
7. NPCs and the Content Systems
8. Legibility — How the Player Actually Perceives Any of This
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role — The World's Own Background Engine

This document doesn't replace anything. Rival Houses' Background/Note tiering, Notable Households' lifecycle events, Settlement Demographics' growth trend, Cultural Drift, and Technology & Discoveries' diffusion all remain exactly as designed. What none of them ever specified is the actual shared mechanism underneath the words "periodic abstract roll," or the order in which all of it should resolve relative to everything else each month. This document supplies both: a real World Tick (§2) and a single, generic Growth Roll (§3) that every one of those systems' own vague language now points to concretely.

Per direction, this document is bounded to **the wider world only** — every other household, settlement, and institution the player doesn't directly control. The player's own household never runs through this document's own rolls; it continues to be driven entirely by its own explicit decisions across Estate & Settlement, Villa, Succession & Dynasty, and every other system built around deliberate player choice.

---

## 2. The World Tick — Order of Operations

Once the player's own household resolves its own monthly turn, the wider world resolves in a fixed, real sequence — necessary because later steps genuinely depend on earlier ones' outcomes (a settlement's own population trend affects a new business's survival odds; a House's own extinction can trigger a settlement-level contested-plot event), and resolving out of order would produce results that don't actually make sense together:

1. **Household-level lifecycle** — Notable Households' aging/marriage/birth/death cycle; Background and Note-tier Rival House abstract rolls (Rival Houses §2.1).
2. **Settlement-level demographic and economic movement** — Settlement Demographics' own growth trend; spontaneous Notable Business formation and closure (§6 below); any live Business Competition escalation progressing a further rung.
3. **Civic and infrastructure movement** — an NPC magistrate's own Public Works project, per that document's own existing provision.
4. **Political and military movement** — Rival Ambition (Rival Houses §4): contested elections, Feuds, client-poaching.
5. **Cultural and technological drift** — Cultural Drift; Technology & Discoveries' own Cultural Head Start and diffusion (§8 of that document).
6. **Extinction and replacement** — any House whose line ran out this month is finalized (Succession & Dynasty §7); a new Rising House is rolled to replace it (Rival Houses §2.2, Notable Households §6), closing the loop rather than letting the world only ever shrink.

---

## 3. The Growth Roll — One Shared Abstraction

A single, generic mechanism, deliberately **not** a new stat to track per entity — it simply reads whatever tags already exist (a House's own standing trend, a settlement's Employment Ratio, a Culture's own real historical trajectory, a Trade Route's own health) and resolves them into one of three real outcome bands:

- **Growth** — the entity's relevant figure (wealth, population, prestige, prominence) improves.
- **Stagnation** — no meaningful change; the default, most common outcome for anything not already trending strongly.
- **Decline** — the relevant figure worsens.

The odds of each band are weighted directly by the entity's own existing standing tag — a Rising House or a settlement with a strong growth trend skews meaningfully toward Growth; an Established one skews toward Stagnation; a Declining one skews toward Decline — with concrete external modifiers (a recent Natural Disaster, an active Feud, a newly published Discovery) shifting the weighting further in an obvious, legible direction. This is the one real formalization this document adds: every other system's own previously-vague "periodic abstract roll" now means this, specifically, rather than five different unstated processes.

---

## 4. Life Events — Real Success and Real Failure

Per direction — a real CK3 "sometimes it works, sometimes it doesn't" texture. A Growth Roll landing in a particularly consequential band can trigger a named **Life Event**, each with genuinely distinct success and failure outcomes rather than a single guaranteed result:

- **Succession Attempt** — triggered when a House of Note's Head dies or retires. Reuses Succession & Dynasty's own resolution wholesale, per Rival Houses' own existing commitment (§3.2 of that document). **Success:** a smooth handoff. **Failure:** a real Succession Crisis — a contested claim, a Cadet Branch splitting off (Rival Houses §2.2), or, at the sharpest extreme, outright extinction if no viable heir exists at all.
- **Marriage Negotiation** — a House pursuing an alliance match. **Success:** the match is made, real Standing gained. **Failure:** the courted family declines, a real, minor Standing cost to the rejected suitor.
- **Business Venture** — a Notable Business (spontaneous or established) attempting to expand or simply endure. **Success:** growth, rising Reputation. **Failure:** closure through Economy & Finance's own existing Insolvency mechanic.
- **Military/Feud Engagement** — resolved entirely through Military & Combat's own Combat Resolution Engine; this document simply confirms it as one of the real Life Event types a Growth Roll can trigger for a Rival House pursuing an ambition.
- **Technological Pursuit** — new, explicit permission: per Technology & Discoveries §6, a Rival House or Institution can independently run its own Discovery Attempt. **Success:** the Discovery, potentially triggering a Discovery Race (§9 of that document) if the player happens to be pursuing the same one. **Failure:** the same partial, permanent head-start-on-failure rule that document already established, never a total loss.

---

## 5. The Household Lifecycle Pipeline

A single, explicit flowchart consolidating what Notable Households and Rival Houses each described only in fragments:

**Notable Household** (ambient, undifferentiated, Settlement Demographics/Notable Households) → *Rising House Transition* (triggered by sustained Prosperous wealth or a property buyout, Notable Households §6) → **Background Rival House** (lightweight tracking, Rival Houses §2.1) → *Promotion* (triggered by real player contact, Rival Houses §2.3) → **House of Note** (full Dossier, Rival Houses §7) → *Decline* (repeated Decline-band Growth Rolls, a lost Feud, or a failed Succession Attempt) → **Extinction** (Succession & Dynasty §7, Rival Houses §5.3) → *Replacement* (a new Rising House rolled per Rival Houses §2.2), closing the loop back to the top.

Every arrow in this pipeline already existed somewhere in the project's own text; this document's only real contribution is drawing the whole thing as one continuous cycle rather than leaving it implied across two separate documents.

---

## 6. Settlement-Level Autonomous Growth

- **Population** now explicitly reads the Growth Roll (§3) each Tick, rather than Settlement Demographics' own "growth trend" remaining an unspecified process.
- **Spontaneous Business Formation** — new, explicit mechanic, and the concrete answer to a question Notable Businesses and Business Competition both assumed without ever specifying: each Tick, an underserved trade (read directly against Business Competition's own Market Entry and Saturation logic, §6 of that document) carries a real, small chance of a brand-new Notable Business spontaneously forming, founded by a freshly, lazily instantiated Character per Characters' own generation rules.
- **Spontaneous Business Closure** — the mirror: an existing Notable Business failing a Business Venture Life Event (§4) can close outright, its Property Record and market share becoming available exactly as Business Competition's own "Spoils of Victory" (§7 of that document) already describes for a defeated rival.
- **NPC-Driven Public Works** — confirms and extends Public Works & Euergetism's own existing provision for a sitting magistrate funding a civic project independently of the player, now explicitly triggered by this Tick's own Civic/Infrastructure step (§2, step 3) — a settlement's own skyline can genuinely change over a long game even in a place the player never personally invests in.

---

## 7. NPCs and the Content Systems

A brief, confirming section rather than new invention: a Living World Actor (Rival Houses §6) can independently engage with this project's own content systems at whatever depth its current tier actually warrants. A House of Note can host its own Activities (Activity Engine §8, already established), independently pursue a Technology Discovery (§4 above), commission Art or Books for its own Pinacotheca or Library, maintain a real Menagerie, or commission a Ship — a natural, light extension of the "the player is never a protected special case" principle this project already applies elsewhere (Piracy & Banditry §7). A Background House's own version of all of this stays pure, unstated flavor; a House of Note's can become a real, felt rival collection worth covering or competing over, directly engaging Art & Art Commissions' own Rival Collecting mechanic (§12.1 of that document) and Masterworks' own Discovery Races.

---

## 8. Legibility — How the Player Actually Perceives Any of This

Reused machinery, not new invention: Dynasty Chronicle catches anything crossing into real House-of-Note relevance; Correspondence & Letters' own gossip channel is how a Rival Dossier's own deliberate staleness (Rival Houses §7) gets refreshed. Most Background House and settlement-level Growth Rolls produce **no visible output to the player at all** — and that silence is the correct, intended behavior, not a gap: the whole point of this document is a world that keeps moving without requiring the player to watch every roll of it happen.

---

## 9. Cross-System Integration

- **Rival Houses:** this document is the direct, concrete mechanism behind that document's own previously-unspecified "periodic abstract rolls" (§2.1 of that document), and the explicit order-of-operations context for its own Rival Ambition (§4).
- **Notable Households:** the Rising House Transition (§6 of that document) and this document's own Household Lifecycle Pipeline (§5) are the same real bridge, now drawn as one continuous cycle.
- **Settlement Demographics:** population growth now explicitly reads the Growth Roll (§3).
- **Notable Businesses / Business Competition:** Spontaneous Business Formation and Closure (§6) supply the concrete origin and end mechanism both documents previously assumed without specifying.
- **Public Works & Euergetism:** NPC-driven civic projects are now explicitly tied to this document's own Tick order (§2).
- **Succession & Dynasty:** the Succession Attempt Life Event (§4) is a direct, real application of that document's own resolution rules to a non-player House.
- **Technology & Discoveries:** the Technological Pursuit Life Event (§4) is new, explicit permission for a Living World Actor to independently accelerate a Discovery, directly feeding that document's own Discovery Race mechanic (§9 of that document).
- **Cultures of the Known World:** Cultural Drift is read directly as one of this document's own World Tick steps (§2, step 5) rather than a disconnected process.
- **Activity Engine:** §7's confirmation of NPC-hosted Activities directly reuses that document's own §8.
- **Art & Art Commissions / Masterworks & Unique Crafted Objects:** §7 directly engages both documents' own Rival Collecting and Discovery Race mechanics.
- **Dynasty Chronicle / Correspondence & Letters:** both are this document's own real legibility layer (§8), reused rather than duplicated.

---

## 10. Data Model

```
WorldTickLog {
  month,
  stepsResolved: [ "householdLifecycle", "settlementMovement", "civicInfrastructure",
                    "politicalMilitary", "culturalTechnological", "extinctionReplacement" ],
}

GrowthRoll {
  rollId, entityType,
  entityId,
  standingTagUsed,
  externalModifiers: [ ... ],
  outcome,
  month,
}

LifeEvent {
  eventId, eventType,
  entityId,
  outcome,
  failureDetail,
  month,
}
```

---

## 11. Open Questions

- **All numeric sizing**, per convention — Growth Roll band-weighting by standing tag, Life Event trigger thresholds, and Spontaneous Business Formation's own base rate are all unsized.
- **Total World Tick computational scope.** Whether every single Background House and Notable Household in the entire known world is actually rolled every month, or whether distant, currently-irrelevant regions resolve at a coarser, less frequent cadence, isn't specified.
- **Player visibility toggle.** Whether a player who wants more insight into the wider world's own background movement should have an optional, more detailed "world almanac" view is left as a possible future UX feature.
- **Interaction between simultaneous Life Events.** Whether two Houses independently pursuing the same Marriage Negotiation target in the same month should resolve as a real, felt competition or simply resolve independently in Tick order isn't addressed.
