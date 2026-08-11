# GENS — Design Authority Registry

*This document exists to close Phase 1, Item 3 of the Comprehensive Build Roadmap: "for every shared concept, name the authoritative document and mark older descriptions as summaries, extensions, or superseded text." It is a different artifact from the Canonical Object & Data Registry — that one enumerates named entities (every Culture, every Good, every Trait); this one resolves **ownership conflicts** between documents that describe the same mechanic, record, or concept. The corpus turns out to be unusually self-documenting about this already: 19 documents use the word "recap," 9 use "supersede," 17 use "unchanged from," and 18 make an explicit "authoritative"/"owns" claim. This registry systematizes those scattered self-declarations into one lookup table, quoting or citing the actual textual evidence rather than asserting authority from outside the corpus. 31 clusters are mapped below. Where no explicit statement exists but a clear pattern does (e.g., the four sampling-and-promotion siblings), that's noted as inferred rather than declared.*

---

## 0. How to Read This Registry

Each cluster below names:
- **The shared concept** — the thing multiple documents touch.
- **Authoritative document** — the one document that owns the actual rule/field/mechanic. This is what implementation should read.
- **Extensions** — documents that build *on top of* the authoritative source without competing (an instance, a widened source list, a domain-specific application).
- **Superseded/Deprecated** — older text that should be treated as historical only; implementation should not read it.
- **Evidence** — the corpus's own words establishing the boundary, so this registry stays checkable against source rather than asserted.

---

## 1. The Universal Character Record

**Authoritative:** `gens-characters-design.md` — owns Character identity, lifecycle, Personality Axes, Interaction Catalog, and the Scheme engine.

**Superseded:** Familia §8's original sketch and Politics & Patronage's lighter `Notable{}` generation/scheme resolution.

**Evidence:** Characters' own schema comment — `Character { ... } // supersedes Familia §8's sketch and Politics & Patronage's Notable{}` and `Scheme { ... } // §10 — supersedes Politics & Patronage's lighter Scheme{}`. Politics & Patronage confirms this directly: *"this document's own lighter treatment of 'Notable' generation and ad-hoc scheme resolution should be read as superseded by that pass rather than as a competing design."*

**Extensions:** Companions & Court Positions (position roster only — explicitly does not create political office, per its own §5.5 line honored by Politics & Patronage); Epithets, Nicknames & Titles (the naming/agnomina layer); Traits (see §2 below).

---

## 2. The Trait Catalog

**Authoritative:** `gens-traits-design.md` ("Full Catalog") — owns every trait pair, the three Tiered Spectrums, and Combo Titles.

**Superseded:** Characters' original inline trait pairs and simple two-value spectrums.

**Evidence:** Traits §3.1–3.3 headers read *"supersedes Quick/Slow,"* *"supersedes Beautiful/Plain,"* *"supersedes Strong/Weak"* directly. Traits §8 is an explicit "Migration Note — What Changes From the Characters Document."

---

## 3. Household & Lifecycle

**Authoritative:** `gens-familia-design.md` — owns lifecycle stages, household role, birth, marriage mechanics (`affectio maritalis`), and legitimacy.

**Extensions:** Coming-of-Age & Education (the adolescent lifecycle window specifically); Retirement & Old Age (the Elder window specifically); Succession & Dynasty (headship transition); Dynasty Chronicle / Dynasty & Legacy Tree (the historical record of the above, not new lifecycle rules).

**Evidence:** Weddings' own framing: *"this document isn't building the mechanism that makes two people married; Familia's own marriage math already does that. This document is building the celebration."*

---

## 4. Romance & Sexuality

**Authoritative:** `gens-romance-sexuality-lineage-design.md` — owns implemented romance/pregnancy/lineage rules, including the Concubine bond tier.

**Superseded:** `gens-romance-seduction-design.md` for any rule the newer document also covers.

**Evidence:** Roadmap Phase 1 names this explicitly: *"gens-romance-sexuality-lineage-design.md supersedes gens-romance-seduction-design.md for implemented romance rules."* Romance, Sexuality & Lineage's own text treats Familia's Divorce and Legal & Court's status law as settled dependencies rather than re-deriving them.

**Open gap:** the two romance documents' actual boundary (which specific mechanics in Romance & Seduction are *not* superseded, if any) isn't spelled out anywhere — flagged in §10 below.

---

## 5. Starting Regions

**Authoritative:** `gens-starting-regions-design.md` — owns the Region Profile Schema (§4), the launch/extensible roster, Reputation Duality's applicability rules, and the Regional Gazetteer format.

**Superseded:** Core Design's original four-region sketch (§4 of that document).

**Extensions (data, not schema):** all 17 individual region documents — each explicitly inherits the schema rather than redefining it (e.g., Mesopotamia's own record comment: `// inherits full Region schema from Starting Regions §12`).

