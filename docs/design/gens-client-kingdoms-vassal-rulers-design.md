# GENS — System Design: Client Kingdoms & Vassal Rulers
*The generalized, reusable engine sitting underneath four Starting Regions that have each already, individually, built a piece of this without ever naming the shared pattern: the Bosporan Kingdom's own permanent, stable Client relationship, Armenia's own Great Power Allegiance and its Alliance Against Rome mechanic, and Nubia's and Arabia Felix's own Independent Kingdom household shape. This document is what those four documents were all quietly reaching for — Investiture, Tribute & Fealty, hostage-taking (*obsides*), succession crisis, conversion to full Province, and breaking away — built once, properly, and handed back to every region (existing or future) that needs it.*

---

## Contents

1. Scope & Role — Naming the Pattern Four Documents Already Half-Built
2. The Client Relationship — Tribute & Fealty, Distinct from Reputation Duality
3. Investiture — Rome Makes a King
4. Tribute, Auxiliary Levies & the Player as Patron
5. Obsides — Hostages as Statecraft
6. Succession Crisis — Backing a Claimant
7. Playing as the Ruling House
8. Conversion to Province
9. Breaking Away
10. Cross-System Integration
11. Data Model
12. Open Questions

---

## 1. Scope & Role — Naming the Pattern Four Documents Already Half-Built

Four Starting Regions have each, independently and for entirely good reasons, built a real piece of what a "client kingdom" actually is without ever formalizing the shared shape underneath: the Bosporan Kingdom is a real, permanently stable Client culture that never converts to full annexation across this game's entire range; Armenia is a Contested Buffer whose throne Rome and Parthia both spent centuries installing and deposing client kings on, tracked through the existing `ArmenianAllegiance` field and a full `AllianceAgainstRome` mechanic; Nubia and Arabia Felix are both real Independent Kingdoms where the default household isn't Roman at all. Diplomacy with Non-Roman Peoples already supplies real pieces this document reuses wholesale — the Legate posting, Peace Treaty and Hostage Exchange mechanics, and Armenia's own `AllianceAgainstRome` record.

This document's actual job is small and specific: **name the pattern once**, build its missing pieces (Investiture, a generalized Tribute & Fealty axis, a reusable Succession Crisis, and formal Conversion-to-Province mechanics), and hand the whole thing back so that any future region — or any of this project's own Diplomacy-tracked Frontier Peoples that never gets a bespoke Starting Region document at all — can use it directly rather than reinventing it region by region.

**What doesn't move here:** the Bosporan Kingdom's own stable-client flavor, Armenia's own Great Power Allegiance texture, and Nubia/Arabia Felix's own Independent Kingdom identity all stay exactly as their own documents wrote them. This document supplies the reusable engine; it doesn't touch a word of their own regional color.

---

## 2. The Client Relationship — Tribute & Fealty, Distinct from Reputation Duality

A genuinely important distinction worth drawing before anything else: **Reputation Duality** (Politics & Patronage §2.1) measures how a Roman citizen population feels about Rome's own administration of them. It has never been the right tool for measuring how a *foreign ruler* feels about their own obligations to Rome — the Bosporan Kingdom's own document already recognized this by reaching an honest "None" for Reputation Duality while its real, live relationship (the Sarmatian/Scythian frontier) sat entirely in Diplomacy's own Frontier toolkit instead. This document names that missing axis directly: **Tribute & Fealty**, a standing, single-track relationship score between a Client Kingdom's own ruling house and Rome, independent of and never to be confused with Reputation Duality.

| Tier | What It Means |
|---|---|
| **Loyal Client** | Tribute paid in full and on time, auxiliary levies honored, no independent foreign policy pursued without Roman knowledge — the Bosporan Kingdom's own real, stable default for its entire 368-year span |
| **Reluctant Client** | Obligations technically met, but resentfully, slowly, or at reduced rates — a real, live pressure state rather than a crisis |
| **Defiant** | Tribute withheld, an unsanctioned foreign contact pursued, or a succession resolved without Roman input — the direct precursor to §9's own Breaking Away |
| **Broken Away** | Open rebellion or a formal repudiation of the relationship — resolves through §9 |

