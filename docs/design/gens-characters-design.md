# GENS — System Design: Characters
*The universal framework underneath everyone with a name — family, slaves, freedmen, clients, companions, court-position holders, fellow Decurions, rivals, travel encounters, event guests, all of it. This document formalizes what Familia's stat architecture and Politics & Patronage's "Notable" tier were both reaching for, retires the Notable/full-record split, and builds the trait catalog, the hidden personality layer, and the interaction system everything else plugs into. This pass adds two new trait pairs, a stated typical trait load, Group & Public Interactions, several missing verbs (Betray, Threaten, Mediate, Propose Adoption), and resolves Combo Title collision handling and age-appropriate backfill for lazily-instantiated adults.*

---

## Contents

1. Scope & Role
2. What Is a Character
3. Attributes, Skills & Condition — Now Universal
4. Personality Traits — The Full Catalog
5. Personality Axes — The Hidden Behavioral Layer
6. Combo Titles — The Headline Description
7. The Relationship Web, Extended
8. How Characters Act — Mechanical and Narrative Resolution
9. The Interaction Catalog
10. Multi-Stage Schemes — Progress, Discovery, Counter-Play
11. Generation & Lazy Instantiation
12. Retiring the Notable Tier — Migration Note
13. Cross-System Integration
14. Data Model
15. Open Questions

---

## 1. Scope & Role

Familia (§6.1) already built the stat architecture — Core Attributes, Labor Skills, Condition Stats, Legal Status, Personality Traits, the Relationship Web — and stated plainly that it's "the schema they all share, not a separate silo." Politics & Patronage then needed a lighter version of the same idea for clients and rivals and called it a **Notable**, explicitly lighter than a full Familia record for practicality's sake. Per direction, this document collapses that distinction: there is no more lightweight tier. Every named individual in the game — a family member, a slave, a freedman, a client, a companion, a Decurion colleague, a rival candidate, a stranger met on the road, a guest at a dinner — is a **Character**, and every Character gets the complete schema, immediately, from the moment they're first named.

This document is also where the project's CK3-derived ambition gets its actual mechanism: traits and a deeper hidden personality layer that make a character's behavior legible and consistent, a combo-title system for quick reads, and — the piece nothing else in this project has built yet — a real, wide **Interaction Catalog** covering everything from a compliment to an assassination, resolved through a shared engine rather than each future system (Romance & Seduction, Espionage, Legal & Court, Rival Houses) inventing its own.

**What doesn't move here:** Familia remains the authority on lifecycle, birth, aging, marriage, and legitimacy; this document doesn't re-litigate any of that. It *is* the authority, going forward, on traits, the hidden personality layer, combo titles, and how any character — named anywhere, by any system — actually behaves and can be interacted with.

---

## 2. What Is a Character

A **Character** is any specifically named individual the game tracks — full stop, no sub-tiers. Characters arrive from every direction:

- **Familia** — family members, enslaved household members, freedmen, clients, companions.
- **Companions & Court Positions** — every Overseer, Senior Position holder, and appointee is a Character first, a position second.
- **Politics & Patronage** — fellow Decurions, rival candidates, Clientela members drawn from the Curiales pool (formerly "Notables" — see §12).
- **Rival Houses (§6.10, future)** — any member of another gens the player's household actually interacts with.
- **Travel (§6.18), Events (§6.8), Games & Spectacle (§6.22)** — anyone specifically encountered rather than described in passing.
- **Guests** — anyone hosted at a Salutatio, a Triclinium feast, or any other hospitality moment (Villa doc §4.5).

What a Character is *not*: the unnamed background population Settlement Demographics (§6.26) tracks in aggregate. That boundary is unchanged — a Curialis, a Coloni household, an Operarii family stay abstracted right up until the moment the game specifically names one of them, at which point §11's instantiation rule applies and they become a full Character on the spot.

---

## 3. Attributes, Skills & Condition — Now Universal

Restated here as explicitly universal rather than Familia-specific, since every Character now shares them regardless of source:

- **Core Attributes** (Diplomacy, Martial, Stewardship, Intrigue, Learning) — Familia §2.1, unchanged.
- **Labor Skills** (Fieldwork, Domestic Service, Craft, Culinary, Medicine) — Familia §2.2, unchanged; less relevant to a rival Senator than a household slave, but present on every record for the edge cases where it matters (a captured rival assigned to a duty slot).
- **Condition Stats** (Health, Fatigue, Loyalty, Ambition, Fertility) — Familia §2.3, unchanged.
- **Legal Status & Social Class** — Familia §2.5, unchanged.
- **Detailed/Body Attributes** — Familia §2.4, unchanged, feeding Appearance the same way for every Character, not just household members.

Nothing here is new; the point is simply that a rival candidate generated by Politics & Patronage no longer gets a stripped-down version of this list. They get all of it.

---

## 4. Personality Traits — The Full Catalog

