# GENS — System Design: Monuments & Legacy Building (§6.23)
*The core doc's own four named Monuments finally get a real system: a Legacy Value mechanic where Dignitas grows the longer a Monument survives real generational successions, several new types tying into other systems' own capstones, settlement-scale capacity limits, a Named Public Works bridge letting an ordinary Policies & Edicts spend become permanent legacy, and a real, mechanical Rival Houses reaction. This pass adds the dark mirror the whole system was missing: real, formal Damnatio Memoriae — the actual Roman practice of condemning a name and ordering every trace of it erased — because "Memory has weight" cuts both ways, and a system all about memory growing more valuable with age needed its own honest account of memory being violently destroyed instead.*

---

## Contents

1. Scope & Role
2. The Monument Roster — Existing & New
3. Named Public Works — Turning a Funded Action into Permanent Legacy
4. Monument Capacity — Settlement Scale as a Real Constraint
5. Legacy Value — Dignitas That Grows With Age
6. Monument Permanence — Decay, Damage & the Cost of Neglect
7. Damnatio Memoriae — When Memory Is Erased, Not Just Forgotten
8. Public and Private Legacy
9. Rival Reaction — A Real, Mechanical Response
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "a prestige-only construction category... These don't produce yield; they produce Dignitas and Chronicle entries, and can become landmarks other rival houses visibly react to." This document is that category's own real system: a genuine Legacy mechanic (§5), a real permanence-and-neglect model (§6), the actual rival-reaction mechanism the core doc promised (§9), and, new this pass, the honest dark counterpart every "memory has weight" system eventually needs — memory can be destroyed as deliberately as it can be built (§7).

---

## 2. The Monument Roster — Existing & New

### 2.1 Existing *(recap — Buildings §4.12, unchanged)*

Statue → Grand Statue, Family Tomb, Dedicatory Temple, the military-victory-gated Triumphal Arch, the Nymphaeum, and the public Necropolis.

### 2.2 From the Prior Pass — Tied to Other Systems' Own Capstones *(unchanged)*

The Doctrine Apex Monument, Mausoleum, Freedman's Monument, Liberty Column, Founder's Stone, and Inscribed Dedication — see the prior pass for each one's own triggering achievement and real historical grounding.

### 2.3 New This Pass

| Monument | Triggering achievement | Flavor |
|---|---|---|
| **Terminus Stone** | Any real expansion of the estate's own landholding (Estate & Settlement) | A real, genuinely charming detail: *Terminus* was an actual, distinctly Roman minor deity of boundaries — marking new land with his stone was real religious practice, not merely surveying. The cheapest Monument in this document's own roster, and the most common |
| **Tropaeum (Battlefield Trophy)** | A Military & Combat victory that doesn't rise to the Triumphal Arch's own threshold | A real, distinct, and genuinely more modest alternative — erected at or near the actual site of a real but lesser victory, rather than the grand, urban, capital-scale statement an Arch makes |

### 2.4 A Note on Naming *(unchanged from the prior pass)*

Trajan's Column remains flavor for the Dacian Wars specifically (Cultures §5) rather than a second, competing generic column type; a Doctrine Apex Monument or a Legendary-tier Chronicle entry can simply take a column's own real historical form as one available presentation.

---

## 3. Named Public Works — Turning a Funded Action into Permanent Legacy

New this pass, and a real, direct bridge between two categories that had no connection before: Policies & Edicts' own Public Works Funded Action (§4 of that document) is, as designed, a one-off spend with a contained, non-recurring payoff — real, but not permanent. This document adds the real, historically ubiquitous Roman option sitting right next to it: a genuinely ancient and common practice, visible on countless real surviving inscriptions ("*[name] restored/built this at his own expense*"), of a wealthy patron attaching their own family's name to a public work permanently rather than merely funding an anonymous improvement.

