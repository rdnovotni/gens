# GENS — System Design: Piracy & Banditry (§6.24)
*The system with the most infrastructure sitting ready for it in this entire project — Military & Combat's Combat Resolution Engine, Fleet concept, and Irregular Combatant type were all built explicitly to serve this; Rival Houses' Living World Actor framework already has a "banditConfederation" actor type in its own enum with nothing behind it; Espionage, Travel, Labor & Slavery, and Resources & Goods have all been pointing here for a while. This pass adds Targeted Contracts — paying a Confederation to kidnap, kill, or enslave a specific named individual, up to and including a rival house's own Head — reuses Espionage's Discovery/Traceability model directly for the real stakes involved, and closes two balance gaps: a hired intermediary can now simply steal the proceeds, and an ignored Confederation grows bolder over time rather than staying a permanently safe-to-ignore threat.*

---

## Contents

1. Scope & Role
2. Bandit & Pirate Confederations — Anatomy
3. Being Raided — Threat, Interception, Consequence
4. Bribery & Tribute
5. Retaliation
6. Turning Raider — Playing Both Ways
7. Allying With & Contracting Raiders
   7.1 Targeted Contracts — Kidnap, Kill, Enslave
8. Kidnapping — Goods, Livestock, and People
9. Estate Security — The Defensive Investment
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "a human threat layer distinct from Natural Disasters' impersonal hazards: raids on trade goods, caravans, and travelers that scale with the player's security investment and can be interceptable, bribed off, or retaliated against, rather than simply weathered."

This document resolves an unusually large, already-built backlog: Military & Combat's Combat Resolution Engine, Fleet, and Irregular Combatant type were designed explicitly with this system in mind; Rival Houses generalized its Living World Actor framework specifically so a bandit confederation would have a home; Espionage extended its target model the same way; Travel named Piracy & Banditry directly as a real-stakes en-route event source; Companions & Court Positions already named the Vigil, Praefectus Vigilum, Navarchus, and Navarchus Princeps as "the named roles behind this system's estate-side countermeasures"; Labor & Slavery and Resources & Goods both already treat a Piracy/Banditry outcome as a real acquisition and disruption source. Per direction, this document goes further than the core doc's own framing by making the relationship genuinely two-way: the player can be raided, bribe, retaliate — and also raid, ally, or contract raiders themselves.

---

## 2. Bandit & Pirate Confederations — Anatomy

Reusing Rival Houses' framework directly rather than inventing a parallel one: a Confederation is a **LivingWorldActor** (`actorType: "banditConfederation"`), tiered Background or Note exactly like a gens, with a leader generated as a full Character the moment the player actually faces or deals with them, a Force (Military & Combat §4.1's Irregular Combatant type) or Fleet (for a maritime Confederation specifically), a hideout or base location, and a standing reputation. **Banditry** is land-based — a hideout in difficult terrain (forest, hills, a frontier region); **Piracy** is sea-based — a hidden cove or island base, Fleet-built. Both are the same underlying actor type at different terrain and Combatant profiles, not two separate systems.

**A Confederation isn't static.** It inherits Rival Houses' own standing-trend concept (§2.1 of that doc) directly: a Confederation left unchallenged for a long stretch skews Rising — bolder, more frequent, and larger raids as it grows in strength and confidence — while one that's been recently bribed off or roughed up without being finished skews Declining, weaker and more risk-averse until it either rebuilds or fades out. Ignoring a real threat indefinitely is never a neutral, cost-free choice.

---

## 3. Being Raided — Threat, Interception, Consequence

