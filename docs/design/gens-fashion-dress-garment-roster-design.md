# GENS — System Design: Fashion & Dress — The Garment Roster
*The companion catalog Fashion & Dress deliberately deferred: every named garment, accessory, hairstyle, and cosmetic a Character can actually hold as a Garment Slot or Outfit Tier component, organized by function, status, region, and occasion. This document adds no new mechanics — every entry plugs directly into the parent document's existing Wardrobe, Outfit Tier, Garment Slot, and Occasion structure. Where an item carries a real, specific piece of Roman (or provincial) history worth knowing, it's named here rather than left generic. This pass significantly expands coverage: dinner and philosopher's dress, bathing and athletic gear, arena and racing dress tied directly to Games & Spectacle's own named gladiator types, priestly dancing-and-augury regalia, funeral ancestor masks, the freedman's cap, the rarest Roman military honor, several individually broken-out regional entries (Armenia, Arabia Felix, Nubia, Judaea, Sicily), a Phrygian cap for Mithraic devotees, and — the centerpiece of this pass — a full layered-paperdoll rendering model (§16) explaining exactly how a Character's Wardrobe drives their generated portrait, in the same procedural spirit as Free Cities' own model system.*

---

## Contents

1. Scope & Role
2. The Roman Core Wardrobe — Everyday & Formal
3. The Toga Family — One Garment, Many Meanings
4. Footwear
5. Jewelry & Accessories
6. Military Dress, Insignia & Dona Militaria
7. Priestly & Ceremonial Vestments
8. Slave, Labor & Poverty Dress — and the Freedman's Cap
9. Milestone & Life-Event Garments
10. Regional & Cultural Dress
11. Bathing, Athletics & Leisure Dress
12. Arena & Circus Dress
13. Hair, Wigs, Cosmetics & Grooming Tools
14. Fabrics, Dyes & Color Symbolism
15. Outfit Tier Reference — What Each Tier Actually Looks Like
16. Portrait Rendering — The Wardrobe as a Layered Paperdoll
17. Cross-System Integration
18. Data Model
19. Open Questions

---

## 1. Scope & Role

Fashion & Dress built the engine — Outfit Tier, Garment Slots, Occasion categories, the Household Dress Policy hierarchy, Livery, Disguise, Era Drift. This document is the content that fills it: the actual named pieces, real where the historical record supports it, organized so the parent document's `garmentId` references have somewhere real to point. Every entry below states its **Category** (which Occasion(s) it belongs to), its **Gate** (if any — reusing Fashion & Dress §5's own gate types), and its rough **Outfit Tier** contribution, so a designer or a future balancing pass can slot it directly into the existing schema without guesswork.

This document does not introduce new mechanics, new Traits, or new Legal Status categories. Where an entry ties to a status marker, an office, or a piece of history another document already owns (the laticlavus, the Vestal's suffibulum, dona militaria, Games & Spectacle's own gladiator fighting styles), this document names the physical object; the system governing who may hold it, or resolving what happens with it, stays exactly where it already lives.

---

## 2. The Roman Core Wardrobe — Everyday & Formal

The baseline layer nearly every Roman-culture Character draws from, regardless of Legal Status or Social Class — differentiated by material, dye, and condition (Outfit Tier) far more than by cut.

