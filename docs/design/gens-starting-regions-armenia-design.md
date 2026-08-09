# GENS — System Design: Starting Regions — Armenia
*The eighth extensible-slate region, and the first genuine departure from this roster's own core assumption: every prior region, however calm or however contested, was Roman territory a household could settle in in some functional sense. Armenia never was, for almost this entire game's own range — it's a real, formally tracked **Contested Buffer** between Rome and Parthia (Cultures of the Known World §9), not a province, and Diplomacy with Non-Roman Peoples already built a real mechanic for its allegiance (§9 of that document, and the existing `ArmenianAllegiance` data model). This document's own actual job turns out to be smaller than it first appears: not inventing a new system, but giving an already-existing one its first real regional home, and building a household's entire life around it.*

---

## Contents

1. Scope & Identity — A Region That Was Never Quite a Province
2. Terrain & Feature Profile
3. Economic Package
4. Political & Legal Texture — Where Ordinary Legal Status Doesn't Apply
5. The Household's Own Two Real Options — Roman Legate Family or Armenian Noble House
6. Great Power Allegiance — This Region's Own Replacement for Reputation Duality
7. Religious & Cultural Defaults
8. Regional Goods & Trade
9. Population & Culture Distribution
10. Gazetteer
11. Rival Seeding
12. Home Anchor
13. Templated Background Flavor
14. Distance & Travel
15. Historical Timeline Hooks — The Most Volatile Political Timeline on the Roster
16. Cross-System Integration
17. Data Model
18. Open Questions

---

## 1. Scope & Identity — A Region That Was Never Quite a Province

Every region built before this one — Sicily, the Alpine Provinces, even Britannia's own not-yet-conquered Dacia sub-area — was either Roman territory already or became Roman territory at a real, dateable point within this game's range. Armenia is different in kind, not just degree: for almost the entirety of the 133 BC – AD 235 span, it was a real, formally independent kingdom whose throne Rome and Parthia both spent centuries installing, deposing, and re-installing client kings to control — a genuine tug-of-war neither side ever definitively won, with one real, brief exception (§15.4). This document names that honestly as a departure from every other region's own premise rather than dressing it up as an ordinary Reputation Duality variant.

The good news, and the reason this document exists at all: Diplomacy with Non-Roman Peoples already tracks Armenia's own contested allegiance as a real, resolvable game state (`ArmenianAllegiance`, per that document's own §13 data model), changeable by covert Scheme or overt campaign. This document's real contribution isn't inventing a new mechanic — it's making that already-existing one the entire spine of a household's life here, the same "give existing content its first concrete regional home" move this project has already made for Ephesus, Cyrene, and Cornish Tin.

---

## 2. Terrain & Feature Profile

Genuinely mountainous, real high-altitude terrain surrounding the real Araxes river valley, with Mount Ararat itself — real, significant to Armenian identity independent of any single religious tradition's own later use of it — anchoring the region's own geography. Real strategic mountain passes connecting the Anatolian plateau to the Mesopotamian lowlands are this region's own actual source of importance; its terrain was never going to support the kind of wealth-generating agriculture or trade-hub economy most other regions on this roster are built around.

---

## 3. Economic Package

*(Qualitative — numeric packages remain Start Modes' own territory, per the framework's §4.2.)* Genuinely the most modest economy on the entire roster, and this document doesn't dress that up — Armenia's real historical importance was strategic and diplomatic, not economic. What agricultural and pastoral wealth exists is real but limited; this region's own one genuine export point of pride is real, well-regarded Armenian horses and cavalry mounts (§8), not land, trade-hub position, or mineral wealth the way almost every other region on this roster is built around.

---

## 4. Political & Legal Texture — Where Ordinary Legal Status Doesn't Apply

A direct, honest statement rather than an attempt to force-fit this region into Legal & Court's usual citizen/Latin-rights/Peregrine framework: **for almost this entire game's own range, Armenia's population simply isn't part of the Roman legal system at all.** It has its own king, its own court, and its own nobility, functioning as a real independent kingdom whose foreign policy Rome and Parthia both leaned on heavily without actually annexing it. The one real exception is Trajan's brief formal annexation (§15.4, AD 114–117) — the single window in this entire game's range where ordinary Roman provincial administration and Legal Status genuinely applied here, before Hadrian's own real, deliberate withdrawal reverted the region to its usual contested-buffer state.

