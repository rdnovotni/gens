# GENS — System Design: Ancestor Veneration & Funerary Customs (§6.39, new)
*A genuinely missing piece this project has been gesturing at since the Villa doc first gave the Atrium's ancestor masks (*imagines*) an undefined "ongoing small Dignitas trickle," and since Monuments & Legacy Building explicitly flagged the Ancestor Gallery as deferred. Per direction, this document keeps that Villa-room question deferred and stays building-agnostic — it owns the funeral itself, the mourning that follows it, and the ongoing cult of the family dead, with a real new tracked value (**Memoria**) sitting alongside Favor and Dignitas as a third axis of household standing. If Design Pillar #7 is "memory has weight," this is the document where that pillar stops being a theme and becomes an actual number.*

---

## Contents

1. Scope & Role
2. Death & the Funeral
3. Burial Method — Cremation, Inhumation, and Real Cultural Drift
4. Mourning — *Luctus*
5. The *Manes* & the Ongoing Ancestor Cult
6. Memoria — The Third Axis
7. The *Laudatio Funebris* as a Political Instrument
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role

Religion (§6.6) already owns the household's living relationship with its gods — the Lares, Penates, and the head-of-household's own Genius, tended daily and read through the Favor meter. This document owns something adjacent but genuinely distinct: the Roman dead themselves, the **di Manes** (deified ancestral spirits), and everything that turns an ordinary death into a lasting, honored — or dishonored, or simply forgotten — part of a family's memory.

Per direction, this system is **mid-weight**: a real new tracked value (§6) that other systems read the way they read Favor or Dignitas, built without becoming a second full religious-simulation subsystem sitting alongside the one Religion already provides. It owns three real, sequential moments in a Character's death — the funeral itself (§2), the mourning that follows (§4), and the ongoing cult that continues indefinitely afterward (§5) — and gives the existing Family Tomb, Mausoleum, and Companions & Court Positions' Libitinarius role the actual procedures and consequences they've been waiting for since Monuments and Companions & Court Positions each named them without building around them.

