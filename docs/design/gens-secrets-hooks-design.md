# GENS — System Design: Secrets & Hooks (§6.59)

*Built explicitly on CK3's own Secrets-and-Hooks model, per direction, and unusually compositional even by this project's own standard: Characters' Fabricate a Hook / Blackmail Material Interaction and its standing Blackmail Leverage bond tag (§7 of that document), Espionage's own "a spy is the concrete agent behind Fabricate a Hook" delivery mechanism (§4), and Scandal's entire severity/exposure machinery were all built with exactly this system in mind, and have been quietly waiting on it. What's never existed until now is the thing itself: a real, catalogued **Secret** record independent of who currently knows it, and a **Hook** with actual internal state — Strong or Weak, usable, tradeable, and spendable — rather than a flat yes/no bond tag. This document is where that finally gets built, changing no existing mechanic in the process.*

---

## Contents

1. Scope & Role
2. The Secret — A Real, Discrete Record
3. The Secret Catalog
4. Discovery — How a Secret Becomes Known
5. The Hook — Turning Knowledge into Leverage
6. Using a Hook — Compel, Extort, Expose
7. Hook Decay, Trading & Expiration
8. Fabricated Hooks — Reusing an Existing Pattern
9. Preemptive Confession
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role

Every piece this document needs already exists somewhere else, described but never formalized:

- **Characters** already built **Fabricate a Hook / Blackmail Material** (Multi-stage, §9.4) and **Blackmail / Extort** (Quick, once material exists), plus a standing **Blackmail Leverage** bond tag (§7) — "one party holds damaging material on the other... a standing, usable threat rather than just poor opinion."
- **Espionage** already named the *content* those Interactions produce, purely descriptively: "the one who actually digs up a target's compromising secret (an affair, per Faithful/Adulterous; a hidden illegitimate child; financial corruption; a buried crime) and converts it into usable Blackmail Leverage" (§4).
- **Scandal** already owns the entire public-exposure aftermath — severity tiers, scope, spread, consequence — the instant something private goes public.
- **Crime, Punishment & Imprisonment** already built the exact shape a fabricated record needs to take (§9's Fabricating Justification) — a manufactured fact that resolves identically to a real one until seriously challenged, and backfires hard on the fabricator if it is.

This document's job is almost entirely to connect these pieces into one real, working system rather than invent new mechanics from scratch — exactly the role Espionage itself played for the spy network a few documents ago. It changes no existing mechanic: Characters' Scheme engine still resolves Fabricate a Hook exactly as specified; this document defines what that Scheme actually *produces*. Scandal keeps sole ownership of what happens once something is public.

---

## 2. The Secret — A Real, Discrete Record

The same move Scandal made for scattered discovery moments, applied one layer earlier: a **Secret** is a real, dated, discrete record — not a passive flag quietly implied by an affair or a debt somewhere else in the data, but a specific fact about a specific Character that other Characters may or may not currently know.

A Secret is born the moment its underlying real event occurs — an affair begins, a crime is committed, a debt is concealed — not only once someone happens to go looking for it. This means a Secret can sit entirely unknown, or known only to its own holder, for a long time before anyone ever runs a Scheme against it; §4 is about how that changes.

Every Secret carries:

- A **Holder** — the Character it's actually about.
- A **Type** — §3's catalog.
- A **Severity Tier** — reused directly from Scandal's own three-tier scale (`minorEmbarrassment` / `publicDisgrace` / `notaCensoriaEligible`, rendered in §3's table as Minor Embarrassment / Public Disgrace / Nota Censoria-Eligible) rather than inventing a fourth naming scheme, since a Secret's severity *is* simply what its eventual Scandal would read as if it ever went public.
- Zero or more **Knowers** — Characters currently holding a Hook on it (§5).
- An **Exposure state** — private or exposed (§6, §9).

A Secret outlives its own Holder. Death doesn't erase the underlying fact — a buried crime or a concealed true parentage discovered after the Holder's death still resolves through this same record, still generates Scandal if Exposed, and can still cost the household Memoria (Ancestor Veneration & Funerary Customs) or complicate a Succession dispute exactly as it would have while the Holder was alive.

---

## 3. The Secret Catalog

Real Roman-grounded secret types, each tied to the system that actually originates or resolves the underlying fact — this document invents no new fiction, only names what several existing documents already imply.

