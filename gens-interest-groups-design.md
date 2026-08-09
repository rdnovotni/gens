# GENS — System Design: Interest Groups (§6.38, new)
*A genuine third organizing structure, distinct from the two this project already has: Faction (Politics & Patronage §3.1) is a broad, permanent ideological label; Collegia (Collegia & Guilds) is a permanent, trade-based organization. Neither describes what actually forms when a specific policy question — a proposed Land Redistribution Edict, a debt-cancellation Tabulae Novae — creates a real, temporary coalition cutting straight across both: a Traditionalist landowner and a Popularist one can sit on the same side of a land question despite disagreeing about everything else, and a household can belong to a trade Collegium and an entirely unrelated Interest Group at the same time. This document is that missing structure, built around one particularly rich real Roman institution this project hasn't used yet: a province's own formal, designated Senate patronus.*

---

## Contents

1. Scope & Role — A Third Organizing Structure
2. Real Historical Interest Groups
3. Provincial Patronage — A Real, Formal Institution
4. Formation and Membership
5. Interest Group Actions
6. Persistence and Dissolution
7. Cross-System Integration
8. Data Model
9. Open Questions

---

## 1. Scope & Role — A Third Organizing Structure

Faction answers "what does this household broadly believe." Collegia answers "what trade does this household practice, and who shares it." Neither answers a real, common political question: "who actually benefits or loses from this specific proposal, regardless of ideology or trade." A landless veteran and a landless urban laborer might both support a Land Redistribution Edict for entirely different reasons, while a Traditionalist old landowner and a Popularist newly-wealthy one might both oppose it for the same one — real interest, not shared belief or shared craft, is what actually organizes them. This document names that structure directly: the **Interest Group**, a real, often temporary coalition formed around one specific shared material stake, capable of pulling members from across every Faction and every Collegium at once.

---

## 2. Real Historical Interest Groups

Five real, historically grounded coalition types, each tied directly to a mechanic this project already has:

