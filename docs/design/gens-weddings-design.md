# GENS — System Design: Weddings (§6.49, the Activity Engine's second Activity Type)
*The Activity Engine's own second real proof of concept, and the one that gets to build on a genuinely useful historical fact Romance, Sexuality & Lineage already established: Roman marriage required no ceremony at all to be legally real. What made a marriage was affectio maritalis — mutual intent, evidenced by cohabitation — not a wedding day. That means this document isn't building the mechanism that makes two people married; Familia's own marriage math already does that. This document is building the celebration — a real, optional, Extended Activity spanning betrothal to wedding day to a nested Feast, giving an existing legal and financial arrangement a real, socially visible, guest-listed expression worth throwing a party over.*

---

## Contents

1. Scope & Role — The Celebration, Not the Marriage Itself
2. The Wedding's Six Slots
3. Phases — Betrothal, the Wedding Day, and a Nested Feast
4. Ceremony Form — Confarreatio, Coemptio, and Usus
5. Elopement — Skipping the Activity Entirely
6. Cross-Cultural and Cross-Status Weddings
7. Cross-System Integration
8. Data Model
9. Open Questions

---

## 1. Scope & Role — The Celebration, Not the Marriage Itself

Familia's own marriage math (§5 of that document) — dowry, alliance value, consent/happiness — and Romance, Sexuality & Lineage's own courtship track (§4 of that document) already determine *whether* two people marry and on what real terms. This document doesn't touch either. What it builds is the real, optional social event around that decision: a Wedding, built on the Activity Engine (§6.47), giving an already-real marriage its own visible, Guest-List-driven, Dignitas-and-relationship-building celebration — genuinely valuable when held, and, per §5, genuinely skippable without invalidating the marriage itself.

---

## 2. The Wedding's Six Slots

1. **Host** — historically the bride's own family hosted the *sponsalia* betrothal, and the groom's family the wedding day itself; this document treats "Host" loosely as both families jointly convening rather than forcing a strict division most playthroughs won't want to micromanage.
2. **Type** — `"wedding"`.
3. **Venue** — the Atrium (for the sponsalia and Auspices-taking), the groom's own Villa broadly for the wedding day's own rituals, and the Oecus or Triclinium for the culminating Feast (§3).
4. **Guest List** — both families' own Clientela, Companions, and kin, per Activity Engine §4; a wedding's own Guest List is a real, doubled political question — whose standing each family wants displayed, and which of the other family's own guests hosting alongside them actually entails.
5. **Duration** — Extended (Activity Engine §3), spanning the real betrothal period through to the wedding day itself.
6. **Phases** — §3.

---

## 3. Phases — Betrothal, the Wedding Day, and a Nested Feast

### 3.1 Sponsalia (Betrothal)

A real, formal event in its own right: dowry terms are finalized (Familia §5), and the real, historically attested iron betrothal ring (*anulus pronubus*) is exchanged — a direct, natural occasion for Masterworks & Unique Crafted Objects' own Heirloom Jewelry (§3 of that document): a household commissioning a new ring, or passing down one that's carried real family provenance for generations, exactly the moment that category was built for.

### 3.2 Auspices Taken

A real, historically attested pre-wedding practice: consulting the Auspices (Religion §4.2) for a favorable reading before the wedding day itself. An Ill Omen here is a real, atmospheric, dramatic complication — not a hard block, consistent with Omens' own existing heed-or-ignore choice architecture — rather than a wedding-canceling event.

### 3.3 The Bride's Preparation

Pure, rich flavor content needing no new mechanic: the real *seni crines* hairstyle, parted with the ceremonial *hasta caelibaris* (a small, real, attested spear-shaped implement), the flame-colored *flammeum* veil, and the *tunica recta* — genuine historical texture giving this Phase real color without inventing anything.

### 3.4 Ceremony Form

See §4.

### 3.5 The Deductio (Procession)

