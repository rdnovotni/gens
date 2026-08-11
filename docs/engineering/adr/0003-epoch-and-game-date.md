# ADR 0003 — Epoch and GameDate

**Status:** Accepted

## Context

The roadmap's current-state audit is blunt about this specific gap: "Time | Placeholder | `TotalMonths` needs a defined epoch, BCE/CE mapping, calendar rules, and display conversion." Phase 2, item 3 makes it a required deliverable: "Replace bare `GameDate(TotalMonths)` semantics with a defined epoch, historical-year conversion, BCE/CE display, month-of-year, and overflow-safe arithmetic. Keep the authoritative representation compact."

The code already exists and is exactly the placeholder described: `src/Gens.Simulation/Time/GameDate.cs` defines `readonly record struct GameDate(int TotalMonths)` with a single `NextMonth()` using `checked` arithmetic (so overflow already throws rather than wrapping — worth preserving, not fixing). `src/Gens.Simulation/Time/MonthlySimulation.cs` already ticks against `GameDate` without any calendar semantics attached.

The design corpus assigns real calendar ownership elsewhere and deliberately does not resolve it here: per the design-authority registry cluster 15, `gens-events-design.md` §6.2 owns the `GameCalendar` (starting year, current year/month, era) and the bounded historical range; `gens-roman-calendar-design.md` owns "real month names, real day-counting, the real Julian calendar reform, the market cycle, and year-reckoning" as "the structural layer sitting underneath" both Events' `GameCalendar` and Religion's sacred calendar. Roadmap Phase 4 names `gens-roman-calendar-design.md` and "the time-scale sections of `gens-core-design.md`" as primary inputs for the bootable headless campaign shell — i.e., day-level and named-month semantics are not required until Phase 4, but the month-counting epoch this ADR fixes must already be stable by then, since nothing later can renumber history out from under existing saves.

## Decision

`GameDate.TotalMonths` remains the sole authoritative, persisted representation of simulation time — a compact `int`, month-granular, per the roadmap's explicit "keep the authoritative representation compact" instruction. Everything else in this ADR is about what `TotalMonths` means and how it is displayed, not about replacing it.

- **Epoch:** `TotalMonths = 0` is January of astronomical year **-753** (i.e., 754 BCE proleptic Julian, one year before Rome's traditional founding date), giving every campaign room to start anywhere from the earliest plausible starting-region date forward without a negative `TotalMonths` in ordinary play, while leaving headroom before it for edge-case tooling (replay diagnostics, historical-timeline authoring) that needs pre-epoch dates.
- **Conversion is a pure, stateless function**, not stored state: `TotalMonths → (astronomicalYear, monthOfYear)` by integer division/modulo (`checked`, matching the existing `NextMonth()` convention), and a separate, explicitly-named `ToDisplayYear()` maps astronomical year to the BCE/CE label a player sees (astronomical year 0 = 1 BCE; there is no year 0 in BCE/CE display, so the mapping subtracts one crossing that boundary). This conversion function is the seam `gens-roman-calendar-design.md`'s real month names and `gens-events-design.md`'s `GameCalendar` era/year fields are expected to sit on top of — this ADR commits to the epoch and the month-counting contract those documents assume exists but never specify themselves.
- **Overflow safety:** arithmetic stays `checked`, matching the existing `GameDate.NextMonth()`; a campaign that somehow runs past `int.MaxValue` months (over 178 million years) fails loudly rather than silently wrapping into a corrupt date — an acceptable bound given the roadmap's own soak-test target is measured in centuries, not eons.

## Consequences

- Save compatibility is anchored the moment this ADR ships: changing the epoch afterward is a save-breaking change requiring a migration (ADR 0011), so this decision should not be revisited casually once golden save fixtures exist.
- `gens-roman-calendar-design.md`'s day-level model (Julian reform, market week) and `gens-events-design.md`'s `GameCalendar` era field both layer on top of this ADR's conversion function rather than reopening `TotalMonths`' meaning — neither document is itself an engineering contract, so this ADR is what makes their assumptions buildable.
- Any future day-granular feature (if ever needed) requires a second, explicitly-scoped ADR — this one deliberately keeps the authoritative tick at month granularity per the roadmap's instruction and the corpus's own monthly-tick design throughout (every "Automation Principle" section in Resources & Goods, Settlement Demographics, and Economy & Finance describes "every month-tick" resolution, never a day-level one).

## Alternatives Considered

- **Epoch anchored at a real historical date (e.g., 1 CE) with a signed year field stored directly**, rather than a derived astronomical-year conversion. Rejected: stores redundant, derivable state (a `year` field alongside `TotalMonths` can drift or be migrated inconsistently); a pure conversion function has no state to keep in sync.
- **Day-granular authoritative tick now**, anticipating `gens-roman-calendar-design.md`'s eventual day-counting. Rejected as premature: no vertical-slice system (Phases 5–9) requires day resolution, and it would multiply `WorldState` size and hashing cost for a feature not yet scheduled.
- **Deferring the epoch decision entirely** until `gens-roman-calendar-design.md` gets its own engineering pass. Rejected: Phase 4's headless campaign shell needs a stable, saveable date now, and every phase after it writes dates into saves — deferring risks a Phase-4-era save that a later epoch choice would break.