**Evidence:** *"Core Design: supersedes the original four-region sketch in §4 of that document with the five-region launch roster in §5.1 above."* Also: *"Buildings: §3's terrain/feature gate table... is the authoritative source each region document's Terrain & Feature Profile (§4.1) reads from"* — meaning Buildings retains a narrow, specific piece of authority (terrain gating) inside the regions cluster.

---

## 6. The Living-World Actor Abstraction (Sampling-and-Promotion Family)

**Authoritative:** `gens-rival-houses-design.md` — owns the shared Living World Actor model (a tribal leader or petty king "is simply a Character... functioning as a Living World Actor at whatever tier the player's actual contact warrants").

**Sibling instances (not competing designs, per the "sampling-and-promotion pattern" the corpus names explicitly):**
- Notable Households — ordinary families
- Wandering Populations — itinerant specialists
- Notable Businesses — named commercial enterprises

**Evidence:** Notable Businesses' own framing: *"A fourth and final application of the sampling-and-promotion pattern this project has now used at every population tier — Rival Houses for gentes, Notable Households for ordinary families, Wandering Populations for itinerant specialists, and now this document for named commercial enterprises."*

**Adjacent, narrower ownership:** Diplomacy with Non-Roman Peoples "owns the actual depth" (treaties, tribute, alliance) for foreign Actors specifically; Rival Houses supplies only the shared model it builds on. (*"Diplomacy... owns the actual depth here... this document supplies the shared model that system should build on rather than invent separately."*)

---

## 7. Fame

**Authoritative:** `gens-games-spectacle-design.md` §2 — owns the core 0–100 Fame field, decay shape, and arena/circus/theatrical generation.

**Extensions:** Celebrities & Influential Figures — widens the *source list* for Fame without altering the mechanic.

**Corrected (not superseded, just fixed):** Wandering Populations previously implied a parallel Fame field; Celebrities' own cross-system note explicitly corrects this to point at the shared field: *"corrects that document's own Fame score to explicitly use this shared universal field rather than a parallel one — no mechanical change... only a corrected cross-reference."*

---

## 8. The Activity Engine

**Authoritative:** `gens-activities-activity-engine-design.md` — owns the six-slot multi-phase hosted-activity anatomy (Host, Type, Phase structure, etc.).

**Extensions (Activity Types, each an instance, each also retaining its own domain-specific ownership):**
- Feasts (§6.48) — plugs Food Culture's existing Banquet Quality/Cuisine Match machinery into the Engine; Food Culture retains ownership of that formula.
- Weddings (§6.49) — the celebration layer only; Familia retains ownership of the actual marriage mechanic.
- Pilgrimages (§6.50) — reuses Travel's journey machinery wholesale; Travel retains ownership of journey resolution.
- Hunts, Beast Taming & Menageries (§6.51) — owns hunt/capture/Notable Specimen mechanics; the Bestiary (§9 below) retains descriptive ownership of what each creature *is*.

**Evidence:** Each document's own subtitle names itself as "(the Activity Engine's Nth Activity Type)."

---

## 9. Peoples, Faiths, Fauna & Flora — The Four Sibling Registries

**Authoritative documents, each explicitly modeled on the others:**
- `gens-cultures-of-the-known-world-design.md` — peoples
- `gens-religions-of-the-known-world-design.md` — faiths
- `gens-bestiary-fauna-registry-design.md` — animals & creatures
- `gens-flora-herbal-registry-design.md` — plants

**Evidence:** Bestiary's own framing names the pattern directly: *"A standalone, authoritative registry, built in the same spirit as Resources & Goods (the authoritative registry for trade goods) and Cultures/Religions of the Known World (the authoritative registry for peoples and faiths)... but for animals and creatures."* Flora confirms itself as Bestiary's *"direct, deliberate sibling... built the same way the Bestiary was built for fauna."*

