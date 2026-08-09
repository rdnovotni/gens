# GENS — System Design: Books & Manuscripts (§6.44, new)
*Expansion and polish pass. A real gap sitting directly beside several finished systems: the Bibliotheca and public Library both exist as passive, aggregate training-speed buildings; Historian, Poet, Playwright, Philosopher, Theologian, Legal Scholar, Genealogist, Astrologer, Naturalist, and Cartographer all exist as Traits carrying only a vague "flavor bonus"; Literary Patronage exists as a Cultural Prestige generator with no actual deliverable; Institutions of Renown named Alexandria's real Library and Museum without ever giving the player an object to actually take from it. This document is the CK3-artifact, Dwarf-Fortress-book treatment all of that groundwork was waiting for: real, named, individually-tracked Works and Copies, with genuine authorship, genuine physical scarcity, genuine provenance, and a genuine, honest possibility of being lost forever. This pass adds a real Fragmentary Survival state between "extant" and "lost," a distinct Autograph/Master Copy concept, a real Forgery & Misattribution mechanic, functional Maps as their own specialized Copy type, Translation as a real cross-language action, and civic Library donation as a genuine euergetism option.*

---

## Contents

1. Scope & Role — Real Objects, Not Just a Training-Speed Bonus
2. The Work and the Copy — Two Real, Distinct Things
3. Authorship — Where a Work Actually Comes From
4. Content Categories
5. Copying, the Autograph, and Translation
6. Provenance — A Copy's Own History
7. Acquisition
8. Loss, Fragments, and the Lost Work
9. Forgery & Misattribution
10. Maps — A Specialized, Functional Copy
11. The Household Library — Collecting, and Public Donation
12. Reading — Using a Book, Not Just Owning One
13. Cross-System Integration
14. Data Model
15. Open Questions

---

## 1. Scope & Role — Real Objects, Not Just a Training-Speed Bonus

The Bibliotheca (Villa §4.5) and the public Library/Bibliotheca (Buildings §4.10) stay exactly as designed — a passive, aggregate Education/Learning-training-speed building, consuming generic Writing Tablets or Parchment as ongoing upkeep. This document doesn't touch that mechanic at all. What it adds is what could actually sit on those shelves: individual, named, real objects — a specific poem, a specific family history, a specific philosophical treatise, a specific hand-drawn map — each with its own authorship, its own physical copies, its own ownership history, and its own real, honest chance of being lost to time forever. A Bibliotheca holding zero individually-tracked Books works exactly as before; this document is an optional, deepening layer for a household that wants its library to be a real place with a real story, not a requirement placed on every playthrough.

---

## 2. The Work and the Copy — Two Real, Distinct Things

The single structural idea this whole document is built on, borrowed deliberately from how Dwarf Fortress treats written knowledge and how CK3 treats a named artifact, fused into one model:

- **A Work** is the actual intellectual content — a specific poem, history, treatise, play, philosophical dialogue, or map — authored once, by a specific Character, at a specific time and place. The Work itself is not a physical object; it's closer to this project's own Dynasty Chronicle entry than to a Resources & Goods commodity — a real, singular act of creation.
- **A Copy** is a physical instantiation of a Work — a specific set of Writing Tablets, a specific Parchment codex, a specific Papyrus scroll, hand-copied by a scribe. Because this era has no printing, every Copy is a genuinely separate physical object with its own independent Condition, Copy Quality, and provenance, even though its textual content is identical to every other Copy of the same Work. A Work can have many Copies, one Copy, or — the honest, sobering case this document builds real stakes around — zero.

**The load-bearing consequence:** a Work survives exactly as long as at least one of its Copies does, anywhere. Losing one Copy is a real loss, but not necessarily an ending. Losing the *last* Copy is (§8).

---

## 3. Authorship — Where a Work Actually Comes From

The direct, concrete payoff this document finally gives ten existing Traits that have carried nothing but a vague flavor bonus since they were written: **Historian, Poet, Playwright, Philosopher, Theologian, Legal Scholar, Genealogist, Astrologer, Naturalist,** and **Cartographer**. A Character holding one of these Traits, given real time and a physical venue (the household's own Private Scriptorium or Bibliotheca, or the settlement's public equivalents), can undertake **Authorship** — a genuine, deliberate, extended project producing a brand-new Work, its Content Category (§4) matching the authoring Trait directly.

