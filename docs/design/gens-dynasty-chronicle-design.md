# GENS — System Design: Dynasty Chronicle (§6.11)
*The single most-referenced undesigned system in this project — nearly every document built so far has flagged a specific moment as "Chronicle-worthy" without this document existing to catch it. This is where all of that actually lands, organized, tiered, and readable as an actual household record rather than a raw event log.*

---

## Contents

1. Scope & Role
2. Entry Anatomy
3. Significance Tiers
4. Organization — Chapters & Filters
5. Milestones — The Chronicle as Goal-Tracker
6. Generation & Curation
7. Player Annotation & Pinning
8. The Chronicle as Diegetic Object
9. Cross-House Chronicle — Rival Entries
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "an in-fiction, readable record of the gens's history: births, deaths, marriages, scandals, offices held, wars fought, buildings raised, generational transitions." Design Pillar #7 states the commitment directly: "Memory has weight. The dynasty's own history is a first-class, readable feature — a game about lineage should let you read your lineage." And the Visual Identity section already named the deeper reason this document matters: "the game's UI and its own diegetic record-keeping are the same object" — the Chronicle isn't a menu bolted onto the simulation, it's the same kind of artifact the rest of the interface already pretends to be.

This document does two things: it defines the shared entry format, significance tiers, and browsing structure every other system's "this is a Chronicle entry" reference writes into — and, per the decision to treat them as the same object, it's also where the core doc's separately-named "optional milestone catalog" actually lives.

---

## 2. Entry Anatomy

Every entry carries:

- **A prose line** — one to three sentences, written in an annal/household-record voice rather than a system log ("In the consulship of Lucius Piso, Marcus Aemilius wed Julia Prisca of the Cornelii, sealing an alliance long sought"), not a bare stat delta.
- **A category** — Births & Deaths, Marriages & Family, Politics & Office, War & Combat, Wealth & Building, Faith & Scandal, or an open-ended catch-all for whatever a future system needs.
- **A significance tier** (§3).
- **A date** (month/year, per Familia's monthly tick).
- **Linked Characters** — whoever the entry is actually about.
- **A source reference** — which system actually generated it, for traceability.
- **A milestone flag** (§5).

---

## 3. Significance Tiers

Per the decision to log comprehensively rather than only the headline moments, with tiers doing the actual curation work:

| Tier | What qualifies | Default visibility |
|---|---|---|
| **Minor** | An ordinary birth, a routine promotion, a small contract, a passing relationship shift | Logged, but filtered out of the default read |
| **Notable** | A marriage, a moderate office won, a building completed, an ordinary Squad engagement | Visible by default |
| **Major** | A significant death, a triumph, a public scandal, a war's outcome, a Feud's resolution, an Insolvency's Fall of the House entry (Economy & Finance §9) | Always visible, given real prose weight |
| **Legendary** | A Consulship or triumph-worthy command, a dynasty's founding or extinction, a splinter house born from a lost succession claim, the battlefield death of a player-character or a commanding officer (Praefectus, Tribune, Legate) | Always visible, gets the richest treatment (§8) |

Every entry gets a tier at generation time (§6); the player reads whichever slice they actually want via §4's filtering, rather than the Chronicle deciding for them what's worth seeing.

---

## 4. Organization — Chapters & Filters

Per the decision to combine both: the default read is **generational chapters** — each head's tenure is its own chapter, titled by name and dates ("The Age of Gaius Cornelius, 34 BCE – 12 CE"), opening with a short summary of how they came to hold the position and containing every entry from their reign in chronological order. Layered on top, a **category filter** cuts across every chapter at once — "show me every war this dynasty has ever fought," regardless of which generation fought it — for a themed read rather than a strictly linear one. Neither view is the "real" one; they're the same underlying entries read two different ways.

---

## 5. Milestones — The Chronicle as Goal-Tracker

Per the decision to treat the milestone catalog and the Chronicle as one object: a player can pre-mark an aspiration — reach the Consulship, build a Fortress, found a second settlement, survive ten generations — as a tracked **Milestone** before it's ever actually happened. This creates a real, visually distinct entry in the Chronicle immediately: a pending, aspirational placeholder rather than a completed record. The moment the underlying event actually occurs, that placeholder resolves into a genuine, highlighted entry at whatever tier the achievement actually earns (usually Major or Legendary). Unachieved Milestones stay visible as a standing aspirations list the player can review, abandon, or add to at any time — giving Design Pillar #2's "self-set objectives... for structure" a concrete, in-fiction mechanism rather than a separate checklist UI sitting outside the simulation's own record.

---

## 6. Generation & Curation

This document doesn't invent new triggers — every system that already flagged something as "Chronicle-worthy" throughout this project's design is the actual generation source; this document just defines the shared format and tier-assignment rule those systems write into. A rough default mapping, so tier assignment isn't ad hoc per system:

- **Legendary by default:** a Consulship/triumph, dynasty extinction, a splinter house founded from a lost succession claim, the battlefield death of a player-character or a commanding officer.
- **Major by default:** any other death of a named Character (including an ordinary Squad member lost in an engagement), a local magistracy won, a completed Monument, a Feud's resolution, a Fall of the House (Insolvency), a discovered-and-escalated Scheme.
- **Notable by default:** a marriage, a birth of an eventual heir candidate, a completed civic building, an ordinary military engagement's outcome.
- **Minor by default:** everything else that's still worth logging — routine births, small contracts, minor promotions.

Individual systems can override this default where their own context warrants (a "routine" marriage that happens to seal a major alliance can post as Major instead of Notable), but the mapping above is the shared starting point rather than each system inventing its own scale.

---

## 7. Player Annotation & Pinning

Two lightweight, purely personal tools sitting on top of the generated record:

- **Pinning** — the player can elevate any entry's personal visibility regardless of its assigned tier, without changing what any other system reads that tier as. A Minor-tier birth the player just personally cares about can be pinned to always show up in their own default read.
- **Annotation** — a free-text note the player can attach to any entry ("This is the day I decided never to trust the Cornelii again"), and, separately, a lightweight **personal note** entry type the player can add directly — not generated by any system, a pure diary-style addition — clearly distinguished from system-generated entries (§11's `source` field) so the two never get confused with each other.

---

## 8. The Chronicle as Diegetic Object

Directly realizing the Visual Identity section's own framing: the Chronicle is rendered as an actual illuminated or painted household record — the same wax-tablet, painted-ledger, inscribed-stone register of textures the rest of the interface already commits to — not a conventional scrolling feed. A Legendary-tier entry earns a small illustrative flourish (a tiny painted vignette or heraldic-style motif, the same period-appropriate rendering language already established for portraits) rather than reading identically to a Minor one just filtered up a level — the Chronicle's own visual weight should track its narrative weight.

---

## 9. Cross-House Chronicle — Rival Entries

Rival Houses §7 already names "recent Chronicle entries involving them" as part of a House of Note's own Dossier — this is that mechanism. A House of Note maintains its own lightweight Chronicle, generally populated at Major/Legendary tier only (their minutiae was never being tracked in the first place, per that document's own tiering). Any entry that genuinely involves both the player's house and a rival's — a marriage alliance, a Feud engagement, a splinter house's founding — posts to **both** Chronicles simultaneously, cross-linked, each told from its own house's natural vantage ("we triumphed" reading rather differently from "they suffered a grievous defeat" for the same underlying engagement) without requiring two entirely separate narrative-generation passes to achieve it.

---

## 10. Cross-System Integration

- **Succession & Dynasty:** every handoff, contested-succession resolution, splinter house founding, and extinction (§7 of that doc) is a direct, named entry source — that document's own closing "Chronicle compiles a genuine closing account" line is fulfilled here.
- **Rival Houses:** rival-vs-rival dynamics, absorption, and extinction are all named material for this document's pool; §9 above is the concrete mechanism behind that document's own Dossier reference.
- **Military & Combat:** a triumph, a lost standard, a captured/ransomed commander, and a Catastrophic Defeat were all explicitly flagged as Chronicle material by that document.
- **Politics & Patronage:** every cursus honorum rung and a won or lost contested election are named directly as "exactly the milestone-catalog material" this document now formally owns.
- **Economy & Finance:** the Fall of the House entry (§9 of that doc) is a direct, named Legendary-tier source.
- **Familia:** births, deaths, marriages, and divorces are the base layer this whole document rests on.
- **Companions & Court Positions:** the promotion ladder is explicitly "framed as Chronicle-worthy" in that document's own language.
- **Villa / Buildings' Monuments (§6.23):** a completed Monument is named directly as producing "Dignitas and Chronicle entries," and can become a landmark rival houses visibly react to — itself further Chronicle material.
- **Characters / Traits:** a major Reactive trait shift (Broken, Corrupt, Ruthless) and a discovered-and-escalated Scheme are natural Notable/Major entries; a Combo Title shift is a nice optional flourish in an entry's own prose.
- **Games & Spectacle (§6.22, future) / Events (§6.8, future):** both are natural future contributors to this pool rather than needing their own separate record.

---

## 11. Data Model

```
ChronicleEntry {
  entryId, gensId,
  month,
  category,          // "birthsAndDeaths" | "marriagesAndFamily" | "politicsAndOffice" |
                       // "warAndCombat" | "wealthAndBuilding" | "faithAndScandal" | "other"
  tier,              // "minor" | "notable" | "major" | "legendary"
  prose,
  linkedCharacterIds: [...],
  sourceSystem,        // which document/system generated this
  source,             // "system" | "playerNote" — §7's distinction
  isMilestone: bool,
  milestoneStatus,      // "pending" | "achieved" | null (non-milestone entries)
  pinned: bool,
  playerAnnotation,      // free text, null unless the player added one
  crossHouseLinkedEntryId,  // §9 — set only when this entry also posted to a rival house's own Chronicle
}

GenerationalChapter {
  gensId, headCharacterId,
  startMonth, endMonth,
  chapterSummary,
}
```

---

## 12. Open Questions

- **All numeric sizing.** Consistent with this project's convention: exact tier thresholds and how strongly an individual system's override can deviate from §6's default mapping are unsized.
- **Illustrated flourish generation.** §8 establishes Legendary entries earn a visual flourish without specifying whether these are hand-authored per entry type or procedurally assembled.
- **Cross-house prose divergence depth.** §9 gestures at each house's Chronicle reading its shared entries from its own vantage; how different that prose actually needs to be, or whether a lighter shared-template approach is more practical, isn't decided.
- **Milestone catalog's starting content.** §5 doesn't specify whether the game offers a suggested starter set of common Milestones (reach the Senate, found a second settlement) or leaves the list entirely player-authored from a blank state.
- **Chronicle length management.** Over a genuinely long playthrough (ten generations, per the core doc's own stated possibility), whether the full Minor-tier record needs any pruning/archiving beyond simple filtering isn't addressed.