The real, public procession from the bride's home to the groom's — a real, felt public Dignitas display, its visibility scaling directly with the Wedding's own Scale tier (Activity Engine §5.1) and Witness Pool (§7 of that document): a larger, more elaborate procession is seen by more of the settlement, for better or worse. Real, documented custom accompanies it — nuts thrown to children along the route, and the crowd's own real customary shout of good fortune.

### 3.6 Threshold-Crossing

The groom carrying the bride over the threshold — real, documented Roman custom, meant to avoid the bad omen of her stumbling on the way in — a small, natural closing beat to the wedding day's own ritual sequence.

### 3.7 The Wedding Feast — A Nested Activity

Rather than building a second feast mechanic, this Phase is simply a full **nested Feast Activity** (§6.48), Purpose set to `"weddingFeast"`, run exactly as that document already specifies. This is worth naming as a small, genuinely useful extension to the Activity Engine itself: **a Phase can be an entire nested Activity**, not only a simple Interaction or Event moment — the clean, reusable pattern behind any future large Activity Type (a Triumph, a major religious festival) that similarly wants to culminate in its own real Feast without reinventing one.

---

## 4. Ceremony Form — Confarreatio, Coemptio, and Usus

A real, historically distinct three-way choice, tying directly into Romance, Sexuality & Lineage's own existing manus/sine manu legal fork (§4.1 of that document) rather than inventing a parallel flavor system:

- **Confarreatio** — the older, more elaborate, patrician-associated religious ceremony, involving a real offering of spelt bread to Jupiter, and the fullest expression of manus marriage. Genuinely rare by this game's own era even among the old aristocracy — choosing it is a real, deliberate, old-fashioned statement, carrying a real Traditionalist-audience reception bonus (Politics & Patronage §3.1) precisely because of how consciously archaic it reads.
- **Coemptio** — a real, symbolic "mock sale" ceremony, also a manus form, but more accessible and less overtly patrician than Confarreatio — a formal, respectable, unremarkable choice by this game's own era.
- **Usus** — no formal ceremony required at all beyond a real year of cohabitation, the genuine sine manu path letting the wife remain under her own birth family's legal authority — increasingly the practical norm by this game's era, and the same real legal mechanism underlying Elopement (§5) when no Wedding Activity happens at all.

---

## 5. Elopement — Skipping the Activity Entirely

The honest, real alternative this document deliberately preserves rather than treating a Wedding as mandatory: per Romance, Sexuality & Lineage's own affectio maritalis (§4.1 of that document), a couple can simply become married through mutual intent and cohabitation alone — no sponsalia, no Auspices, no procession, no Feast. This is cheap, fast, and available to any household regardless of wealth, but forfeits every one of this document's own Dignitas, relationship, and Guest-List benefits entirely.

Elopement also carries a real, felt cost per the Activity Engine's own Exclusion logic (§4.2 of that document): any family member, patron, or ally who'd reasonably have expected real involvement in a proper Wedding — most sharply, a patron who personally brokered an arranged alliance match and expected a real, public celebration to display it — reads being cut out of one via elopement as a genuine, if unspoken, slight. A love-match eloping to defy family opposition or simply out of haste, and an arranged match's own family quietly skipping the expense, are mechanically identical events that read as genuinely different stories depending entirely on context — exactly the kind of legible-but-not-heavy-handed distinction this project consistently favors.

---

## 6. Cross-Cultural and Cross-Status Weddings