A further real, genuinely interesting detail: Armenia's own royal house was, for long stretches, a branch of Parthia's own Arsacid dynasty — meaning the throne Rome and Parthia both fought over wasn't even a neutral local dynasty, but a real, blood-tied extension of the Parthian royal family itself, part of why Parthia's own claim to influence here was never easy for Rome to simply out-argue.

---

## 5. The Household's Own Two Real Options — Roman Legate Family or Armenian Noble House

Consistent with the framework's own established principle that region and starting culture are chosen independently (Starting Regions §3.1), this document names two genuinely different, both-legitimate ways a household actually lives here, rather than assuming the usual "Roman colonist" default every other region uses:

- **A Roman diplomatic household**, holding or seeking the Legate posting Diplomacy with Non-Roman Peoples already tracks (§8.1 of that document) — the safer, more mechanically straightforward option, since the household's own goals plug directly into that system's existing Peace Treaty, Hostage Exchange, and covert-Scheme machinery without requiring any new design work. This is this document's own recommended default.
- **A native Armenian noble house**, genuinely embedded in the court whose own allegiance is the thing being contested — a real, historically honest alternative, but a genuinely bigger departure from this project's own core premise of household management within a broadly Roman social and administrative world. This document doesn't fully resolve what such a household's own day-to-day life looks like mechanically (§18) — it names the option as real and legitimate rather than pretending the departure isn't there.

---

## 6. Great Power Allegiance — This Region's Own Replacement for Reputation Duality

Armenia doesn't carry Reputation Duality in any of this project's four existing shapes (none, full, tapering, permanent structural, or Syria/the Levant's and the Balkans' own localized combinations) — none of them describe a household whose entire outward-facing concern is which of *two* outside empires currently holds the upper hand, rather than reconciling local standing with a single ruling power. This document names a genuinely new fifth concept instead: **Great Power Allegiance**, built directly on the existing `ArmenianAllegiance` field (Diplomacy with Non-Roman Peoples §13) rather than inventing a competing structure.

The practical shape: a household here tracks standing with **both** Rome and Parthia simultaneously, and the region's own ruling king's allegiance — already a real, live, changeable game state per that document's own §9 — is the actual stage the household's own political life plays out on. A Legate-holding Roman household works to keep that allegiance pointed toward Rome; a native Armenian noble house navigates which patron actually serves its own family's interests better at any given moment, potentially switching sides more than once across a long playthrough, exactly as the real historical kingdom itself did (§15).

---

## 7. Religious & Cultural Defaults

Armenian religious tradition is already tracked directly: a real, Zoroastrian-adjacent practice syncretized with attested local deities including Anahit (Religions of the Known World §5) — this document inherits that entry rather than redefining it. Mount Ararat's own real significance to Armenian identity (§2) sits alongside that religious tradition as a geographic and cultural anchor independent of any single faith's own later claim to the mountain.

---

## 8. Regional Goods & Trade

Armenian horses and cavalry mounts, real-historically well-regarded in antiquity, are this region's own one genuine point of economic pride (§3) — a distinct livestock-and-cavalry identity in the same general register as the Balkans' own Thracian horse-breeding flavor (Starting Regions: The Balkans §8), though arising from a completely different real historical reputation. Beyond that, this document names Armenia's own goods profile honestly as modest rather than inventing texture it doesn't have strong grounding for.

---

## 9. Population & Culture Distribution