A Confederation targets goods (a Trade Route, Economy & Finance §7; a caravan), livestock (Resources & Goods §3.2's rustling), Familia members (kidnapping, §8), or a settlement directly. Every raid resolves through the Combat Resolution Engine (Military & Combat §4) — the raiders as an Irregular Force against whatever defense is actually present (§9). **"Interceptable," as the core doc names it directly:** a well-defended target can repel or capture the raiders outright, using Military & Combat's own Defense deployment type; a captured raider is a real Character, available for a Legal & Court matter or — a real, deliberately full-circle irony — sale into slavery through Labor & Slavery's own acquisition pipeline.

---

## 4. Bribery & Tribute

Per the decision to support both scales:

- **A one-time payoff** — a specific brewing raid, once detected (via Reconnaissance-equivalent warning or an Espionage tip), can be bought off with a single payment, cheaper than the raid's likely losses, with no lasting relationship created.
- **A standing tribute** — a recurring Economy & Finance cost paid to a specific named Confederation of Note, keeping the player entirely off that Confederation's target list for as long as it's maintained — a real protection-racket dynamic, the direct inverse of a Military Supply Contract. This carries a real Dignitas risk if it becomes known: a citizen household quietly paying criminals for safety is a genuine social liability, reacted to sharply by a Traditionalist audience (Politics & Patronage §3.1) specifically.

---

## 5. Retaliation

Military & Combat's Offense/Campaign deployment type, aimed specifically at a Confederation's actual hideout or base — which usually has to be *found* first, an Espionage or Reconnaissance prerequisite rather than an automatically-known target. A successful retaliation nets real spoils (Military & Combat §7) and, pushed far enough, can eliminate the Confederation outright — the bandit/pirate equivalent of Rival Houses' own extinction, removing that actor from the world rather than just suppressing it for a season.

---

## 6. Turning Raider — Playing Both Ways

Per the decision to make this genuinely playable in both directions: the player's own Force or Fleet can conduct raids as the aggressor, resolving exactly like a Military & Combat Offense/Campaign and generating the same spoils and captives — but doing so **openly**, under the player's own colors, is a real scandal distinct from honest campaigning, carrying its own Dignitas and Reputation Duality cost proportional to how public it becomes. The real choice this creates: raid openly and keep the full proceeds at real personal risk, or route it through a hired intermediary (§7) instead, trading a cut of the profit for deniability. **That trade isn't risk-free, though** — a hired Confederation can simply keep the proceeds and report the raid failed, a real reliability risk read off the leader's own Honor axis and the House Standing already established between them and the player (Rival Houses §5.2); deniability from a criminal intermediary was never the same thing as trustworthiness.

---

## 7. Allying With & Contracting Raiders

The richer relationship per direction — using bandits and pirates as tools, not just fighting them:

- **Contracted raids** — paying an existing Confederation to raid a specific target (a rival's shipment, a rival's own trade route) on the player's behalf. This is a real transaction, not a Scheme: the Confederation conducts the raid using its own Force, and repeated use moves House Standing (Rival Houses §5.2) between the player and that Confederation toward Allied.
- **Sourced goods and slaves** — buying raid-sourced goods or people, already established directly in Labor & Slavery §2 ("Piracy & kidnapping — buying from a source that is itself a Piracy & Banditry outcome") and Resources & Goods' own black-market-adjacent trade; this document simply confirms the mechanism rather than redesigning it.
- **A genuine alliance** goes further still: an Allied Confederation can supply auxiliary raiding support during the player's own Military & Combat campaigns, or pass along intelligence on a rival's own vulnerable shipment — an Espionage-adjacent benefit that costs nothing extra to arrange once the relationship is real.
- **The exposure is real, not cosmetic.** Being caught consorting with criminals is its own scandal, and — consistent with Espionage's own "hard to prove conclusively" texture — a Legal & Court case built on this kind of association is a real, live, but rarely airtight risk.

### 7.1 Targeted Contracts — Kidnap, Kill, Enslave

A sharper, far higher-stakes order of business than raiding a shipment: paying a Confederation to go after a **specific, named individual** — a rival house's Head, an heir, a daughter, anyone a Character record exists for — rather than a generic target type. Three real contract outcomes, not one generic "harm them" button:

- **Kidnap** — the target is taken and held, opening the same Ransom negotiation (Characters §9.5) or leverage use (Blackmail Leverage, Characters §7) a kidnapping normally would, except now deliberately arranged by the player rather than opportunistic.
- **Kill** — a contracted assassination. If it succeeds against a house's own Head or a critical heir, it's a direct, real trigger into Succession & Dynasty's own handoff or dispute mechanics (§5-6 of that doc), and — where the target was already the last viable heir — a genuine extinction trigger for that house (Rival Houses §5.3), the darkest and most consequential outcome this entire document can produce.
- **Enslave** — the sharpest, most personal option of the three: rather than ransom or death, the target is delivered directly into the contracting player's own household as a slave. This is a deliberate, targeted status inversion aimed at humiliating a specific enemy, not merely removing them, and is treated with correspondingly heavier social weight (below) than an ordinary kidnapping-for-ransom.

**Cost and prerequisite.** A targeted contract is expensive, scaling with the target's own Dignitas and personal security, and requires a Confederation the player already has *some* real standing with — Rival Houses' own default Neutral relationship isn't enough; this needs at least the working trust an established contracted-raid or tribute relationship (§4, §7) already builds. A first-contact Confederation simply won't take a contract this sensitive.

**Resolution reuses Espionage's Discovery/Traceability model directly** (that document's §6) rather than inventing a parallel one: first, whether the operation succeeds, is botched (the target escapes, possibly now warned and guarded), or fails outright against the target's own security; second — independently — whether it's ever traced back to the player specifically, weighted by the Confederation's own concealment quality against the target house's own investigative capability. **Consequences scale with both axes together, not just the outcome:** a clean, untraced success is the outcome that actually pays off; an untraced failure just wastes the payment; a traced attempt — successful or not — is close to the single most severe diplomatic incident this document can produce, realistically an instant Feud (Rival Houses §5.2) and a live capital Legal & Court case, not merely a Standing dip.

