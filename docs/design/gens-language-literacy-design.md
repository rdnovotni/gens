# GENS — System Design: Language, Literacy & Language Proficiency (§6.34, FINAL)
*Final pass. A CK3-style language layer, per direction, built on real historical linguistic geography rather than an invented abstraction: every region this project has built already has a real, distinct dominant language or language family, and every one of them already has a Population & Culture Distribution table this document reads directly rather than duplicating. Literacy stays primarily a derived fact — most Characters don't need a tracked number — but becomes a real, explicit stat for named individuals where it actually matters, including one honest historical inversion worth building in directly: a household's own educated Greek slave or freedman scribe was often genuinely more literate than the Roman citizen who owned them. The prior expansion pass corrected three gaps (Meroitic, Basque/Aquitanian, and the split of Gaulish/Brythonic/Goidelic Celtic); this final pass catches a fourth, more significant one — Oscan, a genuine distinct Italic sister-language to Latin, directly tied to the substantial real Oscan/Samnite cultural content the Italian Heartland document already built around Pompeii, Nola, and Nuceria, which had gone entirely unlanguaged until now — plus Noric's own Celtic-family placement and a brief, honest footnote for Phrygian, Umbrian, Ligurian, and Venetic as real but untracked early remnants.*

---

## Contents

1. Scope & Role
2. The Language Map — Real Linguistic Geography
3. Literacy — Derived by Default, Tracked Where It Matters
4. Fluency Tiers
5. Learning a Language
6. The Language Barrier — Soft Penalty, Hard Gate
7. The Interpres — A New Companion Role
8. Multilingual Elites — No Hard Cap
9. Cross-System Integration
10. Data Model
11. Open Questions

---

## 1. Scope & Role

This document adds two related but distinct mechanics this project has never formalized: **Literacy** (can a Character read and write at all) and **Language Proficiency** (which spoken languages a Character actually knows, and how well). Neither is a new standalone system competing with anything already built — both read directly from Education & Culture's own Learning investment, Familia's own Legal Status and origin-culture fields, and, most importantly, every Starting Region document's own Population & Culture Distribution table, which already names exactly which real cultures populate exactly which places. This document's actual job is mapping those existing cultures onto real historical languages and giving the resulting gaps between them genuine mechanical weight.

---

## 2. The Language Map — Real Linguistic Geography

A comprehensive real set of language groups, organized by actual linguistic family so genuine relationships (and genuine isolation) are visible rather than flattened into one arbitrary list — deliberately grouped at the family level rather than tracking every real historical dialect individually, the same abstraction discipline this project applies to Distance Tiers and Reputation Duality shapes. Every entry below ties directly to a culture or region this project already tracks; nothing here is invented population data, only a language layer read onto data that already exists.

### 2.1 Italic

**Latin** — Italian Heartland natively; the empire-wide administrative and elite second language everywhere else, the one language a genuinely well-traveled Roman household can assume near-universal recognition of among other elites, if not among ordinary populations.

**Oscan** — a real, genuinely distinct sister language to Latin within the same Italic family, not a dialect of it, and a real gap this document corrects directly: Campania's own document builds substantial real texture around its Oscan/Samnite cultural thread (Starting Regions: Italian Heartland §4.5, §4.8's own Nola and Nuceria entries, the household of Gens Alfidia), including the real, genuine fact that surviving Pompeian graffiti includes actual Oscan-language inscriptions alongside Latin ones. This document names Oscan explicitly rather than letting that entire cultural thread go unlanguaged. A further, real Italic-family footnote worth a brief mention without full separate tracking: Umbrian, Ligurian, and Venetic were all real, distinct early Italian languages absorbed into Latin well before this game's own range opens, surviving nowhere as a living Proficiency option but real enough to name honestly rather than pretend never existed.

### 2.2 Hellenic