Same three categories Familia §2.6 established — **Congenital** (rolled at birth, inheritance-weighted), **Formative** (Childhood/Adolescence, upbringing- and Education-driven), **Reactive** (adulthood, treatment- and event-driven) — now enumerated in full rather than sketched by example. Traits are tags, mostly organized in mutually-exclusive opposed pairs (can't hold both sides), grouped below by theme for readability. Each trait nudges one or more Personality Axes (§5) and may carry its own direct bespoke effect beyond that nudge.

**Typical load, not the full pool.** No Character holds anywhere near all 115 — a person is legible precisely because they hold a *handful*. As a working guideline: 2-4 Congenital traits (rolled once, young), 1-3 Formative (accumulated through Adolescence), and 0-5 Reactive (starting at zero in Adulthood and only ever growing, never handed out pre-emptively) — a believable elder Character might carry 8-10 traits total across all three categories, a young adult closer to 4-6. This keeps any single Character reading as a specific person with real edges rather than a checklist of every tag their category permits.

### 4.1 Congenital (36)

*Body & Vigor*

| Trait | Opposed | Note |
|---|---|---|
| Strong | Weak | Labor Skill/Martial ceiling |
| Weak | Strong | |
| Hardy | Sickly | Disease resistance |
| Sickly | Hardy | Disease vulnerability |
| Beautiful | Plain | Appearance-linked; Romance/marriage-market draw |
| Plain | Beautiful | |
| Fecund | Barren | Fertility Condition stat modifier |

*Wits & Temperament*

| Trait | Opposed | Note |
|---|---|---|
| Quick | Slow | Learning-adjacent, quick to react in schemes/negotiation |
| Slow | Quick | |
| Perceptive | Oblivious | Discovery-risk reader (§10) |
| Oblivious | Perceptive | |
| Calm | Wrathful | Slow to anger |
| Wrathful | Calm | Fast to anger, feud-prone |
| Patient | Impatient | |
| Impatient | Patient | |
| Bold | Cautious | Scheme/election initiation |
| Cautious | Bold | |
| Gregarious | Reserved | Salutatio/Clientela reach |
| Reserved | Gregarious | |
| Proud | Humble | Dignitas sensitivity |
| Humble | Proud | |
| Stubborn | Pliant | Negotiation resistance |
| Pliant | Stubborn | |

*Virtue & Vice (innate)*

| Trait | Opposed | Note |
|---|---|---|
| Honest | Deceitful | Honor axis |
| Deceitful | Honest | Honor axis; Intrigue-adjacent |
| Generous | Greedy | Greed axis |
| Greedy | Generous | Greed axis; bribe/poach susceptibility |
| Compassionate | Callous | Compassion axis |
| Callous | Compassionate | Compassion axis |
| Diligent | Slothful | Stewardship-adjacent |
| Slothful | Diligent | |
| Lustful | Chaste | Romance initiation odds |
| Chaste | Lustful | |
| Temperate | Gluttonous | Health/upkeep-adjacent |
| Gluttonous | Temperate | |

### 4.2 Formative (36)

*Belief & Worldview*

| Trait | Opposed | Note |
|---|---|---|
| Zealous | Impious | Zealotry axis; canon example |
| Impious | Zealous | Zealotry axis |
| Rational | Superstitious | Event-response flavor |
| Superstitious | Rational | |
| Idealistic | Pragmatic | Scheme/negotiation flavor |
| Pragmatic | Idealistic | |
| Honor-Bound | Opportunistic | Honor axis |
| Opportunistic | Honor-Bound | Honor axis |
| Cosmopolitan | Xenophobic | Reputation Duality local-standing axis, Diplomacy with Non-Roman Peoples |
| Xenophobic | Cosmopolitan | Same, in reverse |

*Upbringing & Discipline*

| Trait | Opposed | Note |
|---|---|---|
| Trusting | Cynical | Canon example; deception vulnerability |
| Cynical | Trusting | Harder to deceive, harder to befriend |
| Studious | Incurious | Education & Culture's hook |
| Incurious | Studious | |
| Disciplined | Undisciplined | |
| Undisciplined | Disciplined | |
| Dutiful | Wayward | Succession-drama-adjacent |
| Wayward | Dutiful | |
| Filial | Rebellious | Parent/child opinion baseline |
| Rebellious | Filial | |

*Refinement & Culture*

| Trait | Opposed | Note |
|---|---|---|
| Eloquent | Tongue-Tied | Diplomacy-adjacent, negotiation |
| Tongue-Tied | Eloquent | |
| Refined | Coarse | Dignitas-adjacent social reception |
| Coarse | Refined | |
| Frugal | Extravagant | Economy & Finance-adjacent |
| Extravagant | Frugal | |
| Well-Traveled | Provincial | Travel/Diplomacy-adjacent |
| Provincial | Well-Traveled | |

*Disposition Toward Others*

| Trait | Opposed | Note |
|---|---|---|
| Charitable | Mercenary | Clientela/favor generosity |
| Mercenary | Charitable | Transactional worldview |
| Loyal-Hearted | Fickle | Direct nudge to Loyalty Condition stat |
| Fickle | Loyal-Hearted | Direct nudge to Loyalty Condition stat |
| Contentious | Amicable | Curia/scheme friction baseline |
| Amicable | Contentious | |
| Martial-Minded | Peace-Loving | Military & Combat/Games affinity |
| Peace-Loving | Martial-Minded | |

### 4.3 Reactive (43)

*Response to Treatment*

| Trait | Opposed | Note |
|---|---|---|
| Content | Resentful | Canon example |
| Resentful | Content | Canon example; Labor & Slavery's main hook |
| Grateful | Vengeful | Canon example |
| Vengeful | Grateful | Canon example; Vengefulness axis |
| Devoted | Estranged | Strong bond from sustained good/poor treatment |
| Estranged | Devoted | |
| Bitter | Forgiving | Vengefulness axis |
| Forgiving | Bitter | Vengefulness axis |
| Emboldened | Cowed | Direct product of reward/punishment history |
| Cowed | Emboldened | |
| Defiant | Broken | Labor & Slavery's punishment-ladder endpoint |
| Broken | Defiant | Severe/Lethal punishment aftermath |

*Trauma & Resilience*

| Trait | Opposed | Note |
|---|---|---|
| Traumatized | Resilient | Violence/loss aftermath |
| Resilient | Traumatized | |
| Battle-Hardened | Shell-Shocked | Military & Combat aftermath |
| Shell-Shocked | Battle-Hardened | |
| Paranoid | Serene | Post-scheme-discovery or betrayal aftermath |
| Serene | Paranoid | |
| Haunted | — | By a specific, named past death or loss; standalone |
| Grieving | — | Fresh loss, typically time-limited; standalone |
| Addled | — | Age/injury/disease-driven cognitive decline; standalone |
| Scarred | — | Permanent injury's visible mark (Familia §3.1); standalone |

*Vice & Corruption (acquired)*

| Trait | Opposed | Note |
|---|---|---|
| Drunkard | Abstemious | Health/reliability cost |
| Abstemious | Drunkard | |
| Corrupt | Incorruptible | Office-holding temptation, Politics & Patronage |
| Incorruptible | Corrupt | |
| Complacent | Driven | Post-success/failure drift |
| Driven | Complacent | |
| Envious | Magnanimous | Rival-comparison-driven |
| Magnanimous | Envious | |
| Ruthless | Merciful | *Chosen* hardening through command/authority — distinct from innate Callous |
| Merciful | Ruthless | |
| Feral | — | Extreme neglect/Bare Regimen aftermath; standalone, rare |
| Fanatical | Disaffected | Radicalization or disillusionment from a cause |
| Disaffected | Fanatical | |

*Romance-Specific*

| Trait | Opposed | Note |
|---|---|---|
| Heartbroken | Guarded | Post-affair/divorce aftermath |
| Guarded | Heartbroken | |
| Infatuated | Disillusioned | Active courtship state vs. its collapse |
| Disillusioned | Infatuated | |
| Faithful | Adulterous | Acquired from actual marital conduct, not innate Chaste/Lustful; Honor axis; feeds Familia's affair/legitimacy mechanics directly |
| Adulterous | Faithful | Same, in reverse |

*War-Specific*

| Trait | Opposed | Note |
|---|---|---|
| Warmonger | War-Weary | Sustained campaign exposure |
| War-Weary | Warmonger | |

**Total: 115 traits**, comfortably past the 100+ target. Exclusivity is enforced only within a listed pair; a Character can otherwise hold any combination across categories (a Callous-Congenital, Zealous-Formative, Corrupt-Reactive individual is a perfectly coherent, if unpleasant, combination). Note that Lustful/Chaste (§4.1, an innate appetite) and Faithful/Adulterous (§4.3, an acquired record of actual conduct) are deliberately separate axes of the same general territory — a Chaste-Congenital character can still become Adulterous-Reactive under the right pressure, and a Lustful one can remain perfectly Faithful; appetite and behavior aren't the same trait wearing two names.

### 4.4 Trait Acquisition & Loss

Congenital traits roll once at birth. Formative traits roll or get selected during Childhood/Adolescence per Familia §2.6's existing rule. Reactive traits are the only category that changes during adulthood — gained or lost based on sustained treatment (Labor & Slavery's Regimen, Loyalty trends), major events (a betrayal discovered, a scheme survived, a war fought, a love affair ended), or office-holding (Corrupt/Incorruptible). A Reactive trait's opposed pair can flip (Content → Resentful) if treatment reverses sharply enough and for long enough — this isn't a one-way ratchet.

