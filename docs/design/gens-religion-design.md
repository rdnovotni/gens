# GENS — System Design: Religion (§6.6)
*The Culture & Belief pillar's own entry, upgraded per direction from a pure flavor layer into a real mid-weight system: a single household Favor meter with a chosen Patron Deity, a sacred calendar Religion itself owns, a passive-plus-active Omens/Auspices pair, a lightweight state Priesthood track running alongside Politics & Patronage's magistracy ladder, and a foreign-cult/syncretism mechanic sized to matter without becoming its own religious-simulation subsystem.*

---

## Contents

1. Scope & Role
2. Divine Favor — The Core Meter
3. Household Worship — Lares & Penates
4. Omens & Auspices
5. The Sacred Calendar — Festivals & Feast Days
6. Priesthoods
7. Foreign Cults & Religious Syncretism
8. Piety, Traits & the Zealotry Axis
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

The core doc's own framing named the pieces — "household gods (Lares, Penates), omens, festivals, and priesthoods" — but scoped the whole thing as "mostly a flavor layer." Per direction, this pass keeps the *flavor* (the household gods stay warm and specific, not an abstracted meter with a Latin label bolted on) while giving it a real mechanical spine: a single tracked **Favor** value that actually moves other systems, rather than a background number nobody watches.

This document deliberately doesn't try to be a full theological simulation. It owns three things cleanly: the household's relationship with its own gods and its chosen Patron Deity (§2–3), the omens/auspices decision layer (§4), and the sacred calendar (§5) — while leaning hard on existing machinery elsewhere rather than inventing parallel systems. Priesthoods (§6) extend two roles Companions & Court Positions already named rather than starting over. Foreign cults (§7) get real teeth without needing their own tracked meters. Everything here reads Piety, Zealotry, and the existing Religious trait pool (Traits §7.6) rather than introducing a competing personality layer.

---

## 2. Divine Favor — The Core Meter

**Favor** is a single household-wide value, sitting alongside Dignitas as a second, distinct axis of standing — not a competitor to it. Dignitas is what Rome and one's peers think of the household; Favor is what the gods do. The two usually move together (a pious, well-regarded house tends to read as favored on both counts) but can diverge sharply, and the divergence is exactly where this system gets interesting: a Dignitas-rich, Favor-poor house is one Rome respects and the gods, as far as anyone can tell, do not — a real and unsettling story in its own right, and vice versa for an obscure but devout one.

Favor isn't split into a full per-deity pantheon of separate meters — a deliberate weight decision. Instead, depth comes from **where** Favor is spent and **which** Patron Deity is currently reading it.

### 2.1 Patron Deity

At founding (or through a later, deliberate **Reconsecration** event — see Open Questions) the household selects a **Patron Deity** from the major Roman pantheon: Jupiter, Juno, Mars, Venus, Minerva, Ceres, Neptune, Mercury, Vesta, Apollo, Diana, and Bacchus are all real, viable picks, each with a distinct domain. The Patron Deity doesn't create a second meter — it determines what the single Favor score actually *does*:

- **High Favor** grants a domain-flavored bonus matching the deity: Mars leans Military & Combat (recruitment quality, morale), Ceres leans agricultural yield stability (a real hedge against Estate & Settlement's harvest variance), Venus leans Familia's marriage/romance outcomes, Mercury leans Commerce and trade-route reliability, Minerva leans Education & Culture and Learning-driven actions, Neptune leans naval ventures and a real edge against Piracy & Banditry, Vesta leans household stability (Familia relationship-web resilience, a buffer against Loyalty decay), Jupiter leans broadly at Dignitas and political standing, and so on for the remainder.
- **Low Favor** doesn't just withhold the bonus — it exposes the household to a domain-flavored **Ill Omen** (§4.1) specifically themed to the offended deity: a Mars-patronized house at low Favor risks a military disaster omen; a Ceres-patronized one risks a blight portent; a Neptune-patronized one risks a shipwreck warning. The tradeoff this creates is real and intentional: choosing a Patron Deity is choosing which domain the household is betting its religious life on, for better and for worse.

**Reconsecration** — changing Patron Deity mid-game — is real but rare, and deliberately tied to a natural story beat rather than available on a whim: it opens whenever a new paterfamilias or materfamilias assumes headship of the household (Succession & Dynasty's own succession moment, the cleanest and most historically fitting trigger, since a new head bringing their own patron into the household was a real Roman pattern) or following a major Chronicle-worthy event plausibly attributed to divine intervention one way or the other. Reconsecration itself is a Funded Action — a real ceremony, not a menu toggle — and resets accumulated Favor toward a neutral middle rather than preserving it outright, since the gods being courted have, in a real sense, changed. A household that reconsecrates often reads as fickle to a Traditionalist audience (Politics & Patronage §3.1); one that never does, even across generations, is its own quiet statement of continuity.

### 2.2 Favor Gains

Favor accrues from a genuinely wide set of already-established actions rather than one dedicated grind loop:

- Sustained household worship (§3) performed at a consistent standard.
- A funded festival or feast day (§5) — Economy & Finance's Funded Actions (§4.3 of that doc) already named "religious favor" as one of three things a Funded Action buys, ahead of this document's own pass; this is that payoff realized.
- A correctly-heeded Omen (§4.1) or a well-executed Auspices reading (§4.2).
- Holding a state Priesthood (§6.2) or having a Vestal in the family (§6.3).
- A Chronicle-worthy divine moment — a battle plausibly won "with the gods' favor," a difficult childbirth survived, a founding rite completed without incident.
- A well-maintained Temple/Shrine (Buildings §4.10) with its Incense upkeep (Buildings §4.6) reliably met.

### 2.3 Favor Losses & Divine Displeasure

Favor erodes from neglect and active offense alike: an unmaintained household shrine, an Impious paterfamilias (Traits §3.5 already forfeits this system's bonuses outright for that tier), a broken religious oath (feeding Legal & Court sacrilege cases, §9 below), a scandal involving a priest or Vestal (a direct Dynasty Chronicle Faith & Scandal entry), or an aggressively-pursued foreign cult read as impious by a Traditionalist audience (§7).

At sufficiently low Favor, the household enters **Divine Displeasure** — not a hard failure state (consistent with the "no forced ending" pillar; nothing here can end a game) but a standing vulnerability: Ill Omens (§4.1) trigger more frequently and skew harsher, a Traditionalist-leaning political audience (Politics & Patronage §3.1) reads the household less favorably independent of its actual Dignitas, and Settlement Demographics' Contentment math takes a background hit from a populace that has noticed its patrons seem to have fallen out of favor with the gods. Recovery is always available and always the same shape as recovering from any other neglected standing meter in this project: sustained correct behavior, a well-funded festival, or a strong Auspices outcome pulls it back.

---

## 3. Household Worship — Lares & Penates

The daily, physical layer this system is built on top of, and already partly designed elsewhere — this section consolidates rather than re-invents. The **Lares** (household guardian spirits) and **Penates** (storeroom/pantry gods) are venerated at the household shrine — the Villa doc's **Lararium**, upgradeable to the **Aedicula Lararium** (§4.4 of that doc) for a stronger passive Favor contribution as an architectural statement of piety in its own right, with the **Sacrarium** (Domus stage) available as a dedicated secondary shrine for a personally-chosen deity beyond the generic household gods — the natural physical seat for a Patron Deity distinct from Vesta's default hearth association, or for a foreign cult (§7) kept separate from the main Lararium.

Daily/monthly rites are performed by the household's own **Sacerdos Domesticus** (Companions & Court Positions §5.1) — this document doesn't restate that role's staffing mechanics, only its output: a consistently-tended Lararium under a competent Sacerdos Domesticus is the single most reliable ongoing source of Favor (§2.2), and its neglect (an absent or poor-quality holder of that role) is the single most common quiet drain on it. A household without a dedicated Sacerdos Domesticus doesn't stop worshipping entirely — the paterfamilias or materfamilias performs the rite personally by default, at a lower and more Piety-trait-dependent standard.

A third figure belongs alongside the Lares and Penates for historical completeness, even though it drives no separate mechanic of its own: the **Genius** (for a materfamilias, the **Juno**) — the guardian spirit of the household head personally, distinct from the ancestral Lares and the pantry-guarding Penates, and honored at the same Lararium rather than a separate shrine. Its presence is flavor text and Chronicle color (a household head's own Genius is invoked at their birthday, and its neglect is folded into the same worship standard §3 already tracks) rather than a fourth tracked value — a case where naming the real institution matters more than giving it its own number.

### 3.1 The Rites Budget — A Standing Policy

Per this project's own pillar that every standing-policy mechanic presents real tradeoffs, worship gets one of its own rather than resolving entirely through one-off Funded Actions: the **Rites Budget** is a recurring household policy, set and left running the way Economy & Finance's Tax Policy or Labor & Slavery's Regimen are, at a tier the player chooses — roughly Frugal, Standard, or Lavish — trading an ongoing Incense/Treasury draw against Favor stability. A Frugal budget saves real, recurring denarii but leaves the household more exposed to Favor drift and Omen severity in a lean month; a Lavish one buys a real Favor cushion and a Traditionalist-audience reputation for proper piety, at a genuine, recurring cost that competes with every other line on the Ledger (Economy & Finance §10). Consistent with Policies & Edicts (§6.12, future) being named as the eventual home for standing household policy broadly, this document builds the Rites Budget's concrete mechanics now — the same treatment Economy & Finance's own Funded Actions already received — ahead of that system's own pass.

---

## 4. Omens & Auspices

Per direction, both halves — the passive narrative layer and the active, commissionable action.

### 4.1 Passive Omens

**Omen Events** surface periodically as narrative Events, entirely independent of anything the player commissions: a flight of birds read a certain way, a strange dream, a sudden storm on an inauspicious day. Frequency and severity scale with the household's current Favor (§2.3's Divine Displeasure) and with individual Characters' Zealotry axis and Superstitious/Astrologer traits (Traits §3–4) — a household full of rational, indifferent Characters simply notices fewer of these; a Superstitious or Zealous one sees omens everywhere, for better and worse.

Every Omen Event presents a real choice, never a forced outcome: **heed it** (a concrete, usually modest cost paid now — delaying a departure, calling off a hunt, sparing the expense of a small propitiatory offering) in exchange for averting whatever the omen warned of, or **ignore it** (no cost, but a real chance the omen was accurate and the warned-of consequence lands anyway). An Impious Character (Traits §3.5) is mechanically immune to the penalty for ignoring an omen — the direct payoff for that tier's own "forfeits bonuses" tradeoff — while a Zealous one suffers a real Favor and morale cost for ignoring one even when nothing bad follows, since to a true believer the omen mattered regardless of outcome.

### 4.2 Active Auspices

A genuine, commissionable action rather than a passive wait-and-see: **taking the Auspices** before a major decision — a Military campaign's launch, a long Travel journey, founding a new settlement, a high-stakes marriage — consumes Incense (Resources & Goods) and the time of whoever performs it, and returns an "informed risk" preview in the same spirit as Military & Combat's own Reconnaissance: not a guarantee, but a real skew toward or away from the decision, or in some cases a one-time reroll/insurance against the single worst possible outcome of the action it precedes.

Who can perform a reliable reading matters. A household's own Sacerdos Domesticus can take basic Auspices at a modest reliability; a Character holding the **Augur** office (§6.2) reads them at real, mechanically superior accuracy — the concrete payoff for that office existing at all, and the reason a household serious about this layer has real reason to pursue it rather than treating the office as pure flavor. This mirrors real Roman practice directly: haruspicy (reading the entrails of a sacrificed animal) and augury (reading the flight of birds) were both genuine, actively-commissioned state and household practices, not passive superstition alone.

---

## 5. The Sacred Calendar — Festivals & Feast Days

Per the decision on calendar ownership: **Religion owns the sacred calendar and its feast days; Politics & Patronage and Games & Spectacle own the spectacle logistics** once a feast day calls for either. A recurring set of real Roman observances — Saturnalia, Lupercalia, the Vestalia, Ceres's own Cerealia, and others — sit on the calendar year, each associated with a deity domain and, for some, a particular building (a Ludi-adjacent feast day naturally routes to the Amphitheater or Circus, Games & Spectacle's actual venue and resolution machinery; a purely domestic feast stays entirely inside the Villa).

Each feast day can be observed at two tiers:

- **Passively** — the household marks the day without special expense, a small automatic Favor tick and nothing more.
- **Actively funded** — a genuine **Funded Action** (Economy & Finance §4.3, which already flagged "a religious festival" as a spendable category ahead of this document's own pass), buying a real Favor and Dignitas payoff sized to the spend, plus a Settlement Demographics Contentment boost through the same bread-and-circuses channel that already covers Ludi. A well-funded festival at a Games & Spectacle venue is this system's clearest overlap point with that pillar — Religion supplies the occasion and its Favor payoff, Games & Spectacle supplies the actual event resolution.

A representative, non-exhaustive sample of the calendar's real named observances (the full year-round roster is a natural later-pass task, per Open Questions):

| Feast Day | Real Term | Associated Deity/Domain | Typical Venue |
|---|---|---|---|
| New Year Rites | **Kalends of January** | Janus, household Genius (§3) | Domestic |
| Wolf Festival | **Lupercalia** | Faunus, fertility | Public procession |
| Feast of the Dead | **Parentalia** | Ancestral Lares specifically | Domestic, tomb visits |
| Sowing Festival | **Cerealia** | Ceres | Public, Circus Ludi |
| Vesta's Feast | **Vestalia** | Vesta, the household hearth | Domestic + public Temple |
| Neptune's Feast | **Neptunalia** | Neptune | Public, coastal settlements |
| Harvest Thanksgiving | **Saturnalia** | Saturn | Public, household role-reversal customs |
| Founding-Day Rite | *(household-specific)* | The household's own Patron Deity | Domestic, Chronicle-eligible |

A neglected calendar — feast days consistently skipped rather than even passively observed — is a slow, background Favor drain in its own right, distinct from and additive to the neglect described in §2.3.

---

## 6. Priesthoods

### 6.1 Household Priesthood (Recap)

Already fully staffed by Companions & Court Positions' **Sacerdos Domesticus** (§5.1 of that doc) — this document adds no new household-scale role, only the mechanical output that role now feeds (§2–4 above).

### 6.2 State Priesthood — A Lightweight Parallel Track

Per direction, a mix of a real office track and a deliberately representative (not exhaustive) roster, sized the way Politics & Patronage's own local magistracy ladder was sized: achievable within a single playthrough, running *alongside* that ladder rather than folding into it, since historically a Roman priesthood and a magistracy were related but genuinely separate honors.

Public-scale worship is already anchored by Companions & Court Positions' **Sacerdos Publicus** (§5.2 of that doc, distinct from the household's own Sacerdos Domesticus) — the baseline public temple-keeper role. This document adds two further, real, historically-attested offices above it, gated by the Piety trait tier (Devout or Zealous, Traits §3.5) and Learning rather than by Politics & Patronage's Dignitas/citizenship gate alone, though citizenship still applies per Familia §2.5's own restriction:

- **Augur** — the office named directly in §4.2 above: a Character holding this reads Auspices at superior reliability, both for their own household and, engaged formally, for the wider settlement's own major decisions (a Curia vote on a contested action, a settlement-wide Auspices reading ahead of a Military campaign the whole town has a stake in).
- **Flamen** — a priest dedicated specifically to the household's own Patron Deity (§2.1), historically the arrangement for Rome's major state cults (the *Flamen Dialis* for Jupiter chief among them). Holding this office is the single strongest available multiplier on that deity's own domain bonus — the direct, achievable payoff for committing to a Patron Deity rather than treating the choice as cosmetic.

A capstone **Pontifex** role — a local overseer of religious affairs broadly, standing to the Augur and Flamen roughly as a Duumvir stands to a plain Decurion in Politics & Patronage's own ladder — is the rare, prestige endpoint of this track: real Dignitas and Favor weight, a genuine Dynasty Chronicle entry on attainment, and, per the same logic Politics & Patronage's Clientela sponsorship already uses, a plausible credential contributing to being "noticed by Rome" for that document's own distant cursus honorum goal (§6 of that doc) — a respected priestly family is a real, historically-grounded second door into Roman notice, distinct from Dignitas and Net Worth alone.

A settlement or household without any qualifying family member isn't locked out of Auspices entirely: a genuine, historically-attested **itinerant Haruspex** — traditionally an Etruscan specialist, since Etruria's own divinatory tradition was what Rome itself borrowed the practice from — can be hired for a one-time fee per reading, at a reliability between the household default and a dedicated Augur. This is a real, deliberately-costed alternative rather than a workaround, matching the same hired-specialist pattern Companions & Court Positions already uses for wage-earning free labor: no ongoing office, no Favor-track commitment, just a paid service available whenever the coin is there for it.

### 6.3 The Vestals — A Special Case

A deliberate, narrow, high-prestige exception rather than a generalized mechanic, and real Roman practice played with full historical frankness per this project's own pillar rather than softened. A young, unmarried woman from the Familia can be offered for dedication as a **Vestal Virgin** — a public state institution entirely outside household control once accepted, not a Companions & Court Positions appointment.

Mechanically, dedication is one of the single highest Dignitas and Favor events a household can generate — genuinely rare, genuinely Chronicle-worthy, and a real story beat rather than a background stat bump. It carries real, unsanitized stakes matching the actual institution:

- For the tenure's real historical duration, the Vestal is **exempt from *patria potestas*** — Legal & Court §6 confirms that authority is otherwise near-absolute over a household's dependents, and this is the one formal, legally-recognized carve-out from it — and exempt from marriage and betrothal entirely for the same period.
- The vow carries a **Chastity requirement** enforced with real historical severity: a violation is among the most severe Legal & Court criminal cases available, sitting at that document's own capital-case tier (§9 of that doc), reflecting the real historical penalty without this document needing to depict anything beyond the fact of the case and its outcome — sexual content stays entirely indirect per this project's own content pillar; the mechanic is about legal and dynastic consequence, not depiction.
- Successful completion of the full tenure returns the woman to civilian life with an exceptional Dignitas standing and, historically accurate, real property rights independent of any male guardian for the remainder of her life — a genuinely unusual outcome worth the Dynasty Chronicle entry it generates in its own right. She re-enters Familia's ordinary marriage-candidate pool at whatever age and Core Attributes she's reached by then if she or the household chooses to pursue a (real, if historically less common) late marriage, and Succession & Dynasty's standard eligibility rules for a daughter apply to her exactly as they would to any other — her prior service adds Dignitas and Chronicle weight, not a different inheritance rule.

---

## 7. Foreign Cults & Religious Syncretism

Per direction: primarily a real but secondary mechanic, with room for a heavier moment where it's genuinely earned. The Roman world's actual religious life was syncretic and provincial contact constant — Isis, Cybele (Magna Mater), Mithras, Judaism, and, in a sufficiently late-period setting, early Christianity are all real, attested presences worth representing without turning this document into a comparative-religion simulation.

**Personal devotion** is the light-touch entry point: through a Travel encounter, a trading contact, or a Religious-Specialty Clientela favor (Politics & Patronage §4.2), an individual Character can pick up a personal foreign-cult affiliation — a trait-level tag, not a household-wide shift — carrying a small personal bonus flavored to the cult (Isis leaning fertility/protection, Mithras leaning military camaraderie particularly among veteran Characters, Cybele leaning ecstatic, high-variance Favor swings) at essentially no household-level risk.

**A genuine household shift** — formally adopting a foreign cult as a real household practice, potentially consecrating the Sacrarium (§3) to it, even alongside or in place of the Roman Patron Deity — is the heavier, real-tradeoff version: a distinctive bonus flavor unavailable anywhere else in the Roman pantheon, weighed against genuine Traditionalist Faction backlash (Politics & Patronage §3.1) and a real Dignitas risk with any Traditionalist-leaning audience. Pushed far enough, or in a period/setting where a specific cult reads as *religio illicita*, this becomes the "major mechanic" ceiling the direction allowed room for: a real **persecution risk** — Legal & Court cases brought under exactly that framing, a genuine Event chain distinct from ordinary Sumptuary or criminal cases, and (for the latest-period settings) the specific historical shape of Christian persecution available as a real, unflinching story rather than an implied one. This ceiling is deliberately rare and opt-in through play rather than something an ordinary household stumbles into by accident.

**On stacking affiliations:** personal-tier devotion (the light-touch entry point above) can be held freely and by multiple Characters at once — there's no reason a Mithras-affiliated veteran and an Isis-affiliated wife can't coexist under the same roof, and historically often did. The household-shift tier is capped at one active foreign cult at a time, though it can genuinely coexist *alongside* the Roman Patron Deity rather than replacing it outright (true syncretism, not conversion) — a second household-shift cult pursued simultaneously reads as diluted devotion rather than doubled benefit, and simply isn't offered as an option until the first is either fully abandoned or elevated into permanent, unremarkable household custom.

---

## 8. Piety, Traits & the Zealotry Axis

This document deliberately introduces no new personality layer — it reads the ones Traits and Characters already built:

- **Piety** (Traits §3.5: Impious / Indifferent / Devout / Zealous) is the direct dial on how strongly an individual Character engages everything in this document — Impious forfeits Favor-driven bonuses outright but is immune to Omen penalties for ignoring a warning; Zealous engages hardest in both directions, the strongest Priesthood/Auspices aptitude and the sharpest cost for irreligious behavior.
- **Zealotry** (the Personality Axis, Characters §5: Zealous ↔ Rational) is the hidden behavioral number this document's own Events and Auspices choices weight against — a household of high-Zealotry Characters heeds omens more readily, funds festivals more readily, and reacts more sharply to a Patron Deity's low-Favor Ill Omens.
- **Superstitious** and **Astrologer** (existing Traits) directly raise Omen Event frequency and the weight the game gives to interpreting them; **Theologian** gives real argument-construction weight to Auspices interpretation and to any Legal & Court case with a religious dimension, the same way Legal Scholar already does for ordinary litigation.
- The existing **Religious Combo Titles** (Traits §7.6 — The Weeping Faithful, The False Prophet, The Doubting Priest, and others) require no new mechanics from this document; they're flavor read directly off the trait/axis combinations this system already uses.

---

## 9. Cross-System Integration

- **Companions & Court Positions:** Sacerdos Domesticus and Sacerdos Publicus (§5.1–5.2 of that doc) are this document's household- and public-scale staffing; §6.2's Augur/Flamen/Pontifex track is new here.
- **Villa:** the Lararium, Aedicula Lararium, and Sacrarium (§4.4 of that doc) are this document's physical seats for household worship and any foreign-cult consecration.
- **Buildings & Production Chains:** the Shrine → Temple chain and its Incense upkeep (§4.6, §4.10) are the settlement-scale infrastructure Favor and Auspices both draw on.
- **Economy & Finance:** Funded Actions (§4.3) gets its "religious favor" category actually realized by §5's festival mechanic; Bribes (§4.2) has no direct religious analog, kept deliberately separate.
- **Politics & Patronage:** Faction (§3.1) determines both Sumptuary and foreign-cult reception; a state Priesthood is a second, real credential toward the cursus honorum's "noticed by Rome" gate (§6 of that doc) alongside Clientela sponsorship; a Religious-Specialty client (§4.2 of that doc) is the mechanism behind personal foreign-cult introductions.
- **Legal & Court:** sacrilege and broken religious oaths are a real case type this document feeds; a Vestal's Chastity violation sits at that document's capital-case tier; persecution-framed cases (§7) are a distinct Event chain built on the same Case machinery.
- **Familia:** *patria potestas*'s otherwise-absolute authority (Legal & Court §6) carries its one formal carve-out for an active Vestal; Piety (§3.5) originates in that document's own attribute pass.
- **Settlement Demographics:** the **Aeditui** pop group (temple staff) scales directly with Temple/Shrine presence; Contentment reads both a well-funded festival and sustained Divine Displeasure.
- **Traits & Characters:** Piety, Zealotry, Superstitious/Astrologer/Theologian, and the Religious Combo Titles are all read directly rather than duplicated; no new Personality Axis is introduced.
- **Dynasty Chronicle:** a Vestal dedication, a Pontifex attainment, a religious scandal, or a persecution case are all real Faith & Scandal category entries.
- **Military & Combat:** Auspices taken ahead of a campaign (§4.2) is this document's own version of that system's Reconnaissance pattern; a Mithras affiliation (§7) leans specifically military-flavored.
- **Games & Spectacle:** a Ludi-associated feast day (§5) routes its actual event resolution to that system while Religion keeps ownership of the occasion and its Favor payoff.
- **Travel:** foreign-cult encounters (§7) are a natural Travel-Event category, alongside that system's existing encounter machinery.
- **Rival Houses:** a rival gens carries its own Patron Deity and Favor standing exactly as the player's household does, available as a point of contrast, alliance (shared cult sympathy), or friction (a Traditionalist rival reacting to a Popularist house's foreign cult).
- **Natural Disasters (§6.17, future):** a Ceres-patronized household's blight Ill Omen, a Neptune-patronized household's shipwreck warning, and Divine Displeasure's general vulnerability (§2.3) are this document's own contribution to that system's hazard framing — a portent that precedes, rather than replaces, an actual disaster resolution.
- **Piracy & Banditry (§6.24, future):** a Neptune Patron Deity's high-Favor bonus is named directly in §2.1 as a real edge against that system's maritime threat.
- **Espionage (§6.15, future):** a Vestal's rumored impropriety, a household head's secretly-held foreign cult, or a priest's private impiety are all natural blackmail-material sources once that system exists — the same "uglier leverage" role Espionage already plays for Politics & Patronage and Legal & Court.
- **Policies & Edicts (§6.12, future):** the Rites Budget (§3.1) is this document's own standing policy, built now the same way Economy & Finance's Funded Actions were, ahead of that system's eventual pass gathering all standing household policy under one roof.

---

## 10. Data Model

```
HouseholdReligion {
  householdId,
  patronDeity,          // "jupiter" | "juno" | "mars" | "venus" | "minerva" | "ceres" |
                          // "neptune" | "mercury" | "vesta" | "apollo" | "diana" | "bacchus"
  favor,                 // §2 — single scalar, the core meter
  favorTrend,            // rolling direction, read by Omen frequency/severity (§4.1)
  divineDispleasure: bool,   // §2.3 — derived from favor crossing the low threshold
  ritesBudgetTier,        // §3.1 — "frugal" | "standard" | "lavish", a standing policy
  reconsecrationHistory: [   // §2.1
    { fromDeity, toDeity, triggeringEvent: "successionChange" | "chronicleEvent", month }
  ],
  foreignCultAffiliations: [
    { cultId, tier: "personal" | "householdShift", consecratedSacrarium: bool }
    // householdShift tier capped at one active entry at a time — §7
  ],
}

OmenEvent {                // §4.1
  eventId, householdId, month,
  themedDeity,            // usually the Patron Deity, occasionally a foreign cult
  severity,
  playerChoice,           // "heeded" | "ignored" | null (not yet resolved)
  outcome,                // "averted" | "consequenceLanded" | "noConsequence"
}

AuspicesAction {            // §4.2
  actionId, householdId, month,
  performedByCharacterId,     // null if performedByHiredHaruspex is true
  performedByHiredHaruspex: bool,   // §6.2 — the itinerant, per-reading alternative
  precedingDecisionType,      // "militaryCampaign" | "travel" | "settlementFounding" | "marriage" | other
  incenseSpent, feeSpent,      // feeSpent only set for a hired Haruspex reading
  reliabilityTier,           // Sacerdos Domesticus (base) < hired Haruspex (mid) < Augur officeholder (superior)
  resultSkew,                // the informed-risk output fed to the preceding decision's own resolution
}

Festival {                 // §5
  festivalId,
  name,                    // "Saturnalia" | "Lupercalia" | "Vestalia" | "Cerealia" | ...
  associatedDeity,
  month,
  observanceTier,          // "passive" | "funded"
  fundedActionRef,          // links to Economy & Finance's FundedAction record when observanceTier is "funded"
  venueBuildingRef,         // null for purely domestic feasts; Amphitheater/Circus/Theatre for Ludi-linked ones
}

PriesthoodOffice {           // §6.2, mirrors Politics & Patronage's own office-record shape
  officeId,
  officeType,              // "sacerdosPublicus" | "augur" | "flamen" | "pontifex"
  holderCharacterId,
  settlementId,
  flamenDeity,             // set only for "flamen" — matches the household's own Patron Deity
}

VestalRecord {              // §6.3
  characterId,
  dedicationMonth,
  tenureLength,             // unsized — see Open Questions
  vowStatus,                // "active" | "completedHonorably" | "violated"
  patriaPotestasExempt: bool,   // true for the full active tenure
}
```

---

## 11. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Favor gain/loss magnitudes, the Divine Displeasure threshold, Omen frequency/severity curves, Auspices reliability deltas across Sacerdos Domesticus/hired Haruspex/Augur, Vestal tenure length, and the Rites Budget's three-tier cost/stability curve are all unsized.
- **Historical time-period scope for persecution.** §7's heaviest ceiling (Christian persecution specifically) depends on where in Roman history a given playthrough is set, which this document doesn't itself define — a broader project-level question about the game's supported time range rather than a gap unique to Religion.
- **Feast day roster completeness.** §5's table is a representative, real-and-attested sample rather than a committed exhaustive year-round calendar — a natural follow-up pass once balancing begins, likely alongside whichever system ends up owning the master game calendar (if any exists beyond month-ticks).
- **Reconsecration frequency limits.** §2.1 ties Reconsecration to a succession change or a major Chronicle event, but doesn't specify whether a single reign can reconsecrate more than once, or whether a cooldown applies beyond the natural rarity of qualifying trigger events.
- **Household-shift foreign cult abandonment.** §7 notes a second household-shift cult isn't offered until the first is "abandoned or elevated into permanent custom," but doesn't specify what abandonment itself costs in Favor or Traditionalist standing, versus simply lapsing from neglect.
- **Whether a hired Haruspex can be retained on ongoing contract** rather than paid per-reading, blurring toward a Companions & Court Positions wage-position — currently scoped as strictly per-reading (§6.2) to keep the household-role/hired-specialist line clean, but worth revisiting if playtesting shows the per-reading fee becomes a friction point.
