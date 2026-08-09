# GENS — System Design: Epithets, Nicknames & Titles (§6.32, new)
*A genuinely distinct third member of a family this project has already built two of: Traits' own Combo Title system (§6–7 of that document) gives a Character a descriptive personality label that shifts as their traits do; Policies & Edicts' own Hybrid Doctrine naming (§3.4 of that document) gives a whole household a combined-philosophy label the same way. Neither covers what this document does: a real, permanent, earned name — a genuine Roman agnomen, won through a specific real deed, becoming part of how a person is formally known from that point forward rather than a fluid description of who they currently are.*

---

## Contents

1. Scope & Role — The Third Naming System, Not a Competing One
2. Real Roman Agnomina — Conquest, Virtue, and Mockery
3. Earning a Conquest Agnomen
4. Formal Grant vs. Crowd-Given Nickname
5. Inherited Cognomina — When an Epithet Becomes a Family Name
6. Dynastic Epithets — A Whole House's Earned Reputation
7. The Mocking Epithet — When the Name Sticks Anyway
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — The Third Naming System, Not a Competing One

Three real, distinct naming layers now exist, and this document is careful to keep them separate rather than let them blur together:

- **Combo Titles** (Traits §6–7) — a descriptive personality snapshot, generated from a Character's own current trait combination, that can shift as those traits change. "Treacherous Hero," "Venal Magistrate." Who someone *is*, right now.
- **Hybrid Doctrine names** (Policies & Edicts §3.4) — the same descriptive pattern applied to a household's own philosophical combination. "The Old Guard," "Lords of the Wide Roads." What a *house* stands for, right now.
- **Epithets/Agnomina** (this document) — a real, permanent, earned name, won through a specific documented deed, becoming part of how the person is *formally referred to* from that point forward, not a description of their personality or philosophy at all. What someone *did*.

A Character can carry a Combo Title and a real Agnomen at the same time, and they'll often say very different things — a personally Deceitful, Ambitious individual (a real Combo Title) who also happens to carry "Germanicus" (a real, earned military honor) because he genuinely did conquer or pacify Germanic territory, whatever his own private character actually is. This document treats that as a feature, not a contradiction — real Roman history is full of exactly this gap between a person's earned public honors and their own private reputation.

---

## 2. Real Roman Agnomina — Conquest, Virtue, and Mockery

Three real historical categories, all genuinely attested:

- **Conquest agnomina** — a real, extremely well-documented Roman practice: a general or Emperor who conquered or decisively pacified a specific people or territory could be granted an honorific name derived directly from it — *Africanus*, *Germanicus*, *Britannicus*, *Parthicus*, *Dacicus*. This project's own extensive real region and culture roster (every Starting Region document, Cultures of the Known World's own thirty-six-plus entries) gives this practice an unusually rich, ready-made source list — a household that leads or funds a real campaign against Dacia, Parthia, or the Caledonian frontier has a real, concrete, historically accurate honorific waiting on the other side of success.
- **Virtue and achievement agnomina** — real, attested honorific names granted for something other than conquest: *Pius* ("dutiful"), *Felix* ("fortunate"), *Magnus* ("the Great"), *Optimus* ("the best") — each a real historical title, awarded (formally or informally) for a specific, real quality or achievement rather than a battlefield victory specifically.
- **Mocking or unflattering nicknames** — a real, genuinely famous historical case: the emperor popularly known to history by a nickname literally meaning "little boot," originally an affectionate name given as a child accompanying his father's own military campaigns, that stuck for the rest of his life and eclipsed his actual given name in both contemporary and later usage. This document names this real historical pattern directly (§7) as proof that not every lasting epithet is something its bearer would have chosen.

---

## 3. Earning a Conquest Agnomen