A Character without a matching Trait isn't locked out — anyone sufficiently literate (Language & Literacy) can attempt a modest personal Work (a memoir, a simple household treatise) — but the relevant Trait is what raises both the resulting Work's own quality ceiling and the odds it rises to real Notable or Renowned significance (§6.2) rather than staying a private, unremarkable curiosity. This is deliberately the same permissive-but-scaled shape this project already uses everywhere else: participation is open, but the specialist does it better.

---

## 4. Content Categories

| Category | Authoring Trait | Real Grounding | Natural Destination |
|---|---|---|---|
| **History/Chronicle** | Historian | A household's own dramatized, literary account of its family history | A direct, literal excerpt or companion piece to Dynasty Chronicle |
| **Poetry/Verse** | Poet | — | Cultural Patronage, gift-giving |
| **Drama** | Playwright | Real Roman comedy and tragedy both circulated as genuinely distinct traditions | Games & Spectacle's Theatre |
| **Philosophy** | Philosopher | Real Stoic/Epicurean schools, per Food Culture §5's own framing | Education & Culture's Philosophy Track |
| **Theology/Religious** | Theologian | — | Religion |
| **Legal Commentary** | Legal Scholar | A real, well-attested ancient genre — legal treatises and commentaries genuinely circulated and were genuinely cited | Legal & Court's case-argument mechanics |
| **Genealogical Record** | Genealogist | — | Succession & Dynasty, Dynasty Chronicle |
| **Astrological/Astronomical Treatise** | Astrologer | A real, extensively practiced ancient discipline, genuinely central to how many Romans read Omens | Religion's Omens & Auspices |
| **Naturalist/Bestiary Work** | Naturalist | A real, well-attested ancient genre cataloguing plants, animals, and natural phenomena | Villa's Menagerie/Aviary, Resources & Goods flavor |
| **Map/Geography** | Cartographer | See §10 — the one content category with its own dedicated functional mechanic beyond Reading | Travel, Military & Combat, Espionage |
| **Technical/Practical Treatise** | *(any sufficiently Learned Character)* | A real, well-attested ancient genre — practical handbooks on agriculture, medicine, or military affairs genuinely circulated in antiquity | Estate & Settlement, Disease & Public Health, or Military & Combat flavor, depending on subject |
| **Correspondence Collection** | *(any Character with a substantial Correspondence & Letters history)* | A real, historically attested practice — a notable figure's own letters compiled and circulated as a real literary work in their own right | Correspondence & Letters, Dynasty Chronicle |
| **Culinary Treatise** *(new)* | *(a Character with the Gourmet Trait or a renowned Archimagirus, per Food Culture §6)* | A real, attested ancient genre — culinary and household-management handbooks genuinely circulated | Food Culture's Named Cook mechanic |

---

## 5. Copying, the Autograph, and Translation

A new Copy of an existing Work is produced at a Scriptorium — the household's own Private Scriptorium (Villa §4.5) or the settlement's production chain (Buildings §5's Writing Tablets chain) — consuming Writing Tablets, Parchment, or Papyrus (Resources & Goods) as material input, plus real scribe labor-time from the household's own Amanuensis/Secretary or a hired professional copyist. Every Copy carries its own **Copy Quality** grade — Common, Fine, or Exceptional, reusing Resources & Goods' existing three-tier system (§10 of that document) — set by the copying scribe's own skill.

### 5.1 The Autograph

The very first Copy ever produced — written in the author's own hand, or under their own direct personal supervision — is flagged permanently as the **Autograph**, distinct from every later Copy regardless of that later Copy's own Quality grade. An Autograph carries a real, inherent provenance head-start (§6) purely for being the original, the same instinct that makes a CK3 artifact's earliest recorded owner matter — a Fine copy made yesterday is still, in a real sense, less than the plain, unadorned scroll the author actually touched.

### 5.2 Translation

A real, historically common ancient practice, and a genuine bridge to Language & Literacy's own fluency-tier system: a bilingual Character (holding real proficiency in both the Work's original language and a target one) can produce a **Translated Copy** — mechanically a Copy like any other, but flagged with its own source language and the translating Character's own credit alongside the original author's. Translation quality is real and variable, reading the translator's own fluency tier directly: a poor translation is a genuinely diminished version of the Work (a reduced Reading bonus, §12), while a skilled one preserves the original's own full Significance. This is this document's own concrete answer to how a genuinely foreign Work — an Institution of Renown's own Hellenic philosophy, a Punic historical account — actually becomes accessible to a Latin-only household, beyond Diplomacy with Non-Roman Peoples' own Interpreter Problem (Correspondence & Letters §7) simply being declared solved.

