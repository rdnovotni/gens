# GENS — System Design: The Paper Doll — Appearance, Description & Portrait Synthesis
*The culmination of the entire appearance stack: Familia's fixed genetics, Fashion & Dress's Wardrobe, the Garment Roster's Layer Stack, and Hair, Facial Hair & Body Marking's own permanent record all resolve here, into two outputs a player actually sees — a rendered **Portrait** and a procedurally generated **Description**, available at three levels of detail from a one-line Summary up to a genuinely comprehensive Full Description. This document invents no new appearance content of its own; it is entirely an assembly and presentation layer over four documents that already exist. This pass adds real descriptive depth within the register §2 establishes — tasteful figure descriptors, cultural-origin phrasing, personality-inflected description, combined hair clauses — and closes it out with the project's own deepest cuts: Family Resemblance, Observer-Colored Description, a Portrait History timeline, and the Family Portrait payoff Villa's own document has been waiting on. A further pass added occupation-linked Hands, Boldness/Dignitas-driven Posture, Bearing & Presence, Voice quality, temporary Pregnancy and Illness states, and a harsher permanent Maiming tier. This final pass adds an explicit, permanent boundary on how Child-lifecycle Characters are described, a full one-page pipeline reference closing out the whole four-document family, two small finishing touches (a signature carried item, a handedness note), and concrete resolutions to several previously open questions.*

---

## Contents

1. Scope & Role — Assembly, Not Invention
   - 1.1 The Full Pipeline, at a Glance
2. A Note on Register — Where This Document Draws the Line
3. The Three Output Modes
   - 3.1 Context-Specific Summaries
4. The Full Description — What It Actually Contains
   - 4.1 Figure Descriptors — Tasteful, Not Clinical
   - 4.2 Cultural Origin, Accent & Voice
   - 4.3 Notable Features — A Quick-Scan Header
   - 4.4 Hands
   - 4.5 Posture, Bearing & Presence
   - 4.6 Temporary States — Pregnancy & Illness
   - 4.7 Maiming — A Harsher Permanent Tier
   - 4.8 Small Personal Touches — Carried Items & Handedness
5. Procedural Text Generation — How a Description Gets Written
   - 5.1 Family Resemblance — The Genetic Echo
   - 5.2 Observer-Colored Description — The Same Facts, A Different Voice
   - 5.3 Worked Examples
6. The Portrait — Rendering the Layer Stack
   - 6.1 The Family Portrait — A Household Assembled
7. Portrait History — The Body's Own Timeline
8. Detail Level as a Player Preference
9. Special Cases — Concealment, Disguise & Incomplete Information
10. Appearance as a Source — Epithets and Beyond
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role — Assembly, Not Invention

Four documents already built everything a Character's appearance can consist of: Familia §2.4/Core §7.11 (fixed genetics — build, complexion, natural hair/eye color), Fashion & Dress (the Wardrobe and its Occasion-driven selection), the Garment Roster (the actual named items and the Layer Stack that stacks them into a coherent image), and Hair, Facial Hair & Body Marking (hairstyle, facial hair, tattoos, scars, and every other durable-or-permanent body detail). This document does not add a fifth category of appearance content. Its entire job is to take whatever combination of the above a given Character currently has, and produce two things from it: a **Portrait** (an image, per §6) and a **Description** (text, per §3–§5) — the same underlying data, presented two different ways, exactly the way the direction's own Free Cities comparison implies. Where this document's final pass goes further is in what it does *with* that same underlying data — cross-referencing it against a Character's own family tree (§5.1) and against who's actually doing the looking (§5.2) — genuine new depth built entirely out of connections between documents this project already has, rather than a fifth category of raw content.

### 1.1 The Full Pipeline, at a Glance

A single reference table, closing out this whole appearance-system family in one place, for whoever eventually implements it:

| Layer | Owning Document | What It Contributes |
|---|---|---|
| Fixed genetics | Familia §2.4 / Core §7.11 | Build, complexion base, natural hair/eye color — never changes |
| Clothing, jewelry, cosmetics | Fashion & Dress | The Wardrobe, Outfit Tier, Occasion-driven selection |
| The actual named items | Garment Roster | Every specific garment/accessory, and the Layer Stack that composites them |
| Hair, facial hair, permanent marks | Hair, Facial Hair & Body Marking | Hairstyle, tattoos, scars, piercings, complexion's own sun-exposure modifier |
| Assembly & synthesis | **This document** | The Portrait render, the three Description modes, Family Resemblance, Observer-Coloring, Portrait History |

Nothing above this table needs to change for this document to keep working — a future addition to any one of the first four layers simply becomes one more field this document's own templates can already draw on.

---

## 2. A Note on Register — Where This Document Draws the Line

Worth stating plainly before the mechanics below, since this document sits closer to explicit physical description than anything else in the project: this document keeps the same register Romance, Sexuality & Lineage already established — mature content handled with real narrative restraint, described rather than clinically measured. A Full Description (§4) is genuinely detailed and can absolutely register that a Character is striking, plain, imposing, or worn down by a hard life — but it does so the way a well-written historical novel or a Dynasty Chronicle entry would, through build, bearing, and specific detail, not through explicit sexual measurement statistics. This isn't a new restriction invented for this document alone; it's the same line Fashion & Dress's own Garment Roster already drew in its §16.4 ("this system exists to communicate status, wealth, culture, and office — never a body-exposure or arousal state") and that Romance & Seduction already holds for conduct. It matters more here than almost anywhere else in the project specifically because so much of this game's own cast can be enslaved and under another Character's direct authority — a system that mechanically catalogued sexualized body detail on every Character, including the enslaved, would sit very differently than the same content applied to consenting adults in an unrelated context, and this document declines to build it. What the Full Description *does* deliver in its place — real specificity on build, Beauty tier, bearing, marks, dress, lineage, and even the tone of whoever's describing them — is detailed enough to satisfy the "wall of text" request in full without crossing that line.