- **Cross-cultural marriage** (Education & Culture §9, Diplomacy with Non-Roman Peoples' own Marriage Alliance) blends ceremony traditions at the culminating Feast Phase specifically, reading Food Culture's own Cuisine Match mechanic (§3 of that document) directly — a real, natural Cosmopolitan-audience moment rather than a forced binary choice between two cultures' own customs.
- **Concubinage** (Romance, Sexuality & Lineage §6) never uses this Activity Type — a concubine bond forms and ends without a Wedding, consistent with that document's own framing that it "simply ends" rather than being subject to formal marriage or divorce processes.
- **A widow's remarriage** must clear Ancestor Veneration & Funerary Customs' own *tempus lugendi* (§4.2 of that document) before any Wedding Activity can be scheduled at all — a real, direct, hard gate this document defers to entirely rather than duplicating.
- **A legally restricted pairing** — a senator barred from marrying a freedwoman or actress under the real *lex Julia et Papia* (Romance §6) — cannot hold a Confarreatio or Coemptio Wedding for that union at all; the honest, available path is Concubinage instead, not a workaround Wedding this document would need to invent.

---

## 7. Cross-System Integration

- **Activity Engine:** this document is the Engine's second fully-specified Activity Type, and the first to demonstrate a Phase containing a full nested Activity (§3.7) — a light, useful extension worth folding back into that document's own future revisions.
- **Feasts:** the Wedding Feast Phase is a direct, literal instance of that document's own Feast Activity Type, Purpose = weddingFeast.
- **Familia:** dowry finalization (§3.1) and the underlying marriage decision itself remain entirely that document's own §5 territory; this document adds no new marriage mechanic.
- **Romance, Sexuality & Lineage:** the manus/sine manu fork (§4.1) directly drives Ceremony Form (§4); affectio maritalis is the real legal mechanism behind Elopement (§5); Concubinage (§6) is explicitly excluded from this Activity Type.
- **Masterworks & Unique Crafted Objects:** the *anulus pronubus* betrothal ring (§3.1) is a natural, real Heirloom Jewelry occasion.
- **Religion:** the pre-wedding Auspices (§3.2) reuse that document's own Omens machinery directly.
- **Ancestor Veneration & Funerary Customs:** the widow's *tempus lugendi* (§4.2 of that document) is a hard, deferred-to gate on scheduling any Wedding.
- **Education & Culture / Diplomacy with Non-Roman Peoples:** cross-cultural marriage's own ceremony blending is handled through the nested Feast's Cuisine Match, not a parallel mechanic.
- **Politics & Patronage:** Confarreatio's own Traditionalist reception bonus reads that document's own Faction axis; a patron's own reaction to being excluded via Elopement is a direct, real Clientela consequence.
- **Policies & Edicts:** Marital Diplomacy Posture (§2.11 of that document) sets the household's own general lean on dowry-vs-consent that this Activity's own underlying marriage still reads, unmodified.
- **Scandal:** an Ill Omen ignored, an Elopement that visibly slights an expectant patron, or a badly-executed, publicly visible Deductio are all real, felt Scandal-adjacent material.
- **Dynasty Chronicle:** a Confarreatio wedding, a Legendary-Quality nested Feast, or a dramatic elopement are all natural, guaranteed-weight entries.

---

## 8. Data Model

```
Wedding extends Activity {              // §6.47's Activity, type = "wedding", durationMode = "extended"
  brideCharacterId, groomCharacterId,
  ceremonyForm,                    // "confarreatio" | "coemptio" | "usus" — §4
  manusOrSineManu,                  // read directly from Romance §4.1's own existing flag
  betrothalRingMasterworkId,          // nullable — §3.1
  auspicesOutcome,                   // nullable — §3.2, reuses Religion's Omen resolution
  nestedFeastActivityId,               // §3.7 — links to a real Feast record
  isElopement: bool,                  // §5 — true means every other Phase above is skipped entirely
}
```

---

## 9. Open Questions

- **All numeric sizing**, per convention — the Confarreatio Traditionalist bonus, procession-visibility Scale scaling, and Elopement's own patron-slight magnitude are all unsized.
- **Whether the two families' own separate Host role (§2) should ever be split into two formally distinct, jointly-resolved Guest Lists** rather than one merged list, particularly where the two households are on genuinely poor terms with each other's own guests.
- **Confarreatio's own real historical difficulty of divorce.** Real Roman practice held this ceremony's own manus bond as genuinely harder to dissolve than an ordinary marriage; whether Familia's own Divorce mechanic (§5.1 of that document) should read Ceremony Form as a real complicating factor isn't addressed here.
- **Nested-Activity precedent for future Types.** §3.7's pattern is deliberately generalizable, but this document doesn't formally amend the Activity Engine's own text — left as a note for that document's own next revision.
