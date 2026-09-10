# ADR 0020 — Save Slot Metadata Storage

**Status:** Accepted

## Context

ADR 0010 and ADR 0011 cover the `.gens` archive format and its migration
mechanism, but neither addresses player-facing save management: naming a
save, browsing multiple saves, seeing when one was last played or how long,
and deleting one. Before this ADR, the native desktop client
(`src/Gens.Client.Desktop`) recognized exactly one save file —
`paths.Quicksave` — with no slots, no metadata, and no browser. Closing that
gap requires storing data ADR 0010 explicitly keeps out of the archive: a
player-editable display name and a wall-clock save timestamp. `SaveWriter`
deliberately pins ZIP entry timestamps to 1980-01-01 so identical campaign
state produces byte-identical archives — a real save timestamp cannot live
inside `manifest.json`/`world.json` without breaking that guarantee, and a
free-text name has no place in a checksummed, canonical, campaign-hash-
affecting document either.

## Decision

Save-slot metadata (display name, last-saved time, playtime) lives in a
single versioned, atomically-written file, `saves/index.json`, owned
entirely by `Gens.Client.Desktop` (`SaveIndexService`,
`src/Gens.Client.Desktop/Saves/`) — **not** inside the `.gens` archive, and
**not** as a per-slot sidecar file next to each save.

- **Outside the archive, not a `SaveManifestDto` extension.** A display name
  and a real timestamp are exactly the non-deterministic, player-editable
  data ADR 0010's canonical-JSON/checksum contract must never carry. Keeping
  them out means `SaveFormat`, `SaveWriter`, `SaveReader`, and the migration
  registry are completely unaffected by this feature — no `SaveFormat`
  version bump, no new migration, no new fixture.
- **One index file, not one sidecar per save.** A sidecar (`<slot>.gens.meta.json`
  next to each `.gens` file) requires two atomic writes per save action with
  no way to make them atomic together, and an orphaned or missing sidecar
  (a save copied in from elsewhere, a crash between the two writes) is a
  routine failure mode a single index avoids by construction.
- **Reconciled against the filesystem on every load**, not trusted blindly.
  `SaveIndexService.Reconcile` scans `paths.Saves` for `*.gens` files:
  metadata for a file it already knows about is kept as-is, a file with no
  entry (a bare pre-existing `quicksave.gens` from before this shipped, or
  any `.gens` file dropped in from elsewhere) gets a synthesized entry from
  the filename and the file's own last-write time, and an entry whose file
  no longer exists is dropped. Exactly one quicksave entry is always
  guaranteed to exist, even if its file is absent, so callers never need a
  separate `File.Exists` check for it.
- **Filenames are always GUID-derived, never the player's typed name**
  (`save-{Guid.NewGuid():N}.gens`). This keeps `DisplayName` free to hold
  arbitrary Unicode and punctuation without ever touching filesystem-
  reserved-name or path-traversal concerns; `IApplicationPaths.ResolveSafePath`
  still guards every path built from an index entry as defense in depth.
- **Same load/atomic-save/corrupt-file-preserved recipe as `SettingsService`**
  (`src/Gens.Client.Desktop/Settings/DesktopSettings.cs`): a `Version` field
  on the index for future schema evolution, `FileStream(..., FileOptions.WriteThrough)`
  + `File.Replace` for atomicity, and a corrupt index file renamed aside
  (never deleted) with safe defaults substituted. CLAUDE.md already states
  this exact shape for settings ("versioned, migrated, atomically written,
  and separate from campaign saves"); this treats save-slot metadata as the
  same category of desktop-owned, non-simulation persisted state.
- **Playtime is desktop-layer only.** It is accumulated in
  `DesktopApplicationController` from wall-clock frame deltas and stored
  only in `saves/index.json`, never in `WorldState`/`CampaignConfig` or the
  archive — `GameDate` is deterministic in-game calendar time, not wall-clock
  playtime, and repurposing it would make campaign state depend on how long
  a player left the window open, which nothing in the simulation may do.

## Consequences

- `Gens.Simulation`, `Gens.Application`, and `Gens.Presentation` are
  untouched by slot bookkeeping: browsing saves is a "which files exist
  under `IApplicationPaths.Saves`" desktop/OS concern, not a simulation
  query, so `CampaignSession`/`IWorldQuery` have nothing to say about it. The
  save browser's own screen model (`SaveBrowserModel`) still lives in
  `Gens.Presentation` as a pure, I/O-free mapping (`SaveBrowserMapper`) from
  a desktop-supplied row shape, keeping the adapter-layer convention intact
  without requiring `Gens.Presentation` to depend on `Gens.Client.Desktop`.
- A corrupted or missing `index.json` never blocks loading an existing
  `.gens` save: reconciliation rebuilds sensible defaults from the saves
  directory itself, so the index is a cache of convenience, not a second
  source of truth a save depends on.
- Every existing single-quicksave install upgrades silently: the first
  `SaveIndexService.Load()` after this ships finds no index, reconciles the
  pre-existing `quicksave.gens` into a `Quicksave`-named slot with unknown
  ("not tracked before this update") playtime, and persists it — no manual
  migration step, no data loss.

## Alternatives Considered

- **Extend `SaveManifestDto` with optional name/timestamp fields.** Rejected
  outright by ADR 0010's canonical-JSON/checksum contract — these fields are
  exactly the non-deterministic, player-editable data that must never affect
  the campaign hash or the archive's byte-for-byte reproducibility.
- **A `.meta.json` sidecar per save file.** Rejected: doubles the atomic-write
  surface per save action with no way to keep both writes atomic together,
  and requires the same directory-reconciliation logic a single index needs
  anyway, for strictly more file I/O and more orphaned-file failure modes.
- **Deriving a synthetic "playtime" from `WorldState.Date` deltas.** Rejected:
  `GameDate` is deterministic in-game calendar time advanced by explicit
  `AdvanceMonth` commands, not wall-clock time; a player who leaves the
  client open mid-month for an hour and one who plays through in five
  minutes must not report the same "playtime," and simulation state must
  never depend on real-world elapsed time regardless.
