# GENS — System Design: Art & Art Commissions (§6.45, new)
*Expansion and polish pass. The visual-art companion to Books & Manuscripts (§6.44), built on the same real structural insight applied to a different medium: Villa decoration, the Pinacotheca, Monuments' Statue/Grand Statue, and Resources & Goods' Fine Glass/Jewelry/Statues all exist, but no individual named artist or commissioned artwork has ever existed as a persistent, provenanced asset. Ancient art gives this document an even more literal real-world parallel than literature did — an enormous share of what survives of ancient sculpture today are Roman-era marble copies of lost Greek bronze originals, the bronze itself long since melted down for its metal. This pass adds two further real media (Cameo Glass, Ivory Carving), a direct Funerary Relief tie to Ancestor Veneration, a real Style Family tag reusing Villa's own five decoration traditions, Artistic Lineage and Posthumous Renown for named Artists, Itinerant Artists as a new Wandering Populations archetype, a Public Commission acquisition path distinct from private patronage, and a Rival Collecting mechanic giving the Pinacotheca its own real competitive-escalation dynamic.*

---

## Contents

1. Scope & Role
2. The Original and the Replica — Real Roman Copying Practice
3. Named Artists — Real Occupational Titles
4. Medium Categories
5. Style Families — Tying Back to Villa's Own Five Traditions
6. Commissioning
7. Provenance
8. Acquisition
9. Loss, Melting & Re-Carving
10. Forgery & Misattribution
11. Fresco — A Site-Specific Exception, Resolved
12. The Pinacotheca, With Real Teeth
13. Cross-System Integration
14. Data Model
15. Open Questions

---

## 1. Scope & Role

The Villa's own Decoration System (§7 of that document), the Pinacotheca room, Monuments & Legacy Building's Statue/Grand Statue roster, and Resources & Goods' own Fine Glass, Jewelry, and Statues goods all stay exactly as designed. This document adds what could actually be *inside* all of that: an individually-tracked **ArtworkPiece** — a named, provenanced object with a real maker, a real history, and a real chance of being lost — the direct visual-art parallel to Books & Manuscripts' Work/Copy model, built the same way for the same reason.

---

## 2. The Original and the Replica — Real Roman Copying Practice

Books & Manuscripts built its own Work/Copy split as an abstraction. This document gets to build the identical structure on top of genuine, well-documented history rather than analogy: a real, enormous share of the ancient sculpture that survives today does so only as a **Roman-era marble copy of a lost Greek bronze original** — the original bronze itself melted down for its metal value centuries ago (§9), the copy alone surviving to be found. This document names that real pattern directly:

- An **Original** is the one-of-a-kind first artwork, made by a specific named Artist at a specific time.
- A **Replica** is a later, physically distinct copy made by a different artisan — sometimes a faithful, skilled reproduction; sometimes a workshop's own lesser stock piece. Unlike a manuscript Copy, a Replica is never mechanically "identical" to its Original — it's a genuine, separate act of craftsmanship, capable of matching, falling short of, or (rarely, memorably) surpassing the Original's own quality.

A Work — the underlying artistic conception — survives exactly as long as at least one Original or Replica of it does, anywhere. Losing the Original but leaving Replicas in circulation is a real, bittersweet, historically apt outcome, not a full loss.

---

## 3. Named Artists — Real Occupational Titles

Rather than inventing new personality Traits, this document extends Companions & Court Positions with real, historically attested Latin occupational titles — the same treatment Archimagirus already gives cooking — because visual-art production is a skilled trade, not a temperament. The existing **Master Craftsman** Trait remains the quality multiplier (exactly the role Gourmet plays for the Archimagirus in Food Culture §6), while the position itself determines the medium:

| Position | Real Latin Term | Medium |
|---|---|---|
| **Sculptor** | A real, plainly attested term | Stone and marble sculpture, bronze casting |
| **Pictor** | Real Latin for "painter" | Panel painting and Fresco (§11) |
| **Musivarius** | A real, attested term for a mosaic craftsman | Mosaic, both portable panels and site-specific floors |
| **Caelator** | A real, attested term for an engraver/relief-metalworker | Toreutic work — decorated bronze and silver vessels and reliefs |
| **Gemmarius** | A real, attested term for a gem-cutter | Glyptic art (engraved gems, cameos, intaglios) and Cameo Glass (§4) |

