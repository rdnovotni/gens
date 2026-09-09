# Audio decoder fixtures

- `tone.wav` — a synthetic 0.1s, 48kHz, mono, 16-bit PCM 440Hz sine tone,
  generated for this repository specifically to test `WavDecoder`.
- `fixture.ogg` — the `TestFiles/1test.ogg` fixture from the
  [NVorbis](https://github.com/NVorbis/NVorbis) project (MIT license), reused
  here to test `VorbisDecoder` against a real Ogg Vorbis encoder's output
  without vendoring an encoder into this repository.
