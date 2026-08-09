# GENS — System Design: Policies & Edicts (§6.12)
*The one system nearly every other document has been quietly building toward without ever formalizing: Economy & Finance's Tax Policy, Labor & Slavery's household Regimen default, Religion's Rites Budget, and Politics & Patronage's Sumptuary Edict were all explicitly flagged as living here "once that system exists." This pass gives them a single canonical roof, closes every flagship gap nothing had designed yet, adds a genuine Edicts mechanic with real Faction-driven backlash, and layers a full Free Cities-style emergent identity system on top: Household Doctrine, where a house's accumulated policy choices resolve into one of seven real, historically-grounded societal paths — each with its own Emerging bonus, Defining capstone, and a rare, generation-spanning Apex tier that rewards a dynasty for staying the course across a real succession. Twelve Standing Policies, eight Edict types, eight Funded Action categories, named hybrid Doctrine titles, and a Playbook system for actually living with this much customization comfortably.*

---

## Contents

1. Scope & Role — Three Categories, Plus an Emergent Fourth
2. Standing Policies — The Canonical Roster
3. Household Doctrine — Emergent Identity Paths
4. Funded Actions — Canonical Categories
5. Edicts — One-Off Proclamations With Teeth
6. Policy Playbooks — Presets & Fast Switching
7. The Policy Screen — Player-Facing Frame
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — Three Categories, Plus an Emergent Fourth

The core doc's own definition names two things in one breath: "standing, revisable household/estate policies (slave treatment, tenant taxation, military recruitment) plus one-off funded actions (games, festivals, public works) for prestige, patronage, or religious favor." Design Pillar #8 sharpens the stakes: "Laws, edicts, funded public events, and standing household policy are lasting, revisable choices with real tradeoffs, not one-off prompts." Read against the direction to give this system real *Free Cities*-style depth, that's actually **four** distinct shapes working together:

- **Standing Policies** (§2) — continuous, always-on dials. Twelve of them now: the four mechanics other documents already built (Tax Policy, the Regimen default, the Rites Budget, the Sumptuary Edict), the two this document's first pass introduced (Recruitment Doctrine, Annona Provision), the four its second pass added (Trade Openness, Patronage Generosity, Education Investment, Provincial Administration Posture), and two more this pass closes out (Marital Diplomacy Posture, Frontier Security Posture).
- **Funded Actions** (§4) — a one-off spend for an immediate, contained payoff. Eight categories now.
- **Edicts** (§5) — a rare, dramatic, single-stroke proclamation with lasting effect and real escalating risk. Eight types now, spanning from General Amnesty's mercy to Proscription's naked power.
- **Household Doctrine** (§3) — the identity layer that actually delivers "shape your villa and settlement the way *Free Cities* shapes an arcology": seven emergent societal paths, each solidifying from sustained policy patterns rather than a menu pick, each now carrying a full three-stage arc (Emerging → Defining → Apex) that rewards not just reaching an identity but *keeping* it across a real generational succession.

**A naming note worth repeating:** Politics & Patronage's own **Sumptuary Edict** predates this document and, despite its name, is mechanically a Standing Policy (§2.4) — a toggled restriction left on or off, not a one-time proclamation. The functional test this document uses throughout is simple: **does it sit toggled, or does it fire once?**

---

## 2. Standing Policies — The Canonical Roster

The first five subsections are consolidation. The rest are new — six of the twelve were designed specifically across this document's own passes, purpose-built to widen the customization space the direction asked for.

### 2.1 Tax Policy *(recap — Economy & Finance §5)*

Vectigalia (indirect, Commerce transaction volume) and Decuma (direct, Coloni harvest/urban rent) rates, gated on holding the local Quaestorship.

### 2.2 Household Regimen Posture *(recap — this document's first pass)*

Mirrors Labor & Slavery's own four axes — Diet (Meager/Adequate/Generous), Accommodation (Bare/Basic/Comfortable), Permitted Freedoms (Confined/Restricted/Free Movement), Discipline Strictness (Lenient/Firm/Harsh) — one level above that document's group-level defaults. The hierarchy resolves in three levels: Household Posture (this document, the seed value) → Group defaults (live-inherit until explicitly set) → Individual overrides (always win). Changing the household dial never overwrites a deliberately-set group or individual value.

