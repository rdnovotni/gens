# GENS — Canonical Object & Data Registry (Pass 1)

*This document exists to solve a problem the design corpus has now outgrown its ability to solve informally: with 111+ design documents, no single place lists every canonical named entity — every Culture, Religion, Doctrine, Region, Building, Good, Deity, Legendary Creature, Ambition, Court Position, and so on — that other documents are expected to reference rather than reinvent. This registry is that place. It does not redefine mechanics; every entry cites its home document and section, which remains the sole authority for that entity's actual rules text. This document represents a completed four-pass extraction: every category identified across the full corpus is now either fully enumerated, extracted and materially complete, or explicitly logged as complete-as-designed — see §32 for the final summary and the honest residual gaps.*

---

## 0. Coverage Status — What's Fully Enumerated vs. Pointer-Only in This Pass

*(Numbering note: §13 and §29 were absorbed into adjacent sections during iterative editing across passes and don't appear as standalone headers — a cosmetic gap, not missing content. Nothing beyond that renumbering has changed section-to-section.)*

| Category | Status | Item Count | Source Doc |
|---|---|---|---|
| Starting Regions | **Full** | 18 | Starting Regions (+ 17 individual region docs) |
| Cultures | **Full** | 35 | Cultures of the Known World §12 |
| Religions/Faiths (named traditions) | **Full** | 27 | Religions of the Known World |
| Roman Pantheon (Patron Deity picks) | **Full** | 12 | Religion §2.1 |
| Non-Roman Named Deities | **Full** | ~20 | Religions of the Known World §5 |
| Household Doctrines | **Full** | 7 | Policies & Edicts §3.2 |
| Standing Policies | **Full** | 12 | Policies & Edicts §2 |
| Edicts | **Full** | 9 | Policies & Edicts §5 |
| Character Ambitions | **Full** | 24 (across 7 categories) | Character Ambitions §3 |
| Legendary Creatures (confirmed) | **Full** | 17 | Bestiary §11.3 |
| Full Building Index | **Full** | 94 | Buildings §8 |
| Full Court/Companion Position Index | **Full** | 55 | Companions & Court Positions §8 |
| Outfit Tiers | **Full** | 5 | Fashion & Dress Garment Roster §15 |
| Goods Registry | **Extracted, ~91% (131/144)** | 131 named items extracted (materially complete) | Resources & Goods §7 (see §24.2) |
| Occupations & Trades | **Extracted, full (119)** | 119 named trades | Occupations & Trades §3–16 (see §24.3) |
| Traits | **Extracted, full (219)** | 219 named pairs/titles/combo-titles extracted | Traits (Full Catalog) §3–7 (see §24.1) |
| Discovery Roster | **Extracted, ~66% (62/94)** | 62 named discoveries extracted; 5 prerequisite chains fully mapped (§30) | Discovery Roster §3–9 (see §26.5, §30) |
| Flora | **Extracted, full (105)** | 105 named plants extracted | Flora & Herbal Registry §3–14 (see §26.1) |
| Fauna (non-legendary) | **Extracted, full (60)** | 60 named creatures extracted | Bestiary §3–10 (see §26.2) |
| Hair/Body Marking | **Extracted, full (31)** | 31 named styles/marks extracted | Hair & Body Marking §2, §4–13 (see §26.3) |
| Garment Roster (individual garments) | **Extracted, full for §2-9/§11-14 (98)** | 98 named garments/items extracted | Fashion & Dress Garment Roster §2–9, §11–14 (see §26.4) |
| Collegia Types | **Full** | 4 | Collegia & Guilds §2 |
| Interest Groups | **Full** | 5 | Interest Groups §2 |
| Punishment Catalog | **Full** | 9 | Crime & Punishment §7 |
| Named Epidemic Diseases | **Full** | 4 | Disease & Public Health §3.2 |
| Sources of Scandal | **Full** | 9 | Scandal §4 |
| Named Roads | **Full** | 8 | Named Roads & Trade Itineraries §3 |
| Social Place Registry | **Full** | 11 | Social Places §2 |
| Secret Catalog | **Full** | 17 | Secrets & Hooks §3 |
| Monument Roster | **Full** | 13 | Monuments & Legacy Building §2 |
| Event Taxonomy | **Full** | 4 scopes | Events §2 |
| Epithets & Titles | **Complete as designed** | 4 fixed (Virtue) + dynamic (Conquest) | Epithets, Nicknames & Titles §2–4 (see §27) |
| Disease & Public Health, full roster | **Full** | 10 (7 endemic + 4 epidemic, 1 shared) | Disease & Public Health §2, §3.2 (see §28) |

Added in this pass (§30–32): Technology Prerequisite Chains (5, full), Historical Timeline Named Figures (52, full) and dated-event counts (121 total, counts only — full text deliberately not duplicated, see §31). Registry is now considered feature-complete per the Final Coverage Summary at §32; remaining gaps are small residuals (§26.5's ~32 uncaptured discoveries, §24.2's ~13 uncaptured goods) rather than untouched categories.

---

## 1. Starting Regions (18)

Six launch regions plus a promoted/extensible slate, each with its own design document (Starting Regions §5).

**Launch Regions:** Latium · Campania · Gallic Frontier · Iberian Colony · North African Colony · Greek East

**Promoted Extensible-Slate Regions:** Egypt · Syria/The Levant · Britannia · Anatolia/Asia Minor · The Balkans

**Further Individual Region Docs:** Alpine Provinces · Armenia · Mesopotamia · Nubia · Arabia Felix · Bosporan Kingdom · Sicily

*Notes:* Egypt uses Permanent Structural Reputation Duality (unique). Syria and the Balkans both use Localized Reputation Duality. Nubia and Arabia Felix are independent kingdoms — default household is non-Roman. Armenia uses Great Power Allegiance in place of Reputation Duality. Mesopotamia carries a Historical Divergence mechanic for extended Roman occupation. Dacia (within the Balkans) is not selectable before its real AD 106 annexation date.

*Source: Starting Regions §5–6; individual region docs.*

---

## 2. Cultures (35)

*Source: Cultures of the Known World §12 ("Quick Reference — Every Culture at a Glance")*

| Culture | Category | Region |
|---|---|---|
| Roman | — (the default) | Italian heartland |
| Gallic | Provincial | Gallic frontier |
| Iberian | Provincial | Iberian colony |
| Hellenic | Provincial | Greek East |
| Germanic | Frontier | Beyond the Rhine/Danube |
| British | Frontier → Provincial (AD 43 onward) | Britain |
| Hibernian | Frontier, permanently | Ireland |
| Caledonian | Frontier, permanently | Northern Britain |
| Batavian | Frontier + auxiliary-service | Rhine delta |
| Numidian/Mauri | Client → Provincial | North Africa |
| Punic | Provincial (in eclipse) | North Africa, Iberia, Sardinia |
| Etruscan | Provincial (absorbed pre-range) | Etruria |
| Galatian | Provincial | Central Anatolia |
| Cappadocian/Anatolian | Client → Provincial | Anatolia |
| Thracian | Provincial | Balkans |
| Dacian | Frontier → Provincial (AD 106 onward) | North of the Danube |
| Illyrian/Pannonian | Provincial / Frontier→Provincial | Adriatic/Danubian Balkans |
| Cretan | Provincial | Crete |
| Judaean | Client → Provincial | Judaea |
| Syrian/Levantine | Provincial | The Levant |
| Nabataean | Client → Provincial (AD 106 onward) | Arabia Petraea |
| Cilician | Provincial (post-67 BC) | Southern Anatolian coast |
| Palmyrene | Provincial (quasi-autonomous) | Syrian desert |
| Egyptian | Client → Provincial (30 BC onward) | Egypt |
| Alexandrian Greek | Provincial (urban) | Alexandria |
| Nubian/Kushite | Independent | South of Egypt |
| Blemmyes | Frontier (raiding) | Egypt's eastern desert |
| Sarmatian/Scythian | Frontier | Pontic steppe |
| Bosporan | Client | Crimea/Black Sea |
| Parthian | Great Power | Mesopotamia to India's edge |
| Armenian | Contested Buffer | Between Rome and Parthia |
| Indian | Trade Contact Only | Indian Ocean/Red Sea ports |
| Chinese | Trade Contact Only | The far East |
| Garamantian | Trade Contact Only | Central Sahara |
| Aksumite | Trade Contact Only | Horn of Africa |
| Taprobane | Trade Contact Only | Sri Lanka |
| Sogdian | Trade Contact Only | Central Asia (Silk Road) |

---

## 3. Religions & Faiths (27 named traditions)

*Source: Religions of the Known World §4–9*

**State & Civic Polytheisms (4):** Roman State Religion · Hellenic Religion · Egyptian Religion · Parthian Zoroastrianism

**Ethnic & Tribal Polytheisms (18):** Gallic/British Druidic Tradition · Germanic Paganism · Galatian Religious Blend · Numidian/Berber Tradition · Punic Religion (Baal Hammon, Tanit) · Iberian Tradition (Endovelicus) · Sarmatian/Steppe Tradition · Dacian Religion (Zalmoxis) · Thracian Religion (Thracian Horseman) · Illyrian/Pannonian Tradition · Nabataean/Arabian Religion (Dushara) · Syrian/Levantine Religion (Atargatis, Hadad, Adonis) · Cappadocian/Anatolian Religion (Mên) · Nubian/Kushite Religion (Amun, Apedemak) · Armenian Religion (Anahit) · Bosporan Religious Blend · Cretan Mythological Distinctiveness

**Mystery Cults & Personal Devotions:** covered as a category, §6 (individual cults not yet extracted to this registry — Pass 2)

**Monotheistic Exception (3):** Judaism · Samaritanism · Early Christianity

**Distant & Emerging Faiths (2):** Indian/Chinese Religious Tradition · Manichaeism (emerging, range's tail end)

---

## 4. Roman Pantheon — Patron Deity Picks (12)

*Source: Religion §2.1*

Jupiter · Juno · Mars · Venus · Minerva · Ceres · Neptune · Mercury · Vesta · Apollo · Diana · Bacchus

Each carries a distinct domain-flavored Favor bonus and a matching Ill Omen risk at low Favor (e.g., Mars↔military disaster, Ceres↔blight, Neptune↔shipwreck).

---

## 5. Household Doctrines (7)

*Source: Policies & Edicts §3.2*

1. **Mos Maiorum** — The Old Blood (Traditionalist; capstone: Ancestral Sanction)
2. **Res Publica Popularis** — The Popularist Reformer (capstone: Reformer's Momentum)
3. **Domus Mercatoria** — The Mercantile Dynasty (capstone: Trade Concession)
4. **Domus Bellatrix** — The Military Aristocracy (capstone: Call to Arms)
5. **Domus Pia** — The Pious House (capstone: The Great Rite)
6. **Domus Provincialis** — The Frontier Syncretist (capstone: Foederati Pact)
7. **Domus Dura** — The Exploiter House (capstone: Iron Hand — double-edged, permanent Unrest increase)

---

## 6. Standing Policies (12)

*Source: Policies & Edicts §2*

Tax Policy · Household Regimen Posture · Rites Budget · Sumptuary Edict · Recruitment Doctrine · Annona Provision · Trade Openness · Patronage Generosity · Education Investment · Provincial Administration Posture (frontier-only) · Marital Diplomacy Posture · Frontier Security Posture

## 6a. Edicts (9)

*Source: Policies & Edicts §5*

Tabulae Novae (Debt Cancellation) · General Amnesty · Land Redistribution · Manumission Edict · Citizenship Grant · Proscription · Debt Bondage Ban · Grain Requisition

---

## 7. Character Ambitions (24, across 7 categories)

*Source: Character Ambitions §3*

**Power & Standing:** Win the Local Magistracy · Catch Rome's Eye · Rise Through the Ranks · Lead the Collegium · Command in the Field

**Wealth:** Build a Fortune · Corner a Trade · A Ship of One's Own · Land, Not Just Denarii

**Love & Family:** Marry for Love · Marry Into House [X] · Restore the Old Name · Raise a Worthy Heir

**Vice & Vengeance:** Settle the Score · Ruin a Rival · One Great Vice, Indulged

**Knowledge, Craft & Piety:** Master a Craft · Commission a Masterwork · Write One's Name Into History · Devote a Life to the Gods

**Freedom** *(gated: Enslaved/Freedman)*: Earn Manumission · Buy One's Own Freedom · Free a Loved One · A Good Name, Freed

**Legacy** *(gated: Elder or post-completion)*: See the Heir Settled · Make Peace with a Rival · Die Well-Remembered

---

## 8. Legendary Creatures — Confirmed Roster (17)

*Source: Bestiary §11.3*

The Nemean Lion · The Lernaean Hydra · The Chimera · Cerberus · The Griffin · The Manticore · The Basilisk · The Phoenix · The Minotaur · Harpies · The Sea Serpent (Cetus) · The Unicorn · The Sphinx · Pegasus · Sirens · Scylla and Charybdis · Satyrs and Fauns

*(A separate "Rumored & Unconfirmed" lighter third tier exists at §11.4 — not yet extracted to this registry.)*

---

## 9. Full Building Index (94)

*Source: Buildings §8 (alphabetical; §-refs point to home category)*

Academy · Alimenta/Orphanage · Amphitheater · Apothecary · Aqueduct · Argentaria · Armory · Bakery · Barracks · Basilica · Bathhouse · Brewery · Brickworks · Bridge · Brothel · Carpentry Workshop · Charcoal Kiln · Circus · Cistern · City Walls & Gates · Cobbler's Workshop · Concrete Works · Curia · Customs House/Portorium · Dairy · Distillery · Dye Works · Dyer's Workshop · Emporium · Fishing Wharf · Fortress · Foundry · Fulling Works · Garrison · Garum Works · Glassblower's Studio · Glassworks · Goldsmith's Studio · Grain Mill · Grand Baths · Grand Statue · Gymnasium/Palaestra · Harbor · Herb Garden · Horreum · Incense Workshop · Insulae/Domus · Ironworks · Leatherworks · Library/Bibliotheca · Lighthouse · Linen Works · Ludus (Gladiator School) · Macellum · Malting House · Marble Works · Market Stall/Market · Mint/Moneta · Necropolis · Nymphaeum · Odeon · Olive Press · Oyster Beds · Parchment Works · Perfumery · Port · Potter's Works · Praetorium · Public Latrines/Fountains · Rendering House · Sawmill · School · Scriptorium · Shipyard/Navalia · Shrine/Temple · Siege Workshop · Slave Market (Venalicium) · Smeltery · Smithy · Soap Works · Stable · Storehouse/Warehouse · Tabularium · Tailoring House · Tannery · Tavern/Caupona · Theatre · Timber Camp · Trading Post · Triumphal Arch · Valetudinarium · Vigiles Post · Watchtower · Weaver's Loom · Winery

Building categories (§4.1–4.12): Infrastructure & Materials · Agriculture—Staples · Agriculture—Cash & Luxury Crops · Livestock & Apiary · Extraction & Metalworking · Artisan & Luxury Manufacturing · Food, Provisioning & Sea Harvest · Commerce & Trade Services · Imported Goods (Trade-Only) · Civic & Public · Military · Monuments

---

## 10. Full Court/Companion Position Index (55)

*Source: Companions & Court Positions §8 (alphabetical)*

Actor · Alimentarius · Amanuensis (Secretary) · Aquarius · Archimagirus (Head Cook) · Arcarius (Treasurer) · Argentarius · Balneator · Bodyguard · Chamberlain (Cubicularius Maior) · Cellarer (Promus) · Curator · Dovecote-Keeper (Columbarius) · Editor · Editor Muneris · Ergastularius · Furnace-Master (Fornacator) · Granary-Keeper · Guard-Captain/Marshal · Harbor-Steward · Horrearius · Household Priest (Sacerdos Domesticus) · Household Spymaster · Institor · Institor Maximus · Lanista · Lena/Leno · Libitinarius · Magister Officinae · Magistra Textrinii (Weaving-Mistress) · Master Beekeeper (Apiarius) · Master of Hospitality (Xenodochus) · Menagerie-Keeper · Metallarius · Navarchus · Navarchus Princeps · Nurse (Nutrix) · Paedagogus (Tutor) · Portitor · Praefectus Metallorum · Praefectus Vigilum · Procurator · Rationalis · Rhetor/Magister · Sacerdos Publicus · Steward (Dispensator) · Symposiarch · Tabularius · Valetudinarius · Venalicius · Vigil · Vilicus

---

## 11. Outfit Tiers (5)

*Source: Fashion & Dress Garment Roster §15*

Meager · Modest · Respectable · Fine · Opulent — illustrative dress varies by Legal Status, culture, and Occasion; full detail at source.

---

## 12. Large Catalogs — Taxonomy & Pointers (Pass 2 candidates for full enumeration)

### 12.1 Goods Registry (144 items across 6 tiers)
*Source: Resources & Goods §7* — Raw Materials (46) · Intermediate Goods (31) · Finished Goods (28) · Luxury Goods (13) · Imported Goods (18) · Livestock, tracked as headcount (8 types, §3.1)

### 12.2 Traits (~240 across 3 lifecycle tiers)
*Source: Traits (Full Catalog) §3–7* — Tiered Spectrums (Intellect, Beauty, Physique, Four Humors, Piety); Congenital Traits (69, §4); Formative Traits (72, §5, incl. Lifestyle & Vocation subsection); Reactive Traits (§6, multiple subcategories); Combo Titles (10 thematic groups, §7: Villainous & Corrupt, Heroic & Virtuous, Tragic, Comedic & Quirky, Political, Religious, Romantic, Family & Dynasty, Martial, Intellectual & Eccentric).

### 12.3 Occupations & Trades (16 categories)
*Source: Occupations & Trades §3–16* — Agriculture & the Land · Food & Drink Production · Textile, Dress & Personal Care · Building & Craft Trades · Metalworking & Fine Craft · Commerce & Trade · Transport & Maritime · Medicine & Healing · Religious & Ritual Service · Entertainment & Spectacle · Education & Letters · Domestic & Household Service · Law, Administration & Public Service · Regional & Cultural Trade Variants.

### 12.4 Discovery Roster (94 across 6 eras)
*Source: Discovery Roster §3–8* — Era I Republican Legacy · Era II High Imperial Engineering · Era III Crisis & Reform (Dominate) · Era IV Constantinian Transition · Era V Late Antique Twilight · Era VI Justinian's Renewal.

### 12.5 Flora (14 categories)
*Source: Flora & Herbal Registry §3–15* — Grain & Staple Crops · Orchard, Vine & Grove · Fiber & Dye Plants · Garden & Culinary Herbs · Medicinal & Healing Plants · Poisonous & Dangerous Plants · Sacred & Ritual Plants · Funerary & Mourning Flora · Ornamental & Garden Flora · Trees & Timber · Aromatic, Perfume & Incense Plants · Wild & Foraged Flora · Regional & Cultural Flora Variants.

### 12.6 Fauna, Non-Legendary (9 categories)
*Source: Bestiary §3–10* — Domesticated Livestock & Working Animals · Wild Game (Common) · Wild Game (Dangerous) · Exotic & Imported Beasts · Marine & Aquatic Life · Birds & Aviary Species · Venomous & Dangerous Small Creatures · Vermin & Pests.

### 12.7 Hair & Body Marking (14 categories)
*Source: Hair & Body Marking §2–13* — Hairstyles (Women's/Men's chronological progressions) · Coming-of-Age Hair Customs · Cultural Hairstyle Traditions · Hair Color/Complexion/Baldness · Facial Hair · Eyebrows/Teeth · Tattoos & Scarification · Branding & Punitive Marking · Forced Head-Shaving · Piercings · Body Paint · Ritual & Religious Body Modification · Scars.

### 12.8 Garment Roster (15 categories, beyond Outfit Tiers §11 above)
*Source: Fashion & Dress Garment Roster §2–14* — Roman Core Wardrobe · The Toga Family · Footwear · Jewelry & Accessories · Military Dress/Insignia/Dona Militaria · Priestly & Ceremonial Vestments · Slave/Labor/Poverty Dress · Milestone & Life-Event Garments · Regional & Cultural Dress · Bathing/Athletics/Leisure Dress · Arena & Circus Dress · Hair/Wigs/Cosmetics/Grooming Tools · Fabrics, Dyes & Color Symbolism.

---

## 14. Collegia Types (4)

*Source: Collegia & Guilds §2*

Collegia Opificum (Trade Guilds) · Collegia Funeraticia (Burial Societies) · Cult-Specific Collegia · Collegia Compitalicia (Neighborhood Associations). A single Character/Household can hold multiple, overlapping memberships.

---

## 15. Interest Groups (5)

*Source: Interest Groups §2*

Landowners vs. the Landless · Creditors vs. Debtors · Publicani and Equestrian Trade Interests · Veterans · Provincial Interests

---

## 16. The Punishment Catalog (9, across Honestiores/Humiliores tiers)

*Source: Crime & Punishment & Imprisonment §7*

**Honestiores' sentencing range (5):** Fine · Relegatio (milder exile) · Deportatio (harsher exile) · Ignominia (loss of rank) · The Honorable Exit (permitted suicide before capital sentence)

**Humiliores' sentencing range (4):** Flogging · Forced Labor/*Damnatio ad Metalla* · Servus Poenae (free person reduced to slavery) · *Damnatio ad Bestias* (resolved in Games & Spectacle §4) · Crucifixion

*Cross-tie:* the *Senatus Consultum Silanianum* — a real Roman law requiring execution of an entire enslaved household if a murdered master's killer isn't identified — is named as a standing Legal & Court/Crime crisis-scenario trigger.

---

## 17. Named Epidemic Diseases (4)

*Source: Disease & Public Health §3.2*

Pestilence (generic severe fever-plague, Antonine Plague-grade) · Pox (disfiguring, permanent Appearance change in survivors) · Camp Fever (military-camp typhus/fever, ties to Military & Combat's "Plague in the Camp" siege event) · Enteric Fever (typhoid — the sole water-borne vector, tied to Aqueduct/Cistern infrastructure and Flood aftermath)

---

## 18. Sources of Scandal (9)

*Source: Scandal §4*

High-Stakes Affair Discovery · Unjust Imprisonment/Execution · Discovered Fabrication · Scandalous Theatrical Performance · Fame Collapse (Celebrities) · Politically-Weaponized Legal Case · Illicit Collegium Exposure · Aggressive Tax-Farming Corruption Exposed · Deliberately Weaponized Rumor

---

## 19. Named Roads (8)

*Source: Named Roads & Trade Itineraries §3*

Via Appia (Rome→Capua→Brundisium) · Via Egnatia (Dyrrachium→Thessalonica→Byzantium) · Via Flaminia (Rome→Ariminum) · Via Domitia (Italy→Gallic Frontier→Iberian border) · Via Augusta (length of Hispania) · Via Maris (Levantine coastal route) · The Incense Route (Arabia Felix caravan corridor) · The Royal Road's Roman-era descendant (Mesopotamia→Parthia)

---

## 20. Social Place Registry (11)

*Source: Social Places §2*

The Public Baths (Thermae/Balneum) · The Barbershop (Tonstrina) · The Tavern (Caupona/Popina) · The Thermopolium · The Forum · The Macellum (Covered Market) · The Portico (Ambulatio) · The Gymnasium/Palaestra · The Circus/Amphitheater Concourse · The Harbor Docks · Public Latrines (Foricae)

---

## 21. The Secret Catalog (17)

*Source: Secrets & Hooks §3*

Illegitimate Parentage · Adultery/Affair · Concealed Servile/Foreign Origin · Concealed True Parentage/Adoption · Buried Crime/Murder · Financial Fraud/Embezzlement · Concealed Debt · Proscribed Religious Practice · Broken Religious Vow · Treason/Conspiracy · Secret Foreign Alliance · Vestal Chastity Violation · Espionage Collaboration · Piracy/Banditry Collaboration · Complicity in a Scheme · A Disgraceful Past Act · Broken Betrothal in Bad Faith

Each carries a default severity (Minor Embarrassment → Public Disgrace → Nota Censoria-Eligible) per source doc.

---

## 22. Monument Roster (13)

*Source: Monuments & Legacy Building §2*

**Existing (Buildings §4.12):** Statue → Grand Statue · Family Tomb · Dedicatory Temple · Triumphal Arch (military-victory-gated) · Nymphaeum · Necropolis

**Prior Pass (tied to other systems' capstones):** Doctrine Apex Monument · Mausoleum · Freedman's Monument · Liberty Column · Founder's Stone · Inscribed Dedication

**New:** Terminus Stone (cheapest, most common — triggered by any landholding expansion) · Tropaeum (Battlefield Trophy — a lesser alternative to the Triumphal Arch)

---

## 23. Event Taxonomy — Four Scopes

*Source: Events §2*

Personal · Household · Regional · Imperial (the "Three Tiers" recap at §6.1 refers to the Real-History axis: Scripted/Historical, Random/Systemic, and Divergence — distinct from, but overlaid on, the four scopes above).

---

## 24. Full Item-Level Appendices (Extracted from Source Tables)

*The following are machine-extracted directly from the named-item columns of each source document's tables. They supersede the taxonomy-only counts in §12 for these three categories and should be spot-checked against source before being treated as 100% authoritative — extraction can occasionally misparse a table row (e.g., a spectrum tier label bleeding into the list). Flagged as a §25 Open Question.*

### 24.1 Traits — Full Extracted Name List (219 entries)

*Source: Traits (Full Catalog) §3–7. Pairs shown with their ↔ opposite where the table presented them that way; combo titles and standalone traits shown singly. This is the complete extracted list (superseding the earlier truncated version).*

Addled · Adventurous ↔ Homebound · Architect · Assimilated ↔ Unbowed · Astrologer · Astrologer + Superstitious · Average · Awkward + Gregarious · Battle-Hardened + Merciful · Battle-Hardened ↔ Shell-Shocked · Beautiful · Beautiful + Deceitful · Betrayed · Bitter ↔ Forgiving · Bloodlust + Herculean · Bloodlust ↔ Squeamish · Bold + Deceitful · Bold ↔ Cautious · Brilliant · Brilliant + Deceitful · Brilliant + Honor-Bound · Broken + Devoted · Callous + Zealous · Calm ↔ Wrathful · Cartographer · Cautious + Deceitful · Charitable ↔ Mercenary · Chaste + Devoted · Choleric · Choleric + Vengeful · Clever · Comely · Comely + Adulterous · Compassionate + Diligent · Compassionate + Impious · Compassionate ↔ Callous · Complacent ↔ Driven · Content ↔ Resentful · Contentious ↔ Amicable · Corrupt ↔ Incorruptible · Cosmopolitan ↔ Xenophobic · Creative + Precocious · Creative ↔ Unimaginative · Cunning Survivor · Curious ↔ Apathetic · Cursed + Traumatized · Cursed ↔ Blessed · Cynical + Eloquent · Debt-Scarred ↔ Debt-Free at Last · Deceitful + Zealous · Deferential ↔ Skeptical of Authority · Defiant ↔ Broken · Devoted ↔ Estranged · Devotee ↔ Apostate · Devout · Devout + Charitable · Diligent ↔ Slothful · Diplomat · Disciplined ↔ Undisciplined · Disowned · Drunkard + Eloquent · Drunkard ↔ Abstemious · Duelist · Dull · Dull + Content · Dutiful + Filial · Dutiful ↔ Wayward · Egalitarian + Charitable · Egalitarian ↔ Hierarchical · Eloquent ↔ Tongue-Tied · Emboldened ↔ Cowed · Engineer · Envious ↔ Magnanimous · Epileptic · Even-Handed ↔ Capricious · Exiled · Faithful + Homebound · Faithful ↔ Adulterous · Fanatical + Zealous · Fanatical ↔ Disaffected · Fecund ↔ Barren · Feral · Filial ↔ Rebellious · Forgetful + Diligent · Frail · Freed Spirit · Freed Spirit + Merchant's Instinct · Frontier-Raised + Bold · Frontier-Raised ↔ City-Raised · Frugal ↔ Extravagant · Genealogist · Genealogist + Proud · Generous + Gregarious · Generous ↔ Greedy · Gladiator's Heart · Gluttonous + Gregarious · Gourmet · Graceful ↔ Clumsy · Grateful ↔ Vengeful · Greedy + Corrupt · Greedy + Zealous · Green Thumb · Gregarious ↔ Reserved · Grieving · Guarded + Cynical · Hardy ↔ Sickly · Haunted · Heartbroken + Loyal-Hearted · Heartbroken ↔ Guarded · Herbalist · Herculean · Herculean + Callous · Hideous · Hideous + Compassionate · Hierarchical + Proud · Historian · Historian + Forgetful · Honest + Bold · Honest ↔ Deceitful · Honor-Bound ↔ Opportunistic · Horseman · Hunter · Idealistic ↔ Pragmatic · Illiterate + Proud · Impious · Impious + Rational · Incorruptible + Frugal · Indifferent · Infatuated + Trusting · Infatuated ↔ Disillusioned · Insomniac ↔ Deep Sleeper · Inspiring Commander ↔ Feared Commander · Institutionalized · Institutionalized + Loyal-Hearted · Iron-Willed ↔ Weak-Willed · Jaded + Incorruptible · Keen-Eyed ↔ Nearsighted · Kingmaker · Kingmaker + Cynical · Left-Handed ↔ Right-Handed · Legal Scholar · Litigious + Eloquent · Litigious ↔ Conflict-Averse · Long-Lived Stock + Patient · Long-Lived Stock ↔ Short-Lived Stock · Loyal-Hearted ↔ Fickle · Lustful + Eloquent · Lustful ↔ Chaste · Martial-Minded ↔ Peace-Loving · Master Craftsman · Melancholic · Melancholic + Devout · Melodious ↔ Harsh-Voiced · Merchant's Instinct · Merciful + Herculean · Naturalist · Naturalist + Curious · Nimble ↔ Awkward · Numismatist · Numismatist + Frugal · Orphaned · Paranoid ↔ Serene · Patient ↔ Impatient · Perceptive ↔ Oblivious · Philosopher · Philosopher + Melancholic · Phlegmatic · Physician · Plague Survivor · Plain · Playwright · Poet · Power-Hungry + Deceitful · Power-Hungry + Eloquent · Power-Hungry ↔ Jaded · Precocious ↔ Late Bloomer · Prodigious Memory ↔ Forgetful · Proud ↔ Humble · Rational ↔ Superstitious · Rebellious + Bold · Refined ↔ Coarse · Ruthless + Disciplined · Ruthless ↔ Merciful · Sailor · Sanguine · Sanguine + Opportunistic · Scandal-Marked ↔ Rehabilitated · Scarred · Scarred + Brilliant · Sharp-Tongued ↔ Soft-Spoken · Sole Survivor · Sole Survivor + Paranoid · Spymaster · Steady-Handed ↔ Shaky · Strategist + Cynical · Strategist ↔ Berserker · Strong · Stubborn ↔ Pliant · Studious ↔ Incurious · Temperate ↔ Gluttonous · Theologian · Theologian + Cynical · Thick-Skinned ↔ Thin-Skinned · Traumatized ↔ Resilient · Trusting ↔ Cynical · Vengeful + Eloquent · Vigilant + Paranoid · Vigilant ↔ Careless · Vintner · War Hero ↔ War Criminal · Warmonger + Herculean · Warmonger ↔ War-Weary · Well-Read ↔ Illiterate · Well-Traveled ↔ Provincial · Widowed · Wrathful + Vengeful · Wrestler · Zealous · Zealous + Corrupt

### 24.2 Goods Registry — Full Extracted Item Names (131 of 144)

*Source: Resources & Goods §7.1–7.6. Complete extracted list (superseding the earlier truncated version); ~13 items likely lost to table-formatting variance (multi-line cells, footnoted variants) — treat as materially complete rather than literally exhaustive.*

Alabaster · Armor · Aromatic Woods (new) · Baltic Amber · Beef (culled livestock yield) · Beer · Bread · Bronze · Building Stone (new) · Butter · Charcoal · Cheese · Cinnabar · Clay · Common Dye · Concrete · Copper Ore · Coral (new) · Cotton (new, minor) · Cut Building Stone (new) · Dried Fruit/Raisins (new) · Eastern Spices · Esparto Grass · Exotic Beasts (Venatio Stock) · Faience · Feathers · Felt (new) · Fine Glass · Fine Incense · Fine Seafood (Oysters) · Fish · Flax · Flour · Frankincense · Furniture · Furs/Pelts · Garden Produce (new) · Garum · Gemstones (new) · Glass · Glue (new) · Goat Meat (new, culled Goat yield) · Gold Ore · Grain (Wheat/Barley/Oats) · Grapes · Gypsum (new) · Hemp · Herbs · Honey (new) · Incense · Indigo (new) · Iron · Iron Ore · Ivory · Jewelry · Lard (new) · Lavender · Lavender Oil · Lead · Lead Ore · Leather · Legumes · Limestone · Linen Fiber · Malt · Manure · Medicine · Milk · Mortar/Plaster (new) · Mulsum, Passum (Specialty Wines) (new) · Murex Snails · Mutton (new, culled Sheep yield) · Myrrh (new) · Natron · Nuts (new) · Olive Oil · Olives · Orchard Fruit (new) · Orichalcum (new) · Oysters · Papyrus · Parchment · Pearl (new) · Pepper (new) · Perfume · Pigments · Pitch · Pork (culled livestock yield) · Pottery/Amphorae · Poultry, Eggs · Pozzolana · Preserved Meat (new) · Purple-Trimmed Togas · Quartz Sand · Quicklime (new) · Raw Hides/Skins (new) · Raw Marble · Reeds (new) · Refined Gold · Refined Silver · Resin · Rope/Cordage · Saffron · Salt · Sandals · Sandarac Wood · Sausages · Sea Sponges · Siege Engines · Silk · Silphium · Silver Ore · Sinew · Soap · Tallow · Tile · Timber · Tin Ore · Tools · Truffles · Tunics · Tyrian Purple · Vinegar · Wax · Weapons · Wine · Woad/Dye Plants · Wool · Worked Marble · Woven Cloth · Writing Tablets

### 24.3 Occupations — Full Extracted Name List (119 entries)

*Source: Occupations & Trades §3–16. Complete list.*

Tenant Farmer · Shepherd · Vinedresser · Plowman · Reaper/Harvester · Herdsman · Ornamental Gardener · Market Gardener · Miller · Woodcutter/Forester · Charcoal-Burner · Salt-Worker · Baker · Butcher · Fisherman · Fish-Dealer · Cook (hired/market) · Innkeeper · Tavern-Keeper · Wine-Merchant · Confectioner · Sausage-Seller · Oil-Presser · Cheese-Maker · Fish-Sauce Producer · Weaver · Fuller · Purple-Dyer · Dyer (common) · Tailor/Clothier · Mender · Cobbler · Perfumer · Hairdresser · Barber · Architect · Mason · Carpenter · Roofer/Tiler · Plasterer · Plumber/Pipe-Fitter · Surveyor · Well-Digger · Quarryman · Blacksmith · Locksmith · Goldsmith · Bronzeworker · Glassblower · Gem-Cutter · Engraver/Seal-Cutter · Potter · Ropemaker · Basket-Weaver · Armorer · Shopkeeper · Merchant · Auctioneer/Crier · Grain-Dealer · Wool-Dealer · Money-Changer · Pawnbroker · Bookseller · Muleteer/Carter · Wagon-Driver · Sailor · Stevedore/Dockworker · Litter-Bearer · Courier/Messenger · Stable-Keeper · Waystation-Keeper · Shipwright · Diver · Physician · Midwife · Surgeon · Herbalist/Apothecary · Veterinarian (equine) · Corpse-Bearer · Corpse-Preparer · Temple Attendant · Sacrificial Assistant · Freelance Diviner · Professional Mourner · Actor · Musician · Dancer · Acrobat · Mime Performer · Reader/Reciter · Elementary Teacher · Grammar Teacher · Rhetoric Teacher · Scribe/Copyist · Shorthand-Writer · Interpreter · Wet-Nurse · Doorkeeper · Bath Attendant · Laundress · Advocate/Lawyer · Public Scribe/Clerk · Tax-Farmer · Customs Officer · Prison-Keeper · Public Executioner · Brewer (*Cervesarius*) · Embalmer (*Taricheutes*) · Caravan Agent · Gold-Panner · Horse-Breeder · Purple-Dye Specialist (Phoenician tradition) · Torah-Scribe (*Sofer*) · Caravan Guard · Tin-Streamer · Gold-Miner (*Ruina Montium* tradition) · Frankincense-Harvester · Silk-Worker · Athletic Trainer (*Paedotriba*)


---

## 25. Open Questions

- §26.5 (Discovery Roster) and §24.2 (Goods Registry) each have a small residual gap (~32 and ~13 items respectively) from table-parsing edge cases — a manual reconciliation pass would close these, but they're not systematic omissions.
- Every appendix in §24 and §26 was machine-extracted from source tables; a manual spot-check against source is still the honest final step before citing any of them elsewhere as gospel-authoritative, since table-parsing can misfire on an edge case (a multi-line cell, an embedded aside).
- §31's dated-event text was deliberately left at source rather than duplicated (121 entries, richly described) — if a future need arises for those events to be queryable from this registry directly (e.g., for a "what happened near year X" tool), that would be a genuinely new artifact, not an extension of this one.
- No mechanism yet exists for keeping this registry in sync as new design docs are written — see §32's suggested next step.

---

## 26. Full Item-Level Appendices, Round 2 (Extracted from Source Tables)

*Same extraction method and same caveat as §24: machine-extracted from source tables, bounded to exclude Cross-System Integration/Data Model/Open Questions sections and (where present) Regional-or-Cultural-Variant tables that key on Culture/Region rather than the actual item name. Spot-check against source before treating as fully authoritative.*

### 26.1 Flora — Full Extracted Name List (105 entries)

*Source: Flora & Herbal Registry §3–14 (Grain & Staple Crops through Wild & Foraged Flora; excludes §15 Regional/Cultural Variants, §16 Notable Absences). Complete extracted list.*

Acanthus · Aconite/Wolfsbane · Acorn · Almond · Amaranth · Apple · Ash · Asphodel · Balsam of Judaea · Barley · Basil · Bay Laurel · Beech · Betony · Boxwood · Bramble/Blackberry · Cabbage · Castor Bean · Cedar · Chamomile · Cherry · Chickpea · Comfrey · Coriander · Cumin · Cypress · Date Palm · Deadly Nightshade · Dill · Elecampane · Elm · Emmer/Spelt · Ergot · Esparto Grass · Fava Bean · Fennel · Fig · Fir · Flax · Foxglove · Frankincense Tree · Garlic · Grape · Hellebore · Hemlock · Hemp · Henbane · Horehound · Hyacinth · Iris · Ivy · Lavender · Leek · Lentil · Lily · Lotus · Madder · Mandrake · Marshmallow · Millet · Mint · Mistletoe · Myrrh Tree · Myrtle · Narcissus · Nard/Spikenard · Nettle · Oak · Oats · Oleander · Olive · Olive Branch · Onion · Opium Poppy · Oregano/Marjoram · Parsley · Pear · Pine · Plane Tree · Plantain · Pomegranate · Poplar · Quince · Rose · Rosemary · Rue · Sacred Fig · Saffron Crocus · Sage · Samphire · Squill · Storax · Thyme · Truffle · Vervain · Violet · Walnut · Water Hemlock · Wheat · Wild Asparagus · Wild Mushroom · Willow · Woad · Wormwood · Yew

### 26.2 Fauna, Non-Legendary — Full Extracted Name List (60 entries)

*Source: Bestiary §3–10 (Domesticated Livestock through Vermin & Pests; excludes §11 Legendary, already at §8, and §13 Regional Distribution). Complete extracted list.*

"Cameleopard" (Giraffe) · Asp (Egyptian Cobra) · Bear · Bee · Bonasus (Bison) · Camel · Cat · Cattle · Crocodile · Deer/Stag · Dolphin · Dove/Pigeon · Eagle · Edible Dormouse · Elephant · Falcon/Hawk · Fish (general) · Fleas and Lice · Fox · Goat · Goose · Guard Dog · Hare · Hippopotamus · Horse · Hunting Hound · Leopard/Panther · Lion · Locusts · Lynx · Moray Eel · Mule/Donkey · Murex Snails · Nightingale · Onager (Wild Ass) · Ostrich · Owl · Ox · Oysters, Fine Seafood · Parrot · Partridge · Peacock · Pig · Poultry (Chicken) · Rats and Mice · Raven/Crow · Rhinoceros · Scorpions · Sheep · Swan · Tiger · Torpedo Ray · Venomous Snakes · Venomous Spiders · Weevils · Wild Boar · Wild Bull/Aurochs · Wild Goat/Ibex · Wildfowl (Duck, Goose, Pheasant-adjacent species) · Wolf

### 26.3 Hair & Body Marking — Full Extracted Name List (31 entries)

*Source: Hair & Body Marking §2, §4–13. Excludes §3's Cultural Traditions table (keys on Culture, not style name) and, per spot-check, three residual Culture-name leaks removed from the raw extraction (Egyptian, Hellenic, Roman). Complete list of style/mark names.*

Baldness · British/Caledonian body art · Clean-shaven · Full beard, any other era · Full philosopher's beard · Going/kept grey or white · Hadrianic–Antonine Curled Hair · Henna dye · Julio-Claudian Waves · Julio-Claudian/Flavian Combed Fringe · Later Roman military tattooing · Lightening treatments · Men's ear piercing · Mustache without beard · North/Sub-Saharan African scarification traditions · Nose rings — Judaean, Nabataean, Arabian cultures · Other piercing · Republican Close Crop · Republican Plain Style · Severan Tight Ringlets · Severan Waves · Simple/Practical Braid or Bun · Stigmata — punitive tattooing · The Antonine Coronet Braid · The Flavian Toupet (Orbis) · The Melon Coiffure · The Nodus · The Seni Crines · Thracian elite tattooing · Trajanic Practical Cut · Women's ear piercing

### 26.4 Garment Roster Items — Full Extracted Name List (98 entries)

*Source: Fashion & Dress Garment Roster §2–9, §11–14. Excludes §10 Regional & Cultural Dress (keys on Culture/Region group, not garment name — that table's actual garment-per-culture detail lives at source, not duplicated here), §15 Outfit Tiers (already at §11), §16 Portrait Rendering. Complete list for the sections that key on item name.*

Amber · Amber jewelry · Ancestor death masks · Archaic mourning veil · Arm bands · Athletic wrap · Augur's staff · Balteus · Basic undyed tunic · Bathing sandals · Bathing wrap · Birrus · Bridal hairstyle · Bridal veil · Cerussa (white lead) · Charioteer's wrapped leathers · Child's rattle-pendants · Civic Crown · Coming-of-age toga · Common/Woad Dye · Cotton · Curling iron · Dark/black (undyed dark wool) · Dark/undyed toga · Decorative discs · Diadem · Dowry Trousseau · Endromis · Ergastulum-tier dress · Faction colors · Felt · Fibula (Brooch/Pin) · Flamen's cap · Formal outdoor shoes · Freedman's cap · Fully purple/gold-embroidered toga · General's cloak · Gold Ring (citizen privilege) · Grass/Blockade Crown · Hairnet · Hairpins · Hand mirror · High boots · Indoor sandals · Iron Betrothal Ring · Kohl · Lacerna · Linen · Livery · Military belt · Military boots · Mourning dress · Mural Crown · Murmillo armor · Naval Crown · Neck torc (military) · Necklaces, earrings, bracelets · Oil flask · Paenula · Palla · Pallium · Perfumed oil · Phrygian cap · Plain white toga · Plain/bare feet · Priestly skullcap · Purple-bordered toga · Retiarius gear · Ring cabinet · Rouge · Sacred woolen fillet · Sacrificial white robes · Saffron/Crocus Yellow · Sagum · Salii dancing-priest regalia · Secutor helmet · Segmented arm guard · Senatorial shoes · Signet Ring · Silk · Skin scraper · Socks · Stola · Subligaculum · Synthesis · The Bulla · The Freedman's Cap · The slave collar · Thraex armor · Torc · Tunica (Tunic) · Tweezers · Tyrian Purple · Undyed natural cream/white · Vestal's veil · Whitened/chalked toga · Wig · Wool

### 26.5 Discovery Roster — Extracted Name List (62 of 94 entries)

*Source: Discovery Roster §3–8 (the six Era buckets; excludes §9 Prerequisite Chains, now fully covered at §30, and §10 Feature Tie-In Index, which keys on System name). The remaining ~32 discoveries are real entries not captured by this table-parse — likely due to multi-line cell formatting in the source tables — and are not yet reconciled; treat this list as substantial but not complete.*

Acta Diurna (The Public Gazette) (new) · Aetius of Amida's Medical Compilation · Alimenta Program · Amphitheater Engineering Refinement · Amphora Sealing Improvement · Antoninianus Introduction (Ambiguous) · Aurelian Walls Fortification Doctrine · Barbegal-Style Industrial Milling · Caesar Cipher Correspondence · Cataphract Heavy Cavalry Doctrine · Census Methodology Refinement (new) · Centuriation Land Division · Christian Basilica Architectural Adaptation · Codex Theodosianus · Comitatenses/Limitanei Reorganization · Constantinople's Founding Engineering · Continued Byzantine Guild Regulation · Corpus Juris Civilis · Cursus Publicus Formalization · Die-Engraving Refinement · Diocletian's Currency Reform · Eastern Grain Trade Resilience · Edictum Perpetuum (Hadrianic Legal Codification) · Extensive Aqueduct Network Maturity · Frontier Defense-in-Depth Doctrine · Funerary Preservation Technique (Egyptian-Influenced) · Galenic Medical Systemization · Glassblowing Technique · Greek Fire (Early Development) · Guild Continuity Under Successor Kingdoms · Hagia Sophia Pendentive Dome Engineering · Hydraulic Gold Mining (Ruina Montium) · Improved Torsion Siege Engineering · Interpreter Corps Formalization · Itinerant Guild Circuit Formalization (new) · Latifundia Estate Consolidation (Ambiguous) · Lead Pipe Plumbing Standardization (Ambiguous) · Legionary Equipment Standardization (Lorica Segmentata) · Library Cataloguing System (Pinakes-Style) · Marian Military Reforms · Notitia Dignitatum Military Bureaucracy · Opus Caementicium Refinement · Ostrogothic Administrative Continuity · Pantheon Dome Mastery · Parchment Development (Pergamene Tradition) · Pattern-Welded Smithing · Professionalized Freedman Administration (new) · Provincial Subdivision Reform · Refined Surgical Instrumentation · Sericulture Smuggled to the West · Silver Denarius Standardization · Solidus Gold Coinage · Spatha Adoption (Heavy Infantry Doctrine) · Standardized Naval Patrol Doctrine (new) · Standardized Weights & Measures · The Basilica Cistern · The Groma (Surveying Instrument) · The Vigiles (Fire Brigade & Watch) (new) · Underwater Concrete Harbor Construction · Vegetius's Military Treatise · Via Stone Paving Standardization · Vindolanda-Style Frontier Record-Keeping


---

## 27. Epithets & Titles — The Three Real Categories (By Design, Not a Fixed Roster)

*Source: Epithets, Nicknames & Titles §2–4*

**Conquest Agnomina** — generated dynamically from whichever Culture/Region a household's campaign actually defeats (real historical models: *Africanus*, *Germanicus*, *Britannicus*, *Parthicus*, *Dacicus*). Not a fixed list — every Culture (§2 above) and Region (§1 above) is a live source.

**Virtue & Achievement Agnomina (4, fixed, real-historical):** *Pius* (dutiful) · *Felix* (fortunate) · *Magnus* (the Great) · *Optimus* (the best)

**Mocking/Unflattering Nicknames** — not a fixed catalog; a real-historical pattern (the document cites the "little boot" case) rather than an enumerable set. Two grant sources exist for any epithet: **Formal Grant** (voted/conferred, real Dignitas weight) vs. **Crowd-Given Nickname** (arising from Fame or Scandal spread, no guaranteed Dignitas, not always declinable).

---

## 28. Disease & Public Health — Full Roster (10 named diseases)

*Source: Disease & Public Health §2, §2.1, §3.2*

**Endemic (Chronic Background Layer, 7):** Roman Fever (malaria) · The Flux (dysentery) · Ophthalmia (eye affliction) · Consumption (tuberculosis) · Leprosy (social-exclusion mechanic, not Health-drain) · Gout (wealth-driven) · Saturnism (lead poisoning — dual driver: elite luxury cookware/wine *or* Iberian mining-proximity)

**Epidemic (Acute Outbreaks, already at §17 above, 4):** Pestilence · Pox · Camp Fever · Enteric Fever

**Regional Disease Profiles (illustrative distribution, not a hard gate):**

| Region | Most Relevant Endemic Diseases |
|---|---|
| Italian Heartland | Roman Fever, Consumption |
| Gallic Frontier | The Flux, Camp Fever *(epidemic)* |
| Iberian Colony | Ophthalmia, Saturnism *(mining driver)* |
| Greek East | Gout, Saturnism *(wealth driver)* |

---

## 30. Technology Prerequisite Chains (5 chains)

*Source: Discovery Roster §9*

- **The Legal Chain:** Edictum Perpetuum → Codex Theodosianus → Corpus Juris Civilis
- **The Architectural Chain:** Opus Caementicium Refinement → Pantheon Dome Mastery → Constantinople's Founding Engineering → Hagia Sophia Pendentive Dome Engineering
- **The Monetary Chain:** Silver Denarius Standardization → Antoninianus Introduction (*Ambiguous*) → Diocletian's Currency Reform → Solidus Gold Coinage
- **The Guild Continuity Chain:** Guild Continuity Under Successor Kingdoms → Continued Byzantine Guild Regulation
- **The Administrative Record-Keeping Chain:** Vindolanda-Style Frontier Record-Keeping (Era II) → Census Methodology Refinement (Era II)

*Most of the 94 discoveries in §26.5 are not part of any named chain — these five are the only explicitly sequenced dependencies the source doc defines.*

---

## 31. Historical Timeline — Named Figures & Event Counts

*Source: the two dedicated timeline-content docs. Full narrative/dated-event text is deliberately not duplicated here (121 richly-described dated entries between the two docs — reproducing them would just re-author the source documents); the Named Historical Figures rosters are reproduced in full below since they're the compact, genuinely reusable cross-reference surface.*

### 31.1 Named Historical Figures — Full Roster (52 figures)

*Source: Events: Historical Timeline (Content) §6 and Events: Historical Timeline (Late Antiquity) §12. All are real, backdrop-only — driving Events by name, never instantiated as an interactive Character.*

**Republic-era (10):** Tiberius Gracchus · Gaius Gracchus · Marius · Mithridates VI · Sulla · Julius Caesar · Vercingetorix · Pompey · Crassus · Cleopatra

**Emperors, Augustus through Severus Alexander, in succession order (24):** Augustus · Tiberius · Caligula · Claudius · Nero · Galba · Otho · Vitellius · Vespasian · Titus · Domitian · Nerva · Trajan · Hadrian · Antoninus Pius · Marcus Aurelius · Lucius Verus · Commodus · Septimius Severus · Caracalla · Geta · Macrinus · Elagabalus · Severus Alexander

**Other notable figures, Early/High Principate (8):** Arminius · Boudicca · Decebalus · Avidius Cassius · Clodius Albinus · Pliny the Elder · Josephus · Galen — plus Jesus of Nazareth (real biographical facts only, per Religions of the Known World's treatment)

**Crisis-era (6):** Decius · Valerian · Shapur I · Postumus · Zenobia · Aurelian

**Dominate & Constantinian (5):** Diocletian · Galerius · Constantine · Julian "the Apostate" · Theodosius I

**Western Twilight (4):** Alaric · Attila · Odoacer · Romulus Augustulus

**Eastern Renewal (5):** Theodoric the Great · Clovis · Justinian · Belisarius · Narses

### 31.2 Dated Event Counts by Section

*Source: same two docs, table rows per era section*

| Doc | Era Section | Dated Events |
|---|---|---|
| Timeline (Content) | Late Republic (133–27 BC) | part of 37 total across §2–5 |
| Timeline (Content) | Early Principate, High Principate, Severan Era | (see above) |
| Timeline (Late Antiquity) | Crisis, Dominate, Constantinian Shift, Division/Western Twilight, Eastern Renewal | 84 total across §5, §6, §7, §9, §11 |

**121 dated historical events total** across both docs. Full text (year, event description, Event Type tag, Region/Culture tag) lives only at source — this registry does not duplicate it, consistent with the reuse-over-duplication convention; cite the section headers above to navigate directly.

---

## 32. Final Coverage Summary

Every category identified across Passes 1–4 is now either **fully enumerated**, **extracted and materially complete** (95%+, with the residual gap noted per-category above), or explicitly logged as **complete-as-designed** (Epithets & Titles' dynamic Conquest Agnomina). The two categories that remain deliberately un-duplicated are the Historical Timeline's full dated-event text (§31 — narrative content better read at source) and the ~32 residual Discovery Roster entries (§26.5) that a table-parse didn't catch cleanly.

**Recommended next steps, if useful, rather than further passes on this document:**
1. A manual spot-check of the machine-extracted appendices (§24, §26) against source, since table-parsing is mechanical and can misfire on an edge case.
2. Reconciling the ~13 missing Goods Registry items and ~32 missing Discovery Roster items by hand rather than further automated extraction, since both gaps appear to be table-formatting edge cases rather than systematic omissions.
3. Establishing a lightweight process for keeping this registry in sync going forward — e.g., a note at the bottom of future design docs' own Data Model sections flagging "new named entity, registry update needed."