**Greek (Koine)** — Greek East and Anatolia's western coast natively; Egypt's Alexandria; the real, genuine lingua franca of the entire eastern Mediterranean, known by educated elites across Syria, Judaea, and Egypt regardless of their own native tongue. The Bosporan Kingdom's own Hellenic population (Starting Regions: The Bosporan Kingdom) speaks this natively as well, genuinely distant from the Greek East's own dialect but mutually intelligible.

### 2.3 Celtic — Three Real, Distinct Branches

Real linguistic classification splits Celtic into branches this document keeps distinct rather than flattening into one label, each carrying real, partial mutual intelligibility within itself but genuine distance from the others:

- **Gaulish (Continental Celtic)** — Gallic Frontier natively.
- **Brythonic (Insular Celtic)** — Britannia natively, a real, distinct branch from Gaulish despite the shared family, though close enough that a Gaulish speaker has a real, plausible discount toward learning it (§5).
- **Goidelic** — Hibernian (Ireland), genuinely more distant again from both Gaulish and Brythonic, consistent with Britannia's own document treating the Hibernian relationship as distant and only lightly engaged.
- **Galatian** — Anatolia's own Celtic-descended population, a real, fascinating case of a Continental Celtic-family language transplanted into central Anatolia generations before this game's own range, gradually Hellenizing (Starting Regions: Anatolia / Asia Minor §7) rather than staying linguistically pure — a Galatian speaker plausibly holds real Greek proficiency alongside their own native tongue by default.
- **Noric** — the Alpine Provinces' own Celtic-influenced native tongue (Starting Regions: The Alpine Provinces §7), related to but distinct from Gaulish proper, consistent with that document's own description of Noric culture as genuinely Celtic-influenced rather than a straightforward transplant.

### 2.4 Semitic

- **Punic** — Iberian Colony's southern/western coast, North African Colony; the real Western Phoenician-diaspora language.
- **Aramaic** — Syria/The Levant's real everyday vernacular, and Mesopotamia.
- **Hebrew** — Judaea specifically, primarily in liturgical and religious use rather than daily vernacular by this era — Aramaic remains the real everyday language even there.
- **South Arabian** — Arabia Felix's own real, distinct South Semitic branch, genuinely separate from Aramaic and Hebrew's own Northwest Semitic family despite the shared wider Semitic root.

### 2.5 Afroasiatic (Non-Semitic)

**Egyptian (Demotic)** — native Egyptian population, distinct from Alexandria's own Greek-speaking city, and from Semitic entirely despite sharing the wider Afroasiatic family tree.

### 2.6 Iranian

- **Parthian** — Mesopotamia, and Armenia's own Arsacid-descended nobility.
- **Sarmatian/Scythian** — the Bosporan Kingdom's own frontier, and the Pontic steppe more broadly, a real, distinct Iranian-family language despite geographic distance from Parthia.
- **Sogdian** *(Trade Contact Only, extremely rare)* — the real, historically significant lingua franca of the overland Silk Road trade, encountered only through the same rare individual trade contact Cultures of the Known World already restricts this culture to.

### 2.7 A Distinct Indo-European Branch

**Armenian** — genuinely its own separate branch of the Indo-European family, related to but distinct from every other language on this list, exactly matching the real linguistic fact that Armenian shares no close relative anywhere on this roster.

### 2.8 Sino-Tibetan and Indo-Aryan *(Trade Contact Only, extremely rare)*

- **Sanskrit/Prakrit** — India's own real trade-contact languages, reachable only through the genuine Indian Ocean trade this project's own Periplus references (Starting Regions: Arabia Felix §5) already establish.
- **Chinese** — reachable only through the most indirect, rarest possible Silk Road contact, consistent with Cultures of the Known World's own treatment of this culture as barely present at all.

### 2.9 A Further Semitic Outlier *(Trade Contact Only, rare)*

**Ge'ez** — the real, distinct language of the rising Aksumite kingdom (Starting Regions: Nubia §15.2's own Slow Rise of Aksum), genuinely reachable more often than Sogdian or Chinese given Aksum's own real, growing Red Sea trade presence, but still a real rarity outside that specific contact.

