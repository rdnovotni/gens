# GENS — System Design: Starting Regions — Italian Heartland (Latium & Campania)
*Final polish and balance pass. Fills in the Starting Regions framework's Region Profile schema (§4) for both halves of the Italian Heartland split — Latium and Campania — with fuller Gazetteers, a fourth rival house per region, concrete Distance Tiers connecting both regions to Rome and to the rest of the launch roster, a Historical Timeline Hooks section giving Vesuvius and the Social War real Events-system teeth, and a Templated Backgrounds tie-in for each region. This pass corrects two inconsistencies the expansion pass introduced — an overlapping Templated Background claim between the two regions (§3.11, §4.11), and a Gazetteer Tier assigned to Puteoli that overstated Campania's real administrative status — and is intended as the finalized version of this document pending only the numeric balancing pass every design document in this project defers to the end.*

---

## Contents

1. Why Split Italian Heartland in Two
2. Shared Italian Identity
3. Latium
4. Campania
5. Rome — The Capital
6. Distance & Travel — The Italian Heartland in Context
7. Historical Timeline Hooks — Divergence-Eligible Moments
8. Cross-System Integration
9. Data Model
10. Open Questions

---

## 1. Why Split Italian Heartland in Two

The core doc's original one-line sketch — "high prestige ceiling, high land cost, dense political competition, easy access to Rome" — is entirely true of Latium and only partly true of Campania. Campania's real historical identity runs in a different direction: less about Senate proximity and more about trade wealth (Puteoli was Italy's busiest port for most of the Republic and early Principate), agricultural richness (*Campania Felix* — "fertile Campania" — was a real ancient name for the region, not a flattering exaggeration), a genuinely layered multicultural population (Oscan/Samnite substrate, real Greek civic identity at Naples and Cumae going back centuries before Rome ever took an interest), and a real, singular, catastrophic risk the rest of the Italian Heartland simply doesn't share: Vesuvius.

Splitting them gives each a coherent identity instead of averaging two into a blur, and — per Design Pillar #1 — gives the launch roster a second real internal tradeoff pair, alongside the Iberian/North African split: **Latium trades economic security for political proximity; Campania trades political centrality for economic richness and cosmopolitan texture, at the cost of carrying the single most dramatic tail risk in the entire regional roster.**

This expanded pass leans further into that contrast wherever it can — not just at the level of "one is political, one is economic," but down into specific goods, specific Gazetteer towns, and a specific pair of real historical flashpoints (§7) that give each region its own distinct texture of risk.

---

## 2. Shared Italian Identity

Both regions share a few things worth stating once rather than twice:

- **No Reputation Duality.** Per the framework's §6 table, neither Latium nor Campania uses the Reputation Duality split — there is no "local, non-Roman" populace to hold a second axis of standing with here the way there is on an actual frontier. Both regions' populations are overwhelmingly enfranchised, long-settled, and legally Roman or Latin-rights by default (Familia's Legal Status mechanics), even where real cultural minority threads persist underneath that status (§3.7, §4.7).
- **No standing Frontier neighbor.** Diplomacy with Non-Roman Peoples has nothing to anchor to here — there is no adjacent non-Roman people, hostile or otherwise, within either region's own borders. Both regions' "outward-facing" systems point toward the sea and toward Rome's own institutions rather than toward a land frontier.
- **No standing Legionary garrison.** Under Pax Romana specifically, legions are frontier-stationed, not posted in Italy itself — a real, deliberate historical fact both region profiles below lean into rather than paper over with an anachronistic in-region legion. Where a military flavor is wanted, both regions reach for a different, more accurate hook (§3.4, §4.4).
- **Era flexibility.** Consistent with the core doc's own note that region and era stay decoupled, nothing in either profile below hard-locks to a single specific building or event date (a Pax Romana-era Colosseum reference, for instance, is deliberately avoided) — both regions describe Rome and its surroundings in terms that hold up whether a given playthrough starts in the late Republic or the early Dominate.
- **A shared "home continent" relationship to risk.** Neither region faces raiding, tribal warfare, or Diplomacy failure states the way every other launch region does in some form — but per §7, that doesn't mean either region is *safe*. It means each region's real danger is a different shape: Latium's is slow and structural (supply dependency); Campania's is rare and catastrophic (Vesuvius). Design Pillar #1 holds even here, where the "no dominant setting" tension has nothing to do with tribes or armies at all.
- **Both regions read Legal Status distinctly from a frontier region.** Because there's no colonial administration layer here, Legal & Court's Peregrine-status mechanics apply differently in each region: Latium approaches "everyone is simply a citizen" (§3.3), while Campania is this project's best illustration of a Legal Status mix that's layered and historically real without being a colonial-administration story at all (§4.3) — a useful contrast for a future Legal & Court pass to have on hand.

---

## 3. Latium

### 3.1 Terrain & Feature Profile

Fertile river-plain terrain along the Tiber, limited but real coastal access near Ostia, and rolling hill country inland (the Alban Hills) — a genuine terrain mix, but one with almost no untouched land left in it: this is the most intensively, continuously cultivated ground on the entire roster. No mineral deposits of note; Latium's own wealth was never built on extraction.

A further terrain note worth adding this pass: Latium's own coastal fringe near Ostia carries a real historical **salt-pan** tradition — the *Via Salaria*, the "Salt Road," began at Rome specifically because of salt harvested from pans near the Tiber's mouth, predating even Rome's own rise to prominence. This gives Latium a small, genuine second coastal industry alongside Ostia's own grain-import role (§3.2), and a direct link to Resources & Goods' existing Salt Pans building (Coast-gated, per Buildings §3).

### 3.2 Economic Package

*(Qualitative — numeric packages remain Start Modes' own territory, per the framework's §4.2.)* The most expensive land on the launch roster, and the least room to expand into raw new acreage — a Latium start is fundamentally about maximizing an already-limited, already-valuable plot rather than growing outward the way a frontier start can. Markets here are the deepest and most liquid in the game, simply by virtue of proximity to Rome's own economic gravity.

**A real, deliberate vulnerability worth naming directly:** Rome's population, by the high empire, had long since outgrown what Latium's own farmland could feed — the historical Cura Annonae grain dole existed precisely because Italy, Latium very much included, depended on grain shipped in from Sicily, Africa, and above all Egypt. A Latium household's own economic security is thus real but genuinely conditional: wealthy and stable in ordinary times, but exposed to the same supply shocks (a bad harvest abroad, a disrupted grain fleet) that could unsettle the capital itself. This is Latium's own answer to a frontier region's raid risk — the danger here isn't a raiding party, it's a bad month for the Ostia grain fleet.

**A second, denser-population economic wrinkle, new this pass:** Latium's own land scarcity makes urban rental income (Buildings' Insulae, per its Housing function type) a genuinely more central part of a Latium household's economic identity than it would be almost anywhere else on the roster — a household here is more likely to hold income-generating urban property alongside its agricultural land than a Gallic or Iberian household would be, simply because Latium is where Rome's own housing pressure is most acutely felt.

### 3.3 Political & Legal Texture

The purest expression of "dense political competition" on the roster: Curia elections (Politics & Patronage §5) here are the most heavily contested in the game, and — per the shorter travel distance to Rome itself (§6) — a Latium household clears the cursus honorum's "noticed by Rome" gate (Politics & Patronage §6) faster and more readily than any other region's household could, all else equal. Legal Status here skews almost entirely Roman citizen; Latin Rights and Peregrine status are genuinely rare in this region specifically, a hard contrast with Campania's own more layered population (§4.3).

**A real historical wrinkle worth folding in this pass:** Latium's own "everyone is basically a citizen" simplicity wasn't always true. Before the real Social War (91–87 BC) extended full citizenship across Italy, many of Latium's own old Latin-League towns — the very towns this document's Gazetteer names (§3.8) — held only Latin Rights rather than full citizenship, a genuine, historically real point of tension between "Roman" and "Latin" identity that predates this game's own default Pax Romana setting but remains available as a real Scenario Start or Divergence hook (§7.2) for a playthrough set earlier in the timeline.

### 3.4 Diplomatic & Military Exposure

No Frontier neighbor, per §2. Latium's military flavor instead runs through **patronage-based officer recruitment** — a Latium household's own connections (Clientela, Politics & Patronage) give its sons a real, concrete edge at securing a prestigious military tribune's commission or a staff posting under a governor, rather than ordinary legionary recruitment. Security flavor at home leans on Rome's own urban cohort and Praetorian presence rather than a legion, reflecting the real historical fact that Italy's internal order was kept by these specifically urban forces, not by the frontier-postured legions.

### 3.5 Religious & Cultural Defaults

Latium's default is the Roman state religion at its most concentrated and traditional — the natural home turf for the **Mos Maiorum** Household Doctrine (Policies & Edicts §3). Real minority religious texture exists but is genuinely faint here rather than a live cultural force: the old Etruscan tradition of *haruspicy* (reading the liver of a sacrificed animal) is real-historically an Etruscan import into Roman practice, still traceable in Religion's own Haruspex specialist role, even though Etruscan culture itself is, per Cultures of the Known World, fully absorbed by this game's era and survives only as this kind of residual religious influence.

A further, purely flavorful thread: several of Latium's own Gazetteer towns (§3.8) — Tusculum, Praeneste, Alba Longa, and, new this pass, Lavinium and Gabii — were real independent members of the ancient Latin League, older than Rome's own dominance over the region. They carry no distinct mechanical culture tag of their own (they're thoroughly Roman by this game's era), but a region document can and should let that history live in the Gazetteer's own grounding notes rather than pretending Latium's political map was always simply "Rome and its suburbs."

**New this pass — the Vestal connection.** Rome's own state hearth-cult, the Vestal Virgins (Religion's existing Vestal mechanics), is administratively a Rome-specific institution rather than a Latium-wide one, but a Latium household — by virtue of proximity and prestige — is realistically the most likely household on the entire regional roster to have a daughter genuinely considered for Vestal service, a real, historically accurate point of pride (and real, historically accurate burden — Vestal service meant decades removed from ordinary marriage prospects) worth flagging as a distinctly Latium-flavored Familia storyline rather than a generic one.

