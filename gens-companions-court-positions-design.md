# GENS — System Design: Companions & Court Positions (§6.20)
*Final polish pass — now self-contained: the full Villa roster is reproduced here rather than only cross-referenced, the Overseer tier is expanded, several new positions are added, and a full alphabetical Position Index closes the document.*

---

## Contents

1. Scope & Role
2. The Three-Tier Staffing Model
3. Household Staff Positions (Recap)
4. Overseers
5. Senior/Court Positions — Full Roster
6. Promotion Pathways
7. The Travel Retinue
8. Full Position Index
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

§6.20 bundles two related but distinct things under one system-list entry, and this document formalizes both:

- **The appointable staffing ladder** — Labor duty slots, Overseers, and Senior/Court Positions — spanning the Villa (this pass folds in the Villa doc's §9 roster in full, rather than only cross-referencing it) and the whole of Estate & Settlement's building roster, out to provincial-scale appointments a growing gens eventually acquires.
- **The Travel Retinue** — who physically accompanies the player character when they travel. This is *not* a separate recruited character class with its own arcs or stat block. "Companion" in the core doc's system list just means retinue composition: an existing Familia member brought along on a journey, contributing whatever stats and Position they already have.

Both halves share a throughline: nearly every building, room, and journey in this design has an obvious person who'd actually run it, and this document is where that person gets a title, an attribute, and a mechanical effect. This pass leans harder into that throughline than the last one did — several rooms and public buildings that previously had no named operator get one below, and the document now stands on its own without needing the Villa doc open alongside it.

---

## 2. The Three-Tier Staffing Model

Familia (§4) splits appointable roles along two axes: **Labor duty slots** (Labor Skills, open to anyone Adolescent-or-older regardless of status) and **Court Positions** (Core Attributes, the higher tier). Labor & Slavery (§4) introduced the **Vilicus** as a third, intermediate tier specific to field labor — above a duty slot, below a true Court Position, evaluated on Stewardship, moderating the output and Regimen compliance of everyone beneath them.

This document generalizes the Vilicus pattern into a named tier — the **Overseer** — and applies it across every major building category the Estate & Settlement and Buildings docs define, not just fields. The result is one consistent three-tier model everywhere in the game:

| Tier | Skill axis | Scale | Example |
|---|---|---|---|
| **Labor Duty Slot** | Labor Skills | One building or field | Field Hand, Ironworker, Weaver |
| **Overseer** | Mostly Stewardship (a Core Attribute) | One building or a tight cluster of them | Vilicus, Magister Officinae, Institor |
| **Senior/Court Position** | Core Attributes | A whole Villa, settlement-wide category, or province | Steward, Praefectus Metallorum, Procurator |

Legal status never gates which tier someone can reach — an enslaved individual, a freedman, a client, or a family member can all occupy any of the three, exactly as Familia §2.5 and §5.1 below already establish for Court Positions specifically.

**Coverage before Vicus stage.** Most Senior Positions in §5.2 don't unlock until Estate & Settlement reaches Vicus stage or later. Before that, the Villa's own **Steward (Dispensator)** — the first Senior Position any household has, from Rustica stage onward — implicitly covers general administrative efficiency across whatever few Estate & Settlement buildings a Rustica-stage household has managed to put up. Nothing is left unsupervised at the start of a game; the roster below simply differentiates as the estate grows past what one Steward can reasonably run alone.

---

## 3. Household Staff Positions (Recap)

Familia §4 and §5.1 below cover the base Labor Duty Slot tier in full — Field Hand, Domestic Servant, Cook, Craftsman, and so on, drawn from Labor Skills and open to any eligible household member. This document doesn't restate that; it only extends the *pattern* outward: an Ironworks needs Ironworkers the same uncomplicated way a Culina needs a Cook, and every building in the Buildings doc gets its labor filled the same way without needing its own bespoke duty-slot design.

---

## 4. Overseers

### 4.1 What an Overseer Does

Identical in function to the Vilicus, just applied estate-wide: an Overseer is assigned to a specific building instance or a tightly bound production cluster (a single Latifundium, a single Ironworks, a paired Fishing Wharf + Salt Pans feeding one Garum Works). A good Overseer raises that building's effective output above what raw Labor Skill alone would produce; a poor or resented one drags it down — the same tradeoff Labor & Slavery §4 already describes for the Vilicus, now generalized.

Most Overseer roles read primarily off **Stewardship**, but a few lean on a second attribute where the building's real character calls for it (a mine's Overseer reading some Martial for safety/revolt-risk, a Slave Market's Overseer reading some Intrigue for managing deception odds). Skilled Overseer candidates command the same acquisition price premium Labor & Slavery §4 already establishes for skilled labor generally.

### 4.2 Overseer Roster by Building Category

This pass upgrades several titles toward real attested Latin terms where one exists, and resolves a naming collision with the settlement-wide Praefectus Vigilum (§5.2) by renaming the building-level fire-watch role to a plain **Vigil**. See §11 for which titles below are confirmed historical terms versus reasonable functional constructions.

| Category | Role | Buildings | Primary Attribute | Notes |
|---|---|---|---|---|
| **Agriculture** | **Vilicus** | Fields, Groves, Vineyards, Pastures, Latifundium-tier estates | Stewardship | Real, attested term; the origin case for this whole tier (Labor & Slavery §4) |
| **Industry** | **Magister Officinae** (Workshop-Master) | Ironworks, Bronzeworks, Glassworks, Potter's Works, Weaver's Loom, Tannery, and similar workshop buildings | Stewardship | *Officina* ("workshop") is a real term; the full title is this document's construction |
| **Extraction** | **Metallarius** | Iron/Copper/Tin/Gold/Silver Mines, Marble/Stone Quarries | Stewardship (+ Martial) | Attested term for a miner, repurposed here as the overseer; the Martial lean reflects real mine-labor danger and elevated revolt-risk (Labor & Slavery §7) — a poor Metallarius raises Unrest faster than an equivalent workshop failure would |
| **Commerce** | **Institor** | Market Stall/Market/Trading Post/Emporium, Storehouse/Warehouse | Diplomacy (+ Stewardship) | Real term for a commercial agent/factor |
| **Trade administration** | **Portitor** *(new)* | Customs House/Portorium | Diplomacy (+ Stewardship) | Real term for a customs/toll collector; the concrete operator behind that building's future tariff hooks into Policies & Edicts (§6.12) |
| **Infrastructure** | **Aquarius** *(new)* | Aqueduct, Cistern | Stewardship | Real term (a water-system manager); reliability here is a direct input to Natural Disasters' drought mitigation and to every Cistern-gated Civic building |
| **Public granary** | **Horrearius** *(new)* | Horreum | Stewardship | Real term (a warehouse/granary keeper); the settlement-scale counterpart to the Villa's private Granary-Keeper (§5.1) |
| **Religion (public)** | **Sacerdos Publicus** | Shrine, Temple | Learning | Distinct from the Villa's private *Sacerdos Domesticus* (§5.1), who serves only the household Lararium; "Sacerdos" is real, the "Publicus" qualifier is this document's construction |
| **Education** | **Rhetor/Magister** | School, Academy | Learning | Both real terms for a teacher; feeds Education & Culture (§6.14) for the wider settlement, distinct from the Villa's *Paedagogus* |
| **Entertainment (training)** | **Lanista** | Ludus (Gladiator School) | Martial | Real, attested term; ties directly to Games & Spectacle (§6.22) and Labor & Slavery's gladiator labor subtype |
| **Entertainment (venue)** | **Editor** | Amphitheater, Circus, Theatre, Odeon | Diplomacy | Real term — the sponsor/stager of a show; manages logistics, billing, and (for the Amphitheater/Circus) wagering |
| **Health (public)** | **Valetudinarius** | Valetudinarium | Learning | Constructed from the real building name; the public counterpart to the Villa's private Court Physician/Iatreion |
| **Slave trade** | **Venalicius** | Slave Market (Venalicium) | Intrigue (+ Stewardship) | Real, attested term for a slave-dealer; manages the rotating stock and deception odds Labor & Slavery §3 describes — a corrupt or careless Venalicius is the direct mechanical route to that building's Dignitas liability |
| **Licensed vice** | **Leno / Lena** *(revised)* | Brothel (Lupanar) | Diplomacy | Real, attested terms (male/female brothel-keeper); Buildings doc §4.8 already recommends arm's-length management through a freedman or client specifically to avoid the owning gens's Dignitas cost — this role is that recommendation made concrete |
| **Banking** | **Argentarius** | Argentaria | Stewardship | Real term for a banker; ties to Economy & Finance's debt mechanics and Legal & Court disputes over defaulted loans |
| **Fire/security (civic, building-level)** | **Vigil** *(renamed)* | Vigiles Post, individual Watchtowers | Martial | Plain, real term for a watchman; the settlement-wide command tier is the **Praefectus Vigilum** Senior Position (§5.2), avoiding the earlier title collision |
| **Charitable welfare** | **Alimentarius** *(new, flagged)* | Alimenta/Orphanage | Stewardship | A functional coinage rather than a confirmed single-title term — real Roman *alimenta* welfare schemes existed, but no attested individual "manager" title is used with confidence here; worth revisiting in a historical-accuracy pass |
| **Funerary** | **Libitinarius** *(new)* | Necropolis | Learning | Real, attested term (an undertaker, named for Libitina, goddess of funerals); ties to Religion (§6.6) and gives Succession & Dynasty's death-handling a concrete operator distinct from the private Family Tomb, which needs none |
| **Naval** | **Navarchus** | Shipyard/Navalia | Martial (+ Stewardship) | Real term for a ship's captain, repurposed here as the shipyard's overseer; builds and crews warships as an active Piracy & Banditry countermeasure, the counterpart to the Lighthouse's passive one |

---

## 5. Senior/Court Positions — Full Roster

### 5.1 Villa Positions — Full Roster

Reproduced here in full from the Villa doc's §9.2, with an added **Attribute** column for consistency with this document's own tables, plus four new additions (marked *new*) for rooms that previously had no named operator.

| Stage | Position | Tied Room | Attribute | What Filling It Does |
|---|---|---|---|---|
| Rustica | **Steward** (*Dispensator*) | Tablinum | Stewardship | Administrative efficiency scales with the Steward's Stewardship rather than being flat |
| Rustica | **Bodyguard** | Ostium | Martial | Personal security |
| Rustica | **Household Priest** (*Sacerdos Domesticus*) | Lararium | Learning | Religion's household rituals/omens read the Priest's standing |
| Urbana | **Secretary** (*Amanuensis*) | Private Scriptorium | Diplomacy | Correspondence efficiency/reach |
| Urbana | **Cellarer** (*Promus*) | Apotheca/Grand Cellar | Stewardship | Wine-aging quality feeding Triclinium/Oecus events |
| Urbana | **Head Cook** (*Archimagirus*) | Culina | Stewardship | Hosting-event quality |
| Urbana | **Tutor** (*Paedagogus*) | Bibliotheca | Learning | Children's Education & Culture progress |
| Urbana | **Nurse** (*Nutrix*) | Nursery | Learning | Infant Health/mortality-risk mitigation |
| Urbana | **Weaving-Mistress** (*Magistra Textrinii*) | Textrinum | Stewardship | Cloth output and quality — often the *materfamilias* herself |
| Urbana | **Master of Hospitality** (*Xenodochus*) | Xenodochium | Diplomacy | Guest-capacity management and how a hosted guest is treated |
| Urbana | **Balneator** *(new)* | Balneum / Balneum Completum | Learning | Bathing-sequence quality — the Disease & Public Health benefit and the Dignitas payoff of hosting a guest through it scale with the Balneator's care, rather than the room functioning identically whether staffed or not |
| Domus | **Chamberlain** (*Cubicularius Maior*) | Master Suite/family wing | Stewardship | Overall family-affairs administration, a step above the Steward's estate focus |
| Domus | **Household Spymaster** | Cryptoporticus | Intrigue | Espionage & Information Network |
| Domus | **Guard-Captain/Marshal** | Armory Alcove + Ostium | Martial | Personal-combat readiness and Villa-wide security, above a single Bodyguard's scope |
| Domus | **Court Physician** (*Medicus*) | Iatreion | Learning | Disease & Public Health at the personal scale |
| Domus | **Treasurer** (*Arcarius*) | Private Strongroom/Treasury | Stewardship | Theft/loss risk reduction, Economy & Finance |
| Domus | **Master Beekeeper** (*Apiarius*) | Private Apiary | Stewardship | Honey output |
| Domus | **Furnace-Master** (*Fornacator*) | Praefurnium | Stewardship | Heating reliability/efficiency |
| Domus | **Menagerie-Keeper** | Menagerie/Aviary | Stewardship | Upkeep of exotic animals, prevents a Dignitas-damaging mishap |
| Domus | **Harbor-Steward** *(coastal only)* | Private Dock/Boathouse | Stewardship | Sea-Travel readiness, Piracy exposure management |
| Domus | **Dovecote-Keeper** (*Columbarius*) | Columbarium | Stewardship | Correspondence & Letters reliability once that system exists |
| Domus | **Baker-in-Chief** (*Pistor*) | Private Pistrinum | Stewardship | Bread self-sufficiency output |
| Domus | **Granary-Keeper** | Cella Frumentaria | Stewardship | Famine-buffer size/reliability |
| Domus | **Curator** *(new)* | Pinacotheca | Stewardship | Collection care and arrangement — turns the Pinacotheca's Dignitas display from a flat effect of owning the goods into something that scales with how well they're kept and shown |
| Domus | **Ergastularius** *(new)* | Ergastulum | Martial | The enforcer who physically runs the household's harshest Regimen tier (Labor & Slavery §5) and administers Severe/Lethal punishments (that doc's §6) that happen there — a role the Villa doc named the room for but left unstaffed |
| Urbana *(Greek East)* | **Symposiarch** *(new)* | Andron/Symposium Room | Learning | Real Greek term for the host/master of ceremonies at a symposium; leans the room toward Education & Culture per its own description rather than the Triclinium's more purely social hosting function |

