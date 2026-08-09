# GENS — System Design: Education & Culture (§6.14)
*Two distinct halves exactly as the core doc frames them — a real childhood Pedagogy investment pipeline following the real Roman age-based curriculum, and an adult/household Cultural Patronage layer with a Symposium mechanic and Maecenas-flavored Literary Patronage — built on Culture as a tracked, CK3-style Character identity. This pass adds the piece a CK3 comparison calls for directly: real, named foreign Institutions of Renown — Athens, Rhodes, Alexandria, Pergamon, and Massilia — sending a Character abroad via Travel for superior training, a lasting credential, and the single sharpest available lever on Hellenization, plus a reverse "Renown attracts Renown" mechanic once a household's own Academy becomes prestigious enough to draw foreign students in turn.*

---

## Contents

1. Scope & Role
2. Culture — A Tracked Identity
3. Pedagogy — The Childhood Investment Pipeline
4. Educational Tracks — Rhetoric, Philosophy & the Gymnasium
5. Institutions of Renown — Studying Abroad
6. Literacy, Hard Gates & Career Access
7. Cultural Patronage — Household Cultural Prestige
8. Hellenization & the Traditionalist Tension
9. Cross-Cultural Marriage & Diplomacy
10. Daughters, Education & the Rhetoric Track
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "rhetoric schools, Greek tutors, philosophy, and literacy as an investment system raising stats and unlocking career/political/marriage options; cultural prestige as a soft-power complement to raw dignitas." Two real, distinct halves — **Pedagogy** (§3–6) and **Cultural Patronage** (§7) — sitting on top of **Culture** (§2). New this pass, **Institutions of Renown** (§5) gives Pedagogy its own real "university" equivalent, built the way Rome itself actually handled advanced education: not a local degree-granting building, but a real, historically-attested practice of sending a promising young household member abroad to study under the most famous names and schools the wider Mediterranean world actually had.

---

## 2. Culture — A Tracked Identity *(unchanged)*

Every Character carries a real `culture` field — Roman, Gallic, Iberian, or Hellenic — that drifts (fast during Childhood/Adolescence, slow but real in adulthood), can remain permanently blended for a cross-cultural marriage's child, and can be deliberately steered via a Foreign Tutor (§3.3) or, far more powerfully, via Institutions of Renown (§5).

---

## 3. Pedagogy — The Childhood Investment Pipeline *(unchanged)*

A real age-based curriculum: foundational literacy and physical development during Childhood, the genuine Educational Track choice (§4) activating at Adolescence, delivered by the household's own Paedagogus or the settlement's Rhetor/Magister, with a Distinguished tier (§4.1) and a deliberate Foreign Tutor option (§3.3) both available as real upgrades on the ordinary local path.

---

## 4. Educational Tracks — Rhetoric, Philosophy & the Gymnasium *(unchanged)*