### 3.6 Regional Goods & Trade

Latium's production identity is **wine, olive oil, salt, and building stone** — real Alban-hills viticulture (a real, distinct if less internationally famous appellation than Campania's Falernian), olive groves, the Via Salaria's own salt tradition (§3.1), and *peperino*, a real volcanic tufa stone quarried in Latium and used in Roman construction since the earliest Republic, giving Latium its own building-stone identity distinct from Campania's own Pozzolana-driven concrete industry (Resources & Goods §7). Ostia's own role as the empire's grain-import gateway (§3.2) makes Latium's trade identity as much about *moving* goods — especially grain it doesn't produce enough of itself — as producing them.

### 3.7 Population & Culture Distribution

Overwhelmingly Roman/Latin by weight, per §3.3 — the least culturally mixed region on the launch roster, which is itself the point: Latium is what "the Roman mainstream, unmixed" looks like, a deliberate contrast point against every other region's own real cultural layering. A relative-weight read (descriptive tiers, not numeric percentages, per this project's standing convention):

| Culture | Presence |
|---|---|
| Roman/Latin | Dominant |
| Etruscan (residual, religious-influence only) | Rare, and cultural rather than demographic — see §3.5 |
| Any other culture (Greek tutors, traders passing through Ostia, etc.) | Rare, individual-level outliers only, never a settled community |

