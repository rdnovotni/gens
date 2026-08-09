# GENS — System Design: Military & Combat (§6.7)
*Two parallel tracks — a persistent private Estate Force the player actually builds, squad by squad, and a rare, real path into Rome's own military career — sitting on top of a shared Combat Resolution Engine built to serve Piracy & Banditry, Rival House feuds, and Games & Spectacle's arena combat the same way Characters' Scheme engine already serves scheming, seduction, and espionage. This pass adds the character-driven layer combat was missing entirely: enemy commanders as real Characters, named personal Retinues, Battlefield Duels, a rollable event table, real injury/capture/death stakes, prolonged Sieges, Naval Fleets, and the Muster mechanism Settlement Demographics was always waiting on. A further polish pass adds Mercenaries, real Desertion consequences for collapsed Morale, a costed and riskier Reconnaissance, an explicit no-safety-net answer for the player's own character falling in battle, and a Battle Report tying it all together — plus fixes a data-model bug that couldn't represent two captured commanders getting two different fates in the same engagement. A final balance pass closes the mercenary-squad-cap loophole, gives the two career tracks a genuine tradeoff instead of an implied one, clarifies the Praefectus's wage isn't double-counted, and rebalances the Random Battlefield Event table with a real positive entry.*

---

## Contents

1. Scope & Role
2. The Estate Force — Persistent Private Military
3. Roman Service — The Career Path
4. The Combat Resolution Engine
5. Battlefield Events & Personal Stakes
6. Deployment Types
7. War Spoils, Captives & the Aftermath Economy
8. Buildings & Goods — Recap, Not Redesign
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own framing is a deliberate constraint, not just a starting point: "estate-level security akin to the *Free Cities* pregmod security/combat mod... stat-driven squad-level resolution... without becoming a full tactical wargame." Per direction, this document builds **two genuinely parallel tracks, roughly equal in weight**:

- **The Estate Force (§2)** — a persistent private military the player actually constructs: build the Barracks, upgrade it, raise named squads, arm and man them, same shape as the *Free Cities* loop the direction named directly.
- **Roman Service (§3)** — a real, separate career path for a household member (or the player's own character) into Rome's actual military structure, distant and rare the way Politics & Patronage's cursus honorum is, rather than something every playthrough touches.

Both tracks resolve every actual engagement through one shared **Combat Resolution Engine (§4)** — designed now, in shape rather than in final numbers, specifically so Piracy & Banditry, Rival House feuds, and Games & Spectacle's arena combat can all plug into it later rather than each inventing bespoke fight resolution from scratch.

This document doesn't redesign the buildings or goods that already exist for it — Buildings §4.11's Watchtower→Barracks→Garrison→Fortress chain, the Armory, Siege Workshop, and Shipyard, and Resources & Goods' Weapons/Armor/Siege Engines/Horses are all unchanged, cross-referenced rather than rebuilt (§8).

---

## 2. The Estate Force — Persistent Private Military

### 2.1 Recruitment Pool & Legal Status

Per the decision to blend both options: the pool splits cleanly by what the manpower is actually for.

- **Defensive militia (baseline, unremarkable)** — armed for estate defense and local Suppression duty only, drawn from either the household's own enslaved workers (a temporary pull from labor duty, not a change of Legal Status or an enlistment in any citizenship-track sense) or the lowest tier of free citizens (Operarii, per Settlement Demographics §3). This is ordinary, expected practice for a Roman household defending its own property — no Dignitas cost, no narrative flag.
- **Campaign-eligible manpower** — anyone contributing to an actual offensive deployment or Roman Service (§3) must be free: Coloni, Operarii, or the Veterans pool Settlement Demographics §3.1 already built specifically as this document's recruitment source. Enslaved workers are never eligible here, matching Roman norm.
- **The extreme measure** — arming and deploying enslaved workers *offensively*, or in a genuine last-stand defense beyond ordinary militia use, is a real, rare, Dignitas-costly option reserved for actual crisis (a siege, an existential threat) rather than routine practice — narratively marked as unusual, per the historical-frankness pillar, not softened into an ordinary recruitment source. A slave who fights well under these circumstances is a strong, natural candidate for a battlefield manumission (Labor & Slavery's own mechanic), a real and historically attested outcome worth keeping available.

### 2.2 Building the Force

The literal *Free Cities*-style loop the direction called for: Barracks (Buildings §4.11) is the prerequisite for raising any real squad at all — a Watchtower alone provides only a passive security bonus, no actual manpower. From there, the player names and creates **Squads** directly, and the Barracks→Garrison→Fortress progression raises both the squad-count cap and the readiness ceiling available to them. City Walls & Gates adds a passive defensive multiplier to the whole Force rather than counting toward the squad cap itself.

### 2.3 Squad Composition

Each Squad is a single type — infantry, cavalry, siege, or militia — rather than a mixed-unit blob; the Force's actual tactical variety comes from fielding several differently-typed Squads together, keeping any one Squad's own internal bookkeeping simple while still giving the player real composition choices at the Force level. A Squad tracks:

- **Manpower** — headcount, drawn from §2.1's pool.
- **Equipment Tier** — set by how much Weapons/Armor (Armory, Buildings §4.5/§4.11), Horses (Stable), or Siege Engines (Siege Workshop) have been committed to it; a direct, real multiplier in the Combat Resolution Engine (§4).
- **Readiness/Training** — rises during downtime (drilling, ideally under a named commander per §3.2), falls after deployment; a Squad sent out repeatedly without recovery time fights as a genuinely worse version of itself, the same shape Familia's Fatigue Condition stat already uses for individuals.
- **Morale** — separate from Readiness; drops sharply after a costly or losing engagement, recovers slower than simple fatigue, and feeds directly into §4.6's aftermath. **Morale has a real floor, not just a vague penalty:** a Squad whose Morale collapses risks **Desertion** — a permanent manpower loss distinct from and beyond combat casualties, soldiers simply walking away rather than falling in the field. This is what actually gives Morale teeth as its own tracked stat rather than reading as Readiness with a different name; an unpaid (§2.4) or repeatedly-defeated Squad is a Squad that can quietly shrink even between engagements.

### 2.4 Upkeep & Wages

A recurring Economy & Finance expense (Wages, §4.1 of that doc) for every free/citizen Squad, scaling with headcount and Equipment Tier; militia drawn from enslaved workers costs no wage directly but carries a real opportunity cost — every worker pulled into militia duty is a worker not doing their ordinary Labor & Slavery-assigned job that month.

### 2.5 Muster — Calling Up the Veterans

The concrete mechanism behind Settlement Demographics' own promise that "a Military & Combat recruitment drive can call a portion of them back to active service" (§3.1 of that doc). A **Muster** is a discrete, deliberate action distinct from ordinary recruitment: rather than permanently growing the Estate Force, the player temporarily draws a portion of the settlement's Veterans pop group back into active Squads for the duration of a specific campaign or crisis. Mustered Veterans arrive with real starting advantages ordinary fresh recruits don't — meaningfully higher Readiness from the outset, and often a Battle-Hardened or similar Trait already in hand — but a Muster also directly shrinks the civilian Veterans pool for its duration (that document's own "temporarily smaller civilian labor pool" cost), and discharge afterward feeds them straight back rather than creating a second, separate Veteran wave.

### 2.6 Mercenaries — Buying Readiness Instead of Building It

A genuine third option, distinct from both the patient Estate Force build (§2.2) and a Muster's reliance on an existing Veterans pool: **hiring a mercenary company** — a real, historically attested practice, especially on the frontier — gives a player with denarii but no standing military infrastructure a way to field real, combat-ready manpower immediately. Mercenaries arrive at full Equipment Tier and Readiness, need no Barracks investment at all, and cost a steep, one-off-plus-recurring denarii premium (Economy & Finance) rather than the Estate Force's slower Wages/Muster tradeoffs. **This speed doesn't bypass the squad cap, though** — a mercenary Squad still occupies a slot against §2.2's Barracks-tier-driven cap like any other, specifically so denarii alone can never substitute for the building investment the whole Estate Force concept is built around; what mercenaries actually buy is *skipping the wait* to fill a slot the player has already earned, not a way around earning it. The real cost beyond price is reliability: a mercenary Squad's Loyalty is inherently shallow and directly tied to timely payment — a mercenary company that goes unpaid, or is on the losing side of a Costly Victory or worse, carries a real, elevated Desertion risk (§2.3) far above any citizen or Veteran Squad's baseline, the classic historical failure mode of relying on hired swords rather than one's own.

**The three methods side by side:** building the Estate Force (§2.2) is cheap and loyal but slow — the only path that grows the squad cap itself; Muster (§2.5) is fast and arrives battle-tested but depends entirely on having a Veterans pool to draw from in the first place; Mercenaries (§2.6) are instant and need no infrastructure at all, at the highest ongoing cost and the least trustworthy Loyalty. No single method dominates the other two — each trades speed against cost against reliability in a different direction, matching however the player actually got into a given crisis.

---

## 3. Roman Service — The Career Path

### 3.1 Auxiliary vs. Legionary Service

Directly resolves Settlement Demographics' own forward reference: a Peregrine or Latin-Rights individual — whether from the household or drawn from the Coloni/Operarii pool via that document's military loop (§5 of that doc) — serves as an **Auxiliary**, and gains full Roman Citizenship on discharge, exactly as that document already promised. A full Roman Citizen serves as a **Legionary**, or, with sufficient Social Class standing (Equestrian/Senatorial, per Politics & Patronage), enters directly as a junior **Officer**.

### 3.2 The Household Rank Ladder — Achievable, Local

Governs command of the Estate Force itself, and blends both directions given: mostly an informal progression any sufficiently-Martial Familia member or hired officer climbs, but capped by a real, formal Companions & Court Positions appointment at the top.

| Rank | Notes |
|---|---|
| Recruit | Entry point; commands nothing yet. |
| Optio | A Squad's second-in-command; the practical training rung. |
| Centurion *(informal, private-force use of the title)* | Commands a single Squad outright. |
| **Praefectus** | Commands the whole Estate Force — a genuine Companions & Court Positions Senior Position (that document's own naming pattern, alongside the Rationalis and Argentarius), not just an informal top rung. **Worth stating plainly:** the Praefectus's own personal wage (Companions & Court Positions' standard appointment cost, Economy & Finance §4.1) is a separate line from the rank-and-file Wages every Squad already costs (§2.4) — commanding the Force and staffing it are two different expenses, not one double-counted. |

### 3.3 The Roman Military Career — Distant, Rare Goal

The real, separate track, deliberately built to the same distant-and-rare shape as Politics & Patronage's cursus honorum rather than something every playthrough reaches. A sufficiently Martial household member, sponsored the same way that document's own cursus honorum requires a sponsor (§6 of that doc — often an existing Clientela relationship), can be noticed by Rome and offered a real commission:

- **Military Tribune → Legate**, the officer track, available to citizens of adequate Social Class and Dignitas-with-Rome (Reputation Duality, Politics & Patronage §2.1) — the direct military equivalent of that document's own property-census-gated Senate entry.
- **Centurion (Roman)**, the enlisted-and-risen-through-merit track — a real, historically attested alternate route that doesn't require Equestrian/Senatorial standing at all, giving a lower-Social-Class citizen a genuine path to real distinction purely through service, a nice soft-mobility story this project's other systems already value.
- The rare pinnacle — an actual triumph-worthy command — is exactly what Buildings §4.12's Triumphal Arch was named specifically to commemorate ("military-victory-specific, giving Military & Combat its own Monuments payoff"), and is a natural Dynasty Chronicle (§6.11, future) milestone.

**The real choice this creates:** a household's single most Martial member is genuinely worth more in one of these two tracks than the other, and the game never picks for the player. Keeping them as Praefectus (§3.2) means a commander the player directly controls, who trains the Estate Force's Readiness personally and is never more than one deployment decision away — real, ongoing value, entirely inside the player's own household. Sponsoring them into Roman Service (§3.3) trades that direct control away entirely — a Tribune or Legate serves at Rome's discretion, fights battles the player doesn't choose the terms of, and carries real, independent risk (§5.5 applies to them exactly as it would to anyone else) — in exchange for a shot at the kind of Dignitas, citizenship-track benefits for a non-citizen, and Dynasty Chronicle weight that private command can never quite match. Sending a talented household member away is a real sacrifice of control for a chance at real glory, not a strictly better option lurking behind the modest one.

### 3.4 Discharge, Veterans & Assimilation

Unchanged, cross-referenced rather than redesigned: discharge — not enlistment — is what actually creates a new Veteran (Settlement Demographics §5), a non-citizen typically emerges with full citizenship (§10 of that doc), and an occasional land-grant wave populates Veterans directly (§8.1 of that doc). This document is simply the system that was always meant to sit on the other end of that loop.

---

## 4. The Combat Resolution Engine

The shared backbone, designed in shape now per the decision to fully define the resolution's inputs, order, and outcomes without committing to real numbers yet.

### 4.1 Force & Combatant Composition

A **Force** is a collection of Squads (§2.3); a lighter engagement (a duel, a small raid, an arena bout) can resolve against a single Combatant or a handful, using the same engine at a smaller scale rather than a different one. Squad/Combatant types — Militia, Auxiliary, Legionary, Cavalry, Siege, and a looser **Irregular** type covering pirates, bandits, and gladiators for the systems that will eventually plug in here — are all read identically by the engine. A **Fleet** (Shipyard/Navalia-built warships, Buildings §4.11) is the naval mirror of a Force, resolving through the same engine with Coast/River terrain treated as the naval equivalent of §4.3's terrain fit — this is Piracy & Banditry's most direct hook into the engine, since most of that system's raids are inherently maritime.

### 4.2 Commander Inputs

A named commander is never optional in practice — a Force without one resolves against a flat, unfavorable default rather than a neutral one, which is deliberate: it makes assigning a real commander (§3.2's ladder) a genuine decision with a genuine cost to skipping. Where a commander is present, their Martial Core Attribute, relevant Personality Axes (Boldness for aggression, Rationality for tactical soundness), and relevant Traits (Strategist/Berserker, Battle-Hardened/Shell-Shocked, Inspiring/Feared Commander — all already built in the Traits catalog specifically for this) directly weight the resolution. **This runs identically for the enemy side** — a Rival House commander, a pirate captain, or a frontier war-leader is a full Character (per the Characters system), generated on demand exactly per that document's lazy-instantiation rule (§11 of that doc) the first time the player actually faces them, not an anonymous stat block wearing the "defender" label. §5 builds directly on this fact.

### 4.3 Terrain & Situational Modifiers

Terrain (Estate & Settlement's existing plot/region tags — Hills, Forest, Coast, River, Fertile Plain) favors or penalizes specific Squad types the way real tactics would: Cavalry favors open Plain, Infantry favors Hills/Forest, Siege is only relevant against a fortified target at all. Situational modifiers stack on top: the defender's City Walls/Fortress tier, a surprise/ambush condition (Piracy & Banditry's typical shape), a numbers disparity, and each side's current Readiness and Morale.

### 4.4 Resolution Steps

1. **Assemble** both sides' Forces (or ad-hoc Combatant sets for a lighter engagement).
2. **Compute effective strength** per side: manpower, weighted by Equipment Tier, Readiness, the commander modifier (§4.2), and terrain/situational fit (§4.3).
3. **Compare with variance** — effective strength sets the odds, not a guaranteed outcome; a real, if usually small, chance for the weaker side to win outright, consistent with how this project already treats every other high-stakes resolution (Characters §10's Schemes never being a guaranteed thing either).
4. **Resolve losses** proportional to the disparity and the specific outcome tier reached (§4.5).
5. **Resolve aftermath** — Readiness and Morale shift for both sides, not just the loser (§4.6).

### 4.5 Outcomes

Five real outcomes, not a binary win/lose, matching the same real-outcome-diversity philosophy Characters' Scheme engine already established:

- **Decisive Victory** — minimal own losses, a real aftermath bonus, and access to §7's spoils/captives.
- **Costly Victory** — the objective is achieved, but losses are real on both sides.
- **Repulsed/Stalemate** — no resolution; both sides withdraw and the underlying situation persists unresolved.
- **Defeat** — the Force routs, takes real losses, and any present commander or Familia member is exposed to §4.6's capture risk.
- **Catastrophic Defeat** — the Force is effectively destroyed; a present commander faces real casualty risk, and the aftermath is severe and lasting.

### 4.6 Casualties, Morale & Personal Stakes

Casualties reduce a Squad's manpower permanently, requiring real re-recruitment rather than simply regenerating; a Familia member present as commander can take a Permanent Injury (Familia §3.1) or die outright — this is where Military & Combat's stakes become genuinely personal rather than purely economic. A captured commander (either side) becomes a real Character available for Ransom (Characters §9.5) — the Interaction Catalog's existing mechanism, not a new one invented here.

### 4.7 Sieges — A Prolonged Variant

A genuine gap in treating Siege purely as "a Squad type only relevant against a fortified target": a real siege isn't a single resolution tick, it's a sustained state. Once a Siege-type Squad or Force commits against a Fortress/City Walls target, the engagement enters a **prolonged mode**: it persists over multiple months rather than resolving immediately, drawing down the besieger's own supply (an ongoing Economy & Finance cost) and the besieged settlement's Horreum reserve (Resources & Goods' granary stock, the same one Settlement Demographics' Annona lever draws on) each tick it continues. A siege ends one of three ways, each with a genuinely different aftermath:

- **Relief** — a friendly Force arrives and breaks the siege in a normal §4.4 engagement against the besieger.
- **Negotiated Surrender** — the besieged capitulates before a breach; spoils and captives (§7) are real but reduced, and the settlement's own population/buildings go untouched — the "civilized" outcome, and the one that costs the besieger the least standing.
- **The Sack** — the besieger breaches and takes the settlement by force; spoils and captives are maximized, but at a real, lasting Dignitas and Reputation Duality cost (Politics & Patronage §2.1), and Settlement Demographics' Contentment/population takes a genuine hit if the settlement is the player's own future territory rather than a target they intend to simply plunder and leave.

---

## 5. Battlefield Events & Personal Stakes

The piece this pass exists to add: combat with named people on both sides, not just opposing strength totals. Every mechanism below runs through the Characters system directly rather than inventing a parallel character model — this section is almost entirely composition of things that already exist, aimed at a new purpose.

### 5.1 Enemy Commanders Are Characters Too

Per §4.2's note: any opposing leader — a Rival House member, a generated pirate captain or frontier war-leader — is a full Character, carrying real Traits, Personality Axes, and their own relationship web, generated the moment the player actually faces them (Characters §11's lazy instantiation). This has an immediate, concrete payoff: **Reconnaissance**, a pre-engagement action (an Intrigue or Martial check, boosted by a Spymaster or Naturalist Trait) that reveals some of the enemy commander's key Traits, *and* a rough read on the opposing Force's own composition and size, before the player commits — informed risk, not a total black box. A scouted report that the enemy leader is Craven and Undisciplined changes the decision to attack; one that reveals Battle-Hardened and Ruthless should too. **Reconnaissance isn't free:** it costs real time — the engagement window it's spent scouting is a window the enemy can use to reinforce, retreat, or simply notice the scouting itself, a small, real discovery risk of its own (mechanically an echo of Characters §10's Scheme discovery, though lighter-weight) that can turn a cautious player's information-gathering into the very ambush they were trying to avoid.

### 5.2 The Personal Retinue

A commander of Centurion rank or above can maintain a small named **Retinue** — a handful of Companions or Familia members fighting directly alongside them, the CK3 "knights" the direction pointed at. A Retinue does real work beyond flavor:

- It **absorbs personal risk** — a Retinue member can be wounded or killed protecting the commander specifically, converting what would otherwise be the commander's own casualty roll into someone else's, a real sacrifice with real relationship-web weight (Characters §7) rather than an abstract shield value.
- A Retinue member who distinguishes themselves in a Decisive Victory is a genuine candidate for a new Reactive Trait (Battle-Hardened, Inspiring Commander) or a promotion up §3.2's rank ladder — this is the concrete mechanism that actually populates that ladder with people worth commanding, rather than leaving it an abstract title track.
- **A Retinue run dry is a real warning sign** — once every Retinue member is lost, the commander themselves fights genuinely exposed for the rest of that engagement and any that follow until it's rebuilt, a visible, legible risk state rather than a hidden one.

### 5.3 Battlefield Duels

Characters §9.6 already defined Duel as a formal, Honor-governed, consensual violent Interaction. A **Battlefield Duel** is that same Interaction, given real stakes beyond personal Dignitas: before or during a major engagement, either commander (the player's or, per Characters §8.3's "Characters act on their own" rule, a sufficiently Bold/Berserker-Trait enemy) can issue a formal challenge. Winning grants the challenger's whole side a real, one-time strength/Morale boost for that engagement — single combat before a battle was genuine historical practice, not a flavor invention — while losing crushes the loser's side's Morale, and a commander's death or capture in the Duel itself (resolved through §5.5's own injury/capture/death mechanics exactly as any other battlefield casualty would be, not a separate roll) can end the surrounding engagement immediately, bypassing §4.4's normal resolution entirely rather than merely feeding into it. Always optional for the player; never required to resolve an engagement.

### 5.4 Random Battlefield Events

A rollable table of mid-engagement events, each carrying a real, if modest, mechanical nudge on top of its narrative texture — flavor with teeth, not just narration laid over a fixed number:

- **The Standard Falls** — a Squad's banner is lost or captured mid-fight: a sharp Morale penalty, a real Dignitas hit, and a genuine disgrace real Roman commanders (Crassus at Carrhae, Varus at Teutoburg) suffered exactly this way — and a recoverable one, since recapturing a lost standard in a follow-up engagement is a natural, ready-made revenge arc.
- **A Rousing Speech** — an Eloquent, Inspiring Commander, or sufficiently Bold commander turns a tense moment into a genuine Morale surge for their whole side — the table's clean, unambiguous positive, balancing the several ways an engagement can go wrong with at least one clear way it can go right beyond simply winning outright.
- **A Soldier's Sacrifice** — a named Retinue or Squad member saves the commander at real personal cost, feeding §5.2's mechanism directly and offering a genuine relationship-web moment (gratitude, guilt, a posthumous reward for the family) rather than a silent stat trade.
- **The Enemy Commander Is Exposed** — a mid-engagement tactical opening to press directly at the opposing leader specifically: a bonus to capturing or killing them, at real personal risk to whoever presses it.
- **Weather Turns** — a storm, mud, or heat shifts §4.3's terrain fit mid-engagement, a light Natural Disasters crossover.
- **Plague in the Camp** — reserved for a prolonged Siege (§4.7) specifically: a Disease & Public Health crossover that can force an early Negotiated Surrender or Relief attempt rather than let the siege simply grind on unaffected.

### 5.5 Injury, Capture & Death — The Personal Ledger

§4.6 stated the bare mechanism; this is its actual texture. Injury severity scales with both the outcome tier reached *and* the commander's own choices — a Herculean, Bold commander who leads from the front (rather than directing from the rear) takes on real additional personal risk in exchange for the strength/Morale bonus their presence provides, a genuine risk-for-reward trade rather than a stat that's purely beneficial with no cost attached. **Capture** specifically opens real choices for the captor, not just Ransom: Ransom (Characters §9.5) remains the standard, expected resolution, but a captor can instead choose a harsher path — execution, public humiliation, or handing the captive to Legal & Court (§6.16, future) if some legal claim applies — each carrying its own Dignitas and Reputation Duality consequences rather than a single "correct" outcome. **Death** in battle triggers Succession & Dynasty's (§6.9, future) inheritance mechanics directly if the fallen held that role, is always a Dynasty Chronicle (§6.11, future) entry, and drops the affected Squad's Morale sharply — with the Squad's own Optio (§3.2) field-promoted temporarily to hold the line rather than leaving the unit leaderless mid-engagement.

**When the fallen is the player's own character specifically:** this is the sharpest version of the stakes above, not a special case requiring different rules — the same injury, capture, and death mechanics apply without a safety net. Consistent with the core design pillar that "the game never declares the player has won" (and, symmetrically, never simply ends because a single character died): a player-character death in battle hands off directly through Succession & Dynasty's own inheritance resolution, the same as a death from age or illness would, rather than triggering any battle-specific game-over state. Leading personally is a genuine, weighable risk the player opts into for its Boldness-driven strength bonus — not a decision the game silently protects them from.

### 5.6 The Battle Report

Everything above — Reconnaissance findings, a Battlefield Duel's result, whichever Random Events actually rolled, casualties, captures and their resolutions, spoils — compiles into one readable **Battle Report** the moment an engagement resolves, the same automation-plus-legible-summary pattern this project already uses everywhere else (Economy & Finance's Ledger, Settlement Demographics' migration report). The player's real point of contact with most engagements — especially routine Defense/Suppression actions a Praefectus can be trusted to handle without direct oversight — is reading this report afterward, not micromanaging every resolution step in real time; a major campaign or a Battlefield Duel the player personally initiated is where they'd actually watch it unfold instead.

---

## 6. Deployment Types

- **Defense** — reactive; the Estate Force defends against a Piracy & Banditry raid, a Labor & Slavery revolt in progress, or a Rival House incursion, with the defender's terrain and fortification bonuses (§4.3) fully active.
- **Suppression** — proactive, but contained to the player's own territory: putting down an active slave revolt or clearing out local banditry before it reaches raid stage.
- **Offense/Campaign** — the Force deploys outward, whether against a specific target (a punitive raid, seized land) or as a lump contribution to an actual Roman campaign (distinct from an individual joining Roman Service per §3 — a household can send troops *and* a son can hold a commission, as genuinely separate contributions). **Worth stating explicitly:** a campaign fought against a non-Roman native population, in a Reputation Duality setting (Politics & Patronage §2.1), is a real values tension rather than a free win — it can raise Dignitas-with-Rome while directly damaging local standing, the same divergence that document's own frontier framing always implied was possible, now with a concrete mechanism actually producing it.
- **Private Feuds** — the concrete mechanism behind Characters §9.6's "Declare a Feud" interaction and Rival Houses' (§6.10, future) forward hook: two households' Forces can engage each other directly through this same engine, entirely outside of and unsanctioned by Rome, the private-conflict layer the core doc's "estate-level security" framing always implied was possible.
- **Naval** — any of the above, fought between Fleets (§4.1) rather than land Forces; Piracy & Banditry's raids are overwhelmingly this deployment type, and a Lighthouse's passive protection (Buildings §4.10) sits alongside a Shipyard-built Fleet's active one the same way City Walls and a land garrison already complement each other.

---

## 7. War Spoils, Captives & the Aftermath Economy

- **War Spoils** feed Economy & Finance's Windfalls category (§3.4 of that doc) directly — a Decisive or Costly Victory's concrete payoff, and, per §4.7, a Sack's maximized (but Dignitas-costly) version of the same payoff.
- **War Captives** feed Labor & Slavery's own acquisition list (§2 of that doc already names "war captives" as a direct output of Military & Combat campaigns) — this document is simply where that output actually gets generated.
- A captured **enemy commander or notable** is a real Character, and §5.5 already covers the real choice a captor faces beyond simple Ransom.
- **Military Supply Contracts** (Economy & Finance §3.2) remain the lower-risk alternative to actually deploying: an Armory/Stable-heavy estate can simply sell Weapons, Armor, and Horses to Rome's war effort at a premium rather than fielding a Force at all.

---

## 8. Buildings & Goods — Recap, Not Redesign

Unchanged, cross-referenced only: Watchtower → Barracks → Garrison → Fortress (Buildings §4.11) remains the Estate Force's construction ladder; Armory, Siege Workshop, and Shipyard/Navalia remain the equipping buildings; City Walls & Gates remains the passive fortification multiplier; Weapons, Armor, Siege Engines, and Horses (Resources & Goods) remain the goods that actually arm a Squad. Nothing in this document alters any of that — it's the consumer of that material layer, not a second design pass on it.

---

## 9. Cross-System Integration

- **Settlement Demographics:** §3's recruitment pool and §3.1/§10's Veterans/Assimilation loop finally get the "other end" that document built and named this system as waiting on; §2.5's Muster is the literal mechanism behind that document's own "recruitment drive" reference.
- **Buildings & Resources and Goods:** §8 — the entire material/construction layer is theirs, unchanged.
- **Labor & Slavery:** §2.1's militia/extreme-measure framing and §7's War Captives both feed directly into that document's own acquisition and Regimen mechanics.
- **Economy & Finance:** Wages (§2.4), Mercenary hiring costs (§2.6), War Spoils as Windfalls (§7), and Military Supply Contracts (§7) are this document's concrete contributions to that system's income/expense categories; a prolonged Siege's supply drain (§4.7) is a new, real ongoing cost.
- **Politics & Patronage:** the cursus honorum's sponsor mechanic and Reputation Duality both gate §3.3's Roman Service track directly; a triumph is a Dignitas event of the first order; §4.7's Sack outcome and §5.5's harsher capture choices both carry real Reputation Duality costs.
- **Companions & Court Positions:** the Praefectus (§3.2) is a new, named Senior Position in that document's own pattern.
- **Characters:** §4.2's commander weighting reads Strategist/Berserker, Battle-Hardened/Shell-Shocked, and Inspiring/Feared Commander directly; §5 as a whole — Reconnaissance, Retinues, Battlefield Duels, and capture/ransom — is built entirely on that document's existing Character schema, Interaction Catalog, and lazy-instantiation rule rather than inventing a parallel one.
- **Traits:** War Hero/War Criminal, Debt-Scarred (indirectly, via campaign financing), and the whole Martial-flavored Lifestyle set (Strategist, Duelist, Gladiator's Heart) all find their concrete mechanical home here for the first time; §5.2's Retinue is the concrete mechanism that actually grants Battle-Hardened and Inspiring Commander in the first place.
- **Piracy & Banditry (§6.24, future):** inherits the Combat Resolution Engine (§4) wholesale as its own eventual fight resolution, plus §6's Defense/Suppression/Naval deployment types directly — the Fleet concept (§4.1) is this document's most direct hook into that system specifically.
- **Rival Houses (§6.10, future):** §6's Private Feuds is this document's concrete contribution to that system's eventual conflict mechanics; §5.1's enemy-Character generation is exactly the mechanism a Rival House commander should use.
- **Games & Spectacle (§6.22, future):** the core doc's own note that gladiatorial combat "shares some resolution logic with Military & Combat" is realized directly through §4's Irregular Combatant type and the engine's smaller-scale mode.
- **Buildings' Triumphal Arch (§4.12):** §3.3 gives that Monument its actual triggering achievement.
- **Disease & Public Health (§6.13, future):** §5.4's "Plague in the Camp" event and §4.7's siege supply drain are this document's forward hooks into that system.
- **Dynasty Chronicle (§6.11, future):** a triumph, a captured/ransomed commander, a lost standard, and a Catastrophic Defeat or battlefield death are all natural milestone-catalog entries.
- **Familia:** §4.6's Permanent Injury/death risk and §5.5's captured-commander stakes are this document's sharpest tie into that system's own mortality and injury mechanics; a battlefield death's Succession & Dynasty trigger is a direct, real consequence rather than a flagged-but-unwired one.
- **Succession & Dynasty (§6.9, future):** §5.5 explicitly routes a player-character's own battlefield death through that system's inheritance resolution rather than any special-cased game-over state, honoring the core doc's "no forced ending" pillar directly.
- **Diplomacy with Non-Roman Peoples (§6.25, future):** §6's Offense/Campaign deployment type is this document's concrete source of the Dignitas-with-Rome-vs-local-standing tension that system will eventually need to resolve in full.

---

## 10. Data Model

```
Squad {
  squadId, settlementId,
  type,              // "militia" | "auxiliary" | "legionary" | "cavalry" | "siege"
  manpower,
  equipmentTier,       // driven by committed Weapons/Armor/Horses/SiegeEngines stock
  readiness,           // 0-100, rises with drilling downtime, falls after deployment
  morale,              // 0-100, distinct from readiness, slower to recover
  desertionRisk,        // §2.3 — rises as morale bottoms out; elevated baseline if isMercenary
  isMercenary: bool,    // §2.6 — full equipment/readiness on hire, no Barracks needed, steep recurring cost
  commanderId,         // a Character id; absent = flat unfavorable default in resolution
  standardIntact: bool,  // §5.4 — false after a lost/uncaptured-back standard
}

Fleet {              // §4.1 — the naval mirror of EstateForce
  settlementId,
  ships: [...],        // Shipyard/Navalia-built vessels, mirroring Squad's role at sea
  fleetCap,
}

EstateForce {
  settlementId,
  squads: [Squad, ...],
  squadCap,             // driven by Barracks/Garrison/Fortress tier
  fortificationBonus,    // City Walls & Gates
}

MilitaryCareerRecord {
  characterId,
  track,             // "estateForceLadder" | "romanService"
  rank,              // "recruit" | "optio" | "centurionPrivate" | "praefectus" |
                      // "auxiliary" | "legionary" | "centurionRoman" | "tribune" | "legate"
  citizenshipGainedOnDischarge: bool,   // §3.1 — true for non-citizen auxiliary service
}

MusterRecord {         // §2.5
  settlementId,
  monthActivated,
  veteransDrawn,
  active: bool,
}

Retinue {              // §5.2 — a commander's named companions
  commanderId,
  members: [ characterId, ... ],   // small, hard-capped
}

BattlefieldDuel {       // §5.3 — an optional pre/mid-engagement layer on top of CombatEngagement
  engagementId,          // the parent CombatEngagement, if any
  challengerId, defenderId,
  outcome,              // "challengerWins" | "defenderWins" | "declined"
  sideEffect,           // "strengthMoraleBoost" | "moraleCrush" | "engagementEndedEarly"
}

CombatEngagement {          // §4 — the shared engine, reused by Piracy & Banditry, Rival House feuds, Games & Spectacle
  engagementId,
  attackerForce, defenderForce,     // Force/Squad sets, Fleets, or ad-hoc Combatant lists for lighter fights
  deploymentType,     // "defense" | "suppression" | "offenseCampaign" | "privateFeud" | "naval" | "arena" | "piracySuppression"
  terrain, situationalModifiers: [...],
  isSiege: bool, siegeMonthsElapsed,       // §4.7
  battlefieldEvents: [...],                 // §5.4 — rolled during resolution
  duel: BattlefieldDuel | null,               // §5.3
  reconnaissance: { performed: bool, discovered: bool, findings: [...] },   // §5.1
  outcome,            // "decisiveVictory" | "costlyVictory" | "repulsedStalemate" | "defeat" | "catastrophicDefeat" |
                       // "negotiatedSurrender" | "theSack"   (siege-specific, §4.7)
  casualties: { attacker, defender },
  desertions: { attacker, defender },     // §2.3 — distinct from combat casualties
  capturedCommanders: [ { characterId, resolution } ],   // §5.5 — each capture resolved independently:
                                                           // "ransomed" | "executed" | "humiliated" | "legalMatter"
  spoilsGenerated, captivesGenerated,
}

BattleReport {       // §5.6 — the readable summary generated from a resolved CombatEngagement
  engagementId,
  narrativeSummary,
  keyMoments: [...],    // events, the duel, notable individual casualties/captures — the "highlights," not the raw log
}
```

---

## 11. Open Questions

- **All numeric sizing.** Per the decision to design shape rather than formulas: squad-cap-per-Barracks-tier, equipment tier bonuses, the variance band in §4.4, and the casualty curve are all deliberately unsized.
- **Squad cap's exact scaling.** §2.2 ties the cap to Barracks/Garrison/Fortress tier without specifying the actual numbers per tier.
- **Games & Spectacle's "no-death-by-default" question.** §4's Irregular Combatant type is built to serve arena combat, but whether that context needs its own outcome variant (a non-lethal default distinct from §4.5's five) is left for that system's own pass.
- **Multi-settlement Force logistics.** Whether a Force raised at one settlement can defend or deploy from a second one the player also holds isn't specified — the same open question Economy & Finance and Politics & Patronage already carry for multi-settlement play generally.
- **Roman Service commission trigger detail.** §3.3 gestures at "a rare, high-stakes Event," mirroring the cursus honorum's own unspecified trigger — neither document nails down the actual qualifying threshold.
- **Battlefield manumission's exact criteria.** §2.1 flags this as a real, available outcome without specifying what "fought well" actually means mechanically.
- **Retinue size cap.** §5.2 establishes a Retinue is "small" and "hard-capped" without stating the actual number, or whether it scales with rank (a Praefectus plausibly warranting a larger one than a mere Centurion).
- **Battlefield Duel's own resolution formula.** §5.3 establishes real stakes and an early-termination possibility but not the actual attribute/Trait weighting distinct from an ordinary Characters §9.6 Duel — presumably heavier on Martial and lighter on the social Honor-axis framing an ordinary Duel uses, but unconfirmed.
- **Random Battlefield Event frequency and full table size.** §5.4 sketches five illustrative events; the actual roll frequency per engagement and the full eventual table size are both left for a later content pass.
- **Siege duration and supply-drain rate.** §4.7 establishes sieges as multi-month and resource-draining without sizing either the typical duration or the drain curve.
- **Fleet cap scaling.** §4.1/§10 mirror EstateForce's squadCap pattern for Fleet without specifying the Shipyard-tier-to-fleetCap relationship.
- **Mercenary pricing and desertion-risk curve.** §2.6 establishes the cost/reliability tradeoff shape without sizing either the hiring premium or how much faster a mercenary Squad's Desertion risk actually climbs relative to a citizen one.
- **Desertion threshold.** §2.3 ties Desertion to Morale bottoming out without specifying the actual threshold or whether it's a single cliff-edge or a smoothly rising risk.
- **Reconnaissance's discovery-risk sizing.** §5.1 establishes that scouting carries its own small discovery risk without sizing it relative to an ordinary Characters §10 Scheme's own curve.
- **Squad cap sharing across manpower types.** §2.6 establishes mercenaries occupy the same squad cap as citizen/Veteran Squads rather than bypassing it, but whether a Mustered Veteran Squad also draws against that same cap or has its own separate temporary allowance isn't specified.
