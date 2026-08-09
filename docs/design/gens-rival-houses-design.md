# GENS — System Design: Rival Houses & the Living World (§6.10)
*The system three separate documents have been quietly building toward — Characters' generated-rival pattern, Politics & Patronage's contested elections and client-poaching, Military & Combat's unused Private Feuds deployment type all resolve here. Per direction, this document also generalizes past gentes alone into a shared framework for every non-player actor the world needs: guilds, foreign peoples, pirate confederations, cult institutions. A prior pass added new houses rising to balance extinction, made the player's own house an explicit, unprotected target of rival ambition, gave the standing tiers real mechanical teeth, added pre-contact regional visibility, and wired in Espionage and Correspondence & Letters. This final pass adds Cadet Branches as a second, real path for a new house to rise, resolves rival-house succession by simply reusing Familia/Succession & Dynasty's own rules, introduces Ancestral Grudges so a severe Feud can outlive the individuals who fought it, ties Traits' Combo Titles into a Head's Dossier entry, and gives Dossiers a deliberate staleness for houses gone quiet.*

---

## Contents

1. Scope & Role
2. The Tiered Simulation Model
3. Anatomy of a Rival House
4. Rival Ambition — How Houses Act
5. The Player's Relationship With a Rival House
6. Beyond Gentes — Other Organizations & Forces
7. Legibility — The Rival Dossier
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role

The core doc's own definition is direct: "other gentes with their own holdings, ambitions, and family trees, advancing on their own simulated timeline, competing for the same marriages, offices, patronage, and land. Their fortunes stay legible without requiring player micromanagement." Design Pillar #6 states the same thing as a first-class commitment: "Rival houses pursue the same land, offices, marriages, and prestige on their own initiative, whether or not the player engages them."

This document is almost entirely composition rather than invention — the pieces already exist:

- **Familia §7** already wrote this system's core rule before it existed: "their own pater/materfamilias and any member the player's household actually interacts with... gets a full record; the rest of their household stays abstracted." This document is where that rule actually gets built out.
- **Characters** already built the universal schema, the Interaction Catalog, the Scheme engine, and lazy instantiation — a rival house's members are simply Characters, generated exactly the way any other Character is.
- **Politics & Patronage** already resolves elections and client-poaching against a generated placeholder, explicitly flagged to "swap for a real Rival House record" once this document existed.
- **Military & Combat** already built Private Feuds as a full deployment type with no real opponent to use it against.

Per direction, this document also does one genuinely new thing: it generalizes the tiered actor model past gentes specifically into a shared framework any non-player organization can use — trade guilds, foreign peoples, pirate confederations, cult institutions — rather than each of those future systems (Diplomacy with Non-Roman Peoples, Piracy & Banditry, Religion) reinventing the same "how much do we actually simulate this" question independently.

---

## 2. The Tiered Simulation Model

Per the decision to keep this tractable: simulating every rival gens with full player-level fidelity would be both wasteful and pointless — most of them will never matter to a given playthrough. Two tiers, with a clean, automatic rule for moving between them.

### 2.1 Background Houses — The Default

The vast majority of gentes in the world. Tracked as a lightweight record: a name, a rough standing tier (Rising/Established/Declining), an approximate Net Worth band, a Dignitas band, and a single dominant **Identity** tag for flavor (see §3.3). Their fortunes evolve through periodic abstract rolls — a rough simulation of births, deaths, marriages, and fortune shifts — rather than a full parallel economy or politics tick. A Background House's Head is not generated as a full Character until something requires it.

**The standing tier isn't just flavor.** A **Rising** house is the one likelier to actually initiate contact — contesting a plot, entering an election, courting a marriage candidate — since its own abstract rolls skew toward growth and ambition; an **Established** house is stable background scenery more often than not, and correspondingly the most reliable Ally to actually have (§5.2) — not desperately expanding into the player's own interests, and not weak enough to be a liability if a Feud does arise; a **Declining** house is the natural target for absorption (§5.3) and a softer mark for a Feud or a contested claim, the world's own version of blood in the water. This is what actually makes the tier worth reading in a Dossier (§7) rather than a cosmetic label.

### 2.2 New Houses Rising

