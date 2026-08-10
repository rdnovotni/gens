# GENS — System Design: Hunts, Beast Taming & Menageries (§6.51)
*Comprehensive expansion and polish pass. A real Activity Type (Hunts) paired directly with the standing collection system it naturally feeds — Villa's own Menagerie/Aviary room has existed since that document's first pass as a pure, passive Dignitas display with a Menagerie-Keeper whose job description ("prevents a Dignitas-damaging mishap") was never actually resolved into a real mishap. This document builds the real hunting expedition that fills that room with something worth displaying, resolves what a Menagerie mishap actually is, and — per explicit direction overriding this project's usual historical-grounding default — treats mythological creatures as real, confirmed, huntable and tamable inhabitants of the world's own wild edges, not merely ancient rumor. This is a deliberate, bounded exception, named as such, rather than a quiet drift into high fantasy across the rest of the project.*

---

## Contents

**Part I — Hunts**
1. Scope & Role
2. The Hunt's Six Slots
3. Game Tiers — Common, Dangerous, Exotic, and Legendary
4. Phases
5. Kill or Capture — Two Real Outcomes

**Part II — Legendary Game: Real Mythological Creatures**
6. A Deliberate Departure
7. The Bestiary
8. Where They're Found
9. Combat & Danger — Legendary-Tier Threats
10. Rewards — Capture, Kill & the Truly Rare
11. Repeat Encounters — Escaped, Not Always Ended

**Part III — Taming, Collection & the Menagerie**
12. From Captured to Tamed
13. Notable Specimens — A Real, Named, Provenanced Animal
14. Legendary Specimens — When the Menagerie Houses a Myth
15. The Menagerie as a Real Collection
16. Breeding
17. Escape — Resolving the Menagerie-Keeper's Own Flagged Mishap
18. Acquisition Beyond the Hunt

**Part IV**
19. Cross-System Integration
20. Data Model
21. Open Questions

---

# Part I — Hunts

## 1. Scope & Role

Villa's own Diaeta room has carried a vague "hunting-adjacent Event" note, and the Hunter Trait a similarly vague flavor bonus, since their first pass. This document is where both finally become a real, played Activity, built on the Activity Engine (§6.47): a genuine hunting expedition with a real Guest List, real Phases, real risk, and a real choice between killing for trophy and capturing for the Menagerie (Part III).

---

## 2. The Hunt's Six Slots

1. **Host** — the Character organizing the hunt, typically holding or aspiring to the Hunter Trait.
2. **Type** — `"hunt"`.
3. **Venue** — a private hunting preserve (a developed Diaeta-adjacent estate holding), an ordinary wild Forest or Hills Plot (Estate & Settlement), or, for Exotic and Legendary game, a genuinely distant frontier or foreign wilderness reached via Travel.
4. **Guest List** — a real hunting party: family, Companions, and invited guests, following Activity Engine §4 directly — a Hunt is a genuine, valid social occasion in its own right, not merely a solitary pursuit.
5. **Duration** — Quick for an ordinary day hunt on the household's own land; Extended (Activity Engine §3) for a genuine multi-day wilderness expedition after Exotic or Legendary game.
6. **Phases** — §4.

---

## 3. Game Tiers — Common, Dangerous, Exotic, and Legendary

| Tier | Examples | Real Risk | Real Reward |
|---|---|---|---|
| **Common Game** | Boar, deer, hare | Low — the occasional minor injury | Meat, hide, modest Dignitas |
| **Dangerous Game** | Wolf, bear | Real, meaningful injury and rare death risk | Better hide/trophy value, real Boldness/Reactive Trait potential |
| **Exotic Game** | Lion, leopard, elephant (frontier/foreign territory only) | Significant — genuinely dangerous, usually requiring a hired specialist Bestiarius (§12) to manage safely | Resources & Goods' own Exotic Beasts good directly, or a live capture destined for the Menagerie |
| **Legendary Game** | Griffins, hydras, manticores, and more — see Part II | The single greatest danger this document offers, often requiring a large, well-equipped party | The single greatest reward this document offers — see §10 |

