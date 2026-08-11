# GENS — System Design: The Dynasty Legacy Tree (§6.60)

*Built on CK3's own Dynasty Legacy model, per direction, and adapted to reuse rather than reinvent: the "spendable Renown" this system needs already exists in this project under a different, better-fitting name — **Memoria**, Ancestor Veneration & Funerary Customs' own third household axis, "what the household's own dead think, and how much of their memory the living have actually preserved." That document gave Memoria real weight but no real spend; this document is Memoria's first genuine sink, and the payoff is permanent, structural, and literally generation-spanning — Design Pillar #7 made mechanical a second time, in a genuinely different way than Policies & Edicts' own Household Doctrine already made it mechanical once. The two systems are deliberately not the same thing, and §1 says so directly before anything else.*

**Distinct from:** two other systems share the word "Dynasty" but are not duplicates of this one — [Dynasty Chronicle](gens-dynasty-chronicle-design.md) is the household's own record/log, and [Succession & Dynasty](gens-succession-dynasty-design.md) governs inheritance and generational transition. This document is a spendable-Renown progression tree.

---

## Contents

1. Scope & Role — Distinct From Household Doctrine
2. Memoria as Currency — Reused, Not Reinvented
3. Tree Structure — Seven Branches, Three Tiers Each
4. Sanguis — Blood & Lineage
5. Nomen — Name & Renown
6. Auctoritas — Household & Court
7. Patrimonium — Inheritance & Estate
8. Virtus Maiorum — Ancestral Valor
9. Manes — The Ancestor Cult
10. Ingenium — Learning & Culture
11. The Real Tradeoff — Spending vs. the *Pius* Threshold
12. A Worked Example — The Domus Aemilia Across Three Generations
13. Extinction — What Happens When the Line Ends
14. Cross-System Integration
15. Data Model
16. Open Questions

---

## 1. Scope & Role — Distinct From Household Doctrine

Worth resolving before anything else, since Policies & Edicts' own Household Doctrine system already occupies adjacent ground: a Doctrine (Domus Bellatrix, Domus Pia, Domus Mercatoria, and the rest) **emerges for free from sustained policy behavior** — no cost, no player-authored choice beyond the standing policies the player was already setting for other reasons, decaying if the pattern lapses. This document is the deliberate opposite kind of permanence: a **Legacy Tree node is explicitly purchased**, once, with a real and finite resource the household had to actually earn through Ancestor Veneration's own practices, and once bought it never decays and never needs to be "kept" the way a Doctrine does. Where a Doctrine answers "what has this household quietly become," a Legacy answers "what did this family deliberately decide, generations ago, to permanently become" — two different flavors of the same Pillar #7 commitment, not one system wearing two names. Several reward flavors below sit thematically near a Doctrine's own capstone (both care about martial bloodline, both care about piety); every one of them is written to grant a concretely different effect than its Doctrine cousin, and §14 calls out each near-neighbor explicitly rather than leaving the overlap for the player to puzzle out.

The word "Legacy" itself is also already spoken for twice elsewhere in this project, worth disambiguating directly rather than leaving to context: Monuments & Legacy Building's own "Legacy Tier" describes a physical structure's grandeur and decay state, and Character Ambitions' "Legacy Ambitions" describes a personal goal passed to a specific heir. Neither is this document's own **Legacy node** — a permanent, tree-gated, Memoria-purchased household-wide unlock with no physical building and no single Character attached to it. All three are genuine, independent uses of the same real English word for a genuinely Pillar #7-flavored concept; none of them is a typo for either of the others.

---

## 2. Memoria as Currency — Reused, Not Reinvented

