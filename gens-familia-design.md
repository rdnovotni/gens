# GENS — System Design: Familia (§6.1)
*The household roster and its people — the load-bearing system beneath nearly everything else.*

---

## 1. Scope & Role

Familia defines every individual the game tracks with a full stat block: family, enslaved household members, freedmen, clients, and companions. Appearance/Portraiture (§7.11), Labor & Slavery (§6.3), Romance & Seduction (§6.19), Companions & Court Positions (§6.20), Succession & Dynasty including Adoption (§6.9), and Education & Culture (§6.14) all read and write these same records — Familia is the schema they all share, not a separate silo.

---

## 2. Stat Architecture

Every tracked individual has stats at three layers: **Core Attributes** (the primary drivers of most systems), **Labor Skills** (for manual duty assignment), and a much larger set of **Detailed/Body Attributes** (granular, mostly feeding Appearance and flavor rather than being read directly by other systems) — resolving the brief's call for an extremely detailed model without every system having to reason about dozens of numbers.

### 2.1 Core Attributes (five, CK3-derived, numeric 0–100)

The same five attributes apply to *everyone* — family, slave, freedman, client, or companion — so any person can be evaluated identically for a marriage, an appointment, or a mission, regardless of status:

| Attribute | Feeds primarily |
|---|---|
| **Diplomacy** | Politics & Patronage, Romance & Seduction, Diplomacy with Non-Roman Peoples |
| **Martial** | Military & Combat, Games & Spectacle (as a fighter), personal security |
| **Stewardship** | Estate & Settlement, Economy & Finance, Settlement Demographics |
| **Intrigue** | Espionage, Legal & Court (building a case, concealing one) |
| **Learning** | Education & Culture, Religion, Legal & Court (arguing one), Disease (as a physician) |

This resolves the earlier "broad skills vs. one-per-system" question directly: these five *are* the per-system mapping, while flavor labels like "Rhetoric" or "Piety" are read as Diplomacy- or Learning-flavored description rather than separate tracked numbers — one clean stat block instead of a duplicated one.

### 2.2 Labor Skills (small set, numeric 0–100, relevant mainly to enslaved/laboring individuals)

A second, smaller set governs manual duty assignment and doesn't meaningfully apply to a magistrate or a senator's wife: **Fieldwork, Domestic Service, Craft, Culinary, Medicine (lay/practical, distinct from Learning-driven formal medicine)**. This is the axis Household Duties (§4) reads from, keeping "who's a good field hand" mechanically separate from "who'd make a good spymaster."

### 2.3 Condition Stats (numeric 0–100, universal)

- **Health** — current physical condition; feeds Disease vulnerability, death risk, and work capacity.
- **Fatigue/Overwork** — accumulated strain; high fatigue depresses Health regeneration and (for the enslaved) feeds Labor & Slavery's unrest math.
- **Loyalty** — opinion of this person specifically toward the player; the single most-read stat for compliance across every system that asks someone to do something.
- **Ambition** — how much this person wants more than their current station; low-ambition people are content, high-ambition people press for marriages, offices, or freedom, or become succession-drama risks if thwarted.
- **Fertility** — age/sex-gated; feeds §6 below.

### 2.4 Detailed/Body Attributes (dozens, mostly write-only for other systems)