That's a genuinely large roster by Domus/Palace stage — closer to two dozen possible appointments than a CK3-style council's half-dozen — but exactly as the Villa doc establishes, the player is never obligated to fill all of them. An empty Cellarer just means the Apotheca ages wine at its unstaffed default rather than the game demanding a full staff roster before anything works.

### 5.2 Estate & Settlement Senior Positions

A settlement-scale tier, unlocked as Estate & Settlement's own stage advances (§5 of that doc: Villa → Vicus → Town → City), sitting **above individual Overseers** the same way a Villa Chamberlain sits above a single Cellarer. This pass upgrades several titles toward fuller Latin constructions and resolves the Vigil/Praefectus Vigilum naming collision noted in §4.2.

| Stage | Position | Oversees | Attribute | Notes |
|---|---|---|---|---|
| **Vicus** | **Actor** (Master of the Fields) | Every Vilicus | Stewardship | Real Roman term for an estate agent; aggregates an Agriculture output bonus across all held fields/groves/vineyards |
| **Vicus** | **Institor Maximus** (Master Factor) | Every Institor and Portitor | Diplomacy | Aggregate Commerce bonus; the natural appointee to negotiate a contested-plot situation (Estate & Settlement §7) on the player's behalf |
| **Town** | **Praefectus Metallorum** (Master of Mines) | Every Magister Officinae and Metallarius | Stewardship | Estate-wide Industry/Extraction output bonus; also the appointee who most directly suppresses the elevated Unrest a poorly-run mine generates |
| **Town** | **Praefectus Vigilum** (Prefect of the Watch) | Every Vigil, Watchtower, City Walls | Martial | Real, attested historical title (the actual commander of Rome's Vigiles); settlement-wide suppression of Piracy & Banditry, fire risk (Natural Disasters §6.17), and general Unrest |
| **Town** | **Navarchus Princeps** (Navarch-in-Chief) | Every Navarchus, the Port/Harbor | Martial/Stewardship | Coordinates sea trade protection and naval readiness across multiple coastal holdings at once |
| **City** | **Editor Muneris** (Master of Games) | Amphitheater, Circus, Theatre, Odeon | Diplomacy | The actual game-throwing authority behind Games & Spectacle's Dignitas/Politics payoff, above the venue-level Editor |
| **City** | **Tabularius** (Master of Records) | Tabularium | Learning/Intrigue | Settlement-wide legal record-keeping; feeds Legal & Court (§6.16) filings and the Dynasty Chronicle (§6.11) directly |
| **City** | **Procurator** | An entire second settlement (Estate & Settlement §7's late-game possibility) | Stewardship | See §5.3 |
| **City** | **Rationalis** *(new, capstone)* | The economic Senior Positions as a set (Treasurer, Argentarius, Institor Maximus, Cellarer) | Stewardship | See §5.4 |

### 5.3 The Procurator — Resolving Second-Settlement Management

Estate & Settlement §9 flagged an open question: whether a second settlement, once acquired, is independently managed, run by an appointed steward, or merged into a unified view. This document resolves it: a **Procurator** — evaluated exactly like any other Senior Position, on Core Attributes and standing — runs a second settlement in the player's absence. Mechanically, this mirrors §6.28's Steward auto-management principle (sensible default handling of routine business, anything consequential held for the player's attention) but *permanently* rather than only during Travel, since the player's own character can't be in two settlements at once. A weak or disloyal Procurator is a real liability — an underperforming second settlement, or in the worst case a Procurator who begins acting in their own interest, is squarely this system's problem to surface rather than a silent background number.

### 5.4 The Rationalis — A Capstone for the Economic Roster

New this pass. By City stage, a household can plausibly be running a Treasurer, an Argentarius, an Institor Maximus, and a Cellarer all at once — four Stewardship-leaning appointments whose effects genuinely interlock (wine quality feeds hosting, hosting feeds Politics, trade feeds the treasury, the treasury funds everything else) but who otherwise have no relationship to each other in the data model. The **Rationalis** — a real term for an imperial-style chief financial officer, borrowed here at household scale — is a single City-stage appointment coordinating that whole cluster: a small compounding bonus when the roles beneath it are *also* filled competently, rather than a new independent effect of its own. This gives the economic side of the roster the same kind of capstone the security side already effectively has in the Praefectus Vigilum, without inventing a new mechanic to do it.

### 5.5 The Line Against Politics & Patronage

None of the positions in §5.1–5.4 constitute actual Roman **public office**. A Procurator, a Praefectus Vigilum, or a Rationalis is a private household appointment managing the player's own domestic and economic interests — even where the title borrows real terms Rome also used for public administrators. Actual magistracies, the cursus honorum, and provincial governorships remain entirely Politics & Patronage's (§6.5) domain and are deliberately out of scope here. This line is worth holding explicitly when that system gets its own design pass, since several titles above (Procurator especially, and now Rationalis) had real public meanings historically that this document is deliberately not claiming.

---

## 6. Promotion Pathways

The three tiers form a real ladder, not just a classification scheme:

**Labor Duty Slot → Overseer.** A standout performer in a duty slot — high relevant Labor Skill, good Loyalty, demonstrated reliability — becomes eligible for promotion to Overseer of that same building or cluster. This is the concrete, everyday version of the "labor continuity" principle Labor & Slavery §8 already establishes for manumission: rising through this ladder doesn't require a change in legal status first.

**Overseer → Senior/Court Position.** Requires broader standing than one building's performance — the Core Attributes a Senior Position reads (rather than Overseer's near-universal Stewardship lean) and enough visibility (Loyalty, a track record, sometimes an outright political or family push) to be trusted with an estate-wide or Villa-wide remit rather than a single building.

**Skipping tiers.** The ladder is the *typical* path for an enslaved or lower-status individual climbing on demonstrated performance; it isn't a hard gate for everyone. A family member, an educated freedman, or a political ally with obviously strong Core Attributes can be appointed directly to a Senior Position without ever holding an Overseer post — the same way a real Roman household's own son might become its Steward without first running a single field.

**Manumission interaction.** Climbing this ladder is itself one of Labor & Slavery §8's concrete "promotion into a Court Position" triggers, and — like any major status change — a natural Chronicle entry (§6.11): the household remembers who rose from Field Hand to Vilicus to Actor over three decades of service.

---

## 7. The Travel Retinue

### 7.1 What It Is (and Isn't)

When the player character travels (§6.18), they assemble a **retinue** from eligible Familia members — family, freedman, client, or trusted slave, including anyone already holding an Overseer or Senior Position — up to a capacity limit. This is the entirety of what "Companions" means in the core doc's system list. There is no separate Companion character type, no dedicated stat block, and no bespoke recruitment pool distinct from the ordinary household — a person is either part of the Familia roster or they aren't, and if they are, they're eligible to travel.

### 7.2 What a Retinue Member Contributes

Each retinue member contributes using whatever stats and Position they already hold, applied to the travel context rather than read through any new system:

- A **Bodyguard** or **Marshal** reduces ambush/Piracy risk en route.
- A **Secretary (Amanuensis)** handles correspondence drafted on the road, keeping the Correspondence & Letters system (§6.27) available even while traveling.
- A **Court Physician** or **Valetudinarius** reduces the trip's Disease/injury exposure.
- A family member brought along for a specific purpose (a betrothal negotiation, a political visit) adds their own relevant Core Attribute to that encounter directly.
- An Overseer or Senior Position holder brought along essentially leaves their post unstaffed for the duration — the same tradeoff as any other absence, and the natural moment §6.28's auto-management QoL layer covers for whatever's left behind.

### 7.3 Recruitment via Travel

Travel remains "the primary gateway into Companion recruitment" exactly as the core doc states — certain Travel encounters can result in a new person joining the household outright (a promising client's son, a stranded freedman, a skilled captive). That person becomes a full Familia record the moment they join, per Familia §7's existing promotion rule, and from that point on is simply an ordinary household member: eligible for a Labor Duty Slot, an Overseer post, a Senior Position, or the Travel Retinue itself, like anyone else. Nothing about how they joined the household persists as a special flag.

---

## 8. Full Position Index

*(Alphabetical, each pointing to its home tier and section.)*

Actor §5.2 · Alimentarius §4.2 · Amanuensis (Secretary) §5.1 · Aquarius §4.2 · Archimagirus (Head Cook) §5.1 · Arcarius (Treasurer) §5.1 · Argentarius §4.2 · Balneator §5.1 · Bodyguard §5.1 · Chamberlain (*Cubicularius Maior*) §5.1 · Cellarer (*Promus*) §5.1 · Curator §5.1 · Dovecote-Keeper (*Columbarius*) §5.1 · Editor §4.2 · Editor Muneris §5.2 · Ergastularius §5.1 · Furnace-Master (*Fornacator*) §5.1 · Granary-Keeper §5.1 · Guard-Captain/Marshal §5.1 · Harbor-Steward §5.1 · Head Cook (*Archimagirus*) §5.1 · Horrearius §4.2 · Household Priest (*Sacerdos Domesticus*) §5.1 · Household Spymaster §5.1 · Institor §4.2 · Institor Maximus §5.2 · Lanista §4.2 · Lena / Leno §4.2 · Libitinarius §4.2 · Magister Officinae §4.2 · Magistra Textrinii (Weaving-Mistress) §5.1 · Master Beekeeper (*Apiarius*) §5.1 · Master of Hospitality (*Xenodochus*) §5.1 · Menagerie-Keeper §5.1 · Metallarius §4.2 · Navarchus §4.2 · Navarchus Princeps §5.2 · Nurse (*Nutrix*) §5.1 · Paedagogus (Tutor) §5.1 · Portitor §4.2 · Praefectus Metallorum §5.2 · Praefectus Vigilum §5.2 · Procurator §5.2, §5.3 · Promus (Cellarer) §5.1 · Rationalis §5.2, §5.4 · Rhetor/Magister §4.2 · Sacerdos Publicus §4.2 · Secretary (*Amanuensis*) §5.1 · Steward (*Dispensator*) §5.1 · Symposiarch §5.1 · Tabularius §5.2 · Treasurer (*Arcarius*) §5.1 · Tutor (*Paedagogus*) §5.1 · Valetudinarius §4.2 · Venalicius §4.2 · Vigil §4.2 · Vilicus §4.2 · Weaving-Mistress (*Magistra Textrinii*) §5.1

---

## 9. Cross-System Integration

- **Labor & Slavery (§6.3):** the Vilicus is this document's Agriculture-category Overseer; the Ergastularius gives the Ergastulum's Bare Regimen tier and Severe/Lethal punishment actions a named operator; the promotion ladder (§6) formalizes that doc's "labor continuity" and manumission-trigger language into an explicit path.
- **Estate & Settlement (§6.2):** every Overseer and Senior Position in §4–5.2 maps directly onto that document's building categories; the Procurator (§5.3) resolves its own open second-settlement question.
- **Buildings (Production Chains) doc:** the Venalicius, Leno/Lena, Argentarius, Lanista, Portitor, Aquarius, and Horrearius give the Slave Market, Brothel, Argentaria, Ludus, Customs House, Aqueduct/Cistern, and Horreum each a named operator.
- **Familia (§6.1):** §2's Core Attribute/Labor Skill split is this whole document's spine; §7's promotion-to-full-record rule is exactly what makes Travel recruitment (§7.3) work without a parallel system.
- **Villa (interior design doc):** §5.1 now fully reproduces and extends that document's §9.2 roster (Balneator, Curator, Ergastularius, Symposiarch are new); the Rooms without a Position (Solarium, Peristylium, Diaeta, and the minor-utility rooms) are unchanged from that doc's own treatment and deliberately stay that way.
- **Politics & Patronage (§6.5):** §5.5 draws the explicit line this system needs to hold once that document gets its own pass.
- **Games & Spectacle (§6.22):** the Lanista and both tiers of Editor are this document's contribution to that system's eventual resolution-formula design.
- **Religion (§6.6):** the Sacerdos Publicus and Libitinarius extend the Household Priest's private-scale role out to the public Temple and Necropolis.
- **Travel (§6.18):** §7 is this document's full treatment of that system's retinue mechanic.
- **Succession & Dynasty (§6.9) / Dynasty Chronicle (§6.11):** the promotion ladder is explicitly framed as Chronicle-worthy; the Libitinarius gives death-handling a physical operator.
- **Economy & Finance (§6.4):** the Argentarius, Institor, Institor Maximus, and the new Rationalis capstone are this document's concrete operators behind that system's trade and debt mechanics.
- **Piracy & Banditry (§6.24):** the Vigil, Praefectus Vigilum, and Navarchus/Navarchus Princeps are the named roles behind that system's estate-side countermeasures.
- **Natural Disasters (§6.17):** the Aquarius (drought) and Horrearius (famine reserve) are new this pass, giving two of that system's hazard types a named operator the way Vigiles already covered fire.
- **Steward/Council Auto-Management (§6.28):** §5.3 and §7.2 both explicitly extend that QoL principle — permanently for a Procurator, temporarily for anyone left behind during Travel.

---

## 10. Data Model

```
Overseer {
  personId,
  role,                    // "vilicus" | "magisterOfficinae" | "metallarius" | "institor" |
                            // "portitor" | "aquarius" | "horrearius" | "sacerdosPublicus" |
                            // "rhetor" | "lanista" | "editor" | "valetudinarius" |
                            // "venalicius" | "lenoLena" | "argentarius" | "vigil" |
                            // "alimentarius" | "libitinarius" | "navarchus"
  assignedPlotOrBuildingId,
  category                 // maps to Estate & Settlement's building categories
}

SeniorPosition {
  personId,
  title,                   // full Villa (§5.1) and Estate/Provincial (§5.2) roster
  scope,                   // "villa" | "estateWide" | "secondSettlement" | "economicCluster"
  tiedRoomOrBuildingId,     // for Villa positions specifically
  oversees: [ overseerIds or positionIds ],   // for aggregate/capstone positions like Rationalis
  coreAttributeRead        // which of the five Familia Core Attributes this position reads
}

TravelRetinue {
  travelEventId,
  members: [ personId, ... ],   // capacity-limited; drawn from existing Familia records
  vacatedPositions: [ overseerOrSeniorPositionId, ... ]  // what's left unstaffed for the duration
}
```

---

## 11. Open Questions

- **Overseer granularity.** Whether every single instance of a building (a third, fourth, fifth Ironworks) needs its own named Overseer, or whether Overseers apply per production *cluster* once an estate has several identical buildings, isn't yet decided.
- **Bonus magnitudes.** Consistent with every other document's deferred-numbers decision, none of the Overseer, Senior Position, or Rationalis-capstone effects in §4–5 are numerically sized yet.
- **Travel Retinue capacity.** §7.1 establishes a capacity limit exists but not its actual number or what it scales with (Villa stage, wealth, a dedicated escort-size stat).
- **Procurator autonomy boundary.** §5.3 resolves *that* a Procurator runs a second settlement, but not the specific decision boundary (which categories of choice they can make alone vs. must hold for the player) — this mirrors the same still-open question §6.28 already carries for the Villa-stage Steward.
- **Promotion thresholds.** §6 establishes the ladder qualitatively; the actual stat/Loyalty thresholds that make someone "eligible" for promotion at each step aren't specified.
- **Disloyal Procurator/Senior Position consequences.** §5.3 gestures at a Procurator "acting in their own interest" as a worst case; the actual mechanic (a rebellion event, a slow embezzlement drain, an eventual Legal & Court case) isn't designed.
- **Historical-terminology confidence levels.** This pass upgraded several titles toward real attested Latin/Greek terms (Vilicus, Metallarius, Institor, Portitor, Aquarius, Horrearius, Lanista, Editor, Venalicius, Leno/Lena, Argentarius, Vigil, Praefectus Vigilum, Navarchus, Actor, Libitinarius, Symposiarch, Rationalis are all real or directly attested). A smaller set remain this document's own functional constructions rather than confirmed single-title terms: **Magister Officinae, Valetudinarius, Sacerdos Publicus, Institor Maximus, Praefectus Metallorum, Navarchus Princeps, Editor Muneris** (a real *phrase*, less certain as a standing title), and **Alimentarius** (flagged directly in §4.2 as the weakest of the set). Worth a dedicated pass if the project wants every title held to the same bar.
- **Interaction with Curia/political office at the building level.** The Curia (Buildings doc §4.10) unlocks holding and contesting local magistracies — whether any Senior Position here (the Actor or Institor Maximus, say) plausibly becomes a stepping-stone toward an actual magistracy once Politics & Patronage is designed, or stays strictly separate per §5.5, is worth revisiting then.
- **Rationalis activation condition.** §5.4 establishes it as a compounding bonus contingent on the roles beneath it being "also filled competently" — the actual threshold for what counts as competently filled isn't specified.
- **Whether the Villa doc itself should be revised.** §5.1's four new positions (Balneator, Curator, Ergastularius, Symposiarch) currently exist only in this document; the Villa doc's own §9.2 table doesn't yet reflect them. Worth reconciling in a future revision pass on that document so the two stay in sync.