A direct, concrete mechanical hook into Military & Combat: a household head, a Companion holding real command, or a son serving in a Roman Service commission who leads or is centrally credited with a real, decisive campaign against a specific named culture or region — a Starting Region's own Timeline Hook resolving in Rome's favor, a Frontier people's Diplomacy relationship (Diplomacy with Non-Roman Peoples) shifting decisively through military success rather than negotiation — becomes eligible for the matching conquest agnomen, named directly for the culture or region actually defeated. This is deliberately rare and tied to a genuinely major, real achievement, not an ordinary Military & Combat engagement — the same "distant, rare-but-reachable goal" pacing this project already applies to the cursus honorum's own top rungs (Politics & Patronage §6).

---

## 4. Formal Grant vs. Crowd-Given Nickname

Two genuinely different sources for the same kind of name, each with its own real texture:

- **A formal grant** — voted or conferred by the Senate or a sufficiently authoritative body (Politics & Patronage's own Curia at a lesser scale, for a locally-significant achievement), carrying real Dignitas weight the moment it's granted, and reading as this document's own positive counterpart to Scandal's own Nota Censoria (Scandal §2, §7) — a formal, recorded, legitimate honor rather than a formal disgrace.
- **A crowd-given nickname** — arising organically from Fame (Games & Spectacle §2, Celebrities & Influential Figures §6.22.1) or a Scandal's own spread (Scandal §5) rather than any formal vote, and genuinely capable of being affectionate, mocking, or both depending on how it's received — real Roman history shows both directions happening to the same category of person. A crowd-given nickname carries real Fame weight but no guaranteed Dignitas benefit, and, per §7, is not always something its bearer can simply decline.

---

## 5. Inherited Cognomina — When an Epithet Becomes a Family Name

A real, genuinely fascinating Roman practice worth building in directly: a personally-earned agnomen could become a real, permanent, **inherited** family cognomen for that person's own descendants — Scipio Africanus's own real historical line continued using "Africanus" as part of their own family name for generations afterward, regardless of whether any individual descendant personally repeated the achievement. This document treats that as a real, available Succession & Dynasty outcome: when a Character carrying an earned Agnomen has an heir, the household can choose to formally adopt that Agnomen as a standing part of the family's own cognomen going forward — a real, permanent Dynasty Chronicle-worthy decision, distinct from an ordinary inheritance, that changes how every subsequent generation is actually named.

---

## 6. Dynastic Epithets — A Whole House's Earned Reputation

Distinct from Policies & Edicts' own Hybrid Doctrine naming (which describes a house's philosophy) and from §5's own inherited-cognomen mechanic (which is a specific name formally adopted into the family's own naming convention): a whole gens can also earn a real, informal, reputation-based epithet across generations that never becomes a formal part of anyone's actual name — "the House That Held the Rhine," "the House the Plague Couldn't Touch" — generated from a sufficiently significant, sustained pattern of Dynasty Chronicle entries rather than any single achievement. This is flavor-tier, read directly off Rival Houses' own standing-trend and Dynasty Chronicle's own accumulated record, and exists purely to give a long-running house's own reputation a real, nameable shorthand the way real historical dynasties often accumulated one informally over time.

---

## 7. The Mocking Epithet — When the Name Sticks Anyway

Per §2's own real historical precedent: a sufficiently severe or widely-spread Scandal (Scandal §6) or a sufficiently distinctive, crowd-noticed trait or incident can generate an unflattering nickname that behaves exactly like an earned Agnomen mechanically — it attaches to the Character's own name, it's visible wherever their name is displayed — without the Character or their household having any real say in it. This document treats this as a genuine, real possibility rather than something only ever chosen deliberately: a Character can actively try to suppress or outlive a mocking nickname (using the same Damage Control tools Scandal §8 already provides — Suppression, Spin, sustained good conduct feeding Rehabilitation), but, consistent with real history's own most famous example, a sufficiently well-established mocking nickname can outlast every attempt to shed it, eventually becoming the name history actually remembers regardless of what the person or their family would have preferred.

---

## 8. Cross-System Integration

