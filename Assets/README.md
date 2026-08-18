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
stylesheet imports. `GensUIController` needs its `UIDocument` and the six
`VisualTreeAsset` fields (the five screens/ink-bar plus
`ConfirmationDialog.uxml`) wired in the Inspector on the scene GameObject
that also carries `CampaignShellBehaviour`.

The same controller also implements roadmap Phase 9 items 7-8 on top of
item 6's screens:

- **Wax-seal/ordinary confirmation** (item 7): `ConfirmationDialog.uxml`/`.uss`
  is a floating tablet modal over a dimmed background (`gens-core-design.md`
  §7.4's event-modal shape). Its severity comes straight from
  `com.gens.simulation`'s `ActionDefinition.Confirmation`
  (`ActionConfirmationSeverity`): a `WaxSeal` action shows the circular
  seal-press control (`WaxSeal.uss`, §7.6), an `Ordinary` action shows plain
  Confirm/Cancel buttons. `Scripts/Adapters/ActionConfirmationAdapter.cs`
  translates an `ActionDefinition` plus its non-mutating
  `ProjectResult` preview into the dialog's title/body text.
- **Command submission** (item 8): the estate/settlement screen's two
  household-action buttons (Change Rites Budget, Fund a Festival) run the
  action catalog's eligibility check, show the confirmation dialog, and on
  confirm submit the real command through `CampaignShell.Submit` — the same
  `PolicyActionDefinitions`/`ActionCatalog` pair Phase 9 items 1-2 built.
- **Pause/advance** (item 8): the ink bar's status label, Pause/Resume
  toggle, and wax-seal-styled Advance control. The campaign starts paused;
  Advance calls `CampaignShell.AdvanceMonth` and feeds the result to
  `RefreshInkBar`/`ApplyMonthlyEvents`.
- **Save/load** (item 8): the ink bar's Save/Load controls call the new
  `CampaignShell.Save`/`CampaignShell.Load`, the latter swapping in the
  loaded shell through `CampaignShellBehaviour.ReplaceShell`.
- **Deterministic replay diagnostics** (item 8): the ink bar's Diagnostics
  control runs `CampaignShell.VerifyDeterministicReplay` — save, reload,
  and compare `StateHasher` hashes, mirroring the console runner's
  `replay`/`compare-hashes` verbs — and reports the result through the same
  confirmation-dialog machinery as an informational message.
- **Placeholder/procedural portraits** (item 8): the character detail
  screen's portrait medallion. No composited art exists yet
  (`PortraitRecipe` is deliberately just data), so
  `Scripts/Adapters/PortraitAdapter.cs` derives a deterministic tint and
  monogram from the character's own `CharacterVisualProfile` instead.

