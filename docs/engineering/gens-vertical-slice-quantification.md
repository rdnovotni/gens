# GENS — Vertical-Slice Quantification

*This document closes Phase 1, Item 6 of the Comprehensive Build Roadmap: "quantify the vertical slice — initial household size, named/background population bands, production yields, needs, wages, prices, construction times, tick order, event cadence, relationship ranges, and report thresholds." The roadmap's own "Vertical-slice acceptance test" and Phase 5/6 descriptions specify shape but not numbers; this document proposes concrete numbers, grounding each in the design corpus wherever it already supplies one and marking every invented figure explicitly as a proposed default pending playtesting (per the roadmap's own rule: "do not solve balancing exclusively in prose... put tunable values in versioned content").*

Every number below should land in versioned content data, not hardcoded logic, so it can move without a code change once playtesting starts.

---

## 1. Initial household size and named roster

**Proposed: 8 named household members**, inside the roadmap's stated 6–10 range (`gens-comprehensive-build-roadmap.md`, Phase 5 exit gate and Vertical-slice acceptance test).

Grounded in `gens-familia-design.md` §3's five lifecycle stages (Infant 0–3, Child 4–12, Adolescent 13–17, Adult 18–59, Elderly 60+) — the slice roster is built to exercise all five stages and both the marriage and succession-pressure mechanics named in the same document's exit-relevant sections:

| # | Role | Lifecycle stage (§3) | Proposed starting age | Note |
|---|---|---|---|---|
| 1 | Paterfamilias (player character) | Adult | 35 | Household head, patria potestas |
| 2 | Materfamilias | Adult | 32 | Spouse |
| 3 | Heir apparent (elder son) | Adult | 19 | Labor/Court Position eligible per §3 |
| 4 | Daughter | Adolescent | 15 | Betrothal-eligible per §3 |
| 5 | Younger son | Child | 9 | Education investment active, no labor |
| 6 | Infant daughter | Infant | 2 | Health/mortality risk surface only |
| 7 | Paterfamilias's own surviving parent | Elderly | 64 | Succession-pressure flavor, retired from active duties |
| 8 | Household Steward (first Companion/Court Position) | Adult | 40 | Doubles as the "one overseer" the acceptance test requires |

*Dependency:* this roster is the seed for S1–S9 in `gens-open-question-queues.md` (trait generation, marriage consent, inheritance weighting) — those slice-tuning numbers must be resolved before this roster can generate deterministically.

---

## 2. Named vs. background population bands

**Corpus structure:** `gens-settlement-demographics-design.md` §3 defines exactly eight background pop groups (Coloni, Operarii, Opifices, Negotiatores, Aeditui, Curiales, Veterani, Non-Household Enslaved) and §13's "Illustrative Composition" gives directional proportions for a *typical mid-size settlement*: Coloni+Operarii 55–70%, Opifices+Negotiatores 20–30% combined, Curiales 5–10%, Veterans scaling with regional militarism, Non-Household Enslaved varying by setting/era. No absolute headcounts are given anywhere in the corpus — confirmed by Estate & Settlement §9's own open question ("stage-transition population numbers... depend on Settlement Demographics").

**Proposed default (needs playtesting):** a Villa-stage starting settlement of **220 background population**, split per §13's own directional bands:

| Pop group | Share | Proposed headcount |
|---|---|---|
| Coloni | 45% | 99 |
| Operarii | 15% | 33 |
| Opifices | 12% | 26 |
| Negotiatores | 10% | 22 |
| Aeditui | 4% | 9 |
| Curiales | 5% | 11 |
| Veterani | 3% | 7 |
| Non-Household Enslaved | 6% | 13 |

This sits below §5's "modest threshold" for Vicus so the slice starts at Villa stage per the acceptance test's single-estate scope, and is sized to make Phase 7's conservation tests (no group silently duplicating or disappearing) tractable to eyeball in a 24-month or 120-month run. The 220 total and the exact percentage split are both proposed defaults, not corpus figures.

---

## 3. Production yields (three compact chains)

Buildings' own Open Questions concede chain math isn't attempted: "Processing-time/ratio balancing isn't attempted here — this document establishes *what* the chains are, not their throughput math" (`gens-buildings-design.md` §9). Resources & Goods confirms spoilage/quality/supply-demand numbers are all still open (§17).

**Proposed default chains (needs playtesting)**, chosen from Buildings' §5 "Showcase Chains" and §4.2 Agriculture-Staples category so the slice matches Phase 6's "three compact chains" requirement:

| Chain | Input(s) → Output | Proposed monthly yield per building |
|---|---|---|
| Grain | Farmland labor → Grain | 40 units/month per staffed Farm slot |
| Bread | Grain → Bread (via Pistrinum) | 1 Bread per 1 Grain, capped at 35 units/month per Pistrinum |
| Wine | Vineyard labor → Wine (via Wine Press/Cella Vinaria) | 20 units/month per staffed Vineyard, subject to §9.3's Wine spoilage exception (slowest decay of the three spoilage tiers) |

All three numbers are proposed defaults; the corpus supplies the chain *shape* (Grain→Bread, Vineyard→Wine per Buildings §4.2/§4.3 and §5) but not throughput.

---

## 4. Needs (household diet tiers)

**Grounded in the corpus:** Resources & Goods §13.2 defines three Regimen Diet Tiers verbatim:

| Diet Tier | Consumes (per §13.2) |
|---|---|
| Meager | Grain/Legumes only |
| Adequate | Bread, plus a modest Wine or Beer allotment |
| Generous | Bread, Wine/Beer, Cheese or Sausages, and occasional Fish or Garum |

**Proposed default quantities (needs playtesting, tiers themselves are corpus-grounded):** Adequate tier (the slice's starting default per household status) consumes 2 units Bread + 0.5 unit Wine per named adult per month, 1 unit Bread + 0.25 unit Wine per Adolescent/Child, and 0.5 unit Bread for an Infant — proposed so the 8-person roster's monthly Bread demand (~13 units) sits comfortably inside the proposed Pistrinum yield above (35 units/month), leaving surplus for the background population's Tavern draw-down (§13.1).

---

## 5. Wages

Economy & Finance's own Open Questions concede: "wage scales... are all unsized" (§13). No wage figures exist anywhere in the corpus.

**Proposed default (needs playtesting):** 4 denarii/month for an unskilled Operarius-equivalent background laborer; 8 denarii/month for a skilled Opifex-equivalent; 15 denarii/month for the Steward (Companion-tier position). These three numbers are internally consistent with each other (a 2x skill premium, a further ~2x for a named Companion role) but are not derived from any corpus figure — Economy & Finance §4.1 names Wages as an expense category without sizing it.

*Dependency:* wages must stay consistent with prices (§6 below) — the proposed 4 denarii/month unskilled wage should comfortably clear the proposed Bread price, or the ledger's needs-satisfaction loop breaks for the background population within the first few simulated months.

---

## 6. Prices

Resources & Goods §11: "Base price by tier, modified by Quality, regional scarcity, and Market Dynamics" — confirms a tiered *structure* (goods sit in Raw Materials / Intermediate / Finished / Luxury / Imported categories per §7, with three Quality grades per §10.2: Common/Fine/Exceptional) but supplies no base-price numbers.

**Proposed default (needs playtesting):**

| Good | Category (§7) | Proposed base price (denarii, Common quality) |
|---|---|---|
| Grain | Raw Materials | 0.3/unit |
| Bread | Intermediate | 0.6/unit |
| Wine | Finished | 1.5/unit |

Fine quality proposed at 1.5x base, Exceptional at 3x base, consistent with §10's three-grade structure. These multipliers and base prices are proposed defaults, not corpus figures. *Dependency:* the Bread price above must stay consistent with the wage figures in §5 — at 4 denarii/month and 0.6/unit Bread, an unskilled laborer's wage covers roughly 6.6 units of Bread/month, comfortably above the proposed 1–2 units/person/month diet need in §4.

---

## 7. Construction times

No construction-time numbers exist in the corpus; Estate & Settlement's own Open Questions flag the adjacent Repair-cost/time question as unresolved (§9), implying construction times are equally unsized.

**Proposed default (needs playtesting):**

| Building tier | Proposed construction time |
|---|---|
| Basic production building (Farm plot, Pistrinum) | 2 months |
| Mid-tier building (Vineyard + Press, Market Stall) | 4 months |
| Civic/Monument-adjacent building | 8 months |

Repair time proposed at 40% of original construction time (S10 in the open-question queues), also a proposed default.

---

## 8. Tick order

**Grounded directly in the roadmap.** Phase 2 Item 4 specifies the tick-phase order the vertical slice should implement: *scheduled commands → lifecycle → production → employment/needs → markets/ledger → relationships/actors → hazards → events → reports → invariant checks* (`gens-comprehensive-build-roadmap.md`, Phase 2). This is a roadmap decision, not a design-corpus number, and needs no further quantification — only enforcement as declared phase dependencies per Phase 2 Item 5 ("systems declare ID, phase, dependencies, read set, and write set").

For the vertical slice specifically, this resolves to: command intake → Familia lifecycle (aging, births, deaths) → the three production chains → labor/needs consumption → ledger posting and the one market/contract → relationship/opinion decay → (no hazards in the slice's minimum scope — Phase 14) → the weighted event pool and three compact event chains → monthly report generation → invariant/hash checks.

---

## 9. Event cadence

Events' own Open Questions narrow to two genuine gaps (Historical Timeline authoring and Divergence threshold — both post-slice per Queue 3); no monthly firing-rate number for the Weighted Event Pool itself is given anywhere in `gens-events-design.md`.

**Proposed default (needs playtesting):** for the slice's three compact event chains, propose a base weighted-pool roll of 1 candidate draw/month per household, with each of the three chains' entry conditions weighted so that, in expectation, at least one chain's opening stage fires within the first 6 months of a 24-month playthrough (satisfying the acceptance test's requirement that the player "respond to three compact event chains" inside that window). Scripted Events (Doctrine milestones, lifecycle transitions, settlement-stage changes) remain deterministic per Events §4 and need no cadence number — they fire the instant their condition is met.

---

## 10. Relationship ranges

**Fully grounded in the corpus, no invention needed:**

- **Opinion:** −100 to 100, 0 neutral, between any two named individuals (`gens-characters-design.md` §7, "Familia §2.7's model — opinion (−100 to 100) plus bond tags... is unchanged and... explicitly universal").
- **Personality axes:** seven axes (Honor, Compassion, Greed, Zealotry, Vengefulness, Boldness, Rationality), each −100 to 100, 0 neutral, hidden from ordinary UI (`gens-characters-design.md` §5/§15).

No proposed defaults needed here — these ranges are load-bearing corpus values and should be treated as fixed contract, not tunable content, consistent with Phase 1 Item 3's field-ledger goal (one owner, one unit, one range).

---

## 11. Report thresholds

No explicit "importance"/"priority" numeric scale is given in the corpus; `gens-events-design.md` §7 and the roadmap's Phase 4 Item 5 both describe the Monthly Report's *shape* — "importance, grouping, acknowledgement state, and links to involved entities" — without sizing an importance scale.

**Proposed default (needs playtesting):** a 3-tier importance scale for report entries — **Routine** (auto-summarized, collapsed by default: ordinary production/consumption postings), **Notable** (shown expanded: a birth, a completed construction, a wage change), **Critical** (requires acknowledgement before advancing: a death, an insolvency warning, an event-chain decision point) — mirroring the three-tier structure Events §6.1 already uses for imperial-event prominence (Tier 1 Ambient Flavor / Tier 2 Real Mechanical Ripple / Tier 3 Full Drama), reused here for consistency rather than inventing an unrelated scale.

---

## Summary of quantified areas

| Area | Headline number(s) | Grounding |
|---|---|---|
| Household size | 8 named members across all 5 lifecycle stages | Roadmap range + Familia §3 stages |
| Population bands | 220 background pop, 8 groups per §13 proportions | Settlement Demographics §3/§13 (proportions); headcount proposed |
| Production yields | Grain 40, Bread 35, Wine 20 units/month/building | Buildings §5 chain shape; yields proposed |
| Needs | 3 diet tiers (Meager/Adequate/Generous); ~2 Bread + 0.5 Wine/adult/month | Resources & Goods §13.2 tiers; quantities proposed |
| Wages | 4 / 8 / 15 denarii-month (unskilled/skilled/Companion) | Proposed default, needs playtesting |
| Prices | Grain 0.3, Bread 0.6, Wine 1.5 denarii/unit (Common) | Resources & Goods §7/§10/§11 structure; prices proposed |
| Construction times | 2 / 4 / 8 months by tier | Proposed default, needs playtesting |
| Tick order | commands → lifecycle → production → needs → markets/ledger → relationships → hazards → events → reports → invariants | Roadmap Phase 2 Item 4 |
| Event cadence | ~1 pool draw/month/household; ≥1 chain-opening within 6 months | Proposed default, needs playtesting |
| Relationship ranges | Opinion & 7 personality axes, both −100..100 | Characters §5/§7/§15 (fixed contract) |
| Report thresholds | 3-tier Routine/Notable/Critical | Proposed, reusing Events §6.1's tier pattern |