| Population | Presence |
|---|---|
| Armenian | Dominant |
| Parthian-descended royal/noble house (per §4's own Arsacid-branch detail) | Common among the nobility specifically, rare in the wider population |
| Roman/Latin (a small diplomatic and, during §15.4's own brief window, administrative presence) | Rare outside that specific historical window |
| Any other population | Rare, individual-level outliers only |

---

## 10. Gazetteer

Deliberately smaller than most regions on this roster, consistent with §3's own honest accounting of this region's real modest scale.

| Location | Role(s) | Tier | Grounding |
|---|---|---|---|
| **Artaxata** | Royal Seat *(new role, see below)*, Sanctuary | Provincial Seat *(highest available tier, despite the non-Roman Role)* | The real Armenian royal capital — sacked by the real Roman general Corbulo in AD 58 (§15.2), rebuilt afterward. |
| **Tigranocerta** | Market Hub | Regional Center | Real, founded by Tigranes the Great before this game's own range opens; besieged by Lucullus in 69 BC (§15.1). |
| **Mount Ararat** | Sanctuary | Outpost | Real, significant to Armenian identity independent of any single religious tradition (§2, §7). |

*(This document introduces **Royal Seat** as a new Gazetteer Role, alongside the framework's own existing Capital and Provincial Seat — a non-Roman monarch's own capital, distinct from a Roman governor's provincial seat since Armenia's own king was never a Roman appointee in the way an ordinary province's governor is. Flagged for the framework document's own future Role table, the same way this project's Capital Role was first introduced by the Italian Heartland document.)*

---

## 11. Rival Seeding

Three houses — deliberately fewer than the usual four, consistent with §10's own honest smaller scale — with two built to directly embody §6's own Great Power Allegiance mechanic.

- **The household of Artavasdes** *(seated at Artaxata)* — a noble house leaning Parthian, carrying a real, recurring Armenian/Arsacid dynastic name used by more than one real historical figure rather than uniquely identifying one, the same safe-because-recurring naming logic already applied to the Balkans' household of Cotys.
- **The household of Vahagni** *(seated near Tigranocerta)* — a noble house leaning Roman, its own name a theophoric echo of Vahagn, a real Armenian war-god, in the same naming spirit Egyptian and other theophoric-pattern cultures already use elsewhere on this roster.
- **Gens Domitia** *(seated at or near Artaxata)* — a Roman diplomatic family holding or seeking the Legate posting across multiple generations, this document's own concrete illustration of §5's own recommended default household.

---

## 12. Home Anchor

**Near Artaxata**, regardless of which of §5's two household types the player chooses — the real seat of whatever government currently holds sway, Roman-aligned or Parthian-aligned, and the natural center of gravity for either the Legate posting or a native noble house's own court life.

---

## 13. Templated Background Flavor

None of Core §5.2's four archetypes were written with this region's own genuinely exceptional premise in mind, and this document doesn't force a connection — consistent with the precedent Sicily and the Alpine Provinces both already set for regions that don't need one. A future Start Modes pass may eventually want a fifth archetype built specifically around Armenia's own Legate-or-noble-house choice, but this document doesn't presume to write that itself.

---

## 14. Distance & Travel

| From | To | Distance Tier | Note |
|---|---|---|---|
| Armenia | Syria | Near | Consistent with Syria's own document naming Zeugma as the primary Euphrates crossing this region sits just beyond. |
| Armenia | Anatolia | Near | Consistent with Greek East's own Cappadocian sub-area and Satala's own proximity to this region. |
| Armenia | Greek East | Moderate | Via Anatolia's own overland route. |
| Armenia | Egypt | Moderate | A real, established eastern Mediterranean-and-overland route. |
| Armenia | The Balkans | Moderate | Via Anatolia's own overland route. |
| Armenia | North African Colony | Far | The longest realistic Mediterranean-adjacent pairing available. |
| Armenia | Latium/Campania, Iberian Colony, Gallic Frontier, Britannia | Far | Consistent with every other eastern-frontier region's own general distance pattern to the far western regions. |

---

## 15. Historical Timeline Hooks — The Most Volatile Political Timeline on the Roster

Unlike every other region's own single (or, at most, doubled) closing event, Armenia's own allegiance genuinely shifts multiple times across this game's range — the Events timeline's own existing entry already describes this as "a recurring rather than single-dated entry across the High Principate and Severan span" (Events: Historical Timeline Content §5). This document names the strongest real hinges directly.

### 15.1 Lucullus's Siege of Tigranocerta (69 BC, closed history by default)

An early real Roman-Armenian military encounter, predating this game's own default range but real, dateable backstory behind Tigranocerta's own Gazetteer entry.

### 15.2 Corbulo's Campaign and the Sack of Artaxata (AD 58, closed history by default)

A real, significant Roman military campaign, led by the real general Corbulo, resulting in Artaxata's own real destruction and later rebuilding.

### 15.3 Tiridates' Coronation by Nero (AD 66, closed history by default)

A real, famous, extravagantly documented diplomatic spectacle: a real Armenian king, himself of Parthian royal blood, traveled to Rome to be crowned personally by the Emperor — a genuine "both sides get to claim victory" resolution, and the single clearest real illustration of §6's own Great Power Allegiance concept in miniature.

### 15.4 Trajan's Brief Annexation (AD 114–117, era-conditional, self-reverting)

The one real window where Armenia was genuinely, formally a Roman province — and, distinctly from every other region's own "map grows" moment (Britannia's Dacia sub-area, for instance), this one **reverts** rather than persisting: Hadrian's own real, deliberate withdrawal restored the ordinary contested-buffer status by AD 117, making this this project's first Timeline Hook whose own structural change is temporary rather than permanent.

### 15.5 The Parthian War of Lucius Verus (AD 161–166, era-conditional)

Already named in the Events timeline as tagged "Parthian/Armenian" — a further real allegiance-contest flashpoint within this game's own range.

---

## 16. Cross-System Integration

- **Starting Regions (framework):** the first region built around a Contested Buffer culture rather than a Provincial, Frontier, Independent, or Great Power one — a genuine new category for this document's own roster, and the third region built entirely outside the framework's original candidate list, following Sicily and the Alpine Provinces. Also introduces the Royal Seat Gazetteer Role (§10), flagged for that document's own future Role table.
- **Diplomacy with Non-Roman Peoples:** this document's own single most direct dependency — §6 builds Great Power Allegiance entirely on that document's existing `ArmenianAllegiance` field and Armenia-resolution mechanics (§9 of that document) rather than duplicating them.
- **Religions of the Known World:** §7 inherits the Armenian Religion entry directly.
- **Cultures of the Known World:** inherits the Armenian Contested Buffer classification directly, and is the first region document to actually build a household's whole premise around that category rather than referencing it as an external relationship (the way Greek East and Syria both do).
- **Companions & Court Positions:** the Legate role (§5, §11) is this document's own concrete illustration of that Companion's actual, lived function rather than an abstract diplomatic office.
- **Legal & Court:** §4 is this project's most direct acknowledgment yet that its own Legal Status framework doesn't universally apply — a genuine, honest gap rather than a forced fit.
- **Events:** §15's own multiply-recurring hinge pattern is a structural first, and §15.4 is this project's first self-reverting (rather than permanent) structural Timeline Hook.

---

## 17. Data Model

```
Region {
  regionId: "armenia",
  status: "extensibleSlate",
  regionCategory: "contestedBuffer",          // a genuine new value, distinct from every other region's own provincial/frontier footing
  ...                                        // inherits full Region schema from Starting Regions §12, with §4's own caveat that
                                              // ordinary Legal Status doesn't apply outside the AD 114–117 window
  reputationDualityMode: "n/a — see GreatPowerAllegiance",
  hasStandingFrontierNeighbor: false,         // not applicable in the ordinary sense — Armenia IS the contested space, not adjacent to it
}

GreatPowerAllegiance {                        // §6 — this document's own new concept, built on Diplomacy's existing ArmenianAllegiance
  householdId,
  standingWithRome,
  standingWithParthia,
  currentRegionalAllegiance,                  // reads directly from Diplomacy with Non-Roman Peoples' own ArmenianAllegiance.currentAllegiance
  householdType: "romanLegateFamily" | "armenianNobleHouse",   // §5 — the two real options this document names
}

RoyalSeat {                                   // §10 — new Gazetteer Role
  locationId: "artaxata",
  role: "royalSeat",
  distinctFrom: ["capital", "provincialSeat"],   // a non-Roman monarch's own seat, not a Roman administrative one
}

TimelineHook {
  hookId: "siegeOfTigranocerta" | "corbuloSacksArtaxata" | "tiridatesCoronation" | "trajanicAnnexation" | "parthianWarOfVerus",
  regionId: "armenia",
  eraConditional: bool,
  selfReverting: bool,                         // true only for trajanicAnnexation — §15.4's own structural first
}
```

---

## 18. Open Questions

- **All numeric sizing**, per this project's standing convention — every Distance Tier in §14 and Great Power Allegiance's own standing-shift mechanics are left to a future balancing pass.
- **The Armenian Noble House option's own mechanical depth (§5).** This document names it as real and legitimate but doesn't resolve what day-to-day household management actually looks like for a family that isn't operating within ordinary Roman administrative or legal structures — genuinely the single biggest open question this document raises, and likely requiring input from Familia, Politics & Patronage, and Legal & Court alike before it could be fully specified.
- **Whether a fifth Templated Background archetype should eventually be built around this region's own choice (§13)** — this document names the gap without attempting to fill it.
- **Whether Great Power Allegiance (§6) should ever generalize beyond Armenia specifically** — this document treats it as a region-specific concept for now, but a future region built around another Contested-Buffer-style culture could plausibly reuse the same underlying shape.
- **Whether this region should carry a stronger, more explicit disclaimer in its own player-facing selection screen**, given how much further it departs from ordinary household management than every other region on the roster — this document flags the departure honestly throughout but doesn't specify how that honesty should actually surface to a player choosing a starting region.
