# GENS — System Design: Events (§6.8)
*The shared engine nearly every other document has already been quietly plugging into: Religion's Omens, Natural Disasters' hazard rolls, Travel's en-route Events, Characters' Scheme discovery beats, Rival Houses' living-world texture, Settlement Demographics' migration spikes, and Succession & Dynasty's own disputes are all, mechanically, Events. This final pass closes out the Wider Empire layer completely: a real, bounded historical range (133 BC – AD 235, deliberately bookended on the Gracchi at one end), a concrete era breakdown, a curated table of illustrative Starting Years, an explicit statement that player foreknowledge of real history is a feature this design leans into rather than something to protect against, and a fully resolved Divergence frequency commitment. What was five open structural questions is now two — both genuinely just authoring/implementation work, not design gaps.*

---

## Contents

1. Scope & Role
2. The Event Taxonomy — Four Scopes
3. The Weighted Event Pool — How Selection Works
4. Scripted vs. Random Triggers
5. Prominence — The Household's Own Volume Knob on the Wider World
6. The Wider Empire — Imperial Events, Real History & Two-Way Ripple
7. The Monthly Report — Delivery, Automation & Player Agency
8. Event Chains & Multi-Stage Events
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "monthly-tick-triggered random and scripted events weighted by current stats/traits/relationships. The wider Roman Empire (wars, emperors, edicts, governors) intrudes regularly and meaningfully, occasionally rippling from the player's own choices outward." Nearly every document written since has named this system directly as "(Events §6.8, future)" for its own content — this document pays that debt with one shared spine, and now, with this pass, a fully closed-out historical backbone underneath it.

Three jobs:

- **The Engine** (§2–4, §7–8) — taxonomy, weighting, trigger types, delivery, chaining. Pure infrastructure other systems plug their own event content into.
- **The Wider Empire, Generically** (§5, §6.1) — the tiered ripple system from this document's first pass: ambient flavor, real mechanical effects, and rare full-drama entanglement, gated by a tracked Prominence score.
- **The Wider Empire, Historically** (§6.2–6.7) — a real Game Calendar with a bounded, deliberately-chosen historical range, a curated timeline of actual Roman events firing on their real dates, real historical figures who shape that timeline without ever becoming interactive Characters, and the rare, now fully-specified mechanism by which a household important enough can knock history off its real course.

---

## 2. The Event Taxonomy — Four Scopes

| Scope | Targets | Representative existing content |
|---|---|---|
| **Personal** | A single named Character | Characters' Formative-stage trait events, Romance & Seduction beats, a Scheme's Discovery moment, a Disease exposure roll, a Travel en-route Encounter |
| **Household** | The whole estate as a unit | A Natural Disaster Event, a Religion Omen, a Labor & Slavery Unrest flare-up, a Legal & Court case landing, a Succession Dispute trigger |
| **Settlement** | The wider background population | Settlement Demographics migration waves, a Piracy & Banditry raid, a local Curia shakeup |
| **Imperial** | The wider Roman world | §6 |

Every existing system's own event content already sorts cleanly into one of the first three scopes without changing anything about how that content works — this taxonomy is a reading frame, not a redesign.

---

## 3. The Weighted Event Pool

Every event definition registers into one shared pool carrying its **scope** (§2), **eligibility conditions**, and **weight inputs** drawn from whichever hidden meters this project has already built — Exposure scores, Faction lean, Doctrine Affinity, Piety tier, Loyalty, Contentment, Legal Status, Prominence (§5). Nothing here invents a new weight input; this document formalizes that they all feed one shared selection mechanism rather than each system rolling its own separate dice. Each monthly tick, the pool resolves at scope-appropriate frequency: Personal rolls per eligible Character, Household once for the estate, Settlement once for the background population, Imperial on its own slower cadence (§6).

---

## 4. Scripted vs. Random Triggers

