# GENS — System Design: Crime, Punishment & Imprisonment (§6.25, new)
*Built explicitly on CK3's own justice-and-imprisonment model, per direction: imprisonment, execution, torture, and ransom are all real tools available to whoever holds authority over a character — not locked behind a formal Legal & Court verdict — but a character's own history of actual **Punishable Offenses** determines whether using those tools reads as justice or as an unjust abuse of power, with consequences scaled accordingly. This document owns the punishment catalog in real historical depth (including the honestiores/humiliores two-tier system Legal & Court's own short sentence list only gestured at) and introduces Detention as a genuine new tracked status, while leaving Legal & Court's own trial, evidence, and verdict process untouched.*

---

## Contents

1. Scope & Role — Power First, Legitimacy Second
2. The Justice Spectrum — From Verdict to Vengeance
3. Punishable Offense — What Opens the Door
4. The Imprison Action — Anyone Reachable, Any Time, Real Consequences
5. Detention — A Real Tracked Status
6. Interrogation & Torture Under Duress
7. The Punishment Catalog — Honestiores and Humiliores
8. Execution and Its Consequences
9. Fabricating Justification — A New Scheme Type
10. Ransom and Release
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role — Power First, Legitimacy Second

Per direction, this document is built on the same premise *Crusader Kings III* uses for its own justice system: a ruler — or, here, a *paterfamilias*, a presiding magistrate, or anyone else holding real authority over a character — can imprison, execute, or otherwise act against nearly anyone reachable, at essentially any time. The action itself is never the gate. What actually matters, and what this document spends most of its own depth on, is whether that action is **justified** — a real, documented Punishable Offense on the target's own record (§3) — or whether it's a naked exercise of power the rest of the world reads, correctly, as unjust (§4, §8).

This deliberately doesn't duplicate Legal & Court (§6.16), which remains the *formal* path: filing a case, gathering evidence, a Hearing, a Ruling. This document is the *personal and extrajudicial* layer sitting alongside it — the same relationship CK3's own dungeon mechanics have to its formal justice actions — and gives Legal & Court's own thin four-item sentence list (fine, exile, debt bondage, execution) the real historical depth and breadth it never had room for.

---

## 2. The Justice Spectrum — From Verdict to Vengeance

A single continuum, most to least legitimate, each point carrying progressively less built-in protection against backlash:

1. **A formal Legal & Court verdict** — the cleanest path. A real Hearing, a real Ruling, a sentence handed down by a recognized magistrate. Carries the least risk of an "unjust" reading regardless of how severe the sentence actually is, because the process itself supplies the legitimacy.
2. **Direct exercise of household or magisterial authority** — this document's own primary territory (§4). A *paterfamilias* imprisoning, exiling, or even executing someone within their own authority, or a magistrate acting outside a formal Hearing, justified by a real Punishable Offense (§3) rather than a court's own Ruling. Faster and more personal than §1, but only as legitimate as the offense actually backing it.
3. **Proscription** (Policies & Edicts §5.7) — fully extralegal, broad, and, per that document's own note, carrying a real demonstration effect across every Rival House watching, not just the target.
4. **Private vengeance** — a Feud's own violence (Rival Houses §6.2), a Duel (Characters §9.6), or an outright Coercive Interaction escalation, with no legal fig leaf at all.

This document's own consequence math (§4, §8) reads a character's chosen action against where it actually sits on this spectrum, not just against the outcome — the same sentence carries a genuinely different Dignitas and relationship-web cost depending on whether it arrived via a Hearing or via a household head simply deciding it was so.

---

## 3. Punishable Offense — What Opens the Door

A new, formally tracked flag on a character's own record, built from real events this project's other systems already generate rather than a new detection mechanic:

- A Legal & Court **conviction**, of any severity.
- A **Discovered-and-Escalated Scheme** (Characters §10) — a caught seduction, a caught assassination attempt, a caught act of sabotage.
- A **high-stakes affair's own discovery** (Romance, Sexuality & Lineage §11) where the outcome genuinely implicates the target in adultery under real Roman law.
- A **Piracy & Banditry** capture, or a Military & Combat captive taken in a genuinely hostile action.
- A **fabricated** offense (§9) — mechanically identical to a real one once it's on the record, which is precisely the point.

A character with a real, standing Punishable Offense can be imprisoned, exiled, or executed with minimal Dignitas or relationship-web cost — the offense itself supplies the justification the rest of the world accepts. A character with no such record who suffers the same treatment triggers §4's own full unjust-action consequences instead. This is the actual mechanical answer to direction's own framing: "you could execute someone but there may be consequences, especially if unjust" — the Punishable Offense flag is what separates the two outcomes.

---

## 4. The Imprison Action — Anyone Reachable, Any Time, Real Consequences

A new Interaction, broadly available rather than gated behind a court process, targetable at anyone within the actor's own real reach: a household member or dependent (via *patria potestas*), a Client (via Clientela authority), a captured rival (via Military & Combat or Piracy & Banditry), or, for a sitting magistrate, anyone within their own jurisdiction pending a Hearing.

**Resolution:**
- **Justified** (a real Punishable Offense on record, §3) — the action proceeds with minimal fallout: a small, expected relationship-web cost from the target's own family, but no broader Dignitas penalty and no Faction-style reaction from uninvolved parties.
- **Unjust** (no real offense on record) — a genuinely severe consequence, scaled to the target's own Dignitas and standing: a direct Dignitas hit to the actor, a serious relationship-web scar with the target's entire family rather than just the target, a real chance of triggering a Rival Houses Feud if the target belongs to one, and, for a sufficiently prominent or sympathetic target, a broader reputational cost among Clientela and allied houses generally — the same "everyone is watching how you use power" reaction CK3's own tyranny system models, built here from this project's own existing Dignitas and relationship-web machinery rather than a new imported stat.

---

## 5. Detention — A Real Tracked Status

Per direction, primarily historical: the real Roman *carcer* was overwhelmingly a place to hold someone **before** a trial or an execution, not a facility for serving a long sentence the way a modern prison works — Rome simply didn't use extended incarceration as an ordinary criminal punishment. This document keeps that real historical shape as its own default, while still giving Detention genuine mechanical weight rather than treating it as an instant, invisible transition.

A **Detained** status is tracked explicitly, distinct from Enslaved:
- **Duration** — genuinely open-ended for someone awaiting a Legal & Court Hearing or a ransom negotiation (§10), but this document doesn't model an ordinary criminal sentence as "X months of imprisonment then release" the way a modern system would; a resolved case moves a Detained character to whichever real outcome §7 actually specifies instead.
- **Location** — the real public **Carcer**, a new settlement-level building (Estate & Settlement's own public-building family, alongside the Forum and Baths) for holding free persons awaiting trial, execution, or ransom; and the real private **Ergastulum**, a household-level building for confining enslaved individuals specifically, giving Labor & Slavery's own existing flight-prevention and punishment mechanics a real physical structure to anchor to rather than an abstraction.
- **Escape risk**, mirroring Labor & Slavery's own flight/recapture math (that document's §7) directly rather than inventing a parallel system: Loyalty, conditions, and opportunity determine risk; a genuine escape attempt and pursuit resolve exactly as that document already specifies.
- **A real, historically honest exception for the game's own "some imprisonment" allowance, per direction:** a Detained status can genuinely persist for a real, extended stretch specifically while a major Legal & Court case (that document's §5) runs its own multi-stage course, or while a ransom negotiation (§10) plays out — this is the honest, historically grounded shape "incorporate some imprisonment" actually takes here, rather than a standalone "sentenced to prison" penalty that wouldn't reflect real Roman practice.

