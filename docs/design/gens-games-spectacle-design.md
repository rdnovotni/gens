# GENS — System Design: Games & Spectacle (§6.22)
*Resolves two open questions flagged directly by name — the core doc's own "how a game's outcome and Dignitas/Politics payoff are actually calculated" and Military & Combat's "whether arena combat needs its own non-lethal-by-default outcome variant" — while giving all three named spectacle types (gladiatorial combat, chariot racing, theater) genuinely distinct mechanical identities. This pass generalizes Fame into a universal mechanic any Character can earn through any kind of deed, not just arena performance, and adds beast hunts, executions, group battles, and the rare, spectacular naumachia as real game types alongside the original three. A final pass ties damnatio sentencing to real Legal Status distinctions, connects a well-hosted game to Politics & Patronage's Influence resource, and closes the loop with Piracy & Banditry's Targeted Contracts — a Famous competitor is a real liability, not just an asset.*

---

## Contents

1. Scope & Role
2. Fame — A Universal Mechanic
3. Gladiatorial Combat
4. Beast Hunts & Executions
5. Group Battles & Naumachia
6. Chariot Racing
7. Theatrical Performances
8. Staging a Game — The Funding & Hosting Loop
9. Audience Wagering
10. Fielding Your Own Competitors
11. Dignitas & Politics Payoff
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "sponsoring gladiatorial games, chariot races, and theatrical performances as a far bigger patronage lever than a generic funded event under Policies & Edicts. Includes acquiring and training gladiators as a distinct labor subtype... audience wagering, and games as a direct, visible Dignitas and Politics & Patronage investment." Two open questions were flagged by name, waiting specifically for this document: the core doc's own "how a given game's outcome and its Dignitas/Politics payoff are actually calculated," and Military & Combat §4.5's "whether arena combat needs its own non-lethal-by-default outcome variant." Both are resolved directly, in §11 and §3 respectively.

The backlog this composes from: the Ludus and Lanista (training and its operator), the Editor and the Editor Muneris (venue-level and settlement-capstone hosting authority), four venues (Amphitheater, Circus, Theatre, Odeon), Military & Combat's Combat Resolution Engine and Irregular Combatant type (built explicitly to share logic with this system), the Aedile's funding duty (Politics & Patronage), Gladiator's Heart/Bloodlust/Squeamish (Traits), the Stable-to-Circus horse supply, and Settlement Demographics' already-established Contentment payoff from funded games.

---

## 2. Fame — A Universal Mechanic

Per direction, generalized well beyond this document's own competitors: **Fame** is a real, persistent, 0–100 tracked score any Character can carry — not exclusive to a gladiator, charioteer, or performer, and not owned by this document alone. It's the individual, personal counterpart to a house's own Dignitas: a Character can be famous in their own right, independent of (and sometimes in spite of) their family's standing.

**Fame is a magnitude, not a verdict.** It measures how *known* someone is, not whether they're admired or despised — that reading comes from whichever Traits, Dignitas history, and Reactive events actually produced the fame in the first place. A War Criminal and a War Hero can carry comparable Fame; what differs is how people react to hearing the name, not how many people have heard it.

**Generators, across the whole project, not just this document:**

- **Games & Spectacle** — a real, major source: surviving bouts (§3), a racing career (§6), a triumphant performance (§7). This document remains the single richest generator of Fame, but no longer the only one.
- **Politics & Patronage** — winning a contested election, reaching a real cursus honorum rung, an actual triumph.
- **Military & Combat** — the War Hero Trait, a Retinue member who distinguished themselves, a Battlefield Duel won publicly.
- **Dynasty Chronicle** — any Legendary-tier entry a Character is personally attached to is a real Fame contributor in its own right.
- **Legal & Court / Espionage / Romance & Seduction** — the *notorious* side of the same coin: a public capital trial, a traced Espionage operation, a Scandalous affair all raise Fame exactly as much as a triumph does — it's simply fame of a different color.

**What Fame actually does, mechanically:**

