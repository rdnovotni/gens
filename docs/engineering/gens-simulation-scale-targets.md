# GENS — Simulation Scale Targets

*This document closes Phase 1, Item 7 of the Comprehensive Build Roadmap: "define simulation scale targets for small, normal, large, and soak-test worlds." It is grounded in the roadmap's and `tech-stack.md`'s own stated performance budget rather than invented from scratch.*

## The stated performance budget (source of truth)

- **`docs/engineering/tech-stack.md`, Boundaries:** "Monthly ticks are deterministic and target 250 ms normally and one second at maximum scale."
- **`gens-comprehensive-build-roadmap.md`, current-state audit table:** "deterministic monthly ticks with a normal target below 250 ms and a one-second maximum-scale ceiling."
- **`gens-comprehensive-build-roadmap.md`, Vertical-slice acceptance test:** "Normal-scale monthly ticks meet the agreed 250 ms target on the reference machine with subsystem timing reported."
- **`gens-comprehensive-build-roadmap.md`, Phase 4 exit gate:** "a seed-defined empty campaign advances for 1,200 months, saves/loads repeatedly, and reproduces identical hashes without unbounded allocations or history growth."
- **`gens-comprehensive-build-roadmap.md`, Phase 7 exit gate:** "population, employment, housing, needs, and migration reach stable or explainably changing equilibria in seeded 50-, 200-, and 1,000-year headless runs."
- **`gens-comprehensive-build-roadmap.md`, Vertical-slice acceptance test:** "A 24-month manual playthrough and 200-year headless soak both pass."
- **`gens-comprehensive-build-roadmap.md`, Phase 10 exit gate:** "several rival actors and a delegated household survive a 200-year soak."

There are exactly two named numeric budgets in the corpus (250 ms normal, 1,000 ms maximum), plus a set of named soak durations (1,200 months / 100 years for the empty-campaign check; 50/200/1,000 years for demographics equilibrium; 200 years for the vertical-slice and rival-house soaks). The tiers below are built to be consistent with all of these rather than invent a separate scale.

---

## Scale tiers

| Tier | Households (named, Familia-tracked) | Settlements | Total population (named + background) | Campaign duration | Tick time budget | Expected save size |
|---|---|---|---|---|---|---|
| **Small** | 1 player household (6–10 named) + 1–2 rival seeds | 1 | ~150–300 (matches the vertical-slice §2 default of 220 background + the named roster) | 24 months (manual) up to 50 years (headless) | ≤ 100 ms/tick — well under the 250 ms normal target, since this is the minimum viable scale the budget is set against | ≤ 5 MB |
| **Normal** | 1 player household + 5–15 rival houses across 2–4 settlements | 2–4 | ~2,000–6,000 (background pop groups scaled per settlement per Settlement Demographics §3) | 200 years (matches the roadmap's own repeatedly-named 200-year soak: vertical-slice acceptance test, Phase 10 exit gate) | ≤ 250 ms/tick — this **is** the named "normal target below 250 ms" | ≤ 50 MB |
| **Large** | 1 player household + 30–60 rival houses across 6–10 settlements, multiple regions active (Phase 13+) | 6–10 | ~20,000–60,000 | 1,000 years (matches Phase 7's named 1,000-year demographic-equilibrium run) | ≤ 600 ms/tick — inside the 1-second maximum-scale ceiling with headroom for subsystem variance | ≤ 300 MB |
| **Soak-test** | Maximum content-supported scale: full rival-house population across all active regions, deep multi-generational succession chains | All available (17 starting regions once Phase 13 content lands) | Upper bound the content catalog supports, unbounded by design but must not exceed the ceiling below | 1,200+ months minimum (Phase 4's own empty-campaign check), extended to multi-thousand-year runs for long-horizon determinism/leak testing | ≤ 1,000 ms/tick — this **is** the named "one-second maximum-scale ceiling"; a soak run that exceeds it is a regression, not a tuning question | No hard cap, but must grow sub-linearly with elapsed months (Phase 4 exit gate: "without unbounded allocations or history growth") — flag any run whose save size grows faster than population + history-entry count |

All headcounts, settlement counts, and save-size figures are **proposed defaults, needs playtesting** — the corpus does not name population or save-size ceilings anywhere. Only the two tick-time budgets (250 ms, 1 s) and the named campaign durations (1,200 months / 50 / 200 / 1,000 years) come directly from the roadmap and `tech-stack.md`.

---

## Mapping to CI / test strategy

- **Small** is the tier every pull request's automated suite runs against: unit/property tests, the golden-seed replay check, and the save/migration fixtures from Phase 3. It must run fast enough to execute on every PR without slowing CI, so its own tick budget target is deliberately tighter (≤100 ms) than the "normal" 250 ms contract — this tier exists to catch regressions long before they'd show up at normal scale.
- **Normal** is exercised by the required CI suite at a lower frequency than Small (e.g., nightly or pre-merge-to-main rather than every PR): the 200-year headless soak matching the vertical-slice acceptance test and Phase 10's rival-survival exit gate. This is the tier the "Normal-scale monthly ticks meet the agreed 250 ms target... with subsystem timing reported" acceptance criterion is written against, so its CI run should assert against the 250 ms figure directly and fail the build if the reported subsystem timing regresses past it.
- **Large** is a scheduled (e.g., weekly) CI job, not a per-PR or per-merge gate — consistent with Phase 7's own framing of the 1,000-year run as a periodic equilibrium check rather than a routine one. It validates the 1-second ceiling holds once multiple regions and dozens of rival houses are active, and is where BenchmarkDotNet's monthly-tick benchmark (named in `tech-stack.md`'s Verification section) should be run at full scale.
- **Soak-test** is a manual or long-running background job (per Phase 18's own "1,000-year soaks, deterministic cross-platform comparisons" item), used before major milestone cuts rather than on any routine cadence — it is explicitly the tier where "no hard cap" on save growth needs a human to read the trend line, not just an automated pass/fail. Determinism (identical state hashes across repeated runs, per the Phase 2 exit gate) is the primary signal at this tier, since absolute population size is unbounded by design.

In short: Small gates every PR, Normal gates merges to main and is the one tier with a named, non-negotiable millisecond budget, Large is a scheduled regression watch on the maximum-scale ceiling, and Soak-test is a pre-release/pre-milestone manual exercise focused on long-horizon determinism and unbounded-growth detection rather than a strict per-tick number.