This document adds no new resource. Memoria already accumulates from consistent *Parentalia*/*Lemuria* observance, a well-conducted funeral, a maintained Family Tomb, and a genuine Dynasty Chronicle entry (Ancestor Veneration §6.1) — until now, entirely a passive, quietly-read stat with no deliberate spend attached to it. This document gives it one: **a Legacy Tree node costs a real, one-time amount of the household's current banked Memoria**, permanently reducing that total the moment it's bought, while the unlocked Legacy itself is never lost afterward regardless of how Memoria subsequently rises or falls — exactly CK3's own Renown-for-Legacies exchange, running on a stat this project had already built for an entirely different reason.

---

## 3. Tree Structure — Seven Branches, Three Tiers Each

Seven branches, each gated Tier I → Tier II → Tier III in strict order (a household cannot skip ahead), with Tier III always resolving as a **two-way capstone fork** — a genuine, mutually exclusive choice between two distinct Roman household traditions, never a single obvious "best" pick, consistent with this project's own "no dominant settings" pillar. Costs scale by tier — Tier I modest, Tier II substantial, Tier III capstones representing a real lifetime's accumulation — left unsized per this project's standing numeric convention.

---

## 4. Sanguis — Blood & Lineage

- **Tier I — Strong Stock:** a small, standing improvement to the odds of a beneficial Congenital trait appearing at birth, household-wide.
- **Tier II — A Name Worth Marrying Into:** the household's own banked Memoria becomes a real, standing input to Familia's marriage-market appeal calculation, independent of the current head's own personal Dignitas — a genuinely old family reads as a catch even when its current fortunes are middling.
- **Tier III (fork):**
  - **Purity of Blood** — a real, standing advantage to matches made within the same elite social circle, rewarding a dynasty that marries its own kind generation after generation.
  - **Vigor of Mixed Blood** — a real, standing advantage to cross-cultural matches specifically, rewarding a dynasty built on alliance breadth over pedigree.

---

## 5. Nomen — Name & Renown

- **Tier I — A Name That Precedes You:** a Character born into the household starts with a small, standing Fame floor (Celebrities & Influential Figures), simply for being born into a family people already recognize.
- **Tier II — Written Into the Record:** Ancestor Veneration's own qualitative claim that a high-Memoria household's Dynasty Chronicle entries occasionally read with "an added flourish tying it explicitly back to a named ancestor" becomes reliable rather than occasional for this household specifically.
- **Tier III (fork):**
  - **The Grand Name** — Fame and Dignitas grow together more easily for this household than the Fame/Dignitas Divergence paradox (Celebrities & Influential Figures §2) ordinarily allows — the genuinely rare "both at once" combination that document calls out as exceptional, made a real, standing likelihood rather than a rare accident.
  - **The Notorious Name** — the opposite bet, deliberately: this household's Fame grows unusually well *from* scandal specifically, embracing the "any publicity is publicity" dynamic that document names as a real, dark feature rather than fighting it.

---

## 6. Auctoritas — Household & Court

- **Tier I — A House Remembered:** Companions and Clientela recruited into the household carry a small, standing Loyalty bonus simply from serving a family with real accumulated Memoria.
- **Tier II — The Steward's Legacy:** Steward Council & Auto-Management's own automated decision-making runs measurably more reliably for this household.
- **Tier III (fork):**
  - **The Great House** — a real, standing increase to Companions and Clientela capacity, a dynasty built on the sheer size of its own retinue.
  - **The Quiet Word** — a real, standing resistance specifically to Secrets & Hooks' own Discovery mechanics being used *against* this household — a family that has learned, across generations, the discipline of keeping its own affairs quiet.

---

## 7. Patrimonium — Inheritance & Estate

- **Tier I — Sound Management:** a small, standing discount on Economy & Finance's own interest rates, reflecting generations of proven creditworthiness.
- **Tier II — Undivided Inheritance:** Succession & Dynasty's own estate-division math runs slightly more efficiently for this household, a real, standing reduction in the fragmentation partible inheritance otherwise causes across generations.
- **Tier III (fork):**
  - **The Compounding Fortune** — a further inheritance-efficiency gain plus a real, standing Business Competition edge, a dynasty built on growth.
  - **The Unshakeable House** — real, standing resistance to Natural Disasters' own economic shocks and to Economy & Finance's default risk, a dynasty built on never being caught exposed.

---

## 8. Virtus Maiorum — Ancestral Valor

- **Tier I — The Weight of Their Arms:** a Masterwork weapon or piece of armor that has genuinely passed down through the family carries a richer functional bonus than an equivalent freshly-acquired piece — reused Masterworks provenance, not a new Congenital-odds effect, and deliberately distinct from Domus Bellatrix's own Apex reward (§14).
- **Tier II — Triumphal Memory:** a Triumph's own Dignitas and Fame yield reads richer for a house with deep banked Memoria — a real, compounding reward for a family that has earned Triumphs before, distinct from Domus Bellatrix's own Wages-discount and Muster-scale rewards.
- **Tier III (fork):**
  - **Sons of Mars** — a direct, standing Combat Resolution input bonus, deliberately structured to scale with whatever Memoria the household currently has banked rather than being fixed at unlock — the one capstone in the tree that keeps rewarding continued Memoria accumulation even after its own purchase price is paid, distinct from any policy currently in effect.
  - **The Disciplined Line** — a real, standing veteran-retention improvement and lower baseline Unrest in any Force-heavy household, a defensive rather than offensive bet.

---

## 9. Manes — The Ancestor Cult

- **Tier I — Rites Never Missed:** a small, standing resistance to Memoria's own neglect-decay (Ancestor Veneration §6.3) — a household that has genuinely internalized the practice no longer drifts from a single missed *Parentalia*.
- **Tier II — The Tomb Remembers:** Memoria's own generation rate from *Parentalia*/*Lemuria* observance and funeral tier both improve directly — a deliberate, explicit feedback loop rewarding continued investment in the exact resource this entire tree spends.
- **Tier III (fork):**
  - **Favored of the Dead** — a further Memoria generation bonus on top of Tier II's, the purest snowball option in the tree.
  - **Keepers of the Threshold** — real, standing Ill Omen resistance and improved *Lemuria* outcomes household-wide, a protective rather than accumulative bet, distinct from Domus Pia's own Divine Favor-focused Apex reward (§14) since this branch is Manes-cult-specific rather than state-religion-facing.

