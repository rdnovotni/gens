# GENS — System Design: Scandal (§6.31, new)
*The shared engine this project has been quietly building pieces of for a long time without ever assembling them: Traits already has a Scandal-Marked/Rehabilitated trait pair, Dynasty Chronicle already has a "Faith & Scandal" category, Characters already has a "Spread a Damaging Rumor" Interaction, and Legal & Court already describes "a case that can't win but can still hurt" generating exactly this kind of mark. This document is where all of that finally connects — the same relationship Events has to the many systems that already fed it random and scripted content before it existed to formalize the pattern.*

---

## Contents

1. Scope & Role — Assembling What Already Existed in Pieces
2. Real Roman Concepts — Fama, Libellus Famosus, Nota Censoria
3. The Scandal Record
4. Sources of Scandal
5. The Rumor Mill — How a Scandal Actually Spreads
6. Severity and Scope
7. Consequences — Including the Paradox
8. Damage Control
9. The Scandal Lifecycle
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role — Assembling What Already Existed in Pieces

A discovered affair, an Unjust execution, a Scandalous theatrical performance, a Fabricated Justification exposed after the fact, a beloved gladiator's public disgrace — this project has independently built the moment each of these things becomes *known* several times over, in several different documents, without ever formalizing what happens once that knowledge actually becomes public. This document is that missing formalization: not a new consequence system, but the shared engine a discovered secret, a botched cover-up, or an act of raw power actually resolves through once it stops being a private matter and becomes a real, talked-about public fact.

This document doesn't replace any existing discovery mechanic — Characters' own Scheme-discovery math (§10 of that document), Romance's own affair-discovery scaling (§11 of that document), Legal & Court's own case-resolution process all stay exactly as built. What this document adds is the layer sitting immediately *after* discovery: how far the news actually travels, how severe it reads to different audiences, what a household can actually do about it, and how long it actually lasts.

---

## 2. Real Roman Concepts — Fama, Libellus Famosus, Nota Censoria

Three real, historically grounded concepts anchor this document, one of which is already this project's own etymological root for a system it's built extensively:

- **Fama** — the actual Latin word this project's own Fame mechanic (Games & Spectacle §2, Celebrities & Influential Figures §6.22.1) descends from, and, tellingly, the same word covers both "renown" and "rumor" in Latin — Romans themselves didn't draw a hard line between being famous and being talked about, which is exactly why this document and the Fame system are close cousins rather than unrelated mechanics (§7).
- **Libellus Famosus** — a real, historically attested practice: anonymous defamatory pamphlets or verses, posted publicly or circulated privately, targeting a specific person's own reputation — Rome's own genuine equivalent of a tabloid exposé, and a real, concrete mechanism for a Scandal to originate anonymously rather than always being traceable to a specific accuser.
- **Nota Censoria** — a real, formal Roman institution this document gives genuine mechanical teeth: the Censor's own historical authority to formally mark, and in the most severe cases expel, a Senator for public disgrace, entirely independent of any criminal conviction. This is this document's own extreme, rare, formal consequence tier (§7), distinct from and more severe than an ordinary social Scandal.

---

## 3. The Scandal Record

A Scandal is a real, discrete, named record — not a passive Dignitas modifier quietly applied in the background, but a specific, dated incident with its own visible lifecycle (§9): who it's about, what it concerns, how it became known, how severe it currently reads, and how far it has actually spread. This gives the many individual discovery moments this project has already built a shared, comparable shape, the same way Events gave every system's own individual random/scripted content one shared taxonomy and delivery mechanism.

---

## 4. Sources of Scandal

Every source below is a real, already-existing moment somewhere in this project — this document adds no new discovery mechanic, only the shared aftermath layer:

- **A high-stakes affair's discovery** (Romance, Sexuality & Lineage §11) — the clearest, most direct source.
- **An Unjust imprisonment or execution** (Crime & Punishment §4, §8) — power exercised without a real Punishable Offense behind it.
- **A discovered Fabrication** (Crime & Punishment §9) — retroactively the single worst-case scandal source this project has built, since it proves the underlying action was knowingly dishonest.
- **A Scandalous theatrical performance** (Games & Spectacle §7.3) — the existing audience-reception tier of that name, now given its own real downstream Scandal Record when the reception is genuinely severe rather than merely provocative.
- **A Fame Collapse via public disgrace** (Celebrities & Influential Figures §7) — a courtesan's affair turning genuinely ugly, a poet's public humiliation.
- **A politically-weaponized Legal & Court case** (that document's §6) — a case brought with no real chance of winning, purely to generate exactly this kind of public airing.
- **An Illicit Collegium's exposure** (Collegia & Guilds §7) — a patron's own public association with a dissolved, disgraced collegium.
- **Aggressive tax-farming corruption exposed** (Land Ownership & Real Estate §8) — the real Publicanus scandal risk that document already names.
- **A deliberately weaponized rumor** — Characters' own existing "Spread a Damaging Rumor" Interaction (§9.4 of that document), the one source on this list that's a deliberate player or NPC action rather than an accidental discovery, and this document's own concrete resolution mechanism for it.

---

## 5. The Rumor Mill — How a Scandal Actually Spreads

A Scandal's own spread isn't instant or uniform — it moves through real, already-existing channels rather than a new standalone diffusion model:

- **Ambient spread** — Settlement Demographics' own aggregate population and Notable Households' own sampled residents (Notable Households §2) are this document's own real "crowd" carrying the news locally, the same population Celebrities & Influential Figures already names as Fame's own audience (§4 of that document) — a Scandal is, in a real sense, negative Fame moving through the identical social channel.
- **Distant spread** — Correspondence & Letters' own News & Gossip action (§5 of that document) is this document's own concrete mechanism for a Scandal reaching an audience too far away for ambient spread alone, with the same real transit delay that document already models for anything else it carries.
- **Deliberate acceleration** — a Libellus Famosus (§2) or a targeted "Spread a Damaging Rumor" Interaction can push a Scandal further and faster than its own natural spread would achieve, at the real risk of the spreader being identified as the source if the attempt is itself discovered.

---

## 6. Severity and Scope

Two independent dimensions, not one combined score:

**Severity** — how bad the underlying matter actually reads, from a minor embarrassment (a socially awkward but not disgraceful incident) through a genuine public disgrace (an affair, a proven Fabrication, an Unjust execution) to the rare, maximal case warranting formal Nota Censoria consideration (§2, §7).

**Scope** — how far the Scandal has actually spread, from household-only (contained, not yet public) through settlement-wide (the ordinary default once ambient spread runs its course) to provincial or Rome-wide, gated by the same Prominence concept (Events §5) that already governs how visible a household is to the wider Roman world generally — a genuinely Prominent household's own Scandal is inherently harder to keep contained than an obscure one's identical misdeed.

---

## 7. Consequences — Including the Paradox

A Scandal's own consequences read differently depending on severity, scope, and — critically — the household's own existing Fame/Dignitas position (Celebrities & Influential Figures §2):

- **The ordinary case** — a real Dignitas penalty scaled to severity and scope, a relationship-web scar across everyone connected to the matter, and, for a sufficiently severe or public case, the **Scandal-Marked** Reactive Trait (Traits §6.10) applied directly — the concrete mechanical payoff Legal & Court's own document already named without fully specifying its own trigger.
- **Faction-dependent reception** — a Traditionalist audience and a Popularist one (Politics & Patronage §3.1) genuinely read the same Scandal differently, exactly the way that document's own existing Faction mechanic already colors every other politically-charged moment.
- **The real paradox, worth naming directly:** per Celebrities & Influential Figures §2's own Fame/Dignitas Divergence, a Scandal doesn't always reduce a Character's own Fame — for someone already occupying the "famous and disreputable" archetype (a gladiator, an actor, a courtesan), a fresh scandal can genuinely *raise* Fame even while further damaging Dignitas, a real, dark "any publicity is publicity" dynamic this document treats as an honest feature of that Divergence rather than smoothing it into a uniformly negative outcome.
- **Nota Censoria** — reserved for the rare, maximal-severity case involving a sitting Senator: a formal mark, and in the most extreme instances outright expulsion from the Senate, entirely independent of any criminal conviction — Politics & Patronage's own cursus honorum record (§6 of that document) takes a real, direct, and rare hit distinct from an ordinary lost election or lapsed term.

---

## 8. Damage Control

A household isn't purely passive once a Scandal exists — several real, historically plausible responses are available, each a genuine tradeoff rather than a free fix:

- **Suppression** — spending Influence (Politics & Patronage §4.4) or outright bribery (Economy & Finance §4.2) to slow or cap a Scandal's own spread before it reaches wider Scope — genuinely effective against ambient spread specifically, much less effective against something already carried by Correspondence to a distant audience.
- **Spin** — a funded public gesture (a Games & Spectacle event, a Funded Action per Economy & Finance §4.3) timed to compete for the crowd's own attention and goodwill, not erasing the Scandal but genuinely softening its felt severity.
- **Scapegoating** — a real, historically plausible and genuinely dark option: publicly attributing blame to a household dependent or an enslaved member rather than the actual responsible party, trading that dependent's own wellbeing and standing (a real, live Labor & Slavery or Familia consequence) for the household head's own protection — this document names it factually as a real available option without endorsing it, consistent with this project's own standing restraint around difficult material.
- **Rehabilitation** — the real, existing payoff for sustained good conduct following a Scandal: the **Rehabilitated** Reactive Trait (Traits §6.10), already named alongside Scandal-Marked as its own natural counterpart, now given this document's own explicit trigger condition — a real, sustained stretch without further incident, converting a lingering Scandal-Marked stigma into genuine, earned redemption.

---

## 9. The Scandal Lifecycle

Consistent with the same decay shape this project applies to Influence, Fame, and every other opinion-driven figure: an ordinary Scandal's own felt severity fades over time if not actively refreshed by a further incident, eventually settling into background Dynasty Chronicle memory rather than an active, ongoing penalty. A sufficiently severe Scandal — one that produced a Scandal-Marked Trait, a Nota Censoria, or a genuine Feud (Rival Houses §5.2) — leaves a **Faith & Scandal** category Dynasty Chronicle entry permanently, exactly the category that document already names, regardless of how much the Trait itself eventually fades through Rehabilitation.

---

## 10. Cross-System Integration

- **Traits:** Scandal-Marked and Rehabilitated (§6.10 of that document) are this document's own primary character-level output — this document formalizes their trigger and resolution conditions rather than introducing new Traits.
- **Dynasty Chronicle:** the existing Faith & Scandal category is this document's own permanent-record destination for any sufficiently severe case.
- **Characters:** Spread a Damaging Rumor (§9.4 of that document) is this document's own concrete deliberate-acceleration mechanism (§5); Scheme-discovery (§10 of that document) remains the upstream trigger for many Scandal sources, unchanged.
- **Legal & Court:** §6's own "a case that can't win but can still hurt" is this document's own direct precedent and confirmation case; this document is the general engine that specific example was always describing.
- **Romance, Sexuality & Lineage:** high-stakes affair discovery (§11 of that document) is this document's single clearest, most common Scandal source.
- **Crime & Punishment:** Unjust imprisonment/execution (§4, §8) and a discovered Fabrication (§9) are both direct Scandal sources, the latter this document's own worst-case example.
- **Games & Spectacle, Celebrities & Influential Figures:** a Scandalous performance and a Fame Collapse via disgrace are both direct sources; §7's own Fame/Dignitas paradox is inherited directly from that document's own Divergence concept.
- **Collegia & Guilds, Land Ownership & Real Estate:** an Illicit Collegium's exposure and a Publicanus corruption scandal are both direct sources.
- **Politics & Patronage:** Faction-dependent reception (§7) reuses that document's existing mechanic directly; Nota Censoria (§2, §7) is a new, rare, formal addition to that document's own cursus honorum record.
- **Settlement Demographics, Notable Households:** both are this document's own real "crowd" carrying ambient spread (§5).
- **Correspondence & Letters:** News & Gossip (§5 of that document) is this document's own concrete distant-spread mechanism.
- **Economy & Finance, Politics & Patronage:** Suppression and Spin (§8) both spend those documents' own existing Influence, bribery, and Funded Action mechanics directly.

---

## 11. Data Model

```
ScandalRecord {
  scandalId,
  primaryCharacterOrHouseholdId,
  sourceType,                     // "affairDiscovery" | "unjustAction" | "discoveredFabrication" |
                                     // "scandalousPerformance" | "fameCollapse" | "weaponizedLegalCase" |
                                     // "illicitCollegiumExposure" | "publicanusCorruption" | "deliberateRumor"
  severity,                        // "minorEmbarrassment" | "publicDisgrace" | "notaCensoriaEligible"
  scope,                            // "householdOnly" | "settlementWide" | "provincial" | "romeWide"
  originatedViaLibellusFamosus: bool,
  currentFameEffect,                 // can be negative or positive — §7's own paradox
  scandalMarkedTraitApplied: bool,
  notaCensoriaIssued: bool,
  factionDependentReception: { traditionalistReading, popularistReading },
  damageControlActionsTaken: [ { actionType, effectiveness } ],   // "suppression" | "spin" | "scapegoating"
  isActive: bool,                     // false once faded into pure Chronicle memory — §9
  dynastyChronicleEntryId,
}
```

---

## 12. Open Questions

- **All numeric sizing**, per this project's standing convention — Scandal decay rates, the ambient-spread-to-Scope threshold, and Suppression/Spin's own effectiveness curves are all unsized.
- **Nota Censoria's own actual trigger threshold.** §7 names it as rare and maximal-severity but doesn't specify the precise conditions distinguishing a Senate-shaking Scandal from an ordinary severe one.
- **Scapegoating's own moral and mechanical weighting.** §8 names it factually as a real option but doesn't specify whether it should carry its own distinct, harsher long-term consequence (a Loyalty collapse across the rest of the household, say) beyond the immediate protection it buys — left open rather than resolved here.
- **Multiple simultaneous Scandals.** Whether a household or Character can be tracking more than one active Scandal Record at once, and how their severities interact if so, isn't addressed.
- **Libellus Famosus's own traceability.** §2 and §5 name it as a real, anonymous origination method, but don't specify whether or how an investigation (Espionage, Legal & Court) could ever trace one back to its actual author.
