# GENS — System Design: The Roman Calendar (§6.8.1, new)
*The structural layer sitting underneath Events' own GameCalendar (§6.2 of that document) and underneath Religion's own sacred calendar (§5 of that document) — this document owns real month names, real day-counting, the real Julian calendar reform, the market cycle, and year-reckoning, none of which the two documents above ever specified. It does not own festivals or feast days; Religion made that ownership decision explicitly ("Religion owns the sacred calendar and its feast days"), and this document's job is to give that existing festival table the real structural calendar to actually sit on.*

---

## Contents

1. Scope & Role
2. Two Calendars in One Historical Range
3. The Roman Months
4. Kalends, Nones & Ides — How Romans Actually Counted Days
5. The Nundinal Cycle — The Roman Market Week
6. Reckoning the Year — AUC, Consular Dating, and the Game's Own BC/AD Convenience
7. The Roman Day — Unequal Hours
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role

Events already established that every playthrough runs against a real, bounded calendar (133 BC – AD 235) and tracks a `GameCalendar` with a starting year, a current year and month, and an era. Religion already established that it owns the sacred calendar's actual feast days, sitting on the calendar year at fixed points. Neither document specified what the calendar itself actually looks like — real month names, how Romans actually counted days within a month, what a "market week" was, or how the game's own two eras (before and after Julius Caesar's real calendar reform) should read differently. This document is exactly that missing structural layer, adding nothing that competes with either document's own existing ownership.

---

## 2. Two Calendars in One Historical Range

This game's own 133 BC – AD 235 range genuinely spans a real, significant calendar reform, and this document treats that as a live structural fact rather than an invisible background detail.

**Before 46 BC — the Republican calendar.** A real, notoriously irregular lunar-solar hybrid, kept roughly aligned to the seasons only by periodic intercalation — inserting an extra short month when needed. Critically, that decision belonged to the **Pontifex Maximus**, a real, prestigious religious office (Religion §6.2's own state priesthood track), and real Roman history records that office being used for genuine political manipulation: extending or shortening a sitting magistrate's own term by choosing whether or not to intercalate that year. This is a real, concrete Politics & Patronage-adjacent lever available specifically to a pre-46 BC playthrough's own Pontifex Maximus — the calendar itself as a tool of political advantage, not merely a backdrop.

**From 46 BC onward — the Julian calendar.** Caesar's own real reform fixed the year at 365 days with a leap day every fourth year, ending discretionary intercalation entirely. This document flags the reform's real date directly as the natural moment Events' own Historical Timeline should mark it, and treats it as the hinge between two genuinely different calendar behaviors within the same playthrough range — a household starting before 46 BC experiences the older, politically-manipulable calendar; nearly every other playthrough experiences the regular one Rome used for the rest of this game's own history.

---

## 3. The Roman Months

Twelve real named months, unchanged in count across the reform (the reform fixed day-lengths and intercalation, not the roster of months itself): **Ianuarius, Februarius, Martius, Aprilis, Maius, Iunius, Quintilis, Sextilis, September, October, November, December.**

A real, genuinely fun etymological detail worth stating directly: September through December literally mean "seventh" through "tenth" month — because the Roman civil year originally began in March, not January, with the New Year's shift to January 1 only becoming standard practice a real, historically-recorded generation before this game's own range even opens (153 BC). By this game's own era, January 1 is already the firmly established start of the civil and consular year; the old March-based numbering simply survives, fossilized, in four month names nobody bothered to rename.

**Two months *were* renamed, and this document ties both directly to Events' own Named Historical Figures:** Quintilis became **Iulius** (July), honoring Julius Caesar, and Sextilis became **Augustus** (August), honoring the first Emperor — both real, both landing on real, dateable points in the Historical Timeline (Events §6.4), giving a Dynasty Chronicle entry written after either date a real, small piece of texture: a household's own correspondence dated "the Kalends of Sextilis" before the rename, "the Kalends of Augustus" after it.

---

## 4. Kalends, Nones & Ides — How Romans Actually Counted Days

Real Romans didn't number days 1 through 30 the way this project's own player-facing UI naturally will. They counted backward from three fixed points each month: the **Kalends** (always the 1st), the **Nones** (the 5th or 7th, depending on the month), and the **Ides** (the 13th or 15th, depending on the month) — a date was expressed as "X days before" whichever of the three came next, not as an ascending count from the start of the month.

This document doesn't ask the game's own player-facing calendar to abandon simple day numbers — that would be a usability regression for no real gain — but it does establish Kalends/Nones/Ides as the **flavor-dating convention** available anywhere the game wants authentic texture: a Dynasty Chronicle entry, an in-world letter, a formal legal document. The single most famous real date on this entire project's own Historical Timeline is already expressed this way in real history and deserves to be in-game too: the Ides of March — Caesar's own real assassination date — reads as exactly that phrase in any flavor-dated context, not "March 15th."

---

## 5. The Nundinal Cycle — The Roman Market Week

Rome's real native "week" wasn't seven days — it was an eight-day cycle called the **nundinal cycle**, with the eighth day, the *nundinae*, functioning as the real market day: rural producers came into town to sell, business and legal proceedings often paused, and Economy & Finance's own Market Dynamics (that document's own existing pricing and trade-route model) can read the nundinal cycle directly as a real, periodic liquidity spike distinct from the ordinary steady-state market — goods move and sell more readily on a market day than on an ordinary one, a real, authentic texture layered on top of that document's existing mechanics rather than a new pricing model.

A real, genuinely interesting parallel worth naming: the familiar **seven-day planetary week** (dies Solis, dies Lunae, and so on — the direct ancestor of the modern Sunday-through-Saturday week) existed alongside the official nundinal cycle for real, growing stretches of this game's own range, spreading informally alongside astrology's own rising popularity rather than replacing the nundinal cycle officially. This document treats the seven-day week as real, era-appropriate flavor — increasingly natural to reference in a later-range playthrough, genuinely anachronistic-feeling in an earlier one — rather than the game's own primary time unit, which stays the real nundinal cycle throughout.

---

## 6. Reckoning the Year — AUC, Consular Dating, and the Game's Own BC/AD Convenience

A real, worth-stating-directly fact: no Roman ever said "133 BC." The BC/AD system is a much later retrospective convention this project's own Events document uses purely for player-facing clarity — a reasonable, necessary simplification, not a claim about how Romans actually experienced time. Two real, authentic alternatives exist for exactly the flavor-dating role §4 already established for Kalends/Nones/Ides:

- ***Ab Urbe Condita* (AUC)** — dating from Rome's own legendary founding (753 BC), giving any in-game year a real, alternate numeral a household's own correspondence or Dynasty Chronicle entry can use instead of a BC/AD figure.
- **Consular dating** — the real, dominant Roman practice: naming a year by its two sitting consuls ("in the consulship of Cicero and Antonius"), which this document ties directly to Politics & Patronage's own Curia and cursus honorum records — any year in the game's own range where the player's household, a Named Historical Figure, or a Rival House holds the consulship has a real, concrete, flavorful year-name available for exactly this purpose.

Events' own BC/AD `GameCalendar` stays the system's authoritative, mechanical date — this section adds authentic, optional flavor-dating on top of it, exactly the same relationship §4's Kalends/Nones/Ides already has to simple day numbers.

---

## 7. The Roman Day — Unequal Hours

A brief, real, flavor-only detail: Romans divided daylight into twelve *horae*, regardless of season — meaning a Roman "hour" in high summer was real-historically longer in absolute terms than one in midwinter, since twelve hours always had to fit between a given day's own sunrise and sunset. This document doesn't ask any other system to model variable-length hours mechanically — the game's own monthly tick resolution has no real use for sub-day granularity — but flags it as real, authentic texture available to any future flavor text wanting to describe "the third hour of a short winter day" versus "the third hour of a long summer one" accurately.

---

## 8. Cross-System Integration

- **Events:** this document is a direct structural extension of that document's own `GameCalendar` (§6.2 of that document) — real month names, real day-counting, and the Julian reform's own real date all read into fields that document's own data model didn't previously specify; the reform's real 46 BC date is a natural addition to that document's own Historical Timeline.
- **Religion:** explicitly deferred to for all feast-day and festival content (§5 of that document) — this document supplies the calendar Religion's own festival table sits on, and adds nothing competing with it.
- **Economy & Finance:** the nundinal cycle (§5) is a real, periodic input into that document's own existing Market Dynamics, not a new pricing model.
- **Politics & Patronage:** consular dating (§6) ties directly to that document's own Curia and cursus honorum office records; pre-46 BC intercalation (§2) is a real, concrete lever available specifically to a Pontifex Maximus officeholder.
- **Dynasty Chronicle:** Kalends/Nones/Ides and consular/AUC dating (§4, §6) are both real, authentic flavor-dating options for any Chronicle entry wanting period-accurate texture.
- **Starting Regions (framework):** Start Mode's own Starting Year selection (Events §6.2) determines directly whether a given playthrough experiences the pre- or post-reform calendar (§2).

---

## 9. Data Model

```
CalendarSystem {                     // extends Events' own GameCalendar — §6.2 of that document
  householdId,
  usesJulianCalendar: bool,          // false before 46 BC, true from 46 BC onward — §2
  currentMonthName,                  // one of the twelve real month names — §3
  monthRenamesActive: {              // §3 — flips true on each real historical rename date
    quintilisToIulius: bool,
    sextilisToAugustus: bool,
  },
}

FlavorDateFormat {                   // §4, §6 — optional, non-mechanical presentation layer
  realDayOfMonth,
  kalendsNonesIdesExpression,        // e.g. "the Ides of March"
  aucYear,
  consularYearName,                  // nullable — populated only when a known consular pair exists for that year
}

NundinalCycle {                      // §5
  settlementId,
  currentCyclePosition,               // 1-8
  isMarketDay: bool,                  // true on day 8 — a real, periodic Economy & Finance liquidity input
  sevenDayWeekFlavorAvailable: bool,   // era-gated — more natural to reference later in the range than earlier
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — the nundinal market day's own actual liquidity/price bonus, and how strongly a pre-46 BC Pontifex Maximus's intercalation choice should actually affect term length, are both unsized.
- **Whether flavor-dating (§4, §6) should ever be player-selectable as a display preference** rather than purely narrative texture in Chronicle/correspondence contexts — this document treats it as flavor-only, but a settings toggle letting a player see Kalends/Nones/Ides or consular-year dates throughout the UI is a real, plausible future option.
- **Consular Year Name coverage.** §6 ties flavor-dating to Politics & Patronage's own consulship records, but doesn't specify what a year's own consular name defaults to when no household or Rival House happens to hold the office that year — likely a generated placeholder pair, left to a future pass.
- **The Pontifex Maximus's own actual intercalation mechanic.** §2 names the real historical lever directly but doesn't specify its own trigger conditions, frequency, or exact effect on a term's length.
