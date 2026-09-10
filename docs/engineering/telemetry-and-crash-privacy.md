# Telemetry, crash reporting, and diagnostics privacy

The native client uses an explicit-consent, local-first observability model. Both anonymous usage telemetry and crash reporting default to `NotAsked`, which behaves as disabled. The first-run prompt offers a clear allow or keep-off choice, and Settings exposes independent controls plus immediate deletion.

No network transport or analytics vendor is configured. Consented data stays in the Gens user-data directory until the player deliberately exports an anonymized diagnostics ZIP. A future upload transport must be separately reviewed and must consume the same allowlisted schema; it may not add arbitrary event properties or identifiers.

## Collected data

Usage telemetry consists only of cumulative integer counters:

- sessions and campaigns started;
- campaigns loaded;
- months advanced;
- commands accepted or rejected;
- saves succeeded or failed;
- loads failed.

Balance instrumentation deliberately has no generic event-property API. It cannot accept campaign seeds, hashes, regions, difficulties, monetary values, character or household identifiers, event text, typed input, or timestamps for individual gameplay actions.

Consented crash reports contain a random per-report ID, UTC occurrence time rounded to the hour, application version, coarse operating-system family, process architecture, failure kind, exception type, and up to 40 managed type/method names. Exception messages, source file names, source line numbers, local paths, environment variables, usernames, machine names, IP addresses, stable device/install identifiers, saves, screenshots, and console logs are excluded.

## Diagnostics exports

Settings → Privacy & diagnostics → **Export anonymized diagnostics** creates a ZIP in the local logs directory. It contains:

- an allowlisted manifest with coarse environment and current runtime counters;
- aggregate balance counters only when usage telemetry is enabled;
- already-scrubbed local crash reports.

The exporter never scans the user-data directory and never includes settings files, saves, generated artwork, cache contents, or console logs. **Delete local diagnostics** removes telemetry counters and crash reports and sets both consent choices to denied.

## Crash UX

Fatal startup/runtime exceptions, unhandled AppDomain exceptions, and unobserved task exceptions pass through the scrubbed crash writer when crash consent is granted. On the next launch, the client reports how many local crash records exist and offers a direct route to Settings for export or deletion. Gameplay remains available and observability failures do not influence deterministic simulation state.