Hunting Gear (Masterworks & Unique Crafted Objects §3) and a Character's own Hunter Trait are the direct, real inputs to a Hunt's own success odds and Quality.

---

## 4. Phases

- **Preparation.** Assembling the party, equipping Hunting Gear, and, for Dangerous, Exotic, or Legendary game, engaging a Bestiarius (§12) or hunting hounds (§12.1).
- **The Hunt.** The actual pursuit — resolved through a real, Trait-and-Gear-weighted check against the chosen Game Tier's own difficulty.
- **The Kill or Capture.** §5.
- **The Aftermath.** A natural, optional culminating Feast celebrating a successful hunt, or a somber return following an injury or loss.

---

## 5. Kill or Capture — Two Real Outcomes

- **Kill** yields real Resources & Goods and a **Hunting Trophy** — a real, named, lightweight provenanced object in the same spirit as Masterworks & Unique Crafted Objects, mountable in the Armory Alcove or Pinacotheca alongside a Masterwork Weapon.
- **Capture** yields a live specimen, feeding directly into Part III's own Taming and Menagerie system — genuinely harder to achieve than a kill for anything above Common Game, and the only path to a truly Notable Specimen.

---

# Part II — Legendary Game: Real Mythological Creatures

## 6. A Deliberate Departure

Per explicit direction, this section overrides this project's own usual historical-grounding default on purpose, and says so plainly rather than quietly drifting: the creatures in §7 are **real, confirmed inhabitants of the world's own wild and unmapped edges** — not rumor, not misidentified ordinary animals, not ambiguous Omen-adjacent atmosphere. A player who never seeks them out will never need to know they exist; the rest of this project's own historical texture (real trade goods, real politics, real disease, real death) continues entirely unaffected. This is a bounded, named exception, confined to this one document's own Legendary Game content, not a signal that the wider world has quietly become high fantasy.

---

## 7. The Bestiary