- Scales ticket draw and wagering interest whenever a Famous Character performs, races, or fights (§9).
- Gives a real first-impression modifier in ordinary Interactions (Characters §9) — a Famous Character's reputation precedes them, for better or worse, the same way Reconnaissance removes a blank slate in Military & Combat.
- Raises a Character's own house's visibility in Rival Houses' "Notable Families" pre-contact list (§7 of that doc) faster than an equally wealthy but unknown house would earn on its own.
- Decays slowly if genuinely inactive — a retired champion's name fades as new ones rise, the same shape Influence (Politics & Patronage §4.4) already uses for exactly this reason.

This document adds the field and the generators most directly under its own control; the mechanic itself belongs to Characters' own schema (§13), available to every system in this project rather than fenced off here.

---

## 3. Gladiatorial Combat

### 3.1 Gladiators as Named Characters

Per direction: a gladiator entering serious competition is a full Character (Characters §11's lazy instantiation), reusing the exact same framework everyone else in this project does rather than a parallel system. Their Martial, Physique tier, and Personality Axes (Boldness especially) drive performance directly. **Sourcing** draws from Labor & Slavery's own acquisition and Legal & Court's sentencing options: a Ludus-trained slave, a condemned criminal (a real historical sentence, *damnatio ad ludum* — "condemned to the games" — now a concrete Legal & Court §9 conviction outcome alongside fine/exile/debt bondage/execution, and, per §4.2's own note, not normally available against a full Roman Citizen), or, rarely and at real Dignitas cost if it becomes known, a free volunteer (*auctoratus*, a real attested practice).

### 3.2 Match Types — Lethality as a Real Choice

Per the decision that both lethality modes are genuinely possible depending on the match and the player's own choice: an Editor scheduling a match sets its **Match Type**, which sets the baseline stakes rather than a single global toggle.

- **Ad Digitum** ("to the finger," the classic gesture) — the standard, missione-eligible match. Mercy is the *expected* default outcome for a defeated gladiator, not a guarantee: even here, a real **Crowd Verdict** roll — reading the fight's own drama, the loser's performance and Boldness, and the hosting Editor's own reputation — can still end in death despite mercy being the norm, keeping the setting's historical frankness intact rather than making the "safe" match type actually risk-free.
- **Sine Missione** ("without release") — a declared death match, the real historical term for exactly this. No mercy option exists; the match only ends when one side yields no further or dies. This draws a bigger crowd, sharper wagering interest, and a larger Dignitas swing (§11) — at the real cost of destroying a trained, valuable asset outright, a genuine spectacle-versus-asset-preservation tradeoff matching how this project treats every other high-value resource tension.

### 3.3 Fighting Styles

Real historical gladiator types, kept as flavor with a light, genuine tactical lean rather than a deep sub-system of their own: **Murmillo** (heavy, sword and shield — rewards Strong/Herculean), **Retiarius** (net and trident, fast and lightly armored — rewards Nimble/Perceptive), **Thraex** (curved blade, small shield), **Secutor** (a helmet design specifically countering the Retiarius — the classic, historically real style matchup), **Dimachaerus** (two swords, rare and flashy). A style matchup is a real, modest modifier in resolution (§3.4), the arena's version of terrain fit.

### 3.4 Resolution — Sharing the Combat Resolution Engine

This is the direct answer to Military & Combat's own flagged open question. A gladiatorial bout resolves through that document's Combat Resolution Engine (§4) at its smallest scale — a single Combatant against another, exactly as that document already anticipated — with Fighting Style (§3.3) standing in for terrain fit. The same five outcome tiers apply, remapped for the arena rather than the battlefield:

- **Decisive Victory** — a clean win; the loser yields.
- **Costly Victory** — the winner is also genuinely wounded.
- **Repulsed/Stalemate** — a real historical outcome: *stans missus*, both fighters stood down undecided, a rarer result that leaves the crowd unsatisfied.
- **Defeat** — the loser yields and faces §3.2's Crowd Verdict (Ad Digitum) or the fight simply continues (Sine Missione).
- **Catastrophic Defeat** — where Match Type actually bites: under Ad Digitum, this resolves as severe injury plus a harsher-weighted Crowd Verdict roll (mercy is still possible, less likely); under Sine Missione, this — or even an ordinary Defeat — resolves as death outright, no roll required.

