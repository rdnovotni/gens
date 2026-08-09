# GENS — System Design: Economy & Finance (§6.4)
*The treasury layer sitting above Resources & Goods and Estate & Settlement — where denarii actually comes from beyond goods sold, where it actually goes beyond upkeep, what happens when either side of a debt can't pay, and what the household is actually worth. This pass adds windfalls, the maritime loan, a Net Worth figure, symmetric debt bondage in both directions, Minting/seigniorage, Funded Actions, Capital Expenditure as its own category, and Insolvency as the system's own capstone worst-case.*

---

## Contents

1. Scope & Role
2. The Treasury
3. Income Beyond Yield
4. Expenses Beyond Upkeep
5. Taxation — Both Directions
6. Debt & Lending
7. Trade Routes — Mostly the Existing Model, With Real Levers
8. Estate Valuation & Net Worth
9. Insolvency & Financial Ruin
10. The Ledger & the Monthly Report
11. Cross-System Integration
12. Data Model
13. Open Questions

---

## 1. Scope & Role

Resources & Goods already built the mechanism that turns stored goods into denarii (§11–12 of that doc: a single tracked currency, base price by tier, Quality, regional scarcity, and a full dynamic market). Estate & Settlement already flagged that every building's upkeep "folds into Economy & Finance's monthly expense total." Labor & Slavery already named the Argentaria as the building behind its Debt Bondage acquisition path. This document is where all three of those forward references actually get paid off, plus the income and expense categories the core doc named but no prior pass has designed: **rents, contracts, wages, bribes, and taxation in both directions** — and, this pass, one-off windfalls (§3.4), a real Net Worth figure (§8), and debt bondage designed as the genuinely two-way, symmetric mechanic the setting calls for rather than only a risk the player's own household runs (§6.4–6.5).

What this document is *not*: it doesn't re-litigate goods pricing, storage, or market dynamics — Resources & Goods §11–12 remains authoritative there, unchanged. It doesn't design magistracies or the cursus honorum — that's Politics & Patronage's (§6.5) ground, and this document deliberately holds the same line Companions & Court Positions §5.5 already drew: financial *administration* is this document's business; political *office* is that system's, when it gets its pass. Where this document needs a political hook (see §5.2), it gates on the Curia — already established (Buildings §4.10) as the building that "unlocks holding and contesting local magistracies" — without designing what holding one actually entails.

---

## 2. The Treasury

A single tracked **Treasury** balance in Denarii, per settlement the player holds (a second settlement's Procurator, per Companions & Court Positions §5.3, runs a genuinely separate treasury — see §13's open question on whether that should ever consolidate). Every income and expense category below posts to it on the monthly tick, alongside goods-sale proceeds Resources & Goods already generates. The Treasury can run negative — this is the state that makes Debt (§6) load-bearing rather than decorative: a household doesn't simply fail to act when short on cash, it borrows, and a Treasury that stays negative too long is what actually triggers default.

A **Reserve Threshold** the player can set (a standing instruction, not a one-off action) determines how aggressively the household liquidates goods-overflow or takes on short-term debt to avoid dipping below it — the financial equivalent of the Regimen's standing-policy pattern already established in Labor & Slavery.

---

## 3. Income Beyond Yield

Resources & Goods already covers goods sold through Commerce buildings. Rents and Contracts are the two categories the core doc names directly; Tax Revenue and Windfalls round out the full picture designed here:

### 3.1 Rents — Coloni and Urban Tenants

The real gap this closes: Settlement Demographics tracks Coloni farming land the player doesn't personally cultivate through an Estate & Settlement building, and tracks Operarii/Opifices/Negotiatores/Aeditui/Curiales living in Insulae or Domus the player built — but neither pop group's presence generated the player any denarii until now. **Rent** is that missing link:

- **Agricultural rent** — a share of each Coloni household's harvest, paid in kind or converted to denarii at harvest time, scaling with how much unclaimed Agriculture-suitable land (Settlement Demographics §7.2) the player's own territory holds under tenancy rather than direct cultivation.
- **Urban rent** — a per-occupant charge on Insulae and Domus units, scaling with the building's tier and the occupying pop group's Legal Status/class tier (a Domus-heavy building full of Negotiatores pays more per head than an Insulae full of Operarii).

Both rent streams respond to the same levers Settlement Demographics already tracks: low Contentment or an overcrowded building (§7.3 of that doc) doesn't just risk Emigration — it also depresses collectible rent, giving neglect a second, more immediate financial consequence beyond the slower population one.

### 3.2 Contracts

One-off or standing deals negotiated above ordinary Market price, distinct from routine goods sale:

- **Military supply contracts** — a Barracks/Garrison/Fortress-holding settlement, or one near an active campaign, can contract to supply Grain, Weapons, Horses, or Siege Engines to the legions at a premium over Market rate. The natural negotiator is the Institor Maximus (Companions & Court Positions §5.2); the natural trigger is Military & Combat, once designed.
- **Mining/quarrying concessions** — rather than building out a Quarry or Mine chain personally, the player can lease extraction rights on a Hills/mine-favoring plot to an external operator (or, in the other direction, pay for a concession on land they don't own) for recurring concession income, a lighter-weight alternative to full vertical Industry investment.
- **Provincial supply contracts** — recurring bulk sale to the provincial administration itself (grain for the *annona*, timber for public works), functioning like a Market sale with a better price and a Dignitas-with-Rome tie rather than a genuinely separate mechanic.

### 3.3 Tax Revenue

See §5.2 — inbound taxation is real income once the player holds the qualifying office, but it's substantial enough to warrant its own section given the political gate involved.

### 3.4 Windfalls

One-off, non-recurring income, distinct from every category above by not being a standing policy or a repeatable transaction:

- **War spoils/plunder** — a Military & Combat campaign's victory payout, once that system exists; the natural counterweight to that system's own cost (wages for raised troops, equipment, the wages/upkeep already covered in §4.1 and Estate & Settlement).
- **Dowries received** — Familia's marriage market already tracks dowry as an alliance-value figure; when the player's own household is the one *receiving* a bride, her dowry posts here as a real Treasury windfall rather than only a narrative negotiation number.
- **Inheritance** — a death in the family (Succession & Dynasty, §6.9, once designed) transfers the deceased's personal wealth into the household Treasury, alongside whatever land/title transfer that system handles separately.
- **Treasure finds** — a rare Events (§6.8) payout: a hoard uncovered during construction, a lucky salvage, a gift from a grateful client — small in frequency, meant as flavor-forward variance rather than a planned income source.

### 3.5 Minting & Seigniorage

The Mint/Moneta (Buildings §4.10, City-stage, gated behind a Politics & Patronage milestone) has been a named building since that document's own pass without an actual income mechanism — this closes the gap. Operating a Mint generates ongoing **seigniorage**: the real, historically-grounded profit margin between a coin's face value and its actual metal/production cost, posting as a small but steady recurring income once built.

The Mint also carries a genuine temptation, in the same family as every other standing-policy lever this project gives the player (Regimen tiers, Tax Policy rates): the player can order a **debasement** — reducing the precious-metal content of newly-struck coin for an immediate, larger Treasury injection. This is a real short-term-gain-for-long-term-pain lever, not a free bonus:

- **Market consequence** — debased coin depresses purchasing power across the regional market Resources & Goods §12 already models, a real, felt inflationary effect on the player's own future transactions, not just an abstract penalty.
- **Political consequence** — coinage was an imperial prerogative; a settlement debasing its own currency without sanction is a serious matter once Politics & Patronage exists, risking Dignitas-with-Rome specifically (the Reputation Duality axis, §6.21) and a plausible Legal & Court exposure of its own.
- Debasement is deliberately rare-use by design — a lever a player reaches for in genuine crisis, not a routine income boost, the same restraint Labor & Slavery already applies to its own harshest Regimen and punishment options.

---

## 4. Expenses Beyond Upkeep

Estate & Settlement's building upkeep already folds in automatically. Five further categories:

### 4.1 Wages

Companions & Court Positions staff (Household Staff, Overseers, Senior Positions) and any hired-rather-than-enslaved labor filling a duty slot draw a recurring wage, scaled to position tier and the holder's Core Attributes — a free Vilicus or a Companion costs more to retain than an enslaved one, the concrete financial half of the free-labor-vs-slavery tradeoff Labor & Slavery's Regimen system already frames morally and mechanically. A poorly-paid free staff member's Loyalty erodes the same way a poorly-Regimen'd enslaved worker's does; wages are simply the lever available for the former where Regimen tiers are the lever for the latter.

### 4.2 Bribes

Not a building or a standing cost — a **deliberate action**, spent against a specific Legal & Court ruling, a Politics & Patronage negotiation, or a Piracy & Banditry/Legal investigation the player wants softened, once those systems exist to receive it. Recorded on the Ledger (§10) as a discrete line rather than folded into any recurring category, since a bribe's whole point is that it's an off-the-books-feeling expense with a specific target and a specific (never guaranteed) payoff.

### 4.3 Funded Actions

The same deliberate-action shape as Bribes, aimed outward rather than at a specific ruling: a one-off spend funding **Ludi** (games, ahead of Games & Spectacle's own design), a religious **festival** (ahead of Religion's), or a **public works** contribution beyond a civic building's normal construction cost (a bath renovation, an aqueduct extension) — each buying Dignitas, patronage standing, or religious favor directly rather than through any building's passive output. Policies & Edicts (§6.12) is named directly in the core doc as this spending's eventual home ("funded actions... for prestige, patronage, or religious favor"); this document simply gives it a Ledger category and a Treasury cost now, ahead of that system's own pass, the same treatment Bribes already got for Politics & Patronage and Legal & Court.

### 4.4 Capital & Acquisition Expenditure

A real gap this pass closes: large, discrete one-off purchases — a Slave Market acquisition, a new land parcel, a Villa stage upgrade's material cost, a significant livestock purchase — were always implicitly "just spending denarii" through whichever document actually names the good or building, but never had their own Ledger category distinct from the recurring hum of Wages and Upkeep. Naming it matters mechanically: Capital spending is exactly the category most likely to push the Treasury below the **Reserve Threshold** (§2) in a single tick, and is therefore the most common deliberate trigger for taking on new debt (§6.1) rather than an incidental one. A player weighing "can I afford this Domus upgrade and the new field slaves in the same season" is weighing two Capital line items against one Treasury, which is exactly the kind of concrete tradeoff this document exists to make legible.

### 4.5 Tribute & Outbound Taxes

See §5.1.

---

## 5. Taxation — Both Directions

### 5.1 Outbound — Tribute to Rome or the Provincial Administration

A recurring expense, the **Tributum**, scaled to the settlement's declared land value and production — Estate & Settlement's own building footprint and Resources & Goods' production totals are the natural inputs. Paid whether the player likes it or not; missing a payment doesn't just cost denarii-in-arrears, it damages standing specifically *with Rome* — the Reputation Duality axis (§6.21) frontier settings already track separately from local standing, giving a missed Tributum payment a distinct consequence from a merely poor local reputation. In frontier or newly-annexed settings, an irregular **Stipendium**-style tribute (historically the tribute a conquered people paid, distinct from a citizen province's Tributum) is the flavor-appropriate variant, tying into Diplomacy with Non-Roman Peoples (§6.25) where the paying party isn't fully Roman to begin with.

### 5.2 Inbound — Levying Tax on the Player's Own Settlement

Gated behind holding a qualifying local office — mechanically, behind the Curia (Buildings §4.10), narratively, behind whatever magistracy Politics & Patronage eventually names as the office that carries this authority. Until that system exists, the Curia itself is the placeholder gate: a player who's built one and (once Politics & Patronage is designed) won the relevant office can set a **Tax Policy**, a standing instruction in the same family as Labor & Slavery's Regimen and Resources & Goods' livestock policy:

- **Vectigalia-style indirect tax** — a toll rate on Commerce transaction volume (Market/Emporium/Customs House), the least visible to individual pop groups since it's assessed on trade flow rather than a person directly.
- **Decuma-style direct tax** — a percentage levy on Coloni harvest and/or Insulae/Domus rent (§3.1), layered on top of the rent the player already collects as landlord — a real, felt double-take from the same population, which is the point: it's more lucrative and more resented than rent alone.

Either setting is a real tradeoff, not a pure win: higher rates mean more Treasury income now, at the cost of Contentment across the taxed pop groups (Settlement Demographics §6) and elevated Emigration pressure (§8.2 of that doc) — the same neglect-has-consequences shape every other automated system in this project already uses. This is also the natural home, once Politics & Patronage exists, for a **Publicanus**-style contract: Rome (or the provincial governor) occasionally offers the player the right to collect a *region's* tribute on its behalf for a cut — a real, attested Roman practice, and a natural bridge between this document's tax mechanics and that system's political ones. Left unresolved for now (see §13) rather than designed in full, since it depends on machinery that doesn't exist yet.

---

## 6. Debt & Lending

### 6.1 Borrowing

The Argentaria/Argentarius (Buildings §4.8, Companions & Court Positions' banking role) is the concrete building and operator behind every loan the player takes: construction shortfalls, disaster recovery (Natural Disasters §6.17), a bad harvest, or simply outpacing the Treasury's Reserve Threshold. A loan is principal plus an accruing interest rate, tracked on the Ledger as a standing `DebtRecord` rather than a one-time transaction — visible, ongoing, and capable of compounding if left unpaid.

### 6.2 Lending

The player can also be the lender, not just the borrower — issuing loans to clients, freedmen, or Curiales-tier settlement residents. Two distinct postures, both real:

- **Patronage lending** — favorable terms extended to a client or freedman, functioning less as an income stream than as a *Clientela* investment: the loan itself is the leverage, repaid in loyalty and obligation as much as denarii, a natural future hook for Politics & Patronage's patron-client mechanics.
- **Commercial lending** — ordinary interest-bearing loans to anyone else, a genuine income stream, with the same default exposure in reverse: a defaulted borrower owes the player, not the other way around, and becomes a Legal & Court plaintiff's opportunity rather than a liability — and, per §6.4, a genuine acquisition opportunity.

### 6.3 Default — When the Household Owes

Per the decision to give this real teeth: a missed payment doesn't just sit as a growing negative number.

1. **Interest escalation** — a missed payment increases the rate or adds a penalty amount, the mildest and first-stage consequence.
2. **Legal exposure** — sustained default gives the creditor (the Argentarius acting on the player's behalf, or a rival lender if the player is the debtor) standing to bring a Legal & Court case (§6.16) — a real dispute with a ruling, not an automatic penalty.
3. **Asset seizure** — a ruling (or, short of a full case, a sufficiently severe default on its own) can result in seizure of specific goods, a plot, or a building — the household's own assets converted directly to debt repayment.
4. **Debt bondage for free household members** — the sharpest consequence: a Peregrine, Latin-Rights, or Freedman member of the player's own household (never a full Roman Citizen — historically accurate, since formal debt-slavery for citizens was long abolished by this era) can be bonded to the creditor as debt repayment if the household's own default is severe enough and a court so rules. This is a deliberately harsh, rare, worst-case outcome, not a routine one — consistent with the frank-but-not-gratuitous tone Labor & Slavery already established for its own subject matter.

### 6.4 Default — When Others Owe the Player

The mirror image of §6.3, and now designed with the same real teeth rather than stopping at "becomes a plaintiff's opportunity." When a client, a Curiales-tier resident, a freedman outside the household, or anyone else the player has lent to (§6.2) defaults, the player has the same escalating ladder available *as the creditor* — and the top rung is a genuine acquisition path, not just a Legal & Court win:

1. **Interest escalation / renegotiation** — the routine first step, often resolved here without further consequence.
2. **Legal action** — the player, via the Argentarius or directly, brings the case to Legal & Court (§6.16); a ruling in the player's favor establishes the debt as formally uncollected and unlocks the steps below.
3. **Asset seizure** — the debtor's own property (land, goods, a building) is forfeit to the player first, where the debtor has any.
4. **Debt bondage — the acquisition itself.** Where seizable assets don't cover the debt, or the debtor has none, a defaulted debtor of Peregrine, Latin-Rights, or Freedman status can be bonded directly into the player's household as an enslaved laborer — this is exactly Labor & Slavery's Debt Bondage acquisition avenue, now given its concrete trigger condition rather than an unexplained starting option. The bonded individual enters play as a new Non-Household Enslaved record or, if the player chooses to track them individually, is promoted straight into Familia (§11 of Settlement Demographics) as a fresh acquisition — the same promotion trigger already used for a direct Slave Market purchase. **Historically real and worth naming directly:** where a whole family's debt is at issue and the law/setting allows it, more than one member of the debtor's household can be bonded to satisfy a single debt — a spouse, children, or dependents alongside the principal debtor — mirroring how debt bondage historically fell on households, not just individuals. This is the harsher end of an already-harsh mechanic and should surface with the same frank, non-exploitative narrative tone Labor & Slavery established, not softened because the player is now the beneficiary rather than the one exposed.

Symmetry matters here: whether the player is creditor or debtor, the eligibility rule is the same (no Roman Citizen can be bonded either direction) and the ladder is the same shape. What changes is only which side of the transaction the player is on — a deliberate design choice so debt reads as a real, two-way structural feature of the setting rather than a one-directional punishment the player alone risks.

### 6.5 Other Roads Into Debt Bondage

Argentaria lending is the cleanest path into §6.4, but not the only one worth naming — a Peregrine, Latin-Rights, or Freedman individual can become debt-bondage-eligible through non-loan arrears too, each already tracked elsewhere in this document or Settlement Demographics:

- **Rent arrears** — a Coloni household or urban tenant (§3.1) who falls sufficiently far behind on rent is functionally in the same position as a defaulted borrower, and can follow the same §6.4 ladder, landlord standing in for lender.
- **Tax arrears** — once the player holds inbound tax authority (§5.2), a Decuma-liable resident who can't pay is a second, distinct road into the same mechanic — historically the more common real-world route into debt bondage, arguably more so than private lending.
- **Contract default** — a party who took an advance against a Contracts (§3.2) delivery and failed to deliver is a rarer, more specific third route, worth naming for completeness even if it surfaces less often than the two above.

All three feed the same §6.4 resolution ladder rather than each inventing their own — this document has one debt-bondage mechanism with several doors into it, not four separate ones.

---

## 7. Trade Routes — Mostly the Existing Model, With Real Levers

Per the decision to keep this primarily passive: Resources & Goods §12's full dynamic simulation (supply/demand, seasonality, Disaster/Piracy/War disruption, a shared regional market with Rival Houses) remains the authoritative baseline and is **unchanged** here. This document adds only the treasury-facing layer on top:

- **Route-level risk investment.** At Port/Harbor/Grand Harbor and Emporium tiers, the player can commit denarii toward a specific active trade route to reduce its Piracy & Banditry disruption exposure — an escort, better packing, a bribed customs official — without turning route management into a constant active job.
- **Occasional deliberate route choice.** At the higher Commerce tiers specifically, the player is sometimes offered a genuine choice between named regional routes with different risk/reward shape — a steady, low-margin grain route to a nearby port versus a lucrative, higher-disruption-risk luxury route reaching Silk or Eastern Spices' actual source region. This surfaces occasionally as a real decision, not a constant management layer, consistent with the passive-by-default steer.

### 7.1 Fenus Nauticum — The Maritime Loan

A real, historically-attested Roman financial instrument, and a natural bridge between §6 and this section: a **fenus nauticum** is a loan financing a specific sea voyage, at a much higher interest rate than an ordinary loan, on the condition that the debt is entirely forgiven if the ship is lost at sea. It's the closest thing this setting has to marine insurance, and it gives the higher-risk trade routes above (§7's luxury route) a genuine financing option distinct from just committing Treasury cash outright:

- The player, via the Argentaria, can either **borrow** against a risky voyage this way (spreading the loss if Piracy & Banditry or a storm claims the shipment, at the cost of a steep premium if it arrives safely) or **lend** this way to a Negotiatores-tier merchant undertaking one (a Contracts-adjacent, higher-risk-higher-return alternative to ordinary commercial lending, §6.2).
- Because the whole instrument's point is that loss forgives the debt, a fenus nauticum never feeds §6.4's default ladder — a lost ship isn't a default, it's the contracted outcome. This is worth stating explicitly so it doesn't get conflated with an ordinary defaulted loan.

---

## 8. Estate Valuation & Net Worth

Everything above tracks *flow* — money moving in and out month to month. This section adds the single *stock* figure other systems actually need: an aggregate **Net Worth**, combining Treasury balance, stored-goods value (Resources & Goods' pricing model, applied to current stock rather than a sale), livestock headcount value, outstanding DebtRecords (subtracted if the household owes, added if it's owed), and land/building value (Estate & Settlement's own construction-cost figures, depreciated by neglect the same way upkeep already is). This is the Villa's Grandeur Score's economic counterpart — that score aggregates room tiers and decoration into one prestige figure; Net Worth aggregates the same household's actual wealth into one comparable figure — and the two are deliberately kept separate rather than merged, since a genuinely wealthy household can still choose to live modestly, or vice versa.

Net Worth is read, not spent — it isn't a second currency — but it feeds real decisions elsewhere:

- **Familia's marriage market** — dowry and alliance-value negotiations (Familia §6) can reference the proposing household's actual Net Worth rather than a purely narrative wealth impression.
- **Rival Houses (§6.10, future)** — the natural, legible number for comparing the player's own fortunes against a rival gens's, the way the Villa doc already floats Grandeur comparisons.
- **Succession & Dynasty (§6.9, future)** — inheritance division at a death naturally operates on Net Worth's constituent parts (Treasury, goods, land, debt) rather than needing its own separate wealth model.
- **Dignitas** — a very high or very publicly-known Net Worth can be a soft Dignitas input in its own right, distinct from Grandeur's more visible, decoration-driven prestige.

---

## 9. Insolvency & Financial Ruin

Every other automated system in this project carries a real escalating worst-case for sustained neglect — Settlement Demographics has Emigration, Labor & Slavery has revolt risk, Disease has spread. This document's individual debts already have real teeth (§6.3–6.4), but nothing until now named what happens when the household's overall position — not one debt, the whole Net Worth (§8) — stays deeply negative for a sustained stretch. **Insolvency** is that missing capstone.

A household crosses into Insolvency when Net Worth remains substantially negative for a sustained run of months (unsized per §13's convention), triggering an escalating ladder distinct from, and independent of, §6.3–6.4's per-debt consequences:

1. **Forced liquidation** — goods stockpiles and livestock are sold off automatically, beyond the ordinary overflow-sale Resources & Goods already runs, at a worse-than-Market rate.
2. **Forced asset sale** — specific plots or buildings are sold or demolished for partial value, the involuntary counterpart to Estate & Settlement's own voluntary demolition/repurposing option.
3. **Villa stage demotion** — where upkeep genuinely can't be sustained, the Villa's own stage (Domus → Urbana → Rustica) can be forced backward, the Grandeur Score's stage-advancement gate running in reverse for the first time.
4. **Loss of qualifying office or census standing** — once Politics & Patronage exists: Rome's real historical wealth qualifications for the Senate and equestrian order mean a sufficiently ruined house can be stripped of a held magistracy or Curia standing, not merely denied a new one.
5. **A Dynasty Chronicle entry** — "The Fall of the House" is exactly the kind of milestone the Dynasty Chronicle (§6.11) is built to record, given the same narrative weight as a death or a scandal rather than treated as a purely mechanical state.

**Deliberately kept separate from §6.3–6.4's debt bondage.** Insolvency's ladder is asset- and standing-focused; it does not, on its own, put any household member's personal freedom at risk. Debt bondage stays tied specifically to a named, defaulted `DebtRecord` and the court process that decides it — conflating the two would let a purely aggregate financial state (which the player might reach through no single bad decision) carry the same personal stakes as an actual defaulted loan, which is a sharper, more deliberate mechanic earned through §6's own specific triggers.

**Not an ending.** Consistent with the core design pillar that the game "never declares the player has won" — the same holds in reverse. Insolvency is a real, painful state, not a game-over: every lever that got the household into it (Rents, Contracts, Tax Policy, borrowing, even a well-timed Windfall) remains available to claw back out, and a genuine "rebuilt from ruin" arc is exactly the kind of Chronicle-worthy story this project's memory-has-weight pillar is meant to support.

---

## 10. The Ledger & the Monthly Report

Every category above — rents, contracts, wages, bribes, tribute, tax revenue, loan interest, route investment — posts as a discrete line to a monthly **Ledger**, surfaced through the Monthly Report the same way every other automated system in this project reports its results. The player's real point of contact is reading that report and adjusting standing policy (Reserve Threshold, Tax Policy, Regimen, wage levels) rather than approving individual transactions — the Automation Principle Resources & Goods and Settlement Demographics both already established, applied here to money specifically.

---

## 11. Cross-System Integration

- **Resources & Goods:** §11–12's currency, pricing, and market-dynamics model is this document's unchanged foundation; goods-sale income continues to post through Commerce buildings exactly as that doc specifies; debasement (§3.5) is this document's one deliberate exception to "unchanged," feeding back into that model as a real inflationary shock.
- **Estate & Settlement:** building upkeep (§4 of that doc) posts as this document's baseline recurring expense; mining/quarrying concessions (§3.2) offer a lighter-weight alternative to that doc's own Industry chains; forced asset sale (§9) is the involuntary counterpart to that doc's own voluntary demolition/repurposing option.
- **Buildings (Production Chains) doc:** the Mint/Moneta finally gets an income mechanism (§3.5), closing a gap that building has carried since that document's own pass.
- **Settlement Demographics:** Rents (§3.1) and Tax Policy (§5.2) are the missing income mechanism that pop group's presence always implied; rent and tax arrears are now also named acquisition triggers (§6.5), giving that document's Curiales/Coloni cohorts a real, if rare, path into the player's own household; Tax Policy's Contentment/Emigration cost reuses that doc's own consequence machinery directly.
- **Labor & Slavery:** Debt Bondage's acquisition-avenue framing gets both a mirror-image exposure risk (§6.3) and its actual acquisition trigger conditions (§6.4–6.5) here; the Argentaria/Argentarius is shared infrastructure between both documents; a Slave Market purchase is now a named Capital Expenditure line item (§4.4).
- **Familia:** promoting a debt-bondage acquisition (§6.4) straight into a tracked record reuses §7/§11's promotion-into-Familia rule directly; Net Worth (§8) gives the marriage market's dowry/alliance-value negotiations a real number to reference; Windfalls (§3.4) is where a received dowry actually posts.
- **Villa (interior design doc):** a stage upgrade's material cost is a named Capital Expenditure line (§4.4); Insolvency (§9) is the first mechanic to ever run that document's own stage-advancement gate in reverse.
- **Companions & Court Positions:** the Argentarius, Institor Maximus, and Rationalis (§5.2–5.4 of that doc) are this document's named operators for lending, contracts, fenus nauticum arrangements (§7.1), and the whole economic cluster's capstone bonus respectively.
- **Legal & Court (§6.16, future):** loan disputes and default rulings (§6.3–6.4) are this document's concrete contribution to that system's eventual caseload, in both directions; a reckless debasement (§3.5) is a plausible future case of its own.
- **Politics & Patronage (§6.5, future):** §5.2's tax-levying authority and §6.2's patronage lending are both explicitly gated on or feeding into that system once it exists; the Publicanus tax-farming contract is flagged rather than designed for the same reason; Insolvency's office/census-standing loss (§9) is this document's sharpest forward hook into that system.
- **Policies & Edicts (§6.12, future):** Tax Policy and the Reserve Threshold are both standing-instruction mechanics of the exact shape that system will eventually formalize; Funded Actions (§4.3) is this document's placeholder for that system's own "games, festivals, public works" spending.
- **Games & Spectacle (§6.22, future) / Religion (§6.6):** Funded Actions (§4.3) gives Ludi and festival funding a Ledger line ahead of either system's own design pass.
- **Reputation Duality (§6.21) / Diplomacy with Non-Roman Peoples (§6.25):** Tributum vs. Stipendium (§5.1) is this document's concrete hook into the Rome-standing-vs-local-standing split those systems track; debasement's political consequence (§3.5) is a second hook into the same axis.
- **Military & Combat (§6.7, future) / Piracy & Banditry (§6.24):** military supply contracts (§3.2), war spoils (§3.4), and trade-route risk investment/fenus nauticum (§7) are this document's forward hooks into both.
- **Natural Disasters (§6.17):** disaster recovery is a named, recurring reason a household turns to borrowing (§6.1); a fenus nauticum-financed shipment lost to a storm (§7.1) is the one loan outcome that deliberately never becomes a default.
- **Succession & Dynasty (§6.9, future):** inheritance (§3.4) and Net Worth's constituent breakdown (§8) are this document's concrete contribution to that system's eventual death/inheritance mechanics.
- **Rival Houses / Living World (§6.10, future):** Net Worth (§8) is the legible comparison figure that system will need to make rival fortunes readable without micromanagement.
- **Dynasty Chronicle (§6.11, future):** a Fall of the House entry (§9) is this document's own contribution to that system's eventual milestone catalog, the financial-ruin counterpart to a death or scandal.

---

## 12. Data Model

```
Treasury {
  settlementId,
  balance,                    // denarii; can run negative
  reserveThreshold,           // standing policy — triggers liquidation/borrowing below this line
}

LedgerEntry {
  settlementId,
  month,
  category,        // "goodsSale" | "rentAgricultural" | "rentUrban" | "contractMilitary" |
                    // "contractConcession" | "contractProvincial" | "taxRevenue" | "windfall" |
                    // "seigniorage" | "wages" | "bribe" | "fundedAction" | "capitalExpenditure" |
                    // "tributum" | "loanInterestPaid" | "loanInterestReceived" | "upkeep" | "routeInvestment"
  amount,           // positive (income) or negative (expense)
  sourceOrTarget,   // personId, popGroupId, buildingId, or "rome"/"province" as applicable
}

CapitalExpenditure {       // §4.4 — one-off, discrete large purchases distinct from recurring Wages/Upkeep
  settlementId,
  month,
  type,             // "slaveMarketPurchase" | "landParcel" | "villaStageUpgrade" | "livestockPurchase" | "other"
  amount,
  linkedRecordId,   // the acquired personId, plotId, or livestock lot, for cross-reference
}

MintPolicy {          // §3.5 — only relevant once the Mint/Moneta is built
  settlementId,
  seigniorageRate,        // steady recurring income
  debasementActive: bool, // the deliberate, rare-use lever
  debasementSeverity,      // scales both the one-time Treasury gain and the market/political consequences
}

FundedAction {         // §4.3
  settlementId,
  month,
  type,             // "ludi" | "festival" | "publicWorks"
  amount,
  dignitasOrFavorGained,
}

DebtRecord {
  debtId,
  settlementId,
  lenderIsPlayer: bool,        // false = player borrowed; true = player lent
  counterparty,                 // "argentaria" | personId | rivalHouseId
  origin,           // "loan" | "rentArrears" | "taxArrears" | "contractAdvance" — §6.5's several doors
  principal,
  interestRate,
  isFenusNauticum: bool,       // §7.1 — if true, loss-of-shipment forgives the debt instead of triggering default
  monthsOverdue,
  status,           // "current" | "overdue" | "inLegalDispute" | "defaulted" | "forgiven" (fenus nauticum loss)
  resolution,       // once defaulted: "renegotiated" | "assetSeizure" | "debtBondage" | null
  bondedPersonIds: [ personId, ... ]   // §6.4 — populated only on a debtBondage resolution; can exceed one person per household debt
}

WindfallEvent {
  settlementId,
  month,
  type,             // "warSpoils" | "dowryReceived" | "inheritance" | "treasureFind"
  amount,
  sourceEventOrPersonId,
}

NetWorth {              // §8 — computed/read, not a spendable balance
  settlementId,
  treasuryBalance,
  storedGoodsValue,
  livestockValue,
  landAndBuildingValue,
  netOutstandingDebt,    // owed-to minus owed-by, from active DebtRecords
  total,
}

InsolvencyState {       // §9 — distinct from any single DebtRecord's own status
  settlementId,
  monthsBelowThreshold,       // how long Net Worth has been substantially negative
  stage,            // "solvent" | "atRisk" | "insolvent" | "ruined"
  consequencesApplied: [ "forcedLiquidation" | "forcedAssetSale" | "villaStageDemotion" |
                         "officeOrCensusLoss" | "chronicleEntry" ],
}

TaxPolicy {
  settlementId,
  vectigaliaRate,      // indirect, on Commerce transaction volume
  decumaRate,           // direct, on Coloni harvest / urban rent
  requiresOffice: bool, // gated on Curia + (eventually) Politics & Patronage's magistracy
}

TradeRouteInvestment {
  routeId,
  settlementId,
  denariiCommitted,     // reduces this route's disruption exposure
  riskProfile,          // "steady" | "highRiskHighMargin" — for the occasional deliberate-choice routes (§7)
}
```

---

## 13. Open Questions

- **All numeric sizing.** Consistent with this project's established convention: interest rates, wage scales, rent rates, tribute formulas, and tax-rate-to-Contentment-penalty curves are all unsized.
- **Publicanus tax-farming contracts.** §5.2 flags this as a real, historically-attested mechanic and a natural bridge to Politics & Patronage, but deliberately doesn't design it now, since it depends on that system's cursus honorum/provincial administration machinery.
- **Multi-settlement treasury consolidation.** §2 notes a second settlement's Procurator runs a genuinely separate treasury; whether a sufficiently senior Rationalis or a later game-stage should ever let the player view or manage both as one consolidated ledger isn't decided.
- **Legal-dispute-vs-automatic threshold for asset seizure.** §6.3 leaves open whether severe default can trigger seizure directly or always requires a Legal & Court ruling first — depends partly on how procedurally deep that system ends up being, itself still an open question on the core document.
- **Debt bondage's exact severity threshold.** §6.3's sharpest consequence is deliberately framed as rare; the actual default-severity threshold that makes it available as a court outcome (versus asset seizure alone) isn't specified.
- **Wage-vs-Regimen crossover for freedmen.** Wages (§4.1) and Regimen tiers (Labor & Slavery) are framed as parallel systems for free and enslaved labor respectively; whether a Freedman still bound by *obsequium* to the player sits closer to one model or genuinely blends both isn't resolved.
- **Family-bondage scope.** §6.4 allows more than one household member to be bonded against a single debt; the actual scope rule (capped at the debtor plus one dependent? uncapped, subject to the total owed? a Legal & Court ruling decides case by case?) isn't specified.
- **Rent/tax arrears threshold before §6.4 applies.** §6.5 establishes that sustained rent or tax non-payment can lead into the same debt-bondage ladder as a defaulted loan, but not how many months overdue, or what floor of severity, actually qualifies.
- **Fenus nauticum premium sizing.** §7.1 establishes the instrument's shape (high interest, full forgiveness on loss) but not the actual rate, nor what fraction of a shipment's declared value it can cover.
- **Net Worth depreciation formula.** §8 notes land/building value depreciates by neglect "the same way upkeep already is," but the actual curve isn't specified, consistent with this project's numbers-later convention.
- **Insolvency's actual trigger threshold.** §9 says Net Worth "remains substantially negative for a sustained run of months" without sizing either the depth or the duration — deliberately left to a numeric-balancing pass.
- **Villa stage demotion's reversibility.** §9 establishes that Insolvency can force a Villa stage backward; whether recovering solvency later restores the stage automatically, requires the same cost/Grandeur gate a forward advance does, or permanently loses whatever was there isn't decided.
- **Debasement's actual mechanism.** §3.5 establishes debasement as a lever with real market and political consequences, but the specific relationship between debasement severity, the one-time Treasury gain, and how long the market-price consequence persists isn't specified.
- **Capital Expenditure vs. ordinary goods purchase, exact boundary.** §4.4 names Slave Market purchases, land, Villa upgrades, and "significant" livestock purchases as Capital Expenditure; where the line sits between a "significant" livestock purchase and an ordinary Resources & Goods transaction isn't drawn precisely.