Tribute & Fealty moves in response to §4's own tribute-rate choices, §6's own succession outcomes, and simple sustained neglect — a client relationship left entirely unattended drifts downward over time, the same "a policy is a standing choice, not a set-and-forget" logic this project already applies everywhere else.

---

## 3. Investiture — Rome Makes a King

A real, historically vivid practice this project already has one spectacular named example of: Armenia's own document names Nero personally crowning the Armenian king Tiridates in Rome as "a real, famous, extravagantly documented diplomatic spectacle... the single clearest real illustration of Great Power Allegiance in miniature." This document generalizes that specific, singular example into a real, repeatable mechanic available to any Client Kingdom.

**Investiture** is the formal act by which Rome recognizes a new or disputed ruler's legitimacy — sometimes a real journey to Rome and a personal ceremony (Tiridates's own model), more often a simpler formal recognition delivered through a Legate or Governor (Politics & Patronage §7). A Roman Character who personally sponsors or presides over an Investiture — most naturally a Legate holding the relevant posting (Diplomacy with Non-Roman Peoples §8.1), or a sufficiently senior Provincial Governor — earns real, direct Dignitas and Influence for having done so, the same "sponsor and be rewarded for it" shape Politics & Patronage's own cursus honorum sponsor mechanic already establishes. A newly-invested ruler starts their own reign at a real, meaningful Tribute & Fealty bonus specifically toward whichever Roman figure actually sponsored them — a genuine, personal debt of gratitude distinct from the kingdom's own baseline relationship to Rome as an institution.

---

## 4. Tribute, Auxiliary Levies & the Player as Patron

The concrete, recurring content of the Client relationship, giving Tribute & Fealty (§2) something real to actually respond to:

- **Tribute** is a real, standing income stream — reusing Economy & Finance's own Rent/Tax Revenue precedent directly rather than inventing a parallel structure — flowing from the Client Kingdom to whichever Roman authority (a Provincial Governor, or the player's own household if it holds that authority) actually administers the relationship. The **rate** is a real, standing player choice: a generous rate sustains or improves Tribute & Fealty at the cost of reduced revenue; a harsh one extracts more in the short term at real, accumulating risk to the relationship, the same no-dominant-strategy shape this project's every other extraction lever already uses.
- **Auxiliary levies** are Military & Combat's own real, already-established auxiliary recruitment practice (the Batavian real historical precedent, Cultures §3), formalized here as a standing obligation a Client Kingdom can be called on to honor — a real, mutual arrangement where the client supplies real troops for a Roman campaign in exchange for continued protection and, often, eventual citizenship for those who serve (Military & Combat §3.4's own Discharge/Assimilation loop).
- **A Roman Patron's own standing.** A Character who personally administers a specific Client Kingdom's relationship — through a Legate posting, a Provincial Governor's own extended authority (Politics & Patronage §7), or simply sustained personal Clientela ties to its ruling house — accrues real Dignitas and Influence from a well-managed, Loyal-tier relationship, and real risk exposure from a Defiant or Broken one under their own watch.

---

## 5. Obsides — Hostages as Statecraft

A real, specific Latin term worth using directly: **obsides** — hostages, typically a client ruler's own children or close relatives, held not as prisoners in any punitive sense but as a real, mutually understood guarantee of continued loyalty. Diplomacy with Non-Roman Peoples already tracks two adjacent but distinct shapes of this — a coercive `FrontierHostageRecord` and a more mutual Parthian `hostageRecord` — and this document names the Client Kingdom's own version as a real, third, and genuinely rich variant: **the hostage is very often raised inside a Roman household, absorbing Roman education, values, and culture, before eventually returning home to rule.**

This is a real, historically well-documented practice, and this document gives it a concrete, satisfying mechanical and narrative payoff rather than leaving it as a flat, functional guarantee:

- A young obses raised in a Roman household — potentially the player's own, if their household holds the relevant Legate posting or sufficient standing — is a real Character, developing normally through Familia's own lifecycle, and a strong, natural candidate to develop the **Assimilated** trait (Fashion & Dress §10, Traits §6.6) given years of genuine immersion rather than a superficial dress choice.
- **When that obses eventually returns home to rule** (typically triggered by §6's own Succession Crisis), a strongly Assimilated former hostage makes a real, historically apt, genuinely more pliable and pro-Roman client ruler than one raised entirely at their own native court — a direct, mechanical payoff for the entire practice, and a real, satisfying long-arc narrative a patient player can watch pay off across a full generation.
- **The risk, played honestly:** an obses who instead develops Unbowed, or who returns home Resentful or Estranged from real mistreatment during their own hostage years, makes an equally real, historically plausible, and considerably more dangerous ruler once invested — this document doesn't guarantee the practice always works in Rome's favor, consistent with how every other long-horizon investment in this project carries genuine risk alongside its upside.

---

## 6. Succession Crisis — Backing a Claimant

A generalized, reusable version of the mechanic Armenia's own document built specifically for itself. When a Client Kingdom's ruler dies, is deposed, or (per §5) an obses comes of age and returns to press a claim, a **Succession Crisis** fires: one or more claimants generate as real Characters via Characters' own lazy instantiation, each carrying their own real Traits, Tribute & Fealty leanings, and, where relevant, their own §5 hostage history.

- **An ordinary Client Kingdom** (one bordering only Rome's own sphere, without a rival Great Power in play) resolves its Succession Crisis primarily as a domestic contest between claimants, with Rome — the player, if they hold the relevant standing — able to back a preferred candidate through §3's Investiture mechanism, diplomatic pressure, or, at the harsher end, direct Military & Combat support for their chosen claimant's own succession bid.
- **A Client Kingdom bordering a second Great Power** (Armenia's own specific, named case, and the template for any future region built the same way) escalates the same Succession Crisis into a real contest between outside patrons rather than a purely domestic one — this document doesn't redesign Armenia's own Great Power Allegiance mechanic, it simply confirms that mechanic *is* this document's own Succession Crisis, run at its most contested possible setting, exactly the relationship the Armenia document's own §6 already implies but never states in these general terms.

A resolved Succession Crisis sets the new ruler's own starting Tribute & Fealty tier per §3's Investiture rules, and is a genuine, real Dynasty Chronicle-eligible moment for any Roman house that backed the winning — or, more dramatically, the losing — claimant.

---

## 7. Playing as the Ruling House

The formal, generalized version of what Nubia, Arabia Felix, Armenia's own noble-house option, and the Bosporan Kingdom have each already established individually: a player's own household can *be* a Client Kingdom's own ruling dynasty rather than a Roman family managing one from outside.

- **Legal Status runs on the kingdom's own local system**, not the ordinary Roman Legal Status ladder (Familia §2.5) — consistent with how Nubia and Arabia Felix are already described as regions "where the default household is not Roman."
- **Succession is the kingdom's own succession** — Succession & Dynasty's own existing mechanics apply directly to the throne itself, not merely to household inheritance, the highest possible stakes that document's own framework already supports.
- **Tribute & Fealty (§2) becomes the household's own single most important standing relationship** — a ruling house playing this way experiences §4's own tribute-rate tension from the paying side rather than the collecting one, and §9's Breaking Away as a genuine, deliberately pursuable playthrough arc rather than merely a threat to be managed.
- **An heir sent as an obses (§5)** is this household's own single hardest, highest-stakes parenting decision — a real, felt tradeoff between the immediate safety and continued goodwill a hostage arrangement buys and the real, uncertain question of who that child will have become by the time they come home to rule.

---

## 8. Conversion to Province

The real, historically common endpoint several Client Kingdoms on this project's own roster explicitly avoid (the Bosporan Kingdom, permanently; Armenia, all but once) and several real ones didn't: Judaea (AD 6), Egypt (30 BC), Cappadocia (AD 17), Thrace (AD 46), Mauretania (AD 44), Nabataea (AD 106) — all real, all dateable, and all following one of three genuine historical patterns this document formalizes as real trigger conditions:

- **A ruler dies without an heir Rome trusts** — the single most common real historical annexation trigger, and a direct, natural consequence of an unresolved or badly-resolved Succession Crisis (§6).
- **A deliberate Roman political decision** — Rome simply choosing, for its own administrative or strategic reasons, to absorb a Client Kingdom outright regardless of its current ruler's own standing or loyalty.
- **A punitive annexation following Broken Away (§9)** — a rebellion crushed rather than negotiated ends, often, in real Roman practice, with the kingdom's own independent existence ended outright rather than merely restored to its prior Client status.

**Mechanically**, Conversion to Province replaces the kingdom's own Tribute & Fealty relationship (§2) with an ordinary Reputation Duality reading (Politics & Patronage §2.1) for the first time, installs a genuine Provincial Governor (§7 of that document) in place of the former ruling house, and converts the local population's own Legal Status onto this project's ordinary ladder. **The former ruling house's own fate** is a real, honest, and often difficult question this document doesn't soften: absorption into local Equestrian-tier elite status under the new provincial order is the more fortunate real historical outcome; exile, imprisonment, or execution is the harsher one, and both are legitimate, available resolutions depending on how the conversion actually came about.

---

## 9. Breaking Away

The direct, generalized reuse of Diplomacy with Non-Roman Peoples' own already-built `AllianceAgainstRome` mechanic — this document adds no new mechanic here at all, it simply confirms that record's own existing stage list (`secretNegotiation → openDeclaration → warUnderway → victoriousAutonomy / victoriousCleanBreak → crushed`) is the correct, reusable resolution path for *any* Client Kingdom's own Tribute & Fealty collapsing all the way to Broken Away (§2), not solely Armenia's own Great-Power-contested case.

**The genuine difference from Servile Wars' own Collective Resistance framing** (that document's own Regional Revolt and Servile War tiers) is worth stating plainly: a Client Kingdom breaking away is a sovereign or semi-sovereign ruler's own political and military act, resolved through Military & Combat's Combat Resolution Engine at full Force-and-Campaign scale against a real royal army rather than an improvised Irregular Combatant Revolt Force — the Bosporan Kingdom, Judaea, and Armenia's own repeated real allegiance shifts all fought (or credibly threatened to fight) as genuine states, not as an uprising of the enslaved. The two systems can, in principle, compound — a Client Kingdom in the middle of Breaking Away is exactly the kind of regionally destabilized environment where Servile Wars' own Regional Unrest (§3 of that document) might independently spike — but they remain two distinct real historical phenomena, and this document keeps them mechanically separate rather than merging them into one.

---

## 10. Cross-System Integration

- **Diplomacy with Non-Roman Peoples:** the Legate posting, Peace Treaty and Hostage Exchange mechanics, and the entire `AllianceAgainstRome` record are all reused wholesale, not redesigned; §9 is a direct, explicit confirmation that record generalizes beyond Armenia.
- **Starting Regions — Bosporan Kingdom, Armenia, Nubia, Arabia Felix:** this document is the shared engine underneath all four; none of their own regional flavor, terrain, or population content moves or changes.
- **Politics & Patronage:** §3's Investiture reuses the cursus honorum's own sponsor-and-be-rewarded shape directly; §8's Conversion to Province installs an ordinary Provincial Governor and activates Reputation Duality for the first time in that region.
- **Economy & Finance:** §4's Tribute reuses the Rent/Tax Revenue precedent directly as its income model.
- **Military & Combat:** §4's auxiliary levies reuse that document's own auxiliary recruitment and Discharge/Assimilation loop; §9's Breaking Away resolves through the full Combat Resolution Engine at Force/Campaign scale.
- **Familia, Traits:** §5's obsides mechanic is a real, direct payoff for the Assimilated/Unbowed trait pair, and a genuine Familia lifecycle arc if the hostage is fostered within the player's own household.
- **Characters:** §6's Succession Crisis claimants generate through lazy instantiation exactly like any other Character this project generates on demand.
- **Succession & Dynasty:** §7 applies that document's own mechanics directly to a throne rather than only to household inheritance, for any household actually playing as a Client Kingdom's own ruling house.
- **Servile Wars, Slave Revolts & Collective Resistance:** §9 draws the explicit, necessary line between a sovereign ruler's own rebellion and Regional Unrest's own enslaved-population uprising, while noting the two can realistically compound.
- **Dynasty Chronicle:** an Investiture, a resolved Succession Crisis, a Conversion to Province, and any stage of Breaking Away are all guaranteed, high-tier Chronicle material.

---

## 11. Data Model

```
ClientKingdomRelation {                  // §2 — the core new record this document adds
  clientKingdomActorId,                    // the ruling house/kingdom, a Living World Actor (Rival Houses' own schema)
  tributeAndFealtyTier,                     // "loyalClient" | "reluctantClient" | "defiant" | "brokenAway"
  administeringRomanCharacterId,             // nullable — the Legate/Governor/Patron currently responsible
  tributeRate,                               // player-set, standing
  lastInvestitureId,
}

InvestitureRecord {                       // §3
  investitureId,
  clientKingdomActorId,
  investedRulerCharacterId,
  sponsoringRomanCharacterId,
  ceremonyType,                              // "personalCeremonyInRome" | "provincialRecognition"
  month,
}

ObsesRecord {                             // §5 — extends Diplomacy's own hostage-record family
  hostageCharacterId,
  sourceClientKingdomActorId,
  fosteredInHouseholdId,                     // nullable — the player's own household, if applicable
  startedAtMonth, returnedAtMonth,
  assimilationTraitOutcome,                    // nullable, resolved at return — "assimilated" | "unbowed" | "neutral"
}

SuccessionCrisisRecord {                  // §6
  crisisId,
  clientKingdomActorId,
  claimantCharacterIds: [ ... ],
  isGreatPowerContested: bool,                 // true for an Armenia-style case
  backedByRomanCharacterId,                     // nullable
  backedByRivalGreatPowerId,                     // nullable, Great-Power-contested only
  resolvedClaimantId,
  resultingInvestitureId,
}

ProvinceConversionRecord {                // §8
  clientKingdomActorId,
  triggerType,                               // "heirlessDeath" | "deliberatePolicy" | "punitiveAfterRebellion"
  formerRulingHouseFate,                       // "absorbedAsEquestrian" | "exiled" | "imprisonedOrExecuted"
  newProvincialGovernorCharacterId,
  month,
}

// AllianceAgainstRome — reused directly from Diplomacy with Non-Roman Peoples §13, no new fields required
```

---

## 12. Open Questions

- **All numeric sizing**, per this project's standing convention — Tribute & Fealty's own tier thresholds, tribute-rate effect magnitudes, and Investiture's own Dignitas/Influence reward are all unsized.
- **Whether every Client Kingdom needs its own bespoke Starting Region document, or whether a "generic Client Kingdom" can exist purely through Diplomacy's own Frontier Peoples list plus this document's mechanics, with no dedicated region document at all.** This document is written to support both, but doesn't resolve which the project actually wants going forward.
- **Multiple simultaneous obsides.** §5 doesn't specify whether a Client Kingdom can have more than one hostage arrangement active at once (with Rome and, in a Great-Power-contested case, with the rival power as well) or whether the two are mutually exclusive.
- **The exact mechanical weight of an Assimilated-versus-Unbowed returning obses** (§5) on their own subsequent Tribute & Fealty performance once they rule — flagged as a real, intended effect without a sized formula.
- **Whether Conversion to Province (§8) can ever be reversed** — whether a converted province could, in principle, revert to Client status under a later, different Roman policy, or whether this document treats the conversion as permanent for the remainder of any given playthrough.