---

## 10. Ingenium — Learning & Culture

- **Tier I — A Cultured Line:** Education & Culture's own curriculum outcomes read slightly better across the household's children, generations of prior investment paying forward.
- **Tier II — Renowned Patrons:** Art & Art Commissions and Books & Manuscripts both carry a small, standing prestige bonus when commissioned by a house with deep Memoria.
- **Tier III (fork):**
  - **The Salon** — a real, standing Symposium hosting-quality and Fame-generation bonus, a dynasty known for who gathers at its table.
  - **The Archive** — a real, standing collection-value and Masterwork-provenance bonus, a dynasty known for what it keeps.

---

## 11. The Real Tradeoff — Spending vs. the *Pius* Threshold

Worth naming directly, since it's the single sharpest tension this document introduces into a stat that previously had none: Ancestor Veneration already names *sustained high Memoria* as a real path to a *Pius*-family cognomen or epithet (Epithets, Nicknames & Titles §6.2). Spending Memoria down on a Legacy node is now in direct, honest competition with banking it toward that threshold — a household can build a permanent structural advantage now, or hold out for the *Pius* name later, but doing both at once is a real, felt stretch rather than a free lunch. Neither choice is ever wrong; this is precisely the kind of genuine tradeoff this project's own pillars call for.

---

## 12. A Worked Example — The Domus Aemilia Across Three Generations

The elder Aemilia, a devout widow, spends the household's first meaningful Memoria bank on Manes Tier I and II across a quiet decade of consistent *Parentalia* observance — a deliberate bet that the family's own ancestor-cult should fund itself faster before spending outward. Her son, inheriting a household now generating Memoria noticeably faster, redirects the next generation's surplus into Auctoritas, ultimately choosing **The Quiet Word** at Tier III after a rival house's own spy nearly exposes an inconvenient family matter — a direct, felt payoff from Secrets & Hooks almost costing them dearly the generation before. His own daughter, inheriting both completed branches and a genuinely old, well-documented name, spends the third generation's Memoria on Nomen, choosing **The Grand Name** specifically because the family's history of quiet discretion (Auctoritas) and sustained piety (Manes) already gives her a real head start on keeping Fame and Dignitas moving together rather than apart. Three heads, three deliberate choices, one permanent structure — none of it achievable within a single lifetime, exactly as intended.

---

## 13. Extinction — What Happens When the Line Ends

A genuinely high-stakes throughline this document adds to Succession & Dynasty's own extinction trigger (Rival Houses §5.3): every Legacy Tree node a household has ever unlocked is tied to that specific *gens*'s own continuous identity, not to any single Character. If the line truly ends with no surviving heir, every unlocked Legacy is lost along with it — the single starkest, most literal expression this project has of Design Pillar #7 read in reverse: a family's accumulated memory only has weight for as long as someone is left to carry it forward.

A softer, distinct case worth naming separately: a formal adoption bringing an entire household under another's name (rather than an outright extinction with no heir at all) is not this section's own trigger — the adopting *gens*'s own Legacy Tree continues untouched, and whether the absorbed house's own unlocks ever carry forward in any diminished form is left to Succession & Dynasty's own adoption rules rather than assumed here.

---

## 14. Cross-System Integration

