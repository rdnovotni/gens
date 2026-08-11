# GENS — System Design: Retirement & Old Age
*The formal definition behind two things this project has flagged but never fully built: Familia's own Elderly lifecycle stage, which names "retirement from active duties" as "an available choice rather than a forced one" without ever specifying what that choice actually triggers, and Succession & Dynasty's own Elder Statesman, introduced as a retired head who "remains a living, un-controlled Familia member" without giving that continued life any concrete mechanical shape. This document is both of those definitions, plus the real, genuinely rich texture Roman culture itself attached to old age — a civilization whose own governing body took its name directly from the word for "old man."*

---

## Contents

1. Scope & Role
2. The Elderly Stage — What Decline Actually Looks Like
3. Real Age-Linked Conditions
4. Venerable — Auctoritas and the Case for Age
5. The Decision to Retire
6. Refusing to Retire
7. The Elder Statesman — A Concrete Advisory Role
8. Grandparenting
9. Aging Companions & Officeholders
10. The Final Stretch — Legacy Ambitions, Recap
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role

Familia §3's own lifecycle table already names five real stages, the last of which — Elderly, 60+ — states plainly that "Core Attributes and Health gradually decline; wisdom-flavored traits become more likely; succession pressure and death risk both rise; retirement from active duties becomes an available choice rather than a forced one." Succession & Dynasty §6.1 takes the retirement half of that sentence one step further, establishing that a retired head "remains a living, un-controlled Familia member... whose opinion and Loyalty still matter, and who can plausibly fill an advisory Companion-style role." Neither document ever finished the thought. This document is that finish: a concrete decline model, a real trigger and mechanical shape for the Retire action itself, a genuine reason a Character might refuse to take it, and an actual, playable definition of what an Elder Statesman or Materfamilias *does* with the rest of their life rather than simply existing as a name on the household record.

**What doesn't move here:** Familia's own five-stage lifecycle table, Succession & Dynasty's Handoff mechanic (§6.1, death vs. retirement), and Character Ambitions' own Legacy category (§3.7) all stay exactly as written. This document fills the gap between them.

---

## 2. The Elderly Stage — What Decline Actually Looks Like

Familia's own lifecycle table names the direction of decline without ever specifying its shape. This document supplies that shape directly, in terms this project already has rather than a new stat:

- **Core Attributes** (Stewardship, Martial, Diplomacy, Intrigue, and the rest) drift downward gradually across the Elderly stage rather than dropping sharply at the moment of turning 60 — a real, slow curve rather than a cliff edge, consistent with how every other gradual process in this project (Soil Fertility, Regional Unrest) is already modeled.
- **Physique tier** (Traits §3.3) trends toward Frail over a sufficiently long Elderly stretch, absent a countervailing Herculean-tier Congenital roll or a Long-Lived Stock trait (§4.4 of that document) slowing the slide.
- **Health** becomes more easily lost and more slowly recovered, exactly the register Familia's own Hardy/Sickly trait pair already establishes, now compounding with age on top of whatever that Congenital roll already set.

**Infirmity — the slow-onset counterpart to Permanent Injury.** Familia §3.1 already establishes Permanent Injury as "a lasting, non-healing modifier" from a single traumatic event — a wound, an accident, a difficult birth. This document names the honest, gradual equivalent: **Infirmity**, a slow-accumulating standing penalty to a specific Core Attribute or Labor Skill that isn't the result of any single dramatic incident, simply the real, accumulated toll of a long life. Mechanically identical in shape to a Permanent Injury (a standing penalty, visible on the character record and in the Appearance system's own rendered portrait per that document's own §16), but earned through years rather than a moment.

---

## 3. Real Age-Linked Conditions

A small, real, and historically grounded set of specific Infirmities worth naming directly rather than leaving §2's mechanic entirely abstract:

