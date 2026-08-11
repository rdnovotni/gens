# GENS — System Design: Bandit Lords & Named Outlaw Factions
*Piracy & Banditry already built the Confederation as a real Living World Actor — a leader generated as a full Character, a standing reputation trend, House Standing, even Targeted Contracts. What it never built is the difference between an ordinary raiding nuisance and the rare, specific, historically real phenomenon of an outlaw who became genuinely famous — someone the whole province had an opinion about, whose name a magistrate said with real, personal frustration, and whose eventual downfall (when it came at all) rarely came from a clean battlefield win. This document is that missing tier, grounded directly in three real, historically documented Roman-world bandits — a social bandit sheltered by the very people he preyed among, a guerrilla leader who fought Rome to a standstill for years, and a pirate confederation so brazen it once held the young Julius Caesar himself for ransom. This pass gives each of the three real archetypes its own historically distinct downfall mechanism, resolves archetype assignment and the multi-Nemesis question, and adds a real aftermath layer — legend, copycat successors, and a captured Lord's own fate — plus a worked example.*

---

## Contents

1. Scope & Role — A Tier Above the Ordinary Confederation
2. Real Historical Archetypes
3. The Bandit Lord Dossier
4. Signature MO & Local Goodwill — The Social Bandit
5. The Nemesis Arc
6. Three Downfalls — Betrayal, Bribery, or the Weight of a Real Campaign
7. Ransom, Vengeance & the Caesar Beat
8. Capture's Aftermath — Justice, Legend & the Copycat Successor
9. Illustrative Example
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role — A Tier Above the Ordinary Confederation

Piracy & Banditry's own Confederation already does real, substantial work: it's a full Living World Actor, tiered Background or Note exactly like a rival gens, with a leader generated as a full Character the moment the player actually faces them, a standing reputation trend, and a real, two-way relationship the player can bribe, retaliate against, ally with, or contract. What that document never fully built out is the specific, rarer case of a Confederation whose leader becomes genuinely, individually famous — not merely another raid source, but a recurring, personally-known antagonist (or, in the rarest cases, a genuine folk hero) a household comes to have real, specific history with across a campaign.

This document names that rarer case directly: a **Bandit Lord of Note** is simply a Confederation that has crossed from Piracy & Banditry's own ordinary Background/Note tiering into something closer to Rival Houses' own full treatment — a real Dossier (§3), a real personal MO (§4), and, where the player's own household has genuinely tangled with them more than once, a real Nemesis Arc (§5). This document adds no new base mechanic Piracy & Banditry doesn't already have; it's an elevation and a real, historically-grounded flavor layer on top of a system that was always capable of supporting one.