**A second, equally firm boundary, made explicit this pass rather than left implicit: none of §4's adult-oriented registers apply to a Child-lifecycle-stage Character.** Beauty tier's own Romantic-interaction framing (Traits §3.2), Figure Descriptors (§4.1), and Presence (§4.5) are all, by their own underlying mechanical definition, Adult-and-older concepts — Traits' own tiered spectrums are read and applied the same way for every age in the strict data sense, but this document's *generated prose* deliberately declines to render a Child in those terms at all. A Child's Full Description stays plain and age-appropriate: build, coloring, Family Resemblance (§5.1, often especially charming and worth including at this age precisely because it's harmless), visible Formative traits as they emerge, and nothing drawn from the Romantic-interaction or figure-and-presence registers §4.1 and §4.5 establish for adults. This isn't a gap this document expects a future pass to fill in — it's a deliberate, permanent design boundary, exactly as firm as the one the paragraph above already draws.

---

## 3. The Three Output Modes

Consistent with this project's own automation principle — sensible defaults, full detail available on request rather than forced on everyone — a Character's appearance resolves to text at three selectable levels, and to one Portrait regardless of which text level is showing.

| Mode | Length | Where It's Used |
|---|---|---|
| **Summary** | A single line | Character lists, Familia trees, a Salutatio queue, any dense list view where a full description would overwhelm the screen |
| **Standard Description** | A short paragraph | The default view on an individual Character's own sheet — enough to place them at a glance without requiring a click-through |
| **Full Description** | Several paragraphs — the genuine "wall of text" the direction calls for | Opt-in, one click away from Standard — the single most detailed textual output this project produces about any Character |

All three modes are generated from the same underlying data (§5) at different levels of compression — there is no separate "short version" content to author; the Summary is a real, algorithmic compression of the Full Description's own fields, not a hand-written alternative.

### 3.1 Context-Specific Summaries

New this pass, and a direct resolution of a real design question the prior pass left open: a single generic Summary doesn't actually serve every list view equally well, so Summary compression draws on a small, different priority order depending on where it's being shown, rather than one fixed template everywhere.

| Context | Summary Prioritizes |
|---|---|
| A Familia tree | Family Resemblance (§5.1) where a strong one exists, otherwise Beauty/Physique tier |
| A Salutatio or Curia queue | Held office's own dress marker, Outfit Tier, Dignitas-relevant Dona Militaria |
| A marriage-market list (Familia §5) | Beauty tier, Figure Descriptor (§4.1), Trousseau-relevant Outfit Tier |
| A military roster | Rank, Dona Militaria, Scarred/Battle-Hardened status |
| Any general/unspecified list | Beauty tier and Outfit Tier — the honest, always-available fallback |

This is still a single-sentence compression in every case — §3's own length constraint doesn't move — only which fact wins the single available slot changes by context.

---

## 4. The Full Description — What It Actually Contains

The complete field list a Full Description draws from, organized by which document actually owns the underlying data:

**From Fixed Appearance (Familia §2.4 / Core §7.11):**
- Build/Physique tier (Traits §3.3 — Frail, Average, Strong, Herculean)
- **Figure descriptor (§4.1)** — a tasteful, non-numeric body-type impression layered on top of Physique tier
- Beauty tier (Traits §3.2 — Hideous, Plain, Comely, Beautiful)
- Complexion, natural hair color, natural eye color
- **Distinctive complexion detail** — freckling, a birthmark, or similar natural, specific texture, where the underlying Appearance roll includes one
- Age band and its visible effects (an Elder's own age-appropriate wear, distinct from any deliberate marking)

**From Culture & Language:**
- **Cultural origin, accent, and voice (§4.2)** — native culture tag (Cultures of the Known World), accent where relevant, and a personal voice-quality note

**From Occupation and Bearing (new this pass, §4.4–§4.5):**
- **Hands** — a visible, occupation-linked detail distinct from Complexion's own sun-exposure marker
- **Posture, gait, and presence** — how a Character carries themselves, reading Boldness, Dignitas, and, where one exists, a lasting old-injury effect

**From the Wardrobe (Fashion & Dress / Garment Roster):**
- Current Outfit Tier and its visible Wear Condition (Garment Roster §16.2)
- Every currently-worn Garment Slot item, by name — a Toga Praetexta, a Corona Civica, a Livery color
- Cultural dress signal, where a non-default culture choice is active (Fashion & Dress §10)

**From Hair, Facial Hair & Body Marking:**
- Current hairstyle, length, and any Era Drift flavor it carries, described together with hair color in a single natural clause rather than as separate inventory lines (§5's own generation technique)
- Eyebrow description, drawing on that document's own §6.1 fullness/style fields
- Facial hair style, where applicable
- Complexion's own sun-exposure modifier (that document's §4.1) — the visible mark of an outdoor working life or an indoor sheltered one, layered on top of the fixed base complexion above
- Every visible Skin Marking currently uncovered by clothing — a tattoo, a scar, a brand, or a commemorative/devotional mark (that document's §7.1) — per the same coverage logic §16 of this document (§9, below) reuses directly
- **A permanent Maiming (§4.7, new this pass)**, where one is on record, described plainly and without dwelling
- Cosmetic overlay detail (kohl, rouge, cerussa) where actually applied, or a plain note that the Character wears no cosmetics at all where that's the more notable fact
- A small, incidental personal-grooming detail where one is on record — well-kept nails, tidy or unkempt overall presentation — kept to the same light, non-anatomical register as the rest of this list

**Temporary states (§4.6, new this pass):**
- Visible pregnancy, where applicable
- The visible signs of a current illness or recent recovery, where applicable

**From Familia's own lineage record:**
- **Family Resemblance (§5.1)** — a real, explicit tie to a parent's, sibling's, or recorded ancestor's own appearance, where a genuine match exists

**A closing personal-history line, where one exists:** a Full Description's final line draws on a Character's own most Chronicle-relevant physical fact where one is available — a Dona Militaria decoration actually worn, a Pileus received at manumission, a Scarred/Battle-Hardened trait's own flavor line (Traits §6.2, §6.7) — giving the description a genuine narrative close rather than trailing off after the last inventory item.

### 4.1 Figure Descriptors — Tasteful, Not Clinical

A Character's overall figure or build impression, described the way a historical novel would — Willowy, Slender, Athletic, Statuesque, Curvaceous, Full-Figured, Broad-Shouldered, Stout, Wiry, and similar plain-language terms — used freely and for any Character regardless of gender, as a genuine complement to Physique tier's own more mechanical Frail/Average/Strong/Herculean scale. This is a real, worthwhile addition to the Full Description's own richness, and it stays exactly at this register deliberately: an overall figure impression, not a numeric measurement and not a description built around sexual availability or capability. The line drawn in §2 hasn't moved — what's moved is that this document now has real, concrete descriptive vocabulary for figure and build sitting alongside everything else on the list above, which is what actually delivers on "detailed" without crossing into what §2 already declined.

**Resolved this pass:** a Figure Descriptor is rolled once, alongside Beauty tier, when a Character reaches Adulthood, and stays fixed absent a genuine triggering life event — a difficult childbirth, a prolonged illness, or a serious injury are the honest, real-world reasons a figure visibly changes, and any of the three can trigger a re-roll; ordinary aging alone does not, since Age Band already carries its own separate visible effects (§4).

### 4.2 Cultural Origin, Accent & Voice

The Full Description states a Character's native culture (Cultures of the Known World) plainly where it's a meaningful part of how they'd actually be perceived, and, where relevant, notes the accent or speech pattern that origin carries — a Gallic-culture Character serving in an Italian household described as speaking Latin with a real, noticeable Gallic accent, precisely the same way this project's own Assimilated/Unbowed trait pair (Traits §6.6) already treats a retained cultural marker as worth noting rather than smoothing over.

**Resolved this pass:** whether an accent note actually appears is tied directly to that same trait pair rather than being permanent once established — a Character who has crossed fully into the Assimilated trait no longer has their accent mentioned in generated text at all, the honest, natural implication of what that trait already means; an Unbowed Character carries the note for as long as the trait itself is held.

**Voice quality, new this pass, and genuinely distinct from accent.** Where accent is about *where* someone is from, voice is about *how they actually sound* regardless of origin — and this document already has a real, existing Trait pair built for exactly this: Sharp-Tongued/Soft-Spoken (Traits §4.3). A Sharp-Tongued Character's voice reads as quick, cutting, or precise; a Soft-Spoken one's as gentle, low, or measured — reusing that Trait's own flavor register rather than inventing a separate one, the same borrowing principle §5 already establishes for Herculean build and Beauty tier.

### 4.3 Notable Features — A Quick-Scan Header

Sitting above the Full Description's own prose, a short, non-prose line of two to four tags surfacing the single most objectively distinctive facts on a Character's record — a Dona Militaria decoration, a striking Beauty or Physique tier, a distinguishing scar or tattoo, a held office's own dress marker. This isn't a duplicate of the Summary (§3, one compressed sentence woven into running text) — it's a fast, scannable header for a player working through a large cast who wants to know at a glance whether a given Character is worth reading the full prose for, before committing to it.

### 4.4 Hands

New this pass, and a direct structural parallel to Hair, Facial Hair & Body Marking §4.1's own Complexion/Sun-Exposure modifier, applied to a different, equally honest part of the body: a Character's hands read off the same underlying Duty Slot or Court Position data, entirely independent of Physique tier or Outfit Tier. A field laborer's, a smith's, or a soldier's hands are visibly roughened and calloused regardless of how well they're otherwise dressed; a scholar's, a musician's, or an idle elite's stay smooth and uncalloused regardless of how humbly they might otherwise present. This is, like §4.1 of the parent document, a free, legible piece of texture requiring no new assignment mechanic — simply which real work a Character's hands actually do, made visible.

### 4.5 Posture, Bearing & Presence

New this pass: how a Character actually carries themselves, distinct from their fixed Figure Descriptor (§4.1) or Physique tier. Posture and gait read primarily off the Boldness Personality Axis (Characters §5) — a high-Boldness Character stands and moves with real, visible confidence; a low-Boldness one more guardedly or diffidently — and, where one is on record, a lasting old-injury effect (a real limp from a poorly-healed wound, distinct from and less severe than the Maiming tier below) colors it further, a permanent physical consequence of a past Military & Combat wound or a serious accident that a mere Scarred trait's own skin-deep mark doesn't fully capture on its own.

**Presence, distinct from Beauty.** A related, equally real addition: a Character's own Dignitas can generate a genuine "commands a room" quality in their Description entirely apart from their Beauty tier — a Plain-tier Character with substantial Dignitas is described as carrying real, visible authority despite not being conventionally striking, exactly the same "Beauty and standing are two different things" distinction this project's own Fashion & Dress §14 already draws between innate Beauty and earned presentation. This gives a description one more real, non-redundant axis: a Character can be beautiful, imposing, or both, or neither, and the description says so honestly in each case.

### 4.6 Temporary States — Pregnancy & Illness

New this pass, and a real, honest gap given how central childbirth and succession already are to this entire project: a visibly pregnant Character's Full Description notes it plainly and matter-of-factly, reading directly off Familia's own existing gestation record — a genuinely significant, temporary, and often emotionally loaded appearance state that this document had no field for until now, despite how much weight the rest of the project already places on heirs, Ambitions like "Raise a Worthy Heir" (Character Ambitions §3.3), and succession planning generally.

**Active illness**, similarly, is a real, temporary, and honestly-described state distinct from any of this document's own permanent markings — a Character currently suffering a Disease & Public Health condition, or newly recovered from one, is described as visibly drawn, pale, or thin for the duration, rather than the Full Description simply staying silent on a fact that would plainly be obvious to anyone actually looking at them. A Plague Survivor (Traits §6.9) carries a related but distinct and genuinely permanent version of this same texture — a lingering gauntness that never fully leaves, worth a small, real mention in that Character's own baseline description rather than only at the point of active illness.

**Resolved this pass — pregnancy granularity.** The Full Description reads a real, approximate stage off Familia's own gestation timeline rather than a flat visible/not-visible toggle — an early pregnancy goes unmentioned (consistent with how it would genuinely be unnoticeable), a mid-term one is noted plainly, and a near-term one carries real, specific weight in both the Description and, per §7, as a strong natural Portrait History snapshot trigger in its own right, distinct from and well ahead of the birth itself.

### 4.7 Maiming — A Harsher Permanent Tier

New this pass, and a real, honest extension of Hair, Facial Hair & Body Marking's own Scars entry (that document's §13) for the cases where a real injury goes further than a mark on the skin. Consistent with this project's own historical-frankness pillar, and with exactly the same restraint already applied to Punishment and to Scars generally, a sufficiently severe Military & Combat wound (that document's own worse Combat Resolution outcomes), a serious Games & Spectacle arena injury, or a genuine workplace accident (Labor & Slavery, Buildings' more hazardous production chains) can leave a Character with a real, permanent, and honestly-described physical loss — a missing finger or hand, a blinded eye, a permanent limp beyond §4.5's own milder version. This is described the same way this entire document family already handles hard material: factually, briefly, and without dwelling, exactly one sentence's worth of honest information rather than an extended focus. A Maiming is, like every other permanent mark in this document's own family, real and lasting Chronicle-eligible material in its own right — a veteran's missing hand is exactly the kind of detail a household's own Dynasty Chronicle entry for them would actually include.

### 4.8 Small Personal Touches — Carried Items & Handedness

Two final, deliberately minor fields rounding out the Full Description's own honest completeness, each costing this document nothing beyond a single sentence:

- **A signature carried item**, where one exists — a stylus and wax tablet habitually at hand for a literate Character, a walking stick for an Elder, a favored weapon worn at the hip for a soldier or a Praefectus even off duty. Not a Garment Slot and not tracked as inventory; simply a small, humanizing habitual detail worth a passing mention where the Character's own station or Traits make one obvious.
- **Handedness**, per Traits §4.2's own Left-Handed/Right-Handed/Ambidextrous pair — genuinely minor, but real: a Left-Handed Character's own real Roman-era *sinister*-hand association (already noted in that document as a small Duel-surprise bonus) is exactly the kind of specific, textured detail a Full Description can note in passing without it needing to carry any further weight than that.

---

## 5. Procedural Text Generation — How a Description Gets Written

The Full Description is assembled from a **template library**, not a bare field dump — the direction's own "wall of text" should read as prose a person would actually write, not a stat sheet with commas between entries. A small number of interchangeable sentence templates exist per field category (build, dress, marking, closing line), each written to combine grammatically regardless of which specific value fills it, and the generator selects and orders them to produce a coherent paragraph structure: an opening sentence establishing overall build and bearing, a middle section covering dress and notable marks, and the closing personal-history line from §4.

**Trait-flavored language, reused rather than duplicated.** Where a relevant Trait already has its own flavor line (Traits' own italicized descriptions throughout that document), the generator can draw on that exact phrasing rather than writing new text — a Herculean Character's description can genuinely borrow "the sort of build that ends arguments before they start" rather than the generator inventing a separate description of the same fact. This keeps the Full Description consistent with how the rest of the project already talks about a given Character, rather than introducing a second, competing voice.

**Personality-inflected physical description.** Rather than listing a physical feature and a personality Trait as two unconnected facts, the generator can pair them in one clause where a natural connection exists — eye color described alongside an Intellect-tier or Perceptive/Oblivious-driven adjective ("her ice-blue gaze is quick and appraising" rather than "her eyes are blue" followed separately by "she is Clever"). This is purely a phrasing technique operating over data this document already has.

**Combined hair clauses.** Similarly, hair color, length (Hair, Facial Hair & Body Marking §4.2), and current hairstyle (§2 of that document) read as one natural descriptive clause — "her long, red hair is worked into braids" — rather than three separate inventory lines, the same combining principle §4.1's figure descriptors and §4.2's cultural-origin note both use.

**A richer natural color vocabulary.** A small, purely stylistic addition: eye and hair color descriptions can draw on ordinary, real-world evocative comparisons — amber, hazel, copper, jet-black, sapphire-blue — rather than flat, clinical color-swatch naming, giving the same underlying fixed Appearance value (Familia §2.4) a genuinely more vivid presentation. This adds no new field and no new mechanic; it's purely a vocabulary choice for the template library to draw on when rendering a color that's already on record.

**The Standard Description and Summary** are generated by applying real compression rules to the same underlying data — Standard keeps the opening build/bearing sentence and the single most status-relevant dress or marking detail; Summary keeps only the single fact its own context (§3.1) prioritizes.

### 5.1 Family Resemblance — The Genetic Echo

New this pass, and the single most direct payoff available anywhere in this document for Design Pillar #7. Familia's own inheritance mechanics already weight a child's Congenital traits, Beauty tier, Physique tier, and natural coloring toward their parents' own recorded values (Characters §4.4) — real data this project has always generated but never actually *said out loud* in a Character's own description. This document closes that gap directly: whenever a Character's own combination of hair color, eye color, Beauty tier, and Physique tier shows a genuine, strong match against a parent, sibling, or a specific recorded ancestor, the Full (and, per §3.1, sometimes Standard or Summary) Description includes a real, explicit resemblance line — "shares her mother's copper hair and quick green eyes," or, for a resemblance running back further than living memory, something with real weight behind it: "has his grandfather's exact build, down to the set of his shoulders, though he never knew the man himself."

**This is genuinely stronger where the ancestor in question is otherwise only preserved in the Dynasty Chronicle or as a household Imago (Garment Roster §9)** — a resemblance line connecting a living Character's own face to a wax mask hanging in the family Atrium is exactly the kind of quiet, physical through-line this project's own core design pillar was written for, and this document is where that connection actually gets said in plain language rather than left for the player to notice on their own. **Twins and close-in-age siblings** are the single most common and most striking real case, and the generator weights resemblance-checking between them accordingly — a resemblance line between twins carries no special extra mechanic, simply a higher real likelihood of triggering given how Familia's own inheritance rolls already work.

### 5.2 Observer-Colored Description — The Same Facts, A Different Voice

New this pass, and a genuine, real depth addition rather than a cosmetic one: the *facts* underlying a Character's Description never change based on who's reading it — this document has never and will never generate different underlying data for different viewers — but the **framing language** wrapping those same facts can shift, drawing directly on the real, existing Opinion and relationship-web data (Characters §7) between the Character being described and whichever Character is doing the looking. A weathered, work-worn face reads as "a face that's earned every one of its lines" in a Devoted spouse's own generated description, and as "hasn't aged well" in a Estranged rival's — the same underlying Age Band and complexion facts (§4), two entirely different, equally legitimate real voices.

This is deliberately a **framing-only** effect: it changes adjectives and emphasis, never the actual inventory of what's true (a tattoo, a scar, an Outfit Tier, a held office are stated identically regardless of who's asking) — the honest facts stay honest, only the color around them shifts, the same distinction a real biased narrator draws in any well-written historical account. An Infatuated Character's own real, temporary Trait state (Traits §6.4) is this mechanic's single richest natural trigger — a genuinely Plain-tier love interest described in warmer, more flattering language than an objective stranger would ever use, without a single fact in the description actually being false.

**Resolved this pass — the player's own Character is the deliberate exception.** Observer-Coloring applies in full whenever another Character is the one doing the describing — a rival's own letter (Correspondence & Letters), a Companion's private report, or a suitor's own account of a courtship all pass through this framing layer exactly as described above. The player's own default view of their own controlled Character, by contrast, always renders as the neutral, objective baseline — the one deliberate place this document favors clarity of information over narrative color, since a player needs to know their own Character's true standing without a rival's bias or an admirer's flattery clouding the read. A letter written *to* the player *about* their own Character, however, is a different matter entirely, and renders fully colored by whoever actually wrote it.

### 5.3 Worked Examples

*(Illustrative only, demonstrating the register §2 and §4 establish — richly detailed, entirely free of numeric measurement or sexual-capability content.)*

**A standard Full Description, showing §4's full field range together:**

> **Brigid**, a Gallic-culture Companion serving an Italian household as a household physician's assistant.
>
> Brigid is a Devoted, Clever young woman of nineteen. Her pale, lightly freckled complexion and her long, copper-red hair — worn today in a simple practical braid rather than anything more formal — mark her out at a glance as someone not born to this household, and her Latin, though fluent, still carries a real, unmistakable Gallic lilt she's never quite shaken and, if she's honest, never much tried to. Her eyes are a clear grey-green, quick and watchful in the particular way of someone used to reading a room before she's asked to speak in it. She has a slight, wiry figure, more suited to the quick work of a physician's assistant than to anything decorative, and carries herself with the unshowy competence of someone who has learned not to waste a movement. She wears a plain, well-kept Modest-tier tunic and no jewelry beyond a small iron ring; her face is bare of any cosmetic. A thin, old scar along her left forearm — earned, she says, learning to set a bone the hard way — is the one mark on her anyone's ever asked about twice.

**The same woman, described twice — §5.1 and §5.2 together:**

> *As her own mother would describe her:* "Brigid has my mother's own copper hair, may she rest easy — I'd know that color anywhere, in any market in the province. She's grown into a fine, capable young woman, and that little scar on her arm only proves she was never afraid of hard work."
>
> *As a rival Companion, resentful of her standing in the household, might instead:* "The girl's hair is loud enough to spot across a crowded room, if nothing else. That scar on her arm is the sort of thing you'd expect from someone careless enough to still be learning basic tasks at her age."

Both passages describe the identical Character — the same hair, the same scar, the same underlying facts — filtered through two genuinely different, equally legitimate relationships. Neither is "the real" description; both are.

---

## 6. The Portrait — Rendering the Layer Stack

The Portrait is the direct, literal output of the Garment Roster's own Layer Stack (§16 of that document) and Hair, Facial Hair & Body Marking's own Rendering Integration (§16 of that document) — every layer from Body up through Status Overlay, composited in order, using whatever asset each layer's current value resolves to. This document adds no new rendering logic beyond confirming that the Portrait and the Description are generated from **the same single source of truth** — a Character's own current Wardrobe, BodyMarking, and FacialHairProfile records — so the two outputs can never contradict each other: a Description that mentions a Toga Praetexta is describing the exact same garment the Portrait is currently rendering in Layer 5, not a separately-tracked fact that could drift out of sync.

**Regeneration triggers.** The Portrait re-renders automatically whenever any input layer changes — an Occasion switch (Fashion & Dress §4), a new Garment Slot unlock, a fresh tattoo or scar, a hairstyle change. This is the same automation principle already established for Occasion-based dressing generally: the player never manually "updates" a portrait, it simply always reflects current truth.

**Resolved this pass — the pre-contact fallback:** consistent with Characters' own lazy instantiation, a Character the player hasn't yet actually met renders, if shown at all in some indirect context (mentioned in a letter, referenced by a Rival House), with a generic, culture-and-station-appropriate silhouette rather than a fully detailed Portrait — enough to communicate "a Roman citizen woman" or "a Parthian nobleman" at a glance without implying detail the player hasn't actually earned by meeting them yet. A full Portrait generates the moment first meaningful contact actually occurs.

### 6.1 The Family Portrait — A Household Assembled

New this pass, and the direct, concrete realization of a genuine forward reference the Villa document has carried since it was first written: that document's own §7 Family Portrait wall decoration option was explicitly built to draw "on the same Appearance system that generates individual character portraits," without specifying how. This document is where that actually happens. A **Family Portrait** composes the current individual Portraits of a chosen set of household members — typically the current head, their spouse, and their children, though the player can adjust the roster — into a single, unified group image, generated on request rather than continuously live-updated.

**Deliberately a snapshot, not a live view.** A Family Portrait captures its subjects' appearance at the specific moment it's commissioned, and does not automatically update afterward even as those same individuals' own current Portraits keep changing — the same honest logic a real painted family portrait would follow, and the direct bridge into §7's own Portrait History below: a household's Villa can, over a full campaign, accumulate several Family Portraits from different points in its own history, each one a genuine, dateable artifact in its own right rather than a single mutable decoration.

---

## 7. Portrait History — The Body's Own Timeline

New this pass, and this document's own most direct, literal expression of Design Pillar #7: a Character's Portrait and Description both change over a real lifetime — aging, a new scar, a completed Ambition's commemorative tattoo, a fresh Dona Militaria, the Pileus received at manumission — and this document formalizes keeping a real, browsable record of that change rather than letting every earlier version simply be overwritten and lost the moment the current one regenerates.

**What actually gets saved, and why not everything.** Saving a full snapshot on every single Wardrobe or Occasion change would be both wasteful and, more importantly, uninteresting — most day-to-day changes aren't worth remembering. Instead, a Portrait History snapshot is captured specifically at **Dynasty Chronicle-eligible moments** (reusing that document's own significance tiering directly rather than inventing a separate one) — a Toga Virilis, a manumission, a completed Primary Ambition, a Dona Militaria award, a marriage, a serious injury. This keeps the Timeline meaningful and production-realistic at once: a handful of real, significant snapshots across a full lifetime, not an unmanageable flood of near-duplicate images.