---

## 6. Provenance — A Copy's Own History

Every Copy tracks a real, accumulating history rather than existing as a static inventory line — the direct mechanical realization of "memory has weight" this document exists to deliver: every purchase, gift, inheritance, or theft is logged, not silently overwritten; notable involvement (cited by an Institution of Renown scholar, read by a figure of real later significance, surviving a Natural Disaster that claimed other Copies nearby) accumulates permanently on that specific Copy's own record.

### 6.1 Renowned Copies

A Copy with a sufficiently rich provenance log becomes a formally **Renowned Copy**: a real, individually named object (not merely "a copy of [Work]," but "the [Work], as owned by [notable ancestor]") carrying a Dignitas and Education bonus meaningfully above an ordinary fresh Copy of the identical text — this document's own version of a CK3 artifact leveling up, the same physical object made more valuable by nothing but the real history that's happened to it. An Autograph (§5.1) starts this ladder with a real head start over any later Copy.

### 6.2 Work Significance

A Work as a whole (independent of any single Copy) carries its own **Significance Tier** — Modest, Notable, or Renowned — driven by the author's own Trait strength and standing at the time of writing, and capable of rising further if the Work goes on to real influence.

---

## 7. Acquisition

Six real, distinct paths, several reusing machinery this project already built rather than inventing parallel ones:

- **Commission** — Education & Culture's own Literary Patronage (§7.2 of that document) finally gets a concrete deliverable: sponsoring a poet or scholar now produces an actual, real, named Work credited to that Character, rather than only ticking an abstract Cultural Prestige number.
- **Purchase** — an ordinary market transaction, most naturally through a bookseller-type Notable Business or simply an abstract dealer, priced per Resources & Goods' own Gift Value logic (§14 of that document) extended to a unique asset rather than a commodity.
- **Gift** — a Copy, particularly a Fine, Exceptional, Autograph, or Renowned one, is real, high-value Dignitas-gifting material, the intellectual counterpart to Food Culture's own Named Vintage.
- **Inheritance** — Succession & Dynasty's own inheritance division names specific, notable Copies individually rather than folding a household Library into an undifferentiated Net Worth figure.
- **War Spoils** — a real, well-documented ancient practice worth using directly: a defeated household's or foreign people's library was a genuine, real target of plunder. Military & Combat's existing War Spoils category (§7 of that document) can specifically yield a captured Work or Copy as part of a campaign's payout.
- **Discovery** — a rare, evocative Travel encounter turning up a forgotten manuscript, thought lost or never widely known, is a real, exciting possibility this document deliberately keeps rare rather than a routine outcome.
- **Theft** — a rival's own prized, irreplaceable Copy is a real, legitimate Scheme target (Characters §10), distinct from ordinary goods theft precisely because a unique or last-surviving Copy can't simply be replaced with an equivalent purchase.

---

## 8. Loss, Fragments, and the Lost Work