---

## 6. Interrogation & Torture Under Duress

Per direction not to shy away from this. A real, specific, and genuinely severe fact of Roman legal practice: testimony from an enslaved person was, by real Roman legal convention, only considered valid if extracted under torture (*quaestio per tormenta*) — free testimony from an enslaved witness carried no legal standing at all in a formal case. This document names that fact directly rather than softening it, and gives it a real mechanical home: interrogating a Detained enslaved witness for Legal & Court testimony (that document's §8) proceeds through this document's own Torture resolution rather than an ordinary Request Testimony check, reflecting the real legal requirement rather than treating it as an optional cruelty.

For a free Detained character, Torture is a real, available but far more serious option — genuinely risking the interrogator's own Dignitas and, if the target turns out to have no real Punishable Offense justifying their detention in the first place, compounding directly into §4's own unjust-action penalty on top of the act itself. Torture resolves as information extracted (real, if the target actually knows something, or false, if they don't and simply say what stops the pain — a real, honest limitation this document doesn't pretend away) rather than a guaranteed truth-generator.

---

## 7. The Punishment Catalog — Honestiores and Humiliores

The real, historically documented core of this document, per direction: by this game's own later range especially, Roman law drew an increasingly formal distinction between **honestiores** (senators, equestrians, decurions, soldiers, and other persons of real established standing) and **humiliores** (everyone else — ordinary free citizens, freedmen, foreigners) — the same crime drawing genuinely different sentencing options depending purely on which tier the convicted person belonged to.

**Honestiores' own real sentencing range:**
- **Fine** — Legal & Court's own existing default.
- **Relegatio** — a real, milder exile: temporary or permanent removal from Rome or a specific province, but citizenship and most property retained.
- **Deportatio** — a real, harsher exile: permanent, to a specific fixed location (often an island), with real, substantial property confiscation and loss of citizenship.
- **Ignominia** — loss of rank, office, or senatorial/equestrian status itself, without physical punishment.
- **The honorable exit** — a real, genuinely attested practice: an honestiore facing a near-certain capital verdict was often permitted, or even quietly expected, to take their own life before sentence was formally carried out, preserving family Dignitas and protecting the estate from the fuller confiscation a formal execution carried. This document treats this as a real, available, dignified resolution option distinct from an ordinary execution, carrying its own distinct, generally lighter Dignitas outcome for the family left behind.

**Humiliores' own real, considerably harsher sentencing range:**
- **Flogging** — a real, common corporal punishment for lesser offenses.
- **Forced labor / *damnatio ad metalla*** — condemnation to the mines, already established as a real Legal & Court sentencing option (Starting Regions: Iberian Colony §3) — this document confirms it as humiliores-tier specifically.
- **Servus poenae** — a real, specific Roman legal category: a free person condemned to certain severe penalties was legally *reduced to slavery* as part of the sentence itself, a genuinely severe and historically real outcome distinct from debt bondage's own separate mechanism (Economy & Finance §6.4).
- ***Damnatio ad bestias*** — condemnation to the beasts, already established in Games & Spectacle (§4 of that document) as a real, humiliores-and-enslaved-tier execution method, resolved there exactly as that document already specifies rather than redefined here.
- **Crucifixion** — per direction, named factually and without flinching: a real, historically significant Roman execution method, specifically associated with humiliores and enslaved persons, and, per real Roman law (the *lex Porcia* and related statutes), only very rarely applied to citizens at all. This document treats it with the same restraint every other harsh system in this project already uses — real, played straight, described with narrative purpose, never lingered on for its own sake.

**A further, real, and genuinely severe historical rule specifically touching Labor & Slavery:** the real *Senatus Consultum Silanianum* required, by law, the execution of every enslaved person residing under the same roof as a master who was murdered, if the actual killer wasn't identified — a real, formally attested Roman legal rule, not an invention. This document names it directly as a legitimate, high-stakes Legal & Court/Crime crisis scenario: a household head's own unexplained murder puts the entire enslaved household at real legal risk until the true culprit is found, giving Espionage's and Legal & Court's own investigation tools a genuinely urgent, high-stakes reason to matter beyond the ordinary case.

---

## 8. Execution and Its Consequences

Execution itself — by whatever method §7 specifies for the condemned's own tier — resolves through the same Justified/Unjust lens §4 already establishes, scaled up to its natural maximum severity: a Justified execution of someone with a real, severe Punishable Offense on record carries real but contained fallout (grief from the condemned's own family, nothing broader); an Unjust execution is this document's own single most severe consequence-generating event, carrying a serious Dignitas collapse, a near-guaranteed Feud trigger if the condemned belonged to a Rival House, and a real, lasting mark on the acting household's own Dynasty Chronicle regardless of how the rest of the playthrough goes. Per direction's own explicit framing, the player retains full freedom to execute anyone reachable at any time — this document's whole point is making sure that choice always carries a real, honestly-scaled weight rather than a free, consequence-less action.

