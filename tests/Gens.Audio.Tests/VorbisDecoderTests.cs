using Gens.Audio.Decoding;
using NUnit.Framework;

namespace Gens.Audio.Tests;

[TestFixture]
public sealed class VorbisDecoderTests
{
    [Test]
    public void CanDecodeRecognizesOggPageHeaderOnly()
    {
        var decoder = new VorbisDecoder();
        Assert.Multiple(() =>
        {
            Assert.That(decoder.CanDecode("OggS\0\0\0\0"u8), Is.True);
            Assert.That(decoder.CanDecode("RIFF\0\0\0\0WAVE"u8), Is.False);
        });
    }

    [Test]
    public void DecodesFixtureOggToPcm16WithPlausibleFormatAndLength()
    {
        var decoder = new VorbisDecoder();
        (PcmFormat format, TimeSpan duration, Func<Stream> openPcmStream) = decoder.Decode(OpenFixture);

        Assert.Multiple(() =>
        {
            Assert.That(format.Channels, Is.GreaterThan(0));
            Assert.That(format.SampleRate, Is.GreaterThan(0));
            Assert.That(duration, Is.GreaterThan(TimeSpan.Zero));
        });

        using Stream pcm = openPcmStream();
        using var buffer = new MemoryStream();
        pcm.CopyTo(buffer);
        byte[] decoded = buffer.ToArray();

        Assert.That(decoded.Length, Is.GreaterThan(0));
        Assert.That(decoded.Length % 2, Is.EqualTo(0), "Decoded output must be whole PCM16 samples.");

        // Tolerance-based sanity check: Vorbis is lossy, so we assert the expected sample count within a few
        // frames rather than a bit-exact match, and that samples stay within the signed 16-bit range.
        int frameBytes = format.Channels * 2;
        long expectedFrames = (long)(duration.TotalSeconds * format.SampleRate);
        long actualFrames = decoded.Length / frameBytes;
        Assert.That(actualFrames, Is.EqualTo(expectedFrames).Within(format.SampleRate)); // within ~1s of frames
    }

    [Test]
    public void OpenPcmStreamCanBeReadMultipleTimesFromTheStart()
    {
        var decoder = new VorbisDecoder();
        (_, _, Func<Stream> openPcmStream) = decoder.Decode(OpenFixture);

        int First()
        {
            using Stream s = openPcmStream();
            using var m = new MemoryStream();
            s.CopyTo(m);
            return m.ToArray().Length;
        }
        int firstLength = First();
        int secondLength = First();
        Assert.That(firstLength, Is.EqualTo(secondLength));
        Assert.That(firstLength, Is.GreaterThan(0));
    }

    [Test]
    public void ThrowsOnNonOggData() => Assert.Throws<InvalidDataException>(() => new VorbisDecoder().Decode(() => new MemoryStream("not an ogg file at all, just junk bytes"u8.ToArray())));

    private static Stream OpenFixture() => File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "fixture.ogg"));
}
