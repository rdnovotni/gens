# GENS — System Design: Romance, Sexuality & Lineage (§6.19, FINAL)
*Final polish and expansion pass. This document absorbs [Romance & Seduction](gens-romance-seduction-design.md) (§6.19) in its entirety and folds in Familia's own Fertility & Childbirth section (§6 of that document), so every mechanic touching courtship, sex, pregnancy, and their social and legal consequences lives in one place. The organizing principle, per direction, is explicitly CK3-style: the overwhelming majority of a household's romantic and sexual life happens in the background, autonomously, among characters the player never directly controls — surfaced through the Monthly Report and Dynasty Chronicle, escalating to a real player-facing decision only when the stakes genuinely warrant it. Nothing here is ever depicted directly; every mechanic resolves relational, political, legal, and biological consequence, never the act itself. This pass adds real historical texture (affectio maritalis, the manus/sine manu marriage distinction, concubinage's second real motivation), a concrete worked example grounding the background-simulation principle, and — the most important addition in this pass — an explicit statement that the game's Adult-stage lifecycle gate is a deliberate ethical override of real Roman legal practice on minimum marriage age, not an application of this project's own general historical-accuracy default. Two exclusions remain load-bearing and non-negotiable throughout: nothing in this document ever applies to a character below the Adult lifecycle stage, and no relationship touching Enslaved status is ever read through this document's own courtship math rather than Labor & Slavery's power-imbalance framework.*

---

## Contents

1. Scope & Role — The Background-Simulation Principle
2. The Two Hard Exclusions
3. Affection & Attraction — Two Tracked Scores
4. The Relationship Track — Courtship to Marriage
   4.1 What Actually Made a Marriage — *Affectio Maritalis*
5. Same-Sex Relationships — Historically Grounded, Never Legally Married
6. Concubinage — The Real Legal Middle Ground
7. The Scheme Track — Seduction as Leverage
8. Autonomous Romance — The Household Simulates Itself
   8.1 A Worked Example — One Household's Own Season
9. Pregnancy, Fertility & Childbirth
10. Legitimacy
11. Affairs & Discovery
12. Real Roman Law — Adultery and Its Consequences
13. Status, Role & Infamia
14. Content Handling
15. Cross-System Integration
16. Data Model
17. Open Questions

---

## 1. Scope & Role — The Background-Simulation Principle

Per direction, this document's whole organizing idea is the same one *Crusader Kings III* uses for its own web of schemes, affairs, and seductions: most of it happens whether or not the player is watching. A household full of family members, Companions, Clientela, and guests is a genuinely live simulation, not static furniture — sons father children outside marriage, Companions begin affairs with each other, a widowed head takes a lover, and the player learns about most of it after the fact, through the Monthly Report or a Dynasty Chronicle entry, rather than by being asked to adjudicate every pairing personally. The system escalates to a real, explicit player decision only when the stakes genuinely justify it — a Rival House's own member is involved, a child's Legitimacy is contested, a politically important marriage is directly threatened — the same stakes-scaling principle §11 already applies to Discovery specifically, now stated as this entire document's own default posture rather than one section's local rule.

This document also formally absorbs Familia's own Fertility & Childbirth section (§6 of that document) — pregnancy was always a direct, mechanical consequence of the exact relationships this document tracks, and keeping it in a separate document created an artificial seam between cause and effect. Familia itself is updated to point here rather than carrying its own copy.

---

## 2. The Two Hard Exclusions

Stated once, prominently, rather than buried inside a later section, because everything else in this document depends on both holding without exception:

- **Lifecycle gating, deliberately overriding real historical practice.** Every mechanic in this document — Affection/Attraction, courtship, the Seduce Scheme, autonomous romance, pregnancy — requires both participants to have reached the Adult lifecycle stage (Familia §3). A Child or Adolescent character is never a valid participant in any interaction this document defines, under any circumstance, regardless of trait rolls, Scheme success, or autonomous-romance targeting. This is a hard gate enforced at the data layer, not a tendency the numbers happen to produce. **This document states directly, rather than leaving it to inference, that this gate is a deliberate ethical override of real Roman legal practice, not an application of it.** Real Roman law set minimum legal marriage ages far below what any responsible modern treatment should model or reference in mechanical or numeric detail, and this project does not do so anywhere in this document, its data model, or any future content built on it — the Adult lifecycle threshold stands regardless of what real Roman law actually permitted, the same way this project already declines to reproduce other real historical content it judges harmful to model directly. Historical accuracy is this project's own general default, stated repeatedly across every other document; it is not the operative value here, and this document does not treat the two as in tension needing resolution — child safety wins automatically and without exception.
- **Power imbalance with Enslaved status.** A relationship-web bond involving an Enslaved-status individual and their own owner — or anyone holding direct authority over them — is never read through this document's Affection/Attraction courtship math. It is read through Labor & Slavery's own Regimen and power-imbalance framework, in full, every time. Where genuine mutual Affection exists in such a pairing, a real if historically fraught possibility this project's own frankness doesn't pretend away, it is still never treated as an escape from the underlying legal reality, and it never grants the enslaved party agency the Regimen system doesn't otherwise give them. This document's own courtship, seduction, and pregnancy mechanics simply do not activate for such a pairing; Labor & Slavery's contubernium bond (§9 of that document) is the correct, honest mechanism for a lasting relationship between two enslaved individuals, and remains that document's own territory, not this one's.

A related principle threaded through every mechanic below: **a Seduce Scheme's odds are never a stat-driven override of a target's genuine unwillingness.** A target with low Attraction, high Chaste or Faithful standing, and no real interest has correspondingly poor odds regardless of the initiator's own Diplomacy, Boldness, or any other stat. High stats make someone a more effective, more charming pursuer; they do not manufacture consent that isn't there, for any pairing, under any circumstance.

---

## 3. Affection & Attraction — Two Tracked Scores

Every relationship-web pairing with any romantic dimension — a marriage, an active courtship, an ongoing affair — tracks two distinct 0–100 scores alongside its ordinary relationship-web opinion figure, so that a marriage of convenience with real Affection but no Attraction (or the reverse) reads as a genuinely different story than one flat number could tell:

- **Affection** — the emotional bond: warmth, genuine care, trust. Distinct from ordinary relationship-web opinion, which can be high purely from political utility with no romantic warmth behind it at all.
- **Attraction** — physical and romantic desire, independent of Affection. A passionate but emotionally shallow affair runs on high Attraction and low Affection; a beloved, trusted companion the initiator simply isn't drawn to runs the other way.

Both read from material this project already built: Congenital Lustful/Chaste and a character's Beauty tier (Traits §3.2) weight Attraction directly; Formative temperament compatibility and shared Traits weight Affection; actual Romantic Interactions (Flirt, Court/Woo) raise both over time, while neglect or a Rebuke lowers them. This is also where a Seduce Scheme's real odds actually resolve — reading the target's own Attraction toward the initiator specifically, not just the initiator's Diplomacy and the target's raw resistance traits, so two attempts against the same target by two different initiators can have genuinely different odds with identical stats, because the target simply likes one of them more.

---

## 4. The Relationship Track — Courtship to Marriage

Courtship Interactions (Flirt, Court/Woo, Confess Feelings) build Affection and Attraction over real time — Court/Woo is a light, multi-stage Interaction (Characters §9.2), the natural home for this. A courtship that succeeds leads to Propose Marriage as a genuine love-match — Familia's own alternate path to its transactional arranged-marriage model (Familia §5) — or, where one party is already married, directly into affair territory (§11). This track is worth tracking in its own right regardless of whether it's ever politically weaponized: a genuine relationship system running alongside, not replacing, the marriage market.

**Arranged marriage**, per Familia §5, is calculated from dowry, alliance value, and family prestige, layered with a real consent/happiness factor derived from pre-existing relationship-web opinion and personality-trait compatibility. Low consent doesn't block the marriage — Roman marriage didn't require it — but it depresses long-term happiness and raises the odds of this document's own affair mechanics triggering against the marriage rather than in support of it. **Love-match** marriage instead lets a relationship reach a high mutual Affection/Attraction organically before formalizing it, typically at a lower guaranteed dowry/alliance value but with genuinely higher starting happiness — a real tradeoff, not a dominant strategy either way.

### 4.1 What Actually Made a Marriage — *Affectio Maritalis*

A real, genuinely useful detail for how this document's own mechanics should read: Roman marriage, for most of this game's own range, required no ceremony, no state registration, and no formal document to be legally real. What constituted a marriage was **affectio maritalis** — the real, mutual, ongoing intent of both parties to be married, evidenced in practice by cohabitation and public treatment of one another as spouses. A real, formal betrothal (*sponsalia*) commonly preceded marriage, sometimes marked by the real historical exchange of an iron betrothal ring (*anulus pronubus*), but the wedding celebration itself, however elaborate, was a real social and religious observance rather than the actual legal act of marrying. This document treats a Love-match's own formalization (§4) as the natural mechanical expression of this real practice: the relationship *becoming* a marriage once both Affection/Attraction and mutual intent are established, rather than a marriage requiring a separate bureaucratic step layered on top. Familia's own dowry and property mechanics additionally distinguish, per real Roman law, between **manus** marriage (moving a wife fully under her husband's own legal authority) and the increasingly common **sine manu** form (letting her remain under her birth family's own authority and retain her own property) — a real, significant legal fork this document flags for Familia's own dowry system to reflect, without redefining that system's own territory here.

---

## 5. Same-Sex Relationships — Historically Grounded, Never Legally Married

Per Familia's own existing determination (§2.9 of that document): same-sex relationships exist as their own real, mechanically weighted track within this document, entirely independent of the heir-producing legal marriage market. This isn't an exclusion — it's historical accuracy. Roman marriage as a legal institution was built specifically around producing legitimate heirs and managing dowry and property between one man and one woman; there was no real legal category for a same-sex union to occupy, and this document doesn't invent one where none existed. What it does instead is give such relationships the exact same Affection/Attraction tracking, the same courtship track, the same weight in autonomous romance, and the same narrative dignity as any other pairing — the relationship is fully real and fully modeled, it simply never plugs into Familia's own marriage/Legitimacy mechanics, because those mechanics are specifically about heirs and dowry in a way this kind of pairing historically wasn't.

**What a same-sex relationship *can* do instead, all real and all already available elsewhere in this project's own design:** a genuine, durable Affection/Attraction bond carries the same informal Alliance value with a Rival House that any romantic bond does (§7); the couple can share a household and Villa life exactly as any cohabiting pair would; and, most significantly, formal continuity of a household's own line remains available through the existing Adoption mechanic (Familia §6.9), a real, historically well-attested Roman practice for exactly this kind of succession question, using the same Core Attribute and relationship-web evaluation any marriage candidate receives.

A real, well-documented historical figure is worth naming directly, with the same restrained, factual-only treatment this project already extends to every Named Historical Figure (Events: Historical Timeline Content §6.5): Hadrian's own real, deeply documented relationship with Antinous, and his real, genuine grief and formal deification of Antinous after his death, is a concrete, dignified illustration that such relationships carried real emotional and even religious weight at the very top of Roman society, not a marginal curiosity. This document names the real biographical facts and stops there, exactly as this project's own standing rule for historical figures requires — no invented dialogue, no dramatized private moments.

---

## 6. Concubinage — The Real Legal Middle Ground

A new tracked relationship type this document adds, resolving a real gap between marriage and an affair: Roman law recognized **concubinatus**, a genuine, legally distinct, lesser relationship, often used precisely where one partner's status made formal marriage impossible or undesirable. A real, concrete case this document names directly: the actual *lex Julia et Papia* (Augustus's own real marriage legislation) barred senators specifically from marrying freedwomen, actresses, or others of comparably low status — a real, historically attested legal restriction, not an invented one — making concubinage the honest, legally recognized alternative for exactly this kind of cross-status pairing.

