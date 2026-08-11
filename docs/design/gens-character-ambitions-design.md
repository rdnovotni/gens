# GENS — System Design: Character Ambitions
*A CK3-style personal, short/medium-term goal system for the individual Character — distinct from household-level strategy (Policies & Edicts' Household Doctrine), distinct from the distant, rare dynasty-level milestone (Politics & Patronage's Cursus Honorum, Military & Combat's Roman military career), and built to give Traits, the Personality Axes, and the Interaction Catalog a real throughline beyond reactive play. This document invents no new resolution engine — an Ambition is a tracker that hangs concrete milestones on systems that already exist. This pass adds Legacy Ambitions (an unfinished goal passing to an heir), a worked illustrative example, a resolved Household Doctrine friction rule, a default re-roll/cooldown answer, and a direct Epithets & Titles completion hook.*

---

## Contents

1. Scope & Role
2. What Is an Ambition
3. The Ambition Catalog
4. Selection & Assignment
5. Milestones & Progress
6. Collision — When Two Characters Want the Same Thing
7. Primary and Minor Ambitions
8. Completion
9. Thwarted and Abandoned Ambitions
10. Legacy — When an Ambition Outlives the Character
11. Friction with Household Doctrine
12. NPC Ambitions & the Living World
13. Illustrative Example
14. Cross-System Integration
15. Data Model
16. Open Questions

---

## 1. Scope & Role

Three systems in this project already sit at different altitudes of "what does someone want," and none of them, on their own, answer the question this document is for:

- **Household Doctrine** (Policies & Edicts §3) is standing, revisable *institutional* identity — what the household has become, not what any one person is chasing right now.
- **The Cursus Honorum** and the **Roman military career** (Politics & Patronage §6, Military & Combat §3.3) are deliberately distant, rare, dynasty-scale peaks — "a household that reaches the Consulship has done something a hundred other gentes across a full campaign never will."
- The **Ambition Condition stat** and the **Boldness/Vengefulness Personality Axes** (Familia §2.3, Characters §5, §8.3) already determine *that* a Character presses for more, and let an NPC initiate an Interaction or a Scheme on their own behalf — but that's an engine with no steering wheel. It explains why a high-Ambition Procurator might act in their own interest (Companions & Court Positions' own flagged open question); it never says *toward what*.

Character Ambitions is the missing middle layer: a **specific, named, trackable personal goal**, scaled to one Character's own adult life rather than a household's institutional arc or a dynasty's multi-generational one. "Become Consul" is Cursus Honorum-scale and stays there. "Get noticed enough by the right sponsor to make a first real run at it" is Ambition-scale. "Marry into House Fabricia" is an Ambition; the multi-generational alliance that marriage buys the house afterward is Rival Houses and Succession's business. This document gives the existing Ambition stat, the Personality Axes, and the Interaction Catalog an actual object to point at — the concrete throughline the core doc's design intent named directly.

**What doesn't move here:** the Ambition Condition stat itself, the Personality Axes, the Interaction Catalog, and the Scheme engine are all Characters-doc property and stay exactly as built. This document doesn't touch their math — it gives them a name to work toward and a milestone list to check off.

---

## 2. What Is an Ambition

An Ambition is a **named goal with a category, a small ordered set of milestones, a completion reward, and a real consequence if it's thwarted or abandoned** — not a new stat, not a new dice-roll, not a parallel Scheme engine. Every milestone resolves through a system this project has already built: winning a specific office, completing a specific Scheme, crossing a Net Worth threshold, landing a specific marriage, holding a role for a term, surviving a specific Event. An Ambition's job is to string 2-4 of those together into something that reads as one coherent personal story with a beginning, a middle, and a real ending — exactly the CK3 shape being drawn from, built entirely out of parts this project already owns.

An Ambition always belongs to exactly one Character and is visible to the player for their own controlled character and for any household member whose record they can inspect; an NPC outside the household pursuing one privately is, per §12, discoverable rather than automatically shown.

---

## 3. The Ambition Catalog

Organized by category. Loosely station-gated per direction: most entries are open to any Character regardless of Legal Status or Social Class (reading differently in flavor and in which systems resolve their milestones), and a smaller marked set is hard-gated to a specific station because the goal is simply impossible otherwise. Not exhaustive to every conceivable personal goal — built to cover real breadth across every category this project's systems already support, the same standard the Interaction Catalog held itself to.

