# GENS
### A Roman Villa & Dynasty Management Sim
**Final Core Design Document (Revised)**

---

## Contents

1. Concept & Premise
2. Design Pillars
3. Structural Skeleton
4. Setting & Start Selection
5. Character Creation / Game Start
6. Systems Overview (27 systems)
7. Visual Identity
8. Time Scale & Numbers Philosophy
9. Content & Tone Guidelines
10. Glossary
11. Full System Checklist
12. Open Design Questions
13. Next Steps

---

## 1. Concept & Premise

**Gens** *(Latin: "clan," "family line" — the working title)* is a text-and-visual, menu-driven management sim built on the structural bones of *Free Cities*: a hub-and-submenu interface, deep per-character stat sheets, recurring randomized events gated by those stats, and slow, accretive growth from a modest starting holding into something much larger.

The player builds a Roman household (*familia*) and estate (*villa*) during a settlement-friendly window of the Roman world — the Pax Romana by default, with the setting flexible to adjacent eras — and grows it, over one lifetime or across many generations, into a thriving estate, a full settlement, or a genuine political dynasty. There is no fixed ending. A playthrough succeeds or fails entirely on the player's own terms.

The game treats its subject matter — slavery, political violence, the machinery of Roman power — with real mechanical stakes and unsanitized narrative tone, the same unflinching spirit *Free Cities* brings to its own subject. It is not, however, a game *about* sex the way *Free Cities* is. Romance and seduction are fully systemized as political and relational mechanics; their sexual dimension is never depicted directly, handled instead the way a serious historical drama would — implied, faded to black, described rather than shown.

---

## 2. Design Pillars

1. **Deep individuals.** Every family member, slave, freedman, client, companion, and notable NPC is a full stat-and-trait character — never a resource line.
2. **Open-ended scope.** No fixed win condition, no generational cap. A playthrough might end in one lifetime or sprawl across ten generations.
3. **Total economic and architectural freedom.** Total War / Crusader Kings-style building chains support agrarian, mercantile, industrial, and martial identities equally.
4. **Frank harshness, thematic honesty.** Slavery and political brutality carry genuine narrative and mechanical weight, entirely without sexual content as the mechanism of that harshness.
5. **Player as protagonist, not just administrator.** The player directly controls one character while governing everyone else by policy — compliance depends on loyalty, personality, and standing, not player fiat.
6. **A living world.** Rival houses pursue the same land, offices, marriages, and prestige on their own initiative, whether or not the player engages them.
7. **Memory has weight.** The dynasty's own history is a first-class, readable feature — a game about lineage should let you read your lineage.
8. **Governance through policy, not just clicks.** Laws, edicts, funded public events, and standing household policy are lasting, revisable choices with real tradeoffs, not one-off prompts.
9. **The world beyond the villa gate is reachable.** Travel, court appointments, and personal relationships extend play into a wider social and geographic world, not just the estate ledger.

---

## 3. Structural Skeleton

*Gens* borrows its interface skeleton directly from *Free Cities*, remapped to Roman household life:

| Free Cities element | Gens equivalent |
|---|---|
| Main hub / arcology overview | **Villa Overview** — season, treasury, granary, dignitas, household census at a glance |
| Slave list / individual slave sheet | **Familia Roster** — every family member, slave, freedman, client, and companion, each with a full stat block |
| Facility construction menu | **Estate & Settlement menu** — building chains, upgrades, land acquisition |
| Weekly report / random events | **Monthly Report** — the core time-tick, triggering stat-weighted random and scripted events |
| Reputation stat | **Dignitas** (public reputation), individual relationship/opinion tracking, and a frontier reputation duality where relevant |
| Rules/policy sliders | **Policies & Edicts** — standing, revisable household and estate law |
| Body customization | **Appearance system** — detailed generated portraits, with full player-character customization and lighter household customization |
| Endgame / arcology "victory" states | None fixed — pure sandbox, with optional self-set objectives and a milestone catalog for structure |

---

## 4. Setting & Start Selection

Before play begins, the player selects a **starting region**, akin to choosing an arcology's location in *Free Cities*. Each defines starting land quality, nearby markets, local threats, and locally-available politics:

- **Italian heartland** (Latium/Campania) — high prestige ceiling, high land cost, dense political competition, easy access to Rome.
- **Gallic/frontier province** — cheap land, room for raw settlement expansion, higher military/security risk, lower starting prestige, more social mobility. The primary setting for the frontier reputation duality (§6.21).
- **Iberian/North African colony** — strong trade and resource extraction (mining, olives, garum, grain), moderate risk, colonial-administration flavor.
- **Greek East** — culture/education bonuses, strong trade networks, distinct legal/social wrinkles (Greek citizenship status, Hellenistic custom).
- *An extensible slot exists for additional regions post-launch — Egypt, Syria, and Britannia are natural candidates.*

**Pax Romana** is the default/recommended era for the stability it offers settlement-building, but the region/era pairing stays flexible enough to shift earlier (late-Republic frontier colonization) or later (early Dominate) without breaking core systems.

---

## 5. Character Creation / Game Start

1. **Full Custom** — hand-build the starting pater/materfamilias: name, stats, starting traits, background, starting household composition, and full appearance customization (§7.11).
2. **Templated Backgrounds** — pre-built archetypes (impoverished patrician clawing back status, jumped-up equestrian merchant, veteran given a land grant, provincial notable) with light customization on top.
3. **Randomized Start** — fully rolled character and starting situation, *Free Cities*-style.
4. **Scenario Starts** — fixed, flavorful starting situations with built-in hooks ("you've just inherited a debt-ridden estate," "a newly settled veteran colony," "a disgraced family seeking to rebuild").
5. **Easy/"Cheat" Start** — generous resources and low early risk, for players who'd rather skip the hard-scrabble opening.

---

## 6. Systems Overview

Twenty-seven systems in total (plus one extension and one visual-identity element, noted where they occur). Each will receive its own dedicated design-doc pass; this section scopes all of them and shows how they connect.

### Household & People
- **6.1 Familia** — full stat blocks (health, physical stats, intellect/education, skills, personality traits, loyalty/opinion toward the player, ambition, fertility/age, status) applied uniformly across family, slaves, freedmen, clients, and companions, differing only by status flags and available actions. Includes the marriage market (dowry, alliance value, family prestige) and the player-character's own trainable stats.
- **6.3 Labor, Slavery & Punishment** — acquisition (markets, war captives, debt bondage), labor assignment, punishment/discipline, overwork/health/revolt-risk tradeoffs, and manumission as a strategic and social tool. Same character depth as family members; no sexual content anywhere in this system.
- **6.19 Romance & Seduction** — operates on two tracks at once: a political scheme tool (seduction pursued for leverage, blackmail, alliance, or information, feeding Politics and Espionage) and a genuine relationship system (affection/attraction stats tracked alongside, not replacing, the transactional marriage market). Sexual content stays indirect per §9.
- **6.20 Companions & Court Positions** — a small persistent recruitable retinue (own stat blocks, loyalty, personal arcs) drawn from the household and from Travel, alongside a formal roster of appointable positions — steward, marshal, spymaster, court physician, bodyguard — each mechanically feeding its associated system.

