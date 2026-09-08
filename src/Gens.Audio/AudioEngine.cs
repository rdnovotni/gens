namespace Gens.Audio;

public readonly record struct AudioBusState(float Volume, bool Muted)
{
    public AudioBusState Normalize() => new(Math.Clamp(float.IsFinite(Volume) ? Volume : 0, 0, 1), Muted);
}

public sealed class AudioEngine : IDisposable
{
    private readonly IAudioBackend backend;
    private readonly Dictionary<AudioBus, AudioBusState> buses = Enum.GetValues<AudioBus>().ToDictionary(static bus => bus, static _ => new AudioBusState(1, false));
    private readonly List<AudioVoice> voices = [];
    private bool disposed;

    public AudioEngine(IAudioBackend backend) => this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
    public string BackendName => backend.Name;
    public bool IsOutputAvailable => backend.IsAvailable;
    public string? UnavailableReason => backend.UnavailableReason;
    public IReadOnlyList<AudioVoice> ActiveVoices => voices;
    public event Action<AudioBus, AudioBusState>? BusChanged;
    public event Action<bool>? ActivityChanged;

    public AudioBusState GetBus(AudioBus bus) => buses[bus];
    public void SetBusVolume(AudioBus bus, float volume) => SetBus(bus, buses[bus] with { Volume = volume });
    public void SetMuted(AudioBus bus, bool muted) => SetBus(bus, buses[bus] with { Muted = muted });

    public AudioVoice Play(AudioClip clip, AudioBus bus, float volume = 1, bool loop = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (bus == AudioBus.Master) throw new ArgumentException("Sounds must route through a semantic bus beneath Master.", nameof(bus));
        var voice = new AudioVoice(this, backend.StartPlayback(clip, loop), clip, bus, Math.Clamp(volume, 0, 1), loop);
        voices.Add(voice); voice.ApplyGain(); ActivityChanged?.Invoke(true); return voice;
    }

    public void Update(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        foreach (AudioVoice voice in voices.ToArray())
        {
            voice.Update(delta);
            if (voice.IsStopped) voices.Remove(voice);
        }
    }

    internal float EffectiveGain(AudioBus bus, float voiceVolume)
    {
        AudioBusState master = buses[AudioBus.Master], routed = buses[bus];
        return master.Muted || routed.Muted ? 0 : Math.Clamp(voiceVolume * routed.Volume * master.Volume, 0, 1);
    }

    internal void Remove(AudioVoice voice) { if (voices.Remove(voice)) ActivityChanged?.Invoke(voices.Count > 0); }
    private void SetBus(AudioBus bus, AudioBusState state)
    {
        state = state.Normalize();
        if (buses[bus] == state) return;
        buses[bus] = state;
        foreach (AudioVoice voice in voices) voice.ApplyGain();
        BusChanged?.Invoke(bus, state);
    }

    public void Dispose()
    {
        if (disposed) return;
        foreach (AudioVoice voice in voices.ToArray()) voice.Dispose();
        backend.Dispose(); disposed = true;
    }
}

public sealed class AudioVoice : IDisposable
{
    private readonly AudioEngine engine;
    private readonly IBackendAudioVoice? backend;
    private float volume;
    private float? fadeStart, fadeTarget;
    private TimeSpan fadeElapsed, fadeDuration;
    private TimeSpan playbackElapsed;
    private bool stopAfterFade;
    private bool stopped;

    internal AudioVoice(AudioEngine engine, IBackendAudioVoice? backend, AudioClip clip, AudioBus bus, float volume, bool loop)
    { this.engine = engine; this.backend = backend; Clip = clip; Bus = bus; this.volume = volume; Loop = loop; }

    public AudioClip Clip { get; }
    public AudioBus Bus { get; }
    public bool Loop { get; }
    public bool IsStopped => stopped;
    public float Volume => volume;
    public float EffectiveVolume => engine.EffectiveGain(Bus, volume);
    public void SetVolume(float value) { volume = Math.Clamp(value, 0, 1); fadeTarget = null; ApplyGain(); }
    public void Pause() => backend?.Pause();
    public void Resume() => backend?.ResumePlayback();
    public void Stop() { if (stopped) return; stopped = true; backend?.StopPlayback(); backend?.Dispose(); engine.Remove(this); }
    public void Fade(float target, TimeSpan duration, bool stopWhenSilent = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);
        fadeStart = volume; fadeTarget = Math.Clamp(target, 0, 1); fadeDuration = duration; fadeElapsed = TimeSpan.Zero;
        stopAfterFade = stopWhenSilent && fadeTarget == 0;
        if (duration == TimeSpan.Zero) CompleteFade();
    }
    internal void Update(TimeSpan delta)
    {
        if (stopped) return;
        playbackElapsed += delta;
        if (!Loop && playbackElapsed >= Clip.Duration && (backend is null || !backend.IsPlaying)) { Stop(); return; }
        if (fadeTarget is null || fadeStart is null) return;
        fadeElapsed += delta;
        float amount = fadeDuration == TimeSpan.Zero ? 1 : (float)Math.Clamp(fadeElapsed.TotalSeconds / fadeDuration.TotalSeconds, 0, 1);
        volume = fadeStart.Value + (fadeTarget.Value - fadeStart.Value) * amount; ApplyGain();
        if (amount >= 1) CompleteFade();
    }
    internal void ApplyGain() => backend?.SetGain(EffectiveVolume);
    private void CompleteFade() { if (fadeTarget is float target) volume = target; fadeStart = fadeTarget = null; ApplyGain(); if (stopAfterFade) Stop(); stopAfterFade = false; }
    public void Dispose() => Stop();
}

public sealed class MusicService(AudioEngine engine)
{
    public AudioVoice? Current { get; private set; }
    public AudioVoice Play(AudioClip clip, bool loop = true)
    {
        Current?.Stop();
        return Current = engine.Play(clip, AudioBus.Music, loop: loop);
    }
    public AudioVoice CrossfadeTo(AudioClip clip, TimeSpan duration, bool loop = true)
    {
        AudioVoice? outgoing = Current;
        AudioVoice incoming = engine.Play(clip, AudioBus.Music, 0, loop);
        incoming.Fade(1, duration); outgoing?.Fade(0, duration, stopWhenSilent: true); Current = incoming; return incoming;
    }
    public void Stop(TimeSpan? fade = null) { if (fade is { } duration) Current?.Fade(0, duration); else Current?.Stop(); Current = null; }
}