Real outliers remain possible (per the framework's own standing rule that no distribution is ever exclusive) but skew toward individuals rather than communities — a single Greek tutor, a single Egyptian trader passing through Ostia — rather than any settled minority population of real size.

### 3.8 Gazetteer

Expanded this pass with two further real Latin-League towns, giving Latium's own founding-myth texture (§3.5) more than one Gazetteer entry to live in.

| Location | Role(s) | Tier | Grounding |
|---|---|---|---|
| **Ostia** | Major Port | Regional Center | Rome's own real port, and specifically the empire's grain-import gateway — the physical location where §3.2's own vulnerability actually lands. Also the real home of Latium's own salt-pan tradition (§3.1). |
| **Tusculum** | Market Hub | Regional Center | A real, favored country-villa retreat for Rome's senatorial class (a real, well-documented villa culture existed here) — a natural, elite-flavored Home Anchor candidate (§3.10). |
| **Praeneste** | Sanctuary | Regional Center | Home to the real, genuinely major Temple of Fortuna Primigenia — one of the largest religious complexes in the ancient Italian world, a real Religion-system destination in its own right. |
| **Tibur** | Market Hub | Regional Center | Modern Tivoli — another real elite retreat town, kept deliberately generic here so it stays plausible across this project's own flexible era range rather than pinned to one emperor's reign. |
| **Antium** | Major Port | Regional Center | A real coastal town with its own genuine elite-villa history and port function, giving Latium a second, lesser coastal outlet beyond Ostia. |
| **Alba Longa** | Sanctuary | Outpost | The real, legendary mother-city of Rome itself in Roman foundation myth — mechanically minor, but an unmatched Dynasty Chronicle and Religion flavor location precisely because of how symbolically loaded the name already is. |
| **Lavinium** *(new)* | Sanctuary | Outpost | A real, genuinely ancient Latin town with its own foundational role in Roman myth (traditionally linked to Aeneas), and a real historical seat of shared Latin League religious ritual — a second, distinct mythic-weight Sanctuary alongside Alba Longa rather than a redundant copy of it. |
| **Gabii** *(new)* | Market Hub | Outpost | A real, genuinely ancient Latin town, notable historically for retaining a distinct local ritual/augural tradition of its own even after full absorption into Rome's orbit — a small, honest illustration of §3.3's own "Latin vs. Roman" historical texture. |

### 3.9 Rival Seeding

Expanded this pass with a fourth house, giving Latium's own political field a genuine range from ancient-and-declining to newly-arrived rather than just old-money-vs-new-money.

- **Gens Fabricia** *(Rome — see §5)* — an ancient, deeply entrenched patrician house with a long, real Senate presence and a starting Household Doctrine already leaning Mos Maiorum; views newly-risen wealth, however legitimately earned, as inherently vulgar. High starting Dignitas, low starting warmth toward a climbing player household.
- **Gens Octavinia** *(seated at Tusculum)* — equestrian-money climbers, aggressively courting marriage alliances upward into the old patrician tier; a real, live rival for any marriage prospect a player household is also pursuing.
- **Gens Sergiana** *(seated at Praeneste)* — an old patrician name in real, quiet decline: strong Dignitas, weakening Net Worth, precisely the mirror case Politics & Patronage's own cursus honorum gate describes (§6 of that document). A natural rival for prestige, but also a plausible marriage-alliance target for a player household with the dowry to offer and the patience to absorb an old name's debts.
- **Gens Considia** *(seated at Gabii, new)* — a comparatively recent arrival to Latium's political scene: a successful equestrian family that only reached Curia-eligible standing within living memory, still treated with a real, felt condescension by houses like Gens Fabricia. A useful rival specifically for a player household pursuing its own *novus homo* cursus honorum arc (Politics & Patronage §6) — Considia is the closest thing Latium has to a peer competitor in that exact story, rather than an entrenched obstacle to it.

### 3.10 Home Anchor

**Tusculum.** The player's own estate sits in the same real, historically-attested band of countryside villa land that Rome's own senatorial class favored — close enough for Latium's own political proximity to feel real and immediate, but the estate itself is the player's, not a literal placement inside Tusculum proper.

### 3.11 Templated Background Flavor *(new)*

Resolving part of the first draft's own open question: a Latium start is the natural home for Templated Backgrounds' **"impoverished patrician clawing back status"** archetype (Core §5.2) — Gens Sergiana (§3.9) exists partly to make that archetype's own social world feel populated rather than abstract, since a genuinely declining-but-proud old house is exactly the kind of neighbor an impoverished-patrician player household should have. Gens Considia's own recent arrival (§3.9) shows that a *novus homo* equestrian-climber story can happen in Latium too, but §4.11 below gives that archetype a stronger, more natural home elsewhere — Latium's own version of it plays out in the old-guard's shadow specifically (Fabricia's real condescension toward Considia), which is a different flavor from the same archetype's Campania expression.

---

## 4. Campania

### 4.1 Terrain & Feature Profile

Coastal Bay-of-Naples terrain over genuinely exceptional volcanic soil — the real ancient *Campania Felix* — plus a fixed **Dormant Volcano** terrain feature (Natural Disasters & Environment §2.2) on specific Campania plots, directly inheriting that document's own Vesuvius mechanic rather than duplicating it. Excellent conditions for viticulture and olive cultivation on the volcanic slopes themselves, alongside real coastal access supporting both fishing and major trade shipping.

**New this pass — the Lucrine Lake.** A real, specific, and genuinely famous body of water on the Bay of Naples, historically renowned across the ancient Mediterranean for producing the finest cultivated oysters in the Roman world. This gives Campania a concrete, high-value luxury-goods hook distinct from its wine, cement, and garum identity (§4.6), and a natural terrain-level tie to Resources & Goods' existing Oyster Beds building (Coast-gated, per Buildings §3) at a genuinely prestige tier rather than an ordinary coastal-subsistence one.

### 4.2 Economic Package

*(Qualitative.)* Land here costs less than Latium's but is still genuinely desirable — the combination of real agricultural fertility, established trade infrastructure, and elite leisure-villa demand keeps Campania firmly upper-tier rather than a bargain region. Unlike Latium, Campania is a real historical net agricultural *exporter*, not an importer — the direct economic mirror of Latium's own grain dependency, and the clearest single illustration of why splitting these two regions was worth doing.

### 4.3 Political & Legal Texture

Less institutionally central than Latium — no direct Senate proximity pressure, and Curia contests here, while real, don't carry Latium's same maximum-competition intensity. What Campania has instead is a real, distinct **social** political layer: enough of Rome's own elite kept leisure villas at Baiae and along the Bay that a Campania household's political life plays out as much through informal social contact with visiting Roman elites as through the region's own formal offices. Legal Status here is more genuinely layered than Latium's — real Greek civic identity persisted at Naples specifically (see §4.5), meaning a meaningful Peregrine or culturally-Greek citizen population is a plausible, historically honest part of this region's texture in a way it simply isn't in Latium.

**New this pass — Naples' own distinct civic-legal status.** Worth naming directly: Naples' real, historically documented retention of Greek civic institutions (its own magistrates, its own games, its own language in public life) alongside Roman rule is a genuinely unusual legal arrangement for an Italian-Heartland town — closer in spirit to how a Greek East city might be governed than to how an ordinary Italian municipium worked. A future Legal & Court pass could reasonably treat Naples as a small, contained exception worth its own brief case-study note, rather than folding it silently into Campania's general Legal Status mix.

### 4.4 Diplomatic & Military Exposure

No Frontier neighbor, per §2. Campania's military flavor instead runs through the sea: **Misenum**, a real Gazetteer entry (§4.8), was the actual headquarters of the western imperial fleet (the *Classis Misenensis*) — giving this region a genuine naval-service recruitment identity distinct from ordinary legionary service, and a real, direct, concrete hook into Piracy & Banditry's own anti-piracy patrol flavor. Naval service was also, historically, a more attainable path to citizenship and social mobility for freedmen and provincials than legionary service — a real, honest social-mobility texture worth letting Familia's own Legal Status and social-class mechanics reflect here.

**New this pass — the mechanical shape of that mobility path.** Concretely: a freedman or Peregrine-status individual with strong Martial and a Campania-region household connection should read as a genuinely plausible Misenum naval recruit in a way a comparable individual in Latium simply wouldn't have an equivalent path for — not a new tracked mechanic, but a flavor-and-eligibility note for whichever future pass (Familia's own status-progression content, most likely) builds out concrete social-mobility career paths.

### 4.5 Religious & Cultural Defaults

Campania is this region pair's real cosmopolitan counterweight to Latium's traditionalism, and its own religious/cultural default should read that way directly:

- **Greek religious and civic identity at Naples and Cumae** — Naples (ancient Neapolis) was founded as a genuine Greek colony and kept real, distinct Greek civic customs and games even under Roman rule; Cumae, the oldest Greek colony on the Italian mainland, is home to the real, famous Cumaean Sibyl and a genuine Temple of Apollo — an unmatched, ready-made Religion-system pilgrimage/prophecy destination (Omens & Auspices, Religion §system).
- **Oscan/Samnite substrate culture**, particularly around Pompeii and, new this pass, Nola and Nuceria (§4.8), a real pre-Roman local identity fully folded into Roman civic life by this game's era but still a legitimate, distinct cultural thread rather than an invented one.
- **Real foreign-cult presence** — a genuine, historically attested Temple of Isis stood at Pompeii, and Puteoli's own status as a major international port makes it entirely plausible for further real foreign cult worship (Religion's own foreign-cult/syncretism mechanics) to take root here well before it would anywhere in Latium.