### 2.3 Rites Budget *(recap — Religion §3.1)*

Frugal / Standard / Lavish, trading Incense/Treasury draw against Divine Favor stability.

### 2.4 Sumptuary Edict *(recap — Politics & Patronage §8)*

Restricts luxury-good display by Legal Status/Social Class tier, available at Decurion or above.

### 2.5 Recruitment Doctrine *(recap — this document's first pass)*

**Source Doctrine:** Volunteer-Only / Levy-Dilectus / Slave-Militia Reliance (the last locked to an active Crisis flag, auto-reverting after). **Recruitment Intensity:** Conservative / Standard / Aggressive.

### 2.6 Annona Provision *(recap — this document's first pass)*

A simple on/off Standing Policy: a modest, continuous Horreum draw-down maintaining a Contentment floor for Operarii and Coloni. The larger, crisis-scale Grain Dole lives in §4.

### 2.7 Trade Openness *(recap — this document's second pass)*

Protectionist (higher tariffs, real Piracy-exposure insulation, slower Negotiatores growth) / Balanced / Open Markets (faster Negotiatores growth and pricing, real Piracy exposure and Traditionalist friction).

### 2.8 Patronage Generosity *(recap — this document's second pass)*

Frugal (cheap, Clientela opinion and Loyalty drift down) / Standard / Lavish (real recurring cost, faster Influence and Loyalty growth, faster roster growth).

### 2.9 Education Investment *(recap — this document's second pass)*

Minimal / Household-Only / Broad Investment — the last the fastest Core Attribute/Labor Skill growth household-wide, at Labor & Slavery's own named systemic cost: a household-wide rise in flight-planning and Unrest-organizing capacity, not just a per-individual risk.

### 2.10 Provincial Administration Posture *(recap — this document's second pass, frontier-only)*

Exploitative (Treasury/Dignitas-with-Rome up, local standing down) / Balanced / Assimilationist (local standing and Diplomacy head-start up, extraction down). Inert off-frontier.

### 2.11 Marital Diplomacy Posture *(new)*

Familia's own marriage math (§5 of that doc) already weighs dowry/alliance value against a consent/happiness factor for every individual match, but never had a standing household lean toward which side of that scale the family generally favors — this closes it:

| Tier | Effect |
|---|---|
| **Alliance-Maximizing** | Every new arranged-marriage negotiation defaults toward maximizing dowry and alliance value; a real, felt reduction in typical starting consent/happiness household-wide, and a correspondingly higher baseline risk of Romance & Seduction's affair mechanics triggering against a marriage built this way |
| **Balanced** | Familia's own existing arranged-marriage math, unmodified — the default |
| **Compassionate** | Negotiations weight consent and pre-existing relationship-web opinion more heavily, even at a real, quantifiable cost to typical secured dowry/alliance value; meaningfully stronger baseline marital happiness and a lower affair-risk floor across the household |

### 2.12 Frontier Security Posture *(new)*

Distinct from Recruitment Doctrine (§2.5), which governs *where manpower comes from* — this governs *how the Estate Force the household already has is postured*, closing a real gap Military & Combat and the still-undesigned Piracy & Banditry (§6.24) both quietly assume a household has some standing answer to:

| Tier | Effect |
|---|---|
| **Fortify** | Estate Force weight skews toward passive defense — City Walls-style investment emphasis and real, elevated resistance to Piracy & Banditry raids and security-relevant Natural Disaster exposure — at a steady Wages/Treasury cost and reduced Squad availability for offensive deployment or Muster elsewhere |
| **Patrol** | The balanced default — active interdiction against trade-route threats specifically, moderate cost, no strong pull either way |
| **Minimal Garrison** | Frees the largest share of Squads for campaign use or Muster, lowest standing cost, but a real and meaningfully elevated vulnerability to raids and a reduced local Unrest-suppression capacity |

---

## 3. Household Doctrine — Emergent Identity Paths

Per direction, this is the system's own answer to *Free Cities*' arcology-shaping societies: an identity that *emerges* from accumulated policy pattern, the same way Politics & Patronage's own household Faction already does at smaller scale ("a slow-moving reflection of accumulated choices... rather than a one-time pick") — widened here across every policy and Edict this document owns, and given, new this pass, real generational stakes.

