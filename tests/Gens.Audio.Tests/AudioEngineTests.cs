using Gens.Audio;
using NUnit.Framework;

namespace Gens.Audio.Tests;

[TestFixture]
public sealed class AudioEngineTests
{
    [Test]
    public void MixerAppliesVoiceBusMasterAndMuteWithoutLosingVolumes()
    {
        using var backend = new FakeBackend(); using var engine = new AudioEngine(backend);
        AudioVoice voice = engine.Play(Clip("effect"), AudioBus.Effects, .5f);
        engine.SetBusVolume(AudioBus.Effects, .8f); engine.SetBusVolume(AudioBus.Master, .5f);
        Assert.That(voice.EffectiveVolume, Is.EqualTo(.2f).Within(.0001));
        engine.SetMuted(AudioBus.Effects, true); Assert.That(voice.EffectiveVolume, Is.Zero);
        engine.SetMuted(AudioBus.Effects, false); Assert.That(voice.EffectiveVolume, Is.EqualTo(.2f).Within(.0001));
    }

    [Test]
    public void FadeAndMusicCrossfadeUsePresentationTime()
    {
        using var engine = new AudioEngine(new FakeBackend()); var music = new MusicService(engine);
        AudioVoice first = music.Play(Clip("first", AudioStorage.Streaming)); AudioVoice second = music.CrossfadeTo(Clip("second", AudioStorage.Streaming), TimeSpan.FromSeconds(2));
        engine.Update(TimeSpan.FromSeconds(1));
        Assert.Multiple(() => { Assert.That(first.Volume, Is.EqualTo(.5f).Within(.001)); Assert.That(second.Volume, Is.EqualTo(.5f).Within(.001)); });
        engine.Update(TimeSpan.FromSeconds(1)); Assert.That(first.IsStopped, Is.True);
    }

    [Test] public void MissingDeviceContinuesSilently() { using var engine = new AudioEngine(new NullAudioBackend()); AudioVoice voice = engine.Play(Clip("ui"), AudioBus.UI); Assert.Multiple(() => { Assert.That(engine.IsOutputAvailable, Is.False); Assert.That(voice.IsStopped, Is.False); }); }
    private static AudioClip Clip(string id, AudioStorage storage = AudioStorage.Buffered) => new(id, TimeSpan.FromSeconds(1), 2, 48_000, storage, static () => new MemoryStream([0]));
    private sealed class FakeBackend : IAudioBackend
    {
        public string Name => "Fake"; public bool IsAvailable => true; public string? UnavailableReason => null;
        public IBackendAudioVoice StartPlayback(AudioClip clip, bool shouldLoop) => new FakeVoice(); public void Dispose() { }
    }
    private sealed class FakeVoice : IBackendAudioVoice { public bool IsPlaying { get; private set; } = true; public void SetGain(float gain) { } public void Pause() => IsPlaying = false; public void ResumePlayback() => IsPlaying = true; public void StopPlayback() => IsPlaying = false; public void Dispose() => StopPlayback(); }
}