### 3.5 Careers, Fame & Following

Per direction: gladiators build real, persistent careers, exactly like *Free Cities*'s own named-fighter approach. A surviving gladiator accumulates Fame (§2) at a rate few other pursuits can match — the single richest source of it in the whole project — which in turn scales ticket draw, wagering interest, and the Dignitas payout (§11) whenever they perform. Survived bouts feed real Reactive Traits already in the catalog (Battle-Hardened), and Traits' own Combo Title system has real room for an arena-specific entry (flagged in §14). A sufficiently famous gladiator is a genuine asset — a share of wagering income, real prestige for the owning house — and can be manumitted as a legendary reward (Labor & Slavery's manumission mechanics), a freed champion becoming Companion-eligible or a real Dynasty Chronicle figure in their own right.

---

## 4. Beast Hunts & Executions

Per direction to expand game types, two more real, historically attested categories — deliberately kept distinct in both mechanics and tone, since one is a sport and the other is a stark historical reality this project's own content guidelines require treating with purpose rather than spectacle for its own sake.

### 4.1 Venatio — Beast Hunts

A **venator** (hunter) — a real Character, sourced and built exactly like a gladiator (§3.1) but leaning Perceptive/Nimble over raw Physique — fights or hunts an exotic animal drawn directly from Resources & Goods' own Exotic Beasts stock, the Menagerie-Keeper (Companions & Court Positions) supplying and maintaining it beforehand. This resolves through the Combat Resolution Engine exactly like a gladiatorial bout (§3.4), with the beast itself as an Irregular Combatant profile scaled by species — a boar is a real but modest threat, a lion or bear a serious one, an elephant genuinely formidable and correspondingly rare and expensive to stage. **A real economic cost sits underneath the spectacle**: a beast lost in the hunt is a consumed Exotic Beasts good, not a reusable prop, making a Venatio a genuine one-time investment rather than a free show.

### 4.2 Damnatio ad Bestias — Execution by Beasts

A distinct, harsher category worth naming plainly rather than folding into Venatio's sporting frame: a second real Legal & Court §9 capital sentence, alongside execution and *damnatio ad ludum*, in which a condemned prisoner is executed by animal attack as public spectacle. Consistent with this project's own content guidelines ("harsh systems... played straight rather than softened, but described with narrative purpose rather than for shock value"), this is not designed as a sporting contest — there is no meaningful Combat Resolution Engine roll, no real chance for the condemned, and no framing that invites the player to enjoy it as entertainment. Mechanically, it resolves as a direct Legal & Court sentence outcome with a Dignitas and Faction-dependent reception (§11) exactly like any other execution — the spectacle is the *setting* Rome actually used for this sentence, not a new gameplay loop layered on top of it. **Consistent with real Roman legal practice and this project's own Legal Status distinctions (Familia §2.5):** both *damnatio* sentences were historically reserved for the enslaved, foreigners, and lower-status condemned — a full Roman Citizen facing a capital charge faces a different, less theatrical set of sentencing options at Legal & Court's own §9 table, not these two.

---

## 5. Group Battles & Naumachia

Per direction to expand game types further, two ways of scaling gladiatorial combat up past a single Combatant.

### 5.1 Group Battles

A team gladiatorial contest or a historical re-enactment (restaging a famous Roman victory for the crowd, a real attested practice and a nice Dignitas-flavored option in its own right) resolves through Military & Combat's Combat Resolution Engine at full **Squad** scale (that document's §4.1) rather than the single-Combatant mode §3.4 uses — the same engine, genuinely scaled up, not a separate system. This is a natural bridge between individual arena combat and full military resolution, and a real chance for several gladiators to build Fame (§2) from one event rather than only ever fighting solo.

### 5.2 Naumachia — Mock Naval Battles

