# GENS — Open Question Queues

*This document closes Phase 1, Item 5 of the Comprehensive Build Roadmap: "convert open questions into three queues — structural blockers required before coding a system, slice tuning required before the first playable version, and post-slice balancing intentionally left configurable." It draws on the design corpus's own self-flagged "Open Questions" / "Open Questions Carried Forward" / "Open Design Questions" sections — present in 109 of the 113 files under `docs/design/`, plus TBD-style markers inline — rather than inventing gaps. Only real, quoted-or-closely-paraphrased findings appear below; each cites its source document and, where the registry already named it, the registry cluster.*

*Priority is given to the roadmap's Phase 1 "Primary design inputs" (`gens-core-design.md`, `gens-characters-design.md`, `gens-traits-design.md`, `gens-familia-design.md`, `gens-estate-settlement-design.md`, `gens-resources-goods-design.md`, `gens-buildings-design.md`, `gens-settlement-demographics-design.md`, `gens-economy-finance-design.md`, `gens-policies-edicts-design.md`, `gens-events-design.md`, `gens-rival-houses-design.md`, `gens-starting-regions-design.md`) plus the registry's own named open items (§4 Romance/Seduction boundary, §10 Goods/Buildings contradiction, §30 Sanitation Investment migration). Findings from other design docs appear where they clearly bear on the vertical slice (`gens-labor-slavery-design.md`, `gens-villa-design.md`, `gens-romance-sexuality-lineage-design.md`).*

---

## How items were sorted

- **Structural blocker** — the *mechanism itself* is undefined (not just its magnitude), and a Phase 1–9 system cannot be coded correctly without picking one. Includes cases where the registry or a design doc names a genuine authority/contradiction gap rather than a missing number.
- **Slice tuning** — the mechanism is defined; only a concrete number, default, or minor rule is missing, and the vertical-slice acceptance test (6–10 named household, three production chains, one market/contract, background population groups, 24-month playthrough, 120–200-year soaks) directly exercises it.
- **Post-slice balancing** — real, but the system it belongs to (rival autonomy, full romance, region content breadth, legal process depth, Doctrine/Edict escalation, etc.) sits in Phase 10+ and is intentionally left as tunable content data, per the roadmap's own rule ("do not solve balancing exclusively in prose... put tunable values in versioned content").

---

## Queue 1 — Structural Blockers