- **Traits:** explicitly distinguished from, not merged with, the existing Combo Title system (§6–7 of that document) — §1's own three-way naming taxonomy makes the boundary explicit.
- **Policies & Edicts:** explicitly distinguished from Hybrid Doctrine naming (§3.4 of that document) on the same grounds — philosophy-description versus earned-deed-name.
- **Military & Combat, Diplomacy with Non-Roman Peoples:** §3's conquest agnomen is a direct, concrete reward for a real, decisive campaign outcome in either system.
- **Starting Regions (all documents), Cultures of the Known World:** the entire real region and culture roster is this document's own source list for conquest agnomina — a household that helps close out Britannia's, Dacia's, or Armenia's own Timeline Hooks in Rome's favor has a real, named honorific waiting.
- **Politics & Patronage:** a formal grant (§4) is a real Curia- or Senate-level act, and carries genuine Dignitas weight the way any other political honor does; Rome-wide grants scale with Prominence (Events §5) the same way a Scandal's own Scope does.
- **Games & Spectacle, Celebrities & Influential Figures:** Fame is the direct mechanical source behind a crowd-given nickname (§4).
- **Scandal:** Nota Censoria (Scandal §2, §7) is this document's own direct negative counterpart to a formal grant (§4); §7's mocking epithet is built directly on that document's own Scandal-spread and Damage Control mechanics.
- **Succession & Dynasty:** §5's inherited-cognomen adoption is a new, real, permanent naming-convention decision available at the point of inheritance, distinct from ordinary property/title succession.
- **Rival Houses, Dynasty Chronicle:** §6's dynastic epithet reads directly off both documents' own existing standing-trend and accumulated-record data, adding no new tracked mechanic of its own.
- **Familia:** builds directly on that document's own existing full Roman naming convention (§2.8) — an Agnomen is a real, additional element appended to the tria nomina structure that document already establishes, not a replacement for any part of it.

---

## 9. Data Model

```
Agnomen {
  agnomenId, characterId,
  agnomenType,                      // "conquest" | "virtueOrAchievement" | "crowdGivenNickname" | "mockingNickname"
  name,                              // e.g. "Britannicus", "Felix", "the little boot"
  grantMethod,                        // "formalSenateOrCuriaGrant" | "organicCrowdOrigin"
  sourceCampaignOrRegionId,            // set only for conquest agnomina
  sourceScandalId,                     // set only for a mocking nickname originating from Scandal §6
  dignitasEffect,                       // positive for a formal grant, null/negative for a mocking nickname
  fameEffect,
  isSuppressible: bool,                  // §7 — whether Damage Control tools can realistically remove it
}

InheritedCognomenDecision {            // §5
  originalAgnomenId, decidingHouseholdId,
  adoptedAsPermanentCognomen: bool,
  effectiveFromGeneration,
}

DynasticEpithet {                       // §6 — flavor-tier, no formal naming-convention effect
  householdOrGensId,
  epithetText,
  derivedFromChronicleEntryIds: [ ... ],
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — the Dignitas value of a formal grant, the Fame threshold for a crowd-given nickname to actually form, and how "sufficiently decisive" a campaign needs to be to qualify for §3's conquest agnomen are all unsized.
- **Multiple agnomina.** Whether a single Character can hold more than one earned Agnomen at once (a real, historically plausible case for an especially accomplished figure) isn't addressed.
- **Formal revocation.** Whether a formally granted Agnomen can ever be stripped — the real historical mirror of Nota Censoria, applied to an honor rather than a disgrace — isn't specified.
- **Cross-generational conquest agnomina.** §3 assumes the agnomen is earned by the individual who led the campaign; whether a close relative who funded or materially supported the same campaign has any partial claim to it isn't addressed.
- **The mocking epithet's own permanence threshold.** §7 names real precedent for a nickname outlasting every suppression attempt, but doesn't specify what makes one case permanent and another successfully overcome through Rehabilitation.
