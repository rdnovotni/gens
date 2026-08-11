# GENS — System Design: Servile Wars, Slave Revolts & Collective Resistance
*The escalation tier sitting above Labor & Slavery's own individual-scale Unrest, Punishment, and Flight & Recapture mechanics — this document is what happens when household-level discontent stops being one person's private calculation and becomes a genuine, organized, collective act. Grounded directly in real Roman history: the First and Second Servile Wars in Sicily (already named in that region's own document) and the Third — Spartacus's own revolt, which marched through Italy itself and has, until now, gone unnamed anywhere in this project despite touching the single most central starting region on the whole roster.*

---

## Contents

1. Scope & Role — Individual Unrest Versus Collective Revolt
2. Real Historical Grounding — The Three Servile Wars
3. Regional Unrest — The Aggregate Reading
4. Sparks — What Actually Ignites a Revolt
5. The Revolt Leader — A Named Character
6. Three Scales of Uprising
7. Resolving a Revolt — The Combat Resolution Engine
8. Player Choices During an Active Revolt
9. Aftermath — Suppression, Failure & the Long Shadow
10. Prevention & Mitigation
11. Inciting a Rival's Revolt
12. Cross-System Integration
13. Data Model
14. Open Questions

---

## 1. Scope & Role — Individual Unrest Versus Collective Revolt

Labor & Slavery already tracks Unrest as a real, standing household-level pressure reading off Regimen choices, Punishment history, and individual Traits — and it already resolves at the individual scale: a single person's Flight risk, a single Punishment's own consequences. That system was never meant to model what happens when discontent stops being many separate individual calculations and becomes one shared, coordinated act — dozens or hundreds of people acting together, seizing weapons, killing overseers, and in the most severe real cases, fielding something close to an actual army. This document is that missing collective tier, sitting directly on top of Labor & Slavery's own existing Unrest math rather than replacing any part of it.

**What doesn't move here:** Regimen, the Punishment ladder, and ordinary individual Flight & Recapture all stay exactly as Labor & Slavery already built them. This document reads their aggregate output as one of its own key inputs (§3) and supplies the layer above them — what happens when that aggregate crosses a real, dangerous threshold.

---

## 2. Real Historical Grounding — The Three Servile Wars

This project already treats two of the three real, historically named large-scale slave revolts of the Roman Republic with real care — Sicily's own document names both the First Servile War (135–132 BC, led by Eunus, closing right at this game's own opening threshold) and the Second Servile War (104–100 BC, led by Salvius), explicitly citing this project's "standing commitment to frank, respectful treatment of slavery." The third, and by far the most famous, has gone unnamed anywhere in this project until now, despite marching directly through the single most central starting region on the entire roster.

**The Third Servile War (73–71 BC) — Spartacus.** A real, historically monumental slave uprising beginning with an escape from a gladiatorial *ludus* at Capua — Games & Spectacle's own gladiator-sourcing mechanic (§3.1 of that document) is, mechanically, the exact real pipeline this revolt actually began in — and growing into a real force that defeated multiple Roman armies before its eventual, brutal suppression. This document names it directly as a live, in-range Historical Timeline entry (Events' own Historical Timeline, §6.4) available to any playthrough whose own start date and Italian Heartland presence make it relevant, closing a real gap the Italian Heartland region document itself never filled. Consistent with this project's own established restraint, its real, historically documented ending — mass crucifixion of surviving rebels along the Appian Road — is named factually and soberly in §9, once, without invented dramatization.

These three real wars are this document's own north star: not every revolt in a given playthrough needs to reach this scale (§6 defines two smaller, more common tiers below it), but the mechanics this document builds are specifically sized to make a Third-Servile-War-scale event a genuine, rare, campaign-defining possibility rather than an impossible background fact the game merely references in flavor text.

---

## 3. Regional Unrest — The Aggregate Reading

A single household's own Unrest score (Labor & Slavery §3) is the wrong scale to ask "could a Servile War actually happen here." Real history is specific on this point: both Sicilian wars and Spartacus's own revolt grew explosively precisely because enslaved populations were concentrated in dense agricultural and gladiatorial-training regions at real, historically enormous scale relative to the free citizen population watching over them. This document adds a second, aggregate reading — **Regional Unrest** — computed from:

- The **average Unrest** across every household's own enslaved population within a settlement or region, reading Labor & Slavery's existing per-household values directly rather than tracking a separate parallel number.
- **Concentration** — Settlement Demographics' own enslaved-population-to-free-citizen ratio for that settlement; a region with a genuinely enormous enslaved agricultural workforce relative to a thin citizen and garrison presence (the real Sicilian latifundia condition) reads as structurally more dangerous even at moderate average Unrest, exactly the real historical lesson both Sicilian wars taught and went unheeded after the first.
- **Garrison presence** — an active Estate Force or Garrison-tier military presence (Military & Combat, Buildings §4.11) suppresses Regional Unrest directly, the concrete mechanical reason a real Roman military drawdown elsewhere was historically often the actual spark that let existing regional tension finally erupt.

Regional Unrest is a slow-moving, background value — not something the player manages tick-by-tick, but a real, honest reading of whether the region they've built is quietly becoming exactly the kind of place history shows this actually happens.

---

## 4. Sparks — What Actually Ignites a Revolt

High Regional Unrest is necessary but never sufficient on its own — real revolts, including all three historical ones, were touched off by a specific, identifiable spark rather than simply crossing a numeric threshold in the abstract. A **Spark** is a real, discrete trigger condition, and Regional Unrest is best read as the amount of dry tinder actually present when one lands:

- **A particularly severe or public Punishment** (Labor & Slavery §6's own Severe or Lethal tier), witnessed by enough people to spread rather than remain contained to one household.
- **A charismatic individual reaching a breaking point** — a specific enslaved Character with high Boldness and Martial, a thwarted Freedom-category Ambition (Character Ambitions §3.6), or a fresh, severe Punishment of their own, becomes the actual spark rather than the aggregate number alone. Spartacus's own real origin as a specific, named gladiator rather than an anonymous mass is the direct historical model here.
- **A military drawdown** — a garrison or Estate Force temporarily reduced (deployed elsewhere, per Military & Combat's own Muster mechanic) lowers the real, felt cost of attempting a revolt precisely when it's most vulnerable, the same real historical pattern behind more than one actual uprising.
- **A famine, disaster, or genuine Hungry Gap** (Seasons §7) stretching an already-Harsh Regimen past what it was ever going to bear.
- **A Rival House's own deliberate incitement** (§11) — the concrete payoff for the Interaction Catalog's own long-standing, previously undesigned "Incite Rebellion/Unrest" entry (Characters §9.4).

A Spark landing against low Regional Unrest fizzles into an ordinary, contained Labor & Slavery Punishment-and-Flight event; the same Spark landing against high Regional Unrest is what actually escalates into §6's own larger tiers.

---

## 5. The Revolt Leader — A Named Character

Per real history's own clearest lesson (Eunus, Salvius, and Spartacus were all specific, named individuals, not anonymous crowds), any Revolt at Regional or Servile War scale (§6) generates a real Leader through Characters' own lazy instantiation — reusing the exact framework Games & Spectacle already established for a gladiator or venator, since a former gladiator is, per §2, the single most historically apt real origin for one. A Leader carries real Martial, Boldness, and often a Freedom-category Ambition (Character Ambitions §3.6) either completed through the revolt itself or the entire reason it was undertaken in the first place.

**A Leader builds real Fame** (Games & Spectacle §2's own universal mechanic) at a rate matching or exceeding a gladiator's own — Spartacus's own real, lasting historical fame is the obvious model — and a sufficiently Famous Leader who survives an engagement becomes a real, recurring, and increasingly dangerous figure the longer they remain at large, exactly the same "loose thread" logic Labor & Slavery's own Flight & Recapture (§7 of that document) already establishes for an ordinary escapee, scaled up considerably.

---

## 6. Three Scales of Uprising

Mirroring Natural Disasters' own severity-tier shape, but scoped specifically to collective resistance rather than a single unified scale — each tier is a genuinely different kind of event, not merely a bigger number.

| Scale | Real Model | What It Actually Is |
|---|---|---|
| **Local Uprising** | An ordinary, contained event | A single estate or building's own enslaved population rises against its immediate overseers — resolves largely through Labor & Slavery's own existing Punishment/Flight framework at an elevated, multi-person scale, rarely requiring a full Military & Combat engagement |
| **Regional Revolt** | The early stages of either Sicilian war | Multiple estates within a settlement or region, coordinated or rapidly spreading, genuinely requiring an actual Military & Combat response — a real Force deployment against an Irregular-type Revolt Force (§7), not merely a Punishment decision |
| **Servile War** | The full scale of all three real historical wars | Province- or region-spanning, multi-season, potentially requiring Rome's own direct intervention independent of the player's own household resources — genuinely rare, campaign-defining, and, per §9, capable of ending badly for the player's own household specifically rather than merely being an inconvenience to manage |

A Servile War is deliberately built to the same distant-and-rare shape as Politics & Patronage's own cursus honorum and Military & Combat's own Roman Service track — most playthroughs should see a Local Uprising or two, a genuinely attentive or unlucky one might see a Regional Revolt, and a Servile War is the kind of event a Dynasty Chronicle remembers a household by for generations, exactly the way the real Sicilian and Italian populations actually did.

---

## 7. Resolving a Revolt — The Combat Resolution Engine

Consistent with this project's own reuse-over-reinvention principle, and directly extending Military & Combat's own explicit note that its Irregular Combatant type covers "pirates, bandits, and gladiators for the systems that will eventually plug in here" — a Revolt Force at Regional or Servile War scale is simply another Irregular Combatant profile, resolved through that document's existing Combat Resolution Engine (§4 of that document) exactly like a pirate raid or a gladiatorial bout, scaled to Squad size for a Regional Revolt and to full Force scale for a Servile War. The Revolt Leader (§5) fills the Commander Inputs slot (Military & Combat §4.2) precisely the way any other enemy commander does, generated on demand the moment the player actually faces them.

**A genuinely improvised, lower-Equipment-Tier force.** Consistent with real history — rebel forces were armed with agricultural tools, captured weapons, and whatever an estate's own Armory happened to hold, not standing military equipment — a Revolt Force resolves at a real, structural Equipment Tier disadvantage relative to an equivalent citizen Squad, offset partly by numbers (Regional Unrest's own concentration reading, §3) and partly by the Leader's own Martial/Boldness where one has emerged. This is what makes a Local Uprising genuinely containable while a full Servile War, fielding real numbers over real time, can still credibly defeat an under-prepared household's own Estate Force — exactly as real history shows happened more than once before Rome's full military weight was finally brought to bear.

---

## 8. Player Choices During an Active Revolt

Consistent with this project's own no-dominant-strategy pillar, a live Revolt (Regional scale or above) offers genuine, differently-costed options rather than a single obviously-correct response:

- **Military suppression** — the historically default Roman response, resolved through §7's Combat Resolution Engine. Success ends the Revolt on the player's own terms, feeding §9's aftermath; failure is a genuine, real setback, not a soft loss state.
- **Negotiation** — genuinely rare and historically atypical (Rome did not, as a rule, negotiate with a servile uprising), but not impossible: a real, costly Legal & Court or Diplomacy-style parley can end a Revolt early, typically at the real cost of a manumission concession, a Regimen commitment, or a direct Dignitas hit for having negotiated with rebels at all — a legitimate, if socially costly, off-ramp for a household genuinely unable to win militarily.
- **Flight or abandonment** — a real, legitimate emergency response for an under-resourced household: abandoning an overrun estate (Estate & Settlement's own Plot condition mechanics) preserves the family's own lives at the cost of the property and everything on it, a genuine "lose the battle, keep the war" choice distinct from either of the above.
- **Covert exploitation** — per §11, a player can also choose to do nothing at all about a Revolt actively striking a *rival's* holdings, or actively fan it further.

---

## 9. Aftermath — Suppression, Failure & the Long Shadow

**A successfully suppressed Revolt** carries real, mixed consequences rather than a clean win: relief and a real, if modest, Dignitas recovery for restoring order, set against a genuine, permanent labor-pool loss (killed or executed rebels are gone, not merely punished) and, per real Roman practice and this project's own established restraint around hard material, a sober, factually-stated possibility of mass execution or crucifixion for captured survivors — named plainly, once, exactly the treatment §2 already establishes for Spartacus's own real historical ending, never dwelt on or dramatized. A captured Leader specifically faces Legal & Court's own existing capital sentencing options (§9 of that document), including the *damnatio ad bestias* Games & Spectacle already built — a real, grim, and historically apt closing of that particular loop.

**A Revolt that goes badly for the player** is a genuine, real possibility at Servile War scale specifically, consistent with this project's own stated willingness to let hard systems carry real stakes: a Catastrophic-tier outcome can see an estate genuinely overrun, Familia members captured or killed (reading through that document's own mortality/capture mechanics), or an outright forced abandonment of the settlement — rare, but real, exactly the same register Natural Disasters already established for its own Catastrophic tier.

**The long shadow.** Consistent with Policies & Edicts' own Domus Dura Doctrine already establishing that a harsh path's costs can compound rather than plateau, a region that has actually suffered a Regional Revolt or Servile War carries a real, elevated Regional Unrest baseline going forward that doesn't fully recede — the same honest, permanent-scar logic already built into this project's harshest existing systems, and a genuine, felt argument for a player who's seen one revolt to actually change course rather than simply rebuild and repeat the same Regimen choices that produced it.

---

## 10. Prevention & Mitigation

The real, player-facing toolkit, entirely composed of levers this project already built rather than a new bespoke prevention system:

- **Regimen tuning** (Labor & Slavery §3) is the single most direct lever — this document adds no new mechanic here, only a much higher-stakes reason to actually use the one that already exists.
- **A visible garrison or Estate Force presence** suppresses Regional Unrest directly (§3) — the concrete, generalized mechanical form of Companions & Court Positions' own already-flagged note that a mine's Overseer "reads some Martial for safety/revolt-risk," now extended from a single building to the settlement level.
- **Manumission as pressure release** (Labor & Slavery §8) — freeing a specific high-Ambition or severely Resentful individual is a real, legitimate way to defuse a specific Spark (§4) before it lands, at the honest cost of losing that individual's own labor value.
- **Religion's own Omen system** (reusing Natural Disasters' own precedent directly) — a household in Divine Displeasure can have its passive Omens skew toward foreshadowing unrest specifically where Regional Unrest is already elevated, giving an attentive, pious player a real, early warning signal.
- **Espionage** (§6.15, future) — a planted agent among a Rival House's or even one's own household's enslaved population is a natural, direct early-warning mechanism for a Spark building before it actually lands.

---

## 11. Inciting a Rival's Revolt

The direct, concrete design behind Characters' own long-standing, previously undesigned Interaction Catalog entry, "Incite Rebellion/Unrest" (§9.4 of that document, explicitly noted there as resolving through "Labor & Slavery's Unrest math"). A sufficiently skilled Intrigue Character can deliberately raise a targeted rival's own Regional Unrest, or directly manufacture a Spark (§4) against them — a genuine, real Scheme-tier act of sabotage (Characters §10), carrying the same real discovery risk any other Scheme does, and a severe Scandal/Legal exposure if it's ever traced back to the player's own household, consistent with how seriously real Roman society would have treated deliberately fomenting a servile uprising against a fellow citizen. A successful, undiscovered incitement is a genuinely devastating rival's-own-problem to have created — and, per §8's own note, the player can then simply choose to watch it unfold rather than intervene, a cold, legitimate, and very Roman use of someone else's crisis.

---

## 12. Cross-System Integration

- **Labor & Slavery:** Regimen, the Punishment ladder, and individual Flight & Recapture are this document's entire foundation, reused wholesale; §3's Regional Unrest is a direct aggregate extension of that document's own per-household Unrest value.
- **Settlement Demographics:** the enslaved-to-citizen concentration ratio (§3) is a direct read of that document's own tracked pop groups.
- **Military & Combat:** §7 resolves entirely through that document's own Combat Resolution Engine and Irregular Combatant type — this document is the concrete "systems that will eventually plug in here" reference that type was explicitly built to anticipate; a Muster-driven garrison drawdown (§4 of that doc) is a direct Spark source (§4).
- **Games & Spectacle:** a Revolt Leader is generated exactly like a gladiator (§3.1 of that document); §2's own Fame mechanic applies identically; *damnatio ad bestias* (§4.2 of that document) is a real, available fate for a captured Leader; the Ludus/escaped-gladiator pipeline is this document's own direct real-historical origin story for Spartacus specifically.
- **Character Ambitions:** a Freedom-category Ambition (§3.6 of that document), especially a thwarted one, is a primary Spark source and a primary motivator for an emergent Leader.
- **Politics & Patronage, Legal & Court:** a captured Leader faces that document's own capital sentencing table; negotiation (§8) carries a real Dignitas cost read through that system.
- **Seasons:** a Hungry Gap (§7 of that document) is a direct, named Spark source.
- **Religion:** the Omen-foreshadowing mechanism is a direct, named extension of Natural Disasters' own established precedent.
- **Policies & Edicts:** Domus Dura's own already-established permanent Unrest compounding is the direct model behind §9's Long Shadow effect.
- **Estate & Settlement:** an overrun estate's abandonment (§8) reads through that document's own Plot condition and Demolition/rebuild mechanics.
- **Characters:** §11 is the concrete, long-deferred design behind the Interaction Catalog's own "Incite Rebellion/Unrest" entry.
- **Companions & Court Positions:** §10's garrison-suppression mechanic is a direct, generalized extension of the Metallarius's own already-flagged Martial/revolt-risk note.
- **Events, Historical Timeline:** the Third Servile War (§2) is a real, newly-named Divergence-eligible entry this document supplies directly, closing a gap the Italian Heartland region document itself left open.
- **Dynasty Chronicle:** a Servile War of any real scale, a captured or famous Leader, and a household's own survival or destruction in one are all guaranteed top-tier Chronicle material.
- **Rival Houses:** §11 is a direct, concrete new tool in that document's own conflict repertoire; a rival's own suffered Revolt is a real, visible Living World event.

---

## 13. Data Model

```
RegionalUnrest {                      // §3
  settlementId,
  aggregateHouseholdUnrest,             // derived from Labor & Slavery's own per-household values
  concentrationFactor,                   // enslaved:citizen ratio, read from Settlement Demographics
  garrisonSuppressionModifier,             // active Estate Force/Garrison presence
  currentReading,                        // derived composite
}

RevoltSpark {                          // §4
  sparkType,                              // "severePunishmentWitnessed" | "leaderBreakingPoint" |
                                        // "garrisonDrawdown" | "hungryGap" | "rivalIncitement"
  settlementId,
  triggeredAtMonth,
  regionalUnrestAtTrigger,
  resultingScale,                          // "fizzled" | "localUprising" | "regionalRevolt" | "servileWar"
}

RevoltRecord {                          // §6, §7
  revoltId,
  scale,                                   // "localUprising" | "regionalRevolt" | "servileWar"
  settlementIds: [ ... ],                    // more than one only at servileWar scale
  leaderCharacterId,                         // nullable — Local Uprisings rarely produce a named Leader
  forceProfileId,                            // pointer to Military & Combat's own Irregular Combatant record
  status,                                   // "active" | "suppressed" | "negotiatedEnd" | "playerAbandoned" | "playerOverrun"
  startedAtMonth, resolvedAtMonth,
}

RevoltAftermath {                       // §9
  revoltId,
  labborPoolLoss,
  capturedSurvivorCount,
  leaderCapturedOrKilled: bool,
  longShadowUnrestBaselineIncrease,
  chronicleEligible: true,
}

IncitementScheme {                      // §11 — a specific instance of Characters' own Scheme engine
  schemeId,                                // links to Characters' own Scheme record
  initiatorHouseholdId, targetHouseholdId,
  method,                                   // "raiseRegionalUnrest" | "manufactureSpark"
  discovered: bool,
}
```

---

## 14. Open Questions

- **All numeric sizing**, per this project's standing convention — Regional Unrest's own composite formula, Spark probability weighting, and a Revolt Force's actual Equipment Tier disadvantage are all unsized.
- **Servile War's exact multi-settlement mechanics.** §6 establishes that the largest scale can span more than one settlement without specifying how resolution actually coordinates across settlement boundaries the way a single-settlement Regional Revolt cleanly does.
- **Whether Rome itself can intervene independent of the player.** §6 flags a Servile War as "potentially requiring Rome's own direct intervention independent of the player's own household resources" without designing what that intervention actually looks like mechanically — a real, open question about whether the player is ever purely a bystander to a large enough conflagration.
- **Negotiation's exact concession menu.** §8 names manumission, a Regimen commitment, or a Dignitas hit as plausible negotiated outcomes without specifying the actual choice structure.
- **The Long Shadow's decay curve, if any.** §9 states the elevated post-Revolt Unrest baseline "doesn't fully recede" without specifying whether it decays slowly over a very long time or is genuinely permanent for the remainder of the playthrough.