**What a player actually gets from this:** the ability to open any Character's own sheet and page backward through their own life — the eager young recruit before their first campaign scar, the newly-freed Character still wearing their first Pileus, the young bride in her Flammeum decades before she became the household's own formidable materfamilias. Combined with §6.1's own Family Portraits, a household's full visual history becomes a real, accumulated artifact any long campaign naturally builds toward, without requiring the player to have deliberately saved anything themselves.

---

## 8. Detail Level as a Player Preference

A simple, standing player preference — not a per-Character setting — determines which text mode (§3) displays by default when opening any Character's sheet, with the other two always one click away regardless of the default. This mirrors the project's own broader "player sets standing decisions, the simulation handles the rest" philosophy rather than asking the player to choose a detail level separately for every single Character they ever look at.

---

## 9. Special Cases — Concealment, Disguise & Incomplete Information

Two direct, necessary ties back into Hair, Facial Hair & Body Marking's own mechanics, since a Description and a Portrait are both, in-fiction, things another Character is *looking at* — and what's actually visible isn't always the complete truth.

- **Concealment (Hair, Facial Hair & Body Marking §14).** A concealed Stigmata mark or brand doesn't appear in either the Portrait or the Description under ordinary viewing conditions. **Resolved this pass:** whether a viewer sees past the concealment isn't a dedicated menu action — it resolves naturally off the same relationship-depth data §5.2 already reads. A spouse, a Devoted family member, or a sufficiently deep Confidant-tier bond simply sees the true, unconcealed record automatically, the honest implication of genuine intimacy; anyone else needs a real, narratively-grounded moment of actual close exposure (a shared bathing Occasion, a medical examination, a search) rather than a button a player can press on demand — concealment stays meaningful specifically because it can't be casually defeated.
- **Disguise (Fashion & Dress §12).** A Character actively engaged in a Disguise attempt generates a Portrait and Description reflecting the *disguised* appearance — a different culture's default dress, a concealed mark — rather than their own true underlying record, for exactly as long as the disguise holds. A discovered disguise reverts both outputs to the true record immediately, itself a small, legible "the mask came off" moment.
- **Unknown/unmet Characters.** Consistent with Characters' own lazy-instantiation principle, a Character the player hasn't yet actually encountered simply has no rendered Portrait or generated Description yet beyond §6's own generic fallback — both are produced in full at first meaningful contact, not speculatively in advance.

