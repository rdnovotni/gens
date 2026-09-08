# Native audio system

`Gens.Audio` is presentation-only and has no dependency on Simulation, SDL, or a concrete device API. `AudioEngine` owns semantic buses, active voices, fades, and backend lifetime. Every voice routes through Music, Ambience, Effects, UI, or Voice; Master is a multiplier rather than a playback destination. Effective gain is `voice × bus × Master`, clamped to 0–1. Mute is independent of stored volume.

`AudioClip` exposes duration, channel count, sample rate, buffered/streaming storage, and a stream factory without leaking device handles. `MusicService` owns the single foreground track and crossfades using presentation time. Long music/ambience clips are marked Streaming so a backend can decode incrementally; short effects can be Buffered.

The client composes `SdlAudioBackend`, isolated in `Gens.Platform.Sdl`. It initializes SDL's audio subsystem, opens the default output lazily, and feeds interleaved signed 16-bit PCM through SDL audio streams in bounded chunks; streaming clips reopen their source when looping. Device/subsystem failure returns no backend voice and the engine continues silently. A WAV/OGG decoder/asset loader, default-device change recovery, and packaged audio fixtures remain release blockers. Backend status is surfaced in Settings and package metadata. Audio never gates commands or conveys gameplay-only information.

Settings schema v2 stores Master 100%, Music/Ambience 80%, Effects/UI 100%, and Master mute. Changes update active voices immediately. Automated tests use fake/null backends and need no physical device.

The 2026-09-08 Windows packaged software-renderer sample recorded 0.0 ms process CPU over a three-second settled idle interval. SDL audio opens streams lazily, so this is the no-active-voice client baseline; an active streaming-voice benchmark remains required before release.