| Garment | Category | Outfit Tier Range | Notes |
|---|---|---|---|
| **Tunica (Tunic)** | Everyday, base layer under everything else | Meager–Fine | The universal Roman base garment, worn by everyone from field labor to senators; Outfit Tier is read almost entirely off its cloth quality and cleanliness rather than its cut |
| **Subligaculum** | Everyday, undergarment | All tiers | A simple loincloth/underwear, worn under the tunic by both sexes; purely a baseline assumption, never itself a Garment Slot choice |
| **Stola** | Formal, women's citizen marker | Respectable–Opulent | The married citizen matron's own formal overdress, worn over the tunic — see Fashion & Dress §5's own gated-garment table |
| **Palla** | Formal/Everyday, women's wrap | Modest–Opulent | A large rectangular wrap/shawl worn by women over the stola or tunic, doubling as a head-covering; the female equivalent, in everyday versatility, of a man's cloak |
| **Paenula** | Everyday/Travel, hooded cloak | Modest–Respectable | A practical, hooded, front-closed traveling cloak worn by anyone regardless of status when weather or the road called for it — a real leveler garment, since a senator on a rain-soaked journey wore the same paenula a courier did |
| **Lacerna** | Formal-adjacent, cloak worn over the toga | Respectable–Fine | Fashionable but historically considered a touch too casual for the most formal Senate business — a real, era-appropriate tension a Refined Character's own dress choices can play with |
| **Sagum** | Everyday/Military, short cloak | Modest–Respectable | A shorter, plainer cloak strongly associated with soldiers and with Gallic dress (§10) — the real Latin idiom *saga sumere* ("to don the war-cloak") meant, literally, to go to war |
| **Birrus** | Everyday/Travel, hooded cloak | Modest–Respectable | A later-era (Imperial) hooded weatherproof cloak, a genuine Era Drift-appropriate alternative to the Paenula for a later-set playthrough (Fashion & Dress §11) |
| **Pallium** | Formal-adjacent, intellectual/Hellenic dress | Respectable–Fine | A real, rectangular Greek-style mantle, worn in place of the toga specifically by philosophers, teachers, and Hellenized intellectuals (Education & Culture's own philosophical schools) — a deliberate, real, and mildly pointed dress statement in Roman society: choosing Greek dress over the toga was itself a legible cultural position, not a neutral substitution |
| **Synthesis** | Formal, dinner-specific | Respectable–Opulent | A real, lighter, often brightly colored matching tunic-and-wrap set worn specifically for dining (the toga being genuinely impractical for reclining at a Triclinium feast) — real Roman convention held it improper to wear the synthesis in public outside the dinner setting itself, a small, concrete Occasion-specific rule this document's own §Occasion structure can enforce directly for Feasts (Activity Engine) |
| **Endromis** | Athletic/Leisure | Modest–Fine | A real, heavy wool wrap worn after exercise to avoid a chill — see §11 |
| **Balteus** | Military, worn accessory | — | A sword-belt/baldric worn across the shoulder, distinct from the waist-worn Cingulum Militare (§6) — together the two real, distinct belt types a Roman soldier actually wore |

---

## 3. The Toga Family — One Garment, Many Meanings

The Toga is not one item but a real family of distinct, historically specific variants, each its own Garment Slot with its own gate — the single richest status-marker cluster in Roman dress, and worth its own dedicated section rather than folding into §2's general table.

| Variant | Real Name | Gate | Meaning |
|---|---|---|---|
| **Plain white toga** | *Toga Virilis* / *Toga Pura* | Adult male citizen | The ordinary formal toga of an adult male citizen — see §9's Toga Virilis coming-of-age beat, traditionally taken on the real festival of **Liberalia** (March 17 — the Roman Calendar's own dateable hook for this milestone) |
| **Purple-bordered toga** | *Toga Praetexta* | A held magistracy, or a freeborn child pre-Toga Virilis | Real dual meaning: worn by curule magistrates in office *and* by freeborn children before their coming-of-age — an intentional, real historical overlap between "not yet a citizen adult" and "a citizen exercising formal authority," both marked by the same purple border |
| **Fully purple/gold-embroidered toga** | *Toga Picta* / *Toga Purpurea* | A held Triumph, or (later Imperial era) the reigning emperor | The rarest and highest-status toga variant in the entire roster — worn by a general during an actual Triumph procession (Military & Combat §3.3's own rare pinnacle), and, by the later Imperial era, increasingly an emperor's own everyday formal dress. A household earning the right to this garment even once is a genuine, singular Dynasty Chronicle image |
| **Whitened/chalked toga** | *Toga Candida* | Actively standing for election | Real, specific, and the literal root of the modern word "candidate" — a toga deliberately whitened with chalk, worn only while actively campaigning for a magistracy (Politics & Patronage §5). A perfect, concrete visual hook for a contested-election Event: a household's own candidate is, for the campaign season, visibly and constantly announcing it |
| **Dark/undyed toga** | *Toga Pulla* | Mourning period, any citizen | §9's own mourning garment — undyed or deliberately dark wool, worn for the customary mourning period following a death |

---

## 4. Footwear

| Item | Real Name | Gate | Notes |
|---|---|---|---|
| **Formal outdoor shoes** | *Calcei* | Citizen, worn with the toga | The correct formal footwear paired with any toga variant; removed indoors in favor of Soleae |
| **Senatorial shoes** | *Calceus Senatorius* / *Mulleus* | Senatorial Social Class | Real, distinct reddish-brown or purple-tinted senatorial footwear, fastened with a real, distinctive crescent-moon buckle (the *lunula*) — a second, foot-level status marker running directly alongside the laticlavus stripe, and, per Fashion & Dress §5, an impersonation-worthy Scandal trigger in its own right if worn without the Social Class to back it |
| **Indoor sandals** | *Soleae* | None | Simple, informal sandals worn indoors and swapped for at a Triclinium's own threshold — real Roman etiquette held that outdoor shoes didn't belong at the dinner table |
| **Military boots** | *Caligae* | Enlisted/legionary service | Real, iconic hobnailed soldier's boots — famously the source of the emperor Caligula's own nickname ("Little Boot"), earned as a child accompanying his father's legions |
| **High boots** | *Campagus* | Senior office, later Imperial court dress | A more elaborate, higher-status boot, increasingly associated with senior officials and the later Imperial court rather than ordinary citizen footwear |
| **Socks** | *Udones* | None, but a genuine Frontier/Northern-culture practicality | A real, specific, and genuinely delightful piece of hard archaeological evidence: the real Vindolanda tablets, recovered from a Roman fort in Britannia, include an actual surviving letter requesting socks and underpants be sent to soldiers stationed there — direct, dated proof that Roman soldiers on the Northern Frontier wore socks with their sandals despite the popular modern image otherwise. A small, textured, historically airtight detail for any Britannia- or Gallic Frontier-stationed Character's Everyday dress |
| **Plain/bare feet** | — | None | The Meager-tier default; bare feet or simple undyed leather are the honest, unremarkable norm for most of the enslaved and rural poor (§8) |

---

## 5. Jewelry & Accessories

| Item | Real Name | Gate | Notes |
|---|---|---|---|
| **The Bulla** | *Bulla* | Freeborn child, pre-Toga Virilis | A round pendant/locket, often containing a protective amulet, worn by freeborn children until formally set aside at the Toga Virilis ceremony (§9) — a real, tender, and genuinely touching Roman practice worth the Chronicle weight that milestone already carries |
| **Signet Ring** | *Anulus Signatorius* | Head of household, or a formally delegated authority | Fashion & Dress §5's own named tie into Correspondence & Letters — its wax impression is the real, standard method of authenticating a sealed letter or document |
| **Gold Ring (citizen privilege)** | *Ius Anuli Aurei* | Historically Equestrian/Senatorial; later broadened | A real, specific Roman legal privilege: the right to wear a gold ring publicly was, for much of the Republic, formally restricted to Senators and Equestrians, with ordinary citizens historically limited to iron — a genuine, concrete second illustration (alongside the laticlavus/angusticlavus) of jewelry itself carrying formal legal weight rather than being purely decorative |
| **Iron Betrothal Ring** | *Anulus Pronubus* | Formally betrothed (Romance, Sexuality & Lineage §4.1) | The real, historically attested iron ring exchanged at formal betrothal (*sponsalia*) — this document's own concrete physical object behind that system's own already-established mechanic |
| **Fibula (Brooch/Pin)** | *Fibula* | None, but style is a real cultural marker | The practical pin fastening a cloak or toga at the shoulder; style and material (plain iron for a soldier, ornate gold for an elite Character, a distinctive La Tène spiral pattern for a Gallic-culture one, §10) is a genuine, legible at-a-glance cultural and wealth signal |
| **Necklaces, earrings, bracelets** | — | None (Sumptuary-adjacent at Opulent tier) | Generic Jewelry-category goods (Buildings §4.5's Goldsmith's Studio output); a sufficiently valuable set (real gemstones, Pearl) is itself Sumptuary Edict territory (Fashion & Dress §5) |
| **Torc** | *Torques* (as jewelry, distinct from the military decoration, §6) | Gallic/British/Germanic cultural dress | A real, iconic Celtic neck-ring, worn as an everyday high-status marker in Gallic, British, and Germanic culture (§10) long before Rome adopted its own military version (§6) — the same object, two entirely different meanings depending on who's wearing it and why |
| **Amber jewelry** | *Sucinum* | None; a real Northern-import luxury good | Baltic amber, traded south along a real, well-documented long-distance route through Germanic territory and prized enough that the elder Pliny himself wrote at length about its trade and mystique — a genuine luxury good distinct from the Mediterranean's own Pearl and gemstone supply, and a natural Resources & Goods addition tied specifically to the Northern Frontier |
| **Ring cabinet** | *Dactyliotheca* | Wealth-signaling household fixture, not worn | Not a Garment Slot but a real, attested elite Roman collecting practice worth naming for Villa decoration purposes (Villa §7) — the elder Pliny records a real, famous example (the collection of Scaurus). A household with a genuine Dactyliotheca on display is quietly announcing exactly the kind of accumulated wealth a Sumptuary Edict (Fashion & Dress §5) is built to notice |
| **Child's rattle-pendants** | *Crepundia* | Young child | A real, small set of toy pendants or tokens strung together and hung around a young child's neck — historically also, per real Roman comic tradition, sometimes later used as a recognition token for a child separated from their family, a small, genuine hook for a Familia-adjacent lost-and-found story beat if one is ever wanted |

---

## 6. Military Dress, Insignia & Dona Militaria

Closes a real gap: Military & Combat established rank, command, and battlefield resolution in full, but never gave the game a physical, wearable expression of a soldier's own earned distinction. **Dona Militaria** — real, historically attested Roman military decorations — is that missing piece, and slots directly into the Garment Slot/Accessory framework as a genuine, earned reward parallel to a Combo Title.

| Item | Real Name | Gate | Notes |
|---|---|---|---|
| **General's cloak** | *Paludamentum* | Held military command | A distinct, high-status purple-or-red cloak marking a commanding officer in the field, visually separating a Legate or Praefectus (Companions & Court Positions §5.2, Military & Combat §3.2) from the ranks they lead |
| **Military belt** | *Cingulum Militare* | Enlisted/legionary service | A real, symbolically loaded item — issued on enlistment and formally removed on dishonorable discharge; "stripped of the belt" was real Roman idiom for exactly that disgrace, a natural, concrete visual for a Military & Combat court-martial or dismissal outcome |
| **Segmented arm guard** | *Manica* | Legionary or gladiatorial equipment | Real armor piece worn by some legionaries and, notably, several gladiator types (§12) — a small, concrete visual link between military and arena dress |
| **Neck torc** *(military)* | *Torques* | Awarded for valor | A genuine Roman military decoration for bravery, real-historically borrowed directly from Gallic/Celtic tradition (§5, §10) — earned rather than purchased, gated by a specific Military & Combat valor outcome rather than Legal Status or wealth |
| **Arm bands** | *Armillae* | Awarded for valor | A real companion decoration to the torc, worn on the forearm |
| **Decorative discs** | *Phalerae* | Awarded for valor | Real, medal-like metal discs worn on a harness or breastplate — the closest real Roman equivalent to a modern campaign medal, and a natural, visible "how decorated is this veteran" read at a glance |
| **Civic Crown** | *Corona Civica* | Awarded for saving a fellow citizen's life in battle | One of the single highest personal honors in the entire Roman military honor system — a real, oak-leaf crown, genuinely rare, and a guaranteed Dynasty Chronicle-tier moment wherever it's earned |
| **Mural Crown** | *Corona Muralis* | Awarded to the first soldier over an enemy wall in a siege | A real, specific siege-honor, giving Military & Combat's own siege resolution a concrete, earnable decoration it didn't have before |
| **Naval Crown** | *Corona Navalis* | Awarded for outstanding naval valor | The naval equivalent, a real and rarer honor still, tying directly to any future naval-engagement content |
| **Grass/Blockade Crown** | *Corona Graminea* (also *Corona Obsidionalis*) | Awarded, exceptionally rarely, for personally saving an entire besieged army or legion | The single rarest and most prestigious military honor Rome ever awarded — real Roman history records only a small handful of recipients across the entire Republic and Empire (Sulla among the real, attested few), and, uniquely, woven from grass and plants gathered from the actual battlefield where the deed occurred rather than crafted from metal. This document reserves it as this project's own rarest possible Dona Militaria — a genuine, Legendary-tier, once-or-twice-per-campaign Dynasty Chronicle event rather than an ordinary reward, deliberately above even the Corona Civica |

A Character holding one or more Dona Militaria wears them automatically for the Military/Campaign and Formal Occasion categories (Fashion & Dress §4) once earned — no separate unlock action required beyond the underlying Military & Combat outcome itself, exactly the same "office investiture" automation Fashion & Dress §9 already establishes for a magistracy or Priesthood.

---

## 7. Priestly & Ceremonial Vestments

| Item | Real Name | Gate | Notes |
|---|---|---|---|
| **Flamen's cap** | *Apex* | Held Flamen office (Religion §6.2) | A real, distinctive pointed cap topped with a small wool tuft on a rod, the single most visually specific piece of Roman priestly regalia |
| **Priestly skullcap** | *Galerus* | Held any major Priesthood office | The leather skullcap forming the base of the Apex; also worn on its own by lower priestly attendants |
| **Sacred woolen fillet** | *Infula* | Any priest, a Vestal, or a sacrificial victim | A real, plain woolen headband/fillet worn during rites — worth naming directly rather than softening, since real Roman practice placed one on a sacrificial animal as well as on a priest, a small, honest historical detail consistent with this project's frankness pillar |
| **Vestal's veil** | *Suffibulum* | An active VestalRecord (Religion §6.3) | A distinct white veil, fastened with its own fibula, unique to the Vestals; per Fashion & Dress §7, entirely outside household dress-policy authority |
| **Sacrificial white robes** | — | Performing a formal rite | The plain, deliberately unadorned dress worn by the officiant during a sacrifice — a real, restrained contrast to the ornamented Apex/Suffibulum worn otherwise |
| **Salii dancing-priest regalia** | — | Held membership in the real Salii priesthood (Religion §6.2) | A real, genuinely striking exception to Roman priestly restraint: the Salii ("leaping priests" of Mars) processed and danced through Rome each March and October in archaic bronze armor, carrying the sacred *ancilia* shields, wearing a distinct conical cap — vivid, colorful, martial-ceremonial dress unlike any other priestly office, and a natural Roman Calendar-tied seasonal Event |
| **Augur's staff** | *Lituus* | Held Augur office (Religion §6.2) | Not a garment but a real, iconic curved ceremonial staff used to mark out the sky's sections for divination — worth naming here as the single most recognizable accessory of the office, carried rather than worn |
| **Phrygian cap** | — | Cult of Mithras affiliation (Religions of the Known World §6) | A real, distinct soft cap with a forward-folded peak, originally associated with Phrygia and the wider East — and, most usefully for this project, the real, standard headwear Mithras himself is depicted wearing across surviving Roman-era Mithraic art. A small, concrete, historically accurate visual marker for a Mithras-affiliated Character (often, per that document's own note, a veteran) distinct from any ordinary Roman priestly cap |
| **Diadem** | — | Hellenistic-influenced royalty/high nobility only (Bosporan Kingdom, Armenia) | A real, simple cloth or metal band worn around the head, the actual historical mark of Hellenistic kingship inherited from Alexander's own successor states — a fitting, real, and distinctly *non*-Roman marker for a Bosporan or Armenian ruling figure (§10), since Rome's own citizens famously and pointedly rejected the diadem as monarchical and un-Republican throughout most of this game's own range |
| **Archaic mourning veil** | *Ricinium* | Mourning, an older/Republican-era alternative to the Toga Pulla for women specifically | A real, older Roman mourning headcovering, largely superseded in general use by the later Toga Pulla and dark palla (§9) by the Imperial era — a genuine, small Era Drift (§11 of the parent document) opportunity: a household holding to the Ricinium well into the Imperial period reads as a small, deliberate Traditionalist statement, the funeral-dress equivalent of the beard-fashion example the parent document already gives |

---

## 8. Slave, Labor & Poverty Dress — and the Freedman's Cap

Consistent with this project's own frankness pillar — described plainly, without gratuitousness, and without softening a real, harsh historical fact where one exists, while also giving this section's single most hopeful real object its proper weight.

| Item | Notes |
|---|---|
| **Basic undyed tunic** | The Meager-tier default across nearly all enslaved and rural poor Characters — plain, undyed wool or coarse linen, Buildings' own Weaver's Loom output at its cheapest grade |
| **Ergastulum-tier dress** | Labor & Slavery's own Bare Regimen tier, given physical form here: worn, minimally maintained clothing, a direct, visible signal distinguishing a harshly-kept labor gang from ordinary household staff even at a glance |
| **The slave collar** | A real, specific, and genuinely grim historical object worth naming factually rather than omitting: some enslaved individuals, particularly those with a prior flight attempt on record, were fitted with an inscribed metal collar identifying them as a fugitive risk and directing a finder to return them to their owner. This document names it as a real, available (and deliberately ugly) escalation tied directly to Labor & Slavery's own flight-risk and recapture mechanics (§7 of that document) — a visible, lasting mark of a specific disciplinary history, not a default state, and depicted with the same restraint this project already applies to Punishment (Labor & Slavery §10) |
| **The Freedman's Cap** | *Pileus* | The direct, real, and deliberately hopeful counterpart to the collar above: a soft, conical felt cap, historically placed on a newly manumitted individual's head as part of the actual manumission ceremony (Labor & Slavery §8) — real Roman practice, and the genuine historical origin of the "liberty cap" imagery that would go on to outlive Rome itself by nearly two thousand years. This document makes it a guaranteed, automatic Garment Slot unlock the instant manumission completes — the single most emotionally direct piece of dress in the entire roster, and a natural, small Dynasty Chronicle beat in its own right the first time a household's own freed Character is shown wearing one |
| **Livery** | See Fashion & Dress §8 in full — a household's own deliberate, matching staff standard sits above this section's baseline, by choice and at cost |

---

## 9. Milestone & Life-Event Garments

Consolidating Fashion & Dress §9's own named references into concrete, described items, and adding the funeral's own single most iconic real object.

| Item | Real Name | Occasion | Notes |
|---|---|---|---|
| Coming-of-age toga | *Toga Virilis* | Toga Virilis ceremony, traditionally on Liberalia | §3 above |
| Freedman's cap | *Pileus* | Manumission | §8 above |
| Bridal veil | *Flammeum* | Weddings | A real, vivid flame-orange/yellow veil, the single most visually distinct piece of Roman wedding dress |
| Bridal hairstyle | *Seni Crines* ("six locks") | Weddings | A real, specific, ritually significant hairstyle for the bride, parted into six braided locks with a spear-point (historically, a real ceremonial spearhead) — a genuine, concrete detail for §13's hairstyle system to render on this one specific Occasion |
| Dowry Trousseau | — | Marriage negotiation | Fashion & Dress §9's own dowry-linked garment/jewelry set |
| Mourning dress | *Toga Pulla* (men), dark *palla* (women) | Mourning | §3 above, and §2's Palla entry |
| **Ancestor death masks** | *Imagines* | Funeral procession (*pompa funebris*) | A real, genuinely striking Roman funerary practice, and this document's own most direct expression of Design Pillar #7: wax masks of a family's own deceased ancestors, kept on permanent display in the household's Atrium (Ancestor Veneration & Funerary Customs), worn by hired actors who processed through the streets *as* those ancestors at a family funeral — a household's entire visible lineage, literally walking again for one afternoon. Not a Garment Slot for a living Character, but a real, named funeral-Activity element worth this document giving proper form, since so much of this project's own Memoria and Chronicle weight is exactly what this practice was invented to produce |

---

## 10. Regional & Cultural Dress

Organized by the same broad regional groupings Cultures of the Known World already uses, with several individually significant cultures broken out on their own this pass rather than left folded into a group entry, per direction to draw "from all over the cultures." Per Fashion & Dress §10, adopting or retaining these against the Roman default is a direct, visible Assimilated/Unbowed signal.

| Region / Culture | Signature Dress | Real Grounding |
|---|---|---|
| **Western Mediterranean & Northern Frontier** (Gallic, Germanic, British, Caledonian, Batavian) | *Bracae* (trousers) and a *sagum*-style cloak, real Celtic torcs and fibulae (§5), and — for British/Caledonian specifically — real, attested woad-based blue body dye/paint, distinct from any textile dye; genuine Udones socks (§4) for the whole Frontier population regardless of rank | The real Roman visual shorthand for "northern barbarian" dress, trousers specifically standing in real, sharp contrast to a toga-wearing Roman citizen |
| **Sicily** *(broken out this pass)* | A genuine, visible hybrid of Greek chiton/himation draping with a real, lingering Punic-Phoenician textile and jewelry tradition, reflecting the island's own centuries as a contested Greek-then-Punic-then-Roman cultural crossroads before this game's own range even opens | Sicily's own starting-region document already establishes this layered cultural history; this is its concrete dress expression, distinct from either the mainland Hellenic entry or the Punic entry alone |
| **Numidian / Mauri, Punic** | Light, practical riding dress reflecting Numidia's own real, famous light-cavalry tradition (Cultures §3); Punic dress retains a real, lingering Phoenician-Levantine textile tradition distinct from its North African neighbors | — |
| **Hellenic, Galatian, Cappadocian, Thracian, Dacian, Illyrian/Pannonian, Cretan** | *Chiton* and *himation* (draped rather than tailored, distinct from Roman toga-and-tunic construction), *peplos* for women, a *chlamys* military cloak, a broad-brimmed *petasos* travel hat; Thracian dress specifically includes a real, distinct *zeira* cloak; the *pallium* (§2) is this same draped tradition's own Roman-adopted form | The Hellenic world's own draped-garment tradition, genuinely older than and distinct from Roman dress despite centuries of mutual influence |
| **Judaean / Jewish** *(broken out this pass)* | Distinct religious dress conventions genuinely separate from the broader Levantine entry below, reflecting Cultures §6's own explicit note that Judaean culture is "categorically distinct" as this document's only monotheistic tradition — most notably real, attested ritual fringed garment edges (*tzitzit*) worn by observant men, a specific, textually-grounded practice worth this document naming directly rather than folding into a generic "Near Eastern robes" bucket | Distinguishing this entry honestly, per Cultures §6's own stated reasoning, from Syrian/Levantine polytheistic dress despite geographic proximity |
| **Syrian/Levantine, Nabataean, Cilician, Palmyrene** | Long, layered robes and head-coverings reflecting real regional Near Eastern practice, with Palmyrene dress specifically noted, real-historically, for a genuine hybrid of Aramaic, Arab, and Greco-Roman elements visible in surviving Palmyrene portrait sculpture | Palmyra's own real, attested hybrid material culture (Cultures §6) gives this entry unusually good, specific visual grounding |
| **Nabataean / Arabia Felix** *(Arabia Felix broken out this pass)* | Flowing desert dress suited to genuine incense-trade-route travel, real, elaborate gold jewelry reflecting South Arabia's own real historical wealth from the frankincense and myrrh trade, and fine imported textiles moving through the same caravan networks | Arabia Felix's own starting-region document already establishes this incense-wealth identity; this document gives it a concrete dress expression distinct from Nabataea's own narrower caravan-city entry |
| **Egyptian, Alexandrian Greek, Blemmyes** | The *kalasiris*, a real, distinctive close-fitting linen sheath dress, alongside genuinely elaborate Egyptian jewelry and cosmetic tradition (kohl eye-lining is real-historically an Egyptian practice Rome itself adopted, §13); Alexandrian Greek dress instead follows the broader Hellenic entry above, a real, visible marker distinguishing the city's own Greek elite from the native Egyptian population it lived alongside | — |
| **Nubia** *(broken out this pass)* | A genuinely distinct African elite dress tradition, real-historically noted for exceptionally rich gold jewelry (Nubia's own real, attested ancient gold wealth, a real historical rival to Egypt's own), vivid dyed textiles, and dress markers genuinely separate from Egyptian convention despite the two cultures' long shared frontier | Nubia's own starting-region document already establishes an Independent Kingdom identity outside the default Roman household; this document gives that kingdom's own elite a dress identity as distinct as its political one |
| **Danubian & Pontic Steppe (Sarmatian, Scythian-adjacent), Bosporan Kingdom** | Trousers and pointed caps in the steppe-nomad tradition, genuinely convergent with Parthian dress below despite no direct relation; Bosporan elite dress is real-historically a genuine, visible hybrid of Greek chiton/himation with steppe-tradition trousers and jewelry, reflecting that kingdom's own real hybrid Greco-Scythian identity | The Bosporan Kingdom's own starting-region document already establishes this hybrid identity in full; this is its concrete dress expression |
| **Armenia** *(broken out this pass)* | A genuinely distinct Great Power-adjacent dress tradition rather than simply "Parthian-adjacent" — real, elaborate tall pointed headdresses (a distinct Armenian royal/noble convention) alongside sleeved tunics and trousers in the broader Eastern tradition, reflecting Armenia's own real, precarious position as a buffer culture between Rome and Parthia rather than a client of either | Armenia's own starting-region document already establishes a distinct Great Power Allegiance mechanic replacing Reputation Duality outright; this document gives that distinct political status a distinct dress identity to match, rather than folding Armenia into Parthia's own entry as the prior pass did |
| **Parthia** | Sleeved tunics and trousers rather than draped Mediterranean dress, often with a soft peaked or domed cap — a real, visually striking East-vs-West contrast Roman writers themselves commented on directly | A genuine, concrete visual shorthand for Parthia's own Great Power status distinct from any client or provincial culture |
| **Beyond the Frontier (Indian, Chinese, Garamantian, Aksumite, Taprobane, Sogdian)** | Genuinely exotic, rarely-seen dress by Roman standards — Chinese silk robes chief among them, the real source material behind the Silk import good itself (Resources & Goods §7) | A Character from one of these six cultures is, per Cultures §10.7, already a rare event in its own right; their dress should read as genuinely unfamiliar rather than a minor regional variant |

**A real, worth-naming historical footnote on Silk specifically:** fine, near-transparent silk garments were a real, attested source of Roman moral anxiety — several real Roman moralists (Seneca among them) wrote disapprovingly of silk's own revealing quality, especially on women. This document flags Silk's own dress use as a legitimate, historically grounded Scandal-adjacent flavor source (Fashion & Dress §13) distinct from a straightforward Sumptuary Edict violation, since the real objection was about propriety and modesty rather than cost alone.

---

## 11. Bathing, Athletics & Leisure Dress

New this pass: a genuine gap, given how central the Balneum/Bathhouse already is to this project's own social and hosting mechanics (Villa §9, Buildings §4.3) without ever having its own dress-and-accessory layer.

| Item | Real Name | Notes |
|---|---|---|
| **Bathing wrap** | — | A simple linen wrap worn to and from the bath itself, distinct from ordinary Everyday dress |
| **Oil flask** | *Aryballos* | A real, small flask (often carried on a wrist strap) holding scented oil, applied before exercise or bathing and then scraped off along with sweat and dirt |
| **Skin scraper** | *Strigil* | A real, curved bronze tool used to scrape oil and grime from the skin after exercise or in the bath — among the most recognizable everyday objects surviving from Roman material culture, and a natural small Villa/Balneum flavor prop |
| **Athletic wrap** | *Endromis* | §2 above — worn after exercise specifically to prevent a chill, real-historically attested in Roman satire (Juvenal) as a genuinely fashionable garment in its own right, not merely practical kit |
| **Bathing sandals** | — | Simple wooden-soled sandals suited to wet floors, distinct from the everyday Soleae (§4) |

---

## 12. Arena & Circus Dress

New this pass, and a direct, concrete dress-specific companion to Games & Spectacle's own already-established gladiator fighting styles and racing factions — this document adds no new fighter types or race mechanics of its own, only their physical dress.

| Item | Notes |
|---|---|
| **Murmillo armor** | Heavy armor, a large rectangular shield, and a distinctive fish-crested helmet — Games & Spectacle §3.3's own heavy, Strong/Herculean-rewarding style, given its real physical form here |
| **Retiarius gear** | Famously minimal armor, a weighted net and trident, and a distinct shoulder guard (*galerus*, a different real object from the priestly Galerus of §7 despite the shared name) — that document's own fast, Nimble/Perceptive-rewarding style |
| **Thraex armor** | A curved short sword (*sica*), a small shield, and tall greaves — that document's own curved-blade style |
| **Secutor helmet** | A real, distinct smooth, rounded helmet design specifically built to give a Retiarius's net nothing to catch on — the real, historically attested style-counter matchup Games & Spectacle §3.3 already names directly |
| **Charioteer's wrapped leathers** | A real, historically attested practice: chariot racers wound leather straps tightly around their own torso for a measure of crash protection, since a charioteer drove with the reins actually tied around their body rather than held loosely in-hand — a genuine, concrete reason racing was as dangerous as Games & Spectacle §6.3's own Crash/DNF and Fatal Crash outcomes already reflect |
| **Faction colors** | Fashion & Dress §5's own named Garment Slot category — Red, White, Blue, Green (Games & Spectacle §6.1), worn by charioteers, their supporting teams, and any fan wealthy or devoted enough to display allegiance in daily dress |

---

## 13. Hair, Wigs, Cosmetics & Grooming Tools

Concrete named items behind Fashion & Dress §3's own hair/cosmetics layer.

| Item | Real Name | Notes |
|---|---|---|
| **Hairnet** | *Reticulum* | A real, often gold-thread hairnet worn by elite Roman women, restraining an elaborate style |
| **Hairpins** | *Acus Crinalis* | Real decorative pins, frequently bone, bronze, or gold, used to secure elaborate braided or piled hairstyles |
| **Curling iron** | *Calamistrum* | A real, heated iron rod (heated in ashes) used by an *ornatrix* to curl hair — the physical tool behind that Companion role Fashion & Dress §3 flags for Companions & Court Positions |
| **Wig** | — | Fashion & Dress §3's own named import item — real, attested Germanic/Gallic blonde hair specifically prized for elite Roman wigmaking |
| **Kohl** | — | Real eye cosmetic, historically an Egyptian-origin practice (§10) widely adopted across the Roman world |
| **Cerussa (white lead)** | *Cerussa* | Fashion & Dress §3's own named Health-risk cosmetic — real, popular, and real-historically toxic |
| **Rouge** | — | Red ochre or (for the wealthiest) more exotic pigments, used on cheeks and lips |
| **Perfumed oil** | *Unguentum* | A real, genuine luxury good in its own right, distinct from bathing oil (§11) — scented specifically for personal wear rather than utility, and a real, small Romance & Seduction-adjacent accessory (a gift, or a deliberate seduction tool) |
| **Tweezers** | *Volsella* | A real Roman grooming tool; elite body-hair removal is a genuine, attested Roman elite practice (noted, often mockingly, by real Roman satirists), included here factually and lightly rather than dwelt on |
| **Hand mirror** | *Speculum* | A real, polished-metal mirror — a small, concrete personal accessory rather than a Garment Slot in its own right, but worth naming for flavor and Cubiculum decoration purposes (Villa §6) |
| **The Barber** | *Tonsor* | Not an item but the real service role behind shaving, haircutting, and much of this section's own grooming — a natural, small addition to Companions & Court Positions' own staff roster alongside the Ornatrix Fashion & Dress §3 already flags |

---

## 14. Fabrics, Dyes & Color Symbolism

A quick-reference layer tying this document's garments back to Buildings' own named goods (§4.6 of that doc) and giving color a real, legible symbolic register rather than leaving it purely cosmetic.

| Fabric/Dye | Source | Symbolic/Status Reading |
|---|---|---|
| **Wool** | Domestic Pasture → Weaver's Loom | The universal baseline fabric across every tier |
| **Linen** | Flax Field → Linen Works | Cooler, lighter, favored in warmer regions (Egypt especially, §10) and for undergarments generally |
| **Silk** | Imported only (Resources & Goods §7) | The single highest luxury/status fabric in the game, and a real, direct Sumptuary/Scandal flavor source (§10) |
| **Cotton** | Imported, minor (Resources & Goods §7.5) | A minor curiosity good per that document's own framing; not a textile pillar |
| **Felt** | Processed wool, Weaver's Loom | The specific material behind the Pileus (§8) and the Apex/Galerus (§7) — worth naming since it's mechanically distinct from woven cloth |
| **Tyrian Purple** | Murex harvest → Dye Works | The single most legally and socially loaded color in the entire roster — see Fashion & Dress §5 in full |
| **Common/Woad Dye** | Woad Plants → Dye Works | The ordinary, unrestricted dye alternative; also, as a body paint rather than a textile dye, a real British/Caledonian cultural marker (§10) |
| **Saffron/Crocus Yellow** | A minor, specific dye good | The real, traditional color of the *flammeum* bridal veil (§9) — worth a small, dedicated color note given how visually central that one garment is |
| **Undyed natural cream/white** | Base wool or linen, undyed | The Toga Virilis and Toga Candida's own base state (§3) — plainness itself carrying real specific meaning depending on context |
| **Dark/black (undyed dark wool)** | Base wool, unbleached darker grades | The Toga Pulla and general mourning dress (§9) |
| **Amber** | Imported (§5), Northern Frontier trade | A jewelry material rather than a fabric, but included here as this pass's own new luxury-goods addition, distinct from Mediterranean gemstone supply |

---

## 15. Outfit Tier Reference — What Each Tier Actually Looks Like

Concrete, illustrative combinations for Fashion & Dress §2's own five-tier scale, so the abstract tier reads as something specific at a glance. Illustrative, not prescriptive — the actual combination varies by Legal Status, culture, and Occasion.

| Tier | A Citizen's Version | An Enslaved/Poor Character's Version |
|---|---|---|
| **Meager** | A single worn, undyed tunic, bare feet or simple sandals | Ergastulum-tier dress (§8), bare feet, no jewelry |
| **Modest** | A clean but plain wool tunic, simple sandals, minimal jewelry | A basic but adequately maintained tunic (Labor & Slavery's Adequate Regimen tier made visible) |
| **Respectable** | A proper toga for formal Occasions, calcei, a modest ring | Livery-tier dress if the household maintains one (§8, Fashion & Dress §8); otherwise unusual for this population |
| **Fine** | Good-quality dyed cloth, real Jewelry, a well-kept toga, an ornatrix-styled hairstyle for a woman of the household | Reserved for a favored, trusted individual specifically elevated by the player; a newly-freed Character's first Pileus (§8) is a natural Fine-tier milestone moment even if the rest of their Wardrobe hasn't caught up yet |
| **Opulent** | Tyrian Purple trim (Sumptuary-flagged if worn without clearance), a full Jewelry set, a wig, cerussa cosmetics, calceus senatorius if the Social Class applies | Essentially never applies to an enslaved Character under any ordinary circumstance |

---

## 16. Portrait Rendering — The Wardrobe as a Layered Paperdoll

New this pass, and the direct mechanical answer to the *Free Cities* comparison the direction calls out specifically: that game's own character models are assembled procedurally from stacked, swappable art layers, and a Character's generated portrait (Core §7.11) works the same way here — every entry in this roster is tagged with a real, fixed **Render Layer**, and a Character's current outfit (Fashion & Dress §4's Occasion-resolved selection) is simply the stack of layers actually active at that moment. Changing the Occasion, the Household Dress Policy, or a single Garment Slot doesn't require a bespoke new portrait — it swaps one layer in the existing stack, exactly the way *Free Cities* itself never needed a whole new sprite for a new shirt.

### 16.1 The Layer Stack, Bottom to Top

| # | Layer | What Lives Here | Examples |
|---|---|---|---|
| 0 | **Body** | The Character's own fixed Appearance attributes (Familia §2.4/Core §7.11) — build, complexion, natural hair/eye color. Never a Garment Slot; everything above is drawn over this base | — |
| 1 | **Hair (Base)** | The Character's own styled hair, per Fashion & Dress §3 | A given hairstyle, §13 |
| 2 | **Torso-Base** | The foundational body garment | Tunica, Kalasiris (§10), Chiton |
| 3 | **Legs** | A distinct layer that renders as *empty* (bare legs under a tunic hem) for most Roman-culture Characters, and as a genuine, visible garment for several others — the single most immediate visual culture-marker this system has | Bracae/trousers (Gallic, Germanic, British, Parthian, Steppe cultures, §10) — a Roman-culture Character and a Parthian-culture Character standing side by side read as visibly different at this layer alone, before a single accessory is added |
| 4 | **Waist** | Belts, worn at the torso-base layer's own waistline | Cingulum Militare, Balteus (§2, §6) |
| 5 | **Torso-Outer / Formal** | The single most information-dense layer in the whole stack — this is where Legal Status, Social Class, and office are read at a glance | Any Toga variant (§3), Stola, Palla, Synthesis, Pallium |
| 6 | **Cloak/Back** | A distinct overlay layer, since a cloak is worn *in addition to*, not *instead of*, the Torso-Outer layer | Paenula, Sagum, Lacerna, Birrus, Paludamentum |
| 7 | **Footwear** | — | Calcei, Calceus Senatorius, Caligae, Soleae, Udones-plus-sandals (§4) |
| 8 | **Headwear** | Sits above the Hair layer, and in several cases replaces its visibility entirely | Apex, Galerus, Suffibulum, Phrygian cap (§7), a Wig (§13, which itself can be flagged to *override* Layer 1 rather than stack above it) |
| 9 | **Neck/Chest Accessory** | — | The Bulla, necklaces, the Infula (§5, §7) |
| 10 | **Hand/Finger Accessory** | — | Rings of every kind (§5) |
| 11 | **Cosmetic Overlay** | Not a physical object but a real, distinct rendering pass over the Body layer's own face region | Kohl, Cerussa, Rouge (§13) |
| 12 | **Status Overlay** | The topmost layer, reserved for earned or awarded items that should always read clearly regardless of what's underneath | Dona Militaria (§6), Livery insignia/color trim (Fashion & Dress §8), racing Faction colors (§12) |
| — | **Held Prop** | Not worn, and therefore not part of the paperdoll stack proper, but flagged for scene-specific rendering | The Lituus, a Strigil, an Aryballos (§7, §11) |

### 16.2 Wear Condition — Outfit Tier as a Visual, Not Just a Numeric, Signal

A single Garment rendered at different Outfit Tiers should not look identical with a different number attached — *Free Cities* itself ties clothing condition (and quality) directly into what the model actually shows, and this document does the same: each layer's rendered asset carries a **Wear Condition** read directly off the wearer's current Outfit Tier — a Meager-tier Tunica renders visibly patched, frayed, or faded; the same Tunica at Fine or Opulent tier renders clean, well-dyed, and closely fitted. This applies within a single garment type rather than requiring a separate item for every tier — a Toga is a Toga at every tier (§3's own gates don't change), but a Meager-tier citizen's toga and a Fine-tier citizen's toga should never be mistaken for each other at a glance.

### 16.3 Cultural and Regional Layer Substitution

§10's regional dress table is, under this rendering model, really a set of **layer substitution rules**: a Character's culture tag doesn't add a separate system, it simply determines which real-world asset fills Layers 2, 3, 5, and 6 by default before any individual Garment Slot choice overrides it — a Hellenic-culture Character defaults to a Chiton/Himation filling Layers 2 and 5 rather than a Tunica/Toga, a Gallic-culture Character defaults to Bracae actually populating Layer 3 rather than leaving it empty. Fashion & Dress §10's own Assimilated/Unbowed signal is, visually, nothing more or less than which culture's default asset set a given Character is actually rendering with at any given time — switching it is exactly as legible on the portrait as it is in the underlying Trait.

### 16.4 What This Deliberately Does Not Do

Consistent with this project's own restraint around sexuality and the body (Romance, Sexuality & Lineage's "described rather than depicted" standard), this layer system exists to communicate **status, wealth, culture, and office** — never a body-exposure or arousal state the way some paperdoll systems in this genre also track. There is no "layer removed" escalation ladder modeled here; a bathing Occasion (§11) renders its own specific, modest bathing-wrap asset rather than an absence of Layer 2/5 coverage. The system's entire purpose is legibility of standing, not undress.

---

## 17. Cross-System Integration

- **Fashion & Dress:** this entire document is that document's own Garment Roster — every `garmentId` referenced there resolves to an entry here.
- **Military & Combat:** §6's Dona Militaria closes a real gap — that document specifies rank and battlefield outcomes but never gave earned distinction a physical, wearable form until now; the Paludamentum and Cingulum Militare are this document's own concrete visual expression of command and of a soldier's own standing respectively; the Corona Graminea is this document's own rarest, Legendary-tier addition to that honor system.
- **Games & Spectacle:** §12 gives that document's own already-named gladiator fighting styles (Murmillo, Retiarius, Thraex, Secutor) and racing factions their concrete physical dress, without altering any of that document's own resolution mechanics.
- **Religion:** §7's Apex, Galerus, Infula, and Suffibulum are the physical objects behind that document's own Flamen/Augur/Pontifex/Vestal offices; the Salii and the Lituus give two further named offices a concrete visual identity they didn't have before.
- **Correspondence & Letters:** the Signet Ring (§5) is this document's concrete sealing-and-authentication object.
- **Romance, Sexuality & Lineage:** the Anulus Pronubus (§5) is the physical object behind that document's own betrothal mechanic; the Flammeum and Seni Crines (§9) are its own wedding-day dress contribution; Unguentum (§13) is a small, real seduction-adjacent accessory.
- **Labor & Slavery:** the slave collar (§8) is a real, direct, deliberately restrained expression of that document's own flight-risk and recapture mechanics (§7 of that doc); the Pileus (§8) is this document's own direct, hopeful dress expression of that system's manumission mechanic.
- **Cultures of the Known World:** §10 maps every one of that document's regional groupings onto a real, distinct dress identity, with Armenia, Arabia Felix, Nubia, Judaea, and Sicily now broken out individually rather than folded into a broader neighbor entry.
- **Politics & Patronage, Merchant Families:** the Toga Candida, Toga Praetexta, laticlavus, and angusticlavus (§3, Fashion & Dress §5) are this document's own physical objects behind the cursus honorum and equestrian-order mechanics respectively.
- **Roman Calendar:** Liberalia (§3, §9) and the Salii's own seasonal processions (§7) are two concrete, dateable dress-linked Calendar events.
- **Ancestor Veneration & Funerary Customs:** the Imagines (§9) are this document's own direct, named expression of that system's own household-memory practice — a genuinely central piece of Design Pillar #7 given its proper physical form.
- **Villa:** §6's Cubicula personalization pattern (trait-driven default, player-overridable) is the direct template Fashion & Dress §7 reuses for free-family dress preference; the Dactyliotheca (§5) and Balneum (§11) are natural Villa decoration/room-flavor ties.
- **Buildings & Production Chains:** §14's fabric/dye table ties every garment in this document back to a real, named production-chain good; Felt and Amber are small, genuine additions to that document's own goods list.
- **Resources & Goods:** Silk's own real historical moral-anxiety footnote (§10) gives that document's own import good a genuine social-consequence dimension beyond price and rarity; Amber (§5) is a new, concrete Northern Frontier luxury import.
- **Education & Culture:** the Pallium (§2) is this document's own concrete dress expression of that document's philosophical-school identity, and a real, legible alternative to the toga for an intellectually-coded Character.
- **Companions & Court Positions:** the Ornatrix and Tonsor (§13) are natural, small additions to that document's own staff roster.
- **Scandal:** §10's Silk footnote and §3's Toga Candida/Praetexta impersonation risk are both concrete, named trigger sources for that system's shared aftermath engine.
- **Dynasty Chronicle (§6.11, future):** the Toga Picta (§3), the Pileus (§8), the Imagines (§9), and any of §6's Dona Militaria — especially the Corona Civica and the exceptionally rare Corona Graminea — are all guaranteed top-tier Chronicle material.
- **Religions of the Known World:** the Phrygian cap gives the Cult of Mithras (§6 of that doc) a concrete, historically real visual marker it didn't have before.
- **Core / Appearance & Portraiture (§7.11):** §16 in full is this document's own direct answer to that system's rendering needs — every Garment entry's Render Layer tag is a real, usable input rather than an abstract "status-appropriate dress" placeholder.

---

## 18. Data Model

```
Garment {                    // resolves Fashion & Dress's own garmentId references
  garmentId,
  name,                      // real Latin/regional term where one exists
  category,                  // "everyday" | "formal" | "ceremonial" | "military" | "mourning" |
                              // "athletic" | "cultural" | "milestone" | "arena"
  gate,                      // "citizenship" | "socialClass" | "office" | "eventRecord" |
                              // "culturalOrigin" | "militaryValor" | "none"
  outfitTierRange,           // e.g. ["fine", "opulent"] — the tiers this piece is plausible within
  sourceGood,                // pointer to the Buildings & Production Chains good it's made from, where applicable
  historicalNote,             // flavor text field — several entries above carry real, specific grounding worth preserving verbatim
  renderLayer,                // §16.1 — 0-12, or "prop" for a Held item; fixes stacking order
  overridesLayer,              // nullable — e.g. a Wig sets this to 1 (Hair Base) to flag full replacement rather than stacking
  wearConditionScales: bool,     // §16.2 — true for ordinary cloth/leather goods, false for fixed-appearance items (Dona Militaria, Jewelry) that don't visually degrade with Outfit Tier
  culturalDefaultFor: [...],      // §16.3 — cultureIds for which this Garment is the automatic Layer default absent an override
}

DonaMilitariaRecord {          // §6
  characterId,
  decoration,                 // "torques" | "armillae" | "phalerae" | "coronaCivica" |
                              // "coronaMuralis" | "coronaNavalis" | "coronaGraminea"
  awardedForActionId,          // pointer to the triggering Military & Combat engagement/outcome
  month,
}

CulturalDressProfile {          // §10 — a lightweight lookup, not a per-Character record
  cultureId,                   // matches Cultures of the Known World's own culture tag
  regionalGroup,
  signatureGarmentIds: [...],
}

ManumissionDressEvent {         // §8, new this pass — a small, dedicated record for the Pileus moment
  characterId,
  manumissionRecordId,          // pointer to Labor & Slavery's own manumission record
  pileusGranted: true,
  chronicleEligible: true,
}
```

---

## 19. Open Questions

- **All numeric sizing**, per this project's standing convention — no Dignitas, cost, or Health-risk magnitude is specified for any individual item in this roster; those all resolve against Fashion & Dress's own already-deferred numbers.
- **Dona Militaria stacking and display.** §6 doesn't specify whether a Character who earns multiple decorations displays all of them simultaneously during a Formal Occasion, or whether a portrait/rendering practicality caps the visibly-displayed set — an implementation question more than a design one.
- **Regional dress granularity below the group level.** §10 now breaks out Armenia, Arabia Felix, Nubia, Judaea, and Sicily individually; whether any further culture (Dacian gold-wealth dress, or Cilician's own distinct maritime identity) eventually warrants the same treatment is left for a future pass.
- **The slave collar's actual trigger condition.** §8 ties it to "a prior flight attempt on record" without specifying whether it's automatic after a first attempt, a player-chosen Punishment option, or scales with repeated attempts — left to Labor & Slavery's own eventual numeric pass.
- **Whether Dona Militaria items are ever revocable.** Real Roman practice allowed decorations to be stripped alongside a dishonorable discharge (mirroring the Cingulum Militare's own removal); this document doesn't specify whether that's modeled as a distinct mechanic or left as unaddressed edge-case territory.
- **Whether the Imagines (§9) require a minimum number of prior Chronicle-recorded ancestors before a funeral can stage one at all.** A first-generation household, by definition, has no ancestors to represent — whether this is simply a natural, self-solving absence or needs an explicit gate is left to Ancestor Veneration & Funerary Customs' own eventual revisit.
- **Ornatrix and Tonsor formalization**, carried forward from the parent document's own open question — both remain suggested additions to Companions & Court Positions' own roster rather than added there directly by this document.
- **Actual asset/art production for the Layer Stack (§16).** This document specifies the stacking logic, the tagging scheme, and the cultural-substitution rule; it does not specify how many actual visual variants must be produced per layer per culture per Wear Condition tier — a real, substantial content-production question for whichever team eventually builds the rendering pipeline, outside this document's own design-only scope.
- **Whether Wear Condition (§16.2) is a continuous slider or a small fixed set of discrete visual states per garment.** This document assumes discrete states (one visual asset per Outfit Tier band) as the more production-realistic default, but doesn't rule out a finer continuous treatment if the eventual art pipeline supports it cheaply.