Campania's starting Cultural Drift lean (Education & Culture) should accordingly sit meaningfully more open to Hellenic influence than Latium's own default — the direct mechanical payoff of a region whose own real elite culture (Baiae's leisure-villa scene, Naples' Greek games) was already steeped in Greek taste centuries before this game's range even opens.

### 4.6 Regional Goods & Trade

Campania's production identity is built on four real, famous exports: **Falernian wine** (the single most celebrated wine of the ancient Roman world, genuinely from this region), **Pozzolana** (the volcanic-ash ingredient behind Rome's own signature hydraulic concrete, already tagged to the Italian heartland in Resources & Goods and now specifically homed here), **garum** — Pompeii was a real, major center of garum production, distinctive and valuable enough that surviving amphorae are actually inscribed with the maker's name, a real ancient equivalent of a branded product — and, new this pass, the **Lucrine oyster**, a genuine luxury delicacy good in its own right rather than an ordinary coastal foodstuff. Between the volcanic soil's raw agricultural output and Puteoli's trade volume, Campania reads as the wealthier and more diversified economy of the two Italian regions, offset entirely by §4.1's own standing catastrophic risk.

### 4.7 Population & Culture Distribution

Roman-majority but genuinely, meaningfully mixed — a relative-weight read alongside Latium's own table in §3.7, for direct comparison:

| Culture | Presence |
|---|---|
| Roman/Latin | Dominant |
| Hellenic (Naples, Cumae) | Common — a real, settled, multi-generational community, not an outlier |
| Oscan/Samnite (interior Campania, Pompeii, Nola, Nuceria) | Common |
| Egyptian (foreign-cult-following traders, Puteoli specifically) | Rare, but a real, plausible small settled community rather than a pure individual outlier |
| Any other culture (further Mediterranean traders passing through Puteoli) | Rare, individual-level |

This is Campania's own clearest point of contrast with Latium's population profile (§3.7): where Latium is what unmixed Roman mainstream looks like, Campania is what genuine, long-settled Mediterranean cosmopolitanism looks like within the Italian Heartland itself.

### 4.8 Gazetteer

Expanded substantially this pass — the real historical density of the Bay of Naples genuinely supports the largest Gazetteer of any region on the launch roster, and this document leans into that rather than trimming it down to match Latium's own smaller list.

| Location | Role(s) | Tier | Grounding |
|---|---|---|---|
| **Puteoli** | Major Port, Market Hub | Regional Center | Italy's real busiest port for most of the Republic and early Principate, well before Ostia's own later expansion — the actual commercial heart of the region, even though Campania itself never held separate provincial-capital status the way a true frontier province would (Italy as a whole sat outside the ordinary provincial system, per §5's own Capital note). |
| **Neapolis (Naples)** | Market Hub | Regional Center | The real Greek-founded city that kept genuine Hellenic civic identity and games under Roman rule — a natural home for a distinct Games & Spectacle flavor (the real Sebasta games) beyond the gladiatorial norm, and per §4.3, a genuine legal/civic outlier worth its own note. |
| **Misenum** | Naval Base | Regional Center | The real headquarters of the *Classis Misenensis*, the western imperial fleet — this document's own naval-service answer to a legionary base, per §4.4. |
| **Pompeii** | Market Hub | Regional Center | A real center of garum production and, per Natural Disasters & Environment §2.2, a settlement sitting directly in the Dormant Volcano's shadow — the single most historically loaded location on this entire regional roster. |
| **Herculaneum** | Market Hub | Outpost | A smaller, real elite-villa town alongside Pompeii, equally within Vesuvius's real historical blast radius; home to the real Villa of the Papyri, a natural Education & Culture library/scholarship flavor hook. |
| **Baiae** | Market Hub | Regional Center | The real, famously hedonistic leisure-villa resort of the Roman elite — hot springs, real historical scandalous reputation, and the natural venue for Campania's own social-not-institutional political texture (§4.3). |
| **Cumae** | Sanctuary | Regional Center | The oldest real Greek colony on the Italian mainland, home to the real Cumaean Sibyl and a genuine Temple of Apollo — this region's own premier Religion-system destination. |
| **Capua** | Market Hub | Regional Center | A real, historically famous center of gladiatorial training — the actual *ludus* tradition Games & Spectacle already gestures at, and, per Cultures of the Known World's own Thracian entry, the real starting point of Spartacus's own revolt. |
| **Stabiae** *(new)* | Market Hub | Outpost | A real, genuine third Bay-of-Naples town lost in the same Vesuvius eruption that struck Pompeii and Herculaneum — included specifically to make §4.10's own catastrophic risk feel like it touches a real, plural community rather than two towns treated as a single unit. |
| **Nola** *(new)* | Market Hub | Outpost | A real inland Oscan-heritage town, giving §4.5's own Oscan/Samnite cultural thread a second concrete Gazetteer anchor beyond Pompeii alone. |
| **Nuceria** *(new)* | Market Hub | Outpost | A further real Oscan-heritage town with its own attested rivalry with Pompeii historically — a small, honest, pre-existing local tension a future Rival Houses or Events pass could draw on without inventing one from nothing. |

### 4.9 Rival Seeding

Expanded this pass with a fourth house, giving Campania's own field a naval/military-adjacent rival to sit alongside the trade, heritage, and leisure archetypes already present.

- **Gens Vibiana** *(seated at Puteoli)* — a wealthy shipping-and-garum trade family with a real Domus Mercatoria lean (Policies & Edicts §3); aggressive, commercially-minded rivals for trade routes and market position rather than for Senate seats.
- **Gens Alfidia** *(seated near Pompeii)* — an old Oscan-descended local family, thoroughly Romanized in law and custom but genuinely proud of its pre-Roman roots; a natural rival for the loyalty and Contentment of Campania's own Oscan-heritage population (Settlement Demographics), with a real, attested historical rivalry (§4.8's Nuceria note) available to color that further.
- **Gens Herennia** *(seated at Baiae)* — a real, attested Campanian family name repurposed here as a leisure-class house more invested in spectacle, hospitality, and Dignitas-through-luxury than in any formal office; a natural rival in Games & Spectacle and Villa grandeur rather than in the Curia.
- **Gens Naevolia** *(seated at Misenum, new)* — a family built on naval command rather than trade or land, with several generations of real, genuine Classis Misenensis service; a natural rival specifically for Military & Combat's own naval-flavored recruitment and for Piracy & Banditry's anti-piracy patrol standing, distinct from every other Campania or Latium house's own civilian-flavored rivalry.

