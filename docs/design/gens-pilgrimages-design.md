# GENS — System Design: Pilgrimages (§6.50, an Activity Type built on the Activity Engine and Travel)
*A real, satisfying two-birds resolution: Religions of the Known World's own Open Questions flagged two real, unresolved gaps across two separate design passes — the Oracular tenet's own "exact 'Consult the Oracle' action" and the Initiatory tenet's own "exact cost/ritual shape" — and this document is where both finally get built, as a real, played Activity rather than a passive tenet effect. A Pilgrimage sends a specific Character on a real Journey (reusing Travel's own machinery wholesale) to one of this project's own already-named real holy sites — Delphi, Eleusis, Pessinus, Ephesus — for a real, specific religious purpose, with real, unmitigated risk the whole way there and back.*

---

## Contents

1. Scope & Role — Resolving Religion's Own Two Open Questions
2. The Pilgrimage's Six Slots
3. Purpose — What a Pilgrimage Is Actually For
4. Phases — Departure, the Journey, the Site, the Return
5. The Holy Sites — A Real, Named Registry
6. The Vow (Votum) — A Real Conditional Promise
7. Risk and Reward — Why Not Every Household Does This
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Scope & Role — Resolving Religion's Own Two Open Questions

Across two full design passes, Religions of the Known World named the Oracular and Initiatory tenets as real, mechanically-flagged faith properties without ever building the action either one actually requires: consulting a real oracle, or undergoing a real cult initiation. This document is that action, built once, generically, as a real Activity Type: a **Pilgrimage** — a genuinely elective, often once-per-need journey a specific Character undertakes to a real, named holy site, distinct from a household's own routine local Temple and Lararium worship (Religion §2–3), which remains exactly as designed and handles everything ordinary and recurring.

A Pilgrimage is fundamentally personal rather than a hosted gathering — its "Guest List" (Activity Engine §4) is really a small Travel Party (Travel §2), and its real Venue is wherever the destination actually is, reached the same way any other Journey is.

---

## 2. The Pilgrimage's Six Slots

1. **Host** — the pilgrim Character themselves; the household bears the real cost and risk of their absence rather than "hosting" in the ordinary Activity sense.
2. **Type** — `"pilgrimage"`.
3. **Venue** — the actual destination Holy Site (§5), never a Villa room.
4. **Guest List** — a small Travel Party: bodyguards, a Companion, or family members undertaking the journey together, per Travel §2's own existing party-composition mechanic.
5. **Duration** — always Extended (Activity Engine §3), following Travel's own real Journey-duration math directly for both the outbound and return legs rather than a generic Engine default.
6. **Phases** — §4.

---

## 3. Purpose — What a Pilgrimage Is Actually For

A real, meaningful tag, resolving Religion's own two flagged gaps directly alongside several further real, honest motivations:

- **Consult the Oracle** — the direct resolution of the Oracular tenet's own flagged gap: a real action at Delphi, Dodona, Siwa, or Cumae's own Sibyl (§5), yielding a genuine **Prophecy** — a real, narrative-weighted hint or warning about a specific looming decision or danger, read with unusually high reliability given the site's own real fame, distinct from an ordinary household Auspices reading's more modest accuracy.
- **Mystery Initiation** — the direct resolution of the Initiatory tenet's own flagged "cost/ritual shape": a real, one-time journey to Eleusis or another Initiatory faith's own true site, undergoing the actual rite and permanently marking the Character with that cult's full membership — a real, deeper status than a household's own Household Doctrine merely leaning toward that faith, and the concrete gate on that faith's own full tenet benefits.
- **Healing Pilgrimage** — visiting a real Asclepeion (at Epidaurus, or Pergamon's own real, attested healing sanctuary, distinct from that city's Library) seeking relief from a real, named Chronic Condition or active Illness (Disease & Public Health), with honest, historically appropriate uncertainty — a real chance of genuine improvement, never a guaranteed cure, consistent with this project's own stance on not overstating ancient medicine's real effectiveness.
- **Devotion** — a straightforward Piety-driven visit to a Patron Deity's own most significant real site (a household devoted to Cybele making the real journey to Pessinus), yielding a direct Favor gain scaling with the site's own real prestige and the distance actually traveled.
- **Penance** — a household or Character under real accumulated Ill Omens, a Scandal's own aftermath, or genuine guilt (a Guilt-prone Trait, Characters §4) undertaking a real, humbling journey specifically to seek forgiveness and restored Favor — an honest emotional and religious motivation distinct from the more transactional Purposes above.
- **Vow Fulfillment** — see §6.

