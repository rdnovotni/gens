using Gens.Audio.Decoding;
using NUnit.Framework;

namespace Gens.Audio.Tests;

[TestFixture]
public sealed class AudioAssetLoaderTests
{
    [Test]
    public void SniffsWavByMagicBytesRegardlessOfExtension()
    {
        var loader = new AudioAssetLoader();
        AudioClip clip = loader.LoadClip("wav-clip", () => File.OpenRead(FixturePath("tone.wav")), AudioStorage.Buffered);
        Assert.Multiple(() =>
        {
            Assert.That(clip.Channels, Is.EqualTo(1));
            Assert.That(clip.SampleRate, Is.EqualTo(48_000));
        });
        using Stream pcm = clip.OpenStream();
        Assert.That(pcm.Length, Is.GreaterThan(0));
    }

    [Test]
    public void SniffsOggByMagicBytesRegardlessOfExtension()
    {
        var loader = new AudioAssetLoader();
        AudioClip clip = loader.LoadClip("ogg-clip", () => File.OpenRead(FixturePath("fixture.ogg")), AudioStorage.Buffered);
        Assert.That(clip.Channels, Is.GreaterThan(0));
        Assert.That(clip.SampleRate, Is.GreaterThan(0));
    }

    [Test]
    public void ThrowsInvalidDataExceptionForUnrecognizedContainer()
    {
        var loader = new AudioAssetLoader();
        Assert.Throws<InvalidDataException>(() => loader.LoadClip("bad-clip", () => new MemoryStream("not audio"u8.ToArray()), AudioStorage.Buffered));
    }

    [Test]
    public void BufferedStorageDecodesOnceAndServesRepeatedReadsFromMemory()
    {
        int opens = 0;
        var loader = new AudioAssetLoader();
        AudioClip clip = loader.LoadClip("buffered", () => { opens++; return File.OpenRead(FixturePath("tone.wav")); }, AudioStorage.Buffered);
        int opensAfterLoad = opens;

        using (Stream first = clip.OpenStream()) using (var m = new MemoryStream()) first.CopyTo(m);
        using (Stream second = clip.OpenStream()) using (var m = new MemoryStream()) second.CopyTo(m);

        // Buffered clips decode the source exactly once at load time; repeated OpenStream calls must not re-open the source file.
        Assert.That(opens, Is.EqualTo(opensAfterLoad));
    }

    [Test]
    public void StreamingStorageReDecodesFromTheSourceOnEachOpenStreamCall()
    {
        int opens = 0;
        var loader = new AudioAssetLoader();
        AudioClip clip = loader.LoadClip("streaming", () => { opens++; return File.OpenRead(FixturePath("tone.wav")); }, AudioStorage.Streaming);
        int opensAfterLoad = opens;

        using (Stream first = clip.OpenStream()) using (var m = new MemoryStream()) first.CopyTo(m);
        using (Stream second = clip.OpenStream()) using (var m = new MemoryStream()) second.CopyTo(m);

        Assert.That(opens, Is.GreaterThan(opensAfterLoad));
    }

    private static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
}