### 3.1 Power & Standing

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Win the Local Magistracy | Build Dignitas to threshold → win a contested election | Politics & Patronage §5 |
| Catch Rome's Eye *(gated: citizen, adequate Social Class)* | Clear the Net Worth gate → clear the Dignitas-with-Rome gate → secure a sponsor | Politics & Patronage §6 |
| Rise Through the Ranks *(gated: currently in a Labor Duty Slot or junior Court Position)* | Demonstrate reliability → earn promotion to Overseer or a Senior Position | Companions & Court Positions §6 |
| Lead the Collegium *(gated: Freedman or Peregrine)* | Build standing within a collegium → win election as Magister or Quinquennalis | Collegia & Guilds §9 |
| Command in the Field *(gated: adequate Martial)* | Serve capably as Praefectus → be sponsored into Roman Service | Military & Combat §3.2-3.3 |

### 3.2 Wealth

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Build a Fortune | Reach a personal Net Worth threshold | Economy & Finance §8 |
| Corner a Trade | Win a Named Competition ladder outright | Business Competition §2, §7 |
| A Ship of One's Own | Save toward and commission a vessel | Private Ships & Shipping Ventures |
| Land, Not Just Denarii | Acquire a specific Property Record | Land Ownership & Real Estate §5 |

### 3.3 Love & Family

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Marry for Love | Build mutual Affection/Attraction to a Love-match threshold → formalize | Romance & Seduction, Familia §5 |
| Marry Into House [X] | Court a specific eligible member of a named house → secure the match | Familia §5, Rival Houses §4.2 |
| Restore the Old Name *(gated: declining prestige, low relative Net Worth)* | Marry a dowry-rich match → rebuild Net Worth toward the old Dignitas | Familia §5, Economy & Finance |
| Raise a Worthy Heir | See a chosen heir reach Adulthood with a specific trait/skill profile intact | Familia §3, Succession & Dynasty §2 |

### 3.4 Vice & Vengeance

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Settle the Score | Identify a specific wrong done → complete a Scheme against the responsible Character | Characters §10 |
| Ruin a Rival | Undermine a named rival's Dignitas or office below a threshold | Characters §9.4/§9.7, Scandal |
| One Great Vice, Indulged | Sustain a specific Lifestyle-adjacent pattern (gambling, drink, excess) without it becoming Scandal-Marked | Traits §5.3, Scandal |

### 3.5 Knowledge, Craft & Piety

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Master a Craft | Acquire a specific Lifestyle trait | Traits §5.3 |
| Commission a Masterwork | Fund and see completed a specific Art Commission | Art & Art Commissions, Masterworks |
| Write One's Name Into History | Author a genuine Work (not a Copy) | Books & Manuscripts |
| Devote a Life to the Gods | Reach a high standing within a specific cult or priesthood | Religion §6.6 |

### 3.6 Freedom *(gated: Enslaved or Freedman only)*

| Ambition | Example Milestones | Resolves Through |
|---|---|---|
| Earn Manumission | Demonstrate sustained loyal service → be freed | Labor & Slavery §8 |
| Buy One's Own Freedom | Accumulate a peculium toward a manumission price | Labor & Slavery §8, Economy & Finance |
| Free a Loved One | Accumulate toward and secure a *specific other* Character's manumission | Labor & Slavery §8 |
| A Good Name, Freed *(gated: Freedman)* | Build enough standing to shed Scandal-Marked or a low-birth stigma | Scandal, Epithets & Titles |

### 3.7 Legacy *(gated: Elder lifecycle stage, or any Character who has already completed one Primary Ambition)*

A quieter, deliberately smaller-milestone category new this pass, for a Character whose adult ambitions are substantially behind them — the "one last thing" register rather than a fresh multi-stage climb. Typically a single milestone, often reusing another category's own destination at reduced scope (see a Retired head's living arrangement, Succession & Dynasty §6.1).

| Ambition | Example Milestone | Resolves Through |
|---|---|---|
| See the Heir Settled | Witness the chosen heir's own marriage or first office before retiring or dying | Familia §5, Succession & Dynasty §2 |
| Make Peace with a Rival | Convert a standing Ancestral Grudge or personal feud into at least neutral standing before the end | Rival Houses §5.2, Interaction Catalog |
| Die Well-Remembered | Reach a specific Dignitas/Memoria threshold intact by the time of death | Ancestor Veneration & Funerary Customs |

---

## 4. Selection & Assignment