---

## 4. Phases — Departure, the Journey, the Site, the Return

- **Departure.** The household sees the pilgrim off — a small, real domestic beat, naturally resolved as a light Group Interaction.
- **The Outbound Journey.** Travel's own existing Journey machinery runs in full and entirely unmodified — Piracy & Banditry, Natural Disasters, chance encounters, all of it. A Pilgrimage receives no special protection simply because its purpose is religious; the road doesn't care why someone is on it, and that honesty is deliberate.
- **At the Site.** The actual Purpose-specific resolution (§3) fires here — a Prophecy delivered, an Initiation undergone, a healing attempted, Favor gained, penance accepted.
- **The Return Journey.** A second, fully real Travel leg, carrying its own independent risk — arriving safely at the oracle is no guarantee of a safe road home.

---

## 5. The Holy Sites — A Real, Named Registry

Consolidating what this project has already established across Religion, Religions of the Known World, and several Starting Regions documents into one direct, usable table:

| Site | Region | Purpose(s) Supported | Real Grounding |
|---|---|---|---|
| **Delphi** | Greek East | Consult the Oracle | The single most famous real oracle in the ancient Mediterranean world |
| **Dodona** | Greek East | Consult the Oracle | A real, genuinely older oracle tradition than Delphi's own |
| **Siwa** | Egypt | Consult the Oracle | A real desert oracle of Ammon |
| **Cumae's Sibyl** | Italian Heartland | Consult the Oracle | A real, domestic oracle option — lower prestige than Delphi, but reachable without leaving Italy at all |
| **Eleusis** | Greek East | Mystery Initiation | The real, single most prestigious mystery cult site of the ancient world |
| **Pessinus** | Anatolia | Devotion | Cybele's real site of origin |
| **Ephesus** | Greek East / Anatolia | Devotion | Home to one of the real Seven Wonders of the ancient world |
| **Epidaurus / Pergamon Asclepeion** | Greek East | Healing Pilgrimage | Real, attested ancient healing sanctuaries dedicated to Asclepius |
| **Jerusalem (the Temple)** | Syria/Levant | Devotion (Judaism, before its real AD 70 destruction) | The real historical center of Judaism until that date |
| **Mount Gerizim** | Syria/Levant | Devotion (Samaritanism) | The real, distinct holy site marking Samaritanism's genuine separation from mainstream Judaism |
| **The Cretan Caves (Dictaean, Idaean)** | Greek East | Devotion | Real, archaeologically-confirmed ancient cult activity tied to Zeus's own mythological birthplace |
| **Aquae Sulis (Bath)** | Britannia | Devotion (syncretic Sulis-Minerva) | The real, specific site of that real syncretic cult |

This registry is deliberately extensible — any future region or faith content can add its own entry without touching this document's own structure.

---

## 6. The Vow (Votum) — A Real Conditional Promise

A real, well-documented ancient religious practice, and a genuinely satisfying mechanic worth building directly: a person facing real danger — a Storm at sea (Private Ships & Shipping Ventures), a battlefield crisis (Military & Combat), a severe Illness (Disease & Public Health) — could make a real, binding **votum**: a formal promise to a god that, if spared or granted their request, they would undertake a specific pilgrimage or dedication in thanks.

### 6.1 Making a Vow

During a genuinely qualifying crisis, a Character can make a real Vow naming a specific destination Holy Site (§5) as the promised pilgrimage.

### 6.2 The Real Stakes of a Broken Vow

Once the crisis has passed — the storm survived, the illness recovered from, the battle won — the Vow becomes a real, standing obligation. Failing to fulfill it is a genuine, serious religious transgression: a guaranteed Ill Omen and a real, standing Favor penalty that persists (distinct from ordinary Favor decay) until the Pilgrimage is actually undertaken, mirroring the real ancient anxiety around broken vows directly rather than treating the promise as a mere flavor prompt.

### 6.3 Fulfillment

Undertaking the named Pilgrimage closes the obligation, and does so with a real, above-and-beyond Favor reward on top of whatever the underlying Purpose (§3) already grants — a distinct, additional payoff specifically for having kept the promise, giving a Vow real emotional and mechanical weight beyond an ordinary transactional visit.

---

## 7. Risk and Reward — Why Not Every Household Does This