| # | Question | Source | Why it's a blocker | Blocks |
|---|---|---|---|---|
| B1 | **Goods/Buildings taxonomy contradiction.** `gens-buildings-design.md` §2 and `gens-estate-settlement-design.md` §8 still contain their own now-superseded partial goods lists; Resources & Goods' own Open Questions calls this "the most concrete remaining task before implementation." | `gens-resources-goods-design.md` §17; Registry §10 | Two contradictory goods lists exist in source docs. The Phase 3 content compiler cannot emit one typed `goods` definition family while two documents disagree on what a "good" is. | Phase 3 Item 4 (typed content schema for goods) and any Phase 6 building/goods content authoring. |
| B2 | **Romance & Seduction's exact supersession boundary.** Registry itself flags: "the two romance documents' actual boundary (which specific mechanics in Romance & Seduction are *not* superseded, if any) isn't spelled out anywhere." | `gens-romance-sexuality-lineage-design.md` (Registry §4) | Without a resolved boundary, an engineer implementing romance/lineage fields cannot know which document is authoritative for a given mechanic. | Phase 17 romance implementation; any Phase 5 field that touches marriage/legitimacy data shared with romance (low slice risk, since Phase 5 only uses the "lifecycle/lineage minimum," but the ADR for shared fields should record the gap now). |
| B3 | **Sanitation Investment's document home.** Disease & Public Health's own §10 says it is "built here in full but explicitly flagged as belonging in Policies & Edicts' own Standing Policy roster... on its own next revisit." | `gens-disease-public-health-design.md` §12; Registry §30 | The Phase 3 content schema needs one canonical home for this field (policy vs. hazard) before typed `policies` and `hazards` definition families are cut, or the field gets defined twice. | Phase 3 Item 4 (typed definition families for policies). |
| B4 | **Goods-to-income conversion formula for Commerce buildings.** Estate & Settlement §3.1 establishes Commerce buildings convert stockpiled goods to income but not the per-good price or how a Market vs. Emporium vs. Port differ in conversion efficiency. | `gens-estate-settlement-design.md` §9 | The ledger cannot post a Commerce-building sale transaction without a conversion rule; this is a mechanism gap, not just a missing constant, since "how buildings differ" is unspecified. | Phase 8 Items 3–4 (market/price formation) and Phase 6 Item 8 (production/consumption events feeding the ledger). |
| B5 | **Capital Expenditure vs. ordinary goods purchase boundary.** Economy & Finance §4.4 names Slave Market purchases, land, Villa upgrades, and "significant" livestock purchases as Capital Expenditure, but the line against an ordinary Resources & Goods transaction isn't drawn precisely. | `gens-economy-finance-design.md` §13 | The ledger's transaction-type taxonomy (Phase 8 Item 2) needs a deterministic classification rule, or the same purchase could be posted two different ways in different runs — a determinism/save-hash risk. | Phase 8 Item 2 (ledger account/transaction-type definitions). |
| B6 | **Background economy vs. player economy price competition.** Whether independent Opifices'/Negotiatores' own production adds to shared local supply, competing with the player's goods, is unresolved. | `gens-settlement-demographics-design.md` §16 | Phase 8's market/price-formation contract (supply aggregation) cannot be written correctly without deciding whether background pop-group output is a supply source. | Phase 8 Item 3 (settlement markets, price formation). |
| B7 | **Consumption/demand layer ownership.** Buildings' own Open Questions: "whether a fuller demand system exists at the population level is still tied to Settlement Demographics." Directly duplicated by the Insulae/Domus population-numbers gap in the same section. | `gens-buildings-design.md` §9 | Phase 6 Item 8 ("emit complete... consumption... events") needs a settled owner for population-level demand before those events can be typed; currently split unresolved between two documents. | Phase 6 Item 8; Phase 7 Item 4 (needs demand). |
| B8 | **Stage-transition population numbers for Estate & Settlement's Villa → Vicus → Town → City ladder.** Explicitly deferred: "actual numbers depend on Settlement Demographics being designed first," and Settlement Demographics' own §12 only resolves *which* population figure counts (total background pop across 8 groups), not the threshold values. | `gens-estate-settlement-design.md` §9; `gens-settlement-demographics-design.md` §12 | The stage-transition state machine (a Phase 6/7 system) has an unresolved dependency edge: it reads a threshold neither document supplies. This blocks writing the system's read/write contract, not just its balance. | Phase 6 Item 1 (region/settlement/plot boundaries) and Phase 7 (settlement demographics) integration ADR. |
| B9 | **Wage-vs-Regimen crossover for freedmen.** Wages (Economy & Finance) and Regimen tiers (Labor & Slavery) are framed as parallel systems for free and enslaved labor respectively; whether a Freedman still bound by *obsequium* sits closer to one model or blends both is unresolved. | `gens-economy-finance-design.md` §13; `gens-labor-slavery-design.md` §12 | Phase 6 Item 6 (labor assignments, output) and Phase 7 Item 3 (employment matching, wages) both need one deterministic pay/output rule per labor-status category; a freedman is a legitimate household member the vertical slice could include. | Phase 6 Item 6; Phase 7 Item 3. |
| B10 | **Lifestyle trait acquisition trigger mechanism.** Traits §5.3 establishes these are earned through "sustained adult practice" without specifying the actual trigger condition (a duty slot held long enough, an Interaction count, a player choice). | `gens-traits-design.md` §11 | Phase 5 Item 4 ("implement traits... a small representative slice of the catalog") cannot code automatic Lifestyle-trait acquisition without picking one trigger mechanism — a design decision, not a tuning number. | Phase 5 Item 4. |
| B11 | **Promotion threshold from Settlement Demographics into Familia.** Familia's own §9 flags this as still needing "concrete trigger conditions once §6.26 gets its own pass"; Settlement Demographics §11 lists the qualitative triggers (deliberate hire, marriage proposal, Travel/Events encounter, Slave Market purchase) but not the deterministic rule for *when* a lazy-instantiation system fires one of them automatically. | `gens-familia-design.md` §9; `gens-settlement-demographics-design.md` §11 | Phase 5 Item 7 ("deterministic lazy instantiation and promotion from aggregate/background people to named characters") is an explicit exit-gate requirement and needs one deterministic rule to stay replay-safe. | Phase 5 Item 7 (exit gate: "promotion are deterministic and save-safe"). |