### 4.10 Home Anchor

**Between Pompeii and Herculaneum**, on the fertile Vesuvian slopes both towns share — and now, per §4.8's expanded Gazetteer, within the same general reach of Stabiae as well, reinforcing that this is a real, plural community living under a shared risk rather than an isolated household's own private danger. This remains a deliberate choice rather than a safer, vaguer alternative: it puts Natural Disasters & Environment's own Vesuvius mechanic in direct, felt proximity to the player's own household from turn one, consistent with this project's design pillar of historical frankness without gratuitousness — the risk is real, present, and mechanically live, not a background flavor note the player never actually has to reckon with.

### 4.11 Templated Background Flavor *(new)*

A Campania start is the **primary** natural home for Templated Backgrounds' **"jumped-up equestrian merchant"** archetype (Core §5.2) — Gens Vibiana's own trade-wealth identity (§4.9) gives that archetype a populated social world to enter, and Puteoli's own status as the region's commercial heart makes a merchant-background player household's presence there feel earned rather than arbitrary; this is the stronger, more natural expression of the archetype than Latium's own version of it (§3.11). The **"veteran given a land grant"** archetype also fits naturally here, reading against Gens Naevolia's own naval-service identity (§4.9) if the veteran's own background is naval rather than legionary — a small, genuine variant on that archetype this region is uniquely positioned to offer.

---

## 5. Rome — The Capital

Rome itself belongs to neither Latium nor Campania exclusively — it sits, per the framework's own **Capital** Role (Starting Regions §8.3), outside the ordinary Provincial Seat category altogether, reflecting the real historical fact that Italy was never organized as a province under the Principate the way Gaul or Hispania were. Rome is instead the single shared Gazetteer entry both Italian regions' households can reach with unmatched ease relative to every other region on the roster:

- **The full cursus honorum** (Politics & Patronage §6) is anchored here and nowhere else.
- **The Senate itself**, and by extension the highest tier of Legal & Court proceedings.
- **The Vestal institution** (§3.5), administratively centered here regardless of which Italian region a candidate's own household calls home.
- **The greatest concentration of Rival Houses' own activity** — many old patrician gentes, including Latium's own Gens Fabricia (§3.9), plausibly maintain a Rome townhouse in addition to their countryside seat, making Rome the single most politically crowded Gazetteer entry in the game.
- **The largest venues for Games & Spectacle**, described here deliberately in general terms (great fora, temples, and amphitheaters) rather than pinned to any one specific, date-sensitive monument, consistent with §2's era-flexibility note.

A Latium household reaches Rome fastest and most cheaply of anyone in the game; a Campania household reaches it a real, meaningfully greater but still short distance by comparison — both dramatically closer than any other region's own relationship to the capital, which is the entire mechanical point of naming this pair "the Italian Heartland" in the first place.

---

## 6. Distance & Travel — The Italian Heartland in Context

