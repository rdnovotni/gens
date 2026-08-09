# GENS — System Design: The Villa
*The player's personal residence — rooms, expansion, and customization, in the spirit of Free Cities' penthouse but built for a much larger scope*

---

## Contents

1. Purpose & Relationship to Estate & Settlement
2. The Villa Growth Track
3. The Dollhouse View
4. Full Room List
5. Room Mechanics
6. Personalization: Cubicula
7. Decoration System
8. Cross-System Integration
9. Villa Positions & Staffing
10. Data Model
11. Open Questions

---

## 1. Purpose & Relationship to Estate & Settlement

Estate & Settlement (§6.2) treats the Villa as a single plot icon — the fixed, unbuildable-over tile every other plot grows up around. This document is what's actually *inside* that icon: a dedicated interior-customization layer the player zooms into, the way Free Cities lets a player customize a penthouse room by room — except scoped to a full Roman domus, with historically real rooms, real mechanical hooks into other systems, and its own independent growth track.

The Villa is personal in a way Estate & Settlement's production buildings aren't: it's where the player's own character and family actually live, where Familia's relationship web plays out day to day, and where several other systems' more intimate moments (a private dinner, a covert meeting, a punishment) physically happen rather than resolving as an abstract event.

### 1.1 Relationship to the Surrounding Settlement

The Villa's growth track (§2) is its own — a family's home and legacy can outpace or lag the settlement around it, and that's intentional. But "separate" doesn't mean "unaffected." As Estate & Settlement's own stage advances (Vicus → Town → City), the Villa feels it both ways: real benefits, and real costs, rather than a one-sided bonus.

**Benefits:**
- **Better goods access.** Decoration materials and luxury goods (§7) that would otherwise require Travel or depend on Imported Goods availability (Buildings doc §4.9) become locally sourceable once the settlement's own Commerce buildings (Market, Emporium, Macellum) are developed enough.
- **Improved security.** A settlement with a Garrison, City Walls, and a Watchtower lowers the Villa's own baseline exposure to Piracy & Banditry and general unrest.
- **Cheaper, faster Villa construction.** Proximity to the settlement's own building-material production (Buildings doc §4.1) modestly reduces the cost/time of Villa stage upgrades and room construction.

**Tradeoffs:**
- **Noise and crowding.** A Town or City growing dense around a Villa erodes the passive Happiness benefit of open-air rooms (Peristylium, Solarium, Xystus/Hortus) unless the player specifically invests in mitigation — walling off the property more completely, or upgrading toward the isolation a countryside estate has by default.
- **Reduced privacy.** The Cryptoporticus and Exedra's Espionage/Romance effectiveness quietly diminishes as a City's population and traffic grow around the property — a covert meeting is a different proposition in a bustling city block than on an isolated rural estate, unless offset by security investment (the Ostium, a City Walls-adjacent buffer).
- **Physical expansion pressure.** At Domus stage inside a full City, further Villa room growth can require buying out or politically maneuvering for neighboring urban plots (a Politics & Patronage or Economy & Finance action) rather than simply having open land to build on the way a rural Vicus-stage estate does.

The net effect is a real strategic tension rather than a strictly upward curve: a thriving City makes the Villa easier to furnish and defend, but harder to keep quiet, private, and physically unconstrained — mirroring the actual historical tension between a countryside villa and a townhouse swallowed by its own city's growth.

---

## 2. The Villa Growth Track

A parallel track to Estate & Settlement's Villa→Vicus→Town→City progression, but tracking personal wealth and status rather than settlement population:

| Stage | Room slots (approx.) | Unlocks |
|---|---|---|
| **Villa Rustica** *(starting state)* | 6–8 | The functional core: Vestibulum, Atrium, Tablinum, one or two Cubicula, Culina, Cellae Familiae, Lararium — plus a Nursery if there's an infant in the household. Posticum, Private Latrina, the Puteal, and the base Ostium are also available from the start but don't count against the slot budget (§4.1) |
| **Villa Urbana** | 14–16 | Peristylium, Triclinium, Balneum, Bibliotheca, Xenodochium, Textrinum, Apotheca, Exedra, Xystus/Hortus, Private Scriptorium, Diaeta, Private Pistrinum, Cella Frumentaria, additional Cubicula for a growing family and companions, and any region-appropriate regional room (§4.7) |
| **Domus/Palace** | 20+ | Oecus, Pinacotheca, Sacrarium, Ergastulum (if not already built), Cryptoporticus, Solarium, a personal Armory Alcove, the Household Wing/Balneum Completum/Aedicula Lararium/Grand Cellar/Grand Vestibule/Master Suite upgrades, Menagerie/Aviary, Servants' Hall, and multiple guest Cubicula |

