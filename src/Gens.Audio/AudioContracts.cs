namespace Gens.Audio;

public enum AudioBus { Master, Music, Ambience, Effects, UI, Voice }
public enum AudioStorage { Buffered, Streaming }

public sealed class AudioClip
{
    public AudioClip(string id, TimeSpan duration, int channels, int sampleRate, AudioStorage storage, Func<Stream> openStream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(channels, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sampleRate, 0);
        ArgumentNullException.ThrowIfNull(openStream);
        Id = id; Duration = duration; Channels = channels; SampleRate = sampleRate; Storage = storage; OpenStream = openStream;
    }
    public string Id { get; }
    public TimeSpan Duration { get; }
    public int Channels { get; }
    public int SampleRate { get; }
    public AudioStorage Storage { get; }
    public Func<Stream> OpenStream { get; }
}

public interface IAudioBackend : IDisposable
{
    string Name { get; }
    bool IsAvailable { get; }
    string? UnavailableReason { get; }
    IBackendAudioVoice? StartPlayback(AudioClip clip, bool shouldLoop);
}

public interface IBackendAudioVoice : IDisposable
{
    bool IsPlaying { get; }
    void SetGain(float gain);
    void Pause();
    void ResumePlayback();
    void StopPlayback();
}

public sealed class NullAudioBackend(string? reason = null) : IAudioBackend
{
    public string Name => "Null audio";
    public bool IsAvailable => false;
    public string? UnavailableReason { get; } = reason ?? "No audio output device is available.";
    public IBackendAudioVoice? StartPlayback(AudioClip clip, bool shouldLoop) => null;
    public void Dispose() { }
}
