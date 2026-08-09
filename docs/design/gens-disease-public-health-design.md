# GENS — System Design: Disease & Public Health (§6.13)
*Two real, separately-tracked layers — chronic Endemic Illness and acute, contagious Epidemics — with seven endemic diseases and four epidemic ones, spanning poverty-coded ailments through wealth-coded ones (Gout) and, in Saturnism, a single disease with two entirely unrelated real drivers at opposite ends of the social scale: elite lead-sweetened wine at one end, Iberian mining exposure at the other. A real Immunity mechanic rewards epidemic survivors, a Sanitation Investment standing policy gives the player one direct lever over the whole system, a light zoonotic crossover connects human and livestock outbreaks without overstating the science, and a closing Regional Disease Profiles table gives all of it a concrete, at-a-glance shape across the game's four starting regions.*

---

## Contents

1. Scope & Role
2. Endemic Illness — The Chronic Background Layer
3. Epidemics — Acute Outbreaks & Contagion
4. Quarantine & Containment
5. Immunity — Surviving Leaves a Mark
6. Sanitation Investment — A Standing Policy
7. Treatment — Medicine, Physicians & Sanitation
8. Livestock Disease
9. The Antonine Plague — A Real Historical Epidemic
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "plagues and endemic illness moving through the household/settlement independent of labor conditions, though treatment and crowding affect vulnerability. Interacts with medicine/doctors, sanitation buildings, and Religion." Two genuinely distinct, separately-tracked layers:

- **Endemic Illness** (§2) — chronic, regional, always present at some level, a steady Health drain rather than a discrete Event.
- **Epidemics** (§3–4) — acute, rare, and genuinely contagious: real person-to-person spread, quarantine as a felt decision, and now, new this pass, real lasting **Immunity** (§5) for whoever survives one.

Both layers mirror Natural Disasters' own multi-hazard design language: several named, historically real diseases, each with its own driver and flavor, rather than one generic meter wearing different labels. This pass also gives the player a real, direct lever over the whole system — **Sanitation Investment** (§6) — and closes the loop on livestock disease's relationship to human disease with a light, deliberately careful crossover.

---

## 2. Endemic Illness — The Chronic Background Layer

Every settlement carries a standing **Exposure** score per endemic disease, read from terrain, infrastructure, living conditions, and — new this pass — wealth and diet, since not every real ancient chronic illness was a disease of poverty. Unlike an Epidemic, an endemic disease never resolves as a discrete Event; it's a continuous, low-grade Health drain, real and felt over a long stretch rather than in a single dramatic moment.

