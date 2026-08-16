# About Gens

## One line

*Gens* is a text-and-visual Roman household management sim: build a *familia*, grow a *villa* into a settlement or a dynasty, and play across one lifetime or ten generations, with no fixed ending.

## Elevator pitch

You are the pater- or materfamilias of a Roman household during the Pax Romana (or an adjacent era of your choosing). You start with a modest holding — a plot of land, a handful of family members, a few slaves and clients — and grow it however you see fit: as a mercantile empire, a military dynasty, a political powerhouse chasing the Senate, a pious house building temples in its own name, or a quiet agrarian estate that simply endures.

Every person in your household — family, slave, freedman, client, companion — is a full character with stats, traits, loyalty, and their own arc, never a resource line. Rival houses pursue the same land, offices, marriages, and prestige on their own initiative, whether or not you engage them. Laws, edicts, and standing household policy are lasting choices with real tradeoffs, not one-off prompts. Slavery and political brutality are simulated with genuine mechanical stakes and an unflinching narrative tone — the same spirit *Free Cities* brings to its own subject matter — but *Gens* is not a game about sex the way *Free Cities* is: romance and seduction are systemized fully as political and relational mechanics, and their sexual dimension is always handled indirectly, the way a serious historical drama would.

There is no victory screen. A playthrough succeeds or fails entirely on the terms the player sets for themselves.

## Genre and lineage

*Gens* borrows its interface skeleton directly from *Free Cities* — a hub-and-submenu structure, deep per-character stat sheets, recurring stat-weighted random events, slow accretive growth from a small starting holding — and remaps it onto Roman household life. Mechanically it also draws on *Crusader Kings*' dynastic play (succession drama, adoption, contested inheritance) and *Total War*'s building-chain economy (agrarian, mercantile, industrial, and martial identities as equally valid paths), without adopting either game's map-and-battle core.

## What you actually do

- **Run a household.** Assign labor, arrange marriages, raise children, manage slaves and freedmen, appoint companions and court officers, and watch relationships, loyalty, and ambition play out among named people you actually know.
- **Grow an estate.** Build production chains — agriculture, industry, commerce, civic, military — and expand physical landholding from a single villa toward a *vicus* and, eventually, a town or city.
- **Play politics.** Cultivate patrons and clients, stand for local office, chase the distant *cursus honorum*, manage Dignitas and individual relationships, and navigate law courts, scandal, and espionage.
- **Practice governance.** Set standing policy on taxation, recruitment, slave treatment, and religious rites; issue one-off edicts; fund games, festivals, and public works — each a lasting, revisable choice rather than a single prompt.
- **Reach beyond the villa gate.** Travel to Rome, a provincial capital, a rival's estate, or the frontier; correspond by letter when travel isn't worth the risk; build a network of companions, spies, and allies across the map.
- **Build a dynasty.** Choose your own succession, use adoption as a real political tool the way Augustus did, and read your household's own history back as a first-class, illuminated Chronicle — a game about lineage that lets you actually read your lineage.

## Setting

The default era is the **Pax Romana**, chosen for the stability it offers a settlement-building game, but the region/era pairing is deliberately flexible enough to shift earlier (late-Republic frontier colonization) or later (the early Dominate) without breaking core systems. The design corpus's historical range in practice runs from the *Gracchi* (133 BC) through Justinian's reconquests (to roughly AD 565), giving the game a genuinely long usable historical window rather than a single frozen decade.

At launch, the player chooses a **starting region**, each with its own land quality, local politics, and risk profile — the Italian heartland, the Gallic frontier, an Iberian or North African colony, or the Greek East — with an extensible slate (Egypt, Syria/the Levant, Britannia, Anatolia, the Balkans, and further individual regions: the Alpine provinces, Armenia, Mesopotamia, Nubia, Arabia Felix, the Bosporan Kingdom, Sicily) reaching well past the four launch regions. Frontier starts carry a distinct **Reputation Duality** — standing with Rome and standing with the surrounding populace pull in different directions — and open onto diplomacy with non-Roman neighbors, including the possibility of an alliance against Rome itself.

## Design pillars

1. **Deep individuals** — every household member is a full stat-and-trait character, never a resource line.
2. **Open-ended scope** — no fixed win condition or generational cap; a playthrough might end in one lifetime or sprawl across ten generations.
3. **Total economic and architectural freedom** — agrarian, mercantile, industrial, and martial identities are all equally viable.
4. **Frank harshness, thematic honesty** — slavery and political brutality carry real weight, entirely without sexual content as the mechanism of that harshness.
5. **Player as protagonist, not just administrator** — the player directly controls one character while governing everyone else by policy; compliance depends on loyalty, personality, and standing, not player fiat.
6. **A living world** — rival houses pursue land, offices, marriages, and prestige on their own initiative.
7. **Memory has weight** — the dynasty's own history is a first-class, readable feature.
8. **Governance through policy, not just clicks** — laws, edicts, and standing policy are lasting, revisable choices with real tradeoffs.
9. **The world beyond the villa gate is reachable** — travel, court appointments, and personal relationships extend play into a wider social and geographic world.

