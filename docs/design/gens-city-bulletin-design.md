# GENS — System Design: The City Bulletin (Monthly Report & Daily Acta) (§6.57, new)
*Expansion and polish pass. A document this project has been assuming into existence since its very first pass without ever actually designing it. This pass adds a real Headline mechanic (the month's single biggest story leads, the way an actual newspaper's would), Special Editions for a genuinely major event that breaks the ordinary structure, an Imperial/Regional News tier for a household prominent enough to earn it, a Correction & Retraction mechanic giving "Rumors" real accountability when a reported story turns out to be false, a resolved archive policy distinguishing the Bulletin's own ephemeral record from Dynasty Chronicle's permanent one, and a note on how a second settlement's own local news actually reaches a household managing more than one place.*

---

## Contents

1. Scope & Role — Finally Designing What Every Document Already Assumed Existed
2. Cadence — Why Monthly, With a Daily Layer on Top
3. The Monthly Bulletin — Structure & Sections
4. The Headline — Leading With the Month's Biggest Story
5. Special Editions — When Ordinary Structure Isn't Enough
6. The Daily Acta — A Real, Cosmetic Narrative Layer
7. Personalization — What Reaches You Depends on Who You Are
8. Correction & Retraction — When a Reported Story Was Wrong
9. Sourcing — Every Line Traces to Something Real
10. Public Acta vs. Private Report
11. Archives — What's Kept, and For How Long
12. A Second Settlement's Own News
13. Cross-System Integration
14. Data Model
15. Open Questions

---

## 1. Scope & Role — Finally Designing What Every Document Already Assumed Existed

This document invents no new game state. It is purely an aggregation and presentation layer sitting on top of everything else — Familia, Economy & Finance, Politics & Patronage, Scandal, Graffiti, Games & Spectacle, Military & Combat, Religion, and Organic & Autonomous Growth's own Legibility principle (§8 of that document) all already generate real, tracked events every month. This document is the filter and presentation layer that decides what actually reaches the player, in what order, and at what level of detail.

---

## 2. Cadence — Why Monthly, With a Daily Layer on Top

This project's entire simulation runs on a real monthly tick. This document keeps its real, functional content at that same cadence: the **Monthly Report** (§3) is the actual mechanical digest. The real historical Acta Diurna's own daily publication schedule is honored purely as presentation: the **Daily Acta** (§6) redistributes the same monthly data into smaller, dated clippings — no new simulation layer, no new tick.

---

## 3. The Monthly Bulletin — Structure & Sections

Ten real sections, each sourced directly from an existing system:

| Section | Sourced From |
|---|---|
| **Household Affairs** | Familia, Succession & Dynasty |
| **Economic Summary** | Economy & Finance, Resources & Goods |
| **Political News** | Politics & Patronage, Policies & Edicts |
| **Public Notices** | The Acta Diurna, Graffiti's own *Programmata* |
| **Social & Scandal** | Correspondence & Letters, Scandal, Social Places, Graffiti |
| **Games & Spectacle Results** | Games & Spectacle |
| **Military & Frontier** | Military & Combat, Rival Houses, Piracy & Banditry |
| **Weather & Omens** | Natural Disasters, Religion |
| **The Wider World** | Organic & Autonomous Growth, Technology & Discoveries, Diplomacy with Non-Roman Peoples |
| **Chronicle Highlights** | Dynasty Chronicle |

---

## 4. The Headline — Leading With the Month's Biggest Story

New this pass: rather than presenting all ten sections as equally-weighted, the single most significant event of the month — read directly against whichever underlying system already flagged it as Chronicle-worthy or Dignitas-shifting at the largest real magnitude — is promoted to a genuine **Headline**, leading the Bulletin the way an actual newspaper's own biggest story would. A Succession, a major military victory or defeat, a Legendary Game encounter, a Discovery's own publication, or a household's own Scandal breaking wide are all natural Headline candidates. This costs nothing new to compute — it's simply a presentation choice reading the same "guaranteed weight" flag Dynasty Chronicle already applies — but it gives the Bulletin real narrative shape instead of reading as ten flat, equally-important lists every month.

---

## 5. Special Editions — When Ordinary Structure Isn't Enough

For the rare month containing something genuinely too large for an ordinary section slot — a Triumph, the household's own Succession, a war's formal conclusion — the Bulletin can break its own normal ten-section structure entirely for a **Special Edition**: a single, focused document devoted to that one event, with the month's remaining ordinary news folded into a shorter appendix rather than competing for attention. This is deliberately rare, reserved for the same tier of event that would otherwise dominate a real newspaper's own front page regardless of what else happened that week.

---

## 6. The Daily Acta — A Real, Cosmetic Narrative Layer

An entirely optional presentation mode: the same month's own Bulletin content is broken apart and redistributed across that month's own real calendar days, read as a sequence of small, dated notices in the style of the real Acta Diurna. Pure narrative dressing, never containing information the Monthly Bulletin doesn't already have.

---

## 7. Personalization — What Reaches You Depends on Who You Are

The Bulletin is never identical for every household: Dignitas, Correspondence & Letters network reach, and Politics & Patronage prominence directly gate how much of the wider world actually reaches it.

### 7.1 Imperial & Regional News — A New, Higher Tier

