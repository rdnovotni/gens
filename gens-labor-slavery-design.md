# GENS — System Design: Labor, Slavery & Punishment (§6.3)

---

## 1. Scope & Role

This system covers how enslaved people enter the household, how they're treated day to day, and how that treatment resolves — through punishment, flight, manumission, or sale. It reads and writes Familia's stat blocks directly (§6.1: Legal Status, Loyalty, Fatigue, Reactive traits, permanent injury) and connects outward to Estate & Settlement (labor output), Policies & Edicts (household-wide defaults), Legal & Court (debt bondage, manumission formality), Military & Combat (war captives), Piracy & Banditry (kidnapping as a supply source), and Games & Spectacle (gladiator sourcing, handled in that system's own doc rather than duplicated here).

This is the system the design's "frank harshness, thematic honesty" pillar is most directly tested by, and it's treated that way throughout.

---

## 2. Acquisition

Every avenue you asked for, plus a few more that fell naturally out of the existing system list:

- **Slave markets** — a rotating stock the player can browse and buy from, with partial/uncertain information (§3).
- **War captives** — a direct output of Military & Combat campaigns; arrive in batches rather than one at a time, typically cheaper than market purchase but starting with worse Loyalty and a higher chance of Reactive traits like *Resentful* or *Defiant*, reflecting how they arrived.
- **Debt bondage & self-sale** — free citizens (or peregrini) unable to pay a debt can be bonded into service, either by the player acquiring someone else's defaulting debtor or — by default, though configurably (§8) — by the player's *own* household falling into this during financial ruin.
- **Birth to an enslaved mother** — automatic, per the historical rule that a child's status follows the mother's (*partus sequitur ventrem*); resolves through Familia's childbirth mechanics (§6.1 §6) with legal status assigned at birth rather than requiring a separate acquisition action.
- **Inheritance & gifts** — slaves arriving as part of a bequest, a dowry, or a gift from a patron/ally, priced at zero but otherwise identical to a market acquisition in terms of information uncertainty.
- **Legal seizure** — confiscated from a debtor or condemned party through Legal & Court proceedings (§6.16), often at a discount but with a higher chance of a *Resentful* or *Vengeful* starting trait.
- **Piracy & kidnapping** — buying from a source that is itself a Piracy & Banditry (§6.24) outcome; cheapest of all the legitimate-market options, but carries a standing Dignitas risk if the connection becomes known, and the highest chance of hostile starting traits.

---

## 3. Information & Deception

Consistent with wanting this "realistic and frank," a slave's full stat block is never fully visible at the point of acquisition:

- **Visible at a glance:** approximate age, an apparent health *range* (not the exact number), and origin.
- **Hidden or uncertain until owned:** exact Core Attributes and Labor Skills (shown as a rough band — "seems capable with tools" rather than "Craft: 62"), most personality traits, and the presence of any permanent injury not visibly obvious.
- **Deception:** a seller can actively misrepresent condition or skill — a market slave described as "strong and willing" may turn out *Sullen* and below-average Health. The odds and severity of being deceived scale inversely with the size/reputation of the seller (a reputable dealer misrepresents rarely; a back-alley deal is a real gamble) and can be reduced by paying for a formal inspection or by sending an Intrigue- or Learning-skilled household member (steward, physician) to assess the person first.
- **Warranty & recourse:** consistent with real Roman customary practice (the aedilician edict required sellers to disclose known defects), discovered deception isn't a dead loss — a buyer who later proves a seller knowingly misrepresented a purchase has recourse through Legal & Court (§6.16): a partial refund, a voided contract, or damage to the seller's own standing if the case becomes public. The same exposure runs the other way — a player who knowingly sells a misrepresented individual (§9) can face an identical claim from the buyer, making deceptive selling a real if tempting risk rather than a consequence-free way to offload a problem.

---

## 4. Labor Assignment & Output

This system doesn't duplicate Familia §4's duty-slot mechanic; it supplies the population that fills it. Output from a given duty slot is a function of the assigned person's relevant Labor Skill, moderated by current Health and Fatigue (heavy fatigue caps effective output regardless of raw skill) and by their active Regimen settings (§5). Skilled individuals (a trained scribe, a physician, a craftsman) command a real price premium at acquisition and are correspondingly rarer in war-captive batches than in market stock or inheritance — reflecting that skilled labor was valuable enough not to be casually captured and dumped on the market.

**The Vilicus.** Field labor specifically has a natural intermediate tier between an ordinary Field Hand duty slot and a full Court Position: the **vilicus**, an overseer (often an enslaved or freed individual themselves) who manages day-to-day field operations. Mechanically, a vilicus sits above the Labor Skills tier but below a true Court Position — assigned from Stewardship-leaning individuals regardless of legal status, and directly moderating the output and Regimen compliance of everyone under them, so a good vilicus meaningfully raises a field gang's effective output while a poor or resented one drags it down. This is the concrete bridge between this system and Estate & Settlement's building output (§6.2).

**Education as a deliberate choice.** Investing Education & Culture (§6.14) resources in an enslaved or unskilled household member raises their Labor Skills and Core Attributes — making them more valuable and eligible for skilled duties or even a Court Position later — but the same investment raises their capability to forge documents, plan a more effective escape, or organize others, feeding directly into this system's flight-risk (§7) and Unrest math. Educating labor is a real strategic bet, not a strictly beneficial upgrade.

---

## 5. The Regimen System

The standing, Free-Cities-style management layer — separate from one-off Punishment actions (§6), though the two interact.

**Four axes**, each a simple tiered setting:

| Axis | Tiers | Primarily affects |
|---|---|---|
| **Diet** | Meager / Adequate / Generous | Health trend, upkeep cost |
| **Accommodation** | Bare / Basic / Comfortable | Health trend, Loyalty trend, upkeep cost |
| **Permitted Freedoms** | Confined / Restricted / Free Movement | Flight opportunity (§7), Loyalty trend, access to errands/travel-adjacent tasks |
| **Discipline Strictness** | Lenient / Firm / Harsh | Fatigue/output ceiling, Loyalty trend, Unrest contribution, Reactive-trait odds |

**Scope of application:** the player can set a Regimen at the **group level** (e.g., "all Field Hands," "all Household Slaves") as a working default, and layer **per-individual overrides** on top for anyone who warrants different treatment — a trusted steward on Comfortable/Free Movement while the general field-labor pool sits on Basic/Restricted, for instance. An individual override always takes precedence over its group default.

**The tradeoff, stated plainly:** better provisioning and more freedom cost more in upkeep and reduce short-term output ceilings, but improve Loyalty and Health and suppress Unrest and flight risk over time. Harsher, cheaper regimens raise the achievable output ceiling and lower upkeep in the short term, at a rising cost in Unrest, flight risk, and Reactive traits the longer they're sustained — there's no dominant setting, only a tradeoff the player owns.

**Relationship to Policies & Edicts (§6.12):** the household-wide "slave treatment policy" slider established in that system is the **default new acquisitions start on** — it's the coarse, estate-wide setting. This Regimen system is that same idea made granular and overridable at the group and individual level. The two aren't competing systems; §6.12 sets the household's general posture, §6.3 lets the player actually differentiate.

---

## 6. Punishment Actions

A simple, deliberately blunt one-off ladder — this is the escalation path an individual action can take, distinct from the standing Regimen above:

| Tier | Example action | Typical effect |
|---|---|---|
| **Mild** | Extra duties, reduced rations for a period | Small Fatigue/Loyalty hit, cheap, low Unrest impact |
| **Moderate** | Corporal punishment | Larger Loyalty and Fatigue hit, Health risk, real Unrest increase, a chance of acquiring a Reactive trait (*Resentful*, *Defiant*) |
| **Severe** | Maiming/mutilation | Rare and drastic — a near-guaranteed permanent injury (Familia §3.1), a major Loyalty hit, a significant Unrest spike, but a strong, lasting suppression of that individual's Ambition/defiance |
| **Lethal** | Execution | Removes the person permanently; the household-wide Loyalty/Unrest effect depends heavily on whether the act is perceived as justified (an execution for a serious offense reads very differently than an arbitrary one) and carries a real Dignitas risk if seen as excessive, especially if handled outside Legal & Court's formal process |

Every punishment action logs to the Chronicle. Depiction defaults to **frank and direct**, matching the design's stated tone, but sits behind the same **content-intensity toggle** used elsewhere (§6.1 §2.9, the fertility-risk toggle) — the underlying mechanics and consequences stay identical regardless of the setting; only the narration softens.

**Legal risk beyond Dignitas.** Severe and Lethal punishments carry a chance of triggering actual Legal & Court (§6.16) attention, not just a reputational cost — consistent with the real, if limited, legal checks on extreme cruelty that existed by this period. A sufficiently public or excessive case can prompt a magistrate's inquiry, with outcomes ranging from a formal warning to an actual finding against the player, independent of whatever Dignitas damage already occurred. This is the one place punishment severity can produce a consequence beyond reputation and Unrest.

---

## 7. Flight & Recapture

A full mechanic, not an abstraction:

- **Flight risk** is derived from Loyalty (low), household Unrest, the individual's Regimen (Confined/Harsh suppresses *opportunity* but raises *motive*; Free Movement raises opportunity but a well-treated person under it is less likely to use it), and personal traits (*Resentful*, *Ambitious* raise it; *Content*, *Grateful* lower it).
- **The escape event** triggers when flight risk crosses a threshold and an opportunity presents itself — during Travel, amid a chaotic Event (a fire, a raid, a disaster), or simply an unwatched moment.
- **Pursuit** is a player-initiated response with a limited window (a few months) before the trail goes cold: hire bounty hunters (a cost plus a success roll weighted by their Martial/Intrigue), or dispatch a Companion or household Court Position holder (a Marshal or Bodyguard) instead.
- **Outcomes:** recapture (the person returns — typically triggering an immediate Punishment choice, with a Loyalty hit either way since the attempt itself is a rupture); permanent loss (a real asset loss and a minor Dignitas hit for visibly "losing control" of the household); or capture-with-harm (injured or killed during pursuit, forfeiting whatever value they had). An escapee who's never recaptured is a loose thread the Chronicle and Rival Houses/Events systems can pull on later — resurfacing as a bandit, joining someone else's household, or becoming a minor recurring figure.

---

## 8. Manumission

Simplified versions of the three real historical mechanisms, each with a different cost/speed/formality tradeoff:

| Type | Mechanism | Cost/Speed | Notes |
|---|---|---|---|
| **Vindicta** | A formal act before a magistrate | Moderate cost, immediate | Ties into Legal & Court (§6.16); the most visibly "correct" method, carrying a small Dignitas bump for doing it properly |
| **Testamento** | Granted by will | Free at the time, but doesn't take effect until the *pater/materfamilias* dies | A genuine succession-planning tool (§6.9) — a way to reward loyal service without an immediate cost, at the price of the freedom being conditional on the master's death |
| **Censu** | Enrollment during a census | Cheap | Only available during an active census Event — a periodic window rather than an anytime option, adding a little real scheduling pressure to using it |

Whichever route is used, the outcome is identical on the Familia side: the person becomes a **freedman** (§6.1 §2.5), takes the patron's *nomen* (§6.1 §2.8), and carries ongoing *obsequium* obligations to the player as patron.

**Labor continuity.** Manumission doesn't default to the person simply leaving. Most freedmen realistically continue serving in some capacity — often the same duty, now as paid labor or a client relationship, or a promotion into a Court Position (§6.20) if their Core Attributes and standing warrant it. The player can choose to release a freedman from the household entirely, but that's a deliberate choice rather than manumission's automatic result.

---

## 9. Sale

Both modes you asked for, genuinely different rather than one being a strict subset of the other:

- **Quick/generic sale** — immediate, an anonymous buyer, a standard price (driven by apparent skill/health/age, minus a "quick sale" discount for the lack of negotiation). No relationship-web consequences beyond however the rest of the household reacts to losing that person.
- **Targeted/arranged sale** — the player chooses a specific buyer (a named rival house, a specialized dealer/collector), which can command a higher price for a notable or skilled individual, or serve as a deliberate favor/leverage move in Politics & Patronage. This route is visible: household members with an existing relationship-web bond to the sold person react accordingly, and the buying house's opinion of the player shifts based on how fair the deal was perceived to be.

**Family separation.** Either sale mode can break apart a parent and child, or a couple bound by a **contubernium** — the informal, legally-unrecognized union that was the practical equivalent of marriage among the enslaved, tracked in Familia's relationship web (§6.1 §2.7) as its own bond tag distinct from formal Spouse. Selling one half of a contubernium pair, or a mother away from a young child, is deliberately *not* softened by the interface: the game surfaces the bond plainly before the sale confirms, and the resulting relationship-web and Loyalty/Unrest fallout among those left behind is proportionate to how central that bond was — the same frankness the rest of this system holds to, applied to its most human stakes rather than only its harshest ones.

---

## 10. Content & Tone

Default depiction is **frank and direct**, consistent with the project's stated stance — described with narrative purpose rather than lingered on for shock value, the same "historical frankness without gratuitousness" line drawn everywhere else in the design. A **player-configurable content-intensity toggle**, part of the same settings family as the fertility/childbirth-risk and historical-restriction toggles, lets a player soften the narration without changing any underlying mechanic or consequence.

---

## 11. Data Model Additions

Extending the Familia record from its own doc:

```
{
  ...(Familia fields),
  acquisition: { method, date, price },              // §2
  regimen: { diet, accommodation, freedoms, discipline },  // §5 — per-individual; falls back to group default if unset
  flightRisk,                                          // derived, §7
  pursuit: { active, monthsRemaining, lastKnownLocation }, // §7, only if fled
  manumissionPlan: { type, effectiveOn }                // §8, e.g. a pending testamento grant
}
```

---

## 12. Open Questions Carried Forward

- **Exact pricing formula.** Weights across skill, health, age, appearance, and deception-risk for market/inheritance/seizure acquisitions aren't numerically specified yet.
- **Flight-risk thresholds.** The formula's inputs are listed (§7) but not its actual numeric thresholds or how often an "opportunity roll" occurs.
- **Regimen tier deltas.** The four axes and their tiers are specified (§5), but the exact numeric effect of each tier on upkeep, Health/Loyalty trend, and Unrest isn't yet tuned.
- **Group-default-vs-override blending.** Individual overrides take precedence (§5), but what happens when a group default changes while several individual overrides already exist isn't specified — do overrides persist untouched, or partially re-baseline?
- **Debt-bondage-of-own-household severity.** §2/§8 confirm it's possible by default and configurable, but not whether the configuration is a single on/off or has its own severity gradient.
- **Legal-risk trigger thresholds.** §6's new legal-risk note establishes that severe/public punishments can draw magistrate attention, but not the actual probability curve or what counts as "sufficiently public."
- **Warranty claim resolution.** §3's recourse mechanic establishes the consequence categories (refund, voided contract, reputational damage) but not the actual Legal & Court process for adjudicating a disputed claim.
- **Contubernium formation.** Whether these bonds form automatically over time between eligible enslaved individuals (mirroring how the relationship web already accrues) or require explicit player facilitation isn't yet decided.
- **Vilicus vs. standard Steward distinction.** §4 establishes the vilicus as a distinct intermediate tier, but exactly how its output-moderation math differs from a generic Court Position Steward's isn't yet specified.
