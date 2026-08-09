# GENS — System Design: Traits (Full Catalog)
*A dedicated, deeper pass on Characters §4 — every existing trait carried over, given a real mechanical effect and flavor line instead of a one-line note, plus a genuine expansion toward CK3-scale breadth. Introduces tiered spectrums for five traits that always work better as a scale than a binary switch, and folds in the real historical Four Humors as the setting's own period-appropriate personality framework. This pass pushes the catalog to 234 traits, corrects two stale trait names inherited from the Characters document, gives every Combo Title an actual description across nine organized themes, and fixes the Lifestyle & Vocation subsection's lack of any real cost.*

---

## Contents

1. Scope & Role
2. Trait Anatomy — What Every Entry Contains
3. Tiered Spectrums
4. Congenital Traits
5. Formative Traits
6. Reactive Traits
7. Combo Titles — Expanded
8. Migration Note — What Changes From the Characters Document
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

Characters §4 enumerated 115 traits at a one-line-note depth — enough to establish the architecture (three lifecycle-gated categories, opposed pairs, Axis nudges, bespoke effects) but not enough to actually read each trait's texture. This document is that texture pass: every trait gets its **Axis nudge** stated directly, its **bespoke mechanical effect** spelled out rather than gestured at, and a **flavor line** — a snippet of how the trait actually sounds in the character's own voice or the narrator's description of them. It also genuinely expands the catalog, past 200 traits, and introduces **tiered spectrums** for five traits that were always better served by a scale than an on/off switch.

This document doesn't touch Characters §5 (Personality Axes themselves), §6 (Combo Titles' mechanism), §8 (resolution), §9 (the Interaction Catalog), or §10 (the Scheme engine) — those stay exactly as designed. It only replaces and expands §4's actual trait content, and extends §6's curated Combo Title list to make use of the new tiered traits.

---

## 2. Trait Anatomy — What Every Entry Contains

Every trait or trait pair below carries four things:

- **Axis Nudge** — which of the seven Personality Axes (Honor, Compassion, Greed, Zealotry, Vengefulness, Boldness, Rationality) it pushes, and roughly how hard: `+`/`-` for a small nudge, `++`/`--` for a large one, matching each Axis's own Pole A ↔ Pole B direction from Characters §5.
- **Effect** — the trait's bespoke, direct mechanical consequence, independent of the Axis nudge (per Characters §5's "axes and bespoke effects aren't competing systems" rule).
- **Flavor** — a short italicized line capturing how the trait actually reads — a line of internal voice, a narrator's description, or both sides of an opposed pair in brief contrast.
- **Category tags** in the running notes (Labor & Slavery, Politics & Patronage, etc.) where a trait's home system isn't obvious from the effect alone.

Traits remain tags, not sliders — a Character either holds a trait or doesn't, except within a **tiered spectrum** (§3), where holding one tier's tag actively excludes every other tier in that same spectrum.

---

## 3. Tiered Spectrums

Five traits from the original catalog always wanted to be a scale rather than a light switch. Each is now a small, exclusive, mandatory-pick-one spectrum — every Character holds exactly one tag per spectrum below, the same way a physical trait like handedness is always *some* answer rather than sometimes absent.

### 3.1 Intellect *(supersedes Quick/Slow)*

| Tier | Axis Nudge | Effect | Flavor |
|---|---|---|---|
| Dull | Rationality -- | Learning-adjacent tasks resolve at a real penalty; slow to see a scheme coming | *Means well. Gets there eventually.* |
| Average | — | No modifier; the unremarkable default most Characters hold | *Nothing notable, in either direction.* |
| Clever | Rationality + | Learning-adjacent tasks resolve favorably; quicker to spot a Scheme's discovery tell (Characters §10) | *Two steps ahead, usually without saying so.* |
| Brilliant | Rationality ++ | Strong bonus to Learning-adjacent resolution and Scheme discovery; rare | *The kind of mind other people quietly resent.* |

### 3.2 Beauty *(supersedes Beautiful/Plain)*

| Tier | Axis Nudge | Effect | Flavor |
|---|---|---|---|
| Hideous | — | Real penalty to Romantic-category interactions and first-impression Social ones; no Axis nudge — beauty isn't a moral quality | *People remember the face before the person.* |
| Plain | — | No modifier | *Unremarkable, and unbothered by it.* |
| Comely | — | Small bonus to Romantic/Social interactions | *Handsome enough to turn a head without trying.* |
| Beautiful | — | Strong bonus to Romantic/Social interactions, marriage-market draw (Familia §5) | *A face that becomes its own kind of leverage.* |

### 3.3 Physique *(supersedes Strong/Weak)*

| Tier | Axis Nudge | Effect | Flavor |
|---|---|---|---|
| Frail | — | Labor Skill and Martial ceiling penalty; Health more easily lost | *Wiry, or simply worn thin.* |
| Average | — | No modifier | |
| Strong | — | Labor Skill and Martial ceiling bonus | *Built for the work, whatever the work turns out to be.* |
| Herculean | Boldness + | Strong Labor/Martial bonus; a small Boldness nudge — the confidence of someone who's rarely physically overmatched | *The sort of build that ends arguments before they start.* |

### 3.4 The Four Humors *(new — the setting's own period psychology)*

Galenic humorism was the dominant framework for temperament in the Roman world, and every Character holds exactly one — a genuinely period-appropriate alternative to inventing a modern personality-type system. Distinct from the other tiers above: there's no "average" rung, since ancient medicine held that *everyone* runs on one dominant humor.

| Humor | Axis Nudge | Effect | Flavor |
|---|---|---|---|
| Sanguine | Boldness +, Rationality - | Quick to socialize and to act; slightly impulsive | *Warm blood, warm welcome, and not much patience for waiting.* |
| Choleric | Boldness +, Vengefulness + | Driven and ambitious; slow to let a slight go | *Fire under the skin — useful in a crisis, exhausting at dinner.* |
| Melancholic | Boldness -, Rationality + | Careful and introspective; prone to real sorrow | *Sees the shadow in every room before the light.* |
| Phlegmatic | Vengefulness -, Rationality + | Steady and hard to provoke; slow to grieve or celebrate either | *Unmoved, or simply very good at not showing it.* |

### 3.5 Piety *(supersedes Zealous/Impious)*

| Tier | Axis Nudge | Effect | Flavor |
|---|---|---|---|
| Impious | Zealotry -- | Ignores omens; immune to religious-Event penalties but forfeits their bonuses; Sumptuary/Traditionalist friction | *The gods, if they exist, have better things to do.* |
| Indifferent | — | No modifier — goes through the motions without real belief | *Burns the incense because it's expected, not because it matters.* |
| Devout | Zealotry + | Engages Religion's (§6.6, future) bonuses at moderate strength | *Keeps the household gods well-tended, and means it.* |
| Zealous | Zealotry ++ | Strongest Religion engagement; also the strongest Sumptuary-enforcement/Traditionalist-audience reception (Politics & Patronage §8) | *Would rather lose the argument than the ritual.* |