- **Ancestor Veneration & Funerary Customs:** this document's entire currency and its central feedback loop (Manes Tier II) both read that document's own Memoria mechanics directly, unmodified.
- **Policies & Edicts:** §1's own distinction is the load-bearing cross-reference here — Virtus Maiorum's rewards are deliberately object- and yield-scaling rather than Domus Bellatrix's own Congenital-odds and Wages-discount rewards; Manes's rewards are deliberately Manes-cult-facing rather than Domus Pia's own state-religion Favor-facing rewards.
- **Secrets & Hooks:** Auctoritas's own Quiet Word capstone is a direct, standing defensive tie to that document's own Discovery mechanics.
- **Celebrities & Influential Figures:** Nomen's entire branch, and both of its capstone forks, read that document's own Fame/Dignitas Divergence concept directly.
- **Succession & Dynasty / Rival Houses:** Patrimonium's inheritance-efficiency rewards and §13's extinction trigger both read those documents' own existing mechanics directly rather than inventing parallel ones.
- **Masterworks & Unique Crafted Objects:** Virtus Maiorum Tier I is a direct, real Masterwork-provenance bonus, and Ingenium's Archive capstone extends the same provenance logic to a collection as a whole.
- **Military & Combat / Games & Spectacle:** Virtus Maiorum Tier II reads that document's own Triumph-adjacent content directly; Sons of Mars is a direct, standing Combat Resolution Engine input.
- **Monuments & Legacy Building / Character Ambitions:** named directly in §1's own disambiguation rather than left to context — a Legacy Tier and a Legacy Ambition are both real, unrelated uses of this document's own central word.
- **Epithets, Nicknames & Titles:** §11's own spend-versus-*Pius* tension is this document's single most direct, deliberate point of friction with another system.
- **Companions & Court Positions / Steward Council & Auto-Management:** Auctoritas Tier I and II both read those documents' own Loyalty and automation mechanics directly.
- **Economy & Finance / Business Competition / Natural Disasters:** Patrimonium's remaining rewards read those documents' own interest-rate, competition, and shock-resistance mechanics directly.
- **Education & Culture / Art & Art Commissions / Books & Manuscripts:** Ingenium's full branch reads those documents' own curriculum, commission, and collection mechanics directly.
- **Familia:** Sanguis Tier II and its full capstone fork both read that document's own marriage-market calculation directly.

---

## 15. Data Model

```
Household {
  // existing fields unchanged
  memoria: number,                       // existing, Ancestor Veneration §6 — now a real spend as well as a passive read
  legacyTreeUnlocks: [ legacyNodeId ],     // permanent once added; cleared only by §13's extinction trigger
}

LegacyNode {
  legacyNodeId,
  branch,             // "sanguis" | "nomen" | "auctoritas" | "patrimonium" | "virtusMaiorum" | "manes" | "ingenium"
  tier,               // 1 | 2 | 3
  capstonePath,        // nullable — populated only for tier-3 nodes, e.g. "purityOfBlood" | "vigorOfMixedBlood"
  prerequisiteNodeId,   // nullable — tier 2 requires tier 1 of the same branch, tier 3 requires tier 2
  memoriaCost,         // unsized, per convention — qualitatively increasing by tier
}

LegacyUnlockRecord {
  householdId, legacyNodeId,
  unlockedMonth,
  memoriaSpentAtUnlock,
}
```

---

## 16. Open Questions

- **All numeric sizing**, per this project's standing convention — every Tier's Memoria cost, and every bonus's actual magnitude, is left unsized.
- **Whether a household can hold more than one Tier III capstone fork choice per branch retroactively** — this document assumes the fork is a one-time, permanent, unreversible choice per branch, never revisitable even after further Memoria accumulates.
- **Whether the Legacy Tree should be visible on the same screen as Dynasty Chronicle**, or deserves its own dedicated UI surface — a presentation question outside this document's own scope.
- **Cross-cultural framing.** This document's branch names and flavor are written Roman-first, consistent with Ancestor Veneration's own primary scope; a non-Roman default household (Nubia, Arabia Felix) plausibly deserves its own equivalent tree with different names over the same seven-branch structure, left open rather than assumed here.
- **Whether a partial Memoria refund should ever be offered** if a household later regrets a Tier III fork choice — this document currently treats every purchase as final with no take-backs, consistent with Pillar #7's own weight, but a future balancing pass may find this too harsh.
- **Multi-settlement households.** For a player running more than one settlement, whether a single Legacy Tree covers the whole *gens* or each settlement's own branch household tracks a separate tree isn't decided — the same open question Policies & Edicts' own Playbook portability already carries.
- **Absorbed-house Legacy carryover.** §13's own softer adoption case names the question honestly without resolving it — whether an absorbed house's unlocks ever partially transfer to the adopting *gens* is left to a future Succession & Dynasty pass.