Drawn directly from real, genuine Greco-Roman mythology — public-domain stories over two thousand years old, exactly the kind of source material this project already treats as fair, safe ground elsewhere (the Aeneid's own themes, the Trojan cycle, and so on referenced only in genre and premise, never reproduced as text). Each entry below is a real, playable, legendary-tier opponent and potential Notable Specimen.

| Creature | Mythic Origin | Nature | Signature Danger |
|---|---|---|---|
| **The Nemean Lion** | The first of Heracles's real, famous mythic labors | A lion of monstrous size | Its hide cannot be pierced by any ordinary weapon — it must be wrestled and strangled, or overcome by a Masterwork of truly Legendary quality |
| **The Lernaean Hydra** | Heracles's second labor | A many-headed serpent | Severing a head causes two more to grow in its place unless the wound is immediately cauterized by fire |
| **The Chimera** | Greek myth, traditionally slain by Bellerophon | A composite of lion, goat, and serpent | Breathes real fire — a direct, serious burn-injury hazard to anyone without fire-resistant preparation |
| **Cerberus** | The three-headed hound guarding the Underworld's entrance | A colossal, triple-headed hound | Three heads mean three real, simultaneous threats in a single encounter; found only at the one place ancient Romans themselves genuinely believed was an entrance to the underworld (§8) |
| **The Griffin** | Widely attested across real ancient sources, especially tied to Scythian gold-guarding | Eagle-headed, lion-bodied, and capable of true flight | Flight makes it nearly impossible to run from; ferociously territorial over anything resembling treasure |
| **The Manticore** | Real ancient accounts place its origin in Persia and India | A human-faced, lion-bodied beast with a scorpion's tail | Fires venomous spines from its tail at range — a real ranged threat unlike most other Legendary Game |
| **The Basilisk** | Real ancient sources describe it as lethally dangerous | A serpent whose gaze or breath alone can kill | The single most dangerous creature to approach directly — most successful hunts use a mirrored shield or an indirect method rather than a face-to-face charge |
| **The Phoenix** | A real, extensively attested Egyptian and Roman religious and mythological symbol | A single, radiant firebird | Cannot be permanently killed by ordinary means — see §10.3 |
| **The Minotaur** | The real, famous myth of Crete's own Labyrinth | A bull-headed man of immense strength | Found only within a genuine, maze-like Labyrinth site — the hunt itself is as much a navigation challenge as a combat one |
| **Harpies** | Real Greek myth — winged, predatory women | A swarm rather than a single foe | Steal food, goods, and even carried treasure mid-encounter rather than simply attacking — a nuisance-and-loss threat distinct from every other entry here |
| **The Sea Serpent (Cetus)** | Real Greco-Roman sea-monster myth | An enormous ocean predator | Encountered only at sea, directly threatening a Ship (Private Ships & Shipping Ventures) rather than a land hunting party |
| **The Unicorn** | Real ancient travelers' accounts of a one-horned beast in distant lands | A gentle, extraordinarily rare beast, violent only when cornered | Cannot reasonably be killed for trophy without real narrative and Dignitas cost — capture, and only capture, is the honest goal (§10) |

---

## 8. Where They're Found

Each creature is tied to a real, specific location already established somewhere in this project, giving Legendary Game genuine geographic texture rather than a floating, placeless bestiary:

- **Nemean Lion & Lernaean Hydra** — the wild countryside around Nemea and Lerna, Greek East.
- **Chimera** — the mountainous wilds of Lycia, Anatolia.
- **Cerberus** — Lake Avernus, Campania (Italian Heartland) — the real, specific site ancient Romans themselves genuinely believed was an entrance to the underworld.
- **Griffin** — the far Sarmatian steppe and Bosporan Kingdom's own wild frontier.
- **Manticore** — the Armenia/Parthian frontier's own remote borderlands.
- **Basilisk** — the deep desert interior beyond the North African Colony.
- **Phoenix** — the deserts and temple-lands of Egypt and Arabia Felix.
- **Minotaur** — a genuine, real Labyrinth site on Crete (Greek East).
- **Harpies** — remote, wind-scoured islands and mountain crags, most plausibly in the Greek East or the Alpine Provinces.
- **Sea Serpent** — the open ocean, anywhere a Ship travels far enough from the coast.
- **Unicorn** — the farthest, least-mapped wild edges of the known world, appropriately the rarest and hardest of all to even locate.

---

## 9. Combat & Danger — Legendary-Tier Threats

Every creature in §7 resolves through the Combat Resolution Engine (Military & Combat) as a genuine **Legendary Irregular Combatant** — a tier above even an Exotic Game elephant, deliberately requiring real preparation: a large, well-equipped hunting party, Masterwork-tier weapons and armor (Masterworks & Unique Crafted Objects §3–4), and, for the very worst encounters (Cerberus, the Hydra, a Chimera), a party that has specifically prepared a counter to that creature's own signature danger (fire for the Hydra and Chimera, an indirect approach for the Basilisk, real numbers and discipline for Cerberus's three simultaneous threats). An underprepared party facing genuine Legendary Game risks a real, serious defeat — injury, death, or a full rout — not a guaranteed heroic victory simply for showing up.