Mechanically, a **Concubine** bond sits between Spouse and Affair on the relationship-web's own bond-tag spectrum: publicly acknowledged rather than hidden (unlike an affair), carrying real Affection/Attraction tracking (§3) and real pregnancy risk (§9), but never producing a Legitimacy claim the way a marriage does (§10) and never itself the subject of a Divorce (Familia §5.1) — it simply ends when either party chooses, or when a marriage supersedes it. This gives Legal & Court's own senatorial-marriage restriction genuine mechanical teeth: a character barred from marrying a specific partner by real status law isn't simply blocked, they have a real, historically honest alternative path available.

A further real, honest motivation worth naming: concubinage wasn't only used where marriage was legally barred. A real, historically attested pattern saw men — including some who were already widowers past the age of wanting more legitimate heirs — choose a concubine specifically *because* the relationship carried no Legitimacy or inheritance complications, a deliberate simplicity rather than a lesser substitute forced by circumstance. This document treats both motivations as equally legitimate reasons for the bond to exist.

---

## 7. The Scheme Track — Seduction as Leverage

The political tool, built directly on Characters' existing Seduce Scheme type. Four concrete uses:

- **Leverage & Blackmail** — a successfully completed Seduce Scheme generates the same Blackmail Leverage bond tag (Characters §7) Espionage already spends.
- **Recruitment** — the resolution to Espionage's own prior open question: a successful seduction doesn't automatically enlist anyone, but unlocks a new, favorably-weighted option to recruit the target as a spy (Espionage §2.2's Persistent Network) on the strength of their now-real Affection, rather than the seduction itself quietly becoming espionage.
- **Alliance** — a genuine romantic bond with a Rival House's Head or a notable member is a real, informal thumb on that house's Standing (Rival Houses §5.2) toward Allied, distinct from and cheaper than a formal marriage contract.
- **Information** — a willing counterpart to Espionage's Interrogate: information given because Affection and Attraction are genuinely high, not extracted under pressure, and mechanically lighter than Interrogate, unavailable without a real Affection/Attraction baseline already in place.

---

## 8. Autonomous Romance — The Household Simulates Itself

This is §1's own background-simulation principle given its concrete mechanism. Any two Adult characters (§2) with sufficient mutual Affection/Attraction, compatible Traits, and real opportunity — living in the same household, meeting through Travel, an Event, or a hosted gathering — can independently initiate a Romantic Interaction with each other, entirely without the player choosing it. A household's Companions, Clientela, guests, and family members are a genuinely live simulation: a son can father a child outside marriage, two Companions can begin an affair, a widowed head can take a lover, an appointed Overseer and a visiting guest can begin something that has nothing to do with the player at all.

This surfaces through the Monthly Report and Dynasty Chronicle, tiered by stakes exactly as §11 scales Discovery, rather than requiring the player to watch it unfold in real time — and the player retains full intervention power (a *patria potestas*-backed disapproval, arranging a pre-emptive marriage, a direct confrontation) without ever being required to exercise it. This is the actual mechanism that populates §9's own pregnancy system with children conceived outside marriage, which is what makes §10's own Legitimacy question a real, live one rather than a purely theoretical concern.

### 8.1 A Worked Example — One Household's Own Season

Concretely, in a single game-year inside a mid-sized household, the background simulation might independently resolve: a widowed cousin and a visiting Companion develop mutual Affection over several hosted dinners and quietly become lovers, surfacing as a single Monthly Report line since neither is married and no Rival House is involved; a son, away at a provincial posting, fathers a child with a local woman, surfacing as a slightly heavier Report entry once Familia's own Fertility math resolves the pregnancy, with Legitimacy (§10) left as an open question for the player to act on or ignore; and a married daughter's own long, mutual Affection with a family friend finally becomes physical, only escalating to a real, player-facing decision point once her husband's own suspicion crosses Characters' Scheme-discovery threshold and the affair becomes a genuine high-stakes case (§11) because the friend in question belongs to a Rival House. Three romantic threads, three different weights, only the last one ever actually stopping the player's own attention on it — the same density and the same escalation logic *Crusader Kings III* uses for its own web of court intrigues, built here from mechanics this project had already mostly assembled elsewhere.

---

## 9. Pregnancy, Fertility & Childbirth

*(Absorbed from Familia §6 in full — Familia now points here rather than carrying its own copy.)*

- Fertility (a Core Condition stat, Familia §2.3) combined with an active marriage, concubinage, or affair relationship determines pregnancy chance per relevant time tick — read through the exact same Fertility-driven math regardless of which of this document's own relationship types produced it, per §8's own direct mechanism.
- Pregnancy and childbirth carry real, period-appropriate stakes by default: a health cost during pregnancy, and a genuine risk of death at childbirth for the mother — moderated by Health, and improved by Learning-driven medical care, tying into Education & Culture and the Court Physician position — and a separate risk for the infant.
- A **player-configurable toggle**, set at game start alongside this document's own other content-intensity choices (§14), lets a player dial this down to a more abstracted, lower-risk mode without removing the fertility system entirely — a legitimate accessibility axis, not a softening of the setting's default frankness.

---

## 10. Legitimacy

*(Absorbed from Familia §5.2.)* A child's legitimacy is tracked explicitly rather than assumed: children born within a recognized marriage are legitimate by default; children resulting from an affair or an autonomous romance (§8) are not, unless the *paterfamilias* explicitly acknowledges and legitimizes them — a deliberate, visible choice with its own social cost (a Dignitas risk, a relationship-web hit from the betrayed spouse) rather than a quiet toggle. Legitimacy status directly gates default eligibility in Succession & Dynasty: illegitimate children aren't barred from ever inheriting, but require the same explicit intervention (acknowledgment, or Adoption) a legitimate heir doesn't need. Per §6, a child born of a Concubine relationship follows this same illegitimate-by-default rule, with the identical acknowledgment path available.

---

## 11. Affairs & Discovery

Consent and power are addressed once, comprehensively, in §2 — this section covers what happens once an affair exists and becomes known, scaled to actual stakes rather than treating every case identically:

- **A minor dalliance** — no Rival House involved, no Legitimacy question, no politically important marriage at risk — resolves entirely through Characters' existing Scheme-discovery mechanics (Characters §10). A Discovered-and-Escalated outcome simply *is* the affair becoming known, feeding the Adulterous Reactive Trait and Heartbroken/Guarded for the wronged party, with no separate system required.
- **A high-stakes affair** — a Rival House character is involved, a resulting child's Legitimacy is genuinely contested, or a politically important marriage is directly threatened — earns fuller treatment: a real confrontation between the wronged party and the offender or third party, with a genuine choice rather than an automatic outcome — **Forgive** (a real relationship-web recovery path, the natural on-ramp to the Rehabilitated Reactive Trait), **Divorce** (Familia §5.1, triggered formally), or **Challenge** (Characters §9.6's Duel, if the wronged party or a relative wants satisfaction). Where a Rival House is the other party, this can move House Standing sharply (Rival Houses §5.2) or trigger an outright Feud. §12 below gives this its own real legal dimension beyond the purely relational one.

