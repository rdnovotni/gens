using Gens.Audio;
using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class AudioAssetSourceTests
{
    private string root = null!;

    [SetUp]
    public void SetUp()
    {
        root = Path.Combine(Path.GetTempPath(), "gens-audio-asset-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
    }

    [TearDown]
    public void TearDown() { if (Directory.Exists(root)) Directory.Delete(root, true); }

    [Test]
    public void LoadsAWavFileFromUnderTheAssetRoot()
    {
        File.WriteAllBytes(Path.Combine(root, "cue.wav"), BuildMinimalWav());
        var source = new AudioAssetSource(root);

        AudioClip clip = source.Load("cue", "cue.wav");

        Assert.Multiple(() =>
        {
            Assert.That(clip.Channels, Is.EqualTo(1));
            Assert.That(clip.SampleRate, Is.EqualTo(8_000));
        });
    }

    [TestCase("../escape.wav")]
    [TestCase("..\\escape.wav")]
    [TestCase("nested/../../escape.wav")]
    public void RejectsRelativePathsThatEscapeTheAssetRoot(string escapePath)
    {
        var source = new AudioAssetSource(root);
        Assert.Throws<InvalidOperationException>(() => source.Load("escape", escapePath));
    }

    [Test]
    public void ForcedNoAudioDeviceLetsTheDesktopControllerStartAndShutDownCleanly()
    {
        var paths = new DesktopApplicationPaths(Path.Combine(root, "userdata"));
        var audio = new AudioEngine(new NullAudioBackend("No audio output device is available."));

        DesktopApplicationController controller = null!;
        Assert.DoesNotThrow(() => controller = new DesktopApplicationController(paths, audio));
        Assert.That(controller.Audio.IsOutputAvailable, Is.False);

        // A forced-no-device backend must never crash or hang playback/settings plumbing.
        Assert.DoesNotThrow(() => controller.SetAudioVolume(AudioBus.Music, 0.5f));
        Assert.DoesNotThrow(() => controller.RequestQuit());
    }

    private static byte[] BuildMinimalWav()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        const int sampleRate = 8_000;
        const int channels = 1;
        const short bitsPerSample = 16;
        byte[] data = [0, 0, 1, 0, 2, 0]; // three PCM16 samples
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * (bitsPerSample / 8));
        writer.Write((short)(channels * (bitsPerSample / 8)));
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(data.Length);
        writer.Write(data);
        return stream.ToArray();
    }
}