A truly overwhelming threat (a Hydra actively terrorizing a settlement's own countryside, say) can escalate beyond a private Hunt entirely into a genuine Military & Combat response — a Squad or even multiple Squads mobilized against it, read exactly like any other serious regional threat, the legendary equivalent of Natural Disasters' own worst Severity Tiers.

---

## 10. Rewards — Capture, Kill & the Truly Rare

### 10.1 Kill Rewards

A slain Legendary creature yields an extraordinary Hunting Trophy — the single rarest, highest-Dignitas trophy category this document offers — and, for several entries, a genuinely valuable crafting material: a Chimera's hide or a Manticore's spines as an exceptional, narratively rich material input for a future Masterwork (Masterworks & Unique Crafted Objects §2), a real, legendary provenance baked into the resulting object from the moment it's forged.

### 10.2 Capture Rewards

A successfully captured Legendary creature becomes the single most prestigious possible Notable Specimen (§13–14) a Menagerie can ever hold — a tamed Griffin or a living Manticore is a real, standing wonder, drawing real attention from Celebrities & Influential Figures-adjacent circles and Education & Culture's own Renown Attracts Renown mechanic in its own right.

### 10.3 The Phoenix — A Unique Case

The Phoenix cannot be permanently killed by ordinary means, per its own real myth: a "slain" Phoenix simply burns away and is reborn from its own ashes after a real span of time, meaning it can never be a conventional Kill trophy. A household fortunate enough to actually keep one in its Menagerie holds something genuinely unique in this entire document — a Notable Specimen that, per its own myth, effectively never truly dies of old age or ordinary harm, the one real exception to §13's own honest "it ages and eventually dies" rule.

### 10.4 The Unicorn — Capture Only, By Design

Consistent with its own real myth, this document deliberately doesn't offer a Kill option with any real reward for the Unicorn — killing one for trophy is available but reads as a genuinely cruel, narratively costed act (a real, direct Dignitas and Scandal penalty) rather than an accomplishment, while a successful, gentle Capture is this creature's own real, singular purpose: a live Unicorn in the Menagerie, and, per the real historical belief in its horn's curative power, a standing, honest (now-confirmed-real, per this section's own override) minor Disease & Public Health benefit to the household that keeps it.

---

## 11. Repeat Encounters — Escaped, Not Always Ended

Not every Legendary Game encounter needs to end in a kill, capture, or death. A creature that overwhelms or evades the hunting party simply **escapes**, exactly as any other wild beast can — meaning a truly formidable Griffin or Hydra can be a real, recurring regional legend a household returns to challenge again with better preparation, rather than a single-use encounter permanently consumed the first time it's found.

---

# Part III — Taming, Collection & the Menagerie

## 12. From Captured to Tamed

A live Capture is not automatically a safe, tame animal — it's a real, ongoing process. A **Bestiarius** (a real, historically attested Latin term for a beast-handler, distinct from the Menagerie-Keeper's own more general Stewardship-driven upkeep role) works with a newly captured animal over real time, at real risk: an incompletely tamed animal can injure its handler, and the taming process itself can simply fail — for an exotic species per ordinary historical difficulty, or, for a Legendary creature, per that creature's own mythic temperament (a Griffin's territorial pride, a Manticore's real cunning).

### 12.1 Hunting Hounds — A Lighter, Practical Case

A household's own trained hunting pack is a real, practical asset directly feeding a Hunt's own success odds, maintained far more easily than an exotic capture. A particularly excellent, loyal hound can itself become a real Notable Specimen — a smaller-stakes, more personal, more emotionally direct version of this document's own provenance system than a prestige lion or Griffin ever offers.

---

## 13. Notable Specimens — A Real, Named, Provenanced Animal