- **Random Events** draw from the weighted pool per §3 — no guarantee, purely probabilistic.
- **Scripted Events** fire deterministically the instant a specific condition is met, bypassing the weighted roll entirely — a Household Doctrine reaching Defining or Apex, a Succession transition, a settlement reaching City stage, a Character's Adolescence-to-Adulthood transition, and every dated entry on the Historical Timeline (§6.3). A Scripted Event is queued to the very next Monthly Report tick, keeping every trigger landing through the same predictable channel.

---

## 5. Prominence — The Household's Own Volume Knob on the Wider World

Unchanged: a tracked household-level score, distinct from Dignitas — Dignitas measures how well-regarded a household is, Prominence measures how much the wider political world has noticed it exists at all. This is the formal definition behind Succession & Dynasty's own `prominenceScaling` trigger reason. It reads Dignitas magnitude, cursus honorum/provincial office, Net Worth scale, a Doctrine at Defining or Apex, a Roman military commission, and a Proscription or Land Redistribution's demonstration effect. It governs Tier 3 personal address (§6.1) and upward ripple, up to and including Divergence (§6.6).

---

## 6. The Wider Empire — Imperial Events, Real History & Two-Way Ripple

### 6.1 The Three Tiers *(recap)*

**Tier 1 — Ambient Flavor:** pure texture, no mechanical effect. **Tier 2 — Real Mechanical Ripple** *(the default weight)*: genuine effects reaching every household — an Emperor's succession, a declared war raising recruitment demand, a new Governor, a currency reform, a Rome-issued proscription list. **Tier 3 — Full Drama** *(rarer, Prominence-gated)*: a personal summons to Rome, a direct cursus honorum sponsorship offer, or a genuine civil-war-adjacent allegiance choice.

### 6.2 The Game Calendar — A Real, Bounded Historical Range

Every playthrough runs against a real Roman calendar date. The supported range is now fixed rather than left open-ended: **133 BC – AD 235**, deliberately bookended for real thematic reasons rather than arbitrary round numbers. The opening year is Tiberius Gracchus's own tribunate and land reform — the exact real event Policies & Edicts' own Land Redistribution Edict (§5.4 of that doc) is modeled on — meaning a player who starts at the earliest possible date can, in principle, watch the actual historical moment their own Edict system quotes as its inspiration unfold as a dated Timeline entry. The closing year is the end of the Severan dynasty, the last coherent, stable imperial succession before the genuine chaos of the Crisis of the Third Century — a real, sensible place to stop rather than trying to model Rome's messiest and least-settled stretch inside a game about one household's own stability and continuity.

Within that range, four **eras** are tracked, each carrying a real, distinct political and religious texture other documents can read directly:

| Era | Years | Texture |
|---|---|---|
| **Late Republic** | 133 BC – 27 BC | Senate-dominant, no Emperor at all — the cursus honorum's Consulship is the genuine peak of power, not a title an Emperor grants |
| **Early Principate** | 27 BC – AD 96 | Augustus through the Flavians — the Emperor exists, the Senate still matters, Reputation Duality's frontier framing is at its most active as the Empire is still expanding |
| **High Principate** | AD 96 – AD 192 | Nerva through Commodus, the so-called "Five Good Emperors" era — the Empire at its most stable and its largest territorial extent |
| **Severan** | AD 193 – AD 235 | A real, felt uptick in succession instability and military influence over the throne — a noticeably higher baseline for Tier 2 Imperial succession-related Events than the calmer High Principate |