### 2.10 Genuine Language Isolates and Thinly Attested Tongues

Several real languages on this roster share no close relative at all, or survive so thinly in the real historical record that this document names that honestly rather than inventing false certainty:

- **Basque/Aquitanian** — genuinely one of the most striking linguistic facts available anywhere on this roster: this real language, native to part of Iberian Colony's own northern population, isn't Indo-European at all, sharing no meaningful relationship with Celtiberian, Latin, or any other language this document tracks. A true isolate.
- **Numidian/Berber** — native North African Colony populations, its own distinct Afroasiatic branch separate from both Semitic and Egyptian.
- **Iberian/Celtiberian** — native Iberian Colony populations, distinct from the Basque isolate above.
- **Meroitic** — Nubia's own real, historically attested written and spoken language — genuinely still not fully deciphered by modern scholarship even in the real world, exactly as that document's own §1 already notes. This document names it as a real, distinct, and honestly mysterious language rather than silently omitting it.
- **Illyrian/Thracian/Dacian** — the Balkans, grouped given real, honest historical thinness of individual attestation.
- **Rhaetic** — the Alpine Provinces, honestly flagged (consistent with that document's own §7) as thinly attested.
- **Caledonian (Pictish-ancestral)** — Britannia's own permanently unconquered northern population, real and distinct from Brythonic, and, consistent with the real historical record, only thinly attested.
- **Cappadocian** — Anatolia's own native frontier population, a real, poorly-attested Anatolian-family remnant by this era, alongside the similarly obscure real remnant of Phrygian in the same broader region — neither survives this game's own era as more than a thin trace, named honestly rather than invented into false clarity.
- **Eteocypriot** — a real, genuine pre-Greek language attested on Cyprus, though by this game's own era Cyprus itself is thoroughly Hellenized in daily use, making this an honest historical footnote more than a living, tracked option.

### 2.11 Extinct Languages With Living Ritual Use

Two real languages worth naming directly even though this document doesn't track them as spoken options at all, because both survive as real, specifically religious registers: **Etruscan**, genuinely extinct as a spoken language by this game's own era but real-historically preserved in haruspicy's own ritual formulas (Starting Regions: Italian Heartland §3.5), and **Sicel/Sicani**, Sicily's own pre-Greek native tongues, absorbed long before this game's range opens with no real living trace beyond place names. Neither is a learnable Language Proficiency entry — both are pure flavor, available to Religion's own ritual content specifically.

---

## 3. Literacy — Derived by Default, Tracked Where It Matters

For the overwhelming majority of the population — Notable Households, ambient pop groups — Literacy is never separately tracked; it's simply assumed from Legal Status and Wealth tier (a Citizen or Prosperous-tier household reads reasonably often; a Meager-tier Coloni household usually doesn't, consistent with real historical literacy patterns). For a named, full Character record, Literacy becomes a real, explicit boolean or light tier, derived from Learning and Education & Culture investment — still not a separately-trained stat requiring its own dedicated actions, but visible and meaningful where the game actually needs to know it.

**A real, honest historical inversion worth building in directly:** literacy didn't track wealth or citizenship cleanly. A genuinely common, well-documented pattern saw an educated Greek slave or freedman — often purchased or trained specifically for administrative or secretarial skill — serve as a household's own literate specialist even when the Roman citizen who owned them was comparatively less literate themselves. This document names that pattern directly as the real texture behind Companions & Court Positions' own Tabularius and Tutor roles: a household's own Literacy, where it matters mechanically, is often actually the literacy of whoever holds one of those two positions, not the household head's own personal reading ability.

---

## 4. Fluency Tiers

A simple, four-step scale for Language Proficiency, applied per language a Character has any exposure to:

- **None** — no meaningful comprehension.
- **Basic** — enough for simple trade and basic exchange, a real, plausible level for anyone with regular but shallow contact (a merchant's own limited vocabulary in a trading partner's tongue).
- **Conversational** — genuine, comfortable daily communication, without the polish needed for formal or technical matters.
- **Fluent/Native** — full command, including the formal register a real diplomatic negotiation or a written legal document requires.

---

## 5. Learning a Language

Several real, concrete paths, none of them a new standalone minigame:

- **Native acquisition** — a Character's own origin culture (per their region's own Population & Culture Distribution table) grants Fluent/Native status in that language's own group automatically at creation.
- **Formal education** — Education & Culture's own Learning investment, especially at an Institution of Renown (Athens, Rhodes, Alexandria, Pergamon, Massilia — all genuinely cosmopolitan, historically multilingual environments), is the fastest, most reliable path to a second or third language reaching Fluent status.
- **Sustained exposure** — time spent holding a Distant Holding (Starting Regions §7) in a region with a different dominant language, or an extended Travel stay, gradually raises Proficiency from None toward Basic and Conversational without any deliberate study action, a real, honest reflection of how immersion actually works.
- **A Wanderer teacher** — a Philosopher or Rhetorician-type Wanderer (Wandering Populations §2), Hosted or Recruited, is a real, concrete acceleration path for formal language instruction specifically.
- **A family-relationship discount** — per §2.3 and §2.6, a Character who already holds real Proficiency in one member of a related language family (Gaulish before Brythonic; Sarmatian/Scythian alongside Parthian, both Iranian) acquires the related language faster than an entirely unrelated one would take, reflecting real, partial mutual intelligibility rather than starting from zero each time.

---

## 6. The Language Barrier — Soft Penalty, Hard Gate

Two genuinely different weights, reserved for genuinely different stakes:

- **Ordinary interactions** — a Romantic Interaction, a routine Clientela conversation, an ordinary social exchange between Characters who share no language carries a real but soft penalty: reduced effectiveness, described narratively as halting or imprecise, rather than an outright block. Two people can still manage, awkwardly.
- **Formal diplomacy — a real, earned hard gate.** Negotiating directly with a Frontier, Contested Buffer, Independent, or Great Power people — Armenia's own court (Starting Regions: Armenia, Armenian), Nubia's Kandake (Starting Regions: Nubia, Meroitic), Arabia Felix's Sabaean nobility (South Arabian), the Sarmatian/Scythian frontier (The Bosporan Kingdom), or Gallic Frontier's own Germanic neighbor — genuinely cannot proceed at all without either the negotiating Character holding at least Conversational proficiency in the relevant language, or a qualified Interpres (§7) present. This is a deliberate hard gate, reserved for exactly the kind of high-stakes moment this project already reserves other hard gates for (the Senate's own property census, the cursus honorum's Dignitas threshold) rather than applied to routine gameplay.

---

## 7. The Interpres — A New Companion Role

A new Companion/Court Position, per direction, though deliberately flexible rather than mandatory: any Character who happens to hold Conversational-or-better proficiency in the needed language can serve this function informally, whether or not they hold this specific title. A household that formally appoints an **Interpres** — often a freedman or a Companion with real, relevant regional origin or education — gets a standing, reliable answer to §6's own hard gate without needing to hope the right Character happens to already be fluent when a negotiation opportunity arises, the same "hire the standing solution instead of hoping" logic Companions & Court Positions already applies to every other specialist role.

---

## 8. Multilingual Elites — No Hard Cap

Per direction, no artificial ceiling on how many languages a sufficiently learned Character can hold — real elite Roman practice routinely produced genuine trilingualism (Latin, Greek, and a native regional tongue) as an unremarkable baseline for an educated household, and a genuinely exceptional, well-traveled, or highly-educated individual could plausibly hold Fluent or Conversational proficiency in four or five languages at once, exactly the kind of real-world outlier this document's own uncapped Education & Culture-driven acquisition path already supports naturally without needing a special exception.

---

## 9. Cross-System Integration