---

## 10. Appearance as a Source — Epithets and Beyond

A short, closing loop worth stating directly: a sufficiently distinctive, recurring detail surfaced repeatedly in a Character's own Full Description — a notable scar, a striking hair color, a permanently worn decoration — is exactly the kind of raw material Epithets, Nicknames & Titles already draws its own earned epithets from. This document doesn't add a new mechanic here; it simply confirms that this document's own generated output is a legitimate, direct source that system can already read from, closing a small but real gap between two documents that clearly always belonged next to each other.

---

## 11. Cross-System Integration

- **Familia, Core (§7.11):** this document is the actual, concrete delivery mechanism for that document's own long-standing "status-appropriate dress/grooming that updates automatically" promise; §5.1's Family Resemblance is the first time that document's own inheritance math is actually spoken aloud in generated text; §4.6's pregnancy note reads directly off that document's own gestation record.
- **Fashion & Dress, Garment Roster:** the Wardrobe, Occasion system, and full Layer Stack are reused wholesale as this document's own primary data source and rendering engine respectively; §4.5's Presence note is a direct extension of that document's own Beauty-versus-earned-presentation distinction (Fashion & Dress §14).
- **Hair, Facial Hair & Body Marking:** every field in §4's marking/hairstyle list, and the Concealment mechanic specifically (§9 of this document), are drawn directly from that document without alteration; §4.7's Maiming is a direct, harsher extension of that document's own Scars entry (§13).
- **Traits:** Beauty and Physique tier flavor lines, and several Reactive trait flavor lines (Scarred, Battle-Hardened), are directly reused as generation source text (§5); the Infatuated trait (§6.4) is §5.2's own richest natural trigger; Sharp-Tongued/Soft-Spoken (§4.3 of that doc) is §4.2's own new Voice field; Plague Survivor (§6.9) is §4.6's own permanent-gauntness case.
- **Romance, Sexuality & Lineage:** §2's register decision is a direct, deliberate extension of that document's own "described rather than depicted" standard into the one place in the project most tempted to abandon it.
- **Cultures of the Known World, Language & Literacy:** §4.2's cultural-origin-and-accent field gives every Character's existing culture tag a genuine, spoken presence in their own generated description.
- **Characters:** lazy instantiation governs exactly when a Portrait/Description first gets generated for any given Character (§6, §9); the Opinion/relationship-web data (§7 of that doc) is §5.2's own entire mechanism; the Boldness Axis (§5 of that doc) drives §4.5's posture read directly.
- **Villa:** §6.1 is the direct, concrete realization of that document's own long-standing Family Portrait forward reference.
- **Military & Combat, Labor & Slavery, Games & Spectacle, Buildings:** a severe wound, a workplace accident, or a serious arena injury are all real, legitimate triggers for §4.7's Maiming tier; a laborious Duty Slot or Court Position is §4.4's own entire Hands mechanism.
- **Disease & Public Health:** an active illness or recent recovery is §4.6's own second temporary state.
- **Dynasty Chronicle (§6.11, future):** §7's Portrait History reuses that document's own significance tiering directly as its snapshot trigger, and is itself one of the single richest possible expressions of Design Pillar #7 in the entire project; a Full Description's own closing personal-history line (§4) is a light-touch way to surface a Character's most Chronicle-relevant physical fact; a Maiming (§4.7) is itself real Chronicle-eligible material.
- **Epithets, Nicknames & Titles:** §10 confirms this document's own output as a legitimate, direct source for that system's earned-epithet generation.
- **Character Ambitions:** a completed Primary Ambition is both a Full Description closing-line candidate (§4) and a Portrait History snapshot trigger (§7); "Raise a Worthy Heir" (§3.3 of that doc) is §4.6's own direct narrative motivation for tracking pregnancy at all.