**A necessary distinction, stated plainly:** the three real historical figures named in §2 are exactly that — real, historical, closed-history precedent, treated the same restrained, factual way this project's own Historical Timeline entries and Servile Wars' own Spartacus reference already are. No playthrough ever faces the real Bulla Felix, Viriathus, or the actual Cilician pirates who once held Caesar — those are real people and real events, named here to explain and ground the *archetype*, not simulated as opposable in-game actors. What a given playthrough actually faces is a fresh, procedurally-generated fictional Bandit Lord (Characters' own lazy instantiation, exactly as any other Confederation leader already generates) built in that real archetype's own image — the same treatment Servile Wars already gives Spartacus: real history as the template, a new, invented Character as the thing the player actually plays against.

---

## 2. Real Historical Archetypes

Three real, well-documented figures, each the template behind one of this document's own named archetypes:

**The Social Bandit — modeled on Bulla Felix.** A real, well-attested Italian bandit leader active in the early third century AD (per the ancient historian Cassius Dio), operating for roughly two years despite the Emperor Septimius Severus personally dispatching troops to hunt him down — genuinely evading a targeted imperial manhunt for that entire stretch. What makes him the right template for §4's own mechanic specifically: real ancient accounts describe him moving fluidly among the common population, reportedly showing real restraint toward ordinary people and craftsmen while preying chiefly on the wealthy, and being sheltered by locals precisely because of how he conducted himself — a genuine, centuries-early illustration of what later folklore would call a "social bandit," a real, historically attested phenomenon rather than a modern invention retrofitted onto antiquity. Real accounts place at least part of his activity in Italy along its own major road network — a natural, direct tie to this project's own Named Roads & Trade Itineraries and the Via Appia specifically.

**The Guerrilla Resistance Leader — modeled on Viriathus.** A real Lusitanian leader who fought Rome for nearly a decade (147–139 BC) using genuine hit-and-run tactics rather than open battle, inflicting real, repeated defeats on Roman forces before being assassinated by his own men after Rome bribed them — a real, well-documented, and genuinely dark historical footnote in its own right. His own real war closes right at the very threshold of this game's own opening year, the same "already in motion, or just concluded, when you arrive" quality already established for the Numantine War in Iberian Colony's own document — a real, closed-history precedent whose lingering local memory (a real, native Lusitanian resentment Rome only partially resolved through betrayal rather than victory) is exactly the kind of texture a fresh, in-game Iberian-region Bandit Lord of this archetype can plausibly draw on.

**The Pirate Confederation Chief — modeled on the Cilician pirates.** A real, large, and genuinely brazen network of pirate bands operating from real, well-documented strongholds along the Cilician coast (already named directly in this project's own Anatolia document), significant enough that Pompey the Great led a real, large-scale campaign to suppress them in 67 BC. The single best-known real incident, worth naming directly for the template it gives §7: a young Julius Caesar was genuinely captured and held for ransom by Cilician pirates, reportedly spending his captivity treating his captors with real, pointed contempt and promising, half in jest, that he would return and crucify them once freed — and, per the real historical account, he actually did exactly that after his ransom was paid and he raised a force of his own. A genuine, real, and remarkably game-ready template for exactly the kind of ransom-then-revenge arc §7 builds on. Pompey's own real solution is worth naming too, since it's the direct model for §6's third downfall path: not betrayal, but overwhelming, well-resourced, and well-coordinated conventional force.

A small note on real vocabulary worth using directly in flavor text: the ordinary Latin term for a bandit was **latro** (plural *latrones*), and **latrocinium** (banditry/brigandage) was a real, recognized category of offense in Roman legal thought — the actual word this document's own Legal & Court cross-reference (§10) should reach for rather than a generic modern equivalent.

---

## 3. The Bandit Lord Dossier

The direct, concrete answer to a real gap: Rival Houses §7 already built a full **Dossier** treatment for any tracked Living World Actor — Name, Head, Identity tags, Dignitas, Net Worth or Military Strength, current Standing, and recent notable Chronicle entries, with a Combo Title (Traits §7) serving as natural headline flavor. Piracy & Banditry's own Confederation never explicitly confirmed it receives the same treatment. This document confirms it directly: a Confederation that reaches Note tier gets the identical Dossier presentation a Rival House of Note already receives, substituting Force/Fleet Strength for Net Worth and a real, standing reputation trend (that document's own §2) for Dignitas — the same "reading this is the player's actual point of contact, not personally simulating them" principle Rival Houses already established, now genuinely extended rather than merely implied.

A Bandit Lord's own Combo Title-style headline draws first on §2's own archetype where one applies — "A Social Bandit, sheltered by those he doesn't rob" reads as immediately more informative than a generic Traits pairing would on its own.

**Resolved this pass — archetype assignment.** Which of §2's three archetypes a freshly-generated Bandit Lord actually rolls is weighted first by regional and cultural context — an Italian-region Confederation defaults toward Social Bandit odds, an Iberian one toward Guerrilla Resistance, a Cilician/Anatolian coastal one toward Pirate Confederation Chief — and then genuinely shiftable by the underlying leader Character's own rolled Traits: a high-Compassion leader generated outside Italy can still plausibly land Social Bandit, and a Callous one inside it can just as plausibly not. Region sets the odds; the individual Character's own Traits still get the final, honest say.

---

## 4. Signature MO & Local Goodwill — The Social Bandit

The concrete mechanical payoff of §2's own Bulla Felix template, and a genuinely rich piece of "no dominant strategy" texture worth building directly: a Bandit Lord generated against the Social Bandit archetype specifically preys selectively — targeting wealthy shipments and travelers while genuinely sparing common people and craftsmen — and, per real historical account, this isn't merely flavor. A sustained Social Bandit MO generates real, accumulating **Local Goodwill** among a settlement's own lower-tier pop groups (Settlement Demographics' own Operarii, rural Coloni), a genuine Contentment benefit distinct from and additive to whatever else is affecting that reading.

**This is what makes suppression genuinely complicated rather than a simple military calculation.** A player pursuing Retaliation (Piracy & Banditry §5) against a Bandit Lord who's built real Local Goodwill faces a real, felt tension beyond the ordinary military math: local informants are less forthcoming, Espionage-driven infiltration (§6) reads against a real, elevated difficulty, and a sufficiently public, heavy-handed suppression campaign against a genuinely popular outlaw carries its own real Contentment and Dignitas cost among a Popularist-leaning audience specifically (Politics & Patronage §3.1) — the direct mechanical reason Bulla Felix's own real capture ultimately came through personal betrayal (§6) rather than a triumphant military campaign the way an ordinary Confederation's Retaliation outcome usually resolves.