### Land & Economy
- **6.2 Estate & Settlement** — Total War/Crusader Kings-style building chains across agriculture, industry, commerce, civic, and military categories; land acquisition and physical expansion from a single villa toward a *vicus* and eventually a town or city; multiple viable economic identities rather than one intended build order.
- **6.4 Economy & Finance** — treasury, income (yield, trade, rents, contracts including military supply and mining/quarrying revenue), expenses (upkeep, wages, bribes, taxes), debt mechanics, and trade routes/market fluctuation for commerce-leaning play.
- **6.17 Natural Disasters & Environment** — fires, floods, earthquakes, and drought as periodic crisis events, distinct from but able to compound with Disease, that test infrastructure choices made in Estate & Settlement and can ripple into Politics (disaster relief as patronage) and Religion (a disaster as an omen).
- **6.13 Disease & Public Health** — plagues and endemic illness moving through the household/settlement independent of labor conditions, though treatment and crowding affect vulnerability. Interacts with medicine/doctors, sanitation buildings, and Religion.
- **6.23 Monuments & Legacy Building** — a prestige-only construction category, distinct from Estate & Settlement's utility buildings: family tombs, dedicatory statues, temples endowed in the gens's name. These don't produce yield; they produce Dignitas and Chronicle entries, and can become landmarks other rival houses visibly react to.
- **6.26 Settlement Demographics** — once a villa grows into a *vicus* or town (Estate & Settlement's expansion track), population becomes its own thing to manage: colonist influx, freedmen setting up independent shops, general growth pressure on housing and food — distinct from the named individuals tracked in the Familia Roster.

### Power & Standing
- **6.5 Politics & Patronage** — local/regional politics as the primary day-to-day pillar (patron-client relationships, local magistracies, provincial administration, regional rivalries and alliances), with cursus honorum/Senate seats as a distant, rare-but-reachable goal. Dignitas runs alongside individual relationship/opinion tracking.
- **6.21 Reputation Duality (Frontier Play)** — an extension of Politics rather than a standalone system: in frontier starts, Dignitas (standing with Rome) and local standing with the surrounding populace are tracked separately and can pull in different directions.
- **6.25 Diplomacy with Non-Roman Peoples** — also frontier-specific: the neighboring tribes aren't just an unrest modifier but actors in their own right, reachable for treaties, tribute arrangements, and alliances — including, potentially, an alliance against Rome itself. Builds directly on the Reputation Duality axis above.
- **6.16 Legal & Court System** — formal disputes and lawsuits over land, contracts, debts, or slave ownership; *patria potestas* mechanics played with full historical weight (marriage approval, disownment, life-and-death authority in the harshest cases); magistrate rulings shaped by reputation, bribery, and patronage.
- **6.10 Rival Houses / Living World** — other gentes with their own holdings, ambitions, and family trees, advancing on their own simulated timeline, competing for the same marriages, offices, patronage, and land. Their fortunes stay legible without requiring player micromanagement.
- **6.15 Espionage & Information Network** — spies/informants within the player's own household, a rival's, or the local administration, generating blackmail material, early warnings, and covert-sabotage options that feed Politics and Rival Houses directly.

### Culture & Belief
- **6.6 Religion** — household gods (Lares, Penates), omens, festivals, and priesthoods as mostly a flavor layer, with select moments where religious standing meaningfully swings an outcome.
- **6.14 Education & Culture** — rhetoric schools, Greek tutors, philosophy, and literacy as an investment system raising stats and unlocking career/political/marriage options; cultural prestige as a soft-power complement to raw dignitas.
- **6.12 Policies & Edicts** — standing, revisable household/estate policies (slave treatment, tenant taxation, military recruitment) plus one-off funded actions (games, festivals, public works) for prestige, patronage, or religious favor. Structured as persistent, legible-tradeoff settings, not one-time prompts.

### Conflict
- **6.7 Military & Combat** — a playable military career path for sons and other eligible household members; estate-level security akin to the *Free Cities* pregmod security/combat mod — recruit from slaves or free citizens, train, pay upkeep, deploy for defense, suppression, or offense. Stat-driven squad-level resolution: unit composition, terrain/situation modifiers, commander stats — without becoming a full tactical wargame.
- **6.22 Games & Spectacle** — sponsoring gladiatorial games, chariot races, and theatrical performances as a far bigger patronage lever than a generic funded event under Policies & Edicts. Includes acquiring and training gladiators as a distinct labor subtype (drawing on Labor & Slavery and sharing some resolution logic with Military & Combat), audience wagering, and games as a direct, visible Dignitas and Politics & Patronage investment.
- **6.24 Piracy & Banditry** — a human threat layer distinct from Natural Disasters' impersonal hazards: raids on trade goods, caravans, and travelers that scale with the player's security investment and can be interceptable, bribed off, or retaliated against, rather than simply weathered.

### Time, Memory & Reach
- **6.8 Events** — monthly-tick-triggered random and scripted events weighted by current stats/traits/relationships. The wider Roman Empire (wars, emperors, edicts, governors) intrudes regularly and meaningfully, occasionally rippling from the player's own choices outward.
- **6.9 Succession & Dynasty** — no fixed generational endpoint; succession is primarily player-chosen, layered with optional Crusader-Kings-style succession drama (rival claimants, contested inheritance) when circumstances make it compelling. **Adoption** is an explicit tool within this system, not just a fallback: a childless or dissatisfied pater/materfamilias can formally adopt an heir — a promising young client, a rival house's spare son, a distinguished freedman's child — to import outside political talent or cement an alliance, the same lever real Roman dynasties (Augustus's own succession, chief among them) relied on.
- **6.11 Dynasty Chronicle** — an in-fiction, readable record of the gens's history: births, deaths, marriages, scandals, offices held, wars fought, buildings raised, generational transitions.
- **6.18 Travel** — the player (and optionally other family members) can journey to Rome, provincial capitals, a rival's estate, the frontier, or a campaign. Primarily abstracted (pick a destination, commit travel time, arrive to an encounter) with a lightweight real-time flourish — a visible route indicator on the regional map, occasional ambient minor events en route — rather than full continuous movement. The primary gateway into Companion recruitment and many Espionage/Politics/Romance opportunities.
- **6.27 Correspondence & Letters** — the remote counterpart to Travel: petitioning a patron, negotiating a marriage, running an espionage operation, or managing a rival relationship without physically going anywhere. Lower-risk and lower-reward than the equivalent handled in person via Travel, and the natural way to keep distant relationships (a married-off daughter, a son on campaign, a rival across the province) alive between visits.

*Heirlooms and family relics, a formal slave-revolt escalation ladder, and ancestor veneration/funerary rites as their own system were all considered during design and set aside; not currently part of the scope. Modding/extensibility support was also raised twice and declined both times.*

---

## 7. Visual Identity

### 7.1 Design Philosophy

The interface should feel like paging through an actual Roman household's own records — wax tablets, painted ledgers, inscribed stone, an illuminated scroll — rather than a conventional strategy-game UI skin. Every screen reads as a document the household itself would have produced, which is also the thematic justification for the Chronicle: the game's UI and its own diegetic record-keeping are the same object.

Deliberately avoided: the near-black-background-plus-neon-accent look and the hairline-rule broadsheet look common to generic AI-generated interfaces, both of which read as anachronistic here — and the generic "warm cream + terracotta" combination that has become its own AI-design cliché. The aged-document direction is earned by the setting and executed with a specific, non-generic palette rather than defaulted into.

### 7.2 Color

| Token | Hex | Role |
|---|---|---|
| Papyrus | `#E9DFC4` | Primary background — an aged document tone, not a bright cream |
| Iron-gall Ink | `#2A231B` | Primary text, top bars, structural lines |
| Tyrian Purple | `#5C3350` | People/authority — family, loyalty, high office, the player's own identity color |
| Terracotta Oxide | `#9C4B2E` | Land/labor/economy — a duskier, rustier red than the typical AI-default clay |
| Verdigris Bronze | `#6E8272` | UI chrome, dividers, neutral iconography, secondary text |
| Gold Leaf | `#B9922E` | Prestige/wealth — reserved for treasury figures, dignitas call-outs, rare celebratory moments |
| Blood Oxide | `#7A2E1F` | Crisis-only (revolt, insolvency, plague, disaster) — deliberately off the base palette so its rarity itself signals severity |

**Usage rules:** no screen uses more than three of the seven tokens as prominent color at once. Gold Leaf is the scarcest color in the system — more than one or two gold elements on a screen signals a hierarchy problem. The color-to-meaning mapping (purple = people, terracotta = land, bronze = neutral, gold = prestige, oxide-red = crisis) holds consistently everywhere so players build color literacy over time.

### 7.3 Typography

Three roles, never interchangeable:

- **Display** (headers, tituli, section banners) — a high-contrast, wide-set inscriptional capital face in the spirit of Roman monumental lettering. **Cinzel** (open-source) is a strong practical match; **Trajan Pro** is the premium alternative if licensing allows. Used sparingly, always caps, always letter-spaced, reserved for section titles and the top banner — never for body text or buttons.
- **Body** (menus, descriptions, dialogue, event narration) — a warm old-style serif with real texture. **EB Garamond** or **Spectral** (both open-source) fit; either reads well at length without feeling like a neutral system font.
- **Utility** (numbers, stat labels, ledgers, dates) — a slab or monospace face so figures read as stamped rather than typeset. **Roboto Slab** for a stamped-numeral feel, or **IBM Plex Mono** for a more mechanical ledger feel — either is a reasonable default; final choice should follow a side-by-side check against the actual UI.

**Scale:** a restrained type scale — roughly 4-5 sizes total, consistent with a document-styled interface rather than a typical app's finer gradation. Weight carries emphasis within body text; the display face's inherent weight does the work for headers.

### 7.4 Layout: The Wax Tablet Diptych

The organizing metaphor is the Roman *diptych* — two hinged, wax-coated leaves — joined by a shared ink-bar spine:

```
+------------------------------------------------------------+
|  INK BAR — gens name · date/season · treasury · dignitas    |
+------------------------+-------------------------------------+
|  LEFT LEAF             |  RIGHT LEAF                          |
|  identity / summary:   |  the screen's actual job:             |
|  portrait, key stats,  |  navigable list, stat panel,          |
|  a flavor quote        |  building grid, event choices, etc.   |
+------------------------+-------------------------------------+
```

**Per-screen application:**

| Screen | Layout |
|---|---|
| Villa Overview (hub) | Diptych — left: household summary; right: navigation |
| Familia individual record | Diptych — left: identity/portrait/traits; right: stat gauges and actions |
| Estate & Settlement | Diptych — left: tile-based plot map; right: grouped building chains |
| Travel | Diptych variant — left: journey/route/companions; right: resolves into the event modal on arrival |
| Court & Positions | Roster list, structurally close to the Familia roster |
| Legal/Court proceedings | Event-modal variant, more formal tone: case summary in the utility face at top, argument/decision choices below |
| Chronicle | Breaks the diptych deliberately — a single unfurling illuminated scroll, chronologically ordered, the most ornate screen in the game |
| Event modal | A smaller floating tablet over a dimmed background — narration at top, 2-4 wax-stamped choice buttons below |
| Map/regional view (Rival Houses, imperial events, Travel routing) | Mosaic-styled overworld, tile texture rather than flat leaves |
| Combat resolution | A simplified tactical strip — opposing rosters left/right, a shared field in the middle for terrain/modifiers, resolving to a short narrated outcome |

The ink-bar spine persists across every screen, diptych or not, so the player always keeps their bearings.

### 7.5 Iconography

A small, consistent icon set built from simple geometric forms rather than illustrative detail, so it holds up at small sizes and doesn't fight the document aesthetic:

- **People/status:** laurel wreath (prestige/office), togate silhouette (citizen), chain-link (enslaved), broken chain (freedman).
- **Land/economy:** wheat-sheaf (agriculture), amphora (trade goods/oil/wine), pickaxe-and-block (quarrying/mining), coin stack (treasury).
- **Civic/religion:** temple pediment (religious buildings/events), torch (festivals).
- **Military:** crossed gladius-and-shield (combat/security), watchtower (defense).

Icons render in Iron-gall Ink or Verdigris Bronze by default, recoloring only to indicate state (an oxide-red chain-link for a slave at high revolt-risk; a gold laurel for an office actually held versus a bronze one for an office merely eligible).

### 7.6 The Signature Element: The Wax Seal

Every consequential decision — approving a marriage, issuing an edict, sending soldiers to battle, confirming a manumission — is confirmed through a **wax seal** interaction: a circular seal-press motif that visually "sets" when pressed, rather than a generic button. This is the one place the design spends its boldness; everything else stays quiet and legible, concentrating personality into a single, thematically-justified gesture rather than distributing ornament everywhere. The Chronicle's unfurling-scroll presentation is the second-most-ornate element, positioned as the emotional payoff screen a player returns to and finds richer over time.

### 7.7 Motion & Feedback

- **Month advancement:** a brief, subdued animation — the wax seal pressing down, or a page turning — so time's passage always registers physically rather than cutting abruptly.
- **Stat changes:** gauges fill or drain smoothly rather than jumping, so a punishment or a good harvest reads as a felt event.
- **Crisis states:** the only place allowed a more insistent cue (a subtle pulsing border in Blood Oxide), reserved for genuine urgency so it isn't diluted into decoration.
- **General rule:** motion always represents something happening in the fiction — time passing, ink drying, a seal setting — never purely decorative flourish.

### 7.8 Sound Direction

Not required for a text/UI prototype, but worth setting direction early: ambient, diegetic sound (a stylus scratching wax for menu navigation, a door or gate for major screen transitions, distant field or market ambience under the hub) rather than a generic orchestral score, keeping audio consistent with the "this is a real document/household" conceit established visually.

### 7.9 Accessibility & Legibility

- Contrast ratios (Iron-gall Ink on Papyrus, and each accent against both) should be checked against standard text-contrast guidelines before implementation — the aged-document aesthetic shouldn't cost readability.
- Because the palette carries meaning, every color-coded state also needs a redundant non-color cue (an icon, a label, a pattern), so the game stays legible to colorblind players.
- A "reduce density" or "plain numbers" display mode is worth reserving as a settings-level option for players who prefer a spreadsheet-deep reading over the narrative-forward default.

### 7.10 Writing & Copy Voice

- Interface copy is written from the household's own point of view where possible — "The Household Attends" rather than a neutral "Main Menu" — since the whole UI is conceived as an in-fiction document.
- Action labels stay concrete and consistent through a flow: a button reading "Offer for Sale" leads to a confirmation and outcome that also say "sale," never quietly becoming "transfer" partway through.
- Event narration and flavor text carry the game's literary voice; UI chrome stays plain and functional so the two registers don't blur.
- Failure/empty states are written in-voice rather than as generic system errors — an empty granary reads as a steward's report ("The stores stand empty this month"), not "No data available."

### 7.11 Appearance & Portraiture

Every character's portrait is generated from a detailed underlying set of appearance attributes — height and build, facial structure, complexion, hair and eye color/style, notable features (a scar, a broken nose, gray at the temples with age), and status-appropriate dress/grooming that updates automatically with office, wealth, and age. This is the same principle *Free Cities* uses for its body-description system, adapted to portraiture: a rendered composite rather than a hand-placed illustration, so dozens of household members can each have a distinct, consistent likeness without hand-drawing every one.

**Customization layers:**
- **The player character** gets full customization at creation (every attribute directly selectable), consistent with the "Full Custom" start option, and can revise it later at natural narrative points (aging, injury, a change in status) rather than freely at any time.
- **Family and household members** get a lighter pass — the player can nudge or select from a constrained option set for close family, especially when building the initial household, but most newly-introduced members (a new hire, a purchased slave, a marriage candidate from a rival house) generate procedurally with only minor after-the-fact adjustment (a few "reroll" or "adjust" tokens, not a full character-creator pass) — keeping the emphasis on *discovering* who's joined the household rather than authoring them wholesale.
- **Visual treatment:** portraits sit in the identity leaf's portrait frame, rendered closer to a Fayum mummy portrait or a coin's profile bust than a modern photorealistic render — the generated-composite nature of the system is an asset here, not something to disguise. It's meant to look like a period-appropriate painted or engraved likeness.

---

## 8. Time Scale & Numbers Philosophy

- **Core time-advancement tick:** monthly, aligned to agricultural/seasonal cycles, with major life events (births, deaths, marriages, elections, campaigns) resolving on their own natural timelines within that cadence.
- **Numbers/UI philosophy:** stats stay mostly hidden or lightly abstracted by default — narrative/menu-forward, in keeping with the document conceit — but surface as clear stat panels wherever a system specifically benefits (a character sheet, a treasury ledger, a military roster), rather than committing globally to either a pure-narrative or full-spreadsheet style.
- **6.28 Steward/Council Auto-Management (quality of life)** — a trusted appointee (see Companions & Court Positions, §6.20) can run routine estate business while the player character is away via Travel, so leaving home doesn't mean the household simply freezes for the duration. Scope is QoL rather than simulation depth: sensible default handling of day-to-day decisions, with anything consequential still held for the player's return.

---

## 9. Content & Tone Guidelines

- **Slavery, violence, and political brutality** are simulated with real mechanical stakes and unflinching narrative tone — the same spirit *Free Cities* brings to its subject matter.
- **Sexual content is never a mechanic or a focus.** Romance and Seduction (§6.19) are fully systemized on their political and relational dimensions; the sexual dimension of any relationship is always handled indirectly — implied, faded to black, described the way a serious historical drama would — never depicted graphically, never given its own mechanical resolution.
- **No forced ending.** The game never declares the player has "won." Self-set objectives and an optional milestone catalog give structure without imposing a stop condition.
- **Historical frankness without gratuitousness.** Harsh systems (punishment, legal authority over life and death, warfare) are played straight rather than softened, but described with narrative purpose rather than for shock value.

---

## 10. Glossary

- **Gens** — a Roman clan or family line; the game's own title and unit of long-term progress.
- **Familia** — the full household in the Roman sense: family, slaves, freedmen, and clients together, not just blood relations.
- **Pater/Materfamilias** — the legal and social head of the household.
- **Patria Potestas** — a household head's formal legal authority over descendants, including marriage approval, disownment, and, in the harshest historical cases, life-and-death authority.
- **Dignitas** — public standing/prestige, the game's primary reputation stat.
- **Cursus Honorum** — the traditional sequence of public offices a Roman political career ascends through.
- **Vicus** — a village-scale settlement; the intermediate stage between a single villa and a full town or city.
- **Clientela** — the patron-client relationship structure underlying much of Roman social and political life.
- **Manumission** — the formal act of freeing a slave.
- **Diptych** — the two-hinged-leaf writing-tablet metaphor underlying the game's default screen layout.
- **Titulus** — the inscribed banner/nameplate metaphor underlying the top ink-bar present on every screen.

---

## 11. Full System Checklist

For the upcoming system-by-system design passes:

- [ ] 6.1 Familia
- [ ] 6.2 Estate & Settlement
- [ ] 6.3 Labor, Slavery & Punishment
- [ ] 6.4 Economy & Finance
- [ ] 6.5 Politics & Patronage
- [ ] 6.6 Religion
- [ ] 6.7 Military & Combat
- [ ] 6.8 Events
- [ ] 6.9 Succession & Dynasty
- [ ] 6.10 Rival Houses / Living World
- [ ] 6.11 Dynasty Chronicle
- [ ] 6.12 Policies & Edicts
- [ ] 6.13 Disease & Public Health
- [ ] 6.14 Education & Culture
- [ ] 6.15 Espionage & Information Network
- [ ] 6.16 Legal & Court System
- [ ] 6.17 Natural Disasters & Environment
- [ ] 6.18 Travel
- [ ] 6.19 Romance & Seduction
- [ ] 6.20 Companions & Court Positions
- [ ] 6.22 Games & Spectacle
- [ ] 6.23 Monuments & Legacy Building
- [ ] 6.24 Piracy & Banditry
- [ ] 6.25 Diplomacy with Non-Roman Peoples
- [ ] 6.26 Settlement Demographics
- [ ] 6.27 Correspondence & Letters
- [ ] 6.28 Steward/Council Auto-Management (QoL)

*(6.21 Reputation Duality folds into 6.5; Adoption folds into 6.9; Appearance & Portraiture is §7.11, a visual-identity element rather than a standalone gameplay system.)*

---

## 12. Open Design Questions

Flagged honestly rather than quietly assumed — worth resolving during the relevant system's own design pass rather than here:

- **Combat resolution formula:** §6.7 specifies stat-driven squad-level resolution in principle; the actual formula (how unit composition, terrain, and commander stats combine into an outcome) is undesigned.
- **Disaster frequency/tuning:** §6.17's disaster types are scoped but not yet tuned against how often they should plausibly hit a given estate.
- **Legal system procedural depth:** §6.16 establishes the fiction (lawsuits, patria potestas, magistrate rulings) but not yet the actual decision tree or how deep a single case goes.
- **Appearance attribute schema:** §7.11 establishes the principle; the actual attribute list (how many sliders/categories exist) is not yet enumerated.
- **Rival house AI depth:** §6.10 specifies legibility without micromanagement, but how rival gentes actually make decisions (a simple utility function vs. something richer) is open.
- **Milestone catalog contents:** referenced in the pillars and win/loss framing but the actual list of milestones has not been drafted.
- **Games & Spectacle resolution:** §6.22 establishes gladiator training and wagering in principle; how a given game's outcome and its Dignitas/Politics payoff are actually calculated is undesigned.
- **Settlement demographics granularity:** §6.26 establishes that population becomes its own thing to track once a villa grows; whether that's abstracted (a single "population" number with modifiers) or more granular (tracked cohorts — colonists, freedmen, laborers) is open.
- **Steward auto-management scope:** §6.28 sets QoL intent but not the actual decision boundary — exactly which categories of decision a steward may make alone versus must hold for the player is still to be defined.

---

## 13. Next Steps

This document is now the stable reference for everything downstream. The recommended next step is the system-by-system design pass, starting with **Familia** — the load-bearing system beneath Appearance, Companions, Romance, and Labor & Slavery alike — though **Travel** or **Companions & Court Positions** are reasonable alternate starting points given how recently they were scoped.