---

## 12. Data Model

```
CharacterAppearanceOutput {          // computed, not stored — regenerated on demand from source records
  characterId,
  portraitAssetRef,                   // resolves via Garment Roster §16's Layer Stack
  descriptionSummary,                  // one line, context-weighted per §3.1
  descriptionStandard,                 // one paragraph
  descriptionFull,                     // multi-paragraph
  notableFeatures: [...],               // §4.3 — 2-4 quick-scan tags
  signatureCarriedItem,                  // §4.8, nullable
  handednessNote,                        // §4.8, drawn straight from Traits' own Left/Right/Ambidextrous record
  handsDescriptor,                      // §4.4 — derived from current Duty Slot/Court Position, no separate record needed
  postureDescriptor,                    // §4.5 — derived from Boldness Axis + any lasting old-injury flag
  presenceNote,                         // §4.5 — derived from current Dignitas score
  pregnancyStateRef,                     // §4.6 — nullable pointer to Familia's own gestation record
  illnessStateRef,                       // §4.6 — nullable pointer to Disease & Public Health's active condition record
  lastRegeneratedAtMonth,
  sourceRecordVersion,                  // a simple hash/version of the Wardrobe+BodyMarking+FacialHair
                                       // records used, so a stale cached output can be detected and rebuilt
}

DescriptionTemplateEntry {            // §5 — the template library, not per-Character data
  templateId,
  fieldCategory,                       // "build" | "figureDescriptor" | "culturalOriginSpeech" | "voice" | "eyeDescription" |
                                       // "hairClause" | "eyebrowDescription" | "hands" | "postureAndPresence" |
                                       // "temporaryState" | "maiming" | "dress" | "marking" |
                                       // "familyResemblance" | "closingLine"
  templateText,                         // with slot placeholders for the actual field value
  reusesTraitFlavorLine: bool,           // true where it borrows Traits' own existing phrasing
  pairsWithTraitField: bool,             // true for personality-inflected templates
  framingTone,                          // nullable — "flattering" | "neutral" | "critical", §5.2's Observer-Coloring pool
}

FamilyResemblanceRecord {              // §5.1, new this pass — computed, not authored
  characterId,
  resemblesCharacterId,                 // parent, sibling, or recorded ancestor
  matchedFields: [...],                  // e.g. ["hairColor", "beautyTier", "physiqueTier"]
  resemblanceStrength,                   // "notable" | "striking" — gates whether a line is generated at all
  sourceIsImagoOrChronicleOnly: bool,      // true when the matched ancestor is otherwise only preserved in
                                        // the household record rather than personally known to the viewer
}

PlayerDetailPreference {               // §8 — a single, standing, account-level setting
  defaultDescriptionMode,               // "summary" | "standard" | "full"
}

DisguisedAppearanceOverride {           // §9, temporary
  characterId,
  activeDisguiseAttemptId,               // pointer to Fashion & Dress's own DisguiseAttempt record
  overriddenPortraitAssetRef,
  overriddenDescription,
  revertsOnDiscoveryOrExpiry: true,
}

FamilyPortraitRecord {                  // §6.1, new this pass
  familyPortraitId,
  householdId,
  subjectCharacterIds: [...],
  commissionedAtMonth,
  portraitAssetRef,                      // a composed, saved snapshot — never live-updated after creation
}

PortraitHistorySnapshot {               // §7, new this pass
  characterId,
  snapshotAssetRef,
  descriptionFullAtSnapshot,
  triggeringChronicleEventId,             // reuses Dynasty Chronicle's own significance tiering as the trigger
  month,
}

MaimingRecord {                          // §4.7, new this pass — extends Hair, Facial Hair & Body Marking's
                                        // BodyMarking schema with a harsher permanent tier
  characterId,
  affectedPart,                          // "hand" | "finger" | "eye" | "leg" | "other"
  cause,                                  // "militaryWound" | "arenaInjury" | "laborAccident"
  causingEventId,                          // pointer to the triggering Military & Combat/Games & Spectacle/
                                        // Labor & Slavery record
  chronicleEligible: true,
}
```