Any of these can be held by an enslaved worker, a freedman, or a free Character alike — legal status never gates a skill-based role in this project, consistent with every other Labor position. Worth naming honestly, though: real Roman elite attitudes toward manual craft carried a genuine class ambivalence — some liberal-arts-minded Romans (Cicero among the real, on-record examples) held certain crafts in real social disdain relative to oratory or philosophy, even while prizing the objects those crafts produced. A household can therefore own genuinely excellent art made by a socially unremarkable or even enslaved hand — a real, honest tension this document doesn't resolve away, only names.

### 3.1 Artistic Lineage — Apprenticeship and Legacy

New this pass: a Renowned Artist (an artisan whose own Works have accumulated real Significance, §7.2) can take on a genuine apprentice — a younger Character assigned to their same workshop specifically to learn under them. An apprentice who eventually produces their own independent, credited Work carries a real, earned **Lineage bonus** — a modest head start on that Work's own Significance Tier, purely for having trained under an acknowledged master. This is this document's own quiet nod to CK3's dynasty-of-craft flavor: a household that cultivates one great Sculptor can, with real investment, cultivate a whole workshop tradition across generations rather than starting over with each new artisan.

### 3.2 Posthumous Renown — When the Artist Dies

A real, genuinely poignant art-market truth worth building in directly: an artist's body of work stops growing the moment they die, and that fixed, no-longer-expanding catalogue often becomes more prized precisely because of it. When an Artist Character dies, every extant Work they authored receives a one-time, permanent Significance bump — not because the object itself changed, but because there will never be another one from that same hand. This ties naturally into Ancestor Veneration & Funerary Customs' own Memoria (§6 of that document): a household that owns several pieces by its own recently-deceased household Sculptor holds real, freshly-elevated assets at exactly the same moment it's tending that same person's memory.

### 3.3 Itinerant Artists

A famous, wandering Sculptor or Pictor moving between settlements to take on commissions wherever the wealthy will pay is a real, natural fit for Wandering Populations' own existing Wanderer taxonomy (that document's §2) — a new, concrete Wanderer archetype rather than a parallel recruitment system, giving a household without its own resident Artist a real, if less reliable, way to still commission real work.

---

## 4. Medium Categories

| Category | Position | Real Grounding | Natural Destination |
|---|---|---|---|
| **Sculpture (Statue/Bust)** | Sculptor | The Greek/Roman marble and bronze tradition this project's Monuments already draw on | Monuments & Legacy Building's Statue/Grand Statue, the Pinacotheca |
| **Panel Painting** | Pictor | A real, distinct ancient portable painting tradition, genuinely attested and genuinely surviving in some real archaeological finds | Pinacotheca, personal Villa decoration |
| **Fresco** | Pictor | Villa's own existing Decoration System (§7 of that document) | See §11 — site-specific, not portable |
| **Mosaic** | Musivarius | Villa's own existing Punic-Iberian Mosaic Style; a portable mosaic panel is a real, distinct ancient practice alongside the more familiar floor mosaic | Pinacotheca (portable) or the Villa room itself (floor) |
| **Toreutic Relief** | Caelator | A real, genuine ancient decorative-metalwork tradition — relief-worked silver and bronze vessels | Pinacotheca, gift-giving, Food Culture's own Banquet Quality (a famous silver dinner service as real hosting prestige) |
| **Glyptic Art (Gem/Cameo)** | Gemmarius | A real, prized ancient collectible category | Pinacotheca, gift-giving, personal adornment |
| **Cameo Glass** *(new)* | Gemmarius | A real, distinct, and genuinely prized ancient glassworking technique, layering contrasting colors of glass and carving through them — a real luxury pinnacle of Resources & Goods' own Fine Glass chain | Pinacotheca, the single most prestigious Fine-Glass-adjacent gift good available |
| **Ivory Carving** *(new)* | Sculptor | A real, well-attested ancient luxury carving medium, drawing directly on the existing imported Ivory good (Resources & Goods §6.6) | Pinacotheca, small portable luxury pieces |
| **Funerary Relief** *(new)* | Sculptor | A real, extensively attested ancient art genre — a decorated sarcophagus or a carved funerary stele | A direct, concrete tie to Ancestor Veneration & Funerary Customs' own burial method (§3 of that document) — the physical decoration of the tomb object itself |
| **Terracotta/Relief** *(budget tier)* | *(an ordinary Craftsman, no specialist required)* | A real, genuinely common, mass-producible ancient decorative art form | The accessible entry point for a household of modest means |
| **Numismatic/Medallion Art** | Gemmarius or Caelator | A real ancient practice of commissioning commemorative medallions | The Mint/Moneta (Buildings §4.10), the Numismatist Trait |
| **Triumphal/Historical Relief** | Sculptor | A smaller-scale narrative relief commemorating a real event | Military & Combat triumphs, Monuments' Triumphal Arch and Tropaeum |

