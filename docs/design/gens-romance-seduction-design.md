# GENS — System Design: Romance & Seduction (§6.19)
*Two tracks at once, exactly as the core doc frames it — a genuine relationship system with its own tracked emotional life, and a political tool that hands leverage to Espionage and alliance-building to Politics & Patronage. Built almost entirely on infrastructure Characters, Familia, and the Villa doc already put in place specifically for this. This pass adds an explicit, direct treatment of consent and power imbalance for any relationship touching Enslaved status, states plainly that seduction odds can never override genuine unwillingness, and makes the affair-to-Fertility pregnancy pipeline explicit rather than assumed.*

---

## Contents

1. Scope & Role
2. Affection & Attraction — Two New Tracked Scores
3. The Relationship Track — Courtship to Marriage
4. The Scheme Track — Seduction as Leverage
5. Characters Act on Their Own — Autonomous Romance
6. Affairs & Discovery
   6.1 Consent & Power
   6.2 Discovery
7. Historical Social Reception — Status and Role
8. Content Handling
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own framing: Romance & Seduction "operates on two tracks at once: a political scheme tool (seduction pursued for leverage, blackmail, alliance, or information, feeding Politics and Espionage) and a genuine relationship system (affection/attraction stats tracked alongside, not replacing, the transactional marriage market). Sexual content stays indirect per §9."

This is, once again, largely compositional. Characters already built the entire Romantic Interaction category (Flirt, Court/Woo, Seduce, Confess Feelings, Propose Marriage, Take as Lover/End Affair, Elope, and more, §9.2) and the Seduce Scheme type; Familia already wired affairs into Legitimacy (§5.2) and Divorce (§5.1) and named the consent/happiness factor that "raises the odds of Romance & Seduction's affair mechanics triggering against the marriage"; the Villa doc already flagged the Solarium and Exedra as favorable settings. This document's real job is the two things that don't already exist: the actual tracked Affection/Attraction scores the core doc calls for, and the autonomous layer that lets any Character — not just the player's — pursue a romantic life of their own.

---

## 2. Affection & Attraction — Two New Tracked Scores

Per the decision to build real new numbers rather than just read existing ones: every relationship-web pairing with any romantic dimension — a marriage, an active courtship, an ongoing affair — tracks two distinct 0–100 scores alongside its ordinary opinion figure, precisely so a marriage of convenience with real Affection but no Attraction (or the reverse) reads as a genuinely different story than one flat number could tell:

- **Affection** — the emotional bond: warmth, genuine care, trust. Distinct from ordinary relationship-web opinion, which can be high purely from political utility with no romantic warmth behind it at all.
- **Attraction** — physical and romantic desire, independent of Affection. A passionate but emotionally shallow affair runs on high Attraction and low Affection; a beloved, trusted companion the initiator simply isn't drawn to runs the other way.

Both feed from — and feed into — material this project already built rather than sitting in isolation: Congenital Lustful/Chaste and a Character's Beauty tier (Traits §3.2) weight Attraction directly; Formative temperament compatibility and shared Traits weight Affection; and actual Romantic Interactions (Flirt, Court/Woo) raise both over time, while neglect or a Rebuke lowers them. **This is also where the "behind the scenes" resolution actually happens**, per direction: a Seduce Scheme's real odds read the target's own Attraction toward the initiator specifically — not just the initiator's Diplomacy and the target's raw Chaste/Faithful resistance — so two attempts against the same target by two different initiators can have genuinely different odds even with identical stats, because the target simply likes one of them more.

---

## 3. The Relationship Track — Courtship to Marriage

Courtship Interactions (Flirt, Court/Woo, Confess Feelings) build Affection and Attraction over real time — Court/Woo is already specified as a light multi-stage Interaction (Characters §9.2), the natural home for this. A courtship that succeeds can lead to Propose Marriage as a genuine love-match (Familia §5's own alternate path to the transactional arranged model) or, where one party is already married, directly into affair territory (§6). This track is worth tracking in its own right regardless of whether it's ever politically weaponized — the core doc's explicit point that this is "not replacing" the marriage market, but running alongside it.

---

## 4. The Scheme Track — Seduction as Leverage

The political tool, reusing Characters' Seduce Scheme type directly rather than inventing a parallel mechanic. The core doc names four concrete uses, each with a real mechanism:

- **Leverage & Blackmail** — a successfully completed Seduce Scheme generates the same Blackmail Leverage bond tag (Characters §7) Espionage already spends. **This is also the concrete resolution to Espionage's own open question** ("does a seduced target become a Persistent Network placement automatically, or a distinct thing"): it's the latter — a successful seduction doesn't automatically enlist anyone, but it unlocks a new, favorably-weighted option to recruit the target as a spy (Espionage §2.2's Persistent Network) on the strength of their now-real Affection, rather than the seduction itself quietly becoming espionage.
- **Alliance** — a genuine romantic bond with a Rival House's Head or a notable member is a real, informal thumb on that house's Standing (Rival Houses §5.2) toward Allied, entirely distinct from and cheaper than a formal marriage contract — a real soft-power option a poorer or less politically connected house can still use.
- **Information** — a willing counterpart to Espionage's Interrogate: information given because Affection and Attraction are genuinely high, not extracted under pressure. Mechanically lighter than Interrogate, and unavailable at all without a real Affection/Attraction baseline already in place.