A Copy can be destroyed by a Fire (Natural Disasters' own existing hazard, given real, direct teeth here — a Bibliotheca or Library is a genuinely plausible Fire target, echoing the real, historically documented ancient anxiety surrounding libraries and fire across antiquity), by ordinary neglect and decay (a Copy's own Condition, with a cheaper Papyrus Copy decaying faster than a sturdier Parchment one), or by deliberate destruction (§9's own darker Damnatio Memoriae extension).

### 8.1 Fragmentary Survival — A Real Third State

New this pass, and a genuinely more historically honest model than a flat Extant/Lost binary: the actual real condition of most surviving ancient literature isn't a complete, intact text — it's a **fragment**, quoted or excerpted inside some other, later surviving Work (a Historian citing an older account, a Correspondence Collection preserving a line from an otherwise-vanished poem). When a Work's last true Copy is destroyed, this document checks first whether any surviving Work anywhere quotes or excerpts it — if so, the Work drops to **Fragmentary** rather than falling straight to Lost: a real, genuine, if much-diminished, survival. A Fragmentary Work still yields a reduced version of Reading's bonus (§12) and remains real Dynasty Chronicle material, but can never be restored to full Significance — the honest, permanent difference between having a book and having a single remembered line of it.

### 8.2 The Lost Work

If a Work's last Copy is destroyed with no surviving Fragmentary citation anywhere to catch it, the Work becomes formally, permanently **Lost** — no guaranteed recovery, no safety net, mirroring the sobering real historical fact that the overwhelming majority of ancient literature genuinely doesn't survive to any later reader. A Lost Work is real, weighty Dynasty Chronicle material, especially where it was the player's own household's original creation.

---

## 9. Forgery & Misattribution

A real, well-documented ancient problem worth building in directly rather than treating every Copy's stated authorship as automatically reliable: a lesser or anonymous work falsely attributed to a famous name — deliberately, to inflate its value, or simply through generations of copying error — was a genuine, real phenomenon in the ancient world. A Copy can carry a hidden **Forgery Flag**: its stated author isn't its real one, whether through a scribe's or dealer's deliberate fraud (a real, concealed Scheme, Characters §10) or honest historical confusion accumulated over generations of provenance.

- **Discovery** exposes the Copy's real authorship — a genuine, felt reversal for whoever paid a premium believing they owned a Renowned philosopher's own Autograph, and a real Legal & Court fraud case or Scandal exposure if the forger is identified and still living.
- **An undiscovered forgery** simply behaves as though its false attribution were true for every practical purpose — Dignitas, Reading bonuses, and Gift Value all read the stated author, not the real one, until and unless the truth actually surfaces.

This is a light, occasional mechanic rather than a constant hidden-check burden — most Copies are exactly what they claim to be; a forgery is a real, rare, dramatic exception worth having precisely because it isn't the default assumption.

---

## 10. Maps — A Specialized, Functional Copy

Elevated to its own section given how much real, distinct mechanical utility it carries beyond every other Content Category: a **Map**, authored by a Cartographer (§3, §4) or copied from an existing one, is a genuine functional tool rather than only a Reading-bonus curiosity.

- **Carrying a Map on a Journey** gives Travel (§6.18) a real, direct reduction in Piracy & Banditry and Natural Disaster exposure along the specific route it depicts — a concrete, physical realization of the Cartographer Trait's own existing route-planning bonus, now something a household can own, copy, sell, or lose independently of whether it happens to have its own Cartographer available for that particular trip.
- **A Map of a rival's own holdings, a contested frontier, or a province's defenses** is genuinely valuable Espionage material — a real, worthwhile theft or purchase target distinct from an ordinary manuscript, since its value is operational rather than purely intellectual or prestige-driven.
- **A captured enemy Map** during a Military & Combat campaign is real, concrete War Spoils (§7) with an actual tactical use, not merely a curiosity to shelve.

---

## 11. The Household Library — Collecting, and Public Donation

A household's assembled collection of owned Copies is a real, comparable prestige asset in its own right, following the same curated-display logic the Pinacotheca (Villa §4.6) already established — staffed here by the **Bibliothecarius**, a genuine Roman term for a librarian, distinct from the Pinacotheca's own art-focused Curator. A well-tended collection under a competent Bibliothecarius is real Dignitas and Education material, and a plausible, natural trigger for Education & Culture's own Renown Attracts Renown mechanic (§5.4 of that document) once it reaches real scale.

### 11.1 Donating to the Public Library

New this pass, and a genuine euergetism option: a household can formally donate a Copy — commonly a duplicate the household holds more than one of, or a single generous, deliberate gift of something genuinely valuable — to the settlement's own public Library (Buildings §4.10), a real, historically attested practice (several actual ancient libraries were patron-endowed). This is a direct, concrete Public Works & Euergetism (§2 of that document) contribution: real Dignitas and inscription credit for the donating household, and, mechanically, the donated Copy becomes meaningfully safer — a public Library, being a larger, better-resourced, more actively maintained institution than most private households' own Bibliothecae, carries a real, lower ongoing decay and Fire-loss rate than the same Copy would face sitting in a single family's own private collection.

---

## 12. Reading — Using a Book, Not Just Owning One

Owning a Copy purely for display and Dignitas is entirely legitimate, but this document also gives reading one a real, felt mechanical payoff distinct from passive collection value: a literate Character (Language & Literacy) who spends real time actually reading a specific Work gains a modest, one-time nudge to the relevant Core Attribute or Learning, and a real, if not guaranteed, assist toward acquiring the matching Lifestyle Trait, following Characters' own existing "traits accumulate through treatment and environment" pattern (§4.4 of that document). A Renowned Work or Copy carries a real, elevated version of this bonus; a Fragmentary one (§8.1) or a poor Translation (§5.2) carries a real, reduced version of it, rather than either being treated identically to a complete, well-made Copy.

---

## 13. Cross-System Integration

- **Villa / Buildings:** the Bibliotheca, Private Scriptorium, and public Library/Bibliotheca stay unchanged; this document is the individually-tracked object layer sitting inside them.
- **Companions & Court Positions:** the Bibliothecarius (§11) is a new, real position, distinct from the Pinacotheca's existing Curator; the Amanuensis/Secretary is this document's own ordinary copying and translation labor.
- **Traits:** Historian, Poet, Playwright, Philosopher, Theologian, Legal Scholar, Genealogist, Astrologer, Naturalist, and Cartographer all get their first real, concrete mechanical payoff.
- **Education & Culture:** Literary Patronage (§7.2 of that document) now produces a real, named Work; Institutions of Renown (§5) are a natural source of famous foreign Works to copy, translate, or acquire, and a household's own authored Work can feed Renown Attracts Renown (§5.4) in the other direction.
- **Language & Literacy:** literacy tier gates Authorship (§3) and Reading (§12); fluency tier directly drives Translation quality (§5.2).
- **Correspondence & Letters:** the Correspondence Collection content category is a direct, literal compiled product of that document's own letter history; the Oral Tradition Problem (§7 of that document) is a real, honest tension with this document's own Authorship access, left open per §15.
- **Dynasty Chronicle:** a Historian's authored family history is a literal physical companion to that system; a Renowned Copy, a Fragmentary Survival, and a Lost Work are all real, guaranteed-weight entries.
- **Succession & Dynasty:** a household Library's own notable Copies are named, specific inheritance items rather than folded into an undifferentiated Net Worth figure.
- **Military & Combat:** War Spoils (§7 of that document) can specifically yield a captured Work, Copy, or Map (§10).
- **Natural Disasters:** Fire is a real, direct, named threat to a household's own Library collection.
- **Monuments & Legacy Building:** Damnatio Memoriae (§7 of that document) extends naturally and darkly to a condemned author's own Works.
- **Legal & Court:** a Legal Scholar's own authored commentary is a real, citable asset in that document's case-argument mechanics; an exposed Forgery (§9) is a genuine new case type.
- **Characters:** book theft is a real Scheme target; an undiscovered Forgery (§9) is itself a live, concealed Scheme; Reading (§12) extends that document's own existing trait-acquisition pattern.
- **Travel / Piracy & Banditry / Natural Disasters:** a Map (§10) is a real, carryable risk-reduction item on any Journey.
- **Espionage:** a rival's Map or a forgery's true authorship are both real, concrete discovery targets.
- **Celebrities & Influential Figures:** a genuinely celebrated author is real Fame material, distinct from and additive to Education & Culture's own Cultural Prestige.
- **Resources & Goods:** Writing Tablets, Parchment, and Papyrus remain the material inputs to Copying; the existing Gift Value mechanic extends to a Work or Copy as a unique-valued good.
- **Notable Businesses:** a bookseller is a natural, concrete Notable Business type dealing specifically in Copies.
- **Public Works & Euergetism:** §11.1's public Library donation is a direct, real Funded-generosity contribution.
- **Food Culture:** the Culinary Treatise category (§4) is a direct extension of that document's own Named Cook mechanic.
- **Scandal:** the theft or forced sale of a household's own storied, Renowned Copy, or an exposed Forgery, are both real, felt Scandal material.

---

## 14. Data Model

```
Work {
  workId, title, category,           // per §4's table
  authorCharacterId, authorshipMonth,
  significanceTier,                    // "modest" | "notable" | "renowned" — §6.2
  status,                              // "extant" | "fragmentary" | "lost" — §8
  sourceLanguage,                       // §5.2 — the language it was originally authored in
  copiesExtantCount,                    // derived from active Copy records
}

Copy {
  copyId, workId,
  material,                           // "writingTablets" | "parchment" | "papyrus"
  copyQuality,                          // "common" | "fine" | "exceptional" — §5
  condition,                           // reuses Estate & Settlement's own condition scale
  isAutograph: bool,                     // §5.1 — true for exactly one Copy per Work, if it still exists
  isTranslation: bool,                    // §5.2
  translatedByCharacterId,                // nullable
  translationQualityTier,                  // nullable — reads the translator's own fluency
  isMap: bool,                            // §10
  mapRegionOrRouteId,                       // nullable — set only if isMap
  currentOwnerHouseholdId,                    // null if donated to a public Library, §11.1
  donatedToPublicLibrary: bool,
  forgeryFlag: {                            // §9 — hidden until discovered
    isForgery: bool,
    trueAuthorCharacterId,                   // nullable
    discovered: bool,
  },
  isRenownedCopy: bool,                       // §6.1
  status,                                    // "extant" | "destroyed" | "presumedLost"
}

ProvenanceEvent {
  eventId, copyId, month,
  eventType,                           // "created" | "inherited" | "gifted" | "purchased" | "stolen" |
                                        // "plunderedAsWarSpoils" | "citedByScholar" | "survivedDisaster" |
                                        // "donatedToPublicLibrary" | "forgeryDiscovered" |
                                        // "destroyedByDamnatio" | "destroyedByFire" | "destroyedByNeglect"
  fromHouseholdOrCharacterId, toHouseholdOrCharacterId,
}

HouseholdLibrary {
  householdId,
  bibliothecariusId,
  ownedCopyIds: [ ... ],
  collectionPrestigeTier,
}

AuthorshipProject {
  projectId, authorCharacterId,
  category, startMonth, monthsInProgress,
  commissionedByHouseholdId,
  resultingWorkId,
}

ReadingRecord {
  characterId, copyId, month,
  outcome,                              // "attributeNudge" | "traitAcquisitionAssist" | "reducedEffectFragmentaryOrPoorTranslation" | "noFurtherEffect"
}
```

---

## 15. Open Questions

- **All numeric sizing**, per convention — Authorship project duration, Copy Quality/Condition decay rates, Significance Tier thresholds, Forgery detection odds, and every Reading bonus magnitude are unsized.
- **Co-authorship.** §3 treats every Work as single-authored; whether two Characters should ever jointly author one Work isn't addressed.
- **Oral preservation of a Lost Work.** Correspondence & Letters' own Oral Tradition Problem (§7 of that document) establishes that some cultures deliberately preserve knowledge outside writing; whether a nominally Lost written Work could ever be partially reconstructed from a surviving oral tradition in a relevant culture is left open.
- **Whether a purely oral-tradition culture's own Character can hold Historian, Poet, or similar Traits at all**, given that same real tension — left honest and unresolved rather than papered over.
- **In-fiction title and content generation.** This document specifies the real structural mechanics but doesn't attempt to author the actual pool of in-fiction titles, authors, or excerpted content a player would encounter — left as a future content-authoring pass.
- **Bibliothecarius's own Villa-stage gating**, consistent with how the Curator and other late Companions & Court Positions roles are gated.
- **Forgery's own detection trigger.** §9 names Scheme discovery and honest historical confusion as two real paths to a forgery existing, but not a formal, shared roll for when (if ever) an undiscovered one comes to light on its own.
- **Whether a Fragmentary Work's surviving citation should itself be vulnerable** to being lost in turn (the one Work quoting it also being destroyed) — §8.1 doesn't specify whether Fragmentary status can regress further into a true Lost state under that circumstance.