---

## 5. Style Families — Tying Back to Villa's Own Five Traditions

New this pass, and a direct, deliberate consolidation rather than a new invention: Villa's own Decoration Style Guide (§7.1 of that document) already built five real, historically distinct style families — the Four Pompeian Styles, Provincial/Gallic, Punic-Iberian Mosaic, Hellenistic, and Provincial Fusion. Every portable ArtworkPiece this document creates carries the same **Style Family** tag, not just site-specific Fresco and floor Mosaic. A Sculpture commissioned in the Hellenistic tradition reads, and is received by a Traditionalist or Cosmopolitan audience, exactly the way a Hellenistic-styled Peristylium already does — this document doesn't invent a second, competing taste system, it simply extends the one Villa already built to cover objects as well as rooms.

---

## 6. Commissioning

Commissioning an ArtworkPiece is a genuine, deliberate project at the relevant workshop (Goldsmith's Studio, Glassblower's Studio, Marble Works, or the Villa's own Sculptor/Pictor assignment), following Books & Manuscripts' own Authorship shape: choosing a **Medium** (§4), a **Style Family** (§5), an **Artist** (a specific named Character, or an anonymous workshop commission at a lower but still real quality floor), a **Quality tier** (Common/Fine/Exceptional, Resources & Goods §10), and — the real, meaningful choice — a **Subject**.

### 6.1 Subject Matter

A real, live decision rather than flavor text: a portrait of a specific family member (drawing on the same Appearance system Villa's own Family Portrait wall already uses), a mythological or historical scene, or — the direct, concrete resolution of Villa's own long-standing open question — **a real depicted event from the household's own Dynasty Chronicle**. A commissioned piece depicting a specific Chronicle entry (a triumph, a founding, a notable ancestor's own achievement) is directly, mechanically linked to that entry, giving Villa's own flagged "is this purely descriptive or does it actually pull Chronicle entries" question a clear, resolved answer: **yes, when the player deliberately chooses that subject** — never automatic, always a real commissioning decision.

### 6.2 Public Commission

Distinct from a private household commissioning something for its own Pinacotheca or Villa: a settlement itself, most naturally through an Aedile's own public-works funding duty (Politics & Patronage §5.2) or a Public Works & Euergetism project (§2 of that document), can commission a real ArtworkPiece for civic display — a statue for the Forum, a relief for the Basilica's own façade. This is this document's own concrete extension of both systems' existing civic-generosity machinery, giving a magistrate's funding decision a genuine physical object rather than an abstract Dignitas tick.

---

## 7. Provenance

Every ArtworkPiece — Original or Replica alike — tracks a real, accumulating ownership and involvement history exactly as Books & Manuscripts' own Copy does: every purchase, gift, inheritance, or theft logged permanently; notable involvement (displayed at a famous Symposium, survived a Natural Disaster, admired by a visiting foreign dignitary) accumulating as real, lasting record.

### 7.1 Renowned Pieces

A sufficiently storied piece becomes a formally **Renowned** ArtworkPiece, carrying real Dignitas and display value above an otherwise-identical fresh commission — this document's own version of a leveling CK3 artifact, built the same way Books' Renowned Copy already was.

### 7.2 Work Significance

A Work as a whole carries its own **Significance Tier** — Modest, Notable, or Renowned — driven by the authoring Artist's own skill and standing, capable of rising through §3.1's Lineage bonus, §3.2's Posthumous bump, or simple accumulated real influence over time.

---

## 8. Acquisition

Seven real, distinct paths — six shared directly with Books & Manuscripts, plus §6.2's own civic addition:

- **Commission** (§6) — the primary, deliberate private path.
- **Public Commission** (§6.2) — the civic equivalent.
- **Purchase** — an ordinary market transaction, naturally through a dealer or a Notable Business specializing in art.
- **Gift** — high-value Dignitas material, particularly a Fine, Exceptional, or Renowned piece.
- **Inheritance** — Succession & Dynasty names specific, notable pieces individually rather than folding a collection into undifferentiated Net Worth.
- **War Spoils** — and here, this document gets to close a loop the project already opened: **Verres's own real, already-established prosecution** (Starting Regions: Sicily §4, §15.3, and Public Contracts & Competitive Bidding §6.2's own extended precedent) was, historically, substantially *about* looted Greek art specifically — this document is where that real historical content finally has somewhere concrete to land, with Military & Combat's own War Spoils category (§7 of that document) able to specifically yield a captured ArtworkPiece.
- **Discovery** — a rare Travel encounter turning up a forgotten, unattributed piece.
- **Theft** — a rival's own prized, storied piece is a real, legitimate Scheme target, distinct from ordinary goods theft precisely because a unique or last-surviving Original can't simply be replaced.

