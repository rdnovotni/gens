# Unity presentation layer

Unity-facing scenes, UI Toolkit documents, rendering configuration, and
presentation adapters belong here. Domain and simulation code belongs in the
engine-independent `com.gens.simulation` local package.

`Scripts/Shell` is the application shell (ADR 0013, roadmap Phase 9 item 5):
`CampaignShell` owns the running campaign's `WorldState`/`RandomStreamSet` and
exposes only query execution and command submission, and
`CampaignShellBehaviour` is the `MonoBehaviour` that bootstraps and owns one
per scene. `Scripts/Adapters` holds the translation layer between simulation
projection DTOs and UI Toolkit-bound view models — the only code permitted to
do that translation per ADR 0013. Neither assembly is referenced by
`com.gens.simulation`; both reference it one-way.

`Scripts/Shell/GensUIController.cs` is the persistent ink bar and four
first-class screens (roadmap Phase 9 item 6): household roster,
estate/settlement, monthly report, and character detail, each backed by a
named query in `com.gens.simulation`'s `Queries` namespace and its own
adapter in `Scripts/Adapters`. `UI/` holds each screen's UXML/USS
(`InkBar.*`, `HouseholdRosterScreen.*`, `EstateSettlementScreen.*`,
`MonthlyReportScreen.*`, `CharacterDetailScreen.*`), plus the shared
`Palette.uss` (the seven-color ink-bar palette, `gens-core-design.md` §7.2)
and `Diptych.uss` (the wax-tablet two-leaf layout, §7.4) every screen's own
stylesheet imports. `GensUIController` needs its `UIDocument` and the five
`VisualTreeAsset` fields wired in the Inspector on the scene GameObject that
also carries `CampaignShellBehaviour`; wax-seal confirmation (item 7) and
the pause/advance UI that feeds the report screen new events each month
(item 8) are later roadmap items this controller exposes a seam for
(`ApplyMonthlyEvents`) but does not itself implement.