---

## 5. Personality Axes — The Hidden Behavioral Layer

Per the decision to build something more advanced than a simple trait-to-behavior lookup: seven numeric axes (-100 to 100, 0 neutral), hidden from ordinary UI the way CK3's own AI weights are, that every trait nudges by a small weighted amount and that all mechanical resolution (§8) reads uniformly:

| Axis | Pole A ↔ Pole B | Feeds |
|---|---|---|
| **Honor** | Honorable ↔ Treacherous | Contract/oath reliability, Legal & Court behavior, betrayal odds |
| **Compassion** | Compassionate ↔ Callous | Punishment/Regimen reactions, mercy in rulings, Sumptuary/Tax Policy reception |
| **Greed** | Greedy ↔ Content | Bribability, poaching susceptibility, corruption uptake |
| **Zealotry** | Zealous ↔ Rational | Religious/event participation, fanatic scheme behavior, Sumptuary Edict reception |
| **Vengefulness** | Vengeful ↔ Forgiving | Grudge duration, retaliation odds, feud escalation |
| **Boldness** | Bold ↔ Cautious | Scheme/election initiation, risk tolerance, duel acceptance |
| **Rationality** | Rational ↔ Impulsive | Predictability of self-interested behavior — the axis Rival Houses' own AI will lean on hardest |