---

## Queue 2 — Slice Tuning

*Numbers/rules the vertical slice's own exit gates (Phase 5–9, plus the Vertical-Slice Acceptance Test) directly need. Proposed concrete defaults for several of these are worked out in `gens-vertical-slice-quantification.md`.*

| # | Question | Source | Why it's slice tuning |
|---|---|---|---|
| S1 | **Personality-axis nudge magnitudes.** "Exact Axis nudge magnitudes behind 'small'/'large'... are all unsized." | `gens-traits-design.md` §11; `gens-characters-design.md` §15 | Phase 5 Items 4–5 need working trait→axis→relationship resolution for the 6–10-person household; the mechanism (seven axes, −100..100) is defined, only magnitude is missing. |
| S2 | **Tiered-spectrum roll distribution (flat vs. bell curve)** for Intellect/Beauty/Physique. | `gens-traits-design.md` §11 | Needed at Phase 5 Item 2 (deterministic name/appearance/attribute generation) to seed the starting household and any lazily-instantiated character. |
| S3 | **Typical trait load's exact distribution curve** — the "2–4/1–3/0–5" guideline is a working default, not a tuned probability distribution. | `gens-characters-design.md` §15 | Same generation path as S2; needed to seed the starting 6–10 members deterministically. |
| S4 | **Backfill generation depth** for lazily-instantiated adults — full mini-biography vs. minimum-viable trait set. | `gens-characters-design.md` §15 | Directly exercised whenever the slice promotes a background pop-group member (Phase 5 Item 7, Phase 7 Item 5). |
| S5 | **Trait inheritance weighting** for the full catalog (a parent's Congenital trait raising a child's odds). | `gens-traits-design.md` §11; `gens-familia-design.md` §9; `gens-characters-design.md` §15 | Phase 5 exit gate requires "a 6–10 named-person household [to] run for multiple generations"; births need a working inheritance roll. |
| S6 | **Long-Lived/Short-Lived Stock's actual lifespan modifier.** | `gens-traits-design.md` §11 | Directly feeds mortality, needed for the multi-generation Phase 5 exit gate. |
| S7 | **Lifestyle Cap's exact number** (working default: three). | `gens-traits-design.md` §11 | A concrete number is needed the moment the representative trait slice includes any Lifestyle trait. |
| S8 | **Consent/happiness formula for marriage** — inputs (prior opinion, trait compatibility) are named, weighting isn't. | `gens-familia-design.md` §9 | Marriage is a lifecycle transition the Phase 5 exit gate exercises across generations. |
| S9 | **Fertility/childbirth toggle granularity** (simple on/off vs. multi-step slider). | `gens-familia-design.md` §9 | `CampaignConfig` (Phase 4 Item 1) needs this resolved before the headless bootstrap can expose the toggle the vertical slice's campaign creation flow requires. |
| S10 | **Repair action cost/time** relative to original construction cost. | `gens-estate-settlement-design.md` §9 | Phase 6 Item 7 ("maintenance") and the 120-month Phase 6 exit gate ("shortages... and interrupted construction resolve consistently") need a concrete repair cost/duration. |
| S11 | **Chain balancing (processing time/ratio)** for the three compact production chains. | `gens-buildings-design.md` §9 | Directly named by the Phase 6 exit gate: "one estate transforms inputs, labor, time, and maintenance into deterministic outputs for 120 months." |
| S12 | **New buildings' population/Dignitas/income numbers** — for the small subset of buildings the slice actually uses. | `gens-buildings-design.md` §9 | Same Phase 6 exit gate; only the slice's own building subset needs numbers now, the rest of the catalog is post-slice. |
| S13 | **Spoilage timers and Quality multipliers** for stockpiled goods. | `gens-resources-goods-design.md` §17 | Phase 6 Item 3 ("stockpiles with... spoilage hooks") needs at least placeholder numbers to be testable over 120 months. |
| S14 | **Mobility/migration rates, Employment Ratio thresholds, Contentment formula weighting, Assimilation rate.** | `gens-settlement-demographics-design.md` §16 | The vertical-slice acceptance test explicitly requires "background population groups"; Phase 7's exit gate (stable/explainable equilibria) needs these to run at all. |
| S15 | **Wage scales and inbound tax rate.** "All numeric sizing... wage scales... are all unsized." | `gens-economy-finance-design.md` §13 | The acceptance test requires "employment, wages, prices, transactions, tax/upkeep... reconcile in the ledger." |
| S16 | **Insolvency's actual trigger threshold** (depth and duration Net Worth must stay negative). | `gens-economy-finance-design.md` §13 | Ledger reconciliation in the acceptance test implicitly requires this to be well-defined so a shortage doesn't silently pass or silently break invariants. |
| S17 | **Net Worth depreciation formula** for neglected land/buildings. | `gens-economy-finance-design.md` §13 | Needed for estate valuation, which the ledger and monthly report both surface. |
| S18 | **Standing-policy and Edict cost/effect numeric sizing** (Reception curves, Doctrine Affinity rates) for the slice's own small policy set and one funded action. | `gens-policies-edicts-design.md` §10 | Phase 9's vertical-slice contents explicitly include "a small policy set... one funded action." |
| S19 | **Scenario Starts' region defaults** — which starting region a given Scenario Start implies isn't drawn yet. | `gens-starting-regions-design.md` §13 | Phase 9's vertical-slice contents require "one representative start profile," which needs a concrete region pairing. |
| S20 | **Villa room-slot counts per stage** — "approximate... real balancing depends on how many named individuals a typical household actually has." | `gens-villa-design.md` §11 | The slice's household (6–10 named members) directly determines whether the Villa's starting room count is adequate; needs a concrete slice-sized number now. |

