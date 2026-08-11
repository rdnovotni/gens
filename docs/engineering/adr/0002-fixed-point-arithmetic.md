# ADR 0002 — Fixed-Point and Integer Arithmetic

**Status:** Accepted

## Context

Non-negotiable rule 4: "Integers or named fixed-point values are authoritative. Floating point may be used in presentation but not for campaign outcomes." `tech-stack.md` confirms: "Simulation outcomes use integer values." Roadmap Phase 8 sharpens this for money specifically: "double-entry-style household and actor ledgers using integer minor units."

The design corpus is mostly integer-friendly already: Core Attributes, Labor Skills, and Condition stats are "numeric 0–100" (`gens-familia-design.md` §2.1–2.3); Personality Axes are "-100 to 100" (`gens-characters-design.md` §5); relationship opinion is "-100 to 100" (`gens-familia-design.md` §2.7); Scheme `progress` is "0-100" (`gens-characters-design.md` §14). But the corpus also names several genuinely fractional quantities with no stated representation: `gens-settlement-demographics-design.md` §4.2 defines the Employment Ratio as "available background job slots in a sector divided by that pop group's own size" — a ratio that is well above or below 1.0, not an integer; `gens-economy-finance-design.md` §12's `DebtRecord.interestRate` and `MintPolicy.debasementSeverity`/`seigniorageRate` are rates, not counts; `gens-resources-goods-design.md` §3.2's Herd Strategy trades "faster headcount recovery" against "faster current output" — an implied rate; `gens-estate-settlement-design.md` §6's specialization bonus is described as "compounding" without a stated formula shape. None of these can be honestly represented as bare integers without silently truncating, and none may be represented as `double`/`float` without violating rule 4.

## Decision

Two numeric representations, both integer-backed, used according to what the quantity *is*:

1. **Integer minor units** for money and headcounts. Denarii is stored as a signed 64-bit integer of minor units (1 denarius = 100 minor units, matching Phase 8's "integer minor units" instruction), never as a decimal or float. Livestock headcount (`gens-resources-goods-design.md` §16 `LivestockStock.headcount`), population size (`gens-settlement-demographics-design.md` §15 `PopGroup.size`), and every 0–100/-100–100 stat already named by the corpus stay plain signed 32-bit integers — they need no fractional precision and the design consistently states them as whole numbers.
2. **`Fixed64`**, a `readonly record struct` wrapping a `long` scaled by a fixed implicit denominator of 1,000,000 (parts-per-million), for every quantity the corpus describes as a rate, ratio, or multiplier without committing to a scale: Employment Ratio, interest rates, debasement severity, herd-strategy growth/yield multipliers, specialization bonus curves, and gift-value multipliers (`gens-resources-goods-design.md` §16 `giftValueMultiplier`). `Fixed64` supports only deterministic integer-domain operations (add, subtract, scaled multiply/divide with explicit round-half-to-even) — no conversion to or from `double` anywhere in `Gens.Simulation`. A `ToDisplayString()` on the presentation boundary (ADR 0013) is the only sanctioned path to a human-readable fractional number.

Where the design corpus leaves a formula's actual shape open (which is nearly everywhere per each document's own "Open Questions" section, e.g. `gens-estate-settlement-design.md` §9's "Specialization bonus curve" or `gens-economy-finance-design.md` §13's "Net Worth depreciation formula"), `Fixed64` is the substrate that formula will be authored against once numerically sized — this ADR only commits to the representation, not the values, consistent with the corpus's own stated numbers-later convention.

## Consequences

- Every new numeric field added to a state record must be classified as one of: plain `int`/`long` count, integer minor-unit money, or `Fixed64` rate — there is no fourth option and no raw `double` field is ever added to simulation state.
- Deterministic hashing (Phase 2's state-hash exit gate) is safe by construction: `Fixed64` and `int` both hash bit-for-bit identically across platforms, which IEEE-754 `double` is not guaranteed to do across .NET runtimes/architectures.
- A small `Fixed64` math library (add/sub/mul/div/compare, saturating on overflow rather than silently wrapping) is a Phase 2 prerequisite, gating any system that reads Employment Ratio, interest, or bonus curves.

## Alternatives Considered

- **`decimal`.** Rejected: not blittable, notably slower, and still not guaranteed bit-identical across all supported runtimes/IL2CPP; offers no benefit over a purpose-built `Fixed64` for this project's actual precision needs (parts-per-million comfortably covers every rate the corpus names).
- **Bare `double` "for now, tighten later."** Rejected outright by rule 4; deferring this decision would let non-determinism leak into every system built before the tightening pass, exactly the trap Phase 2's exit gate ("same seed plus same ordered commands produces identical event logs and state hashes") is designed to prevent.
- **A single scale for both money and rates.** Considered and rejected: money's natural precision (whole minor units) and rate precision (parts-per-million) differ by orders of magnitude; forcing one scale would either waste range on money or starve precision on rates like debasement severity.