**This is the "advanced hybrid" specifically:** axes answer the big yes/no behavioral questions (does this person betray, accept a bribe, forgive a slight, initiate a scheme) through one small, consistent set of numbers — but individual traits *also* carry their own bespoke direct effects independent of the axes (Eloquent adds directly to negotiation resolution; Loyal-Hearted nudges the Loyalty Condition stat directly; Fecund modifies Fertility directly). Axes and bespoke effects aren't competing systems — axes handle the general "how would this person react," bespoke effects handle the specific "this trait does this one concrete thing," and a character's full behavior is always the sum of both.

Axis values drift slowly over time based on sustained Reactive-trait-driving experience, the same way Reactive traits themselves shift — a character who spends a decade being betrayed and betraying in turn will show it in their Honor and Vengefulness axes even before any specific new trait is gained.

---

## 6. Combo Titles — The Headline Description

Per the decision to curate the most iconic pairs and let everything else generate dynamically: a short list of designed, Roman-flavored titles for specific trait pairings that carry real narrative punch, functioning exactly like CK3's "Treacherous Villain" — a headline description shown alongside a Character's name and portrait.

| Trait 1 | Trait 2 | Title |
|---|---|---|
| Deceitful | Zealous | Pious Fraud |
| Compassionate | Impious | Godless Altruist |
| Callous | Zealous | Zealous Tyrant |
| Bold | Deceitful | Treacherous Hero |
| Vengeful | Eloquent | Silver-Tongued Avenger |
| Greedy | Zealous | Temple Grifter |
| Cautious | Deceitful | Cowardly Schemer |
| Honest | Bold | Forthright Champion |
| Compassionate | Diligent | Devoted Caregiver |
| Wrathful | Vengeful | Undying Grudge-Bearer |
| Generous | Gregarious | Beloved Patron |
| Greedy | Corrupt | Venal Magistrate |
| Ruthless | Disciplined | Iron Hand |
| Cynical | Eloquent | Honeyed Viper |

**Fallback rule:** where a Character's held traits don't match a curated pair, a headline still generates — dynamically, from whichever two or three traits currently read as most narratively prominent (the most recently acquired Reactive trait, weighted alongside the strongest-magnitude Axis) — however the game's narrative/description layer actually renders text elsewhere. This document doesn't specify that rendering mechanism; it only guarantees every Character has *something* to show, curated or not, and that the curated list stays small and deliberate rather than trying to enumerate every possible pairing by hand.

**Collision handling:** the table above is checked top-to-bottom; the first pair a Character matches wins, and a Character matching more than one curated pair simply shows the highest one in the list — the table is ordered roughly by narrative distinctiveness, not by category or alphabet, so this is a deliberate priority order rather than an arbitrary one. **Recalculation:** a Combo Title isn't fixed at generation — since Reactive traits (§4.4) can flip and Congenital/Formative traits never change, a title is recomputed whenever the Reactive trait set changes meaningfully, meaning the headline a player sees can genuinely shift over a Character's life (a Compassionate, Zealous priest who's beaten down into Broken and Corrupt over a hard decade stops reading as a "Devoted Caregiver" and starts reading as something considerably darker).

---

## 7. The Relationship Web, Extended

Familia §2.7's model — opinion (-100 to 100) plus bond tags, between any two named individuals — is unchanged and, per this document, explicitly universal rather than Familia-scoped. A small set of bond tags worth naming now that they have real mechanical homes elsewhere in the project:

- **Nemesis** — an escalated Rival, past the point of ordinary political friction (Politics & Patronage §9).
- **Debtor / Creditor** — the relationship-web mirror of an active Economy & Finance `DebtRecord`.
- **Co-Magistrate** — the Duumvir colleague relationship (Politics & Patronage §5.4).
- **Blackmail Leverage** — one party holds damaging material on the other (Espionage's forward hook), distinct from ordinary Rival friction because it's a standing, usable threat rather than just poor opinion.

Existing tags (Friend, Rival, Lover, Patron/Client, Mentor/Student, Contubernium, family bonds) are unchanged.

---

## 8. How Characters Act — Mechanical and Narrative Resolution

Every Character's behavior resolves through one of two layers, both reading the same underlying record so neither ever contradicts the other:

### 8.1 Mechanical Resolution

For well-defined, checkable interactions — an election (Politics & Patronage §5.5), a loan default (Economy & Finance §6.3-6.4), a scheme's monthly progress tick (§10) — a weighted formula combines the relevant Core Attribute, the relevant Personality Axis or axes, current relationship opinion, and any situational modifier (Influence spent, a client's favor, a bribe offered) into a probability or magnitude. This is the layer every existing system in this project already uses; nothing about it changes.

### 8.2 Narrative Resolution

For open-ended moments — a real conversation, a confrontation, a negotiation with room for genuine back-and-forth — a Character's full record (traits, axes, relationship history with the player, recent relevant events) compiles into a compact **character brief**, and whatever narrates the scene (the game's AI-driven dialogue/narration layer) reads that brief to keep the character's words, resistance, and concessions consistent with who they actually are. This is the direct mechanism behind the project's stated goal: a Treacherous, Greedy character doesn't narratively fold to a heartfelt appeal the way a Loyal-Hearted, Compassionate one might, even in a scene too open-ended for a single dice-roll to cover. This document doesn't specify the narration technology itself — only that the brief it hands to that layer is always generated from the same Character record the mechanical layer reads, so the two never disagree about who someone is.