**This isn't free to spam.** Repeated use against the same target or the same Confederation raises a cumulative suspicion the individual-operation Traceability roll doesn't fully capture on its own — too many convenient deaths and disappearances near one house is itself a pattern anyone paying attention eventually notices, regardless of how clean any single operation was.

**Symmetry, stated plainly:** nothing here is a special player-only tool. A rival house with the means and the motive can contract exactly the same operation against the player's own family — consistent with Rival Houses §4.1's own "the player is never a protected special case" principle — and a well-guarded head of household or a traveling family member with a real Retinue (Companions & Court Positions §7) is precisely how the player defends against it, the same security investment §9 already covers.

**One further wrinkle worth naming:** a Confederation the player has used isn't necessarily loyal simply because it took payment. A sufficiently wealthy or persuasive rival — the very house the player targeted, or another party entirely — can approach that same Confederation afterward and buy its loyalty away, the actor-level mirror of Espionage's own double-agent mechanism (that document's §6). A middleman used once is a middleman someone else can also try to use.

---

## 8. Kidnapping — Goods, Livestock, and People

- **Goods and livestock** — already covered directly (Resources & Goods §3.2's rustling, Economy & Finance §7's trade disruption).
- **Background population** — a raid on an under-defended settlement can carry off Coloni or Operarii, feeding a Confederation's own slave pipeline and a real Settlement Demographics consequence, not just a goods loss.
- **Familia members** — per the decision to make this a real risk: a household member, especially while traveling (Travel §4's real-stakes en-route events), is a genuine kidnap target. Capture resolves through the same mechanics Military & Combat and Characters already use, opening straight into a **Ransom** negotiation (Characters §9.5) with the Confederation as the demanding party — and, where ransom goes unpaid or mishandled, a real, historically consistent worst case: the captive sold into slavery themselves, or worse, rather than a guaranteed safe return.

---

## 9. Estate Security — The Defensive Investment

The concrete mechanism behind "scale with the player's security investment," using operators and buildings this project already named without ever wiring up: the Vigil and Praefectus Vigilum (Companions & Court Positions), the Navarchus and Navarchus Princeps for maritime exposure, Watchtower/City Walls (Buildings), and a standing Fleet (Military & Combat §4.1). Real investment across these lowers a target's odds twice over — a better-defended estate is both less likely to be targeted for a raid in the first place (a worse risk-reward proposition for the Confederation) and far more likely to repel one outright (§3) if targeted anyway.

---

## 10. Cross-System Integration

- **Military & Combat:** the Combat Resolution Engine, Fleet, and Irregular Combatant type are all reused wholesale, exactly as that document intended.
- **Rival Houses:** the Living World Actor framework and its own "banditConfederation" actor type are fully realized here; retaliation's extinction outcome mirrors that document's own; §7.1's targeted kill contracts are a genuine, deliberate path to that document's extinction trigger, not just an incidental one.
- **Succession & Dynasty:** §7.1's targeted kill contracts, when successful against a Head or critical heir, are a direct, real trigger into that document's own handoff and contested-succession mechanics — assassination as a genuine political tool, not just flavor text.
- **Espionage:** a Confederation's hideout location is a natural infiltration/Reconnaissance target; the deniability question in §6-7 reuses that document's own Traceability logic directly; §7.1's contract resolution and its "flipped middleman" risk both reuse that document's Discovery/double-agent model wholesale.
- **Labor & Slavery:** kidnapping and Piracy-sourced acquisition (§2 of that doc) are fully realized; a captured raider entering the slave pipeline (§3) closes a genuinely circular loop.
- **Resources & Goods:** livestock rustling and trade disruption are fully realized as concrete raid types.
- **Economy & Finance:** standing Tribute (§4) is a direct, named recurring expense category; contracted raiding (§7) is a real income/leverage tool.
- **Companions & Court Positions:** the Vigil, Praefectus Vigilum, Navarchus, and Navarchus Princeps finally get their actual mechanical function.
- **Travel:** en-route ambush risk (§4 of that doc) is this document's concrete supplier; Familia-member kidnapping is realized directly.
- **Characters:** Ransom (§9.5) is reused directly for a kidnapped Familia member's resolution.
- **Legal & Court:** a captured raider's disposition and a discovered collusion-with-criminals case are both concrete forward hooks.
- **Politics & Patronage:** open raiding and known tribute payment both carry real Faction-dependent Dignitas consequences.
- **Settlement Demographics:** a raid carrying off background population is a real, direct population consequence distinct from a simple goods loss.

---

## 11. Data Model

```
// BanditConfederation reuses Rival Houses' LivingWorldActor directly:
// actorType: "banditConfederation", with terrain/domain distinguishing land (Banditry) from sea (Piracy)
// standingTrend (Rival Houses §2.1, reused) — "rising" | "established" | "declining"; drives raid frequency/boldness over time
// loyaltyToContractor — per-relationship field, read against the leader's Honor axis for §6's reliability risk and §7.1's flip risk

RaidEvent {
  raidId,
  confederationActorId,
  targetType,          // "tradeRoute" | "livestock" | "settlement" | "familiaMember" | "backgroundPopulation"
  defenderSecurityLevel,   // §9 — read from Vigil/Navarchus/fortification investment
  outcome,             // "interceptedRepelled" | "raidersCaptured" | "raidSucceeded" | "boughtOff"
  spoilsLost, captivesTaken: [...],
}

TributeArrangement {
  confederationActorId,
  type,               // "oneTimePayoff" | "standingTribute"
  monthlyAmount,        // set only for standingTribute
  isPubliclyKnown: bool,   // §4 — drives the Dignitas risk
}

PlayerRaid {          // §6 — the player as aggressor
  raidId,
  method,             // "openDirect" | "contractedIntermediary"
  targetActorId,        // a rival house, a Confederation, a trade target
  contractedConfederationActorId,   // set only for "contractedIntermediary"
  intermediaryDelivered: bool,   // §6 — false if the Confederation simply kept the proceeds
  outcome, spoilsGained, captivesGained,
  discoveredPublicly: bool,
}

TargetedContract {      // §7.1 — kidnap/kill/enslave against a specific named Character
  contractId,
  contractingActorId,     // the player, or a rival house doing the same thing symmetrically
  targetCharacterId,
  contractedConfederationActorId,
  contractType,         // "kidnap" | "kill" | "enslave"
  cost,               // scales with target Dignitas/personal security
  resultOutcome,         // "cleanSuccess" | "botchedTargetEscaped" | "failedAgainstSecurity"
  traced: bool,          // independent of resultOutcome — resolved via Espionage §6's model
  cumulativeSuspicion,     // rises with repeated use against the same target/Confederation, §7.1
  triggeredSuccessionEvent: bool,   // true if "kill" succeeded against a Head/critical heir
  triggeredExtinction: bool,       // true if the kill left the target house with no viable heir
}

KidnappingRecord {
  victimCharacterId,
  confederationActorId,
  ransomDemanded,
  resolution,          // "ransomed" | "rescued" | "soldIntoSlavery" | "unresolved"
}
```

---

## 12. Open Questions

- **All numeric sizing.** Consistent with this project's convention: raid-trigger frequency, the security-investment-to-risk curve, ransom amounts, and tribute pricing are all unsized.
- **Retaliation's location-finding prerequisite.** §5 requires knowing a Confederation's actual base before striking it; the exact Espionage/Reconnaissance threshold that reveals one isn't specified.
- **Open raiding's Reputation Duality split.** §6 notes open raiding carries a Dignitas cost; whether it affects standing-with-Rome and local standing identically or differently (given Reputation Duality's own frontier-specific divergence) isn't decided.
- **Multiple Confederations contracted against each other.** §7 doesn't address what happens if the player contracts one Confederation against a target that's itself Allied with a different Confederation the player also deals with.
- **Kidnapped-Familia-member recovery beyond Ransom.** §8 names Ransom as the primary path; whether a direct rescue attempt (a Military & Combat strike on the holding location) is a realistic alternative isn't detailed beyond the general retaliation mechanism in §5.
- **Targeted contract pricing curve.** §7.1 establishes cost scales with the target's Dignitas and personal security without specifying the actual formula.
- **Cumulative suspicion's exact decay.** §7.1 notes repeated use raises suspicion beyond any single operation's own Traceability roll; whether and how that suspicion ever fades with time or inactivity isn't specified.
- **Confederation standing-trend growth rate.** §2 borrows Rival Houses' Rising/Established/Declining concept directly; the actual rate at which an ignored Confederation escalates isn't sized.
- **Minimum relationship threshold for a targeted contract.** §7.1 requires "some real standing" beyond Neutral before a Confederation will accept a contract this sensitive, without specifying the exact Standing level required.