Advancing a stage costs denarii and building materials (Worked Marble, Concrete, Tile, Furniture — all from the Buildings doc's material chains) plus construction time scaled the same way as Estate & Settlement's larger buildings (§4 of that doc) — this is deliberately not a separate cost model, just that document's construction rules applied to the Villa specifically.

### 2.1 Villa Grandeur Score

A single aggregate number, combining every room's tier and decoration level (§7) into one tracked figure. Grandeur feeds an ongoing passive Dignitas contribution distinct from any individual room's own effect, and crosses specific thresholds to unlock milestones — Chronicle-worthy moments ("The finest house in the province") and comparative flavor against Rival Houses (§6.10), whose own estates can be measured the same way. Grandeur is also a soft gate on Villa stage advancement itself: reaching Villa Urbana or Domus/Palace requires both the denarii/materials cost above *and* a minimum Grandeur floor, so a stage upgrade can't be bought outright without the household actually having lived up to it room by room first.

### 2.2 Second Settlements and Outpost Homes

If the player later acquires a second settlement (Estate & Settlement §7's late-game possibility), it does **not** get its own full Villa. It gets a capped **outpost residence**, permanently limited to Villa Rustica's room slots and tier ceiling regardless of how developed that second settlement becomes — a functional, unglamorous base rather than a second ancestral seat. This keeps the Villa meaningfully singular: there is one true family home, and everywhere else the gens holds property is administered, not lived in the same way.

### 2.3 Hypocaust Heating

A Villa-wide infrastructure upgrade, available at Urbana stage or later **regardless of region** — not limited to the frontier's Gallic Feasting Hall (§4.7), which remains its own culturally-specific room built *around* this same underlying technology. Installing a hypocaust gives every room it serves a modest, ongoing Health/Happiness benefit through the colder months, independent of any single room's own effects — the general-purpose version of what the Winter Hall gives the frontier specifically.

### 2.4 Household, Guest & Family Capacity

A hybrid approach, deliberately: how many people a Villa can actually house is driven by **both** how many housing-type rooms exist **and** an independent Capacity Tier upgrade on each one — rather than picking one mechanism and forcing every scaling need through it alone.

- **Personalized housing (Cubicula)** stays exactly as established in §6 — one room per named individual, scaling 1:1 by building more of them. Bulk capacity scaling never applies here, because the whole point of a Cubiculum is that it's personal, not a housing unit.
- **Bulk housing (Cellae Familiae/Household Wing, for servants; Xenodochium, for guests)** gets a **Capacity Tier** — Modest → Ample → Grand → Vast — upgradeable independently of that room's Quality/Regimen tier (§8). A single Vast Household Wing can comfortably house dozens on its own; a player who wants genuinely large numbers can also simply build a second or third Household Wing, since Domus/Palace stage has the room slots to support it. The two mechanisms compound rather than compete: more rooms *and* bigger rooms both genuinely help.
- **Illustrative scale, not a final balance pass** (per §11's deferred-numbers decision): a single Cellae Familiae at Modest capacity might house a handful of servants; a fully-upgraded Domus with two or three Vast-tier Household Wings could plausibly house several dozen — enough to genuinely satisfy "significant amounts of servants... if you wish" without requiring it of a player who'd rather stay small.

This is also where the Xenodochium's Capacity Tier does its real work: a Modest Xenodochium comfortably hosts one important guest overnight; a Vast one can accommodate a full delegation or wedding party at once — the direct mechanical answer to "guest capacity" alongside the servant-housing answer above.

---

## 3. The Dollhouse View

Presented as a literal architectural cutaway — a labeled cross-section illustration in the game's established Iron-gall-ink line-work on Papyrus, matching the visual identity doc's aesthetic rather than departing from it for this one screen. Rooms are tinted using the same **Building Function Type** tags introduced in the Buildings doc (§2.1 there): Governance-leaning rooms in Tyrian Purple, Production/Service rooms in Terracotta Oxide, religious/prestige rooms in Gold Leaf, security/harsh rooms in Blood Oxide, neutral/domestic rooms in Verdigris Bronze. A player reading the dollhouse view builds the same color literacy the rest of the game already trains.

Clicking a room opens a smaller floating tablet (the same event-modal styling used elsewhere) with that room's current state, available upgrades, decoration options, and — for mechanical rooms — whatever action that room unlocks.

---

## 4. Full Room List

Organized into six functional clusters, matching the Buildings doc's Function Type taxonomy loosely rather than rigidly.

### 4.1 Public & Reception

| Room | Notes |
|---|---|
| **Vestibulum → Grand Vestibule** *(2-tier)* | The entrance hall — first impression, minor Dignitas. The Grand Vestibule upgrade (Domus stage) adds an inscribed or mosaic threshold — the real Pompeian *cave canem* ("beware of the dog") mosaic is the famous example, though a wealthier household might instead inscribe a welcome, a family motto, or an apotropaic symbol against ill fortune |
| **Atrium** | The central hall, historically containing the *impluvium* (rainwater pool) and displaying the family's ancestor masks (*imagines*) — the venue for the daily *salutatio*, the morning ritual of clients calling on their patron. Mechanically the room that actively works Politics & Patronage's clientela relationships, and passively displays Chronicle-notable ancestors for an ongoing small Dignitas trickle |
| **Tablinum** | The *paterfamilias*'s study and office, historically where household records were kept and business conducted — the room from which Estate management, correspondence (Travel's letter-writing), and Legal paperwork actions get a small efficiency bonus, functioning as a real home office rather than flavor |
| **Peristylium** *(Urbana stage)* | The colonnaded garden courtyard — Dignitas plus a modest Health/Happiness benefit for the household, and a venue for smaller, less formal gatherings than the Oecus |
| **Oecus** *(Domus stage)* | A grand reception/banquet hall, larger and more formal than the Triclinium — the venue for major political or social events (a large patronage dinner, a wedding celebration) |
| **Xenodochium (Guest Wing)** *(Urbana stage)* | A dedicated overnight hospitality suite — several guest Cubicula plus a small reception nook, distinct from housing a guest in an ordinary spare bedroom. The concrete venue for hosting an important visitor properly (a rival house's representative, a visiting magistrate), which Politics & Patronage (§6.5) can read as a deliberate courtesy or a calculated snub depending on whether the guest is actually offered it |
| **Posticum** *(minor utility — see note below)* | A secondary, unadorned back entrance for servants, deliveries, and discreet movement — distinct from the Vestibulum's formal, front-facing role. A real historical feature of larger domus, and mechanically the quiet counterpart to the Cryptoporticus (§4.6): less secret, but far more mundane and therefore less noticed, which is its own kind of useful for Labor & Slavery's routine household logistics or an Espionage errand that doesn't need real secrecy, just inattention |

*Minor utility rooms (Posticum here; Private Latrina and Puteal in §4.3; the base Ostium in §4.6) are cheap enough, and universal enough, that they don't consume the room-slot budget in §2 or contribute to the Grandeur Score (§2.1) — matching the Buildings doc's own precedent for Public Latrines as something "meant to be built repeatedly" rather than a real capital investment. They're available from Villa Rustica onward regardless of stage.*

### 4.2 Family & Private

| Room | Notes |
|---|---|
| **Cubiculum** *(one per family member/companion)* | Personalizable per §6 below. The player's own Cubiculum can upgrade to a **Master Suite** tier — larger, more luxurious, and carrying a modest Happiness/Dignitas bonus a standard family member's room doesn't get, reflecting the head of household's own station without denying everyone else a personalized room of their own |
| **Solarium** *(Domus stage)* | A rooftop terrace — minor Happiness, and a natural setting for a private conversation or a Romance & Seduction moment |
| **Balneum → Balneum Completum** *(Urbana stage; 2-tier, Completum at Domus)* | The base Balneum combines the *apodyterium* (changing room), *tepidarium* (warm room), *caldarium* (hot room), and *frigidarium* (cold room) sequence into a single room at Urbana stage — functional, but compressed. The Domus-stage **Balneum Completum** upgrade splits these into their proper four distinct spaces, improving both the Health/Disease benefit and the Dignitas value of hosting guests through an actual bathing sequence rather than a single generic room standing in for one |
| **Exedra** *(Urbana stage)* | A semi-open conversation room opening onto the Peristylium — a favorable setting for Companion or Romance & Seduction relationship-building interactions |

### 4.3 Household Service & Utility

| Room | Notes |
|---|---|
| **Culina** | The kitchen — staffed by a Cook duty slot (Familia §4); its quality feeds directly into how well a Triclinium or Oecus event lands |
| **Puteal (Private Well)** *(minor utility — see §4.1 note)* | Water access independent of the settlement's own Aqueduct — a genuine early-game utility for a Rustica-stage estate that hasn't connected to (or whose region hasn't yet built) civic water infrastructure. A minor Health benefit on its own, and the thing that makes a Balneum or Hypocaust buildable even before the settlement reaches Vicus stage |
| **Private Pistrinum** *(Urbana stage)* | A household-scale mill-and-bakery, distinct from relying on the settlement's own Bakery (Buildings §4.7) — a self-sufficiency upgrade that keeps Bread production going even if the wider settlement's supply is disrupted (Natural Disasters, a bad Estate & Settlement harvest month) |
| **Cella Frumentaria** *(Urbana stage)* | A private household grain store, distinct from the Apotheca's wine/oil focus and from the settlement's civic Horreum (Buildings §4.10) — a modest personal buffer against famine at the household scale specifically |
| **Private Apiary** *(Urbana stage)* | Small-scale beekeeping, continuing the same self-sufficiency pattern as the Pistrinum and Frumentaria — a modest personal source of Honey, feeding the household's own Wine (§4.3 of the Buildings doc) and Writing Tablets (via Wax) without depending on the settlement's production Apiary |
| **Praefurnium** *(Urbana stage)* | The actual furnace room behind the Hypocaust (§2.3) and the heated Balneum — formalizing what would otherwise be an upgrade with no physical room behind it. Staffed by a furnace-tender duty slot, unglamorous but structurally necessary once a Villa commits to real heating rather than relying on braziers |
| **Iatreion (Private Physician's Room)** *(Urbana stage)* | A small consultation-and-treatment room giving the Court Physician (§9.2) an actual workspace, distinct from the settlement's public Valetudinarium (Buildings §4.10) — the personal-scale counterpart to that facility, consuming Medicine (Buildings §4.3) for household members only |
| **Servants' Hall** *(Domus stage)* | A common gathering space for off-duty staff, distinct from the sleeping quarters in Cellae Familiae/Household Wing — a modest but genuinely aggregate Loyalty benefit across the whole staff roster at once, the natural companion to §2.4's capacity expansion |
| **Apotheca → Grand Cellar** *(Urbana stage; 2-tier, Grand Cellar at Domus)* | Wine/oil cellar — dedicated storage capacity for Wine and Olive Oil specifically, distinct from the settlement Granary. The Grand Cellar upgrade properly ages Wine rather than just storing it, meaningfully improving the quality (and therefore the Dignitas/relationship payoff) of whatever gets served at a Triclinium or Oecus event |
| **Textrinum** *(Urbana stage)* | A private weaving and spinning room — historically where the *materfamilias* and household women produced the family's cloth, a genuine domestic virtue in Roman elite self-presentation (funerary epitaphs specifically praised women for it). Mechanically a small, steady source of Cloth (Buildings §4.6) independent of the settlement's Weaver's Loom, and a natural room to tie a materfamilias's own Familia stats to a visible, ongoing household contribution |
| **Cellae Familiae → Household Wing** *(Rustica stage; Household Wing upgrade at Domus)* | Basic household-slave quarters, directly tied to Labor & Slavery's Regimen Accommodation setting (§8) — Cellae Familiae reads as Basic. The Domus-stage **Household Wing** upgrade is the Comfortable-tier housing referenced there: proper individual or small-group quarters for a trusted steward, a favored freedman, or senior household staff, rather than everyone above Ergastulum sharing one undifferentiated tier |
| **Ergastulum** *(Domus stage, optional)* | The historically real, grim overnight lockup for chained field labor. Resolved this pass: Ergastulum **is** the Bare Regimen tier itself, not a step below it — Labor & Slavery's harshness ceiling stays where that document set it, rather than escalating further here. Also the room where more severe Punishment actions are physically administered rather than narrated as happening somewhere unspecified |
| **Private Latrina** *(minor utility — see §4.1 note)* | A household-only convenience, distinct from the settlement's Public Latrines (Buildings §4.10) — a minor Health benefit and a genuine comfort/status marker, since most of the population relied on public facilities |

### 4.4 Religious & Ceremonial

| Room | Notes |
|---|---|
| **Lararium → Aedicula Lararium** *(Rustica stage; Aedicula upgrade at Urbana)* | The household shrine to the Lares and Penates — the personal, family-scale counterpart to the public Shrine/Temple, and the room Religion's (§6.6) omens and household rituals are actually tied to. The Aedicula Lararium upgrade replaces a simple wall niche with a proper small shrine structure — an architectural statement of piety in its own right, carrying a modest additional Dignitas/Religion bonus over the base room |
| **Sacrarium** *(Domus stage, optional)* | A secondary shrine for a personally chosen patron deity beyond the generic household Lares |

### 4.5 Leisure & Culture

| Room | Notes |
|---|---|
| **Triclinium** *(Urbana stage)* | The formal dining room — hosts small-to-medium feasts, consuming Wine, Garum, and fine Bread/Cheese (Buildings doc §4.7) for event quality; the venue for a Politics & Patronage patron-client dinner or a Romance & Seduction courtship |
| **Bibliotheca** *(Urbana stage)* | A private library, consuming Writing Tablets or Parchment as ongoing upkeep the same way the public Library does — boosts the household's own Learning attribute training speed rather than settlement-wide Education |
| **Private Scriptorium** *(Urbana stage)* | A working room, distinct from the Bibliotheca's reading/storage function — where the household's own Scribe or Secretary (a Companions & Court Positions appointment, §6.20) actually produces correspondence, giving Travel's letter-writing system (§6.18) a physical home rather than an abstract writing-desk assumption |
| **Columbarium (Dovecote)** *(Urbana stage)* | Kept doves and pigeons — a real Roman practice, and the natural physical counterpart to the Scriptorium: one room writes the letter, the other can carry it. A faster, if less reliable, alternative within the Correspondence & Letters system (§6's system list) once that system gets its own dedicated design pass, and a minor food source in the meantime |
| **Pinacotheca** *(Domus stage)* | An art gallery — the room that gives Fine Glass, Jewelry, and Statues somewhere to be *displayed* for passive Dignitas rather than only ever sold or gifted |
| **Xystus/Hortus** *(Urbana stage)* | A garden walk — minor Dignitas/Happiness, mostly aesthetic |
| **Diaeta** *(Urbana stage)* | A small garden pavilion, quieter and more private than the Peristylium proper — a good setting for a family member who wants somewhere to retreat rather than the semi-public colonnade |

### 4.6 Security

| Room | Notes |
|---|---|
| **Ostium (Doorkeeper's Post)** *(minor utility — see §4.1 note)* | The entrance guard station — the room that actually unlocks assigning a Bodyguard (Companions & Court Positions, §6.20) specifically to Villa security, rather than that appointment existing abstractly |
| **Armory Alcove** *(Domus stage)* | A personal weapons-and-armor display/storage room for the family's own equipment — distinct from the settlement's production Armory, this is about equipping the player's own character and family for personal combat (Military & Combat) or a duel, not the army |
| **Cryptoporticus** *(Domus stage, optional)* | A covered, partly hidden passage — historically used for cool storage, but mechanically the room that gives Espionage & Information Network (§6.15) a physical venue for a covert meeting or hiding someone from view |

### 4.7 Regional & Thematic Rooms

Available anywhere in principle — consistent with how the Buildings doc treats regional flavor as soft weighting rather than a hard lock (that doc's §3) — but naturally suited to, and cheaper/earlier-accessible in, a specific starting region or terrain.

| Room | Region/Terrain | Stage | Notes |
|---|---|---|---|
| **Viridarium / Summer Triclinium** | Italian heartland | Urbana | An open-air dining variant reflecting Campania's mild climate — the Peristylium's dining counterpart to the enclosed Triclinium, used seasonally rather than replacing it |
| **Gallic Feasting Hall + Winter Hall** | Gallic/frontier | Urbana | A heated great hall (using a proper hypocaust furnace, formalizing what's otherwise just implied by the Balneum's heating) built around local feast customs — served with Beer (Buildings §4.2) rather than Wine, giving the frontier's cultural identity a Villa-level expression to match its production-level one |
| **Private Strongroom/Treasury** | Iberian colony | Urbana | A secure vault for wealth and precious goods, echoing the region's historical mining wealth — mechanically a high-security storage room reducing theft/loss risk distinct from ordinary Apotheca storage |
| **Andron/Symposium Room** | Greek East | Urbana | A Greek-style formal dining-and-discussion room, distinct in custom from the Roman Triclinium — philosophical and political conversation over a meal, leaning into Education & Culture rather than the Triclinium's more purely social/political hosting function |
| **Private Dock/Boathouse** | Coastal | Urbana | Personal-scale water access, distinct from the settlement's Port — a faster, more private departure point for sea Travel, and a direct exposure point to Piracy & Banditry that a Lighthouse or Shipyard doesn't fully cover at the personal scale |
| **Nursery** | *(lifecycle-gated, not region-gated)* | Rustica | A specialized room for Infant-stage (Familia doc §3's lifecycle stages) family members — distinct from an ordinary Cubiculum, and the natural room to tie into that lifecycle stage's elevated Disease/mortality stakes (Familia doc §6, Fertility & Childbirth). Available from the start regardless of Villa stage, since a family can have an infant long before it can afford a Peristylium |
| **Menagerie/Aviary** | — | Domus | Exotic animals as a genuine elite status symbol — a Dignitas-generating luxury room with no production function, the personal-scale counterpart to displaying Imported Goods (Buildings §4.9) rather than trading them |

---

## 5. Room Mechanics

Consistent with wanting a genuine mix rather than one or the other:

**Mechanical rooms** (unlock or measurably boost a specific system): Atrium (Politics & Patronage), Tablinum (administrative efficiency), Triclinium/Oecus (hosting), Xenodochium (hospitality/Politics), Lararium/Aedicula Lararium (Religion), Balneum/Balneum Completum (Disease & Public Health), Bibliotheca (Education & Culture), Private Scriptorium (Court Positions/Travel correspondence), Columbarium (future Correspondence & Letters system), Pinacotheca (Dignitas display sink), Cellae Familiae/Household Wing/Ergastulum (Labor & Slavery Regimen), Ostium (Companions & Court Positions), Cryptoporticus/Posticum (Espionage/discreet logistics), Exedra (Romance/Companions), Armory Alcove (Military & Combat), Apotheca/Grand Cellar (storage capacity and hosting quality), Private Strongroom/Treasury (Economy & Finance security), Andron (Education & Culture, distinct hosting mode), Private Dock/Boathouse (Travel, Piracy & Banditry exposure), Nursery (Familia lifecycle/Disease stakes), Private Latrina/Puteal (minor Health), Private Pistrinum/Cella Frumentaria/Private Apiary (self-sufficiency, Disaster resilience), Textrinum (small Cloth production, materfamilias stat tie-in), Praefurnium (the physical room behind Hypocaust/heated-Balneum functionality), Iatreion (Disease & Public Health, personal scale), Servants' Hall (aggregate staff Loyalty).

**Cosmetic/Dignitas rooms** (comfort and prestige, no unique system unlock): Vestibulum/Grand Vestibule, Peristylium, Solarium, Diaeta, individual Cubicula (personalization value, plus the player's own Master Suite tier), Culina (mostly functional/flavor), Xystus/Hortus, Sacrarium, Viridarium/Summer Triclinium (seasonal variant of an existing mechanical room), Gallic Feasting Hall (cultural variant), Menagerie/Aviary (pure Dignitas).

---

## 6. Personalization: Cubicula

Each family member, freedman, client, or companion with a Cubiculum can have it personalized — and per your call, this draws directly on that person's existing Familia data rather than being a disconnected decoration minigame:

- **Trait-driven defaults:** a *Studious* person's room defaults toward books and scrolls; a *Devout* person's includes a small personal devotional nook; an *Ambitious* person's leans toward symbols of office or aspiration; a *Resentful* enslaved person's assigned space (within whatever Regimen tier applies) might read as deliberately bare regardless of what's available, reflecting the relationship rather than the budget.
- **Player override:** the player can freely override these defaults for anyone, most obviously their own character and close family.
- **Appearance tie-in:** portraits (§7.11 of the visual design doc) and room dressing share the same underlying aesthetic logic, so a room doesn't visually contradict the person living in it.

This is the payoff for having built such a deep trait and appearance system in Familia — the household actually *looks* like the specific people in it, not like a generic furnished house.

---

## 7. Decoration System

**Presets, primarily.** Each room offers three to four preset **Style packages** (for the Atrium, for instance: Modest, Traditional, Opulent, Hellenistic-influenced), each with a cost and a corresponding Dignitas/effect tier — this covers the large majority of play, matching your steer toward simplicity as the default.

**Granular mode, for players who want it.** Beyond the presets, a player can individually select wall treatment (plain vs. fresco, and fresco subject matter — genuinely a place Chronicle-worthy family history could be depicted), floor treatment (plain tile vs. a chosen mosaic pattern), furniture quality tier, and specific statuary/art placement — consuming the actual named goods from the Buildings doc (Worked Marble, Fine Glass, Furniture, Statues) as real decoration inputs rather than an abstract "decor points" currency. This is where a player who wants Free-Cities-penthouse-level granularity gets it, without forcing that depth on everyone.

**Specific decoration additions this pass:**
- **Ornamental Piscina** — a fish pond, available as an Atrium or Peristylium decoration choice rather than a separate room; a genuine (and genuinely slightly absurd) elite Roman status symbol, contributing Dignitas with no other mechanical function.
- **Impluvium style** — the Atrium's central rainwater pool gets its own decoration choice, separate from the room's overall preset: plain pool, an ornamental fountain, or the Piscina variant above, so the single most visually central feature of the house isn't locked to whatever the Atrium's broader style happens to default to.
- **Family Portrait wall** — a wall-decoration option distinct from the Pinacotheca's purchased/collected art: portraits of the family itself, drawing on the same Appearance system (§7.11 of the visual design doc) that generates individual character portraits, giving a household's own history a decorative presence separate from art bought for prestige.

### 7.1 Decoration Style Guide

Five real, historically distinct style families, replacing the placeholder "Modest/Traditional/Opulent/Hellenistic" list with actual art-historical substance. All five are available regardless of starting region — consistent with every other regional mechanic in this design, it's soft weighting (cheaper, earlier-accessible, and the sensible default in its home region) rather than a hard lock; importing a style is a real, slightly more expensive choice a player can make deliberately.

**The Four Pompeian Styles** *(Italian heartland's signature progression — genuine Roman wall-painting art history, and unusually good material for a tiered decoration system since they're a real, sequential 1st-century-BC-through-1st-century-AD progression)*:
- **First Style (Structural):** Plastered and painted to imitate expensive marble veneer panels — the cheapest-reading, most restrained option, appropriate for a Villa Rustica just starting out.
- **Second Style (Architectural):** Illusionistic painted architecture — false columns, painted vistas opening onto imagined landscapes beyond the actual wall. Ambitious and visually striking for a mid-tier Urbana room.
- **Third Style (Ornamental):** Flat, elegant monochrome panels with delicate linear ornament and small mythological vignettes — refined rather than showy, a favorite for a room meant to read as tasteful rather than loud.
- **Fourth Style (Intricate):** The most elaborate — combines architectural illusion, ornamental panels, and full mythological scenes into one dense, theatrical composition. The natural top-tier choice for a Domus-stage Oecus or Atrium.

**Provincial/Gallic Style** *(frontier)*: Leans on wood paneling and woven wool hangings rather than fresco and marble, reflecting both material availability and a colder climate less forgiving of open courtyards. Where decoration does appear, it draws on La Tène-derived Celtic motifs — spirals, knotwork, and stylized animal forms — blended with provincial Roman framing rather than replacing it outright. Reads as sturdy and distinctive rather than opulent, and pairs naturally with the Gallic Feasting Hall (§4.7) and Beer-based hosting.

**Punic-Iberian Mosaic Style** *(Iberian colony)*: Where the Italian heartland leans on fresco, this region's signature is the floor — elaborate, colorful figural mosaics (hunting scenes, marine life, mythological narrative) in the real historical tradition Roman Spain and North Africa became famous for. A room decorated in this style spends its budget on the floor rather than the walls, which is itself a distinctive visual signature at the dollhouse-view level.

**Hellenistic Style** *(Greek East)*: White and polychrome marble (real Greek varieties like Pentelic or Parian are the aspirational high end), idealized figural sculpture, and philosophical or mythological fresco subjects favoring restrained, classically-proportioned composition over the Fourth Pompeian Style's theatrical density. Pairs naturally with the Andron/Symposium Room and Gymnasium-adjacent Education & Culture flavor.

**Provincial Fusion** *(available anywhere, no regional default)*: A deliberate blend rather than a single tradition — the style choice for a family that's genuinely cosmopolitan, has traveled extensively (Travel, §6.18), or simply doesn't want to commit to one regional identity. Mechanically the most expensive of the five, since it's assembled from imported elements rather than drawing on whatever a single region does cheaply and well.

---

## 8. Cross-System Integration

- **Labor & Slavery (§6.3):** Cellae Familiae, its Household Wing upgrade, and Ergastulum aren't just flavor — building or assigning someone to one of these rooms **directly sets their Regimen Accommodation setting** (that doc's §5): Cellae Familiae reads as Basic, the Household Wing as Comfortable (for a trusted steward or favored freedman), and Ergastulum as Bare — the ceiling and floor of that document's own Accommodation scale, not something this doc escalates past. Housing and Regimen are now the same decision, not two separate menus that happen to agree.
- **Politics & Patronage (§6.5):** the Atrium's *salutatio* mechanic is the household-side counterpart to whatever that system's own patron-client mechanics specify — clients visit a place, not an abstract relationship number.
- **Religion (§6.6):** the Lararium is where that system's household-scale rituals and omens are physically anchored.
- **Disease & Public Health (§6.13):** the Balneum is a private, household-only version of the public Bathhouse's benefit.
- **Education & Culture (§6.14):** the Bibliotheca is the private, household-scale version of the public Library.
- **Espionage (§6.15):** the Cryptoporticus is that system's physical venue within the player's own home.
- **Companions & Court Positions (§6.20):** the Ostium is specifically what makes a Bodyguard assignment meaningful at the Villa level, rather than that appointment only mattering during Travel.
- **Romance & Seduction (§6.19):** the Exedra and Solarium are the Villa's natural settings for that system's relationship-track interactions.
- **Military & Combat (§6.7):** the Armory Alcove equips personal combat, distinct from the settlement Armory equipping the army.
- **Dignitas & the Chronicle (§6.11):** the Pinacotheca and the Atrium's ancestor display both convert existing goods/history into passive, ongoing Dignitas rather than one-time effects; the Menagerie/Aviary does the same for exotic Imported Goods.
- **Regional identity:** the Buildings doc gives Wine-vs-Beer and similar chains a regional production identity; the Villa's regional rooms (§4.7) give that same identity a personal, lived-in expression — a frontier Villa's Gallic Feasting Hall pairs naturally with that estate's own Brewery output.
- **Familia lifecycle (§6.1 §3):** the Nursery gives the Infant lifecycle stage a physical room tied to its elevated stakes, rather than infants sharing an undifferentiated Cubiculum with everyone else.
- **Estate & Settlement (§6.2) and Buildings (Piracy & Banditry, Travel):** the Private Dock/Boathouse and Private Strongroom both give personal-scale expressions to systems that otherwise operate at the settlement scale (a Port, a bank).
- **Correspondence & Letters (future system):** the Private Scriptorium and Columbarium together give that not-yet-designed system a physical home at the Villa level before its own design pass exists — writing and sending are already two distinct rooms rather than one abstract "send a letter" action.
- **Natural Disasters/Estate & Settlement self-sufficiency:** the Puteal, Cella Frumentaria, Private Pistrinum, and Private Apiary together mean a Villa can weather a settlement-level supply disruption for a while on its own reserves, independent of how well the Estate's own production is doing that month.
- **Companions & Court Positions (§6.20) and Familia (§6.1 §2.1/§2.2):** §9's staffing system is where those two documents' abstract Attribute/Labor-Skill distinction becomes concrete — every Senior Position reads Core Attributes, every Household Staff Position reads Labor Skills, and the room list is what turns "an appointment exists" into "an appointment exists *somewhere specific*."

---

## 9. Villa Positions & Staffing

CK3's council is five or six powerful appointments. This is that idea taken much further — because a Roman household genuinely ran on dozens of named roles, and because nearly every room added across this document's many passes has an obvious person who'd actually operate it.

### 9.1 Two Tiers

**Senior/Court Positions** — the CK3-equivalent tier: a handful of powerful appointments (Companions & Court Positions, §6.20) filled by a family member, companion, freedman, or trusted slave, evaluated on the five Core Attributes (Familia §2.1) rather than Labor Skills. These **scale with Villa stage** rather than being capped at a fixed small number — Rustica offers a few essentials, Urbana adds meaningfully more, and Domus/Palace opens up the full, genuinely large roster real elite households actually employed. Not every position needs to be filled; an unfilled senior position simply means that room or function runs at its unstaffed baseline rather than getting a bonus.

**Household Staff Positions** — ordinary duty slots (Familia §4) evaluated on Labor Skills rather than Core Attributes, the same underlying system the Estate's Field Hand or Domestic Servant slots use. The difference here is naming: **a room only gets its own unique staff title when the role is genuinely distinct**, not for every minor utility space — a Cellarer and a Beekeeper are different enough jobs to deserve different names; nobody needs a unique title for tending the Puteal.

### 9.2 Senior/Court Positions by Villa Stage

| Stage | Positions unlocked | Tied room | What filling it does |
|---|---|---|---|
| **Rustica** | Steward (*Dispensator*) | Tablinum | Administrative efficiency bonus (§4.1) scales with the Steward's Stewardship rather than being flat |
| | Bodyguard | Ostium | Personal security, as established in §8 |
| | Household Priest (*Sacerdos Domesticus*) | Lararium | Religion's household rituals/omens read the Priest's Learning/Piety-adjacent standing |
| **Urbana** | Secretary (*Amanuensis*) | Private Scriptorium | Correspondence efficiency/reach |
| | Cellarer (*Promus*) | Apotheca/Grand Cellar | Wine-aging quality feeding Triclinium/Oecus events |
| | Head Cook (*Archimagirus*) | Culina | Hosting-event quality |
| | Tutor (*Paedagogus*) | Bibliotheca | Children's Education & Culture progress |
| | Nurse (*Nutrix*) | Nursery | Infant Health/mortality-risk mitigation |
| | Weaving-Mistress (*Magistra Textrinii*) | Textrinum | Cloth output and quality — often the *materfamilias* herself |
| | Master of Hospitality (*Xenodochus*) | Xenodochium | Guest-capacity management (§2.4) and how a hosted guest is treated |
| **Domus/Palace** | Chamberlain (*Cubicularius Maior*) | Master Suite/family wing | Overall family-affairs administration, a step above the Steward's estate focus |
| | Household Spymaster | Cryptoporticus | Espionage & Information Network |
| | Guard-Captain/Marshal | Armory Alcove + Ostium | Personal-combat readiness and Villa-wide security, above a single Bodyguard's scope |
| | Court Physician (*Medicus*) | Iatreion *(new room, below)* | Disease & Public Health at the personal scale |
| | Treasurer (*Arcarius*) | Private Strongroom/Treasury | Theft/loss risk reduction, Economy & Finance |
| | Master Beekeeper (*Apiarius*) | Private Apiary | Honey output |
| | Furnace-Master (*Fornacator*) | Praefurnium | Heating reliability/efficiency |
| | Menagerie-Keeper | Menagerie/Aviary | Upkeep of exotic animals, prevents a Dignitas-damaging mishap |
| | Harbor-Steward *(coastal only)* | Private Dock/Boathouse | Sea-Travel readiness, Piracy exposure management |
| | Dovecote-Keeper (*Columbarius*) | Columbarium | Correspondence & Letters reliability once that system exists |
| | Baker-in-Chief (*Pistor*) | Private Pistrinum | Bread self-sufficiency output |
| | Granary-Keeper | Cella Frumentaria | Famine-buffer size/reliability |

That's a genuinely large roster by Domus/Palace stage — closer to two dozen possible appointments than CK3's half-dozen — but the player is never obligated to fill all of them; an empty Cellarer position just means the Apotheca ages wine at its unstaffed default rather than the game demanding a full staff roster before anything works.

### 9.3 New Room: Iatreion (Private Physician's Room)

*(Urbana stage)* A small consultation-and-treatment room, giving the Court Physician position an actual workspace distinct from the settlement's public Valetudinarium — the personal-scale counterpart to that Buildings-doc facility, the same relationship the Balneum has to the public Bathhouse. Consumes Medicine (Buildings §4.3's Apothecary chain) the same way the Valetudinarium does, but only for household members rather than the wider population.

### 9.4 New Room: Servants' Hall

*(Domus stage)* A common gathering space for off-duty staff, distinct from Cellae Familiae/Household Wing's sleeping quarters — the direct answer to a household large enough to need somewhere its now-substantial staff can actually gather rather than just sleep. A modest but real Loyalty benefit across the whole household staff roster at once, rather than per-individual — the first room in this document whose effect is explicitly aggregate rather than tied to one person or one system.

---

## 10. Data Model

```
Villa {
  stage,   // Rustica / Urbana / Domus  (outpost homes, §2.2, are permanently capped at Rustica)
  grandeurScore,   // §2.1 — aggregate of all room tiers/decoration, feeds Dignitas and stage-advancement gating
  isOutpost: bool,
  rooms: [
    {
      key, cluster, functionType,       // Governance/Service/Prestige/Housing/Defense, per Buildings §2.1
      tier,
      capacityTier,      // §2.4 — Modest/Ample/Grand/Vast; only applies to bulk-housing rooms (Cellae Familiae/Household Wing, Xenodochium)
      decoration: { preset } | { walls, floor, furniture, displayedItems: [...] },
      assignedTo: personId | null,      // for Cubicula, and for Regimen-linked quarters
      staffPosition: { title, holderId } | null,   // §9 — for rooms with an associated Senior or Household Staff position
      mechanicalEffect: { system, magnitude }  // for mechanical rooms only
    }
  ]
}
```

---

## 11. Open Questions

- **Exact room-slot counts per stage.** §2's numbers are approximate; real balancing depends on how many named individuals a typical household actually has at each Estate & Settlement stage.
- **Style package costs/effects.** §7's presets are scoped conceptually but not costed or valued numerically — this now applies to the five real style families in §7.1 as well.
- **Fresco subject matter as a Chronicle feature.** §7 floats depicting real family history in frescoes; whether this is purely descriptive flavor or actually pulls specific Chronicle entries automatically isn't specified.
- **Whether Villa Grandeur feeds settlement-level Dignitas caps.** Resolved in the opposite direction this pass — §1.1 establishes the settlement benefits the Villa, one-directionally, rather than the reverse; whether a sufficiently grand Villa should ALSO ease the settlement's own Town/City thresholds remains a separate, still-open question if a tighter two-way coupling is ever wanted.
- **Passive-benefit magnitudes.** §1.1 establishes that goods access, security, and construction cost/time all improve with settlement stage, but not by how much at each stage.
- **Strongroom security mechanics.** §4.7 establishes the Private Strongroom reduces theft/loss risk; the actual risk model it's reducing isn't yet specified (ties to a future Espionage or Legal & Court threat-model pass).
- **Nursery's mechanical distinction from a standard Cubiculum.** Established narratively; the actual Health/Disease modifier difference isn't yet numerically specified.
- **Tradeoff magnitudes.** §1.1 establishes real Happiness/privacy costs from settlement growth alongside the benefits, but not their actual size relative to the benefits — the intended tension only works if neither side is trivial.
- **Mitigation mechanics for the tradeoffs.** §1.1 mentions walling off the property or security investment as offsets; the specific action/building behind that mitigation isn't yet named.
- **Grandeur Score formula.** §2.1 establishes it as an aggregate of room tier and decoration level; the actual weighting between those two inputs isn't yet specified.
- **Grandeur milestone list.** Referenced as unlocking Chronicle moments and Rival House comparisons, but the actual milestone thresholds/names haven't been drafted, mirroring the still-open general Milestone Catalog question from the core design doc.
- **Hypocaust vs. Winter Hall overlap.** §2.3 positions the Hypocaust as the general-purpose version of the frontier Winter Hall's heating; whether a Villa can sensibly have both, or whether building one should preclude the other, isn't yet decided.
- **Xenodochium capacity.** Established as "several guest Cubicula plus a reception nook"; the actual room-slot cost isn't specified.
- **Master Suite's exact bonus size relative to a standard Cubiculum.** Not yet numerically specified.
- **Private Pistrinum/Cella Frumentaria self-sufficiency magnitude.** Both established as buffers against settlement-level disruption, but how much grain/bread they can actually sustain a household on isn't sized.
- **Textrinum output rate.** Established as "a small, steady source of Cloth"; not yet quantified relative to the settlement's own Weaver's Loom output.
- **Provincial Fusion's exact cost premium.** Established as "the most expensive of the five" styles; not yet numerically specified relative to a region's own default style.
- **Puteal's interaction with a later Aqueduct connection.** Once the settlement does reach Vicus stage and offers Aqueduct access, whether the Puteal becomes redundant, stays as a backup, or offers some small ongoing benefit isn't specified.
- **Columbarium reliability.** Framed as "faster, if less reliable" than standard correspondence; the actual success/speed tradeoff depends on the Correspondence & Letters system's own eventual design.
- **Private Apiary and Praefurnium output/effect sizes.** Both follow the established self-sufficiency room pattern but, like the others in that pattern, aren't numerically sized yet.
- **Capacity Tier numbers.** §2.4 gives illustrative scale ("a handful" to "several dozen") deliberately rather than final numbers, consistent with deferring exact balancing — real figures depend on how large a household Familia and Settlement Demographics end up supporting.
- **Senior Position effect magnitudes.** §9.2's table describes *what* each position affects but not the size of a filled-vs-unfilled difference for any of them.
- **Whether unfilled Senior Positions carry any downside.** §9.2 states an empty position just means "the unstaffed default" rather than a bonus — whether that default is neutral or mildly negative (an untended Apotheca slowly loses quality, say) isn't decided.
- **Servants' Hall's aggregate Loyalty formula.** Established as a whole-roster effect rather than per-individual, but not sized or specified as flat vs. percentage-based.
