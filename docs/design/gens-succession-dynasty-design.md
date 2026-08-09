# GENS — System Design: Succession & Dynasty (§6.9)
*The mechanism behind "no forced ending" — every death, retirement, and generational transition has somewhere real to go. Adoption folds in here per the core doc's own note; Legitimacy's eligibility gate, Economy & Finance's inheritance, Military & Combat's battlefield-death handoff, and Rival Houses' extinction trigger and succession reuse all resolve against what this document builds.*

---

## Contents

1. Scope & Role
2. Heir Designation
3. Legitimacy & Eligibility
4. Adoption
5. Succession Drama
6. The Handoff
7. Extinction
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role

The core doc's own framing: "no fixed generational endpoint; succession is primarily player-chosen, layered with optional Crusader-Kings-style succession drama... when circumstances make it compelling." Adoption is named directly as folding into this system rather than standing alone. This document is where every other system's forward reference to death, inheritance, and generational continuity actually resolves:

- **Familia §5.2's Legitimacy gate** — this document is where that gate actually determines heir eligibility.
- **Familia §3's lifecycle table** — "retirement from active duties becomes an available choice rather than a forced one" — this document is where that choice's actual consequence (a handoff, not just a status change) gets built.
- **Economy & Finance §3.4's Windfalls** — inheritance is named there as a Windfall category; this document is its source.
- **Military & Combat §5.5** — a player-character battlefield death is explicitly stated to "hand off directly through Succession & Dynasty's own inheritance resolution... rather than triggering any battle-specific game-over state." This document is that resolution.
- **Rival Houses §3.2 and §5.3** — that document explicitly commits to "reusing that system's inheritance resolution wholesale" for any House of Note's Head, and names extinction as its own trigger condition without designing extinction itself.

Per direction, this document also resolves a real tension directly rather than smoothing it over: "no forced ending" means the game never declares an artificial win or loss *despite* the simulation still having someone to control — it does not mean the simulation is rigged to prevent a genuine dead end from ever being reachable. §7 handles this distinction explicitly.

---

## 2. Heir Designation

Per the decision to build both: a quiet, always-adjustable default, and an optional formal act that actually carries weight.

### 2.1 The Default Preference

At any time, the current head can set or change a **preferred heir** from the eligible pool (§3) — a lightweight, cost-free, reversible setting, more a running intention than a binding decision. If the head dies or retires with only a default preference in place (or none at all), it resolves quietly using §2.4's fallback order, without triggering any of §5's drama on its own.

### 2.2 Formal Declaration

A real, occasional in-game action — a Curia announcement or household ceremony, not a menu toggle — that locks the choice in with genuine stakes on both sides. Declaring a formal heir:

- Grants a real Dignitas boost (a stability signal Rome and rivals alike read favorably) and a Loyalty lift for the declared heir specifically.
- Meaningfully **reduces** §5's succession-drama likelihood — a formally declared heir has a real legitimacy a mere quiet preference doesn't carry, the concrete mechanical reason to actually use this over just setting §2.1's preference and moving on.
- Is **not** free to reverse: replacing a standing Declaration with a new one costs the displaced heir's opinion sharply and carries a real Dignitas cost of its own for the public reversal — a genuine tradeoff against flip-flopping, not a free do-over. **This cost is specifically for discretionary reversal** — swapping a Declaration because a better option came along, or because the head simply changed their mind. It doesn't apply when the standing heir has become genuinely ineligible through no discretionary choice (death, a disqualifying Legitimacy reversal, a Legal & Court ruling) — replacing an heir who's no longer eligible at all is a necessity, not a reversal, and costs nothing extra.

### 2.3 Disownment

The sharper tool §5.2 references directly: a head can formally **disown** an eligible heir, removing them from the pool (§3) entirely rather than merely passing over them in a Declaration. Disownment is not a quiet decision — it costs real Dignitas (a public family rupture reads badly regardless of cause) and permanently damages the relationship-web opinion between the disowned party and everyone who stayed loyal to the head's decision, not just the head themselves. It is reversible only through an equally deliberate act of reconciliation, never automatically, and a disowned heir is exactly the kind of aggrieved party §5.3's splinter-house outcome or a Rival Houses adoption (§4) is the natural next chapter for — disownment doesn't erase a person, it just closes one door for them.

### 2.4 Default Fallback Order

Where neither a Declaration nor even a quiet preference exists, the historically-accurate default applies: agnatic-line, eldest-legitimate-son-first succession. Consistent with this project's established pattern (Familia §2.5's own historical-restriction toggle), a player-configurable option set at game start can relax this to birth-order-only or fully player-neutral succession instead, without removing the historical default as the baseline.