### 3.1 How a Doctrine Solidifies

Each of the seven Doctrines carries its own hidden **Affinity score** (0–100), read monthly against the household's actual Standing Policy settings and Edict history. Matching choices raise Affinity; contradicting choices lower it; unfed Affinity decays slowly on its own — reflecting sustained practice, not one good month.

Three thresholds now, not two:

- **Emerging** — visible on the Policy Screen and in Chronicle framing; a modest, immediate flavor bonus.
- **Defining** — regional recognition as a real exemplar; a genuine Chronicle-worthy Dignitas event; unlocks the Doctrine's unique **capstone**.
- **Apex** *(new)* — the rare, generation-spanning tier described in §3.3, reserved for a dynasty that doesn't just reach Defining but keeps it.

**Doctrines are not mutually exclusive, but they are not free to stack either.** Several pairs actively suppress each other because the underlying policy choices genuinely pull opposite ways (heavy Sumptuary enforcement and aggressive Open Markets both feed real Doctrines while quietly working against each other, the same real tension actual elite Roman anxiety about trade reflected). The player is never locked out of a combination, but drifting toward one has a real, felt cost to at least one other.

### 3.2 The Seven Doctrines

**Mos Maiorum — The Old Blood.** Built from sustained Sumptuary enforcement, a Lavish Rites Budget, Volunteer-Only recruitment, a Traditionalist Faction lean. *Emerging:* a modest Traditionalist-audience Dignitas bonus. *Defining capstone — Ancestral Sanction:* once per generation, overturn a Legal & Court ruling against the household without the usual political cost.

**Res Publica Popularis — The Popularist Reformer.** Built from issuing Tabulae Novae or General Amnesty, Lavish Patronage Generosity, frequent Funded Ludi, a Popularist Faction lean. *Emerging:* faster Clientela roster growth. *Defining capstone — Reformer's Momentum:* every future Edict's Influence/Dignitas issuance cost is permanently reduced.

**Domus Mercatoria — The Mercantile Dynasty.** Built from Open Markets, sustained Commerce-building investment, low Vectigalia. *Emerging:* improved regional Market Dynamics pricing. *Defining capstone — Trade Concession:* a standing near-monopoly advantage in one chosen regional good.

**Domus Bellatrix — The Military Aristocracy.** Built from sustained Levy/Aggressive Recruitment, Harsh Discipline Regimen, victory-funded Ludi. *Emerging:* a modest Estate Force Wages discount. *Defining capstone — Call to Arms:* a one-time Muster drawing beyond the household's normal Veterans-pool and squad-cap limits.

**Domus Pia — The Pious House.** Built from a Lavish Rites Budget, frequent funded Festivals, a Devout-or-Zealous Piety lean, holding a state Priesthood. *Emerging:* a modest Divine Favor generation bonus. *Defining capstone — The Great Rite:* a one-time, Edict-scale ceremony granting a major Favor and Dignitas surge.

**Domus Provincialis — The Frontier Syncretist.** Built from an Assimilationist Provincial Administration Posture, real foreign-cult engagement, active Diplomacy with Non-Roman Peoples. *Emerging:* a modest local-standing (Reputation Duality) bonus. *Defining capstone — Foederati Pact:* a standing alliance with a named non-Roman people.

**Domus Dura — The Exploiter House.** The dark path this project's own "frank harshness" pillar calls for, played straight. Built from a sustained Bare/Confined/Harsh Regimen, leaning into Slave-Militia Reliance whenever a Crisis allows it, at least one Proscription issued. *Emerging:* a real labor-output ceiling bonus beyond any other Doctrine's. *Defining capstone — Iron Hand:* the single highest sustained labor-output multiplier in the project — genuinely double-edged, arriving with a **permanent** Unrest/flight-risk/Legal-scrutiny baseline increase that doesn't recede even if the household's policies later soften.

### 3.3 Apex Tier — Doctrines Across Generations