This document deliberately stays **building-agnostic** per direction: the Ancestor Gallery Villa room remains a future Villa-doc revisit, and nothing here requires a new physical space beyond what already exists (the Family Tomb, the Atrium's existing *imagines* display, the public Necropolis).

---

## 2. Death & the Funeral

Every death in this project — old age, illness (Disease & Public Health), battlefield loss (Military & Combat §5.5), a Legal & Court capital sentence, a successful assassination (Espionage, Piracy & Banditry) — now routes through the same real sequence before Succession & Dynasty's inheritance resolution begins in earnest. The funeral is not a delay bolted in front of the mechanically important part; it *is* mechanically important, because the choices made here are this document's primary lever on Memoria (§6).

### 2.1 The *Collocatio* — Laying Out

The deceased is washed, anointed, and laid out in the Atrium for a viewing period, feet toward the door, per real Roman custom — a coin placed for Charon's fee is a real, attested practice the game can render as flavor text without needing a mechanic of its own. This step is automatic and cost-free; it exists to make the funeral feel like a real event rather than a menu transition.

### 2.2 The *Pompa Funebris* — The Funeral Procession

The real mechanical heart of the funeral. The player chooses a **Funeral Tier** — Modest, Proper, or Grand — the same three-tier shape as Religion's Rites Budget, trading a real, one-time Treasury cost against the funeral's Memoria and Dignitas yield:

- **Modest** — a small procession, hired mourners (*praeficae*) if any, no *imagines* displayed. Cheap, appropriate for a household in genuine financial distress or for a death the family has real reason not to publicize (Scandal exposure, a disgraced Character).
- **Proper** — the expected standard for a household of real standing: musicians, professional mourners, a respectable procession route through the settlement.
- **Grand** — for elite households only, and the historically real, genuinely striking practice this project hasn't used yet: actors wearing the household's *imagines* (wax ancestor masks) and dressed in the ancestors' own magisterial regalia walk in the procession, so a Roman funeral for a distinguished family was also, quite literally, a parade of the family's entire remembered history walking through the streets. A Grand funeral for a Character with few or no Chronicle-notable ancestors reads as hollow ambition rather than earned grandeur — the tier's own Memoria and Dignitas payoff scales with how much real ancestral achievement (Dynasty Chronicle entries, prior Memoria) the household actually has to display, not with cost alone.

### 2.3 The *Laudatio Funebris* — see §7, handled separately given its real political weight.

### 2.4 Interment

Closes the sequence — see §3 for method and §8 for the Family Tomb, Mausoleum, and Collegia Funeraticia's burial guarantee (Collegia & Guilds §8) as the three real destinations available depending on household means and standing.

---

## 3. Burial Method — Cremation, Inhumation, and Real Cultural Drift

Per direction, burial method is **primarily determined by culture and religion rather than offered as a free mechanical choice**, with a light flavor-adjustable layer on top.

### 3.1 The Real Historical Shift

A genuinely useful, underused piece of real history for a game spanning 133 BC–AD 235: cremation was the dominant elite Roman practice through the Republic and into the early Empire, while inhumation gradually overtook it as the prevailing norm across the 2nd century AD, driven by a real mix of changing fashion, Eastern mystery-cult influence, and (toward the game's outer edge) early Christian practice. Rather than a static default, this is modeled as a **soft cultural drift** — a household's Culture (Cultures of the Known World) and Faith (Religions of the Known World) tenets set a strong default and a household is free to follow it without a second thought, but a household that deliberately chooses against its own era's or culture's drifting norm (a stubbornly cremating household deep into the 2nd century AD, or an early inhumation adopter well ahead of the trend) reads as a real, legible statement — Traditionalist audiences (Politics & Patronage §3.1) read the former as commendable old-fashioned piety or stubborn backwardness depending on the household's own standing, and the latter as fashionable, foreign-influenced, or quietly heterodox depending on which Faith is doing the adopting.

### 3.2 Faith Tenets as the Real Determinant

Religions of the Known World's own tenet system is the actual mechanical driver: a faith carrying a strict burial-practice tenet (early Christianity and Judaism both historically favored inhumation on real theological grounds; several mystery cults carried their own real preferences) locks that household's default hard, no drift required. A household following Roman State religion or a faith without a strict tenet simply follows the timeline-driven soft drift described in §3.1. Neither path requires new numeric tradeoffs — the "some mechanical weight" the person allowed for lives entirely in the Traditionalist-audience reception described above, not in a new cost/benefit table.

### 3.3 The Physical Destination

Cremated remains go to an urn housed in the Family Tomb, or — for a household of modest means, or a freedman without one of his own — a shared **Columbarium**, a real, attested Roman structure literally built with wall-niches for many households' urns at once, and the natural physical home for the Collegia Funeraticia's burial guarantee (Collegia & Guilds §8) to actually point at. Inhumation uses a sarcophagus, likewise housed in the Family Tomb or, for a household without one, the public Necropolis (Buildings §4.10). Neither destination requires a new Building — the Columbarium is a flavor variant of the existing Family Tomb/Necropolis pairing, not a new structure demanding its own construction slot.

---

## 4. Mourning — *Luctus*

A real, socially binding period following any household death, distinct from the funeral event itself and lasting a household-defined duration (no numeric sizing per convention, deferred to balancing).

### 4.1 Household Mourning

For the duration of *luctus*, the household observes real, historically attested restrictions: dark clothing (the *toga pulla*), women's hair worn unbound and unadorned, no bathing beyond basic hygiene, and — mechanically real — a standing prohibition on hosting or attending Games & Spectacle events, throwing a Religion §3.1 Lavish-tier feast, or opening formal Romance & Seduction courtship, all of which would read as a genuine scandal if violated. A household that visibly breaks its own mourning early (attending a rival's banquet, opening a betrothal negotiation) is a real, direct Scandal (Scandal §4) trigger — "dancing on the grave" being exactly the kind of transgression that document's aftermath engine already exists to catch.

### 4.2 Widow's Mourning — the *Tempus Lugendi*

A specific, real, and historically significant sub-case: a widow observed a real, extended mourning period — traditionally around ten months — before remarriage was socially acceptable, rooted in genuine period concern over establishing a child's paternity beyond doubt. This is Romance, Sexuality & Lineage's own concrete gate: that document's existing lifecycle and legitimacy machinery reads this period as a hard floor on how soon a widow can be courted or remarried without a real social cost, with an early remarriage reading as either desperate necessity (a poor widow with no other means of support) or real impropriety depending on the household's actual financial position.

### 4.3 *Iustitium* — Public Mourning

The genuinely rare, settlement- or state-scale mirror of §4.1: a formally declared suspension of public business — courts closed, the Curia in recess, markets subdued — historically real and reserved for the death of a truly significant figure (a sitting magistrate, an Emperor, or, at this project's own scale, a household's own head if that household has reached real Prominence in Politics & Patronage). A player-triggered *iustitium* is a genuine, rare Funded Action available only to a household or figure of real standing, producing a substantial one-time Memoria and Dignitas gain precisely because it is a public acknowledgment that the whole settlement, not just one household, has stopped to mourn.

---

## 5. The *Manes* & the Ongoing Ancestor Cult

Distinct from Religion's Lares (household guardians), Penates (pantry gods), and Genius/Juno (the living head's own guardian spirit): the **di Manes** are the collective spirits of the family's own dead, venerated at the tomb itself rather than the household Lararium, and tended through two real, dateable festivals that become permanent additions to Religion's Sacred Calendar (§5 of that document).