A Pilgrimage is genuinely costly — real Travel time, real money, and real, unmitigated risk (§4) — and genuinely optional: most households, most of the time, rely entirely on their own local Temple and Lararium and never undertake one at all. This document is deliberately built so a Pilgrimage reads as a real, occasional, meaningful choice rather than a routine action, consistent with Design Pillar #1: the Oracle's Prophecy is valuable, but the road there is dangerous; a Mystery Initiation's full benefits are real and permanent, but costly and irreversible to obtain; a Healing Pilgrimage might simply not work. None of this is a dominant strategy — it's a real, weighed bet a household makes when the stakes feel worth the risk.

---

## 8. Cross-System Integration

- **Religions of the Known World:** this document directly resolves both the Oracular tenet's "Consult the Oracle" gap and the Initiatory tenet's "cost/ritual shape" gap, both explicitly flagged as unresolved across that document's own two passes; §5's registry is drawn directly from that document's own §10.
- **Religion:** Devotion and Penance Purposes both feed the household's existing Favor track directly; the Vow (§6) is a new, real extension of that document's own Omens machinery.
- **Travel:** the Journey mechanic (§4) is reused wholesale and unmodified for both legs of a Pilgrimage.
- **Activity Engine:** this document is a real, slightly atypical Activity Type — personal rather than hosted, its Guest List functioning as a Travel Party — demonstrating the Engine's own flexibility beyond a purely social gathering.
- **Disease & Public Health:** the Healing Pilgrimage Purpose is a real, honest, uncertain alternative or supplement to ordinary Court Physician treatment.
- **Piracy & Banditry / Natural Disasters:** both apply in full and unmitigated to a Pilgrimage's own Journey Phases.
- **Private Ships & Shipping Ventures:** a sea-voyage Pilgrimage to a site like Ephesus or Isis's own cult centers naturally uses a household's own Ship where one is available.
- **Characters:** the Guilt-prone Trait is a direct, natural driver of the Penance Purpose.
- **Scandal:** Penance is a real, available response to an existing Scandal's own aftermath.
- **Starting Regions:** Greek East's own dense sacred geography, Italian Heartland's Cumae, Egypt's Siwa, and Britannia's Aquae Sulis all become concrete, reachable Pilgrimage destinations rather than static regional flavor.
- **Dynasty Chronicle:** a fulfilled Vow, a successful Mystery Initiation, or a genuinely dramatic Oracle consultation are all natural, weighty entries.
- **Companions & Court Positions:** a household's own Sacerdos Domesticus, or a hired itinerant Haruspex, is a natural, optional addition to a Pilgrimage's own Travel Party.

---

## 9. Data Model

```
Pilgrimage extends Activity {          // §6.47's Activity, type = "pilgrimage", durationMode = "extended"
  pilgrimCharacterId,
  purpose,                    // "consultOracle" | "mysteryInitiation" | "healingPilgrimage" |
                               // "devotion" | "penance" | "vowFulfillment"
  destinationSiteId,             // §5
  vowRecordId,                  // nullable — set only for vowFulfillment
  travelPartyIds: [ ... ],
  outboundJourneyRef, returnJourneyRef,   // both reuse Travel's own Journey record directly
  outcomeResult: {
    prophecyText,               // nullable
    initiationGranted: bool,       // nullable
    healingOutcome,              // nullable — "improved" | "noChange"
    favorGained,
  },
}

VowRecord {                     // §6
  vowId, characterId,
  madeDuringCrisisType,           // "storm" | "battle" | "illness"
  promisedDestinationSiteId,
  fulfilled: bool,
  monthsOutstanding,
}

HolySite {                      // §5
  siteId, name, regionId,
  associatedFaith,
  supportedPurposes: [ ... ],
}
```

---

## 10. Open Questions

- **All numeric sizing**, per convention — Journey risk/cost for each named site, Favor gain magnitude by Purpose and site prestige, and the broken-Vow penalty's own severity are all unsized.
- **Prophecy's own honesty.** Whether a Consult the Oracle Prophecy should always be literally true but deliberately ambiguously phrased (the real, classic ancient oracle-story pattern) or occasionally genuinely wrong isn't resolved — the former reads as more thematically faithful to how real oracle narratives actually worked, but isn't mandated here.
- **Joint Pilgrimages.** Whether multiple Characters (a married couple, several family members) can undertake one shared Pilgrimage as a single Activity with a shared outcome, versus always being tracked as individual journeys, isn't addressed.
- **Foreign-territory access.** A Holy Site sitting inside a foreign polity's own borders (rather than Roman-controlled Greek East or Egypt) would need a real, honest interaction with Diplomacy with Non-Roman Peoples' own access and safe-passage mechanics — not addressed here, and worth resolving if a future Holy Site is ever added outside Roman control.