Deliberately lighter than the full artifact-trilogy treatment (Books, Art, Masterworks) — an animal is a living thing, not a reproducible Work or a singular crafted object. A **Notable Specimen** is a real, individually named and tracked animal — a rare species, a beast that survived a great Hunt, a gift from a foreign ruler, or a beloved hunting hound — carrying a real name and origin story, a real provenance log, and a real lifespan: it ages and eventually dies of natural causes (the Phoenix, §10.3, being this document's one real, mythic exception), a genuine, honest difference from an inanimate Masterwork's indefinite persistence.

---

## 14. Legendary Specimens — When the Menagerie Houses a Myth

A captured creature from §7 is simply a Notable Specimen with `isLegendary: true` — no separate system, just the honest acknowledgment that a live Manticore in the household's own Menagerie is meaningfully different from a lion in every practical sense: unmatched Dignitas, a real draw for visiting dignitaries and scholars, a natural subject for a Naturalist's own authored Work (Books & Manuscripts §4), and, per §17, a genuinely more dangerous Escape risk than any ordinary Exotic animal if the household's own Bestiarius and Menagerie-Keeper ever let their guard down.

---

## 15. The Menagerie as a Real Collection

Villa's own Menagerie/Aviary room, previously a flat Dignitas number from owning "some exotic animals" in the abstract, now houses a real roster of Notable and Legendary Specimens alongside ordinary, undifferentiated Exotic Beasts stock for lesser animals — the same real depth-on-top-of-an-existing-passive-room pattern Books' Household Library and Art's Pinacotheca already established. The existing Menagerie-Keeper now manages this real roster directly, and a Menagerie holding several long-kept, storied specimens — especially a single Legendary one — is real, comparable prestige, potentially the single most famous private collection in the entire game.

---

## 16. Breeding

A rare, advanced Menagerie activity for a genuinely well-developed collection, treated with real honesty rather than trivial ease for ordinary Exotic species — many historically didn't breed reliably in ancient captivity. Legendary creatures are rarer still to breed successfully — a real, exceptional, Dynasty-Chronicle-guaranteed achievement on the rare occasion two Legendary Specimens produce genuine offspring, itself born as a new Legendary Specimen carrying its own parentage as the first line of its own provenance.

---

## 17. Escape — Resolving the Menagerie-Keeper's Own Flagged Mishap

A direct, concrete resolution of a real, standing gap: the Menagerie-Keeper has always been described as someone who "prevents a Dignitas-damaging mishap" without ever specifying what that mishap actually is. This document names it: an unmaintained, understaffed, or simply unlucky Menagerie can suffer a real **Escape** — a dangerous animal getting loose into the settlement. For an ordinary Exotic animal, this is a genuine, serious incident: real injury or death risk to bystanders, a real Legal & Court liability question for the household, and direct Scandal exposure. **A Legendary Specimen's escape is this document's own worst-case scenario** — a Griffin or Manticore loose near a settlement is a genuine, Military & Combat-scale emergency, not merely a local incident, and a household responsible for unleashing one carries real, severe, lasting Dignitas and legal consequences on top of whatever damage the creature itself causes before it's recaptured, killed, or driven off.

---

## 18. Acquisition Beyond the Hunt

A Notable or Legendary Specimen doesn't have to come from a Hunt at all:

- **Purchase** — an ordinary transaction through the Exotic Beasts trade good's own existing supply chain, for ordinary specimens only; a Legendary creature is never simply for sale on the open market.
- **Diplomatic Gift** — a real, well-documented ancient practice of exchanging exotic animals between rulers; a foreign power's own gift of a genuinely Legendary creature would be an extraordinary, narratively major diplomatic event in its own right (Diplomacy with Non-Roman Peoples).
- **Inheritance** — a long-kept, beloved family specimen passes down as a real, named part of a household's own continuity, per Succession & Dynasty.

---

# Part IV

## 19. Cross-System Integration

- **Activity Engine / Feasts:** the Hunt is this project's third fully-specified Activity Type.
- **Villa:** the Diaeta's own long-vague "hunting-adjacent Event" note and the Menagerie/Aviary's own flat Dignitas function are both given real, concrete mechanics here.
- **Companions & Court Positions:** the Menagerie-Keeper's own flagged "prevents a mishap" description is fully resolved (§17); the Bestiarius (§12) is a new, real position distinct from it.
- **Traits:** Hunter finally gets a concrete home; Naturalist's own existing bonus extends to correctly assessing a Legendary Game encounter's own real danger.
- **Masterworks & Unique Crafted Objects:** Hunting Gear is this document's own direct combat-adjacent input; a Legendary creature's own hide or material is a genuinely extraordinary future Masterwork's own origin story.
- **Military & Combat:** Legendary creatures resolve through the Combat Resolution Engine as a genuine new Irregular Combatant tier above Exotic Game; a rampaging Legendary creature or an escaped Legendary Specimen can escalate into a real campaign-scale response.
- **Resources & Goods:** the Exotic Beasts good remains the undifferentiated bulk-stock baseline; a Notable or Legendary Specimen is this document's own individually-tracked layer on top.
- **Books & Manuscripts:** a Naturalist's own authored Bestiary Work is a natural, direct product of real Hunt and Menagerie experience, including, remarkably, real firsthand Legendary Game material.
- **Legal & Court / Scandal:** an Escape is a genuine new liability case type and Scandal source, sharply elevated for a Legendary Specimen.
- **Diplomacy with Non-Roman Peoples:** live animal gift-exchange, including the rare possibility of a Legendary creature, is a real, vivid addition to that document's own existing gift mechanics.
- **Games & Spectacle:** a household can donate or sell a specimen into a public Venatio; a Legendary creature fighting in the arena would be the single greatest spectacle draw the entire system offers.
- **Private Ships & Shipping Ventures:** the Sea Serpent (§7–8) is a direct, real threat to a Ship's own voyage, distinct from an ordinary Storm or Piracy encounter.
- **Celebrities & Influential Figures / Education & Culture:** a Legendary Specimen is a genuine Renown Attracts Renown trigger in its own right.
- **Dynasty Chronicle:** every Legendary Game encounter, capture, escape, or breeding is real, guaranteed-weight, and likely to be the single most memorable entry in a household's own entire Chronicle.
- **Disease & Public Health:** injury from Dangerous, Exotic, or Legendary hunts and Escapes all resolve through that document's existing machinery; the Unicorn's own horn (§10.4) is a rare, direct, confirmed-real curative input.

---

## 20. Data Model

```
Hunt extends Activity {
  gameTier,                    // "common" | "dangerous" | "exotic" | "legendary"
  legendaryCreatureType,          // nullable — "nemeanLion" | "hydra" | "chimera" | "cerberus" | "griffin" |
                                  // "manticore" | "basilisk" | "phoenix" | "minotaur" | "harpies" |
                                  // "seaSerpent" | "unicorn"
  outcome,                     // "kill" | "capture" | "escaped" | "injuryOrDeath" | "routed"
  resultingTrophyId, resultingSpecimenId,
}

NotableSpecimen {
  specimenId, name, species,
  isLegendary: bool,               // §14
  legendaryCreatureType,             // nullable, matches Hunt's own enum
  origin,                       // "huntCapture" | "purchase" | "diplomaticGift" | "inheritance" | "bredOffspring"
  ownerHouseholdId, bestiariusId,
  tamingStatus,
  provenanceLog: [ ... ],
  parentSpecimenIds: [ ... ],
  isPhoenix: bool,                  // §10.3 — exempts from ordinary natural-death aging
  birthMonth, status,               // "alive" | "diedNaturally" | "diedInEscape" | "lostInEscape" | "gifted" | "sold"
}

HuntingTrophy {
  trophyId, huntId, hunterCharacterId, species,
  isLegendaryTrophy: bool,
  displayLocation,
}

MenagerieEscapeIncident {
  incidentId, householdId, specimenId,
  isLegendaryEscape: bool,            // §17 — gates the Military & Combat-scale response
  month, casualtiesOccurred: bool,
  legalLiabilityCaseRef, scandalRef, militaryResponseRef,
  specimenRecovered: bool,
}
```

---

## 21. Open Questions

- **All numeric sizing**, per convention.
- **Legendary Game encounter frequency and fairness.** How the game ensures a given playthrough gets a meaningful chance at this content without it becoming either absent or routine isn't specified.
- **Whether every Legendary creature should have a real counter-strategy discoverable through play** (fire for the Hydra and Chimera, an indirect approach for the Basilisk) versus some being simply harder and requiring brute strength — this document assumes the former for most entries but doesn't fully specify each one.
- **The Phoenix's own rebirth timing and location** — whether it always returns to the same site, and how long after a "kill" it reappears, isn't specified.
- **Legendary breeding's own real odds and whether cross-species Legendary breeding should ever be possible** (a griffin and a manticore, say) — left entirely open and, on the surface, likely not intended.
- **How a Legendary Specimen's presence should be advertised or hidden.** Whether owning one is automatically public knowledge (inviting theft, Espionage interest, or a jealous rival's own scheme) or can be kept discreet isn't addressed here.