**Boundary within the Fauna/Hunts split:** *"this document [Bestiary] is the canonical descriptive source for every Legendary entry — Hunts itself owns the combat mechanics, capture rules, and Notable Specimen treatment; this document owns what each one is."*

**Boundary within the Flora/Resources split:** Flora re-lists every crop Resources & Goods already named (Grain, Grapes, Olives, Flax, etc.) for species identity and flavor, but *"that document keeps sole ownership of storage, perishability, and pricing."*

---

## 10. Goods & the Buildings Taxonomy

**Authoritative:** `gens-resources-goods-design.md` — self-declared *"the complete, authoritative registry"* for every good, including the Unified Goods Registry (§7).

**Superseded, but NOT yet cleaned up (flagged by the source doc itself as unfinished work):**
- `gens-buildings-design.md` §2's original goods taxonomy
- `gens-estate-settlement-design.md` §8's simplified storage list

**Evidence — and this is a real, named, still-open task, not a resolved one:** *"Supersedes the Buildings doc's §2 goods taxonomy and Estate & Settlement's §8 simplified storage list in full."* And in the same document's Open Questions: *"Back-porting into the Buildings and Estate & Settlement docs. Both still contain their own now-superseded partial lists; this pass makes that gap larger, not smaller, and it's the most concrete remaining task before implementation."*

**This is the single most concrete, self-identified action item this registry surfaces: Buildings §2 and Estate & Settlement §8 should be edited (or annotated as deprecated) before Phase 3 content-schema work begins, or the content compiler will have two contradictory goods lists to draw from.**

---

## 11. Market & Treasury

**Authoritative:** `gens-resources-goods-design.md` §12 — owns the full dynamic market simulation (supply/demand, seasonality, Disaster/Piracy/War disruption, the shared regional market with Rival Houses).

**Extension:** `gens-economy-finance-design.md` — adds only the treasury-facing layer (household-level ledger, taxation, debt) on top; does not re-simulate the market.

**Evidence:** *"Resources & Goods §12's full dynamic simulation... remains the authoritative baseline and is unchanged here. This document adds only the treasury-facing layer on top."*

---

## 12. The Named-Business Cluster