---

## 9. Fabricating Justification — A New Scheme Type

A direct CK3 parallel, per direction to look at that game's own model: a new Scheme type, built on Characters' existing Scheme engine (§10 of that document) rather than a parallel mechanism, that manufactures a Punishable Offense record for a target who hasn't actually earned one — false testimony purchased or coerced, physical evidence planted, a confession extracted from an unrelated party under Torture (§6). A successful Fabrication resolves identically to a real Punishable Offense for the purposes of §3 and §4's own Justified/Unjust check — the rest of the world can't tell the difference, which is the entire reason a player or an NPC would use it. A **discovered** Fabrication, however, is this document's own single worst-case outcome: it retroactively converts the original action into a *provably* unjust one, applying §4's and §8's own full penalty on top of a further, severe Dignitas cost for the fabrication itself, and typically producing a Dynasty Chronicle entry that follows the household for a long time.

---

## 10. Ransom and Release

A real, common Roman practice this document formalizes: a Detained captive of sufficient standing — a captured Rival House member, a hostage taken during a raid or a Frontier conflict — can be ransomed rather than tried, executed, or held indefinitely. A Ransom negotiation runs through Economy & Finance's own existing offer/counter-offer machinery, sized to the captive's own Dignitas and their house's own wealth, and its resolution (paid, refused, or bargained down) is a real, direct Rival Houses Standing event in its own right — a successfully ransomed captive returning home is a real, concrete goodwill gesture; a refused or excessively harsh demand reads as its own kind of provocation. **Release** without ransom — simple mercy — is always available too, and, consistent with §4's own logic, reads as a genuine, positive relationship-web and Dignitas event precisely because it was never required.