Reaching Defining is already rare. **Apex** is rarer still, and deliberately tied to Design Pillar #7 — "Memory has weight" — rather than to anything achievable within a single lifetime. By default, Doctrine Affinity carries over across a succession event at a real but partial reduction, reflecting institutional momentum a new head inherits without having personally earned. **Apex specifically rewards the new head who chooses to continue matching the same policy pattern for a further sustained stretch after taking over**, rather than merely coasting on an inherited number — a dynasty proving the identity is real, not just its last holder's personal habit. This is the direct, mechanical answer to this document's own open question about Doctrine inheritance: continuity is rewarded, drift is not punished beyond the natural decay Affinity already carries, and nothing about succession itself resets a Doctrine outright.

Each Apex reward is strictly more narratively and mechanically significant than its Defining capstone:

- **Mos Maiorum Apex — "A Name Rome Remembers."** The household's own name now carries a passive Dignitas floor that never fully collapses, even after a real scandal.
- **Res Publica Popularis Apex — "The People's House."** Popularist-audience Contentment and Dignitas reception becomes permanently elevated, regardless of any single Edict's own Reception swing — the reform legacy outlives any one reformer.
- **Domus Mercatoria Apex — "A Trading House of Note."** The Trade Concession extends automatically to a second regional good, and the household becomes a real, named node other houses seek out in Resources & Goods' regional trade network.
- **Domus Bellatrix Apex — "A Line of Commanders."** Every eligible household member starts with a real, permanent bonus toward Martial-leaning traits and Combat Resolution inputs — bred and raised for command across generations.
- **Domus Pia Apex — "Blessed Across Generations."** Divine Favor never fully bottoms out, even during genuine Divine Displeasure — the gods remember a house's long devotion through a bad stretch.
- **Domus Provincialis Apex — "Two Peoples, One House."** The Foederati Pact extends automatically to a second non-Roman people, and the Reputation Duality local-standing axis gains a permanent floor.
- **Domus Dura Apex — "The Name Every Slave Fears."** The Iron Hand labor bonus increases further still — but its permanent Unrest/flight-risk/Legal-scrutiny penalty compounds with it, a further step down the same road rather than a plateau. The darkest path gets darker the longer a dynasty stays committed to it, never resolving into a clean win state.

### 3.4 Hybrid Doctrines — Named Combinations

Consistent with Traits' own Combo Title treatment (§7 of that doc), several commonly-co-occurring Doctrine pairs earn their own named flavor once both sit at Emerging or above — no new mechanics beyond a small combined bonus, purely the same "the whole reads as more than its parts" instinct the Combo Title system already established:

| Doctrine Pair | Combined Title | Flavor |
|---|---|---|
| Mos Maiorum + Domus Pia | **Keepers of the Rite** | Tradition and piety, indistinguishable in this house. |
| Mos Maiorum + Domus Bellatrix | **The Old Guard** | Fights the way the ancestors fought, and sees no reason to stop. |
| Res Publica Popularis + Domus Mercatoria | **Merchant Princes of the People** | Reform funded by trade wealth — the equestrian tension, made into a single house's whole identity. |
| Domus Provincialis + Domus Mercatoria | **Lords of the Wide Roads** | A house that made the frontier's own trade routes its fortune. |
| Domus Bellatrix + Domus Dura | **The Iron Legion House** | Grim efficiency in both the field and the ergastulum — a potent, compounding, and deeply feared combination. |
| Domus Pia + Domus Provincialis | **The Syncretic Faithful** | Roman rite and foreign cult, practiced in the same breath without apparent contradiction. |

---

## 4. Funded Actions — Canonical Categories

Eight categories now, spanning three passes:

| Funded Action | Payoff | Home system |
|---|---|---|
| **Ludi (Games)** | Dignitas, Settlement Demographics Contentment | Games & Spectacle (§6.22, future) |
| **Festival** | Divine Favor, Dignitas | Religion §5 |
| **Public Works** | Dignitas, a boosted civic building output | Estate & Settlement / Buildings |
| **Grain Dole** | A large, immediate Contentment spike, drawing hard on Horreum reserves | Settlement Demographics §6.3 |
| **Colonization Grant** | Funds a new wave of Coloni or Veteran settlers onto available frontier land | Estate & Settlement / Settlement Demographics §8.1 |
| **Triumphal Dedication** | Funds a Triumphal Arch or comparable victory monument after a Decisive Victory | Buildings §4.12 / Military & Combat §3.3 |
| **Plague Relief** *(new)* | A one-off spend during an active outbreak, reducing mortality and speeding recovery — the health-crisis sibling to Grain Dole's famine-crisis shape | Disease & Public Health (§6.13, future) |
| **Dowry Subsidy** *(new)* | A one-off Treasury contribution boosting a specific upcoming marriage's alliance value directly, without waiting on organic wealth accumulation — a deliberate spend rather than Marital Diplomacy Posture's (§2.11) standing lean | Familia §5 / Politics & Patronage |