This is where the "Free Cities-level granular" requirement actually lives, and it's deliberately **the same dataset the Appearance system (§7.11) already defines** rather than a second invented list: height and build, facial structure, complexion, hair/eye color and style, notable features, and status-appropriate dress/grooming. Other systems don't read these directly (a marriage negotiation doesn't query "nose shape"), but they drive the generated portrait and occasionally gate a specific trait or event (a notable scar prompting a "How did you get that?" flavor beat, poor build interacting with Health under hard labor).

### 2.5 Legal Status (categorical, five values, each with real mechanical differences)

| Status | Mechanical distinctions |
|---|---|
| **Roman Citizen** | Full legal rights: can own property, marry other citizens, testify in court, hold office if male and of sufficient class |
| **Latin Rights** | Limited citizenship (common in provincial contexts) — can trade and contract but not fully vote or hold the highest offices; a plausible upgrade path to full citizenship |
| **Peregrine** | Free non-citizen (a conquered or allied people's member) — most restricted free status; local customary law often applies instead of Roman law |
| **Freedman** | Formerly enslaved, now free, but bound by *obsequium* — ongoing obligations of respect and service to their former owner (the player), who remains their patron; can't hold the highest offices; children of freedmen are born fully free |
| **Enslaved** | No independent legal personhood; subject to *patria potestas*-adjacent ownership rather than family law; manumission is the only path out |

Legal status is orthogonal to **Social Class** (a separate, citizen-only categorical: Senatorial, Equestrian, Plebeian) which gates political ceiling in Politics & Patronage rather than basic rights.

### 2.6 Personality Traits (target pool: 100+, CK3-style)

Three categories, matching the earlier hybrid decision:

- **Congenital traits** — rolled at birth with some inheritance weight from both parents (a trait either parent holds has an elevated chance of passing down); represent innate temperament (*Quick*, *Slow*, *Craven*, *Brave*) and tie into the Appearance system where relevant (*Beautiful*, *Plain*, *Scarred*).
- **Formative traits** — acquired during Childhood/Adolescence based on upbringing and Education investment (*Zealous*, *Trusting*, *Cynical*, *Studious*) — this is Education & Culture's main hook into Familia.
- **Reactive traits** — gained or lost during adulthood based on treatment and events (*Resentful*, *Content*, *Vengeful*, *Grateful*, *Drunkard*) — this is Labor & Slavery's, Romance's, and Events' main hook: how a person is actually treated leaves marks.

Traits can be mutually exclusive within a pair (can't be both *Brave* and *Craven*) but are otherwise tags rather than a single spectrum slider, keeping them legible as badges in the UI (per the existing visual design's trait-badge treatment).

### 2.7 Relationship Web (CK3-style, between named individuals only)

Every named individual (see §7 on scale) can hold a tracked relationship toward every other *relevant* named individual — not just toward the player. Each relationship carries:
- A numeric **opinion** value (-100 to 100).
- One or more **bond tags**: family relations (Parent/Child/Sibling/Spouse — these are structural, not opinion-based, and persist regardless of opinion), earned bonds (*Friend*, *Rival*, *Lover*, *Patron/Client*, *Mentor/Student*), and **Contubernium** — the informal, legally-unrecognized union standing in for marriage among the enslaved (defined fully in the Labor, Slavery & Punishment doc, §9), tracked the same way as any other bond but without the legal weight of a formal Spouse tag.

This web is what makes a marriage proposal, a scheme, or a succession dispute involve more than one number — a rival claimant's odds depend on *their* relationship to the rest of the family, not just the player's opinion of them.

### 2.8 Naming Conventions

Names follow the full Roman convention rather than a flavor-only approximation:

- **Male citizens** carry the *tria nomina*: **praenomen** (personal name, drawn from a small traditional pool — Marcus, Gaius, Lucius, etc.), **nomen** (the gens name — shared by the whole family, and the game's own "Gens Aurelia" naming layer), and **cognomen** (a branch/personal identifier, sometimes inherited, sometimes descriptive).
- **Female citizens** typically take the feminized **nomen** alone (a daughter of the Aurelii is "Aurelia"), with a numeral or descriptor added when needed to distinguish sisters (*Aurelia Prima*, *Aurelia Secunda*), consistent with actual Roman practice.
- **Freedmen**, on manumission, take their former owner's **praenomen and nomen** and retain their prior name as a **cognomen** — mechanically, this is the one moment a person's full name changes in play, and it's worth surfacing as a small ceremonial beat (and a Chronicle entry) rather than a silent field update.
- **Enslaved individuals** and **peregrini** carry a single name (often origin-flavored — Numidian, Gallic, Syrian naming pools matching §2.5's origin culture) until and unless manumission or a citizenship grant changes that.

### 2.9 Sex-Based Social Restrictions

The game defaults to **historical accuracy**: the *paterfamilias* role, the highest political offices, and most inheritance-privileging mechanics are male-only by default, matching the setting rather than softening it. Two things sit alongside that default rather than replacing it:

- **Same-sex relationships** exist as their own track within Romance & Seduction (§6.19), independent of the heir-producing marriage market — this reflects real (if socially complex) space in Roman elite culture and gives those relationships mechanical weight without requiring them to route through marriage/legitimacy mechanics that don't historically apply to them.
- **A player-configurable toggle**, set alongside the fertility/childbirth-risk toggle (§6) at game start, can relax these restrictions for players who'd rather not play within them — consistent with treating historical frankness as the default rather than the only option.

---

## 3. Lifecycle & Aging

Five stages, each gating different available actions and events:

| Stage | Age (approx.) | What opens up |
|---|---|---|
| **Infant** | 0–3 | No stats beyond Health; primary gameplay surface is Disease/mortality risk |
| **Child** | 4–12 | Formative traits begin forming; Education investment starts; no labor, no marriage |
| **Adolescent** | 13–17 | Betrothal and marriage negotiations can begin (historically accurate, played with the same frankness as the rest of the design); Labor Skills training/duty assignment becomes available; Formative traits continue solidifying |
| **Adult** | 18–59 (roughly) | Full participation: all duties, Court Positions, Military service, Companion recruitment, Romance & Seduction, political office |
| **Elderly** | 60+ | Core Attributes and Health gradually decline; wisdom-flavored traits become more likely; succession pressure and death risk both rise; retirement from active duties becomes an available choice rather than a forced one |

**Death** is deliberately unrestricted in cause, consistent with the "fully open" decision: old age (elderly-stage probability), disease (§6.13), childbirth (§6 below), violence (Military §6.7, Legal §6.16 executions, Espionage §6.15 assassination), and disaster (§6.17) can all end a life. Familia doesn't gate which systems are allowed to kill someone; it just guarantees every death has somewhere to register (Chronicle, Succession, the relationship web of everyone who knew them).

### 3.1 Permanent Injury & Disability

Not every serious harm should be recoverable. Alongside the recoverable Health stat (§2.3), a person can acquire **permanent injuries** — a lasting, non-healing modifier rather than a temporary dip: a lamed leg from a battlefield wound, a maiming from a workplace or punishment accident, blindness from disease, a difficult birth's lasting toll on the mother. Mechanically, a permanent injury applies a standing penalty to a relevant Core Attribute, Labor Skill, or Fertility rather than to Health itself (which continues to recover normally around it), and is visible on the character record and in the Appearance system's rendered portrait (a scar, a limp implied in posture) rather than being a hidden number. Sources feed in from wherever the design already allows harm: Military & Combat, Labor & Slavery's punishment mechanics, Natural Disasters, and childbirth alike.

---

## 4. Household Roles & Duties

Two distinct tiers, split cleanly along the two skill axes from §2:

- **Labor duty slots** (Field Hand, Domestic Servant, Cook, Groundskeeper, Craftsman, Tutor's Aide, etc.) are drawn from **Labor Skills** and open to any eligible Adolescent-or-older household member regardless of status. This is what "Assign Duty" in the prototype represents.
- **Court Positions** (Steward, Marshal, Spymaster, Court Physician, Bodyguard — full detail belongs to §6.20's own design pass) are drawn from the **Core Attributes** instead, and are explicitly the higher tier: appointing a Steward reads Stewardship, a Spymaster reads Intrigue, and so on. A single person could plausibly hold a labor duty early in life and graduate into a Court Position later as their Core Attributes and reputation develop.

---

## 5. Marriage & Family Formation

Both approved directions coexist as genuinely different paths rather than one absorbing the other:

- **Arranged marriage:** calculated from dowry offered, alliance value (the other house's standing and usefulness), and family prestige — the transactional model from the original core doc. Layered on top is a **consent/happiness factor**, derived from the couple's pre-existing relationship-web opinion (if any) and personality-trait compatibility (e.g., two *Zealous* partners of the same cult read as compatible; a *Cynical* partner reduces the receiving end's initial happiness). Low consent doesn't block the marriage — Roman marriage didn't require it — but it depresses long-term happiness and raises the odds of Romance & Seduction's affair mechanics triggering *against* the marriage rather than in support of it.
- **Love-match:** the player can instead let or encourage a relationship to develop organically (via the relationship web reaching a high mutual opinion, often through Romance & Seduction's relationship track) and then formalize it. This typically carries a *lower* guaranteed dowry/alliance value than a well-negotiated arranged match, but starts with high happiness and a stronger relationship-web bond — a real political-security-vs-domestic-harmony tradeoff, not a strictly dominant option either way.

Adoption (§6.9) uses the same Core Attribute and relationship-web evaluation as a marriage candidate would — the game treats "who joins this family" as one underlying question with two different doors in (marriage, adoption).

### 5.1 Divorce

Marriage isn't a permanent lock-in. Consistent with the comparative ease of elite Roman divorce, either the player or an NPC spouse can initiate one — politically (the alliance the marriage was built on has soured, or a *better* alliance has appeared) or personally (persistent low relationship-web opinion, an affair surfaced through Romance & Seduction, sustained low consent/happiness from §5's arranged-marriage math). Divorce carries real consequences rather than a clean reset: dowry return/retention terms, a Dignitas hit sized to how the divorce is perceived (amicable and mutual vs. scandalous), and a lasting relationship-web scar between the two families involved — which the Rival Houses system can act on.

### 5.2 Legitimacy

A child's **legitimacy** is tracked explicitly rather than assumed: children born within a recognized marriage are legitimate by default; children resulting from an affair (Romance & Seduction) are not, unless the *paterfamilias* explicitly acknowledges and legitimizes them — a deliberate, visible choice with its own social cost (a Dignitas risk, a relationship-web hit from the betrayed spouse) rather than a quiet toggle. Legitimacy status directly gates default eligibility in Succession & Dynasty (§6.9): illegitimate children aren't barred from ever inheriting, but require the same explicit intervention (acknowledgment, or the Adoption mechanic) that a legitimate heir doesn't need.

---

## 6. Fertility & Childbirth

- Fertility (a Core Condition stat, §2.3) combined with an active marriage/relationship determines pregnancy chance per relevant time tick.
- Pregnancy and childbirth carry **real, period-appropriate stakes** by default: a health cost during pregnancy, and a genuine risk of death at childbirth for the mother (moderated by Health, and improved by Learning-driven medical care, tying into Education & Culture and the Court Physician position) and a separate risk for the infant.
- A **player-configurable toggle** (set at game start alongside the other difficulty/content choices) lets a player dial this down to a more abstracted, lower-risk mode without removing the fertility system entirely — consistent with treating this as a legitimate accessibility axis rather than softening the setting's default frankness.

---

## 7. Scale & Abstraction Rule

Every member of the player's own **Familia** — family, household slaves, freedmen, clients, and companions — always keeps a full stat block, however large that group grows. This is a deliberate, uncapped commitment for the household the player actually manages.

**Unnamed background population** (once Estate & Settlement grows into a *vicus* or town) does *not* automatically get full stat blocks — that population is Settlement Demographics' (§6.26) responsibility, tracked in aggregate. A background colonist can be "promoted" into a full Familia record if the player specifically interacts with them (hiring them into a duty slot, a marriage proposal involving them, a notable event singling them out) — at which point they join the fully-tracked roster like anyone else.

The Rival Houses system (§6.10) follows the same rule for other gentes: their own pater/materfamilias and any member the player's household actually interacts with (a marriage candidate, a political rival, a spy's target) gets a full record; the rest of their household stays abstracted.

---

## 8. Data Model Sketch

For continuity with the existing prototype's `people` array, a fuller record looks roughly like this (illustrative, not final field names):

```
{
  id, praenomen, nomen, cognomen, sex, age, lifecycleStage,   // identity & age, §2.8
  legalStatus, socialClass,              // §2.5
  coreAttributes: { diplomacy, martial, stewardship, intrigue, learning },
  laborSkills: { fieldwork, domestic, craft, culinary, medicine },
  condition: { health, fatigue, loyalty, ambition, fertility },
  permanentInjuries: [...],              // §3.1
  traits: { congenital: [...], formative: [...], reactive: [...] },
  appearance: { ...detailed attributes per §7.11 },
  relationships: { [otherId]: { opinion, bonds: [...] } },
  maritalHistory: [ { spouseId, startDate, endDate, endReason } ],  // §5, §5.1
  legitimacy,                            // §5.2, relevant to children only
  role: dutySlotOrCourtPosition,
  originCulture
}
```

---

## 9. Open Questions Carried Forward

- **Exact trait list.** The 100+ pool is scoped by category (§2.6) but not yet enumerated — a natural next task, possibly its own short reference doc rather than part of core Familia design.
- **Inheritance weighting formula.** How strongly a congenital trait's presence in a parent should raise the child's odds of rolling it isn't yet specified numerically.
- **Consent/happiness formula.** §5 establishes the inputs (prior relationship opinion, trait compatibility) but not the actual weighting or resulting happiness curve.
- **Fertility/childbirth toggle granularity.** Whether the configurable toggle is a simple on/off or a multi-step slider (as with several other content-intensity choices already flagged) is undecided.
- **Promotion threshold from Settlement Demographics.** What specifically triggers a background person becoming a full Familia record needs concrete trigger conditions once §6.26 gets its own pass.
- **Restriction-toggle granularity.** Whether the historical-restrictions toggle (§2.9) is a single on/off switch or covers each restriction (offices, inheritance, paterfamilias role) independently is undecided.
- **Divorce consequence tuning.** §5.1 establishes the inputs (dowry terms, Dignitas hit, relationship scar) but not their actual magnitudes or how "scandalous vs. amicable" gets determined.
- **Legitimization cost formula.** §5.2 establishes that acknowledging an illegitimate child carries a Dignitas/relationship cost, but not its size relative to an ordinary divorce or scandal event.

*Verna (household-born) vs. purchased-slave origin was considered as a distinct flag during this pass and set aside; general origin culture (§2.5-adjacent) still applies to enslaved individuals.*