---

## 11. Cross-System Integration

- **Legal & Court:** this document's own punishment catalog (§7) directly replaces and substantially deepens that document's own short four-item sentence list; §5's Detention gives that document's own "major case" multi-stage process (§5 of that document) a real, tracked status for the accused during the interim; §6's torture-testimony rule gives that document's own Testimony & Evidence section (§8) its real historical mechanism for enslaved witnesses specifically.
- **Characters:** §9's Fabrication is a new Scheme type built on that document's own existing engine; §4's Imprison is a new Interaction alongside that document's own existing catalog.
- **Labor & Slavery:** the Ergastulum (§5) gives that document's own flight-prevention and punishment mechanics a real physical building; the Senatus Consultum Silanianum (§7) is a direct, high-stakes extension of that document's own stated commitment to frank treatment of slavery's harshest realities.
- **Estate & Settlement:** the public Carcer (§5) is a new settlement-level building alongside the Forum and Baths.
- **Games & Spectacle:** *damnatio ad bestias* (§7) is inherited directly from that document rather than redefined.
- **Policies & Edicts:** §2 places Proscription (§5.7 of that document) explicitly on this document's own Justice Spectrum, one step further from legitimacy than this document's own direct Imprison/Execute actions.
- **Rival Houses:** an Unjust imprisonment or execution (§4, §8) is a genuine, high-weight Feud trigger; a ransomed captive (§10) is a real, direct Standing event.
- **Romance, Sexuality & Lineage:** a discovered high-stakes affair (that document's §11) is a real, direct Punishable Offense source (§3); the real *lex Julia de adulteriis*'s own extreme legal remedy (that document's §12) is this document's own honestiores/humiliores-adjacent case of a father or husband exercising personal, extrajudicial punishment authority rather than going through a formal Hearing.
- **Economy & Finance:** Ransom (§10) runs through that document's own offer/counter-offer machinery directly; *servus poenae* (§7) is distinct from and doesn't replace that document's own separate debt bondage mechanism.
- **Dynasty Chronicle:** every execution, every Fabrication (discovered or not), and every ransom resolution is real material, tiered by this document's own Justified/Unjust distinction.
- **Military & Combat, Piracy & Banditry:** a captured commander or raider is this document's own natural Detained-status entry point, feeding directly into §10's own Ransom mechanic.

---

## 12. Data Model

```
PunishableOffense {
  characterId,
  source,                   // "legalConviction" | "discoveredScheme" | "discoveredAffair" |
                              // "militaryCapture" | "piracyCapture" | "fabricated"
  severity,                  // qualitative tier, feeding §7's own sentencing range
  isFabricated: bool,        // §9 — mechanically identical to real until/unless discovered
  fabricationDiscovered: bool,
}

ImprisonAction {
  actorCharacterId, targetCharacterId,
  authorityBasis,            // "patriaPotestas" | "clientelaAuthority" | "militaryCapture" |
                              // "piracyCapture" | "magisterialJurisdiction"
  justified: bool,           // §3/§4 — read directly off an active, undiscovered-as-fabricated PunishableOffense
  outcome,                    // "detained" | "executed" | "exiled" | "released" | "ransomed"
}

DetentionRecord {             // §5 — new tracked status, distinct from Enslaved
  characterId,
  locationType,               // "publicCarcer" | "privateErgastulum"
  startMonth,
  linkedLegalCaseId,           // nullable — ties to an in-progress Legal & Court major case
  escapeRisk,                  // derived per Labor & Slavery §7's own formula
  tortureAppliedForTestimony: bool,   // §6
}

SentenceRecord {               // §7 — replaces Legal & Court's own short sentence enum
  characterId, tier,           // "honestiores" | "humiliores"
  sentenceType,                // honestiores: "fine" | "relegatio" | "deportatio" | "ignominia" | "honorableExit"
                                 // humiliores: "flogging" | "damnatioAdMetalla" | "servusPoenae" |
                                 //             "damnatioAdBestias" | "crucifixion"
  wasJustified: bool,           // §4/§8
}

RansomNegotiation {
  captiveCharacterId, capturingHouseholdId, targetHouseholdId,
  amountOffered, amountCountered,
  resolution,                  // "paid" | "refused" | "bargainedDown" | "mercyReleaseNoRansom"
  rivalHousesStandingDelta,
}
```

---

## 13. Open Questions

- **All numeric sizing**, per this project's standing convention — the Justified/Unjust Dignitas and relationship-web deltas, escape-risk thresholds, Ransom pricing, and every severity tier's own exact weighting are all unsized.
- **Whether the "honorable exit" (§7) should be offered as a real, explicit player choice**, or should instead resolve as a plausible AI-driven outcome for an NPC facing the same circumstances without the player ever selecting it directly.
- **The exact threshold separating a "minor" fabrication attempt from one severe enough to itself count as a Punishable Offense if the fabricator is later caught (§9).** This document establishes the consequence but not the precise line.
- **Small-household Ergastulum access.** §5 assumes any household wealthy enough to own the building can use it; whether a smaller household without one has any private detention option at all, or must rely entirely on the public Carcer, isn't specified.
- **The Senatus Consultum Silanianum's own trigger conditions (§7).** This document names the real legal rule and its dramatic stakes but doesn't specify exactly how "the true killer is found" resolves mechanically — likely a direct Legal & Court investigation, but the specific investigation-to-resolution pipeline is left to a future pass.
- **Torture's own real reliability curve (§6).** This document states plainly that false information is a real possible outcome, but doesn't specify the actual probability split between true and false results, or what raises/lowers it.
