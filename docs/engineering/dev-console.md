# Dev/debug console

A DFHack-style in-game console for inspecting and nudging a running campaign
during playtesting, without stopping to use `tools/Gens.ContentCompiler`.

## Enabling it

1. Settings → "Enable Dev Console" (persisted via `PlayerPrefs`, key
   `DevConsoleController.EnabledPrefKey`).
2. Press `` ` `` (backquote) at any time to open or close it. The toggle is
   polled globally through the Input System's `Keyboard` device, so it works
   regardless of which UI panel currently has focus.

The Settings toggle alone is never enough to open the console: it also
requires `Debug.isDebugBuild || Application.isEditor`
(`DevConsoleController.IsAvailable`). A stray enabled pref left over from a
development build can therefore never surface the console in a shipped
release build.

## Architecture

The console is a second, always-present `UIDocument`
(`DevConsolePanelSettings.asset`, sorted above the main app's) owned by
`DevConsoleController`, which lives alongside `GensAppController` in
`Assets/Scenes/Main.unity` rather than inside either of that controller's or
`GensUIController`'s own documents — so it survives every screen swap
(`root.Clear()`) those controllers do.

Every command reaches the campaign only through the same two entry points
the rest of the UI uses (ADR 0013):

- **Reads** go through `CampaignShell.Query<TProjection>` — see `state`
  (`CampaignDebugQuery`) and `query <name>` (a fixed table of existing
  `IWorldQuery<T>` implementations).
- **Writes** go through `CampaignShell.Submit<TCommand>` — see `submit`,
  which wraps input in a `GenericCommand` exactly like
  `tools/Gens.ContentCompiler`'s `submit-command` verb does.

`DevConsoleController` itself never reads or writes `WorldState` directly;
`DevConsoleLineParser` (pure, Unity-free) splits each typed line into a verb
and arguments, and `DevConsoleCommandRegistry` holds the fixed set of
`IDevConsoleCommand` implementations under
`Assets/Scripts/Shell/DevConsole/Commands/`.

**Commands submitted through the console are real commands.** They consume
a command sequence number and affect the deterministic state hash exactly
like a player action would (ADR 0006) — this is correct, not a gap. It does
mean a session where dev-console commands were used should not be included
in a determinism/replay comparison run, the same as any other
extraordinary input.

## Command reference (v1)

| Verb | Usage | Description |
| --- | --- | --- |
| `help` | `help` | Lists every available command. |
| `state` | `state` | Prints partition sizes, the next command sequence number, and the state hash (`CampaignDebugQuery`). |
| `hash` | `hash` | Prints the state hash alone, for a quick before/after divergence check. |
| `query` | `query <inkBar\|householdRoster\|estateSettlement\|householdFinancials\|characterDetail> [characterId]` | Runs a named `IWorldQuery<T>` and prints the resulting projection record. |
| `submit` | `submit <type> <payloadJson> [actorId]` | Submits a `GenericCommand` through `CommandPipeline` (payload JSON must not contain spaces). Defaults to actor `dev-console`. |
| `clear` | `clear` | Clears the console scrollback. |

The console also mirrors every `Debug.Log`/`Warning`/`Error` into its
scrollback (`Application.logMessageReceived`), so it doubles as a live log
viewer. Up/Down arrows cycle through previously submitted lines.

Adding a new verb means adding a new `IDevConsoleCommand` implementation
under `Commands/` and registering it in `DevConsoleCommandRegistry` — the
set is fixed and reviewed, not reflection-discovered.