New this pass, resolving a real gap in the original Personalization model: a household that has genuinely reached the top of Politics & Patronage's own cursus honorum, holds a provincial governorship (Public Contracts & Competitive Bidding's own Censor track), or has otherwise become one of the settlement's most prominent, earns access to a real, further tier beyond ordinary Political News — actual Imperial and cross-provincial news, distant campaigns, and Rome's own high politics, read directly against that household's own real standing rather than being available to everyone equally. A modest household never sees this section at all; it isn't hidden from them, it simply genuinely doesn't reach them, exactly the way real news reach worked in the ancient world.

---

## 8. Correction & Retraction — When a Reported Story Was Wrong

New this pass, and a genuinely honest addition given this document's own name includes "Rumors": not everything that gets reported turns out to be true. A piece of Social & Scandal content, in particular, can be reported as fact in one month's Bulletin and revealed as false or exaggerated in a later one — a rumor that overstated a Scandal's real severity, an inaccurate report of a Rival House's own fortunes. When this happens, the later Bulletin carries a real, explicit **Correction** entry, and, where the original false report can be traced to a specific source (a Graffito, a gossiping Character, a Scriptor Commission), a real, small Dignitas or Reputation consequence lands on whoever spread it, per Graffiti's own existing Backfire logic (§9 of that document). This gives "Rumors" real teeth as a genuinely fallible information channel rather than a channel that's quietly always accurate.

---

## 9. Sourcing — Every Line Traces to Something Real

Nothing in the Bulletin is generated fresh for the Bulletin's own sake. Every line is a direct pull from an already-existing record, reformatted for readability.

---

## 10. Public Acta vs. Private Report

- **The Public Acta** — genuinely public, containing only what's actually, legitimately public knowledge.
- **The Private Report** — richer and personal, including a private Scheme's status, detailed finances, or an unexposed Scandal.

---

## 11. Archives — What's Kept, and For How Long

New this pass, resolving the first pass's own open question directly: a Bulletin is deliberately **ephemeral**, distinct from Dynasty Chronicle's own permanent record. Recent months remain fully browsable in detail; older ones progressively compress down to just their own Headline (§4) and Chronicle Highlights, with the ordinary day-to-day texture of an economic summary or a minor Games result naturally fading from easy access the way an old newspaper does in reality — the Chronicle is where anything actually worth permanently remembering already lives, and the Bulletin was never meant to duplicate that job.

---

## 12. A Second Settlement's Own News

A household managing a second settlement (Estate & Settlement's own late-game possibility) receives that settlement's own separate, smaller local Bulletin, generated by the exact same structure (§3) but reflecting that outpost's own, typically more modest, standing — folded into the player's own combined Private Report as a distinct, clearly-labeled sub-section rather than merged indistinguishably into the primary settlement's own news.

---

## 13. Cross-System Integration

- **Every system named in §3's table:** the direct, comprehensive aggregation point for all of them.
- **Organic & Autonomous Growth:** this document is the concrete implementation of that document's own Legibility principle.
- **Technology & Discoveries:** the real Acta Diurna Discovery is this document's own direct anchor and namesake.
- **Graffiti, Dynamic Walls & Rumors:** Public Notices, Social & Scandal, and Correction & Retraction (§8) all draw directly on that document's own records, including its Backfire mechanic.
- **Dynasty Chronicle:** Chronicle Highlights and the Headline mechanic (§4) both pull directly from that document's own "guaranteed weight" entries; the Archive policy (§11) explicitly distinguishes this document's own ephemeral record from that one's permanent one.
- **Politics & Patronage / Public Contracts & Competitive Bidding:** both directly gate the new Imperial & Regional News tier (§7.1).
- **Social Places:** the Forum remains this document's own real, physical in-world seat for the Public Acta.
- **Estate & Settlement:** §12's second-settlement provision is a direct, real extension of that document's own late-game outpost possibility.

---

## 14. Data Model

```
MonthlyBulletin {
  bulletinId, householdId, settlementId, month,
  sections: {
    householdAffairs: [ ... ], economicSummary: [ ... ], politicalNews: [ ... ],
    publicNotices: [ ... ], socialAndScandal: [ ... ], gamesResults: [ ... ],
    militaryAndFrontier: [ ... ], weatherAndOmens: [ ... ], widerWorld: [ ... ],
    chronicleHighlights: [ ... ], imperialRegionalNews: [ ... ],   // §7.1, nullable/empty for most households
  },
  headlineLineItemId,                  // §4, nullable
  isSpecialEdition: bool,                // §5
  personalizationTierUsed,
}

BulletinLineItem {
  lineItemId, bulletinId, section,
  sourceSystem, sourceRecordRef,
  isPublic: bool,
  isCorrection: bool,                   // §8
  correctsLineItemId,                    // nullable — set only if isCorrection
}

DailyActaEntry {
  entryId, bulletinId, sourceLineItemId,
  displayDay,
}

PublicActaPosting {
  postingId, settlementId, month,
  lineItemRefs: [ ... ],
  linkedSocialPlaceId,
}

BulletinArchiveEntry {                  // §11
  bulletinId,
  compressionLevel,                      // "full" | "headlineAndChronicleOnly"
}
```

---

## 15. Open Questions

- **All numeric sizing**, per convention — Headline-selection thresholds, Imperial & Regional News access criteria, Correction frequency, and Archive compression timing are all unsized.
- **Whether the Daily Acta should be generated in advance or revealed day-by-day** remains a presentation/UX question outside this document's own scope.
- **Whether a rival household's own Bulletin should ever be partially visible to the player** through Espionage remains a plausible, unbuilt future extension.
- **Whether a Correction (§8) should ever be contested** — a Character insisting the original report was actually true despite an official retraction — is a real, dramatic possibility this pass doesn't resolve either way.