---

## 4. Congenital Traits

Rolled once at birth, inheritance-weighted (Characters §4.4, unchanged). 15 pairs carried over unchanged from Characters §4.1, plus 19 new pairs and one rare standalone — **69 traits**, not counting the three tiered spectrums (Intellect, Beauty, Physique) that used to live here as simple pairs.

### 4.1 Carried Over

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Hardy ↔ Sickly | — | Disease resistance up / down. *"Never sick a day in his life" ↔ "Wraps up warm even in summer."* |
| Fecund ↔ Barren | — | Fertility Condition stat up / down. *No flavor needed — a private, biological fact.* |
| Perceptive ↔ Oblivious | Rationality + / - | Reads a room, spots a Scheme's tell early / late. *"Notices the knife before it's drawn" ↔ "Finds out from everyone else."* |
| Calm ↔ Wrathful | Vengefulness - / + | Slow to anger / fast, feud-prone. *"Lets it go" ↔ "Remembers, and reddens."* |
| Patient ↔ Impatient | — | Bonus / penalty to multi-stage Scheme initiation. *"Waits for the tide" ↔ "Wants it settled by supper."* |
| Bold ↔ Cautious | Boldness + / - | Scheme/election initiation up / down. *"Leaps" ↔ "Looks, looks again."* |
| Gregarious ↔ Reserved | — | Salutatio/Clientela reach up / down. *"Never eats alone" ↔ "Prefers the company of two, at most."* |
| Proud ↔ Humble | — | Dignitas-loss sensitivity up / down. *"Wears the toga like armor" ↔ "Doesn't need the room to know his name."* |
| Stubborn ↔ Pliant | — | Negotiation resistance up / down. *"Outlasts every argument" ↔ "Talked into it before he'd noticed."* |
| Honest ↔ Deceitful | Honor ++ / -- | Contract/oath reliability up / down. *"His word is his bond" ↔ "Promises are just useful noises."* |
| Generous ↔ Greedy | Greed - / + | Bribability down / up, Gift-Giving cost down / up. *"Gives before he's asked" ↔ "Everything has a price, even kindness."* |
| Compassionate ↔ Callous | Compassion + / - | Punishment/Regimen reaction, mercy in rulings, up / down. *"Winces at the lash" ↔ "Doesn't understand why anyone would."* |
| Diligent ↔ Slothful | — | Stewardship task resolution up / down. *"First up, last to bed" ↔ "Tomorrow's problem, always."* |
| Lustful ↔ Chaste | — | Romance initiation odds up / down (distinct from actual conduct — see Faithful/Adulterous, §6). *"Notices everyone" ↔ "Notices no one, on principle."* |
| Temperate ↔ Gluttonous | — | Health/upkeep cost down / up. *"Eats to live" ↔ "Lives to eat, loudly."* |

### 4.2 New — Body & Senses

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Left-Handed ↔ Right-Handed | — | Left-Handed carries a small Duel surprise-bonus (a real Roman superstition about the *sinister* hand); otherwise flavor-only. *"Fights from the wrong side, on purpose."* — **Ambidextrous** (rare, standalone, no opposite): both hands equally able; small bonus to Craft Labor Skill and Duel resolution. *"Never had to choose."* |
| Keen-Eyed ↔ Nearsighted | Rationality + / — | Small bonus / penalty to any distance-dependent task (archery, reading, spotting an approach). *"Sees the ship before the lookout does" ↔ "Squints at everything past an arm's length."* |
| Melodious ↔ Harsh-Voiced | — | Small bonus / penalty to Social/Salutatio reception, singing-adjacent Events. *"A voice you'd follow into a burning building" ↔ "A voice like a cart on cobblestones."* |
| Graceful ↔ Clumsy | — | Small bonus / penalty to social deportment, dance, formal occasions. *"Moves like the room was built around her" ↔ "Trips over flat ground."* |
| Nimble ↔ Awkward | — | Small bonus / penalty to Craft Labor Skill, Duel footwork, theft-adjacent Schemes. *"Fingers faster than his thoughts" ↔ "All elbows, always."* |
| Insomniac ↔ Deep Sleeper | — | Fatigue recovers slower / an Insomniac is harder to catch off-guard at night (small Espionage-defense bonus); Deep Sleeper is easier to rob or assassinate in their own bed. *"Counts the hours instead of sheep" ↔ "Could sleep through a siege."* |