The rare, spectacular extreme: a real historical practice, a mock naval battle staged in a flooded arena or a purpose-built basin. This is deliberately gated as a genuine rarity rather than an ordinary game option — it requires a significant, dedicated venue investment (a unique upgrade, not a standard Amphitheater feature) and resolves through Military & Combat's naval Fleet mechanics (§4.1 of that doc) at a real, often genuinely lethal scale, historically often fought using condemned prisoners or actual war captives as the combatants rather than trained gladiators. A Naumachia is this document's own Legendary-tier event — reserved for the wealthiest, most ambitious hosts, and a natural Dynasty Chronicle entry in its own right whenever one is actually staged.

---

## 6. Chariot Racing

### 6.1 Racing Factions

Per direction, given real distinct depth: the four historical color factions — **Red** (*Russata*), **White** (*Albata*), **Blue** (*Veneta*), **Green** (*Prasina*) — are real, joinable allegiances. The player's own racing operation aligns with one (fielding entries across multiple factions is rarer and reserved for exceptionally wealthy sponsors). A faction carries its own local following, a light Settlement Demographics-adjacent fan base distinct from formal political Faction (Politics & Patronage §3.1) — flavor-forward rather than a deep sub-simulation of its own.

### 6.2 Charioteers & Horses

Charioteers are Characters exactly like gladiators (§3.1's same lazy-instantiation principle), with their own Martial/Boldness-driven skill and career arc, building Fame (§2) the same way. Horses draw from the Stable (Resources & Goods/Buildings), with their own existing Quality tier (Common/Fine/Exceptional) feeding race performance directly — a genuinely bred, Exceptional-grade team is a real, visible investment distinct from a merely serviceable one.

### 6.3 Resolution

A lighter variant of the Combat Resolution Engine, substituting track conditions and weather (a Natural Disasters §6.17 crossover) for terrain, and horse Quality plus charioteer skill for equipment tier and commander weighting. Outcomes: **Decisive Win**, **Close Win**, **Contested Placement** (a genuine photo-finish, a real, tense outcome distinct from a clean win), **Crash/DNF** (a real historical danger, injuring the charioteer or horses), and, rarely, a **Fatal Crash** — the racing equivalent of Catastrophic Defeat.

---

## 7. Theatrical Performances

### 7.1 Performance Types

Per direction, given real distinct depth and a deliberately non-violent resolution axis: **Tragedy** (high-culture, Traditionalist-coded), **Comedy**, and **Pantomime/Mime** (broadly popular, Popularist-coded, and historically often genuinely risqué or satirical). Each performance type carries its own Faction-dependent reception lean (Politics & Patronage §3.1), directly.

### 7.2 The Whole Troupe, Not Just the Lead

Extending performers-as-Characters beyond the headline name: a lead performer is a full Character exactly like a gladiator or charioteer, but a real troupe's supporting cast is too, at whatever depth the specific production actually calls for — a promising supporting actor can build their own smaller Fame trajectory and eventually be singled out into a lead role in their own right, the same lazy-instantiation and career-building logic §3.1 and §3.5 already establish, simply applied to a different craft.

### 7.3 Resolution — Learning/Diplomacy-Driven Quality

Deliberately combat-free, the real third axis this document needed: a **Performance Quality** score reads the lead performer's Learning and Diplomacy, relevant Traits already built for exactly this (Poet, Playwright, Philosopher, Traits §5.3), the supporting cast's own quality (§7.2), and the hosting venue's own quality (Theatre outranking the cheaper, earlier Odeon). Outcomes are audience-reception tiers — **Triumphant**, **Well-Received**, **Lukewarm**, **Poor**, and **Scandalous** — the last being a real, double-edged possibility: bold political satire can land as a genuine Popularist statement against a Traditionalist target (or the reverse), with real Dignitas swings in either direction depending on who's actually in the audience.

---

## 8. Staging a Game — The Funding & Hosting Loop

The end-to-end loop, and where "a far bigger patronage lever than a generic funded event" actually gets mechanical teeth: an Aedile (Politics & Patronage §5.2) or the Editor Muneris (Companions & Court Positions' City-stage capstone) commits real Economy & Finance funding (Funded Actions §4.3) to stage a game at a specific venue, choosing Match Type (§3.2) or category (Venatio, Group Battle, Naumachia) where relevant, the actual roster (Ludus stock, the player's own fielded Characters per §10, hired troupes for theater), and **Scale** — a modest local exhibition versus a major multi-day spectacle. Scale is the direct multiplier on §11's payoff, the concrete mechanism that makes a real Games investment outweigh an ordinary Funded Action the way the core doc says it should.

---

## 9. Audience Wagering

A real betting mechanic, not flavor text: the player — and, per Characters §8.3, any NPC acting on their own initiative — can wager on a specific outcome (a gladiator to win, a faction to place) before the event resolves, with odds set by the competitors' own known stats, Fame (§2), and Quality (a genuine underdog pays out more). A won wager posts as Economy & Finance Windfall-adjacent income; a lost one, a minor expense. The hosting Editor's own house cut of every wager placed is a real, standing Commerce-adjacent income source for a well-run, well-attended venue.

---

## 10. Fielding Your Own Competitors

Per direction: the player can put forward their own slaves, gladiators, charioteers, or performers as actual competitors rather than only ever watching generic Ludus stock. This is a genuine personal stake — a valuable, named, Fame-carrying asset can die in a Sine Missione match, or become a beloved local celebrity — and a direct wagering angle: betting on your own entry is an informed bet, not a blind one, against the general public's own uninformed odds. **A Famous competitor is also a real liability, not just an asset.** A star gladiator or champion charioteer is exactly the kind of high-value, well-known target Piracy & Banditry's Targeted Contracts (§7.1 of that doc) exist for — a jealous rival with the means and the grudge can pay a Confederation to kidnap, enslave, or kill your own champion out of spite or simple competitive advantage, the same way they could target a household's own family member. Fame cuts both ways.

---

## 11. Dignitas & Politics Payoff

The core doc's own flagged open question, resolved directly: a game's payoff scales with **Scale** (§8's funding level), **Match Type/stakes** (a Sine Missione spectacle, a Naumachia, or a real racing/theater equivalent draws sharply more attention than routine exhibition), **outcome drama** (a close contest or a genuine upset pays more than a predictable rout), and the hosting Editor or Editor Muneris's own skill. This feeds Politics & Patronage's Dignitas directly, reads Faction-dependent reception per performance/match type (§3-7), and feeds Settlement Demographics' already-established Contentment effect — closing that document's own forward reference with an actual formula shape rather than a named-but-uncalculated payoff. **Worth stating directly:** a well-attended, well-received major game is a real public political act, not just a Dignitas transaction — it generates a modest Influence trickle (Politics & Patronage §4.4) the same way that document's own Salutatio does, on the reasoning that being seen hosting a genuinely popular spectacle builds exactly the same kind of standing as holding court well.

---

## 12. Cross-System Integration

- **Characters:** Fame (§2) is a genuine, universal addition to that document's own schema (§13), not fenced off to this document; gladiators, venatores, charioteers, and performers are all Characters via lazy instantiation, not a parallel system.
- **Labor & Slavery:** gladiator sourcing (§3.1) and the *damnatio ad ludum* sentencing tie are fully realized.
- **Legal & Court:** *damnatio ad ludum* and *damnatio ad bestias* (§4.2) are both real, named sentences alongside fine/exile/debt bondage/execution (§9 of that doc), reserved for non-citizens consistent with real Roman legal practice and this project's own Legal Status framework.
- **Military & Combat:** the Combat Resolution Engine, Irregular Combatant type, Squad-scale resolution (§5.1), and Fleet mechanics (§5.2) are all reused wholesale; that document's own open question is resolved here directly.
- **Resources & Goods:** Exotic Beasts stock is directly consumed by Venatio (§4.1) as a real, one-time economic cost.
- **Companions & Court Positions:** the Lanista, Editor, Editor Muneris, and Menagerie-Keeper all get their actual mechanical function rather than just a title and a venue.
- **Politics & Patronage:** the Aedile's funding duty and Faction-dependent reception (§11) are both fully realized; a Famous Character's own political reach benefits directly from §2's Interaction modifier; a well-received major game generates a real Influence trickle (§11) alongside its Dignitas payoff.
- **Piracy & Banditry:** §10 confirms a Famous competitor is a real, named-target liability for that document's own Targeted Contracts mechanic (§7.1) — Fame is a double-edged asset, not a purely positive one.
- **Economy & Finance:** Funded Actions (§8), wagering income/expense (§9), and a venue's standing wagering-cut income (§9) are all concrete contributions.
- **Rival Houses:** §2's Fame-driven visibility boost is a direct, named accelerant to that document's own "Notable Families" pre-contact list (§7).
- **Traits:** Poet/Playwright/Philosopher (§7.3), Gladiator's Heart, Battle-Hardened, and Bloodlust/Squeamish (spectator reception) all find real mechanical use.
- **Settlement Demographics:** the Contentment payoff (§11) closes that document's own forward reference.
- **Dynasty Chronicle:** a legendary gladiator's manumission, a Fatal Crash, a Scandalous performance's political fallout, and any staged Naumachia are all natural milestone material.
- **Natural Disasters (§6.17, future):** weather/track conditions are a real input to §6.3's racing resolution.

---

## 13. Data Model

```
// Addition to Characters' own Character{} schema (§14 of that doc):
// fame — 0-100, universal, generated by Games & Spectacle and many other systems (§2); decays slowly if inactive

// Gladiators, venatores, charioteers, and performers all reuse Characters' Character{} schema directly,
// with one further addition specific to this document:
// competitorRecord — wins/losses/draws, carried forward across appearances

Game {
  gameId, venueId,        // Amphitheater | Circus | Theatre | Odeon | (unique Naumachia venue)
  gameType,             // "gladiatorial" | "venatio" | "groupBattle" | "naumachia" | "racing" | "theatrical"
  scale,               // "local" | "major" — §8, the direct payoff multiplier
  matchType,             // "adDigitum" | "sineMissione" — gladiatorial/groupBattle only
  hostingCharacterId,      // the Editor or Editor Muneris
  competitors: [...],       // Character ids, own-fielded (§10) or Ludus/troupe stock
  outcome,
  dignitasPayoff, contentmentEffect,
}

ExecutionSentence {       // §4.2 — distinct from Game{}; a Legal & Court outcome staged as public spectacle, not a contest
  sentenceId, condemnedCharacterId,
  method,               // "damnatioAdBestias" | "standardExecution"
  dignitasEffect,
}

Wager {
  wagerId, gameId,
  bettorCharacterId,
  wageredOn,
  odds, amountWagered,
  outcome,             // "won" | "lost"
}

RacingFactionAllegiance {
  characterOrHouseholdId,
  faction,              // "red" | "white" | "blue" | "green"
}
```

---

## 14. Open Questions

- **All numeric sizing.** Consistent with this project's convention: the Crowd Verdict formula, Fame's growth/decay curve, and the Scale-to-payoff multiplier are all unsized.
- **Fame's relationship to the Combo Title system.** §3.5 gestures at "real room for an arena-specific Combo Title entry" without actually adding one — left as a small follow-up for Traits' own next revision rather than designed here.
- **Damnatio ad ludum and ad bestias' exact severity tiers.** §3.1 and §4.2 add these as Legal & Court sentences without specifying where each sits relative to exile, debt bondage, or standard execution in severity.
- **Naumachia's actual venue requirement.** §5.2 gestures at "a unique upgrade" without specifying what building or Estate & Settlement stage actually gates it.
- **Multi-faction sponsorship's actual mechanics.** §6.1 flags that an exceptionally wealthy sponsor can rarely field entries across multiple racing factions without detailing how that differs from single-faction sponsorship beyond flavor.
- **Own-competitor death's Dignitas treatment.** §10 establishes real personal stakes for a fielded gladiator's death; whether losing your own named entry carries a distinct Dignitas effect beyond the ordinary game payoff isn't specified.
- **Fame's exact decay rate and interaction with multiple concurrent sources.** §2 establishes Fame decays like Influence when inactive; how it aggregates when a single Character is simultaneously, say, a war hero and a retired gladiator isn't specified.
