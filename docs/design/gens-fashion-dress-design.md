# GENS — System Design: Fashion & Dress
*The Free Cities-style "dress your household" layer: what a Character actually wears, how it reads mechanically, and how far the player's own authority over it extends — from an absolute hand over an enslaved wardrobe down to a real, consent-aware request where a free spouse or adult child is concerned. This document covers structure and mechanics only; the full itemized garment roster (the actual named pieces, by region and era) is the next, companion document. This pass adds Hair, Cosmetics & Grooming as a distinct fashion layer, Era-Appropriate Fashion Drift across the game's own 133 BC–AD 235 range, a Disguise & Deception mechanic tying dress to Flight, Espionage, and Travel, a direct Marriage Market/Dowry Trousseau tie, and a worked illustrative example.*

---

## Contents

1. Scope & Role
2. The Wardrobe — Outfit Tier and Garment Slots
3. Hair, Cosmetics & Grooming — Fashion, Not Appearance
4. Occasion-Based Dressing & Automation
5. Status Markers — Gated Garments
6. Household Dress Policy — The Player's Control Hierarchy
7. Free Family Members — Consent, Preference & Friction
8. Livery — The Household in Matching Dress
9. Milestone & Ceremonial Dress
10. Cultural & Regional Dress
11. Era-Appropriate Fashion Drift
12. Disguise & Deception
13. Fashion as Leverage — Reward, Punishment & Scandal
14. Fashion & the Marriage Market
15. Illustrative Example
16. Cross-System Integration
17. Data Model
18. Open Questions

---

## 1. Scope & Role

The core doc's own §7.11 already established that a Character's portrait includes "status-appropriate dress/grooming that updates automatically with office, wealth, and age" — real, but entirely passive: a rendering rule, not a system the player actually touches. This document is what turns that passive rule into a real, *Free Cities*-depth mechanic: a Character's clothing, hair, and grooming are now something the player can see, choose, restrict, reward with, or impose — on their own character, on family, and, with real and different authority, on the enslaved and the household's own hired staff.

This document does not re-invent anything Buildings & Production Chains already built. Every garment a Character can actually wear is, ultimately, one of that document's own named textile-chain products (a Tunic, a Toga, Sandals, Jewelry, a Purple-Trimmed Toga) or a close cousin of one. This document's job is the layer *above* the goods themselves: how those goods become a specific person's specific outfit, what wearing (or being denied) a given piece actually means, and who gets to decide. The full, exhaustive named-garment catalog — cut, regional variation, era-appropriate detail — is deliberately left to a dedicated companion **Fashion & Dress: Garment Roster** document, the same split this project already used for Traits (catalog) versus Characters (the engine the catalog plugs into).

**What doesn't move here:** the Appearance/Portraiture system (Core §7.11), the textile production chains (Buildings §4.6), and the Legal Status/Social Class framework (Familia §2.5) are all reused wholesale. This document adds the wardrobe layer connecting them.

---

## 2. The Wardrobe — Outfit Tier and Garment Slots

Every Character's **Wardrobe** is two things layered together, deliberately kept this simple rather than an itemized per-garment inventory sim:

**Outfit Tier** — a single, coarse quality/wealth scale, in the same register as Villa's own Decoration Style packages and Labor & Slavery's Regimen tiers: **Meager → Modest → Respectable → Fine → Opulent**. This is the everyday "how well turned-out is this person" reading, driving a real Dignitas/Interaction modifier (§5, §17) without requiring the player to hand-pick a shirt.

**Garment Slots** — a small number of named, status-bearing pieces layered on top of the Outfit Tier for specific purposes: a Toga (citizen-only, §5), a set of Jewelry, a distinguishing stripe or trim, a piece of Priestly or Military dress, and so on. A Garment Slot entry isn't a quality dial — it's a flag: either a Character has access to a specific named piece (and can choose to wear it for a relevant Occasion, §4) or they don't, gated by exactly the same Legal Status, Social Class, office, and Event conditions this project already uses everywhere else.

This two-layer model keeps the system legible at a glance — "a Fine-tier household, and the pater's own laticlavus stripe besides" reads immediately — while still supporting the real specificity the direction asked for.

---

## 3. Hair, Cosmetics & Grooming — Fashion, Not Appearance