### 4.3 New — Temperament & Sensibility

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Thick-Skinned ↔ Thin-Skinned | — | Rebuke/Insult opinion damage taken down / up. *"Water off a duck" ↔ "Remembers every unkind word verbatim."* |
| Curious ↔ Apathetic | Rationality + / — | Faster Formative trait acquisition in Childhood (an innate head start on Studious, distinct from it); small bonus / penalty to Informational interactions. *"Has to know how it ends" ↔ "Doesn't much care either way."* |
| Creative ↔ Unimaginative | — | Bonus / penalty to Lifestyle-trait acquisition (§5.3) and Education & Culture's eventual creative-work outputs. *"Sees the statue in the stone" ↔ "Sees a rock."* |
| Adventurous ↔ Homebound | Boldness + / - | Bonus / penalty to Travel encounter frequency and quality. *"Already asking about the next road" ↔ "Has everything he needs within the walls."* |
| Even-Handed ↔ Capricious | Honor + / - | Consistency in dealing with subordinates/clients — small Loyalty stability bonus / instability penalty across the whole Clientela roster or household. *"Treats everyone the same, which is its own kind of comfort" ↔ "Never know which version you'll get."* |
| Sharp-Tongued ↔ Soft-Spoken | — | Rebuke/Insult effectiveness up (Sharp-Tongued) / Comfort and Confide effectiveness up (Soft-Spoken). *"Says the cruel thing before the kind one occurs to her" ↔ "Could talk a mourner back to sleep."* |
| Vigilant ↔ Careless | Rationality + / - | Bonus / penalty to Scheme discovery (the target's side, Characters §10) and estate security-adjacent tasks. *"Checks the locks twice" ↔ "Leaves the gate open more often than he'd admit."* |

### 4.4 New — Mind & Body, Round Two

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Iron-Willed ↔ Weak-Willed | Rationality + / - | Resistance to Seduce/Scheme manipulation up / down (Characters §9.2, §9.4) — distinct from Stubborn/Pliant, which is about negotiation rather than manipulation specifically. *"Nothing gets past the front door of his mind" ↔ "Talked into things he later can't quite explain."* |
| Prodigious Memory ↔ Forgetful | Rationality + / - | Bonus / penalty to Legal testimony, Espionage recall, and Dynasty Chronicle detail — distinct from the Intellect tier, which is raw capacity rather than retention. *"Recites the debt's exact wording a decade later" ↔ "Swears he'd remember, and doesn't."* |
| Steady-Handed ↔ Shaky | — | Bonus / penalty to Craft and Medicine Labor Skill precision, and to Duel accuracy. *"Never spills, never slips" ↔ "The tremor gets worse when it matters most."* |
| Precocious ↔ Late Bloomer | — | Faster / slower Formative trait acquisition in Childhood, and earlier / later eligibility for Lifestyle traits. *"Already reading at four, unbearably" ↔ "Came into himself well past when anyone expected."* |
| Long-Lived Stock ↔ Short-Lived Stock | — | Inheritance-weighted lifespan tendency — a real hook into Familia's aging/mortality mechanics and Succession & Dynasty (§6.9, future) planning, distinct from Hardy/Sickly's disease resistance specifically. *"Buried three siblings and outlived them all by decades" ↔ "The family's men rarely see sixty, and everyone quietly knows it."* |
| Bloodlust ↔ Squeamish | Compassion - / + | Reaction to violence and gore — colors Games & Spectacle spectator enjoyment, reception to witnessing a punishment (Labor & Slavery) or an execution (Characters §9.6). *"Leans forward when the games turn bloody" ↔ "Looks away, every time, and hates that everyone notices."* |

---

## 5. Formative Traits

Acquired through Childhood/Adolescence via upbringing and Education (Characters §4.4, unchanged). 17 pairs carried over from Characters §4.2 (minus the retired Zealous/Impious, now the Piety tier, §3.5), plus 4 new pairs and a genuinely large new **Lifestyle & Vocation** subsection — **72 traits** total.

### 5.1 Carried Over

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Rational ↔ Superstitious | Rationality + / - | Event-response flavor; Rational shrugs off an omen, Superstitious takes it seriously. *"A coincidence is just a coincidence" ↔ "That crow meant something."* |
| Idealistic ↔ Pragmatic | Honor + / - | Scheme/negotiation flavor — Idealistic resists dishonorable options even when they'd work. *"Won't win this way, and won't try" ↔ "It works, so it's the plan."* |
| Honor-Bound ↔ Opportunistic | Honor ++ / -- | Strong contract/oath reliability shift, on top of any Congenital Honest/Deceitful. *"A vow is a vow" ↔ "Circumstances change; so do commitments."* |
| Cosmopolitan ↔ Xenophobic | — | Reputation Duality local-standing gain/loss (Politics & Patronage §2.1); Diplomacy with Non-Roman Peoples reception. *"A guest is a guest, wherever he's from" ↔ "Rome's ways, or no ways."* |
| Trusting ↔ Cynical | — | Deception vulnerability up (Trusting) / Befriend/Seduce resistance up (Cynical). *"Takes people at their word" ↔ "Assumes the angle first."* |
| Studious ↔ Incurious | — | Education & Culture's main hook; formal-learning speed up / down. *"Always another scroll to finish" ↔ "Learned enough already, thanks."* |
| Disciplined ↔ Undisciplined | — | Consistency in duty performance; small Stewardship-adjacent reliability shift. *"Same hour, every day, without fail" ↔ "Gets there when he gets there."* |
| Dutiful ↔ Wayward | — | Succession-drama risk down / up; parent's baseline opinion higher / lower. *"Does what's expected, and means it" ↔ "Expected is exactly the problem."* |
| Filial ↔ Rebellious | — | Parent/child relationship-web opinion baseline up / down. *"Never raised his voice to his father" ↔ "Left home the day he could."* |
| Eloquent ↔ Tongue-Tied | — | Diplomacy-adjacent negotiation bonus / penalty. *"Could talk the Senate into anything" ↔ "Loses every argument to his own stammer."* |
| Refined ↔ Coarse | — | Dignitas-adjacent social reception up / down. *"Knows which fork, and why it matters" ↔ "Eats like the meal's about to be taken away."* |
| Frugal ↔ Extravagant | Greed + / - | Economy & Finance expense baseline down / up. *"A denarius saved" ↔ "A denarius is for spending."* |
| Well-Traveled ↔ Provincial | — | Travel/Diplomacy bonus / minor penalty outside home territory. *"Has seen three seas and isn't impressed by a fourth" ↔ "Rome is far enough, thank you."* |
| Charitable ↔ Mercenary | Greed - / + | Clientela favor generosity up / down. *"Gives without keeping score" ↔ "Every favor is a loan, whether he says so or not."* |
| Loyal-Hearted ↔ Fickle | — | Direct Loyalty Condition stat nudge, up / down. *"Once given, never taken back" ↔ "Today's devotion, tomorrow's memory."* |
| Contentious ↔ Amicable | — | Curia/scheme friction baseline up / down. *"Finds the disagreement in any room" ↔ "Finds the common ground instead."* |
| Martial-Minded ↔ Peace-Loving | — | Military & Combat/Games affinity up / down. *"Itching for the campaign season" ↔ "Has never once missed the sound of it."* |

### 5.2 New

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Well-Read ↔ Illiterate | — | Direct literacy flag — distinct from Studious/Incurious (curiosity) and Intellect (raw capacity); gates Correspondence & Letters (§6.27, future) and certain Education outcomes. *"Keeps a library he's actually read" ↔ "Signs with a mark, and isn't ashamed of it."* |
| Egalitarian ↔ Hierarchical | Compassion + / - | Attitude toward class distinction — colors Labor & Slavery treatment choices and Politics & Patronage Faction leaning without being identical to either. *"Doesn't see why the Ostiarius should eat after the family" ↔ "Everyone has their proper place, and it isn't hard to see."* |
| Deferential ↔ Skeptical of Authority | Honor + / - | Attitude toward magistrates, officers, and institutions broadly — distinct from Filial/Rebellious, which is about parents specifically. *"Assumes the office deserves the respect, whoever holds it" ↔ "Waits to see if the man's earned it first."* |
| Frontier-Raised ↔ City-Raised | — | Where a Character actually grew up, distinct from Well-Traveled/Provincial (which is about adult travel, not childhood environment) — feeds Reputation Duality's local-standing axis and Settlement Demographics flavor. *"Learned to read weather and threat before he learned to read letters" ↔ "Knew the Forum's layout before he knew most of his cousins."* |

### 5.3 Lifestyle & Vocation *(new subsection)*

The one deliberate exception to Formative's Childhood/Adolescence timing: these are acquired through sustained adult practice, the same way CK3's own Lifestyle traits work, rather than locked in by the end of Adolescence. A Character can pick up a Lifestyle trait at any point in Adulthood through dedicated activity — mostly standalone badges of specialization rather than opposed temperament pairs.

**Balance note — the Lifestyle Cap.** Nearly thirty of these exist, and every single one is pure upside with no attached cost, which risks turning the category into a checklist a min-maxing Character (or a min-maxing player) simply completes rather than a real specialization. To keep a Lifestyle trait meaning something: **a Character holds at most three at once.** Acquiring a fourth requires letting one of the existing three lapse from disuse (the same "hasn't practiced it in years" logic that already governs how these are earned in the first place) — this keeps the roster a genuine, curated identity rather than an ever-growing badge collection, and gives the player a real choice at the point of a fourth acquisition rather than a free add. A small number of Lifestyle traits also carry their own direct cost, noted below, on top of the cap.

**Costed exceptions:**

- **Duelist** — a reputation that isn't fully controllable: an established Duelist finds it socially costly (a real Honor-axis-linked Dignitas penalty) to *decline* a formal Challenge once issued, unlike everyone else, for whom Duel remains an optional Interaction.
- **Spymaster** — better concealment while active, but a harsher fall: if a Spymaster's own Scheme is ever discovered, it resolves as Discovered-and-Escalated (Characters §10) rather than merely Discovered-and-Foiled — the reputation that makes them good at this makes exposure worse, not softer.

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| Strategist ↔ Berserker | Rationality + / - | Strategist: bonus to command-resolution and Scheme planning. Berserker: bonus to raw Martial force, penalty to anything requiring patience. *"Wins the battle before it's fought" ↔ "Wins it by refusing to stop."* |
| Horseman | — | Travel speed/quality bonus; Cavalry-adjacent Military & Combat bonus. *"More at ease in the saddle than the Curia."* |
| Hunter | — | Villa Diaeta/hunting-adjacent Event bonus; Martial-adjacent flavor. *"Knows the boar trails better than the family trails."* |
| Architect | — | Estate & Settlement construction-quality flavor bonus. *"Sees the finished building in the empty plot."* |
| Physician | — | Personal Learning-driven medical bonus, distinct from the formal Court Physician/Valetudinarius position. *"Trusts his own hands over any hired one's."* |
| Historian | — | Education & Culture flavor bonus; Dynasty Chronicle entries read richer when authored by one. *"Remembers the family's whole shape, not just his branch of it."* |
| Poet | — | Diplomacy-adjacent cultural-prestige flavor bonus. *"Turns a eulogy into something people actually repeat."* |
| Theologian | Zealotry + | Religion (§6.6, future) engagement bonus, distinct from simple Piety-tier devotion. *"Doesn't just pray — argues about what the prayer means."* |
| Gourmet | — | Culinary Labor Skill flavor bonus; distinct from Gluttonous (a vice, not a palate). *"Can name the vintage blind."* |
| Green Thumb | — | Fieldwork Labor Skill flavor bonus. *"Talks to the vines, and swears they listen."* |
| Master Craftsman | — | Craft Labor Skill flavor bonus. *"Every joint he cuts outlasts him."* |
| Merchant's Instinct | Greed + | Trade/Contract negotiation bonus (Economy & Finance §3.2); distinct from simple Greed — this is skill, not appetite. *"Smells a good deal three streets away."* |
| Diplomat | — | Negotiation/alliance-brokering bonus beyond the raw Diplomacy Attribute. *"Makes the concession feel like a victory."* |
| Legal Scholar | — | Legal & Court (§6.16, future) case-argument bonus. *"Cites precedent the way other men curse."* |
| Naturalist | — | Menagerie-Keeper/Dovecote-adjacent flavor bonus; small bonus to identifying a poison or omen correctly. *"Reads the birds' flight like a letter."* |
| Gladiator's Heart | Boldness + | Arena-adjacent Martial bonus; Games & Spectacle (§6.22, future) flavor. *"Trained for the sand, and never quite left it behind."* |
| Sailor | — | Sea Travel bonus; Navarchus-adjacent flavor. *"More sure-footed on a deck than a floor."* |
| Engineer | — | Aqueduct/road/fortification-adjacent Estate & Settlement bonus, distinct from Architect's broader design flavor. *"Cares less how it looks than whether it holds."* |
| Cartographer | — | Travel route-planning bonus; reduces a route's Piracy & Banditry risk slightly (Economy & Finance §7). *"Has never once been lost, and finds that suspicious in other people."* |
| Astrologer | Zealotry + | Omen-reading flavor bonus, distinct from Theologian's formal cult focus. *"Reads the coming year in this month's sky."* |
| Vintner | — | Wine-adjacent Estate & Settlement quality bonus. *"Knows exactly which slope gives the better grape."* |
| Herbalist | — | Medicine Labor Skill bonus distinct from Physician's general practice; a small, dual-use bonus toward both remedies and poison-adjacent Schemes. *"Knows which root heals and which one doesn't, and keeps both in the same cupboard."* |
| Wrestler | — | Gymnasium-adjacent Martial/Health bonus, distinct from Gladiator's Heart's arena focus — Greek athletic culture rather than Roman spectacle. *"Still keeps the oil and the sand-pit, years past his last real match."* |
| Philosopher | Rationality + | Secular wisdom-school flavor bonus (Stoic, Epicurean, or otherwise) — distinct from Theologian's cult focus; Education & Culture hook. *"Has an answer for everything, and isn't always right, and doesn't much mind."* |
| Playwright | — | Dramatic-composition bonus, distinct from Poet's broader verse; Games & Spectacle (§6.22, future) hook. *"Writes the tragedy, and privately enjoys how much the audience weeps."* |
| Numismatist | — | Minor Economy & Finance flavor bonus to appraising coin/goods value; mostly a personality quirk. *"Can tell a debased coin by weight alone."* |
| Genealogist | — | Dynasty Chronicle (§6.11, future) detail-richness bonus; Familia lineage-dispute flavor. *"Can recite eleven generations without pausing for breath."* |
| Duelist | Boldness + | Formal one-on-one combat specialization, distinct from Gladiator's Heart's arena context and Strategist's command focus — the Duel interaction's dedicated practitioner (Characters §9.6). *"Has never once lost a formal challenge, and makes sure everyone remembers it."* |
| Spymaster | — | Espionage's (§6.15, future) forward hook — bonus to Scheme concealment and to running an assisting-agent network. *"Knows things he was never told, and never says how."* |

---

## 6. Reactive Traits

Gained or lost during Adulthood based on treatment and events (Characters §4.4). **Amendment worth stating plainly:** grief doesn't wait for adulthood — a small, named exception lets *loss-driven* Reactive traits (Orphaned, Widowed, Grieving, Haunted, Traumatized) be acquired at any lifecycle stage from Child onward, while every other Reactive trait still only begins accumulating in Adulthood as originally specified. All 43 traits from Characters §4.3 carry over unchanged, plus a substantial new set — **73 traits** total.

### 6.1 Carried Over — Response to Treatment

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Content ↔ Resentful | Vengefulness - / + | Baseline mood/compliance up / Labor & Slavery's main Unrest hook. *"Has made peace with his station" ↔ "Smiles, and means none of it."* |
| Grateful ↔ Vengeful | Vengefulness - / + | Opinion recovery bonus / grudge duration extension. *"Never forgets a kindness" ↔ "Never forgets anything else, either."* |
| Devoted ↔ Estranged | — | Strong Loyalty bond / strong Loyalty penalty from sustained treatment. *"Would take the blow meant for you" ↔ "Answers, and nothing more."* |
| Bitter ↔ Forgiving | Vengefulness + / - | Grudge duration up / down. *"Keeps the wound open on purpose" ↔ "Lets the scar just be a scar."* |
| Emboldened ↔ Cowed | Boldness + / - | Direct product of reward/punishment history. *"Learned that speaking up works" ↔ "Learned that it doesn't."* |
| Defiant ↔ Broken | Boldness + / - | Punishment-ladder endpoints (Labor & Slavery). *"Still won't kneel" ↔ "Stopped fighting it, and stopped being anyone in particular."* |

### 6.2 Carried Over — Trauma & Resilience

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| Traumatized ↔ Resilient | Boldness - / + | Violence/loss aftermath. *"Flinches at sounds no one else hears" ↔ "Buried it, and kept walking."* |
| Battle-Hardened ↔ Shell-Shocked | Boldness + / - | Military & Combat aftermath. *"The field doesn't frighten him anymore" ↔ "Never quite left the field."* |
| Paranoid ↔ Serene | Rationality - / + | Post-discovery/betrayal aftermath; Paranoid raises own Scheme-discovery sensitivity but strains every relationship. *"Checks the wine twice" ↔ "Decided fear wasn't worth the cost."* |
| Haunted | — | By a specific, named past death or loss; standalone. *"Sets a place at the table that's never filled."* |
| Grieving | — | Fresh, typically time-limited. *"The house is loud, and somehow that's worse."* |
| Addled | — | Age/injury/disease-driven cognitive decline; standalone. *"Tells the same story like it's new every time."* |
| Scarred | — | Permanent injury's visible mark (Familia §3.1); standalone. *"The story's on his face before he tells it."* |

### 6.3 Carried Over — Vice & Corruption

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Drunkard ↔ Abstemious | — | Health/reliability cost up / down. *"The wine finds him before noon" ↔ "Hasn't touched it since the funeral."* |
| Corrupt ↔ Incorruptible | Greed + / - | Office-holding temptation up / down. *"Every ruling has a price, quietly" ↔ "Couldn't be bought with the whole treasury."* |
| Complacent ↔ Driven | Boldness - / + | Post-success/failure drift. *"Decided this was enough" ↔ "Decided nothing ever is."* |
| Envious ↔ Magnanimous | Greed + / - | Rival-comparison-driven. *"Can't enjoy his own house for watching yours" ↔ "Genuinely happy to see you do well."* |
| Ruthless ↔ Merciful | Compassion - / + | Chosen hardening through command/authority — distinct from innate Callous. *"Learned to stop flinching, on purpose" ↔ "Could have won harder, and chose not to."* |
| Feral | — | Extreme neglect/Bare Regimen aftermath; standalone, rare. *"Doesn't trust a kind word anymore — assumes it's a trick."* |
| Fanatical ↔ Disaffected | Zealotry ++ / -- | Radicalization or disillusionment from a cause. *"Would die for it tomorrow" ↔ "Watched the cause eat someone he loved."* |

### 6.4 Carried Over — Romance-Specific

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Heartbroken ↔ Guarded | — | Post-affair/divorce aftermath. *"Still sets out two cups" ↔ "Never again, and means it."* |
| Infatuated ↔ Disillusioned | — | Active courtship state vs. its collapse. *"Can't finish a sentence without her name in it" ↔ "Wonders what he ever saw."* |
| Faithful ↔ Adulterous | Honor + / - | Acquired from actual marital conduct, distinct from innate Lustful/Chaste appetite. *"Never gave anyone reason to doubt him" ↔ "Has a second life the household half-suspects."* |

### 6.5 Carried Over — War-Specific

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Warmonger ↔ War-Weary | Boldness + / - | Sustained campaign exposure. *"Asks when the next levy is" ↔ "Prays there isn't one."* |

### 6.6 New — Belief, Law & Standing

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Devotee ↔ Apostate | Zealotry + / -- | Attachment to (or renunciation of) a specific foreign cult — Religion's (§6.6, future) forward hook. *"Wears the god's mark where it can be seen" ↔ "Walked out mid-rite, and never went back."* |
| Litigious ↔ Conflict-Averse | Honor - / + | Legal & Court's (§6.16, future) forward hook — quick to sue / avoids confrontation even when owed. *"Has a favorite advocate on retainer" ↔ "Would rather eat the loss than the argument."* |
| Power-Hungry ↔ Jaded | Boldness + / - | Developed after tasting real office — Politics & Patronage's forward hook. *"One magistracy was never going to be enough" ↔ "Held the office, and found it hollow."* |
| Scandal-Marked ↔ Rehabilitated | — | A lasting Dignitas penalty from public disgrace / a recovered one from a successfully managed comeback. *"The whole city remembers, whether he'd like that or not" ↔ "Outlived the whisper, mostly."* |
| Assimilated ↔ Unbowed | — | Reputation Duality (Politics & Patronage §2.1) — a Peregrine/Latin-Rights individual's embrace of Roman custom, or deliberate retention of native identity. *"Wears the toga better than some who were born to it" ↔ "Keeps his own tongue, his own gods, his own name."* |

### 6.7 New — Slavery & Manumission Aftermath

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| Freed Spirit | Boldness ++ | A manumitted individual's joyous embrace of freedom — strong Ambition/Boldness lift. *"Still can't quite believe he's allowed to leave the room."* |
| Institutionalized | Boldness -- | The opposite response — freedom itself becomes a source of anxiety; prefers structure and direction. *"Asks permission for things no one would deny him."* |
| Cunning Survivor | Rationality + | Harsh treatment hardened into shrewdness rather than despair — a real, if uncomfortable, alternative outcome to Broken/Feral. *"Learned exactly how far the rules bend, and lives there."* |

### 6.8 New — Command & Loss

| Pair | Axis Nudge | Effect & Flavor |
|---|---|---|
| Inspiring Commander ↔ Feared Commander | Compassion + / - | Two effective but distinct Military & Combat leadership styles — Loyalty-driven versus fear-driven compliance. *"They'd follow him into anything, and know it" ↔ "They follow him because the alternative is worse."* |
| Debt-Scarred ↔ Debt-Free at Last | Greed - / — | Economy & Finance's Insolvency (§9 of that doc) aftermath — lasting frugality/anxiety from near-ruin, or genuine relief and a lighter hand with money going forward. *"Counts every denarius like it's the last one" ↔ "Spends a little more freely now, having earned the right."* |

### 6.9 New — Standalone Losses *(the lifecycle-timing exception, §6 intro)*

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| Widowed | — | Marks the loss of a spouse; a Familia/Succession flavor tag distinct from generic Grieving, relevant to remarriage negotiations. *"Still wears the ring, out of habit or defiance, hard to say which."* |
| Orphaned | — | Marks a childhood or adulthood without one or both parents; can be acquired well before Adulthood, per §6's own amendment. *"Learned early that no one was coming."* |
| Sole Survivor | Boldness + | The only one left standing after a disaster, shipwreck, or massacre — rare, narratively weighty. *"Has never fully explained why it was him and not the others."* |
| Betrayed | Honor - | A specific, named broken trust from a specific relationship — distinct from generalized Bitter/Vengeful. *"Doesn't trust easily anymore, and can tell you exactly why."* |
| Epileptic | — | A real, period-attested condition (the "sacred disease"); occasional incapacitation, historically often read as an omen rather than an illness — a Religion-adjacent flavor hook rather than a purely medical one. *"The household priest reads more into it than the physician does."* |

### 6.10 New — Round Two: War, Law & Family

| Trait | Axis Nudge | Effect & Flavor |
|---|---|---|
| War Hero ↔ War Criminal | — | Two opposite Military & Combat (§6.7, future) reputation outcomes — strong Dignitas gain / a lasting Dignitas and Legal exposure cost. *"The whole settlement turns out for his return" ↔ "Some of them still won't say his name in public."* |
| Plague Survivor | — | A Disease & Public Health (§6.13, future) forward hook — small Health-resilience bonus, standalone. *"Was sick enough that they'd already started the arrangements."* |
| Exiled | — | A severe Legal & Court/Politics & Patronage outcome — standalone; marks someone living, or having lived, outside their home territory by compulsion rather than choice. *"Writes home more than he'll admit, and never gets an answer he likes."* |
| Disowned | Honor - | A severe Familia/Succession rupture — standalone; formally cut off, whether or not the relationship-web opinion ever fully recovers. *"The name's still his. Nothing else in that house is, anymore."* |
| Kingmaker | — | Marks someone who was decisive in another Character's major election or office win (Politics & Patronage §5.5) — standalone, a real political-prestige flag distinct from simply holding office oneself. *"Never stood for anything himself. Never needed to."* |
| Cursed ↔ Blessed | — | Marks a Character others believe is touched by ill or good fortune after a run of remarkable luck either way — a Superstitious-adjacent social perception rather than any real mechanical fortune-changer. *"People cross the street rather than walk near him" ↔ "Every venture he touches seems to just work out."* |

---

## 7. Combo Titles — Expanded, With Descriptions

Characters §6 curated 14 pairings as bare title strings. This section makes this document the single authoritative source for Combo Titles going forward — every original entry is reproduced here (with one small correction noted below), every entry gets an actual description of how it plays out, and the roster grows substantially using the new tiered and Lifestyle traits. **Collision and fallback rules are unchanged from Characters §6:** checked top-to-bottom, first match wins, dynamic fallback for anything matching neither this list nor a curated pair, recalculated whenever the underlying Reactive traits change.

**Two corrections while consolidating:** Characters §6's original table paired "Brave" with Deceitful for *Treacherous Hero*, and "Craven" with Deceitful for *Cowardly Schemer* — but neither Brave nor Craven was ever actually defined as a trait; Characters §4.1 only defined **Bold ↔ Cautious**. Both stale labels are corrected below (Bold + Deceitful, Cautious + Deceitful) and fixed at the source in Characters §6 and §9.4 itself.

### 7.1 Villainous & Corrupt

| Combo | Title | Description |
|---|---|---|
| Deceitful + Zealous | Pious Fraud | Preaches virtue from the front of the room and quietly profits from every donation box behind it. |
| Callous + Zealous | Zealous Tyrant | Enforces the faith with a lash, genuinely convinced that cruelty is a form of devotion. |
| Bold + Deceitful | Treacherous Hero | Wins the crowd's admiration and the enemy's trust in the same breath, and spends both without hesitation. |
| Greedy + Zealous | Temple Grifter | Reads the omens however the paying customer needs them read. |
| Cautious + Deceitful | Cowardly Schemer | Would rather poison a rival quietly than ever risk facing him. |
| Greedy + Corrupt | Venal Magistrate | Every ruling has a price; she stopped pretending otherwise years ago. |
| Ruthless + Disciplined | Iron Hand | Never raises her voice, and never needs to — the results speak for themselves. |
| Cynical + Eloquent | Honeyed Viper | Says exactly what the room wants to hear, and means none of it. |
| Brilliant + Deceitful | Mastermind | Three moves ahead of anyone who makes the mistake of trusting her. |
| Power-Hungry + Deceitful | The Climber | Every kindness she shows is an investment, never a gift, and she's kept careful track of the returns. |

### 7.2 Heroic & Virtuous

| Combo | Title | Description |
|---|---|---|
| Honest + Bold | Forthright Champion | Says the hard thing to the powerful, in the room where it matters, and doesn't flinch after. |
| Compassionate + Diligent | Devoted Caregiver | The kind of tending that outlasts any wage or duty roster — shows up because she chooses to, every day. |
| Generous + Gregarious | Beloved Patron | The Salutatio line at his door is the longest in the settlement, and he actually remembers all their names. |
| Brilliant + Honor-Bound | The Just Genius | Clever enough to cut every corner available to her, and refuses on principle, every single time. |
| Merciful + Herculean | The Gentle Giant | Strong enough to end any argument by force, and consistently chooses not to. |
| Incorruptible + Frugal | The Unbought Man | Nothing in his house was ever paid for with someone else's coin, and he'll tell you so, unprompted. |

### 7.3 Tragic

| Combo | Title | Description |
|---|---|---|
| Compassionate + Impious | Godless Altruist | Kind without needing a watching god to make the kindness worthwhile — and quietly tired of being asked how. |
| Wrathful + Vengeful | Undying Grudge-Bearer | Remembers every slight with perfect, exhausting clarity, and has never once let one go. |
| Vengeful + Eloquent | Silver-Tongued Avenger | Turns every old grievance into a speech worth repeating — and repeats it often. |
| Hideous + Compassionate | Kind Monster | Watches people flinch at her face and has simply stopped expecting better. |
| Heartbroken + Loyal-Hearted | The Widower's Devotion | Loves a memory more faithfully than most people manage to love the living. |
| Broken + Devoted | The Faithful Wreck | What's left of him still shows up, every single day, because showing up is all that's left. |
| Scarred + Brilliant | The Marked Mind | The face people stare at first; the mind they underestimate right after, to their own cost. |
| Sole Survivor + Paranoid | The One Who Lived | Has never stopped waiting for the second disaster, and half-suspects everyone of causing the first. |
| Choleric + Vengeful | The Grudge That Walks | Has a memory for slights longer than the Republic's own history, and brings every one of them to dinner. |
| Cursed + Traumatized | The Ill-Fated | Has stopped arguing with the neighbors about it, and privately isn't sure they're wrong. |

### 7.4 Comedic & Quirky

| Combo | Title | Description |
|---|---|---|
| Dull + Content | Simple and Satisfied | Blissfully, genuinely unaware of exactly how little he understands, and none the worse for it. |
| Gluttonous + Gregarious | The Life of the Feast | Has never once left a Triclinium with food still on the table. |
| Drunkard + Eloquent | The Charming Wreck | Says the wittiest thing in the room, roughly three cups past anyone else's definition of sober. |
| Numismatist + Frugal | The Hoarder | Loves denarii less for what they can buy than for the simple fact of holding them. |
| Vigilant + Paranoid | The Overwatcher | Checks the locks, then checks them again, then lies awake wondering if she checked them right. |
| Illiterate + Proud | The Man Who Won't Admit It | Insists he "just prefers to listen" rather than say the word aloud. |
| Awkward + Gregarious | The Well-Meaning Disaster | Throws the best parties in the settlement and breaks the most furniture at her own. |
| Forgetful + Diligent | Tries So Hard | Shows up every single day, without fail, and still can't remember why half the time. |

### 7.5 Political

| Combo | Title | Description |
|---|---|---|
| Sanguine + Opportunistic | The Charming Opportunist | Walks into every room already knowing exactly how he intends to use it. |
| Power-Hungry + Eloquent | The Demagogue | Has never lost a crowd, and has never once told one the whole truth either. |
| Jaded + Incorruptible | The Reluctant Statesman | Stayed honest well after losing every reason to believe it would actually matter. |
| Kingmaker + Cynical | The Man Behind the Curule Chair | Never runs for office himself, and has never once needed to. |
| Frontier-Raised + Bold | The Untamed | Rome's laws reach him eventually, but his instincts got there first. |
| Egalitarian + Charitable | The People's Patron | Treats her clients like equals, and somehow they love her all the more for it. |
| Hierarchical + Proud | The Old Blood | Never lets anyone in the room forget which of them was born to it. |
| Litigious + Eloquent | The Courtroom Fixture | Has sued more neighbors than he's ever dined with. |

### 7.6 Religious

| Combo | Title | Description |
|---|---|---|
| Melancholic + Devout | The Weeping Faithful | Prays harder since the loss, as if sheer volume could substitute for certainty. |
| Zealous + Corrupt | The False Prophet | Preaches the gods' favor from the front of the room, and sells it quietly out the back. |
| Astrologer + Superstitious | The Reader of Omens | Won't leave the house in the morning without checking what the sky has to say about it. |
| Theologian + Cynical | The Doubting Priest | Argues theology for a living, and privately isn't sure she believes a word of her own argument. |
| Devout + Charitable | The Gods' Own Steward | Gives to the temple and the beggar at its steps with the same open hand. |
| Impious + Rational | The Quiet Doubter | Attends every rite the household expects, and privately suspects none of it changes anything. |
| Fanatical + Zealous | The True Believer | Would burn the whole house down tomorrow if the omens genuinely asked for it. |

### 7.7 Romantic

| Combo | Title | Description |
|---|---|---|
| Beautiful + Deceitful | Deceptive Beauty | The face that opens every door he has no honest intention of walking through. |
| Comely + Adulterous | The Charming Wanderer | Never short of an invitation, and never short of an excuse for the last one either. |
| Faithful + Homebound | The Steadfast Heart | Never once looked elsewhere, and genuinely never wanted to. |
| Infatuated + Trusting | Head Over Heels | Sees no flaw in her yet, and isn't interested in being shown one. |
| Lustful + Eloquent | The Practiced Charmer | Has said the same beautiful lie to more people than he can actually count. |
| Chaste + Devoted | The One True Love | Never needed anyone else, not even long enough to notice. |
| Guarded + Cynical | Burned Twice | Doesn't trust the compliment, and doesn't much trust the person giving it either. |

### 7.8 Family & Dynasty

| Combo | Title | Description |
|---|---|---|
| Freed Spirit + Merchant's Instinct | Self-Made | Arrived with nothing, and built the whole thing with his own two hands, and will remind you of it. |
| Institutionalized + Loyal-Hearted | The Perfect Servant | Obeys not from love but from a quiet, permanent fear of what freedom would actually require of her. |
| Genealogist + Proud | The Keeper of the Lineage | Can recite eleven generations of the family without pausing for breath, and judges anyone who can't. |
| Dutiful + Filial | The Ideal Heir | Delivers everything expected of him, without complaint, and without much of himself left over. |
| Rebellious + Bold | The Family's Black Sheep | Left the path laid out for her at the first real opportunity, and hasn't once looked back. |
| Long-Lived Stock + Patient | The Long Game | Has outlived two rivals' entire plans simply by refusing to hurry his own. |

### 7.9 Martial

| Combo | Title | Description |
|---|---|---|
| Herculean + Callous | The Brute | Strong enough to win most arguments before they start, and rarely bothers finding out if he needs to. |
| Battle-Hardened + Merciful | The Soldier's Conscience | Has seen enough of war to know precisely when it's time to stop, and says so. |
| Warmonger + Herculean | Born for the Legion | Was never going to be anything else, and never once wanted to be. |
| Strategist + Cynical | The Cold Calculator | Every battle is a ledger to him, and the men involved are simply the ink. |
| Bloodlust + Herculean | The Arena's Favorite | Never happier than when the crowd is roaring and someone's about to lose. |

### 7.10 Intellectual & Eccentric

| Combo | Title | Description |
|---|---|---|
| Philosopher + Melancholic | The Brooding Sage | Has a tidy answer for the meaning of suffering, and still hasn't found peace with her own. |
| Naturalist + Curious | The Endless Cataloguer | Can name every bird in the province on sight, and regularly forgets the names of people he's met twice. |
| Historian + Forgetful | The Irony of It | Remembers the whole family's history across six generations, and can't recall where he left his stylus. |
| Creative + Precocious | The Prodigy | Was composing real verse before most children finish learning their letters, and everyone's a little unsettled by it. |

---

## 8. Migration Note — What Changes From the Characters Document

Characters §4 remains structurally correct (three lifecycle-gated categories, opposed pairs, Axis nudges) but its actual trait content and count (115) are now **superseded** by this document. Specifically:

- **Retired as flat pairs, reborn as tiered spectrums (§3):** Quick/Slow → Intellect; Beautiful/Plain → Beauty; Strong/Weak → Physique; Zealous/Impious → Piety.
- **New, with no prior equivalent:** The Four Humors (§3.4), the full Lifestyle & Vocation subsection (§5.3), and every trait listed under §4.2-4.3, §5.2, and §6.6-6.9.
- **Unchanged:** every other trait's name, pairing, and Axis assignment — this document only adds depth and breadth, it doesn't reverse any prior design call.
- **New total: 234 traits** (69 Congenital + 72 Formative + 73 Reactive + 20 tiered-spectrum tags across the five spectrums), well past the CK3-scale breadth requested, with a substantially expanded Combo Title list (§7) to match.

A cosmetic follow-up (swapping Characters §4's own tables for a pointer to this document, the same treatment Politics & Patronage's Notable references already got) is small, optional, follow-up work — flagged in §11.

---

## 9. Cross-System Integration

- **Characters:** this document is now the authoritative trait catalog; §5 (Axes), §6 (Combo Titles' mechanism), §8-10 (resolution, Interaction Catalog, Scheme engine) are all unchanged and simply read this richer trait set instead of the original 115.
- **Familia:** literacy (Well-Read/Illiterate), fidelity (Faithful/Adulterous), and the marriage-market draw of Beauty all give that document's existing mechanics real trait-level texture they didn't have before.
- **Labor & Slavery:** Freed Spirit, Institutionalized, and Cunning Survivor (§6.7) finally give manumission and harsh-Regimen aftermath a proper trait home, distinct from the punishment ladder's own Broken/Defiant endpoint.
- **Economy & Finance:** Debt-Scarred/Debt-Free at Last (§6.8) gives Insolvency's aftermath a lasting personal mark; Merchant's Instinct gives Contract negotiation a real trait-driven edge.
- **Politics & Patronage:** Power-Hungry/Jaded and Assimilated/Unbowed (§6.6) give office-holding and Reputation Duality real Reactive consequences; Piety (§3.5) directly feeds Sumptuary Edict reception exactly as that document already described.
- **Religion (§6.6, future):** Devotee/Apostate, Theologian, Astrologer, and the Piety tier are this document's concrete starting material for that system's eventual pass.
- **Legal & Court (§6.16, future):** Litigious/Conflict-Averse and Legal Scholar are named, ready-made hooks.
- **Military & Combat (§6.7, future):** Inspiring/Feared Commander, Strategist/Berserker, and Gladiator's Heart all give that system real trait material ahead of its own design pass.
- **Games & Spectacle (§6.22, future):** Gladiator's Heart and the Lanista/Editor roles (Companions & Court Positions) now have a personal-trait complement, not just an institutional one.

---

## 10. Data Model

```
Trait {
  name,
  category,          // "congenital" | "formative" | "reactive" | "lifestyle" (§5.3's timing exception)
  spectrum,          // null, or "intellect" | "beauty" | "physique" | "humors" | "piety" for tiered traits
  tierPosition,       // null for ordinary pairs; 0-3 for tiered spectrum members
  opposedTrait,       // null for standalone (Haunted, Ambidextrous, etc.)
  axisNudges: [ { axis, magnitude } ],   // magnitude: "small" | "large", direction per Characters §5's Pole A/B
  bespokeEffect,       // free text — the mechanical consequence independent of axis nudges
  flavorLine,
  minLifecycleStage,   // "infant" (rare) | "child" | "adolescent" | "adult" — §6's loss-trait exception included
  costOverride,        // null for most Lifestyle traits; set for Duelist/Spymaster-style costed exceptions (§5.3)
}

CharacterLifestyleSlots {   // §5.3's cap, tracked per Character rather than as a bare trait list
  characterId,
  activeLifestyleTraits: [...],   // max 3
  lapsedLifestyleTraits: [...],   // retained for flavor/history, no longer mechanically active
}
```

Supersedes the inline `traits: { congenital: [...], formative: [...], reactive: [...] }` array shape sketched in Characters §14 only insofar as each string entry there should now resolve against this richer `Trait` table rather than a bare label.

---

## 11. Open Questions

- **All numeric sizing.** Consistent with this project's convention: exact Axis nudge magnitudes behind "small"/"large," tiered-spectrum roll probabilities, and Lifestyle-trait acquisition thresholds are all unsized.
- **Characters §4 cosmetic pointer.** §8 flags that Characters' own trait tables still show the original 115-trait content; swapping them for a pointer to this document is optional cleanup, not required for correctness.
- **Tiered spectrum roll distribution.** Whether the four rungs of Intellect/Beauty/Physique roll on a flat distribution or a bell curve (most Characters landing on the middle rungs, extremes genuinely rare) isn't specified — a bell curve reads more realistic but isn't decided here.
- **Ambidextrous's rarity weighting.** Flagged as "rare" in §4.2 without a specific roll weight relative to Left-Handed/Right-Handed.
- **Lifestyle trait acquisition trigger.** §5.3 establishes these are earned through "sustained adult practice" without specifying the actual trigger condition (a duty slot held long enough, a specific number of related Interactions, a dedicated player-facing choice).
- **Humor inheritance.** Unlike ordinary Congenital traits, whether a parent's Humor should weight a child's own roll (the same way other Congenital traits already do per Characters §4.4) isn't decided — ancient humorism didn't have a strong hereditary theory to draw on here.
- **Combo Title list size vs. collision frequency.** §7's list has grown to roughly 70 entries across ten themes; whether that's approaching a size where two curated pairs start competing for the same Character often enough to matter isn't tested.
- **Lifestyle Cap's exact number.** §5.3 sets the cap at three as a working default; whether that's the right number, or whether it should scale with age/Intellect tier (a Brilliant, long-lived Character plausibly picking up more real specializations over a longer life than most), isn't tested.
- **Lifestyle lapse mechanism.** §5.3 resolves a fourth acquisition by having an existing Lifestyle trait "lapse from disuse," but doesn't specify whether the player chooses which one lapses, it's the least-recently-exercised one automatically, or some other rule.
- **Long-Lived/Short-Lived Stock's actual lifespan effect.** §4.4 ties this pair into Familia's aging/mortality mechanics without specifying the actual modifier — deferred to a numeric-balancing pass on that document as much as this one.