- **Starting Regions (all documents):** §2's entire Language Map reads directly from each region's own existing Population & Culture Distribution table — no new population data invented, only a language layer added on top. Oscan (§2.1) specifically closes a real gap in the Italian Heartland document's own already-substantial Campania content.
- **Education & Culture:** Learning investment and the named Institutions of Renown are this document's own primary language-acquisition mechanism (§5).
- **Diplomacy with Non-Roman Peoples:** §6's hard gate is this document's own most consequential addition — formal negotiation with any Frontier, Contested Buffer, Independent, or Great Power people now genuinely requires it.
- **Companions & Court Positions:** the Interpres (§7) is a new named role; the Tabularius and Tutor (§3) are given their own real historical literacy-specialist texture.
- **Wandering Populations:** a Philosopher/Rhetorician Wanderer is a real, concrete language-acquisition accelerator (§5).
- **Starting Regions' own Distant Holding mechanic:** sustained exposure through a Distant Holding (§7 of the framework document) is a real, passive Proficiency-growth path (§5).
- **Familia:** a Character's own origin culture, already tracked there, is this document's own direct source for native-language assignment.
- **Notable Households, Settlement Demographics:** ambient population Literacy and Language stay derived rather than tracked, per §1 and §3's own restraint.
- **Religion, Religions of the Known World:** Etruscan haruspicy formulas and any comparable ritual-only survival (§2.11) are this document's own explicit, narrow exception to "extinct languages aren't tracked" — real, but flavor-only, never a learnable Proficiency entry.
- **Cultures of the Known World:** every Trade-Contact-Only culture's own real language (§2.6, §2.8, §2.9) is named here consistently with that document's own established rarity treatment — encountered, never a normal acquisition target.

---

## 10. Data Model

```
LanguageProficiency {                  // tracked only for full, named Character records
  characterId, language,
  fluencyTier,                          // "none" | "basic" | "conversational" | "fluentNative"
  acquisitionMethod,                     // "nativeOrigin" | "formalEducation" | "sustainedExposure" | "wandererInstruction"
}

LanguageFamily {                        // §2, §5 — supports the family-relationship acquisition discount
  familyId, familyName,                  // e.g. "celtic", "iranian", "semiticNorthwest"
  memberLanguages: [ ... ],
  isIsolate: bool,                        // true for Basque/Aquitanian specifically — no discount ever applies
}

LiteracyRecord {                        // tracked only where mechanically relevant
  characterId,
  isLiterate: bool,
  derivedFrom,                           // "legalStatusAndWealth" (ambient default) | "learningAttribute" (named Characters)
}

InterpresAppointment {                   // §7
  householdId, characterId,
  languagesCovered: [ ... ],
}

DiplomacyLanguageGate {                  // §6 — the hard-gate check
  negotiationId, requiredLanguage,
  gateClearedBy,                          // "negotiatorFluency" | "interpresPresent" | null (negotiation cannot proceed)
}
```

---

## 11. Open Questions

- **All numeric sizing**, per this project's standing convention — Proficiency growth rates from sustained exposure, and the exact Learning threshold for reaching Fluent status through formal education, are unsized.
- **Whether Literacy should ever gate a specific Interaction** (reading a letter, a legal document) the way §6 gates formal diplomacy — this document treats Literacy as a real fact with narrative and Companion-role weight but doesn't specify a hard mechanical block tied to it directly.
- **Galatian and Gaulish mutual intelligibility's own exact discount value.** §5's own family-relationship discount is now established as a real mechanic, but the precise magnitude of the discount for each specific family pairing (Continental-to-Insular Celtic versus the more distant Continental-to-Goidelic case, for instance) isn't sized here.
- **Whether a Character can ever forget a language** through prolonged disuse — this document assumes acquired Proficiency is permanent once reached, which may be worth revisiting in a future pass.
- **The Interpres role's own capacity limits.** §7 doesn't specify whether a single appointed Interpres can cover multiple languages at once or whether a household needing several would require several appointments.