- **Gout.** A real, extensively attested affliction of the wealthy Roman elite specifically, historically (if imprecisely, by modern medical understanding) linked to a rich diet heavy in meat and wine — a genuine, pointed irony this document names directly: Food Culture's own richest, highest-status Banquet options are, over a long enough life, a real contributing factor to exactly this real ailment. A concrete, small Stewardship/Martial penalty and a standing Health cost, worn as a real, visible mark of a life of privilege rather than hardship — the wealthy Elder's own honest counterpart to a laborer's more physically demanding decline.
- **Failing eyesight.** A real, ordinary consequence of age this project has already, correctly, declined to solve with anachronistic corrective eyewear (Garment Roster §4's own honest note that glasses didn't exist). The real historical adaptation is this document's own answer instead: an **Anagnostes** — a real, attested household position, a reader who read aloud to a household member whose own eyesight could no longer manage it comfortably — is a small, natural addition to Companions & Court Positions' own staff roster, and a genuine, humane, and period-accurate mechanical response to this specific Infirmity rather than a flat penalty with no in-world remedy at all.
- **Addled.** Already a real, standalone Trait in this project's own catalog (Traits §6.2) — "age/injury/disease-driven cognitive decline" — this document doesn't duplicate it, only confirms it as the Elderly stage's own most common single Infirmity outcome, and the real, mechanical reason a household should think carefully before leaving a genuinely Addled Character in a position of real consequence (§6).

---

## 4. Venerable — Auctoritas and the Case for Age

A real, worth-naming linguistic and cultural fact this project hasn't yet used: the Roman **Senate** — *Senatus* — takes its own name directly from *senex*, "old man." Roman political culture didn't merely tolerate age in its leadership; it was structurally and culturally built around the idea that age conferred real, earned authority — *auctoritas* — a genuine, positive counterweight to §2's own honest physical decline, and this document names a new, standalone Trait to carry it: **Venerable**.

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| **Venerable** | Rationality + | A real, Elderly-stage-gated bonus to advice-giving (§7), to being heeded in family and political Interactions specifically, and a small, genuine Dignitas bonus reflecting real Roman deference to age — standalone, no opposed pair, the positive counterpart Addled's own existing entry never had. *"Doesn't raise his voice anymore. Doesn't need to — the room already goes quiet."* |

**The real historical tension worth keeping, not resolving away:** the Roman cursus honorum itself carried real, formal minimum ages for its highest offices (a consul historically needed to have reached his early forties at the least) — meaning the very system that gated real political power behind experience and age was, at the exact same moment, handing that power to people whose own Core Attributes might already be well into §2's own decline curve. This document doesn't smooth that tension away: a household's own aged Consul or Provincial Governor is a genuinely real, historically apt picture — real authority and real capability don't always arrive in the same body at the same time, and that's a feature of this system worth keeping rather than a bug worth fixing.

---

## 5. The Decision to Retire

The concrete definition behind Familia's own flagged-but-undefined choice. **Retire** is a real, player-initiated action available to any Character once they've entered the Elderly stage, and it comes in two genuinely different shapes:

- **Full Retirement** — stepping back from household headship entirely, triggering Succession & Dynasty's own Handoff mechanic (§6.1 of that document) exactly as if the head had died, except that the retiring Character remains alive and un-controlled afterward, per §7 below.
- **Partial Retirement** — stepping back from a *specific* held office, command, or Senior Position (a magistracy, a Praefectus command, a Rationalis post) without surrendering full household headship — a real, meaningful distinction this project hasn't drawn before. A household head can, entirely plausibly, retire from active field command (Military & Combat §3.2) while remaining head of the family, or step down from a magistracy (Politics & Patronage §5) while continuing to run the estate day to day. Partial Retirement simply frees the specific position for reassignment — to an heir, a Companion, or a hired specialist — without triggering anything in Succession & Dynasty at all.

Either form is a genuine, real Dynasty Chronicle-eligible moment, matching the weight Succession & Dynasty already assigns Full Retirement specifically ("the torch passes... a real narrative beat, not a silent stat swap").

---

## 6. Refusing to Retire

The real, dramatic tension this document adds on top of the plain mechanical choice above: nothing forces a Character to actually take the Retire action once they're eligible for it, and several real, already-existing Traits give a Character genuine, in-character reason not to. A Proud, Power-Hungry, or sufficiently Stubborn head — or simply one who hasn't consciously registered their own decline — can and often will continue holding an office or a command well into real Infirmity (§2–3), and this document treats the real consequences honestly rather than softening them:

- **An infirm officeholder** genuinely performs worse at the office they're clinging to — Politics & Patronage's own magistracy resolution and Military & Combat's own commander-weighting inputs (§4.2 of that document) both read a Character's own current, declined Core Attributes directly, with no special exemption for age or past accomplishment.
- **A new, light Interaction — Urge Retirement** (Characters' own Family/Social category) — gives an heir, a spouse, or a concerned Companion a real, direct way to press the issue, resolved the same way any other Persuasion-leaning Interaction already is: the aging Character's own Stubborn/Pliant and Proud/Humble traits, and their relationship-web opinion of whoever's doing the asking, determine whether it actually lands.
- **This can, and sometimes should, go unresolved.** A household that never successfully urges its own aging, declining, Power-Hungry patriarch to step down is a genuine, real, and entirely legitimate outcome this document doesn't correct for — the same "the game never picks for the player" restraint already applied to Military & Combat's own Praefectus-versus-Roman-Service choice.

---

## 7. The Elder Statesman — A Concrete Advisory Role

The actual, mechanical answer to Succession & Dynasty's own "can plausibly fill an advisory Companion-style role" — no longer merely plausible, now defined:

- **A mentorship bonus.** A retired head (Full Retirement, §5) still living in the household provides a real, standing bonus to whoever now holds their former position — a retired Praefectus improving a new commander's own early Readiness training, a retired magistrate's own accumulated Clientela contacts smoothing their successor's first term, the concrete mechanical expression of "learning from someone who's actually done this before."
- **A real Advice Interaction**, reading the Elder Statesman's own accumulated Traits (Venerable chief among them, §4) and lifetime experience — a Character who consults them ahead of a major decision (a contested election, a military campaign, a marriage negotiation) receives a real, small improvement to that decision's own resolution odds, a concrete payoff for keeping an aged, retired head around and genuinely listened to rather than quietly sidelined.
- **A living bridge to the Chronicle.** Per Design Pillar #7, an Elder Statesman still alive in the household is a genuine, first-hand source for Dynasty Chronicle detail and family history in a way even the richest written record can't quite match — a real, human throughline the household's own younger members can actually sit down and talk to, right up until the point (Ancestor Veneration & Funerary Customs) they become a name on the wall instead.

---

## 8. Grandparenting

A small, warm, and mechanically light addition: an Elder Statesman or Materfamilias living alongside their own grandchildren provides a real, modest bonus to that grandchild's own Formative trait development (Traits §5, Familia's own Childhood/Adolescence window) — a doting or storytelling grandparent's own real, small, positive influence on a child's upbringing, distinct from and additive to whatever a parent or Paedagogus (Companions & Court Positions) is already providing. This is deliberately minor rather than a full parallel Education system — a nice, human piece of texture rather than a second mechanic competing for the same design space Familia's own Education investment already occupies.

---

## 9. Aging Companions & Officeholders

Nothing in this document is exclusive to a household's own bloodline. A long-serving Companion or hired specialist — a Praefectus, a Rationalis, a favored Overseer — ages and declines through exactly the same §2 curve, and can be offered the same Retire choice (§5), Partial Retirement being the far more common shape for a non-family appointee stepping back from their specific post rather than from "headship" of anything. A genuinely loyal, long-serving Companion who's earned a comfortable, respected retirement within the household rather than simply being replaced the moment their Stewardship dips is a small, real, and legitimate Loyalty-building gesture in its own right — the same logic already established for a freed slave's own continued household service (Labor & Slavery §8) extended to an aged free retainer instead.

---

## 10. The Final Stretch — Legacy Ambitions, Recap

This document deliberately doesn't redesign what Character Ambitions §3.7 already built for exactly this life stage — "See the Heir Settled," "Make Peace with a Rival," "Die Well-Remembered" remain that document's own Elder-lifecycle-gated capstone goals, and §10 of that same document already covers what happens to an Ambition left unfinished at death. This document's own contribution is simply the runway leading up to that point: a Character actually reaching Elderly, actually declining (§2), actually retiring or refusing to (§5–6), and actually being present, listened to, and useful (§7–8) for however many years remain before either death or a Legacy Ambition's own resolution closes the story.

---

## 11. Cross-System Integration