### 8.3 Characters Act on Their Own

Ambition (Condition stat) plus the Boldness and Vengefulness axes let any Character — not just ones the player targets — *initiate* an interaction: a high-Ambition, high-Boldness Notable-turned-Character schemes for an office without the player triggering it; a Vengeful client who was slighted a season ago quietly starts working against the player, unprompted. This is the concrete mechanism behind the core doc's "a living world" pillar — NPC-initiated interactions run through this same Interaction Catalog and the same two resolution layers, on the NPC's own behalf, rather than being a separate scripted system.

---

## 9. The Interaction Catalog

Organized by category, each entry noting its typical resolution layer (§8) and primary inputs. Not exhaustive down to every possible dialogue line, but built to cover the real breadth requested rather than a token handful per category.

### 9.1 Social

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Befriend | Quick | Diplomacy, opinion, compatible traits |
| Small Talk / Compliment | Quick | Diplomacy |
| Confide | Quick | Opinion, Honor axis (will they keep it?) |
| Comfort | Quick | Compassion axis |
| Gift-Giving | Quick | Denarii/goods spent, Greed axis |
| Extend Hospitality | Quick | Villa hosting quality (Villa doc §4.5) |
| Mentor | Quick, recurring | Learning, relevant skill transfer |
| Rebuke / Insult | Quick | Diplomacy vs. Pride, opinion damage |
| Apologize / Reconcile | Quick | Honor axis, opinion recovery |
| Introduce | Quick | Broker a new relationship-web link between two others |
| Gossip | Quick | Spread or suppress information, Intrigue |
| Request a Favor | Quick | Clientela favor system (Politics & Patronage §4.2) if a client |
| Console (after a loss) | Quick | Compassion axis |
| Praise / Shame Publicly | Quick | Dignitas-adjacent, public opinion shift |
| Propose Adoption / Fostering | Quick, rare | Familia's adoption mechanic (core doc §6.9 forward reference); same evaluation as a marriage candidate |
| Mediate a Dispute | Quick to Multi-stage | Third-party intervention in an existing conflict between two *other* Characters — the player's concrete lever into §8.3's NPC-on-NPC drama |

### 9.2 Romantic

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Flirt | Quick | Diplomacy, Lustful/Chaste |
| Court / Woo | Multi-stage (light) | Sustained opinion-building |
| Seduce | **Multi-stage** | Diplomacy, target's Chaste/Lustful and Honor axis, discovery risk if either is married |
| Confess Feelings | Quick | Opinion threshold |
| Propose Marriage | Quick | Familia §5's existing arranged/love-match math |
| Break Off Betrothal | Quick | Dignitas cost |
| Take as Lover / End Affair | Quick to initiate, ongoing state | Legitimacy (Familia §5.2) if a child results |
| Spurn / Reject | Quick | Opinion damage, possible Heartbroken trait |
| Elope | Quick, high-consequence | Dignitas, family opinion |
| Arrange Assignation | Quick | Discovery-risk exposure per encounter |

### 9.3 Political

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Endorse / Undermine a Candidate | Quick to Multi-stage | Politics & Patronage §5.5, §9 |
| Request Political Support | Quick | Clientela, Curia opinion (§5.6 of that doc) |
| Broker an Alliance | Multi-stage | Diplomacy, mutual Dignitas |
| Sponsor for Office | Quick, rare | The cursus honorum sponsor mechanic (Politics & Patronage §6) |
| Call In a Favor | Quick | Clientela |
| Threaten Censure / Bring Charges | Quick to Multi-stage | Legal & Court hook |
| Found/Join a Curia Faction Bloc | Multi-stage | Faction axis (Politics & Patronage §3.1) |

### 9.4 Coercive / Intrigue

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Scheme (generic wrapper) | **Multi-stage** | §10 in full |
| Fabricate a Hook / Blackmail Material | **Multi-stage** | Intrigue, target's Perceptive/Oblivious |
| Blackmail / Extort | Quick once material exists | Blackmail Leverage bond (§7) |
| Threaten | Quick | Martial or Intrigue depending on the threat's nature; Boldness axis, target's Cautious/Bold |
| Bribe | Quick | Greed axis, denarii/Influence spent |
| Betray | Quick, high-consequence | Directly resolves an active alliance, Co-Magistrate bond, or standing trust; Honor axis is the primary predictor, and a low roll here is exactly what should have been foreseeable from a Treacherous/Opportunistic Character all along |
| Sabotage | **Multi-stage** | Intrigue, discovery risk |
| Frame | **Multi-stage** | Intrigue, Honor axis of any witnesses |
| Spread a Damaging Rumor | Quick to Multi-stage | Intrigue, target's Dignitas |
| Incite Rebellion/Unrest | Multi-stage | Labor & Slavery's Unrest math |
| Poach a Client | Quick to Multi-stage | Politics & Patronage §4.5 |
| Recruit / Plant a Spy | Multi-stage | Espionage's forward hook |
| Assassinate | **Multi-stage** | Highest discovery risk in the catalog |
| Kidnap / Imprison | Quick to Multi-stage | Martial, Legal exposure |
| Duel | Quick, one-shot | Martial, Honor axis (formal, consensual violence) |