| Secret Type | Real Grounding | Default Severity | Cross-Tie |
|---|---|---|---|
| Illegitimate Parentage | A child outside a recognized marriage — including the sharper case where the legal father genuinely doesn't know | Public Disgrace | Familia's Legitimacy, Romance Sexuality & Lineage's Autonomous Romance |
| Adultery/Affair | Espionage's own named clearest example | Public Disgrace | Romance, Sexuality & Lineage §11 — already Scandal's single clearest named source |
| Concealed Servile/Foreign Origin | A freed or foreign-born Character passing as freeborn | Public Disgrace | Labor & Slavery, Fashion & Dress's Assimilated/Unbowed |
| Concealed True Parentage/Adoption | An heir raised as blood kin who is, in fact, adopted | Public Disgrace | Succession & Dynasty's own adoption path |
| Buried Crime/Murder | A killing never brought before Legal & Court | Nota Censoria-Eligible | Legal & Court, Masterworks' Poison Ring |
| Financial Fraud/Embezzlement | A skimming Institor, a falsified account | Public Disgrace | Economy & Finance, Character Ambitions' own Marcus-the-skimming-Institor example |
| Concealed Debt | A debt hidden from a spouse or patron | Minor Embarrassment to Public Disgrace | Economy & Finance's DebtRecord |
| Proscribed Religious Practice | Real historical precedent: the actual 186 BC Bacchanalia suppression | Public Disgrace | Religion §7's foreign cults |
| Broken Religious Vow | A lapsed Nazirite-style vow, a Gallus's own broken commitment | Public Disgrace | Hair, Body & Marking's VowBoundHairRecord, Religion |
| Treason/Conspiracy | Plotting against the state or a rival house | Nota Censoria-Eligible | Politics & Patronage, Rival Houses |
| Secret Foreign Alliance | A quiet arrangement with a client king or foreign people against Rome's own interest | Nota Censoria-Eligible | Diplomacy with Non-Roman Peoples, Client Kingdoms & Vassal Rulers |
| Vestal Chastity Violation | Religion's own already-named capital-case tier | Nota Censoria-Eligible | Religion §6.3 |
| Espionage Collaboration | Acting as an enemy's spy before being traced | Public Disgrace | Espionage §6's Discovery/Traceability |
| Piracy/Banditry Collaboration | Consorting with raiders | Public Disgrace | Piracy & Banditry §7's own exposure risk |
| Complicity in a Scheme | Knowingly covering for another's Scheme | Minor Embarrassment to Public Disgrace | Characters' Scheme engine |
| A Disgraceful Past Act | A hidden desertion, a family's own buried prior-generation Scandal | Minor Embarrassment to Public Disgrace | Military & Combat, Dynasty Chronicle |
| Broken Betrothal in Bad Faith | A reneged marriage promise | Minor Embarrassment | Familia's marriage market, Politics & Patronage |

---

## 4. Discovery — How a Secret Becomes Known

Four real paths, none of them a new mechanic in their own right — this section connects existing ones rather than adding a discovery roll this project doesn't already have somewhere:

- **Active Investigation** — Characters' existing Fabricate a Hook / Blackmail Material Scheme (§9.4), Intrigue vs. the target's Perceptive/Oblivious. Success creates a new Hook (§5) against an existing Secret. Per Espionage §4, the investigator digging is very often a placed spy doing exactly this.
- **Witnessed** — reusing the Witness Pool concept Activity Engine content already established (a Wedding's *deductio*, the Toga Virilis's Forum Procession): an act performed where a Witness Pool is present carries a real, standing chance that an attending Character — a Companion, a household slave who serves everywhere — simply already knows, with no Scheme ever run.
- **Confession** — a Character reveals their own Secret to another, deliberately or under Interrogate — the one Discovery method that creates a Hook with no risk roll at all, since the Secret's own holder chose it.
- **Intercepted Correspondence** — a letter never meant to be read by anyone but its addressee, seized or copied via Correspondence & Letters' own interception mechanic — a real, physical-evidence Discovery route distinct from an Espionage placement, and often the single strongest evidence a Hook can be built from.
- **Inheritance** — when a Knower dies, their Hook doesn't automatically vanish; it passes to an heir who was actually told, or lapses if it wasn't, mirroring how this project already treats relationship-web content surviving a succession.

A fifth, deliberately partial state worth naming separately: a Secret can also simply **leak as an unconfirmed Rumor** through Graffiti, Dynamic Walls & Rumors' own existing rumor-circulation mechanic, without yet rising to a real Hook or a full Exposure. This is the honest middle ground CK3 itself doesn't model but Roman social life plainly had — everyone at the baths half-believes something about a household without anyone actually holding usable leverage or the matter being formally, damagingly public yet. A Rumor can harden into a real Hook if someone follows up with an actual Active Investigation, or fade back into background noise if no one does.

---

## 5. The Hook — Turning Knowledge into Leverage

A Hook is the specific, live relationship between one Knower and one Secret — the direct formalization of Characters' existing Blackmail Leverage bond tag, now given real internal state instead of a flat yes/no:

- **Strength** — Strong or Weak, set at creation by how solid the Discovery actually was: a directly Witnessed act, or a high-margin Fabricate a Hook success, is Strong; a low-margin success, hearsay, or a Fabricated Hook (§8) starts Weak.
- **The Blackmail Leverage bond tag is the Hook's own visible face** on the relationship web (Characters §7) — this document adds no parallel bond, it gives the existing one real teeth.
- **Holding a Hook is not the same as using one.** A high-Compassion, high-Honor Knower may simply know and never weaponize it — exactly the way Personality Axes already color the Betray Interaction's own resolution, reused here rather than duplicated.

---

## 6. Using a Hook — Compel, Extort, Expose

Three distinct real uses, all resolving through the existing Blackmail/Extort Quick Interaction (Characters §9.4) rather than three new ones:

- **Compel** — force a single, specific, mechanical action: a Curia vote (Politics & Patronage), breaking a betrothal (Familia), stepping down from a Companions & Court Position, forgiving a debt (Economy & Finance's DebtRecord), handing over a sum or a Masterwork. The compelled action reads whatever system it targets directly rather than this document inventing a parallel effect.
- **Extort (ongoing)** — a standing, repeated drain of Influence or denarii for as long as the Hook stays unspent, at the real, growing risk that a resentful, repeatedly-bled target eventually risks everything on a Betray or a Legal & Court case rather than keep paying.
- **Expose** — the Knower makes the Secret public rather than holding it privately, immediately triggering Scandal at the Secret's own Severity Tier. Usually the worse choice for the Knower's own continued leverage — an exposed Secret generates no further Hooks for anyone — but the right one when the goal is pure damage: a rival house burning a Hook publicly to wreck a marriage alliance rather than quietly bleeding it for favors.

---

## 7. Hook Decay, Trading & Expiration

- **Use weakens a Hook.** A Strong Hook spent on Compel or Extort downgrades to Weak; a Weak Hook spent is destroyed outright — though the underlying Secret itself may still exist, and could still be independently Exposed or freshly Discovered by someone else entirely.
- **A Hook can be traded.** Gifted or sold to an ally exactly like any other favor — a Patron passing a useful Hook to a Client to do the compelling instead, or a rival house's own gesture toward Allied Standing (Rival Houses §5.2). The Hook's own Strength transfers unchanged; only the Knower changes.
- **A Hook expires.** If the Secret is independently Exposed by anyone, every other Knower's Hook on it loses all value simultaneously, since a public fact stops being leverage the instant it's public. If the underlying fact simply becomes moot on its own — a concealed debt gets paid off, an affair's marriage ends anyway — the Hook fades unspent.

---

## 8. Fabricated Hooks — Reusing an Existing Pattern

A direct sibling to Crime, Punishment & Imprisonment §9's own **Fabricating Justification**, not a parallel invention: the same underlying pattern — Characters' existing Frame Interaction manufactures a false record that resolves identically to a real one until seriously challenged, and a *discovered* fabrication backfires severely onto the fabricator — applied here toward inventing a false Secret instead of a false Punishable Offense.

This document doesn't duplicate that risk math; it reuses it wholesale. `Secret.isFabricated` is this document's own direct mirror of that document's `PunishableOffense.isFabricated` flag, and a discovered Fabricated Hook lands on Scandal's own already-named worst case — "retroactively the single worst-case scandal source this project has built" — on the *fabricator* rather than the falsely accused target, who gains real sympathy instead of damage. A Fabricated Hook also starts at Weak Strength by default (§5), reflecting that it was never actually earned by real evidence.

---

## 9. Preemptive Confession

A genuine strategic option this document adds directly: a Secret's own Holder can get ahead of it by confessing publicly before anyone else exposes it. This still triggers Scandal at the Secret's own Severity Tier, but per judgment call, at a real, meaningful discount compared to being caught — the honest social difference between owning a mistake and being exposed in one. Confessing immediately destroys every outstanding Hook on that Secret, since it's no longer secret from anyone: a costed, real way to disarm a rival's accumulated leverage entirely rather than continuing to live under it.

---

## 10. Cross-System Integration

- **Characters:** formalizes Fabricate a Hook, Blackmail/Extort, Frame, and the Blackmail Leverage bond tag directly; adds no new Interaction of its own.
- **Espionage:** a spy's own "Blackmail Material" delivery (§4) is now, concretely, this document's Discovery mechanism in action; a traced spy's own Espionage Collaboration (§3) is itself a Secret before it's ever traced.
- **Scandal:** Expose (§6) and a caught Extortion both resolve as this document's trigger into that document's existing severity/scope/consequence machinery — this document supplies the Secret's own Severity Tier directly as Scandal's input, changing nothing about how Scandal itself resolves.
- **Crime, Punishment & Imprisonment:** §8's Fabricated Hooks reuse that document's Fabricating Justification pattern wholesale rather than duplicating it; a Buried Crime Secret (§3) is a natural precursor to that document's own Punishable Offense flag once discovered.
- **Religion:** a Vestal Chastity Violation Secret (§3) ties directly to that document's own already-named capital-case tier (§6.3).
- **Romance, Sexuality & Lineage:** an affair is both a Secret Type (§3) and Scandal's own named clearest source — this document is the missing link connecting the two.
- **Politics & Patronage:** Compel's Curia-vote use (§6), and Hook trading (§7) between a Patron and Client.
- **Economy & Finance:** Compel's debt-forgiveness use, and Concealed Debt as a Secret Type.
- **Rival Houses:** Hook trading as a genuine alliance gesture (§7); a rival's own Secret is a natural Dossier-adjacent target for a Persistent Network.
- **Character Ambitions:** the skimming Institor worked example (Marcus) is this document's own Financial Fraud Secret in miniature, already fully realized elsewhere in the project.
- **Traits:** Honor, Compassion, and Vengefulness color whether a Knower ever chooses to use a Hook at all, exactly as they already color Betray — no new personality layer introduced.
- **Piracy & Banditry:** consorting-with-criminals exposure (§7) is both a direct Secret Type and a Discovery source.
- **Legal & Court:** a live Hook is exactly the "uglier, if real" case leverage that document already named without a mechanism behind it — a Compel or an Expose can supply evidence for an active case directly, and a Buried Crime Secret is that document's own natural pre-trial state.
- **Dynasty Chronicle:** a Nota Censoria-Eligible Exposure, a Vestal Chastity Violation, a discovered Fabricated Hook, or a dramatic Preemptive Confession are all real, guaranteed-weight entries — this document generates Chronicle material at least as reliably as Scandal itself does.
- **Correspondence & Letters:** that document's own interception mechanic is this document's Intercepted Correspondence Discovery path (§4) directly.
- **Graffiti, Dynamic Walls & Rumors:** that document's rumor-circulation content is this document's own unconfirmed-Rumor middle state (§4) — a real, existing mechanism this document didn't have a name for until now.
- **Succession & Dynasty:** Concealed True Parentage/Adoption (§3) is a direct, high-stakes tie to that document's own heir-eligibility and adoption rules; an Exposure landing after a succession has already completed is a genuine, late-arriving complication that document should be prepared to read.
- **Diplomacy with Non-Roman Peoples / Client Kingdoms & Vassal Rulers:** Secret Foreign Alliance (§3) is a direct Secret Type sourced from either document's own existing relationship content.
- **Hair, Body & Marking:** a Broken Religious Vow (§3) reads that document's own VowBoundHairRecord directly as its originating fact.
- **Companions & Court Positions:** Compel's forced-resignation use (§6) reads that document's own appointment mechanic directly rather than inventing a parallel removal process.

---

## 11. Data Model

```
Secret {
  secretId,
  holderCharacterId,
  secretType,             // enum from §3's catalog
  severityTier,            // "minorEmbarrassment" | "publicDisgrace" | "notaCensoriaEligible" — reused from Scandal
  dateOriginated,
  originatingRecordId,      // nullable — the Scheme, Event, or Bond that created the underlying fact
  isFabricated: bool,        // §8
  isExposed: bool,          // §6's Expose or §9's Confession both set this true
}

Hook {
  hookId,
  secretId,
  knowerCharacterId,
  strength,               // "strong" | "weak"
  discoveryMethod,          // "activeInvestigation" | "witnessed" | "confessed" | "interceptedCorrespondence" | "traded" | "inherited"
  isSpent: bool,
  createdMonth,
}
```

---

## 12. Open Questions

- **All numeric sizing**, per this project's standing convention — Discovery odds, Hook decay timing, Confession's exact severity discount, and Compel's own success math are all unsized.
- **Whether one Knower can hold two independent Hooks derived from two separate Secrets on the same Holder simultaneously**, and use them in combination. Multiple *different* Knowers each holding their own independent Hook on the same Secret already works cleanly under this model; that isn't the open question.
- **Whether Inheritance (§4) should require the heir to have been explicitly told**, or whether a sufficiently well-documented household — a kept Dossier, a Spymaster's own records — should pass Hooks down automatically as a function of record-keeping rather than a direct conversation.
- **Whether a Fabricated Hook that's never seriously challenged should ever quietly convert to behaving exactly like a real one for game-state purposes**, or should always retain a hidden, permanent flag and some residual discovery risk indefinitely.
- **Whether a turned spy's own disinformation output (Espionage §6)** should be able to manufacture a Fabricated Hook directly as one of its concrete disinformation products — a natural, currently unbuilt tie between the two documents.
- **Whether §4's unconfirmed-Rumor middle state needs its own tracked record**, distinct from both `Secret` and `Hook`, or whether it's better left as a purely narrative/flavor state Graffiti's own existing rumor content already handles without this document needing a third data structure.