- **Familia:** this document is the direct, complete definition behind the Elderly lifecycle stage's own flagged "retirement... an available choice" line, and Permanent Injury (§3.1 of that document) is the direct structural template for §2's own new Infirmity mechanic.
- **Succession & Dynasty:** §5's Full Retirement is the concrete trigger for that document's own Handoff mechanic (§6.1); §7 is the complete, promised definition of that same section's own Elder Statesman reference.
- **Traits:** Venerable (§4) is a genuine new standalone addition to that document's own catalog, the positive counterpart Addled never had; Stubborn/Pliant and Proud/Humble are §6's own direct mechanism for whether an Urge Retirement Interaction actually lands.
- **Politics & Patronage, Military & Combat:** §4's own real historical age-minimum tension, and §6's own infirm-officeholder consequence, both read directly off those documents' existing magistracy-resolution and commander-weighting math with no special exemption.
- **Companions & Court Positions:** the Anagnostes (§3) is a small, real, and period-accurate new staff position; §9 extends this entire document to non-family Companions and appointees.
- **Food Culture:** Gout (§3) is a real, pointed, and honestly-earned consequence of that document's own richest Banquet options over a long enough life.
- **Character Ambitions:** §10 explicitly declines to redesign that document's own Legacy category (§3.7) or its Unfinished Ambition handling (§10 of that document), deferring to both directly.
- **Ancestor Veneration & Funerary Customs:** §7's "living bridge to the Chronicle" is the direct, honest predecessor state to that document's own eventual treatment of the same Character once they've died.
- **Characters:** Urge Retirement (§6) and the Advice Interaction (§7) are both new, small additions to that document's own Interaction Catalog, resolved through its existing Trait/Opinion machinery rather than a bespoke new resolution system.
- **Dynasty Chronicle (§6.11, future):** both Full and Partial Retirement (§5) are guaranteed, real milestone material, matching the weight Succession & Dynasty already assigns the moment.

---

## 12. Data Model

```
ElderlyDeclineProfile {                // §2
  characterId,
  coreAttributeDriftRates: { [attribute]: rate },
  physiqueTierTrendingToward,             // nullable — "frail", absent Herculean/Long-Lived Stock counterweight
  healthRecoveryPenaltyActive: bool,
}

InfirmityRecord {                       // §2, §3 — structurally parallel to Familia's own PermanentInjury
  characterId,
  infirmityType,                          // "gout" | "failingEyesight" | "generalFrailty" | "other"
  affectedAttributeOrSkill,
  penaltyMagnitude,
  onsetMonth,                              // gradual — no single triggering event, unlike PermanentInjury
}

RetirementRecord {                       // §5
  characterId,
  retirementType,                          // "full" | "partial"
  vacatedPositionOrOfficeId,                 // set for partial; null for full (handled via Succession & Dynasty instead)
  month,
  chronicleEligible: true,
}

UrgeRetirementInteraction {              // §6 — a specific instance of Characters' own Interaction Catalog
  interactionId,
  initiatorId, targetId,
  outcome,                                  // "accepted" | "refused" | "resentfulRefusal"
}

ElderStatesmanAdvisoryRole {             // §7
  retiredCharacterId,
  mentoredSuccessorCharacterId,               // nullable
  mentorshipBonusActive: bool,
  advisableForDecisionTypes: [ ... ],           // "election" | "militaryCampaign" | "marriageNegotiation" | ...
}

GrandparentingBonus {                    // §8
  grandparentCharacterId,
  grandchildCharacterId,
  formativeTraitBonusActive: bool,
}
```

---

## 13. Open Questions

- **All numeric sizing**, per this project's standing convention — Core Attribute drift rates, Infirmity penalty magnitudes, Venerable's own Dignitas/advice bonus, and the Urge Retirement Interaction's own success odds are all unsized.
- **Infirmity accumulation cap.** §2 doesn't specify whether a Character can accumulate more than one named Infirmity (§3) over a sufficiently long Elderly stage, or whether one is the practical ceiling before the broader decline curve simply takes over.
- **The Anagnostes's own formalization.** §3 suggests this new position for Companions & Court Positions' own roster without formally adding it there itself — consistent with how this document family has handled similar small suggested additions before (the Ornatrix, the Tonsor).
- **Whether Venerable can ever be lost.** §4 doesn't specify whether a genuinely disgraced or Broken (Labor & Slavery's own Punishment-ladder endpoint, in the rare case it applies) Elderly Character can lose an already-held Venerable trait, or whether it's permanent once earned.
- **Partial Retirement's re-engagement question.** §5 doesn't specify whether a Character who partially retires from a specific office or command can later un-retire and resume it, or whether the vacated position is considered permanently passed on.
- **Grandparenting's interaction with a Nursery-stage Villa room.** §8's bonus and Villa's own existing Nursery room (gated to Infant-stage family members) aren't explicitly reconciled — likely additive rather than competing, but not stated here.