---

## 12. Real Roman Law — Adultery and Its Consequences

A new addition this pass, giving §11's own high-stakes discovery outcome real legal teeth through Legal & Court rather than leaving it purely social. Augustus's own real *lex Julia de adulteriis* is named directly and treated with the same frankness this project extends to its other difficult historical material:

- **A formal accusation** could be brought before a real, dedicated standing court, distinct from an ordinary private dispute — this document's own new Legal & Court case type, available specifically when a high-stakes affair (§11) is discovered.
- **A father's real, narrowly limited legal right** to kill his own daughter and her lover, if caught together in his own house or her husband's, under specific, historically narrow conditions — and a husband's own more limited version of the same right — are named factually as real historical legal fact, available as an extreme, rarely-exercised resolution option alongside Forgive/Divorce/Challenge, carrying severe, guaranteed consequences of its own (a Dignitas and relationship-web reckoning regardless of the legal justification, since exercising it was itself a serious, watched act even when legally permitted).
- **Conviction's real standard consequence** was *relegatio* — formal exile, typically to an island, with real partial property confiscation — available as Legal & Court's own resolution to a formally prosecuted case, distinct from and more severe than an informal Divorce.

This document names these real legal facts plainly, consistent with the project's own standing commitment to historical frankness without gratuitousness — described with narrative purpose, never lingered on, and always resolving as a relational, legal, or Dignitas consequence rather than any depicted content.