**For the player's own controlled Character**, Ambitions are a real, deliberate player choice: a selection screen, gated per §3's rules against that Character's current Legal Status, Social Class, held offices, and traits, presenting the open pool. The player is never forced into one — leaving the Primary Ambition slot empty is legitimate, if quieter.

**For every other Character** (household members and, per §12, NPCs generally), an Ambition auto-generates from exactly the inputs that already exist for this purpose: the Ambition Condition stat sets *whether and how strongly* a Character reaches for one at all (a low-Ambition Character often simply has none active, and that's a legitimate, Content outcome rather than a gap the system needs to fill); the Personality Axes and Traits weight *which category* — high Greed points toward Wealth, high Boldness and Martial skill point toward Power & Standing's military branch, high Vengefulness surfaces Vice & Vengeance, a Devoted or Pious Formative trait points toward §3.5. Current station filters the eligible pool exactly as it does for the player. This reuses §8.3's existing "Characters Act on Their Own" mechanism wholesale — an Ambition just gives that mechanism a specific, checkable object instead of a purely emergent one.

**Re-rolling after Completion, Abandonment, or being Thwarted.** The default is a short, deliberate quiet period rather than an instant replacement — a Character doesn't walk out of one Ambition straight into the next the same month. The length of that quiet period isn't numerically fixed (per this project's standing convention, §16), but its existence is: it's what makes a completed or lost Ambition read as a real event rather than a slot the game is in a hurry to refill. A new Primary is then offered to the player (for their own Character) or auto-generated (for everyone else) once that period lapses, using the same §4 mechanism as the first one.

---

## 5. Milestones & Progress

An Ambition is a small ordered (or, occasionally, unordered) list of 2-4 milestones, each one a concrete, checkable condition already native to some other system — a threshold crossed, an office won, a Scheme resolved, a specific relationship state reached. There is no separate Ambition-specific dice roll and no separate discovery-risk layer: a milestone that happens to route through a Scheme uses that Scheme's own progress and discovery mechanics (Characters §10) untouched; a milestone that routes through a contested election uses Politics & Patronage §5.5 untouched. The Ambition record's own job is only to watch for the moment an underlying system reports the relevant outcome and check the box.

This keeps an Ambition legible at a glance (a short checklist, exactly the CK3 texture being drawn from) while every actual mechanical resolution stays owned by the system that already does it best — full compliance with this project's reuse-over-reinvention principle.

---

## 6. Collision — When Two Characters Want the Same Thing

Rather than layering a new interference/discovery mechanic onto every Ambition, two Ambition records that target the same office, the same marriage candidate, or the same rival's downfall are simply flagged as **Colliding**. The friction this produces is never a new mechanic — it's whichever existing system already governs that contest surfacing it naturally: a contested election (Politics & Patronage §5.5) between two Characters who both hold "Win the Local Magistracy," a courtship race (Familia §5's marriage market) between two Characters who both hold "Marry Into House Fabricia," a Scheme and a counter-Scheme (Characters §10) between a Character pursuing "Settle the Score" and the target who's noticed. A Collision flag is purely informational — it lets the player (and the game's own flavor text) recognize *why* a rival is suddenly contesting this specific seat or courting this specific bride, rather than reading as an unmotivated coincidence.

---

## 7. Primary and Minor Ambitions

Per direction, a Character holds **one Primary Ambition** — the one actually shaping their behavior, their Interaction initiative (§8.3), and their reaction to being thwarted — plus room for a small number of **Minor Ambitions** alongside it: lower-stakes, lower-visibility goals (typically single-milestone, drawn from the same catalog) that don't carry §9's full thwarted-consequence weight if they fail, and don't generate a Collision flag against another Character's Primary. A Minor Ambition completing is a nice moment; a Minor Ambition failing is a shrug. The Primary slot is where the real story — and the real risk — lives.

A completed or abandoned Primary Ambition leaves the slot open for a new one to be chosen or generated (§4); nothing requires a Character to always have one filled.

---

## 8. Completion

Completing a Primary Ambition is a genuine moment, scaled to match: a real Dignitas, Favor, or Loyalty gain appropriate to the category (Wealth Ambitions read against Net Worth and Greed satisfaction; Power Ambitions against Dignitas; Love Ambitions against personal Happiness and the relevant relationship-web bond), a Combo Title eligibility check (Characters §6 — "Freed the Slaver's Debt" or an equivalent earned title is exactly the kind of curated entry this rewards), and a guaranteed Dynasty Chronicle (§6.11) entry at a tier matching the Ambition's own stakes — a completed "Buy One's Own Freedom" is a real, human, chronicle-worthy beat in a way a completed "One Great Vice, Indulged" plainly isn't, and the Chronicle's own significance scale is the right place to hold that distinction, not this document.

A sufficiently significant completion is also a natural, direct hook into **Epithets, Nicknames & Titles** — "the Freedman," "the Matchmaker," "the Shipwright" are exactly the register that document's own earned-epithet mechanic already covers, and a completed Ambition (rather than an arbitrary accumulation of unrelated actions) is one of the cleanest, most legible triggers that system could ask for.

Completing an Ambition also typically nudges the underlying Ambition Condition stat itself downward, at least temporarily — a satisfied Character is, briefly, a more Content one, consistent with what that stat already represents.

---

## 9. Thwarted and Abandoned Ambitions

A Primary Ambition can end three ways, and only one of them is quiet:

- **Completed** — §8.
- **Abandoned** — the player (for their own Character) or a shifting circumstance (an NPC's station changing enough that the Ambition no longer makes sense) simply drops it. No penalty; a Character free to want something else next.
- **Thwarted** — a milestone becomes provably, permanently unreachable: the marriage candidate weds someone else, the office is lost to a rival's own Collision (§6), the Scheme at the Ambition's center is Discovered-and-Foiled with no recovery path. This is the consequential outcome, and it routes directly into Traits' own Reactive category rather than inventing a new one: a thwarted Ambition is exactly the kind of "response to treatment" event that already produces Resentful, Bitter, Vengeful, or — where the Ambition was Freedom-category and the blocker was the Character's own owner — Defiant. Which Reactive trait actually lands depends on the existing Axes already in play (a high-Vengefulness Character thwarted in "Settle the Score" is a natural Vengeful/Bitter roll; a high-Rationality one is more plausibly Serene about it).

Critically, a severely thwarted Ambition is itself a legitimate trigger for the Character to initiate a fresh Scheme or Interaction against whoever's responsible (§8.3's existing mechanism, simply pointed at a fresh, well-motivated target) — the player's own household should genuinely feel the aftershock of having blocked a Companion's, a client's, or a rival's Ambition, not just watch a quiet stat tick down.

---

## 10. Legacy — When an Ambition Outlives the Character

New this pass, and a direct, deliberate expression of Design Pillar #7 ("memory has weight"): when a Character dies with an unfinished Primary Ambition still active, that Ambition doesn't simply vanish along with them. It becomes available — never mandatory — as a **Legacy Ambition** option (§3.7) the designated heir (Succession & Dynasty §2) can choose to formally take up, with its milestones and any progress already made carried forward intact rather than restarted from zero. A father who died mid-Scheme to "Settle the Score" against a named rival, or a mother who never quite finished "Restore the Old Name," is exactly the kind of thread a dynasty game should let a descendant knowingly pick back up.

Taking up a parent's or predecessor's Legacy Ambition is itself a small, real character beat worth surfacing: a Dutiful or Filial heir is a natural fit for the offer; a Rebellious or Wayward one plausibly refuses it outright even when the underlying goal would otherwise suit them well, precisely because it isn't theirs. Declining a Legacy Ambition carries no mechanical penalty of its own — it simply closes that thread for good, which is itself a real, Chronicle-worthy outcome ("the ambition died with him") rather than a default the game quietly expects the player to accept.

---

## 11. Friction with Household Doctrine

A Character's Primary Ambition and the household's own standing Doctrine (Policies & Edicts §3) are allowed to genuinely disagree, and this document resolves that tension directly rather than leaving the two systems to silently coexist: no automatic penalty or bonus applies in either direction, but a real mismatch — a Martial-leaning heir's "Command in the Field" Ambition inside an otherwise Mercantile-Doctrine household, say — is exactly the kind of thing the Dutiful/Rebellious and Filial/Wayward Formative traits (Traits §5.1) already exist to color, and a legitimate standing prompt for the current head to intervene through the ordinary Interaction Catalog (a Rebuke, a Mediate, or simply a Confide) rather than through any Ambition-specific mechanic of its own. A household doesn't need to resolve this tension for the game to keep functioning — a house can simply carry a known, felt disagreement between what it stands for and what one of its own members privately wants, and that disagreement is itself real texture rather than an error state.

---

## 12. NPC Ambitions & the Living World

This is where the system pays for itself well beyond the player's own household. Rival Houses §4.1 already established that a house's behavior is "driven by exactly the mechanism Characters §8.3 already built for any Character acting on their own initiative" — a House of Note's own Head now has a real, specific, checkable Primary Ambition rather than a purely emergent behavioral lean, giving Rival Houses' background maneuvering concrete shape ("Gens Sergiana's Head is pursuing Restore the Old Name" is a legible, trackable story rather than flavor text). The same applies one level down to Companions & Court Positions' own flagged worst case: a high-Ambition, low-Honor, Opportunistic Procurator "acting in their own interest" is no longer just a named risk — it's now specifically *which* Ambition (very plausibly Build a Fortune, pursued quietly at the estate's expense) they're privately running, giving that document's own open question its first real mechanical answer.

An NPC's Ambition is not automatically visible to the player. Discovering one — through Gossip, Espionage's Information Network (§6.15), a confidant's Confide Interaction, or simply watching a pattern of behavior long enough to infer it — is itself a small, real payoff, consistent with this project's standing preference for a legible-but-not-omniscient living world.

---

## 13. Illustrative Example

*(A worked arc across a single lifetime, texture only — no numbers implied.)*

> **Marcus, a Companion serving as Institor** — Ambition stat rising through his thirties, Personality Axes reading high Greed, moderate Boldness, low Honor.
>
> Auto-generated Primary Ambition: **Build a Fortune** (Wealth). Milestones: (1) accumulate a personal peculium above a modest threshold, (2) reach a substantial personal Net Worth threshold independent of the household's own treasury.
>
> Marcus quietly begins skimming margin on a trade route he manages (§12's own worst-case Companions & Court Positions scenario, now legible as *this specific Ambition* rather than an unexplained risk). Milestone 1 completes. The player, alerted by an Argentarius's routine audit, confronts him directly (a Coercive Interaction) rather than dismissing him outright.
>
> Marcus's Ambition is now **Thwarted** — the skimming route is closed, and Milestone 2 is no longer reachable through his current position. Per §9, this rolls into a Reactive trait: given his low Honor and moderate Boldness, he lands **Bitter** rather than **Resentful**. The player's mercy in not dismissing him becomes a live, ongoing question rather than a closed one — a Bitter Companion who still holds his post is exactly the kind of "legible, predictable risk" this document exists to produce.
>
> Fifteen years later, Marcus dies still holding a smaller Minor Ambition, unresolved. Nothing carries forward — Legacy Ambitions (§10) apply only to a Primary, and his had already ended. His Dynasty Chronicle entry reads as a real, specific man's story, not a generic "a Companion died" line.

---

## 14. Cross-System Integration

- **Characters:** the Ambition Condition stat, the Personality Axes, the Interaction Catalog, and the Scheme engine are this document's entire foundation, reused wholesale and untouched; §8.3's NPC-initiative mechanism finally gets a concrete object to aim at.
- **Traits:** a thwarted Ambition (§9) is a first-class, well-motivated source for the Reactive category (Resentful, Bitter, Vengeful, Defiant); a completed Freedom-category Ambition is a natural trigger for Freed Spirit or Institutionalized (§6.7 of that doc); Dutiful/Rebellious and Filial/Wayward now have a concrete flashpoint in §11.
- **Companions & Court Positions:** resolves that document's own flagged "Procurator acting in their own interest" open question directly — that behavior now has a specific, named Ambition driving it.
- **Rival Houses:** gives §4.1's Head-level "Ambition drives behavior" mechanism a concrete, trackable goal instead of a purely emergent lean; Collision (§6) is this document's own mechanism behind a rival house suddenly, legibly contesting a specific seat, marriage, or acquisition.
- **Politics & Patronage:** the Cursus Honorum and local magistracy ladder are this document's most common Power & Standing milestone destinations; §6's *novus homo* story is a natural full-arc Ambition sequence (several Ambitions in succession across a single lifetime) rather than a single one.
- **Military & Combat:** the Roman military career (§3.3 of that doc) is this document's gated Power & Standing entry for a sufficiently Martial Character.
- **Labor & Slavery:** the Freedom category (§3.6) is this document's concrete personal-stakes expression of that system's own manumission path.
- **Familia:** Love & Family Ambitions (§3.3) run entirely on that document's existing marriage-market and lifecycle mechanics; a completed "Raise a Worthy Heir" is a natural bridge into Succession & Dynasty's own heir-designation moment.
- **Succession & Dynasty:** a passed-over or Rebellious heir's own thwarted Ambition (very plausibly Power & Standing or a specific inheritance-adjacent goal) is a natural, well-motivated additional trigger feeding §5.1's Succession Drama conditions; §6.1's Elder Statesman retirement state is this document's natural home for a Legacy Ambition (§3.7, §10) to be picked up or offered.
- **Scandal:** Vice & Vengeance Ambitions (§3.4) run directly against that system's shared aftermath engine; "One Great Vice, Indulged" is this document's own explicit tightrope between a Lifestyle trait and a Scandal-Marked outcome.
- **Collegia & Guilds:** Lead the Collegium (§3.1) is this document's concrete personal-stakes hook into that system's own "second prestige ladder" (§9 of that doc).
- **Epithets, Nicknames & Titles:** §8's completion hook is a clean, legible trigger source for that system's own earned-epithet mechanic.
- **Ancestor Veneration & Funerary Customs:** "Die Well-Remembered" (§3.7) reads directly against that document's Memoria axis.
- **Policies & Edicts:** §11 resolves the relationship between a personal Ambition and standing Household Doctrine directly — real, felt disagreement rather than either automatic penalty or silent non-interaction.
- **Espionage (§6.15, future):** a discovered rival's Ambition (§12) is exactly the kind of intelligence that system's Information Network is built to surface.
- **Dynasty Chronicle (§6.11, future):** every completed (and many thwarted) Primary Ambitions are guaranteed, tier-scaled Chronicle material; Legacy Ambitions (§10) taken up or knowingly declined are their own distinct, generational-thread Chronicle beat — a dynasty's own history read back as a string of personal Ambitions won, lost, and inherited is squarely Design Pillar #7's "memory has weight" in practice.

---

## 15. Data Model

```
Ambition {
  ambitionId,
  characterId,
  category,           // "power" | "wealth" | "love" | "vice" | "knowledge" | "freedom" | "legacy"
  catalogEntryId,      // which named Ambition from §3
  slot,                // "primary" | "minor"
  milestones: [
    {
      milestoneId,
      description,
      resolvesVia,       // pointer to the owning system's own record type
                          // e.g. "politicsPatronage.election", "characters.scheme",
                          // "economyFinance.netWorthThreshold", "familia.marriage"
      complete: bool,
    }
  ],
  status,               // "active" | "completed" | "abandoned" | "thwarted"
  collidesWithAmbitionId,  // nullable — §6
  generatedBy,          // "playerChoice" | "auto" — §4
  inheritedFromAmbitionId, // nullable — §10, set when taken up as a Legacy Ambition
  startedAtMonth,
  resolvedAtMonth,
}

AmbitionThwartedOutcome {   // §9
  ambitionId,
  causeCharacterId,          // nullable — who/what specifically blocked it, if identifiable
  resultingReactiveTrait,     // nullable
  triggeredRetaliation: bool,  // did it spawn a fresh Interaction/Scheme per §8.3
}

LegacyAmbitionOffer {   // §10
  deceasedCharacterId,
  originalAmbitionId,
  offeredToHeirId,
  heirResponse,          // "accepted" | "declined" | "pending"
}
```

---

## 16. Open Questions

- **All numeric sizing**, per this project's standing convention — Dignitas/Favor/Loyalty completion rewards, the Minor Ambition slot count, the re-roll cooldown's actual length (§4), and Ambition-generation weighting against the Condition stat and Axes are all unsized.
- **Minor Ambition slot count.** §7 states "a small number" without fixing it at two, three, or scaling by some other factor (Ambition stat height, Intellect tier).
- **Player override of an NPC's auto-generated Ambition.** Whether the player can ever directly set or veto a household member's Ambition (a parent forbidding a child's chosen path, distinct from §11's after-the-fact friction) rather than only reacting to it isn't specified — a natural, dramatic hook if built, but not required for the system to function.
- **Catalog completeness.** §3's tables are broad but not exhaustive; a future pass could add entries as new systems (Espionage, Legal & Court) come online, the same way the Interaction Catalog expects to grow.
- **Ambition visibility threshold for NPCs.** §12 establishes that discovery is real and mechanism-driven, but not the specific threshold (a single successful Confide, a sustained Gossip pattern, a flat Intrigue-vs-Intrigue check) that actually reveals one.
- **Legacy Ambition eligibility limit.** §10 doesn't specify whether an heir can only ever take up their immediate predecessor's Legacy Ambition, or whether an even older, still-unclaimed one further back in the Dynasty Chronicle could theoretically be revived — left open as a judgment call for however deep the Chronicle's own lookup ends up being practical to support.