Three tracks — Rhetoric (Diplomacy), Philosophy (Learning, Cultural Prestige), Gymnasium (Martial, Learning) — built on existing buildings (School→Academy, Academy's advanced tier, Gymnasium/Palaestra), with a Distinguished resident tutor as a rare, costly quality tier above the ordinary staffed position.

---

## 5. Institutions of Renown — Studying Abroad

The direct answer to a real, well-documented Roman elite practice: Rome never developed a university in the medieval or modern sense, but wealthy Roman families very much did send their sons — and, per §10, occasionally their daughters — abroad to study under the most celebrated teachers and schools the Mediterranean actually had. Cicero's own son studied rhetoric at Rhodes; young Roman nobles traveled to Athens to round out a philosophical education; Alexandria's Library and Museum were, without exaggeration, the single greatest concentration of scholarship in the ancient world. This section gives that real practice its own concrete mechanic rather than leaving Educational Tracks as a purely local affair.

### 5.1 The Named Institutions

| Institution | Real grounding | Specialty | Prime cultural association |
|---|---|---|---|
| **The Academy at Athens** | Plato's own real school, with a genuine, documented continuity of teaching across centuries | Philosophy — the single most prestigious Philosophy destination available | Hellenic |
| **Rhodes** | A real, well-attested premier school of rhetoric — the actual destination Cicero sent his own son to | Rhetoric — the single most prestigious Rhetoric destination available | Hellenic |
| **Alexandria** | The real Library and Museum (*Mouseion*) — the single greatest concentration of ancient scholarship, spanning philosophy, medicine, and the sciences at once | General Learning, with a distinct **medicine** specialty — a Character sent here specifically to study under Alexandria's real physicians carries a unique, superior Disease & Public Health §5 diagnostic bonus on return | Hellenic |
| **Pergamon** | A real rival library and medical center — genuinely second only to Alexandria, Galen's own real later association | General Learning and medicine, at a real notch below Alexandria's own prestige | Hellenic |
| **Massilia** | A real, distinct Western option — a genuinely attested reputation as "the Athens of Gaul," a real Greek-founded city with its own real intellectual culture, reachable without the long voyage to the Greek East | Philosophy and Rhetoric both, at a real notch below the Greek East's own top-tier destinations, but a meaningfully shorter and safer Journey for a Gallic-frontier or Iberian-colony household specifically | Gallic-adjacent Hellenic — a real, distinct blend rather than a lesser copy of Athens |

### 5.2 The Journey

Sending a Character to study abroad is a real, multi-year **Travel** (§6.18) Journey, deliberately longer than an ordinary round-trip — actual ancient study abroad took real years, and this document doesn't compress that into a season. The Journey carries every real risk Travel already models (Piracy & Banditry exposure en route, Disease & Public Health exposure both traveling and residing abroad, Natural Disasters), plus a real, ongoing Denarii cost for the duration — tuition and living costs, not a one-time fee.

The Character is genuinely unavailable to the household for the Journey's full length, the same absence tradeoff Travel already applies to any extended trip — a real cost sitting alongside the real reward below.

### 5.3 The Credential

On successful return, the Character carries a permanent, named **Institution Credential** — "Educated at Athens," "Trained at Rhodes" — functioning as a light, lifelong distinction rather than a fading bonus: a real, meaningfully faster Track progression than the equivalent local path would have delivered, a direct and ongoing Cultural Prestige contribution whenever that Character later hosts a Symposium (§7.1), a genuine boost to marriage-market desirability independent of the household's own general Cultural Prestige, and, for Alexandria/Pergamon specifically, the named medical specialty bonus feeding Disease & Public Health directly if that Character later serves as a Court Physician.

**The real, felt cost:** a Journey to any of these institutions — the Greek East ones most sharply, Massilia more mildly — is the single strongest available driver of that specific Character's own Cultural Drift (§2) toward Hellenic culture, a far sharper and more deliberate version of what a Foreign Tutor (§3.3) only nudges gradually. Sending a son to Athens for years is choosing, with full knowledge of what it costs, to risk him coming home meaningfully more Greek than Roman — exactly the real anxiety Cato's own historical position (§8) was actually about, now with a single, concrete, nameable decision point rather than only a slow background drift.

### 5.4 Renown Attracts Renown

A reverse mechanic, new this pass, closing the loop the other direction: once a household's own Academy or Library, combined with a sufficiently high Cultural Prestige (§7), crosses a real recognition threshold, the household's own seat becomes a plausible destination in its own right — a foreign notable's own promising child can arrive seeking to study *there*, generating a real Clientela-adjacent tie to that notable's own family and a further Cultural Prestige gain simply from having been chosen. This is a genuine, rare, earned endgame payoff for sustained Cultural Patronage investment, the same "the best houses eventually become somewhere other houses want to be" logic Household Doctrine's own Apex tier already rewards at a different scale.

---

## 6. Literacy, Hard Gates & Career Access *(unchanged)*

Illiteracy blocks Learning-tier record-keeping roles and Correspondence outright; contesting a magistracy above the lowest rung requires the Rhetoric Track (an Institution Credential from Rhodes satisfies this at least as well as the local equivalent); prestigious marriage candidates aren't offered below a real Cultural Prestige threshold.

---

## 7. Cultural Patronage — Household Cultural Prestige *(unchanged, cross-referencing §5)*

The Symposium (§7.1) and Literary Patronage (§7.2) remain this document's two concrete Prestige-generating mechanisms, now joined by any resident Character's own Institution Credential (§5.3) as a further, ongoing contribution whenever they host or participate.

### 7.1 The Symposium *(unchanged)*
### 7.2 Literary Patronage *(unchanged)*

---

## 8. Hellenization & the Traditionalist Tension

Unchanged in its core shape — real Cultural Prestige and Domus Provincialis synergy against real Mos Maiorum suppression and Traditionalist backlash — now with Institutions of Renown (§5.3) as its single sharpest, most deliberate lever: a household can drift toward Hellenization slowly and almost accidentally through ordinary Cultural Patronage, or it can make one specific, large, nameable bet by sending an heir to Athens and living with exactly what that bet costs.

---

## 9. Cross-Cultural Marriage & Diplomacy *(unchanged)*

---

## 10. Daughters, Education & the Rhetoric Track

Unchanged in substance from the previous pass — a daughter can pursue any Track, and Rhetoric's investment channels into marriage-negotiation leverage, Clientela influence, and Symposium hosting rather than magistracy access. **Worth addressing directly given §5's new content:** sending a daughter abroad to an Institution of Renown was real but genuinely rarer historically than sending a son, and this document keeps that honest rather than pretending otherwise — it's allowed, carries the identical mechanical credential and Prestige benefit, but a more cosmopolitan or already-Hellenizing household is a more plausible one to actually choose it for a daughter, a real, legible social-expectation texture rather than a hard gender lock.

---

## 11. Cross-System Integration

- **Travel:** Institutions of Renown (§5) are this document's own named, extended Journey type — the single largest new integration point this pass adds, sitting alongside Travel's existing encounter and recruitment machinery rather than replacing any of it.
- **Disease & Public Health:** Alexandria and Pergamon's medicine specialty (§5.1) is a direct, named forward hook into that document's Court Physician diagnostic mechanics.
- **Piracy & Banditry, Natural Disasters:** both are real, unmodified risk factors during an Institution Journey, read through Travel's own existing exposure model.
- **Familia, Traits, Companions & Court Positions, Buildings/Villa, Policies & Edicts, Settlement Demographics, Politics & Patronage, Religion, Correspondence & Letters, Diplomacy with Non-Roman Peoples, Dynasty Chronicle:** all unchanged from the prior pass's integration, with Dynasty Chronicle gaining one further guaranteed entry type — a Character's own successful return from a named Institution.

---

## 12. Data Model

```
InstitutionOfRenown {                  // §5.1 — a small, fixed roster, not player-created
  institutionId, name,                    // "athens" | "rhodes" | "alexandria" | "pergamon" | "massilia"
  specialtyTrack,                          // "philosophy" | "rhetoric" | "learningGeneral" | "medicine"
  primeCulturalAssociation,
  prestigeTier,                             // relative ranking, e.g. alexandria/athens/rhodes top, pergamon/massilia second
}

StudyAbroadJourney {                     // §5.2 — extends Travel's own Journey record
  journeyId, characterId, institutionId,
  startMonth, durationMonths,
  costPerMonth,
  outcomeCredentialGranted: bool,
  culturalDriftAcceleration,                  // §5.3 — the sharp, deliberate drift push this specific Journey applies
}

CharacterInstitutionCredential {          // §5.3 — permanent, attached to the Character record
  characterId, institutionName, creditedTrack,
  ongoingPrestigeContribution,
  medicalSpecialtyBonus: bool,                // true only for an Alexandria/Pergamon medicine credential
}

RenownAttractsRenownState {               // §5.4
  householdId, recognitionThresholdMet: bool,
  incomingForeignStudentOpportunityActive: bool,
}
```

---

## 13. Open Questions

- **All numeric sizing.** Journey duration and cost, drift-acceleration magnitude per Institution, and the Renown-Attracts-Renown recognition threshold are all unsized, alongside every other unsized figure carried forward from prior passes.
- **Institution capacity/exclusivity.** Whether a real historical Institution like Athens should ever be modeled as having limited "slots" (competing with Rival Houses' own sons for the same renowned teacher's attention) isn't addressed — a plausible, evocative future refinement rather than a structural gap.
- **Return risk.** §5.2 covers the outbound Journey's risks; whether a separate, distinct risk profile applies to the return trip, or whether it's treated as the identical Journey in reverse, isn't specified.
- **A player-character's own eligibility.** Whether the player's own controlled Character can personally undertake a Study Abroad Journey (temporarily ceding direct control the way an NPC-run household already does during other absences) or whether this is restricted to other Familia members isn't decided.
- Carried forward, unchanged: adult drift rate, blended-culture permanence (both resolved in the prior pass), a fifth/sixth named Culture, Track-switching's progress penalty, and the full Doctrine-suppression matrix beyond Mos Maiorum/Domus Provincialis.
