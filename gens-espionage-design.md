# GENS — System Design: Espionage & Information Network (§6.15)
*Genuinely compositional — the Household Spymaster position, the Cryptoporticus, and Characters' entire Coercive/Intrigue Interaction set were all built with this system specifically in mind. This document is where they actually connect into a working spy network, rather than each sitting as an isolated forward reference. This pass adds double agents and real disinformation risk, a spy's own Loyalty as an independent defection risk, extends targets to the full Living World Actor framework (pirates, foreign peoples, cults), and closes two balance gaps — ongoing network upkeep and a Spymaster capacity cap — that would otherwise have let a Persistent Network scale for free.*

---

## Contents

1. Scope & Role
2. Spy Placement — Quick Op vs. Persistent Network
3. Targets — Household, Rival, and Administration
4. What a Spy Actually Delivers
5. Counter-Espionage
6. Discovery, Traceability & Consequences
7. The Spymaster
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role

The core doc's own definition: "spies/informants within the player's own household, a rival's, or the local administration, generating blackmail material, early warnings, and covert-sabotage options that feed Politics and Rival Houses directly." Romance & Seduction's own core doc line adds a second feed in the other direction: seduction pursued "for leverage, blackmail, alliance, or information, feeding Politics **and Espionage**" — the two systems hand material back and forth.

This document is unusually compositional even by this project's own standard. Nearly everything it needs already exists:

- **Companions & Court Positions** already named a **Household Spymaster** (Domus stage, Intrigue attribute, based at the Cryptoporticus) without defining what they actually do.
- **Villa** already gave that role a physical home: "the room that gives Espionage & Information Network a physical venue for a covert meeting or hiding someone from view."
- **Characters** already built Recruit/Plant a Spy, Fabricate a Hook/Blackmail Material, Sabotage, Frame, and Spread a Damaging Rumor (§9.4), plus a standing **Blackmail Leverage** bond tag (§7) and the full Scheme engine (§10) — all explicitly built with this system in mind.
- **Legal & Court** just named blackmail material as a real, if uglier, source of case leverage.

This document's job is almost entirely to connect these pieces into an actual working network rather than invent new mechanics from scratch.

---

## 2. Spy Placement — Quick Op vs. Persistent Network

Per the decision to offer both, at genuinely different cost and risk:

### 2.1 Quick Op

A single-use Scheme — Characters' existing Recruit/Plant a Spy interaction, resolved once for one specific piece of intel (a Dossier reveal, advance word on a single decision, one document) rather than creating any lasting asset. Cheaper, lower cumulative risk (one roll, not an accumulating one), and done the moment it resolves — nothing left behind to discover afterward.

### 2.2 Persistent Network

Embeds a real Character — drawn from Clientela, a Companion, or a freshly recruited/planted individual — inside a target as a standing asset. Costs more upfront and carries a real ongoing exposure: **Discovery Risk climbs the longer the placement runs**, the same shape a Scheme's own discovery risk already rises with time (Characters §10), just resolved as a recurring background check rather than a single countdown. In exchange, an undiscovered Persistent Network delivers the recurring, passive benefits in §4 for as long as it survives.

**It also isn't free to maintain.** A modest recurring Economy & Finance cost attaches to every active Persistent Network — an embedded spy still needs paying, hiding, or otherwise supported, consistent with how nothing else standing in this project runs at zero ongoing cost (a Squad's Wages, a Clientela roster's upkeep). **Capacity is capped, too:** a single Household Spymaster (§7) can only reliably run so many Persistent Networks at once — the same soft-cap logic Traits §5.3 already applies to Lifestyle specializations — so growing a real network is a genuine investment in the Spymaster role itself, not a resource the player can scale without limit just by paying for more placements.

---

## 3. Targets — Household, Rival, and Administration

- **A rival house** — Rival Houses' own framework applies directly; any House of Note is a real, reachable target the moment the player has cause to infiltrate it.
- **The local Roman administration** — a magistrate's staff, a provincial governor's court, occasionally a Legion's own officer corps: new territory this document opens, giving advance insight into a legal ruling's leaning, an upcoming Sumptuary sweep, a tax assessment, or a campaign's timing before any of it becomes public.
- **Any other Living World Actor** — Rival Houses §6 generalized its own tiered actor model past gentes specifically; this document inherits that generalization rather than stopping short of it. A pirate confederation (advance word of a planned raid), a foreign people or petty kingdom (intelligence ahead of a Diplomacy negotiation), or a religious institution are all reachable exactly the same way a rival house is, once they've reached Houses-of-Note-equivalent standing.
- **The player's own household** — the mirror image: an enemy can place a spy here exactly the same way the player can place one elsewhere, which is what §5's counter-espionage actually exists to catch.

---

## 4. What a Spy Actually Delivers

