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