New this pass: a first concrete pass at the Starting Regions framework's own Distance Tier lookup table (§7.1 of that document), scoped to the Italian Heartland's own relationships — resolving a small piece of that document's own open question rather than leaving every pairing abstract indefinitely.

| From | To | Distance Tier | Note |
|---|---|---|---|
| Latium | Rome | Near (minimal) | The defining trait of the region — see §5. |
| Campania | Rome | Near–Moderate | Genuinely short by the standards of the wider roster, but a real, felt step further than Latium's own relationship to the capital. |
| Latium | Campania | Near | Both Italian regions sit close enough to each other that inter-regional Travel between them should read as comfortably short — closer to a routine trip than a real journey. |
| Latium/Campania | Gallic Frontier | Moderate | A genuine sea-or-overland journey, but along well-established Roman roads and shipping lanes rather than into the unknown. |
| Latium/Campania | Iberian Colony | Moderate | Similar logic — a real trip, well within the bounds of ordinary Roman trade and travel infrastructure. |
| Latium/Campania | North African Colony | Moderate | A short sea crossing by ancient Mediterranean standards; Puteoli specifically (§4.8) is a plausible, well-trafficked embarkation point. |
| Latium/Campania | Greek East | Moderate | A real, established sea route; nothing about it should read as more remote than the other Moderate-tier pairings above. |
| Latium/Campania | Egypt *(extensible slate)* | Far | The full length of the Mediterranean — a real, substantial journey even by the standards of a well-traveled Roman. |
| Latium/Campania | Britannia *(extensible slate)* | Far | The edge of the known-and-governed world, per that region's own extensible-slate note in the framework document. |

This table exists to give a future Distant Holding (Starting Regions §7) decision involving an Italian Heartland household something concrete to read from immediately, without waiting for every other region's own document to be written first.

---

## 7. Historical Timeline Hooks — Divergence-Eligible Moments

New this pass, paralleling the framework's own note that Britannia's Boudicca revolt and Iberia's Cantabrian Wars are natural Events-system Divergence sources (Starting Regions §11's cross-reference to Events). The Italian Heartland has two of its own, of very different character:

### 7.1 The Vesuvius Eruption (Campania)

Already mechanically real via Natural Disasters & Environment §2.2 — this document's own contribution is simply naming it here as the single highest-weight Divergence-eligible moment available to any Campania household, Events-system-side, precisely because it's tied to named, real Gazetteer locations (Pompeii, Herculaneum, Stabiae) and the player's own Home Anchor (§4.10) rather than an abstract regional roll. A Dynasty Chronicle entry generated by surviving — or not surviving — this event should read at the highest tier that document's own significance scale supports.

### 7.2 The Social War (Latium, era-conditional)