## Tone and content guidelines

- Slavery, violence, and political brutality are simulated with real mechanical stakes and unflinching narrative tone.
- Sexual content is never a mechanic or a focus — romance and seduction are fully systemized on their political and relational dimensions, but the sexual dimension of any relationship is always implied or faded to black, never depicted or given its own mechanical resolution.
- No forced ending — self-set objectives and an optional milestone catalog give structure without imposing a stop condition.
- Harsh systems (punishment, legal authority over life and death, warfare) are played straight, described with narrative purpose rather than for shock value.

## Visual identity

The interface is designed to feel like paging through an actual Roman household's own records — wax tablets, painted ledgers, inscribed stone — rather than a conventional strategy-game skin. Screens are organized around a wax-tablet **diptych** layout with a persistent ink-bar spine; a seven-color palette (papyrus, iron-gall ink, Tyrian purple, terracotta oxide, verdigris bronze, gold leaf, and a crisis-only blood oxide) carries consistent meaning across every screen; consequential decisions are confirmed with a signature **wax-seal** interaction; and the Dynasty Chronicle — the household's readable history — is the most ornate screen in the game, presented as a single unfurling illuminated scroll.

## The scale of the world

*Gens* is backed by an unusually large authored-content corpus. As catalogued in the [Canonical Object & Data Registry](../gens-canonical-registry-design.md), the design specifies (among other things):

- 35 named Cultures and 27 named Religions/Faiths across the Known World, plus the full Roman pantheon of patron deities;
- 18 Starting Regions (6 launch, 5 promoted, 7 further individual region documents);
- 94 building types, 55 court/companion positions, 144 tradeable goods, 219 named character Traits, 119 named occupations;
- 105 named plants and 60 named non-legendary creatures (plus 17 confirmed legendary creatures), 94 named technologies/discoveries across six historical eras, and 52 named real historical figures woven into 121 dated historical events spanning 133 BC to AD 565.

That content sits on top of 110 individual system design documents (roughly 384,000 words), reconciled into a single ownership map by the [Design Authority Registry](../gens-design-authority-registry.md), which resolves 31 clusters of overlapping documents down to one authoritative source per shared concept — the record implementation is meant to read from.

## Current status

*Gens* is in early development. The supported toolchain — Unity 6.3 LTS with an engine-independent, deterministic C# simulation core — is established and recorded in [`docs/engineering/tech-stack.md`](../engineering/tech-stack.md). The deterministic simulation package (partitioned `WorldState`, phased monthly ticks, a command/event envelope, named and persistable PCG32 random streams, canonical `.gens` save serialization with migrations, and a headless campaign bootstrap) is implemented, and on top of it characters and Familia households (lifecycle, traits, relationships, roles) and land/goods/buildings/villas/labor with a production network are implemented and covered by headless exit-gate soak tests, backed by real typed content families in `content/source/` validated against `content/schemas/`. Population groups, the ledger/market, and the player-facing Unity loop are not started. The [Comprehensive Build Roadmap](../engineering/gens-comprehensive-build-roadmap.md) defines the engineering construction order from the current state to a simulation-complete release; the [Feature Roadmap](gens-feature-roadmap.md) defines the corresponding order in which player-facing features and content should come online.

## Glossary

- **Gens** — a Roman clan or family line; the game's own title and unit of long-term progress.
- **Familia** — the full household in the Roman sense: family, slaves, freedmen, and clients together, not just blood relations.
- **Pater/Materfamilias** — the legal and social head of the household.
- **Patria Potestas** — a household head's formal legal authority over descendants, including marriage approval, disownment, and, in the harshest historical cases, life-and-death authority.
- **Dignitas** — public standing/prestige, the game's primary reputation stat.
- **Cursus Honorum** — the traditional sequence of public offices a Roman political career ascends through.
- **Vicus** — a village-scale settlement; the intermediate stage between a single villa and a full town or city.
- **Clientela** — the patron-client relationship structure underlying much of Roman social and political life.
- **Manumission** — the formal act of freeing a slave.

*Full detail on every system, term, and mechanic lives in the [game design index](README.md); this page is the pitch, not the specification.*