---

## 3. Legitimacy & Eligibility

Restating and completing Familia §5.2's rule now that it has somewhere to fully apply. The eligible heir pool is:

- **Legitimate children** — eligible by default, no action required.
- **Acknowledged illegitimate children** — eligible only after the explicit acknowledgment Familia §5.2 already requires; unacknowledged illegitimate children are never in the pool at all.
- **Adopted children** (§4) — fully eligible, identical standing to a legitimate birth child from the moment the adoption completes.
- **A surviving spouse** — not usually a permanent heir, but a real, historically-grounded option when no adult heir yet exists: a widow or widower can hold the estate **in trust** rather than the line simply passing to whoever's biologically next, buying time for a minor heir to come of age (§6.2's Regency covers the mechanical version of this).

---

## 4. Adoption

Familia §5 already sketched this directly: adoption "uses the same Core Attribute and relationship-web evaluation as a marriage candidate would... a promising young client, a rival house's spare son, a distinguished freedman's child" — real Roman practice, Augustus's own succession chief among the historical examples. This document builds the actual mechanic:

- **Candidates** are drawn from exactly the pools already named: Clientela members (Politics & Patronage), a rival house's spare or cadet-eligible son (a real, direct alternative to Rival Houses §2.2's own peaceful cadet-branch split — the same son, absorbed into the player's own line instead of founding a separate one; the two outcomes are mutually exclusive for a given individual, whichever happens first), a distinguished freedman's child, or a Companion.
- **Resolution** runs through Characters' existing Propose Adoption interaction (§9.1 of that doc) — the same weighted-comparison shape a marriage proposal already uses, not a new formula.
- **A real cost worth stating plainly:** adopting an outside candidate over an existing blood heir who expected the position is a genuine slight — a real risk of a Resentful or Rebellious reaction from the passed-over child, and exactly the kind of condition §5.1 lists as a succession-drama trigger in its own right.

---

## 5. Succession Drama

Per the decision to combine all three approaches rather than pick one.

### 5.1 What Triggers Contested Succession

Three inputs, layered rather than exclusive:

- **Trait- and relationship-driven conditions** — multiple eligible heirs with genuinely high Ambition, a Rebellious/Wayward/Power-Hungry heir apparent, a contested Legitimacy case, or a blood heir resentful of an adopted one (§4) — the specific, already-modeled conditions that make a contest plausible in the first place.
- **Prominence scaling** — the more Dignitas and Net Worth actually at stake, the more likely a transition draws a real challenge, independent of exactly who the heirs are.
- **A player-set frequency toggle**, at game start, alongside the fertility/childbirth-risk and historical-restriction toggles this project already establishes — from rare/off to frequent — scaling how readily the conditions above actually escalate into real drama rather than resolving quietly despite technically being present.

### 5.2 The Contest Itself

