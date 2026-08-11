# ADR 0006 — Command Envelope and Atomicity

**Status:** Proposed

## Context

Non-negotiable rule 2: "One command path. Player actions, AI decisions, steward automation, events, debug tools, and migration repairs use the same validated action/command layer whenever they cause equivalent world changes." Rule 1 reinforces the same seam from the other direction: "Unity displays projections and submits commands. It never owns authoritative campaign state." Phase 2, item 6 spells out the required envelope fields: "command ID, actor, submitted date, causation, validation error code, emitted events, and deterministic sequence number." Phase 2's exit gate states the atomicity requirement in one sentence: "A rejected or failed command leaves state and RNG unchanged."

The code already has a minimal version of this to extend, not replace: `src/Gens.Simulation/Commands/CommandPipeline.cs` defines `interface ICommand`, `interface IDomainEvent` (both empty marker interfaces today), a `CommandResult(bool Accepted, IReadOnlyList<IDomainEvent> Events, string? Error)`, and `CommandPipeline<TState, TCommand>` that runs a `_validate` function and, only if it returns `null` (no error), an `_mutate` function. This validate-then-mutate shape is already correct in outline — the gap is that `TCommand` itself has no ID, actor, submission date, causation, or sequence number, `CommandResult.Error` is a bare `string` rather than a stable code, and nothing in the pipeline touches RNG at all, meaning nothing currently prevents a validated-but-failing mutation from having already consumed random draws before failing partway through.

The corpus repeatedly assumes exactly this single-path model without designing it: `gens-characters-design.md` §8.3 states NPC-initiated interactions "run through this same Interaction Catalog and the same two resolution layers, on the NPC's own behalf, rather than being a separate scripted system" — the same command path a player action uses. `gens-settlement-demographics-design.md`'s entire "Automation Principle" (§2) — pop groups shifting class, migrating, growing — is background simulation acting through the same mutation surface a player's building order does, never a parallel silent-write path.

## Decision

Extend, rather than replace, the existing `ICommand`/`CommandPipeline` shape:

- **`ICommand` gains required members**: `CommandId` (`RuntimeId<Command>`, ADR 0001), `ActorId` (a character, steward, or a reserved system-actor ID for pure-automation commands — every command has an actor, satisfying rule 2's "player actions, AI decisions, steward automation... use the same... layer"), `SubmittedDate` (`GameDate`, ADR 0003), and `CausationId` (nullable — the event or command ID that produced this one, letting a scheme's monthly progress tick, an NPC's autonomous decision, or a migration repair all carry an honest provenance chain back to their root cause).
- **`CommandResult.Error` becomes a `ValidationErrorCode`** — a stable, versioned enum/string per command family (e.g. `"labor.assignDuty.slotOccupied"`), never a free-text string, so UI (ADR 0013) and AI decision-making can branch on it reliably rather than string-matching.
- **A deterministic `SequenceNumber`** (a campaign-wide monotonic counter, itself campaign state) is assigned at the moment a command is *accepted* into the pipeline (not at submission), giving every executed command a total order independent of wall-clock arrival — the concrete mechanism Phase 2's replay-determinism exit gate depends on.
- **Validation must be pure and RNG-free.** `_validate` may read state but must never call into `RandomStreamSet`. Only `_mutate`, which runs strictly after a successful validation, may consume RNG. This makes atomicity trivial by construction: a rejected command never touched RNG, so "state and RNG unchanged" (the exit-gate wording) holds without needing a transactional rollback mechanism. `_mutate` itself is still expected to be a single, non-partial state transition — a system that would need to leave state partially mutated on internal failure must instead fail during validation.

## Consequences

- Debug tools and migration repairs (rule 2's own examples) issue the same `ICommand` types as the player UI, just with a different `ActorId` — no shadow mutation API is ever introduced for tooling convenience.
- `ValidationErrorCode` becomes de facto public API the UI (ADR 0013) can localize and react to without parsing prose.
- The RNG-free-validation rule interacts directly with ADR 0004: because command execution order is now deterministically sequenced, and validation never perturbs RNG state, replaying the same sequence number stream reproduces identical RNG draws regardless of how many commands were *rejected* along the way — rejected commands are invisible to the RNG timeline entirely.

## Alternatives Considered

- **Full transactional rollback** (snapshot state before mutate, discard on any post-mutate invariant failure) instead of RNG-free validation. Rejected as the primary mechanism: snapshotting `WorldState` every command is expensive at scale and doesn't by itself solve the RNG-perturbation problem (a partially-run mutate could still have drawn from a stream before failing); the RNG-free-validation rule solves the actual failure mode more cheaply. Post-mutate invariant checks (ADR 0005's `InvariantChecks` phase) remain as a second, coarser safety net for whole-tick consistency, not per-command rollback.
- **Free-text error strings kept as-is.** Rejected: blocks any AI decision-making or UI branching on validation failure reasons, and the existing `string? Error` field already signals this was meant as a placeholder, not a final design.
- **A separate command path for "system" (automation-originated) commands versus player commands.** Rejected outright by rule 2's explicit text; the `ActorId` field is the whole mechanism needed to distinguish origin without duplicating the pipeline.