---

## 13. Open Questions

- **All numeric sizing**, per this project's standing convention — no specific paragraph-count or word-count target is fixed for the Full Description; "several paragraphs" is a design intent, not a hard spec. Family Resemblance's own matching threshold ("notable" vs. "striking"), Observer-Coloring's own Opinion-score cutoffs, and the exact stage boundaries behind §4.6's own pregnancy granularity are all unsized.
- **Template library size**, now genuinely reduced in urgency by §5.1 and §5.2's own variety (the same base facts now support materially different generated text depending on family ties and who's asking), but not eliminated as a real production question for whichever team eventually authors the library.
- **Portrait History storage scope.** §7 caps snapshot frequency to Chronicle-eligible moments specifically to keep this manageable, but doesn't specify a hard cap on total retained snapshots across a very long-lived Character or a many-generation dynasty — a real technical question for implementation.
- **Family Portrait roster flexibility.** §6.1 allows the player to adjust who's included beyond the default head/spouse/children set, without specifying any limit on group size or whether a deceased member can be included retroactively from their own last Portrait History snapshot.
- **Maiming's exact severity threshold.** §4.7 establishes that a "sufficiently severe" wound or accident triggers it without specifying where that line actually sits relative to Military & Combat's own wound-outcome tiers or Labor & Slavery's own accident math — left to those documents' own eventual numeric passes.
- **Whether a Maiming ever affects mechanical performance beyond description.** §4.7 is scoped to this document's own descriptive purpose only; whether a missing hand also carries a Labor Skill or Martial penalty is a question for Military & Combat/Labor & Slavery to answer, not this document.