- **Landowners vs. the Landless** — the real, defining conflict this project's own Events timeline opens on, directly opposing Policies & Edicts' own Land Redistribution Edict (§5.4 of that document).
- **Creditors vs. Debtors** — the real coalition directly opposing or supporting a Tabulae Novae debt-cancellation Edict (§5.2 of that document), a household's own membership determined by whether it currently holds active DebtRecords as creditor or debtor.
- **Publicani and Equestrian Trade Interests** — a real, historically documented lobbying bloc: Roman tax-farming and trade interests genuinely organized collectively to push for favorable provincial administration policy, directly tied to Land Ownership & Real Estate's own Publicanus Contract (§8 of that document) and Merchant Families & the Equestrian Order's own equestrian tension (§5 of that document).
- **Veterans** — real, politically significant once settled: veteran soldiers awaiting or having received land grants (Settlement Demographics' own Veterans loop, Military & Combat) formed a real, historically attested political bloc with a genuine, shared material stake in land policy specifically.
- **Provincial Interests** — households and Notable Households from a tapering or localized Reputation Duality region (Iberian Colony, North African Colony, Syria/The Levant, The Balkans) sharing a real, common stake in how Rome treats their own home region, given formal shape in §3 below.

---

## 3. Provincial Patronage — A Real, Formal Institution

The single richest real historical mechanic this document introduces: a province, or a specific region's own population, could formally designate a Roman senator as its own **patronus** — a real, genuine institution, distinct from an ordinary individual Clientela relationship (Politics & Patronage §4), representing an entire region's own collective interest in the Senate. A household holding this role gains real, substantial Dignitas and a genuine, standing influence over any Edict or policy question touching that specific region directly, and, in turn, carries a real, felt obligation: a patronus who fails to defend their own province's interests — voting for an Aggressive Publicanus Contract renewal against their own client region, say — suffers a real, serious relationship-web and Dignitas cost with that region's own population, a genuine, higher-stakes version of any individual patron's own ordinary Clientela obligations. This ties directly into the Reputation Duality mechanic itself: a skilled, genuinely committed provincial patronus is a real, concrete lever a player can use to improve a tapering or localized region's own local-standing trajectory from Rome's own side of the relationship, distinct from anything the region's own local administration can do on its own.

---

## 4. Formation and Membership

An Interest Group forms the moment a real, relevant policy question becomes live — an Edict proposal, a contested Curia election with a clear material stake, a Publicanus Contract renewal — and dissolves once that question resolves, unless §6's own persistence conditions apply. Membership is read directly from existing household data rather than requiring a separate join action for most members: a household's own DebtRecords, land holdings, veteran status, or region of origin automatically determine which Interest Groups it belongs to for the duration of a live question, the same "derived, not separately tracked" principle this project already applies to ambient Notable Household and Wanderer population. A household can belong to multiple Interest Groups simultaneously, and, per §1, its own memberships frequently cut directly across its Faction and Collegium affiliations rather than aligning neatly with either.

---

## 5. Interest Group Actions

Two real, concrete actions, both extending existing mechanics rather than inventing parallel ones:

- **Collective Lobbying** — the group-scale version of Notable Businesses' own individual Lobby Government action (§8 of that document) and Collegia & Guilds' own endorsement mechanic (§6 of that document), pooling Influence from every participating household to move a live Edict's own Reception (Policies & Edicts §5.1) more sharply than any single household's own lobbying could.
- **A Curia Faction Bloc** — Characters' own existing "Found/Join a Curia Faction Bloc" Interaction (§9.3 of that document), now given a real, concrete Interest-Group-shaped membership rather than a purely Faction-aligned one, letting a contested election's own bloc voting reflect real material interest rather than only broad ideology.

---

## 6. Persistence and Dissolution

Most Interest Groups are genuinely temporary, exactly matching their own real historical shape: Landowners-vs-Landless and Creditors-vs-Debtors dissolve the moment their triggering Edict actually resolves, win or lose, since the specific material question that created them is now settled. Publicani/Equestrian interests and Veterans, by contrast, are real, quasi-permanent standing coalitions — their own underlying material stake (provincial tax administration, land-grant policy generally) never fully resolves the way a single Edict vote does, so this document treats both as persistent background Interest Groups a household's own status continuously determines membership in, rather than a one-off coalition tied to a single event.

---

## 7. Cross-System Integration

- **Politics & Patronage:** §1 explicitly distinguishes Interest Groups from that document's own Faction axis (§3.1); §3's Provincial Patronage extends that document's own individual Clientela concept to a real, region-scale relationship.
- **Policies & Edicts:** §2's own five interest groups are directly opposed or aligned around that document's own Land Redistribution and Tabulae Novae Edicts (§5.2, §5.4).
- **Collegia & Guilds, Notable Businesses:** §5's collective lobbying is the group-scale version of both documents' own existing endorsement and lobbying actions.
- **Land Ownership & Real Estate, Merchant Families & the Equestrian Order:** the Publicani/Equestrian interest group (§2) ties directly to both documents' own existing content.
- **Settlement Demographics, Military & Combat:** the Veterans interest group (§2) reads directly from that document's own existing Veterans loop.
- **Starting Regions (all tapering/localized documents):** §3's Provincial Patronage gives every such region a real, concrete, Rome-side lever affecting its own Reputation Duality trajectory.
- **Characters:** §5's Curia Faction Bloc reuses that document's own existing Interaction directly.
- **Economy & Finance:** Creditor/Debtor membership (§2) is read directly from that document's existing DebtRecord data.

---

## 8. Data Model

```
InterestGroup {
  interestGroupId, groupType,             // "landownersVsLandless" | "creditorsVsDebtors" | "publicaniEquestrian" |
                                             // "veterans" | "provincialInterest"
  isPersistent: bool,                       // true for publicaniEquestrian and veterans — §6
  linkedEdictId,                             // nullable — set for a temporary, Edict-triggered group
  linkedRegionId,                             // nullable — set only for provincialInterest
  memberHouseholdIds: [ ... ],                // derived, not separately joined, per §4
}

ProvincialPatronage {                        // §3
  patronusCharacterOrHouseholdId, regionId,
  dignitasGained,
  obligationFulfilled: bool,
  reputationDualityInfluence,                  // the real lever this role gives over that region's own trajectory
}

CollectiveLobbyingAction {                    // §5
  actionId, interestGroupId, targetEdictOrElectionId,
  pooledInfluence,
  receptionShift,
}
```

---

## 9. Open Questions

- **All numeric sizing**, per this project's standing convention — pooled lobbying's own effectiveness multiplier and Provincial Patronage's own Dignitas value are unsized.
- **Whether a household can formally decline membership** in an Interest Group its own material circumstances would otherwise place it in — this document treats membership as automatically derived, but a household actively choosing not to act on a shared interest it technically holds isn't addressed.
- **Multiple patroni for one region.** §3 doesn't specify whether more than one household can hold Provincial Patronage over the same region simultaneously, or whether it's necessarily exclusive.
- **Interest Group conflict with Faction loyalty.** §1 and §4 establish that memberships cut across Faction lines, but don't specify how a household should weigh a direct conflict between its own Faction identity and its own Interest Group's material stake on a specific vote.
- **New Interest Group types beyond the five named here.** §2 treats this list as a strong starting set rather than exhaustive — whether future policy content (a new Edict type, a new region category) should spawn its own new Interest Group is left open.