A deliberate, direct line worth drawing clearly, since two documents now sit close together on similar ground: Familia §2.4 and Core §7.11 already define a Character's *natural* hair and eye color, build, and complexion as fixed Appearance attributes — what a person **is**. This section covers what a person **does with it** — a real, distinct, changeable third layer of the Wardrobe alongside Outfit Tier and Garment Slots, and a genuinely rich piece of real Roman social history in its own right.

- **Hairstyle** — a real, era- and status-legible choice (§11), from an elite Roman matron's elaborately curled and piled style, requiring real time and a skilled *ornatrix* (hairdressing companion role, a natural minor addition to Companions & Court Positions' own roster) to a simple, practical style suiting field labor or Frontier life.
- **Wigs** — a real, genuinely popular Roman elite fashion, worth naming directly rather than treating hair as always natural: Germanic and Gallic blonde hair was a real, sought-after import specifically for Roman wigmaking, a nice concrete tie to the Gallic Frontier's own regional identity (Resources & Goods) and a small, specific luxury good in its own right rather than lumped into generic Jewelry.
- **Cosmetics** — real, attested Roman practice: kohl for the eyes, red ochre for the cheeks and lips, and, most notably, **cerussa** — white lead face powder — a genuinely popular elite beauty product that was also, historically and honestly, a real slow poison. This document names that tension directly rather than softening it: a sustained Cerussa-based cosmetic regimen is a small, real, optional Health-trend cost (Disease & Public Health) layered under its Beauty-tier/Romantic-interaction benefit (§14) — vanity with a real, historically accurate price, matching this project's own "no dominant strategy" pillar in miniature.
- **Grooming** — beard and facial-hair presentation specifically, which §11 below treats as a genuine, era-tracked fashion signal rather than a fixed personal choice.

None of this touches the underlying Appearance attributes themselves — a Character's natural hair color doesn't change because they're wearing a blonde wig, exactly as a Plain-tier Character's underlying Beauty tier (Traits §3.2) doesn't change because they're well-dressed. This layer sits on top of, and interacts with, both without overwriting either.

---

## 4. Occasion-Based Dressing & Automation

Consistent with the project's own automation principle (the player sets standing decisions; the simulation runs itself), a Character doesn't need to be dressed by hand for every scene. The Wardrobe maps Outfit Tier, Garment Slots, and Hair/Grooming choices (§3) against a small set of **Occasion categories** — Everyday, Formal/Public, Ceremonial, Military/Campaign, Mourning, Athletic/Bathing — and the game automatically selects the appropriate combination whenever an Interaction, Event, or Activity calls for one: a Triclinium feast (Characters §9.1) pulls Formal; a Field Labor duty slot pulls Everyday/Work-tier regardless of the Character's broader Outfit Tier; a funeral pulls Mourning (§9) without the player needing to remember to switch it.

The player can always override a specific Occasion's default manually — dressing deliberately down for a humility-signaling appearance, or deliberately up for an occasion that doesn't automatically call for it — but the sensible default handles the ordinary case without asking for constant attention, the same shape Steward/Council Auto-Management already establishes for routine business generally.

---

## 5. Status Markers — Gated Garments

This is where Fashion & Dress pays off several threads other documents already laid down without ever building the mechanism connecting them. A Garment Slot is gated exactly the way this project already gates everything else — Legal Status, Social Class, held office, or a specific Event/record — and wearing (or being caught wearing) one outside its gate is a real, mechanically consequential act, not flavor text.

