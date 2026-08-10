# GENS — System Design: Graffiti, Dynamic Walls & Rumors (§6.56, new)
*Polish pass. A real, genuinely rich vein of ancient evidence this project hadn't drawn on: the walls of real Roman cities — Pompeii's own extraordinarily well-preserved streets chief among them — survive covered in real, ordinary ancient handwriting: political endorsements, insults, boasts, love declarations, gladiator win tallies, shop advertisements, scribbled lines of Virgil, and simple "I was here" markers. This pass adds that last, genuinely charming real category directly, splits out Circus faction graffiti as its own real, distinct thing, resolves the standing regional-variance question with a light, real linguistic layer, gives a particularly witty or significant piece of graffiti a real chance to outlive its own origin as local color, adds a real backfire risk for graffiti that overplays its hand, and closes the loop on how an illiterate population still knows what the walls say.*

---

## Contents

1. Scope & Role — Real Walls, Real Voices
2. The Real Categories of Ancient Graffiti
3. The Scriptor — Commissioned, Favorable Graffiti
4. Dynamic Generation — How the Walls React to the World
5. Reading the Walls — A Real Information-Gathering Action
6. Defamatory Graffiti — A New Scheme Type
7. Erasure — Fighting Back Against a Damaging Wall
8. Curse Tablets (*Defixiones*) — A Darker, Distinct Practice
9. Backfire — When a Wall Overplays Its Hand
10. Famous Graffiti — A Small, Real Kind of Memory
11. Density, Language & Literacy — Who Actually Reads a Wall
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role — Real Walls, Real Voices

Social Places already noted, in passing, that real surviving graffiti attests to conversation at Rome's own public latrines. This document is where that offhand fact becomes a real, full system: a settlement's own walls carry real, dynamically-generated **Graffiti** — reflecting live Scandal, an active Election, a beloved gladiator, a hated rival — readable by the player as a genuine, ambient information source, and, per §3 and §6, directly writable by the player too, for better or worse. Graffiti sits alongside Correspondence & Letters' own gossip channel as a second, distinct real information-propagation medium: gossip is spoken and private; graffiti is written, public, and durable, and those two real properties are what this whole document is built around.

---

## 2. The Real Categories of Ancient Graffiti

All genuinely, extensively attested in the real archaeological record, most famously at Pompeii — expanded this pass with two further real, distinct categories:

| Category | Real Grounding | Ties |
|---|---|---|
| **Electoral Endorsements (*Programmata*)** | The single most numerous real category — professionally painted wall notices urging support for a specific candidate | Politics & Patronage's own contested elections |
| **Personal Insults & Scandal** | Real, ordinary ancient wall-writing calling out a named individual's alleged misdeeds | Scandal |
| **Romantic Declarations** | A real, extensively attested genre, handled with this project's own standing indirect approach | Romance, Sexuality & Lineage |
| **Games & Gladiator Commentary** | Real surviving graffiti records actual gladiator win/loss tallies and genuine fan opinion | Games & Spectacle, Celebrities & Influential Figures |
| **Circus Faction Graffiti** *(new)* | Distinct from ordinary gladiator commentary — real, genuine fan identification with a specific racing color/faction, an early real form of the intense faction culture that would later become famous in the Byzantine world | Games & Spectacle's own Circus content specifically |
| **Commercial Notices** | Real shop advertisements and rental notices | Notable Businesses |
| **Literary Quotation** *(new)* | A real, genuinely charming ancient practice — ordinary literate Romans really did scrawl lines of Virgil and other poets on walls, sometimes as genuine appreciation, sometimes as a literacy exercise, sometimes simply to show off | Books & Manuscripts, Education & Culture, Language & Literacy — real, direct evidence of a settlement's own literacy reach |
| **Simple Presence Markers** | The real, universal "so-and-so was here" | Pure flavor |

---

## 3. The Scriptor — Commissioned, Favorable Graffiti

A real, genuinely attested professional trade: a hired *scriptor* painted electoral and commercial wall notices for pay. A household can hire one directly to paint deliberate, favorable Graffiti — boosting a candidate's own visible public support ahead of a Curia election, advertising a Notable Business, or simply displaying Dignitas. This is explicitly **paid, deliberate messaging**, distinct from and running alongside the organic, reactive Graffiti §4 generates on its own.

### 3.1 The Scriptor's Own Loose Tongue