A real, historically significant conflict (91–87 BC) fought specifically over whether Rome's Italian allies — including several of Latium's own Gazetteer towns' real historical neighbors — would receive full citizenship rather than the lesser Latin Rights status many held at the time (§3.3). Unlike Vesuvius, this isn't a standing mechanic available in every playthrough — it's era-conditional, only live for a playthrough whose start date genuinely predates the war's real historical resolution, and effectively closed history (already resolved in Rome's favor) for the default Pax Romana setting. Flagged here so a future Events or Scenario Starts pass has a real, historically honest option for an earlier-era Latium playthrough, rather than Latium reading as having no timeline texture at all outside of Vesuvius's own Campania-side drama.

---

## 8. Cross-System Integration

- **Starting Regions (framework):** fulfills the Region Profile schema (§4) for both Latium and Campania, updates the launch roster to six regions (§5.1), and introduces the Capital Role now folded back into that document's own §8.3 Role table; this pass further resolves part of that document's own Distance Tier open question (§6 above).
- **Natural Disasters & Environment:** Campania's Dormant Volcano terrain feature and Vesuvius Event are inherited wholesale from §2.2 of that document, not redefined here; this document's own contribution is placing specific Gazetteer entries (Pompeii, Herculaneum, and now Stabiae) and the Home Anchor itself inside that risk, and naming it directly as a Divergence source (§7.1).
- **Politics & Patronage:** Latium's own accelerated cursus honorum access and maximum-contest Curia elections read directly against §5–6 of that document; Gens Sergiana and Gens Considia (§3.9) are direct, intentional illustrations of that document's own Dignitas-vs-Net-Worth cursus honorum gate and *novus homo* storyline respectively.
- **Villa:** the Four Pompeian Styles (Villa §7.1) are this region pair's own signature decoration progression, and now have a literal namesake Gazetteer entry (Pompeii) to anchor them to.
- **Resources & Goods:** Latium's wine/olive/salt/peperino identity and Campania's Falernian/Pozzolana/garum/Lucrine-oyster identity both read from and specify that document's existing region tags (§5/§7), with the Lucrine oyster and Via Salaria salt-pan tradition new, concrete additions this pass.
- **Religion:** Campania's Cumae entry and its foreign-cult texture (Temple of Isis at Pompeii) are this document's own concrete hooks into that system's syncretism mechanics; Latium's Praeneste entry and its own Vestal-candidacy flavor (§3.5) are its parallel hooks into that system's mainstream Roman worship and state-cult institutions respectively.
- **Games & Spectacle:** Capua's gladiatorial-training history and Naples' real Greek-style games give that system two genuinely distinct flavor sources within the same region pair.
- **Education & Culture:** Campania's more Hellenic-leaning starting Cultural Drift and Herculaneum's Villa of the Papyri hook both feed that system directly.
- **Legal & Court:** Naples' own distinct Greek civic-legal arrangement (§4.3) and Latium's pre-Social-War Latin Rights texture (§7.2) are both flagged as concrete case-study material for that system's own future depth pass.
- **Military & Combat, Piracy & Banditry:** Gens Naevolia (§4.9) and Misenum's own Naval Base role (§4.4, §4.8) give both systems a genuine naval-flavored recruitment and patrol identity distinct from every land-based region's own military texture.
- **Rival Houses:** eight named, region-seated houses across the two regions (§3.9, §4.9) — the framework's own §9 mechanism, now with a fourth house per region and a real historical rivalry (Nuceria/Pompeii, §4.8) available to color one of them.
- **Dynasty Chronicle:** Alba Longa and Lavinium's own mythic weight (§3.8) make them natural high-tier Chronicle flavor references independent of anything the player's own household does there; the Vesuvius eruption (§7.1) is this region pair's own single highest-weight Chronicle event source.
- **Events:** §7 as a whole is this document's own direct contribution to that system's Divergence-eligible moment roster.

---

## 9. Data Model

```
Region {
  regionId: "latium" | "campania",
  ...                                        // inherits full Region schema from Starting Regions §12
  reputationDualityMode: "none",
  hasStandingFrontierNeighbor: false,
  hasStandingLegionaryGarrison: false,
}

LatiumEconomicProfile {
  grainImportDependency: true,               // §3.2 — Latium's own standing vulnerability
  landCostTier: "highest",
  saltPanAccess: true,                        // new — §3.1
  urbanRentalIncomeEmphasis: true,            // new — §3.2
}

CampaniaEconomicProfile {
  netAgriculturalExporter: true,             // §4.2 — the direct mirror of Latium's own dependency
  landCostTier: "high",
  dormantVolcanoRef,                         // points to Natural Disasters & Environment's existing terrain feature
  lucrineOysterAccess: true,                  // new — §4.1, §4.6
}

CapitalLocation {                            // Rome specifically — §5, extends GazetteerLocation
  locationId: "rome",
  role: "capital",                           // the only Gazetteer entry ever carrying this Role
  reachableFromRegionIds: ["latium", "campania"],
  travelCostTierFromLatium: "minimal",
  travelCostTierFromCampania: "shortModerate",
}

DistanceTierEntry {                          // new — §6, populates Starting Regions' own DistanceTier lookup
  fromRegionId, toRegionId, tier,             // "near" | "moderate" | "far"
}

TimelineHook {                                // new — §7, feeds Events' own Divergence system
  hookId: "vesuviusEruption" | "socialWar",
  regionId,
  eraConditional: bool,                       // true for socialWar — closed history under default Pax Romana start
  divergenceWeight,                           // qualitative only, per this project's no-numeric-sizing convention
}
```

---

## 10. Open Questions

- **All numeric sizing**, per this project's standing convention — Curia contest intensity, grain-dependency severity, Vesuvius eruption frequency (already flagged as an open question in Natural Disasters & Environment itself), and the exact strength of every Distance Tier in §6 are left to a future balancing pass.
- **Whether Campania's own internal Bay-of-Naples geography ever needs finer division.** This document now treats eleven named locations as one region's Gazetteer (Puteoli, Naples, Misenum, Pompeii, Herculaneum, Baiae, Cumae, Capua, Stabiae, Nola, Nuceria); a future pass could ask whether that's too dense for one region and would read better split further, though this document's own judgment remains that the real historical density is the point and shouldn't be diluted.
- **How a Latium-vs-Campania choice interacts with Randomized and Scenario Starts specifically.** §3.11 and §4.11 now give each region a clear primary Templated Background archetype (impoverished patrician / declining-old-guard for Latium, jumped-up equestrian merchant / naval veteran for Campania), but Randomized and Scenario Starts' own exact default-weighting logic between the two regions still isn't specified.
- **Naval service's exact social-mobility mechanics.** §4.4 names the real historical pattern and a concrete flavor note in §4.4's own expansion, but doesn't specify how Familia's own Legal Status progression should model it mechanically beyond noting the hook exists.
- **Gens Sergiana as an alliance target vs. a rival.** §3.9 deliberately leaves this house's eventual relationship to the player ambiguous — a real, live design choice for whichever system (Politics & Patronage's own marriage market, most likely) ends up resolving it in practice.
- **The Social War's actual playability.** §7.2 names it as an era-conditional hook but doesn't resolve whether any current or planned Start Mode actually opens a playthrough early enough for it to matter, or whether it remains a purely aspirational flag until a future era-range decision addresses it directly.
- **Naples' Greek civic-legal exception's mechanical depth.** §4.3 flags it as worth a future Legal & Court case study but doesn't attempt that system's own depth here, correctly deferring to that document's own ownership of Legal Status mechanics.