At Start Mode, a Full Custom or Templated Backgrounds start lets the player pick any year in range directly; a Scenario Start with its own built-in historical flavor (a "newly settled veteran colony," for instance) auto-suggests the specific real year that flavor actually corresponds to — a post-Actium veteran settlement wave suggests 30 BC, for instance, rather than leaving the player to guess which year matches the scenario they picked; a Randomized start draws uniformly across the full range unless the player has also picked a specific region whose own flavor narrows it (a frontier-province Randomized start won't land in a year before Rome held that frontier at all).

**Illustrative Starting Years** — a curated handful worth naming directly, each chosen because it lands the player at a real, evocative moment with a direct tie to an existing system:

| Year | Moment | Why it resonates |
|---|---|---|
| **133 BC** | Tiberius Gracchus's land reform | The literal real event behind Policies & Edicts' own Land Redistribution Edict — start here to watch the original unfold |
| **63 BC** | Cicero's consulship, the Catiline conspiracy | A real debt-and-conspiracy crisis — a natural thematic partner to Policies & Edicts' Tabulae Novae |
| **44 BC** | Caesar's assassination | A Republic in genuine, immediate crisis — Succession & Dynasty's own contested-claimant drama has no more apt real backdrop |
| **AD 64** | Four years before the Great Fire of Rome | A slow-burning countdown to a dated Timeline entry the player knows is coming |
| **AD 79** | The year Vesuvius erupts | Starting in this exact year means the eruption is essentially immediate for a Campania-adjacent household, not a distant future entry |
| **AD 117** | Trajan's death, the Empire at its largest | A high-water-mark start — everything from here reads as maintaining a peak rather than building toward one |
| **AD 180** | The death of Marcus Aurelius | The traditional (if debated) marker for "the end of Rome's golden age" — a start defined by knowing the easy years are already over |

### 6.3 Player Foreknowledge Is a Feature, Not a Bug

Worth stating explicitly rather than leaving implicit: a player who knows Nero eventually succeeds Claudius, or that AD 79 means Vesuvius, is meant to feel that knowledge as part of the game's own appeal — the same way Crusader Kings' players brace for the Mongols or a Victoria player watches for a railway boom they know is coming. This document doesn't try to obscure or randomize away real historical knowledge to preserve surprise; a savvy player deliberately starting in 78 AD specifically to have one year to prepare a Campania estate before Vesuvius, or founding a household in 132 AD specifically to watch it hold together (or not) through the harder Severan years ahead, is playing the game exactly as intended.

### 6.4 The Historical Timeline — Real Events, Real Dates

A curated, chronologically ordered list of real events, each locked to its real date within the 133 BC – AD 235 range, layered underneath the probabilistic Tier 1–3 pool. Once a Starting Year is set, every entry after it becomes a real, dated Scripted Event (§4); everything before it is already history by the time play begins and simply never fires.

- **A real Emperor's accession and death**, on real dates, resolving as Tier 2 Events exactly as §6.1 describes — now dated rather than randomly timed.
- **A real, named war or provincial revolt**, firing where a playthrough's region and year genuinely line up with one; everywhere else, the existing generic randomized Tier 2 war-Event fills the gap. Real where it fits, generic elsewhere — this document never force-fits history onto a mismatched setting.
- **A real natural disaster on its real date** — Vesuvius's AD 79 eruption directly resolves Natural Disasters' own previously-open question. **Worth clarifying explicitly:** this dated entry doesn't replace that document's own generic, low-frequency Dormant Volcano hazard (§2.2 of that doc) — a Campania-adjacent household still carries that ordinary, rare possibility every year regardless of the real date, and AD 79 is simply a guaranteed, historically exact addition layered on top for any household whose calendar actually reaches it, not a substitute for the standing mechanic.
- **A real, rare religious observance** — the *Ludi Saeculares*, historically held only a handful of times across the entire range, exactly the kind of genuinely rare, dated event no random weighted roll would ever produce on its own.

### 6.5 Named Historical Figures — Backdrop, Not Characters

Real historical figures — every real head of state across the 133 BC – AD 235 range (the late Republic's dominant consuls and triumvirs, then every Emperor through Severus Alexander), plus a curated, non-exhaustive supplementary roster of the era's other most genuinely famous figures (Cicero and Cato for the Late Republic; Agrippa and Sejanus for the Early Principate, and so on) — drive the Historical Timeline's content by name, but are never instantiated as full, interactive Characters. The player cannot duel, marry, seduce, or run a Scheme directly against any of them.

Two reasons: **realism** — a private provincial household, however Prominent, realistically never gets personally close enough to a sitting Emperor for the Interaction Catalog to make sense applied to him directly, and reads about him in dispatches instead, exactly as §6.1's Tier 2 already models — and a **clean, deliberate line** against ever needing to fabricate actions, quotes, or private moments for a real individual. A Named Historical Figure record (§10) tracks only real, documented biographical facts, used purely to drive which Timeline entries fire and how they're flavored. A governor or legate the household *does* deal with directly is always this project's own generated Character filling that real office — a real person in a real role, not a specific documented individual history remembers by name, mirroring the real, uneven way history actually preserves names.

### 6.6 Real Names, Fictional Lives — Rival Houses and Historical Flavor

An optional flavor layer: Rival Houses can be seeded with real, historically-attested *gens* names — Cornelia, Julia, Claudia, Fabia — for period texture, without implying that a given house's own generated members are the historical individuals who shared that surname. Real names, invented lives, never claiming otherwise.

### 6.7 Divergence — When the Timeline Actually Branches

The release valve for Design Pillar #6's "living world" promise against an otherwise-fixed historical backbone. A sufficiently Prominent household (§5) whose own upward-rippling action crosses a real severity threshold — a consequential Edict, a Doctrine reaching Apex, landing on the winning or losing side of a Tier 3 civil-war allegiance choice — can cause the Timeline to genuinely branch from that point forward: a different claimant wins, a war resolves differently, a figure who should have died on schedule doesn't. Once Diverged, that thread stops drawing on the real historical roster and the game generates its own alternate-history content going forward.

**Frequency, now a real design commitment rather than an open question:** the overwhelming majority of playthroughs should see **zero** Divergence events across their entire length — this is the real, unaltered historical backbone remaining the default experience by a wide margin. A genuinely Prominent, actively-engaged playthrough might earn **one** across a full campaign, treated as that dynasty's single defining, legacy-making moment. More than one in the same playthrough should be vanishingly rare, reserved for a household so thoroughly dominant it's arguably stopped playing within history and started making it. Every Divergence, without exception, is an automatic maximum-tier Dynasty Chronicle entry — the kind of thing that single-handedly defines what a later reader of that dynasty's Chronicle understands the whole playthrough to have been about.

**Cross-playthrough consistency:** by design, two different households starting in the same real year see an identical Historical Timeline by default — a player who starts in 44 BC always sees Caesar's real assassination on schedule unless their *own* actions changed it. This is deliberate rather than a missed opportunity for variety: predictability is what makes Divergence *mean* something. Real, learnable history is the stable floor; Divergence is the rare, earned exception layered on top of it, and diluting the floor with artificial randomization would only cheapen the exception.

### 6.8 Downward and Upward Ripple

Unchanged: **downward** (Empire → Household) is the default and doesn't require Prominence — Tier 2 effects and dated Timeline entries reach every household regardless of scale. **Upward** (Household → Empire) stays Prominence-gated, with Divergence (§6.7) as its rarest, most consequential possible expression.

---

## 7. The Monthly Report — Delivery, Automation & Player Agency

Unchanged: **Auto-Resolved** for the large majority of Personal/Settlement Events and Tier 1 flavor; **Flagged for Choice** for Household-scope Events and any significant Personal, Tier 3, or Divergence-triggering moment; a genuine, per-category **Manual Mode** opt-in. A dated Historical Timeline entry is always at minimum an Auto-Resolved digest line, and anything Chronicle-worthy is treated identically whether it came from the random pool or the real historical record.

---

## 8. Event Chains & Multi-Stage Events

Unchanged: Characters' Scheme engine and Natural Disasters' Compounding Hazards remain the two existing chaining models this document's delivery layer surfaces across successive Reports. A multi-year real historical arc (a war's actual historical course, a full Imperial reign) uses the identical chained-delivery pattern.

---

## 9. Cross-System Integration

- **Religion:** Omens read Divine Displeasure; the *Ludi Saeculares* is a concrete dated Timeline entry that system can hook its own festival calendar to.
- **Natural Disasters:** Vesuvius's real AD 79 date resolves that document's own previously-flagged open question, layered on top of (not replacing) its standing Dormant Volcano mechanic.
- **Policies & Edicts:** 133 BC's own Gracchi-era Timeline opening is the literal real event Land Redistribution is modeled on; Tabulae Novae's own real-world root event (63 BC, Catiline) is a named Illustrative Starting Year.
- **Settlement Demographics, Characters, Travel, Succession & Dynasty, Rival Houses, Politics & Patronage, Military & Combat, Economy & Finance, Dynasty Chronicle:** unchanged from prior passes' integration.
- **Legal & Court:** a real historical law or trial referenced on the Timeline is natural precedent-flavor for Rulings, without mechanically binding them.
- **Correspondence & Letters (§6.27, future):** the natural channel for a distant Timeline development reaching a household not positioned to receive it through the ambient Report alone.
- **Start Mode (Core doc):** the Starting Year selection, its era breakdown, and Scenario Start's auto-suggested year are this document's own concrete extension of Full Custom, Templated Backgrounds, Randomized, and Scenario options.

---

## 10. Data Model

```
GameCalendar {
  householdId,
  startingYear,                    // real BC/AD year within 133 BC – AD 235
  currentYear, currentMonth,
  era,                              // "lateRepublic" | "earlyPrincipate" | "highPrincipate" | "severan"
}

HistoricalTimelineEntry {
  entryId, realYear, realMonth,
  eventType,                        // "imperialSuccession" | "warOrRevolt" | "naturalDisaster" |
                                     // "religiousObservance" | "politicalTrial" | "other"
  realWorldName,                     // e.g. "Vesuvius Eruption", "Great Fire of Rome", "Ludi Saeculares"
  regionRelevance: [ ... ],
  involvedFigureIds: [ ... ],
  linkedEventDefinitionRef,
  divergenceState,                    // "onTrack" | "diverged" | "notYetReached" | "predatesStart"
}

NamedHistoricalFigure {
  figureId, realName, role,           // "headOfState" | "general" | "senator" | "governor" | "other"
  realAccessionOrStartYear, realDeathOrEndYear,
  currentStatus,                       // "aliveOnTrack" | "deceasedOnSchedule" | "survivedPastRealDate" (Divergence only)
}

RivalHouseHistoricalFlavor {
  rivalHouseId, seededGensName,        // flavor only — §6.6
}

DivergenceRecord {
  divergenceId, month, triggeringHouseholdId,
  triggeringAction,
  affectedTimelineEntryIds: [ ... ],
  newAlternateHistoryBranchActive: bool,
  chronicleEntryTier,                  // always maximum tier — §6.7
}

// EventDefinition, EventInstance, ProminenceScore, and MonthlyReport are unchanged from this document's first pass.
```

---

## 11. Open Questions

Down to two genuine ones — everything else this document previously flagged (the range's bookend years, the Named Historical Figure roster's scope, Divergence's frequency target, and cross-playthrough consistency) is now a resolved design commitment rather than an open question:

- **The full Historical Timeline and Named Historical Figure roster.** §6.4–6.5 establish the mechanism, the bounded range, and illustrative examples; authoring the complete, dated catalog spanning all 368 years is substantial, real content work — a production task, not a remaining structural design question.
- **Divergence's exact severity threshold and downstream authoring burden.** §6.7 commits to a real frequency target (zero for most, one for a genuinely Prominent playthrough, more only in exceptional cases) and to generating fresh alternate-history content once a thread branches; the precise numeric threshold and how much of that alternate content needs pre-authored contingency versus procedural generation at the moment of Divergence remain open, consistent with this project's standing convention of deferring numeric balancing to a later pass.