---

## 5. Characters Act on Their Own — Autonomous Romance

Per direction, given directly: "your son could sire a bastard, a guest or appointed person might have [a relationship] with another character, infidelity" — this is Characters §8.3's "Characters act on their own initiative" principle, given its Romance-specific texture rather than a new mechanism.

Any two Characters with sufficient mutual Affection/Attraction, compatible Traits, and real opportunity — living in the same household, meeting through Travel, an Event, or a hosted gathering — can independently initiate a Romantic Interaction with each other, entirely without the player choosing it. A household's Companions, Clientela, guests, and family members are a genuinely live simulation rather than static furniture: a son can father a child outside marriage, two Companions can begin an affair, a widowed head can take a lover, an appointed Overseer and a visiting guest can begin something that has nothing to do with the player at all. This surfaces through the Monthly Report and Dynasty Chronicle (tiered by stakes, per §6) rather than requiring the player to watch it unfold — and the player retains full intervention power (a *patria potestas*-backed disapproval, arranging a pre-emptive marriage, a direct confrontation) without ever being required to exercise it. **This is the actual mechanism that populates Familia's own Fertility/Childbirth system (§6 of that doc) with illegitimate children** — an affair or an autonomous romance rolls pregnancy chance through the exact same Fertility-driven math a marriage already uses, which is what makes Legitimacy (Familia §5.2) a real, live question rather than a purely theoretical one.

---

## 6. Affairs & Discovery

### 6.1 Consent & Power

Worth stating directly, consistent with how Labor & Slavery already treats its own subject matter: this document's mechanics assume rough parity of agency between the parties involved, which is true for a courtship between two free people but is never true between an owner and an enslaved member of their own household. A relationship-web bond involving an Enslaved-status individual and their own owner is never mechanically treated as an ordinary courtship between equals — the total power imbalance Labor & Slavery's Regimen system already tracks is the actual, honest frame for that dynamic, not this document's Affection/Attraction courtship math. Where genuine mutual Affection exists in such a pairing (a real, if historically fraught, possibility this project's frankness doesn't need to deny), the game reads it through that same power-imbalanced lens rather than pretending it away, and never lets it function as an escape from the underlying legal reality.

**A related principle worth being explicit about everywhere else, too:** a Seduce Scheme's odds are never a stat-driven override of a target's genuine unwillingness — a target with low Attraction, high Faithful/Chaste standing, and no real interest has correspondingly poor odds regardless of the initiator's own Diplomacy or Boldness. High stats make someone a more effective, more charming pursuer; they don't manufacture consent that isn't there.

### 6.2 Discovery

Per the decision to scale weight with actual stakes rather than treating every affair identically:

- **A minor dalliance** — no Rival House involved, no Legitimacy question, no politically important marriage at risk — resolves entirely through Characters' existing Scheme-discovery mechanics (§10 of that doc). A Discovered-and-Escalated outcome simply *is* the affair becoming known, feeding the Adulterous Reactive Trait and Heartbroken/Guarded for the wronged party, with no separate system required.
- **A high-stakes affair** — a Rival House Character is involved, a resulting child's Legitimacy is genuinely contested, or a politically important marriage is directly threatened — earns the fuller treatment: a real confrontation moment between the wronged party and the offender (or the third party), with a genuine choice rather than an automatic outcome — **Forgive** (a real relationship-web recovery path, the natural on-ramp to the Rehabilitated Reactive Trait), **Divorce** (Familia §5.1's mechanic, triggered formally rather than assumed), or **Challenge** (Characters §9.6's Duel, if the wronged party or a relative wants satisfaction). Where a Rival House is the other party, this can move House Standing sharply (Rival Houses §5.2) or trigger an outright Feud.

---

## 7. Historical Social Reception — Status and Role

Per the decision to make this historically textured rather than uniform, grounded in the actual, well-documented Roman framework rather than a modern one projected backward: elite Roman social judgment of a relationship centered on **status and role**, not a modern binary of partners' sexes. A citizen man's relations with a clearly lower-status partner — an enslaved person, a freedman, a prostitute — in the socially dominant role were unremarkable regardless of that partner's sex, and carried minimal Dignitas weight on their own. What carried real social risk was a citizen — of either sex — in a perceived socially subordinate role, or a liaison between two people of comparable free/citizen status (especially involving a freeborn Roman youth), since either implied a loss of the standing a person of that status was expected to maintain. This applied a real, if asymmetric, weight to certain pairings that a same-status heterosexual affair of comparable discretion didn't carry to the same degree — a genuine historical texture, not a modern judgment layered on top of it.

Mechanically: a **Dignitas modifier**, read directly off the relative Legal Status/Social Class of the two parties and which one is publicly perceived as occupying which role, applies on top of §6.2's ordinary discovery consequences — regardless of the specific sexes involved, and applied identically whichever sexes they are. Every pairing is written with the same narrative dignity and the same frank, non-judgmental description; only the mechanical Dignitas modifier — grounded in real status and role, not the fact of the pairing itself — carries the historical asymmetry.

---

## 8. Content Handling

Unchanged from the core doc's own standing rule (§9 of that document): sexual content stays indirect always — implied, faded to black, described the way a serious historical drama would, never depicted and never given its own mechanical resolution. Every mechanic in this document resolves the *relational, political, and social* consequences of a romance or affair — Affection/Attraction shifts, Legitimacy, Dignitas, discovery, House Standing — never the act itself.

---

## 9. Cross-System Integration

- **Characters:** the entire Romantic Interaction category and the Seduce Scheme type are reused directly; §5's autonomous romance is a direct application of §8.3's "Characters act on their own" principle.
- **Familia:** affairs, Legitimacy, and Divorce (§5.1-5.2) all get their full mechanism here rather than a forward reference; the arranged-marriage consent/happiness factor is this document's own Affection/Attraction baseline at the moment of marriage; §5's autonomous romance is the concrete mechanism populating that document's own Fertility/Childbirth system with illegitimate children.
- **Labor & Slavery:** §6.1's power-imbalance principle explicitly defers to that document's own Regimen/Loyalty framework for any owner-enslaved relationship, rather than treating it as ordinary courtship.
- **Education & Culture (§6.14, future):** a Character's Diplomacy-driven courtship effectiveness (§3) is a natural beneficiary of that system's own rhetoric/education investment once it's designed.
- **Espionage:** §4 resolves that document's own open question directly — a successful seduction is a favorable recruitment opportunity, not automatic enlistment.
- **Politics & Patronage:** an informal romantic alliance (§4) is a real, cheaper alternative to a formal marriage contract for moving a Rival House's Standing.
- **Rival Houses:** a cross-house romance or affair is this document's concrete contributor to House Standing shifts and, at the extreme, a Feud trigger (§6.2).
- **Traits:** Lustful/Chaste, Faithful/Adulterous, Infatuated/Disillusioned, Heartbroken/Guarded, and Beauty are all read directly by §2's Affection/Attraction math rather than needing new romance-specific tags.
- **Villa:** the Solarium and Exedra are this system's named physical settings, exactly as that document already flagged.
- **Legal & Court:** a contested Legitimacy case arising from a high-stakes affair (§6.2) is a direct Family-category case for that document.
- **Dynasty Chronicle:** a high-stakes affair's resolution, a notable love-match marriage, and any autonomous romance significant enough to matter are all real material, tiered by §6.2's own stakes-scaling.
- **Companions & Court Positions:** a Companion or appointed position-holder is exactly as reachable by §5's autonomous romance as any Familia member, per the direction's own example.

---

## 10. Data Model

```
RomanticBond {
  characterAId, characterBId,
  affection,        // 0-100
  attraction,        // 0-100
  bondType,          // "courtship" | "marriage" | "affair" | "pastRelationship"
  isKnownPublicly: bool,   // §6.2 — false for an undiscovered affair
  powerImbalanced: bool,     // §6.1 — true whenever either party is Enslaved-status relative to the other;
                              // read through Labor & Slavery's Regimen framework, never ordinary courtship math
}

AffairRecord {
  affairId,
  offenderCharacterId, thirdPartyCharacterId, wrongedSpouseId,
  stakesLevel,          // "minor" | "highStakes" — §6.2's scaling trigger
  involvesRivalHouse: bool, legitimacyContested: bool, threatensPoliticalMarriage: bool,
  resolution,          // "quietlyResolved" (minor only) | "forgiven" | "divorced" | "challenged"
  statusRoleDignitasModifier,   // §7 — applied regardless of the pairing's sexes
}
```

---

## 11. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Affection/Attraction growth and decay rates, the status/role Dignitas modifier's actual values, and autonomous-romance trigger frequency are all unsized.
- **Autonomous romance frequency tuning.** §5 establishes that any two sufficiently-disposed Characters can independently begin something; how often this should actually fire across a household full of Companions and Clientela, so it reads as a living world rather than constant noise, isn't decided.
- **Multiple simultaneous romantic interests.** Whether a Character can hold real Affection/Attraction toward more than one person at once, and how that resolves if it becomes known, isn't explicitly addressed.
- **Same-sex marriage's own legal standing.** §7 addresses social reception in detail; whether Familia's own marriage mechanics (§6.1) ever recognize a same-sex union as a formal legal marriage, versus this document's relationship track being the only avenue available to such a pairing, is left to that document's own future revision if needed.