---

## 5. Edicts — One-Off Proclamations With Teeth

Eight types now, running the full range from mercy to naked power.

### 5.1 The Edict Shape

**Declaration** (a real, immediate Dynasty Chronicle entry), **Effect** (the concrete change), **Reception** (a genuine backlash chain reading Faction, affected pop groups/Curiales/Rival Houses, and severity — capable of escalating into a Scheme, Legal & Court case, or Private Feud). Every Edict costs real Influence and Dignitas to issue; Reformer's Momentum (§3.2) is the one named cost reduction.

### 5.2 Tabulae Novae — Debt Cancellation

Forgives active DebtRecords for a chosen scope. **Reception:** Contentment spike among the relieved, sharp creditor-class fury. Feeds Res Publica Popularis.

### 5.3 General Amnesty

Pardons standing Legal & Court sentences. **Reception:** relationship-web repair and Popularist Dignitas gain, a real Legal & Court credibility cost. Feeds Res Publica Popularis.

### 5.4 Land Redistribution

Reallocates large landholdings toward Coloni or Veterans — the Gracchi-model reform. **Reception:** severe Curiales backlash, real Rival House grievance, genuine escalation risk to a Legal case or Private Feud. Feeds Res Publica Popularis; suppresses Mos Maiorum.

### 5.5 Manumission Edict

A sweeping, single-stroke mass-freeing of enslaved workers. **Effect:** labor disruption, a very large Favor/Dignitas gain. **Reception:** sharp Traditionalist and fellow-slaveholder backlash. Feeds Res Publica Popularis and Domus Pia; strongly suppresses Domus Dura.

### 5.6 Citizenship Grant

Extends citizenship or Latin Rights to a group or individual — real Social-War-era stakes. **Effect:** rapid Assimilation and Loyalty gain for the target. **Reception:** Traditionalist alarm and a plausible Legal & Court challenge to the grant's own validity. Feeds Domus Provincialis and Res Publica Popularis.

### 5.7 Proscription

The single darkest Edict available, modeled directly on Sulla's and the Second Triumvirate's real proscriptions. Declares a named Rival House or Character an outlaw, stripping legal protection and seizing assets in one stroke. **Reception:** the most severe available, including a genuine **demonstration effect** — every regional Rival House shifts toward Wary or Hostile, not just the target. Gated behind Duumvir-or-above or an active civil-crisis Event. Feeds Domus Dura heavily — on its own, close to sufficient for that Doctrine's Emerging threshold.

### 5.8 Debt Bondage Ban *(new)*

The mercy-side mirror to Proscription's naked power: a formal proclamation outlawing new debt-bondage acquisitions (Labor & Slavery §2) for the household going forward. **Effect:** closes that acquisition avenue outright until a future Edict reopens it; a real Popularist/Compassion-axis Dignitas gain and a Divine Favor tick. **Reception:** real but moderate — Traditionalist and economic friction from anyone who relied on that acquisition channel, a modest ongoing labor-supply cost rather than a sharp one-time backlash. Feeds Res Publica Popularis and Domus Pia; suppresses Domus Dura.

### 5.9 Grain Requisition *(new)*

The harsh mirror to Grain Dole: rather than funding relief, the household **seizes** a portion of Coloni's own harvest and stores — the real, historically resented *annona militaris* practice that provincial farmers actually lived through. **Effect:** an immediate Treasury or military-supply injection, sized well beyond what an ordinary tax collection would yield in the same month. **Reception:** a sharp Coloni Contentment drop and real, elevated Unrest risk — this is extraction, not administration, and it reads that way. Feeds Domus Bellatrix and Domus Dura directly; actively suppresses Res Publica Popularis.

---