- **Blackmail Material.** A spy is the concrete agent behind Fabricate a Hook (Characters §9.4) — the one who actually digs up a target's compromising secret (an affair, per Faithful/Adulterous; a hidden illegitimate child; financial corruption; a buried crime) and converts it into usable Blackmail Leverage (Characters §7), the standing bond tag a separate Blackmail/Extort Interaction then spends.
- **Early Warnings.** A Persistent Network's core passive benefit: a rising chance of being tipped off before an enemy Scheme targeting the player resolves — mechanically, it strengthens the player's own side of that Scheme's discovery-risk math (Characters §10). A well-placed spy makes the household genuinely harder to successfully scheme against, not just informed after the fact.
- **Covert Sabotage.** Some Sabotage targets (Characters §9.4) simply aren't reachable at all without an embedded asset already in place — poisoning a specific stockpile, disrupting a named Squad's Readiness before an engagement (Military & Combat §2.3), tampering with a rival's Trade Route (Economy & Finance §7). The spy is the access, not just a bonus to an already-possible action.
- **Dossier Currency.** For a Rival House target specifically, an embedded spy is the concrete override to Rival Houses §7's own information-staleness rule — a house being actively spied on stays genuinely current rather than only as fresh as the last real contact.
- **Administrative Foreknowledge.** Against the local administration specifically: advance word on a Legal & Court ruling's leaning (ahead of that document's own magistrate-scouting, which only applies once a case is already filed), a Sumptuary sweep, or — rare and valuable — a campaign's actual timing.

---

## 5. Counter-Espionage

Per judgment call, blending a light passive baseline with a real active option rather than picking purely one or the other: a well-staffed Household Spymaster (§7) quietly raises the ambient odds of noticing an embedded enemy spy simply by existing, reading their own Intrigue and relevant Traits — no player action required for this baseline improvement. On top of that, the player can task a deliberate, costed **Sweep** — a real Interrogate-adjacent effort, spending time and Influence, that directly rolls against whatever enemy spy is currently embedded rather than waiting on the ambient baseline alone. This gives a genuine choice between low-effort improvement and active vigilance, without demanding either as the only option.

---

## 6. Discovery, Traceability & Consequences

Per the decision that the outcome genuinely depends on circumstances rather than following one fixed rule, this resolves as two distinct rolls rather than a single binary catch/no-catch:

1. **Discovery** — whether the spy is caught at all. A Quick Op carries a single, lower cumulative risk; a Persistent Network's risk climbs the longer it runs, per §2.2.
2. **Traceability** — if caught, whether the trail actually leads back to the player. This is its own real roll, weighted by the spy's own concealment quality (a higher-Intrigue spy, or a more carefully/expensively arranged placement, leaves a cleaner trail) against the target's own investigative capability (a high-Intrigue Head, a well-staffed rival Spymaster equivalent, or the administration's own competence).

The consequence genuinely differs by outcome, rather than being fixed in advance:

- **Caught, not traced** — the spy alone faces whatever the target chooses: imprisonment, execution, or Ransom (Characters §9.5 and Military & Combat §5.5's existing capture-resolution options) end the placement outright and cost the player only the lost asset. A real third option belongs alongside those, though: the target can choose to **turn** the spy instead of punishing them, keeping the placement nominally active from the player's perspective while it now quietly serves the other side. A turned spy is the concrete source of **disinformation** — intel that reads as genuine but is deliberately false, sized to matter (a fabricated warning that sends the player's attention somewhere it isn't needed, a falsified Dossier figure) rather than being a cosmetic flavor risk. A turned spy is eventually the player's own problem to notice, through the same Traceability-style logic in reverse — nothing here guarantees the player ever finds out, which is exactly what makes the option worth the target choosing it.
- **Caught and traced** — a real diplomatic incident: House Standing moves toward Rivalrous or Feuding (Rival Houses §5.2), a Legal & Court case becomes a live possibility (though notably hard to *prove* conclusively, per that document's own evidence mechanics — accusation and conviction aren't the same thing), and if it becomes public, a real Dignitas cost and a Dynasty Chronicle Faith & Scandal entry.

**A spy is also a Character, not just an asset.** Independent of anything the target does, a long-embedded Persistent Network placement's own Loyalty (Familia's Condition stat) can erode exactly the way any other Character's would — sustained risk without reward, a better offer from the target side, or simply time and distance from the player's own household. A spy whose Loyalty collapses can defect on their own initiative, with no external "catching" involved at all — the same self-directed risk Politics & Patronage's own Clientela poaching already models, now applied to the one relationship in this system where the player has the most to lose from it going unnoticed.

---

## 7. The Spymaster

The Household Spymaster (Companions & Court Positions, Domus stage) is this system's one named operator, and this document is where their actual mechanical function finally gets defined rather than just their title and room. Their Intrigue directly drives §5's passive counter-espionage baseline and improves the odds of any Quick Op or Persistent Network the player initiates — both the offense and the defense of this system run through the same appointment, consistent with how a single well-chosen operator already carries real weight elsewhere in this project (an Ergastularius, an Argentarius).

---

## 8. Cross-System Integration

- **Characters:** Recruit/Plant a Spy, Fabricate a Hook, Sabotage, Frame, and the Blackmail Leverage bond tag are all reused directly rather than duplicated; the Scheme engine's discovery-risk shape is this document's own §2.2 and §6's model.
- **Companions & Court Positions:** the Household Spymaster finally gets a real mechanical function, not just a title and a room.
- **Villa:** the Cryptoporticus is this system's physical venue, exactly as that document already named it.
- **Politics & Patronage:** blackmail material and administrative foreknowledge are both direct, named feeds per the core doc; §6's Loyalty-driven defection reuses that document's own Clientela-poaching logic directly rather than inventing a parallel one.
- **Rival Houses:** infiltration targets a House of Note directly (and, per §3, its full generalization to any Living World Actor); a traced spy moves House Standing toward Rivalrous/Feuding; an embedded spy overrides that document's own Dossier-staleness rule.
- **Diplomacy with Non-Roman Peoples (§6.25, future) / Piracy & Banditry (§6.24, future):** both inherit this document's target model (§3) the same way they already inherit Rival Houses' tiered actor framework — a foreign people or a bandit confederation is reachable exactly like a rival house once it's noteworthy enough.
- **Legal & Court:** blackmail material as case leverage (that document's own cross-reference) and a traced spy's own hard-to-prove accusation are both realized here.
- **Romance & Seduction (§6.19, future):** the two-way feed the core doc names directly — a seduced target is a natural informant source, and Espionage-gathered material is natural seduction leverage — is confirmed as a real connection point for that system's own eventual pass.
- **Military & Combat:** Sabotage against a Squad's Readiness and advance campaign-timing intelligence are both concrete uses this document supplies.
- **Economy & Finance:** Trade Route sabotage is a named Sabotage target.
- **Dynasty Chronicle:** a publicly traced spy is real Faith & Scandal material.
- **Traits:** Spymaster (Traits §5.3) directly improves placement and concealment quality; Cunning Survivor and Paranoid both read naturally into this system's own texture without needing new tags.

---

## 9. Data Model

```
SpyPlacement {
  placementId,
  type,              // "quickOp" | "persistentNetwork"
  spyCharacterId,
  targetActorId,       // a Rival House or other LivingWorldActor, "administration", or the player's own household
  targetSpecific,       // e.g. a specific magistrate, a specific rival Character
  concealmentQuality,    // driven by spy Intrigue + placement cost/care
  discoveryRisk,        // rises over time for persistentNetwork; single roll for quickOp
  monthsActive,          // relevant only for persistentNetwork
  monthlyUpkeepCost,      // §2.2 — persistentNetwork only
  spyLoyalty,           // read from the spy's own Character record; independent defection risk, §6
  benefitsDelivered: [...],   // §4 — blackmail material generated, warnings given, sabotage enabled
  isDoubleAgent: bool,     // §6 — true once turned; the player is not necessarily aware
  disinformationDelivered: [...],   // §6 — only populated while isDoubleAgent is true
  status,              // "active" | "caughtNotTraced" | "caughtAndTraced" | "turned" | "defected" | "concluded" (quickOp only)
}

CounterEspionageSweep {
  sweepId, month,
  initiatingCharacterId,   // typically the Household Spymaster
  targetedPlacementId,      // the suspected enemy SpyPlacement, if one is actually found
  outcome,              // "nothingFound" | "spyIdentified"
}
```

---

## 10. Open Questions

- **All numeric sizing.** Consistent with this project's convention: Discovery Risk curves, Traceability weighting, Sweep costs, network upkeep, and the Spymaster's actual capacity cap number are all unsized.
- **Disinformation detection.** §6 deliberately doesn't guarantee the player ever learns a placement was turned; whether there's any mechanism at all to eventually suspect it (an inconsistency between a spy's reports and other evidence) or whether it can stay hidden indefinitely isn't decided.
- **Turning's own success condition.** §6 establishes turning as a real option a target can choose over punishment, without specifying what makes a target likely to attempt it rather than simply eliminate the spy — presumably a function of the target's own Rationality axis and how valuable ongoing disinformation would be to them.
- **Multi-settlement Spymaster coverage.** Companions & Court Positions' Household Spymaster is a single Domus-stage appointment; whether a player running multiple settlements (per that document's own Procurator mechanic) needs a second, provincial-scale espionage role isn't addressed here and is left to a future revision of that document if it proves necessary.
- **Romance & Seduction's seduced-informant mechanic.** §8 confirms the connection point exists; the actual mechanic (does a seduced target become a Persistent Network placement automatically, or a distinct thing) is left to that system's own pass.
- **Legal & Court accusation-without-proof texture.** §6 notes a traced spy is "hard to prove conclusively" without specifying what a Legal & Court case actually looks like when it can't produce a clean verdict either way.