---

## 13. Status, Role & Infamia

Elite Roman social judgment of a relationship centered on **status and role**, not a modern binary of partners' sexes. A citizen man's relations with a clearly lower-status partner — an enslaved person, a freedman, a prostitute — in the socially dominant role were unremarkable regardless of that partner's sex, and carried minimal Dignitas weight on their own. What carried real social risk was a citizen, of either sex, in a perceived socially subordinate role, or a liaison between two people of comparable free/citizen status, especially involving a freeborn Roman youth, since either implied a loss of the standing that status was expected to maintain. Mechanically: a **Dignitas modifier**, read directly off the relative Legal Status/Social Class of the two parties and which one is publicly perceived as occupying which role, applies on top of §11's ordinary discovery consequences — regardless of the specific sexes involved, and applied identically whichever sexes they are. Every pairing is written with the same narrative dignity; only this status-and-role-grounded modifier carries the historical asymmetry.

**Infamia**, a real, specific Roman legal status, is this document's own new addition tying directly into Familia's Legal Status system: prostitutes (working out of Buildings' own existing Brothel), actors, and gladiators alike carried this real, formal loss of certain legal protections and rights — the concrete legal consequence of occupying one of the socially subordinate roles named above as a profession rather than an isolated incident. This document names the connection directly rather than leaving the Brothel building's own social cost implicit.

---

## 14. Content Handling

Unchanged from this project's own standing rule: sexual content stays indirect always — implied, faded to black, described the way a serious historical drama would, never depicted and never given its own mechanical resolution. Every mechanic in this document resolves the *relational, political, legal, and biological* consequences of a romance, affair, or pregnancy — Affection/Attraction shifts, Legitimacy, Dignitas, Legal & Court proceedings, discovery, House Standing, birth outcomes — never the act itself. A player-configurable content-intensity toggle, part of the same settings family as §9's own fertility/childbirth-risk toggle and Familia's historical-restrictions toggle, lets a player soften narration further without changing any underlying mechanic or consequence.

---

## 15. Cross-System Integration

- **Familia:** this document formally absorbs that document's own §5.2 (Legitimacy) and §6 (Fertility & Childbirth) — that document now points here rather than carrying its own copy; §2.9's same-sex relationship determination is inherited directly rather than redefined; §4.1's manus/sine manu distinction is flagged for that document's own dowry and property mechanics to reflect.
- **Characters:** the entire Romantic Interaction category and the Seduce Scheme type are used directly; §8's autonomous romance is a direct application of that document's own §8.3 "Characters act on their own initiative" principle.
- **Labor & Slavery:** §2's power-imbalance exclusion explicitly defers to that document's own Regimen framework for any owner-enslaved pairing, and to its own contubernium bond (§9 of that document) for a lasting relationship between two enslaved individuals — neither is this document's own territory.
- **Legal & Court:** §12 gives that document a real new case type (formal adultery prosecution) and a real historical legal-restriction hook (the *lex Julia et Papia*'s senatorial marriage bar, §6); §13's Infamia is a direct, concrete addition to that document's own Legal Status framework.
- **Buildings:** the Brothel's own social cost, previously implicit, is now explicitly tied to §13's Infamia status.
- **Espionage:** §7 resolves that document's own prior open question directly — a successful seduction is a favorable recruitment opportunity, not automatic enlistment.
- **Politics & Patronage, Rival Houses:** §7's informal romantic alliance is a real, cheaper alternative to a formal marriage contract for moving a Rival House's Standing; a cross-house romance or affair is this document's own concrete contributor to House Standing shifts and, at the extreme, a Feud trigger (§11).
- **Traits:** Lustful/Chaste, Faithful/Adulterous, Infatuated/Disillusioned, Heartbroken/Guarded, Rehabilitated, and Beauty are all read directly by §3's Affection/Attraction math and §11's discovery outcomes.
- **Villa:** the Solarium and Exedra remain this document's own named physical settings for courtship.
- **Events:** Hadrian and Antinous (§5) join Events' own Named Historical Figures Roster, factual-only, per that document's own standing treatment.
- **Dynasty Chronicle:** a high-stakes affair's resolution, a notable love-match marriage, a formal adultery prosecution, and any autonomous romance significant enough to matter are all real material, tiered by §11's own stakes-scaling.
- **Companions & Court Positions:** a Companion or appointed position-holder is exactly as reachable by §8's autonomous romance as any Familia member.

---

## 16. Data Model

```
RomanticBond {
  characterAId, characterBId,
  affection, attraction,          // 0-100 each — §3
  bondType,                        // "courtship" | "marriage" | "concubinage" | "affair" | "pastRelationship"
  isSameSex: bool,                 // flavor/reporting only — never gates eligibility for any mechanic in this document
  isKnownPublicly: bool,           // §11 — false for an undiscovered affair
  powerImbalanced: bool,           // §2 — true whenever either party is Enslaved-status relative to the other;
                                    // when true, this document's own courtship/pregnancy mechanics never activate —
                                    // read instead through Labor & Slavery's own Regimen and contubernium framework
}

PregnancyRecord {                  // absorbed from Familia §6
  motherCharacterId, fatherCharacterId, conceivedViaBondType,
  legitimacyStatus,                 // "legitimate" | "illegitimate" | "acknowledged" — §10
  maternalRiskResolved, infantRiskResolved,
  contentIntensityToggleApplied: bool,
}

AffairRecord {
  affairId,
  offenderCharacterId, thirdPartyCharacterId, wrongedSpouseId,
  stakesLevel,                      // "minor" | "highStakes" — §11's own scaling trigger
  involvesRivalHouse: bool, legitimacyContested: bool, threatensPoliticalMarriage: bool,
  resolution,                       // "quietlyResolved" (minor only) | "forgiven" | "divorced" | "challenged" |
                                     // "prosecutedAdultery" | "extremeLegalRemedyExercised" — §12
  statusRoleDignitasModifier,       // §13 — applied regardless of the pairing's sexes
}

InfamiaStatus {                     // §13 — new Legal Status addition
  characterId,
  source,                            // "prostitution" | "acting" | "gladiatorial" | "convictedAdultery"
  legalProtectionsLost: [ ... ],
}
```

---

## 17. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Affection/Attraction growth and decay rates, the status/role Dignitas modifier's actual values, autonomous-romance trigger frequency, and Infamia's exact legal-protection loss list are all unsized.
- **Autonomous romance frequency tuning.** §8.1's worked example illustrates the intended texture and density, but the actual trigger rates behind it — how often this should fire across a household full of Companions and Clientela so it reads as a living world rather than constant noise — remain a future balancing question.
- **Multiple simultaneous romantic interests.** Whether a character can hold real Affection/Attraction toward more than one person at once, and how that resolves if it becomes known, isn't explicitly addressed.
- **The extreme legal remedy's own actual trigger conditions and frequency (§12).** This document names the real historical legal fact and gives it a resolution slot, but deliberately doesn't specify how often, or under what narrow AI-driven conditions, an NPC would actually exercise it — this is intentionally left rare and consequence-heavy pending a future balancing pass rather than tuned now.
- **Concubinage's own interaction with existing dowry mechanics.** §6 establishes the bond type and its Legitimacy default, but not whether any dowry-adjacent exchange is expected or modeled for it the way a marriage's own dowry is.
- **Manus versus sine manu's own mechanical depth (§4.1).** This document flags the real legal distinction and its relevance to Familia's own dowry/property system, but doesn't specify that system's own implementation — left to a future Familia pass.