## 6. Policy Playbooks — Presets & Fast Switching

A deliberate quality-of-life layer answering a real risk of this document's own success: twelve Standing Policies, eight Edict types, and seven Doctrines is a genuinely large space, and a large space that's tedious to navigate stops feeling like customization and starts feeling like homework. A **Playbook** is a named, saved snapshot of the household's full current Standing Policy configuration (§2) — "Frugal Frontier Governor," "Old Money Idle Years," "Wartime Footing" — that the player can recall in a single action rather than resetting every dial by hand. Recalling a Playbook changes the dials only; it never directly grants or subtracts Doctrine Affinity, which keeps reading actual sustained behavior exactly as §3.1 describes — a Playbook makes a deliberate pivot toward a given Doctrine's underlying pattern fast and comfortable to execute, without letting the player shortcut the "sustained" part of *sustained pattern*. Playbooks are also the natural object a future Steward/Council Auto-Management pass (§6.28) would hand off to an acting steward during Travel or a succession interregnum — a household running on a saved Playbook rather than improvised defaults.

---

## 7. The Policy Screen — Player-Facing Frame

The core doc's own Structural Skeleton named the destination directly: "Rules/policy sliders → **Policies & Edicts** — standing, revisable household and estate law." This screen is now three panels deep: every Standing Policy (§2) as a live dial, organized into logical clusters (Labor & Household, Fiscal & Trade, Military & Security, Diplomatic & Social) rather than one long undifferentiated list; the Funded Action and Edict menus (§4–5); and a **Doctrine panel** showing every Doctrine at Emerging or above as a real, visible progress state through all three tiers, plus any earned Hybrid title (§3.4) displayed the way a Combo Title already renders elsewhere in this project. A Playbook selector (§6) sits alongside the Standing Policy panel specifically, since that's the only category it touches. A player checking this screen sees, at a glance, not just what's set where, but what those choices have actually made the household *into* — Design Pillar #8's legible-tradeoff promise, now doing real identity work across a genuinely wide space rather than reporting a short settings list.

---

## 8. Cross-System Integration