### 5.1 *Parentalia*

A real nine-day February festival (traditionally the 13th–21st) during which Roman families visited and tended ancestral tombs, leaving offerings of wine, milk, and flowers — a real, warm, family-centered observance rather than a somber one. In this project, a household's *Parentalia* observance is the single most reliable ongoing source of Memoria (§6), structurally parallel to Religion's own household-worship Favor mechanic: a consistently observed *Parentalia* is a quiet, recurring gain, and a skipped one — through neglect, absence on Travel, or a household in genuine financial crisis unable to afford even a modest offering — is Memoria's most common source of quiet drift downward.

### 5.2 *Feralia*

The real closing day of the *Parentalia* period, marking the formal end of the festival with a final, more solemn round of tomb offerings — folded in as *Parentalia*'s own concluding beat rather than a separate mechanic.

### 5.3 *Lemuria*

A real, distinctly different May festival, addressing the dead's other, less comfortable half: the *lemures*, restless or improperly-honored spirits believed capable of troubling the living. The real historical rite — walking barefoot at midnight, throwing black beans over one's shoulder nine times while repeating a set formula to ward off the *lemures* — is genuinely vivid, atmospheric content perfectly suited to an Omens & Auspices-style Event (Religion §4.1): observed correctly, *Lemuria* is a minor, protective Favor/Memoria touch; a skipped or botched observance in a household already suffering low Memoria (§6.3) is one of this document's own real sources of an Ill Omen, read by a Superstitious or Zealous Character as the *lemures* making their displeasure known.

---

## 6. Memoria — The Third Axis

Per direction on mechanical weight: a single new tracked household value, structurally parallel to Religion's Favor and sitting alongside Dignitas as this project's third real axis of standing — Dignitas is what the living think of the household; Favor is what the gods think; **Memoria is what the household's own dead think, and how much of their memory the living have actually preserved.**

### 6.1 What Builds Memoria

- A well-conducted funeral (§2.2), scaled by tier and by real ancestral achievement already on record.
- Consistent *Parentalia* observance (§5.1) — the single most reliable ongoing source, exactly mirroring Favor's own household-worship foundation.
- A correctly-observed *Lemuria* (§5.3).
- A Dynasty Chronicle entry for any ancestor, which contributes a small, permanent Memoria trickle for as long as that entry remains part of the family record — this is the document's most direct realization of "memory has weight": a family with a long, rich Chronicle literally has more Memoria to draw on than a young or unremarkable one, independent of current wealth or Dignitas.
- A maintained Family Tomb or Mausoleum (Monuments & Legacy Building) — Monuments' own neglect-and-decay mechanic (§6 of that document) is the direct, shared driver; a Family Tomb allowed to fall into disrepair is a real, concrete Memoria loss on top of that document's own Legacy Tier regression.
- An *iustitium* (§4.3), where available.

### 6.2 What Memoria Actually Does

Memoria is deliberately a *quiet* meter compared to Favor's more active Omens/Auspices layer — its effects are steady, cumulative, and mostly read by other systems rather than generating its own frequent Events:

- **Dynasty Chronicle:** a high-Memoria household's entries read with more real gravity — the game's own prose treats "the fifth generation to hold this land" differently from "the family's first notable act," and a Chronicle-worthy achievement by a high-Memoria household has a real chance of an added flourish tying it explicitly back to a named ancestor.
- **Politics & Patronage:** a Traditionalist-leaning audience reads sustained high Memoria as a real, independent mark of a *bona fide* old family, distinct from raw Dignitas — a *novus homo* with excellent Dignitas but negligible Memoria is still legibly new money to exactly the audience that cares most about the distinction.
- **Epithets, Nicknames & Titles:** sustained high Memoria is a real, earned path to a *Pius* — or, read cynically by a rival, an *ostentatiously pious* — cognomen or epithet, alongside that document's existing sources.
- **Succession & Dynasty:** a genuinely low-Memoria household facing extinction (§7 of that document) reads as a quieter, sadder kind of loss than a wealthy one — the family's own memory, not just its holdings, was already thin before the line ran out.

### 6.3 Memoria Loss

Memoria erodes from real neglect rather than active offense (a genuine, deliberate insult to the dead — grave desecration, a rival's Damnatio Memoriae petition succeeding against a house's own ancestor — is Monuments & Legacy Building's own territory, not duplicated here): a skipped *Parentalia*, an untended or decayed Family Tomb, a Modest funeral chosen for a Character the household actually had the means to honor properly, or — the sharpest, rarest case — a full generational gap where an heir simply never engages with any of this document's mechanics at all, reading as a house that has quietly stopped caring about its own dead.

Recovery follows this project's now-standard shape: sustained correct observance, a strong *iustitium* or Grand funeral, or a genuine Dynasty Chronicle achievement pulls Memoria back — nothing here is a hard, unrecoverable failure state, consistent with "no forced ending."

---

## 7. The *Laudatio Funebris* as a Political Instrument

Worth its own section given how much real political weight this single real Roman institution actually carried: the funeral oration, delivered publicly — often in the Forum itself for a sufficiently prominent figure — by a family member or a chosen orator, was simultaneously a personal eulogy, a recitation of the *family's entire ancestral record* (the real occasion the *imagines* procession of §2.2 existed to visually support), and a genuine opportunity for the living speaker's own political self-presentation.

Mechanically, delivering a *laudatio funebris* is a real Politics & Patronage action available to whichever Character the player designates as speaker: a well-delivered one is a direct, above-board Dignitas and Memoria gain for the speaker specifically, not just the household; a poorly-delivered one (a weak Rhetoric stat, an Character genuinely disliked by the audience) can actually cost Dignitas rather than merely fail to gain it. A politically ambitious heir has a real, historically accurate incentive to want to deliver their own predecessor's oration personally rather than deferring to an older relative or a hired orator — the *laudatio funebris* was a real, attested launching point for several actual Roman political careers, and this document lets a player's own heir use it the same way.

---

## 8. Cross-System Integration

- **Religion:** the Manes cult (§5) is this document's direct extension of that document's Sacred Calendar (§5 of that doc); Favor and Memoria are explicitly parallel, non-competing axes of standing, distinguished by living-gods-vs-family-dead rather than one superseding the other.
- **Monuments & Legacy Building:** the Family Tomb and Mausoleum finally get real procedural mechanics (§2.4, §3.3) and a genuine Memoria stake in that document's own neglect/decay system (§6.1); a Damnatio Memoriae verdict (§7 of that document) is the sharpest possible external threat to Memoria this document doesn't itself generate.
- **Dynasty Chronicle:** every Chronicle entry now carries a small permanent Memoria contribution (§6.1); the *laudatio funebris* (§7) is a natural, real Chronicle-eligible moment in its own right.
- **Succession & Dynasty:** every death now routes through this document's funeral sequence (§2) before that document's inheritance resolution proper begins; a low-Memoria extinction (§6.2) is this document's own quiet contribution to that system's own honest, non-forced ending.
- **Politics & Patronage:** *iustitium* (§4.3) is a genuine, rare Funded Action gated on real Prominence; the *laudatio funebris* (§7) is a real political action in its own right; sustained Memoria reads independently of Dignitas to a Traditionalist audience (§6.2).
- **Romance, Sexuality & Lineage:** the widow's *tempus lugendi* (§4.2) is a concrete, real gate on that document's own remarriage timing.
- **Collegia & Guilds:** the Collegia Funeraticia's burial guarantee (§8 of that document) now has a real physical destination (the Columbarium, §3.3) and a real place in this document's own funeral sequence for a household without independent means.
- **Companions & Court Positions:** the Libitinarius (§4 of that document) is this document's concrete staffing answer for arranging the funeral itself, particularly at Proper or Grand tier.
- **Cultures of the Known World / Religions of the Known World:** burial method (§3) is directly driven by that pair of documents' own culture and faith tenets rather than a free-standing choice.
- **Scandal:** an early-broken mourning period (§4.1) is a real, new Scandal source; a conspicuously hollow Grand funeral (§2.2) for a Character with no real ancestral record is a quieter, Dignitas-only embarrassment rather than a formal Scandal.
- **Epithets, Nicknames & Titles:** sustained high Memoria is a real, new path to a *Pius*-family cognomen or epithet (§6.2).
- **Traits:** Piety and Zealotry (Traits §3.5, Characters §5) weight *Parentalia*/*Lemuria* engagement and Memoria-related Omen severity exactly the way they already weight Religion's own Favor mechanics — no new personality layer introduced.
- **Villa:** the Atrium's existing *imagines* display (Villa §4.1) is now explicitly this document's flavor seat for the household's ancestral record, its existing small Dignitas trickle reframed as a visible expression of accumulated Memoria rather than an independent, unexplained bonus; the fuller Ancestor Gallery room remains deliberately deferred per direction.

---

## 9. Data Model

```
Household {
  // existing fields unchanged
  memoria: number,              // new — the third axis, alongside dignitas and (Religion's) favor
}

FuneralRecord {
  funeralId, deceasedCharacterId, householdId,
  tier,                    // "modest" | "proper" | "grand"
  imaginesDisplayed: bool,     // true only at grand tier, and only if real ancestral record exists
  laudatioDeliveredBy,          // characterId, nullable
  laudatioOutcome,            // "strong" | "adequate" | "poor" | null if none delivered
  burialMethod,              // "cremation" | "inhumation" — driven by §3's culture/faith logic
  interredAt,                // "familyTomb" | "mausoleum" | "columbarium" | "necropolis"
  month,
  memoriaGained, dignitasGained,
}

MourningPeriod {
  householdId, triggeringDeathId,
  startMonth, endMonth,
  type,                    // "household" | "widow" | "iustitium"
  brokenEarly: bool,           // true triggers Scandal §4
}

ManesObservance {
  householdId,
  parentaliaObservedThisYear: bool,
  lemuriaObservedThisYear: bool,
  consecutiveYearsObserved,        // drives the steady Memoria trickle
}
```

---

## 10. Open Questions

- **All numeric sizing deferred, per convention** — Funeral Tier costs, *luctus*/*tempus lugendi* exact durations, Memoria gain/loss magnitudes, and the *iustitium* Prominence gate are all unsized.
- **Memoria's exact decay curve relative to Favor's.** Both are neglect-driven third axes; whether Memoria should decay faster, slower, or identically to Favor when ignored isn't yet specified.
- **Columbarium's status as a distinct Building entry.** Currently treated as a flavor variant of the existing Family Tomb/Necropolis rather than a new constructible — worth revisiting once Buildings gets its own balancing pass, in case a dedicated Columbarium entry (with its own modest capacity/cost) is worth adding for settlements with a large freedman/Collegia Funeraticia population.
- **The Ancestor Gallery Villa room**, per direction, remains fully deferred to a future Villa-doc pass rather than resolved here.
- **Cross-cultural funerary variety.** This document's specific rites (*Parentalia*, *Lemuria*, the *pompa funebris*) are the Roman State default; a genuinely thorough pass would give at least the major non-Roman cultures (Cultures of the Known World) their own distinct funerary custom rather than having every household observe Roman rites regardless of culture — flagged here rather than assumed.
- **Whether a household's Memoria should ever be directly displayed to the player as a number versus only ever read qualitatively** (the way Favor gets a real numeric readout but this document's prose leans harder on "the family's memory feels thin" framing) — a UX question outside this document's own scope.