---

## 5. The Nemesis Arc

A Bandit Lord who survives more than one real engagement against the same household builds real, accumulating **Fame** (Games & Spectacle §2's own universal mechanic, reused directly rather than invented anew) at a genuine, felt rate — every failed Retaliation attempt, every successfully bribed-off raid, every escaped ambush adds to a reputation that starts to precede them specifically, distinct from the Confederation's own general standing trend (Piracy & Banditry §2). A sufficiently Famous Bandit Lord who has specifically outlasted the same household's own repeated attempts to end them develops a genuine, standing personal House Standing entry (Rival Houses §5.2's own existing field, reused wholesale) skewed hard toward hostile specifically with that household — a real, recurring nemesis rather than an anonymous, interchangeable raid source, exactly the direction's own request.

Each failed attempt against a Nemesis-tier Bandit Lord raises their own Fame further still — a real, self-reinforcing cycle this document treats honestly rather than smoothing away: the harder a household tries and fails to end a particular outlaw, the more legendary that outlaw becomes, and the more satisfying (and, per §6, the more likely to require something other than brute force) their eventual downfall actually is.

**Resolved this pass — multiple simultaneous Nemeses.** No hard cap is imposed, but none is really needed: Fame and House Standing hostility naturally concentrate on whichever single Confederation a household has actually engaged most, since both accrue specifically from repeated, real contact rather than from a flat passive tick. A household could, in principle, accumulate two genuine Nemeses at once by picking fights with two different Confederations in parallel — but doing so is a real, self-inflicted choice rather than something the system pushes a player toward, consistent with how this project prefers a natural, emergent rarity over an artificial wall.

---

## 6. Three Downfalls — Betrayal, Bribery, or the Weight of a Real Campaign

New this pass: rather than a single generic "infiltrate or fight" choice, each of §2's three real archetypes has its own historically distinct, mechanically differentiated path to actually ending a Nemesis-tier Bandit Lord — the direct payoff of grounding this document in three genuinely different real precedents rather than one.

- **The Social Bandit falls through Infiltration.** Directly modeled on Bulla Felix's own real capture: Espionage's existing infiltration and Discovery/Traceability model (already reused wholesale by Piracy & Banditry's own §7.1 Targeted Contracts) is the mechanism, with §4's own Local Goodwill directly raising its difficulty — real, earned community loyalty is exactly what has to be overcome.
- **The Guerrilla Resistance Leader falls through Bribing the Lieutenants.** Directly modeled on Viriathus's own real, considerably darker end: rather than infiltrating from outside, this path targets the Bandit Lord's own trusted subordinates directly — a real Bribe Interaction (Characters §9.4) aimed at whichever lieutenant carries the weakest Honor and Loyalty reading, offering them a real, tempting personal reward to turn on their own leader. Successful, this ends the Bandit Lord immediately and without a battle — but, per the real, honest darkness of the historical precedent it's modeled on, it's a genuinely uglier, more morally compromising method than open combat, and this document doesn't pretend otherwise: a household that wins this way carries a small, real, and legitimate Honor-adjacent cost of its own for how the victory was actually achieved, independent of the Dignitas gained from the outlaw's own death.
- **The Pirate Confederation Chief falls through the weight of a real campaign.** Directly modeled on Pompey's own real, historically effective solution: unlike the other two archetypes, straightforward military Retaliation (Piracy & Banditry §5), pursued with genuinely substantial, well-coordinated Fleet and Force investment rather than a token effort, is this archetype's own correct, historically apt answer — no betrayal required, simply the real, overwhelming application of a resourced campaign against a threat that, per real history, was never actually beaten by cleverness alone.

A Nemesis-tier Bandit Lord of any archetype can still, in principle, be approached through either of the other two methods — this document doesn't hard-lock a single solution to a single archetype — but each one's own *native* path, modeled directly on its own real precedent, resolves at meaningfully better odds than reaching for a method that doesn't fit their own historical grain.

---

## 7. Ransom, Vengeance & the Caesar Beat

The direct, concrete template from §2's own Cilician pirate example: a captured Familia member or Companion held for Ransom (Characters §9.5, Piracy & Banditry §8's own Kidnapping mechanic) by a specific, named Bandit Lord isn't necessarily a closed story once the ransom is paid and the captive returns home. Per the real, well-attested Caesar precedent — a captive who endured real contempt and humiliation during captivity, then genuinely returned to destroy his own captors once free — this document names a real, optional follow-on beat:

A ransomed Character with sufficiently high Boldness and the Vengeful Reactive trait (Traits §6.1) can take up **Settle the Score** (Character Ambitions §3.4's own existing Vice & Vengeance category) with that specific Bandit Lord as the named target — no new Ambition invented, simply a natural, well-motivated instance of one this project already built, given a real, concrete origin story. Completing it — hunting down and personally ending the very outlaw who once held them captive — is exactly the kind of full-circle, deeply satisfying arc the real Caesar story already provides for free, and a genuine, guaranteed Dynasty Chronicle highlight regardless of whether the household's own future Caesar goes on to any further greatness afterward.

---

## 8. Capture's Aftermath — Justice, Legend & the Copycat Successor

New this pass, closing the gap of what actually happens once a Bandit Lord of Note's own story ends.

**A captured (rather than killed) Bandit Lord** faces Legal & Court's own existing sentencing options for *latrocinium* (§2) exactly as any other convicted criminal would — and, where the household prefers a more visible, deliberate act of justice over a quiet execution, Games & Spectacle's own *damnatio ad ludum* or *damnatio ad bestias* sentences (§4 of that document) are a real, historically apt public alternative: forcing a once-untouchable outlaw into the arena is a genuine, satisfying, and historically consistent way to convert years of frustrated pursuit into a single, very public Dignitas payoff.

**Legend outlives the person.** A sufficiently Famous Bandit Lord (§5) who falls — by any of §6's three methods, or through capture and a public arena sentence — doesn't simply vanish from local memory the moment they're gone. Per real precedent (both Bulla Felix and Viriathus entered lasting local memory well beyond their own actual lifetimes), this document names a small, real, lasting regional flavor effect: local stories and songs about the fallen Bandit Lord persist as a genuine, minor Culture-flavor texture in the region for years afterward, and, per the honest logic of how banditry actually recurs, a real chance that a fresh, unrelated Confederation eventually rises in the same region drawing deliberately on the fallen Lord's own legend — a **Copycat Successor**, invoking the old name or reputation without literally being the same organization, giving a truly Legendary-tier downfall a genuine, satisfying, but not permanently closed-off aftermath rather than a clean, forgotten ending.

**A defeated Confederation's own lieutenant inheriting leadership** is the more immediate, common version of the same idea — Rival Houses' own succession logic, applied to a Confederation rather than a gens: the organization itself can survive its own founder's fall under new, lesser-known leadership, available to rebuild toward its own future Note-tier status rather than being guaranteed to disappear the moment its most famous name is gone.

---

## 9. Illustrative Example

*(Texture only — no numbers implied.)*

> **Casca**, a Social Bandit-archetype outlaw operating along a stretch of the Via Appia (Named Roads & Trade Itineraries), begins as an ordinary Background-tier Confederation the player's own household barely notices.
>
> After two separate raids specifically sparing the household's own tenant farmers while stripping a luxury shipment bare, Casca's own Local Goodwill (§4) among the region's Operarii and Coloni climbs high enough that the player's first Retaliation attempt fails outright — a local informant who might otherwise have talked simply doesn't. A second attempt, launched too hastily, also fails. Casca's own Fame (§5) is now real; the household's own House Standing with him has turned openly hostile in both directions — a genuine Nemesis.
>
> Rather than a third doomed military attempt, the player instead commissions an Infiltration (§6) — a trusted Companion, posing as a fugitive debtor, spends real months earning a place in Casca's own confederation before finally locating the hideout. Casca is captured alive rather than killed in the raid that follows.
>
> Rather than a quiet execution, the player elects a public *damnatio ad ludum* sentence (§8) — Casca fights and dies in the arena, a deliberate, very visible end to a story the whole settlement has been following for two years. The household's own Dignitas gain is real and substantial. A year later, a new, smaller band calling itself "Casca's Own" begins operating in roughly the same stretch of road — a Copycat Successor (§8), and the household's own next real problem.

---

## 10. Cross-System Integration

- **Piracy & Banditry:** this document's entire foundation — the Confederation, its LivingWorldActor schema, standingTrend, Retaliation, and Targeted Contracts are all reused wholesale; §3–§8 are a direct elevation and flavor layer, not a redesign.
- **Rival Houses:** §3's Dossier is a direct, confirmed extension of that document's own §7 Legibility mechanic to a Confederation of Note; §5's House Standing field is reused verbatim; §8's lieutenant succession directly mirrors that document's own gens-succession logic.
- **Games & Spectacle:** §5's Fame mechanic is that document's own universal system, reused directly; §8's *damnatio ad ludum*/*ad bestias* sentencing gives a captured Bandit Lord a real, historically apt public fate.
- **Settlement Demographics, Politics & Patronage:** §4's Local Goodwill is a new, real input into Contentment specifically for a Social Bandit-archetype Confederation; the Popularist-audience Dignitas cost of heavy-handed suppression reads directly against that document's own Faction axis.
- **Espionage:** §6's Social Bandit downfall is the direct, primary intended use of that document's own infiltration and Discovery/Traceability model against a Bandit Lord specifically.
- **Characters:** §6's Bribing the Lieutenants reuses the existing Bribe Interaction directly, reading a lieutenant's own Honor and Loyalty exactly as that Interaction already does elsewhere.
- **Character Ambitions, Traits:** §7's Settle the Score reuse is a natural, well-motivated instance of an existing Ambition category; Vengeful and Boldness are the direct, existing mechanical drivers.
- **Legal & Court:** *latrocinium* (§2) is this document's own real, correct legal-vocabulary tie for a captured Bandit Lord's own formal sentencing.
- **Named Roads & Trade Itineraries:** the Social Bandit archetype's own real historical tie to Italy's major road network is a direct, natural flavor connection to that document's own Via Appia entry, and this document's own §9 example.
- **Cultures of the Known World:** §8's persisting local legend is a small, real, lasting regional Culture-flavor addition.
- **Starting Regions (Italian Heartland, Iberian Colony, Anatolia):** each of §2's three real archetypes has a natural, named regional home already established in this project's own roster.
- **Dynasty Chronicle:** a Nemesis-tier Bandit Lord's eventual downfall, a completed Settle the Score against one specifically, and a Copycat Successor's own re-emergence are all guaranteed or near-guaranteed, top-tier entries.

---

## 11. Data Model

```
BanditLordOfNote {                        // extends Piracy & Banditry's own Confederation/LivingWorldActor record
  confederationActorId,
  archetype,                                 // "socialBandit" | "guerrillaResistance" | "pirateConfederationChief" | "generic"
  fame,                                       // reuses Games & Spectacle's own universal Fame field directly
  localGoodwill,                               // §4, only meaningful for the socialBandit archetype
  nemesisHouseholdId,                           // nullable — set once a specific household's own House Standing turns hostile
  failedRetaliationCount,                        // §5 — feeds both Fame accrual and downfall difficulty
  dossierHeadline,                              // §3 — Combo-Title-style flavor string
  legendaryAfterDowned: bool,                     // §8 — set on a sufficiently Famous downfall
}

InfiltrationAttempt {                       // §6 — Social Bandit path, a specific instance of Espionage's own mechanic
  attemptId,
  targetBanditLordActorId,
  infiltratingCharacterId,
  localGoodwillDifficultyModifier,
  outcome,                                     // "success" | "discovered" | "failed"
}

LieutenantBribeAttempt {                     // §6 — Guerrilla Resistance path
  attemptId,
  targetBanditLordActorId,
  targetLieutenantCharacterId,
  bribingHouseholdId,
  outcome,                                      // "success" | "refused" | "reportedToLeader"
  honorCostIncurred: bool,
}

RansomVengeanceLink {                       // §7
  ransomedCharacterId,
  capturingBanditLordActorId,
  settleTheScoreAmbitionId,                      // nullable — links to Character Ambitions' own record
}

CaptureOutcomeRecord {                       // §8
  banditLordActorId,
  fate,                                          // "executedQuietly" | "damnatioAdLudum" | "damnatioAdBestias" | "killedInRaid"
  publicDignitasGain,
}

CopycatSuccessorRecord {                      // §8
  originalBanditLordActorId,
  newConfederationActorId,
  invokesOriginalName: bool,
  settlementId,
}
```

---

## 12. Open Questions

- **All numeric sizing**, per this project's standing convention — Fame accrual per failed Retaliation, Local Goodwill's own Contentment magnitude, each of §6's three downfall paths' own success-odds curve, and the Honor cost of Bribing the Lieutenants are all unsized.
- **Copycat Successor's own trigger probability and delay.** §8 names the mechanic without specifying how likely or how soon one actually emerges after a Legendary-tier downfall.
- **Whether a Guerrilla Resistance-archetype Bandit Lord can ever have genuinely loyal, unbribable lieutenants** — §6 assumes at least one exploitable weak link exists, without addressing the rarer case of a Confederation with uniformly high-Honor, high-Loyalty leadership beneath its own chief.
- **Whether the small Honor cost from a successful Bribing the Lieutenants (§6) should ever be visible to Rival Houses or the wider political world**, or remains a purely private mark on the household's own record.