- **Economy & Finance:** Tax Policy, every Funded Action, and Grain Requisition all read/write Treasury, DebtRecord, and Net Worth directly.
- **Labor & Slavery:** Household Regimen Posture, Education Investment, Manumission Edict, and Debt Bondage Ban all extend or close mechanics that document named but left for this one.
- **Religion:** Rites Budget, Domus Pia, and Debt Bondage Ban's Favor tick all extend that system directly.
- **Politics & Patronage:** Sumptuary Edict, Faction (the Household Doctrine model's own template), Patronage Generosity, Reformer's Momentum's Edict-cost discount, and Dowry Subsidy's alliance-value boost.
- **Familia:** Marital Diplomacy Posture and Dowry Subsidy both extend that document's own marriage math with a standing household lean and a deliberate spend respectively.
- **Settlement Demographics:** Recruitment Doctrine, Annona Provision, Provincial Administration Posture, Citizenship Grant, Grain Dole, Grain Requisition, and Colonization Grant all read or write Contentment, Employment Ratio, or Assimilation.
- **Military & Combat:** Recruitment Doctrine's Slave-Militia gating, Frontier Security Posture's Squad-allocation tradeoff, Domus Bellatrix, and Triumphal Dedication.
- **Characters:** every Edict's Reception runs on that document's Scheme engine.
- **Legal & Court:** General Amnesty, Citizenship Grant's validity challenge, and severe Edict backlash all generate real cases there.
- **Dynasty Chronicle:** every Edict Declaration and every Doctrine threshold (Emerging, Defining, and especially Apex) is guaranteed Chronicle material — a household's Doctrine arc is meant to read as real, generational history.
- **Succession & Dynasty:** Apex tier (§3.3) is this document's direct, deliberate extension of that system's own succession moment into a genuine long-game mechanical reward, closing this document's own inheritance question with real stakes rather than a flat carry-over number.
- **Rival Houses:** Land Redistribution, Proscription's demonstration effect, and Domus Mercatoria's Trade Concession all generate lasting, real inter-house dynamics.
- **Diplomacy with Non-Roman Peoples (§6.25, future) / Piracy & Banditry (§6.24, future):** Provincial Administration Posture, Domus Provincialis's Foederati Pact, and Frontier Security Posture are this document's forward hooks into both.
- **Disease & Public Health (§6.13, future):** Plague Relief is this document's named forward hook.
- **Games & Spectacle (§6.22, future):** Ludi and Domus Bellatrix's victory-funded condition.
- **Steward/Council Auto-Management (§6.28, future):** Playbooks (§6) are the natural object that system's own automation extends.

---

## 9. Data Model

```
StandingPolicies {
  householdId,
  taxPolicyRef, regimenPosture: { diet, accommodation, freedoms, discipline },
  ritesBudgetTier, sumptuaryEdictActive: bool,
  recruitmentDoctrine: { sourceDoctrine, slaveMilitiaCrisisGated: bool, intensity },
  annonaProvisionActive: bool,
  tradeOpenness, patronageGenerosity, educationInvestment,
  provincialAdministrationPosture,       // null off-frontier
  maritalDiplomacyPosture,               // "allianceMaximizing" | "balanced" | "compassionate"     — §2.11
  frontierSecurityPosture,               // "fortify" | "patrol" | "minimalGarrison"                 — §2.12
}

HouseholdDoctrine {
  householdId,
  doctrineType,
  affinityScore,               // 0-100
  tier,                        // "none" | "emerging" | "defining" | "apex"
  capstoneUnlocked: bool,
  capstoneUsedThisGeneration: bool,
  apexEligibleSinceMonth,       // set once Defining survives a succession event with continued matching policy — §3.3
  hybridTitlesActive: [ ... ],   // §3.4 — derived, recomputed whenever two+ Doctrines sit at Emerging or above
}

FundedAction {
  settlementId, month,
  type,                        // "ludi" | "festival" | "publicWorks" | "grainDole" | "colonizationGrant" |
                                // "triumphalDedication" | "plagueRelief" | "dowrySubsidy"
  amount, payoffRef,
}

Edict {
  edictId,
  type,                         // "tabulaeNovae" | "generalAmnesty" | "landRedistribution" | "manumissionEdict" |
                                // "citizenshipGrant" | "proscription" | "debtBondageBan" | "grainRequisition"
  issuedMonth, scope,
  influenceCost, dignitasCostToIssue,
  effectApplied,
  reception: { contentmentShift, dignitasShift, factionBacklashTriggered: bool,
               schemeRef, legalCaseRef, feudRef, demonstrationEffect: bool },
}

PolicyPlaybook {                // §6
  playbookId, householdId, name,
  savedStandingPolicies,         // a full StandingPolicies snapshot
  savedMonth,
}
```

---

## 10. Open Questions

- **All numeric sizing.** Every Standing Policy's effect deltas, Edict costs, Reception curves, Doctrine Affinity gain/decay rates, and Apex's own continuation-duration requirement are all unsized, per this project's convention.
- **Doctrine mutual-suppression matrix.** Several pairs are named directly (Mos Maiorum vs. Domus Mercatoria, Res Publica Popularis vs. Domus Dura, and others noted inline), but the full seven-by-seven matrix isn't enumerated.
- **Multiple Apex Doctrines at once.** Whether a household can hold Apex in more than one Doctrine simultaneously, and how their rewards interact if so, isn't addressed.
- **Hybrid title stacking.** §3.4 lists six named pairs; whether a household with three or more Doctrines at Emerging-plus should read multiple hybrid titles at once, or only the single strongest pairing, isn't specified.
- **Debt Bondage Ban repeal.** §5.8 notes the avenue reopens only "until a future Edict reopens it" without naming what that reopening Edict actually is — a plausible small addition to this roster rather than a genuine gap.
- **Proscription's civil-crisis qualification.** The exact criteria for the alternate crisis-based gate (beyond holding Duumvir-or-above) aren't enumerated.
- **Playbook portability across settlements.** For a player running more than one settlement, whether a single Playbook applies everywhere or must be settlement-specific isn't decided — the same open multi-settlement question this document's earlier passes already carry.
- **Grain Requisition's relationship to ordinary Tax Policy.** Whether a household under an already-high Decuma rate faces any additional friction issuing a Requisition on top of it, or whether the two are read as fully independent levers, isn't specified.