### 9.5 Economic

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Offer / Request a Loan | Quick | Economy & Finance §6 |
| Forgive / Call In a Debt | Quick | Same |
| Offer Employment / a Position | Quick | Companions & Court Positions |
| Dismiss from Position | Quick | Loyalty impact |
| Negotiate a Contract | Quick | Economy & Finance §3.2 |
| Sponsor / Patronize | Quick, recurring | Clientela |
| Manumit | Quick, rare | Labor & Slavery's manumission mechanics |
| Purchase / Sell (Slave Market) | Quick | Labor & Slavery §2-3 |
| Ransom | Quick | Post-kidnap resolution |

### 9.6 Violent

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Assault | Quick | Martial, Legal exposure |
| Execute | Quick, irreversible | Labor & Slavery's Lethal punishment tier, or Legal & Court sentencing |
| Maim / Mutilate | Quick, irreversible | Labor & Slavery's Severe punishment tier |
| Order Flogged | Quick | Labor & Slavery's Moderate tier |
| Defend / Protect / Rescue | Quick | Martial, Bodyguard/retinue involvement |
| Declare a Feud | Multi-stage, standing state | Rival Houses' forward hook |

### 9.7 Informational

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Interrogate | Quick to Multi-stage | Intrigue vs. target's Honor/resolve |
| Gather Intelligence / Spy On | Multi-stage | Espionage's forward hook |
| Share / Withhold Information | Quick | Opinion, Honor axis |
| Reveal a Secret / Expose a Scheme | Quick, high-consequence | Directly resolves an active Scheme (§10) |
| Confront With Evidence | Quick | Ends a scheme's discovery phase decisively |
| Request Testimony | Quick | Legal-Specialty client favor (Politics & Patronage §4.2) |

### 9.8 Group & Public Interactions

Everything above is one-initiator-one-target. A real gap worth closing: several moments this project already treats as central — the Salutatio, a Triclinium feast, addressing the Curia — are inherently group moments, not a string of one-on-one actions. Rather than a separate mechanic, a **Group Interaction** is simply a normal Interaction (§9.1-9.7) resolved simultaneously against every present Character, with each target's own opinion, traits, and Axes read individually even though the initiating action is one and the same:

| Interaction | Resolution | Primary Inputs |
|---|---|---|
| Host a Gathering (Salutatio, Triclinium feast) | Quick, aggregate | Diplomacy/hosting quality vs. every attendee's opinion and Faction individually; Politics & Patronage §4.3 |
| Address a Crowd / Public Speech | Quick, aggregate | Diplomacy, Dignitas-wide effect rather than a single relationship |
| Address the Curia | Quick, aggregate | Politics & Patronage §5.6's body vote, run as a Group Interaction against every seated Decurion |

A Group Interaction's aggregate result is always just the sum of its individual per-target resolutions — there's no separate "crowd" stat to invent; a room full of Traditionalist-leaning Decurion Characters simply each independently react to a Sumptuary Edict announcement the way any single one of them would.

---

## 10. Multi-Stage Schemes — Progress, Discovery, Counter-Play

Per the decision to give high-stakes interactions real teeth: any interaction marked **Multi-stage** above runs through one shared engine rather than each system inventing its own.