A small, honest new risk: the *scriptor* who painted your favorable notices last month is just as available to a rival household willing to pay more, and has no particular loyalty keeping quiet about who hired them for what. A hiring household's own identity behind a Scriptor Commission carries a light, real leak risk over time, feeding Correspondence & Letters' own gossip channel or, at worse, Espionage — hiring the same scriptor repeatedly builds a real, quiet paper trail rather than perfect deniability.

---

## 4. Dynamic Generation — How the Walls React to the World

The real heart of this document: Graffiti isn't static flavor text, it's a live, reactive layer reading directly off other systems' own existing state:

- **An active Scandal** generates real, organic, negative Graffiti naming the household involved.
- **A contested Election** generates real, competing *Programmata* from both sides.
- **A beloved or notorious gladiator or racing faction** generates real fan commentary.
- **A published Discovery, a Renowned Book, or a celebrated Artist** can generate real, admiring commentary once genuinely public knowledge.
- **An active Feud** can generate real, hostile Graffiti about the opposing house, organically.

---

## 5. Reading the Walls — A Real Information-Gathering Action

A light, passive information-gathering action, naturally paired with Social Places' own Forum, Tavern, and Public Latrines entries: a Character can "read the walls" while there, surfacing a small, real sample of current Graffiti.

---

## 6. Defamatory Graffiti — A New Scheme Type

A new, real Scheme type, giving a player a direct, active tool the same dishonest real Roman political rivals sometimes used: commissioning hostile, damaging Graffiti about a specific target, resolved through the Scheme engine — real Progress, real Discovery risk, real Counter-play. A discovered Defamatory Graffiti Scheme is a genuine, felt Scandal in its own right for the initiator.

---

## 7. Erasure — Fighting Back Against a Damaging Wall

A real, deliberate countermeasure: a household can pay to have damaging Graffiti painted over or scrubbed away. Erasure removes the *specific* physical inscription, but doesn't erase the underlying Scandal, Rumor, or Feud generating it — if the root cause is still live, new Graffiti can simply reappear.

---

## 8. Curse Tablets (*Defixiones*) — A Darker, Distinct Practice

A real, well-documented, and genuinely different ancient practice: a **defixio** — a curse written on a thin lead tablet, naming a specific rival, then deposited somewhere with real ritual significance (a well, a bathhouse drain, a grave) — is real, attested archaeological practice. Consistent with this project's own default historical-grounding stance, a *defixio*'s actual efficacy is left entirely, deliberately ambiguous, read the same honest way Religion's own Omens are. What is real and concrete: commissioning or discovering one is a genuine, serious social and legal matter, regardless of whether the curse itself "worked."

---

## 9. Backfire — When a Wall Overplays Its Hand

New this pass, and a direct application of Design Pillar #1's own "no dominant strategy" instinct: Graffiti that is too obviously biased, too viciously personal, or aimed at a target who is currently genuinely well-regarded can **backfire** — read as the sponsor's own poor judgment or petty cruelty rather than a credible claim. A Scriptor-commissioned *Programma* attacking a popular incumbent, or a Defamatory Graffiti Scheme (§6) targeting a Character with strong existing public sympathy, carries a real, standing risk of actually damaging the *sponsor's* own Dignitas more than the intended target's, once discovered or simply once public sentiment reads it as unfair. This gives Defamatory Graffiti a genuine, felt risk-reward calculation beyond pure Discovery odds — sometimes the smarter move is not attacking at all.

---

## 10. Famous Graffiti — A Small, Real Kind of Memory

New this pass, and a genuinely fitting capstone given how much this whole project cares about "memory has weight": an especially witty, dramatic, or historically significant piece of Graffiti — the line that actually swung an Election's own public mood, the insult a whole settlement still quotes years later — can become **Famous**: a real, low-stakes but permanent piece of local color, referenced in later flavor text and eligible for its own small Dynasty Chronicle mention, entirely independent of whether the household it concerns is still prominent, still exists, or even remembers it themselves. A single, particularly good line surviving longer than the people it was about is exactly the kind of quiet historical irony this project already leans into elsewhere (a Lost Work, a Legendary Masterwork, an Ancestral Grudge outliving both original combatants).

---

## 11. Density, Language & Literacy — Who Actually Reads a Wall

Two honest resolutions to real, practical questions:

- **Density.** A larger, busier settlement (reading Settlement Demographics' own population figure and Villa/Buildings' own Grandeur-adjacent civic development) simply carries more Graffiti at any given time — more walls, more people, more to say — giving a genuinely bustling City real, felt ambient texture a small Vicus doesn't have.
- **Literacy.** Language & Literacy's own tiered fluency system means not every Character can actually read a wall directly — but real, ordinary daily practice plausibly had a literate passer-by or a hired reader relay a notice's contents aloud for an illiterate one, meaning Reading the Walls (§5) remains available even to an illiterate Character, at a small, real fidelity cost (a secondhand account is less precise and slightly more prone to distortion than reading it directly) rather than being flatly locked behind literacy.

A light, real regional note: a Greek East settlement's own walls plausibly carry a real mix of Latin and Greek Graffiti, and Egypt's own a further Demotic thread — flavor only, not a mechanical gate, resolving the standing open question from the first pass without overbuilding a full regional-content system for it.

---

## 12. Cross-System Integration

- **Social Places:** Reading the Walls (§5) and Density (§11) both directly extend that document's own Forum, Tavern, and Public Latrines entries.
- **Scandal:** an active Scandal is Graffiti's own single most common organic trigger; a discovered Defamatory Graffiti Scheme, a discovered *defixio*, or a Backfire (§9) are all real Scandal sources in their own right.
- **Politics & Patronage:** *Programmata* are a direct, real mechanism for a contested Election's own visible public sentiment; Backfire (§9) is a real, new risk for a poorly-judged campaign attack.
- **Correspondence & Letters / Espionage:** the Scriptor's own loose tongue (§3.1) is a light, new leak vector for both.
- **Characters:** Defamatory Graffiti is a new, concrete Scheme type running on that document's existing engine.
- **Games & Spectacle / Celebrities & Influential Figures:** fan and Circus Faction Graffiti are real, organic Fame material.
- **Technology & Discoveries / Books & Manuscripts / Art & Art Commissions:** each gains a real, organic public-recognition channel; Literary Quotation Graffiti (§2) is a direct, real Books & Manuscripts and Language & Literacy tie.
- **Rival Houses:** an active Feud is a real, organic Graffiti trigger.
- **Religion:** the *defixio*'s own deliberately ambiguous efficacy reads the same honest framework Omens already established.
- **Legal & Court:** a discovered *defixio* or Defamatory Graffiti Scheme is a genuine, real case type.
- **Settlement Demographics / Villa/Buildings:** Density (§11) reads both documents' own existing population and Grandeur figures directly.
- **Language & Literacy:** the reading-aloud resolution (§11) keeps Graffiti a genuinely universal information source without contradicting that document's own literacy tiers.
- **Dynasty Chronicle:** Famous Graffiti (§10) is a real, new, delightfully minor entry type, alongside a household's own name appearing in genuinely admiring or damning public Graffiti.

---

## 13. Data Model

```
Graffito {
  graffitoId, settlementId, locationRef,
  category,                          // per §2's table, now including "circusFaction" and "literaryQuotation"
  tone,
  originType,
  triggeringSourceRef,
  targetCharacterOrHouseholdId,
  monthCreated,
  erased: bool,
  isFamous: bool,                      // §10
  backfired: bool,                     // §9
  languageScript,                       // §11 — "latin" | "greek" | "demotic" | etc., flavor only
}

ScriptorCommission {
  commissionId, hiringHouseholdId,
  purpose,
  resultingGraffitoId,
  leakRiskAccumulated,                  // §3.1
}

DefamatoryGraffitiScheme extends Scheme {
  targetCharacterOrHouseholdId,
  resultingGraffitoId,
  backfireRisk,                        // §9
}

DefixioRecord {
  defixioId, commissioningCharacterId, targetCharacterId,
  depositLocation,
  discovered: bool,
  legalCaseRef, scandalRef,
}
```

---

## 14. Open Questions

- **All numeric sizing**, per convention — Graffiti generation frequency, Scriptor cost and leak-risk accumulation rate, Erasure cost/effectiveness, Backfire trigger thresholds, and Famous Graffiti's own promotion criteria are all unsized.
- **Whether Graffiti should ever directly feed back into the originating Scandal or Election's own severity/outcome**, or remain a purely read-only reflection of state generated elsewhere.
- **Whether a *defixio*'s target ever has any real, in-fiction way of discovering they've been cursed absent the tablet itself being found** — left deliberately unresolved, consistent with the practice's own real, ambiguous nature.
- **Whether Famous Graffiti (§10) should ever be physically relocated or preserved** (a fragment of wall plaster kept as a household curiosity) rather than simply remaining flavor text tied to its original location — a plausible, minor future refinement.