| Garment | Gate | Consequence if worn without meeting the gate |
|---|---|---|
| **The Toga** | Roman Citizen only (Familia §2.5) | The single most legible citizenship marker in the game; a non-citizen in a toga is an immediate, visible Scandal/Legal & Court flag |
| **Laticlavus stripe** (broad purple) | Senatorial Social Class | Impersonating rank — a severe Scandal trigger, treated the same register as a Fabricated credential |
| **Angusticlavus stripe** (narrow purple) | Equestrian Social Class (Merchant Families §2) | As above, one tier down |
| **Toga Praetexta** (purple-bordered) | Held magistracy, or a freeborn child pre-Toga Virilis (§9) | Office-impersonation Scandal for an adult; simply incorrect/comedic for anyone else |
| **Tyrian Purple / Purple-Trimmed Toga** (full) | Wealth alone historically, but real-game gated by any active Sumptuary Edict (Politics & Patronage §8) | A direct, checkable Sumptuary Edict violation — this document is that edict's concrete enforcement surface |
| **Stola** | Roman citizen matron (married, freeborn) | The mirror status marker to the Toga for women; per real Roman practice, a woman convicted of adultery or working as a prostitute historically lost the right to it — this document's own direct, unflinching link to the Infamia status Romance & Sexuality §13 already named |
| **The Toga, worn by a prostitute** | *(historical inversion — see below)* | — |
| **Bulla** | Freeborn child, pre-Toga Virilis | Purely a childhood marker; its removal is the Toga Virilis ceremony itself (§9) |
| **Priestly vestments (Flamen/Augur/Pontifex regalia)** | Holding the relevant Priesthood Office (Religion §6.2) | Impersonating a priest — a real sacrilege-adjacent Legal & Court case (Religion §9) |
| **Vestal robes** | An active VestalRecord (Religion §6.3) | Not applicable outside the office; a Vestal's own dress is otherwise entirely outside household control, consistent with that document's own exemption from *patria potestas* |
| **Military dress/insignia** | A held Military rank or Praefectus appointment (Military & Combat §3, Companions & Court Positions §5.2) | Impersonating rank in the field is a real Military & Combat/Legal & Court matter |
| **Racing faction colors** | Sponsorship of that faction (Games & Spectacle §7's four color factions) | Purely social — wearing a rival faction's colors at the wrong gathering is a minor, real, and often genuinely funny social friction point rather than a hard violation |
| **The Signet Ring** *(new)* | Head of household, or a formally delegated authority | Its impression is the real, historically standard method of sealing a letter or document (Correspondence & Letters) — an unauthorized signet is a direct forgery vector, not merely a fashion violation |

**The historical inversion, played straight rather than softened, per this project's own frankness pillar:** real Roman social convention required a prostitute to wear the toga specifically *because* it was the men's citizen garment, a deliberate public marker excluding her from the married-citizen-woman's own stola — the exact reverse of the ordinary "toga marks a citizen man" rule above. This document names that historical detail directly, mechanically tying it to the Brothel (Buildings §4.8) and the Infamia legal status (Romance & Sexuality §13) it already established, rather than leaving the connection implicit.

---

## 6. Household Dress Policy — The Player's Control Hierarchy

For the enslaved and for hired free staff, dress is a real, deliberate lever the player holds close to absolutely — and this document reuses Labor & Slavery's own Regimen structure (§5 of that doc) wholesale rather than inventing a parallel hierarchy:

**Household Dress Policy** (the household-wide default, set alongside Policies & Edicts' own Household Regimen Posture, §2.2 of that doc) → **Group Dress Policy** (a working default for "all Field Hands," "all Household Slaves," "all Companions") → **Individual Override** (always wins). Exactly the same three-level resolution Regimen already uses, and exactly the same reason: a coarse estate-wide setting the player can leave alone, with real room to differentiate a favored steward from the general field-labor pool without re-setting everyone by hand.

**The tradeoff, in the same register as Regimen's own:** a higher Household Dress Policy tier costs more in ongoing upkeep (drawing on the same Tunic/Cloth/Sandal goods Buildings' textile chains already produce) but generates real, ongoing Dignitas during any Group Interaction the household hosts (§8) and a modest Loyalty trend improvement — a household that visibly clothes its people well is read, by guests and by the people themselves, as a household that takes care of its own. A deliberately Meager-tier household dress policy is a real, legitimate frugality choice (reading well to a Mos Maiorum-leaning Household Doctrine audience) at the cost of exactly that Dignitas and Loyalty upside.

**A punitive floor, not a punitive action in itself:** setting an individual's dress deliberately below the household default is available and, per §13, sometimes a deliberate Mild Punishment choice — but doing so as a matter of blanket policy against an entire group reads, and is tracked, as a harsher Regimen posture generally, feeding that document's own Unrest math rather than existing as a separate penalty track.

---

## 7. Free Family Members — Consent, Preference & Friction

The direction is explicit that the player's authority here is real but not identical across the household — "sometimes family as well" implies real limits, not an unquestioned toggle. This document draws that line directly:

For a **free adult family member** (a spouse, an adult child, a sibling), dressing them is a real **Interaction** (Characters §9.1's existing catalog — a Request, a Gift-Giving, or, where it's an outright imposition rather than a suggestion, closer to a Rebuke in tone) rather than a menu setting applied without their input. The outcome reads through exactly the Personality Axes and Traits this project already built for this purpose: a Refined Character is a natural, easy audience for an upgrade; a Frugal one resents an imposed Opulent-tier wardrobe as wasteful regardless of the Dignitas payoff; a Dutiful child accepts a parent's imposed choice more readily than a Rebellious one does; and mismatching a Character's own established preference against an imposed choice generates a real Opinion cost, the same shape Villa's own Cubicula personalization (§6 of that doc) already uses for room assignment versus a resident's own trait-driven preference.

**A minor child** sits closer to the enslaved-and-staff model — a parent's *patria potestas* (Legal & Court §6) gives real, largely unquestioned authority over a household-born child's dress, consistent with how that authority already operates everywhere else in this project — but even here, a sufficiently Rebellious/Wayward Adolescent's resentment at an imposed choice is legitimate, minor Reactive-trait-adjacent texture rather than an enforced non-event.

**What the player never gets, regardless of relationship:** authority over a Vestal's dress (§5's own carve-out, mirroring that office's *patria potestas* exemption exactly) or over anyone genuinely outside the household — a rival house's own members dress themselves, exactly as their own Ambitions, Doctrine, and standing dictate.

---

## 8. Livery — The Household in Matching Dress

A dedicated, named mechanic for a real historical elite practice worth calling out on its own rather than leaving as an unnamed side-effect of §6's Group Dress Policy: **Livery** is a household electing a single, deliberately uniform look — a shared color, a shared trim, a shared quality tier — for its enslaved staff or hired retinue, the wealthy Roman equivalent of a modern household's matching staff uniforms. Setting a Livery Style is a real, one-time (revisable) choice, costed against the same textile goods any Group Dress Policy already draws on, and its payoff is concentrated specifically where it would actually be seen: a genuine, real Dignitas bonus during any **Group Interaction** the household hosts (Characters §9.8 — a Salutatio, a Triclinium feast, addressing the Curia) on top of whatever the household's baseline Dress Policy tier already contributes, since a uniformly and well-dressed staff visible to an entire room of guests reads as a deliberate, coordinated display of wealth in a way an individually-varied but equally well-dressed staff doesn't quite achieve.

A Livery choice is also a real, legible **Household Doctrine** (Policies & Edicts §3) signal — a Domus Mercatoria house's Livery reasonably leans toward visible, novel opulence; a Mos Maiorum house's toward restrained, traditional uniformity; a Domus Dura house's toward a genuinely severe, minimal standard that is itself part of that Doctrine's own harsh character.

A household is free to maintain **more than one Livery Style at once** — a distinct look for house staff answering the door at a Salutatio versus a plainer, practical standard for field labor never seen by a guest — resolving §18's own earlier open question directly: Livery is scoped per Group Dress Policy tag (§6), not capped at one household-wide standard.

---

## 9. Milestone & Ceremonial Dress

Several of this project's own existing lifecycle and Activity beats have a real, historically specific dress component this document gives a proper mechanical home:

- **Toga Virilis.** The real Roman coming-of-age rite — a freeborn boy's *bulla* (§5) formally removed, the *toga virilis* worn for the first time — is this document's own concrete dress-specific beat layered onto the Adolescence-to-Adulthood transition Events already tracks as a Scripted Event (Events §2). A genuine, small Dynasty Chronicle-eligible family milestone, not merely a stat-threshold crossing.
- **The Flammeum.** The real flame-colored bridal veil is this document's own concrete dress contribution to Familia §5's marriage mechanics and the Activity Engine's Weddings activity type — a specific, named, historically grounded detail rather than a generic "wedding outfit."
- **The Dowry Trousseau.** A real, historically standard component of a bride's own dowry: a set of fine clothing and jewelry brought into the marriage as part of her own dowry value (Familia §5), distinct from land or coin. This document treats a Trousseau as a real, optional dowry component a father can invest in specifically — a Fine or Opulent Trousseau raises a match's own perceived alliance value at Familia's own dowry-calculation stage, a genuine, gender-specific expression of wealth display distinct from a straightforward cash dowry.
- **Toga Pulla.** The real dark/undyed mourning toga (and its equivalent for women) is this document's own dress-specific tie into Ancestor Veneration & Funerary Customs — the entire household's Outfit automatically shifts to the Mourning Occasion category (§4) for the customary period following a death, without requiring the player to remember to set it.
- **Office Investiture.** Winning a magistracy (Politics & Patronage §5) or a Priesthood (Religion §6.2) automatically unlocks the relevant Garment Slot (§5) the moment the office is held — a small, real "you can now wear this" reward beat, the dress equivalent of a new Combo Title becoming available.

---

## 10. Cultural & Regional Dress

Cultures of the Known World's 36 playable cultures each carry real, historically distinct dress traditions — Gallic trousers (*bracae*), a Greek chiton and himation, Egyptian linen, Eastern robes — and this document gives that texture a direct mechanical hook rather than leaving it purely cosmetic: a non-Roman-culture Character's choice to adopt Roman dress (or a Roman Character's choice to adopt a foreign style while stationed in a foreign region) is a small, visible, legible expression of the **Assimilated ↔ Unbowed** trait pair (Traits §6.6) and feeds Reputation Duality's local-standing axis (Politics & Patronage §2.1) exactly the way that trait pair already does — this document doesn't add a new mechanic here, it gives an existing one a genuinely visible form. The player can set a household member's default dress culture as a real, standing choice (their own Household Dress Policy override, §6) — a deliberate act of cultural signaling either direction, not just a passive backdrop detail.

A finer imported garment or textile — Silk chief among them (Resources & Goods §7) — also functions as a real, concrete **diplomatic gift** in Diplomacy with Non-Roman Peoples' own gift-and-favor mechanics, a fine Roman toga or a length of genuine Chinese-origin Silk each reading as a deliberate, legible gesture in opposite directions across that relationship.

---

## 11. Era-Appropriate Fashion Drift

New this pass, and a genuine, low-cost way to make the game's own full 133 BC–AD 235 range *feel* different depending on when a playthrough is actually set, in the same spirit as Cultures of the Known World's own Living Cultural Drift concept. Roman fashion was not static across three and a half centuries, and several real, well-documented shifts are worth naming directly as flavor-and-light-mechanical texture rather than a rigid, heavily-tuned system:

- **Facial hair.** The Republic and early Principate favored a clean-shaven look; by the reign of Hadrian (a real, well-attested shift, and reportedly a personal preference of the emperor himself), a full beard became fashionable among elite Roman men, associated with Greek philosophical affectation. A household's own men styling themselves against the current era's fashion — clean-shaven under Hadrian, bearded under Augustus — is a small, real, optional flavor signal (a mild Traditionalist/Popularist or Cosmopolitan/Xenophobic-adjacent read) rather than a hard mechanical penalty.
- **Toga draping.** The toga's own draping style grew more elaborate and voluminous over the imperial centuries compared to the plainer Republican-era wrap — a purely cosmetic, era-flavored rendering detail for the companion Garment Roster document to actually specify.
- **Hairstyle fashion (§3).** Elite Roman women's hairstyles are real-historically among the most datable fashion markers in the ancient world, with recognizably distinct, well-documented styles across different imperial dynasties. A household that keeps a materfamilias or daughter styled in a fashion associated with a prior era reads as a small, deliberate Traditionalist statement (or, less charitably, simply out of step) exactly as an anachronistic style would in any other era.

This is intentionally light-touch flavor layered on top of §2–§10's own mechanical structure, not a separate simulation — a playthrough beginning in 133 BC and one beginning under Trajan should simply *look* different by default, the same unforced texture this project already gives regional starts.

---

## 12. Disguise & Deception

A genuine, new mechanical use for the Wardrobe beyond display: dress as a tool of concealment, tying directly into three systems that already needed exactly this kind of concrete surface.

- **Flight risk.** Labor & Slavery's own flight-risk and pursuit math (§7 of that doc) already reads Loyalty, conditions, and opportunity; this document adds a genuine, concrete lever on top — an enslaved individual who has acquired (stolen, been gifted, or otherwise obtained) free-citizen-appropriate Outfit Tier clothing gets a real, if modest, boost to a flight attempt's success odds and to evading recapture, since appearing free at a glance is a real, historically plausible advantage during an escape. A pursuer's own search correspondingly can specifically flag "missing/appropriated fine clothing" as a lead.
- **Travel incognito.** Deliberately dressing down (or, for a lower-status Character passing through a dangerous area, dressing up) is a legitimate, player-chosen Travel posture — reducing banditry/Piracy target-selection odds (Piracy & Banditry) at the cost of the Dignitas and reception benefits ordinary Fine-tier travel dress would otherwise carry, a real, direct tradeoff rather than a strictly dominant choice.
- **Espionage cover.** A planted agent or a Household Spymaster's own operation (Espionage §6.15, future) naturally draws on this same disguise layer — a spy dressed and groomed to match a target culture or class (§10, §3) is this document's own concrete contribution to that system's eventual cover-identity mechanics.

A **discovered** disguise — a slave caught in appropriated fine dress, a spy's cover blown — resolves through whichever underlying system actually owns the consequence (Labor & Slavery's recapture and punishment ladder, Legal & Court, or Espionage's own discovery mechanics), consistent with this project's standing reuse-over-reinvention rule: this document supplies the lever, not a parallel discovery-and-punishment system of its own.

---

## 13. Fashion as Leverage — Reward, Punishment & Scandal

Dress is never merely decorative in this system — it's a real lever across three registers this project already built, each simply given a concrete dress-shaped expression here rather than a new mechanic of its own:

- **Reward.** Gifting a fine garment or a piece of Jewelry is a legitimate, specific instance of the existing Gift-Giving Interaction (Characters §9.1), reading as a real Loyalty and Opinion boost — a plausible, cheap-in-Denarii, high-in-personal-meaning alternative to a cash gift.
- **Punishment & Humiliation.** Forcing a deliberately degrading, below-station Outfit Tier on a specific individual — stripping an earned Garment Slot, or publicly reducing someone accustomed to Fine or Opulent dress to Meager — is a legitimate **Mild** Punishment (Labor & Slavery §6's own ladder), cheap and low-Unrest exactly as that tier already specifies, but carrying real, felt weight precisely because dress is visible to the entire household in a way a private ration cut isn't.
- **Scandal.** A Sumptuary Edict violation (§5), a status-marker impersonation (a non-citizen in a toga, a false laticlavus), or a genuinely transgressive dress choice by real Roman social convention (cross-dressing carried a real, attested social anxiety in Roman moral discourse) all route directly into Scandal's own shared aftermath engine rather than a bespoke dress-scandal mechanic — this document is simply one more legitimate trigger source alongside the ones Scandal already lists, weighted at whatever severity that document's own tiering assigns a status-transgression case generally, rather than a special harsher or lighter carve-out of its own.

---

## 14. Fashion & the Marriage Market

A direct, worthwhile tie this pass makes explicit rather than leaving implicit: Traits §3.2's Beauty spectrum already grants a Romantic/Social interaction bonus scaling with innate Beauty tier (Comely, Beautiful) — a fixed, unchangeable attribute. This document's own Outfit Tier and Hair/Cosmetics layer (§3) is the real, *earned* counterpart: a Plain-tier Character dressed and groomed at Fine or Opulent level, with a well-chosen Trousseau (§9) behind them, has a genuine, real way to compete in Familia's own marriage market against an innately Beautiful rival who dresses carelessly — a direct, concrete expression of Design Pillar #1's "no dominant strategy," and a real reason for a player managing a plainer heir's own prospects to actually engage with this system rather than treating it as pure decoration.

---

## 15. Illustrative Example

*(Texture only — no numbers implied.)*

> **Aemilia**, a citizen daughter of Plain Beauty tier, approaching marriageable age in a household whose Doctrine leans Domus Mercatoria.
>
> Her father invests in a genuine Fine-tier Trousseau (§9) — several well-made garments and a modest set of Jewelry — ahead of opening marriage negotiations, and sets her personal Household Dress Policy override (§6) to Fine, above the household's own Respectable baseline, specifically ahead of a Triclinium feast (§4's Formal Occasion) where a prospective match's own family is attending.
>
> Aemilia herself, a Refined-trait Character, welcomes the upgrade — no friction under §7's own consent check. Her ornatrix (§3) styles her hair in a fashionable current style rather than an outdated one (§11), a small additional social-reception boost layered on top of the Trousseau itself.
>
> The match succeeds in part on the strength of this presentation — §14's own "no dominant strategy" principle playing out concretely: an innately Beautiful rival candidate from a poorer house, dressed only at Modest tier, reads as the less immediately impressive prospect at the feast itself, whatever her underlying Beauty tier might suggest on paper.
>
> Years later, when Aemilia's own husband is caught in a genuine Sumptuary Edict violation (§5, §13) — displaying Tyrian Purple his Social Class doesn't clear — the household's own Dignitas takes a real, visible hit, and the very Trousseau that once won her the match becomes, briefly, a liability rather than an asset: a house suddenly scrutinized for excess reads its own existing finery less generously than before.

---

## 16. Cross-System Integration

- **Characters:** portrait rendering (Core §7.11) now reads its "status-appropriate dress" input directly from this document's Wardrobe record rather than an unspecified black box; the Interaction Catalog's Gift-Giving, Request, and Rebuke entries (§9.1) are this document's own concrete mechanism for §7's and §13's dress-related beats; Group Interactions (§9.8) read Livery (§8) directly.
- **Traits:** Refined/Coarse and Frugal/Extravagant (Traits §5.1) are reused directly for §7's free-family friction; Assimilated/Unbowed (§6.6 of that doc) is reused directly for §10; the Beauty spectrum (§3.2) is this document's own direct counterpart and complement per §14.
- **Labor & Slavery:** §6's Household/Group/Individual Dress Policy hierarchy is a direct, wholesale reuse of that document's own Regimen structure (§5); §13's Punishment tie-in is a named, concrete example under that document's own existing Mild tier; §12's Disguise mechanic is a genuine new lever on that document's own flight-risk math (§7).
- **Politics & Patronage:** the Sumptuary Edict (§8 of that doc) finally gets its concrete enforcement surface (§5) rather than an implied passive modifier — resolving that document's own flagged open question directly.
- **Merchant Families & the Equestrian Order:** the laticlavus/angusticlavus stripes (§2 of that doc) are this document's own named, gated Garment Slots.
- **Religion:** Priesthood vestments and the Vestal's own dress-autonomy exemption (§6.2–6.3 of that doc) are named Garment Slots and a stated hard carve-out (§7), respectively.
- **Romance, Sexuality & Lineage:** the Infamia legal status (§13 of that doc) gets its concrete, historically specific dress expression (§5's toga inversion) directly.
- **Buildings & Production Chains:** every garment this document names is, at the goods level, a Tailoring House, Dye Works, Cobbler's Workshop, or Goldsmith's Studio product (§4.6 of that doc) — no new goods invented here; Wigs (§3) are a natural small addition to that document's own textile/luxury goods list.
- **Villa:** §7's Cubicula personalization pattern (trait-driven default, player-overridable) is the direct template §7 of this document reuses for free-family dress preference.
- **Policies & Edicts:** Household Dress Policy sits alongside Household Regimen Posture as a natural thirteenth Standing Policy candidate; Livery Style (§8) is a real, legible Household Doctrine signal.
- **Games & Spectacle:** the four racing color factions (§7 of that doc) are a named, low-stakes Garment Slot category.
- **Cultures of the Known World:** §10's cultural dress hook gives every one of that document's 36 cultures a genuinely visible, mechanically-tied expression rather than pure flavor text; §11's Era Drift parallels that document's own Living Cultural Drift concept directly.
- **Diplomacy with Non-Roman Peoples:** §10's fine-textile diplomatic gift is a concrete new instance of that system's own gift-and-favor mechanics.
- **Correspondence & Letters:** the Signet Ring (§5) is this document's own concrete tie into that system's sealing/authentication mechanics.
- **Disease & Public Health:** §3's Cerussa cosmetic note is a small, real, optional Health-trend cost — vanity's own historically accurate price.
- **Espionage (§6.15, future) / Travel / Piracy & Banditry:** §12's Disguise mechanic is this document's own concrete contribution to cover identities, incognito travel posture, and banditry target-selection odds respectively.
- **Scandal:** §13 names this document as a direct, additional trigger source for that system's shared aftermath engine.
- **Familia, Events, Ancestor Veneration & Funerary Customs:** the Toga Virilis, Flammeum, Dowry Trousseau, and Toga Pulla (§9) are this document's own concrete dress contributions to those systems' existing lifecycle, marriage, and funerary beats.
- **Companions & Court Positions:** an appointment to a Senior Position or a Priesthood is this document's own automatic Garment Slot unlock trigger (§9); the Ornatrix (§3) is a natural minor addition to that document's own position roster.
- **Dynasty Chronicle (§6.11, future):** a Toga Virilis, a Sumptuary Edict violation caught in the act, or a household's own Livery becoming locally recognized are all real, small, legitimate Chronicle-eligible beats.

---

## 17. Data Model

```
Wardrobe {
  characterId,
  outfitTier,              // "meager" | "modest" | "respectable" | "fine" | "opulent" — §2
  garmentSlots: [
    {
      garmentId,           // pointer into the companion Garment Roster document
      category,            // "everyday" | "formal" | "ceremonial" | "military" | "mourning" | "athletic" | "cultural"
      gate,                // "citizenship" | "socialClass" | "office" | "eventRecord" | "none"
      unlockedVia,         // "legalStatusGrant" | "officeAppointment" | "lifecycleEvent" | "gift" | "purchase"
      currentlyWorn: bool,
    }
  ],
  hairAndGrooming: {         // §3 — new this pass
    hairstyleId,             // era- and status-flavored, §11
    wigId,                    // nullable
    cosmeticsTier,             // "none" | "modest" | "cerussaHeavy" — the last carrying §3's Health-trend cost
  },
  occasionDefaults: {       // §4 — auto-selection map, player-overridable per occasion
    everyday, formal, ceremonial, military, mourning, athletic
  },
  dressPreference,          // read from Traits — feeds §7's friction check
}

DressPolicy {              // §6, mirrors Labor & Slavery's Regimen record shape exactly
  scope,                    // "household" | "group" | "individual"
  targetId,                 // null for household-wide, a group tag, or a specific characterId
  outfitTier,
  liveryStyleId,             // nullable — §8; a group tag may hold its own distinct Livery
}

LiveryStyle {               // §8
  liveryStyleId,
  householdId,
  groupTag,                  // §8's own resolved multi-Livery scoping
  colorScheme, trimStyle, qualityTier,
  dignitasBonusDuringGroupInteraction: true,   // magnitude unsized, §18
}

DressImpositionInteraction {   // §7 — a specific instance of Characters' own Interaction Catalog
  interactionId,             // links to Characters' own Interaction record
  initiatorId, targetId,
  proposedGarmentOrTier,
  targetResponse,            // "accepted" | "resentfulAcceptance" | "refused"
  opinionDelta,
}

DowryTrousseau {              // §9 — a component of Familia's own dowry record
  characterId,
  garmentAndJewelryTier,       // feeds Familia's dowry/alliance-value calculation directly
}

DisguiseAttempt {              // §12
  characterId,
  context,                    // "flightRisk" | "travelIncognito" | "espionageCover"
  targetAppearanceProfile,     // the Legal Status/Social Class/culture being impersonated
  discovered: bool,
  resolvedVia,                // pointer to the owning system's own consequence record
}
```

---

## 18. Open Questions

- **All numeric sizing**, per this project's standing convention — Outfit Tier upkeep costs, the Dignitas/Loyalty magnitude of each tier and of Livery specifically, the Trousseau's own alliance-value contribution, the Cerussa Health-trend cost, and the Disguise mechanic's actual success-odds modifier are all unsized.
- **Garment Roster scope and count.** This document deliberately defers the actual itemized catalog — how many named pieces per category, how deep the regional/cultural and era-specific variation goes — to the dedicated companion document.
- **Portrait rendering granularity.** Whether the Appearance/Portraiture system (Core §7.11) visually renders every held Garment Slot and hairstyle choice distinctly, or only the currently-selected Occasion outfit, is an implementation question outside this document's own scope.
- **Cross-dressing's exact mechanical weight.** §13 names real Roman social anxiety around transgressive dress as a legitimate Scandal trigger without specifying its severity tier relative to Scandal's other existing categories — left for that document's own eventual weighting pass.
- **Whether a freed Character's Wardrobe carries forward.** Manumission (Labor & Slavery §8) changes Legal Status instantly; per this project's general no-automatic-reset convention, this document's working assumption is that a newly-freed individual's prior enslaved-tier Wardrobe simply persists until the player or the Character deliberately upgrades it, rather than auto-adjusting — worth confirming once that document's own manumission-aftermath texture is revisited.
- **Era Drift's actual granularity.** §11 names two or three concrete, well-documented shifts (beard fashion, toga draping, hairstyle dating) as illustrative rather than an exhaustive period-by-period fashion timeline — how many distinct "fashion eras" the full 133 BC–AD 235 range should actually be broken into is left to the companion Garment Roster document.
- **Ornatrix formalization.** §3 and §16 both flag a hairdressing Companion role as a natural addition to Companions & Court Positions' own roster, but this document doesn't formally add it there — left as a suggested follow-up to that document's own next revision pass rather than done unilaterally here.
