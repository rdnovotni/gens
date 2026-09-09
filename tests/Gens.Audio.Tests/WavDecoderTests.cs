using Gens.Audio.Decoding;
using NUnit.Framework;

namespace Gens.Audio.Tests;

[TestFixture]
public sealed class WavDecoderTests
{
    [Test]
    public void CanDecodeRecognizesRiffWaveHeaderOnly()
    {
        var decoder = new WavDecoder();
        Assert.Multiple(() =>
        {
            Assert.That(decoder.CanDecode("RIFF\0\0\0\0WAVE"u8), Is.True);
            Assert.That(decoder.CanDecode("OggS\0\0\0\0\0\0\0\0"u8), Is.False);
            Assert.That(decoder.CanDecode("RIFF"u8), Is.False);
        });
    }

    [Test]
    public void DecodesFixtureToleWavAsPassthroughPcm16()
    {
        var decoder = new WavDecoder();
        (PcmFormat format, TimeSpan duration, Func<Stream> openPcmStream) = decoder.Decode(OpenFixture);

        Assert.Multiple(() =>
        {
            Assert.That(format.Channels, Is.EqualTo(1));
            Assert.That(format.SampleRate, Is.EqualTo(48_000));
            Assert.That(duration, Is.EqualTo(TimeSpan.FromSeconds(0.1)).Within(TimeSpan.FromMilliseconds(1)));
        });

        using Stream pcm = openPcmStream();
        using var buffer = new MemoryStream();
        pcm.CopyTo(buffer);
        byte[] decoded = buffer.ToArray();

        // The fixture is already 16-bit PCM, so the decoder must pass its sample bytes through unchanged,
        // starting exactly at the canonical 44-byte WAV header (RIFF+fmt+data headers, no extra chunks).
        using Stream raw = OpenFixture();
        using var rawBuffer = new MemoryStream();
        raw.CopyTo(rawBuffer);
        byte[] rawBytes = rawBuffer.ToArray();
        Assert.That(decoded.Length, Is.EqualTo(rawBytes.Length - 44));
        byte[] expectedData = rawBytes[44..];
        Assert.That(decoded, Is.EqualTo(expectedData));
    }

    [Test]
    public void OpenPcmStreamCanBeReadMultipleTimesFromTheStart()
    {
        var decoder = new WavDecoder();
        (_, _, Func<Stream> openPcmStream) = decoder.Decode(OpenFixture);

        byte[] First() { using Stream s = openPcmStream(); using var m = new MemoryStream(); s.CopyTo(m); return m.ToArray(); }
        Assert.That(First(), Is.EqualTo(First()));
    }

    [Test]
    public void ConvertsUnsignedEightBitPcmToSigned16()
    {
        byte[] wav = BuildWav(bitsPerSample: 8, audioFormat: 1, samples: [0, 128, 255]);
        var decoder = new WavDecoder();
        (_, _, Func<Stream> openPcmStream) = decoder.Decode(() => new MemoryStream(wav));

        short[] decoded = ReadInt16Samples(openPcmStream());
        Assert.Multiple(() =>
        {
            Assert.That(decoded[0], Is.EqualTo((short)((0 - 128) * 256)));
            Assert.That(decoded[1], Is.EqualTo((short)((128 - 128) * 256)));
            Assert.That(decoded[2], Is.EqualTo((short)((255 - 128) * 256)));
        });
    }

    [Test]
    public void ThrowsOnTruncatedHeader() => Assert.Throws<InvalidDataException>(() => new WavDecoder().Decode(() => new MemoryStream("RIFF"u8.ToArray())));

    [Test]
    public void ThrowsOnUnsupportedAudioFormatCode()
    {
        byte[] wav = BuildWavHeaderOnly(audioFormat: 99, bitsPerSample: 16, channels: 1, sampleRate: 48_000, dataLength: 0);
        Assert.Throws<InvalidDataException>(() => new WavDecoder().Decode(() => new MemoryStream(wav)));
    }

    [Test]
    public void ThrowsWhenNotARiffStream() => Assert.Throws<InvalidDataException>(() => new WavDecoder().Decode(() => new MemoryStream("junk data here"u8.ToArray())));

    private static Stream OpenFixture() => File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "tone.wav"));

    private static short[] ReadInt16Samples(Stream pcm)
    {
        using var buffer = new MemoryStream();
        pcm.CopyTo(buffer);
        byte[] bytes = buffer.ToArray();
        short[] samples = new short[bytes.Length / 2];
        for (int i = 0; i < samples.Length; i++) samples[i] = BitConverter.ToInt16(bytes, i * 2);
        return samples;
    }

    private static byte[] BuildWav(int bitsPerSample, int audioFormat, byte[] samples)
    {
        byte[] header = BuildWavHeaderOnly(audioFormat, bitsPerSample, channels: 1, sampleRate: 8_000, dataLength: samples.Length);
        return [.. header, .. samples];
    }

    private static byte[] BuildWavHeaderOnly(int audioFormat, int bitsPerSample, int channels, int sampleRate, int dataLength)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        short blockAlign = (short)(channels * (bitsPerSample / 8));

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataLength);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)audioFormat);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write((short)bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(dataLength);
        return stream.ToArray();
    }
}