The world needs to replenish, not just thin out through extinction (§5.3) — a Background House roster that only ever shrinks stops feeling alive well before a long playthrough ends. Two real paths, not one:

- **The *novus homo* path** — a sufficiently wealthy, sufficiently distinguished Curiales-tier family (Settlement Demographics' own upper pop-group ceiling) can be promoted into a genuine new Background House in its own right — the celebrated rags-to-riches arc Politics & Patronage §6 already names, now given its logical endpoint: enough success doesn't just raise one man's own Dignitas, it can found a house his descendants inherit.
- **The cadet branch path** — a sufficiently large, successful existing house can split. A Head with multiple adult sons, each with real Ambition and no single inheritance to satisfy all of them (Familia's own division-of-inheritance mechanics), is the natural trigger: a younger son takes a distinguishing *cognomen*, a share of the parent house's Holdings, and founds a new, separately-tracked LivingWorldActor — related in name and origin, but its own gens from that point forward, exactly the real historical pattern behind cases like the Cornelii Scipiones splitting from the wider Cornelii. A cadet branch starts with a soft positive relationship-web baseline toward its parent house, not a neutral one, though nothing prevents that baseline eroding into genuine rivalry over time the same as any other relationship would.

### 2.3 Houses of Note — Full Depth

Any Background House is promoted the instant real player contact occurs — a marriage candidate drawn from it, an election opponent, a Private Feud, a contested plot, a poached or poaching Clientela relationship, a Ransom negotiation. This is the exact same principle Characters §11's lazy instantiation already uses for individuals, applied one level up, at the household scale. A House of Note gets:

- A full Head Character, generated per Characters §11 the moment they're needed.
- A tracked Household Identity (§3.3) read consistently rather than re-rolled each time.
- Real, evolving Holdings and Net Worth (§3.4), driven by actual events rather than an abstract band.
- Appearances in Events and the Dynasty Chronicle with genuine narrative texture rather than a one-line mention.

### 2.4 Demotion

A House of Note that goes a long stretch without any live thread connecting it to the player — no active Feud, no outstanding marriage negotiation, no contested claim — simply stops receiving extra-fidelity tracking rather than requiring an explicit "downgrade" action. Its last-known state (Head, Identity, approximate Net Worth) freezes into the Background tier's lighter format. This keeps the "of Note" set from growing without bound over a long playthrough, the same practical concern Characters' own lazy-instantiation model was built to manage.

---

## 3. Anatomy of a Rival House

### 3.1 The House Record

A name (a *nomen*, often with a distinguishing branch *cognomen* — "the Aemilii Scauri"), a current Head (the pater/materfamilias equivalent, a Character), a Dignitas figure tracked the same way the player's own is, a Net Worth figure (Economy & Finance §8's schema, at whatever fidelity the tier above warrants), an abstracted Military Strength figure (a House of Note can have this resolved down to real Squads per Military & Combat §2 if a Private Feud actually requires it; a Background House's is just a number), and a home settlement or region.

### 3.2 Household Members Are Characters

Restating Familia §7's founding rule now that it has somewhere to actually apply: the Head, plus anyone the player's household specifically interacts with — a marriage candidate, a rival for an office, a Feud's opposing commander — becomes a full Character via Characters §11's lazy instantiation. The rest of a rival house's household stays an unnamed abstract headcount, exactly as the player's own Settlement Demographics population does below the individually-tracked line.

**Succession isn't a separate NPC algorithm.** When a House of Note's Head dies of natural causes — age, illness, anything short of the house going fully extinct (§5.3) — the new Head is determined by exactly the same inheritance rules Familia and Succession & Dynasty (§6.9, future) define for the player's own household, run against whichever heir the deceased Head actually had. This document doesn't need, and deliberately doesn't build, a parallel NPC-specific succession system; a rival house's family tree obeys the same rules the player's own does, because it's the same kind of thing.

### 3.3 Household Identity

Two tags, both already fully designed elsewhere and simply reapplied at the house level rather than invented fresh:

- **Economic Identity** — Agrarian, Mercantile, Industrial, or Martial (Estate & Settlement §6's existing category) — what the house is actually built on.
- **Faction** — Traditionalist or Popularist (Politics & Patronage §3.1) — how the house tends to lean politically and socially.

These describe the *institution*, not necessarily its current Head — a Martial-Identity house currently led by a Peace-Loving, Studious pater is a real, interesting tension (a family built on the legion led by a man who'd rather be reading), not a contradiction the system needs to resolve.

### 3.4 Holdings & Net Worth

A House of Note's Net Worth is calculated the same way Economy & Finance §8 already calculates the player's own — Treasury-equivalent, goods, land/building value, net debt — just without necessarily needing the full room-by-room granularity the player's own Villa gets. A Background House's is a band (roughly comparable to a specific player Net Worth range) rather than a computed figure, since nothing has yet demanded the precision.

---

## 4. Rival Ambition — How Houses Act

### 4.1 Goals & Initiative

A rival house's behavior is driven by exactly the mechanism Characters §8.3 already built for any Character acting on their own initiative: the Head's Ambition (Condition stat), relevant Personality Axes (Boldness, Greed, Zealotry), and the house's own Identity tags (§3.3) together determine what it actually pursues. A Martial-Identity house with a Bold, high-Ambition Head pushes toward land and military prestige; a Mercantile-Identity house with a Greedy Head pushes toward trade dominance and Clientela reach. No new AI framework is invented here — this document simply points the existing one at the household scale. **Worth stating plainly: the player's own household is never a special case here.** A sufficiently Bold, Ambitious rival can just as easily set its sights on the player's own land, marriage candidates, or Clientela as on any other house's — the same ambition-generation §4.2's list runs on doesn't distinguish the player as off-limits or specially protected. The player is simply the one house in the simulation whose responses aren't automated.

### 4.2 Competing for the Same Things

Named directly by the core doc, and each already has a real mechanism waiting:

- **Marriages** — a rival house's eligible members are genuine competing candidates in Familia's marriage market; the player can lose a desired match to a faster or better-positioned rival, not just to narrative flavor.
- **Political Office** — Politics & Patronage's contested elections (§5.5 of that doc) are now populated by real Houses of Note rather than a generated placeholder.
- **Patronage** — the Clientela poaching mechanic (Politics & Patronage §4.5) runs both directions between real houses now.
- **Land** — Estate & Settlement §7's contested-plot mechanic ("a rival gens can outbid or petition for the same plot") is this document's concrete supplier of the actual rival doing the contesting.

### 4.3 Rival-vs-Rival Dynamics

Per direction: rival houses genuinely compete with *each other*, not only with the player, and this runs whether or not the player is watching. Two Background or Houses of Note can end up in their own marriage alliance, feud, or election contest with no player involvement at all — surfacing as a background Events or Dynasty Chronicle beat ("the Aemilii and the Cornelii have come to blows over grazing rights") that colors the world's texture and can create real opportunities: a rival weakened by someone else's feud is easier prey, and two houses allying against a common rival — possibly the player — is a real threat worth noticing rather than something the simulation hides.

---

## 5. The Player's Relationship With a Rival House

### 5.1 Interactions & Overtures

No new interaction verbs — a Rival House's Head and notable members are Characters, so Characters §9's entire Interaction Catalog already applies directly: Befriend, Broker an Alliance, Endorse/Undermine a Candidate, the full Coercive/Intrigue category, Propose Marriage, Declare a Feud, all of it.

### 5.2 House Standing — Alliance & Feud

A single tracked state per house-to-house relationship — **Allied, Neutral, Rivalrous,** or **Feuding** — sitting above the individual relationship-web opinions between specific members (Politics & Patronage's Faction concept, applied at the house level rather than the individual one). Feuding specifically is what authorizes Military & Combat §6's Private Feuds deployment type between the two houses' Forces; Allied unlocks joint ventures (a shared marriage alliance, a joint political bloc at the Curia).

**A real emergent scenario worth naming:** an Allied house entering a Feud with some third party — another rival, per §4.3's own rival-vs-rival dynamics — doesn't automatically drag the player in, but it does present a genuine choice through the ordinary Interaction Catalog rather than a forced event: honor the alliance and join the cause, stay neutral and risk the relationship, or even use the moment to quietly side with the third party instead. Nothing about §5.2's Standing model forces an answer; it just guarantees the situation is legible enough (via §7's Dossier) that the player actually notices it's happening.

**Multi-generational grudges.** Ordinary Feuding Standing is tied to the two current Heads' own relationship-web opinions, and would naturally soften once both eventually change — which undersells how a truly severe engagement should actually feel. A Feud that resolves in a Catastrophic Defeat (Military & Combat §4.5), a battlefield death, or an execution rather than a Ransom (Characters §9.5) leaves an **Ancestral Grudge** — a standing modifier on the *house-to-house* relationship itself, independent of whichever individuals currently hold each Head role, that decays far slower than an ordinary opinion and can keep two houses reflexively Rivalrous for generations after everyone who actually remembers the original offense is dead. This is the mechanism behind the real, CK3-familiar feeling of two dynasties that have simply always hated each other, without needing either side's current Head to have personally done anything to deserve it.

### 5.3 Absorption — Marriage, Inheritance, and Extinction

Estate & Settlement §7 already named the paths a second settlement most naturally arrives by: "marriage absorption, a legal ruling, conquest, or a rival's extinction leaving their land unclaimed." This document adds the mechanism behind the last one: a house goes **extinct** when its line runs out entirely — no viable heir, the specific failure case Succession & Dynasty (§6.9, future) will need to define in full. Per the decision to resolve this case-by-case rather than with one universal rule: an extinct house's unclaimed holdings are picked up through whichever system actually fits the specific circumstance — a Legal & Court (§6.16, future) ruling if there's a legal claim to argue, a Politics & Patronage land grant if Rome is disposing of it as a reward, or Military & Combat conquest if it's simply taken. This document doesn't adjudicate which applies when; it just confirms extinction is the trigger and hands the actual resolution to whichever system already owns that kind of claim.

---

## 6. Beyond Gentes — Other Organizations & Forces

Per direction, the tiered actor model (§2) generalizes past gentes into a shared **Living World Actor** framework — the same Background/Note tiering, the same "a leader is just a Character" principle, reused rather than reinvented by each system that needs a non-player organization.

### 6.1 Collegia — Trade Guilds

A real, historically attested Roman institution: professional associations (a shipping *collegium*, a builders' *collegium*) that function like a cross-house bloc within a single sector — Economy & Finance §3.2's Contracts can be negotiated with a Collegium directly, Politics & Patronage's elections can carry a Collegium's informal endorsement, and a Commerce-heavy player competes with organized guild interests as much as with any single rival house. Tracked with the same Background/Note tiering as a gens.

### 6.2 Foreign Peoples & Petty Kingdoms

Diplomacy with Non-Roman Peoples (§6.25, future) owns the actual depth here — treaties, tribute, alliance against Rome itself, all named directly in the core doc — but this document supplies the shared model that system should build on rather than invent separately: a tribal leader or petty king is simply a Character, and their people function as a Living World Actor at whatever tier the player's actual contact with them warrants.

### 6.3 Pirate & Bandit Confederations

Piracy & Banditry (§6.24, future) gets the same treatment. Military & Combat §5.1 already generates a pirate captain or bandit chief as a full Character the moment the player actually faces one; a House-of-Note-tier confederation adds a loose Force (Military & Combat §4.1's Irregular type), a hideout location, and a standing reputation on top of that one-off encounter, rather than every pirate raid being a fully disconnected event.

### 6.4 Religious Institutions

A major cult or priesthood (Religion, §6.6, future) as a background actor with its own influence and standing — courtable through Economy & Finance's Funded Actions or a household's own Piety (Traits §3.5), and itself trackable at Background or Note tier exactly like a gens.

### 6.5 Rome Itself

Deliberately not a new actor to design here: Politics & Patronage's Dignitas-with-Rome axis and the wider Reputation Duality split already model Rome as the setting's own background super-actor. This section exists only to confirm that existing model *is* Rome's entry in this framework, not a second, competing one.

---

## 7. Legibility — The Rival Dossier

The concrete answer to "fortunes stay legible without requiring player micromanagement": any tracked Living World Actor, Background or Note tier, has a **Dossier** — Name, Head/leader, Identity tags, Dignitas, Net Worth (figure or band, per tier), Military Strength, current Standing with the player, and recent notable Chronicle entries involving them. Where a full Head Character exists, their Combo Title (Traits §7) is the Dossier's natural headline flavor — "led by a Venal Magistrate" or "led by a Beloved Patron" tells the player more in three words than a stat block would. This is the same automation-plus-readable-summary pattern this project uses everywhere else (Economy & Finance's Ledger, Military & Combat's Battle Report) — the player's actual point of contact with most rival houses is reading this, not simulating them personally.

**A Dossier isn't omnisciently live.** For a House of Note the player is in direct, active contact with, it reflects the current state; for one that's fallen quiet, its information is only as fresh as the last actual contact or piece of correspondence (Correspondence & Letters, §6.27, future) brought back — a house a player hasn't dealt with in years might have genuinely changed Standing, Head, or fortune since the Dossier was last updated, without the player being automatically informed. This is a deliberate small dose of realism rather than a gap: it's what makes a returning rival occasionally worth a surprise, and it's precisely why §7's own "Notable Families" list stays deliberately shallow rather than trying to be a real-time feed.

**Before first contact:** a Dossier's full depth is earned by §2.3's promotion trigger, not available upfront — but a player shouldn't have zero visibility into a region's world before literally colliding with it either. A lighter **"Notable Families of the Region"** list — names, standing tier, and Identity tag only, no Dossier depth — is ambient, discoverable flavor (naturally surfaced through Travel, Events, or eventually Correspondence & Letters, §6.27) rather than a full browsable directory. It's enough to make the world feel populated and give the player something to recognize when a name finally does show up as a marriage candidate or an election rival, without undermining §2's whole point by pre-generating full depth nobody's contacted yet.

---

## 8. Cross-System Integration

- **Familia:** §7's founding rule for rival houses is fully realized here, not just restated.
- **Characters:** the entire schema, Interaction Catalog, and lazy-instantiation rule are reused wholesale rather than duplicated — a Rival House member is a Character, full stop.
- **Politics & Patronage:** contested elections (§5.5) and Clientela poaching (§4.5) are now populated by real houses; §3.3's Faction is reused directly for house-level Identity; §2.2's new-house-rising mechanism gives that document's own *novus homo* story (§6) its logical endpoint — founding a house, not just personally reaching high office.
- **Military & Combat:** Private Feuds (§6) finally has a real, named opponent on the other side; §4.1's Irregular Combatant type is what a pirate confederation's Force actually looks like.
- **Estate & Settlement:** §7's contested-plot mechanic and second-settlement absorption path both get their concrete rival-house supplier here.
- **Economy & Finance:** Net Worth (§8) is reused directly for house-level wealth comparison, at whatever fidelity a given house's tier warrants.
- **Villa:** the Grandeur Score's own cross-reference to "comparative flavor against Rival Houses" is realized directly — a House of Note's Holdings (§3.4) is the comparable figure.
- **Succession & Dynasty (§6.9, future):** extinction (§5.3) is this document's concrete trigger condition for that system's own inheritance-failure edge case; §3.2's succession rule commits this document to reusing that system's inheritance resolution wholesale for any House of Note's Head, rather than a parallel NPC-only algorithm.
- **Traits:** a Head Character's Combo Title (§7 of that doc) is this document's natural Dossier headline; Cadet Branches (§2.2) reuse Familia's own inheritance-division logic rather than inventing a separate NPC split rule.
- **Legal & Court (§6.16, future):** one of §5.3's three case-by-case resolution paths for an extinct house's holdings.
- **Dynasty Chronicle (§6.11, future):** rival-vs-rival dynamics (§4.3), absorptions, and Feud outcomes are all natural milestone-catalog material generated independently of the player.
- **Espionage (§6.15, future):** the core doc names Rival Houses directly as one of that system's two intended feed targets ("blackmail material... feed[ing] Politics and Rival Houses directly") — a spy placed within a House of Note's own household is this document's concrete recipient of that material, feeding §5.1's Interaction Catalog options against them.
- **Correspondence & Letters (§6.27, future):** the natural mechanism behind §7's pre-contact "Notable Families" visibility and behind learning of a distant rival's marriage, death, or scandal without needing to be physically present for it.
- **Settlement Demographics:** resolves that document's own open question directly — a rival gens's investment within a shared settlement does grow Background Economic Capacity, using this document's Holdings figure as the input; §2.2's new-house-rising mechanism is the concrete payoff of that document's own Curiales upward-mobility ceiling.
- **Diplomacy with Non-Roman Peoples (§6.25, future) / Piracy & Banditry (§6.24, future) / Religion (§6.6, future):** all three inherit the Living World Actor framework (§6) as their shared foundation rather than building their own tiering model independently.
- **Events (§6.8, future):** rival-vs-rival beats and extinction/absorption moments are concrete content for that system's own random/scripted event pool.

---

## 9. Data Model

```
LivingWorldActor {          // §6 — the generalized supertype: a gens, a Collegium, a foreign people, a pirate confederation, a cult
  actorId,
  actorType,          // "gens" | "collegium" | "foreignPeople" | "banditConfederation" | "religiousInstitution"
  name,
  tier,               // "background" | "noteworthy"
  standingTrend,       // "rising" | "established" | "declining" — §2.1's mechanically-live tier, not pure flavor
  originStory,          // "ancient" | "novusHomo" | "cadetBranch" — §2.2, set once at creation, read by Chronicle/flavor text
  parentHouseActorId,    // set only if originStory is "cadetBranch"
  identityTags: { economic, faction },     // §3.3 — reused Estate & Settlement/Politics & Patronage tags
  headCharacterId,      // null until first needed, per lazy instantiation
  dignitas,
  netWorth: { figure, band },     // figure populated only at "noteworthy" tier
  militaryStrength: { figure, band, resolvedForceId },   // resolvedForceId only if a Feud actually requires real Squads
  region, homeSettlementId,
}

HouseStanding {          // §5.2 — the house-to-house relationship, distinct from individual Character opinions
  actorAId, actorBId,
  standing,           // "allied" | "neutral" | "rivalrous" | "feuding"
  ancestralGrudge: { active: bool, originEngagementId, decayRate },   // §5.2 — survives a change of Head on either side
}

RivalDossier {         // §7 — the readable summary
  actorId,
  summary,
  headComboTitle,        // Traits §7 — the Dossier's natural headline flavor, where a Head Character exists
  lastUpdatedMonth,       // §7 — staleness marker; not assumed current for a quiet House of Note
  recentChronicleEntries: [...],
}

RegionalFamiliesEntry {    // §7 — the lighter, pre-contact visibility list
  actorId, name, standingTrend, identityTagEconomic,   // deliberately shallow — no Dossier depth until §2.3 promotion
}
```

---

## 10. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Background House roll frequency/formula, promotion/demotion thresholds, and Net Worth band boundaries are all unsized.
- **Total Background House count per region.** Not specified — how many gentes should populate a given region's world before it feels either sparse or unmanageably crowded.
- **Rival-vs-rival event frequency.** §4.3 establishes these happen without sizing how often, or how visible a purely background rival-vs-rival beat should be to a player not directly involved.
- **Collegia's actual mechanical depth.** §6.1 sketches the concept; the real Economy & Finance/Politics & Patronage integration is left for a dedicated pass, likely alongside those systems' own eventual revisits or a Collegia-specific one.
- **Multi-settlement rival presence.** Whether a single House of Note can hold territory spanning multiple player-relevant settlements, or is always scoped to one, isn't decided.
- **Whether Rome itself can ever be a Feuding Standing.** §6.5 treats Rome as already modeled via Reputation Duality; whether that axis should ever escalate to something resembling §5.2's Feuding state (open revolt) is left to Politics & Patronage's own future territory rather than decided here.
- **New-house-rising trigger threshold.** §2.2 establishes both the *novus homo* and cadet-branch paths without specifying what "sufficiently distinguished" or "sufficiently large" actually require numerically.
- **Standing-trend roll bias.** §2.1's Rising/Established/Declining tiers are now stated to skew a house's abstract-roll behavior, but the actual weighting isn't sized.
- **Ancestral Grudge decay rate.** §5.2 establishes this decays "far slower" than an ordinary opinion without sizing the actual curve, or how many generations a typical grudge realistically persists.
- **Dossier staleness threshold.** §7 establishes information can lag for a quiet House of Note without specifying how long "quiet" needs to run before the Dossier is meaningfully out of date.
- **Cadet branch inheritance split specifics.** §2.2 points at "Familia's own division-of-inheritance mechanics" for how much Holdings a splitting son takes with him; the actual split ratio is left to that system's own numeric pass.