---

## 9. Loss, Melting & Re-Carving

Distinct, medium-specific destruction and recycling mechanics — a genuinely richer, more materially honest set of outcomes than Books & Manuscripts needed, precisely because different art media actually fail in different real ways:

- **Bronze and silver pieces** (Caelator's toreutic work, bronze Sculpture) carry a real, standing risk of being **melted down** for raw material value — during a household's own Insolvency, to help fund a Military & Combat campaign, or as the concrete mechanism behind Monuments & Legacy Building's own Damnatio Memoriae (§7 of that document), which already named "statues smashed or re-carved" without specifying which medium does which. This document resolves that directly: **a bronze statue is melted; that's the real, historical reason so few ancient bronzes survive relative to marble.**
- **Marble sculpture** offers the real, cheaper alternative Damnatio Memoriae's own text already gestured at: **re-carving** — a disgraced or simply unwanted portrait bust reworked into a new subject's likeness by a Sculptor, preserving the material and much of the underlying work while erasing the original subject entirely.
- **Fresco and floor Mosaic** are addressed separately in §11, being immovable.
- **Fire** remains a real, direct threat to a Pinacotheca full of portable pieces, mirroring Natural Disasters' own existing hazard.
- **The Lost Work.** If an artistic Work has no surviving Original or Replica anywhere, the Work itself is genuinely, permanently **Lost** — the same honest, unrecoverable stakes Books & Manuscripts already established for literature.

---

## 10. Forgery & Misattribution

Mirrors Books & Manuscripts §9 directly, on real, equally well-documented ancient ground: a lesser workshop piece passed off as a celebrated Sculptor's own hand, or an ordinary Roman Replica represented and sold as an authentic Greek Original, was a genuine ancient problem. A hidden **Forgery Flag** on an ArtworkPiece behaves identically to its literary counterpart — undiscovered, it's treated as its stated attribution for every practical purpose; discovered, it's a real Legal & Court fraud case and Scandal exposure for whoever's currently claiming it.

---

## 11. Fresco — A Site-Specific Exception, Resolved

Unlike every other Medium in §4, a Fresco (or a floor Mosaic) is physically part of a specific Villa room and cannot be sold, gifted, stolen, or relocated — it has no Copy or Replica of itself in the ordinary sense. It still tracks the same Provenance (§7) and can still hold real Chronicle linkage (§6.1), but its only real threats are the room's own physical fate: Natural Disasters' Fire hazard destroying it along with the building, or — a genuinely different, quieter kind of loss worth naming directly — a later generation simply **redecorating over it**, painting a new style atop an ancestor's own commissioned family-history Fresco. This is never forced or flagged as a mistake, but a household that does so while Ancestor Veneration & Funerary Customs' own Memoria (§6 of that document) is already running low reads as a small, real, additional quiet neglect.

---

## 12. The Pinacotheca, With Real Teeth

The Pinacotheca's existing Curator (Companions & Court Positions §5.1) now manages a real roster of individually-tracked ArtworkPieces rather than an abstract "owns some Fine Glass, Jewelry, and Statues" count — each displayed piece contributes its own specific Dignitas value, reading its own Quality, Renown, and provenance individually rather than as an undifferentiated collection total. A Pinacotheca holding several Renowned pieces is real, comparable prestige material, the visual-art counterpart to Books & Manuscripts' own Household Library (§11 of that document) — and, per that document's own precedent, a household can likewise donate a piece to public display as a genuine Public Works & Euergetism contribution.

### 12.1 Rival Collecting — A Real Display Rivalry

New this pass, and a natural extension of an escalation pattern this project already uses repeatedly (Business Competition's own price wars, Public Works & Euergetism's Competitive Euergetism, Monuments' own Rival Reaction): a Rival House's own particularly celebrated Pinacotheca collection is a real, visible provocation a competing household can respond to — commissioning a rival piece, outbidding for the same available Renowned work at market, or attempting to poach a rival's own resident Artist away entirely (a direct, real application of Politics & Patronage's existing poaching mechanic, §4.5 of that document, applied to a skilled artisan rather than a client). This gives art collecting the same live, mutually-escalating texture the project's other prestige competitions already have, rather than leaving it as a purely solitary pursuit.

---

## 13. Cross-System Integration

- **Villa:** the Pinacotheca (§12) and the Decoration System's own Fresco subject-matter question (§6.1, §11) both get real, resolved mechanics; the Family Portrait wall is this document's own direct precedent for Appearance-driven Subject choice; §5's Style Family tag extends that document's own five real traditions to portable objects.
- **Companions & Court Positions:** Sculptor, Pictor, Musivarius, Caelator, and Gemmarius (§3) are new, real positions; the existing Curator (§12) finally has real individual objects to manage.
- **Traits:** Master Craftsman finally gets a concrete, named output the way Gourmet already has one through the Archimagirus.
- **Monuments & Legacy Building:** the existing Statue/Grand Statue roster and Damnatio Memoriae's own "smashed or re-carved" note (§7 of that document) both get their real, medium-specific mechanism (§9).
- **Books & Manuscripts:** this document is built as its direct structural sibling — Work/Original/Replica mirrors Work/Copy, Provenance and Forgery are both reused patterns rather than reinvented ones.
- **Resources & Goods:** Fine Glass, Jewelry, Statues, and imported Ivory remain the bulk-goods baseline; an individually-tracked ArtworkPiece is this document's own deepening layer on top.
- **Ancestor Veneration & Funerary Customs:** Funerary Relief (§4) is a direct tie to that document's own burial-method mechanics; redecorating over a family-history Fresco (§11) and an Artist's own Posthumous Renown (§3.2) are both real, additional Memoria touchpoints.
- **Starting Regions: Sicily / Public Contracts & Competitive Bidding:** Verres's real prosecution (§8) finally gets the art-looting content its own historical basis was always about.
- **Military & Combat:** War Spoils (§8) can specifically yield a captured ArtworkPiece.
- **Natural Disasters:** Fire is a real, direct threat to both a portable Pinacotheca collection and a site-specific Fresco/Mosaic.
- **Dynasty Chronicle:** a deliberately Chronicle-linked commission (§6.1), a Renowned piece, and a Lost Work are all real, natural entries.
- **Succession & Dynasty:** a household's own notable ArtworkPieces are named, specific inheritance items.
- **Legal & Court / Scandal:** an exposed Forgery (§10) is a genuine case type and Scandal source.
- **Characters:** theft of a rival's storied piece is a real Scheme target; §12.1's artisan-poaching reuses that document's own Interaction Catalog directly.
- **Politics & Patronage:** the Aedile's own public-works funding duty (§5.2 of that document) is §6.2's Public Commission trigger; poaching a rival's Artist (§12.1) reuses that document's own existing poaching mechanic (§4.5).
- **Public Works & Euergetism:** §6.2's Public Commission and §12's public-display donation are both direct, real civic-generosity options.
- **Wandering Populations:** Itinerant Artists (§3.3) are a new, concrete Wanderer archetype.
- **Food Culture:** a famous Toreutic dinner service (§4) is real, direct Banquet Quality prestige material.
- **Education & Culture:** a household's own art patronage sits naturally alongside its existing Literary Patronage and Symposium mechanics as a further Cultural Prestige contribution.

---

## 14. Data Model

```
ArtworkWork {
  workId, title, medium,               // per §4's table
  originalArtistCharacterId, creationMonth,
  significanceTier,                      // "modest" | "notable" | "renowned" — §7.2
  status,                                // "extant" | "lost"
  lineageMasterArtistId,                   // nullable — §3.1
  posthumousBumpApplied: bool,               // §3.2
}

ArtworkPiece {
  pieceId, workId,
  isOriginal: bool,                       // false = a Replica
  replicatingArtistCharacterId,             // nullable — set only for a Replica
  medium, styleFamily,                       // §5 — reuses Villa §7.1's five real style families
  quality,                                // "common" | "fine" | "exceptional"
  condition,
  isFresco: bool, isFloorMosaic: bool,        // §11 — true means site-specific, tied to a Villa roomId
  linkedVillaRoomId,                        // nullable — set only for a site-specific piece
  chronicleLinkRef,                         // nullable — §6.1, set only when deliberately commissioned that way
  isPublicCommission: bool,                   // §6.2
  currentOwnerHouseholdId,                    // null if a public/civic display piece
  forgeryFlag: { isForgery: bool, trueArtistCharacterId, discovered: bool },
  isRenowned: bool,
  status,                                 // "extant" | "melted" | "recarved" | "destroyed" | "presumedLost"
}

ProvenanceEvent {
  eventId, pieceId, month,
  eventType,                              // "created" | "inherited" | "gifted" | "purchased" | "stolen" |
                                           // "plunderedAsWarSpoils" | "displayedNotably" | "survivedDisaster" |
                                           // "donatedToPublicDisplay" | "forgeryDiscovered" |
                                           // "melted" | "recarved" | "destroyedByFire"
  fromHouseholdOrCharacterId, toHouseholdOrCharacterId,
}

HouseholdPinacotheca {
  householdId, curatorId,
  displayedPieceIds: [ ... ],
  collectionPrestigeTier,
  rivalCollectingTargetHouseId,               // nullable — §12.1
}

CommissionProject {
  projectId, artistCharacterId, commissioningHouseholdId,   // null commissioningHouseholdId for a Public Commission
  medium, styleFamily, subjectType,             // "portrait" | "mythological" | "chronicleEvent" | "funerary"
  startMonth, monthsInProgress,
  resultingPieceId,
  apprenticeCharacterId,                          // nullable — §3.1
}
```

---

## 15. Open Questions

- **All numeric sizing**, per convention — commission costs/duration, Quality/Condition curves, Renown thresholds, Lineage and Posthumous bump magnitudes, and melting/re-carving trigger conditions are all unsized.
- **Whether a Replica can ever become more Renowned than its own Original** — §2 allows a Replica to mechanically surpass an Original in raw Quality, but doesn't specify whether provenance-driven Renown can invert the same way.
- **Re-carving's own subject-selection mechanic.** §9 establishes that a marble piece can be reworked into a new subject rather than destroyed, but not who chooses the new subject or whether any trace of the original remains discoverable afterward.
- **Cross-reference with Books & Manuscripts' own Forgery detection roll**, which carries the identical open question there — whether the two documents should eventually share one unified discovery mechanic rather than two parallel unsized ones.
- **Artistic Lineage's own generational depth.** §3.1 doesn't specify whether a Lineage bonus can chain across more than one apprenticeship generation (a master's apprentice's own apprentice), or caps at one direct step.
- **Rival Collecting's own escalation ceiling.** §12.1 establishes the mutual-response pattern but, consistent with Public Works & Euergetism's own identical open question about Competitive Euergetism, doesn't specify what naturally ends a given collecting rivalry.