---

## Queue 3 — Post-Slice Balancing

*Real design gaps, but the owning system sits at Phase 10 or later, or the item is pure numeric-balance content the roadmap's own governance rules say belongs in versioned content, not code or prose, and isn't exercised by the vertical-slice acceptance test.*

| # | Question | Source |
|---|---|---|
| P1 | Cross-category trait interactions beyond opposed pairs (e.g., Zealous + Rational). | `gens-characters-design.md` §15 |
| P2 | NPC-on-NPC Scheme visibility to the player. | `gens-characters-design.md` §15 |
| P3 | Duel's formal challenge/acceptance/lethality rules. | `gens-characters-design.md` §15 |
| P4 | Group Interaction scale ceiling (e.g., a 40-client Salutatio). | `gens-characters-design.md` §15 |
| P5 | Ambidextrous's rarity weighting relative to Left-/Right-Handed. | `gens-traits-design.md` §11 |
| P6 | Humor inheritance (no strong hereditary theory in the source culture to draw on). | `gens-traits-design.md` §11 |
| P7 | Combo Title list collision frequency (~70 entries across ten themes). | `gens-traits-design.md` §11 |
| P8 | Lifestyle trait lapse mechanism (player choice vs. automatic least-recently-exercised). | `gens-traits-design.md` §11 |
| P9 | Restriction-toggle granularity (single switch vs. per-restriction). | `gens-familia-design.md` §9 |
| P10 | Divorce consequence tuning (Dignitas hit, relationship scar magnitude). | `gens-familia-design.md` §9 |
| P11 | Legitimization cost formula relative to an ordinary divorce/scandal event. | `gens-familia-design.md` §9 |
| P12 | Specialization bonus curve for concentrated land-use categories. | `gens-estate-settlement-design.md` §9 |
| P13 | Contested-plot resolution mechanism (bidding war, Politics check, timed race). | `gens-estate-settlement-design.md` §9 |
| P14 | Second-settlement management model (independent, steward-run, or unified view). | `gens-estate-settlement-design.md` §9 |
| P15 | Demolition cost-recovery fraction. | `gens-estate-settlement-design.md` §9 |
| P16 | Herd Strategy's three-tier growth/yield tradeoff rates. | `gens-resources-goods-design.md` §17 |
| P17 | Livestock disease mechanics (parallel outbreak system vs. modifier). | `gens-resources-goods-design.md` §17 |
| P18 | Livestock rustling as a Piracy & Banditry raid category. | `gens-resources-goods-design.md` §17 |
| P19 | Oxen/Mules efficiency bonus sizing. | `gens-resources-goods-design.md` §17 |
| P20 | Cotton's mechanical expression beyond narrative flagging. | `gens-resources-goods-design.md` §17 |
| P21 | Regional exclusivity vs. soft weighting for production chains. | `gens-buildings-design.md` §9 |
| P22 | Imported goods pricing/availability model (depends on Port/Emporium and trade). | `gens-buildings-design.md` §9 |
| P23 | Medicine's and Incense's monthly consumption rates. | `gens-buildings-design.md` §9 |
| P24 | Wine vs. Beer regional preference's Dignitas/happiness modifier. | `gens-buildings-design.md` §9 |
| P25 | Mint's and Praetorium's named political-grant milestone triggers. | `gens-buildings-design.md` §9 |
| P26 | Curia's relationship to existing Politics & Patronage office-holding. | `gens-buildings-design.md` §9 |
| P27 | City-status prerequisite list confirmation (Basilica + Aqueduct/Cistern + Walls). | `gens-buildings-design.md` §9 |
| P28 | Valetudinarium treatment capacity relative to settlement population. | `gens-buildings-design.md` §9 |
| P29 | Brothel's *infamia* status pending a Familia social-stigma flag. | `gens-buildings-design.md` §9 |
| P30 | Slave Market and Brothel Dignitas tuning. | `gens-buildings-design.md` §9 |
| P31 | Veteran call-up sizing and discharge replenishment rate. | `gens-settlement-demographics-design.md` §16 |
| P32 | Annona (grain-dole) draw-down rate against Horreum stock. | `gens-settlement-demographics-design.md` §16 |
| P33 | Rural capacity vs. player-expansion land-competition tuning. | `gens-settlement-demographics-design.md` §16 |
| P34 | Multi-settlement population flow. | `gens-settlement-demographics-design.md` §16 |
| P35 | Rival Houses' contribution to Background Economic Capacity. | `gens-settlement-demographics-design.md` §16 |
| P36 | Curiales-to-marriage-market integration UI/selection mechanism. | `gens-settlement-demographics-design.md` §16 |
| P37 | Whether Aeditui should have any post-Religion-pass mobility. | `gens-settlement-demographics-design.md` §16 |
| P38 | Interest rates, rent rates, tribute formulas, tax-rate-to-Contentment-penalty curves. | `gens-economy-finance-design.md` §13 |
| P39 | Publicanus tax-farming contracts (depends on Politics & Patronage's cursus honorum). | `gens-economy-finance-design.md` §13 |
| P40 | Multi-settlement treasury consolidation. | `gens-economy-finance-design.md` §13 |
| P41 | Legal-dispute-vs-automatic threshold for asset seizure. | `gens-economy-finance-design.md` §13 |
| P42 | Debt bondage's exact default-severity threshold. | `gens-economy-finance-design.md` §13 |
| P43 | Family-bondage scope (capped at debtor + one dependent, or uncapped). | `gens-economy-finance-design.md` §13 |
| P44 | Rent/tax arrears threshold before debt-bondage ladder applies. | `gens-economy-finance-design.md` §13 |
| P45 | *Fenus nauticum* premium sizing and coverage fraction. | `gens-economy-finance-design.md` §13 |
| P46 | Villa-stage demotion reversibility after recovering solvency. | `gens-economy-finance-design.md` §13 |
| P47 | Debasement's mechanism linking severity, one-time gain, and market-price persistence. | `gens-economy-finance-design.md` §13 |
| P48 | Doctrine mutual-suppression matrix (full seven-by-seven). | `gens-policies-edicts-design.md` §10 |
| P49 | Multiple Apex Doctrines held simultaneously. | `gens-policies-edicts-design.md` §10 |
| P50 | Hybrid title stacking beyond the six named pairs. | `gens-policies-edicts-design.md` §10 |
| P51 | Debt Bondage Ban's own repeal Edict (not yet named). | `gens-policies-edicts-design.md` §10 |
| P52 | Proscription's civil-crisis qualification criteria. | `gens-policies-edicts-design.md` §10 |
| P53 | Playbook portability across multiple settlements. | `gens-policies-edicts-design.md` §10 |
| P54 | Grain Requisition's interaction with ordinary Tax Policy. | `gens-policies-edicts-design.md` §10 |
| P55 | Full Historical Timeline and Named Historical Figure roster (368-year catalog). | `gens-events-design.md` §11 |
| P56 | Divergence's exact severity threshold and downstream authoring burden. | `gens-events-design.md` §11 |
| P57 | Background House roll frequency, promotion/demotion thresholds, Net Worth bands. | `gens-rival-houses-design.md` §10 |
| P58 | Total Background House count per region. | `gens-rival-houses-design.md` §10 |
| P59 | Rival-vs-rival event frequency/visibility. | `gens-rival-houses-design.md` §10 |
| P60 | Collegia's real Economy/Politics integration depth. | `gens-rival-houses-design.md` §10 |
| P61 | Multi-settlement rival presence. | `gens-rival-houses-design.md` §10 |
| P62 | Whether Rome itself can reach a Feuding standing. | `gens-rival-houses-design.md` §10 |
| P63 | New-house-rising trigger thresholds (*novus homo* / cadet branch). | `gens-rival-houses-design.md` §10 |
| P64 | Standing-trend roll bias weighting. | `gens-rival-houses-design.md` §10 |
| P65 | Ancestral Grudge decay rate/persistence across generations. | `gens-rival-houses-design.md` §10 |
| P66 | Dossier staleness threshold. | `gens-rival-houses-design.md` §10 |
| P67 | Cadet-branch inheritance split ratio. | `gens-rival-houses-design.md` §10 |
| P68 | Distance Tier lookup table contents (per region pair). | `gens-starting-regions-design.md` §13 |
| P69 | Gazetteer entry count per region. | `gens-starting-regions-design.md` §13 |
| P70 | Whether any Gazetteer entry ever becomes ownable. | `gens-starting-regions-design.md` §13 |
| P71 | Home Anchor uniqueness under Full Custom start. | `gens-starting-regions-design.md` §13 |
| P72 | Independent hazard-layer (Reputation Duality/Diplomacy) toggles. | `gens-starting-regions-design.md` §13 |
| P73 | Multiple distant holdings simultaneously. | `gens-starting-regions-design.md` §13 |
| P74 | Autonomous romance frequency tuning. | `gens-romance-sexuality-lineage-design.md` §17; `gens-romance-seduction-design.md` §11 |
| P75 | Multiple simultaneous romantic interests. | `gens-romance-sexuality-lineage-design.md` §17 |
| P76 | Same-sex marriage's formal legal standing vs. relationship-track-only status. | `gens-romance-seduction-design.md` §11 |
| P77 | The extreme legal remedy's (§12) own trigger conditions/frequency. | `gens-romance-sexuality-lineage-design.md` §17 |
| P78 | Concubinage's interaction with dowry mechanics. | `gens-romance-sexuality-lineage-design.md` §17 |
| P79 | *Manus* vs. *sine manu* mechanical depth. | `gens-romance-sexuality-lineage-design.md` §17 |
| P80 | Exact pricing formula for slave acquisition (skill/health/age/appearance/deception weights). | `gens-labor-slavery-design.md` §12 |
| P81 | Flight-risk numeric thresholds and opportunity-roll frequency. | `gens-labor-slavery-design.md` §12 |
| P82 | Regimen tier deltas (upkeep, Health/Loyalty trend, Unrest). | `gens-labor-slavery-design.md` §12 |
| P83 | Group-default-vs-override blending on Regimen change. | `gens-labor-slavery-design.md` §12 |
| P84 | Debt-bondage-of-own-household severity gradient. | `gens-labor-slavery-design.md` §12 |
| P85 | Legal-risk trigger thresholds for punishment visibility. | `gens-labor-slavery-design.md` §12 |
| P86 | Warranty claim resolution process (Legal & Court adjudication). | `gens-labor-slavery-design.md` §12 |
| P87 | Contubernium (enslaved-pair bond) formation trigger. | `gens-labor-slavery-design.md` §12 |
| P88 | *Vilicus* vs. generic Steward output-moderation distinction. | `gens-labor-slavery-design.md` §12 |
| P89 | Villa style-package costs/effects and Grandeur Score weighting. | `gens-villa-design.md` §11 |
| P90 | Villa Grandeur milestone list (mirrors core doc's open Milestone Catalog question). | `gens-villa-design.md` §11; `gens-core-design.md` §12 |
| P91 | Disease/disaster full numeric sizing (exposure curves, contagion spread, quarantine effectiveness). | `gens-disease-public-health-design.md` §12 |
| P92 | Leprosy's exact social-exclusion magnitude/fade curve. | `gens-disease-public-health-design.md` §12 |
| P93 | Zoonotic spillover trigger threshold. | `gens-disease-public-health-design.md` §12 |
| P94 | Combat resolution formula (unit composition/terrain/commander stat weighting). | `gens-core-design.md` §12 |
| P95 | Disaster frequency/tuning against how often a given estate should plausibly be hit. | `gens-core-design.md` §12 |
| P96 | Legal system procedural depth (decision tree, case length). | `gens-core-design.md` §12 |
| P97 | Appearance attribute schema (slider/category count). | `gens-core-design.md` §12 |
| P98 | Rival house AI depth (simple utility function vs. richer model). | `gens-core-design.md` §12 |
| P99 | Milestone catalog contents. | `gens-core-design.md` §12 |
| P100 | Games & Spectacle outcome/Dignitas-payoff calculation. | `gens-core-design.md` §12 |
| P101 | Steward auto-management decision-boundary (which categories a steward may decide alone). | `gens-core-design.md` §12 |

---

## Summary

- **Queue 1 — Structural Blockers:** 11 items (B1–B11).
- **Queue 2 — Slice Tuning:** 20 items (S1–S20).
- **Queue 3 — Post-Slice Balancing:** 101 items (P1–P101).

This is not the full inventory of every "Open Questions" line in the 109 flagged design documents — most design docs also carry an "All numeric sizing... is consistent with this project's convention" catch-all line that intentionally defers *every* number in that document. Those catch-all lines are represented once per document above (folded into the relevant queue item) rather than exploded into one row per undiscovered constant; exploding them further would not change any Phase 1–9 engineering decision, only content-authoring order.