At a sufficiently large investment tier, a Public Works Funded Action offers a real choice: the ordinary, cheaper **anonymous improvement** (Policies & Edicts' own existing shape, unchanged), or a costlier **Named Public Work** — an aqueduct extension, a road, a bridge, a bathhouse wing — that becomes a genuine, permanent Monument entry in its own right, complete with its own Legacy Tier progression (§5) exactly like any other Monument on this document's roster. This is the concrete mechanism turning an ordinary civic improvement into something a family is remembered for generations later, rather than a single Ledger line that fades the month after it's spent.

---

## 4. Monument Capacity — Settlement Scale as a Real Constraint

New this pass: a genuine, practical limit tied directly to Estate & Settlement's own growth stages (§5 of that document), since a scattering of grand statues around a single rural Villa would read as absurd in a way the same collection genuinely wouldn't inside a real City's own forum. Monument Capacity scales with settlement stage — a Villa supports only a modest handful of Monuments meaningfully; a full City can support a real, substantial collection befitting an actual major settlement's own historical center. This isn't a hard, arbitrary wall so much as a real, legible signal: a player wanting to build extensively in this category has a real, concrete reason to also invest in Estate & Settlement's own growth track, rather than the two systems sitting entirely disconnected from each other.

---

## 5. Legacy Value — Dignitas That Grows With Age *(unchanged from the prior pass)*

New, Established, and Ancient tiers, the latter two gated on surviving a real Succession & Dynasty transition, mirroring Household Doctrine's own Emerging/Defining/Apex shape.

---

## 6. Monument Permanence — Decay, Damage & the Cost of Neglect *(unchanged from the prior pass)*

No ordinary staffing or goods upkeep; real vulnerability to Natural Disasters' own struck-Monument rule; a new-this-project regression mechanic where sufficiently long neglect after damage can cost a full Legacy Tier.

---

## 7. Damnatio Memoriae — When Memory Is Erased, Not Just Forgotten

The genuinely necessary dark counterpart to everything else in this document, and a real, well-documented, specifically Roman institution rather than an invented mechanic: the actual formal practice — sometimes ordered by the Senate, sometimes by an Emperor — of condemning a disgraced individual's memory, ordering their statues smashed or re-carved, their inscriptions chiseled out, and their name struck from public record. Several real Roman emperors and officials genuinely suffered exactly this fate. If Legacy Value (§5) is this document's thesis that memory grows more valuable the longer it survives, Damnatio Memoriae is its honest antithesis: memory can also be *ended*, deliberately, publicly, and completely.

### 7.1 As a Fate That Can Befall the Player's Own House

A household that suffers a sufficiently catastrophic and public disgrace — a failed Alliance Against Rome (Diplomacy with Non-Roman Peoples §10), an especially severe Proscription landing against it rather than one it issued, or the worst possible outcome of a Legal & Court capital-tier case — can face real, formal Damnatio Memoriae as a consequence, not merely a Dignitas penalty. Every Monument the household holds is affected: **defaced** (a real, visible, permanent condition state distinct from ordinary disaster damage, carrying its own Legacy Tier reset to nothing) or, for the household's own name specifically, **struck from the Dynasty Chronicle's own public-facing record** — the entries themselves aren't deleted (this project's own "no forced ending, memory has weight" pillars don't permit that), but they're marked, in-fiction, as officially erased, a genuinely chilling distinction between what actually happened and what Rome now permits to be said happened.

### 7.2 As a Weapon Against a Defeated Rival

The reverse is equally real and equally available: a sufficiently Prominent, politically powerful household (reading Politics & Patronage's own standing and cursus honorum weight) can formally **petition for Damnatio Memoriae** against a rival house that's already suffered its own real, severe downfall — a rival crushed after its own failed rebellion, or Proscribed and defeated. Successfully securing this is a real, maximum-weight political and social victory distinct from simply defeating a rival practically: it's the deliberate, permanent erasure of everything that rival house built toward Legacy Value across the whole rest of this document, a genuinely severe and rarely-available capstone to a rivalry rather than an ordinary outcome of ordinary competition.

### 7.3 Reversal — A Real, Rare Historical Footnote

Worth noting honestly rather than pretending the erasure is always final: real Roman history also records real cases of a Damnatio Memoriae later being formally reversed by a subsequent regime, monuments re-erected or restored, a name rehabilitated. This document allows the same rare possibility — a defaced Monument can, at real cost and requiring real political circumstances to have genuinely changed (a new Emperor, a shifted Faction climate), be restored, its Legacy Tier beginning to rebuild from nothing rather than instantly returning to whatever it once was. Rare, difficult, and exactly as real as the erasure it's reversing.

---

## 8. Public and Private Legacy *(unchanged from the prior pass)*

Public settlement-plot Monuments remain distinct from a future private Ancestor Gallery Villa room.

---

## 9. Rival Reaction — A Real, Mechanical Response

Unchanged core mechanism (envy, admiration, competitive construction, envy-driven Scheming), extended this pass with §7.2's own Damnatio Memoriae petition as the single most severe possible rival action available once a house has already fallen — the natural, dramatic ceiling above ordinary competitive monument-building.

---

## 10. Cross-System Integration

- **Buildings/Estate & Settlement:** the existing Monument roster is recapped; Monument Capacity (§4) is this document's own new, direct tie to that system's growth-stage track.
- **Policies & Edicts:** the Doctrine Apex Monument, Liberty Column, Founder's Stone remain unchanged; Named Public Works (§3) is this document's own new, concrete bridge to that system's Public Works Funded Action specifically.
- **Labor & Slavery:** the Freedman's Monument remains unchanged.
- **Education & Culture:** the Inscribed Dedication remains unchanged.
- **Succession & Dynasty:** Legacy Tier's own succession-gated thresholds remain unchanged.
- **Natural Disasters:** the struck-Monument rule and this document's own regression mechanic remain unchanged.
- **Diplomacy with Non-Roman Peoples:** a failed Alliance Against Rome is now a named, real trigger condition for Damnatio Memoriae against the household itself.
- **Legal & Court:** the worst possible capital-tier case outcome is a named Damnatio Memoriae trigger; §7.2's petition process is itself a real, formal Legal & Court action.
- **Politics & Patronage:** Prominence and political standing gate a household's own ability to petition for a rival's Damnatio Memoriae.
- **Rival Houses:** §9's full reaction range, now including Damnatio Memoriae petitions as its ceiling, remains built entirely on that document's own tiered-actor and independent-timeline model.
- **Characters:** envy-driven Scheming remains unchanged.
- **Dynasty Chronicle:** Damnatio Memoriae's own "struck from the record, not deleted" treatment (§7.1) is a direct, careful realization of that document's own commitment to never actually erasing history even when the in-fiction record claims to.
- **Religion:** Terminus, though a minor deity, is a real, attested figure in the Roman pantheon worth a light cross-reference to that document's own broader roster.

---

## 11. Data Model

```
Monument {
  monumentId, settlementId, plotId,
  type,                       // includes "terminusStone" | "tropaeum" | "namedPublicWork", alongside the full
                                // roster from both prior passes
  builtMonth,
  legacyTier,                  // "new" | "established" | "ancient" | "erased" (new — §7.1)
  successionsSurvivedIntact,
  intact: bool,
  defacedByDamnatio: bool,        // new — §7.1
  restoredFromDamnatio: bool,       // new — §7.3
  triggeringAchievementRef,
  linkedFundedActionRef,           // new — set only for a Named Public Work, per §3
  currentDignitasContribution,
}

DamnatioMemoriaeRecord {          // new — §7
  recordId, targetHouseholdId,
  direction,                        // "sufferedByPlayerHousehold" | "petitionedAgainstRival"
  triggeringCauseRef,                 // a failed AllianceAgainstRome, a Proscription, or a capital Legal case
  month,
  affectedMonumentIds: [ ... ],
  reversedMonth,                      // null unless §7.3's rare reversal occurs
}
```

---

## 12. Open Questions

- **All numeric sizing carried forward, plus new unsized figures:** Monument Capacity per settlement stage, Named Public Works' own cost premium over an anonymous improvement, and Damnatio Memoriae's own trigger severity threshold.
- **Damnatio Memoriae's exact trigger list completeness.** §7.1 names three real trigger conditions; whether other sufficiently catastrophic outcomes elsewhere in this project should also qualify isn't fully enumerated.
- **Reversal's exact political-circumstance requirements.** §7.3 gestures at "a new Emperor, a shifted Faction climate" without specifying the precise condition a future pass would need to check.
- **Doctrine Apex Monument slot limits.** Still unresolved from the prior pass.
- **Ancestor Gallery's own full design.** Still deferred to Villa's own future revisit.
- **Mausoleum's exact wealth/Dignitas gate.** Still unresolved from the prior pass.