Once triggered, a rival claimant — a sibling, a resentful passed-over heir, even a cadet-branch cousin — becomes a real Character (Characters §11's lazy instantiation, if not already one) pursuing a genuine claim through the systems that already exist for exactly this: a Legal & Court (§6.16, future) challenge to a Declaration's validity, a Scheme (Characters §10) undermining the chosen heir's standing or legitimacy, or, in the harshest case, open Coercive action — a family turning on itself, resolved at whatever scale actually fits (an Interaction-level confrontation, or, at the extreme, a Military & Combat Private Feud fought within a single house rather than between two). **While the current head still lives**, this is the player's real intervention point: favor a side, disown a claimant outright (§2.3), or mediate a settlement, all through the ordinary Interaction Catalog rather than a special succession-only tool.

### 5.3 A Lost Claim Isn't the End — Splinter Houses

A genuinely satisfying answer to "what happens to the loser": a rival claimant who loses a succession contest doesn't have to mean death or disappearance. Taking a share of the inheritance and walking away to found a new, separate house is the *bitter* mirror of Rival Houses §2.2's own peaceful cadet-branch split — the same mechanical outcome (a new LivingWorldActor, related in origin, now fully independent), reached through rupture instead of agreement. **This is also the single most natural origin for a Rival Houses §5.2 Ancestral Grudge** — a splinter house founded on a lost succession fight already carries exactly the kind of severe originating event that Standing modifier was built to represent, with no extra mechanism required to produce it.

---

## 6. The Handoff

### 6.1 Death vs. Retirement

Control passes to the new head the moment the old one dies **or formally retires** — Familia §3's own lifecycle table already flagged retirement as "an available choice rather than a forced one" without defining what it actually triggers; this is that definition. A death is permanent, Chronicled, and final for that character. A retirement is not: the retired head remains a living, un-controlled Familia member — an Elder Statesman the new head can still consult, whose opinion and Loyalty still matter, and who can plausibly fill an advisory Companion-style role rather than simply vanishing from the household the moment they step back. Either way, the transition is a genuine Dynasty Chronicle (§6.11, future) entry — "the torch passes" is a real narrative beat, not a silent stat swap — and every relationship, every piece of history, carries forward unbroken with the new head, since continuity of memory across generations is the entire point of a dynasty game.

### 6.2 Regency — When the Heir Is a Minor

A real gap worth closing directly: an heir who's still a Child or Adolescent (Familia §3) at the moment of handoff can't simply become a fully agentic head overnight. A **Regent** — the surviving spouse (§3's in-trust option), or, failing that, the household's own highest-ranking appointee (a Rationalis or Procurator, Companions & Court Positions) — runs the estate on the minor heir's behalf until they come of age, using exactly the Steward/Council Auto-Management (§6.28, future) principle already established for a head who's away on Travel: sensible default handling of routine business, with anything genuinely consequential held rather than decided unilaterally.

**Worth stating plainly who the player actually plays during this stretch.** Where the Regent is the surviving spouse, the player controls the Regent directly — a real, if interim, protagonist, not a spectator, since a materfamilias holding the line for her son is exactly the kind of story this project should be able to tell. Where the Regent is a non-family appointee instead (no surviving spouse, or one who's declined/unable to serve), the player has no single character to directly control until the heir comes of age — this stretch runs on Steward/Council auto-management alone, the same way an away-on-Travel household does, and the player's real point of contact is the same automation-plus-report pattern that context already uses rather than a scene-by-scene presence.

---

## 7. Extinction

### 7.1 The Default — A Real, Rare, Honestly-Earned Ending

Per the decision to make this a real possible outcome rather than something the game quietly prevents: if the player's own house genuinely runs out of heirs — no eligible child, no viable adoption candidate, everyone dead — the line ends. This is not a punitive game-over screen; it's the natural, earned conclusion of everything that happened across the whole playthrough, resolved the same way Rival Houses §5.3 already resolves any extinct house's holdings (a Legal & Court ruling, a Politics & Patronage land grant, or Military & Combat conquest, whichever actually fits), now simply applied to the player's own gens instead of an NPC one. The Dynasty Chronicle compiles a genuine closing account rather than a blank stop. **The distinction worth being precise about:** "no forced ending" means the *game* never artificially declares a win or loss while there's still someone to play — it was never a promise that biological extinction is impossible. A dead end reached honestly, through real choices or real bad luck, is a legitimate ending a player arrives at, not one the game forces on them.

### 7.2 Accessibility Toggle

Per the decision to make the harder truth the default while still offering real alternatives: a game-start setting, in the same family as the fertility/childbirth-risk and succession-drama-frequency toggles above, offers real range:

- **Realistic** (default) — §7.1 stands as written; extinction is rare but genuinely reachable.
- **Safety Net** — a last-resort candidate (a distant, previously-unknown cognate relative, or a sufficiently loyal freedman/Companion) reliably surfaces even at a true dead end, giving a player who wants the drama without the actual risk of loss a way out.
- **Extinction Off** — the game guarantees, by construction, that at least one theoretical adoption candidate is always available, removing this failure state entirely for a player who'd simply rather never face it.

---

## 8. Cross-System Integration

- **Familia:** §5.2's Legitimacy gate and §3's retirement flag both get their full, actual mechanism here rather than a forward reference.
- **Characters:** Propose Adoption (§9.1) and the Scheme engine (§10) are reused directly for §4's adoption resolution and §5.2's contested-succession resolution respectively — no parallel mechanic invented.
- **Economy & Finance:** inheritance (§3.4's Windfalls) is this document's concrete source; Insolvency's own aftermath (§9 of that doc) is a real complicating factor a struggling house's succession has to reckon with.
- **Military & Combat:** §5.5's explicit no-safety-net battlefield-death routing is fully honored — a player-character death in battle resolves through this document exactly like any other death would.
- **Politics & Patronage:** a formal Declaration (§2.2) is a real Dignitas event in that document's own economy; a contested succession fought through Legal & Court or as a land-grant dispute touches that system directly.
- **Rival Houses:** §5.3's own extinction trigger and explicit "reuse this system's inheritance resolution wholesale" commitment are both fully realized; §2.2's cadet-branch mechanic and this document's §5.3 splinter-house outcome are confirmed as two paths (amicable, bitter) to the exact same result; a splinter house is the cleanest possible origin for that document's own Ancestral Grudge.
- **Companions & Court Positions:** §6.2's Regency reuses the Rationalis/Procurator role directly rather than inventing a new appointment.
- **Steward/Council Auto-Management (§6.28, future):** §6.2's Regency is this document's own application of that system's exact "sensible defaults, consequential decisions held" principle.
- **Legal & Court (§6.16, future):** a challenge to a formal Declaration's validity, or the legal disposition of an extinct house's holdings, are both concrete forward hooks into that system's eventual caseload.
- **Dynasty Chronicle (§6.11, future):** every handoff, every contested succession's resolution, every splinter house's founding, and an eventual extinction's closing account are all first-class entries in that system's own record.
- **Events (§6.8, future):** a brewing succession dispute, a Regency's day-to-day texture, and a Safety Net's last-resort candidate surfacing are all natural content for that system's random/scripted event pool.
- **Correspondence & Letters (§6.27, future):** the natural mechanism behind learning of a distant cognate relative (§7.2's Safety Net) or a rival house's spare son (§4's Adoption pool) before ever meeting them in person.
- **Traits:** a passed-over blood heir's resentment (§4), a rival claimant's Ambition/Boldness (§5.2), and a retired head's own Traits all read directly off the existing catalog rather than needing new succession-specific tags.

---

## 9. Data Model

```
HeirDesignation {
  headCharacterId,
  preferredHeirId,        // §2.1 — quiet, always-adjustable
  formalDeclaration: {     // §2.2 — null until actually declared
    heirId, declaredMonth, dignitasGranted,
  },
  disownedCharacterIds: [...],   // §2.3 — permanently removed from the eligible pool (§3) unless reconciled
}

SuccessionDispute {        // §5 — only instantiated when drama actually triggers
  disputeId,
  headCharacterId,          // the transition this dispute concerns
  claimantIds: [...],        // includes the designated heir plus any real rival claimant(s)
  triggerReasons: [ "highAmbitionRivalry" | "rebelliousHeir" | "contestedLegitimacy" |
                    "resentfulPassedOverHeir" | "prominenceScaling" ],
  resolutionPath,           // "legalChallenge" | "scheme" | "coerciveAction" | "mediatedByHead"
  outcome,                  // "cleanSuccession" | "splitInheritance" | "splinterHouseFounded" | "civilRupture"
}

Handoff {
  fromCharacterId, toCharacterId,
  trigger,                 // "death" | "retirement"
  month,
  regency: { active: bool, regentCharacterId, heirComesOfAgeMonth } | null,   // §6.2
}

ExtinctionState {          // §7 — rare; only relevant if genuinely reached
  gensId,
  accessibilityMode,        // "realistic" | "safetyNet" | "extinctionOff"
  resolutionPath,           // "legalRuling" | "politicalLandGrant" | "militaryConquest" | null (safetyNet/off avoided it)
  chronicleClosingEntryId,
}
```

---

## 10. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Declaration's exact Dignitas/Loyalty figures, the succession-drama toggle's actual weighting, and extinction's real rarity under "Realistic" mode are all unsized.
- **Regency duration and Regent selection priority.** §6.2 names the surviving spouse or a senior appointee as the natural Regent candidates without specifying the actual priority order when both are plausible, or how long a Regency typically runs before the numbers make an heir "of age."
- **Splinter house starting Standing.** §5.3 strongly implies a bitter splinter starts Rivalrous or worse with the house it split from, but doesn't commit to which, or whether it's automatic or depends on how the dispute actually resolved.
- **Whether a Regent can be deposed or replaced mid-Regency.** Not addressed — a Regent who turns out disloyal or incompetent has no described remedy short of simply waiting out the Regency.
- **Multiple simultaneous claims.** §5.2 describes a single rival claimant for simplicity; whether three or more eligible heirs can all contest at once, and how that resolves differently than a two-way dispute, isn't specified.
- **Extinction Off's actual construction guarantee.** §7.2 states this mode "guarantees, by construction" a candidate always exists, without specifying the actual mechanism that manufactures one when the simulation would otherwise have none.
- **Disownment's reconciliation mechanism.** §2.3 states disownment is reversible only through "an equally deliberate act of reconciliation" without specifying what that act actually is or what it costs to attempt.