| Disease | Real grounding | Primary driver | Mitigated by |
|---|---|---|---|
| **Roman Fever** *(malaria)* | The real ancient term for exactly this illness — the Pontine Marshes near Rome itself were historically notorious for it | Marsh/Poor-land terrain, worse in summer | Aqueduct/Cistern drainage; **no real cure** — only managed severity |
| **The Flux** *(dysentery)* | A real, extremely common ancient gut illness | Poor sanitation — lacking Public Latrines/Fountains or a working Aqueduct | Public Latrines/Fountains, Aqueduct/Cistern |
| **Ophthalmia** | A real, widely-attested ancient eye affliction | Dust-heavy, arid regions — the Iberian colony most of all | Bathhouse/Grand Baths; no full prevention |
| **Consumption** *(tuberculosis)* | A real, slow, wasting ancient illness, spreading readily in close quarters | Population density — Insulae-heavy urban settlements | Reduced Overcrowding; no cure, only slower progression |
| **Leprosy** *(new)* | A real, if often broadly-diagnosed, ancient affliction — chronic and, historically, as much a social condition as a physical one | Not terrain-driven at all — purely a matter of exposure and time; genuinely rare | No infrastructure mitigates it — see below for its real, distinct consequence |
| **Gout** *(new)* | A real, extensively documented ancient affliction, historically associated specifically with a rich diet and heavy wine consumption — "the disease of kings" for real historical reasons | **Wealth**, not poverty: a household or individual sustained at Lavish consumption tier (Settlement Demographics §6.1), heavy Wine intake specifically | Moderating consumption tier; no building fixes it |
| **Saturnism** *(lead poisoning, new)* | A real, genuinely fascinating historical hypothesis: Roman elites sweetened wine with *defrutum* boiled in lead pots, and used lead in plumbing and cookware — a real, documented health cost hiding inside luxury itself. A second, entirely different real driver exists at the other end of the social scale: Iberia was Rome's own major real source of mined lead and silver, and the populations living and working near those mines carried a genuine, documented exposure risk of their own | A Domus-stage household's own plumbing/cookware choices and sustained heavy Wine consumption from lead-processed stock — **or**, independent of any of that, simply living in or operating a Mine (Estate & Settlement's own Hills-terrain chain) in the Iberian colony specifically | Choosing terracotta over lead cookware for the wealthy driver; there is no real mitigation for the mining-proximity driver, an honest reflection of the real, unavoidable occupational exposure actual ancient miners faced |

**Leprosy's real distinct consequence:** rather than a Health drain scaling like the others, a Character carrying it faces genuine **social exclusion** — real ancient stigma, not merely a flavor label: exclusion from Group Interactions, a real penalty to marriage-market negotiations (Familia §5), and a Court Position holder carrying it facing real, felt opinion penalties across the relationship web. This is deliberately the one endemic illness that's as much a Characters-and-Familia mechanic as a Health one.

**Saturnism's real distinct shape:** slow, chronic, and genuinely invisible to period medicine — this document doesn't give the player's own Physician any way to actually diagnose it correctly (a real, honest historical limit), only to treat its symptoms as an unexplained chronic decline. It's also, deliberately, the one disease on this table with two entirely unrelated real drivers at opposite ends of the social scale: a wealthy household leaning into Domus-stage luxury and heavy Wine consumption is trading real, felt comfort and Dignitas for a slow cost nobody in the fiction can actually name, while an Iberian mining operation's own workers carry a comparable risk from simple occupational proximity, with no comfort or luxury attached to it at all. Keeping both true at once is the point — this was never really a disease of wealth *or* poverty specifically, just of lead, however Rome happened to encounter it.

Sustained, severe Exposure to any endemic disease can still escalate into a genuine acute flare under compounding conditions (a Famine or Flood aftermath), resolving through the Epidemic machinery in §3 — unchanged from the first pass.

### 2.1 Regional Disease Profiles — Illustrative, Not Exhaustive

A quick-reference sense of how the roster above actually distributes across the game's four starting regions, since terrain and regional economy do most of the real work in determining which diseases a given household should actually worry about:

| Region | Most relevant endemic diseases | Why |
|---|---|---|
| **Italian heartland** | Roman Fever (the real Pontine Marshes lay in exactly this region), Consumption | Marsh terrain near Rome itself, plus the heartland's own genuinely dense urban settlements |
| **Gallic frontier** | The Flux, Camp Fever *(epidemic)* | A real, frontier military presence and less-developed sanitation infrastructure than an established heartland settlement |
| **Iberian colony** | Ophthalmia, Saturnism *(mining driver)* | Arid terrain, and Rome's own real historical lead/silver mining concentration |
| **Greek East** | Gout, Saturnism *(wealth driver)* | The region's own real historical reputation for Hellenized elite luxury, wine culture, and refined dining |

No region is locked out of any disease entirely — a Gallic frontier household can still develop Gout from a sufficiently Lavish lifestyle, the same way an Italian heartland household near a badly-drained plot can still suffer Roman Fever — this table reflects which risks are *most likely* to matter first, not a hard regional gate.

---

## 3. Epidemics — Acute Outbreaks & Contagion

### 3.1 The Contagion Model *(unchanged)*

Real person-to-person spread through genuine Contact: Group Interactions, shared housing, Travel, and shared Household Duty slots — a Character's own social contact level determines how fast they spread a disease they're carrying, not just whether they catch one.

### 3.2 Named Epidemic Diseases

| Disease | Real grounding | Typical presentation | Primary vector |
|---|---|---|---|
| **Pestilence** | The generic severe ancient fever-plague — the same broad symptom profile the Antonine Plague itself is believed to match | The most severe available; real, meaningful mortality risk | Person-to-person contact (§3.1) |
| **Pox** | A real, disfiguring ancient illness | Survivors can carry a real, permanent Appearance change | Person-to-person contact |
| **Camp Fever** | The crowd-and-poor-sanitation fever real ancient armies suffered from constantly | The named disease behind Military & Combat's existing "Plague in the Camp" siege Event | Person-to-person contact, concentrated in military camps specifically |
| **Enteric Fever** *(typhoid, new)* | A real, distinct ancient water-borne illness | Severe, but resolves faster than Pestilence when treated | **Water-borne, not contact-borne** — a contaminated Aqueduct or a post-Flood water supply, not crowding or Group Interactions |

Enteric Fever is deliberately the one Epidemic with a wholly different vector from the other three: it doesn't spread through Group Interactions or shared housing the way Pestilence, Pox, and Camp Fever do — it spreads through a settlement's own water supply, meaning a household that's invested in Aqueduct/Cistern infrastructure for other reasons gets a real, direct Enteric Fever benefit, while a Flood's aftermath (Natural Disasters' own compounding logic) is this specific disease's own sharpest trigger.

### 3.3 Progression & Outcomes *(unchanged)*

Worsening, recovery (with a real chance of the Plague Survivor trait), or death — resolved through Familia's own unrestricted death mechanism.

---

## 4. Quarantine & Containment

### 4.1 Personal Quarantine and 4.2 Settlement-Wide Quarantine *(unchanged)*

Isolating an infected Character reduces spread at a real cost to their own recovery odds; a settlement-wide Quarantine slows an outbreak at a real Contentment and Commerce cost.

### 4.3 Quarantine at Imperial Scale *(new)*

Worth stating explicitly: during a genuine Empire-wide event like the Antonine Plague (§9), an individual settlement's own Quarantine is **meaningfully less effective** than it is against an ordinary local outbreak — closing the gates helps when the danger is a specific, containable local case, but does far less against something already moving through the whole province by every road and port at once. This isn't a mechanical nerf so much as an honest reflection of what quarantine can and can't actually do against a pandemic operating at that scale, and it's part of what makes the Antonine Plague read as a genuinely different, humbling kind of threat rather than just a bigger version of an ordinary local outbreak.

---

## 5. Immunity — Surviving Leaves a Mark

New this pass, and a real, historically grounded addition: ancient observers — Thucydides' own account of the Plague of Athens is the most famous surviving example — specifically noted that people who survived a serious illness rarely caught the *same* disease again, and were often the ones who ended up nursing everyone else specifically because of it. This document builds that observation in directly: a Character who survives an Epidemic (§3) gains real, lasting **Immunity** to that *specific* disease — Pestilence, Pox, Camp Fever, or Enteric Fever individually, never a blanket immunity to disease in general. A household with several Pestilence-immune Plague Survivors from an earlier outbreak has a real, mechanical reason to lean on them specifically for nursing duty during a later one, exactly the real historical pattern this mechanic is modeling.

---

## 6. Sanitation Investment — A Standing Policy

New this pass: a genuine, direct lever over this entire system at once, built in the same shape Religion's Rites Budget and Policies & Edicts' other Standing Policies already use — **Minimal / Standard / Comprehensive**, trading an ongoing Treasury cost against a real, settlement-wide reduction across every Endemic Exposure score and every Epidemic's severity/spread rate simultaneously. A Comprehensive posture doesn't replace any individual building's own effect (the Aqueduct, the Bathhouse, the Latrines all still matter on their own) — it's a genuine multiplier on top of whatever infrastructure already exists, representing the difference between a settlement that merely *has* sanitation buildings and one that actually staffs, maintains, and enforces their proper use.

Consistent with how Religion's Rites Budget and Natural Disasters' Disaster Relief were both built ahead of Policies & Edicts' own existence and then folded in as that document's canonical home, **Sanitation Investment is built here in full but explicitly flagged as belonging in Policies & Edicts' own Standing Policy roster** (§2 of that document) on its own next revisit, sitting naturally alongside Rites Budget and Annona Provision as a third "steady, recurring civic-good spend."

---

## 7. Treatment — Medicine, Physicians & Sanitation *(unchanged, with one addition)*

Medicine (Apothecary chain) consumed by the Valetudinarium/Iatreion manages severity without guaranteeing a cure — deliberate, historically honest. A skilled Physician adds a real early-diagnosis check. Sanitation infrastructure lowers baseline Exposure continuously. Religion's Aesculapius/Asclepius offering remains a modest, complementary, faith-based improvement to recovery odds, never a substitute for actual treatment.

**New this pass, tied directly to §2's Saturnism entry:** a Physician's early-diagnosis check explicitly *cannot* catch Saturnism specifically — this is a deliberate, narrow exception to the general Physician mechanism, preserving the real historical honesty that lead poisoning's cause was genuinely unknown to ancient medicine, rather than quietly letting a good enough Physician solve every problem in the document.

---

## 8. Livestock Disease *(unchanged, with one addition)*

Murrain (acute Epizootic) and Scab (chronic, productivity-reducing) remain livestock's own parallel diseases, driven by Herd Strategy and Flood contamination, mitigated by Vilicus Stewardship and the "Cull the Sick" action.

**A light, deliberately careful zoonotic crossover, new this pass:** a severe Murrain Epizootic can modestly raise the surrounding settlement's own general Epidemic Exposure for that same month — a real, documented pattern across history that disease can move between animals and people — without this document asserting any specific pathogen or transmission mechanic between named diseases. This stays a light, real-world-plausible correlation rather than a hard simulated link, deliberately avoiding overstating disease science the design isn't trying to model at that level of specificity.

---

## 9. The Antonine Plague — A Real Historical Epidemic *(unchanged)*

A real, dated, Empire-wide Tier 2 Imperial Event (Events §6.4), historically AD 165–180, modeled as a multi-year Event Chain elevating Pestilence Exposure everywhere regardless of individual household preparation, with a returning campaign or Roman Service Character flagged as the real historical introduction vector. Settlement Quarantine's reduced effectiveness during this specific event is now explicit per §4.3. Player foreknowledge of its real date remains a deliberate feature per Events §6.3.

---

## 10. Cross-System Integration

- **Familia:** Health, Fatigue, permanent injury, marriage-market negotiations (Leprosy's social exclusion), and the unrestricted death mechanism.
- **Traits:** Plague Survivor now has a real generating system, plus real, lasting per-disease Immunity (§5) as its mechanical payoff.
- **Estate & Settlement:** Marsh terrain drives Roman Fever; region drives Ophthalmia.
- **Settlement Demographics:** Overcrowding/Insulae density drive Consumption and general Epidemic spread; consumption tiers (Lavish) drive Gout and Saturnism, a genuine "disease of wealth" counterweight to the otherwise poverty-coded roster.
- **Buildings:** Bathhouse, Latrines, Aqueduct/Cistern, Valetudinarium, and the Apothecary's Medicine chain remain this document's core infrastructure; the Aqueduct is now Enteric Fever's own specific mitigation as well as the Flux's.
- **Companions & Court Positions:** the Court Physician/Valetudinarius remain the named treatment-and-diagnosis operators, with a real, honest limit against Saturnism specifically.
- **Villa:** the Iatreion; a Domus-stage household's own plumbing/cookware choices are Saturnism's own concrete driver.
- **Resources & Goods:** Medicine remains the primary treatment good; Wine consumption volume is now a direct Gout and Saturnism input.
- **Natural Disasters:** Flood/Famine aftermath remains a direct trigger for an endemic flare, a livestock Epizootic spike, and now specifically Enteric Fever's own sharpest introduction.
- **Religion:** Ill Omens can foreshadow a coming outbreak; the Aesculapius/Asclepius offering remains a real, modest complement to Medicine.
- **Military & Combat:** Camp Fever remains the named disease behind "Plague in the Camp"; a returning Character remains the Antonine Plague's real introduction vector.
- **Events:** the shared pool/scope machinery and the Antonine Plague's dated Historical Timeline entry are unchanged.
- **Characters:** Group Interactions remain the primary contagion vector; Leprosy's social exclusion reads directly into the relationship web.
- **Policies & Edicts:** Sanitation Investment (§6) is this document's own fully-built, forward-flagged addition to that system's Standing Policy roster, alongside the previously-flagged addition.
- **Dynasty Chronicle:** a severe Epidemic's toll, a Plague Survivor's story, and the Antonine Plague's arrival and passing remain guaranteed entries.

---

## 11. Data Model

```
EndemicIllness {
  settlementId,
  diseaseType,               // "romanFever" | "theFlux" | "ophthalmia" | "consumption" |
                              // "leprosy" | "gout" | "saturnism"
  exposureScore,
  contributingFactors: [ ... ],   // now includes "lavishConsumptionTier", "leadCookwareInUse", "heavyWineIntake"
  chronicHealthDrainActive: bool,
  socialExclusionActive: bool,      // §2 — true only for leprosy
}

EpidemicOutbreak {
  outbreakId, settlementId,
  diseaseType,                // "pestilence" | "pox" | "campFever" | "entericFever"
  vector,                       // "personToPerson" | "waterborne" — new field, distinguishes Enteric Fever
  startMonth,
  infectedCharacterIds: [ ... ],
  quarantinedCharacterIds: [ ... ],
  severity,
  quarantineEffectivenessModifier,   // §4.3 — reduced during an Imperial-scale event
  isAntoninePlagueInstance: bool,
  historicalTimelineRef,
}

CharacterInfectionStatus {
  characterId, diseaseType,
  infectedMonth,
  status,
  quarantined: bool,
  treatedByPhysician: bool,
  contactSourceCharacterId,
  immuneToDiseaseTypes: [ ... ],      // §5 — new field, per-disease, permanent
}

LivestockEpizootic {
  epizooticId, buildingId,
  diseaseType,                  // "murrain" | "scab"
  severity, headcountLost,
  culledProactively: bool,
  zoonoticSpilloverTriggered: bool,     // §8 — new field, the light crossover flag
}

SanitationInvestment {              // §6 — new, forward-flagged to Policies & Edicts
  settlementId,
  tier,                          // "minimal" | "standard" | "comprehensive"
}
```

---

## 12. Open Questions

- **All numeric sizing.** Exposure curves, contagion spread rates, quarantine effectiveness (including its Imperial-scale reduction), treatment recovery odds, Sanitation Investment's own cost/benefit curve, and Immunity's interaction with Exposure math are all unsized.
- **Full disease roster completeness.** Now seven endemic and four epidemic diseases — still representative rather than exhaustive.
- **Leprosy's exact social-exclusion mechanics.** §2 establishes real marriage-market and relationship-web consequences; the precise magnitude and whether it can ever fully fade with time isn't specified.
- **Saturnism's long-term narrative payoff, now lightly resolved rather than fully open:** Dynasty Chronicle (§6.11) can plausibly surface a purely observational, in-fiction line across a few generations of a wealthy line's own recorded deaths — "another of the line taken by the same wasting decline that claimed his father and grandfather" — without ever actually naming lead as the cause, since nobody in the fiction ever could. This stays a Chronicle flavor-text possibility rather than a new mechanic, and the exact wording/trigger condition is left to that document's own future content pass.
- **Contact-graph granularity.** Unchanged — an implementation question, not a design one.
- **Sanitation Investment's exact absorption into Policies & Edicts.** Built here in full; the mechanical act of formally relocating it into that document's own §2 roster is a future editing task, not a design gap.
- **Zoonotic spillover's actual trigger threshold.** §8 establishes it's light and correlational; how severe a Murrain outbreak needs to be before it meaningfully affects human Epidemic Exposure isn't specified.
