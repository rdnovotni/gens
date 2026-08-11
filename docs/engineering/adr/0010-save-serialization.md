# ADR 0010 — Save Serialization

**Status:** Proposed

## Context

`tech-stack.md` already commits to the save file's outer shape: "Save files use the `.gens` extension and are atomic ZIP containers with `manifest.json`, `world.json`, optional `history.json`, generated-asset references, an explicit version, and all deterministic RNG states. Breaking changes require a migration and a permanent fixture." Roadmap Phase 3, items 1–2 ask specifically for "canonical JSON serialization rules and stable ordering" and "atomic `.gens` archive writing/reading, manifest checksums, world/history entries, RNG states, content-pack hashes, and generated-asset references." Phase 2's exit gate requires that "saving/loading at any monthly boundary does not change the result" — i.e., a save/load round trip must be lossless and must not perturb the deterministic hash Phase 2 establishes.

The code already has the constants and manifest shape this ADR must fill in behavior for: `src/Gens.Simulation/Saves/SaveFormat.cs` defines `CurrentVersion = 1`, `Extension = ".gens"`, `ManifestEntry = "manifest.json"`, `WorldEntry = "world.json"`, `HistoryEntry = "history.json"`, and `SaveManifest(int SaveFormatVersion, string GameVersion, IReadOnlyDictionary<string, Pcg32State> RandomStreams, IReadOnlyList<string> GeneratedAssetReferences)` — every field `tech-stack.md` names is already present as a record shape, but nothing yet writes or reads a ZIP, computes a checksum, or defines what "canonical JSON" means byte-for-byte.

## Decision

- **Canonical JSON:** every object's properties serialize in a fixed, explicit order (never reflection/declaration order, per ADR 0004's ordering discipline applied to serialization) — collections use ADR 0001/0004's `RuntimeId`-ascending order, object properties use an explicit `[JsonPropertyOrder]` per DTO. `System.Text.Json` with `JsonSerializerOptions` pinned to `CultureInfo.InvariantCulture`, no indentation in the persisted bytes (a separate `--pretty` debug-export path may reformat for human inspection, but that is not the saved artifact), UTF-8 without BOM, LF line endings. `Fixed64` (ADR 0002) serializes as its raw scaled `long`, never as a decimal string, to avoid a lossy round trip through text formatting.
- **Archive structure:** the `.gens` file is a ZIP (matching `tech-stack.md`), containing `manifest.json`, `world.json`, optional `history.json`, and generated-asset references — exactly `SaveFormat`'s existing entry names, no new entries introduced without a corresponding `SaveFormat` constant.
- **Atomicity:** the archive is built at a temporary path in the same directory as the final target, fully written and flushed, then moved into place with a single atomic rename — never written in place over an existing save. A save that fails partway through never leaves a corrupted or partial `.gens` file at the target path.
- **Checksums:** `manifest.json` records a SHA-256 checksum for each other entry in the archive (`world.json`, `history.json`, any generated-asset reference), computed over the canonical bytes before compression. Load verifies every checksum before touching gameplay state; a mismatch is a hard load failure, never a "best effort" partial load.
- **RNG states:** `SaveManifest.RandomStreams` already matches `RandomStreamSet.CaptureStates()`'s exact shape (`IReadOnlyDictionary<string, Pcg32State>`, already ordinally sorted per ADR 0004) — save simply persists that dictionary as-is; load calls `RandomStreamSet.Restore(...)`, both already implemented in `src/Gens.Simulation/Random/RandomStreamSet.cs`.
- **Content-pack hashes:** `manifest.json` additionally records the loaded content pack's own manifest hash (ADR 0012), so a save can detect at load time that it was authored against different content than is currently installed, before that mismatch causes a confusing runtime reference failure.

## Consequences

- Because canonical JSON is byte-stable for identical state, two saves of the same state (e.g., immediately re-saving without advancing) produce byte-identical `world.json` — a strong, cheap regression test ("save, load, re-save, diff bytes, expect zero diff") becomes available from day one.
- The checksum-verify-before-load step turns silent corruption (a truncated copy, a disk error) into an explicit, actionable load failure rather than a confusing later crash mid-campaign.
- `SaveManifest`'s existing shape needs no breaking change to satisfy this ADR — it already carries every field `tech-stack.md` names; this ADR is about the write/read/verify behavior around it, not the record shape itself.

## Alternatives Considered

- **Binary serialization (e.g., a custom binary format or protobuf) instead of JSON.** Rejected: `tech-stack.md` already commits to `manifest.json`/`world.json` as JSON entries specifically, and JSON's human-diffability materially helps debugging deterministic-replay divergence (a golden-fixture diff is readable directly) — a benefit worth its size/parse-speed cost at this project's scale.
- **In-place save overwrite with a `.bak` fallback instead of atomic temp-then-rename.** Rejected: a `.bak` scheme still has a window where neither file is guaranteed valid (mid-write crash during the primary write, before the swap); atomic rename has no such window on any of this project's target filesystems.
- **Skipping per-entry checksums and relying on ZIP's own CRC32.** Rejected: ZIP's CRC32 catches accidental corruption but is not a security-grade integrity check and does not, by itself, let `manifest.json` assert "this is what `world.json` looked like when written" independent of the ZIP container — SHA-256 in the manifest is a stronger, format-independent guarantee and doubles as the migration-fixture comparison mechanism (ADR 0011).
