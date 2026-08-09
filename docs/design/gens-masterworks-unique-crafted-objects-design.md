# GENS — System Design: Masterworks & Unique Crafted Objects (§6.46, new)
*Polish and expansion pass. The third sibling in this project's own real artifact trilogy, alongside Books & Manuscripts (§6.44) and Art & Art Commissions (§6.45) — and the one built to be the most directly CK3-artifact-shaped of the three. Where a manuscript survives through Copies and a famous sculpture through Replicas, a Masterwork is deliberately singular from the moment it's forged: no copies, no replicas, one object, one history. This pass adds real Masterwork Sets with a combined bonus, Master Crafter reputation (Lineage and Posthumous Renown, mirroring Art's own treatment of its Artists), a genuine Ill Repute state for an object with a dark history, a formal Repair and Recovery path resolving the first pass's own open question about recapturing a lost item, a real Display home in the Villa's existing Armory Alcove and Pinacotheca, and several further categories — dueling weapons, hunting gear, a physician's instrument kit, a decorated ship's figurehead, and a gaming set.*

---

## Contents

1. Scope & Role — Singular by Design, Functional by Default
2. Creation — When an Exceptional Good Becomes a Masterwork
3. Categories
4. Masterwork Sets — Matched Pairs and Their Combined Bonus
5. Functional Use — What Wielding One Actually Does
6. Master Crafters — Reputation, Lineage, and Posthumous Renown
7. Provenance
8. Ill Repute — When an Object Carries Bad Fortune
9. Acquisition
10. Loss, Repair & Recovery
11. Heirloom Status — Passing a Masterwork Down
12. Display — The Armory Alcove and the Pinacotheca
13. Renown — The Legendary Tier
14. Cross-System Integration
15. Data Model
16. Open Questions

---

## 1. Scope & Role — Singular by Design, Functional by Default

Resources & Goods' Weapons, Armor, and Jewelry, the Quality tier system (Common/Fine/Exceptional), and every relevant production building (Armory, Goldsmith's Studio, Glassblower's Studio) all stay exactly as designed. This document adds the layer sitting above the abstract goods pool: a real, individually-tracked **Masterwork** — a single, unique object with its own name, its own maker, its own history, and, critically, a real functional effect when actually used rather than only a passive Dignitas number.

The deliberate structural difference from this document's own two siblings: Books' Work/Copy split and Art's Original/Replica split both exist because real ancient manuscripts and sculptures genuinely were reproduced. A masterwork sword or a signet ring was not, in the same sense — it is, by its very nature, one single object. This document embraces that rather than forcing a parallel structure it doesn't need: **every Masterwork is singular from creation.** There is no copy to fall back on if it's lost.

---

## 2. Creation — When an Exceptional Good Becomes a Masterwork

Any ordinary Exceptional-grade Weapon, Armor, Jewelry, or other eligible good (Resources & Goods §10) *can* simply remain an undifferentiated Exceptional item in the abstract goods pool, exactly as today. A **Masterwork** is what happens when a household deliberately commissions something further: a genuine, named commission at the relevant production building, assigned to a specific named Character holding **Master Craftsman** (the same Trait already doing this work for Art & Art Commissions' own Sculptor, Caelator, and Gemmarius positions), producing one single, permanent, individually-tracked object rather than adding to the fungible goods stockpile.

A Masterwork's own starting significance scales with the crafting Character's own skill and standing, exactly mirroring Art's own Work Significance Tier (Modest/Notable/Renowned) — a competent but ordinary smith can still produce a real, usable Masterwork; a genuinely celebrated one is what gives a new object a real head start toward true Renown (§13).

---

## 3. Categories

| Category | Crafting Position/Building | Real Grounding | Functional Domain |
|---|---|---|---|
| **Weapon or Armor** | Armory, a named smith | The existing Weapons/Armor chain, individualized | Military & Combat — equipped by a specific Character, not folded into an abstract Squad's Equipment Tier |
| **Dueling Weapon** *(new)* | Armory, a named smith | A weapon balanced and ornamented specifically for formal one-on-one combat rather than the battlefield | The Duelist Trait and the Duel interaction (Characters §9.6) directly |
| **Hunting Gear** *(new)* | Armory, a named smith | A fine bow or spear, real elite hunting equipment | The Hunter Trait, Villa's Diaeta hunting-adjacent Event |
| **Military Standard** | Armory/Caelator | A real, direct upgrade to Military & Combat's own existing standardIntact flag (§10 of that document) | A Squad's own morale anchor; its capture or loss is a real, named military and Dignitas crisis |
| **Signet Ring / Seal** | Goldsmith's Studio, a Gemmarius | A real, functional ancient practice — a signet ring genuinely authenticated a sealed document | Correspondence & Letters, Legal & Court |
| **Poison Ring** *(new)* | Goldsmith's Studio, a Gemmarius | A real, feared possibility in Roman court intrigue — a concealed-compartment ring for administering poison to a rival | A real, concrete Coercive tool for Characters' own Scheme engine (§10 of that document), tied to the Herbalist Trait's existing poison-adjacent bonus |
| **Regalia of Office** | Sculptor, Caelator | The curule chair and the fasces — real, attested physical symbols of Roman magistracy | Politics & Patronage — a real, felt prestige bonus while actually held and used in office |
| **Musical Instrument** | (a Character with real skill, no dedicated building required) | Real ancient instruments — the lyre, the cithara, the aulos | Education & Culture's Symposium, Games & Spectacle flavor |
| **Chariot** | Stable, Carpentry Workshop | The Circus's own existing chariot-racing content | Games & Spectacle — a real, named racing asset distinct from an ordinary team's equipment |
| **Ship's Figurehead** *(new)* | Sculptor | A real, attested ancient practice of ornamenting a vessel's prow | Private Ships & Shipping Ventures' own Custom Commissioning (§3 of that document) — a Masterwork figurehead as the ultimate decoration choice for a household's own Flagship |
| **Physician's Instrument Kit** *(new)* | (a Court Physician's own commissioned tools) | A real, attested category of fine ancient medical instruments | Disease & Public Health — a direct, real diagnostic/treatment quality bonus for the Court Physician who owns it |
| **Gaming Set** *(new)* | Carpentry Workshop, Goldsmith's Studio | Real, well-attested ancient board games and their finely-made playing pieces | A light, genuine Villa leisure/hosting flavor item, and a real, small Symposium-adjacent social touch |
| **Wondrous Mechanism** | (rare — see §3.1) | A genuinely real, well-documented ancient marvel: a real geared astronomical calculating device, dated to within this game's own era, is a real, attested archaeological find | Education & Culture's Cultural Prestige, Religion's Astrologer-adjacent Omens, Renown Attracts Renown |
| **Ceremonial/Religious Implement** | Caelator | An Augur's lituus staff, a Priest's ritual vessels | Religion — a real, direct Auspices/Favor reliability bonus |
| **Heirloom Jewelry** | Goldsmith's Studio, a Gemmarius | A specific, named, worn piece — a torque, a diadem, a bridal necklace | Familia's marriage market, Dignitas display |

### 3.1 Wondrous Mechanisms — A Rare, Marvel-Tier Exception

Worth calling out directly: this project's own timeline (133 BC–AD 235) genuinely overlaps with real, documented ancient engineering that still astonishes modern observers — a real, geared bronze mechanism for astronomical calculation, recovered from a real ancient shipwreck, dated to within decades of this game's own starting range. A Wondrous Mechanism is this document's own rare, marvel-tier category built on that real historical fact: vanishingly uncommon, achievable only through an Astrologer, Naturalist, or Engineer Trait-holding Character's own sustained, dedicated effort (or, more plausibly for most households, acquired rather than built, per §9), and carrying real, outsized Education & Culture and Religion prestige simply for existing.

---

## 4. Masterwork Sets — Matched Pairs and Their Combined Bonus

New this pass, and a genuinely fun, real collecting incentive worth adding directly: two or more Masterworks deliberately forged together, or by the same hand for a matched purpose — a sword and its own scabbard-and-belt, a full suit of matched Armor pieces, a Chariot paired with its own team's matched tack — can be flagged as a **Masterwork Set**. Owning every piece of a Set simultaneously grants a real, additional combined bonus beyond what any single piece provides on its own, a concrete, achievable collecting goal distinct from simply accumulating unrelated fine objects. A Set broken up — one piece sold, lost, or captured separately from the rest — loses the combined bonus immediately, though each remaining piece keeps its own individual functional value; reuniting a scattered Set is a real, satisfying, and rare achievement in its own right, worth a genuine Dynasty Chronicle entry when it happens.

---

## 5. Functional Use — What Wielding One Actually Does

The single clearest differentiator from Art & Art Commissions, which is deliberately display-and-provenance-only: a Masterwork is meant to be used.

- **Weapon/Armor:** equipped directly by a specific Character, it carries a real, direct bonus in Military & Combat's Combat Resolution Engine and in a Battlefield Duel specifically.
- **Dueling Weapon:** a real, direct bonus specifically in the formal Duel interaction, distinct from an ordinary battlefield Weapon's own bonus.
- **Hunting Gear:** a real quality bonus to a Diaeta hunting Event's own outcome.
- **Military Standard:** while intact and carried, a real, standing morale bonus to its own Squad.
- **Signet Ring/Seal:** a letter or document sealed with a specific Masterwork ring carries real, elevated authenticity.
- **Poison Ring:** a real, concrete tool improving the odds of a Coercive Scheme specifically involving poison, at the real, standing risk that being caught carrying one is itself deeply incriminating if a Scheme is ever exposed.
- **Regalia of Office:** a real Dignitas and reception bonus specifically while the holder is actively serving in the office that regalia belongs to.
- **Musical Instrument:** played by a skilled Character, a real, direct Symposium hosting-quality bonus.
- **Chariot:** a real, direct Circus racing-performance bonus.
- **Ship's Figurehead:** a real, standing Dignitas contribution to whichever Ship it's mounted on, particularly fitting for a Flagship.
- **Physician's Instrument Kit:** a real, direct bonus to the Court Physician's own diagnostic and treatment quality.
- **Gaming Set:** a light, real bonus to informal social hosting distinct from a full Symposium or Triclinium event.
- **Wondrous Mechanism:** a real, standing Cultural Prestige and Omen-reading reliability bonus simply by being housed in the household's own collection — the one real exception to this section's own "must be used" framing.
- **Ceremonial/Religious Implement:** used by an Augur or Priest, a real, direct Auspices reliability bonus.
- **Heirloom Jewelry:** worn or gifted, a real Dignitas display value and, offered as part of a betrothal, a genuine boost to Familia's own dowry/alliance-value negotiation.

A Masterwork left in storage, unused, still carries its own passive Dignitas value from Quality and Renown alone — using it is always the better payoff, never a requirement.

---

## 6. Master Crafters — Reputation, Lineage, and Posthumous Renown

New this pass, giving craftspeople the same real treatment Art & Art Commissions already gives its own Artists, rather than leaving crafting as an anonymous production step:

- **Reputation.** A smith or jeweler who produces several genuinely notable Masterworks accrues real, personal renown independent of their own household's standing — a "sword from this smith's own forge" carrying real weight in a negotiation or a gift, the craft equivalent of Celebrities & Influential Figures' own household-grown-celebrity pattern.
- **Lineage.** Exactly mirroring Art §3.1: a Renowned crafter's own apprentice, once producing independent, credited work, carries a real, earned head start toward their own eventual Renown — a real, cultivable workshop tradition across generations.
- **Posthumous Renown.** Exactly mirroring Art §3.2: when a Master Craftsman dies, every Masterwork bearing their name receives a one-time, permanent significance bump, the same honest "the catalogue stops growing, and that's exactly what makes it more prized" logic.

---

## 7. Provenance

Every Masterwork tracks the same real, accumulating history Books' Copies and Art's Pieces already do: every owner, every gift, every notable use (a Weapon that won a real Battlefield Duel, a Standard that survived a Catastrophic Defeat intact, a Signet Ring that sealed a treaty). This is, once again, this document's own direct contribution to "memory has weight" — and, in a sense, the most literal one of the three siblings, since a Masterwork's provenance is built from what it did, not merely who owned it.

---

## 8. Ill Repute — When an Object Carries Bad Fortune

New this pass, and the direct dark mirror of Renown (§13), mirroring the same real, honest instinct Private Ships & Shipping Ventures already built for a Ship's own "bad reputation" (§9 of that document): a Masterwork closely associated with a real betrayal, a Catastrophic Defeat, an assassination, or a household's own ruin can accrue a genuine, standing **Ill Repute** — a real, feared, superstition-driven reluctance among Superstitious or Zealous Characters (Traits §3-4) to wield, wear, or even keep the object nearby, read the same way Religion's own Omens system already weights those Traits. Ill Repute isn't purely a penalty to be minimized — a household with a genuinely Rational, unsuperstitious cast of Characters can shrug off a cursed reputation entirely and simply enjoy an otherwise-excellent object at a discount, or a Bold, image-conscious house might even lean into an object's own dark fame deliberately, wearing the notoriety as a real, if unusual, kind of prestige. An object can hold both Renown and Ill Repute at once — genuinely famous and genuinely feared are not mutually exclusive, and the combination is some of this document's own most interesting material.

---

## 9. Acquisition

The same real paths this document's siblings already established, with real category-specific emphasis:

- **Commission** (§2) — the primary, deliberate path for most categories.
- **Capture** — the signature acquisition path for a Weapon, Standard, or Chariot specifically: Military & Combat's own existing capture mechanics (§5.5 of that document, and War Spoils, §7) can yield a named enemy Masterwork directly.
- **Purchase/Gift/Inheritance/Theft** — identical in shape to Books §7 and Art §8, reused directly.
- **Discovery** — a rare Travel encounter, and this document's own most natural home for a Wondrous Mechanism (§3.1).

---

## 10. Loss, Repair & Recovery

- **Damage, not always destruction.** A Masterwork's own Condition can be degraded — by ordinary wear, a lost Battlefield Duel, a Natural Disaster — without the object being destroyed outright. A damaged Masterwork can be genuinely repaired, at real cost, by any sufficiently skilled Master Craftsman (ideally, though not necessarily, its own original crafter or their trained apprentice per §6's Lineage), following the same real logic Estate & Settlement's own Repair action and Private Ships' own Shipyard maintenance already use.
- **Capture in battle** is the mirror image of §9's own acquisition path — a Masterwork Weapon or Standard can be lost to an enemy the same way it can be won from one.
- **Recovery, resolved.** Per the first pass's own open question: a captured Masterwork can be recovered through any of three real paths — a formal Ransom negotiation (Characters §9.5) with whoever currently holds it, an ordinary Purchase if it's since changed hands on the open market, or a deliberate Military & Combat campaign objective specifically aimed at recovering it, giving a lost family sword or standard a real, playable reason to go to war beyond ordinary territorial or political stakes.
- **Melting/breaking down** — a metal Masterwork carries the same real risk Art & Art Commissions already established for bronze sculpture: Insolvency, a war-funding effort, or a formal Damnatio Memoriae can see it melted for raw material value.
- **Simple loss** — misplaced, stolen and never recovered, or lost alongside its owner in a shipwreck (Private Ships' own Presumed Lost outcome) or a Piracy & Banditry raid.
- **No Copy, no Replica, no safety net.** Because every Masterwork is singular by design (§1), its destruction is always total and always permanent — there is no Fragmentary-survival equivalent here. A lost Masterwork is simply gone, the honest cost of this category's own singular nature.

---

## 11. Heirloom Status — Passing a Masterwork Down

A real, direct CK3-style mechanic worth building explicitly: a household can formally designate a specific Masterwork as an Heirloom, meaning Succession & Dynasty's own inheritance division (§6.9 of that document) routes it directly and automatically to the new head of household, regardless of how the rest of the estate is otherwise divided among heirs. An Heirloom is a real, standing statement about what a family considers irreplaceable, and losing one accordingly carries a real, elevated Dynasty Chronicle and Memoria (Ancestor Veneration & Funerary Customs §6) weight beyond an ordinary Masterwork's own loss.

---

## 12. Display — The Armory Alcove and the Pinacotheca

New this pass, resolving where a collected Masterwork actually lives: Villa's own existing Armory Alcove (§4.6 of that document) — already described as a personal weapons-and-armor display room, distinct from the settlement's production Armory — is this document's own natural home for a Weapon, Armor, Dueling Weapon, or Hunting Gear Masterwork, requiring no new room. Every other category (Regalia, Musical Instruments, Signet Rings, Gaming Sets, Wondrous Mechanisms, Ceremonial Implements, Heirloom Jewelry) displays through the existing Pinacotheca, under the same Curator (Companions & Court Positions §5.1) Art & Art Commissions already gave real teeth — a Masterwork's own Renown and Ill Repute both contribute to that room's own collection-prestige reading exactly the way a Renowned ArtworkPiece already does.

---

## 13. Renown — The Legendary Tier

A sufficiently storied Masterwork — one with a rich Provenance log (§7), particularly one carrying real Heirloom status (§11) across multiple generations — rises to a formal Renowned tier, and, rarer still, a genuine Legendary tier above it: an object whose name alone carries real recognition and Dignitas independent of its owner, the natural ceiling this whole trilogy has been building toward. A Legendary Masterwork is real, guaranteed Dynasty Chronicle material on every occasion it changes hands, and a natural target for Celebrities & Influential Figures-style attention in its own right — not a famous person, but a famous object.

---

## 14. Cross-System Integration

- **Books & Manuscripts / Art & Art Commissions:** this document is the third, deliberately structurally distinct sibling — singular rather than Copy/Replica-based, functional rather than passive-display-first; §6's Master Crafter treatment directly mirrors Art §3.1-3.2.
- **Resources & Goods:** the Quality tier system is this document's own direct starting gate.
- **Military & Combat:** a Masterwork Weapon/Armor is equipped by a specific Character; the Military Standard category directly upgrades the existing standardIntact flag into a real, named object; capture and recovery (§9, §10) mirror and extend that document's own captured-commander mechanics.
- **Companions & Court Positions / Traits:** Master Craftsman, Duelist, Hunter, Herbalist, and the Court Physician's own role all get real, concrete objects tied to them.
- **Correspondence & Letters / Legal & Court:** a Signet Ring's own sealing function is a real, concrete authenticity mechanism for both systems.
- **Politics & Patronage:** Regalia of Office carries a real, active-use prestige bonus tied directly to that document's own magistracy ladder.
- **Education & Culture:** Musical Instruments, Gaming Sets, and Wondrous Mechanisms all feed Cultural Patronage and Renown Attracts Renown directly.
- **Games & Spectacle:** a named Chariot is a real, distinct racing asset.
- **Religion:** Ceremonial/Religious Implements carry a real Auspices reliability bonus; Ill Repute (§8) reads the same Superstitious/Zealous Trait weighting Omens already uses.
- **Private Ships & Shipping Ventures:** the Ship's Figurehead (§3) is a direct extension of that document's own Custom Commissioning decoration choice; a Presumed Lost voyage is a real, evocative way for a Masterwork to disappear alongside its owner or shipment; that document's own Ship Reputation mechanic is this document's direct precedent for Ill Repute (§8).
- **Familia / Succession & Dynasty:** Heirloom Jewelry feeds the marriage market; §11's Heirloom designation is a direct, real extension of inheritance division.
- **Ancestor Veneration & Funerary Customs:** losing an Heirloom Masterwork is a real, additional Memoria consideration.
- **Piracy & Banditry:** a captured Masterwork is real, high-value plunder distinct from ordinary goods.
- **Characters:** the Poison Ring (§3) and Ransom-based recovery (§10) both reuse that document's own existing Scheme and Interaction machinery directly.
- **Villa:** the Armory Alcove and Pinacotheca (§12) are this document's own real display venues, requiring no new room.
- **Celebrities & Influential Figures:** a Legendary Masterwork is real Fame material in its own right, independent of any Character.
- **Dynasty Chronicle:** every Heirloom transfer, every capture and recovery, a reunited Masterwork Set, and every Legendary designation are all real, guaranteed-weight material.
- **Scandal:** a stolen or lost family Heirloom, or public exposure of a household's own Poison Ring, are both real, felt Scandal material.

---

## 15. Data Model

```
Masterwork {
  masterworkId, name, category,
  crafterCharacterId, creationMonth,
  significanceTier,
  quality,
  condition,
  setId,
  isHeirloom: bool,
  illRepute: bool,
  linkedSquadId,
  linkedMagistracyType,
  linkedShipId,
  currentOwnerCharacterOrHouseholdId,
  status,
}

MasterworkSet {
  setId, setName,
  memberMasterworkIds: [ ... ],
  allPiecesOwnedByOneHousehold: bool,
}

ProvenanceEvent {
  eventId, masterworkId, month,
  eventType,
  fromCharacterOrHouseholdId, toCharacterOrHouseholdId,
}

CommissionProject {
  projectId, crafterCharacterId, commissioningHouseholdId,
  category, startMonth, monthsInProgress,
  resultingMasterworkId,
  apprenticeCharacterId,
}

HeirloomDesignation {
  masterworkId, householdId,
  designatedMonth,
  automaticSuccessionRouting: bool,
}

MasterCrafterReputation {
  characterId,
  masterworksCreated: [ masterworkId, ... ],
  lineageMasterId,
  posthumousBumpApplied: bool,
}
```

---

## 16. Open Questions

- All numeric sizing, per convention — commission cost/duration, the functional bonus magnitude per category, Set combined-bonus size, Repair cost, and Significance/Renown/Ill Repute thresholds are all unsized.
- Multiple Heirlooms per household. §11 doesn't cap how many Masterworks can hold Heirloom status simultaneously, nor address what happens if a household's holdings genuinely can't keep every designated Heirloom together.
- Wondrous Mechanism's own real functional depth. §3.1 and §5 give this category a real prestige bonus but deliberately don't attempt to model any genuine calculating or predictive function the real device apparently had.
- Interaction with Art & Art Commissions' own Forgery mechanic. A false claim of owning a famous Legendary Masterwork is a real, plausible parallel, but isn't formally built here.
- Ill Repute's own removal path. §8 establishes the state and its real, mixed reception, but doesn't specify whether a sufficiently positive new Provenance event can ever fully clear it, or whether it's permanent once earned.
- Set discovery. §4 assumes the player generally knows which Masterworks belong to a given Set, but doesn't address whether a genuinely obscure historical Set could be rediscovered as a surprise once enough pieces are independently reunited.