**Authoritative:** `gens-notable-businesses-design.md` — owns the business-level record itself (Reputation distinct from owner's personal standing, named competition, named suppliers).

**Extensions, each with a distinct, non-overlapping slice:**
- Societates & Business Partnerships — owns partnership *structure and liability* specifically; explicitly does **not** rebuild Sale/Acquisition/Merger (owned by Land Ownership & Real Estate §5 and Notable Businesses §8).
- Business Competition — owns the *full escalation ladder* beyond Notable Businesses' own opening sketch (§5 of that doc).
- Merchant Families & the Equestrian Order — owns the equestrian-identity/social-mobility character study layered on top; not a mechanics document, a social-context one.

**Evidence:** Societates: *"Sale, Acquisition, and Merger already have real homes... this document doesn't rebuild either."* Business Competition: *"The full escalation ladder Notable Businesses' own Named Competition section... only sketched the opening rungs of."*

---

## 13. Secrets, Scandal, Espionage, Crime & Legal Process

**Authoritative, each owning a distinct stage of the same pipeline:**
- Espionage — names secret *content* only, descriptively (does not own the record or mechanic).
- `gens-secrets-hooks-design.md` — owns the actual Secret record and the Hook (leverage) mechanic.
- `gens-scandal-design.md` — owns the public-exposure aftermath once something goes public (severity, scope, spread, consequence).
- `gens-crime-punishment-imprisonment-design.md` — owns the punishment catalog (honestiores/humiliores) and the new Detention status.
- `gens-legal-court-design.md` — retains sole ownership of trial, evidence, and verdict process, explicitly untouched by Crime & Punishment.

**Evidence:** Secrets & Hooks' own §10 lays out the whole pipeline explicitly: *"Espionage already named the content... purely descriptively... Scandal already owns the entire public-exposure aftermath... Crime, Punishment & Imprisonment already built the exact shape a fabricated record needs to take."* Crime & Punishment confirms the Legal & Court boundary directly: *"leaving Legal & Court's own trial, evidence, and verdict process untouched."*

---

## 14. Buildings, Monuments & Infrastructure

**Authoritative:** `gens-buildings-design.md` — owns building instances and production chains (the Full Building Index).

**Extensions, each explicitly bounded to stay out of the other three:**
- Monuments & Legacy Building — owns the monument-specific roster and Legacy/Damnatio Memoriae mechanics, building on Buildings §4.12.
- Private Infrastructure — owns the connective/improvement layer *between* plots (roads, water, terrain improvement, boundaries) — explicitly not Villa's interior space, not Public Works' civic infrastructure, not Buildings' production chains.
- Public Works & Euergetism — owns civic infrastructure funded for a settlement's whole population.

**Evidence:** Private Infrastructure's own §1: *"Three existing documents already own the neighboring territory, and this one deliberately stays out of all three: Villa owns the household's own interior living space; Public Works & Euergetism owns civic infrastructure funded for the whole settlement's population... Estate & Settlement's Buildings taxonomy owns the production chains themselves."*

---

## 15. Calendar, Feast Days & Seasons

**Authoritative, each owning a distinct, non-overlapping layer of "time":**
- `gens-events-design.md` §6.2 — owns the `GameCalendar` (starting year, current year/month, era) and the bounded historical range.
- `gens-religion-design.md` §5 — owns the sacred calendar's actual feast days, sitting on the calendar year.
- `gens-roman-calendar-design.md` — owns real month names, day-counting, the Julian reform, and the market week — the structural layer neither of the above ever specified.
- `gens-seasons-design.md` — owns the four-season *mechanical effect* layer (Agriculture, Military & Combat, Travel, Disease, Natural Disasters, Economy) — the "what does time of year actually do" layer neither above specified.

**Evidence:** Roman Calendar's own framing: *"The structural layer sitting underneath Events' own GameCalendar... and underneath Religion's own sacred calendar... this document owns real month names, real day-counting, the real Julian calendar reform, the market cycle, and year-reckoning, none of which the two documents above ever specified."* Seasons: *"Neither document specified what the calendar itself actually looks like... Neither ever said what time of year actually does to the rest of the game."*

---

## 16. Religion vs. the Dead

**Authoritative:**
- `gens-religion-design.md` §6.6 — owns the living household's relationship with its gods (Lares, Penates, Genius, Favor meter).
- `gens-ancestor-veneration-funerary-customs-design.md` — owns the Roman dead (di Manes, Memoria, funerals, mourning) — adjacent but genuinely distinct.

**Evidence:** *"Religion (§6.6) already owns the household's living relationship with its gods... This document owns something adjacent but genuinely distinct: the Roman dead themselves."*

---

## 17. Technology — Engine vs. Roster

**Authoritative:** `gens-technology-discoveries-design.md` — owns the engine: natural arrival, acceleration, publication/diffusion, racing, theft, and loss of a Discovery.

**Extension:** `gens-discovery-roster-design.md` — owns the actual content roster of named Discoveries the engine operates on; also owns the Prerequisite Chains (the dependency graph) and the Feature Tie-In Index (which system each Discovery ties to).

**Evidence:** Technology's own closing line: *"This document is the engine; a dedicated companion roster of every individual Discovery follows separately."* This is the same "engine vs. content roster" split as Events/Historical-Timeline (§18 below) and Religion/Religions-of-the-Known-World (§9 above).

---

## 18. Events — Delivery Mechanism vs. Historical Content

**Authoritative:** `gens-events-design.md` — owns triggered event delivery, the Weighted Event Pool, chains, and the Monthly Report projection.

**Extensions (content, not mechanism):** the two Historical Timeline content docs — supply the actual dated real-world events read through Events' own delivery mechanism; also each own a Named Historical Figures roster (backdrop-only, never instantiated as Characters).

**Evidence:** inferred from structure rather than an explicit self-declaration — the two timeline docs contain only dated tables and figure rosters, no independent delivery mechanic, and both reference Events' `GameCalendar` and Divergence rules as given.

---

## 19. Appearance — the Aggregator Pattern

**Authoritative for the render itself:** `gens-paperdoll-appearance-design.md` — owns the "Full Description" composition logic, but explicitly does **not** own most of the underlying data it draws from.

**True data owners, per Paperdoll's own field-by-field breakdown:**
- Familia §2.4 / Core §7.11 — fixed appearance basics
- Traits §3.2/§3.3 — Beauty tier, Physique/Build tier
- Cultures of the Known World — cultural origin, accent
- Fashion & Dress / Garment Roster — current Outfit Tier, worn Garments, cultural dress signal
- Hair & Body Marking — hairstyle, eyebrows, facial hair

**Evidence:** Paperdoll's own §-heading: *"The complete field list a Full Description draws from, organized by which document actually owns the underlying data."* This is a genuinely different pattern from every other cluster above — Paperdoll is a pure *consumer/renderer*, not a partial owner.

**Related boundary:** Fashion & Dress's disguise mechanic is explicitly a lever only — discovery/punishment consequences belong to Labor & Slavery, Legal & Court, or Espionage, per that document's own reuse-over-reinvention framing.

---

## 20. Foreign Relations — Espionage, Diplomacy & Client Kingdoms

**Authoritative, each owning a distinct layer:**
- `gens-client-kingdoms-vassal-rulers-design.md` — owns the generalized vassal-relationship engine (Investiture, Tribute & Fealty, hostage-taking/*obsides*, succession crisis, conversion to Province, breaking away).
- `gens-diplomacy-non-roman-peoples-design.md` — owns Frontier treaty-making and the full Parthian state-to-state layer, including Alliance Against Rome's graduated outcomes.
- `gens-espionage-design.md` — owns the spy-network mechanic itself (Persistent Network, double agents, Loyalty-as-defection-risk); targets extend to the full Living World Actor framework (§6) rather than defining a parallel target model.

**Extensions:** the Bosporan Kingdom, Armenia, Nubia, and Arabia Felix region docs each individually built a piece of the vassal/independence pattern before Client Kingdoms & Vassal Rulers existed; that document is explicitly the generalization of what those four were "each already, individually" reaching for, "handed back to every region."

**Evidence:** Client Kingdoms' own framing: *"The generalized, reusable engine sitting underneath four Starting Regions that have each already, individually, built a piece of this without ever naming the shared pattern... built once, properly, and handed back to every region."* Espionage: *"extends targets to the full Living World Actor framework."*

---

## 21. Occupations & Trades — Confirmed Non-Owner

**Authoritative:** none — `gens-occupations-trades-design.md` explicitly disclaims mechanical ownership of anything.

**True owners it defers to:** Companions & Court Positions (every named Position), Settlement Demographics (every pop group), Traits (every specialist Trait-linked profession — Historian, Master Craftsman, Augur, etc.).

**Evidence:** the document's own opening line resolves this cluster completely: *"A pure flavor and naming roster — no new mechanics, no new stats, no new gates... What none of them supply is the ordinary word... This document is that word bank... honest about carrying no numbers at all."* This confirms the candidate cluster flagged in the prior pass was a false alarm — there's no real overlap to resolve, only a clearly-scoped naming layer.

---

## 22. Organizing Structures — Faction, Collegia & Interest Groups

**Authoritative, three deliberately non-overlapping structures:**
- Politics & Patronage §3.1 — owns Faction (broad, permanent, ideological).
- `gens-collegia-guilds-design.md` — owns Collegia (permanent, trade-based), resolving a gap Rival Houses explicitly left open.
- `gens-interest-groups-design.md` — owns the third, temporary, cross-cutting coalition structure (policy-specific, cuts across both of the above).

**Evidence:** Interest Groups' own framing states the boundary directly: *"Faction (Politics & Patronage §3.1) is a broad, permanent ideological label; Collegia (Collegia & Guilds) is a permanent, trade-based organization. Neither describes what actually forms when a specific policy question... creates a real, temporary coalition."* Collegia & Guilds confirms it resolves an open question Rival Houses raised and left for "a dedicated pass."

---

## 23. Threat Actors — Piracy, Bandit Lords, Servile Wars & Kidnap/Ransom

**Authoritative, a clean four-stage escalation:**
- `gens-piracy-banditry-design.md` — owns the base Confederation Living World Actor type (reusing Military & Combat's Combat Resolution Engine and Espionage's Discovery/Traceability model).
- `gens-bandit-lords-outlaw-factions-design.md` — extends Piracy & Banditry's Confederation with a rare, named-individual "famous outlaw" tier; does not redefine the base Confederation.
- `gens-servile-wars-slave-revolts-design.md` — owns the collective-escalation tier sitting *above* Labor & Slavery's individual-scale Unrest/Punishment/Flight & Recapture mechanics (Labor & Slavery retains those).
- `gens-kidnap-ransom-design.md` — owns the actual Kidnap Scheme and Rescue operation, unifying three previously-scattered fragments: Characters' bare Interaction stubs, Crime & Punishment's Detention/Ransom machinery (built for a battlefield prisoner, not a targeted kidnapping), and Piracy & Banditry's two sourcing paths (Targeted Contract, opportunistic capture).

**Evidence:** Bandit Lords: *"Piracy & Banditry already built the Confederation... What it never built is the difference between an ordinary raiding nuisance and... someone who became genuinely famous."* Kidnap & Ransom: *"three separate documents already reference this exact content without any of them owning it in full... This document is where all three finally connect."*

---

## 24. Property — Land Ownership, Estate & Settlement & the Villa

**Authoritative, a clean three-layer stack:**
- `gens-estate-settlement-design.md` — owns the physical growth engine: the land map, terrain-gated plots, and building categories/chains. Its own §1 names itself as the supplier of inputs to Familia, Labor & Slavery, Economy & Finance, Monuments & Legacy Building, Settlement Demographics, and Natural Disasters.
- `gens-villa-design.md` — owns the player's own personal-residence interior specifically (rooms, expansion, customization) — a narrower scope nested inside Estate & Settlement's built structures.
- `gens-land-ownership-real-estate-design.md` — sits *alongside*, not replacing, Estate & Settlement: owns the individually-named property portfolio and its ownership/leasing market layer on top of the plots Estate & Settlement already tracks.

**Evidence:** Land Ownership's own framing: *"A standalone document sitting alongside Estate & Settlement, Economy & Finance, Rival Houses, and Politics & Patronage rather than replacing any of them... this is where a household's wealth stops being one Net Worth number and becomes a real, named portfolio."* Estate & Settlement's own §1: *"the physical growth engine... supplies the labor demand that Familia and Labor & Slavery fill, the income Economy & Finance runs on, the plots Monuments & Legacy Building occupies, the population Settlement Demographics tracks."*

---

## 25. Population — Settlement Demographics vs. Wealth & Purchasing Power

**Authoritative:**
- `gens-settlement-demographics-design.md` — owns the background pop-group model itself (growth, migration, class mobility) — the supply/labor side.
- `gens-population-wealth-purchasing-power-design.md` — extends the *same* pop-group system with the demand side (what the population can actually afford), rather than building a parallel demand curve.

**Evidence:** *"This document builds that pyramid directly into the existing pop-group system rather than assuming a modern demand curve."* Not a competing model — a missing axis added to an existing one.

---

## 26. Civic Funding & Competitive Sale — Public Works, Contracts & Auctions

**Authoritative, three real Roman institutions kept deliberately distinct:**
- `gens-public-works-euergetism-design.md` — owns the actual depth behind Policies & Edicts' one-line "Public Works" Funded Action category; explicitly not the glory-monument content Monuments & Legacy Building already owns — this is the functional civic infrastructure (aqueducts, roads, sewers, marketplaces).
- `gens-public-contracts-competitive-bidding-design.md` — owns *locatio-conductio publica*, the state contracting its own business out to the highest-standing bidder; unifies fragments from Land Ownership & Real Estate's Publicani contract and Economy & Finance's flagged-but-unbuilt supply contracts.
- `gens-public-auctions-design.md` — owns the *auctio*, the separate institution for competitively selling goods/property/people at public sale, run by a *praeco* rather than a Censor — explicitly named as Public Contracts' "real, distinct commercial cousin," not the same mechanic.

**Evidence:** Public Works: *"Policies & Edicts already lists 'Public Works'... in one line... This document is the full depth that line never had room for."* Public Auctions: *"The real, distinct commercial cousin of Public Contracts & Competitive Bidding — that document built locatio-conductio publica... this document builds the auctio."*

---

## 27. Delegation & Remote Communication — Steward/Council vs. Correspondence

**Authoritative:**
- `gens-steward-council-auto-management-design.md` — owns the shared auto-management framework (Autonomy Level, competence/Loyalty stake, embezzlement risk) that Companions & Court Positions' Procurator, Travel's away-from-home household, and Succession & Dynasty's Regency had each already assumed existed without any of them designing it.
- `gens-correspondence-letters-design.md` — owns the remote, lower-stakes communication channel itself (Inbox, news, written instructions, condolence/congratulation) as Travel's asynchronous counterpart — explicitly *not* a substitute for Travel with some Frontier cultures (oral-tradition resistance to writing forces Travel as the only real channel there).

**Evidence:** Steward/Council: *"Companions & Court Positions' Procurator, Travel's away-from-home household, and Succession & Dynasty's own Regency for a minor heir have all explicitly named this system as the principle they're built on without it ever being designed."* Correspondence: *"The remote, deliberately lower-stakes counterpart to Travel."*

---

## 28. Reputation Naming — Epithets & Titles vs. Scandal

**Authoritative:**
- `gens-scandal-design.md` — owns exposure, spread, severity/scope, and Damage Control (Suppression, Spin, Rehabilitation).
- `gens-epithets-nicknames-titles-design.md` — owns the naming layer built directly on top of Scandal's own mechanics; not a competing exposure system.

**Evidence:** Epithets' own §7: *"Nota Censoria (Scandal §2, §7) is this document's own direct negative counterpart to a formal grant... §7's mocking epithet is built directly on that document's own Scandal-spread and Damage Control mechanics."* A mocking nickname can be actively resisted using "the same Damage Control tools Scandal §8 already provides" — this is reuse, not overlap.

---

## 29. Knowledge & Speech — Language & Literacy vs. Education & Culture

**Authoritative:**
- `gens-education-culture-design.md` — owns the Learning investment pipeline (childhood Pedagogy, adult Cultural Patronage, Institutions of Renown).
- `gens-language-literacy-design.md` — owns two distinct derived stats (Literacy, Language Proficiency) that read *from* Education & Culture's Learning investment rather than tracking a competing progression.

**Evidence:** Language & Literacy's own framing states this as a non-overlap directly: *"Neither is a new standalone system competing with anything already built — both read directly from Education & Culture's own Learning investment, Familia's own Legal Status and origin-culture fields, and... every Starting Region document's own Population & Culture Distribution table."* Education & Culture's own Learning investment and Institutions of Renown are named as *"this document's [Language & Literacy's] own primary language-acquisition mechanism."*

---

## 30. Hazards — Disease & Public Health vs. Natural Disasters

**Authoritative:**
- `gens-natural-disasters-design.md` — owns the five-hazard-type roster (Fire, Flood, Earthquake, Drought, Blight & Infestation) and explicitly draws the line at *not* simulating epidemic disease itself.
- `gens-disease-public-health-design.md` — owns Endemic and Epidemic disease mechanics; mirrors Natural Disasters' own multi-hazard design language deliberately, and reads Flood/Famine/Blight aftermath as its own natural trigger condition rather than building a parallel crisis-compounding system.

**Evidence:** Natural Disasters' own Cross-System line: *"Disease & Public Health (§6.13, future): unchanged — a Flood, Famine, or Blight's own crowding/scarcity fallout is that system's natural trigger condition; this document still deliberately draws the line at not simulating epidemic disease itself."* Disease & Public Health confirms the mirrored design intent directly: *"Both layers mirror Natural Disasters' own multi-hazard design language."*

**Minor open item flagged by the source docs themselves, not invented here:** Disease & Public Health's own §10 notes that Sanitation Investment "is built here in full but explicitly flagged as belonging in Policies & Edicts' own Standing Policy roster... on its own next revisit" — the same pattern Religion's Rites Budget and Natural Disasters' Disaster Relief followed before Policies & Edicts absorbed them. This is a known, self-flagged migration, not a contradiction — listed here so it isn't lost.

---

## 31. Final Status

All clusters flagged as open candidates across both passes are now resolved. **31 clusters total**, covering the shared concepts with explicit textual evidence of an authority relationship. Every cluster in this registry cites the corpus's own words rather than asserting a boundary from outside it.

**What remains genuinely open, carried forward from earlier passes rather than newly discovered:**
- The Goods/Buildings contradiction (§10) — still live, still the most concrete pre-Phase-3 blocker.
- Romance & Seduction's exact supersession boundary (§4) — still needs a direct read-through.
- No sync/tie-breaker mechanism for future edits — still unaddressed.
- The Sanitation Investment migration noted in §30 — small, self-flagged, low-risk, but real.

This registry is now considered complete for the purpose the roadmap named it for: every shared concept with a discoverable authority claim has one canonical owner on record. The next artifact per the roadmap's own sequencing (Phase 1, Items 4–7) is the architecture decision records and the cross-system field ledger — this registry is the map those should be built from.