1. **Initiation.** The player (or an NPC, per §8.3) commits to a Scheme against a target: type, and often an assisting agent (a client's Specialty favor, a hired specialist).
2. **Progress.** Each month, the Scheme advances by an amount driven by the initiator's relevant Core Attribute and Personality Axis (Intrigue and Boldness for most Coercive schemes; Diplomacy and Lustful/Chaste-adjacent axes for Seduce), modified by Influence or denarii spent that month.
3. **Discovery risk.** Rises the longer a Scheme runs, scaled against the target's own Intrigue and Perceptive/Oblivious trait, and against how many other Characters are aware of it (an assisting client is a leak risk as much as an asset).
4. **Counter-play.** Once the target's suspicion crosses a threshold, they aren't just informed after the fact — they can investigate (their own Intrigue), confront the initiator directly (an Informational interaction, §9.7), or launch a counter-scheme of their own. This is real back-and-forth, not a single roll with a delayed reveal.
5. **Resolution.** Four real outcomes, not two: **succeeded**, **failed quietly** (nothing detected, simply didn't work), **discovered-and-foiled** (target countered in time), or **discovered-and-escalated** (a failed poisoning becomes a public scandal, a failed seduction becomes a spurned-and-furious spouse) — the last being meaningfully worse than a clean failure, consistent with how every other high-stakes mechanic in this project treats getting caught versus simply not succeeding.

This engine is deliberately the shared backbone for Politics & Patronage's own §9 Scheming (which should be read as running on this engine going forward, superseding that document's lighter standalone treatment), Romance & Seduction's eventual courtship/affair mechanics, and Espionage's eventual full design — each of those systems supplies scheme *types* and target-specific flavor; this document supplies the *engine* they all run on.

---

## 11. Generation & Lazy Instantiation

Flattening to one tier (§12) raises an obvious practical question: does the game now need to pre-generate full records for every person who could ever be named? No — the answer is **lazy instantiation**, the same moment-of-first-contact principle Familia and the old Notable tier both already used, just without an intermediate lightweight stage to promote *from*. A Character record is created the instant a person is specifically named — a travel encounter rolls a named NPC, an election needs an opponent, a Curia seat needs filling, a Salutatio surfaces a new client — and at that exact moment they receive the **complete** schema: attributes, traits rolled appropriate to their source and context, an initial relationship-web entry, and (if relevant) a Faction leaning. There's no "lite" version ever created and later upgraded; a Character simply doesn't exist until the moment they're needed, and is whole the instant they do. The unnamed aggregate population below that threshold (Settlement Demographics) is completely untouched by any of this — that boundary was never about tiering Characters, and still isn't.

**Age-appropriate backfill.** §4.4's lifecycle gating (Congenital at birth, Formative through Adolescence, Reactive only from Adulthood onward) describes a Character *raised inside the simulation* — a household-born child the player actually watches grow up. A lazily-instantiated adult (a 50-year-old rival candidate, a middle-aged travel encounter) obviously can't start with zero Reactive traits just because the game only just met them; generation back-fills a plausible trait set for their apparent age and source instead — a rough life history compressed into an instant, rather than a blank adult waiting to accumulate one from scratch. A Character generated this way is exactly as "real" from that point forward as one grown in real time; nothing about their origin persists as a special flag, consistent with the same principle Familia §7 and Companions & Court Positions §7.3 already established for promoted individuals.

---

## 12. Retiring the Notable Tier — Migration Note

Politics & Patronage §3 introduced **Notable** as a deliberately lighter-weight tier — the same five Core Attributes and Loyalty/Ambition, but no Detailed/Body Attributes, Labor Skills, or lifecycle machinery. This document retires that distinction. Every reference to "Notable" in that document — the Curiales-recruitment sourcing, the existing-client-tag sourcing, the generated-rival sourcing — should now be read as producing a full **Character** per this document, with the complete schema (Detailed/Body Attributes, Labor Skills, the full Personality Trait catalog, Personality Axes) rather than the deliberately narrower subset that document specified. **Nothing about Politics & Patronage's actual mechanics changes** — Clientela, contested elections, the Curia body, the poaching mechanic all work exactly as designed. Only the depth of the record sitting behind each name grows. A pure terminology cleanup pass on that document (swapping "Notable" for "Character" throughout) is small, cosmetic, follow-up work rather than anything requiring redesign — flagged in §15.

---

## 13. Cross-System Integration

- **Familia:** this document formalizes and universalizes §2's stat architecture; Familia remains sole authority on lifecycle, birth, marriage, and legitimacy.
- **Politics & Patronage:** the Notable tier is retired (§12); Clientela favors and §9's Scheming both now run on this document's Interaction Catalog (§9) and Scheme engine (§10) respectively.
- **Companions & Court Positions:** every position holder was always a Familia record; this document adds the full trait/axis/interaction layer on top, giving "a Procurator acting in their own interest" (that document's own flagged open question) an actual mechanism — a high-Ambition, low-Honor, Opportunistic Procurator is now a legible, predictable risk rather than an unspecified one.
- **Labor & Slavery:** Regimen and punishment reactions now resolve through the Compassion, Honor, and Vengefulness axes directly; Broken, Feral, Resentful, and Defiant all get their proper Reactive-trait homes here.
- **Economy & Finance:** the Greed axis is the direct mechanical driver behind bribability, debasement temptation, and default risk; Debtor/Creditor is a named bond tag (§7).
- **Villa (interior design doc):** the Atrium/Salutatio and Triclinium/hospitality moments are now formally Interaction Catalog entries (§9.1) with a physical home.
- **Settlement Demographics:** the aggregate-population boundary is explicitly unchanged (§2, §11) — this document only ever concerns named individuals above that line.
- **Romance & Seduction (§6.19, future):** inherits the full Romantic category (§9.2) and the Seduce scheme type (§10) as its starting mechanical foundation rather than a blank slate.
- **Espionage (§6.15, future):** inherits the Coercive/Intrigue and Informational categories (§9.4, §9.7) and the full Scheme engine (§10) the same way.
- **Legal & Court (§6.16, future):** the Honor axis and several Coercive/Informational interactions (Interrogate, Request Testimony, Bring Charges) are this document's concrete contribution to that system's eventual caseload.
- **Rival Houses (§6.10, future):** every mechanism in this document — Characters, Axes, the Interaction Catalog, the Scheme engine, Faction — is built to apply to a rival gens's own members without modification; that system's own pass should extend this one, not replace it, exactly as Politics & Patronage already anticipated for its own rival-candidate mechanic.
- **Dynasty Chronicle (§6.11, future):** a discovered-and-escalated Scheme outcome, a major trait shift (Broken, Ruthless, Corrupt), and a notable Combo Title are all natural milestone-catalog material.

---

## 14. Data Model

```
Character {          // supersedes Familia §8's sketch and Politics & Patronage's Notable{}
  id, praenomen, nomen, cognomen, sex, age, lifecycleStage,
  legalStatus, socialClass,
  coreAttributes: { diplomacy, martial, stewardship, intrigue, learning },
  laborSkills: { fieldwork, domestic, craft, culinary, medicine },
  condition: { health, fatigue, loyalty, ambition, fertility },
  permanentInjuries: [...],
  traits: { congenital: [...], formative: [...], reactive: [...] },   // §4, exclusivity enforced per opposed pair
  personalityAxes: { honor, compassion, greed, zealotry, vengefulness, boldness, rationality },  // §5, -100..100
  comboTitle: string,           // §6 — curated lookup or dynamically generated
  appearance: { ... },          // Familia §2.4/§7.11, unchanged
  relationships: { [otherId]: { opinion, bondTags: [...] } },  // §7
  faction: "traditionalist" | "popularist" | null,   // only set where Politics & Patronage §3.1 applies
  source,               // "familia" | "courtPosition" | "curiaSeat" | "rivalGenerated" | "travelEncounter" |
                         // "eventEncounter" | "guest" — replaces Notable's source enum, now universal
  instantiatedAtMonth,   // §11 — when this record was first generated
  backfilledHistory: bool,  // §11 — true if traits/axes were generated as an age-appropriate life history
                             // rather than accumulated in real time (nearly always true for adults met, not raised)
}

Interaction {         // §9 — a single quick-resolution action
  interactionId,
  category,          // "social" | "romantic" | "political" | "coercive" | "economic" | "violent" | "informational"
  type,              // e.g. "befriend", "flirt", "bribe" — per §9's tables
  initiatorId, targetId,
  resolutionLayer,    // "mechanical" | "narrative"
  inputsUsed: { coreAttribute, personalityAxis, opinionModifier, resourceSpent },
  outcome,
}

GroupInteraction {    // §9.8 — one initiating action, resolved per-target
  groupInteractionId,
  category, type,
  initiatorId,
  targetIds: [...],
  perTargetOutcomes: { [targetId]: outcome },   // §9.8 — no separate "crowd" stat; always the sum of individual resolutions
}

Scheme {              // §10 — supersedes Politics & Patronage's lighter Scheme{}
  schemeId,
  type,               // "seduce" | "blackmailFabrication" | "sabotage" | "frame" | "assassinate" | "spyNetwork" | ...
  initiatorId, targetId,
  assistingCharacterIds: [...],
  progress,            // 0-100
  discoveryRisk,        // rises over time, scaled by target's Intrigue/Perceptive
  monthsRunning,
  status,              // "active" | "succeeded" | "failedQuiet" | "discoveredFoiled" | "discoveredEscalated"
}
```

---

## 15. Open Questions

- **All numeric sizing.** Consistent with this project's convention: axis nudge weights per trait, discovery-risk curves, Scheme progress rates, and combo-title dynamic-generation weighting are all unsized.
- **Notable → Character terminology pass.** §12 flags that Politics & Patronage's own text still says "Notable" throughout; a cosmetic find-and-replace pass on that document would tidy the seam but isn't required for the mechanics to work correctly as-is.
- **Trait inheritance weighting for the new full catalog.** Familia §2.6's inheritance-weighting question (a parent's Congenital trait raising a child's odds of rolling it) still isn't numerically specified, and now applies against a much larger enumerated pool.
- **Cross-category trait interactions beyond opposed pairs.** Whether certain non-opposed combinations should suppress or amplify each other's axis nudges (e.g., does Zealous + Rational partially cancel, or simply coexist as tension) isn't decided.
- **Narrative-layer technical implementation.** §8.2 deliberately doesn't specify how the "character brief" actually reaches whatever narrates a scene — that's a technical/engine decision outside this document's scope, not a game-design gap.
- **NPC-on-NPC Scheme visibility.** §8.3 establishes that Characters can scheme against each other without player involvement; whether and how the player is informed of purely NPC-on-NPC outcomes (a Chronicle entry only, a rumor via Gossip, or nothing at all until it affects the player directly) isn't specified.
- **Duel's formal rules.** §9.6 names Duel as a distinct, consensual, Honor-governed violent interaction, but doesn't specify challenge/acceptance conditions or lethality odds.
- **Typical trait load's exact distribution curve.** §4's "2-4 / 1-3 / 0-5" guideline is a working default, not a tuned probability distribution.
- **Group Interaction scale ceiling.** §9.8 resolves a Group Interaction as the sum of individual per-target outcomes; whether there's a practical cap on simultaneous targets (a Salutatio with 40 clients vs. 4) before that stops feeling tractable isn't specified.
- **Backfill generation depth.** §11's age-appropriate backfill guarantees a lazily-instantiated adult isn't a blank slate, but not how detailed that generated history actually needs to be — a full mini-biography versus just enough traits to be legible.
