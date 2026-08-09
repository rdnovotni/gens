# GENS — System Design: Feasts (§6.48, the Activity Engine's first Activity Type)
*The first real proof of concept for the Activity Engine (§6.47) — and, fittingly, the Activity Type this project was already most prepared for: Food Culture (§6.40) built the entire Banquet Quality formula, Cuisine Match, and Ostentatious Display mechanics without ever having a real container to run them inside. This document is substantially a wiring exercise: it plugs Food Culture's existing machinery directly into the Activity Engine's six real slots, and adds only what a genuine multi-phase social event needs that a flat banquet record never had — real seating politics, a real Phase structure, and a Purpose taxonomy tying a Feast to whichever other system actually called for it.*

---

## Contents

1. Scope & Role — Wiring, Not Reinventing
2. The Feast's Six Slots
3. Phases — Arrival, the Meal, the Comissatio, Departure
4. Seating — A Real, Concrete Status Signal
5. The Feast Tier — Reusing Banquet Quality Directly
6. Feast Purpose — Why This Feast Is Happening
7. Entertainment
8. Ostentatious Feasts and the Witness Pool, Resolved
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role — Wiring, Not Reinventing

This document doesn't touch Food Culture's own Banquet Quality formula (§2 of that document), Cuisine Match (§3), or Ostentatious Display (§4) — all three stay exactly as designed and become this document's own direct Quality-axis input (§5), unmodified. What this document adds is the real, missing connective structure: a Feast as an actual **Activity** (§6.47), with a real Guest List, real Phases, real seating politics, and a real reason for happening — the difference between "a household eats a nice dinner" and "a household hosts an event with its own beginning, middle, end, and story."

---

## 2. The Feast's Six Slots

Filling in the Activity Engine's own §2 anatomy directly:

1. **Host** — any household, or an NPC per Activity Engine §8.
2. **Type** — `"feast"`.
3. **Venue** — the Triclinium (the default), the Oecus (grander, Domus-stage), the Andron (a Hellenized-flavored option, though the fuller philosophical Symposium remains its own future Activity Type built on richer Education & Culture content), the Gallic Feasting Hall (frontier), or the Viridarium/Summer Triclinium (a seasonal outdoor variant).
4. **Guest List** — per Activity Engine §4, ranging from a small family dinner to a large patron-client gathering.
5. **Duration** — Quick by default, resolving within a single month; a Feast can also serve as one Phase-sequence nested inside a larger Extended Activity (a Wedding's own culminating banquet, a Triumph's victory feast) once those future Activity Types exist.
6. **Phases** — §3.

---

## 3. Phases — Arrival, the Meal, the Comissatio, Departure

A real, Feast-specific override of the Activity Engine's generic default sequence:

- **Arrival & Seating.** Guests arrive — naturally following the same day's Salutatio where one occurred — and Seating (§4) is assigned.
- **The Meal.** The actual dining, reading Food Culture's Banquet Quality, Cuisine Match, and Ostentatious mechanics directly (§5), with Entertainment (§7) running concurrently where the host has arranged for it.
- **The Comissatio.** A real, historically distinct post-meal phase, genuinely lighter and more informal in register than the Meal itself — the natural venue for a private aside, a Scheme's own Confront With Evidence moment, a Romance flirtation, or a political favor request, per the Activity Engine's own §6.1 examples. An **Arbiter Bibendi** ("master of the drinking," a real, attested Roman and Greek social office) sets the evening's own wine-to-water ratio and pace — reusing the household's own Symposiarch (Companions & Court Positions) where one exists, or a lighter, ad hoc appointment otherwise.
- **Departure.** Guests leave; the Activity Engine's own §9 Resolution & Outcome fires, generating the Feast's Activity Record.

---

## 4. Seating — A Real, Concrete Status Signal

A real, well-documented Roman dining practice, and a genuinely fun, concrete mechanic worth building directly: a Triclinium's three couches carried a real, ranked internal hierarchy, with one specific position — the *locus consularis*, the seat of honor on the middle couch — reserved for whichever guest the host most wished to honor. This document builds that real practice into a real, deliberate host decision, finer-grained than and distinct from the Activity Engine's own blunter Exclusion mechanic (§4.2 of that document):

- **Under-seating** a guest below their own reasonably expected standing — a senior Decurion placed on a lesser couch, a Rival House's own Head seated among ordinary clients — is a real, felt Insult-equivalent event, even though the guest was invited and did attend.
- **Over-seating** a guest above their expected standing is a real, deliberate honor, carrying its own real Dignitas and relationship payoff to the host for the visible generosity — at the real risk of a corresponding envy or Insult reaction from whoever was thereby displaced from where they expected to sit.

A host managing a genuinely large or politically delicate Guest List faces a real, legible seating-politics puzzle this way, not merely a binary invite-or-exclude decision — the single most concrete new texture this document adds on top of the Activity Engine's own general shape.

---

## 5. The Feast Tier — Reusing Banquet Quality Directly

No new formula. Food Culture's own `BanquetRecord` — Ingredient Tier, Preparation Tier, Venue Tier, and Cuisine Match (§2–3 of that document) — **is** this document's Quality axis (Activity Engine §5.2), read without modification, resolving into that same four-tier Modest/Respectable/Refined/Legendary output. The Feast's own Scale axis (Activity Engine §5.1) is set the ordinary way, from Guest List size and Venue choice, exactly as the Engine already specifies.

---

## 6. Feast Purpose — Why This Feast Is Happening

A real, meaningful tag rather than pure flavor text, since a Feast's actual real-world Roman function varied significantly by occasion. Several real, distinct Purposes, each tying directly into an existing system rather than floating free:

| Purpose | Ties Directly Into |
|---|---|
| **Patronage Dinner** | Politics & Patronage's existing patron-client dinner (§4.3 of that document) |
| **Funeral Feast** | Ancestor Veneration & Funerary Customs' own Proper/Grand funeral tier (§2.2 of that document) — retroactively, this is that Purpose |
| **Wedding Feast** | A forward hook for the future Wedding Activity Type's own culminating event |
| **Religious Festival Feast** | Religion's Sacred Calendar (§5) and the Rites Budget (§3.1) |
| **Triumphal/Victory Banquet** | Military & Combat's own triumph |
| **Competitive Euergetism Feast** | A public feast funded via Public Works & Euergetism's own competitive-generosity ladder (§5) or an Aedile's own funding duty |
| **Ordinary/Social** | No special tie — hosting for its own sake, building Clientela or simply entertaining family |

Each Purpose can carry its own natural default Guest List composition and expected Scale (a Funeral Feast reads very differently at Lavish Scale than an Ordinary one does) without changing any of the underlying mechanics — Purpose shapes expectation and reception, not the formula itself.

---

## 7. Entertainment

A real, optional additional Quality input, distinct from and additive to §5's core Banquet Quality — this document simply gives already-existing content a real slot to fill during the Meal or Comissatio Phase rather than inventing anything new: hired musicians (a Masterwork Musical Instrument, §5 of that document, gets genuine, direct use here), a commissioned Playwright's own Drama performed as after-dinner entertainment, a Poet reciting verse, a hired performer (Wandering Populations' own Entertainer-type Wanderer, engaged per that document's own Host mechanic, §6), or, at the harsher and more Ostentatious end, a private gladiatorial bout — real, historically documented elite excess, and a natural, direct escalation of §8's own Ostentatious mechanic.

---

## 8. Ostentatious Feasts and the Witness Pool, Resolved

No redesign: Food Culture's own Ostentatious flag (§4.1) and Sumptuary enforcement mechanism (§4.2) apply to a Feast exactly as already built. This document simply closes the loop that mechanic's own text left open: a Feast's own Guest List — this document's own concrete instance of the Activity Engine's Witness Pool (§7 of that document) — **is** the actual publicity check a Censor or magistrate would be reading when deciding whether an Ostentatious Feast has been held visibly enough to act on. No new detection mechanism is needed; the Engine's own general Witness Pool concept and Food Culture's own specific enforcement trigger were always describing the same real thing.

---

## 9. Cross-System Integration

- **Activity Engine:** this document is the Engine's own first fully-specified Activity Type, filling every one of its six slots without altering the Engine itself.
- **Food Culture:** Banquet Quality, Cuisine Match, and Ostentatious Display are all reused directly and unmodified as this document's own Quality axis and escalation mechanic.
- **Villa:** every named Venue is an existing room; none require new construction.
- **Politics & Patronage:** the Patronage Dinner Purpose is that document's own existing patron-client dinner, now formally an Activity.
- **Ancestor Veneration & Funerary Customs:** the Funeral Feast Purpose retroactively names that document's own existing funeral-feast component.
- **Religion:** the Religious Festival Feast Purpose ties directly to the Sacred Calendar and Rites Budget.
- **Military & Combat:** the Triumphal/Victory Banquet Purpose is a natural, real post-triumph event.
- **Public Works & Euergetism:** the Competitive Euergetism Feast Purpose is a direct, concrete application of that document's own competitive-generosity ladder.
- **Masterworks & Unique Crafted Objects:** a Musical Instrument or a Toreutic dinner service both get real, direct use during Entertainment (§7) and the Meal Phase.
- **Wandering Populations:** an Entertainer-type Wanderer's Host engagement is a natural Entertainment source.
- **Companions & Court Positions:** the Archimagirus, Cellarer, Xenodochus, and Symposiarch (as Arbiter Bibendi) are this document's own real, staffed operators across its Phases.
- **Characters:** Confront With Evidence, Reveal a Secret, a betrothal proposal, and a favor request are all natural Comissatio-phase Interactions.
- **Scandal:** an Ostentatious Feast gone wrong, or a badly-handled seating snub against a proud guest, are both real, felt Scandal sources.
- **Dynasty Chronicle:** a Legendary-Quality Feast, a dramatic Comissatio-phase confrontation, or a notable seating controversy are all natural entries.

---

## 10. Data Model

```
Feast extends Activity {                    // §6.47's Activity, type = "feast"
  purpose,                    // "patronageDinner" | "funeralFeast" | "weddingFeast" | "religiousFestival" |
                               // "triumphalBanquet" | "competitiveEuergetism" | "ordinarySocial"
  banquetRecordRef,             // links directly to Food Culture's own BanquetRecord — §5
  seatingAssignments: [
    { guestId, couchPosition, isLocusConsularis: bool, underSeated: bool, overSeated: bool }
  ],
  arbiterBibendiId,              // nullable — the Comissatio phase's own appointed role
  entertainmentRef,               // nullable — §7
}
```

---

## 11. Open Questions

- **All numeric sizing**, per convention — seating-insult/honor magnitude, Entertainment's own Quality contribution, and Purpose-specific default Scale expectations are all unsized.
- **Seating assignment UX.** §4 establishes the mechanic but not whether the game auto-suggests a sensible default seating (per Steward/Council Auto-Management's own general principle) that the player can then adjust, or expects a fully manual assignment every time.
- **Multiple simultaneous under/over-seatings.** §4 doesn't specify how the game resolves a seating chart with several displaced guests at once — whether each is read independently or the whole arrangement is judged as a single composite impression.
- **Comissatio-only Feasts.** Whether a lighter, drinks-only social gathering (skipping the full Meal Phase entirely) should be its own distinct light Feast variant, or is better served by an ordinary Group Interaction instead, isn't resolved here.
